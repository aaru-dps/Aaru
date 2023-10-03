// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : VMware file system plugin.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the VMware filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class VMfs
{
#region Nested type: VolumeInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeInfo
    {
        public readonly uint magic;
        public readonly uint version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] unknown1;
        public readonly byte lun;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] unknown2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public readonly byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 49)]
        public readonly byte[] unknown3;
        public readonly uint size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public readonly byte[] unknown4;
        public readonly Guid  uuid;
        public readonly ulong ctime;
        public readonly ulong mtime;
    }

#endregion
}