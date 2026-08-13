using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class DiskDoubler
{
    /// <summary>Parse the 80-byte file header (magic already consumed). Returns total compressed size.</summary>
    long ParseFileHeader(string parentPath, string name)
    {
        var hdr = new byte[FILE_HEADER_SIZE];
        _stream.ReadExactly(hdr, 0, FILE_HEADER_SIZE);

        var  dataSize     = BigEndianBitConverter.ToUInt32(hdr, 0);
        var  dataCompSize = BigEndianBitConverter.ToUInt32(hdr, 4);
        var  rsrcSize     = BigEndianBitConverter.ToUInt32(hdr, 8);
        var  rsrcCompSize = BigEndianBitConverter.ToUInt32(hdr, 12);
        byte dataMethod   = hdr[16];
        byte rsrcMethod   = hdr[17];
        byte info1        = hdr[18];

        // hdr[19] = skip
        var modification = BigEndianBitConverter.ToUInt32(hdr, 20);
        var creation     = BigEndianBitConverter.ToUInt32(hdr, 24);
        var fileType     = BigEndianBitConverter.ToUInt32(hdr, 28);
        var creator      = BigEndianBitConverter.ToUInt32(hdr, 32);
        var finderFlags  = BigEndianBitConverter.ToUInt16(hdr, 36);

        // hdr[38..43] = skip (6 bytes)
        // hdr[44..45] = data CRC
        // hdr[46..47] = rsrc CRC
        byte info2 = hdr[48];

        // hdr[49] = skip
        var dataDelta = BigEndianBitConverter.ToUInt16(hdr, 50);
        var rsrcDelta = BigEndianBitConverter.ToUInt16(hdr, 52);

        // hdr[54..73] = skip (20 bytes)
        // hdr[74..75] = data CRC2
        // hdr[76..77] = rsrc CRC2
        // hdr[78..79] = skip (2 bytes, header CRC)

        long start = _stream.Position;

        if(dataSize > 0 || rsrcSize == 0)
        {
            var entry = new Entry
            {
                Filename                 = name,
                DirectoryPath            = parentPath.Length > 0 ? parentPath : null,
                IsDirectory              = false,
                DataOffset               = start,
                DataCompressedSize       = dataCompSize,
                DataUncompressedSize     = dataSize,
                DataMethod               = dataMethod,
                DataDelta                = (byte)(dataDelta & 0xFF),
                ResourceOffset           = start + dataCompSize,
                ResourceCompressedSize   = rsrcCompSize,
                ResourceUncompressedSize = rsrcSize,
                ResourceMethod           = rsrcMethod,
                ResourceDelta            = (byte)(rsrcDelta & 0xFF),
                FileType                 = fileType,
                Creator                  = creator,
                FinderFlags              = finderFlags,
                CreationTime             = DateHandlers.MacToDateTime(creation),
                ModificationTime         = DateHandlers.MacToDateTime(modification),
                Info1                    = info1,
                Info2                    = info2
            };

            _entries.Add(entry);
        }
        else
        {
            // Resource-fork only file
            var entry = new Entry
            {
                Filename                 = name,
                DirectoryPath            = parentPath.Length > 0 ? parentPath : null,
                IsDirectory              = false,
                DataOffset               = 0,
                DataCompressedSize       = 0,
                DataUncompressedSize     = 0,
                DataMethod               = 0,
                DataDelta                = 0,
                ResourceOffset           = start,
                ResourceCompressedSize   = rsrcCompSize,
                ResourceUncompressedSize = rsrcSize,
                ResourceMethod           = rsrcMethod,
                ResourceDelta            = (byte)(rsrcDelta & 0xFF),
                FileType                 = fileType,
                Creator                  = creator,
                FinderFlags              = finderFlags,
                CreationTime             = DateHandlers.MacToDateTime(creation),
                ModificationTime         = DateHandlers.MacToDateTime(modification),
                Info1                    = info1,
                Info2                    = info2
            };

            _entries.Add(entry);
        }

        return dataCompSize + rsrcCompSize;
    }

    void ParseSingleFile(string name)
    {
        // Strip .dd extension if present
        if(name.EndsWith(".dd", StringComparison.OrdinalIgnoreCase) ||
           name.EndsWith(".DD", StringComparison.OrdinalIgnoreCase))
            name = name[..^3];

        ParseFileHeader("", name);
    }

    void ParseDdar()
    {
        // Skip the 74-byte preamble (magic already consumed)
        _stream.Position = 4 + DDAR_PREAMBLE_SIZE;

        var currentDir = "";
        var dirStack   = new Stack<string>();

        while(_stream.Position < _stream.Length)
        {
            var magicBuf = new byte[4];

            if(_stream.Read(magicBuf, 0, 4) < 4) break;

            var magic = BigEndianBitConverter.ToUInt32(magicBuf, 0);

            if(magic == MAGIC_SINGLE)
            {
                // Skip redundant file headers at end of archive
                _stream.Position += FILE_HEADER_SIZE;

                continue;
            }

            if(magic != MAGIC_DDAR) break;

            // Skip 4 bytes
            _stream.Position += 4;

            int namelen = _stream.ReadByte();

            if(namelen < 0) break;

            if(namelen > 63) namelen = 63;

            var nameBuf = new byte[63];
            _stream.ReadExactly(nameBuf, 0, 63);

            string entryName = _encoding.GetString(nameBuf, 0, namelen);

            int isDir  = _stream.ReadByte();
            int endDir = _stream.ReadByte();

            var entryHdr = new byte[38];
            _stream.ReadExactly(entryHdr, 0, 38);

            var dataSize     = BigEndianBitConverter.ToUInt32(entryHdr, 0);
            var rsrcSize     = BigEndianBitConverter.ToUInt32(entryHdr, 4);
            var creation     = BigEndianBitConverter.ToUInt32(entryHdr, 8);
            var modification = BigEndianBitConverter.ToUInt32(entryHdr, 12);
            var fileType     = BigEndianBitConverter.ToUInt32(entryHdr, 16);
            var creator      = BigEndianBitConverter.ToUInt32(entryHdr, 20);
            var finderFlags  = BigEndianBitConverter.ToUInt16(entryHdr, 24);

            // Skip 18 bytes
            // entryHdr[26..37] = reserved (but we only read 38 bytes total, so need to skip remaining)
            _stream.Position += 18 + 2 + 2 + 2; // skip 18 reserved + dataCrc(2) + rsrcCrc(2) + more(2)

            long start = _stream.Position;

            if(endDir != 0)
            {
                currentDir = dirStack.Count > 0 ? dirStack.Pop() : "";
            }
            else if(isDir != 0)
            {
                var dirEntry = new Entry
                {
                    Filename         = entryName,
                    DirectoryPath    = currentDir.Length > 0 ? currentDir : null,
                    IsDirectory      = true,
                    CreationTime     = DateHandlers.MacToDateTime(creation),
                    ModificationTime = DateHandlers.MacToDateTime(modification),
                    FinderFlags      = finderFlags
                };

                _entries.Add(dirEntry);

                dirStack.Push(currentDir);
                currentDir = currentDir.Length > 0 ? currentDir + "/" + entryName : entryName;

                _stream.Position = start;
            }
            else if((finderFlags & FINDER_FLAG_INLINE) != 0)
            {
                // Inline (uncompressed) data — no 0xabcd0054 sub-header
                if(dataSize > 0 || rsrcSize == 0)
                {
                    var entry = new Entry
                    {
                        Filename                 = entryName,
                        DirectoryPath            = currentDir.Length > 0 ? currentDir : null,
                        IsDirectory              = false,
                        DataOffset               = start,
                        DataCompressedSize       = dataSize,
                        DataUncompressedSize     = dataSize,
                        DataMethod               = 0,
                        DataDelta                = 0,
                        ResourceOffset           = start + dataSize,
                        ResourceCompressedSize   = rsrcSize,
                        ResourceUncompressedSize = rsrcSize,
                        ResourceMethod           = 0,
                        ResourceDelta            = 0,
                        FileType                 = fileType,
                        Creator                  = creator,
                        FinderFlags              = finderFlags,
                        CreationTime             = DateHandlers.MacToDateTime(creation),
                        ModificationTime         = DateHandlers.MacToDateTime(modification),
                        Info1                    = 0,
                        Info2                    = 0
                    };

                    _entries.Add(entry);
                }
                else
                {
                    var entry = new Entry
                    {
                        Filename                 = entryName,
                        DirectoryPath            = currentDir.Length > 0 ? currentDir : null,
                        IsDirectory              = false,
                        DataOffset               = 0,
                        DataCompressedSize       = 0,
                        DataUncompressedSize     = 0,
                        DataMethod               = 0,
                        DataDelta                = 0,
                        ResourceOffset           = start,
                        ResourceCompressedSize   = rsrcSize,
                        ResourceUncompressedSize = rsrcSize,
                        ResourceMethod           = 0,
                        ResourceDelta            = 0,
                        FileType                 = fileType,
                        Creator                  = creator,
                        FinderFlags              = finderFlags,
                        CreationTime             = DateHandlers.MacToDateTime(creation),
                        ModificationTime         = DateHandlers.MacToDateTime(modification),
                        Info1                    = 0,
                        Info2                    = 0
                    };

                    _entries.Add(entry);
                }

                _stream.Position = start + dataSize + rsrcSize;
            }
            else
            {
                // Compressed file — expect 0xabcd0054 sub-header
                var subMagic = new byte[4];
                _stream.ReadExactly(subMagic, 0, 4);

                var subMagicVal = BigEndianBitConverter.ToUInt32(subMagic, 0);

                if(subMagicVal != MAGIC_SINGLE) break;

                long totalSize = ParseFileHeader(currentDir, entryName);

                _stream.Position = start + 84 + totalSize;
            }
        }
    }

    void ParseDda2()
    {
        // Skip the 58-byte preamble (magic already consumed)
        _stream.Position = 4 + DDA2_PREAMBLE_SIZE;

        var dirParts = new List<string>();

        while(_stream.Position < _stream.Length)
        {
            long entryStart = _stream.Position;

            var magicBuf = new byte[4];

            if(_stream.Read(magicBuf, 0, 4) < 4) break;

            var magic = BigEndianBitConverter.ToUInt32(magicBuf, 0);

            if(magic != MAGIC_DDA2) break;

            var entryTypeBuf = new byte[2];
            _stream.ReadExactly(entryTypeBuf, 0, 2);

            var entryType = BigEndianBitConverter.ToUInt16(entryTypeBuf, 0);

            if(entryType == DDA2_TERMINATOR) break;

            int namelen = _stream.ReadByte();

            if(namelen < 0) break;

            if(namelen > 31) namelen = 31;

            var nameBuf = new byte[31];
            _stream.ReadExactly(nameBuf, 0, 31);

            string entryName = _encoding.GetString(nameBuf, 0, namelen);

            var levelBuf = new byte[8];
            _stream.ReadExactly(levelBuf, 0, 8);

            var dirLevel  = (int)(BigEndianBitConverter.ToUInt32(levelBuf, 0) - 2);
            var totalSize = BigEndianBitConverter.ToUInt32(levelBuf, 4);

            // An entry cannot be smaller than its fixed header, and advancing by less would loop forever
            if(totalSize < 46) break;

            if(dirLevel < 0)
            {
                _stream.Position = entryStart + totalSize;

                continue;
            }

            // Pop directories to match current level
            while(dirParts.Count > dirLevel) dirParts.RemoveAt(dirParts.Count - 1);

            string currentDir = string.Join("/", dirParts);

            if((entryType & 0x8000) != 0)
            {
                // Directory entry
                {
                    // Skip 8 bytes
                    _stream.Position += 8;

                    var timeBuf = new byte[8];
                    _stream.ReadExactly(timeBuf, 0, 8);

                    var creation     = BigEndianBitConverter.ToUInt32(timeBuf, 0);
                    var modification = BigEndianBitConverter.ToUInt32(timeBuf, 4);

                    var dirEntry = new Entry
                    {
                        Filename         = entryName,
                        DirectoryPath    = currentDir.Length > 0 ? currentDir : null,
                        IsDirectory      = true,
                        CreationTime     = DateHandlers.MacToDateTime(creation),
                        ModificationTime = DateHandlers.MacToDateTime(modification)
                    };

                    _entries.Add(dirEntry);

                    dirParts.Add(entryName);
                }
            }
            else
            {
                // File entry — skip 10 bytes, then expect 0xabcd0054
                _stream.Position += 10;

                var subMagic = new byte[4];
                _stream.ReadExactly(subMagic, 0, 4);

                var subMagicVal = BigEndianBitConverter.ToUInt32(subMagic, 0);

                if(subMagicVal == MAGIC_SINGLE) ParseFileHeader(currentDir, entryName);
            }

            _stream.Position = entryStart + totalSize;
        }
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < 62) return ErrorNumber.InvalidArgument;

        _encoding        = encoding ?? Encoding.GetEncoding("macintosh");
        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        var hdr = new byte[4];
        _stream.ReadExactly(hdr, 0, 4);

        var magic = BigEndianBitConverter.ToUInt32(hdr, 0);

        _entries = [];

        switch(magic)
        {
            case MAGIC_SINGLE:
                ParseSingleFile(filter.Filename ?? "unnamed");

                break;
            case MAGIC_DDAR:
                ParseDdar();

                break;
            case MAGIC_DDA2:
                ParseDda2();

                break;
            default:
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

            if((entry.DataMethod & 0x7F) != 0 || (entry.ResourceMethod & 0x7F) != 0) hasCompression = true;

            if(entry.DirectoryPath is not null) hasSubdirectories = true;

            if(!entry.IsDirectory && (entry.ResourceUncompressedSize > 0 || entry.FileType != 0 || entry.Creator != 0))
                hasXAttrs = true;
        }

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

        _stream?.Close();

        _stream  = null;
        _entries = null;
        Opened   = false;
    }

#endregion
}