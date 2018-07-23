// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.DiscImages
{
    public partial class Chd
    {
        // Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
        // Sectors are fixed at 512 bytes/sector
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV1
        {
            /// <summary>
            ///     Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] tag;
            /// <summary>
            ///     Length of header
            /// </summary>
            public uint length;
            /// <summary>
            ///     Image format version
            /// </summary>
            public uint version;
            /// <summary>
            ///     Image flags, <see cref="ChdFlags" />
            /// </summary>
            public uint flags;
            /// <summary>
            ///     Compression algorithm, <see cref="ChdCompression" />
            /// </summary>
            public uint compression;
            /// <summary>
            ///     Sectors per hunk
            /// </summary>
            public uint hunksize;
            /// <summary>
            ///     Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            ///     Cylinders on disk
            /// </summary>
            public uint cylinders;
            /// <summary>
            ///     Heads per cylinder
            /// </summary>
            public uint heads;
            /// <summary>
            ///     Sectors per track
            /// </summary>
            public uint sectors;
            /// <summary>
            ///     MD5 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] md5;
            /// <summary>
            ///     MD5 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] parentmd5;
        }

        // Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV2
        {
            /// <summary>
            ///     Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] tag;
            /// <summary>
            ///     Length of header
            /// </summary>
            public uint length;
            /// <summary>
            ///     Image format version
            /// </summary>
            public uint version;
            /// <summary>
            ///     Image flags, <see cref="ChdFlags" />
            /// </summary>
            public uint flags;
            /// <summary>
            ///     Compression algorithm, <see cref="ChdCompression" />
            /// </summary>
            public uint compression;
            /// <summary>
            ///     Sectors per hunk
            /// </summary>
            public uint hunksize;
            /// <summary>
            ///     Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            ///     Cylinders on disk
            /// </summary>
            public uint cylinders;
            /// <summary>
            ///     Heads per cylinder
            /// </summary>
            public uint heads;
            /// <summary>
            ///     Sectors per track
            /// </summary>
            public uint sectors;
            /// <summary>
            ///     MD5 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] md5;
            /// <summary>
            ///     MD5 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] parentmd5;
            /// <summary>
            ///     Bytes per sector
            /// </summary>
            public uint seclen;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV3
        {
            /// <summary>
            ///     Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] tag;
            /// <summary>
            ///     Length of header
            /// </summary>
            public uint length;
            /// <summary>
            ///     Image format version
            /// </summary>
            public uint version;
            /// <summary>
            ///     Image flags, <see cref="ChdFlags" />
            /// </summary>
            public uint flags;
            /// <summary>
            ///     Compression algorithm, <see cref="ChdCompression" />
            /// </summary>
            public uint compression;
            /// <summary>
            ///     Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            ///     Total bytes in image
            /// </summary>
            public ulong logicalbytes;
            /// <summary>
            ///     Offset to first metadata blob
            /// </summary>
            public ulong metaoffset;
            /// <summary>
            ///     MD5 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] md5;
            /// <summary>
            ///     MD5 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] parentmd5;
            /// <summary>
            ///     Bytes per hunk
            /// </summary>
            public uint hunkbytes;
            /// <summary>
            ///     SHA1 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] sha1;
            /// <summary>
            ///     SHA1 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] parentsha1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdMapV3Entry
        {
            /// <summary>
            ///     Offset to hunk from start of image
            /// </summary>
            public ulong offset;
            /// <summary>
            ///     CRC32 of uncompressed hunk
            /// </summary>
            public uint crc;
            /// <summary>
            ///     Lower 16 bits of length
            /// </summary>
            public ushort lengthLsb;
            /// <summary>
            ///     Upper 8 bits of length
            /// </summary>
            public byte length;
            /// <summary>
            ///     Hunk flags
            /// </summary>
            public byte flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdTrackOld
        {
            public uint type;
            public uint subType;
            public uint dataSize;
            public uint subSize;
            public uint frames;
            public uint extraFrames;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV4
        {
            /// <summary>
            ///     Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] tag;
            /// <summary>
            ///     Length of header
            /// </summary>
            public uint length;
            /// <summary>
            ///     Image format version
            /// </summary>
            public uint version;
            /// <summary>
            ///     Image flags, <see cref="ChdFlags" />
            /// </summary>
            public uint flags;
            /// <summary>
            ///     Compression algorithm, <see cref="ChdCompression" />
            /// </summary>
            public uint compression;
            /// <summary>
            ///     Total # of hunk in image
            /// </summary>
            public uint totalhunks;
            /// <summary>
            ///     Total bytes in image
            /// </summary>
            public ulong logicalbytes;
            /// <summary>
            ///     Offset to first metadata blob
            /// </summary>
            public ulong metaoffset;
            /// <summary>
            ///     Bytes per hunk
            /// </summary>
            public uint hunkbytes;
            /// <summary>
            ///     SHA1 of raw+meta data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] sha1;
            /// <summary>
            ///     SHA1 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] parentsha1;
            /// <summary>
            ///     SHA1 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] rawsha1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdHeaderV5
        {
            /// <summary>
            ///     Magic identifier, 'MComprHD'
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] tag;
            /// <summary>
            ///     Length of header
            /// </summary>
            public uint length;
            /// <summary>
            ///     Image format version
            /// </summary>
            public uint version;
            /// <summary>
            ///     Compressor 0
            /// </summary>
            public uint compressor0;
            /// <summary>
            ///     Compressor 1
            /// </summary>
            public uint compressor1;
            /// <summary>
            ///     Compressor 2
            /// </summary>
            public uint compressor2;
            /// <summary>
            ///     Compressor 3
            /// </summary>
            public uint compressor3;
            /// <summary>
            ///     Total bytes in image
            /// </summary>
            public ulong logicalbytes;
            /// <summary>
            ///     Offset to hunk map
            /// </summary>
            public ulong mapoffset;
            /// <summary>
            ///     Offset to first metadata blob
            /// </summary>
            public ulong metaoffset;
            /// <summary>
            ///     Bytes per hunk
            /// </summary>
            public uint hunkbytes;
            /// <summary>
            ///     Bytes per unit within hunk
            /// </summary>
            public uint unitbytes;
            /// <summary>
            ///     SHA1 of raw data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] rawsha1;
            /// <summary>
            ///     SHA1 of raw+meta data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] sha1;
            /// <summary>
            ///     SHA1 of parent file
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] parentsha1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdCompressedMapHeaderV5
        {
            /// <summary>
            ///     Length of compressed map
            /// </summary>
            public uint length;
            /// <summary>
            ///     Offset of first block (48 bits) and CRC16 of map (16 bits)
            /// </summary>
            public ulong startAndCrc;
            /// <summary>
            ///     Bits used to encode compressed length on map entry
            /// </summary>
            public byte bitsUsedToEncodeCompLength;
            /// <summary>
            ///     Bits used to encode self-refs
            /// </summary>
            public byte bitsUsedToEncodeSelfRefs;
            /// <summary>
            ///     Bits used to encode parent unit refs
            /// </summary>
            public byte bitsUsedToEncodeParentUnits;
            public byte reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdMapV5Entry
        {
            /// <summary>
            ///     Compression (8 bits) and length (24 bits)
            /// </summary>
            public uint compAndLength;
            /// <summary>
            ///     Offset (48 bits) and CRC (16 bits)
            /// </summary>
            public ulong offsetAndCrc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChdMetadataHeader
        {
            public uint  tag;
            public uint  flagsAndLength;
            public ulong next;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HunkSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public ulong[] hunkEntry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HunkSectorSmall
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public uint[] hunkEntry;
        }
    }
}