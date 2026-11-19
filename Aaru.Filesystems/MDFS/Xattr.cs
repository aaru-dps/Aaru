// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;

namespace Aaru.Filesystems;

public sealed partial class MDFS
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = [];

        if(entry.FinderInfo is not null)
        {
            xattrs.Add(Xattrs.XATTR_APPLE_FINDER_INFO);

            if(!entry.IsDirectory)
            {
                xattrs.Add(Xattrs.XATTR_APPLE_HFS_CREATOR);
                xattrs.Add(Xattrs.XATTR_APPLE_HFS_OSTYPE);
            }
        }

        if(!entry.IsDirectory && entry.ResourceLength > 0) xattrs.Add(Xattrs.XATTR_APPLE_RESOURCE_FORK);

        xattrs.Sort();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(xattr)) return ErrorNumber.InvalidArgument;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if(entry.FinderInfo is not null)
        {
            if(string.Equals(xattr, Xattrs.XATTR_APPLE_FINDER_INFO, StringComparison.OrdinalIgnoreCase))
            {
                buf = (byte[])entry.FinderInfo.Clone();

                return ErrorNumber.NoError;
            }

            // FInfo starts with the type and the creator
            if(!entry.IsDirectory &&
               string.Equals(xattr, Xattrs.XATTR_APPLE_HFS_OSTYPE, StringComparison.OrdinalIgnoreCase))
            {
                buf = entry.FinderInfo[..4];

                return ErrorNumber.NoError;
            }

            if(!entry.IsDirectory &&
               string.Equals(xattr, Xattrs.XATTR_APPLE_HFS_CREATOR, StringComparison.OrdinalIgnoreCase))
            {
                buf = entry.FinderInfo[4..8];

                return ErrorNumber.NoError;
            }
        }

        if(entry.IsDirectory                                                                 ||
           entry.ResourceLength == 0                                                         ||
           !string.Equals(xattr, Xattrs.XATTR_APPLE_RESOURCE_FORK, StringComparison.OrdinalIgnoreCase))
            return ErrorNumber.NoSuchExtendedAttribute;

        errno = GetResourceExtents(entry, out Extent[] extents);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ReadFromExtents(extents, 0, entry.ResourceLength, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        buf = data;

        return ErrorNumber.NoError;
    }
}