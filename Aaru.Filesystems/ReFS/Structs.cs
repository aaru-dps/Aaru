// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Resilient File System plugin
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

/// <inheritdoc />
/// <summary>Implements detection of Microsoft's Resilient filesystem (ReFS)</summary>
public sealed partial class ReFS
{
#region Nested type: VolumeHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] mustBeZero;
        public readonly uint   identifier;
        public readonly ushort length;
        public readonly ushort checksum;
        public readonly ulong  sectors;
        public readonly uint   bytesPerSector;
        public readonly uint   sectorsPerCluster;
        public readonly uint   unknown1;
        public readonly uint   unknown2;
        public readonly ulong  unknown3;
        public readonly ulong  unknown4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15872)]
        public readonly byte[] unknown5;
    }

#endregion
}