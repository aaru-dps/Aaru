// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS old filesystem plugin.
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

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Attributes;

namespace Aaru.Filesystems;

public sealed partial class BOFS
{
    const int DIR_TYPE = -1; // SDIR - subdirectory marker
    /// <summary>Version used by BeOS 1.1d6, has no root mode in the superblock nor mode in file entries</summary>
    const int VERSION_2 = 0x20000;
    /// <summary>Version used from BeOS 1.1d7 to DR8</summary>
    const int VERSION_3 = 0x30000;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Track0
    {
        /// <summary>
        ///     Version number
        /// </summary>
        public int VersionNumber;
        /// <summary>
        ///     Formatting date
        /// </summary>
        public int FormatDate;
        /// <summary>
        ///     First allocation bitmap sector
        /// </summary>
        public int FirstBitMapSector;
        /// <summary>
        ///     Number of sectors in the allocation bitmap
        /// </summary>
        public int BitMapSize;
        /// <summary>
        ///     Start of directory
        /// </summary>
        public int FirstDirectorySector;
        /// <summary>
        ///     How many logical sectors in volume
        /// </summary>
        public int TotalSectors;
        /// <summary>
        ///     Bytes per sector
        /// </summary>
        public int BytesPerSector;
        /// <summary>
        ///     Hint of first free block for a directory
        /// </summary>
        public int DirectoryBlockHint;
        /// <summary>
        ///     Hint of first free sector for data
        /// </summary>
        public int FreeSectorHint;
        /// <summary>
        ///     Number of used sectors
        /// </summary>
        public int SectorsUsed;
        /// <summary>
        ///     Non-zero for removable media
        /// </summary>
        public byte Removable;
        /// <summary>
        ///     Filler
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Padding;
        /// <summary>
        ///     Volume name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] VolumeName;
        /// <summary>
        ///     Non-zero if cleanly unmounted
        /// </summary>
        public int CleanShutdown;
        /// <summary>
        ///     Root reference
        /// </summary>
        public int RootReference;
        /// <summary>
        ///     Root mode, only on version 3, version 2 has filler here
        /// </summary>
        public int RootMode;
        /// <summary>
        ///     Padding to fill 512 bytes
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 107)]
        public int[] Padding2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct DirectoryBlockHeader
    {
        public int NextDirectoryBlock;
        public int LinkUp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public int[] Filler;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct FileEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] FileName;
        public int FirstAllocList;
        public int LastAllocList;
        public int FileType;
        public int CreationDate;
        public int ModificationDate;
        public int LogicalSize;
        public int PhysicalSize;
        public int Creator;
        public int RecordId;
        public int Mode;
        public int unused2;
        public int unused3;
        public int unused4;
        public int unused5;
        public int unused6;
        public int unused7;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct DirectoryBlock
    {
        public DirectoryBlockHeader Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public FileEntry[] Entries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] Filler;
    }
}