// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin : Filesystem
    {
        public override Errno ReadDir(string path, ref List<string> contents)
        {
            if(!mounted) return Errno.AccessDenied;

            if(!string.IsNullOrEmpty(path) && string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
                return Errno.NotSupported;

            contents = new List<string>();
            foreach(PascalFileEntry ent in fileEntries)
                contents.Add(StringHandlers.PascalToString(ent.filename, CurrentEncoding));

            if(debug)
            {
                contents.Add("$");
                contents.Add("$Boot");
            }

            contents.Sort();
            return Errno.NoError;
        }
    }
}