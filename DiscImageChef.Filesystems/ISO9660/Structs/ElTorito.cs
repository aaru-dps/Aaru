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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoBootRecord
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] id;
            public byte version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] boot_id;
            public uint catalog_sector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1974)]
            public byte[] boot_use;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoValidationEntry
        {
            public ElToritoIndicator header_id;
            public ElToritoPlatform platform_id;
            public ushort reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] developer_id;
            public ushort checksum;
            public ushort signature;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoInitialEntry
        {
            public ElToritoIndicator bootable;
            public ElToritoEmulation boot_type;
            public ushort load_seg;
            public byte system_type;
            public byte reserved1;
            public ushort sector_count;
            public uint load_rba;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoSectionHeaderEntry
        {
            public ElToritoIndicator header_id;
            public ElToritoPlatform platform_id;
            public ushort entries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public byte[] identifier;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoSectionEntry
        {
            public ElToritoIndicator bootable;
            public ElToritoEmulation boot_type;
            public ushort load_seg;
            public byte system_type;
            public byte reserved1;
            public ushort sector_count;
            public uint load_rba;
            public byte selection_criteria_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
            public byte[] selection_criterias;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ElToritoSectionEntryExtension
        {
            public ElToritoIndicator extension_indicator;
            public ElToritoFlags extension_flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public byte[] selection_criterias;
        }
    }
}
