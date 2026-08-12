using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Compression.CompactPro;
using Aaru.Helpers.IO;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class CompactPro
{
    ErrorNumber ReadResourceFork(Entry entry, out byte[] buffer)
    {
        buffer = null;

        // Validate against the file size, OffsetStream throws on invalid bounds
        if(entry.ResourceCompressedSize                        <= 0 ||
           entry.ResourceOffset                                < 0  ||
           entry.ResourceOffset + entry.ResourceCompressedSize > _stream.Length)
            return ErrorNumber.InvalidArgument;

        Stream stream = new OffsetStream(new NonClosableStream(_stream),
                                         entry.ResourceOffset,
                                         entry.ResourceOffset + entry.ResourceCompressedSize - 1);

        try
        {
            stream = entry.ResourceLzh
                         ? new LzhStream(stream, entry.ResourceUncompressedSize)
                         : new RleStream(stream, entry.ResourceUncompressedSize);

            buffer          = new byte[stream.Length];
            stream.Position = 0;
            stream.ReadExactly(buffer, 0, buffer.Length);

            return ErrorNumber.NoError;
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Exception reading resource fork: {0}", ex);

            return ErrorNumber.InOutError;
        }
    }

    static byte[] BuildFinderInfo(Entry entry)
    {
        // Classic Mac FinderInfo is 16 bytes: type(4) + creator(4) + flags(2) + location(4) + folder(2)
        var info = new byte[16];

        info[0] = (byte)(entry.FileType >> 24);
        info[1] = (byte)(entry.FileType >> 16);
        info[2] = (byte)(entry.FileType >> 8);
        info[3] = (byte)entry.FileType;
        info[4] = (byte)(entry.Creator >> 24);
        info[5] = (byte)(entry.Creator >> 16);
        info[6] = (byte)(entry.Creator >> 8);
        info[7] = (byte)entry.Creator;
        info[8] = (byte)(entry.FinderFlags >> 8);
        info[9] = (byte)entry.FinderFlags;

        return info;
    }

    static byte[] TypeCodeToBytes(uint code)
    {
        var bytes = new byte[4];
        bytes[0] = (byte)(code >> 24);
        bytes[1] = (byte)(code >> 16);
        bytes[2] = (byte)(code >> 8);
        bytes[3] = (byte)code;

        return bytes;
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

        if(_comment is not null) xattrs.Add(XATTR_COMMENT);

        if(entry.IsDirectory) return ErrorNumber.NoError;

        if(entry.ResourceUncompressedSize > 0) xattrs.Add(XATTR_APPLE_RESOURCE_FORK);
        if(entry.FileType != 0 || entry.Creator != 0 || entry.FinderFlags != 0) xattrs.Add(XATTR_APPLE_FINDER_INFO);
        if(entry.FileType != 0) xattrs.Add(XATTR_APPLE_HFS_TYPE);
        if(entry.Creator  != 0) xattrs.Add(XATTR_APPLE_HFS_CREATOR);

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
                if(_comment is null) return ErrorNumber.NoSuchExtendedAttribute;

                buffer = Encoding.UTF8.GetBytes(_comment);

                return ErrorNumber.NoError;

            case XATTR_APPLE_RESOURCE_FORK:
                if(entry.IsDirectory || entry.ResourceUncompressedSize == 0) return ErrorNumber.NoSuchExtendedAttribute;

                if(entry.Encrypted) return ErrorNumber.NotSupported;

                return ReadResourceFork(entry, out buffer);

            case XATTR_APPLE_FINDER_INFO:
                if(entry.IsDirectory || entry is { FileType: 0, Creator: 0, FinderFlags: 0 })
                    return ErrorNumber.NoSuchExtendedAttribute;

                buffer = BuildFinderInfo(entry);

                return ErrorNumber.NoError;

            case XATTR_APPLE_HFS_TYPE:
                if(entry.IsDirectory || entry.FileType == 0) return ErrorNumber.NoSuchExtendedAttribute;

                buffer = TypeCodeToBytes(entry.FileType);

                return ErrorNumber.NoError;

            case XATTR_APPLE_HFS_CREATOR:
                if(entry.IsDirectory || entry.Creator == 0) return ErrorNumber.NoSuchExtendedAttribute;

                buffer = TypeCodeToBytes(entry.Creator);

                return ErrorNumber.NoError;

            default:
                return ErrorNumber.NoSuchExtendedAttribute;
        }
    }

#endregion
}