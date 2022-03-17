// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IWritableImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by writable block addressable image plugins.
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

namespace Aaru.CommonTypes.Interfaces;

using Aaru.CommonTypes.Enums;

/// <inheritdoc cref="Aaru.CommonTypes.Interfaces.IMediaImage" />
/// <summary>
///     Abstract class to implement disk image writing plugins. TODO: This interface is subject to change until
///     notice.
/// </summary>
public interface IWritableImage : IMediaImage, IBaseWritableImage
{
    /// <summary>Sets media geometry</summary>
    /// <param name="cylinders">Cylinders</param>
    /// <param name="heads">Heads</param>
    /// <param name="sectorsPerTrack">Sectors per track</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack);

    /// <summary>Writes a media tag to the image</summary>
    /// <param name="data">Media tag</param>
    /// <param name="tag">
    ///     <see cref="MediaTagType" />
    /// </param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteMediaTag(byte[] data, MediaTagType tag);

    /// <summary>Writes a sector to the image</summary>
    /// <param name="data">Sector data</param>
    /// <param name="sectorAddress">Sector address</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSector(byte[] data, ulong sectorAddress);

    /// <summary>Writes a sector to the image with main channel tags attached</summary>
    /// <param name="data">Sector data with its main channel tags attached</param>
    /// <param name="sectorAddress">Sector address</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSectorLong(byte[] data, ulong sectorAddress);

    /// <summary>Writes several sectors to the image</summary>
    /// <param name="data">Sectors data</param>
    /// <param name="sectorAddress">Sector starting address</param>
    /// <param name="length">How many sectors to write</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSectors(byte[] data, ulong sectorAddress, uint length);

    /// <summary>Writes several sectors to the image</summary>
    /// <param name="data">Sector data with their main channel tags attached</param>
    /// <param name="sectorAddress">Sector starting address</param>
    /// <param name="length">How many sectors to write</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length);

    /// <summary>Writes parallel or subchannel sector tag for several sector</summary>
    /// <param name="data">Tag data to write</param>
    /// <param name="sectorAddress">Starting sector address</param>
    /// <param name="length">How many sectors to write</param>
    /// <param name="tag">Tag type</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag);

    /// <summary>Writes parallel or subchannel sector tag for one sector</summary>
    /// <param name="data">Tag data to write</param>
    /// <param name="sectorAddress">Sector address</param>
    /// <param name="tag">Tag type</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag);
}