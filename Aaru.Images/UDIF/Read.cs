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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.PropertyList;
using Claunia.RsrcFork;
using Ionic.Zlib;
using SharpCompress.Compressors.Xz;
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
                AaruConsole.ErrorWriteLine(Localization.Unable_to_find_UDIF_signature);

                return ErrorNumber.InvalidArgument;
            }

            AaruConsole.VerboseWriteLine(Localization.Found_obsolete_UDIF_format);
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.signature = 0x{0:X8}",     _footer.signature);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.version = {0}",            _footer.version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.headerSize = {0}",         _footer.headerSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.flags = {0}",              _footer.flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.runningDataForkOff = {0}", _footer.runningDataForkOff);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.dataForkOff = {0}",        _footer.dataForkOff);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.dataForkLen = {0}",        _footer.dataForkLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.rsrcForkOff = {0}",        _footer.rsrcForkOff);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.rsrcForkLen = {0}",        _footer.rsrcForkLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.segmentNumber = {0}",      _footer.segmentNumber);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.segmentCount = {0}",       _footer.segmentCount);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.segmentId = {0}",          _footer.segmentId);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.dataForkChkType = {0}",    _footer.dataForkChkType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.dataForkLen = {0}",        _footer.dataForkLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.dataForkChk = 0x{0:X8}",   _footer.dataForkChk);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.plistOff = {0}",           _footer.plistOff);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.plistLen = {0}",           _footer.plistLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.masterChkType = {0}",      _footer.masterChkType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.masterChkLen = {0}",       _footer.masterChkLen);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.masterChk = 0x{0:X8}",     _footer.masterChk);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.imageVariant = {0}",       _footer.imageVariant);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.sectorCount = {0}",        _footer.sectorCount);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "footer.reserved1 is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved1));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "footer.reserved2 is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved2));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "footer.reserved3 is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved3));

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "footer.reserved4 is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(_footer.reserved4));

        // Block chunks and headers
        List<byte[]> blkxList = [];
        _chunks = new Dictionary<ulong, BlockChunk>();

        byte[] vers = null;

        if(_footer.plistLen == 0 && _footer.rsrcForkLen != 0)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_resource_fork);
            var rsrcB = new byte[_footer.rsrcForkLen];
            stream.Seek((long)_footer.rsrcForkOff, SeekOrigin.Begin);
            stream.EnsureRead(rsrcB, 0, rsrcB.Length);

            var rsrc = new ResourceFork(rsrcB);

            if(!rsrc.ContainsKey(BLOCK_OS_TYPE))
            {
                AaruConsole.ErrorWriteLine(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            Resource blkxRez = rsrc.GetResource(BLOCK_OS_TYPE);

            if(blkxRez == null)
            {
                AaruConsole.ErrorWriteLine(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            if(blkxRez.GetIds().Length == 0)
            {
                AaruConsole.ErrorWriteLine(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            blkxList.AddRange(blkxRez.GetIds().Select(blkxId => blkxRez.GetResource(blkxId)));

            Resource versRez = rsrc.GetResource(0x76657273);

            if(versRez != null) vers = versRez.GetResource(versRez.GetIds()[0]);
        }
        else if(_footer.plistLen != 0)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_property_list);
            var plistB = new byte[_footer.plistLen];
            stream.Seek((long)_footer.plistOff, SeekOrigin.Begin);
            stream.EnsureRead(plistB, 0, plistB.Length);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Parsing_property_list);
            var plist = (NSDictionary)XmlPropertyListParser.Parse(plistB);

            if(plist == null)
            {
                AaruConsole.ErrorWriteLine(Localization.Could_not_parse_property_list);

                return ErrorNumber.InOutError;
            }

            if(!plist.TryGetValue(RESOURCE_FORK_KEY, out NSObject rsrcObj))
            {
                AaruConsole.ErrorWriteLine(Localization.Could_not_retrieve_resource_fork);

                return ErrorNumber.InOutError;
            }

            var rsrc = (NSDictionary)rsrcObj;

            if(!rsrc.TryGetValue(BLOCK_KEY, out NSObject blkxObj))
            {
                AaruConsole.ErrorWriteLine(Localization.Could_not_retrieve_block_chunks_array);

                return ErrorNumber.InOutError;
            }

            NSObject[] blkx = ((NSArray)blkxObj).GetArray();

            foreach(NSDictionary part in blkx.Cast<NSDictionary>())
            {
                if(!part.TryGetValue("Name", out _))
                {
                    AaruConsole.ErrorWriteLine(Localization.Could_not_retrieve_Name);

                    return ErrorNumber.InOutError;
                }

                if(!part.TryGetValue("Data", out NSObject dataObj))
                {
                    AaruConsole.ErrorWriteLine(Localization.Could_not_retrieve_Data);

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
                AaruConsole.ErrorWriteLine(Localization.This_image_needs_the_resource_fork_to_work);

                return ErrorNumber.InvalidArgument;
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_resource_fork);
            Stream rsrcStream = imageFilter.GetResourceForkStream();

            var rsrcB = new byte[rsrcStream.Length];
            rsrcStream.Position = 0;
            rsrcStream.EnsureRead(rsrcB, 0, rsrcB.Length);

            var rsrc = new ResourceFork(rsrcB);

            if(!rsrc.ContainsKey(BLOCK_OS_TYPE))
            {
                AaruConsole.ErrorWriteLine(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            Resource blkxRez = rsrc.GetResource(BLOCK_OS_TYPE);

            if(blkxRez == null)
            {
                AaruConsole.ErrorWriteLine(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

                return ErrorNumber.InvalidArgument;
            }

            if(blkxRez.GetIds().Length == 0)
            {
                AaruConsole.ErrorWriteLine(Localization.Image_resource_fork_doesnt_contain_UDIF_block_chunks);

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

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   Localization.Image_application_0_version_1,
                                   _imageInfo.Application,
                                   _imageInfo.ApplicationVersion);

        _imageInfo.Sectors = 0;

        if(blkxList.Count == 0)
        {
            AaruConsole.ErrorWriteLine(Localization.Could_not_retrieve_block_chunks);

            return ErrorNumber.InvalidArgument;
        }

        _buffersize = 0;

        foreach(byte[] blkxBytes in blkxList)
        {
            var bHdrB = new byte[Marshal.SizeOf<BlockHeader>()];
            Array.Copy(blkxBytes, 0, bHdrB, 0, Marshal.SizeOf<BlockHeader>());
            BlockHeader bHdr = Marshal.ByteArrayToStructureBigEndian<BlockHeader>(bHdrB);

            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.signature = 0x{0:X8}",  bHdr.signature);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.version = {0}",         bHdr.version);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.sectorStart = {0}",     bHdr.sectorStart);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.sectorCount = {0}",     bHdr.sectorCount);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.dataOffset = {0}",      bHdr.dataOffset);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.buffers = {0}",         bHdr.buffers);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.descriptor = 0x{0:X8}", bHdr.descriptor);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.reserved1 = {0}",       bHdr.reserved1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.reserved2 = {0}",       bHdr.reserved2);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.reserved3 = {0}",       bHdr.reserved3);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.reserved4 = {0}",       bHdr.reserved4);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.reserved5 = {0}",       bHdr.reserved5);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.reserved6 = {0}",       bHdr.reserved6);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.checksumType = {0}",    bHdr.checksumType);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.checksumLen = {0}",     bHdr.checksumLen);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.checksum = 0x{0:X8}",   bHdr.checksum);
            AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunks = {0}",          bHdr.chunks);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "bHdr.reservedChk is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(bHdr.reservedChk));

            if(bHdr.buffers > _buffersize) _buffersize = bHdr.buffers * SECTOR_SIZE;

            for(var i = 0; i < bHdr.chunks; i++)
            {
                var bChnkB = new byte[Marshal.SizeOf<BlockChunk>()];

                Array.Copy(blkxBytes,
                           Marshal.SizeOf<BlockHeader>() + Marshal.SizeOf<BlockChunk>() * i,
                           bChnkB,
                           0,
                           Marshal.SizeOf<BlockChunk>());

                BlockChunk bChnk = Marshal.ByteArrayToStructureBigEndian<BlockChunk>(bChnkB);

                AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunk[{0}].type = 0x{1:X8}", i, bChnk.type);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunk[{0}].comment = {1}",   i, bChnk.comment);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunk[{0}].sector = {1}",    i, bChnk.sector);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunk[{0}].sectors = {1}",   i, bChnk.sectors);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunk[{0}].offset = {1}",    i, bChnk.offset);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bHdr.chunk[{0}].length = {1}",    i, bChnk.length);

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

                    // TODO: Handle compressed chunks
                    case CHUNK_TYPE_KENCODE:
                        AaruConsole.ErrorWriteLine(Localization.Chunks_compressed_with_KenCode_are_not_yet_supported);

                        return ErrorNumber.NotImplemented;
                    case CHUNK_TYPE_LZH:
                        AaruConsole.ErrorWriteLine(Localization.Chunks_compressed_with_LZH_are_not_yet_supported);

                        return ErrorNumber.NotImplemented;
                    case CHUNK_TYPE_LZFSE when !LZFSE.IsSupported:
                        AaruConsole.ErrorWriteLine(Localization.Chunks_compressed_with_lzfse_are_not_yet_supported);

                        return ErrorNumber.NotImplemented;
                }

                if(bChnk.type is > CHUNK_TYPE_NOCOPY and < CHUNK_TYPE_COMMNT or > CHUNK_TYPE_LZMA and < CHUNK_TYPE_END)
                {
                    AaruConsole.ErrorWriteLine(string.Format(Localization.Unsupported_chunk_type_0_found, bChnk.type));

                    return ErrorNumber.InvalidArgument;
                }

                if(bChnk.sectors > 0) _chunks.Add(bChnk.sector, bChnk);
            }
        }

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
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(_sectorCache.TryGetValue(sectorAddress, out buffer)) return ErrorNumber.NoError;

        var   readChunk        = new BlockChunk();
        var   chunkFound       = false;
        ulong chunkStartSector = 0;

        foreach(KeyValuePair<ulong, BlockChunk> kvp in _chunks.Where(kvp => sectorAddress >= kvp.Key))
        {
            readChunk        = kvp.Value;
            chunkFound       = true;
            chunkStartSector = kvp.Key;
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
                            realSize  = decStream?.Read(tmpBuffer, 0, (int)_buffersize) ?? 0;
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
                            realSize  = AppleRle.DecodeBuffer(cmpBuffer, tmpBuffer);
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
                    AaruConsole.WriteLine(Localization.zlib_exception_on_chunk_starting_at_sector_0, readChunk.sector);

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
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}