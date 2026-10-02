// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
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

// ReSharper disable UnusedType.Local

// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Local

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
public sealed partial class AppleHFS
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        xattrs = [];

        // Get file entry
        ErrorNumber error = GetFileEntry(normalizedPath, out CatalogEntry entry);

        if(error != ErrorNumber.NoError) return error;

        // Only files can have these xattrs, not directories
        if(entry is not FileEntry fileEntry) return ErrorNumber.NoError;

        // Add Finder Info xattr (always present for files)
        xattrs.Add(Xattrs.XATTR_APPLE_FINDER_INFO);

        // Add HFS creator and type xattrs
        xattrs.Add(Xattrs.XATTR_APPLE_HFS_CREATOR);
        xattrs.Add(Xattrs.XATTR_APPLE_HFS_OSTYPE);

        // Add Resource Fork xattr if it exists and is non-empty
        if(fileEntry.ResourceForkLogicalSize > 0) xattrs.Add(Xattrs.XATTR_APPLE_RESOURCE_FORK);

        xattrs.Sort();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(xattr)) return ErrorNumber.InvalidArgument;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Get file entry
        ErrorNumber error = GetFileEntry(normalizedPath, out CatalogEntry entry);

        if(error != ErrorNumber.NoError) return error;

        // Only files can have these xattrs
        if(entry is not FileEntry fileEntry) return ErrorNumber.NoSuchExtendedAttribute;

        // Handle Finder Info xattr: concatenate FInfo (16 bytes) + FXInfo (16 bytes) = 32 bytes
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_FINDER_INFO, StringComparison.OrdinalIgnoreCase))
        {
            buf = new byte[32];

            // FInfo (16 bytes)
            byte[] finderInfoBytes = Marshal.StructureToByteArrayBigEndian(fileEntry.FinderInfo);
            Array.Copy(finderInfoBytes, 0, buf, 0, Math.Min(finderInfoBytes.Length, 16));

            // FXInfo (16 bytes)
            byte[] extendedFinderInfoBytes = Marshal.StructureToByteArrayBigEndian(fileEntry.ExtendedFinderInfo);
            Array.Copy(extendedFinderInfoBytes, 0, buf, 16, Math.Min(extendedFinderInfoBytes.Length, 16));

            return ErrorNumber.NoError;
        }

        // Handle HFS creator xattr (4 bytes, as stored, most significant byte first)
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_HFS_CREATOR, StringComparison.OrdinalIgnoreCase))
        {
            buf = BigEndianBitConverter.GetBytes(fileEntry.FinderInfo.fdCreator);

            return ErrorNumber.NoError;
        }

        // Handle HFS type xattr (4 bytes, as stored, most significant byte first)
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_HFS_OSTYPE, StringComparison.OrdinalIgnoreCase))
        {
            buf = BigEndianBitConverter.GetBytes(fileEntry.FinderInfo.fdType);

            return ErrorNumber.NoError;
        }

        // Handle Resource Fork xattr
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_RESOURCE_FORK, StringComparison.OrdinalIgnoreCase))
        {
            return fileEntry.ResourceForkLogicalSize == 0
                       ? ErrorNumber.NoSuchExtendedAttribute
                       : ReadFork(fileEntry,
                                  ForkType.Resource,
                                  fileEntry.ResourceForkLogicalSize,
                                  fileEntry.ResourceForkExtents,
                                  out buf);
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }
}