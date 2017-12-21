// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SGI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages SGI DVHs (Disk Volume Headers).
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
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Partitions
{
    public class SGI : PartitionPlugin
    {
        const int SGI_MAGIC = 0x0BE5A941;

        public SGI()
        {
            Name = "SGI Disk Volume Header";
            PluginUuid = new Guid("AEF5AB45-4880-4CE8-8735-F0A402E2E5F2");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(sectorOffset);
            if(sector.Length < 512) return false;

            SGILabel dvh = BigEndianMarshal.ByteArrayToStructureBigEndian<SGILabel>(sector);
            for(int i = 0; i < dvh.volume.Length; i++)
                dvh.volume[i] = BigEndianMarshal.SwapStructureMembersEndian(dvh.volume[i]);
            for(int i = 0; i < dvh.partitions.Length; i++)
                dvh.partitions[i] = BigEndianMarshal.SwapStructureMembersEndian(dvh.partitions[i]);

            dvh.device_params = BigEndianMarshal.SwapStructureMembersEndian(dvh.device_params);

            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.magic = 0x{0:X8} (should be 0x{1:X8})", dvh.magic,
                                      SGI_MAGIC);

            if(dvh.magic != SGI_MAGIC) return false;

            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.root_part_num = {0}", dvh.root_part_num);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.swap_part_num = {0}", dvh.swap_part_num);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.boot_file = \"{0}\"",
                                      StringHandlers.CToString(dvh.boot_file));
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_skew = {0}", dvh.device_params.dp_skew);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_gap1 = {0}", dvh.device_params.dp_gap1);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_gap2 = {0}", dvh.device_params.dp_gap2);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_spares_cyl = {0}",
                                      dvh.device_params.dp_spares_cyl);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_cyls = {0}", dvh.device_params.dp_cyls);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_shd0 = {0}", dvh.device_params.dp_shd0);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_trks0 = {0}", dvh.device_params.dp_trks0);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_ctq_depth = {0}",
                                      dvh.device_params.dp_ctq_depth);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_cylshi = {0}", dvh.device_params.dp_cylshi);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_secs = {0}", dvh.device_params.dp_secs);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_secbytes = {0}",
                                      dvh.device_params.dp_secbytes);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_interleave = {0}",
                                      dvh.device_params.dp_interleave);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_flags = {0}", dvh.device_params.dp_flags);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_datarate = {0}",
                                      dvh.device_params.dp_datarate);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_nretries = {0}",
                                      dvh.device_params.dp_nretries);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_mspw = {0}", dvh.device_params.dp_mspw);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_xgap1 = {0}", dvh.device_params.dp_xgap1);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_xsync = {0}", dvh.device_params.dp_xsync);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_xrdly = {0}", dvh.device_params.dp_xrdly);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_xgap2 = {0}", dvh.device_params.dp_xgap2);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_xrgate = {0}", dvh.device_params.dp_xrgate);
            DicConsole.DebugWriteLine("SGIVH plugin", "dvh.device_params.dp_xwcont = {0}", dvh.device_params.dp_xwcont);

            ulong counter = 0;

            for(int i = 0; i < dvh.partitions.Length; i++)
            {
                DicConsole.DebugWriteLine("SGIVH plugin", "dvh.partitions[{0}].num_blocks = {1}", i,
                                          dvh.partitions[i].num_blocks);
                DicConsole.DebugWriteLine("SGIVH plugin", "dvh.partitions[{0}].first_block = {1}", i,
                                          dvh.partitions[i].first_block);
                // TODO: Solve big endian marshal with enumerations
                dvh.partitions[i].type = (SGIType)Swapping.Swap((uint)dvh.partitions[i].type);
                DicConsole.DebugWriteLine("SGIVH plugin", "dvh.partitions[{0}].type = {1}", i, dvh.partitions[i].type);

                Partition part = new Partition
                {
                    Start =
                        dvh.partitions[i].first_block * dvh.device_params.dp_secbytes / imagePlugin.GetSectorSize(),
                    Offset = dvh.partitions[i].first_block * dvh.device_params.dp_secbytes,
                    Length =
                        dvh.partitions[i].num_blocks * dvh.device_params.dp_secbytes / imagePlugin.GetSectorSize(),
                    Size = dvh.partitions[i].num_blocks * dvh.device_params.dp_secbytes,
                    Type = TypeToString(dvh.partitions[i].type),
                    Sequence = counter,
                    Scheme = Name
                };
                if(part.Size > 0 && dvh.partitions[i].type != SGIType.Header && dvh.partitions[i].type != SGIType.Volume
                )
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SGILabel
        {
            /// <summary></summary>
            public uint magic;
            /// <summary></summary>
            public short root_part_num;
            /// <summary></summary>
            public short swap_part_num;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] boot_file;
            /// <summary></summary>
            public SGIDeviceParameters device_params;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)] public SGIVolume[] volume;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public SGIPartition[] partitions;
            /// <summary></summary>
            public uint csum;
            /// <summary></summary>
            public uint padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SGIVolume
        {
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] name;
            /// <summary></summary>
            public uint block_num;
            /// <summary></summary>
            public uint num_bytes;
        }

        enum SGIType : uint
        {
            Header = 0,
            TrkRepl = 1,
            SecRepl = 2,
            Swap = 3,
            Bsd = 4,
            SystemV = 5,
            Volume = 6,
            EFS = 7,
            Lvol = 8,
            Rlvol = 9,
            XFS = 0xA,
            Xlvol = 0xB,
            Rxlvol = 0xC,
            Xvm = 0x0D,
            LinuxSwap = 0x82,
            Linux = 0x83,
            LinuxRAID = 0xFD
        }

        static string TypeToString(SGIType typ)
        {
            switch(typ)
            {
                case SGIType.Header: return "Volume header";
                case SGIType.TrkRepl: return "Track replacements";
                case SGIType.SecRepl: return "Sector replacements";
                case SGIType.Swap: return "Raw data (swap)";
                case SGIType.Bsd: return "4.2BSD Fast File System";
                case SGIType.SystemV: return "UNIX System V";
                case SGIType.Volume: return "Whole device";
                case SGIType.EFS: return "EFS";
                case SGIType.Lvol: return "Logical volume";
                case SGIType.Rlvol: return "Raw logical volume";
                case SGIType.XFS: return "XFS";
                case SGIType.Xlvol: return "XFS log device";
                case SGIType.Rxlvol: return "XLV volume";
                case SGIType.Xvm: return "SGI XVM";
                case SGIType.LinuxSwap: return "Linux swap";
                case SGIType.Linux: return "Linux";
                case SGIType.LinuxRAID: return "Linux RAID";
                default: return "Unknown";
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SGIPartition
        {
            /// <summary></summary>
            public uint num_blocks;
            /// <summary></summary>
            public uint first_block;
            /// <summary></summary>
            public SGIType type;
        }

        struct SGIDeviceParameters
        {
            public byte dp_skew;
            public byte dp_gap1;
            public byte dp_gap2;
            public byte dp_spares_cyl;
            public ushort dp_cyls;
            public ushort dp_shd0;
            public ushort dp_trks0;
            public byte dp_ctq_depth;
            public byte dp_cylshi;
            public ushort dp_unused;
            public ushort dp_secs;
            public ushort dp_secbytes;
            public ushort dp_interleave;
            public uint dp_flags;
            public uint dp_datarate;
            public uint dp_nretries;
            public uint dp_mspw;
            public ushort dp_xgap1;
            public ushort dp_xsync;
            public ushort dp_xrdly;
            public ushort dp_xgap2;
            public ushort dp_xrgate;
            public ushort dp_xwcont;
        }
    }
}