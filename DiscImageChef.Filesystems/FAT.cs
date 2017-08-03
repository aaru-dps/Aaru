// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft FAT filesystem and shows information.
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    // TODO: Differentiate between Atari and X68k FAT, as this one uses a standard BPB.
    public class FAT : Filesystem
    {
        public FAT()
        {
            Name = "Microsoft File Allocation Table";
            PluginUUID = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
            CurrentEncoding = Encoding.GetEncoding("IBM437");
        }

        public FAT(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Microsoft File Allocation Table";
            PluginUUID = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("IBM437");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if((2 + partition.Start) >= partition.End)
                return false;

            ushort bps;
            byte spc;
            byte fats_no;
            ushort reserved_secs;
            ushort root_entries;
            ushort sectors;
            byte media_descriptor;
            ushort fat_sectors;
            uint big_sectors;
            byte bpb_signature;
            byte fat32_signature;
            ulong huge_sectors;
            byte[] fat32_id = new byte[8];
            byte[] msx_id = new byte[6];
            byte fat_id;
            byte[] dos_oem = new byte[8];
            byte[] atari_oem = new byte[6];
            ushort bootable = 0;

            byte[] bpb_sector = imagePlugin.ReadSector(0 + partition.Start);
            byte[] fat_sector = imagePlugin.ReadSector(1 + partition.Start);

            Array.Copy(bpb_sector, 0x02, atari_oem, 0, 6);
            Array.Copy(bpb_sector, 0x03, dos_oem, 0, 8);
            bps = BitConverter.ToUInt16(bpb_sector, 0x00B);
            spc = bpb_sector[0x00D];
            reserved_secs = BitConverter.ToUInt16(bpb_sector, 0x00E);
            fats_no = bpb_sector[0x010];
            root_entries = BitConverter.ToUInt16(bpb_sector, 0x011);
            sectors = BitConverter.ToUInt16(bpb_sector, 0x013);
            media_descriptor = bpb_sector[0x015];
            fat_sectors = BitConverter.ToUInt16(bpb_sector, 0x016);
            Array.Copy(bpb_sector, 0x052, msx_id, 0, 6);
            big_sectors = BitConverter.ToUInt32(bpb_sector, 0x020);
            bpb_signature = bpb_sector[0x026];
            fat32_signature = bpb_sector[0x042];
            Array.Copy(bpb_sector, 0x052, fat32_id, 0, 8);
            huge_sectors = BitConverter.ToUInt64(bpb_sector, 0x052);
            fat_id = fat_sector[0];
            int bits_in_bps = Helpers.CountBits.Count(bps);
            if(imagePlugin.ImageInfo.sectorSize >= 512)
                bootable = BitConverter.ToUInt16(bpb_sector, 0x1FE);

            bool correct_spc = spc == 1 || spc == 2 || spc == 4 || spc == 8 || spc == 16 || spc == 32 || spc == 64;
            string msx_string = Encoding.ASCII.GetString(msx_id);
            string fat32_string = Encoding.ASCII.GetString(fat32_id);
            bool atari_oem_correct = atari_oem[0] >= 0x20 && atari_oem[1] >= 0x20 && atari_oem[2] >= 0x20 &&
                                     atari_oem[3] >= 0x20 && atari_oem[4] >= 0x20 && atari_oem[5] >= 0x20;
            bool dos_oem_correct = dos_oem[0] >= 0x20 && dos_oem[1] >= 0x20 && dos_oem[2] >= 0x20 && dos_oem[3] >= 0x20 &&
                                   dos_oem[4] >= 0x20 && dos_oem[5] >= 0x20 && dos_oem[6] >= 0x20 && dos_oem[7] >= 0x20;
            string atari_string = Encoding.ASCII.GetString(atari_oem);
            string oem_string = Encoding.ASCII.GetString(dos_oem);

            DicConsole.DebugWriteLine("FAT plugin", "atari_oem_correct = {0}", atari_oem_correct);
            DicConsole.DebugWriteLine("FAT plugin", "dos_oem_correct = {0}", dos_oem_correct);
            DicConsole.DebugWriteLine("FAT plugin", "bps = {0}", bps);
            DicConsole.DebugWriteLine("FAT plugin", "bits in bps = {0}", bits_in_bps);
            DicConsole.DebugWriteLine("FAT plugin", "spc = {0}", spc);
            DicConsole.DebugWriteLine("FAT plugin", "correct_spc = {0}", correct_spc);
            DicConsole.DebugWriteLine("FAT plugin", "reserved_secs = {0}", reserved_secs);
            DicConsole.DebugWriteLine("FAT plugin", "fats_no = {0}", fats_no);
            DicConsole.DebugWriteLine("FAT plugin", "root_entries = {0}", root_entries);
            DicConsole.DebugWriteLine("FAT plugin", "sectors = {0}", sectors);
            DicConsole.DebugWriteLine("FAT plugin", "media_descriptor = 0x{0:X2}", media_descriptor);
            DicConsole.DebugWriteLine("FAT plugin", "fat_sectors = {0}", fat_sectors);
            DicConsole.DebugWriteLine("FAT plugin", "msx_id = \"{0}\"", msx_string);
            DicConsole.DebugWriteLine("FAT plugin", "big_sectors = {0}", big_sectors);
            DicConsole.DebugWriteLine("FAT plugin", "bpb_signature = 0x{0:X2}", bpb_signature);
            DicConsole.DebugWriteLine("FAT plugin", "fat32_signature = 0x{0:X2}", fat32_signature);
            DicConsole.DebugWriteLine("FAT plugin", "fat32_id = \"{0}\"", fat32_string);
            DicConsole.DebugWriteLine("FAT plugin", "huge_sectors = {0}", huge_sectors);
            DicConsole.DebugWriteLine("FAT plugin", "fat_id = 0x{0:X2}", fat_id);

            // This is to support FAT partitions on hybrid ISO/USB images
            if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
            {
                sectors /= 4;
                big_sectors /= 4;
                huge_sectors /= 4;
            }

            // exFAT
            if(oem_string == "EXFAT   ")
                return false;
            // NTFS
            if(oem_string == "NTFS    " && bootable == 0xAA55 && fats_no == 0 && fat_sectors == 0)
                return false;
            // QNX4
            if(oem_string == "FQNX4FS ")
                return false;

            // HPFS
            if(16 + partition.Start <= partition.End)
            {
                uint hpfs_magic1, hpfs_magic2;

                byte[] hpfs_sb_sector = imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16
                hpfs_magic1 = BitConverter.ToUInt32(hpfs_sb_sector, 0x000);
                hpfs_magic2 = BitConverter.ToUInt32(hpfs_sb_sector, 0x004);

                if(hpfs_magic1 == 0xF995E849 && hpfs_magic2 == 0xFA53E9C5)
                    return false;
            }

            // FAT32 for sure
            if(bits_in_bps == 1 && correct_spc && fats_no <= 2 && sectors == 0 && fat_sectors == 0 && fat32_signature == 0x29 && fat32_string == "FAT32   ")
                return true;
            // short FAT32
            if(bits_in_bps == 1 && correct_spc && fats_no <= 2 && sectors == 0 && fat_sectors == 0 && fat32_signature == 0x28)
                return big_sectors == 0 ? huge_sectors <= (partition.End - partition.Start) + 1 : big_sectors <= (partition.End - partition.Start) + 1;
            // MSX-DOS FAT12
            if(bits_in_bps == 1 && correct_spc && fats_no <= 2 && root_entries > 0 && sectors <= (partition.End - partition.Start) + 1 && fat_sectors > 0 && msx_string == "VOL_ID")
                return true;
            // EBPB
            if(bits_in_bps == 1 && correct_spc && fats_no <= 2 && root_entries > 0 && fat_sectors > 0 && (bpb_signature == 0x28 || bpb_signature == 0x29))
                return sectors == 0 ? big_sectors <= (partition.End - partition.Start) + 1 : sectors <= (partition.End - partition.Start) + 1;

            // BPB
            if(bits_in_bps == 1 && correct_spc && reserved_secs < (partition.End - partition.Start) && fats_no <= 2 && root_entries > 0 && fat_sectors > 0)
                return sectors == 0 ? big_sectors <= (partition.End - partition.Start) + 1 : sectors <= (partition.End - partition.Start) + 1;

            // All FAT12 without BPB can only be used on floppies, without partitions.
            if(partition.Start != 0)
                return false;

            byte fat2 = fat_sector[1];
            byte fat3 = fat_sector[2];
            ushort fat_2nd_cluster = (ushort)(((fat2 << 8) + fat3) & 0xFFF);

            DicConsole.DebugWriteLine("FAT plugin", "1st fat cluster 1 = {0:X3}", fat_2nd_cluster);
            if(fat_2nd_cluster < 0xFF0)
                return false;

            ulong fat2_sector_no = 0;

            switch(fat_id)
            {
                case 0xE5:
                    if(imagePlugin.ImageInfo.sectors == 2002 && imagePlugin.ImageInfo.sectorSize == 128)
                        fat2_sector_no = 2;
                    break;
                case 0xFD:
                    if(imagePlugin.ImageInfo.sectors == 4004 && imagePlugin.ImageInfo.sectorSize == 128)
                        fat2_sector_no = 7;
                    else if(imagePlugin.ImageInfo.sectors == 2002 && imagePlugin.ImageInfo.sectorSize == 128)
                        fat2_sector_no = 7;
                    break;
                case 0xFE:
                    if(imagePlugin.ImageInfo.sectors == 320 && imagePlugin.ImageInfo.sectorSize == 512)
                        fat2_sector_no = 2;
                    else if(imagePlugin.ImageInfo.sectors == 2002 && imagePlugin.ImageInfo.sectorSize == 128)
                        fat2_sector_no = 7;
                    else if(imagePlugin.ImageInfo.sectors == 1232 && imagePlugin.ImageInfo.sectorSize == 1024)
                        fat2_sector_no = 3;
                    else if(imagePlugin.ImageInfo.sectors == 616 && imagePlugin.ImageInfo.sectorSize == 1024)
                        fat2_sector_no = 2;
                    else if(imagePlugin.ImageInfo.sectors == 720 && imagePlugin.ImageInfo.sectorSize == 128)
                        fat2_sector_no = 5;
                    break;
                case 0xFF:
                    if(imagePlugin.ImageInfo.sectors == 640 && imagePlugin.ImageInfo.sectorSize == 512)
                        fat2_sector_no = 2;
                    break;
                default:
                    if(fat_id < 0xE8)
                        return false;
                    fat2_sector_no = 2;
                    break;
            }

            if(fat2_sector_no > partition.End)
                return false;

            DicConsole.DebugWriteLine("FAT plugin", "2nd fat starts at = {0}", fat2_sector_no);

            byte[] fat2_sector = imagePlugin.ReadSector(fat2_sector_no);

            fat2 = fat2_sector[1];
            fat3 = fat2_sector[2];
            fat_2nd_cluster = (ushort)(((fat2 << 8) + fat3) & 0xFFF);
            if(fat_2nd_cluster < 0xFF0)
                return false;

            return fat_id == fat2_sector[0];
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            xmlFSType = new Schemas.FileSystemType();

            bool useAtariBPB = false;
            bool useMSXBPB = false;
            bool useDOS2BPB = false;
            bool useDOS3BPB = false;
            bool useDOS32BPB = false;
            bool useDOS33BPB = false;
            bool useShortEBPB = false;
            bool useEBPB = false;
            bool useShortFAT32 = false;
            bool useLongFAT32 = false;

            AtariParameterBlock atariBPB = new AtariParameterBlock();
            MSXParameterBlock msxBPB = new MSXParameterBlock();
            BIOSParameterBlock2 dos2BPB = new BIOSParameterBlock2();
            BIOSParameterBlock30 dos30BPB = new BIOSParameterBlock30();
            BIOSParameterBlock32 dos32BPB = new BIOSParameterBlock32();
            BIOSParameterBlock33 dos33BPB = new BIOSParameterBlock33();
            BIOSParameterBlockShortEBPB shortEBPB = new BIOSParameterBlockShortEBPB();
            BIOSParameterBlockEBPB EBPB = new BIOSParameterBlockEBPB();
            FAT32ParameterBlockShort shortFat32BPB = new FAT32ParameterBlockShort();
            FAT32ParameterBlock Fat32BPB = new FAT32ParameterBlock();

            byte[] bpb_sector = imagePlugin.ReadSectors(partition.Start, 2);

            if(imagePlugin.ImageInfo.sectorSize >= 256)
            {
                IntPtr bpbPtr = Marshal.AllocHGlobal(512);
                Marshal.Copy(bpb_sector, 0, bpbPtr, 512);

                atariBPB = (AtariParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(AtariParameterBlock));
                msxBPB = (MSXParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(MSXParameterBlock));
                dos2BPB = (BIOSParameterBlock2)Marshal.PtrToStructure(bpbPtr, typeof(BIOSParameterBlock2));
                dos30BPB = (BIOSParameterBlock30)Marshal.PtrToStructure(bpbPtr, typeof(BIOSParameterBlock30));
                dos32BPB = (BIOSParameterBlock32)Marshal.PtrToStructure(bpbPtr, typeof(BIOSParameterBlock32));
                dos33BPB = (BIOSParameterBlock33)Marshal.PtrToStructure(bpbPtr, typeof(BIOSParameterBlock33));
                shortEBPB = (BIOSParameterBlockShortEBPB)Marshal.PtrToStructure(bpbPtr, typeof(BIOSParameterBlockShortEBPB));
                EBPB = (BIOSParameterBlockEBPB)Marshal.PtrToStructure(bpbPtr, typeof(BIOSParameterBlockEBPB));
                shortFat32BPB = (FAT32ParameterBlockShort)Marshal.PtrToStructure(bpbPtr, typeof(FAT32ParameterBlockShort));
                Fat32BPB = (FAT32ParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(FAT32ParameterBlock));

                Marshal.FreeHGlobal(bpbPtr);

                int bits_in_bps_atari = Helpers.CountBits.Count(atariBPB.bps);
                int bits_in_bps_msx = Helpers.CountBits.Count(msxBPB.bps);
                int bits_in_bps_dos20 = Helpers.CountBits.Count(dos2BPB.bps);
                int bits_in_bps_dos30 = Helpers.CountBits.Count(dos30BPB.bps);
                int bits_in_bps_dos32 = Helpers.CountBits.Count(dos32BPB.bps);
                int bits_in_bps_dos33 = Helpers.CountBits.Count(dos33BPB.bps);
                int bits_in_bps_dos34 = Helpers.CountBits.Count(shortEBPB.bps);
                int bits_in_bps_dos40 = Helpers.CountBits.Count(EBPB.bps);
                int bits_in_bps_fat32_short = Helpers.CountBits.Count(shortFat32BPB.bps);
                int bits_in_bps_fat32 = Helpers.CountBits.Count(Fat32BPB.bps);

                bool correct_spc_atari = atariBPB.spc == 1 || atariBPB.spc == 2 || atariBPB.spc == 4 || atariBPB.spc == 8 || atariBPB.spc == 16 || atariBPB.spc == 32 || atariBPB.spc == 64;
                bool correct_spc_msx = msxBPB.spc == 1 || msxBPB.spc == 2 || msxBPB.spc == 4 || msxBPB.spc == 8 || msxBPB.spc == 16 || msxBPB.spc == 32 || msxBPB.spc == 64;
                bool correct_spc_dos20 = dos2BPB.spc == 1 || dos2BPB.spc == 2 || dos2BPB.spc == 4 || dos2BPB.spc == 8 || dos2BPB.spc == 16 || dos2BPB.spc == 32 || dos2BPB.spc == 64;
                bool correct_spc_dos30 = dos30BPB.spc == 1 || dos30BPB.spc == 2 || dos30BPB.spc == 4 || dos30BPB.spc == 8 || dos30BPB.spc == 16 || dos30BPB.spc == 32 || dos30BPB.spc == 64;
                bool correct_spc_dos32 = dos32BPB.spc == 1 || dos32BPB.spc == 2 || dos32BPB.spc == 4 || dos32BPB.spc == 8 || dos32BPB.spc == 16 || dos32BPB.spc == 32 || dos32BPB.spc == 64;
                bool correct_spc_dos33 = dos33BPB.spc == 1 || dos33BPB.spc == 2 || dos33BPB.spc == 4 || dos33BPB.spc == 8 || dos33BPB.spc == 16 || dos33BPB.spc == 32 || dos33BPB.spc == 64;
                bool correct_spc_dos34 = shortEBPB.spc == 1 || shortEBPB.spc == 2 || shortEBPB.spc == 4 || shortEBPB.spc == 8 || shortEBPB.spc == 16 || shortEBPB.spc == 32 || shortEBPB.spc == 64;
                bool correct_spc_dos40 = EBPB.spc == 1 || EBPB.spc == 2 || EBPB.spc == 4 || EBPB.spc == 8 || EBPB.spc == 16 || EBPB.spc == 32 || EBPB.spc == 64;
                bool correct_spc_fat32_short = shortFat32BPB.spc == 1 || shortFat32BPB.spc == 2 || shortFat32BPB.spc == 4 || shortFat32BPB.spc == 8 || shortFat32BPB.spc == 16 || shortFat32BPB.spc == 32 || shortFat32BPB.spc == 64;
                bool correct_spc_fat32 = Fat32BPB.spc == 1 || Fat32BPB.spc == 2 || Fat32BPB.spc == 4 || Fat32BPB.spc == 8 || Fat32BPB.spc == 16 || Fat32BPB.spc == 32 || Fat32BPB.spc == 64;

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
                {
                    atariBPB.sectors /= 4;
                    msxBPB.sectors /= 4;
                    dos2BPB.sectors /= 4;
                    dos30BPB.sectors /= 4;
                    dos32BPB.sectors /= 4;
                    dos33BPB.sectors /= 4;
                    dos33BPB.big_sectors /= 4;
                    shortEBPB.sectors /= 4;
                    shortEBPB.big_sectors /= 4;
                    EBPB.sectors /= 4;
                    EBPB.big_sectors /= 4;
                    shortFat32BPB.sectors /= 4;
                    shortFat32BPB.big_sectors /= 4;
                    shortFat32BPB.huge_sectors /= 4;
                    Fat32BPB.sectors /= 4;
                    Fat32BPB.big_sectors /= 4;
                }

                if(bits_in_bps_fat32 == 1 && correct_spc_fat32 && Fat32BPB.fats_no <= 2 && Fat32BPB.sectors == 0 && Fat32BPB.spfat == 0 && Fat32BPB.signature == 0x29 && Encoding.ASCII.GetString(Fat32BPB.fs_type) == "FAT32   ")
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using FAT32 BPB");
                    useLongFAT32 = true;
                }
                else if(bits_in_bps_fat32_short == 1 && correct_spc_fat32_short && shortFat32BPB.fats_no <= 2 && shortFat32BPB.sectors == 0 && shortFat32BPB.spfat == 0 && shortFat32BPB.signature == 0x28)
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using short FAT32 BPB");
                    useShortFAT32 = shortFat32BPB.big_sectors == 0 ? shortFat32BPB.huge_sectors <= (partition.End - partition.Start) + 1 : shortFat32BPB.big_sectors <= (partition.End - partition.Start) + 1;
                }
                else if(bits_in_bps_msx == 1 && correct_spc_msx && msxBPB.fats_no <= 2 && msxBPB.root_ent > 0 && msxBPB.sectors <= (partition.End - partition.Start) + 1 && msxBPB.spfat > 0 && Encoding.ASCII.GetString(msxBPB.vol_id) == "VOL_ID")
                {
                    DicConsole.DebugWriteLine("FAT plugin", "Using MSX BPB");
                    useMSXBPB = true;
                }
                else if(bits_in_bps_dos40 == 1 && correct_spc_dos40 && EBPB.fats_no <= 2 && EBPB.root_ent > 0 && EBPB.spfat > 0 && (EBPB.signature == 0x28 || EBPB.signature == 0x29))
                {
                    if(EBPB.sectors == 0)
                    {
                        if(EBPB.big_sectors <= (partition.End - partition.Start) + 1)
                        {
                            if(EBPB.signature == 0x29)
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 4.0 BPB");
                                useEBPB = true;
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.4 BPB");
                                useShortEBPB = true;
                            }
                        }
                    }
                    else if(EBPB.sectors <= (partition.End - partition.Start) + 1)
                    {
                        if(EBPB.signature == 0x29)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 4.0 BPB");
                            useEBPB = true;
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.4 BPB");
                            useShortEBPB = true;
                        }
                    }
                }
                else if(bits_in_bps_dos33 == 1 && correct_spc_dos33 && dos33BPB.rsectors < (partition.End - partition.Start) && dos33BPB.fats_no <= 2 && dos33BPB.root_ent > 0 && dos33BPB.spfat > 0)
                {
                    if(dos33BPB.sectors == 0 && dos33BPB.hsectors <= partition.Start && dos33BPB.big_sectors > 0 && dos33BPB.big_sectors <= (partition.End - partition.Start) + 1)
                    {
                        DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.3 BPB");
                        useDOS33BPB = true;
                    }
                    else if(dos33BPB.big_sectors == 0 && dos33BPB.hsectors <= partition.Start && dos33BPB.sectors > 0 && dos33BPB.sectors <= (partition.End - partition.Start) + 1)
                    {
                        if(atariBPB.jump[0] == 0x60 || (atariBPB.jump[0] == 0xE9 && atariBPB.jump[1] == 0x00) && Encoding.ASCII.GetString(dos33BPB.oem_name) != "NEXT    ")
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                            useAtariBPB = true;
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.3 BPB");
                            useDOS33BPB = true;
                        }
                    }
                    else
                    {
                        if(dos32BPB.hsectors <= partition.Start && dos32BPB.hsectors + dos32BPB.sectors == dos32BPB.total_sectors)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.2 BPB");
                            useDOS32BPB = true;
                        }
                        else if(dos30BPB.sptrk > 0 && dos30BPB.sptrk < 64 && dos30BPB.heads > 0 && dos30BPB.heads < 256)
                        {
                            if(atariBPB.jump[0] == 0x60 || (atariBPB.jump[0] == 0xE9 && atariBPB.jump[1] == 0x00) && Encoding.ASCII.GetString(dos33BPB.oem_name) != "NEXT    ")
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                                useAtariBPB = true;
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 3.0 BPB");
                                useDOS3BPB = true;
                            }
                        }
                        else
                        {
                            if(atariBPB.jump[0] == 0x60 || (atariBPB.jump[0] == 0xE9 && atariBPB.jump[1] == 0x00) && Encoding.ASCII.GetString(dos33BPB.oem_name) != "NEXT    ")
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using Atari BPB");
                                useAtariBPB = true;
                            }
                            else
                            {
                                DicConsole.DebugWriteLine("FAT plugin", "Using DOS 2.0 BPB");
                                useDOS2BPB = true;
                            }
                        }
                    }
                }
            }

            BIOSParameterBlockEBPB fakeBPB = new BIOSParameterBlockEBPB();
            byte[] fat_sector;
            bool isFAT12 = false;
            bool isFAT16 = false;
            bool isFAT32 = false;
            ulong root_directory_sector = 0;
            string extraInfo = null;
            string bootChk = null;
            Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
            sha1Ctx.Init();
            byte[] chkTmp;

            // This is needed because for FAT16, GEMDOS increases bytes per sector count instead of using big_sectors field.
            uint sectors_per_real_sector = 0;
            // This is needed because some OSes don't put volume label as first entry in the root directory
            uint sectors_for_root_directory = 0;

            if(!useAtariBPB && !useMSXBPB && !useDOS2BPB && !useDOS3BPB && !useDOS32BPB && !useDOS33BPB && !useShortEBPB && !useEBPB && !useShortFAT32 && !useLongFAT32)
            {
                isFAT12 = true;
                fat_sector = imagePlugin.ReadSector(1 + partition.Start);
                switch(fat_sector[0])
                {
                    case 0xE5:
                        if(imagePlugin.ImageInfo.sectors == 2002 && imagePlugin.ImageInfo.sectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 128;
                            fakeBPB.spc = 4;
                            fakeBPB.rsectors = 1;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 64;
                            fakeBPB.sectors = 2002;
                            fakeBPB.media = 0xE5;
                            fakeBPB.sptrk = 26;
                            fakeBPB.heads = 1;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 1;
                        }
                        break;
                    case 0xFD:
                        if(imagePlugin.ImageInfo.sectors == 4004 && imagePlugin.ImageInfo.sectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 128;
                            fakeBPB.spc = 4;
                            fakeBPB.rsectors = 4;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 68;
                            fakeBPB.sectors = 4004;
                            fakeBPB.media = 0xFD;
                            fakeBPB.sptrk = 26;
                            fakeBPB.heads = 2;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 6;
                        }
                        else if(imagePlugin.ImageInfo.sectors == 2002 && imagePlugin.ImageInfo.sectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 128;
                            fakeBPB.spc = 4;
                            fakeBPB.rsectors = 4;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 68;
                            fakeBPB.sectors = 2002;
                            fakeBPB.media = 0xFD;
                            fakeBPB.sptrk = 26;
                            fakeBPB.heads = 1;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 6;
                        }
                        break;
                    case 0xFE:
                        if(imagePlugin.ImageInfo.sectors == 320 && imagePlugin.ImageInfo.sectorSize == 512)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" SSDD.");
                            fakeBPB.bps = 512;
                            fakeBPB.spc = 1;
                            fakeBPB.rsectors = 1;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 64;
                            fakeBPB.sectors = 320;
                            fakeBPB.media = 0xFE;
                            fakeBPB.sptrk = 8;
                            fakeBPB.heads = 1;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 1;
                        }
                        else if(imagePlugin.ImageInfo.sectors == 2002 && imagePlugin.ImageInfo.sectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 128;
                            fakeBPB.spc = 4;
                            fakeBPB.rsectors = 1;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 68;
                            fakeBPB.sectors = 2002;
                            fakeBPB.media = 0xFE;
                            fakeBPB.sptrk = 26;
                            fakeBPB.heads = 1;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 6;
                        }
                        else if(imagePlugin.ImageInfo.sectors == 1232 && imagePlugin.ImageInfo.sectorSize == 1024)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 1024;
                            fakeBPB.spc = 1;
                            fakeBPB.rsectors = 1;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 192;
                            fakeBPB.sectors = 1232;
                            fakeBPB.media = 0xFE;
                            fakeBPB.sptrk = 8;
                            fakeBPB.heads = 2;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 2;
                        }
                        else if(imagePlugin.ImageInfo.sectors == 616 && imagePlugin.ImageInfo.sectorSize == 1024)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 1024;
                            fakeBPB.spc = 1;
                            fakeBPB.rsectors = 1;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 6192;
                            fakeBPB.sectors = 616;
                            fakeBPB.media = 0xFE;
                            fakeBPB.sptrk = 8;
                            fakeBPB.heads = 2;
                            fakeBPB.hsectors = 0;
                        }
                        else if(imagePlugin.ImageInfo.sectors == 720 && imagePlugin.ImageInfo.sectorSize == 128)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB.");
                            fakeBPB.bps = 128;
                            fakeBPB.spc = 2;
                            fakeBPB.rsectors = 54;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 64;
                            fakeBPB.sectors = 720;
                            fakeBPB.media = 0xFE;
                            fakeBPB.sptrk = 18;
                            fakeBPB.heads = 1;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 4;
                        }
                        break;
                    case 0xFF:
                        if(imagePlugin.ImageInfo.sectors == 640 && imagePlugin.ImageInfo.sectorSize == 512)
                        {
                            DicConsole.DebugWriteLine("FAT plugin", "Using hardcoded BPB for 5.25\" DSDD.");
                            fakeBPB.bps = 512;
                            fakeBPB.spc = 2;
                            fakeBPB.rsectors = 1;
                            fakeBPB.fats_no = 2;
                            fakeBPB.root_ent = 112;
                            fakeBPB.sectors = 640;
                            fakeBPB.media = 0xFF;
                            fakeBPB.sptrk = 8;
                            fakeBPB.heads = 2;
                            fakeBPB.hsectors = 0;
                            fakeBPB.spfat = 1;
                        }
                        break;
                }

                // This assumes a bootable sector will jump somewhere or disable interrupts in x86 code
                xmlFSType.Bootable |= (bpb_sector[0] == 0xFA || (bpb_sector[0] == 0xEB && bpb_sector[1] <= 0x7F));
                fakeBPB.boot_code = bpb_sector;
            }
            else if(useShortFAT32 || useLongFAT32)
            {
                isFAT32 = true;

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
                {
                    Fat32BPB.bps *= 4;
                    Fat32BPB.spc /= 4;
                    Fat32BPB.big_spfat /= 4;
                    Fat32BPB.hsectors /= 4;
                    Fat32BPB.sptrk /= 4;
                }

                if(Fat32BPB.version != 0)
                {
                    sb.AppendLine("FAT+");
                    xmlFSType.Type = "FAT+";
                }
                else
                {
                    sb.AppendLine("Microsoft FAT32");
                    xmlFSType.Type = "FAT32";
                }

                if(Fat32BPB.oem_name != null)
                {
                    if(Fat32BPB.oem_name[5] == 0x49 && Fat32BPB.oem_name[6] == 0x48 && Fat32BPB.oem_name[7] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                        xmlFSType.SystemIdentifier = StringHandlers.CToString(Fat32BPB.oem_name);
                }

                if(!string.IsNullOrEmpty(xmlFSType.SystemIdentifier))
                    sb.AppendFormat("OEM Name: {0}", xmlFSType.SystemIdentifier.Trim()).AppendLine();
                sb.AppendFormat("{0} bytes per sector.", Fat32BPB.bps).AppendLine();
                sb.AppendFormat("{0} sectors per cluster.", Fat32BPB.spc).AppendLine();
                xmlFSType.ClusterSize = Fat32BPB.bps * Fat32BPB.spc;
                sb.AppendFormat("{0} sectors reserved between BPB and FAT.", Fat32BPB.rsectors).AppendLine();
                if(Fat32BPB.big_sectors == 0 && Fat32BPB.signature == 0x28)
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", shortFat32BPB.huge_sectors, shortFat32BPB.huge_sectors * shortFat32BPB.bps).AppendLine();
                    xmlFSType.Clusters = (long)(shortFat32BPB.huge_sectors / shortFat32BPB.spc);
                }
                else
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", Fat32BPB.big_sectors, Fat32BPB.big_sectors * Fat32BPB.bps).AppendLine();
                    xmlFSType.Clusters = Fat32BPB.big_sectors / Fat32BPB.spc;
                }
                sb.AppendFormat("{0} clusters on volume.", xmlFSType.Clusters).AppendLine();
                sb.AppendFormat("Media descriptor: 0x{0:X2}", Fat32BPB.media).AppendLine();
                sb.AppendFormat("{0} sectors per FAT.", Fat32BPB.big_spfat).AppendLine();
                sb.AppendFormat("{0} sectors per track.", Fat32BPB.sptrk).AppendLine();
                sb.AppendFormat("{0} heads.", Fat32BPB.heads).AppendLine();
                sb.AppendFormat("{0} hidden sectors before BPB.", Fat32BPB.hsectors).AppendLine();
                sb.AppendFormat("Cluster of root directory: {0}", Fat32BPB.root_cluster).AppendLine();
                sb.AppendFormat("Sector of FSINFO structure: {0}", Fat32BPB.fsinfo_sector).AppendLine();
                sb.AppendFormat("Sector of backup FAT32 parameter block: {0}", Fat32BPB.backup_sector).AppendLine();
                sb.AppendFormat("Drive number: 0x{0:X2}", Fat32BPB.drive_no).AppendLine();
                sb.AppendFormat("Volume Serial Number: 0x{0:X8}", Fat32BPB.serial_no).AppendLine();
                xmlFSType.VolumeSerial = string.Format("{0:X8}", Fat32BPB.serial_no);

                if((Fat32BPB.flags & 0xF8) == 0x00)
                {
                    if((Fat32BPB.flags & 0x01) == 0x01)
                    {
                        sb.AppendLine("Volume should be checked on next mount.");
                        xmlFSType.Dirty = true;
                    }
                    if((Fat32BPB.flags & 0x02) == 0x02)
                        sb.AppendLine("Disk surface should be on next mount.");
                }

                if((Fat32BPB.mirror_flags & 0x80) == 0x80)
                    sb.AppendFormat("FATs are out of sync. FAT #{0} is in use.", Fat32BPB.mirror_flags & 0xF).AppendLine();
                else
                    sb.AppendLine("All copies of FAT are the same.");

                if((Fat32BPB.mirror_flags & 0x6F20) == 0x6F20)
                    sb.AppendLine("DR-DOS will boot this FAT32 using CHS.");
                else if((Fat32BPB.mirror_flags & 0x4F20) == 0x4F20)
                    sb.AppendLine("DR-DOS will boot this FAT32 using LBA.");

                if(Fat32BPB.signature == 0x29)
                {
                    xmlFSType.VolumeName = Encoding.ASCII.GetString(Fat32BPB.volume_label);
                    sb.AppendFormat("Filesystem type: {0}", Encoding.ASCII.GetString(Fat32BPB.fs_type)).AppendLine();
                    bootChk = sha1Ctx.Data(Fat32BPB.boot_code, out chkTmp);
                }
                else
                    bootChk = sha1Ctx.Data(shortFat32BPB.boot_code, out chkTmp);

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                xmlFSType.Bootable |= (Fat32BPB.jump[0] == 0xEB && Fat32BPB.jump[1] > 0x58 && Fat32BPB.jump[1] < 0x80 && Fat32BPB.boot_signature == 0xAA55);

                sectors_per_real_sector = Fat32BPB.bps / imagePlugin.ImageInfo.sectorSize;
                // First root directory sector
                root_directory_sector = (ulong)((Fat32BPB.root_cluster - 2) * Fat32BPB.spc + Fat32BPB.big_spfat * Fat32BPB.fats_no + Fat32BPB.rsectors) * sectors_per_real_sector;
                sectors_for_root_directory = 1;

                if(Fat32BPB.fsinfo_sector + partition.Start <= partition.End)
                {
                    byte[] fsinfo_sector = imagePlugin.ReadSector(Fat32BPB.fsinfo_sector + partition.Start);
                    IntPtr fsinfo_ptr = Marshal.AllocHGlobal(512);
                    Marshal.Copy(fsinfo_sector, 0, fsinfo_ptr, 512);
                    FSInfoSector fs_info = (FSInfoSector)Marshal.PtrToStructure(fsinfo_ptr, typeof(FSInfoSector));
                    Marshal.FreeHGlobal(fsinfo_ptr);

                    if(fs_info.signature1 == fsinfo_signature1 &&
                      fs_info.signature2 == fsinfo_signature2 &&
                      fs_info.signature3 == fsinfo_signature3)
                    {
                        if(fs_info.free_clusters < 0xFFFFFFFF)
                        {
                            sb.AppendFormat("{0} free clusters", fs_info.free_clusters).AppendLine();
                            xmlFSType.FreeClusters = fs_info.free_clusters;
                            xmlFSType.FreeClustersSpecified = true;
                        }

                        if(fs_info.last_cluster > 2 && fs_info.last_cluster < 0xFFFFFFFF)
                            sb.AppendFormat("Last allocated cluster {0}", fs_info.last_cluster).AppendLine();
                    }
                }
            }
            else if(useEBPB)
                fakeBPB = EBPB;
            else if(useShortEBPB)
            {
                fakeBPB.jump = shortEBPB.jump;
                fakeBPB.oem_name = shortEBPB.oem_name;
                fakeBPB.bps = shortEBPB.bps;
                fakeBPB.spc = shortEBPB.spc;
                fakeBPB.rsectors = shortEBPB.rsectors;
                fakeBPB.fats_no = shortEBPB.fats_no;
                fakeBPB.root_ent = shortEBPB.root_ent;
                fakeBPB.sectors = shortEBPB.sectors;
                fakeBPB.media = shortEBPB.media;
                fakeBPB.spfat = shortEBPB.spfat;
                fakeBPB.sptrk = shortEBPB.sptrk;
                fakeBPB.heads = shortEBPB.heads;
                fakeBPB.hsectors = shortEBPB.hsectors;
                fakeBPB.big_sectors = shortEBPB.big_sectors;
                fakeBPB.drive_no = shortEBPB.drive_no;
                fakeBPB.flags = shortEBPB.flags;
                fakeBPB.signature = shortEBPB.signature;
                fakeBPB.serial_no = shortEBPB.serial_no;
                fakeBPB.boot_code = shortEBPB.boot_code;
                fakeBPB.boot_signature = shortEBPB.boot_signature;
            }
            else if(useDOS33BPB)
            {
                fakeBPB.jump = dos33BPB.jump;
                fakeBPB.oem_name = dos33BPB.oem_name;
                fakeBPB.bps = dos33BPB.bps;
                fakeBPB.spc = dos33BPB.spc;
                fakeBPB.rsectors = dos33BPB.rsectors;
                fakeBPB.fats_no = dos33BPB.fats_no;
                fakeBPB.root_ent = dos33BPB.root_ent;
                fakeBPB.sectors = dos33BPB.sectors;
                fakeBPB.media = dos33BPB.media;
                fakeBPB.spfat = dos33BPB.spfat;
                fakeBPB.sptrk = dos33BPB.sptrk;
                fakeBPB.heads = dos33BPB.heads;
                fakeBPB.hsectors = dos33BPB.hsectors;
                fakeBPB.big_sectors = dos33BPB.big_sectors;
                fakeBPB.boot_code = dos33BPB.boot_code;
                fakeBPB.boot_signature = dos33BPB.boot_signature;
            }
            else if(useDOS32BPB)
            {
                fakeBPB.jump = dos32BPB.jump;
                fakeBPB.oem_name = dos32BPB.oem_name;
                fakeBPB.bps = dos32BPB.bps;
                fakeBPB.spc = dos32BPB.spc;
                fakeBPB.rsectors = dos32BPB.rsectors;
                fakeBPB.fats_no = dos32BPB.fats_no;
                fakeBPB.root_ent = dos32BPB.root_ent;
                fakeBPB.sectors = dos32BPB.sectors;
                fakeBPB.media = dos32BPB.media;
                fakeBPB.spfat = dos32BPB.spfat;
                fakeBPB.sptrk = dos32BPB.sptrk;
                fakeBPB.heads = dos32BPB.heads;
                fakeBPB.hsectors = dos32BPB.hsectors;
                fakeBPB.boot_code = dos32BPB.boot_code;
                fakeBPB.boot_signature = dos32BPB.boot_signature;
            }
            else if(useDOS3BPB)
            {
                fakeBPB.jump = dos30BPB.jump;
                fakeBPB.oem_name = dos30BPB.oem_name;
                fakeBPB.bps = dos30BPB.bps;
                fakeBPB.spc = dos30BPB.spc;
                fakeBPB.rsectors = dos30BPB.rsectors;
                fakeBPB.fats_no = dos30BPB.fats_no;
                fakeBPB.root_ent = dos30BPB.root_ent;
                fakeBPB.sectors = dos30BPB.sectors;
                fakeBPB.media = dos30BPB.media;
                fakeBPB.spfat = dos30BPB.spfat;
                fakeBPB.sptrk = dos30BPB.sptrk;
                fakeBPB.heads = dos30BPB.heads;
                fakeBPB.hsectors = dos30BPB.hsectors;
                fakeBPB.boot_code = dos30BPB.boot_code;
                fakeBPB.boot_signature = dos30BPB.boot_signature;
            }
            else if(useDOS2BPB)
            {
                fakeBPB.jump = dos2BPB.jump;
                fakeBPB.oem_name = dos2BPB.oem_name;
                fakeBPB.bps = dos2BPB.bps;
                fakeBPB.spc = dos2BPB.spc;
                fakeBPB.rsectors = dos2BPB.rsectors;
                fakeBPB.fats_no = dos2BPB.fats_no;
                fakeBPB.root_ent = dos2BPB.root_ent;
                fakeBPB.sectors = dos2BPB.sectors;
                fakeBPB.media = dos2BPB.media;
                fakeBPB.spfat = dos2BPB.spfat;
                fakeBPB.boot_code = dos2BPB.boot_code;
                fakeBPB.boot_signature = dos2BPB.boot_signature;
            }
            else if(useMSXBPB)
            {
                isFAT12 = true;
                fakeBPB.jump = msxBPB.jump;
                fakeBPB.oem_name = msxBPB.oem_name;
                fakeBPB.bps = msxBPB.bps;
                fakeBPB.spc = msxBPB.spc;
                fakeBPB.rsectors = msxBPB.rsectors;
                fakeBPB.fats_no = msxBPB.fats_no;
                fakeBPB.root_ent = msxBPB.root_ent;
                fakeBPB.sectors = msxBPB.sectors;
                fakeBPB.media = msxBPB.media;
                fakeBPB.spfat = msxBPB.spfat;
                fakeBPB.sptrk = msxBPB.sptrk;
                fakeBPB.heads = msxBPB.heads;
                fakeBPB.hsectors = msxBPB.hsectors;
                fakeBPB.boot_code = msxBPB.boot_code;
                fakeBPB.boot_signature = msxBPB.boot_signature;
                fakeBPB.serial_no = msxBPB.serial_no;
                // TODO: Is there any way to check this?
                xmlFSType.Bootable = true;
            }
            else if(useAtariBPB)
            {
                fakeBPB.jump = atariBPB.jump;
                fakeBPB.oem_name = atariBPB.oem_name;
                fakeBPB.bps = atariBPB.bps;
                fakeBPB.spc = atariBPB.spc;
                fakeBPB.rsectors = atariBPB.rsectors;
                fakeBPB.fats_no = atariBPB.fats_no;
                fakeBPB.root_ent = atariBPB.root_ent;
                fakeBPB.sectors = atariBPB.sectors;
                fakeBPB.media = atariBPB.media;
                fakeBPB.spfat = atariBPB.spfat;
                fakeBPB.sptrk = atariBPB.sptrk;
                fakeBPB.heads = atariBPB.heads;
                fakeBPB.boot_code = atariBPB.boot_code;

                ushort sum = 0;
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                for(int i = 0; i < bpb_sector.Length; i += 2)
                    sum += BigEndianBitConverter.ToUInt16(bpb_sector, i);

                // TODO: Check this
                if(sum == 0x1234)
                {
                    xmlFSType.Bootable = true;
                    StringBuilder atariSb = new StringBuilder();
                    atariSb.AppendFormat("cmdload will be loaded with value {0:X4}h", BigEndianBitConverter.ToUInt16(bpb_sector, 0x01E)).AppendLine();
                    atariSb.AppendFormat("Boot program will be loaded at address {0:X4}h", atariBPB.ldaaddr).AppendLine();
                    atariSb.AppendFormat("FAT and directory will be cached at address {0:X4}h", atariBPB.fatbuf).AppendLine();
                    if(atariBPB.ldmode == 0)
                    {
                        byte[] tmp = new byte[8];
                        Array.Copy(atariBPB.fname, 0, tmp, 0, 8);
                        string fname = Encoding.ASCII.GetString(tmp).Trim();
                        tmp = new byte[3];
                        Array.Copy(atariBPB.fname, 8, tmp, 0, 3);
                        string extension = Encoding.ASCII.GetString(tmp).Trim();
                        string filename;

                        if(string.IsNullOrEmpty(extension))
                            filename = fname;
                        else
                            filename = fname + "." + extension;

                        atariSb.AppendFormat("Boot program resides in file \"{0}\"", filename).AppendLine();
                    }
                    else
                        atariSb.AppendFormat("Boot program starts in sector {0} and is {1} sectors long ({2} bytes)", atariBPB.ssect, atariBPB.sectcnt, atariBPB.sectcnt * atariBPB.bps).AppendLine();

                    extraInfo = atariSb.ToString();
                }
            }

            if(!isFAT32)
            {
                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
                {
                    fakeBPB.bps *= 4;
                    fakeBPB.spc /= 4;
                    fakeBPB.spfat /= 4;
                    fakeBPB.hsectors /= 4;
                    fakeBPB.sptrk /= 4;
                    fakeBPB.rsectors /= 4;
                }

                // This assumes no sane implementation will violate cluster size rules
                // However nothing prevents this to happen
                // If first file on disk uses only one cluster there is absolutely no way to differentiate between FAT12 and FAT16,
                // so let's hope implementations use common sense?
                if(!isFAT12 && !isFAT16)
                {
                    ulong clusters;
                    if(fakeBPB.sectors == 0)
                        clusters = fakeBPB.big_sectors / fakeBPB.spc;
                    else
                        clusters = (ulong)(fakeBPB.sectors / fakeBPB.spc);

                    if(clusters < 4089)
                        isFAT12 = true;
                    else
                        isFAT16 = true;
                }

                if(isFAT12)
                {
                    if(useAtariBPB)
                        sb.AppendLine("Atari FAT12");
                    else
                        sb.AppendLine("Microsoft FAT12");
                    xmlFSType.Type = "FAT12";
                }
                else if(isFAT16)
                {
                    if(useAtariBPB)
                        sb.AppendLine("Atari FAT16");
                    else
                        sb.AppendLine("Microsoft FAT16");
                    xmlFSType.Type = "FAT16";
                }

                if(useAtariBPB)
                {
                    if(atariBPB.serial_no[0] == 0x49 && atariBPB.serial_no[1] == 0x48 && atariBPB.serial_no[2] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                        xmlFSType.VolumeSerial = string.Format("{0:X2}{1:X2}{2:X2}", atariBPB.serial_no[0], atariBPB.serial_no[1], atariBPB.serial_no[2]);

                    xmlFSType.SystemIdentifier = StringHandlers.CToString(atariBPB.oem_name);
                    if(string.IsNullOrEmpty(xmlFSType.SystemIdentifier))
                        xmlFSType.SystemIdentifier = null;
                }
                else if(fakeBPB.oem_name != null)
                {
                    if(fakeBPB.oem_name[5] == 0x49 && fakeBPB.oem_name[6] == 0x48 && fakeBPB.oem_name[7] == 0x43)
                        sb.AppendLine("Volume has been modified by Windows 9x/Me Volume Tracker.");
                    else
                    {
                        // Later versions of Windows create a DOS 3 BPB without OEM name on 8 sectors/track floppies
                        // OEM ID should be ASCII, otherwise ignore it
                        if(fakeBPB.oem_name[0] >= 0x20 && fakeBPB.oem_name[0] <= 0x7F && 
                          fakeBPB.oem_name[1] >= 0x20 && fakeBPB.oem_name[1] <= 0x7F && 
                          fakeBPB.oem_name[2] >= 0x20 && fakeBPB.oem_name[2] <= 0x7F && 
                          fakeBPB.oem_name[3] >= 0x20 && fakeBPB.oem_name[3] <= 0x7F && 
                          fakeBPB.oem_name[4] >= 0x20 && fakeBPB.oem_name[4] <= 0x7F && 
                          fakeBPB.oem_name[5] >= 0x20 && fakeBPB.oem_name[5] <= 0x7F && 
                          fakeBPB.oem_name[6] >= 0x20 && fakeBPB.oem_name[6] <= 0x7F && 
                          fakeBPB.oem_name[7] >= 0x20 && fakeBPB.oem_name[7] <= 0x7F)
                            xmlFSType.SystemIdentifier = StringHandlers.CToString(fakeBPB.oem_name);
                    }

                    if(fakeBPB.signature == 0x28 || fakeBPB.signature == 0x29)
                        xmlFSType.VolumeSerial = string.Format("{0:X8}", fakeBPB.serial_no);
                }

                if(xmlFSType.SystemIdentifier != null)
                    sb.AppendFormat("OEM Name: {0}", xmlFSType.SystemIdentifier.Trim()).AppendLine();

                sb.AppendFormat("{0} bytes per sector.", fakeBPB.bps).AppendLine();
                if(fakeBPB.sectors == 0)
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", fakeBPB.big_sectors, fakeBPB.big_sectors * fakeBPB.bps).AppendLine();
                    xmlFSType.Clusters = (long)(fakeBPB.big_sectors / fakeBPB.spc);
                }
                else
                {
                    sb.AppendFormat("{0} sectors on volume ({1} bytes).", fakeBPB.sectors, fakeBPB.sectors * fakeBPB.bps).AppendLine();
                    xmlFSType.Clusters = fakeBPB.sectors / fakeBPB.spc;
                }
                sb.AppendFormat("{0} sectors per cluster.", fakeBPB.spc).AppendLine();
                sb.AppendFormat("{0} clusters on volume.", xmlFSType.Clusters).AppendLine();
                xmlFSType.ClusterSize = fakeBPB.bps * fakeBPB.spc;
                sb.AppendFormat("{0} sectors reserved between BPB and FAT.", fakeBPB.rsectors).AppendLine();
                sb.AppendFormat("{0} FATs.", fakeBPB.fats_no).AppendLine();
                sb.AppendFormat("{0} entries on root directory.", fakeBPB.root_ent).AppendLine();

                if(fakeBPB.media > 0)
                    sb.AppendFormat("Media descriptor: 0x{0:X2}", fakeBPB.media).AppendLine();
                
                sb.AppendFormat("{0} sectors per FAT.", fakeBPB.spfat).AppendLine();

                if(fakeBPB.sptrk > 0 && fakeBPB.sptrk < 64 && fakeBPB.heads > 0 && fakeBPB.heads < 256)
                {
                    sb.AppendFormat("{0} sectors per track.", fakeBPB.sptrk).AppendLine();
                    sb.AppendFormat("{0} heads.", fakeBPB.heads).AppendLine();
                
                }
                if(fakeBPB.hsectors <= partition.Start)
                    sb.AppendFormat("{0} hidden sectors before BPB.", fakeBPB.hsectors).AppendLine();

                if(fakeBPB.signature == 0x28 || fakeBPB.signature == 0x29)
                {
                    sb.AppendFormat("Drive number: 0x{0:X2}", fakeBPB.drive_no).AppendLine();

                    if(xmlFSType.VolumeSerial != null)
                        sb.AppendFormat("Volume Serial Number: {0}", xmlFSType.VolumeSerial).AppendLine();

                    if((fakeBPB.flags & 0xF8) == 0x00)
                    {
                        if((fakeBPB.flags & 0x01) == 0x01)
                        {
                            sb.AppendLine("Volume should be checked on next mount.");
                            xmlFSType.Dirty = true;
                        }
                        if((fakeBPB.flags & 0x02) == 0x02)
                            sb.AppendLine("Disk surface should be on next mount.");
                    }

                    if(fakeBPB.signature == 0x29)
                    {
                        xmlFSType.VolumeName = Encoding.ASCII.GetString(fakeBPB.volume_label);
                        sb.AppendFormat("Filesystem type: {0}", Encoding.ASCII.GetString(fakeBPB.fs_type)).AppendLine();
                    }
                }
                else if(useAtariBPB && xmlFSType.VolumeSerial != null)
                    sb.AppendFormat("Volume Serial Number: {0}", xmlFSType.VolumeSerial).AppendLine();

                bootChk = sha1Ctx.Data(fakeBPB.boot_code, out chkTmp);

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                if(xmlFSType.Bootable == false && fakeBPB.jump != null)
                    xmlFSType.Bootable |= (fakeBPB.jump[0] == 0xEB && fakeBPB.jump[1] > 0x58 && fakeBPB.jump[1] < 0x80 && fakeBPB.boot_signature == 0xAA55);

                sectors_per_real_sector = fakeBPB.bps / imagePlugin.ImageInfo.sectorSize;
                // First root directory sector
                root_directory_sector = (ulong)(fakeBPB.spfat * fakeBPB.fats_no + fakeBPB.rsectors) * sectors_per_real_sector;
                sectors_for_root_directory = (uint)((fakeBPB.root_ent * 32) / imagePlugin.ImageInfo.sectorSize);
            }

            if(extraInfo != null)
                sb.Append(extraInfo);

            if(root_directory_sector + partition.Start < partition.End && imagePlugin.ImageInfo.xmlMediaType != ImagePlugins.XmlMediaType.OpticalDisc)
            {
                byte[] root_directory = imagePlugin.ReadSectors(root_directory_sector + partition.Start, sectors_for_root_directory);
                for(int i = 0; i < root_directory.Length; i += 32)
                {
                    // Not a correct entry
                    if(root_directory[i] < 0x20 && root_directory[i] != 0x05)
                        continue;

                    // Deleted or subdirectory entry
                    if(root_directory[i] == 0x2E || root_directory[i] == 0xE5)
                        continue;

                    // Not a volume label
                    if(root_directory[i + 0x0B] != 0x08 && root_directory[i + 0x0B] != 0x28)
                        continue;

                    IntPtr entry_ptr = Marshal.AllocHGlobal(32);
                    Marshal.Copy(root_directory, i, entry_ptr, 32);
                    DirectoryEntry entry = (DirectoryEntry)Marshal.PtrToStructure(entry_ptr, typeof(DirectoryEntry));
                    Marshal.FreeHGlobal(entry_ptr);

                    byte[] fullname = new byte[11];
                    Array.Copy(entry.filename, 0, fullname, 0, 8);
                    Array.Copy(entry.extension, 0, fullname, 8, 3);
                    string volname = CurrentEncoding.GetString(fullname).Trim();
                    if(!string.IsNullOrEmpty(volname))
                    {
                        if((entry.caseinfo & 0x0C) > 0)
                            xmlFSType.VolumeName = volname.ToLower();
                        else
                            xmlFSType.VolumeName = volname;
                    }

                    if(entry.ctime > 0 && entry.cdate > 0)
                    {
                        xmlFSType.CreationDate = DateHandlers.DOSToDateTime(entry.cdate, entry.ctime);
                        if(entry.ctime_ms > 0)
                            xmlFSType.CreationDate.AddMilliseconds(entry.ctime_ms * 10);
                        xmlFSType.CreationDateSpecified = true;
                        sb.AppendFormat("Volume created on {0}", xmlFSType.CreationDate).AppendLine();
                    }

                    if(entry.mtime > 0 && entry.mdate > 0)
                    {
                        xmlFSType.ModificationDate = DateHandlers.DOSToDateTime(entry.mdate, entry.mtime);
                        xmlFSType.ModificationDateSpecified = true;
                        sb.AppendFormat("Volume last modified on {0}", xmlFSType.ModificationDate).AppendLine();
                    }

                    if(entry.adate > 0)
                        sb.AppendFormat("Volume last accessed on {0:d}", DateHandlers.DOSToDateTime(entry.adate, 0)).AppendLine();
                }
            }

            if(!string.IsNullOrEmpty(xmlFSType.VolumeName))
                sb.AppendFormat("Volume label: {0}", xmlFSType.VolumeName).AppendLine();
            if(xmlFSType.Bootable)
            {
                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
            }

            information = sb.ToString();
        }

        /// <summary>
        /// BIOS Parameter Block as used by Atari ST GEMDOS on FAT12 volumes.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AtariParameterBlock
        {
            /// <summary>68000 BRA.S jump or x86 loop</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] jump;
            /// <summary>OEM Name, 6 bytes, space-padded, "Loader" for Atari ST boot loader</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] oem_name;
            /// <summary>Volume serial number<summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] serial_no;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT (inclusive)</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor, unused by GEMDOS</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB, unused by GEMDOS</summary>
            public ushort hsectors;
            /// <summary>Word to be loaded in the cmdload system variable. Big-endian.</summary>
            public ushort execflag;
            /// <summary>Word indicating load mode. If zero, file named <see cref="fname"/> is located and loaded. It not, sectors specified in <see cref="ssect"/> and <see cref="sectcnt"/> are loaded. Big endian.</summary>
            public ushort ldmode;
            /// <summary>Starting sector of boot code.</summary>
            public ushort ssect;
            /// <summary>Count of sectors of boot code.</summary>
            public ushort sectcnt;
            /// <summary>Address where boot code should be loaded.</summary>
            public ushort ldaaddr;
            /// <summary>Padding.</summary>
            public ushort padding;
            /// <summary>Address where FAT and root directory sectors must be loaded.</summary>
            public ushort fatbuf;
            /// <summary>Unknown.</summary>
            public ushort unknown;
            /// <summary>Filename to be loaded for booting.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] fname;
            /// <summary>Reserved</summary>
            public ushort reserved;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 455)]
            public byte[] boot_code;
            /// <summary>Big endian word to make big endian sum of all sector words be equal to 0x1234 if disk is bootable.</summary>
            public ushort checksum;
        }

        /// <summary>
        /// BIOS Parameter Block as used by MSX-DOS 2.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MSXParameterBlock
        {
            /// <summary>x86 loop</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT (inclusive)</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public ushort hsectors;
            /// <summary>Jump for MSX-DOS 1 boot code</summary>
            public ushort msxdos_jmp;
            /// <summary>Set to "VOL_ID" by MSX-DOS 2</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] vol_id;
            /// <summary>Bigger than 0 if there are deleted files (MSX-DOS 2)</summary>
            public byte undelete_flag;
            /// <summary>Volume serial number (MSX-DOS 2)<summary>
            public uint serial_no;
            /// <summary>Reserved (MSX-DOS 2)<summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] reserved;
            /// <summary>Jump for MSX-DOS 2 boot code (MSX-DOS 2)</summary>
            public ushort msxdos2_jmp;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 460)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 2.0 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BIOSParameterBlock2
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 486)]
            public byte[] boot_code;
            /// <summary>0x55 0xAA if bootable.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.0 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BIOSParameterBlock30
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public ushort hsectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.2 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BIOSParameterBlock32
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public ushort hsectors;
            /// <summary>Total sectors including hidden ones</summary>
            public ushort total_sectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 478)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.31 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BIOSParameterBlock33
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume if > 65535</summary>
            public uint big_sectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 474)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 3.4 BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BIOSParameterBlockShortEBPB
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume if > 65535</summary>
            public uint big_sectors;
            /// <summary>Drive number<summary>
            public byte drive_no;
            /// <summary>Volume flags<summary>
            public byte flags;
            /// <summary>EPB signature, 0x28<summary>
            public byte signature;
            /// <summary>Volume serial number<summary>
            public uint serial_no;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 467)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>DOS 4.0 or higher BIOS Parameter Block.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BIOSParameterBlockEBPB
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume if > 65535</summary>
            public uint big_sectors;
            /// <summary>Drive number<summary>
            public byte drive_no;
            /// <summary>Volume flags<summary>
            public byte flags;
            /// <summary>EPB signature, 0x29<summary>
            public byte signature;
            /// <summary>Volume serial number<summary>
            public uint serial_no;
            /// <summary>Volume label, 11 bytes, space-padded
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] volume_label;
            /// <summary>Filesystem type, 8 bytes, space-padded
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>FAT32 Parameter Block</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FAT32ParameterBlockShort
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory, set to 0</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume, set to 0</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT, set to 0</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume</summary>
            public uint big_sectors;
            /// <summary>Sectors per FAT</summary>
            public uint big_spfat;
            /// <summary>FAT flags</summary>
            public ushort mirror_flags;
            /// <summary>FAT32 version</summary>
            public ushort version;
            /// <summary>Cluster of root directory</summary>
            public uint root_cluster;
            /// <summary>Sector of FSINFO structure</summary>
            public ushort fsinfo_sector;
            /// <summary>Sector of FAT32PB backup</summary>
            public ushort backup_sector;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Volume flags</summary>
            public byte flags;
            /// <summary>Signature, should be 0x28</summary>
            public byte signature;
            /// <summary>Volume serial number</summary>
            public uint serial_no;
            /// <summary>Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] reserved2;
            /// <summary>Sectors in volume if <see cref="big_sectors"/> equals 0</summary>
            public ulong huge_sectors;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 420)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        /// <summary>FAT32 Parameter Block</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FAT32ParameterBlock
        {
            /// <summary>x86 jump</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory, set to 0</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume, set to 0</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT, set to 0</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrk;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Sectors in volume</summary>
            public uint big_sectors;
            /// <summary>Sectors per FAT</summary>
            public uint big_spfat;
            /// <summary>FAT flags</summary>
            public ushort mirror_flags;
            /// <summary>FAT32 version</summary>
            public ushort version;
            /// <summary>Cluster of root directory</summary>
            public uint root_cluster;
            /// <summary>Sector of FSINFO structure</summary>
            public ushort fsinfo_sector;
            /// <summary>Sector of FAT32PB backup</summary>
            public ushort backup_sector;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Volume flags</summary>
            public byte flags;
            /// <summary>Signature, should be 0x29</summary>
            public byte signature;
            /// <summary>Volume serial number</summary>
            public uint serial_no;
            /// <summary>Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] volume_label;
            /// <summary>Filesystem type, 8 bytes, space-padded, must be "FAT32   "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 419)]
            public byte[] boot_code;
            /// <summary>Always 0x55 0xAA.</summary>
            public ushort boot_signature;
        }

        public const uint fsinfo_signature1 = 0x41615252;
        public const uint fsinfo_signature2 = 0x61417272;
        public const uint fsinfo_signature3 = 0xAA550000;

        /// <summary>FAT32 FS Information Sector</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FSInfoSector
        {
            /// <summary>Signature must be <see cref="fsinfo_signature1"/></summary>
            public uint signature1;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
            public byte[] reserved1;
            /// <summary>Signature must be <see cref="fsinfo_signature2"/></summary>
            public uint signature2;
            /// <summary>Free clusters</summary>
            public uint free_clusters;
            /// <summary>  cated cluster</summary>
            public uint last_cluster;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved2;
            /// <summary>Signature must be <see cref="fsinfo_signature3"/></summary>
            public uint signature3;
        }

        [Flags]
        public enum FatAttributes : byte
        {
            ReadOnly = 0x01,
            Hidden = 0x02,
            System = 0x04,
            VolumeLabel = 0x08,
            Subdirectory = 0x10,
            Archive = 0x20,
            Device = 0x40,
            Reserved = 0x80,
            LFN = 0x0F
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DirectoryEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] filename;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] extension;
            public FatAttributes attributes;
            public byte caseinfo;
            public byte ctime_ms;
            public ushort ctime;
            public ushort cdate;
            public ushort adate;
            public ushort ea_handle;
            public ushort mtime;
            public ushort mdate;
            public ushort start_cluster;
            public uint size;
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