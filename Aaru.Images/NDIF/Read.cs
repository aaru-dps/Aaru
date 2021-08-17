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
//     Reads Apple New Disk Image Format.
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
using Claunia.Encoding;
using Claunia.RsrcFork;
using SharpCompress.Compressors.ADC;
using Version = Resources.Version;

namespace Aaru.DiscImages
{
    public sealed partial class Ndif
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            if(!imageFilter.HasResourceFork() ||
               imageFilter.GetResourceForkLength() == 0)
                return false;

            ResourceFork rsrcFork;
            Resource     rsrc;
            short[]      bcems;

            try
            {
                rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());

                if(!rsrcFork.ContainsKey(NDIF_RESOURCE))
                    return false;

                rsrc = rsrcFork.GetResource(NDIF_RESOURCE);

                bcems = rsrc.GetIds();

                if(bcems        == null ||
                   bcems.Length == 0)
                    return false;
            }
            catch(InvalidCastException)
            {
                return false;
            }

            _imageInfo.Sectors = 0;

            foreach(byte[] bcem in bcems.Select(id => rsrc.GetResource(NDIF_RESOURCEID)))
            {
                if(bcem.Length < 128)
                    return false;

                _header = Marshal.ByteArrayToStructureBigEndian<ChunkHeader>(bcem);

                AaruConsole.DebugWriteLine("NDIF plugin", "footer.type = {0}", _header.version);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.driver = {0}", _header.driver);

                AaruConsole.DebugWriteLine("NDIF plugin", "footer.name = {0}",
                                           StringHandlers.PascalToString(_header.name,
                                                                         Encoding.GetEncoding("macintosh")));

                AaruConsole.DebugWriteLine("NDIF plugin", "footer.sectors = {0}", _header.sectors);

                AaruConsole.DebugWriteLine("NDIF plugin", "footer.maxSectorsPerChunk = {0}",
                                           _header.maxSectorsPerChunk);

                AaruConsole.DebugWriteLine("NDIF plugin", "footer.dataOffset = {0}", _header.dataOffset);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.crc = 0x{0:X7}", _header.crc);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.segmented = {0}", _header.segmented);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.p1 = 0x{0:X8}", _header.p1);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.p2 = 0x{0:X8}", _header.p2);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.unknown[0] = 0x{0:X8}", _header.unknown[0]);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.unknown[1] = 0x{0:X8}", _header.unknown[1]);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.unknown[2] = 0x{0:X8}", _header.unknown[2]);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.unknown[3] = 0x{0:X8}", _header.unknown[3]);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.unknown[4] = 0x{0:X8}", _header.unknown[4]);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.encrypted = {0}", _header.encrypted);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.hash = 0x{0:X8}", _header.hash);
                AaruConsole.DebugWriteLine("NDIF plugin", "footer.chunks = {0}", _header.chunks);

                // Block chunks and headers
                _chunks = new Dictionary<ulong, BlockChunk>();

                for(int i = 0; i < _header.chunks; i++)
                {
                    // Obsolete read-only NDIF only prepended the header and then put the image without any kind of block references.
                    // So let's falsify a block chunk
                    var    bChnk  = new BlockChunk();
                    byte[] sector = new byte[4];
                    Array.Copy(bcem, 128 + 0 + (i * 12), sector, 1, 3);
                    bChnk.sector = BigEndianBitConverter.ToUInt32(sector, 0);
                    bChnk.type   = bcem[128                                 + 3 + (i * 12)];
                    bChnk.offset = BigEndianBitConverter.ToUInt32(bcem, 128 + 4 + (i * 12));
                    bChnk.length = BigEndianBitConverter.ToUInt32(bcem, 128 + 8 + (i * 12));

                    AaruConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].type = 0x{1:X2}", i, bChnk.type);
                    AaruConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].sector = {1}", i, bChnk.sector);
                    AaruConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].offset = {1}", i, bChnk.offset);
                    AaruConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].length = {1}", i, bChnk.length);

                    if(bChnk.type == CHUNK_TYPE_END)
                        break;

                    bChnk.offset += _header.dataOffset;
                    bChnk.sector += (uint)_imageInfo.Sectors;

                    // TODO: Handle compressed chunks
                    switch(bChnk.type)
                    {
                        case CHUNK_TYPE_KENCODE:
                            throw new
                                ImageNotSupportedException("Chunks compressed with KenCode are not yet supported.");
                        case CHUNK_TYPE_LZH:
                            throw new ImageNotSupportedException("Chunks compressed with LZH are not yet supported.");
                        case CHUNK_TYPE_STUFFIT:
                            throw new
                                ImageNotSupportedException("Chunks compressed with StuffIt! are not yet supported.");
                    }

                    // TODO: Handle compressed chunks
                    if((bChnk.type > CHUNK_TYPE_COPY    && bChnk.type < CHUNK_TYPE_KENCODE) ||
                       (bChnk.type > CHUNK_TYPE_ADC     && bChnk.type < CHUNK_TYPE_STUFFIT) ||
                       (bChnk.type > CHUNK_TYPE_STUFFIT && bChnk.type < CHUNK_TYPE_END)     ||
                       bChnk.type == 1)
                        throw new ImageNotSupportedException($"Unsupported chunk type 0x{bChnk.type:X8} found");

                    _chunks.Add(bChnk.sector, bChnk);
                }

                _imageInfo.Sectors += _header.sectors;
            }

            if(_header.segmented > 0)
                throw new ImageNotSupportedException("Segmented images are not yet supported.");

            if(_header.encrypted > 0)
                throw new ImageNotSupportedException("Encrypted images are not yet supported.");

            switch(_imageInfo.Sectors)
            {
                case 1440:
                    _imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;

                    break;
                case 1600:
                    _imageInfo.MediaType = MediaType.AppleSonyDS;

                    break;
                case 2880:
                    _imageInfo.MediaType = MediaType.DOS_35_HD;

                    break;
                case 3360:
                    _imageInfo.MediaType = MediaType.DMF;

                    break;
                default:
                    _imageInfo.MediaType = MediaType.GENERIC_HDD;

                    break;
            }

            if(rsrcFork.ContainsKey(0x76657273))
            {
                Resource versRsrc = rsrcFork.GetResource(0x76657273);

                if(versRsrc != null)
                {
                    byte[] vers = versRsrc.GetResource(versRsrc.GetIds()[0]);

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

                    _imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
                    _imageInfo.Application        = version.VersionString;
                    _imageInfo.Comments           = version.VersionMessage;

                    if(version.MajorVersion == 3)
                        _imageInfo.Application = "ShrinkWrap™";
                    else if(version.MajorVersion == 6)
                        _imageInfo.Application = "DiskCopy";
                }
            }

            AaruConsole.DebugWriteLine("NDIF plugin", "Image application = {0} version {1}", _imageInfo.Application,
                                       _imageInfo.ApplicationVersion);

            _sectorCache           = new Dictionary<ulong, byte[]>();
            _chunkCache            = new Dictionary<ulong, byte[]>();
            _currentChunkCacheSize = 0;
            _imageStream           = imageFilter.GetDataForkStream();
            _bufferSize            = _header.maxSectorsPerChunk * SECTOR_SIZE;

            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            _imageInfo.MediaTitle = StringHandlers.PascalToString(_header.name, Encoding.GetEncoding("macintosh"));

            _imageInfo.SectorSize         = SECTOR_SIZE;
            _imageInfo.XmlMediaType       = XmlMediaType.BlockMedia;
            _imageInfo.ImageSize          = _imageInfo.Sectors * SECTOR_SIZE;
            _imageInfo.ApplicationVersion = "6";
            _imageInfo.Application        = "Apple DiskCopy";

            switch(_imageInfo.MediaType)
            {
                case MediaType.AppleSonyDS:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 10;

                    break;
                case MediaType.DOS_35_DS_DD_9:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case MediaType.DOS_35_HD:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 18;

                    break;
                case MediaType.DMF:
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.Heads           = 2;
                    _imageInfo.SectorsPerTrack = 21;

                    break;
                default:
                    _imageInfo.MediaType       = MediaType.GENERIC_HDD;
                    _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
                    _imageInfo.Heads           = 16;
                    _imageInfo.SectorsPerTrack = 63;

                    break;
            }

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(_sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            var   currentChunk     = new BlockChunk();
            bool  chunkFound       = false;
            ulong chunkStartSector = 0;

            foreach(KeyValuePair<ulong, BlockChunk> kvp in _chunks.Where(kvp => sectorAddress >= kvp.Key))
            {
                currentChunk     = kvp.Value;
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

            if((currentChunk.type & CHUNK_TYPE_COMPRESSED_MASK) == CHUNK_TYPE_COMPRESSED_MASK)
            {
                if(!_chunkCache.TryGetValue(chunkStartSector, out byte[] buffer))
                {
                    byte[] cmpBuffer = new byte[currentChunk.length];
                    _imageStream.Seek(currentChunk.offset, SeekOrigin.Begin);
                    _imageStream.Read(cmpBuffer, 0, cmpBuffer.Length);
                    var cmpMs = new MemoryStream(cmpBuffer);
                    int realSize;

                    switch(currentChunk.type)
                    {
                        case CHUNK_TYPE_ADC:
                        {
                            Stream decStream = new ADCStream(cmpMs);
                            byte[] tmpBuffer = new byte[_bufferSize];
                            realSize = decStream.Read(tmpBuffer, 0, (int)_bufferSize);
                            buffer   = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, buffer, 0, realSize);

                            break;
                        }

                        case CHUNK_TYPE_RLE:
                        {
                            byte[] tmpBuffer = new byte[_bufferSize];
                            realSize = 0;
                            var rle = new AppleRle(cmpMs);

                            for(int i = 0; i < _bufferSize; i++)
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

                        default:
                            throw new
                                ImageNotSupportedException($"Unsupported chunk type 0x{currentChunk.type:X8} found");
                    }

                    if(_currentChunkCacheSize + realSize > MAX_CACHE_SIZE)
                    {
                        _chunkCache.Clear();
                        _currentChunkCacheSize = 0;
                    }

                    _chunkCache.Add(chunkStartSector, buffer);
                    _currentChunkCacheSize += (uint)realSize;
                }

                sector = new byte[SECTOR_SIZE];
                Array.Copy(buffer, relOff, sector, 0, SECTOR_SIZE);

                if(_sectorCache.Count >= MAX_CACHED_SECTORS)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, sector);

                return sector;
            }

            switch(currentChunk.type)
            {
                case CHUNK_TYPE_NOCOPY:
                    sector = new byte[SECTOR_SIZE];

                    if(_sectorCache.Count >= MAX_CACHED_SECTORS)
                        _sectorCache.Clear();

                    _sectorCache.Add(sectorAddress, sector);

                    return sector;
                case CHUNK_TYPE_COPY:
                    _imageStream.Seek(currentChunk.offset + relOff, SeekOrigin.Begin);
                    sector = new byte[SECTOR_SIZE];
                    _imageStream.Read(sector, 0, sector.Length);

                    if(_sectorCache.Count >= MAX_CACHED_SECTORS)
                        _sectorCache.Clear();

                    _sectorCache.Add(sectorAddress, sector);

                    return sector;
            }

            throw new ImageNotSupportedException($"Unsupported chunk type 0x{currentChunk.type:X8} found");
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
    }
}