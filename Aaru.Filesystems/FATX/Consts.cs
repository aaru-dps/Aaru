// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     FATX filesystem constants.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable UnusedMember.Local

namespace Aaru.Filesystems;

using System;

public sealed partial class XboxFatPlugin
{
    const uint   FATX_MAGIC          = 0x58544146;
    const uint   FATX_CIGAM          = 0x46415458;
    const byte   UNUSED_DIRENTRY     = 0x00;
    const byte   DELETED_DIRENTRY    = 0xE5;
    const byte   FINISHED_DIRENTRY   = 0xFF;
    const byte   MAX_FILENAME        = 42;
    const int    MAX_XFAT16_CLUSTERS = 65525;
    const int    FAT_START           = 4096;
    const uint   FATX32_ID           = 0xFFFFFFF8;
    const ushort FATX16_ID           = 0xFFF8;
    const uint   FAT32_MASK          = 0x0FFFFFFF;
    const uint   FAT32_END_MASK      = 0xFFFFFF8;
    const uint   FAT32_FORMATTED     = 0xFFFFFF6;
    const uint   FAT32_BAD           = 0xFFFFFF7;
    const uint   FAT32_RESERVED      = 0xFFFFFF0;
    const ushort FAT16_END_MASK      = 0xFFF8;
    const ushort FAT16_FORMATTED     = 0xFFF6;
    const ushort FAT16_BAD           = 0xFFF7;
    const ushort FAT16_RESERVED      = 0xFFF0;
    const ushort FAT_RESERVED        = 1;

    [Flags]
    enum Attributes : byte
    {
        ReadOnly  = 0x01,
        Hidden    = 0x02,
        System    = 0x04,
        Directory = 0x10,
        Archive   = 0x20
    }
}