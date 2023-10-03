// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MicroDOS filesystem plugin
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

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>
///     Implements detection for the MicroDOS filesystem. Information from http://www.owg.ru/mkt/BK/MKDOS.TXT Thanks
///     to tarlabnor for translating it
/// </summary>
public sealed partial class MicroDOS
{
#region Nested type: Block0

    // Followed by directory entries
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Block0
    {
        /// <summary>BK starts booting here</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public readonly byte[] bootCode;
        /// <summary>Number of files in directory</summary>
        public readonly ushort files;
        /// <summary>Total number of blocks in files of the directory</summary>
        public readonly ushort usedBlocks;
        /// <summary>Unknown</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 228)]
        public readonly byte[] unknown;
        /// <summary>Ownership label (label that shows it belongs to Micro DOS format)</summary>
        public readonly ushort label;
        /// <summary>MK-DOS directory format label</summary>
        public readonly ushort mklabel;
        /// <summary>Unknown</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public readonly byte[] unknown2;
        /// <summary>
        ///     Disk size in blocks (absolute value for the system unlike NORD, NORTON etc.) that doesn't use two fixed values
        ///     40 or 80 tracks, but i.e. if you drive works with 76 tracks this field will contain an appropriate number of blocks
        /// </summary>
        public readonly ushort blocks;
        /// <summary> Number of the first file's block. Value is changable</summary>
        public readonly ushort firstUsedBlock;
        /// <summary>Unknown</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] unknown3;
    }

#endregion

#region Nested type: DirectoryEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        /// <summary>File status</summary>
        public readonly byte status;
        /// <summary>Directory number (0 - root)</summary>
        public readonly byte directory;
        /// <summary>File name 14. symbols in ASCII KOI8</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] filename;
        /// <summary>Block number</summary>
        public readonly ushort blockNo;
        /// <summary>Length in blocks</summary>
        public readonly ushort blocks;
        /// <summary>Address</summary>
        public readonly ushort address;
        /// <summary>Length</summary>
        public readonly ushort length;
    }

#endregion
}