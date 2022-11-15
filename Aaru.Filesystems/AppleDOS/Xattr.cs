// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Apple DOS extended attributes (file type).
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

namespace Aaru.Filesystems;

public sealed partial class AppleDOS
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

        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 30)
            return ErrorNumber.NameTooLong;

        xattrs = new List<string>();

        if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                      string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                      string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0)) {}
        else
        {
            if(!_catalogCache.ContainsKey(filename))
                return ErrorNumber.NoSuchFile;

            xattrs.Add("com.apple.dos.type");

            if(_debug)
                xattrs.Add("com.apple.dos.tracksectorlist");
        }

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

        string filename = pathElements[0].ToUpperInvariant();

        if(filename.Length > 30)
            return ErrorNumber.NameTooLong;

        if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                      string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                      string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
            return ErrorNumber.NoSuchExtendedAttribute;

        if(!_catalogCache.ContainsKey(filename))
            return ErrorNumber.NoSuchFile;

        if(string.Compare(xattr, "com.apple.dos.type", StringComparison.InvariantCulture) == 0)
        {
            if(!_fileTypeCache.TryGetValue(filename, out byte type))
                return ErrorNumber.InvalidArgument;

            buf    = new byte[1];
            buf[0] = type;

            return ErrorNumber.NoError;
        }

        if(string.Compare(xattr, "com.apple.dos.tracksectorlist", StringComparison.InvariantCulture) != 0 ||
           !_debug)
            return ErrorNumber.NoSuchExtendedAttribute;

        if(!_extentCache.TryGetValue(filename, out byte[] ts))
            return ErrorNumber.InvalidArgument;

        buf = new byte[ts.Length];
        Array.Copy(ts, 0, buf, 0, buf.Length);

        return ErrorNumber.NoError;
    }
}