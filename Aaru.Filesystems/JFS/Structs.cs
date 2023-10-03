// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
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

// ReSharper disable UnusedMember.Local

using System;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
#region Nested type: Extent

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Extent
    {
        /// <summary>Leftmost 24 bits are extent length, rest 8 bits are most significant for <see cref="addr2" /></summary>
        public readonly uint len_addr;
        public readonly uint addr2;
    }

#endregion

#region Nested type: SuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        public readonly uint       s_magic;
        public readonly uint       s_version;
        public readonly ulong      s_size;
        public readonly uint       s_bsize;
        public readonly ushort     s_l2bsize;
        public readonly ushort     s_l2bfactor;
        public readonly uint       s_pbsize;
        public readonly ushort     s_l1pbsize;
        public readonly ushort     pad;
        public readonly uint       s_agsize;
        public readonly Flags      s_flags;
        public readonly State      s_state;
        public readonly uint       s_compress;
        public readonly Extent     s_ait2;
        public readonly Extent     s_aim2;
        public readonly uint       s_logdev;
        public readonly uint       s_logserial;
        public readonly Extent     s_logpxd;
        public readonly Extent     s_fsckpxd;
        public readonly TimeStruct s_time;
        public readonly uint       s_fsckloglen;
        public readonly sbyte      s_fscklog;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] s_fpack;
        public readonly ulong  s_xsize;
        public readonly Extent s_xfsckpxd;
        public readonly Extent s_xlogpxd;
        public readonly Guid   s_uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] s_label;
        public readonly Guid s_loguuid;
    }

#endregion

#region Nested type: TimeStruct

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct TimeStruct
    {
        public readonly uint tv_sec;
        public readonly uint tv_nsec;
    }

#endregion
}