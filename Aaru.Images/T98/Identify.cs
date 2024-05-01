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
//     Identifies T98 disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class T98
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length % 256 != 0) return false;

        var hdrB = new byte[256];
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        for(var i = 4; i < 256; i++)
        {
            if(hdrB[i] != 0) return false;
        }

        var cylinders = BitConverter.ToInt32(hdrB, 0);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.cylinders_equal_0, cylinders);

        // This format is expanding, so length can be smaller
        // Just grow it, I won't risk false positives...
        return stream.Length == cylinders * 8 * 33 * 256 + 256;
    }

#endregion
}