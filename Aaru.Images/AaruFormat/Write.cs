// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes Aaru Format disk images.
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
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Decoders;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;
using Partition = Aaru.CommonTypes.Partition;
using Session = Aaru.CommonTypes.Structs.Session;
using TapeFile = Aaru.CommonTypes.Structs.TapeFile;
using TapePartition = Aaru.CommonTypes.Structs.TapePartition;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages;

public sealed partial class AaruFormat
{
    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        uint sectorsPerBlock;
        uint maxDdtSize;
        bool doMd5;
        bool doSha1;
        bool doSha256;
        bool doSpamsum;

        if(options != null)
        {
            if(options.TryGetValue("sectors_per_block", out string tmpValue))
            {
                if(!uint.TryParse(tmpValue, out sectorsPerBlock))
                {
                    ErrorMessage = Localization.Invalid_value_for_sectors_per_block_option;

                    return false;
                }
            }
            else
                sectorsPerBlock = 4096;

            if(options.TryGetValue("dictionary", out tmpValue))
            {
                if(!uint.TryParse(tmpValue, out _dictionarySize))
                {
                    ErrorMessage = Localization.Invalid_value_for_dictionary_option;

                    return false;
                }
            }
            else
                _dictionarySize = 1 << 25;

            if(options.TryGetValue("max_ddt_size", out tmpValue))
            {
                if(!uint.TryParse(tmpValue, out maxDdtSize))
                {
                    ErrorMessage = Localization.Invalid_value_for_max_ddt_size_option;

                    return false;
                }
            }
            else
                maxDdtSize = 256;

            if(options.TryGetValue("md5", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out doMd5))
                {
                    ErrorMessage = Localization.Invalid_value_for_md5_option;

                    return false;
                }
            }
            else
                doMd5 = true;

