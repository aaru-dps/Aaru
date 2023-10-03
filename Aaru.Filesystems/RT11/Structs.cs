// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : RT-11 file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the RT-11 file system and shows information.
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

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from http://www.trailing-edge.com/~shoppa/rt11fs/
/// <inheritdoc />
/// <summary>Implements detection of the DEC RT-11 filesystem</summary>
public sealed partial class RT11 : IFilesystem
{
#region Nested type: HomeBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HomeBlock
    {
        /// <summary>Bad block replacement table</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 130)]
        public readonly byte[] badBlockTable;
        /// <summary>Unused</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] unused;
        /// <summary>INITIALIZE/RESTORE data area</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)]
        public readonly byte[] initArea;
        /// <summary>BUP information area</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        public readonly byte[] bupInformation;
        /// <summary>Empty</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public readonly byte[] empty;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] reserved1;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] empty2;
        /// <summary>Cluster size</summary>
        public readonly ushort cluster;
        /// <summary>Block of the first directory segment</summary>
        public readonly ushort rootBlock;
        /// <summary>"V3A" in Radix-50</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] systemVersion;
        /// <summary>Name of the volume, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] volname;
        /// <summary>Name of the volume owner, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] ownername;
        /// <summary>RT11 defines it as "DECRT11A    ", 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] format;
        /// <summary>Unused</summary>
        public readonly ushort unused2;
        /// <summary>Checksum of preceding 255 words (16 bit units)</summary>
        public readonly ushort checksum;
    }

#endregion
}