// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ElTorito.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     El Torito extensions constants and enumerations.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        const ushort ElToritoMagic = 0xAA55;
        const int ElToritoEntrySize = 32;

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