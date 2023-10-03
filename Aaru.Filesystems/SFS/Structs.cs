// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Smart File System</summary>
public sealed partial class SFS : IFilesystem
{
#region Nested type: RootBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RootBlock
    {
        public readonly uint   blockId;
        public readonly uint   blockChecksum;
        public readonly uint   blockSelfPointer;
        public readonly ushort version;
        public readonly ushort sequence;
        public readonly uint   datecreated;
        public readonly Flags  bits;
        public readonly byte   padding1;
        public readonly ushort padding2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] reserved1;
        public readonly ulong firstbyte;
        public readonly ulong lastbyte;
        public readonly uint  totalblocks;
        public readonly uint  blocksize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly uint[] reserved3;
        public readonly uint bitmapbase;
        public readonly uint adminspacecontainer;
        public readonly uint rootobjectcontainer;
        public readonly uint extentbnoderoot;
        public readonly uint objectnoderoot;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly uint[] reserved4;
    }

#endregion
}