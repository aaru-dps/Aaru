// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UDIF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple Universal Disk Image Format.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Claunia.PropertyList;
using Claunia.RsrcFork;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Compression;
using DiscImageChef.Console;
using Ionic.Zlib;
using Schemas;
using SharpCompress.Compressors.ADC;
using SharpCompress.Compressors.BZip2;
using Version = Resources.Version;

#pragma warning disable 612

namespace DiscImageChef.DiscImages
{
    public class Udif : IWritableImage
    {
        const uint UDIF_SIGNATURE  = 0x6B6F6C79;
        const uint CHUNK_SIGNATURE = 0x6D697368;

        // All chunk types with this mask are compressed
        const uint CHUNK_TYPE_COMPRESSED_MASK = 0x80000000;

        const uint CHUNK_TYPE_ZERO    = 0x00000000;
        const uint CHUNK_TYPE_COPY    = 0x00000001;
        const uint CHUNK_TYPE_NOCOPY  = 0x00000002;
        const uint CHUNK_TYPE_KENCODE = 0x80000001;
        const uint CHUNK_TYPE_RLE     = 0x80000002;
        const uint CHUNK_TYPE_LZH     = 0x80000003;
        const uint CHUNK_TYPE_ADC     = 0x80000004;
        const uint CHUNK_TYPE_ZLIB    = 0x80000005;
        const uint CHUNK_TYPE_BZIP    = 0x80000006;
        const uint CHUNK_TYPE_LZFSE   = 0x80000007;
        const uint CHUNK_TYPE_COMMNT  = 0x7FFFFFFE;
        const uint CHUNK_TYPE_END     = 0xFFFFFFFF;

        const uint UDIF_CHECKSUM_TYPE_NONE   = 0;
        const uint UDIF_CHECKSUM_TYPE_CRC28  = 1;
        const uint UDIF_CHECKSUM_TYPE_CRC32  = 2;
        const uint UDIF_CHECKSUM_TYPE_DC42   = 3;
        const uint UDIF_CHECKSUM_TYPE_MD5    = 4;
        const uint UDIF_CHECKSUM_TYPE_SHA    = 5;
        const uint UDIF_CHECKSUM_TYPE_SHA1   = 6;
        const uint UDIF_CHECKSUM_TYPE_SHA256 = 7;
        const uint UDIF_CHECKSUM_TYPE_SHA384 = 8;
        const uint UDIF_CHECKSUM_TYPE_SHA512 = 9;

        const string                  RESOURCE_FORK_KEY  = "resource-fork";
        const string                  BLOCK_KEY          = "blkx";
        const uint                    BLOCK_OS_TYPE      = 0x626C6B78;
        const uint                    MAX_CACHE_SIZE     = 16777216;
        const uint                    SECTOR_SIZE        = 512;
        const uint                    MAX_CACHED_SECTORS = MAX_CACHE_SIZE / SECTOR_SIZE;
        uint                          buffersize;
        Dictionary<ulong, byte[]>     chunkCache;
        Dictionary<ulong, BlockChunk> chunks;
        BlockChunk                    currentChunk;
        uint                          currentChunkCacheSize;
        ulong                         currentSector;
        Crc32Context                  dataForkChecksum;

        UdifFooter footer;
        ImageInfo  imageInfo;

        Stream       imageStream;
        Crc32Context masterChecksum;

        Dictionary<ulong, byte[]> sectorCache;
        FileStream                writingStream;

        public Udif()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = null,
                Application           = null,
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

        public ImageInfo Info => imageInfo;

        public string Name => "Apple Universal Disk Image Format";
        public Guid   Id   => new Guid("5BEB9002-CF3D-429C-8E06-9A96F49203FF");

        public string Format => "Apple Universal Disk Image Format";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 512) return false;

            stream.Seek(-Marshal.SizeOf(footer), SeekOrigin.End);
            byte[] footerB = new byte[Marshal.SizeOf(footer)];

            stream.Read(footerB, 0, Marshal.SizeOf(footer));
            footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UdifFooter>(footerB);

            if(footer.signature == UDIF_SIGNATURE) return true;

