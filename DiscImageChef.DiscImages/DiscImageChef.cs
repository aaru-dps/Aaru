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
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using SharpCompress.Compressors.LZMA;

namespace DiscImageChef.DiscImages
{
    // TODO: Work in progress
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
        Dictionary<MediaTagType, byte[]> mediaTags;
        long                             outMemoryDdtPosition;
        byte                             shift;
        byte[]                           structureBytes;
        IntPtr                           structurePointer;
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
        public List<Partition> Partitions { get; }
        public List<Track>     Tracks     { get; private set; }
        public List<Session>   Sessions   { get; }

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

            bool foundUserDataDdt = false;
            mediaTags             = new Dictionary<MediaTagType, byte[]>();
            foreach(IndexEntry entry in index)
            {
                imageStream.Position = (long)entry.offset;
                switch(entry.blockType)
                {
                    // TODO: Non-deduplicatable sector tags are data blocks
                    case BlockType.DataBlock:
                        // NOP block, skip
                        if(entry.dataType == DataType.NoData ||
                           // Unused, skip
                           entry.dataType == DataType.UserData) break;

                        imageStream.Position = (long)entry.offset;

                        BlockHeader blockHeader = new BlockHeader();
                        structureBytes          = new byte[Marshal.SizeOf(blockHeader)];
                        imageStream.Read(structureBytes, 0, structureBytes.Length);
                        structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(blockHeader));
                        Marshal.Copy(structureBytes, 0, structurePointer, Marshal.SizeOf(blockHeader));
                        blockHeader = (BlockHeader)Marshal.PtrToStructure(structurePointer, typeof(BlockHeader));
                        Marshal.FreeHGlobal(structurePointer);
                        imageInfo.ImageSize = blockHeader.cmpLength;

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

                        byte[]       data;
                        MediaTagType mediaTagType = GetMediaTagTypeForDataType(blockHeader.type);

                        DicConsole.DebugWriteLine("DiscImageChef format plugin", "Found media tag {0} at position {1}",
                                                  mediaTagType, entry.offset);

                        if(blockHeader.compression == CompressionType.Lzma)
                        {
                            byte[] compressedTag  = new byte[blockHeader.cmpLength];
                            byte[] lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                            imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                            imageStream.Read(compressedTag,  0, (int)blockHeader.cmpLength);
                            MemoryStream compressedTagMs = new MemoryStream(compressedTag);
                            LzmaStream   lzmaBlock       = new LzmaStream(lzmaProperties, compressedTagMs);
                            data                         = new byte[blockHeader.length];
                            lzmaBlock.Read(data, 0, (int)blockHeader.length);
                            lzmaBlock.Close();
                            compressedTagMs.Close();
                            compressedTag = null;
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
                        if(BitConverter.ToUInt64(blockCrc, 0) != blockHeader.crc64)
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Incorrect CRC found: 0x{0:X16} found, expected 0x{1:X16}, continuing...",
                                                      BitConverter.ToUInt64(blockCrc, 0), blockHeader.crc64);
                            break;
                        }

                        if(mediaTags.ContainsKey(mediaTagType))
                        {
                            DicConsole.DebugWriteLine("DiscImageChef format plugin",
                                                      "Media tag type {0} duplicated, removing previous entry...",
                                                      mediaTagType);

                            mediaTags.Remove(mediaTagType);
                        }

                        mediaTags.Add(mediaTagType, data);
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
                        imageInfo.ImageSize = ddtHeader.cmpLength;

                        if(ddtHeader.identifier != BlockType.DeDuplicationTable) break;

                        // TODO: Check CRC64
                        imageInfo.Sectors = ddtHeader.entries;
                        shift             = ddtHeader.shift;

