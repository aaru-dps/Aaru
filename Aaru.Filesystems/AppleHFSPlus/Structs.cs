// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
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

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <inheritdoc />
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <summary>HFS+ Volume Header, should be at offset 0x0400 bytes in volume with a size of 532 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VolumeHeader
    {
        /// <summary>0x000, "H+" for HFS+, "HX" for HFSX</summary>
        public ushort signature;
        /// <summary>0x002, 4 for HFS+, 5 for HFSX</summary>
        public readonly ushort version;
        /// <summary>0x004, Volume attributes</summary>
        public readonly uint attributes;
        /// <summary>
        ///     0x008, Implementation that last mounted the volume. Reserved by Apple: "8.10" Mac OS 8.1 to 9.2.2 "10.0" Mac
        ///     OS X "HFSJ" Journaled implementation "fsck" /sbin/fsck
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] lastMountedVersion;
        /// <summary>0x00C, Allocation block number containing the journal</summary>
        public readonly uint journalInfoBlock;
        /// <summary>0x010, Date of volume creation</summary>
        public readonly uint createDate;
        /// <summary>0x014, Date of last volume modification</summary>
        public readonly uint modifyDate;
        /// <summary>0x018, Date of last backup</summary>
        public readonly uint backupDate;
        /// <summary>0x01C, Date of last consistency check</summary>
        public readonly uint checkedDate;
        /// <summary>0x020, File on the volume</summary>
        public readonly uint fileCount;
        /// <summary>0x024, Folders on the volume</summary>
        public readonly uint folderCount;
        /// <summary>0x028, Bytes per allocation block</summary>
        public readonly uint blockSize;
        /// <summary>0x02C, Allocation blocks on the volume</summary>
        public readonly uint totalBlocks;
        /// <summary>0x030, Free allocation blocks</summary>
        public readonly uint freeBlocks;
        /// <summary>0x034, Hint for next allocation block</summary>
        public readonly uint nextAllocation;
        /// <summary>0x038, Resource fork clump size</summary>
        public readonly uint rsrcClumpSize;
        /// <summary>0x03C, Data fork clump size</summary>
        public readonly uint dataClumpSize;
        /// <summary>0x040, Next unused CNID</summary>
        public readonly uint nextCatalogID;
        /// <summary>0x044, Times that the volume has been mounted writable</summary>
        public readonly uint writeCount;
        /// <summary>0x048, Used text encoding hints</summary>
        public readonly ulong encodingsBitmap;
        /// <summary>0x050, finderInfo[0], CNID for bootable system's directory</summary>
        public readonly uint drFndrInfo0;
        /// <summary>0x054, finderInfo[1], CNID of the directory containing the boot application</summary>
        public readonly uint drFndrInfo1;
        /// <summary>0x058, finderInfo[2], CNID of the directory that should be opened on boot</summary>
        public readonly uint drFndrInfo2;
        /// <summary>0x05C, finderInfo[3], CNID for Mac OS 8 or 9 directory</summary>
        public readonly uint drFndrInfo3;
        /// <summary>0x060, finderInfo[4], Reserved</summary>
        public readonly uint drFndrInfo4;
        /// <summary>0x064, finderInfo[5], CNID for Mac OS X directory</summary>
        public readonly uint drFndrInfo5;
        /// <summary>0x068, finderInfo[6], first part of Mac OS X volume ID</summary>
        public readonly uint drFndrInfo6;
        /// <summary>0x06C, finderInfo[7], second part of Mac OS X volume ID</summary>
        public readonly uint drFndrInfo7;

        // HFSPlusForkData     allocationFile;
        /// <summary>0x070</summary>
        public readonly ulong allocationFile_logicalSize;
        /// <summary>0x078</summary>
        public readonly uint allocationFile_clumpSize;
        /// <summary>0x07C</summary>
        public readonly uint allocationFile_totalBlocks;
        /// <summary>0x080</summary>
        public readonly uint allocationFile_extents_startBlock0;
        /// <summary>0x084</summary>
        public readonly uint allocationFile_extents_blockCount0;
        /// <summary>0x088</summary>
        public readonly uint allocationFile_extents_startBlock1;
        /// <summary>0x08C</summary>
        public readonly uint allocationFile_extents_blockCount1;
        /// <summary>0x090</summary>
        public readonly uint allocationFile_extents_startBlock2;
        /// <summary>0x094</summary>
        public readonly uint allocationFile_extents_blockCount2;
        /// <summary>0x098</summary>
        public readonly uint allocationFile_extents_startBlock3;
        /// <summary>0x09C</summary>
        public readonly uint allocationFile_extents_blockCount3;
        /// <summary>0x0A0</summary>
        public readonly uint allocationFile_extents_startBlock4;
        /// <summary>0x0A4</summary>
        public readonly uint allocationFile_extents_blockCount4;
        /// <summary>0x0A8</summary>
        public readonly uint allocationFile_extents_startBlock5;
        /// <summary>0x0AC</summary>
        public readonly uint allocationFile_extents_blockCount5;
        /// <summary>0x0B0</summary>
        public readonly uint allocationFile_extents_startBlock6;
        /// <summary>0x0B4</summary>
        public readonly uint allocationFile_extents_blockCount6;
        /// <summary>0x0B8</summary>
        public readonly uint allocationFile_extents_startBlock7;
        /// <summary>0x0BC</summary>
        public readonly uint allocationFile_extents_blockCount7;

        // HFSPlusForkData     extentsFile;
        /// <summary>0x0C0</summary>
        public readonly ulong extentsFile_logicalSize;
        /// <summary>0x0C8</summary>
        public readonly uint extentsFile_clumpSize;
        /// <summary>0x0CC</summary>
        public readonly uint extentsFile_totalBlocks;
        /// <summary>0x0D0</summary>
        public readonly uint extentsFile_extents_startBlock0;
        /// <summary>0x0D4</summary>
        public readonly uint extentsFile_extents_blockCount0;
        /// <summary>0x0D8</summary>
        public readonly uint extentsFile_extents_startBlock1;
        /// <summary>0x0DC</summary>
        public readonly uint extentsFile_extents_blockCount1;
        /// <summary>0x0E0</summary>
        public readonly uint extentsFile_extents_startBlock2;
        /// <summary>0x0E4</summary>
        public readonly uint extentsFile_extents_blockCount2;
        /// <summary>0x0E8</summary>
        public readonly uint extentsFile_extents_startBlock3;
        /// <summary>0x0EC</summary>
        public readonly uint extentsFile_extents_blockCount3;
        /// <summary>0x0F0</summary>
        public readonly uint extentsFile_extents_startBlock4;
        /// <summary>0x0F4</summary>
        public readonly uint extentsFile_extents_blockCount4;
        /// <summary>0x0F8</summary>
        public readonly uint extentsFile_extents_startBlock5;
        /// <summary>0x0FC</summary>
        public readonly uint extentsFile_extents_blockCount5;
        /// <summary>0x100</summary>
        public readonly uint extentsFile_extents_startBlock6;
        /// <summary>0x104</summary>
        public readonly uint extentsFile_extents_blockCount6;
        /// <summary>0x108</summary>
        public readonly uint extentsFile_extents_startBlock7;
        /// <summary>0x10C</summary>
        public readonly uint extentsFile_extents_blockCount7;

        // HFSPlusForkData     catalogFile;
        /// <summary>0x110</summary>
        public readonly ulong catalogFile_logicalSize;
        /// <summary>0x118</summary>
        public readonly uint catalogFile_clumpSize;
        /// <summary>0x11C</summary>
        public readonly uint catalogFile_totalBlocks;
        /// <summary>0x120</summary>
        public readonly uint catalogFile_extents_startBlock0;
        /// <summary>0x124</summary>
        public readonly uint catalogFile_extents_blockCount0;
        /// <summary>0x128</summary>
        public readonly uint catalogFile_extents_startBlock1;
        /// <summary>0x12C</summary>
        public readonly uint catalogFile_extents_blockCount1;
        /// <summary>0x130</summary>
        public readonly uint catalogFile_extents_startBlock2;
        /// <summary>0x134</summary>
        public readonly uint catalogFile_extents_blockCount2;
        /// <summary>0x138</summary>
        public readonly uint catalogFile_extents_startBlock3;
        /// <summary>0x13C</summary>
        public readonly uint catalogFile_extents_blockCount3;
        /// <summary>0x140</summary>
        public readonly uint catalogFile_extents_startBlock4;
        /// <summary>0x144</summary>
        public readonly uint catalogFile_extents_blockCount4;
        /// <summary>0x148</summary>
        public readonly uint catalogFile_extents_startBlock5;
        /// <summary>0x14C</summary>
        public readonly uint catalogFile_extents_blockCount5;
        /// <summary>0x150</summary>
        public readonly uint catalogFile_extents_startBlock6;
        /// <summary>0x154</summary>
        public readonly uint catalogFile_extents_blockCount6;
        /// <summary>0x158</summary>
        public readonly uint catalogFile_extents_startBlock7;
        /// <summary>0x15C</summary>
        public readonly uint catalogFile_extents_blockCount7;

        // HFSPlusForkData     attributesFile;
        /// <summary>0x160</summary>
        public readonly ulong attributesFile_logicalSize;
        /// <summary>0x168</summary>
        public readonly uint attributesFile_clumpSize;
        /// <summary>0x16C</summary>
        public readonly uint attributesFile_totalBlocks;
        /// <summary>0x170</summary>
        public readonly uint attributesFile_extents_startBlock0;
        /// <summary>0x174</summary>
        public readonly uint attributesFile_extents_blockCount0;
        /// <summary>0x178</summary>
        public readonly uint attributesFile_extents_startBlock1;
        /// <summary>0x17C</summary>
        public readonly uint attributesFile_extents_blockCount1;
        /// <summary>0x180</summary>
        public readonly uint attributesFile_extents_startBlock2;
        /// <summary>0x184</summary>
        public readonly uint attributesFile_extents_blockCount2;
        /// <summary>0x188</summary>
        public readonly uint attributesFile_extents_startBlock3;
        /// <summary>0x18C</summary>
        public readonly uint attributesFile_extents_blockCount3;
        /// <summary>0x190</summary>
        public readonly uint attributesFile_extents_startBlock4;
        /// <summary>0x194</summary>
        public readonly uint attributesFile_extents_blockCount4;
        /// <summary>0x198</summary>
        public readonly uint attributesFile_extents_startBlock5;
        /// <summary>0x19C</summary>
        public readonly uint attributesFile_extents_blockCount5;
        /// <summary>0x1A0</summary>
        public readonly uint attributesFile_extents_startBlock6;
        /// <summary>0x1A4</summary>
        public readonly uint attributesFile_extents_blockCount6;
        /// <summary>0x1A8</summary>
        public readonly uint attributesFile_extents_startBlock7;
        /// <summary>0x1AC</summary>
        public readonly uint attributesFile_extents_blockCount7;

        // HFSPlusForkData     startupFile;
        /// <summary>0x1B0</summary>
        public readonly ulong startupFile_logicalSize;
        /// <summary>0x1B8</summary>
        public readonly uint startupFile_clumpSize;
        /// <summary>0x1BC</summary>
        public readonly uint startupFile_totalBlocks;
        /// <summary>0x1C0</summary>
        public readonly uint startupFile_extents_startBlock0;
        /// <summary>0x1C4</summary>
        public readonly uint startupFile_extents_blockCount0;
        /// <summary>0x1C8</summary>
        public readonly uint startupFile_extents_startBlock1;
        /// <summary>0x1D0</summary>
        public readonly uint startupFile_extents_blockCount1;
        /// <summary>0x1D4</summary>
        public readonly uint startupFile_extents_startBlock2;
        /// <summary>0x1D8</summary>
        public readonly uint startupFile_extents_blockCount2;
        /// <summary>0x1DC</summary>
        public readonly uint startupFile_extents_startBlock3;
        /// <summary>0x1E0</summary>
        public readonly uint startupFile_extents_blockCount3;
        /// <summary>0x1E4</summary>
        public readonly uint startupFile_extents_startBlock4;
        /// <summary>0x1E8</summary>
        public readonly uint startupFile_extents_blockCount4;
        /// <summary>0x1EC</summary>
        public readonly uint startupFile_extents_startBlock5;
        /// <summary>0x1F0</summary>
        public readonly uint startupFile_extents_blockCount5;
        /// <summary>0x1F4</summary>
        public readonly uint startupFile_extents_startBlock6;
        /// <summary>0x1F8</summary>
        public readonly uint startupFile_extents_blockCount6;
        /// <summary>0x1FC</summary>
        public readonly uint startupFile_extents_startBlock7;
        /// <summary>0x200</summary>
        public readonly uint startupFile_extents_blockCount7;
    }
}