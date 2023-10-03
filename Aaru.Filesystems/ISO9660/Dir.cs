// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
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
// Copyright © 2011-2023 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    Dictionary<string, Dictionary<string, DecodedDirectoryEntry>> _directoryCache;

#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(string.IsNullOrWhiteSpace(path) ||
           path == "/")
        {
            node = new Iso9660DirNode
            {
                Path      = path,
                _position = 0,
                _entries  = _rootDirectoryCache.Values.ToArray()
            };

            return ErrorNumber.NoError;
        }

        string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                             ? path[1..].ToLower(CultureInfo.CurrentUICulture)
                             : path.ToLower(CultureInfo.CurrentUICulture);

        if(_directoryCache.TryGetValue(cutPath, out Dictionary<string, DecodedDirectoryEntry> currentDirectory))
        {
            node = new Iso9660DirNode
            {
                Path      = path,
                _position = 0,
                _entries  = currentDirectory.Values.ToArray()
            };

            return ErrorNumber.NoError;
        }

        string[] pieces = cutPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        KeyValuePair<string, DecodedDirectoryEntry> entry =
            _rootDirectoryCache.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[0]);

        if(string.IsNullOrEmpty(entry.Key))
            return ErrorNumber.NoSuchFile;

        if(!entry.Value.Flags.HasFlag(FileFlags.Directory))
            return ErrorNumber.NotDirectory;

        string currentPath = pieces[0];

        currentDirectory = _rootDirectoryCache;

        for(var p = 0; p < pieces.Length; p++)
        {
            entry = currentDirectory.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[p]);

            if(string.IsNullOrEmpty(entry.Key))
                return ErrorNumber.NoSuchFile;

            if(!entry.Value.Flags.HasFlag(FileFlags.Directory))
                return ErrorNumber.NotDirectory;

            currentPath = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";

            if(_directoryCache.TryGetValue(currentPath, out currentDirectory))
                continue;

            if(entry.Value.Extents.Count == 0)
                return ErrorNumber.InvalidArgument;

            currentDirectory = _cdi
                                   ? DecodeCdiDirectory(entry.Value.Extents[0].extent + entry.Value.XattrLength,
                                                        entry.Value.Extents[0].size)
                                   : _highSierra
                                       ? DecodeHighSierraDirectory(
                                           entry.Value.Extents[0].extent + entry.Value.XattrLength,
                                           entry.Value.Extents[0].size)
                                       : DecodeIsoDirectory(entry.Value.Extents[0].extent + entry.Value.XattrLength,
                                                            entry.Value.Extents[0].size);

            if(_usePathTable)
            {
                foreach(DecodedDirectoryEntry subDirectory in _cdi
                                                                  ? GetSubdirsFromCdiPathTable(currentPath)
                                                                  : _highSierra
                                                                      ? GetSubdirsFromHighSierraPathTable(currentPath)
                                                                      : GetSubdirsFromIsoPathTable(currentPath))
                    currentDirectory[subDirectory.Filename] = subDirectory;
            }

            _directoryCache.Add(currentPath, currentDirectory);
        }

        if(currentDirectory is null)
            return ErrorNumber.NoSuchFile;

        node = new Iso9660DirNode
        {
            Path      = path,
            _position = 0,
            _entries  = currentDirectory.Values.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not Iso9660DirNode mynode)
            return ErrorNumber.InvalidArgument;

        if(mynode._position < 0)
            return ErrorNumber.InvalidArgument;

        if(mynode._position >= mynode._entries.Length)
            return ErrorNumber.NoError;

        switch(_namespace)
        {
            case Namespace.Normal:
                filename = mynode._entries[mynode._position].Filename.EndsWith(";1", StringComparison.Ordinal)
                               ? mynode._entries[mynode._position].Filename[..^2]
                               : mynode._entries[mynode._position].Filename;

                break;
            case Namespace.Vms:
            case Namespace.Joliet:
            case Namespace.Rrip:
            case Namespace.Romeo:
                filename = mynode._entries[mynode._position].Filename;

                break;
            default:
                return ErrorNumber.InvalidArgument;
        }

        mynode._position++;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not Iso9660DirNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode._position = -1;
        mynode._entries  = null;

        return ErrorNumber.NoError;
    }

