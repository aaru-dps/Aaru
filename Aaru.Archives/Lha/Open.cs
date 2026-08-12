using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Lha
{
    List<Entry> _entries;

    bool ParseLevel0(long headerStart,  byte headerSizeByte, uint      compressedSize, uint uncompressedSize,
                     uint dosTimestamp, byte attrs,          ref Entry entry)
    {
        int headerSize = headerSizeByte + 2;

        // After level byte, read: namelen(1), filename(namelen), crc16(2)
        if(_stream.Position >= _stream.Length) return false;

        int namelen = _stream.ReadByte();

        if(namelen < 0) return false;

        var filename = "";

        if(namelen > 0)
        {
            if(_stream.Position + namelen > _stream.Length) return false;

            var nameBytes = new byte[namelen];
            _stream.ReadExactly(nameBytes, 0, namelen);
            filename = StringHandlers.CToString(nameBytes, _encoding);
        }

        if(_stream.Position + 2 > _stream.Length) return false;

        var crcBytes = new byte[2];
        _stream.ReadExactly(crcBytes, 0, 2);
        entry.Crc16 = BitConverter.ToUInt16(crcBytes, 0);

        // Level 0 uses DOS timestamps
        entry.LastWriteTime = DateHandlers.DosToDateTime((ushort)(dosTimestamp >> 16), (ushort)(dosTimestamp & 0xFFFF));

        entry.UncompressedSize = uncompressedSize;
        entry.CompressedSize   = compressedSize;
        entry.Filename         = filename;

        // Data starts right after the header
        entry.DataOffset = headerStart      + headerSize;
        _stream.Position = entry.DataOffset + compressedSize;

        // Try to separate directory from filename
        SplitPath(ref entry);

        return true;
    }

    bool ParseLevel1(long headerStart,  byte headerSizeByte, uint      compressedSize, uint uncompressedSize,
                     uint dosTimestamp, byte attrs,          ref Entry entry)
    {
        int headerSize = headerSizeByte + 2;

        // After level byte, read: namelen(1), filename(namelen), crc16(2), os(1)
        if(_stream.Position >= _stream.Length) return false;

        int namelen = _stream.ReadByte();

        if(namelen < 0) return false;

        var filename = "";

        if(namelen > 0)
        {
            if(_stream.Position + namelen > _stream.Length) return false;

            var nameBytes = new byte[namelen];
            _stream.ReadExactly(nameBytes, 0, namelen);
            filename = StringHandlers.CToString(nameBytes, _encoding);
        }

        if(_stream.Position + 3 > _stream.Length) return false;

        var tailBytes = new byte[3];
        _stream.ReadExactly(tailBytes, 0, 3);
        entry.Crc16 = BitConverter.ToUInt16(tailBytes, 0);
        entry.Os    = (OsType)tailBytes[2];

        // Level 1 uses DOS timestamps
        entry.LastWriteTime = DateHandlers.DosToDateTime((ushort)(dosTimestamp >> 16), (ushort)(dosTimestamp & 0xFFFF));

        entry.UncompressedSize = uncompressedSize;
        entry.Filename         = filename;

        // Parse extended headers; their sizes are included in compressedSize
        long extHeadersSize = ParseExtendedHeaders16(ref entry);

        // Extended headers cannot be larger than the compressed size that contains them,
        // a negative size would rewind the stream and loop forever
        if(extHeadersSize > compressedSize) return false;

        // Compressed data size is compressedSize minus extended header overhead
        entry.CompressedSize = compressedSize - extHeadersSize;

        // Data starts after extended headers
        entry.DataOffset = _stream.Position;
        _stream.Position = entry.DataOffset + entry.CompressedSize;

        // Try to separate directory from filename
        SplitPath(ref entry);

        return true;
    }

    bool ParseLevel2(long headerStart,   ushort totalHeaderSize, uint      compressedSize, uint uncompressedSize,
                     uint unixTimestamp, byte   attrs,           ref Entry entry)
    {
        // After level byte: crc16(2), os(1), then extended headers
        if(_stream.Position + 3 > _stream.Length) return false;

        var tailBytes = new byte[3];
        _stream.ReadExactly(tailBytes, 0, 3);
        entry.Crc16 = BitConverter.ToUInt16(tailBytes, 0);
        entry.Os    = (OsType)tailBytes[2];

        // Level 2 uses Unix timestamps
        entry.LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;

        entry.UncompressedSize = uncompressedSize;
        entry.CompressedSize   = compressedSize;

        // Parse extended headers within the header area
        long endOfHeader = headerStart + totalHeaderSize;
        ParseExtendedHeaders16(ref entry, endOfHeader);

        // Data starts after the total header
        entry.DataOffset = endOfHeader;
        _stream.Position = entry.DataOffset + compressedSize;

        // Try to separate directory from filename
        SplitPath(ref entry);

        return true;
    }

    bool ParseLevel3(long      headerStart, uint compressedSize, uint uncompressedSize, uint unixTimestamp, byte attrs,
                     ref Entry entry)
    {
        // After level byte: crc16(2), os(1), total_headersize(4)
        if(_stream.Position + 7 > _stream.Length) return false;

        var tailBytes = new byte[7];
        _stream.ReadExactly(tailBytes, 0, 7);
        entry.Crc16 = BitConverter.ToUInt16(tailBytes, 0);
        entry.Os    = (OsType)tailBytes[2];
        var totalHeaderSize = BitConverter.ToUInt32(tailBytes, 3);

        // Level 3 uses Unix timestamps
        entry.LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;

        entry.UncompressedSize = uncompressedSize;
        entry.CompressedSize   = compressedSize;

        // Parse extended headers with 32-bit sizes
        long endOfHeader = headerStart + totalHeaderSize;
        ParseExtendedHeaders32(ref entry, endOfHeader);

        // Data starts after the total header
        entry.DataOffset = endOfHeader;
        _stream.Position = entry.DataOffset + compressedSize;

        // Try to separate directory from filename
        SplitPath(ref entry);

        return true;
    }

    /// <summary>Parse extended headers with 16-bit size fields (levels 1 and 2).</summary>
    /// <returns>Total bytes consumed by extended headers (for level 1 compsize adjustment).</returns>
    long ParseExtendedHeaders16(ref Entry entry, long boundary = long.MaxValue)
    {
        long totalSize = 0;

        while(_stream.Position + 2 <= _stream.Length && _stream.Position + 2 <= boundary)
        {
            var sizeBytes = new byte[2];
            _stream.ReadExactly(sizeBytes, 0, 2);
            var extSize = BitConverter.ToUInt16(sizeBytes, 0);

            if(extSize == 0) break;

            totalSize += extSize;

            // extSize includes the 2-byte size field itself
            int dataLen = extSize - 2;

            if(dataLen <= 0) continue;

            if(_stream.Position + dataLen > _stream.Length) break;

            var extData = new byte[dataLen];
            _stream.ReadExactly(extData, 0, dataLen);

            ParseExtendedHeader(extData, ref entry);
        }

        return totalSize;
    }

    /// <summary>Parse extended headers with 32-bit size fields (level 3).</summary>
    void ParseExtendedHeaders32(ref Entry entry, long boundary)
    {
        while(_stream.Position + 4 <= _stream.Length && _stream.Position + 4 <= boundary)
        {
            var sizeBytes = new byte[4];
            _stream.ReadExactly(sizeBytes, 0, 4);
            var extSize = BitConverter.ToUInt32(sizeBytes, 0);

            if(extSize == 0) break;

            // extSize includes the 4-byte size field itself
            var dataLen = (int)(extSize - 4);

            if(dataLen <= 0) continue;

            if(_stream.Position + dataLen > _stream.Length) break;

            var extData = new byte[dataLen];
            _stream.ReadExactly(extData, 0, dataLen);

            ParseExtendedHeader(extData, ref entry);
        }
    }

    void ParseExtendedHeader(byte[] data, ref Entry entry)
    {
        if(data.Length < 1) return;

        byte type = data[0];
        int  len  = data.Length - 1;

        AaruLogging.Debug(MODULE_NAME,
                          "[navy]extended header type[/] = [teal]0x{0:X2}[/], [navy]len[/] = [teal]{1}[/]",
                          type,
                          len);

        switch(type)
        {
            case EXT_HEADER_CRC:
                // Header CRC — skip validation for now
                break;

            case EXT_FILENAME:
                if(len > 0) entry.Filename = _encoding.GetString(data, 1, len).TrimEnd('\0');

                break;

            case EXT_DIRECTORY:
                if(len > 0)
                {
                    // Directory path with 0xFF as separator
                    var dirBytes = new byte[len];
                    Array.Copy(data, 1, dirBytes, 0, len);

                    // Replace 0xFF separators with '/'
                    for(var i = 0; i < dirBytes.Length; i++)
                    {
                        if(dirBytes[i] == 0xFF) dirBytes[i] = (byte)'/';
                    }

                    entry.DirectoryPath = _encoding.GetString(dirBytes).TrimEnd('\0', '/');
                }

                break;

            case EXT_COMMENT:
            case EXT_COMMENT_ALT:
                if(len > 0) entry.Comment = _encoding.GetString(data, 1, len).TrimEnd('\0');

                break;

            case EXT_DOS_ATTRS:
                if(len >= 2) entry.DosAttributes = data[1];

                break;

            case EXT_WINDOWS_TIMESTAMP:
                if(len >= 24)
                {
                    // 3 Windows FILETIMEs: creation, modification, access
                    var createTime = BitConverter.ToInt64(data, 1);
                    var modTime    = BitConverter.ToInt64(data, 9);
                    var accessTime = BitConverter.ToInt64(data, 17);

                    if(createTime > 0) entry.CreationTime   = DateTime.FromFileTime(createTime);
                    if(modTime    > 0) entry.LastWriteTime  = DateTime.FromFileTime(modTime);
                    if(accessTime > 0) entry.LastAccessTime = DateTime.FromFileTime(accessTime);
                }

                break;

            case EXT_LARGE_FILE:
                AaruLogging.Debug(MODULE_NAME, "[yellow]64-bit file size extension not supported[/]");

                break;

            case EXT_UNIX_PERMS:
                if(len >= 2)
                {
                    entry.UnixPermissions    = BitConverter.ToUInt16(data, 1);
                    entry.HasUnixPermissions = true;
                }

                break;

            case EXT_UNIX_UIDGID:
                if(len >= 4)
                {
                    entry.Gid = BitConverter.ToUInt16(data, 1);
                    entry.Uid = BitConverter.ToUInt16(data, 3);
                }

                break;

            case EXT_UNIX_GROUP_NAME:
                if(len > 0) entry.GroupName = _encoding.GetString(data, 1, len).TrimEnd('\0');

                break;

            case EXT_UNIX_USER_NAME:
                if(len > 0) entry.UserName = _encoding.GetString(data, 1, len).TrimEnd('\0');

                break;

            case EXT_UNIX_TIMESTAMP:
                if(len >= 4)
                {
                    var modUnix = BitConverter.ToUInt32(data, 1);
                    entry.LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(modUnix).DateTime;
                }

                break;

            case EXT_COMBINED_UNIX:
                if(len >= 12)
                {
                    // DOS attrs(2) + POSIX mode(2) + gid(2) + uid(2) + create_time(4) + mod_time(4)
                    // Minimum 12 for timestamps, may have more
                    var pos = 1;

                    // entry.DosAttributes already set from main header
                    if(len >= 2)
                    {
                        entry.UnixPermissions    = BitConverter.ToUInt16(data, pos + 2);
                        entry.HasUnixPermissions = true;
                    }

                    if(len >= 8)
                    {
                        entry.Gid = BitConverter.ToUInt16(data, pos + 4);
                        entry.Uid = BitConverter.ToUInt16(data, pos + 6);
                    }

                    if(len >= 16)
                    {
                        var createUnix = BitConverter.ToUInt32(data, pos + 8);
                        var modUnix    = BitConverter.ToUInt32(data, pos + 12);

                        if(createUnix > 0) entry.CreationTime = DateTimeOffset.FromUnixTimeSeconds(createUnix).DateTime;

                        if(modUnix > 0) entry.LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(modUnix).DateTime;
                    }
                }

                break;

            case EXT_EXTENDED_UNIX:
                if(len >= 20)
                {
                    // POSIX perms(4) + gid(4) + uid(4) + create_time(4) + mod_time(4)
                    var pos = 1;
                    entry.UnixPermissions    = (ushort)(BitConverter.ToUInt32(data, pos) & 0xFFFF);
                    entry.HasUnixPermissions = true;
                    entry.Gid                = (ushort)BitConverter.ToUInt32(data, pos + 4);
                    entry.Uid                = (ushort)BitConverter.ToUInt32(data, pos + 8);

                    var createUnix = BitConverter.ToUInt32(data, pos + 12);
                    var modUnix    = BitConverter.ToUInt32(data, pos + 16);

                    if(createUnix > 0) entry.CreationTime = DateTimeOffset.FromUnixTimeSeconds(createUnix).DateTime;

                    if(modUnix > 0) entry.LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(modUnix).DateTime;
                }

                break;
        }
    }

    static Method ParseMethod(byte[] methodBytes)
    {
        // Method format: -XX?- (5 bytes)
        byte family1 = methodBytes[1];
        byte family2 = methodBytes[2];
        byte method  = methodBytes[3];

        if(family1 == (byte)'l' && family2 == (byte)'h')
        {
            return method switch
                   {
                       (byte)'0' => Method.Stored,
                       (byte)'1' => Method.Lh1,
                       (byte)'2' => Method.Lh2,
                       (byte)'3' => Method.Lh3,
                       (byte)'4' => Method.Lh4,
                       (byte)'5' => Method.Lh5,
                       (byte)'6' => Method.Lh6,
                       (byte)'7' => Method.Lh7,
                       (byte)'d' => Method.Directory,
                       _         => Method.Stored
                   };
        }

        if(family1 == (byte)'l' && family2 == (byte)'z')
        {
            return method switch
                   {
                       (byte)'s' => Method.Lzs,
                       (byte)'5' => Method.Lz5,
                       _         => Method.Stored // lz0, lz4 are stored
                   };
        }

        if(family1 == (byte)'p' && family2 == (byte)'m')
        {
            return method switch
                   {
                       (byte)'1' => Method.Pm1,
                       (byte)'2' => Method.Pm2,
                       _         => Method.Stored // pm0 is stored
                   };
        }

        return Method.Stored;
    }

    /// <summary>Split a filename with an embedded path into DirectoryPath and Filename components.</summary>
    static void SplitPath(ref Entry entry)
    {
        // If directory was already set via extended header, don't split
        if(entry.DirectoryPath is not null) return;

        if(entry.Filename is null) return;

        // Normalize separators
        string normalized = entry.Filename.Replace('\\', '/');

        int lastSlash = normalized.LastIndexOf('/');

        if(lastSlash < 0) return;

        entry.DirectoryPath = normalized[..lastSlash];
        entry.Filename      = normalized[(lastSlash + 1)..];
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < MIN_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;
        _encoding        = encoding ?? Encoding.ASCII;
        _entries         = [];

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        while(_stream.Position < _stream.Length)
        {
            long headerStart = _stream.Position;

            // First byte zero means end of archive
            int firstByte = _stream.ReadByte();

            if(firstByte <= 0) break;

            // Need at least the method field to continue
            if(_stream.Position + 1 >= _stream.Length) break;

            int secondByte = _stream.ReadByte();

            if(secondByte < 0) break;

            // Peek at the method field to validate this is a real entry
            if(_stream.Position + METHOD_LEN > _stream.Length) break;

            var methodBytes = new byte[METHOD_LEN];
            _stream.ReadExactly(methodBytes, 0, METHOD_LEN);

            if(!IsValidMethod(methodBytes, 0))
            {
                AaruLogging.Debug(MODULE_NAME, "[red]Invalid method at offset {0}[/]", headerStart);

                break;
            }

            // Now read the rest of the header depending on level
            // We need to look at the level byte at a fixed offset from the method
            // Method is at offset 2 from header start, so level is at offset 20
            // We've read 7 bytes so far (firstByte + secondByte + method[5])
            // Need 13 more bytes to reach level at offset 20

            // Read compsize(4) + size(4) + time(4) + attrs(1)
            if(_stream.Position + 13 > _stream.Length) break;

            var fixedPart = new byte[13];
            _stream.ReadExactly(fixedPart, 0, 13);

            var  compressedSize   = BitConverter.ToUInt32(fixedPart, 0);
            var  uncompressedSize = BitConverter.ToUInt32(fixedPart, 4);
            var  timestamp        = BitConverter.ToUInt32(fixedPart, 8);
            byte attrs            = fixedPart[12];

            // Read level byte
            if(_stream.Position >= _stream.Length) break;

            int levelByte = _stream.ReadByte();

            if(levelByte < 0) break;

            Method method = ParseMethod(methodBytes);
            var    entry  = new Entry();
            entry.Method      = method;
            entry.Os          = OsType.Generic;
            entry.IsDirectory = method == Method.Directory;

            AaruLogging.Debug(MODULE_NAME, "[navy]level[/] = [teal]{0}[/]", levelByte);

            AaruLogging.Debug(MODULE_NAME,
                              "[navy]method[/] = [green]\"{0}\"[/]",
                              Encoding.ASCII.GetString(methodBytes));

            AaruLogging.Debug(MODULE_NAME, "[navy]compressedSize[/] = [teal]{0}[/]",   compressedSize);
            AaruLogging.Debug(MODULE_NAME, "[navy]uncompressedSize[/] = [teal]{0}[/]", uncompressedSize);
            AaruLogging.Debug(MODULE_NAME, "[navy]timestamp[/] = [teal]{0}[/]",        timestamp);
            AaruLogging.Debug(MODULE_NAME, "[navy]attrs[/] = [teal]0x{0:X2}[/]",       attrs);

            switch(levelByte)
            {
                case 0:
                    if(!ParseLevel0(headerStart,
                                    (byte)firstByte,
                                    compressedSize,
                                    uncompressedSize,
                                    timestamp,
                                    attrs,
                                    ref entry))
                        goto done;

                    break;
                case 1:
                    if(!ParseLevel1(headerStart,
                                    (byte)firstByte,
                                    compressedSize,
                                    uncompressedSize,
                                    timestamp,
                                    attrs,
                                    ref entry))
                        goto done;

                    break;
                case 2:
                    if(!ParseLevel2(headerStart,
                                    (ushort)((byte)firstByte | secondByte << 8),
                                    compressedSize,
                                    uncompressedSize,
                                    timestamp,
                                    attrs,
                                    ref entry))
                        goto done;

                    break;
                case 3:
                    if(!ParseLevel3(headerStart, compressedSize, uncompressedSize, timestamp, attrs, ref entry))
                        goto done;

                    break;
                default:
                    AaruLogging.Debug(MODULE_NAME, "[red]Unknown header level {0}[/]", levelByte);

                    goto done;
            }

            entry.DosAttributes = attrs;

            if(entry.Method != Method.Directory || entry.IsDirectory)
            {
                if(entry.Method != Method.Stored && entry.CompressedSize > 0)
                    _features |= ArchiveSupportedFeature.SupportsCompression;

                if(entry.IsDirectory)
                {
                    _features |= ArchiveSupportedFeature.HasExplicitDirectories |
                                 ArchiveSupportedFeature.SupportsSubdirectories;
                }

                if(entry.Comment is not null) _features |= ArchiveSupportedFeature.SupportsXAttrs;

                _entries.Add(entry);
            }
        }

    done:
        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();
        _entries?.Clear();

        _stream = null;
        Opened  = false;
    }

#endregion
}