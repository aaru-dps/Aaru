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
    public class NTFS : IFilesystem
    {
        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public virtual FileSystemType XmlFsType => xmlFsType;
        public virtual Encoding Encoding => currentEncoding;
        public virtual string Name => "New Technology File System (NTFS)";
        public virtual Guid Id => new Guid("33513B2C-1e6d-4d21-a660-0bbc789c3871");

        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            byte[] eigthBytes = new byte[8];
            byte fatsNo;
            ushort spFat, signature;

            byte[] ntfsBpb = imagePlugin.ReadSector(0 + partition.Start);

            Array.Copy(ntfsBpb, 0x003, eigthBytes, 0, 8);
            string oemName = StringHandlers.CToString(eigthBytes);

            if(oemName != "NTFS    ") return false;

            fatsNo = ntfsBpb[0x010];

            if(fatsNo != 0) return false;

            spFat = BitConverter.ToUInt16(ntfsBpb, 0x016);

            if(spFat != 0) return false;

            signature = BitConverter.ToUInt16(ntfsBpb, 0x1FE);

            return signature == 0xAA55;
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = Encoding.Unicode;
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] ntfsBpb = imagePlugin.ReadSector(0 + partition.Start);

            IntPtr bpbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(ntfsBpb, 0, bpbPtr, 512);
            NtfsBootBlock ntfsBb = (NtfsBootBlock)Marshal.PtrToStructure(bpbPtr, typeof(NtfsBootBlock));
            Marshal.FreeHGlobal(bpbPtr);

            sb.AppendFormat("{0} bytes per sector", ntfsBb.bps).AppendLine();
            sb.AppendFormat("{0} sectors per cluster ({1} bytes)", ntfsBb.spc, ntfsBb.spc * ntfsBb.bps).AppendLine();
            //          sb.AppendFormat("{0} reserved sectors", ntfs_bb.rsectors).AppendLine();
            //          sb.AppendFormat("{0} FATs", ntfs_bb.fats_no).AppendLine();
            //          sb.AppendFormat("{0} entries in the root folder", ntfs_bb.root_ent).AppendLine();
            //          sb.AppendFormat("{0} sectors on volume (small)", ntfs_bb.sml_sectors).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", ntfsBb.media).AppendLine();
            //          sb.AppendFormat("{0} sectors per FAT", ntfs_bb.spfat).AppendLine();
            sb.AppendFormat("{0} sectors per track", ntfsBb.sptrk).AppendLine();
            sb.AppendFormat("{0} heads", ntfsBb.heads).AppendLine();
            sb.AppendFormat("{0} hidden sectors before filesystem", ntfsBb.hsectors).AppendLine();
            //          sb.AppendFormat("{0} sectors on volume (big)", ntfs_bb.big_sectors).AppendLine();
            sb.AppendFormat("BIOS drive number: 0x{0:X2}", ntfsBb.drive_no).AppendLine();
            //          sb.AppendFormat("NT flags: 0x{0:X2}", ntfs_bb.nt_flags).AppendLine();
            //          sb.AppendFormat("Signature 1: 0x{0:X2}", ntfs_bb.signature1).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", ntfsBb.sectors, ntfsBb.sectors * ntfsBb.bps)
              .AppendLine();
            sb.AppendFormat("Cluster where $MFT starts: {0}", ntfsBb.mft_lsn).AppendLine();
            sb.AppendFormat("Cluster where $MFTMirr starts: {0}", ntfsBb.mftmirror_lsn).AppendLine();

            if(ntfsBb.mft_rc_clusters > 0)
                sb.AppendFormat("{0} clusters per MFT record ({1} bytes)", ntfsBb.mft_rc_clusters,
                                ntfsBb.mft_rc_clusters * ntfsBb.bps * ntfsBb.spc).AppendLine();
            else sb.AppendFormat("{0} bytes per MFT record", 1 << -ntfsBb.mft_rc_clusters).AppendLine();
            if(ntfsBb.index_blk_cts > 0)
                sb.AppendFormat("{0} clusters per Index block ({1} bytes)", ntfsBb.index_blk_cts,
                                ntfsBb.index_blk_cts * ntfsBb.bps * ntfsBb.spc).AppendLine();
            else sb.AppendFormat("{0} bytes per Index block", 1 << -ntfsBb.index_blk_cts).AppendLine();

            sb.AppendFormat("Volume serial number: {0:X16}", ntfsBb.serial_no).AppendLine();
            //          sb.AppendFormat("Signature 2: 0x{0:X4}", ntfs_bb.signature2).AppendLine();

            xmlFsType = new FileSystemType();

            if(ntfsBb.jump[0] == 0xEB && ntfsBb.jump[1] > 0x4E && ntfsBb.jump[1] < 0x80 && ntfsBb.signature2 == 0xAA55)
            {
                xmlFsType.Bootable = true;
                Sha1Context sha1Ctx = new Sha1Context();
                sha1Ctx.Init();
                string bootChk = sha1Ctx.Data(ntfsBb.boot_code, out _);
                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
            }

            xmlFsType.ClusterSize = ntfsBb.spc * ntfsBb.bps;
            xmlFsType.Clusters = ntfsBb.sectors / ntfsBb.spc;
            xmlFsType.VolumeSerial = $"{ntfsBb.serial_no:X16}";
            xmlFsType.Type = "NTFS";

            information = sb.ToString();
        }

        public virtual Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding, bool debug)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public virtual Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }

        /// <summary>
        ///     NTFS $BOOT
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NtfsBootBlock
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
    }
}