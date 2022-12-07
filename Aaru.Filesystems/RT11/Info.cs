// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : RT-11 file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the RT-11 file system and shows information.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

namespace Aaru.Filesystems;

// Information from http://www.trailing-edge.com/~shoppa/rt11fs/
/// <inheritdoc />
/// <summary>Implements detection of the DEC RT-11 filesystem</summary>
public sealed partial class RT11
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(1 + partition.Start >= partition.End)
            return false;

        byte[]      magicB = new byte[12];
        ErrorNumber errno  = imagePlugin.ReadSector(1 + partition.Start, out byte[] hbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(hbSector.Length < 512)
            return false;

        Array.Copy(hbSector, 0x1F0, magicB, 0, 12);
        string magic = Encoding.ASCII.GetString(magicB);

        return magic == "DECRT11A    ";
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = new Radix50();
        information = "";

        var sb = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(1 + partition.Start, out byte[] hbSector);

        if(errno != ErrorNumber.NoError)
            return;

        HomeBlock homeblock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);

        /* TODO: Is this correct?
         * Assembler:
         *      MOV address, R0
         *      CLR R1
         *      MOV #255., R2
         * 10$: ADD (R0)+, R1
         *      SOB R2, 10$
         *      MOV 1,@R0
         */
        ushort check = 0;

        for(int i = 0; i < 512; i += 2)
            check += BitConverter.ToUInt16(hbSector, i);

        sb.AppendFormat(Localization.Volume_format_is_0,
                        StringHandlers.SpacePaddedToString(homeblock.format, Encoding.ASCII)).AppendLine();

        sb.AppendFormat(Localization._0_sectors_per_cluster_1_bytes, homeblock.cluster, homeblock.cluster * 512).
           AppendLine();

        sb.AppendFormat(Localization.First_directory_segment_starts_at_block_0, homeblock.rootBlock).AppendLine();
        sb.AppendFormat(Localization.Volume_owner_is_0, Encoding.GetString(homeblock.ownername).TrimEnd()).AppendLine();
        sb.AppendFormat(Localization.Volume_label_0, Encoding.GetString(homeblock.volname).TrimEnd()).AppendLine();
        sb.AppendFormat(Localization.Checksum_0_calculated_1, homeblock.checksum, check).AppendLine();

        imagePlugin.ReadSector(0, out byte[] bootBlock);

        XmlFsType = new FileSystemType
        {
            Type        = FS_TYPE,
            ClusterSize = (uint)(homeblock.cluster * 512),
            Clusters    = homeblock.cluster,
            VolumeName  = StringHandlers.SpacePaddedToString(homeblock.volname, Encoding),
            Bootable    = !ArrayHelpers.ArrayIsNullOrEmpty(bootBlock)
        };

        information = sb.ToString();
    }
}