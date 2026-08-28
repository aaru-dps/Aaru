// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VideoNowOffsetTests.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.Core.Media.Detection;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Media;

/// <summary>
///     Covers the VideoNow and VideoNow Color combined offset search. These regressed silently because
///     <c>CompactDisc.GetOffset</c> handed the search a nine sector buffer it had allocated but never filled from the
///     drive, so the marker never matched and every VideoNow dump reported an offset of zero, disabling offset
///     correction entirely.
/// </summary>
[TestFixture]
[Category("Unit")]
public class VideoNowOffsetTests
{
    /// <summary>Size of the buffer both searches are handed, nine sectors of 2352 bytes.</summary>
    const int FRAME_BUFFER_SIZE = 9 * 2352;

    /// <summary>Byte position of the first video channel byte of the frame header in the interleaved stream.</summary>
    const int VIDEO_NOW_FRAME_START = 10124;

    /// <summary>Same, for VideoNow Color.</summary>
    const int VIDEO_NOW_COLOR_FRAME_START = 18032;

    /// <summary>Filler that cannot be part of either marker, so the only match is the one we plant.</summary>
    const byte FILLER = 0x55;

    /// <summary>The black and white marker, 0xE1 on both video channel bytes of every sample, 335 samples long.</summary>
    static byte[] BlackAndWhiteMarker() =>
        Enumerable.Repeat<byte[]>([0xE1, 0xE1, 0x00, 0x00], 335).SelectMany(static b => b).ToArray();

    static byte[] BufferWith(byte[] marker, int framePosition)
    {
        var buffer = new byte[FRAME_BUFFER_SIZE];
        buffer.AsSpan().Fill(FILLER);
        Array.Copy(marker, 0, buffer, framePosition, marker.Length);

        return buffer;
    }

    [Test]
    public void VideoNowOffsetIsZeroWhenTheMarkerIsAbsent()
    {
        var buffer = new byte[FRAME_BUFFER_SIZE];
        buffer.AsSpan().Fill(FILLER);

        MMC.GetVideoNowOffset(buffer).Should().Be(0);
    }

    /// <summary>
    ///     Regression guard for the bug itself. An all zero buffer, which is exactly what <c>GetOffset</c> used to
    ///     pass in, yields zero. A zero result therefore means "not found" and is indistinguishable from a genuine zero
    ///     offset, which is why the failure went unnoticed.
    /// </summary>
    [Test]
    public void AnUnfilledBufferYieldsZeroForBothVariants()
    {
        var buffer = new byte[FRAME_BUFFER_SIZE];

        MMC.GetVideoNowOffset(buffer).Should().Be(0);
        MMC.GetVideoNowColorOffset(buffer).Should().Be(0);
    }

    [Test]
    public void VideoNowOffsetIsZeroWhenTheFrameIsWhereItShouldBe() =>
        MMC.GetVideoNowOffset(BufferWith(BlackAndWhiteMarker(), VIDEO_NOW_FRAME_START)).Should().Be(0);

    [Test]
    public void VideoNowOffsetIsPositiveWhenTheFrameStartsEarly()
    {
        const int framePosition = 9000;

        MMC.GetVideoNowOffset(BufferWith(BlackAndWhiteMarker(), framePosition))
           .Should()
           .Be(VIDEO_NOW_FRAME_START - framePosition);
    }

    [Test]
    public void VideoNowOffsetIsNegativeWhenTheFrameStartsLate()
    {
        const int framePosition = 12000;

        MMC.GetVideoNowOffset(BufferWith(BlackAndWhiteMarker(), framePosition))
           .Should()
           .Be(VIDEO_NOW_FRAME_START - framePosition);
    }

