// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IFilesystem.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filesystem plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Interface for filesystem plugins.
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Interface to implement filesystem plugins.</summary>
public interface IFilesystem
{
    /// <summary>Defines the encoding used to interpret strings in the filesystem</summary>
    Encoding Encoding { get; }
    /// <summary>Plugin name (translatable).</summary>
    string Name { get; }
    /// <summary>Plugin UUID.</summary>
    Guid Id { get; }
    /// <summary>Plugin author</summary>
    string Author { get; }

    /// <summary>Identifies the filesystem in the specified LBA</summary>
    /// <param name="imagePlugin">Disk image.</param>
    /// <param name="partition">Partition.</param>
    /// <returns><c>true</c>, if the filesystem is recognized, <c>false</c> otherwise.</returns>
    bool Identify(IMediaImage imagePlugin, Partition partition);

    /// <summary>Gets information about the identified filesystem.</summary>
    /// <param name="imagePlugin">Disk image.</param>
    /// <param name="partition">Partition.</param>
    /// <param name="encoding">Which encoding to use for this filesystem.</param>
    /// <param name="information">Filesystem information.</param>
    /// <param name="metadata">Information about the filesystem as expected by Aaru Metadata</param>
    void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                        out FileSystem metadata);
}