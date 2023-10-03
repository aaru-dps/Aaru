// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
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
using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
#region Nested type: Flags

    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum Flags : uint
    {
        Unicode      = 0x00000001,
        RemountRO    = 0x00000002,
        Continue     = 0x00000004,
        Panic        = 0x00000008,
        UserQuota    = 0x00000010,
        GroupQuota   = 0x00000020,
        NoJournal    = 0x00000040,
        Discard      = 0x00000080,
        GroupCommit  = 0x00000100,
        LazyCommit   = 0x00000200,
        Temporary    = 0x00000400,
        InlineLog    = 0x00000800,
        InlineMoving = 0x00001000,
        BadSAIT      = 0x00010000,
        Sparse       = 0x00020000,
        DASDEnabled  = 0x00040000,
        DASDPrime    = 0x00080000,
        SwapBytes    = 0x00100000,
        DirIndex     = 0x00200000,
        Linux        = 0x10000000,
        DFS          = 0x20000000,
        OS2          = 0x40000000,
        AIX          = 0x80000000
    }

#endregion

#region Nested type: State

    [Flags]
    enum State : uint
    {
        Clean    = 0,
        Mounted  = 1,
        Dirty    = 2,
        Logredo  = 4,
        Extendfs = 8
    }

#endregion
}