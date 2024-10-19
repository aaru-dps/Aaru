// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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

// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Professional File System</summary>
public sealed partial class PFS
{
    /// <summary>Identifier for AFS (PFS v1)</summary>
    const uint AFS_DISK = 0x41465301;
    /// <summary>Identifier for PFS v2</summary>
    const uint PFS2_DISK = 0x50465302;
    /// <summary>Identifier for PFS v3</summary>
    const uint PFS_DISK = 0x50465301;
    /// <summary>Identifier for multi-user AFS</summary>
    const uint MUAF_DISK = 0x6D754146;
    /// <summary>Identifier for multi-user PFS</summary>
    const uint MUPFS_DISK = 0x6D755046;

    const string FS_TYPE = "pfs";
}