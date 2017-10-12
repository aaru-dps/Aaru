// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the BeOS filesystem and shows information.
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
using hammer_tid_t = System.UInt64;
using hammer_off_t = System.UInt64;
using hammer_crc_t = System.UInt32;

namespace DiscImageChef.Filesystems
{
    public class HAMMER : Filesystem
    {
        const ulong HAMMER_FSBUF_VOLUME = 0xC8414D4DC5523031;
        const ulong HAMMER_FSBUF_VOLUME_REV = 0x313052C54D4D41C8;
        const uint HAMMER_VOLHDR_SIZE = 1928;
        const int HAMMER_BIGBLOCK_SIZE = 8192 * 1024;

        public HAMMER()
        {
            Name = "HAMMER Filesystem";
            PluginUUID = new Guid("91A188BF-5FD7-4677-BBD3-F59EBA9C864D");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public HAMMER(Encoding encoding)
        {
            Name = "HAMMER Filesystem";
            PluginUUID = new Guid("91A188BF-5FD7-4677-BBD3-F59EBA9C864D");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public HAMMER(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "HAMMER Filesystem";
            PluginUUID = new Guid("91A188BF-5FD7-4677-BBD3-F59EBA9C864D");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            uint run = HAMMER_VOLHDR_SIZE / imagePlugin.GetSectorSize();

            if(HAMMER_VOLHDR_SIZE % imagePlugin.GetSectorSize() > 0)
                run++;

            if((run + partition.Start) >= partition.End)
                return false;

            ulong magic;

            byte[] sb_sector = imagePlugin.ReadSectors(partition.Start, run);

            magic = BitConverter.ToUInt64(sb_sector, 0);

            return magic == HAMMER_FSBUF_VOLUME || magic == HAMMER_FSBUF_VOLUME_REV;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            HammerSuperBlock hammer_sb = new HammerSuperBlock();

            uint run = HAMMER_VOLHDR_SIZE / imagePlugin.GetSectorSize();

            if(HAMMER_VOLHDR_SIZE % imagePlugin.GetSectorSize() > 0)
                run++;

            ulong magic;

            byte[] sb_sector = imagePlugin.ReadSectors(partition.Start, run);

            magic = BitConverter.ToUInt64(sb_sector, 0);

            if(magic == HAMMER_FSBUF_VOLUME)
            {
                GCHandle handle = GCHandle.Alloc(sb_sector, GCHandleType.Pinned);
                hammer_sb = (HammerSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HammerSuperBlock));
                handle.Free();
            }
            else
            {
                hammer_sb = BigEndianMarshal.ByteArrayToStructureBigEndian<HammerSuperBlock>(sb_sector);
            }

            sb.AppendLine("HAMMER filesystem");

            sb.AppendFormat("Volume version: {0}", hammer_sb.vol_version).AppendLine();
            sb.AppendFormat("Volume {0} of {1} on this filesystem", hammer_sb.vol_no + 1, hammer_sb.vol_count).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(hammer_sb.vol_label, CurrentEncoding)).AppendLine();
            sb.AppendFormat("Volume serial: {0}", hammer_sb.vol_fsid).AppendLine();
            sb.AppendFormat("Filesystem type: {0}", hammer_sb.vol_fstype).AppendLine();
            sb.AppendFormat("Boot area starts at {0}", hammer_sb.vol_bot_beg).AppendLine();
            sb.AppendFormat("Memory log starts at {0}", hammer_sb.vol_mem_beg).AppendLine();
            sb.AppendFormat("First volume buffer starts at {0}", hammer_sb.vol_buf_beg).AppendLine();
            sb.AppendFormat("Volume ends at {0}", hammer_sb.vol_buf_end).AppendLine();

            xmlFSType = new Schemas.FileSystemType
            {
                Clusters = (long)(partition.Size / HAMMER_BIGBLOCK_SIZE),
                ClusterSize = HAMMER_BIGBLOCK_SIZE,
                Dirty = false,
                Type = "HAMMER",
                VolumeName = StringHandlers.CToString(hammer_sb.vol_label, CurrentEncoding),
                VolumeSerial = hammer_sb.vol_fsid.ToString(),
            };

            if(hammer_sb.vol_no == hammer_sb.vol_rootvol)
            {
                sb.AppendFormat("Filesystem contains {0} \"big-blocks\" ({1} bytes)", hammer_sb.vol0_stat_bigblocks, hammer_sb.vol0_stat_bigblocks * HAMMER_BIGBLOCK_SIZE).AppendLine();
                sb.AppendFormat("Filesystem has {0} \"big-blocks\" free ({1} bytes)", hammer_sb.vol0_stat_freebigblocks, hammer_sb.vol0_stat_freebigblocks * HAMMER_BIGBLOCK_SIZE).AppendLine();
                sb.AppendFormat("Filesystem has {0} inode used", hammer_sb.vol0_stat_inodes).AppendLine();

                xmlFSType.Clusters = hammer_sb.vol0_stat_bigblocks;
                xmlFSType.FreeClusters = hammer_sb.vol0_stat_freebigblocks;
                xmlFSType.FreeClustersSpecified = true;
                xmlFSType.Files = hammer_sb.vol0_stat_inodes;
                xmlFSType.FilesSpecified = true;
            }
            // 0 ?
            //sb.AppendFormat("Volume header CRC: 0x{0:X8}", afs_sb.vol_crc).AppendLine();

            information = sb.ToString();
        }

