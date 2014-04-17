/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : MinixFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies Minix v1, v2 and v3 filesystems and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using FileSystemIDandChk;

// Information from the Linux kernel
namespace FileSystemIDandChk.Plugins
{
    class MinixFS : Plugin
    {
        const UInt16 MINIX_MAGIC = 0x137F;
        // Minix v1, 14 char filenames
        const UInt16 MINIX_MAGIC2 = 0x138F;
        // Minix v1, 30 char filenames
        const UInt16 MINIX2_MAGIC = 0x2468;
        // Minix v2, 14 char filenames
        const UInt16 MINIX2_MAGIC2 = 0x2478;
        // Minix v2, 30 char filenames
        const UInt16 MINIX3_MAGIC = 0x4D5A;
        // Minix v3, 60 char filenames
        // Byteswapped
        const UInt16 MINIX_CIGAM = 0x7F13;
        // Minix v1, 14 char filenames
        const UInt16 MINIX_CIGAM2 = 0x8F13;
        // Minix v1, 30 char filenames
        const UInt16 MINIX2_CIGAM = 0x6824;
        // Minix v2, 14 char filenames
        const UInt16 MINIX2_CIGAM2 = 0x7824;
        // Minix v2, 30 char filenames
        const UInt16 MINIX3_CIGAM = 0x5A4D;
        // Minix v3, 60 char filenames

        public MinixFS(PluginBase Core)
        {
            Name = "Minix Filesystem";
            PluginUUID = new Guid("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            UInt16 magic;
            byte[] minix_sb_sector = imagePlugin.ReadSector(2 + partitionOffset);

            magic = BitConverter.ToUInt16(minix_sb_sector, 0x010); // Here should reside magic number on Minix V1 & V2
			
            if (magic == MINIX_MAGIC || magic == MINIX_MAGIC2 || magic == MINIX2_MAGIC || magic == MINIX2_MAGIC2 ||
                magic == MINIX_CIGAM || magic == MINIX_CIGAM2 || magic == MINIX2_CIGAM || magic == MINIX2_CIGAM2)
                return true;
            magic = BitConverter.ToUInt16(minix_sb_sector, 0x018); // Here should reside magic number on Minix V3

            if (magic == MINIX3_MAGIC || magic == MINIX3_CIGAM)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();

            bool minix3 = false;
            int filenamesize;
            string minixVersion;
            UInt16 magic;
            byte[] minix_sb_sector = imagePlugin.ReadSector(2 + partitionOffset);

            magic = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x018);

            if (magic == MINIX3_MAGIC || magic == MINIX3_CIGAM)
            {
                filenamesize = 60;
                minixVersion = "Minix V3 filesystem";
                BigEndianBitConverter.IsLittleEndian = magic != MINIX3_CIGAM;

                minix3 = true;
            }
            else
            {
                magic = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x010);

                switch (magic)
                {
                    case MINIX_MAGIC:
                        filenamesize = 14;
                        minixVersion = "Minix V1 filesystem";
                        BigEndianBitConverter.IsLittleEndian = true;
                        break;
                    case MINIX_MAGIC2:
                        filenamesize = 30;
                        minixVersion = "Minix V1 filesystem";
                        BigEndianBitConverter.IsLittleEndian = true;
                        break;
                    case MINIX2_MAGIC:
                        filenamesize = 14;
                        minixVersion = "Minix V2 filesystem";
                        BigEndianBitConverter.IsLittleEndian = true;
                        break;
                    case MINIX2_MAGIC2:
                        filenamesize = 30;
                        minixVersion = "Minix V2 filesystem";
                        BigEndianBitConverter.IsLittleEndian = true;
                        break;
                    case MINIX_CIGAM:
                        filenamesize = 14;
                        minixVersion = "Minix V1 filesystem";
                        BigEndianBitConverter.IsLittleEndian = false;
                        break;
                    case MINIX_CIGAM2:
                        filenamesize = 30;
                        minixVersion = "Minix V1 filesystem";
                        BigEndianBitConverter.IsLittleEndian = false;
                        break;
                    case MINIX2_CIGAM:
                        filenamesize = 14;
                        minixVersion = "Minix V2 filesystem";
                        BigEndianBitConverter.IsLittleEndian = false;
                        break;
                    case MINIX2_CIGAM2:
                        filenamesize = 30;
                        minixVersion = "Minix V2 filesystem";
                        BigEndianBitConverter.IsLittleEndian = false;
                        break;
                    default:
                        return;
                }
            }

