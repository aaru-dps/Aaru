// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 30_Apple.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Apple MODE PAGE 30h: Apple OEM String.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
#region Apple Mode Page 0x30: Apple OEM String

    static readonly byte[] AppleOEMString = "APPLE COMPUTER, INC."u8.ToArray();

    public static bool IsAppleModePage_30(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40) return false;

        if((pageResponse?[0] & 0x3F) != 0x30) return false;

        if(pageResponse[1] + 2 != pageResponse.Length) return false;

        if(pageResponse.Length != 30) return false;

        var str = new byte[20];
        Array.Copy(pageResponse, 10, str, 0, 20);

        return AppleOEMString.SequenceEqual(str);
    }

#endregion Apple Mode Page 0x30: Apple OEM String
}