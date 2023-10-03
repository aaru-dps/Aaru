// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IFloppyImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by floppy image plugins.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc />
/// <summary>
///     Abstract class to implement disk image reading plugins that can contain floppy images. This interface is
///     needed because floppy formatting characteristics are not necessarily compatible with the whole. LBA-oriented
///     interface is defined by <see cref="T:Aaru.CommonTypes.Interfaces.IMediaImage" />. All data returned by these
///     methods is already decoded from its corresponding bitstream.
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
    /// <param name="buffer">Buffer where the sector data will be stored.</param>
    ErrorNumber ReadSector(ushort track, byte head, ushort sector, out FloppySectorStatus status, out byte[] buffer);

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
    /// <param name="tag">Sector tag</param>
    /// <param name="buffer">Buffer where the sector tag data will be stored.</param>
    ErrorNumber ReadSectorTag(ushort track, byte head, ushort sector, out FloppySectorStatus status, SectorTagType tag,
                              out byte[] buffer);

    /// <summary>Reads a whole track. It includes all gaps, address marks, sectors data, etc.</summary>
    /// <returns>The track data.</returns>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
    /// <param name="head">Physical head (0-based).</param>
    /// <param name="buffer">Buffer where the track data will be stored.</param>
    ErrorNumber ReadTrack(ushort track, byte head, out byte[] buffer);

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
    /// <param name="buffer">Buffer where the sector data will be stored.</param>
    ErrorNumber ReadSectorLong(ushort     track, byte head, ushort sector, out FloppySectorStatus status,
                               out byte[] buffer);

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
    bool? VerifySector(ushort track, byte head, ushort sector);
}