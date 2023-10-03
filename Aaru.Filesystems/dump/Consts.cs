// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : dump(8) file system plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies backups created with dump(8) shows information.
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

using System.Diagnostics.CodeAnalysis;
using ufs_daddr_t = int;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements identification of a dump(8) image (virtual filesystem on a file)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class dump
{
    /// <summary>Magic number for old dump</summary>
    const ushort OFS_MAGIC = 60011;
    /// <summary>Magic number for new dump</summary>
    const uint NFS_MAGIC = 60012;
    /// <summary>Magic number for AIX dump</summary>
    const uint XIX_MAGIC = 60013;
    /// <summary>Magic number for UFS2 dump</summary>
    const uint UFS2_MAGIC = 0x19540119;
    /// <summary>Magic number for old dump</summary>
    const uint OFS_CIGAM = 0x6BEA0000;
    /// <summary>Magic number for new dump</summary>
    const uint NFS_CIGAM = 0x6CEA0000;
    /// <summary>Magic number for AIX dump</summary>
    const uint XIX_CIGAM = 0x6DEA0000;
    /// <summary>Magic number for UFS2 dump</summary>
    const uint UFS2_CIGAM = 0x19015419;

    const int TP_BSIZE = 1024;

    /// <summary>Dump tape header</summary>
    const short TS_TAPE = 1;
    /// <summary>Beginning of file record</summary>
    const short TS_INODE = 2;
    /// <summary>Map of inodes on tape</summary>
    const short TS_BITS = 3;
    /// <summary>Continuation of file record</summary>
    const short TS_ADDR = 4;
    /// <summary>Map of inodes deleted since last dump</summary>
    const short TS_END = 5;
    /// <summary>Inode bitmap</summary>
    const short TS_CLRI = 6;
    const short TS_ACL = 7;
    const short TS_PCL = 8;

    const int TP_NINDIR = TP_BSIZE / 2;
    const int LBLSIZE   = 16;
    const int NAMELEN   = 64;

    const int NDADDR = 12;
    const int NIADDR = 3;

    const string FS_TYPE = "dump";
}