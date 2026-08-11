// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Nintendo optical filesystems plugin.
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
using System.Security.Cryptography;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class NintendoPlugin
{
    /// <summary>Mount a Wii U disc filesystem using disc key for decryption</summary>
    ErrorNumber MountWiiU()
    {
        // Step 1: Get disc key from WiiUDiscKey media tag
        ErrorNumber errno = _imagePlugin.ReadMediaTag(MediaTagType.WiiUDiscKey, out _wiiuDiscKey);

        if(errno != ErrorNumber.NoError || _wiiuDiscKey is not { Length: 16 })
        {
            AaruLogging.Debug(MODULE_NAME, "No Wii U disc key available");

            return ErrorNumber.NoData;
        }

        AaruLogging.Debug(MODULE_NAME, "Wii U disc key: {0}", BitConverter.ToString(_wiiuDiscKey));

        // Step 2: Read and decrypt TOC at WIIU_ENCRYPTED_OFFSET (physical sector 3)
        uint  sectorSize     = _imagePlugin.Info.SectorSize;
        ulong tocStartSector = WIIU_ENCRYPTED_OFFSET     / sectorSize;
        uint  tocSectorCount = WIIU_PHYSICAL_SECTOR_SIZE / sectorSize;

        errno = _imagePlugin.ReadSectors(tocStartSector, false, tocSectorCount, out byte[] encTocSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Cannot read TOC sector: {0}", errno);

            return errno;
        }

        // Decrypt with disc key, IV = all zeros
        byte[] decTocSector;

        using(var aes = Aes.Create())
        {
            aes.Key     = _wiiuDiscKey;
            aes.IV      = new byte[16];
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            decTocSector = decryptor.TransformFinalBlock(encTocSector, 0, encTocSector.Length);
        }

        // Step 3: Validate TOC signature
        var tocSig = BigEndianBitConverter.ToUInt32(decTocSector, 0);

        if(tocSig != WIIU_TOC_SIGNATURE)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "TOC signature mismatch: 0x{0:X8} (expected 0x{1:X8})",
                              tocSig,
                              WIIU_TOC_SIGNATURE);

            return ErrorNumber.InvalidArgument;
        }

        // Step 4: Parse partition entries
        var tocPartCount = BigEndianBitConverter.ToUInt32(decTocSector, 0x1C);

        if(tocPartCount > WIIU_MAX_PARTITIONS) tocPartCount = WIIU_MAX_PARTITIONS;

        AaruLogging.Debug(MODULE_NAME, "Wii U TOC: {0} partition(s)", tocPartCount);

        var wiiuParts = new WiiuTocPartition[tocPartCount];

        for(uint i = 0; i < tocPartCount; i++)
        {
            uint entryOff = WIIU_TOC_ENTRIES_OFFSET + i * WIIU_TOC_ENTRY_SIZE;

            // Identifier is the first 25 bytes as a null-terminated string
            var identBytes = new byte[25];
            Array.Copy(decTocSector, (int)entryOff, identBytes, 0, 25);
            wiiuParts[i].Identifier  = StringHandlers.CToString(identBytes, Encoding.ASCII);
            wiiuParts[i].StartSector = BigEndianBitConverter.ToUInt32(decTocSector, (int)(entryOff + 0x20));
            wiiuParts[i].Key         = new byte[16];
            wiiuParts[i].HasTitleKey = false;

            AaruLogging.Debug(MODULE_NAME,
                              "  Partition {0}: \"{1}\", start_sector={2}",
                              i,
                              wiiuParts[i].Identifier,
                              wiiuParts[i].StartSector);
        }

        // Step 5: Extract title keys from SI/GI partitions
        WiiuExtractTitleKeys(wiiuParts);

        // Set disc key as fallback for partitions without title key
        for(var i = 0; i < wiiuParts.Length; i++)
            if(!wiiuParts[i].HasTitleKey)
                Array.Copy(_wiiuDiscKey, wiiuParts[i].Key, 16);

        // Step 6: Mount each partition by parsing its FST
        var partitionList = new List<PartitionInfo>();

        for(var i = 0; i < wiiuParts.Length; i++)
        {
            PartitionInfo partInfo = TryMountWiiuPartition(wiiuParts[i]);

            if(partInfo != null) partitionList.Add(partInfo);
        }

        if(partitionList.Count == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "No Wii U partitions could be mounted");

            return ErrorNumber.InvalidArgument;
        }

        _partitions     = partitionList.ToArray();
        _multiPartition = _partitions.Length > 1;

        AaruLogging.Debug(MODULE_NAME,
                          "Mounted {0} Wii U partition(s), multiPartition = {1}",
                          _partitions.Length,
                          _multiPartition);

        return ErrorNumber.NoError;
    }

    /// <summary>Read and decrypt data from a Wii U partition at a given offset within the partition</summary>
    /// <param name="key">AES-128 key for this partition</param>
    /// <param name="partitionDiscOffset">Absolute disc byte offset of the partition</param>
    /// <param name="fileOffset">Offset within the partition data</param>
    /// <param name="size">Number of bytes to read</param>
    /// <returns>Decrypted data, or null on error</returns>
    byte[] ReadWiiuVolumeDecrypted(byte[] key, ulong partitionDiscOffset, ulong fileOffset, uint size)
    {
        var  result     = new byte[size];
        uint done       = 0;
        uint sectorSize = _imagePlugin.Info.SectorSize;

        while(done < size)
        {
            ulong cur    = fileOffset + done;
            ulong secIdx = cur / WIIU_PHYSICAL_SECTOR_SIZE;
            var   secOff = (uint)(cur % WIIU_PHYSICAL_SECTOR_SIZE);

            ulong discOff       = partitionDiscOffset + secIdx * WIIU_PHYSICAL_SECTOR_SIZE;
            ulong sectorStart   = discOff                   / sectorSize;
            uint  sectorsToRead = WIIU_PHYSICAL_SECTOR_SIZE / sectorSize;

            ErrorNumber errno =
                _imagePlugin.ReadSectors(sectorStart, false, sectorsToRead, out byte[] encSector, out _);

            if(errno != ErrorNumber.NoError) return null;

            byte[] decSector;

            using(var aes = Aes.Create())
            {
                aes.Key     = key;
                aes.IV      = new byte[16];
                aes.Mode    = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                using ICryptoTransform decryptor = aes.CreateDecryptor();
                decSector = decryptor.TransformFinalBlock(encSector, 0, encSector.Length);
            }

            uint chunk = WIIU_PHYSICAL_SECTOR_SIZE - secOff;

            if(chunk > size - done) chunk = size - done;

            Array.Copy(decSector, secOff, result, done, chunk);
            done += chunk;
        }

        return result;
    }

    /// <summary>Extract title keys from TITLE.TIK files in SI/GI Wii U partitions</summary>
    void WiiuExtractTitleKeys(WiiuTocPartition[] parts)
    {
        for(var p = 0; p < parts.Length; p++)
        {
            // Only scan SI and GI partitions
            if(!parts[p].Identifier.StartsWith("SI", StringComparison.Ordinal) &&
               !parts[p].Identifier.StartsWith("GI", StringComparison.Ordinal))
                continue;

            ulong partDiscOff = WIIU_ENCRYPTED_OFFSET +
                                (ulong)parts[p].StartSector * WIIU_PHYSICAL_SECTOR_SIZE -
                                0x10000;

            // Read FST header (first physical sector of partition)
            byte[] fstHdr = ReadWiiuVolumeDecrypted(_wiiuDiscKey, partDiscOff, 0, WIIU_PHYSICAL_SECTOR_SIZE);

            if(fstHdr == null) continue;

            // Verify "FST\0" magic
            if(fstHdr[0] != 'F' || fstHdr[1] != 'S' || fstHdr[2] != 'T' || fstHdr[3] != 0) continue;

            var offsetFactor = BigEndianBitConverter.ToUInt32(fstHdr, 4);
            var clusterCount = BigEndianBitConverter.ToUInt32(fstHdr, 8);

            var clusterOffsets = new ulong[clusterCount];

            for(uint c = 0; c < clusterCount; c++)
            {
                var   raw   = BigEndianBitConverter.ToUInt32(fstHdr, (int)(0x20 + c * 0x20));
                ulong start = (ulong)raw * WIIU_PHYSICAL_SECTOR_SIZE;
                clusterOffsets[c] = start > WIIU_PHYSICAL_SECTOR_SIZE ? start - WIIU_PHYSICAL_SECTOR_SIZE : 0;
            }

            ulong entriesOffset = (ulong)offsetFactor * clusterCount + 0x20;

            // Read root entry to get total entries
            byte[] rootEntryData = ReadWiiuVolumeDecrypted(_wiiuDiscKey, partDiscOff, entriesOffset, 0x10);

            if(rootEntryData == null) continue;

            var totalEntries = BigEndianBitConverter.ToUInt32(rootEntryData, 8);

            if(totalEntries > 100000) continue;

            ulong nameTableOffset = entriesOffset + (ulong)totalEntries * 0x10;

            // Read entries + name table
            var fstDataSize = (uint)(nameTableOffset - entriesOffset + 0x10000);

            if(fstDataSize > 16 * 1024 * 1024) continue;

            byte[] fstData = ReadWiiuVolumeDecrypted(_wiiuDiscKey, partDiscOff, entriesOffset, fstDataSize);

            if(fstData == null) continue;

            // Scan for TITLE.TIK files
            for(uint e = 0; e < totalEntries; e++)
            {
                var   entOff    = (int)(e * 0x10);
                byte  type      = fstData[entOff];
                uint  nameOff   = BigEndianBitConverter.ToUInt32(fstData, entOff) & 0x00FFFFFF;
                ulong fileOff   = (ulong)BigEndianBitConverter.ToUInt32(fstData, entOff + 4) << 5;
                var   fileSize  = BigEndianBitConverter.ToUInt32(fstData, entOff + 8);
                var   clusterId = (ushort)(fstData[entOff + 0x0E] << 8 | fstData[entOff + 0x0F]);

                if(type == 1) continue; // directory

                if(fileSize < 0x200) continue;

                var nameTableBase = (uint)(nameTableOffset - entriesOffset);

                if(nameOff >= fstDataSize - nameTableBase) continue;

                string fname = StringHandlers.CToString(fstData, Encoding.ASCII, false, (int)(nameTableBase + nameOff));

                if(!fname.Equals("title.tik", StringComparison.OrdinalIgnoreCase)) continue;

                if(clusterId >= clusterCount) continue;

                // Read ticket: encrypted title key at TIK+0x1BF, title ID at TIK+0x1DC
                ulong tikVolumeOff = clusterOffsets[clusterId] + fileOff;

                byte[] tikBuf =
                    ReadWiiuVolumeDecrypted(_wiiuDiscKey, partDiscOff, tikVolumeOff + 0x1BF, 0x10 + 0x1D + 8);

                if(tikBuf == null) continue;

                var encTitleKey = new byte[16];
                var titleId     = new byte[8];
                Array.Copy(tikBuf, 0,    encTitleKey, 0, 16);
                Array.Copy(tikBuf, 0x1D, titleId,     0, 8);

                // Decrypt title key: AES-128-CBC with common key, IV = title_id + 8 zero bytes
                var tikIv = new byte[16];
                Array.Copy(titleId, 0, tikIv, 0, 8);

                byte[] decTitleKey;

                using(var aes = Aes.Create())
                {
                    aes.Key     = WIIU_COMMON_KEY;
                    aes.IV      = tikIv;
                    aes.Mode    = CipherMode.CBC;
                    aes.Padding = PaddingMode.None;

                    using ICryptoTransform decryptor = aes.CreateDecryptor();
                    decTitleKey = decryptor.TransformFinalBlock(encTitleKey, 0, 16);
                }

                // Build expected GM partition name from title ID
                string gmName = $"GM{titleId[0]:X2}{titleId[1]:X2}{titleId[2]:X2}{titleId[3]:X2}" +
                                $"{titleId[4]:X2}{titleId[5]:X2}{titleId[6]:X2}{titleId[7]:X2}";

                // Match to a GM partition
                for(var g = 0; g < parts.Length; g++)
                {
                    if(!parts[g].Identifier.StartsWith("GM", StringComparison.Ordinal)) continue;

                    if(!parts[g].Identifier.StartsWith(gmName, StringComparison.Ordinal)) continue;

                    Array.Copy(decTitleKey, parts[g].Key, 16);
                    parts[g].HasTitleKey = true;

                    AaruLogging.Debug(MODULE_NAME, "Title key for {0} extracted", gmName);

                    break;
                }
            }
        }
    }

    /// <summary>Try to mount a Wii U partition by reading and parsing its FST</summary>
    PartitionInfo TryMountWiiuPartition(WiiuTocPartition tocPart)
    {
        ulong partDiscOff = WIIU_ENCRYPTED_OFFSET + (ulong)tocPart.StartSector * WIIU_PHYSICAL_SECTOR_SIZE - 0x10000;

        // Read FST header
        byte[] fstHdr = ReadWiiuVolumeDecrypted(tocPart.Key, partDiscOff, 0, WIIU_PHYSICAL_SECTOR_SIZE);

        if(fstHdr == null)
        {
            AaruLogging.Debug(MODULE_NAME, "Cannot read FST header for partition {0}", tocPart.Identifier);

            return null;
        }

        // Verify "FST\0" magic
        if(fstHdr[0] != 'F' || fstHdr[1] != 'S' || fstHdr[2] != 'T' || fstHdr[3] != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "No FST magic for partition {0}", tocPart.Identifier);

            return null;
        }

        var offsetFactor = BigEndianBitConverter.ToUInt32(fstHdr, 4);
        var clusterCount = BigEndianBitConverter.ToUInt32(fstHdr, 8);

        ulong entriesOffset = (ulong)offsetFactor * clusterCount + 0x20;

        // Read root entry
        byte[] rootEntryData = ReadWiiuVolumeDecrypted(tocPart.Key, partDiscOff, entriesOffset, 0x10);

        if(rootEntryData == null) return null;

        var totalEntries = BigEndianBitConverter.ToUInt32(rootEntryData, 8);

        if(totalEntries == 0 || totalEntries > 100000) return null;

        ulong nameTableOffset = entriesOffset + (ulong)totalEntries * 0x10;

        // Read all entries + name table
        var fstDataSize = (uint)(nameTableOffset - entriesOffset + 0x10000);

        if(fstDataSize > 16 * 1024 * 1024) return null;

        byte[] fstData = ReadWiiuVolumeDecrypted(tocPart.Key, partDiscOff, entriesOffset, fstDataSize);

        if(fstData == null) return null;

        // Parse into PartitionInfo using WiiuFstEntry (16 bytes each)
        var partInfo = new PartitionInfo
        {
            Name            = tocPart.Identifier,
            IsWiiU          = true,
            WiiuKey         = tocPart.Key,
            PartitionOffset = partDiscOff,
            FstEntries      = new FstEntry[totalEntries],
            FstNames        = new string[totalEntries],
            WiiuFstEntries  = new WiiuFstEntry[totalEntries]
        };

        // Build cluster offsets table
        partInfo.WiiuClusterOffsets = new ulong[clusterCount];

        for(uint c = 0; c < clusterCount; c++)
        {
            var   raw   = BigEndianBitConverter.ToUInt32(fstHdr, (int)(0x20 + c * 0x20));
            ulong start = (ulong)raw * WIIU_PHYSICAL_SECTOR_SIZE;
            partInfo.WiiuClusterOffsets[c] = start > WIIU_PHYSICAL_SECTOR_SIZE ? start - WIIU_PHYSICAL_SECTOR_SIZE : 0;
        }

        var nameTableBase = (uint)(nameTableOffset - entriesOffset);

        for(uint i = 0; i < totalEntries; i++)
        {
            var off = (int)(i * 0x10);

            // Convert Wii U 16-byte FST entry to the standard 12-byte FstEntry structure
            var typeAndName    = BigEndianBitConverter.ToUInt32(fstData, off);
            var offsetOrParent = BigEndianBitConverter.ToUInt32(fstData, off + 4);
            var sizeOrNext     = BigEndianBitConverter.ToUInt32(fstData, off + 8);

            partInfo.FstEntries[i] = new FstEntry
            {
                TypeAndNameOffset = typeAndName,
                OffsetOrParent    = offsetOrParent,
                SizeOrNext        = sizeOrNext
            };

            partInfo.WiiuFstEntries[i] = new WiiuFstEntry
            {
                TypeAndNameOffset = typeAndName,
                OffsetOrParent    = offsetOrParent,
                SizeOrNext        = sizeOrNext,
                Flags             = (ushort)(fstData[off + 0x0C] << 8 | fstData[off + 0x0D]),
                ClusterIndex      = (ushort)(fstData[off + 0x0E] << 8 | fstData[off + 0x0F])
            };

            uint nameOffset = typeAndName & 0x00FFFFFF;

            partInfo.FstNames[i] = nameOffset < fstDataSize - nameTableBase
                                       ? StringHandlers.CToString(fstData,
                                                                  _encoding,
                                                                  false,
                                                                  (int)(nameTableBase + nameOffset))
                                       : "";
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Successfully mounted Wii U partition \"{0}\" with {1} FST entries",
                          tocPart.Identifier,
                          totalEntries);

        return partInfo;
    }

    /// <summary>Mount a GameCube disc filesystem (no encryption, single partition)</summary>
    ErrorNumber MountGameCube()
    {
        var partition = new PartitionInfo
        {
            DiscHeader = _discHeader
        };

        uint fstOff  = _discHeader.FstOff;
        uint fstSize = _discHeader.FstSize;

        AaruLogging.Debug(MODULE_NAME, "GameCube FST offset: 0x{0:X8}, size: 0x{1:X8}", fstOff, fstSize);

        if(fstOff == 0 || fstSize == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "GameCube FST offset or size is zero, returning InvalidArgument");

            return ErrorNumber.InvalidArgument;
        }

        // FST may not be sector-aligned, account for sub-sector offset
        uint  sectorSize     = _imagePlugin.Info.SectorSize;
        ulong fstStartSector = fstOff                               / sectorSize;
        uint  fstBase        = fstOff                               % sectorSize;
        ulong fstSectorCount = (fstBase + fstSize + sectorSize - 1) / sectorSize;

        AaruLogging.Debug(MODULE_NAME,
                          "GameCube FST: reading {0} sectors starting at sector {1}, sub-sector offset {2}",
                          fstSectorCount,
                          fstStartSector,
                          fstBase);

        ErrorNumber errno =
            _imagePlugin.ReadSectors(fstStartSector, false, (uint)fstSectorCount, out byte[] sectorData, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadSectors for FST returned {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Read {0} bytes of sector data", sectorData.Length);

        // Extract exact FST data from the sector-aligned buffer
        var fstData = new byte[fstSize];
        Array.Copy(sectorData, fstBase, fstData, 0, fstSize);

        ErrorNumber fstResult = ParseFst(partition, fstData, false);

        if(fstResult != ErrorNumber.NoError) return fstResult;

        // Calculate DOL size from its header
        partition.DolOffset = _discHeader.DolOff;

        ulong dolStartSector   = partition.DolOffset                          / sectorSize;
        uint  dolBase          = partition.DolOffset                          % sectorSize;
        uint  dolSectorsNeeded = (dolBase + DOL_HEADER_SIZE + sectorSize - 1) / sectorSize;

        errno = _imagePlugin.ReadSectors(dolStartSector, false, dolSectorsNeeded, out byte[] dolSectorData, out _);

        if(errno != ErrorNumber.NoError) return errno;

        var dolHeader = new byte[DOL_HEADER_SIZE];
        Array.Copy(dolSectorData, dolBase, dolHeader, 0, DOL_HEADER_SIZE);

        partition.DolSize = CalculateDolSize(dolHeader);

        AaruLogging.Debug(MODULE_NAME,
                          "GameCube DOL at 0x{0:X8}, size {1} bytes",
                          partition.DolOffset,
                          partition.DolSize);

        _partitions     = [partition];
        _multiPartition = false;

        return ErrorNumber.NoError;
    }

    /// <summary>Mount a Wii disc filesystem (encrypted partitions). Enumerates all usable partitions.</summary>
    ErrorNumber MountWii(byte[] discData)
    {
        var partitions   = new List<PartitionInfo>();
        var nameCounters = new Dictionary<string, int>();

        // Scan all 4 partition tables
        for(var tbl = 0; tbl < 4; tbl++)
        {
            var  count  = BigEndianBitConverter.ToUInt32(discData, 0x40000 + tbl * 8);
            uint offset = BigEndianBitConverter.ToUInt32(discData, 0x40004 + tbl * 8) << 2;

            for(var i = 0; i < count; i++)
            {
                if(offset + i * 8 + 8 >= 0x50000) continue;

                WiiPartitionTableEntry entry =
                    Marshal.ByteArrayToStructureBigEndian<WiiPartitionTableEntry>(discData, (int)(offset + i * 8), 8);

                ulong partOffset = (ulong)entry.Offset << 2;

                AaruLogging.Debug(MODULE_NAME,
                                  "Found Wii partition: table {0}, type {1}, offset 0x{2:X8}",
                                  tbl,
                                  entry.Type,
                                  partOffset);

                PartitionInfo partInfo = TryMountWiiPartition(entry.Type, partOffset);

                if(partInfo == null) continue;

                // Assign name based on partition type
                string baseName = entry.Type switch
                                  {
                                      0 => "DATA",
                                      1 => "UPDATE",
                                      2 => "CHANNEL",
                                      _ => $"PARTITION_{entry.Type}"
                                  };

                if(!nameCounters.TryGetValue(baseName, out int nameCount))
                {
                    partInfo.Name          = baseName;
                    nameCounters[baseName] = 1;
                }
                else
                {
                    nameCounters[baseName] = nameCount + 1;
                    partInfo.Name          = $"{baseName}_{nameCount + 1}";
                }

                partitions.Add(partInfo);
            }
        }

        if(partitions.Count == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "No usable partitions found on Wii disc");

            return ErrorNumber.InvalidArgument;
        }

        _partitions     = partitions.ToArray();
        _multiPartition = _partitions.Length > 1;

        AaruLogging.Debug(MODULE_NAME,
                          "Mounted {0} Wii partition(s), multiPartition = {1}",
                          _partitions.Length,
                          _multiPartition);

        return ErrorNumber.NoError;
    }

    /// <summary>Try to mount a single Wii partition, returning its info on success or null on failure</summary>
    /// <param name="type">Partition type (0 = DATA, 1 = UPDATE, 2 = CHANNEL)</param>
    /// <param name="partitionOffset">Absolute offset of the partition on disc</param>
    /// <returns>Populated PartitionInfo on success, null if the partition cannot be decrypted or parsed</returns>
    PartitionInfo TryMountWiiPartition(uint type, ulong partitionOffset)
    {
        var partInfo = new PartitionInfo
        {
            Type            = type,
            PartitionOffset = partitionOffset
        };

        // Read the ticket from the partition header to get the title key
        // Title key (encrypted) is at partition offset + 0x1BF, 16 bytes
        // IV is at partition offset + 0x1DC, 8 bytes (padded to 16 with zeros)
        ulong ticketSector = (partitionOffset + 0x1BF) / _imagePlugin.Info.SectorSize;

        var ticketSectorCount =
            (uint)((partitionOffset +
                    0x1DC           +
                    16 -
                    ticketSector * _imagePlugin.Info.SectorSize +
                    _imagePlugin.Info.SectorSize -
                    1) /
                   _imagePlugin.Info.SectorSize);

        ErrorNumber errno =
            _imagePlugin.ReadSectors(ticketSector, false, ticketSectorCount, out byte[] ticketData, out _);

        if(errno != ErrorNumber.NoError) return null;

        var ticketBase = (uint)(partitionOffset + 0x1BF - ticketSector * _imagePlugin.Info.SectorSize);

        var encryptedTitleKey = new byte[16];
        Array.Copy(ticketData, ticketBase, encryptedTitleKey, 0, 16);

        var iv = new byte[16];
        Array.Copy(ticketData, ticketBase + (0x1DC - 0x1BF), iv, 0, 8);

        // Remaining 8 bytes of IV are zero

        // Determine which common key to use based on key index at partition offset + 0x1F1
        ulong keyIndexSector = (partitionOffset + 0x1F1) / _imagePlugin.Info.SectorSize;

        errno = _imagePlugin.ReadSectors(keyIndexSector, false, 1, out byte[] keyIndexData, out _);

        if(errno != ErrorNumber.NoError) return null;

        byte keyIndex = keyIndexData[(partitionOffset + 0x1F1) % _imagePlugin.Info.SectorSize];

        byte[] commonKey = keyIndex == 1 ? WII_KOREAN_KEY : WII_COMMON_KEY;

        // Decrypt the title key using the common key
        byte[] partitionKey;

        using(var aes = Aes.Create())
        {
            aes.Key     = commonKey;
            aes.IV      = iv;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            partitionKey = decryptor.TransformFinalBlock(encryptedTitleKey, 0, 16);
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Partition at 0x{0:X8}: decrypted key {1}",
                          partitionOffset,
                          BitConverter.ToString(partitionKey));

        // Set up AES for partition data decryption
        partInfo.PartitionAes         = Aes.Create();
        partInfo.PartitionAes.Key     = partitionKey;
        partInfo.PartitionAes.Mode    = CipherMode.CBC;
        partInfo.PartitionAes.Padding = PaddingMode.None;

        // Read data offset and data size from the partition header at partition offset + 0x2B8
        ulong dataOffSector = (partitionOffset + 0x2B8) / _imagePlugin.Info.SectorSize;

        errno = _imagePlugin.ReadSectors(dataOffSector, false, 1, out byte[] dataOffData, out _);

        if(errno != ErrorNumber.NoError)
        {
            partInfo.PartitionAes.Dispose();

            return null;
        }

        var dataOffBase = (uint)((partitionOffset + 0x2B8) % _imagePlugin.Info.SectorSize);

        partInfo.PartitionDataOffset = (ulong)BigEndianBitConverter.ToUInt32(dataOffData, (int)dataOffBase) << 2;

        AaruLogging.Debug(MODULE_NAME,
                          "Partition at 0x{0:X8}: data offset 0x{1:X8}",
                          partitionOffset,
                          partInfo.PartitionDataOffset);

        // Read the partition's internal disc header to get FST offset and size
        byte[] partHeader = ReadWiiPartitionData(partInfo, 0, 0x440);

        if(partHeader == null)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to read partition header at 0x{0:X8}", partitionOffset);
            partInfo.PartitionAes.Dispose();

            return null;
        }

        partInfo.DiscHeader = Marshal.ByteArrayToStructureBigEndian<DiscHeader>(partHeader);

        // Wii partition internal FST offset and size need to be shifted left by 2
        uint fstOff  = partInfo.DiscHeader.FstOff  << 2;
        uint fstSize = partInfo.DiscHeader.FstSize << 2;

        AaruLogging.Debug(MODULE_NAME,
                          "Partition at 0x{0:X8}: FST offset 0x{1:X8}, size 0x{2:X8}",
                          partitionOffset,
                          fstOff,
                          fstSize);

        if(fstOff == 0 || fstSize == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Partition at 0x{0:X8}: FST offset or size is zero", partitionOffset);

            partInfo.PartitionAes.Dispose();

            return null;
        }

        // Read FST from the encrypted partition data
        byte[] fstData = ReadWiiPartitionData(partInfo, fstOff, fstSize);

        if(fstData == null)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to read FST for partition at 0x{0:X8}", partitionOffset);
            partInfo.PartitionAes.Dispose();

            return null;
        }

        ErrorNumber fstResult = ParseFst(partInfo, fstData, true);

        if(fstResult != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Failed to parse FST for partition at 0x{0:X8}: {1}",
                              partitionOffset,
                              fstResult);

            partInfo.PartitionAes.Dispose();

            return null;
        }

        // Calculate DOL size from its header within the encrypted partition data
        partInfo.DolOffset = partInfo.DiscHeader.DolOff << 2;

        byte[] dolHeader = ReadWiiPartitionData(partInfo, partInfo.DolOffset, DOL_HEADER_SIZE);

        if(dolHeader != null)
            partInfo.DolSize = CalculateDolSize(dolHeader);
        else
            AaruLogging.Debug(MODULE_NAME, "Failed to read DOL header for partition at 0x{0:X8}", partitionOffset);

        AaruLogging.Debug(MODULE_NAME,
                          "Successfully mounted partition at 0x{0:X8}: DOL at 0x{1:X8}, size {2}",
                          partitionOffset,
                          partInfo.DolOffset,
                          partInfo.DolSize);

        return partInfo;
    }

    /// <summary>Read data from a Wii encrypted partition, decrypting the cluster blocks</summary>
    /// <param name="partition">Partition info with AES key and offsets</param>
    /// <param name="offset">Offset within the partition data (in decrypted space)</param>
    /// <param name="size">Number of bytes to read</param>
    /// <returns>Decrypted data, or null on error</returns>
    byte[] ReadWiiPartitionData(PartitionInfo partition, uint offset, uint size)
    {
        var  result     = new byte[size];
        uint bytesRead  = 0;
        uint currentOff = offset;

        while(bytesRead < size)
        {
            uint blockIndex  = currentOff / WII_CLUSTER_DATA_SIZE;
            uint blockOffset = currentOff % WII_CLUSTER_DATA_SIZE;
            uint bytesToRead = Math.Min(size - bytesRead, WII_CLUSTER_DATA_SIZE - blockOffset);

            // Physical offset on disc = partition offset + data offset + block * 0x8000
            ulong physicalOffset = partition.PartitionOffset     +
                                   partition.PartitionDataOffset +
                                   (ulong)blockIndex * WII_CLUSTER_SIZE;

            ulong sectorStart = physicalOffset / _imagePlugin.Info.SectorSize;

            uint sectorsNeeded = (WII_CLUSTER_SIZE + _imagePlugin.Info.SectorSize - 1) / _imagePlugin.Info.SectorSize;

            ErrorNumber errno =
                _imagePlugin.ReadSectors(sectorStart, false, sectorsNeeded, out byte[] clusterData, out _);

            if(errno != ErrorNumber.NoError) return null;

            // The cluster may not be aligned to sector boundaries
            var clusterBase = (uint)(physicalOffset - sectorStart * _imagePlugin.Info.SectorSize);

            if(clusterBase + WII_CLUSTER_SIZE > clusterData.Length) return null;

            // IV for data decryption is at offset 0x3D0 within the cluster (in the hash area)
            var dataIv = new byte[16];
            Array.Copy(clusterData, clusterBase + 0x3D0, dataIv, 0, 16);

            // Encrypted data starts at offset 0x400 within the cluster
            var encryptedData = new byte[WII_CLUSTER_DATA_SIZE];
            Array.Copy(clusterData, clusterBase + WII_CLUSTER_HASH_SIZE, encryptedData, 0, WII_CLUSTER_DATA_SIZE);

            partition.PartitionAes.IV = dataIv;

            byte[] decryptedData;

            using(ICryptoTransform decryptor = partition.PartitionAes.CreateDecryptor())
            {
                decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, WII_CLUSTER_DATA_SIZE);
            }

            Array.Copy(decryptedData, blockOffset, result, bytesRead, bytesToRead);

            bytesRead  += bytesToRead;
            currentOff += bytesToRead;
        }

        return result;
    }

    /// <summary>Calculate the total size of a DOL executable from its header</summary>
    /// <param name="dolHeader">The first 0x100 bytes of the DOL file</param>
    /// <returns>Total size of the DOL file on disc</returns>
    static uint CalculateDolSize(byte[] dolHeader)
    {
        uint max = 0;

        // 7 code (text) segments: offsets at 0x00, sizes at 0x90
        for(var i = 0; i < DOL_CODE_SEGMENTS; i++)
        {
            var offset = BigEndianBitConverter.ToUInt32(dolHeader, i * 4);
            var size   = BigEndianBitConverter.ToUInt32(dolHeader, 0x90 + i * 4);

            if(offset + size > max) max = offset + size;
        }

        // 11 data segments: offsets at 0x1C, sizes at 0xAC
        for(var i = 0; i < DOL_DATA_SEGMENTS; i++)
        {
            var offset = BigEndianBitConverter.ToUInt32(dolHeader, 0x1C + i * 4);
            var size   = BigEndianBitConverter.ToUInt32(dolHeader, 0xAC + i * 4);

            if(offset + size > max) max = offset + size;
        }

        return max;
    }

    /// <summary>Parse the FST (File System Table) from raw FST data into a partition's entries</summary>
    /// <param name="partition">Partition to store the parsed entries in</param>
    /// <param name="fstData">Raw FST bytes</param>
    /// <param name="isWii">
    ///     True if Wii (file offsets need &lt;&lt;2), false for GameCube
    /// </param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ParseFst(PartitionInfo partition, byte[] fstData, bool isWii)
    {
        AaruLogging.Debug(MODULE_NAME, "ParseFst: fstData.Length = {0}, isWii = {1}", fstData.Length, isWii);

        if(fstData.Length < 12)
        {
            AaruLogging.Debug(MODULE_NAME, "ParseFst: FST data too small ({0} < 12)", fstData.Length);

            return ErrorNumber.InvalidArgument;
        }

        // Root entry is at index 0, always a directory
        FstEntry rootEntry = Marshal.ByteArrayToStructureBigEndian<FstEntry>(fstData, 0, 12);

        AaruLogging.Debug(MODULE_NAME,
                          "ParseFst: root TypeAndNameOffset=0x{0:X8}, OffsetOrParent=0x{1:X8}, SizeOrNext=0x{2:X8}",
                          rootEntry.TypeAndNameOffset,
                          rootEntry.OffsetOrParent,
                          rootEntry.SizeOrNext);

        uint totalEntries = rootEntry.SizeOrNext;

        AaruLogging.Debug(MODULE_NAME, "FST has {0} entries", totalEntries);

        if(totalEntries == 0 || (ulong)totalEntries * 12 > (ulong)fstData.Length)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ParseFst: invalid totalEntries ({0}), fstData.Length = {1}",
                              totalEntries,
                              fstData.Length);

            return ErrorNumber.InvalidArgument;
        }

        // String table starts right after the FST entries
        uint stringTableOffset = totalEntries * 12;

        AaruLogging.Debug(MODULE_NAME, "ParseFst: stringTableOffset = 0x{0:X}", stringTableOffset);

        // Parse all entries
        partition.FstEntries = new FstEntry[totalEntries];
        partition.FstNames   = new string[totalEntries];

        partition.FstEntries[0] = rootEntry;
        partition.FstNames[0]   = "";

        for(uint i = 1; i < totalEntries; i++)
        {
            partition.FstEntries[i] = Marshal.ByteArrayToStructureBigEndian<FstEntry>(fstData, (int)(i * 12), 12);

            uint nameOffset = partition.FstEntries[i].TypeAndNameOffset & 0x00FFFFFF;

            if(stringTableOffset + nameOffset >= (uint)fstData.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "ParseFst: entry[{0}] name offset out of bounds", i);

                return ErrorNumber.InvalidArgument;
            }

            partition.FstNames[i] =
                StringHandlers.CToString(fstData, _encoding, false, (int)(stringTableOffset + nameOffset));

            AaruLogging.Debug(MODULE_NAME,
                              "ParseFst: entry[{0}] type={1} name=\"{2}\" offsetOrParent=0x{3:X8} sizeOrNext=0x{4:X8}",
                              i,
                              partition.FstEntries[i].TypeAndNameOffset >> 24 != 0 ? "dir" : "file",
                              partition.FstNames[i],
                              partition.FstEntries[i].OffsetOrParent,
                              partition.FstEntries[i].SizeOrNext);
        }

        AaruLogging.Debug(MODULE_NAME, "ParseFst: successfully parsed {0} entries", totalEntries);

        return ErrorNumber.NoError;
    }

    /// <summary>Cache the root directory contents for a partition (entries whose parent is 0)</summary>
    static void CachePartitionRootDirectory(PartitionInfo partition)
    {
        partition.RootDirectoryCache.Clear();

        // Add virtual system files
        partition.RootDirectoryCache["boot.bin"] = BOOT_BIN_VIRTUAL_INDEX;
        partition.RootDirectoryCache["bi2.bin"]  = BI2_BIN_VIRTUAL_INDEX;

        if(partition.DolSize > 0) partition.RootDirectoryCache["main.dol"] = DOL_VIRTUAL_INDEX;

        // Root is entry 0. Its children are entries 1..(SizeOrNext - 1) that have parent == 0
        for(var i = 1; i < partition.FstEntries.Length; i++)
        {
            bool isDirectory = partition.FstEntries[i].TypeAndNameOffset >> 24 != 0;

            if(isDirectory)
            {
                // For directories, OffsetOrParent is the parent index
                if(partition.FstEntries[i].OffsetOrParent == 0) partition.RootDirectoryCache[partition.FstNames[i]] = i;
            }
            else
            {
                // For files, check if they're a direct child of root (not inside a subdirectory)
                if(IsDirectChild(partition, i, 0)) partition.RootDirectoryCache[partition.FstNames[i]] = i;
            }
        }
    }

    /// <summary>Check if FST entry at index is a direct child of the directory at parentIndex</summary>
    static bool IsDirectChild(PartitionInfo partition, int entryIndex, int parentIndex)
    {
        bool isDirectory = partition.FstEntries[entryIndex].TypeAndNameOffset >> 24 != 0;

        if(isDirectory) return (int)partition.FstEntries[entryIndex].OffsetOrParent == parentIndex;

        // For a file, find the innermost enclosing directory
        var currentParent = 0;

        for(var i = 0; i < partition.FstEntries.Length; i++)
        {
            if(i == entryIndex) return currentParent == parentIndex;

            bool isDir = partition.FstEntries[i].TypeAndNameOffset >> 24 != 0;

            if(!isDir) continue;

            if(i == 0)
            {
                currentParent = 0;

                continue;
            }

            // If we're entering this directory's range, it becomes the parent
            if(entryIndex > i && entryIndex < (int)partition.FstEntries[i].SizeOrNext) currentParent = i;
        }

        return currentParent == parentIndex;
    }

