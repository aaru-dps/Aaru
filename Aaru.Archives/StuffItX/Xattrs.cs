using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Logging;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Archives;

public sealed partial class StuffItX
{
    static byte[] TypeCodeFromFinderInfo(byte[] finderInfo, int offset)
    {
        if(finderInfo == null || finderInfo.Length < offset + 4) return null;

        var bytes = new byte[4];
        bytes[0] = finderInfo[offset];
        bytes[1] = finderInfo[offset + 1];
        bytes[2] = finderInfo[offset + 2];
        bytes[3] = finderInfo[offset + 3];

        // Check if all zeros (no type/creator)
        if(bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0) return null;

        return bytes;
    }

    bool HasResourceFork(int entryNumber)
    {
        Entry entry = _entries[entryNumber];

        if(entry.IsDirectory) return false;

        // Look for a resource fork entry that shares the same filename/path
        GetFilename(entryNumber, out string dataName);

        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            if(i == entryNumber) continue;

            Entry other = _entries[i];

            if(!other.IsResourceFork) continue;

            GetFilename(i, out string resName);

            if(dataName != null && dataName == resName) return true;
        }

        return false;
    }

    ErrorNumber ReadResourceFork(int entryNumber, out byte[] buffer)
    {
        buffer = null;

        Entry entry = _entries[entryNumber];
        GetFilename(entryNumber, out string dataName);

        // Find the corresponding resource fork entry
        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            if(i == entryNumber) continue;

            Entry other = _entries[i];

            if(!other.IsResourceFork) continue;

            GetFilename(i, out string resName);

            if(dataName != resName) continue;

            // Found the resource fork — decompress it
            ErrorNumber errno = GetEntry(i, out IFilter resFilter);

            if(errno != ErrorNumber.NoError) return errno;

            try
            {
                Stream resStream = resFilter.GetDataForkStream();
                buffer             = new byte[resStream.Length];
                resStream.Position = 0;
                resStream.ReadExactly(buffer, 0, buffer.Length);

                return ErrorNumber.NoError;
            }
            catch(Exception ex)
            {
                AaruLogging.Debug(MODULE_NAME, "Exception reading resource fork: {0}", ex);

                return ErrorNumber.InOutError;
            }
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber ListXAttr(int entryNumber, out List<string> xattrs)
    {
        xattrs = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        xattrs = [];

        Entry entry = _entries[entryNumber];

        if(entry.Comment is not null) xattrs.Add(XATTR_COMMENT);

        if(entry.IsDirectory) return ErrorNumber.NoError;

        if(entry.FinderInfo is { Length: >= 10 })
        {
            xattrs.Add(XATTR_APPLE_FINDER_INFO);

            if(TypeCodeFromFinderInfo(entry.FinderInfo, 0) != null) xattrs.Add(XATTR_APPLE_HFS_TYPE);
            if(TypeCodeFromFinderInfo(entry.FinderInfo, 4) != null) xattrs.Add(XATTR_APPLE_HFS_CREATOR);
        }

        // Check for resource fork entries that share the same path
        if(HasResourceFork(entryNumber)) xattrs.Add(XATTR_APPLE_RESOURCE_FORK);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(int entryNumber, string xattr, ref byte[] buffer)
    {
        buffer = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        switch(xattr)
        {
            case XATTR_COMMENT:
            {
                if(entry.Comment is null) return ErrorNumber.NoSuchExtendedAttribute;

                buffer = Encoding.UTF8.GetBytes(entry.Comment);

                return ErrorNumber.NoError;
            }

            case XATTR_APPLE_FINDER_INFO:
                if(entry.IsDirectory || entry.FinderInfo == null || entry.FinderInfo.Length < 10)
                    return ErrorNumber.NoSuchExtendedAttribute;

                buffer = entry.FinderInfo;

                return ErrorNumber.NoError;

            case XATTR_APPLE_HFS_TYPE:
                if(entry.IsDirectory || entry.FinderInfo == null) return ErrorNumber.NoSuchExtendedAttribute;

                buffer = TypeCodeFromFinderInfo(entry.FinderInfo, 0);

                return buffer == null ? ErrorNumber.NoSuchExtendedAttribute : ErrorNumber.NoError;

            case XATTR_APPLE_HFS_CREATOR:
                if(entry.IsDirectory || entry.FinderInfo == null) return ErrorNumber.NoSuchExtendedAttribute;

                buffer = TypeCodeFromFinderInfo(entry.FinderInfo, 4);

                return buffer == null ? ErrorNumber.NoSuchExtendedAttribute : ErrorNumber.NoError;

            case XATTR_APPLE_RESOURCE_FORK:
                if(entry.IsDirectory) return ErrorNumber.NoSuchExtendedAttribute;

                return ReadResourceFork(entryNumber, out buffer);

            default:
                return ErrorNumber.NoSuchExtendedAttribute;
        }
    }

#endregion
}