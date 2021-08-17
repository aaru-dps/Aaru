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
//     Defines the interface to be implemented by writable image plugins.
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

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.CommonTypes.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Abstract class to implement disk image writing plugins. TODO: This interface is subject to change until
    ///     notice.
    /// </summary>
    public interface IWritableImage : IMediaImage
    {
        /// <summary>Gets a list of <see cref="MediaTagType" /> that are supported by the media image format</summary>
        IEnumerable<MediaTagType> SupportedMediaTags { get; }
        /// <summary>Gets a list of <see cref="SectorTagType" /> that are supported by the media image format</summary>
        IEnumerable<SectorTagType> SupportedSectorTags { get; }
        /// <summary>Gets a list of <see cref="MediaType" /> that are supported by the media image format</summary>
        IEnumerable<MediaType> SupportedMediaTypes { get; }
        /// <summary>Retrieves a list of options supported by the filesystem, with name, type and description</summary>
        IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions { get; }
        /// <summary>Gets a list of known extensions for format auto-choosing</summary>
        IEnumerable<string> KnownExtensions { get; }

        /// <summary>If set to <c>true</c> means the image is opened for writing</summary>
        bool IsWriting { get; }
        /// <summary>Contains a description of the last error</summary>
        string ErrorMessage { get; }

        /// <summary>
        ///     Creates a new image in the specified path, for the specified <see cref="MediaType" />, with the specified
        ///     options to hold a media with the specified number of sectors
        /// </summary>
        /// <param name="path">Path to the new image, with extension</param>
        /// <param name="mediaType"><see cref="MediaType" /> that will be written in the image</param>
        /// <param name="options">Options to be used when creating new image</param>
        /// <param name="sectors">How many sectors the media has.</param>
        /// <param name="sectorSize"></param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                    uint sectorSize);

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

        /// <summary>Writes several sectors to the image</summary>
        /// <param name="data">Sectors data</param>
        /// <param name="sectorAddress">Sector starting address</param>
        /// <param name="length">How many sectors to write</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSectors(byte[] data, ulong sectorAddress, uint length);

        /// <summary>Writes a sector to the image with main channel tags attached</summary>
        /// <param name="data">Sector data with its main channel tags attached</param>
        /// <param name="sectorAddress">Sector address</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSectorLong(byte[] data, ulong sectorAddress);

        /// <summary>Writes several sectors to the image</summary>
        /// <param name="data">Sector data with their main channel tags attached</param>
        /// <param name="sectorAddress">Sector starting address</param>
        /// <param name="length">How many sectors to write</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length);

        /// <summary>Closes and flushes to disk the image</summary>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool Close();

        /// <summary>Sets image metadata</summary>
        /// <param name="metadata"><see cref="ImageInfo" /> containing image metadata</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool SetMetadata(ImageInfo metadata);

        /// <summary>Sets media geometry</summary>
        /// <param name="cylinders">Cylinders</param>
        /// <param name="heads">Heads</param>
        /// <param name="sectorsPerTrack">Sectors per track</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack);

        /// <summary>Writes parallel or subchannel sector tag for one sector</summary>
        /// <param name="data">Tag data to write</param>
        /// <param name="sectorAddress">Sector address</param>
        /// <param name="tag">Tag type</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag);

        /// <summary>Writes parallel or subchannel sector tag for several sector</summary>
        /// <param name="data">Tag data to write</param>
        /// <param name="sectorAddress">Starting sector address</param>
        /// <param name="length">How many sectors to write</param>
        /// <param name="tag">Tag type</param>
        /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
        bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag);

        /// <summary>Sets the list of dump hardware used to create the image from real media</summary>
        bool SetDumpHardware(List<DumpHardwareType> dumpHardware);

        /// <summary>Sets the CICM XML metadata for the image</summary>
        bool SetCicmMetadata(CICMMetadataType metadata);
    }
}