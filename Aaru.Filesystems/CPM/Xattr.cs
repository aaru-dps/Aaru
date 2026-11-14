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
// Copyright © 2011-2026 Natalia Portillo
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

        string filename = GetFilename(path);

        if(filename is null) return ErrorNumber.NotSupported;

        if(!_fileCache.ContainsKey(filename)) return ErrorNumber.NoSuchFile;

        if(string.Equals(xattr, "com.caldera.cpm.password", StringComparison.InvariantCulture))
        {
            if(!_passwordCache.TryGetValue(filename, out buf)) return ErrorNumber.NoError;
        }

        if(!string.Equals(xattr, "com.caldera.cpm.password.text", StringComparison.InvariantCulture))
            return ErrorNumber.NoSuchExtendedAttribute;

        return !_passwordCache.TryGetValue(filename, out buf)
                   ? ErrorNumber.NoError
                   : ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string filename = GetFilename(path);

        if(filename is null) return ErrorNumber.NotSupported;

        if(!_fileCache.ContainsKey(filename)) return ErrorNumber.NoSuchFile;

        xattrs = [];

        if(_passwordCache.ContainsKey(filename)) xattrs.Add("com.caldera.cpm.password");

        if(_decodedPasswordCache.ContainsKey(filename))
            xattrs.Add("com.caldera.cpm.password.text");

        return ErrorNumber.NoError;
    }

#endregion
}