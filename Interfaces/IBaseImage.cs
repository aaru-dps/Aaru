// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IBaseImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by image plugins.
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

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Base interface for all images</summary>
public interface IBaseImage
{
    /// <summary>Plugin author</summary>
    string Author { get; }
    /// <summary>Gets the CICM XML metadata for the image</summary>
    CICMMetadataType CicmMetadata { get; }
    /// <summary>List of dump hardware used to create the image from real media</summary>
    List<DumpHardwareType> DumpHardware { get; }
    /// <summary>Gets the image format.</summary>
    /// <value>The image format.</value>
    string Format { get; }
    /// <summary>Plugin UUID.</summary>
    Guid Id { get; }
    /// <summary>Image information</summary>
    ImageInfo Info { get; }
    /// <summary>Plugin name.</summary>
    string Name { get; }

    /// <summary>Identifies the image.</summary>
    /// <returns><c>true</c>, if image was identified, <c>false</c> otherwise.</returns>
    /// <param name="imageFilter">Image filter.</param>
    bool Identify(IFilter imageFilter);

    /// <summary>Opens the image.</summary>
    /// <returns><c>true</c>, if image was opened, <c>false</c> otherwise.</returns>
    /// <param name="imageFilter">Image filter.</param>
    ErrorNumber Open(IFilter imageFilter);
}