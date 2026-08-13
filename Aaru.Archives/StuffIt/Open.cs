using System;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class StuffIt
{
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

        var magic2 = BigEndianBitConverter.ToUInt32(hdr, 10);

        if(magic2 != MAGIC2) return ErrorNumber.InvalidArgument;

        // ushort numFiles = BigEndianBitConverter.ToUInt16(hdr, 4);
        var totalSize = BigEndianBitConverter.ToUInt32(hdr, 6);

        // Do not trust the declared archive size beyond the real file size
        long endOffset = Math.Min(totalSize, _stream.Length);

        _entries = [];

        var currentDir = "";

        while(_stream.Position + FILE_HEADER_SIZE <= endOffset)
        {
            var entryHeader = new byte[FILE_HEADER_SIZE];
            _stream.ReadExactly(entryHeader, 0, FILE_HEADER_SIZE);

            // Validate header CRC16 (IBM polynomial, unconditioned, over first 110 bytes)
            var storedCrc = BigEndianBitConverter.ToUInt16(entryHeader, HEADER_CRC_OFFSET);

            var crcData = new byte[HEADER_CRC_OFFSET];
            Array.Copy(entryHeader, 0, crcData, 0, HEADER_CRC_OFFSET);

            var crc16 = new CRC16IbmContext();
            crc16.Update(crcData);
            byte[] crcBytes    = crc16.Final();
            var    computedCrc = BigEndianBitConverter.ToUInt16(crcBytes, 0);

            if(storedCrc != computedCrc) return ErrorNumber.InvalidArgument;

            byte resourceMethod = entryHeader[0];
            byte dataMethod     = entryHeader[1];

            int nameLen = entryHeader[2];

            if(nameLen > 31) nameLen = 31;

            string name = _encoding.GetString(entryHeader, 3, nameLen);

            var rsrcLength   = BigEndianBitConverter.ToUInt32(entryHeader, 84);
            var dataLength   = BigEndianBitConverter.ToUInt32(entryHeader, 88);
            var rsrcCompLen  = BigEndianBitConverter.ToUInt32(entryHeader, 92);
            var dataCompLen  = BigEndianBitConverter.ToUInt32(entryHeader, 96);
            var fileType     = BigEndianBitConverter.ToUInt32(entryHeader, 66);
            var creator      = BigEndianBitConverter.ToUInt32(entryHeader, 70);
            var finderFlags  = BigEndianBitConverter.ToUInt16(entryHeader, 74);
            var creationDate = BigEndianBitConverter.ToUInt32(entryHeader, 76);
            var modDate      = BigEndianBitConverter.ToUInt32(entryHeader, 80);
            var rsrcCrc      = BigEndianBitConverter.ToUInt16(entryHeader, 100);
            var dataCrc      = BigEndianBitConverter.ToUInt16(entryHeader, 102);

            long dataStart = _stream.Position;

            var dataFolderBits     = (byte)(dataMethod     & FOLDER_MASK);
            var resourceFolderBits = (byte)(resourceMethod & FOLDER_MASK);

            if(dataFolderBits == START_FOLDER || resourceFolderBits == START_FOLDER)
            {
                // Folder start marker
                string path = currentDir.Length > 0 ? currentDir + "/" + name : name;

                var dirEntry = new Entry
                {
                    Filename      = name,
                    DirectoryPath = currentDir.Length > 0 ? currentDir : null,
                    IsDirectory   = true,
                    Encrypted = (dataMethod     & FOLDER_CONTAINS_ENCRYPTED) != 0 ||
                                (resourceMethod & FOLDER_CONTAINS_ENCRYPTED) != 0,
                    FinderFlags      = finderFlags,
                    CreationTime     = DateHandlers.MacToDateTime(creationDate),
                    ModificationTime = DateHandlers.MacToDateTime(modDate)
                };

                _entries.Add(dirEntry);

                currentDir = path;

                // No data follows a folder start marker; stream position stays at dataStart
                _stream.Position = dataStart;
            }
            else if(dataFolderBits == END_FOLDER || resourceFolderBits == END_FOLDER)
            {
                // Folder end marker
                int lastSlash = currentDir.LastIndexOf('/');
                currentDir = lastSlash >= 0 ? currentDir[..lastSlash] : "";
            }
            else
            {
                // Regular file entry
                bool dataEncrypted = (dataMethod     & ENCRYPTED_FLAG) != 0;
                bool rsrcEncrypted = (resourceMethod & ENCRYPTED_FLAG) != 0;
                bool encrypted     = dataEncrypted || rsrcEncrypted;

                long actualRsrcCompLen = rsrcCompLen;
                long actualDataCompLen = dataCompLen;

                if(rsrcEncrypted && rsrcCompLen >= 16) actualRsrcCompLen = rsrcCompLen - 16;

                if(dataEncrypted && dataCompLen >= 16) actualDataCompLen = dataCompLen - 16;

                var entry = new Entry
                {
                    Filename                 = name,
                    DirectoryPath            = currentDir.Length > 0 ? currentDir : null,
                    IsDirectory              = false,
                    Encrypted                = encrypted,
                    DataOffset               = dataStart + rsrcCompLen,
                    DataCompressedSize       = actualDataCompLen,
                    DataUncompressedSize     = dataLength,
                    DataMethod               = (CompressionMethod)(dataMethod & METHOD_MASK),
                    DataCrc16                = dataCrc,
                    ResourceOffset           = dataStart,
                    ResourceCompressedSize   = actualRsrcCompLen,
                    ResourceUncompressedSize = rsrcLength,
                    ResourceMethod           = (CompressionMethod)(resourceMethod & METHOD_MASK),
                    ResourceCrc16            = rsrcCrc,
                    FileType                 = fileType,
                    Creator                  = creator,
                    FinderFlags              = finderFlags,
                    CreationTime             = DateHandlers.MacToDateTime(creationDate),
                    ModificationTime         = DateHandlers.MacToDateTime(modDate)
                };

                _entries.Add(entry);

                _stream.Position = dataStart + rsrcCompLen + dataCompLen;
            }
        }

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        var hasCompression    = false;
        var hasSubdirectories = false;
        var hasDirectories    = false;
        var hasXAttrs         = false;
        var hasProtection     = false;

        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            Entry entry = _entries[i];

            if(entry.IsDirectory) hasDirectories = true;

            if(entry.DataMethod != CompressionMethod.None || entry.ResourceMethod != CompressionMethod.None)
                hasCompression = true;

            if(entry.DirectoryPath is not null) hasSubdirectories = true;

            if(entry.Encrypted) hasProtection = true;

            if(!entry.IsDirectory && (entry.ResourceUncompressedSize > 0 || entry.FileType != 0 || entry.Creator != 0))
                hasXAttrs = true;
        }

        if(hasCompression) _features    |= ArchiveSupportedFeature.SupportsCompression;
        if(hasSubdirectories) _features |= ArchiveSupportedFeature.SupportsSubdirectories;
        if(hasDirectories) _features    |= ArchiveSupportedFeature.HasExplicitDirectories;
        if(hasXAttrs) _features         |= ArchiveSupportedFeature.SupportsXAttrs;
        if(hasProtection) _features     |= ArchiveSupportedFeature.SupportsProtection;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();

        _stream  = null;
        _entries = null;
        Opened   = false;
    }

#endregion
}