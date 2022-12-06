﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Aaru Format disk images.
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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
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
        public bool Open(IFilter imageFilter)
        {
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            _imageStream = imageFilter.GetDataForkStream();
            _imageStream.Seek(0, SeekOrigin.Begin);

            if(_imageStream.Length < Marshal.SizeOf<AaruHeader>())
                return false;

            _structureBytes = new byte[Marshal.SizeOf<AaruHeader>()];
            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
            _header = Marshal.ByteArrayToStructureLittleEndian<AaruHeader>(_structureBytes);

            if(_header.imageMajorVersion > AARUFMT_VERSION)
                throw
                    new FeatureUnsupportedImageException($"Image version {_header.imageMajorVersion} not recognized.");

            _imageInfo.Application        = _header.application;
            _imageInfo.ApplicationVersion = $"{_header.applicationMajorVersion}.{_header.applicationMinorVersion}";
            _imageInfo.Version            = $"{_header.imageMajorVersion}.{_header.imageMinorVersion}";
            _imageInfo.MediaType          = _header.mediaType;

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Read the index header
            _imageStream.Position = (long)_header.indexOffset;
            _structureBytes       = new byte[Marshal.SizeOf<IndexHeader>()];
            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
            IndexHeader idxHeader = Marshal.SpanToStructureLittleEndian<IndexHeader>(_structureBytes);

            if(idxHeader.identifier != BlockType.Index &&
               idxHeader.identifier != BlockType.Index2)
                throw new FeatureUnsupportedImageException("Index not found!");

            if(idxHeader.identifier == BlockType.Index2)
            {
                _imageStream.Position = (long)_header.indexOffset;
                _structureBytes       = new byte[Marshal.SizeOf<IndexHeader2>()];
                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                IndexHeader2 idxHeader2 = Marshal.SpanToStructureLittleEndian<IndexHeader2>(_structureBytes);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries",
                                           _header.indexOffset, idxHeader2.entries);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                // Fill in-memory index
                _index = new List<IndexEntry>();

                for(ulong i = 0; i < idxHeader2.entries; i++)
                {
                    _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                    _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                    IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(_structureBytes);

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               "Block type {0} with data type {1} is indexed to be at {2}",
                                               entry.blockType, entry.dataType, entry.offset);

                    _index.Add(entry);
                }
            }
            else
            {
                AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries",
                                           _header.indexOffset, idxHeader.entries);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                // Fill in-memory index
                _index = new List<IndexEntry>();

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
            }

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            _imageInfo.ImageSize = 0;

            bool foundUserDataDdt = false;
            _mediaTags = new Dictionary<MediaTagType, byte[]>();
            List<CompactDiscIndexEntry> compactDiscIndexes = null;

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
                        {
                            if(blockHeader.sectorSize > _imageInfo.SectorSize)
                                _imageInfo.SectorSize = blockHeader.sectorSize;

                            break;
                        }

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

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found data block type {0} at position {1}",
                                                   entry.dataType, entry.offset);

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
                                    _sectorPrefixMs ??= new NonClosableStream();
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
                                    _sectorSuffixMs ??= new NonClosableStream();
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
                        _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                        _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                        DdtHeader ddtHeader = Marshal.SpanToStructureLittleEndian<DdtHeader>(_structureBytes);
                        _imageInfo.ImageSize += ddtHeader.cmpLength;

                        if(ddtHeader.identifier != BlockType.DeDuplicationTable)
                            break;

                        switch(entry.dataType)
                        {
                            case DataType.UserData:
                                _imageInfo.Sectors = ddtHeader.entries;
                                _shift             = ddtHeader.shift;

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                // Check for DDT compression
                                switch(ddtHeader.compression)
                                {
                                    case CompressionType.Lzma:
                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");
                                        DateTime ddtStart = DateTime.UtcNow;
                                        byte[]   compressedDdt = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                        byte[]   lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
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

                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                                   GC.GetTotalMemory(false));

                                        break;
                                    case CompressionType.None:
                                        _inMemoryDdt          = false;
                                        _outMemoryDdtPosition = (long)entry.offset;

                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                                   GC.GetTotalMemory(false));

                                        break;
                                    default:
                                        throw new
                                            ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");
                                }

                                foundUserDataDdt = true;

                                break;
                            case DataType.CdSectorPrefixCorrected:
                            case DataType.CdSectorSuffixCorrected:
                            {
                                byte[] decompressedDdt = new byte[ddtHeader.length];

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                // Check for DDT compression
                                switch(ddtHeader.compression)
                                {
                                    case CompressionType.Lzma:
                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");
                                        DateTime ddtStart = DateTime.UtcNow;
                                        byte[]   compressedDdt = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                        byte[]   lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
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

                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                                   GC.GetTotalMemory(false));

                                        break;
                                    case CompressionType.None:
                                        _imageStream.Read(decompressedDdt, 0, decompressedDdt.Length);

                                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                                   GC.GetTotalMemory(false));

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
                                        _sectorPrefixMs  = new NonClosableStream();

                                        break;
                                    case DataType.CdSectorSuffixCorrected:
                                        _sectorSuffixDdt = cdDdt;
                                        _sectorSuffixMs  = new NonClosableStream();

                                        break;
                                }

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

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
                            _imageInfo.Creator = Encoding.Unicode.GetString(metadata, (int)metadataBlock.creatorOffset,
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

                        if(metadataBlock.mediaPartNumberOffset                                       > 0 &&
                           metadataBlock.mediaPartNumberLength + metadataBlock.mediaPartNumberOffset <= metadata.Length)
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
                        TracksHeader tracksHeader = Marshal.SpanToStructureLittleEndian<TracksHeader>(_structureBytes);

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
                                TrackFile        = imageFilter.GetFilename(),
                                TrackFileType    = "BINARY",
                                TrackFilter      = imageFilter
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

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

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
                                                       "Exception {0} processing CICM XML metadata block", ex.Message);

                            CicmMetadata = null;
                        }

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

                        break;

                    // Dump hardware block
                    case BlockType.DumpHardwareBlock:
                        _structureBytes = new byte[Marshal.SizeOf<DumpHardwareHeader>()];
                        _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                        DumpHardwareHeader dumpBlock =
                            Marshal.SpanToStructureLittleEndian<DumpHardwareHeader>(_structureBytes);

                        if(dumpBlock.identifier != BlockType.DumpHardwareBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found dump hardware block at position {0}",
                                                   entry.offset);

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

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
                                tmp = new byte[dumpEntry.softwareOperatingSystemLength - 1];
                                _imageStream.Read(tmp, 0, tmp.Length);
                                _imageStream.Position         += 1;
                                dump.Software.OperatingSystem =  Encoding.UTF8.GetString(tmp);
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

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

                        break;

                    // Tape partition block
                    case BlockType.TapePartitionBlock:
                        _structureBytes = new byte[Marshal.SizeOf<TapePartitionHeader>()];
                        _imageStream.Read(_structureBytes, 0, _structureBytes.Length);

                        TapePartitionHeader partitionHeader =
                            Marshal.SpanToStructureLittleEndian<TapePartitionHeader>(_structureBytes);

                        if(partitionHeader.identifier != BlockType.TapePartitionBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found tape partition block at position {0}",
                                                   entry.offset);

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
                throw new ImageNotSupportedException("Could not find user data deduplication table.");

            _imageInfo.CreationTime = DateTime.FromFileTimeUtc(_header.creationTime);
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Image created on {0}", _imageInfo.CreationTime);
            _imageInfo.LastModificationTime = DateTime.FromFileTimeUtc(_header.lastWrittenTime);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Image last written on {0}",
                                       _imageInfo.LastModificationTime);

            _imageInfo.XmlMediaType = GetXmlMediaType(_header.mediaType);

            if(_geometryBlock.identifier != BlockType.GeometryBlock &&
               _imageInfo.XmlMediaType   == XmlMediaType.BlockMedia)
            {
                _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
                _imageInfo.Heads           = 16;
                _imageInfo.SectorsPerTrack = 63;
            }

            _imageInfo.ReadableMediaTags.AddRange(_mediaTags.Keys);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Initialize caches
            _blockCache       = new Dictionary<ulong, byte[]>();
            _blockHeaderCache = new Dictionary<ulong, BlockHeader>();
            _currentCacheSize = 0;

            if(!_inMemoryDdt)
                _ddtEntryCache = new Dictionary<ulong, ulong>();

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Initialize tracks, sessions and partitions
            if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(Tracks != null)
                {
                    bool leadOutFixed       = false;
                    bool sessionPregapFixed = false;

                    if(_mediaTags.TryGetValue(MediaTagType.CD_FullTOC, out byte[] fullToc))
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
                                leadOutFixed                      = true;
                            }
                        }
                    }

                    if(_header.imageMajorVersion <= 1)
                    {
                        foreach(Track track in Tracks)
                        {
                            if(track.TrackSequence <= 1)
                                continue;

                            uint firstTrackNumberInSameSession = Tracks.
                                                                 Where(t => t.TrackSession == track.TrackSession).
                                                                 Min(t => t.TrackSequence);

                            if(firstTrackNumberInSameSession != track.TrackSequence)
                                continue;

                            if(track.TrackPregap == 150)
                                continue;

                            long dif = (long)track.TrackPregap                            - 150;
                            track.TrackPregap      = (ulong)((long)track.TrackPregap      - dif);
                            track.TrackStartSector = (ulong)((long)track.TrackStartSector + dif);

                            sessionPregapFixed = true;
                        }
                    }

                    if(leadOutFixed)
                        AaruConsole.ErrorWriteLine("This image has a corrupted track list, convert will fix it.");

                    if(sessionPregapFixed)
                        AaruConsole.
                            ErrorWriteLine("This image has a corrupted track list, a best effort has been tried but may require manual editing or redump.");
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
                            TrackFile              = imageFilter.GetFilename(),
                            TrackFileType          = "BINARY",
                            TrackFilter            = imageFilter,
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
                    switch(track.TrackSequence)
                    {
                        case 0:
                            track.TrackPregap = 150;
                            track.Indexes[0]  = -150;
                            track.Indexes[1]  = (int)track.TrackStartSector;

                            continue;
                        case 1 when Tracks.All(t => t.TrackSequence != 0):
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
                        Type = track.TrackType.ToString(),
                        Name = $"Track {track.TrackSequence}",
                        Offset = currentTrackOffset,
                        Start = (ulong)track.Indexes[1],
                        Size = (track.TrackEndSector - (ulong)track.Indexes[1] + 1) * (ulong)track.TrackBytesPerSector,
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

            SetMetadataFromTags();

            if(_sectorSuffixDdt != null)
                EccInit();

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                return true;

            if(_imageInfo.MediaType == MediaType.CD            ||
               _imageInfo.MediaType == MediaType.CDDA          ||
               _imageInfo.MediaType == MediaType.CDG           ||
               _imageInfo.MediaType == MediaType.CDEG          ||
               _imageInfo.MediaType == MediaType.CDI           ||
               _imageInfo.MediaType == MediaType.CDROM         ||
               _imageInfo.MediaType == MediaType.CDROMXA       ||
               _imageInfo.MediaType == MediaType.CDPLUS        ||
               _imageInfo.MediaType == MediaType.CDMO          ||
               _imageInfo.MediaType == MediaType.CDR           ||
               _imageInfo.MediaType == MediaType.CDRW          ||
               _imageInfo.MediaType == MediaType.CDMRW         ||
               _imageInfo.MediaType == MediaType.VCD           ||
               _imageInfo.MediaType == MediaType.SVCD          ||
               _imageInfo.MediaType == MediaType.PCD           ||
               _imageInfo.MediaType == MediaType.DTSCD         ||
               _imageInfo.MediaType == MediaType.CDMIDI        ||
               _imageInfo.MediaType == MediaType.CDV           ||
               _imageInfo.MediaType == MediaType.CDIREADY      ||
               _imageInfo.MediaType == MediaType.FMTOWNS       ||
               _imageInfo.MediaType == MediaType.PS1CD         ||
               _imageInfo.MediaType == MediaType.PS2CD         ||
               _imageInfo.MediaType == MediaType.MEGACD        ||
               _imageInfo.MediaType == MediaType.SATURNCD      ||
               _imageInfo.MediaType == MediaType.GDROM         ||
               _imageInfo.MediaType == MediaType.GDR           ||
               _imageInfo.MediaType == MediaType.MilCD         ||
               _imageInfo.MediaType == MediaType.SuperCDROM2   ||
               _imageInfo.MediaType == MediaType.JaguarCD      ||
               _imageInfo.MediaType == MediaType.ThreeDO       ||
               _imageInfo.MediaType == MediaType.PCFX          ||
               _imageInfo.MediaType == MediaType.NeoGeoCD      ||
               _imageInfo.MediaType == MediaType.CDTV          ||
               _imageInfo.MediaType == MediaType.CD32          ||
               _imageInfo.MediaType == MediaType.Playdia       ||
               _imageInfo.MediaType == MediaType.Pippin        ||
               _imageInfo.MediaType == MediaType.VideoNow      ||
               _imageInfo.MediaType == MediaType.VideoNowColor ||
               _imageInfo.MediaType == MediaType.VideoNowXp    ||
               _imageInfo.MediaType == MediaType.CVD)
                return true;

            {
                foreach(Track track in Tracks)
                {
                    track.TrackPregap = 0;
                    track.Indexes?.Clear();
                }
            }

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(_mediaTags.TryGetValue(tag, out byte[] data))
                return data;

            throw new FeatureNotPresentImageException("Requested tag is not present in image");
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            ulong ddtEntry    = GetDdtEntry(sectorAddress);
            uint  offsetMask  = (uint)((1 << _shift) - 1);
            ulong offset      = ddtEntry & offsetMask;
            ulong blockOffset = ddtEntry >> _shift;

            // Partially written image... as we can't know the real sector size just assume it's common :/
            if(ddtEntry == 0)
                return new byte[_imageInfo.SectorSize];

            byte[] sector;

            // Check if block is cached
            if(_blockCache.TryGetValue(blockOffset, out byte[] block) &&
               _blockHeaderCache.TryGetValue(blockOffset, out BlockHeader blockHeader))
            {
                sector = new byte[blockHeader.sectorSize];
                Array.Copy(block, (long)(offset * blockHeader.sectorSize), sector, 0, blockHeader.sectorSize);

                return sector;
            }

            // Read block header
            _imageStream.Position = (long)blockOffset;
            _structureBytes       = new byte[Marshal.SizeOf<BlockHeader>()];
            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
            blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(_structureBytes);

            // Decompress block
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            switch(blockHeader.compression)
            {
                case CompressionType.None:
                    block = new byte[blockHeader.length];
                    _imageStream.Read(block, 0, (int)blockHeader.length);

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    break;
                case CompressionType.Lzma:
                    byte[] compressedBlock = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                    byte[] lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                    _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                    _imageStream.Read(compressedBlock, 0, compressedBlock.Length);
                    var compressedBlockMs = new MemoryStream(compressedBlock);
                    var lzmaBlock         = new LzmaStream(lzmaProperties, compressedBlockMs);
                    block = new byte[blockHeader.length];
                    lzmaBlock.Read(block, 0, (int)blockHeader.length);
                    lzmaBlock.Close();
                    compressedBlockMs.Close();

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    break;
                case CompressionType.Flac:
                    byte[] flacBlock = new byte[blockHeader.cmpLength];
                    _imageStream.Read(flacBlock, 0, flacBlock.Length);
                    var flacMs      = new MemoryStream(flacBlock);
                    var flakeReader = new AudioDecoder(new DecoderSettings(), "", flacMs);
                    block = new byte[blockHeader.length];
                    int samples     = (int)(block.Length / blockHeader.sectorSize * 588);
                    var audioBuffer = new AudioBuffer(AudioPCMConfig.RedBook, block, samples);
                    flakeReader.Read(audioBuffer, samples);
                    flakeReader.Close();
                    flacMs.Close();

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    break;
                default:
                    throw new
                        ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)blockHeader.compression}");
            }

            // Check if cache needs to be emptied
            if(_currentCacheSize + blockHeader.length >= MAX_CACHE_SIZE)
            {
                _currentCacheSize = 0;
                _blockHeaderCache = new Dictionary<ulong, BlockHeader>();
                _blockCache       = new Dictionary<ulong, byte[]>();
            }

            // Add block to cache
            _currentCacheSize += blockHeader.length;
            _blockHeaderCache.Add(blockOffset, blockHeader);
            _blockCache.Add(blockOffset, block);

            sector = new byte[blockHeader.sectorSize];
            Array.Copy(block, (long)(offset * blockHeader.sectorSize), sector, 0, blockHeader.sectorSize);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            return sector;
        }

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk?.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSector(trk.TrackStartSector + sectorAddress);
        }

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk?.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSectorTag(trk.TrackStartSector + sectorAddress, tag);
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            uint   sectorOffset;
            uint   sectorSize;
            uint   sectorSkip;
            byte[] dataSource;

            if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                       sectorAddress <= t.TrackEndSector);

                if(trk is null)
                    throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                          "Can't found track containing requested sector");

                if(trk.TrackSequence    == 0 &&
                   trk.TrackStartSector == 0 &&
                   trk.TrackEndSector   == 0)
                    throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                          "Can't found track containing requested sector");

                if(trk.TrackType == TrackType.Data)
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));

                switch(tag)
                {
                    case SectorTagType.CdSectorEcc:
                    case SectorTagType.CdSectorEccP:
                    case SectorTagType.CdSectorEccQ:
                    case SectorTagType.CdSectorEdc:
                    case SectorTagType.CdSectorHeader:
                    case SectorTagType.CdSectorSubchannel:
                    case SectorTagType.CdSectorSubHeader:
                    case SectorTagType.CdSectorSync:
                    case SectorTagType.DvdCmi:
                    case SectorTagType.DvdTitleKey:
                    case SectorTagType.DvdTitleKeyDecrypted: break;
                    case SectorTagType.CdTrackFlags:
                        return _trackFlags.TryGetValue((byte)sectorAddress, out byte flags) ? new[]
                        {
                            flags
                        } : null;
                    case SectorTagType.CdTrackIsrc:
                        return _trackIsrcs.TryGetValue((byte)sectorAddress, out string isrc)
                                   ? Encoding.UTF8.GetBytes(isrc) : null;
                    default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                }

                switch(trk.TrackType)
                {
                    case TrackType.CdMode1:
                        switch(tag)
                        {
                            case SectorTagType.CdSectorSync:
                            {
                                sectorOffset = 0;
                                sectorSize   = 12;
                                sectorSkip   = 4;
                                dataSource   = _sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorHeader:
                            {
                                sectorOffset = 12;
                                sectorSize   = 4;
                                sectorSkip   = 2336;
                                dataSource   = _sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorSubHeader:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CdSectorEcc:
                            {
                                sectorOffset = 12;
                                sectorSize   = 276;
                                sectorSkip   = 0;
                                dataSource   = _sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorEccP:
                            {
                                sectorOffset = 12;
                                sectorSize   = 172;
                                sectorSkip   = 104;
                                dataSource   = _sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorEccQ:
                            {
                                sectorOffset = 184;
                                sectorSize   = 104;
                                sectorSkip   = 0;
                                dataSource   = _sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorEdc:
                            {
                                sectorOffset = 0;
                                sectorSize   = 4;
                                sectorSkip   = 284;
                                dataSource   = _sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorSubchannel:
                            {
                                sectorOffset = 0;
                                sectorSize   = 96;
                                sectorSkip   = 0;
                                dataSource   = _sectorSubchannel;

                                break;
                            }

                            default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }

                        break;
                    case TrackType.CdMode2Formless:
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CdSectorSync:
                            {
                                sectorOffset = 0;
                                sectorSize   = 12;
                                sectorSkip   = 4;
                                dataSource   = _sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorHeader:
                            {
                                sectorOffset = 12;
                                sectorSize   = 4;
                                sectorSkip   = 0;
                                dataSource   = _sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorSubHeader:
                            {
                                sectorOffset = 0;
                                sectorSize   = 8;
                                sectorSkip   = 0;
                                dataSource   = _mode2Subheaders;

                                break;
                            }

                            // These could be implemented
                            case SectorTagType.CdSectorEcc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorEdc:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CdSectorSubchannel:
                            {
                                sectorOffset = 0;
                                sectorSize   = 96;
                                sectorSkip   = 0;
                                dataSource   = _sectorSubchannel;

                                break;
                            }

                            default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }

                        break;
                    }

                    case TrackType.Audio:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CdSectorSubchannel:
                            {
                                sectorOffset = 0;
                                sectorSize   = 96;
                                sectorSkip   = 0;
                                dataSource   = _sectorSubchannel;

                                break;
                            }

                            default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }

                        break;
                    }

                    case TrackType.Data:
                    {
                        if(_imageInfo.MediaType == MediaType.DVDROM)
                        {
                            switch(tag)
                            {
                                case SectorTagType.DvdCmi:
                                {
                                    sectorOffset = 0;
                                    sectorSize   = 1;
                                    sectorSkip   = 5;
                                    dataSource   = _sectorCpiMai;

                                    break;
                                }
                                case SectorTagType.DvdTitleKey:
                                {
                                    sectorOffset = 1;
                                    sectorSize   = 5;
                                    sectorSkip   = 0;
                                    dataSource   = _sectorCpiMai;

                                    break;
                                }
                                case SectorTagType.DvdTitleKeyDecrypted:
                                {
                                    sectorOffset = 0;
                                    sectorSize   = 5;
                                    sectorSkip   = 0;
                                    dataSource   = _sectorDecryptedTitleKey;

                                    break;
                                }
                                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }

                        break;
                    }

                    default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
                }
            }
            else
                throw new FeatureNotPresentImageException("Feature not present in image");

            if(dataSource == null)
                throw new ArgumentException("Unsupported tag requested", nameof(tag));

            byte[] data = new byte[sectorSize * length];

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
            {
                Array.Copy(dataSource, (long)(sectorAddress * sectorSize), data, 0, length * sectorSize);

                return data;
            }

            for(int i = 0; i < length; i++)
                Array.Copy(dataSource, (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)), data,
                           i * sectorSize, sectorSize);

            return data;
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk?.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

            return ReadSectors(trk.TrackStartSector + sectorAddress, length);
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk?.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

            return ReadSectorsTag(trk.TrackStartSector + sectorAddress, length, tag);
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            switch(_imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                           sectorAddress <= t.TrackEndSector);

                    if(trk is null)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if(trk.TrackSequence    == 0 &&
                       trk.TrackStartSector == 0 &&
                       trk.TrackEndSector   == 0)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if((_sectorSuffix    == null || _sectorPrefix    == null) &&
                       (_sectorSuffixDdt == null || _sectorPrefixDdt == null))
                        return ReadSector(sectorAddress);

                    byte[] sector = new byte[2352];
                    byte[] data   = ReadSector(sectorAddress);

                    switch(trk.TrackType)
                    {
                        case TrackType.Audio:
                        case TrackType.Data: return data;
                        case TrackType.CdMode1:
                            Array.Copy(data, 0, sector, 16, 2048);

                            if(_sectorPrefix != null)
                                Array.Copy(_sectorPrefix, (int)sectorAddress * 16, sector, 0, 16);
                            else if(_sectorPrefixDdt != null)
                            {
                                if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                    ReconstructPrefix(ref sector, trk.TrackType, (long)sectorAddress);
                                else if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.NotDumped ||
                                        _sectorPrefixDdt[sectorAddress] == 0)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    uint prefixPosition = ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;

                                    if(prefixPosition > _sectorPrefixMs.Length)
                                        throw new
                                            InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                    _sectorPrefixMs.Position = prefixPosition;

                                    _sectorPrefixMs.Read(sector, 0, 16);
                                }
                            }
                            else
                                throw new InvalidProgramException("Should not have arrived here");

                            if(_sectorSuffix != null)
                                Array.Copy(_sectorSuffix, (int)sectorAddress * 288, sector, 2064, 288);
                            else if(_sectorSuffixDdt != null)
                            {
                                if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                    ReconstructEcc(ref sector, trk.TrackType);
                                else if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.NotDumped ||
                                        _sectorSuffixDdt[sectorAddress] == 0)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    uint suffixPosition = ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;

                                    if(suffixPosition > _sectorSuffixMs.Length)
                                        throw new
                                            InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                    _sectorSuffixMs.Position = suffixPosition;

                                    _sectorSuffixMs.Read(sector, 2064, 288);
                                }
                            }
                            else
                                throw new InvalidProgramException("Should not have arrived here");

                            return sector;
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            if(_sectorPrefix != null)
                                Array.Copy(_sectorPrefix, (int)sectorAddress * 16, sector, 0, 16);
                            else if(_sectorPrefixMs != null)
                            {
                                if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                    ReconstructPrefix(ref sector, trk.TrackType, (long)sectorAddress);
                                else if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.NotDumped ||
                                        _sectorPrefixDdt[sectorAddress] == 0)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    uint prefixPosition = ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;

                                    if(prefixPosition > _sectorPrefixMs.Length)
                                        throw new
                                            InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                    _sectorPrefixMs.Position = prefixPosition;

                                    _sectorPrefixMs.Read(sector, 0, 16);
                                }
                            }
                            else
                                throw new InvalidProgramException("Should not have arrived here");

                            if(_mode2Subheaders != null &&
                               _sectorSuffixDdt != null)
                            {
                                Array.Copy(_mode2Subheaders, (int)sectorAddress * 8, sector, 16, 8);

                                if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Mode2Form1Ok)
                                {
                                    Array.Copy(data, 0, sector, 24, 2048);
                                    ReconstructEcc(ref sector, TrackType.CdMode2Form1);
                                }
                                else if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.Mode2Form2Ok ||
                                        (_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.Mode2Form2NoCrc)
                                {
                                    Array.Copy(data, 0, sector, 24, 2324);

                                    if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                       (uint)CdFixFlags.Mode2Form2Ok)
                                        ReconstructEcc(ref sector, TrackType.CdMode2Form2);
                                }
                                else if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.NotDumped ||
                                        _sectorSuffixDdt[sectorAddress] == 0)
                                {
                                    // Do nothing
                                }
                                else // Mode 2 where EDC failed
                                {
                                    // Incorrectly written images
                                    if(data.Length == 2328)
                                    {
                                        Array.Copy(data, 0, sector, 24, 2328);
                                    }
                                    else
                                    {
                                        bool form2 = (sector[18] & 0x20) == 0x20 || (sector[22] & 0x20) == 0x20;

                                        uint suffixPosition =
                                            ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;

                                        if(suffixPosition > _sectorSuffixMs.Length)
                                            throw new
                                                InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                        _sectorSuffixMs.Position = suffixPosition;

                                        _sectorSuffixMs.Read(sector, form2 ? 2348 : 2072, form2 ? 4 : 280);
                                        Array.Copy(data, 0, sector, 24, form2 ? 2324 : 2048);
                                    }
                                }
                            }
                            else if(_mode2Subheaders != null)
                            {
                                Array.Copy(_mode2Subheaders, (int)sectorAddress * 8, sector, 16, 8);
                                Array.Copy(data, 0, sector, 24, 2328);
                            }
                            else
                                Array.Copy(data, 0, sector, 16, 2336);

                            return sector;
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    switch(_imageInfo.MediaType)
                    {
                        case MediaType.AppleFileWare:
                        case MediaType.AppleProfile:
                        case MediaType.AppleSonySS:
                        case MediaType.AppleSonyDS:
                        case MediaType.AppleWidget:
                        case MediaType.PriamDataTower: return ReadSectorsLong(sectorAddress, 1);
                    }

                    break;
            }

            throw new FeatureNotPresentImageException("Feature not present in image");
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk?.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSectorLong(trk.TrackStartSector + sectorAddress);
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            byte[] sectors;
            byte[] data;

            switch(_imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                           sectorAddress <= t.TrackEndSector);

                    if(trk is null)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if(trk.TrackSequence    == 0 &&
                       trk.TrackStartSector == 0 &&
                       trk.TrackEndSector   == 0)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if(sectorAddress + length > trk.TrackEndSector + 1)
                        throw new ArgumentOutOfRangeException(nameof(length),
                                                              $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

                    switch(trk.TrackType)
                    {
                        // These types only contain user data
                        case TrackType.Audio:
                        case TrackType.Data: return ReadSectors(sectorAddress, length);

                        // Join prefix (sync, header) with user data with suffix (edc, ecc p, ecc q)
                        case TrackType.CdMode1:
                            if(_sectorPrefix != null &&
                               _sectorSuffix != null)
                            {
                                sectors = new byte[2352 * length];
                                data    = ReadSectors(sectorAddress, length);

                                for(uint i = 0; i < length; i++)
                                {
                                    Array.Copy(_sectorPrefix, (int)((sectorAddress + i) * 16), sectors, (int)(i * 2352),
                                               16);

                                    Array.Copy(data, (int)(i * 2048), sectors, (int)(i * 2352) + 16, 2048);

                                    Array.Copy(_sectorSuffix, (int)((sectorAddress + i) * 288), sectors,
                                               (int)(i * 2352) + 2064, 288);
                                }

                                return sectors;
                            }
                            else if(_sectorPrefixDdt != null &&
                                    _sectorSuffixDdt != null)
                            {
                                sectors = new byte[2352 * length];

                                for(uint i = 0; i < length; i++)
                                {
                                    byte[] temp = ReadSectorLong(sectorAddress + i);
                                    Array.Copy(temp, 0, sectors, 2352 * i, 2352);
                                }

                                return sectors;
                            }
                            else
                                return ReadSectors(sectorAddress, length);

                        // Join prefix (sync, header) with user data
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            if(_sectorPrefix != null &&
                               _sectorSuffix != null)
                            {
                                sectors = new byte[2352 * length];
                                data    = ReadSectors(sectorAddress, length);

                                for(uint i = 0; i < length; i++)
                                {
                                    Array.Copy(_sectorPrefix, (int)((sectorAddress + i) * 16), sectors, (int)(i * 2352),
                                               16);

                                    Array.Copy(data, (int)(i * 2336), sectors, (int)(i * 2352) + 16, 2336);
                                }

                                return sectors;
                            }
                            else if(_sectorPrefixDdt != null &&
                                    _sectorSuffixDdt != null)
                            {
                                sectors = new byte[2352 * length];

                                for(uint i = 0; i < length; i++)
                                {
                                    byte[] temp = ReadSectorLong(sectorAddress + i);
                                    Array.Copy(temp, 0, sectors, 2352 * i, 2352);
                                }

                                return sectors;
                            }

                            return ReadSectors(sectorAddress, length);
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    switch(_imageInfo.MediaType)
                    {
                        // Join user data with tags
                        case MediaType.AppleFileWare:
                        case MediaType.AppleProfile:
                        case MediaType.AppleSonySS:
                        case MediaType.AppleSonyDS:
                        case MediaType.AppleWidget:
                        case MediaType.PriamDataTower:
                            if(_sectorSubchannel == null)
                                return ReadSector(sectorAddress);

                            uint tagSize = 0;

                            switch(_imageInfo.MediaType)
                            {
                                case MediaType.AppleFileWare:
                                case MediaType.AppleProfile:
                                case MediaType.AppleWidget:
                                    tagSize = 20;

                                    break;
                                case MediaType.AppleSonySS:
                                case MediaType.AppleSonyDS:
                                    tagSize = 12;

                                    break;
                                case MediaType.PriamDataTower:
                                    tagSize = 24;

                                    break;
                            }

                            uint sectorSize = 512 + tagSize;
                            data    = ReadSectors(sectorAddress, length);
                            sectors = new byte[(sectorSize + 512) * length];

                            for(uint i = 0; i < length; i++)
                            {
                                Array.Copy(_sectorSubchannel, (int)((sectorAddress + i) * tagSize), sectors,
                                           (int)((i * sectorSize) + 512), tagSize);

                                Array.Copy(data, (int)((sectorAddress + i) * 512), sectors, (int)(i * 512), 512);
                            }

                            return sectors;
                    }

                    break;
            }

            throw new FeatureNotPresentImageException("Feature not present in image");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk?.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

            return ReadSectorsLong(trk.TrackStartSector + sectorAddress, length);
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session) =>
            Tracks.Where(t => t.TrackSequence == session.SessionSequence).ToList();

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session) => Tracks.Where(t => t.TrackSequence == session).ToList();
    }
}