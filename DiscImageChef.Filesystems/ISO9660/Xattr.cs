using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno ListXAttr(string path, out List<string> xattrs) => throw new NotImplementedException();

        public Errno GetXattr(string path, string xattr, ref byte[] buf) => throw new NotImplementedException();
    }
}