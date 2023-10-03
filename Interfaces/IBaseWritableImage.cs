// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IBaseWritableImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the base interface to be implemented by writable image plugins.
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

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc />
/// <summary>Base interface for all writable images</summary>
public interface IBaseWritableImage : IBaseImage
{
    /// <summary>Contains a description of the last error</summary>
    string ErrorMessage { get; }

    /// <summary>If set to <c>true</c> means the image is opened for writing</summary>
    bool IsWriting { get; }

    /// <summary>Gets a list of known extensions for format auto-choosing</summary>
    IEnumerable<string> KnownExtensions { get; }

    /// <summary>Gets a list of <see cref="MediaTagType" /> that are supported by the media image format</summary>
    IEnumerable<MediaTagType> SupportedMediaTags { get; }

    /// <summary>Gets a list of <see cref="MediaType" /> that are supported by the media image format</summary>
    IEnumerable<MediaType> SupportedMediaTypes { get; }

    /// <summary>Retrieves a list of options supported by the filesystem, with name, type and description</summary>
    IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions { get; }

    /// <summary>Gets a list of <see cref="SectorTagType" /> that are supported by the media image format</summary>
    IEnumerable<SectorTagType> SupportedSectorTags { get; }

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
    bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors, uint sectorSize);

    /// <summary>Closes the image and flushes all data to disk</summary>
    /// <returns>Error number</returns>
    bool Close();

    /// <summary>Sets the Aaru Metadata for the image</summary>
    bool SetMetadata(AaruMetadata.Metadata metadata);

    /// <summary>Sets the list of dump hardware used to create the image from real media</summary>
    bool SetDumpHardware(List<DumpHardware> dumpHardware);

    /// <summary>Sets image metadata</summary>
    /// <param name="imageInfo"><see cref="ImageInfo" /> containing image metadata</param>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    bool SetImageInfo(ImageInfo imageInfo);
}