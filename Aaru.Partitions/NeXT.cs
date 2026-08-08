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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

// Information learnt from XNU source and testing against real disks

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Attributes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of NeXT disklabels</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class NeXTDisklabel : IPartition
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
    const string MODULE_NAME = "NeXT disklabel plugin";

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.NeXTDisklabel_Name;

    /// <inheritdoc />
    public Guid Id => new("246A6D93-4F1A-1F8A-344D-50187A5513A9");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        var    magicFound = false;
        byte[] labelSector;

        uint sectorSize = imagePlugin.Info.SectorSize is 2352 or 2448 ? 2048 : imagePlugin.Info.SectorSize;

        partitions = [];

        ulong       labelPosition = 0;
        ErrorNumber errno;

        foreach(ulong i in new ulong[]
                {
                    0, 4, 15, 16
                }.TakeWhile(i => i + sectorOffset < imagePlugin.Info.Sectors))
        {
            errno = imagePlugin.ReadSector(i + sectorOffset, false, out labelSector, out _);

            if(errno != ErrorNumber.NoError) continue;

            var magic = BigEndianBitConverter.ToUInt32(labelSector, 0x00);

            if(magic != NEXT_MAGIC1 && magic != NEXT_MAGIC2 && magic != NEXT_MAGIC3) continue;

            magicFound    = true;
            labelPosition = i + sectorOffset;

            break;
        }

        if(!magicFound) return false;

        uint sectorsToRead = 7680 / imagePlugin.Info.SectorSize;

        if(7680 % imagePlugin.Info.SectorSize > 0) sectorsToRead++;

        errno = imagePlugin.ReadSectors(labelPosition, false, sectorsToRead, out labelSector, out _);

        if(errno != ErrorNumber.NoError) return false;

        Label label    = Marshal.ByteArrayToStructureBigEndian<Label>(labelSector);
        var   disktabB = new byte[498];
        Array.Copy(labelSector, 44, disktabB, 0, 498);
        label.dl_dt              = Marshal.ByteArrayToStructureBigEndian<DiskTab>(disktabB);
        label.dl_dt.d_partitions = new Entry[8];

        AaruLogging.Debug(MODULE_NAME, "label.dl_version = 0x{0:X8}", label.dl_version);
        AaruLogging.Debug(MODULE_NAME, "label.dl_label_blkno = {0}",  label.dl_label_blkno);
        AaruLogging.Debug(MODULE_NAME, "label.dl_size = {0}",         label.dl_size);

        AaruLogging.Debug(MODULE_NAME, "label.dl_label = \"{0}\"", StringHandlers.CToString(label.dl_label));

        AaruLogging.Debug(MODULE_NAME, "label.dl_flags = {0}",    label.dl_flags);
        AaruLogging.Debug(MODULE_NAME, "label.dl_tag = 0x{0:X8}", label.dl_tag);

        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_name = \"{0}\"", StringHandlers.CToString(label.dl_dt.d_name));

        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_type = \"{0}\"", StringHandlers.CToString(label.dl_dt.d_type));

        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_secsize = {0}",    label.dl_dt.d_secsize);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_ntracks = {0}",    label.dl_dt.d_ntracks);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_nsectors = {0}",   label.dl_dt.d_nsectors);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_ncylinders = {0}", label.dl_dt.d_ncylinders);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_rpm = {0}",        label.dl_dt.d_rpm);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_front = {0}",      label.dl_dt.d_front);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_back = {0}",       label.dl_dt.d_back);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_ngroups = {0}",    label.dl_dt.d_ngroups);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_ag_size = {0}",    label.dl_dt.d_ag_size);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_ag_alts = {0}",    label.dl_dt.d_ag_alts);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_ag_off = {0}",     label.dl_dt.d_ag_off);

        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_boot0_blkno[0] = {0}", label.dl_dt.d_boot0_blkno[0]);

        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_boot0_blkno[1] = {0}", label.dl_dt.d_boot0_blkno[1]);

        AaruLogging.Debug(MODULE_NAME,
                          "label.dl_dt.d_bootfile = \"{0}\"",
                          StringHandlers.CToString(label.dl_dt.d_bootfile));

        AaruLogging.Debug(MODULE_NAME,
                          "label.dl_dt.d_hostname = \"{0}\"",
                          StringHandlers.CToString(label.dl_dt.d_hostname));

        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_rootpartition = {0}", label.dl_dt.d_rootpartition);
        AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_rwpartition = {0}",   label.dl_dt.d_rwpartition);

        for(var i = 0; i < 8; i++)
        {
            var partB = new byte[44];
            Array.Copy(labelSector, 44 + 146 + 44 * i, partB, 0, 44);
            label.dl_dt.d_partitions[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(partB);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_base = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_base);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_size = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_size);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_bsize = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_bsize);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_fsize = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_fsize);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_opt = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_opt);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_cpg = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_cpg);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_density = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_density);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_minfree = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_minfree);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_newfs = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_newfs);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_mountpt = \"{1}\"",
                              i,
                              StringHandlers.CToString(label.dl_dt.d_partitions[i].p_mountpt));

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_automnt = {1}",
                              i,
                              label.dl_dt.d_partitions[i].p_automnt);

            AaruLogging.Debug(MODULE_NAME,
                              "label.dl_dt.d_partitions[{0}].p_type = \"{1}\"",
                              i,
                              StringHandlers.CToString(label.dl_dt.d_partitions[i].p_type));

            if(label.dl_dt.d_partitions[i].p_size  <= 0 ||
               label.dl_dt.d_partitions[i].p_base  < 0  ||
               label.dl_dt.d_partitions[i].p_bsize < 0)
                continue;

            var sb = new StringBuilder();

            var part = new Partition
            {
                Size     = (ulong)label.dl_dt.d_partitions[i].p_size * (uint)label.dl_dt.d_secsize,
                Type     = StringHandlers.CToString(label.dl_dt.d_partitions[i].p_type),
                Sequence = (ulong)i,
                Name     = StringHandlers.CToString(label.dl_dt.d_partitions[i].p_mountpt),
                Length = (ulong)label.dl_dt.d_partitions[i].p_size *
                         (uint)label.dl_dt.d_secsize /
                         sectorSize,
                Offset = ((ulong)label.dl_dt.d_partitions[i].p_base + (uint)label.dl_dt.d_front) *
                         (uint)label.dl_dt.d_secsize +
                         sectorOffset * sectorSize,
                Start = ((ulong)label.dl_dt.d_partitions[i].p_base + (uint)label.dl_dt.d_front) *
                        (uint)label.dl_dt.d_secsize /
                        sectorSize +
                        sectorOffset,
                Scheme = Name
            };

            if(part.Start >= imagePlugin.Info.Sectors)
            {
                AaruLogging.Debug(MODULE_NAME, Localization.Partition_bigger_than_device_reducing);

                continue;
            }

            if(part.Start + part.Length > imagePlugin.Info.Sectors)
            {
                AaruLogging.Debug(MODULE_NAME, Localization.Partition_bigger_than_device_reducing);
                part.Length = imagePlugin.Info.Sectors - part.Start;
                part.Size   = part.Length * sectorSize;

                AaruLogging.Debug(MODULE_NAME, "label.dl_dt.d_partitions[{0}].p_size = {1}", i, part.Length);
            }

            sb.AppendFormat(Localization._0_bytes_per_block,    label.dl_dt.d_partitions[i].p_bsize).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_fragment, label.dl_dt.d_partitions[i].p_fsize).AppendLine();

            if(label.dl_dt.d_partitions[i].p_opt == 's')
                sb.AppendLine(Localization.Space_optimized);
            else if(label.dl_dt.d_partitions[i].p_opt == 't')
                sb.AppendLine(Localization.Time_optimized);
            else
                sb.AppendFormat(Localization.Unknown_optimization_0_X2, label.dl_dt.d_partitions[i].p_opt).AppendLine();

            sb.AppendFormat(Localization._0_cylinders_per_group, label.dl_dt.d_partitions[i].p_cpg).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_inode,     label.dl_dt.d_partitions[i].p_density).AppendLine();

            sb.AppendFormat(Localization._0_of_space_must_be_free_at_minimum, label.dl_dt.d_partitions[i].p_minfree)
              .AppendLine();

            if(label.dl_dt.d_partitions[i].p_newfs != 1)
                sb.AppendLine(Localization.Filesystem_should_be_formatted_at_start);

            if(label.dl_dt.d_partitions[i].p_automnt == 1)
                sb.AppendLine(Localization.Filesystem_should_be_automatically_mounted);

            part.Description = sb.ToString();

            partitions.Add(part);
        }

        return true;
    }

