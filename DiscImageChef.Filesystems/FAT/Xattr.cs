// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Microsoft FAT extended attributes.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.FAT
{
    public partial class FAT
    {
        /// <summary>
        ///     Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        public Errno ListXAttr(string path, out List<string> xattrs) => throw new NotImplementedException();

        /// <summary>
        ///     Reads an extended attribute, alternate data stream or fork from the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="xattr">Extendad attribute, alternate data stream or fork name.</param>
        /// <param name="buf">Buffer.</param>
        public Errno GetXattr(string path, string xattr, ref byte[] buf) => throw new NotImplementedException();

        /// <summary>
        ///     Lists special Apple Lisa filesystem features as extended attributes
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="fileId">File identifier.</param>
        /// <param name="xattrs">Extended attributes.</param>
        Errno ListXAttr(short fileId, out List<string> xattrs) => throw new NotImplementedException();

        /// <summary>
        ///     Lists special Apple Lisa filesystem features as extended attributes
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="fileId">File identifier.</param>
        /// <param name="xattr">Extended attribute name.</param>
        /// <param name="buf">Buffer where the extended attribute will be stored.</param>
        Errno GetXattr(short fileId, string xattr, out byte[] buf) => throw new NotImplementedException();
    }
}