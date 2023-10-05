// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IWritableTapeImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by writable block addressable
//     sequential tape image plugins.
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
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc cref="Aaru.CommonTypes.Interfaces.ITapeImage" />
/// <summary>Defines an image that is writable and can store information about a streaming, digital, tape</summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public interface IWritableTapeImage : ITapeImage, IWritableImage
{
    /// <summary>Registers a new file in the image</summary>
    /// <param name="file">Tape file descriptor</param>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise</returns>
    bool AddFile(TapeFile file);

    /// <summary>Registers a new partition</summary>
    /// <param name="partition">Tape partition descriptor</param>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise</returns>
    bool AddPartition(TapePartition partition);

    /// <summary>
    ///     Tells the image plugin to set the internal structures to expect a tape (e.g. unknown block count and size).
    ///     Must be called before <see cref="IWritableImage.Create" />
    /// </summary>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise</returns>
    bool SetTape();
}