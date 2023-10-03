// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : F2FS filesystem plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Flash-Friendly File System (F2FS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class F2FS
{
    const string FS_TYPE = "f2fs";
    // ReSharper disable InconsistentNaming
    const uint F2FS_MAGIC        = 0xF2F52010;
    const uint F2FS_SUPER_OFFSET = 1024;
    const uint F2FS_MIN_SECTOR   = 512;
    const uint F2FS_MAX_SECTOR   = 4096;
    const uint F2FS_BLOCK_SIZE   = 4096;

    // ReSharper restore InconsistentNaming
}