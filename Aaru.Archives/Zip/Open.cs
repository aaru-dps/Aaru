using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Archives;

public sealed partial class Zip
{
    List<Entry> _entries;

    void FindEndOfCentralDirectory(out long eocdOffset, out long zip64LocOffset)
    {
        eocdOffset     = -1;
        zip64LocOffset = -1;

        long end = _stream.Length;

        // Search backward in chunks for the EOCD signature
        long chunkEnd = end;

        while(chunkEnd > 0)
        {
            int numBytes = EOCD_SEARCH_CHUNK;

            if(chunkEnd - numBytes < 0) numBytes = (int)chunkEnd;

            long chunkStart = chunkEnd - numBytes;

            // Overlap by 20 bytes to catch ZIP64 locator that might span chunk boundaries
            int preservedBytes = chunkStart >= 20 ? 20 : 0;

            var buf = new byte[numBytes];
            _stream.Position = chunkStart;

            int bytesRead = _stream.Read(buf, 0, numBytes);

            if(bytesRead < 4) break;

            // Search for PK\x05\x06 (EOCD signature)
            for(int pos = bytesRead - 4; pos >= preservedBytes; pos--)
            {
                if(buf[pos] != 0x50 || buf[pos + 1] != 0x4B || buf[pos + 2] != 0x05 || buf[pos + 3] != 0x06) continue;

                eocdOffset = chunkStart + pos;

                // Check for ZIP64 end of central directory locator 20 bytes before EOCD
                if(pos           >= 20   &&
                   buf[pos - 20] == 0x50 &&
                   buf[pos - 19] == 0x4B &&
                   buf[pos - 18] == 0x06 &&
                   buf[pos - 17] == 0x07)
                    zip64LocOffset = chunkStart + pos - 20;

                return;
            }

            chunkEnd -= numBytes - preservedBytes * 2;
        }
    }

    void ParseCentralDirectory(long eocdOffset, long zip64LocOffset)
    {
        var reader = new BinaryReader(_stream, Encoding.UTF8, true);

        // Read EOCD record
        _stream.Position = eocdOffset + 4; // skip signature

        ushort diskNumber       = reader.ReadUInt16();
        ushort centralDirDisk   = reader.ReadUInt16();
        ushort entriesOnDisk    = reader.ReadUInt16();
        long   totalEntries     = reader.ReadUInt16();
        long   centralDirSize   = reader.ReadUInt32();
        long   centralDirOffset = reader.ReadUInt32();
        ushort commentLength    = reader.ReadUInt16();

        if(commentLength > 0)
        {
            byte[] commentBytes = reader.ReadBytes(commentLength);
            _archiveComment = Encoding.UTF8.GetString(commentBytes);
        }

        // If we have a ZIP64 locator, read the ZIP64 EOCD record
        if(zip64LocOffset >= 0)
        {
            _stream.Position = zip64LocOffset + 4; // skip signature

            uint zip64EocdDisk   = reader.ReadUInt32();
            long zip64EocdOffset = reader.ReadInt64();

            // Reject offsets outside the file
            if(zip64EocdOffset < 0 || zip64EocdOffset + 56 > _stream.Length) return;

            _stream.Position = zip64EocdOffset;
            uint zip64Sig = reader.ReadUInt32();

            if(zip64Sig == ZIP64_EOCD_SIG)
            {
                reader.ReadInt64();  // size of ZIP64 EOCD record
                reader.ReadUInt16(); // version made by
                reader.ReadUInt16(); // version needed
                reader.ReadUInt32(); // disk number
                centralDirDisk = (ushort)reader.ReadUInt32();
                reader.ReadInt64(); // entries on disk
                totalEntries     = reader.ReadInt64();
                centralDirSize   = reader.ReadInt64();
                centralDirOffset = reader.ReadInt64();
            }
        }

        // Seek to the start of the central directory, rejecting offsets outside the file
        if(centralDirOffset < 0 || centralDirOffset > _stream.Length) return;

        _stream.Position = centralDirOffset;

        for(long i = 0; i < totalEntries; i++)
        {
            if(_stream.Position >= _stream.Length) break;

            Entry entry = ReadCentralDirectoryRecord(reader);

            if(entry.Filename is null) break;

            long savedPos = _stream.Position;

            // Reject local header offsets outside the file
            if(entry.DataOffset < 0 || entry.DataOffset + 30 > _stream.Length) break;

            // Read local header to get exact data offset
            _stream.Position = entry.DataOffset; // locheaderoffset stored temporarily in DataOffset
            uint localSig = reader.ReadUInt32();

            if(localSig == LOCAL_HEADER_SIG || localSig == EOCD_SIG)
            {
                reader.ReadUInt16(); // version needed
                reader.ReadUInt16(); // flags
                reader.ReadUInt16(); // compression method
                uint localDate = reader.ReadUInt32();
                reader.ReadUInt32(); // crc32
                reader.ReadUInt32(); // compressed size
                reader.ReadUInt32(); // uncompressed size
                ushort localNameLen  = reader.ReadUInt16();
                ushort localExtraLen = reader.ReadUInt16();

                long dataOffset = _stream.Position + localNameLen + localExtraLen;

                // Parse local extra fields for additional metadata (timestamps, unicode paths, etc.)
                if(localNameLen > 0) reader.ReadBytes(localNameLen);

                if(localExtraLen > 0)
                {
                    long extraStart = _stream.Position;
                    ParseExtraFields(reader, localExtraLen, ref entry);
                    _stream.Position = extraStart + localExtraLen;
                }

                entry.DataOffset = dataOffset;
            }

            // Handle overflow for archives with >65535 entries without ZIP64
            if(i == totalEntries - 1 && zip64LocOffset < 0)
            {
                long remaining = centralDirOffset + centralDirSize - savedPos;

                if(remaining > 65536 * CENTRAL_DIR_ENTRY_SIZE) totalEntries += 65536;
            }

            _entries.Add(entry);

            _stream.Position = savedPos;
        }
    }

