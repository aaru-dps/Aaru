// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Tar plugin.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Tar
{
    List<Entry> _entries;
    TarFormat   _format;

    static TarFormat DetectFormat(byte[] header)
    {
        // Check for USTAR magic "ustar\0" at offset 257 with version "00" at offset 263
        bool isUstar = header[257] == 'u'  &&
                       header[258] == 's'  &&
                       header[259] == 't'  &&
                       header[260] == 'a'  &&
                       header[261] == 'r'  &&
                       header[262] == '\0' &&
                       header[263] == '0'  &&
                       header[264] == '0';

        // Check for GNU magic "ustar  \0" at offset 257
        bool isGnu = header[257] == 'u' &&
                     header[258] == 's' &&
                     header[259] == 't' &&
                     header[260] == 'a' &&
                     header[261] == 'r' &&
                     header[262] == ' ' &&
                     header[263] == ' ' &&
                     header[264] == '\0';

        if(isUstar)
        {
            // Check for STAR extended magic "tar\0" at offset 508
            bool isStar = header[508] == 't' && header[509] == 'a' && header[510] == 'r' && header[511] == '\0';

            return isStar ? TarFormat.Star : TarFormat.Ustar;
        }

        if(isGnu) return TarFormat.Gnu;

        // No magic — check if it has a valid checksum (V7 recognized)
        var storedChecksum = (uint)ReadOctalNumber(header, CHECKSUM_OFFSET, CHECKSUM_LENGTH);

        return ValidateChecksum(header, storedChecksum) ? TarFormat.V7Recognized : TarFormat.V7;
    }

    static long ReadOctalNumber(byte[] buffer, int offset, int length)
    {
        // Check for binary encoding: high bit set on first byte
        if((buffer[offset] & 0x80) == 0x80)
        {
            // Big-endian 64-bit integer stored in the last 8 bytes of the field
            int start = offset + length - 8;

            ulong binValue = BinaryPrimitives.ReadUInt64BigEndian(buffer.AsSpan(start, 8));

            // Reject values that would wrap negative
            return binValue > long.MaxValue ? 0 : (long)binValue;
        }

        // Parse as octal ASCII string
        long result = 0;

        for(int i = offset; i < offset + length; i++)
        {
            byte b = buffer[i];

            // Skip leading spaces and nulls
            if(b is (byte)' ' or 0)
            {
                if(result > 0) break; // Trailing space/null — we're done

                continue;
            }

            // Not an octal digit — stop
            if(b < '0' || b > '7') break;

            result = result * 8 + (b - '0');
        }

        return result;
    }

    static bool ValidateChecksum(byte[] header, uint storedChecksum)
    {
        var  signedSum   = 0;
        uint unsignedSum = 0;

        for(var i = 0; i < BLOCK_SIZE; i++)
        {
            if(i >= CHECKSUM_OFFSET && i < CHECKSUM_OFFSET + CHECKSUM_LENGTH)
            {
                // Treat checksum field as 8 spaces
                signedSum   += ' ';
                unsignedSum += ' ';
            }
            else
            {
                signedSum   += (sbyte)header[i];
                unsignedSum += header[i];
            }
        }

        return storedChecksum == (uint)signedSum || storedChecksum == unsignedSum;
    }

    bool ParseGenericHeader(byte[] header, ref Entry entry)
    {
        // Validate checksum
        var storedChecksum = (uint)ReadOctalNumber(header, CHECKSUM_OFFSET, CHECKSUM_LENGTH);

        if(!ValidateChecksum(header, storedChecksum)) return false;

        entry.Filename       = ReadString(header, NAME_OFFSET, NAME_LENGTH, _encoding);
        entry.Mode           = (uint)ReadOctalNumber(header, MODE_OFFSET, MODE_LENGTH);
        entry.Uid            = (uint)ReadOctalNumber(header, UID_OFFSET,  UID_LENGTH);
        entry.Gid            = (uint)ReadOctalNumber(header, GID_OFFSET,  GID_LENGTH);
        entry.Size           = ReadOctalNumber(header, SIZE_OFFSET, SIZE_LENGTH);
        entry.CompressedSize = entry.Size + (entry.Size % BLOCK_SIZE == 0 ? 0 : BLOCK_SIZE - entry.Size % BLOCK_SIZE);

        long mtimeSeconds = ReadOctalNumber(header, MTIME_OFFSET, MTIME_LENGTH);

        // Clamp out of range timestamps instead of throwing
        if(mtimeSeconds is < 0 or > MAX_UNIX_TIME) mtimeSeconds = 0;

        entry.LastWriteTimeUtc = DateTimeOffset.FromUnixTimeSeconds(mtimeSeconds).UtcDateTime;

        var typeFlag = (TypeFlag)header[TYPEFLAG_OFFSET];

        // Read link name for hard/symbolic links
        if(typeFlag is TypeFlag.HardLink or TypeFlag.SymLink)
            entry.LinkTarget = ReadString(header, LINKNAME_OFFSET, LINKNAME_LENGTH, _encoding);

        AaruLogging.Debug(MODULE_NAME, "[blue]entry.Filename[/] = [green]\"{0}\"[/]", entry.Filename);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.Mode[/] = [teal]0{0}[/]", Convert.ToString(entry.Mode, 8));
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.Uid[/] = [teal]{0}[/]", entry.Uid);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.Gid[/] = [teal]{0}[/]", entry.Gid);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.Size[/] = [teal]{0}[/]", entry.Size);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.LastWriteTimeUtc[/] = [teal]{0}[/]", entry.LastWriteTimeUtc);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.TypeFlag[/] = [teal]{0} (0x{1:X2})[/]", typeFlag, (byte)typeFlag);

        return true;
    }

    void ParseUstarHeader(byte[] header, ref Entry entry)
    {
        entry.UserName  = ReadString(header, UNAME_OFFSET, UNAME_LENGTH, _encoding);
        entry.GroupName = ReadString(header, GNAME_OFFSET, GNAME_LENGTH, _encoding);

        // Prefix + "/" + name
        string prefix = ReadString(header, PREFIX_OFFSET, PREFIX_LENGTH, _encoding);

        if(!string.IsNullOrEmpty(prefix))
            entry.Filename = prefix + "/" + ReadString(header, NAME_OFFSET, NAME_LENGTH, _encoding);

        AaruLogging.Debug(MODULE_NAME, "[blue]entry.UserName[/] = [green]\"{0}\"[/]",  entry.UserName);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.GroupName[/] = [green]\"{0}\"[/]", entry.GroupName);

        if(!string.IsNullOrEmpty(prefix))
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.Filename (with prefix)[/] = [green]\"{0}\"[/]", entry.Filename);
    }

    static void ParseDeviceFields(byte[] header, ref Entry entry, TypeFlag typeFlag)
    {
        if(typeFlag is not (TypeFlag.CharDevice or TypeFlag.BlockDevice)) return;

        entry.DevMajor = (uint)ReadOctalNumber(header, DEVMAJOR_OFFSET, DEVMAJOR_LENGTH);
        entry.DevMinor = (uint)ReadOctalNumber(header, DEVMINOR_OFFSET, DEVMINOR_LENGTH);
    }

    void ParseGnuHeader(byte[] header, ref Entry entry)
    {
        entry.UserName  = ReadString(header, UNAME_OFFSET, UNAME_LENGTH, _encoding);
        entry.GroupName = ReadString(header, GNAME_OFFSET, GNAME_LENGTH, _encoding);
    }

    void ParseGnuSparseHeader(byte[] header, ref Entry entry)
    {
        ParseGnuHeader(header, ref entry);

        // Real size from offset 483 (12 bytes)
        entry.RealSize      = ReadOctalNumber(header, SPARSE_REALSIZE_OFFSET, SPARSE_REALSIZE_LENGTH);
        entry.IsSparse      = true;
        entry.SparseRegions = [];

        // Parse 4 sparse map entries in the main header
        ParseSparseMapEntries(header, SPARSE_MAP_OFFSET, SPARSE_MAP_ENTRIES, entry.SparseRegions);

        // Check for extended sparse headers
        if(header[SPARSE_EXTENDED_OFFSET] != 0)
        {
            var hasExtended = true;

            while(hasExtended)
            {
                var extBlock = new byte[BLOCK_SIZE];
                int read     = _stream.Read(extBlock, 0, BLOCK_SIZE);

                if(read < BLOCK_SIZE) break;

                ParseSparseMapEntries(extBlock, 0, SPARSE_EXTENDED_ENTRIES, entry.SparseRegions);
                hasExtended = extBlock[SPARSE_EXTENDED_FLAG_OFFSET] != 0;
            }
        }

        AaruLogging.Debug(MODULE_NAME, "[blue]entry.RealSize[/] = [teal]{0}[/]",            entry.RealSize);
        AaruLogging.Debug(MODULE_NAME, "[blue]entry.SparseRegions.Count[/] = [teal]{0}[/]", entry.SparseRegions.Count);
    }

    static void ParseSparseMapEntries(byte[] data, int baseOffset, int numEntries, List<SparseRegion> regions)
    {
        for(var i = 0; i < numEntries; i++)
        {
            int  entryOffset  = baseOffset + i * SPARSE_MAP_ENTRY_SIZE;
            long regionOffset = ReadOctalNumber(data, entryOffset,      12);
            long regionLength = ReadOctalNumber(data, entryOffset + 12, 12);

            if(regionLength == 0 && regionOffset == 0) break;

            regions.Add(new SparseRegion
            {
                Offset = regionOffset,
                Length = regionLength
            });
        }
    }

    void ParsePaxHeader(byte[] data, ref Entry entry)
    {
        var position = 0;
        int length   = data.Length;

        while(position < length)
        {
            // Read record length
            int startPos = position;
            int numEnd   = position;

            while(numEnd < length && data[numEnd] != (byte)' ' && data[numEnd] != 0) numEnd++;

            if(numEnd >= length || numEnd == startPos) break;

            string lenStr = Encoding.ASCII.GetString(data, startPos, numEnd - startPos);

            if(!int.TryParse(lenStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int recordLen) ||
               recordLen <= 0)
                break;

            // The record length includes the length digits, the space, key=value, and newline
            int recordEnd = startPos + recordLen;

            if(recordEnd > length) break;

            // Skip the space after length
            int kvStart = numEnd + 1;

            // Parse key=value (exclude trailing newline)
            int kvEnd = recordEnd - 1;

            if(kvEnd <= kvStart)
            {
                position = recordEnd;

                continue;
            }

            // Find the '=' separator
            int eqPos = kvStart;

            while(eqPos < kvEnd && data[eqPos] != (byte)'=') eqPos++;

            if(eqPos >= kvEnd)
            {
                position = recordEnd;

                continue;
            }

            string key   = Encoding.UTF8.GetString(data, kvStart, eqPos - kvStart);
            string value = Encoding.UTF8.GetString(data, eqPos          + 1, kvEnd - eqPos - 1);

            switch(key)
            {
                case "atime":
                    if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double atime) &&
                       atime is > 0 and < MAX_UNIX_TIME)
                        entry.AccessTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)(atime * 1000)).UtcDateTime;

                    break;
                case "ctime":
                    if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double ctime) &&
                       ctime is > 0 and < MAX_UNIX_TIME)
                        entry.CreationTimeUtc =
                            DateTimeOffset.FromUnixTimeMilliseconds((long)(ctime * 1000)).UtcDateTime;

                    break;
                case "mtime":
                    if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double mtime) &&
                       mtime is > 0 and < MAX_UNIX_TIME)
                        entry.LastWriteTimeUtc =
                            DateTimeOffset.FromUnixTimeMilliseconds((long)(mtime * 1000)).UtcDateTime;

                    break;
                case "uname":
                    entry.UserName = value;

                    break;
                case "gname":
                    entry.GroupName = value;

                    break;
                case "uid":
                    if(uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint uid))
                        entry.Uid = uid;

                    break;
                case "gid":
                    if(uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint gid))
                        entry.Gid = gid;

                    break;
                case "path":
                    entry.Filename = value;

                    break;
                case "linkpath":
                    entry.LinkTarget = value;

                    break;
                case "size":
                    if(long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long size))
                    {
                        entry.Size           = size;
                        entry.CompressedSize = size + (size % BLOCK_SIZE == 0 ? 0 : BLOCK_SIZE - size % BLOCK_SIZE);
                    }

                    break;
                case "comment":
                    entry.Comment =  value;
                    _features     |= ArchiveSupportedFeature.SupportsXAttrs;

                    break;
            }

            position = recordEnd;
        }
    }

    void SetTypeAndAdd(byte[] header, ref Entry entry, TypeFlag typeFlag, byte[] currentGlobalPax)
    {
        switch(_format)
        {
            case TarFormat.Ustar:
            case TarFormat.Star:
                ParseUstarHeader(header, ref entry);

                if(currentGlobalPax is not null) ParsePaxHeader(currentGlobalPax, ref entry);

                ParseDeviceFields(header, ref entry, typeFlag);

                break;
        }

        SetEntryType(ref entry, typeFlag);
        FinalizeEntry(ref entry);
        _entries.Add(entry);
        SkipToNextBlock(_stream.Position, entry.Size);
    }

    static void SetEntryType(ref Entry entry, TypeFlag typeFlag)
    {
        switch(typeFlag)
        {
            case TypeFlag.Directory:
                entry.Type = TypeFlag.Directory;

                break;
            case TypeFlag.SymLink:
                entry.Type = TypeFlag.SymLink;

                break;
            case TypeFlag.HardLink:
                entry.Type = TypeFlag.HardLink;

                break;
            case TypeFlag.CharDevice:
                entry.Type = TypeFlag.CharDevice;

                break;
            case TypeFlag.BlockDevice:
                entry.Type = TypeFlag.BlockDevice;

                break;
            case TypeFlag.Fifo:
                entry.Type = TypeFlag.Fifo;

                break;
            case TypeFlag.GnuSparse:
                entry.Type = TypeFlag.GnuSparse;

                break;
            default:
                entry.Type = TypeFlag.File;

                break;
        }
    }

    void FinalizeEntry(ref Entry entry)
    {
        // Directories have no data
        if(entry.Type == TypeFlag.Directory)
        {
            entry.Size           = 0;
            entry.CompressedSize = 0;

            // Ensure directory names end with '/' for proper path handling
            if(!entry.Filename.EndsWith('/')) entry.Filename += "/";
        }

        // Record data offset
        entry.DataOffset = _stream.Position;

        // For sparse files, RealSize is the actual file size, Size is the on-disk data size
        if(entry.IsSparse && entry.RealSize == 0) entry.RealSize = entry.Size;
    }

    string ReadDataAsString(long size)
    {
        // Bound the allocation by the remaining stream
        size = Math.Clamp(size, 0, Math.Min(_stream.Length - _stream.Position, int.MaxValue));

        var buffer = new byte[size];
        _stream.ReadExactly(buffer, 0, (int)size);

        // Trim trailing null bytes
        int len = buffer.Length;

        while(len > 0 && buffer[len - 1] == 0) len--;

        return _encoding.GetString(buffer, 0, len);
    }

    byte[] ReadDataAsBytes(long size)
    {
        // Bound the allocation by the remaining stream
        size = Math.Clamp(size, 0, Math.Min(_stream.Length - _stream.Position, int.MaxValue));

        var buffer = new byte[size];
        _stream.ReadExactly(buffer, 0, (int)size);

        return buffer;
    }

    byte[] ReadNextHeader()
    {
        if(_stream.Position + BLOCK_SIZE > _stream.Length) return null;

        var header    = new byte[BLOCK_SIZE];
        int bytesRead = _stream.Read(header, 0, BLOCK_SIZE);

        return bytesRead < BLOCK_SIZE ? null : header;
    }

    void SkipToNextBlock(long dataStart, long dataSize)
    {
        long nextOffset = dataStart + dataSize;

        if(nextOffset % BLOCK_SIZE != 0) nextOffset += BLOCK_SIZE - nextOffset % BLOCK_SIZE;

        _stream.Position = nextOffset <= _stream.Length ? nextOffset : _stream.Length;
    }

    static bool IsZeroBlock(byte[] block)
    {
        for(var i = 0; i < BLOCK_SIZE; i++)
        {
            if(block[i] != 0) return false;
        }

        return true;
    }

    static string ReadString(byte[] buffer, int offset, int length, Encoding encoding)
    {
        // Find null terminator
        int end = offset;

        while(end < offset + length && buffer[end] != 0) end++;

        if(end == offset) return string.Empty;

        return encoding.GetString(buffer, offset, end - offset);
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < BLOCK_SIZE) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;
        _encoding        = encoding ?? Encoding.UTF8;

        _features = ArchiveSupportedFeature.SupportsFilenames      |
                    ArchiveSupportedFeature.SupportsSubdirectories |
                    ArchiveSupportedFeature.HasExplicitDirectories |
                    ArchiveSupportedFeature.HasEntryTimestamp;

        _entries = [];
        _format  = TarFormat.V7;

        byte[] currentGlobalPax = null;
        var    formatDetected   = false;

        while(_stream.Position + BLOCK_SIZE <= _stream.Length)
        {
            var header    = new byte[BLOCK_SIZE];
            int bytesRead = _stream.Read(header, 0, BLOCK_SIZE);

            if(bytesRead < BLOCK_SIZE) break;

            // Check for end-of-archive (512 zero bytes)
            if(IsZeroBlock(header)) break;

            // Detect format on first header
            if(!formatDetected)
            {
                _format        = DetectFormat(header);
                formatDetected = true;
            }

            // Parse generic header fields
            var entry = new Entry();

            if(!ParseGenericHeader(header, ref entry)) break;

            var typeFlag = (TypeFlag)header[TYPEFLAG_OFFSET];

            // Kludge: broken directory typeflags — null typeflag but name ends with '/'
            if(typeFlag == TypeFlag.AltFile && entry.Filename.EndsWith('/')) typeFlag = TypeFlag.Directory;

            // Kludge: detect ././@LongLink and force GNU format
            if(entry.Filename.StartsWith(GNU_LONGLINK, StringComparison.Ordinal)) _format = TarFormat.Gnu;

            long dataSize  = entry.Size;
            long dataStart = _stream.Position;

            switch(typeFlag)
            {
                case TypeFlag.GnuLongName:
                {
                    string longName = ReadDataAsString(dataSize);
                    SkipToNextBlock(dataStart, dataSize);

                    // Read the actual file header that follows
                    header = ReadNextHeader();

                    if(header is null) goto done;

                    entry = new Entry();

                    if(!ParseGenericHeader(header, ref entry)) goto done;

                    // Parse GNU-specific fields on the real header
                    ParseGnuHeader(header, ref entry);

                    entry.Filename = longName;

                    typeFlag = (TypeFlag)header[TYPEFLAG_OFFSET];

                    if(typeFlag == TypeFlag.AltFile && entry.Filename.EndsWith('/')) typeFlag = TypeFlag.Directory;

                    // Handle the case where longname is followed by longlink
                    if(typeFlag == TypeFlag.GnuLongLink)
                    {
                        dataSize  = entry.Size;
                        dataStart = _stream.Position;

                        string longLink = ReadDataAsString(dataSize);
                        SkipToNextBlock(dataStart, dataSize);

                        string savedName = entry.Filename;

                        header = ReadNextHeader();

                        if(header is null) goto done;

                        entry = new Entry();

                        if(!ParseGenericHeader(header, ref entry)) goto done;

                        ParseGnuHeader(header, ref entry);

                        entry.Filename   = savedName;
                        entry.LinkTarget = longLink;
                        typeFlag         = (TypeFlag)header[TYPEFLAG_OFFSET];
                    }

                    SetTypeAndAdd(header, ref entry, typeFlag, currentGlobalPax);

                    break;
                }
                case TypeFlag.GnuLongLink:
                {
                    string longLink = ReadDataAsString(dataSize);
                    SkipToNextBlock(dataStart, dataSize);

                    // Read the actual file header that follows
                    header = ReadNextHeader();

                    if(header is null) goto done;

                    entry = new Entry();

                    if(!ParseGenericHeader(header, ref entry)) goto done;

                    ParseGnuHeader(header, ref entry);

                    entry.LinkTarget = longLink;

                    typeFlag = (TypeFlag)header[TYPEFLAG_OFFSET];

                    if(typeFlag == TypeFlag.AltFile && entry.Filename.EndsWith('/')) typeFlag = TypeFlag.Directory;

                    // Handle the case where longlink is followed by longname
                    if(typeFlag == TypeFlag.GnuLongName)
                    {
                        dataSize  = entry.Size;
                        dataStart = _stream.Position;

                        string longName = ReadDataAsString(dataSize);
                        SkipToNextBlock(dataStart, dataSize);

                        string savedLink = entry.LinkTarget;

                        header = ReadNextHeader();

                        if(header is null) goto done;

                        entry = new Entry();

                        if(!ParseGenericHeader(header, ref entry)) goto done;

                        ParseGnuHeader(header, ref entry);

                        entry.LinkTarget = savedLink;
                        entry.Filename   = longName;
                        typeFlag         = (TypeFlag)header[TYPEFLAG_OFFSET];
                    }

                    SetTypeAndAdd(header, ref entry, typeFlag, currentGlobalPax);

                    break;
                }
                case TypeFlag.PaxGlobal:
                {
                    currentGlobalPax = ReadDataAsBytes(dataSize);
                    SkipToNextBlock(dataStart, dataSize);

                    break;
                }
                case TypeFlag.PaxExtended:
                {
                    byte[] paxData = ReadDataAsBytes(dataSize);
                    SkipToNextBlock(dataStart, dataSize);

                    // Read the actual file header that follows
                    header = ReadNextHeader();

                    if(header is null) goto done;

                    entry = new Entry();

                    if(!ParseGenericHeader(header, ref entry)) goto done;

                    ParseUstarHeader(header, ref entry);

                    // Apply global PAX first, then per-file PAX overrides
                    if(currentGlobalPax is not null) ParsePaxHeader(currentGlobalPax, ref entry);

                    ParsePaxHeader(paxData, ref entry);

                    typeFlag = (TypeFlag)header[TYPEFLAG_OFFSET];

                    if(typeFlag == TypeFlag.AltFile && entry.Filename.EndsWith('/')) typeFlag = TypeFlag.Directory;

                    SetEntryType(ref entry, typeFlag);
                    ParseDeviceFields(header, ref entry, typeFlag);
                    FinalizeEntry(ref entry);
                    _entries.Add(entry);

                    SkipToNextBlock(_stream.Position, entry.Size);

                    break;
                }
                case TypeFlag.GnuSparse:
                {
                    ParseGnuSparseHeader(header, ref entry);
                    entry.Type = TypeFlag.GnuSparse;
                    FinalizeEntry(ref entry);
                    _entries.Add(entry);

                    // Data starts after the extended sparse headers, not after the main header
                    SkipToNextBlock(entry.DataOffset, dataSize);

                    break;
                }
                default:
                {
                    switch(_format)
                    {
                        case TarFormat.Ustar:
                        case TarFormat.Star:
                            ParseUstarHeader(header, ref entry);

                            if(currentGlobalPax is not null) ParsePaxHeader(currentGlobalPax, ref entry);

                            ParseDeviceFields(header, ref entry, typeFlag);

                            break;
                        case TarFormat.Gnu:
                            ParseGnuHeader(header, ref entry);

                            break;
                    }

                    SetEntryType(ref entry, typeFlag);
                    FinalizeEntry(ref entry);
                    _entries.Add(entry);
                    SkipToNextBlock(dataStart, dataSize);

                    break;
                }
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
        _stream = null;
        Opened  = false;
    }

#endregion
}