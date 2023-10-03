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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Aaru.DiscImages;

public sealed partial class Udif
{
#region Nested type: BlockChunk

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BlockChunk
    {
        public          uint  type;
        public readonly uint  comment;
        public          ulong sector;
        public          ulong sectors;
        public          ulong offset;
        public          ulong length;
    }

#endregion

#region Nested type: BlockHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BlockHeader
    {
        public          uint  signature;
        public          uint  version;
        public readonly ulong sectorStart;
        public          ulong sectorCount;
        public readonly ulong dataOffset;
        public readonly uint  buffers;
        public readonly uint  descriptor;
        public readonly uint  reserved1;
        public readonly uint  reserved2;
        public readonly uint  reserved3;
        public readonly uint  reserved4;
        public readonly uint  reserved5;
        public readonly uint  reserved6;
        public          uint  checksumType;
        public          uint  checksumLen;
        public          uint  checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
        public readonly byte[] reservedChk;
        public uint chunks;
    }

#endregion

#region Nested type: Footer

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Footer
    {
        public          uint  signature;
        public          uint  version;
        public          uint  headerSize;
        public          uint  flags;
        public readonly ulong runningDataForkOff;
        public readonly ulong dataForkOff;
        public          ulong dataForkLen;
        public readonly ulong rsrcForkOff;
        public readonly ulong rsrcForkLen;
        public          uint  segmentNumber;
        public          uint  segmentCount;
        public          Guid  segmentId;
        public          uint  dataForkChkType;
        public          uint  dataForkChkLen;
        public          uint  dataForkChk;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
        public readonly byte[] reserved1;
        public ulong plistOff;
        public ulong plistLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
        public readonly byte[] reserved2;
        public readonly uint masterChkType;
        public readonly uint masterChkLen;
        public readonly uint masterChk;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
        public readonly byte[] reserved3;
        public uint  imageVariant;
        public ulong sectorCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] reserved4;
    }

#endregion
}