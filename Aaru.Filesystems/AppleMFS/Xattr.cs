// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;

namespace Aaru.Filesystems;

// Information from Inside Macintosh Volume II
public sealed partial class AppleMFS
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        path = pathElements[0];

        xattrs = new List<string>();

        if(_debug)
            if(string.Compare(path, "$", StringComparison.InvariantCulture)       == 0 ||
               string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0 ||
               string.Compare(path, "$Boot", StringComparison.InvariantCulture)   == 0 ||
               string.Compare(path, "$MDB", StringComparison.InvariantCulture)    == 0)
            {
                if(_device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                    xattrs.Add("com.apple.macintosh.tags");

                return ErrorNumber.NoError;
            }

        if(!_filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId))
            return ErrorNumber.NoSuchFile;

        if(!_idToEntry.TryGetValue(fileId, out FileEntry entry))
            return ErrorNumber.NoSuchFile;

        if(entry.flRLgLen > 0)
        {
            xattrs.Add("com.apple.ResourceFork");

            if(_debug && _device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
                xattrs.Add("com.apple.ResourceFork.tags");
        }

        xattrs.Add("com.apple.FinderInfo");

        if(_debug                                                                 &&
           _device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag) &&
           entry.flLgLen > 0)
            xattrs.Add("com.apple.macintosh.tags");

        xattrs.Sort();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        path = pathElements[0];

        if(_debug)
            if(string.Compare(path, "$", StringComparison.InvariantCulture)       == 0 ||
               string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0 ||
               string.Compare(path, "$Boot", StringComparison.InvariantCulture)   == 0 ||
               string.Compare(path, "$MDB", StringComparison.InvariantCulture)    == 0)
                if(_device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag) &&
                   string.Compare(xattr, "com.apple.macintosh.tags", StringComparison.InvariantCulture) == 0)
                {
                    if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                    {
                        buf = new byte[_directoryTags.Length];
                        Array.Copy(_directoryTags, 0, buf, 0, buf.Length);

                        return ErrorNumber.NoError;
                    }

                    if(string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0)
                    {
                        buf = new byte[_bitmapTags.Length];
                        Array.Copy(_bitmapTags, 0, buf, 0, buf.Length);

                        return ErrorNumber.NoError;
                    }

                    if(string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
                    {
                        buf = new byte[_bootTags.Length];
                        Array.Copy(_bootTags, 0, buf, 0, buf.Length);

                        return ErrorNumber.NoError;
                    }

                    if(string.Compare(path, "$MDB", StringComparison.InvariantCulture) == 0)
                    {
                        buf = new byte[_mdbTags.Length];
                        Array.Copy(_mdbTags, 0, buf, 0, buf.Length);

                        return ErrorNumber.NoError;
                    }
                }
                else
                    return ErrorNumber.NoSuchExtendedAttribute;

        ErrorNumber error;

        if(!_filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId))
            return ErrorNumber.NoSuchFile;

        if(!_idToEntry.TryGetValue(fileId, out FileEntry entry))
            return ErrorNumber.NoSuchFile;

        switch(entry.flRLgLen)
        {
            case > 0 when string.Compare(xattr, "com.apple.ResourceFork", StringComparison.InvariantCulture) == 0:
                error = ReadFile(path, out buf, true, false);

                return error;
            case > 0 when string.Compare(xattr, "com.apple.ResourceFork.tags", StringComparison.InvariantCulture) == 0:
                error = ReadFile(path, out buf, true, true);

                return error;
        }

        if(string.Compare(xattr, "com.apple.FinderInfo", StringComparison.InvariantCulture) == 0)
        {
            buf = Marshal.StructureToByteArrayBigEndian(entry.flUsrWds);

            return ErrorNumber.NoError;
        }

        if(!_debug                                                                 ||
           !_device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag) ||
           string.Compare(xattr, "com.apple.macintosh.tags", StringComparison.InvariantCulture) != 0)
            return ErrorNumber.NoSuchExtendedAttribute;

        error = ReadFile(path, out buf, false, true);

        return error;
    }
}