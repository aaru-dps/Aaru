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

            if(entry.AssociatedFile != null) xattrs.Add("org.iso.9660.ea");

            return Errno.NoError;
        }

        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            buf = null;
            if(!mounted) return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);
            if(err != Errno.NoError) return err;

            switch(xattr)
            {
                case "org.iso.9660.ea":
                    if(entry.AssociatedFile is null) return Errno.NoSuchExtendedAttribute;

                    if(entry.AssociatedFile.Extent == 0) return Errno.InvalidArgument;

                    if(entry.AssociatedFile.Size == 0)
                    {
                        buf = new byte[0];
                        return Errno.NoError;
                    }

                    // TODO: XA
                    uint eaSizeInSectors = entry.AssociatedFile.Size / 2048;
                    if(entry.AssociatedFile.Size % 2048 > 0) eaSizeInSectors++;

                    byte[] ea = image.ReadSectors(entry.AssociatedFile.Extent, eaSizeInSectors);

                    buf = new byte[entry.AssociatedFile.Size];
                    Array.Copy(ea, 0, buf, 0, buf.LongLength);

                    return Errno.NoError;
                default: return Errno.NoSuchExtendedAttribute;
            }
        }
    }
}