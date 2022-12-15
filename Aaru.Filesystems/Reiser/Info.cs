// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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
/// <summary>Implements detection of the Reiser v3 filesystem</summary>
public sealed partial class Reiser : IFilesystem
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        uint sbAddr = REISER_SUPER_OFFSET / imagePlugin.Info.SectorSize;

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

        return _magic35.SequenceEqual(reiserSb.magic) || _magic36.SequenceEqual(reiserSb.magic) ||
               _magicJr.SequenceEqual(reiserSb.magic);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        uint sbAddr = REISER_SUPER_OFFSET / imagePlugin.Info.SectorSize;

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

        if(!_magic35.SequenceEqual(reiserSb.magic) &&
           !_magic36.SequenceEqual(reiserSb.magic) &&
           !_magicJr.SequenceEqual(reiserSb.magic))
            return;

        var sb = new StringBuilder();

        if(_magic35.SequenceEqual(reiserSb.magic))
            sb.AppendLine(Localization.Reiser_3_5_filesystem);
        else if(_magic36.SequenceEqual(reiserSb.magic))
            sb.AppendLine(Localization.Reiser_3_6_filesystem);
        else if(_magicJr.SequenceEqual(reiserSb.magic))
            sb.AppendLine(Localization.Reiser_Jr_filesystem);

        sb.AppendFormat(Localization.Volume_has_0_blocks_with_1_blocks_free, reiserSb.block_count,
                        reiserSb.free_blocks).AppendLine();

        sb.AppendFormat(Localization._0_bytes_per_block, reiserSb.blocksize).AppendLine();
        sb.AppendFormat(Localization.Root_directory_resides_on_block_0, reiserSb.root_block).AppendLine();

        if(reiserSb.umount_state == 2)
            sb.AppendLine(Localization.Volume_has_not_been_cleanly_umounted);

        sb.AppendFormat(Localization.Volume_last_checked_on_0,
                        DateHandlers.UnixUnsignedToDateTime(reiserSb.last_check)).AppendLine();

        if(reiserSb.version >= 2)
        {
            sb.AppendFormat(Localization.Volume_UUID_0, reiserSb.uuid).AppendLine();
            sb.AppendFormat(Localization.Volume_name_0, Encoding.GetString(reiserSb.label)).AppendLine();
        }

        information = sb.ToString();

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = reiserSb.blocksize,
            Clusters     = reiserSb.block_count,
            FreeClusters = reiserSb.free_blocks,
            Dirty        = reiserSb.umount_state == 2
        };

        if(reiserSb.version < 2)
            return;

        Metadata.VolumeName   = StringHandlers.CToString(reiserSb.label, Encoding);
        Metadata.VolumeSerial = reiserSb.uuid.ToString();
    }
}