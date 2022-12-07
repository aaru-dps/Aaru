// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HP Logical Interchange Format plugin
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

namespace Aaru.Filesystems;

// Information from http://www.hp9845.net/9845/projects/hpdir/#lif_filesystem
/// <inheritdoc />
/// <summary>Implements detection of the LIF filesystem</summary>
public sealed partial class LIF
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SystemBlock
    {
        public readonly ushort magic;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] volumeLabel;
        public readonly uint   directoryStart;
        public readonly ushort lifId;
        public readonly ushort unused;
        public readonly uint   directorySize;
        public readonly ushort lifVersion;
        public readonly ushort unused2;
        public readonly uint   tracks;
        public readonly uint   heads;
        public readonly uint   sectors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] creationDate;
    }
}