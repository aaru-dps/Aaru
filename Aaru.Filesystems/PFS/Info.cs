// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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

// ReSharper disable UnusedType.Local

using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Professional File System</summary>
public sealed partial class PFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Length < 3)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

        return magic is AFS_DISK or PFS2_DISK or PFS_DISK or MUAF_DISK or MUPFS_DISK;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        information = "";
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] rootBlockSector);

        if(errno != ErrorNumber.NoError)
            return;

        RootBlock rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockSector);

        var sbInformation = new StringBuilder();
        XmlFsType = new FileSystemType();

        switch(rootBlock.diskType)
        {
            case AFS_DISK:
            case MUAF_DISK:
                sbInformation.Append(Localization.Professional_File_System_v1);
                XmlFsType.Type = FS_TYPE;

                break;
            case PFS2_DISK:
                sbInformation.Append(Localization.Professional_File_System_v2);
                XmlFsType.Type = FS_TYPE;

                break;
            case PFS_DISK:
            case MUPFS_DISK:
                sbInformation.Append(Localization.Professional_File_System_v3);
                XmlFsType.Type = FS_TYPE;

                break;
        }

        if(rootBlock.diskType is MUAF_DISK or MUPFS_DISK)
            sbInformation.Append(Localization.with_multi_user_support);

        sbInformation.AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_name_0, StringHandlers.PascalToString(rootBlock.diskname, Encoding)).
            AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_has_0_free_sectors_of_1, rootBlock.blocksfree, rootBlock.diskSize).
            AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.AmigaToDateTime(rootBlock.creationday, rootBlock.creationminute,
                                                                rootBlock.creationtick)).AppendLine();

        if(rootBlock.extension > 0)
            sbInformation.AppendFormat(Localization.Root_block_extension_resides_at_block_0, rootBlock.extension).
                          AppendLine();

        information = sbInformation.ToString();

        XmlFsType.CreationDate =
            DateHandlers.AmigaToDateTime(rootBlock.creationday, rootBlock.creationminute, rootBlock.creationtick);

        XmlFsType.CreationDateSpecified = true;
        XmlFsType.FreeClusters          = rootBlock.blocksfree;
        XmlFsType.FreeClustersSpecified = true;
        XmlFsType.Clusters              = rootBlock.diskSize;
        XmlFsType.ClusterSize           = imagePlugin.Info.SectorSize;
        XmlFsType.VolumeName            = StringHandlers.PascalToString(rootBlock.diskname, Encoding);
    }
}