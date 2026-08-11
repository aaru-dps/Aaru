// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Veritas File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Marshal = System.Runtime.InteropServices.Marshal;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class VxFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // Try Unixware/x86 location (block 1, offset 0x400, little-endian)
        ulong sbSectorOff = VXFS_BASE / imagePlugin.Info.SectorSize;
        uint  sbOff       = VXFS_BASE % imagePlugin.Info.SectorSize;

        if(partition.Start + sbSectorOff < partition.End)
        {
            ErrorNumber errno = imagePlugin.ReadSector(partition.Start + sbSectorOff, false, out byte[] sector, out _);

            if(errno == ErrorNumber.NoError && sbOff + 4 <= sector.Length)
            {
                var magic = BitConverter.ToUInt32(sector, (int)sbOff);

                if(magic == VXFS_MAGIC) return true;
            }
        }

        // Try HP-UX/parisc location (block 8, offset 0x2000, big-endian)
        sbSectorOff = VXFS_BASE_BE / imagePlugin.Info.SectorSize;
        sbOff       = VXFS_BASE_BE % imagePlugin.Info.SectorSize;

        if(partition.Start + sbSectorOff >= partition.End) return false;

        {
            ErrorNumber errno = imagePlugin.ReadSector(partition.Start + sbSectorOff, false, out byte[] sector, out _);

            if(errno != ErrorNumber.NoError || sbOff + 4 > sector.Length) return false;

            var magic = BitConverter.ToUInt32(sector, (int)sbOff);

            return magic == VXFS_MAGIC_BE;
        }
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.UTF8;
        information =   "";
        metadata    =   new FileSystem();

        var   bigEndian   = false;
        ulong sbSectorOff = VXFS_BASE / imagePlugin.Info.SectorSize;
        uint  sbOff       = VXFS_BASE % imagePlugin.Info.SectorSize;

        int sbSizeInBytes = Marshal.SizeOf<SuperBlock>();

        var sbSizeInSectors = (uint)((sbOff + sbSizeInBytes + imagePlugin.Info.SectorSize - 1) /
                                     imagePlugin.Info.SectorSize);

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbSectorOff,
                                                    false,
                                                    sbSizeInSectors,
                                                    out byte[] sbSector,
                                                    out _);

        if(errno != ErrorNumber.NoError) return;

        if(sbOff + 4 > sbSector.Length) return;

        var magic = BitConverter.ToUInt32(sbSector, (int)sbOff);

        if(magic != VXFS_MAGIC)
        {
            // Try HP-UX/parisc location (block 8, offset 0x2000, big-endian)
            sbSectorOff = VXFS_BASE_BE / imagePlugin.Info.SectorSize;
            sbOff       = VXFS_BASE_BE % imagePlugin.Info.SectorSize;

            sbSizeInSectors = (uint)((sbOff + sbSizeInBytes + imagePlugin.Info.SectorSize - 1) /
                                     imagePlugin.Info.SectorSize);

            errno = imagePlugin.ReadSectors(partition.Start + sbSectorOff, false, sbSizeInSectors, out sbSector, out _);

            if(errno != ErrorNumber.NoError) return;

            if(sbOff + 4 > (uint)sbSector.Length) return;

            magic = BitConverter.ToUInt32(sbSector, (int)sbOff);

            if(magic != VXFS_MAGIC_BE) return;

            bigEndian = true;
        }

        var sb = new byte[sbSizeInBytes];

        if(sbOff + sbSizeInBytes > sbSector.Length) return;

        Array.Copy(sbSector, sbOff, sb, 0, sbSizeInBytes);

        SuperBlock vxSb = bigEndian
                              ? Helpers.Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sb)
                              : Helpers.Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sb);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Veritas_file_system);

        sbInformation.AppendFormat(Localization.Volume_version_0, vxSb.vs_version).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(vxSb.vs_fname, encoding))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_blocks_of_1_bytes_each, vxSb.vs_size, vxSb.vs_bsize)
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_inodes_per_block, vxSb.vs_inopb).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_free_inodes,      vxSb.vs_ifree).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_free_blocks,      vxSb.vs_free).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(vxSb.vs_ctime, vxSb.vs_cutime))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_last_modified_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(vxSb.vs_wtime, vxSb.vs_wutime))
                     .AppendLine();

        if(vxSb.vs_clean != 0) sbInformation.AppendLine(Localization.Volume_is_dirty);

        information = sbInformation.ToString();

        metadata = new FileSystem
        {
            Type             = FS_TYPE,
            CreationDate     = DateHandlers.UnixUnsignedToDateTime(vxSb.vs_ctime, vxSb.vs_cutime),
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(vxSb.vs_wtime, vxSb.vs_wutime),
            Clusters         = (ulong)vxSb.vs_size,
            ClusterSize      = (uint)vxSb.vs_bsize,
            Dirty            = vxSb.vs_clean != 0,
            FreeClusters     = (ulong)vxSb.vs_free
        };
    }

#endregion
}