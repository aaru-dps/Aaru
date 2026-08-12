using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class Stfs
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < Marshal.SizeOf<RemotePackage>()) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        var hdr = new byte[Marshal.SizeOf<RemotePackage>()];

        _stream.ReadExactly(hdr, 0, hdr.Length);

        RemotePackage header = Marshal.ByteArrayToStructureBigEndian<RemotePackage>(hdr);

        if(header.Magic is not (PackageMagic.Console or PackageMagic.Live or PackageMagic.Microsoft))
            return ErrorNumber.InvalidArgument;

        // SVOD is managed as a media image
        if(header.Metadata.DescriptorType != 0) return ErrorNumber.InvalidArgument;

        VolumeDescriptor vd =
            Marshal.ByteArrayToStructureLittleEndian<VolumeDescriptor>(header.Metadata.VolumeDescriptor);

        // Convert big endian int24 to int32
        int fileTableBlockNumber = vd.FileTableBlockNumber[0] << 16 |
                                   vd.FileTableBlockNumber[1] << 8  |
                                   vd.FileTableBlockNumber[2];

        fileTableBlockNumber = ComputeBlockNumber(fileTableBlockNumber,
                                                  (int)header.Metadata.HeaderSize,
                                                  vd.BlockSeparation,
                                                  header.Magic == PackageMagic.Console);

        int fileTablePosition = BlockToPosition(fileTableBlockNumber, (int)header.Metadata.HeaderSize);

        if(vd.FileTableBlockCount                             <= 0 ||
           fileTablePosition                                  < 0  ||
           fileTablePosition + 4096L * vd.FileTableBlockCount > _stream.Length)
            return ErrorNumber.InvalidArgument;

        var buffer = new byte[4096 * vd.FileTableBlockCount];
        _stream.Position = fileTablePosition;
        _stream.ReadExactly(buffer, 0, buffer.Length);

        List<FileTableEntry> entries   = [];
        int                  entrySize = Marshal.SizeOf<FileTableEntry>();
        var                  inPos    = 0;

        do
        {
            FileTableEntry entry = Marshal.ByteArrayToStructureBigEndian<FileTableEntry>(buffer, inPos, entrySize);

            if(entry.FilenameLength == 0) break;

            entries.Add(entry);

            inPos += entrySize;
        } while(inPos + entrySize < buffer.Length);

        _entries = new FileEntry[entries.Count];

        for(var i = 0; i < entries.Count; i++)
        {
            // While entries[i].PathIndicator > 0 recursively inversely prepend entries[PathIndicator].Filename to entries[i].Filename
            int pathIndicator = entries[i].PathIndicator;
            var path          = "";
            int depth         = 0;

            while(pathIndicator > 0)
            {
                // Guard against out of range parents and parent loops
                if(pathIndicator >= entries.Count || ++depth > entries.Count) return ErrorNumber.InvalidArgument;

                path = Path.Combine(StringHandlers.CToString(entries[pathIndicator].Filename, Encoding.ASCII), path);

                pathIndicator = entries[pathIndicator].PathIndicator;
            }

            _entries[i].Filename = path != ""
                                       ? Path.Combine(path,
                                                      StringHandlers.CToString(entries[i].Filename, Encoding.ASCII))
                                       : StringHandlers.CToString(entries[i].Filename, Encoding.ASCII);

            _entries[i].FileSize = entries[i].FileSize;

            _entries[i].StartingBlock = entries[i].StartingBlock[2] << 16 |
                                        entries[i].StartingBlock[1] << 8  |
                                        entries[i].StartingBlock[0];

            _entries[i].StartingBlock = ComputeBlockNumber(_entries[i].StartingBlock,
                                                           (int)header.Metadata.HeaderSize,
                                                           vd.BlockSeparation,
                                                           header.Magic == PackageMagic.Console);

            _entries[i].LastWrite   = DateHandlers.DosToDateTime(entries[i].LastWriteDate,  entries[i].LastWriteTime);
            _entries[i].LastAccess  = DateHandlers.DosToDateTime(entries[i].LastAccessDate, entries[i].LastAccessTime);
            _entries[i].IsDirectory = (entries[i].FilenameLength & 0x80) > 0;
        }

        _headerSize      = (int)header.Metadata.HeaderSize;
        _blockSeparation = vd.BlockSeparation;
        _isConsole       = header.Magic == PackageMagic.Console;

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