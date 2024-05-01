// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for RS-IDE disk images.
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
using System.Text;

namespace Aaru.Images;

public sealed partial class RsIde
{
    static byte[] ScrambleAtaString(string text, int length)
    {
        byte[] inbuf = Encoding.ASCII.GetBytes(text);

        if(inbuf.Length % 2 != 0)
        {
            var tmpbuf = new byte[inbuf.Length + 1];
            Array.Copy(inbuf, 0, tmpbuf, 0, inbuf.Length);
            tmpbuf[^1] = 0x20;
            inbuf      = tmpbuf;
        }

        var outbuf = new byte[inbuf.Length];

        for(var i = 0; i < length; i += 2)
        {
            outbuf[i] = inbuf[i + 1];
            outbuf[i            + 1] = inbuf[i];
        }

        var retBuf = new byte[length];

        for(var i = 0; i < length; i++) retBuf[i] = 0x20;

        Array.Copy(outbuf, 0, retBuf, 0, outbuf.Length >= length ? length : outbuf.Length);

        return retBuf;
    }
}