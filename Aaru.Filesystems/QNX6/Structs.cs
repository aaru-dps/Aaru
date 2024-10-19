// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX6 filesystem plugin.
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
/// <summary>Implements detection of QNX 6 filesystem</summary>
public sealed partial class QNX6
{
#region Nested type: AudiSuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct AudiSuperBlock
    {
        public readonly uint  magic;
        public readonly uint  checksum;
        public readonly ulong serial;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] spare1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] id;
        public readonly uint     blockSize;
        public readonly uint     numInodes;
        public readonly uint     freeInodes;
        public readonly uint     numBlocks;
        public readonly uint     freeBlocks;
        public readonly uint     spare2;
        public readonly RootNode inode;
        public readonly RootNode bitmap;
        public readonly RootNode longfile;
        public readonly RootNode unknown;
    }

#endregion

#region Nested type: RootNode

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RootNode
    {
        public readonly ulong size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly uint[] pointers;
        public readonly byte levels;
        public readonly byte mode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] spare;
    }

#endregion

#region Nested type: SuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        public readonly uint   magic;
        public readonly uint   checksum;
        public readonly ulong  serial;
        public readonly uint   ctime;
        public readonly uint   atime;
        public readonly uint   flags;
        public readonly ushort version1;
        public readonly ushort version2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] volumeid;
        public readonly uint     blockSize;
        public readonly uint     numInodes;
        public readonly uint     freeInodes;
        public readonly uint     numBlocks;
        public readonly uint     freeBlocks;
        public readonly uint     allocationGroup;
        public readonly RootNode inode;
        public readonly RootNode bitmap;
        public readonly RootNode longfile;
        public readonly RootNode unknown;
    }

#endregion
}