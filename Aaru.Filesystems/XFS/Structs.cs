// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
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
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of SGI's XFS</summary>
public sealed partial class XFS
{
#region Nested type: Superblock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        public readonly uint   magicnum;
        public readonly uint   blocksize;
        public readonly ulong  dblocks;
        public readonly ulong  rblocks;
        public readonly ulong  rextents;
        public readonly Guid   uuid;
        public readonly ulong  logstat;
        public readonly ulong  rootino;
        public readonly ulong  rbmino;
        public readonly ulong  rsumino;
        public readonly uint   rextsize;
        public readonly uint   agblocks;
        public readonly uint   agcount;
        public readonly uint   rbmblocks;
        public readonly uint   logblocks;
        public readonly ushort version;
        public readonly ushort sectsize;
        public readonly ushort inodesize;
        public readonly ushort inopblock;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] fname;
        public readonly byte   blocklog;
        public readonly byte   sectlog;
        public readonly byte   inodelog;
        public readonly byte   inopblog;
        public readonly byte   agblklog;
        public readonly byte   rextslog;
        public readonly byte   inprogress;
        public readonly byte   imax_pct;
        public readonly ulong  icount;
        public readonly ulong  ifree;
        public readonly ulong  fdblocks;
        public readonly ulong  frextents;
        public readonly ulong  uquotino;
        public readonly ulong  gquotino;
        public readonly ushort qflags;
        public readonly byte   flags;
        public readonly byte   shared_vn;
        public readonly ulong  inoalignmt;
        public readonly ulong  unit;
        public readonly ulong  width;
        public readonly byte   dirblklog;
        public readonly byte   logsectlog;
        public readonly ushort logsectsize;
        public readonly uint   logsunit;
        public readonly uint   features2;
        public readonly uint   bad_features2;
        public readonly uint   features_compat;
        public readonly uint   features_ro_compat;
        public readonly uint   features_incompat;
        public readonly uint   features_log_incompat;

        // This field is little-endian while rest of superblock is big-endian
        public readonly uint  crc;
        public readonly uint  spino_align;
        public readonly ulong pquotino;
        public readonly ulong lsn;
        public readonly Guid  meta_uuid;
    }

#endregion
}