// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle CP/M extended attributes (password).
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;

namespace Aaru.Filesystems;

public sealed partial class CPM
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
                                           {
                                               '/'
                                           },
                                           StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1) return ErrorNumber.NotSupported;

        if(!_fileCache.ContainsKey(pathElements[0].ToUpperInvariant())) return ErrorNumber.NoSuchFile;

        if(string.Compare(xattr, "com.caldera.cpm.password", StringComparison.InvariantCulture) == 0)
            if(!_passwordCache.TryGetValue(pathElements[0].ToUpperInvariant(), out buf))
                return ErrorNumber.NoError;

        if(string.Compare(xattr, "com.caldera.cpm.password.text", StringComparison.InvariantCulture) != 0)
            return ErrorNumber.NoSuchExtendedAttribute;

        return !_passwordCache.TryGetValue(pathElements[0].ToUpperInvariant(), out buf)
                   ? ErrorNumber.NoError
                   : ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
                                           {
                                               '/'
                                           },
                                           StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1) return ErrorNumber.NotSupported;

        if(!_fileCache.ContainsKey(pathElements[0].ToUpperInvariant())) return ErrorNumber.NoSuchFile;

        xattrs = [];

        if(_passwordCache.ContainsKey(pathElements[0].ToUpperInvariant())) xattrs.Add("com.caldera.cpm.password");

        if(_decodedPasswordCache.ContainsKey(pathElements[0].ToUpperInvariant()))
            xattrs.Add("com.caldera.cpm.password.text");

        return ErrorNumber.NoError;
    }

#endregion
}