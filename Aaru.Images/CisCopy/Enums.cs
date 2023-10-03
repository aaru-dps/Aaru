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
//     Contains enumerations for CisCopy disk images.
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

public sealed partial class CisCopy
{
#region Nested type: Compression

    enum Compression : byte
    {
        None   = 0,
        Normal = 1,
        High   = 2
    }

#endregion

#region Nested type: DiskType

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum DiskType : byte
    {
        MD1DD8 = 1,
        MD1DD  = 2,
        MD2DD8 = 3,
        MD2DD  = 4,
        MF2DD  = 5,
        MD2HD  = 6,
        MF2HD  = 7
    }

#endregion

#region Nested type: TrackType

    enum TrackType : byte
    {
        Copied           = 0x4C,
        Omitted          = 0xFA,
        OmittedAlternate = 0xFE
    }

#endregion
}