#endregion

#region Nested type: DiskTab

    /// <summary>NeXT disktab and partitions, 498 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct DiskTab
    {
        /// <summary>Drive name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] d_name;
        /// <summary>Drive type</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] d_type;
        /// <summary>Sector size</summary>
        public int d_secsize;
        /// <summary>tracks/cylinder</summary>
        public int d_ntracks;
        /// <summary>sectors/track</summary>
        public int d_nsectors;
        /// <summary>cylinders</summary>
        public int d_ncylinders;
        /// <summary>revolutions/minute</summary>
        public int d_rpm;
        /// <summary>size of front porch in sectors</summary>
        public short d_front;
        /// <summary>size of back porch in sectors</summary>
        public short d_back;
        /// <summary>number of alt groups</summary>
        public short d_ngroups;
        /// <summary>alt group size in sectors</summary>
        public short d_ag_size;
        /// <summary>alternate sectors per alt group</summary>
        public short d_ag_alts;
        /// <summary>sector offset to first alternate</summary>
        public short d_ag_off;
        /// <summary>"blk 0" boot locations</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] d_boot0_blkno;
        /// <summary>default bootfile</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] d_bootfile;
        /// <summary>host name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] d_hostname;
        /// <summary>root partition</summary>
        public byte d_rootpartition;
        /// <summary>r/w partition</summary>
        public byte d_rwpartition;
        /// <summary>partitions</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public Entry[] d_partitions;
    }

