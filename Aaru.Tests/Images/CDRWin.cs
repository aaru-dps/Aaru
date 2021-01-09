// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDRWin.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.DiscImages;
using Aaru.Filters;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class CDRWin
    {
        readonly string[] _testFiles =
        {
            "pcengine.cue", "pcfx.cue", "report_audiocd.cue", "report_cdr.cue", "report_cdrw.cue",
            "test_audiocd_cdtext.cue", "test_incd_udf200_finalized.cue", "test_multi_karaoke_sampler.cue",
            "test_multiple_indexes.cue", "test_videocd.cue", "cdg/report_audiocd.cue",
            "cdg/test_multi_karaoke_sampler.cue", "cooked_cdg/test_multi_karaoke_sampler.cue",
            "cooked/report_cdrom.cue", "cooked/report_cdrw.cue", "cooked/test_multi_karaoke_sampler.cue"
        };

        readonly ulong[] _sectors =
        {
            // pcengine.cue
            160356,

            // pcfx.cue
            246305,

            // report_audiocd.cue
            247073,

            // report_cdr.cue
            254265,

            // report_cdrw.cue
            308224,

            // test_audiocd_cdtext.cue
            277696,

            // test_incd_udf200_finalized.cue
            350134,

            // test_multi_karaoke_sampler.cue
            329008,

            // test_multiple_indexes.cue
            65536,

            // test_videocd.cue
            48794,

            // cdg/report_audiocd.cue
            247073,

            // cdg/test_multi_karaoke_sampler.cue
            329008,

            // cooked_cdg/test_multi_karaoke_sampler.cue
            329008,

            // cooked/report_cdrom.cue
            254265,

            // cooked/report_cdrw.cue
            308224,

            // cooked/test_multi_karaoke_sampler.cue
            329008
        };

        readonly MediaType[] _mediaTypes =
        {
            // pcengine.cue
            MediaType.CD,

            // pcfx.cue
            MediaType.CD,

            // report_audiocd.cue
            MediaType.CDDA,

            // report_cdr.cue
            MediaType.CDROM,

            // report_cdrw.cue
            MediaType.CDROM,

            // test_audiocd_cdtext.cue
            MediaType.CDDA,

            // test_incd_udf200_finalized.cue
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.cue
            MediaType.CDROMXA,

            // test_multiple_indexes.cue
            MediaType.CDDA,

            // test_videocd.cue
            MediaType.CDROMXA,

            // cdg/report_audiocd.cue
            MediaType.CDDA,

            // cdg/test_multi_karaoke_sampler.cue
            MediaType.CDROMXA,

            // cooked_cdg/test_multi_karaoke_sampler.cue
            MediaType.CDROMXA,

            // cooked/report_cdrom.cue
            MediaType.CDROM,

            // cooked/report_cdrw.cue
            MediaType.CDROM,

            // cooked/test_multi_karaoke_sampler.cue
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // pcengine.cue
            "8eb436b476c9df343acb89ac1ba7e1b4",

            // pcfx.cue
            "73e2855fff156f95fb8f0ae7c58d1b9d",

            // report_audiocd.cue
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdr.cue
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.cue
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // test_audiocd_cdtext.cue
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_incd_udf200_finalized.cue
            "13d4c3def37e968b2ddc5cf5a9f18fdc",

            // test_multi_karaoke_sampler.cue
            "f09312ba25a479fb81912a2965babd22",

            // test_multiple_indexes.cue
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_videocd.cue
            "0d80890beeadf3f6e2cf2f88d0067afe",

            // cdg/report_audiocd.cue
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // cdg/test_multi_karaoke_sampler.cue
            "f09312ba25a479fb81912a2965babd22",

            // cooked_cdg/test_multi_karaoke_sampler.cue
            "f09312ba25a479fb81912a2965babd22",

            // cooked/report_cdrom.cue
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // cooked/report_cdrw.cue
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // cooked/test_multi_karaoke_sampler.cue
            "f09312ba25a479fb81912a2965babd22"
        };

        readonly string[] _longMd5S =
        {
            // pcengine.cue
            "bdcd5cabf4f48333f9dbb08967dce7a8",

            // pcfx.cue
            "f421fc4af3ac528911b6d824825ff9b5",

            // report_audiocd.cue
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdr.cue
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.cue
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // test_audiocd_cdtext.cue
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_incd_udf200_finalized.cue
            "31e772f6997eb8dbf3ecf9aca9ea6bc6",

            // test_multi_karaoke_sampler.cue
            "f48603d11883593f45ec4a3824681e4e",

            // test_multiple_indexes.cue
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_videocd.cue
            "96ac6c364e4c3cb2f043197a45a97183",

            // cdg/report_audiocd.cue
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // cdg/test_multi_karaoke_sampler.cue
            "f48603d11883593f45ec4a3824681e4e",

            // cooked_cdg/test_multi_karaoke_sampler.cue
            "UNKNOWN",

            // cooked/report_cdrom.cue
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // cooked/report_cdrw.cue
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // cooked/test_multi_karaoke_sampler.cue
            "f48603d11883593f45ec4a3824681e4e"
        };

        readonly string[] _subchannelMd5S =
        {
            // pcengine.cue
            null,

            // pcfx.cue
            null,

            // report_audiocd.cue
            null,

            // report_cdr.cue
            null,

            // report_cdrw.cue
            null,

            // test_audiocd_cdtext.cue
            null,

            // test_incd_udf200_finalized.cue
            null,

            // test_multi_karaoke_sampler.cue
            null,

            // test_multiple_indexes.cue
            null,

            // test_videocd.cue
            null,

            // cdg/report_audiocd.cue
            "UNKNOWN",

            // cdg/test_multi_karaoke_sampler.cue
            "UNKNOWN",

            // cooked_cdg/test_multi_karaoke_sampler.cue
            "UNKNOWN",

            // cooked/report_cdrom.cue
            null,

            // cooked/report_cdrw.cue
            null,

            // cooked/test_multi_karaoke_sampler.cue
            null
        };

        readonly int[] _tracks =
        {
            // pcengine.cue
            16,

            // pcfx.cue
            8,

            // report_audiocd.cue
            14,

            // report_cdr.cue
            1,

            // report_cdrw.cue
            1,

            // test_audiocd_cdtext.cue
            11,

            // test_incd_udf200_finalized.cue
            1,

            // test_multi_karaoke_sampler.cue
            16,

            // test_multiple_indexes.cue
            5,

            // test_videocd.cue
            2,

            // cdg/report_audiocd.cue
            14,

            // cdg/test_multi_karaoke_sampler.cue
            16,

            // cooked_cdg/test_multi_karaoke_sampler.cue
            16,

            // cooked/report_cdrom.cue
            1,

            // cooked/report_cdrw.cue
            1,

            // cooked/test_multi_karaoke_sampler.cue
            16
        };

        readonly int[][] _trackSessions =
        {
            // pcengine.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.cue
            new[]
            {
                1
            },

            // report_cdrw.cue
            new[]
            {
                1
            },

            // test_audiocd_cdtext.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_incd_udf200_finalized.cue
            new[]
            {
                1
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.cue
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_videocd.cue
            new[]
            {
                1, 1
            },

            // cdg/report_audiocd.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // cdg/test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // cooked_cdg/test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // cooked/report_cdrom.cue
            new[]
            {
                1
            },

            // cooked/report_cdrw.cue
            new[]
            {
                1
            },

            // cooked/test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // pcengine.cue
            new ulong[]
            {
                0, 3365, 38239, 46692, 52976, 61294, 68038, 74872, 82605, 85956, 90742, 98749, 106168, 111713, 119745,
                125629
            },

            // pcfx.cue
            new ulong[]
            {
                0, 4170, 4684, 5716, 41834, 220420, 225121, 234973
            },

            // report_audiocd.cue
            new ulong[]
            {
                0, 16399, 29901, 47800, 63164, 78775, 94582, 116975, 136016, 153922, 170601, 186389, 201649, 224299
            },

            // report_cdr.cue
            new ulong[]
            {
                0
            },

            // report_cdrw.cue
            new ulong[]
            {
                0
            },

            // test_audiocd_cdtext.cue
            new ulong[]
            {
                0, 29752, 65034, 78426, 95080, 126147, 154959, 191685, 222776, 243438, 269600
            },

            // test_incd_udf200_finalized.cue
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                0, 1737, 32449, 52372, 70004, 99798, 119461, 136699, 155490, 175526, 206161, 226150, 244055, 273665,
                293452, 310411
            },

            // test_multiple_indexes.cue
            new ulong[]
            {
                0, 4654, 13725, 41035, 54839
            },

            // test_videocd.cue
            new ulong[]
            {
                0, 1252
            },

            // cdg/report_audiocd.cue
            new ulong[]
            {
                0, 16399, 29901, 47800, 63164, 78775, 94582, 116975, 136016, 153922, 170601, 186389, 201649, 224299
            },

            // cdg/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                0, 1737, 32449, 52372, 70004, 99798, 119461, 136699, 155490, 175526, 206161, 226150, 244055, 273665,
                293452, 310411
            },

            // cooked_cdg/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                0, 1737, 32449, 52372, 70004, 99798, 119461, 136699, 155490, 175526, 206161, 226150, 244055, 273665,
                293452, 310411
            },

            // cooked/report_cdrom.cue
            new ulong[]
            {
                0
            },

            // cooked/report_cdrw.cue
            new ulong[]
            {
                0
            },

            // cooked/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                0, 1737, 32449, 52372, 70004, 99798, 119461, 136699, 155490, 175526, 206161, 226150, 244055, 273665,
                293452, 310411
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // pcengine.cue
            new ulong[]
            {
                3364, 38238, 46691, 52975, 61293, 68037, 74871, 82604, 85955, 90741, 98748, 106167, 111712, 119744,
                125628, 160355
            },

            // pcfx.cue
            new ulong[]
            {
                4169, 4683, 5715, 41833, 220419, 225120, 234972, 246304
            },

            // report_audiocd.cue
            new ulong[]
            {
                16398, 29900, 47799, 63163, 78774, 94581, 116974, 136015, 153921, 170600, 186388, 201648, 224298, 247072
            },

            // report_cdr.cue
            new ulong[]
            {
                254264
            },

            // report_cdrw.cue
            new ulong[]
            {
                308223
            },

            // test_audiocd_cdtext.cue
            new ulong[]
            {
                29751, 65033, 78425, 95079, 126146, 154958, 191684, 222775, 243437, 269599, 277695
            },

            // test_incd_udf200_finalized.cue
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                1736, 32448, 52371, 70003, 99797, 119460, 136698, 155489, 175525, 206160, 226149, 244054, 273664,
                293451, 310410, 329007
            },

            // test_multiple_indexes.cue
            new ulong[]
            {
                4653, 13724, 41034, 54838, 65535
            },

            // test_videocd.cue
            new ulong[]
            {
                1251, 48793
            },

            // cdg/report_audiocd.cue
            new ulong[]
            {
                16398, 29900, 47799, 63163, 78774, 94581, 116974, 136015, 153921, 170600, 186388, 201648, 224298, 247072
            },

            // cdg/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                1736, 32448, 52371, 70003, 99797, 119460, 136698, 155489, 175525, 206160, 226149, 244054, 273664,
                293451, 310410, 329007
            },

            // cooked_cdg/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                1736, 32448, 52371, 70003, 99797, 119460, 136698, 155489, 175525, 206160, 226149, 244054, 273664,
                293451, 310410, 329007
            },

            // cooked/report_cdrom.cue
            new ulong[]
            {
                254264
            },

            // cooked/report_cdrw.cue
            new ulong[]
            {
                308223
            },

            // cooked/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                1736, 32448, 52371, 70003, 99797, 119460, 136698, 155489, 175525, 206160, 226149, 244054, 273664,
                293451, 310410, 329007
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // pcengine.cue
            new ulong[]
            {
                150, 225, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 225
            },

            // pcfx.cue
            new ulong[]
            {
                150, 225, 0, 0, 0, 150, 150, 150
            },

            // report_audiocd.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // report_cdr.cue
            new ulong[]
            {
                150
            },

            // report_cdrw.cue
            new ulong[]
            {
                150
            },

            // test_audiocd_cdtext.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_incd_udf200_finalized.cue
            new ulong[]
            {
                150
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_multiple_indexes.cue
            new ulong[]
            {
                150, 150, 150, 150, 150
            },

            // test_videocd.cue
            new ulong[]
            {
                150, 0
            },

            // cdg/report_audiocd.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // cdg/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // cooked_cdg/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // cooked/report_cdrom.cue
            new ulong[]
            {
                150
            },

            // cooked/report_cdrw.cue
            new ulong[]
            {
                150
            },

            // cooked/test_multi_karaoke_sampler.cue
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            }
        };

        readonly byte[][] _trackFlags =
        {
            // pcengine.cue
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.cue
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // report_audiocd.cue
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.cue
            new byte[]
            {
                4
            },

            // report_cdrw.cue
            new byte[]
            {
                4
            },

            // test_audiocd_cdtext.cue
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_incd_udf200_finalized.cue
            new byte[]
            {
                7
            },

            // test_multi_karaoke_sampler.cue
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.cue
            new byte[]
            {
                2, 0, 0, 8, 1
            },

            // test_videocd.cue
            new byte[]
            {
                4, 4
            },

            // cdg/report_audiocd.cue
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // cdg/test_multi_karaoke_sampler.cue
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // cooked_cdg/test_multi_karaoke_sampler.cue
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // cooked/report_cdrom.cue
            new byte[]
            {
                4
            },

            // cooked/report_cdrw.cue
            new byte[]
            {
                4
            },

            // cooked/test_multi_karaoke_sampler.cue
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        [Test]
        public void Test()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CDRWin");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new CdrWin();
                Assert.AreEqual(true, images[i].Open(filters[i]), $"Open: {_testFiles[i]}");
            }

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_sectors[i], images[i].Info.Sectors, $"Sectors: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_mediaTypes[i], images[i].Info.MediaType, $"Media type: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_tracks[i], images[i].Tracks.Count, $"Tracks: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackSessions[i].Should().
                                  BeEquivalentTo(images[i].Tracks.Select(t => t.TrackSession),
                                                 $"Track session: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackStarts[i].Should().BeEquivalentTo(images[i].Tracks.Select(t => t.TrackStartSector),
                                                        $"Track start: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackEnds[i].Should().
                              BeEquivalentTo(images[i].Tracks.Select(t => t.TrackEndSector),
                                             $"Track end: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackPregaps[i].Should().
                                 BeEquivalentTo(images[i].Tracks.Select(t => t.TrackPregap),
                                                $"Track pregap: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                int trackNo = 0;

                foreach(Track currentTrack in images[i].Tracks)
                {
                    if(images[i].Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                        Assert.AreEqual(_trackFlags[i][trackNo],
                                        images[i].ReadSectorTag(currentTrack.TrackSequence, SectorTagType.CdTrackFlags)
                                            [0], $"Track flags: {_testFiles[i]}, track {currentTrack.TrackSequence}");

                    trackNo++;
                }
            }

            foreach(bool @long in new[]
            {
                false, true
            })
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var ctx = new Md5Context();

                    foreach(Track currentTrack in images[i].Tracks)
                    {
                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= sectorsToRead)
                            {
                                sector = @long ? images[i].
                                                 ReadSectorsLong(doneSectors, sectorsToRead, currentTrack.TrackSequence)
                                             : images[i].
                                                 ReadSectors(doneSectors, sectorsToRead, currentTrack.TrackSequence);

                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = @long ? images[i].ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                           currentTrack.TrackSequence)
                                             : images[i].ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                     currentTrack.TrackSequence);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(),
                                    $"{(@long ? "Long hash" : "Hash")}: {_testFiles[i]}");
                }

            for(int i = 0; i < _testFiles.Length; i++)
                if(images[i].Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                {
                    var ctx = new Md5Context();

                    foreach(Track currentTrack in images[i].Tracks)
                    {
                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= sectorsToRead)
                            {
                                sector = images[i].ReadSectorsTag(doneSectors, sectorsToRead,
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = images[i].ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(_subchannelMd5S[i], ctx.End(), $"Subchannel hash: {_testFiles[i]}");
                }
        }
    }
}