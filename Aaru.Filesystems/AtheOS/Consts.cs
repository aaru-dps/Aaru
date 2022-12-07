// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Atheos filesystem plugin.
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
/// <summary>Implements detection for the AtheOS filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class AtheOS
{
    // Little endian constants (that is, as read by .NET :p)
    const uint AFS_MAGIC1 = 0x41465331;
    const uint AFS_MAGIC2 = 0xDD121031;
    const uint AFS_MAGIC3 = 0x15B6830E;

    // Common constants
    const uint AFS_SUPERBLOCK_SIZE = 1024;
    const uint AFS_BOOTBLOCK_SIZE  = AFS_SUPERBLOCK_SIZE;

    const string FS_TYPE = "atheos";
}