    Entry ReadCentralDirectoryRecord(BinaryReader reader)
    {
        Entry entry = new();

        uint sig = reader.ReadUInt32();

        if(sig != CENTRAL_DIR_SIG) return entry;

        byte   creatorVersion = reader.ReadByte();
        byte   system         = reader.ReadByte();
        ushort extractVersion = reader.ReadUInt16();
        ushort flags          = reader.ReadUInt16();
        ushort method         = reader.ReadUInt16();
        uint   dosDateTime    = reader.ReadUInt32();
        uint   crc32          = reader.ReadUInt32();
        long   compSize       = reader.ReadUInt32();
        long   uncompSize     = reader.ReadUInt32();
        ushort nameLength     = reader.ReadUInt16();
        ushort extraLength    = reader.ReadUInt16();
        ushort commentLength  = reader.ReadUInt16();
        ushort startDisk      = reader.ReadUInt16();
        ushort intFileAttrib  = reader.ReadUInt16();
        uint   extFileAttrib  = reader.ReadUInt32();
        long   locHeaderOff   = reader.ReadUInt32();

        // Read filename
        byte[] nameBytes = nameLength > 0 ? reader.ReadBytes(nameLength) : null;

        // Parse extra fields in central directory (primarily for ZIP64 overrides)
        int extraRemaining = extraLength;

        while(extraRemaining >= 4)
        {
            ushort extId   = reader.ReadUInt16();
            ushort extSize = reader.ReadUInt16();
            extraRemaining -= 4;

            if(extSize > extraRemaining) break;

            extraRemaining -= extSize;
            long nextExtra = _stream.Position + extSize;

            if(extId == EXTRA_ZIP64)
            {
                if(uncompSize == 0xFFFFFFFF && _stream.Position + 8 <= nextExtra) uncompSize = reader.ReadInt64();

                if(compSize == 0xFFFFFFFF && _stream.Position + 8 <= nextExtra) compSize = reader.ReadInt64();

                if(locHeaderOff == 0xFFFFFFFF && _stream.Position + 8 <= nextExtra) locHeaderOff = reader.ReadInt64();

                if(startDisk == 0xFFFF && _stream.Position + 4 <= nextExtra) startDisk = (ushort)reader.ReadUInt32();
            }

            _stream.Position = nextExtra;
        }

        if(extraRemaining > 0) _stream.Position += extraRemaining;

        // Read comment
        string comment = null;

        if(commentLength > 0)
        {
            byte[] commentBytes = reader.ReadBytes(commentLength);

            comment = (flags & FLAG_UTF8) != 0
                          ? Encoding.UTF8.GetString(commentBytes)
                          : _encoding.GetString(commentBytes);
        }

        // Decode filename
        string filename = null;

        if(nameBytes is not null)
        {
            filename = (flags & FLAG_UTF8) != 0 ? Encoding.UTF8.GetString(nameBytes) : _encoding.GetString(nameBytes);

            // Normalize path separators
            filename = filename.Replace('\\', '/');
        }

        // Detect directory
        var isDirectory = false;

        if(filename is not null && filename.EndsWith('/') && uncompSize == 0) isDirectory = true;

        if(system == (byte)HostOs.MsDos && (extFileAttrib & ATTR_DIRECTORY) != 0 && compSize == 0 && uncompSize == 0)
            isDirectory = true;

        // Extract Unix permissions from external attributes for Unix-created archives
        ushort unixPerms = 0;

        if(system == (byte)HostOs.Unix)
        {
            var perm = (int)(extFileAttrib >> 16);

            if(perm != 0) unixPerms = (ushort)perm;
        }

        entry.Method             = (CompressionMethod)method;
        entry.Filename           = filename;
        entry.CompressedSize     = compSize;
        entry.UncompressedSize   = uncompSize;
        entry.DataOffset         = locHeaderOff; // temporary, resolved to actual data offset later
        entry.LastWriteTime      = DosToDateTime(dosDateTime);
        entry.Crc32              = crc32;
        entry.System             = (HostOs)system;
        entry.ExternalAttributes = extFileAttrib;
        entry.UnixPermissions    = unixPerms;
        entry.Flags              = flags;
        entry.Comment            = comment;
        entry.IsDirectory        = isDirectory;
        entry.IsEncrypted        = (flags & FLAG_ENCRYPTED) != 0;

        return entry;
    }

    void ParseWithoutCentralDirectory()
    {
        var reader = new BinaryReader(_stream, Encoding.UTF8, true);

        _stream.Position = 0;

        while(_stream.Position < _stream.Length - 4)
        {
            uint sig;

            try
            {
                sig = reader.ReadUInt32();
            }
            catch(EndOfStreamException)
            {
                break;
            }

            switch(sig)
            {
                case LOCAL_HEADER_SIG:
                case EOCD_SIG: // kludge for strange archives
                {
                    ushort extractVersion = reader.ReadUInt16();
                    ushort flags          = reader.ReadUInt16();
                    ushort method         = reader.ReadUInt16();
                    uint   dosDateTime    = reader.ReadUInt32();
                    uint   crc32          = reader.ReadUInt32();
                    long   compSize       = reader.ReadUInt32();
                    long   uncompSize     = reader.ReadUInt32();
                    ushort nameLength     = reader.ReadUInt16();
                    ushort extraLength    = reader.ReadUInt16();

                    long dataOffset = _stream.Position + nameLength + extraLength;

                    byte[] nameBytes = nameLength > 0 ? reader.ReadBytes(nameLength) : null;

                    var entry = new Entry
                    {
                        Method           = (CompressionMethod)method,
                        Flags            = flags,
                        Crc32            = crc32,
                        CompressedSize   = compSize,
                        UncompressedSize = uncompSize,
                        LastWriteTime    = DosToDateTime(dosDateTime),
                        IsEncrypted      = (flags & FLAG_ENCRYPTED) != 0
                    };

                    // Parse extra fields
                    if(extraLength > 0)
                    {
                        long extraStart = _stream.Position;

                        // ZIP64 fields may override sizes
                        ParseExtraFields(reader, extraLength, ref entry);

                        _stream.Position = extraStart + extraLength;
                    }

                    // If data descriptor flag is set, we need to find the sizes after the data
                    if((flags & FLAG_DATA_DESCRIPTOR) != 0)
                    {
                        // Skip to the data descriptor after compressed data
                        // For streaming archives, scan for the next signature
                        FindDataDescriptor(reader, ref entry);
                    }

                    // Decode filename
                    if(nameBytes is not null)
                    {
                        entry.Filename = (flags & FLAG_UTF8) != 0
                                             ? Encoding.UTF8.GetString(nameBytes)
                                             : _encoding.GetString(nameBytes);

                        entry.Filename = entry.Filename.Replace('\\', '/');

                        if(entry.Filename.EndsWith('/') && entry.UncompressedSize == 0) entry.IsDirectory = true;
                    }

                    entry.DataOffset = dataOffset;

                    long nextPos = (flags & FLAG_DATA_DESCRIPTOR) != 0
                                       ? _stream.Position
                                       : dataOffset + entry.CompressedSize;

                    _entries.Add(entry);

                    _stream.Position = nextPos;

                    break;
                }

                case CENTRAL_DIR_SIG:
                    // Hit central directory — stop scanning
                    return;

                default:
                    // Try to find the next valid entry
                    if(!FindNextLocalHeader(reader)) return;

                    break;
            }
        }
    }

