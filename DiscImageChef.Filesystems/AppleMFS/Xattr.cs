// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Apple Macintosh File System extended attributes
//     (Finder Info, Resource Fork, etc.)
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS
    {
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            xattrs = new List<string>();

            if(debug)
                if(string.Compare(path, "$",       StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$Boot",   StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$MDB",    StringComparison.InvariantCulture) == 0)
                {
                    if(device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                        xattrs.Add("com.apple.macintosh.tags");

                    return Errno.NoError;
                }

            if(!filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId)) return Errno.NoSuchFile;

            if(!idToEntry.TryGetValue(fileId, out MFS_FileEntry entry)) return Errno.NoSuchFile;

            if(entry.flRLgLen > 0)
            {
                xattrs.Add("com.apple.ResourceFork");
                if(debug && device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                    xattrs.Add("com.apple.ResourceFork.tags");
            }

            if(!ArrayHelpers.ArrayIsNullOrEmpty(entry.flUsrWds)) xattrs.Add("com.apple.FinderInfo");

            if(debug && device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag) && entry.flLgLen > 0)
                xattrs.Add("com.apple.macintosh.tags");

            xattrs.Sort();

            return Errno.NoError;
        }

        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            if(debug)
                if(string.Compare(path, "$",       StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$Boot",   StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$MDB",    StringComparison.InvariantCulture) == 0)
                    if(device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag) &&
                       string.Compare(xattr, "com.apple.macintosh.tags", StringComparison.InvariantCulture) == 0)
                    {
                        if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                        {
                            buf = new byte[directoryTags.Length];
                            Array.Copy(directoryTags, 0, buf, 0, buf.Length);
                            return Errno.NoError;
                        }

                        if(string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0)
                        {
                            buf = new byte[bitmapTags.Length];
                            Array.Copy(bitmapTags, 0, buf, 0, buf.Length);
                            return Errno.NoError;
                        }

                        if(string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
                        {
                            buf = new byte[bootTags.Length];
                            Array.Copy(bootTags, 0, buf, 0, buf.Length);
                            return Errno.NoError;
                        }

                        if(string.Compare(path, "$MDB", StringComparison.InvariantCulture) == 0)
                        {
                            buf = new byte[mdbTags.Length];
                            Array.Copy(mdbTags, 0, buf, 0, buf.Length);
                            return Errno.NoError;
                        }
                    }
                    else
                        return Errno.NoSuchExtendedAttribute;

            Errno error;

            if(!filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId)) return Errno.NoSuchFile;

            if(!idToEntry.TryGetValue(fileId, out MFS_FileEntry entry)) return Errno.NoSuchFile;

            if(entry.flRLgLen                                                                     > 0 &&
               string.Compare(xattr, "com.apple.ResourceFork", StringComparison.InvariantCulture) == 0)
            {
                error = ReadFile(path, out buf, true, false);
                return error;
            }

            if(entry.flRLgLen                                                                          > 0 &&
               string.Compare(xattr, "com.apple.ResourceFork.tags", StringComparison.InvariantCulture) == 0)
            {
                error = ReadFile(path, out buf, true, true);
                return error;
            }

            if(!ArrayHelpers.ArrayIsNullOrEmpty(entry.flUsrWds) &&
               string.Compare(xattr, "com.apple.FinderInfo", StringComparison.InvariantCulture) == 0)
            {
                buf = new byte[16];
                Array.Copy(entry.flUsrWds, 0, buf, 0, 16);
                return Errno.NoError;
            }

            if(!debug || !device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag) ||
               string.Compare(xattr, "com.apple.macintosh.tags", StringComparison.InvariantCulture) != 0)
                return Errno.NoSuchExtendedAttribute;

            error = ReadFile(path, out buf, false, true);
            return error;
        }
    }
}