            if (minix3)
            {
                Minix3SuperBlock mnx_sb = new Minix3SuperBlock();

                mnx_sb.s_ninodes = BigEndianBitConverter.ToUInt32(minix_sb_sector, 0x00);
                mnx_sb.s_pad0 = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x04);
                mnx_sb.s_imap_blocks = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x06);
                mnx_sb.s_zmap_blocks = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x08);
                mnx_sb.s_firstdatazone = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x0A);
                mnx_sb.s_log_zone_size = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x0C);
                mnx_sb.s_pad1 = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x0E);
                mnx_sb.s_max_size = BigEndianBitConverter.ToUInt32(minix_sb_sector, 0x10);
                mnx_sb.s_zones = BigEndianBitConverter.ToUInt32(minix_sb_sector, 0x14);
                mnx_sb.s_magic = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x18);
                mnx_sb.s_pad2 = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x1A);
                mnx_sb.s_blocksize = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x1C);
                mnx_sb.s_disk_version = minix_sb_sector[0x1E];

                sb.AppendLine(minixVersion);
                sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();
                sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_zones, mnx_sb.s_zones * mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("{0} bytes/block", mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("{0} inodes on volume", mnx_sb.s_ninodes).AppendLine();
                sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnx_sb.s_imap_blocks, mnx_sb.s_imap_blocks * mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnx_sb.s_zmap_blocks, mnx_sb.s_zmap_blocks * mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("First data zone: {0}", mnx_sb.s_firstdatazone).AppendLine();
                //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
                sb.AppendFormat("{0} bytes maximum per file", mnx_sb.s_max_size).AppendLine();
                sb.AppendFormat("On-disk filesystem version: {0}", mnx_sb.s_disk_version).AppendLine();
            }
            else
            {
                MinixSuperBlock mnx_sb = new MinixSuperBlock();
				
                mnx_sb.s_ninodes = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x00);
                mnx_sb.s_nzones = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x02);
                mnx_sb.s_imap_blocks = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x04);
                mnx_sb.s_zmap_blocks = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x06);
                mnx_sb.s_firstdatazone = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x08);
                mnx_sb.s_log_zone_size = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x0A);
                mnx_sb.s_max_size = BigEndianBitConverter.ToUInt32(minix_sb_sector, 0x0C);
                mnx_sb.s_magic = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x10);
                mnx_sb.s_state = BigEndianBitConverter.ToUInt16(minix_sb_sector, 0x12);
                mnx_sb.s_zones = BigEndianBitConverter.ToUInt32(minix_sb_sector, 0x14);

                sb.AppendLine(minixVersion);
                sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();
                if (mnx_sb.s_zones > 0) // On V2
					sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_zones, mnx_sb.s_zones * 1024).AppendLine();
                else
                    sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_nzones, mnx_sb.s_nzones * 1024).AppendLine();
                sb.AppendFormat("{0} inodes on volume", mnx_sb.s_ninodes).AppendLine();
                sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnx_sb.s_imap_blocks, mnx_sb.s_imap_blocks * 1024).AppendLine();
                sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnx_sb.s_zmap_blocks, mnx_sb.s_zmap_blocks * 1024).AppendLine();
                sb.AppendFormat("First data zone: {0}", mnx_sb.s_firstdatazone).AppendLine();
                //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
                sb.AppendFormat("{0} bytes maximum per file", mnx_sb.s_max_size).AppendLine();
                sb.AppendFormat("Filesystem state: {0:X4}", mnx_sb.s_state).AppendLine();
            }
            information = sb.ToString();
        }

        public struct MinixSuperBlock
        {
            public UInt16 s_ninodes;
            // 0x00, inodes on volume
            public UInt16 s_nzones;
            // 0x02, zones on volume
            public UInt16 s_imap_blocks;
            // 0x04, blocks on inode map
            public UInt16 s_zmap_blocks;
            // 0x06, blocks on zone map
            public UInt16 s_firstdatazone;
            // 0x08, first data zone
            public UInt16 s_log_zone_size;
            // 0x0A, log2 of blocks/zone
            public UInt32 s_max_size;
            // 0x0C, max file size
            public UInt16 s_magic;
            // 0x10, magic
            public UInt16 s_state;
            // 0x12, filesystem state
            public UInt32 s_zones;
            // 0x14, number of zones
        }

        public struct Minix3SuperBlock
        {
            public UInt32 s_ninodes;
            // 0x00, inodes on volume
            public UInt16 s_pad0;
            // 0x04, padding
            public UInt16 s_imap_blocks;
            // 0x06, blocks on inode map
            public UInt16 s_zmap_blocks;
            // 0x08, blocks on zone map
            public UInt16 s_firstdatazone;
            // 0x0A, first data zone
            public UInt16 s_log_zone_size;
            // 0x0C, log2 of blocks/zone
            public UInt16 s_pad1;
            // 0x0E, padding
            public UInt32 s_max_size;
            // 0x10, max file size
            public UInt32 s_zones;
            // 0x14, number of zones
            public UInt16 s_magic;
            // 0x18, magic
            public UInt16 s_pad2;
            // 0x1A, padding
            public UInt16 s_blocksize;
            // 0x1C, bytes in a block
            public byte s_disk_version;
            // 0x1E, on-disk structures version
        }
    }
}

