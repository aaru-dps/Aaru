using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class Amg
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        encoding ??= Encoding.ASCII;

        if(filter.DataForkLength < Marshal.SizeOf<ArcHeader>()) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        var hdr = new byte[Marshal.SizeOf<ArcHeader>()];

        _stream.ReadExactly(hdr, 0, hdr.Length);

        ArcHeader header = Marshal.ByteArrayToStructureLittleEndian<ArcHeader>(hdr);

        // Not a valid magic
        if(header.magic != ARC_MAGIC) return ErrorNumber.InvalidArgument;


        // Skip comment
        _stream.Position += header.commentLength;

        int fileHeaderLen = Marshal.SizeOf<FileHeader>();

        _files = [];

        while(_stream.Position + fileHeaderLen < _stream.Length)
        {
            var fileHdr = new byte[fileHeaderLen];

            _stream.ReadExactly(fileHdr, 0, fileHdr.Length);

            FileHeader fh = Marshal.ByteArrayToStructureLittleEndian<FileHeader>(fileHdr);

            // Not a valid file magic
            if(fh.magic != FILE_MAGIC) break;

            // Compressed size includes the header, path and comment; reject sizes smaller than those
            if(fh.compressed < fh.pathLength + fh.commentLength + fileHeaderLen) return ErrorNumber.InvalidArgument;

            var entry = new FileEntry
            {
                Attributes   = (FileAttributes)fh.attr,
                Compressed   = (uint)(fh.compressed - fh.pathLength - fh.commentLength - fileHeaderLen),
                Crc          = fh.crc,
                Flags        = fh.flags,
                LastWrite    = DateHandlers.DosToDateTime(fh.date, fh.time),
                Uncompressed = fh.uncompressed,
                Filename     = StringHandlers.CToString(fh.filename, encoding)
            };

            string extension = StringHandlers.CToString(fh.extension, encoding);

            if(!string.IsNullOrEmpty(extension)) entry.Filename += "." + extension;

            if(fh.pathLength > 0)
            {
                var buffer = new byte[fh.pathLength];
                _stream.ReadExactly(buffer, 0, buffer.Length);
                string path = StringHandlers.CToString(buffer, encoding);
                path           = path.Replace('\\', Path.DirectorySeparatorChar);
                entry.Filename = Path.Combine(path, entry.Filename);
            }

            if(fh.commentLength > 0)
            {
                var buffer = new byte[fh.commentLength];
                _stream.ReadExactly(buffer, 0, buffer.Length);
                entry.Comment = StringHandlers.CToString(buffer, encoding);
            }

            entry.Offset = _stream.Position;

            _files.Add(entry);

            // Skip compressed data
            _stream.Position += entry.Compressed;
        }

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        // Already closed
        if(!Opened) return;

        _stream?.Close();
        _files?.Clear();

        _stream = null;
        Opened  = false;
    }

#endregion
}