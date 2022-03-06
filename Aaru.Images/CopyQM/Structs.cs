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
//     Contains structures for Sydex CopyQM disk images.
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

using System.Runtime.InteropServices;

namespace Aaru.DiscImages;

public sealed partial class CopyQm
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        /// <summary>0x00 magic, "CQ"</summary>
        public readonly ushort magic;
        /// <summary>0x02 always 0x14</summary>
        public readonly byte mark;
        /// <summary>0x03 Bytes per sector (part of FAT's BPB)</summary>
        public readonly ushort sectorSize;
        /// <summary>0x05 Sectors per cluster (part of FAT's BPB)</summary>
        public readonly byte sectorPerCluster;
        /// <summary>0x06 Reserved sectors (part of FAT's BPB)</summary>
        public readonly ushort reservedSectors;
        /// <summary>0x08 Number of FAT copies (part of FAT's BPB)</summary>
        public readonly byte fatCopy;
        /// <summary>0x09 Maximum number of entries in root directory (part of FAT's BPB)</summary>
        public readonly ushort rootEntries;
        /// <summary>0x0B Sectors on disk (part of FAT's BPB)</summary>
        public readonly ushort sectors;
        /// <summary>0x0D Media descriptor (part of FAT's BPB)</summary>
        public readonly byte mediaType;
        /// <summary>0x0E Sectors per FAT (part of FAT's BPB)</summary>
        public readonly ushort sectorsPerFat;
        /// <summary>0x10 Sectors per track (part of FAT's BPB)</summary>
        public readonly ushort sectorsPerTrack;
        /// <summary>0x12 Heads (part of FAT's BPB)</summary>
        public readonly ushort heads;
        /// <summary>0x14 Hidden sectors (part of FAT's BPB)</summary>
        public readonly uint hidden;
        /// <summary>0x18 Sectors on disk (part of FAT's BPB)</summary>
        public readonly uint sectorsBig;
        /// <summary>0x1C Description</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
        public readonly string description;
        /// <summary>0x58 Blind mode. 0 = DOS, 1 = blind, 2 = HFS</summary>
        public readonly byte blind;
        /// <summary>0x59 Density. 0 = Double, 1 = High, 2 = Quad/Extra</summary>
        public readonly byte density;
        /// <summary>0x5A Cylinders in image</summary>
        public readonly byte imageCylinders;
        /// <summary>0x5B Cylinders on disk</summary>
        public readonly byte totalCylinders;
        /// <summary>0x5C CRC32 of data</summary>
        public readonly uint crc;
        /// <summary>0x60 DOS volume label</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public readonly string volumeLabel;
        /// <summary>0x6B Modification time</summary>
        public readonly ushort time;
        /// <summary>0x6D Modification date</summary>
        public readonly ushort date;
        /// <summary>0x6F Comment length</summary>
        public readonly ushort commentLength;
        /// <summary>0x71 Sector base (first sector - 1)</summary>
        public readonly byte secbs;
        /// <summary>0x72 Unknown</summary>
        public readonly ushort unknown;
        /// <summary>0x74 Interleave</summary>
        public readonly byte interleave;
        /// <summary>0x75 Skew</summary>
        public readonly byte skew;
        /// <summary>0x76 Source drive type. 1 = 5.25" DD, 2 = 5.25" HD, 3 = 3.5" DD, 4 = 3.5" HD, 6 = 3.5" ED</summary>
        public readonly byte drive;
        /// <summary>0x77 Filling bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public readonly byte[] fill;
        /// <summary>0x84 Header checksum</summary>
        public readonly byte headerChecksum;
    }
}