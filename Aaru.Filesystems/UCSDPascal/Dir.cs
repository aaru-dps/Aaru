// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to show the U.C.S.D. Pascal catalog as a directory.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public sealed partial class PascalPlugin
    {
        /// <inheritdoc />
        public Errno ReadDir(string path, out List<string> contents)
        {
            contents = null;

            if(!_mounted)
                return Errno.AccessDenied;

            if(!string.IsNullOrEmpty(path) &&
               string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
                return Errno.NotSupported;

            contents = _fileEntries.Select(ent => StringHandlers.PascalToString(ent.Filename, Encoding)).ToList();

            if(_debug)
            {
                contents.Add("$");
                contents.Add("$Boot");
            }

            contents.Sort();

            return Errno.NoError;
        }
    }
}