// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiscImageChef.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DiscImageChef format disk images.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

/*
 The idea of the format is being able to easily store, retrieve, and access any data that can be read from media.

 At the start of a file there's a header that contains a format version, application creator name, and a pointer to
 the index.
 
 The index points to one or several DeDuplication Tables, or media tag blocks.
 
 A deduplication table is a table of offsets to blocks and sectors inside blocks. Each entry equals to an LBA and points
 to a byte offset in the file shift left to the number of sectors contained in a block, plus the number of sector inside
 the block.
 Each block must contain sectors of equal size, but that size can be different between blocks.
 The deduplication table should be stored decompressed if its size is too big to be stored on-memory. This is chosen at
 creation time but it is a good idea to set the limit to 256MiB (this allows for a device of 33 million sectors,
 17Gb at 512 bps, 68Gb at 2048 bps and 137Gb at 4096 bps).
 
 Sector tags that are simply too small to be deduplicated are contained in a single block pointed by the index (e.g.
 Apple GCR sector tags). 
 
 Optical disks contain a track block that describes the tracks.
 Streaming tapes contain a file block that describes the files and an optional partition block that describes the tape
 partitions.
 
 There are also blocks for image metadata, contents metadata and dump hardware information.
 
 A differencing image will have all the metadata and deduplication tables, but the entries in these ones will be set to
 0 if the block is stored in the parent image. This is not yet implemented.
 
 Also because the file becomes useless without the index and deduplication table, each can be stored twice. In case of
 the index it should just be searched for. In case of deduplication tables, both copies should be indexed.
 
 Finally, writing new data to an existing image is just Copy-On-Write. Create a new block with the modified data, change
 the pointer in the corresponding deduplication table.
 
 P.S.: Data Position Measurement is doable, as soon as I know how to do it.
 P.S.2: Support for floppy image containg bitslices and/or fluxes will be added soon.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders;
using DiscImageChef.Filters;
using SharpCompress.Compressors.LZMA;

namespace DiscImageChef.DiscImages
{
    // TODO: Work in progress
    // TODO: Get manufacurer, model, firmware, from tags, if available
    public class DiscImageChef : IWritableImage
    {
        const ulong DIC_MAGIC              = 0x544D464444434944;
        const byte  DICF_VERSION           = 0;
        const uint  MAX_CACHE_SIZE         = 256 * 1024 * 1024;
        const int   LZMA_PROPERTIES_LENGTH = 5;
        const int   MAX_DDT_ENTRY_CACHE    = 16000000;

        Dictionary<ulong, byte[]>        blockCache;
        Dictionary<ulong, BlockHeader>   blockHeaderCache;
        MemoryStream                     blockStream;
        SHA256                           checksumProvider;
        LzmaStream                       compressedBlockStream;
        Crc64Context                     crc64;
        BlockHeader                      currentBlockHeader;
        uint                             currentBlockOffset;
        uint                             currentCacheSize;
        Dictionary<ulong, ulong>         ddtEntryCache;
        Dictionary<byte[], ulong>        deduplicationTable;
        GeometryBlock                    geometryBlock;
        DicHeader                        header;
        ImageInfo                        imageInfo;
        Stream                           imageStream;
        List<IndexEntry>                 index;
        bool                             inMemoryDdt;
        LzmaEncoderProperties            lzmaEncoderProperties;
        Dictionary<MediaTagType, byte[]> mediaTags;
        long                             outMemoryDdtPosition;
        byte[]                           sectorPrefix;
        byte[]                           sectorSubchannel;
        byte[]                           sectorSuffix;
        byte                             shift;
        byte[]                           structureBytes;
        IntPtr                           structurePointer;
        Dictionary<byte, byte>           trackFlags;
        Dictionary<byte, string>         trackIsrcs;
        ulong[]                          userDataDdt;

        public DiscImageChef()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = null,
                Application           = "DiscImageChef",
                ApplicationVersion    = null,
                Creator               = null,
                Comments              = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaSerialNumber     = null,
                MediaBarcode          = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
        }

        public ImageInfo       Info       => imageInfo;
        public string          Name       => "DiscImageChef format";
        public Guid            Id         => new Guid("49360069-1784-4A2F-B723-0C844D610B0A");
        public string          Format     => "DiscImageChef";
        public List<Partition> Partitions { get; private set; }
        public List<Track>     Tracks     { get; private set; }
        public List<Session>   Sessions   { get; private set; }

        public bool Identify(IFilter imageFilter)
        {
            imageStream = imageFilter.GetDataForkStream();
            imageStream.Seek(0, SeekOrigin.Begin);

            if(imageStream.Length < 512) return false;

            header         = new DicHeader();
            structureBytes = new byte[Marshal.SizeOf(header)];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(header));
            header = (DicHeader)Marshal.PtrToStructure(structurePointer, typeof(DicHeader));
            Marshal.FreeHGlobal(structurePointer);

            return header.identifier == DIC_MAGIC && header.imageMajorVersion == 0;
        }

        public bool Open(IFilter imageFilter)
        {
            imageStream = imageFilter.GetDataForkStream();
            imageStream.Seek(0, SeekOrigin.Begin);

            if(imageStream.Length < 512) return false;

            header         = new DicHeader();
            structureBytes = new byte[Marshal.SizeOf(header)];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(header));
            header = (DicHeader)Marshal.PtrToStructure(structurePointer, typeof(DicHeader));
            Marshal.FreeHGlobal(structurePointer);

            if(header.imageMajorVersion > DICF_VERSION)
                throw new FeatureUnsupportedImageException($"Image version {header.imageMajorVersion} not recognized.");

            imageInfo.Application        = header.application;
            imageInfo.ApplicationVersion = $"{header.applicationMajorVersion}.{header.applicationMinorVersion}";
            imageInfo.Version            = $"{header.imageMajorVersion}.{header.imageMinorVersion}";
            imageInfo.MediaType          = header.mediaType;

            imageStream.Position  = (long)header.indexOffset;
            IndexHeader idxHeader = new IndexHeader();
            structureBytes        = new byte[Marshal.SizeOf(idxHeader)];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(idxHeader));
            Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(idxHeader));
            idxHeader = (IndexHeader)Marshal.PtrToStructure(structurePointer, typeof(IndexHeader));
            Marshal.FreeHGlobal(structurePointer);

            if(idxHeader.identifier != BlockType.Index) throw new FeatureUnsupportedImageException("Index not found!");

            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Index at {0} contains {1} entries",
                                      header.indexOffset, idxHeader.entries);

            index = new List<IndexEntry>();
            for(ushort i = 0; i < idxHeader.entries; i++)
            {
                IndexEntry entry = new IndexEntry();
                structureBytes   = new byte[Marshal.SizeOf(entry)];
                imageStream.Read(structureBytes, 0, structureBytes.Length);
                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(entry));
                Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(entry));
                entry = (IndexEntry)Marshal.PtrToStructure(structurePointer, typeof(IndexEntry));
                Marshal.FreeHGlobal(structurePointer);
                DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                          "Block type {0} with data type {1} is indexed to be at {2}", entry.blockType,
                                          entry.dataType, entry.offset);
                index.Add(entry);
            }

            imageInfo.ImageSize = 0;

            bool foundUserDataDdt = false;
            mediaTags             = new Dictionary<MediaTagType, byte[]>();
            foreach(IndexEntry entry in index)
            {
                imageStream.Position = (long)entry.offset;
                switch(entry.blockType)
                {
                    case BlockType.DataBlock:
                        // NOP block, skip
                        if(entry.dataType == DataType.NoData) break;

                        imageStream.Position = (long)entry.offset;

                        BlockHeader blockHeader = new BlockHeader();
                        structureBytes          = new byte[Marshal.SizeOf(blockHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(blockHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(blockHeader));
                        blockHeader = (BlockHeader)Marshal.PtrToStructure(structurePointer, typeof(BlockHeader));
                        Marshal.FreeHGlobal(structurePointer);
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
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Incorrect identifier for data block at position {0}",
                                                      entry.offset);
                            break;
                        }

                        if(blockHeader.type != entry.dataType)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Expected block with data type {0} at position {1} but found data type {2}",
                                                      entry.dataType, entry.offset, blockHeader.type);
                            break;
                        }

                        byte[] data;

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Found data block type {0} at position {1}", entry.dataType,
                                                  entry.offset);

                        if(blockHeader.compression == CompressionType.Lzma)
                        {
                            byte[] compressedTag  = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                            byte[] lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                            imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                            imageStream.Read(compressedTag,  0, compressedTag.Length);
                            MemoryStream compressedTagMs = new MemoryStream(compressedTag);
                            LzmaStream   lzmaBlock       = new LzmaStream(lzmaProperties, compressedTagMs);
                            data                         = new byte[blockHeader.length];
                            lzmaBlock.Read(data, 0, (int)blockHeader.length);
                            lzmaBlock.Close();
                            compressedTagMs.Close();
                        }
                        else if(blockHeader.compression == CompressionType.None)
                        {
                            data = new byte[blockHeader.length];
                            imageStream.Read(data, 0, (int)blockHeader.length);
                        }
                        else
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Found unknown compression type {0}, continuing...",
                                                      (ushort)blockHeader.compression);
                            break;
                        }

                        Crc64Context.Data(data, out byte[] blockCrc);
                        blockCrc = blockCrc.Reverse().ToArray();
                        if(BitConverter.ToUInt64(blockCrc, 0) != blockHeader.crc64)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                      BitConverter.ToUInt64(blockCrc, 0), blockHeader.crc64);
                            break;
                        }

                        switch(entry.dataType)
                        {
                            case DataType.CdSectorPrefix:
                                sectorPrefix = data;
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                                break;
                            case DataType.CdSectorSuffix:
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
                                break;
                            case DataType.CdSectorSubchannel:
                                sectorSubchannel = data;
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                                break;
                            case DataType.AppleProfileTag:
                            case DataType.AppleSonyTag:
                            case DataType.PriamDataTowerTag:
                                sectorSubchannel = data;
                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
                                break;
                            default:
                                MediaTagType mediaTagType = GetMediaTagTypeForDataType(blockHeader.type);

                                if(mediaTags.ContainsKey(mediaTagType))
                                {
                                    DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                              "Media tag type {0} duplicated, removing previous entry...",
                                                              mediaTagType);

                                    mediaTags.Remove(mediaTagType);
                                }

                                mediaTags.Add(mediaTagType, data);
                                break;
                        }

                        break;
                    case BlockType.DeDuplicationTable:
                        // Only user data deduplication tables are used right now
                        if(entry.dataType != DataType.UserData) break;

                        DdtHeader ddtHeader = new DdtHeader();
                        structureBytes      = new byte[Marshal.SizeOf(ddtHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(ddtHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(ddtHeader));
                        ddtHeader = (DdtHeader)Marshal.PtrToStructure(structurePointer, typeof(DdtHeader));
                        Marshal.FreeHGlobal(structurePointer);
                        imageInfo.ImageSize += ddtHeader.cmpLength;

                        if(ddtHeader.identifier != BlockType.DeDuplicationTable) break;

                        imageInfo.Sectors = ddtHeader.entries;
                        shift             = ddtHeader.shift;

                        switch(ddtHeader.compression)
                        {
                            case CompressionType.Lzma:
                                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Decompressing DDT...");
                                DateTime ddtStart       = DateTime.UtcNow;
                                byte[]   compressedDdt  = new byte[ddtHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                                byte[]   lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                imageStream.Read(compressedDdt,  0, compressedDdt.Length);
                                MemoryStream compressedDdtMs = new MemoryStream(compressedDdt);
                                LzmaStream   lzmaDdt         = new LzmaStream(lzmaProperties, compressedDdtMs);
                                byte[]       decompressedDdt = new byte[ddtHeader.length];
                                lzmaDdt.Read(decompressedDdt, 0, (int)ddtHeader.length);
                                lzmaDdt.Close();
                                compressedDdtMs.Close();
                                userDataDdt   = new ulong[ddtHeader.entries];
                                for(ulong i = 0; i < ddtHeader.entries; i++)
                                    userDataDdt[i] = BitConverter.ToUInt64(decompressedDdt, (int)(i * sizeof(ulong)));
                                DateTime ddtEnd    = DateTime.UtcNow;
                                inMemoryDdt        = true;
                                DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                          "Took {0} seconds to decompress DDT",
                                                          (ddtEnd - ddtStart).TotalSeconds);
                                break;
                            case CompressionType.None:
                                inMemoryDdt          = false;
                                outMemoryDdtPosition = (long)entry.offset;
                                break;
                            default:
                                throw new
                                    ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)ddtHeader.compression}");
                        }

                        foundUserDataDdt = true;
                        break;
                    case BlockType.GeometryBlock:
                        geometryBlock  = new GeometryBlock();
                        structureBytes = new byte[Marshal.SizeOf(geometryBlock)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(geometryBlock));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(geometryBlock));
                        geometryBlock = (GeometryBlock)Marshal.PtrToStructure(structurePointer, typeof(GeometryBlock));
                        Marshal.FreeHGlobal(structurePointer);
                        if(geometryBlock.identifier == BlockType.GeometryBlock)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Geometry set to {0} cylinders {1} heads {2} sectors per track",
                                                      geometryBlock.cylinders, geometryBlock.heads,
                                                      geometryBlock.sectorsPerTrack);
                            imageInfo.Cylinders       = geometryBlock.cylinders;
                            imageInfo.Heads           = geometryBlock.heads;
                            imageInfo.SectorsPerTrack = geometryBlock.sectorsPerTrack;
                        }

                        break;
                    case BlockType.MetadataBlock:
                        MetadataBlock metadataBlock = new MetadataBlock();
                        structureBytes              = new byte[Marshal.SizeOf(metadataBlock)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(metadataBlock));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(metadataBlock));
                        metadataBlock = (MetadataBlock)Marshal.PtrToStructure(structurePointer, typeof(MetadataBlock));
                        Marshal.FreeHGlobal(structurePointer);

                        if(metadataBlock.identifier != entry.blockType)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Incorrect identifier for data block at position {0}",
                                                      entry.offset);
                            break;
                        }

                        DicConsole.DebugWriteLine("DiscImageChef format plugin", "Found metadata block at position {0}",
                                                  entry.offset);

                        byte[] metadata      = new byte[metadataBlock.blockSize];
                        imageStream.Position = (long)entry.offset;
                        imageStream.Read(metadata, 0, metadata.Length);

                        if(metadataBlock.mediaSequence > 0 && metadataBlock.lastMediaSequence > 0)
                        {
                            imageInfo.MediaSequence     = metadataBlock.mediaSequence;
                            imageInfo.LastMediaSequence = metadataBlock.lastMediaSequence;
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Setting media sequence as {0} of {1}", imageInfo.MediaSequence,
                                                      imageInfo.LastMediaSequence);
                        }

                        if(metadataBlock.creatorLength                               > 0 &&
                           metadataBlock.creatorLength + metadataBlock.creatorOffset <= metadata.Length)
                        {
                            imageInfo.Creator = Encoding.Unicode.GetString(metadata, (int)metadataBlock.creatorOffset,
                                                                           (int)(metadataBlock.creatorLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting creator: {0}",
                                                      imageInfo.Creator);
                        }

                        if(metadataBlock.commentsOffset                                > 0 &&
                           metadataBlock.commentsLength + metadataBlock.commentsOffset <= metadata.Length)
                        {
                            imageInfo.Comments =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.commentsOffset,
                                                           (int)(metadataBlock.commentsLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting comments: {0}",
                                                      imageInfo.Comments);
                        }

                        if(metadataBlock.mediaTitleOffset                                  > 0 &&
                           metadataBlock.mediaTitleLength + metadataBlock.mediaTitleOffset <= metadata.Length)
                        {
                            imageInfo.MediaTitle =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaTitleOffset,
                                                           (int)(metadataBlock.mediaTitleLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting media title: {0}",
                                                      imageInfo.MediaTitle);
                        }

                        if(metadataBlock.mediaManufacturerOffset                                         > 0 &&
                           metadataBlock.mediaManufacturerLength + metadataBlock.mediaManufacturerOffset <=
                           metadata.Length)
                        {
                            imageInfo.MediaManufacturer =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaManufacturerOffset,
                                                           (int)(metadataBlock.mediaManufacturerLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting media manufacturer: {0}",
                                                      imageInfo.MediaManufacturer);
                        }

                        if(metadataBlock.mediaModelOffset                                  > 0 &&
                           metadataBlock.mediaModelLength + metadataBlock.mediaModelOffset <= metadata.Length)
                        {
                            imageInfo.MediaModel =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaModelOffset,
                                                           (int)(metadataBlock.mediaModelLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting media model: {0}",
                                                      imageInfo.MediaModel);
                        }

                        if(metadataBlock.mediaSerialNumberOffset                                         > 0 &&
                           metadataBlock.mediaSerialNumberLength + metadataBlock.mediaSerialNumberOffset <=
                           metadata.Length)
                        {
                            imageInfo.MediaSerialNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaSerialNumberOffset,
                                                           (int)(metadataBlock.mediaSerialNumberLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting media serial number: {0}",
                                                      imageInfo.MediaSerialNumber);
                        }

                        if(metadataBlock.mediaBarcodeOffset                                    > 0 &&
                           metadataBlock.mediaBarcodeLength + metadataBlock.mediaBarcodeOffset <= metadata.Length)
                        {
                            imageInfo.MediaBarcode =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaBarcodeOffset,
                                                           (int)(metadataBlock.mediaBarcodeLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting media barcode: {0}",
                                                      imageInfo.MediaBarcode);
                        }

                        if(metadataBlock.mediaPartNumberOffset                                       > 0 &&
                           metadataBlock.mediaPartNumberLength + metadataBlock.mediaPartNumberOffset <= metadata.Length)
                        {
                            imageInfo.MediaPartNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.mediaPartNumberOffset,
                                                           (int)(metadataBlock.mediaPartNumberLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting media part number: {0}",
                                                      imageInfo.MediaPartNumber);
                        }

                        if(metadataBlock.driveManufacturerOffset                                         > 0 &&
                           metadataBlock.driveManufacturerLength + metadataBlock.driveManufacturerOffset <=
                           metadata.Length)
                        {
                            imageInfo.DriveManufacturer =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveManufacturerOffset,
                                                           (int)(metadataBlock.driveManufacturerLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting drive manufacturer: {0}",
                                                      imageInfo.DriveManufacturer);
                        }

                        if(metadataBlock.driveModelOffset                                  > 0 &&
                           metadataBlock.driveModelLength + metadataBlock.driveModelOffset <= metadata.Length)
                        {
                            imageInfo.DriveModel =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveModelOffset,
                                                           (int)(metadataBlock.driveModelLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting drive model: {0}",
                                                      imageInfo.DriveModel);
                        }

                        if(metadataBlock.driveSerialNumberOffset                                         > 0 &&
                           metadataBlock.driveSerialNumberLength + metadataBlock.driveSerialNumberOffset <=
                           metadata.Length)
                        {
                            imageInfo.DriveSerialNumber =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveSerialNumberOffset,
                                                           (int)(metadataBlock.driveSerialNumberLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Setting drive serial number: {0}",
                                                      imageInfo.DriveSerialNumber);
                        }

                        if(metadataBlock.driveFirmwareRevisionOffset > 0 && metadataBlock.driveFirmwareRevisionLength +
                           metadataBlock.driveFirmwareRevisionOffset <= metadata.Length)
                        {
                            imageInfo.DriveFirmwareRevision =
                                Encoding.Unicode.GetString(metadata, (int)metadataBlock.driveFirmwareRevisionOffset,
                                                           (int)(metadataBlock.driveFirmwareRevisionLength - 2));

                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Setting drive firmware revision: {0}",
                                                      imageInfo.DriveFirmwareRevision);
                        }

                        break;
                    case BlockType.TracksBlock:
                        TracksHeader tracksHeader = new TracksHeader();
                        structureBytes            = new byte[Marshal.SizeOf(tracksHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(tracksHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(tracksHeader));
                        tracksHeader = (TracksHeader)Marshal.PtrToStructure(structurePointer, typeof(TracksHeader));
                        Marshal.FreeHGlobal(structurePointer);
                        if(tracksHeader.identifier != BlockType.TracksBlock)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Incorrect identifier for tracks block at position {0}",
                                                      entry.offset);
                            break;
                        }

                        structureBytes = new byte[Marshal.SizeOf(typeof(TrackEntry)) * tracksHeader.entries];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        Crc64Context.Data(structureBytes, out byte[] trksCrc);
                        if(BitConverter.ToUInt64(trksCrc, 0) != tracksHeader.crc64)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                      BitConverter.ToUInt64(trksCrc, 0), tracksHeader.crc64);
                            break;
                        }

                        imageStream.Position -= structureBytes.Length;

                        Tracks     = new List<Track>();
                        trackFlags = new Dictionary<byte, byte>();
                        trackIsrcs = new Dictionary<byte, string>();

                        DicConsole.DebugWriteLine("DiscImageChef format plugin", "Found {0} tracks at position {0}",
                                                  tracksHeader.entries, entry.offset);

                        for(ushort i = 0; i < tracksHeader.entries; i++)
                        {
                            TrackEntry trackEntry = new TrackEntry();
                            structureBytes        = new byte[Marshal.SizeOf(trackEntry)];
                            imageStream.Read(structureBytes, 0, structureBytes.Length);
                            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(trackEntry));
                            Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(trackEntry));
                            trackEntry = (TrackEntry)Marshal.PtrToStructure(structurePointer, typeof(TrackEntry));
                            Marshal.FreeHGlobal(structurePointer);

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

                            trackFlags.Add(trackEntry.sequence, trackEntry.flags);
                            trackIsrcs.Add(trackEntry.sequence, trackEntry.isrc);
                        }

                        break;
                }
            }

            if(!foundUserDataDdt) throw new ImageNotSupportedException("Could not find user data deduplication table.");

            imageInfo.CreationTime = DateTime.FromFileTimeUtc(header.creationTime);
            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Image created on", imageInfo.CreationTime);
            imageInfo.LastModificationTime = DateTime.FromFileTimeUtc(header.lastWrittenTime);
            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Image last written on",
                                      imageInfo.LastModificationTime);

            if(geometryBlock.identifier != BlockType.GeometryBlock && imageInfo.XmlMediaType == XmlMediaType.BlockMedia)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;
            }

            imageInfo.XmlMediaType = GetXmlMediaType(header.mediaType);
            imageInfo.ReadableMediaTags.AddRange(mediaTags.Keys);

            // Initialize caches
            blockCache                     = new Dictionary<ulong, byte[]>();
            blockHeaderCache               = new Dictionary<ulong, BlockHeader>();
            currentCacheSize               = 0;
            if(!inMemoryDdt) ddtEntryCache = new Dictionary<ulong, ulong>();

            if(imageInfo.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                if(Tracks == null || Tracks.Count == 0)
                {
                    Tracks = new List<Track>
                    {
                        new Track
                        {
                            Indexes                = new Dictionary<int, ulong>(),
                            TrackBytesPerSector    = (int)imageInfo.SectorSize,
                            TrackEndSector         = imageInfo.Sectors - 1,
                            TrackFile              = imageFilter.GetFilename(),
                            TrackFileType          = "BINARY",
                            TrackFilter            = imageFilter,
                            TrackRawBytesPerSector = (int)imageInfo.SectorSize,
                            TrackSession           = 1,
                            TrackSequence          = 1,
                            TrackType              = TrackType.Data
                        }
                    };

                    trackFlags = new Dictionary<byte, byte> {{1, (byte)CdFlags.DataTrack}};
                    trackIsrcs = new Dictionary<byte, string>();
                }

                Sessions = new List<Session>();
                for(int i = 1; i <= Tracks.Max(t => t.TrackSession); i++)
                    Sessions.Add(new Session
                    {
                        SessionSequence = (ushort)i,
                        StartTrack      = Tracks.Where(t => t.TrackSession == i).Max(t => t.TrackSequence),
                        EndTrack        = Tracks.Where(t => t.TrackSession == i).Min(t => t.TrackSequence),
                        StartSector     = Tracks.Where(t => t.TrackSession == i).Min(t => t.TrackStartSector),
                        EndSector       = Tracks.Where(t => t.TrackSession == i).Max(t => t.TrackEndSector)
                    });

                ulong currentTrackOffset = 0;
                Partitions               = new List<Partition>();
                foreach(Track track in Tracks.OrderBy(t => t.TrackStartSector))
                {
                    Partitions.Add(new Partition
                    {
                        Sequence = track.TrackSequence,
                        Type     = track.TrackType.ToString(),
                        Name     = $"Track {track.TrackSequence}",
                        Offset   = currentTrackOffset,
                        Start    = track.TrackStartSector,
                        Size     = (track.TrackEndSector - track.TrackStartSector + 1) *
                                   (ulong)track.TrackBytesPerSector,
                        Length = track.TrackEndSector - track.TrackStartSector + 1,
                        Scheme = "Optical disc track"
                    });
                    currentTrackOffset += (track.TrackEndSector - track.TrackStartSector + 1) *
                                          (ulong)track.TrackBytesPerSector;
                }

                Track[] tracks = Tracks.ToArray();
                for(int i = 0; i < tracks.Length; i++)
                {
                    byte[] sector                    = ReadSector(tracks[i].TrackStartSector);
                    tracks[i].TrackBytesPerSector    = sector.Length;
                    tracks[i].TrackRawBytesPerSector =
                        sectorPrefix != null && sectorSuffix != null ? 2352 : sector.Length;

                    if(sectorSubchannel == null) continue;

                    tracks[i].TrackSubchannelFile   = tracks[i].TrackFile;
                    tracks[i].TrackSubchannelFilter = tracks[i].TrackFilter;
                    tracks[i].TrackSubchannelType   = TrackSubchannelType.Raw;
                }

                Tracks = tracks.ToList();
            }
            else
            {
                Tracks     = null;
                Sessions   = null;
                Partitions = null;
            }

            return true;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(mediaTags.TryGetValue(tag, out byte[] data)) return data;

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
            if(ddtEntry == 0) return new byte[imageInfo.SectorSize];

            byte[] sector;

            if(blockCache.TryGetValue(blockOffset, out byte[] block) &&
               blockHeaderCache.TryGetValue(blockOffset, out BlockHeader blockHeader))
            {
                sector = new byte[blockHeader.sectorSize];
                Array.Copy(block, (long)(offset * blockHeader.sectorSize), sector, 0, blockHeader.sectorSize);
                return sector;
            }

            imageStream.Position = (long)blockOffset;
            blockHeader          = new BlockHeader();
            structureBytes       = new byte[Marshal.SizeOf(blockHeader)];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(blockHeader));
            Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(blockHeader));
            blockHeader = (BlockHeader)Marshal.PtrToStructure(structurePointer, typeof(BlockHeader));
            Marshal.FreeHGlobal(structurePointer);

            switch(blockHeader.compression)
            {
                case CompressionType.None:
                    block = new byte[blockHeader.length];
                    imageStream.Read(block, 0, (int)blockHeader.length);
                    break;
                case CompressionType.Lzma:
                    byte[] compressedBlock = new byte[blockHeader.cmpLength - LZMA_PROPERTIES_LENGTH];
                    byte[] lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                    imageStream.Read(lzmaProperties,  0, LZMA_PROPERTIES_LENGTH);
                    imageStream.Read(compressedBlock, 0, compressedBlock.Length);
                    MemoryStream compressedBlockMs = new MemoryStream(compressedBlock);
                    LzmaStream   lzmaBlock         = new LzmaStream(lzmaProperties, compressedBlockMs);
                    block                          = new byte[blockHeader.length];
                    lzmaBlock.Read(block, 0, (int)blockHeader.length);
                    lzmaBlock.Close();
                    compressedBlockMs.Close();
                    break;
                default:
                    throw new
                        ImageNotSupportedException($"Found unsupported compression algorithm {(ushort)blockHeader.compression}");
            }

            if(currentCacheSize + blockHeader.length >= MAX_CACHE_SIZE)
            {
                currentCacheSize = 0;
                blockHeaderCache = new Dictionary<ulong, BlockHeader>();
                blockCache       = new Dictionary<ulong, byte[]>();
            }

            currentCacheSize += blockHeader.length;
            blockHeaderCache.Add(blockOffset, blockHeader);
            blockCache.Add(blockOffset, block);

            sector = new byte[blockHeader.sectorSize];
            Array.Copy(block, (long)(offset * blockHeader.sectorSize), sector, 0, blockHeader.sectorSize);
            return sector;
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);
            if(trk.TrackSequence                                   != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            return ReadSector(trk.TrackStartSector + sectorAddress);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);
            if(trk.TrackSequence                                   != track)
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

            MemoryStream ms = new MemoryStream();

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
                if(trk.TrackSequence                                 == 0)
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
                        return trackFlags.TryGetValue((byte)trk.TrackSequence, out byte flags) ? new[] {flags} : null;
                    case SectorTagType.CdTrackIsrc:
                        return trackIsrcs.TryGetValue((byte)trk.TrackSequence, out string isrc)
                                   ? Encoding.UTF8.GetBytes(isrc)
                                   : null;
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
            else throw new FeatureNotPresentImageException("Feature not present in image");

            if(dataSource == null) throw new ArgumentException("Unsupported tag requested", nameof(tag));

            byte[] data = new byte[sectorSize * length];

            if(sectorOffset == 0 && sectorSkip == 0)
            {
                Array.Copy(dataSource, (long)(sectorAddress * sectorSize), data, 0, length * sectorSize);
                return data;
            }

            for(int i = 0; i < length; i++)
                Array.Copy(dataSource, (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)), data,
                           i                                * sectorSize, sectorSize);

            return data;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);
            if(trk.TrackSequence                                   != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

            return ReadSectors(trk.TrackStartSector + sectorAddress, length);
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                throw new FeatureNotPresentImageException("Feature not present in image");

            Track trk = Tracks.FirstOrDefault(t => t.TrackSequence == track);
            if(trk.TrackSequence                                   != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

            return ReadSectorsTag(trk.TrackStartSector + sectorAddress, length, tag);
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            switch(imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.TrackStartSector &&
                                                           sectorAddress <= t.TrackEndSector);
                    if(trk.TrackSequence                                 == 0)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if(sectorSuffix == null || sectorPrefix == null) return ReadSector(sectorAddress);

                    byte[] sector = new byte[2352];
                    byte[] data   = ReadSector(sectorAddress);

                    switch(trk.TrackType)
                    {
                        case TrackType.Audio:
                        case TrackType.Data: return data;
                        case TrackType.CdMode1:
                            Array.Copy(sectorPrefix, (int)sectorAddress * 16,  sector, 0,    16);
                            Array.Copy(data,         0,                        sector, 16,   2048);
                            Array.Copy(sectorSuffix, (int)sectorAddress * 288, sector, 2064, 288);
                            return sector;
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            Array.Copy(sectorPrefix, (int)sectorAddress * 16, sector, 0,  16);
                            Array.Copy(data,         0,                       sector, 16, 2336);
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
            if(trk.TrackSequence                                   != track)
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
                    if(trk.TrackSequence                                 == 0)
                        throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                              "Can't found track containing requested sector");

                    if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                        throw new ArgumentOutOfRangeException(nameof(length),
                                                              $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

                    switch(trk.TrackType)
                    {
                        case TrackType.Audio:
                        case TrackType.Data: return ReadSectors(sectorAddress, length);
                        case TrackType.CdMode1:
                            if(sectorPrefix == null || sectorSuffix == null) return ReadSectors(sectorAddress, length);

                            sectors = new byte[2352 * length];
                            data    = ReadSectors(sectorAddress, length);
                            for(uint i = 0; i < length; i++)
                            {
                                Array.Copy(sectorPrefix, (int)((sectorAddress + i) * 16), sectors,
                                           (int)((sectorAddress               + i) * 2352), 16);
                                Array.Copy(data, (int)((sectorAddress         + i) * 2048), sectors,
                                           (int)((sectorAddress               + i) * 2352) + 16, 2048);
                                Array.Copy(sectorSuffix, (int)((sectorAddress + i) * 288), sectors,
                                           (int)((sectorAddress               + i) * 2352) + 2064, 288);
                            }

                            return sectors;
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            if(sectorPrefix == null || sectorSuffix == null) return ReadSectors(sectorAddress, length);

                            sectors = new byte[2352 * length];
                            data    = ReadSectors(sectorAddress, length);
                            for(uint i = 0; i < length; i++)
                            {
                                Array.Copy(sectorPrefix, (int)((sectorAddress + i) * 16), sectors,
                                           (int)((sectorAddress               + i) * 2352), 16);
                                Array.Copy(data, (int)((sectorAddress         + i) * 2336), sectors,
                                           (int)((sectorAddress               + i) * 2352) + 16, 2336);
                            }

                            return sectors;
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
                        case MediaType.PriamDataTower:
                            if(sectorSubchannel == null) return ReadSector(sectorAddress);

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
                            data            = ReadSectors(sectorAddress, length);
                            sectors         = new byte[(sectorSize + 512) * length];
                            for(uint i = 0; i < length; i++)
                            {
                                Array.Copy(sectorSubchannel, (int)((sectorAddress + i) * tagSize), sectors,
                                           (int)((sectorAddress                   + i) * sectorSize + 512), tagSize);
                                Array.Copy(data, (int)((sectorAddress             + i) * 512), sectors,
                                           (int)((sectorAddress                   + i) * 512), 512);
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
            if(trk.TrackSequence                                   != track)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(trk.TrackStartSector + sectorAddress + length > trk.TrackEndSector + 1)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({trk.TrackEndSector - trk.TrackStartSector + 1}), won't cross tracks");

            return ReadSectorsLong(trk.TrackStartSector + sectorAddress, length);
        }

        public List<Track> GetSessionTracks(Session session)
        {
            return Tracks.Where(t => t.TrackSequence == session.SessionSequence).ToList();
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            return Tracks.Where(t => t.TrackSequence == session).ToList();
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc) return null;

            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc) return null;

            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            {
                for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

                return null;
            }

            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas   = new List<ulong>();
            unknownLbas   = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;

            return failingLbas.Count <= 0;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
            {
                failingLbas = new List<ulong>();
                unknownLbas = new List<ulong>();

                for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

                return null;
            }

            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas   = new List<ulong>();
            unknownLbas   = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;

            return failingLbas.Count <= 0;
        }

        public bool? VerifyMediaImage()
        {
            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Checking index integrity at {0}",
                                      header.indexOffset);
            imageStream.Position = (long)header.indexOffset;

            IndexHeader idxHeader = new IndexHeader();
            structureBytes        = new byte[Marshal.SizeOf(idxHeader)];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(idxHeader));
            Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(idxHeader));
            idxHeader = (IndexHeader)Marshal.PtrToStructure(structurePointer, typeof(IndexHeader));
            Marshal.FreeHGlobal(structurePointer);

            if(idxHeader.identifier != BlockType.Index)
            {
                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Incorrect index identifier");
                return false;
            }

            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Index at {0} contains {1} entries",
                                      header.indexOffset, idxHeader.entries);

            structureBytes = new byte[Marshal.SizeOf(typeof(IndexEntry)) * idxHeader.entries];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            Crc64Context.Data(structureBytes, out byte[] verifyCrc);

            if(BitConverter.ToUInt64(verifyCrc, 0) != idxHeader.crc64)
            {
                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Expected index CRC {0:X16} but got {1:X16}",
                                          idxHeader.crc64, BitConverter.ToUInt64(verifyCrc, 0));
                return false;
            }

            imageStream.Position -= structureBytes.Length;

            List<IndexEntry> vrIndex = new List<IndexEntry>();
            for(ushort i = 0; i < idxHeader.entries; i++)
            {
                IndexEntry entry = new IndexEntry();
                structureBytes   = new byte[Marshal.SizeOf(entry)];
                imageStream.Read(structureBytes, 0, structureBytes.Length);
                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(entry));
                Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(entry));
                entry = (IndexEntry)Marshal.PtrToStructure(structurePointer, typeof(IndexEntry));
                Marshal.FreeHGlobal(structurePointer);
                DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                          "Block type {0} with data type {1} is indexed to be at {2}", entry.blockType,
                                          entry.dataType, entry.offset);
                vrIndex.Add(entry);
            }

            const int VERIFY_SIZE = 1024 * 1024;

            foreach(IndexEntry entry in vrIndex)
            {
                imageStream.Position = (long)entry.offset;
                Crc64Context crcVerify;
                ulong        readBytes;
                byte[]       verifyBytes;

                switch(entry.blockType)
                {
                    case BlockType.DataBlock:
                        BlockHeader blockHeader = new BlockHeader();
                        structureBytes          = new byte[Marshal.SizeOf(blockHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(blockHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(blockHeader));
                        blockHeader = (BlockHeader)Marshal.PtrToStructure(structurePointer, typeof(BlockHeader));
                        Marshal.FreeHGlobal(structurePointer);

                        crcVerify = new Crc64Context();
                        crcVerify.Init();
                        readBytes = 0;

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Verifying data block type {0} at position {1}", entry.dataType,
                                                  entry.offset);

                        while(readBytes + VERIFY_SIZE < blockHeader.cmpLength)
                        {
                            verifyBytes = new byte[readBytes];
                            imageStream.Read(verifyBytes, 0, verifyBytes.Length);
                            crcVerify.Update(verifyBytes);
                            readBytes += (ulong)verifyBytes.LongLength;
                        }

                        verifyBytes = new byte[blockHeader.cmpLength - readBytes];
                        imageStream.Read(verifyBytes, 0, verifyBytes.Length);
                        crcVerify.Update(verifyBytes);
                        readBytes += (ulong)verifyBytes.LongLength;
                        System.Console.WriteLine("Read {0} bytes of {1}", readBytes, blockHeader.cmpLength);

                        verifyCrc = crcVerify.Final();

                        if(BitConverter.ToUInt64(verifyCrc, 0) != blockHeader.cmpCrc64)
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Expected block CRC {0:X16} but got {1:X16}",
                                                      blockHeader.cmpCrc64, BitConverter.ToUInt64(verifyCrc, 0));

                        break;
                    case BlockType.DeDuplicationTable:
                        DdtHeader ddtHeader = new DdtHeader();
                        structureBytes      = new byte[Marshal.SizeOf(ddtHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(ddtHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(ddtHeader));
                        ddtHeader = (DdtHeader)Marshal.PtrToStructure(structurePointer, typeof(DdtHeader));
                        Marshal.FreeHGlobal(structurePointer);

                        crcVerify = new Crc64Context();
                        crcVerify.Init();
                        readBytes = 0;

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Verifying deduplication table type {0} at position {1}",
                                                  entry.dataType, entry.offset);

                        while(readBytes + VERIFY_SIZE < ddtHeader.cmpLength)
                        {
                            verifyBytes = new byte[readBytes];
                            imageStream.Read(verifyBytes, 0, verifyBytes.Length);
                            crcVerify.Update(verifyBytes);
                            readBytes += (ulong)verifyBytes.LongLength;
                        }

                        verifyBytes = new byte[ddtHeader.cmpLength - readBytes];
                        imageStream.Read(verifyBytes, 0, verifyBytes.Length);
                        crcVerify.Update(verifyBytes);

                        verifyCrc = crcVerify.Final();

                        if(BitConverter.ToUInt64(verifyCrc, 0) != ddtHeader.cmpCrc64)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Expected block CRC {0:X16} but got {1:X16}", ddtHeader.cmpCrc64,
                                                      BitConverter.ToUInt64(verifyCrc, 0));
                            return false;
                        }

                        break;
                    case BlockType.TracksBlock:
                        TracksHeader trkHeader = new TracksHeader();
                        structureBytes         = new byte[Marshal.SizeOf(trkHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(trkHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(trkHeader));
                        trkHeader = (TracksHeader)Marshal.PtrToStructure(structurePointer, typeof(IndexHeader));
                        Marshal.FreeHGlobal(structurePointer);

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Track block at {0} contains {1} entries", header.indexOffset,
                                                  trkHeader.entries);

                        structureBytes = new byte[Marshal.SizeOf(typeof(TrackEntry)) * trkHeader.entries];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        Crc64Context.Data(structureBytes, out verifyCrc);

                        if(BitConverter.ToUInt64(verifyCrc, 0) != trkHeader.crc64)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Expected index CRC {0:X16} but got {1:X16}", trkHeader.crc64,
                                                      BitConverter.ToUInt64(verifyCrc, 0));
                            return false;
                        }

                        break;
                    default:
                        DicConsole.DebugWriteLine("DiscImageChef format plugin", "Ignored field type {0}",
                                                  entry.blockType);
                        break;
                }
            }

            return true;
        }

        public IEnumerable<MediaTagType> SupportedMediaTags =>
            Enum.GetValues(typeof(MediaTagType)).Cast<MediaTagType>();
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            Enum.GetValues(typeof(SectorTagType)).Cast<SectorTagType>();
        public IEnumerable<MediaType> SupportedMediaTypes =>
            Enum.GetValues(typeof(MediaType)).Cast<MediaType>();
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new[]
            {
                ("sectors_per_block", typeof(uint),
                "How many sectors to store per block (will be rounded to next power of two)"),
                ("dictionary", typeof(uint), "Size, in bytes, of the LZMA dictionary"),
                ("max_ddt_size", typeof(uint),
                "Maximum size, in mebibytes, for in-memory DDT. If image needs a bigger one, it will be on-disk")
            };
        public IEnumerable<string> KnownExtensions => new[] {".dicf"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        // TODO: Support resume
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            uint sectorsPerBlock;
            uint dictionary;
            uint maxDdtSize;

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
                else sectorsPerBlock = 4096;

                if(options.TryGetValue("dictionary", out tmpValue))
                {
                    if(!uint.TryParse(tmpValue, out dictionary))
                    {
                        ErrorMessage = "Invalid value for dictionary option";
                        return false;
                    }
                }
                else dictionary = 1 << 25;

                if(options.TryGetValue("dictionary", out tmpValue))
                {
                    if(!uint.TryParse(tmpValue, out maxDdtSize))
                    {
                        ErrorMessage = "Invalid value for max_ddt_size option";
                        return false;
                    }
                }
                else maxDdtSize = 256;
            }
            else
            {
                sectorsPerBlock = 4096;
                dictionary      = 1 << 25;
                maxDdtSize      = 256;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            shift                   = 0;
            uint oldSectorsPerBlock = sectorsPerBlock;
            while(sectorsPerBlock > 1)
            {
                sectorsPerBlock >>= 1;
                shift++;
            }

            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Got a shift of {0} for {1} sectors per block",
                                      shift, oldSectorsPerBlock);

            imageInfo = new ImageInfo
            {
                MediaType    = mediaType,
                SectorSize   = sectorSize,
                Sectors      = sectors,
                XmlMediaType = GetXmlMediaType(mediaType)
            };

            try { imageStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            index = new List<IndexEntry>();

            // TODO: Set correct version
            header = new DicHeader
            {
                identifier              = DIC_MAGIC,
                application             = "DiscImageChef",
                imageMajorVersion       = DICF_VERSION,
                imageMinorVersion       = 0,
                applicationMajorVersion = 4,
                applicationMinorVersion = 0,
                mediaType               = mediaType,
                creationTime            = DateTime.UtcNow.ToFileTimeUtc()
            };

            inMemoryDdt = sectors <= maxDdtSize * 1024 * 1024 / sizeof(ulong);

            DicConsole.DebugWriteLine("DiscImageChef format plugin", "In memory DDT?: {0}", inMemoryDdt);

            imageStream.Write(new byte[Marshal.SizeOf(typeof(DicHeader))], 0, Marshal.SizeOf(typeof(DicHeader)));

            if(inMemoryDdt) userDataDdt = new ulong[sectors];
            else
            {
                outMemoryDdtPosition = imageStream.Position;
                index.Add(new IndexEntry
                {
                    blockType = BlockType.DeDuplicationTable,
                    dataType  = DataType.UserData,
                    offset    = (ulong)outMemoryDdtPosition
                });

                // CRC64 will be calculated later
                DdtHeader ddtHeader = new DdtHeader
                {
                    identifier  = BlockType.DeDuplicationTable,
                    type        = DataType.UserData,
                    compression = CompressionType.None,
                    shift       = shift,
                    entries     = sectors,
                    cmpLength   = sectors * sizeof(ulong),
                    length      = sectors * sizeof(ulong)
                };

                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(ddtHeader));
                structureBytes   = new byte[Marshal.SizeOf(ddtHeader)];
                Marshal.StructureToPtr(ddtHeader, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                imageStream.Write(structureBytes, 0, structureBytes.Length);
                structureBytes = null;

                // TODO: Can be changed to a seek?
                imageStream.Write(new byte[sectors * sizeof(ulong)], 0, (int)(sectors * sizeof(ulong)));
            }

            mediaTags          = new Dictionary<MediaTagType, byte[]>();
            checksumProvider   = SHA256.Create();
            deduplicationTable = new Dictionary<byte[], ulong>();
            trackIsrcs         = new Dictionary<byte, string>();
            trackFlags         = new Dictionary<byte, byte>();

            lzmaEncoderProperties = new LzmaEncoderProperties(true, (int)dictionary, 255);

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(mediaTags.ContainsKey(tag)) mediaTags.Remove(tag);

            mediaTags.Add(tag, data);

            ErrorMessage = "";
            return true;
        }

        // TODO: Resume
        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(sectorAddress >= Info.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            byte[] hash = checksumProvider.ComputeHash(data);

            if(deduplicationTable.TryGetValue(hash, out ulong pointer))
            {
                SetDdtEntry(sectorAddress, pointer);
                ErrorMessage = "";
                return true;
            }

            // Close current block first
            if(blockStream                    != null &&
               (currentBlockHeader.sectorSize != data.Length || currentBlockOffset == 1 << shift))
            {
                currentBlockHeader.length = currentBlockOffset * currentBlockHeader.sectorSize;
                currentBlockHeader.crc64  = BitConverter.ToUInt64(crc64.Final(), 0);
                byte[] lzmaProperties     = compressedBlockStream.Properties;
                compressedBlockStream.Close();
                currentBlockHeader.cmpLength = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                Crc64Context cmpCrc64Context = new Crc64Context();
                cmpCrc64Context.Init();
                cmpCrc64Context.Update(lzmaProperties);
                cmpCrc64Context.Update(blockStream.ToArray());
                currentBlockHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                index.Add(new IndexEntry
                {
                    blockType = BlockType.DataBlock,
                    dataType  = DataType.UserData,
                    offset    = (ulong)imageStream.Position
                });

                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(currentBlockHeader));
                structureBytes   = new byte[Marshal.SizeOf(currentBlockHeader)];
                Marshal.StructureToPtr(currentBlockHeader, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                imageStream.Write(structureBytes, 0, structureBytes.Length);
                structureBytes = null;
                imageStream.Write(lzmaProperties,        0, lzmaProperties.Length);
                imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                blockStream        = null;
                currentBlockOffset = 0;
            }

            // No block set
            if(blockStream == null)
            {
                blockStream           = new MemoryStream();
                compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                currentBlockHeader    = new BlockHeader
                {
                    identifier  = BlockType.DataBlock,
                    type        = DataType.UserData,
                    compression = CompressionType.Lzma,
                    sectorSize  = (uint)data.Length
                };
                crc64 = new Crc64Context();
                crc64.Init();
            }

            ulong ddtEntry = (ulong)((imageStream.Position << shift) + currentBlockOffset);
            deduplicationTable.Add(hash, ddtEntry);
            compressedBlockStream.Write(data, 0, data.Length);
            SetDdtEntry(sectorAddress, ddtEntry);
            crc64.Update(data);
            currentBlockOffset++;

            ErrorMessage = "";
            return true;
        }

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
                if(!WriteSector(tmp, sectorAddress + i)) return false;
            }

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            byte[] sector;

            switch(imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track track =
                        Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

                    if(track.TrackSequence == 0)
                    {
                        ErrorMessage = $"Can't found track containing {sectorAddress}";
                        return false;
                    }

                    if(data.Length != 2352)
                    {
                        ErrorMessage = "Incorrect data size";
                        return false;
                    }

                    switch(track.TrackType)
                    {
                        case TrackType.Audio:
                        case TrackType.Data: return WriteSector(data, sectorAddress);
                        case TrackType.CdMode1:
                            if(sectorPrefix == null) sectorPrefix = new byte[imageInfo.Sectors * 16];
                            if(sectorSuffix == null) sectorSuffix = new byte[imageInfo.Sectors * 288];
                            sector                                = new byte[2048];
                            Array.Copy(data, 0,    sectorPrefix, (int)sectorAddress * 16,  16);
                            Array.Copy(data, 16,   sector,       0,                        2048);
                            Array.Copy(data, 2064, sectorSuffix, (int)sectorAddress * 288, 288);
                            return WriteSector(sector, sectorAddress);
                        case TrackType.CdMode2Formless:
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                            if(sectorPrefix == null) sectorPrefix = new byte[imageInfo.Sectors * 16];
                            if(sectorSuffix == null) sectorSuffix = new byte[imageInfo.Sectors * 288];
                            sector                                = new byte[2336];
                            Array.Copy(data, 0,  sectorPrefix, (int)sectorAddress * 16, 16);
                            Array.Copy(data, 16, sector,       0,                       2336);
                            return WriteSector(sector, sectorAddress);
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    switch(imageInfo.MediaType)
                    {
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
                                case 12 when imageInfo.MediaType == MediaType.AppleProfile ||
                                             imageInfo.MediaType == MediaType.AppleFileWare:
                                    oldTag = new byte[12];
                                    Array.Copy(data, 512, oldTag, 0, 12);
                                    newTag = LisaTag.DecodeSonyTag(oldTag)?.ToProfile().GetBytes();
                                    break;
                                // Sony tag, convert to Priam
                                case 12 when imageInfo.MediaType == MediaType.PriamDataTower:
                                    oldTag = new byte[12];
                                    Array.Copy(data, 512, oldTag, 0, 12);
                                    newTag = LisaTag.DecodeSonyTag(oldTag)?.ToPriam().GetBytes();
                                    break;
                                // Sony tag, copy to Sony
                                case 12 when imageInfo.MediaType == MediaType.AppleSonySS ||
                                             imageInfo.MediaType == MediaType.AppleSonySS:
                                    newTag = new byte[12];
                                    Array.Copy(data, 512, newTag, 0, 12);
                                    break;
                                // Profile tag, copy to Profile
                                case 20 when imageInfo.MediaType == MediaType.AppleProfile ||
                                             imageInfo.MediaType == MediaType.AppleFileWare:
                                    newTag = new byte[20];
                                    Array.Copy(data, 512, newTag, 0, 20);
                                    break;
                                // Profile tag, convert to Priam
                                case 20 when imageInfo.MediaType == MediaType.PriamDataTower:
                                    oldTag = new byte[20];
                                    Array.Copy(data, 512, oldTag, 0, 20);
                                    newTag = LisaTag.DecodeProfileTag(oldTag)?.ToPriam().GetBytes();
                                    break;
                                // Profile tag, convert to Sony
                                case 20 when imageInfo.MediaType == MediaType.AppleSonySS ||
                                             imageInfo.MediaType == MediaType.AppleSonySS:
                                    oldTag = new byte[20];
                                    Array.Copy(data, 512, oldTag, 0, 20);
                                    newTag = LisaTag.DecodeProfileTag(oldTag)?.ToSony().GetBytes();
                                    break;
                                // Priam tag, convert to Profile
                                case 24 when imageInfo.MediaType == MediaType.AppleProfile ||
                                             imageInfo.MediaType == MediaType.AppleFileWare:
                                    oldTag = new byte[24];
                                    Array.Copy(data, 512, oldTag, 0, 24);
                                    newTag = LisaTag.DecodePriamTag(oldTag)?.ToProfile().GetBytes();
                                    break;
                                // Priam tag, copy to Priam
                                case 12 when imageInfo.MediaType == MediaType.PriamDataTower:
                                    newTag = new byte[24];
                                    Array.Copy(data, 512, newTag, 0, 24);
                                    break;
                                // Priam tag, convert to Sony
                                case 24 when imageInfo.MediaType == MediaType.AppleSonySS ||
                                             imageInfo.MediaType == MediaType.AppleSonySS:
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

                            if(newTag == null) return WriteSector(sector, sectorAddress);

                            if(sectorSubchannel == null)
                                sectorSubchannel = new byte[newTag.Length         * (int)imageInfo.Sectors];
                            Array.Copy(newTag, 0, sectorSubchannel, newTag.Length * (int)sectorAddress, newTag.Length);

                            return WriteSector(sector, sectorAddress);
                    }

                    break;
            }

            ErrorMessage = "Unknown long sector type, cannot write.";
            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            byte[] sector;
            switch(imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    Track track =
                        Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

                    if(track.TrackSequence == 0)
                    {
                        ErrorMessage = $"Can't found track containing {sectorAddress}";
                        return false;
                    }

                    if(data.Length % 2352 != 0)
                    {
                        ErrorMessage = "Incorrect data size";
                        return false;
                    }

                    if(track.TrackStartSector + sectorAddress + length > track.TrackEndSector + 1)
                        throw new ArgumentOutOfRangeException(nameof(length),
                                                              $"Requested more sectors ({length + sectorAddress}) than present in track ({track.TrackEndSector - track.TrackStartSector + 1}), won't cross tracks");

                    sector = new byte[2352];
                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(data, 2352 * i, sector, 0, 2352);
                        if(!WriteSectorLong(sector, sectorAddress + i)) return false;
                    }

                    ErrorMessage = "";
                    return true;
                case XmlMediaType.BlockMedia:
                    switch(imageInfo.MediaType)
                    {
                        case MediaType.AppleFileWare:
                        case MediaType.AppleProfile:
                        case MediaType.AppleSonyDS:
                        case MediaType.AppleSonySS:
                        case MediaType.AppleWidget:
                        case MediaType.PriamDataTower:
                            int sectorSize                             = 0;
                            if(data.Length      % 524 == 0) sectorSize = 524;
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
                                if(!WriteSectorLong(sector, sectorAddress + i)) return false;
                            }

                            ErrorMessage = "";
                            return true;
                    }

                    break;
            }

            ErrorMessage = "Unknown long sector type, cannot write.";
            return false;
        }

        public bool SetTracks(List<Track> tracks)
        {
            if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
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

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            // Close current block first
            if(blockStream != null)
            {
                currentBlockHeader.length = currentBlockOffset * currentBlockHeader.sectorSize;
                currentBlockHeader.crc64  = BitConverter.ToUInt64(crc64.Final(), 0);
                byte[] lzmaProperties     = compressedBlockStream.Properties;
                compressedBlockStream.Close();
                currentBlockHeader.cmpLength = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                Crc64Context cmpCrc64Context = new Crc64Context();
                cmpCrc64Context.Init();
                cmpCrc64Context.Update(lzmaProperties);
                cmpCrc64Context.Update(blockStream.ToArray());
                currentBlockHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                index.Add(new IndexEntry
                {
                    blockType = BlockType.DataBlock,
                    dataType  = DataType.UserData,
                    offset    = (ulong)imageStream.Position
                });

                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(currentBlockHeader));
                structureBytes   = new byte[Marshal.SizeOf(currentBlockHeader)];
                Marshal.StructureToPtr(currentBlockHeader, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                imageStream.Write(structureBytes, 0, structureBytes.Length);
                structureBytes = null;
                imageStream.Write(lzmaProperties,        0, lzmaProperties.Length);
                imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                blockStream = null;
            }

            IndexEntry idxEntry;

            foreach(KeyValuePair<MediaTagType, byte[]> mediaTag in mediaTags)
            {
                DataType dataType = GetDataTypeForMediaTag(mediaTag.Key);
                idxEntry          = new IndexEntry
                {
                    blockType = BlockType.DataBlock,
                    dataType  = dataType,
                    offset    = (ulong)imageStream.Position
                };

                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing tag type {0} to position {1}",
                                          mediaTag.Key, idxEntry.offset);

                Crc64Context.Data(mediaTag.Value, out byte[] tagCrc);

                BlockHeader tagBlock = new BlockHeader
                {
                    identifier = BlockType.DataBlock,
                    type       = dataType,
                    length     = (uint)mediaTag.Value.Length,
                    crc64      = BitConverter.ToUInt64(tagCrc, 0)
                };

                blockStream           = new MemoryStream();
                compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                compressedBlockStream.Write(mediaTag.Value, 0, mediaTag.Value.Length);
                byte[] lzmaProperties = compressedBlockStream.Properties;
                compressedBlockStream.Close();
                byte[] tagData;

                // Not compressible
                if(blockStream.Length + LZMA_PROPERTIES_LENGTH >= mediaTag.Value.Length)
                {
                    tagBlock.cmpLength   = tagBlock.length;
                    tagBlock.cmpCrc64    = tagBlock.crc64;
                    tagData              = mediaTag.Value;
                    tagBlock.compression = CompressionType.None;
                }
                else
                {
                    tagData = blockStream.ToArray();
                    Crc64Context.Data(tagData, out tagCrc);
                    tagBlock.cmpLength   = (uint)tagData.Length + LZMA_PROPERTIES_LENGTH;
                    tagBlock.cmpCrc64    = BitConverter.ToUInt64(tagCrc, 0);
                    tagBlock.compression = CompressionType.Lzma;
                }

                compressedBlockStream = null;
                blockStream           = null;

                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(tagBlock));
                structureBytes   = new byte[Marshal.SizeOf(tagBlock)];
                Marshal.StructureToPtr(tagBlock, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                imageStream.Write(structureBytes, 0, structureBytes.Length);
                if(tagBlock.compression == CompressionType.Lzma)
                    imageStream.Write(lzmaProperties, 0, lzmaProperties.Length);
                imageStream.Write(tagData,            0, tagData.Length);

                index.Add(idxEntry);
            }

            if(geometryBlock.identifier == BlockType.GeometryBlock)
            {
                idxEntry = new IndexEntry
                {
                    blockType = BlockType.GeometryBlock,
                    dataType  = DataType.NoData,
                    offset    = (ulong)imageStream.Position
                };

                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing geometry block to position {0}",
                                          idxEntry.offset);

                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(geometryBlock));
                structureBytes   = new byte[Marshal.SizeOf(geometryBlock)];
                Marshal.StructureToPtr(geometryBlock, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                imageStream.Write(structureBytes, 0, structureBytes.Length);

                index.Add(idxEntry);
            }

            if(inMemoryDdt)
            {
                idxEntry = new IndexEntry
                {
                    blockType = BlockType.DeDuplicationTable,
                    dataType  = DataType.UserData,
                    offset    = (ulong)imageStream.Position
                };

                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing user data DDT to position {0}",
                                          idxEntry.offset);

                DdtHeader ddtHeader = new DdtHeader
                {
                    identifier  = BlockType.DeDuplicationTable,
                    type        = DataType.UserData,
                    compression = CompressionType.Lzma,
                    shift       = shift,
                    entries     = (ulong)userDataDdt.LongLength,
                    length      = (ulong)(userDataDdt.LongLength * sizeof(ulong))
                };

                blockStream           = new MemoryStream();
                compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                crc64                 = new Crc64Context();
                crc64.Init();
                for(ulong i = 0; i < (ulong)userDataDdt.LongLength; i++)
                {
                    byte[] ddtEntry = BitConverter.GetBytes(userDataDdt[i]);
                    crc64.Update(ddtEntry);
                    compressedBlockStream.Write(ddtEntry, 0, ddtEntry.Length);
                }

                byte[] lzmaProperties = compressedBlockStream.Properties;
                compressedBlockStream.Close();
                ddtHeader.cmpLength          = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                Crc64Context cmpCrc64Context = new Crc64Context();
                cmpCrc64Context.Init();
                cmpCrc64Context.Update(lzmaProperties);
                cmpCrc64Context.Update(blockStream.ToArray());
                ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64Context.Final(), 0);

                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(ddtHeader));
                structureBytes   = new byte[Marshal.SizeOf(ddtHeader)];
                Marshal.StructureToPtr(ddtHeader, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                imageStream.Write(structureBytes, 0, structureBytes.Length);
                structureBytes = null;
                imageStream.Write(lzmaProperties,        0, lzmaProperties.Length);
                imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                blockStream           = null;
                compressedBlockStream = null;

                index.Add(idxEntry);
            }

            switch(imageInfo.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc when Tracks != null && Tracks.Count > 0:
                    if(sectorPrefix                       != null && sectorSuffix != null)
                    {
                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorPrefix,
                            offset    = (ulong)imageStream.Position
                        };

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Writing CD sector prefix block to position {0}", idxEntry.offset);

                        Crc64Context.Data(sectorPrefix, out byte[] blockCrc);

                        BlockHeader prefixBlock = new BlockHeader
                        {
                            identifier = BlockType.DataBlock,
                            type       = DataType.CdSectorPrefix,
                            length     = (uint)sectorPrefix.Length,
                            crc64      = BitConverter.ToUInt64(blockCrc, 0)
                        };

                        blockStream           = new MemoryStream();
                        compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                        compressedBlockStream.Write(sectorPrefix, 0, sectorPrefix.Length);
                        byte[] lzmaProperties = compressedBlockStream.Properties;
                        compressedBlockStream.Close();

                        Crc64Context.Data(blockStream.ToArray(), out blockCrc);
                        prefixBlock.cmpLength   = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                        prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        prefixBlock.compression = CompressionType.Lzma;

                        compressedBlockStream = null;

                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(prefixBlock));
                        structureBytes   = new byte[Marshal.SizeOf(prefixBlock)];
                        Marshal.StructureToPtr(prefixBlock, structurePointer, true);
                        Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                        Marshal.FreeHGlobal(structurePointer);
                        imageStream.Write(structureBytes, 0, structureBytes.Length);
                        if(prefixBlock.compression == CompressionType.Lzma)
                            imageStream.Write(lzmaProperties,    0, lzmaProperties.Length);
                        imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                        index.Add(idxEntry);

                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorSuffix,
                            offset    = (ulong)imageStream.Position
                        };

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Writing CD sector suffix block to position {0}", idxEntry.offset);

                        Crc64Context.Data(sectorSuffix, out blockCrc);

                        prefixBlock = new BlockHeader
                        {
                            identifier = BlockType.DataBlock,
                            type       = DataType.CdSectorSuffix,
                            length     = (uint)sectorSuffix.Length,
                            crc64      = BitConverter.ToUInt64(blockCrc, 0)
                        };

                        blockStream           = new MemoryStream();
                        compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                        compressedBlockStream.Write(sectorSuffix, 0, sectorSuffix.Length);
                        lzmaProperties = compressedBlockStream.Properties;
                        compressedBlockStream.Close();

                        Crc64Context.Data(blockStream.ToArray(), out blockCrc);
                        prefixBlock.cmpLength   = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                        prefixBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        prefixBlock.compression = CompressionType.Lzma;

                        compressedBlockStream = null;

                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(prefixBlock));
                        structureBytes   = new byte[Marshal.SizeOf(prefixBlock)];
                        Marshal.StructureToPtr(prefixBlock, structurePointer, true);
                        Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                        Marshal.FreeHGlobal(structurePointer);
                        imageStream.Write(structureBytes, 0, structureBytes.Length);
                        if(prefixBlock.compression == CompressionType.Lzma)
                            imageStream.Write(lzmaProperties,    0, lzmaProperties.Length);
                        imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                        index.Add(idxEntry);
                        blockStream = null;
                    }

                    if(sectorSubchannel != null)
                    {
                        idxEntry = new IndexEntry
                        {
                            blockType = BlockType.DataBlock,
                            dataType  = DataType.CdSectorSubchannel,
                            offset    = (ulong)imageStream.Position
                        };

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Writing CD subchannel block to position {0}", idxEntry.offset);

                        Crc64Context.Data(sectorSubchannel, out byte[] blockCrc);

                        BlockHeader subchannelBlock = new BlockHeader
                        {
                            identifier = BlockType.DataBlock,
                            type       = DataType.CdSectorSubchannel,
                            length     = (uint)sectorSubchannel.Length,
                            crc64      = BitConverter.ToUInt64(blockCrc, 0)
                        };

                        blockStream           = new MemoryStream();
                        compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                        compressedBlockStream.Write(sectorSubchannel, 0, sectorSubchannel.Length);
                        byte[] lzmaProperties = compressedBlockStream.Properties;
                        compressedBlockStream.Close();

                        Crc64Context.Data(blockStream.ToArray(), out blockCrc);
                        subchannelBlock.cmpLength   = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                        subchannelBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        subchannelBlock.compression = CompressionType.Lzma;

                        compressedBlockStream = null;

                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(subchannelBlock));
                        structureBytes   = new byte[Marshal.SizeOf(subchannelBlock)];
                        Marshal.StructureToPtr(subchannelBlock, structurePointer, true);
                        Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                        Marshal.FreeHGlobal(structurePointer);
                        imageStream.Write(structureBytes, 0, structureBytes.Length);
                        if(subchannelBlock.compression == CompressionType.Lzma)
                            imageStream.Write(lzmaProperties,    0, lzmaProperties.Length);
                        imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                        index.Add(idxEntry);
                        blockStream = null;
                    }

                    List<TrackEntry> trackEntries = new List<TrackEntry>();
                    foreach(Track track in Tracks)
                    {
                        trackFlags.TryGetValue((byte)track.TrackSequence, out byte flags);
                        trackIsrcs.TryGetValue((byte)track.TrackSequence, out string isrc);

                        if((flags & (int)CdFlags.DataTrack) == 0 && track.TrackType != TrackType.Audio)
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
                    }

                    if(trackEntries.Count > 0)
                    {
                        blockStream = new MemoryStream();

                        foreach(TrackEntry entry in trackEntries)
                        {
                            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(entry));
                            structureBytes   = new byte[Marshal.SizeOf(entry)];
                            Marshal.StructureToPtr(entry, structurePointer, true);
                            Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                            Marshal.FreeHGlobal(structurePointer);
                            blockStream.Write(structureBytes, 0, structureBytes.Length);
                        }

                        Crc64Context.Data(blockStream.ToArray(), out byte[] trksCrc);
                        TracksHeader trkHeader = new TracksHeader
                        {
                            identifier = BlockType.TracksBlock,
                            entries    = (ushort)trackEntries.Count,
                            crc64      = BitConverter.ToUInt64(trksCrc, 0)
                        };

                        DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing tracks to position {0}",
                                                  imageStream.Position);

                        index.Add(new IndexEntry
                        {
                            blockType = BlockType.TracksBlock,
                            dataType  = DataType.NoData,
                            offset    = (ulong)imageStream.Position
                        });

                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(trkHeader));
                        structureBytes   = new byte[Marshal.SizeOf(trkHeader)];
                        Marshal.StructureToPtr(trkHeader, structurePointer, true);
                        Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                        Marshal.FreeHGlobal(structurePointer);
                        imageStream.Write(structureBytes,        0, structureBytes.Length);
                        imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    if(sectorSubchannel     != null &&
                       (imageInfo.MediaType == MediaType.AppleFileWare ||
                        imageInfo.MediaType == MediaType.AppleSonySS   ||
                        imageInfo.MediaType == MediaType.AppleSonyDS   ||
                        imageInfo.MediaType == MediaType.AppleProfile  ||
                        imageInfo.MediaType == MediaType.AppleWidget   ||
                        imageInfo.MediaType == MediaType.PriamDataTower))
                    {
                        DataType tagType = DataType.NoData;

                        switch(imageInfo.MediaType)
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
                            offset    = (ulong)imageStream.Position
                        };

                        DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                  "Writing apple sector tag block to position {0}", idxEntry.offset);

                        Crc64Context.Data(sectorSubchannel, out byte[] blockCrc);

                        BlockHeader subchannelBlock = new BlockHeader
                        {
                            identifier = BlockType.DataBlock,
                            type       = tagType,
                            length     = (uint)sectorSubchannel.Length,
                            crc64      = BitConverter.ToUInt64(blockCrc, 0)
                        };

                        blockStream           = new MemoryStream();
                        compressedBlockStream = new LzmaStream(lzmaEncoderProperties, false, blockStream);
                        compressedBlockStream.Write(sectorSubchannel, 0, sectorSubchannel.Length);
                        byte[] lzmaProperties = compressedBlockStream.Properties;
                        compressedBlockStream.Close();

                        Crc64Context.Data(blockStream.ToArray(), out blockCrc);
                        subchannelBlock.cmpLength   = (uint)blockStream.Length + LZMA_PROPERTIES_LENGTH;
                        subchannelBlock.cmpCrc64    = BitConverter.ToUInt64(blockCrc, 0);
                        subchannelBlock.compression = CompressionType.Lzma;

                        compressedBlockStream = null;

                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(subchannelBlock));
                        structureBytes   = new byte[Marshal.SizeOf(subchannelBlock)];
                        Marshal.StructureToPtr(subchannelBlock, structurePointer, true);
                        Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                        Marshal.FreeHGlobal(structurePointer);
                        imageStream.Write(structureBytes, 0, structureBytes.Length);
                        if(subchannelBlock.compression == CompressionType.Lzma)
                            imageStream.Write(lzmaProperties,    0, lzmaProperties.Length);
                        imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

                        index.Add(idxEntry);
                        blockStream = null;
                    }

                    break;
            }

            MetadataBlock metadataBlock = new MetadataBlock();
            blockStream                 = new MemoryStream();
            blockStream.Write(new byte[Marshal.SizeOf(metadataBlock)], 0, Marshal.SizeOf(metadataBlock));
            byte[] tmpUtf16Le;

            if(imageInfo.MediaSequence > 0 && imageInfo.LastMediaSequence > 0)
            {
                metadataBlock.identifier        = BlockType.MetadataBlock;
                metadataBlock.mediaSequence     = imageInfo.MediaSequence;
                metadataBlock.lastMediaSequence = imageInfo.LastMediaSequence;
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.Creator))
            {
                tmpUtf16Le                  = Encoding.Unicode.GetBytes(imageInfo.Creator);
                metadataBlock.identifier    = BlockType.MetadataBlock;
                metadataBlock.creatorOffset = (uint)blockStream.Position;
                metadataBlock.creatorLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.Comments))
            {
                tmpUtf16Le                   = Encoding.Unicode.GetBytes(imageInfo.Comments);
                metadataBlock.identifier     = BlockType.MetadataBlock;
                metadataBlock.commentsOffset = (uint)blockStream.Position;
                metadataBlock.commentsLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.MediaTitle))
            {
                tmpUtf16Le                     = Encoding.Unicode.GetBytes(imageInfo.MediaTitle);
                metadataBlock.identifier       = BlockType.MetadataBlock;
                metadataBlock.mediaTitleOffset = (uint)blockStream.Position;
                metadataBlock.mediaTitleLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.MediaManufacturer))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(imageInfo.MediaManufacturer);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.mediaManufacturerOffset = (uint)blockStream.Position;
                metadataBlock.mediaManufacturerLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.MediaModel))
            {
                tmpUtf16Le                     = Encoding.Unicode.GetBytes(imageInfo.MediaModel);
                metadataBlock.identifier       = BlockType.MetadataBlock;
                metadataBlock.mediaModelOffset = (uint)blockStream.Position;
                metadataBlock.mediaModelLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.MediaSerialNumber))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(imageInfo.MediaSerialNumber);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.mediaSerialNumberOffset = (uint)blockStream.Position;
                metadataBlock.mediaSerialNumberLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.MediaBarcode))
            {
                tmpUtf16Le                       = Encoding.Unicode.GetBytes(imageInfo.MediaBarcode);
                metadataBlock.identifier         = BlockType.MetadataBlock;
                metadataBlock.mediaBarcodeOffset = (uint)blockStream.Position;
                metadataBlock.mediaBarcodeLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.MediaPartNumber))
            {
                tmpUtf16Le                          = Encoding.Unicode.GetBytes(imageInfo.MediaPartNumber);
                metadataBlock.identifier            = BlockType.MetadataBlock;
                metadataBlock.mediaPartNumberOffset = (uint)blockStream.Position;
                metadataBlock.mediaPartNumberLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.DriveManufacturer))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(imageInfo.DriveManufacturer);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.driveManufacturerOffset = (uint)blockStream.Position;
                metadataBlock.driveManufacturerLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.DriveModel))
            {
                tmpUtf16Le                     = Encoding.Unicode.GetBytes(imageInfo.DriveModel);
                metadataBlock.identifier       = BlockType.MetadataBlock;
                metadataBlock.driveModelOffset = (uint)blockStream.Position;
                metadataBlock.driveModelLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.DriveSerialNumber))
            {
                tmpUtf16Le                            = Encoding.Unicode.GetBytes(imageInfo.DriveSerialNumber);
                metadataBlock.identifier              = BlockType.MetadataBlock;
                metadataBlock.driveSerialNumberOffset = (uint)blockStream.Position;
                metadataBlock.driveSerialNumberLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(!string.IsNullOrWhiteSpace(imageInfo.DriveFirmwareRevision))
            {
                tmpUtf16Le                                = Encoding.Unicode.GetBytes(imageInfo.DriveFirmwareRevision);
                metadataBlock.identifier                  = BlockType.MetadataBlock;
                metadataBlock.driveFirmwareRevisionOffset = (uint)blockStream.Position;
                metadataBlock.driveFirmwareRevisionLength = (uint)(tmpUtf16Le.Length + 2);
                blockStream.Write(tmpUtf16Le,        0, tmpUtf16Le.Length);
                blockStream.Write(new byte[] {0, 0}, 0, 2);
            }

            if(metadataBlock.identifier == BlockType.MetadataBlock)
            {
                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing metadata to position {0}",
                                          imageStream.Position);
                metadataBlock.blockSize = (uint)blockStream.Length;
                structurePointer        = Marshal.AllocHGlobal(Marshal.SizeOf(metadataBlock));
                structureBytes          = new byte[Marshal.SizeOf(metadataBlock)];
                Marshal.StructureToPtr(metadataBlock, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                blockStream.Position = 0;
                blockStream.Write(structureBytes, 0, structureBytes.Length);
                index.Add(new IndexEntry
                {
                    blockType = BlockType.MetadataBlock,
                    dataType  = DataType.NoData,
                    offset    = (ulong)imageStream.Position
                });
                imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);
            }

            header.indexOffset = (ulong)imageStream.Position;
            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing index to position {0}",
                                      header.indexOffset);

            blockStream = new MemoryStream();

            foreach(IndexEntry entry in index)
            {
                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(entry));
                structureBytes   = new byte[Marshal.SizeOf(entry)];
                Marshal.StructureToPtr(entry, structurePointer, true);
                Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
                Marshal.FreeHGlobal(structurePointer);
                blockStream.Write(structureBytes, 0, structureBytes.Length);
            }

            Crc64Context.Data(blockStream.ToArray(), out byte[] idxCrc);

            IndexHeader idxHeader = new IndexHeader
            {
                identifier = BlockType.Index,
                entries    = (ushort)index.Count,
                crc64      = BitConverter.ToUInt64(idxCrc, 0)
            };

            structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(idxHeader));
            structureBytes   = new byte[Marshal.SizeOf(idxHeader)];
            Marshal.StructureToPtr(idxHeader, structurePointer, true);
            Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
            Marshal.FreeHGlobal(structurePointer);
            imageStream.Write(structureBytes,        0, structureBytes.Length);
            imageStream.Write(blockStream.ToArray(), 0, (int)blockStream.Length);

            DicConsole.DebugWriteLine("DiscImageChef format plugin", "Writing header");
            header.lastWrittenTime = DateTime.UtcNow.ToFileTimeUtc();
            imageStream.Position   = 0;
            structurePointer       = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            structureBytes         = new byte[Marshal.SizeOf(header)];
            Marshal.StructureToPtr(header, structurePointer, true);
            Marshal.Copy(structurePointer, structureBytes, 0, structureBytes.Length);
            Marshal.FreeHGlobal(structurePointer);
            imageStream.Write(structureBytes, 0, structureBytes.Length);

            imageStream.Flush();
            imageStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Creator               = metadata.Creator;
            imageInfo.Comments              = metadata.Comments;
            imageInfo.MediaManufacturer     = metadata.MediaManufacturer;
            imageInfo.MediaModel            = metadata.MediaModel;
            imageInfo.MediaSerialNumber     = metadata.MediaSerialNumber;
            imageInfo.MediaBarcode          = metadata.MediaBarcode;
            imageInfo.MediaPartNumber       = metadata.MediaPartNumber;
            imageInfo.MediaSequence         = metadata.MediaSequence;
            imageInfo.LastMediaSequence     = metadata.LastMediaSequence;
            imageInfo.DriveManufacturer     = metadata.DriveManufacturer;
            imageInfo.DriveModel            = metadata.DriveModel;
            imageInfo.DriveSerialNumber     = metadata.DriveSerialNumber;
            imageInfo.DriveFirmwareRevision = metadata.DriveFirmwareRevision;

            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(imageInfo.XmlMediaType != XmlMediaType.BlockMedia)
            {
                ErrorMessage = "Tried to set geometry on a media that doesn't suppport it";
                return false;
            }

            geometryBlock = new GeometryBlock
            {
                identifier      = BlockType.GeometryBlock,
                cylinders       = cylinders,
                heads           = heads,
                sectorsPerTrack = sectorsPerTrack
            };

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            Track track = new Track();
            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc:
                case SectorTagType.CdSectorSubchannel:
                    if(imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                    {
                        ErrorMessage = "Incorrect tag for disk type";
                        return false;
                    }

                    track = Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                         sectorAddress <= trk.TrackEndSector);
                    if(track.TrackSequence                             == 0)
                    {
                        ErrorMessage = $"Can't found track containing {sectorAddress}";
                        return false;
                    }

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

                    trackFlags.Add((byte)track.TrackSequence, data[0]);

                    return true;
                }
                case SectorTagType.CdTrackIsrc:
                {
                    if(data != null) trackIsrcs.Add((byte)track.TrackSequence, Encoding.UTF8.GetString(data));
                    return true;
                }
                case SectorTagType.CdSectorSubchannel:
                {
                    if(data.Length != 96)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";
                        return false;
                    }

                    if(sectorSubchannel == null) sectorSubchannel = new byte[imageInfo.Sectors * 96];

                    Array.Copy(data, 0, sectorSubchannel, (int)(96 * sectorAddress), 96);

                    return true;
                }
                default:
                    ErrorMessage = $"Don't know how to write sector tag type {tag}";
                    return false;
            }
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
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

                    if(sectorSubchannel == null) sectorSubchannel = new byte[imageInfo.Sectors * 96];

                    if(sectorAddress * 96 + length * 96 > (ulong)sectorSubchannel.LongLength)
                    {
                        ErrorMessage = "Tried to write more data than possible";
                        return false;
                    }

                    Array.Copy(data, 0, sectorSubchannel, (int)(96 * sectorAddress), 96 * length);

                    return true;
                }

                default:
                    ErrorMessage = $"Don't know how to write sector tag type {tag}";
                    return false;
            }
        }

        static XmlMediaType GetXmlMediaType(MediaType type)
        {
            switch(type)
            {
                case MediaType.CD:
                case MediaType.CDDA:
                case MediaType.CDG:
                case MediaType.CDEG:
                case MediaType.CDI:
                case MediaType.CDROM:
                case MediaType.CDROMXA:
                case MediaType.CDPLUS:
                case MediaType.CDMO:
                case MediaType.CDR:
                case MediaType.CDRW:
                case MediaType.CDMRW:
                case MediaType.VCD:
                case MediaType.SVCD:
                case MediaType.PCD:
                case MediaType.SACD:
                case MediaType.DDCD:
                case MediaType.DDCDR:
                case MediaType.DDCDRW:
                case MediaType.DTSCD:
                case MediaType.CDMIDI:
                case MediaType.CDV:
                case MediaType.DVDROM:
                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.DVDPR:
                case MediaType.DVDPRW:
                case MediaType.DVDPRWDL:
                case MediaType.DVDRDL:
                case MediaType.DVDPRDL:
                case MediaType.DVDRAM:
                case MediaType.DVDRWDL:
                case MediaType.DVDDownload:
                case MediaType.HDDVDROM:
                case MediaType.HDDVDRAM:
                case MediaType.HDDVDR:
                case MediaType.HDDVDRW:
                case MediaType.HDDVDRDL:
                case MediaType.HDDVDRWDL:
                case MediaType.BDROM:
                case MediaType.BDR:
                case MediaType.BDRE:
                case MediaType.BDRXL:
                case MediaType.BDREXL:
                case MediaType.EVD:
                case MediaType.FVD:
                case MediaType.HVD:
                case MediaType.CBHD:
                case MediaType.HDVMD:
                case MediaType.VCDHD:
                case MediaType.SVOD:
                case MediaType.FDDVD:
                case MediaType.LD:
                case MediaType.LDROM:
                case MediaType.LDROM2:
                case MediaType.LVROM:
                case MediaType.MegaLD:
                case MediaType.PS1CD:
                case MediaType.PS2CD:
                case MediaType.PS2DVD:
                case MediaType.PS3DVD:
                case MediaType.PS3BD:
                case MediaType.PS4BD:
                case MediaType.UMD:
                case MediaType.XGD:
                case MediaType.XGD2:
                case MediaType.XGD3:
                case MediaType.XGD4:
                case MediaType.MEGACD:
                case MediaType.SATURNCD:
                case MediaType.GDROM:
                case MediaType.GDR:
                case MediaType.SuperCDROM2:
                case MediaType.JaguarCD:
                case MediaType.ThreeDO:
                case MediaType.GOD:
                case MediaType.WOD:
                case MediaType.WUOD: return XmlMediaType.OpticalDisc;
                default:             return XmlMediaType.BlockMedia;
            }
        }

        ulong GetDdtEntry(ulong sectorAddress)
        {
            if(inMemoryDdt) return userDataDdt[sectorAddress];

            if(ddtEntryCache.TryGetValue(sectorAddress, out ulong entry)) return entry;

            long oldPosition     = imageStream.Position;
            imageStream.Position =  outMemoryDdtPosition + Marshal.SizeOf(typeof(DdtHeader));
            imageStream.Position += (long)(sectorAddress * sizeof(ulong));
            byte[] temp          = new byte[sizeof(ulong)];
            imageStream.Read(temp, 0, sizeof(ulong));
            imageStream.Position = oldPosition;
            entry                = BitConverter.ToUInt64(temp, 0);

            if(ddtEntryCache.Count >= MAX_DDT_ENTRY_CACHE) ddtEntryCache.Clear();

            ddtEntryCache.Add(sectorAddress, entry);
            return entry;
        }

        void SetDdtEntry(ulong sectorAddress, ulong pointer)
        {
            if(inMemoryDdt)
            {
                userDataDdt[sectorAddress] = pointer;
                return;
            }

            long oldPosition     = imageStream.Position;
            imageStream.Position =  outMemoryDdtPosition + Marshal.SizeOf(typeof(DdtHeader));
            imageStream.Position += (long)(sectorAddress * sizeof(ulong));
            imageStream.Write(BitConverter.GetBytes(pointer), 0, sizeof(ulong));
            imageStream.Position = oldPosition;
        }

        static MediaTagType GetMediaTagTypeForDataType(DataType type)
        {
            switch(type)
            {
                case DataType.CompactDiscPartialToc:            return MediaTagType.CD_TOC;
                case DataType.CompactDiscSessionInfo:           return MediaTagType.CD_SessionInfo;
                case DataType.CompactDiscToc:                   return MediaTagType.CD_FullTOC;
                case DataType.CompactDiscPma:                   return MediaTagType.CD_PMA;
                case DataType.CompactDiscAtip:                  return MediaTagType.CD_ATIP;
                case DataType.CompactDiscLeadInCdText:          return MediaTagType.CD_TEXT;
                case DataType.DvdPfi:                           return MediaTagType.DVD_PFI;
                case DataType.DvdLeadInCmi:                     return MediaTagType.DVD_CMI;
                case DataType.DvdDiscKey:                       return MediaTagType.DVD_DiscKey;
                case DataType.DvdBca:                           return MediaTagType.DVD_BCA;
                case DataType.DvdDmi:                           return MediaTagType.DVD_DMI;
                case DataType.DvdMediaIdentifier:               return MediaTagType.DVD_MediaIdentifier;
                case DataType.DvdMediaKeyBlock:                 return MediaTagType.DVD_MKB;
                case DataType.DvdRamDds:                        return MediaTagType.DVDRAM_DDS;
                case DataType.DvdRamMediumStatus:               return MediaTagType.DVDRAM_MediumStatus;
                case DataType.DvdRamSpareArea:                  return MediaTagType.DVDRAM_SpareArea;
                case DataType.DvdRRmd:                          return MediaTagType.DVDR_RMD;
                case DataType.DvdRPrerecordedInfo:              return MediaTagType.DVDR_PreRecordedInfo;
                case DataType.DvdRMediaIdentifier:              return MediaTagType.DVDR_MediaIdentifier;
                case DataType.DvdRPfi:                          return MediaTagType.DVDR_PFI;
                case DataType.DvdAdip:                          return MediaTagType.DVD_ADIP;
                case DataType.HdDvdCpi:                         return MediaTagType.HDDVD_CPI;
                case DataType.HdDvdMediumStatus:                return MediaTagType.HDDVD_MediumStatus;
                case DataType.DvdDlLayerCapacity:               return MediaTagType.DVDDL_LayerCapacity;
                case DataType.DvdDlMiddleZoneAddress:           return MediaTagType.DVDDL_MiddleZoneAddress;
                case DataType.DvdDlJumpIntervalSize:            return MediaTagType.DVDDL_JumpIntervalSize;
                case DataType.DvdDlManualLayerJumpLba:          return MediaTagType.DVDDL_ManualLayerJumpLBA;
                case DataType.BlurayDi:                         return MediaTagType.BD_DI;
                case DataType.BlurayBca:                        return MediaTagType.BD_BCA;
                case DataType.BlurayDds:                        return MediaTagType.BD_DDS;
                case DataType.BlurayCartridgeStatus:            return MediaTagType.BD_CartridgeStatus;
                case DataType.BluraySpareArea:                  return MediaTagType.BD_SpareArea;
                case DataType.AacsVolumeIdentifier:             return MediaTagType.AACS_VolumeIdentifier;
                case DataType.AacsSerialNumber:                 return MediaTagType.AACS_SerialNumber;
                case DataType.AacsMediaIdentifier:              return MediaTagType.AACS_MediaIdentifier;
                case DataType.AacsMediaKeyBlock:                return MediaTagType.AACS_MKB;
                case DataType.AacsDataKeys:                     return MediaTagType.AACS_DataKeys;
                case DataType.AacsLbaExtents:                   return MediaTagType.AACS_LBAExtents;
                case DataType.CprmMediaKeyBlock:                return MediaTagType.AACS_CPRM_MKB;
                case DataType.HybridRecognizedLayers:           return MediaTagType.Hybrid_RecognizedLayers;
                case DataType.ScsiMmcWriteProtection:           return MediaTagType.MMC_WriteProtection;
                case DataType.ScsiMmcDiscInformation:           return MediaTagType.MMC_DiscInformation;
                case DataType.ScsiMmcTrackResourcesInformation: return MediaTagType.MMC_TrackResourcesInformation;
                case DataType.ScsiMmcPowResourcesInformation:   return MediaTagType.MMC_POWResourcesInformation;
                case DataType.ScsiInquiry:                      return MediaTagType.SCSI_INQUIRY;
                case DataType.ScsiModePage2A:                   return MediaTagType.SCSI_MODEPAGE_2A;
                case DataType.AtaIdentify:                      return MediaTagType.ATA_IDENTIFY;
                case DataType.AtapiIdentify:                    return MediaTagType.ATAPI_IDENTIFY;
                case DataType.PcmciaCis:                        return MediaTagType.PCMCIA_CIS;
                case DataType.SecureDigitalCid:                 return MediaTagType.SD_CID;
                case DataType.SecureDigitalCsd:                 return MediaTagType.SD_CSD;
                case DataType.SecureDigitalScr:                 return MediaTagType.SD_SCR;
                case DataType.SecureDigitalOcr:                 return MediaTagType.SD_OCR;
                case DataType.MultiMediaCardCid:                return MediaTagType.MMC_CID;
                case DataType.MultiMediaCardCsd:                return MediaTagType.MMC_CSD;
                case DataType.MultiMediaCardOcr:                return MediaTagType.MMC_OCR;
                case DataType.MultiMediaCardExtendedCsd:        return MediaTagType.MMC_ExtendedCSD;
                case DataType.XboxSecuritySector:               return MediaTagType.Xbox_SecuritySector;
                case DataType.FloppyLeadOut:                    return MediaTagType.Floppy_LeadOut;
                case DataType.DvdDiscControlBlock:              return MediaTagType.DCB;
                case DataType.CompactDiscLeadIn:                return MediaTagType.CD_LeadIn;
                case DataType.CompactDiscLeadOut:               return MediaTagType.CD_LeadOut;
                case DataType.ScsiModeSense6:                   return MediaTagType.SCSI_MODESENSE_6;
                case DataType.ScsiModeSense10:                  return MediaTagType.SCSI_MODESENSE_10;
                case DataType.UsbDescriptors:                   return MediaTagType.USB_Descriptors;
                case DataType.XboxDmi:                          return MediaTagType.Xbox_DMI;
                case DataType.XboxPfi:                          return MediaTagType.Xbox_PFI;
                default:                                        throw new ArgumentOutOfRangeException();
            }
        }

        static DataType GetDataTypeForMediaTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_TOC:                        return DataType.CompactDiscPartialToc;
                case MediaTagType.CD_SessionInfo:                return DataType.CompactDiscSessionInfo;
                case MediaTagType.CD_FullTOC:                    return DataType.CompactDiscToc;
                case MediaTagType.CD_PMA:                        return DataType.CompactDiscPma;
                case MediaTagType.CD_ATIP:                       return DataType.CompactDiscAtip;
                case MediaTagType.CD_TEXT:                       return DataType.CompactDiscLeadInCdText;
                case MediaTagType.DVD_PFI:                       return DataType.DvdPfi;
                case MediaTagType.DVD_CMI:                       return DataType.DvdLeadInCmi;
                case MediaTagType.DVD_DiscKey:                   return DataType.DvdDiscKey;
                case MediaTagType.DVD_BCA:                       return DataType.DvdBca;
                case MediaTagType.DVD_DMI:                       return DataType.DvdDmi;
                case MediaTagType.DVD_MediaIdentifier:           return DataType.DvdMediaIdentifier;
                case MediaTagType.DVD_MKB:                       return DataType.DvdMediaKeyBlock;
                case MediaTagType.DVDRAM_DDS:                    return DataType.DvdRamDds;
                case MediaTagType.DVDRAM_MediumStatus:           return DataType.DvdRamMediumStatus;
                case MediaTagType.DVDRAM_SpareArea:              return DataType.DvdRamSpareArea;
                case MediaTagType.DVDR_RMD:                      return DataType.DvdRRmd;
                case MediaTagType.DVDR_PreRecordedInfo:          return DataType.DvdRPrerecordedInfo;
                case MediaTagType.DVDR_MediaIdentifier:          return DataType.DvdRMediaIdentifier;
                case MediaTagType.DVDR_PFI:                      return DataType.DvdRPfi;
                case MediaTagType.DVD_ADIP:                      return DataType.DvdAdip;
                case MediaTagType.HDDVD_CPI:                     return DataType.HdDvdCpi;
                case MediaTagType.HDDVD_MediumStatus:            return DataType.HdDvdMediumStatus;
                case MediaTagType.DVDDL_LayerCapacity:           return DataType.DvdDlLayerCapacity;
                case MediaTagType.DVDDL_MiddleZoneAddress:       return DataType.DvdDlMiddleZoneAddress;
                case MediaTagType.DVDDL_JumpIntervalSize:        return DataType.DvdDlJumpIntervalSize;
                case MediaTagType.DVDDL_ManualLayerJumpLBA:      return DataType.DvdDlManualLayerJumpLba;
                case MediaTagType.BD_DI:                         return DataType.BlurayDi;
                case MediaTagType.BD_BCA:                        return DataType.BlurayBca;
                case MediaTagType.BD_DDS:                        return DataType.BlurayDds;
                case MediaTagType.BD_CartridgeStatus:            return DataType.BlurayCartridgeStatus;
                case MediaTagType.BD_SpareArea:                  return DataType.BluraySpareArea;
                case MediaTagType.AACS_VolumeIdentifier:         return DataType.AacsVolumeIdentifier;
                case MediaTagType.AACS_SerialNumber:             return DataType.AacsSerialNumber;
                case MediaTagType.AACS_MediaIdentifier:          return DataType.AacsMediaIdentifier;
                case MediaTagType.AACS_MKB:                      return DataType.AacsMediaKeyBlock;
                case MediaTagType.AACS_DataKeys:                 return DataType.AacsDataKeys;
                case MediaTagType.AACS_LBAExtents:               return DataType.AacsLbaExtents;
                case MediaTagType.AACS_CPRM_MKB:                 return DataType.CprmMediaKeyBlock;
                case MediaTagType.Hybrid_RecognizedLayers:       return DataType.HybridRecognizedLayers;
                case MediaTagType.MMC_WriteProtection:           return DataType.ScsiMmcWriteProtection;
                case MediaTagType.MMC_DiscInformation:           return DataType.ScsiMmcDiscInformation;
                case MediaTagType.MMC_TrackResourcesInformation: return DataType.ScsiMmcTrackResourcesInformation;
                case MediaTagType.MMC_POWResourcesInformation:   return DataType.ScsiMmcPowResourcesInformation;
                case MediaTagType.SCSI_INQUIRY:                  return DataType.ScsiInquiry;
                case MediaTagType.SCSI_MODEPAGE_2A:              return DataType.ScsiModePage2A;
                case MediaTagType.ATA_IDENTIFY:                  return DataType.AtaIdentify;
                case MediaTagType.ATAPI_IDENTIFY:                return DataType.AtapiIdentify;
                case MediaTagType.PCMCIA_CIS:                    return DataType.PcmciaCis;
                case MediaTagType.SD_CID:                        return DataType.SecureDigitalCid;
                case MediaTagType.SD_CSD:                        return DataType.SecureDigitalCsd;
                case MediaTagType.SD_SCR:                        return DataType.SecureDigitalScr;
                case MediaTagType.SD_OCR:                        return DataType.SecureDigitalOcr;
                case MediaTagType.MMC_CID:                       return DataType.MultiMediaCardCid;
                case MediaTagType.MMC_CSD:                       return DataType.MultiMediaCardCsd;
                case MediaTagType.MMC_OCR:                       return DataType.MultiMediaCardOcr;
                case MediaTagType.MMC_ExtendedCSD:               return DataType.MultiMediaCardExtendedCsd;
                case MediaTagType.Xbox_SecuritySector:           return DataType.XboxSecuritySector;
                case MediaTagType.Floppy_LeadOut:                return DataType.FloppyLeadOut;
                case MediaTagType.DCB:                           return DataType.DvdDiscControlBlock;
                case MediaTagType.CD_LeadIn:                     return DataType.CompactDiscLeadIn;
                case MediaTagType.CD_LeadOut:                    return DataType.CompactDiscLeadOut;
                case MediaTagType.SCSI_MODESENSE_6:              return DataType.ScsiModeSense6;
                case MediaTagType.SCSI_MODESENSE_10:             return DataType.ScsiModeSense10;
                case MediaTagType.USB_Descriptors:               return DataType.UsbDescriptors;
                case MediaTagType.Xbox_DMI:                      return DataType.XboxDmi;
                case MediaTagType.Xbox_PFI:                      return DataType.XboxPfi;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tag), tag, null);
            }
        }

        enum CompressionType : ushort
        {
            None = 0,
            Lzma = 1
        }

        enum DataType : ushort
        {
            NoData                           = 0,
            UserData                         = 1,
            CompactDiscPartialToc            = 2,
            CompactDiscSessionInfo           = 3,
            CompactDiscToc                   = 4,
            CompactDiscPma                   = 5,
            CompactDiscAtip                  = 6,
            CompactDiscLeadInCdText          = 7,
            DvdPfi                           = 8,
            DvdLeadInCmi                     = 9,
            DvdDiscKey                       = 10,
            DvdBca                           = 11,
            DvdDmi                           = 12,
            DvdMediaIdentifier               = 13,
            DvdMediaKeyBlock                 = 14,
            DvdRamDds                        = 15,
            DvdRamMediumStatus               = 16,
            DvdRamSpareArea                  = 17,
            DvdRRmd                          = 18,
            DvdRPrerecordedInfo              = 19,
            DvdRMediaIdentifier              = 20,
            DvdRPfi                          = 21,
            DvdAdip                          = 22,
            HdDvdCpi                         = 23,
            HdDvdMediumStatus                = 24,
            DvdDlLayerCapacity               = 25,
            DvdDlMiddleZoneAddress           = 26,
            DvdDlJumpIntervalSize            = 27,
            DvdDlManualLayerJumpLba          = 28,
            BlurayDi                         = 29,
            BlurayBca                        = 30,
            BlurayDds                        = 31,
            BlurayCartridgeStatus            = 32,
            BluraySpareArea                  = 33,
            AacsVolumeIdentifier             = 34,
            AacsSerialNumber                 = 35,
            AacsMediaIdentifier              = 36,
            AacsMediaKeyBlock                = 37,
            AacsDataKeys                     = 38,
            AacsLbaExtents                   = 39,
            CprmMediaKeyBlock                = 40,
            HybridRecognizedLayers           = 41,
            ScsiMmcWriteProtection           = 42,
            ScsiMmcDiscInformation           = 43,
            ScsiMmcTrackResourcesInformation = 44,
            ScsiMmcPowResourcesInformation   = 45,
            ScsiInquiry                      = 46,
            ScsiModePage2A                   = 47,
            AtaIdentify                      = 48,
            AtapiIdentify                    = 49,
            PcmciaCis                        = 50,
            SecureDigitalCid                 = 51,
            SecureDigitalCsd                 = 52,
            SecureDigitalScr                 = 53,
            SecureDigitalOcr                 = 54,
            MultiMediaCardCid                = 55,
            MultiMediaCardCsd                = 56,
            MultiMediaCardOcr                = 57,
            MultiMediaCardExtendedCsd        = 58,
            XboxSecuritySector               = 59,
            FloppyLeadOut                    = 60,
            DvdDiscControlBlock              = 61,
            CompactDiscLeadIn                = 62,
            CompactDiscLeadOut               = 63,
            ScsiModeSense6                   = 64,
            ScsiModeSense10                  = 65,
            UsbDescriptors                   = 66,
            XboxDmi                          = 67,
            XboxPfi                          = 68,
            CdSectorPrefix                   = 69,
            CdSectorSuffix                   = 70,
            CdSectorSubchannel               = 71,
            AppleProfileTag                  = 72,
            AppleSonyTag                     = 73,
            PriamDataTowerTag                = 74
        }

        enum BlockType : uint
        {
            DataBlock          = 0x484B4C42,
            DeDuplicationTable = 0x48544444,
            Index              = 0x48584449,
            GeometryBlock      = 0x4D4F4547,
            MetadataBlock      = 0x5444545D,
            TracksBlock        = 0x534B5254
        }

        /// <summary>Header, at start of file</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        struct DicHeader
        {
            /// <summary>Header identifier, <see cref="DIC_MAGIC" /></summary>
            public ulong identifier;
            /// <summary>UTF-16LE name of the application that created the image</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string application;
            /// <summary>Image format major version. A new major version means a possibly incompatible change of format</summary>
            public byte imageMajorVersion;
            /// <summary>Image format minor version. A new minor version indicates a compatible change of format</summary>
            public byte imageMinorVersion;
            /// <summary>Major version of the application that created the image</summary>
            public byte applicationMajorVersion;
            /// <summary>Minor version of the application that created the image</summary>
            public byte applicationMinorVersion;
            /// <summary>Type of media contained on image</summary>
            public MediaType mediaType;
            /// <summary>Offset to index</summary>
            public ulong indexOffset;
            /// <summary>
            ///     Windows filetime (100 nanoseconds since 1601/01/01 00:00:00 UTC) of image creation time
            /// </summary>
            public long creationTime;
            /// <summary>
            ///     Windows filetime (100 nanoseconds since 1601/01/01 00:00:00 UTC) of image last written time
            /// </summary>
            public long lastWrittenTime;
        }

        /// <summary>Header for a deduplication table. Table follows it</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DdtHeader
        {
            /// <summary>Identifier, <see cref="BlockType.DeDuplicationTable" /></summary>
            public BlockType identifier;
            /// <summary>Type of data pointed by this DDT</summary>
            public DataType type;
            /// <summary>Compression algorithm used to compress the DDT</summary>
            public CompressionType compression;
            /// <summary>Each entry is ((byte offset in file) &lt;&lt; shift) + (sector offset in block)</summary>
            public byte shift;
            /// <summary>How many entries are in the table</summary>
            public ulong entries;
            /// <summary>Compressed length for the DDT</summary>
            public ulong cmpLength;
            /// <summary>Uncompressed length for the DDT</summary>
            public ulong length;
            /// <summary>CRC64-ECMA of the compressed DDT</summary>
            public ulong cmpCrc64;
            /// <summary>CRC64-ECMA of the uncompressed DDT</summary>
            public ulong crc64;
        }

        /// <summary>Header for the index, followed by entries</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IndexHeader
        {
            /// <summary>Identifier, <see cref="BlockType.Index" /></summary>
            public BlockType identifier;
            /// <summary>How many entries follow this header</summary>
            public ushort entries;
            /// <summary>CRC64-ECMA of the index</summary>
            public ulong crc64;
        }

        /// <summary>Index entry</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IndexEntry
        {
            /// <summary>Type of item pointed by this entry</summary>
            public BlockType blockType;
            /// <summary>Type of data contained by the block pointed by this entry</summary>
            public DataType dataType;
            /// <summary>Offset in file where item is stored</summary>
            public ulong offset;
        }

        /// <summary>Block header, precedes block data</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockHeader
        {
            /// <summary>Identifier, <see cref="BlockType.DataBlock" /></summary>
            public BlockType identifier;
            /// <summary>Type of data contained by this block</summary>
            public DataType type;
            /// <summary>Compression algorithm used to compress the block</summary>
            public CompressionType compression;
            /// <summary>Size in bytes of each sector contained in this block</summary>
            public uint sectorSize;
            /// <summary>Compressed length for the block</summary>
            public uint cmpLength;
            /// <summary>Uncompressed length for the block</summary>
            public uint length;
            /// <summary>CRC64-ECMA of the compressed block</summary>
            public ulong cmpCrc64;
            /// <summary>CRC64-ECMA of the uncompressed block</summary>
            public ulong crc64;
        }

        /// <summary>Geometry block, contains physical geometry information</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct GeometryBlock
        {
            /// <summary>Identifier, <see cref="BlockType.GeometryBlock" /></summary>
            public BlockType identifier;
            public uint      cylinders;
            public uint      heads;
            public uint      sectorsPerTrack;
        }

        /// <summary>Metadata block, contains metadata</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MetadataBlock
        {
            /// <summary>Identifier, <see cref="BlockType.MetadataBlock" /></summary>
            public BlockType identifier;
            /// <summary>Size in bytes of this whole metadata block</summary>
            public uint blockSize;
            /// <summary>Sequence of media set this media belongs to</summary>
            public int mediaSequence;
            /// <summary>Total number of media on the media set this media belongs to</summary>
            public int lastMediaSequence;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint creatorOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint creatorLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint commentsOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint commentsLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint mediaTitleOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint mediaTitleLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint mediaManufacturerOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint mediaManufacturerLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint mediaModelOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint mediaModelLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint mediaSerialNumberOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint mediaSerialNumberLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint mediaBarcodeOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint mediaBarcodeLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint mediaPartNumberOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint mediaPartNumberLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint driveManufacturerOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint driveManufacturerLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint driveModelOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint driveModelLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint driveSerialNumberOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint driveSerialNumberLength;
            /// <summary>Offset to start of creator string from start of this block</summary>
            public uint driveFirmwareRevisionOffset;
            /// <summary>Length in bytes of the null-terminated UTF-16LE creator string</summary>
            public uint driveFirmwareRevisionLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TracksHeader
        {
            /// <summary>Identifier, <see cref="BlockType.TracksBlock" /></summary>
            public BlockType identifier;
            /// <summary>How many entries follow this header</summary>
            public ushort entries;
            /// <summary>CRC64-ECMA of the block</summary>
            public ulong crc64;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        struct TrackEntry
        {
            public byte      sequence;
            public TrackType type;
            public long      start;
            public long      end;
            public long      pregap;
            public byte      session;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
            public string isrc;
            public byte   flags;
        }
    }
}