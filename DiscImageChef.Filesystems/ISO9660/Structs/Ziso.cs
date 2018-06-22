// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ziso.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     zisofs extensions structures.
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
    public partial class ISO9660
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ZisofsHeader
        {
            public ulong magic;
            public uint  uncomp_len;
            public uint  uncomp_len_be;
            public byte  header_size;    // Shifted >> 2
            public byte  block_size_log; // log2(block_size)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ZisofsEntry
        {
            public ushort signature;
            public byte   length;
            public byte   version;
            public ushort alogirhtm;
            public byte   header_size;    // Shifted >> 2
            public byte   block_size_log; // log2(block_size)
            public uint   uncomp_len;
            public uint   uncomp_len_be;
        }
    }
}