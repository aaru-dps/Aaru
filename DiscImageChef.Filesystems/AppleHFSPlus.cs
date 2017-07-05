// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;

namespace DiscImageChef.Filesystems
{
    // Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
    public class AppleHFSPlus : Filesystem
    {
        /// <summary>
        /// "BD", HFS magic
        /// </summary>
        const ushort HFS_MAGIC = 0x4244;
        /// <summary>
        /// "H+", HFS+ magic
        /// </summary>
        const ushort HFSP_MAGIC = 0x482B;
        /// <summary>
        /// "HX", HFSX magic
        /// </summary>
        const ushort HFSX_MAGIC = 0x4858;

        public AppleHFSPlus()
        {
            Name = "Apple HFS+ filesystem";
            PluginUUID = new Guid("36405F8D-0D26-6EBE-436F-62F0586B4F08");
            CurrentEncoding = Encoding.UTF8;
        }

        public AppleHFSPlus(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, Encoding encoding)
        {
            Name = "Apple HFS+ filesystem";
            PluginUUID = new Guid("36405F8D-0D26-6EBE-436F-62F0586B4F08");
            if(encoding == null)
                CurrentEncoding = Encoding.UTF8;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if((2 + partitionStart) >= partitionEnd)
                return false;

            ushort drSigWord;
            ushort xdrStABNt;
            ushort drAlBlSt;
            uint drAlBlkSiz;

            byte[] vh_sector;
            ulong hfsp_offset;

            vh_sector = imagePlugin.ReadSector(2 + partitionStart); // Read volume header, of HFS Wrapper MDB

            drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0); // Check for HFS Wrapper MDB

            if(drSigWord == HFS_MAGIC) // "BD"
            {
                drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0x07C); // Read embedded HFS+ signature

                if(drSigWord == HFSP_MAGIC) // "H+"
                {
                    xdrStABNt = BigEndianBitConverter.ToUInt16(vh_sector, 0x07E); // Starting block number of embedded HFS+ volume

                    drAlBlkSiz = BigEndianBitConverter.ToUInt32(vh_sector, 0x014); // Block size

                    drAlBlSt = BigEndianBitConverter.ToUInt16(vh_sector, 0x01C); // Start of allocated blocks (in 512-byte/block)

                    hfsp_offset = (drAlBlSt + xdrStABNt * (drAlBlkSiz / 512)) * (imagePlugin.GetSectorSize() / 512);
                }
                else
                {
                    hfsp_offset = 0;
                }
            }
            else
            {
                hfsp_offset = 0;
            }

            vh_sector = imagePlugin.ReadSector(2 + partitionStart + hfsp_offset); // Read volume header

            drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0);
            if(drSigWord == HFSP_MAGIC || drSigWord == HFSX_MAGIC)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";

            ushort drSigWord;
            ushort xdrStABNt;
            ushort drAlBlSt;
            uint drAlBlkSiz;
            HFSPlusVolumeHeader HPVH = new HFSPlusVolumeHeader();

            ulong hfsp_offset;
            bool wrapped;
            byte[] vh_sector;

            vh_sector = imagePlugin.ReadSector(2 + partitionStart); // Read volume header, of HFS Wrapper MDB

            drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0); // Check for HFS Wrapper MDB

            if(drSigWord == HFS_MAGIC) // "BD"
            {
                drSigWord = BigEndianBitConverter.ToUInt16(vh_sector, 0x07C); // Read embedded HFS+ signature

                if(drSigWord == HFSP_MAGIC) // "H+"
                {
                    xdrStABNt = BigEndianBitConverter.ToUInt16(vh_sector, 0x07E); // Starting block number of embedded HFS+ volume

                    drAlBlkSiz = BigEndianBitConverter.ToUInt32(vh_sector, 0x014); // Block size

                    drAlBlSt = BigEndianBitConverter.ToUInt16(vh_sector, 0x01C); // Start of allocated blocks (in 512-byte/block)

                    hfsp_offset = (drAlBlSt + xdrStABNt * (drAlBlkSiz / 512)) * (imagePlugin.GetSectorSize() / 512);
                    wrapped = true;
                }
                else
                {
                    hfsp_offset = 0;
                    wrapped = false;
                }
            }
            else
            {
                hfsp_offset = 0;
                wrapped = false;
            }

            vh_sector = imagePlugin.ReadSector(2 + partitionStart + hfsp_offset); // Read volume header

            HPVH.signature = BigEndianBitConverter.ToUInt16(vh_sector, 0x000);
            if(HPVH.signature == HFSP_MAGIC || HPVH.signature == HFSX_MAGIC)
            {
                StringBuilder sb = new StringBuilder();

                if(HPVH.signature == 0x482B)
                    sb.AppendLine("HFS+ filesystem.");
                if(HPVH.signature == 0x4858)
                    sb.AppendLine("HFSX filesystem.");
                if(wrapped)
                    sb.AppendLine("Volume is wrapped inside an HFS volume.");

                HPVH.version = BigEndianBitConverter.ToUInt16(vh_sector, 0x002);

                if(HPVH.version == 4 || HPVH.version == 5)
                {
                    HPVH.attributes = BigEndianBitConverter.ToUInt32(vh_sector, 0x004);
                    byte[] lastMountedVersion_b = new byte[4];
                    Array.Copy(vh_sector, 0x008, lastMountedVersion_b, 0, 4);
                    HPVH.lastMountedVersion = Encoding.ASCII.GetString(lastMountedVersion_b);
                    HPVH.journalInfoBlock = BigEndianBitConverter.ToUInt32(vh_sector, 0x00C);

                    HPVH.createDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x010);
                    HPVH.modifyDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x018);
                    HPVH.backupDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x020);
                    HPVH.checkedDate = BigEndianBitConverter.ToUInt32(vh_sector, 0x028);

                    HPVH.fileCount = BigEndianBitConverter.ToUInt32(vh_sector, 0x030);
                    HPVH.folderCount = BigEndianBitConverter.ToUInt32(vh_sector, 0x034);

                    HPVH.blockSize = BigEndianBitConverter.ToUInt32(vh_sector, 0x038);
                    HPVH.totalBlocks = BigEndianBitConverter.ToUInt32(vh_sector, 0x03C);
                    HPVH.freeBlocks = BigEndianBitConverter.ToUInt32(vh_sector, 0x040);

                    HPVH.nextAllocation = BigEndianBitConverter.ToUInt32(vh_sector, 0x044);
                    HPVH.rsrcClumpSize = BigEndianBitConverter.ToUInt32(vh_sector, 0x048);
                    HPVH.dataClumpSize = BigEndianBitConverter.ToUInt32(vh_sector, 0x04C);
                    HPVH.nextCatalogID = BigEndianBitConverter.ToUInt32(vh_sector, 0x050);

                    HPVH.writeCount = BigEndianBitConverter.ToUInt32(vh_sector, 0x054);

                    HPVH.drFndrInfo0 = BigEndianBitConverter.ToUInt32(vh_sector, 0x060);
                    HPVH.drFndrInfo1 = BigEndianBitConverter.ToUInt32(vh_sector, 0x064);
                    HPVH.drFndrInfo2 = BigEndianBitConverter.ToUInt32(vh_sector, 0x068);
                    HPVH.drFndrInfo3 = BigEndianBitConverter.ToUInt32(vh_sector, 0x06C);
                    HPVH.drFndrInfo5 = BigEndianBitConverter.ToUInt32(vh_sector, 0x074);
                    HPVH.drFndrInfo6 = BigEndianBitConverter.ToUInt32(vh_sector, 0x078);
                    HPVH.drFndrInfo7 = BigEndianBitConverter.ToUInt32(vh_sector, 0x07C);

                    HPVH.allocationFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x080);
                    HPVH.extentsFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x0D0);
                    HPVH.catalogFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x120);
                    HPVH.attributesFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x170);
                    HPVH.startupFile_logicalSize = BigEndianBitConverter.ToUInt64(vh_sector, 0x1C0);

                    sb.AppendFormat("Filesystem version is {0}.", HPVH.version).AppendLine();

                    if((HPVH.attributes & 0x80) == 0x80)
                        sb.AppendLine("Volume is locked on hardware.");
                    if((HPVH.attributes & 0x100) == 0x100)
                        sb.AppendLine("Volume is unmounted.");
                    if((HPVH.attributes & 0x200) == 0x200)
                        sb.AppendLine("There are bad blocks in the extents file.");
                    if((HPVH.attributes & 0x400) == 0x400)
                        sb.AppendLine("Volume does not require cache.");
                    if((HPVH.attributes & 0x800) == 0x800)
                        sb.AppendLine("Volume state is inconsistent.");
                    if((HPVH.attributes & 0x1000) == 0x1000)
                        sb.AppendLine("CNIDs are reused.");
                    if((HPVH.attributes & 0x2000) == 0x2000)
                        sb.AppendLine("Volume is journaled.");
                    if((HPVH.attributes & 0x8000) == 0x8000)
                        sb.AppendLine("Volume is locked on software.");

                    sb.AppendFormat("Implementation that last mounted the volume: \"{0}\".", HPVH.lastMountedVersion).AppendLine();
                    if((HPVH.attributes & 0x2000) == 0x2000)
                        sb.AppendFormat("Journal starts at allocation block {0}.", HPVH.journalInfoBlock).AppendLine();
                    sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(HPVH.createDate)).AppendLine();
                    sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(HPVH.modifyDate)).AppendLine();
                    sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(HPVH.backupDate)).AppendLine();
                    sb.AppendFormat("Last check date: {0}", DateHandlers.MacToDateTime(HPVH.checkedDate)).AppendLine();
                    sb.AppendFormat("{0} files on volume.", HPVH.fileCount).AppendLine();
                    sb.AppendFormat("{0} folders on volume.", HPVH.folderCount).AppendLine();
                    sb.AppendFormat("{0} bytes per allocation block.", HPVH.blockSize).AppendLine();
                    sb.AppendFormat("{0} allocation blocks.", HPVH.totalBlocks).AppendLine();
                    sb.AppendFormat("{0} free blocks.", HPVH.freeBlocks).AppendLine();
                    sb.AppendFormat("Next allocation block: {0}.", HPVH.nextAllocation).AppendLine();
                    sb.AppendFormat("Resource fork clump size: {0} bytes.", HPVH.rsrcClumpSize).AppendLine();
                    sb.AppendFormat("Data fork clump size: {0} bytes.", HPVH.dataClumpSize).AppendLine();
                    sb.AppendFormat("Next unused CNID: {0}.", HPVH.nextCatalogID).AppendLine();
                    sb.AppendFormat("Volume has been mounted writable {0} times.", HPVH.writeCount).AppendLine();
                    sb.AppendFormat("Allocation File is {0} bytes.", HPVH.allocationFile_logicalSize).AppendLine();
                    sb.AppendFormat("Extents File is {0} bytes.", HPVH.extentsFile_logicalSize).AppendLine();
                    sb.AppendFormat("Catalog File is {0} bytes.", HPVH.catalogFile_logicalSize).AppendLine();
                    sb.AppendFormat("Attributes File is {0} bytes.", HPVH.attributesFile_logicalSize).AppendLine();
                    sb.AppendFormat("Startup File is {0} bytes.", HPVH.startupFile_logicalSize).AppendLine();
                    sb.AppendLine("Finder info:");
                    sb.AppendFormat("CNID of bootable system's directory: {0}", HPVH.drFndrInfo0).AppendLine();
                    sb.AppendFormat("CNID of first-run application's directory: {0}", HPVH.drFndrInfo1).AppendLine();
                    sb.AppendFormat("CNID of previously opened directory: {0}", HPVH.drFndrInfo2).AppendLine();
                    sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", HPVH.drFndrInfo3).AppendLine();
                    sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", HPVH.drFndrInfo5).AppendLine();
                    sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", HPVH.drFndrInfo6, HPVH.drFndrInfo7).AppendLine();

                    xmlFSType = new Schemas.FileSystemType();
                    if(HPVH.backupDate > 0)
                    {
                        xmlFSType.BackupDate = DateHandlers.MacToDateTime(HPVH.backupDate);
                        xmlFSType.BackupDateSpecified = true;
                    }
                    xmlFSType.Bootable |= (HPVH.drFndrInfo0 != 0 || HPVH.drFndrInfo3 != 0 || HPVH.drFndrInfo5 != 0);
                    xmlFSType.Clusters = HPVH.totalBlocks;
                    xmlFSType.ClusterSize = (int)HPVH.blockSize;
                    if(HPVH.createDate > 0)
                    {
                        xmlFSType.CreationDate = DateHandlers.MacToDateTime(HPVH.createDate);
                        xmlFSType.CreationDateSpecified = true;
                    }
                    xmlFSType.Dirty = (HPVH.attributes & 0x100) != 0x100;
                    xmlFSType.Files = HPVH.fileCount;
                    xmlFSType.FilesSpecified = true;
                    xmlFSType.FreeClusters = HPVH.freeBlocks;
                    xmlFSType.FreeClustersSpecified = true;
                    if(HPVH.modifyDate > 0)
                    {
                        xmlFSType.ModificationDate = DateHandlers.MacToDateTime(HPVH.modifyDate);
                        xmlFSType.ModificationDateSpecified = true;
                    }
                    if(HPVH.signature == 0x482B)
                        xmlFSType.Type = "HFS+";
                    if(HPVH.signature == 0x4858)
                        xmlFSType.Type = "HFSX";
                    if(HPVH.drFndrInfo6 != 0 && HPVH.drFndrInfo7 != 0)
                        xmlFSType.VolumeSerial = string.Format("{0:X8}{1:x8}", HPVH.drFndrInfo6, HPVH.drFndrInfo7);
                    xmlFSType.SystemIdentifier = HPVH.lastMountedVersion;
                }
                else
                {
                    sb.AppendFormat("Filesystem version is {0}.", HPVH.version).AppendLine();
                    sb.AppendLine("This version is not supported yet.");
                }

                information = sb.ToString();
            }
            else
                return;
        }

        /// <summary>
        /// HFS+ Volume Header, should be at offset 0x0400 bytes in volume with a size of 532 bytes
        /// </summary>
        struct HFSPlusVolumeHeader
        {
            /// <summary>0x000, "H+" for HFS+, "HX" for HFSX</summary>
            public ushort signature;
            /// <summary>0x002, 4 for HFS+, 5 for HFSX</summary>
            public ushort version;
            /// <summary>0x004, Volume attributes</summary>
            public uint attributes;
            /// <summary>0x008, Implementation that last mounted the volume.
            /// Reserved by Apple:
            /// "8.10" Mac OS 8.1 to 9.2.2
            /// "10.0" Mac OS X
            /// "HFSJ" Journaled implementation
            /// "fsck" /sbin/fsck</summary>
            public string lastMountedVersion;
            /// <summary>0x00C, Allocation block number containing the journal</summary>
            public uint journalInfoBlock;
            /// <summary>0x010, Date of volume creation</summary>
            public ulong createDate;
            /// <summary>0x018, Date of last volume modification</summary>
            public ulong modifyDate;
            /// <summary>0x020, Date of last backup</summary>
            public ulong backupDate;
            /// <summary>0x028, Date of last consistency check</summary>
            public ulong checkedDate;
            /// <summary>0x030, File on the volume</summary>
            public uint fileCount;
            /// <summary>0x034, Folders on the volume</summary>
            public uint folderCount;
            /// <summary>0x038, Bytes per allocation block</summary>
            public uint blockSize;
            /// <summary>0x03C, Allocation blocks on the volume</summary>
            public uint totalBlocks;
            /// <summary>0x040, Free allocation blocks</summary>
            public uint freeBlocks;
            /// <summary>0x044, Hint for next allocation block</summary>
            public uint nextAllocation;
            /// <summary>0x048, Resource fork clump size</summary>
            public uint rsrcClumpSize;
            /// <summary>0x04C, Data fork clump size</summary>
            public uint dataClumpSize;
            /// <summary>0x050, Next unused CNID</summary>
            public uint nextCatalogID;
            /// <summary>0x054, Times that the volume has been mounted writable</summary>
            public uint writeCount;
            /// <summary>0x058, Used text encoding hints</summary>
            public ulong encodingsBitmap;
            /// <summary>0x060, finderInfo[0], CNID for bootable system's directory</summary>
            public uint drFndrInfo0;
            /// <summary>0x064, finderInfo[1], CNID of the directory containing the boot application</summary>
            public uint drFndrInfo1;
            /// <summary>0x068, finderInfo[2], CNID of the directory that should be opened on boot</summary>
            public uint drFndrInfo2;
            /// <summary>0x06C, finderInfo[3], CNID for Mac OS 8 or 9 directory</summary>
            public uint drFndrInfo3;
            /// <summary>0x070, finderInfo[4], Reserved</summary>
            public uint drFndrInfo4;
            /// <summary>0x074, finderInfo[5], CNID for Mac OS X directory</summary>
            public uint drFndrInfo5;
            /// <summary>0x078, finderInfo[6], first part of Mac OS X volume ID</summary>
            public uint drFndrInfo6;
            /// <summary>0x07C, finderInfo[7], second part of Mac OS X volume ID</summary>
            public uint drFndrInfo7;
            // HFSPlusForkData     allocationFile;
            /// <summary>0x080</summary>
            public ulong allocationFile_logicalSize;
            /// <summary>0x088</summary>
            public uint allocationFile_clumpSize;
            /// <summary>0x08C</summary>
            public uint allocationFile_totalBlocks;
            /// <summary>0x090</summary>
            public uint allocationFile_extents_startBlock0;
            /// <summary>0x094</summary>
            public uint allocationFile_extents_blockCount0;
            /// <summary>0x098</summary>
            public uint allocationFile_extents_startBlock1;
            /// <summary>0x09C</summary>
            public uint allocationFile_extents_blockCount1;
            /// <summary>0x0A0</summary>
            public uint allocationFile_extents_startBlock2;
            /// <summary>0x0A4</summary>
            public uint allocationFile_extents_blockCount2;
            /// <summary>0x0A8</summary>
            public uint allocationFile_extents_startBlock3;
            /// <summary>0x0AC</summary>
            public uint allocationFile_extents_blockCount3;
            /// <summary>0x0B0</summary>
            public uint allocationFile_extents_startBlock4;
            /// <summary>0x0B4</summary>
            public uint allocationFile_extents_blockCount4;
            /// <summary>0x0B8</summary>
            public uint allocationFile_extents_startBlock5;
            /// <summary>0x0BC</summary>
            public uint allocationFile_extents_blockCount5;
            /// <summary>0x0C0</summary>
            public uint allocationFile_extents_startBlock6;
            /// <summary>0x0C4</summary>
            public uint allocationFile_extents_blockCount6;
            /// <summary>0x0C8</summary>
            public uint allocationFile_extents_startBlock7;
            /// <summary>0x0CC</summary>
            public uint allocationFile_extents_blockCount7;
            // HFSPlusForkData     extentsFile;
            /// <summary>0x0D0</summary>
            public ulong extentsFile_logicalSize;
            /// <summary>0x0D8</summary>
            public uint extentsFile_clumpSize;
            /// <summary>0x0DC</summary>
            public uint extentsFile_totalBlocks;
            /// <summary>0x0E0</summary>
            public uint extentsFile_extents_startBlock0;
            /// <summary>0x0E4</summary>
            public uint extentsFile_extents_blockCount0;
            /// <summary>0x0E8</summary>
            public uint extentsFile_extents_startBlock1;
            /// <summary>0x0EC</summary>
            public uint extentsFile_extents_blockCount1;
            /// <summary>0x0F0</summary>
            public uint extentsFile_extents_startBlock2;
            /// <summary>0x0F4</summary>
            public uint extentsFile_extents_blockCount2;
            /// <summary>0x0F8</summary>
            public uint extentsFile_extents_startBlock3;
            /// <summary>0x0FC</summary>
            public uint extentsFile_extents_blockCount3;
            /// <summary>0x100</summary>
            public uint extentsFile_extents_startBlock4;
            /// <summary>0x104</summary>
            public uint extentsFile_extents_blockCount4;
            /// <summary>0x108</summary>
            public uint extentsFile_extents_startBlock5;
            /// <summary>0x10C</summary>
            public uint extentsFile_extents_blockCount5;
            /// <summary>0x110</summary>
            public uint extentsFile_extents_startBlock6;
            /// <summary>0x114</summary>
            public uint extentsFile_extents_blockCount6;
            /// <summary>0x118</summary>
            public uint extentsFile_extents_startBlock7;
            /// <summary>0x11C</summary>
            public uint extentsFile_extents_blockCount7;
            // HFSPlusForkData     catalogFile;
            /// <summary>0x120</summary>
            public ulong catalogFile_logicalSize;
            /// <summary>0x128</summary>
            public uint catalogFile_clumpSize;
            /// <summary>0x12C</summary>
            public uint catalogFile_totalBlocks;
            /// <summary>0x130</summary>
            public uint catalogFile_extents_startBlock0;
            /// <summary>0x134</summary>
            public uint catalogFile_extents_blockCount0;
            /// <summary>0x138</summary>
            public uint catalogFile_extents_startBlock1;
            /// <summary>0x13C</summary>
            public uint catalogFile_extents_blockCount1;
            /// <summary>0x140</summary>
            public uint catalogFile_extents_startBlock2;
            /// <summary>0x144</summary>
            public uint catalogFile_extents_blockCount2;
            /// <summary>0x148</summary>
            public uint catalogFile_extents_startBlock3;
            /// <summary>0x14C</summary>
            public uint catalogFile_extents_blockCount3;
            /// <summary>0x150</summary>
            public uint catalogFile_extents_startBlock4;
            /// <summary>0x154</summary>
            public uint catalogFile_extents_blockCount4;
            /// <summary>0x158</summary>
            public uint catalogFile_extents_startBlock5;
            /// <summary>0x15C</summary>
            public uint catalogFile_extents_blockCount5;
            /// <summary>0x160</summary>
            public uint catalogFile_extents_startBlock6;
            /// <summary>0x164</summary>
            public uint catalogFile_extents_blockCount6;
            /// <summary>0x168</summary>
            public uint catalogFile_extents_startBlock7;
            /// <summary>0x16C</summary>
            public uint catalogFile_extents_blockCount7;
            // HFSPlusForkData     attributesFile;
            /// <summary>0x170</summary>
            public ulong attributesFile_logicalSize;
            /// <summary>0x178</summary>
            public uint attributesFile_clumpSize;
            /// <summary>0x17C</summary>
            public uint attributesFile_totalBlocks;
            /// <summary>0x180</summary>
            public uint attributesFile_extents_startBlock0;
            /// <summary>0x184</summary>
            public uint attributesFile_extents_blockCount0;
            /// <summary>0x188</summary>
            public uint attributesFile_extents_startBlock1;
            /// <summary>0x18C</summary>
            public uint attributesFile_extents_blockCount1;
            /// <summary>0x190</summary>
            public uint attributesFile_extents_startBlock2;
            /// <summary>0x194</summary>
            public uint attributesFile_extents_blockCount2;
            /// <summary>0x198</summary>
            public uint attributesFile_extents_startBlock3;
            /// <summary>0x19C</summary>
            public uint attributesFile_extents_blockCount3;
            /// <summary>0x1A0</summary>
            public uint attributesFile_extents_startBlock4;
            /// <summary>0x1A4</summary>
            public uint attributesFile_extents_blockCount4;
            /// <summary>0x1A8</summary>
            public uint attributesFile_extents_startBlock5;
            /// <summary>0x1AC</summary>
            public uint attributesFile_extents_blockCount5;
            /// <summary>0x1B0</summary>
            public uint attributesFile_extents_startBlock6;
            /// <summary>0x1B4</summary>
            public uint attributesFile_extents_blockCount6;
            /// <summary>0x1B8</summary>
            public uint attributesFile_extents_startBlock7;
            /// <summary>0x1BC</summary>
            public uint attributesFile_extents_blockCount7;
            // HFSPlusForkData     startupFile;
            /// <summary>0x1C0</summary>
            public ulong startupFile_logicalSize;
            /// <summary>0x1C8</summary>
            public uint startupFile_clumpSize;
            /// <summary>0x1CC</summary>
            public uint startupFile_totalBlocks;
            /// <summary>0x1D0</summary>
            public uint startupFile_extents_startBlock0;
            /// <summary>0x1D4</summary>
            public uint startupFile_extents_blockCount0;
            /// <summary>0x1D8</summary>
            public uint startupFile_extents_startBlock1;
            /// <summary>0x1E0</summary>
            public uint startupFile_extents_blockCount1;
            /// <summary>0x1E4</summary>
            public uint startupFile_extents_startBlock2;
            /// <summary>0x1E8</summary>
            public uint startupFile_extents_blockCount2;
            /// <summary>0x1EC</summary>
            public uint startupFile_extents_startBlock3;
            /// <summary>0x1F0</summary>
            public uint startupFile_extents_blockCount3;
            /// <summary>0x1F4</summary>
            public uint startupFile_extents_startBlock4;
            /// <summary>0x1F8</summary>
            public uint startupFile_extents_blockCount4;
            /// <summary>0x1FC</summary>
            public uint startupFile_extents_startBlock5;
            /// <summary>0x200</summary>
            public uint startupFile_extents_blockCount5;
            /// <summary>0x204</summary>
            public uint startupFile_extents_startBlock6;
            /// <summary>0x208</summary>
            public uint startupFile_extents_blockCount6;
            /// <summary>0x20C</summary>
            public uint startupFile_extents_startBlock7;
            /// <summary>0x210</summary>
            public uint startupFile_extents_blockCount7;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}
