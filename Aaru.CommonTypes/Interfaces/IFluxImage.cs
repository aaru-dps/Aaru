// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IFluxImage.cs
// Author(s)      : Rebecca Wallander <sakcheen+github@gmail.com>
//
// Component      : Flux image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by flux image plugins.
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
// Copyright © 2011-2024 Rebecca Wallander
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc />
/// <summary>Abstract class to implement flux reading plugins.</summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IFluxImage : IBaseImage
{
    /// <summary>
    ///     An image may have more than one capture for a specific head/track/sub-track combination. This returns
    ///     the amount of captures in the image for the specified head/track/sub-track combination.
    /// </summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="length">The number of captures</param>
    ErrorNumber CapturesLength(uint head, ushort track, byte subTrack, out uint length);

    /// <summary>Reads the resolution (sample rate) of a index signal capture in picoseconds.</summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    /// <param name="resolution">The resolution of the index capture in picoseconds</param>
    ErrorNumber ReadFluxIndexResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                        out ulong resolution);

    /// <summary>Reads the resolution (sample rate) of a data signal capture in picoseconds.</summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    /// <param name="resolution">The resolution of the data capture in picoseconds</param>
    ErrorNumber ReadFluxDataResolution(uint head, ushort track, byte subTrack, uint captureIndex, out ulong resolution);

    /// <summary>Reads the resolution (sample rate) of a flux capture in picoseconds.</summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    /// <param name="indexResolution">The resolution of the index capture in picoseconds</param>
    /// <param name="dataResolution">The resolution of the data capture in picoseconds</param>
    ErrorNumber ReadFluxResolution(uint head, ushort track, byte subTrack, uint captureIndex, out ulong indexResolution,
                                   out ulong dataResolution);

    /// <summary>Reads the entire flux capture with index and data streams, as well as its resolution.</summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    /// <param name="indexResolution">The resolution (sample rate) of the index capture in picoseconds</param>
    /// <param name="dataResolution">The resolution (sample rate) of the data capture in picoseconds</param>
    /// <param name="indexBuffer">Buffer to store the index stream in</param>
    /// <param name="dataBuffer">Buffer to store the data stream in</param>
    ErrorNumber ReadFluxCapture(uint head, ushort track, byte subTrack, uint captureIndex, out ulong indexResolution,
                                out ulong dataResolution, out byte[] indexBuffer, out byte[] dataBuffer);

    /// <summary>Reads a capture's index stream.</summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    /// <param name="buffer">Buffer to store the data in</param>
    ErrorNumber ReadFluxIndexCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer);

    /// <summary>Reads a capture's data stream.</summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="subTrack">Physical sub-step of track (e.g. half-track)</param>
    /// <param name="captureIndex">Which capture to read. See also <see cref="CapturesLength" /></param>
    /// <param name="buffer">Buffer to store the data in</param>
    ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer);

    /// <summary>
    ///     An image may have tracks split into sub-steps. This returns the highest sub-step index for the track.
    /// </summary>
    /// <returns>Error number</returns>
    /// <param name="head">Physical head (0-based)</param>
    /// <param name="track">Physical track (position of the heads over the floppy media, 0-based)</param>
    /// <param name="length">The number of captures</param>
    ErrorNumber SubTrackLength(uint head, ushort track, out byte length);
}