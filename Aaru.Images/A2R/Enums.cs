// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for A2R flux images.
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
// Copyright © 2011-2023 Rebecca Wallander
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class A2R
{
#region A2RDiskType enum

    public enum A2RDiskType : byte
    {
        _525 = 0x01,
        _35  = 0x2
    }

#endregion

#region A2rDriveType enum

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum A2rDriveType : byte
    {
        SS_525_40trk_quarterStep = 0x1,
        DS_35_80trk_appleCLV     = 0x2,
        DS_525_80trk             = 0x3,
        DS_525_40trk             = 0x4,
        DS_35_80trk              = 0x5,
        DS_8                     = 0x6
    }

#endregion
}