// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Worm.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads the directory of write-once (WORM, RPS 1 and 257) volumes.
//
//     Directories are chains of fixed-size segments ("DSS-SEG "), each sector
//     holding one directory entry ("DESSTR"). Entries are never rewritten: a
//     new version of an entry is appended instead, so the last one written for
//     a name is the current one. When a segment fills, the end of its last
//     sector points to the next segment of the chain.
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
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class HPOFS
{
    /// <summary>Path separator of a WORM volume, from the SMI block</summary>
    byte _wormSeparator;
    /// <summary>Sectors in a WORM directory segment, from the SMI block</summary>
    uint _wormSegmentSectors;

    /// <summary>Reads the current volume management information block of a WORM volume</summary>
    /// <param name="vmi">Contents of the VMI block</param>
    ErrorNumber ReadWormVmi(out byte[] vmi)
    {
        vmi = null;

        // Primary and backup VMI locations are stored in the SMI block
        foreach(int offset in new[]
                {
                    SMI_VMI_PRIMARY_OFFSET, SMI_VMI_BACKUP_OFFSET
                })
        {
            uint lba = BigEndianBitConverter.ToUInt32(_medInfo.rest, offset - SMI_REST_OFFSET);

            if(ReadSectors(lba, 1, out byte[] sector) != ErrorNumber.NoError) continue;

            if(!sector.Take(8).SequenceEqual(_vmiSignature)) continue;

            vmi = sector;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Reads all the directory entries in a chain of segments, in the order they were written</summary>
    /// <param name="firstSegment">LBA of the first segment</param>
    /// <param name="entries">Raw name, version and file record of each entry</param>
    ErrorNumber ReadWormSegmentChain(ulong firstSegment, out List<(byte[] name, uint version, byte[] record)> entries)
    {
        entries = [];
        ulong          segment = firstSegment;
        HashSet<ulong> visited = [];

        while(segment != NO_LBA && visited.Add(segment))
        {
            ErrorNumber errno = ReadSectors(segment, _wormSegmentSectors, out byte[] data);

            if(errno != ErrorNumber.NoError) return errno;

            ulong next = NO_LBA;

            for(uint i = 0; i < _wormSegmentSectors; i++)
            {
                var sectorStart = (int)(i * _bytesPerSector);

                // Sectors are written in order, an empty one marks the end of the chain
                if(ArrayHelpers.ArrayIsNullOrEmpty(data[sectorStart..(sectorStart + (int)_bytesPerSector)]))
                    return ErrorNumber.NoError;

                if(i == 0 && !data.Skip(sectorStart).Take(8).SequenceEqual(_segmentSignature))
                {
                    AaruLogging.Debug(MODULE_NAME, "Invalid directory segment at LBA {0}", segment);

                    return ErrorNumber.InvalidArgument;
                }

                int entryStart = sectorStart + (i == 0 ? SEGMENT_FIRST_ENTRY_OFFSET : SEGMENT_ENTRY_OFFSET);

                if(DecodeWormEntry(data, entryStart, sectorStart + (int)_bytesPerSector, out byte[] name,
                                   out uint version, out byte[] record))
                    entries.Add((name, version, record));

                // The last sector of a full segment ends with a pointer to the next segment
                if(i != _wormSegmentSectors - 1) continue;

                int linkOffset = sectorStart + (int)_bytesPerSector - SEGMENT_LINK_SIZE;

                ulong link = (ulong)BigEndianBitConverter.ToUInt16(data, linkOffset + 2) << 32 |
                             BigEndianBitConverter.ToUInt32(data, linkOffset + 4);

                if(BigEndianBitConverter.ToUInt16(data, linkOffset) == SEGMENT_LINK_MARKER && link != 0) next = link;
            }

            segment = next;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Decodes a directory entry ("DESSTR") of a WORM volume</summary>
    /// <param name="data">Buffer</param>
    /// <param name="start">Offset of the entry in the buffer</param>
    /// <param name="end">End of the sector holding the entry</param>
    /// <param name="name">Raw full path of the entry</param>
    /// <param name="version">Version, counting down from 0xFFFFFFFE</param>
    /// <param name="record">File record, up to the end of the sector</param>
    bool DecodeWormEntry(byte[] data, int start, int end, out byte[] name, out uint version, out byte[] record)
    {
        name    = null;
        version = 0;
        record  = null;

        if(start + WORM_ENTRY_HEADER_SIZE > end) return false;

        if(!data.Skip(start).Take(6).SequenceEqual(_entrySignature)) return false;

        // Signature (8 bytes), location of the entry in the parent directory (8 bytes), then the key
        int    key        = start + 16;
        ushort nameLength = BigEndianBitConverter.ToUInt16(data, key + 2);
        int    nameStart  = key + 6;

        if(nameLength == 0 || nameStart + nameLength > end) return false;

        name = new byte[nameLength];
        Array.Copy(data, nameStart, name, 0, nameLength);

        // Version follows the name, aligned to 4 bytes
        int versionOffset = nameStart + nameLength + 3 & ~3;

        if(versionOffset + 4 + FILE_RECORD_SIZE > end) return false;

        version = BigEndianBitConverter.ToUInt32(data, versionOffset);
        record  = new byte[end - versionOffset - 4];
        Array.Copy(data, versionOffset + 4, record, 0, record.Length);

        return true;
    }

    /// <summary>Gets the current entries of a WORM directory</summary>
    /// <param name="directory">Directory entry</param>
    /// <param name="children">Raw name and entry of each current child</param>
    ErrorNumber ReadWormDirectoryEntries(Entry directory, out List<(byte[] name, Entry entry)> children)
    {
        children = [];

        if(directory.Subfiles.Length < SUBFILE_DATA) return ErrorNumber.NoError;

        ErrorNumber errno = GetExtents(directory.Subfiles[SUBFILE_DATA - 1], SUBFILE_DATA, out Extent[] extents);

        if(errno != ErrorNumber.NoError) return errno;

        if(extents.Length == 0) return ErrorNumber.NoError;

        errno = ReadWormSegmentChain(extents[0].Lba, out List<(byte[] name, uint version, byte[] record)> records);

        if(errno != ErrorNumber.NoError) return errno;

        // Last written version of each name is the current one, keep first-seen order
        Dictionary<string, int>          positions = new(StringComparer.Ordinal);
        List<(byte[] name, Entry entry)> current   = [];

        foreach((byte[] name, uint version, byte[] record) in records)
        {
            Entry entry = DecodeRecord(record);

            if(entry is null) continue;

            entry.Version = version;

            string key = Convert.ToHexString(name);

            if(positions.TryGetValue(key, out int position))
                current[position] = (name, entry);
            else
            {
                positions[key] = current.Count;
                current.Add((name, entry));
            }
        }

        children = current.Where(static c => (c.entry.Record.flags & FLAGS_DELETED) == 0).ToList();

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the whole directory hierarchy of a WORM volume</summary>
    /// <param name="vmi">Current VMI block</param>
    ErrorNumber ReadWormDirectory(byte[] vmi)
    {
        _wormSeparator      = _medInfo.rest[SMI_SEPARATOR_OFFSET - SMI_REST_OFFSET];
        _wormSegmentSectors = BigEndianBitConverter.ToUInt16(_medInfo.rest, SMI_SEGMENT_SECTORS_OFFSET - SMI_REST_OFFSET);

        if(_wormSegmentSectors == 0) return ErrorNumber.InvalidArgument;

        // The root anchor segment holds the entry of the root directory
        uint rootAnchor = BigEndianBitConverter.ToUInt32(vmi, VMI_ROOT_ANCHOR_OFFSET);

        ErrorNumber errno = ReadWormSegmentChain(rootAnchor,
                                                 out List<(byte[] name, uint version, byte[] record)> anchor);

        if(errno != ErrorNumber.NoError) return errno;

        _root = null;

        foreach((byte[] name, uint version, byte[] record) in anchor)
        {
            if(name.Length < 2 || name[0] != SYSTEM_NAME_PREFIX || name[1] != SYSTEM_NAME_PREFIX) continue;

            _root = DecodeRecord(record);

            if(_root is not null) _root.Version = version;
        }

        if(_root is null)
        {
            AaruLogging.Debug(MODULE_NAME, "Root directory entry not found");

            return ErrorNumber.InvalidArgument;
        }

        StringComparer comparer = _medInfo.rps == RPS_WORM_MIXED_CASE
                                      ? StringComparer.Ordinal
                                      : StringComparer.OrdinalIgnoreCase;

        _entries  = new Dictionary<string, Entry>(comparer);
        _children = new Dictionary<string, List<string>>(comparer);

        Queue<(string path, Entry entry)> pending = new();
        pending.Enqueue(("", _root));

        while(pending.Count > 0)
        {
            (string path, Entry directory) = pending.Dequeue();

            if(_children.ContainsKey(path)) continue;

            List<string> names = [];
            _children[path] = names;

            errno = ReadWormDirectoryEntries(directory, out List<(byte[] name, Entry entry)> children);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Could not read directory {0}: {1}", path, errno);

                continue;
            }

            foreach((byte[] rawName, Entry entry) in children)
            {
                // Keys are full paths, directories end in a separator
                string[] components = SplitPath(rawName, _wormSeparator)
                                     .Where(static c => c.Length > 0)
                                     .Select(c => StringHandlers.CToString(c, _encoding))
                                     .ToArray();

                if(components.Length == 0) continue;

                string childPath = string.Join("/", components);
                string parent    = components.Length > 1 ? string.Join("/", components[..^1]) : "";

                // Entries are stored in the directory they belong to
                if(!comparer.Equals(parent, path))
                {
                    AaruLogging.Debug(MODULE_NAME, "Entry {0} found in directory {1}", childPath, path);

                    continue;
                }

                if(!_entries.TryAdd(childPath, entry)) continue;

                names.Add(components[^1]);

                if(entry.IsDirectory) pending.Enqueue((childPath, entry));
            }
        }

        AaruLogging.Debug(MODULE_NAME, "Directory contains {0} entries", _entries.Count);

        return ErrorNumber.NoError;
    }
}