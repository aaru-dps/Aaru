// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
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

// ReSharper disable NotAccessedField.Local

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from Apple ProDOS 8 Technical Reference
/// <inheritdoc />
/// <summary>Implements detection of Apple ProDOS filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class ProDOSPlugin
{
    const byte EMPTY_STORAGE_TYPE = 0x00;
    /// <summary>A file that occupies one block or less</summary>
    const byte SEEDLING_FILE_TYPE = 0x01;
    /// <summary>A file that occupies between 2 and 256 blocks</summary>
    const byte SAPLING_FILE_TYPE = 0x02;
    /// <summary>A file that occupies between 257 and 32768 blocks</summary>
    const byte TREE_FILE_TYPE = 0x03;
    const byte PASCAL_AREA_TYPE         = 0x04;
    const byte SUBDIRECTORY_TYPE        = 0x0D;
    const byte SUBDIRECTORY_HEADER_TYPE = 0x0E;
    const byte ROOT_DIRECTORY_TYPE      = 0x0F;

    const byte VERSION1 = 0x00;

    const uint YEAR_MASK   = 0xFE000000;
    const uint MONTH_MASK  = 0x1E00000;
    const uint DAY_MASK    = 0x1F0000;
    const uint HOUR_MASK   = 0x1F00;
    const uint MINUTE_MASK = 0x3F;

    const byte DESTROY_ATTRIBUTE       = 0x80;
    const byte RENAME_ATTRIBUTE        = 0x40;
    const byte BACKUP_ATTRIBUTE        = 0x20;
    const byte WRITE_ATTRIBUTE         = 0x02;
    const byte READ_ATTRIBUTE          = 0x01;
    const byte RESERVED_ATTRIBUTE_MASK = 0x1C;

    const byte STORAGE_TYPE_MASK = 0xF0;
    const byte NAME_LENGTH_MASK  = 0x0F;
    const byte ENTRY_LENGTH      = 0x27;
    const byte ENTRIES_PER_BLOCK = 0x0D;

    const string FS_TYPE = "prodos";
}