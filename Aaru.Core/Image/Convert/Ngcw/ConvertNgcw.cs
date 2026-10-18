// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ConvertNgcw.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Image conversion.
//
// --[ Description ] ----------------------------------------------------------
//
//     GameCube / Wii conversion pipeline: inject media tags, convert sectors
//     with junk detection and Wii decryption, enrich metadata.
//     Port of tool/ngcw/convert.c.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program. If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2019-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Decoders.Nintendo;
using Aaru.Decryption.Ngcw;
using NgcwPartitions = Aaru.Decryption.Ngcw.Partitions;
using Aaru.Helpers;
using Aaru.Localization;

namespace Aaru.Core.Image;

public partial class Convert
{
    DataRegion[][]       _ngcwPartDataMaps;
    List<WiiPartition>   _ngcwPartitions;
    ulong[]              _ngcwPartSysEnd;
    WiiPartitionRegion[] _ngcwRegions;

    /// <summary>
    ///     For WOD: parses the Wii partition table, decrypts title keys, builds the
    ///     partition key map, and writes it as a media tag.
    ///     For GOD: no-op (GameCube has no encryption).
    ///     Must be called before ConvertMediaTags.
    /// </summary>
    ErrorNumber InjectNgcwMediaTags()
    {
        if(_aborted) return ErrorNumber.NoError;

        // GOD has no partition keys to inject
        if(_mediaType == MediaType.GOD) return ErrorNumber.NoError;

        InitProgress?.Invoke();

        // Parse Wii partition table
        PulseProgress?.Invoke(UI.Ngcw_parsing_partition_table);

        _ngcwPartitions = NgcwPartitions.ParseWiiPartitions(_inputImage);

        if(_ngcwPartitions == null)
        {
            EndProgress?.Invoke();
            StoppingErrorMessage?.Invoke(UI.Ngcw_cannot_parse_partitions);

            return ErrorNumber.InvalidArgument;
        }

        UpdateStatus?.Invoke(string.Format(UI.Ngcw_found_0_partitions, _ngcwPartitions.Count));

        // Build partition region map
        PulseProgress?.Invoke(UI.Ngcw_building_partition_key_map);

        _ngcwRegions = NgcwPartitions.BuildRegionMap(_ngcwPartitions);

        // Serialize and write partition key map
        byte[] keyMapData = NgcwPartitions.SerializeKeyMap(_ngcwRegions);

        _outputImage.WriteMediaTag(keyMapData, MediaTagType.WiiPartitionKeyMap);

        UpdateStatus?.Invoke(UI.Ngcw_written_partition_key_map);

        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Converts GameCube/Wii sectors with junk detection (and Wii decryption).
    ///     For GC: reads blocks, detects junk via LFG, writes with Dumped/Generable status.
    ///     For Wii: decrypts partition groups, detects junk in user data, zeroes junk,
    ///     writes with Unencrypted status; plaintext areas use Dumped/Generable.
    ///     Does not copy sector tags, negative sectors, or overflow sectors.
    /// </summary>
    ErrorNumber ConvertNgcwSectors(bool useLong)
    {
        if(_aborted) return ErrorNumber.NoError;

        InitProgress?.Invoke();

        ulong totalLogicalSectors = _inputImage.Info.Sectors;
        ulong discSize            = totalLogicalSectors * Crypto.SECTOR_SIZE;

        var   jc          = new JunkCollector();
        ulong dataSectors = 0;
        ulong junkSectors = 0;

        if(_mediaType == MediaType.GOD)
        {
            ErrorNumber errno = useLong
                                    ? ConvertGameCubeSectorsLong(discSize,
                                                                 totalLogicalSectors,
                                                                 jc,
                                                                 ref dataSectors,
                                                                 ref junkSectors)
                                    : ConvertGameCubeSectors(discSize,
                                                             totalLogicalSectors,
                                                             jc,
                                                             ref dataSectors,
                                                             ref junkSectors);

            if(errno != ErrorNumber.NoError)
            {
                EndProgress?.Invoke();

                return errno;
            }
        }
        else
        {
            ErrorNumber errno = useLong
                                    ? ConvertWiiSectorsLong(discSize,
                                                            totalLogicalSectors,
                                                            jc,
                                                            ref dataSectors,
                                                            ref junkSectors)
                                    : ConvertWiiSectors(discSize,
                                                        totalLogicalSectors,
                                                        jc,
                                                        ref dataSectors,
                                                        ref junkSectors);

            if(errno != ErrorNumber.NoError)
            {
                EndProgress?.Invoke();

                return errno;
            }
        }

        // Store junk map
        if(jc.Count > 0)
        {
            byte[] junkMapData = Junk.Serialize(jc.Entries);
            _outputImage.WriteMediaTag(junkMapData, MediaTagType.NgcwJunkMap);

            UpdateStatus?.Invoke(string.Format(UI.Ngcw_stored_junk_map_0_entries_1_bytes,
                                               jc.Count,
                                               junkMapData.Length));
        }

        UpdateStatus?.Invoke(string.Format(UI.Ngcw_converted_0_data_1_junk, dataSectors, junkSectors));

        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    /// <summary>GameCube sector conversion pipeline.</summary>
    ErrorNumber ConvertGameCubeSectors(ulong     discSize,    ulong     totalLogicalSectors, JunkCollector jc,
                                       ref ulong dataSectors, ref ulong junkSectors)
    {
        const int blockSize       = Crypto.GROUP_SIZE; // 0x8000
        const int sectorsPerBlock = Crypto.LOGICAL_PER_GROUP;
        const int sectorSize      = Crypto.SECTOR_SIZE;

        // Read disc header to get FST info
        byte[] header = ReadNgcwSectors(0, 2); // first 0x1000 bytes (need 0x42C)

        if(header == null || header.Length < 0x42C) return ErrorNumber.InOutError;

        // Read extended header for FST pointers (at 0x424)
        byte[] extHeader = ReadNgcwSectors(0, (0x440 + sectorSize - 1) / sectorSize);

        var   fstOffset = BigEndianBitConverter.ToUInt32(extHeader, 0x424);
        var   fstSize   = BigEndianBitConverter.ToUInt32(extHeader, 0x428);
        ulong sysEnd    = fstOffset + fstSize;

        // Build FST data map
        DataRegion[] dataMap = null;

        if(fstSize > 0 && fstSize < 64 * 1024 * 1024)
        {
            byte[] fst = ReadNgcwBytes(fstOffset, (int)fstSize);

            if(fst != null) dataMap = DataMap.BuildFromFst(fst, 0, 0);
        }

        // Process disc in 0x8000-byte blocks
        var blockBuf       = new byte[blockSize];
        var sectorStatuses = new SectorStatus[sectorsPerBlock];

        for(ulong blockOff = 0; blockOff < discSize; blockOff += blockSize)
        {
            if(_aborted) break;

            int blockBytes = blockSize;

            if(blockOff + (ulong)blockBytes > discSize) blockBytes = (int)(discSize - blockOff);

            ulong baseSector = blockOff / sectorSize;

            UpdateProgress?.Invoke(string.Format(UI.Converting_sectors_0_to_1,
                                                 baseSector,
                                                 baseSector + sectorsPerBlock),
                                   (long)baseSector,
                                   (long)totalLogicalSectors);

            // Read block
            for(var s = 0; s < sectorsPerBlock && s * sectorSize < blockBytes; s++)
            {
                ErrorNumber errno = _inputImage.ReadSector(baseSector + (ulong)s, false, out byte[] sectorData, out _);

                if(errno != ErrorNumber.NoError || sectorData == null)
                    Array.Clear(blockBuf, s * sectorSize, sectorSize);
                else
                    Array.Copy(sectorData, 0, blockBuf, s * sectorSize, sectorSize);
            }

            // Detect junk
            Junk.DetectJunkInBlock(blockBuf,
                                   blockBytes,
                                   blockOff,
                                   dataMap,
                                   sysEnd,
                                   0xFFFF,
                                   jc,
                                   ref dataSectors,
                                   ref junkSectors,
                                   sectorStatuses);

            // Write sectors
            int numSectors = blockBytes / sectorSize;

            for(var si = 0; si < numSectors; si++)
            {
                ulong sector     = baseSector + (ulong)si;
                var   sectorData = new byte[sectorSize];
                Array.Copy(blockBuf, si * sectorSize, sectorData, 0, sectorSize);

                bool ok = _outputImage.WriteSector(sectorData, sector, false, sectorStatuses[si]);

                if(!ok)
                {
                    if(_force)
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                           _outputImage.ErrorMessage,
                                                           sector));
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                   _outputImage.ErrorMessage,
                                                                   sector));

                        return ErrorNumber.WriteError;
                    }
                }
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Fill a 32 KiB Wii encrypted group from 2048-byte logical ReadSector slices.</summary>
    void FillWiiEncryptedGroupFromShortSectors(ulong firstLogicalSector, byte[] encGrp)
    {
        const int sectorSize      = Crypto.SECTOR_SIZE;
        const int sectorsPerBlock = Crypto.LOGICAL_PER_GROUP;

        for(int s = 0; s < sectorsPerBlock; s++)
        {
            ulong       sec   = firstLogicalSector + (ulong)s;
            ErrorNumber errno = _inputImage.ReadSector(sec, false, out byte[] sd, out _);

            if(errno != ErrorNumber.NoError || sd == null)
                Array.Clear(encGrp, s * sectorSize, sectorSize);
            else
                Array.Copy(sd, 0, encGrp, s * sectorSize, sectorSize);
        }
    }

