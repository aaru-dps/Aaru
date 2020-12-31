// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ElTorito.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     El Torito extensions structures.
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
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ElToritoBootRecord
        {
            public readonly byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public readonly byte[] id;
            public readonly byte version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] boot_id;
            public readonly uint catalog_sector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1974)]
            public readonly byte[] boot_use;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ElToritoValidationEntry
        {
            public readonly ElToritoIndicator header_id;
            public readonly ElToritoPlatform  platform_id;
            public readonly ushort            reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] developer_id;
            public readonly ushort checksum;
            public readonly ushort signature;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoInitialEntry
        {
            public readonly ElToritoIndicator bootable;
            public          ElToritoEmulation boot_type;
            public readonly ushort            load_seg;
            public readonly byte              system_type;
            public readonly byte              reserved1;
            public readonly ushort            sector_count;
            public readonly uint              load_rba;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public readonly byte[] reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ElToritoSectionHeaderEntry
        {
            public readonly ElToritoIndicator header_id;
            public readonly ElToritoPlatform  platform_id;
            public readonly ushort            entries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public readonly byte[] identifier;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ElToritoSectionEntry
        {
            public readonly ElToritoIndicator bootable;
            public readonly ElToritoEmulation boot_type;
            public readonly ushort            load_seg;
            public readonly byte              system_type;
            public readonly byte              reserved1;
            public readonly ushort            sector_count;
            public readonly uint              load_rba;
            public readonly byte              selection_criteria_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
            public readonly byte[] selection_criterias;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ElToritoSectionEntryExtension
        {
            public readonly ElToritoIndicator extension_indicator;
            public readonly ElToritoFlags     extension_flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public readonly byte[] selection_criterias;
        }
    }
}