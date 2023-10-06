// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for Parallels disk images.
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

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Parallels
{
    const    uint   PARALLELS_VERSION    = 2;
    const    uint   PARALLELS_INUSE      = 0x746F6E59;
    const    uint   PARALLELS_CLOSED     = 0x312E3276;
    const    uint   PARALLELS_EMPTY      = 0x00000001;
    const    uint   MAX_CACHE_SIZE       = 16777216;
    const    uint   MAX_CACHED_SECTORS   = MAX_CACHE_SIZE / 512;
    const    uint   DEFAULT_CLUSTER_SIZE = 1048576;
    readonly byte[] _extMagic            = "WithouFreSpacExt"u8.ToArray();
    readonly byte[] _magic               = "WithoutFreeSpace"u8.ToArray();
}