    void ParseExtraFields(BinaryReader reader, int length, ref Entry entry)
    {
        long end = _stream.Position + length;

        while(_stream.Position + 4 <= end)
        {
            ushort extId   = reader.ReadUInt16();
            ushort extSize = reader.ReadUInt16();

            if(_stream.Position + extSize > end) break;

            long nextExtra = _stream.Position + extSize;

            switch(extId)
            {
                case EXTRA_ZIP64:
                    if(entry.UncompressedSize == 0xFFFFFFFF && _stream.Position + 8 <= nextExtra)
                        entry.UncompressedSize = reader.ReadInt64();

                    if(entry.CompressedSize == 0xFFFFFFFF && _stream.Position + 8 <= nextExtra)
                        entry.CompressedSize = reader.ReadInt64();

                    break;

                case EXTRA_EXT_TIMESTAMP when extSize >= 1:
                {
                    byte tsFlags = reader.ReadByte();

                    if((tsFlags & 1) != 0 && _stream.Position + 4 <= nextExtra)
                        entry.LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;

                    if((tsFlags & 2) != 0 && _stream.Position + 4 <= nextExtra)
                        entry.LastAccessTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;

                    if((tsFlags & 4) != 0 && _stream.Position + 4 <= nextExtra)
                        entry.CreationTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;

                    break;
                }

                case EXTRA_UNIX1 when extSize >= 8:
                {
                    entry.LastAccessTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;
                    entry.LastWriteTime  = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;

                    if(extSize >= 10) entry.Uid = reader.ReadUInt16();

                    if(extSize >= 12) entry.Gid = reader.ReadUInt16();

                    break;
                }

                case EXTRA_UNIX2 when extSize >= 4:
                {
                    entry.Uid = reader.ReadUInt16();
                    entry.Gid = reader.ReadUInt16();

                    break;
                }

                case EXTRA_UNIX3 when extSize >= 4:
                {
                    byte version = reader.ReadByte();

                    if(version == 1)
                    {
                        byte uidSize = reader.ReadByte();

                        entry.Uid = uidSize switch
                                    {
                                        2 => reader.ReadUInt16(),
                                        4 => reader.ReadUInt32(),
                                        _ => entry.Uid
                                    };

                        if(uidSize != 2 && uidSize != 4) _stream.Position += uidSize;

                        byte gidSize = reader.ReadByte();

                        entry.Gid = gidSize switch
                                    {
                                        2 => reader.ReadUInt16(),
                                        4 => reader.ReadUInt32(),
                                        _ => entry.Gid
                                    };

                        if(gidSize != 2 && gidSize != 4) _stream.Position += gidSize;
                    }

                    break;
                }

                case EXTRA_UNICODE_PATH when extSize >= 6:
                {
                    byte unicodeVersion = reader.ReadByte();

                    if(unicodeVersion == 1)
                    {
                        uint nameCrc = reader.ReadUInt32();

                        int unicodeLen = extSize - 5;

                        if(unicodeLen > 0)
                        {
                            byte[] unicodeBytes = reader.ReadBytes(unicodeLen);

                            // Strip trailing null bytes
                            int trimLen = unicodeLen;

                            while(trimLen > 0 && unicodeBytes[trimLen - 1] == 0) trimLen--;

                            if(trimLen > 0)
                            {
                                string unicodeName = Encoding.UTF8.GetString(unicodeBytes, 0, trimLen);

                                // Normalize path separators
                                unicodeName    = unicodeName.Replace('\\', '/');
                                entry.Filename = unicodeName;
                            }
                        }
                    }

                    break;
                }

                case EXTRA_UNICODE_COMMENT when extSize >= 6:
                {
                    byte ucVersion = reader.ReadByte();

                    if(ucVersion == 1)
                    {
                        reader.ReadUInt32(); // CRC32 of original comment

                        int ucLen = extSize - 5;

                        if(ucLen > 0)
                        {
                            byte[] ucBytes = reader.ReadBytes(ucLen);

                            int trimLen = ucLen;

                            while(trimLen > 0 && ucBytes[trimLen - 1] == 0) trimLen--;

                            if(trimLen > 0) entry.UnicodeComment = Encoding.UTF8.GetString(ucBytes, 0, trimLen);
                        }
                    }

                    break;
                }

                case EXTRA_WINZIP_AES when extSize >= 7:
                {
                    ushort aesVersion = reader.ReadUInt16();
                    ushort aesVendor  = reader.ReadUInt16();
                    byte   aesKeySize = reader.ReadByte();
                    ushort aesMethod  = reader.ReadUInt16();

                    // The actual compression method is stored in the AES extra field
                    if(entry.Method == CompressionMethod.WinZipAes) entry.Method = (CompressionMethod)aesMethod;

                    entry.IsEncrypted = true;

                    break;
                }

                case EXTRA_NTFS when extSize >= 8:
                {
                    reader.ReadUInt32(); // reserved

                    long tagEnd = nextExtra;

                    while(_stream.Position + 4 <= tagEnd)
                    {
                        ushort tag     = reader.ReadUInt16();
                        ushort tagSize = reader.ReadUInt16();

                        if(_stream.Position + tagSize > tagEnd) break;

                        if(tag == 0x0001 && tagSize >= 24)
                        {
                            long mtime = reader.ReadInt64();
                            long atime = reader.ReadInt64();
                            long ctime = reader.ReadInt64();

                            try
                            {
                                entry.LastWriteTime  = DateTime.FromFileTimeUtc(mtime);
                                entry.LastAccessTime = DateTime.FromFileTimeUtc(atime);
                                entry.CreationTime   = DateTime.FromFileTimeUtc(ctime);
                            }
                            catch(ArgumentOutOfRangeException)
                            {
                                // Invalid FILETIME values
                            }
                        }
                        else
                            _stream.Position += tagSize;
                    }

                    break;
                }

                case EXTRA_PKWARE_UNIX when extSize >= 12:
                {
                    entry.LastAccessTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;
                    entry.LastWriteTime  = DateTimeOffset.FromUnixTimeSeconds(reader.ReadInt32()).DateTime;
                    entry.Uid            = reader.ReadUInt16();
                    entry.Gid            = reader.ReadUInt16();

                    int remaining = extSize - 12;

                    if(remaining > 0)
                    {
                        // Determine if symlink or device from Unix mode in external attributes
                        var fileType = (ushort)(entry.UnixPermissions & S_IFMT);

                        if(fileType == S_IFLNK && remaining > 0)
                        {
                            byte[] linkTarget = reader.ReadBytes(remaining);
                            entry.SymlinkTarget = Encoding.UTF8.GetString(linkTarget);
                            entry.IsSymlink     = true;
                        }
                        else if((fileType == S_IFBLK || fileType == S_IFCHR) && remaining >= 8)
                        {
                            uint major = reader.ReadUInt32();
                            uint minor = reader.ReadUInt32();
                            entry.DeviceNo = (ulong)major << 32 | minor;
                        }
                    }

                    break;
                }

                case EXTRA_ASI_UNIX when extSize >= 12:
                {
                    reader.ReadUInt32(); // CRC32

                    ushort mode   = reader.ReadUInt16();
                    uint   sizDev = reader.ReadUInt32();
                    entry.Uid = reader.ReadUInt16();
                    entry.Gid = reader.ReadUInt16();

                    entry.UnixPermissions = mode;

                    var fileType = (ushort)(mode & S_IFMT);

                    if(fileType == S_IFLNK)
                    {
                        int linkLen = extSize - 12;

                        if(linkLen > 0)
                        {
                            byte[] linkTarget = reader.ReadBytes(linkLen);
                            entry.SymlinkTarget = Encoding.UTF8.GetString(linkTarget);
                        }

                        entry.IsSymlink = true;
                    }
                    else if(fileType == S_IFBLK || fileType == S_IFCHR) entry.DeviceNo = sizDev;

                    break;
                }

                case EXTRA_MAC_OLD when extSize >= 30:
                {
                    byte[] sigBytes = reader.ReadBytes(4);
                    uint   macSig   = BinaryPrimitives.ReadUInt32BigEndian(sigBytes);

                    // "JLEE" in big-endian = 0x4A4C4545
                    if(macSig == 0x4A4C4545)
                    {
                        byte[] finfo = reader.ReadBytes(16);

                        byte[] crDatBytes = reader.ReadBytes(4);
                        byte[] mdDatBytes = reader.ReadBytes(4);
                        uint   crDat      = BinaryPrimitives.ReadUInt32BigEndian(crDatBytes);
                        uint   mdDat      = BinaryPrimitives.ReadUInt32BigEndian(mdDatBytes);

                        byte[] flagsBytes = reader.ReadBytes(4);
                        uint   macFlags   = BinaryPrimitives.ReadUInt32BigEndian(flagsBytes);

                        if(crDat != 0) entry.CreationTime  = _macEpoch.AddSeconds(crDat);
                        if(mdDat != 0) entry.LastWriteTime = _macEpoch.AddSeconds(mdDat);

                        // FinderInfo: 16-byte FInfo (pad to 32 bytes with zeros for FXInfo)
                        entry.FinderInfo = new byte[32];
                        Array.Copy(finfo, 0, entry.FinderInfo, 0, 16);

                        // Extract file type and creator from FInfo (big-endian)
                        if(finfo[0] != 0 || finfo[1] != 0 || finfo[2] != 0 || finfo[3] != 0)
                            entry.MacFileType = Encoding.ASCII.GetString(finfo, 0, 4);

                        if(finfo[4] != 0 || finfo[5] != 0 || finfo[6] != 0 || finfo[7] != 0)
                            entry.MacCreator = Encoding.ASCII.GetString(finfo, 4, 4);

                        // Bit 0 of flags: 1=data fork, 0=resource fork
                        if((macFlags & 1) == 0)
                        {
                            // This entry is a resource fork — store the entire file data as resource fork
                            // The resource fork data is in the main file entry data, not in this extra field
                            // Mark it via the FinderInfo; actual data extraction handled by GetEntry
                        }
                    }

                    break;
                }

                case EXTRA_ZIPIT_MAC1 when extSize >= 9:
                {
                    byte[] sigBytes = reader.ReadBytes(4);
                    uint   zpitSig  = BinaryPrimitives.ReadUInt32BigEndian(sigBytes);

                    // "ZPIT" = 0x5A504954
                    if(zpitSig == 0x5A504954)
                    {
                        byte fnLen = reader.ReadByte();

                        if(fnLen > 0 && _stream.Position + fnLen <= nextExtra) reader.ReadBytes(fnLen); // skip filename

                        if(_stream.Position + 8 <= nextExtra)
                        {
                            byte[] ftBytes = reader.ReadBytes(4);
                            byte[] crBytes = reader.ReadBytes(4);

                            if(ftBytes[0] != 0 || ftBytes[1] != 0 || ftBytes[2] != 0 || ftBytes[3] != 0)
                                entry.MacFileType = Encoding.ASCII.GetString(ftBytes);

                            if(crBytes[0] != 0 || crBytes[1] != 0 || crBytes[2] != 0 || crBytes[3] != 0)
                                entry.MacCreator = Encoding.ASCII.GetString(crBytes);
                        }
                    }

                    break;
                }

                case EXTRA_ZIPIT_MAC2 when extSize >= 12:
                {
                    byte[] sigBytes = reader.ReadBytes(4);
                    uint   zpitSig  = BinaryPrimitives.ReadUInt32BigEndian(sigBytes);

                    // "ZPIT" = 0x5A504954
                    if(zpitSig == 0x5A504954)
                    {
                        byte[] ftBytes = reader.ReadBytes(4);
                        byte[] crBytes = reader.ReadBytes(4);

                        if(ftBytes[0] != 0 || ftBytes[1] != 0 || ftBytes[2] != 0 || ftBytes[3] != 0)
                            entry.MacFileType = Encoding.ASCII.GetString(ftBytes);

                        if(crBytes[0] != 0 || crBytes[1] != 0 || crBytes[2] != 0 || crBytes[3] != 0)
                            entry.MacCreator = Encoding.ASCII.GetString(crBytes);
                    }

                    break;
                }

                case EXTRA_MAC_NEW when extSize >= 14:
                {
                    uint   bSize   = reader.ReadUInt32();
                    ushort mFlags  = reader.ReadUInt16();
                    byte[] ftBytes = reader.ReadBytes(4);
                    byte[] crBytes = reader.ReadBytes(4);

                    if(ftBytes[0] != 0 || ftBytes[1] != 0 || ftBytes[2] != 0 || ftBytes[3] != 0)
                        entry.MacFileType = Encoding.ASCII.GetString(ftBytes);

                    if(crBytes[0] != 0 || crBytes[1] != 0 || crBytes[2] != 0 || crBytes[3] != 0)
                        entry.MacCreator = Encoding.ASCII.GetString(crBytes);

                    // Parse local header attributes if present
                    if(extSize > 14)
                    {
                        bool uncompressed = (mFlags & 0x04) != 0;

                        byte[] attribData;

                        if(uncompressed)
                        {
                            var attribLen = (int)(nextExtra - _stream.Position);

                            if(attribLen > 0)
                                attribData = reader.ReadBytes(attribLen);
                            else
                                attribData = null;
                        }
                        else if(_stream.Position + 6 <= nextExtra)
                        {
                            ushort cType     = reader.ReadUInt16();
                            uint   attribCrc = reader.ReadUInt32();

                            var compLen = (int)(nextExtra - _stream.Position);

                            if(compLen > 0 && bSize > 0)
                                attribData = DecompressExtraFieldData(reader.ReadBytes(compLen), bSize, cType);
                            else
                                attribData = null;
                        }
                        else
                            attribData = null;

                        if(attribData is not null && attribData.Length >= 30)
                            ParseMac3Attributes(attribData, mFlags, ref entry);
                    }

                    break;
                }

                case EXTRA_SMARTZIP_MAC when extSize >= 64:
                {
                    byte[] sigBytes = reader.ReadBytes(4);
                    uint   smartSig = BinaryPrimitives.ReadUInt32BigEndian(sigBytes);

                    // "dZip" = 0x645A6970
                    if(smartSig == 0x645A6970)
                    {
                        byte[] ftBytes = reader.ReadBytes(4);
                        byte[] crBytes = reader.ReadBytes(4);

                        if(ftBytes[0] != 0 || ftBytes[1] != 0 || ftBytes[2] != 0 || ftBytes[3] != 0)
                            entry.MacFileType = Encoding.ASCII.GetString(ftBytes);

                        if(crBytes[0] != 0 || crBytes[1] != 0 || crBytes[2] != 0 || crBytes[3] != 0)
                            entry.MacCreator = Encoding.ASCII.GetString(crBytes);

                        // Rest is big-endian finder flags, location, folder, timestamps, filename
                        // Build partial FinderInfo from available data
                        byte[] fdFlagsBytes = reader.ReadBytes(2);
                        byte[] fdLocV       = reader.ReadBytes(2);
                        byte[] fdLocH       = reader.ReadBytes(2);
                        byte[] fdFldr       = reader.ReadBytes(2);

                        byte[] crDatBytes = reader.ReadBytes(4);
                        byte[] mdDatBytes = reader.ReadBytes(4);
                        uint   crDat      = BinaryPrimitives.ReadUInt32BigEndian(crDatBytes);
                        uint   mdDat      = BinaryPrimitives.ReadUInt32BigEndian(mdDatBytes);

                        if(crDat != 0) entry.CreationTime  = _macEpoch.AddSeconds(crDat);
                        if(mdDat != 0) entry.LastWriteTime = _macEpoch.AddSeconds(mdDat);

                        // Build 32-byte FinderInfo: FInfo(16) + FXInfo(16)
                        entry.FinderInfo = new byte[32];

                        // FInfo: fdType(4) + fdCreator(4) + fdFlags(2) + fdLocation(4) + fdFldr(2)
                        Array.Copy(ftBytes,      0, entry.FinderInfo, 0,  4);
                        Array.Copy(crBytes,      0, entry.FinderInfo, 4,  4);
                        Array.Copy(fdFlagsBytes, 0, entry.FinderInfo, 8,  2);
                        Array.Copy(fdLocV,       0, entry.FinderInfo, 10, 2);
                        Array.Copy(fdLocH,       0, entry.FinderInfo, 12, 2);
                        Array.Copy(fdFldr,       0, entry.FinderInfo, 14, 2);
                    }

                    break;
                }

                case EXTRA_OS2_EA when extSize >= 10:
                {
                    uint   bSize   = reader.ReadUInt32();
                    ushort cType   = reader.ReadUInt16();
                    uint   eaCrc   = reader.ReadUInt32();
                    var    compLen = (int)(nextExtra - _stream.Position);

                    if(compLen > 0 && bSize > 0)
                    {
                        byte[] eaData = DecompressExtraFieldData(reader.ReadBytes(compLen), bSize, cType);

                        if(eaData is not null) entry.Os2Eas = ParseFea2List(eaData);
                    }

                    break;
                }

                case EXTRA_OS2_ACL when extSize >= 10:
                {
                    uint   bSize   = reader.ReadUInt32();
                    ushort cType   = reader.ReadUInt16();
                    uint   aclCrc  = reader.ReadUInt32();
                    var    compLen = (int)(nextExtra - _stream.Position);

                    if(compLen > 0 && bSize > 0)
                        entry.Os2Acl = DecompressExtraFieldData(reader.ReadBytes(compLen), bSize, cType);

                    break;
                }

                case EXTRA_NTSD when extSize >= 11:
                {
                    uint   bSize   = reader.ReadUInt32();
                    byte   version = reader.ReadByte();
                    ushort cType   = reader.ReadUInt16();
                    uint   sdCrc   = reader.ReadUInt32();
                    var    compLen = (int)(nextExtra - _stream.Position);

                    if(version == 0 && compLen > 0 && bSize > 0)
                        entry.NtSecurityDescriptor = DecompressExtraFieldData(reader.ReadBytes(compLen), bSize, cType);

                    break;
                }

                case EXTRA_BEOS when extSize >= 5:
                {
                    uint bSize = reader.ReadUInt32();
                    byte flags = reader.ReadByte();

                    byte[] attribData;

                    if((flags & 1) != 0)
                    {
                        // Uncompressed
                        var dataLen = (int)(nextExtra - _stream.Position);

                        attribData = dataLen > 0 ? reader.ReadBytes(dataLen) : null;
                    }
                    else if(_stream.Position + 6 <= nextExtra)
                    {
                        ushort cType   = reader.ReadUInt16();
                        uint   beCrc   = reader.ReadUInt32();
                        var    compLen = (int)(nextExtra - _stream.Position);

                        if(compLen > 0 && bSize > 0)
                            attribData = DecompressExtraFieldData(reader.ReadBytes(compLen), bSize, cType);
                        else
                            attribData = null;
                    }
                    else
                        attribData = null;

                    if(attribData is not null) entry.BeOsAttributes = ParseBeOsAttributes(attribData);

                    break;
                }

                case EXTRA_ACORN when extSize >= 16:
                {
                    byte[] sigBytes = reader.ReadBytes(4);

                    // "ARC0" = 0x41524330
                    if(sigBytes[0] == 0x41 && sigBytes[1] == 0x52 && sigBytes[2] == 0x43 && sigBytes[3] == 0x30)
                    {
                        entry.AcornLoadAddr = reader.ReadBytes(4);
                        entry.AcornExecAddr = reader.ReadBytes(4);
                        entry.AcornAttr     = reader.ReadBytes(4);

                        // skip Zero field (4 bytes)
                    }

                    break;
                }

                case EXTRA_OPENVMS when extSize >= 4:
                {
                    entry.OpenVmsAttributes = reader.ReadBytes(extSize);

                    break;
                }

                case EXTRA_FWKCS_MD5 when extSize >= 19:
                {
                    byte[] sig = reader.ReadBytes(3);

                    // "MD5"
                    if(sig[0] == 0x4D && sig[1] == 0x44 && sig[2] == 0x35) entry.Md5Hash = reader.ReadBytes(16);

                    break;
                }

                case EXTRA_S390_UNCOMP when extSize > 0:
                    entry.S390Attributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_S390_COMP when extSize > 0:
                    entry.S390Attributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_VM_CMS when extSize > 0:
                    entry.VmCmsAttributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_MVS when extSize > 0:
                    entry.MvsAttributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_THEOS_OLD when extSize > 0:
                    entry.TheosAttributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_THEOS when extSize > 0:
                    entry.TheosAttributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_QDOS when extSize > 0:
                    entry.QdosAttributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_TANDEM when extSize > 0:
                    entry.TandemAttributes = reader.ReadBytes(extSize);

                    break;

                case EXTRA_AOS_VS when extSize > 0:
                    entry.AosVsAttributes = reader.ReadBytes(extSize);

                    break;
            }

            _stream.Position = nextExtra;
        }

        _stream.Position = end;
    }

