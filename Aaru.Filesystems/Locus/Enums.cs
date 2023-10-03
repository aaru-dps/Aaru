// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
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
//     License aint with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// Commit count

using System;
using System.Diagnostics.CodeAnalysis;
using commitcnt_t = int;

// Disk address
using daddr_t = int;

// Fstore
using fstore_t = int;

// Global File System number
using gfs_t = int;

// Inode number
using ino_t = int;

// Filesystem pack number
using pckno_t = short;

// Timestamp
using time_t = int;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class Locus
{
#region Nested type: Flags

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    [Flags]
    enum Flags : ushort
    {
        SB_RDONLY   = 0x1,   /* no writes on filesystem */
        SB_CLEAN    = 0x2,   /* fs unmounted cleanly (or checks run) */
        SB_DIRTY    = 0x4,   /* fs mounted without CLEAN bit set */
        SB_RMV      = 0x8,   /* fs is a removable file system */
        SB_PRIMPACK = 0x10,  /* This is the primary pack of the filesystem */
        SB_REPLTYPE = 0x20,  /* This is a replicated type filesystem. */
        SB_USER     = 0x40,  /* This is a "user" replicated filesystem. */
        SB_BACKBONE = 0x80,  /* backbone pack ; complete copy of primary pack but not modifiable */
        SB_NFS      = 0x100, /* This is a NFS type filesystem */
        SB_BYHAND   = 0x200, /* Inhibits automatic fscks on a mangled file system */
        SB_NOSUID   = 0x400, /* Set-uid/Set-gid is disabled */
        SB_SYNCW    = 0x800  /* Synchronous Write */
    }

#endregion

#region Nested type: Version

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    [Flags]
    enum Version : byte
    {
        SB_SB4096  = 1, /* smallblock filesys with 4096 byte blocks */
        SB_B1024   = 2, /* 1024 byte block filesystem */
        NUMSCANDEV = 5  /* Used by scangfs(), refed in space.h */
    }

#endregion
}