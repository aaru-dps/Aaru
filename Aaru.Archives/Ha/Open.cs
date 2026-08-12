using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class Ha
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        encoding ??= Encoding.UTF8;

        if(filter.DataForkLength < Marshal.SizeOf<HaHeader>()) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        var hdr = new byte[Marshal.SizeOf<HaHeader>()];

        _stream.ReadExactly(hdr, 0, hdr.Length);

        HaHeader header = Marshal.ByteArrayToStructureLittleEndian<HaHeader>(hdr);

        // Not a valid magic
        if(header.magic != HA_MAGIC) return ErrorNumber.InvalidArgument;

        _entries = [];

        int fhLen   = Marshal.SizeOf<FHeader>();
        var fhBuf   = new byte[fhLen];
        var pathBuf = new byte[16384];
        var nameBuf = new byte[256];

        while(_stream.Position + fhLen < _stream.Length)
        {
            _stream.ReadExactly(fhBuf, 0, fhLen);
            FHeader fh = Marshal.ByteArrayToStructureLittleEndian<FHeader>(fhBuf);

            int i;

            for(i = 0; i < pathBuf.Length; i++)
            {
                int b = _stream.ReadByte();

                if(b < 0) return ErrorNumber.InvalidArgument; // Makes no sense here

                switch(b)
                {
                    case 0xFF:
                        pathBuf[i] = 0x2F;

                        continue;

                    case 0:
                        pathBuf[i] = 0;

                        break;

                    default:
                        pathBuf[i] = (byte)b;

                        continue;
                }

                break;
            }

            if(i == pathBuf.Length) return ErrorNumber.InvalidArgument; // Got beyond the buffer length

            for(i = 0; i < nameBuf.Length; i++)
            {
                int b = _stream.ReadByte();

                if(b < 0) return ErrorNumber.InvalidArgument; // Makes no sense here

                if(b == 0)
                {
                    nameBuf[i] = 0;

                    break;
                }

                nameBuf[i] = (byte)b;
            }

            if(i == nameBuf.Length) return ErrorNumber.InvalidArgument; // Got beyond the buffer length

            int mdiLen = _stream.ReadByte();

            // End of stream or machine dependent information running past it
            if(mdiLen < 0 || mdiLen > _stream.Length - _stream.Position) return ErrorNumber.InvalidArgument;

            var mdi = new byte[mdiLen];
            _stream.ReadExactly(mdi, 0, mdiLen);

            string path = StringHandlers.CToString(pathBuf, encoding);
            string name = StringHandlers.CToString(nameBuf, encoding);

            // Remove drive letter
            if(path.Length > 3 && path[1] == ':') path = path[2..];

            // ... or leading slash
            if(path.Length > 0 && path[0] == '/') path = path[1..];

            // replace slash with system separator
            path = path.Replace('/', Path.DirectorySeparatorChar);

            var entry = new Entry
            {
                Method       = (Method)(fh.VerType & 0x0F),
                Compressed   = fh.clen,
                Uncompressed = fh.olen,
                LastWrite    = DateHandlers.UnixToDateTime(fh.time),
                DataOffset   = _stream.Position,
                Filename     = Path.Combine(path, name)
            };

            switch(mdi.Length > 0 ? (MdiSource)mdi[0] : (MdiSource)0xFF)
            {
                case MdiSource.MSDOS when mdi.Length >= 2:
                    entry.Attributes = (FileAttributes)mdi[1];

                    break;
                case MdiSource.UNIX when mdi.Length >= Marshal.SizeOf<UnixMdi>():
                {
                    UnixMdi unixMdi = Marshal.ByteArrayToStructureLittleEndian<UnixMdi>(mdi);
                    entry.Mode = unixMdi.attr;
                    entry.Uid  = unixMdi.user;
                    entry.Gid  = unixMdi.group;

                    if(entry.Method == Method.Directory) entry.Attributes = FileAttributes.Directory;

                    break;
                }
            }


            _entries.Add(entry);

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

        _stream = null;
        Opened  = false;
    }

#endregion
}