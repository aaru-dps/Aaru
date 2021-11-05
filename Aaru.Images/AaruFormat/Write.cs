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
// Copyright © 2011-2021 Natalia Portillo
// Copyright © 2020-2021 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using CUETools.Codecs;
using CUETools.Codecs.Flake;
using Schemas;
using SharpCompress.Compressors.LZMA;
using Marshal = Aaru.Helpers.Marshal;
using Session = Aaru.CommonTypes.Structs.Session;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public sealed partial class AaruFormat
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            uint sectorsPerBlock;
            uint dictionary;
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
                        ErrorMessage = "Invalid value for sectors_per_block option";

                        return false;
                    }
                }
                else
                    sectorsPerBlock = 4096;

                if(options.TryGetValue("dictionary", out tmpValue))
                {
                    if(!uint.TryParse(tmpValue, out dictionary))
                    {
                        ErrorMessage = "Invalid value for dictionary option";

                        return false;
                    }
                }
                else
                    dictionary = 1 << 25;

                if(options.TryGetValue("max_ddt_size", out tmpValue))
                {
                    if(!uint.TryParse(tmpValue, out maxDdtSize))
                    {
                        ErrorMessage = "Invalid value for max_ddt_size option";

                        return false;
                    }
                }
                else
                    maxDdtSize = 256;

                if(options.TryGetValue("md5", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out doMd5))
                    {
                        ErrorMessage = "Invalid value for md5 option";

                        return false;
                    }
                }
                else
                    doMd5 = true;

                if(options.TryGetValue("sha1", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out doSha1))
                    {
                        ErrorMessage = "Invalid value for sha1 option";

                        return false;
                    }
                }
                else
                    doSha1 = true;

                if(options.TryGetValue("sha256", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out doSha256))
                    {
                        ErrorMessage = "Invalid value for sha256 option";

                        return false;
                    }
                }
                else
                    doSha256 = true;

                if(options.TryGetValue("spamsum", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out doSpamsum))
                    {
                        ErrorMessage = "Invalid value for spamsum option";

                        return false;
                    }
                }
                else
                    doSpamsum = false;

                if(options.TryGetValue("deduplicate", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out _deduplicate))
                    {
                        ErrorMessage = "Invalid value for deduplicate option";

                        return false;
                    }
                }
                else
                    _deduplicate = true;

                if(options.TryGetValue("compress", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out _compress))
                    {
                        ErrorMessage = "Invalid value for compress option";

                        return false;
                    }
                }
                else
                    _compress = true;
            }
            else
            {
                sectorsPerBlock = 4096;
                dictionary      = 1 << 25;
                maxDdtSize      = 256;
                doMd5           = true;
                doSha1          = true;
                doSha256        = true;
                doSpamsum       = false;
                _deduplicate    = true;
                _compress       = true;
            }

            // This really, cannot happen
            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupported media format {mediaType}";

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

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Got a shift of {0} for {1} sectors per block", _shift,
                                       oldSectorsPerBlock);

            _imageInfo = new ImageInfo
            {
                MediaType    = mediaType,
                SectorSize   = sectorSize,
                Sectors      = sectors,
                XmlMediaType = GetXmlMediaType(mediaType)
            };

            try
            {
                _imageStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            // Check if appending to an existing image
            if(_imageStream.Length > Marshal.SizeOf<AaruHeader>())
            {
                _structureBytes = new byte[Marshal.SizeOf<AaruHeader>()];
                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                _header = Marshal.ByteArrayToStructureLittleEndian<AaruHeader>(_structureBytes);

                if(_header.identifier != DIC_MAGIC &&
                   _header.identifier != AARU_MAGIC)
                {
                    ErrorMessage = "Cannot append to a non Aaru Format image";

                    return false;
                }

                if(_header.imageMajorVersion > AARUFMT_VERSION)
                {
                    ErrorMessage = $"Cannot append to an unknown image version {_header.imageMajorVersion}";

                    return false;
                }

                if(_header.mediaType != mediaType)
                {
                    ErrorMessage =
                        $"Cannot write a media with type {mediaType} to an image with type {_header.mediaType}";

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
                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                IndexHeader idxHeader = Marshal.SpanToStructureLittleEndian<IndexHeader>(_structureBytes);

                if(idxHeader.identifier != BlockType.Index)
                {
                    ErrorMessage = "Index not found in existing image, cannot continue";

                    return false;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries",
                                           _header.indexOffset, idxHeader.entries);

                for(ushort i = 0; i < idxHeader.entries; i++)
                {
                    _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                    _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                    IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(_structureBytes);

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               "Block type {0} with data type {1} is indexed to be at {2}",
                                               entry.blockType, entry.dataType, entry.offset);

                    _index.Add(entry);
                }

                // Invalidate previous checksum block
                _index.RemoveAll(t => t.blockType == BlockType.ChecksumBlock && t.dataType == DataType.NoData);

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
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                            BlockHeader blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(_structureBytes);
                            _imageInfo.ImageSize += blockHeader.cmpLength;

                            // Unused, skip
                            if(entry.dataType == DataType.UserData)
                                break;

                            if(blockHeader.identifier != entry.blockType)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect identifier for data block at position {0}",
                                                           entry.offset);

                                break;
                            }

                            if(blockHeader.type != entry.dataType)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Expected block with data type {0} at position {1} but found data type {2}",
                                                           entry.dataType, entry.offset, blockHeader.type);

                                break;
                            }

                            byte[] data;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Found data block type {0} at position {1}", entry.dataType,
                                                       entry.offset);

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            // Decompress media tag
                            if(blockHeader.compression == CompressionType.Lzma ||
                               blockHeader.compression == CompressionType.LzmaClauniaSubchannelTransform)
                            {
                                if(blockHeader.compression == CompressionType.LzmaClauniaSubchannelTransform &&
                                   entry.dataType          != DataType.CdSectorSubchannel)
                                {
                                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                               "Invalid compression type {0} for block with data type {1}, continuing...",
                                                               blockHeader.compression, entry.dataType);

                                    break;
                                }

                                DateTime startDecompress = DateTime.Now;
                                byte[]   compressedTag   = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                byte[]   lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                                _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                _imageStream.Read(compressedTag, 0, compressedTag.Length);
                                var compressedTagMs = new MemoryStream(compressedTag);
                                var lzmaBlock       = new LzmaStream(lzmaProperties, compressedTagMs);
                                data = new byte[blockHeader.length];
                                lzmaBlock.Read(data, 0, (int)blockHeader.length);
                                lzmaBlock.Close();
                                compressedTagMs.Close();

                                if(blockHeader.compression == CompressionType.LzmaClauniaSubchannelTransform)
                                    data = ClauniaSubchannelUntransform(data);

                                DateTime endDecompress = DateTime.Now;

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to decompress block",
                                                           (endDecompress - startDecompress).TotalSeconds);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));
                            }
                            else if(blockHeader.compression == CompressionType.None)
                            {
                                data = new byte[blockHeader.length];
                                _imageStream.Read(data, 0, (int)blockHeader.length);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));
                            }
                            else
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Found unknown compression type {0}, continuing...",
                                                           (ushort)blockHeader.compression);

                                break;
                            }

                            // Check CRC, if not correct, skip it
                            Crc64Context.Data(data, out byte[] blockCrc);

                            if(BitConverter.ToUInt64(blockCrc, 0) != blockHeader.crc64 &&
                               blockHeader.crc64                  != 0)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
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
                                        _sectorPrefixMs = new NonClosableStream();
                                        _sectorPrefixMs.Write(data, 0, data.Length);
                                    }
                                    else
                                        _sectorPrefix = data;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case DataType.CdSectorSuffix:
                                case DataType.CdSectorSuffixCorrected:
                                    if(entry.dataType == DataType.CdSectorSuffixCorrected)
                                    {
                                        _sectorSuffixMs = new NonClosableStream();
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

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case DataType.CdSectorSubchannel:
                                    _sectorSubchannel = data;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case DataType.AppleProfileTag:
                                case DataType.AppleSonyTag:
                                case DataType.PriamDataTowerTag:
                                    _sectorSubchannel = data;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case DataType.CompactDiscMode2Subheader:
                                    _mode2Subheaders = data;

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case DataType.DvdSectorCpiMai:
                                    _sectorCpiMai = data;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.DvdCmi))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.DvdCmi);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.DvdTitleKey))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.DvdTitleKey);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case DataType.DvdSectorTitleKeyDecrypted:
                                    _sectorDecryptedTitleKey = data;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.DvdTitleKeyDecrypted))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.DvdTitleKeyDecrypted);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                default:
                                    MediaTagType mediaTagType = GetMediaTagTypeForDataType(blockHeader.type);

                                    if(_mediaTags.ContainsKey(mediaTagType))
                                    {
                                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                                   "Media tag type {0} duplicated, removing previous entry...",
                                                                   mediaTagType);

                                        _mediaTags.Remove(mediaTagType);
                                    }

                                    _mediaTags.Add(mediaTagType, data);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                            }

                            break;
                        case BlockType.DeDuplicationTable:
                            // Only user data deduplication tables are used right now
                            if(entry.dataType == DataType.UserData)
                            {
                                _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                                DdtHeader ddtHeader =
                                    Marshal.ByteArrayToStructureLittleEndian<DdtHeader>(_structureBytes);

                                if(ddtHeader.identifier != BlockType.DeDuplicationTable)
                                    break;

                                if(ddtHeader.entries != _imageInfo.Sectors &&
                                   !IsTape)
                                {
                                    ErrorMessage =
                                        $"Trying to write a media with {_imageInfo.Sectors} sectors to an image with {ddtHeader.entries} sectors, not continuing...";

                                    return false;
                                }

                                _shift = ddtHeader.shift;

                                switch(ddtHeader.compression)
                                {
                                    case CompressionType.Lzma:
                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");

                                        DateTime ddtStart = DateTime.UtcNow;

                                        byte[] compressedDdt = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];

                                        byte[] lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                        _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                        _imageStream.Read(compressedDdt, 0, compressedDdt.Length);
                                        var    compressedDdtMs = new MemoryStream(compressedDdt);
                                        var    lzmaDdt         = new LzmaStream(lzmaProperties, compressedDdtMs);
                                        byte[] decompressedDdt = new byte[ddtHeader.length];
                                        lzmaDdt.Read(decompressedDdt, 0, (int)ddtHeader.length);
                                        lzmaDdt.Close();
                                        compressedDdtMs.Close();
                                        _userDataDdt = MemoryMarshal.Cast<byte, ulong>(decompressedDdt).ToArray();
                                        DateTime ddtEnd = DateTime.UtcNow;
                                        _inMemoryDdt = true;

                                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                                   "Took {0} seconds to decompress DDT",
                                                                   (ddtEnd - ddtStart).TotalSeconds);

                                        break;
                                    case CompressionType.None:
                                        _inMemoryDdt          = false;
                                        _outMemoryDdtPosition = (long)entry.offset;

                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");
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
                            else if(entry.dataType == DataType.CdSectorPrefixCorrected ||
                                    entry.dataType == DataType.CdSectorSuffixCorrected)
                            {
                                _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                                DdtHeader ddtHeader =
                                    Marshal.ByteArrayToStructureLittleEndian<DdtHeader>(_structureBytes);

                                if(ddtHeader.identifier != BlockType.DeDuplicationTable)
                                    break;

                                if(ddtHeader.entries != _imageInfo.Sectors)
                                {
                                    ErrorMessage =
                                        $"Trying to write a media with {_imageInfo.Sectors} sectors to an image with {ddtHeader.entries} sectors, not continuing...";

                                    return false;
                                }

                                byte[] decompressedDdt = new byte[ddtHeader.length];

                                switch(ddtHeader.compression)
                                {
                                    case CompressionType.Lzma:
                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");

                                        DateTime ddtStart = DateTime.UtcNow;

                                        byte[] compressedDdt = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];

                                        byte[] lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                        _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                        _imageStream.Read(compressedDdt, 0, compressedDdt.Length);
                                        var compressedDdtMs = new MemoryStream(compressedDdt);
                                        var lzmaDdt         = new LzmaStream(lzmaProperties, compressedDdtMs);
                                        lzmaDdt.Read(decompressedDdt, 0, (int)ddtHeader.length);
                                        lzmaDdt.Close();
                                        compressedDdtMs.Close();
                                        DateTime ddtEnd = DateTime.UtcNow;

                                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                                   "Took {0} seconds to decompress DDT",
                                                                   (ddtEnd - ddtStart).TotalSeconds);

                                        break;
                                    case CompressionType.None:
                                        _imageStream.Read(decompressedDdt, 0, decompressedDdt.Length);

                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");
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
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                            _geometryBlock = Marshal.SpanToStructureLittleEndian<GeometryBlock>(_structureBytes);

                            if(_geometryBlock.identifier == BlockType.GeometryBlock)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Geometry set to {0} cylinders {1} heads {2} sectors per track",
                                                           _geometryBlock.cylinders, _geometryBlock.heads,
                                                           _geometryBlock.sectorsPerTrack);

                                _imageInfo.Cylinders       = _geometryBlock.cylinders;
                                _imageInfo.Heads           = _geometryBlock.heads;
                                _imageInfo.SectorsPerTrack = _geometryBlock.sectorsPerTrack;

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));
                            }

                            break;

                        // Metadata block
                        case BlockType.MetadataBlock:
                            _structureBytes = new byte[Marshal.SizeOf<MetadataBlock>()];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            MetadataBlock metadataBlock =
                                Marshal.SpanToStructureLittleEndian<MetadataBlock>(_structureBytes);

                            if(metadataBlock.identifier != entry.blockType)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect identifier for data block at position {0}",
                                                           entry.offset);

                                break;
                            }

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Found metadata block at position {0}",
                                                       entry.offset);

                            byte[] metadata = new byte[metadataBlock.blockSize];
                            _imageStream.Position = (long)entry.offset;
                            _imageStream.Read(metadata, 0, metadata.Length);

                            if(metadataBlock.mediaSequence     > 0 &&
                               metadataBlock.lastMediaSequence > 0)
                            {
                                _imageInfo.MediaSequence     = metadataBlock.mediaSequence;
                                _imageInfo.LastMediaSequence = metadataBlock.lastMediaSequence;

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media sequence as {0} of {1}",
                                                           _imageInfo.MediaSequence, _imageInfo.LastMediaSequence);
                            }

                            if(metadataBlock.creatorLength                               > 0 &&
                               metadataBlock.creatorLength + metadataBlock.creatorOffset <= metadata.Length)
                            {
                                _imageInfo.Creator =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.creatorOffset,
                                                               (int)(metadataBlock.creatorLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting creator: {0}",
                                                           _imageInfo.Creator);
                            }

                            if(metadataBlock.commentsOffset                                > 0 &&
                               metadataBlock.commentsLength + metadataBlock.commentsOffset <= metadata.Length)
                            {
                                _imageInfo.Comments =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.commentsOffset,
                                                               (int)(metadataBlock.commentsLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting comments: {0}",
                                                           _imageInfo.Comments);
                            }

                            if(metadataBlock.mediaTitleOffset                                  > 0 &&
                               metadataBlock.mediaTitleLength + metadataBlock.mediaTitleOffset <= metadata.Length)
                            {
                                _imageInfo.MediaTitle =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaTitleOffset,
                                                               (int)(metadataBlock.mediaTitleLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media title: {0}",
                                                           _imageInfo.MediaTitle);
                            }

                            if(metadataBlock.mediaManufacturerOffset > 0 &&
                               metadataBlock.mediaManufacturerLength + metadataBlock.mediaManufacturerOffset <=
                               metadata.Length)
                            {
                                _imageInfo.MediaManufacturer =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaManufacturerOffset,
                                                               (int)(metadataBlock.mediaManufacturerLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media manufacturer: {0}",
                                                           _imageInfo.MediaManufacturer);
                            }

                            if(metadataBlock.mediaModelOffset                                  > 0 &&
                               metadataBlock.mediaModelLength + metadataBlock.mediaModelOffset <= metadata.Length)
                            {
                                _imageInfo.MediaModel =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaModelOffset,
                                                               (int)(metadataBlock.mediaModelLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media model: {0}",
                                                           _imageInfo.MediaModel);
                            }

                            if(metadataBlock.mediaSerialNumberOffset > 0 &&
                               metadataBlock.mediaSerialNumberLength + metadataBlock.mediaSerialNumberOffset <=
                               metadata.Length)
                            {
                                _imageInfo.MediaSerialNumber =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaSerialNumberOffset,
                                                               (int)(metadataBlock.mediaSerialNumberLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media serial number: {0}",
                                                           _imageInfo.MediaSerialNumber);
                            }

                            if(metadataBlock.mediaBarcodeOffset                                    > 0 &&
                               metadataBlock.mediaBarcodeLength + metadataBlock.mediaBarcodeOffset <= metadata.Length)
                            {
                                _imageInfo.MediaBarcode =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaBarcodeOffset,
                                                               (int)(metadataBlock.mediaBarcodeLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media barcode: {0}",
                                                           _imageInfo.MediaBarcode);
                            }

                            if(metadataBlock.mediaPartNumberOffset > 0 &&
                               metadataBlock.mediaPartNumberLength + metadataBlock.mediaPartNumberOffset <=
                               metadata.Length)
                            {
                                _imageInfo.MediaPartNumber =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaPartNumberOffset,
                                                               (int)(metadataBlock.mediaPartNumberLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media part number: {0}",
                                                           _imageInfo.MediaPartNumber);
                            }

                            if(metadataBlock.driveManufacturerOffset > 0 &&
                               metadataBlock.driveManufacturerLength + metadataBlock.driveManufacturerOffset <=
                               metadata.Length)
                            {
                                _imageInfo.DriveManufacturer =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveManufacturerOffset,
                                                               (int)(metadataBlock.driveManufacturerLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive manufacturer: {0}",
                                                           _imageInfo.DriveManufacturer);
                            }

                            if(metadataBlock.driveModelOffset                                  > 0 &&
                               metadataBlock.driveModelLength + metadataBlock.driveModelOffset <= metadata.Length)
                            {
                                _imageInfo.DriveModel =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveModelOffset,
                                                               (int)(metadataBlock.driveModelLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive model: {0}",
                                                           _imageInfo.DriveModel);
                            }

                            if(metadataBlock.driveSerialNumberOffset > 0 &&
                               metadataBlock.driveSerialNumberLength + metadataBlock.driveSerialNumberOffset <=
                               metadata.Length)
                            {
                                _imageInfo.DriveSerialNumber =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveSerialNumberOffset,
                                                               (int)(metadataBlock.driveSerialNumberLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive serial number: {0}",
                                                           _imageInfo.DriveSerialNumber);
                            }

                            if(metadataBlock.driveFirmwareRevisionOffset > 0 &&
                               metadataBlock.driveFirmwareRevisionLength + metadataBlock.driveFirmwareRevisionOffset <=
                               metadata.Length)
                            {
                                _imageInfo.DriveFirmwareRevision =
                                    Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveFirmwareRevisionOffset,
                                                               (int)(metadataBlock.driveFirmwareRevisionLength - 2));

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive firmware revision: {0}",
                                                           _imageInfo.DriveFirmwareRevision);
                            }

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            break;

                        // Optical disc tracks block
                        case BlockType.TracksBlock:
                            _structureBytes = new byte[Marshal.SizeOf<TracksHeader>()];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            TracksHeader tracksHeader =
                                Marshal.SpanToStructureLittleEndian<TracksHeader>(_structureBytes);

                            if(tracksHeader.identifier != BlockType.TracksBlock)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect identifier for tracks block at position {0}",
                                                           entry.offset);

                                break;
                            }

                            _structureBytes = new byte[Marshal.SizeOf<TrackEntry>() * tracksHeader.entries];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                            Crc64Context.Data(_structureBytes, out byte[] trksCrc);

                            if(BitConverter.ToUInt64(trksCrc, 0) != tracksHeader.crc64)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                           BitConverter.ToUInt64(trksCrc, 0), tracksHeader.crc64);

                                break;
                            }

                            _imageStream.Position -= _structureBytes.Length;

                            Tracks      = new List<Track>();
                            _trackFlags = new Dictionary<byte, byte>();
                            _trackIsrcs = new Dictionary<byte, string>();

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Found {0} tracks at position {0}",
                                                       tracksHeader.entries, entry.offset);

                            for(ushort i = 0; i < tracksHeader.entries; i++)
                            {
                                _structureBytes = new byte[Marshal.SizeOf<TrackEntry>()];
                                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                                TrackEntry trackEntry =
                                    Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(_structureBytes);

                                Tracks.Add(new Track
                                {
                                    TrackSequence    = trackEntry.sequence,
                                    TrackType        = trackEntry.type,
                                    TrackStartSector = (ulong)trackEntry.start,
                                    TrackEndSector   = (ulong)trackEntry.end,
                                    TrackPregap      = (ulong)trackEntry.pregap,
                                    TrackSession     = trackEntry.session,
                                    TrackFileType    = "BINARY"
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

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            break;

                        // CICM XML metadata block
                        case BlockType.CicmBlock:
                            _structureBytes = new byte[Marshal.SizeOf<CicmMetadataBlock>()];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            CicmMetadataBlock cicmBlock =
                                Marshal.SpanToStructureLittleEndian<CicmMetadataBlock>(_structureBytes);

                            if(cicmBlock.identifier != BlockType.CicmBlock)
                                break;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Found CICM XML metadata block at position {0}", entry.offset);

                            byte[] cicmBytes = new byte[cicmBlock.length];
                            _imageStream.Read(cicmBytes, 0, cicmBytes.Length);
                            var cicmMs = new MemoryStream(cicmBytes);
                            var cicmXs = new XmlSerializer(typeof(CICMMetadataType));

                            try
                            {
                                var sr = new StreamReader(cicmMs);
                                CicmMetadata = (CICMMetadataType)cicmXs.Deserialize(sr);
                                sr.Close();
                            }
                            catch(XmlException ex)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Exception {0} processing CICM XML metadata block",
                                                           ex.Message);

                                CicmMetadata = null;
                            }

                            break;

                        // Dump hardware block
                        case BlockType.DumpHardwareBlock:
                            _structureBytes = new byte[Marshal.SizeOf<DumpHardwareHeader>()];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            DumpHardwareHeader dumpBlock =
                                Marshal.SpanToStructureLittleEndian<DumpHardwareHeader>(_structureBytes);

                            if(dumpBlock.identifier != BlockType.DumpHardwareBlock)
                                break;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Found dump hardware block at position {0}", entry.offset);

                            _structureBytes = new byte[dumpBlock.length];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                            Crc64Context.Data(_structureBytes, out byte[] dumpCrc);

                            if(BitConverter.ToUInt64(dumpCrc, 0) != dumpBlock.crc64)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                           BitConverter.ToUInt64(dumpCrc, 0), dumpBlock.crc64);

                                break;
                            }

                            _imageStream.Position -= _structureBytes.Length;

                            DumpHardware = new List<DumpHardwareType>();

                            for(ushort i = 0; i < dumpBlock.entries; i++)
                            {
                                _structureBytes = new byte[Marshal.SizeOf<DumpHardwareEntry>()];
                                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                                DumpHardwareEntry dumpEntry =
                                    Marshal.SpanToStructureLittleEndian<DumpHardwareEntry>(_structureBytes);

                                var dump = new DumpHardwareType
                                {
                                    Software = new SoftwareType(),
                                    Extents  = new ExtentType[dumpEntry.extents]
                                };

                                byte[] tmp;

                                if(dumpEntry.manufacturerLength > 0)
                                {
                                    tmp = new byte[dumpEntry.manufacturerLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Manufacturer     =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.modelLength > 0)
                                {
                                    tmp = new byte[dumpEntry.modelLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Model            =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.revisionLength > 0)
                                {
                                    tmp = new byte[dumpEntry.revisionLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Revision         =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.firmwareLength > 0)
                                {
                                    tmp = new byte[dumpEntry.firmwareLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Firmware         =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.serialLength > 0)
                                {
                                    tmp = new byte[dumpEntry.serialLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Serial           =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.softwareNameLength > 0)
                                {
                                    tmp = new byte[dumpEntry.softwareNameLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Software.Name    =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.softwareVersionLength > 0)
                                {
                                    tmp = new byte[dumpEntry.softwareVersionLength - 1];
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    _imageStream.Position += 1;
                                    dump.Software.Version =  Encoding.UTF8.GetString(tmp);
                                }

                                if(dumpEntry.softwareOperatingSystemLength > 0)
                                {
                                    tmp                   =  new byte[dumpEntry.softwareOperatingSystemLength - 1];
                                    _imageStream.Position += 1;
                                    _imageStream.Read(tmp, 0, tmp.Length);
                                    dump.Software.OperatingSystem = Encoding.UTF8.GetString(tmp);
                                }

                                tmp = new byte[16];

                                for(uint j = 0; j < dumpEntry.extents; j++)
                                {
                                    _imageStream.Read(tmp, 0, tmp.Length);

                                    dump.Extents[j] = new ExtentType
                                    {
                                        Start = BitConverter.ToUInt64(tmp, 0),
                                        End   = BitConverter.ToUInt64(tmp, 8)
                                    };
                                }

                                dump.Extents = dump.Extents.OrderBy(t => t.Start).ToArray();

                                if(dump.Extents.Length > 0)
                                    DumpHardware.Add(dump);
                            }

                            if(DumpHardware.Count == 0)
                                DumpHardware = null;

                            break;

                        // Tape partition block
                        case BlockType.TapePartitionBlock:
                            _structureBytes = new byte[Marshal.SizeOf<TapePartitionHeader>()];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            TapePartitionHeader partitionHeader =
                                Marshal.SpanToStructureLittleEndian<TapePartitionHeader>(_structureBytes);

                            if(partitionHeader.identifier != BlockType.TapePartitionBlock)
                                break;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Found tape partition block at position {0}", entry.offset);

                            byte[] tapePartitionBytes = new byte[partitionHeader.length];
                            _imageStream.Read(tapePartitionBytes, 0, tapePartitionBytes.Length);

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
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            TapeFileHeader fileHeader =
                                Marshal.SpanToStructureLittleEndian<TapeFileHeader>(_structureBytes);

                            if(fileHeader.identifier != BlockType.TapeFileBlock)
                                break;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Found tape file block at position {0}",
                                                       entry.offset);

                            byte[] tapeFileBytes = new byte[fileHeader.length];
                            _imageStream.Read(tapeFileBytes, 0, tapeFileBytes.Length);
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
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                            CompactDiscIndexesHeader indexesHeader =
                                Marshal.SpanToStructureLittleEndian<CompactDiscIndexesHeader>(_structureBytes);

                            if(indexesHeader.identifier != BlockType.CompactDiscIndexesBlock)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect identifier for compact disc indexes block at position {0}",
                                                           entry.offset);

                                break;
                            }

                            _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexEntry>() * indexesHeader.entries];
                            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                            Crc64Context.Data(_structureBytes, out byte[] idsxCrc);

                            if(BitConverter.ToUInt64(idsxCrc, 0) != indexesHeader.crc64)
                            {
                                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                           "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                           BitConverter.ToUInt64(idsxCrc, 0), indexesHeader.crc64);

                                break;
                            }

                            _imageStream.Position -= _structureBytes.Length;

                            compactDiscIndexes = new List<CompactDiscIndexEntry>();

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Found {0} compact disc indexes at position {0}",
                                                       indexesHeader.entries, entry.offset);

                            for(ushort i = 0; i < indexesHeader.entries; i++)
                            {
                                _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexEntry>()];
                                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                                compactDiscIndexes.Add(Marshal.
                                                           ByteArrayToStructureLittleEndian<
                                                               CompactDiscIndexEntry>(_structureBytes));
                            }

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            break;
                    }
                }

                if(!foundUserDataDdt)
                {
                    ErrorMessage = "Could not find user data deduplication table.";

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
                if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
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
                            Dictionary<int, long> leadOutStarts = new Dictionary<int, long>(); // Lead-out starts

                            foreach(FullTOC.TrackDataDescriptor trk in
                                decodedFullToc.Value.TrackDescriptors.Where(trk => (trk.ADR == 1 || trk.ADR == 4) &&
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

                                foreach(Track trk in Tracks.Where(trk => trk.TrackSession == leadOuts.Key).
                                                            Where(trk => trk.TrackSequence >
                                                                         lastTrackInSession.TrackSequence))
                                    lastTrackInSession = trk;

                                if(lastTrackInSession.TrackSequence  == 0 ||
                                   lastTrackInSession.TrackEndSector == (ulong)leadOuts.Value - 1)
                                    continue;

                                lastTrackInSession.TrackEndSector = (ulong)leadOuts.Value - 1;
                            }
                        }
                    }

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    if(Tracks       == null ||
                       Tracks.Count == 0)
                    {
                        Tracks = new List<Track>
                        {
                            new Track
                            {
                                TrackBytesPerSector    = (int)_imageInfo.SectorSize,
                                TrackEndSector         = _imageInfo.Sectors - 1,
                                TrackFileType          = "BINARY",
                                TrackRawBytesPerSector = (int)_imageInfo.SectorSize,
                                TrackSession           = 1,
                                TrackSequence          = 1,
                                TrackType              = TrackType.Data
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

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    Sessions = new List<Session>();

                    for(int i = 1; i <= Tracks.Max(t => t.TrackSession); i++)
                        Sessions.Add(new Session
                        {
                            SessionSequence = (ushort)i,
                            StartTrack      = Tracks.Where(t => t.TrackSession == i).Min(t => t.TrackSequence),
                            EndTrack        = Tracks.Where(t => t.TrackSession == i).Max(t => t.TrackSequence),
                            StartSector     = Tracks.Where(t => t.TrackSession == i).Min(t => t.TrackStartSector),
                            EndSector       = Tracks.Where(t => t.TrackSession == i).Max(t => t.TrackEndSector)
                        });

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    foreach(Track track in Tracks.OrderBy(t => t.TrackStartSector))
                    {
                        if(track.TrackSequence == 1)
                        {
                            track.TrackPregap = 150;
                            track.Indexes[0]  = -150;
                            track.Indexes[1]  = (int)track.TrackStartSector;

                            continue;
                        }

                        if(track.TrackPregap > 0)
                        {
                            track.Indexes[0] = (int)track.TrackStartSector;
                            track.Indexes[1] = (int)(track.TrackStartSector + track.TrackPregap);
                        }
                        else
                            track.Indexes[1] = (int)track.TrackStartSector;
                    }

                    ulong currentTrackOffset = 0;
                    Partitions = new List<Partition>();

                    foreach(Track track in Tracks.OrderBy(t => t.TrackStartSector))
                    {
                        Partitions.Add(new Partition
                        {
                            Sequence = track.TrackSequence,
                            Type     = track.TrackType.ToString(),
                            Name     = $"Track {track.TrackSequence}",
                            Offset   = currentTrackOffset,
                            Start    = (ulong)track.Indexes[1],
                            Size = (track.TrackEndSector - (ulong)track.Indexes[1] + 1) *
                                   (ulong)track.TrackBytesPerSector,
                            Length = track.TrackEndSector - (ulong)track.Indexes[1] + 1,
                            Scheme = "Optical disc track"
                        });

                        currentTrackOffset += (track.TrackEndSector - track.TrackStartSector + 1) *
                                              (ulong)track.TrackBytesPerSector;
                    }

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    Track[] tracks = Tracks.ToArray();

                    foreach(Track trk in tracks)
                    {
                        byte[] sector = ReadSector(trk.TrackStartSector);
                        trk.TrackBytesPerSector = sector.Length;

                        trk.TrackRawBytesPerSector =
                            (_sectorPrefix    != null && _sectorSuffix    != null) ||
                            (_sectorPrefixDdt != null && _sectorSuffixDdt != null) ? 2352 : sector.Length;

                        if(_sectorSubchannel == null)
                            continue;

                        trk.TrackSubchannelFile   = trk.TrackFile;
                        trk.TrackSubchannelFilter = trk.TrackFilter;
                        trk.TrackSubchannelType   = TrackSubchannelType.Raw;
                    }

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    Tracks = tracks.ToList();

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    if(compactDiscIndexes != null)
                    {
                        foreach(CompactDiscIndexEntry compactDiscIndex in compactDiscIndexes.OrderBy(i => i.Track).
                            ThenBy(i => i.Index))
                        {
                            Track track = Tracks.FirstOrDefault(t => t.TrackSequence == compactDiscIndex.Track);

                            if(track is null)
                                continue;

                            track.Indexes[compactDiscIndex.Index] = compactDiscIndex.Lba;
                        }
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
                    MemoryMarshal.Write(_structureBytes, ref ddtHeader);
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

            AaruConsole.DebugWriteLine("Aaru Format plugin", "In memory DDT?: {0}", _inMemoryDdt);

            _imageStream.Seek(0, SeekOrigin.End);

            // Initialize compressors properties (all maxed)
            _lzmaEncoderProperties = new LzmaEncoderProperties(true, (int)dictionary, 273);

            _flakeWriterSettings = new EncoderSettings
            {
                PCM                = AudioPCMConfig.RedBook,
                DoMD5              = false,
                BlockSize          = (1 << _shift) * SAMPLES_PER_SECTOR,
                MinFixedOrder      = 0,
                MaxFixedOrder      = 4,
                MinLPCOrder        = 1,
                MaxLPCOrder        = 32,
                MaxPartitionOrder  = 8,
                StereoMethod       = StereoMethod.Evaluate,
                PredictionType     = PredictionType.Search,
                WindowMethod       = WindowMethod.EvaluateN,
                EstimationDepth    = 5,
                MinPrecisionSearch = 1,
                MaxPrecisionSearch = 1,
                TukeyParts         = 0,
                TukeyOverlap       = 1.0,
                TukeyP             = 1.0,
                AllowNonSubset     = true
            };

            // Check if FLAKE's block size is bigger than what we want
            if(_flakeWriterSettings.BlockSize > MAX_FLAKE_BLOCK)
                _flakeWriterSettings.BlockSize = MAX_FLAKE_BLOCK;

            if(_flakeWriterSettings.BlockSize < MIN_FLAKE_BLOCK)
                _flakeWriterSettings.BlockSize = MIN_FLAKE_BLOCK;

            AudioEncoder.Vendor = "Aaru";

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        /// <inheritdoc />
        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

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
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(sectorAddress >= Info.Sectors &&
               !IsTape)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            if((_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc || !_writingLong) &&
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
            if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                 sectorAddress <= t.TrackEndSector) ?? new Track();

                if(trk.TrackSequence    == 0 &&
                   trk.TrackStartSector == 0 &&
                   trk.TrackEndSector   == 0)
                    trk.TrackType = TrackType.Data; // TODO: Check intersession data type
            }

            // Close current block first
            if(_blockStream != null &&

               // When sector siz changes
               (_currentBlockHeader.sectorSize != data.Length ||

                // When block if filled
                _currentBlockOffset == 1 << _shift ||

                // When we change to/from CompactDisc audio
                (_currentBlockHeader.compression == CompressionType.Flac && trk.TrackType != TrackType.Audio)))
            {
                _currentBlockHeader.length = _currentBlockOffset * _currentBlockHeader.sectorSize;
                _currentBlockHeader.crc64  = BitConverter.ToUInt64(_crc64.Final(), 0);

                var cmpCrc64Context = new Crc64Context();

                byte[] lzmaProperties = Array.Empty<byte>();

                switch(_currentBlockHeader.compression)
                {
                    case CompressionType.Flac:
                    {
                        long remaining = _currentBlockOffset * SAMPLES_PER_SECTOR % _flakeWriter.Settings.BlockSize;

                        // Fill FLAC block
                        if(remaining != 0)
                        {
                            var audioBuffer =
                                new AudioBuffer(AudioPCMConfig.RedBook, new byte[remaining * 4], (int)remaining);

                            _flakeWriter.Write(audioBuffer);
                        }

                        _flakeWriter.Close();

                        break;
                    }
                    case CompressionType.Lzma:
                    {
                        lzmaProperties = _lzmaBlockStream.Properties;
                        _lzmaBlockStream.Close();
                        _lzmaBlockStream = null;
                        cmpCrc64Context.Update(lzmaProperties);

                        if(_blockStream.Length > _decompressedStream.Length)
                            _currentBlockHeader.compression = CompressionType.None;

                        break;
                    }
                }

                if(_currentBlockHeader.compression == CompressionType.None)
                {
                    _blockStream                 = _decompressedStream;
                    _currentBlockHeader.cmpCrc64 = _currentBlockHeader.crc64;
                }
                else
                {
                    cmpCrc64Context.Update(_blockStream.ToArray());
                    _currentBlockHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);
                }

                _currentBlockHeader.cmpLength = (uint)_blockStream.Length;

                if(_currentBlockHeader.compression == CompressionType.Lzma)
                    _currentBlockHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

                _index.Add(new IndexEntry
                {
                    blockType = BlockType.DataBlock,
                    dataType  = DataType.UserData,
                    offset    = (ulong)_imageStream.Position
                });

                _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                MemoryMarshal.Write(_structureBytes, ref _currentBlockHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _structureBytes = null;

                if(_currentBlockHeader.compression == CompressionType.Lzma)
                    _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                _blockStream.Close();
                _blockStream = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
                _currentBlockOffset = 0;
            }

            // No block set
            if(_blockStream == null)
            {
                _currentBlockHeader = new BlockHeader
                {
                    identifier  = BlockType.DataBlock,
                    type        = DataType.UserData,
                    compression = _compress ? CompressionType.Lzma : CompressionType.None,
                    sectorSize  = (uint)data.Length
                };

                if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc &&
                   trk.TrackType           == TrackType.Audio          &&
                   _compress)
                    _currentBlockHeader.compression = CompressionType.Flac;

                // JaguarCD stores data in audio tracks. FLAC is too inefficient, use LZMA there.
                // VideoNow stores video in audio tracks, and LZMA works better too.
                if(((_imageInfo.MediaType == MediaType.JaguarCD && trk.TrackSession > 1) ||
                    _imageInfo.MediaType == MediaType.VideoNow || _imageInfo.MediaType == MediaType.VideoNowColor ||
                    _imageInfo.MediaType == MediaType.VideoNowXp) &&
                   trk.TrackType == TrackType.Audio               &&
                   _compress                                      &&
                   _currentBlockHeader.compression == CompressionType.Flac)
                    _currentBlockHeader.compression = CompressionType.Lzma;

                _blockStream        = new NonClosableStream();
                _decompressedStream = new NonClosableStream();

                switch(_currentBlockHeader.compression)
                {
                    case CompressionType.Flac:
                        _flakeWriter = new AudioEncoder(_flakeWriterSettings, "", _blockStream)
                        {
                            DoSeekTable = false
                        };

                        break;
                    case CompressionType.Lzma:
                        _lzmaBlockStream = new LzmaStream(_lzmaEncoderProperties, false, _blockStream);

                        break;
                    default:
                        _lzmaBlockStream = null;

                        break;
                }

                _crc64 = new Crc64Context();
            }

            ulong ddtEntry = (ulong)((_imageStream.Position << _shift) + _currentBlockOffset);

            if(hash != null)
                _deduplicationTable.Add(hashString, ddtEntry);

            if(_currentBlockHeader.compression == CompressionType.Flac)
            {
                var audioBuffer = new AudioBuffer(AudioPCMConfig.RedBook, data, SAMPLES_PER_SECTOR);
                _flakeWriter.Write(audioBuffer);
            }
            else
            {
                _decompressedStream.Write(data, 0, data.Length);

                if(_currentBlockHeader.compression == CompressionType.Lzma)
                    _lzmaBlockStream.Write(data, 0, data.Length);
            }

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
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(sectorAddress + length > Info.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

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
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            byte[] sector;

            switch(_imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track track =
                        Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

                    if(track is null)
                    {
                        ErrorMessage = "Track not found";

                        return false;
                    }

                    if(track.TrackSequence    == 0 &&
                       track.TrackStartSector == 0 &&
                       track.TrackEndSector   == 0)
                        track.TrackType = TrackType.Data;

                    if(data.Length != 2352)
                    {
                        ErrorMessage = "Incorrect data size";

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
                    switch(track.TrackType)
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

                            _sectorSuffixMs ??= new NonClosableStream();

                            _sectorPrefixMs ??= new NonClosableStream();

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
                                    _sectorPrefixMs.Position =
                                        ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;
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
                                    _sectorSuffixMs.Position =
                                        ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;
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

                            _sectorSuffixMs ??= new NonClosableStream();

                            _sectorPrefixMs ??= new NonClosableStream();

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
                                    _sectorPrefixMs.Position =
                                        ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;
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
                                {
                                    _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.Mode2Form2Ok;
                                }
                                else if(BitConverter.ToUInt32(data, 0x92C) == 0)
                                {
                                    _sectorSuffixDdt[sectorAddress] = (uint)CdFixFlags.Mode2Form2NoCrc;
                                }
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
                case XmlMediaType.BlockMedia:
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
                                case 12 when _imageInfo.MediaType == MediaType.AppleProfile ||
                                             _imageInfo.MediaType == MediaType.AppleFileWare:
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
                                case 12 when _imageInfo.MediaType == MediaType.AppleSonySS ||
                                             _imageInfo.MediaType == MediaType.AppleSonySS:
                                    newTag = new byte[12];
                                    Array.Copy(data, 512, newTag, 0, 12);

                                    break;

                                // Profile tag, copy to Profile
                                case 20 when _imageInfo.MediaType == MediaType.AppleProfile ||
                                             _imageInfo.MediaType == MediaType.AppleFileWare:
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
                                case 20 when _imageInfo.MediaType == MediaType.AppleSonySS ||
                                             _imageInfo.MediaType == MediaType.AppleSonySS:
                                    oldTag = new byte[20];
                                    Array.Copy(data, 512, oldTag, 0, 20);
                                    newTag = LisaTag.DecodeProfileTag(oldTag)?.ToSony().GetBytes();

                                    break;

                                // Priam tag, convert to Profile
                                case 24 when _imageInfo.MediaType == MediaType.AppleProfile ||
                                             _imageInfo.MediaType == MediaType.AppleFileWare:
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
                                case 24 when _imageInfo.MediaType == MediaType.AppleSonySS ||
                                             _imageInfo.MediaType == MediaType.AppleSonySS:
                                    oldTag = new byte[24];
                                    Array.Copy(data, 512, oldTag, 0, 24);
                                    newTag = LisaTag.DecodePriamTag(oldTag)?.ToSony().GetBytes();

                                    break;
                                case 0:
                                    newTag = null;

                                    break;
                                default:
                                    ErrorMessage = "Incorrect data size";

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

            ErrorMessage = "Unknown long sector type, cannot write.";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            byte[] sector;

            switch(_imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    if(data.Length % 2352 != 0)
                    {
                        ErrorMessage = "Incorrect data size";

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
                case XmlMediaType.BlockMedia:
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
                                ErrorMessage = "Incorrect data size";

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

            ErrorMessage = "Unknown long sector type, cannot write.";

            return false;
        }

        /// <inheritdoc />
        public bool SetTracks(List<Track> tracks)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            {
                ErrorMessage = "Unsupported feature";

                return false;
            }

            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

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
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            // Close current block first
            if(_blockStream != null)
            {
                _currentBlockHeader.length = _currentBlockOffset * _currentBlockHeader.sectorSize;
                _currentBlockHeader.crc64  = BitConverter.ToUInt64(_crc64.Final(), 0);

                var cmpCrc64Context = new Crc64Context();

                byte[] lzmaProperties = Array.Empty<byte>();

                if(_currentBlockHeader.compression == CompressionType.Flac)
                {
                    long remaining = _currentBlockOffset * SAMPLES_PER_SECTOR % _flakeWriter.Settings.BlockSize;

                    // Fill FLAC block
                    if(remaining != 0)
                    {
                        var audioBuffer =
                            new AudioBuffer(AudioPCMConfig.RedBook, new byte[remaining * 4], (int)remaining);

                        _flakeWriter.Write(audioBuffer);
                    }

                    _flakeWriter.Close();
                }
                else if(_currentBlockHeader.compression == CompressionType.Lzma)
                {
                    lzmaProperties = _lzmaBlockStream.Properties;
                    _lzmaBlockStream.Close();
                    _lzmaBlockStream = null;
                    cmpCrc64Context.Update(lzmaProperties);

                    if(_blockStream.Length > _decompressedStream.Length)
                        _currentBlockHeader.compression = CompressionType.None;
                }

                if(_currentBlockHeader.compression == CompressionType.None)
                {
                    _blockStream                 = _decompressedStream;
                    _currentBlockHeader.cmpCrc64 = _currentBlockHeader.crc64;
                }
                else
                {
                    cmpCrc64Context.Update(_blockStream.ToArray());
                    _currentBlockHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);
                }

                _currentBlockHeader.cmpLength = (uint)_blockStream.Length;

                if(_currentBlockHeader.compression == CompressionType.Lzma)
                    _currentBlockHeader.cmpLength += LZMA_PROPERTIES_LENGTH;

                _index.Add(new IndexEntry
                {
                    blockType = BlockType.DataBlock,
                    dataType  = DataType.UserData,
                    offset    = (ulong)_imageStream.Position
                });

                _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                MemoryMarshal.Write(_structureBytes, ref _currentBlockHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _structureBytes = null;

                if(_currentBlockHeader.compression == CompressionType.Lzma)
                    _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                _blockStream.ReallyClose();
                _blockStream = null;
            }

            if(_deduplicate)
                AaruConsole.DebugWriteLine("Aaru Format plugin", "Of {0} sectors written, {1} are unique ({2:P})",
                                           _writtenSectors, _deduplicationTable.Count,
                                           (double)_deduplicationTable.Count / _writtenSectors);

            IndexEntry idxEntry;

            // Write media tag blocks
            foreach(KeyValuePair<MediaTagType, byte[]> mediaTag in _mediaTags)
            {
                DataType dataType = GetDataTypeForMediaTag(mediaTag.Key);

                if(mediaTag.Value is null)
                {
                    AaruConsole.ErrorWriteLine("Tag type {0} is null, skipping...", dataType);

                    continue;
                }

                idxEntry = new IndexEntry
                {
                    blockType = BlockType.DataBlock,
                    dataType  = dataType,
                    offset    = (ulong)_imageStream.Position
                };

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing tag type {0} to position {1}", mediaTag.Key,
                                           idxEntry.offset);

                Crc64Context.Data(mediaTag.Value, out byte[] tagCrc);

                var tagBlock = new BlockHeader
                {
                    identifier = BlockType.DataBlock,
                    type       = dataType,
                    length     = (uint)mediaTag.Value.Length,
                    crc64      = BitConverter.ToUInt64(tagCrc, 0)
                };

                _blockStream = new NonClosableStream();

                byte[] lzmaProperties =
                    CompressDataToStreamWithLZMA(mediaTag.Value, _lzmaEncoderProperties, _blockStream);

                byte[] tagData;

                // Not compressible
                if(_blockStream.Length + LZMA_PROPERTIES_LENGTH >= mediaTag.Value.Length)
                {
                    tagBlock.cmpLength   = tagBlock.length;
                    tagBlock.cmpCrc64    = tagBlock.crc64;
                    tagData              = mediaTag.Value;
                    tagBlock.compression = CompressionType.None;
                }
                else
                {
                    tagData = _blockStream.ToArray();
                    var crc64Ctx = new Crc64Context();
                    crc64Ctx.Update(lzmaProperties);
                    crc64Ctx.Update(tagData);
                    tagCrc               = crc64Ctx.Final();
                    tagBlock.cmpLength   = (uint)tagData.Length + LZMA_PROPERTIES_LENGTH;
                    tagBlock.cmpCrc64    = BitConverter.ToUInt64(tagCrc, 0);
                    tagBlock.compression = CompressionType.Lzma;
                }

                _blockStream.ReallyClose();
                _blockStream = null;

                _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                MemoryMarshal.Write(_structureBytes, ref tagBlock);
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

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing geometry block to position {0}",
                                           idxEntry.offset);

                _structureBytes = new byte[Marshal.SizeOf<GeometryBlock>()];
                MemoryMarshal.Write(_structureBytes, ref _geometryBlock);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                _index.RemoveAll(t => t.blockType == BlockType.GeometryBlock && t.dataType == DataType.NoData);

                _index.Add(idxEntry);
            }

            // If we have dump hardware, write it
            if(DumpHardware != null)
            {
                var dumpMs = new MemoryStream();

                foreach(DumpHardwareType dump in DumpHardware)
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
                        extents                       = (uint)dump.Extents.Length
                    };

                    _structureBytes = new byte[Marshal.SizeOf<DumpHardwareEntry>()];
                    MemoryMarshal.Write(_structureBytes, ref dumpEntry);
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

                    foreach(ExtentType extent in dump.Extents)
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

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing dump hardware block to position {0}",
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
                MemoryMarshal.Write(_structureBytes, ref dumpBlock);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _imageStream.Write(dumpMs.ToArray(), 0, (int)dumpMs.Length);

                _index.RemoveAll(t => t.blockType == BlockType.DumpHardwareBlock && t.dataType == DataType.NoData);

                _index.Add(idxEntry);
            }

            // If we have CICM XML metadata, write it
            if(CicmMetadata != null)
            {
                var cicmMs = new MemoryStream();
                var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(cicmMs, CicmMetadata);

                idxEntry = new IndexEntry
                {
                    blockType = BlockType.CicmBlock,
                    dataType  = DataType.NoData,
                    offset    = (ulong)_imageStream.Position
                };

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing CICM XML block to position {0}",
                                           idxEntry.offset);

                var cicmBlock = new CicmMetadataBlock
                {
                    identifier = BlockType.CicmBlock,
                    length     = (uint)cicmMs.Length
                };

                _structureBytes = new byte[Marshal.SizeOf<CicmMetadataBlock>()];
                MemoryMarshal.Write(_structureBytes, ref cicmBlock);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _imageStream.Write(cicmMs.ToArray(), 0, (int)cicmMs.Length);

                _index.RemoveAll(t => t.blockType == BlockType.CicmBlock && t.dataType == DataType.NoData);

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
                    MemoryMarshal.Write(_structureBytes, ref md5Entry);
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
                    MemoryMarshal.Write(_structureBytes, ref sha1Entry);
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
                    MemoryMarshal.Write(_structureBytes, ref sha256Entry);
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
                    MemoryMarshal.Write(_structureBytes, ref spamsumEntry);
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

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing checksum block to position {0}",
                                               idxEntry.offset);

                    _structureBytes = new byte[Marshal.SizeOf<ChecksumHeader>()];
                    MemoryMarshal.Write(_structureBytes, ref chkHeader);
                    _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                    _imageStream.Write(chkMs.ToArray(), 0, (int)chkMs.Length);

                    _index.RemoveAll(t => t.blockType == BlockType.ChecksumBlock && t.dataType == DataType.NoData);

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

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing tape partitions to position {0}",
                                           idxEntry.offset);

                TapePartitionEntry[] tapePartitionEntries = new TapePartitionEntry[TapePartitions.Count];

                for(int t = 0; t < TapePartitions.Count; t++)
                {
                    tapePartitionEntries[t] = new TapePartitionEntry
                    {
                        Number     = TapePartitions[t].Number,
                        FirstBlock = TapePartitions[t].FirstBlock,
                        LastBlock  = TapePartitions[t].LastBlock
                    };
                }

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
                MemoryMarshal.Write(_structureBytes, ref tapePartitionHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _structureBytes = null;
                _imageStream.Write(tapePartitionEntriesData, 0, tapePartitionEntriesData.Length);

                _index.RemoveAll(t => t.blockType == BlockType.TapePartitionBlock && t.dataType == DataType.UserData);
                _index.Add(idxEntry);

                idxEntry = new IndexEntry
                {
                    blockType = BlockType.TapeFileBlock,
                    dataType  = DataType.UserData,
                    offset    = (ulong)_imageStream.Position
                };

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing tape files to position {0}", idxEntry.offset);

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
                MemoryMarshal.Write(_structureBytes, ref tapeFileHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _structureBytes = null;
                _imageStream.Write(tapeFileEntriesData, 0, tapeFileEntriesData.Length);

                _index.RemoveAll(t => t.blockType == BlockType.TapeFileBlock && t.dataType == DataType.UserData);
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

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing user data DDT to position {0}",
                                           idxEntry.offset);

                var ddtHeader = new DdtHeader
                {
                    identifier  = BlockType.DeDuplicationTable,
                    type        = DataType.UserData,
                    compression = CompressionType.Lzma,
                    shift       = _shift,
                    entries     = (ulong)_userDataDdt.LongLength,
                    length      = (ulong)(_userDataDdt.LongLength * sizeof(ulong))
                };

                _blockStream = new NonClosableStream();
                var userDataDdtStream = new MemoryStream();
                _crc64 = new Crc64Context();
                byte[] ddtEntries = MemoryMarshal.Cast<ulong, byte>(_userDataDdt).ToArray();
                _crc64.Update(ddtEntries);
                userDataDdtStream.Write(ddtEntries, 0, ddtEntries.Length);

                byte[] lzmaProperties =
                    CompressDataToStreamWithLZMA(userDataDdtStream.ToArray(), _lzmaEncoderProperties, _blockStream);

                userDataDdtStream.Close();
                ddtHeader.cmpLength = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                var cmpCrc64Context = new Crc64Context();
                cmpCrc64Context.Update(lzmaProperties);
                cmpCrc64Context.Update(_blockStream.ToArray());
                ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                MemoryMarshal.Write(_structureBytes, ref ddtHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                _structureBytes = null;
                _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);
                _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                _blockStream.ReallyClose();
                _blockStream = null;

                _index.RemoveAll(t => t.blockType == BlockType.DeDuplicationTable && t.dataType == DataType.UserData);

                _index.Add(idxEntry);
            }

            // Write the sector prefix, suffix and subchannels if present
            switch(_imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc when Tracks != null && Tracks.Count > 0:
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
                                                   "Writing CD sector prefix block to position {0}", idxEntry.offset);

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
                            _blockStream            = new NonClosableStream(_sectorPrefix);
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorPrefix, _lzmaEncoderProperties, _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                = cmpCrc.Final();
                            prefixBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            prefixBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to compress prefix",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref prefixBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(prefixBlock.compression == CompressionType.Lzma)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.CdSectorPrefix);

                        _index.Add(idxEntry);

                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorSuffix,
                            offset    = (ulong)_imageStream.Position
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "Writing CD sector suffix block to position {0}", idxEntry.offset);

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
                            _blockStream            = new NonClosableStream(_sectorSuffix);
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorSuffix, _lzmaEncoderProperties, _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                = cmpCrc.Final();
                            prefixBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            prefixBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to compress suffix",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref prefixBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(prefixBlock.compression == CompressionType.Lzma)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.CdSectorSuffix);

                        _index.Add(idxEntry);
                        _blockStream.ReallyClose();
                        _blockStream = null;
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
                            if((_sectorPrefixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped)
                                notDumpedPrefixes++;
                            else if((_sectorPrefixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                correctPrefixes++;
                            else if((_sectorPrefixDdt[i] & CD_DFIX_MASK) > 0)
                                writtenPrefixes++;

                        for(long i = 0; i < _sectorPrefixDdt.LongLength; i++)
                            if((_sectorSuffixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped)
                                notDumpedSuffixes++;
                            else if((_sectorSuffixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                correctSuffixes++;
                            else if((_sectorSuffixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.Mode2Form1Ok)
                                correctMode2Form1++;
                            else if((_sectorSuffixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.Mode2Form2Ok)
                                correctMode2Form2++;
                            else if((_sectorSuffixDdt[i] & CD_XFIX_MASK) == (uint)CdFixFlags.Mode2Form2NoCrc)
                                emptyMode2Form1++;
                            else if((_sectorSuffixDdt[i] & CD_DFIX_MASK) > 0)
                                writtenSuffixes++;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "{0} ({1:P}% prefixes are correct, {2} ({3:P}%) prefixes have not been dumped, {4} ({5:P}%) prefixes have been written to image",
                                                   correctPrefixes, correctPrefixes     / _imageInfo.Sectors,
                                                   notDumpedPrefixes, notDumpedPrefixes / _imageInfo.Sectors,
                                                   writtenPrefixes, writtenPrefixes     / _imageInfo.Sectors);

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "{0} ({1:P}% suffixes are correct, {2} ({3:P}%) suffixes have not been dumped, {4} ({5:P}%) suffixes have been written to image",
                                                   correctSuffixes, correctSuffixes     / _imageInfo.Sectors,
                                                   notDumpedSuffixes, notDumpedSuffixes / _imageInfo.Sectors,
                                                   writtenSuffixes, writtenSuffixes     / _imageInfo.Sectors);

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "{0} ({1:P}% MODE 2 Form 1 are correct, {2} ({3:P}%) MODE 2 Form 2 are correct, {4} ({5:P}%) MODE 2 Form 2 have empty CRC",
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
                                                   "Writing CompactDisc sector prefix DDT to position {0}",
                                                   idxEntry.offset);

                        var ddtHeader = new DdtHeader
                        {
                            identifier  = BlockType.DeDuplicationTable,
                            type        = DataType.CdSectorPrefixCorrected,
                            compression = CompressionType.Lzma,
                            entries     = (ulong)_sectorPrefixDdt.LongLength,
                            length      = (ulong)(_sectorPrefixDdt.LongLength * sizeof(uint))
                        };

                        _blockStream = new NonClosableStream();
                        var sectorPrefixDdtStream = new MemoryStream();
                        _crc64 = new Crc64Context();
                        byte[] ddtEntries = MemoryMarshal.Cast<uint, byte>(_sectorPrefixDdt).ToArray();
                        _crc64.Update(ddtEntries);
                        sectorPrefixDdtStream.Write(ddtEntries, 0, ddtEntries.Length);

                        byte[] lzmaProperties =
                            CompressDataToStreamWithLZMA(sectorPrefixDdtStream.ToArray(), _lzmaEncoderProperties,
                                                         _blockStream);

                        sectorPrefixDdtStream.Close();
                        ddtHeader.cmpLength = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                        var cmpCrc64Context = new Crc64Context();
                        cmpCrc64Context.Update(lzmaProperties);
                        cmpCrc64Context.Update(_blockStream.ToArray());
                        ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                        _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref ddtHeader);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                        _structureBytes = null;
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);
                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;

                        _index.RemoveAll(t => t.blockType == BlockType.DeDuplicationTable &&
                                              t.dataType  == DataType.CdSectorPrefixCorrected);

                        _index.Add(idxEntry);

                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DeDuplicationTable,
                            dataType  = DataType.CdSectorSuffixCorrected,
                            offset    = (ulong)_imageStream.Position
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "Writing CompactDisc sector suffix DDT to position {0}",
                                                   idxEntry.offset);

                        ddtHeader = new DdtHeader
                        {
                            identifier  = BlockType.DeDuplicationTable,
                            type        = DataType.CdSectorSuffixCorrected,
                            compression = CompressionType.Lzma,
                            entries     = (ulong)_sectorSuffixDdt.LongLength,
                            length      = (ulong)(_sectorSuffixDdt.LongLength * sizeof(uint))
                        };

                        _blockStream = new NonClosableStream();
                        var sectorSuffixDdtStream = new MemoryStream();
                        _crc64     = new Crc64Context();
                        ddtEntries = MemoryMarshal.Cast<uint, byte>(_sectorSuffixDdt).ToArray();
                        _crc64.Update(ddtEntries);
                        sectorSuffixDdtStream.Write(ddtEntries, 0, ddtEntries.Length);

                        lzmaProperties =
                            CompressDataToStreamWithLZMA(sectorSuffixDdtStream.ToArray(), _lzmaEncoderProperties,
                                                         _blockStream);

                        ddtHeader.cmpLength = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                        cmpCrc64Context     = new Crc64Context();
                        cmpCrc64Context.Update(lzmaProperties);
                        cmpCrc64Context.Update(_blockStream.ToArray());
                        ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                        _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref ddtHeader);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                        _structureBytes = null;
                        _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);
                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;

                        _index.RemoveAll(t => t.blockType == BlockType.DeDuplicationTable &&
                                              t.dataType  == DataType.CdSectorSuffixCorrected);

                        _index.Add(idxEntry);

                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorPrefixCorrected,
                            offset    = (ulong)_imageStream.Position
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "Writing CD sector corrected prefix block to position {0}",
                                                   idxEntry.offset);

                        Crc64Context.Data(_sectorPrefixMs.GetBuffer(), (uint)_sectorPrefixMs.Length,
                                          out byte[] blockCrc);

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
                            _blockStream            = _sectorPrefixMs;
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorPrefixMs.ToArray(), _lzmaEncoderProperties,
                                                             _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                = cmpCrc.Final();
                            prefixBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            prefixBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to compress prefix",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref prefixBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(prefixBlock.compression == CompressionType.Lzma)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.CdSectorPrefixCorrected);

                        _index.Add(idxEntry);

                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorSuffixCorrected,
                            offset    = (ulong)_imageStream.Position
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "Writing CD sector corrected suffix block to position {0}",
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
                            _blockStream            = _sectorSuffixMs;
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorSuffixMs.ToArray(), _lzmaEncoderProperties,
                                                             _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                = cmpCrc.Final();
                            suffixBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            suffixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            suffixBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to compress suffix",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref suffixBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(suffixBlock.compression == CompressionType.Lzma)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.CdSectorSuffixCorrected);

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
                                                   "Writing CD MODE2 subheaders block to position {0}",
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
                            _blockStream               = new NonClosableStream(_mode2Subheaders);
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_mode2Subheaders, _lzmaEncoderProperties, _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                   = cmpCrc.Final();
                            subheaderBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            subheaderBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            subheaderBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Took {0} seconds to compress MODE2 subheaders",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref subheaderBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(subheaderBlock.compression == CompressionType.Lzma)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.CompactDiscMode2Subheader);

                        _index.Add(idxEntry);
                        _blockStream.ReallyClose();
                        _blockStream = null;
                    }

                    if(_sectorSubchannel != null)
                    {
                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorSubchannel,
                            offset    = (ulong)_imageStream.Position
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing CD subchannel block to position {0}",
                                                   idxEntry.offset);

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
                            _blockStream                = new NonClosableStream(_sectorSubchannel);
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            byte[] transformedSubchannel = ClauniaSubchannelTransform(_sectorSubchannel);
                            _blockStream = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(transformedSubchannel, _lzmaEncoderProperties,
                                                             _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                    = cmpCrc.Final();
                            subchannelBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            subchannelBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            subchannelBlock.compression = CompressionType.LzmaClauniaSubchannelTransform;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to compress subchannel",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref subchannelBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(subchannelBlock.compression == CompressionType.Lzma ||
                           subchannelBlock.compression == CompressionType.LzmaClauniaSubchannelTransform)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.CdSectorSubchannel);

                        _index.Add(idxEntry);
                        _blockStream.ReallyClose();
                        _blockStream = null;
                    }

                    if(_sectorCpiMai != null)
                    {
                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.DvdSectorCpiMai,
                            offset    = (ulong)_imageStream.Position
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing DVD CPI_MAI block to position {0}",
                                                   idxEntry.offset);

                        Crc64Context.Data(_sectorCpiMai, out byte[] blockCrc);

                        var cpiMaiBlock = new BlockHeader
                        {
                            identifier = BlockType.DataBlock,
                            type       = DataType.DvdSectorCpiMai,
                            length     = (uint)_sectorCpiMai.Length,
                            crc64      = BitConverter.ToUInt64(blockCrc, 0),
                            sectorSize = 6
                        };

                        byte[] lzmaProperties = null;

                        if(!_compress)
                        {
                            cpiMaiBlock.compression = CompressionType.None;
                            cpiMaiBlock.cmpCrc64    = cpiMaiBlock.crc64;
                            cpiMaiBlock.cmpLength   = cpiMaiBlock.length;
                            _blockStream            = new NonClosableStream(_sectorCpiMai);
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorCpiMai, _lzmaEncoderProperties, _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                = cmpCrc.Final();
                            cpiMaiBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            cpiMaiBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            cpiMaiBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0} seconds to compress CPI_MAI",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref cpiMaiBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(cpiMaiBlock.compression == CompressionType.Lzma ||
                           cpiMaiBlock.compression == CompressionType.LzmaClauniaSubchannelTransform)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.DvdSectorCpiMai);

                        _index.Add(idxEntry);
                        _blockStream.ReallyClose();
                        _blockStream = null;
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
                                                   "Writing decrypted DVD title key block to position {0}",
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
                            _blockStream              = new NonClosableStream(_sectorDecryptedTitleKey);
                        }
                        else
                        {
                            startCompress = DateTime.Now;
                            _blockStream  = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorDecryptedTitleKey, _lzmaEncoderProperties,
                                                             _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                  = cmpCrc.Final();
                            titleKeyBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            titleKeyBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            titleKeyBlock.compression = CompressionType.Lzma;

                            endCompress = DateTime.Now;

                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Took {0} seconds to compress decrypted DVD title keys",
                                                       (endCompress - startCompress).TotalSeconds);
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref titleKeyBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(titleKeyBlock.compression == CompressionType.Lzma ||
                           titleKeyBlock.compression == CompressionType.LzmaClauniaSubchannelTransform)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock &&
                                              t.dataType  == DataType.DvdSectorTitleKeyDecrypted);

                        _index.Add(idxEntry);
                        _blockStream.ReallyClose();
                        _blockStream = null;
                    }

                    List<TrackEntry>            trackEntries            = new List<TrackEntry>();
                    List<CompactDiscIndexEntry> compactDiscIndexEntries = new List<CompactDiscIndexEntry>();

                    foreach(Track track in Tracks)
                    {
                        _trackFlags.TryGetValue((byte)track.TrackSequence, out byte flags);
                        _trackIsrcs.TryGetValue((byte)track.TrackSequence, out string isrc);

                        if((flags & (int)CdFlags.DataTrack) == 0 &&
                           track.TrackType                  != TrackType.Audio)
                            flags += (byte)CdFlags.DataTrack;

                        trackEntries.Add(new TrackEntry
                        {
                            sequence = (byte)track.TrackSequence,
                            type     = track.TrackType,
                            start    = (long)track.TrackStartSector,
                            end      = (long)track.TrackEndSector,
                            pregap   = (long)track.TrackPregap,
                            session  = (byte)track.TrackSession,
                            isrc     = isrc,
                            flags    = flags
                        });

                        if(!track.Indexes.ContainsKey(0) &&
                           track.TrackPregap > 0)
                        {
                            track.Indexes[0] = (int)track.TrackStartSector;
                            track.Indexes[1] = (int)(track.TrackStartSector + track.TrackPregap);
                        }
                        else if(!track.Indexes.ContainsKey(0) &&
                                !track.Indexes.ContainsKey(1))
                            track.Indexes[0] = (int)track.TrackStartSector;

                        compactDiscIndexEntries.AddRange(track.Indexes.Select(trackIndex => new CompactDiscIndexEntry
                        {
                            Index = trackIndex.Key,
                            Lba   = trackIndex.Value,
                            Track = (ushort)track.TrackSequence
                        }));
                    }

                    // If there are tracks build the tracks block
                    if(trackEntries.Count > 0)
                    {
                        _blockStream = new NonClosableStream();

                        foreach(TrackEntry entry in trackEntries)
                        {
                            _structurePointer =
                                System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<TrackEntry>());

                            _structureBytes = new byte[Marshal.SizeOf<TrackEntry>()];
                            System.Runtime.InteropServices.Marshal.StructureToPtr(entry, _structurePointer, true);

                            System.Runtime.InteropServices.Marshal.Copy(_structurePointer, _structureBytes, 0,
                                                                        _structureBytes.Length);

                            System.Runtime.InteropServices.Marshal.FreeHGlobal(_structurePointer);
                            _blockStream.Write(_structureBytes, 0, _structureBytes.Length);
                        }

                        Crc64Context.Data(_blockStream.ToArray(), out byte[] trksCrc);

                        var trkHeader = new TracksHeader
                        {
                            identifier = BlockType.TracksBlock,
                            entries    = (ushort)trackEntries.Count,
                            crc64      = BitConverter.ToUInt64(trksCrc, 0)
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing tracks to position {0}",
                                                   _imageStream.Position);

                        _index.RemoveAll(t => t.blockType == BlockType.TracksBlock && t.dataType == DataType.NoData);

                        _index.Add(new IndexEntry
                        {
                            blockType = BlockType.TracksBlock,
                            dataType  = DataType.NoData,
                            offset    = (ulong)_imageStream.Position
                        });

                        _structureBytes = new byte[Marshal.SizeOf<TracksHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref trkHeader);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;
                    }

                    // If there are track indexes bigger than 1
                    if(compactDiscIndexEntries.Any(i => i.Index > 1))
                    {
                        _blockStream = new NonClosableStream();

                        foreach(CompactDiscIndexEntry entry in compactDiscIndexEntries)
                        {
                            _structurePointer =
                                System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.
                                    SizeOf<CompactDiscIndexEntry>());

                            _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexEntry>()];
                            System.Runtime.InteropServices.Marshal.StructureToPtr(entry, _structurePointer, true);

                            System.Runtime.InteropServices.Marshal.Copy(_structurePointer, _structureBytes, 0,
                                                                        _structureBytes.Length);

                            System.Runtime.InteropServices.Marshal.FreeHGlobal(_structurePointer);
                            _blockStream.Write(_structureBytes, 0, _structureBytes.Length);
                        }

                        Crc64Context.Data(_blockStream.ToArray(), out byte[] cdixCrc);

                        var cdixHeader = new CompactDiscIndexesHeader
                        {
                            identifier = BlockType.CompactDiscIndexesBlock,
                            entries    = (ushort)compactDiscIndexEntries.Count,
                            crc64      = BitConverter.ToUInt64(cdixCrc, 0)
                        };

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing compact disc indexes to position {0}",
                                                   _imageStream.Position);

                        _index.RemoveAll(t => t.blockType == BlockType.CompactDiscIndexesBlock &&
                                              t.dataType  == DataType.NoData);

                        _index.Add(new IndexEntry
                        {
                            blockType = BlockType.CompactDiscIndexesBlock,
                            dataType  = DataType.NoData,
                            offset    = (ulong)_imageStream.Position
                        });

                        _structureBytes = new byte[Marshal.SizeOf<CompactDiscIndexesHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref cdixHeader);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                        _blockStream.ReallyClose();
                        _blockStream = null;
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    if(_sectorSubchannel != null &&
                       (_imageInfo.MediaType == MediaType.AppleFileWare ||
                        _imageInfo.MediaType == MediaType.AppleSonySS   ||
                        _imageInfo.MediaType == MediaType.AppleSonyDS   ||
                        _imageInfo.MediaType == MediaType.AppleProfile  ||
                        _imageInfo.MediaType == MediaType.AppleWidget   ||
                        _imageInfo.MediaType == MediaType.PriamDataTower))
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
                                                   "Writing apple sector tag block to position {0}", idxEntry.offset);

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
                            _blockStream                = new NonClosableStream(_sectorSubchannel);
                        }
                        else
                        {
                            _blockStream = new NonClosableStream();

                            lzmaProperties =
                                CompressDataToStreamWithLZMA(_sectorSubchannel, _lzmaEncoderProperties, _blockStream);

                            var cmpCrc = new Crc64Context();
                            cmpCrc.Update(lzmaProperties);
                            cmpCrc.Update(_blockStream.ToArray());
                            blockCrc                    = cmpCrc.Final();
                            subchannelBlock.cmpLength   = (uint)_blockStream.Length + LZMA_PROPERTIES_LENGTH;
                            subchannelBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                            subchannelBlock.compression = CompressionType.Lzma;
                        }

                        _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        MemoryMarshal.Write(_structureBytes, ref subchannelBlock);
                        _imageStream.Write(_structureBytes, 0, _structureBytes.Length);

                        if(subchannelBlock.compression == CompressionType.Lzma)
                            _imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);

                        _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);

                        _index.RemoveAll(t => t.blockType == BlockType.DataBlock && t.dataType == tagType);

                        _index.Add(idxEntry);
                        _blockStream.ReallyClose();
                        _blockStream = null;
                    }

                    break;
            }

            // Write metadata if present
            SetMetadataFromTags();
            var metadataBlock = new MetadataBlock();
            _blockStream = new NonClosableStream();
            _blockStream.Write(new byte[Marshal.SizeOf<MetadataBlock>()], 0, Marshal.SizeOf<MetadataBlock>());
            byte[] tmpUtf16Le;

            if(_imageInfo.MediaSequence     > 0 &&
               _imageInfo.LastMediaSequence > 0)
            {
                metadataBlock.identifier        = BlockType.MetadataBlock;
                metadataBlock.mediaSequence     = _imageInfo.MediaSequence;
                metadataBlock.lastMediaSequence = _imageInfo.LastMediaSequence;
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.Creator))
            {
                tmpUtf16Le                  = Encoding.Unicode.GetBytes(_imageInfo.Creator);
                metadataBlock.identifier    = BlockType.MetadataBlock;
                metadataBlock.creatorOffset = (uint)_blockStream.Position;
                metadataBlock.creatorLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.Comments))
            {
                tmpUtf16Le                   = Encoding.Unicode.GetBytes(_imageInfo.Comments);
                metadataBlock.identifier     = BlockType.MetadataBlock;
                metadataBlock.commentsOffset = (uint)_blockStream.Position;
                metadataBlock.commentsLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.MediaTitle))
            {
                tmpUtf16Le                     = Encoding.Unicode.GetBytes(_imageInfo.MediaTitle);
                metadataBlock.identifier       = BlockType.MetadataBlock;
                metadataBlock.mediaTitleOffset = (uint)_blockStream.Position;
                metadataBlock.mediaTitleLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.MediaManufacturer))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.MediaManufacturer);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.mediaManufacturerOffset = (uint)_blockStream.Position;
                metadataBlock.mediaManufacturerLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.MediaModel))
            {
                tmpUtf16Le                     = Encoding.Unicode.GetBytes(_imageInfo.MediaModel);
                metadataBlock.identifier       = BlockType.MetadataBlock;
                metadataBlock.mediaModelOffset = (uint)_blockStream.Position;
                metadataBlock.mediaModelLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.MediaSerialNumber))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.MediaSerialNumber);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.mediaSerialNumberOffset = (uint)_blockStream.Position;
                metadataBlock.mediaSerialNumberLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.MediaBarcode))
            {
                tmpUtf16Le                       = Encoding.Unicode.GetBytes(_imageInfo.MediaBarcode);
                metadataBlock.identifier         = BlockType.MetadataBlock;
                metadataBlock.mediaBarcodeOffset = (uint)_blockStream.Position;
                metadataBlock.mediaBarcodeLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.MediaPartNumber))
            {
                tmpUtf16Le                          = Encoding.Unicode.GetBytes(_imageInfo.MediaPartNumber);
                metadataBlock.identifier            = BlockType.MetadataBlock;
                metadataBlock.mediaPartNumberOffset = (uint)_blockStream.Position;
                metadataBlock.mediaPartNumberLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.DriveManufacturer);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.driveManufacturerOffset = (uint)_blockStream.Position;
                metadataBlock.driveManufacturerLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
            {
                tmpUtf16Le                     = Encoding.Unicode.GetBytes(_imageInfo.DriveModel);
                metadataBlock.identifier       = BlockType.MetadataBlock;
                metadataBlock.driveModelOffset = (uint)_blockStream.Position;
                metadataBlock.driveModelLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(_imageInfo.DriveSerialNumber);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.driveSerialNumberOffset = (uint)_blockStream.Position;
                metadataBlock.driveSerialNumberLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
            {
                tmpUtf16Le                                = Encoding.Unicode.GetBytes(_imageInfo.DriveFirmwareRevision);
                metadataBlock.identifier                  = BlockType.MetadataBlock;
                metadataBlock.driveFirmwareRevisionOffset = (uint)_blockStream.Position;
                metadataBlock.driveFirmwareRevisionLength = (uint)(tmpUtf16Le.Length + 2);
                _blockStream.Write(tmpUtf16Le, 0, tmpUtf16Le.Length);

                _blockStream.Write(new byte[]
                {
                    0, 0
                }, 0, 2);
            }

            // Check if we set up any metadata earlier, then write its block
            if(metadataBlock.identifier == BlockType.MetadataBlock)
            {
                AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing metadata to position {0}",
                                           _imageStream.Position);

                metadataBlock.blockSize = (uint)_blockStream.Length;
                _structureBytes         = new byte[Marshal.SizeOf<MetadataBlock>()];
                MemoryMarshal.Write(_structureBytes, ref metadataBlock);
                _blockStream.Position = 0;
                _blockStream.Write(_structureBytes, 0, _structureBytes.Length);
                _index.RemoveAll(t => t.blockType == BlockType.MetadataBlock && t.dataType == DataType.NoData);

                _index.Add(new IndexEntry
                {
                    blockType = BlockType.MetadataBlock,
                    dataType  = DataType.NoData,
                    offset    = (ulong)_imageStream.Position
                });

                _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
                _blockStream.ReallyClose();
                _blockStream = null;
            }

            _header.indexOffset = (ulong)_imageStream.Position;

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing index to position {0}", _header.indexOffset);

            _blockStream = new NonClosableStream();

            // Write index to memory
            foreach(IndexEntry entry in _index)
            {
                _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                IndexEntry indexEntry = entry;
                MemoryMarshal.Write(_structureBytes, ref indexEntry);
                _blockStream.Write(_structureBytes, 0, _structureBytes.Length);
            }

            Crc64Context.Data(_blockStream.ToArray(), out byte[] idxCrc);

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
                MemoryMarshal.Write(_structureBytes, ref idxHeader);
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
                MemoryMarshal.Write(_structureBytes, ref idxHeader);
                _imageStream.Write(_structureBytes, 0, _structureBytes.Length);
            }

            // Write index to disk
            _imageStream.Write(_blockStream.ToArray(), 0, (int)_blockStream.Length);
            _blockStream.ReallyClose();
            _blockStream = null;

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Writing header");
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
        public bool SetMetadata(ImageInfo metadata)
        {
            _imageInfo.Creator               = metadata.Creator;
            _imageInfo.Comments              = metadata.Comments;
            _imageInfo.MediaManufacturer     = metadata.MediaManufacturer;
            _imageInfo.MediaModel            = metadata.MediaModel;
            _imageInfo.MediaSerialNumber     = metadata.MediaSerialNumber;
            _imageInfo.MediaBarcode          = metadata.MediaBarcode;
            _imageInfo.MediaPartNumber       = metadata.MediaPartNumber;
            _imageInfo.MediaSequence         = metadata.MediaSequence;
            _imageInfo.LastMediaSequence     = metadata.LastMediaSequence;
            _imageInfo.DriveManufacturer     = metadata.DriveManufacturer;
            _imageInfo.DriveModel            = metadata.DriveModel;
            _imageInfo.DriveSerialNumber     = metadata.DriveSerialNumber;
            _imageInfo.DriveFirmwareRevision = metadata.DriveFirmwareRevision;
            _imageInfo.MediaTitle            = metadata.MediaTitle;

            return true;
        }

        /// <inheritdoc />
        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(_imageInfo.XmlMediaType != XmlMediaType.BlockMedia)
            {
                ErrorMessage = "Tried to set geometry on a media that doesn't support it";

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
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(sectorAddress >= _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            Track track;

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                    if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                    {
                        ErrorMessage = "Incorrect tag for disk type";

                        return false;
                    }

                    track = Tracks.FirstOrDefault(trk => sectorAddress == trk.TrackSequence);

                    if(track is null ||
                       (track.TrackSequence == 0 && track.TrackStartSector == 0 && track.TrackEndSector == 0))
                    {
                        ErrorMessage = $"Can't find track {sectorAddress}";

                        return false;
                    }

                    break;
                case SectorTagType.CdSectorSubchannel:
                    if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                    {
                        ErrorMessage = "Incorrect tag for disk type";

                        return false;
                    }

                    track = Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                         sectorAddress <= trk.TrackEndSector);

                    if(track is { TrackSequence: 0, TrackStartSector: 0, TrackEndSector: 0 })
                        track.TrackType = TrackType.Data;

                    break;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                {
                    if(data.Length != 1)
                    {
                        ErrorMessage = "Incorrect data size for track flags";

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
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    _sectorSubchannel ??= new byte[_imageInfo.Sectors * 96];

                    Array.Copy(data, 0, _sectorSubchannel, (int)(96 * sectorAddress), 96);

                    return true;
                }

                case SectorTagType.DvdCmi:
                {
                    if(data.Length != 1)
                    {
                        ErrorMessage = "Incorrect data size for CMI";

                        return false;
                    }

                    _sectorCpiMai ??= new byte[_imageInfo.Sectors * 6];

                    Array.Copy(data, 0, _sectorCpiMai, (int)(6 * sectorAddress), 1);

                    return true;
                }
                case SectorTagType.DvdTitleKey:
                {
                    if(data.Length != 5)
                    {
                        ErrorMessage = "Incorrect data size for title key";

                        return false;
                    }

                    _sectorCpiMai ??= new byte[_imageInfo.Sectors * 6];

                    Array.Copy(data, 0, _sectorCpiMai, (int)(1 + (6 * sectorAddress)), 5);

                    return true;
                }
                case SectorTagType.DvdTitleKeyDecrypted:
                {
                    if(data.Length != 5)
                    {
                        ErrorMessage = "Incorrect data size for decrypted title key";

                        return false;
                    }

                    _sectorDecryptedTitleKey ??= new byte[_imageInfo.Sectors * 5];

                    Array.Copy(data, 0, _sectorDecryptedTitleKey, (int)(5 * sectorAddress), 5);

                    return true;
                }

                default:
                    ErrorMessage = $"Don't know how to write sector tag type {tag}";

                    return false;
            }
        }

        /// <inheritdoc />
        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(sectorAddress + length > _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

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
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    _sectorSubchannel ??= new byte[_imageInfo.Sectors * 96];

                    if((sectorAddress * 96) + (length * 96) > (ulong)_sectorSubchannel.LongLength)
                    {
                        ErrorMessage = "Tried to write more data than possible";

                        return false;
                    }

                    Array.Copy(data, 0, _sectorSubchannel, (int)(96 * sectorAddress), 96 * length);

                    return true;
                }

                default:
                    ErrorMessage = $"Don't know how to write sector tag type {tag}";

                    return false;
            }
        }

        /// <inheritdoc />
        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            DumpHardware = dumpHardware;

            return true;
        }

        /// <inheritdoc />
        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            CicmMetadata = metadata;

            return true;
        }

        /// <summary>This method exists to ensure the .NET memory allocator frees the LZ tree on each call</summary>
        /// <param name="data">Data to compress</param>
        /// <param name="properties">LZMA properties</param>
        /// <param name="stream">Stream where to write the compressed data to</param>
        /// <returns>The properties as a byte array</returns>
        static byte[] CompressDataToStreamWithLZMA(byte[] data, LzmaEncoderProperties properties, Stream stream)
        {
            using var lzmaStream = new LzmaStream(properties, false, stream);

            lzmaStream.Write(data, 0, data.Length);
            byte[] propertiesArray = new byte[lzmaStream.Properties.Length];
            lzmaStream.Properties.CopyTo(propertiesArray, 0);

            return propertiesArray;
        }
    }
}