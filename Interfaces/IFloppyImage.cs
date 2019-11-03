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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.CommonTypes.Interfaces
{
    /// <summary>
    ///     Abstract class to implement disk image reading plugins that can contain floppy images. This interface is
    ///     needed because floppy formatting characteristics are not necesarily compatible with the whole. LBA-oriented
    ///     interface is defined by <see cref="IMediaImage" />. All data returned by these methods is already decoded from its
    ///     corresponding bitstream.
    /// </summary>
    public interface IFloppyImage : IMediaImage
    {
        /// <summary>
        ///     Floppy info, contains information about physical characteristics of floppy, like size, bitrate, track density,
        ///     etc...
        /// </summary>
        FloppyInfo FloppyInfo { get; }

        /// <summary>Reads a sector's user data.</summary>
        /// <returns>
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates is returned
        ///     randomly. If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" /> or
        ///     <see cref="FloppySectorStatus.Hole" /> random data is returned. If <see cref="status" /> is
        ///     <see cref="FloppySectorStatus.NotFound" /> <c>null</c> is returned. Otherwise, whatever is in the sector is
        ///     returned.
        /// </returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        byte[] ReadSector(ushort track, byte head, ushort sector, out FloppySectorStatus status);

        /// <summary>Reads a sector's tag.</summary>
        /// <returns>
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates is returned
        ///     randomly. If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" /> or
        ///     <see cref="FloppySectorStatus.Hole" /> random data is returned. If <see cref="status" /> is
        ///     <see cref="FloppySectorStatus.NotFound" /> <c>null</c> is returned. Otherwise, whatever tag is in the sector is
        ///     returned.
        /// </returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        byte[] ReadSectorTag(ushort track, byte head, ushort sector, out FloppySectorStatus status, SectorTagType tag);

        /// <summary>Reads a whole track. It includes all gaps, address marks, sectors data, etc.</summary>
        /// <returns>The track data.</returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        byte[] ReadTrack(ushort track, byte head);

        /// <summary>Reads a sector's data including all tags, address mark, and so, in a format dependent of represented media.</summary>
        /// <returns>
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates is returned
        ///     randomly. If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" /> or
        ///     <see cref="FloppySectorStatus.Hole" /> random data is returned. If <see cref="status" /> is
        ///     <see cref="FloppySectorStatus.NotFound" /> <c>null</c> is returned. Otherwise, whatever is in the sector is
        ///     returned.
        /// </returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        byte[] ReadSectorLong(ushort track, byte head, ushort sector, out FloppySectorStatus status);

        /// <summary>Verifies a track.</summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        bool? VerifyTrack(ushort track, byte head);

        /// <summary>Verifies a sector, relative to track.</summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        bool? VerifySector(ushort track, byte head, ushort sector);
    }
}