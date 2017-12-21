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
using Claunia.PropertyList;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using Ionic.Zlib;
using SharpCompress.Compressors.ADC;
using SharpCompress.Compressors.BZip2;
using CompressionMode = SharpCompress.Compressors.CompressionMode;
using Version = Resources.Version;

namespace DiscImageChef.DiscImages
{
    public class Udif : ImagePlugin
    {
        #region Internal constants
        const uint UDIF_SIGNATURE = 0x6B6F6C79;
        const uint CHUNK_SIGNATURE = 0x6D697368;

        // All chunk types with this mask are compressed
        const uint CHUNK_TYPE_COMPRESSED_MASK = 0x80000000;

        const uint CHUNK_TYPE_ZERO = 0x00000000;
        const uint CHUNK_TYPE_COPY = 0x00000001;
        const uint CHUNK_TYPE_NOCOPY = 0x00000002;
        const uint CHUNK_TYPE_KENCODE = 0x80000001;
        const uint CHUNK_TYPE_RLE = 0x80000002;
        const uint CHUNK_TYPE_LZH = 0x80000003;
        const uint CHUNK_TYPE_ADC = 0x80000004;
        const uint CHUNK_TYPE_ZLIB = 0x80000005;
        const uint CHUNK_TYPE_BZIP = 0x80000006;
        const uint CHUNK_TYPE_LZFSE = 0x80000007;
        const uint CHUNK_TYPE_COMMNT = 0x7FFFFFFE;
        const uint CHUNK_TYPE_END = 0xFFFFFFFF;

