using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Rar
{
    List<Entry> _entries;
    bool        _solidArchive;

    // ======================================================================
    //  RAR 1.x – 4.x parsing
    // ======================================================================

    ErrorNumber ParseRar4()
    {
        // Read archive header (block type 0x73)
        if(!ReadRar4ArchiveHeader()) return ErrorNumber.InvalidArgument;

        // Read file entries until end of archive or stream
        while(_stream.Position < _stream.Length)
            if(!ReadRar4Block())
                break;

        return ErrorNumber.NoError;
    }

    bool ReadRar4ArchiveHeader()
    {
        long blockStart = _stream.Position;

        // Base block header: CRC(2) + Type(1) + Flags(2) + Size(2) = 7 bytes
        var baseHeader = new byte[7];

        if(_stream.Read(baseHeader, 0, 7) < 7) return false;

        var blockType = (Rar4BlockType)baseHeader[2];

        if(blockType != Rar4BlockType.ArchiveHeader) return false;

        var flags      = BitConverter.ToUInt16(baseHeader, 3);
        var headerSize = BitConverter.ToUInt16(baseHeader, 5);

        // Header cannot be smaller than the base block header
        if(headerSize < 7) return false;

        AaruLogging.Debug(MODULE_NAME, "[navy]archive header flags[/] = [teal]0x{0:X4}[/]", flags);

        _solidArchive = (flags & MHD_SOLID) != 0;

        if(_solidArchive) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is solid[/]");

        if((flags & MHD_VOLUME) != 0) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is multi-volume[/]");

        if((flags & MHD_PASSWORD) != 0) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is encrypted[/]");

        // Skip rest of archive header
        _stream.Position = blockStart + headerSize;

        return true;
    }

    bool ReadRar4Block()
    {
        long blockStart = _stream.Position;

        // Base block header: CRC(2) + Type(1) + Flags(2) + Size(2) = 7 bytes
        var baseHeader = new byte[7];

        if(_stream.Read(baseHeader, 0, 7) < 7) return false;

        var blockType  = (Rar4BlockType)baseHeader[2];
        var flags      = BitConverter.ToUInt16(baseHeader, 3);
        var headerSize = BitConverter.ToUInt16(baseHeader, 5);

        // Header cannot be smaller than the base block header, would loop without advancing otherwise
        if(headerSize < 7) return false;

        // Read data size if present
        long dataSize = 0;

        if((flags & RARFLAG_LONG_BLOCK) != 0 || blockType == Rar4BlockType.FileHeader)
        {
            var dataSizeBuf = new byte[4];

            if(_stream.Read(dataSizeBuf, 0, 4) < 4) return false;

            dataSize = BitConverter.ToUInt32(dataSizeBuf, 0);
        }

        switch(blockType)
        {
            case Rar4BlockType.FileHeader:
                ParseRar4FileHeader(blockStart, headerSize, flags, dataSize);

                break;

            case Rar4BlockType.EndArchive:
                return false;

            default:
                // Skip unknown block
                _stream.Position = blockStart + headerSize + dataSize;

                break;
        }

        return true;
    }

    void ParseRar4FileHeader(long blockStart, ushort headerSize, ushort flags, long dataSize)
    {
        // Position is after base header (7 bytes) + data size (4 bytes) = at offset 11 from blockStart
        // File header layout (after base):
        //   +0: UncompressedSize (4 bytes)
        //   +4: OS (1 byte)
        //   +5: CRC32 (4 bytes)
        //   +9: DateTime (4 bytes, DOS format)
        //  +13: UnpVer (1 byte)
        //  +14: Method (1 byte)
        //  +15: NameLength (2 bytes)
        //  +17: Attributes (4 bytes)
        // Total: 21 bytes of fixed fields

        var fixedFields = new byte[21];

        if(_stream.Read(fixedFields, 0, 21) < 21) return;

        long uncompressedSize = BitConverter.ToUInt32(fixedFields, 0);
        var  os               = (HostOs)fixedFields[4];
        var  crc32            = BitConverter.ToUInt32(fixedFields, 5);
        var  dosTime          = BitConverter.ToUInt32(fixedFields, 9);
        byte unpVer           = fixedFields[13];
        byte method           = fixedFields[14];
        var  nameLength       = BitConverter.ToUInt16(fixedFields, 15);
        var  attributes       = BitConverter.ToUInt32(fixedFields, 17);

        // Handle 64-bit sizes if LHD_LARGE is set
        if((flags & LHD_LARGE) != 0)
        {
            var highBits = new byte[8];

            if(_stream.Read(highBits, 0, 8) < 8) return;

            long highUncomp = BitConverter.ToUInt32(highBits, 0);
            long highComp   = BitConverter.ToUInt32(highBits, 4);

            uncompressedSize |= highUncomp << 32;
            dataSize         |= highComp   << 32;
        }

        // Read filename
        if(nameLength is 0 or > 4096) return;

        var nameData = new byte[nameLength];

        if(_stream.Read(nameData, 0, nameLength) < nameLength) return;

        string filename = (flags & LHD_UNICODE) != 0
                              ? DecodeUnicodeFilename(nameData, nameLength, _encoding)
                              : _encoding.GetString(nameData, 0, nameLength);

        // Normalize path separators
        filename = filename.Replace('\\', '/');

        // Skip salt if present
        if((flags & LHD_SALT) != 0) _stream.Position += 8;

        // Determine directory status
        bool isDirectory = (flags & LHD_WINDOWMASK) == LHD_DIRECTORY;

        // Fallback: MS-DOS directory attribute
        if(!isDirectory && unpVer == 15 && os == HostOs.MsDos && (attributes & ATTR_DIRECTORY) != 0) isDirectory = true;

        bool isSolid;

        if(unpVer < 20)
        {
            // v1.x has no per-file solid flag: in a solid archive every file but the first depends on the previous
            isSolid = _solidArchive && _entries.Count > 0;
        }
        else
            isSolid = (flags & LHD_SOLID) != 0;

        bool isEncrypted = (flags & LHD_PASSWORD) != 0;
        bool isSplit     = (flags & LHD_SPLIT_BEFORE) != 0 || (flags & LHD_SPLIT_AFTER) != 0;

        // Parse extended time if present
        var entry = new Entry
        {
            Filename         = filename,
            CompressedSize   = dataSize,
            UncompressedSize = uncompressedSize,
            DataOffset       = blockStart + headerSize,
            LastWriteTime    = DosToDateTime(dosTime),
            Crc32            = crc32,
            Attributes       = attributes,
            UnpVersion       = unpVer,
            Method           = (CompressionMethod)method,
            Os               = os,
            IsRar5           = false,
            IsDirectory      = isDirectory,
            IsSolid          = isSolid,
            IsEncrypted      = isEncrypted,
            IsSplit          = isSplit,
            WindowSize       = 0
        };

        // Parse extended timestamps if LHD_EXTTIME is set
        if((flags & LHD_EXTTIME) != 0)
        {
            // Extended time data is after filename (and salt if present) until end of header
            long extTimeOffset = _stream.Position;
            var  remaining     = (int)(blockStart + headerSize - extTimeOffset);

            if(remaining > 0)
            {
                var extTimeData = new byte[remaining];

                if(_stream.Read(extTimeData, 0, remaining) >= 2)
                    ParseExtendedTime(extTimeData, 0, remaining, ref entry);
            }
        }

        if(isDirectory) _features |= ArchiveSupportedFeature.HasExplicitDirectories;

        if(method != (byte)CompressionMethod.Store && !isDirectory)
            _features |= ArchiveSupportedFeature.SupportsCompression;

        AaruLogging.Debug(MODULE_NAME,
                          "[navy]file[/] = [green]\"{0}\"[/], [navy]method[/] = [teal]0x{1:X2}[/], " +
                          "[navy]ver[/] = [teal]{2}[/], [navy]packed[/] = [teal]{3}[/], "            +
                          "[navy]original[/] = [teal]{4}[/]",
                          filename,
                          method,
                          unpVer,
                          dataSize,
                          uncompressedSize);

        _entries.Add(entry);

        // Skip past compressed data
        _stream.Position = blockStart + headerSize + dataSize;
    }

    // ======================================================================
    //  RAR 5.0 parsing
    // ======================================================================

    ErrorNumber ParseRar5()
    {
        while(_stream.Position < _stream.Length)
            if(!ReadRar5Block())
                break;

        return ErrorNumber.NoError;
    }

    bool ReadRar5Block()
    {
        // CRC32 (4 bytes)
        var crcBuf = new byte[4];

        if(_stream.Read(crcBuf, 0, 4) < 4) return false;

        ulong headerSize = ReadVint(_stream);

        // block.start is the position AFTER the headerSize vint.
        // headerSize measures from this point to the end of the header.
        long blockContentStart = _stream.Position;

        ulong blockType  = ReadVint(_stream);
        ulong blockFlags = ReadVint(_stream);

        ulong dataSize = 0;

        if((blockFlags & RAR5_BLOCK_HAS_EXTRA) != 0) _ = ReadVint(_stream);

        if((blockFlags & RAR5_BLOCK_HAS_DATA) != 0) dataSize = ReadVint(_stream);

        // Header ends at block.start + headerSize
        long headerEnd = blockContentStart + (long)headerSize;

        // Reject sizes that overflow to negative or point outside the file, would seek backwards otherwise
        if((long)headerSize           <= 0             ||
           (long)dataSize             < 0              ||
           headerEnd                  > _stream.Length ||
           headerEnd + (long)dataSize > _stream.Length)
            return false;

        switch((Rar5BlockType)blockType)
        {
            case Rar5BlockType.Main:
                ParseRar5MainHeader();

                break;

            case Rar5BlockType.File:
                ParseRar5FileHeader(blockFlags, headerEnd, (long)dataSize);

                break;

            case Rar5BlockType.Encryption:
                AaruLogging.Debug(MODULE_NAME, "[yellow]Archive has encryption header[/]");

                break;

            case Rar5BlockType.End:
                return false;
        }

        // Advance past header + data
        _stream.Position = headerEnd + (long)dataSize;

        return true;
    }

    void ParseRar5MainHeader()
    {
        ulong archFlags = ReadVint(_stream);

        AaruLogging.Debug(MODULE_NAME, "[navy]RAR5 archive flags[/] = [teal]0x{0:X}[/]", archFlags);

        if((archFlags & RAR5_ARCHIVE_SOLID) != 0) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is solid[/]");

        if((archFlags & RAR5_ARCHIVE_VOLUME) != 0) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is multi-volume[/]");
    }

    void ParseRar5FileHeader(ulong blockFlags, long headerEnd, long dataSize)
    {
        ulong fileFlags = ReadVint(_stream);

        ulong uncompressedSize = 0;

        if((fileFlags & RAR5_FILE_UNPACKED_SIZE_UNK) == 0) uncompressedSize = ReadVint(_stream);

        ulong attributes = ReadVint(_stream);

        DateTime lastWriteTime = DateTime.MinValue;

        if((fileFlags & RAR5_FILE_HAS_MTIME) != 0)
        {
            var mtimeBuf = new byte[4];

            if(_stream.Read(mtimeBuf, 0, 4) >= 4)
            {
                var unixTime = BitConverter.ToUInt32(mtimeBuf, 0);
                lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            }
        }

        uint crc32 = 0;

        if((fileFlags & RAR5_FILE_HAS_CRC32) != 0)
        {
            var crcBuf = new byte[4];

            if(_stream.Read(crcBuf, 0, 4) >= 4) crc32 = BitConverter.ToUInt32(crcBuf, 0);
        }

        // Compression info packed into a single vint
        ulong compInfo = ReadVint(_stream);

        var  compVersion = (int)(compInfo       & 0x3F);
        bool isSolid     = (compInfo            & 0x40) != 0;
        var  compMethod  = (int)(compInfo >> 7  & 0x07);
        var  dictShift   = (int)(compInfo >> 10 & 0x0F);

        // Window size matches unrar: 0x20000 << dictShift, minimum 0x40000.
        var windowSize = (nint)(0x20000L << dictShift);

        if(windowSize < 0x40000) windowSize = 0x40000;

        // OS
        var os = (Rar5Os)ReadVint(_stream);

        // Filename (UTF-8)
        ulong nameLength = ReadVint(_stream);

        if(nameLength is 0 or > 4096)
        {
            _stream.Position = headerEnd + dataSize;

            return;
        }

        var nameData = new byte[(int)nameLength];

        if(_stream.Read(nameData, 0, (int)nameLength) < (int)nameLength) return;

        string filename = Encoding.UTF8.GetString(nameData);

        // Normalize path separators
        filename = filename.Replace('\\', '/');

        bool isDirectory = (fileFlags & RAR5_FILE_IS_DIRECTORY) != 0;
        var  isEncrypted = false;
        bool isSplit     = (blockFlags & RAR5_BLOCK_SPLIT_BEFORE) != 0 || (blockFlags & RAR5_BLOCK_SPLIT_AFTER) != 0;

        DateTime creationTime    = DateTime.MinValue;
        DateTime lastAccessTime  = DateTime.MinValue;
        var      hasCreationTime = false;
        var      hasAccessTime   = false;

        // Parse extra area for additional metadata
        while(_stream.Position < headerEnd)
        {
            ulong extraBlockSize = ReadVint(_stream);
            long  extraDataStart = _stream.Position;

            // Reject sizes that overflow to negative, would seek backwards otherwise
            if((long)extraBlockSize <= 0) break;

            ulong extraBlockType = ReadVint(_stream);

            switch(extraBlockType)
            {
                case RAR5_EXTRA_FILE_ENCRYPTION:
                    isEncrypted = true;

                    break;

                case RAR5_EXTRA_FILE_TIME:
                    ParseRar5FileTime(ref lastWriteTime,
                                      ref creationTime,
                                      ref lastAccessTime,
                                      ref hasCreationTime,
                                      ref hasAccessTime);

                    break;
            }

            // Advance past extra block (size is measured from after the size vint)
            _stream.Position = extraDataStart + (long)extraBlockSize;

            if(extraDataStart + (long)extraBlockSize >= headerEnd) break;
        }

        // Map RAR5 compression method to a CompressionMethod value
        CompressionMethod mappedMethod = compMethod == 0
                                             ? CompressionMethod.Store
                                             : (CompressionMethod)(0x30 + compMethod);

        var entry = new Entry
        {
            Filename          = filename,
            CompressedSize    = dataSize,
            UncompressedSize  = (long)uncompressedSize,
            DataOffset        = headerEnd,
            LastWriteTime     = lastWriteTime,
            CreationTime      = creationTime,
            LastAccessTime    = lastAccessTime,
            HasCreationTime   = hasCreationTime,
            HasLastAccessTime = hasAccessTime,
            Crc32             = crc32,
            Attributes        = (uint)attributes,
            UnpVersion        = (byte)compVersion,
            Method            = mappedMethod,
            IsRar5            = true,
            Os5               = os,
            IsDirectory       = isDirectory,
            IsSolid           = isSolid,
            IsEncrypted       = isEncrypted,
            IsSplit           = isSplit,
            WindowSize        = windowSize
        };

        if(isDirectory) _features |= ArchiveSupportedFeature.HasExplicitDirectories;

        if(compMethod != 0 && !isDirectory) _features |= ArchiveSupportedFeature.SupportsCompression;

        AaruLogging.Debug(MODULE_NAME,
                          "[navy]file[/] = [green]\"{0}\"[/], [navy]method[/] = [teal]{1}[/], " +
                          "[navy]ver[/] = [teal]{2}[/], [navy]packed[/] = [teal]{3}[/], "       +
                          "[navy]original[/] = [teal]{4}[/], [navy]dict[/] = [teal]{5}[/]",
                          filename,
                          compMethod,
                          compVersion,
                          dataSize,
                          uncompressedSize,
                          windowSize);

        _entries.Add(entry);

        // Stream position will be set by ReadRar5Block
    }

    void ParseRar5FileTime(ref DateTime mtime, ref DateTime ctime, ref DateTime atime, ref bool hasCtime,
                           ref bool     hasAtime)
    {
        ulong timeFlags = ReadVint(_stream);

        bool isUnix = (timeFlags & RAR5_TIME_IS_UNIX) != 0;

        if((timeFlags & RAR5_TIME_HAS_MTIME) != 0)
        {
            if(isUnix)
            {
                var buf = new byte[4];

                if(_stream.Read(buf, 0, 4) >= 4)
                    mtime = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToUInt32(buf, 0)).DateTime;
            }
            else
            {
                var buf = new byte[8];

                if(_stream.Read(buf, 0, 8) >= 8)
                {
                    var filetime = BitConverter.ToInt64(buf, 0);
                    mtime = DateTime.FromFileTimeUtc(filetime);
                }
            }
        }

        if((timeFlags & RAR5_TIME_HAS_CTIME) != 0)
        {
            if(isUnix)
            {
                var buf = new byte[4];

                if(_stream.Read(buf, 0, 4) >= 4)
                {
                    ctime    = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToUInt32(buf, 0)).DateTime;
                    hasCtime = true;
                }
            }
            else
            {
                var buf = new byte[8];

                if(_stream.Read(buf, 0, 8) >= 8)
                {
                    var filetime = BitConverter.ToInt64(buf, 0);
                    ctime    = DateTime.FromFileTimeUtc(filetime);
                    hasCtime = true;
                }
            }
        }

        if((timeFlags & RAR5_TIME_HAS_ATIME) != 0)
        {
            if(isUnix)
            {
                var buf = new byte[4];

                if(_stream.Read(buf, 0, 4) >= 4)
                {
                    atime    = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToUInt32(buf, 0)).DateTime;
                    hasAtime = true;
                }
            }
            else
            {
                var buf = new byte[8];

                if(_stream.Read(buf, 0, 8) >= 8)
                {
                    var filetime = BitConverter.ToInt64(buf, 0);
                    atime    = DateTime.FromFileTimeUtc(filetime);
                    hasAtime = true;
                }
            }
        }
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;
        _encoding        = encoding ?? Encoding.GetEncoding(850);
        _entries         = [];
        _archiveComment  = null;
        _isRar5          = false;
        _solidArchive    = false;

        _features = ArchiveSupportedFeature.SupportsFilenames      |
                    ArchiveSupportedFeature.SupportsSubdirectories |
                    ArchiveSupportedFeature.HasEntryTimestamp;

        // Read signature to detect format version
        var sig  = new byte[8];
        int read = _stream.Read(sig, 0, 8);

        if(read < MIN_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        bool isRar5 = read   >= 8                 &&
                      sig[0] == RAR5_SIGNATURE[0] &&
                      sig[1] == RAR5_SIGNATURE[1] &&
                      sig[2] == RAR5_SIGNATURE[2] &&
                      sig[3] == RAR5_SIGNATURE[3] &&
                      sig[4] == RAR5_SIGNATURE[4] &&
                      sig[5] == RAR5_SIGNATURE[5] &&
                      sig[6] == RAR5_SIGNATURE[6] &&
                      sig[7] == RAR5_SIGNATURE[7];

        bool isRar4 = sig[0] == RAR4_SIGNATURE[0] &&
                      sig[1] == RAR4_SIGNATURE[1] &&
                      sig[2] == RAR4_SIGNATURE[2] &&
                      sig[3] == RAR4_SIGNATURE[3] &&
                      sig[4] == RAR4_SIGNATURE[4] &&
                      sig[5] == RAR4_SIGNATURE[5] &&
                      sig[6] == RAR4_SIGNATURE[6];

        if(!isRar5 && !isRar4) return ErrorNumber.InvalidArgument;

        _isRar5 = isRar5;

        ErrorNumber errno;

        if(isRar5)
        {
            _stream.Position = 8;
            errno            = ParseRar5();
        }
        else
        {
            _stream.Position = 7;
            errno            = ParseRar4();
        }

        if(errno != ErrorNumber.NoError) return errno;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();
        _entries?.Clear();

        _stream         = null;
        _archiveComment = null;
        Opened          = false;
    }

#endregion
}