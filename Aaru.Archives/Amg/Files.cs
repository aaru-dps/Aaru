using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using FileAttributes = System.IO.FileAttributes;

namespace Aaru.Archives;

public sealed partial class Amg
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        fileName = _files[entryNumber].Filename;

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

        for(int i = 0, count = _files.Count; i < count; i++)
        {
            if(!_files[i].Filename.Equals(fileName, comparison)) continue;

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

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        length = _files[entryNumber].Compressed;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        length = _files[entryNumber].Uncompressed;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(int entryNumber, out FileEntryInfo stat)
    {
        stat = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        FileEntry entry = _files[entryNumber];

        stat = new FileEntryInfo
        {
            Attributes       = CommonTypes.Structs.FileAttributes.File,
            Blocks           = entry.Uncompressed / 512,
            BlockSize        = 512,
            Length           = entry.Uncompressed,
            LastWriteTime    = entry.LastWrite,
            LastWriteTimeUtc = entry.LastWrite
        };

        if(entry.Uncompressed % 512 != 0) stat.Blocks++;

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

#endregion
}