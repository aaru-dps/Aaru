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

public sealed partial class Lha
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        fileName = entry.DirectoryPath is not null
                       ? Path.Combine(entry.DirectoryPath, entry.Filename ?? "")
                       : entry.Filename ?? "";

        // Normalize to OS separators
        fileName = fileName.Replace('\\', '/');

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

        if(entry.HasUnixPermissions) stat.Mode = entry.UnixPermissions;

        if(entry.Uid > 0) stat.UID = entry.Uid;
        if(entry.Gid > 0) stat.GID = entry.Gid;

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

            switch(entry.Method)
            {
                case Method.Stored:
                    // No decompression needed
                    break;
                case Method.Lh1:
                    stream = new LhaStream(stream, entry.UncompressedSize, 1);

                    break;
                case Method.Lh2:
                    stream = new LhaStream(stream, entry.UncompressedSize, 2);

                    break;
                case Method.Lh3:
                    stream = new LhaStream(stream, entry.UncompressedSize, 3);

                    break;
                case Method.Lh4:
                    stream = new LhaStream(stream, entry.UncompressedSize, 4);

                    break;
                case Method.Lh5:
                    stream = new LhaStream(stream, entry.UncompressedSize, 5);

                    break;
                case Method.Lh6:
                    stream = new LhaStream(stream, entry.UncompressedSize, 6);

                    break;
                case Method.Lh7:
                    stream = new LhaStream(stream, entry.UncompressedSize, 7);

                    break;
                case Method.Lzs:
                    stream = new LarcStream(stream, entry.UncompressedSize, 0);

                    break;
                case Method.Lz5:
                    stream = new LarcStream(stream, entry.UncompressedSize, 5);

                    break;
                case Method.Pm1:
                    stream = new PmarcStream(stream, entry.UncompressedSize, 1);

                    break;
                case Method.Pm2:
                    stream = new PmarcStream(stream, entry.UncompressedSize, 2);

                    break;
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