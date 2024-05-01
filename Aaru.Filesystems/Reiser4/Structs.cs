// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
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

using System;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Reiser v4 filesystem</summary>
public sealed partial class Reiser4
{
#region Nested type: Superblock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] magic;
        public readonly ushort diskformat;
        public readonly ushort blocksize;
        public readonly Guid   uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] label;
    }

#endregion
}