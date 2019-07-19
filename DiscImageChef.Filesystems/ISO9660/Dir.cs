using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno ReadDir(string path, out List<string> contents) => throw new NotImplementedException();
    }
}