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

using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.CommonTypes.Interfaces
{
    /// <summary>
    ///     Abstract class to implement disk image reading plugins that can contain floppy images.
    ///     This interface is needed because floppy formatting characteristics are not necesarily compatible with the whole
    ///     LBA-oriented interface defined by <see cref="IMediaImage" />.
    ///     All data expected by these methods is already decoded from its corresponding bitstream.
    /// </summary>
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

        /// <summary>
        ///     Writes a sector's user data.
        /// </summary>
        /// <param name="data">
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" />, <see cref="FloppySectorStatus.Hole" />,
        ///     <see cref="FloppySectorStatus.NotFound" /> it will be ignored.
        ///     Otherwise, whatever data should be in the sector.
        /// </param>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of sector.</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSector(byte[] data, ushort track, byte head, ushort sector, FloppySectorStatus status);

        /// <summary>
        ///     Writes a whole track, including all gaps, address marks, sectors data, etc.
        /// </summary>
        /// <param name="data">The track data.</param>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteTrack(byte[] data, ushort track, byte head);

        /// <summary>
        ///     Writes a sector's data including all tags, address mark, and so, in a format dependent of represented media.
        /// </summary>
        /// <param name="data">
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Duplicated" /> one of the duplicates.
        ///     If <see cref="status" /> is <see cref="FloppySectorStatus.Demagnetized" />, <see cref="FloppySectorStatus.Hole" />,
        ///     <see cref="FloppySectorStatus.NotFound" /> it will be ignored.
        ///     Otherwise, whatever data should be in the sector.
        /// </param>
        /// <param name="track">Physical track (position of the heads over the floppy media, 0-based).</param>
        /// <param name="head">Physical head (0-based).</param>
        /// <param name="sector">Logical sector ID.</param>
        /// <param name="status">Status of request.</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSectorLong(byte[] data, ushort track, byte head, ushort sector, out FloppySectorStatus status);
    }
}