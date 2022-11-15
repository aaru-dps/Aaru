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
//     Contains constants for Apple New Disk Image Format.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Ndif
{
    /// <summary>Resource OSType for NDIF is "bcem"</summary>
    const uint NDIF_RESOURCE = 0x6263656D;
    /// <summary>Resource ID is always 128? Never found another</summary>
    const short NDIF_RESOURCEID = 128;

    const byte CHUNK_TYPE_NOCOPY  = 0;
    const byte CHUNK_TYPE_COPY    = 2;
    const byte CHUNK_TYPE_KENCODE = 0x80;
    const byte CHUNK_TYPE_RLE     = 0x81;
    const byte CHUNK_TYPE_LZH     = 0x82;
    const byte CHUNK_TYPE_ADC     = 0x83;
    /// <summary>Created by ShrinkWrap 3.5, dunno which version of the StuffIt algorithm it is using</summary>
    const byte CHUNK_TYPE_STUFFIT = 0xF0;
    const byte CHUNK_TYPE_END             = 0xFF;
    const byte CHUNK_TYPE_COMPRESSED_MASK = 0x80;

    const short DRIVER_OSX         = -1;
    const short DRIVER_HFS         = 0;
    const short DRIVER_PRODOS      = 256;
    const short DRIVER_DOS         = 18771;
    const uint  MAX_CACHE_SIZE     = 16777216;
    const uint  SECTOR_SIZE        = 512;
    const uint  MAX_CACHED_SECTORS = MAX_CACHE_SIZE / SECTOR_SIZE;
}