            // Old UDIF as created by DiskCopy 6.5 using "OBSOLETE" format. (DiskCopy 5 rumored format?)
            stream.Seek(0, SeekOrigin.Begin);
            byte[] headerB = new byte[Marshal.SizeOf(footer)];

            stream.Read(headerB, 0, Marshal.SizeOf(footer));
            footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UdifFooter>(headerB);

            return footer.signature == UDIF_SIGNATURE;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 512) return false;

            stream.Seek(-Marshal.SizeOf(footer), SeekOrigin.End);
            byte[] footerB = new byte[Marshal.SizeOf(footer)];

            stream.Read(footerB, 0, Marshal.SizeOf(footer));
            footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UdifFooter>(footerB);

            if(footer.signature != UDIF_SIGNATURE)
            {
                stream.Seek(0, SeekOrigin.Begin);
                footerB = new byte[Marshal.SizeOf(footer)];

                stream.Read(footerB, 0, Marshal.SizeOf(footer));
                footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UdifFooter>(footerB);

                if(footer.signature != UDIF_SIGNATURE) throw new Exception("Unable to find UDIF signature.");

                DicConsole.VerboseWriteLine("Found obsolete UDIF format.");
            }

            DicConsole.DebugWriteLine("UDIF plugin", "footer.signature = 0x{0:X8}",     footer.signature);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.version = {0}",            footer.version);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.headerSize = {0}",         footer.headerSize);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.flags = {0}",              footer.flags);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.runningDataForkOff = {0}", footer.runningDataForkOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkOff = {0}",        footer.dataForkOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkLen = {0}",        footer.dataForkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.rsrcForkOff = {0}",        footer.rsrcForkOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.rsrcForkLen = {0}",        footer.rsrcForkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.segmentNumber = {0}",      footer.segmentNumber);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.segmentCount = {0}",       footer.segmentCount);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.segmentId = {0}",          footer.segmentId);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkChkType = {0}",    footer.dataForkChkType);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkLen = {0}",        footer.dataForkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkChk = 0x{0:X8}",   footer.dataForkChk);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.plistOff = {0}",           footer.plistOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.plistLen = {0}",           footer.plistLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.masterChkType = {0}",      footer.masterChkType);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.masterChkLen = {0}",       footer.masterChkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.masterChk = 0x{0:X8}",     footer.masterChk);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.imageVariant = {0}",       footer.imageVariant);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.sectorCount = {0}",        footer.sectorCount);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved1 is empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved1));
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved2 is empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved2));
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved3 is empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved3));
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved4 is empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved4));

            // Block chunks and headers
            List<byte[]> blkxList = new List<byte[]>();
            chunks = new Dictionary<ulong, BlockChunk>();

            bool fakeBlockChunks = false;

            byte[] vers = null;

            if(footer.plistLen == 0 && footer.rsrcForkLen != 0)
            {
                DicConsole.DebugWriteLine("UDIF plugin", "Reading resource fork.");
                byte[] rsrcB = new byte[footer.rsrcForkLen];
                stream.Seek((long)footer.rsrcForkOff, SeekOrigin.Begin);
                stream.Read(rsrcB, 0, rsrcB.Length);

                ResourceFork rsrc = new ResourceFork(rsrcB);

                if(!rsrc.ContainsKey(BLOCK_OS_TYPE))
                    throw new
                        ImageNotSupportedException("Image resource fork doesn't contain UDIF block chunks. Please fill an issue and send it to us.");

                Resource blkxRez = rsrc.GetResource(BLOCK_OS_TYPE);

                if(blkxRez == null)
                    throw new
                        ImageNotSupportedException("Image resource fork doesn't contain UDIF block chunks. Please fill an issue and send it to us.");

                if(blkxRez.GetIds().Length == 0)
                    throw new
                        ImageNotSupportedException("Image resource fork doesn't contain UDIF block chunks. Please fill an issue and send it to us.");

                blkxList.AddRange(blkxRez.GetIds().Select(blkxId => blkxRez.GetResource(blkxId)));

                Resource versRez = rsrc.GetResource(0x76657273);

                if(versRez != null) vers = versRez.GetResource(versRez.GetIds()[0]);
            }
            else if(footer.plistLen != 0)
            {
                DicConsole.DebugWriteLine("UDIF plugin", "Reading property list.");
                byte[] plistB = new byte[footer.plistLen];
                stream.Seek((long)footer.plistOff, SeekOrigin.Begin);
                stream.Read(plistB, 0, plistB.Length);

                DicConsole.DebugWriteLine("UDIF plugin", "Parsing property list.");
                NSDictionary plist = (NSDictionary)XmlPropertyListParser.Parse(plistB);
                if(plist == null) throw new Exception("Could not parse property list.");

                if(!plist.TryGetValue(RESOURCE_FORK_KEY, out NSObject rsrcObj))
                    throw new Exception("Could not retrieve resource fork.");

                NSDictionary rsrc = (NSDictionary)rsrcObj;

                if(!rsrc.TryGetValue(BLOCK_KEY, out NSObject blkxObj))
                    throw new Exception("Could not retrieve block chunks array.");

                NSObject[] blkx = ((NSArray)blkxObj).GetArray();

                foreach(NSDictionary part in blkx.Cast<NSDictionary>())
                {
                    if(!part.TryGetValue("Name", out _)) throw new Exception("Could not retrieve Name");

                    if(!part.TryGetValue("Data", out NSObject dataObj)) throw new Exception("Could not retrieve Data");

                    blkxList.Add(((NSData)dataObj).Bytes);
                }

                if(rsrc.TryGetValue("vers", out NSObject versObj))
                {
                    NSObject[] versArray           = ((NSArray)versObj).GetArray();
                    if(versArray.Length >= 1) vers = ((NSData)versArray[0]).Bytes;
                }
            }
            else
            {
                // Obsolete read-only UDIF only prepended the header and then put the image without any kind of block references.
                // So let's falsify a block chunk
                BlockChunk bChnk = new BlockChunk
                {
                    length  = footer.dataForkLen,
                    offset  = footer.dataForkOff,
                    sector  = 0,
                    sectors = footer.sectorCount,
                    type    = CHUNK_TYPE_COPY
                };
                imageInfo.Sectors = footer.sectorCount;
                chunks.Add(bChnk.sector, bChnk);
                buffersize      = 2048 * SECTOR_SIZE;
                fakeBlockChunks = true;
            }

            if(vers != null)
            {
                Version version = new Version(vers);

                string release = null;
                string dev     = null;
                string pre     = null;

                string major                              = $"{version.MajorVersion}";
                string minor                              = $".{version.MinorVersion / 10}";
                if(version.MinorVersion % 10 > 0) release = $".{version.MinorVersion % 10}";
                switch(version.DevStage)
                {
                    case Version.DevelopmentStage.Alpha:
                        dev = "a";
                        break;
                    case Version.DevelopmentStage.Beta:
                        dev = "b";
                        break;
                    case Version.DevelopmentStage.PreAlpha:
                        dev = "d";
                        break;
                }

                if(dev == null && version.PreReleaseVersion > 0) dev = "f";

                if(dev != null) pre = $"{version.PreReleaseVersion}";

                imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
                imageInfo.Application        = version.VersionString;
                imageInfo.Comments           = version.VersionMessage;

                if(version.MajorVersion      == 3) imageInfo.Application = "ShrinkWrap™";
                else if(version.MajorVersion == 6) imageInfo.Application = "DiskCopy";
            }
            else imageInfo.Application = "DiskCopy";

            DicConsole.DebugWriteLine("UDIF plugin", "Image application = {0} version {1}", imageInfo.Application,
                                      imageInfo.ApplicationVersion);

            imageInfo.Sectors = 0;
            if(!fakeBlockChunks)
            {
                if(blkxList.Count == 0)
                    throw new
                        ImageNotSupportedException("Could not retrieve block chunks. Please fill an issue and send it to us.");

                buffersize = 0;

                foreach(byte[] blkxBytes in blkxList)
                {
                    BlockHeader bHdr  = new BlockHeader();
                    byte[]      bHdrB = new byte[Marshal.SizeOf(bHdr)];
                    Array.Copy(blkxBytes, 0, bHdrB, 0, Marshal.SizeOf(bHdr));
                    bHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<BlockHeader>(bHdrB);

                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.signature = 0x{0:X8}",  bHdr.signature);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.version = {0}",         bHdr.version);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.sectorStart = {0}",     bHdr.sectorStart);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.sectorCount = {0}",     bHdr.sectorCount);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.dataOffset = {0}",      bHdr.dataOffset);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.buffers = {0}",         bHdr.buffers);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.descriptor = 0x{0:X8}", bHdr.descriptor);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved1 = {0}",       bHdr.reserved1);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved2 = {0}",       bHdr.reserved2);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved3 = {0}",       bHdr.reserved3);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved4 = {0}",       bHdr.reserved4);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved5 = {0}",       bHdr.reserved5);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved6 = {0}",       bHdr.reserved6);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.checksumType = {0}",    bHdr.checksumType);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.checksumLen = {0}",     bHdr.checksumLen);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.checksum = 0x{0:X8}",   bHdr.checksum);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunks = {0}",          bHdr.chunks);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reservedChk is empty? = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(bHdr.reservedChk));

                    if(bHdr.buffers > buffersize) buffersize = bHdr.buffers * SECTOR_SIZE;

                    for(int i = 0; i < bHdr.chunks; i++)
                    {
                        BlockChunk bChnk  = new BlockChunk();
                        byte[]     bChnkB = new byte[Marshal.SizeOf(bChnk)];
                        Array.Copy(blkxBytes, Marshal.SizeOf(bHdr) + Marshal.SizeOf(bChnk) * i, bChnkB, 0,
                                   Marshal.SizeOf(bChnk));
                        bChnk = BigEndianMarshal.ByteArrayToStructureBigEndian<BlockChunk>(bChnkB);

                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].type = 0x{1:X8}", i, bChnk.type);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].comment = {1}",   i, bChnk.comment);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sector = {1}",    i, bChnk.sector);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sectors = {1}",   i, bChnk.sectors);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].offset = {1}",    i, bChnk.offset);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].length = {1}",    i, bChnk.length);

                        if(bChnk.type == CHUNK_TYPE_END) break;

                        imageInfo.Sectors += bChnk.sectors;

                        // Chunk offset is relative
                        bChnk.sector += bHdr.sectorStart;
                        bChnk.offset += bHdr.dataOffset;

                        switch(bChnk.type)
                        {
                            // TODO: Handle comments
                            case CHUNK_TYPE_COMMNT: continue;
                            // TODO: Handle compressed chunks
                            case CHUNK_TYPE_KENCODE:
                                throw new
                                    ImageNotSupportedException("Chunks compressed with KenCode are not yet supported.");
                            case CHUNK_TYPE_LZH:
                                throw new
                                    ImageNotSupportedException("Chunks compressed with LZH are not yet supported.");
                            case CHUNK_TYPE_LZFSE:
                                throw new
                                    ImageNotSupportedException("Chunks compressed with lzfse are not yet supported.");
                        }

                        if(bChnk.type > CHUNK_TYPE_NOCOPY && bChnk.type < CHUNK_TYPE_COMMNT ||
                           bChnk.type > CHUNK_TYPE_LZFSE  && bChnk.type < CHUNK_TYPE_END)
                            throw new ImageNotSupportedException($"Unsupported chunk type 0x{bChnk.type:X8} found");

                        if(bChnk.sectors > 0) chunks.Add(bChnk.sector, bChnk);
                    }
                }
            }

            sectorCache           = new Dictionary<ulong, byte[]>();
            chunkCache            = new Dictionary<ulong, byte[]>();
            currentChunkCacheSize = 0;
            imageStream           = stream;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.SectorSize           = SECTOR_SIZE;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = imageInfo.Sectors * SECTOR_SIZE;
            imageInfo.Version              = $"{footer.version}";

            imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
            imageInfo.Heads           = 16;
            imageInfo.SectorsPerTrack = 63;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            BlockChunk readChunk        = new BlockChunk();
            bool       chunkFound       = false;
            ulong      chunkStartSector = 0;

            foreach(KeyValuePair<ulong, BlockChunk> kvp in chunks.Where(kvp => sectorAddress >= kvp.Key))
            {
                readChunk        = kvp.Value;
                chunkFound       = true;
                chunkStartSector = kvp.Key;
            }

            long relOff = ((long)sectorAddress - (long)chunkStartSector) * SECTOR_SIZE;

            if(relOff < 0)
                throw new ArgumentOutOfRangeException(nameof(relOff),
                                                      $"Got a negative offset for sector {sectorAddress}. This should not happen.");

            if(!chunkFound)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if((readChunk.type & CHUNK_TYPE_COMPRESSED_MASK) == CHUNK_TYPE_COMPRESSED_MASK)
            {
                if(!chunkCache.TryGetValue(chunkStartSector, out byte[] buffer))
                {
                    byte[] cmpBuffer = new byte[readChunk.length];
                    imageStream.Seek((long)readChunk.offset, SeekOrigin.Begin);
                    imageStream.Read(cmpBuffer, 0, cmpBuffer.Length);
                    MemoryStream cmpMs     = new MemoryStream(cmpBuffer);
                    Stream       decStream = null;

                    switch(readChunk.type)
                    {
                        case CHUNK_TYPE_ADC:
                            decStream = new ADCStream(cmpMs);
                            break;
                        case CHUNK_TYPE_ZLIB:
                            decStream = new ZlibStream(cmpMs, CompressionMode.Decompress);
                            break;
                        case CHUNK_TYPE_BZIP:
                            decStream = new BZip2Stream(cmpMs, SharpCompress.Compressors.CompressionMode.Decompress);
                            break;
                        case CHUNK_TYPE_RLE: break;
                        default:
                            throw new ImageNotSupportedException($"Unsupported chunk type 0x{readChunk.type:X8} found");
                    }

                    #if DEBUG
                    try
                    {
                        #endif
                        byte[] tmpBuffer;
                        int    realSize;
                        switch(readChunk.type)
                        {
                            case CHUNK_TYPE_ADC:
                            case CHUNK_TYPE_ZLIB:
                            case CHUNK_TYPE_BZIP:
                                tmpBuffer = new byte[buffersize];
                                realSize  = decStream.Read(tmpBuffer, 0, (int)buffersize);
                                buffer    = new byte[realSize];
                                Array.Copy(tmpBuffer, 0, buffer, 0, realSize);

                                if(currentChunkCacheSize + realSize > MAX_CACHE_SIZE)
                                {
                                    chunkCache.Clear();
                                    currentChunkCacheSize = 0;
                                }

                                chunkCache.Add(chunkStartSector, buffer);
                                currentChunkCacheSize += (uint)realSize;
                                break;
                            case CHUNK_TYPE_RLE:
                                tmpBuffer = new byte[buffersize];
                                realSize  = 0;
                                AppleRle rle = new AppleRle(cmpMs);
                                for(int i = 0; i < buffersize; i++)
                                {
                                    int b = rle.ProduceByte();
                                    if(b == -1) break;

                                    tmpBuffer[i] = (byte)b;
                                    realSize++;
                                }

                                buffer = new byte[realSize];
                                Array.Copy(tmpBuffer, 0, buffer, 0, realSize);
                                break;
                        }
                        #if DEBUG
                    }
                    catch(ZlibException)
                    {
                        DicConsole.WriteLine("zlib exception on chunk starting at sector {0}", readChunk.sector);
                        throw;
                    }
                    #endif
                }

                sector = new byte[SECTOR_SIZE];
                Array.Copy(buffer, relOff, sector, 0, SECTOR_SIZE);

                if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);

                return sector;
            }

            switch(readChunk.type)
            {
                case CHUNK_TYPE_NOCOPY:
                case CHUNK_TYPE_ZERO:
                    sector = new byte[SECTOR_SIZE];

                    if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);
                    return sector;
                case CHUNK_TYPE_COPY:
                    imageStream.Seek((long)readChunk.offset + relOff, SeekOrigin.Begin);
                    sector = new byte[SECTOR_SIZE];
                    imageStream.Read(sector, 0, sector.Length);

                    if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);
                    return sector;
            }

            throw new ImageNotSupportedException($"Unsupported chunk type 0x{readChunk.type:X8} found");
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

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive, MediaType.CompactFlash,
                MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
                MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".dmg"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            chunks           = new Dictionary<ulong, BlockChunk>();
            currentChunk     = new BlockChunk();
            currentSector    = 0;
            dataForkChecksum = new Crc32Context();
            masterChecksum   = new Crc32Context();

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";
            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            if(sectorAddress < currentSector)
            {
                ErrorMessage = "Tried to rewind, this format rewinded on writing";
                return false;
            }

            masterChecksum.Update(data);

            bool isEmpty = ArrayHelpers.ArrayIsNullOrEmpty(data);

            switch(currentChunk.type)
            {
                case CHUNK_TYPE_ZERO:
                    currentChunk.type = isEmpty ? CHUNK_TYPE_NOCOPY : CHUNK_TYPE_COPY;
                    break;
                case CHUNK_TYPE_NOCOPY when !isEmpty:
                case CHUNK_TYPE_COPY when isEmpty:
                    chunks.Add(currentChunk.sector, currentChunk);
                    currentChunk = new BlockChunk
                    {
                        type   = isEmpty ? CHUNK_TYPE_NOCOPY : CHUNK_TYPE_COPY,
                        sector = currentSector,
                        offset = (ulong)(isEmpty ? 0 : writingStream.Position)
                    };
                    break;
            }

            currentChunk.sectors++;
            currentChunk.length += (ulong)(isEmpty ? 0 : 512);
            currentSector++;

            if(!isEmpty)
            {
                dataForkChecksum.Update(data);
                writingStream.Write(data, 0, data.Length);
            }

            ErrorMessage = "";
            return true;
        }

        // TODO: This can be optimized
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
            {
                if(currentChunk.type == CHUNK_TYPE_COPY)
                {
                    chunks.Add(currentChunk.sector, currentChunk);
                    currentChunk = new BlockChunk {type = CHUNK_TYPE_NOCOPY, sector = currentSector};
                }

                currentChunk.sectors += (ulong)(data.Length / imageInfo.SectorSize);
                currentSector        += (ulong)(data.Length / imageInfo.SectorSize);
                masterChecksum.Update(data);

                ErrorMessage = "";
                return true;
            }

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[imageInfo.SectorSize];
                Array.Copy(data, i * imageInfo.SectorSize, tmp, 0, imageInfo.SectorSize);
                if(!WriteSector(tmp, sectorAddress + i)) return false;
            }

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            if(currentChunk.type != CHUNK_TYPE_NOCOPY) currentChunk.length = currentChunk.sectors * 512;
            chunks.Add(currentChunk.sector, currentChunk);

            chunks.Add(imageInfo.Sectors, new BlockChunk {type = CHUNK_TYPE_END, sector = imageInfo.Sectors});

            BlockHeader bHdr = new BlockHeader
            {
                signature    = CHUNK_SIGNATURE,
                version      = 1,
                sectorCount  = imageInfo.Sectors,
                checksumType = UDIF_CHECKSUM_TYPE_CRC32,
                checksumLen  = 32,
                checksum     = BitConverter.ToUInt32(dataForkChecksum.Final().Reverse().ToArray(), 0),
                chunks       = (uint)chunks.Count
            };

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            MemoryStream chunkMs = new MemoryStream();
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.signature),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.version),      0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.sectorStart),  0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.sectorCount),  0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.dataOffset),   0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.buffers),      0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.descriptor),   0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved1),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved2),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved3),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved4),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved5),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved6),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksumType), 0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksumLen),  0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksum),     0, 4);
            chunkMs.Write(new byte[124],                                     0, 124);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.chunks),       0, 4);

            foreach(BlockChunk chunk in chunks.Values)
            {
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.type),    0, 4);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.comment), 0, 4);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.sector),  0, 8);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.sectors), 0, 8);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.offset),  0, 8);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.length),  0, 8);
            }

            byte[] plist = Encoding.UTF8.GetBytes(new NSDictionary
            {
                {
                    "resource-fork",
                    new NSDictionary
                    {
                        {
                            "blkx",
                            new NSArray
                            {
                                new NSDictionary
                                {
                                    {"Attributes", "0x0050"},
                                    {"CFName", "whole disk (DiscImageChef : 0)"},
                                    {"Data", chunkMs.ToArray()},
                                    {"ID", "0"},
                                    {"Name", "whole disk (DiscImageChef : 0)"}
                                }
                            }
                        }
                    }
                }
            }.ToXmlPropertyList());

            footer = new UdifFooter
            {
                signature       = UDIF_SIGNATURE,
                version         = 4,
                headerSize      = 512,
                flags           = 1,
                dataForkLen     = (ulong)writingStream.Length,
                segmentNumber   = 1,
                segmentCount    = 1,
                segmentId       = Guid.NewGuid(),
                dataForkChkType = UDIF_CHECKSUM_TYPE_CRC32,
                dataForkChkLen  = 32,
                dataForkChk     = BitConverter.ToUInt32(dataForkChecksum.Final().Reverse().ToArray(), 0),
                plistOff        = (ulong)writingStream.Length,
                plistLen        = (ulong)plist.Length,
                // TODO: Find how is this calculated
                /*masterChkType   = 2,
                masterChkLen    = 32,
                masterChk       = BitConverter.ToUInt32(masterChecksum.Final().Reverse().ToArray(), 0),*/
                imageVariant = 2,
                sectorCount  = imageInfo.Sectors
            };

            writingStream.Seek(0, SeekOrigin.End);
            writingStream.Write(plist,                                                     0, plist.Length);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.signature),          0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.version),            0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.headerSize),         0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.flags),              0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.runningDataForkOff), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkOff),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkLen),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.rsrcForkOff),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.rsrcForkLen),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.segmentNumber),      0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.segmentCount),       0, 4);
            writingStream.Write(footer.segmentId.ToByteArray(),                            0, 16);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkChkType),    0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkChkLen),     0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkChk),        0, 4);
            writingStream.Write(new byte[124],                                             0, 124);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.plistOff),           0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.plistLen),           0, 8);
            writingStream.Write(new byte[120],                                             0, 120);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.masterChkType),      0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.masterChkLen),       0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.masterChk),          0, 4);
            writingStream.Write(new byte[124],                                             0, 124);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.imageVariant),       0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.sectorCount),        0, 8);
            writingStream.Write(new byte[12],                                              0, 12);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        // TODO: Comments
        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            // Not stored in image
            return true;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            // Not supported
            return false;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            // Not supported
            return false;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UdifFooter
        {
            public uint  signature;
            public uint  version;
            public uint  headerSize;
            public uint  flags;
            public ulong runningDataForkOff;
            public ulong dataForkOff;
            public ulong dataForkLen;
            public ulong rsrcForkOff;
            public ulong rsrcForkLen;
            public uint  segmentNumber;
            public uint  segmentCount;
            public Guid  segmentId;
            public uint  dataForkChkType;
            public uint  dataForkChkLen;
            public uint  dataForkChk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reserved1;
            public ulong plistOff;
            public ulong plistLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
            public byte[] reserved2;
            public uint masterChkType;
            public uint masterChkLen;
            public uint masterChk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reserved3;
            public uint  imageVariant;
            public ulong sectorCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockHeader
        {
            public uint  signature;
            public uint  version;
            public ulong sectorStart;
            public ulong sectorCount;
            public ulong dataOffset;
            public uint  buffers;
            public uint  descriptor;
            public uint  reserved1;
            public uint  reserved2;
            public uint  reserved3;
            public uint  reserved4;
            public uint  reserved5;
            public uint  reserved6;
            public uint  checksumType;
            public uint  checksumLen;
            public uint  checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reservedChk;
            public uint chunks;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockChunk
        {
            public uint  type;
            public uint  comment;
            public ulong sector;
            public ulong sectors;
            public ulong offset;
            public ulong length;
        }
    }
}