// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
// ****************************************************************************/

using System;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
#region Nested type: NodeFlags

    /// <summary>Node type and flags</summary>
    [Flags]
    enum NodeFlags : ushort
    {
        /// <summary>System object, not listed in directories</summary>
        Hidden = 0x0001,
        /// <summary>Soft link</summary>
        SoftLink = 0x0040,
        /// <summary>Directory</summary>
        Directory = 0x0080
    }

#endregion

#region Nested type: VolumeFlags

    /// <summary>Superblock flags</summary>
    [Flags]
    enum VolumeFlags : byte
    {
        /// <summary>Deleted entries are moved to the <c>.recycled</c> directory</summary>
        RecycleBin = 0x40,
        /// <summary>Names are compared case-sensitively</summary>
        CaseSensitive = 0x80
    }

#endregion
}