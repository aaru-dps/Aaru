// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UDIF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Claunia.PropertyList;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;
using DiscImageChef.Filters;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors;

namespace DiscImageChef.DiscImages
{
    public class UDIF : ImagePlugin
    {
        #region Internal constants
        const uint UDIF_Signature = 0x6B6F6C79;
        const uint Chunk_Signature = 0x6D697368;

        // All chunk types with this mask are compressed
        const uint ChunkType_CompressedMask = 0x80000000;

        const uint ChunkType_Zero    = 0x00000000;
        const uint ChunkType_Copy    = 0x00000001;
        const uint ChunkType_NoCopy  = 0x00000002;
		const uint ChunkType_KenCode = 0x80000001;
		const uint ChunkType_RLE     = 0x80000002;
		const uint ChunkType_LZH     = 0x80000003;
		const uint ChunkType_ADC     = 0x80000004;
        const uint ChunkType_Zlib    = 0x80000005;
        const uint ChunkType_Bzip    = 0x80000006;
        const uint ChunkType_LZFSE   = 0x80000007;
        const uint ChunkType_Commnt  = 0x7FFFFFFF;
        const uint ChunkType_End     = 0xFFFFFFFF;

        const string ResourceForkKey = "resource-fork";
        const string BlockKey = "blkx";
        const uint BlockOSType = 0x626C6B78;
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UDIF_Footer
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
            public uint imageVariant;
            public ulong sectorCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] reserved4;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 124)]
            public byte[] reservedChk;
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

        UDIF_Footer footer;
        Dictionary<ulong, BlockChunk> chunks;

        Dictionary<ulong, byte[]> sectorCache;
		Dictionary<ulong, byte[]> chunkCache;
        const uint MaxCacheSize = 16777216;
        const uint sectorSize = 512;
        uint maxCachedSectors = MaxCacheSize / sectorSize;
		uint currentChunkCacheSize;
		uint buffersize;

        Stream imageStream;

        public UDIF()
        {
            Name = "Apple Universal Disk Image Format";
            PluginUUID = new Guid("5BEB9002-CF3D-429C-8E06-9A96F49203FF");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaSerialNumber = null;
            ImageInfo.mediaBarcode = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 512)
                return false;

            stream.Seek(-Marshal.SizeOf(footer), SeekOrigin.End);
            byte[] footer_b = new byte[Marshal.SizeOf(footer)];

            stream.Read(footer_b, 0, Marshal.SizeOf(footer));
            footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UDIF_Footer>(footer_b);

            if(footer.signature == UDIF_Signature)
                return true;

            // Old UDIF as created by DiskCopy 6.5 using "OBSOLETE" format. (DiskCopy 5 rumored format?)
            stream.Seek(0, SeekOrigin.Begin);
            byte[] header_b = new byte[Marshal.SizeOf(footer)];

            stream.Read(header_b, 0, Marshal.SizeOf(footer));
            footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UDIF_Footer>(header_b);

            return footer.signature == UDIF_Signature;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 512)
                return false;

            stream.Seek(-Marshal.SizeOf(footer), SeekOrigin.End);
            byte[] footer_b = new byte[Marshal.SizeOf(footer)];

            stream.Read(footer_b, 0, Marshal.SizeOf(footer));
            footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UDIF_Footer>(footer_b);

            if(footer.signature != UDIF_Signature)
            {
                stream.Seek(0, SeekOrigin.Begin);
                footer_b = new byte[Marshal.SizeOf(footer)];

                stream.Read(footer_b, 0, Marshal.SizeOf(footer));
                footer = BigEndianMarshal.ByteArrayToStructureBigEndian<UDIF_Footer>(footer_b);

                if(footer.signature != UDIF_Signature)
                    throw new Exception("Unable to find UDIF signature.");

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
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved1 is empty? = {0}", ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved1));
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved2 is empty? = {0}", ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved2));
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved3 is empty? = {0}", ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved3));
            DicConsole.DebugWriteLine("UDIF plugin", "footer.reserved4 is empty? = {0}", ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved4));

            // Block chunks and headers
            List<byte[]> blkxList = new List<byte[]>();
            chunks = new Dictionary<ulong, BlockChunk>();

            bool fakeBlockChunks = false;

            byte[] vers = null;

            if(footer.plistLen == 0 && footer.rsrcForkLen != 0)
            {
                DicConsole.DebugWriteLine("UDIF plugin", "Reading resource fork.");
                byte[] rsrc_b = new byte[footer.rsrcForkLen];
                stream.Seek((long)footer.rsrcForkOff, SeekOrigin.Begin);
                stream.Read(rsrc_b, 0, rsrc_b.Length);

                ResourceFork rsrc = new ResourceFork(rsrc_b);

                if(!rsrc.ContainsKey(BlockOSType))
                    throw new ImageNotSupportedException("Image resource fork doesn't contain UDIF block chunks. Please fill an issue and send it to us.");

                Resource blkxRez = rsrc.GetResource(BlockOSType);

                if(blkxRez == null)
                    throw new ImageNotSupportedException("Image resource fork doesn't contain UDIF block chunks. Please fill an issue and send it to us.");

                if(blkxRez.GetIds().Length == 0)
                    throw new ImageNotSupportedException("Image resource fork doesn't contain UDIF block chunks. Please fill an issue and send it to us.");

                foreach(short blkxId in blkxRez.GetIds())
                    blkxList.Add(blkxRez.GetResource(blkxId));

                Resource versRez = rsrc.GetResource(0x76657273);

                if(versRez != null)
                    vers = versRez.GetResource(versRez.GetIds()[0]);
            }
            else if(footer.plistLen != 0)
            {
                DicConsole.DebugWriteLine("UDIF plugin", "Reading property list.");
                byte[] plist_b = new byte[footer.plistLen];
                stream.Seek((long)footer.plistOff, SeekOrigin.Begin);
                stream.Read(plist_b, 0, plist_b.Length);

                DicConsole.DebugWriteLine("UDIF plugin", "Parsing property list.");
                NSDictionary plist = (NSDictionary)XmlPropertyListParser.Parse(plist_b);
                if(plist == null)
                    throw new Exception("Could not parse property list.");

                NSObject rsrcObj;

                if(!plist.TryGetValue(ResourceForkKey, out rsrcObj))
                    throw new Exception("Could not retrieve resource fork.");

                NSDictionary rsrc = (NSDictionary)rsrcObj;

                NSObject blkxObj;

                if(!rsrc.TryGetValue(BlockKey, out blkxObj))
                    throw new Exception("Could not retrieve block chunks array.");

                NSObject[] blkx = ((NSArray)blkxObj).GetArray();

                foreach(NSObject partObj in blkx)
                {
                    NSDictionary part = (NSDictionary)partObj;
                    NSObject nameObj, dataObj;

                    if(!part.TryGetValue("Name", out nameObj))
                        throw new Exception("Could not retrieve Name");

                    if(!part.TryGetValue("Data", out dataObj))
                        throw new Exception("Could not retrieve Data");

                    blkxList.Add(((NSData)dataObj).Bytes);
                }

                NSObject versObj;

                if(rsrc.TryGetValue("vers", out versObj))
                {
                    NSObject[] versArray = ((NSArray)versObj).GetArray();
                    if(versArray.Length >= 1)
                        vers = ((NSData)versArray[0]).Bytes;
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
                bChnk.type = ChunkType_Copy;
                ImageInfo.sectors = footer.sectorCount;
                chunks.Add(bChnk.sector, bChnk);
				buffersize = 2048 * sectorSize;
                fakeBlockChunks = true;
            }

            if(vers != null)
            {
                Resources.Version version = new Resources.Version(vers);

                string major;
                string minor;
                string release = null;
                string dev = null;
                string pre = null;

                major = string.Format("{0}", version.MajorVersion);
                minor = string.Format(".{0}", version.MinorVersion / 10);
                if(version.MinorVersion % 10 > 0)
                    release = string.Format(".{0}", version.MinorVersion % 10);
                switch(version.DevStage)
                {
                    case Resources.Version.DevelopmentStage.Alpha:
                        dev = "a";
                        break;
                    case Resources.Version.DevelopmentStage.Beta:
                        dev = "b";
                        break;
                    case Resources.Version.DevelopmentStage.PreAlpha:
                        dev = "d";
                        break;
                }

                if(dev == null && version.PreReleaseVersion > 0)
                    dev = "f";

                if(dev != null)
                    pre = string.Format("{0}", version.PreReleaseVersion);

                ImageInfo.imageApplicationVersion = string.Format("{0}{1}{2}{3}{4}", major, minor, release, dev, pre);
                ImageInfo.imageApplication = version.VersionString;
                ImageInfo.imageComments = version.VersionMessage;

                if(version.MajorVersion == 3)
                    ImageInfo.imageApplication = "ShrinkWrap™";
                else if(version.MajorVersion == 6)
                    ImageInfo.imageApplication = "DiskCopy";
            }
            else
                ImageInfo.imageApplication = "DiskCopy";
            DicConsole.DebugWriteLine("UDIF plugin", "Image application = {0} version {1}", ImageInfo.imageApplication, ImageInfo.imageApplicationVersion);

			ImageInfo.sectors = 0;
            if(!fakeBlockChunks)
            {
                if(blkxList.Count == 0)
                    throw new ImageNotSupportedException("Could not retrieve block chunks. Please fill an issue and send it to us.");

				buffersize = 0;

                foreach(byte[] blkxBytes in blkxList)
                {
                    BlockHeader bHdr = new BlockHeader();
                    byte[] bHdr_b = new byte[Marshal.SizeOf(bHdr)];
                    Array.Copy(blkxBytes, 0, bHdr_b, 0, Marshal.SizeOf(bHdr));
                    bHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<BlockHeader>(bHdr_b);

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
                    DicConsole.DebugWriteLine("UDIF plugin", "bHdr.reservedChk is empty? = {0}", ArrayHelpers.ArrayIsNullOrEmpty(bHdr.reservedChk));

					if(bHdr.buffers > buffersize)
						buffersize = bHdr.buffers * sectorSize;

                    for(int i = 0; i < bHdr.chunks; i++)
                    {
                        BlockChunk bChnk = new BlockChunk();
                        byte[] bChnk_b = new byte[Marshal.SizeOf(bChnk)];
                        Array.Copy(blkxBytes, Marshal.SizeOf(bHdr) + Marshal.SizeOf(bChnk) * i, bChnk_b, 0, Marshal.SizeOf(bChnk));
                        bChnk = BigEndianMarshal.ByteArrayToStructureBigEndian<BlockChunk>(bChnk_b);


                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].type = 0x{1:X8}", i, bChnk.type);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].comment = {1}", i, bChnk.comment);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sector = {1}", i, bChnk.sector);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sectors = {1}", i, bChnk.sectors);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].offset = {1}", i, bChnk.offset);
                        DicConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].length = {1}", i, bChnk.length);

                        if(bChnk.type == ChunkType_End)
                            break;

						ImageInfo.sectors += bChnk.sectors;

						// Chunk offset is relative
						bChnk.sector += bHdr.sectorStart;
						bChnk.offset += bHdr.dataOffset;

                        // TODO: Handle comments
                        if(bChnk.type == ChunkType_Commnt)
                            continue;

						// TODO: Handle compressed chunks
						if((bChnk.type == ChunkType_KenCode))
							throw new ImageNotSupportedException("Chunks compressed with KenCode are not yet supported.");
						if((bChnk.type == ChunkType_RLE))
							throw new ImageNotSupportedException("Chunks compressed with RLE are not yet supported.");
						if((bChnk.type == ChunkType_LZH))
							throw new ImageNotSupportedException("Chunks compressed with LZH are not yet supported.");
						if((bChnk.type == ChunkType_ADC))
							throw new ImageNotSupportedException("Chunks compressed with ADC are not yet supported.");
						if((bChnk.type == ChunkType_LZFSE))
							throw new ImageNotSupportedException("Chunks compressed with lzfse are not yet supported.");

						if((bChnk.type > ChunkType_NoCopy && bChnk.type < ChunkType_Commnt) ||
						   (bChnk.type > ChunkType_LZFSE && bChnk.type < ChunkType_End))
                            throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found", bChnk.type));

                        if(bChnk.sectors > 0)
                            chunks.Add(bChnk.sector, bChnk);
                    }
                }
            }

            sectorCache = new Dictionary<ulong, byte[]>();
			chunkCache = new Dictionary<ulong, byte[]>();
			currentChunkCacheSize = 0;
			imageStream = stream;

            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectorSize = sectorSize;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.mediaType = MediaType.GENERIC_HDD;
            ImageInfo.imageSize = ImageInfo.sectors * sectorSize;
            ImageInfo.imageVersion = string.Format("{0}", footer.version);

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector))
                return sector;

            BlockChunk currentChunk = new BlockChunk();
            bool chunkFound = false;
            ulong chunkStartSector = 0;

            foreach(KeyValuePair<ulong, BlockChunk> kvp in chunks)
            {
                if(sectorAddress >= kvp.Key)
                {
                    currentChunk = kvp.Value;
                    chunkFound = true;
                    chunkStartSector = kvp.Key;
                }
            }

            long relOff = ((long)sectorAddress - (long)chunkStartSector) * sectorSize;

            if(relOff < 0)
                throw new ArgumentOutOfRangeException(nameof(relOff), string.Format("Got a negative offset for sector {0}. This should not happen.", sectorAddress));

            if(!chunkFound)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			if((currentChunk.type & ChunkType_CompressedMask) == ChunkType_CompressedMask)
			{
				byte[] buffer;
				if(!chunkCache.TryGetValue(chunkStartSector, out buffer))
				{
					byte[] cmpBuffer = new byte[currentChunk.length];
					imageStream.Seek((long)currentChunk.offset, SeekOrigin.Begin);
					imageStream.Read(cmpBuffer, 0, cmpBuffer.Length);
					MemoryStream cmpMs = new MemoryStream(cmpBuffer);
					Stream decStream;

					if(currentChunk.type == ChunkType_Zlib)
						decStream = new ZlibStream(cmpMs, CompressionMode.Decompress);
					else if(currentChunk.type == ChunkType_Bzip)
						decStream = new BZip2Stream(cmpMs, CompressionMode.Decompress);
					else
						throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found", currentChunk.type));

					byte[] tmpBuffer = new byte[buffersize];
					int realSize = decStream.Read(tmpBuffer, 0, (int)buffersize);
					buffer = new byte[realSize];
					Array.Copy(tmpBuffer, 0, buffer, 0, realSize);
					tmpBuffer = null;

					if(currentChunkCacheSize + realSize > MaxCacheSize)
					{
						chunkCache.Clear();
						currentChunkCacheSize = 0;
					}

					chunkCache.Add(chunkStartSector, buffer);
					currentChunkCacheSize += (uint)realSize;
				}

				sector = new byte[sectorSize];
				Array.Copy(buffer, relOff, sector, 0, sectorSize);

				if(sectorCache.Count >= maxCachedSectors)
					sectorCache.Clear();

				sectorCache.Add(sectorAddress, sector);

				return sector;
			}

            if(currentChunk.type == ChunkType_NoCopy || currentChunk.type == ChunkType_Zero)
            {
                sector = new byte[sectorSize];

                if(sectorCache.Count >= maxCachedSectors)
                    sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
                return sector;
            }

            if(currentChunk.type == ChunkType_Copy)
            {
                imageStream.Seek((long)currentChunk.offset + relOff, SeekOrigin.Begin);
                sector = new byte[sectorSize];
                imageStream.Read(sector, 0, sector.Length);

                if(sectorCache.Count >= maxCachedSectors)
                    sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
                return sector;
            }

            throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found", currentChunk.type));
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.sectors)
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
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override string GetImageFormat()
        {
            return "Apple Universal Disk Image Format";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.sectors; i++)
                UnknownLBAs.Add(i);
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
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

