using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Arc
{
    List<Entry> _entries;

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < Marshal.SizeOf<Header>()) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        var hdr = new byte[Marshal.SizeOf<Header>()];

        _stream.ReadExactly(hdr, 0, hdr.Length);

        Header header = Marshal.ByteArrayToStructureLittleEndian<Header>(hdr);

        // Not a valid marker
        if(header.marker != MARKER) return ErrorNumber.InvalidArgument;

        switch((int)header.method)
        {
            // Not a valid compression method
            case > 12 and < 20:
            // Not a valid informational item
            case > 22 and < 30:
            // Not a valid control item
            case > 31:
                return ErrorNumber.InvalidArgument;
        }

        // Compressed size is larger than file size
        // Hope for the best
        if(header.compressed >= _stream.Length && (int)header.method != 31) return ErrorNumber.InvalidArgument;

        Opened           = true;
        _encoding        = encoding ?? Encoding.ASCII;
        _stream.Position = 0;
        _entries         = [];

        _features = ArchiveSupportedFeature.SupportsCompression |
                    ArchiveSupportedFeature.SupportsFilenames   |
                    ArchiveSupportedFeature.HasEntryTimestamp;

        var    path       = "";
        string longname   = null;
        string comment    = null;
        string attributes = null;
        var    br         = new BinaryReader(_stream);

        // Process headers
        while(true)
        {
            // Truncated archive without an end of archive marker
            if(_stream.Position + 2 > _stream.Length) break;

            byte peekedByte = br.ReadByte();
            AaruLogging.Debug(MODULE_NAME, "[navy]peekedByte[/] = [teal]0x{0:X2}[/]", peekedByte);
            peekedByte = br.ReadByte();
            AaruLogging.Debug(MODULE_NAME, "[navy]peekedByte[/] = [teal]0x{0:X2}[/]", peekedByte);

            if((Method)peekedByte == Method.EndOfArchive) break;

            if((Method)peekedByte == Method.SubdirectoryEnd)
            {
                // Remove last directory from path
                int lastSlash = path.LastIndexOf(Path.DirectorySeparatorChar);

                path = lastSlash >= 0 ? path[..lastSlash] : "";

                continue;
            }

            _stream.Position -= 2;

            // Truncated header at end of file
            if(_stream.Position + hdr.Length > _stream.Length) break;

            // Decode header
            _stream.ReadExactly(hdr, 0, hdr.Length);
            header = Marshal.ByteArrayToStructureLittleEndian<Header>(hdr);

            AaruLogging.Debug(MODULE_NAME, "[navy]header.marker[/] = [teal]0x{0:X2}[/]", header.marker);
            AaruLogging.Debug(MODULE_NAME, "[navy]header.method[/] = [teal]{0}[/]",      header.method);

            AaruLogging.Debug(MODULE_NAME,
                              "[navy]header.filename[/] = [green]\"{0}\"[/]",
                              StringHandlers.CToString(header.filename));

            AaruLogging.Debug(MODULE_NAME, "[navy]header.compressed[/] = [teal]{0}[/]",   header.compressed);
            AaruLogging.Debug(MODULE_NAME, "[navy]header.date[/] = [teal]{0}[/]",         header.date);
            AaruLogging.Debug(MODULE_NAME, "[navy]header.time[/] = [teal]{0}[/]",         header.time);
            AaruLogging.Debug(MODULE_NAME, "[navy]header.crc[/] = [teal]0x{0:X4}[/]",     header.crc);
            AaruLogging.Debug(MODULE_NAME, "[navy]header.uncompressed[/] = [teal]{0}[/]", header.uncompressed);

            // Sizes that would rewind the stream or point past end of file make no sense
            if(header.compressed < 0 || header.compressed > _stream.Length - _stream.Position)
                return ErrorNumber.InvalidArgument;

            if(header.method == Method.FileInformation)
            {
                long infoStart   = _stream.Position;
                int  recordsSize = header.compressed;
                var  recordsRead = 0;

                while(recordsRead < recordsSize)
                {
                    if(_stream.Position + 3 > _stream.Length) break;

                    ushort len       = br.ReadUInt16();
                    var    finfoType = (FileInformationType)br.ReadByte();

                    // A record cannot be smaller than its own length field and type byte
                    if(len < 3) break;

                    byte[] info = br.ReadBytes(len - 3);

                    recordsRead += len;

                    switch(finfoType)
                    {
                        case FileInformationType.Description:
                            comment = StringHandlers.CToString(info, _encoding);

                            break;
                        case FileInformationType.Attributes:
                            attributes = StringHandlers.CToString(info, _encoding);

                            break;
                        case FileInformationType.LongName:
                            longname = StringHandlers.CToString(info, _encoding);

                            break;
                    }
                }

                // File information describes the next entry, it is not an entry itself
                _stream.Position = infoStart + header.compressed;

                continue;
            }

            string filename;

            Entry entry;

            if(header.method == Method.Subdirectory)
            {
                filename = StringHandlers.CToString(header.filename, _encoding);
                if(longname is not null) filename = longname;

                path = Path.Combine(path, filename);

                entry = new Entry
                {
                    Method        = header.method,
                    Filename      = path,
                    Compressed    = 0,
                    Uncompressed  = 0,
                    LastWriteTime = DateHandlers.DosToDateTime(header.date, header.time),
                    DataOffset    = 0,
                    Comment       = comment,
                    Attributes    = FileAttributes.Directory
                };

                _features |= ArchiveSupportedFeature.HasExplicitDirectories |
                             ArchiveSupportedFeature.SupportsSubdirectories;

                if(attributes is not null)
                {
                    if(attributes.Contains('A')) entry.Attributes |= FileAttributes.Archive;
                    if(attributes.Contains('R')) entry.Attributes |= FileAttributes.ReadOnly;
                    if(attributes.Contains('H')) entry.Attributes |= FileAttributes.Hidden;
                    if(attributes.Contains('S')) entry.Attributes |= FileAttributes.System;
                }

                longname   = null;
                comment    = null;
                attributes = null;

                _entries.Add(entry);

                continue;
            }

            filename = StringHandlers.CToString(header.filename, _encoding);
            if(longname is not null) filename = longname;

            if(header.method == Method.UnpackedOld) _stream.Position -= 4;

            entry = new Entry
            {
                Method        = header.method,
                Filename      = Path.Combine(path, filename),
                Compressed    = header.compressed,
                Uncompressed  = header.method == Method.UnpackedOld ? header.compressed : header.uncompressed,
                LastWriteTime = DateHandlers.DosToDateTime(header.date, header.time),
                DataOffset    = _stream.Position,
                Comment       = comment,
                Attributes    = FileAttributes.Normal
            };

            if(attributes is not null)
            {
                if(attributes.Contains('A')) entry.Attributes |= FileAttributes.Archive;
                if(attributes.Contains('R')) entry.Attributes |= FileAttributes.ReadOnly;
                if(attributes.Contains('H')) entry.Attributes |= FileAttributes.Hidden;
                if(attributes.Contains('S')) entry.Attributes |= FileAttributes.System;
            }

            longname   = null;
            comment    = null;
            attributes = null;

            _entries.Add(entry);

            _stream.Position += header.compressed;
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
        _entries?.Clear();

        _stream = null;
        Opened  = false;
    }

#endregion
}