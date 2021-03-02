// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CloneCD.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class CloneCD : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "cdiready_theapprentice.ccd", "jaguarcd.ccd", "pcengine.ccd", "pcfx.ccd", "report_audiocd.ccd",
            "report_cdrom.ccd", "report_cdrw_2x.ccd", "report_enhancedcd.ccd", "test_audiocd_cdtext.ccd",
            "test_castrated_leadout.ccd", "test_disc_starts_at_track2.ccd", "test_data_track_as_audio_fixed_sub.ccd",
            "test_data_track_as_audio.ccd", "test_enhancedcd.ccd", "test_incd_udf200_finalized.ccd",
            "test_karaoke_multi_sampler.ccd", "test_multiple_indexes.ccd", "test_multisession.ccd",
            "test_track0_in_session2.ccd", "test_track111_in_session2_fixed_sub.ccd", "test_track1-2-9_fixed_sub.ccd",
            "test_track1-2-9.ccd", "test_track1_overlaps_session2.ccd", "test_track2_inside_leadout.ccd",
            "test_track2_inside_session2_leadin.ccd", "test_track2_inside_track1.ccd", "test_videocd.ccd"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // cdiready_the_apprentice.ccd
            279300,

            // jaguarcd.ccd
            243587,

            // pcengine.ccd
            160956,

            // pcfx.ccd
            246680,

            // report_audiocd.ccd
            247073,

            // report_cdrom.ccd
            254265,

            // report_cdrw_2x.ccd
            308224,

            // report_enhancedcd.ccd
            303316,

            // test_audiocd_cdtext.ccd
            277696,

            // test_castrated_leadout.ccd
            1050,

            // test_disc_starts_at_track2.ccd
            62385,

            // test_data_track_as_audio_fixed_sub.ccd
            62385,

            // test_data_track_as_audio.ccd
            62385,

            // test_enhancedcd.ccd
            59206,

            // test_incd_udf200_finalized.ccd
            350134,

            // test_karaoke_multi_sampler.ccd
            329158,

            // test_multiple_indexes.ccd
            65536,

            // test_multisession.ccd
            51168,

            // test_track0_in_session2.ccd
            36939,

            // test_track111_in_session2_fixed_sub.ccd
            36939,

            // test_track1-2-9_fixed_sub.ccd
            25539,

            // test_track1_2_9.ccd
            25539,

            // test_track1_overlaps_session2.ccd
            25539,

            // test_track2_inside_leadout.ccd
            25539,

            // test_track2_inside_session2_leadin.ccd
            62385,

            // test_track2_inside_track1.ccd
            62385,

            // test_videocd.ccd
            48794
        };
        public override uint[] _sectorSize => null;

        public override MediaType[] _mediaTypes => new[]
        {
            // cdiready_the_apprentice.ccd
            MediaType.CDDA,

            // jaguarcd.ccd
            MediaType.CDDA,

            // pcengine.ccd
            MediaType.CD,

            // pcfx.ccd
            MediaType.CD,

            // report_audiocd.ccd
            MediaType.CDDA,

            // report_cdrom.ccd
            MediaType.CDROM,

            // report_cdrw_2x.ccd
            MediaType.CDROM,

            // report_enhancedcd.ccd
            MediaType.CDROMXA,

            // test_audiocd_cdtext.ccd
            MediaType.CDDA,

            // test_castrated_leadout.ccd
            MediaType.CDDA,

            // test_disc_starts_at_track2.ccd
            MediaType.CDROMXA,

            // test_data_track_as_audio_fixed_sub.ccd
            MediaType.CDROMXA,

            // test_data_track_as_audio.ccd
            MediaType.CDROMXA,

            // test_enhancedcd.ccd
            MediaType.CDROMXA,

            // test_incd_udf200_finalized.ccd
            MediaType.CDROMXA,

            // test_karaoke_multi_sampler.ccd
            MediaType.CDROMXA,

            // test_multiple_indexes.ccd
            MediaType.CDDA,

            // test_multisession.ccd
            MediaType.CDROMXA,

            // test_track0_in_session2.ccd
            MediaType.CDROMXA,

            // test_track111_in_session2_fixed_sub.ccd
            MediaType.CDROMXA,

            // test_track1-2-9_fixed_sub.ccd
            MediaType.CDROMXA,

            // test_track1_2_9.ccd
            MediaType.CDROMXA,

            // test_track1_overlaps_session2.ccd
            MediaType.CDROM,

            // test_track2_inside_leadout.ccd
            MediaType.CDROMXA,

            // test_track2_inside_session2_leadin.ccd
            MediaType.CDROMXA,

            // test_track2_inside_track1.ccd
            MediaType.CDROMXA,

            // test_videocd.ccd
            MediaType.CDROMXA
        };

        public override string[] _md5S => new[]
        {
            // cdiready_the_apprentice.ccd
            "UNKNOWN",

            // jaguarcd.ccd
            "3147ff203341692813de8e5775f45d84",

            // pcengine.ccd
            "127b0a92b00ea9a67df1ed8c80daadc7",

            // pcfx.ccd
            "9d538bd1ee1db068685ed59d29185941",

            // report_audiocd.ccd
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdrom.ccd
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw_2x.ccd
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_enhancedcd.ccd
            "588d8ff1fef693bbe5719ac6c2f96bc1",

            // test_audiocd_cdtext.ccd
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_castrated_leadout.ccd
            "UNKNOWN",

            // test_disc_starts_at_track2.ccd
            "6fa06c10561343438736a8d3d9a965ea",

            // test_data_track_as_audio_fixed_sub.ccd
            "d9d46cae2a3a46316c8e1411e84d40ef",

            // test_data_track_as_audio.ccd
            "d9d46cae2a3a46316c8e1411e84d40ef",

            // test_enhancedcd.ccd
            "5984f395dccd4d1e10df0f92d54d872d",

            // test_incd_udf200_finalized.ccd
            "f95d6f978ddb4f98bbffda403f627fe1",

            // test_karaoke_multi_sampler.ccd
            "9a19aa0df066732a8ec34025e8160248",

            // test_multiple_indexes.ccd
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.ccd
            "f793fecc486a83cbe05b51c2d98059b9",

            // test_track0_in_session2.ccd
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track111_in_session2_fixed_sub.ccd
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track1-2-9_fixed_sub.ccd
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track1_2_9.ccd
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track1_overlaps_session2.ccd
            "UNKNOWN",

            // test_track2_inside_leadout.ccd
            "UNKNOWN",

            // test_track2_inside_session2_leadin.ccd
            "6fa06c10561343438736a8d3d9a965ea",

            // test_track2_inside_track1.ccd
            "6fa06c10561343438736a8d3d9a965ea",

            // test_videocd.ccd
            "b640eed2eba209ebba4e6cd3171883a4"
        };

        public override string[] _longMd5S => new[]
        {
            // cdiready_the_apprentice.ccd
            "UNKNOWN",

            // jaguarcd.ccd
            "3147ff203341692813de8e5775f45d84",

            // pcengine.ccd
            "6ead3bdedb374f7b9bdf24773d30e491",

            // pcfx.ccd
            "76f4bd63c13db3e44fbf7acda20f49e2",

            // report_audiocd.ccd
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdrom.ccd
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw_2x.ccd
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_enhancedcd.ccd
            "d72e737f49482d1330e8fe03b9f40b79",

            // test_audiocd_cdtext.ccd
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_castrated_leadout.ccd
            "UNKNOWN",

            // test_disc_starts_at_track2.ccd
            "c82d20702d31bc15bdc91f7e107862ae",

            // test_data_track_as_audio_fixed_sub.ccd
            "b3550e61649ba5276fed8d74f8e512ee",

            // test_data_track_as_audio.ccd
            "b3550e61649ba5276fed8d74f8e512ee",

            // test_enhancedcd.ccd
            "df8f4b8b58b9cada80ee442ddbd690f4",

            // test_incd_udf200_finalized.ccd
            "6751e0ae7821f92221672b1cd5a1ff36",

            // test_karaoke_multi_sampler.ccd
            "e981f7dfdb522ba937fe75474e23a446",

            // test_multiple_indexes.ccd
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.ccd
            "199b85a01c27f55f463fc7d606adfafa",

            // test_track0_in_session2.ccd
            "3b3172070738044417ae5284195acbfd",

            // test_track111_in_session2_fixed_sub.ccd
            "396f86cdd8bfb012b68eabd5a94f604b",

            // test_track1-2-9_fixed_sub.ccd
            "649047a018bc6c1ba667397049eae888",

            // test_track1_2_9.ccd
            "3b3172070738044417ae5284195acbfd",

            // test_track1_overlaps_session2.ccd
            "UNKNOWN",

            // test_track2_inside_leadout.ccd
            "UNKNOWN",

            // test_track2_inside_session2_leadin.ccd
            "608a73cd10bccdadde68523aead1ee72",

            // test_track2_inside_track1.ccd
            "450fe640a58c0bc1fc9cd6e779884d2c",

            // test_videocd.ccd
            "a1194d29dfb4e207eabf6208f908a213"
        };

        public override string[] _subchannelMd5S => new[]
        {
            // cdiready_the_apprentice.ccd
            "UNKNOWN",

            // jaguarcd.ccd
            "0534d96336d9fb46f2c48c9c27f07999",

            // pcengine.ccd
            "315ee5ebb36969b4ce0fb0162f7a9932",

            // pcfx.ccd
            "d9804e5f919ffb1531832049df8f0165",

            // report_audiocd.ccd
            "b744ddaf1d4ebd3bd0b96a160f55637d",

            // report_cdrom.ccd
            "c5ae648d586e55afd1108294c9b86ca6",

            // report_cdrw_2x.ccd
            "c73559a91abd57f732c7ea609fef547a",

            // report_enhancedcd.ccd
            "3d4bab2b1bf8f9a373e35f4e743aa883",

            // test_audiocd_cdtext.ccd
            "2a2918ad19f5bf1b6e52b57e40fe47eb",

            // test_castrated_leadout.ccd
            "UNKNOWN",

            // test_disc_starts_at_track2.ccd
            "976f4684da623c64acee464e9dca046e",

            // test_data_track_as_audio_fixed_sub.ccd
            "a53aba8a0fdb038ef67e68ba009aa5b1",

            // test_data_track_as_audio.ccd
            "a53aba8a0fdb038ef67e68ba009aa5b1",

            // test_enhancedcd.ccd
            "a80b7b6b704cf7fe94942df281d4588a",

            // test_incd_udf200_finalized.ccd
            "569c87cdc115f2d02b2268fc2b4d8b11",

            // test_karaoke_multi_sampler.ccd
            "c48c09b8c7c4af99de1cf97faaef32fc",

            // test_multiple_indexes.ccd
            "d374e82dfcbc4515c09a9a6e5955bf1d",

            // test_multisession.ccd
            "7e9326e1de734f00ba71d5a8100d0cda",

            // test_track0_in_session2.ccd
            "7eedb60edb3dc77eac41fd8f2214dfb8",

            // test_track111_in_session2_fixed_sub.ccd
            "c81a161af0fcd01dfd340290178a32fd",

            // test_track1-2-9_fixed_sub.ccd
            "5fcfa6ec0511a0b5c73a817a456d5412",

            // test_track1_2_9.ccd
            "5fcfa6ec0511a0b5c73a817a456d5412",

            // test_track1_overlaps_session2.ccd
            "UNKNOWN",

            // test_track2_inside_leadout.ccd
            "UNKNOWN",

            // test_track2_inside_session2_leadin.ccd
            "933f1699ba88a70aff5062f9626ef529",

            // test_track2_inside_track1.ccd
            "1bf8af738f8dddb7142b308c245d05f5",

            // test_videocd.ccd
            "712725733e44be46e55f16569659fd07"
        };

        public override int[] _tracks => new[]
        {
            // cdiready_the_apprentice.ccd
            22,

            // jaguarcd.ccd
            11,

            // pcengine.ccd
            16,

            // pcfx.ccd
            8,

            // report_audiocd.ccd
            14,

            // report_cdrom.ccd
            1,

            // report_cdrw_2x.ccd
            1,

            // report_enhancedcd.ccd
            14,

            // test_audiocd_cdtext.ccd
            11,

            // test_castrated_leadout.ccd
            11,

            // test_disc_starts_at_track2.ccd
            2,

            // test_data_track_as_audio_fixed_sub.ccd
            2,

            // test_data_track_as_audio.ccd
            2,

            // test_enhancedcd.ccd
            3,

            // test_incd_udf200_finalized.ccd
            1,

            // test_karaoke_multi_sampler.ccd
            16,

            // test_multiple_indexes.ccd
            5,

            // test_multisession.ccd
            4,

            // test_track0_in_session2.ccd
            2,

            // test_track111_in_session2_fixed_sub.ccd
            2,

            // test_track1-2-9_fixed_sub.ccd
            2,

            // test_track1_2_9.ccd
            2,

            // test_track1_overlaps_session2.ccd
            1,

            // test_track2_inside_leadout.ccd
            2,

            // test_track2_inside_session2_leadin.ccd
            3,

            // test_track2_inside_track1.ccd
            3,

            // test_videocd.ccd
            2
        };

        public override int[][] _trackSessions => new[]
        {
            // cdiready_the_apprentice.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // jaguarcd.ccd
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // pcengine.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.ccd
            new[]
            {
                1
            },

            // report_cdrw_2x.ccd
            new[]
            {
                1
            },

            // report_enhancedcd.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_audiocd_cdtext.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_castrated_leadout.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_disc_starts_at_track2.ccd
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio_fixed_sub.ccd
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio.ccd
            new[]
            {
                1, 2
            },

            // test_enhancedcd.ccd
            new[]
            {
                1, 1, 2
            },

            // test_incd_udf200_finalized.ccd
            new[]
            {
                1
            },

            // test_karaoke_multi_sampler.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.ccd
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.ccd
            new[]
            {
                1, 2, 3, 4
            },

            // test_track0_in_session2.ccd
            new[]
            {
                1, 1
            },

            // test_track111_in_session2_fixed_sub.ccd
            new[]
            {
                1, 1
            },

            // test_track1-2-9_fixed_sub.ccd
            new[]
            {
                1, 1
            },

            // test_track1_2_9.ccd
            new[]
            {
                1, 1
            },

            // test_track1_overlaps_session2.ccd
            new[]
            {
                1
            },

            // test_track2_inside_leadout.ccd
            new[]
            {
                1, 1
            },

            // test_track2_inside_session2_leadin.ccd
            new[]
            {
                1, 1, 2
            },

            // test_track2_inside_track1.ccd
            new[]
            {
                1, 1, 2
            },

            // test_videocd.ccd
            new[]
            {
                1, 1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // cdiready_the_apprentice.ccd
            new ulong[]
            {
                69150, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // jaguarcd.ccd
            new ulong[]
            {
                0, 27640, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.ccd
            new ulong[]
            {
                0, 3590, 38614, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // pcfx.ccd
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220795, 225646, 235498
            },

            // report_audiocd.ccd
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdrom.ccd
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.ccd
            new ulong[]
            {
                0
            },

            // report_enhancedcd.ccd
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234180
            },

            // test_audiocd_cdtext.ccd
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_castrated_leadout.ccd
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_disc_starts_at_track2.ccd
            new ulong[]
            {
                0, 36939
            },

            // test_data_track_as_audio_fixed_sub.ccd
            new ulong[]
            {
                0, 36939
            },

            // test_data_track_as_audio.ccd
            new ulong[]
            {
                0, 36939
            },

            // test_enhancedcd.ccd
            new ulong[]
            {
                0, 14405, 40353
            },

            // test_incd_udf200_finalized.ccd
            new ulong[]
            {
                0
            },

            // test_karaoke_multi_sampler.ccd
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multiple_indexes.ccd
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession.ccd
            new ulong[]
            {
                0, 19533, 32860, 45378
            },

            // test_track0_in_session2.ccd
            new ulong[]
            {
                0, 36939
            },

            // test_track111_in_session2_fixed_sub.ccd
            new ulong[]
            {
                0, 36939
            },

            // test_track1-2-9_fixed_sub.ccd
            new ulong[]
            {
                0, 13350
            },

            // test_track1_2_9.ccd
            new ulong[]
            {
                0, 13350
            },

            // test_track1_overlaps_session2.ccd
            new ulong[]
            {
                113870
            },

            // test_track2_inside_leadout.ccd
            new ulong[]
            {
                0, 62385
            },

            // test_track2_inside_session2_leadin.ccd
            new ulong[]
            {
                0, 25500, 36939
            },

            // test_track2_inside_track1.ccd
            new ulong[]
            {
                0, 13350, 36939
            },

            // test_videocd.ccd
            new ulong[]
            {
                0, 1252
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // cdiready_the_apprentice.ccd
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279299
            },

            // jaguarcd.ccd
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.ccd
            new ulong[]
            {
                3589, 38613, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },

            // pcfx.ccd
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220794, 225645, 235497, 246679
            },

            // report_audiocd.ccd
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdrom.ccd
            new ulong[]
            {
                254264
            },

            // report_cdrw_2x.ccd
            new ulong[]
            {
                308223
            },

            // report_enhancedcd.ccd
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // test_audiocd_cdtext.ccd
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_castrated_leadout.ccd
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 1049
            },

            // test_disc_starts_at_track2.ccd
            new ulong[]
            {
                25538, 62384
            },

            // test_data_track_as_audio_fixed_sub.ccd
            new ulong[]
            {
                25538, 62384
            },

            // test_data_track_as_audio.ccd
            new ulong[]
            {
                25538, 62384
            },

            // test_enhancedcd.ccd
            new ulong[]
            {
                14404, 28952, 59205
            },

            // test_incd_udf200_finalized.ccd
            new ulong[]
            {
                350133
            },

            // test_karaoke_multi_sampler.ccd
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multiple_indexes.ccd
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.ccd
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_track0_in_session2.ccd
            new ulong[]
            {
                36938, 36938
            },

            // test_track111_in_session2_fixed_sub.ccd
            new ulong[]
            {
                36938, 36938
            },

            // test_track1-2-9_fixed_sub.ccd
            new ulong[]
            {
                13349, 25538
            },

            // test_track1_2_9.ccd
            new ulong[]
            {
                13349, 25538
            },

            // test_track1_overlaps_session2.ccd
            new ulong[]
            {
                25538
            },

            // test_track2_inside_leadout.ccd
            new ulong[]
            {
                62384, 25538
            },

            // test_track2_inside_session2_leadin.ccd
            new ulong[]
            {
                25499, 25538, 62384
            },

            // test_track2_inside_track1.ccd
            new ulong[]
            {
                13349, 25538, 62384
            },

            // test_videocd.ccd
            new ulong[]
            {
                1251, 48793
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // cdiready_the_apprentice.ccd
            new ulong[]
            {
                69300, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // jaguarcd.ccd
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.ccd
            new ulong[]
            {
                150
            },

            // report_cdrw_2x.ccd
            new ulong[]
            {
                150
            },

            // report_enhancedcd.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_audiocd_cdtext.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_castrated_leadout.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_disc_starts_at_track2.ccd
            new ulong[]
            {
                150, 150
            },

            // test_data_track_as_audio_fixed_sub.ccd
            new ulong[]
            {
                150, 150
            },

            // test_data_track_as_audio.ccd
            new ulong[]
            {
                150, 150
            },

            // test_enhancedcd.ccd
            new ulong[]
            {
                150, 0, 150
            },

            // test_incd_udf200_finalized.ccd
            new ulong[]
            {
                150
            },

            // test_karaoke_multi_sampler.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0
            },

            // test_multisession.ccd
            new ulong[]
            {
                150, 150, 150, 150
            },

            // test_track0_in_session2.ccd
            new ulong[]
            {
                150, 0
            },

            // test_track111_in_session2_fixed_sub.ccd
            new ulong[]
            {
                150, 0
            },

            // test_track1-2-9_fixed_sub.ccd
            new ulong[]
            {
                150, 0
            },

            // test_track1_2_9.ccd
            new ulong[]
            {
                150, 0
            },

            // test_track1_overlaps_session2.ccd
            new ulong[]
            {
                150
            },

            // test_track2_inside_leadout.ccd
            new ulong[]
            {
                150, 0
            },

            // test_track2_inside_session2_leadin.ccd
            new ulong[]
            {
                150, 0, 150
            },

            // test_track2_inside_track1.ccd
            new ulong[]
            {
                150, 0, 150
            },

            // test_videocd.ccd
            new ulong[]
            {
                150, 0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // cdiready_the_apprentice.ccd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // jaguarcd.ccd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.ccd
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.ccd
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // report_audiocd.ccd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.ccd
            new byte[]
            {
                4
            },

            // report_cdrw_2x.ccd
            new byte[]
            {
                4
            },

            // report_enhancedcd.ccd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // test_audiocd_cdtext.ccd
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_castrated_leadout.ccd
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_disc_starts_at_track2.ccd
            new byte[]
            {
                4, 4
            },

            // test_data_track_as_audio_fixed_sub.ccd
            new byte[]
            {
                4, 2
            },

            // test_data_track_as_audio.ccd
            new byte[]
            {
                4, 2
            },

            // test_enhancedcd.ccd
            new byte[]
            {
                0, 0, 4
            },

            // test_incd_udf200_finalized.ccd
            new byte[]
            {
                7
            },

            // test_karaoke_multi_sampler.ccd
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.ccd
            new byte[]
            {
                2, 0, 0, 8, 1
            },

            // test_multisession.ccd
            new byte[]
            {
                4, 4, 4, 4
            },

            // test_track0_in_session2.ccd
            new byte[]
            {
                0, 0
            },

            // test_track111_in_session2_fixed_sub.ccd
            new byte[]
            {
                0, 0
            },

            // test_track1-2-9_fixed_sub.ccd
            new byte[]
            {
                0, 0
            },

            // test_track1_2_9.ccd
            new byte[]
            {
                0, 0
            },

            // test_track1_overlaps_session2.ccd
            new byte[]
            {
                0, 0
            },

            // test_track2_inside_leadout.ccd
            new byte[]
            {
                0, 0
            },

            // test_track2_inside_session2_leadin.ccd
            new byte[]
            {
                4, 4, 4
            },

            // test_track2_inside_track1.ccd
            new byte[]
            {
                0, 0, 0
            },

            // test_videocd.ccd
            new byte[]
            {
                0, 0
            }
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CloneCD");
        public override IMediaImage _plugin => new CloneCd();
    }
}