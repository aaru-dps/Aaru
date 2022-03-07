// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for Apple New Disk Image Format.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Ndif
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ChunkHeader
    {
        /// <summary>Version</summary>
        public readonly short version;
        /// <summary>Filesystem ID</summary>
        public readonly short driver;
        /// <summary>Disk image name, Str63 (Pascal string)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] name;
        /// <summary>Sectors in image</summary>
        public readonly uint sectors;
        /// <summary>Maximum number of sectors per chunk</summary>
        public readonly uint maxSectorsPerChunk;
        /// <summary>Offset to add to every chunk offset</summary>
        public readonly uint dataOffset;
        /// <summary>CRC28 of whole image</summary>
        public readonly uint crc;
        /// <summary>Set to 1 if segmented</summary>
        public readonly uint segmented;
        /// <summary>Unknown</summary>
        public readonly uint p1;
        /// <summary>Unknown</summary>
        public readonly uint p2;
        /// <summary>Unknown, spare?</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly uint[] unknown;
        /// <summary>Set to 1 by ShrinkWrap if image is encrypted</summary>
        public readonly uint encrypted;
        /// <summary>Set by ShrinkWrap if image is encrypted, value is the same for same password</summary>
        public readonly uint hash;
        /// <summary>How many chunks follow the header</summary>
        public readonly uint chunks;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BlockChunk
    {
        /// <summary>Starting sector, 3 bytes</summary>
        public uint sector;
        /// <summary>Chunk type</summary>
        public byte type;
        /// <summary>Offset in start of chunk</summary>
        public uint offset;
        /// <summary>Length in bytes of chunk</summary>
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SegmentHeader
    {
        /// <summary>Segment #</summary>
        public readonly ushort segment;
        /// <summary>How many segments</summary>
        public readonly ushort segments;
        /// <summary>Seems to be a Guid, changes with different images, same for all segments of same image</summary>
        public readonly Guid segmentId;
        /// <summary>Seems to be a CRC28 of this segment, unchecked</summary>
        public readonly uint crc;
    }
}