                        switch(ddtHeader.compression)
                        {
                            case CompressionType.Lzma:
                                DicConsole.DebugWriteLine("DiscImageChef format plugin", "Decompressing DDT...");
                                DateTime ddtStart       = DateTime.UtcNow;
                                byte[]   compressedDdt  = new byte[ddtHeader.cmpLength];
                                byte[]   lzmaProperties = new byte[LZMA_PROPERTIES_LENGTH];
                                imageStream.Read(lzmaProperties, 0, LZMA_PROPERTIES_LENGTH);
                                imageStream.Read(compressedDdt,  0, (int)ddtHeader.cmpLength);
                                MemoryStream compressedDdtMs = new MemoryStream(compressedDdt);
                                LzmaStream   lzmaDdt         = new LzmaStream(lzmaProperties, compressedDdtMs);
                                byte[]       decompressedDdt = new byte[ddtHeader.length];
                                lzmaDdt.Read(decompressedDdt, 0, (int)ddtHeader.length);
                                lzmaDdt.Close();
                                compressedDdtMs.Close();
                                compressedDdt = null;
                                userDataDdt   = new ulong[ddtHeader.entries];
                                for(ulong i = 0; i < ddtHeader.entries; i++)
                                    userDataDdt[i] = BitConverter.ToUInt64(decompressedDdt, (int)(i * sizeof(ulong)));
                                decompressedDdt    = null;
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
                }
            }

            if(!foundUserDataDdt) throw new ImageNotSupportedException("Could not find user data deduplication table.");

            // TODO: Sector size!
            imageInfo.SectorSize = 512;

            // TODO: Timestamps in header?
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            // TODO: Metadata
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            // TODO: Get from media type
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            // TODO: Calculate
            //imageInfo.ImageSize            = qHdr.size;
            // TODO: If no geometry
            /*imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
            imageInfo.Heads           = 16;
            imageInfo.SectorsPerTrack = 63;*/

            // Initialize caches
            blockCache                     = new Dictionary<ulong, byte[]>();
            blockHeaderCache               = new Dictionary<ulong, BlockHeader>();
            currentCacheSize               = 0;
            if(!inMemoryDdt) ddtEntryCache = new Dictionary<ulong, ulong>();

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
                    byte[] compressedBlock = new byte[blockHeader.cmpLength];
                    byte[] lzmaProperties  = new byte[LZMA_PROPERTIES_LENGTH];
                    imageStream.Read(lzmaProperties,  0, LZMA_PROPERTIES_LENGTH);
                    imageStream.Read(compressedBlock, 0, (int)blockHeader.cmpLength);
                    MemoryStream compressedBlockMs = new MemoryStream(compressedBlock);
                    LzmaStream   lzmaBlock         = new LzmaStream(lzmaProperties, compressedBlockMs);
                    block                          = new byte[blockHeader.length];
                    lzmaBlock.Read(block, 0, (int)blockHeader.length);
                    lzmaBlock.Close();
                    compressedBlockMs.Close();
                    compressedBlock = null;
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
            throw new NotImplementedException();
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new NotImplementedException();
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            throw new NotImplementedException();
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            throw new NotImplementedException();
        }

        public bool? VerifyMediaImage()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MediaTagType>  SupportedMediaTags  => Enum.GetValues(typeof(MediaType)).Cast<MediaTagType>();
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            Enum.GetValues(typeof(MediaType)).Cast<SectorTagType>();
        public IEnumerable<MediaType> SupportedMediaTypes =>
            Enum.GetValues(typeof(MediaType)).Cast<MediaType>();
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new[]
            {
                ("sectors_per_block", typeof(uint),
                "How many sectors to store per block (will be rounded to next power of two)")
            };
        public IEnumerable<string> KnownExtensions => new[] {".dicf"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        // TODO: Support resume
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            uint sectorsPerBlock;

            if(options != null)
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
            else sectorsPerBlock    = 4096;

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

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

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
                mediaType               = mediaType
            };

            // TODO: Settable
            inMemoryDdt = sectors <= 256 * 1024 * 1024 / sizeof(ulong);

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
                currentBlockHeader.cmpLength = (uint)blockStream.Length;
                Crc64Context.Data(blockStream.ToArray(), out byte[] cmpCrc64);
                currentBlockHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64, 0);

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
                compressedBlockStream = new LzmaStream(new LzmaEncoderProperties(), false, blockStream);
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

        // TODO: Optimize this
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

        // TODO: Implement
        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not yet implemented.";
            return false;
        }

        // TODO: Implement
        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not yet implemented.";
            return false;
        }

        public bool SetTracks(List<Track> tracks)
        {
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
                currentBlockHeader.cmpLength = (uint)blockStream.Length;
                Crc64Context.Data(blockStream.ToArray(), out byte[] cmpCrc64);
                currentBlockHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64, 0);

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
                compressedBlockStream = new LzmaStream(new LzmaEncoderProperties(), false, blockStream);
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
                    tagBlock.cmpLength   = (uint)tagData.Length;
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
                compressedBlockStream = new LzmaStream(new LzmaEncoderProperties(), false, blockStream);
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
                ddtHeader.cmpLength = (uint)blockStream.Length;
                Crc64Context.Data(blockStream.ToArray(), out byte[] cmpCrc64);
                ddtHeader.cmpCrc64 = BitConverter.ToUInt64(cmpCrc64, 0);

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
            imageStream.Position = 0;
            structurePointer     = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            structureBytes       = new byte[Marshal.SizeOf(header)];
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

        // TODO: Implement
        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
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

        // TODO: Implement
        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not yet implemented.";
            return false;
        }

        // TODO: Implement
        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not yet implemented.";
            return false;
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
            XboxPfi                          = 68
        }

        enum BlockType : uint
        {
            DataBlock          = 0x484B4C42,
            DeDuplicationTable = 0x48544444,
            Index              = 0x48584449,
            GeometryBlock      = 0x4D4F4547
        }

        /// <summary>Header, at start of file</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        struct DicHeader
        {
            /// <summary>Header identifier, <see cref="DIC_MAGIC" /></summary>
            public ulong identifier;
            /// <summary>UTF-16 name of the application that created the image</summary>
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
    }
}