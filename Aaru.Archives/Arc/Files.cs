using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression.Arc;
using Aaru.Compression.Pak;
using Aaru.Filters;
using Aaru.Helpers.IO;
using FileAttributes = System.IO.FileAttributes;
using PackStream = Aaru.Compression.Arc.PackStream;

namespace Aaru.Archives;

public sealed partial class Arc
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        fileName = _entries[entryNumber].Filename;

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
            if(!_entries[i].Filename.Equals(fileName, comparison)) continue;

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

        length = _entries[entryNumber].Compressed;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        length = _entries[entryNumber].Uncompressed;

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
            Attributes       = CommonTypes.Structs.FileAttributes.None,
            Blocks           = entry.Uncompressed / 512,
            BlockSize        = 512,
            Length           = entry.Uncompressed,
            LastWriteTime    = entry.LastWriteTime,
            LastWriteTimeUtc = entry.LastWriteTime
        };

        if(entry.Uncompressed % 512 != 0) stat.Blocks++;

        if(entry.Attributes.HasFlag(FileAttributes.Directory))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.Directory;
        else
            stat.Attributes |= CommonTypes.Structs.FileAttributes.File;

        if(entry.Attributes.HasFlag(FileAttributes.Archive))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.Archive;

        if(entry.Attributes.HasFlag(FileAttributes.Hidden))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.Hidden;

        if(entry.Attributes.HasFlag(FileAttributes.ReadOnly))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.ReadOnly;

        if(entry.Attributes.HasFlag(FileAttributes.System))
            stat.Attributes |= CommonTypes.Structs.FileAttributes.System;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntry(int entryNumber, out IFilter filter)
    {
        filter = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        if((int)_entries[entryNumber].Method >= 20) return ErrorNumber.InvalidArgument;

        if(_entries[entryNumber].Method > Method.Distill) return ErrorNumber.NotSupported;

        Stream stream = new OffsetStream(new NonClosableStream(_stream),
                                         _entries[entryNumber].DataOffset,
                                         _entries[entryNumber].DataOffset + _entries[entryNumber].Compressed);

        if(_entries[entryNumber].Uncompressed == 0) stream = new MemoryStream([]);

        if(_entries[entryNumber].Method == Method.Pack)
            stream = new PackStream(stream, _entries[entryNumber].Uncompressed);

        if(_entries[entryNumber].Method == Method.Squeeze)
            stream = new SqueezeStream(stream, _entries[entryNumber].Uncompressed);

        if(_entries[entryNumber].Method == Method.CrunchOld)
            stream = new CrunchStream(stream, _entries[entryNumber].Uncompressed, false, false);

        if(_entries[entryNumber].Method == Method.Crunch)
            stream = new CrunchStream(stream, _entries[entryNumber].Uncompressed, true, false);

        if(_entries[entryNumber].Method == Method.CrunchFastHash)
            stream = new CrunchStream(stream, _entries[entryNumber].Uncompressed, true, true);

        if(_entries[entryNumber].Method == Method.CrunchDynamic)
            stream = new LzwStream(stream, _entries[entryNumber].Uncompressed, false);

        if(_entries[entryNumber].Method == Method.Squash)
            stream = new LzwStream(stream, _entries[entryNumber].Uncompressed, true);

        if(_entries[entryNumber].Method == Method.Crush)
            stream = new CrushStream(stream, _entries[entryNumber].Uncompressed);

        if(_entries[entryNumber].Method == Method.Distill)
            stream = new DistillStream(stream, _entries[entryNumber].Uncompressed);

        filter = new ZZZNoFilter();
        ErrorNumber errno = filter.Open(stream);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        stream.Close();

        return errno;
    }

#endregion
}