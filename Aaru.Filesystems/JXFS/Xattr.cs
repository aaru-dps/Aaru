// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
using Aaru.Helpers;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out DirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = [Xattrs.XATTR_AMIGA_PROTECTION];

        if(ReadAttributes(entry.Node, out _, out _, out string comment) == ErrorNumber.NoError &&
           !string.IsNullOrEmpty(comment))
            xattrs.Add(Xattrs.XATTR_AMIGA_COMMENTS);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out DirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        switch(xattr)
        {
            case Xattrs.XATTR_AMIGA_PROTECTION:
                buf = BigEndianBitConverter.GetBytes(entry.Node.Protection ^ PROTECTION_INVERTED_BITS);

                return ErrorNumber.NoError;
            case Xattrs.XATTR_AMIGA_COMMENTS:
                if(ReadAttributes(entry.Node, out _, out _, out string comment) != ErrorNumber.NoError ||
                   string.IsNullOrEmpty(comment))
                    return ErrorNumber.NoSuchExtendedAttribute;

                buf = _encoding.GetBytes(comment);

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NoSuchExtendedAttribute;
        }
    }
}