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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems
{
    public sealed partial class CPM
    {
        /// <inheritdoc />
        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(!_fileCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                return Errno.NoSuchFile;

            if(string.Compare(xattr, "com.caldera.cpm.password", StringComparison.InvariantCulture) == 0)
                if(!_passwordCache.TryGetValue(pathElements[0].ToUpperInvariant(), out buf))
                    return Errno.NoError;

            if(string.Compare(xattr, "com.caldera.cpm.password.text", StringComparison.InvariantCulture) != 0)
                return Errno.NoSuchExtendedAttribute;

            return !_passwordCache.TryGetValue(pathElements[0].ToUpperInvariant(), out buf) ? Errno.NoError
                       : Errno.NoSuchExtendedAttribute;
        }

        /// <inheritdoc />
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;

            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(!_fileCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                return Errno.NoSuchFile;

            xattrs = new List<string>();

            if(_passwordCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                xattrs.Add("com.caldera.cpm.password");

            if(_decodedPasswordCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                xattrs.Add("com.caldera.cpm.password.text");

            return Errno.NoError;
        }
    }
}