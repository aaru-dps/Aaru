// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-FX plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-FX track header and shows information.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// Not a filesystem, more like an executable header
/// <inheritdoc />
/// <summary>Implements detection of NEC PC-FX headers</summary>
public sealed partial class PCFX
{
#region Nested type: Header

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)]
        public readonly byte[] copyright;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x710)]
        public readonly byte[] unknown;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] title;
        public readonly uint loadOffset;
        public readonly uint loadCount;
        public readonly uint loadAddress;
        public readonly uint entryPoint;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] makerId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public readonly byte[] makerName;
        public readonly uint   volumeNumber;
        public readonly byte   majorVersion;
        public readonly byte   minorVersion;
        public readonly ushort country;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] date;
    }

#endregion
}