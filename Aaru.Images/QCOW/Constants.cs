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
//     Contains constants for QEMU Copy-On-Write disk images.
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

namespace Aaru.DiscImages;

public sealed partial class Qcow
{
    /// <summary>Magic number: 'Q', 'F', 'I', 0xFB</summary>
    const uint QCOW_MAGIC = 0x514649FB;
    const uint  QCOW_VERSION         = 1;
    const uint  QCOW_ENCRYPTION_NONE = 0;
    const uint  QCOW_ENCRYPTION_AES  = 1;
    const ulong QCOW_COMPRESSED      = 0x8000000000000000;

    const int MAX_CACHE_SIZE     = 16777216;
    const int MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
}