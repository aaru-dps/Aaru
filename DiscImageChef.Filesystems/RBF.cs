// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RBF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Random Block File filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Random Block File filesystem and shows information.
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
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    public class RBF : Filesystem
    {
        /// <summary>
        /// Identification sector. Wherever the sector this resides on, becomes LSN 0.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RBF_IdSector
        {
            /// <summary>Sectors on disk</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] dd_tot;
            /// <summary>Tracks</summary>
            public byte dd_tks;
            /// <summary>Bytes in allocation map</summary>
            public ushort dd_map;
            /// <summary>Sectors per cluster</summary>
            public ushort dd_bit;
            /// <summary>LSN of root directory</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] dd_dir;
            /// <summary>Owner ID</summary>
            public ushort dd_own;
            /// <summary>Attributes</summary>
            public byte dd_att;
            /// <summary>Disk ID</summary>
            public ushort dd_dsk;
            /// <summary>Format byte</summary>
            public byte dd_fmt;
            /// <summary>Sectors per track</summary>
            public ushort dd_spt;
            /// <summary>Reserved</summary>
            public ushort dd_res;
            /// <summary>LSN of boot file</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] dd_bt;
            /// <summary>Size of boot file</summary>
            public ushort dd_bsz;
            /// <summary>Creation date</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] dd_dat;
            /// <summary>Volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] dd_nam;
            /// <summary>Path options</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] dd_opt;
            /// <summary>Reserved</summary>
            public byte reserved;
            /// <summary>Magic number</summary>
            public uint dd_sync;
            /// <summary>LSN of allocation map</summary>
            public uint dd_maplsn;
            /// <summary>Size of an LSN</summary>
            public ushort dd_lsnsize;
            /// <summary>Version ID</summary>
            public ushort dd_versid;
        }

        /// <summary>
        /// Identification sector. Wherever the sector this resides on, becomes LSN 0.
        /// Introduced on OS-9000, this can be big or little endian.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RBF_NewIdSector
        {
            /// <summary>Magic number</summary>
            public uint rid_sync;
            /// <summary>Disk ID</summary>
            public uint rid_diskid;
            /// <summary>Sectors on disk</summary>
            public uint rid_totblocks;
            /// <summary>Cylinders</summary>
            public ushort rid_cylinders;
            /// <summary>Sectors in cylinder 0</summary>
            public ushort rid_cyl0size;
            /// <summary>Sectors per cylinder</summary>
            public ushort rid_cylsize;
            /// <summary>Heads</summary>
            public ushort rid_heads;
            /// <summary>Bytes per sector</summary>
            public ushort rid_blocksize;
            /// <summary>Disk format</summary>
            public ushort rid_format;
            /// <summary>Flags</summary>
            public ushort rid_flags;
            /// <summary>Padding</summary>
            public ushort rid_unused1;
            /// <summary>Sector of allocation bitmap</summary>
            public uint rid_bitmap;
            /// <summary>Sector of debugger FD</summary>
            public uint rid_firstboot;
            /// <summary>Sector of bootfile FD</summary>
            public uint rid_bootfile;
            /// <summary>Sector of root directory FD</summary>
            public uint rid_rootdir;
            /// <summary>Group owner of media</summary>
            public ushort rid_group;
            /// <summary>Owner of media</summary>
            public ushort rid_owner;
            /// <summary>Creation time</summary>
            public uint rid_ctime;
            /// <summary>Last write time for this structure</summary>
            public uint rid_mtime;
            /// <summary>Volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rid_name;
            /// <summary>Endian flag</summary>
            public byte rid_endflag;
            /// <summary>Padding</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] rid_unused2;
            /// <summary>Parity</summary>
            public uint rid_parity;
        }

        /// <summary>Magic number for OS-9. Same for OS-9000?</summary>
        const uint RBF_Sync = 0x4372757A;
        const uint RBF_Cnys = 0x7A757243;

        public RBF()
        {
            Name = "OS-9 Random Block File Plugin";
            PluginUUID = new Guid("E864E45B-0B52-4D29-A858-7BDFA9199FB2");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public RBF(Encoding encoding)
        {
            Name = "OS-9 Random Block File Plugin";
            PluginUUID = new Guid("E864E45B-0B52-4D29-A858-7BDFA9199FB2");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public RBF(DiscImages.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "OS-9 Random Block File Plugin";
            PluginUUID = new Guid("E864E45B-0B52-4D29-A858-7BDFA9199FB2");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < 256) return false;

            // Documentation says ID should be sector 0
            // I've found that OS-9/X68000 has it on sector 4
            // I've read OS-9/Apple2 has it on sector 15
            foreach(ulong location in new[] {0, 4, 15})
            {
                RBF_IdSector RBFSb = new RBF_IdSector();

                uint sbSize = (uint)(Marshal.SizeOf(RBFSb) / imagePlugin.GetSectorSize());
                if(Marshal.SizeOf(RBFSb) % imagePlugin.GetSectorSize() != 0) sbSize++;

                if(partition.Start + location + sbSize >= imagePlugin.GetSectors()) break;

                byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                if(sector.Length < Marshal.SizeOf(RBFSb)) return false;

                RBFSb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_IdSector>(sector);
                RBF_NewIdSector RBF9000Sb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_NewIdSector>(sector);

                DicConsole.DebugWriteLine("RBF plugin",
                                          "magic at {0} = 0x{1:X8} or 0x{2:X8} (expected 0x{3:X8} or 0x{4:X8})",
                                          location, RBFSb.dd_sync, RBF9000Sb.rid_sync, RBF_Sync, RBF_Cnys);

                if(RBFSb.dd_sync == RBF_Sync || RBF9000Sb.rid_sync == RBF_Sync || RBF9000Sb.rid_sync == RBF_Cnys)
                    return true;
            }

            return false;
        }

        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 256) return;

            RBF_IdSector RBFSb = new RBF_IdSector();
            RBF_NewIdSector RBF9000Sb = new RBF_NewIdSector();

            foreach(ulong location in new[] {0, 4, 15})
            {
                uint sbSize = (uint)(Marshal.SizeOf(RBFSb) / imagePlugin.GetSectorSize());
                if(Marshal.SizeOf(RBFSb) % imagePlugin.GetSectorSize() != 0) sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                if(sector.Length < Marshal.SizeOf(RBFSb)) return;

                RBFSb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_IdSector>(sector);
                RBF9000Sb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_NewIdSector>(sector);

                DicConsole.DebugWriteLine("RBF plugin",
                                          "magic at {0} = 0x{1:X8} or 0x{2:X8} (expected 0x{3:X8} or 0x{4:X8})",
                                          location, RBFSb.dd_sync, RBF9000Sb.rid_sync, RBF_Sync, RBF_Cnys);

                if(RBFSb.dd_sync == RBF_Sync || RBF9000Sb.rid_sync == RBF_Sync || RBF9000Sb.rid_sync == RBF_Cnys) break;
            }

            if(RBFSb.dd_sync != RBF_Sync && RBF9000Sb.rid_sync != RBF_Sync && RBF9000Sb.rid_sync != RBF_Cnys) return;

            if(RBF9000Sb.rid_sync == RBF_Cnys) RBF9000Sb = BigEndianMarshal.SwapStructureMembersEndian(RBF9000Sb);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("OS-9 Random Block File");

            if(RBF9000Sb.rid_sync == RBF_Sync)
            {
                sb.AppendFormat("Volume ID: {0:X8}", RBF9000Sb.rid_diskid).AppendLine();
                sb.AppendFormat("{0} blocks in volume", RBF9000Sb.rid_totblocks).AppendLine();
                sb.AppendFormat("{0} cylinders", RBF9000Sb.rid_cylinders).AppendLine();
                sb.AppendFormat("{0} blocks in cylinder 0", RBF9000Sb.rid_cyl0size).AppendLine();
                sb.AppendFormat("{0} blocks per cylinder", RBF9000Sb.rid_cylsize).AppendLine();
                sb.AppendFormat("{0} heads", RBF9000Sb.rid_heads).AppendLine();
                sb.AppendFormat("{0} bytes per block", RBF9000Sb.rid_blocksize).AppendLine();
                // TODO: Convert to flags?
                if((RBF9000Sb.rid_format & 0x01) == 0x01) sb.AppendLine("Disk is double sided");
                else sb.AppendLine("Disk is single sided");
                if((RBF9000Sb.rid_format & 0x02) == 0x02) sb.AppendLine("Disk is double density");
                else sb.AppendLine("Disk is single density");
                if((RBF9000Sb.rid_format & 0x10) == 0x10) sb.AppendLine("Disk is 384 TPI");
                else if((RBF9000Sb.rid_format & 0x08) == 0x08) sb.AppendLine("Disk is 192 TPI");
                else if((RBF9000Sb.rid_format & 0x04) == 0x04) sb.AppendLine("Disk is 96 TPI or 135 TPI");
                else sb.AppendLine("Disk is 48 TPI");
                sb.AppendFormat("Allocation bitmap descriptor starts at block {0}",
                                RBF9000Sb.rid_bitmap == 0 ? 1 : RBF9000Sb.rid_bitmap).AppendLine();
                if(RBF9000Sb.rid_firstboot > 0)
                    sb.AppendFormat("Debugger descriptor starts at block {0}", RBF9000Sb.rid_firstboot).AppendLine();
                if(RBF9000Sb.rid_bootfile > 0)
                    sb.AppendFormat("Boot file descriptor starts at block {0}", RBF9000Sb.rid_bootfile).AppendLine();
                sb.AppendFormat("Root directory descriptor starts at block {0}", RBF9000Sb.rid_rootdir).AppendLine();
                sb.AppendFormat("Disk is owned by group {0} user {1}", RBF9000Sb.rid_group, RBF9000Sb.rid_owner)
                  .AppendLine();
                sb.AppendFormat("Volume was created on {0}", DateHandlers.UNIXToDateTime(RBF9000Sb.rid_ctime))
                  .AppendLine();
                sb.AppendFormat("Volume's identification block was last written on {0}",
                                DateHandlers.UNIXToDateTime(RBF9000Sb.rid_mtime)).AppendLine();
                sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(RBF9000Sb.rid_name, CurrentEncoding))
                  .AppendLine();

                xmlFSType = new Schemas.FileSystemType
                {
                    Type = "OS-9 Random Block File",
                    Bootable = RBF9000Sb.rid_bootfile > 0,
                    ClusterSize = RBF9000Sb.rid_blocksize,
                    Clusters = RBF9000Sb.rid_totblocks,
                    CreationDate = DateHandlers.UNIXToDateTime(RBF9000Sb.rid_ctime),
                    CreationDateSpecified = true,
                    ModificationDate = DateHandlers.UNIXToDateTime(RBF9000Sb.rid_mtime),
                    ModificationDateSpecified = true,
                    VolumeName = StringHandlers.CToString(RBF9000Sb.rid_name, CurrentEncoding),
                    VolumeSerial = string.Format("{0:X8}", RBF9000Sb.rid_diskid)
                };
            }
            else
            {
                sb.AppendFormat("Volume ID: {0:X4}", RBFSb.dd_dsk).AppendLine();
                sb.AppendFormat("{0} blocks in volume", LSNToUInt32(RBFSb.dd_tot)).AppendLine();
                sb.AppendFormat("{0} tracks", RBFSb.dd_tks).AppendLine();
                sb.AppendFormat("{0} sectors per track", RBFSb.dd_spt).AppendLine();
                sb.AppendFormat("{0} bytes per sector", 256 << RBFSb.dd_lsnsize).AppendLine();
                sb.AppendFormat("{0} sectors per cluster ({1} bytes)", RBFSb.dd_bit,
                                RBFSb.dd_bit * (256 << RBFSb.dd_lsnsize)).AppendLine();
                // TODO: Convert to flags?
                if((RBFSb.dd_fmt & 0x01) == 0x01) sb.AppendLine("Disk is double sided");
                else sb.AppendLine("Disk is single sided");
                if((RBFSb.dd_fmt & 0x02) == 0x02) sb.AppendLine("Disk is double density");
                else sb.AppendLine("Disk is single density");
                if((RBFSb.dd_fmt & 0x10) == 0x10) sb.AppendLine("Disk is 384 TPI");
                else if((RBFSb.dd_fmt & 0x08) == 0x08) sb.AppendLine("Disk is 192 TPI");
                else if((RBFSb.dd_fmt & 0x04) == 0x04) sb.AppendLine("Disk is 96 TPI or 135 TPI");
                else sb.AppendLine("Disk is 48 TPI");
                sb.AppendFormat("Allocation bitmap descriptor starts at block {0}",
                                RBFSb.dd_maplsn == 0 ? 1 : RBFSb.dd_maplsn).AppendLine();
                sb.AppendFormat("{0} bytes in allocation bitmap", RBFSb.dd_map).AppendLine();
                if(LSNToUInt32(RBFSb.dd_bt) > 0 && RBFSb.dd_bsz > 0)
                    sb.AppendFormat("Boot file starts at block {0} and has {1} bytes", LSNToUInt32(RBFSb.dd_bt),
                                    RBFSb.dd_bsz).AppendLine();
                sb.AppendFormat("Root directory descriptor starts at block {0}", LSNToUInt32(RBFSb.dd_dir))
                  .AppendLine();
                sb.AppendFormat("Disk is owned by user {0}", RBFSb.dd_own).AppendLine();
                sb.AppendFormat("Volume was created on {0}", DateHandlers.OS9ToDateTime(RBFSb.dd_dat)).AppendLine();
                sb.AppendFormat("Volume attributes: {0:X2}", RBFSb.dd_att).AppendLine();
                sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(RBFSb.dd_nam, CurrentEncoding))
                  .AppendLine();
                sb.AppendFormat("Path descriptor options: {0}", StringHandlers.CToString(RBFSb.dd_opt, CurrentEncoding))
                  .AppendLine();

                xmlFSType = new Schemas.FileSystemType
                {
                    Type = "OS-9 Random Block File",
                    Bootable = LSNToUInt32(RBFSb.dd_bt) > 0 && RBFSb.dd_bsz > 0,
                    ClusterSize = RBFSb.dd_bit * (256 << RBFSb.dd_lsnsize),
                    Clusters = LSNToUInt32(RBFSb.dd_tot),
                    CreationDate = DateHandlers.OS9ToDateTime(RBFSb.dd_dat),
                    CreationDateSpecified = true,
                    VolumeName = StringHandlers.CToString(RBFSb.dd_nam, CurrentEncoding),
                    VolumeSerial = string.Format("{0:X4}", RBFSb.dd_dsk)
                };
            }

            information = sb.ToString();
        }

        public static uint LSNToUInt32(byte[] lsn)
        {
            if(lsn == null || lsn.Length != 3) return 0;

            return (uint)((lsn[0] << 16) + (lsn[1] << 8) + lsn[2]);
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