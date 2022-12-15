// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
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

/// <inheritdoc />
/// <summary>Implements detection of the Reiser v4 filesystem</summary>
public sealed partial class Reiser4
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        uint sbAddr = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;

        if(sbAddr == 0)
            sbAddr = 1;

        uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        if(partition.Start + sbAddr + sbSize >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return false;

        Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        return _magic.SequenceEqual(reiserSb.magic);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        uint sbAddr = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;

        if(sbAddr == 0)
            sbAddr = 1;

        uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return;

        Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        if(!_magic.SequenceEqual(reiserSb.magic))
            return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.Reiser_4_filesystem);
        sb.AppendFormat(Localization._0_bytes_per_block, reiserSb.blocksize).AppendLine();
        sb.AppendFormat(Localization.Volume_disk_format_0, reiserSb.diskformat).AppendLine();
        sb.AppendFormat(Localization.Volume_UUID_0, reiserSb.uuid).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(reiserSb.label, Encoding)).AppendLine();

        information = sb.ToString();

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = reiserSb.blocksize,
            Clusters     = (partition.End - partition.Start) * imagePlugin.Info.SectorSize / reiserSb.blocksize,
            VolumeName   = StringHandlers.CToString(reiserSb.label, Encoding),
            VolumeSerial = reiserSb.uuid.ToString()
        };
    }
}