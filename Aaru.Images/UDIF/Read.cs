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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Compression.Apple;
using Aaru.Helpers;
using Aaru.Logging;
using Claunia.PropertyList;
using Claunia.RsrcFork;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.Xz;
using ADC = Aaru.Compression.Apple.ADC;
using Version = Resources.Version;

#pragma warning disable 612

namespace Aaru.Images;

public sealed partial class Udif
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < 512) return ErrorNumber.InvalidArgument;

        stream.Seek(-Marshal.SizeOf<Footer>(), SeekOrigin.End);
        var footerB = new byte[Marshal.SizeOf<Footer>()];

        stream.EnsureRead(footerB, 0, Marshal.SizeOf<Footer>());
        _footer = Marshal.ByteArrayToStructureBigEndian<Footer>(footerB);

        if(_footer.signature != UDIF_SIGNATURE)
        {
            stream.Seek(0, SeekOrigin.Begin);
            footerB = new byte[Marshal.SizeOf<Footer>()];

            stream.EnsureRead(footerB, 0, Marshal.SizeOf<Footer>());
            _footer = Marshal.ByteArrayToStructureBigEndian<Footer>(footerB);

            if(_footer.signature != UDIF_SIGNATURE)
            {
                AaruLogging.Error(Localization.Unable_to_find_UDIF_signature);

                return ErrorNumber.InvalidArgument;
            }

            AaruLogging.Verbose(Localization.Found_obsolete_UDIF_format);
        }

        AaruLogging.Debug(MODULE_NAME, "footer.signature = 0x{0:X8}",     _footer.signature);
        AaruLogging.Debug(MODULE_NAME, "footer.version = {0}",            _footer.version);
        AaruLogging.Debug(MODULE_NAME, "footer.headerSize = {0}",         _footer.headerSize);
        AaruLogging.Debug(MODULE_NAME, "footer.flags = {0}",              _footer.flags);
        AaruLogging.Debug(MODULE_NAME, "footer.runningDataForkOff = {0}", _footer.runningDataForkOff);
        AaruLogging.Debug(MODULE_NAME, "footer.dataForkOff = {0}",        _footer.dataForkOff);
        AaruLogging.Debug(MODULE_NAME, "footer.dataForkLen = {0}",        _footer.dataForkLen);
        AaruLogging.Debug(MODULE_NAME, "footer.rsrcForkOff = {0}",        _footer.rsrcForkOff);
        AaruLogging.Debug(MODULE_NAME, "footer.rsrcForkLen = {0}",        _footer.rsrcForkLen);
        AaruLogging.Debug(MODULE_NAME, "footer.segmentNumber = {0}",      _footer.segmentNumber);
        AaruLogging.Debug(MODULE_NAME, "footer.segmentCount = {0}",       _footer.segmentCount);
        AaruLogging.Debug(MODULE_NAME, "footer.segmentId = {0}",          _footer.segmentId);
        AaruLogging.Debug(MODULE_NAME, "footer.dataForkChkType = {0}",    _footer.dataForkChkType);
        AaruLogging.Debug(MODULE_NAME, "footer.dataForkLen = {0}",        _footer.dataForkLen);
        AaruLogging.Debug(MODULE_NAME, "footer.dataForkChk = 0x{0:X8}",   _footer.dataForkChk);
        AaruLogging.Debug(MODULE_NAME, "footer.plistOff = {0}",           _footer.plistOff);
        AaruLogging.Debug(MODULE_NAME, "footer.plistLen = {0}",           _footer.plistLen);
        AaruLogging.Debug(MODULE_NAME, "footer.masterChkType = {0}",      _footer.masterChkType);
        AaruLogging.Debug(MODULE_NAME, "footer.masterChkLen = {0}",       _footer.masterChkLen);
        AaruLogging.Debug(MODULE_NAME, "footer.masterChk = 0x{0:X8}",     _footer.masterChk);
        AaruLogging.Debug(MODULE_NAME, "footer.imageVariant = {0}",       _footer.imageVariant);
        AaruLogging.Debug(MODULE_NAME, "footer.sectorCount = {0}",        _footer.sectorCount);

        AaruLogging.Debug(MODULE_NAME,
                          "footer.reserved1 is empty? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved1));

        AaruLogging.Debug(MODULE_NAME,
                          "footer.reserved2 is empty? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved2));

        AaruLogging.Debug(MODULE_NAME,
                          "footer.reserved3 is empty? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved3));

        AaruLogging.Debug(MODULE_NAME,
                          "footer.reserved4 is empty? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved4));

        // Block chunks and headers
        List<byte[]> blkxList = [];
        _chunks = new Dictionary<ulong, BlockChunk>();

        byte[] vers = null;

        if(_footer.plistLen == 0 && _footer.rsrcForkLen != 0)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.Reading_resource_fork);
            var rsrcB = new byte[_footer.rsrcForkLen];
            stream.Seek((long)_footer.rsrcForkOff, SeekOrigin.Begin);
            stream.EnsureRead(rsrcB, 0, rsrcB.Length);

            var rsrc = new ResourceFork(rsrcB);

            if(!rsrc.ContainsKey(BLOCK_OS_TYPE))
            {
                AaruLogging.Error(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            Resource blkxRez = rsrc.GetResource(BLOCK_OS_TYPE);

            if(blkxRez == null)
            {
                AaruLogging.Error(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            if(blkxRez.GetIds().Length == 0)
            {
                AaruLogging.Error(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            blkxList.AddRange(blkxRez.GetIds().Select(blkxId => blkxRez.GetResource(blkxId)));

            Resource versRez = rsrc.GetResource(0x76657273);

            if(versRez != null) vers = versRez.GetResource(versRez.GetIds()[0]);
        }
        else if(_footer.plistLen != 0)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.Reading_property_list);
            var plistB = new byte[_footer.plistLen];
            stream.Seek((long)_footer.plistOff, SeekOrigin.Begin);
            stream.EnsureRead(plistB, 0, plistB.Length);

            AaruLogging.Debug(MODULE_NAME, Localization.Parsing_property_list);
            var plist = (NSDictionary)XmlPropertyListParser.Parse(plistB);

            if(plist == null)
            {
                AaruLogging.Error(Localization.Could_not_parse_property_list);

                return ErrorNumber.InOutError;
            }

            if(!plist.TryGetValue(RESOURCE_FORK_KEY, out NSObject rsrcObj))
            {
                AaruLogging.Error(Localization.Could_not_retrieve_resource_fork);

                return ErrorNumber.InOutError;
            }

            var rsrc = (NSDictionary)rsrcObj;

            if(!rsrc.TryGetValue(BLOCK_KEY, out NSObject blkxObj))
            {
                AaruLogging.Error(Localization.Could_not_retrieve_block_chunks_array);

                return ErrorNumber.InOutError;
            }

            NSObject[] blkx = ((NSArray)blkxObj).GetArray();

            foreach(NSDictionary part in blkx.Cast<NSDictionary>())
            {
                if(!part.TryGetValue("Name", out _))
                {
                    AaruLogging.Error(Localization.Could_not_retrieve_Name);

                    return ErrorNumber.InOutError;
                }

                if(!part.TryGetValue("Data", out NSObject dataObj))
                {
                    AaruLogging.Error(Localization.Could_not_retrieve_Data);

                    return ErrorNumber.InOutError;
                }

                blkxList.Add(((NSData)dataObj).Bytes);
            }

            if(rsrc.TryGetValue("vers", out NSObject versObj))
            {
                NSObject[] versArray = ((NSArray)versObj).GetArray();

                if(versArray.Length >= 1) vers = ((NSData)versArray[0]).Bytes;
            }
        }
        else
        {
            if(imageFilter.ResourceForkLength == 0)
            {
                AaruLogging.Error(Localization.This_image_needs_the_resource_fork_to_work);

                return ErrorNumber.InvalidArgument;
            }

            AaruLogging.Debug(MODULE_NAME, Localization.Reading_resource_fork);
            Stream rsrcStream = imageFilter.GetResourceForkStream();

            var rsrcB = new byte[rsrcStream.Length];
            rsrcStream.Position = 0;
            rsrcStream.EnsureRead(rsrcB, 0, rsrcB.Length);

            var rsrc = new ResourceFork(rsrcB);

            if(!rsrc.ContainsKey(BLOCK_OS_TYPE))
            {
                AaruLogging.Error(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            Resource blkxRez = rsrc.GetResource(BLOCK_OS_TYPE);

            if(blkxRez == null)
            {
                AaruLogging.Error(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            if(blkxRez.GetIds().Length == 0)
            {
                AaruLogging.Error(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            blkxList.AddRange(blkxRez.GetIds().Select(blkxId => blkxRez.GetResource(blkxId)));

            Resource versRez = rsrc.GetResource(0x76657273);

            if(versRez != null) vers = versRez.GetResource(versRez.GetIds()[0]);
        }

        if(vers != null)
        {
            var version = new Version(vers);

            string release = null;
            string pre     = null;

            var major = $"{version.MajorVersion}";
            var minor = $".{version.MinorVersion / 10}";

            if(version.MinorVersion % 10 > 0) release = $".{version.MinorVersion % 10}";

            string dev = version.DevStage switch
                         {
                             Version.DevelopmentStage.Alpha    => "a",
                             Version.DevelopmentStage.Beta     => "b",
                             Version.DevelopmentStage.PreAlpha => "d",
                             _                                 => null
                         };

            if(dev == null && version.PreReleaseVersion > 0) dev = "f";

            if(dev != null) pre = $"{version.PreReleaseVersion}";

            _imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
            _imageInfo.Application        = version.VersionString;
            _imageInfo.Comments           = version.VersionMessage;

            _imageInfo.Application = version.MajorVersion switch
                                     {
                                         3 => "ShrinkWrap™",
                                         6 => "DiskCopy",
                                         _ => _imageInfo.Application
                                     };
        }
        else
            _imageInfo.Application = "DiskCopy";

        AaruLogging.Debug(MODULE_NAME,
                          Localization.Image_application_0_version_1,
                          _imageInfo.Application,
                          _imageInfo.ApplicationVersion);

        _imageInfo.Sectors = 0;

        if(blkxList.Count == 0)
        {
            AaruLogging.Error(Localization.Could_not_retrieve_block_chunks);

            return ErrorNumber.InvalidArgument;
        }

        _buffersize = 0;

        foreach(byte[] blkxBytes in blkxList)
        {
            if(blkxBytes.Length < Marshal.SizeOf<BlockHeader>()) return ErrorNumber.InvalidArgument;

            var bHdrB = new byte[Marshal.SizeOf<BlockHeader>()];
            Array.Copy(blkxBytes, 0, bHdrB, 0, Marshal.SizeOf<BlockHeader>());
            BlockHeader bHdr = Marshal.ByteArrayToStructureBigEndian<BlockHeader>(bHdrB);

            AaruLogging.Debug(MODULE_NAME, "bHdr.signature = 0x{0:X8}",  bHdr.signature);
            AaruLogging.Debug(MODULE_NAME, "bHdr.version = {0}",         bHdr.version);
            AaruLogging.Debug(MODULE_NAME, "bHdr.sectorStart = {0}",     bHdr.sectorStart);
            AaruLogging.Debug(MODULE_NAME, "bHdr.sectorCount = {0}",     bHdr.sectorCount);
            AaruLogging.Debug(MODULE_NAME, "bHdr.dataOffset = {0}",      bHdr.dataOffset);
            AaruLogging.Debug(MODULE_NAME, "bHdr.buffers = {0}",         bHdr.buffers);
            AaruLogging.Debug(MODULE_NAME, "bHdr.descriptor = 0x{0:X8}", bHdr.descriptor);
            AaruLogging.Debug(MODULE_NAME, "bHdr.reserved1 = {0}",       bHdr.reserved1);
            AaruLogging.Debug(MODULE_NAME, "bHdr.reserved2 = {0}",       bHdr.reserved2);
            AaruLogging.Debug(MODULE_NAME, "bHdr.reserved3 = {0}",       bHdr.reserved3);
            AaruLogging.Debug(MODULE_NAME, "bHdr.reserved4 = {0}",       bHdr.reserved4);
            AaruLogging.Debug(MODULE_NAME, "bHdr.reserved5 = {0}",       bHdr.reserved5);
            AaruLogging.Debug(MODULE_NAME, "bHdr.reserved6 = {0}",       bHdr.reserved6);
            AaruLogging.Debug(MODULE_NAME, "bHdr.checksumType = {0}",    bHdr.checksumType);
            AaruLogging.Debug(MODULE_NAME, "bHdr.checksumLen = {0}",     bHdr.checksumLen);
            AaruLogging.Debug(MODULE_NAME, "bHdr.checksum = 0x{0:X8}",   bHdr.checksum);
            AaruLogging.Debug(MODULE_NAME, "bHdr.chunks = {0}",          bHdr.chunks);

            AaruLogging.Debug(MODULE_NAME,
                              "bHdr.reservedChk is empty? = {0}",
                              ArrayHelpers.ArrayIsNullOrEmpty(bHdr.reservedChk));

            // buffers is a sector count, buffersize is in bytes
            if(bHdr.buffers * SECTOR_SIZE > _buffersize) _buffersize = bHdr.buffers * SECTOR_SIZE;

            if(blkxBytes.Length < Marshal.SizeOf<BlockHeader>() + (long)bHdr.chunks * Marshal.SizeOf<BlockChunk>())
                return ErrorNumber.InvalidArgument;

            for(var i = 0; i < bHdr.chunks; i++)
            {
                var bChnkB = new byte[Marshal.SizeOf<BlockChunk>()];

                Array.Copy(blkxBytes,
                           Marshal.SizeOf<BlockHeader>() + Marshal.SizeOf<BlockChunk>() * i,
                           bChnkB,
                           0,
                           Marshal.SizeOf<BlockChunk>());

                BlockChunk bChnk = Marshal.ByteArrayToStructureBigEndian<BlockChunk>(bChnkB);

                AaruLogging.Debug(MODULE_NAME, "bHdr.chunk[{0}].type = 0x{1:X8}", i, bChnk.type);
                AaruLogging.Debug(MODULE_NAME, "bHdr.chunk[{0}].comment = {1}",   i, bChnk.comment);
                AaruLogging.Debug(MODULE_NAME, "bHdr.chunk[{0}].sector = {1}",    i, bChnk.sector);
                AaruLogging.Debug(MODULE_NAME, "bHdr.chunk[{0}].sectors = {1}",   i, bChnk.sectors);
                AaruLogging.Debug(MODULE_NAME, "bHdr.chunk[{0}].offset = {1}",    i, bChnk.offset);
                AaruLogging.Debug(MODULE_NAME, "bHdr.chunk[{0}].length = {1}",    i, bChnk.length);

                if(bChnk.type == CHUNK_TYPE_END) break;

                _imageInfo.Sectors += bChnk.sectors;

                // Chunk offset is relative
                bChnk.sector += bHdr.sectorStart;
                bChnk.offset += bHdr.dataOffset;

                switch(bChnk.type)
                {
                    // TODO: Handle comments
                    case CHUNK_TYPE_COMMNT:
                        continue;

                    case CHUNK_TYPE_LZFSE when !LZFSE.IsSupported:
                        AaruLogging.Error(Localization.Chunks_compressed_with_lzfse_are_not_yet_supported);

                        return ErrorNumber.NotImplemented;
                }

                if(bChnk.type is > CHUNK_TYPE_NOCOPY and < CHUNK_TYPE_COMMNT or > CHUNK_TYPE_LZMA and < CHUNK_TYPE_END)
                {
                    AaruLogging.Error(string.Format(Localization.Unsupported_chunk_type_0_found, bChnk.type));

                    return ErrorNumber.InvalidArgument;
                }

                if(bChnk.sectors > 0) _chunks.Add(bChnk.sector, bChnk);
            }
        }

        _chunkStartSectors = _chunks.Keys.ToArray();
        Array.Sort(_chunkStartSectors);
        _sectorCache           = new Dictionary<ulong, byte[]>();
        _chunkCache            = new Dictionary<ulong, byte[]>();
        _currentChunkCacheSize = 0;
        _imageStream           = stream;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.SectorSize           = SECTOR_SIZE;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = _imageInfo.Sectors * SECTOR_SIZE;
        _imageInfo.Version              = $"{_footer.version}";

        _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
        _imageInfo.Heads           = 16;
        _imageInfo.SectorsPerTrack = 63;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        sectorStatus = SectorStatus.Dumped;

        if(_sectorCache.TryGetValue(sectorAddress, out buffer)) return ErrorNumber.NoError;

        var   readChunk        = new BlockChunk();
        var   chunkFound       = false;
        ulong chunkStartSector = 0;

        // Binary search for the greatest chunk start at or below the address
        int idx = Array.BinarySearch(_chunkStartSectors, sectorAddress);

        if(idx < 0) idx = ~idx - 1;

        if(idx >= 0 && _chunks.TryGetValue(_chunkStartSectors[idx], out BlockChunk value))
        {
            readChunk        = value;
            chunkFound       = true;
            chunkStartSector = _chunkStartSectors[idx];
        }

        long relOff = ((long)sectorAddress - (long)chunkStartSector) * SECTOR_SIZE;

        if(relOff < 0) return ErrorNumber.InvalidArgument;

        if(!chunkFound) return ErrorNumber.SectorNotFound;

        if((readChunk.type & CHUNK_TYPE_COMPRESSED_MASK) == CHUNK_TYPE_COMPRESSED_MASK)
        {
            if(!_chunkCache.TryGetValue(chunkStartSector, out byte[] data))
            {
                var cmpBuffer = new byte[readChunk.length];
                _imageStream.Seek((long)(readChunk.offset + _footer.dataForkOff), SeekOrigin.Begin);
                _imageStream.EnsureRead(cmpBuffer, 0, cmpBuffer.Length);
                var    cmpMs     = new MemoryStream(cmpBuffer);
                Stream decStream = null;

                switch(readChunk.type)
                {
                    case CHUNK_TYPE_ZLIB:
                        decStream = new ZlibStream(cmpMs, CompressionMode.Decompress);

                        break;
                    case CHUNK_TYPE_BZIP:
                    case CHUNK_TYPE_ADC:
                    case CHUNK_TYPE_RLE:
                    case CHUNK_TYPE_LZH:
                    case CHUNK_TYPE_KENCODE:
                    case CHUNK_TYPE_LZFSE:
                        break;
                    case CHUNK_TYPE_LZMA:
                        decStream = new XZStream(cmpMs);

                        break;

                    default:
                        return ErrorNumber.NotImplemented;
                }

#if DEBUG
                try
                {
#endif
                    byte[] tmpBuffer;
                    var    realSize = 0;

                    switch(readChunk.type)
                    {
                        case CHUNK_TYPE_ZLIB:
                        case CHUNK_TYPE_LZMA:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = decStream?.EnsureRead(tmpBuffer, 0, (int)_buffersize) ?? 0;
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                        case CHUNK_TYPE_BZIP:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = BZip2.DecodeBuffer(cmpBuffer, tmpBuffer);
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                        case CHUNK_TYPE_ADC:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = ADC.DecodeBuffer(cmpBuffer, tmpBuffer);
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                        case CHUNK_TYPE_RLE:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = Rle.DecodeBuffer(cmpBuffer, tmpBuffer);
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                        case CHUNK_TYPE_LZH:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = Lzh.DecodeBuffer(cmpBuffer, tmpBuffer);
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                        case CHUNK_TYPE_KENCODE:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = KenCode.DecodeBuffer(cmpBuffer, tmpBuffer);
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                        case CHUNK_TYPE_LZFSE:
                            tmpBuffer = new byte[_buffersize];
                            realSize  = LZFSE.DecodeBuffer(cmpBuffer, tmpBuffer);
                            data      = new byte[realSize];
                            Array.Copy(tmpBuffer, 0, data, 0, realSize);

                            break;
                    }

                    if(_currentChunkCacheSize + realSize > MAX_CACHE_SIZE)
                    {
                        _chunkCache.Clear();
                        _currentChunkCacheSize = 0;
                    }

                    _chunkCache.Add(chunkStartSector, data);
                    _currentChunkCacheSize += (uint)realSize;

#if DEBUG
                }
                catch(ZlibException)
                {
                    AaruLogging.WriteLine(Localization.zlib_exception_on_chunk_starting_at_sector_0, readChunk.sector);

                    throw;
                }
#endif
            }

            buffer = new byte[SECTOR_SIZE];

            // Shall not happen
            if(data is null) return ErrorNumber.InvalidArgument;

            Array.Copy(data, relOff, buffer, 0, SECTOR_SIZE);

            if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

            _sectorCache.Add(sectorAddress, buffer);

            return ErrorNumber.NoError;
        }

        switch(readChunk.type)
        {
            case CHUNK_TYPE_NOCOPY:
            case CHUNK_TYPE_ZERO:
                buffer = new byte[SECTOR_SIZE];

                if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, buffer);

                return ErrorNumber.NoError;
            case CHUNK_TYPE_COPY:
                _imageStream.Seek((long)(readChunk.offset + (ulong)relOff + _footer.dataForkOff), SeekOrigin.Begin);
                buffer = new byte[SECTOR_SIZE];
                _imageStream.EnsureRead(buffer, 0, buffer.Length);

                if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, buffer);

                return ErrorNumber.NoError;
        }

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, false, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}