// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BTree.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads extents, subfiles and B-tree files (directory and free space).
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
    /// <summary>Reads sectors relative to the start of the partition</summary>
    ErrorNumber ReadSectors(ulong lba, uint count, out byte[] buffer)
    {
        buffer = null;

        if(lba + count > _partition.End - _partition.Start + 1) return ErrorNumber.InvalidArgument;

        return _image.ReadSectors(_partition.Start + lba, false, count, out buffer, out _);
    }

    /// <summary>Gets the extents of a subfile, in file order</summary>
    /// <param name="descriptor">Subfile descriptor</param>
    /// <param name="subfile">Subfile number, 1-based, selects the extent list signature</param>
    /// <param name="extents">Extents</param>
    ErrorNumber GetExtents(SubfileDescriptor descriptor, int subfile, out Extent[] extents)
    {
        extents = null;
        List<Extent> list = [];

        ulong firstLba = (ulong)descriptor.firstLbaHigh << 32 | descriptor.firstLbaLow;

        if(firstLba != NO_LBA && descriptor.firstSectors > 0) list.Add(new Extent(firstLba, descriptor.firstSectors));

        ulong secondLba = (ulong)descriptor.secondLbaHigh << 32 | descriptor.secondLbaLow;

        // No extent lists, second extent is inline
        if(descriptor.lists == 0)
        {
            if(secondLba != NO_LBA && descriptor.secondSectors > 0)
                list.Add(new Extent(secondLba, descriptor.secondSectors));

            extents = list.ToArray();

            return ErrorNumber.NoError;
        }

        // Rest of extents are in a chain of extent list blocks, each one sector long
        ulong listLba = secondLba;

        for(uint i = 0; i < descriptor.lists; i++)
        {
            ErrorNumber errno = ReadSectors(listLba, 1, out byte[] block);

            if(errno != ErrorNumber.NoError) return errno;

            ExtentListHeader header =
                Marshal.ByteArrayToStructureBigEndian<ExtentListHeader>(block, 0, EXTENT_LIST_HEADER_SIZE);

            if(!header.signature.SequenceEqual(_extentListSignatures[subfile - 1]))
            {
                AaruLogging.Debug(MODULE_NAME, "Invalid extent list signature at LBA {0}", listLba);

                return ErrorNumber.InvalidArgument;
            }

            var found = 0;

            for(int pos = EXTENT_LIST_HEADER_SIZE; pos + EXTENT_SIZE <= block.Length && found < header.extents;
                pos += EXTENT_SIZE)
            {
                ushort sectors = BigEndianBitConverter.ToUInt16(block, pos);
                ulong  lba     = (ulong)BigEndianBitConverter.ToUInt16(block, pos + 2) << 32 |
                                 BigEndianBitConverter.ToUInt32(block, pos + 4);

                if(lba == NO_LBA) continue;

                found++;

                if(sectors > 0) list.Add(new Extent(lba, sectors));
            }

            ulong next = (ulong)header.nextHigh << 32 | header.nextLow;

            // A single list block has been seen pointing to itself
            if(next == NO_LBA || next == secondLba || next == listLba) break;

            listLba = next;
        }

        extents = list.ToArray();

        return ErrorNumber.NoError;
    }

    /// <summary>Reads part of a subfile given its extents</summary>
    ErrorNumber ReadFromExtents(Extent[] extents, ulong offset, uint length, out byte[] data)
    {
        data = new byte[length];
        ulong filePos = 0;
        uint  done    = 0;

        foreach(Extent extent in extents)
        {
            if(done >= length) break;

            ulong extentBytes = (ulong)extent.Sectors * _bytesPerSector;

            if(offset + done >= filePos + extentBytes)
            {
                filePos += extentBytes;

                continue;
            }

            ulong inExtent    = offset + done - filePos;
            var   firstSector = (uint)(inExtent / _bytesPerSector);
            var   inSector    = (uint)(inExtent % _bytesPerSector);
            ulong toCopy      = Math.Min(extentBytes - inExtent, length - done);
            var   sectors     = (uint)((inSector + toCopy + _bytesPerSector - 1) / _bytesPerSector);

            ErrorNumber errno = ReadSectors(extent.Lba + firstSector, sectors, out byte[] buffer);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(buffer, inSector, data, done, (long)toCopy);
            done    += (uint)toCopy;
            filePos += extentBytes;
        }

        if(done >= length) return ErrorNumber.NoError;

        // Happens on WORM volumes when writing was interrupted before the data was placed
        AaruLogging.Debug(MODULE_NAME, "Extents end before requested data, {0} of {1} bytes", done, length);

        return ErrorNumber.InOutError;
    }

    /// <summary>Reads all leaf entries of a B-tree file, in key order</summary>
    /// <param name="extents">Extents of the B-tree file</param>
    /// <param name="entries">Key and record of each entry</param>
    ErrorNumber ReadBTree(Extent[] extents, out List<(byte[] key, byte[] record)> entries)
    {
        entries = [];

        ErrorNumber errno = ReadFromExtents(extents, 0, _bytesPerSector, out byte[] mastSector);

        if(errno != ErrorNumber.NoError) return errno;

        MasterRecord master = Marshal.ByteArrayToStructureBigEndian<MasterRecord>(mastSector, 0, 0x9C);

        if(!master.signature.SequenceEqual(_mastSignature))
        {
            AaruLogging.Debug(MODULE_NAME, "B-tree master record not found");

            return ErrorNumber.InvalidArgument;
        }

        if(master.nodeSize < NODE_HEADER_SIZE + 2) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME,
                          "B-tree with nodes of {0} bytes, {1} used, root {2}, first leaf {3}",
                          master.nodeSize,
                          master.usedNodes,
                          master.rootNode,
                          master.firstLeaf);

        // Leaves are chained, so all entries can be read in order without walking the index
        uint          node    = master.firstLeaf;
        HashSet<uint> visited = [];

        while(node != 0 && visited.Add(node))
        {
            errno = ReadFromExtents(extents, (ulong)node * master.nodeSize, master.nodeSize, out byte[] data);

            if(errno != ErrorNumber.NoError) return errno;

            NodeHeader header = Marshal.ByteArrayToStructureBigEndian<NodeHeader>(data, 0, NODE_HEADER_SIZE);

            if(!header.signature.SequenceEqual(_dataSignature))
            {
                AaruLogging.Debug(MODULE_NAME, "B-tree node {0} is not a leaf", node);

                return ErrorNumber.InvalidArgument;
            }

            // Slot table at the end of the node holds the offset of each entry, in key order
            for(var i = 0; i < header.entries; i++)
            {
                int slot = data.Length - 2 - i * 2;

                if(slot < NODE_HEADER_SIZE) return ErrorNumber.InvalidArgument;

                ushort offset = BigEndianBitConverter.ToUInt16(data, slot);

                if(offset < NODE_HEADER_SIZE || offset + 4 > slot) return ErrorNumber.InvalidArgument;

                ushort keyLength    = BigEndianBitConverter.ToUInt16(data, offset);
                ushort recordLength = BigEndianBitConverter.ToUInt16(data, offset + 2);

                if(offset + 4 + keyLength + recordLength > slot) return ErrorNumber.InvalidArgument;

                byte[] key    = new byte[keyLength];
                byte[] record = new byte[recordLength];
                Array.Copy(data, offset + 4,             key,    0, keyLength);
                Array.Copy(data, offset + 4 + keyLength, record, 0, recordLength);
                entries.Add((key, record));
            }

            node = header.next;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Decodes a file record and its subfile descriptors</summary>
    static Entry DecodeRecord(byte[] record)
    {
        if(record.Length < FILE_RECORD_SIZE) return null;

        var entry = new Entry
        {
            Record = Marshal.ByteArrayToStructureBigEndian<FileRecord>(record, 0, FILE_RECORD_SIZE)
        };

        int subfiles = (entry.Record.flags & FLAGS_SUBFILES_MASK) >> FLAGS_SUBFILES_SHIFT;

        subfiles = Math.Min(subfiles, (record.Length - FILE_RECORD_SIZE) / SUBFILE_DESCRIPTOR_SIZE);

        entry.Subfiles = new SubfileDescriptor[subfiles];

        for(var i = 0; i < subfiles; i++)
        {
            entry.Subfiles[i] =
                Marshal.ByteArrayToStructureBigEndian<SubfileDescriptor>(record,
                                                                         FILE_RECORD_SIZE +
                                                                         i * SUBFILE_DESCRIPTOR_SIZE,
                                                                         SUBFILE_DESCRIPTOR_SIZE);
        }

        return entry;
    }

    /// <summary>Reads the file record of a system file and gets the extents of its data</summary>
    ErrorNumber GetSystemFileExtents(ulong recordSector, out Extent[] extents)
    {
        extents = null;

        ErrorNumber errno = ReadSectors(recordSector, 1, out byte[] sector);

        if(errno != ErrorNumber.NoError) return errno;

        Entry entry = DecodeRecord(sector);

        if(entry is null || entry.Subfiles.Length < 1) return ErrorNumber.InvalidArgument;

        return GetExtents(entry.Subfiles[0], SUBFILE_DATA, out extents);
    }

    /// <summary>Counts the free sectors listed in the free space B-tree</summary>
    ErrorNumber CountFreeSectors(out ulong free)
    {
        free = 0;

        ErrorNumber errno = GetSystemFileExtents(FREEFILE_RECORD_SECTOR, out Extent[] extents);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadBTree(extents, out List<(byte[] key, byte[] record)> entries);

        if(errno != ErrorNumber.NoError) return errno;

        // Key is a byte and a 48-bit LBA, record is a byte and a 32-bit sector count
        foreach((byte[] _, byte[] record) in entries)
        {
            if(record.Length < 5) continue;

            free += BigEndianBitConverter.ToUInt32(record, 1);
        }

        return ErrorNumber.NoError;
    }
}