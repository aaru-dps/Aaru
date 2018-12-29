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
//     Contains structures for Apple Universal Disk Image Format.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.DiscImages
{
    public partial class Udif
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UdifFooter
        {
            public uint  signature;
            public uint  version;
            public uint  headerSize;
            public uint  flags;
            public ulong runningDataForkOff;
            public ulong dataForkOff;
            public ulong dataForkLen;
            public ulong rsrcForkOff;
            public ulong rsrcForkLen;
            public uint  segmentNumber;
            public uint  segmentCount;
            public Guid  segmentId;
            public uint  dataForkChkType;
            public uint  dataForkChkLen;
            public uint  dataForkChk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reserved1;
            public ulong plistOff;
            public ulong plistLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
            public byte[] reserved2;
            public uint masterChkType;
            public uint masterChkLen;
            public uint masterChk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reserved3;
            public uint  imageVariant;
            public ulong sectorCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockHeader
        {
            public uint  signature;
            public uint  version;
            public ulong sectorStart;
            public ulong sectorCount;
            public ulong dataOffset;
            public uint  buffers;
            public uint  descriptor;
            public uint  reserved1;
            public uint  reserved2;
            public uint  reserved3;
            public uint  reserved4;
            public uint  reserved5;
            public uint  reserved6;
            public uint  checksumType;
            public uint  checksumLen;
            public uint  checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reservedChk;
            public uint chunks;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockChunk
        {
            public uint  type;
            public uint  comment;
            public ulong sector;
            public ulong sectors;
            public ulong offset;
            public ulong length;
        }
    }
}