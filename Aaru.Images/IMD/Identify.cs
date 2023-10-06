// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Dunfield's IMD disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Imd
{
#region IMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 4)
            return false;

        var hdr = new byte[stream.Length < 256 ? stream.Length : 256];
        stream.EnsureRead(hdr, 0, hdr.Length);

        string hdrStr = StringHandlers.CToString(hdr, Encoding.ASCII);

        // IMD for DOS
        Match imd = new Regex(REGEX_HEADER).Match(hdrStr);

        // SAMdisk
        Match sam = new Regex(REGEX_SAMDISK).Match(hdrStr);

        // z88dk
        Match z88dk = new Regex(REGEX_Z88DK).Match(hdrStr);

        return imd.Success || sam.Success || z88dk.Success;
    }

#endregion
}