    /// <summary>
    ///     Read one encrypted Wii group from long sectors: copy main_data into <paramref name="encGrp" /> and
    ///     keep full long buffers for output (one ReadSectorLong per logical sector).
    /// </summary>
    void ReadWiiEncryptedGroupFromLongSectors(ulong firstLogicalSector, byte[] encGrp, byte[][] longSectorBuffers,
                                              int   longSectorSize)
    {
        const int sectorSize      = Crypto.SECTOR_SIZE;
        const int sectorsPerBlock = Crypto.LOGICAL_PER_GROUP;

        for(int s = 0; s < sectorsPerBlock; s++)
        {
            ulong       sec   = firstLogicalSector + (ulong)s;
            ErrorNumber errno = _inputImage.ReadSectorLong(sec, false, out byte[] sd, out _);

            byte[] stored = longSectorBuffers[s];

            if(stored == null || stored.Length != longSectorSize)
            {
                stored               = new byte[longSectorSize];
                longSectorBuffers[s] = stored;
            }

            if(errno != ErrorNumber.NoError || sd == null)
            {
                Array.Clear(encGrp, s * sectorSize, sectorSize);
                Array.Clear(stored, 0,              longSectorSize);

                continue;
            }

            int copyLong = sd.Length < longSectorSize ? sd.Length : longSectorSize;
            Array.Copy(sd, 0, stored, 0, copyLong);

            if(copyLong < longSectorSize) Array.Clear(stored, copyLong, longSectorSize - copyLong);

            if(sd.Length >= Sector.NintendoMainDataOffset + sectorSize)
                Array.Copy(sd, Sector.NintendoMainDataOffset, encGrp, s * sectorSize, sectorSize);
            else
                Array.Clear(encGrp, s * sectorSize, sectorSize);
        }
    }

