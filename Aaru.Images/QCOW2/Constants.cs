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
//     Contains constants for QEMU Copy-On-Write v2 disk images.
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
public sealed partial class Qcow2
{
    /// <summary>Magic number: 'Q', 'F', 'I', 0xFB</summary>
    const uint QCOW_MAGIC = 0x514649FB;
    const uint QCOW_VERSION2        = 2;
    const uint QCOW_VERSION3        = 3;
    const uint QCOW_ENCRYPTION_NONE = 0;
    const uint QCOW_ENCRYPTION_AES  = 1;

    const ulong QCOW_FEATURE_DIRTY   = 0x01;
    const ulong QCOW_FEATURE_CORRUPT = 0x02;
    const ulong QCOW_FEATURE_MASK    = 0xFFFFFFFFFFFFFFFC;

    const ulong QCOW_COMPAT_FEATURE_LAZY_REFCOUNTS = 0x01;
    const ulong QCOW_AUTO_CLEAR_FEATURE_BITMAP     = 0x01;

    const ulong QCOW_FLAGS_MASK = 0x3FFFFFFFFFFFFFFF;
    const ulong QCOW_COPIED     = 0x8000000000000000;
    const ulong QCOW_COMPRESSED = 0x4000000000000000;

    const ulong QCOW_HEADER_EXTENSION_BACKING_FILE  = 0xE2792ACA;
    const ulong QCOW_HEADER_EXTENSION_FEATURE_TABLE = 0x6803F857;
    const ulong QCOW_HEADER_EXTENSION_BITMAPS       = 0x23852875;

    const int MAX_CACHE_SIZE     = 16777216;
    const int MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
}