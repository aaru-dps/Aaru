using System;
using System.IO;
using System.IO.Compression;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Compression.Zip;
using Aaru.Filters;
using Aaru.Helpers.IO;
using Aaru.Logging;
using BZip2 = Aaru.Compression.BZip2;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Archives;

public sealed partial class Zip
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        fileName = _entries[entryNumber].Filename ?? "";

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

        FileAttributes attrs = entry.IsDirectory ? FileAttributes.Directory : FileAttributes.File;

        if(entry.IsSymlink) attrs |= FileAttributes.Symlink;

        if(entry.UnixPermissions != 0)
        {
            var fileType = (ushort)(entry.UnixPermissions & S_IFMT);

            if(fileType == S_IFBLK) attrs |= FileAttributes.BlockDevice;

            if(fileType == S_IFCHR) attrs |= FileAttributes.CharDevice;
        }

        stat = new FileEntryInfo
        {
            Attributes       = attrs,
            Blocks           = entry.UncompressedSize / 512,
            BlockSize        = 512,
            Length           = entry.UncompressedSize,
            LastWriteTimeUtc = entry.LastWriteTime != default(DateTime) ? entry.LastWriteTime.ToUniversalTime() : null
        };

        if(entry.LastAccessTime != default(DateTime)) stat.AccessTimeUtc = entry.LastAccessTime.ToUniversalTime();

        if(entry.CreationTime != default(DateTime)) stat.CreationTimeUtc = entry.CreationTime.ToUniversalTime();

        if(entry.BackupTime != default(DateTime)) stat.BackupTimeUtc = entry.BackupTime.ToUniversalTime();

        if(entry.Uid != 0) stat.UID = entry.Uid;

        if(entry.Gid != 0) stat.GID = entry.Gid;

        if(entry.UnixPermissions != 0) stat.Mode = entry.UnixPermissions;

        if(entry.DeviceNo.HasValue) stat.DeviceNo = entry.DeviceNo.Value;

        if(entry.UncompressedSize % 512 != 0) stat.Blocks++;

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

        if(entry.IsEncrypted) return ErrorNumber.NotSupported;

        Stream stream;

        if(entry.UncompressedSize == 0)
            stream = new MemoryStream([]);
        else
        {
            // Validate against the file size, OffsetStream throws on invalid bounds
            if(entry.CompressedSize   <= 0 ||
               entry.DataOffset       <  0 ||
               entry.UncompressedSize <  0 ||
               entry.DataOffset + entry.CompressedSize > _stream.Length)
                return ErrorNumber.InvalidArgument;

            Stream compressedStream = new OffsetStream(new NonClosableStream(_stream),
                                                       entry.DataOffset,
                                                       entry.DataOffset + entry.CompressedSize - 1);

            switch(entry.Method)
            {
                case CompressionMethod.Stored:
                    stream = compressedStream;

                    break;

                case CompressionMethod.Shrink:
                    stream = new ShrinkStream(compressedStream, entry.UncompressedSize);

                    break;

                case CompressionMethod.Reduce1:
                case CompressionMethod.Reduce2:
                case CompressionMethod.Reduce3:
                case CompressionMethod.Reduce4:
                    stream = new ReduceStream(compressedStream, entry.UncompressedSize, (int)entry.Method - 1);

                    break;

                case CompressionMethod.Implode:
                    stream = new ImplodeStream(compressedStream,
                                               entry.UncompressedSize,
                                               (entry.Flags & FLAG_IMPLODE_8K)       != 0,
                                               (entry.Flags & FLAG_IMPLODE_LITERALS) != 0);

                    break;

                case CompressionMethod.Deflate:
                {
                    if(entry.UncompressedSize > int.MaxValue) return ErrorNumber.NotSupported;

                    // DeflateStream decompresses and we wrap in a MemoryStream for seekability
                    var decompressed = new byte[entry.UncompressedSize];

                    using(var deflate = new DeflateStream(compressedStream, CompressionMode.Decompress, true))
                    {
                        var totalRead = 0;

                        while(totalRead < decompressed.Length)
                        {
                            int bytesRead = deflate.Read(decompressed, totalRead, decompressed.Length - totalRead);

                            if(bytesRead == 0) break;

                            totalRead += bytesRead;
                        }
                    }

                    stream = new MemoryStream(decompressed);

                    break;
                }

                case CompressionMethod.Deflate64:
                    stream = new Deflate64Stream(compressedStream, entry.UncompressedSize);

                    break;

                case CompressionMethod.Blast:
                    stream = new BlastStream(compressedStream, entry.UncompressedSize);

                    break;

                case CompressionMethod.BZip2:
                {
                    if(entry.UncompressedSize > int.MaxValue) return ErrorNumber.NotSupported;

                    var compressedData  = new byte[entry.CompressedSize];
                    _ = compressedStream.Read(compressedData, 0, compressedData.Length);
                    var decompressedBuf = new byte[entry.UncompressedSize];

                    try
                    {
                        BZip2.DecodeBuffer(compressedData, decompressedBuf);
                    }
                    catch(Exception ex)
                    {
                        AaruLogging.Debug(MODULE_NAME, "Exception decompressing BZip2 data: {0}", ex);

                        return ErrorNumber.InOutError;
                    }

                    stream = new MemoryStream(decompressedBuf);

                    break;
                }

                case CompressionMethod.Lzma:
                {
                    // ZIP LZMA header: 2 bytes version, 2 bytes properties size, N bytes properties
                    var lzmaHeader = new byte[4];
                    compressedStream.Read(lzmaHeader, 0, 4);
                    var propsSize = BitConverter.ToUInt16(lzmaHeader, 2);

                    var properties = new byte[propsSize];
                    compressedStream.Read(properties, 0, propsSize);

                    long remainingCompressed = entry.CompressedSize - 4 - propsSize;

                    // The properties cannot be larger than the compressed data
                    if(remainingCompressed < 0 || entry.UncompressedSize > int.MaxValue)
                        return ErrorNumber.InvalidArgument;

                    var compressedData = new byte[remainingCompressed];
                    compressedStream.Read(compressedData, 0, compressedData.Length);

                    var decompressedBuf = new byte[entry.UncompressedSize];

                    try
                    {
                        LZMA.DecodeBuffer(compressedData, decompressedBuf, properties);
                    }
                    catch(Exception ex)
                    {
                        AaruLogging.Debug(MODULE_NAME, "Exception decompressing LZMA data: {0}", ex);

                        return ErrorNumber.InOutError;
                    }

                    stream = new MemoryStream(decompressedBuf);

                    break;
                }

                case CompressionMethod.PPMd:
                {
                    // PPMd info word: 2 bytes
                    var infoBytes = new byte[2];
                    compressedStream.Read(infoBytes, 0, 2);
                    var info = BitConverter.ToUInt16(infoBytes, 0);

                    int  maxOrder     = (info & 0x0F) + 1;
                    int  subAllocSize = (info >> 4 & 0xFF) + 1 << 20;
                    bool restoration  = (info >> 12 & 0x0F) != 0;

                    // Remaining data is the PPMd stream
                    long remaining = entry.CompressedSize - 2;

                    Stream ppmdInput = new OffsetStream(new NonClosableStream(_stream),
                                                        entry.DataOffset                        + 2,
                                                        entry.DataOffset + entry.CompressedSize - 1);

                    stream = new PpmdStream(ppmdInput, entry.UncompressedSize, maxOrder, subAllocSize, restoration);

                    break;
                }

                default:
                    return ErrorNumber.NotSupported;
            }
        }

        filter = new ZZZNoFilter();
        ErrorNumber errno = filter.Open(stream);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        stream.Close();

        return errno;
    }

#endregion
}