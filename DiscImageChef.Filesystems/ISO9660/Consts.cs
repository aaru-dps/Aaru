// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
        readonly string IsoMagic = "CD001";
        readonly string HighSierraMagic = "CDROM";
        const ushort ElToritoMagic = 0xAA55;
        const int ElToritoEntrySize = 32;

        [Flags]
        enum FileFlags : byte
        {
            Hidden = 0x01,
            Directory = 0x02,
            Associated = 0x04,
            Record = 0x08,
            Protected = 0x10,
            MultiExtent = 0x80
        }

        [Flags]
        enum Permissions : ushort
        {
            SystemRead = 0x01,
            SystemExecute = 0x04,
            OwnerRead = 0x10,
            OwnerExecute = 0x40,
            GroupRead = 0x100,
            GroupExecute = 0x400,
            OtherRead = 0x1000,
            OtherExecute = 0x4000,
        }

        enum RecordFormat : byte
        {
            Unspecified = 0,
            FixedLength = 1,
            VariableLength = 2,
            VariableLengthAlternate = 3
        }

        enum RecordAttribute : byte
        {
            LFCR = 0,
            ISO1539 = 1,
            ControlContained = 2,
        }

        enum ElToritoIndicator : byte
        {
            Header = 1,
            Extension = 0x44,
            Bootable = 0x88,
            MoreHeaders = 0x90,
            LastHeader = 0x91
        }

        enum ElToritoPlatform : byte
        {
            x86 = 0,
            PowerPC = 1,
            Macintosh = 2,
            EFI = 0xef
        }

        enum ElToritoEmulation : byte
        {
            None = 0,
            Md2hd = 1,
            Mf2hd = 2,
            Mf2ed = 3,
            Hdd = 4
        }

        [Flags]
        enum ElToritoFlags : byte
        {
            Reserved = 0x10,
            Continued = 0x20,
            ATAPI = 0x40,
            SCSI = 0x08
        }
    }
}
