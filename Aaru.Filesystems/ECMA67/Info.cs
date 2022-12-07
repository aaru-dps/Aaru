// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ECMA-67 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ECMA-67 file system and shows information.
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

using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem described in ECMA-67</summary>
public sealed partial class ECMA67
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start > 0)
            return false;

        if(partition.End < 8)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(6, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length != 128)
            return false;

        VolumeLabel vol = Marshal.ByteArrayToStructureLittleEndian<VolumeLabel>(sector);

        return _magic.SequenceEqual(vol.labelIdentifier) && vol is { labelNumber: 1, recordLength: 0x31 };
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        information = "";
        ErrorNumber errno = imagePlugin.ReadSector(6, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        var sbInformation = new StringBuilder();

        VolumeLabel vol = Marshal.ByteArrayToStructureLittleEndian<VolumeLabel>(sector);

        sbInformation.AppendLine(Localization.ECMA_67);

        sbInformation.AppendFormat(Localization.Volume_name_0, Encoding.ASCII.GetString(vol.volumeIdentifier)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_owner_0, Encoding.ASCII.GetString(vol.owner)).AppendLine();

        XmlFsType = new FileSystemType
        {
            Type        = FS_TYPE,
            ClusterSize = 256,
            Clusters    = partition.End - partition.Start + 1,
            VolumeName  = Encoding.ASCII.GetString(vol.volumeIdentifier)
        };

        information = sbInformation.ToString();
    }
}