#endregion

#region Nested type: Entry

    /// <summary>Partition entries, 44 bytes each</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Entry
    {
        /// <summary>Sector of start, counting from front porch</summary>
        public int p_base;
        /// <summary>Length in sectors</summary>
        public int p_size;
        /// <summary>Filesystem's block size</summary>
        public short p_bsize;
        /// <summary>Filesystem's fragment size</summary>
        public short p_fsize;
        /// <summary>'s'pace or 't'ime</summary>
        public byte p_opt;
        /// <summary>Cylinders per group</summary>
        public short p_cpg;
        /// <summary>Bytes per inode</summary>
        public short p_density;
        /// <summary>% of minimum free space</summary>
        public byte p_minfree;
        /// <summary>Should newfs be run on first start?</summary>
        public byte p_newfs;
        /// <summary>Mount point or empty if mount where you want</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] p_mountpt;
        /// <summary>Should automount</summary>
        public byte p_automnt;
        /// <summary>Filesystem type, always "4.3BSD"?</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] p_type;
    }

#endregion

#region Nested type: Label

    /// <summary>NeXT v3 disklabel, 544 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Label
    {
        /// <summary>Signature</summary>
        public uint dl_version;
        /// <summary>Block on which this label resides</summary>
        public int dl_label_blkno;
        /// <summary>Device size in blocks</summary>
        public int dl_size;
        /// <summary>Device name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] dl_label;
        /// <summary>Device flags</summary>
        public uint dl_flags;
        /// <summary>Device tag</summary>
        public uint dl_tag;
        /// <summary>Device info and partitions</summary>
        public DiskTab dl_dt;
        /// <summary>Checksum</summary>
        public ushort dl_v3_checksum;
    }

#endregion

#region Nested type: LabelOld

    /// <summary>NeXT v1 and v2 disklabel, 7224 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct LabelOld
    {
        /// <summary>Signature</summary>
        public uint dl_version;
        /// <summary>Block on which this label resides</summary>
        public int dl_label_blkno;
        /// <summary>Device size in blocks</summary>
        public int dl_size;
        /// <summary>Device name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] dl_label;
        /// <summary>Device flags</summary>
        public uint dl_flags;
        /// <summary>Device tag</summary>
        public uint dl_tag;
        /// <summary>Device info and partitions</summary>
        public DiskTab dl_dt;
        /// <summary>Bad sector table</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1670)]
        public int[] dl_bad;
        /// <summary>Checksum</summary>
        public ushort dl_checksum;
    }

#endregion
}