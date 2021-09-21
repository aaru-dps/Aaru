// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IOpticalMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by optical disc image plugins.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces
{
    /// <inheritdoc cref="IMediaImage" />
    /// <summary>Abstract class to implement disk image reading plugins.</summary>
    public interface IOpticalMediaImage : IMediaImage, IPartitionableMediaImage, IVerifiableSectorsImage
    {
        /// <summary>Gets the disc track extents (start, length).</summary>
        /// <value>The track extents.</value>
        List<Track> Tracks { get; }
        /// <summary>Gets the sessions (optical discs only).</summary>
        /// <value>The sessions.</value>
        List<Session> Sessions { get; }

        /// <summary>Reads a sector's user data, relative to track.</summary>
        /// <returns>The sector's user data.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer);

        /// <summary>Reads a sector's tag, relative to track.</summary>
        /// <returns>The sector's tag.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        /// <param name="tag">Tag type.</param>
        byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag);

        /// <summary>Reads user data from several sectors, relative to track.</summary>
        /// <returns>The sectors user data.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer);

        /// <summary>Reads tag from several sectors, relative to track.</summary>
        /// <returns>The sectors tag.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="tag">Tag type.</param>
        byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag);

        /// <summary>Reads a complete sector (user data + all tags), relative to track.</summary>
        /// <returns>The complete sector. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer);

        /// <summary>Reads several complete sector (user data + all tags), relative to track.</summary>
        /// <returns>The complete sectors. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer);

        /// <summary>Gets the disc track extents for a specified session.</summary>
        /// <returns>The track extents for that session.</returns>
        /// <param name="session">Session.</param>
        List<Track> GetSessionTracks(Session session);

        /// <summary>Gets the disc track extents for a specified session.</summary>
        /// <returns>The track extents for that session.</returns>
        /// <param name="session">Session.</param>
        List<Track> GetSessionTracks(ushort session);

        /// <summary>Verifies several sectors, relative to track.</summary>
        /// <returns>True if all are correct, false if any is incorrect, null if any is uncheckable.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="failingLbas">List of incorrect sectors</param>
        /// <param name="unknownLbas">List of uncheckable sectors</param>
        bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                            out List<ulong> unknownLbas);
    }
}