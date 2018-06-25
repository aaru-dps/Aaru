// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines interface to be implemented by disc image plugins.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.CommonTypes.Interfaces
{
    /// <summary>
    ///     Abstract class to implement disk image reading plugins.
    /// </summary>
    public interface IMediaImage
    {
        /// <summary>Image information</summary>
        ImageInfo Info { get; }
        /// <summary>Plugin name.</summary>
        string Name { get; }
        /// <summary>Plugin UUID.</summary>
        Guid Id { get; }
        /// <summary>
        ///     Gets the image format.
        /// </summary>
        /// <value>The image format.</value>
        string Format { get; }
        /// <summary>
        ///     Gets an array partitions. Typically only useful for optical disc
        ///     images where each track and index means a different partition, as
        ///     reads can be relative to them.
        /// </summary>
        /// <value>The partitions.</value>
        List<Partition> Partitions { get; }
        /// <summary>
        ///     Gets the disc track extents (start, length).
        /// </summary>
        /// <value>The track extents.</value>
        List<Track> Tracks { get; }
        /// <summary>
        ///     Gets the sessions (optical discs only).
        /// </summary>
        /// <value>The sessions.</value>
        List<Session> Sessions { get; }
        /// <summary>List of dump hardware used to create the image from real media</summary>
        List<DumpHardwareType> DumpHardware { get; }
        /// <summary>Gets the CICM XML metadata for the image</summary>
        CICMMetadataType CicmMetadata { get; }

        /// <summary>
        ///     Identifies the image.
        /// </summary>
        /// <returns><c>true</c>, if image was identified, <c>false</c> otherwise.</returns>
        /// <param name="imageFilter">Image filter.</param>
        bool Identify(IFilter imageFilter);

        /// <summary>
        ///     Opens the image.
        /// </summary>
        /// <returns><c>true</c>, if image was opened, <c>false</c> otherwise.</returns>
        /// <param name="imageFilter">Image filter.</param>
        bool Open(IFilter imageFilter);

        /// <summary>
        ///     Reads a disk tag.
        /// </summary>
        /// <returns>Disk tag</returns>
        /// <param name="tag">Tag type to read.</param>
        byte[] ReadDiskTag(MediaTagType tag);

        /// <summary>
        ///     Reads a sector's user data.
        /// </summary>
        /// <returns>The sector's user data.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        byte[] ReadSector(ulong sectorAddress);

        /// <summary>
        ///     Reads a sector's tag.
        /// </summary>
        /// <returns>The sector's tag.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        /// <param name="tag">Tag type.</param>
        byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag);

        /// <summary>
        ///     Reads a sector's user data, relative to track.
        /// </summary>
        /// <returns>The sector's user data.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        byte[] ReadSector(ulong sectorAddress, uint track);

        /// <summary>
        ///     Reads a sector's tag, relative to track.
        /// </summary>
        /// <returns>The sector's tag.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        /// <param name="tag">Tag type.</param>
        byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag);

        /// <summary>
        ///     Reads user data from several sectors.
        /// </summary>
        /// <returns>The sectors user data.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        byte[] ReadSectors(ulong sectorAddress, uint length);

        /// <summary>
        ///     Reads tag from several sectors.
        /// </summary>
        /// <returns>The sectors tag.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="tag">Tag type.</param>
        byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag);

        /// <summary>
        ///     Reads user data from several sectors, relative to track.
        /// </summary>
        /// <returns>The sectors user data.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        byte[] ReadSectors(ulong sectorAddress, uint length, uint track);

        /// <summary>
        ///     Reads tag from several sectors, relative to track.
        /// </summary>
        /// <returns>The sectors tag.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="tag">Tag type.</param>
        byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag);

        /// <summary>
        ///     Reads a complete sector (user data + all tags).
        /// </summary>
        /// <returns>The complete sector. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        byte[] ReadSectorLong(ulong sectorAddress);

        /// <summary>
        ///     Reads a complete sector (user data + all tags), relative to track.
        /// </summary>
        /// <returns>The complete sector. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        byte[] ReadSectorLong(ulong sectorAddress, uint track);

        /// <summary>
        ///     Reads several complete sector (user data + all tags).
        /// </summary>
        /// <returns>The complete sectors. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        byte[] ReadSectorsLong(ulong sectorAddress, uint length);

        /// <summary>
        ///     Reads several complete sector (user data + all tags), relative to track.
        /// </summary>
        /// <returns>The complete sectors. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track);

        /// <summary>
        ///     Gets the disc track extents for a specified session.
        /// </summary>
        /// <returns>The track exents for that session.</returns>
        /// <param name="session">Session.</param>
        List<Track> GetSessionTracks(Session session);

        /// <summary>
        ///     Gets the disc track extents for a specified session.
        /// </summary>
        /// <returns>The track exents for that session.</returns>
        /// <param name="session">Session.</param>
        List<Track> GetSessionTracks(ushort session);

        /// <summary>
        ///     Verifies a sector.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        bool? VerifySector(ulong sectorAddress);

        /// <summary>
        ///     Verifies a sector, relative to track.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        bool? VerifySector(ulong sectorAddress, uint track);

        /// <summary>
        ///     Verifies several sectors.
        /// </summary>
        /// <returns>True if all are correct, false if any is incorrect, null if any is uncheckable.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="failingLbas">List of incorrect sectors</param>
        /// <param name="unknownLbas">List of uncheckable sectors</param>
        bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas, out List<ulong> unknownLbas);

        /// <summary>
        ///     Verifies several sectors, relative to track.
        /// </summary>
        /// <returns>True if all are correct, false if any is incorrect, null if any is uncheckable.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="failingLbas">List of incorrect sectors</param>
        /// <param name="unknownLbas">List of uncheckable sectors</param>
        bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                            out List<ulong> unknownLbas);

        /// <summary>
        ///     Verifies media image internal checksum.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if there is no internal checksum available</returns>
        bool? VerifyMediaImage();
    }
}