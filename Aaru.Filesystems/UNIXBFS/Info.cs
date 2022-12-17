// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UnixWare boot filesystem plugin.
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

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the UNIX boot filesystem</summary>
public sealed partial class BFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] tmp);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BitConverter.ToUInt32(tmp, 0);

        return magic == BFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";
        metadata    = new FileSystem();

        var         sb    = new StringBuilder();
        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bfsSbSector);

        if(errno != ErrorNumber.NoError)
            return;

        byte[] sbStrings = new byte[6];

        var bfsSb = new SuperBlock
        {
            s_magic = BitConverter.ToUInt32(bfsSbSector, 0x00),
            s_start = BitConverter.ToUInt32(bfsSbSector, 0x04),
            s_end   = BitConverter.ToUInt32(bfsSbSector, 0x08),
            s_from  = BitConverter.ToUInt32(bfsSbSector, 0x0C),
            s_to    = BitConverter.ToUInt32(bfsSbSector, 0x10),
            s_bfrom = BitConverter.ToInt32(bfsSbSector, 0x14),
            s_bto   = BitConverter.ToInt32(bfsSbSector, 0x18)
        };

        Array.Copy(bfsSbSector, 0x1C, sbStrings, 0, 6);
        bfsSb.s_fsname = StringHandlers.CToString(sbStrings, Encoding);
        Array.Copy(bfsSbSector, 0x22, sbStrings, 0, 6);
        bfsSb.s_volume = StringHandlers.CToString(sbStrings, Encoding);

        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_magic: 0x{0:X8}", bfsSb.s_magic);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_start: 0x{0:X8}", bfsSb.s_start);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_end: 0x{0:X8}", bfsSb.s_end);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_from: 0x{0:X8}", bfsSb.s_from);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_to: 0x{0:X8}", bfsSb.s_to);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bfrom: 0x{0:X8}", bfsSb.s_bfrom);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_bto: 0x{0:X8}", bfsSb.s_bto);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_fsname: 0x{0}", bfsSb.s_fsname);
        AaruConsole.DebugWriteLine("BFS plugin", "bfs_sb.s_volume: 0x{0}", bfsSb.s_volume);

        sb.AppendLine(Localization.UNIX_Boot_Filesystem);

        sb.AppendFormat(Localization.Volume_goes_from_byte_0_to_byte_1_for_2_bytes, bfsSb.s_start, bfsSb.s_end,
                        bfsSb.s_end - bfsSb.s_start).AppendLine();

        sb.AppendFormat(Localization.Filesystem_name_0, bfsSb.s_fsname).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, bfsSb.s_volume).AppendLine();

        metadata = new FileSystem
        {
            Type        = FS_TYPE,
            VolumeName  = bfsSb.s_volume,
            ClusterSize = imagePlugin.Info.SectorSize,
            Clusters    = partition.End - partition.Start + 1
        };

        information = sb.ToString();
    }
}