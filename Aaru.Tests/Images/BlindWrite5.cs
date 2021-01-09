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
    public class BlindWrite5
    {
        readonly string[] _testFiles =
        {
            "dvdrom.B5T", "gigarec.B5T", "jaguarcd.B5T", "pcengine.B5T", "pcfx.B5T", "report_audiocd.B5T",
            "report_cdr.B5T", "report_cdrom.B5T", "report_cdrw_2x.B5T", "test_all_tracks_are_track1.B5T",
            "test_audiocd_cdtext.B5T", "test_castrated_leadout.B5T", "test_data_track_as_audio.B5T",
            "test_data_track_as_audio_fixed_sub.B5T", "test_disc_starts_at_track2.B5T", "test_enhancedcd.B5T",
            "test_incd_udf200_finalized.B5T", "test_multiple_indexes.B5T", "test_multisession.B5T",
            "test_track1_overlaps_session2.B5T", "test_track2_inside_session2_leadin.B5T",
            "test_track2_inside_track1.B5T", "test_videocd.B5T"
        };

        readonly ulong[] _sectors =
        {
            // dvdrom.B5T
            0,

            // gigarec.B5T
            469652,

            // jaguarcd.B5T
            243587,

            // pcengine.B5T
            160956,

            // pcfx.B5T
            246680,

            // report_audiocd.B5T
            247073,

            // report_cdr.B5T
            254265,

            // report_cdrom.B5T
            254265,

            // report_cdrw_2x.B5T
            308224,

            // test_all_tracks_are_track1.B5T
            0,

            // test_audiocd_cdtext.B5T
            277696,

            // test_castrated_leadout.B5T
            0,

            // test_data_track_as_audio.B5T
            62385,

            // test_data_track_as_audio_fixed_sub.B5T
            62385,

            // test_disc_starts_at_track2.B5T
            62385,

            // test_enhancedcd.B5T
            59206,

            // test_incd_udf200_finalized.B5T
            350134,

            // test_multiple_indexes.B5T
            65536,

            // test_multisession.B5T
            51168,

            // test_track1_overlaps_session2.B5T
            0,

            // test_track2_inside_session2_leadin.B5T
            62385,

            // test_track2_inside_track1.B5T
            0,

            // test_videocd.B5T
            48794
        };

        readonly MediaType[] _mediaTypes =
        {
            // dvdrom.B5T
            MediaType.DVDROM,

            // gigarec.B5T
            MediaType.CDR,

            // jaguarcd.B5T
            MediaType.CDDA,

            // pcengine.B5T
            MediaType.CD,

            // pcfx.B5T
            MediaType.CD,

            // report_audiocd.B5T
            MediaType.CDDA,

            // report_cdr.B5T
            MediaType.CDR,

            // report_cdrom.B5T
            MediaType.CDROM,

            // report_cdrw_2x.B5T
            MediaType.CDRW,

            // test_all_tracks_are_track1.B5T
            MediaType.CDR,

            // test_audiocd_cdtext.B5T
            MediaType.CDR,

            // test_castrated_leadout.B5T
            MediaType.CDR,

            // test_data_track_as_audio.B5T
            MediaType.CDR,

            // test_data_track_as_audio_fixed_sub.B5T
            MediaType.CDR,

            // test_disc_starts_at_track2.B5T
            MediaType.CDR,

            // test_enhancedcd.B5T
            MediaType.CDR,

            // test_incd_udf200_finalized.B5T
            MediaType.CDR,

            // test_multiple_indexes.B5T
            MediaType.CDR,

            // test_multisession.B5T
            MediaType.CDR,

            // test_track1_overlaps_session2.B5T
            MediaType.CDR,

            // test_track2_inside_session2_leadin.B5T
            MediaType.CDR,

            // test_track2_inside_track1.B5T
            MediaType.CDR,

            // test_videocd.B5T
            MediaType.CDR
        };

        readonly string[] _md5S =
        {
            // dvdrom.B5T
            "UNKNOWN",

            // gigarec.B5T
            "e2e967adc0e5c530964ac4eebe8cac47",

            // jaguarcd.B5T
            "3dd5bd0f7d95a40d411761d69255567a",

            // pcengine.B5T
            "4f5165069b3c5f11afe5f59711bd945d",

            // pcfx.B5T
            "c1bc8de499756453d1387542bb32bb4d",

            // report_audiocd.B5T
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdr.B5T
            "65e79ef740833188a0f5be19da14c09d",

            // report_cdrom.B5T
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw_2x.B5T
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // test_all_tracks_are_track1.B5T
            "UNKNOWN",

            // test_audiocd_cdtext.B5T
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_castrated_leadout.B5T
            "UNKNOWN",

            // test_data_track_as_audio.B5T
            "ce3d63e831b4e6191b05ec9ce452ad91",

            // test_data_track_as_audio_fixed_sub.B5T
            "ce3d63e831b4e6191b05ec9ce452ad91",

            // test_disc_starts_at_track2.B5T
            "25fb1b49726aaac09196ea56490beeb1",

            // test_enhancedcd.B5T
            "3736dbfcb7bf5648e3ac067379087001",

            // test_incd_udf200_finalized.B5T
            "901e4fe17ea6591b1fd53ba822428ef4",

            // test_multiple_indexes.B5T
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.B5T
            "e2e19cf38891e67a0829d01842b4052e",

            // test_track1_overlaps_session2.B5T
            "UNKNOWN",

            // test_track2_inside_session2_leadin.B5T
            "4e797aa5dedaac71a0e67ebd9ac9d555",

            // test_track2_inside_track1.B5T
            "UNKNOWN",

            // test_videocd.B5T
            "203a40d27b9bee018705c2df8d15e96d"
        };

        readonly string[] _longMd5S =
        {
            // dvdrom.B5T
            "UNKNOWN",

            // gigarec.B5T
            "1dc7801008110af6b8015aad64d91739",

            // jaguarcd.B5T
            "3dd5bd0f7d95a40d411761d69255567a",

            // pcengine.B5T
            "fd30db9486f67654179c90c8a5052edb",

            // pcfx.B5T
            "455ec326506d2c5b974c4617c1010796",

            // report_audiocd.B5T
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdr.B5T
            "47b32c32a6427ad1e6b4b1bd047df716",

            // report_cdrom.B5T
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw_2x.B5T
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // test_all_tracks_are_track1.B5T
            "UNKNOWN",

            // test_audiocd_cdtext.B5T
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_castrated_leadout.B5T
            "UNKNOWN",

            // test_data_track_as_audio.B5T
            "4bd5511229857ca167b45e607dea12dc",

            // test_data_track_as_audio_fixed_sub.B5T
            "4bd5511229857ca167b45e607dea12dc",

            // test_disc_starts_at_track2.B5T
            "8fd0dbe9085363cc20709f0ca76a373d",

            // test_enhancedcd.B5T
            "c2dfd5a32678c3ff049c143c98ad36a5",

            // test_incd_udf200_finalized.B5T
            "7b489457540c40037aabcf3f21e0201e",

            // test_multiple_indexes.B5T
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.B5T
            "3e646a04eb29a8e0ad892b6ac00ba962",

            // test_track1_overlaps_session2.B5T
            "UNKNOWN",

            // test_track2_inside_session2_leadin.B5T
            "311d641c93a3fe1dfae7deb3a2be28c7",

            // test_track2_inside_track1.B5T
            "UNKNOWN",

            // test_videocd.B5T
            "a686cade367db0a12fef1d9862f39e1d"
        };

        readonly string[] _subchannelMd5S =
        {
            // dvdrom.B5T
            null,

            // gigarec.B5T
            null,

            // jaguarcd.B5T
            null,

            // pcengine.B5T
            null,

            // pcfx.B5T
            null,

            // report_audiocd.B5T
            null,

            // report_cdr.B5T
            null,

            // report_cdrom.B5T
            null,

            // report_cdrw_2x.B5T
            null,

            // test_all_tracks_are_track1.B5T
            null,

            // test_audiocd_cdtext.B5T
            null,

            // test_castrated_leadout.B5T
            null,

            // test_data_track_as_audio.B5T
            null,

            // test_data_track_as_audio_fixed_sub.B5T
            null,

            // test_disc_starts_at_track2.B5T
            null,

            // test_enhancedcd.B5T
            null,

            // test_incd_udf200_finalized.B5T
            null,

            // test_multiple_indexes.B5T
            null,

            // test_multisession.B5T
            null,

            // test_track1_overlaps_session2.B5T
            null,

            // test_track2_inside_session2_leadin.B5T
            null,

            // test_track2_inside_track1.B5T
            null,

            // test_videocd.B5T
            null
        };

        readonly int[] _tracks =
        {
            // dvdrom.B5T
            1,

            // gigarec.B5T
            1,

            // jaguarcd.B5T
            11,

            // pcengine.B5T
            16,

            // pcfx.B5T
            8,

            // report_audiocd.B5T
            14,

            // report_cdr.B5T
            1,

            // report_cdrom.B5T
            1,

            // report_cdrw_2x.B5T
            1,

            // test_all_tracks_are_track1.B5T
            2,

            // test_audiocd_cdtext.B5T
            11,

            // test_castrated_leadout.B5T
            11,

            // test_data_track_as_audio.B5T
            2,

            // test_data_track_as_audio_fixed_sub.B5T
            2,

            // test_disc_starts_at_track2.B5T
            2,

            // test_enhancedcd.B5T
            3,

            // test_incd_udf200_finalized.B5T
            1,

            // test_multiple_indexes.B5T
            5,

            // test_multisession.B5T
            4,

            // test_track1_overlaps_session2.B5T
            2,

            // test_track2_inside_session2_leadin.B5T
            3,

            // test_track2_inside_track1.B5T
            3,

            // test_videocd.B5T
            2
        };

        readonly int[][] _trackSessions =
        {
            // dvdrom.B5T
            new[]
            {
                1
            },

            // gigarec.B5T
            new[]
            {
                1
            },

            // jaguarcd.B5T
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // pcengine.B5T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.B5T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.B5T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.B5T
            new[]
            {
                1
            },

            // report_cdrom.B5T
            new[]
            {
                1
            },

            // report_cdrw_2x.B5T
            new[]
            {
                1
            },

            // test_all_tracks_are_track1.B5T
            new[]
            {
                1, 2
            },

            // test_audiocd_cdtext.B5T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_castrated_leadout.B5T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_data_track_as_audio.B5T
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio_fixed_sub.B5T
            new[]
            {
                1, 2
            },

            // test_disc_starts_at_track2.B5T
            new[]
            {
                1, 2
            },

            // test_enhancedcd.B5T
            new[]
            {
                1, 1, 2
            },

            // test_incd_udf200_finalized.B5T
            new[]
            {
                1
            },

            // test_multiple_indexes.B5T
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.B5T
            new[]
            {
                1, 2, 3, 4
            },

            // test_track1_overlaps_session2.B5T
            new[]
            {
                1
            },

            // test_track2_inside_session2_leadin.B5T
            new[]
            {
                1, 1, 2
            },

            // test_track2_inside_track1.B5T
            new[]
            {
                1, 1, 1
            },

            // test_videocd.B5T
            new[]
            {
                1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // dvdrom.B5T
            new ulong[]
            {
                0
            },

            // gigarec.B5T
            new ulong[]
            {
                0
            },

            // jaguarcd.B5T
            new ulong[]
            {
                0, 27490, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.B5T
            new ulong[]
            {
                0, 3590, 38464, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // pcfx.B5T
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220645, 225646, 235498
            },

            // report_audiocd.B5T
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdr.B5T
            new ulong[]
            {
                0
            },

            // report_cdrom.B5T
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.B5T
            new ulong[]
            {
                0
            },

            // test_all_tracks_are_track1.B5T
            new ulong[]
            {
                0, 25539
            },

            // test_audiocd_cdtext.B5T
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_castrated_leadout.B5T
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_data_track_as_audio.B5T
            new ulong[]
            {
                0, 36789
            },

            // test_data_track_as_audio_fixed_sub.B5T
            new ulong[]
            {
                0, 36789
            },

            // test_disc_starts_at_track2.B5T
            new ulong[]
            {
                0, 36789
            },

            // test_enhancedcd.B5T
            new ulong[]
            {
                0, 14405, 40203
            },

            // test_incd_udf200_finalized.B5T
            new ulong[]
            {
                0
            },

            // test_multiple_indexes.B5T
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession.B5T
            new ulong[]
            {
                0, 19383, 32710, 45228
            },

            // test_track1_overlaps_session2.B5T
            new ulong[]
            {
                0
            },

            // test_track2_inside_session2_leadin.B5T
            new ulong[]
            {
                0, 25500, 36789
            },

            // test_track2_inside_track1.B5T
            new ulong[]
            {
                0, 13350, 36939
            },

            // test_videocd.B5T
            new ulong[]
            {
                0, 1252
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // dvdrom.B5T
            new ulong[]
            {
                0
            },

            // gigarec.B5T
            new ulong[]
            {
                469651
            },

            // jaguarcd.B5T
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.B5T
            new ulong[]
            {
                3589, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },

            // pcfx.B5T
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },

            // report_audiocd.B5T
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdr.B5T
            new ulong[]
            {
                254264
            },

            // report_cdrom.B5T
            new ulong[]
            {
                254264
            },

            // report_cdrw_2x.B5T
            new ulong[]
            {
                308223
            },

            // test_all_tracks_are_track1.B5T
            new ulong[]
            {
                25538, 51077
            },

            // test_audiocd_cdtext.B5T
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_castrated_leadout.B5T
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 1049
            },

            // test_data_track_as_audio.B5T
            new ulong[]
            {
                25538, 62384
            },

            // test_data_track_as_audio_fixed_sub.B5T
            new ulong[]
            {
                25538, 62384
            },

            // test_disc_starts_at_track2.B5T
            new ulong[]
            {
                25538, 62384
            },

            // test_enhancedcd.B5T
            new ulong[]
            {
                14404, 28952, 59205
            },

            // test_incd_udf200_finalized.B5T
            new ulong[]
            {
                350133
            },

            // test_multiple_indexes.B5T
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.B5T
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_track1_overlaps_session2.B5T
            new ulong[]
            {
                0
            },

            // test_track2_inside_session2_leadin.B5T
            new ulong[]
            {
                25499, 25538, 62384
            },

            // test_track2_inside_track1.B5T
            new ulong[]
            {
                13349, 25538, 62384
            },

            // test_videocd.B5T
            new ulong[]
            {
                1251, 48793
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // dvdrom.B5T
            new ulong[]
            {
                0
            },

            // gigarec.B5T
            new ulong[]
            {
                150
            },

            // jaguarcd.B5T
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.B5T
            new ulong[]
            {
                150, 0, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.B5T
            new ulong[]
            {
                150, 0, 0, 0, 0, 150, 0, 0
            },

            // report_audiocd.B5T
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.B5T
            new ulong[]
            {
                150
            },

            // report_cdrom.B5T
            new ulong[]
            {
                150
            },

            // report_cdrw_2x.B5T
            new ulong[]
            {
                150
            },

            // test_all_tracks_are_track1.B5T
            new ulong[]
            {
                150, 150
            },

            // test_audiocd_cdtext.B5T
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_castrated_leadout.B5T
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_data_track_as_audio.B5T
            new ulong[]
            {
                150, 150
            },

            // test_data_track_as_audio_fixed_sub.B5T
            new ulong[]
            {
                150, 150
            },

            // test_disc_starts_at_track2.B5T
            new ulong[]
            {
                150, 150
            },

            // test_enhancedcd.B5T
            new ulong[]
            {
                150, 0, 150
            },

            // test_incd_udf200_finalized.B5T
            new ulong[]
            {
                150
            },

            // test_multiple_indexes.B5T
            new ulong[]
            {
                150, 0, 0, 0, 0
            },

            // test_multisession.B5T
            new ulong[]
            {
                150, 150, 150, 150
            },

            // test_track1_overlaps_session2.B5T
            new ulong[]
            {
                150
            },

            // test_track2_inside_session2_leadin.B5T
            new ulong[]
            {
                150, 0, 150
            },

            // test_track2_inside_track1.B5T
            new ulong[]
            {
                150, 0, 150
            },

            // test_videocd.B5T
            new ulong[]
            {
                150, 0
            }
        };

        readonly byte[][] _trackFlags =
        {
            // dvdrom.B5T
            null,

            // gigarec.B5T
            new byte[]
            {
                4
            },

            // jaguarcd.B5T
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.B5T
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.B5T
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // report_audiocd.B5T
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.B5T
            new byte[]
            {
                4
            },

            // report_cdrom.B5T
            new byte[]
            {
                4
            },

            // report_cdrw_2x.B5T
            new byte[]
            {
                4
            },

            // test_all_tracks_are_track1.B5T
            new byte[]
            {
                4, 4
            },

            // test_audiocd_cdtext.B5T
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_castrated_leadout.B5T
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_data_track_as_audio.B5T
            new byte[]
            {
                4, 2
            },

            // test_data_track_as_audio_fixed_sub.B5T
            new byte[]
            {
                4, 2
            },

            // test_disc_starts_at_track2.B5T
            new byte[]
            {
                4, 4
            },

            // test_enhancedcd.B5T
            new byte[]
            {
                0, 0, 4
            },

            // test_incd_udf200_finalized.B5T
            new byte[]
            {
                7
            },

            // test_multiple_indexes.B5T
            new byte[]
            {
                2, 0, 0, 8, 1
            },

            // test_multisession.B5T
            new byte[]
            {
                4, 4, 4, 4
            },

            // test_track1_overlaps_session2.B5T
            new byte[]
            {
                4, 4
            },

            // test_track2_inside_session2_leadin.B5T
            new byte[]
            {
                4, 4, 4
            },

            // test_track2_inside_track1.B5T
            new byte[]
            {
                4, 4, 4
            },

            // test_videocd.B5T
            new byte[]
            {
                4, 4
            }
        };

        [Test]
        public void Test()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 5");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new DiscImages.BlindWrite5();
                System.Console.WriteLine(_testFiles[i]);
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