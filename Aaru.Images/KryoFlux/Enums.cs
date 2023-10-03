// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for KryoFlux STREAM images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class KryoFlux
{
#region Nested type: BlockIds

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum BlockIds : byte
    {
        Flux2   = 0x00,
        Flux2_1 = 0x01,
        Flux2_2 = 0x02,
        Flux2_3 = 0x03,
        Flux2_4 = 0x04,
        Flux2_5 = 0x05,
        Flux2_6 = 0x06,
        Flux2_7 = 0x07,
        Nop1    = 0x08,
        Nop2    = 0x09,
        Nop3    = 0x0A,
        Ovl16   = 0x0B,
        Flux3   = 0x0C,
        Oob     = 0x0D
    }

#endregion

#region Nested type: OobTypes

    enum OobTypes : byte
    {
        Invalid    = 0x00,
        StreamInfo = 0x01,
        Index      = 0x02,
        StreamEnd  = 0x03,
        KFInfo     = 0x04,
        EOF        = 0x0D
    }

#endregion
}