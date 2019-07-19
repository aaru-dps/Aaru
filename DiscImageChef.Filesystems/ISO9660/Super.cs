using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options,     string    @namespace) =>
            throw new NotImplementedException();

        public Errno Unmount() => throw new NotImplementedException();

        public Errno StatFs(out FileSystemInfo stat) => throw new NotImplementedException();
    }
}