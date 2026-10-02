// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apple.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Attributes;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
#region Nested type: AppleHFSIconSystemUse

    // Big-endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct AppleHFSIconSystemUse
    {
        public ushort     signature;
        public AppleOldId id;
        public uint       type;
        public uint       creator;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] icon;
        /// <summary>Makes the extension an even length (GS/OS Reference, table C-4)</summary>
        public byte padding;
    }

#endregion

#region Nested type: AppleHFSOldSystemUse

    // Big-endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct AppleHFSOldSystemUse
    {
        public ushort     signature;
        public AppleOldId id;
        public uint       type;
        public uint       creator;
        public ushort     finder_flags;
    }

#endregion

#region Nested type: AppleHFSSystemUse

    // Big-endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct AppleHFSSystemUse
    {
        public ushort                  signature;
        public byte                    length;
        public AppleId                 id;
        public uint                    type;
        public uint                    creator;
        public AppleCommon.FinderFlags finder_flags;
    }

#endregion

#region Nested type: AppleHFSTypeCreatorSystemUse

    // Big-endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct AppleHFSTypeCreatorSystemUse
    {
        public ushort     signature;
        public AppleOldId id;
        public uint       type;
        public uint       creator;
        /// <summary>Makes the extension an even length (GS/OS Reference, table C-4)</summary>
        public byte padding;
    }

#endregion

#region Nested type: AppleProDOSOldSystemUse

    // Little-endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct AppleProDOSOldSystemUse
    {
        public readonly ushort     signature;
        public readonly AppleOldId id;
        public readonly byte       type;
        public readonly ushort     aux_type;
    }

#endregion

#region Nested type: AppleProDOSSystemUse

    // Little-endian
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct AppleProDOSSystemUse
    {
        public readonly ushort  signature;
        public readonly byte    length;
        public readonly AppleId id;
        public readonly byte    type;
        public readonly ushort  aux_type;
    }

#endregion
}