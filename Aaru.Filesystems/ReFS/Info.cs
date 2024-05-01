// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Resilient File System plugin
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Microsoft's Resilient filesystem (ReFS)</summary>
public sealed partial class ReFS : IFilesystem
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        var sbSize = (uint)(Marshal.SizeOf<VolumeHeader>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<VolumeHeader>() % imagePlugin.Info.SectorSize != 0) sbSize++;

        if(partition.Start + sbSize >= partition.End) return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError) return false;

        if(sector.Length < Marshal.SizeOf<VolumeHeader>()) return false;

        VolumeHeader vhdr = Marshal.ByteArrayToStructureLittleEndian<VolumeHeader>(sector);

        return vhdr.identifier == FSRS                          &&
               ArrayHelpers.ArrayIsNullOrEmpty(vhdr.mustBeZero) &&
               vhdr.signature.SequenceEqual(_signature);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        var sbSize = (uint)(Marshal.SizeOf<VolumeHeader>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<VolumeHeader>() % imagePlugin.Info.SectorSize != 0) sbSize++;

        if(partition.Start + sbSize >= partition.End) return;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError) return;

        if(sector.Length < Marshal.SizeOf<VolumeHeader>()) return;

        VolumeHeader vhdr = Marshal.ByteArrayToStructureLittleEndian<VolumeHeader>(sector);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "VolumeHeader.jump empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(vhdr.jump));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "VolumeHeader.signature = {0}",
                                   StringHandlers.CToString(vhdr.signature));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "VolumeHeader.mustBeZero empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(vhdr.mustBeZero));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "VolumeHeader.identifier = {0}",
                                   StringHandlers.CToString(BitConverter.GetBytes(vhdr.identifier)));

        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.length = {0}",         vhdr.length);
        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.checksum = 0x{0:X4}",  vhdr.checksum);
        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.sectors = {0}",        vhdr.sectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.bytesPerSector = {0}", vhdr.bytesPerSector);

        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.sectorsPerCluster = {0}", vhdr.sectorsPerCluster);

        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.unknown1 zero? = {0}", vhdr.unknown1 == 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.unknown2 zero? = {0}", vhdr.unknown2 == 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.unknown3 zero? = {0}", vhdr.unknown3 == 0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "VolumeHeader.unknown4 zero? = {0}", vhdr.unknown4 == 0);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "VolumeHeader.unknown5 empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(vhdr.unknown5));

        if(vhdr.identifier != FSRS                           ||
           !ArrayHelpers.ArrayIsNullOrEmpty(vhdr.mustBeZero) ||
           !vhdr.signature.SequenceEqual(_signature))
            return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.Microsoft_Resilient_File_System);
        sb.AppendFormat(Localization.Volume_uses_0_bytes_per_sector, vhdr.bytesPerSector).AppendLine();

        sb.AppendFormat(Localization.Volume_uses_0_sectors_per_cluster_1_bytes,
                        vhdr.sectorsPerCluster,
                        vhdr.sectorsPerCluster * vhdr.bytesPerSector)
          .AppendLine();

        sb.AppendFormat(Localization.Volume_has_0_sectors_1_bytes, vhdr.sectors, vhdr.sectors * vhdr.bytesPerSector)
          .AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Type        = FS_TYPE,
            ClusterSize = vhdr.bytesPerSector * vhdr.sectorsPerCluster,
            Clusters    = vhdr.sectors        / vhdr.sectorsPerCluster
        };
    }

#endregion
}