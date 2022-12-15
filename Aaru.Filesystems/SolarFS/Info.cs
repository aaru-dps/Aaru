// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SolarOS filesystem plugin.
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
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Based on FAT's BPB, cannot find a FAT or directory
/// <inheritdoc />
/// <summary>Implements detection of the Solar OS filesystem</summary>
public sealed partial class SolarFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bpb);

        if(errno != ErrorNumber.NoError)
            return false;

        byte[] fsTypeB = new byte[8];

        byte signature = bpb[0x25];
        Array.Copy(bpb, 0x35, fsTypeB, 0, 8);
        string fsType = StringHandlers.CToString(fsTypeB);

        return signature == 0x29 && fsType == "SOL_FS  ";
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var         sb    = new StringBuilder();
        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bpbSector);

        if(errno != ErrorNumber.NoError)
            return;

        var bpb = new BiosParameterBlock
        {
            bps       = BitConverter.ToUInt16(bpbSector, 0x0B),
            root_ent  = BitConverter.ToUInt16(bpbSector, 0x10),
            sectors   = BitConverter.ToUInt16(bpbSector, 0x12),
            media     = bpbSector[0x14],
            spfat     = BitConverter.ToUInt16(bpbSector, 0x15),
            sptrk     = BitConverter.ToUInt16(bpbSector, 0x17),
            heads     = BitConverter.ToUInt16(bpbSector, 0x19),
            signature = bpbSector[0x25]
        };

        byte[] bpbStrings = new byte[8];
        Array.Copy(bpbSector, 0x03, bpbStrings, 0, 8);
        bpb.OEMName = StringHandlers.CToString(bpbStrings);
        bpbStrings  = new byte[8];
        Array.Copy(bpbSector, 0x2A, bpbStrings, 0, 11);
        bpb.vol_name = StringHandlers.CToString(bpbStrings, Encoding);
        bpbStrings   = new byte[8];
        Array.Copy(bpbSector, 0x35, bpbStrings, 0, 8);
        bpb.fs_type = StringHandlers.CToString(bpbStrings, Encoding);

        bpb.x86_jump = new byte[3];
        Array.Copy(bpbSector, 0x00, bpb.x86_jump, 0, 3);
        bpb.unk1 = bpbSector[0x0D];
        bpb.unk2 = BitConverter.ToUInt16(bpbSector, 0x0E);
        bpb.unk3 = new byte[10];
        Array.Copy(bpbSector, 0x1B, bpb.unk3, 0, 10);
        bpb.unk4 = BitConverter.ToUInt32(bpbSector, 0x26);

        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.x86_jump: 0x{0:X2}{1:X2}{2:X2}", bpb.x86_jump[0],
                                   bpb.x86_jump[1], bpb.x86_jump[2]);

        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.OEMName: \"{0}\"", bpb.OEMName);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.bps: {0}", bpb.bps);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.unk1: 0x{0:X2}", bpb.unk1);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.unk2: 0x{0:X4}", bpb.unk2);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.root_ent: {0}", bpb.root_ent);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.sectors: {0}", bpb.sectors);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.media: 0x{0:X2}", bpb.media);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.spfat: {0}", bpb.spfat);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.sptrk: {0}", bpb.sptrk);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.heads: {0}", bpb.heads);

        AaruConsole.DebugWriteLine("SolarFS plugin",
                                   "BPB.unk3: 0x{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}",
                                   bpb.unk3[0], bpb.unk3[1], bpb.unk3[2], bpb.unk3[3], bpb.unk3[4], bpb.unk3[5],
                                   bpb.unk3[6], bpb.unk3[7], bpb.unk3[8], bpb.unk3[9]);

        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.signature: 0x{0:X2}", bpb.signature);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.unk4: 0x{0:X8}", bpb.unk4);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.vol_name: \"{0}\"", bpb.vol_name);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.fs_type: \"{0}\"", bpb.fs_type);

        sb.AppendLine(Localization.Solar_OS_filesystem);
        sb.AppendFormat(Localization.Media_descriptor_0, bpb.media).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, bpb.bps).AppendLine();

        if(imagePlugin.Info.SectorSize is 2336 or 2352 or 2448)
        {
            if(bpb.bps != imagePlugin.Info.SectorSize)
                sb.
                    AppendFormat(Localization.WARNING_Filesystem_describes_a_0_bytes_sector_while_device_describes_a_1_bytes_sector,
                                 bpb.bps, 2048).AppendLine();
        }
        else if(bpb.bps != imagePlugin.Info.SectorSize)
            sb.
                AppendFormat(Localization.WARNING_Filesystem_describes_a_0_bytes_sector_while_device_describes_a_1_bytes_sector,
                             bpb.bps, imagePlugin.Info.SectorSize).AppendLine();

        sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, bpb.sectors, bpb.sectors * bpb.bps).AppendLine();

        if(bpb.sectors > imagePlugin.Info.Sectors)
            sb.AppendFormat(Localization.WARNING_Filesystem_describes_a_0_sectors_volume_bigger_than_device_1_sectors,
                            bpb.sectors, imagePlugin.Info.Sectors);

        sb.AppendFormat(Localization._0_heads, bpb.heads).AppendLine();
        sb.AppendFormat(Localization._0_sectors_per_track, bpb.sptrk).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, bpb.vol_name).AppendLine();

        Metadata = new FileSystem
        {
            Type        = FS_TYPE,
            Clusters    = bpb.sectors,
            ClusterSize = bpb.bps,
            VolumeName  = bpb.vol_name
        };

        information = sb.ToString();
    }
}