    /// <summary>
    ///     The right (audio) channel of a black and white frame carries audio samples, not zeroes, so the search masks
    ///     bytes 2 and 3 of every sample before comparing. Real audio content must not prevent a match.
    /// </summary>
    [Test]
    public void VideoNowOffsetIgnoresTheAudioChannel()
    {
        const int framePosition = 9000;

        byte[] marker = BlackAndWhiteMarker();

        for(var ab = 2; ab < marker.Length; ab += 4)
        {
            marker[ab]     = 0xAA;
            marker[ab + 1] = 0xBB;
        }

        MMC.GetVideoNowOffset(BufferWith(marker, framePosition))
           .Should()
           .Be(VIDEO_NOW_FRAME_START - framePosition);
    }

    [Test]
    public void VideoNowColorOffsetIsZeroWhenTheMarkerIsAbsent()
    {
        var buffer = new byte[FRAME_BUFFER_SIZE];
        buffer.AsSpan().Fill(FILLER);

        MMC.GetVideoNowColorOffset(buffer).Should().Be(0);
    }

    [Test]
    public void VideoNowColorOffsetIsPositiveWhenTheFrameStartsEarly()
    {
        const int framePosition = 17000;

        MMC.GetVideoNowColorOffset(BufferWith(VideoNowColorMarker(), framePosition))
           .Should()
           .Be(VIDEO_NOW_COLOR_FRAME_START - framePosition);
    }

    [Test]
    public void VideoNowColorOffsetIsNegativeWhenTheFrameStartsLate()
    {
        const int framePosition = 19000;

        MMC.GetVideoNowColorOffset(BufferWith(VideoNowColorMarker(), framePosition))
           .Should()
           .Be(VIDEO_NOW_COLOR_FRAME_START - framePosition);
    }

    /// <summary>Every tenth byte of a VideoNow Color frame header is an audio byte, and is masked before comparing.</summary>
    [Test]
    public void VideoNowColorOffsetIgnoresTheAudioChannel()
    {
        const int framePosition = 17000;

        byte[] marker = VideoNowColorMarker();

        for(var ab = 9; ab < marker.Length; ab += 10) marker[ab] = 0xAA;

        MMC.GetVideoNowColorOffset(BufferWith(marker, framePosition))
           .Should()
           .Be(VIDEO_NOW_COLOR_FRAME_START - framePosition);
    }

    /// <summary>The VideoNow Color frame header, copied verbatim from the detection code it is matched against.</summary>
    static byte[] VideoNowColorMarker() =>
    [
        0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3,
        0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81,
        0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7,
        0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3,
        0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00,
        0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3,
        0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81,
        0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7,
        0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3,
        0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00,
        0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3,
        0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81,
        0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7,
        0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x00, 0x00, 0x02, 0x01, 0x04, 0x02, 0x06, 0x03, 0xFF, 0x00, 0x08, 0x04,
        0x0A, 0x05, 0x0C, 0x06, 0x0E, 0x07, 0xFF, 0x00, 0x11, 0x08, 0x13, 0x09, 0x15, 0x0A, 0x17, 0x0B, 0xFF, 0x00,
        0x19, 0x0C, 0x1B, 0x0D, 0x1D, 0x0E, 0x1F, 0x0F, 0xFF, 0x00, 0x00, 0x28, 0x02, 0x29, 0x04, 0x2A, 0x06, 0x2B,
        0xFF, 0x00, 0x08, 0x2C, 0x0A, 0x2D, 0x0C, 0x2E, 0x0E, 0x2F, 0xFF, 0x00, 0x11, 0x30, 0x13, 0x31, 0x15, 0x32,
        0x17, 0x33, 0xFF, 0x00, 0x19, 0x34, 0x1B, 0x35, 0x1D, 0x36, 0x1F, 0x37, 0xFF, 0x00, 0x00, 0x38, 0x02, 0x39,
        0x04, 0x3A, 0x06, 0x3B, 0xFF, 0x00, 0x08, 0x3C, 0x0A, 0x3D, 0x0C, 0x3E, 0x0E, 0x3F, 0xFF, 0x00, 0x11, 0x40,
        0x13, 0x41, 0x15, 0x42, 0x17, 0x43, 0xFF, 0x00, 0x19, 0x44, 0x1B, 0x45, 0x1D, 0x46, 0x1F, 0x47, 0xFF, 0x00,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0x00
    ];
}
