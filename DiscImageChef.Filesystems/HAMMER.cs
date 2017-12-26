// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HAMMER.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HAMMER filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the HAMMER filesystem and shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;
using hammer_crc_t = System.UInt32;
using hammer_off_t = System.UInt64;
using hammer_tid_t = System.UInt64;

#pragma warning disable 169

namespace DiscImageChef.Filesystems
{
    public class HAMMER : IFilesystem
    {
        const ulong HAMMER_FSBUF_VOLUME = 0xC8414D4DC5523031;
        const ulong HAMMER_FSBUF_VOLUME_REV = 0x313052C54D4D41C8;
        const uint HAMMER_VOLHDR_SIZE = 1928;
        const int HAMMER_BIGBLOCK_SIZE = 8192 * 1024;

        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;

        public Encoding Encoding => currentEncoding;
        public string Name => "HAMMER Filesystem";
        public Guid Id => new Guid("91A188BF-5FD7-4677-BBD3-F59EBA9C864D");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            uint run = HAMMER_VOLHDR_SIZE / imagePlugin.Info.SectorSize;

            if(HAMMER_VOLHDR_SIZE % imagePlugin.Info.SectorSize > 0) run++;

            if(run + partition.Start >= partition.End) return false;

            ulong magic;

            byte[] sbSector = imagePlugin.ReadSectors(partition.Start, run);

            magic = BitConverter.ToUInt64(sbSector, 0);

            return magic == HAMMER_FSBUF_VOLUME || magic == HAMMER_FSBUF_VOLUME_REV;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            StringBuilder sb = new StringBuilder();

            HammerSuperBlock hammerSb;

            uint run = HAMMER_VOLHDR_SIZE / imagePlugin.Info.SectorSize;

            if(HAMMER_VOLHDR_SIZE % imagePlugin.Info.SectorSize > 0) run++;

            ulong magic;

            byte[] sbSector = imagePlugin.ReadSectors(partition.Start, run);

            magic = BitConverter.ToUInt64(sbSector, 0);

            if(magic == HAMMER_FSBUF_VOLUME)
            {
                GCHandle handle = GCHandle.Alloc(sbSector, GCHandleType.Pinned);
                hammerSb = (HammerSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                    typeof(HammerSuperBlock));
                handle.Free();
            }
            else hammerSb = BigEndianMarshal.ByteArrayToStructureBigEndian<HammerSuperBlock>(sbSector);

            sb.AppendLine("HAMMER filesystem");

            sb.AppendFormat("Volume version: {0}", hammerSb.vol_version).AppendLine();
            sb.AppendFormat("Volume {0} of {1} on this filesystem", hammerSb.vol_no + 1, hammerSb.vol_count)
              .AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(hammerSb.vol_label, currentEncoding))
              .AppendLine();
            sb.AppendFormat("Volume serial: {0}", hammerSb.vol_fsid).AppendLine();
            sb.AppendFormat("Filesystem type: {0}", hammerSb.vol_fstype).AppendLine();
            sb.AppendFormat("Boot area starts at {0}", hammerSb.vol_bot_beg).AppendLine();
            sb.AppendFormat("Memory log starts at {0}", hammerSb.vol_mem_beg).AppendLine();
            sb.AppendFormat("First volume buffer starts at {0}", hammerSb.vol_buf_beg).AppendLine();
            sb.AppendFormat("Volume ends at {0}", hammerSb.vol_buf_end).AppendLine();

            xmlFsType = new FileSystemType
            {
                Clusters = (long)(partition.Size / HAMMER_BIGBLOCK_SIZE),
                ClusterSize = HAMMER_BIGBLOCK_SIZE,
                Dirty = false,
                Type = "HAMMER",
                VolumeName = StringHandlers.CToString(hammerSb.vol_label, currentEncoding),
                VolumeSerial = hammerSb.vol_fsid.ToString()
            };

            if(hammerSb.vol_no == hammerSb.vol_rootvol)
            {
                sb.AppendFormat("Filesystem contains {0} \"big-blocks\" ({1} bytes)", hammerSb.vol0_stat_bigblocks,
                                hammerSb.vol0_stat_bigblocks * HAMMER_BIGBLOCK_SIZE).AppendLine();
                sb.AppendFormat("Filesystem has {0} \"big-blocks\" free ({1} bytes)", hammerSb.vol0_stat_freebigblocks,
                                hammerSb.vol0_stat_freebigblocks * HAMMER_BIGBLOCK_SIZE).AppendLine();
                sb.AppendFormat("Filesystem has {0} inode used", hammerSb.vol0_stat_inodes).AppendLine();

                xmlFsType.Clusters = hammerSb.vol0_stat_bigblocks;
                xmlFsType.FreeClusters = hammerSb.vol0_stat_freebigblocks;
                xmlFsType.FreeClustersSpecified = true;
                xmlFsType.Files = hammerSb.vol0_stat_inodes;
                xmlFsType.FilesSpecified = true;
            }
            // 0 ?
            //sb.AppendFormat("Volume header CRC: 0x{0:X8}", afs_sb.vol_crc).AppendLine();

            information = sb.ToString();
        }

        /// <summary>
        ///     Hammer superblock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
        struct HammerSuperBlock
        {
            /// <summary><see cref="HAMMER_FSBUF_VOLUME" /> for a valid header</summary>
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] vol_label;

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

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public uint[] vol_reserved;

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

            /// <summary>
            ///     Blockmaps for zones.  Not all zones use a blockmap.  Note that the entire root blockmap is cached in the
            ///     hammer_mount structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public HammerBlockMap[] vol0_blockmap;

            /// <summary>Array of zone-2 addresses for undo FIFO.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public hammer_off_t[] vol0_undo_array;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
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
    }
}