#endregion

    Dictionary<string, DecodedDirectoryEntry> DecodeCdiDirectory(ulong start, uint size)
    {
        Dictionary<string, DecodedDirectoryEntry> entries  = new();
        var                                       entryOff = 0;

        ErrorNumber errno = ReadSingleExtent(size, (uint)start, out byte[] data);

        if(errno != ErrorNumber.NoError)
            return entries;

        while(entryOff + _cdiDirectoryRecordSize < data.Length)
        {
            CdiDirectoryRecord record =
                Marshal.ByteArrayToStructureBigEndian<CdiDirectoryRecord>(data, entryOff, _cdiDirectoryRecordSize);

            if(record.length == 0)
            {
                // Skip to next sector
                if(data.Length - (entryOff / 2048 + 1) * 2048 > 0)
                {
                    entryOff = (entryOff / 2048 + 1) * 2048;

                    continue;
                }

                break;
            }

            // Special entries for current and parent directories, skip them
            if(record.name_len == 1)
            {
                if(data[entryOff + _directoryRecordSize] == 0 ||
                   data[entryOff + _directoryRecordSize] == 1)
                {
                    entryOff += record.length;

                    continue;
                }
            }

            var entry = new DecodedDirectoryEntry
            {
                Size                 = record.size,
                Filename             = Encoding.GetString(data, entryOff + _directoryRecordSize, record.name_len),
                VolumeSequenceNumber = record.volume_sequence_number,
                Timestamp            = DecodeHighSierraDateTime(record.date),
                XattrLength          = record.xattr_len,
                Extents              = new List<(uint extent, uint size)>()
            };

            if(record.size != 0)
                entry.Extents.Add((record.start_lbn, record.size));

            if(record.flags.HasFlag(CdiFileFlags.Hidden))
            {
                entry.Flags |= FileFlags.Hidden;

                continue;
            }

            int systemAreaStart = entryOff + record.name_len + _cdiDirectoryRecordSize;

            if(systemAreaStart % 2 != 0)
                systemAreaStart++;

            entry.CdiSystemArea =
                Marshal.ByteArrayToStructureBigEndian<CdiSystemArea>(data, systemAreaStart, _cdiSystemAreaSize);

            if(entry.CdiSystemArea.Value.attributes.HasFlag(CdiAttributes.Directory))
                entry.Flags |= FileFlags.Directory;

            if(!entry.CdiSystemArea.Value.attributes.HasFlag(CdiAttributes.Directory) ||
               !_usePathTable)
                entries[entry.Filename] = entry;

            entryOff += record.length;
        }

        return entries;
    }

    Dictionary<string, DecodedDirectoryEntry> DecodeHighSierraDirectory(ulong start, uint size)
    {
        Dictionary<string, DecodedDirectoryEntry> entries  = new();
        var                                       entryOff = 0;

        ErrorNumber errno = ReadSingleExtent(size, (uint)start, out byte[] data);

        if(errno != ErrorNumber.NoError)
            return entries;

        while(entryOff + _directoryRecordSize < data.Length)
        {
            HighSierraDirectoryRecord record =
                Marshal.ByteArrayToStructureLittleEndian<HighSierraDirectoryRecord>(data, entryOff,
                    _highSierraDirectoryRecordSize);

            if(record.length == 0)
            {
                // Skip to next sector
                if(data.Length - (entryOff / 2048 + 1) * 2048 > 0)
                {
                    entryOff = (entryOff / 2048 + 1) * 2048;

                    continue;
                }

                break;
            }

            // Special entries for current and parent directories, skip them
            if(record.name_len == 1)
            {
                if(data[entryOff + _directoryRecordSize] == 0 ||
                   data[entryOff + _directoryRecordSize] == 1)
                {
                    entryOff += record.length;

                    continue;
                }
            }

            var entry = new DecodedDirectoryEntry
            {
                Size                 = record.size,
                Flags                = record.flags,
                Interleave           = record.interleave,
                VolumeSequenceNumber = record.volume_sequence_number,
                Filename             = Encoding.GetString(data, entryOff + _directoryRecordSize, record.name_len),
                Timestamp            = DecodeHighSierraDateTime(record.date),
                XattrLength          = record.xattr_len,
                Extents              = new List<(uint extent, uint size)>()
            };

            if(record.size != 0)
                entry.Extents.Add((record.extent, record.size));

            if(entry.Flags.HasFlag(FileFlags.Directory) && _usePathTable)
            {
                entryOff += record.length;

                continue;
            }

            if(!entries.ContainsKey(entry.Filename))
                entries.Add(entry.Filename, entry);

            entryOff += record.length;
        }

        if(_useTransTbl)
            DecodeTransTable(entries);

        return entries;
    }

    Dictionary<string, DecodedDirectoryEntry> DecodeIsoDirectory(ulong start, uint size)
    {
        Dictionary<string, DecodedDirectoryEntry> entries  = new();
        var                                       entryOff = 0;

        ErrorNumber errno = ReadSingleExtent(size, (uint)start, out byte[] data);

        if(errno != ErrorNumber.NoError)
            return entries;

        while(entryOff + _directoryRecordSize < data.Length)
        {
            DirectoryRecord record =
                Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(data, entryOff, _directoryRecordSize);

            if(record.length == 0)
            {
                // Skip to next sector
                if(data.Length - (entryOff / 2048 + 1) * 2048 > 0)
                {
                    entryOff = (entryOff / 2048 + 1) * 2048;

                    continue;
                }

                break;
            }

            // Special entries for current and parent directories, skip them
            if(record.name_len == 1)
            {
                if(data[entryOff + _directoryRecordSize] == 0 ||
                   data[entryOff + _directoryRecordSize] == 1)
                {
                    entryOff += record.length;

                    continue;
                }
            }

            var entry = new DecodedDirectoryEntry
            {
                Size  = record.size,
                Flags = record.flags,
                Filename =
                    _joliet
                        ? Encoding.BigEndianUnicode.GetString(data, entryOff + _directoryRecordSize,
                                                              record.name_len)
                        : Encoding.GetString(data, entryOff + _directoryRecordSize, record.name_len),
                FileUnitSize         = record.file_unit_size,
                Interleave           = record.interleave,
                VolumeSequenceNumber = record.volume_sequence_number,
                Timestamp            = DecodeIsoDateTime(record.date),
                XattrLength          = record.xattr_len,
                Extents              = new List<(uint extent, uint size)>()
            };

            if(record.size != 0)
                entry.Extents.Add((record.extent, record.size));

            if(entry.Flags.HasFlag(FileFlags.Directory) && _usePathTable)
            {
                entryOff += record.length;

                continue;
            }

            // Mac OS can use slashes, we cannot
            entry.Filename = entry.Filename.Replace('/', '\u2215');

            // Tailing '.' is only allowed on RRIP. If present it will be recreated below with the alternate name
            if(entry.Filename.EndsWith(".", StringComparison.Ordinal))
                entry.Filename = entry.Filename[..^1];

            if(entry.Filename.EndsWith(".;1", StringComparison.Ordinal))
                entry.Filename = entry.Filename[..^3] + ";1";

            // This is a legal Joliet name, different from VMS version fields, but Nero MAX incorrectly creates these filenames
            if(_joliet && entry.Filename.EndsWith(";1", StringComparison.Ordinal))
                entry.Filename = entry.Filename[..^2];

            int systemAreaStart  = entryOff      + record.name_len + _directoryRecordSize;
            int systemAreaLength = record.length - record.name_len - _directoryRecordSize;

            if(systemAreaStart % 2 != 0)
            {
                systemAreaStart++;
                systemAreaLength--;
            }

            DecodeSystemArea(data, systemAreaStart, systemAreaStart + systemAreaLength, ref entry,
                             out bool hasResourceFork);

            if(entry.Flags.HasFlag(FileFlags.Associated))
            {
                if(entries.ContainsKey(entry.Filename))
                {
                    if(hasResourceFork)
                    {
                        entries[entry.Filename].ResourceFork.Size += entry.Size;
                        entries[entry.Filename].ResourceFork.Extents.Add(entry.Extents[0]);
                    }
                    else
                    {
                        entries[entry.Filename].AssociatedFile.Size += entry.Size;
                        entries[entry.Filename].AssociatedFile.Extents.Add(entry.Extents[0]);
                    }
                }
                else
                {
                    entries[entry.Filename] = new DecodedDirectoryEntry
                    {
                        Size                 = 0,
                        Flags                = record.flags ^ FileFlags.Associated,
                        FileUnitSize         = 0,
                        Interleave           = 0,
                        VolumeSequenceNumber = record.volume_sequence_number,
                        Filename             = entry.Filename,
                        Timestamp            = DecodeIsoDateTime(record.date),
                        XattrLength          = 0
                    };

                    if(hasResourceFork)
                        entries[entry.Filename].ResourceFork = entry;
                    else
                        entries[entry.Filename].AssociatedFile = entry;
                }
            }
            else
            {
                if(entries.ContainsKey(entry.Filename))
                {
                    entries[entry.Filename].Size += entry.Size;

                    // Can appear after an associated file
                    if(entries[entry.Filename].Extents is null)
                    {
                        entries[entry.Filename].Extents              = new List<(uint extent, uint size)>();
                        entries[entry.Filename].Flags                = entry.Flags;
                        entries[entry.Filename].FileUnitSize         = entry.FileUnitSize;
                        entries[entry.Filename].Interleave           = entry.Interleave;
                        entries[entry.Filename].VolumeSequenceNumber = entry.VolumeSequenceNumber;
                        entries[entry.Filename].Filename             = entry.Filename;
                        entries[entry.Filename].Timestamp            = entry.Timestamp;
                        entries[entry.Filename].XattrLength          = entry.XattrLength;
                    }

                    if(entry.Extents?.Count > 0)
                        entries[entry.Filename].Extents.Add(entry.Extents[0]);
                }
                else
                    entries[entry.Filename] = entry;
            }

            entryOff += record.length;
        }

        if(_useTransTbl)
            DecodeTransTable(entries);

        // Relocated directories should be shown in correct place when using Rock Ridge namespace
        return _namespace == Namespace.Rrip
                   ? entries.Where(e => !e.Value.RockRidgeRelocated).ToDictionary(x => x.Key, x => x.Value)
                   : entries;
    }

    void DecodeTransTable(Dictionary<string, DecodedDirectoryEntry> entries)
    {
        KeyValuePair<string, DecodedDirectoryEntry> transTblEntry =
            entries.FirstOrDefault(e => !e.Value.Flags.HasFlag(FileFlags.Directory) &&
                                        (e.Value.Filename.ToLower(CultureInfo.CurrentUICulture) == "trans.tbl" ||
                                         e.Value.Filename.ToLower(CultureInfo.CurrentUICulture) == "trans.tbl;1"));

        if(transTblEntry.Value == null)
            return;

        ErrorNumber errno = ReadWithExtents(0, (long)transTblEntry.Value.Size, transTblEntry.Value.Extents,
                                            transTblEntry.Value.XA?.signature == XA_MAGIC &&
                                            transTblEntry.Value.XA?.attributes.HasFlag(XaAttributes.Interleaved) ==
                                            true, transTblEntry.Value.XA?.filenumber ?? 0, out byte[] transTbl);

        if(errno != ErrorNumber.NoError)
            return;

        var mr = new MemoryStream(transTbl, 0, (int)transTblEntry.Value.Size, false);
        var sr = new StreamReader(mr, Encoding);

        string line = sr.ReadLine();

        while(line != null)
        {
            // Skip the type field and the first space
            string cutLine      = line[2..];
            int    spaceIndex   = cutLine.IndexOf(' ');
            string originalName = cutLine[..spaceIndex];
            string originalNameWithVersion;
            string newName = cutLine[(spaceIndex + 1)..].TrimStart();

            if(originalName.EndsWith(";1", StringComparison.Ordinal))
            {
                originalNameWithVersion = originalName.ToLower(CultureInfo.CurrentUICulture);
                originalName            = originalNameWithVersion[..(originalName.Length - 2)];
            }
            else
            {
                originalName            = originalName.ToLower(CultureInfo.CurrentUICulture);
                originalNameWithVersion = originalName + ";1";
            }

            // Pre-read next line
            line = sr.ReadLine();

            KeyValuePair<string, DecodedDirectoryEntry> originalEntry =
                entries.FirstOrDefault(e => !e.Value.Flags.HasFlag(FileFlags.Directory) &&
                                            (e.Value.Filename.ToLower(CultureInfo.CurrentUICulture) == originalName ||
                                             e.Value.Filename.ToLower(CultureInfo.CurrentUICulture) ==
                                             originalNameWithVersion));

            originalEntry.Value.Filename = newName;
            entries.Remove(originalEntry.Key);
            entries[newName] = originalEntry.Value;
        }

        entries.Remove(transTblEntry.Key);
    }

    void DecodeSystemArea(byte[] data, int start, int end, ref DecodedDirectoryEntry entry, out bool hasResourceFork)
    {
        int systemAreaOff = start;
        hasResourceFork = false;
        var continueSymlink          = false;
        var continueSymlinkComponent = false;

        while(systemAreaOff + 2 <= end)
        {
            var systemAreaSignature = BigEndianBitConverter.ToUInt16(data, systemAreaOff);

            if(BigEndianBitConverter.ToUInt16(data, systemAreaOff + 6) == XA_MAGIC)
                systemAreaSignature = XA_MAGIC;

            AppleCommon.FInfo fInfo;

            switch(systemAreaSignature)
            {
                case APPLE_MAGIC:
                    byte appleLength = data[systemAreaOff + 2];
                    var  appleId     = (AppleId)data[systemAreaOff + 3];

                    // Old AAIP
                    if(appleId     == AppleId.ProDOS &&
                       appleLength != 7)
                        goto case AAIP_MAGIC;

                    switch(appleId)
                    {
                        case AppleId.ProDOS:
                            AppleProDOSSystemUse appleProDosSystemUse =
                                Marshal.ByteArrayToStructureLittleEndian<AppleProDOSSystemUse>(data, systemAreaOff,
                                    Marshal.SizeOf<AppleProDOSSystemUse>());

                            entry.AppleProDosType = appleProDosSystemUse.aux_type;
                            entry.AppleDosType    = appleProDosSystemUse.type;

                            break;
                        case AppleId.HFS:
                            AppleHFSSystemUse appleHfsSystemUse =
                                Marshal.ByteArrayToStructureBigEndian<AppleHFSSystemUse>(data, systemAreaOff,
                                    Marshal.SizeOf<AppleHFSSystemUse>());

                            hasResourceFork = true;

                            fInfo = new AppleCommon.FInfo
                            {
                                fdCreator = appleHfsSystemUse.creator,
                                fdFlags   = appleHfsSystemUse.finder_flags,
                                fdType    = appleHfsSystemUse.type
                            };

                            entry.FinderInfo = fInfo;

                            break;
                    }

                    systemAreaOff += appleLength;

                    break;
                case APPLE_MAGIC_OLD:
                    var appleOldId = (AppleOldId)data[systemAreaOff + 2];

                    switch(appleOldId)
                    {
                        case AppleOldId.ProDOS:
                            AppleProDOSOldSystemUse appleProDosOldSystemUse =
                                Marshal.ByteArrayToStructureLittleEndian<AppleProDOSOldSystemUse>(data, systemAreaOff,
                                    Marshal.SizeOf<AppleProDOSOldSystemUse>());

                            entry.AppleProDosType = appleProDosOldSystemUse.aux_type;
                            entry.AppleDosType    = appleProDosOldSystemUse.type;

                            systemAreaOff += Marshal.SizeOf<AppleProDOSOldSystemUse>();

                            break;
                        case AppleOldId.TypeCreator:
                        case AppleOldId.TypeCreatorBundle:
                            AppleHFSTypeCreatorSystemUse appleHfsTypeCreatorSystemUse =
                                Marshal.ByteArrayToStructureBigEndian<AppleHFSTypeCreatorSystemUse>(data, systemAreaOff,
                                    Marshal.SizeOf<AppleHFSTypeCreatorSystemUse>());

                            hasResourceFork = true;

                            fInfo = new AppleCommon.FInfo
                            {
                                fdCreator = appleHfsTypeCreatorSystemUse.creator,
                                fdType    = appleHfsTypeCreatorSystemUse.type
                            };

                            entry.FinderInfo = fInfo;

                            systemAreaOff += Marshal.SizeOf<AppleHFSTypeCreatorSystemUse>();

                            break;
                        case AppleOldId.TypeCreatorIcon:
                        case AppleOldId.TypeCreatorIconBundle:
                            AppleHFSIconSystemUse appleHfsIconSystemUse =
                                Marshal.ByteArrayToStructureBigEndian<AppleHFSIconSystemUse>(data, systemAreaOff,
                                    Marshal.SizeOf<AppleHFSIconSystemUse>());

                            hasResourceFork = true;

                            fInfo = new AppleCommon.FInfo
                            {
                                fdCreator = appleHfsIconSystemUse.creator,
                                fdType    = appleHfsIconSystemUse.type
                            };

                            entry.FinderInfo = fInfo;
                            entry.AppleIcon  = appleHfsIconSystemUse.icon;

                            systemAreaOff += Marshal.SizeOf<AppleHFSIconSystemUse>();

                            break;
                        case AppleOldId.HFS:
                            AppleHFSOldSystemUse appleHfsSystemUse =
                                Marshal.ByteArrayToStructureBigEndian<AppleHFSOldSystemUse>(data, systemAreaOff,
                                    Marshal.SizeOf<AppleHFSOldSystemUse>());

                            hasResourceFork = true;

                            fInfo = new AppleCommon.FInfo
                            {
                                fdCreator = appleHfsSystemUse.creator,
                                fdFlags   = (AppleCommon.FinderFlags)appleHfsSystemUse.finder_flags,
                                fdType    = appleHfsSystemUse.type
                            };

                            entry.FinderInfo = fInfo;

                            systemAreaOff += Marshal.SizeOf<AppleHFSOldSystemUse>();

                            break;
                        default:
                            // Cannot continue as we don't know this structure size
                            systemAreaOff = end;

                            break;
                    }

                    break;
                case XA_MAGIC:
                    entry.XA = Marshal.ByteArrayToStructureBigEndian<CdromXa>(data, systemAreaOff,
                                                                              Marshal.SizeOf<CdromXa>());

                    systemAreaOff += Marshal.SizeOf<CdromXa>();

                    break;

                // All of these follow the SUSP indication of 2 bytes for signature 1 byte for length
                case AAIP_MAGIC:
                case AMIGA_MAGIC:
                    AmigaEntry amiga =
                        Marshal.ByteArrayToStructureBigEndian<AmigaEntry>(data, systemAreaOff,
                                                                          Marshal.SizeOf<AmigaEntry>());

                    var protectionLength = 0;

                    if(amiga.flags.HasFlag(AmigaFlags.Protection))
                    {
                        entry.AmigaProtection =
                            Marshal.ByteArrayToStructureBigEndian<AmigaProtection>(data,
                                systemAreaOff + Marshal.SizeOf<AmigaEntry>(), Marshal.SizeOf<AmigaProtection>());

                        protectionLength = Marshal.SizeOf<AmigaProtection>();
                    }

                    if(amiga.flags.HasFlag(AmigaFlags.Comment))
                    {
                        entry.AmigaComment ??= Array.Empty<byte>();

                        var newComment = new byte[entry.AmigaComment.Length +
                                                  data
                                                      [systemAreaOff + Marshal.SizeOf<AmigaEntry>() + protectionLength] -
                                                  1];

                        Array.Copy(entry.AmigaComment, 0, newComment, 0, entry.AmigaComment.Length);

                        Array.Copy(data, systemAreaOff + Marshal.SizeOf<AmigaEntry>() + protectionLength, newComment,
                                   entry.AmigaComment.Length,
                                   data[systemAreaOff + Marshal.SizeOf<AmigaEntry>() + protectionLength] - 1);

                        entry.AmigaComment = newComment;
                    }

                    systemAreaOff += amiga.length;

                    break;

                // This merely indicates the existence of RRIP extensions, we don't need it
                case RRIP_MAGIC:
                    byte rripLength = data[systemAreaOff + 2];
                    systemAreaOff += rripLength;

                    break;
                case RRIP_POSIX_ATTRIBUTES:
                    byte pxLength = data[systemAreaOff + 2];

                    switch(pxLength)
                    {
                        case 36:
                            entry.PosixAttributesOld =
                                Marshal.ByteArrayToStructureLittleEndian<PosixAttributesOld>(data, systemAreaOff,
                                    Marshal.SizeOf<PosixAttributesOld>());

                            break;
                        case >= 44:
                            entry.PosixAttributes =
                                Marshal.ByteArrayToStructureLittleEndian<PosixAttributes>(data, systemAreaOff,
                                    Marshal.SizeOf<PosixAttributes>());

                            break;
                    }

                    systemAreaOff += pxLength;

                    break;
                case RRIP_POSIX_DEV_NO:
                    byte pnLength = data[systemAreaOff + 2];

                    entry.PosixDeviceNumber =
                        Marshal.ByteArrayToStructureLittleEndian<PosixDeviceNumber>(data, systemAreaOff,
                            Marshal.SizeOf<PosixDeviceNumber>());

                    systemAreaOff += pnLength;

                    break;
                case RRIP_SYMLINK:
                    byte slLength = data[systemAreaOff + 2];

                    SymbolicLink sl =
                        Marshal.ByteArrayToStructureLittleEndian<SymbolicLink>(data, systemAreaOff,
                                                                               Marshal.SizeOf<SymbolicLink>());

                    SymbolicLinkComponent slc =
                        Marshal.ByteArrayToStructureLittleEndian<SymbolicLinkComponent>(data,
                            systemAreaOff + Marshal.SizeOf<SymbolicLink>(),
                            Marshal.SizeOf<SymbolicLinkComponent>());

                    if(!continueSymlink ||
                       entry.SymbolicLink is null)
                        entry.SymbolicLink = "";

                    if(slc.flags.HasFlag(SymlinkComponentFlags.Root))
                        entry.SymbolicLink = "/";

                    if(slc.flags.HasFlag(SymlinkComponentFlags.Current))
                        entry.SymbolicLink += ".";

                    if(slc.flags.HasFlag(SymlinkComponentFlags.Parent))
                        entry.SymbolicLink += "..";

                    if(!continueSymlinkComponent &&
                       !slc.flags.HasFlag(SymlinkComponentFlags.Root))
                        entry.SymbolicLink += "/";

                    entry.SymbolicLink += slc.flags.HasFlag(SymlinkComponentFlags.Networkname)
                                              ? Environment.MachineName
                                              : _joliet
                                                  ? Encoding.BigEndianUnicode.GetString(data,
                                                      systemAreaOff + Marshal.SizeOf<SymbolicLink>() +
                                                      Marshal.SizeOf<SymbolicLinkComponent>(), slc.length)
                                                  : Encoding.GetString(data,
                                                                       systemAreaOff + Marshal.SizeOf<SymbolicLink>() +
                                                                       Marshal.SizeOf<SymbolicLinkComponent>(),
                                                                       slc.length);

                    continueSymlink          = sl.flags.HasFlag(SymlinkFlags.Continue);
                    continueSymlinkComponent = slc.flags.HasFlag(SymlinkComponentFlags.Continue);

                    systemAreaOff += slLength;

                    break;
                case RRIP_NAME:
                    byte nmLength = data[systemAreaOff + 2];

                    if(_namespace != Namespace.Rrip)
                    {
                        systemAreaOff += nmLength;

                        break;
                    }

                    AlternateName alternateName =
                        Marshal.ByteArrayToStructureLittleEndian<AlternateName>(data, systemAreaOff,
                            Marshal.SizeOf<AlternateName>());

                    byte[] nm;

                    if(alternateName.flags.HasFlag(AlternateNameFlags.Networkname))
                    {
                        nm = _joliet
                                 ? Encoding.BigEndianUnicode.GetBytes(Environment.MachineName)
                                 : Encoding.GetBytes(Environment.MachineName);
                    }
                    else
                    {
                        nm = new byte[nmLength - Marshal.SizeOf<AlternateName>()];

                        Array.Copy(data, systemAreaOff + Marshal.SizeOf<AlternateName>(), nm, 0, nm.Length);
                    }

                    entry.RockRidgeAlternateName ??= Array.Empty<byte>();

                    var newNm = new byte[entry.RockRidgeAlternateName.Length + nm.Length];
                    Array.Copy(entry.RockRidgeAlternateName, 0, newNm, 0, entry.RockRidgeAlternateName.Length);
                    Array.Copy(nm,                           0, newNm, entry.RockRidgeAlternateName.Length, nm.Length);

                    entry.RockRidgeAlternateName = newNm;

                    if(!alternateName.flags.HasFlag(AlternateNameFlags.Continue))
                    {
                        entry.Filename = _joliet
                                             ? Encoding.BigEndianUnicode.GetString(entry.RockRidgeAlternateName)
                                             : Encoding.GetString(entry.RockRidgeAlternateName);

                        entry.RockRidgeAlternateName = null;
                    }

                    systemAreaOff += nmLength;

                    break;
                case RRIP_CHILDLINK:
                    byte clLength = data[systemAreaOff + 2];

                    // If we are not in Rock Ridge namespace, or we are using the Path Table, skip it
                    if(_namespace != Namespace.Rrip || _usePathTable)
                    {
                        systemAreaOff += clLength;

                        break;
                    }

                    ChildLink cl =
                        Marshal.ByteArrayToStructureLittleEndian<ChildLink>(data, systemAreaOff,
                                                                            Marshal.SizeOf<ChildLink>());

                    ErrorNumber errno = ReadSector(cl.child_dir_lba, out byte[] childSector);

                    if(errno != ErrorNumber.NoError)
                    {
                        systemAreaOff = end;

                        break;
                    }

                    DirectoryRecord childRecord =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(childSector);

                    // As per RRIP 4.1.5.1, we leave name as in previous entry, substitute location with the one in
                    // the CL, and replace all other fields with the ones found in the first entry of the child
                    entry.Extents = new List<(uint extent, uint size)>
                    {
                        (cl.child_dir_lba, childRecord.size)
                    };

                    entry.Size                 = childRecord.size;
                    entry.Flags                = childRecord.flags;
                    entry.FileUnitSize         = childRecord.file_unit_size;
                    entry.Interleave           = childRecord.interleave;
                    entry.VolumeSequenceNumber = childRecord.volume_sequence_number;
                    entry.Timestamp            = DecodeIsoDateTime(childRecord.date);
                    entry.XattrLength          = childRecord.xattr_len;

                    systemAreaOff += clLength;

                    break;
                case RRIP_PARENTLINK:
                    // SKip, we don't need it
                    byte plLength = data[systemAreaOff + 2];
                    systemAreaOff += plLength;

                    break;
                case RRIP_RELOCATED_DIR:
                    byte reLength = data[systemAreaOff + 2];
                    systemAreaOff += reLength;

                    entry.RockRidgeRelocated = true;

                    break;
                case RRIP_TIMESTAMPS:
                    byte tfLength = data[systemAreaOff + 2];

                    Timestamps timestamps =
                        Marshal.ByteArrayToStructureLittleEndian<Timestamps>(data, systemAreaOff,
                                                                             Marshal.SizeOf<Timestamps>());

                    int tfOff = systemAreaOff + Marshal.SizeOf<Timestamps>();
                    int tfLen = timestamps.flags.HasFlag(TimestampFlags.LongFormat) ? 17 : 7;

                    if(timestamps.flags.HasFlag(TimestampFlags.Creation))
                    {
                        entry.RripCreation = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripCreation, 0, tfLen);
                        tfOff += tfLen;
                    }

                    if(timestamps.flags.HasFlag(TimestampFlags.Modification))
                    {
                        entry.RripModify = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripModify, 0, tfLen);
                        tfOff += tfLen;
                    }

                    if(timestamps.flags.HasFlag(TimestampFlags.Access))
                    {
                        entry.RripAccess = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripAccess, 0, tfLen);
                        tfOff += tfLen;
                    }

                    if(timestamps.flags.HasFlag(TimestampFlags.AttributeChange))
                    {
                        entry.RripAttributeChange = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripAttributeChange, 0, tfLen);
                        tfOff += tfLen;
                    }

                    if(timestamps.flags.HasFlag(TimestampFlags.Backup))
                    {
                        entry.RripBackup = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripBackup, 0, tfLen);
                        tfOff += tfLen;
                    }

                    if(timestamps.flags.HasFlag(TimestampFlags.Expiration))
                    {
                        entry.RripExpiration = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripExpiration, 0, tfLen);
                        tfOff += tfLen;
                    }

                    if(timestamps.flags.HasFlag(TimestampFlags.Effective))
                    {
                        entry.RripEffective = new byte[tfLen];
                        Array.Copy(data, tfOff, entry.RripEffective, 0, tfLen);
                    }

                    systemAreaOff += tfLength;

                    break;
                case RRIP_SPARSE:
                    // TODO
                    byte sfLength = data[systemAreaOff + 2];
                    systemAreaOff += sfLength;

                    break;
                case SUSP_CONTINUATION:
                    byte ceLength = data[systemAreaOff + 2];

                    ContinuationArea ca =
                        Marshal.ByteArrayToStructureLittleEndian<ContinuationArea>(data, systemAreaOff,
                            Marshal.SizeOf<ContinuationArea>());

                    errno = ReadSingleExtent(ca.offset, ca.ca_length, ca.block, out byte[] caData);

                    // TODO: Check continuation area definition, this is not a proper fix
                    if(errno         == ErrorNumber.NoError &&
                       caData.Length > 0)
                        DecodeSystemArea(caData, 0, (int)ca.ca_length, ref entry, out hasResourceFork);

                    systemAreaOff += ceLength;

                    break;
                case SUSP_PADDING:
                    // Just padding, skip
                    byte pdLength = data[systemAreaOff + 2];
                    systemAreaOff += pdLength;

                    break;
                case SUSP_INDICATOR:
                    // Only to be found on CURRENT entry of root directory
                    byte spLength = data[systemAreaOff + 2];
                    systemAreaOff += spLength;

                    break;
                case SUSP_TERMINATOR:
                    // Not seen on the wild
                    byte stLength = data[systemAreaOff + 2];
                    systemAreaOff += stLength;

                    break;
                case SUSP_REFERENCE:
                    // Only to be found on CURRENT entry of root directory
                    byte erLength = data[systemAreaOff + 2];
                    systemAreaOff += erLength;

                    break;
                case SUSP_SELECTOR:
                    // Only to be found on CURRENT entry of root directory
                    byte esLength = data[systemAreaOff + 2];
                    systemAreaOff += esLength;

                    break;
                case ZISO_MAGIC:
                    // TODO: Implement support for zisofs
                    byte zfLength = data[systemAreaOff + 2];
                    systemAreaOff += zfLength;

                    break;
                default:
                    // Cannot continue as we don't know this structure size
                    systemAreaOff = end;

                    break;
            }
        }
    }

    IEnumerable<PathTableEntryInternal> GetPathTableEntries(string path)
    {
        IEnumerable<PathTableEntryInternal> tableEntries;
        List<PathTableEntryInternal>        pathTableList = new(_pathTable);

        if(path is "" or "/")
            tableEntries = _pathTable.Where(p => p.Parent == 1 && p != _pathTable[0]);
        else
        {
            string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                                 ? path[1..].ToLower(CultureInfo.CurrentUICulture)
                                 : path.ToLower(CultureInfo.CurrentUICulture);

            string[] pieces = cutPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var currentParent = 1;
            var currentPiece  = 0;

            while(currentPiece < pieces.Length)
            {
                PathTableEntryInternal currentEntry = _pathTable.FirstOrDefault(p => p.Parent == currentParent &&
                    p.Name.ToLower(CultureInfo.
                                       CurrentUICulture) ==
                    pieces[currentPiece]);

                if(currentEntry is null)
                    break;

                currentPiece++;
                currentParent = pathTableList.IndexOf(currentEntry) + 1;
            }

            tableEntries = _pathTable.Where(p => p.Parent == currentParent);
        }

        return tableEntries.ToArray();
    }

    DecodedDirectoryEntry[] GetSubdirsFromCdiPathTable(string path)
    {
        IEnumerable<PathTableEntryInternal> tableEntries = GetPathTableEntries(path);
        List<DecodedDirectoryEntry>         entries      = new();

        foreach(PathTableEntryInternal tEntry in tableEntries)
        {
            ErrorNumber errno = ReadSector(tEntry.Extent, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                continue;

            CdiDirectoryRecord record =
                Marshal.ByteArrayToStructureBigEndian<CdiDirectoryRecord>(sector, tEntry.XattrLength,
                                                                          _cdiDirectoryRecordSize);

            if(record.length == 0)
                break;

            var entry = new DecodedDirectoryEntry
            {
                Size                 = record.size,
                Filename             = tEntry.Name,
                VolumeSequenceNumber = record.volume_sequence_number,
                Timestamp            = DecodeHighSierraDateTime(record.date),
                XattrLength          = tEntry.XattrLength,
                Extents              = new List<(uint extent, uint size)>()
            };

            if(record.size != 0)
                entry.Extents.Add((record.start_lbn, record.size));

            if(record.flags.HasFlag(CdiFileFlags.Hidden))
                entry.Flags |= FileFlags.Hidden;

            int systemAreaStart = record.name_len + _cdiDirectoryRecordSize;

            if(systemAreaStart % 2 != 0)
                systemAreaStart++;

            entry.CdiSystemArea =
                Marshal.ByteArrayToStructureBigEndian<CdiSystemArea>(sector, systemAreaStart, _cdiSystemAreaSize);

            if(entry.CdiSystemArea.Value.attributes.HasFlag(CdiAttributes.Directory))
                entry.Flags |= FileFlags.Directory;

            entries.Add(entry);
        }

        return entries.ToArray();
    }

    DecodedDirectoryEntry[] GetSubdirsFromIsoPathTable(string path)
    {
        IEnumerable<PathTableEntryInternal> tableEntries = GetPathTableEntries(path);
        List<DecodedDirectoryEntry>         entries      = new();

        foreach(PathTableEntryInternal tEntry in tableEntries)
        {
            ErrorNumber errno = ReadSector(tEntry.Extent, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                continue;

            DirectoryRecord record =
                Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(sector, tEntry.XattrLength,
                                                                          _directoryRecordSize);

            if(record.length == 0)
                break;

            var entry = new DecodedDirectoryEntry
            {
                Size                 = record.size,
                Flags                = record.flags,
                Filename             = tEntry.Name,
                FileUnitSize         = record.file_unit_size,
                Interleave           = record.interleave,
                VolumeSequenceNumber = record.volume_sequence_number,
                Timestamp            = DecodeIsoDateTime(record.date),
                XattrLength          = tEntry.XattrLength,
                Extents              = new List<(uint extent, uint size)>()
            };

            if(record.size != 0)
                entry.Extents.Add((record.extent, record.size));

            int systemAreaStart  = record.name_len + _directoryRecordSize;
            int systemAreaLength = record.length   - record.name_len - _directoryRecordSize;

            if(systemAreaStart % 2 != 0)
            {
                systemAreaStart++;
                systemAreaLength--;
            }

            DecodeSystemArea(sector, systemAreaStart, systemAreaStart + systemAreaLength, ref entry, out _);

            entries.Add(entry);
        }

        return entries.ToArray();
    }

    DecodedDirectoryEntry[] GetSubdirsFromHighSierraPathTable(string path)
    {
        IEnumerable<PathTableEntryInternal> tableEntries = GetPathTableEntries(path);
        List<DecodedDirectoryEntry>         entries      = new();

        foreach(PathTableEntryInternal tEntry in tableEntries)
        {
            ErrorNumber errno = ReadSector(tEntry.Extent, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                continue;

            HighSierraDirectoryRecord record =
                Marshal.ByteArrayToStructureLittleEndian<HighSierraDirectoryRecord>(sector, tEntry.XattrLength,
                    _highSierraDirectoryRecordSize);

            var entry = new DecodedDirectoryEntry
            {
                Size                 = record.size,
                Flags                = record.flags,
                Filename             = tEntry.Name,
                Interleave           = record.interleave,
                VolumeSequenceNumber = record.volume_sequence_number,
                Timestamp            = DecodeHighSierraDateTime(record.date),
                XattrLength          = tEntry.XattrLength,
                Extents              = new List<(uint extent, uint size)>()
            };

            if(record.size != 0)
                entry.Extents.Add((record.extent, record.size));

            entries.Add(entry);
        }

        return entries.ToArray();
    }
}