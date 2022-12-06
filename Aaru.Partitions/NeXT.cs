// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NeXT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages NeXTStep and OpenStep disklabels.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

// Information learnt from XNU source and testing against real disks
namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of NeXT disklabels</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
    public sealed class NeXTDisklabel : IPartition
    {
        /// <summary>"NeXT"</summary>
        const uint NEXT_MAGIC1 = 0x4E655854;
        /// <summary>"dlV2"</summary>
        const uint NEXT_MAGIC2 = 0x646C5632;
        /// <summary>"dlV3"</summary>
        const uint NEXT_MAGIC3 = 0x646C5633;
        /// <summary>180</summary>
        const ushort DISKTAB_START = 0xB4;
        /// <summary>44</summary>
        const ushort DISKTAB_ENTRY_SIZE = 0x2C;

        /// <inheritdoc />
        public string Name => "NeXT Disklabel";
        /// <inheritdoc />
        public Guid Id => new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            bool   magicFound = false;
            byte[] labelSector;

            uint sectorSize;

            if(imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448)
                sectorSize = 2048;
            else
                sectorSize = imagePlugin.Info.SectorSize;

            partitions = new List<Partition>();

            ulong labelPosition = 0;

            foreach(ulong i in new ulong[]
            {
                0, 4, 15, 16
            }.TakeWhile(i => i + sectorOffset < imagePlugin.Info.Sectors))
            {
                labelSector = imagePlugin.ReadSector(i + sectorOffset);
                uint magic = BigEndianBitConverter.ToUInt32(labelSector, 0x00);

                if(magic != NEXT_MAGIC1 &&
                   magic != NEXT_MAGIC2 &&
                   magic != NEXT_MAGIC3)
                    continue;

                magicFound    = true;
                labelPosition = i + sectorOffset;

                break;
            }

            if(!magicFound)
                return false;

            uint sectorsToRead = 7680 / imagePlugin.Info.SectorSize;

            if(7680 % imagePlugin.Info.SectorSize > 0)
                sectorsToRead++;

            labelSector = imagePlugin.ReadSectors(labelPosition, sectorsToRead);

            Label  label    = Marshal.ByteArrayToStructureBigEndian<Label>(labelSector);
            byte[] disktabB = new byte[498];
            Array.Copy(labelSector, 44, disktabB, 0, 498);
            label.dl_dt              = Marshal.ByteArrayToStructureBigEndian<DiskTab>(disktabB);
            label.dl_dt.d_partitions = new Entry[8];

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_version = 0x{0:X8}", label.dl_version);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_label_blkno = {0}", label.dl_label_blkno);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_size = {0}", label.dl_size);

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_label = \"{0}\"",
                                       StringHandlers.CToString(label.dl_label));

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_flags = {0}", label.dl_flags);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_tag = 0x{0:X8}", label.dl_tag);

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_name = \"{0}\"",
                                       StringHandlers.CToString(label.dl_dt.d_name));

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_type = \"{0}\"",
                                       StringHandlers.CToString(label.dl_dt.d_type));

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_secsize = {0}", label.dl_dt.d_secsize);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_ntracks = {0}", label.dl_dt.d_ntracks);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_nsectors = {0}", label.dl_dt.d_nsectors);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_ncylinders = {0}", label.dl_dt.d_ncylinders);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_rpm = {0}", label.dl_dt.d_rpm);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_front = {0}", label.dl_dt.d_front);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_back = {0}", label.dl_dt.d_back);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_ngroups = {0}", label.dl_dt.d_ngroups);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_ag_size = {0}", label.dl_dt.d_ag_size);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_ag_alts = {0}", label.dl_dt.d_ag_alts);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_ag_off = {0}", label.dl_dt.d_ag_off);

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_boot0_blkno[0] = {0}",
                                       label.dl_dt.d_boot0_blkno[0]);

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_boot0_blkno[1] = {0}",
                                       label.dl_dt.d_boot0_blkno[1]);

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_bootfile = \"{0}\"",
                                       StringHandlers.CToString(label.dl_dt.d_bootfile));

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_hostname = \"{0}\"",
                                       StringHandlers.CToString(label.dl_dt.d_hostname));

            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_rootpartition = {0}", label.dl_dt.d_rootpartition);
            AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_rwpartition = {0}", label.dl_dt.d_rwpartition);

            for(int i = 0; i < 8; i++)
            {
                byte[] partB = new byte[44];
                Array.Copy(labelSector, 44 + 146 + (44 * i), partB, 0, 44);
                label.dl_dt.d_partitions[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(partB);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_base = {1}", i,
                                           label.dl_dt.d_partitions[i].p_base);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_size = {1}", i,
                                           label.dl_dt.d_partitions[i].p_size);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_bsize = {1}", i,
                                           label.dl_dt.d_partitions[i].p_bsize);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_fsize = {1}", i,
                                           label.dl_dt.d_partitions[i].p_fsize);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_opt = {1}", i,
                                           label.dl_dt.d_partitions[i].p_opt);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_cpg = {1}", i,
                                           label.dl_dt.d_partitions[i].p_cpg);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_density = {1}", i,
                                           label.dl_dt.d_partitions[i].p_density);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_minfree = {1}", i,
                                           label.dl_dt.d_partitions[i].p_minfree);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_newfs = {1}", i,
                                           label.dl_dt.d_partitions[i].p_newfs);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_mountpt = \"{1}\"", i,
                                           StringHandlers.CToString(label.dl_dt.d_partitions[i].p_mountpt));

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_automnt = {1}", i,
                                           label.dl_dt.d_partitions[i].p_automnt);

                AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_type = \"{1}\"", i,
                                           StringHandlers.CToString(label.dl_dt.d_partitions[i].p_type));

                if(label.dl_dt.d_partitions[i].p_size  <= 0 ||
                   label.dl_dt.d_partitions[i].p_base  < 0  ||
                   label.dl_dt.d_partitions[i].p_bsize < 0)
                    continue;

                var sb = new StringBuilder();

                var part = new Partition
                {
                    Size = (ulong)(label.dl_dt.d_partitions[i].p_size * label.dl_dt.d_secsize),
                    Offset =
                        (ulong)((label.dl_dt.d_partitions[i].p_base + label.dl_dt.d_front) * label.dl_dt.d_secsize),
                    Type     = StringHandlers.CToString(label.dl_dt.d_partitions[i].p_type),
                    Sequence = (ulong)i,
                    Name     = StringHandlers.CToString(label.dl_dt.d_partitions[i].p_mountpt),
                    Length   = (ulong)(label.dl_dt.d_partitions[i].p_size * label.dl_dt.d_secsize / sectorSize),
                    Start = (ulong)((label.dl_dt.d_partitions[i].p_base + label.dl_dt.d_front) * label.dl_dt.d_secsize /
                                    sectorSize),
                    Scheme = Name
                };

                if(part.Start + part.Length > imagePlugin.Info.Sectors)
                {
                    AaruConsole.DebugWriteLine("NeXT Plugin", "Partition bigger than device, reducing...");
                    part.Length = imagePlugin.Info.Sectors - part.Start;
                    part.Size   = part.Length * sectorSize;

                    AaruConsole.DebugWriteLine("NeXT Plugin", "label.dl_dt.d_partitions[{0}].p_size = {1}", i,
                                               part.Length);
                }

                sb.AppendFormat("{0} bytes per block", label.dl_dt.d_partitions[i].p_bsize).AppendLine();
                sb.AppendFormat("{0} bytes per fragment", label.dl_dt.d_partitions[i].p_fsize).AppendLine();

                if(label.dl_dt.d_partitions[i].p_opt == 's')
                    sb.AppendLine("Space optimized");
                else if(label.dl_dt.d_partitions[i].p_opt == 't')
                    sb.AppendLine("Time optimized");
                else
                    sb.AppendFormat("Unknown optimization {0:X2}", label.dl_dt.d_partitions[i].p_opt).AppendLine();

                sb.AppendFormat("{0} cylinders per group", label.dl_dt.d_partitions[i].p_cpg).AppendLine();
                sb.AppendFormat("{0} bytes per inode", label.dl_dt.d_partitions[i].p_density).AppendLine();

                sb.AppendFormat("{0}% of space must be free at minimum", label.dl_dt.d_partitions[i].p_minfree).
                   AppendLine();

                if(label.dl_dt.d_partitions[i].p_newfs != 1)
                    sb.AppendLine("Filesystem should be formatted at start");

                if(label.dl_dt.d_partitions[i].p_automnt == 1)
                    sb.AppendLine("Filesystem should be automatically mounted");

                part.Description = sb.ToString();

                partitions.Add(part);
            }

            return true;
        }

        /// <summary>NeXT v3 disklabel, 544 bytes</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Label
        {
            /// <summary>Signature</summary>
            public readonly uint dl_version;
            /// <summary>Block on which this label resides</summary>
            public readonly int dl_label_blkno;
            /// <summary>Device size in blocks</summary>
            public readonly int dl_size;
            /// <summary>Device name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] dl_label;
            /// <summary>Device flags</summary>
            public readonly uint dl_flags;
            /// <summary>Device tag</summary>
            public readonly uint dl_tag;
            /// <summary>Device info and partitions</summary>
            public DiskTab dl_dt;
            /// <summary>Checksum</summary>
            public readonly ushort dl_v3_checksum;
        }

        /// <summary>NeXT v1 and v2 disklabel, 7224 bytes</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct LabelOld
        {
            /// <summary>Signature</summary>
            public readonly uint dl_version;
            /// <summary>Block on which this label resides</summary>
            public readonly int dl_label_blkno;
            /// <summary>Device size in blocks</summary>
            public readonly int dl_size;
            /// <summary>Device name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] dl_label;
            /// <summary>Device flags</summary>
            public readonly uint dl_flags;
            /// <summary>Device tag</summary>
            public readonly uint dl_tag;
            /// <summary>Device info and partitions</summary>
            public readonly DiskTab dl_dt;
            /// <summary>Bad sector table</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1670)]
            public readonly int[] dl_bad;
            /// <summary>Checksum</summary>
            public readonly ushort dl_checksum;
        }

        /// <summary>NeXT disktab and partitions, 498 bytes</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DiskTab
        {
            /// <summary>Drive name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] d_name;
            /// <summary>Drive type</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] d_type;
            /// <summary>Sector size</summary>
            public readonly int d_secsize;
            /// <summary>tracks/cylinder</summary>
            public readonly int d_ntracks;
            /// <summary>sectors/track</summary>
            public readonly int d_nsectors;
            /// <summary>cylinders</summary>
            public readonly int d_ncylinders;
            /// <summary>revolutions/minute</summary>
            public readonly int d_rpm;
            /// <summary>size of front porch in sectors</summary>
            public readonly short d_front;
            /// <summary>size of back porch in sectors</summary>
            public readonly short d_back;
            /// <summary>number of alt groups</summary>
            public readonly short d_ngroups;
            /// <summary>alt group size in sectors</summary>
            public readonly short d_ag_size;
            /// <summary>alternate sectors per alt group</summary>
            public readonly short d_ag_alts;
            /// <summary>sector offset to first alternate</summary>
            public readonly short d_ag_off;
            /// <summary>"blk 0" boot locations</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly int[] d_boot0_blkno;
            /// <summary>default bootfile</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] d_bootfile;
            /// <summary>host name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] d_hostname;
            /// <summary>root partition</summary>
            public readonly byte d_rootpartition;
            /// <summary>r/w partition</summary>
            public readonly byte d_rwpartition;
            /// <summary>partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Entry[] d_partitions;
        }

        /// <summary>Partition entries, 44 bytes each</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        struct Entry
        {
            /// <summary>Sector of start, counting from front porch</summary>
            public readonly int p_base;
            /// <summary>Length in sectors</summary>
            public readonly int p_size;
            /// <summary>Filesystem's block size</summary>
            public readonly short p_bsize;
            /// <summary>Filesystem's fragment size</summary>
            public readonly short p_fsize;
            /// <summary>'s'pace or 't'ime</summary>
            public readonly byte p_opt;
            /// <summary>Cylinders per group</summary>
            public readonly short p_cpg;
            /// <summary>Bytes per inode</summary>
            public readonly short p_density;
            /// <summary>% of minimum free space</summary>
            public readonly byte p_minfree;
            /// <summary>Should newfs be run on first start?</summary>
            public readonly byte p_newfs;
            /// <summary>Mount point or empty if mount where you want</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] p_mountpt;
            /// <summary>Should automount</summary>
            public readonly byte p_automnt;
            /// <summary>Filesystem type, always "4.3BSD"?</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] p_type;
        }
    }
}