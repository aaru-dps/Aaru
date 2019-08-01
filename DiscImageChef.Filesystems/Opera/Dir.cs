using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems
{
    public partial class OperaFS
    {
        public Errno ReadDir(string path, out List<string> contents) => throw new NotImplementedException();
    }
}