        const string RESOURCE_FORK_KEY = "resource-fork";
        const string BLOCK_KEY = "blkx";
        const uint BLOCK_OS_TYPE = 0x626C6B78;
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UdifFooter
        {
            public uint signature;
            public uint version;
            public uint headerSize;
            public uint flags;
            public ulong runningDataForkOff;
            public ulong dataForkOff;
            public ulong dataForkLen;
            public ulong rsrcForkOff;
            public ulong rsrcForkLen;
            public uint segmentNumber;
            public uint segmentCount;
            public Guid segmentId;
            public uint dataForkChkType;
            public uint dataForkChkLen;
            public uint dataForkChk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)] public byte[] reserved1;
            public ulong plistOff;
            public ulong plistLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)] public byte[] reserved2;
            public uint masterChkType;
            public uint masterChkLen;
            public uint masterChk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)] public byte[] reserved3;
            public uint imageVariant;
            public ulong sectorCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] reserved4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockHeader
        {
            public uint signature;
            public uint version;
            public ulong sectorStart;
            public ulong sectorCount;
            public ulong dataOffset;
            public uint buffers;
            public uint descriptor;
            public uint reserved1;
            public uint reserved2;
            public uint reserved3;
            public uint reserved4;
            public uint reserved5;
            public uint reserved6;
            public uint checksumType;
            public uint checksumLen;
            public uint checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)] public byte[] reservedChk;
            public uint chunks;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlockChunk
        {
            public uint type;
            public uint comment;
            public ulong sector;
            public ulong sectors;
            public ulong offset;
            public ulong length;
        }
        #endregion

        UdifFooter footer;
        Dictionary<ulong, BlockChunk> chunks;

        Dictionary<ulong, byte[]> sectorCache;
        Dictionary<ulong, byte[]> chunkCache;
        const uint MAX_CACHE_SIZE = 16777216;
        const uint SECTOR_SIZE = 512;
        uint maxCachedSectors = MAX_CACHE_SIZE / SECTOR_SIZE;
        uint currentChunkCacheSize;
        uint buffersize;

        Stream imageStream;

        public Udif()
        {
            Name = "Apple Universal Disk Image Format";
            PluginUuid = new Guid("5BEB9002-CF3D-429C-8E06-9A96F49203FF");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = null;
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
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

        public override bool OpenImage(Filter imageFilter)
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

            DicConsole.DebugWriteLine("UDIF plugin", "footer.signature = 0x{0:X8}", footer.signature);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.version = {0}", footer.version);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.headerSize = {0}", footer.headerSize);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.flags = {0}", footer.flags);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.runningDataForkOff = {0}", footer.runningDataForkOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkOff = {0}", footer.dataForkOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkLen = {0}", footer.dataForkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.rsrcForkOff = {0}", footer.rsrcForkOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.rsrcForkLen = {0}", footer.rsrcForkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.segmentNumber = {0}", footer.segmentNumber);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.segmentCount = {0}", footer.segmentCount);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.segmentId = {0}", footer.segmentId);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkChkType = {0}", footer.dataForkChkType);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkLen = {0}", footer.dataForkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.dataForkChk = 0x{0:X8}", footer.dataForkChk);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.xmlOff = {0}", footer.plistOff);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.xmlLen = {0}", footer.plistLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.masterChkType = {0}", footer.masterChkType);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.masterChkLen = {0}", footer.masterChkLen);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.masterChk = 0x{0:X8}", footer.masterChk);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.imageVariant = {0}", footer.imageVariant);
            DicConsole.DebugWriteLine("UDIF plugin", "footer.sectorCount = {0}", footer.sectorCount);
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

                NSObject rsrcObj;

                if(!plist.TryGetValue(RESOURCE_FORK_KEY, out rsrcObj))
                    throw new Exception("Could not retrieve resource fork.");

                NSDictionary rsrc = (NSDictionary)rsrcObj;

                NSObject blkxObj;

                if(!rsrc.TryGetValue(BLOCK_KEY, out blkxObj))
                    throw new Exception("Could not retrieve block chunks array.");

                NSObject[] blkx = ((NSArray)blkxObj).GetArray();

                foreach(NSDictionary part in blkx.Cast<NSDictionary>()) {
                    NSObject nameObj, dataObj;

                    if(!part.TryGetValue("Name", out nameObj)) throw new Exception("Could not retrieve Name");

                    if(!part.TryGetValue("Data", out dataObj)) throw new Exception("Could not retrieve Data");

                    blkxList.Add(((NSData)dataObj).Bytes);
                }

                NSObject versObj;

                if(rsrc.TryGetValue("vers", out versObj))
                {
                    NSObject[] versArray = ((NSArray)versObj).GetArray();
                    if(versArray.Length >= 1) vers = ((NSData)versArray[0]).Bytes;
                }
            }
            else
            {
                // Obsolete read-only UDIF only prepended the header and then put the image without any kind of block references.
                // So let's falsify a block chunk
                BlockChunk bChnk = new BlockChunk();
                bChnk.length = footer.dataForkLen;
                bChnk.offset = footer.dataForkOff;
                bChnk.sector = 0;
                bChnk.sectors = footer.sectorCount;
                bChnk.type = CHUNK_TYPE_COPY;
                ImageInfo.Sectors = footer.sectorCount;
                chunks.Add(bChnk.sector, bChnk);
                buffersize = 2048 * SECTOR_SIZE;
                fakeBlockChunks = true;
            }

            if(vers != null)
            {
                Version version = new Version(vers);

                string major;
                string minor;
                string release = null;
                string dev = null;
                string pre = null;

                major = string.Format("{0}", version.MajorVersion);
                minor = string.Format(".{0}", version.MinorVersion / 10);
                if(version.MinorVersion % 10 > 0) release = string.Format(".{0}", version.MinorVersion % 10);
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

                if(dev != null) pre = string.Format("{0}", version.PreReleaseVersion);

                ImageInfo.ImageApplicationVersion = string.Format("{0}{1}{2}{3}{4}", major, minor, release, dev, pre);
                ImageInfo.ImageApplication = version.VersionString;
                ImageInfo.ImageComments = version.VersionMessage;

                if(version.MajorVersion == 3) ImageInfo.ImageApplication = "ShrinkWrap™";
                else if(version.MajorVersion == 6) ImageInfo.ImageApplication = "DiskCopy";
            }
            else ImageInfo.ImageApplication = "DiskCopy";

            DicConsole.DebugWriteLine("UDIF plugin", "Image application = {0} version {1}", ImageInfo.ImageApplication,
                                      ImageInfo.ImageApplicationVersion);

            ImageInfo.Sectors = 0;
            if(!fakeBlockChunks)
            {
                if(blkxList.Count == 0)
                    throw new
                        ImageNotSupportedException("Could not retrieve block chunks. Please fill an issue and send it to us.");

                buffersize = 0;

                foreach(byte[] blkxBytes in blkxList)
                {
                    BlockHeader bHdr = new BlockHeader();
                    byte[] bHdrB = new byte[Marshal.SizeOf(bHdr)];
                    Array.Copy(blkxBytes, 0, bHdrB, 0, Marshal.SizeOf(bHdr));
                    bHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<BlockHeader>(bHdrB);

                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.signature = 0x{0:X8}", bHdr.signature);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.version = {0}", bHdr.version);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.sectorStart = {0}", bHdr.sectorStart);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.sectorCount = {0}", bHdr.sectorCount);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.dataOffset = {0}", bHdr.dataOffset);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.buffers = {0}", bHdr.buffers);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.descriptor = 0x{0:X8}", bHdr.descriptor);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved1 = {0}", bHdr.reserved1);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved2 = {0}", bHdr.reserved2);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved3 = {0}", bHdr.reserved3);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved4 = {0}", bHdr.reserved4);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved5 = {0}", bHdr.reserved5);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved6 = {0}", bHdr.reserved6);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.checksumType = {0}", bHdr.checksumType);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.checksumLen = {0}", bHdr.checksumLen);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.checksum = 0x{0:X8}", bHdr.checksum);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunks = {0}", bHdr.chunks);
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reservedChk is empty? = {0}",
                                              ArrayHelpers.ArrayIsNullOrEmpty(bHdr.reservedChk));

                    if(bHdr.buffers > buffersize) buffersize = bHdr.buffers * SECTOR_SIZE;

                    for(int i = 0; i < bHdr.chunks; i++)
                    {
                        BlockChunk bChnk = new BlockChunk();
                        byte[] bChnkB = new byte[Marshal.SizeOf(bChnk)];
                        Array.Copy(blkxBytes, Marshal.SizeOf(bHdr) + Marshal.SizeOf(bChnk) * i, bChnkB, 0,
                                   Marshal.SizeOf(bChnk));
                        bChnk = BigEndianMarshal.ByteArrayToStructureBigEndian<BlockChunk>(bChnkB);

                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].type = 0x{1:X8}", i, bChnk.type);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].comment = {1}", i, bChnk.comment);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sector = {1}", i, bChnk.sector);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sectors = {1}", i, bChnk.sectors);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].offset = {1}", i, bChnk.offset);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].length = {1}", i, bChnk.length);

                        if(bChnk.type == CHUNK_TYPE_END) break;

                        ImageInfo.Sectors += bChnk.sectors;

                        // Chunk offset is relative
                        bChnk.sector += bHdr.sectorStart;
                        bChnk.offset += bHdr.dataOffset;

                        switch(bChnk.type) {
                            // TODO: Handle comments
                            case CHUNK_TYPE_COMMNT: continue;
                            // TODO: Handle compressed chunks
                            case CHUNK_TYPE_KENCODE:
                                throw new
                                    ImageNotSupportedException("Chunks compressed with KenCode are not yet supported.");
                            case CHUNK_TYPE_RLE: throw new ImageNotSupportedException("Chunks compressed with RLE are not yet supported.");
                            case CHUNK_TYPE_LZH: throw new ImageNotSupportedException("Chunks compressed with LZH are not yet supported.");
                            case CHUNK_TYPE_LZFSE: throw new ImageNotSupportedException("Chunks compressed with lzfse are not yet supported.");
                        }

                        if(bChnk.type > CHUNK_TYPE_NOCOPY && bChnk.type < CHUNK_TYPE_COMMNT ||
                           bChnk.type > CHUNK_TYPE_LZFSE && bChnk.type < CHUNK_TYPE_END)
                            throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found",
                                                                               bChnk.type));

                        if(bChnk.sectors > 0) chunks.Add(bChnk.sector, bChnk);
                    }
                }
            }

            sectorCache = new Dictionary<ulong, byte[]>();
            chunkCache = new Dictionary<ulong, byte[]>();
            currentChunkCacheSize = 0;
            imageStream = stream;

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.SectorSize = SECTOR_SIZE;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.MediaType = MediaType.GENERIC_HDD;
            ImageInfo.ImageSize = ImageInfo.Sectors * SECTOR_SIZE;
            ImageInfo.ImageVersion = string.Format("{0}", footer.version);

            ImageInfo.Cylinders = (uint)(ImageInfo.Sectors / 16 / 63);
            ImageInfo.Heads = 16;
            ImageInfo.SectorsPerTrack = 63;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector)) return sector;

            BlockChunk currentChunk = new BlockChunk();
            bool chunkFound = false;
            ulong chunkStartSector = 0;

            foreach(KeyValuePair<ulong, BlockChunk> kvp in chunks.Where(kvp => sectorAddress >= kvp.Key)) {
                currentChunk = kvp.Value;
                chunkFound = true;
                chunkStartSector = kvp.Key;
            }

            long relOff = ((long)sectorAddress - (long)chunkStartSector) * SECTOR_SIZE;

            if(relOff < 0)
                throw new ArgumentOutOfRangeException(nameof(relOff),
                                                      string
                                                          .Format("Got a negative offset for sector {0}. This should not happen.",
                                                                  sectorAddress));

            if(!chunkFound)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if((currentChunk.type & CHUNK_TYPE_COMPRESSED_MASK) == CHUNK_TYPE_COMPRESSED_MASK)
            {
                byte[] buffer;
                if(!chunkCache.TryGetValue(chunkStartSector, out buffer))
                {
                    byte[] cmpBuffer = new byte[currentChunk.length];
                    imageStream.Seek((long)currentChunk.offset, SeekOrigin.Begin);
                    imageStream.Read(cmpBuffer, 0, cmpBuffer.Length);
                    MemoryStream cmpMs = new MemoryStream(cmpBuffer);
                    Stream decStream;

                    switch(currentChunk.type) {
                        case CHUNK_TYPE_ADC: decStream = new ADCStream(cmpMs, CompressionMode.Decompress);
                            break;
                        case CHUNK_TYPE_ZLIB: decStream = new ZlibStream(cmpMs, Ionic.Zlib.CompressionMode.Decompress);
                            break;
                        case CHUNK_TYPE_BZIP: decStream = new BZip2Stream(cmpMs, CompressionMode.Decompress);
                            break;
                        default:
                            throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found",
                                                                               currentChunk.type));
                    }

