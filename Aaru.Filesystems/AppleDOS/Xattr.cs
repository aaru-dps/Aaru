// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems.AppleDOS
{
    public partial class AppleDOS
    {
        /// <summary>
        ///     Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();
            if(filename.Length > 30) return Errno.NameTooLong;

            xattrs = new List<string>();

            if(debug && (string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0)) { }
            else
            {
                if(!catalogCache.ContainsKey(filename)) return Errno.NoSuchFile;

                xattrs.Add("com.apple.dos.type");

                if(debug) xattrs.Add("com.apple.dos.tracksectorlist");
            }

            return Errno.NoError;
        }

        /// <summary>
        ///     Reads an extended attribute, alternate data stream or fork from the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="xattr">Extended attribute, alternate data stream or fork name.</param>
        /// <param name="buf">Buffer.</param>
        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            string filename = pathElements[0].ToUpperInvariant();
            if(filename.Length > 30) return Errno.NameTooLong;

            if(debug && (string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Vtoc", StringComparison.InvariantCulture) == 0))
                return Errno.NoSuchExtendedAttribute;

            if(!catalogCache.ContainsKey(filename)) return Errno.NoSuchFile;

            if(string.Compare(xattr, "com.apple.dos.type", StringComparison.InvariantCulture) == 0)
            {
                if(!fileTypeCache.TryGetValue(filename, out byte type)) return Errno.InvalidArgument;

                buf    = new byte[1];
                buf[0] = type;
                return Errno.NoError;
            }

            if(string.Compare(xattr, "com.apple.dos.tracksectorlist", StringComparison.InvariantCulture) != 0 || !debug)
                return Errno.NoSuchExtendedAttribute;

            if(!extentCache.TryGetValue(filename, out byte[] ts)) return Errno.InvalidArgument;

            buf = new byte[ts.Length];
            Array.Copy(ts, 0, buf, 0, buf.Length);
            return Errno.NoError;
        }
    }
}