    /// <summary>
    ///     Classify FST regions, run LFG junk detection on decrypted user data, and build the 32 KiB
    ///     plaintext group (hash + user, junk zeroed).
    /// </summary>
    byte[] ProcessWiiDecryptedGroup(int           inPart, ulong     groupDiscOff, byte[]    hashBlock, byte[] groupData,
                                    JunkCollector jc,     ref ulong dataSectors,  ref ulong junkSectors)
    {
        const int groupSize     = Crypto.GROUP_SIZE;
        const int hashSize      = Crypto.GROUP_HASH_SIZE;
        const int groupDataSize = Crypto.GROUP_DATA_SIZE;
        const int sectorSize    = Crypto.SECTOR_SIZE;

        ulong groupNum      = (groupDiscOff - _ngcwPartitions[inPart].DataOffset) / groupSize;
        ulong logicalOffset = groupNum                                            * groupDataSize;

        bool[] sectorIsData = new bool[16];
        int    udCount      = 0;

        for(ulong off = 0; off < groupDataSize; off += sectorSize)
        {
            ulong chunk = groupDataSize - off;

            if(chunk > sectorSize) chunk = sectorSize;

            if(logicalOffset + off < _ngcwPartSysEnd[inPart])
                sectorIsData[udCount] = true;
            else if(_ngcwPartDataMaps[inPart] != null)
            {
                sectorIsData[udCount] = DataMap.IsDataRegion(_ngcwPartDataMaps[inPart], logicalOffset + off, chunk);
            }
            else
                sectorIsData[udCount] = true;

            udCount++;
        }

        ulong blockPhase  = logicalOffset % groupSize;
        ulong block2Start = blockPhase > 0 ? groupSize - blockPhase : groupDataSize;

        if(block2Start > groupDataSize) block2Start = groupDataSize;

        bool   haveSeed1 = false;
        uint[] seed1     = new uint[Lfg.SEED_SIZE];
        bool   haveSeed2 = false;
        uint[] seed2     = new uint[Lfg.SEED_SIZE];

        for(int s = 0; s < udCount; s++)
        {
            if(sectorIsData[s]) continue;

            ulong soff     = (ulong)s * sectorSize;
            bool  inBlock2 = soff >= block2Start;

            if(inBlock2  && haveSeed2) continue;
            if(!inBlock2 && haveSeed1) continue;

            int avail = (int)(groupDataSize - soff);
            int doff  = (int)((logicalOffset + soff) % groupSize);

            if(avail < Lfg.MIN_SEED_DATA_BYTES) continue;

            uint[] dst = inBlock2 ? seed2 : seed1;
            int    m   = Lfg.GetSeed(groupData.AsSpan((int)soff, avail), doff, dst);

            if(m > 0)
            {
                if(inBlock2)
                    haveSeed2 = true;
                else
                    haveSeed1 = true;
            }

            if(haveSeed1 && haveSeed2) break;
        }

        byte[] decryptedGroup = new byte[groupSize];
        Array.Copy(hashBlock, 0, decryptedGroup, 0, hashSize);

        for(int s = 0; s < udCount; s++)
        {
            ulong off    = (ulong)s * sectorSize;
            int   chunk  = groupDataSize - (int)off;
            int   outOff = hashSize      + (int)off;

            if(chunk > sectorSize) chunk = sectorSize;

            if(sectorIsData[s])
            {
                Array.Copy(groupData, (int)off, decryptedGroup, outOff, chunk);
                dataSectors++;

                continue;
            }

            bool   inBlock2 = off >= block2Start;
            bool   haveSeed = inBlock2 ? haveSeed2 : haveSeed1;
            uint[] theSeed  = inBlock2 ? seed2 : seed1;

            if(!haveSeed)
            {
                Array.Copy(groupData, (int)off, decryptedGroup, outOff, chunk);
                dataSectors++;

                continue;
            }

            uint[] lfgBuffer = new uint[Lfg.MIN_SEED_DATA_BYTES / sizeof(uint)];
            uint[] seedCopy  = new uint[Lfg.SEED_SIZE];
            Array.Copy(theSeed, seedCopy, Lfg.SEED_SIZE);
            Lfg.SetSeed(lfgBuffer, seedCopy);
            int positionBytes = 0;

            int adv = (int)((logicalOffset + off) % groupSize);

            if(adv > 0)
            {
                byte[] discard = new byte[4096];
                int    rem     = adv;

                while(rem > 0)
                {
                    int step = rem > discard.Length ? discard.Length : rem;
                    Lfg.GetBytes(lfgBuffer, ref positionBytes, discard, 0, step);
                    rem -= step;
                }
            }

            byte[] expected = new byte[sectorSize];
            Lfg.GetBytes(lfgBuffer, ref positionBytes, expected, 0, chunk);

            if(groupData.AsSpan((int)off, chunk).SequenceEqual(expected.AsSpan(0, chunk)))
            {
                Array.Clear(decryptedGroup, outOff, chunk);
                jc.Add(groupDiscOff + hashSize + off, (ulong)chunk, (ushort)inPart, theSeed);
                junkSectors++;
            }
            else
            {
                Array.Copy(groupData, (int)off, decryptedGroup, outOff, chunk);
                dataSectors++;
            }
        }

        return decryptedGroup;
    }

