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
//     Identifies CPCEMU disk images.
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

using System;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Cpcdsk
{
#region IMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return false;

        var headerB = new byte[256];
        stream.EnsureRead(headerB, 0, 256);

        int pos;

        for(pos = 0; pos < 254; pos++)
        {
            if(headerB[pos]     == 0x0D &&
               headerB[pos + 1] == 0x0A)
                break;
        }

        if(pos >= 254)
            return false;

        string magic = Encoding.ASCII.GetString(headerB, 0, pos);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.magic_equals_0_quoted, magic);

        return string.Compare(CPCDSK_ID, magic, StringComparison.InvariantCultureIgnoreCase) == 0 ||
               string.Compare(EDSK_ID,   magic, StringComparison.InvariantCultureIgnoreCase) == 0 ||
               string.Compare(DU54_ID,   magic, StringComparison.InvariantCultureIgnoreCase) == 0;
    }

#endregion
}