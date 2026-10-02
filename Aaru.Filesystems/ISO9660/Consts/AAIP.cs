// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AAIP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
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
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

// ReSharper disable UnusedMember.Local

using System;

// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    const ushort AAIP_MAGIC     = 0x414C; // "AL"
    const ushort AAIP_MAGIC_OLD = 0x4141; // "AA"

    // ACL entry types of AAIP 2.0
    const int AAIP_ACL_USER_OBJ    = 1;
    const int AAIP_ACL_GROUP_OBJ   = 3;
    const int AAIP_ACL_MASK        = 5;
    const int AAIP_ACL_OTHER       = 6;
    const int AAIP_ACL_SWITCH_MARK = 8;
    const int AAIP_ACL_USER_N      = 10;
    const int AAIP_ACL_GROUP_N     = 12;

    // Tags of the system.posix_acl_* extended attributes of Linux
    const ushort ACL_USER_OBJ  = 0x01;
    const ushort ACL_USER      = 0x02;
    const ushort ACL_GROUP_OBJ = 0x04;
    const ushort ACL_GROUP     = 0x08;
    const ushort ACL_MASK      = 0x10;
    const ushort ACL_OTHER     = 0x20;

    /// <summary>Identifier of the entries that are not for a named user or group</summary>
    const uint ACL_UNDEFINED_ID = 0xFFFFFFFF;
    /// <summary>Version of the system.posix_acl_* extended attributes of Linux</summary>
    const uint LINUX_ACL_VERSION = 2;

#region Nested type: AAIPFlags

    [Flags]
    enum AAIPFlags : byte
    {
        Continue = 1
    }

#endregion
}