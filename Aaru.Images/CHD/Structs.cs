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
//     Contains structures for MAME Compressed Hunks of Data disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Chd
{
#region Nested type: CompressedMapHeaderV5

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CompressedMapHeaderV5
    {
        /// <summary>Length of compressed map</summary>
        public readonly uint length;
        /// <summary>Offset of first block (48 bits) and CRC16 of map (16 bits)</summary>
        public readonly ulong startAndCrc;
        /// <summary>Bits used to encode compressed length on map entry</summary>
        public readonly byte bitsUsedToEncodeCompLength;
        /// <summary>Bits used to encode self-refs</summary>
        public readonly byte bitsUsedToEncodeSelfRefs;
        /// <summary>Bits used to encode parent unit refs</summary>
        public readonly byte bitsUsedToEncodeParentUnits;
        public readonly byte reserved;
    }

#endregion

#region Nested type: HeaderV1

    // Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
    // Sectors are fixed at 512 bytes/sector
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HeaderV1
    {
        /// <summary>Magic identifier, 'MComprHD'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] tag;
        /// <summary>Length of header</summary>
        public readonly uint length;
        /// <summary>Image format version</summary>
        public readonly uint version;
        /// <summary>Image flags, <see cref="Flags" /></summary>
        public readonly uint flags;
        /// <summary>Compression algorithm, <see cref="Compression" /></summary>
        public readonly uint compression;
        /// <summary>Sectors per hunk</summary>
        public readonly uint hunksize;
        /// <summary>Total # of hunk in image</summary>
        public readonly uint totalhunks;
        /// <summary>Cylinders on disk</summary>
        public readonly uint cylinders;
        /// <summary>Heads per cylinder</summary>
        public readonly uint heads;
        /// <summary>Sectors per track</summary>
        public readonly uint sectors;
        /// <summary>MD5 of raw data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] md5;
        /// <summary>MD5 of parent file</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] parentmd5;
    }

#endregion

#region Nested type: HeaderV2

    // Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HeaderV2
    {
        /// <summary>Magic identifier, 'MComprHD'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] tag;
        /// <summary>Length of header</summary>
        public readonly uint length;
        /// <summary>Image format version</summary>
        public readonly uint version;
        /// <summary>Image flags, <see cref="Flags" /></summary>
        public readonly uint flags;
        /// <summary>Compression algorithm, <see cref="Compression" /></summary>
        public readonly uint compression;
        /// <summary>Sectors per hunk</summary>
        public readonly uint hunksize;
        /// <summary>Total # of hunk in image</summary>
        public readonly uint totalhunks;
        /// <summary>Cylinders on disk</summary>
        public readonly uint cylinders;
        /// <summary>Heads per cylinder</summary>
        public readonly uint heads;
        /// <summary>Sectors per track</summary>
        public readonly uint sectors;
        /// <summary>MD5 of raw data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] md5;
        /// <summary>MD5 of parent file</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] parentmd5;
        /// <summary>Bytes per sector</summary>
        public readonly uint seclen;
    }

#endregion

#region Nested type: HeaderV3

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HeaderV3
    {
        /// <summary>Magic identifier, 'MComprHD'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] tag;
        /// <summary>Length of header</summary>
        public readonly uint length;
        /// <summary>Image format version</summary>
        public readonly uint version;
        /// <summary>Image flags, <see cref="Flags" /></summary>
        public readonly uint flags;
        /// <summary>Compression algorithm, <see cref="Compression" /></summary>
        public readonly uint compression;
        /// <summary>Total # of hunk in image</summary>
        public readonly uint totalhunks;
        /// <summary>Total bytes in image</summary>
        public readonly ulong logicalbytes;
        /// <summary>Offset to first metadata blob</summary>
        public readonly ulong metaoffset;
        /// <summary>MD5 of raw data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] md5;
        /// <summary>MD5 of parent file</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] parentmd5;
        /// <summary>Bytes per hunk</summary>
        public readonly uint hunkbytes;
        /// <summary>SHA1 of raw data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] sha1;
        /// <summary>SHA1 of parent file</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] parentsha1;
    }

#endregion

#region Nested type: HeaderV4

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HeaderV4
    {
        /// <summary>Magic identifier, 'MComprHD'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] tag;
        /// <summary>Length of header</summary>
        public readonly uint length;
        /// <summary>Image format version</summary>
        public readonly uint version;
        /// <summary>Image flags, <see cref="Flags" /></summary>
        public readonly uint flags;
        /// <summary>Compression algorithm, <see cref="Compression" /></summary>
        public readonly uint compression;
        /// <summary>Total # of hunk in image</summary>
        public readonly uint totalhunks;
        /// <summary>Total bytes in image</summary>
        public readonly ulong logicalbytes;
        /// <summary>Offset to first metadata blob</summary>
        public readonly ulong metaoffset;
        /// <summary>Bytes per hunk</summary>
        public readonly uint hunkbytes;
        /// <summary>SHA1 of raw+meta data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] sha1;
        /// <summary>SHA1 of parent file</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] parentsha1;
        /// <summary>SHA1 of raw data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] rawsha1;
    }

#endregion

#region Nested type: HeaderV5

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HeaderV5
    {
        /// <summary>Magic identifier, 'MComprHD'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] tag;
        /// <summary>Length of header</summary>
        public readonly uint length;
        /// <summary>Image format version</summary>
        public readonly uint version;
        /// <summary>Compressor 0</summary>
        public readonly uint compressor0;
        /// <summary>Compressor 1</summary>
        public readonly uint compressor1;
        /// <summary>Compressor 2</summary>
        public readonly uint compressor2;
        /// <summary>Compressor 3</summary>
        public readonly uint compressor3;
        /// <summary>Total bytes in image</summary>
        public readonly ulong logicalbytes;
        /// <summary>Offset to hunk map</summary>
        public readonly ulong mapoffset;
        /// <summary>Offset to first metadata blob</summary>
        public readonly ulong metaoffset;
        /// <summary>Bytes per hunk</summary>
        public readonly uint hunkbytes;
        /// <summary>Bytes per unit within hunk</summary>
        public readonly uint unitbytes;
        /// <summary>SHA1 of raw data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] rawsha1;
        /// <summary>SHA1 of raw+meta data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] sha1;
        /// <summary>SHA1 of parent file</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] parentsha1;
    }

#endregion

#region Nested type: HunkSector

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HunkSector
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly ulong[] hunkEntry;
    }

