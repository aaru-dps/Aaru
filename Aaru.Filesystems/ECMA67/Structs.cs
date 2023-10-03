// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ECMA-67 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ECMA-67 file system and shows information.
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

/// <inheritdoc />
/// <summary>Implements detection of the filesystem described in ECMA-67</summary>
public sealed partial class ECMA67
{
#region Nested type: VolumeLabel

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeLabel
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] labelIdentifier;
        public readonly byte labelNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] volumeIdentifier;
        public readonly byte volumeAccessibility;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public readonly byte[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] owner;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] reserved2;
        public readonly byte surface;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] reserved3;
        public readonly byte recordLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] reserved4;
        public readonly byte fileLabelAllocation;
        public readonly byte labelStandardVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public readonly byte[] reserved5;
    }

#endregion
}