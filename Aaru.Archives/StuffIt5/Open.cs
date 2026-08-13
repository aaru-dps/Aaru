using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class StuffIt5
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < MIN_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        _encoding        = encoding ?? Encoding.GetEncoding("macintosh");
        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        long baseOffset = _stream.Position;

        // Skip the 82-byte header text
        _stream.Position = baseOffset + 82;

        int version = _stream.ReadByte();
        int flags   = _stream.ReadByte();

        if(version != ARCHIVE_VERSION) return ErrorNumber.InvalidArgument;

        var archiveHdr = new byte[16];
        _stream.ReadExactly(archiveHdr, 0, 16);

        // uint totalSize  = BigEndianBitConverter.ToUInt32(archiveHdr, 0);
        // uint something  = BigEndianBitConverter.ToUInt32(archiveHdr, 4);
        var numFiles  = BigEndianBitConverter.ToUInt16(archiveHdr, 8);
        var firstOffs = BigEndianBitConverter.ToUInt32(archiveHdr, 10);

        // ushort headerCrc = BigEndianBitConverter.ToUInt16(archiveHdr, 14);

        if((flags & ARCHIVE_FLAGS_14BYTES) != 0) _stream.Position += 14;

        var commentSize = 0;
        var lengthB     = 0;

        if((flags & ARCHIVE_FLAGS_20) != 0)
        {
            var buf = new byte[4];
            _stream.ReadExactly(buf, 0, 4);
            commentSize = BigEndianBitConverter.ToUInt16(buf, 0);
            lengthB     = BigEndianBitConverter.ToUInt16(buf, 2);
        }

        if((flags & ARCHIVE_FLAGS_CRYPTED) != 0)
        {
            int hashSize = _stream.ReadByte();

            if(hashSize != KEY_LENGTH) return ErrorNumber.InvalidArgument;

            // Skip the hash — encryption not supported
            _stream.Position += hashSize;
        }

        if((flags & ARCHIVE_FLAGS_40) != 0)
        {
            var lenBuf = new byte[2];
            _stream.ReadExactly(lenBuf, 0, 2);
            int lengthN = BigEndianBitConverter.ToUInt16(lenBuf, 0);

            // Each entry: 4 uint32s + 5 int16s + 2 uint8s + 3 uint16s = 30 bytes
            for(var i = 0; i < lengthN; i++) _stream.Position += 30;
        }

        if((flags & ARCHIVE_FLAGS_20) != 0)
        {
            if(commentSize > 0)
            {
                var commentBytes = new byte[commentSize];
                _stream.ReadExactly(commentBytes, 0, commentSize);
                _archiveComment = _encoding.GetString(commentBytes);
            }

            _stream.Position += lengthB;
        }

        _stream.Position = firstOffs + baseOffset;

        _entries = [];

        var dirs = new Dictionary<long, string>();

        int numEntries = numFiles;

        for(var i = 0; i < numEntries; i++)
        {
            if(_stream.Position + 34 > _stream.Length) break;

            long entryOffset = _stream.Position;

            var idBuf = new byte[4];
            _stream.ReadExactly(idBuf, 0, 4);
            var headId = BigEndianBitConverter.ToUInt32(idBuf, 0);

            if(headId != ENTRY_ID) return ErrorNumber.InvalidArgument;

            int entryVersion = _stream.ReadByte();
            _stream.Position++; // skip unknown byte

            var hdrSizeBuf = new byte[2];
            _stream.ReadExactly(hdrSizeBuf, 0, 2);
            var headerSize = BigEndianBitConverter.ToUInt16(hdrSizeBuf, 0);

            long headerEnd = entryOffset + headerSize;

            // The header, including name and comment, must fit in the file
            if(headerEnd > _stream.Length) return ErrorNumber.InvalidArgument;

            _stream.Position++; // skip system ID byte

            int entryFlags = _stream.ReadByte();

            var dateBuf = new byte[16];
            _stream.ReadExactly(dateBuf, 0, 16);

            var creationDate     = BigEndianBitConverter.ToUInt32(dateBuf, 0);
            var modificationDate = BigEndianBitConverter.ToUInt32(dateBuf, 4);

            // uint prevOffs       = BigEndianBitConverter.ToUInt32(dateBuf, 8);
            // uint nextOffs       = BigEndianBitConverter.ToUInt32(dateBuf, 12);

            var dirOffsBuf = new byte[4];
            _stream.ReadExactly(dirOffsBuf, 0, 4);
            var dirOffs = BigEndianBitConverter.ToUInt32(dirOffsBuf, 0);

            var nameLenBuf = new byte[2];
            _stream.ReadExactly(nameLenBuf, 0, 2);
            var nameLength = BigEndianBitConverter.ToUInt16(nameLenBuf, 0);

            // Skip header CRC
            _stream.Position += 2;

            var sizeBuf = new byte[8];
            _stream.ReadExactly(sizeBuf, 0, 8);
            var dataLength  = BigEndianBitConverter.ToUInt32(sizeBuf, 0);
            var dataCompLen = BigEndianBitConverter.ToUInt32(sizeBuf, 4);

            var crcBuf = new byte[4];
            _stream.ReadExactly(crcBuf, 0, 4);
            var dataCrc = BigEndianBitConverter.ToUInt16(crcBuf, 0);

            // skip 2 unknown bytes

            CompressionMethod dataMethod  = CompressionMethod.None;
            var               numDirFiles = 0;
            bool              encrypted   = (entryFlags & FLAGS_ENCRYPTED) != 0;

            if((entryFlags & FLAGS_DIRECTORY) != 0)
            {
                // Directory entry — next 2 bytes are numfiles
                var numFilesBuf = new byte[2];
                _stream.ReadExactly(numFilesBuf, 0, 2);
                numDirFiles = BigEndianBitConverter.ToUInt16(numFilesBuf, 0);

                // Sentinel entry check (datalength == 0xffffffff)
                if(dataLength == 0xFFFFFFFF)
                {
                    numEntries++;

                    continue;
                }
            }
            else
            {
                // File entry
                dataMethod = (CompressionMethod)_stream.ReadByte();

                int passLen = _stream.ReadByte();

                if(encrypted && dataLength > 0)
                {
                    if(passLen != KEY_LENGTH) return ErrorNumber.InvalidArgument;

                    // Skip entry key — encryption not supported
                    _stream.Position += passLen;
                }
                else if(passLen > 0)
                {
                    // Unexpected password data
                    _stream.Position += passLen;
                }
            }

            // Read filename
            if(nameLength > _stream.Length - _stream.Position) return ErrorNumber.InvalidArgument;

            var nameData = new byte[nameLength];
            _stream.ReadExactly(nameData, 0, nameLength);
            string name = _encoding.GetString(nameData);

            // Read optional comment
            string entryComment = null;

            if(_stream.Position < headerEnd)
            {
                var commentBuf = new byte[4];
                _stream.ReadExactly(commentBuf, 0, 4);
                int entryCommentSize = BigEndianBitConverter.ToUInt16(commentBuf, 0);

                // skip 2 unknown bytes

                if(entryCommentSize > 0)
                {
                    if(entryCommentSize > _stream.Length - _stream.Position) return ErrorNumber.InvalidArgument;

                    var commentData = new byte[entryCommentSize];
                    _stream.ReadExactly(commentData, 0, entryCommentSize);
                    entryComment = _encoding.GetString(commentData);
                }
            }

            // Read second block: finder info and Mac metadata
            if(_stream.Position + 36 > _stream.Length) break;

            var finderBuf = new byte[4];
            _stream.ReadExactly(finderBuf, 0, 4);
            var something = BigEndianBitConverter.ToUInt16(finderBuf, 0);

            // skip 2 unknown bytes

            var typeBuf = new byte[10];
            _stream.ReadExactly(typeBuf, 0, 10);
            var fileType    = BigEndianBitConverter.ToUInt32(typeBuf, 0);
            var fileCreator = BigEndianBitConverter.ToUInt32(typeBuf, 4);
            var finderFlags = BigEndianBitConverter.ToUInt16(typeBuf, 8);

            // Version-dependent skip
            if(entryVersion == 1)
                _stream.Position += 22;
            else
                _stream.Position += 18;

            // Resource fork info
            uint              resourceLength  = 0;
            uint              resourceCompLen = 0;
            ushort            resourceCrc     = 0;
            CompressionMethod resourceMethod  = CompressionMethod.None;
            bool              hasResource     = (something & 0x01) != 0;

            if(hasResource)
            {
                if(_stream.Position + 14 > _stream.Length) break;

                var rsrcBuf = new byte[12];
                _stream.ReadExactly(rsrcBuf, 0, 12);
                resourceLength  = BigEndianBitConverter.ToUInt32(rsrcBuf, 0);
                resourceCompLen = BigEndianBitConverter.ToUInt32(rsrcBuf, 4);
                resourceCrc     = BigEndianBitConverter.ToUInt16(rsrcBuf, 8);

                // skip 2 unknown bytes at rsrcBuf[10..11]

                resourceMethod = (CompressionMethod)_stream.ReadByte();

                int passLen = _stream.ReadByte();

                if(encrypted && resourceLength > 0)
                {
                    if(passLen != KEY_LENGTH) return ErrorNumber.InvalidArgument;

                    // Skip resource entry key
                    _stream.Position += passLen;
                }
                else if(passLen > 0) _stream.Position += passLen;
            }

            long dataStart = _stream.Position;

            // Resolve parent directory path
            var parentPath = "";

            if(dirs.TryGetValue(dirOffs, out string dirPath)) parentPath = dirPath;

            string fullPath = parentPath.Length > 0 ? parentPath + "/" + name : name;

            if((entryFlags & FLAGS_DIRECTORY) != 0)
            {
                // Register directory
                dirs[entryOffset] = fullPath;

                var dirEntry = new Entry
                {
                    Filename         = name,
                    DirectoryPath    = parentPath.Length > 0 ? parentPath : null,
                    IsDirectory      = true,
                    Encrypted        = encrypted,
                    Comment          = entryComment,
                    FinderFlags      = finderFlags,
                    FileType         = fileType,
                    Creator          = fileCreator,
                    CreationTime     = DateHandlers.MacToDateTime(creationDate),
                    ModificationTime = DateHandlers.MacToDateTime(modificationDate)
                };

                _entries.Add(dirEntry);

                // The directory's child entries will be parsed in subsequent iterations
                _stream.Position =  dataStart;
                numEntries       += numDirFiles;
            }
            else
            {
                // File entry
                var entry = new Entry
                {
                    Filename                 = name,
                    DirectoryPath            = parentPath.Length > 0 ? parentPath : null,
                    IsDirectory              = false,
                    Encrypted                = encrypted,
                    Comment                  = entryComment,
                    DataOffset               = dataStart + resourceCompLen,
                    DataCompressedSize       = dataCompLen,
                    DataUncompressedSize     = dataLength,
                    DataMethod               = dataMethod,
                    DataCrc16                = dataCrc,
                    ResourceOffset           = dataStart,
                    ResourceCompressedSize   = resourceCompLen,
                    ResourceUncompressedSize = resourceLength,
                    ResourceMethod           = resourceMethod,
                    ResourceCrc16            = resourceCrc,
                    FileType                 = fileType,
                    Creator                  = fileCreator,
                    FinderFlags              = finderFlags,
                    CreationTime             = DateHandlers.MacToDateTime(creationDate),
                    ModificationTime         = DateHandlers.MacToDateTime(modificationDate)
                };

                _entries.Add(entry);

                _stream.Position = dataStart + resourceCompLen + dataCompLen;
            }
        }

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        var hasCompressionFlag = false;
        var hasSubdirectories  = false;
        var hasDirectories     = false;
        var hasXAttrs          = false;
        var hasProtection      = false;

        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            Entry entry = _entries[i];

            if(entry.IsDirectory) hasDirectories = true;

            if(entry.DataMethod != CompressionMethod.None || entry.ResourceMethod != CompressionMethod.None)
                hasCompressionFlag = true;

            if(entry.DirectoryPath is not null) hasSubdirectories = true;

            if(entry.Encrypted) hasProtection = true;

            if(!entry.IsDirectory && (entry.ResourceUncompressedSize > 0 || entry.FileType != 0 || entry.Creator != 0))
                hasXAttrs = true;

            if(entry.Comment is not null) hasXAttrs = true;
        }

        if(_archiveComment is not null) hasXAttrs = true;

        if(hasCompressionFlag) _features |= ArchiveSupportedFeature.SupportsCompression;
        if(hasSubdirectories) _features  |= ArchiveSupportedFeature.SupportsSubdirectories;
        if(hasDirectories) _features     |= ArchiveSupportedFeature.HasExplicitDirectories;
        if(hasXAttrs) _features          |= ArchiveSupportedFeature.SupportsXAttrs;
        if(hasProtection) _features      |= ArchiveSupportedFeature.SupportsProtection;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();

        _stream         = null;
        _entries        = null;
        _archiveComment = null;
        Opened          = false;
    }

#endregion
}