    /// <summary>Wii sector conversion pipeline.</summary>
    ErrorNumber ConvertWiiSectors(ulong discSize, ulong totalLogicalSectors, JunkCollector jc, ref ulong dataSectors,
                                  ref ulong junkSectors)
    {
        const int groupSize       = Crypto.GROUP_SIZE;
        const int hashSize        = Crypto.GROUP_HASH_SIZE;
        const int groupDataSize   = Crypto.GROUP_DATA_SIZE;
        const int sectorSize      = Crypto.SECTOR_SIZE;
        const int sectorsPerBlock = Crypto.LOGICAL_PER_GROUP;

        // Build FST data maps for each partition
        BuildWiiPartitionFstMaps();

        ulong offset = 0;

        while(offset < discSize)
        {
            if(_aborted) break;

            ulong baseSector = offset / sectorSize;

            UpdateProgress?.Invoke(string.Format(UI.Converting_sectors_0_to_1,
                                                 baseSector,
                                                 baseSector + sectorsPerBlock),
                                   (long)baseSector,
                                   (long)totalLogicalSectors);

            // Check if inside a partition's data area
            int inPart = NgcwPartitions.FindPartitionAtOffset(_ngcwPartitions, offset);

            if(inPart >= 0)
            {
                // Inside partition — decrypt group, detect junk, write
                ulong groupDiscOff = _ngcwPartitions[inPart].DataOffset +
                                     (offset - _ngcwPartitions[inPart].DataOffset) / groupSize * groupSize;

                var encGrp = new byte[groupSize];
                FillWiiEncryptedGroupFromShortSectors(groupDiscOff / sectorSize, encGrp);

                var hashBlock = new byte[hashSize];
                var groupData = new byte[groupDataSize];
                Crypto.DecryptGroup(_ngcwPartitions[inPart].TitleKey, encGrp, hashBlock, groupData);

                byte[] decryptedGroup =
                    ProcessWiiDecryptedGroup(inPart,
                                             groupDiscOff,
                                             hashBlock,
                                             groupData,
                                             jc,
                                             ref dataSectors,
                                             ref junkSectors);

                // Write all 16 sectors as SectorStatusUnencrypted
                for(var s = 0; s < sectorsPerBlock; s++)
                {
                    ulong sector     = groupDiscOff / sectorSize + (ulong)s;
                    var   sectorData = new byte[sectorSize];
                    Array.Copy(decryptedGroup, s * sectorSize, sectorData, 0, sectorSize);

                    bool ok = _outputImage.WriteSector(sectorData, sector, false, SectorStatus.Unencrypted);

                    if(!ok)
                    {
                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                               _outputImage.ErrorMessage,
                                                               sector));
                        }
                        else
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                       _outputImage.ErrorMessage,
                                                                       sector));

