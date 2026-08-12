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

public sealed partial class Ace
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

        // Solid files that depend on prior decompression state cannot be extracted independently.
        // The first file of the archive can always be extracted since the dictionary starts fresh.
        if(entry.IsSolid && entryNumber > 0) return ErrorNumber.NotSupported;

        // Encrypted files not supported
        if(entry.IsEncrypted) return ErrorNumber.NotSupported;

        // Split files (multi-volume) not supported
        if(entry.IsSplit) return ErrorNumber.NotSupported;

        Stream stream;

        if(entry.UncompressedSize == 0)
            stream = new MemoryStream([]);
        else
        {
            stream = new OffsetStream(new NonClosableStream(_stream),
                                      entry.DataOffset,
                                      entry.DataOffset + entry.CompressedSize - 1);

            switch(entry.Method)
            {
                case CompressionType.Stored:
                    // No decompression needed
                    break;

                case CompressionType.Lz77:
                    stream = new AceStream(stream, entry.UncompressedSize, 1);

                    break;

                case CompressionType.Blocked:
                    stream = new AceStream(stream, entry.UncompressedSize, 2);

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