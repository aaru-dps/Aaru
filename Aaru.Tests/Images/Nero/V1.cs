// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol120.cs
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

namespace Aaru.Tests.Images.Nero
{
    [TestFixture]
    public class V1
    {
        readonly string[] _testFiles =
        {
             "cdiready_the_apprentice.nrg", "jaguarcd.nrg", "pcengine.nrg", "pcfx.nrg", "report_audiocd.nrg",
            "report_cdrom.nrg", "report_cdrw.nrg", "report_enhancedcd.nrg", "test_audiocd_cdtext.nrg",
            "test_data_track_as_audio.nrg", "test_incd_udf200_finalized.nrg", "test_multi_karaoke_sampler.nrg",
            "test_multiple_indexes.nrg", "test_multisession.nrg", "test_track2_inside_session2_leadin.nrg",
            "test_track2_inside_track1.nrg", "test_videocd.nrg",
                        
            "make_audiocd_dao.nrg",
            "make_audiocd_tao.nrg",
            "make_data_mode1_joliet_dao.nrg",
            "make_data_mode1_joliet_level2_dao.nrg",
            "make_data_mode1_joliet_level2_tao.nrg",
            "make_data_mode1_joliet_tao.nrg",
            "make_data_mode1_udf_dao.nrg",
            "make_data_mode1_udf_tao.nrg",
            "make_data_mode2_joliet_dao.nrg",
            "make_data_mode2_joliet_level2_dao.nrg",
            "make_data_mode2_joliet_level2_tao.nrg",
            "make_data_mode2_joliet_tao.nrg",
            "make_data_mode2_udf_dao.nrg",
            "make_data_mode2_udf_tao.nrg",
            "make_mixed_mode_dao.nrg",
            "make_mixed_mode_tao.nrg",
            "make_udf_dao.nrg",
            "make_udf_tao.nrg",
            
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.nrg
            279300,

            // jaguarcd.nrg
            243587,

            // pcengine.nrg
            160956,

            // pcfx.nrg
            246680,

            // report_audiocd.nrg
            247073,

            // report_cdrom.nrg
            254265,

            // report_cdrw.nrg
            308224,

            // report_enhancedcd.nrg
            303316,

            // test_audiocd_cdtext.nrg
            277696,

            // test_data_track_as_audio.nrg
            62385,

            // test_incd_udf200_finalized.nrg
            350134,

            // test_multi_karaoke_sampler.nrg
            329158,

            // test_multiple_indexes.nrg
            65536,

            // test_multisession.nrg
            51168,

            // test_track2_inside_session2_leadin.nrg
            62385,

            // test_track2_inside_track1.nrg
            62385,

            // test_videocd.nrg
            48794,
            // make_audiocd_dao.nrg
            279196,
            // make_audiocd_tao.nrg
            277696,
            // make_data_mode1_joliet_dao.nrg
            83078,
            // make_data_mode1_joliet_level2_dao.nrg
            83084,
            // make_data_mode1_joliet_level2_tao.nrg
            83084,
            // make_data_mode1_joliet_tao.nrg
            83078,
            // make_data_mode1_udf_dao.nrg
            85733,
            // make_data_mode1_udf_tao.nrg
            85733,
            // make_data_mode2_joliet_dao.nrg
            83092,
            // make_data_mode2_joliet_level2_dao.nrg
            83092,
            // make_data_mode2_joliet_level2_tao.nrg
            83092,
            // make_data_mode2_joliet_tao.nrg
            83092,
            // make_data_mode2_udf_dao.nrg
            85747,
            // make_data_mode2_udf_tao.nrg
            85747,
            // make_mixed_mode_dao.nrg
            325928,
            // make_mixed_mode_tao.nrg
            324278,
            // make_udf_dao.nrg
            84985,
            // make_udf_tao.nrg
            84985,
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.nrg
            MediaType.CDDA,

            // jaguarcd.nrg
            MediaType.CDDA,

            // pcengine.nrg
            MediaType.CD,

            // pcfx.nrg
            MediaType.CD,

            // report_audiocd.nrg
            MediaType.CDDA,

            // report_cdrom.nrg
            MediaType.CDROM,

            // report_cdrw.nrg
            MediaType.CDROM,

            // report_enhancedcd.nrg
            MediaType.CDPLUS,

            // test_audiocd_cdtext.nrg
            MediaType.CDDA,

            // test_data_track_as_audio.nrg
            MediaType.CDROMXA,

            // test_incd_udf200_finalized.nrg
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.nrg
            MediaType.CDROMXA,

            // test_multiple_indexes.nrg
            MediaType.CD,

            // test_multisession.nrg
            MediaType.CDROMXA,

            // test_track2_inside_session2_leadin.nrg
            MediaType.CDROMXA,

            // test_track2_inside_track1.nrg
            MediaType.CDROMXA,

            // test_videocd.nrg
            MediaType.CDROMXA,
            // make_audiocd_dao.nrg
            MediaType.CDDA,
            // make_audiocd_tao.nrg
            MediaType.CDDA,
            // make_data_mode1_joliet_dao.nrg
            MediaType.CDROM,
            // make_data_mode1_joliet_level2_dao.nrg
            MediaType.CDROM,
            // make_data_mode1_joliet_level2_tao.nrg
            MediaType.CDROM,
            // make_data_mode1_joliet_tao.nrg
            MediaType.CDROM,
            // make_data_mode1_udf_dao.nrg
            MediaType.CDROM,
            // make_data_mode1_udf_tao.nrg
            MediaType.CDROM,
            // make_data_mode2_joliet_dao.nrg
            MediaType.CDROMXA,
            // make_data_mode2_joliet_level2_dao.nrg
            MediaType.CDROMXA,
            // make_data_mode2_joliet_level2_tao.nrg
            MediaType.CDROMXA,
            // make_data_mode2_joliet_tao.nrg
            MediaType.CDROMXA,
            // make_data_mode2_udf_dao.nrg
            MediaType.CDROMXA,
            // make_data_mode2_udf_tao.nrg
            MediaType.CDROMXA,
            // make_mixed_mode_dao.nrg
            MediaType.CDROMXA,
            // make_mixed_mode_tao.nrg
            MediaType.CDROMXA,
            // make_udf_dao.nrg
            MediaType.CDROM,
            // make_udf_tao.nrg
            MediaType.CDROM,
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // jaguarcd.nrg
            "79ade978aad90667f272a693012c11ca",

            // pcengine.nrg
            "7119f623e909737e59732b935f103908",

            // pcfx.nrg
            "5a1ed6d71094e8e7ae53b6604a6fcc0a",

            // report_audiocd.nrg
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdrom.nrg
            "bf4bbec517101d0d6f45d2e4d50cb875",
            
            // report_cdrw.nrg
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_enhancedcd.nrg
            "dfd6c0bd02c19145b2a64d8a15912302",

            // test_audiocd_cdtext.nrg
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_data_track_as_audio.nrg
            "d9d46cae2a3a46316c8e1411e84d40ef",

            // test_incd_udf200_finalized.nrg
            "f95d6f978ddb4f98bbffda403f627fe1",

            // test_multi_karaoke_sampler.nrg
            "1731384a29149b7e6f4c0d0d07f178ca",

            // test_multiple_indexes.nrg
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.nrg
            "f793fecc486a83cbe05b51c2d98059b9",

            // test_track2_inside_session2_leadin.nrg
            "6fa06c10561343438736a8d3d9a965ea",

            // test_track2_inside_track1.nrg
            "6fa06c10561343438736a8d3d9a965ea",

            // test_videocd.nrg
            "ec7c86e6cfe5f965faa2488ae940e15a",
            // make_audiocd_dao.nrg
            "UNKNOWN",
            // make_audiocd_tao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_dao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_level2_dao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_level2_tao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_tao.nrg
            "UNKNOWN",
            // make_data_mode1_udf_dao.nrg
            "UNKNOWN",
            // make_data_mode1_udf_tao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_dao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_level2_dao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_level2_tao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_tao.nrg
            "UNKNOWN",
            // make_data_mode2_udf_dao.nrg
            "UNKNOWN",
            // make_data_mode2_udf_tao.nrg
            "UNKNOWN",
            // make_mixed_mode_dao.nrg
            "UNKNOWN",
            // make_mixed_mode_tao.nrg
            "UNKNOWN",
            // make_udf_dao.nrg
            "UNKNOWN",
            // make_udf_tao.nrg
            "UNKNOWN",
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // jaguarcd.nrg
            "8086a3654d6dede562621d24ae18729e",

            // pcengine.nrg
            "f1c1dbe1cd9df11fe2c1f0a97130c25f",

            // pcfx.nrg
            "dac5dc0961fa435da3c7d433477cda1a",

            // report_audiocd.nrg
            "ff35cfa013871b322ef54612e719c185",

            // report_cdrom.nrg
            "6b4e35ec371770751f26163629253015",

            // report_cdrw.nrg
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_enhancedcd.nrg
            "0038395e272242a29e84a1fb34a3a15e",

            // test_audiocd_cdtext.nrg
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_data_track_as_audio.nrg
            "b3550e61649ba5276fed8d74f8e512ee",

            // test_incd_udf200_finalized.nrg
            "6751e0ae7821f92221672b1cd5a1ff36",

            // test_multi_karaoke_sampler.nrg
            "efe2b3fe51022ef8e0a62587294d1d9c",

            // test_multiple_indexes.nrg
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.nrg
            "199b85a01c27f55f463fc7d606adfafa",

            // test_track2_inside_session2_leadin.nrg
            "608a73cd10bccdadde68523aead1ee72",

            // test_track2_inside_track1.nrg
            "c82d20702d31bc15bdc91f7e107862ae",

            // test_videocd.nrg
            "4a045788e69965efe0c87950d013e720",
            // make_audiocd_dao.nrg
            "UNKNOWN",
            // make_audiocd_tao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_dao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_level2_dao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_level2_tao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_tao.nrg
            "UNKNOWN",
            // make_data_mode1_udf_dao.nrg
            "UNKNOWN",
            // make_data_mode1_udf_tao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_dao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_level2_dao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_level2_tao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_tao.nrg
            "UNKNOWN",
            // make_data_mode2_udf_dao.nrg
            "UNKNOWN",
            // make_data_mode2_udf_tao.nrg
            "UNKNOWN",
            // make_mixed_mode_dao.nrg
            "UNKNOWN",
            // make_mixed_mode_tao.nrg
            "UNKNOWN",
            // make_udf_dao.nrg
            "UNKNOWN",
            // make_udf_tao.nrg
            "UNKNOWN",
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // jaguarcd.nrg
            "83ec1010fc44694d69dc48bacec5481a",

            // pcengine.nrg
            "9e9a6b51bc2e5ec67400cb33ad0ca33f",

            // pcfx.nrg
            "e3a0d78b6c32f5795b1b513bd13a6bda",

            // report_audiocd.nrg
            "9da6ad8f6f0cadd92509c10809da7296",

            // report_cdrom.nrg
            "1994c303674718c74b35f9a4ea1d3515",

            // report_cdrw.nrg
            "6fe81a972e750c68e08f6935e4d91e34",

            // report_enhancedcd.nrg
            "e6f7319532f46c3fa4fd3569c65546e1",

            // test_audiocd_cdtext.nrg
            "ca781a7afc4eb77c51f7c551ed45c03c",

            // test_data_track_as_audio.nrg
            "5479a1115bb6481db69fd6262e8c6076",

            // test_incd_udf200_finalized.nrg
            "65f938f7f9ac34fabd3ab94c14eb76b5",

            // test_multi_karaoke_sampler.nrg
            "f8c96f120cac18c52178b99ef4c4e2a9",

            // test_multiple_indexes.nrg
            "25bae9e30657e2f64a45e5f690e3ae9e",

            // test_multisession.nrg
            "48656afdbc40b6df06486a04a4d62401",

            // test_track2_inside_session2_leadin.nrg
            "933f1699ba88a70aff5062f9626ef529",

            // test_track2_inside_track1.nrg
            "d8eed571f137c92f22bb858d78fc1e41",

            // test_videocd.nrg
            "935a91f5850352818d92b71f1c87c393",
            // make_audiocd_dao.nrg
            "UNKNOWN",
            // make_audiocd_tao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_dao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_level2_dao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_level2_tao.nrg
            "UNKNOWN",
            // make_data_mode1_joliet_tao.nrg
            "UNKNOWN",
            // make_data_mode1_udf_dao.nrg
            "UNKNOWN",
            // make_data_mode1_udf_tao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_dao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_level2_dao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_level2_tao.nrg
            "UNKNOWN",
            // make_data_mode2_joliet_tao.nrg
            "UNKNOWN",
            // make_data_mode2_udf_dao.nrg
            "UNKNOWN",
            // make_data_mode2_udf_tao.nrg
            "UNKNOWN",
            // make_mixed_mode_dao.nrg
            "UNKNOWN",
            // make_mixed_mode_tao.nrg
            "UNKNOWN",
            // make_udf_dao.nrg
            "UNKNOWN",
            // make_udf_tao.nrg
            "UNKNOWN",
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.nrg
            22,

            // jaguarcd.nrg
            11,

            // pcengine.nrg
            16,

            // pcfx.nrg
            8,

            // report_audiocd.nrg
            14,

            // report_cdrom.nrg
            1,

            // report_cdrw.nrg
            1,

            // report_enhancedcd.nrg
            14,

            // test_audiocd_cdtext.nrg
            11,

            // test_data_track_as_audio.nrg
            2,

            // test_incd_udf200_finalized.nrg
            1,

            // test_multi_karaoke_sampler.nrg
            16,

            // test_multiple_indexes.nrg
            5,

            // test_multisession.nrg
            4,

            // test_track2_inside_session2_leadin.nrg
            3,

            // test_track2_inside_track1.nrg
            3,

            // test_videocd.nrg
            2,
            // make_audiocd_dao.nrg
            11,
            // make_audiocd_tao.nrg
            11,
            // make_data_mode1_joliet_dao.nrg
            1,
            // make_data_mode1_joliet_level2_dao.nrg
            1,
            // make_data_mode1_joliet_level2_tao.nrg
            1,
            // make_data_mode1_joliet_tao.nrg
            1,
            // make_data_mode1_udf_dao.nrg
            1,
            // make_data_mode1_udf_tao.nrg
            1,
            // make_data_mode2_joliet_dao.nrg
            1,
            // make_data_mode2_joliet_level2_dao.nrg
            1,
            // make_data_mode2_joliet_level2_tao.nrg
            1,
            // make_data_mode2_joliet_tao.nrg
            1,
            // make_data_mode2_udf_dao.nrg
            1,
            // make_data_mode2_udf_tao.nrg
            1,
            // make_mixed_mode_dao.nrg
            12,
            // make_mixed_mode_tao.nrg
            12,
            // make_udf_dao.nrg
            1,
            // make_udf_tao.nrg
            1,
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // jaguarcd.nrg
            new[]
            {
                // TODO: The image does not contain a second session, need to redump
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcengine.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.nrg
            new[]
            {
                1
            },

            // report_cdrw.nrg
            new[]
            {
                1
            },

            // report_enhancedcd.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_audiocd_cdtext.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_data_track_as_audio.nrg
            new[]
            {
                1, 2
            },

            // test_incd_udf200_finalized.nrg
            new[]
            {
                1
            },

            // test_multi_karaoke_sampler.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.nrg
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.nrg
            new[]
            {
                1, 2, 3, 4
            },

            // test_track2_inside_session2_leadin.nrg
            new[]
            {
                1, 1, 1
            },

            // test_track2_inside_track1.nrg
            new[]
            {
                1, 1, 2
            },

            // test_videocd.nrg
            new[]
            {
                1, 1
            },
            // make_audiocd_dao.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            // make_audiocd_tao.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            // make_data_mode1_joliet_dao.nrg
            new[]
            {
                1
            },
            // make_data_mode1_joliet_level2_dao.nrg
            new[]
            {
                1
            },
            // make_data_mode1_joliet_level2_tao.nrg
            new[]
            {
                1
            },
            // make_data_mode1_joliet_tao.nrg
            new[]
            {
                1
            },
            // make_data_mode1_udf_dao.nrg
            new[]
            {
                1
            },
            // make_data_mode1_udf_tao.nrg
            new[]
            {
                1
            },
            // make_data_mode2_joliet_dao.nrg
            new[]
            {
                1
            },
            // make_data_mode2_joliet_level2_dao.nrg
            new[]
            {
                1
            },
            // make_data_mode2_joliet_level2_tao.nrg
            new[]
            {
                1
            },
            // make_data_mode2_joliet_tao.nrg
            new[]
            {
                1
            },
            // make_data_mode2_udf_dao.nrg
            new[]
            {
                1
            },
            // make_data_mode2_udf_tao.nrg
            new[]
            {
                1
            },
            // make_mixed_mode_dao.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            // make_mixed_mode_tao.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            // make_udf_dao.nrg
            new[]
            {
                1
            },
            // make_udf_tao.nrg
            new[]
            {
                1
            },
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                69150, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // jaguarcd.nrg
            new ulong[]
            {
                 0, 27640, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.nrg
            new ulong[]
            {
                  0, 3590, 38614, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                  126229
            },

            // pcfx.nrg
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220795, 225646, 235498
            },

            // report_audiocd.nrg
            new ulong[]
            {
                 0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdrom.nrg
            new ulong[]
            {
                0
            },

            // report_cdrw.nrg
            new ulong[]
            {
                0
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                   0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234180
            },

            // test_audiocd_cdtext.nrg
            new ulong[]
            {
                 0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_data_track_as_audio.nrg
            new ulong[]
            {
                0, 36939
            },

            // test_incd_udf200_finalized.nrg
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965, 293752, 310711
            },

            // test_multiple_indexes.nrg
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession.nrg
            new ulong[]
            {
                0, 19533, 32860, 45378
            },

            // test_track2_inside_session2_leadin.nrg
            new ulong[]
            {
                0, 25500, 36939
            },

            // test_track2_inside_track1.nrg
            new ulong[]
            {
                0, 13350, 36939
            },

            // test_videocd.nrg
            new ulong[]
            {
                0, 1252
            },
            // make_audiocd_dao.nrg
            new ulong[]
            {
                0, 27454, 62934, 4428, 22432, 54833, 9459, 45087, 4360, 244938, 271250
            },
            // make_audiocd_tao.nrg
            new ulong[]
            {
                0, 27454, 62934, 4428, 22432, 54833, 9459, 45087, 4360, 244938, 271250
            },
            // make_data_mode1_joliet_dao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode1_joliet_level2_dao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode1_joliet_level2_tao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode1_joliet_tao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode1_udf_dao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode1_udf_tao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode2_joliet_dao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode2_joliet_level2_dao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode2_joliet_level2_tao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode2_joliet_tao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode2_udf_dao.nrg
            new ulong[]
            {
                0
            },
            // make_data_mode2_udf_tao.nrg
            new ulong[]
            {
                0
            },
            // make_mixed_mode_dao.nrg
            new ulong[]
            {
                0, 45382, 4586, 36450, 49960, 67964, 27229, 58575, 23403, 264817, 285629, 311941
            },
            // make_mixed_mode_tao.nrg
            new ulong[]
            {
                0, 45382, 4586, 36450, 49960, 67964, 27229, 58575, 23403, 264817, 285629, 311941
            },
            // make_udf_dao.nrg
            new ulong[]
            {
                0
            },
            // make_udf_tao.nrg
            new ulong[]
            {
                0
            },
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279299
            },

            // jaguarcd.nrg
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.nrg
            new ulong[]
            {
                3439, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269, 126078, 160955
            },

            // pcfx.nrg
            new ulong[]
            {
                4244, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },

            // report_audiocd.nrg
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdrom.nrg
            new ulong[]
            {
                254264
            },

            // report_cdrw.nrg
            new ulong[]
            {
                308223
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779, 303315
            },

            // test_audiocd_cdtext.nrg
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_data_track_as_audio.nrg
            new ulong[]
            {
                25538, 62384
            },

            // test_incd_udf200_finalized.nrg
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964, 293751, 310710, 329157
            },

            // test_multiple_indexes.nrg
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.nrg
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_track2_inside_session2_leadin.nrg
            new ulong[]
            {
                25499, 25538, 62384
            },

            // test_track2_inside_track1.nrg
            new ulong[]
            {
                13199, 25688, 62384
            },

            // test_videocd.nrg
            new ulong[]
            {
                1101, 48793
            },
            // make_audiocd_dao.nrg
            new ulong[]
            {
                29901, 63035, 76625, 21381, 53798, 83944, 46484, 76477, 25321, 271399, 279495
            },
            // make_audiocd_tao.nrg
            new ulong[]
            {
                29901, 63035, 76625, 21381, 53798, 83944, 46484, 76477, 25321, 271399, 279495
            },
            // make_data_mode1_joliet_dao.nrg
            new ulong[]
            {
                83077
            },
            // make_data_mode1_joliet_level2_dao.nrg
            new ulong[]
            {
                83083
            },
            // make_data_mode1_joliet_level2_tao.nrg
            new ulong[]
            {
                83083
            },
            // make_data_mode1_joliet_tao.nrg
            new ulong[]
            {
                83077
            },
            // make_data_mode1_udf_dao.nrg
            new ulong[]
            {
                85732
            },
            // make_data_mode1_udf_tao.nrg
            new ulong[]
            {
                85732
            },
            // make_data_mode2_joliet_dao.nrg
            new ulong[]
            {
                83091
            },
            // make_data_mode2_joliet_level2_dao.nrg
            new ulong[]
            {
                83091
            },
            // make_data_mode2_joliet_level2_tao.nrg
            new ulong[]
            {
                83091
            },
            // make_data_mode2_joliet_tao.nrg
            new ulong[]
            {
                83091
            },
            // make_data_mode2_udf_dao.nrg
            new ulong[]
            {
                85746
            },
            // make_data_mode2_udf_tao.nrg
            new ulong[]
            {
                85746
            },
            // make_mixed_mode_dao.nrg
            new ulong[]
            {
                46581, 75583, 40167, 50141, 66913, 99330, 56340, 95600, 53593, 285778, 312090, 320186
            },
            // make_mixed_mode_tao.nrg
            new ulong[]
            {
                                46581, 75583, 40167, 50141, 66913, 99330, 56340, 95600, 53593, 285778, 312090, 320186
            },
            // make_udf_dao.nrg
            new ulong[]
            {
                84984
            },
            // make_udf_tao.nrg
            new ulong[]
            {
                84984
            },
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                69300, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // jaguarcd.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.nrg
            new ulong[]
            {
                150, 150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // pcfx.nrg
            new ulong[]
            {
                150, 150, 0, 0, 0, 150, 0, 0
            },

            // report_audiocd.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.nrg
            new ulong[]
            {
                150
            },

            // report_cdrw.nrg
            new ulong[]
            {
                150
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_audiocd_cdtext.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_data_track_as_audio.nrg
            new ulong[]
            {
                150, 150
            },

            // test_incd_udf200_finalized.nrg
            new ulong[]
            {
                150
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0
            },

            // test_multisession.nrg
            new ulong[]
            {
                150, 150, 150, 150
            },

            // test_track2_inside_session2_leadin.nrg
            new ulong[]
            {
                150, 150, 150
            },

            // test_track2_inside_track1.nrg
            new ulong[]
            {
                150, 0, 150
            },

            // test_videocd.nrg
            new ulong[]
            {
                150, 150
            },
            // make_audiocd_dao.nrg
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },
            // make_audiocd_tao.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            // make_data_mode1_joliet_dao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode1_joliet_level2_dao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode1_joliet_level2_tao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode1_joliet_tao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode1_udf_dao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode1_udf_tao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode2_joliet_dao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode2_joliet_level2_dao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode2_joliet_level2_tao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode2_joliet_tao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode2_udf_dao.nrg
            new ulong[]
            {
                150
            },
            // make_data_mode2_udf_tao.nrg
            new ulong[]
            {
                150
            },
            // make_mixed_mode_dao.nrg
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150

            },
            // make_mixed_mode_tao.nrg
            new ulong[]
            {
                                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            // make_udf_dao.nrg
            new ulong[]
            {
                150
            },
            // make_udf_tao.nrg
            new ulong[]
            {
                150
            },
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // jaguarcd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.nrg
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.nrg
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // report_audiocd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.nrg
            new byte[]
            {
                4
            },

            // report_cdrw.nrg
            new byte[]
            {
                4
            },

            // report_enhancedcd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // test_audiocd_cdtext.nrg
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_data_track_as_audio.nrg
            new byte[]
            {
                4, 2
            },

            // test_incd_udf200_finalized.nrg
            new byte[]
            {
                7
            },

            // test_multi_karaoke_sampler.nrg
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.nrg
            new byte[]
            {
                2, 0, 0, 8, 1
            },

            // test_multisession.nrg
            new byte[]
            {
                4, 4, 4, 4
            },

            // test_track2_inside_session2_leadin.nrg
            new byte[]
            {
                4, 4, 4
            },

            // test_track2_inside_track1.nrg
            new byte[]
            {
                4, 4, 4
            },

            // test_videocd.nrg
            new byte[]
            {
                4, 4
            },
            // make_audiocd_dao.nrg
            new byte[]
            {
                4
            },
            // make_audiocd_tao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode1_joliet_dao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode1_joliet_level2_dao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode1_joliet_level2_tao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode1_joliet_tao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode1_udf_dao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode1_udf_tao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode2_joliet_dao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode2_joliet_level2_dao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode2_joliet_level2_tao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode2_joliet_tao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode2_udf_dao.nrg
            new byte[]
            {
                4
            },
            // make_data_mode2_udf_tao.nrg
            new byte[]
            {
                4
            },
            // make_mixed_mode_dao.nrg
            new byte[]
            {
                4
            },
            // make_mixed_mode_tao.nrg
            new byte[]
            {
                4
            },
            // make_udf_dao.nrg
            new byte[]
            {
                4
            },
            // make_udf_tao.nrg
            new byte[]
            {
                4
            },
        };

        [Test]
        public void Test()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory =
                Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Nero Burning ROM", "V1");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new DiscImages.Nero();
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