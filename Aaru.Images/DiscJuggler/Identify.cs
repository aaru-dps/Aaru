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
//     Identifies DiscJuggler disc images.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class DiscJuggler
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        _imageStream = imageFilter.GetDataForkStream();

        _imageStream.Seek(-4, SeekOrigin.End);
        var dscLenB = new byte[4];
        _imageStream.EnsureRead(dscLenB, 0, 4);
        var dscLen = BitConverter.ToInt32(dscLenB, 0);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dscLen = {0}", dscLen);

        if(dscLen >= _imageStream.Length) return false;

        var descriptor = new byte[dscLen];
        _imageStream.Seek(-dscLen, SeekOrigin.End);
        _imageStream.EnsureRead(descriptor, 0, dscLen);

        // Sessions
        if(descriptor.Length < 1 || descriptor[0] > 99 || descriptor[0] == 0) return false;

        // Seems all sessions start with this data
        if(descriptor[1]  != 0x00 ||
           descriptor[3]  != 0x00 ||
           descriptor[4]  != 0x00 ||
           descriptor[5]  != 0x00 ||
           descriptor[6]  != 0x00 ||
           descriptor[7]  != 0x00 ||
           descriptor[8]  != 0x00 ||
           descriptor[9]  != 0x00 ||
           descriptor[10] != 0x01 ||
           descriptor[11] != 0x00 ||
           descriptor[12] != 0x00 ||
           descriptor[13] != 0x00 ||
           descriptor[14] != 0xFF ||
           descriptor[15] != 0xFF)
            return false;

        // Too many tracks
        return descriptor[2] <= 99;
    }

#endregion
}