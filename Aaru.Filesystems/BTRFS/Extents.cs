// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extents.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Extent data extraction methods.
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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class BTRFS
{
    /// <summary>Walks a tree node recursively to collect all EXTENT_DATA items for the specified objectid</summary>
    /// <param name="nodeData">Raw tree node data</param>
    /// <param name="header">Parsed node header</param>
    /// <param name="objectId">The objectid to search for</param>
    /// <param name="extents">List to add found extents to</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber WalkTreeForExtents(byte[] nodeData, in Header header, ulong objectId, List<ExtentEntry> extents)
    {
        int headerSize = Marshal.SizeOf<Header>();

        if(header.level == 0) return ExtractExtentsFromLeaf(nodeData, header, objectId, extents);

        int keyPtrSize = Marshal.SizeOf<KeyPtr>();

        BinarySearchNodeRange(nodeData, header, objectId, out int startIdx, out int endIdx);

        for(int i = startIdx; i < endIdx; i++)
        {
            int keyPtrOffset = headerSize + i * keyPtrSize;

            if(keyPtrOffset + keyPtrSize > nodeData.Length) break;

            KeyPtr keyPtr = Marshal.ByteArrayToStructureLittleEndian<KeyPtr>(nodeData, keyPtrOffset, keyPtrSize);

            ErrorNumber errno = ReadTreeBlock(keyPtr.blockptr, out byte[] childData);

            if(errno != ErrorNumber.NoError) continue;

            Header childHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(childData);

            WalkTreeForExtents(childData, childHeader, objectId, extents);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Extracts all EXTENT_DATA items from a leaf node for the specified objectid</summary>
    /// <param name="leafData">Raw leaf node data</param>
    /// <param name="header">Parsed leaf header</param>
    /// <param name="objectId">The objectid to search for</param>
    /// <param name="extents">List to add found extents to</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ExtractExtentsFromLeaf(byte[] leafData, in Header header, ulong objectId, List<ExtentEntry> extents)
    {
        int itemSize        = Marshal.SizeOf<Item>();
        int headerSize      = Marshal.SizeOf<Header>();
        int extentItemSize  = Marshal.SizeOf<FileExtentItem>();
        var inlineHeaderLen = 21; // FileExtentItem fields before the disk_bytenr (for inline extents)

        uint startItem = BinarySearchLeaf(leafData, header, objectId);

        for(uint i = startItem; i < header.nritems; i++)
        {
            int itemOffset = headerSize + (int)i * itemSize;

            if(itemOffset + itemSize > leafData.Length) break;

            Item item = Marshal.ByteArrayToStructureLittleEndian<Item>(leafData, itemOffset, itemSize);

            if(item.key.objectid > objectId) break;

            if(item.key.objectid != objectId || item.key.type != BTRFS_EXTENT_DATA_KEY) continue;

            int dataOffset = headerSize + (int)item.offset;

            if(dataOffset < headerSize || dataOffset + inlineHeaderLen > leafData.Length) continue;

            FileExtentItem extentItem =
                Marshal.ByteArrayToStructureLittleEndian<FileExtentItem>(leafData,
                                                                         dataOffset,
                                                                         Math.Min(extentItemSize, (int)item.size));

            var entry = new ExtentEntry
            {
                FileOffset  = item.key.offset,
                Type        = extentItem.type,
                Compression = extentItem.compression
            };

            entry.RamBytes = extentItem.ram_bytes;

            if(extentItem.type == BTRFS_FILE_EXTENT_INLINE)
            {
                // Inline data: starts after the 21-byte header (generation + ram_bytes + compression + encryption +
                // other_encoding + type)
                int inlineDataOffset = dataOffset     + inlineHeaderLen;
                int inlineDataLen    = (int)item.size - inlineHeaderLen;

                if(inlineDataLen > 0 && inlineDataOffset + inlineDataLen <= leafData.Length)
                {
                    entry.InlineData = new byte[inlineDataLen];
                    Array.Copy(leafData, inlineDataOffset, entry.InlineData, 0, inlineDataLen);
                    entry.Length = extentItem.ram_bytes;
                }
                else
                {
                    entry.InlineData = [];
                    entry.Length     = 0;
                }
            }
            else
            {
                // REG or PREALLOC
                entry.DiskBytenr   = extentItem.disk_bytenr;
                entry.DiskBytes    = extentItem.disk_num_bytes;
                entry.ExtentOffset = extentItem.offset;
                entry.Length       = extentItem.num_bytes;
            }

            extents.Add(entry);
        }

        return ErrorNumber.NoError;
    }
}