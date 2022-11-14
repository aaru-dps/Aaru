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
//     Contains helpers for SuperCardPro flux images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System;
using System.IO;
using System.Text;
using Aaru.Helpers;

public sealed partial class SuperCardPro
{
    static string ReadPStringUtf8(Stream stream, uint position)
    {
        if(position == 0)
            return null;

        stream.Position = position;
        var lenB = new byte[2];
        stream.EnsureRead(lenB, 0, 2);
        var len = BitConverter.ToUInt16(lenB, 0);

        if(len                   == 0 ||
           len + stream.Position >= stream.Length)
            return null;

        var str = new byte[len];
        stream.EnsureRead(str, 0, len);

        return Encoding.UTF8.GetString(str);
    }
}