using System.Collections.Generic;
using DiscImageChef.Helpers;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        PathTableEntryInternal[] DecodePathTable(byte[] data)
        {
            if(data is null) return null;

            List<PathTableEntryInternal> table = new List<PathTableEntryInternal>();

            int off = 0;
            while(off < data.Length)
            {
                PathTableEntry entry =
                    Marshal.ByteArrayToStructureBigEndian<PathTableEntry>(data, off, Marshal.SizeOf<PathTableEntry>());

                if(entry.name_len == 0) break;

                off += Marshal.SizeOf<PathTableEntry>();

                string name = Encoding.GetString(data, off, entry.name_len);

                table.Add(new PathTableEntryInternal
                {
                    Extent = entry.start_lbn, Name = name, Parent = entry.parent_dirno
                });

                off += entry.name_len;

                if(entry.name_len % 2 != 0) off++;
            }

            return table.ToArray();
        }
    }
}