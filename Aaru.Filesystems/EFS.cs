// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : EFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Extent File System plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Extent File System and shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements identification for the SGI Extent FileSystem</summary>
public sealed class EFS : IFilesystem
{
    const uint EFS_MAGIC     = 0x00072959;
    const uint EFS_MAGIC_NEW = 0x0007295A;

    const string FS_TYPE = "efs";

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.EFS_Name;
    /// <inheritdoc />
    public Guid Id => new("52A43F90-9AF3-4391-ADFE-65598DEEABAB");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        // Misaligned
        if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
        {
            uint sbSize = (uint)((Marshal.SizeOf<Superblock>() + 0x200) / imagePlugin.Info.SectorSize);

            if((Marshal.SizeOf<Superblock>() + 0x200) % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return false;

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return false;

            byte[] sbpiece = new byte[Marshal.SizeOf<Superblock>()];

            Array.Copy(sector, 0x200, sbpiece, 0, Marshal.SizeOf<Superblock>());

            Superblock sb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

            AaruConsole.DebugWriteLine("EFS plugin", Localization.magic_at_0_equals_1_expected_2_or_3, 0x200,
                                       sb.sb_magic, EFS_MAGIC, EFS_MAGIC_NEW);

            if(sb.sb_magic is EFS_MAGIC or EFS_MAGIC_NEW)
                return true;
        }
        else
        {
            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + 1, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return false;

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return false;

            Superblock sb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

            AaruConsole.DebugWriteLine("EFS plugin", Localization.magic_at_0_equals_1_expected_2_or_3, 1, sb.sb_magic,
                                       EFS_MAGIC, EFS_MAGIC_NEW);

            if(sb.sb_magic is EFS_MAGIC or EFS_MAGIC_NEW)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        Superblock efsSb;

        // Misaligned
        if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
        {
            uint sbSize = (uint)((Marshal.SizeOf<Superblock>() + 0x400) / imagePlugin.Info.SectorSize);

            if((Marshal.SizeOf<Superblock>() + 0x400) % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return;

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            byte[] sbpiece = new byte[Marshal.SizeOf<Superblock>()];

            Array.Copy(sector, 0x200, sbpiece, 0, Marshal.SizeOf<Superblock>());

            efsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

            AaruConsole.DebugWriteLine("EFS plugin", Localization.magic_at_0_X3_equals_1_expected_2_or_3, 0x200,
                                       efsSb.sb_magic, EFS_MAGIC, EFS_MAGIC_NEW);
        }
        else
        {
            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + 1, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return;

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            efsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

            AaruConsole.DebugWriteLine("EFS plugin", Localization.magic_at_0_equals_1_expected_2_or_3, 1,
                                       efsSb.sb_magic, EFS_MAGIC, EFS_MAGIC_NEW);
        }

        if(efsSb.sb_magic != EFS_MAGIC &&
           efsSb.sb_magic != EFS_MAGIC_NEW)
            return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.SGI_extent_filesystem);

        if(efsSb.sb_magic == EFS_MAGIC_NEW)
            sb.AppendLine(Localization.New_version);

        sb.AppendFormat(Localization.Filesystem_size_0_basic_blocks, efsSb.sb_size).AppendLine();
        sb.AppendFormat(Localization.First_cylinder_group_starts_at_block_0, efsSb.sb_firstcg).AppendLine();
        sb.AppendFormat(Localization.Cylinder_group_size_0_basic_blocks, efsSb.sb_cgfsize).AppendLine();
        sb.AppendFormat(Localization._0_inodes_per_cylinder_group, efsSb.sb_cgisize).AppendLine();
        sb.AppendFormat(Localization._0_sectors_per_track, efsSb.sb_sectors).AppendLine();
        sb.AppendFormat(Localization._0_heads_per_cylinder, efsSb.sb_heads).AppendLine();
        sb.AppendFormat(Localization._0_cylinder_groups, efsSb.sb_ncg).AppendLine();
        sb.AppendFormat(Localization.Volume_created_on_0, DateHandlers.UnixToDateTime(efsSb.sb_time)).AppendLine();
        sb.AppendFormat(Localization._0_bytes_on_bitmap, efsSb.sb_bmsize).AppendLine();
        sb.AppendFormat(Localization._0_free_blocks, efsSb.sb_tfree).AppendLine();
        sb.AppendFormat(Localization._0_free_inodes, efsSb.sb_tinode).AppendLine();

        if(efsSb.sb_bmblock > 0)
            sb.AppendFormat(Localization.Bitmap_resides_at_block_0, efsSb.sb_bmblock).AppendLine();

        if(efsSb.sb_replsb > 0)
            sb.AppendFormat(Localization.Replacement_superblock_resides_at_block_0, efsSb.sb_replsb).AppendLine();

        if(efsSb.sb_lastinode > 0)
            sb.AppendFormat(Localization.Last_inode_allocated_0, efsSb.sb_lastinode).AppendLine();

        if(efsSb.sb_dirty > 0)
            sb.AppendLine(Localization.Volume_is_dirty);

        sb.AppendFormat(Localization.Checksum_0_X8, efsSb.sb_checksum).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(efsSb.sb_fname, Encoding)).AppendLine();
        sb.AppendFormat(Localization.Volume_pack_0, StringHandlers.CToString(efsSb.sb_fpack, Encoding)).AppendLine();

        information = sb.ToString();

        XmlFsType = new FileSystemType
        {
            Type                  = FS_TYPE,
            ClusterSize           = 512,
            Clusters              = (ulong)efsSb.sb_size,
            FreeClusters          = (ulong)efsSb.sb_tfree,
            FreeClustersSpecified = true,
            Dirty                 = efsSb.sb_dirty > 0,
            VolumeName            = StringHandlers.CToString(efsSb.sb_fname, Encoding),
            VolumeSerial          = $"{efsSb.sb_checksum:X8}",
            CreationDate          = DateHandlers.UnixToDateTime(efsSb.sb_time),
            CreationDateSpecified = true
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming")]
    readonly struct Superblock
    {
        /* 0:   fs size incl. bb 0 (in bb) */
        public readonly int sb_size;
        /* 4:   first cg offset (in bb) */
        public readonly int sb_firstcg;
        /* 8:   cg size (in bb) */
        public readonly int sb_cgfsize;
        /* 12:  inodes/cg (in bb) */
        public readonly short sb_cgisize;
        /* 14:  geom: sectors/track */
        public readonly short sb_sectors;
        /* 16:  geom: heads/cylinder (unused) */
        public readonly short sb_heads;
        /* 18:  num of cg's in the filesystem */
        public readonly short sb_ncg;
        /* 20:  non-0 indicates fsck required */
        public readonly short sb_dirty;
        /* 22:  */
        public readonly short sb_pad0;
        /* 24:  superblock ctime */
        public readonly int sb_time;
        /* 28:  magic [0] */
        public readonly uint sb_magic;
        /* 32:  name of filesystem */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] sb_fname;
        /* 38:  name of filesystem pack */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] sb_fpack;
        /* 44:  bitmap size (in bytes) */
        public readonly int sb_bmsize;
        /* 48:  total free data blocks */
        public readonly int sb_tfree;
        /* 52:  total free inodes */
        public readonly int sb_tinode;
        /* 56:  bitmap offset (grown fs) */
        public readonly int sb_bmblock;
        /* 62:  repl. superblock offset */
        public readonly int sb_replsb;
        /* 64:  last allocated inode */
        public readonly int sb_lastinode;
        /* 68:  unused */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] sb_spare;
        /* 88:  checksum (all above) */
        public readonly uint sb_checksum;
    }
}