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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class RBF : IFilesystem
    {
        /// <summary>Magic number for OS-9. Same for OS-9000?</summary>
        const uint RBF_SYNC = 0x4372757A;
        const uint RBF_CNYS = 0x7A757243;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "OS-9 Random Block File Plugin";
        public Guid           Id        => new Guid("E864E45B-0B52-4D29-A858-7BDFA9199FB2");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 256) return false;

            // Documentation says ID should be sector 0
            // I've found that OS-9/X68000 has it on sector 4
            // I've read OS-9/Apple2 has it on sector 15
            foreach(int i in new[] {0, 4, 15})
            {
                ulong        location = (ulong)i;
                RBF_IdSector rbfSb    = new RBF_IdSector();

                uint sbSize = (uint)(Marshal.SizeOf(rbfSb) / imagePlugin.Info.SectorSize);
                if(Marshal.SizeOf(rbfSb) % imagePlugin.Info.SectorSize != 0) sbSize++;

                if(partition.Start + location + sbSize >= imagePlugin.Info.Sectors) break;

                byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                if(sector.Length < Marshal.SizeOf(rbfSb)) return false;

                rbfSb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_IdSector>(sector);
                RBF_NewIdSector rbf9000Sb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_NewIdSector>(sector);

                DicConsole.DebugWriteLine("RBF plugin",
                                          "magic at {0} = 0x{1:X8} or 0x{2:X8} (expected 0x{3:X8} or 0x{4:X8})",
                                          location, rbfSb.dd_sync, rbf9000Sb.rid_sync, RBF_SYNC, RBF_CNYS);

                if(rbfSb.dd_sync == RBF_SYNC || rbf9000Sb.rid_sync == RBF_SYNC || rbf9000Sb.rid_sync == RBF_CNYS)
                    return true;
            }

            return false;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            if(imagePlugin.Info.SectorSize < 256) return;

            RBF_IdSector    rbfSb     = new RBF_IdSector();
            RBF_NewIdSector rbf9000Sb = new RBF_NewIdSector();

            foreach(int i in new[] {0, 4, 15})
            {
                ulong location = (ulong)i;
                uint  sbSize   = (uint)(Marshal.SizeOf(rbfSb) / imagePlugin.Info.SectorSize);
                if(Marshal.SizeOf(rbfSb) % imagePlugin.Info.SectorSize != 0) sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                if(sector.Length < Marshal.SizeOf(rbfSb)) return;

                rbfSb     = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_IdSector>(sector);
                rbf9000Sb = BigEndianMarshal.ByteArrayToStructureBigEndian<RBF_NewIdSector>(sector);

                DicConsole.DebugWriteLine("RBF plugin",
                                          "magic at {0} = 0x{1:X8} or 0x{2:X8} (expected 0x{3:X8} or 0x{4:X8})",
                                          location, rbfSb.dd_sync, rbf9000Sb.rid_sync, RBF_SYNC, RBF_CNYS);

                if(rbfSb.dd_sync == RBF_SYNC || rbf9000Sb.rid_sync == RBF_SYNC || rbf9000Sb.rid_sync == RBF_CNYS) break;
            }

            if(rbfSb.dd_sync != RBF_SYNC && rbf9000Sb.rid_sync != RBF_SYNC && rbf9000Sb.rid_sync != RBF_CNYS) return;

            if(rbf9000Sb.rid_sync == RBF_CNYS)
                rbf9000Sb = (RBF_NewIdSector)BigEndianMarshal.SwapStructureMembersEndian(rbf9000Sb);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("OS-9 Random Block File");

            if(rbf9000Sb.rid_sync == RBF_SYNC)
            {
                sb.AppendFormat("Volume ID: {0:X8}", rbf9000Sb.rid_diskid).AppendLine();
                sb.AppendFormat("{0} blocks in volume", rbf9000Sb.rid_totblocks).AppendLine();
                sb.AppendFormat("{0} cylinders", rbf9000Sb.rid_cylinders).AppendLine();
                sb.AppendFormat("{0} blocks in cylinder 0", rbf9000Sb.rid_cyl0size).AppendLine();
                sb.AppendFormat("{0} blocks per cylinder", rbf9000Sb.rid_cylsize).AppendLine();
                sb.AppendFormat("{0} heads", rbf9000Sb.rid_heads).AppendLine();
                sb.AppendFormat("{0} bytes per block", rbf9000Sb.rid_blocksize).AppendLine();
                // TODO: Convert to flags?
                sb.AppendLine((rbf9000Sb.rid_format & 0x01) == 0x01 ? "Disk is double sided" : "Disk is single sided");
                sb.AppendLine((rbf9000Sb.rid_format & 0x02) == 0x02
                                  ? "Disk is double density"
                                  : "Disk is single density");
                if((rbf9000Sb.rid_format      & 0x10) == 0x10) sb.AppendLine("Disk is 384 TPI");
                else if((rbf9000Sb.rid_format & 0x08) == 0x08) sb.AppendLine("Disk is 192 TPI");
                else if((rbf9000Sb.rid_format & 0x04) == 0x04) sb.AppendLine("Disk is 96 TPI or 135 TPI");
                else sb.AppendLine("Disk is 48 TPI");
                sb.AppendFormat("Allocation bitmap descriptor starts at block {0}",
                                rbf9000Sb.rid_bitmap == 0 ? 1 : rbf9000Sb.rid_bitmap).AppendLine();
                if(rbf9000Sb.rid_firstboot > 0)
                    sb.AppendFormat("Debugger descriptor starts at block {0}", rbf9000Sb.rid_firstboot).AppendLine();
                if(rbf9000Sb.rid_bootfile > 0)
                    sb.AppendFormat("Boot file descriptor starts at block {0}", rbf9000Sb.rid_bootfile).AppendLine();
                sb.AppendFormat("Root directory descriptor starts at block {0}", rbf9000Sb.rid_rootdir).AppendLine();
                sb.AppendFormat("Disk is owned by group {0} user {1}", rbf9000Sb.rid_group, rbf9000Sb.rid_owner)
                  .AppendLine();
                sb.AppendFormat("Volume was created on {0}", DateHandlers.UnixToDateTime(rbf9000Sb.rid_ctime))
                  .AppendLine();
                sb.AppendFormat("Volume's identification block was last written on {0}",
                                DateHandlers.UnixToDateTime(rbf9000Sb.rid_mtime)).AppendLine();
                sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(rbf9000Sb.rid_name, Encoding))
                  .AppendLine();

                XmlFsType = new FileSystemType
                {
                    Type                      = "OS-9 Random Block File",
                    Bootable                  = rbf9000Sb.rid_bootfile > 0,
                    ClusterSize               = rbf9000Sb.rid_blocksize,
                    Clusters                  = rbf9000Sb.rid_totblocks,
                    CreationDate              = DateHandlers.UnixToDateTime(rbf9000Sb.rid_ctime),
                    CreationDateSpecified     = true,
                    ModificationDate          = DateHandlers.UnixToDateTime(rbf9000Sb.rid_mtime),
                    ModificationDateSpecified = true,
                    VolumeName                = StringHandlers.CToString(rbf9000Sb.rid_name, Encoding),
                    VolumeSerial              = $"{rbf9000Sb.rid_diskid:X8}"
                };
            }
            else
            {
                sb.AppendFormat("Volume ID: {0:X4}", rbfSb.dd_dsk).AppendLine();
                sb.AppendFormat("{0} blocks in volume", LSNToUInt32(rbfSb.dd_tot)).AppendLine();
                sb.AppendFormat("{0} tracks", rbfSb.dd_tks).AppendLine();
                sb.AppendFormat("{0} sectors per track", rbfSb.dd_spt).AppendLine();
                sb.AppendFormat("{0} bytes per sector", 256 << rbfSb.dd_lsnsize).AppendLine();
                sb.AppendFormat("{0} sectors per cluster ({1} bytes)", rbfSb.dd_bit,
                                rbfSb.dd_bit * (256 << rbfSb.dd_lsnsize)).AppendLine();
                // TODO: Convert to flags?
                sb.AppendLine((rbfSb.dd_fmt & 0x01) == 0x01 ? "Disk is double sided" : "Disk is single sided");
                sb.AppendLine((rbfSb.dd_fmt & 0x02) == 0x02 ? "Disk is double density" : "Disk is single density");
                if((rbfSb.dd_fmt      & 0x10) == 0x10) sb.AppendLine("Disk is 384 TPI");
                else if((rbfSb.dd_fmt & 0x08) == 0x08) sb.AppendLine("Disk is 192 TPI");
                else if((rbfSb.dd_fmt & 0x04) == 0x04) sb.AppendLine("Disk is 96 TPI or 135 TPI");
                else sb.AppendLine("Disk is 48 TPI");
                sb.AppendFormat("Allocation bitmap descriptor starts at block {0}",
                                rbfSb.dd_maplsn == 0 ? 1 : rbfSb.dd_maplsn).AppendLine();
                sb.AppendFormat("{0} bytes in allocation bitmap", rbfSb.dd_map).AppendLine();
                if(LSNToUInt32(rbfSb.dd_bt) > 0 && rbfSb.dd_bsz > 0)
                    sb.AppendFormat("Boot file starts at block {0} and has {1} bytes", LSNToUInt32(rbfSb.dd_bt),
                                    rbfSb.dd_bsz).AppendLine();
                sb.AppendFormat("Root directory descriptor starts at block {0}", LSNToUInt32(rbfSb.dd_dir))
                  .AppendLine();
                sb.AppendFormat("Disk is owned by user {0}", rbfSb.dd_own).AppendLine();
                sb.AppendFormat("Volume was created on {0}", DateHandlers.Os9ToDateTime(rbfSb.dd_dat)).AppendLine();
                sb.AppendFormat("Volume attributes: {0:X2}", rbfSb.dd_att).AppendLine();
                sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(rbfSb.dd_nam, Encoding)).AppendLine();
                sb.AppendFormat("Path descriptor options: {0}", StringHandlers.CToString(rbfSb.dd_opt, Encoding))
                  .AppendLine();

                XmlFsType = new FileSystemType
                {
                    Type                  = "OS-9 Random Block File",
                    Bootable              = LSNToUInt32(rbfSb.dd_bt) > 0 && rbfSb.dd_bsz > 0,
                    ClusterSize           = rbfSb.dd_bit * (256 << rbfSb.dd_lsnsize),
                    Clusters              = LSNToUInt32(rbfSb.dd_tot),
                    CreationDate          = DateHandlers.Os9ToDateTime(rbfSb.dd_dat),
                    CreationDateSpecified = true,
                    VolumeName            = StringHandlers.CToString(rbfSb.dd_nam, Encoding),
                    VolumeSerial          = $"{rbfSb.dd_dsk:X4}"
                };
            }

            information = sb.ToString();
        }

        static uint LSNToUInt32(byte[] lsn)
        {
            if(lsn == null || lsn.Length != 3) return 0;

            return (uint)((lsn[0] << 16) + (lsn[1] << 8) + lsn[2]);
        }

        /// <summary>
        ///     Identification sector. Wherever the sector this resides on, becomes LSN 0.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RBF_IdSector
        {
            /// <summary>Sectors on disk</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] dd_tot;
            /// <summary>Tracks</summary>
            public byte dd_tks;
            /// <summary>Bytes in allocation map</summary>
            public ushort dd_map;
            /// <summary>Sectors per cluster</summary>
            public ushort dd_bit;
            /// <summary>LSN of root directory</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] dd_dir;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] dd_bt;
            /// <summary>Size of boot file</summary>
            public ushort dd_bsz;
            /// <summary>Creation date</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] dd_dat;
            /// <summary>Volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] dd_nam;
            /// <summary>Path options</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] dd_opt;
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
        ///     Identification sector. Wherever the sector this resides on, becomes LSN 0.
        ///     Introduced on OS-9000, this can be big or little endian.
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rid_name;
            /// <summary>Endian flag</summary>
            public byte rid_endflag;
            /// <summary>Padding</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] rid_unused2;
            /// <summary>Parity</summary>
            public uint rid_parity;
        }
    }
}