using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Filters;
using Aaru.Helpers.IO;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Archives;

public sealed partial class Arj
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

        stat = new FileEntryInfo
        {
            Attributes       = entry.IsDirectory ? FileAttributes.Directory : FileAttributes.File,
            Blocks           = entry.UncompressedSize / 512,
            BlockSize        = 512,
            Length           = entry.UncompressedSize,
            LastWriteTime    = entry.LastWriteTime,
            LastWriteTimeUtc = entry.LastWriteTime.ToUniversalTime()
        };

        if(entry.UncompressedSize % 512 != 0) stat.Blocks++;

        if(entry.CreationTime != default(DateTime))
        {
            stat.CreationTime    = entry.CreationTime;
            stat.CreationTimeUtc = entry.CreationTime.ToUniversalTime();
        }

        if(entry.LastAccessTime != default(DateTime))
        {
            stat.AccessTime    = entry.LastAccessTime;
            stat.AccessTimeUtc = entry.LastAccessTime.ToUniversalTime();
        }

        // Unix file permissions for Unix hosts
        if(entry.HostOs is HostOs.Unix or HostOs.Next) stat.Mode = entry.FileMode;

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

        // Encrypted files are not supported
        if((entry.ArjFlags & (byte)ArjFlags.Garbled) != 0) return ErrorNumber.NotSupported;

        Stream stream;

        if(entry.UncompressedSize == 0)
            stream = new MemoryStream([]);
        else
        {
            // Validate against the file size, OffsetStream throws on invalid bounds
            if(entry.CompressedSize                    <= 0 ||
               entry.DataOffset                        < 0  ||
               entry.DataOffset + entry.CompressedSize > _stream.Length)
                return ErrorNumber.InvalidArgument;

            stream = new OffsetStream(new NonClosableStream(_stream),
                                      entry.DataOffset,
                                      entry.DataOffset + entry.CompressedSize - 1);

            switch(entry.ArjxNbr)
            {
                // ARJZ dispatch based on arj_x_nbr
                case ARJZ_METHOD1_VERSION:
                    stream = new ArjzStream(stream, entry.UncompressedSize, 1);

                    break;
                case ARJZ_METHOD2_VERSION:
                    stream = new ArjzStream(stream, entry.UncompressedSize, 2);

                    break;
                case ARJZ_METHOD3_VERSION:
                    stream = new ArjzStream(stream, entry.UncompressedSize, 3);

                    break;
                case ARJZ_DEFLATEZ_VERSION:
                    stream = new ArjzStream(stream, entry.UncompressedSize, 5);

                    break;
                default:
                    // Standard ARJ dispatch based on method
                    switch(entry.Method)
                    {
                        case Method.Stored:
                            // No decompression needed
                            break;
                        case Method.Method1:
                            stream = new ArjStream(stream, entry.UncompressedSize, 1);

                            break;
                        case Method.Method2:
                            stream = new ArjStream(stream, entry.UncompressedSize, 2);

                            break;
                        case Method.Method3:
                            stream = new ArjStream(stream, entry.UncompressedSize, 3);

                            break;
                        case Method.Fastest:
                            stream = new ArjStream(stream, entry.UncompressedSize, 4);

                            break;
                        default:
                            return ErrorNumber.NotSupported;
                    }

                    break;
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