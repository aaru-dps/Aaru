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
//     Contains structures for RS-IDE disk images.
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

public sealed partial class RsIde
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] magic;
        public byte       revision;
        public RsIdeFlags flags;
        public ushort     dataOff;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public byte[] reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 106)]
        public byte[] identify;
    }
}