// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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

/// <inheritdoc />
public sealed partial class AmigaDOSPlugin
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = [];

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize the path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory has no comment
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
            return ErrorNumber.NoError;

        // Find the entry
        ErrorNumber error = GetBlockForPath(normalizedPath, out uint blockNum);

        if(error != ErrorNumber.NoError) return error;

        // Read the block
        error = ReadBlock(blockNum, out byte[] blockData);

        if(error != ErrorNumber.NoError) return error;

        // If there's a comment, add the xattr to the list
        if(GetInlineCommentBytes(blockData, _hasLongNames).Length > 0 ||
           GetCommentBlockPointer(blockData, _hasLongNames)       != 0)
            xattrs.Add(Xattrs.XATTR_AMIGA_COMMENTS);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        // We only support the amiga.comment xattr
        if(!string.Equals(xattr, Xattrs.XATTR_AMIGA_COMMENTS, StringComparison.Ordinal))
            return ErrorNumber.NotSupported;

        // Normalize the path
        string normalizedPath = path ?? "/";

        if(normalizedPath == "" || normalizedPath == ".") normalizedPath = "/";

        // Root directory has no comment
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
            return ErrorNumber.NoSuchExtendedAttribute;

        // Find the entry
        ErrorNumber error = GetBlockForPath(normalizedPath, out uint blockNum);

        if(error != ErrorNumber.NoError) return error;

        // Read the block
        error = ReadBlock(blockNum, out byte[] blockData);

        if(error != ErrorNumber.NoError) return error;

        error = GetEntryComment(blockData, blockNum, out byte[] comment);

        if(error != ErrorNumber.NoError) return error;

        if(comment.Length == 0) return ErrorNumber.NoSuchExtendedAttribute;

        buf = comment;

        return ErrorNumber.NoError;
    }
}