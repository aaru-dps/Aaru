using System;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Core.Image.WiiU;
using Aaru.Localization;
using Humanizer;

namespace Aaru.Core.Image;

public partial class Convert
{
    /// <summary>Wii U disc key (16 bytes).</summary>
    byte[] _wiiuDiscKey;
    /// <summary>Parsed Wii U partitions from TOC, used by both InjectWiiuMediaTags and ConvertWiiuSectors.</summary>
    WiiuPartition[] _wiiuPartitions;
    /// <summary>Wii U partition region map, used by ConvertWiiuSectors.</summary>
    WiiuPartitionRegion[] _wiiuRegions;

    /// <summary>
    ///     Resolves the Wii U disc key, parses the TOC, extracts title keys,
    ///     builds the partition key map, and writes Wii U-specific media tags
    ///     (WiiUDiscKey, WiiUPartitionKeyMap) to the output image.
    ///     Must be called before ConvertMediaTags.
    /// </summary>
    ErrorNumber InjectWiiuMediaTags()
    {
        if(_aborted) return ErrorNumber.NoError;

        InitProgress?.Invoke();

        // Step 1: Get disc key from source image media tags
        ErrorNumber errno = _inputImage.ReadMediaTag(MediaTagType.WiiUDiscKey, out _wiiuDiscKey);

        if(errno != ErrorNumber.NoError || _wiiuDiscKey is not { Length: 16 })
        {
            EndProgress?.Invoke();
            StoppingErrorMessage?.Invoke(UI.WiiU_no_disc_key_found);

            return ErrorNumber.NoData;
        }

        UpdateStatus?.Invoke(UI.WiiU_disc_key_loaded);

        // Step 2: Parse TOC
        PulseProgress?.Invoke(UI.WiiU_parsing_partition_table);

        if(!WiiuToc.ParseToc(_inputImage, _wiiuDiscKey, out _wiiuPartitions))
        {
            EndProgress?.Invoke();
            StoppingErrorMessage?.Invoke(UI.WiiU_cannot_parse_toc);

            return ErrorNumber.InvalidArgument;
        }

        UpdateStatus?.Invoke(string.Format(UI.WiiU_found_0_partitions, _wiiuPartitions.Length));

        // Step 3: Extract title keys from SI/GI partitions
        PulseProgress?.Invoke(UI.WiiU_extracting_title_keys);

        int keysFound = WiiuTitleKeys.ExtractTitleKeys(_inputImage, _wiiuDiscKey, _wiiuPartitions);

        UpdateStatus?.Invoke(string.Format(UI.WiiU_extracted_0_title_keys, keysFound));

        // Step 4: Build partition region map
        PulseProgress?.Invoke(UI.WiiU_building_partition_key_map);

        var totalPhysicalSectors = (uint)(_inputImage.Info.Sectors / WiiuCrypto.LOGICAL_PER_PHYSICAL);
        _wiiuRegions = WiiuPartitionMap.BuildRegionMap(_wiiuPartitions, totalPhysicalSectors);

        // Step 5: Write media tags
        PulseProgress?.Invoke(string.Format(UI.WiiU_writing_media_tag_0, MediaTagType.WiiUDiscKey.Humanize()));

        _outputImage.WriteMediaTag(_wiiuDiscKey, MediaTagType.WiiUDiscKey);

        byte[] keyMapData = WiiuPartitionMap.Serialize(_wiiuRegions);

        PulseProgress?.Invoke(string.Format(UI.WiiU_writing_media_tag_0, MediaTagType.WiiUPartitionKeyMap.Humanize()));

        _outputImage.WriteMediaTag(keyMapData, MediaTagType.WiiUPartitionKeyMap);

        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Converts Wii U sectors from input to output, decrypting encrypted physical sectors
    ///     and writing 16 logical sectors per physical sector with appropriate SectorStatus.
    ///     Plaintext sectors get <see cref="SectorStatus.Dumped" />, decrypted sectors get
    ///     <see cref="SectorStatus.Unencrypted" />.
    ///     Does not copy sector tags, negative sectors, or overflow sectors.
    /// </summary>
    ErrorNumber ConvertWiiuSectors()
    {
        if(_aborted) return ErrorNumber.NoError;

        InitProgress?.Invoke();

        ulong totalLogicalSectors  = _inputImage.Info.Sectors;
        ulong totalPhysicalSectors = totalLogicalSectors / WiiuCrypto.LOGICAL_PER_PHYSICAL;

        for(ulong phys = 0; phys < totalPhysicalSectors; phys++)
        {
            if(_aborted) break;

            ulong baseLogical = phys * WiiuCrypto.LOGICAL_PER_PHYSICAL;

            UpdateProgress?.Invoke(string.Format(UI.Converting_sectors_0_to_1,
                                                 baseLogical,
                                                 baseLogical + WiiuCrypto.LOGICAL_PER_PHYSICAL),
                                   (long)baseLogical,
                                   (long)totalLogicalSectors);

            // Read 16 logical sectors (one physical sector) from source
            var physSectorBuf = new byte[WiiuCrypto.SECTOR_SIZE];

            for(uint s = 0; s < WiiuCrypto.LOGICAL_PER_PHYSICAL; s++)
            {
                ErrorNumber errno = _inputImage.ReadSector(baseLogical + s, false, out byte[] logicalSector, out _);

                if(errno != ErrorNumber.NoError)
                {
                    // Zero-fill on error
                    Array.Clear(physSectorBuf,
                                (int)(s * WiiuCrypto.LOGICAL_SECTOR_SIZE),
                                WiiuCrypto.LOGICAL_SECTOR_SIZE);

                    continue;
                }

                Array.Copy(logicalSector,
                           0,
                           physSectorBuf,
                           (int)(s * WiiuCrypto.LOGICAL_SECTOR_SIZE),
                           WiiuCrypto.LOGICAL_SECTOR_SIZE);
            }

            // Determine if this physical sector is plaintext or encrypted
            var          isPlaintext = false;
            SectorStatus writeStatus;

            if(phys < WiiuCrypto.HEADER_PHYSICAL_SECTORS)
                isPlaintext = true;
            else
            {
                // Partition start sectors are plaintext
                for(var pi = 0; pi < _wiiuPartitions.Length; pi++)
                {
                    if(phys == _wiiuPartitions[pi].StartSector)
                    {
                        isPlaintext = true;

                        break;
                    }
                }
            }

            if(isPlaintext)
                writeStatus = SectorStatus.Dumped;
            else
            {
                // Find which partition region this sector belongs to and decrypt
                byte[] partKey = null;

                for(var pi = 0; pi < _wiiuRegions.Length; pi++)
                {
                    if(phys > _wiiuRegions[pi].StartSector && phys < _wiiuRegions[pi].EndSector)
                    {
                        partKey = _wiiuRegions[pi].Key;

                        break;
                    }
                }

                if(partKey != null)
                {
                    physSectorBuf = WiiuCrypto.DecryptSector(partKey, physSectorBuf);
                    writeStatus   = SectorStatus.Unencrypted;
                }
                else
                {
                    // Outside any known partition — store as plaintext
                    writeStatus = SectorStatus.Dumped;
                }
            }

            // Write 16 logical sectors with uniform status
            for(uint s = 0; s < WiiuCrypto.LOGICAL_PER_PHYSICAL; s++)
            {
                ulong logical    = baseLogical + s;
                var   sectorData = new byte[WiiuCrypto.LOGICAL_SECTOR_SIZE];

                Array.Copy(physSectorBuf,
                           (int)(s * WiiuCrypto.LOGICAL_SECTOR_SIZE),
                           sectorData,
                           0,
                           WiiuCrypto.LOGICAL_SECTOR_SIZE);

                bool ok = _outputImage.WriteSector(sectorData, logical, false, writeStatus);

                if(!ok)
                {
                    if(_force)
                    {
                        ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_continuing,
                                                           _outputImage.ErrorMessage,
                                                           logical));
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke(string.Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                   _outputImage.ErrorMessage,
                                                                   logical));

                        return ErrorNumber.WriteError;
                    }
                }
            }
        }

        EndProgress?.Invoke();

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Extracts Wii U product code and disc number from the disc header (logical sector 0)
    ///     and sets MediaPartNumber and MediaSequence on the output image.
    /// </summary>
    void EnrichWiiuMetadata()
    {
        if(_aborted) return;

        ErrorNumber errno = _inputImage.ReadSector(0, false, out byte[] header, out _);

        if(errno != ErrorNumber.NoError) return;

        if(header.Length < 0x16) return;

        // Product code: first 10 bytes, trimmed of nulls and spaces
        var codeLen = 10;

        while(codeLen > 0 && (header[codeLen - 1] == 0 || header[codeLen - 1] == ' ')) codeLen--;

        if(codeLen > 0)
        {
            string productCode = Encoding.ASCII.GetString(header, 0, codeLen);

            // SetImageInfo() replaces all the metadata, so keep what is already set (creator, comments...)
            CommonTypes.Structs.ImageInfo info = _outputImage.Info;
            info.MediaPartNumber = productCode;
            _outputImage.SetImageInfo(info);

            UpdateStatus?.Invoke(string.Format(UI.WiiU_product_code_0, productCode));
        }

        // Disc number: byte at offset 0x15 (ASCII digit)
        if(header[0x15] >= '0' && header[0x15] <= '9')
        {
            int discNum = header[0x15] - '0' + 1;

            CommonTypes.Structs.ImageInfo info = _outputImage.Info;
            info.MediaSequence     = discNum;
            info.LastMediaSequence = discNum;
            _outputImage.SetImageInfo(info);

            UpdateStatus?.Invoke(string.Format(UI.WiiU_disc_number_0, discNum));
        }
    }
}