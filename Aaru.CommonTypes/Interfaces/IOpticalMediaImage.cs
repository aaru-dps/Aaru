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
// Copyright © 2011-2025 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc cref="IMediaImage" />
/// <summary>Abstract class to implement disk image reading plugins.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public interface IOpticalMediaImage : IMediaImage, IPartitionableMediaImage, IVerifiableSectorsImage
{
    /// <summary>Gets the disc track extents (start, length).</summary>
    /// <value>The track extents.</value>
    List<Track> Tracks { get; }

    /// <summary>Gets the sessions (optical discs only).</summary>
    /// <value>The sessions.</value>
    List<Session> Sessions { get; }

    /// <summary>Reads a disc's DPM data.</summary>
    /// <returns>The disc's DPM data.</returns>
    /// <param name="dpmStartSector">Starting sector lba.</param>
    /// <param name="dpmResolution">DPM Resolution.</param>
    /// <param name="numberOfDpmEntries">The number of DPM entries.</param>
    /// <param name="dpm">The array of DPM data.</param>
    ErrorNumber ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm);

    /// <summary>Reads a sector's DPM data.</summary>
    /// <returns>The sector's DPM data, null if it is not stored for that sector.</returns>
    /// <param name="dpm">The sector's dpm.</param>
    ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm);

    /// <summary>Reads a sector's user data, relative to track.</summary>
    /// <returns>The sector's user data.</returns>
    /// <param name="sectorAddress">Sector address (relative LBA).</param>
    /// <param name="track">Track.</param>
    /// <param name="buffer">The sector's user data.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus);

    /// <summary>Reads a sector's tag, relative to track.</summary>
    /// <returns>The sector's tag.</returns>
    /// <param name="sectorAddress">Sector address (relative LBA).</param>
    /// <param name="track">Track.</param>
    /// <param name="tag">Tag type.</param>
    /// <param name="buffer">The sector's tag.</param>
    ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer);

    /// <summary>Reads user data from several sectors, relative to track.</summary>
    /// <returns>The sectors user data.</returns>
    /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="track">Track.</param>
    /// <param name="buffer">The sectors user data.</param>
    /// <param name="sectorStatus">The status of each sector.</param>
    ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                            out SectorStatus[] sectorStatus);

    /// <summary>Reads tag from several sectors, relative to track.</summary>
    /// <returns>The sectors tag.</returns>
    /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="track">Track.</param>
    /// <param name="tag">Tag type.</param>
    /// <param name="buffer">The sectors tag.</param>
    ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag, out byte[] buffer);

    /// <summary>Reads a complete sector (user data + all tags), relative to track.</summary>
    /// <returns>The complete sector. Format depends on disk type.</returns>
    /// <param name="sectorAddress">Sector address (relative LBA).</param>
    /// <param name="track">Track.</param>
    /// <param name="buffer">The complete sector.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus);

    /// <summary>Reads several complete sector (user data + all tags), relative to track.</summary>
    /// <returns>The complete sectors. Format depends on disk type.</returns>
    /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="track">Track.</param>
    /// <param name="buffer">The complete sectors.</param>
    /// <param name="sectorStatus">The status of each sector.</param>
    ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                out SectorStatus[] sectorStatus);

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
    /// <param name="failingLbas">List of incorrect sectors.</param>
    /// <param name="unknownLbas">List of uncheckable sectors.</param>
    bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                        out List<ulong> unknownLbas);
}