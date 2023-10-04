// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// TODO: Detect bootable
/// <inheritdoc />
/// <summary>Implements detection of the Universal Disk Format filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class UDF
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // UDF needs at least that
        if(partition.End - partition.Start < 256)
            return false;

        // UDF needs at least that
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        var    anchor = new AnchorVolumeDescriptorPointer();

        // All positions where anchor may reside, with the ratio between 512 and 2048bps
        ulong[][] positions =
        {
            new ulong[] { 256, 1 }, new ulong[] { 512, 1 }, new ulong[] { partition.End - 256, 1 },
            new ulong[] { partition.End, 1 }, new ulong[] { 1024, 4 }, new ulong[] { 2048, 4 },
            new ulong[] { partition.End - 1024, 4 }, new ulong[] { partition.End - 4, 4 }
        };

        var    anchorFound = false;
        uint   ratio       = 1;
        byte[] sector      = null;

        foreach(ulong[] position in from position in
                                        positions.Where(position =>
                                                            position[0] + partition.Start + position[1] <=
                                                            partition.End && position[0] < partition.End)
                                    let errno =
                                        imagePlugin.ReadSectors(position[0], (uint)position[1], out sector)
                                    where errno == ErrorNumber.NoError
                                    select position)
        {
            anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(sector);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.tagIdentifier = {0}", anchor.tag.tagIdentifier);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.descriptorVersion = {0}",
                                       anchor.tag.descriptorVersion);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.tagChecksum = 0x{0:X2}", anchor.tag.tagChecksum);
            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.reserved = {0}",         anchor.tag.reserved);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.tagSerialNumber = {0}", anchor.tag.tagSerialNumber);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.descriptorCrc = 0x{0:X4}", anchor.tag.descriptorCrc);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.descriptorCrcLength = {0}",
                                       anchor.tag.descriptorCrcLength);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.tag.tagLocation = {0}", anchor.tag.tagLocation);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.mainVolumeDescriptorSequenceExtent.length = {0}",
                                       anchor.mainVolumeDescriptorSequenceExtent.length);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.mainVolumeDescriptorSequenceExtent.location = {0}",
                                       anchor.mainVolumeDescriptorSequenceExtent.location);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.reserveVolumeDescriptorSequenceExtent.length = {0}",
                                       anchor.reserveVolumeDescriptorSequenceExtent.length);

            AaruConsole.DebugWriteLine(MODULE_NAME, "anchor.reserveVolumeDescriptorSequenceExtent.location = {0}",
                                       anchor.reserveVolumeDescriptorSequenceExtent.location);

            if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               anchor.tag.tagLocation != position[0] / position[1] ||
               anchor.mainVolumeDescriptorSequenceExtent.location * position[1] + partition.Start >= partition.End)
                continue;

            anchorFound = true;
            ratio       = (uint)position[1];

            break;
        }

        if(!anchorFound)
            return false;

        ulong count = 0;

        while(count < 256)
        {
            ErrorNumber errno =
                imagePlugin.
                    ReadSectors(
                        partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location * ratio + count * ratio,
                        ratio, out sector);

            if(errno != ErrorNumber.NoError)
            {
                count++;

                continue;
            }

            var tagId    = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
            var location = BitConverter.ToUInt32(sector, 0x0C);

            if(location == partition.Start / ratio + anchor.mainVolumeDescriptorSequenceExtent.location + count)
            {
                if(tagId == TagIdentifier.TerminatingDescriptor)
                    break;

                if(tagId == TagIdentifier.LogicalVolumeDescriptor)
                {
                    LogicalVolumeDescriptor lvd =
                        Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(sector);

                    return _magic.SequenceEqual(lvd.domainIdentifier.identifier);
                }
            }
            else
                break;

            count++;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        ErrorNumber errno;
        metadata = new FileSystem();

        // UDF is always UTF-8
        encoding = Encoding.UTF8;
        byte[] sector;

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Universal_Disk_Format);

        var anchor = new AnchorVolumeDescriptorPointer();

        // All positions where anchor may reside, with the ratio between 512 and 2048bps
        ulong[][] positions =
        {
            new ulong[] { 256, 1 }, new ulong[] { 512, 1 }, new ulong[] { partition.End - 256, 1 },
            new ulong[] { partition.End, 1 }, new ulong[] { 1024, 4 }, new ulong[] { 2048, 4 },
            new ulong[] { partition.End - 1024, 4 }, new ulong[] { partition.End - 4, 4 }
        };

        uint ratio = 1;

        foreach(ulong[] position in positions)
        {
            errno = imagePlugin.ReadSectors(position[0], (uint)position[1], out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(sector);

            if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               anchor.tag.tagLocation != position[0] / position[1] ||
               anchor.mainVolumeDescriptorSequenceExtent.location + partition.Start >= partition.End)
                continue;

            ratio = (uint)position[1];

            break;
        }

        ulong count = 0;

        var pvd    = new PrimaryVolumeDescriptor();
        var lvd    = new LogicalVolumeDescriptor();
        var lvidiu = new LogicalVolumeIntegrityDescriptorImplementationUse();

        while(count < 256)
        {
            errno =
                imagePlugin.
                    ReadSectors(
                        partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location * ratio + count * ratio,
                        ratio, out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            var tagId    = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
            var location = BitConverter.ToUInt32(sector, 0x0C);

            if(location == partition.Start / ratio + anchor.mainVolumeDescriptorSequenceExtent.location + count)
            {
                if(tagId == TagIdentifier.TerminatingDescriptor)
                    break;

                switch(tagId)
                {
                    case TagIdentifier.LogicalVolumeDescriptor:
                        lvd = Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(sector);

                        break;
                    case TagIdentifier.PrimaryVolumeDescriptor:
                        pvd = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(sector);

                        break;
                }
            }
            else
                break;

            count++;
        }

        errno = imagePlugin.ReadSectors(lvd.integritySequenceExtent.location * ratio, ratio, out sector);

        if(errno != ErrorNumber.NoError)
            return;

        LogicalVolumeIntegrityDescriptor lvid =
            Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptor>(sector);

        if(lvid.tag.tagIdentifier == TagIdentifier.LogicalVolumeIntegrityDescriptor &&
           lvid.tag.tagLocation   == lvd.integritySequenceExtent.location)
        {
            lvidiu =
                Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptorImplementationUse>(sector,
                    (int)(lvid.numberOfPartitions * 8 + 80),
                    System.Runtime.InteropServices.Marshal.SizeOf(lvidiu));
        }
        else
            lvid = new LogicalVolumeIntegrityDescriptor();

        sbInformation.AppendFormat(Localization.Volume_is_number_0_of_1, pvd.volumeSequenceNumber,
                                   pvd.maximumVolumeSequenceNumber).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_set_identifier_0,
                                   StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier)).AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_name_0, StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier)).
            AppendLine();

        sbInformation.AppendFormat(Localization.Volume_uses_0_bytes_per_block, lvd.logicalBlockSize).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_was_last_written_on_0, EcmaToDateTime(lvid.recordingDateTime)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_contains_0_partitions, lvid.numberOfPartitions).AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_contains_0_files_and_1_directories, lvidiu.files, lvidiu.directories).
            AppendLine();

        sbInformation.AppendFormat(Localization.Volume_conforms_to_0,
                                   encoding.GetString(lvd.domainIdentifier.identifier).TrimEnd('\u0000')).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_was_last_written_by_0,
                                   encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000')).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_requires_UDF_version_0_1_to_be_read,
                                   Convert.ToInt32($"{(lvidiu.minimumReadUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.minimumReadUDF & 0xFF}",          10)).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_requires_UDF_version_0_1_to_be_written_to,
                                   Convert.ToInt32($"{(lvidiu.minimumWriteUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.minimumWriteUDF & 0xFF}",          10)).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_cannot_be_written_by_any_UDF_version_higher_than_0_1,
                                   Convert.ToInt32($"{(lvidiu.maximumWriteUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.maximumWriteUDF & 0xFF}",          10)).AppendLine();

        metadata = new FileSystem
        {
            Type                  = FS_TYPE,
            ApplicationIdentifier = encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'),
            ClusterSize           = lvd.logicalBlockSize,
            ModificationDate      = EcmaToDateTime(lvid.recordingDateTime),
            Files                 = lvidiu.files,
            VolumeName            = StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier),
            VolumeSetIdentifier   = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
            VolumeSerial          = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
            SystemIdentifier      = encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000')
        };

        metadata.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / metadata.ClusterSize;

        information = sbInformation.ToString();
    }

#endregion
}