// /***************************************************************************
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
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2020-2022 Rebecca Wallander
// ****************************************************************************/

namespace Aaru.DiscImages;

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
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Decoders.CD;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;
using Session = Aaru.CommonTypes.Structs.Session;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

public sealed partial class AaruFormat
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

        _imageStream = imageFilter.GetDataForkStream();
        _imageStream.Seek(0, SeekOrigin.Begin);

        if(_imageStream.Length < Marshal.SizeOf<AaruHeader>())
            return ErrorNumber.InvalidArgument;

        _structureBytes = new byte[Marshal.SizeOf<AaruHeader>()];
        _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
        _header = Marshal.ByteArrayToStructureLittleEndian<AaruHeader>(_structureBytes);

        if(_header.imageMajorVersion > AARUFMT_VERSION)
        {
            AaruConsole.ErrorWriteLine($"Image version {_header.imageMajorVersion} not recognized.");

            return ErrorNumber.NotSupported;
        }

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
        {
            AaruConsole.ErrorWriteLine("Index not found!");

            return ErrorNumber.InvalidArgument;
        }

        if(idxHeader.identifier == BlockType.Index2)
        {
            _imageStream.Position = (long)_header.indexOffset;
            _structureBytes       = new byte[Marshal.SizeOf<IndexHeader2>()];
            _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
            IndexHeader2 idxHeader2 = Marshal.SpanToStructureLittleEndian<IndexHeader2>(_structureBytes);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries", _header.indexOffset,
                                       idxHeader2.entries);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Fill in-memory index
            _index = new List<IndexEntry>();

            for(ulong i = 0; i < idxHeader2.entries; i++)
            {
                _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(_structureBytes);

                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                           "Block type {0} with data type {1} is indexed to be at {2}", entry.blockType,
                                           entry.dataType, entry.offset);

                _index.Add(entry);
            }
        }
        else
        {
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries", _header.indexOffset,
                                       idxHeader.entries);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Fill in-memory index
            _index = new List<IndexEntry>();

            for(ushort i = 0; i < idxHeader.entries; i++)
            {
                _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
                IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(_structureBytes);

                AaruConsole.DebugWriteLine("Aaru Format plugin",
                                           "Block type {0} with data type {1} is indexed to be at {2}", entry.blockType,
                                           entry.dataType, entry.offset);

                _index.Add(entry);
            }
        }

        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

        _imageInfo.ImageSize = 0;

        var foundUserDataDdt = false;
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
                                                   "Incorrect identifier for data block at position {0}", entry.offset);

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
                    if(blockHeader.compression is CompressionType.Lzma
                                               or CompressionType.LzmaClauniaSubchannelTransform)
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
                        var      compressedTag   = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                        var      lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                        _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                        _imageStream.Read(compressedTag, 0, compressedTag.Length);
                        data = new byte[blockHeader.length];
                        int decompressedLength = LZMA.DecodeBuffer(compressedTag, data, lzmaProperties);

                        if(decompressedLength != blockHeader.length)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Error decompressing block, should be {0} bytes but got {1} bytes.",
                                                       blockHeader.length, decompressedLength);

                            return ErrorNumber.InOutError;
                        }

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
                                _sectorPrefixMs ??= new MemoryStream();
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
                                _sectorSuffixMs ??= new MemoryStream();
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
                                    DateTime ddtStart       = DateTime.UtcNow;
                                    var      compressedDdt  = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                    var      lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                    _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                    _imageStream.Read(compressedDdt, 0, compressedDdt.Length);
                                    var decompressedDdt = new byte[ddtHeader.length];

                                    var decompressedLength =
                                        (ulong)LZMA.DecodeBuffer(compressedDdt, decompressedDdt, lzmaProperties);

                                    if(decompressedLength != ddtHeader.length)
                                    {
                                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                                   "Error decompressing DDT, should be {0} bytes but got {1} bytes.",
                                                                   ddtHeader.length, decompressedLength);

                                        return ErrorNumber.InOutError;
                                    }

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
                                    AaruConsole.
                                        ErrorWriteLine($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");

                                    return ErrorNumber.NotSupported;
                            }

                            foundUserDataDdt = true;

                            break;
                        case DataType.CdSectorPrefixCorrected:
                        case DataType.CdSectorSuffixCorrected:
                        {
                            var decompressedDdt = new byte[ddtHeader.length];

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            // Check for DDT compression
                            switch(ddtHeader.compression)
                            {
                                case CompressionType.Lzma:
                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");
                                    DateTime ddtStart       = DateTime.UtcNow;
                                    var      compressedDdt  = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                    var      lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                    _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                    _imageStream.Read(compressedDdt, 0, compressedDdt.Length);

                                    var decompressedLength =
                                        (ulong)LZMA.DecodeBuffer(compressedDdt, decompressedDdt, lzmaProperties);

                                    DateTime ddtEnd = DateTime.UtcNow;

                                    if(decompressedLength != ddtHeader.length)
                                    {
                                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                                   "Error decompressing DDT, should be {0} bytes but got {1} bytes.",
                                                                   ddtHeader.length, decompressedLength);

                                        return ErrorNumber.InOutError;
                                    }

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
                                    AaruConsole.
                                        ErrorWriteLine($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");

                                    return ErrorNumber.NotSupported;
                            }

                            uint[] cdDdt = MemoryMarshal.Cast<byte, uint>(decompressedDdt).ToArray();

                            switch(entry.dataType)
                            {
                                case DataType.CdSectorPrefixCorrected:
                                    _sectorPrefixDdt = cdDdt;
                                    _sectorPrefixMs  = new MemoryStream();

                                    break;
                                case DataType.CdSectorSuffixCorrected:
                                    _sectorSuffixDdt = cdDdt;
                                    _sectorSuffixMs  = new MemoryStream();

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

                    MetadataBlock metadataBlock = Marshal.SpanToStructureLittleEndian<MetadataBlock>(_structureBytes);

                    if(metadataBlock.identifier != entry.blockType)
                    {
                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "Incorrect identifier for data block at position {0}", entry.offset);

                        break;
                    }

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Found metadata block at position {0}",
                                               entry.offset);

                    var metadata = new byte[metadataBlock.blockSize];
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

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting creator: {0}", _imageInfo.Creator);
                    }

                    if(metadataBlock.commentsOffset                                > 0 &&
                       metadataBlock.commentsLength + metadataBlock.commentsOffset <= metadata.Length)
                    {
                        _imageInfo.Comments = Encoding.Unicode.GetString(metadata, (int)metadataBlock.commentsOffset,
                                                                         (int)(metadataBlock.commentsLength - 2));

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting comments: {0}", _imageInfo.Comments);
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

                    if(metadataBlock.mediaManufacturerOffset                                         > 0 &&
                       metadataBlock.mediaManufacturerLength + metadataBlock.mediaManufacturerOffset <= metadata.Length)
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

                    if(metadataBlock.mediaSerialNumberOffset                                         > 0 &&
                       metadataBlock.mediaSerialNumberLength + metadataBlock.mediaSerialNumberOffset <= metadata.Length)
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

                    if(metadataBlock.driveManufacturerOffset                                         > 0 &&
                       metadataBlock.driveManufacturerLength + metadataBlock.driveManufacturerOffset <= metadata.Length)
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

                    if(metadataBlock.driveSerialNumberOffset                                         > 0 &&
                       metadataBlock.driveSerialNumberLength + metadataBlock.driveSerialNumberOffset <= metadata.Length)
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

                        TrackEntry trackEntry = Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(_structureBytes);

                        Tracks.Add(new Track
                        {
                            Sequence    = trackEntry.sequence,
                            Type        = trackEntry.type,
                            StartSector = (ulong)trackEntry.start,
                            EndSector   = (ulong)trackEntry.end,
                            Pregap      = (ulong)trackEntry.pregap,
                            Session     = trackEntry.session,
                            File        = imageFilter.Filename,
                            FileType    = "BINARY",
                            Filter      = imageFilter
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

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Found CICM XML metadata block at position {0}",
                                               entry.offset);

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    var cicmBytes = new byte[cicmBlock.length];
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

                    var tapePartitionBytes = new byte[partitionHeader.length];
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

                    TapeFileHeader fileHeader = Marshal.SpanToStructureLittleEndian<TapeFileHeader>(_structureBytes);

                    if(fileHeader.identifier != BlockType.TapeFileBlock)
                        break;

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Found tape file block at position {0}",
                                               entry.offset);

                    var tapeFileBytes = new byte[fileHeader.length];
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

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Found {0} compact disc indexes at position {0}",
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
            AaruConsole.ErrorWriteLine("Could not find user data deduplication table.");

            return ErrorNumber.InvalidArgument;
        }

        _imageInfo.CreationTime = DateTime.FromFileTimeUtc(_header.creationTime);
        AaruConsole.DebugWriteLine("Aaru Format plugin", "Image created on {0}", _imageInfo.CreationTime);
        _imageInfo.LastModificationTime = DateTime.FromFileTimeUtc(_header.lastWrittenTime);

        AaruConsole.DebugWriteLine("Aaru Format plugin", "Image last written on {0}", _imageInfo.LastModificationTime);

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
                var leadOutFixed       = false;
                var sessionPregapFixed = false;

                if(_mediaTags.TryGetValue(MediaTagType.CD_FullTOC, out byte[] fullToc))
                {
                    var tmp = new byte[fullToc.Length + 2];
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

                            int lastSector = phour * 3600 * 75 + pmin * 60 * 75 + psec * 75 + pframe - 150;
                            leadOutStarts?.Add(trk.SessionNumber, lastSector                         + 1);
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
                            leadOutFixed                 = true;
                        }
                    }
                }

                if(_header.imageMajorVersion <= 1)
                    foreach(Track track in Tracks)
                    {
                        if(track.Sequence <= 1)
                            continue;

                        uint firstTrackNumberInSameSession = Tracks.
                                                             Where(t => t.Session == track.Session).
                                                             Min(t => t.Sequence);

                        if(firstTrackNumberInSameSession != track.Sequence)
                            continue;

                        if(track.Pregap == 150)
                            continue;

                        long dif = (long)track.Pregap                       - 150;
                        track.Pregap      = (ulong)((long)track.Pregap      - dif);
                        track.StartSector = (ulong)((long)track.StartSector + dif);

                        sessionPregapFixed = true;
                    }

                if(leadOutFixed)
                    AaruConsole.ErrorWriteLine("This image has a corrupted track list, convert will fix it.");

                if(sessionPregapFixed)
                    AaruConsole.
                        ErrorWriteLine("This image has a corrupted track list, a best effort has been tried but may require manual editing or redump.");
            }

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            if(Tracks       == null ||
               Tracks.Count == 0)
            {
                Tracks = new List<Track>
                {
                    new()
                    {
                        BytesPerSector    = (int)_imageInfo.SectorSize,
                        EndSector         = _imageInfo.Sectors - 1,
                        File              = imageFilter.Filename,
                        FileType          = "BINARY",
                        Filter            = imageFilter,
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

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            Sessions = new List<Session>();

            for(var i = 1; i <= Tracks.Max(t => t.Session); i++)
                Sessions.Add(new Session
                {
                    Sequence    = (ushort)i,
                    StartTrack  = Tracks.Where(t => t.Session == i).Min(t => t.Sequence),
                    EndTrack    = Tracks.Where(t => t.Session == i).Max(t => t.Sequence),
                    StartSector = Tracks.Where(t => t.Session == i).Min(t => t.StartSector),
                    EndSector   = Tracks.Where(t => t.Session == i).Max(t => t.EndSector)
                });

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

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
                    Name     = $"Track {track.Sequence}",
                    Offset   = currentTrackOffset,
                    Start    = (ulong)track.Indexes[1],
                    Size     = (track.EndSector - (ulong)track.Indexes[1] + 1) * (ulong)track.BytesPerSector,
                    Length   = track.EndSector - (ulong)track.Indexes[1] + 1,
                    Scheme   = "Optical disc track"
                });

                currentTrackOffset += (track.EndSector - track.StartSector + 1) * (ulong)track.BytesPerSector;
            }

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            Track[] tracks = Tracks.ToArray();

            foreach(Track trk in tracks)
            {
                ErrorNumber errno = ReadSector(trk.StartSector, out byte[] sector);

                if(errno != ErrorNumber.NoError)
                    continue;

                trk.BytesPerSector = sector.Length;

                trk.RawBytesPerSector =
                    _sectorPrefix    != null && _sectorSuffix    != null ||
                    _sectorPrefixDdt != null && _sectorSuffixDdt != null ? 2352 : sector.Length;

                if(_sectorSubchannel == null)
                    continue;

                trk.SubchannelFile   = trk.File;
                trk.SubchannelFilter = trk.Filter;
                trk.SubchannelType   = TrackSubchannelType.Raw;
            }

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            Tracks = tracks.ToList();

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

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

        SetMetadataFromTags();

        if(_sectorSuffixDdt != null)
            EccInit();

        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NoError;

        if(_imageInfo.MediaType is MediaType.CD or MediaType.CDDA or MediaType.CDG or MediaType.CDEG or MediaType.CDI
                                or MediaType.CDROM or MediaType.CDROMXA or MediaType.CDPLUS or MediaType.CDMO
                                or MediaType.CDR or MediaType.CDRW or MediaType.CDMRW or MediaType.VCD or MediaType.SVCD
                                or MediaType.PCD or MediaType.DTSCD or MediaType.CDMIDI or MediaType.CDV
                                or MediaType.CDIREADY or MediaType.FMTOWNS or MediaType.PS1CD or MediaType.PS2CD
                                or MediaType.MEGACD or MediaType.SATURNCD or MediaType.GDROM or MediaType.GDR
                                or MediaType.MilCD or MediaType.SuperCDROM2 or MediaType.JaguarCD or MediaType.ThreeDO
                                or MediaType.PCFX or MediaType.NeoGeoCD or MediaType.CDTV or MediaType.CD32
                                or MediaType.Playdia or MediaType.Pippin or MediaType.VideoNow
                                or MediaType.VideoNowColor or MediaType.VideoNowXp or MediaType.CVD)
            return ErrorNumber.NoError;

        {
            foreach(Track track in Tracks)
            {
                track.Pregap = 0;
                track.Indexes?.Clear();
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        return _mediaTags.TryGetValue(tag, out buffer) ? ErrorNumber.NoError : ErrorNumber.NoData;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        ulong ddtEntry    = GetDdtEntry(sectorAddress);
        var   offsetMask  = (uint)((1 << _shift) - 1);
        ulong offset      = ddtEntry & offsetMask;
        ulong blockOffset = ddtEntry >> _shift;

        // Partially written image... as we can't know the real sector size just assume it's common :/
        if(ddtEntry == 0)
        {
            buffer = new byte[_imageInfo.SectorSize];

            return ErrorNumber.NoError;
        }

        // Check if block is cached
        if(_blockCache.TryGetValue(blockOffset, out byte[] block) &&
           _blockHeaderCache.TryGetValue(blockOffset, out BlockHeader blockHeader))
        {
            buffer = new byte[blockHeader.sectorSize];
            Array.Copy(block, (long)(offset * blockHeader.sectorSize), buffer, 0, blockHeader.sectorSize);

            return ErrorNumber.NoError;
        }

        // Read block header
        _imageStream.Position = (long)blockOffset;
        _structureBytes       = new byte[Marshal.SizeOf<BlockHeader>()];
        _imageStream.Read(_structureBytes, 0, _structureBytes.Length);
        blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(_structureBytes);

        // Decompress block
        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

        ulong decompressedLength;

        switch(blockHeader.compression)
        {
            case CompressionType.None:
                block = new byte[blockHeader.length];
                _imageStream.Read(block, 0, (int)blockHeader.length);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                break;
            case CompressionType.Lzma:
                var compressedBlock = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                var lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                _imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                _imageStream.Read(compressedBlock, 0, compressedBlock.Length);
                block              = new byte[blockHeader.length];
                decompressedLength = (ulong)LZMA.DecodeBuffer(compressedBlock, block, lzmaProperties);

                if(decompressedLength != blockHeader.length)
                {
                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               "Error decompressing block, should be {0} bytes but got {1} bytes.",
                                               blockHeader.length, decompressedLength);

                    return ErrorNumber.InOutError;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                break;
            case CompressionType.Flac:
                var flacBlock = new byte[blockHeader.cmpLength];
                _imageStream.Read(flacBlock, 0, flacBlock.Length);
                block              = new byte[blockHeader.length];
                decompressedLength = (ulong)FLAC.DecodeBuffer(flacBlock, block);

                if(decompressedLength != blockHeader.length)
                {
                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               "Error decompressing block, should be {0} bytes but got {1} bytes.",
                                               blockHeader.length, decompressedLength);

                    return ErrorNumber.InOutError;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                break;
            default: return ErrorNumber.NotSupported;
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

        buffer = new byte[blockHeader.sectorSize];
        Array.Copy(block, (long)(offset * blockHeader.sectorSize), buffer, 0, blockHeader.sectorSize);

        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track ? ErrorNumber.SectorNotFound
                   : ReadSector(trk.StartSector + sectorAddress, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track ? ErrorNumber.SectorNotFound
                   : ReadSectorTag(trk.StartSector + sectorAddress, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        uint   sectorOffset;
        uint   sectorSize;
        uint   sectorSkip;
        byte[] dataSource;
        buffer = null;

        if(_imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
        {
            Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

            if(trk is null)
                return ErrorNumber.SectorNotFound;

            if(trk.Sequence    == 0 &&
               trk.StartSector == 0 &&
               trk.EndSector   == 0)
                return ErrorNumber.SectorNotFound;

            if(trk.Type == TrackType.Data)
                return ErrorNumber.NotSupported;

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
                    if(!_trackFlags.TryGetValue((byte)sectorAddress, out byte flags))
                        return ErrorNumber.NoData;

                    buffer = new[]
                    {
                        flags
                    };

                    return ErrorNumber.NoError;
                case SectorTagType.CdTrackIsrc:
                    if(!_trackIsrcs.TryGetValue((byte)sectorAddress, out string isrc))
                        return ErrorNumber.NoData;

                    buffer = Encoding.UTF8.GetBytes(isrc);

                    return ErrorNumber.NoError;
                default: return ErrorNumber.NotSupported;
            }

            switch(trk.Type)
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

                        case SectorTagType.CdSectorSubHeader: return ErrorNumber.NotSupported;
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

                        default: return ErrorNumber.NotSupported;
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
                        case SectorTagType.CdSectorEdc: return ErrorNumber.NotSupported;
                        case SectorTagType.CdSectorSubchannel:
                        {
                            sectorOffset = 0;
                            sectorSize   = 96;
                            sectorSkip   = 0;
                            dataSource   = _sectorSubchannel;

                            break;
                        }

                        default: return ErrorNumber.NotSupported;
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

                        default: return ErrorNumber.NotSupported;
                    }

                    break;
                }

                case TrackType.Data:
                {
                    if(_imageInfo.MediaType == MediaType.DVDROM)
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
                            default: return ErrorNumber.NotSupported;
                        }
                    else
                        return ErrorNumber.NotSupported;

                    break;
                }

                default: return ErrorNumber.NotSupported;
            }
        }
        else
            return ErrorNumber.NoData;

        if(dataSource == null)
            return ErrorNumber.NotSupported;

        buffer = new byte[sectorSize * length];

        if(sectorOffset == 0 &&
           sectorSkip   == 0)
        {
            Array.Copy(dataSource, (long)(sectorAddress * sectorSize), buffer, 0, length * sectorSize);

            return ErrorNumber.NoError;
        }

        for(var i = 0; i < length; i++)
            Array.Copy(dataSource, (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)), buffer,
                       i * sectorSize, sectorSize);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk?.Sequence != track)
            return ErrorNumber.SectorNotFound;

        return trk.StartSector + sectorAddress + length > trk.EndSector + 1 ? ErrorNumber.OutOfRange
                   : ReadSectors(trk.StartSector + sectorAddress, length, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track
                   ? ErrorNumber.SectorNotFound
                   : trk.StartSector + sectorAddress + length > trk.EndSector + 1
                       ? ErrorNumber.OutOfRange
                       : ReadSectorsTag(trk.StartSector + sectorAddress, length, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        switch(_imageInfo.XmlMediaType)
        {
            case XmlMediaType.OpticalDisc:
                Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

                if(trk is null)
                    return ErrorNumber.SectorNotFound;

                if(trk.Sequence    == 0 &&
                   trk.StartSector == 0 &&
                   trk.EndSector   == 0)
                    return ErrorNumber.SectorNotFound;

                if((_sectorSuffix    == null || _sectorPrefix    == null) &&
                   (_sectorSuffixDdt == null || _sectorPrefixDdt == null))
                    return ReadSector(sectorAddress, out buffer);

                buffer = new byte[2352];
                ErrorNumber errno = ReadSector(sectorAddress, out byte[] data);

                if(errno != ErrorNumber.NoError)
                    return errno;

                switch(trk.Type)
                {
                    case TrackType.Audio:
                    case TrackType.Data:
                        buffer = data;

                        return ErrorNumber.NoError;
                    case TrackType.CdMode1:
                        Array.Copy(data, 0, buffer, 16, 2048);

                        if(_sectorPrefix != null)
                            Array.Copy(_sectorPrefix, (int)sectorAddress * 16, buffer, 0, 16);
                        else if(_sectorPrefixDdt != null)
                        {
                            if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                ReconstructPrefix(ref buffer, trk.Type, (long)sectorAddress);
                            else if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                    _sectorPrefixDdt[sectorAddress]                  == 0)
                            {
                                // Do nothing
                            }
                            else
                            {
                                uint prefixPosition = ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;

                                if(prefixPosition > _sectorPrefixMs.Length)
                                    return ErrorNumber.InvalidArgument;

                                _sectorPrefixMs.Position = prefixPosition;

                                _sectorPrefixMs.Read(buffer, 0, 16);
                            }
                        }
                        else
                            return ErrorNumber.InvalidArgument;

                        if(_sectorSuffix != null)
                            Array.Copy(_sectorSuffix, (int)sectorAddress * 288, buffer, 2064, 288);
                        else if(_sectorSuffixDdt != null)
                        {
                            if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                ReconstructEcc(ref buffer, trk.Type);
                            else if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                    _sectorSuffixDdt[sectorAddress]                  == 0)
                            {
                                // Do nothing
                            }
                            else
                            {
                                uint suffixPosition = ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;

                                if(suffixPosition > _sectorSuffixMs.Length)
                                    return ErrorNumber.InvalidArgument;

                                _sectorSuffixMs.Position = suffixPosition;

                                _sectorSuffixMs.Read(buffer, 2064, 288);
                            }
                        }
                        else
                            return ErrorNumber.InvalidArgument;

                        return ErrorNumber.NoError;
                    case TrackType.CdMode2Formless:
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                        if(_sectorPrefix != null)
                            Array.Copy(_sectorPrefix, (int)sectorAddress * 16, buffer, 0, 16);
                        else if(_sectorPrefixMs != null)
                        {
                            if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                ReconstructPrefix(ref buffer, trk.Type, (long)sectorAddress);
                            else if((_sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                    _sectorPrefixDdt[sectorAddress]                  == 0)
                            {
                                // Do nothing
                            }
                            else
                            {
                                uint prefixPosition = ((_sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;

                                if(prefixPosition > _sectorPrefixMs.Length)
                                    return ErrorNumber.InvalidArgument;

                                _sectorPrefixMs.Position = prefixPosition;

                                _sectorPrefixMs.Read(buffer, 0, 16);
                            }
                        }
                        else
                            return ErrorNumber.InvalidArgument;

                        if(_mode2Subheaders != null &&
                           _sectorSuffixDdt != null)
                        {
                            Array.Copy(_mode2Subheaders, (int)sectorAddress * 8, buffer, 16, 8);

                            switch(_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK)
                            {
                                case (uint)CdFixFlags.Mode2Form1Ok:
                                    Array.Copy(data, 0, buffer, 24, 2048);
                                    ReconstructEcc(ref buffer, TrackType.CdMode2Form1);

                                    break;
                                case (uint)CdFixFlags.Mode2Form2Ok:
                                case (uint)CdFixFlags.Mode2Form2NoCrc:
                                {
                                    Array.Copy(data, 0, buffer, 24, 2324);

                                    if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                       (uint)CdFixFlags.Mode2Form2Ok)
                                        ReconstructEcc(ref buffer, TrackType.CdMode2Form2);

                                    break;
                                }
                                default:
                                {
                                    if((_sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                       _sectorSuffixDdt[sectorAddress]                  == 0)
                                    {
                                        // Do nothing
                                    }
                                    else // Mode 2 where EDC failed
                                    {
                                        // Incorrectly written images
                                        if(data.Length == 2328)
                                            Array.Copy(data, 0, buffer, 24, 2328);
                                        else
                                        {
                                            bool form2 = (buffer[18] & 0x20) == 0x20 || (buffer[22] & 0x20) == 0x20;

                                            uint suffixPosition =
                                                ((_sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;

                                            if(suffixPosition > _sectorSuffixMs.Length)
                                                return ErrorNumber.InvalidArgument;

                                            _sectorSuffixMs.Position = suffixPosition;

                                            _sectorSuffixMs.Read(buffer, form2 ? 2348 : 2072, form2 ? 4 : 280);
                                            Array.Copy(data, 0, buffer, 24, form2 ? 2324 : 2048);
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                        else if(_mode2Subheaders != null)
                        {
                            Array.Copy(_mode2Subheaders, (int)sectorAddress * 8, buffer, 16, 8);
                            Array.Copy(data, 0, buffer, 24, 2328);
                        }
                        else
                            Array.Copy(data, 0, buffer, 16, 2336);

                        return ErrorNumber.NoError;
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
                    case MediaType.PriamDataTower: return ReadSectorsLong(sectorAddress, 1, out buffer);
                }

                break;
        }

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track ? ErrorNumber.SectorNotFound
                   : ReadSectorLong(trk.StartSector + sectorAddress, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        byte[]      data;
        ErrorNumber errno;
        buffer = null;

        switch(_imageInfo.XmlMediaType)
        {
            case XmlMediaType.OpticalDisc:
                Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

                if(trk is null)
                    return ErrorNumber.SectorNotFound;

                if(trk.Sequence    == 0 &&
                   trk.StartSector == 0 &&
                   trk.EndSector   == 0)
                    return ErrorNumber.SectorNotFound;

                if(sectorAddress + length > trk.EndSector + 1)
                    return ErrorNumber.OutOfRange;

                switch(trk.Type)
                {
                    // These types only contain user data
                    case TrackType.Audio:
                    case TrackType.Data: return ReadSectors(sectorAddress, length, out buffer);

                    // Join prefix (sync, header) with user data with suffix (edc, ecc p, ecc q)
                    case TrackType.CdMode1:
                        if(_sectorPrefix != null &&
                           _sectorSuffix != null)
                        {
                            buffer = new byte[2352 * length];
                            errno  = ReadSectors(sectorAddress, length, out data);

                            if(errno != ErrorNumber.NoError)
                                return errno;

                            for(uint i = 0; i < length; i++)
                            {
                                Array.Copy(_sectorPrefix, (int)((sectorAddress + i) * 16), buffer, (int)(i * 2352), 16);

                                Array.Copy(data, (int)(i * 2048), buffer, (int)(i * 2352) + 16, 2048);

                                Array.Copy(_sectorSuffix, (int)((sectorAddress + i) * 288), buffer,
                                           (int)(i * 2352) + 2064, 288);
                            }

                            return ErrorNumber.NoError;
                        }

                        if(_sectorPrefixDdt != null &&
                           _sectorSuffixDdt != null)
                        {
                            buffer = new byte[2352 * length];

                            for(uint i = 0; i < length; i++)
                            {
                                errno = ReadSectorLong(sectorAddress + i, out byte[] temp);

                                if(errno != ErrorNumber.NoError)
                                    return errno;

                                Array.Copy(temp, 0, buffer, 2352 * i, 2352);
                            }

                            return ErrorNumber.NoError;
                        }

                        return ReadSectors(sectorAddress, length, out buffer);

                    // Join prefix (sync, header) with user data
                    case TrackType.CdMode2Formless:
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                        if(_sectorPrefix != null &&
                           _sectorSuffix != null)
                        {
                            buffer = new byte[2352 * length];
                            errno  = ReadSectors(sectorAddress, length, out data);

                            if(errno != ErrorNumber.NoError)
                                return errno;

                            for(uint i = 0; i < length; i++)
                            {
                                Array.Copy(_sectorPrefix, (int)((sectorAddress + i) * 16), buffer, (int)(i * 2352), 16);

                                Array.Copy(data, (int)(i * 2336), buffer, (int)(i * 2352) + 16, 2336);
                            }

                            return ErrorNumber.NoError;
                        }

                        if(_sectorPrefixDdt != null &&
                           _sectorSuffixDdt != null)
                        {
                            buffer = new byte[2352 * length];

                            for(uint i = 0; i < length; i++)
                            {
                                errno = ReadSectorLong(sectorAddress + i, out byte[] temp);

                                if(errno != ErrorNumber.NoError)
                                    return errno;

                                Array.Copy(temp, 0, buffer, 2352 * i, 2352);
                            }

                            return ErrorNumber.NoError;
                        }

                        return ReadSectors(sectorAddress, length, out buffer);
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
                            return ReadSector(sectorAddress, out buffer);

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
                        errno = ReadSectors(sectorAddress, length, out data);

                        if(errno != ErrorNumber.NoError)
                            return errno;

                        buffer = new byte[(sectorSize + 512) * length];

                        for(uint i = 0; i < length; i++)
                        {
                            Array.Copy(_sectorSubchannel, (int)((sectorAddress + i) * tagSize), buffer,
                                       (int)(i * sectorSize + 512), tagSize);

                            Array.Copy(data, (int)((sectorAddress + i) * 512), buffer, (int)(i * 512), 512);
                        }

                        return ErrorNumber.NoError;
                }

                break;
        }

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track
                   ? ErrorNumber.SectorNotFound
                   : trk.StartSector + sectorAddress + length > trk.EndSector + 1
                       ? ErrorNumber.OutOfRange
                       : ReadSectorsLong(trk.StartSector + sectorAddress, length, out buffer);
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => Tracks.Where(t => t.Sequence == session.Sequence).ToList();

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => Tracks.Where(t => t.Sequence == session).ToList();
}