    static byte[] DecompressExtraFieldData(byte[] compressedData, uint uncompressedSize, ushort compressionType)
    {
        if(compressedData is null || compressedData.Length == 0) return null;

        if(compressionType == 0) return compressedData;

        if(compressionType != 8) return null; // Only Deflate supported for extra field sub-compression

        try
        {
            var output = new byte[uncompressedSize];

            using(var ms = new MemoryStream(compressedData))
            {
                using(var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    var totalRead = 0;

                    while(totalRead < output.Length)
                    {
                        int bytesRead = deflate.Read(output, totalRead, output.Length - totalRead);

                        if(bytesRead == 0) break;

                        totalRead += bytesRead;
                    }
                }
            }

            return output;
        }
        catch
        {
            return null;
        }
    }

    static Dictionary<string, byte[]> ParseFea2List(byte[] eaData)
    {
        if(eaData is null || eaData.Length < 4) return null;

        Dictionary<string, byte[]> eas = new();

        var pos = 4; // Skip cbList (4 bytes)

        while(pos + 8 <= eaData.Length) // Minimum FEA2: oNextEntryOffset(4) + fEA(1) + cbName(1) + cbValue(2) = 8
        {
            var oNext = BitConverter.ToUInt32(eaData, pos);

            int entryStart = pos;

            pos += 4; // oNextEntryOffset
            pos++;    // fEA (flags)

            byte cbName = eaData[pos++];

            var cbValue = BitConverter.ToUInt16(eaData, pos);
            pos += 2;

            if(pos + cbName >= eaData.Length) break;

            string name = Encoding.ASCII.GetString(eaData, pos, cbName);
            pos += cbName;
            pos++; // null terminator

            if(pos + cbValue > eaData.Length) break;

            var data = new byte[cbValue];

            if(cbValue > 0) Array.Copy(eaData, pos, data, 0, cbValue);

            // OS/2 system attribute name normalization (consistent with FAT/HPFS plugins)
            if(name.Length > 0 && name[0] == '.')
                name = name == ".CLASSINFO" ? "com.ibm.os2.classinfo" : "com.microsoft.os2" + name.ToLower();

            eas[name] = data;

            // Navigate to next entry
            if(oNext == 0) break; // Last entry

            pos = entryStart + (int)oNext;
        }

        return eas.Count > 0 ? eas : null;
    }

