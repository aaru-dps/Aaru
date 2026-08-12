using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression.StuffItX;
using Aaru.Filters;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Archives;

public sealed partial class StuffItX
{
    static Stream DecompressStream(Stream compressedStream, CompressionAlgorithm algorithm, long uncompressedSize)
    {
        switch(algorithm)
        {
            case CompressionAlgorithm.None:
                return compressedStream;

            case CompressionAlgorithm.Brimstone:
            {
                // Read 2-byte PPMd header: exponent (alloc_size = 1 << exponent), max_order
                int exponent = compressedStream.ReadByte();
                int maxOrder = compressedStream.ReadByte();

                // Exponent 31 would overflow the allocation size to a negative value
                if(exponent < 0 || maxOrder < 0 || exponent > 30) return null;

                int allocSize = 1 << exponent;

                // Strip the header bytes
                var remaining = new byte[compressedStream.Length - compressedStream.Position];
                compressedStream.ReadExactly(remaining, 0, remaining.Length);

                return new BrimstoneStream(new MemoryStream(remaining), uncompressedSize, maxOrder, allocSize);
            }

            case CompressionAlgorithm.Cyanide:
                return new CyanideStream(compressedStream, uncompressedSize);

            case CompressionAlgorithm.Darkhorse:
            {
                // Read 1-byte window header
                int windowByte = compressedStream.ReadByte();

                if(windowByte < 0) return null;

                // Strip the header byte
                var remaining = new byte[compressedStream.Length - compressedStream.Position];
                compressedStream.ReadExactly(remaining, 0, remaining.Length);

                return new DarkhorseStream(new MemoryStream(remaining), uncompressedSize, windowByte);
            }

            case CompressionAlgorithm.Deflate:
            {
                // Read 1-byte window size; only 15 is supported
                int windowSize = compressedStream.ReadByte();

                if(windowSize != 15) return null;

                // Strip the window byte
                var remaining = new byte[compressedStream.Length - compressedStream.Position];
                compressedStream.ReadExactly(remaining, 0, remaining.Length);

                return new DeflateStream(new MemoryStream(remaining), uncompressedSize);
            }

            case CompressionAlgorithm.Blend:
                return new BlendStream(compressedStream, uncompressedSize);

            case CompressionAlgorithm.Rc4:
            {
                // Method 5: No compression, obscured by RC4 with 1-byte key
                // Skip 2 header bytes, read 1-byte key
                compressedStream.ReadByte();
                compressedStream.ReadByte();

                int keyByte = compressedStream.ReadByte();

                if(keyByte < 0) return null;

                // Read remaining data
                var data = new byte[compressedStream.Length - compressedStream.Position];
                compressedStream.ReadExactly(data, 0, data.Length);

                // Apply RC4 decryption with single-byte key
                var s = new byte[256];

                for(var i = 0; i < 256; i++) s[i] = (byte)i;

                // KSA with 1-byte key
                var j = 0;

                for(var i = 0; i < 256; i++)
                {
                    j            = j + s[i] + keyByte & 0xFF;
                    (s[i], s[j]) = (s[j], s[i]);
                }

                // PRGA
                int x = 0, y = 0;

                for(var i = 0; i < data.Length; i++)
                {
                    x            =  x + 1    & 0xFF;
                    y            =  y + s[x] & 0xFF;
                    (s[x], s[y]) =  (s[y], s[x]);
                    data[i]      ^= s[s[x] + s[y] & 0xFF];
                }

                return new MemoryStream(data);
            }

            case CompressionAlgorithm.Iron:
                return new IronStream(compressedStream, uncompressedSize);

            default:
                return null;
        }
    }

    static Stream ApplyPreprocessing(Stream decompressedStream, PreprocessAlgorithm algorithm, long actualSize)
    {
        return algorithm switch
               {
                   PreprocessAlgorithm.None    => decompressedStream,
                   PreprocessAlgorithm.English => new EnglishStream(decompressedStream, actualSize),
                   PreprocessAlgorithm.X86     => new X86Stream(decompressedStream, actualSize),
                   _                           => null
               };
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

        length = _entries[entryNumber].CompressedSize;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        length = _entries[entryNumber].UncompressedSize;

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
            Blocks           = entry.UncompressedSize / 512,
            BlockSize        = 512,
            Length           = entry.UncompressedSize,
            LastWriteTimeUtc = entry.ModificationTime,
            CreationTimeUtc  = entry.CreationTime
        };

        if(entry.UncompressedSize % 512 != 0) stat.Blocks++;

        if(entry.IsLink) stat.Attributes |= FileAttributes.Symlink;

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

        if(entry.Encrypted) return ErrorNumber.NotSupported;

        Stream stream;

        if(entry.IsEmptyStream || entry.UncompressedSize == 0)
            stream = new MemoryStream([]);
        else
        {
            if(!entry.HasSolidElement) return ErrorNumber.InvalidArgument;

            // Decompress the entire solid stream from the DATA element
            ErrorNumber errno = DecompressElement(entry.SolidElement, out Stream solidStream);

            if(errno != ErrorNumber.NoError) return errno;

            if(solidStream == null) return ErrorNumber.NotSupported;

            // Read the solid stream fully into memory for slicing
            solidStream.Position = 0;
            var solidData = new byte[solidStream.Length];
            solidStream.ReadExactly(solidData, 0, solidData.Length);

            // Reject offsets outside the solid stream or beyond what a MemoryStream can slice
            if(entry.SolidOffset is < 0 or > int.MaxValue || entry.UncompressedSize > int.MaxValue)
                return ErrorNumber.InvalidArgument;

            // Slice the entry's portion from the solid stream
            if(entry.SolidOffset + entry.UncompressedSize > solidData.Length)
            {
                long available = solidData.Length - entry.SolidOffset;


                if(available <= 0) return ErrorNumber.InvalidArgument;

                stream = new MemoryStream(solidData, (int)entry.SolidOffset, (int)available);
            }
            else
                stream = new MemoryStream(solidData, (int)entry.SolidOffset, (int)entry.UncompressedSize);
        }

        filter = new ZZZNoFilter();
        ErrorNumber openErr = filter.Open(stream);

        if(openErr == ErrorNumber.NoError) return ErrorNumber.NoError;

        stream.Close();

        return openErr;
    }

#endregion
}