// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the AO-DOS file system and shows information.
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information has been extracted looking at available disk images
// This may be missing fields, or not, I don't know russian so any help is appreciated
/// <inheritdoc />
/// <summary>Implements detection of the AO-DOS filesystem</summary>
public sealed partial class AODOS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // Does AO-DOS support hard disks?
        if(partition.Start > 0)
            return false;

        // How is it really?
        if(imagePlugin.Info.SectorSize != 512)
            return false;

        // Does AO-DOS support any other kind of disk?
        if(imagePlugin.Info.Sectors != 800 &&
           imagePlugin.Info.Sectors != 1600)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        BootBlock bb = Marshal.ByteArrayToStructureLittleEndian<BootBlock>(sector);

        return bb.identifier.SequenceEqual(_identifier);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        encoding    = Encoding.GetEncoding("koi8-r");
        ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] sector);
        metadata = new FileSystem();

        if(errno != ErrorNumber.NoError)
            return;

        BootBlock bb = Marshal.ByteArrayToStructureLittleEndian<BootBlock>(sector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Alexander_Osipov_DOS_file_system);

        metadata = new FileSystem
        {
            Type         = FS_TYPE,
            Clusters     = imagePlugin.Info.Sectors,
            ClusterSize  = imagePlugin.Info.SectorSize,
            Files        = bb.files,
            FreeClusters = imagePlugin.Info.Sectors - bb.usedSectors,
            VolumeName   = StringHandlers.SpacePaddedToString(bb.volumeLabel, encoding),
            Bootable     = true
        };

        sbInformation.AppendFormat(Localization._0_files_on_volume, bb.files).AppendLine();
        sbInformation.AppendFormat(Localization._0_used_sectors_on_volume, bb.usedSectors).AppendLine();

        sbInformation.AppendFormat(Localization.Disk_name_0, StringHandlers.CToString(bb.volumeLabel, encoding)).
                      AppendLine();

        information = sbInformation.ToString();
    }
}