// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for MAME Compressed Hunks of Data disk images.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Helpers;
using Aaru.Logging;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace Aaru.Images;

public sealed partial class Chd
{
    Track GetTrack(ulong sector)
    {
        var track = new Track();

        foreach(KeyValuePair<ulong, uint> kvp in _offsetmap.Where(kvp => sector >= kvp.Key))
            _tracks.TryGetValue(kvp.Value, out track);

        return track;
    }

    ulong GetAbsoluteSector(ulong relativeSector, uint track)
    {
        _tracks.TryGetValue(track, out Track aaruTrack);

        return (aaruTrack?.StartSector ?? 0) + relativeSector;
    }

    ErrorNumber GetHunk(ulong hunkNo, out byte[] buffer)
    {
        if(_hunkCache.TryGetValue(hunkNo, out buffer)) return ErrorNumber.NoError;

        switch(_mapVersion)
        {
            case 1:
                ulong offset = _hunkTable[hunkNo] & 0x00000FFFFFFFFFFF;
                ulong length = _hunkTable[hunkNo] >> 44;

                var compHunk = new byte[length];
                _imageStream.Seek((long)offset, SeekOrigin.Begin);
                _imageStream.EnsureRead(compHunk, 0, compHunk.Length);

                if(length == _sectorsPerHunk * _imageInfo.SectorSize)
                    buffer = compHunk;
                else if((Compression)_hdrCompression > Compression.Zlib)
                {
                    AaruLogging.Error(string.Format(Localization.Unsupported_compression_0,
                                                    (Compression)_hdrCompression));

                    return ErrorNumber.InvalidArgument;
                }
                else
                {
                    var zStream = new DeflateStream(new MemoryStream(compHunk), CompressionMode.Decompress);
                    buffer = new byte[_sectorsPerHunk * _imageInfo.SectorSize];
                    int read = zStream.EnsureRead(buffer, 0, (int)(_sectorsPerHunk * _imageInfo.SectorSize));

                    if(read != _sectorsPerHunk * _imageInfo.SectorSize)
                    {
                        AaruLogging.Error(string.Format(Localization
                                                           .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                        read,
                                                        _sectorsPerHunk * _imageInfo.SectorSize));

                        return ErrorNumber.InOutError;
                    }

                    zStream.Close();
                }

                break;
            case 3:
                var entryBytes = new byte[16];
                Array.Copy(_hunkMap, (int)(hunkNo * 16), entryBytes, 0, 16);
                MapEntryV3 entry = Marshal.ByteArrayToStructureBigEndian<MapEntryV3>(entryBytes);

                switch((EntryFlagsV3)(entry.flags & 0x0F))
                {
                    case EntryFlagsV3.Invalid:
                        AaruLogging.Error(Localization.Invalid_hunk_found);

                        return ErrorNumber.InvalidArgument;
                    case EntryFlagsV3.Compressed:
                        switch((Compression)_hdrCompression)
                        {
                            case Compression.None:
                                goto uncompressedV3;
                            case Compression.Zlib:
                            case Compression.ZlibPlus:
                            {
                                var zHunk = new byte[entry.length << 16 | entry.lengthLsb];
                                _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                                _imageStream.EnsureRead(zHunk, 0, zHunk.Length);

                                var zStream = new DeflateStream(new MemoryStream(zHunk), CompressionMode.Decompress);

                                buffer = new byte[_bytesPerHunk];
                                int read = zStream.EnsureRead(buffer, 0, (int)_bytesPerHunk);

                                if(read != _bytesPerHunk)
                                {
                                    AaruLogging.Error(string.Format(Localization
                                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                                    read,
                                                                    _bytesPerHunk));

                                    return ErrorNumber.InOutError;
                                }

                                zStream.Close();

                                break;
                            }
                            case Compression.Av:
                                AaruLogging.Error(string.Format(Localization.Unsupported_compression_0,
                                                                (Compression)_hdrCompression));

                                return ErrorNumber.NotImplemented;
                        }

                        break;
                    case EntryFlagsV3.Uncompressed:
                    uncompressedV3:
                        buffer = new byte[_bytesPerHunk];
                        _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                        _imageStream.EnsureRead(buffer, 0, buffer.Length);

                        break;
                    case EntryFlagsV3.Mini:
                        buffer = new byte[_bytesPerHunk];
                        byte[] mini = BigEndianBitConverter.GetBytes(entry.offset);

                        for(var i = 0; i < _bytesPerHunk; i++) buffer[i] = mini[i % 8];

                        break;
                    case EntryFlagsV3.SelfHunk:
                        // A self-referential entry would recurse forever
                        if(entry.offset == hunkNo || entry.offset >= _totalHunks) return ErrorNumber.InvalidArgument;

                        return GetHunk(entry.offset, out buffer);
                    case EntryFlagsV3.ParentHunk:
                        AaruLogging.Error(Localization.Parent_images_are_not_supported);

                        return ErrorNumber.NotImplemented;
                    case EntryFlagsV3.SecondCompressed:
                    {
                        if(!FLAC.IsSupported)
                        {
                            AaruLogging.Error(Localization.FLAC_is_not_supported);

                            return ErrorNumber.NotImplemented;
                        }

                        int compLength = entry.length << 16 | entry.lengthLsb;
                        var flacHunk   = new byte[compLength];
                        _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                        _imageStream.EnsureRead(flacHunk, 0, flacHunk.Length);

                        // MAME's FLAC format: byte 0 = endianness marker ('L' or 'B'),
                        // followed by raw FLAC stream data
                        bool swapEndian = flacHunk[0] == 'B';

                        var flacData = new byte[compLength - 1];
                        Array.Copy(flacHunk, 1, flacData, 0, compLength - 1);

                        buffer = new byte[_bytesPerHunk];
                        int decoded = FLAC.DecodeBuffer(flacData, buffer);

                        if(decoded != _bytesPerHunk)
                        {
                            AaruLogging.Error(string.Format(Localization
                                                               .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                            decoded,
                                                            _bytesPerHunk));

                            return ErrorNumber.InOutError;
                        }

                        if(swapEndian)
                        {
                            for(var i = 0; i < _bytesPerHunk; i += 2)
                                (buffer[i], buffer[i + 1]) = (buffer[i + 1], buffer[i]);
                        }

                        break;
                    }
                    default:
                        AaruLogging.Error(string.Format(Localization.Hunk_type_0_is_not_supported, entry.flags & 0xF));

                        return ErrorNumber.NotSupported;
                }

                break;
            case 5:
                if(_hdrCompression == 0)
                {
                    buffer = new byte[_bytesPerHunk];

                    if(_hunkTableSmall[hunkNo] == 0)
                    {
                        // Entry value 0 means zeroed hunk
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        _imageStream.Seek((long)_hunkTableSmall[hunkNo] * _bytesPerHunk, SeekOrigin.Begin);
                        _imageStream.EnsureRead(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    ErrorNumber err = GetHunkV5Compressed(hunkNo, out buffer);

                    if(err != ErrorNumber.NoError) return err;
                }

                break;
            default:
                AaruLogging.Error(string.Format(Localization.Unsupported_hunk_map_version_0, _mapVersion));

                return ErrorNumber.NotSupported;
        }

        if(_hunkCache.Count >= _maxBlockCache) _hunkCache.Clear();

        _hunkCache.Add(hunkNo, buffer);

        return ErrorNumber.NoError;
    }

    ErrorNumber GetHunkV5Compressed(ulong hunkNo, out byte[] buffer)
    {
        buffer = null;

        if(hunkNo >= _totalHunks) return ErrorNumber.OutOfRange;

        var  mapOffset = (int)(hunkNo * 12);
        byte compType  = _rawMap[mapOffset];

        var compLength = (uint)(_rawMap[mapOffset + 1] << 16 | _rawMap[mapOffset + 2] << 8 | _rawMap[mapOffset + 3]);

        ulong fileOffset = (ulong)_rawMap[mapOffset + 4] << 40 |
                           (ulong)_rawMap[mapOffset + 5] << 32 |
                           (ulong)_rawMap[mapOffset + 6] << 24 |
                           (ulong)_rawMap[mapOffset + 7] << 16 |
                           (ulong)_rawMap[mapOffset + 8] << 8  |
                           _rawMap[mapOffset + 9];

        switch((EntryFlagsV5)compType)
        {
            case EntryFlagsV5.Compressed0:
            case EntryFlagsV5.Compressed1:
            case EntryFlagsV5.Compressed2:
            case EntryFlagsV5.Compressed3:
            {
                uint[] codecs = [_hdrCompression, _hdrCompression1, _hdrCompression2, _hdrCompression3];

                uint codec = codecs[compType];

                AaruLogging.Debug(MODULE_NAME,
                                  "GetHunkV5Compressed: hunk={0} compType={1} codec=0x{2:X8} compLength={3} fileOffset={4}",
                                  hunkNo,
                                  compType,
                                  codec,
                                  compLength,
                                  fileOffset);

                var compData = new byte[compLength];
                _imageStream.Seek((long)fileOffset, SeekOrigin.Begin);
                _imageStream.EnsureRead(compData, 0, compData.Length);

                buffer = new byte[_bytesPerHunk];

                ErrorNumber err = DecompressV5Codec(codec, compData, (int)compLength, buffer, (int)_bytesPerHunk);

                if(err != ErrorNumber.NoError) return err;

                break;
            }

            case EntryFlagsV5.Uncompressed:
                buffer = new byte[_bytesPerHunk];
                _imageStream.Seek((long)fileOffset, SeekOrigin.Begin);
                _imageStream.EnsureRead(buffer, 0, buffer.Length);

                break;

            case EntryFlagsV5.SelfHunk:
                // A self-referential entry would recurse forever
                if(fileOffset == hunkNo || fileOffset >= _totalHunks) return ErrorNumber.InvalidArgument;

                return GetHunk(fileOffset, out buffer);

            case EntryFlagsV5.ParentHunk:
                AaruLogging.Error(Localization.Parent_images_are_not_supported);

                return ErrorNumber.NotImplemented;

            default:
                AaruLogging.Error(string.Format(Localization.Hunk_type_0_is_not_supported, compType));

                return ErrorNumber.NotSupported;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber DecompressV5Codec(uint codec, byte[] src, int srcLen, byte[] dest, int destLen)
    {
        switch((CompressionV5)codec)
        {
            case CompressionV5.Zlib:
            {
                var zStream = new DeflateStream(new MemoryStream(src, 0, srcLen), CompressionMode.Decompress);

                int read = zStream.EnsureRead(dest, 0, destLen);
                zStream.Close();

                if(read != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    read,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                break;
            }

            case CompressionV5.Lzma:
            {
                byte[] properties = ComputeLzmaProperties(_bytesPerHunk);
                int    decoded    = LZMA.DecodeBuffer(src, dest, properties);

                if(decoded != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    decoded,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                break;
            }

            case CompressionV5.Huffman:
            {
                ErrorNumber err = DecompressHuffmanHunk(src, srcLen, dest, destLen);

                if(err != ErrorNumber.NoError) return err;

                break;
            }

            case CompressionV5.Flac:
            {
                if(!FLAC.IsSupported)
                {
                    AaruLogging.Error(Localization.FLAC_is_not_supported);

                    return ErrorNumber.NotImplemented;
                }

                bool swapEndian = src[0] == 'B';
                var  flacData   = new byte[srcLen - 1];
                Array.Copy(src, 1, flacData, 0, srcLen - 1);

                int decoded = FLAC.DecodeBuffer(flacData, dest);

                if(decoded != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    decoded,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                if(swapEndian)
                {
                    for(var i = 0; i < destLen; i += 2) (dest[i], dest[i + 1]) = (dest[i + 1], dest[i]);
                }

                break;
            }

            case CompressionV5.Zstd:
            {
                if(!ZSTD.IsSupported)
                {
                    AaruLogging.Error(Localization.FLAC_is_not_supported);

                    return ErrorNumber.NotImplemented;
                }

                int decoded = ZSTD.DecodeBuffer(src, dest);

                if(decoded != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    decoded,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                break;
            }

            case CompressionV5.CdZlib:
                return DecompressCdCodec(CompressionV5.Zlib, CompressionV5.Zlib, src, srcLen, dest, destLen);

            case CompressionV5.CdLzma:
                return DecompressCdCodec(CompressionV5.Lzma, CompressionV5.Zlib, src, srcLen, dest, destLen);

            case CompressionV5.CdZstd:
                return DecompressCdCodec(CompressionV5.Zstd, CompressionV5.Zstd, src, srcLen, dest, destLen);

            case CompressionV5.CdFlac:
                return DecompressCdFlac(src, srcLen, dest, destLen);

            default:
                AaruLogging.Error(string.Format(Localization.Unsupported_compression_0, (CompressionV5)codec));

                return ErrorNumber.NotSupported;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber DecompressCdCodec(CompressionV5 baseCodec, CompressionV5 subCodec, byte[] src, int srcLen, byte[] dest,
                                  int           destLen)
    {
        int frames       = destLen / CD_FRAME_SIZE;
        int compLenBytes = destLen < 65536 ? 2 : 3;
        int eccBytes     = (frames + 7) / 8;
        int headerBytes  = eccBytes + compLenBytes;

        int baseCompLen = compLenBytes == 2
                              ? src[eccBytes] << 8  | src[eccBytes                          + 1]
                              : src[eccBytes] << 16 | src[eccBytes + 1] << 8 | src[eccBytes + 2];

        AaruLogging.Debug(MODULE_NAME,
                          "DecompressCdCodec: srcLen={0} destLen={1} frames={2} eccBytes={3} compLenBytes={4} headerBytes={5} baseCompLen={6} headerBytes+baseCompLen={7}",
                          srcLen,
                          destLen,
                          frames,
                          eccBytes,
                          compLenBytes,
                          headerBytes,
                          baseCompLen,
                          headerBytes + baseCompLen);

        // Decompress sector data
        var         sectorBuf = new byte[frames * CD_MAX_SECTOR_DATA];
        ErrorNumber err = DecompressRawCodec(baseCodec, src, headerBytes, baseCompLen, sectorBuf, sectorBuf.Length);

        if(err != ErrorNumber.NoError) return err;

        // Decompress subcode data
        int subOffset  = headerBytes + baseCompLen;
        int subCompLen = srcLen      - subOffset;
        var subBuf     = new byte[frames * CD_MAX_SUBCODE_DATA];

        err = DecompressRawCodec(subCodec, src, subOffset, subCompLen, subBuf, subBuf.Length);

        if(err != ErrorNumber.NoError) return err;

        // Reassemble interleaved frames
        for(var i = 0; i < frames; i++)
        {
            Array.Copy(sectorBuf, i * CD_MAX_SECTOR_DATA, dest, i * CD_FRAME_SIZE, CD_MAX_SECTOR_DATA);

            Array.Copy(subBuf,
                       i * CD_MAX_SUBCODE_DATA,
                       dest,
                       i * CD_FRAME_SIZE + CD_MAX_SECTOR_DATA,
                       CD_MAX_SUBCODE_DATA);

            // Restore ECC/sync if bitmap bit set
            int bytePos = i / 8;
            int bitPos  = i % 8;

            if((src[bytePos] & 1 << bitPos) == 0) continue;

            // Copy sync header
            Array.Copy(_cdSyncHeader, 0, dest, i * CD_FRAME_SIZE, 12);

            // Reconstruct ECC for this sector
            var sectorData = new byte[CD_MAX_SECTOR_DATA];
            Array.Copy(dest, i * CD_FRAME_SIZE, sectorData, 0, CD_MAX_SECTOR_DATA);
            _sectorBuilder?.ReconstructEcc(ref sectorData, TrackType.CdMode1);
            Array.Copy(sectorData, 0, dest, i * CD_FRAME_SIZE, CD_MAX_SECTOR_DATA);
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber DecompressCdFlac(byte[] src, int srcLen, byte[] dest, int destLen)
    {
        if(!FLAC.IsSupported)
        {
            AaruLogging.Error(Localization.FLAC_is_not_supported);

            return ErrorNumber.NotImplemented;
        }

        int frames = destLen / CD_FRAME_SIZE;

        bool swapEndian = src[0] == 'B';

        // Decode FLAC audio data (sector data)
        var flacSrc = new byte[srcLen - 1];
        Array.Copy(src, 1, flacSrc, 0, srcLen - 1);

        var sectorBuf = new byte[frames * CD_MAX_SECTOR_DATA];
        int decoded   = FLAC.DecodeBuffer(flacSrc, sectorBuf);

        if(decoded != frames * CD_MAX_SECTOR_DATA)
        {
            AaruLogging.Error(string.Format(Localization.Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                            decoded,
                                            frames * CD_MAX_SECTOR_DATA));

            return ErrorNumber.InOutError;
        }

        if(swapEndian)
        {
            for(var i = 0; i < sectorBuf.Length; i += 2)
                (sectorBuf[i], sectorBuf[i + 1]) = (sectorBuf[i + 1], sectorBuf[i]);
        }

        // The FLAC stream length is unknown without consumed byte tracking,
        // so decompress subcode by trying from the end.
        // The subcode compressed data is everything after the FLAC stream.
        // We try to find it by scanning backwards for valid zlib data.
        var subBuf     = new byte[frames * CD_MAX_SUBCODE_DATA];
        var subDecoded = false;

        // Try binary search approach: FLAC must produce exactly frames*2352 bytes
        // The compressed FLAC stream is between src[1] and some offset before srcLen
        // Subcode zlib data follows immediately after FLAC data
        for(int flacEnd = srcLen - 1; flacEnd >= 2; flacEnd--)
        {
            try
            {
                int subLen = srcLen - 1 - flacEnd;
                var subSrc = new byte[subLen];
                Array.Copy(src, 1 + flacEnd, subSrc, 0, subLen);

                var zStream = new DeflateStream(new MemoryStream(subSrc), CompressionMode.Decompress);
                int read    = zStream.EnsureRead(subBuf, 0, subBuf.Length);
                zStream.Close();

                if(read == subBuf.Length)
                {
                    subDecoded = true;

                    break;
                }
            }
            catch(InvalidDataException)
            {
                // Invalid zlib data at this offset, try next
            }
            catch(InvalidOperationException)
            {
                // Invalid zlib data at this offset, try next
            }
        }

        if(!subDecoded) Array.Clear(subBuf, 0, subBuf.Length);

        // Reassemble interleaved frames
        for(var i = 0; i < frames; i++)
        {
            Array.Copy(sectorBuf, i * CD_MAX_SECTOR_DATA, dest, i * CD_FRAME_SIZE, CD_MAX_SECTOR_DATA);

            Array.Copy(subBuf,
                       i * CD_MAX_SUBCODE_DATA,
                       dest,
                       i * CD_FRAME_SIZE + CD_MAX_SECTOR_DATA,
                       CD_MAX_SUBCODE_DATA);
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber DecompressRawCodec(CompressionV5 codec, byte[] src, int srcOffset, int srcLen, byte[] dest, int destLen)
    {
        switch(codec)
        {
            case CompressionV5.Zlib:
            {
                var zStream = new DeflateStream(new MemoryStream(src, srcOffset, srcLen), CompressionMode.Decompress);

                int read = zStream.EnsureRead(dest, 0, destLen);
                zStream.Close();

                if(read != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    read,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                break;
            }

            case CompressionV5.Lzma:
            {
                byte[] properties = ComputeLzmaProperties((uint)destLen);
                byte[] lzmaSrc;

                if(src[srcOffset] != 0x00)
                {
                    // Old-style LZMA stream (LZMA SDK before 2018):
                    // Byte 0 is part of the range coder code (bytes 0-3 = initial code).
                    // Modern LZMA SDK expects byte 0 = 0x00 (check only), bytes 1-4 = code.
                    // Prepend a 0x00 check byte to make old-style data compatible.
                    lzmaSrc    = new byte[srcLen + 1];
                    lzmaSrc[0] = 0x00;
                    Array.Copy(src, srcOffset, lzmaSrc, 1, srcLen);
                }
                else
                {
                    lzmaSrc = new byte[srcLen];
                    Array.Copy(src, srcOffset, lzmaSrc, 0, srcLen);
                }

                int decoded = LZMA.DecodeBuffer(lzmaSrc, dest, properties);

                if(decoded != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    decoded,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                break;
            }

            case CompressionV5.Zstd:
            {
                if(!ZSTD.IsSupported)
                {
                    AaruLogging.Error(Localization.FLAC_is_not_supported);

                    return ErrorNumber.NotImplemented;
                }

                var zstdSrc = new byte[srcLen];
                Array.Copy(src, srcOffset, zstdSrc, 0, srcLen);

                int decoded = ZSTD.DecodeBuffer(zstdSrc, dest);

                if(decoded != destLen)
                {
                    AaruLogging.Error(string.Format(Localization
                                                       .Unable_to_decompress_hunk_correctly_got_0_bytes_expected_1,
                                                    decoded,
                                                    destLen));

                    return ErrorNumber.InOutError;
                }

                break;
            }

            default:
                AaruLogging.Error(string.Format(Localization.Unsupported_compression_0, codec));

                return ErrorNumber.NotSupported;
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber DecompressHuffmanHunk(byte[] src, int srcLen, byte[] dest, int destLen)
    {
        var bitstream = new BitStreamReader(src, srcLen);

        // Import Huffman tree with RLE (256 symbols, maxbits=16)
        var numbits = 5; // maxbits >= 16

        var codeLengths = new int[256];
        var curNode     = 0;

        while(curNode < 256)
        {
            int nodeBits = bitstream.Read(numbits);

            if(nodeBits != 1)
                codeLengths[curNode++] = nodeBits;
            else
            {
                nodeBits = bitstream.Read(numbits);

                if(nodeBits == 1)
                    codeLengths[curNode++] = 1;
                else
                {
                    int repCount = bitstream.Read(numbits) + 3;

                    for(var r = 0; r < repCount && curNode < 256; r++) codeLengths[curNode++] = nodeBits;
                }
            }
        }

        // Build canonical Huffman lookup table
        var   maxBits = 16;
        int[] lookup  = BuildHuffmanLookup(codeLengths, 256, maxBits);

        if(lookup == null)
        {
            AaruLogging.Error(Localization.Invalid_hunk_found);

            return ErrorNumber.InvalidArgument;
        }

        // Decode each byte
        for(var i = 0; i < destLen; i++)
        {
            int bits    = bitstream.Peek(maxBits);
            int val     = lookup[bits];
            int symbol  = val >> 5;
            int codeLen = val & 0x1F;
            bitstream.Remove(codeLen);
            dest[i] = (byte)symbol;
        }

        return ErrorNumber.NoError;
    }

    static ErrorNumber DecompressV5Map(Stream stream, ulong mapOffset, uint totalHunks, uint hunkBytes, uint unitBytes,
                                       out byte[] rawMap)
    {
        rawMap = null;

        // Read 16-byte map header
        var mapHeader = new byte[16];
        stream.Seek((long)mapOffset, SeekOrigin.Begin);
        stream.EnsureRead(mapHeader, 0, 16);

        var mapBytes = BigEndianBitConverter.ToUInt32(mapHeader, 0);

        ulong firstOffs = (ulong)mapHeader[4] << 40 |
                          (ulong)mapHeader[5] << 32 |
                          (ulong)mapHeader[6] << 24 |
                          (ulong)mapHeader[7] << 16 |
                          (ulong)mapHeader[8] << 8  |
                          mapHeader[9];

        var mapCrc = (ushort)(mapHeader[10] << 8 | mapHeader[11]);

        int lengthBits = mapHeader[12];
        int selfBits   = mapHeader[13];
        int parentBits = mapHeader[14];

        // Read compressed map data, never more than the file holds
        if(mapBytes > stream.Length - stream.Position) return ErrorNumber.InvalidArgument;

        var mapData = new byte[mapBytes];
        stream.EnsureRead(mapData, 0, (int)mapBytes);

        var bitstream = new BitStreamReader(mapData, (int)mapBytes);

        // Import Huffman tree with RLE (16 symbols, maxbits=8)
        var numbits = 4; // maxbits >= 8

        var codeLengths = new int[16];
        var curNode     = 0;

        while(curNode < 16)
        {
            int nodeBits = bitstream.Read(numbits);

            if(nodeBits != 1)
                codeLengths[curNode++] = nodeBits;
            else
            {
                nodeBits = bitstream.Read(numbits);

                if(nodeBits == 1)
                    codeLengths[curNode++] = 1;
                else
                {
                    int treeRepCount = bitstream.Read(numbits) + 3;

                    for(var r = 0; r < treeRepCount && curNode < 16; r++) codeLengths[curNode++] = nodeBits;
                }
            }
        }

        // Build canonical Huffman lookup table
        var maxBits = 8;

        int[] lookup = BuildHuffmanLookup(codeLengths, 16, maxBits);

        if(lookup == null)
        {
            AaruLogging.Error(Localization.Invalid_hunk_found);

            return ErrorNumber.InvalidArgument;
        }

        // Pass 1: Decode compression types (literal port from working Python)
        var compTypes = new int[totalHunks];
        var repCount  = 0;
        var lastComp  = 0;

        for(uint hunk = 0; hunk < totalHunks; hunk++)
        {
            if(repCount > 0)
            {
                compTypes[hunk] =  lastComp;
                repCount        -= 1;
            }
            else
            {
                int bits    = bitstream.Peek(maxBits);
                int val     = lookup[bits];
                int symbol  = val >> 5;
                int codeLen = val & 0x1F;
                bitstream.Remove(codeLen);

                if(symbol == 7)
                {
                    // RLE small
                    compTypes[hunk] = lastComp;
                    bits            = bitstream.Peek(maxBits);
                    val             = lookup[bits];
                    int count = val >> 5;
                    codeLen = val & 0x1F;
                    bitstream.Remove(codeLen);
                    repCount = 2 + count;
                }
                else if(symbol == 8)
                {
                    // RLE large
                    compTypes[hunk] = lastComp;
                    bits            = bitstream.Peek(maxBits);
                    val             = lookup[bits];
                    int high = val >> 5;
                    codeLen = val & 0x1F;
                    bitstream.Remove(codeLen);
                    repCount = 2 + 16 + (high << 4);
                    bits     = bitstream.Peek(maxBits);
                    val      = lookup[bits];
                    int low = val >> 5;
                    codeLen = val & 0x1F;
                    bitstream.Remove(codeLen);
                    repCount += low;
                }
                else
                    compTypes[hunk] = lastComp = symbol;
            }
        }

        // Pass 2: Decode lengths, offsets, CRCs (literal port from working Python)
        rawMap = new byte[totalHunks * 12];
        ulong curOffset  = firstOffs;
        uint  lastSelf   = 0;
        ulong lastParent = 0;

        for(uint hunk = 0; hunk < totalHunks; hunk++)
        {
            var    entryOffset = (int)(hunk * 12);
            int    compType    = compTypes[hunk];
            ulong  offset      = curOffset;
            uint   length      = 0;
            ushort crc         = 0;

            if(compType >= 0 && compType <= 3)
            {
                length    =  (uint)bitstream.Read(lengthBits);
                crc       =  (ushort)bitstream.Read(16);
                curOffset += length;
            }
            else if(compType == 4)
            {
                length    =  hunkBytes;
                crc       =  (ushort)bitstream.Read(16);
                curOffset += hunkBytes;
            }
            else if(compType == 5)
            {
                lastSelf = (uint)bitstream.Read(selfBits);
                offset   = lastSelf;
            }
            else if(compType == 6)
            {
                offset     = (ulong)bitstream.Read(parentBits);
                lastParent = offset;
            }
            else if(compType == 10)
            {
                lastSelf += 1;
                compType =  5;
                offset   =  lastSelf;
            }
            else if(compType == 9)
            {
                compType = 5;
                offset   = lastSelf;
            }
            else if(compType == 11)
            {
                compType   = 6;
                lastParent = (ulong)hunk * hunkBytes / unitBytes;
                offset     = lastParent;
            }
            else if(compType == 13)
            {
                lastParent += hunkBytes / unitBytes;
                compType   =  6;
                offset     =  lastParent;
            }
            else if(compType == 12)
            {
                compType = 6;
                offset   = lastParent;
            }

            rawMap[entryOffset]      = (byte)(compType     & 0xFF);
            rawMap[entryOffset + 1]  = (byte)(length >> 16 & 0xFF);
            rawMap[entryOffset + 2]  = (byte)(length >> 8  & 0xFF);
            rawMap[entryOffset + 3]  = (byte)(length       & 0xFF);
            rawMap[entryOffset + 4]  = (byte)(offset >> 40 & 0xFF);
            rawMap[entryOffset + 5]  = (byte)(offset >> 32 & 0xFF);
            rawMap[entryOffset + 6]  = (byte)(offset >> 24 & 0xFF);
            rawMap[entryOffset + 7]  = (byte)(offset >> 16 & 0xFF);
            rawMap[entryOffset + 8]  = (byte)(offset >> 8  & 0xFF);
            rawMap[entryOffset + 9]  = (byte)(offset       & 0xFF);
            rawMap[entryOffset + 10] = (byte)(crc >> 8     & 0xFF);
            rawMap[entryOffset + 11] = (byte)(crc          & 0xFF);
        }

        // Verify map CRC16 (CCITT polynomial 0x1021, init 0xFFFF)
        ushort computedCrc = 0xFFFF;

        foreach(byte b in rawMap)
        {
            computedCrc ^= (ushort)(b << 8);

            for(var bit = 0; bit < 8; bit++)
            {
                if((computedCrc & 0x8000) != 0)
                    computedCrc = (ushort)(computedCrc << 1 ^ 0x1021);
                else
                    computedCrc = (ushort)(computedCrc << 1);
            }
        }

        if(mapCrc != computedCrc)
        {
            AaruLogging.Error("Map CRC mismatch: expected 0x{0:X4}, computed 0x{1:X4}", mapCrc, computedCrc);

            return ErrorNumber.InvalidArgument;
        }

        return ErrorNumber.NoError;
    }

    static int[] BuildHuffmanLookup(int[] codeLengths, int numCodes, int maxBits)
    {
        // Build histogram of bit lengths
        var bitHisto = new int[33];

        for(var i = 0; i < numCodes; i++)
        {
            if(codeLengths[i] > maxBits) return null;

            if(codeLengths[i] <= 32) bitHisto[codeLengths[i]]++;
        }

        // Assign starting codes from length 32 down to 1
        uint curStart = 0;

        for(var codeLen = 32; codeLen > 0; codeLen--)
        {
            var nextStart = (uint)(curStart + bitHisto[codeLen] >> 1);
            bitHisto[codeLen] = (int)curStart;
            curStart          = nextStart;
        }

        // Assign canonical codes
        var codes = new uint[numCodes];

        for(var i = 0; i < numCodes; i++)
        {
            if(codeLengths[i] > 0) codes[i] = (uint)bitHisto[codeLengths[i]]++;
        }

        // Build lookup table
        var lookup = new int[1 << maxBits];

        for(var i = 0; i < numCodes; i++)
        {
            if(codeLengths[i] <= 0) continue;

            int value = i << 5 | codeLengths[i] & 0x1F;
            int shift = maxBits - codeLengths[i];
            var start = (int)(codes[i] << shift);
            var end   = (int)((codes[i] + 1 << shift) - 1);

            for(int j = start; j <= end; j++) lookup[j] = value;
        }

        return lookup;
    }

    static byte[] ComputeLzmaProperties(uint hunkBytes)
    {
        var properties = new byte[5];

        // lc=3, lp=0, pb=2 → 2*45+0+3 = 93 = 0x5D
        properties[0] = 0x5D;

        uint dictSize = 1;

        while(dictSize < hunkBytes && dictSize < 1u << 26) dictSize <<= 1;

        properties[1] = (byte)dictSize;
        properties[2] = (byte)(dictSize >> 8);
        properties[3] = (byte)(dictSize >> 16);
        properties[4] = (byte)(dictSize >> 24);

        return properties;
    }

    /// <summary>MSB-first bitstream reader matching MAME's bitstream_in implementation</summary>
    sealed class BitStreamReader
    {
        readonly byte[] _data;
        readonly int    _dataLength;

        internal BitStreamReader(byte[] data, int dataLength)
        {
            _data       = data;
            _dataLength = dataLength;
            DOffset     = 0;
            DBitOffs    = 0;
            Buffer      = 0;
            Bits        = 0;
        }

        internal int DOffset { get; private set; }

        internal int DBitOffs { get; private set; }

        internal uint Buffer { get; private set; }

        internal int Bits { get; private set; }

        internal int Peek(int numBits)
        {
            if(numBits == 0) return 0;

            if(numBits > Bits)
            {
                while(Bits < 32)
                {
                    uint newBits = 0;

                    if(DOffset < _dataLength) newBits = (uint)(_data[DOffset] << DBitOffs & 0xFF);

                    if(Bits + 8 > 32)
                    {
                        DBitOffs =   32 - Bits;
                        newBits  >>= 8  - DBitOffs;
                        Buffer   |=  newBits;
                        Bits     +=  DBitOffs;
                    }
                    else
                    {
                        Buffer   |= newBits << 24 - Bits;
                        Bits     += 8 - DBitOffs;
                        DBitOffs =  0;
                        DOffset++;
                    }
                }
            }

            return (int)(Buffer >> 32 - numBits & (uint)((1 << numBits) - 1));
        }

        internal void Remove(int numBits)
        {
            Buffer =  Buffer << numBits & 0xFFFFFFFF;
            Bits   -= numBits;
        }

        internal int Read(int numBits)
        {
            int result = Peek(numBits);
            Remove(numBits);

            return result;
        }
    }
}