// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SUSP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     SUSP extensions constants and enumerations.
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
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

// ReSharper disable IdentifierTypo

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed partial class ISO9660
    {
        const ushort SUSP_CONTINUATION = 0x4345; // "CE"
        const ushort SUSP_PADDING      = 0x5044; // "PD"
        const ushort SUSP_INDICATOR    = 0x5350; // "SP"
        const ushort SUSP_TERMINATOR   = 0x5354; // "ST"
        const ushort SUSP_REFERENCE    = 0x4552; // "ER"
        const ushort SUSP_SELECTOR     = 0x4553; // "ES"
        const ushort SUSP_MAGIC        = 0xBEEF;
    }
}