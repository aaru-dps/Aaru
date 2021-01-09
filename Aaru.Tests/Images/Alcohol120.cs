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

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Alcohol120
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.mds", "gigarec.mds", "jaguarcd.mds", "pcengine.mds", "pcfx.mds",
            "report_audiocd.mds", "report_cdr.mds", "report_cdrom.mds", "report_cdrw_12x.mds", "report_cdrw_2x.mds",
            "report_cdrw_4x.mds", "report_dvd+r-dl.mds", "report_dvd+r.mds", "report_dvd-r.mds", "report_dvdrom.mds",
            "report_dvd+rw.mds", "report_dvd-rw.mds", "report_enhancedcd.mds", "test_all_tracks_are_track_1.mds",
            "test_audiocd_cdtext.mds", "test_castrated_leadout.mds", "test_data_track_as_audio_fixed_sub.mds",
            "test_data_track_as_audio.mds", "test_enhancedcd.mds", "test_incd_udf200_finalized.mds",
            "test_multi_karaoke_sampler.mds", "test_multiple_indexes.mds", "test_multisession_dvd+r.mds",
            "test_multisession_dvd-r.mds", "test_multisession.mds", "test_track0_in_session2.mds",
            "test_track111_in_session2_fixed_sub.mds", "test_track111_in_session2.mds", "test_track1_2_9_fixed_sub.mds",
            "test_track1_2_9.mds", "test_track2_inside_leadout.mds", "test_track2_inside_session2_leadin.mds",
            "test_track2_inside_track1.mds", "test_videocd.mds"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.mds
            279300,

            // gigarec.mds
            469652,

            // jaguarcd.mds
            243587,

            // pcengine.mds
            160956,

            // pcfx.mds
            246680,

            // report_audiocd.mds
            247073,

            // report_cdr.mds
            254265,

            // report_cdrom.mds
            254265,

            // report_cdrw_12x.mds
            308224,

            // report_cdrw_2x.mds
            308224,

            // report_cdrw_4x.mds
            254265,

            // report_dvd+r-dl.mds
            3455936,

            // report_dvd+r.mds
            2146368,

            // report_dvd-r.mds
            2146368,

            // report_dvdrom.mds
            2146368,

            // report_dvd+rw.mds
            2295104,

            // report_dvd-rw.mds
            2146368,

            // report_enhancedcd.mds
            303316,

            // test_all_tracks_are_track_1.mds
            51078,

            // test_audiocd_cdtext.mds
            277696,

            // test_castrated_leadout.mds
            1050,

            // test_data_track_as_audio_fixed_sub.mds
            62385,

            // test_data_track_as_audio.mds
            62385,

            // test_enhancedcd.mds
            59206,

            // test_incd_udf200_finalized.mds
            350134,

            // test_multi_karaoke_sampler.mds
            329158,

            // test_multiple_indexes.mds
            65536,

            // test_multisession_dvd+r.mds
            230624,

            // test_multisession_dvd-r.mds
            257264,

            // test_multisession.mds
            51168,

            // test_track0_in_session2.mds
            25539,

            // test_track111_in_session2_fixed_sub.mds
            25539,

            // test_track111_in_session2.mds
            25539,

            // test_track1_2_9_fixed_sub.mds
            25539,

            // test_track1_2_9.mds
            25539,

            // test_track2_inside_leadout.mds
            25539,

            // test_track2_inside_session2_leadin.mds
            62385,

            // test_track2_inside_track1.mds
            62385,

            // test_videocd.mds
            48794
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.mds
            MediaType.CDDA,

            // gigarec.mds
            MediaType.CDR,

            // jaguarcd.mds
            MediaType.CDDA,

            // pcengine.mds
            MediaType.CD,

            // pcfx.mds
            MediaType.CD,

            // report_audiocd.mds
            MediaType.CDDA,

            // report_cdr.mds
            MediaType.CDR,

            // report_cdrom.mds
            MediaType.CDROM,

            // report_cdrw_12x.mds
            MediaType.CDRW,

            // report_cdrw_2x.mds
            MediaType.CDRW,

            // report_cdrw_4x.mds
            MediaType.CDRW,

            // report_dvd+r-dl.mds
            MediaType.DVDROM,

            // report_dvd+r.mds
            MediaType.DVDROM,

            // report_dvd-r.mds
            MediaType.DVDROM,

            // report_dvdrom.mds
            MediaType.DVDROM,

            // report_dvd+rw.mds
            MediaType.DVDROM,

            // report_dvd-rw.mds
            MediaType.DVDROM,

            // report_enhancedcd.mds
            MediaType.CDPLUS,

            // test_all_tracks_are_track_1.mds
            MediaType.CDR,

            // test_audiocd_cdtext.mds
            MediaType.CDR,

            // test_castrated_leadout.mds
            MediaType.CDR,

            // test_data_track_as_audio_fixed_sub.mds
            MediaType.CDR,

            // test_data_track_as_audio.mds
            MediaType.CDR,

            // test_enhancedcd.mds
            MediaType.CDR,

            // test_incd_udf200_finalized.mds
            MediaType.CDR,

            // test_multi_karaoke_sampler.mds
            MediaType.CDROMXA,

            // test_multiple_indexes.mds
            MediaType.CDR,

            // test_multisession_dvd+r.mds
            MediaType.DVDROM,

            // test_multisession_dvd-r.mds
            MediaType.DVDROM,

            // test_multisession.mds
            MediaType.CDR,

            // test_track0_in_session2.mds
            MediaType.CDR,

            // test_track111_in_session2_fixed_sub.mds
            MediaType.CDR,

            // test_track111_in_session2.mds
            MediaType.CDR,

            // test_track1_2_9_fixed_sub.mds
            MediaType.CDR,

            // test_track1_2_9.mds
            MediaType.CDR,

            // test_track2_inside_leadout.mds
            MediaType.CDR,

            // test_track2_inside_session2_leadin.mds
            MediaType.CDR,

            // test_track2_inside_track1.mds
            MediaType.CDR,

            // test_videocd.mds
            MediaType.CDR
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.mds
            "UNKNOWN",

            // gigarec.mds
            "dc8aaff9bd1a8a6f642e15bce29cd03e",

            // jaguarcd.mds
            "8086a3654d6dede562621d24ae18729e",

            // pcengine.mds
            "0dac1b20a9dc65c4ed1b11f6160ed983",

            // pcfx.mds
            "bc514cb4f3c7e2ee6857b2a3d470278b",

            // report_audiocd.mds
            "ff35cfa013871b322ef54612e719c185",

            // report_cdr.mds
            "016e9431ca3161d427b29dbc1312a232",

            // report_cdrom.mds
            "016e9431ca3161d427b29dbc1312a232",

            // report_cdrw_12x.mds
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_cdrw_2x.mds
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_cdrw_4x.mds
            "fe67ffb95da123e060a1c4d278df3c5a",

            // report_dvd+r-dl.mds
            "692148a01b4204160b088141fb52bd70",

            // report_dvd+r.mds
            "32746029d25e430cd50c464232536d1a",

            // report_dvd-r.mds
            "c20217c0356fcd074c33b5f4b1355914",

            // report_dvdrom.mds
            "0a49394278360f737a22e48ef125d7cd",

            // report_dvd+rw.mds
            "2022eaeb9ccda7532d981c5e22cc9bec",

            // report_dvd-rw.mds
            "4844a94a97027b0fea664a1fba3ecbb2",

            // report_enhancedcd.mds
            "dfd6c0bd02c19145b2a64d8a15912302",

            // test_all_tracks_are_track_1.mds
            "UNKNOWN",

            // test_audiocd_cdtext.mds
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_castrated_leadout.mds
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.mds
            "d9d46cae2a3a46316c8e1411e84d40ef",

            // test_data_track_as_audio.mds
            "d9d46cae2a3a46316c8e1411e84d40ef",

            // test_enhancedcd.mds
            "eb672b8110c73e4df86fc61bfb37f188",

            // test_incd_udf200_finalized.mds
            "f95d6f978ddb4f98bbffda403f627fe1",

            // test_multi_karaoke_sampler.mds
            "1731384a29149b7e6f4c0d0d07f178ca",

            // test_multiple_indexes.mds
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession_dvd+r.mds
            "020993315e49ab0d36bc7248819162ea",

            // test_multisession_dvd-r.mds
            "dff8f2107a4ea9633a88ce38ff609b8e",

            // test_multisession.mds
            "f793fecc486a83cbe05b51c2d98059b9",

            // test_track0_in_session2.mds
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track111_in_session2_fixed_sub.mds
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track111_in_session2.mds
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track1_2_9_fixed_sub.mds
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track1_2_9.mds
            "f9efc75192a7c0f3252e696c617f8ddd",

            // test_track2_inside_leadout.mds
            "UNKNOWN",

            // test_track2_inside_session2_leadin.mds
            "6fa06c10561343438736a8d3d9a965ea",

            // test_track2_inside_track1.mds
            "6fa06c10561343438736a8d3d9a965ea",

            // test_videocd.mds
            "ec7c86e6cfe5f965faa2488ae940e15a"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.mds
            "UNKNOWN",

            // gigarec.mds
            "1ba5f0fb9f3572197a8d039fd341c0aa",

            // jaguarcd.mds
            "8086a3654d6dede562621d24ae18729e",

            // pcengine.mds
            "f1c1dbe1cd9df11fe2c1f0a97130c25f",

            // pcfx.mds
            "dac5dc0961fa435da3c7d433477cda1a",

            // report_audiocd.mds
            "ff35cfa013871b322ef54612e719c185",

            // report_cdr.mds
            "6b4e35ec371770751f26163629253015",

            // report_cdrom.mds
            "6b4e35ec371770751f26163629253015",

            // report_cdrw_12x.mds
            "a1890f71563eb9907e4a08fef6afd6bf",

            // report_cdrw_2x.mds
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_cdrw_4x.mds
            "9c13c4f7dcb76feae684ba9a368094c5",

            // report_dvd+r-dl.mds
            "692148a01b4204160b088141fb52bd70",

            // report_dvd+r.mds
            "32746029d25e430cd50c464232536d1a",

            // report_dvd-r.mds
            "c20217c0356fcd074c33b5f4b1355914",

            // report_dvdrom.mds
            "0a49394278360f737a22e48ef125d7cd",

            // report_dvd+rw.mds
            "2022eaeb9ccda7532d981c5e22cc9bec",

            // report_dvd-rw.mds
            "4844a94a97027b0fea664a1fba3ecbb2",

            // report_enhancedcd.mds
            "0038395e272242a29e84a1fb34a3a15e",

            // test_all_tracks_are_track_1.mds
            "UNKNOWN",

            // test_audiocd_cdtext.mds
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_castrated_leadout.mds
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.mds
            "b3550e61649ba5276fed8d74f8e512ee",

            // test_data_track_as_audio.mds
            "b3550e61649ba5276fed8d74f8e512ee",

            // test_enhancedcd.mds
            "842a9a248396018ddfbfd90785c3f0ce",

            // test_incd_udf200_finalized.mds
            "6751e0ae7821f92221672b1cd5a1ff36",

            // test_multi_karaoke_sampler.mds
            "efe2b3fe51022ef8e0a62587294d1d9c",

            // test_multiple_indexes.mds
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession_dvd+r.mds
            "020993315e49ab0d36bc7248819162ea",

            // test_multisession_dvd-r.mds
            "dff8f2107a4ea9633a88ce38ff609b8e",

            // test_multisession.mds
            "199b85a01c27f55f463fc7d606adfafa",

            // test_track0_in_session2.mds
            "3b3172070738044417ae5284195acbfd",

            // test_track111_in_session2_fixed_sub.mds
            "396f86cdd8bfb012b68eabd5a94f604b",

            // test_track111_in_session2.mds
            "76175679c852073137299c5ca7b113e4",

            // test_track1_2_9_fixed_sub.mds
            "6ff84bf8ecf2624fbaba37df08462294",

            // test_track1_2_9.mds
            "3b3172070738044417ae5284195acbfd",

            // test_track2_inside_leadout.mds
            "UNKNOWN",

            // test_track2_inside_session2_leadin.mds
            "608a73cd10bccdadde68523aead1ee72",

            // test_track2_inside_track1.mds
            "c82d20702d31bc15bdc91f7e107862ae",

            // test_videocd.mds
            "4a045788e69965efe0c87950d013e720"
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.mds
            "UNKNOWN",

            // gigarec.mds
            "95ef603d7dc9e285929cbf3c79ba9db2",

            // jaguarcd.mds
            "83ec1010fc44694d69dc48bacec5481a",

            // pcengine.mds
            "9e9a6b51bc2e5ec67400cb33ad0ca33f",

            // pcfx.mds
            "e3a0d78b6c32f5795b1b513bd13a6bda",

            // report_audiocd.mds
            "9da6ad8f6f0cadd92509c10809da7296",

            // report_cdr.mds
            "6ea1db8638c111b7fd45b35a138d24fe",

            // report_cdrom.mds
            "1994c303674718c74b35f9a4ea1d3515",

            // report_cdrw_12x.mds
            "337aefffca57a2d0222dabd8989f0b3f",

            // report_cdrw_2x.mds
            "6fe81a972e750c68e08f6935e4d91e34",

            // report_cdrw_4x.mds
            "e4095cb91fa40382dcadc22433b281c3",

            // report_dvd+r-dl.mds
            null,

            // report_dvd+r.mds
            null,

            // report_dvd-r.mds
            null,

            // report_dvdrom.mds
            null,

            // report_dvd+rw.mds
            null,

            // report_dvd-rw.mds
            null,

            // report_enhancedcd.mds
            "e6f7319532f46c3fa4fd3569c65546e1",

            // test_all_tracks_are_track_1.mds
            "UNKNOWN",

            // test_audiocd_cdtext.mds
            "ca781a7afc4eb77c51f7c551ed45c03c",

            // test_castrated_leadout.mds
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.mds
            "77778d0e72a499b6c22f75df11a8d97f",

            // test_data_track_as_audio.mds
            "5479a1115bb6481db69fd6262e8c6076",

            // test_enhancedcd.mds
            "fa2c839e1d7fedd1f4e853f682d3bf51",

            // test_incd_udf200_finalized.mds
            "65f938f7f9ac34fabd3ab94c14eb76b5",

            // test_multi_karaoke_sampler.mds
            "f8c96f120cac18c52178b99ef4c4e2a9",

            // test_multiple_indexes.mds
            "25bae9e30657e2f64a45e5f690e3ae9e",

            // test_multisession_dvd+r.mds
            null,

            // test_multisession_dvd-r.mds
            null,

            // test_multisession.mds
            "48656afdbc40b6df06486a04a4d62401",

            // test_track0_in_session2.mds
            "7eedb60edb3dc77eac41fd8f2214dfb8",

            // test_track111_in_session2_fixed_sub.mds
            "c81a161af0fcd01dfd340290178a32fd",

            // test_track111_in_session2.mds
            "8a3b37786d5276529c8cdbbf57e2d528",

            // test_track1_2_9_fixed_sub.mds
            "b6051aa115a91c08de0ffc47ca64275e",

            // test_track1_2_9.mds
            "ee1a81152b386347dc656697d8f50ab9",

            // test_track2_inside_leadout.mds
            "UNKNOWN",

            // test_track2_inside_session2_leadin.mds
            "933f1699ba88a70aff5062f9626ef529",

            // test_track2_inside_track1.mds
            "d8eed571f137c92f22bb858d78fc1e41",

            // test_videocd.mds
            "935a91f5850352818d92b71f1c87c393"
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.mds
            22,

            // gigarec.mds
            1,

            // jaguarcd.mds
            11,

            // pcengine.mds
            16,

            // pcfx.mds
            8,

            // report_audiocd.mds
            14,

            // report_cdr.mds
            1,

            // report_cdrom.mds
            1,

            // report_cdrw_12x.mds
            1,

            // report_cdrw_2x.mds
            1,

            // report_cdrw_4x.mds
            1,

            // report_dvd+r-dl.mds
            1,

            // report_dvd+r.mds
            1,

            // report_dvd-r.mds
            1,

            // report_dvdrom.mds
            1,

            // report_dvd+rw.mds
            1,

            // report_dvd-rw.mds
            1,

            // report_enhancedcd.mds
            14,

            // test_all_tracks_are_track_1.mds
            2,

            // test_audiocd_cdtext.mds
            11,

            // test_castrated_leadout.mds
            11,

            // test_data_track_as_audio_fixed_sub.mds
            2,

            // test_data_track_as_audio.mds
            2,

            // test_enhancedcd.mds
            3,

            // test_incd_udf200_finalized.mds
            1,

            // test_multi_karaoke_sampler.mds
            16,

            // test_multiple_indexes.mds
            5,

            // test_multisession_dvd+r.mds
            // Alcohol does not detect multiple tracks or sessions in recordable DVDs
            1,

            // test_multisession_dvd-r.mds
            // Alcohol does not detect multiple tracks or sessions in recordable DVDs
            1,

            // test_multisession.mds
            4,

            // test_track0_in_session2.mds
            // Alcohol did not detect the second session
            1,

            // test_track111_in_session2_fixed_sub.mds
            // Alcohol did not detect the second session
            1,

            // test_track111_in_session2.mds
            // Alcohol did not detect the second session
            1,

            // test_track1_2_9_fixed_sub.mds
            // Alcohol did not detect the second session
            2,

            // test_track1_2_9.mds
            // Alcohol did not detect the second session
            2,

            // test_track2_inside_leadout.mds
            // Alcohol did not detect the second session
            2,

            // test_track2_inside_session2_leadin.mds
            3,

            // test_track2_inside_track1.mds
            3,

            // test_videocd.mds
            2
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // gigarec.mds
            new[]
            {
                1
            },

            // jaguarcd.mds
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // pcengine.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.mds
            new[]
            {
                1
            },

            // report_cdrom.mds
            new[]
            {
                1
            },

            // report_cdrw_12x.mds
            new[]
            {
                1
            },

            // report_cdrw_2x.mds
            new[]
            {
                1
            },

            // report_cdrw_4x.mds
            new[]
            {
                1
            },

            // report_dvd+r-dl.mds
            new[]
            {
                1
            },

            // report_dvd+r.mds
            new[]
            {
                1
            },

            // report_dvd-r.mds
            new[]
            {
                1
            },

            // report_dvdrom.mds
            new[]
            {
                1
            },

            // report_dvd+rw.mds
            new[]
            {
                1
            },

            // report_dvd-rw.mds
            new[]
            {
                1
            },

            // report_enhancedcd.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_all_tracks_are_track_1.mds
            new[]
            {
                1, 2
            },

            // test_audiocd_cdtext.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_castrated_leadout.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_data_track_as_audio_fixed_sub.mds
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio.mds
            new[]
            {
                1, 2
            },

            // test_enhancedcd.mds
            new[]
            {
                1, 1, 2
            },

            // test_incd_udf200_finalized.mds
            new[]
            {
                1
            },

            // test_multi_karaoke_sampler.mds
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.mds
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession_dvd+r.mds
            new[]
            {
                1
            },

            // test_multisession_dvd-r.mds
            new[]
            {
                1
            },

            // test_multisession.mds
            new[]
            {
                1, 2, 3, 4
            },

            // test_track0_in_session2.mds
            new[]
            {
                1
            },

            // test_track111_in_session2_fixed_sub.mds
            new[]
            {
                1
            },

            // test_track111_in_session2.mds
            new[]
            {
                1
            },

            // test_track1_2_9_fixed_sub.mds
            new[]
            {
                1, 1
            },

            // test_track1_2_9.mds
            new[]
            {
                1, 1
            },

            // test_track2_inside_leadout.mds
            new[]
            {
                1, 1
            },

            // test_track2_inside_session2_leadin.mds
            new[]
            {
                1, 1, 1
            },

            // test_track2_inside_track1.mds
            new[]
            {
                1, 1, 1
            },

            // test_videocd.mds
            new[]
            {
                1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.mds
            new ulong[]
            {
                69150, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // gigarec.mds
            new ulong[]
            {
                0
            },

            // jaguarcd.mds
            new ulong[]
            {
                0, 27640, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.mds
            new ulong[]
            {
                0, 3590, 38614, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // pcfx.mds
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220795, 225646, 235498
            },

            // report_audiocd.mds
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdr.mds
            new ulong[]
            {
                0
            },

            // report_cdrom.mds
            new ulong[]
            {
                0
            },

            // report_cdrw_12x.mds
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.mds
            new ulong[]
            {
                0
            },

            // report_cdrw_4x.mds
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.mds
            new ulong[]
            {
                0
            },

            // report_dvd+r.mds
            new ulong[]
            {
                0
            },

            // report_dvd-r.mds
            new ulong[]
            {
                0
            },

            // report_dvdrom.mds
            new ulong[]
            {
                0
            },

            // report_dvd+rw.mds
            new ulong[]
            {
                0
            },

            // report_dvd-rw.mds
            new ulong[]
            {
                0
            },

            // report_enhancedcd.mds
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234180
            },

            // test_all_tracks_are_track_1.mds
            new ulong[]
            {
                0, 25539
            },

            // test_audiocd_cdtext.mds
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_castrated_leadout.mds
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_data_track_as_audio_fixed_sub.mds
            new ulong[]
            {
                0, 36939
            },

            // test_data_track_as_audio.mds
            new ulong[]
            {
                0, 36939
            },

            // test_enhancedcd.mds
            new ulong[]
            {
                0, 14405, 40353
            },

            // test_incd_udf200_finalized.mds
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.mds
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multiple_indexes.mds
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession_dvd+r.mds
            new ulong[]
            {
                0
            },

            // test_multisession_dvd-r.mds
            new ulong[]
            {
                0
            },

            // test_multisession.mds
            new ulong[]
            {
                0, 19533, 32860, 45378
            },

            // test_track0_in_session2.mds
            new ulong[]
            {
                0
            },

            // test_track111_in_session2_fixed_sub.mds
            new ulong[]
            {
                0
            },

            // test_track111_in_session2.mds
            new ulong[]
            {
                0
            },

            // test_track1_2_9_fixed_sub.mds
            new ulong[]
            {
                0, 13350
            },

            // test_track1_2_9.mds
            new ulong[]
            {
                0, 13350
            },

            // test_track2_inside_leadout.mds
            new ulong[]
            {
                0, 62385
            },

            // test_track2_inside_session2_leadin.mds
            new ulong[]
            {
                0, 25500, 36939
            },

            // test_track2_inside_track1.mds
            new ulong[]
            {
                0, 13350, 36939
            },

            // test_videocd.mds
            new ulong[]
            {
                0, 1252
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.mds
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279299
            },

            // gigarec.mds
            new ulong[]
            {
                469651
            },

            // jaguarcd.mds
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.mds
            new ulong[]
            {
                3439, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126078, 160955
            },

            // pcfx.mds
            new ulong[]
            {
                4244, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },

            // report_audiocd.mds
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdr.mds
            new ulong[]
            {
                254264
            },

            // report_cdrom.mds
            new ulong[]
            {
                254264
            },

            // report_cdrw_12x.mds
            new ulong[]
            {
                308223
            },

            // report_cdrw_2x.mds
            new ulong[]
            {
                308223
            },

            // report_cdrw_4x.mds
            new ulong[]
            {
                254264
            },

            // report_dvd+r-dl.mds
            new ulong[]
            {
                3455935
            },

            // report_dvd+r.mds
            new ulong[]
            {
                2146367
            },

            // report_dvd-r.mds
            new ulong[]
            {
                2146367
            },

            // report_dvdrom.mds
            new ulong[]
            {
                2146367
            },

            // report_dvd+rw.mds
            new ulong[]
            {
                2295103
            },

            // report_dvd-rw.mds
            new ulong[]
            {
                2146367
            },

            // report_enhancedcd.mds
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // test_all_tracks_are_track_1.mds
            new ulong[]
            {
                25538, 51077
            },

            // test_audiocd_cdtext.mds
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_castrated_leadout.mds
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 1049
            },

            // test_data_track_as_audio_fixed_sub.mds
            new ulong[]
            {
                25538, 62384
            },

            // test_data_track_as_audio.mds
            new ulong[]
            {
                25538, 62384
            },

            // test_enhancedcd.mds
            new ulong[]
            {
                14404, 28952, 59205
            },

            // test_incd_udf200_finalized.mds
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.mds
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multiple_indexes.mds
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession_dvd+r.mds
            new ulong[]
            {
                230623
            },

            // test_multisession_dvd-r.mds
            new ulong[]
            {
                257263
            },

            // test_multisession.mds
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_track0_in_session2.mds
            new ulong[]
            {
                25538
            },

            // test_track111_in_session2_fixed_sub.mds
            new ulong[]
            {
                25538
            },

            // test_track111_in_session2.mds
            new ulong[]
            {
                25538
            },

            // test_track1_2_9_fixed_sub.mds
            new ulong[]
            {
                13349, 25538
            },

            // test_track1_2_9.mds
            new ulong[]
            {
                13349, 25538
            },

            // test_track2_inside_leadout.mds
            new ulong[]
            {
                62234, 25538
            },

            // test_track2_inside_session2_leadin.mds
            new ulong[]
            {
                25499, 25538, 62384
            },

            // test_track2_inside_track1.mds
            new ulong[]
            {
                13349, 25538, 62384
            },

            // test_videocd.mds
            new ulong[]
            {
                1101, 48793
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.mds
            new ulong[]
            {
                69300, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // gigarec.mds
            new ulong[]
            {
                150
            },

            // jaguarcd.mds
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.mds
            new ulong[]
            {
                150, 150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // pcfx.mds
            new ulong[]
            {
                150, 150, 0, 0, 0, 150, 0, 0
            },

            // report_audiocd.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.mds
            new ulong[]
            {
                150
            },

            // report_cdrom.mds
            new ulong[]
            {
                150
            },

            // report_cdrw_12x.mds
            new ulong[]
            {
                150
            },

            // report_cdrw_2x.mds
            new ulong[]
            {
                150
            },

            // report_cdrw_4x.mds
            new ulong[]
            {
                150
            },

            // report_dvd+r-dl.mds
            new ulong[]
            {
                0
            },

            // report_dvd+r.mds
            new ulong[]
            {
                0
            },

            // report_dvd-r.mds
            new ulong[]
            {
                0
            },

            // report_dvdrom.mds
            new ulong[]
            {
                0
            },

            // report_dvd+rw.mds
            new ulong[]
            {
                0
            },

            // report_dvd-rw.mds
            new ulong[]
            {
                0
            },

            // report_enhancedcd.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_all_tracks_are_track_1.mds
            new ulong[]
            {
                150, 150
            },

            // test_audiocd_cdtext.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_castrated_leadout.mds
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_data_track_as_audio_fixed_sub.mds
            new ulong[]
            {
                150, 150
            },

            // test_data_track_as_audio.mds
            new ulong[]
            {
                150, 150
            },

            // test_enhancedcd.mds
            new ulong[]
            {
                150, 0, 150
            },

            // test_incd_udf200_finalized.mds
            new ulong[]
            {
                150
            },

            // test_multi_karaoke_sampler.mds
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.mds
            new ulong[]
            {
                150, 0, 0, 0, 0
            },

            // test_multisession_dvd+r.mds
            new ulong[]
            {
                0
            },

            // test_multisession_dvd-r.mds
            new ulong[]
            {
                0
            },

            // test_multisession.mds
            new ulong[]
            {
                150, 150, 150, 150
            },

            // test_track0_in_session2.mds
            new ulong[]
            {
                150
            },

            // test_track111_in_session2_fixed_sub.mds
            new ulong[]
            {
                150
            },

            // test_track111_in_session2.mds
            new ulong[]
            {
                150
            },

            // test_track1_2_9_fixed_sub.mds
            new ulong[]
            {
                150, 0
            },

            // test_track1_2_9.mds
            new ulong[]
            {
                150, 0
            },

            // test_track2_inside_leadout.mds
            new ulong[]
            {
                150, 150
            },

            // test_track2_inside_session2_leadin.mds
            new ulong[]
            {
                150, 0, 150
            },

            // test_track2_inside_track1.mds
            new ulong[]
            {
                150, 0, 150
            },

            // test_videocd.mds
            new ulong[]
            {
                150, 150
            }
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.mds
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // gigarec.mds
            new byte[]
            {
                4
            },

            // jaguarcd.mds
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.mds
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.mds
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // report_audiocd.mds
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.mds
            new byte[]
            {
                4
            },

            // report_cdrom.mds
            new byte[]
            {
                4
            },

            // report_cdrw_12x.mds
            new byte[]
            {
                4
            },

            // report_cdrw_2x.mds
            new byte[]
            {
                4
            },

            // report_cdrw_4x.mds
            new byte[]
            {
                4
            },

            // report_dvd+r-dl.mds
            null,

            // report_dvd+r.mds
            null,

            // report_dvd-r.mds
            null,

            // report_dvdrom.mds
            null,

            // report_dvd+rw.mds
            null,

            // report_dvd-rw.mds
            null,

            // report_enhancedcd.mds
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // test_all_tracks_are_track_1.mds
            new byte[]
            {
                4, 4
            },

            // test_audiocd_cdtext.mds
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_castrated_leadout.mds
            new byte[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // test_data_track_as_audio_fixed_sub.mds
            new byte[]
            {
                4, 2
            },

            // test_data_track_as_audio.mds
            new byte[]
            {
                4, 2
            },

            // test_enhancedcd.mds
            new byte[]
            {
                0, 0, 4
            },

            // test_incd_udf200_finalized.mds
            new byte[]
            {
                7
            },

            // test_multi_karaoke_sampler.mds
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.mds
            new byte[]
            {
                2, 0, 0, 8, 1
            },

            // test_multisession_dvd+r.mds
            null,

            // test_multisession_dvd-r.mds
            null, // test_multisession.mds
            new byte[]
            {
                4, 4, 4, 4
            },

            // test_track0_in_session2.mds
            new byte[]
            {
                4
            },

            // test_track111_in_session2_fixed_sub.mds
            new byte[]
            {
                4
            },

            // test_track111_in_session2.mds
            new byte[]
            {
                4
            },

            // test_track1_2_9_fixed_sub.mds
            new byte[]
            {
                4, 4
            },

            // test_track1_2_9.mds
            new byte[]
            {
                4, 4
            },

            // test_track2_inside_leadout.mds
            new byte[]
            {
                4, 4
            },

            // test_track2_inside_session2_leadin.mds
            new byte[]
            {
                4, 4, 4
            },

            // test_track2_inside_track1.mds
            new byte[]
            {
                4, 4, 4
            },

            // test_videocd.mds
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

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Alcohol 120%");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new DiscImages.Alcohol120();
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