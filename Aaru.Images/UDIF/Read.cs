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
//     Reads Apple Universal Disk Image Format.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.PropertyList;
using Claunia.RsrcFork;
using Ionic.Zlib;
using SharpCompress.Compressors.ADC;
using SharpCompress.Compressors.BZip2;
using Version = Resources.Version;

#pragma warning disable 612

namespace Aaru.DiscImages
{
    public partial class Udif
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 512)
                return false;

            stream.Seek(-Marshal.SizeOf<UdifFooter>(), SeekOrigin.End);
            byte[] footerB = new byte[Marshal.SizeOf<UdifFooter>()];

            stream.Read(footerB, 0, Marshal.SizeOf<UdifFooter>());
            footer = Marshal.ByteArrayToStructureBigEndian<UdifFooter>(footerB);

            if(footer.signature != UDIF_SIGNATURE)
            {
                stream.Seek(0, SeekOrigin.Begin);
                footerB = new byte[Marshal.SizeOf<UdifFooter>()];

                stream.Read(footerB, 0, Marshal.SizeOf<UdifFooter>());
                footer = Marshal.ByteArrayToStructureBigEndian<UdifFooter>(footerB);

                if(footer.signature != UDIF_SIGNATURE)
                    throw new Exception("Unable to find UDIF signature.");

                AaruConsole.VerboseWriteLine("Found obsolete UDIF format.");
            }

            AaruConsole.DebugWriteLine("UDIF plugin", "footer.signature = 0x{0:X8}", footer.signature);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.version = {0}", footer.version);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.headerSize = {0}", footer.headerSize);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.flags = {0}", footer.flags);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.runningDataForkOff = {0}", footer.runningDataForkOff);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.dataForkOff = {0}", footer.dataForkOff);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.dataForkLen = {0}", footer.dataForkLen);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.rsrcForkOff = {0}", footer.rsrcForkOff);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.rsrcForkLen = {0}", footer.rsrcForkLen);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.segmentNumber = {0}", footer.segmentNumber);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.segmentCount = {0}", footer.segmentCount);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.segmentId = {0}", footer.segmentId);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.dataForkChkType = {0}", footer.dataForkChkType);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.dataForkLen = {0}", footer.dataForkLen);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.dataForkChk = 0x{0:X8}", footer.dataForkChk);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.plistOff = {0}", footer.plistOff);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.plistLen = {0}", footer.plistLen);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.masterChkType = {0}", footer.masterChkType);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.masterChkLen = {0}", footer.masterChkLen);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.masterChk = 0x{0:X8}", footer.masterChk);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.imageVariant = {0}", footer.imageVariant);
            AaruConsole.DebugWriteLine("UDIF plugin", "footer.sectorCount = {0}", footer.sectorCount);

            AaruConsole.DebugWriteLine("UDIF plugin", "footer.reserved1 is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved1));

            AaruConsole.DebugWriteLine("UDIF plugin", "footer.reserved2 is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved2));

            AaruConsole.DebugWriteLine("UDIF plugin", "footer.reserved3 is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved3));

            AaruConsole.DebugWriteLine("UDIF plugin", "footer.reserved4 is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(footer.reserved4));

            // Block chunks and headers
            List<byte[]> blkxList = new List<byte[]>();
            chunks = new Dictionary<ulong, BlockChunk>();

            bool fakeBlockChunks = false;

            byte[] vers = null;

            if(footer.plistLen    == 0 &&
               footer.rsrcForkLen != 0)
            {
                AaruConsole.DebugWriteLine("UDIF plugin", "Reading resource fork.");
                byte[] rsrcB = new byte[footer.rsrcForkLen];
                stream.Seek((long)footer.rsrcForkOff, SeekOrigin.Begin);
                stream.Read(rsrcB, 0, rsrcB.Length);

                var rsrc = new ResourceFork(rsrcB);

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

                if(versRez != null)
                    vers = versRez.GetResource(versRez.GetIds()[0]);
            }
            else if(footer.plistLen != 0)
            {
                AaruConsole.DebugWriteLine("UDIF plugin", "Reading property list.");
                byte[] plistB = new byte[footer.plistLen];
                stream.Seek((long)footer.plistOff, SeekOrigin.Begin);
                stream.Read(plistB, 0, plistB.Length);

                AaruConsole.DebugWriteLine("UDIF plugin", "Parsing property list.");
                var plist = (NSDictionary)XmlPropertyListParser.Parse(plistB);

                if(plist == null)
                    throw new Exception("Could not parse property list.");

                if(!plist.TryGetValue(RESOURCE_FORK_KEY, out NSObject rsrcObj))
                    throw new Exception("Could not retrieve resource fork.");

                var rsrc = (NSDictionary)rsrcObj;

                if(!rsrc.TryGetValue(BLOCK_KEY, out NSObject blkxObj))
                    throw new Exception("Could not retrieve block chunks array.");

                NSObject[] blkx = ((NSArray)blkxObj).GetArray();

                foreach(NSDictionary part in blkx.Cast<NSDictionary>())
                {
                    if(!part.TryGetValue("Name", out _))
                        throw new Exception("Could not retrieve Name");

                    if(!part.TryGetValue("Data", out NSObject dataObj))
                        throw new Exception("Could not retrieve Data");

                    blkxList.Add(((NSData)dataObj).Bytes);
                }

                if(rsrc.TryGetValue("vers", out NSObject versObj))
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
                var bChnk = new BlockChunk
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
                var version = new Version(vers);

                string release = null;
                string dev     = null;
                string pre     = null;

                string major = $"{version.MajorVersion}";
                string minor = $".{version.MinorVersion / 10}";

                if(version.MinorVersion % 10 > 0)
                    release = $".{version.MinorVersion % 10}";

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

                if(dev                       == null &&
                   version.PreReleaseVersion > 0)
                    dev = "f";

                if(dev != null)
                    pre = $"{version.PreReleaseVersion}";

                imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
                imageInfo.Application        = version.VersionString;
                imageInfo.Comments           = version.VersionMessage;

                if(version.MajorVersion == 3)
                    imageInfo.Application = "ShrinkWrap™";
                else if(version.MajorVersion == 6)
                    imageInfo.Application = "DiskCopy";
            }
            else
                imageInfo.Application = "DiskCopy";

            AaruConsole.DebugWriteLine("UDIF plugin", "Image application = {0} version {1}", imageInfo.Application,
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
                    var    bHdr  = new BlockHeader();
                    byte[] bHdrB = new byte[Marshal.SizeOf<BlockHeader>()];
                    Array.Copy(blkxBytes, 0, bHdrB, 0, Marshal.SizeOf<BlockHeader>());
                    bHdr = Marshal.ByteArrayToStructureBigEndian<BlockHeader>(bHdrB);

                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.signature = 0x{0:X8}", bHdr.signature);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.version = {0}", bHdr.version);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.sectorStart = {0}", bHdr.sectorStart);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.sectorCount = {0}", bHdr.sectorCount);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.dataOffset = {0}", bHdr.dataOffset);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.buffers = {0}", bHdr.buffers);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.descriptor = 0x{0:X8}", bHdr.descriptor);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved1 = {0}", bHdr.reserved1);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved2 = {0}", bHdr.reserved2);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved3 = {0}", bHdr.reserved3);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved4 = {0}", bHdr.reserved4);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved5 = {0}", bHdr.reserved5);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reserved6 = {0}", bHdr.reserved6);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.checksumType = {0}", bHdr.checksumType);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.checksumLen = {0}", bHdr.checksumLen);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.checksum = 0x{0:X8}", bHdr.checksum);
                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunks = {0}", bHdr.chunks);

                    AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.reservedChk is empty? = {0}",
                                               ArrayHelpers.ArrayIsNullOrEmpty(bHdr.reservedChk));

                    if(bHdr.buffers > buffersize)
                        buffersize = bHdr.buffers * SECTOR_SIZE;

                    for(int i = 0; i < bHdr.chunks; i++)
                    {
                        var    bChnk  = new BlockChunk();
                        byte[] bChnkB = new byte[Marshal.SizeOf<BlockChunk>()];

                        Array.Copy(blkxBytes, Marshal.SizeOf<BlockHeader>() + (Marshal.SizeOf<BlockChunk>() * i),
                                   bChnkB, 0, Marshal.SizeOf<BlockChunk>());

                        bChnk = Marshal.ByteArrayToStructureBigEndian<BlockChunk>(bChnkB);

                        AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].type = 0x{1:X8}", i, bChnk.type);
                        AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].comment = {1}", i, bChnk.comment);
                        AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sector = {1}", i, bChnk.sector);
                        AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].sectors = {1}", i, bChnk.sectors);
                        AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].offset = {1}", i, bChnk.offset);
                        AaruConsole.DebugWriteLine("UDIF plugin", "bHdr.chunk[{0}].length = {1}", i, bChnk.length);

                        if(bChnk.type == CHUNK_TYPE_END)
                            break;

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

                        if((bChnk.type > CHUNK_TYPE_NOCOPY && bChnk.type < CHUNK_TYPE_COMMNT) ||
                           (bChnk.type > CHUNK_TYPE_LZFSE  && bChnk.type < CHUNK_TYPE_END))
                            throw new ImageNotSupportedException($"Unsupported chunk type 0x{bChnk.type:X8} found");

                        if(bChnk.sectors > 0)
                            chunks.Add(bChnk.sector, bChnk);
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

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            var   readChunk        = new BlockChunk();
            bool  chunkFound       = false;
            ulong chunkStartSector = 0;

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
                    var    cmpMs     = new MemoryStream(cmpBuffer);
                    Stream decStream = null;

                    switch(readChunk.type)
                    {
                        case CHUNK_TYPE_ADC:
                            decStream = new ADCStream(cmpMs);

                            break;
                        case CHUNK_TYPE_ZLIB:
                            decStream = new ZlibStream(cmpMs, CompressionMode.Decompress);

                            break;
                        case CHUNK_TYPE_BZIP:
                            decStream = new BZip2Stream(cmpMs, SharpCompress.Compressors.CompressionMode.Decompress,
                                                        false);

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
                                var rle = new AppleRle(cmpMs);

                                for(int i = 0; i < buffersize; i++)
                                {
                                    int b = rle.ProduceByte();

                                    if(b == -1)
                                        break;

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
                        AaruConsole.WriteLine("zlib exception on chunk starting at sector {0}", readChunk.sector);

                        throw;
                    }
                #endif
                }

                sector = new byte[SECTOR_SIZE];
                Array.Copy(buffer, relOff, sector, 0, SECTOR_SIZE);

                if(sectorCache.Count >= MAX_CACHED_SECTORS)
                    sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);

                return sector;
            }

            switch(readChunk.type)
            {
                case CHUNK_TYPE_NOCOPY:
                case CHUNK_TYPE_ZERO:
                    sector = new byte[SECTOR_SIZE];

                    if(sectorCache.Count >= MAX_CACHED_SECTORS)
                        sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);

                    return sector;
                case CHUNK_TYPE_COPY:
                    imageStream.Seek((long)readChunk.offset + relOff, SeekOrigin.Begin);
                    sector = new byte[SECTOR_SIZE];
                    imageStream.Read(sector, 0, sector.Length);

                    if(sectorCache.Count >= MAX_CACHED_SECTORS)
                        sectorCache.Clear();

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

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}