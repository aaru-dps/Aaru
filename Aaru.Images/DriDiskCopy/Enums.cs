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
//     Contains enumerations for Digital Research's DISKCOPY disk images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class DriDiskCopy
{
#region Nested type: DriveCode

    /// <summary>Drive codes change according to CMOS stored valued</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum DriveCode : byte
    {
        /// <summary>5.25" 360k</summary>
        md2dd = 0,
        /// <summary>5.25" 1.2M</summary>
        md2hd = 1,
        /// <summary>3.5" 720k</summary>
        mf2dd = 2,
        /// <summary>3.5" 1.44M</summary>
        mf2hd = 7,
        /// <summary>3.5" 2.88M</summary>
        mf2ed = 9
    }

#endregion
}