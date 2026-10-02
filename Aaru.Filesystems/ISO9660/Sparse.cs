// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Sparse.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Transparent decoding of Rock Ridge sparse files.
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
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    /// <summary>Size of a sparse file index block, and of the units its table pointers count</summary>
    const int SPARSE_INDEX_BLOCK_SIZE = 2048;
    /// <summary>Size of the units sparse file data pointers count</summary>
    const int SPARSE_DATA_BLOCK_SIZE = 256;
    /// <summary>Table entries in a sparse file index block</summary>
    const int SPARSE_TABLE_ENTRIES = 256;
    /// <summary>Deepest index block allowed by RRIP 1.12, whose entries cover 64K TB each</summary>
    const int SPARSE_MAX_DEPTH = 7;
    /// <summary>Index block depth implied by the RRIP 1.10 "SF" field, which has 32-bit sizes</summary>
    const byte SPARSE_RRIP110_DEPTH = 3;
    const uint SPARSE_ENTRY_BLOCK_MASK = 0x00FFFFFF;
    const uint SPARSE_ENTRY_TABLE      = 0x40000000;
    const uint SPARSE_ENTRY_EMPTY      = 0x80000000;

    /// <summary>Reads and decodes a Rock Ridge sparse file.</summary>
    /// <param name="offset">Offset within the virtual file to start reading from.</param>
    /// <param name="size">Number of virtual file bytes to read.</param>
    /// <param name="entry">Directory entry containing the sparse file information.</param>
    /// <param name="buffer">Buffer to store the decoded data.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadSparseFile(long offset, long size, DecodedDirectoryEntry entry, out byte[] buffer)
    {
        buffer = null;

        if(entry.RripSparseSize is null || entry.Extents is null || entry.Extents.Count == 0)
            return ErrorNumber.InvalidArgument;

        var virtualSize = (long)entry.RripSparseSize.Value;

        if(offset < 0 || offset >= virtualSize)
        {
            buffer = [];

            return ErrorNumber.NoError;
        }

        if(offset + size > virtualSize) size = virtualSize - offset;

        if(entry.RripSparseDepth is < 1 or > SPARSE_MAX_DEPTH) return ErrorNumber.InvalidArgument;

        // Regions not covered by any data entry stay as zeros
        buffer = new byte[size];

        // The first index block is always the second 2K block of the file section (RRIP 4.1.7.1)
        return DecodeSparseIndexBlock(entry, 1, entry.RripSparseDepth, 0, offset, size, buffer);
    }

    /// <summary>Decodes the entries of a sparse file index block that overlap the requested range</summary>
    /// <param name="entry">Directory entry containing the sparse file information.</param>
    /// <param name="indexBlock">Index block location, in 2K blocks from the start of the file section.</param>
    /// <param name="depth">Depth of the index block, an entry covers 256^depth bytes of the virtual file.</param>
    /// <param name="regionStart">Virtual file offset covered by the first entry of this index block.</param>
    /// <param name="offset">Virtual file offset of the first byte of <paramref name="buffer" />.</param>
    /// <param name="size">Number of bytes requested.</param>
    /// <param name="buffer">Buffer receiving the decoded data.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber DecodeSparseIndexBlock(DecodedDirectoryEntry entry, uint indexBlock, int depth, long regionStart,
                                       long offset, long size, byte[] buffer)
    {
        ErrorNumber errno = ReadSparseSection(entry,
                                              (long)indexBlock * SPARSE_INDEX_BLOCK_SIZE,
                                              SPARSE_INDEX_BLOCK_SIZE,
                                              out byte[] table);

        if(errno != ErrorNumber.NoError) return errno;

        if(table.Length < SPARSE_INDEX_BLOCK_SIZE) return ErrorNumber.InvalidArgument;

        // Each entry covers 256^depth bytes, depths above 7 are rejected before getting here
        long span = 1L << 8 * depth;
        long end  = offset + size;

        for(var i = 0; i < SPARSE_TABLE_ENTRIES; i++)
        {
            // Further entries of a depth 7 table would overflow, and start past the largest possible file anyway
            if(depth == SPARSE_MAX_DEPTH && i > 0) break;

            long entryStart = regionStart + i * span;

            if(entryStart >= end) break;

            if(entryStart + span <= offset) continue;

            // Recorded as a both-endian 32-bit number (ISO 9660 7.3.3), the little endian half comes first
            var  value = BitConverter.ToUInt32(table, i * 8);
            uint block = value & SPARSE_ENTRY_BLOCK_MASK;

            if((value & SPARSE_ENTRY_EMPTY) != 0) continue;

            if((value & SPARSE_ENTRY_TABLE) != 0)
            {
                // Each table is one level shallower, so a depth 1 table cannot point to another one
                if(depth == 1) return ErrorNumber.InvalidArgument;

                errno = DecodeSparseIndexBlock(entry,
                                               block,
                                               depth - 1,
                                               entryStart,
                                               offset,
                                               size,
                                               buffer);

                if(errno != ErrorNumber.NoError) return errno;

                continue;
            }

            // Contiguous data, located in 256 byte blocks from the start of the file section
            long copyStart = Math.Max(entryStart, offset);
            long copyEnd   = Math.Min(entryStart + span, end);

            errno = ReadSparseSection(entry,
                                      (long)block * SPARSE_DATA_BLOCK_SIZE + (copyStart - entryStart),
                                      copyEnd - copyStart,
                                      out byte[] data);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(data, 0, buffer, copyStart - offset, Math.Min(data.Length, copyEnd - copyStart));
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads bytes from the encoded file section of a sparse file</summary>
    /// <param name="entry">Directory entry containing the sparse file information.</param>
    /// <param name="sectionOffset">Offset from the start of the file section.</param>
    /// <param name="length">Number of bytes to read.</param>
    /// <param name="buffer">Buffer with the read bytes, shorter if the file section ends before.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadSparseSection(DecodedDirectoryEntry entry, long sectionOffset, long length, out byte[] buffer)
    {
        long sectionSize = (long)entry.Size;

        if(sectionOffset >= sectionSize)
        {
            buffer = [];

            return ErrorNumber.NoError;
        }

        if(sectionOffset + length > sectionSize) length = sectionSize - sectionOffset;

        return ReadWithExtents(sectionOffset + entry.XattrLength * _blockSize,
                               length,
                               entry.Extents,
                               false,
                               0,
                               out buffer);
    }
}