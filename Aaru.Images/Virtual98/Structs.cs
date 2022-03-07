// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for Virtual98 disk images.
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

namespace Aaru.DiscImages;

using System.Runtime.InteropServices;

public sealed partial class Virtual98
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Virtual98Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] comment;
        public readonly uint   padding;
        public readonly ushort mbsize;
        public          ushort sectorsize;
        public          byte   sectors;
        public          byte   surfaces;
        public          ushort cylinders;
        public          uint   totals;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x44)]
        public byte[] padding2;
    }
}