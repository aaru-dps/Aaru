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
//     Contains constants for QEMU Enhanced Disk images.
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
public sealed partial class Qed
{
    /// <summary>Magic number: 'Q', 'E', 'D', 0x00</summary>
    const uint QED_MAGIC = 0x00444551;

    /// <summary>Mask of unsupported incompatible features</summary>
    const ulong QED_FEATURE_MASK = 0xFFFFFFFFFFFFFFF8;

    /// <summary>File is differential (has a backing file)</summary>
    const ulong QED_FEATURE_BACKING_FILE = 0x01;
    /// <summary>Image needs a consistency check before writing</summary>
    const ulong QED_FEATURE_NEEDS_CHECK = 0x02;
    /// <summary>Backing file is a raw disk image</summary>
    const ulong QED_FEATURE_RAW_BACKING = 0x04;

    const int  MAX_CACHE_SIZE       = 16777216;
    const uint MAX_CACHED_SECTORS   = MAX_CACHE_SIZE / 512;
    const uint DEFAULT_CLUSTER_SIZE = 65536;
    const uint DEFAULT_TABLE_SIZE   = 4;
}