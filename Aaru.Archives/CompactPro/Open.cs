using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class CompactPro
{
    void ParseDirectory(string parentPath, int numEntries, int depth)
    {
        // Cap recursion depth so a crafted directory tree cannot overflow the stack
        if(depth > MAX_DIRECTORY_DEPTH) return;

        while(numEntries > 0)
        {
            int namelenByte = _stream.ReadByte();

            if(namelenByte < 0) break;

            int  nameLen = namelenByte & NAME_LENGTH_MASK;
            bool isDir   = (namelenByte & NAME_DIRECTORY_FLAG) != 0;

            var name = "";

            if(nameLen > 0)
            {
                var nameBytes = new byte[nameLen];
                _stream.ReadExactly(nameBytes, 0, nameLen);
                name = _encoding.GetString(nameBytes);
            }

            if(isDir)
            {
                // Directory entry: 2 bytes for number of child entries
                var dirMeta = new byte[DIR_METADATA_SIZE];
                _stream.ReadExactly(dirMeta, 0, DIR_METADATA_SIZE);

                var numDirEntries = BigEndianBitConverter.ToUInt16(dirMeta, 0);

                string dirPath = parentPath.Length > 0 ? parentPath + "/" + name : name;

                var dirEntry = new Entry
                {
                    Filename      = name,
                    DirectoryPath = parentPath.Length > 0 ? parentPath : null,
                    IsDirectory   = true
                };

                _entries.Add(dirEntry);

                ParseDirectory(dirPath, numDirEntries, depth + 1);

                // The directory entry plus its children count as numDirEntries + 1
                numEntries -= numDirEntries + 1;
            }
            else
            {
                // File entry: 45 bytes of metadata
                var fileMeta = new byte[FILE_METADATA_SIZE];
                _stream.ReadExactly(fileMeta, 0, FILE_METADATA_SIZE);

                // Parse fields (all big-endian)
                // byte volume        = fileMeta[0];
                var fileOffset       = BigEndianBitConverter.ToUInt32(fileMeta, 1);
                var fileType         = BigEndianBitConverter.ToUInt32(fileMeta, 5);
                var creator          = BigEndianBitConverter.ToUInt32(fileMeta, 9);
                var creationDate     = BigEndianBitConverter.ToUInt32(fileMeta, 13);
                var modificationDate = BigEndianBitConverter.ToUInt32(fileMeta, 17);
                var finderFlags      = BigEndianBitConverter.ToUInt16(fileMeta, 21);
                var crc              = BigEndianBitConverter.ToUInt32(fileMeta, 23);
                var flags            = BigEndianBitConverter.ToUInt16(fileMeta, 27);
                var resourceLen      = BigEndianBitConverter.ToUInt32(fileMeta, 29);
                var dataLen          = BigEndianBitConverter.ToUInt32(fileMeta, 33);
                var resourceCompLen  = BigEndianBitConverter.ToUInt32(fileMeta, 37);
                var dataCompLen      = BigEndianBitConverter.ToUInt32(fileMeta, 41);

                var entry = new Entry
                {
                    Filename                 = name,
                    DirectoryPath            = parentPath.Length > 0 ? parentPath : null,
                    IsDirectory              = false,
                    DataOffset               = fileOffset + resourceCompLen,
                    DataCompressedSize       = dataCompLen,
                    DataUncompressedSize     = dataLen,
                    DataLzh                  = (flags & FLAG_DATA_LZH) != 0,
                    ResourceOffset           = fileOffset,
                    ResourceCompressedSize   = resourceCompLen,
                    ResourceUncompressedSize = resourceLen,
                    ResourceLzh              = (flags & FLAG_RESOURCE_LZH) != 0,
                    FileType                 = fileType,
                    Creator                  = creator,
                    FinderFlags              = finderFlags,
                    Crc32                    = crc,
                    Encrypted                = (flags & FLAG_ENCRYPTED) != 0,
                    CreationTime             = DateHandlers.MacToDateTime(creationDate),
                    ModificationTime         = DateHandlers.MacToDateTime(modificationDate)
                };

                _entries.Add(entry);

                numEntries--;
            }
        }
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < MIN_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        _encoding        = encoding ?? Encoding.GetEncoding("macintosh");
        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        var hdr = new byte[MIN_HEADER_SIZE];
        _stream.ReadExactly(hdr, 0, hdr.Length);

        if(hdr[0] != MARKER) return ErrorNumber.InvalidArgument;

        // byte 1 = volume, bytes 2-3 = xmagic (skip)
        var dirOffset = BigEndianBitConverter.ToUInt32(hdr, 4);

        if(dirOffset + 7 > (ulong)_stream.Length) return ErrorNumber.InvalidArgument;

        _stream.Position = dirOffset;

        // Read directory header: CRC(4) + numEntries(2) + commentLen(1)
        var dirHdr = new byte[7];
        _stream.ReadExactly(dirHdr, 0, 7);

        // Skip CRC (bytes 0-3)
        var  numEntries = BigEndianBitConverter.ToUInt16(dirHdr, 4);
        byte commentLen = dirHdr[6];

        if(commentLen > 0)
        {
            var commentBytes = new byte[commentLen];
            _stream.ReadExactly(commentBytes, 0, commentLen);
            _comment = _encoding.GetString(commentBytes);
        }

        _entries = [];

        try
        {
            ParseDirectory("", numEntries, 0);
        }
        catch(EndOfStreamException)
        {
            return ErrorNumber.InvalidArgument;
        }

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        var hasCompression    = false;
        var hasSubdirectories = false;
        var hasDirectories    = false;
        var hasXAttrs         = false;

        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            Entry entry = _entries[i];

            if(entry.IsDirectory) hasDirectories = true;

            if(entry.DataLzh || entry.ResourceLzh) hasCompression = true;

            if(entry.DirectoryPath is not null) hasSubdirectories = true;

            if(!entry.IsDirectory && (entry.ResourceUncompressedSize > 0 || entry.FileType != 0 || entry.Creator != 0))
                hasXAttrs = true;
        }

        if(_comment is not null) hasXAttrs = true;

        if(hasCompression) _features    |= ArchiveSupportedFeature.SupportsCompression;
        if(hasSubdirectories) _features |= ArchiveSupportedFeature.SupportsSubdirectories;
        if(hasDirectories) _features    |= ArchiveSupportedFeature.HasExplicitDirectories;
        if(hasXAttrs) _features         |= ArchiveSupportedFeature.SupportsXAttrs;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _entries = null;
        _comment = null;
        Opened   = false;
    }

#endregion
}