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

            if(entry.XattrLength     > 0) xattrs.Add("org.iso.9660.ea");
            if(entry.AssociatedFile  != null) xattrs.Add("org.iso.9660.AssociatedFile");
            if(entry.AppleDosType    != null) xattrs.Add("com.apple.dos.type");
            if(entry.AppleProDosType != null) xattrs.Add("com.apple.prodos.type");
            if(entry.ResourceFork    != null) xattrs.Add("com.apple.ResourceFork");
            if(entry.FinderInfo      != null) xattrs.Add("com.apple.FinderInfo");
            if(entry.AppleIcon       != null) xattrs.Add("com.apple.Macintosh.Icon");
            if(entry.AmigaComment    != null) xattrs.Add("com.amiga.comments");

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
                    if(entry.XattrLength == 0) return Errno.NoSuchExtendedAttribute;

                    if(entry.Extents is null) return Errno.InvalidArgument;

                    uint eaSizeInSectors = (uint)(entry.XattrLength / 2048);
                    if(entry.XattrLength % 2048 > 0) eaSizeInSectors++;

                    byte[] ea = ReadSectors(entry.Extents[0].extent, eaSizeInSectors);

                    buf = new byte[entry.AssociatedFile.Size];
                    Array.Copy(ea, 0, buf, 0, buf.LongLength);

                    return Errno.NoError;
                case "org.iso.9660.AssociatedFile":
                    if(entry.AssociatedFile is null) return Errno.NoSuchExtendedAttribute;

                    if(entry.AssociatedFile.Extents is null) return Errno.InvalidArgument;

                    if(entry.AssociatedFile.Size == 0)
                    {
                        buf = new byte[0];
                        return Errno.NoError;
                    }

                    if(entry.AssociatedFile.Extents.Count == 1)
                    {
                        uint associatedFileSize = (uint)(entry.AssociatedFile.Size / 2048);
                        if(entry.AssociatedFile.Size % 2048 > 0) associatedFileSize++;
                        byte[] buffer = ReadSectors(entry.AssociatedFile.Extents[0].extent, associatedFileSize);

                        buf = new byte[entry.AssociatedFile.Size];
                        Array.Copy(buffer, 0, buf, 0, buf.LongLength);
                    }
                    else buf = ReadWithExtents(0, (long)entry.AssociatedFile.Size, entry.AssociatedFile.Extents);

                    return Errno.NoError;
                case "com.apple.dos.type":
                    if(entry.AppleDosType is null) return Errno.NoSuchExtendedAttribute;

                    buf    = new byte[1];
                    buf[0] = entry.AppleDosType.Value;

                    return Errno.NoError;
                case "com.apple.prodos.type":
                    if(entry.AppleProDosType is null) return Errno.NoSuchExtendedAttribute;

                    buf = BitConverter.GetBytes(entry.AppleProDosType.Value);

                    return Errno.NoError;
                case "com.apple.ResourceFork":
                    if(entry.ResourceFork is null) return Errno.NoSuchExtendedAttribute;

                    if(entry.ResourceFork.Extents is null) return Errno.InvalidArgument;

                    if(entry.ResourceFork.Size == 0)
                    {
                        buf = new byte[0];
                        return Errno.NoError;
                    }

                    if(entry.ResourceFork.Extents.Count == 1)
                    {
                        uint rsrcSizeInSectors = (uint)(entry.ResourceFork.Size / 2048);
                        if(entry.AssociatedFile.Size % 2048 > 0) rsrcSizeInSectors++;
                        byte[] buffer = ReadSectors(entry.ResourceFork.Extents[0].extent, rsrcSizeInSectors);

                        buf = new byte[entry.ResourceFork.Size];
                        Array.Copy(buffer, 0, buf, 0, buf.LongLength);
                    }
                    else buf = ReadWithExtents(0, (long)entry.ResourceFork.Size, entry.ResourceFork.Extents);

                    return Errno.NoError;
                case "com.apple.FinderInfo":
                    if(entry.FinderInfo is null) return Errno.NoSuchExtendedAttribute;

                    buf = new byte[16];
                    byte[] tmp = BigEndianBitConverter.GetBytes(entry.FinderInfo.fdType);
                    Array.Copy(tmp, 0, buf, 0, tmp.Length);
                    tmp = BigEndianBitConverter.GetBytes(entry.FinderInfo.fdCreator);
                    Array.Copy(tmp, 0, buf, 4, tmp.Length);
                    tmp = BigEndianBitConverter.GetBytes((ushort)entry.FinderInfo.fdFlags);
                    Array.Copy(tmp, 0, buf, 8, tmp.Length);
                    tmp = BigEndianBitConverter.GetBytes(entry.FinderInfo.fdLocation.x);
                    Array.Copy(tmp, 0, buf, 10, tmp.Length);
                    tmp = BigEndianBitConverter.GetBytes(entry.FinderInfo.fdLocation.y);
                    Array.Copy(tmp, 0, buf, 12, tmp.Length);
                    tmp = BigEndianBitConverter.GetBytes(entry.FinderInfo.fdFldr);
                    Array.Copy(tmp, 0, buf, 14, tmp.Length);

                    return Errno.NoError;
                case "com.apple.Macintosh.Icon":
                    if(entry.AppleIcon is null) return Errno.NoSuchExtendedAttribute;

                    buf = new byte[entry.AppleIcon.Length];
                    Array.Copy(entry.AppleIcon, 0, buf, 0, entry.AppleIcon.Length);

                    return Errno.NoError;
                case "com.amiga.comments":
                    if(entry.AmigaComment is null) return Errno.NoSuchExtendedAttribute;

                    buf = new byte[entry.AmigaComment.Length];
                    Array.Copy(entry.AmigaComment, 0, buf, 0, entry.AmigaComment.Length);

                    return Errno.NoError;
                default: return Errno.NoSuchExtendedAttribute;
            }
        }
    }
}