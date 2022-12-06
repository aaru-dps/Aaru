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
//     Contains structures for NHD r0 disk images.
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

using System.Runtime.InteropServices;

namespace Aaru.DiscImages
{
    public sealed partial class Nhdr0
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public byte[] szFileID;
            public readonly byte reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
            public byte[] szComment;
            public int   dwHeadSize;
            public int   dwCylinder;
            public short wHead;
            public short wSect;
            public short wSectLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)]
            public byte[] reserved3;
        }
    }
}