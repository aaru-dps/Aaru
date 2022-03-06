// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for SuperCardPro flux images.
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

public sealed partial class SuperCardPro
{
    /// <summary>SuperCardPro footer signature: "FPCS"</summary>
    const uint FOOTER_SIGNATURE = 0x53435046;
    /// <summary>SuperCardPro header signature: "SCP"</summary>
    readonly byte[] _scpSignature =
    {
        0x53, 0x43, 0x50
    };
    /// <summary>SuperCardPro track header signature: "TRK"</summary>
    readonly byte[] _trkSignature =
    {
        0x54, 0x52, 0x4B
    };
}