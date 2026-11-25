// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ITwinSectorsImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines interface to be implemented by image plugins that support twin
//     sectors.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>
/// Defines an image that supports twin sectors, i.e. two sectors with the same logical (and even physical) address but
/// different contents.
/// This is used by some copy protection schemes, and is not supported by most disc image formats.
/// </summary>
public interface ITwinSectorsImage
{
    /// <summary>
    /// Gets how many physical instances of a sector are stored in the image.
    /// </summary>
    /// <param name="sectorAddress">Sector address.</param>
    /// <param name="negative">True for a negative sector.</param>
    /// <param name="count">Receives 0 when the sector was not dumped, 1 for a normal sector, or the number of twins (&gt;= 2).</param>
    /// <returns>Error status</returns>
    ErrorNumber GetSectorTwinCount(ulong sectorAddress, bool negative, out uint count);

    /// <summary>
    /// Reads the user data of one physical instance of a sector.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher).</param>
    /// <param name="buffer">The sector's user data.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    /// <returns>Error status</returns>
    ErrorNumber ReadSectorTwin(ulong            sectorAddress, bool negative, uint index, out byte[] buffer,
                               out SectorStatus sectorStatus);


    /// <summary>
    /// Reads one physical instance of a sector in long (raw) form.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher).</param>
    /// <param name="buffer">The complete sector.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    /// <returns>Error status</returns>
    ErrorNumber ReadSectorTwinLong(ulong            sectorAddress, bool negative, uint index, out byte[] buffer,
                                   out SectorStatus sectorStatus);

    /// <summary>
    /// Reads a sector tag of one physical instance of a sector.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher).</param>
    /// <param name="buffer">The sectors tag.</param>
    /// <param name="tag">Tag type.</param>
    /// <returns>Error status</returns>
    ErrorNumber ReadSectorTwinTag(ulong sectorAddress, bool negative, uint index, SectorTagType tag, out byte[] buffer);

    /// <summary>
    /// Writes the user data of one physical instance of a sector.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher). If same as current number of twin sectors, appends as a new twin sector.</param>
    /// <param name="buffer">The sector's user data.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    /// <returns>Error status</returns>
    ErrorNumber WriteSectorTwin(ulong        sectorAddress, bool negative, uint index, byte[] buffer,
                                SectorStatus sectorStatus);

    /// <summary>
    /// Writes one physical instance of a sector.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher). If same as current number of twin sectors, appends as a new twin sector.</param>
    /// <param name="buffer">The sector's full data.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    /// <returns>Error status</returns>
    ErrorNumber WriteSectorTwinLong(ulong        sectorAddress, bool negative, uint index, byte[] buffer,
                                    SectorStatus sectorStatus);


    /// <summary>
    /// Writes tag data for a twin sector.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher). Must correspond to an already written twin sector.</param>
    /// <param name="buffer">The sector's tag data.</param>
    /// <param name="tag">The type of tag to write.</param>
    /// <returns>Error status</returns>
    ErrorNumber WriteSectorTwinTag(ulong sectorAddress, bool negative, uint index, byte[] buffer, SectorTagType tag);

    /// <summary>
    /// Clears all twin sectors for a given sector address. Keeps only twin #0 as a normal sector.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <returns>Error status</returns>
    ErrorNumber ClearSectorTwins(ulong sectorAddress, bool negative);

    /// <summary>
    /// Lists every twinned sector of the image, sorted by address.
    /// </summary>
    /// <param name="twinSectors">List of twin sectors in the image.</param>
    /// <returns>Error status</returns>
    ErrorNumber GetTwinSectors(out List<TwinSectorInfo> twinSectors);
}