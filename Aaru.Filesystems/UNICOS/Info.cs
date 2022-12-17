// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNICOS filesystem plugin.
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

// UNICOS is ILP64 so let's think everything is 64-bit

using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using blkno_t = System.Int64;
using daddr_t = System.Int64;
using dev_t = System.Int64;
using extent_t = System.Int64;
using ino_t = System.Int64;
using Partition = Aaru.CommonTypes.Partition;
using time_t = System.Int64;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the Cray UNICOS filesystem</summary>
public sealed partial class UNICOS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        if(partition.Start + sbSize >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return false;

        Superblock unicosSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

        AaruConsole.DebugWriteLine("UNICOS plugin", Localization.magic_equals_0_expected_1, unicosSb.s_magic,
                                   UNICOS_MAGIC);

        return unicosSb.s_magic == UNICOS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();

        if(imagePlugin.Info.SectorSize < 512)
            return;

        uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return;

        Superblock unicosSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

        if(unicosSb.s_magic != UNICOS_MAGIC)
            return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.UNICOS_filesystem);

        if(unicosSb.s_secure == UNICOS_SECURE)
            sb.AppendLine(Localization.Volume_is_secure);

        sb.AppendFormat(Localization.Volume_contains_0_partitions, unicosSb.s_npart).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, unicosSb.s_iounit).AppendLine();
        sb.AppendLine(Localization._4096_bytes_per_block);
        sb.AppendFormat(Localization._0_data_blocks_in_volume, unicosSb.s_fsize).AppendLine();
        sb.AppendFormat(Localization.Root_resides_on_inode_0, unicosSb.s_root).AppendLine();
        sb.AppendFormat(Localization._0_inodes_in_volume, unicosSb.s_isize).AppendLine();

        sb.AppendFormat(Localization.Volume_last_updated_on_0, DateHandlers.UnixToDateTime(unicosSb.s_time)).
           AppendLine();

        if(unicosSb.s_error > 0)
            sb.AppendFormat(Localization.Volume_is_dirty_error_code_equals_0, unicosSb.s_error).AppendLine();

        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(unicosSb.s_fname, encoding)).AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Type             = FS_TYPE,
            ClusterSize      = 4096,
            Clusters         = (ulong)unicosSb.s_fsize,
            VolumeName       = StringHandlers.CToString(unicosSb.s_fname, encoding),
            ModificationDate = DateHandlers.UnixToDateTime(unicosSb.s_time)
        };

        metadata.Dirty |= unicosSb.s_error > 0;
    }
}