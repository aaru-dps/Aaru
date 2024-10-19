// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of SGI's XFS</summary>
public sealed partial class XFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512) return false;

        // Misaligned
        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            var sbSize = (uint)((Marshal.SizeOf<Superblock>() + 0x400) / imagePlugin.Info.SectorSize);

            if((Marshal.SizeOf<Superblock>() + 0x400) % imagePlugin.Info.SectorSize != 0) sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError) return false;

            if(sector.Length < Marshal.SizeOf<Superblock>()) return false;

            var sbpiece = new byte[Marshal.SizeOf<Superblock>()];

            foreach(int location in new[]
                    {
                        0, 0x200, 0x400
                    })
            {
                Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf<Superblock>());

                Superblock xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.magic_at_0_X3_equals_1_expected_2,
                                           location,
                                           xfsSb.magicnum,
                                           XFS_MAGIC);

                if(xfsSb.magicnum == XFS_MAGIC) return true;
            }
        }
        else
        {
            foreach(int i in new[]
                    {
                        0, 1, 2
                    })
            {
                var location = (ulong)i;

                var sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

                if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0) sbSize++;

                ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + location, sbSize, out byte[] sector);

                if(errno != ErrorNumber.NoError) continue;

                if(sector.Length < Marshal.SizeOf<Superblock>()) return false;

                Superblock xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.magic_at_0_equals_1_expected_2,
                                           location,
                                           xfsSb.magicnum,
                                           XFS_MAGIC);

                if(xfsSb.magicnum == XFS_MAGIC) return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();

        if(imagePlugin.Info.SectorSize < 512) return;

        var xfsSb = new Superblock();

        // Misaligned
        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            var sbSize = (uint)((Marshal.SizeOf<Superblock>() + 0x400) / imagePlugin.Info.SectorSize);

            if((Marshal.SizeOf<Superblock>() + 0x400) % imagePlugin.Info.SectorSize != 0) sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError || sector.Length < Marshal.SizeOf<Superblock>()) return;

            var sbpiece = new byte[Marshal.SizeOf<Superblock>()];

            foreach(int location in new[]
                    {
                        0, 0x200, 0x400
                    })
            {
                Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf<Superblock>());

                xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.magic_at_0_X3_equals_1_expected_2,
                                           location,
                                           xfsSb.magicnum,
                                           XFS_MAGIC);

                if(xfsSb.magicnum == XFS_MAGIC) break;
            }
        }
        else
        {
            foreach(int i in new[]
                    {
                        0, 1, 2
                    })
            {
                var location = (ulong)i;
                var sbSize   = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

                if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0) sbSize++;

                ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + location, sbSize, out byte[] sector);

                if(errno != ErrorNumber.NoError || sector.Length < Marshal.SizeOf<Superblock>()) return;

                xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.magic_at_0_equals_1_expected_2,
                                           location,
                                           xfsSb.magicnum,
                                           XFS_MAGIC);

                if(xfsSb.magicnum == XFS_MAGIC) break;
            }
        }

        if(xfsSb.magicnum != XFS_MAGIC) return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.XFS_filesystem);
        sb.AppendFormat(Localization.Filesystem_version_0,            xfsSb.version & 0xF).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector,             xfsSb.sectsize).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_block,              xfsSb.blocksize).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_inode,              xfsSb.inodesize).AppendLine();
        sb.AppendFormat(Localization._0_data_blocks_in_volume_1_free, xfsSb.dblocks, xfsSb.fdblocks).AppendLine();
        sb.AppendFormat(Localization._0_blocks_per_allocation_group,  xfsSb.agblocks).AppendLine();
        sb.AppendFormat(Localization._0_allocation_groups_in_volume,  xfsSb.agcount).AppendLine();
        sb.AppendFormat(Localization._0_inodes_in_volume_1_free,      xfsSb.icount, xfsSb.ifree).AppendLine();

        if(xfsSb.inprogress > 0) sb.AppendLine(Localization.fsck_in_progress);

        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(xfsSb.fname, encoding)).AppendLine();
        sb.AppendFormat(Localization.Volume_UUID_0, xfsSb.uuid).AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = xfsSb.blocksize,
            Clusters     = xfsSb.dblocks,
            FreeClusters = xfsSb.fdblocks,
            Files        = xfsSb.icount - xfsSb.ifree,
            Dirty        = xfsSb.inprogress > 0,
            VolumeName   = StringHandlers.CToString(xfsSb.fname, encoding),
            VolumeSerial = xfsSb.uuid.ToString()
        };
    }

#endregion
}