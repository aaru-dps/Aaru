// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Cram file system plugin.
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

// ReSharper disable UnusedMember.Local

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the CRAM filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Cram
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        return magic is CRAM_MAGIC or CRAM_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        var  crSb         = new SuperBlock();
        bool littleEndian = true;

        switch(magic)
        {
            case CRAM_MAGIC:
                crSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

                break;
            case CRAM_CIGAM:
                crSb         = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);
                littleEndian = false;

                break;
        }

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Cram_file_system);
        sbInformation.AppendLine(littleEndian ? Localization.Little_endian : Localization.Big_endian);
        sbInformation.AppendFormat(Localization.Volume_edition_0, crSb.edition).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(crSb.name, Encoding)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_bytes, crSb.size).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_blocks, crSb.blocks).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_files, crSb.files).AppendLine();

        information = sbInformation.ToString();

        XmlFsType = new FileSystemType
        {
            VolumeName            = StringHandlers.CToString(crSb.name, Encoding),
            Type                  = FS_TYPE,
            Clusters              = crSb.blocks,
            Files                 = crSb.files,
            FilesSpecified        = true,
            FreeClusters          = 0,
            FreeClustersSpecified = true
        };
    }
}