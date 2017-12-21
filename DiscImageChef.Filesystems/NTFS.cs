// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NTFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft NT File System and shows information.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from Inside Windows NT
    public class NTFS : Filesystem
    {
        public NTFS()
        {
            Name = "New Technology File System (NTFS)";
            PluginUUID = new Guid("33513B2C-1e6d-4d21-a660-0bbc789c3871");
            CurrentEncoding = Encoding.Unicode;
        }

        public NTFS(Encoding encoding)
        {
            Name = "New Technology File System (NTFS)";
            PluginUUID = new Guid("33513B2C-1e6d-4d21-a660-0bbc789c3871");
            CurrentEncoding = Encoding.Unicode;
        }

        public NTFS(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "New Technology File System (NTFS)";
            PluginUUID = new Guid("33513B2C-1e6d-4d21-a660-0bbc789c3871");
            CurrentEncoding = Encoding.Unicode;
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            byte[] eigth_bytes = new byte[8];
            byte fats_no;
            ushort spfat, signature;
            string oem_name;

            byte[] ntfs_bpb = imagePlugin.ReadSector(0 + partition.Start);

            Array.Copy(ntfs_bpb, 0x003, eigth_bytes, 0, 8);
            oem_name = StringHandlers.CToString(eigth_bytes);

            if(oem_name != "NTFS    ") return false;

            fats_no = ntfs_bpb[0x010];

            if(fats_no != 0) return false;

            spfat = BitConverter.ToUInt16(ntfs_bpb, 0x016);

            if(spfat != 0) return false;

            signature = BitConverter.ToUInt16(ntfs_bpb, 0x1FE);

            return signature == 0xAA55;
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] ntfs_bpb = imagePlugin.ReadSector(0 + partition.Start);

            NTFS_BootBlock ntfs_bb;
            IntPtr bpbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(ntfs_bpb, 0, bpbPtr, 512);
            ntfs_bb = (NTFS_BootBlock)Marshal.PtrToStructure(bpbPtr, typeof(NTFS_BootBlock));
            Marshal.FreeHGlobal(bpbPtr);

            sb.AppendFormat("{0} bytes per sector", ntfs_bb.bps).AppendLine();
            sb.AppendFormat("{0} sectors per cluster ({1} bytes)", ntfs_bb.spc, ntfs_bb.spc * ntfs_bb.bps).AppendLine();
            //          sb.AppendFormat("{0} reserved sectors", ntfs_bb.rsectors).AppendLine();
            //          sb.AppendFormat("{0} FATs", ntfs_bb.fats_no).AppendLine();
            //          sb.AppendFormat("{0} entries in the root folder", ntfs_bb.root_ent).AppendLine();
            //          sb.AppendFormat("{0} sectors on volume (small)", ntfs_bb.sml_sectors).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", ntfs_bb.media).AppendLine();
            //          sb.AppendFormat("{0} sectors per FAT", ntfs_bb.spfat).AppendLine();
            sb.AppendFormat("{0} sectors per track", ntfs_bb.sptrk).AppendLine();
            sb.AppendFormat("{0} heads", ntfs_bb.heads).AppendLine();
            sb.AppendFormat("{0} hidden sectors before filesystem", ntfs_bb.hsectors).AppendLine();
            //          sb.AppendFormat("{0} sectors on volume (big)", ntfs_bb.big_sectors).AppendLine();
            sb.AppendFormat("BIOS drive number: 0x{0:X2}", ntfs_bb.drive_no).AppendLine();
            //          sb.AppendFormat("NT flags: 0x{0:X2}", ntfs_bb.nt_flags).AppendLine();
            //          sb.AppendFormat("Signature 1: 0x{0:X2}", ntfs_bb.signature1).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", ntfs_bb.sectors, ntfs_bb.sectors * ntfs_bb.bps)
              .AppendLine();
            sb.AppendFormat("Cluster where $MFT starts: {0}", ntfs_bb.mft_lsn).AppendLine();
            sb.AppendFormat("Cluster where $MFTMirr starts: {0}", ntfs_bb.mftmirror_lsn).AppendLine();

            if(ntfs_bb.mft_rc_clusters > 0)
                sb.AppendFormat("{0} clusters per MFT record ({1} bytes)", ntfs_bb.mft_rc_clusters,
                                ntfs_bb.mft_rc_clusters * ntfs_bb.bps * ntfs_bb.spc).AppendLine();
            else sb.AppendFormat("{0} bytes per MFT record", 1 << -ntfs_bb.mft_rc_clusters).AppendLine();
            if(ntfs_bb.index_blk_cts > 0)
                sb.AppendFormat("{0} clusters per Index block ({1} bytes)", ntfs_bb.index_blk_cts,
                                ntfs_bb.index_blk_cts * ntfs_bb.bps * ntfs_bb.spc).AppendLine();
            else sb.AppendFormat("{0} bytes per Index block", 1 << -ntfs_bb.index_blk_cts).AppendLine();

            sb.AppendFormat("Volume serial number: {0:X16}", ntfs_bb.serial_no).AppendLine();
            //          sb.AppendFormat("Signature 2: 0x{0:X4}", ntfs_bb.signature2).AppendLine();

            xmlFSType = new FileSystemType();

            if(ntfs_bb.jump[0] == 0xEB && ntfs_bb.jump[1] > 0x4E && ntfs_bb.jump[1] < 0x80 &&
               ntfs_bb.signature2 == 0xAA55)
            {
                xmlFSType.Bootable = true;
                Sha1Context sha1Ctx = new Sha1Context();
                sha1Ctx.Init();
                string bootChk = sha1Ctx.Data(ntfs_bb.boot_code, out byte[] sha1_out);
                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
            }

            xmlFSType.ClusterSize = ntfs_bb.spc * ntfs_bb.bps;
            xmlFSType.Clusters = ntfs_bb.sectors / ntfs_bb.spc;
            xmlFSType.VolumeSerial = $"{ntfs_bb.serial_no:X16}";
            xmlFSType.Type = "NTFS";

            information = sb.ToString();
        }

        /// <summary>
        /// NTFS $BOOT
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NTFS_BootBlock
        {
            // Start of BIOS Parameter Block
            /// <summary>0x000, Jump to boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            /// <summary>0x003, OEM Name, 8 bytes, space-padded, must be "NTFS    "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] oem_name;
            /// <summary>0x00B, Bytes per sector</summary>
            public ushort bps;
            /// <summary>0x00D, Sectors per cluster</summary>
            public byte spc;
            /// <summary>0x00E, Reserved sectors, seems 0</summary>
            public ushort rsectors;
            /// <summary>0x010, Number of FATs... obviously, 0</summary>
            public byte fats_no;
            /// <summary>0x011, Number of entries on root directory... 0</summary>
            public ushort root_ent;
            /// <summary>0x013, Sectors in volume... 0</summary>
            public ushort sml_sectors;
            /// <summary>0x015, Media descriptor</summary>
            public byte media;
            /// <summary>0x016, Sectors per FAT... 0</summary>
            public ushort spfat;
            /// <summary>0x018, Sectors per track, required to boot</summary>
            public ushort sptrk;
            /// <summary>0x01A, Heads... required to boot</summary>
            public ushort heads;
            /// <summary>0x01C, Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>0x020, Sectors in volume if &gt; 65535... 0</summary>
            public uint big_sectors;
            /// <summary>0x024, Drive number</summary>
            public byte drive_no;
            /// <summary>0x025, 0</summary>
            public byte nt_flags;
            /// <summary>0x026, EPB signature, 0x80</summary>
            public byte signature1;
            /// <summary>0x027, Alignment</summary>
            public byte dummy;
            // End of BIOS Parameter Block

            // Start of NTFS real superblock
            /// <summary>0x028, Sectors on volume</summary>
            public long sectors;
            /// <summary>0x030, LSN of $MFT</summary>
            public long mft_lsn;
            /// <summary>0x038, LSN of $MFTMirror</summary>
            public long mftmirror_lsn;
            /// <summary>0x040, Clusters per MFT record</summary>
            public sbyte mft_rc_clusters;
            /// <summary>0x041, Alignment</summary>
            public byte dummy2;
            /// <summary>0x042, Alignment</summary>
            public ushort dummy3;
            /// <summary>0x044, Clusters per index block</summary>
            public sbyte index_blk_cts;
            /// <summary>0x045, Alignment</summary>
            public byte dummy4;
            /// <summary>0x046, Alignment</summary>
            public ushort dummy5;
            /// <summary>0x048, Volume serial number</summary>
            public ulong serial_no;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 430)] public byte[] boot_code;
            /// <summary>0x1FE, 0xAA55</summary>
            public ushort signature2;
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