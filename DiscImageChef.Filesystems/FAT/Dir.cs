// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Microsoft FAT filesystem directories.
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
        ///     Solves a symbolic link.
        /// </summary>
        /// <param name="path">Link path.</param>
        /// <param name="dest">Link destination.</param>
        public Errno ReadLink(string path, out string dest)
        {
            dest = null;
            return Errno.NotSupported;
        }

        /// <summary>
        ///     Lists contents from a directory.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="contents">Directory contents.</param>
        public Errno ReadDir(string path, out List<string> contents) => throw new NotImplementedException();
    }
}