using System;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems
{
    public partial class OperaFS
    {
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock) => throw new NotImplementedException();

        public Errno GetAttributes(string path, out FileAttributes attributes) => throw new NotImplementedException();

        public Errno Read(string path, long offset, long size, ref byte[] buf) => throw new NotImplementedException();

        public Errno Stat(string path, out FileEntryInfo stat) => throw new NotImplementedException();
    }
}