#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _encoding    = encoding ?? Encoding.GetEncoding("shift_jis");
        _imagePlugin = imagePlugin;

        AaruLogging.Debug(MODULE_NAME,
                          "Mount called. partition.Start = {0}, partition.Length = {1}",
                          partition.Start,
                          partition.Length);

        AaruLogging.Debug(MODULE_NAME,
                          "Image sectors = {0}, sector size = {1}",
                          imagePlugin.Info.Sectors,
                          imagePlugin.Info.SectorSize);

        if(partition.Start != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Partition does not start at 0, returning InvalidArgument");

            return ErrorNumber.InvalidArgument;
        }

        ulong totalBytes = imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize;

        AaruLogging.Debug(MODULE_NAME, "Total image bytes = {0} (0x{0:X})", totalBytes);

        if(totalBytes < 0x50000)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Image too small ({0} bytes < 0x50000), returning InvalidArgument",
                              totalBytes);

            return ErrorNumber.InvalidArgument;
        }

        // Read the first 0x50000 bytes (enough for disc header + partition table + region settings)
        uint sectorsToRead = 0x50000 / imagePlugin.Info.SectorSize;

        AaruLogging.Debug(MODULE_NAME, "Reading {0} sectors from sector 0", sectorsToRead);

        ErrorNumber errno = imagePlugin.ReadSectors(0, false, sectorsToRead, out byte[] header, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadSectors returned {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Read {0} bytes of header data", header.Length);

        _discHeader = Marshal.ByteArrayToStructureBigEndian<DiscHeader>(header);

        AaruLogging.Debug(MODULE_NAME,
                          "DiscHeader.WiiMagic = 0x{0:X8}, expected 0x{1:X8}",
                          _discHeader.WiiMagic,
                          WII_MAGIC);

        AaruLogging.Debug(MODULE_NAME,
                          "DiscHeader.GcMagic = 0x{0:X8}, expected 0x{1:X8}",
                          _discHeader.GcMagic,
                          GC_MAGIC);

        AaruLogging.Debug(MODULE_NAME, "DiscHeader.DiscType = 0x{0:X2}", _discHeader.DiscType);

        AaruLogging.Debug(MODULE_NAME,
                          "DiscHeader.FstOff = 0x{0:X8}, FstSize = 0x{1:X8}",
                          _discHeader.FstOff,
                          _discHeader.FstSize);

        if(_discHeader.WiiMagic == WII_MAGIC)
            _isWii = true;
        else if(_discHeader.GcMagic == GC_MAGIC)
            _isWii = false;
        else if(imagePlugin.Info.MediaType == MediaType.WUOD)
            _isWiiU = true;
        else
        {
            AaruLogging.Debug(MODULE_NAME, "Neither Wii nor GameCube magic found, returning InvalidArgument");

            // Dump first 32 bytes for debugging
            AaruLogging.Debug(MODULE_NAME,
                              "First 32 bytes of header: {0}",
                              BitConverter.ToString(header, 0, Math.Min(32, header.Length)));

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "Disc type: {0}", _isWii ? "Wii" : "GameCube");

        string discType = Encoding.ASCII.GetString(new[]
        {
            _discHeader.DiscType
        });

        string gameCode = Encoding.ASCII.GetString(_discHeader.GameCode);

        string regionCode = Encoding.ASCII.GetString(new[]
        {
            _discHeader.RegionCode
        });

        string publisherCode = Encoding.ASCII.GetString(_discHeader.PublisherCode);
        string discId        = discType + gameCode + regionCode + publisherCode;
        string title         = StringHandlers.CToString(_discHeader.Title, _encoding);

        AaruLogging.Debug(MODULE_NAME, "Disc ID = {0}, Title = \"{1}\"", discId, title);

        if(_isWiiU)
            errno = MountWiiU();
        else if(_isWii)
            errno = MountWii(header);
        else
            errno = MountGameCube();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Mount{0} returned {1}", _isWii ? "Wii" : "GameCube", errno);

            return errno;
        }

        // Cache root directory entries for each partition
        foreach(PartitionInfo partInfo in _partitions) CachePartitionRootDirectory(partInfo);

        Metadata = new FileSystem
        {
            Type = _isWiiU
                       ? FS_TYPE_WIIU
                       : _isWii
                           ? FS_TYPE_WII
                           : FS_TYPE_NGC,
            VolumeName   = title,
            ClusterSize  = 2048,
            Clusters     = imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize / 2048,
            Bootable     = true,
            VolumeSerial = discId
        };

        _statfs = new FileSystemInfo
        {
            Blocks         = imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize / 2048,
            FilenameLength = 256,
            FreeBlocks     = 0,
            PluginId       = Id,
            Type = _isWiiU
                       ? FS_TYPE_WIIU
                       : _isWii
                           ? FS_TYPE_WII
                           : FS_TYPE_NGC
        };

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _mounted = false;

        if(_partitions != null)
        {
            foreach(PartitionInfo partInfo in _partitions)
            {
                partInfo.PartitionAes?.Dispose();
                partInfo.RootDirectoryCache.Clear();
                partInfo.DirectoryCache.Clear();
            }

            _partitions = null;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        stat = _statfs.ShallowCopy();

        return ErrorNumber.NoError;
    }

#endregion
}