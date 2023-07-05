// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for A2R flux images.
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
// Copyright © 2011-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;

namespace Aaru.DiscImages;

public sealed partial class A2R
{
    /// <summary>
    /// Takes a Head, Track and Sub-Track representation, as well as the <c>MediaType</c>,
    /// and converts it to the Track representation used by A2R.
    /// </summary>
    /// <param name="head">The head number</param>
    /// <param name="track">The track number</param>
    /// <param name="subTrack">The sub-track number</param>
    /// <param name="mediaType">The media type of the image</param>
    /// <returns>A2R format location</returns>
    static long HeadTrackSubToA2rLocation(uint head, ushort track, byte subTrack, MediaType mediaType)
    {
        if(mediaType == MediaType.Apple32SS)
            return head + (track * 4) + subTrack;

        return head + (track * 2);
    }

    /// <summary>
    /// Takes a Head, Track and Sub-Track representation, as well as the <c>A2rDriveType</c>,
    /// and converts it to the Track representation used by A2R.
    /// </summary>
    /// <param name="head">The head number</param>
    /// <param name="track">The track number</param>
    /// <param name="subTrack">The sub-track number</param>
    /// <param name="driveType">The drive type enum of the A2R image</param>
    /// <returns>A2R format location</returns>
    static long HeadTrackSubToA2rLocation(uint head, ushort track, byte subTrack, A2rDriveType driveType)
    {
        if(driveType == A2rDriveType.SS_525_40trk_quarterStep)
            return head + (track * 4) + subTrack;

        return head + (track * 2);
    }

    /// <summary>
    /// Takes an A2R location and a <c>MediaType</c>, and converts it to a Head, Track and Sub-Track representation
    /// used by the internal representation. The <c>MediaType</c> is needed because the track location is different
    /// for different types of media sources.
    /// </summary>
    /// <param name="location">A2R format location</param>
    /// <param name="mediaType"></param>
    /// <param name="head">The head number</param>
    /// <param name="track">The track number</param>
    /// <param name="subTrack">The sub-track number</param>
    static void A2rLocationToHeadTrackSub(uint location, MediaType mediaType, out uint head, out ushort track,
                                          out byte subTrack)
    {
        if(mediaType == MediaType.Apple32SS)
        {
            head     = 0;
            track    = (ushort)(location / 4);
            subTrack = (byte)(location   % 4);

            return;
        }

        head     = location % 2;
        track    = (ushort)((location - head) / 2);
        subTrack = 0;
    }

    /// <summary>
    /// Takes a single number flux (uint length) and converts it to a flux in the
    /// internal representation format (byte length)
    /// </summary>
    /// <param name="ticks">The <c>uint</c> flux representation</param>
    /// <returns>The <c>byte[]</c> flux representation</returns>
    static byte[] UInt32ToFluxRepresentation(uint ticks)
    {
        uint over = ticks / 255;

        if(over == 0)
            return new[]
            {
                (byte)ticks
            };

        byte[] expanded = new byte[over + 1];

        Array.Fill(expanded, (byte)255, 0, (int)over);
        expanded[^1] = (byte)(ticks % 255);

        return expanded;
    }

    /// <summary>
    /// Takes a flux representation in the internal format (byte length) and converts it to
    /// an array of single number fluxes (uint length)
    /// </summary>
    /// <param name="flux">The <c>byte[]</c> flux representation</param>
    /// <returns>The <c>uint</c> flux representation</returns>
    static List<uint> FluxRepresentationsToUInt32List(IEnumerable<byte> flux)
    {
        List<uint> scpData = new();
        uint       tick    = 0;

        foreach(byte b in flux)
        {
            if(b == 255)
                tick += 255;
            else
            {
                tick += b;
                scpData.Add(tick);
                tick = 0;
            }
        }

        return scpData;
    }

    /// <summary>
    /// A2R has two types of flux capture types; "timing" and "xtiming". The only difference is the length of the
    /// capture, with "timing" being about 1¼ revolutions. This function returns <c>true</c> if the flux buffer is "timing"
    /// and <c>false</c> otherwise.
    /// </summary>
    /// <param name="resolution">The resolution of the flux capture</param>
    /// <param name="buffer">The flux data</param>
    /// <returns><c>true</c> if "timing", <c>false</c> if "xtiming"</returns>
    static bool IsCaptureTypeTiming(ulong resolution, byte[] buffer) =>

        // TODO: This is only accurate for 300rpm
        buffer.Select(static x => (int)x).Sum() * (long)resolution is > 230000000000 and < 270000000000;
}