            if(options.TryGetValue("sha1", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out doSha1))
                {
                    ErrorMessage = Localization.Invalid_value_for_sha1_option;

                    return false;
                }
            }
            else
                doSha1 = true;

            if(options.TryGetValue("sha256", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out doSha256))
                {
                    ErrorMessage = Localization.Invalid_value_for_sha256_option;

                    return false;
                }
            }
            else
                doSha256 = true;

            if(options.TryGetValue("spamsum", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out doSpamsum))
                {
                    ErrorMessage = Localization.Invalid_value_for_spamsum_option;

                    return false;
                }
            }
            else
                doSpamsum = false;

            if(options.TryGetValue("deduplicate", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out _deduplicate))
                {
                    ErrorMessage = Localization.Invalid_value_for_deduplicate_option;

                    return false;
                }
            }
            else
                _deduplicate = true;

            if(options.TryGetValue("compress", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out _compress))
                {
                    ErrorMessage = Localization.Invalid_value_for_compress_option;

                    return false;
                }
            }
            else
                _compress = true;
        }
        else
        {
            sectorsPerBlock = 4096;
            _dictionarySize = 1 << 25;
            maxDdtSize      = 256;
            doMd5           = true;
            doSha1          = true;
            doSha256        = true;
            doSpamsum       = false;
            _deduplicate    = true;
            _compress       = true;
        }

        _compressionAlgorithm = _compress ? CompressionType.Lzma : CompressionType.None;

        // This really, cannot happen
        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        // Calculate shift
        _shift = 0;
        uint oldSectorsPerBlock = sectorsPerBlock;

        while(sectorsPerBlock > 1)
        {
            sectorsPerBlock >>= 1;
            _shift++;
        }

        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Got_a_shift_of_0_for_1_sectors_per_block, _shift,
                                   oldSectorsPerBlock);

        _imageInfo = new ImageInfo
        {
            MediaType         = mediaType,
            SectorSize        = sectorSize,
            Sectors           = sectors,
            MetadataMediaType = GetMetadataMediaType(mediaType)
        };

        try
        {
            _imageStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException e)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, e.Message);

            return false;
        }

        // Check if appending to an existing image
        if(_imageStream.Length > Marshal.SizeOf<AaruHeader>())
        {
            _structureBytes = new byte[Marshal.SizeOf<AaruHeader>()];
            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
            _header = Marshal.ByteArrayToStructureLittleEndian<AaruHeader>(_structureBytes);

            if(_header.identifier != DIC_MAGIC &&
               _header.identifier != AARU_MAGIC)
            {
                ErrorMessage = Localization.Cannot_append_to_a_non_Aaru_Format_image;

                return false;
            }

            if(_header.imageMajorVersion > AARUFMT_VERSION)
            {
                ErrorMessage = string.Format(Localization.Cannot_append_to_an_unknown_image_version_0,
                                             _header.imageMajorVersion);

                return false;
            }

            if(_header.mediaType != mediaType)
            {
                ErrorMessage = string.Format(Localization.Cannot_write_a_media_with_type_0_to_an_image_with_type_1,
                                             mediaType, _header.mediaType);

                return false;
            }
        }
        else
        {
            _header = new AaruHeader
            {
                identifier   = AARU_MAGIC,
                mediaType    = mediaType,
                creationTime = DateTime.UtcNow.ToFileTimeUtc()
            };

            _imageStream.Write(new byte[Marshal.SizeOf<AaruHeader>()], 0, Marshal.SizeOf<AaruHeader>());
        }

        _header.application             = "Aaru";
        _header.imageMajorVersion       = AARUFMT_VERSION_V1;
        _header.imageMinorVersion       = 0;
        _header.applicationMajorVersion = (byte)typeof(AaruFormat).Assembly.GetName().Version.Major;
        _header.applicationMinorVersion = (byte)typeof(AaruFormat).Assembly.GetName().Version.Minor;

        // Initialize tables
        _index                        = new List<IndexEntry>();
        _mediaTags                    = new Dictionary<MediaTagType, byte[]>();
        _checksumProvider             = SHA256.Create();
        _deduplicationTable           = new Dictionary<string, ulong>();
        _trackIsrcs                   = new Dictionary<byte, string>();
        _trackFlags                   = new Dictionary<byte, byte>();
        _imageInfo.ReadableSectorTags = new List<SectorTagType>();

        // If there exists an index, we are appending, so read index
        if(_header.indexOffset > 0)
        {
            List<CompactDiscIndexEntry> compactDiscIndexes = null;

            // Initialize caches
            _blockCache       = new Dictionary<ulong, byte[]>();
            _blockHeaderCache = new Dictionary<ulong, BlockHeader>();
            _currentCacheSize = 0;

            // Can't calculate checksum of an appended image
            _md5Provider     = null;
            _sha1Provider    = null;
            _sha256Provider  = null;
            _spamsumProvider = null;

            _imageStream.Position = (long)_header.indexOffset;
            _structureBytes       = new byte[Marshal.SizeOf<IndexHeader>()];
            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
            IndexHeader idxHeader = Marshal.SpanToStructureLittleEndian<IndexHeader>(_structureBytes);

            if(idxHeader.identifier != BlockType.Index)
            {
                ErrorMessage = Localization.Index_not_found_in_existing_image_cannot_continue;

                return false;
            }

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Index_at_0_contains_1_entries,
                                       _header.indexOffset, idxHeader.entries);

            for(ushort i = 0; i < idxHeader.entries; i++)
            {
                _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(_structureBytes);

                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                           Localization.Block_type_0_with_data_type_1_is_indexed_to_be_at_2,
                                           entry.blockType, entry.dataType, entry.offset);

                _index.Add(entry);
            }

            // Invalidate previous checksum block
            _index.RemoveAll(t => t is { blockType: BlockType.ChecksumBlock, dataType: DataType.NoData });

            bool foundUserDataDdt = false;

            foreach(IndexEntry entry in _index)
            {
                _imageStream.Position = (long)entry.offset;

                switch(entry.blockType)
                {
                    case BlockType.DataBlock:
                        // NOP block, skip
                        if(entry.dataType == DataType.NoData)
                            break;

                        _imageStream.Position = (long)entry.offset;

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                        BlockHeader blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(_structureBytes);
                        _imageInfo.ImageSize += blockHeader.cmpLength;

                        // Unused, skip
                        if(entry.dataType == DataType.UserData)
                            break;

                        if(blockHeader.identifier != entry.blockType)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Incorrect_identifier_for_data_block_at_position_0,
                                                       entry.offset);

                            break;
                        }

                        if(blockHeader.type != entry.dataType)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Expected_block_with_data_type_0_at_position_1_but_found_data_type_2,
                                                       entry.dataType, entry.offset, blockHeader.type);

                            break;
                        }

                        byte[] data;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_data_block_type_0_at_position_1, entry.dataType,
                                                   entry.offset);

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                   GC.GetTotalMemory(false));

                        // Decompress media tag
                        if(blockHeader.compression is CompressionType.Lzma
                           or CompressionType.LzmaClauniaSubchannelTransform)
                        {
                            if(blockHeader.compression == CompressionType.LzmaClauniaSubchannelTransform &&
                               entry.dataType          != DataType.CdSectorSubchannel)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           Localization.
                                                               Invalid_compression_type_0_for_block_with_data_type_1_continuing,
                                                           blockHeader.compression, entry.dataType);

                                break;
                            }

                            DateTime startDecompress = DateTime.Now;
                            byte[]   compressedTag   = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                            byte[]   lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                            _imageStream.EnsureRead(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                            _imageStream.EnsureRead(compressedTag, 0, compressedTag.Length);
                            data = new byte[blockHeader.length];
                            int decompressedLength = LZMA.DecodeBuffer(compressedTag, data, lzmaProperties);

                            if(decompressedLength != blockHeader.length)
                            {
                                ErrorMessage =
                                    string.
                                        Format(Localization.Error_decompressing_block_should_be_0_bytes_but_got_1_bytes,
                                               blockHeader.length, decompressedLength);

                                return false;
                            }

                            if(blockHeader.compression == CompressionType.LzmaClauniaSubchannelTransform)
                                data = ClauniaSubchannelUntransform(data);

                            DateTime endDecompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Took_0_seconds_to_decompress_block,
                                                       (endDecompress - startDecompress).TotalSeconds);

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                       GC.GetTotalMemory(false));
                        }
                        else if(blockHeader.compression == CompressionType.None)
                        {
                            data = new byte[blockHeader.length];
                            _imageStream.EnsureRead(data, 0, (int)blockHeader.length);

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                       GC.GetTotalMemory(false));
                        }
                        else
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Found_unknown_compression_type_0_continuing,
                                                       (ushort)blockHeader.compression);

                            break;
                        }

                        // Check CRC, if not correct, skip it
                        Crc64Context.Data(data, out byte[] blockCrc);

                        if(BitConverter.ToUInt64(blockCrc, 0) != blockHeader.crc64 &&
                           blockHeader.crc64                  != 0)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Incorrect_CRC_found_0_X16_found_expected_1_X16_continuing,
                                                       BitConverter.ToUInt64(blockCrc, 0), blockHeader.crc64);

                            break;
                        }

                        // Check if it's not a media tag, but a sector tag, and fill the appropriate table then
                        switch(entry.dataType)
                        {
                            case DataType.CdSectorPrefix:
                            case DataType.CdSectorPrefixCorrected:
                                if(entry.dataType == DataType.CdSectorPrefixCorrected)
                                {
                                    _sectorPrefixMs = new MemoryStream();
                                    _sectorPrefixMs.Write(data, 0, data.Length);
                                }
                                else
                                    _sectorPrefix = data;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.CdSectorSuffix:
                            case DataType.CdSectorSuffixCorrected:
                                if(entry.dataType == DataType.CdSectorSuffixCorrected)
                                {
                                    _sectorSuffixMs = new MemoryStream();
                                    _sectorSuffixMs.Write(data, 0, data.Length);
                                }
                                else
                                    _sectorSuffix = data;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.CdSectorSubchannel:
                                _sectorSubchannel = data;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.AppleProfileTag:
                            case DataType.AppleSonyTag:
                            case DataType.PriamDataTowerTag:
                                _sectorSubchannel = data;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.CompactDiscMode2Subheader:
                                _mode2Subheaders = data;

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.DvdSectorCprMai:
                                _sectorCprMai = data;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.DvdSectorCmi))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.DvdSectorCmi);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.DvdSectorTitleKey))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.DvdSectorTitleKey);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.DvdSectorTitleKeyDecrypted:
                                _sectorDecryptedTitleKey = data;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.DvdTitleKeyDecrypted))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.DvdTitleKeyDecrypted);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                            default:
                                MediaTagType mediaTagType = GetMediaTagTypeForDataType(blockHeader.type);

                                if(_mediaTags.ContainsKey(mediaTagType))
                                {
                                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                               Localization.
                                                                   Media_tag_type_0_duplicated_removing_previous_entry,
                                                               mediaTagType);

                                    _mediaTags.Remove(mediaTagType);
                                }

                                _mediaTags.Add(mediaTagType, data);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                           GC.GetTotalMemory(false));

                                break;
                        }

                        break;
                    case BlockType.DeDuplicationTable:
                        // Only user data deduplication tables are used right now
                        if(entry.dataType == DataType.UserData)
                        {
                            _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                            DdtHeader ddtHeader = Marshal.ByteArrayToStructureLittleEndian<DdtHeader>(_structureBytes);

                            if(ddtHeader.identifier != BlockType.DeDuplicationTable)
                                break;

                            if(ddtHeader.entries != _imageInfo.Sectors &&
                               !IsTape)
                            {
                                ErrorMessage =
                                    string.
                                        Format(Localization.Trying_to_write_a_media_with_0_sectors_to_an_image_with_1_sectors_not_continuing,
                                               _imageInfo.Sectors, ddtHeader.entries);

                                return false;
                            }

                            _shift = ddtHeader.shift;

                            switch(ddtHeader.compression)
                            {
                                case CompressionType.Lzma:
                                    AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Decompressing_DDT);

                                    DateTime ddtStart = DateTime.UtcNow;

                                    byte[] compressedDdt = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];

                                    byte[] lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                    _imageStream.EnsureRead(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                    _imageStream.EnsureRead(compressedDdt, 0, compressedDdt.Length);
                                    byte[] decompressedDdt = new byte[ddtHeader.length];

                                    ulong decompressedLength =
                                        (ulong)LZMA.DecodeBuffer(compressedDdt, decompressedDdt, lzmaProperties);

                                    if(decompressedLength != ddtHeader.length)
                                    {
                                        ErrorMessage =
                                            string.
                                                Format(Localization.Error_decompressing_DDT_should_be_0_bytes_but_got_1_bytes,
                                                       ddtHeader.length, decompressedLength);

                                        return false;
                                    }

                                    _userDataDdt = MemoryMarshal.Cast<byte, ulong>(decompressedDdt).ToArray();
                                    DateTime ddtEnd = DateTime.UtcNow;
                                    _inMemoryDdt = true;

                                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                               Localization.Took_0_seconds_to_decompress_DDT,
                                                               (ddtEnd - ddtStart).TotalSeconds);

                                    break;
                                case CompressionType.None:
                                    _inMemoryDdt          = false;
                                    _outMemoryDdtPosition = (long)entry.offset;

                                    break;
                                default:
                                    ErrorMessage = string.Format(Localization.Found_unsupported_compression_algorithm_0,
                                                                 (ushort)ddtHeader.compression);

                                    return false;
                            }

                            if(IsTape)
                            {
                                _tapeDdt = new Dictionary<ulong, ulong>();

                                for(long i = 0; i < _userDataDdt.LongLength; i++)
                                    _tapeDdt.Add((ulong)i, _userDataDdt[i]);

                                _userDataDdt = null;
                            }

                            foundUserDataDdt = true;
                        }
                        else if(entry.dataType is DataType.CdSectorPrefixCorrected or DataType.CdSectorSuffixCorrected)
                        {
                            _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                            DdtHeader ddtHeader = Marshal.ByteArrayToStructureLittleEndian<DdtHeader>(_structureBytes);

                            if(ddtHeader.identifier != BlockType.DeDuplicationTable)
                                break;

                            if(ddtHeader.entries != _imageInfo.Sectors)
                            {
                                ErrorMessage =
                                    string.
                                        Format(Localization.Trying_to_write_a_media_with_0_sectors_to_an_image_with_1_sectors_not_continuing,
                                               _imageInfo.Sectors, ddtHeader.entries);

                                return false;
                            }

                            byte[] decompressedDdt = new byte[ddtHeader.length];

                            switch(ddtHeader.compression)
                            {
                                case CompressionType.Lzma:
                                    AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Decompressing_DDT);

                                    DateTime ddtStart = DateTime.UtcNow;

                                    byte[] compressedDdt = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];

                                    byte[] lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                    _imageStream.EnsureRead(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                    _imageStream.EnsureRead(compressedDdt, 0, compressedDdt.Length);

                                    ulong decompressedLength =
                                        (ulong)LZMA.DecodeBuffer(compressedDdt, decompressedDdt, lzmaProperties);

                                    if(decompressedLength != ddtHeader.length)
                                    {
                                        ErrorMessage =
                                            string.
                                                Format(Localization.Error_decompressing_DDT_should_be_0_bytes_but_got_1_bytes,
                                                       ddtHeader.length, decompressedLength);

                                        return false;
                                    }

                                    DateTime ddtEnd = DateTime.UtcNow;

                                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                               Localization.Took_0_seconds_to_decompress_DDT,
                                                               (ddtEnd - ddtStart).TotalSeconds);

                                    break;
                                case CompressionType.None:
                                    _imageStream.EnsureRead(decompressedDdt, 0, decompressedDdt.Length);

                                    break;
                                default:
                                    ErrorMessage = string.Format(Localization.Found_unsupported_compression_algorithm_0,
                                                                 (ushort)ddtHeader.compression);

                                    return false;
                            }

                            uint[] cdDdt = MemoryMarshal.Cast<byte, uint>(decompressedDdt).ToArray();

                            switch(entry.dataType)
                            {
                                case DataType.CdSectorPrefixCorrected:
                                    _sectorPrefixDdt = cdDdt;

                                    break;
                                case DataType.CdSectorSuffixCorrected:
                                    _sectorSuffixDdt = cdDdt;

                                    break;
                            }
                        }

                        break;

                    // Logical geometry block. It doesn't have a CRC coz, well, it's not so important
                    case BlockType.GeometryBlock:
                        _structureBytes = new byte[Marshal.SizeOf<GeometryBlock>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                        _geometryBlock = Marshal.SpanToStructureLittleEndian<GeometryBlock>(_structureBytes);

                        if(_geometryBlock.identifier == BlockType.GeometryBlock)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Geometry_set_to_0_cylinders_1_heads_2_sectors_per_track,
                                                       _geometryBlock.cylinders, _geometryBlock.heads,
                                                       _geometryBlock.sectorsPerTrack);

                            _imageInfo.Cylinders       = _geometryBlock.cylinders;
                            _imageInfo.Heads           = _geometryBlock.heads;
                            _imageInfo.SectorsPerTrack = _geometryBlock.sectorsPerTrack;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                       GC.GetTotalMemory(false));
                        }

                        break;

                    // Metadata block
                    case BlockType.MetadataBlock:
                        _structureBytes = new byte[Marshal.SizeOf<MetadataBlock>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        MetadataBlock metadataBlock =
                            Marshal.SpanToStructureLittleEndian<MetadataBlock>(_structureBytes);

                        if(metadataBlock.identifier != entry.blockType)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Incorrect_identifier_for_data_block_at_position_0,
                                                       entry.offset);

                            break;
                        }

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_metadata_block_at_position_0, entry.offset);

                        byte[] metadata = new byte[metadataBlock.blockSize];
                        _imageStream.Position = (long)entry.offset;
                        _imageStream.EnsureRead(metadata, 0, metadata.Length);

                        if(metadataBlock is { mediaSequence: > 0, lastMediaSequence: > 0 })
                        {
                            _imageInfo.MediaSequence     = metadataBlock.mediaSequence;
                            _imageInfo.LastMediaSequence = metadataBlock.lastMediaSequence;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Setting_media_sequence_as_0_of_1,
                                                       _imageInfo.MediaSequence, _imageInfo.LastMediaSequence);
                        }

                        if(metadataBlock.creatorLength                               > 0 &&
                           metadataBlock.creatorLength + metadataBlock.creatorOffset <= metadata.Length)
                        {
                            _imageInfo.Creator =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.creatorOffset,
                                                           (int)(metadataBlock.creatorLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_creator_0,
                                                       _imageInfo.Creator);
                        }

                        if(metadataBlock.commentsOffset                                > 0 &&
                           metadataBlock.commentsLength + metadataBlock.commentsOffset <= metadata.Length)
                        {
                            _imageInfo.Comments =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.commentsOffset,
                                                           (int)(metadataBlock.commentsLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_comments_0,
                                                       _imageInfo.Comments);
                        }

                        if(metadataBlock.mediaTitleOffset                                  > 0 &&
                           metadataBlock.mediaTitleLength + metadataBlock.mediaTitleOffset <= metadata.Length)
                        {
                            _imageInfo.MediaTitle =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaTitleOffset,
                                                           (int)(metadataBlock.mediaTitleLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_media_title_0,
                                                       _imageInfo.MediaTitle);
                        }

                        if(metadataBlock.mediaManufacturerOffset > 0 &&
                           metadataBlock.mediaManufacturerLength + metadataBlock.mediaManufacturerOffset <=
                           metadata.Length)
                        {
                            _imageInfo.MediaManufacturer =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaManufacturerOffset,
                                                           (int)(metadataBlock.mediaManufacturerLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_media_manufacturer_0,
                                                       _imageInfo.MediaManufacturer);
                        }

                        if(metadataBlock.mediaModelOffset                                  > 0 &&
                           metadataBlock.mediaModelLength + metadataBlock.mediaModelOffset <= metadata.Length)
                        {
                            _imageInfo.MediaModel =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaModelOffset,
                                                           (int)(metadataBlock.mediaModelLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_media_model_0,
                                                       _imageInfo.MediaModel);
                        }

                        if(metadataBlock.mediaSerialNumberOffset > 0 &&
                           metadataBlock.mediaSerialNumberLength + metadataBlock.mediaSerialNumberOffset <=
                           metadata.Length)
                        {
                            _imageInfo.MediaSerialNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaSerialNumberOffset,
                                                           (int)(metadataBlock.mediaSerialNumberLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_media_serial_number_0,
                                                       _imageInfo.MediaSerialNumber);
                        }

                        if(metadataBlock.mediaBarcodeOffset                                    > 0 &&
                           metadataBlock.mediaBarcodeLength + metadataBlock.mediaBarcodeOffset <= metadata.Length)
                        {
                            _imageInfo.MediaBarcode =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaBarcodeOffset,
                                                           (int)(metadataBlock.mediaBarcodeLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_media_barcode_0,
                                                       _imageInfo.MediaBarcode);
                        }

                        if(metadataBlock.mediaPartNumberOffset                                       > 0 &&
                           metadataBlock.mediaPartNumberLength + metadataBlock.mediaPartNumberOffset <= metadata.Length)
                        {
                            _imageInfo.MediaPartNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaPartNumberOffset,
                                                           (int)(metadataBlock.mediaPartNumberLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_media_part_number_0,
                                                       _imageInfo.MediaPartNumber);
                        }

                        if(metadataBlock.driveManufacturerOffset > 0 &&
                           metadataBlock.driveManufacturerLength + metadataBlock.driveManufacturerOffset <=
                           metadata.Length)
                        {
                            _imageInfo.DriveManufacturer =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveManufacturerOffset,
                                                           (int)(metadataBlock.driveManufacturerLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_drive_manufacturer_0,
                                                       _imageInfo.DriveManufacturer);
                        }

                        if(metadataBlock.driveModelOffset                                  > 0 &&
                           metadataBlock.driveModelLength + metadataBlock.driveModelOffset <= metadata.Length)
                        {
                            _imageInfo.DriveModel =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveModelOffset,
                                                           (int)(metadataBlock.driveModelLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_drive_model_0,
                                                       _imageInfo.DriveModel);
                        }

                        if(metadataBlock.driveSerialNumberOffset > 0 &&
                           metadataBlock.driveSerialNumberLength + metadataBlock.driveSerialNumberOffset <=
                           metadata.Length)
                        {
                            _imageInfo.DriveSerialNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveSerialNumberOffset,
                                                           (int)(metadataBlock.driveSerialNumberLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Setting_drive_serial_number_0,
                                                       _imageInfo.DriveSerialNumber);
                        }

                        if(metadataBlock.driveFirmwareRevisionOffset > 0 &&
                           metadataBlock.driveFirmwareRevisionLength + metadataBlock.driveFirmwareRevisionOffset <=
                           metadata.Length)
                        {
                            _imageInfo.DriveFirmwareRevision =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveFirmwareRevisionOffset,
                                                           (int)(metadataBlock.driveFirmwareRevisionLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Setting_drive_firmware_revision_0,
                                                       _imageInfo.DriveFirmwareRevision);
                        }

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                   GC.GetTotalMemory(false));

                        break;

                    // Optical disc tracks block
                    case BlockType.TracksBlock:
                        _structureBytes = new byte[Marshal.SizeOf<TracksHeader>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        TracksHeader tracksHeader = Marshal.SpanToStructureLittleEndian<TracksHeader>(_structureBytes);

                        if(tracksHeader.identifier != BlockType.TracksBlock)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Incorrect_identifier_for_tracks_block_at_position_0,
                                                       entry.offset);

                            break;
                        }

                        _structureBytes = new byte[Marshal.SizeOf<TrackEntry>() * tracksHeader.entries];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                        Crc64Context.Data(_structureBytes, out byte[] trksCrc);

                        if(BitConverter.ToUInt64(trksCrc, 0) != tracksHeader.crc64)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Incorrect_CRC_found_0_X16_found_expected_1_X16_continuing,
                                                       BitConverter.ToUInt64(trksCrc, 0), tracksHeader.crc64);

                            break;
                        }

                        _imageStream.Position -= _structureBytes.Length;

                        Tracks      = new List<Track>();
                        _trackFlags = new Dictionary<byte, byte>();
                        _trackIsrcs = new Dictionary<byte, string>();

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Found_0_tracks_at_position_0,
                                                   tracksHeader.entries, entry.offset);

                        for(ushort i = 0; i < tracksHeader.entries; i++)
                        {
                            _structureBytes = new byte[Marshal.SizeOf<TrackEntry>()];
                            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                            TrackEntry trackEntry =
                                Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(_structureBytes);

                            Tracks.Add(new Track
                            {
                                Sequence    = trackEntry.sequence,
                                Type        = trackEntry.type,
                                StartSector = (ulong)trackEntry.start,
                                EndSector   = (ulong)trackEntry.end,
                                Pregap      = (ulong)trackEntry.pregap,
                                Session     = trackEntry.session,
                                FileType    = "BINARY"
                            });

                            if(trackEntry.type == TrackType.Data)
                                continue;

                            _trackFlags.Add(trackEntry.sequence, trackEntry.flags);

                            if(!string.IsNullOrEmpty(trackEntry.isrc))
                                _trackIsrcs.Add(trackEntry.sequence, trackEntry.isrc);
                        }

                        if(_trackFlags.Count > 0 &&
                           !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                        if(_trackIsrcs.Count > 0 &&
                           !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                        _imageInfo.HasPartitions = true;
                        _imageInfo.HasSessions   = true;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                   GC.GetTotalMemory(false));

                        break;

                    // CICM XML metadata block
                    case BlockType.CicmBlock:
                        _structureBytes = new byte[Marshal.SizeOf<CicmMetadataBlock>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        CicmMetadataBlock cicmBlock =
                            Marshal.SpanToStructureLittleEndian<CicmMetadataBlock>(_structureBytes);

                        if(cicmBlock.identifier != BlockType.CicmBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_CICM_XML_metadata_block_at_position_0,
                                                   entry.offset);

                        byte[] cicmBytes = new byte[cicmBlock.length];
                        _imageStream.EnsureRead(cicmBytes, 0, cicmBytes.Length);
                        var cicmMs = new MemoryStream(cicmBytes);

                        // The converter to AaruMetadata basically overcomes this (should?)
                        #pragma warning disable IL2026
                        var cicmXs = new XmlSerializer(typeof(CICMMetadataType));
                        #pragma warning restore IL2026

                        try
                        {
                            var sr = new StreamReader(cicmMs);

                            // The converter to AaruMetadata basically overcomes this (should?)
                            #pragma warning disable IL2026
                            AaruMetadata = (CICMMetadataType)cicmXs.Deserialize(sr);
                            #pragma warning restore IL2026
                            sr.Close();
                        }
                        catch(XmlException ex)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Exception_0_processing_CICM_XML_metadata_block,
                                                       ex.Message);

                            AaruMetadata = null;
                        }

                        break;

                    // Aaru Metadata JSON block
                    case BlockType.AaruMetadataJsonBlock:
                        _structureBytes = new byte[Marshal.SizeOf<AaruMetadataJsonBlock>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        AaruMetadataJsonBlock aaruMetadataBlock =
                            Marshal.SpanToStructureLittleEndian<AaruMetadataJsonBlock>(_structureBytes);

                        if(aaruMetadataBlock.identifier != BlockType.AaruMetadataJsonBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_Aaru_Metadata_block_at_position_0, entry.offset);

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                   GC.GetTotalMemory(false));

                        byte[] jsonBytes = new byte[aaruMetadataBlock.length];
                        _imageStream.EnsureRead(jsonBytes, 0, jsonBytes.Length);

                        try
                        {
                            AaruMetadata =
                                (JsonSerializer.Deserialize(jsonBytes, typeof(MetadataJson),
                                                            MetadataJsonContext.Default) as MetadataJson)?.AaruMetadata;
                        }
                        catch(JsonException ex)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.Exception_0_processing_Aaru_Metadata_block,
                                                       ex.Message);

                            AaruMetadata = null;
                        }

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                   GC.GetTotalMemory(false));

                        break;

                    // Dump hardware block
                    case BlockType.DumpHardwareBlock:
                        _structureBytes = new byte[Marshal.SizeOf<DumpHardwareHeader>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        DumpHardwareHeader dumpBlock =
                            Marshal.SpanToStructureLittleEndian<DumpHardwareHeader>(_structureBytes);

                        if(dumpBlock.identifier != BlockType.DumpHardwareBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_dump_hardware_block_at_position_0, entry.offset);

                        _structureBytes = new byte[dumpBlock.length];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                        Crc64Context.Data(_structureBytes, out byte[] dumpCrc);

                        if(BitConverter.ToUInt64(dumpCrc, 0) != dumpBlock.crc64)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Incorrect_CRC_found_0_X16_found_expected_1_X16_continuing,
                                                       BitConverter.ToUInt64(dumpCrc, 0), dumpBlock.crc64);

                            break;
                        }

                        _imageStream.Position -= _structureBytes.Length;

                        DumpHardware = new List<DumpHardware>();

                        for(ushort i = 0; i < dumpBlock.entries; i++)
                        {
                            _structureBytes = new byte[Marshal.SizeOf<DumpHardwareEntry>()];
                            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                            DumpHardwareEntry dumpEntry =
                                Marshal.SpanToStructureLittleEndian<DumpHardwareEntry>(_structureBytes);

                            var dump = new DumpHardware
                            {
                                Software = new Software(),
                                Extents  = new List<Extent>()
                            };

                            byte[] tmp;

                            if(dumpEntry.manufacturerLength > 0)
                            {
                                tmp = new byte[dumpEntry.manufacturerLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Manufacturer     =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.modelLength > 0)
                            {
                                tmp = new byte[dumpEntry.modelLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Model            =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.revisionLength > 0)
                            {
                                tmp = new byte[dumpEntry.revisionLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Revision         =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.firmwareLength > 0)
                            {
                                tmp = new byte[dumpEntry.firmwareLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Firmware         =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.serialLength > 0)
                            {
                                tmp = new byte[dumpEntry.serialLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Serial           =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.softwareNameLength > 0)
                            {
                                tmp = new byte[dumpEntry.softwareNameLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Software.Name    =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.softwareVersionLength > 0)
                            {
                                tmp = new byte[dumpEntry.softwareVersionLength - 1];
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                _imageStream.Position += 1;
                                dump.Software.Version =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.softwareOperatingSystemLength > 0)
                            {
                                tmp                   =  new byte[dumpEntry.softwareOperatingSystemLength - 1];
                                _imageStream.Position += 1;
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);
                                dump.Software.OperatingSystem = Encoding.UTF8.GetString(tmp);
                            }

                            tmp = new byte[16];

                            for(uint j = 0; j < dumpEntry.extents; j++)
                            {
                                _imageStream.EnsureRead(tmp, 0, tmp.Length);

                                dump.Extents.Add(new Extent
                                {
                                    Start = BitConverter.ToUInt64(tmp, 0),
                                    End   = BitConverter.ToUInt64(tmp, 8)
                                });
                            }

                            dump.Extents = dump.Extents.OrderBy(t => t.Start).ToList();

                            if(dump.Extents.Count > 0)
                                DumpHardware.Add(dump);
                        }

                        if(DumpHardware.Count == 0)
                            DumpHardware = null;

                        break;

                    // Tape partition block
                    case BlockType.TapePartitionBlock:
                        _structureBytes = new byte[Marshal.SizeOf<TapePartitionHeader>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        TapePartitionHeader partitionHeader =
                            Marshal.SpanToStructureLittleEndian<TapePartitionHeader>(_structureBytes);

                        if(partitionHeader.identifier != BlockType.TapePartitionBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_tape_partition_block_at_position_0, entry.offset);

                        byte[] tapePartitionBytes = new byte[partitionHeader.length];
                        _imageStream.EnsureRead(tapePartitionBytes, 0, tapePartitionBytes.Length);

                        Span<TapePartitionEntry> tapePartitions =
                            MemoryMarshal.Cast<byte, TapePartitionEntry>(tapePartitionBytes);

                        TapePartitions = new List<TapePartition>();

                        foreach(TapePartitionEntry tapePartition in tapePartitions)
                            TapePartitions.Add(new TapePartition
                            {
                                FirstBlock = tapePartition.FirstBlock,
                                LastBlock  = tapePartition.LastBlock,
                                Number     = tapePartition.Number
                            });

                        IsTape = true;

                        break;

                    // Tape file block
                    case BlockType.TapeFileBlock:
                        _structureBytes = new byte[Marshal.SizeOf<TapeFileHeader>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        TapeFileHeader fileHeader =
                            Marshal.SpanToStructureLittleEndian<TapeFileHeader>(_structureBytes);

                        if(fileHeader.identifier != BlockType.TapeFileBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_tape_file_block_at_position_0, entry.offset);

                        byte[] tapeFileBytes = new byte[fileHeader.length];
                        _imageStream.EnsureRead(tapeFileBytes, 0, tapeFileBytes.Length);
                        Span<TapeFileEntry> tapeFiles = MemoryMarshal.Cast<byte, TapeFileEntry>(tapeFileBytes);
                        Files = new List<TapeFile>();

                        foreach(TapeFileEntry file in tapeFiles)
                            Files.Add(new TapeFile
                            {
                                FirstBlock = file.FirstBlock,
                                LastBlock  = file.LastBlock,
                                Partition  = file.Partition,
                                File       = file.File
                            });

                        IsTape = true;

                        break;

                    // Optical disc tracks block
                    case BlockType.CompactDiscIndexesBlock:
                        _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexesHeader>()];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                        CompactDiscIndexesHeader indexesHeader =
                            Marshal.SpanToStructureLittleEndian<CompactDiscIndexesHeader>(_structureBytes);

                        if(indexesHeader.identifier != BlockType.CompactDiscIndexesBlock)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Incorrect_identifier_for_compact_disc_indexes_block_at_position_0,
                                                       entry.offset);

                            break;
                        }

                        _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexEntry>() * indexesHeader.entries];
                        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                        Crc64Context.Data(_structureBytes, out byte[] idsxCrc);

                        if(BitConverter.ToUInt64(idsxCrc, 0) != indexesHeader.crc64)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       Localization.
                                                           Incorrect_CRC_found_0_X16_found_expected_1_X16_continuing,
                                                       BitConverter.ToUInt64(idsxCrc, 0), indexesHeader.crc64);

                            break;
                        }

                        _imageStream.Position -= _structureBytes.Length;

                        compactDiscIndexes = new List<CompactDiscIndexEntry>();

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Found_0_compact_disc_indexes_at_position_0,
                                                   indexesHeader.entries, entry.offset);

                        for(ushort i = 0; i < indexesHeader.entries; i++)
                        {
                            _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexEntry>()];
                            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);

                            compactDiscIndexes.Add(Marshal.
                                                       ByteArrayToStructureLittleEndian<
                                                           CompactDiscIndexEntry>(_structureBytes));
                        }

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                                   GC.GetTotalMemory(false));

                        break;
                }
            }

            if(!foundUserDataDdt)
            {
                ErrorMessage = Localization.Could_not_find_user_data_deduplication_table;

                return false;
            }

            if(_sectorSuffixMs  == null ||
               _sectorSuffixDdt == null ||
               _sectorPrefixMs  == null ||
               _sectorPrefixDdt == null)
            {
                _sectorSuffixMs  = null;
                _sectorSuffixDdt = null;
                _sectorPrefixMs  = null;
                _sectorPrefixDdt = null;
            }

            if(!_inMemoryDdt)
                _ddtEntryCache = new Dictionary<ulong, ulong>();

            // Initialize tracks, sessions and partitions
            if(_imageInfo.MetadataMediaType == MetadataMediaType.OpticalDisc)
            {
                if(Tracks != null &&
                   _mediaTags.TryGetValue(MediaTagType.CD_FullTOC, out byte[] fullToc))
                {
                    byte[] tmp = new byte[fullToc.Length + 2];
                    Array.Copy(fullToc, 0, tmp, 2, fullToc.Length);
                    tmp[0] = (byte)(fullToc.Length >> 8);
                    tmp[1] = (byte)(fullToc.Length & 0xFF);

                    FullTOC.CDFullTOC? decodedFullToc = FullTOC.Decode(tmp);

                    if(decodedFullToc.HasValue)
                    {
                        Dictionary<int, long> leadOutStarts = new(); // Lead-out starts

                        foreach(FullTOC.TrackDataDescriptor trk in
                                decodedFullToc.Value.TrackDescriptors.Where(trk => trk.ADR is 1 or 4 &&
                                                                                trk.POINT == 0xA2))
                        {
                            int phour, pmin, psec, pframe;

                            if(trk.PFRAME == 0)
                            {
                                pframe = 74;

                                if(trk.PSEC == 0)
                                {
                                    psec = 59;

                                    if(trk.PMIN == 0)
                                    {
                                        pmin  = 59;
                                        phour = trk.PHOUR - 1;
                                    }
                                    else
                                    {
                                        pmin  = trk.PMIN - 1;
                                        phour = trk.PHOUR;
                                    }
                                }
                                else
                                {
                                    psec  = trk.PSEC - 1;
                                    pmin  = trk.PMIN;
                                    phour = trk.PHOUR;
                                }
                            }
                            else
                            {
                                pframe = trk.PFRAME - 1;
                                psec   = trk.PSEC;
                                pmin   = trk.PMIN;
                                phour  = trk.PHOUR;
                            }

                            int lastSector = (phour * 3600 * 75) + (pmin * 60 * 75) + (psec * 75) + pframe - 150;
                            leadOutStarts?.Add(trk.SessionNumber, lastSector                               + 1);
                        }

                        foreach(KeyValuePair<int, long> leadOuts in leadOutStarts)
                        {
                            var lastTrackInSession = new Track();

                            foreach(Track trk in Tracks.Where(trk => trk.Session  == leadOuts.Key).
                                                        Where(trk => trk.Sequence > lastTrackInSession.Sequence))
                                lastTrackInSession = trk;

                            if(lastTrackInSession.Sequence  == 0 ||
                               lastTrackInSession.EndSector == (ulong)leadOuts.Value - 1)
                                continue;

                            lastTrackInSession.EndSector = (ulong)leadOuts.Value - 1;
                        }
                    }
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                           GC.GetTotalMemory(false));

                if(Tracks       == null ||
                   Tracks.Count == 0)
                {
                    Tracks = new List<Track>
                    {
                        new()
                        {
                            BytesPerSector    = (int)_imageInfo.SectorSize,
                            EndSector         = _imageInfo.Sectors - 1,
                            FileType          = "BINARY",
                            RawBytesPerSector = (int)_imageInfo.SectorSize,
                            Session           = 1,
                            Sequence          = 1,
                            Type              = TrackType.Data
                        }
                    };

                    _trackFlags = new Dictionary<byte, byte>
                    {
                        {
                            1, (byte)CdFlags.DataTrack
                        }
                    };

                    _trackIsrcs = new Dictionary<byte, string>();
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                           GC.GetTotalMemory(false));

                Sessions = new List<Session>();

                for(int i = 1; i <= Tracks.Max(t => t.Session); i++)
                    Sessions.Add(new Session
                    {
                        Sequence    = (ushort)i,
                        StartTrack  = Tracks.Where(t => t.Session == i).Min(t => t.Sequence),
                        EndTrack    = Tracks.Where(t => t.Session == i).Max(t => t.Sequence),
                        StartSector = Tracks.Where(t => t.Session == i).Min(t => t.StartSector),
                        EndSector   = Tracks.Where(t => t.Session == i).Max(t => t.EndSector)
                    });

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                           GC.GetTotalMemory(false));

                foreach(Track track in Tracks.OrderBy(t => t.StartSector))
                {
                    if(track.Sequence == 1)
                    {
                        track.Pregap     = 150;
                        track.Indexes[0] = -150;
                        track.Indexes[1] = (int)track.StartSector;

                        continue;
                    }

                    if(track.Pregap > 0)
                    {
                        track.Indexes[0] = (int)track.StartSector;
                        track.Indexes[1] = (int)(track.StartSector + track.Pregap);
                    }
                    else
                        track.Indexes[1] = (int)track.StartSector;
                }

                ulong currentTrackOffset = 0;
                Partitions = new List<Partition>();

                foreach(Track track in Tracks.OrderBy(t => t.StartSector))
                {
                    Partitions.Add(new Partition
                    {
                        Sequence = track.Sequence,
                        Type     = track.Type.ToString(),
                        Name     = string.Format(Localization.Track_0, track.Sequence),
                        Offset   = currentTrackOffset,
                        Start    = (ulong)track.Indexes[1],
                        Size     = (track.EndSector - (ulong)track.Indexes[1] + 1) * (ulong)track.BytesPerSector,
                        Length   = track.EndSector - (ulong)track.Indexes[1] + 1,
                        Scheme   = Localization.Optical_disc_track
                    });

                    currentTrackOffset += (track.EndSector - track.StartSector + 1) * (ulong)track.BytesPerSector;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                           GC.GetTotalMemory(false));

                Track[] tracks = Tracks.ToArray();

                foreach(Track trk in tracks)
                {
                    ErrorNumber errno = ReadSector(trk.StartSector, out byte[] sector);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    trk.BytesPerSector = sector.Length;

                    trk.RawBytesPerSector =
                        (_sectorPrefix    != null && _sectorSuffix    != null) ||
                        (_sectorPrefixDdt != null && _sectorSuffixDdt != null) ? 2352 : sector.Length;

                    if(_sectorSubchannel == null)
                        continue;

                    trk.SubchannelFile   = trk.File;
                    trk.SubchannelFilter = trk.Filter;
                    trk.SubchannelType   = TrackSubchannelType.Raw;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                           GC.GetTotalMemory(false));

                Tracks = tracks.ToList();

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Memory_snapshot_0_bytes,
                                           GC.GetTotalMemory(false));

                if(compactDiscIndexes != null)
                    foreach(CompactDiscIndexEntry compactDiscIndex in compactDiscIndexes.OrderBy(i => i.Track).
                                ThenBy(i => i.Index))
                    {
                        Track track = Tracks.FirstOrDefault(t => t.Sequence == compactDiscIndex.Track);

                        if(track is null)
                            continue;

                        track.Indexes[compactDiscIndex.Index] = compactDiscIndex.Lba;
                    }
            }
            else
            {
                Tracks     = null;
                Sessions   = null;
                Partitions = null;
            }
        }

        // Creating new
        else
        {
            // Checking that DDT is smaller than requested size
            _inMemoryDdt = sectors <= maxDdtSize * 1024 * 1024 / sizeof(ulong);

            // If in memory, easy
            if(_inMemoryDdt)
            {
                if(IsTape)
                    _tapeDdt = new Dictionary<ulong, ulong>();
                else
                    _userDataDdt = new ulong[sectors];
            }

            // If not, create the block, add to index, and enlarge the file to allow the DDT to exist on-disk
            else
            {
                _outMemoryDdtPosition = _imageStream.Position;

                _index.Add(new IndexEntry
                {
                    blockType = BlockType.DeDuplicationTable,
                    dataType  = DataType.UserData,
                    offset    = (ulong)_outMemoryDdtPosition
                });

                // CRC64 will be calculated later
                var ddtHeader = new DdtHeader
                {
                    identifier  = BlockType.DeDuplicationTable,
                    type        = DataType.UserData,
                    compression = CompressionType.None,
                    shift       = _shift,
                    entries     = sectors,
                    cmpLength   = sectors * sizeof(ulong),
                    length      = sectors * sizeof(ulong)
                };

                _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                MemoryMarshal.Write(_structureBytes, in ddtHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _structureBytes = null;

                _imageStream.Position += (long)(sectors * sizeof(ulong)) - 1;
                _imageStream.WriteByte(0);
            }

            if(doMd5)
                _md5Provider = new Md5Context();

            if(doSha1)
                _sha1Provider = new Sha1Context();

            if(doSha256)
                _sha256Provider = new Sha256Context();

            if(doSpamsum)
                _spamsumProvider = new SpamSumContext();
        }

        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.In_memory_DDT_0, _inMemoryDdt);

        _imageStream.Seek(0, SeekOrigin.End);

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(_mediaTags.ContainsKey(tag))
            _mediaTags.Remove(tag);

        _mediaTags.Add(tag, data);

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(sectorAddress >= Info.Sectors &&
           !IsTape)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        if((_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc || !_writingLong) &&
           !_rewinded)
        {
            if(sectorAddress <= _lastWrittenBlock && _alreadyWrittenZero)
            {
                _rewinded        = true;
                _md5Provider     = null;
                _sha1Provider    = null;
                _sha256Provider  = null;
                _spamsumProvider = null;
            }

            _md5Provider?.Update(data);
            _sha1Provider?.Update(data);
            _sha256Provider?.Update(data);
            _spamsumProvider?.Update(data);
            _lastWrittenBlock = sectorAddress;
        }

        if(sectorAddress == 0)
            _alreadyWrittenZero = true;

        byte[] hash = null;
        _writtenSectors++;

        // Compute hash only if asked to deduplicate, or the sector is empty (those will always be deduplicated)
        if(_deduplicate || ArrayHelpers.ArrayIsNullOrEmpty(data))
            hash = _checksumProvider.ComputeHash(data);

        string hashString = null;

        if(hash != null)
        {
            var hashSb = new StringBuilder();

            foreach(byte h in hash)
                hashSb.Append(h.ToString("x2"));

            hashString = hashSb.ToString();

            if(_deduplicationTable.TryGetValue(hashString, out ulong pointer))
            {
                SetDdtEntry(sectorAddress, pointer);
                ErrorMessage = "";

                return true;
            }
        }

        var trk = new Track();

        // If optical disc check track
        if(_imageInfo.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector) ??
                  new Track();

            if(trk.Sequence    == 0 &&
               trk.StartSector == 0 &&
               trk.EndSector   == 0)
                trk.Type = TrackType.Data; // TODO: Check intersession data type
        }

        // Close current block first
        if(_writingBuffer != null &&

           // When sector siz changes
           (_currentBlockHeader.sectorSize != data.Length ||

            // When block if filled
            _currentBlockOffset == 1 << _shift ||

            // When we change to/from CompactDisc audio
            (_currentBlockHeader.compression == CompressionType.Flac && trk.Type != TrackType.Audio)))
        {
            _currentBlockHeader.length = _currentBlockOffset * _currentBlockHeader.sectorSize;
            _currentBlockHeader.crc64  = BitConverter.ToUInt64(_crc64.Final(), 0);

            var cmpCrc64Context = new Crc64Context();

            byte[] lzmaProperties   = Array.Empty<byte>();
            int    compressedLength = 0;

            switch(_currentBlockHeader.compression)
            {
                case CompressionType.Flac:
                {
                    uint currentSamples = _currentBlockOffset * SAMPLES_PER_SECTOR;
                    uint flacBlockSize  = _currentBlockOffset * SAMPLES_PER_SECTOR;

                    if(flacBlockSize > MAX_FLAKE_BLOCK)
                        flacBlockSize = MAX_FLAKE_BLOCK;

                    if(flacBlockSize < MIN_FLAKE_BLOCK)
                        flacBlockSize = MIN_FLAKE_BLOCK;

                    long remaining = currentSamples % flacBlockSize;

                    // Fill FLAC block
                    if(remaining != 0)
                        for(int r = 0; r < remaining * 4; r++)
                            _writingBuffer[_writingBufferPosition + r] = 0;

                    compressedLength = FLAC.EncodeBuffer(_writingBuffer, _compressedBuffer, flacBlockSize, true, false,
                                                         "hamming", 12, 15, true, false, 0, 8, "Aaru");

                    if(compressedLength >= _writingBufferPosition)
                        _currentBlockHeader.compression = CompressionType.None;

                    break;
                }
                case CompressionType.Lzma:
                {
                    compressedLength = LZMA.EncodeBuffer(_writingBuffer, _compressedBuffer, out lzmaProperties, 9,
                                                         _dictionarySize, 4, 0, 2, 273);

                    cmpCrc64Context.Update(lzmaProperties);

                    if(compressedLength >= _writingBufferPosition)
                        _currentBlockHeader.compression = CompressionType.None;

                    break;
                }
                case CompressionType.None: break; // Do nothing
                default:                   throw new ArgumentOutOfRangeException();
            }

            if(_currentBlockHeader.compression == CompressionType.None)
            {
                _currentBlockHeader.cmpCrc64  = _currentBlockHeader.crc64;
                _currentBlockHeader.cmpLength = (uint)_writingBufferPosition;
            }
            else
            {
                cmpCrc64Context.Update(_compressedBuffer, (uint)compressedLength);
                _currentBlockHeader.cmpCrc64  = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);
                _currentBlockHeader.cmpLength = (uint)compressedLength;
            }

            if(_currentBlockHeader.compression == CompressionType.Lzma)
                _currentBlockHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

            _index.Add(new IndexEntry
            {
                blockType = BlockType.DataBlock,
                dataType  = DataType.UserData,
                offset    = (ulong)_imageStream.Position
            });

            _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
            MemoryMarshal.Write(_structureBytes, in _currentBlockHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _structureBytes = null;

            if(_currentBlockHeader.compression == CompressionType.Lzma)
                _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

            if(_currentBlockHeader.compression == CompressionType.None)
                _imageStream.Write(_writingBuffer, 0, _writingBufferPosition);
            else
                _imageStream.Write(_compressedBuffer, 0, compressedLength);

            _writingBufferPosition = 0;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
            _currentBlockOffset = 0;
        }

        // No block set
        if(_writingBufferPosition == 0)
        {
            _currentBlockHeader = new BlockHeader
            {
                identifier  = BlockType.DataBlock,
                type        = DataType.UserData,
                compression = _compress ? _compressionAlgorithm : CompressionType.None,
                sectorSize  = (uint)data.Length
            };

            if(_imageInfo.MetadataMediaType == MetadataMediaType.OpticalDisc &&
               trk.Type                     == TrackType.Audio               &&
               _compress)
                _currentBlockHeader.compression = CompressionType.Flac;

            // JaguarCD stores data in audio tracks. FLAC is too inefficient, use LZMA there.
            // VideoNow stores video in audio tracks, and LZMA works better too.
            if(((_imageInfo.MediaType == MediaType.JaguarCD && trk.Session > 1) ||
                _imageInfo.MediaType is MediaType.VideoNow or MediaType.VideoNowColor or MediaType.VideoNowXp) &&
               trk.Type == TrackType.Audio                                                                     &&
               _compress                                                                                       &&
               _currentBlockHeader.compression == CompressionType.Flac)
                _currentBlockHeader.compression = CompressionType.Lzma;

            int maxBufferSize = ((1 << _shift) * data.Length) + (MAX_FLAKE_BLOCK * 4);

            if(_writingBuffer        == null ||
               _writingBuffer.Length < maxBufferSize)
            {
                _writingBuffer    = new byte[maxBufferSize];
                _compressedBuffer = new byte[maxBufferSize * 2];
            }

            _writingBufferPosition = 0;
            _crc64                 = new Crc64Context();
        }

        ulong ddtEntry = (ulong)((_imageStream.Position << _shift) + _currentBlockOffset);

        if(hash != null)
            _deduplicationTable.Add(hashString, ddtEntry);

        Array.Copy(data, 0, _writingBuffer, _writingBufferPosition, data.Length);
        _writingBufferPosition += data.Length;

        SetDdtEntry(sectorAddress, ddtEntry);
        _crc64.Update(data);
        _currentBlockOffset++;

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(sectorAddress + length > Info.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        uint sectorSize = (uint)(data.Length / length);

        for(uint i = 0; i < length; i++)
        {
            byte[] tmp = new byte[sectorSize];
            Array.Copy(data, i * sectorSize, tmp, 0, sectorSize);

            if(!WriteSector(tmp, sectorAddress + i))
                return false;
        }

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        byte[] sector;

        switch(_imageInfo.MetadataMediaType)
        {
            case MetadataMediaType.OpticalDisc:
                Track track =
                    Tracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

                if(track is null)
                {
                    ErrorMessage = Localization.Track_not_found;

                    return false;
                }

                if(track.Sequence    == 0 &&
                   track.StartSector == 0 &&
                   track.EndSector   == 0)
                    track.Type = TrackType.Data;

                if(data.Length          == 2064 &&
                   _imageInfo.MediaType == MediaType.DVDROM)
                {
                    sector        =   new byte[2048];
                    _sectorId     ??= new byte[_imageInfo.Sectors * 4];
                    _sectorIed    ??= new byte[_imageInfo.Sectors * 2];
                    _sectorCprMai ??= new byte[_imageInfo.Sectors * 6];
                    _sectorEdc    ??= new byte[_imageInfo.Sectors * 4];

                    Array.Copy(data, 0, _sectorId, (int)sectorAddress  * 4, 4);
                    Array.Copy(data, 4, _sectorIed, (int)sectorAddress * 2, 2);
                    Array.Copy(data, 6, _sectorCprMai, (int)sectorAddress * 6, 6);
                    Array.Copy(data, 12, sector, 0, 2048);
                    Array.Copy(data, 2060, _sectorEdc, (int)sectorAddress * 4, 4);

                    return WriteSector(sector, sectorAddress);
                }
                
                if(data.Length != 2352)
                {
                    ErrorMessage = Localization.Incorrect_data_size;

                    return false;
                }

                _writingLong = true;

                if(!_rewinded)
                {
                    if(sectorAddress <= _lastWrittenBlock && _alreadyWrittenZero)
                    {
                        _rewinded        = true;
                        _md5Provider     = null;
                        _sha1Provider    = null;
                        _sha256Provider  = null;
                        _spamsumProvider = null;
                    }

                    _md5Provider?.Update(data);
                    _sha1Provider?.Update(data);
                    _sha256Provider?.Update(data);
                    _spamsumProvider?.Update(data);
                    _lastWrittenBlock = sectorAddress;
                }

                bool prefixCorrect;
                int  minute;
                int  second;
                int  frame;
                int  storedLba;

                // Split raw cd sector data in prefix (sync, header), user data and suffix (edc, ecc p, ecc q)
                switch(track.Type)
                {
                    case TrackType.Audio:
                    case TrackType.Data: return WriteSector(data, sectorAddress);
                    case TrackType.CdMode1:
                        if(_sectorPrefix != null &&
                           _sectorSuffix != null)
                        {
                            sector = new byte[2048];
                            Array.Copy(data, 0, _sectorPrefix, (int)sectorAddress * 16, 16);
                            Array.Copy(data, 16, sector, 0, 2048);
                            Array.Copy(data, 2064, _sectorSuffix, (int)sectorAddress * 288, 288);

                            return WriteSector(sector, sectorAddress);
                        }

                        _sectorSuffixMs ??= new MemoryStream();

                        _sectorPrefixMs ??= new MemoryStream();

                        if(_sectorSuffixDdt == null)
                        {
                            _sectorSuffixDdt = new uint[_imageInfo.Sectors];
                            EccInit();
                        }

                        _sectorPrefixDdt ??= new uint[_imageInfo.Sectors];

                        sector = new byte[2048];

                        if(ArrayHelpers.ArrayIsNullOrEmpty(data))
                        {
                            _sectorPrefixDdt[sectorAddress] = (uint)CdFixFlags.NotDumped;
                            _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.NotDumped;

                            return WriteSector(sector, sectorAddress);
                        }

                        prefixCorrect = true;

                        if(data[0x00] != 0x00 ||
                           data[0x01] != 0xFF ||
                           data[0x02] != 0xFF ||
                           data[0x03] != 0xFF ||
                           data[0x04] != 0xFF ||
                           data[0x05] != 0xFF ||
                           data[0x06] != 0xFF ||
                           data[0x07] != 0xFF ||
                           data[0x08] != 0xFF ||
                           data[0x09] != 0xFF ||
                           data[0x0A] != 0xFF ||
                           data[0x0B] != 0x00 ||
                           data[0x0F] != 0x01)
                            prefixCorrect = false;

                        if(prefixCorrect)
                        {
                            minute        = ((data[0x0C] >> 4) * 10)                   + (data[0x0C] & 0x0F);
                            second        = ((data[0x0D] >> 4) * 10)                   + (data[0x0D] & 0x0F);
                            frame         = ((data[0x0E] >> 4) * 10)                   + (data[0x0E] & 0x0F);
                            storedLba     = (minute * 60 * 75) + (second * 75) + frame - 150;
                            prefixCorrect = storedLba == (int)sectorAddress;
                        }

                        if(prefixCorrect)
                            _sectorPrefixDdt[sectorAddress] = (uint)CdFixFlags.Correct;
                        else
                        {
                            if((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) > 0)
                                _sectorPrefixMs.Position = ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;
                            else
                                _sectorPrefixMs.Seek(0, SeekOrigin.End);

                            _sectorPrefixDdt[sectorAddress] = (uint)((_sectorPrefixMs.Position / 16) + 1);
                            _sectorPrefixMs.Write(data, 0, 16);
                        }

                        bool correct = SuffixIsCorrect(data);

                        if(correct)
                            _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.Correct;
                        else
                        {
                            if((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) > 0)
                                _sectorSuffixMs.Position = ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;
                            else
                                _sectorSuffixMs.Seek(0, SeekOrigin.End);

                            _sectorSuffixDdt[sectorAddress] = (uint)((_sectorSuffixMs.Position / 288) + 1);

                            _sectorSuffixMs.Write(data, 2064, 288);
                        }

                        Array.Copy(data, 16, sector, 0, 2048);

                        return WriteSector(sector, sectorAddress);
                    case TrackType.CdMode2Formless:
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                        if(_sectorPrefix != null &&
                           _sectorSuffix != null)
                        {
                            sector = new byte[2336];
                            Array.Copy(data, 0, _sectorPrefix, (int)sectorAddress * 16, 16);
                            Array.Copy(data, 16, sector, 0, 2336);

                            return WriteSector(sector, sectorAddress);
                        }

                        _sectorSuffixMs ??= new MemoryStream();

                        _sectorPrefixMs ??= new MemoryStream();

                        if(_sectorSuffixDdt == null)
                        {
                            _sectorSuffixDdt = new uint[_imageInfo.Sectors];
                            EccInit();
                        }

                        _sectorPrefixDdt ??= new uint[_imageInfo.Sectors];

                        sector = new byte[2328];

                        if(ArrayHelpers.ArrayIsNullOrEmpty(data))
                        {
                            _sectorPrefixDdt[sectorAddress] = (uint)CdFixFlags.NotDumped;

                            return WriteSector(sector, sectorAddress);
                        }

                        prefixCorrect = true;

                        if(data[0x00] != 0x00 ||
                           data[0x01] != 0xFF ||
                           data[0x02] != 0xFF ||
                           data[0x03] != 0xFF ||
                           data[0x04] != 0xFF ||
                           data[0x05] != 0xFF ||
                           data[0x06] != 0xFF ||
                           data[0x07] != 0xFF ||
                           data[0x08] != 0xFF ||
                           data[0x09] != 0xFF ||
                           data[0x0A] != 0xFF ||
                           data[0x0B] != 0x00 ||
                           data[0x0F] != 0x02)
                            prefixCorrect = false;

                        if(prefixCorrect)
                        {
                            minute        = ((data[0x0C] >> 4) * 10)                   + (data[0x0C] & 0x0F);
                            second        = ((data[0x0D] >> 4) * 10)                   + (data[0x0D] & 0x0F);
                            frame         = ((data[0x0E] >> 4) * 10)                   + (data[0x0E] & 0x0F);
                            storedLba     = (minute * 60 * 75) + (second * 75) + frame - 150;
                            prefixCorrect = storedLba == (int)sectorAddress;
                        }

                        if(prefixCorrect)
                            _sectorPrefixDdt[sectorAddress] = (uint)CdFixFlags.Correct;
                        else
                        {
                            if((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) > 0)
                                _sectorPrefixMs.Position = ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;
                            else
                                _sectorPrefixMs.Seek(0, SeekOrigin.End);

                            _sectorPrefixDdt[sectorAddress] = (uint)((_sectorPrefixMs.Position / 16) + 1);

                            _sectorPrefixMs.Write(data, 0, 16);
                        }

                        _mode2Subheaders ??= new byte[_imageInfo.Sectors * 8];

                        bool form2 = (data[18] & 0x20) == 0x20 || (data[22] & 0x20) == 0x20;

                        if(form2)
                        {
                            uint computedEdc = ComputeEdc(0, data, 0x91C, 0x10);
                            uint edc         = BitConverter.ToUInt32(data, 0x92C);
                            bool correctEdc  = computedEdc == edc;

                            sector = new byte[2324];

                            _sectorSuffixDdt ??= new uint[_imageInfo.Sectors];

                            Array.Copy(data, 24, sector, 0, 2324);

                            if(correctEdc)
                                _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.Mode2Form2Ok;
                            else if(BitConverter.ToUInt32(data, 0x92C) == 0)
                                _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.Mode2Form2NoCrc;
                            else
                            {
                                if((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) > 0)
                                    _sectorSuffixMs.Position =
                                        ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;
                                else
                                    _sectorSuffixMs.Seek(0, SeekOrigin.End);

                                _sectorSuffixDdt[sectorAddress] = (uint)((_sectorSuffixMs.Position / 288) + 1);

                                _sectorSuffixMs.Write(data, 2348, 4);
                            }
                        }
                        else
                        {
                            bool correctEcc = SuffixIsCorrectMode2(data);

                            uint computedEdc = ComputeEdc(0, data, 0x808, 0x10);
                            uint edc         = BitConverter.ToUInt32(data, 0x818);
                            bool correctEdc  = computedEdc == edc;

                            sector = new byte[2048];
                            Array.Copy(data, 24, sector, 0, 2048);

                            if(correctEcc && correctEdc)
                            {
                                _sectorSuffixDdt ??= new uint[_imageInfo.Sectors];

                                _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.Mode2Form1Ok;
                            }
                            else
                            {
                                if((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) > 0)
                                    _sectorSuffixMs.Position =
                                        ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;
                                else
                                    _sectorSuffixMs.Seek(0, SeekOrigin.End);

                                _sectorSuffixDdt[sectorAddress] = (uint)((_sectorSuffixMs.Position / 288) + 1);

                                _sectorSuffixMs.Write(data, 2072, 280);
                            }
                        }

                        Array.Copy(data, 16, _mode2Subheaders, (int)sectorAddress * 8, 8);

                        return WriteSector(sector, sectorAddress);
                }

                break;
            case MetadataMediaType.BlockMedia:
                switch(_imageInfo.MediaType)
                {
                    // Split user data from Apple tags
                    case MediaType.AppleFileWare:
                    case MediaType.AppleProfile:
                    case MediaType.AppleSonyDS:
                    case MediaType.AppleSonySS:
                    case MediaType.AppleWidget:
                    case MediaType.PriamDataTower:
                        byte[] oldTag;
                        byte[] newTag;

                        switch(data.Length - 512)
                        {
                            // Sony tag, convert to Profile
                            case 12 when _imageInfo.MediaType is MediaType.AppleProfile or MediaType.AppleFileWare:
                                oldTag = new byte[12];
                                Array.Copy(data, 512, oldTag, 0, 12);
                                newTag = LisaTag.DecodeSonyTag(oldTag)?.ToProfile().GetBytes();

                                break;

                            // Sony tag, convert to Priam
                            case 12 when _imageInfo.MediaType == MediaType.PriamDataTower:
                                oldTag = new byte[12];
                                Array.Copy(data, 512, oldTag, 0, 12);
                                newTag = LisaTag.DecodeSonyTag(oldTag)?.ToPriam().GetBytes();

                                break;

                            // Sony tag, copy to Sony
                            case 12 when _imageInfo.MediaType is MediaType.AppleSonySS or MediaType.AppleSonySS:
                                newTag = new byte[12];
                                Array.Copy(data, 512, newTag, 0, 12);

                                break;

                            // Profile tag, copy to Profile
                            case 20 when _imageInfo.MediaType is MediaType.AppleProfile or MediaType.AppleFileWare:
                                newTag = new byte[20];
                                Array.Copy(data, 512, newTag, 0, 20);

                                break;

                            // Profile tag, convert to Priam
                            case 20 when _imageInfo.MediaType == MediaType.PriamDataTower:
                                oldTag = new byte[20];
                                Array.Copy(data, 512, oldTag, 0, 20);
                                newTag = LisaTag.DecodeProfileTag(oldTag)?.ToPriam().GetBytes();

                                break;

                            // Profile tag, convert to Sony
                            case 20 when _imageInfo.MediaType is MediaType.AppleSonySS or MediaType.AppleSonySS:
                                oldTag = new byte[20];
                                Array.Copy(data, 512, oldTag, 0, 20);
                                newTag = LisaTag.DecodeProfileTag(oldTag)?.ToSony().GetBytes();

                                break;

                            // Priam tag, convert to Profile
                            case 24 when _imageInfo.MediaType is MediaType.AppleProfile or MediaType.AppleFileWare:
                                oldTag = new byte[24];
                                Array.Copy(data, 512, oldTag, 0, 24);
                                newTag = LisaTag.DecodePriamTag(oldTag)?.ToProfile().GetBytes();

                                break;

                            // Priam tag, copy to Priam
                            case 12 when _imageInfo.MediaType == MediaType.PriamDataTower:
                                newTag = new byte[24];
                                Array.Copy(data, 512, newTag, 0, 24);

                                break;

                            // Priam tag, convert to Sony
                            case 24 when _imageInfo.MediaType is MediaType.AppleSonySS or MediaType.AppleSonySS:
                                oldTag = new byte[24];
                                Array.Copy(data, 512, oldTag, 0, 24);
                                newTag = LisaTag.DecodePriamTag(oldTag)?.ToSony().GetBytes();

                                break;
                            case 0:
                                newTag = null;

                                break;
                            default:
                                ErrorMessage = Localization.Incorrect_data_size;

                                return false;
                        }

                        sector = new byte[512];
                        Array.Copy(data, 0, sector, 0, 512);

                        if(newTag == null)
                            return WriteSector(sector, sectorAddress);

                        _sectorSubchannel ??= new byte[newTag.Length * (int)_imageInfo.Sectors];

                        Array.Copy(newTag, 0, _sectorSubchannel, newTag.Length * (int)sectorAddress, newTag.Length);

                        return WriteSector(sector, sectorAddress);
                }

                break;
        }

        ErrorMessage = Localization.Unknown_long_sector_type_cannot_write;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        byte[] sector;

        switch(_imageInfo.MetadataMediaType)
        {
            case MetadataMediaType.OpticalDisc:
                switch(_imageInfo.MediaType)
                {
                    case MediaType.DVDROM:
                        if(data.Length % 2064 != 0)
                        {
                            ErrorMessage = Localization.Incorrect_data_size;

                            return false;
                        }

                        sector = new byte[2064];

                        for(uint i = 0; i < length; i++)
                        {
                            Array.Copy(data, 2064 * i, sector, 0, 2064);

                            if(!WriteSectorLong(sector, sectorAddress + i))
                                return false;
                        }

                        ErrorMessage = "";

                        return true;
                    
                    default:
                        if(data.Length % 2352 != 0)
                        {
                            ErrorMessage = Localization.Incorrect_data_size;

                            return false;
                        }

                        sector = new byte[2352];

                        for(uint i = 0; i < length; i++)
                        {
                            Array.Copy(data, 2352 * i, sector, 0, 2352);

                            if(!WriteSectorLong(sector, sectorAddress + i))
                                return false;
                        }

                        ErrorMessage = "";

                        return true;
                }
                
            case MetadataMediaType.BlockMedia:
                switch(_imageInfo.MediaType)
                {
                    case MediaType.AppleFileWare:
                    case MediaType.AppleProfile:
                    case MediaType.AppleSonyDS:
                    case MediaType.AppleSonySS:
                    case MediaType.AppleWidget:
                    case MediaType.PriamDataTower:
                        int sectorSize = 0;

                        if(data.Length % 524 == 0)
                            sectorSize = 524;
                        else if(data.Length % 532 == 0)
                            sectorSize = 532;
                        else if(data.Length % 536 == 0)
                            sectorSize = 536;

                        if(sectorSize == 0)
                        {
                            ErrorMessage = Localization.Incorrect_data_size;

                            return false;
                        }

                        sector = new byte[sectorSize];

                        for(uint i = 0; i < length; i++)
                        {
                            Array.Copy(data, sectorSize * i, sector, 0, sectorSize);

                            if(!WriteSectorLong(sector, sectorAddress + i))
                                return false;
                        }

                        ErrorMessage = "";

                        return true;
                }

                break;
        }

        ErrorMessage = Localization.Unknown_long_sector_type_cannot_write;

        return false;
    }

    /// <inheritdoc />
    public bool SetTracks(List<Track> tracks)
    {
        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
        {
            ErrorMessage = Localization.Unsupported_feature;

            return false;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        Tracks       = tracks;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        // Close current block first
        if(_writingBuffer != null)
        {
            _currentBlockHeader.length = _currentBlockOffset * _currentBlockHeader.sectorSize;
            _currentBlockHeader.crc64  = BitConverter.ToUInt64(_crc64.Final(), 0);

            var cmpCrc64Context = new Crc64Context();

            byte[] lzmaProperties   = Array.Empty<byte>();
            int    compressedLength = 0;

            switch(_currentBlockHeader.compression)
            {
                case CompressionType.Flac:
                {
                    uint currentSamples = _currentBlockOffset * SAMPLES_PER_SECTOR;
                    uint flacBlockSize  = _currentBlockOffset * SAMPLES_PER_SECTOR;

                    if(flacBlockSize > MAX_FLAKE_BLOCK)
                        flacBlockSize = MAX_FLAKE_BLOCK;

                    if(flacBlockSize < MIN_FLAKE_BLOCK)
                        flacBlockSize = MIN_FLAKE_BLOCK;

                    long remaining = currentSamples % flacBlockSize;

                    // Fill FLAC block
                    if(remaining != 0)
                        for(int r = 0; r < remaining * 4; r++)
                            _writingBuffer[_writingBufferPosition + r] = 0;

                    compressedLength = FLAC.EncodeBuffer(_writingBuffer, _compressedBuffer, flacBlockSize, true, false,
                                                         "hamming", 12, 15, true, false, 0, 8, "Aaru");

                    if(compressedLength >= _writingBufferPosition)
                        _currentBlockHeader.compression = CompressionType.None;

                    break;
                }
                case CompressionType.Lzma:
                {
                    compressedLength = LZMA.EncodeBuffer(_writingBuffer, _compressedBuffer, out lzmaProperties, 9,
                                                         _dictionarySize, 4, 0, 2, 273);

                    cmpCrc64Context.Update(lzmaProperties);

                    if(compressedLength >= _writingBufferPosition)
                        _currentBlockHeader.compression = CompressionType.None;

                    break;
                }
                case CompressionType.None: break; // Do nothing
                default:                   throw new ArgumentOutOfRangeException();
            }

            if(_currentBlockHeader.compression == CompressionType.None)
            {
                _currentBlockHeader.cmpCrc64  = _currentBlockHeader.crc64;
                _currentBlockHeader.cmpLength = (uint)_writingBufferPosition;
            }
            else
            {
                cmpCrc64Context.Update(_compressedBuffer, (uint)compressedLength);
                _currentBlockHeader.cmpCrc64  = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);
                _currentBlockHeader.cmpLength = (uint)compressedLength;
            }

            if(_currentBlockHeader.compression == CompressionType.Lzma)
                _currentBlockHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

            _index.Add(new IndexEntry
            {
                blockType = BlockType.DataBlock,
                dataType  = DataType.UserData,
                offset    = (ulong)_imageStream.Position
            });

            _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
            MemoryMarshal.Write(_structureBytes, in _currentBlockHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _structureBytes = null;

            if(_currentBlockHeader.compression == CompressionType.Lzma)
                _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

            if(_currentBlockHeader.compression == CompressionType.None)
                _imageStream.Write(_writingBuffer, 0, _writingBufferPosition);
            else
                _imageStream.Write(_compressedBuffer, 0, compressedLength);

            _writingBuffer = null;
        }

        if(_deduplicate)
            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Of_0_sectors_written_1_are_unique_2,
                                       _writtenSectors, _deduplicationTable.Count,
                                       (double)_deduplicationTable.Count / _writtenSectors);

        IndexEntry idxEntry;

        // TODO: Reuse buffer
        MemoryStream blockStream;

        // Write media tag blocks
        foreach(KeyValuePair<MediaTagType, byte[]> mediaTag in _mediaTags)
        {
            DataType dataType = GetDataTypeForMediaTag(mediaTag.Key);

            if(mediaTag.Value is null)
            {
                AaruConsole.ErrorWriteLine(Localization.Tag_type_0_is_null_skipping, dataType);

                continue;
            }

            idxEntry = new IndexEntry
            {
                blockType = BlockType.DataBlock,
                dataType  = dataType,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_tag_type_0_to_position_1,
                                       mediaTag.Key, idxEntry.offset);

            Crc64Context.Data(mediaTag.Value, out byte[] tagCrc);

            var tagBlock = new BlockHeader
            {
                identifier = BlockType.DataBlock,
                type       = dataType,
                length     = (uint)mediaTag.Value.Length,
                crc64      = BitConverter.ToUInt64(tagCrc, 0)
            };

            byte[] cmpBuffer = new byte[mediaTag.Value.Length + 262144];

            byte[] lzmaProperties = null;
            bool   doNotCompress  = false;

            switch(_compressionAlgorithm)
            {
                case CompressionType.Lzma:
                    int cmpLen = LZMA.EncodeBuffer(mediaTag.Value, cmpBuffer, out lzmaProperties, 9, _dictionarySize, 4,
                                                   0, 2, 273);

                    if(cmpLen + LZMA_PROPERTIES_LENGTH > mediaTag.Value.Length)
                        doNotCompress = true;

                    break;
                case CompressionType.None:
                    doNotCompress = true;

                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            byte[] tagData;

            // Not compressible
            if(doNotCompress)
            {
                tagBlock.cmpLength   = tagBlock.length;
                tagBlock.cmpCrc64    = tagBlock.crc64;
                tagData              = mediaTag.Value;
                tagBlock.compression = CompressionType.None;
            }
            else
            {
                tagData = cmpBuffer;
                var crc64Ctx = new Crc64Context();

                if(_compressionAlgorithm == CompressionType.Lzma)
                    crc64Ctx.Update(lzmaProperties);

                crc64Ctx.Update(tagData);
                tagCrc               = crc64Ctx.Final();
                tagBlock.cmpLength   = (uint)tagData.Length;
                tagBlock.cmpCrc64    = BitConverter.ToUInt64(tagCrc, 0);
                tagBlock.compression = _compressionAlgorithm;

                if(_compressionAlgorithm == CompressionType.Lzma)
                    tagBlock.cmpLength += LZMA_PROPERTIES_LENGTH;
            }

            _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
            MemoryMarshal.Write(_structureBytes, in tagBlock);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

            if(tagBlock.compression == CompressionType.Lzma)
                _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

            _imageStream.Write(tagData, 0, tagData.Length);

            _index.RemoveAll(t => t.blockType == BlockType.DataBlock && t.dataType == dataType);

            _index.Add(idxEntry);
        }

        // If we have set the geometry block, write it
        if(_geometryBlock.identifier == BlockType.GeometryBlock)
        {
            idxEntry = new IndexEntry
            {
                blockType = BlockType.GeometryBlock,
                dataType  = DataType.NoData,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_geometry_block_to_position_0,
                                       idxEntry.offset);

            _structureBytes = new byte[Marshal.SizeOf<GeometryBlock>()];
            MemoryMarshal.Write(_structureBytes, in _geometryBlock);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

            _index.RemoveAll(t => t is { blockType: BlockType.GeometryBlock, dataType: DataType.NoData });

            _index.Add(idxEntry);
        }

        // If we have dump hardware, write it
        if(DumpHardware != null)
        {
            var dumpMs = new MemoryStream();

            foreach(DumpHardware dump in DumpHardware)
            {
                byte[] dumpManufacturer            = null;
                byte[] dumpModel                   = null;
                byte[] dumpRevision                = null;
                byte[] dumpFirmware                = null;
                byte[] dumpSerial                  = null;
                byte[] dumpSoftwareName            = null;
                byte[] dumpSoftwareVersion         = null;
                byte[] dumpSoftwareOperatingSystem = null;

                if(!string.IsNullOrWhiteSpace(dump.Manufacturer))
                    dumpManufacturer = Encoding.UTF8.GetBytes(dump.Manufacturer);

                if(!string.IsNullOrWhiteSpace(dump.Model))
                    dumpModel = Encoding.UTF8.GetBytes(dump.Model);

                if(!string.IsNullOrWhiteSpace(dump.Revision))
                    dumpRevision = Encoding.UTF8.GetBytes(dump.Revision);

                if(!string.IsNullOrWhiteSpace(dump.Firmware))
                    dumpFirmware = Encoding.UTF8.GetBytes(dump.Firmware);

                if(!string.IsNullOrWhiteSpace(dump.Serial))
                    dumpSerial = Encoding.UTF8.GetBytes(dump.Serial);

                if(!string.IsNullOrWhiteSpace(dump.Software?.Name))
                    dumpSoftwareName = Encoding.UTF8.GetBytes(dump.Software.Name);

                if(!string.IsNullOrWhiteSpace(dump.Software?.Version))
                    dumpSoftwareVersion = Encoding.UTF8.GetBytes(dump.Software.Version);

                if(!string.IsNullOrWhiteSpace(dump.Software?.OperatingSystem))
                    dumpSoftwareOperatingSystem = Encoding.UTF8.GetBytes(dump.Software.OperatingSystem);

                var dumpEntry = new DumpHardwareEntry
                {
                    manufacturerLength            = (uint)(dumpManufacturer?.Length            + 1 ?? 0),
                    modelLength                   = (uint)(dumpModel?.Length                   + 1 ?? 0),
                    revisionLength                = (uint)(dumpRevision?.Length                + 1 ?? 0),
                    firmwareLength                = (uint)(dumpFirmware?.Length                + 1 ?? 0),
                    serialLength                  = (uint)(dumpSerial?.Length                  + 1 ?? 0),
                    softwareNameLength            = (uint)(dumpSoftwareName?.Length            + 1 ?? 0),
                    softwareVersionLength         = (uint)(dumpSoftwareVersion?.Length         + 1 ?? 0),
                    softwareOperatingSystemLength = (uint)(dumpSoftwareOperatingSystem?.Length + 1 ?? 0),
                    extents                       = (uint)dump.Extents.Count
                };

                _structureBytes = new byte[Marshal.SizeOf<DumpHardwareEntry>()];
                MemoryMarshal.Write(_structureBytes, in dumpEntry);
                dumpMs.Write(_structureBytes, 0, _structureBytes.Length);

                if(dumpManufacturer != null)
                {
                    dumpMs.Write(dumpManufacturer, 0, dumpManufacturer.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpModel != null)
                {
                    dumpMs.Write(dumpModel, 0, dumpModel.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpRevision != null)
                {
                    dumpMs.Write(dumpRevision, 0, dumpRevision.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpFirmware != null)
                {
                    dumpMs.Write(dumpFirmware, 0, dumpFirmware.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpSerial != null)
                {
                    dumpMs.Write(dumpSerial, 0, dumpSerial.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpSoftwareName != null)
                {
                    dumpMs.Write(dumpSoftwareName, 0, dumpSoftwareName.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpSoftwareVersion != null)
                {
                    dumpMs.Write(dumpSoftwareVersion, 0, dumpSoftwareVersion.Length);
                    dumpMs.WriteByte(0);
                }

                if(dumpSoftwareOperatingSystem != null)
                {
                    dumpMs.Write(dumpSoftwareOperatingSystem, 0, dumpSoftwareOperatingSystem.Length);
                    dumpMs.WriteByte(0);
                }

                foreach(Extent extent in dump.Extents)
                {
                    dumpMs.Write(BitConverter.GetBytes(extent.Start), 0, sizeof(ulong));
                    dumpMs.Write(BitConverter.GetBytes(extent.End), 0, sizeof(ulong));
                }
            }

            idxEntry = new IndexEntry
            {
                blockType = BlockType.DumpHardwareBlock,
                dataType  = DataType.NoData,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_dump_hardware_block_to_position_0,
                                       idxEntry.offset);

            Crc64Context.Data(dumpMs.ToArray(), out byte[] dumpCrc);

            var dumpBlock = new DumpHardwareHeader
            {
                identifier = BlockType.DumpHardwareBlock,
                entries    = (ushort)DumpHardware.Count,
                crc64      = BitConverter.ToUInt64(dumpCrc, 0),
                length     = (uint)dumpMs.Length
            };

            _structureBytes = new byte[Marshal.SizeOf<DumpHardwareHeader>()];
            MemoryMarshal.Write(_structureBytes, in dumpBlock);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _imageStream.Write(dumpMs.ToArray(), 0, (int)dumpMs.Length);

            _index.RemoveAll(t => t is { blockType: BlockType.DumpHardwareBlock, dataType: DataType.NoData });

            _index.Add(idxEntry);
        }

        // If we have Aaru Metadata, write it
        if(AaruMetadata != null)
        {
            var jsonMs = new MemoryStream();

            JsonSerializer.Serialize(jsonMs, new MetadataJson
            {
                AaruMetadata = AaruMetadata
            }, typeof(MetadataJson), MetadataJsonContext.Default);

            idxEntry = new IndexEntry
            {
                blockType = BlockType.AaruMetadataJsonBlock,
                dataType  = DataType.NoData,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_Aaru_Metadata_block_to_position_0,
                                       idxEntry.offset);

            var jsonBlock = new AaruMetadataJsonBlock
            {
                identifier = BlockType.AaruMetadataJsonBlock,
                length     = (uint)jsonMs.Length
            };

            _structureBytes = new byte[Marshal.SizeOf<AaruMetadataJsonBlock>()];
            MemoryMarshal.Write(_structureBytes, in jsonBlock);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _imageStream.Write(jsonMs.ToArray(), 0, (int)jsonMs.Length);

            // Ensure no CICM XML block is recorded altogether
            _index.RemoveAll(t => t is { blockType: BlockType.CicmBlock, dataType            : DataType.NoData });
            _index.RemoveAll(t => t is { blockType: BlockType.AaruMetadataJsonBlock, dataType: DataType.NoData });

            _index.Add(idxEntry);
        }

        // If we have checksums, write it to disk
        if(_md5Provider     != null ||
           _sha1Provider    != null ||
           _sha256Provider  != null ||
           _spamsumProvider != null)
        {
            var chkMs = new MemoryStream();

            var chkHeader = new ChecksumHeader
            {
                identifier = BlockType.ChecksumBlock
            };

            if(_md5Provider != null)
            {
                byte[] md5 = _md5Provider.Final();

                var md5Entry = new ChecksumEntry
                {
                    type   = ChecksumAlgorithm.Md5,
                    length = (uint)md5.Length
                };

                _structureBytes = new byte[Marshal.SizeOf<ChecksumEntry>()];
                MemoryMarshal.Write(_structureBytes, in md5Entry);
                chkMs.Write(_structureBytes, 0, _structureBytes.Length);
                chkMs.Write(md5, 0, md5.Length);
                chkHeader.entries++;
            }

            if(_sha1Provider != null)
            {
                byte[] sha1 = _sha1Provider.Final();

                var sha1Entry = new ChecksumEntry
                {
                    type   = ChecksumAlgorithm.Sha1,
                    length = (uint)sha1.Length
                };

                _structureBytes = new byte[Marshal.SizeOf<ChecksumEntry>()];
                MemoryMarshal.Write(_structureBytes, in sha1Entry);
                chkMs.Write(_structureBytes, 0, _structureBytes.Length);
                chkMs.Write(sha1, 0, sha1.Length);
                chkHeader.entries++;
            }

            if(_sha256Provider != null)
            {
                byte[] sha256 = _sha256Provider.Final();

                var sha256Entry = new ChecksumEntry
                {
                    type   = ChecksumAlgorithm.Sha256,
                    length = (uint)sha256.Length
                };

                _structureBytes = new byte[Marshal.SizeOf<ChecksumEntry>()];
                MemoryMarshal.Write(_structureBytes, in sha256Entry);
                chkMs.Write(_structureBytes, 0, _structureBytes.Length);
                chkMs.Write(sha256, 0, sha256.Length);
                chkHeader.entries++;
            }

            if(_spamsumProvider != null)
            {
                byte[] spamsum = Encoding.ASCII.GetBytes(_spamsumProvider.End());

                var spamsumEntry = new ChecksumEntry
                {
                    type   = ChecksumAlgorithm.SpamSum,
                    length = (uint)spamsum.Length
                };

                _structureBytes = new byte[Marshal.SizeOf<ChecksumEntry>()];
                MemoryMarshal.Write(_structureBytes, in spamsumEntry);
                chkMs.Write(_structureBytes, 0, _structureBytes.Length);
                chkMs.Write(spamsum, 0, spamsum.Length);
                chkHeader.entries++;
            }

            if(chkHeader.entries > 0)
            {
                chkHeader.length = (uint)chkMs.Length;

                idxEntry = new IndexEntry
                {
                    blockType = BlockType.ChecksumBlock,
                    dataType  = DataType.NoData,
                    offset    = (ulong)_imageStream.Position
                };

                AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_checksum_block_to_position_0,
                                           idxEntry.offset);

                _structureBytes = new byte[Marshal.SizeOf<ChecksumHeader>()];
                MemoryMarshal.Write(_structureBytes, in chkHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _imageStream.Write(chkMs.ToArray(), 0, (int)chkMs.Length);

                _index.RemoveAll(t => t is { blockType: BlockType.ChecksumBlock, dataType: DataType.NoData });

                _index.Add(idxEntry);
            }
        }

        if(IsTape)
        {
            ulong latestBlock = _tapeDdt.Max(b => b.Key);

            _userDataDdt = new ulong[latestBlock + 1];

            foreach(KeyValuePair<ulong, ulong> block in _tapeDdt)
                _userDataDdt[block.Key] = block.Value;

            _inMemoryDdt = true;
            _tapeDdt.Clear();

            idxEntry = new IndexEntry
            {
                blockType = BlockType.TapePartitionBlock,
                dataType  = DataType.UserData,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_tape_partitions_to_position_0,
                                       idxEntry.offset);

            TapePartitionEntry[] tapePartitionEntries = new TapePartitionEntry[TapePartitions.Count];

            for(int t = 0; t < TapePartitions.Count; t++)
                tapePartitionEntries[t] = new TapePartitionEntry
                {
                    Number     = TapePartitions[t].Number,
                    FirstBlock = TapePartitions[t].FirstBlock,
                    LastBlock  = TapePartitions[t].LastBlock
                };

            byte[] tapePartitionEntriesData =
                MemoryMarshal.Cast<TapePartitionEntry, byte>(tapePartitionEntries).ToArray();

            var tapePartitionHeader = new TapePartitionHeader
            {
                identifier = BlockType.TapePartitionBlock,
                entries    = (byte)tapePartitionEntries.Length,
                length     = (ulong)tapePartitionEntriesData.Length
            };

            _crc64 = new Crc64Context();
            _crc64.Update(tapePartitionEntriesData);
            tapePartitionHeader.crc64 = BitConverter.ToUInt64(_crc64.Final(), 0);

            _structureBytes = new byte[Marshal.SizeOf<TapePartitionHeader>()];
            MemoryMarshal.Write(_structureBytes, in tapePartitionHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _structureBytes = null;
            _imageStream.Write(tapePartitionEntriesData, 0, tapePartitionEntriesData.Length);

            _index.RemoveAll(t => t is { blockType: BlockType.TapePartitionBlock, dataType: DataType.UserData });
            _index.Add(idxEntry);

            idxEntry = new IndexEntry
            {
                blockType = BlockType.TapeFileBlock,
                dataType  = DataType.UserData,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_tape_files_to_position_0,
                                       idxEntry.offset);

            TapeFileEntry[] tapeFileEntries = new TapeFileEntry[Files.Count];

            for(int t = 0; t < Files.Count; t++)
                tapeFileEntries[t] = new TapeFileEntry
                {
                    File       = Files[t].File,
                    FirstBlock = Files[t].FirstBlock,
                    LastBlock  = Files[t].LastBlock
                };

            byte[] tapeFileEntriesData = MemoryMarshal.Cast<TapeFileEntry, byte>(tapeFileEntries).ToArray();

            var tapeFileHeader = new TapeFileHeader
            {
                identifier = BlockType.TapeFileBlock,
                entries    = (uint)tapeFileEntries.Length,
                length     = (ulong)tapeFileEntriesData.Length
            };

            _crc64 = new Crc64Context();
            _crc64.Update(tapeFileEntriesData);
            tapeFileHeader.crc64 = BitConverter.ToUInt64(_crc64.Final(), 0);

            _structureBytes = new byte[Marshal.SizeOf<TapeFileHeader>()];
            MemoryMarshal.Write(_structureBytes, in tapeFileHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _structureBytes = null;
            _imageStream.Write(tapeFileEntriesData, 0, tapeFileEntriesData.Length);

            _index.RemoveAll(t => t is { blockType: BlockType.TapeFileBlock, dataType: DataType.UserData });
            _index.Add(idxEntry);
        }

        // If the DDT is in-memory, write it to disk
        if(_inMemoryDdt)
        {
            idxEntry = new IndexEntry
            {
                blockType = BlockType.DeDuplicationTable,
                dataType  = DataType.UserData,
                offset    = (ulong)_imageStream.Position
            };

            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_user_data_DDT_to_position_0,
                                       idxEntry.offset);

            var ddtHeader = new DdtHeader
            {
                identifier = BlockType.DeDuplicationTable,
                type       = DataType.UserData,

                compression = _compressionAlgorithm,
                shift       = _shift,
                entries     = (ulong)_userDataDdt.LongLength,
                length      = (ulong)(_userDataDdt.LongLength * sizeof(ulong))
            };

            _crc64 = new Crc64Context();
            byte[] ddtEntries = MemoryMarshal.Cast<ulong, byte>(_userDataDdt).ToArray();
            _crc64.Update(ddtEntries);

            byte[] cmpBuffer = new byte[ddtEntries.Length + 262144];

            int    cmpLen;
            byte[] lzmaProperties = null;

            switch(_compressionAlgorithm)
            {
                case CompressionType.None:
                    cmpBuffer = ddtEntries;
                    cmpLen    = cmpBuffer.Length;

                    break;
                case CompressionType.Lzma:
                    cmpLen = LZMA.EncodeBuffer(ddtEntries, cmpBuffer, out lzmaProperties, 9, _dictionarySize, 4, 0, 2,
                                               273);

                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

            ddtHeader.cmpLength = (uint)blockStream.Length;

            if(_compressionAlgorithm == CompressionType.Lzma &&
               cmpBuffer             != ddtEntries)
                ddtHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

            var cmpCrc64Context = new Crc64Context();

            if(_compressionAlgorithm == CompressionType.Lzma &&
               cmpBuffer             != ddtEntries)
                cmpCrc64Context.Update(lzmaProperties);

            cmpCrc64Context.Update(blockStream.ToArray());
            ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

            _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
            MemoryMarshal.Write(_structureBytes, in ddtHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            _structureBytes = null;

            if(_compressionAlgorithm == CompressionType.Lzma)
                _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

            _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
            blockStream.Close();

            _index.RemoveAll(t => t is { blockType: BlockType.DeDuplicationTable, dataType: DataType.UserData });

            _index.Add(idxEntry);
        }

        // Write the sector prefix, suffix and subchannels if present
        switch(_imageInfo.MetadataMediaType)
        {
            case MetadataMediaType.OpticalDisc when Tracks is { Count: > 0 }:
                DateTime startCompress;
                DateTime endCompress;

                // Old format
                if(_sectorPrefix != null &&
                   _sectorSuffix != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.CdSectorPrefix,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CD_sector_prefix_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_sectorPrefix, out byte[] blockCrc);

                    var prefixBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.CdSectorPrefix,
                        length     = (uint)_sectorPrefix.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 16
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        prefixBlock.compression = CompressionType.None;
                        prefixBlock.cmpCrc64    = prefixBlock.crc64;
                        prefixBlock.cmpLength   = prefixBlock.length;
                        blockStream             = new MemoryStream(_sectorPrefix);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorPrefix.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorPrefix, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorPrefix;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc              = cmpCrc.Final();
                        prefixBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            prefixBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        prefixBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Took_0_seconds_to_compress_prefix,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in prefixBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(prefixBlock.compression == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();

                    _index.RemoveAll(t => t is { blockType: BlockType.DataBlock, dataType: DataType.CdSectorPrefix });

                    _index.Add(idxEntry);

                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.CdSectorSuffix,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CD_sector_suffix_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_sectorSuffix, out blockCrc);

                    prefixBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.CdSectorSuffix,
                        length     = (uint)_sectorSuffix.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 288
                    };

                    if(!_compress)
                    {
                        prefixBlock.compression = CompressionType.None;
                        prefixBlock.cmpCrc64    = prefixBlock.crc64;
                        prefixBlock.cmpLength   = prefixBlock.length;
                        blockStream             = new MemoryStream(_sectorSuffix);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorSuffix.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorSuffix, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorSuffix;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc              = cmpCrc.Final();
                        prefixBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            prefixBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        prefixBlock.compression = CompressionType.Lzma;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Took_0_seconds_to_compress_suffix,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in prefixBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(prefixBlock.compression == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is { blockType: BlockType.DataBlock, dataType: DataType.CdSectorSuffix });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }
                else if(_sectorSuffixMs  != null &&
                        _sectorSuffixDdt != null &&
                        _sectorPrefixMs  != null &&
                        _sectorPrefixDdt != null)
                {
                #if DEBUG
                    uint notDumpedPrefixes = 0;
                    uint correctPrefixes   = 0;
                    uint writtenPrefixes   = 0;
                    uint notDumpedSuffixes = 0;
                    uint correctSuffixes   = 0;
                    uint writtenSuffixes   = 0;
                    uint correctMode2Form1 = 0;
                    uint correctMode2Form2 = 0;
                    uint emptyMode2Form1   = 0;

                    for(long i = 0; i < _sectorPrefixDdt.LongLength; i++)
                        switch(_sectorPrefixDdt[i] & CD_XFIX_MASK)
                        {
                            case (uint)CdFixFlags.NotDumped:
                                notDumpedPrefixes++;

                                break;
                            case (uint)CdFixFlags.Correct:
                                correctPrefixes++;

                                break;
                            default:
                            {
                                if((_sectorPrefixDdt[i] & CD_DFIX_MASK) > 0)
                                    writtenPrefixes++;

                                break;
                            }
                        }

                    for(long i = 0; i < _sectorPrefixDdt.LongLength; i++)
                        switch(_sectorSuffixDdt[i] & CD_XFIX_MASK)
                        {
                            case (uint)CdFixFlags.NotDumped:
                                notDumpedSuffixes++;

                                break;
                            case (uint)CdFixFlags.Correct:
                                correctSuffixes++;

                                break;
                            case (uint)CdFixFlags.Mode2Form1Ok:
                                correctMode2Form1++;

                                break;
                            case (uint)CdFixFlags.Mode2Form2Ok:
                                correctMode2Form2++;

                                break;
                            case (uint)CdFixFlags.Mode2Form2NoCrc:
                                emptyMode2Form1++;

                                break;
                            default:
                            {
                                if((_sectorSuffixDdt[i] & CD_DFIX_MASK) > 0)
                                    writtenSuffixes++;

                                break;
                            }
                        }

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.
                                                   _0_1_prefixes_are_correct_2_3_prefixes_have_not_been_dumped_4_5_prefixes_have_been_written_to_image,
                                               correctPrefixes, correctPrefixes / _imageInfo.Sectors, notDumpedPrefixes,
                                               notDumpedPrefixes                / _imageInfo.Sectors, writtenPrefixes,
                                               writtenPrefixes                  / _imageInfo.Sectors);

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.
                                                   _0_1_suffixes_are_correct_2_3_suffixes_have_not_been_dumped_4_5_suffixes_have_been_written_to_image,
                                               correctSuffixes, correctSuffixes / _imageInfo.Sectors, notDumpedSuffixes,
                                               notDumpedSuffixes                / _imageInfo.Sectors, writtenSuffixes,
                                               writtenSuffixes                  / _imageInfo.Sectors);

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.
                                                   _0_1_MODE_2_Form_1_are_correct_2_3_MODE_2_Form_2_are_correct_4_5_MODE_2_Form_2_have_empty_CRC,
                                               correctMode2Form1, correctMode2Form1 / _imageInfo.Sectors,
                                               correctMode2Form2, correctMode2Form2 / _imageInfo.Sectors,
                                               emptyMode2Form1, emptyMode2Form1     / _imageInfo.Sectors);
                #endif

                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DeDuplicationTable,
                        dataType  = DataType.CdSectorPrefixCorrected,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CompactDisc_sector_prefix_DDT_to_position_0,
                                               idxEntry.offset);

                    var ddtHeader = new DdtHeader
                    {
                        identifier  = BlockType.DeDuplicationTable,
                        type        = DataType.CdSectorPrefixCorrected,
                        compression = _compressionAlgorithm,
                        entries     = (ulong)_sectorPrefixDdt.LongLength,
                        length      = (ulong)(_sectorPrefixDdt.LongLength * sizeof(uint))
                    };

                    _crc64 = new Crc64Context();
                    byte[] ddtEntries = MemoryMarshal.Cast<uint, byte>(_sectorPrefixDdt).ToArray();
                    _crc64.Update(ddtEntries);

                    byte[] cmpBuffer = new byte[ddtEntries.Length + 262144];

                    int    cmpLen;
                    byte[] lzmaProperties = Array.Empty<byte>();

                    switch(_compressionAlgorithm)
                    {
                        case CompressionType.Lzma:
                            cmpLen = LZMA.EncodeBuffer(ddtEntries, cmpBuffer, out lzmaProperties, 9, _dictionarySize, 4,
                                                       0, 2, 273);

                            break;
                        case CompressionType.None:
                            cmpBuffer = ddtEntries;
                            cmpLen    = cmpBuffer.Length;

                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                    ddtHeader.cmpLength = (uint)blockStream.Length;

                    if(_compressionAlgorithm == CompressionType.Lzma)
                        ddtHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

                    var cmpCrc64Context = new Crc64Context();

                    if(_compressionAlgorithm == CompressionType.Lzma)
                        cmpCrc64Context.Update(lzmaProperties);

                    cmpCrc64Context.Update(blockStream.ToArray());
                    ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                    _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                    MemoryMarshal.Write(_structureBytes, in ddtHeader);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                    _structureBytes = null;

                    if(_compressionAlgorithm == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DeDuplicationTable,
                        dataType : DataType.CdSectorPrefixCorrected
                    });

                    _index.Add(idxEntry);

                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DeDuplicationTable,
                        dataType  = DataType.CdSectorSuffixCorrected,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CompactDisc_sector_suffix_DDT_to_position_0,
                                               idxEntry.offset);

                    ddtHeader = new DdtHeader
                    {
                        identifier  = BlockType.DeDuplicationTable,
                        type        = DataType.CdSectorSuffixCorrected,
                        compression = _compressionAlgorithm,
                        entries     = (ulong)_sectorSuffixDdt.LongLength,
                        length      = (ulong)(_sectorSuffixDdt.LongLength * sizeof(uint))
                    };

                    _crc64     = new Crc64Context();
                    ddtEntries = MemoryMarshal.Cast<uint, byte>(_sectorSuffixDdt).ToArray();
                    _crc64.Update(ddtEntries);

                    cmpBuffer = new byte[ddtEntries.Length + 262144];

                    switch(_compressionAlgorithm)
                    {
                        case CompressionType.Lzma:
                            cmpLen = LZMA.EncodeBuffer(ddtEntries, cmpBuffer, out lzmaProperties, 9, _dictionarySize, 4,
                                                       0, 2, 273);

                            break;
                        case CompressionType.None:
                            cmpBuffer = ddtEntries;
                            cmpLen    = cmpBuffer.Length;

                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                    ddtHeader.cmpLength = (uint)blockStream.Length;

                    if(_compressionAlgorithm == CompressionType.Lzma)
                        ddtHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

                    cmpCrc64Context = new Crc64Context();

                    if(_compressionAlgorithm == CompressionType.Lzma)
                        cmpCrc64Context.Update(lzmaProperties);

                    cmpCrc64Context.Update(blockStream.ToArray());
                    ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                    _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                    MemoryMarshal.Write(_structureBytes, in ddtHeader);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                    _structureBytes = null;

                    if(_compressionAlgorithm == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DeDuplicationTable,
                        dataType : DataType.CdSectorSuffixCorrected
                    });

                    _index.Add(idxEntry);

                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.CdSectorPrefixCorrected,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CD_sector_corrected_prefix_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_sectorPrefixMs.GetBuffer(), (uint)_sectorPrefixMs.Length, out byte[] blockCrc);

                    var prefixBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.CdSectorPrefixCorrected,
                        length     = (uint)_sectorPrefixMs.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 16
                    };

                    lzmaProperties = null;

                    if(!_compress)
                    {
                        prefixBlock.compression = CompressionType.None;
                        prefixBlock.cmpCrc64    = prefixBlock.crc64;
                        prefixBlock.cmpLength   = prefixBlock.length;
                        blockStream             = _sectorPrefixMs;
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] sectorPrefixBuffer = _sectorPrefixMs.ToArray();
                        cmpBuffer = new byte[sectorPrefixBuffer.Length + 262144];

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(sectorPrefixBuffer, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = sectorPrefixBuffer;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc              = cmpCrc.Final();
                        prefixBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            prefixBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        prefixBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Took_0_seconds_to_compress_prefix,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in prefixBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(prefixBlock.compression == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DataBlock,
                        dataType : DataType.CdSectorPrefixCorrected
                    });

                    _index.Add(idxEntry);

                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.CdSectorSuffixCorrected,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CD_sector_corrected_suffix_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_sectorSuffixMs.GetBuffer(), (uint)_sectorSuffixMs.Length, out blockCrc);

                    var suffixBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.CdSectorSuffixCorrected,
                        length     = (uint)_sectorSuffixMs.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 288
                    };

                    lzmaProperties = null;

                    if(!_compress)
                    {
                        suffixBlock.compression = CompressionType.None;
                        suffixBlock.cmpCrc64    = suffixBlock.crc64;
                        suffixBlock.cmpLength   = suffixBlock.length;
                        blockStream             = _sectorSuffixMs;
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] sectorSuffixBuffer = _sectorPrefixMs.ToArray();
                        cmpBuffer = new byte[sectorSuffixBuffer.Length + 262144];

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(sectorSuffixBuffer, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = sectorSuffixBuffer;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc              = cmpCrc.Final();
                        suffixBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            suffixBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        suffixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        suffixBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Took_0_seconds_to_compress_suffix,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in suffixBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(suffixBlock.compression == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DataBlock,
                        dataType : DataType.CdSectorSuffixCorrected
                    });

                    _index.Add(idxEntry);
                }

                if(_mode2Subheaders != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.CompactDiscMode2Subheader,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CD_MODE2_subheaders_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_mode2Subheaders, out byte[] blockCrc);

                    var subheaderBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.CompactDiscMode2Subheader,
                        length     = (uint)_mode2Subheaders.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 8
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        subheaderBlock.compression = CompressionType.None;
                        subheaderBlock.cmpCrc64    = subheaderBlock.crc64;
                        subheaderBlock.cmpLength   = subheaderBlock.length;
                        blockStream                = new MemoryStream(_mode2Subheaders);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_mode2Subheaders.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_mode2Subheaders, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _mode2Subheaders;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc                 = cmpCrc.Final();
                        subheaderBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            subheaderBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        subheaderBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        subheaderBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_MODE2_subheaders,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in subheaderBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(subheaderBlock.compression == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DataBlock,
                        dataType : DataType.CompactDiscMode2Subheader
                    });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }

                if(_sectorSubchannel != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.CdSectorSubchannel,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_CD_subchannel_block_to_position_0, idxEntry.offset);

                    Crc64Context.Data(_sectorSubchannel, out byte[] blockCrc);

                    var subchannelBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.CdSectorSubchannel,
                        length     = (uint)_sectorSubchannel.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 96
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        subchannelBlock.compression = CompressionType.None;
                        subchannelBlock.cmpCrc64    = subchannelBlock.crc64;
                        subchannelBlock.cmpLength   = subchannelBlock.length;
                        blockStream                 = new MemoryStream(_sectorSubchannel);
                    }
                    else
                    {
                        startCompress = DateTime.Now;
                        byte[] transformedSubchannel = ClauniaSubchannelTransform(_sectorSubchannel);

                        byte[] cmpBuffer = new byte[transformedSubchannel.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(transformedSubchannel, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = transformedSubchannel;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc                  = cmpCrc.Final();
                        subchannelBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            subchannelBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        subchannelBlock.cmpCrc64 = BitConverter.ToUInt64(blockCrc, 0);

                        subchannelBlock.compression = _compressionAlgorithm == CompressionType.Lzma
                                                          ? CompressionType.LzmaClauniaSubchannelTransform
                                                          : _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_subchannel,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in subchannelBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(subchannelBlock.compression is CompressionType.Lzma
                       or CompressionType.LzmaClauniaSubchannelTransform)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DataBlock, dataType: DataType.CdSectorSubchannel
                    });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }

                if(_sectorCprMai != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.DvdSectorCprMai,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_DVD_CPR_MAI_block_to_position_0, idxEntry.offset);

                    Crc64Context.Data(_sectorCprMai, out byte[] blockCrc);

                    var cprMaiBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.DvdSectorCprMai,
                        length     = (uint)_sectorCprMai.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 6
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        cprMaiBlock.compression = CompressionType.None;
                        cprMaiBlock.cmpCrc64    = cprMaiBlock.crc64;
                        cprMaiBlock.cmpLength   = cprMaiBlock.length;
                        blockStream             = new MemoryStream(_sectorCprMai);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorCprMai.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorCprMai, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorCprMai;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc              = cmpCrc.Final();
                        cprMaiBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cprMaiBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        cprMaiBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        cprMaiBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_CPR_MAI,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in cprMaiBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(cprMaiBlock.compression is CompressionType.Lzma
                       or CompressionType.LzmaClauniaSubchannelTransform)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is { blockType: BlockType.DataBlock, dataType: DataType.DvdSectorCprMai });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }
                
                if(_sectorId != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.DvdSectorId,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_DVD_ID_block_to_position_0, idxEntry.offset);

                    Crc64Context.Data(_sectorId, out byte[] blockCrc);

                    var idBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.DvdSectorId,
                        length     = (uint)_sectorId.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 4
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        idBlock.compression = CompressionType.None;
                        idBlock.cmpCrc64    = idBlock.crc64;
                        idBlock.cmpLength   = idBlock.length;
                        blockStream         = new MemoryStream(_sectorId);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorId.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorId, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorId;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc          = cmpCrc.Final();
                        idBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            idBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        idBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        idBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_ID,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in idBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(idBlock.compression is CompressionType.Lzma
                       or CompressionType.LzmaClauniaSubchannelTransform)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is { blockType: BlockType.DataBlock, dataType: DataType.DvdSectorId });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }
                
                if(_sectorIed != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.DvdSectorIed,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_DVD_IED_block_to_position_0, idxEntry.offset);

                    Crc64Context.Data(_sectorIed, out byte[] blockCrc);

                    var iedBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.DvdSectorIed,
                        length     = (uint)_sectorIed.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 2
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        iedBlock.compression = CompressionType.None;
                        iedBlock.cmpCrc64    = iedBlock.crc64;
                        iedBlock.cmpLength   = iedBlock.length;
                        blockStream          = new MemoryStream(_sectorIed);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorIed.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorIed, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorIed;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc           = cmpCrc.Final();
                        iedBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            iedBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        iedBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        iedBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_IED,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in iedBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(iedBlock.compression is CompressionType.Lzma
                       or CompressionType.LzmaClauniaSubchannelTransform)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is { blockType: BlockType.DataBlock, dataType: DataType.DvdSectorIed });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }
                
                if(_sectorEdc != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.DvdSectorEdc,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_DVD_EDC_block_to_position_0, idxEntry.offset);

                    Crc64Context.Data(_sectorEdc, out byte[] blockCrc);

                    var edcBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.DvdSectorEdc,
                        length     = (uint)_sectorEdc.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 4
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        edcBlock.compression = CompressionType.None;
                        edcBlock.cmpCrc64    = edcBlock.crc64;
                        edcBlock.cmpLength   = edcBlock.length;
                        blockStream          = new MemoryStream(_sectorEdc);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorEdc.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorEdc, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorEdc;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc           = cmpCrc.Final();
                        edcBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            edcBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        edcBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        edcBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_EDC,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in edcBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(edcBlock.compression is CompressionType.Lzma
                       or CompressionType.LzmaClauniaSubchannelTransform)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is { blockType: BlockType.DataBlock, dataType: DataType.DvdSectorEdc });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }

                if(_sectorDecryptedTitleKey != null)
                {
                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = DataType.DvdSectorTitleKeyDecrypted,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_decrypted_DVD_title_key_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_sectorDecryptedTitleKey, out byte[] blockCrc);

                    var titleKeyBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = DataType.DvdSectorTitleKeyDecrypted,
                        length     = (uint)_sectorDecryptedTitleKey.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0),
                        sectorSize = 5
                    };

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        titleKeyBlock.compression = CompressionType.None;
                        titleKeyBlock.cmpCrc64    = titleKeyBlock.crc64;
                        titleKeyBlock.cmpLength   = titleKeyBlock.length;
                        blockStream               = new MemoryStream(_sectorDecryptedTitleKey);
                    }
                    else
                    {
                        startCompress = DateTime.Now;

                        byte[] cmpBuffer = new byte[_sectorDecryptedTitleKey.Length + 262144];

                        int cmpLen;

                        switch(_compressionAlgorithm)
                        {
                            case CompressionType.Lzma:
                                cmpLen = LZMA.EncodeBuffer(_sectorDecryptedTitleKey, cmpBuffer, out lzmaProperties, 9,
                                                           _dictionarySize, 4, 0, 2, 273);

                                break;
                            case CompressionType.None:
                                cmpBuffer = _sectorDecryptedTitleKey;
                                cmpLen    = cmpBuffer.Length;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc                = cmpCrc.Final();
                        titleKeyBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            titleKeyBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        titleKeyBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        titleKeyBlock.compression = _compressionAlgorithm;

                        endCompress = DateTime.Now;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   Localization.Took_0_seconds_to_compress_decrypted_DVD_title_keys,
                                                   (endCompress - startCompress).TotalSeconds);
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in titleKeyBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(titleKeyBlock.compression is CompressionType.Lzma
                       or CompressionType.LzmaClauniaSubchannelTransform)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.DataBlock,
                        dataType : DataType.DvdSectorTitleKeyDecrypted
                    });

                    _index.Add(idxEntry);
                    blockStream.Close();
                }

                List<TrackEntry>            trackEntries            = new();
                List<CompactDiscIndexEntry> compactDiscIndexEntries = new();

                foreach(Track track in Tracks)
                {
                    _trackFlags.TryGetValue((byte)track.Sequence, out byte flags);
                    _trackIsrcs.TryGetValue((byte)track.Sequence, out string isrc);

                    if((flags & (int)CdFlags.DataTrack) == 0 &&
                       track.Type                       != TrackType.Audio)
                        flags += (byte)CdFlags.DataTrack;

                    trackEntries.Add(new TrackEntry
                    {
                        sequence = (byte)track.Sequence,
                        type     = track.Type,
                        start    = (long)track.StartSector,
                        end      = (long)track.EndSector,
                        pregap   = (long)track.Pregap,
                        session  = (byte)track.Session,
                        isrc     = isrc,
                        flags    = flags
                    });

                    switch(track.Indexes.ContainsKey(0))
                    {
                        case false when track.Pregap > 0:
                            track.Indexes[0] = (int)track.StartSector;
                            track.Indexes[1] = (int)(track.StartSector + track.Pregap);

                            break;
                        case false when !track.Indexes.ContainsKey(1):
                            track.Indexes[0] = (int)track.StartSector;

                            break;
                    }

                    compactDiscIndexEntries.AddRange(track.Indexes.Select(trackIndex => new CompactDiscIndexEntry
                    {
                        Index = trackIndex.Key,
                        Lba   = trackIndex.Value,
                        Track = (ushort)track.Sequence
                    }));
                }

                // If there are tracks build the tracks block
                if(trackEntries.Count > 0)
                {
                    blockStream = new MemoryStream();

                    foreach(TrackEntry entry in trackEntries)
                    {
                        _structurePointer =
                            System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<TrackEntry>());

                        _structureBytes = new byte[Marshal.SizeOf<TrackEntry>()];
                        System.Runtime.InteropServices.Marshal.StructureToPtr(entry, _structurePointer, true);

                        System.Runtime.InteropServices.Marshal.Copy(_structurePointer, _structureBytes, 0,
                                                                    _structureBytes.Length);

                        System.Runtime.InteropServices.Marshal.FreeHGlobal(_structurePointer);
                        blockStream.Write(_structureBytes, 0, _structureBytes.Length);
                    }

                    Crc64Context.Data(blockStream.ToArray(), out byte[] trksCrc);

                    var trkHeader = new TracksHeader
                    {
                        identifier = BlockType.TracksBlock,
                        entries    = (ushort)trackEntries.Count,
                        crc64      = BitConverter.ToUInt64(trksCrc, 0)
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_tracks_to_position_0,
                                               _imageStream.Position);

                    _index.RemoveAll(t => t is { blockType: BlockType.TracksBlock, dataType: DataType.NoData });

                    _index.Add(new IndexEntry
                    {
                        blockType = BlockType.TracksBlock,
                        dataType  = DataType.NoData,
                        offset    = (ulong)_imageStream.Position
                    });

                    _structureBytes = new byte[Marshal.SizeOf<TracksHeader>()];
                    MemoryMarshal.Write(_structureBytes, in trkHeader);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();
                }

                // If there are track indexes bigger than 1
                if(compactDiscIndexEntries.Any(i => i.Index > 1))
                {
                    blockStream = new MemoryStream();

                    foreach(CompactDiscIndexEntry entry in compactDiscIndexEntries)
                    {
                        _structurePointer =
                            System.Runtime.InteropServices.Marshal.
                                   AllocHGlobal(Marshal.SizeOf<CompactDiscIndexEntry>());

                        _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexEntry>()];
                        System.Runtime.InteropServices.Marshal.StructureToPtr(entry, _structurePointer, true);

                        System.Runtime.InteropServices.Marshal.Copy(_structurePointer, _structureBytes, 0,
                                                                    _structureBytes.Length);

                        System.Runtime.InteropServices.Marshal.FreeHGlobal(_structurePointer);
                        blockStream.Write(_structureBytes, 0, _structureBytes.Length);
                    }

                    Crc64Context.Data(blockStream.ToArray(), out byte[] cdixCrc);

                    var cdixHeader = new CompactDiscIndexesHeader
                    {
                        identifier = BlockType.CompactDiscIndexesBlock,
                        entries    = (ushort)compactDiscIndexEntries.Count,
                        crc64      = BitConverter.ToUInt64(cdixCrc, 0)
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_compact_disc_indexes_to_position_0,
                                               _imageStream.Position);

                    _index.RemoveAll(t => t is
                    {
                        blockType: BlockType.CompactDiscIndexesBlock,
                        dataType : DataType.NoData
                    });

                    _index.Add(new IndexEntry
                    {
                        blockType = BlockType.CompactDiscIndexesBlock,
                        dataType  = DataType.NoData,
                        offset    = (ulong)_imageStream.Position
                    });

                    _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexesHeader>()];
                    MemoryMarshal.Write(_structureBytes, in cdixHeader);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    blockStream.Close();
                }

                break;
            case MetadataMediaType.BlockMedia:
                if(_sectorSubchannel != null &&
                   _imageInfo.MediaType is MediaType.AppleFileWare or MediaType.AppleSonySS or MediaType.AppleSonyDS
                       or MediaType.AppleProfile or MediaType.AppleWidget or MediaType.PriamDataTower)
                {
                    DataType tagType = DataType.NoData;

                    switch(_imageInfo.MediaType)
                    {
                        case MediaType.AppleSonySS:
                        case MediaType.AppleSonyDS:
                            tagType = DataType.AppleSonyTag;

                            break;
                        case MediaType.AppleFileWare:
                        case MediaType.AppleProfile:
                        case MediaType.AppleWidget:
                            tagType = DataType.AppleProfileTag;

                            break;
                        case MediaType.PriamDataTower:
                            tagType = DataType.PriamDataTowerTag;

                            break;
                    }

                    idxEntry = new IndexEntry
                    {
                        blockType = BlockType.DataBlock,
                        dataType  = tagType,
                        offset    = (ulong)_imageStream.Position
                    };

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               Localization.Writing_apple_sector_tag_block_to_position_0,
                                               idxEntry.offset);

                    Crc64Context.Data(_sectorSubchannel, out byte[] blockCrc);

                    var subchannelBlock = new BlockHeader
                    {
                        identifier = BlockType.DataBlock,
                        type       = tagType,
                        length     = (uint)_sectorSubchannel.Length,
                        crc64      = BitConverter.ToUInt64(blockCrc, 0)
                    };

                    switch(_imageInfo.MediaType)
                    {
                        case MediaType.AppleSonySS:
                        case MediaType.AppleSonyDS:
                            subchannelBlock.sectorSize = 12;

                            break;
                        case MediaType.AppleFileWare:
                        case MediaType.AppleProfile:
                        case MediaType.AppleWidget:
                            subchannelBlock.sectorSize = 20;

                            break;
                        case MediaType.PriamDataTower:
                            subchannelBlock.sectorSize = 24;

                            break;
                    }

                    byte[] lzmaProperties = null;

                    if(!_compress)
                    {
                        subchannelBlock.compression = CompressionType.None;
                        subchannelBlock.cmpCrc64    = subchannelBlock.crc64;
                        subchannelBlock.cmpLength   = subchannelBlock.length;
                        blockStream                 = new MemoryStream(_sectorSubchannel);
                    }
                    else
                    {
                        byte[] cmpBuffer = new byte[_sectorSubchannel.Length + 262144];

                        int cmpLen = _compressionAlgorithm switch
                        {
                            CompressionType.Lzma => LZMA.EncodeBuffer(_sectorSubchannel, cmpBuffer, out lzmaProperties,
                                                                      9, _dictionarySize, 4, 0, 2, 273),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        blockStream = new MemoryStream(cmpBuffer, 0, cmpLen);

                        var cmpCrc = new Crc64Context();

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            cmpCrc.Update(lzmaProperties);

                        cmpCrc.Update(blockStream.ToArray());
                        blockCrc                  = cmpCrc.Final();
                        subchannelBlock.cmpLength = (uint)blockStream.Length;

                        if(_compressionAlgorithm == CompressionType.Lzma)
                            subchannelBlock.cmpLength += LZMA_PROPERTIES_LENGTH;

                        subchannelBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        subchannelBlock.compression = _compressionAlgorithm;
                    }

                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    MemoryMarshal.Write(_structureBytes, in subchannelBlock);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                    if(subchannelBlock.compression == CompressionType.Lzma)
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                    _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                    _index.RemoveAll(t => t.blockType == BlockType.DataBlock && t.dataType == tagType);

                    _index.Add(idxEntry);
                    blockStream.Close();
                }

                break;
        }

        // Write metadata if present
        SetMetadataFromTags();
        var metadataBlock = new MetadataBlock();
        blockStream = new MemoryStream();
        blockStream.Write(new byte[Marshal.SizeOf<MetadataBlock>()], 0, Marshal.SizeOf<MetadataBlock>());
        byte[] tmpUtf16Le;

        if(_imageInfo is { MediaSequence: > 0, LastMediaSequence: > 0 })
        {
            metadataBlock.identifier        = BlockType.MetadataBlock;
            metadataBlock.mediaSequence     = _imageInfo.MediaSequence;
            metadataBlock.lastMediaSequence = _imageInfo.LastMediaSequence;
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.Creator))
        {
            tmpUtf16Le                  = Encoding.Unicode.GetBytes(_imageInfo.Creator);
            metadataBlock.identifier    = BlockType.MetadataBlock;
            metadataBlock.creatorOffset = (uint)blockStream.Position;
            metadataBlock.creatorLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.Comments))
        {
            tmpUtf16Le                   = Encoding.Unicode.GetBytes(_imageInfo.Comments);
            metadataBlock.identifier     = BlockType.MetadataBlock;
            metadataBlock.commentsOffset = (uint)blockStream.Position;
            metadataBlock.commentsLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.MediaTitle))
        {
            tmpUtf16Le                     = Encoding.Unicode.GetBytes(_imageInfo.MediaTitle);
            metadataBlock.identifier       = BlockType.MetadataBlock;
            metadataBlock.mediaTitleOffset = (uint)blockStream.Position;
            metadataBlock.mediaTitleLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.MediaManufacturer))
        {
            tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.MediaManufacturer);
            metadataBlock.identifier              = BlockType.MetadataBlock;
            metadataBlock.mediaManufacturerOffset = (uint)blockStream.Position;
            metadataBlock.mediaManufacturerLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.MediaModel))
        {
            tmpUtf16Le                     = Encoding.Unicode.GetBytes(_imageInfo.MediaModel);
            metadataBlock.identifier       = BlockType.MetadataBlock;
            metadataBlock.mediaModelOffset = (uint)blockStream.Position;
            metadataBlock.mediaModelLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.MediaSerialNumber))
        {
            tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.MediaSerialNumber);
            metadataBlock.identifier              = BlockType.MetadataBlock;
            metadataBlock.mediaSerialNumberOffset = (uint)blockStream.Position;
            metadataBlock.mediaSerialNumberLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.MediaBarcode))
        {
            tmpUtf16Le                       = Encoding.Unicode.GetBytes(_imageInfo.MediaBarcode);
            metadataBlock.identifier         = BlockType.MetadataBlock;
            metadataBlock.mediaBarcodeOffset = (uint)blockStream.Position;
            metadataBlock.mediaBarcodeLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.MediaPartNumber))
        {
            tmpUtf16Le                          = Encoding.Unicode.GetBytes(_imageInfo.MediaPartNumber);
            metadataBlock.identifier            = BlockType.MetadataBlock;
            metadataBlock.mediaPartNumberOffset = (uint)blockStream.Position;
            metadataBlock.mediaPartNumberLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
        {
            tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.DriveManufacturer);
            metadataBlock.identifier              = BlockType.MetadataBlock;
            metadataBlock.driveManufacturerOffset = (uint)blockStream.Position;
            metadataBlock.driveManufacturerLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
        {
            tmpUtf16Le                     = Encoding.Unicode.GetBytes(_imageInfo.DriveModel);
            metadataBlock.identifier       = BlockType.MetadataBlock;
            metadataBlock.driveModelOffset = (uint)blockStream.Position;
            metadataBlock.driveModelLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
        {
            tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.DriveSerialNumber);
            metadataBlock.identifier              = BlockType.MetadataBlock;
            metadataBlock.driveSerialNumberOffset = (uint)blockStream.Position;
            metadataBlock.driveSerialNumberLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        if(!string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
        {
            tmpUtf16Le                                = Encoding.Unicode.GetBytes(_imageInfo.DriveFirmwareRevision);
            metadataBlock.identifier                  = BlockType.MetadataBlock;
            metadataBlock.driveFirmwareRevisionOffset = (uint)blockStream.Position;
            metadataBlock.driveFirmwareRevisionLength = (uint)(tmpUtf16Le.Length + 2);
            blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

            blockStream.Write(new byte[]
            {
                0, 0
            }, 0, 2);
        }

        // Check if we set up any metadata earlier, then write its block
        if(metadataBlock.identifier == BlockType.MetadataBlock)
        {
            AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_metadata_to_position_0,
                                       _imageStream.Position);

            metadataBlock.blockSize = (uint)blockStream.Length;
            _structureBytes         = new byte[Marshal.SizeOf<MetadataBlock>()];
            MemoryMarshal.Write(_structureBytes, in metadataBlock);
            blockStream.Position = 0;
            blockStream.Write(_structureBytes, 0, _structureBytes.Length);
            _index.RemoveAll(t => t is { blockType: BlockType.MetadataBlock, dataType: DataType.NoData });

            _index.Add(new IndexEntry
            {
                blockType = BlockType.MetadataBlock,
                dataType  = DataType.NoData,
                offset    = (ulong)_imageStream.Position
            });

            _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
            blockStream.Close();
        }

        _header.indexOffset = (ulong)_imageStream.Position;

        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_index_to_position_0, _header.indexOffset);

        blockStream = new MemoryStream();

        // Write index to memory
        foreach(IndexEntry entry in _index)
        {
            _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
            IndexEntry indexEntry = entry;
            MemoryMarshal.Write(_structureBytes, in indexEntry);
            blockStream.Write(_structureBytes, 0, _structureBytes.Length);
        }

        Crc64Context.Data(blockStream.ToArray(), out byte[] idxCrc);

        if(_index.Count > ushort.MaxValue)
        {
            _header.imageMajorVersion = AARUFMT_VERSION;

            var idxHeader = new IndexHeader2
            {
                identifier = BlockType.Index2,
                entries    = (ulong)_index.Count,
                crc64      = BitConverter.ToUInt64(idxCrc, 0)
            };

            // Write index header to disk
            _structureBytes = new byte[Marshal.SizeOf<IndexHeader2>()];
            MemoryMarshal.Write(_structureBytes, in idxHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
        }
        else
        {
            var idxHeader = new IndexHeader
            {
                identifier = BlockType.Index,
                entries    = (ushort)_index.Count,
                crc64      = BitConverter.ToUInt64(idxCrc, 0)
            };

            // Write index header to disk
            _structureBytes = new byte[Marshal.SizeOf<IndexHeader>()];
            MemoryMarshal.Write(_structureBytes, in idxHeader);
            _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
        }

        // Write index to disk
        _imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
        blockStream.Close();

        AaruConsole.DebugWriteLine("Aaru Format plugin", Localization.Writing_header);
        _header.lastWrittenTime = DateTime.UtcNow.ToFileTimeUtc();
        _imageStream.Position   = 0;
        _structurePointer       = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<AaruHeader>());
        _structureBytes         = new byte[Marshal.SizeOf<AaruHeader>()];
        System.Runtime.InteropServices.Marshal.StructureToPtr(_header, _structurePointer, true);
        System.Runtime.InteropServices.Marshal.Copy(_structurePointer, _structureBytes, 0, _structureBytes.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(_structurePointer);
        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

        _imageStream.Flush();
        _imageStream.Close();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo)
    {
        _imageInfo.Creator               = imageInfo.Creator;
        _imageInfo.Comments              = imageInfo.Comments;
        _imageInfo.MediaManufacturer     = imageInfo.MediaManufacturer;
        _imageInfo.MediaModel            = imageInfo.MediaModel;
        _imageInfo.MediaSerialNumber     = imageInfo.MediaSerialNumber;
        _imageInfo.MediaBarcode          = imageInfo.MediaBarcode;
        _imageInfo.MediaPartNumber       = imageInfo.MediaPartNumber;
        _imageInfo.MediaSequence         = imageInfo.MediaSequence;
        _imageInfo.LastMediaSequence     = imageInfo.LastMediaSequence;
        _imageInfo.DriveManufacturer     = imageInfo.DriveManufacturer;
        _imageInfo.DriveModel            = imageInfo.DriveModel;
        _imageInfo.DriveSerialNumber     = imageInfo.DriveSerialNumber;
        _imageInfo.DriveFirmwareRevision = imageInfo.DriveFirmwareRevision;
        _imageInfo.MediaTitle            = imageInfo.MediaTitle;

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(_imageInfo.MetadataMediaType != MetadataMediaType.BlockMedia)
        {
            ErrorMessage = Localization.Tried_to_set_geometry_on_a_media_that_doesnt_support_it;

            return false;
        }

        _geometryBlock = new GeometryBlock
        {
            identifier      = BlockType.GeometryBlock,
            cylinders       = cylinders,
            heads           = heads,
            sectorsPerTrack = sectorsPerTrack
        };

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(sectorAddress >= _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        Track track;

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
            case SectorTagType.CdTrackIsrc:
                if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
                {
                    ErrorMessage = Localization.Incorrect_tag_for_disk_type;

                    return false;
                }

                track = Tracks.FirstOrDefault(trk => sectorAddress == trk.Sequence);

                if(track is null ||
                   (track.Sequence == 0 && track.StartSector == 0 && track.EndSector == 0))
                {
                    ErrorMessage = string.Format(Localization.Cant_find_track_0, sectorAddress);

                    return false;
                }

                break;
            case SectorTagType.CdSectorSubchannel:
                if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
                {
                    ErrorMessage = Localization.Incorrect_tag_for_disk_type;

                    return false;
                }

                track = Tracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector &&
                                                     sectorAddress <= trk.EndSector);

                if(track is { Sequence: 0, StartSector: 0, EndSector: 0 })
                    track.Type = TrackType.Data;

                break;
        }

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
            {
                if(data.Length != 1)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_track_flags;

                    return false;
                }

                _trackFlags[(byte)sectorAddress] = data[0];

                return true;
            }

            case SectorTagType.CdTrackIsrc:
            {
                if(data != null)
                    _trackIsrcs[(byte)sectorAddress] = Encoding.UTF8.GetString(data);

                return true;
            }

            case SectorTagType.CdSectorSubchannel:
            {
                if(data.Length != 96)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_subchannel;

                    return false;
                }

                _sectorSubchannel ??= new byte[_imageInfo.Sectors * 96];

                Array.Copy(data, 0, _sectorSubchannel, (int)(96 * sectorAddress), 96);

                return true;
            }

            case SectorTagType.DvdSectorCmi:
            {
                if(data.Length != 1)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_CMI;

                    return false;
                }

                _sectorCprMai ??= new byte[_imageInfo.Sectors * 6];

                Array.Copy(data, 0, _sectorCprMai, (int)(6 * sectorAddress), 1);

                return true;
            }
            
            case SectorTagType.DvdSectorTitleKey:
            {
                if(data.Length != 5)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_title_key;

                    return false;
                }

                _sectorCprMai ??= new byte[_imageInfo.Sectors * 6];

                Array.Copy(data, 0, _sectorCprMai, (int)(1 + (6 * sectorAddress)), 5);

                return true;
            }
            
            case SectorTagType.DvdSectorInformation:
            {
                if(data.Length != 1)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_dvd_id_information;

                    return false;
                }

                _sectorId ??= new byte[_imageInfo.Sectors * 4];

                Array.Copy(data, 0, _sectorId, (int)(4 * sectorAddress), 1);

                return true;
            }
            
            case SectorTagType.DvdSectorNumber:
            {
                if(data.Length != 3)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_dvd_id_number;

                    return false;
                }

                _sectorId ??= new byte[_imageInfo.Sectors * 4];

                Array.Copy(data, 0, _sectorId, (int)(1 + (4 * sectorAddress)), 3);

                return true;
            }
            
            case SectorTagType.DvdSectorIed:
            {
                if(data.Length != 2)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_ied;

                    return false;
                }

                _sectorIed ??= new byte[_imageInfo.Sectors * 2];

                Array.Copy(data, 0, _sectorIed, (int)(2 * sectorAddress), 2);

                return true;
            }
            
            case SectorTagType.DvdSectorEdc:
            {
                if(data.Length != 4)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_edc;

                    return false;
                }

                _sectorEdc ??= new byte[_imageInfo.Sectors * 4];

                Array.Copy(data, 0, _sectorEdc, (int)(4 * sectorAddress), 4);

                return true;
            }
            
            case SectorTagType.DvdTitleKeyDecrypted:
            {
                if(data.Length != 5)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_decrypted_title_key;

                    return false;
                }

                _sectorDecryptedTitleKey ??= new byte[_imageInfo.Sectors * 5];

                Array.Copy(data, 0, _sectorDecryptedTitleKey, (int)(5 * sectorAddress), 5);

                return true;
            }

            default:
                ErrorMessage = string.Format(Localization.Dont_know_how_to_write_sector_tag_type_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
            case SectorTagType.CdTrackIsrc: return WriteSectorTag(data, sectorAddress, tag);
            case SectorTagType.CdSectorSubchannel:
            {
                if(data.Length % 96 != 0)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_subchannel;

                    return false;
                }

                _sectorSubchannel ??= new byte[_imageInfo.Sectors * 96];

                if((sectorAddress * 96) + (length * 96) > (ulong)_sectorSubchannel.LongLength)
                {
                    ErrorMessage = Localization.Tried_to_write_more_data_than_possible;

                    return false;
                }

                Array.Copy(data, 0, _sectorSubchannel, (int)(96 * sectorAddress), 96 * length);

                return true;
            }

            default:
                ErrorMessage = string.Format(Localization.Dont_know_how_to_write_sector_tag_type_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware)
    {
        DumpHardware = dumpHardware;

        return true;
    }

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata)
    {
        AaruMetadata = metadata;

        return true;
    }
}