// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleHFSPlus.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Hierarchical File System Plus and shows information.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    // Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
    /// <summary>
    /// Implements detection of Apple Hierarchical File System Plus (HFS+)
    /// </summary>
    public sealed class AppleHFSPlus : IFilesystem
    {
        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "Apple HFS+ filesystem";
        /// <inheritdoc />
        public Guid           Id        => new Guid("36405F8D-0D26-6EBE-436F-62F0586B4F08");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End)
                return false;

            ulong hfspOffset;

            uint sectorsToRead = 0x800 / imagePlugin.Info.SectorSize;

            if(0x800 % imagePlugin.Info.SectorSize > 0)
                sectorsToRead++;

            byte[] vhSector = imagePlugin.ReadSectors(partition.Start, sectorsToRead);

            if(vhSector.Length < 0x800)
                return false;

            ushort drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

            if(drSigWord == AppleCommon.HFS_MAGIC) // "BD"
            {
                drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x47C); // Read embedded HFS+ signature

                if(drSigWord == AppleCommon.HFSP_MAGIC) // "H+"
                {
                    ushort xdrStABNt = BigEndianBitConverter.ToUInt16(vhSector, 0x47E);

                    uint drAlBlkSiz = BigEndianBitConverter.ToUInt32(vhSector, 0x414);

                    ushort drAlBlSt = BigEndianBitConverter.ToUInt16(vhSector, 0x41C);

                    hfspOffset = (ulong)(((drAlBlSt * 512) + (xdrStABNt * drAlBlkSiz)) / imagePlugin.Info.SectorSize);
                }
                else
                    hfspOffset = 0;
            }
            else
                hfspOffset = 0;

            vhSector = imagePlugin.ReadSectors(partition.Start + hfspOffset, sectorsToRead); // Read volume header

            drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

            return drSigWord == AppleCommon.HFSP_MAGIC || drSigWord == AppleCommon.HFSX_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = Encoding.BigEndianUnicode;
            information = "";

            var vh = new VolumeHeader();

            ulong hfspOffset;
            bool  wrapped;

            uint sectorsToRead = 0x800 / imagePlugin.Info.SectorSize;

            if(0x800 % imagePlugin.Info.SectorSize > 0)
                sectorsToRead++;

            byte[] vhSector = imagePlugin.ReadSectors(partition.Start, sectorsToRead);

            ushort drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

            if(drSigWord == AppleCommon.HFS_MAGIC) // "BD"
            {
                drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x47C); // Read embedded HFS+ signature

                if(drSigWord == AppleCommon.HFSP_MAGIC) // "H+"
                {
                    ushort xdrStABNt = BigEndianBitConverter.ToUInt16(vhSector, 0x47E);

                    uint drAlBlkSiz = BigEndianBitConverter.ToUInt32(vhSector, 0x414);

                    ushort drAlBlSt = BigEndianBitConverter.ToUInt16(vhSector, 0x41C);

                    hfspOffset = (ulong)(((drAlBlSt * 512) + (xdrStABNt * drAlBlkSiz)) / imagePlugin.Info.SectorSize);
                    wrapped    = true;
                }
                else
                {
                    hfspOffset = 0;
                    wrapped    = false;
                }
            }
            else
            {
                hfspOffset = 0;
                wrapped    = false;
            }

            vhSector = imagePlugin.ReadSectors(partition.Start + hfspOffset, sectorsToRead); // Read volume header

            vh.signature = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

            if(vh.signature != AppleCommon.HFSP_MAGIC &&
               vh.signature != AppleCommon.HFSX_MAGIC)
                return;

            var sb = new StringBuilder();

            if(vh.signature == 0x482B)
                sb.AppendLine("HFS+ filesystem.");

            if(vh.signature == 0x4858)
                sb.AppendLine("HFSX filesystem.");

            if(wrapped)
                sb.AppendLine("Volume is wrapped inside an HFS volume.");

            byte[] tmp = new byte[0x400];
            Array.Copy(vhSector, 0x400, tmp, 0, 0x400);
            vhSector = tmp;

            vh = Marshal.ByteArrayToStructureBigEndian<VolumeHeader>(vhSector);

            if(vh.version == 4 ||
               vh.version == 5)
            {
                sb.AppendFormat("Filesystem version is {0}.", vh.version).AppendLine();

                if((vh.attributes & 0x80) == 0x80)
                    sb.AppendLine("Volume is locked on hardware.");

                if((vh.attributes & 0x100) == 0x100)
                    sb.AppendLine("Volume is unmounted.");

                if((vh.attributes & 0x200) == 0x200)
                    sb.AppendLine("There are bad blocks in the extents file.");

                if((vh.attributes & 0x400) == 0x400)
                    sb.AppendLine("Volume does not require cache.");

                if((vh.attributes & 0x800) == 0x800)
                    sb.AppendLine("Volume state is inconsistent.");

                if((vh.attributes & 0x1000) == 0x1000)
                    sb.AppendLine("CNIDs are reused.");

                if((vh.attributes & 0x2000) == 0x2000)
                    sb.AppendLine("Volume is journaled.");

                if((vh.attributes & 0x8000) == 0x8000)
                    sb.AppendLine("Volume is locked on software.");

                sb.AppendFormat("Implementation that last mounted the volume: \"{0}\".",
                                Encoding.ASCII.GetString(vh.lastMountedVersion)).AppendLine();

                if((vh.attributes & 0x2000) == 0x2000)
                    sb.AppendFormat("Journal starts at allocation block {0}.", vh.journalInfoBlock).AppendLine();

                sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(vh.createDate)).AppendLine();
                sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(vh.modifyDate)).AppendLine();

                if(vh.backupDate > 0)
                    sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(vh.backupDate)).AppendLine();
                else
                    sb.AppendLine("Volume has never been backed up");

                if(vh.backupDate > 0)
                    sb.AppendFormat("Last check date: {0}", DateHandlers.MacToDateTime(vh.checkedDate)).AppendLine();
                else
                    sb.AppendLine("Volume has never been checked up");

                sb.AppendFormat("{0} files on volume.", vh.fileCount).AppendLine();
                sb.AppendFormat("{0} folders on volume.", vh.folderCount).AppendLine();
                sb.AppendFormat("{0} bytes per allocation block.", vh.blockSize).AppendLine();
                sb.AppendFormat("{0} allocation blocks.", vh.totalBlocks).AppendLine();
                sb.AppendFormat("{0} free blocks.", vh.freeBlocks).AppendLine();
                sb.AppendFormat("Next allocation block: {0}.", vh.nextAllocation).AppendLine();
                sb.AppendFormat("Resource fork clump size: {0} bytes.", vh.rsrcClumpSize).AppendLine();
                sb.AppendFormat("Data fork clump size: {0} bytes.", vh.dataClumpSize).AppendLine();
                sb.AppendFormat("Next unused CNID: {0}.", vh.nextCatalogID).AppendLine();
                sb.AppendFormat("Volume has been mounted writable {0} times.", vh.writeCount).AppendLine();
                sb.AppendFormat("Allocation File is {0} bytes.", vh.allocationFile_logicalSize).AppendLine();
                sb.AppendFormat("Extents File is {0} bytes.", vh.extentsFile_logicalSize).AppendLine();
                sb.AppendFormat("Catalog File is {0} bytes.", vh.catalogFile_logicalSize).AppendLine();
                sb.AppendFormat("Attributes File is {0} bytes.", vh.attributesFile_logicalSize).AppendLine();
                sb.AppendFormat("Startup File is {0} bytes.", vh.startupFile_logicalSize).AppendLine();
                sb.AppendLine("Finder info:");
                sb.AppendFormat("CNID of bootable system's directory: {0}", vh.drFndrInfo0).AppendLine();
                sb.AppendFormat("CNID of first-run application's directory: {0}", vh.drFndrInfo1).AppendLine();
                sb.AppendFormat("CNID of previously opened directory: {0}", vh.drFndrInfo2).AppendLine();
                sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", vh.drFndrInfo3).AppendLine();
                sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", vh.drFndrInfo5).AppendLine();

                if(vh.drFndrInfo6 != 0 &&
                   vh.drFndrInfo7 != 0)
                    sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", vh.drFndrInfo6, vh.drFndrInfo7).AppendLine();

                XmlFsType = new FileSystemType();

                if(vh.backupDate > 0)
                {
                    XmlFsType.BackupDate          = DateHandlers.MacToDateTime(vh.backupDate);
                    XmlFsType.BackupDateSpecified = true;
                }

                XmlFsType.Bootable    |= vh.drFndrInfo0 != 0 || vh.drFndrInfo3 != 0 || vh.drFndrInfo5 != 0;
                XmlFsType.Clusters    =  vh.totalBlocks;
                XmlFsType.ClusterSize =  vh.blockSize;

                if(vh.createDate > 0)
                {
                    XmlFsType.CreationDate          = DateHandlers.MacToDateTime(vh.createDate);
                    XmlFsType.CreationDateSpecified = true;
                }

                XmlFsType.Dirty                 = (vh.attributes & 0x100) != 0x100;
                XmlFsType.Files                 = vh.fileCount;
                XmlFsType.FilesSpecified        = true;
                XmlFsType.FreeClusters          = vh.freeBlocks;
                XmlFsType.FreeClustersSpecified = true;

                if(vh.modifyDate > 0)
                {
                    XmlFsType.ModificationDate          = DateHandlers.MacToDateTime(vh.modifyDate);
                    XmlFsType.ModificationDateSpecified = true;
                }

                if(vh.signature == 0x482B)
                    XmlFsType.Type = "HFS+";

                if(vh.signature == 0x4858)
                    XmlFsType.Type = "HFSX";

                if(vh.drFndrInfo6 != 0 &&
                   vh.drFndrInfo7 != 0)
                    XmlFsType.VolumeSerial = $"{vh.drFndrInfo6:X8}{vh.drFndrInfo7:X8}";

                XmlFsType.SystemIdentifier = Encoding.ASCII.GetString(vh.lastMountedVersion);
            }
            else
            {
                sb.AppendFormat("Filesystem version is {0}.", vh.version).AppendLine();
                sb.AppendLine("This version is not supported yet.");
            }

            information = sb.ToString();
        }

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
}