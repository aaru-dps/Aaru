// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads PowerISO disc images.
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
using System.IO.Compression;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Aaru.Logging;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class PowerISO
{
    /// <summary>Decompresses a chunk and returns the decompressed data</summary>
    /// <param name="chunkIndex">Index into the chunk table</param>
    /// <param name="decompressedData">Decompressed chunk data on success</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber DecompressChunk(int chunkIndex, out byte[] decompressedData)
    {
        decompressedData = null;

        if(chunkIndex < 0 || chunkIndex >= _numChunks) return ErrorNumber.OutOfRange;

        if(_chunkCache.TryGetValue(chunkIndex, out decompressedData)) return ErrorNumber.NoError;

        DaaChunk chunk = _chunkTable[chunkIndex];

        // Find the part that contains this chunk's data
        int partIndex = -1;

        for(var p = 0; p < _partTable.Length; p++)
        {
            if(chunk.offset >= _partTable[p].start && chunk.offset < _partTable[p].end)
            {
                partIndex = p;

                break;
            }
        }

        if(partIndex < 0) return ErrorNumber.InvalidArgument;

        DaaPart part = _partTable[partIndex];

        // Seek to the chunk data within the part stream
        var seekPos = (long)(part.offset + (chunk.offset - part.start));
        part.stream.Seek(seekPos, SeekOrigin.Begin);

        // Read compressed data, never more than the I/O buffer can hold (v1 stored chunks can exceed the chunk size)
        var readLen = (int)Math.Min(chunk.length, (uint)_ioBuffer.Length);
        part.stream.EnsureRead(_ioBuffer, 0, readLen);

        switch(chunk.compression)
        {
            case DaaCompressionType.None:
                decompressedData = new byte[_chunkSize];
                Array.Copy(_ioBuffer, 0, decompressedData, 0, readLen);

                break;
            case DaaCompressionType.Zlib:
            {
                decompressedData = new byte[_chunkSize];

                using var compressedMs  = new MemoryStream(_ioBuffer, 0, readLen);
                using var deflateStream = new DeflateStream(compressedMs, CompressionMode.Decompress);
                var       totalRead     = 0;

                while(totalRead < (int)_chunkSize)
                {
                    int bytesRead = deflateStream.Read(decompressedData, totalRead, (int)_chunkSize - totalRead);

                    if(bytesRead == 0) break;

                    totalRead += bytesRead;
                }

                break;
            }
            case DaaCompressionType.Lzma:
            {
                // First LZMA_PROPS_SIZE bytes are LZMA properties, rest is compressed data
                var lzmaProps = new byte[LZMA_PROPS_SIZE];
                Array.Copy(_ioBuffer, 0, lzmaProps, 0, LZMA_PROPS_SIZE);

                var lzmaData = new byte[readLen - LZMA_PROPS_SIZE];
                Array.Copy(_ioBuffer, LZMA_PROPS_SIZE, lzmaData, 0, readLen - LZMA_PROPS_SIZE);

                decompressedData = new byte[_chunkSize];
                LZMA.DecodeBuffer(lzmaData, decompressedData, lzmaProps);

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        // Cache the decompressed chunk
        if(_currentChunkCacheSize + _chunkSize > MAX_CACHE_SIZE)
        {
            _chunkCache.Clear();
            _currentChunkCacheSize = 0;
        }

        _chunkCache[chunkIndex] =  decompressedData;
        _currentChunkCacheSize  += _chunkSize;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads bits from a byte array at a given bit position</summary>
    /// <param name="bits">Number of bits to read</param>
    /// <param name="data">Source byte array</param>
    /// <param name="inBits">Current bit position (updated on return)</param>
    /// <returns>The value read from the bit stream</returns>
    static ulong ReadBits(int bits, byte[] data, ref int inBits)
    {
        ulong ret  = 0;
        var   seek = 0;
        ulong mask = bits < 64 ? (1UL << bits) - 1 : 0xFFFFFFFFFFFFFFFF;

        for(;;)
        {
            int seekBits = inBits & 7;
            ret |= (ulong)data[inBits >> 3] >> seekBits << seek;
            int rem = 8 - seekBits;

            if(rem >= bits) break;

            bits   -= rem;
            inBits += rem;
            seek   += rem;
        }

        inBits += bits;

        return ret & mask;
    }

    /// <summary>Deobfuscates chunk table data for GBI images</summary>
    static void DeobfuscateGbi(byte[] data, ulong isoSize)
    {
        // CRC8 lookup table (same as used by libmirage)
        byte[] crc8Table =
        [
            0x00, 0x5E, 0xBC, 0xE2, 0x61, 0x3F, 0xDD, 0x83, 0xC2, 0x9C, 0x7E, 0x20, 0xA3, 0xFD, 0x1F, 0x41, 0x9D,
            0xC3, 0x21, 0x7F, 0xFC, 0xA2, 0x40, 0x1E, 0x5F, 0x01, 0xE3, 0xBD, 0x3E, 0x60, 0x82, 0xDC, 0x23, 0x7D,
            0x9F, 0xC1, 0x42, 0x1C, 0xFE, 0xA0, 0xE1, 0xBF, 0x5D, 0x03, 0x80, 0xDE, 0x3C, 0x62, 0xBE, 0xE0, 0x02,
            0x5C, 0xDF, 0x81, 0x63, 0x3D, 0x7C, 0x22, 0xC0, 0x9E, 0x1D, 0x43, 0xA1, 0xFF, 0x46, 0x18, 0xFA, 0xA4,
            0x27, 0x79, 0x9B, 0xC5, 0x84, 0xDA, 0x38, 0x66, 0xE5, 0xBB, 0x59, 0x07, 0xDB, 0x85, 0x67, 0x39, 0xBA,
            0xE4, 0x06, 0x58, 0x19, 0x47, 0xA5, 0xFB, 0x78, 0x26, 0xC4, 0x9A, 0x65, 0x3B, 0xD9, 0x87, 0x04, 0x5A,
            0xB8, 0xE6, 0xA7, 0xF9, 0x1B, 0x45, 0xC6, 0x98, 0x7A, 0x24, 0xF8, 0xA6, 0x44, 0x1A, 0x99, 0xC7, 0x25,
            0x7B, 0x3A, 0x64, 0x86, 0xD8, 0x5B, 0x05, 0xE7, 0xB9, 0x8C, 0xD2, 0x30, 0x6E, 0xED, 0xB3, 0x51, 0x0F,
            0x4E, 0x10, 0xF2, 0xAC, 0x2F, 0x71, 0x93, 0xCD, 0x11, 0x4F, 0xAD, 0xF3, 0x70, 0x2E, 0xCC, 0x92, 0xD3,
            0x8D, 0x6F, 0x31, 0xB2, 0xEC, 0x0E, 0x50, 0xAF, 0xF1, 0x13, 0x4D, 0xCE, 0x90, 0x72, 0x2C, 0x6D, 0x33,
            0xD1, 0x8F, 0x0C, 0x52, 0xB0, 0xEE, 0x32, 0x6C, 0x8E, 0xD0, 0x53, 0x0D, 0xEF, 0xB1, 0xF0, 0xAE, 0x4C,
            0x12, 0x91, 0xCF, 0x2D, 0x73, 0xCA, 0x94, 0x76, 0x28, 0xAB, 0xF5, 0x17, 0x49, 0x08, 0x56, 0xB4, 0xEA,
            0x69, 0x37, 0xD5, 0x8B, 0x57, 0x09, 0xEB, 0xB5, 0x36, 0x68, 0x8A, 0xD4, 0x95, 0xCB, 0x29, 0x77, 0xF4,
            0xAA, 0x48, 0x16, 0xE9, 0xB7, 0x55, 0x0B, 0x88, 0xD6, 0x34, 0x6A, 0x2B, 0x75, 0x97, 0xC9, 0x4A, 0x14,
            0xF6, 0xA8, 0x74, 0x2A, 0xC8, 0x96, 0x15, 0x4B, 0xA9, 0xF7, 0xB6, 0xE8, 0x0A, 0x54, 0xD7, 0x89, 0x6B,
            0x35
        ];

        var  size = (byte)(isoSize / 4);
        byte crc8 = 0;

        for(var i = 0; i < data.Length; i++)
        {
            crc8    =  crc8Table[crc8 ^ data[i]];
            data[i] -= crc8;
            data[i] ^= size;
        }
    }

    /// <summary>Deobfuscates chunk table data for DAA images</summary>
    static void DeobfuscateDaa(byte[] data, ulong isoSize)
    {
        ulong sectors = isoSize / 2048;
        var   a       = (byte)(sectors >> 8 & 0xFF);
        var   c       = (byte)(sectors      & 0xFF);

        for(var i = 0; i < data.Length; i++)
        {
            data[i] -= c;
            c       += a;
        }
    }

    /// <summary>Generates a part filename based on the split function type</summary>
    /// <param name="basePath">Path to the main DAA/GBI file</param>
    /// <param name="partIndex">Part index (1-based, 1 = first additional part)</param>
    /// <param name="funcType">Split function type (1, 2, or 3)</param>
    /// <returns>Path to the part file, or null if the function type is unknown</returns>
    static string GetPartFilename(string basePath, int partIndex, int funcType)
    {
        string dir  = Path.GetDirectoryName(basePath) ?? "";
        string name = Path.GetFileNameWithoutExtension(basePath);
        string ext  = Path.GetExtension(basePath);

        switch(funcType)
        {
            case 1:
            {
                // volname.part01.daa → volname.part02.daa, volname.part03.daa, ...
                // Find "part01" or "Part01" etc. in the filename and replace number
                int lastPart = name.LastIndexOf("part", StringComparison.OrdinalIgnoreCase);

                if(lastPart < 0) return null;

                string prefix  = name[..lastPart];
                var    partNum = (partIndex + 1).ToString("D2");

                return Path.Combine(dir, prefix + "part" + partNum + ext);
            }
            case 2:
            {
                // volname.part001.daa → volname.part002.daa, ...
                int lastPart = name.LastIndexOf("part", StringComparison.OrdinalIgnoreCase);

                if(lastPart < 0) return null;

                string prefix  = name[..lastPart];
                var    partNum = (partIndex + 1).ToString("D3");

                return Path.Combine(dir, prefix + "part" + partNum + ext);
            }
            case 3:
            {
                // volname.daa → volname.d00, volname.d01, ...
                string newExt = "." + ext[1] + (partIndex - 1).ToString("D2");

                return Path.Combine(dir, name + newExt);
            }
            default:
                return null;
        }
    }

    /// <summary>Determines media type based on total sector count</summary>
    static MediaType CalculateMediaType(ulong totalSectors) => totalSectors switch
                                                               {
                                                                   <= 360000   => MediaType.CD,
                                                                   <= 2295104  => MediaType.DVDPR,
                                                                   <= 2298496  => MediaType.DVDR,
                                                                   <= 4171712  => MediaType.DVDRDL,
                                                                   <= 4173824  => MediaType.DVDPRDL,
                                                                   <= 24438784 => MediaType.BDR,
                                                                   <= 62500864 => MediaType.BDRXL,
                                                                   _           => MediaType.Unknown
                                                               };

#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 76) return ErrorNumber.InvalidArgument;

        // Read main header
        var headerBytes = new byte[76];
        stream.EnsureRead(headerBytes, 0, 76);
        _header = Marshal.ByteArrayToStructureLittleEndian<DaaMainHeader>(headerBytes);

        // Determine image type from signature
        if(_header.signature.SequenceEqual(_daaMainSignature))
            _imageType = DaaImageType.Daa;
        else if(_header.signature.SequenceEqual(_gbiMainSignature))
            _imageType = DaaImageType.Gbi;
        else
            return ErrorNumber.InvalidArgument;

        // Verify CRC32 over first 72 bytes
        var crc32Ctx = new Crc32Context();
        crc32Ctx.Update(headerBytes, 72);
        byte[] crcBytes    = crc32Ctx.Final();
        var    computedCrc = BigEndianBitConverter.ToUInt32(crcBytes, 0);

        if(computedCrc != _header.crc)
        {
            AaruLogging.Error("CRC32 mismatch in DAA main header");

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "header.chunkTableOffset = {0}",   _header.chunkTableOffset);
        AaruLogging.Debug(MODULE_NAME, "header.formatVersion = 0x{0:X4}", _header.formatVersion);
        AaruLogging.Debug(MODULE_NAME, "header.chunkDataOffset = {0}",    _header.chunkDataOffset);
        AaruLogging.Debug(MODULE_NAME, "header.chunkSize = 0x{0:X8}",     _header.chunkSize);
        AaruLogging.Debug(MODULE_NAME, "header.isoSize = {0}",            _header.isoSize);
        AaruLogging.Debug(MODULE_NAME, "header.daaSize = {0}",            _header.daaSize);

        // Parse format version and extract chunk parameters
        uint chunkSize;
        int  chunkDataOffset;
        bool isVersion2;

        switch((DaaFormatVersion)_header.formatVersion)
        {
            case DaaFormatVersion.Version1:
                isVersion2      = false;
                chunkSize       = _header.chunkSize;
                chunkDataOffset = (int)_header.chunkDataOffset;

                break;
            case DaaFormatVersion.Version2:
                isVersion2      = true;
                chunkDataOffset = (int)(_header.chunkDataOffset & 0x00FFFFFF);
                chunkSize       = (_header.chunkSize            & 0xFFF) << 14;

                _compressedChunkTable = (_header.chunkSize & 0x4000)    != 0;
                _obfuscatedBits       = (_header.chunkSize & 0x20000)   != 0;
                _obfuscatedChunkTable = (_header.chunkSize & 0x8000000) != 0;

                AaruLogging.Debug(MODULE_NAME, "format2.profile = {0}", _header.format2.profile);

                AaruLogging.Debug(MODULE_NAME,
                                  "format2.chunkTableCompressed = {0}",
                                  _header.format2.chunkTableCompressed);

                AaruLogging.Debug(MODULE_NAME,
                                  "format2.chunkTableBitSettings = 0x{0:X2}",
                                  _header.format2.chunkTableBitSettings);

                AaruLogging.Debug(MODULE_NAME, "format2.lzmaFilter = {0}",   _header.format2.lzmaFilter);
                AaruLogging.Debug(MODULE_NAME, "compressedChunkTable = {0}", _compressedChunkTable);
                AaruLogging.Debug(MODULE_NAME, "obfuscatedBits = {0}",       _obfuscatedBits);
                AaruLogging.Debug(MODULE_NAME, "obfuscatedChunkTable = {0}", _obfuscatedChunkTable);

                break;
            default:
                AaruLogging.Error("Unsupported DAA format version: 0x{0:X4}", _header.formatVersion);

                return ErrorNumber.NotSupported;
        }

        _chunkSize        = chunkSize;
        _chunkDataOffset  = chunkDataOffset;
        _chunkTableOffset = (int)_header.chunkTableOffset;

        AaruLogging.Debug(MODULE_NAME, "chunkSize = {0}",       _chunkSize);
        AaruLogging.Debug(MODULE_NAME, "chunkDataOffset = {0}", _chunkDataOffset);

        // Parse descriptors between header and chunk table
        _numParts = 1;
        var       splitFunc   = 0;
        var       splitBlocks = 0;
        List<int> splitData   = [];

        stream.Seek(76, SeekOrigin.Begin);

        while(stream.Position < _header.chunkTableOffset)
        {
            var descHeaderBytes = new byte[8];
            stream.EnsureRead(descHeaderBytes, 0, 8);

            DaaDescriptorHeader descHeader =
                Marshal.ByteArrayToStructureLittleEndian<DaaDescriptorHeader>(descHeaderBytes);

            int dataLength = (int)descHeader.length - 8;

            if(dataLength <= 0)
            {
                AaruLogging.Debug(MODULE_NAME, "Descriptor type {0} with no data, skipping", descHeader.type);

                continue;
            }

            var descData = new byte[dataLength];
            stream.EnsureRead(descData, 0, dataLength);

            switch((DaaDescriptorType)descHeader.type)
            {
                case DaaDescriptorType.Split:
                    var numParts = BitConverter.ToUInt32(descData, 0);

                    // uint dummy = BitConverter.ToUInt32(descData, 4);
                    _numParts = (int)numParts + 1; // +1 for main part

                    int remaining = dataLength - 8;

                    switch(remaining / 5)
                    {
                        case 99:
                            splitFunc   = 1;
                            splitBlocks = 99;

                            break;
                        case 512:
                            splitFunc   = 2;
                            splitBlocks = 512;

                            break;
                        case 101:
                            splitFunc   = 3;
                            splitBlocks = 101;

                            break;
                        default:
                            AaruLogging.Error("Unknown split block count: {0}", remaining / 5);

                            return ErrorNumber.NotSupported;
                    }

                    for(var i = 0; i < splitBlocks; i++) splitData.Add(descData[8 + i * 5]);

                    AaruLogging.Debug(MODULE_NAME,
                                      "Split descriptor: numParts = {0}, func = {1}",
                                      _numParts,
                                      splitFunc);

                    break;
                case DaaDescriptorType.Encryption:
                    _encrypted = true;
                    AaruLogging.Error("Encrypted DAA images are not supported");

                    return ErrorNumber.NotSupported;
                case DaaDescriptorType.Comment:
                    AaruLogging.Debug(MODULE_NAME, "Comment descriptor found, skipping");

                    break;
                case DaaDescriptorType.Part:
                    AaruLogging.Debug(MODULE_NAME, "Part descriptor found, skipping");

                    break;
                default:
                    AaruLogging.Debug(MODULE_NAME, "Unknown descriptor type {0}, skipping", descHeader.type);

                    break;
            }
        }

        // Read and parse chunk table
        stream.Seek(_header.chunkTableOffset, SeekOrigin.Begin);
        int chunkTableLen = chunkDataOffset - (int)_header.chunkTableOffset;

        if(chunkTableLen <= 0)
        {
            AaruLogging.Error("Invalid chunk table length: {0}", chunkTableLen);

            return ErrorNumber.InvalidArgument;
        }

        var chunkTableData = new byte[chunkTableLen];
        stream.EnsureRead(chunkTableData, 0, chunkTableLen);

        // Decompress chunk table if compressed (v2)
        if(isVersion2 && _compressedChunkTable)
        {
            AaruLogging.Debug(MODULE_NAME, "Decompressing chunk table");

            using var compressedMs   = new MemoryStream(chunkTableData, 2, chunkTableData.Length - 2);
            using var deflateStream  = new DeflateStream(compressedMs, CompressionMode.Decompress);
            using var decompressedMs = new MemoryStream();
            deflateStream.CopyTo(decompressedMs);
            chunkTableData = decompressedMs.ToArray();
            chunkTableLen  = chunkTableData.Length;
        }

        // Deobfuscate chunk table
        if(_imageType == DaaImageType.Gbi)
            DeobfuscateGbi(chunkTableData, _header.isoSize);
        else if(_obfuscatedChunkTable) DeobfuscateDaa(chunkTableData, _header.isoSize);

        // Parse chunk entries
        if(!isVersion2)
        {
            // Version 1: 3-byte entries, always zlib
            _numChunks  = chunkTableLen / 3;
            _chunkTable = new DaaChunk[_numChunks];
            ulong runningOffset = 0;

            for(var i = 0; i < _numChunks; i++)
            {
                int off    = i * 3;
                var length = (uint)(chunkTableData[off] << 16 | chunkTableData[off + 2] << 8 | chunkTableData[off + 1]);

                _chunkTable[i] = new DaaChunk
                {
                    offset      = runningOffset,
                    length      = length,
                    compression = length >= _chunkSize ? DaaCompressionType.None : DaaCompressionType.Zlib
                };

                runningOffset += length;
            }
        }
        else
        {
            // Version 2: bit-packed entries
            int bsizeType = _header.format2.chunkTableBitSettings & 7;

            int bsizeLen = _header.format2.chunkTableBitSettings >> 3;

            if(bsizeLen != 0)
                bsizeLen += 10;
            else
            {
                bsizeLen = 0;

                for(uint len = _chunkSize; len > (uint)bsizeType; bsizeLen++, len >>= 1) {}
            }

            _bitsizeType   = bsizeType;
            _bitsizeLength = bsizeLen;

            // Calculate swap type for obfuscated bits
            if(_obfuscatedBits)
            {
                _bitSwapType = _imageType == DaaImageType.Daa
                                   ? (int)(_header.isoSize / 2048 & 0xF) + 5
                                   : (int)(_header.isoSize / 4    & 0xF) + 5;
            }

            _numChunks  = chunkTableLen * 8 / (bsizeType + bsizeLen);
            _chunkTable = new DaaChunk[_numChunks];
            ulong runningOffset = 0;
            var   inBits        = 0;

            for(var i = 0; i < _numChunks; i++)
            {
                var length = (uint)ReadBits(bsizeLen, chunkTableData, ref inBits);
                length += LZMA_PROPS_SIZE;
                var typeBits = (uint)ReadBits(bsizeType, chunkTableData, ref inBits);

                // Deobfuscate type bits
                if(_obfuscatedBits && bsizeType > 0)
                {
                    int swapBit = i % _bitSwapType;

                    if(swapBit < bsizeType) typeBits ^= 1u << swapBit;
                }

                DaaCompressionType compression;

                if(length >= _chunkSize)
                {
                    compression = DaaCompressionType.None;
                    length      = _chunkSize;
                }
                else
                {
                    compression = typeBits switch
                                  {
                                      0 => DaaCompressionType.Lzma,
                                      1 => DaaCompressionType.Zlib,
                                      _ => DaaCompressionType.None
                                  };
                }

                _chunkTable[i] = new DaaChunk
                {
                    offset      = runningOffset,
                    length      = length,
                    compression = compression
                };

                runningOffset += length;
            }
        }

        AaruLogging.Debug(MODULE_NAME, "Parsed {0} chunks", _numChunks);

        // Build part table
        _partTable   = new DaaPart[_numParts];
        _partStreams = [stream];

        // First part is the main file
        _partTable[0] = new DaaPart
        {
            stream = stream,
            offset = (ulong)chunkDataOffset,
            start  = 0,
            end    = (ulong)(stream.Length - chunkDataOffset)
        };

        // Open additional parts if it is a split archive
        if(_numParts > 1)
        {
            string basePath = Path.Combine(imageFilter.ParentFolder, imageFilter.Filename);

            for(var p = 1; p < _numParts; p++)
            {
                string partPath = GetPartFilename(basePath, p, splitFunc);

                if(partPath == null || !File.Exists(partPath))
                {
                    AaruLogging.Error("Cannot find part file: {0}", partPath ?? "(null)");

                    return ErrorNumber.NoSuchFile;
                }

                Stream partStream = new FileStream(partPath, FileMode.Open, FileAccess.Read);
                _partStreams.Add(partStream);

                var partHeaderBytes = new byte[40];
                partStream.EnsureRead(partHeaderBytes, 0, 40);

                DaaPartHeader partHeader = Marshal.ByteArrayToStructureLittleEndian<DaaPartHeader>(partHeaderBytes);

                // Verify part signature
                byte[] expectedSig = _imageType == DaaImageType.Daa ? _daaPartSignature : _gbiPartSignature;

                if(!partHeader.signature.SequenceEqual(expectedSig))
                {
                    AaruLogging.Error("Invalid part file signature: {0}", partPath);

                    return ErrorNumber.InvalidArgument;
                }

                // Verify CRC32 over first 36 bytes
                var partCrc32Ctx = new Crc32Context();
                partCrc32Ctx.Update(partHeaderBytes, 36);
                byte[] partCrcBytes    = partCrc32Ctx.Final();
                var    partComputedCrc = BigEndianBitConverter.ToUInt32(partCrcBytes, 0);

                if(partComputedCrc != partHeader.crc)
                {
                    AaruLogging.Error("CRC32 mismatch in part file: {0}", partPath);

                    return ErrorNumber.InvalidArgument;
                }

                int partDataOffset = isVersion2
                                         ? (int)(partHeader.chunkDataOffset & 0x00FFFFFF)
                                         : (int)partHeader.chunkDataOffset;

                _partTable[p] = new DaaPart
                {
                    stream = partStream,
                    offset = (ulong)partDataOffset,
                    start  = _partTable[p - 1].end,
                    end    = _partTable[p - 1].end + (ulong)(partStream.Length - partDataOffset)
                };
            }
        }

        // Calculate image properties
        ulong totalSectors = _header.isoSize / SECTOR_SIZE;

        // Set up image info
        _imageInfo.Sectors              = totalSectors;
        _imageInfo.SectorSize           = SECTOR_SIZE;
        _imageInfo.ImageSize            = _header.isoSize;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.MediaType            = CalculateMediaType(totalSectors);
        _imageInfo.Application          = _imageType == DaaImageType.Daa ? "PowerISO" : "gBurner";

        // Build tracks, sessions, and partitions for a single data track
        Tracks =
        [
            new Track
            {
                BytesPerSector    = (int)SECTOR_SIZE,
                RawBytesPerSector = (int)SECTOR_SIZE,
                Sequence          = 1,
                Session           = 1,
                StartSector       = 0,
                EndSector         = totalSectors - 1,
                Type              = TrackType.CdMode1,
                SubchannelType    = TrackSubchannelType.None,
                Description       = string.Format(Localization.Track_0, 1),
                File              = imageFilter.Filename,
                FileType          = "BINARY",
                Filter            = imageFilter,
                Indexes = new Dictionary<ushort, int>
                {
                    [0] = -150,
                    [1] = 0
                }
            }
        ];

        Sessions =
        [
            new Session
            {
                Sequence    = 1,
                StartTrack  = 1,
                EndTrack    = 1,
                StartSector = 0,
                EndSector   = totalSectors - 1
            }
        ];

        Partitions =
        [
            new Partition
            {
                Sequence    = 0,
                Start       = 0,
                Length      = totalSectors,
                Offset      = 0,
                Size        = totalSectors * SECTOR_SIZE,
                Description = string.Format(Localization.Track_0, 1),
                Type        = "MODE1/2048"
            }
        ];

        // Store the filter and initialize caches
        _imageFilter           = imageFilter;
        _chunkCache            = new Dictionary<int, byte[]>();
        _currentChunkCacheSize = 0;
        _sectorCache           = new Dictionary<ulong, byte[]>();
        _inflateBuffer         = new byte[_chunkSize];
        _ioBuffer              = new byte[_chunkSize];
        _sectorBuilder         = new SectorBuilder();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm)
    {
        dpmStartSector     = 0;
        dpmResolution      = 0;
        numberOfDpmEntries = 0;
        dpm                = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm)
    {
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
        {
            sectorStatus = SectorStatus.Dumped;

            return ErrorNumber.NoError;
        }

        var chunkIndex    = (int)(sectorAddress  * SECTOR_SIZE / _chunkSize);
        var offsetInChunk = (uint)(sectorAddress * SECTOR_SIZE % _chunkSize);

        ErrorNumber errno = DecompressChunk(chunkIndex, out byte[] chunkData);

        if(errno != ErrorNumber.NoError) return errno;

        buffer = new byte[SECTOR_SIZE];
        Array.Copy(chunkData, (int)offsetInChunk, buffer, 0, SECTOR_SIZE);

        if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

        _sectorCache[sectorAddress] = buffer;
        sectorStatus                = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        using var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, track, out byte[] sectorData, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sectorData, 0, sectorData.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong            sectorAddress, bool negative, out byte[] buffer,
                                  out SectorStatus sectorStatus) =>
        ReadSector(sectorAddress, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus) =>
        ReadSectors(sectorAddress, length, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        ErrorNumber errno = ReadSector(sectorAddress, track, out byte[] userData, out sectorStatus);

        if(errno != ErrorNumber.NoError) return errno;

        buffer = new byte[2352];
        Array.Copy(userData, 0, buffer, 16, 2048);
        _sectorBuilder.ReconstructPrefix(ref buffer, TrackType.CdMode1, (long)sectorAddress);
        _sectorBuilder.ReconstructEcc(ref buffer, TrackType.CdMode1);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        using var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno =
                ReadSectorLong(sectorAddress + i, track, out byte[] sectorData, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sectorData, 0, sectorData.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus) =>
        ReadSectorLong(sectorAddress, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus) =>
        ReadSectorsLong(sectorAddress, length, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
                buffer = [(byte)CdFlags.DataTrack];

                return ErrorNumber.NoError;
            case SectorTagType.CdSectorSync:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEcc:
                break;
            default:
                return ErrorNumber.NotSupported;
        }

        ErrorNumber errno = ReadSectorLong(sectorAddress, track, out byte[] fullSector, out SectorStatus _);

        if(errno != ErrorNumber.NoError) return errno;

        switch(tag)
        {
            case SectorTagType.CdSectorSync:
                buffer = new byte[12];
                Array.Copy(fullSector, 0, buffer, 0, 12);

                break;
            case SectorTagType.CdSectorHeader:
                buffer = new byte[4];
                Array.Copy(fullSector, 12, buffer, 0, 4);

                break;
            case SectorTagType.CdSectorEdc:
                buffer = new byte[4];
                Array.Copy(fullSector, 2064, buffer, 0, 4);

                break;
            case SectorTagType.CdSectorEccP:
                buffer = new byte[172];
                Array.Copy(fullSector, 2076, buffer, 0, 172);

                break;
            case SectorTagType.CdSectorEccQ:
                buffer = new byte[104];
                Array.Copy(fullSector, 2248, buffer, 0, 104);

                break;
            case SectorTagType.CdSectorEcc:
                buffer = new byte[276];
                Array.Copy(fullSector, 2076, buffer, 0, 276);

                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        if(tag == SectorTagType.CdTrackFlags) return ReadSectorTag(sectorAddress, track, tag, out buffer);

        using var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorTag(sectorAddress + i, track, tag, out byte[] tagData);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(tagData, 0, tagData.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer) =>
        ReadSectorTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer) => ReadSectorsTag(sectorAddress, length, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => session.Sequence == 1 ? Tracks : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => session == 1 ? Tracks : null;

#endregion
}