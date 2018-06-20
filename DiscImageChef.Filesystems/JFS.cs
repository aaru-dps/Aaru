// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : JFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the IBM JFS filesystem and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class JFS : IFilesystem
    {
        const uint JFS_BOOT_BLOCKS_SIZE = 0x8000;
        const uint JFS_MAGIC = 0x3153464A;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding Encoding { get; private set; }
        public string Name => "JFS Plugin";
        public Guid Id => new Guid("D3BE2A41-8F28-4055-94DC-BB6C72A0E9C4");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            uint bootSectors = JFS_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;
            if(partition.Start + bootSectors >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start + bootSectors);
            if(sector.Length < 512) return false;

            JfsSuperBlock jfsSb = new JfsSuperBlock();
            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(jfsSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(jfsSb));
            jfsSb = (JfsSuperBlock)Marshal.PtrToStructure(sbPtr, typeof(JfsSuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            return jfsSb.s_magic == JFS_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            StringBuilder sb = new StringBuilder();
            uint bootSectors = JFS_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;
            byte[] sector = imagePlugin.ReadSector(partition.Start + bootSectors);
            if(sector.Length < 512) return;

            JfsSuperBlock jfsSb = new JfsSuperBlock();
            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(jfsSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(jfsSb));
            jfsSb = (JfsSuperBlock)Marshal.PtrToStructure(sbPtr, typeof(JfsSuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            sb.AppendLine("JFS filesystem");
            sb.AppendFormat("Version {0}", jfsSb.s_version).AppendLine();
            sb.AppendFormat("{0} blocks of {1} bytes", jfsSb.s_size, jfsSb.s_bsize).AppendLine();
            sb.AppendFormat("{0} blocks per allocation group", jfsSb.s_agsize).AppendLine();

            if(jfsSb.s_flags.HasFlag(JfsFlags.Unicode)) sb.AppendLine("Volume uses Unicode for directory entries");
            if(jfsSb.s_flags.HasFlag(JfsFlags.RemountRO)) sb.AppendLine("Volume remounts read-only on error");
            if(jfsSb.s_flags.HasFlag(JfsFlags.Continue)) sb.AppendLine("Volume continues on error");
            if(jfsSb.s_flags.HasFlag(JfsFlags.Panic)) sb.AppendLine("Volume panics on error");
            if(jfsSb.s_flags.HasFlag(JfsFlags.UserQuota)) sb.AppendLine("Volume has user quotas enabled");
            if(jfsSb.s_flags.HasFlag(JfsFlags.GroupQuota)) sb.AppendLine("Volume has group quotas enabled");
            if(jfsSb.s_flags.HasFlag(JfsFlags.NoJournal)) sb.AppendLine("Volume is not using any journal");
            if(jfsSb.s_flags.HasFlag(JfsFlags.Discard))
                sb.AppendLine("Volume sends TRIM/UNMAP commands to underlying device");
            if(jfsSb.s_flags.HasFlag(JfsFlags.GroupCommit)) sb.AppendLine("Volume commits in groups of 1");
            if(jfsSb.s_flags.HasFlag(JfsFlags.LazyCommit)) sb.AppendLine("Volume commits lazy");
            if(jfsSb.s_flags.HasFlag(JfsFlags.Temporary)) sb.AppendLine("Volume does not commit to log");
            if(jfsSb.s_flags.HasFlag(JfsFlags.InlineLog)) sb.AppendLine("Volume has log withing itself");
            if(jfsSb.s_flags.HasFlag(JfsFlags.InlineMoving))
                sb.AppendLine("Volume has log withing itself and is moving it out");
            if(jfsSb.s_flags.HasFlag(JfsFlags.BadSAIT)) sb.AppendLine("Volume has bad current secondary ait");
            if(jfsSb.s_flags.HasFlag(JfsFlags.Sparse)) sb.AppendLine("Volume supports sparse files");
            if(jfsSb.s_flags.HasFlag(JfsFlags.DASDEnabled)) sb.AppendLine("Volume has DASD limits enabled");
            if(jfsSb.s_flags.HasFlag(JfsFlags.DASDPrime)) sb.AppendLine("Volume primes DASD on boot");
            if(jfsSb.s_flags.HasFlag(JfsFlags.SwapBytes)) sb.AppendLine("Volume is in a big-endian system");
            if(jfsSb.s_flags.HasFlag(JfsFlags.DirIndex)) sb.AppendLine("Volume has presistent indexes");
            if(jfsSb.s_flags.HasFlag(JfsFlags.Linux)) sb.AppendLine("Volume supports Linux");
            if(jfsSb.s_flags.HasFlag(JfsFlags.DFS)) sb.AppendLine("Volume supports DCE DFS LFS");
            if(jfsSb.s_flags.HasFlag(JfsFlags.OS2)) sb.AppendLine("Volume supports OS/2, and is case insensitive");
            if(jfsSb.s_flags.HasFlag(JfsFlags.AIX)) sb.AppendLine("Volume supports AIX");
            if(jfsSb.s_state != 0) sb.AppendLine("Volume is dirty");
            sb.AppendFormat("Volume was last updated on {0}",
                            DateHandlers.UnixUnsignedToDateTime(jfsSb.s_time.tv_sec, jfsSb.s_time.tv_nsec))
              .AppendLine();
            if(jfsSb.s_version == 1)
                sb.AppendFormat("Volume name: {0}", Encoding.GetString(jfsSb.s_fpack)).AppendLine();
            else sb.AppendFormat("Volume name: {0}", Encoding.GetString(jfsSb.s_label)).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", jfsSb.s_uuid).AppendLine();

            XmlFsType = new FileSystemType
            {
                Type = "JFS filesystem",
                Clusters = (long)jfsSb.s_size,
                ClusterSize = (int)jfsSb.s_bsize,
                Bootable = true,
                VolumeName = Encoding.GetString(jfsSb.s_version == 1 ? jfsSb.s_fpack : jfsSb.s_label),
                VolumeSerial = $"{jfsSb.s_uuid}",
                ModificationDate = DateHandlers.UnixUnsignedToDateTime(jfsSb.s_time.tv_sec, jfsSb.s_time.tv_nsec),
                ModificationDateSpecified = true
            };
            if(jfsSb.s_state != 0) XmlFsType.Dirty = true;

            information = sb.ToString();
        }

        [Flags]
        enum JfsFlags : uint
        {
            Unicode = 0x00000001,
            RemountRO = 0x00000002,
            Continue = 0x00000004,
            Panic = 0x00000008,
            UserQuota = 0x00000010,
            GroupQuota = 0x00000020,
            NoJournal = 0x00000040,
            Discard = 0x00000080,
            GroupCommit = 0x00000100,
            LazyCommit = 0x00000200,
            Temporary = 0x00000400,
            InlineLog = 0x00000800,
            InlineMoving = 0x00001000,
            BadSAIT = 0x00010000,
            Sparse = 0x00020000,
            DASDEnabled = 0x00040000,
            DASDPrime = 0x00080000,
            SwapBytes = 0x00100000,
            DirIndex = 0x00200000,
            Linux = 0x10000000,
            DFS = 0x20000000,
            OS2 = 0x40000000,
            AIX = 0x80000000
        }

        [Flags]
        enum JfsState : uint
        {
            Clean = 0,
            Mounted = 1,
            Dirty = 2,
            Logredo = 4,
            Extendfs = 8
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct JfsExtent
        {
            /// <summary>
            ///     Leftmost 24 bits are extent length, rest 8 bits are most significant for <see cref="addr2" />
            /// </summary>
            public uint len_addr;
            public uint addr2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct JfsTimeStruct
        {
            public uint tv_sec;
            public uint tv_nsec;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct JfsSuperBlock
        {
            public uint s_magic;
            public uint s_version;
            public ulong s_size;
            public uint s_bsize;
            public ushort s_l2bsize;
            public ushort s_l2bfactor;
            public uint s_pbsize;
            public ushort s_l1pbsize;
            public ushort pad;
            public uint s_agsize;
            public JfsFlags s_flags;
            public JfsState s_state;
            public uint s_compress;
            public JfsExtent s_ait2;
            public JfsExtent s_aim2;
            public uint s_logdev;
            public uint s_logserial;
            public JfsExtent s_logpxd;
            public JfsExtent s_fsckpxd;
            public JfsTimeStruct s_time;
            public uint s_fsckloglen;
            public sbyte s_fscklog;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] s_fpack;
            public ulong s_xsize;
            public JfsExtent s_xfsckpxd;
            public JfsExtent s_xlogpxd;
            public Guid s_uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] s_label;
            public Guid s_loguuid;
        }
    }
}