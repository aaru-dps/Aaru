// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : OS/2 High Performance File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the OS/2 High Performance File System and shows information.
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
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    // Information from an old unnamed document
    public class HPFS : Filesystem
    {
        public HPFS()
        {
            Name = "OS/2 High Performance File System";
            PluginUUID = new Guid("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");
            CurrentEncoding = Encoding.GetEncoding("ibm850");
        }

        public HPFS(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "OS/2 High Performance File System";
            PluginUUID = new Guid("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("ibm850");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if((16 + partition.Start) >= partition.End)
                return false;

            uint magic1, magic2;

            byte[] hpfs_sb_sector = imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16
            magic1 = BitConverter.ToUInt32(hpfs_sb_sector, 0x000);
            magic2 = BitConverter.ToUInt32(hpfs_sb_sector, 0x004);

            if(magic1 == 0xF995E849 && magic2 == 0xFA53E9C5)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            HPFS_BIOSParameterBlock hpfs_bpb = new HPFS_BIOSParameterBlock();
            HPFS_SuperBlock hpfs_sb = new HPFS_SuperBlock();
            HPFS_SpareBlock hpfs_sp = new HPFS_SpareBlock();

            byte[] oem_name = new byte[8];
            byte[] volume_name = new byte[11];

            byte[] hpfs_bpb_sector = imagePlugin.ReadSector(0 + partition.Start); // Seek to BIOS parameter block, on logical sector 0
            byte[] hpfs_sb_sector = imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16
            byte[] hpfs_sp_sector = imagePlugin.ReadSector(17 + partition.Start); // Seek to spareblock, on logical sector 17

            IntPtr bpbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(hpfs_bpb_sector, 0, bpbPtr, 512);
            hpfs_bpb = (HPFS_BIOSParameterBlock)Marshal.PtrToStructure(bpbPtr, typeof(HPFS_BIOSParameterBlock));
            Marshal.FreeHGlobal(bpbPtr);

            IntPtr sbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(hpfs_sb_sector, 0, sbPtr, 512);
            hpfs_sb = (HPFS_SuperBlock)Marshal.PtrToStructure(sbPtr, typeof(HPFS_SuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            IntPtr spPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(hpfs_sp_sector, 0, spPtr, 512);
            hpfs_sp = (HPFS_SpareBlock)Marshal.PtrToStructure(spPtr, typeof(HPFS_SpareBlock));
            Marshal.FreeHGlobal(spPtr);

            if(StringHandlers.CToString(hpfs_bpb.fs_type) != "HPFS    " ||
            hpfs_sb.magic1 != 0xF995E849 || hpfs_sb.magic2 != 0xFA53E9C5 ||
            hpfs_sp.magic1 != 0xF9911849 || hpfs_sp.magic2 != 0xFA5229C5)
            {
                sb.AppendLine("This may not be HPFS, following information may be not correct.");
                sb.AppendFormat("File system type: \"{0}\" (Should be \"HPFS    \")", hpfs_bpb.fs_type).AppendLine();
                sb.AppendFormat("Superblock magic1: 0x{0:X8} (Should be 0xF995E849)", hpfs_sb.magic1).AppendLine();
                sb.AppendFormat("Superblock magic2: 0x{0:X8} (Should be 0xFA53E9C5)", hpfs_sb.magic2).AppendLine();
                sb.AppendFormat("Spareblock magic1: 0x{0:X8} (Should be 0xF9911849)", hpfs_sp.magic1).AppendLine();
                sb.AppendFormat("Spareblock magic2: 0x{0:X8} (Should be 0xFA5229C5)", hpfs_sp.magic2).AppendLine();
            }

            sb.AppendFormat("OEM name: {0}", StringHandlers.CToString(hpfs_bpb.oem_name)).AppendLine();
            sb.AppendFormat("{0} bytes per sector", hpfs_bpb.bps).AppendLine();
            //          sb.AppendFormat("{0} sectors per cluster", hpfs_bpb.spc).AppendLine();
            //			sb.AppendFormat("{0} reserved sectors", hpfs_bpb.rsectors).AppendLine();
            //			sb.AppendFormat("{0} FATs", hpfs_bpb.fats_no).AppendLine();
            //			sb.AppendFormat("{0} entries on root directory", hpfs_bpb.root_ent).AppendLine();
            //			sb.AppendFormat("{0} mini sectors on volume", hpfs_bpb.sectors).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", hpfs_bpb.media).AppendLine();
            //			sb.AppendFormat("{0} sectors per FAT", hpfs_bpb.spfat).AppendLine();
            //			sb.AppendFormat("{0} sectors per track", hpfs_bpb.sptrk).AppendLine();
            //			sb.AppendFormat("{0} heads", hpfs_bpb.heads).AppendLine();
            sb.AppendFormat("{0} sectors hidden before BPB", hpfs_bpb.hsectors).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfs_sb.sectors, hpfs_sb.sectors * hpfs_bpb.bps).AppendLine();
            //          sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfs_bpb.big_sectors, hpfs_bpb.big_sectors * hpfs_bpb.bps).AppendLine();
            sb.AppendFormat("BIOS Drive Number: 0x{0:X2}", hpfs_bpb.drive_no).AppendLine();
			sb.AppendFormat("NT Flags: 0x{0:X2}", hpfs_bpb.nt_flags).AppendLine();
            sb.AppendFormat("Signature: 0x{0:X2}", hpfs_bpb.signature).AppendLine();
            sb.AppendFormat("Serial number: 0x{0:X8}", hpfs_bpb.serial_no).AppendLine();
            sb.AppendFormat("Volume label: {0}", StringHandlers.CToString(hpfs_bpb.volume_label, CurrentEncoding)).AppendLine();
            //			sb.AppendFormat("Filesystem type: \"{0}\"", hpfs_bpb.fs_type).AppendLine();

            DateTime last_chk = DateHandlers.UNIXToDateTime(hpfs_sb.last_chkdsk);
            DateTime last_optim = DateHandlers.UNIXToDateTime(hpfs_sb.last_optim);

            sb.AppendFormat("HPFS version: {0}", hpfs_sb.version).AppendLine();
            sb.AppendFormat("Functional version: {0}", hpfs_sb.func_version).AppendLine();
            sb.AppendFormat("Sector of root directory FNode: {0}", hpfs_sb.root_fnode).AppendLine();
            sb.AppendFormat("{0} sectors are marked bad", hpfs_sb.badblocks).AppendLine();
            sb.AppendFormat("Sector of free space bitmaps: {0}", hpfs_sb.bitmap_lsn).AppendLine();
            sb.AppendFormat("Sector of bad blocks list: {0}", hpfs_sb.badblock_lsn).AppendLine();
            if(hpfs_sb.last_chkdsk > 0)
                sb.AppendFormat("Date of last integrity check: {0}", last_chk).AppendLine();
            else
                sb.AppendLine("Filesystem integrity has never been checked");
            if(hpfs_sb.last_optim > 0)
                sb.AppendFormat("Date of last optimization {0}", last_optim).AppendLine();
            else
                sb.AppendLine("Filesystem has never been optimized");
            sb.AppendFormat("Directory band has {0} sectors", hpfs_sb.dband_sectors).AppendLine();
            sb.AppendFormat("Directory band starts at sector {0}", hpfs_sb.dband_start).AppendLine();
            sb.AppendFormat("Directory band ends at sector {0}", hpfs_sb.dband_last).AppendLine();
            sb.AppendFormat("Sector of directory band bitmap: {0}", hpfs_sb.dband_bitmap).AppendLine();
            sb.AppendFormat("Sector of ACL directory: {0}", hpfs_sb.acl_start).AppendLine();

            sb.AppendFormat("Sector of Hotfix directory: {0}", hpfs_sp.hotfix_start).AppendLine();
            sb.AppendFormat("{0} used Hotfix entries", hpfs_sp.hotfix_used).AppendLine();
            sb.AppendFormat("{0} total Hotfix entries", hpfs_sp.hotfix_entries).AppendLine();
            sb.AppendFormat("{0} free spare DNodes", hpfs_sp.spare_dnodes_free).AppendLine();
            sb.AppendFormat("{0} total spare DNodes", hpfs_sp.spare_dnodes).AppendLine();
            sb.AppendFormat("Sector of codepage directory: {0}", hpfs_sp.codepage_lsn).AppendLine();
            sb.AppendFormat("{0} codepages used in the volume", hpfs_sp.codepages).AppendLine();
            sb.AppendFormat("SuperBlock CRC32: {0:X8}", hpfs_sp.sb_crc32).AppendLine();
            sb.AppendFormat("SpareBlock CRC32: {0:X8}", hpfs_sp.sp_crc32).AppendLine();

            sb.AppendLine("Flags:");
            if((hpfs_sp.flags1 & 0x01) == 0x01)
                sb.AppendLine("Filesystem is dirty.");
            else
                sb.AppendLine("Filesystem is clean.");
            if((hpfs_sp.flags1 & 0x02) == 0x02)
                sb.AppendLine("Spare directory blocks are in use");
            if((hpfs_sp.flags1 & 0x04) == 0x04)
                sb.AppendLine("Hotfixes are in use");
            if((hpfs_sp.flags1 & 0x08) == 0x08)
                sb.AppendLine("Disk contains bad sectors");
            if((hpfs_sp.flags1 & 0x10) == 0x10)
                sb.AppendLine("Disk has a bad bitmap");
            if((hpfs_sp.flags1 & 0x20) == 0x20)
                sb.AppendLine("Filesystem was formatted fast");
            if((hpfs_sp.flags1 & 0x40) == 0x40)
                sb.AppendLine("Unknown flag 0x40 on flags1 is active");
            if((hpfs_sp.flags1 & 0x80) == 0x80)
                sb.AppendLine("Filesystem has been mounted by an old IFS");
            if((hpfs_sp.flags2 & 0x01) == 0x01)
                sb.AppendLine("Install DASD limits");
            if((hpfs_sp.flags2 & 0x02) == 0x02)
                sb.AppendLine("Resync DASD limits");
            if((hpfs_sp.flags2 & 0x04) == 0x04)
                sb.AppendLine("DASD limits are operational");
            if((hpfs_sp.flags2 & 0x08) == 0x08)
                sb.AppendLine("Multimedia is active");
            if((hpfs_sp.flags2 & 0x10) == 0x10)
                sb.AppendLine("DCE ACLs are active");
            if((hpfs_sp.flags2 & 0x20) == 0x20)
                sb.AppendLine("DASD limits are dirty");
            if((hpfs_sp.flags2 & 0x40) == 0x40)
                sb.AppendLine("Unknown flag 0x40 on flags2 is active");
            if((hpfs_sp.flags2 & 0x80) == 0x80)
                sb.AppendLine("Unknown flag 0x80 on flags2 is active");

            xmlFSType = new Schemas.FileSystemType();

            // Theoretically everything from BPB to SB is boot code, should I hash everything or only the sector loaded by BIOS itself?
            if(hpfs_bpb.jump[0] == 0xEB && hpfs_bpb.jump[1] > 0x3C && hpfs_bpb.jump[1] < 0x80 && hpfs_bpb.signature2 == 0xAA55)
            {
                xmlFSType.Bootable = true;
                SHA1Context sha1Ctx = new SHA1Context();
                sha1Ctx.Init();
                string bootChk = sha1Ctx.Data(hpfs_bpb.boot_code, out byte[] sha1_out);
                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
            }

            xmlFSType.Dirty |= (hpfs_sp.flags1 & 0x01) == 0x01;
            xmlFSType.Clusters = hpfs_sb.sectors;
            xmlFSType.ClusterSize = hpfs_bpb.bps;
            xmlFSType.Type = "HPFS";
            xmlFSType.VolumeName = StringHandlers.CToString(hpfs_bpb.volume_label, CurrentEncoding);
            xmlFSType.VolumeSerial = string.Format("{0:X8}", hpfs_bpb.serial_no);
            xmlFSType.SystemIdentifier = StringHandlers.CToString(hpfs_bpb.oem_name);

            information = sb.ToString();
        }

        /// <summary>
        /// BIOS Parameter Block, at sector 0
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HPFS_BIOSParameterBlock
        {
            /// <summary>0x000, Jump to boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>0x003, OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>0x00B, Bytes per sector</summary>
            public ushort bps;
            /// <summary>0x00D, Sectors per cluster</summary>
            public byte spc;
            /// <summary>0x00E, Reserved sectors between BPB and... does it have sense in HPFS?</summary>
            public ushort rsectors;
            /// <summary>0x010, Number of FATs... seriously?</summary>
            public byte fats_no;
            /// <summary>0x011, Number of entries on root directory... ok</summary>
            public ushort root_ent;
            /// <summary>0x013, Sectors in volume... doubt it</summary>
            public ushort sectors;
            /// <summary>0x015, Media descriptor</summary>
            public byte media;
            /// <summary>0x016, Sectors per FAT... again</summary>
            public ushort spfat;
            /// <summary>0x018, Sectors per track... you're kidding</summary>
            public ushort sptrk;
            /// <summary>0x01A, Heads... stop!</summary>
            public ushort heads;
            /// <summary>0x01C, Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>0x024, Sectors in volume if &gt; 65535...</summary>
            public uint big_sectors;
            /// <summary>0x028, Drive number</summary>
            public byte drive_no;
            /// <summary>0x029, Volume flags?</summary>
            public byte nt_flags;
            /// <summary>0x02A, EPB signature, 0x29</summary>
            public byte signature;
            /// <summary>0x02B, Volume serial number</summary>
            public uint serial_no;
            /// <summary>0x02F, Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] volume_label;
            /// <summary>0x03A, Filesystem type, 8 bytes, space-padded ("HPFS    ")</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)]
            public byte[] boot_code;
            /// <summary>0x1FE, 0xAA55</summary>
            public ushort signature2;
        }

        /// <summary>
        /// HPFS superblock at sector 16
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HPFS_SuperBlock
        {
            /// <summary>0x000, 0xF995E849</summary>
            public uint magic1;
            /// <summary>0x004, 0xFA53E9C5</summary>
            public uint magic2;
            /// <summary>0x008, HPFS version</summary>
            public byte version;
            /// <summary>0x009, 2 if &lt;= 4 GiB, 3 if &gt; 4 GiB</summary>
            public byte func_version;
            /// <summary>0x00A, Alignment</summary>
            public ushort dummy;
            /// <summary>0x00C, LSN pointer to root fnode</summary>
            public uint root_fnode;
            /// <summary>0x010, Sectors on volume</summary>
            public uint sectors;
            /// <summary>0x014, Bad blocks on volume</summary>
            public uint badblocks;
            /// <summary>0x018, LSN pointer to volume bitmap</summary>
            public uint bitmap_lsn;
            /// <summary>0x01C, 0</summary>
            public uint zero1;
            /// <summary>0x020, LSN pointer to badblock directory</summary>
            public uint badblock_lsn;
            /// <summary>0x024, 0</summary>
            public uint zero2;
            /// <summary>0x028, Time of last CHKDSK</summary>
            public int last_chkdsk;
            /// <summary>0x02C, Time of last optimization</summary>
            public int last_optim;
            /// <summary>0x030, Sectors of dir band</summary>
            public uint dband_sectors;
            /// <summary>0x034, Start sector of dir band</summary>
            public uint dband_start;
            /// <summary>0x038, Last sector of dir band</summary>
            public uint dband_last;
            /// <summary>0x03C, LSN of free space bitmap</summary>
            public uint dband_bitmap;
            /// <summary>0x040, Can be used for volume name (32 bytes)</summary>
            public ulong zero3;
            /// <summary>0x048, ...</summary>
            public ulong zero4;
            /// <summary>0x04C, ...</summary>
            public ulong zero5;
            /// <summary>0x050, ...;</summary>
            public ulong zero6;
            /// <summary>0x058, LSN pointer to ACLs (only HPFS386)</summary>
            public uint acl_start;
        }

        /// <summary>
        /// HPFS spareblock at sector 17
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HPFS_SpareBlock
        {
            /// <summary>0x000, 0xF9911849</summary>
            public uint magic1;
            /// <summary>0x004, 0xFA5229C5</summary>
            public uint magic2;
            /// <summary>0x008, HPFS flags</summary>
            public byte flags1;
            /// <summary>0x009, HPFS386 flags</summary>
            public byte flags2;
            /// <summary>0x00A, Alignment</summary>
            public ushort dummy;
            /// <summary>0x00C, LSN of hotfix directory</summary>
            public uint hotfix_start;
            /// <summary>0x010, Used hotfixes</summary>
            public uint hotfix_used;
            /// <summary>0x014, Total hotfixes available</summary>
            public uint hotfix_entries;
            /// <summary>0x018, Unused spare dnodes</summary>
            public uint spare_dnodes_free;
            /// <summary>0x01C, Length of spare dnodes list</summary>
            public uint spare_dnodes;
            /// <summary>0x020, LSN of codepage directory</summary>
            public uint codepage_lsn;
            /// <summary>0x024, Number of codepages used</summary>
            public uint codepages;
            /// <summary>0x028, SuperBlock CRC32 (only HPFS386)</summary>
            public uint sb_crc32;
            /// <summary>0x02C, SpareBlock CRC32 (only HPFS386)</summary>
            public uint sp_crc32;
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