        /// <summary>
        /// Be superblock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HammerSuperBlock
        {
            /// <summary><see cref="HAMMER_FSBUF_VOLUME"/> for a valid header</summary>
            public ulong vol_signature;

             /* These are relative to block device offset, not zone offsets. */
            /// <summary>offset of boot area</summary>
            public long vol_bot_beg;
            /// <summary>offset of memory log</summary>
            public long vol_mem_beg;
            /// <summary>offset of the first buffer in volume</summary>
            public long vol_buf_beg;
            /// <summary>offset of volume EOF (on buffer boundary)</summary>
            public long vol_buf_end;
            public long vol_reserved01;

            /// <summary>identify filesystem</summary>
            public Guid vol_fsid;
            /// <summary>identify filesystem type</summary>
            public Guid vol_fstype;
            /// <summary>filesystem label</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] vol_label;

            /// <summary>volume number within filesystem</summary>
            public int vol_no;
            /// <summary>number of volumes making up filesystem</summary>
            public int vol_count;

            /// <summary>version control information</summary>
            public uint vol_version;
            /// <summary>header crc</summary>
            public hammer_crc_t vol_crc;
            /// <summary>volume flags</summary>
            public uint vol_flags;
            /// <summary>the root volume number (must be 0)</summary>
            public uint vol_rootvol;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public uint[] vol_reserved;

            /*
             * These fields are initialized and space is reserved in every
             * volume making up a HAMMER filesytem, but only the root volume
             * contains valid data.  Note that vol0_stat_bigblocks does not
             * include big-blocks for freemap and undomap initially allocated
             * by newfs_hammer(8).
             */
            /// <summary>total big-blocks when fs is empty</summary>
            public long vol0_stat_bigblocks;
            /// <summary>number of free big-blocks</summary>
            public long vol0_stat_freebigblocks;
            public long vol0_reserved01;
            /// <summary>for statfs only</summary>
            public long vol0_stat_inodes;
            public long vol0_reserved02;
            /// <summary>B-Tree root offset in zone-8</summary>
            public hammer_off_t vol0_btree_root;
            /// <summary>highest partially synchronized TID</summary>
            public hammer_tid_t vol0_next_tid;
            public hammer_off_t vol0_reserved03;

            /// <summary>Blockmaps for zones.  Not all zones use a blockmap.  Note that the entire root blockmap is cached in the hammer_mount structure.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public HammerBlockMap[] vol0_blockmap;

            /// <summary>Array of zone-2 addresses for undo FIFO.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public hammer_off_t[] vol0_undo_array;
        }

        struct HammerBlockMap
        {
            /// <summary>zone-2 offset only used by zone-4</summary>
            public hammer_off_t phys_offset;
            /// <summary>zone-X offset only used by zone-3</summary>
            public hammer_off_t first_offset;
            /// <summary>zone-X offset for allocation</summary>
            public hammer_off_t next_offset;
            /// <summary>zone-X offset only used by zone-3</summary>
            public hammer_off_t alloc_offset;
            public uint reserved01;
            public hammer_crc_t entry_crc;
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
 