#endregion

#region Nested type: HunkSectorSmall

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HunkSectorSmall
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly uint[] hunkEntry;
    }

#endregion

#region Nested type: MapEntryV3

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MapEntryV3
    {
        /// <summary>Offset to hunk from start of image</summary>
        public readonly ulong offset;
        /// <summary>CRC32 of uncompressed hunk</summary>
        public readonly uint crc;
        /// <summary>Lower 16 bits of length</summary>
        public readonly ushort lengthLsb;
        /// <summary>Upper 8 bits of length</summary>
        public readonly byte length;
        /// <summary>Hunk flags</summary>
        public readonly byte flags;
    }

#endregion

#region Nested type: MapEntryV5

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MapEntryV5
    {
        /// <summary>Compression (8 bits) and length (24 bits)</summary>
        public readonly uint compAndLength;
        /// <summary>Offset (48 bits) and CRC (16 bits)</summary>
        public readonly ulong offsetAndCrc;
    }

#endregion

#region Nested type: MetadataHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MetadataHeader
    {
        public readonly uint  tag;
        public readonly uint  flagsAndLength;
        public readonly ulong next;
    }

#endregion

#region Nested type: TrackOld

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TrackOld
    {
        public uint type;
        public uint subType;
        public uint dataSize;
        public uint subSize;
        public uint frames;
        public uint extraFrames;
    }

#endregion
}