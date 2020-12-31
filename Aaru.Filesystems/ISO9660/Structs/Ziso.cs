// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems
{
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    public sealed partial class ISO9660
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ZisofsHeader
        {
            public readonly ulong magic;
            public readonly uint  uncomp_len;
            public readonly uint  uncomp_len_be;
            public readonly byte  header_size;    // Shifted >> 2
            public readonly byte  block_size_log; // log2(block_size)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ZisofsEntry
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly ushort alogirhtm;
            public readonly byte   header_size;    // Shifted >> 2
            public readonly byte   block_size_log; // log2(block_size)
            public readonly uint   uncomp_len;
            public readonly uint   uncomp_len_be;
        }
    }
}