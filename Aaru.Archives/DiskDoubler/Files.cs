using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression.DiskDoubler;
using Aaru.Filters;
using Aaru.Helpers;
using Aaru.Logging;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Archives;

public sealed partial class DiskDoubler
{
    static bool NeedsXorMask(byte info1, byte info2) => info1 >= 0x2A && (info2 & 0x80) == 0;

    static void ApplyXor(byte[] data, byte mask)
    {
        for(var i = 0; i < data.Length; i++) data[i] ^= mask;
    }

    static void ApplyDelta(byte[] data)
    {
        for(var i = 1; i < data.Length; i++) data[i] += data[i - 1];
    }

    byte[] DecompressFork(long offset, long compressedSize, long uncompressedSize, byte method, byte delta, byte info1,
                          byte info2)
    {
        if(uncompressedSize == 0) return [];

        // Validate sizes and offsets against the file before allocating
        if(offset < 0 || uncompressedSize is < 0 or > int.MaxValue) return null;

        int effectiveMethod = method & 0x7F;

        if(effectiveMethod == 0)
        {
            if(offset + uncompressedSize > _stream.Length) return null;

            // No compression — read raw data
            var raw = new byte[uncompressedSize];
            _stream.Position = offset;
            _stream.ReadExactly(raw, 0, raw.Length);

            if(delta == 1) ApplyDelta(raw);

            return raw;
        }

        if(compressedSize <= 0 || offset + compressedSize > _stream.Length) return null;

        // Read compressed data
        var compData = new byte[compressedSize];
        _stream.Position = offset;
        _stream.ReadExactly(compData, 0, compData.Length);

        byte[] result;

        switch((DiskDoublerMethod)effectiveMethod)
        {
            case DiskDoublerMethod.Compress:
            {
                if(compData.Length < 4) return null;

                bool xor   = NeedsXorMask(info1, info2);
                byte flags = compData[2];

                if(xor) flags ^= XOR_MASK;

                // Strip 3-byte header, pass remaining to LzwStream
                var lzwData = new byte[compressedSize - 3];
                Buffer.BlockCopy(compData, 3, lzwData, 0, lzwData.Length);

                if(xor) ApplyXor(lzwData, XOR_MASK);

                using var ms  = new MemoryStream(lzwData);
                using var lzw = new LzwStream(ms, uncompressedSize, flags);
                result = new byte[uncompressedSize];
                lzw.ReadExactly(result, 0, result.Length);

                break;
            }

            case DiskDoublerMethod.Method2:
            {
                bool xor = NeedsXorMask(info1, info2);

                if(xor) ApplyXor(compData, XOR_MASK);

                using var ms       = new MemoryStream(compData);
                using var m2Stream = new Method2Stream(ms, uncompressedSize, 256);
                result = new byte[uncompressedSize];
                m2Stream.ReadExactly(result, 0, result.Length);

                if(xor) ApplyXor(result, XOR_MASK);

                break;
            }

            case DiskDoublerMethod.Method5:
            {
                if(compData.Length < 2) return null;

                bool xor = NeedsXorMask(info1, info2);

                if(xor) ApplyXor(compData, XOR_MASK);

                int numTrees = compData[0];

                if(numTrees == 0) numTrees = 256;

                // Strip 1-byte numTrees header
                var m5Data = new byte[compressedSize - 1];
                Buffer.BlockCopy(compData, 1, m5Data, 0, m5Data.Length);

                using var ms       = new MemoryStream(m5Data);
                using var m2Stream = new Method2Stream(ms, uncompressedSize, numTrees);
                result = new byte[uncompressedSize];
                m2Stream.ReadExactly(result, 0, result.Length);

                if(xor) ApplyXor(result, XOR_MASK);

                break;
            }

            case DiskDoublerMethod.Huffman:
            case DiskDoublerMethod.Rle:
                // Method 3 (RLE) and method 4 (Huffman) have no known test files
                // and no verified decompressor implementation
                return null;

            case DiskDoublerMethod.Ads:
            case DiskDoublerMethod.Ad:
            {
                using var ms  = new MemoryStream(compData);
                using var adn = new AdnStream(ms, uncompressedSize);
                result = new byte[uncompressedSize];
                adn.ReadExactly(result, 0, result.Length);

                break;
            }

            case DiskDoublerMethod.StacLzs:
            {
                if(compData.Length < 10) return null;

                // Skip table header: 6 bytes + 4-byte entry count + 8 + 2*count bytes
                var  numEntries = BigEndianBitConverter.ToUInt32(compData, 6);
                long tableEnd   = 6 + 4 + 8 + 2L * numEntries;

                // The table cannot extend past the compressed data
                if(tableEnd >= compData.Length) return null;

                var pos = (int)tableEnd;

                // XOR remaining data with 0xFF
                int lzsLen  = compData.Length - pos;
                var lzsData = new byte[lzsLen];
                Buffer.BlockCopy(compData, pos, lzsData, 0, lzsLen);
                ApplyXor(lzsData, XOR_STAC);

                using var ms   = new MemoryStream(lzsData);
                using var stac = new StacLzsStream(ms, uncompressedSize);
                result = new byte[uncompressedSize];
                stac.ReadExactly(result, 0, result.Length);

                // XOR output with 0xFF
                ApplyXor(result, XOR_STAC);

                break;
            }

            case DiskDoublerMethod.CompactPro:
            {
                using var ms  = new MemoryStream(compData);
                using var cpt = new CompactProStream(ms, uncompressedSize);
                result = new byte[uncompressedSize];
                cpt.ReadExactly(result, 0, result.Length);

                break;
            }

            case DiskDoublerMethod.Ddn:
            {
                using var ms  = new MemoryStream(compData);
                using var ddn = new DdnStream(ms, uncompressedSize);
                result = new byte[uncompressedSize];
                ddn.ReadExactly(result, 0, result.Length);

                break;
            }

            default:
                return null;
        }

        if(delta == 1) ApplyDelta(result);

        return result;
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        fileName = entry.DirectoryPath is not null
                       ? entry.DirectoryPath + "/" + (entry.Filename ?? "")
                       : entry.Filename ?? "";

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntryNumber(string fileName, bool caseInsensitiveMatch, out int entryNumber)
    {
        entryNumber = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        StringComparison comparison = caseInsensitiveMatch
                                          ? StringComparison.CurrentCultureIgnoreCase
                                          : StringComparison.CurrentCulture;

        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            GetFilename(i, out string name);

            if(name is null) continue;

            if(!name.Equals(fileName, comparison)) continue;

            entryNumber = i;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber GetCompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        length = _entries[entryNumber].DataCompressedSize;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        length = _entries[entryNumber].DataUncompressedSize;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(int entryNumber, out FileEntryInfo stat)
    {
        stat = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        stat = new FileEntryInfo
        {
            Attributes       = entry.IsDirectory ? FileAttributes.Directory : FileAttributes.File,
            Blocks           = entry.DataUncompressedSize / 512,
            BlockSize        = 512,
            Length           = entry.DataUncompressedSize,
            LastWriteTimeUtc = entry.ModificationTime,
            CreationTimeUtc  = entry.CreationTime
        };

        if(entry.DataUncompressedSize % 512 != 0) stat.Blocks++;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntry(int entryNumber, out IFilter filter)
    {
        filter = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        if(entry.IsDirectory) return ErrorNumber.InvalidArgument;

        byte[] data;

        try
        {
            data = DecompressFork(entry.DataOffset,
                                  entry.DataCompressedSize,
                                  entry.DataUncompressedSize,
                                  entry.DataMethod,
                                  entry.DataDelta,
                                  entry.Info1,
                                  entry.Info2);
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Exception decompressing fork: {0}", ex);

            return ErrorNumber.InOutError;
        }

        if(data is null) return ErrorNumber.NotSupported;

        var ms = new MemoryStream(data);

        filter = new ZZZNoFilter();
        ErrorNumber errno = filter.Open(ms);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        ms.Close();

        return errno;
    }

#endregion
}