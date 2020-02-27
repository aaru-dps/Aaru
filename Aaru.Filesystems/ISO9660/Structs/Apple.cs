// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apple.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple extensions structures.
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
// Copyright © 2011-2020 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        // Little-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleProDOSSystemUse
        {
            public readonly ushort  signature;
            public readonly byte    length;
            public readonly AppleId id;
            public readonly byte    type;
            public readonly ushort  aux_type;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSSystemUse
        {
            public readonly ushort                  signature;
            public readonly byte                    length;
            public readonly AppleId                 id;
            public readonly uint                    type;
            public readonly uint                    creator;
            public readonly AppleCommon.FinderFlags finder_flags;
        }

        // Little-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleProDOSOldSystemUse
        {
            public readonly ushort     signature;
            public readonly AppleOldId id;
            public readonly byte       type;
            public readonly ushort     aux_type;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSTypeCreatorSystemUse
        {
            public readonly ushort     signature;
            public readonly AppleOldId id;
            public readonly uint       type;
            public readonly uint       creator;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSIconSystemUse
        {
            public readonly ushort     signature;
            public readonly AppleOldId id;
            public readonly uint       type;
            public readonly uint       creator;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public readonly byte[] icon;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSOldSystemUse
        {
            public readonly ushort     signature;
            public readonly AppleOldId id;
            public readonly uint       type;
            public readonly uint       creator;
            public readonly ushort     finder_flags;
        }
    }
}