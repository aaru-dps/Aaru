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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems.CPM
{
    internal partial class CPM
    {
        /// <summary>Reads an extended attribute, alternate data stream or fork from the given file.</summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="xattr">Extendad attribute, alternate data stream or fork name.</param>
        /// <param name="buf">Buffer.</param>
        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            if(!mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(!fileCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                return Errno.NoSuchFile;

            if(string.Compare(xattr, "com.caldera.cpm.password", StringComparison.InvariantCulture) == 0)
                if(!passwordCache.TryGetValue(pathElements[0].ToUpperInvariant(), out buf))
                    return Errno.NoError;

            if(string.Compare(xattr, "com.caldera.cpm.password.text", StringComparison.InvariantCulture) != 0)
                return Errno.NoSuchExtendedAttribute;

            return !passwordCache.TryGetValue(pathElements[0].ToUpperInvariant(), out buf) ? Errno.NoError
                       : Errno.NoSuchExtendedAttribute;
        }

        /// <summary>Lists all extended attributes, alternate data streams and forks of the given file.</summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;

            if(!mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(!fileCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                return Errno.NoSuchFile;

            xattrs = new List<string>();

            if(passwordCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                xattrs.Add("com.caldera.cpm.password");

            if(decodedPasswordCache.ContainsKey(pathElements[0].ToUpperInvariant()))
                xattrs.Add("com.caldera.cpm.password.text");

            return Errno.NoError;
        }
    }
}