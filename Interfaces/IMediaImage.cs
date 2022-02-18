// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines interface to be implemented by block addressable disk image
//     plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Abstract class to implement disk image reading plugins.</summary>
public interface IMediaImage : IBaseImage
{
    /// <summary>Reads a disk tag.</summary>
    /// <returns></returns>
    /// <param name="tag">Tag type to read.</param>
    /// <param name="buffer">Disk tag</param>
    ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer);

    /// <summary>Reads a sector's user data.</summary>
    /// <returns>The sector's user data.</returns>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="buffer"></param>
    ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer);

    /// <summary>Reads a complete sector (user data + all tags).</summary>
    /// <returns>The complete sector. Format depends on disk type.</returns>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="buffer"></param>
    ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer);

    /// <summary>Reads user data from several sectors.</summary>
    /// <returns>The sectors user data.</returns>
    /// <param name="sectorAddress">Starting sector address (LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="buffer"></param>
    ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer);

    /// <summary>Reads several complete sector (user data + all tags).</summary>
    /// <returns>The complete sectors. Format depends on disk type.</returns>
    /// <param name="sectorAddress">Starting sector address (LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="buffer"></param>
    ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer);

    /// <summary>Reads tag from several sectors.</summary>
    /// <returns>The sectors tag.</returns>
    /// <param name="sectorAddress">Starting sector address (LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="tag">Tag type.</param>
    /// <param name="buffer"></param>
    ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer);

    /// <summary>Reads a sector's tag.</summary>
    /// <returns>The sector's tag.</returns>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="tag">Tag type.</param>
    /// <param name="buffer"></param>
    ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer);
}