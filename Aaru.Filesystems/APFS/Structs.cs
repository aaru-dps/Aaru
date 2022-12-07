// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Apple File System (APFS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class APFS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ContainerSuperBlock
    {
        public readonly ulong unknown1; // Varies between copies of the superblock
        public readonly ulong unknown2;
        public readonly ulong unknown3; // Varies by 1 between copies of the superblock
        public readonly ulong unknown4;
        public readonly uint  magic;
        public readonly uint  blockSize;
        public readonly ulong containerBlocks;
    }
}