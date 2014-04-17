/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : FAT.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies FAT12/16/32 filesystems and shows information.
 
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

// TODO: Implement detecting DOS bootable disks
// TODO: Implement detecting Atari TOS bootable disks and printing corresponding fields
namespace FileSystemIDandChk.Plugins
{
    class FAT : Plugin
    {
        public FAT(PluginBase Core)
        {
            Name = "Microsoft File Allocation Table";
            PluginUUID = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            byte media_descriptor; // Not present on DOS <= 3, present on TOS but != of first FAT entry
            byte fats_no; // Must be 1 or 2. Dunno if it can be 0 in the wild, but it CANNOT BE bigger than 2
            byte[] fat32_signature = new byte[8]; // "FAT32   "
            UInt32 first_fat_entry; // No matter FAT size we read 4 bytes for checking
            UInt16 bps, rsectors;

            byte[] bpb_sector = imagePlugin.ReadSector(0 + partitionOffset);
            byte[] fat_sector;

            fats_no = bpb_sector[0x010]; // FATs, 1 or 2, maybe 0, never bigger
            media_descriptor = bpb_sector[0x015]; // Media Descriptor if present is in 0x15
            Array.Copy(bpb_sector, 0x52, fat32_signature, 0, 8); // FAT32 signature, if present, is in 0x52
            bps = BitConverter.ToUInt16(bpb_sector, 0x00B); // Bytes per sector
            if (bps == 0)
                bps = 0x200;
            rsectors = BitConverter.ToUInt16(bpb_sector, 0x00E); // Sectors between BPB and FAT, including the BPB sector => [BPB,FAT) 
            if (rsectors == 0)
                rsectors = 1;
            if (imagePlugin.GetSectors() > ((ulong)rsectors + partitionOffset))
                fat_sector = imagePlugin.ReadSector(rsectors + partitionOffset); // First FAT entry
			else
                return false;
            first_fat_entry = BitConverter.ToUInt32(fat_sector, 0); // Easier to manage

            if (MainClass.isDebug)
            {
                Console.WriteLine("FAT: fats_no = {0}", fats_no);
                Console.WriteLine("FAT: media_descriptor = 0x{0:X2}", media_descriptor);
                Console.WriteLine("FAT: fat32_signature = {0}", StringHandlers.CToString(fat32_signature));
                Console.WriteLine("FAT: bps = {0}", bps);
                Console.WriteLine("FAT: first_fat_entry = 0x{0:X8}", first_fat_entry);
            }

            if (fats_no > 2) // Must be 1 or 2, but as TOS makes strange things and I have not checked if it puts this to 0, ignore if 0. MUST NOT BE BIGGER THAN 2!
				return false;

            // Let's start the fun
            if (Encoding.ASCII.GetString(fat32_signature) == "FAT32   ")
                return true; // Seems easy, check reading
			
            if ((first_fat_entry & 0xFFFFFFF0) == 0xFFFFFFF0) // Seems to be FAT16
            {
                if ((first_fat_entry & 0xFF) == media_descriptor)
                    return true; // It MUST be FAT16, or... maybe not :S
            }
            else if ((first_fat_entry & 0x00FFFFF0) == 0x00FFFFF0)
            {
                //if((first_fat_entry & 0xFF) == media_descriptor) // Pre DOS<4 does not implement this, TOS does and is !=
                return true; // It MUST be FAT12, or... maybe not :S
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();

            byte[] dosString; // Space-padded
            bool isFAT32 = false;
            UInt32 first_fat_entry;
            byte media_descriptor, fats_no;
            string fat32_signature;
            UInt16 bps, rsectors;

            byte[] bpb_sector = imagePlugin.ReadSector(0 + partitionOffset);
            byte[] fat_sector;
			
            fats_no = bpb_sector[0x010]; // FATs, 1 or 2, maybe 0, never bigger
            media_descriptor = bpb_sector[0x015]; // Media Descriptor if present is in 0x15
            dosString = new byte[8];
            Array.Copy(bpb_sector, 0x52, dosString, 0, 8); // FAT32 signature, if present, is in 0x52
            fat32_signature = Encoding.ASCII.GetString(dosString);
            bps = BitConverter.ToUInt16(bpb_sector, 0x00B); // Bytes per sector
            if (bps == 0)
                bps = 0x200;
            rsectors = BitConverter.ToUInt16(bpb_sector, 0x00E); // Sectors between BPB and FAT, including the BPB sector => [BPB,FAT) 
            if (rsectors == 0)
                rsectors = 1;
            fat_sector = imagePlugin.ReadSector(rsectors + partitionOffset); // First FAT entry
            first_fat_entry = BitConverter.ToUInt32(fat_sector, 0); // Easier to manage

            if (fats_no > 2) // Must be 1 or 2, but as TOS makes strange things and I have not checked if it puts this to 0, ignore if 0. MUST NOT BE BIGGER THAN 2!
				return;

            // Let's start the fun
            if (fat32_signature == "FAT32   ")
            {
                sb.AppendLine("Microsoft FAT32"); // Seems easy, check reading
                isFAT32 = true;
            }
            else if ((first_fat_entry & 0xFFFFFFF0) == 0xFFFFFFF0) // Seems to be FAT16
            {
                if ((first_fat_entry & 0xFF) == media_descriptor)
                    sb.AppendLine("Microsoft FAT16"); // It MUST be FAT16, or... maybe not :S
            }
            else if ((first_fat_entry & 0x00FFFFF0) == 0x00FFFFF0)
            {
                //if((first_fat_entry & 0xFF) == media_descriptor) // Pre DOS<4 does not implement this, TOS does and is !=
                sb.AppendLine("Microsoft FAT12"); // It MUST be FAT12, or... maybe not :S
            }
            else
                return;
			
            BIOSParameterBlock BPB = new BIOSParameterBlock();
            ExtendedParameterBlock EPB = new ExtendedParameterBlock();
            FAT32ParameterBlock FAT32PB = new FAT32ParameterBlock();
			
            dosString = new byte[8];
            Array.Copy(bpb_sector, 0x03, dosString, 0, 8);
            BPB.OEMName = Encoding.ASCII.GetString(dosString);
            BPB.bps = BitConverter.ToUInt16(bpb_sector, 0x0B);
            BPB.spc = bpb_sector[0x0D];
            BPB.rsectors = BitConverter.ToUInt16(bpb_sector, 0x0E);
            BPB.fats_no = bpb_sector[0x10];
            BPB.root_ent = BitConverter.ToUInt16(bpb_sector, 0x11);
            BPB.sectors = BitConverter.ToUInt16(bpb_sector, 0x13);
            BPB.media = bpb_sector[0x15];
            BPB.spfat = BitConverter.ToUInt16(bpb_sector, 0x16);
            BPB.sptrk = BitConverter.ToUInt16(bpb_sector, 0x18);
            BPB.heads = BitConverter.ToUInt16(bpb_sector, 0x1A);
            BPB.hsectors = BitConverter.ToUInt32(bpb_sector, 0x1C);
            BPB.big_sectors = BitConverter.ToUInt32(bpb_sector, 0x20);
			
            if (isFAT32)
            {
                FAT32PB.spfat = BitConverter.ToUInt32(bpb_sector, 0x24);
                FAT32PB.fat_flags = BitConverter.ToUInt16(bpb_sector, 0x28);
                FAT32PB.version = BitConverter.ToUInt16(bpb_sector, 0x2A);
                FAT32PB.root_cluster = BitConverter.ToUInt32(bpb_sector, 0x2C);
                FAT32PB.fsinfo_sector = BitConverter.ToUInt16(bpb_sector, 0x30);
                FAT32PB.backup_sector = BitConverter.ToUInt16(bpb_sector, 0x32);
                FAT32PB.drive_no = bpb_sector[0x40];
                FAT32PB.nt_flags = bpb_sector[0x41];
                FAT32PB.signature = bpb_sector[0x42];
                FAT32PB.serial_no = BitConverter.ToUInt32(bpb_sector, 0x43);
                dosString = new byte[11];
                Array.Copy(bpb_sector, 0x47, dosString, 0, 11);
                FAT32PB.volume_label = Encoding.ASCII.GetString(dosString);
                dosString = new byte[8];
                Array.Copy(bpb_sector, 0x52, dosString, 0, 8);
                FAT32PB.fs_type = Encoding.ASCII.GetString(dosString);
            }
            else
            {
                EPB.drive_no = bpb_sector[0x24];
                EPB.nt_flags = bpb_sector[0x25];
                EPB.signature = bpb_sector[0x26];
                EPB.serial_no = BitConverter.ToUInt32(bpb_sector, 0x27);
                dosString = new byte[11];
                Array.Copy(bpb_sector, 0x2B, dosString, 0, 11);
                EPB.volume_label = Encoding.ASCII.GetString(dosString);
                dosString = new byte[8];
                Array.Copy(bpb_sector, 0x36, dosString, 0, 8);
                EPB.fs_type = Encoding.ASCII.GetString(dosString);
            }
			
            sb.AppendFormat("OEM Name: {0}", BPB.OEMName).AppendLine();
            sb.AppendFormat("{0} bytes per sector.", BPB.bps).AppendLine();
            sb.AppendFormat("{0} sectors per cluster.", BPB.spc).AppendLine();
            sb.AppendFormat("{0} sectors reserved between BPB and FAT.", BPB.rsectors).AppendLine();
            sb.AppendFormat("{0} FATs.", BPB.fats_no).AppendLine();
            sb.AppendFormat("{0} entires on root directory.", BPB.root_ent).AppendLine();
            if (BPB.sectors == 0)
                sb.AppendFormat("{0} sectors on volume ({1} bytes).", BPB.big_sectors, BPB.big_sectors * BPB.bps).AppendLine();
            else
                sb.AppendFormat("{0} sectors on volume ({1} bytes).", BPB.sectors, BPB.sectors * BPB.bps).AppendLine();
            if ((BPB.media & 0xF0) == 0xF0)
                sb.AppendFormat("Media format: 0x{0:X2}", BPB.media).AppendLine();
            if (fat32_signature == "FAT32   ")
                sb.AppendFormat("{0} sectors per FAT.", FAT32PB.spfat).AppendLine();
            else
                sb.AppendFormat("{0} sectors per FAT.", BPB.spfat).AppendLine();
            sb.AppendFormat("{0} sectors per track.", BPB.sptrk).AppendLine();
            sb.AppendFormat("{0} heads.", BPB.heads).AppendLine();
            sb.AppendFormat("{0} hidden sectors before BPB.", BPB.hsectors).AppendLine();
			
            if (isFAT32)
            {
                sb.AppendFormat("Cluster of root directory: {0}", FAT32PB.root_cluster).AppendLine();
                sb.AppendFormat("Sector of FSINFO structure: {0}", FAT32PB.fsinfo_sector).AppendLine();
                sb.AppendFormat("Sector of backup FAT32 parameter block: {0}", FAT32PB.backup_sector).AppendLine();
                sb.AppendFormat("Drive number: 0x{0:X2}", FAT32PB.drive_no).AppendLine();
                sb.AppendFormat("Volume Serial Number: 0x{0:X8}", FAT32PB.serial_no).AppendLine();
                if ((FAT32PB.nt_flags & 0x01) == 0x01)
                {
                    sb.AppendLine("Volume should be checked on next mount.");	
                    if ((EPB.nt_flags & 0x02) == 0x02)
                        sb.AppendLine("Disk surface should be checked also.");	
                }
					
                sb.AppendFormat("Volume label: {0}", EPB.volume_label).AppendLine();
                sb.AppendFormat("Filesystem type: {0}", EPB.fs_type).AppendLine();
            }
            else if (EPB.signature == 0x28 || EPB.signature == 0x29)
            {
                sb.AppendFormat("Drive number: 0x{0:X2}", EPB.drive_no).AppendLine();
                sb.AppendFormat("Volume Serial Number: 0x{0:X8}", EPB.serial_no).AppendLine();
                if (EPB.signature == 0x29)
                {
                    if ((EPB.nt_flags & 0x01) == 0x01)
                    {
                        sb.AppendLine("Volume should be checked on next mount.");	
                        if ((EPB.nt_flags & 0x02) == 0x02)
                            sb.AppendLine("Disk surface should be checked also.");	
                    }
					
                    sb.AppendFormat("Volume label: {0}", EPB.volume_label).AppendLine();
                    sb.AppendFormat("Filesystem type: {0}", EPB.fs_type).AppendLine();
                }
            }
			
            information = sb.ToString();
        }

        public struct BIOSParameterBlock
        {
            public string OEMName;
            // 0x03, OEM Name, 8 bytes, space-padded
            public UInt16 bps;
            // 0x0B, Bytes per sector
            public byte spc;
            // 0x0D, Sectors per cluster
            public UInt16 rsectors;
            // 0x0E, Reserved sectors between BPB and FAT
            public byte fats_no;
            // 0x10, Number of FATs
            public UInt16 root_ent;
            // 0x11, Number of entries on root directory
            public UInt16 sectors;
            // 0x13, Sectors in volume
            public byte media;
            // 0x15, Media descriptor
            public UInt16 spfat;
            // 0x16, Sectors per FAT
            public UInt16 sptrk;
            // 0x18, Sectors per track
            public UInt16 heads;
            // 0x1A, Heads
            public UInt32 hsectors;
            // 0x1C, Hidden sectors before BPB
            public UInt32 big_sectors;
            // 0x20, Sectors in volume if > 65535
        }
        // This only applies for bootable disks
        // From http://info-coach.fr/atari/software/FD-Soft.php
        public struct AtariBootBlock
        {
            public UInt16 hsectors;
            // 0x01C, Atari ST use 16 bit for hidden sectors, probably so did old DOS
            public UInt16 xflag;
            // 0x01E, indicates if COMMAND.PRG must be executed after OS load
            public UInt16 ldmode;
            // 0x020, load mode for, or 0 if fname indicates boot file
            public UInt16 bsect;
            // 0x022, sector from which to boot
            public UInt16 bsects_no;
            // 0x024, how many sectors to boot
            public UInt32 ldaddr;
            // 0x026, RAM address where boot should be located
            public UInt32 fatbuf;
            // 0x02A, RAM address to copy the FAT and root directory
            public string fname;
            // 0x02E, 11 bytes, name of boot file
            public UInt16 reserved;
            // 0x039, unused
            public byte[] boot_code;
            // 0x03B, 451 bytes boot code
            public UInt16 checksum;
            // 0x1FE, the sum of all the BPB+ABB must be 0x1234, so this bigendian value works as adjustment
        }

        public struct ExtendedParameterBlock
        {
            public byte drive_no;
            // 0x24, Drive number
            public byte nt_flags;
            // 0x25, Volume flags if NT (must be 0x29 signature)
            public byte signature;
            // 0x26, EPB signature, 0x28 or 0x29
            public UInt32 serial_no;
            // 0x27, Volume serial number
            /* Present only if signature == 0x29 */
            public string volume_label;
            // 0x2B, Volume label, 11 bytes, space-padded
            public string fs_type;
            // 0x36, Filesystem type, 8 bytes, space-padded
        }

        public struct FAT32ParameterBlock
        {
            public UInt32 spfat;
            // 0x24, Sectors per FAT
            public UInt16 fat_flags;
            // 0x28, FAT flags
            public UInt16 version;
            // 0x2A, FAT32 version
            public UInt32 root_cluster;
            // 0x2C, Cluster of root directory
            public UInt16 fsinfo_sector;
            // 0x30, Sector of FSINFO structure
            public UInt16 backup_sector;
            // 0x32, Sector of FAT32PB bacup
            byte[] reserved;
            // 0x34, 12 reserved bytes
            public byte drive_no;
            // 0x40, Drive number
            public byte nt_flags;
            // 0x41, Volume flags
            public byte signature;
            // 0x42, FAT32PB signature, should be 0x29
            public UInt32 serial_no;
            // 0x43, Volume serial number
            public string volume_label;
            // 0x47, Volume label, 11 bytes, space-padded
            public string fs_type;
            // 0x52, Filesystem type, 8 bytes, space-padded, must be "FAT32   "
        }
    }
}