    static Dictionary<string, byte[]> ParseBeOsAttributes(byte[] data)
    {
        if(data is null || data.Length < 1) return null;

        Dictionary<string, byte[]> attrs = new();

        var pos = 0;

        while(pos < data.Length)
        {
            // Find null-terminated attribute name
            int nameStart = pos;

            while(pos < data.Length && data[pos] != 0) pos++;

            if(pos >= data.Length) break;

            string name = Encoding.UTF8.GetString(data, nameStart, pos - nameStart);
            pos++; // skip null terminator

            if(pos + 12 > data.Length) break; // Need at least Type(4) + Size(8)

            // Type (4 bytes, big-endian) — stored but not used for naming
            pos += 4;

            // Size (8 bytes, big-endian)
            ulong size = BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(pos));
            pos += 8;

            if(size > (ulong)(data.Length - pos)) break;

            var attrData = new byte[size];

            if(size > 0) Array.Copy(data, pos, attrData, 0, (int)size);

            pos += (int)size;

            if(name.Length > 0) attrs[name] = attrData;
        }

        return attrs.Count > 0 ? attrs : null;
    }

    void ParseMac3Attributes(byte[] data, ushort mFlags, ref Entry entry)
    {
        if(data is null || data.Length < 30) return;

        var pos = 0;

        // fdFlags (2 bytes LE)
        var fdFlags = BitConverter.ToUInt16(data, pos);
        pos += 2;

        // fdLocation.v (2), fdLocation.h (2)
        var fdLocV = new byte[2];
        var fdLocH = new byte[2];
        Array.Copy(data, pos, fdLocV, 0, 2);
        pos += 2;
        Array.Copy(data, pos, fdLocH, 0, 2);
        pos += 2;

        // fdFldr (2)
        var fdFldr = new byte[2];
        Array.Copy(data, pos, fdFldr, 0, 2);
        pos += 2;

        // FXInfo (16 bytes)
        var fxInfo = new byte[16];
        Array.Copy(data, pos, fxInfo, 0, 16);
        pos += 16;

        pos++; // FVersNum
        pos++; // ACUser

        // Timestamps (4 bytes each, Mac local time)
        if(pos + 12 > data.Length) return;

        var flCrDat = BitConverter.ToUInt32(data, pos);
        pos += 4;
        var flMdDat = BitConverter.ToUInt32(data, pos);
        pos += 4;
        var flBkDat = BitConverter.ToUInt32(data, pos);
        pos += 4;

        bool hasTimezones = (mFlags & 0x10) == 0;

        int crGmtOff = 0, mdGmtOff = 0, bkGmtOff = 0;

        if(hasTimezones && pos + 12 <= data.Length)
        {
            crGmtOff =  BitConverter.ToInt32(data, pos);
            pos      += 4;
            mdGmtOff =  BitConverter.ToInt32(data, pos);
            pos      += 4;
            bkGmtOff =  BitConverter.ToInt32(data, pos);
            pos      += 4;
        }

        if(flCrDat != 0)
        {
            DateTime crLocal = _macEpoch.AddSeconds(flCrDat);
            entry.CreationTime = hasTimezones ? crLocal.AddSeconds(-crGmtOff) : crLocal;
        }

        if(flMdDat != 0)
        {
            DateTime mdLocal = _macEpoch.AddSeconds(flMdDat);
            entry.LastWriteTime = hasTimezones ? mdLocal.AddSeconds(-mdGmtOff) : mdLocal;
        }

        if(flBkDat != 0)
        {
            DateTime bkLocal = _macEpoch.AddSeconds(flBkDat);
            entry.BackupTime = hasTimezones ? bkLocal.AddSeconds(-bkGmtOff) : bkLocal;
        }

        // Build 32-byte FinderInfo: FInfo(16) + FXInfo(16)
        entry.FinderInfo = new byte[32];

        // FInfo: fdType(4) + fdCreator(4) already in entry.MacFileType/MacCreator
        if(entry.MacFileType is not null) Encoding.ASCII.GetBytes(entry.MacFileType, 0, 4, entry.FinderInfo, 0);

        if(entry.MacCreator is not null) Encoding.ASCII.GetBytes(entry.MacCreator, 0, 4, entry.FinderInfo, 4);

        // fdFlags at offset 8 in FInfo
        entry.FinderInfo[8] = (byte)(fdFlags >> 8);
        entry.FinderInfo[9] = (byte)(fdFlags & 0xFF);

        // fdLocation at offset 10
        Array.Copy(fdLocV, 0, entry.FinderInfo, 10, 2);
        Array.Copy(fdLocH, 0, entry.FinderInfo, 12, 2);

        // fdFldr at offset 14
        Array.Copy(fdFldr, 0, entry.FinderInfo, 14, 2);

        // FXInfo at offset 16
        Array.Copy(fxInfo, 0, entry.FinderInfo, 16, 16);
    }

    static bool EntryHasXattrs(Entry entry) => entry.Comment is not null              ||
                                               entry.UnicodeComment is not null       ||
                                               entry.ResourceFork is not null         ||
                                               entry.FinderInfo is not null           ||
                                               entry.MacFileType is not null          ||
                                               entry.MacCreator is not null           ||
                                               entry.NtSecurityDescriptor is not null ||
                                               entry.Os2Eas is not null               ||
                                               entry.Os2Acl is not null               ||
                                               entry.BeOsAttributes is not null       ||
                                               entry.AcornLoadAddr is not null        ||
                                               entry.OpenVmsAttributes is not null    ||
                                               entry.Md5Hash is not null              ||
                                               entry.S390Attributes is not null       ||
                                               entry.VmCmsAttributes is not null      ||
                                               entry.MvsAttributes is not null        ||
                                               entry.TheosAttributes is not null      ||
                                               entry.QdosAttributes is not null       ||
                                               entry.TandemAttributes is not null     ||
                                               entry.AosVsAttributes is not null;

    void FindDataDescriptor(BinaryReader reader, ref Entry entry)
    {
        // When the data descriptor flag is set, the CRC and sizes follow the compressed data.
        // We scan for the data descriptor signature or the next local header/central directory.
        long startPos = _stream.Position + entry.CompressedSize;

        if(startPos >= _stream.Length) return;

        _stream.Position = startPos;

        // Try to read a data descriptor at the expected position
        if(_stream.Position + 16 <= _stream.Length)
        {
            uint possibleSig = reader.ReadUInt32();

            if(possibleSig == DATA_DESCRIPTOR_SIG)
            {
                // Data descriptor with signature
                entry.Crc32            = reader.ReadUInt32();
                entry.CompressedSize   = reader.ReadUInt32();
                entry.UncompressedSize = reader.ReadUInt32();

                // Check for ZIP64 data descriptor (8-byte sizes)
                if(entry.CompressedSize == 0xFFFFFFFF || entry.UncompressedSize == 0xFFFFFFFF)
                {
                    _stream.Position       -= 8;
                    entry.CompressedSize   =  reader.ReadInt64();
                    entry.UncompressedSize =  reader.ReadInt64();
                }
            }
            else
            {
                // Data descriptor without signature (CRC directly)
                entry.Crc32            = possibleSig;
                entry.CompressedSize   = reader.ReadUInt32();
                entry.UncompressedSize = reader.ReadUInt32();
            }
        }
    }

    bool FindNextLocalHeader(BinaryReader reader)
    {
        // Scan forward for the next PK signature
        while(_stream.Position < _stream.Length - 4)
        {
            byte b = reader.ReadByte();

            if(b != 0x50) continue;

            if(_stream.Position >= _stream.Length - 3) return false;

            byte b2 = reader.ReadByte();

            if(b2 != 0x4B)
            {
                _stream.Position--;

                continue;
            }

            byte b3 = reader.ReadByte();
            byte b4 = reader.ReadByte();

            // Local header, central directory, or EOCD
            if(b3 == 0x03 && b4 == 0x04 || b3 == 0x01 && b4 == 0x02 || b3 == 0x05 && b4 == 0x06)
            {
                _stream.Position -= 4;

                return true;
            }
        }

        return false;
    }

    static DateTime DosToDateTime(uint dosDateTime)
    {
        var time = (int)(dosDateTime & 0xFFFF);
        var date = (int)(dosDateTime >> 16);

        int second = (time & 0x1F) * 2;
        int minute = time >> 5  & 0x3F;
        int hour   = time >> 11 & 0x1F;

        int day   = date      & 0x1F;
        int month = date >> 5 & 0x0F;
        int year  = (date >> 9 & 0x7F) + 1980;

        // Validate ranges
        if(month < 1) month  = 1;
        if(month > 12) month = 12;
        if(day   < 1) day    = 1;

        if(day > DateTime.DaysInMonth(year, month)) day = DateTime.DaysInMonth(year, month);

        if(hour   > 23) hour   = 23;
        if(minute > 59) minute = 59;
        if(second > 59) second = 59;

        try
        {
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
        }
        catch(ArgumentOutOfRangeException)
        {
            return default(DateTime);
        }
    }

    static uint ComputeCrc32(byte[] data)
    {
        var crc = 0xFFFFFFFF;

        foreach(byte b in data)
        {
            crc ^= b;

            for(var j = 0; j < 8; j++)
            {
                if((crc & 1) != 0)
                    crc = crc >> 1 ^ CRC_POLY;
                else
                    crc >>= 1;
            }
        }

        return crc ^ 0xFFFFFFFF;
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter is null || filter.DataForkLength < MIN_FILE_SIZE) return ErrorNumber.InvalidArgument;

        _stream   = filter.GetDataForkStream();
        _encoding = encoding ?? Encoding.GetEncoding("ibm437");
        _entries  = [];

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.SupportsCompression;

        long eocdOffset     = -1;
        long zip64LocOffset = -1;

        FindEndOfCentralDirectory(out eocdOffset, out zip64LocOffset);

        if(eocdOffset >= 0)
            ParseCentralDirectory(eocdOffset, zip64LocOffset);
        else
            ParseWithoutCentralDirectory();

        // Detect features from parsed entries
        foreach(Entry entry in _entries)
        {
            if(entry.IsDirectory)
            {
                _features |= ArchiveSupportedFeature.SupportsSubdirectories |
                             ArchiveSupportedFeature.HasExplicitDirectories;
            }

            if(entry.LastWriteTime != default(DateTime)) _features |= ArchiveSupportedFeature.HasEntryTimestamp;

            if(EntryHasXattrs(entry)) _features |= ArchiveSupportedFeature.SupportsXAttrs;

            if(entry.IsEncrypted) _features |= ArchiveSupportedFeature.SupportsProtection;
        }

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        _entries = null;
        Opened   = false;
    }

#endregion
}