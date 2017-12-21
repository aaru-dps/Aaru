// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MinixFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the MINIX filesystem and shows information.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from the Linux kernel
    public class MinixFS : Filesystem
    {
        /// <summary>Minix v1, 14 char filenames</summary>
        const ushort MINIX_MAGIC = 0x137F;
        /// <summary>Minix v1, 30 char filenames</summary>
        const ushort MINIX_MAGIC2 = 0x138F;
        /// <summary>Minix v2, 14 char filenames</summary>
        const ushort MINIX2_MAGIC = 0x2468;
        /// <summary>Minix v2, 30 char filenames</summary>
        const ushort MINIX2_MAGIC2 = 0x2478;
        /// <summary>Minix v3, 60 char filenames</summary>
        const ushort MINIX3_MAGIC = 0x4D5A;
        // Byteswapped
        /// <summary>Minix v1, 14 char filenames</summary>
        const ushort MINIX_CIGAM = 0x7F13;
        /// <summary>Minix v1, 30 char filenames</summary>
        const ushort MINIX_CIGAM2 = 0x8F13;
        /// <summary>Minix v2, 14 char filenames</summary>
        const ushort MINIX2_CIGAM = 0x6824;
        /// <summary>Minix v2, 30 char filenames</summary>
        const ushort MINIX2_CIGAM2 = 0x7824;
        /// <summary>Minix v3, 60 char filenames</summary>
        const ushort MINIX3_CIGAM = 0x5A4D;

        public MinixFS()
        {
            Name = "Minix Filesystem";
            PluginUUID = new Guid("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public MinixFS(Encoding encoding)
        {
            Name = "Minix Filesystem";
            PluginUUID = new Guid("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public MinixFS(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Minix Filesystem";
            PluginUUID = new Guid("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            uint sector = 2;
            uint offset = 0;

            if(imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                sector = 0;
                offset = 0x400;
            }

            if(sector + partition.Start >= partition.End) return false;

            ushort magic;
            byte[] minix_sb_sector = imagePlugin.ReadSector(sector + partition.Start);

            // Optical media
            if(offset > 0)
            {
                byte[] tmp = new byte[0x200];
                Array.Copy(minix_sb_sector, offset, tmp, 0, 0x200);
                minix_sb_sector = tmp;
            }

            magic = BitConverter.ToUInt16(minix_sb_sector, 0x010); // Here should reside magic number on Minix v1 & V2

            if(magic == MINIX_MAGIC || magic == MINIX_MAGIC2 || magic == MINIX2_MAGIC || magic == MINIX2_MAGIC2 ||
               magic == MINIX_CIGAM || magic == MINIX_CIGAM2 || magic == MINIX2_CIGAM ||
               magic == MINIX2_CIGAM2) return true;

            magic = BitConverter.ToUInt16(minix_sb_sector, 0x018); // Here should reside magic number on Minix v3

            if(magic == MINIX_MAGIC || magic == MINIX2_MAGIC || magic == MINIX3_MAGIC || magic == MINIX_CIGAM ||
               magic == MINIX2_CIGAM || magic == MINIX3_CIGAM) return true;

            return false;
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            uint sector = 2;
            uint offset = 0;

            if(imagePlugin.ImageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                sector = 0;
                offset = 0x400;
            }

            bool minix3 = false;
            int filenamesize;
            string minixVersion;
            ushort magic;
            byte[] minix_sb_sector = imagePlugin.ReadSector(sector + partition.Start);

            // Optical media
            if(offset > 0)
            {
                byte[] tmp = new byte[0x200];
                Array.Copy(minix_sb_sector, offset, tmp, 0, 0x200);
                minix_sb_sector = tmp;
            }

            magic = BitConverter.ToUInt16(minix_sb_sector, 0x018);

            xmlFSType = new FileSystemType();

            bool littleEndian;

            if(magic == MINIX3_MAGIC || magic == MINIX3_CIGAM || magic == MINIX2_MAGIC || magic == MINIX2_CIGAM ||
               magic == MINIX_MAGIC || magic == MINIX_CIGAM)
            {
                filenamesize = 60;
                littleEndian = magic != MINIX3_CIGAM || magic == MINIX2_CIGAM || magic == MINIX_CIGAM;

                switch(magic) {
                    case MINIX3_MAGIC:
                    case MINIX3_CIGAM:
                        minixVersion = "Minix v3 filesystem";
                        xmlFSType.Type = "Minix v3";
                        break;
                    case MINIX2_MAGIC:
                    case MINIX2_CIGAM:
                        minixVersion = "Minix 3 v2 filesystem";
                        xmlFSType.Type = "Minix 3 v2";
                        break;
                    default:
                        minixVersion = "Minix 3 v1 filesystem";
                        xmlFSType.Type = "Minix 3 v1";
                        break;
                }

                minix3 = true;
            }
            else
            {
                magic = BitConverter.ToUInt16(minix_sb_sector, 0x010);

                switch(magic)
                {
                    case MINIX_MAGIC:
                        filenamesize = 14;
                        minixVersion = "Minix v1 filesystem";
                        littleEndian = true;
                        xmlFSType.Type = "Minix v1";
                        break;
                    case MINIX_MAGIC2:
                        filenamesize = 30;
                        minixVersion = "Minix v1 filesystem";
                        littleEndian = true;
                        xmlFSType.Type = "Minix v1";
                        break;
                    case MINIX2_MAGIC:
                        filenamesize = 14;
                        minixVersion = "Minix v2 filesystem";
                        littleEndian = true;
                        xmlFSType.Type = "Minix v2";
                        break;
                    case MINIX2_MAGIC2:
                        filenamesize = 30;
                        minixVersion = "Minix v2 filesystem";
                        littleEndian = true;
                        xmlFSType.Type = "Minix v2";
                        break;
                    case MINIX_CIGAM:
                        filenamesize = 14;
                        minixVersion = "Minix v1 filesystem";
                        littleEndian = false;
                        xmlFSType.Type = "Minix v1";
                        break;
                    case MINIX_CIGAM2:
                        filenamesize = 30;
                        minixVersion = "Minix v1 filesystem";
                        littleEndian = false;
                        xmlFSType.Type = "Minix v1";
                        break;
                    case MINIX2_CIGAM:
                        filenamesize = 14;
                        minixVersion = "Minix v2 filesystem";
                        littleEndian = false;
                        xmlFSType.Type = "Minix v2";
                        break;
                    case MINIX2_CIGAM2:
                        filenamesize = 30;
                        minixVersion = "Minix v2 filesystem";
                        littleEndian = false;
                        xmlFSType.Type = "Minix v2";
                        break;
                    default: return;
                }
            }

            if(minix3)
            {
                Minix3SuperBlock mnx_sb;

                if(littleEndian)
                {
                    GCHandle handle = GCHandle.Alloc(minix_sb_sector, GCHandleType.Pinned);
                    mnx_sb = (Minix3SuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                      typeof(Minix3SuperBlock));
                    handle.Free();
                }
                else mnx_sb = BigEndianMarshal.ByteArrayToStructureBigEndian<Minix3SuperBlock>(minix_sb_sector);

                if(magic != MINIX3_MAGIC && magic != MINIX3_CIGAM) mnx_sb.s_blocksize = 1024;

                sb.AppendLine(minixVersion);
                sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();
                if(mnx_sb.s_zones > 0) // On V2
                    sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_zones, mnx_sb.s_zones * 1024)
                      .AppendLine();
                else
                    sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_nzones, mnx_sb.s_nzones * 1024)
                      .AppendLine();
                sb.AppendFormat("{0} bytes/block", mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("{0} inodes on volume", mnx_sb.s_ninodes).AppendLine();
                sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnx_sb.s_imap_blocks,
                                mnx_sb.s_imap_blocks * mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnx_sb.s_zmap_blocks,
                                mnx_sb.s_zmap_blocks * mnx_sb.s_blocksize).AppendLine();
                sb.AppendFormat("First data zone: {0}", mnx_sb.s_firstdatazone).AppendLine();
                //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
                sb.AppendFormat("{0} bytes maximum per file", mnx_sb.s_max_size).AppendLine();
                sb.AppendFormat("On-disk filesystem version: {0}", mnx_sb.s_disk_version).AppendLine();

                xmlFSType.ClusterSize = mnx_sb.s_blocksize;
                if(mnx_sb.s_zones > 0) xmlFSType.Clusters = mnx_sb.s_zones;
                else xmlFSType.Clusters = mnx_sb.s_nzones;
            }
            else
            {
                MinixSuperBlock mnx_sb;

                if(littleEndian)
                {
                    GCHandle handle = GCHandle.Alloc(minix_sb_sector, GCHandleType.Pinned);
                    mnx_sb = (MinixSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                                     typeof(MinixSuperBlock));
                    handle.Free();
                }
                else mnx_sb = BigEndianMarshal.ByteArrayToStructureBigEndian<MinixSuperBlock>(minix_sb_sector);

                sb.AppendLine(minixVersion);
                sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();
                if(mnx_sb.s_zones > 0) // On V2
                    sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_zones, mnx_sb.s_zones * 1024)
                      .AppendLine();
                else
                    sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_nzones, mnx_sb.s_nzones * 1024)
                      .AppendLine();
                sb.AppendFormat("{0} inodes on volume", mnx_sb.s_ninodes).AppendLine();
                sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnx_sb.s_imap_blocks,
                                mnx_sb.s_imap_blocks * 1024).AppendLine();
                sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnx_sb.s_zmap_blocks, mnx_sb.s_zmap_blocks * 1024)
                  .AppendLine();
                sb.AppendFormat("First data zone: {0}", mnx_sb.s_firstdatazone).AppendLine();
                //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
                sb.AppendFormat("{0} bytes maximum per file", mnx_sb.s_max_size).AppendLine();
                sb.AppendFormat("Filesystem state: {0:X4}", mnx_sb.s_state).AppendLine();
                xmlFSType.ClusterSize = 1024;
                if(mnx_sb.s_zones > 0) xmlFSType.Clusters = mnx_sb.s_zones;
                else xmlFSType.Clusters = mnx_sb.s_nzones;
            }
            information = sb.ToString();
        }

        /// <summary>
        /// Superblock for Minix v1 and V2 filesystems
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MinixSuperBlock
        {
            /// <summary>0x00, inodes on volume</summary>
            public ushort s_ninodes;
            /// <summary>0x02, zones on volume</summary>
            public ushort s_nzones;
            /// <summary>0x04, blocks on inode map</summary>
            public short s_imap_blocks;
            /// <summary>0x06, blocks on zone map</summary>
            public short s_zmap_blocks;
            /// <summary>0x08, first data zone</summary>
            public ushort s_firstdatazone;
            /// <summary>0x0A, log2 of blocks/zone</summary>
            public short s_log_zone_size;
            /// <summary>0x0C, max file size</summary>
            public uint s_max_size;
            /// <summary>0x10, magic</summary>
            public ushort s_magic;
            /// <summary>0x12, filesystem state</summary>
            public ushort s_state;
            /// <summary>0x14, number of zones</summary>
            public uint s_zones;
        }

        /// <summary>
        /// Superblock for Minix v3 filesystems
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Minix3SuperBlock
        {
            /// <summary>0x00, inodes on volume</summary>
            public uint s_ninodes;
            /// <summary>0x02, old zones on volume</summary>
            public ushort s_nzones;
            /// <summary>0x06, blocks on inode map</summary>
            public ushort s_imap_blocks;
            /// <summary>0x08, blocks on zone map</summary>
            public ushort s_zmap_blocks;
            /// <summary>0x0A, first data zone</summary>
            public ushort s_firstdatazone;
            /// <summary>0x0C, log2 of blocks/zone</summary>
            public ushort s_log_zone_size;
            /// <summary>0x0E, padding</summary>
            public ushort s_pad1;
            /// <summary>0x10, max file size</summary>
            public uint s_max_size;
            /// <summary>0x14, number of zones</summary>
            public uint s_zones;
            /// <summary>0x18, magic</summary>
            public ushort s_magic;
            /// <summary>0x1A, padding</summary>
            public ushort s_pad2;
            /// <summary>0x1C, bytes in a block</summary>
            public ushort s_blocksize;
            /// <summary>0x1E, on-disk structures version</summary>
            public byte s_disk_version;
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