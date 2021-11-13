// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IByteAddressableImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by byte-addressable image plugins.
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

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Interface defining linear media (chips, game carts, etc) images</summary>
public interface IByteAddressableImage : IBaseImage
{
    /// <summary>Gets or sets the current position</summary>
    long Position { get; set; }

    /// <summary>Creates a linear media image</summary>
    /// <param name="path">Path where to create the media image</param>
    /// <param name="mediaType">Media type</param>
    /// <param name="options">Image options</param>
    /// <param name="maximumSize">Maximum size in bytes</param>
    /// <returns>Error number</returns>
    ErrorNumber Create(string path, MediaType mediaType, Dictionary<string, string> options, long maximumSize);

    /// <summary>Gets the media image header (really needed?)</summary>
    /// <param name="header">Header, interchange format still undecided</param>
    /// <returns>Error message</returns>
    ErrorNumber GetHeader(out byte[] header);

    /// <summary>Gets the linear memory mappings, e.g. interleaving, starting address, etc.</summary>
    /// <param name="mappings">Format still not decided</param>
    /// <returns>Error number</returns>
    ErrorNumber GetMappings(out object mappings);

    /// <summary>Reads a byte from the image</summary>
    /// <param name="b">The byte read</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadByte(out byte b, bool advance = true);

    /// <summary>Reads a byte from the image at the specified position</summary>
    /// <param name="position">Position</param>
    /// <param name="b">The byte read</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadByteAt(long position, out byte b, bool advance = true);

    /// <summary>Reads several bytes from the image</summary>
    /// <param name="buffer">Buffer to store the data in</param>
    /// <param name="offset">Offset in buffer where to place the byte in</param>
    /// <param name="bytesToRead">How many bytes to read from image</param>
    /// <param name="bytesRead">How many bytes were read</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadBytes(byte[] buffer, int offset, int bytesToRead, out int bytesRead, bool advance = true);

    /// <summary>Reads several bytes from the image at the specified position</summary>
    /// <param name="position">Position</param>
    /// <param name="buffer">Buffer to store the data in</param>
    /// <param name="offset">Offset in buffer where to place the byte in</param>
    /// <param name="bytesToRead">How many bytes to read from image</param>
    /// <param name="bytesRead">How many bytes were read</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadBytesAt(long position, byte[] buffer, int offset, int bytesToRead, out int bytesRead,
                            bool advance = true);

    /// <summary>Sets the media image header (really needed?)</summary>
    /// <param name="header">Header, interchange format still undecided</param>
    /// <returns>Error message</returns>
    ErrorNumber SetHeader(byte[] header);

    /// <summary>Sets the linear memory mappings, e.g. interleaving, starting address, etc.</summary>
    /// <param name="mappings">Format still not decided</param>
    /// <returns>Error number</returns>
    ErrorNumber SetMappings(object mappings);

    /// <summary>Writes a byte to the image</summary>
    /// <param name="b">The byte to be written</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber WriteByte(byte b, bool advance = true);

    /// <summary>Writes a byte to the image at the specified position</summary>
    /// <param name="position">Position</param>
    /// <param name="b">The byte read</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber WriteByteAt(long position, byte b, bool advance = true);

    /// <summary>Writes several bytes to the image</summary>
    /// <param name="buffer">Buffer to store the data in</param>
    /// <param name="offset">Offset in buffer where the bytes start in</param>
    /// <param name="bytesToWrite">How many bytes to write to image</param>
    /// <param name="bytesWritten">How many bytes were written</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber WriteBytes(byte[] buffer, int offset, int bytesToWrite, out int bytesWritten, bool advance = true);

    /// <summary>Writes several bytes to the image at the specified position</summary>
    /// <param name="position">Position</param>
    /// <param name="buffer">Buffer to store the data in</param>
    /// <param name="offset">Offset in buffer where the bytes start in</param>
    /// <param name="bytesToWrite">How many bytes to write to image</param>
    /// <param name="bytesWritten">How many bytes were written</param>
    /// <param name="advance">Set to <c>true</c> to advance position, <c>false</c> otherwise.</param>
    /// <returns>Error number</returns>
    ErrorNumber WriteBytesAt(long position, byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                             bool advance = true);
}