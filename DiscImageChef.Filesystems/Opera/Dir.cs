using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;

namespace DiscImageChef.Filesystems
{
    public partial class OperaFS
    {
        public Errno ReadDir(string path, out List<string> contents) => throw new NotImplementedException();

        Dictionary<string, DirectoryEntryWithPointers> DecodeDirectory(int firstBlock)
        {
            Dictionary<string, DirectoryEntryWithPointers> entries =
                new Dictionary<string, DirectoryEntryWithPointers>();

            int nextBlock = firstBlock;

            do
            {
                byte[] data =
                    image.ReadSectors((ulong)(nextBlock * volumeBlockSizeRatio), volumeBlockSizeRatio);
                DirectoryHeader header = Marshal.ByteArrayToStructureBigEndian<DirectoryHeader>(data);
                nextBlock = header.next_block;

                int off = (int)header.first_used;

                DirectoryEntry entry = new DirectoryEntry();

                while(off + DirectoryEntrySize < data.Length)
                {
                    entry = Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(data, off, DirectoryEntrySize);
                    string name = StringHandlers.CToString(entry.name, Encoding);

                    DirectoryEntryWithPointers entryWithPointers =
                        new DirectoryEntryWithPointers {entry = entry, pointers = new uint[entry.last_copy + 1]};

                    for(int i = 0; i <= entry.last_copy; i++)
                        entryWithPointers.pointers[i] =
                            BigEndianBitConverter.ToUInt32(data, off + DirectoryEntrySize + i * 4);

                    entries.Add(name, entryWithPointers);

                    if((entry.flags & (uint)FileFlags.LastEntry)        != 0 ||
                       (entry.flags & (uint)FileFlags.LastEntryInBlock) != 0) break;

                    off += (int)(DirectoryEntrySize + (entry.last_copy + 1) * 4);
                }

                if((entry.flags & (uint)FileFlags.LastEntry) != 0) break;
            }
            while(nextBlock != -1);

            return entries;
        }
    }
}