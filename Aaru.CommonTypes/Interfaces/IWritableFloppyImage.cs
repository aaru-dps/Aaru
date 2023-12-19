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

using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc cref="IWritableImage" />
/// <summary>
///     Abstract class to implement disk image reading plugins that can contain floppy images. This interface is
///     needed because floppy formatting characteristics are not necessarily compatible with the whole LBA-oriented
///     interface defined by <see cref="T:Aaru.CommonTypes.Interfaces.IMediaImage" />. All data expected by these methods
///     is already decoded from its corresponding bitstream.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IWritableFloppyImage : IFloppyImage, IWritableImage
{
    /// <summary>
    ///     Indicates the image plugin the floppy physical characteristics and must be called before following methods are
    ///     called. Once this is called, LBA-based methods should not be used.
    /// </summary>
    /// <param name="info">
    ///     Floppy info, contains information about physical characteristics of floppy, like size, bitrate,
    ///     track density, etc...
    /// </param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool SetFloppyCharacteristics(FloppyInfo info);

    /// <summary>Writes a sector's user data.</summary>
    /// <param name="data">
    ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates. If
    ///     <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" />, <see cref="FloppySectorStatus.Hole" />,
    ///     <see cref="FloppySectorStatus.NotFound" /> it will be ignored. Otherwise, whatever data should be in the sector.
    /// </param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
    /// <param name="head">Physical head (0-based).</param>
    /// <param name="sector">Logical sector ID.</param>
    /// <param name="status">Status of sector.</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSector(byte[] data, ushort track, byte head, ushort sector, FloppySectorStatus status);

    /// <summary>Writes a whole track, including all gaps, address marks, sectors data, etc.</summary>
    /// <param name="data">The track data.</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
    /// <param name="head">Physical head (0-based).</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteTrack(byte[] data, ushort track, byte head);

    /// <summary>Writes a sector's data including all tags, address mark, and so, in a format dependent of represented media.</summary>
    /// <param name="data">
    ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates. If
    ///     <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" />, <see cref="FloppySectorStatus.Hole" />,
    ///     <see cref="FloppySectorStatus.NotFound" /> it will be ignored. Otherwise, whatever data should be in the sector.
    /// </param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
    /// <param name="head">Physical head (0-based).</param>
    /// <param name="sector">Logical sector ID.</param>
    /// <param name="status">Status of request.</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSectorLong(byte[] data, ushort track, byte head, ushort sector, out FloppySectorStatus status);
}