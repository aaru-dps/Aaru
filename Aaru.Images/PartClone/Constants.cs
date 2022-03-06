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
//     Contains constants for partclone disk images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

public sealed partial class PartClone
{
    const int  CRC_SIZE           = 4;
    const uint MAX_CACHE_SIZE     = 16777216;
    const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
    readonly byte[] _biTmAgIc =
    {
        0x42, 0x69, 0x54, 0x6D, 0x41, 0x67, 0x49, 0x63
    };
    readonly byte[] _partCloneMagic =
    {
        0x70, 0x61, 0x72, 0x74, 0x63, 0x6C, 0x6F, 0x6E, 0x65, 0x2D, 0x69, 0x6D, 0x61, 0x67, 0x65
    };
}