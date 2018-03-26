// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IFloppyImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines interface to be implemented by floppy image plugins.
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

namespace DiscImageChef.DiscImages
{
    /// <summary>
    ///     Abstract class to implement disk image reading plugins that can contain floppy images.
    ///     This interface is needed because floppy formatting characteristics are not necesarily compatible with the whole.
    ///     LBA-oriented interface is defined by <see cref="IMediaImage" />.
    ///     All data returned by these methods is already decoded from its corresponding bitstream.
    /// </summary>
    public interface IFloppyImage : IMediaImage
    {
        /// <summary>
        ///     Floppy info, contains information about physical characteristics of floppy, like size, bitrate, track density,
        ///     etc...
        /// </summary>
        FloppyInfo FloppyInfo { get; }

        /// <summary>
        ///     Reads a sector's user data.
        /// </summary>
        /// <returns>
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates is returned
        ///     randomly.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" /> or
        ///     <see cref="FloppySectorStatus.Hole" /> random data is returned.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.NotFound" /> <c>null</c> is returned.
        ///     Otherwise, whatever is in the sector is returned.
        /// </returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        byte[] ReadSector(ushort track, byte head, ushort sector, out FloppySectorStatus status);

        /// <summary>
        ///     Reads a sector's tag.
        /// </summary>
        /// <returns>
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates is returned
        ///     randomly.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" /> or
        ///     <see cref="FloppySectorStatus.Hole" /> random data is returned.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.NotFound" /> <c>null</c> is returned.
        ///     Otherwise, whatever tag is in the sector is returned.
        /// </returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        byte[] ReadSectorTag(ushort track, byte head, ushort sector, out FloppySectorStatus status, SectorTagType tag);

        /// <summary>
        ///     Reads a whole track. It includes all gaps, address marks, sectors data, etc.
        /// </summary>
        /// <returns>The track data.</returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        byte[] ReadTrack(ushort track, byte head);

        /// <summary>
        ///     Reads a sector's data including all tags, address mark, and so, in a format dependent of represented media.
        /// </summary>
        /// <returns>
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates is returned
        ///     randomly.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" /> or
        ///     <see cref="FloppySectorStatus.Hole" /> random data is returned.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.NotFound" /> <c>null</c> is returned.
        ///     Otherwise, whatever is in the sector is returned.
        /// </returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        byte[] ReadSectorLong(ushort track, byte head, ushort sector, out FloppySectorStatus status);

        /// <summary>
        ///     Verifies a track.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        bool? VerifyTrack(ushort track, byte head);

        /// <summary>
        ///     Verifies a sector, relative to track.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        bool? VerifySector(ushort track, byte head, ushort sector);
    }
}