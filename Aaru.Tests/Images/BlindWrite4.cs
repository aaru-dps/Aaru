// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite5.cs
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
using Aaru.Filters;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class BlindWrite4
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.BWT", "report_audiocd.BWT", "report_cdr.BWT", "report_cdrom.BWT",
            "report_cdrw.BWT", "report_enhancedcd.BWT", "test_all_tracks_are_track1.BWT", "test_audiocd_cdtext.BWT",
            "test_castrated_leadout.BWT", "test_data_track_as_audio.BWT", "test_data_track_as_audio_fixed_sub.BWT",
            "test_disc_starts_at_track2.BWT", "test_enhancedcd.BWT", "test_incd_udf200_finalized.BWT",
            "test_multi_karaoke_sampler.BWT", "test_multiple_indexes.BWT", "test_multisession.BWT",
            "test_track2_inside_track1.BWT", "test_videocd.BWT"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.BWT
            279300,

            // report_audiocd.BWT
            247073,

            // report_cdr.BWT
            254265,

            // report_cdrom.BWT
            254265,

            // report_cdrw.BWT
            308224,

            // report_enhancedcd.BWT
            303316,

            // test_all_tracks_are_track1.BWT
            62385,

            // test_audiocd_cdtext.BWT
            277696,

            // test_castrated_leadout.BWT
            269750,

            // test_data_track_as_audio.BWT
            62385,

            // test_data_track_as_audio_fixed_sub.BWT
            62385,

            // test_disc_starts_at_track2.BWT
            62385,

            // test_enhancedcd.BWT
            59206,

            // test_incd_udf200_finalized.BWT
            350134,

            // test_multi_karaoke_sampler.BWT
            329158,

            // test_multiple_indexes.BWT
            65536,

            // test_multisession.BWT
            51168,

            // test_track2_inside_track1.BWT
            62385,

            // test_videocd.BWT
            48794
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.BWT
            MediaType.CDDA,

            // report_audiocd.BWT
            MediaType.CDDA,

            // report_cdr.BWT
            MediaType.CDROM,

            // report_cdrom.BWT
            MediaType.CDROM,

            // report_cdrw.BWT
            MediaType.CDROM,

            // report_enhancedcd.BWT
            MediaType.CDPLUS,

            // test_all_tracks_are_track1.BWT
            MediaType.CDROMXA,

            // test_audiocd_cdtext.BWT
            MediaType.CDDA,

            // test_castrated_leadout.BWT
            MediaType.CDDA,

            // test_data_track_as_audio.BWT
            MediaType.CDROMXA,

            // test_data_track_as_audio_fixed_sub.BWT
            MediaType.CDROMXA,

            // test_disc_starts_at_track2.BWT
            MediaType.CDROMXA,

            // test_enhancedcd.BWT
            MediaType.CDPLUS,

            // test_incd_udf200_finalized.BWT
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.BWT
            MediaType.CDROMXA,

            // test_multiple_indexes.BWT
            MediaType.CDDA,

            // test_multisession.BWT
            MediaType.CDROMXA,

            // test_track2_inside_track1.BWT
            MediaType.CDROMXA,

            // test_videocd.BWT
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.BWT
            "3be5ba5cc64cd63030d970cd8eeecc99",

            // report_audiocd.BWT
            "0e4c52acfb90e8954b70b7c50ba01ffb",

            // report_cdr.BWT
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrom.BWT
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.BWT
            "UNKNOWN",

            // report_enhancedcd.BWT
            "UNKNOWN",

            // test_all_tracks_are_track1.BWT
            "UNKNOWN",

            // test_audiocd_cdtext.BWT
            "3463a12134de20f22340d4d36f75ecf1",

            // test_castrated_leadout.BWT
            "0639197a3c2292f62745e05b7e701e4d",

            // test_data_track_as_audio.BWT
            "e1664576ae56b98faaf60652fd050e15",

            // test_data_track_as_audio_fixed_sub.BWT
            "e1664576ae56b98faaf60652fd050e15",

            // test_disc_starts_at_track2.BWT
            "UNKNOWN",

            // test_enhancedcd.BWT
            "UNKNOWN",

            // test_incd_udf200_finalized.BWT
            "f95d6f978ddb4f98bbffda403f627fe1",

            // test_multi_karaoke_sampler.BWT
            "0f8f94e00fed4a163f2590632a1c163e",

            // test_multiple_indexes.BWT
            "9a5ab4e16c0410d4b2040ce836e78d45",

            // test_multisession.BWT
            "UNKNOWN",

            // test_track2_inside_track1.BWT
            "UNKNOWN",

            // test_videocd.BWT
            "UNKNOWN"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.BWT
            "3be5ba5cc64cd63030d970cd8eeecc99",

            // report_audiocd.BWT
            "0e4c52acfb90e8954b70b7c50ba01ffb",

            // report_cdr.BWT
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrom.BWT
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.BWT
            "UNKNOWN",

            // report_enhancedcd.BWT
            "UNKNOWN",

            // test_all_tracks_are_track1.BWT
            "UNKNOWN",

            // test_audiocd_cdtext.BWT
            "3463a12134de20f22340d4d36f75ecf1",

            // test_castrated_leadout.BWT
            "0639197a3c2292f62745e05b7e701e4d",

            // test_data_track_as_audio.BWT
            "1e1a4024e652668b09868b238aadc0f7",

            // test_data_track_as_audio_fixed_sub.BWT
            "1e1a4024e652668b09868b238aadc0f7",

            // test_disc_starts_at_track2.BWT
            "UNKNOWN",

            // test_enhancedcd.BWT
            "UNKNOWN",

            // test_incd_udf200_finalized.BWT
            "6751e0ae7821f92221672b1cd5a1ff36",

            // test_multi_karaoke_sampler.BWT
            "7f8cca32ee186cf1d70d21882cbe8274",

            // test_multiple_indexes.BWT
            "9a5ab4e16c0410d4b2040ce836e78d45",

            // test_multisession.BWT
            "UNKNOWN",

            // test_track2_inside_track1.BWT
            "UNKNOWN",

            // test_videocd.BWT
            "UNKNOWN"
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.BWT
            "fb42293c276a95724616ad25dcc734b6",

            // report_audiocd.BWT
            "5fe9338986050d5631a519a3242dda2d",

            // report_cdr.BWT
            "UNKNOWN",

            // report_cdrom.BWT
            "UNKNOWN",

            // report_cdrw.BWT
            "UNKNOWN",

            // report_enhancedcd.BWT
            "UNKNOWN",

            // test_all_tracks_are_track1.BWT
            "UNKNOWN",

            // test_audiocd_cdtext.BWT
            "73c889ef800df274824d4212c4a060a1",

            // test_castrated_leadout.BWT
            "28267d1e5dbc9589cc2cccc1b7a47095",

            // test_data_track_as_audio.BWT
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.BWT
            "UNKNOWN",

            // test_disc_starts_at_track2.BWT
            "UNKNOWN",

            // test_enhancedcd.BWT
            "UNKNOWN",

            // test_incd_udf200_finalized.BWT
            "UNKNOWN",

            // test_multi_karaoke_sampler.BWT
            "b840fe64cd1784f166fd0ac378487ae0",

            // test_multiple_indexes.BWT
            "bd86329c11da806cda20b57872aa0a49",

            // test_multisession.BWT
            "UNKNOWN",

            // test_track2_inside_track1.BWT
            "UNKNOWN",

            // test_videocd.BWT
            "UNKNOWN"
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.BWT
            22,

            // report_audiocd.BWT
            14,

            // report_cdr.BWT
            1,

            // report_cdrom.BWT
            1,

            // report_cdrw.BWT
            1,

            // report_enhancedcd.BWT
            14,

            // test_all_tracks_are_track1.BWT
            2,

            // test_audiocd_cdtext.BWT
            11,

            // test_castrated_leadout.BWT
            11,

            // test_data_track_as_audio.BWT
            2,

            // test_data_track_as_audio_fixed_sub.BWT
            2,

            // test_disc_starts_at_track2.BWT
            2,

            // test_enhancedcd.BWT
            3,

            // test_incd_udf200_finalized.BWT
            1,

            // test_multi_karaoke_sampler.BWT
            16,

            // test_multiple_indexes.BWT
            5,

            // test_multisession.BWT
            4,

            // test_track2_inside_track1.BWT
            3,

            // test_videocd.BWT
            2
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.BWT
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.BWT
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.BWT
            new[]
            {
                1
            },

            // report_cdrom.BWT
            new[]
            {
                1
            },

            // report_cdrw.BWT
            new[]
            {
                1
            },

            // report_enhancedcd.BWT
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_all_tracks_are_track1.BWT
            new[]
            {
                1, 2
            },

            // test_audiocd_cdtext.BWT
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_castrated_leadout.BWT
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_data_track_as_audio.BWT
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio_fixed_sub.BWT
            new[]
            {
                1, 2
            },

            // test_disc_starts_at_track2.BWT
            new[]
            {
                1, 2
            },

            // test_enhancedcd.BWT
            new[]
            {
                1, 1, 2
            },

            // test_incd_udf200_finalized.BWT
            new[]
            {
                1
            },

            // test_multi_karaoke_sampler.BWT
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.BWT
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.BWT
            new[]
            {
                1, 2, 3, 4
            },

            // test_track2_inside_track1.BWT
            new[]
            {
                1, 1, 2
            },

            // test_videocd.BWT
            new[]
            {
                1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.BWT
            new ulong[]
            {
                69150, 88650, 107475, 112050, 133500, 138075, 159675, 164625, 185250, 189975, 208725, 212850, 232050,
                236550, 241725, 255975, 256725, 265500, 267225, 269850, 271500, 274125
            },

            // report_audiocd.BWT
            new ulong[]
            {
                0, 16399, 29901, 47800, 63164, 78775, 94582, 116975, 136016, 153922, 170601, 186389, 201649, 224299
            },

            // report_cdr.BWT
            new ulong[]
            {
                0
            },

            // report_cdrom.BWT
            new ulong[]
            {
                0
            },

            // report_cdrw.BWT
            new ulong[]
            {
                0
            },

            // report_enhancedcd.BWT
            new ulong[]
            {
                0, 15511, 33809, 51180, 71823, 87432, 103155, 117541, 136017, 153268, 166782, 186963, 201291, 234030
            },

            // test_all_tracks_are_track1.BWT
            new ulong[]
            {
                0, 36789
            },

            // test_audiocd_cdtext.BWT
            new ulong[]
            {
                0, 29752, 65034, 78426, 95080, 126147, 154959, 191685, 222776, 243438, 269600
            },

            // test_castrated_leadout.BWT
            new ulong[]
            {
                0, 29752, 65034, 78426, 95080, 126147, 154959, 191685, 222776, 243438, 269600
            },

            // test_data_track_as_audio.BWT
            new ulong[]
            {
                0, 36789
            },

            // test_data_track_as_audio_fixed_sub.BWT
            new ulong[]
            {
                0, 36789
            },

            // test_disc_starts_at_track2.BWT
            new ulong[]
            {
                0, 36789
            },

            // test_enhancedcd.BWT
            new ulong[]
            {
                0, 14255, 40203
            },

            // test_incd_udf200_finalized.BWT
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.BWT
            new ulong[]
            {
                0, 1737, 32599, 52522, 70154, 99948, 119611, 136849, 155640, 175676, 206311, 226300, 244205, 273815,
                293602, 310561
            },

            // test_multiple_indexes.BWT
            new ulong[]
            {
                0, 4654, 13725, 41035, 54839
            },

            // test_multisession.BWT
            new ulong[]
            {
                0, 19383, 32710, 45228
            },

            // test_track2_inside_track1.BWT
            new ulong[]
            {
                0, 13200, 36789
            },

            // test_videocd.BWT
            new ulong[]
            {
                0, 1102
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.BWT
            new ulong[]
            {
                88649, 107474, 112049, 133499, 138074, 159674, 164624, 185249, 189974, 208724, 212849, 232049, 236549,
                241724, 255974, 256724, 265499, 267224, 269849, 271499, 274124, 279299
            },

            // report_audiocd.BWT
            new ulong[]
            {
                16398, 29900, 47799, 63163, 78774, 94581, 116974, 136015, 153921, 170600, 186388, 201648, 224298, 247072
            },

            // report_cdr.BWT
            new ulong[]
            {
                254264
            },

            // report_cdrom.BWT
            new ulong[]
            {
                254264
            },

            // report_cdrw.BWT
            new ulong[]
            {
                308223
            },

            // report_enhancedcd.BWT
            new ulong[]
            {
                15510, 33808, 51179, 71822, 87431, 103154, 117540, 136016, 153267, 166781, 186962, 201290, 222779,
                303315
            },

            // test_all_tracks_are_track1.BWT
            new ulong[]
            {
                25538, 62384
            },

            // test_audiocd_cdtext.BWT
            new ulong[]
            {
                29751, 65033, 78425, 95079, 126146, 154958, 191684, 222775, 243437, 269599, 277695
            },

            // test_castrated_leadout.BWT
            new ulong[]
            {
                29751, 65033, 78425, 95079, 126146, 154958, 191684, 222775, 243437, 269599, 269749
            },

            // test_data_track_as_audio.BWT
            new ulong[]
            {
                25538, 62384
            },

            // test_data_track_as_audio_fixed_sub.BWT
            new ulong[]
            {
                25538, 62384
            },

            // test_disc_starts_at_track2.BWT
            new ulong[]
            {
                25538, 62384
            },

            // test_enhancedcd.BWT
            new ulong[]
            {
                14254, 28952, 59205
            },

            // test_incd_udf200_finalized.BWT
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.BWT
            new ulong[]
            {
                1736, 32598, 52521, 70153, 99947, 119610, 136848, 155639, 175675, 206310, 226299, 244204, 273814,
                293601, 310560, 329157
            },

            // test_multiple_indexes.BWT
            new ulong[]
            {
                4653, 13724, 41034, 54838, 65535
            },

            // test_multisession.BWT
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_track2_inside_track1.BWT
            new ulong[]
            {
                13199, 25538, 62384
            },

            // test_videocd.BWT
            new ulong[]
            {
                1101, 48793
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150,
                150
            },

            // report_audiocd.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // report_cdr.BWT
            new ulong[]
            {
                0
            },

            // report_cdrom.BWT
            new ulong[]
            {
                0
            },

            // report_cdrw.BWT
            new ulong[]
            {
                0
            },

            // report_enhancedcd.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_all_tracks_are_track1.BWT
            new ulong[]
            {
                0, 150
            },

            // test_audiocd_cdtext.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_castrated_leadout.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_data_track_as_audio.BWT
            new ulong[]
            {
                0, 150
            },

            // test_data_track_as_audio_fixed_sub.BWT
            new ulong[]
            {
                0, 150
            },

            // test_disc_starts_at_track2.BWT
            new ulong[]
            {
                0, 150
            },

            // test_enhancedcd.BWT
            new ulong[]
            {
                0, 150, 150
            },

            // test_incd_udf200_finalized.BWT
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // test_multiple_indexes.BWT
            new ulong[]
            {
                0, 150, 150, 150, 150
            },

            // test_multisession.BWT
            new ulong[]
            {
                0, 150, 150, 150
            },

            // test_track2_inside_track1.BWT
            new ulong[]
            {
                0, 150, 150
            },

            // test_videocd.BWT
            new ulong[]
            {
                0, 150
            }
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.BWT
            new byte[]
            {
                0
            },

            // report_audiocd.BWT
            new byte[]
            {
                0
            },

            // report_cdr.BWT
            new byte[]
            {
                0
            },

            // report_cdrom.BWT
            new byte[]
            {
                0
            },

            // report_cdrw.BWT
            new byte[]
            {
                0
            },

            // report_enhancedcd.BWT
            new byte[]
            {
                0
            },

            // test_all_tracks_are_track1.BWT
            new byte[]
            {
                0
            },

            // test_audiocd_cdtext.BWT
            new byte[]
            {
                0
            },

            // test_castrated_leadout.BWT
            new byte[]
            {
                0
            },

            // test_data_track_as_audio.BWT
            new byte[]
            {
                0
            },

            // test_data_track_as_audio_fixed_sub.BWT
            new byte[]
            {
                0
            },

            // test_disc_starts_at_track2.BWT
            new byte[]
            {
                0
            },

            // test_enhancedcd.BWT
            new byte[]
            {
                0
            },

            // test_incd_udf200_finalized.BWT
            new byte[]
            {
                0
            },

            // test_multi_karaoke_sampler.BWT
            new byte[]
            {
                0
            },

            // test_multiple_indexes.BWT
            new byte[]
            {
                0
            },

            // test_multisession.BWT
            new byte[]
            {
                0
            },

            // test_track2_inside_track1.BWT
            new byte[]
            {
                0
            },

            // test_videocd.BWT
            new byte[]
            {
                0
            }
        };

        [Test]
        public void Test()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 4");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new DiscImages.BlindWrite4();
                Assert.AreEqual(true, images[i].Open(filters[i]), $"Open: {_testFiles[i]}");
            }

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_sectors[i], images[i].Info.Sectors, $"Sectors: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_mediaTypes[i], images[i].Info.MediaType, $"Media type: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_tracks[i], images[i].Tracks.Count, $"Tracks: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackSession).Should().
                          BeEquivalentTo(_trackSessions[i], $"Track session: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackStartSector).Should().
                          BeEquivalentTo(_trackStarts[i], $"Track start: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackEndSector).Should().
                          BeEquivalentTo(_trackEnds[i], $"Track end: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackPregap).Should().
                          BeEquivalentTo(_trackPregaps[i], $"Track pregap: {_testFiles[i]}");

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