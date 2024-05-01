// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem plugin.
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem</summary>

// ReSharper disable once InconsistentNaming
public sealed partial class extFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512) return false;

        ulong sbSectorOff = SB_POS / imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % imagePlugin.Info.SectorSize;

        if(sbSectorOff + partition.Start >= partition.End) return false;

        ErrorNumber errno = imagePlugin.ReadSector(sbSectorOff + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError) return false;

        var sb = new byte[512];

        if(sbOff + 512 > sbSector.Length) return false;

        Array.Copy(sbSector, sbOff, sb, 0, 512);

        var magic = BitConverter.ToUInt16(sb, 0x038);

        return magic == EXT_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        var sb = new StringBuilder();

        if(imagePlugin.Info.SectorSize < 512) return;

        ulong sbSectorOff = SB_POS / imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % imagePlugin.Info.SectorSize;

        if(sbSectorOff + partition.Start >= partition.End) return;

        ErrorNumber errno = imagePlugin.ReadSector(sbSectorOff + partition.Start, out byte[] sblock);

        if(errno != ErrorNumber.NoError) return;

        var sbSector = new byte[512];
        Array.Copy(sblock, sbOff, sbSector, 0, 512);

        var extSb = new SuperBlock
        {
            inodes        = BitConverter.ToUInt32(sbSector, 0x000),
            zones         = BitConverter.ToUInt32(sbSector, 0x004),
            firstfreeblk  = BitConverter.ToUInt32(sbSector, 0x008),
            freecountblk  = BitConverter.ToUInt32(sbSector, 0x00C),
            firstfreeind  = BitConverter.ToUInt32(sbSector, 0x010),
            freecountind  = BitConverter.ToUInt32(sbSector, 0x014),
            firstdatazone = BitConverter.ToUInt32(sbSector, 0x018),
            logzonesize   = BitConverter.ToUInt32(sbSector, 0x01C),
            maxsize       = BitConverter.ToUInt32(sbSector, 0x020)
        };

        sb.AppendLine(Localization.ext_filesystem);
        sb.AppendFormat(Localization._0_zones_in_volume,     extSb.zones);
        sb.AppendFormat(Localization._0_free_blocks_1_bytes, extSb.freecountblk, extSb.freecountblk * 1024);

        sb.AppendFormat(Localization._0_inodes_in_volume_1_free_2,
                        extSb.inodes,
                        extSb.freecountind,
                        extSb.freecountind * 100 / extSb.inodes);

        sb.AppendFormat(Localization.First_free_inode_is_0, extSb.firstfreeind);
        sb.AppendFormat(Localization.First_free_block_is_0, extSb.firstfreeblk);
        sb.AppendFormat(Localization.First_data_zone_is_0,  extSb.firstdatazone);
        sb.AppendFormat(Localization.Log_zone_size_0,       extSb.logzonesize);
        sb.AppendFormat(Localization.Max_zone_size_0,       extSb.maxsize);

        metadata = new FileSystem
        {
            Type         = FS_TYPE,
            FreeClusters = extSb.freecountblk,
            ClusterSize  = 1024,
            Clusters     = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / 1024
        };

        information = sb.ToString();
    }

#endregion
}