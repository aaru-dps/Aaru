// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IWritableFluxImage.cs
// Author(s)      : Rebecca Wallander <sakcheen+github@gmail.com>
//
// Component      : Writable flux image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by writable flux image plugins.
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
// Copyright © 2011-2023 Rebecca Wallander
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc cref="IWritableImage" />
/// <summary>Abstract class to implement flux writing plugins.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedParameter.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public interface IWritableFluxImage : IFluxImage, IWritableImage
{
    /// <summary>Writes a flux capture.</summary>
    /// <returns>Error number</returns>
    /// <param name="indexResolution">The index capture's resolution (sample rate) in picoseconds</param>
    /// <param name="dataResolution">The capture's resolution (sample rate) in picoseconds</param>
    /// <param name="indexBuffer">Flux representation of the index signal</param>
    /// <param name="dataBuffer">Flux representation of the data signal</param>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture slot to write to. See also <see cref="IFluxImage.CapturesLength" /></param>
    ErrorNumber WriteFluxCapture(ulong indexResolution, ulong  dataResolution, byte[] indexBuffer, byte[] dataBuffer,
                                 uint  head,            ushort track,          byte   subTrack,    uint   captureIndex);

    /// <summary>Writes a capture's index stream.</summary>
    /// <returns>Error number</returns>
    /// <param name="resolution">The capture's resolution (sample rate) in picoseconds</param>
    /// <param name="index">Flux representation of the index signal</param>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    ErrorNumber WriteFluxIndexCapture(ulong resolution, byte[] index, uint head, ushort track, byte subTrack,
                                      uint  captureIndex);

    /// <summary>Writes a capture's data stream.</summary>
    /// <returns>Error number</returns>
    /// <param name="resolution">The capture's resolution (sample rate) in picoseconds</param>
    /// <param name="data">Flux representation of the data signal</param>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    ErrorNumber WriteFluxDataCapture(ulong resolution, byte[] data, uint head, ushort track, byte subTrack,
                                     uint  captureIndex);
}