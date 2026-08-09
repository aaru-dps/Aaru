// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
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
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.Logging;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class UDF
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
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
        byte[] buffer;

        _mrw = false;

        for(var pass = 0; pass < 2 && !beaFound; pass++)
        {
            // On the second pass, look for the VRS as laid out on a raw (non-compatible mode) MRW medium
            if(pass == 1)
            {
                if(!MrwPossible(imagePlugin)) break;

                _mrw = true;
            }

            for(ulong i = 0; i < 32; i++) // Search up to 32 sectors
            {
                ulong sector = vrsStart + i;

                if(ReadMrwAwareSector(imagePlugin, _mrw, sector, out buffer) != ErrorNumber.NoError) continue;

                // Check for BEA01 identifier at offset 1
                if(buffer.Length < 6 || !buffer[1..6].SequenceEqual(_bea)) continue;

                beaFound  = true;
                beaSector = sector;

                break;
            }
        }

        if(!beaFound) return ErrorNumber.InvalidArgument;

        if(_mrw) AaruLogging.Debug(MODULE_NAME, Localization.Raw_MRW_layout_detected);

        // Now search within the extended area (after BEA) for NSR02/NSR03 before TEA
        var foundNsr = false;

        for(ulong i = 1; i < 16; i++)
        {
            ulong sector = beaSector + i;

            if(ReadMrwAwareSector(imagePlugin, _mrw, sector, out buffer) != ErrorNumber.NoError)
                return ErrorNumber.InvalidArgument;

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

        if(!foundNsr) return ErrorNumber.InvalidArgument;

        // Next we search for the anchor volume descriptor pointer
        // It should be at sector 256, N-256 or N, with N being the last sector of the volume
        // UDF does not play well with partitioning schemes
        AnchorVolumeDescriptorPointer avdp      = default;
        var                           avdpFound = false;

        // On a raw MRW medium only the user data area is addressable by the volume
        ulong sectors = _mrw ? MrwUserSectors(imagePlugin.Info.Sectors) : imagePlugin.Info.Sectors;

        foreach(ulong location in new[]
                {
                    256UL, sectors - 256, sectors - 1
                })
        {
            if(ReadMrwAwareSector(imagePlugin, _mrw, location, out buffer) != ErrorNumber.NoError)
                return ErrorNumber.InvalidArgument;

            avdp = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(buffer);

            if(avdp.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               avdp.tag.tagLocation   != location)
                continue;

            avdpFound = true;

            break;
        }

        if(!avdpFound) return ErrorNumber.InvalidArgument;

        // Parse Volume Descriptor Sequence to find PVD, LVD, and Partition Descriptor(s)
        var                     partitionDescriptors = new Dictionary<ushort, PartitionDescriptor>();
        PrimaryVolumeDescriptor pvd                  = default;
        LogicalVolumeDescriptor lvd                  = default;
        var                     foundPvd             = false;
        var                     foundLvd             = false;
        uint                    vdsLength            = avdp.mainVolumeDescriptorSequenceExtent.length / sectorSize;
        uint                    vdsLocation          = avdp.mainVolumeDescriptorSequenceExtent.location;

        for(uint i = 0; i < vdsLength; i++)
        {
            if(ReadMrwAwareSector(imagePlugin, _mrw, vdsLocation + i, out buffer) != ErrorNumber.NoError) continue;

            var tagId = (TagIdentifier)BitConverter.ToUInt16(buffer, 0);

            switch(tagId)
            {
                case TagIdentifier.TerminatingDescriptor:
                    i = vdsLength; // Exit loop

                    break;

                case TagIdentifier.PrimaryVolumeDescriptor:
                    pvd      = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(buffer);
                    foundPvd = pvd.tag.tagLocation == vdsLocation + i;

                    break;

                case TagIdentifier.LogicalVolumeDescriptor:
                    lvd      = Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(buffer);
                    foundLvd = lvd.tag.tagLocation == vdsLocation + i;

                    break;

                case TagIdentifier.PartitionDescriptor:
                    PartitionDescriptor pd = Marshal.ByteArrayToStructureLittleEndian<PartitionDescriptor>(buffer);

                    if(pd.tag.tagLocation == vdsLocation + i) partitionDescriptors[pd.partitionNumber] = pd;

                    break;
            }
        }

        if(!foundPvd || !foundLvd || partitionDescriptors.Count == 0) return ErrorNumber.InvalidArgument;

        // Not UDF
        if(!lvd.domainIdentifier.identifier.SequenceEqual(_magic)) return ErrorNumber.InvalidArgument;

        // Read the Logical Volume Integrity Descriptor to check UDF revision
        if(ReadMrwAwareSector(imagePlugin, _mrw, lvd.integritySequenceExtent.location, out byte[] lvidBuffer) !=
           ErrorNumber.NoError)
            return ErrorNumber.InvalidArgument;

        LogicalVolumeIntegrityDescriptor lvid =
            Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptor>(lvidBuffer);

        if(lvid.tag.tagIdentifier != TagIdentifier.LogicalVolumeIntegrityDescriptor ||
           lvid.tag.tagLocation   != lvd.integritySequenceExtent.location)
            return ErrorNumber.InvalidArgument;

        // The Implementation Use area follows the free space and size tables
        // Offset = 80 (fixed LVID header) + numberOfPartitions * 4 (free space) + numberOfPartitions * 4 (size)
        LogicalVolumeIntegrityDescriptorImplementationUse lvidiu =
            Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptorImplementationUse>(lvidBuffer,
                (int)(80 + lvid.numberOfPartitions * 8),
                System.Runtime.InteropServices.Marshal
                      .SizeOf<LogicalVolumeIntegrityDescriptorImplementationUse>());

        // Store UDF version
        _udfVersion = lvidiu.minimumReadUDF;

        // Support UDF versions up to 2.60
        // UDF 1.02 = 0x0102, UDF 1.50 = 0x0150, UDF 2.00 = 0x0200, UDF 2.01 = 0x0201, UDF 2.50 = 0x0250, UDF 2.60 = 0x0260
        if(lvidiu.minimumReadUDF > UDF_VERSION_260) return ErrorNumber.InvalidArgument;

        // Save instance fields needed for partition reading
        _imagePlugin = imagePlugin;
        _sectorSize  = sectorSize;

        // Parse partition maps from LVD to handle Type 1, Virtual, Sparable, and Metadata partitions
        ErrorNumber errno = ParsePartitionMaps(lvd, partitionDescriptors);

        if(errno != ErrorNumber.NoError) return errno;

        // For UDF 1.50+ with virtual partition, we need to load the VAT
        if(_hasVirtualPartition)
        {
            errno = LoadVirtualAllocationTable(imagePlugin, partitionDescriptors);

            if(errno != ErrorNumber.NoError) return errno;
        }

        // For sparable partitions, load the sparing table
        if(_hasSparablePartition)
        {
            errno = LoadSparingTable(imagePlugin);

            if(errno != ErrorNumber.NoError) return errno;
        }

        // For UDF 2.50+ metadata partitions, load the metadata file
        if(_hasMetadataPartition)
        {
            errno = LoadMetadataPartition(imagePlugin, partitionDescriptors);

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Get the first partition for FSD location
        if(!partitionDescriptors.TryGetValue(0, out PartitionDescriptor firstPartition))
        {
            // Use the first available partition if partition 0 doesn't exist
            using Dictionary<ushort, PartitionDescriptor>.ValueCollection.Enumerator enumerator =
                partitionDescriptors.Values.GetEnumerator();

            if(!enumerator.MoveNext()) return ErrorNumber.InvalidArgument;

            firstPartition = enumerator.Current;
        }

        // Save partition starting location for offset calculations
        // This must be set before ReadSectorFromPartition calls below
        _partitionStartingLocation = firstPartition.partitionStartingLocation;

        // The logicalVolumeContentsUse field in the LVD contains a long_ad pointing to the File Set Descriptor
        LongAllocationDescriptor fsdLocation =
            Marshal.ByteArrayToStructureLittleEndian<LongAllocationDescriptor>(lvd.logicalVolumeContentsUse);

        // Read the File Set Descriptor using partition-aware read (handles metadata partitions)
        errno = ReadSectorFromPartition(fsdLocation.extentLocation.logicalBlockNumber,
                                        fsdLocation.extentLocation.partitionReferenceNumber,
                                        _partitionStartingLocation,
                                        out buffer);

        if(errno != ErrorNumber.NoError) return errno;

        FileSetDescriptor fsd = Marshal.ByteArrayToStructureLittleEndian<FileSetDescriptor>(buffer);

        if(fsd.tag.tagIdentifier != TagIdentifier.FileSetDescriptor) return ErrorNumber.InvalidArgument;

        // The rootDirectoryICB contains the ICB of the root directory
        _rootDirectoryIcb = fsd.rootDirectoryICB;

        // Verify the root directory ICB using partition-aware read (handles metadata partitions)
        errno = ReadSectorFromPartition(_rootDirectoryIcb.extentLocation.logicalBlockNumber,
                                        _rootDirectoryIcb.extentLocation.partitionReferenceNumber,
                                        _partitionStartingLocation,
                                        out buffer);

        if(errno != ErrorNumber.NoError) return errno;

        // Check for FileEntry or ExtendedFileEntry (UDF 2.00+)
        var rootTagId = (TagIdentifier)BitConverter.ToUInt16(buffer, 0);

        if(rootTagId != TagIdentifier.FileEntry && rootTagId != TagIdentifier.ExtendedFileEntry)
            return ErrorNumber.InvalidArgument;

        // Fill filesystem info from the descriptors
        // Free space table is at offset 80 in LVID, each entry is 4 bytes per partition
        uint freeBlocks = 0;

        for(uint i = 0; i < lvid.numberOfPartitions; i++)
            freeBlocks += BitConverter.ToUInt32(lvidBuffer, (int)(80 + i * 4));

        _statfs = new FileSystemInfo
        {
            Blocks         = firstPartition.partitionLength,
            FilenameLength = 254, // UDF max filename length
            Files          = lvidiu.files + lvidiu.directories,
            FreeBlocks     = freeBlocks,
            FreeFiles      = 0, // UDF doesn't have a fixed inode table
            PluginId       = Id,
            Type           = $"UDF {lvidiu.minimumReadUDF >> 8}.{lvidiu.minimumReadUDF & 0xFF:D2}"
        };

        // Fill Metadata
        Metadata = new FileSystem
        {
            Type                  = FS_TYPE,
            ClusterSize           = lvd.logicalBlockSize,
            Clusters              = firstPartition.partitionLength,
            FreeClusters          = freeBlocks,
            Files                 = lvidiu.files + lvidiu.directories,
            VolumeName            = StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier),
            VolumeSetIdentifier   = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
            VolumeSerial          = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
            ModificationDate      = EcmaToDateTime(lvid.recordingDateTime),
            ApplicationIdentifier = Encoding.ASCII.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'),
            SystemIdentifier      = Encoding.ASCII.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'),
            Bootable              = IsBootable(imagePlugin, _mrw)
        };


        // Initialize directory caches
        _rootDirectoryCache = [];
        _directoryCache     = [];

        // Read root directory
        errno = ReadDirectoryContents(_rootDirectoryIcb, out Dictionary<string, UdfDirectoryEntry> rootEntries);

        if(errno != ErrorNumber.NoError) return errno;

        _rootDirectoryCache = rootEntries;

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        // Clear directory caches
        _rootDirectoryCache?.Clear();
        _directoryCache?.Clear();

        _rootDirectoryCache = null;
        _directoryCache     = null;

        // Clear instance fields
        _imagePlugin               = null;
        _sectorSize                = 0;
        _mrw                       = false;
        _partitionStartingLocation = 0;
        _rootDirectoryIcb          = default(LongAllocationDescriptor);
        _statfs                    = null;
        Metadata                   = null;

        // Clear UDF 1.50 fields
        _vat                     = null;
        _sparingTable            = null;
        _partitionMap            = null;
        _hasVirtualPartition     = false;
        _hasSparablePartition    = false;
        _virtualPartitionNumber  = 0;
        _sparablePartitionNumber = 0;
        _sparablePacketLength    = 0;
        _udfVersion              = 0;

        // Clear UDF 2.50 metadata partition fields
        _hasMetadataPartition       = false;
        _metadataPartitionNumber    = 0;
        _metadataFileLocation       = 0;
        _metadataMirrorFileLocation = 0;
        _metadataAllocationUnitSize = 0;
        _metadataFileData           = null;

        _mounted = false;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Parses partition maps from the Logical Volume Descriptor to identify
    ///     Type 1, Virtual, and Sparable partitions.
    /// </summary>
    ErrorNumber ParsePartitionMaps(LogicalVolumeDescriptor                 lvd,
                                   Dictionary<ushort, PartitionDescriptor> partitionDescriptors)
    {
        _partitionMap         = [];
        _hasVirtualPartition  = false;
        _hasSparablePartition = false;
        _hasMetadataPartition = false;

        var offset = 0;

        for(uint i = 0; i < lvd.numberOfPartitionMaps; i++)
        {
            if(offset >= lvd.partitionMaps.Length) break;

            byte mapType   = lvd.partitionMaps[offset];
            byte mapLength = lvd.partitionMaps[offset + 1];

            if(mapLength == 0) break;

            switch(mapType)
            {
                case 1:
                {
                    // Type 1 Partition Map - direct mapping
                    Type1PartitionMap type1Map =
                        Marshal.ByteArrayToStructureLittleEndian<Type1PartitionMap>(lvd.partitionMaps,
                            offset,
                            System.Runtime.InteropServices.Marshal.SizeOf<Type1PartitionMap>());

                    _partitionMap[(ushort)i] = type1Map.partitionNumber;

                    break;
                }

                case 2:
                {
                    // Type 2 Partition Map - check for Virtual, Sparable, or Metadata
                    Type2PartitionMapHeader type2Header =
                        Marshal.ByteArrayToStructureLittleEndian<Type2PartitionMapHeader>(lvd.partitionMaps,
                            offset,
                            System.Runtime.InteropServices.Marshal.SizeOf<Type2PartitionMapHeader>());

                    if(CompareIdentifier(type2Header.partitionTypeIdentifier.identifier, _udf_VirtualPartition))
                    {
                        // Virtual Partition Map
                        VirtualPartitionMap virtualMap =
                            Marshal.ByteArrayToStructureLittleEndian<VirtualPartitionMap>(lvd.partitionMaps,
                                offset,
                                System.Runtime.InteropServices.Marshal.SizeOf<VirtualPartitionMap>());

                        _hasVirtualPartition     = true;
                        _virtualPartitionNumber  = (ushort)i;
                        _partitionMap[(ushort)i] = virtualMap.partitionNumber;
                    }
                    else if(CompareIdentifier(type2Header.partitionTypeIdentifier.identifier, _udf_SparablePartition))
                    {
                        // Sparable Partition Map
                        SparablePartitionMap sparableMap =
                            Marshal.ByteArrayToStructureLittleEndian<SparablePartitionMap>(lvd.partitionMaps,
                                offset,
                                System.Runtime.InteropServices.Marshal.SizeOf<SparablePartitionMap>());

                        _hasSparablePartition    = true;
                        _sparablePartitionNumber = (ushort)i;
                        _sparablePacketLength    = sparableMap.packetLength;
                        _partitionMap[(ushort)i] = sparableMap.partitionNumber;
                    }
                    else if(CompareIdentifier(type2Header.partitionTypeIdentifier.identifier, _udf_MetadataPartition))
                    {
                        // Metadata Partition Map (UDF 2.50+)
                        MetadataPartitionMap metadataMap =
                            Marshal.ByteArrayToStructureLittleEndian<MetadataPartitionMap>(lvd.partitionMaps,
                                offset,
                                System.Runtime.InteropServices.Marshal.SizeOf<MetadataPartitionMap>());

                        _hasMetadataPartition       = true;
                        _metadataPartitionNumber    = (ushort)i;
                        _metadataFileLocation       = metadataMap.metadataFileLocation;
                        _metadataMirrorFileLocation = metadataMap.metadataMirrorFileLocation;
                        _metadataAllocationUnitSize = metadataMap.allocationUnitSize;
                        _partitionMap[(ushort)i]    = metadataMap.partitionNumber;
                    }
                    else
                    {
                        // Unknown Type 2 map, use partition number from header
                        _partitionMap[(ushort)i] = type2Header.partitionNumber;
                    }

                    break;
                }
            }

            offset += mapLength;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Loads the Virtual Allocation Table (VAT) for UDF 1.50 virtual partitions.
    ///     The VAT is stored as a file at the end of the recorded area.
    /// </summary>
    ErrorNumber LoadVirtualAllocationTable(IMediaImage                             imagePlugin,
                                           Dictionary<ushort, PartitionDescriptor> partitionDescriptors)
    {
        // In UDF 1.50, the VAT ICB is located in the last recorded sector
        // We need to search backwards from the end of the partition for the VAT file entry

        // Get the physical partition that the virtual partition references
        if(!_partitionMap.TryGetValue(_virtualPartitionNumber, out ushort physicalPartitionNum))
            return ErrorNumber.InvalidArgument;

        if(!partitionDescriptors.TryGetValue(physicalPartitionNum, out PartitionDescriptor physicalPartition))
            return ErrorNumber.InvalidArgument;

        // Search backwards from the last sector for the VAT file entry
        // The VAT ICB should be at the last valid sector of the partition
        ulong partitionEnd = physicalPartition.partitionStartingLocation + physicalPartition.partitionLength - 1;

        // Search up to 256 sectors back for the VAT
        for(ulong i = 0; i < 256; i++)
        {
            ulong sector = partitionEnd - i;

            if(sector < physicalPartition.partitionStartingLocation) break;

            if(ReadMrwAwareSector(imagePlugin, _mrw, sector, out byte[] buffer) != ErrorNumber.NoError) continue;

            if(buffer.Length < 16) continue;

            var tagId = (TagIdentifier)BitConverter.ToUInt16(buffer, 0);

            if(tagId != TagIdentifier.FileEntry) continue;

            // Found a file entry, check if it's the VAT
            FileEntry fe = Marshal.ByteArrayToStructureLittleEndian<FileEntry>(buffer);

            // VAT file type is 248 (0xF8) in UDF 1.50
            if(fe.icbTag.fileType != (FileType)248) continue;

            // Found the VAT file entry, read the VAT data
            var adType = (byte)((ushort)fe.icbTag.flags & 0x07);

            ErrorNumber errno = ReadFileData(fe, buffer, adType, 0, out byte[] vatData);

            if(errno != ErrorNumber.NoError) return errno;

            // Parse the VAT - in UDF 1.50 it's just an array of uint32 values
            // The last entry is the previous VAT ICB location (for multi-session)
            int vatEntries = vatData.Length / 4;

            if(vatEntries > 0)
            {
                // Last entry is previous VAT ICB, not used for address translation
                _vat = new uint[vatEntries - 1];

                for(var j = 0; j < vatEntries - 1; j++) _vat[j] = BitConverter.ToUInt32(vatData, j * 4);
            }

            return ErrorNumber.NoError;
        }

        // VAT not found, this might be a problem for virtual partitions
        return ErrorNumber.InvalidArgument;
    }

    /// <summary>
    ///     Loads the Sparing Table for UDF 1.50 sparable partitions.
    /// </summary>
    ErrorNumber LoadSparingTable(IMediaImage imagePlugin)
    {
        _sparingTable = [];

        // The sparing table locations are stored in the sparable partition map
        // We need to re-parse the partition maps to get the sparing table locations
        // For now, we'll try common locations

        // Search for the sparing table in the volume
        // It's typically located early in the volume and has a specific tag

        // Try to find it by scanning the first part of the volume
        for(ulong sector = 0; sector < 256; sector++)
        {
            if(ReadMrwAwareSector(imagePlugin, _mrw, sector, out byte[] buffer) != ErrorNumber.NoError) continue;

            if(buffer.Length < 24) continue;

            // Check for sparing table tag (it uses a specific descriptor tag)
            var tagId = (TagIdentifier)BitConverter.ToUInt16(buffer, 0);

            // Sparing table uses tag ID 0 with a specific identifier
            if(tagId != 0) continue;

            // Check for "*UDF Sparing Table" identifier
            SparingTable st =
                Marshal.ByteArrayToStructureLittleEndian<SparingTable>(buffer,
                                                                       0,
                                                                       System.Runtime.InteropServices.Marshal
                                                                             .SizeOf<SparingTable>());

            if(!CompareIdentifier(st.sparingIdentifier.identifier, _udf_SparingTable)) continue;

            // Found the sparing table, parse the entries
            int entryOffset = System.Runtime.InteropServices.Marshal.SizeOf<SparingTable>();
            int entrySize   = System.Runtime.InteropServices.Marshal.SizeOf<SparingTableEntry>();

            for(var i = 0; i < st.reallocationTableLength; i++)
            {
                if(entryOffset + entrySize > buffer.Length) break;

                SparingTableEntry entry =
                    Marshal.ByteArrayToStructureLittleEndian<SparingTableEntry>(buffer, entryOffset, entrySize);

                // Only add valid mappings (not 0xFFFFFFFF which means available spare)
                if(entry.originalLocation != 0xFFFFFFFF && entry.mappedLocation != 0xFFFFFFFF)
                    _sparingTable[entry.originalLocation] = entry.mappedLocation;

                entryOffset += entrySize;
            }

            return ErrorNumber.NoError;
        }

        // Sparing table not found, but this may be okay if no defects have been spared
        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Loads the Metadata Partition file for UDF 2.50+ metadata partitions.
    ///     The metadata file contains all metadata (file entries, directories) for the volume.
    /// </summary>
    ErrorNumber LoadMetadataPartition(IMediaImage                             imagePlugin,
                                      Dictionary<ushort, PartitionDescriptor> partitionDescriptors)
    {
        // Get the physical partition that the metadata partition references
        if(!_partitionMap.TryGetValue(_metadataPartitionNumber, out ushort physicalPartitionNum))
            return ErrorNumber.InvalidArgument;

        if(!partitionDescriptors.TryGetValue(physicalPartitionNum, out PartitionDescriptor physicalPartition))
            return ErrorNumber.InvalidArgument;

        // Read the metadata file entry from the physical partition
        ulong metadataFileSector = physicalPartition.partitionStartingLocation + _metadataFileLocation;

        if(ReadMrwAwareSector(imagePlugin, _mrw, metadataFileSector, out byte[] metadataFeBuffer) !=
           ErrorNumber.NoError)
        {
            // Try the mirror location if primary fails
            if(_metadataMirrorFileLocation != 0xFFFFFFFF)
            {
                metadataFileSector = physicalPartition.partitionStartingLocation + _metadataMirrorFileLocation;

                if(ReadMrwAwareSector(imagePlugin, _mrw, metadataFileSector, out metadataFeBuffer) !=
                   ErrorNumber.NoError)
                    return ErrorNumber.InvalidArgument;
            }
            else
                return ErrorNumber.InvalidArgument;
        }

        // Parse the metadata file entry
        ErrorNumber errno = ParseFileEntryInfo(metadataFeBuffer, out UdfFileEntryInfo metadataFileInfo);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the metadata file content
        var adType = (byte)((ushort)metadataFileInfo.IcbTag.flags & 0x07);

        // For metadata file, we need to read using the physical partition directly
        // Store the current partition start, temporarily use physical partition
        uint originalPartitionStart = _partitionStartingLocation;
        _partitionStartingLocation = physicalPartition.partitionStartingLocation;

        errno = ReadFileDataFromInfo(metadataFileInfo, metadataFeBuffer, adType, 0, out _metadataFileData);

        // Restore original partition start
        _partitionStartingLocation = originalPartitionStart;

        return errno;
    }

    /// <summary>
    ///     Translates a logical block number to an absolute sector number,
    ///     taking into account VAT (for virtual partitions), sparing tables
    ///     (for sparable partitions), and metadata partitions (UDF 2.50+).
    /// </summary>
    /// <param name="logicalBlock">The logical block number within the partition</param>
    /// <param name="partitionNumber">The partition reference number</param>
    /// <param name="partitionStart">The starting sector of the physical partition</param>
    /// <returns>The absolute sector number</returns>
    ulong TranslateLogicalBlock(uint logicalBlock, ushort partitionNumber, uint partitionStart)
    {
        uint physicalBlock = logicalBlock;

        // If this is a metadata partition, the logical block is an offset into the metadata file
        // The metadata file is already loaded into _metadataFileData, so we return a special value
        // that indicates the data should be read from the cached metadata file
        // Note: This is handled specially in the read methods

        // If this is a virtual partition, translate through the VAT
        if(_hasVirtualPartition && partitionNumber == _virtualPartitionNumber && _vat != null)
            if(logicalBlock < _vat.Length)
                physicalBlock = _vat[logicalBlock];

        // If this is a sparable partition, check the sparing table
        if(_hasSparablePartition && partitionNumber == _sparablePartitionNumber && _sparingTable != null)
        {
            // Sparing is done at packet boundaries
            uint packetNumber = physicalBlock / _sparablePacketLength;
            uint packetOffset = physicalBlock % _sparablePacketLength;

            uint packetStart = packetNumber * _sparablePacketLength;

            if(_sparingTable.TryGetValue(packetStart, out uint mappedPacket))
                physicalBlock = mappedPacket + packetOffset;
        }

        return partitionStart + physicalBlock;
    }

    /// <summary>
    ///     Reads a sector from the appropriate location, handling metadata partitions.
    ///     For metadata partitions, data is read from the cached metadata file.
    /// </summary>
    /// <param name="logicalBlock">The logical block number</param>
    /// <param name="partitionNumber">The partition reference number</param>
    /// <param name="partitionStart">The starting sector of the physical partition</param>
    /// <param name="buffer">The buffer to read into</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadSectorFromPartition(uint       logicalBlock, ushort partitionNumber, uint partitionStart,
                                        out byte[] buffer)
    {
        buffer = null;

        // Check if this is a metadata partition read
        if(_hasMetadataPartition && partitionNumber == _metadataPartitionNumber && _metadataFileData != null)
        {
            // Read from the cached metadata file
            long offset = logicalBlock * _sectorSize;

            if(offset + _sectorSize > _metadataFileData.Length) return ErrorNumber.InvalidArgument;

            buffer = new byte[_sectorSize];
            Array.Copy(_metadataFileData, offset, buffer, 0, _sectorSize);

            return ErrorNumber.NoError;
        }

        // For other partitions, translate and read from the image
        ulong absoluteSector = TranslateLogicalBlock(logicalBlock, partitionNumber, partitionStart);

        return ReadMrwAwareSector(_imagePlugin, _mrw, absoluteSector, out buffer);
    }

    /// <summary>
    ///     Reads multiple sectors from the appropriate location, handling metadata partitions.
    ///     For metadata partitions, data is read from the cached metadata file.
    /// </summary>
    /// <param name="logicalBlock">The starting logical block number</param>
    /// <param name="partitionNumber">The partition reference number</param>
    /// <param name="partitionStart">The starting sector of the physical partition</param>
    /// <param name="count">Number of sectors to read</param>
    /// <param name="buffer">The buffer to read into</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadSectorsFromPartition(uint logicalBlock, ushort partitionNumber, uint partitionStart, uint count,
                                         out byte[] buffer)
    {
        buffer = null;

        // Check if this is a metadata partition read
        if(_hasMetadataPartition && partitionNumber == _metadataPartitionNumber && _metadataFileData != null)
        {
            // Read from the cached metadata file
            long offset      = logicalBlock * _sectorSize;
            long bytesToRead = count        * _sectorSize;

            if(offset + bytesToRead > _metadataFileData.Length) return ErrorNumber.InvalidArgument;

            buffer = new byte[bytesToRead];
            Array.Copy(_metadataFileData, offset, buffer, 0, bytesToRead);

            return ErrorNumber.NoError;
        }

        // For other partitions, translate and read from the image
        ulong absoluteSector = TranslateLogicalBlock(logicalBlock, partitionNumber, partitionStart);

        return ReadMrwAwareSectors(absoluteSector, count, out buffer);
    }
}