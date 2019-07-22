using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;
            if(!mounted) return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);
            if(err != Errno.NoError) return err;

            xattrs = new List<string>();

            if(entry.AssociatedFile != null)
            {
                xattrs.Add("org.iso.9660.ea");
            }

            return Errno.NoError;
        }

        public Errno GetXattr(string path, string xattr, ref byte[] buf) => throw new NotImplementedException();
    }
}