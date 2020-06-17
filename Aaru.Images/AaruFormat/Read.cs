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
// Copyright © 2011-2020 Natalia Portillo
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
using CUETools.Codecs;
using CUETools.Codecs.Flake;
using Schemas;
using SharpCompress.Compressors.LZMA;
using Marshal = Aaru.Helpers.Marshal;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public partial class AaruFormat
    {
        public bool Open(IFilter imageFilter)
        {
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            imageStream = imageFilter.GetDataForkStream();
            imageStream.Seek(0, SeekOrigin.Begin);

            if(imageStream.Length < Marshal.SizeOf<AaruHeader>())
                return false;

            structureBytes = new byte[Marshal.SizeOf<AaruHeader>()];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            header = Marshal.ByteArrayToStructureLittleEndian<AaruHeader>(structureBytes);

            if(header.imageMajorVersion > AARUFMT_VERSION)
                throw new FeatureUnsupportedImageException($"Image version {header.imageMajorVersion} not recognized.");

            imageInfo.Application        = header.application;
            imageInfo.ApplicationVersion = $"{header.applicationMajorVersion}.{header.applicationMinorVersion}";
            imageInfo.Version            = $"{header.imageMajorVersion}.{header.imageMinorVersion}";
            imageInfo.MediaType          = header.mediaType;

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Read the index header
            imageStream.Position = (long)header.indexOffset;
            structureBytes       = new byte[Marshal.SizeOf<IndexHeader>()];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            IndexHeader idxHeader = Marshal.SpanToStructureLittleEndian<IndexHeader>(structureBytes);

            if(idxHeader.identifier != BlockType.Index &&
               idxHeader.identifier != BlockType.Index2)
                throw new FeatureUnsupportedImageException("Index not found!");

            if(idxHeader.identifier == BlockType.Index2)
            {
                imageStream.Position = (long)header.indexOffset;
                structureBytes       = new byte[Marshal.SizeOf<IndexHeader2>()];
                imageStream.Read(structureBytes, 0, structureBytes.Length);
                IndexHeader2 idxHeader2 = Marshal.SpanToStructureLittleEndian<IndexHeader2>(structureBytes);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries",
                                           header.indexOffset, idxHeader2.entries);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                // Fill in-memory index
                index = new List<IndexEntry>();

                for(ulong i = 0; i < idxHeader2.entries; i++)
                {
                    structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                    imageStream.Read(structureBytes, 0, structureBytes.Length);
                    IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(structureBytes);

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               "Block type {0} with data type {1} is indexed to be at {2}",
                                               entry.blockType, entry.dataType, entry.offset);

                    index.Add(entry);
                }
            }
            else
            {
                AaruConsole.DebugWriteLine("Aaru Format plugin", "Index at {0} contains {1} entries",
                                           header.indexOffset, idxHeader.entries);

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                // Fill in-memory index
                index = new List<IndexEntry>();

                for(ushort i = 0; i < idxHeader.entries; i++)
                {
                    structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
                    imageStream.Read(structureBytes, 0, structureBytes.Length);
                    IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(structureBytes);

                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                               "Block type {0} with data type {1} is indexed to be at {2}",
                                               entry.blockType, entry.dataType, entry.offset);

                    index.Add(entry);
                }
            }

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            imageInfo.ImageSize = 0;

            bool foundUserDataDdt = false;
            mediaTags = new Dictionary<MediaTagType, byte[]>();

            foreach(IndexEntry entry in index)
            {
                imageStream.Position = (long)entry.offset;

                switch(entry.blockType)
                {
                    case BlockType.DataBlock:
                        // NOP block, skip
                        if(entry.dataType == DataType.NoData)
                            break;

                        imageStream.Position = (long)entry.offset;

                        structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        BlockHeader blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(structureBytes);
                        imageInfo.ImageSize += blockHeader.cmpLength;

                        // Unused, skip
                        if(entry.dataType == DataType.UserData)
                        {
                            if(blockHeader.sectorSize > imageInfo.SectorSize)
                                imageInfo.SectorSize = blockHeader.sectorSize;

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
                            imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                            imageStream.Read(compressedTag, 0, compressedTag.Length);
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
                            imageStream.Read(data, 0, (int)blockHeader.length);

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

                        if(BitConverter.ToUInt64(blockCrc, 0) != blockHeader.crc64)
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
                                    sectorPrefixMs = new NonClosableStream();
                                    sectorPrefixMs.Write(data, 0, data.Length);
                                }
                                else
                                    sectorPrefix = data;

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.CdSectorSuffix:
                            case DataType.CdSectorSuffixCorrected:
                                if(entry.dataType == DataType.CdSectorSuffixCorrected)
                                {
                                    sectorSuffixMs = new NonClosableStream();
                                    sectorSuffixMs.Write(data, 0, data.Length);
                                }
                                else
                                    sectorSuffix = data;

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.CdSectorSubchannel:
                                sectorSubchannel = data;

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.AppleProfileTag:
                            case DataType.AppleSonyTag:
                            case DataType.PriamDataTowerTag:
                                sectorSubchannel = data;

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                break;
                            case DataType.CompactDiscMode2Subheader:
                                mode2Subheaders = data;

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                break;
                            default:
                                MediaTagType mediaTagType = GetMediaTagTypeForDataType(blockHeader.type);

                                if(mediaTags.ContainsKey(mediaTagType))
                                {
                                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                               "Media tag type {0} duplicated, removing previous entry...",
                                                               mediaTagType);

                                    mediaTags.Remove(mediaTagType);
                                }

                                mediaTags.Add(mediaTagType, data);

                                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                           GC.GetTotalMemory(false));

                                break;
                        }

                        break;
                    case BlockType.DeDuplicationTable:
                        structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        DdtHeader ddtHeader = Marshal.SpanToStructureLittleEndian<DdtHeader>(structureBytes);
                        imageInfo.ImageSize += ddtHeader.cmpLength;

                        if(ddtHeader.identifier != BlockType.DeDuplicationTable)
                            break;

                        if(entry.dataType == DataType.UserData)
                        {
                            imageInfo.Sectors = ddtHeader.entries;
                            shift             = ddtHeader.shift;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            // Check for DDT compression
                            switch(ddtHeader.compression)
                            {
                                case CompressionType.Lzma:
                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");
                                    DateTime ddtStart       = DateTime.UtcNow;
                                    byte[]   compressedDdt  = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                    byte[]   lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                    imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                    imageStream.Read(compressedDdt, 0, compressedDdt.Length);
                                    var    compressedDdtMs = new MemoryStream(compressedDdt);
                                    var    lzmaDdt         = new LzmaStream(lzmaProperties, compressedDdtMs);
                                    byte[] decompressedDdt = new byte[ddtHeader.length];
                                    lzmaDdt.Read(decompressedDdt, 0, (int)ddtHeader.length);
                                    lzmaDdt.Close();
                                    compressedDdtMs.Close();
                                    userDataDdt = MemoryMarshal.Cast<byte, ulong>(decompressedDdt).ToArray();
                                    DateTime ddtEnd = DateTime.UtcNow;
                                    inMemoryDdt = true;

                                    AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                               "Took {0} seconds to decompress DDT",
                                                               (ddtEnd - ddtStart).TotalSeconds);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                case CompressionType.None:
                                    inMemoryDdt          = false;
                                    outMemoryDdtPosition = (long)entry.offset;

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                default:
                                    throw new
                                        ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");
                            }

                            foundUserDataDdt = true;
                        }
                        else if(entry.dataType == DataType.CdSectorPrefixCorrected ||
                                entry.dataType == DataType.CdSectorSuffixCorrected)
                        {
                            uint[] cdDdt           = new uint[ddtHeader.entries];
                            byte[] decompressedDdt = new byte[ddtHeader.length];

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));

                            // Check for DDT compression
                            switch(ddtHeader.compression)
                            {
                                case CompressionType.Lzma:
                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Decompressing DDT...");
                                    DateTime ddtStart       = DateTime.UtcNow;
                                    byte[]   compressedDdt  = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                    byte[]   lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                    imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                    imageStream.Read(compressedDdt, 0, compressedDdt.Length);
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
                                    imageStream.Read(decompressedDdt, 0, decompressedDdt.Length);

                                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                               GC.GetTotalMemory(false));

                                    break;
                                default:
                                    throw new
                                        ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");
                            }

                            cdDdt = MemoryMarshal.Cast<byte, uint>(decompressedDdt).ToArray();

                            if(entry.dataType == DataType.CdSectorPrefixCorrected)
                                sectorPrefixDdt = cdDdt;
                            else if(entry.dataType == DataType.CdSectorSuffixCorrected)
                                sectorSuffixDdt = cdDdt;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));
                        }

                        break;

                    // Logical geometry block. It doesn't have a CRC coz, well, it's not so important
                    case BlockType.GeometryBlock:
                        structureBytes = new byte[Marshal.SizeOf<GeometryBlock>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        geometryBlock = Marshal.SpanToStructureLittleEndian<GeometryBlock>(structureBytes);

                        if(geometryBlock.identifier == BlockType.GeometryBlock)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Geometry set to {0} cylinders {1} heads {2} sectors per track",
                                                       geometryBlock.cylinders, geometryBlock.heads,
                                                       geometryBlock.sectorsPerTrack);

                            imageInfo.Cylinders       = geometryBlock.cylinders;
                            imageInfo.Heads           = geometryBlock.heads;
                            imageInfo.SectorsPerTrack = geometryBlock.sectorsPerTrack;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                       GC.GetTotalMemory(false));
                        }

                        break;

                    // Metadata block
                    case BlockType.MetadataBlock:
                        structureBytes = new byte[Marshal.SizeOf<MetadataBlock>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);

                        MetadataBlock metadataBlock =
                            Marshal.SpanToStructureLittleEndian<MetadataBlock>(structureBytes);

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
                        imageStream.Position = (long)entry.offset;
                        imageStream.Read(metadata, 0, metadata.Length);

                        if(metadataBlock.mediaSequence     > 0 &&
                           metadataBlock.lastMediaSequence > 0)
                        {
                            imageInfo.MediaSequence     = metadataBlock.mediaSequence;
                            imageInfo.LastMediaSequence = metadataBlock.lastMediaSequence;

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media sequence as {0} of {1}",
                                                       imageInfo.MediaSequence, imageInfo.LastMediaSequence);
                        }

                        if(metadataBlock.creatorLength                               > 0 &&
                           metadataBlock.creatorLength + metadataBlock.creatorOffset <= metadata.Length)
                        {
                            imageInfo.Creator = Encoding.Unicode.GetString(metadata, (int)metadataBlock.creatorOffset,
                                                                           (int)(metadataBlock.creatorLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting creator: {0}", imageInfo.Creator);
                        }

                        if(metadataBlock.commentsOffset                                > 0 &&
                           metadataBlock.commentsLength + metadataBlock.commentsOffset <= metadata.Length)
                        {
                            imageInfo.Comments =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.commentsOffset,
                                                           (int)(metadataBlock.commentsLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting comments: {0}",
                                                       imageInfo.Comments);
                        }

                        if(metadataBlock.mediaTitleOffset                                  > 0 &&
                           metadataBlock.mediaTitleLength + metadataBlock.mediaTitleOffset <= metadata.Length)
                        {
                            imageInfo.MediaTitle =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaTitleOffset,
                                                           (int)(metadataBlock.mediaTitleLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media title: {0}",
                                                       imageInfo.MediaTitle);
                        }

                        if(metadataBlock.mediaManufacturerOffset > 0 &&
                           metadataBlock.mediaManufacturerLength + metadataBlock.mediaManufacturerOffset <=
                           metadata.Length)
                        {
                            imageInfo.MediaManufacturer =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaManufacturerOffset,
                                                           (int)(metadataBlock.mediaManufacturerLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media manufacturer: {0}",
                                                       imageInfo.MediaManufacturer);
                        }

                        if(metadataBlock.mediaModelOffset                                  > 0 &&
                           metadataBlock.mediaModelLength + metadataBlock.mediaModelOffset <= metadata.Length)
                        {
                            imageInfo.MediaModel =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaModelOffset,
                                                           (int)(metadataBlock.mediaModelLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media model: {0}",
                                                       imageInfo.MediaModel);
                        }

                        if(metadataBlock.mediaSerialNumberOffset > 0 &&
                           metadataBlock.mediaSerialNumberLength + metadataBlock.mediaSerialNumberOffset <=
                           metadata.Length)
                        {
                            imageInfo.MediaSerialNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaSerialNumberOffset,
                                                           (int)(metadataBlock.mediaSerialNumberLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media serial number: {0}",
                                                       imageInfo.MediaSerialNumber);
                        }

                        if(metadataBlock.mediaBarcodeOffset                                    > 0 &&
                           metadataBlock.mediaBarcodeLength + metadataBlock.mediaBarcodeOffset <= metadata.Length)
                        {
                            imageInfo.MediaBarcode =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaBarcodeOffset,
                                                           (int)(metadataBlock.mediaBarcodeLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media barcode: {0}",
                                                       imageInfo.MediaBarcode);
                        }

                        if(metadataBlock.mediaPartNumberOffset                                       > 0 &&
                           metadataBlock.mediaPartNumberLength + metadataBlock.mediaPartNumberOffset <= metadata.Length)
                        {
                            imageInfo.MediaPartNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaPartNumberOffset,
                                                           (int)(metadataBlock.mediaPartNumberLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting media part number: {0}",
                                                       imageInfo.MediaPartNumber);
                        }

                        if(metadataBlock.driveManufacturerOffset > 0 &&
                           metadataBlock.driveManufacturerLength + metadataBlock.driveManufacturerOffset <=
                           metadata.Length)
                        {
                            imageInfo.DriveManufacturer =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveManufacturerOffset,
                                                           (int)(metadataBlock.driveManufacturerLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive manufacturer: {0}",
                                                       imageInfo.DriveManufacturer);
                        }

                        if(metadataBlock.driveModelOffset                                  > 0 &&
                           metadataBlock.driveModelLength + metadataBlock.driveModelOffset <= metadata.Length)
                        {
                            imageInfo.DriveModel =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveModelOffset,
                                                           (int)(metadataBlock.driveModelLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive model: {0}",
                                                       imageInfo.DriveModel);
                        }

                        if(metadataBlock.driveSerialNumberOffset > 0 &&
                           metadataBlock.driveSerialNumberLength + metadataBlock.driveSerialNumberOffset <=
                           metadata.Length)
                        {
                            imageInfo.DriveSerialNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveSerialNumberOffset,
                                                           (int)(metadataBlock.driveSerialNumberLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive serial number: {0}",
                                                       imageInfo.DriveSerialNumber);
                        }

                        if(metadataBlock.driveFirmwareRevisionOffset > 0 &&
                           metadataBlock.driveFirmwareRevisionLength + metadataBlock.driveFirmwareRevisionOffset <=
                           metadata.Length)
                        {
                            imageInfo.DriveFirmwareRevision =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveFirmwareRevisionOffset,
                                                           (int)(metadataBlock.driveFirmwareRevisionLength - 2));

                            AaruConsole.DebugWriteLine("Aaru Format plugin", "Setting drive firmware revision: {0}",
                                                       imageInfo.DriveFirmwareRevision);
                        }

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

                        break;

                    // Optical disc tracks block
                    case BlockType.TracksBlock:
                        structureBytes = new byte[Marshal.SizeOf<TracksHeader>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        TracksHeader tracksHeader = Marshal.SpanToStructureLittleEndian<TracksHeader>(structureBytes);

                        if(tracksHeader.identifier != BlockType.TracksBlock)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Incorrect identifier for tracks block at position {0}",
                                                       entry.offset);

                            break;
                        }

                        structureBytes = new byte[Marshal.SizeOf<TrackEntry>() * tracksHeader.entries];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        Crc64Context.Data(structureBytes, out byte[] trksCrc);

                        if(BitConverter.ToUInt64(trksCrc, 0) != tracksHeader.crc64)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                       BitConverter.ToUInt64(trksCrc, 0), tracksHeader.crc64);

                            break;
                        }

                        imageStream.Position -= structureBytes.Length;

                        Tracks     = new List<Track>();
                        trackFlags = new Dictionary<byte, byte>();
                        trackIsrcs = new Dictionary<byte, string>();

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found {0} tracks at position {0}",
                                                   tracksHeader.entries, entry.offset);

                        for(ushort i = 0; i < tracksHeader.entries; i++)
                        {
                            structureBytes = new byte[Marshal.SizeOf<TrackEntry>()];
                            imageStream.Read(structureBytes, 0, structureBytes.Length);

                            TrackEntry trackEntry =
                                Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(structureBytes);

                            Tracks.Add(new Track
                            {
                                TrackSequence    = trackEntry.sequence, TrackType           = trackEntry.type,
                                TrackStartSector = (ulong)trackEntry.start, TrackEndSector  = (ulong)trackEntry.end,
                                TrackPregap      = (ulong)trackEntry.pregap, TrackSession   = trackEntry.session,
                                TrackFile        = imageFilter.GetFilename(), TrackFileType = "BINARY",
                                TrackFilter      = imageFilter
                            });

                            trackFlags.Add(trackEntry.sequence, trackEntry.flags);
                            trackIsrcs.Add(trackEntry.sequence, trackEntry.isrc);
                        }

                        imageInfo.HasPartitions = true;
                        imageInfo.HasSessions   = true;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

                        break;

                    // CICM XML metadata block
                    case BlockType.CicmBlock:
                        structureBytes = new byte[Marshal.SizeOf<CicmMetadataBlock>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);

                        CicmMetadataBlock cicmBlock =
                            Marshal.SpanToStructureLittleEndian<CicmMetadataBlock>(structureBytes);

                        if(cicmBlock.identifier != BlockType.CicmBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                   "Found CICM XML metadata block at position {0}", entry.offset);

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

                        byte[] cicmBytes = new byte[cicmBlock.length];
                        imageStream.Read(cicmBytes, 0, cicmBytes.Length);
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
                        structureBytes = new byte[Marshal.SizeOf<DumpHardwareHeader>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);

                        DumpHardwareHeader dumpBlock =
                            Marshal.SpanToStructureLittleEndian<DumpHardwareHeader>(structureBytes);

                        if(dumpBlock.identifier != BlockType.DumpHardwareBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found dump hardware block at position {0}",
                                                   entry.offset);

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                                   GC.GetTotalMemory(false));

                        structureBytes = new byte[dumpBlock.length];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        Crc64Context.Data(structureBytes, out byte[] dumpCrc);

                        if(BitConverter.ToUInt64(dumpCrc, 0) != dumpBlock.crc64)
                        {
                            AaruConsole.DebugWriteLine("Aaru Format plugin",
                                                       "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                       BitConverter.ToUInt64(dumpCrc, 0), dumpBlock.crc64);

                            break;
                        }

                        imageStream.Position -= structureBytes.Length;

                        DumpHardware = new List<DumpHardwareType>();

                        for(ushort i = 0; i < dumpBlock.entries; i++)
                        {
                            structureBytes = new byte[Marshal.SizeOf<DumpHardwareEntry>()];
                            imageStream.Read(structureBytes, 0, structureBytes.Length);

                            DumpHardwareEntry dumpEntry =
                                Marshal.SpanToStructureLittleEndian<DumpHardwareEntry>(structureBytes);

                            var dump = new DumpHardwareType
                            {
                                Software = new SoftwareType(), Extents = new ExtentType[dumpEntry.extents]
                            };

                            byte[] tmp;

                            if(dumpEntry.manufacturerLength > 0)
                            {
                                tmp = new byte[dumpEntry.manufacturerLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position += 1;
                                dump.Manufacturer    =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.modelLength > 0)
                            {
                                tmp = new byte[dumpEntry.modelLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position += 1;
                                dump.Model           =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.revisionLength > 0)
                            {
                                tmp = new byte[dumpEntry.revisionLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position += 1;
                                dump.Revision        =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.firmwareLength > 0)
                            {
                                tmp = new byte[dumpEntry.firmwareLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position += 1;
                                dump.Firmware        =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.serialLength > 0)
                            {
                                tmp = new byte[dumpEntry.serialLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position += 1;
                                dump.Serial          =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.softwareNameLength > 0)
                            {
                                tmp = new byte[dumpEntry.softwareNameLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position += 1;
                                dump.Software.Name   =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.softwareVersionLength > 0)
                            {
                                tmp = new byte[dumpEntry.softwareVersionLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position  += 1;
                                dump.Software.Version =  Encoding.UTF8.GetString(tmp);
                            }

                            if(dumpEntry.softwareOperatingSystemLength > 0)
                            {
                                tmp = new byte[dumpEntry.softwareOperatingSystemLength - 1];
                                imageStream.Read(tmp, 0, tmp.Length);
                                imageStream.Position          += 1;
                                dump.Software.OperatingSystem =  Encoding.UTF8.GetString(tmp);
                            }

                            tmp = new byte[16];

                            for(uint j = 0; j < dumpEntry.extents; j++)
                            {
                                imageStream.Read(tmp, 0, tmp.Length);

                                dump.Extents[j] = new ExtentType
                                {
                                    Start = BitConverter.ToUInt64(tmp, 0), End = BitConverter.ToUInt64(tmp, 8)
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
                        structureBytes = new byte[Marshal.SizeOf<TapePartitionHeader>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);

                        TapePartitionHeader partitionHeader =
                            Marshal.SpanToStructureLittleEndian<TapePartitionHeader>(structureBytes);

                        if(partitionHeader.identifier != BlockType.TapePartitionBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found tape partition block at position {0}",
                                                   entry.offset);

                        byte[] tapePartitionBytes = new byte[partitionHeader.length];
                        imageStream.Read(tapePartitionBytes, 0, tapePartitionBytes.Length);

                        Span<TapePartitionEntry> tapePartitions =
                            MemoryMarshal.Cast<byte, TapePartitionEntry>(tapePartitionBytes);

                        TapePartitions = new List<TapePartition>();

                        foreach(TapePartitionEntry tapePartition in tapePartitions)
                            TapePartitions.Add(new TapePartition
                            {
                                FirstBlock = tapePartition.FirstBlock, LastBlock = tapePartition.LastBlock,
                                Number     = tapePartition.Number
                            });

                        IsTape = true;

                        break;

                    // Tape file block
                    case BlockType.TapeFileBlock:
                        structureBytes = new byte[Marshal.SizeOf<TapeFileHeader>()];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        TapeFileHeader fileHeader = Marshal.SpanToStructureLittleEndian<TapeFileHeader>(structureBytes);

                        if(fileHeader.identifier != BlockType.TapeFileBlock)
                            break;

                        AaruConsole.DebugWriteLine("Aaru Format plugin", "Found tape file block at position {0}",
                                                   entry.offset);

                        byte[] tapeFileBytes = new byte[fileHeader.length];
                        imageStream.Read(tapeFileBytes, 0, tapeFileBytes.Length);
                        Span<TapeFileEntry> tapeFiles = MemoryMarshal.Cast<byte, TapeFileEntry>(tapeFileBytes);
                        Files = new List<TapeFile>();

                        foreach(TapeFileEntry file in tapeFiles)
                            Files.Add(new TapeFile
                            {
                                FirstBlock = file.FirstBlock, LastBlock = file.LastBlock, Partition = file.Partition,
                                File       = file.File
                            });

                        IsTape = true;

                        break;
                }
            }

            if(!foundUserDataDdt)
                throw new ImageNotSupportedException("Could not find user data deduplication table.");

            imageInfo.CreationTime = DateTime.FromFileTimeUtc(header.creationTime);
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Image created on {0}", imageInfo.CreationTime);
            imageInfo.LastModificationTime = DateTime.FromFileTimeUtc(header.lastWrittenTime);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Image last written on {0}",
                                       imageInfo.LastModificationTime);

            imageInfo.XmlMediaType = GetXmlMediaType(header.mediaType);

            if(geometryBlock.identifier != BlockType.GeometryBlock &&
               imageInfo.XmlMediaType   == XmlMediaType.BlockMedia)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;
            }

            imageInfo.ReadableMediaTags.AddRange(mediaTags.Keys);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Initialize caches
            blockCache       = new Dictionary<ulong, byte[]>();
            blockHeaderCache = new Dictionary<ulong, BlockHeader>();
            currentCacheSize = 0;

            if(!inMemoryDdt)
                ddtEntryCache = new Dictionary<ulong, ulong>();

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            // Initialize tracks, sessions and partitions
            if(imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                if(Tracks       == null ||
                   Tracks.Count == 0)
                {
                    Tracks = new List<Track>
                    {
                        new Track
                        {
                            TrackBytesPerSector = (int)imageInfo.SectorSize, TrackEndSector = imageInfo.Sectors - 1,
                            TrackFile = imageFilter.GetFilename(), TrackFileType = "BINARY", TrackFilter = imageFilter,
                            TrackRawBytesPerSector = (int)imageInfo.SectorSize, TrackSession = 1, TrackSequence = 1,
                            TrackType = TrackType.Data
                        }
                    };

                    trackFlags = new Dictionary<byte, byte>
                    {
                        {
                            1, (byte)CdFlags.DataTrack
                        }
                    };

                    trackIsrcs = new Dictionary<byte, string>();
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

                ulong currentTrackOffset = 0;
                Partitions = new List<Partition>();

                foreach(Track track in Tracks.OrderBy(t => t.TrackStartSector))
                {
                    Partitions.Add(new Partition
                    {
                        Sequence = track.TrackSequence, Type = track.TrackType.ToString(),
                        Name = $"Track {track.TrackSequence}", Offset = currentTrackOffset,
                        Start = track.TrackStartSector,
                        Size = ((track.TrackEndSector - track.TrackStartSector) + 1) * (ulong)track.TrackBytesPerSector,
                        Length = (track.TrackEndSector - track.TrackStartSector) + 1, Scheme = "Optical disc track"
                    });

                    currentTrackOffset += ((track.TrackEndSector - track.TrackStartSector) + 1) *
                                          (ulong)track.TrackBytesPerSector;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                Track[] tracks = Tracks.ToArray();

                for(int i = 0; i < tracks.Length; i++)
                {
                    byte[] sector = ReadSector(tracks[i].TrackStartSector);
                    tracks[i].TrackBytesPerSector = sector.Length;

                    tracks[i].TrackRawBytesPerSector =
                        (sectorPrefix    != null && sectorSuffix    != null) ||
                        (sectorPrefixDdt != null && sectorSuffixDdt != null) ? 2352 : sector.Length;

                    if(sectorSubchannel == null)
                        continue;

                    tracks[i].TrackSubchannelFile   = tracks[i].TrackFile;
                    tracks[i].TrackSubchannelFilter = tracks[i].TrackFilter;
                    tracks[i].TrackSubchannelType   = TrackSubchannelType.Raw;
                }

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));

                Tracks = tracks.ToList();

                AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                           GC.GetTotalMemory(false));
            }
            else
            {
                Tracks     = null;
                Sessions   = null;
                Partitions = null;
            }

            SetMetadataFromTags();

            if(sectorSuffixDdt != null)
                EccInit();

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(mediaTags.TryGetValue(tag, out byte[] data))
                return data;

            throw new FeatureNotPresentImageException("Requested tag is not present in image");
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            ulong ddtEntry    = GetDdtEntry(sectorAddress);
            uint  offsetMask  = (uint)((1 << shift) - 1);
            ulong offset      = ddtEntry & offsetMask;
            ulong blockOffset = ddtEntry >> shift;

            // Partially written image... as we can't know the real sector size just assume it's common :/
            if(ddtEntry == 0)
                return new byte[imageInfo.SectorSize];

            byte[] sector;

            // Check if block is cached
            if(blockCache.TryGetValue(blockOffset, out byte[] block) &&
               blockHeaderCache.TryGetValue(blockOffset, out BlockHeader blockHeader))
            {
                sector = new byte[blockHeader.sectorSize];
                Array.Copy(block, (long)(offset * blockHeader.sectorSize), sector, 0, blockHeader.sectorSize);

                return sector;
            }

            // Read block header
            imageStream.Position = (long)blockOffset;
            structureBytes       = new byte[Marshal.SizeOf<BlockHeader>()];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(structureBytes);

            // Decompress block
            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            switch(blockHeader.compression)
            {
                case CompressionType.None:
                    block = new byte[blockHeader.length];
                    imageStream.Read(block, 0, (int)blockHeader.length);

                    AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes",
                                               GC.GetTotalMemory(false));

                    break;
                case CompressionType.Lzma:
                    byte[] compressedBlock = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                    byte[] lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                    imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                    imageStream.Read(compressedBlock, 0, compressedBlock.Length);
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
                    imageStream.Read(flacBlock, 0, flacBlock.Length);
                    var flacMs      = new MemoryStream(flacBlock);
                    var flakeReader = new AudioDecoder(new DecoderSettings(), "", flacMs);
                    block = new byte[blockHeader.length];
                    int samples     = (int)((block.Length / blockHeader.sectorSize) * 588);
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
            if(currentCacheSize + blockHeader.length >= MAX_CACHE_SIZE)
            {
                currentCacheSize = 0;
                blockHeaderCache = new Dictionary<ulong, BlockHeader>();
                blockCache       = new Dictionary<ulong, byte[]>();
            }

            // Add block to cache
            currentCacheSize += blockHeader.length;
            blockHeaderCache.Add(blockOffset, blockHeader);
            blockCache.Add(blockOffset, block);

            sector = new byte[blockHeader.sectorSize];
            Array.Copy(block, (long)(offset * blockHeader.sectorSize), sector, 0, blockHeader.sectorSize);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Memory snapshot: {0} bytes", GC.GetTotalMemory(false));

            return sector;
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSector(trk.TrackStartSector + sectorAddress);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSectorTag(trk.TrackStartSector + sectorAddress, tag);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            uint   sectorOffset;
            uint   sectorSize;
            uint   sectorSkip;
            byte[] dataSource;

            if(imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                       sectorAddress <= t.TrackEndSector);

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
                    case SectorTagType.CdSectorSync: break;
                    case SectorTagType.CdTrackFlags:
                        return trackFlags.TryGetValue((byte)sectorAddress, out byte flags) ? new[]
                        {
                            flags
                        } : null;
                    case SectorTagType.CdTrackIsrc:
                        return trackIsrcs.TryGetValue((byte)sectorAddress, out string isrc)
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
                                dataSource   = sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorHeader:
                            {
                                sectorOffset = 12;
                                sectorSize   = 4;
                                sectorSkip   = 2336;
                                dataSource   = sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorSubHeader:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CdSectorEcc:
                            {
                                sectorOffset = 12;
                                sectorSize   = 276;
                                sectorSkip   = 0;
                                dataSource   = sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorEccP:
                            {
                                sectorOffset = 12;
                                sectorSize   = 172;
                                sectorSkip   = 104;
                                dataSource   = sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorEccQ:
                            {
                                sectorOffset = 184;
                                sectorSize   = 104;
                                sectorSkip   = 0;
                                dataSource   = sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorEdc:
                            {
                                sectorOffset = 0;
                                sectorSize   = 4;
                                sectorSkip   = 284;
                                dataSource   = sectorSuffix;

                                break;
                            }

                            case SectorTagType.CdSectorSubchannel:
                            {
                                sectorOffset = 0;
                                sectorSize   = 96;
                                sectorSkip   = 0;
                                dataSource   = sectorSubchannel;

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
                                dataSource   = sectorPrefix;

                                break;
                            }

                            case SectorTagType.CdSectorHeader:
                            {
                                sectorOffset = 12;
                                sectorSize   = 4;
                                sectorSkip   = 2336;
                                dataSource   = sectorPrefix;

                                break;
                            }

                            // These could be implemented
                            case SectorTagType.CdSectorEcc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorSubHeader:
                            case SectorTagType.CdSectorEdc:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CdSectorSubchannel:
                            {
                                sectorOffset = 0;
                                sectorSize   = 96;
                                sectorSkip   = 0;
                                dataSource   = sectorSubchannel;

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
                                dataSource   = sectorSubchannel;

                                break;
                            }

                            default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
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

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({(trk.TrackEndSector - trk.TrackStartSector) + 1}), won't cross tracks");

            return ReadSectors(trk.TrackStartSector + sectorAddress, length);
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({(trk.TrackEndSector - trk.TrackStartSector) + 1}), won't cross tracks");

            return ReadSectorsTag(trk.TrackStartSector + sectorAddress, length, tag);
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            switch(imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                           sectorAddress <= t.TrackEndSector);

                    if(trk.TrackSequence    == 0 &&
                       trk.TrackStartSector == 0 &&
                       trk.TrackEndSector   == 0)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if((sectorSuffix   == null || sectorPrefix   == null) &&
                       (sectorSuffixMs == null || sectorPrefixMs == null))
                        return ReadSector(sectorAddress);

                    byte[] sector = new byte[2352];
                    byte[] data   = ReadSector(sectorAddress);

                    switch(trk.TrackType)
                    {
                        case TrackType.Audio:
                        case TrackType.Data: return data;
                        case TrackType.CdMode1:
                            Array.Copy(data, 0, sector, 16, 2048);

                            if(sectorPrefix != null)
                                Array.Copy(sectorPrefix, (int)sectorAddress * 16, sector, 0, 16);
                            else if(sectorPrefixDdt != null)
                            {
                                if((sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                    ReconstructPrefix(ref sector, trk.TrackType, (long)sectorAddress);
                                else if((sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                        sectorPrefixDdt[sectorAddress]                  == 0)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    uint prefixPosition = ((sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;

                                    if(prefixPosition > sectorPrefixMs.Length)
                                        throw new
                                            InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                    sectorPrefixMs.Position = prefixPosition;

                                    sectorPrefixMs.Read(sector, 0, 16);
                                }
                            }
                            else
                                throw new InvalidProgramException("Should not have arrived here");

                            if(sectorSuffix != null)
                                Array.Copy(sectorSuffix, (int)sectorAddress * 288, sector, 2064, 288);
                            else if(sectorSuffixDdt != null)
                            {
                                if((sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                    ReconstructEcc(ref sector, trk.TrackType);
                                else if((sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                        sectorSuffixDdt[sectorAddress]                  == 0)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    uint suffixPosition = ((sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;

                                    if(suffixPosition > sectorSuffixMs.Length)
                                        throw new
                                            InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                    sectorSuffixMs.Position = suffixPosition;

                                    sectorSuffixMs.Read(sector, 2064, 288);
                                }
                            }
                            else
                                throw new InvalidProgramException("Should not have arrived here");

                            return sector;
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            if(sectorPrefix != null)
                                Array.Copy(sectorPrefix, (int)sectorAddress * 16, sector, 0, 16);
                            else if(sectorPrefixMs != null)
                            {
                                if((sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Correct)
                                    ReconstructPrefix(ref sector, trk.TrackType, (long)sectorAddress);
                                else if((sectorPrefixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                        sectorPrefixDdt[sectorAddress]                  == 0)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    uint prefixPosition = ((sectorPrefixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 16;

                                    if(prefixPosition > sectorPrefixMs.Length)
                                        throw new
                                            InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                    sectorPrefixMs.Position = prefixPosition;

                                    sectorPrefixMs.Read(sector, 0, 16);
                                }
                            }
                            else
                                throw new InvalidProgramException("Should not have arrived here");

                            if(mode2Subheaders != null &&
                               sectorSuffixDdt != null)
                            {
                                Array.Copy(mode2Subheaders, (int)sectorAddress * 8, sector, 16, 8);

                                if((sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Mode2Form1Ok)
                                {
                                    Array.Copy(data, 0, sector, 24, 2048);
                                    ReconstructEcc(ref sector, TrackType.CdMode2Form1);
                                }
                                else if((sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.Mode2Form2Ok ||
                                        (sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) ==
                                        (uint)CdFixFlags.Mode2Form2NoCrc)
                                {
                                    Array.Copy(data, 0, sector, 24, 2324);

                                    if((sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.Mode2Form2Ok)
                                        ReconstructEcc(ref sector, TrackType.CdMode2Form2);
                                }
                                else if((sectorSuffixDdt[sectorAddress] & CD_XFIX_MASK) == (uint)CdFixFlags.NotDumped ||
                                        sectorSuffixDdt[sectorAddress]                  == 0)
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
                                            ((sectorSuffixDdt[sectorAddress] & CD_DFIX_MASK) - 1) * 288;

                                        if(suffixPosition > sectorSuffixMs.Length)
                                            throw new
                                                InvalidProgramException("Incorrect data found in image, please re-dump. If issue persists, please open a bug report.");

                                        sectorSuffixMs.Position = suffixPosition;

                                        sectorSuffixMs.Read(sector, form2 ? 2348 : 2072, form2 ? 4 : 280);
                                        Array.Copy(data, 0, sector, 24, form2 ? 2324 : 2048);
                                    }
                                }
                            }
                            else if(mode2Subheaders != null)
                            {
                                Array.Copy(mode2Subheaders, (int)sectorAddress * 8, sector, 16, 8);
                                Array.Copy(data, 0, sector, 24, 2328);
                            }
                            else
                                Array.Copy(data, 0, sector, 16, 2336);

                            return sector;
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    switch(imageInfo.MediaType)
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

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSectorLong(trk.TrackStartSector + sectorAddress);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            byte[] sectors;
            byte[] data;

            switch(imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                           sectorAddress <= t.TrackEndSector);

                    if(trk.TrackSequence    == 0 &&
                       trk.TrackStartSector == 0 &&
                       trk.TrackEndSector   == 0)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if(sectorAddress + length > trk.TrackEndSector + 1)
                        throw new ArgumentOutOfRangeException(nameof(length),
                                                              $"Requested more sectors ({length + sectorAddress}) than present in track ({(trk.TrackEndSector - trk.TrackStartSector) + 1}), won't cross tracks");

                    switch(trk.TrackType)
                    {
                        // These types only contain user data
                        case TrackType.Audio:
                        case TrackType.Data: return ReadSectors(sectorAddress, length);

                        // Join prefix (sync, header) with user data with suffix (edc, ecc p, ecc q)
                        case TrackType.CdMode1:
                            if(sectorPrefix != null &&
                               sectorSuffix != null)
                            {
                                sectors = new byte[2352 * length];
                                data    = ReadSectors(sectorAddress, length);

                                for(uint i = 0; i < length; i++)
                                {
                                    Array.Copy(sectorPrefix, (int)((sectorAddress + i) * 16), sectors, (int)(i * 2352),
                                               16);

                                    Array.Copy(data, (int)(i * 2048), sectors, (int)(i * 2352) + 16, 2048);

                                    Array.Copy(sectorSuffix, (int)((sectorAddress + i) * 288), sectors,
                                               (int)(i * 2352) + 2064, 288);
                                }

                                return sectors;
                            }
                            else if(sectorPrefixDdt != null &&
                                    sectorSuffixDdt != null)
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
                            if(sectorPrefix != null &&
                               sectorSuffix != null)
                            {
                                sectors = new byte[2352 * length];
                                data    = ReadSectors(sectorAddress, length);

                                for(uint i = 0; i < length; i++)
                                {
                                    Array.Copy(sectorPrefix, (int)((sectorAddress + i) * 16), sectors, (int)(i * 2352),
                                               16);

                                    Array.Copy(data, (int)(i * 2336), sectors, (int)(i * 2352) + 16, 2336);
                                }

                                return sectors;
                            }
                            else if(sectorPrefixDdt != null &&
                                    sectorSuffixDdt != null)
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
                    switch(imageInfo.MediaType)
                    {
                        // Join user data with tags
                        case MediaType.AppleFileWare:
                        case MediaType.AppleProfile:
                        case MediaType.AppleSonySS:
                        case MediaType.AppleSonyDS:
                        case MediaType.AppleWidget:
                        case MediaType.PriamDataTower:
                            if(sectorSubchannel == null)
                                return ReadSector(sectorAddress);

                            uint tagSize = 0;

                            switch(imageInfo.MediaType)
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
                                Array.Copy(sectorSubchannel, (int)((sectorAddress + i) * tagSize), sectors,
                                           (int)((i * sectorSize) + 512), tagSize);

                                Array.Copy(data, (int)((sectorAddress + i) * 512), sectors, (int)(i * 512), 512);
                            }

                            return sectors;
                    }

                    break;
            }

            throw new FeatureNotPresentImageException("Feature not present in image");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);

            if(trk.TrackSequence != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({(trk.TrackEndSector - trk.TrackStartSector) + 1}), won't cross tracks");

            return ReadSectorsLong(trk.TrackStartSector + sectorAddress, length);
        }

        public List<Track> GetSessionTracks(Session session) =>
            Tracks.Where(t => t.TrackSequence == session.SessionSequence).ToList();

        public List<Track> GetSessionTracks(ushort session) => Tracks.Where(t => t.TrackSequence == session).ToList();
    }
}