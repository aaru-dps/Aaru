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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Universal Disk Format filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Kept for completeness of on-disk structures")]
public sealed partial class UDF
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // UDF needs at least that
        if(partition.End - partition.Start < 256) return false;

        // UDF needs at least that
        if(imagePlugin.Info.SectorSize < 512) return false;

        // Volume Recognition Sequence starts at sector 16 (for 2048 bps) or byte offset 0x8000 (for other bps)
        uint sectorSize = imagePlugin.Info.SectorSize;

        if(sectorSize == 2352) sectorSize = 2048;

        // Calculate starting sector: sector 16 for 2048 bps, or 0x8000 / sectorSize for other sizes
        ulong vrsStart = sectorSize == 2048 ? 16 : 0x8000 / sectorSize;

        // Search through the Volume Recognition Sequence for BEA
        // The VRS can contain various descriptors (including ISO 9660's CD001)
        // We must traverse them all until finding BEA
        var    beaFound  = false;
        ulong  beaSector = 0;
        byte[] buffer    = [];
        var    mrw       = false;

        for(var pass = 0; pass < 2 && !beaFound; pass++)
        {
            // On the second pass, look for the VRS as laid out on a raw (non-compatible mode) MRW medium
            if(pass == 1)
            {
                if(!MrwPossible(imagePlugin)) break;

                mrw = true;
            }

            for(ulong i = 0; i < 32; i++) // Search up to 32 sectors
            {
                ulong sector = vrsStart + i;

                if(ReadMrwAwareSector(imagePlugin, mrw, sector, out buffer) != ErrorNumber.NoError) continue;

                // Check for BEA01 identifier at offset 1
                if(buffer.Length < 6 || !buffer[1..6].SequenceEqual(_bea)) continue;

                beaFound  = true;
                beaSector = sector;

                break;
            }
        }

        if(!beaFound) return false;

        // Now search within the extended area (after BEA) for NSR02/NSR03 before TEA
        var foundNsr = false;

        for(ulong i = 1; i < 16; i++)
        {
            ulong sector = beaSector + i;

            if(ReadMrwAwareSector(imagePlugin, mrw, sector, out buffer) != ErrorNumber.NoError) continue;

            // Check identifier at offset 1-5
            if(buffer.Length < 6) continue;

            // Found NSR02 or NSR03 - this media is recorded according to ECMA-167
            if(buffer[1..6].SequenceEqual(_nsr) || buffer[1..6].SequenceEqual(_nsr3))
            {
                foundNsr = true;

                continue;
            }

            // Found TEA01 - Terminating Extended Area Descriptor, stop searching
            if(buffer[1..6].SequenceEqual(_tea)) break;
        }

        if(!foundNsr) return false;

        // Now search for anchor volume descriptor pointer
        var anchor = new AnchorVolumeDescriptorPointer();

        // On a raw MRW medium only the user data area is addressable by the volume
        ulong sectors = mrw ? MrwUserSectors(imagePlugin.Info.Sectors) : imagePlugin.Info.Sectors;

        // All positions where anchor may reside
        ulong[] anchorPositions = [256, sectors - 256, sectors - 1];

        var anchorFound = false;

        foreach(ulong position in from position in anchorPositions
                                  let errno = ReadMrwAwareSector(imagePlugin, mrw, position, out buffer)
                                  where errno == ErrorNumber.NoError
                                  select position)
        {
            anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(buffer);

            if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               anchor.tag.tagLocation   != position)
                continue;

            anchorFound = true;

            break;
        }

        if(!anchorFound) return false;

        // Search for Logical Volume Descriptor to confirm it's UDF
        ulong count = 0;

        while(count < 256)
        {
            ErrorNumber errno = ReadMrwAwareSector(imagePlugin,
                                                   mrw,
                                                   anchor.mainVolumeDescriptorSequenceExtent.location + count,
                                                   out buffer);

            if(errno != ErrorNumber.NoError)
            {
                count++;

                continue;
            }

            var tagId    = (TagIdentifier)BitConverter.ToUInt16(buffer, 0);
            var location = BitConverter.ToUInt32(buffer, 0x0C);

            if(location == anchor.mainVolumeDescriptorSequenceExtent.location + count)
            {
                if(tagId == TagIdentifier.TerminatingDescriptor) break;

                if(tagId == TagIdentifier.LogicalVolumeDescriptor)
                {
                    LogicalVolumeDescriptor lvd =
                        Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(buffer);

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
        byte[] buffer;

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Universal_Disk_Format);

        // Volume Recognition Sequence starts at sector 16 (for 2048 bps) or byte offset 0x8000 (for other bps)
        uint sectorSize = imagePlugin.Info.SectorSize;

        if(sectorSize == 2352) sectorSize = 2048;

        // Calculate starting sector: sector 16 for 2048 bps, or 0x8000 / sectorSize for other sizes
        ulong vrsStart = sectorSize == 2048 ? 16 : 0x8000 / sectorSize;

        // Search through the Volume Recognition Sequence for BEA
        // The VRS can contain various descriptors (including ISO 9660's CD001)
        // We must traverse them all until finding BEA
        var   beaFound  = false;
        ulong beaSector = 0;
        var   mrw       = false;

        for(var pass = 0; pass < 2 && !beaFound; pass++)
        {
            // On the second pass, look for the VRS as laid out on a raw (non-compatible mode) MRW medium
            if(pass == 1)
            {
                if(!MrwPossible(imagePlugin)) break;

                mrw = true;
            }

            for(ulong i = 0; i < 32; i++) // Search up to 32 sectors
            {
                ulong sector = vrsStart + i;

                if(ReadMrwAwareSector(imagePlugin, mrw, sector, out buffer) != ErrorNumber.NoError) continue;

                // Check for BEA01 identifier at offset 1
                if(buffer.Length < 6 || !buffer[1..6].SequenceEqual(_bea)) continue;

                beaFound  = true;
                beaSector = sector;

                break;
            }
        }

        if(!beaFound) return;

        // Now search within the extended area (after BEA) for NSR02/NSR03 before TEA
        var foundNsr = false;

        for(ulong i = 1; i < 16; i++)
        {
            ulong sector = beaSector + i;

            if(ReadMrwAwareSector(imagePlugin, mrw, sector, out buffer) != ErrorNumber.NoError) continue;

            // Check identifier at offset 1-5
            if(buffer.Length < 6) continue;

            // Found NSR02 or NSR03 - this media is recorded according to ECMA-167
            if(buffer[1..6].SequenceEqual(_nsr) || buffer[1..6].SequenceEqual(_nsr3))
            {
                foundNsr = true;

                continue;
            }

            // Found TEA01 - Terminating Extended Area Descriptor, stop searching
            if(buffer[1..6].SequenceEqual(_tea)) break;
        }

        if(!foundNsr) return;

        // Now search for anchor volume descriptor pointer
        var anchor = new AnchorVolumeDescriptorPointer();

        // On a raw MRW medium only the user data area is addressable by the volume
        ulong sectors = mrw ? MrwUserSectors(imagePlugin.Info.Sectors) : imagePlugin.Info.Sectors;

        // All positions where anchor may reside
        ulong[] anchorPositions = [256, sectors - 256, sectors - 1];

        var anchorFound = false;

        foreach(ulong position in anchorPositions)
        {
            errno = ReadMrwAwareSector(imagePlugin, mrw, position, out buffer);

            if(errno != ErrorNumber.NoError) continue;

            anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(buffer);

            if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               anchor.tag.tagLocation   != position)
                continue;

            anchorFound = true;

            break;
        }

        if(!anchorFound) return;

        ulong count = 0;

        var pvd    = new PrimaryVolumeDescriptor();
        var lvd    = new LogicalVolumeDescriptor();
        var lvidiu = new LogicalVolumeIntegrityDescriptorImplementationUse();

        while(count < 256)
        {
            errno = ReadMrwAwareSector(imagePlugin,
                                       mrw,
                                       anchor.mainVolumeDescriptorSequenceExtent.location + count,
                                       out buffer);

            if(errno != ErrorNumber.NoError)
            {
                count++;

                continue;
            }

            var tagId    = (TagIdentifier)BitConverter.ToUInt16(buffer, 0);
            var location = BitConverter.ToUInt32(buffer, 0x0C);

            if(location == anchor.mainVolumeDescriptorSequenceExtent.location + count)
            {
                if(tagId == TagIdentifier.TerminatingDescriptor) break;

                switch(tagId)
                {
                    case TagIdentifier.LogicalVolumeDescriptor:
                        lvd = Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(buffer);

                        break;
                    case TagIdentifier.PrimaryVolumeDescriptor:
                        pvd = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(buffer);

                        break;
                }
            }
            else
                break;

            count++;
        }

        errno = ReadMrwAwareSector(imagePlugin, mrw, lvd.integritySequenceExtent.location, out buffer);

        if(errno != ErrorNumber.NoError) return;

        LogicalVolumeIntegrityDescriptor lvid =
            Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptor>(buffer);

        if(lvid.tag.tagIdentifier == TagIdentifier.LogicalVolumeIntegrityDescriptor &&
           lvid.tag.tagLocation   == lvd.integritySequenceExtent.location)
        {
            lvidiu = Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptorImplementationUse>(buffer,
                (int)(lvid.numberOfPartitions * 8 + 80),
                System.Runtime.InteropServices.Marshal.SizeOf(lvidiu));
        }
        else
            lvid = new LogicalVolumeIntegrityDescriptor();

        sbInformation.AppendFormat(Localization.Volume_is_number_0_of_1,
                                   pvd.volumeSequenceNumber,
                                   pvd.maximumVolumeSequenceNumber)
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_set_identifier_0,
                                   StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier))
                     .AppendLine();

        sbInformation
           .AppendFormat(Localization.Volume_name_0, StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier))
           .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_uses_0_bytes_per_block, lvd.logicalBlockSize).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_was_last_written_on_0, EcmaToDateTime(lvid.recordingDateTime))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_contains_0_partitions, lvid.numberOfPartitions).AppendLine();

        sbInformation
           .AppendFormat(Localization.Volume_contains_0_files_and_1_directories, lvidiu.files, lvidiu.directories)
           .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_conforms_to_0,
                                   encoding.GetString(lvd.domainIdentifier.identifier).TrimEnd('\u0000'))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_was_last_written_by_0,
                                   encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_requires_UDF_version_0_1_to_be_read,
                                   Convert.ToInt32($"{(lvidiu.minimumReadUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.minimumReadUDF & 0xFF}",          10))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_requires_UDF_version_0_1_to_be_written_to,
                                   Convert.ToInt32($"{(lvidiu.minimumWriteUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.minimumWriteUDF & 0xFF}",          10))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_cannot_be_written_by_any_UDF_version_higher_than_0_1,
                                   Convert.ToInt32($"{(lvidiu.maximumWriteUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.maximumWriteUDF & 0xFF}",          10))
                     .AppendLine();

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
            SystemIdentifier      = encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'),
            Bootable              = IsBootable(imagePlugin, mrw)
        };

        metadata.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / metadata.ClusterSize;

        information = sbInformation.ToString();
    }

    /// <summary>
    ///     Checks if the UDF volume is bootable by scanning the Volume Recognition Sequence
    ///     for Boot Descriptors with the "BOOT2" identifier per ECMA-167.
    ///     The BOOT2 descriptor must appear within the extended area (after BEA, before TEA).
    /// </summary>
    /// <param name="imagePlugin">The media image</param>
    /// <param name="mrw">Medium follows the raw (non-compatible mode) MRW layout</param>
    /// <returns>True if the volume contains a valid Boot Descriptor</returns>
    static bool IsBootable(IMediaImage imagePlugin, bool mrw)
    {
        // Volume Recognition Sequence starts at sector 16 (for 2048 bps) or byte offset 0x8000 (for other bps)
        uint sectorSize = imagePlugin.Info.SectorSize;

        if(sectorSize == 2352) sectorSize = 2048;

        // Calculate starting sector: sector 16 for 2048 bps, or 0x8000 / sectorSize for other sizes
        ulong vrsStart = sectorSize == 2048 ? 16 : 0x8000 / sectorSize;

        // Search through the Volume Recognition Sequence for BEA
        ulong  beaSector = 0;
        var    beaFound  = false;
        byte[] buffer;

        for(ulong i = 0; i < 32; i++)
        {
            ulong sector = vrsStart + i;

            if(ReadMrwAwareSector(imagePlugin, mrw, sector, out buffer) != ErrorNumber.NoError) continue;

            // Check for BEA01 identifier at offset 1
            if(buffer.Length < 6 || !buffer[1..6].SequenceEqual(_bea)) continue;

            beaFound  = true;
            beaSector = sector;

            break;
        }

        if(!beaFound) return false;

        // Search within the extended area (after BEA) for BOOT2 before TEA
        for(ulong i = 1; i < 16; i++)
        {
            ulong sector = beaSector + i;

            if(ReadMrwAwareSector(imagePlugin, mrw, sector, out buffer) != ErrorNumber.NoError) continue;

            if(buffer.Length < 6) continue;

            // Check for Boot Descriptor (type 0, identifier "BOOT2")
            if(buffer[0] == 0 && buffer[1..6].SequenceEqual(_boot2)) return true;

            // Found TEA01 - Terminating Extended Area Descriptor, stop searching
            if(buffer[1..6].SequenceEqual(_tea)) break;
        }

        return false;
    }

#endregion
}