#if DEBUG
                    try
                    {
#endif
                        byte[] tmpBuffer = new byte[buffersize];
                        int realSize = decStream.Read(tmpBuffer, 0, (int)buffersize);
                        buffer = new byte[realSize];
                        Array.Copy(tmpBuffer, 0, buffer, 0, realSize);
                        tmpBuffer = null;

                        if(currentChunkCacheSize + realSize > MAX_CACHE_SIZE)
                        {
                            chunkCache.Clear();
                            currentChunkCacheSize = 0;
                        }

                        chunkCache.Add(chunkStartSector, buffer);
                        currentChunkCacheSize += (uint)realSize;
#if DEBUG
                    }
                    catch(ZlibException)
                    {
                        DicConsole.WriteLine("zlib exception on chunk starting at sector {0}", currentChunk.sector);
                        throw;
                    }
#endif
                }

                sector = new byte[SECTOR_SIZE];
                Array.Copy(buffer, relOff, sector, 0, SECTOR_SIZE);

                if(sectorCache.Count >= maxCachedSectors) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);

                return sector;
            }

            switch(currentChunk.type) {
                case CHUNK_TYPE_NOCOPY:
                case CHUNK_TYPE_ZERO:
                    sector = new byte[SECTOR_SIZE];

                    if(sectorCache.Count >= maxCachedSectors) sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);
                    return sector;
                case CHUNK_TYPE_COPY:
                    imageStream.Seek((long)currentChunk.offset + relOff, SeekOrigin.Begin);
                    sector = new byte[SECTOR_SIZE];
                    imageStream.Read(sector, 0, sector.Length);

                    if(sectorCache.Count >= maxCachedSectors) sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);
                    return sector;
            }

            throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found",
                                                               currentChunk.type));
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public override bool ImageHasPartitions()
        {
            return false;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override string GetImageFormat()
        {
            return "Apple Universal Disk Image Format";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        #region Unsupported features
        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetMediaManufacturer()
        {
            return null;
        }

        public override string GetMediaModel()
        {
            return null;
        }

        public override string GetMediaSerialNumber()
        {
            return null;
        }

        public override string GetMediaBarcode()
        {
            return null;
        }

        public override string GetMediaPartNumber()
        {
            return null;
        }

        public override int GetMediaSequence()
        {
            return 0;
        }

        public override int GetLastDiskSequence()
        {
            return 0;
        }

        public override string GetDriveManufacturer()
        {
            return null;
        }

        public override string GetDriveModel()
        {
            return null;
        }

        public override string GetDriveSerialNumber()
        {
            return null;
        }

        public override List<Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion
    }
}