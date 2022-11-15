// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for Dunfield's IMD disk images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Imd
{
    const byte SECTOR_CYLINDER_MAP_MASK = 0x80;
    const byte SECTOR_HEAD_MAP_MASK     = 0x40;
    const byte COMMENT_END              = 0x1A;
    const string REGEX_HEADER =
        @"IMD (?<version>\d.\d+):\s+(?<day>\d+)\/\s*(?<month>\d+)\/(?<year>\d+)\s+(?<hour>\d+):(?<minute>\d+):(?<second>\d+)\r\n";
    const string REGEX_SAMDISK =
        @"IMD SAMdisk(?<version>\d.\d+),\s+(?<day>\d+)\/\s*(?<month>\d+)\/(?<year>\d+)\s+(?<hour>\d+):\s*(?<minute>\d+):(?<second>\d+)";

    // ReSharper disable once InconsistentNaming
    const string REGEX_Z88DK =
        @"IMD z88dk:\s+(?<day>\d+)\/\s*(?<month>\d+)\/(?<year>\d+)\s+(?<hour>\d+):\s*(?<minute>\d+):(?<second>\d+)\r\n";
}