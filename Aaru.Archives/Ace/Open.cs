using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Ace
{
    List<Entry> _entries;

    /// <summary>Search for the **ACE** signature in the stream.</summary>
    /// <returns>Stream position of the signature, or -1 if not found.</returns>
    static long FindSignature(Stream stream)
    {
        stream.Position = 0;
        long searchLimit = Math.Min(stream.Length, MAX_SFX_SEARCH);

        // Read in chunks for efficiency
        var  bufSize = (int)Math.Min(searchLimit, 65536);
        var  buf     = new byte[bufSize];
        long filePos = 0;
        int  overlap = ACE_SIGNATURE_LEN - 1;

        while(filePos < searchLimit)
        {
            var toRead = (int)Math.Min(bufSize, searchLimit - filePos);
            int read   = stream.Read(buf, 0, toRead);

            if(read < ACE_SIGNATURE_LEN) break;

            for(var i = 0; i <= read - ACE_SIGNATURE_LEN; i++)
            {
                if(buf[i]     == ACE_SIGNATURE[0] &&
                   buf[i + 1] == ACE_SIGNATURE[1] &&
                   buf[i + 2] == ACE_SIGNATURE[2] &&
                   buf[i + 3] == ACE_SIGNATURE[3] &&
                   buf[i + 4] == ACE_SIGNATURE[4] &&
                   buf[i + 5] == ACE_SIGNATURE[5] &&
                   buf[i + 6] == ACE_SIGNATURE[6])
                {
                    long candidatePos = filePos + i;

                    // Validate: the main header should start 7 bytes before the signature
                    long headerStart = candidatePos - SIGNATURE_HEADER_OFFSET;

                    if(headerStart < 0) continue;

                    // Validate header CRC
                    if(ValidateMainHeaderCrc(stream, headerStart)) return candidatePos;
                }
            }

            // Overlap to catch signatures spanning chunk boundaries
            if(read >= overlap)
            {
                filePos         += read - overlap;
                stream.Position =  filePos;
            }
            else
                break;
        }

        return -1;
    }

    /// <summary>Validate the CRC of the main header at the given position.</summary>
    static bool ValidateMainHeaderCrc(Stream stream, long headerStart)
    {
        long savedPos = stream.Position;

        stream.Position = headerStart;

        // Read CRC(2) + SIZE(2)
        var prefix = new byte[4];

        if(stream.Read(prefix, 0, 4) < 4)
        {
            stream.Position = savedPos;

            return false;
        }

        var storedCrc  = BitConverter.ToUInt16(prefix, 0);
        var headerSize = BitConverter.ToUInt16(prefix, 2);

        if(headerSize < 1 || headerSize > 32768)
        {
            stream.Position = savedPos;

            return false;
        }

        var headerData = new byte[headerSize];

        if(stream.Read(headerData, 0, headerSize) < headerSize)
        {
            stream.Position = savedPos;

            return false;
        }

        ushort computedCrc = ComputeHeaderCrc(headerData);

        stream.Position = savedPos;

        return storedCrc == computedCrc;
    }

    /// <summary>Read and parse the main archive header.</summary>
    bool ReadMainHeader()
    {
        var prefix = new byte[4];

        if(_stream.Read(prefix, 0, 4) < 4) return false;

        var storedCrc  = BitConverter.ToUInt16(prefix, 0);
        var headerSize = BitConverter.ToUInt16(prefix, 2);

        if(headerSize < 1 || headerSize > 32768) return false;

        var headerData = new byte[headerSize];

        if(_stream.Read(headerData, 0, headerSize) < headerSize) return false;

        ushort computedCrc = ComputeHeaderCrc(headerData);

        if(storedCrc != computedCrc)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "[red]Main header CRC mismatch: stored 0x{0:X4}, computed 0x{1:X4}[/]",
                              storedCrc,
                              computedCrc);

            return false;
        }

        // Parse main header fields
        // headerData[0] = HEAD_TYPE (should be 0)
        // headerData[1..2] = HEAD_FLAGS
        // headerData[3..9] = ACESIGN
        // headerData[10] = VER_EXTRACT
        // headerData[11] = VER_CREATED
        // headerData[12] = HOST_CREATED
        // headerData[13] = VOL_NUM
        // headerData[14..17] = TIME_CREATED
        // headerData[18..19] = RES1
        // headerData[20..21] = RES2
        // headerData[22..25] = RES
        // headerData[26] = AV_SIZE
        // headerData[27..27+AV_SIZE-1] = AV string

        if(headerData[0] != (byte)HeaderType.Main) return false;

        var mainFlags = BitConverter.ToUInt16(headerData, 1);

        AaruLogging.Debug(MODULE_NAME, "[navy]main flags[/] = [teal]0x{0:X4}[/]", mainFlags);

        if(headerSize < 27) return false;

        byte avSize = headerData[26];
        int  pos    = 27 + avSize;

        // Parse optional comment
        if((mainFlags & FLAG_COMMENT) != 0 && pos + 2 <= headerSize)
        {
            var commentCompressedSize = BitConverter.ToUInt16(headerData, pos);
            pos += 2;

            if(commentCompressedSize > 0 && pos + commentCompressedSize <= headerSize)
            {
                var commentData = new byte[commentCompressedSize];
                Array.Copy(headerData, pos, commentData, 0, commentCompressedSize);

                _archiveComment = DecompressComment(commentData);

                if(_archiveComment is not null) _features |= ArchiveSupportedFeature.SupportsXAttrs;

                pos += commentCompressedSize;
            }
        }

        if((mainFlags & FLAG_SOLID) != 0) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is solid[/]");

        if((mainFlags & FLAG_MULTIVOLUME) != 0) AaruLogging.Debug(MODULE_NAME, "[yellow]Archive is multi-volume[/]");

        return true;
    }

    /// <summary>Read a generic block header and dispatch based on type.</summary>
    /// <returns>True if a block was successfully read and there may be more blocks.</returns>
    bool ReadBlock()
    {
        long blockStart = _stream.Position;

        // Read CRC(2) + SIZE(2)
        var prefix = new byte[4];

        if(_stream.Read(prefix, 0, 4) < 4) return false;

        var storedCrc  = BitConverter.ToUInt16(prefix, 0);
        var headerSize = BitConverter.ToUInt16(prefix, 2);

        if(headerSize < 1 || headerSize > 32768) return false;

        var headerData = new byte[headerSize];

        if(_stream.Read(headerData, 0, headerSize) < headerSize) return false;

        ushort computedCrc = ComputeHeaderCrc(headerData);

        if(storedCrc != computedCrc)
        {
            AaruLogging.Debug(MODULE_NAME, "[red]Block header CRC mismatch at offset {0}[/]", blockStart);

            return false;
        }

        var blockType = (HeaderType)headerData[0];
        var flags     = BitConverter.ToUInt16(headerData, 1);

        switch(blockType)
        {
            case HeaderType.File32:
                ParseFileHeader32(headerData, flags);

                break;

            case HeaderType.File:
                ParseFileHeader64(headerData, flags);

                break;

            case HeaderType.Main:
            case HeaderType.Recovery32:
            case HeaderType.Recovery:
            case HeaderType.Recovery2:
            default:
                // Skip ADDSIZE if present
                if((flags & FLAG_ADDSIZE) != 0)
                {
                    long addSize = GetAddSize(headerData, flags, blockType);

                    // Reject sizes that would rewind the stream or point past end of file
                    if(addSize < 0 || addSize > _stream.Length - _stream.Position) return false;

                    _stream.Position += addSize;
                }

                break;
        }

        return true;
    }

    /// <summary>Get the ADDSIZE field value from a header block.</summary>
    long GetAddSize(byte[] headerData, ushort flags, HeaderType blockType)
    {
        if((flags & FLAG_ADDSIZE) == 0) return 0;

        // For 64-bit blocks, ADDSIZE is 8 bytes starting at offset 3 (after TYPE + FLAGS)
        // For 32-bit blocks, ADDSIZE is 4 bytes starting at offset 3
        if((flags & FLAG_64BIT) != 0                   ||
           blockType            == HeaderType.File     ||
           blockType            == HeaderType.Recovery ||
           blockType            == HeaderType.Recovery2)
        {
            if(headerData.Length >= 11) return (long)BitConverter.ToUInt64(headerData, 3);
        }
        else
        {
            if(headerData.Length >= 7) return BitConverter.ToUInt32(headerData, 3);
        }

        return 0;
    }

    /// <summary>Parse a 32-bit file header (block type 1).</summary>
    void ParseFileHeader32(byte[] headerData, ushort flags)
    {
        // Layout after TYPE(1) + FLAGS(2):
        // +3: PSIZE (4 bytes, packed/compressed size)
        // +7: SIZE (4 bytes, original size)
        // +11: FTIME (4 bytes, DOS datetime)
        // +15: ATTR (4 bytes)
        // +19: CRC32 (4 bytes)
        // +23: TECH.TYPE (1 byte)
        // +24: TECH.QUAL (1 byte)
        // +25: TECH.PARM (2 bytes)
        // +27: RESERVED (2 bytes)
        // +29: FNAME_SIZE (2 bytes)
        // +31: FNAME (FNAME_SIZE bytes)

        if(headerData.Length < 31) return;

        var packedSize = BitConverter.ToUInt32(headerData, 3);

        // Reject sizes that point past end of file
        if(packedSize > _stream.Length - _stream.Position)
        {
            _stream.Position = _stream.Length;

            return;
        }

        var  originalSize = BitConverter.ToUInt32(headerData, 7);
        var  dosTime      = BitConverter.ToUInt32(headerData, 11);
        var  attr         = BitConverter.ToUInt32(headerData, 15);
        var  crc32        = BitConverter.ToUInt32(headerData, 19);
        byte techType     = headerData[23];
        byte techQual     = headerData[24];
        var  techParm     = BitConverter.ToUInt16(headerData, 25);
        var  filenameSize = BitConverter.ToUInt16(headerData, 29);

        if(31 + filenameSize > headerData.Length) return;

        string filename = _encoding.GetString(headerData, 31, filenameSize).TrimEnd('\0');

        // Normalize path separators
        filename = filename.Replace('\\', '/');

        // Parse optional comment
        string comment = null;
        int    pos     = 31 + filenameSize;

        if((flags & FLAG_COMMENT) != 0 && pos + 2 <= headerData.Length)
        {
            var commentSize = BitConverter.ToUInt16(headerData, pos);
            pos += 2;

            if(commentSize > 0 && pos + commentSize <= headerData.Length)
            {
                var commentData = new byte[commentSize];
                Array.Copy(headerData, pos, commentData, 0, commentSize);
                comment = DecompressComment(commentData);

                if(comment is not null) _features |= ArchiveSupportedFeature.SupportsXAttrs;
            }
        }

        bool isSolid     = (flags & FLAG_SOLID)    != 0;
        bool isEncrypted = (flags & FLAG_PASSWORD) != 0;
        bool isSplit     = (flags & FLAG_SPLITBEFORE) != 0 || (flags & FLAG_SPLITAFTER) != 0;
        bool isDirectory = (attr & ATTR_DIRECTORY) != 0;

        if(isDirectory) _features |= ArchiveSupportedFeature.HasExplicitDirectories;

        if(techType != (byte)CompressionType.Stored && !isDirectory)
            _features |= ArchiveSupportedFeature.SupportsCompression;

        var entry = new Entry
        {
            Method           = (CompressionType)techType,
            Filename         = filename,
            CompressedSize   = packedSize,
            UncompressedSize = originalSize,
            DataOffset       = _stream.Position,
            LastWriteTime    = DosToDateTime(dosTime),
            Crc32            = crc32,
            Attributes       = attr,
            Quality          = techQual,
            DecompParam      = techParm,
            Host             = HostOs.MsDos,
            Comment          = comment,
            IsDirectory      = isDirectory,
            IsSolid          = isSolid,
            IsEncrypted      = isEncrypted,
            IsSplit          = isSplit
        };

        AaruLogging.Debug(MODULE_NAME,
                          "[navy]file[/] = [green]\"{0}\"[/], [navy]method[/] = [teal]{1}[/], [navy]packed[/] = [teal]{2}[/], [navy]original[/] = [teal]{3}[/]",
                          filename,
                          techType,
                          packedSize,
                          originalSize);

        _entries.Add(entry);

        // Skip compressed data (ADDSIZE = PSIZE)
        _stream.Position += packedSize;
    }

    /// <summary>Parse a 64-bit file header (block type 3).</summary>
    void ParseFileHeader64(byte[] headerData, ushort flags)
    {
        // Layout after TYPE(1) + FLAGS(2):
        // +3: PSIZE (8 bytes, packed/compressed size)
        // +11: SIZE (8 bytes, original size)
        // +19: FTIME (4 bytes, DOS datetime)
        // +23: ATTR (4 bytes)
        // +27: CRC32 (4 bytes)
        // +31: TECH.TYPE (1 byte)
        // +32: TECH.QUAL (1 byte)
        // +33: TECH.PARM (2 bytes)
        // +35: RESERVED (2 bytes)
        // +37: FNAME_SIZE (2 bytes)
        // +39: FNAME (FNAME_SIZE bytes)

        if(headerData.Length < 39) return;

        var packedSize = (long)BitConverter.ToUInt64(headerData, 3);

        // Reject sizes that would rewind the stream or point past end of file
        if(packedSize < 0 || packedSize > _stream.Length - _stream.Position)
        {
            _stream.Position = _stream.Length;

            return;
        }

        var  originalSize = (long)BitConverter.ToUInt64(headerData, 11);
        var  dosTime      = BitConverter.ToUInt32(headerData, 19);
        var  attr         = BitConverter.ToUInt32(headerData, 23);
        var  crc32        = BitConverter.ToUInt32(headerData, 27);
        byte techType     = headerData[31];
        byte techQual     = headerData[32];
        var  techParm     = BitConverter.ToUInt16(headerData, 33);
        var  filenameSize = BitConverter.ToUInt16(headerData, 37);

        if(39 + filenameSize > headerData.Length) return;

        string filename = _encoding.GetString(headerData, 39, filenameSize).TrimEnd('\0');

        // Normalize path separators
        filename = filename.Replace('\\', '/');

        // Parse optional comment
        string comment = null;
        int    pos     = 39 + filenameSize;

        if((flags & FLAG_COMMENT) != 0 && pos + 2 <= headerData.Length)
        {
            var commentSize = BitConverter.ToUInt16(headerData, pos);
            pos += 2;

            if(commentSize > 0 && pos + commentSize <= headerData.Length)
            {
                var commentData = new byte[commentSize];
                Array.Copy(headerData, pos, commentData, 0, commentSize);
                comment = DecompressComment(commentData);

                if(comment is not null) _features |= ArchiveSupportedFeature.SupportsXAttrs;
            }
        }

        bool isSolid     = (flags & FLAG_SOLID)    != 0;
        bool isEncrypted = (flags & FLAG_PASSWORD) != 0;
        bool isSplit     = (flags & FLAG_SPLITBEFORE) != 0 || (flags & FLAG_SPLITAFTER) != 0;
        bool isDirectory = (attr & ATTR_DIRECTORY) != 0;

        if(isDirectory) _features |= ArchiveSupportedFeature.HasExplicitDirectories;

        if(techType != (byte)CompressionType.Stored && !isDirectory)
            _features |= ArchiveSupportedFeature.SupportsCompression;

        var entry = new Entry
        {
            Method           = (CompressionType)techType,
            Filename         = filename,
            CompressedSize   = packedSize,
            UncompressedSize = originalSize,
            DataOffset       = _stream.Position,
            LastWriteTime    = DosToDateTime(dosTime),
            Crc32            = crc32,
            Attributes       = attr,
            Quality          = techQual,
            DecompParam      = techParm,
            Host             = HostOs.MsDos,
            Comment          = comment,
            IsDirectory      = isDirectory,
            IsSolid          = isSolid,
            IsEncrypted      = isEncrypted,
            IsSplit          = isSplit
        };

        AaruLogging.Debug(MODULE_NAME,
                          "[navy]file[/] = [green]\"{0}\"[/], [navy]method[/] = [teal]{1}[/], [navy]packed[/] = [teal]{2}[/], [navy]original[/] = [teal]{3}[/]",
                          filename,
                          techType,
                          packedSize,
                          originalSize);

        _entries.Add(entry);

        // Skip compressed data (ADDSIZE = PSIZE)
        _stream.Position += packedSize;
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

        _features = ArchiveSupportedFeature.SupportsFilenames      |
                    ArchiveSupportedFeature.SupportsSubdirectories |
                    ArchiveSupportedFeature.HasEntryTimestamp;

        // Search for the ACE signature to find the archive start
        long archiveBegin = FindSignature(_stream);

        if(archiveBegin < 0) return ErrorNumber.InvalidArgument;

        // Position at the start of the main header
        long mainHeaderStart = archiveBegin - SIGNATURE_HEADER_OFFSET;

        if(mainHeaderStart < 0) return ErrorNumber.InvalidArgument;

        _stream.Position = mainHeaderStart;

        // Read the main header
        if(!ReadMainHeader()) return ErrorNumber.InvalidArgument;

        // Read file entries until end of archive
        while(_stream.Position < _stream.Length)
        {
            if(!ReadBlock()) break;
        }

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