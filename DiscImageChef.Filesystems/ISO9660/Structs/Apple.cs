// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        // Little-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleProDOSSystemUse
        {
            public ushort signature;
            public byte length;
            public AppleId id;
            public byte type;
            public ushort aux_type;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSSystemUse
        {
            public ushort signature;
            public byte length;
            public AppleId id;
            public ushort type;
            public ushort creator;
            public ushort finder_flags;
        }

        // Little-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleProDOSOldSystemUse
        {
            public ushort signature;
            public AppleOldId id;
            public byte type;
            public ushort aux_type;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSTypeCreatorSystemUse
        {
            public ushort signature;
            public AppleOldId id;
            public ushort type;
            public ushort creator;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSIconSystemUse
        {
            public ushort signature;
            public AppleOldId id;
            public ushort type;
            public ushort creator;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] icon;
        }

        // Big-endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleHFSOldSystemUse
        {
            public ushort signature;
            public AppleOldId id;
            public ushort type;
            public ushort creator;
            public ushort finder_flags;
        }
    }
}