                            return ErrorNumber.WriteError;
                        }
                    }
                }

                offset = groupDiscOff + groupSize;
            }
            else
            {
                // Outside partition — read as plaintext, detect junk
                const int blockSize  = groupSize;
                ulong     alignedOff = offset & ~(ulong)(blockSize - 1);

                int blockBytes = blockSize;

                if(alignedOff + (ulong)blockBytes > discSize) blockBytes = (int)(discSize - alignedOff);

                var blockBuf = new byte[blockSize];

                for(var s = 0; s < sectorsPerBlock && s * sectorSize < blockBytes; s++)
                {
                    ulong       sec   = alignedOff / sectorSize + (ulong)s;
                    ErrorNumber errno = _inputImage.ReadSector(sec, false, out byte[] sd, out _);

                    if(errno != ErrorNumber.NoError || sd == null)
                        Array.Clear(blockBuf, s * sectorSize, sectorSize);
                    else
                        Array.Copy(sd, 0, blockBuf, s * sectorSize, sectorSize);
                }

                var sectorStatuses = new SectorStatus[sectorsPerBlock];

                Junk.DetectJunkInBlock(blockBuf,
                                       blockBytes,
                                       alignedOff,
                                       null,
                                       0x50000,
                                       0xFFFF,
                                       jc,
                                       ref dataSectors,
                                       ref junkSectors,
                                       sectorStatuses);

                int numSectors = blockBytes / sectorSize;

                for(var si = 0; si < numSectors; si++)
                {
                    ulong sector     = alignedOff / sectorSize + (ulong)si;
                    var   sectorData = new byte[sectorSize];
                    Array.Copy(blockBuf, si * sectorSize, sectorData, 0, sectorSize);

                    bool ok = _outputImage.WriteSector(sectorData, sector, false, sectorStatuses[si]);

                    if(!ok)
                    {
                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                               _outputImage.ErrorMessage,
                                                               sector));
                        }
                        else
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                       _outputImage.ErrorMessage,
                                                                       sector));

                            return ErrorNumber.WriteError;
                        }
                    }
                }

                offset = alignedOff + (ulong)blockBytes;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Build FST data maps for each Wii partition.</summary>
    void BuildWiiPartitionFstMaps()
    {
        if(_ngcwPartitions == null || _ngcwPartitions.Count == 0) return;

        _ngcwPartDataMaps = new DataRegion[_ngcwPartitions.Count][];
        _ngcwPartSysEnd   = new ulong[_ngcwPartitions.Count];

        for(var p = 0; p < _ngcwPartitions.Count; p++)
        {
            // Read and decrypt first group to get boot block
            var encGrp0       = new byte[Crypto.GROUP_SIZE];
            int sectorsToRead = Crypto.LOGICAL_PER_GROUP;

            for(var s = 0; s < sectorsToRead; s++)
            {
                ulong       sec   = _ngcwPartitions[p].DataOffset / Crypto.SECTOR_SIZE + (ulong)s;
                ErrorNumber errno = _inputImage.ReadSector(sec, false, out byte[] sd, out _);

                if(errno != ErrorNumber.NoError || sd == null) continue;

                Array.Copy(sd, 0, encGrp0, s * Crypto.SECTOR_SIZE, Crypto.SECTOR_SIZE);
            }

            var hb0 = new byte[Crypto.GROUP_HASH_SIZE];
            var gd0 = new byte[Crypto.GROUP_DATA_SIZE];
            Crypto.DecryptGroup(_ngcwPartitions[p].TitleKey, encGrp0, hb0, gd0);

            uint fstOffsetP = BigEndianBitConverter.ToUInt32(gd0, 0x424) << 2;
            uint fstSizeP   = BigEndianBitConverter.ToUInt32(gd0, 0x428) << 2;

            _ngcwPartSysEnd[p] = fstOffsetP + fstSizeP;

            if(fstSizeP == 0 || fstSizeP >= 64 * 1024 * 1024) continue;

            // Read FST from the partition (decrypting groups as needed)
            var  fstP    = new byte[fstSizeP];
            uint fstRead = 0;
            var  fstOk   = true;

            while(fstRead < fstSizeP)
            {
                ulong logicalOff = fstOffsetP + fstRead;
                ulong grpIdx     = logicalOff / Crypto.GROUP_DATA_SIZE;
                var   grpOff     = (int)(logicalOff % Crypto.GROUP_DATA_SIZE);
                ulong discOff    = _ngcwPartitions[p].DataOffset + grpIdx * Crypto.GROUP_SIZE;

                var encG = new byte[Crypto.GROUP_SIZE];

                for(var s = 0; s < sectorsToRead; s++)
                {
                    ulong       sec   = discOff / Crypto.SECTOR_SIZE + (ulong)s;
                    ErrorNumber errno = _inputImage.ReadSector(sec, false, out byte[] sd, out _);

                    if(errno != ErrorNumber.NoError || sd == null)
                    {
                        fstOk = false;

                        break;
                    }

                    Array.Copy(sd, 0, encG, s * Crypto.SECTOR_SIZE, Crypto.SECTOR_SIZE);
                }

                if(!fstOk) break;

                var hb = new byte[Crypto.GROUP_HASH_SIZE];
                var gd = new byte[Crypto.GROUP_DATA_SIZE];
                Crypto.DecryptGroup(_ngcwPartitions[p].TitleKey, encG, hb, gd);

                int avail = Crypto.GROUP_DATA_SIZE - grpOff;
                var chunk = (int)(fstSizeP - fstRead < (uint)avail ? fstSizeP - fstRead : (uint)avail);
                Array.Copy(gd, grpOff, fstP, (int)fstRead, chunk);
                fstRead += (uint)chunk;
            }

            if(fstOk) _ngcwPartDataMaps[p] = DataMap.BuildFromFst(fstP, 0, 2);
        }
    }

    /// <summary>
    ///     Extracts game metadata from the disc header and sets MediaTitle,
    ///     MediaPartNumber, and MediaSequence on the output image.
    /// </summary>
    void EnrichNgcwMetadata()
    {
        if(_aborted) return;

        ErrorNumber errno = _inputImage.ReadSector(0, false, out byte[] header, out _);

        if(errno != ErrorNumber.NoError || header == null) return;

        if(header.Length < 0x60) return;

        // Game title: 64 bytes at offset 0x20, null/space-trimmed
        var titleLen = 64;

        if(header.Length < 0x20 + titleLen) titleLen = header.Length - 0x20;

        while(titleLen > 0 && (header[0x20 + titleLen - 1] == 0 || header[0x20 + titleLen - 1] == ' ')) titleLen--;

        if(titleLen > 0)
        {
            string title = Encoding.ASCII.GetString(header, 0x20, titleLen);

            _outputImage.SetImageInfo(new CommonTypes.Structs.ImageInfo
            {
                MediaTitle = title
            });

            UpdateStatus?.Invoke(string.Format(UI.Ngcw_title_0, title));
        }

        // Game ID (6 bytes at offset 0) → MediaPartNumber
        var codeLen = 6;

        while(codeLen > 0 && (header[codeLen - 1] == 0 || header[codeLen - 1] == ' ')) codeLen--;

        if(codeLen > 0)
        {
            string gameId = Encoding.ASCII.GetString(header, 0, codeLen);

            _outputImage.SetImageInfo(new CommonTypes.Structs.ImageInfo
            {
                MediaPartNumber = gameId
            });

            UpdateStatus?.Invoke(string.Format(UI.Ngcw_game_id_0, gameId));
        }

        // Disc number: byte at offset 6
        byte discNumber = header[6];

        if(discNumber > 0)
        {
            _outputImage.SetImageInfo(new CommonTypes.Structs.ImageInfo
            {
                MediaSequence     = discNumber + 1,
                LastMediaSequence = discNumber + 1
            });

            UpdateStatus?.Invoke(string.Format(UI.Ngcw_disc_number_0, discNumber + 1));
        }
    }

    /// <summary>Read multiple consecutive sectors and return as a single byte array.</summary>
    byte[] ReadNgcwSectors(ulong startSector, int count)
    {
        var result = new byte[count * Crypto.SECTOR_SIZE];

        for(var i = 0; i < count; i++)
        {
            ErrorNumber errno = _inputImage.ReadSector(startSector + (ulong)i, false, out byte[] data, out _);

            if(errno != ErrorNumber.NoError || data == null) return null;

            Array.Copy(data, 0, result, i * Crypto.SECTOR_SIZE, Crypto.SECTOR_SIZE);
        }

        return result;
    }

    /// <summary>Read arbitrary bytes from a disc image at a given byte offset.</summary>
    byte[] ReadNgcwBytes(ulong byteOffset, int length)
    {
        var result = new byte[length];
        var read   = 0;

        while(read < length)
        {
            ulong sector    = (byteOffset + (ulong)read) / Crypto.SECTOR_SIZE;
            var   sectorOff = (int)((byteOffset + (ulong)read) % Crypto.SECTOR_SIZE);
            int   chunk     = Crypto.SECTOR_SIZE - sectorOff;

            if(chunk > length - read) chunk = length - read;

            ErrorNumber errno = _inputImage.ReadSector(sector, false, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError || sectorData == null) return null;

            Array.Copy(sectorData, sectorOff, result, read, chunk);
            read += chunk;
        }

        return result;
    }

    /// GameCube sector conversion pipeline for long sectors.
    ErrorNumber ConvertGameCubeSectorsLong(ulong     discSize,    ulong     totalLogicalSectors, JunkCollector jc,
                                           ref ulong dataSectors, ref ulong junkSectors)
    {
        const int blockSize       = Crypto.GROUP_SIZE; // 0x8000 logical (16 × 2048 user bytes)
        const int sectorsPerBlock = Crypto.LOGICAL_PER_GROUP;
        const int userSize        = Crypto.SECTOR_SIZE;

        // Junk / FST offsets are in logical (user) byte space. User 2048-byte slices match ReadSector (Nintendo main_data at Sector.NintendoMainDataOffset).
        ErrorNumber probeErr = _inputImage.ReadSectorLong(0, false, out byte[] longProbe, out _);

        if(probeErr         != ErrorNumber.NoError ||
           longProbe        == null                ||
           longProbe.Length < Sector.NintendoMainDataOffset + userSize)
            return ErrorNumber.InOutError;

        int longSectorSize = longProbe.Length;

        // Read disc header to get FST info (logical user bytes via ReadSector)
        byte[] header = ReadNgcwSectors(0, 2); // first 0x1000 bytes (need 0x42C)

        if(header == null || header.Length < 0x42C) return ErrorNumber.InOutError;

        // Read extended header for FST pointers (at 0x424)
        byte[] extHeader = ReadNgcwSectors(0, (0x440 + userSize - 1) / userSize);

        var   fstOffset = BigEndianBitConverter.ToUInt32(extHeader, 0x424);
        var   fstSize   = BigEndianBitConverter.ToUInt32(extHeader, 0x428);
        ulong sysEnd    = fstOffset + fstSize;

        // Build FST data map
        DataRegion[] dataMap = null;

        if(fstSize > 0 && fstSize < 64 * 1024 * 1024)
        {
            byte[] fst = ReadNgcwBytes(fstOffset, (int)fstSize);

            if(fst != null) dataMap = DataMap.BuildFromFst(fst, 0, 0);
        }

        // User-only buffer for LFG / data-region junk detection (same as ConvertGameCubeSectors)
        var userBlockBuf   = new byte[blockSize];
        var longSectorBufs = new byte[sectorsPerBlock][];
        var sectorStatuses = new SectorStatus[sectorsPerBlock];

        for(ulong blockOff = 0; blockOff < discSize; blockOff += blockSize)
        {
            if(_aborted) break;

            int blockBytes = blockSize;

            if(blockOff + (ulong)blockBytes > discSize) blockBytes = (int)(discSize - blockOff);

            ulong baseSector = blockOff / userSize;

            UpdateProgress?.Invoke(string.Format(UI.Converting_sectors_0_to_1,
                                                 baseSector,
                                                 baseSector + sectorsPerBlock),
                                   (long)baseSector,
                                   (long)totalLogicalSectors);

            // Read long sectors; pack user main data into userBlockBuf; keep full long buffers for output
            for(var s = 0; s < sectorsPerBlock && s * userSize < blockBytes; s++)
            {
                ErrorNumber errno =
                    _inputImage.ReadSectorLong(baseSector + (ulong)s, false, out byte[] sectorData, out _);

                byte[] stored = new byte[longSectorSize];
                longSectorBufs[s] = stored;

                if(errno != ErrorNumber.NoError || sectorData == null)
                {
                    Array.Clear(userBlockBuf, s * userSize, userSize);

                    continue;
                }

                int copyLong = sectorData.Length < longSectorSize ? sectorData.Length : longSectorSize;
                Array.Copy(sectorData, 0, stored, 0, copyLong);

                if(sectorData.Length >= Sector.NintendoMainDataOffset + userSize)
                    Array.Copy(sectorData, Sector.NintendoMainDataOffset, userBlockBuf, s * userSize, userSize);
                else
                    Array.Clear(userBlockBuf, s * userSize, userSize);
            }

            Junk.DetectJunkInBlock(userBlockBuf,
                                   blockBytes,
                                   blockOff,
                                   dataMap,
                                   sysEnd,
                                   0xFFFF,
                                   jc,
                                   ref dataSectors,
                                   ref junkSectors,
                                   sectorStatuses);

            int numSectors = blockBytes / userSize;

            for(var si = 0; si < numSectors; si++)
            {
                ulong sector = baseSector + (ulong)si;
                bool  ok     = _outputImage.WriteSectorLong(longSectorBufs[si], sector, false, sectorStatuses[si]);

                if(!ok)
                {
                    if(_force)
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                           _outputImage.ErrorMessage,
                                                           sector));
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                   _outputImage.ErrorMessage,
                                                                   sector));

                        return ErrorNumber.WriteError;
                    }
                }
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Wii sector conversion pipeline for long sectors (main_data + framing).</summary>
    ErrorNumber ConvertWiiSectorsLong(ulong     discSize,    ulong     totalLogicalSectors, JunkCollector jc,
                                      ref ulong dataSectors, ref ulong junkSectors)
    {
        const int groupSize       = Crypto.GROUP_SIZE;
        const int hashSize        = Crypto.GROUP_HASH_SIZE;
        const int groupDataSize   = Crypto.GROUP_DATA_SIZE;
        const int sectorSize      = Crypto.SECTOR_SIZE;
        const int sectorsPerBlock = Crypto.LOGICAL_PER_GROUP;

        ErrorNumber probeErr = _inputImage.ReadSectorLong(0, false, out byte[] longProbe, out _);

        if(probeErr         != ErrorNumber.NoError ||
           longProbe        == null                ||
           longProbe.Length < Sector.NintendoMainDataOffset + sectorSize)
            return ErrorNumber.InOutError;

        int longSectorSize = longProbe.Length;

        BuildWiiPartitionFstMaps();

        var longSectorBufs = new byte[sectorsPerBlock][];

        ulong offset = 0;

        while(offset < discSize)
        {
            if(_aborted) break;

            ulong baseSector = offset / sectorSize;

            UpdateProgress?.Invoke(string.Format(UI.Converting_sectors_0_to_1,
                                                 baseSector,
                                                 baseSector + sectorsPerBlock),
                                   (long)baseSector,
                                   (long)totalLogicalSectors);

            int inPart = NgcwPartitions.FindPartitionAtOffset(_ngcwPartitions, offset);

            if(inPart >= 0)
            {
                ulong groupDiscOff = _ngcwPartitions[inPart].DataOffset +
                                     (offset - _ngcwPartitions[inPart].DataOffset) / groupSize * groupSize;

                var encGrp = new byte[groupSize];
                ReadWiiEncryptedGroupFromLongSectors(groupDiscOff / sectorSize, encGrp, longSectorBufs, longSectorSize);

                var hashBlock = new byte[hashSize];
                var groupData = new byte[groupDataSize];
                Crypto.DecryptGroup(_ngcwPartitions[inPart].TitleKey, encGrp, hashBlock, groupData);

                byte[] decryptedGroup =
                    ProcessWiiDecryptedGroup(inPart,
                                             groupDiscOff,
                                             hashBlock,
                                             groupData,
                                             jc,
                                             ref dataSectors,
                                             ref junkSectors);

                // The plaintext group (hash block + user data) spans the 16 sectors' main data as is
                for(int s = 0; s < sectorsPerBlock; s++)
                {
                    Array.Copy(decryptedGroup,
                               s * sectorSize,
                               longSectorBufs[s],
                               Sector.NintendoMainDataOffset,
                               sectorSize);
                }

                for(int s = 0; s < sectorsPerBlock; s++)
                {
                    ulong sector = groupDiscOff / sectorSize + (ulong)s;
                    bool  ok = _outputImage.WriteSectorLong(longSectorBufs[s], sector, false, SectorStatus.Unencrypted);

                    if(!ok)
                    {
                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                               _outputImage.ErrorMessage,
                                                               sector));
                        }
                        else
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                       _outputImage.ErrorMessage,
                                                                       sector));

                            return ErrorNumber.WriteError;
                        }
                    }
                }

                offset = groupDiscOff + groupSize;
            }
            else
            {
                const int blockSize  = groupSize;
                ulong     alignedOff = offset & ~(ulong)(blockSize - 1);

                int blockBytes = blockSize;

                if(alignedOff + (ulong)blockBytes > discSize) blockBytes = (int)(discSize - alignedOff);

                var blockBuf = new byte[blockSize];

                for(int s = 0; s < sectorsPerBlock && s * sectorSize < blockBytes; s++)
                {
                    ulong       sec   = alignedOff / sectorSize + (ulong)s;
                    ErrorNumber errno = _inputImage.ReadSectorLong(sec, false, out byte[] sd, out _);

                    byte[] stored = new byte[longSectorSize];
                    longSectorBufs[s] = stored;

                    if(errno != ErrorNumber.NoError || sd == null)
                    {
                        Array.Clear(blockBuf, s * sectorSize, sectorSize);
                        Array.Clear(stored,   0,              longSectorSize);

                        continue;
                    }

                    int copyLong = sd.Length < longSectorSize ? sd.Length : longSectorSize;
                    Array.Copy(sd, 0, stored, 0, copyLong);

                    if(copyLong < longSectorSize) Array.Clear(stored, copyLong, longSectorSize - copyLong);

                    if(sd.Length >= Sector.NintendoMainDataOffset + sectorSize)
                        Array.Copy(sd, Sector.NintendoMainDataOffset, blockBuf, s * sectorSize, sectorSize);
                    else
                        Array.Clear(blockBuf, s * sectorSize, sectorSize);
                }

                var sectorStatuses = new SectorStatus[sectorsPerBlock];

                Junk.DetectJunkInBlock(blockBuf,
                                       blockBytes,
                                       alignedOff,
                                       null,
                                       0x50000,
                                       0xFFFF,
                                       jc,
                                       ref dataSectors,
                                       ref junkSectors,
                                       sectorStatuses);

                int numSectors = blockBytes / sectorSize;

                for(int si = 0; si < numSectors; si++)
                {
                    ulong sector = alignedOff / sectorSize + (ulong)si;
                    bool  ok     = _outputImage.WriteSectorLong(longSectorBufs[si], sector, false, sectorStatuses[si]);

                    if(!ok)
                    {
                        if(_force)
                        {
                            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                               _outputImage.ErrorMessage,
                                                               sector));
                        }
                        else
                        {
                            StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                       _outputImage.ErrorMessage,
                                                                       sector));

                            return ErrorNumber.WriteError;
                        }
                    }
                }

                offset = alignedOff + (ulong)blockBytes;
            }
        }

        return ErrorNumber.NoError;
    }
}