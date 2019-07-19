using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno ReadDir(string path, out List<string> contents) => throw new NotImplementedException();

        List<DecodedDirectoryEntry> DecodeCdiDirectory(byte[] data) => throw new NotImplementedException();

        List<DecodedDirectoryEntry> DecodeHighSierraDirectory(byte[] data) => throw new NotImplementedException();

        // TODO: Implement system area
        List<DecodedDirectoryEntry> DecodeIsoDirectory(byte[] data)
        {
            List<DecodedDirectoryEntry> entries  = new List<DecodedDirectoryEntry>();
            int                         entryOff = 0;

            while(entryOff + DirectoryRecordSize < data.Length)
            {
                DirectoryRecord record =
                    Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(data, entryOff,
                                                                              Marshal.SizeOf<DirectoryRecord>());

                if(record.length == 0) break;

                // Special entries for current and parent directories, skip them
                if(record.name_len == 1)
                    if(data[entryOff + DirectoryRecordSize] == 0 || data[entryOff + DirectoryRecordSize] == 1)
                    {
                        entryOff += record.length;
                        continue;
                    }

                DecodedDirectoryEntry entry = new DecodedDirectoryEntry();

                entry.Extent               = record.extent;
                entry.Size                 = record.size;
                entry.Flags                = record.flags;
                entry.FileUnitSize         = record.file_unit_size;
                entry.Interleave           = record.interleave;
                entry.VolumeSequenceNumber = record.volume_sequence_number;
                entry.IsoFilename =
                    Encoding.ASCII.GetString(data, entryOff + DirectoryRecordSize, record.name_len);
                entry.Timestamp = DecodeIsoDateTime(record.date);

                // TODO: Multi-extent files
                if(entries.All(e => e.IsoFilename != entry.IsoFilename)) entries.Add(entry);

                entryOff += record.length;
            }

            return entries;
        }
    }
}