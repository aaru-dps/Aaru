// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extents.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class MDFS
{
    /// <summary>Gets the allocation units of a fork</summary>
    /// <param name="location">First allocation unit, or extent record id</param>
    /// <param name="count">Allocation units</param>
    /// <param name="length">Fork length in bytes</param>
    /// <param name="indirect">Location is an extent record id</param>
    /// <param name="extents">Allocation units, in file order</param>
    ErrorNumber GetExtents(uint location, uint count, uint length, bool indirect, out Extent[] extents)
    {
        extents = [];

        if(length == 0) return ErrorNumber.NoError;

        List<Extent> list = [];

        if(!indirect)
        {
            if(count == 0) return ErrorNumber.InvalidArgument;

            list.Add(new Extent(location, count));
        }
        else
        {
            ErrorNumber errno = ReadExtentRecord(location, 0, new HashSet<uint>(), list);

            if(errno != ErrorNumber.NoError) return errno;
        }

        ulong allocated = 0;

        foreach(Extent extent in list) allocated += (ulong)extent.Count * _vd.AllocationUnitBytes;

        if(allocated < length)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Fork of {0} bytes only has {1} bytes allocated",
                              length,
                              allocated);

            return ErrorNumber.InvalidArgument;
        }

        extents = list.ToArray();

        return ErrorNumber.NoError;
    }

    /// <summary>Walks an extent record slot, a leaf with allocation units or an index pointing to other slots</summary>
    ErrorNumber ReadExtentRecord(uint id, int depth, HashSet<uint> visited, List<Extent> extents)
    {
        if(depth > MAX_ERB_DEPTH || !visited.Add(id)) return ErrorNumber.InvalidArgument;

        bool wide     = _vd.WideExtents;
        int  slotSize = wide ? 64 : 32;
        uint sector   = wide ? id >> 16 : id >> 6 & 0x3FF;
        uint slot     = wide ? id & 0xFFFF : id & 0x3F;

        if(slot >= BLOCK_SIZE / slotSize) return ErrorNumber.InvalidArgument;

        ErrorNumber errno = ReadVmaSector(sector, out byte[] buffer);

        if(errno != ErrorNumber.NoError) return errno;

        int recordOffset = (int)(slot * slotSize);

        // Eight pairs of 16-bit or 32-bit values
        uint First(int pair) => wide
                                    ? BigEndianBitConverter.ToUInt32(buffer, recordOffset + pair * 8)
                                    : BigEndianBitConverter.ToUInt16(buffer, recordOffset + pair * 4);

        uint Second(int pair) => wide
                                     ? BigEndianBitConverter.ToUInt32(buffer, recordOffset + pair * 8 + 4)
                                     : BigEndianBitConverter.ToUInt16(buffer, recordOffset + pair * 4 + 2);

        bool isIndex = wide ? First(0) == 0xFFFFFFFF : First(0) == 0xFFFF;

        if(!isIndex)
        {
            for(int pair = 0; pair < 8; pair++)
            {
                uint count = Second(pair);

                if(count == 0) break;

                extents.Add(new Extent(First(pair), count));
            }

            return ErrorNumber.NoError;
        }

        // Index: pairs 1 to 7 hold the logical allocation unit where each child starts and the child id
        for(int pair = 1; pair < 8; pair++)
        {
            uint child = Second(pair);

            if(child == 0) break;

            errno = ReadExtentRecord(child, depth + 1, visited, extents);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads part of a fork</summary>
    ErrorNumber ReadFromExtents(Extent[] extents, ulong offset, uint length, out byte[] data)
    {
        data = new byte[length];

        ulong unitBytes   = _vd.AllocationUnitBytes;
        ulong extentStart = 0;
        uint  done        = 0;

        foreach(Extent extent in extents)
        {
            ulong extentBytes = extent.Count * unitBytes;
            ulong extentEnd   = extentStart + extentBytes;

            if(offset + done >= extentEnd)
            {
                extentStart = extentEnd;

                continue;
            }

            while(done < length && offset + done < extentEnd)
            {
                ulong inExtent   = offset + done - extentStart;
                ulong firstBlock = (ulong)extent.Start * _allocSize + inExtent / BLOCK_SIZE;
                uint  skip       = (uint)(inExtent % BLOCK_SIZE);
                ulong available  = Math.Min(length - done, extentEnd - (offset + done));
                uint  blocks     = (uint)((skip + available + BLOCK_SIZE - 1) / BLOCK_SIZE);

                // Read in reasonably sized chunks
                blocks = Math.Min(blocks, 256);

                ErrorNumber errno = ReadBlocks(firstBlock, blocks, out byte[] buffer);

                if(errno != ErrorNumber.NoError) return errno;

                uint copy = (uint)Math.Min(available, (ulong)blocks * BLOCK_SIZE - skip);

                Array.Copy(buffer, skip, data, done, copy);
                done += copy;
            }

            if(done >= length) break;

            extentStart = extentEnd;
        }

        return done < length ? ErrorNumber.InOutError : ErrorNumber.NoError;
    }

    /// <summary>Gets the extents of the data fork of an entry</summary>
    ErrorNumber GetDataExtents(Entry entry, out Extent[] extents) => GetExtents(entry.DataLocation,
        entry.DataCount,
        entry.Length,
        (entry.Attributes & ATTR_DATA_EXTENT_RECORD) != 0,
        out extents);

    /// <summary>Gets the extents of the resource fork of an entry</summary>
    ErrorNumber GetResourceExtents(Entry entry, out Extent[] extents) => GetExtents(entry.ResourceLocation,
        entry.ResourceCount,
        entry.ResourceLength,
        (entry.Attributes & ATTR_RESOURCE_EXTENT_RECORD) != 0,
        out extents);
}