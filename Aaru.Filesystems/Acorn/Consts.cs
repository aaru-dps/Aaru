// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Acorn filesystem plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Acorn's Advanced Data Filing System (ADFS)</summary>
public sealed partial class AcornADFS
{
    /// <summary>Location for boot block, in bytes</summary>
    const ulong BOOT_BLOCK_LOCATION = 0xC00;
    /// <summary>Size of boot block, in bytes</summary>
    const uint BOOT_BLOCK_SIZE = 0x200;
    /// <summary>Location of new directory, in bytes</summary>
    const ulong NEW_DIRECTORY_LOCATION = 0x400;
    /// <summary>Location of old directory, in bytes</summary>
    const ulong OLD_DIRECTORY_LOCATION = 0x200;
    /// <summary>Size of old directory</summary>
    const uint OLD_DIRECTORY_SIZE = 1280;
    /// <summary>Size of new directory</summary>
    const uint NEW_DIRECTORY_SIZE = 2048;

    /// <summary>New directory format magic number, "Nick"</summary>
    const uint NEW_DIR_MAGIC = 0x6B63694E;
    /// <summary>Old directory format magic number, "Hugo"</summary>
    const uint OLD_DIR_MAGIC = 0x6F677548;

    const string FS_TYPE = "adfs";
}