// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiscJuggler.cs
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
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class DiscJuggler
    {
        readonly string[] _testFiles =
        {
            "jaguarcd.cdi", "make_audiocd.cdi", "make_data_mode1_joliet.cdi", "make_data_mode2_joliet.cdi",
            "make_dvd.cdi", "make_enhancedcd.cdi", "make_mixed_mode.cdi", "make_multisession_dvd.cdi",
            "make_pangram_mode1_joliet.cdi", "make_pangram_mode2_joliet.cdi", "pcengine.cdi", "pcfx.cdi",
            "report_audiocd.cdi", "report_cdr.cdi", "report_cdrom.cdi", "report_cdrw_2x.cdi", "report_dvdram_v1.cdi",
            "report_dvdram_v2.cdi", "report_dvd-r.cdi", "report_dvd+r-dl.cdi", "report_dvdrom.cdi", "report_dvd+rw.cdi",
            "report_dvd-rw.cdi", "report_enhancedcd.cdi", "test_audiocd_cdtext.cdi", "test_data_track_as_audio.cdi",
            "test_data_track_as_audio_fixed_sub.cdi", "test_disc_starts_at_track2.cdi", "test_enhancedcd.cdi",
            "test_incd_udf200_finalized.cdi", "test_karaoke_multi_sampler.cdi", "test_multiple_indexes.cdi",
            "test_multisession.cdi", "test_multisession_dvd+r.cdi", "test_multisession_dvd-r.cdi",
            "test_track111_in_session2.cdi", "test_track111_in_session2_fixed_sub.cdi", "test_track2_inside_track1.cdi",
            "test_videocd.cdi"
        };

        readonly ulong[] _sectors =
        {
            // jaguarcd.cdi
            230845,

            // make_audiocd.cdi
            0,

            // make_data_mode1_joliet.cdi
            0,

            // make_data_mode2_joliet.cdi
            0,

            // make_dvd.cdi
            84896,

            // make_enhancedcd.cdi
            0,

            // make_mixed_mode.cdi
            0,

            // make_multisession_dvd.cdi
            0,

            // make_pangram_mode1_joliet.cdi
            0,

            // make_pangram_mode2_joliet.cdi
            0,

            // pcengine.cdi
            160356,

            // pcfx.cdi
            245930,

            // report_audiocd.cdi
            245273,

            // report_cdr.cdi
            254265,

            // report_cdrom.cdi
            254265,

            // report_cdrw_2x.cdi
            308224,

            // report_dvdram_v1.cdi
            1218961,

            // report_dvdram_v2.cdi
            2236705,

            // report_dvd-r.cdi
            2146368,

            // report_dvd+r-dl.cdi
            3455936,

            // report_dvdrom.cdi
            2146368,

            // report_dvd+rw.cdi
            2295104,

            // report_dvd-rw.cdi
            2146368,

            // report_enhancedcd.cdi
            291916,

            // test_audiocd_cdtext.cdi
            277696,

            // test_data_track_as_audio.cdi
            50985,

            // test_data_track_as_audio_fixed_sub.cdi
            50985,

            // test_disc_starts_at_track2.cdi
            50985,

            // test_enhancedcd.cdi
            47806,

            // test_incd_udf200_finalized.cdi
            350134,

            // test_karaoke_multi_sampler.cdi
            329008,

            // test_multiple_indexes.cdi
            65536,

            // test_multisession.cdi
            25968,

            // test_multisession_dvd+r.cdi
            230624,

            // test_multisession_dvd-r.cdi
            257264,

            // test_track111_in_session2.cdi
            0,

            // test_track111_in_session2_fixed_sub.cdi
            0,

            // test_track2_inside_track1.cdi
            0,

            // test_videocd.cdi
            48644
        };

        readonly MediaType[] _mediaTypes =
        {
            // jaguarcd.cdi
            MediaType.CDDA,

            // make_audiocd.cdi
            MediaType.CDDA,

            // make_data_mode1_joliet.cdi
            MediaType.CDDA,

            // make_data_mode2_joliet.cdi
            MediaType.CDDA,

            // make_dvd.cdi
            MediaType.DVDROM,

            // make_enhancedcd.cdi
            MediaType.CDDA,

            // make_mixed_mode.cdi
            MediaType.CDDA,

            // make_multisession_dvd.cdi
            MediaType.CDDA,

            // make_pangram_mode1_joliet.cdi
            MediaType.CDDA,

            // make_pangram_mode2_joliet.cdi
            MediaType.CDDA,

            // pcengine.cdi
            MediaType.CD,

            // pcfx.cdi
            MediaType.CD,

            // report_audiocd.cdi
            MediaType.CDDA,

            // report_cdr.cdi
            MediaType.CDROM,

            // report_cdrom.cdi
            MediaType.CDROM,

            // report_cdrw_2x.cdi
            MediaType.CDROM,

            // report_dvdram_v1.cdi
            MediaType.DVDROM,

            // report_dvdram_v2.cdi
            MediaType.DVDROM,

            // report_dvd-r.cdi
            MediaType.DVDROM,

            // report_dvd+r-dl.cdi
            MediaType.DVDROM,

            // report_dvdrom.cdi
            MediaType.DVDROM,

            // report_dvd+rw.cdi
            MediaType.DVDROM,

            // report_dvd-rw.cdi
            MediaType.DVDROM,

            // report_enhancedcd.cdi
            MediaType.CDPLUS,

            // test_audiocd_cdtext.cdi
            MediaType.CDDA,

            // test_data_track_as_audio.cdi
            MediaType.CDDA,

            // test_data_track_as_audio_fixed_sub.cdi
            MediaType.CDROMXA,

            // test_disc_starts_at_track2.cdi
            MediaType.CDROMXA,

            // test_enhancedcd.cdi
            MediaType.CDPLUS,

            // test_incd_udf200_finalized.cdi
            MediaType.CDROMXA,

            // test_karaoke_multi_sampler.cdi
            MediaType.CDROMXA,

            // test_multiple_indexes.cdi
            MediaType.CDDA,

            // test_multisession.cdi
            MediaType.CDROMXA,

            // test_multisession_dvd+r.cdi
            MediaType.DVDROM,

            // test_multisession_dvd-r.cdi
            MediaType.DVDROM,

            // test_track111_in_session2.cdi
            MediaType.CDDA,

            // test_track111_in_session2_fixed_sub.cdi
            MediaType.CDDA,

            // test_track2_inside_track1.cdi
            MediaType.CDDA,

            // test_videocd.cdi
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // jaguarcd.cdi
            "e234467539490be2db99d643b1d4e905",

            // make_audiocd.cdi
            "UNKNOWN",

            // make_data_mode1_joliet.cdi
            "UNKNOWN",

            // make_data_mode2_joliet.cdi
            "UNKNOWN",

            // make_dvd.cdi
            "5240b794f12174da73915e8c1f38b6a4",

            // make_enhancedcd.cdi
            "UNKNOWN",

            // make_mixed_mode.cdi
            "UNKNOWN",

            // make_multisession_dvd.cdi
            "UNKNOWN",

            // make_pangram_mode1_joliet.cdi
            "UNKNOWN",

            // make_pangram_mode2_joliet.cdi
            "UNKNOWN",

            // pcengine.cdi
            "b7947d8d77c2ede5199293ee2ac387ed",

            // pcfx.cdi
            "2e872a5cfa43959183677398ede15c08",

            // report_audiocd.cdi
            "UNKNOWN",

            // report_cdr.cdi
            "UNKNOWN",

            // report_cdrom.cdi
            "UNKNOWN",

            // report_cdrw_2x.cdi
            "UNKNOWN",

            // report_dvdram_v1.cdi
            "b04c88635c5d493c250c289964018a7a",

            // report_dvdram_v2.cdi
            "c0823b070513d02c9f272986f23e74e8",

            // report_dvd-r.cdi
            "106f141400355476b499213f36a363f9",

            // report_dvd+r-dl.cdi
            "692148a01b4204160b088141fb52bd70",

            // report_dvdrom.cdi
            "106f141400355476b499213f36a363f9",

            // report_dvd+rw.cdi
            "759e9c19389aee07f88a994132b6f8d9",

            // report_dvd-rw.cdi
            "106f141400355476b499213f36a363f9",

            // report_enhancedcd.cdi
            "UNKNOWN",

            // test_audiocd_cdtext.cdi
            "52d7a2793b7600dc94d007f5e7dfd942",

            // test_data_track_as_audio.cdi
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.cdi
            "UNKNOWN",

            // test_disc_starts_at_track2.cdi
            "UNKNOWN",

            // test_enhancedcd.cdi
            "31054e6b8f4d51fe502ac340490bcd46",

            // test_incd_udf200_finalized.cdi
            "d976a8d0131bf48926542160bb41fc13",

            // test_karaoke_multi_sampler.cdi
            "UNKNOWN",

            // test_multiple_indexes.cdi
            "9315c6fc3cf5371ae3795df2b624bd5e",

            // test_multisession.cdi
            "46e43ed4712e5ae61b653b4d19f27080",

            // test_multisession_dvd+r.cdi
            "020993315e49ab0d36bc7248819162ea",

            // test_multisession_dvd-r.cdi
            "dff8f2107a4ea9633a88ce38ff609b8e",

            // test_track111_in_session2.cdi
            "UNKNOWN",

            // test_track111_in_session2_fixed_sub.cdi
            "UNKNOWN",

            // test_track2_inside_track1.cdi
            "UNKNOWN",

            // test_videocd.cdi
            "e5b596e73f46f646a51e1315b59e7cb9"
        };

        readonly string[] _longMd5S =
        {
            // jaguarcd.cdi
            "e234467539490be2db99d643b1d4e905",

            // make_audiocd.cdi
            "UNKNOWN",

            // make_data_mode1_joliet.cdi
            "UNKNOWN",

            // make_data_mode2_joliet.cdi
            "UNKNOWN",

            // make_dvd.cdi
            "5240b794f12174da73915e8c1f38b6a4",

            // make_enhancedcd.cdi
            "UNKNOWN",

            // make_mixed_mode.cdi
            "UNKNOWN",

            // make_multisession_dvd.cdi
            "UNKNOWN",

            // make_pangram_mode1_joliet.cdi
            "UNKNOWN",

            // make_pangram_mode2_joliet.cdi
            "UNKNOWN",

            // pcengine.cdi
            "9fdbcb9827f0bbafcd886447b386bc58",

            // pcfx.cdi
            "a8939e0fd28ee0bd876101b218af3572",

            // report_audiocd.cdi
            "UNKNOWN",

            // report_cdr.cdi
            "UNKNOWN",

            // report_cdrom.cdi
            "UNKNOWN",

            // report_cdrw_2x.cdi
            "UNKNOWN",

            // report_dvdram_v1.cdi
            "b04c88635c5d493c250c289964018a7a",

            // report_dvdram_v2.cdi
            "c0823b070513d02c9f272986f23e74e8",

            // report_dvd-r.cdi
            "106f141400355476b499213f36a363f9",

            // report_dvd+r-dl.cdi
            "692148a01b4204160b088141fb52bd70",

            // report_dvdrom.cdi
            "106f141400355476b499213f36a363f9",

            // report_dvd+rw.cdi
            "759e9c19389aee07f88a994132b6f8d9",

            // report_dvd-rw.cdi
            "106f141400355476b499213f36a363f9",

            // report_enhancedcd.cdi
            "UNKNOWN",

            // test_audiocd_cdtext.cdi
            "52d7a2793b7600dc94d007f5e7dfd942",

            // test_data_track_as_audio.cdi
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.cdi
            "UNKNOWN",

            // test_disc_starts_at_track2.cdi
            "UNKNOWN",

            // test_enhancedcd.cdi
            "2fc4b8966350322ed3fd553b9e628164",

            // test_incd_udf200_finalized.cdi
            "cd55978d00f1bc127a0e652259ba2418",

            // test_karaoke_multi_sampler.cdi
            "UNKNOWN",

            // test_multiple_indexes.cdi
            "9315c6fc3cf5371ae3795df2b624bd5e",

            // test_multisession.cdi
            "cac33e71b4693b2902f086a0a433129d",

            // test_multisession_dvd+r.cdi
            "020993315e49ab0d36bc7248819162ea",

            // test_multisession_dvd-r.cdi
            "dff8f2107a4ea9633a88ce38ff609b8e",

            // test_track111_in_session2.cdi
            "UNKNOWN",

            // test_track111_in_session2_fixed_sub.cdi
            "UNKNOWN",

            // test_track2_inside_track1.cdi
            "UNKNOWN",

            // test_videocd.cdi
            "acd1a8de676ebe6feeb9d6964ccd63ea"
        };

        readonly string[] _subchannelMd5S =
        {
            // jaguarcd.cdi
            "d02a5fb43012a1f178a540d0e054d183",

            // make_audiocd.cdi
            "UNKNOWN",

            // make_data_mode1_joliet.cdi
            "UNKNOWN",

            // make_data_mode2_joliet.cdi
            "UNKNOWN",

            // make_dvd.cdi
            null,

            // make_enhancedcd.cdi
            "UNKNOWN",

            // make_mixed_mode.cdi
            "UNKNOWN",

            // make_multisession_dvd.cdi
            "UNKNOWN",

            // make_pangram_mode1_joliet.cdi
            "UNKNOWN",

            // make_pangram_mode2_joliet.cdi
            "UNKNOWN",

            // pcengine.cdi
            "19566671874ef21e4c4ba4de5fd5a7ad",

            // pcfx.cdi
            null,

            // report_audiocd.cdi
            "UNKNOWN",

            // report_cdr.cdi
            "UNKNOWN",

            // report_cdrom.cdi
            "UNKNOWN",

            // report_cdrw_2x.cdi
            "UNKNOWN",

            // report_dvdram_v1.cdi
            null,

            // report_dvdram_v2.cdi
            null,

            // report_dvd-r.cdi
            null,

            // report_dvd+r-dl.cdi
            null,

            // report_dvdrom.cdi
            null,

            // report_dvd+rw.cdi
            null,

            // report_dvd-rw.cdi
            null,

            // report_enhancedcd.cdi
            "UNKNOWN",

            // test_audiocd_cdtext.cdi
            null,

            // test_data_track_as_audio.cdi
            "UNKNOWN",

            // test_data_track_as_audio_fixed_sub.cdi
            "UNKNOWN",

            // test_disc_starts_at_track2.cdi
            "UNKNOWN",

            // test_enhancedcd.cdi
            null,

            // test_incd_udf200_finalized.cdi
            null,

            // test_karaoke_multi_sampler.cdi
            "UNKNOWN",

            // test_multiple_indexes.cdi
            null,

            // test_multisession.cdi
            null,

            // test_multisession_dvd+r.cdi
            null,

            // test_multisession_dvd-r.cdi
            null,

            // test_track111_in_session2.cdi
            "UNKNOWN",

            // test_track111_in_session2_fixed_sub.cdi
            "UNKNOWN",

            // test_track2_inside_track1.cdi
            "UNKNOWN",

            // test_videocd.cdi
            null
        };

        readonly int[] _tracks =
        {
            // jaguarcd.cd
            11,

            // make_audiocd.cdi
            0,

            // make_data_mode1_joliet.cdi
            0,

            // make_data_mode2_joliet.cdi
            0,

            // make_dvd.cdi
            1,

            // make_enhancedcd.cdi
            0,

            // make_mixed_mode.cdi
            0,

            // make_multisession_dvd.cdi
            0,

            // make_pangram_mode1_joliet.cdi
            0,

            // make_pangram_mode2_joliet.cdi
            0,

            // pcengine.cdi
            16,

            // pcfx.cdi
            8,

            // report_audiocd.cdi
            14,

            // report_cdr.cdi
            1,

            // report_cdrom.cdi
            1,

            // report_cdrw_2x.cdi
            1,

            // report_dvdram_v1.cdi
            1,

            // report_dvdram_v2.cdi
            1,

            // report_dvd-r.cdi
            1,

            // report_dvd+r-dl.cdi
            1,

            // report_dvdrom.cdi
            1,

            // report_dvd+rw.cdi
            1,

            // report_dvd-rw.cdi
            1,

            // report_enhancedcd.cdi
            14,

            // test_audiocd_cdtext.cdi
            11,

            // test_data_track_as_audio.cdi
            2,

            // test_data_track_as_audio_fixed_sub.cdi
            2,

            // test_disc_starts_at_track2.cdi
            2,

            // test_enhancedcd.cdi
            3,

            // test_incd_udf200_finalized.cdi
            1,

            // test_karaoke_multi_sampler.cdi
            16,

            // test_multiple_indexes.cdi
            5,

            // test_multisession.cdi
            4,

            // test_multisession_dvd+r.cdi
            1,

            // test_multisession_dvd-r.cdi
            1,

            // test_track111_in_session2.cdi
            0,

            // test_track111_in_session2_fixed_sub.cdi
            0,

            // test_track2_inside_track1.cdi
            0,

            // test_videocd.cdi
            2
        };

        readonly int[][] _trackSessions =
        {
            // jaguarcd.cdi
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // make_audiocd.cdi
            new[]
            {
                1
            },

            // make_data_mode1_joliet.cdi
            new[]
            {
                1
            },

            // make_data_mode2_joliet.cdi
            new[]
            {
                1
            },

            // make_dvd.cdi
            new[]
            {
                1
            },

            // make_enhancedcd.cdi
            new[]
            {
                1
            },

            // make_mixed_mode.cdi
            new[]
            {
                1
            },

            // make_multisession_dvd.cdi
            new[]
            {
                1
            },

            // make_pangram_mode1_joliet.cdi
            new[]
            {
                1
            },

            // make_pangram_mode2_joliet.cdi
            new[]
            {
                1
            },

            // pcengine.cdi
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.cdi
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.cdi
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.cdi
            new[]
            {
                1
            },

            // report_cdrom.cdi
            new[]
            {
                1
            },

            // report_cdrw_2x.cdi
            new[]
            {
                1
            },

            // report_dvdram_v1.cdi
            new[]
            {
                1
            },

            // report_dvdram_v2.cdi
            new[]
            {
                1
            },

            // report_dvd-r.cdi
            new[]
            {
                1
            },

            // report_dvd+r-dl.cdi
            new[]
            {
                1
            },

            // report_dvdrom.cdi
            new[]
            {
                1
            },

            // report_dvd+rw.cdi
            new[]
            {
                1
            },

            // report_dvd-rw.cdi
            new[]
            {
                1
            },

            // report_enhancedcd.cdi
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_audiocd_cdtext.cdi
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_data_track_as_audio.cdi
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio_fixed_sub.cdi
            new[]
            {
                1, 2
            },

            // test_disc_starts_at_track2.cdi
            new[]
            {
                1, 2
            },

            // test_enhancedcd.cdi
            new[]
            {
                1, 1, 2
            },

            // test_incd_udf200_finalized.cdi
            new[]
            {
                1
            },

            // test_karaoke_multi_sampler.cdi
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.cdi
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.cdi
            new[]
            {
                1, 2, 3, 4
            },

            // test_multisession_dvd+r.cdi
            new[]
            {
                1
            },

            // test_multisession_dvd-r.cdi
            new[]
            {
                1
            },

            // test_track111_in_session2.cdi
            new[]
            {
                1
            },

            // test_track111_in_session2_fixed_sub.cdi
            new[]
            {
                1
            },

            // test_track2_inside_track1.cdi
            new[]
            {
                1
            },

            // test_videocd.cdi
            new[]
            {
                1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // jaguarcd.cdi
            new ulong[]
            {
                0, 27490, 28088, 78742, 99905, 133054, 160759, 181317, 201875, 222433, 242991
            },

            // make_audiocd.cdi
            new ulong[]
            {
                1
            },

            // make_data_mode1_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_data_mode2_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_dvd.cdi
            new ulong[]
            {
                0
            },

            // make_enhancedcd.cdi
            new ulong[]
            {
                1
            },

            // make_mixed_mode.cdi
            new ulong[]
            {
                1
            },

            // make_multisession_dvd.cdi
            new ulong[]
            {
                1
            },

            // make_pangram_mode1_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_pangram_mode2_joliet.cdi
            new ulong[]
            {
                1
            },

            // pcengine.cdi
            new ulong[]
            {
                0, 3365, 38464, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126004
            },

            // pcfx.cdi
            new ulong[]
            {
                0, 4245, 4759, 5791, 41909, 220645, 225646, 235498
            },

            // report_audiocd.cdi
            new ulong[]
            {
                0, 16399, 30051, 47800, 63164, 78775, 94582, 116975, 136016, 153922, 170601, 186389, 201649, 224299
            },

            // report_cdr.cdi
            new ulong[]
            {
                0
            },

            // report_cdrom.cdi
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.cdi
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.cdi
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.cdi
            new ulong[]
            {
                0
            },

            // report_dvd-r.cdi
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.cdi
            new ulong[]
            {
                0
            },

            // report_dvdrom.cdi
            new ulong[]
            {
                0
            },

            // report_dvd+rw.cdi
            new ulong[]
            {
                0
            },

            // report_dvd-rw.cdi
            new ulong[]
            {
                0
            },

            // report_enhancedcd.cdi
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234030
            },

            // test_audiocd_cdtext.cdi
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_data_track_as_audio.cdi
            new ulong[]
            {
                0, 36789
            },

            // test_data_track_as_audio_fixed_sub.cdi
            new ulong[]
            {
                0, 36789
            },

            // test_disc_starts_at_track2.cdi
            new ulong[]
            {
                0, 36789
            },

            // test_enhancedcd.cdi
            new ulong[]
            {
                0, 14405, 40203
            },

            // test_incd_udf200_finalized.cdi
            new ulong[]
            {
                0
            },

            // test_karaoke_multi_sampler.cdi
            new ulong[]
            {
                0, 1737, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multiple_indexes.cdi
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession.cdi
            new ulong[]
            {
                0, 19383, 32710, 45228
            },

            // test_multisession_dvd+r.cdi
            new ulong[]
            {
                0
            },

            // test_multisession_dvd-r.cdi
            new ulong[]
            {
                0
            },

            // test_track111_in_session2.cdi
            new ulong[]
            {
                1
            },

            // test_track111_in_session2_fixed_sub.cdi
            new ulong[]
            {
                1
            },

            // test_track2_inside_track1.cdi
            new ulong[]
            {
                1
            },

            // test_videocd.cdi
            new ulong[]
            {
                0, 1102
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // jaguarcd.cdi
            new ulong[]
            {
                16239, 28087, 78741, 99904, 133053, 160758, 181316, 201874, 222432, 242990, 243586
            },

            // make_audiocd.cdi
            new ulong[]
            {
                1
            },

            // make_data_mode1_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_data_mode2_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_dvd.cdi
            new ulong[]
            {
                84895
            },

            // make_enhancedcd.cdi
            new ulong[]
            {
                1
            },

            // make_mixed_mode.cdi
            new ulong[]
            {
                1
            },

            // make_multisession_dvd.cdi
            new ulong[]
            {
                1
            },

            // make_pangram_mode1_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_pangram_mode2_joliet.cdi
            new ulong[]
            {
                1
            },

            // pcengine.cdi
            new ulong[]
            {
                3364, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126003, 160955
            },

            // pcfx.cdi
            new ulong[]
            {
                4244, 4758, 5790, 41908, 220644, 225645, 235497, 246679
            },

            // report_audiocd.cdi
            new ulong[]
            {
                16398, 30050, 47799, 63163, 78774, 94581, 116974, 136015, 153921, 170600, 186388, 201648, 224298, 247072
            },

            // report_cdr.cdi
            new ulong[]
            {
                254264
            },

            // report_cdrom.cdi
            new ulong[]
            {
                254264
            },

            // report_cdrw_2x.cdi
            new ulong[]
            {
                308223
            },

            // report_dvdram_v1.cdi
            new ulong[]
            {
                1218960
            },

            // report_dvdram_v2.cdi
            new ulong[]
            {
                2236704
            },

            // report_dvd-r.cdi
            new ulong[]
            {
                2146367
            },

            // report_dvd+r-dl.cdi
            new ulong[]
            {
                3455935
            },

            // report_dvdrom.cdi
            new ulong[]
            {
                2146367
            },

            // report_dvd+rw.cdi
            new ulong[]
            {
                2295103
            },

            // report_dvd-rw.cdi
            new ulong[]
            {
                2146367
            },

            // report_enhancedcd.cdi
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // test_audiocd_cdtext.cdi
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_data_track_as_audio.cdi
            new ulong[]
            {
                25538, 62384
            },

            // test_data_track_as_audio_fixed_sub.cdi
            new ulong[]
            {
                25538, 62384
            },

            // test_disc_starts_at_track2.cdi
            new ulong[]
            {
                25538, 62384
            },

            // test_enhancedcd.cdi
            new ulong[]
            {
                14404, 28952, 59205
            },

            // test_incd_udf200_finalized.cdi
            new ulong[]
            {
                350133
            },

            // test_karaoke_multi_sampler.cdi
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multiple_indexes.cdi
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.cdi
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_multisession_dvd+r.cdi
            new ulong[]
            {
                230623
            },

            // test_multisession_dvd-r.cdi
            new ulong[]
            {
                257263
            },

            // test_track111_in_session2.cdi
            new ulong[]
            {
                1
            },

            // test_track111_in_session2_fixed_sub.cdi
            new ulong[]
            {
                1
            },

            // test_track2_inside_track1.cdi
            new ulong[]
            {
                1
            },

            // test_videocd.cdi
            new ulong[]
            {
                1101, 48793
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // jaguarcd.cdi
            new ulong[]
            {
                150, 150, 149, 150, 149, 149, 149, 149, 149, 149, 149
            },

            // make_audiocd.cdi
            new ulong[]
            {
                1
            },

            // make_data_mode1_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_data_mode2_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_dvd.cdi
            new ulong[]
            {
                0
            },

            // make_enhancedcd.cdi
            new ulong[]
            {
                1
            },

            // make_mixed_mode.cdi
            new ulong[]
            {
                1
            },

            // make_multisession_dvd.cdi
            new ulong[]
            {
                1
            },

            // make_pangram_mode1_joliet.cdi
            new ulong[]
            {
                1
            },

            // make_pangram_mode2_joliet.cdi
            new ulong[]
            {
                1
            },

            // pcengine.cdi
            new ulong[]
            {
                150, 225, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 225
            },

            // pcfx.cdi
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 0, 0
            },

            // report_audiocd.cdi
            new ulong[]
            {
                150, 150, 0, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // report_cdr.cdi
            new ulong[]
            {
                150
            },

            // report_cdrom.cdi
            new ulong[]
            {
                150
            },

            // report_cdrw_2x.cdi
            new ulong[]
            {
                150
            },

            // report_dvdram_v1.cdi
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.cdi
            new ulong[]
            {
                0
            },

            // report_dvd-r.cdi
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.cdi
            new ulong[]
            {
                0
            },

            // report_dvdrom.cdi
            new ulong[]
            {
                0
            },

            // report_dvd+rw.cdi
            new ulong[]
            {
                0
            },

            // report_dvd-rw.cdi
            new ulong[]
            {
                0
            },

            // report_enhancedcd.cdi
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_audiocd_cdtext.cdi
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_data_track_as_audio.cdi
            new ulong[]
            {
                150, 150
            },

            // test_data_track_as_audio_fixed_sub.cdi
            new ulong[]
            {
                150, 150
            },

            // test_disc_starts_at_track2.cdi
            new ulong[]
            {
                150, 150
            },

            // test_enhancedcd.cdi
            new ulong[]
            {
                150, 0, 150
            },

            // test_incd_udf200_finalized.cdi
            new ulong[]
            {
                150
            },

            // test_karaoke_multi_sampler.cdi
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.cdi
            new ulong[]
            {
                150, 0, 0, 0, 0
            },

            // test_multisession.cdi
            new ulong[]
            {
                150, 150, 150, 150
            },

            // test_multisession_dvd+r.cdi
            new ulong[]
            {
                0
            },

            // test_multisession_dvd-r.cdi
            new ulong[]
            {
                0
            },

            // test_track111_in_session2.cdi
            new ulong[]
            {
                1
            },

            // test_track111_in_session2_fixed_sub.cdi
            new ulong[]
            {
                1
            },

            // test_track2_inside_track1.cdi
            new ulong[]
            {
                1
            },

            // test_videocd.cdi
            new ulong[]
            {
                150, 150
            }
        };

        readonly byte[][] _trackFlags =
        {
            // jaguarcd.cdi
            new byte[]
            {
                1
            },

            // make_audiocd.cdi
            new byte[]
            {
                1
            },

            // make_data_mode1_joliet.cdi
            new byte[]
            {
                1
            },

            // make_data_mode2_joliet.cdi
            new byte[]
            {
                1
            },

            // make_dvd.cdi
            new byte[]
            {
                1
            },

            // make_enhancedcd.cdi
            new byte[]
            {
                1
            },

            // make_mixed_mode.cdi
            new byte[]
            {
                1
            },

            // make_multisession_dvd.cdi
            new byte[]
            {
                1
            },

            // make_pangram_mode1_joliet.cdi
            new byte[]
            {
                1
            },

            // make_pangram_mode2_joliet.cdi
            new byte[]
            {
                1
            },

            // pcengine.cdi
            new byte[]
            {
                1
            },

            // pcfx.cdi
            new byte[]
            {
                1
            },

            // report_audiocd.cdi
            new byte[]
            {
                1
            },

            // report_cdr.cdi
            new byte[]
            {
                1
            },

            // report_cdrom.cdi
            new byte[]
            {
                1
            },

            // report_cdrw_2x.cdi
            new byte[]
            {
                1
            },

            // report_dvdram_v1.cdi
            new byte[]
            {
                1
            },

            // report_dvdram_v2.cdi
            new byte[]
            {
                1
            },

            // report_dvd-r.cdi
            new byte[]
            {
                1
            },

            // report_dvd+r-dl.cdi
            new byte[]
            {
                1
            },

            // report_dvdrom.cdi
            new byte[]
            {
                1
            },

            // report_dvd+rw.cdi
            new byte[]
            {
                1
            },

            // report_dvd-rw.cdi
            new byte[]
            {
                1
            },

            // report_enhancedcd.cdi
            new byte[]
            {
                1
            },

            // test_audiocd_cdtext.cdi
            new byte[]
            {
                1
            },

            // test_data_track_as_audio.cdi
            new byte[]
            {
                1
            },

            // test_data_track_as_audio_fixed_sub.cdi
            new byte[]
            {
                1
            },

            // test_disc_starts_at_track2.cdi
            new byte[]
            {
                1
            },

            // test_enhancedcd.cdi
            new byte[]
            {
                1
            },

            // test_incd_udf200_finalized.cdi
            new byte[]
            {
                1
            },

            // test_karaoke_multi_sampler.cdi
            new byte[]
            {
                1
            },

            // test_multiple_indexes.cdi
            new byte[]
            {
                1
            },

            // test_multisession.cdi
            new byte[]
            {
                1
            },

            // test_multisession_dvd+r.cdi
            new byte[]
            {
                1
            },

            // test_multisession_dvd-r.cdi
            new byte[]
            {
                1
            },

            // test_track111_in_session2.cdi
            new byte[]
            {
                1
            },

            // test_track111_in_session2_fixed_sub.cdi
            new byte[]
            {
                1
            },

            // test_track2_inside_track1.cdi
            new byte[]
            {
                1
            },

            // test_videocd.cdi
            new byte[]
            {
                1
            }
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiscJuggler");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new DiscImages.DiscJuggler();
                bool opened = image.Open(filter);

                Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                        Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");

                        Assert.AreEqual(_tracks[i], image.Tracks.Count, $"Tracks: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackSession).Should().
                              BeEquivalentTo(_trackSessions[i], $"Track session: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackStartSector).Should().
                              BeEquivalentTo(_trackStarts[i], $"Track start: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackEndSector).Should().
                              BeEquivalentTo(_trackEnds[i], $"Track end: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackPregap).Should().
                              BeEquivalentTo(_trackPregaps[i], $"Track pregap: {_testFiles[i]}");

                        int trackNo = 0;

                        byte[] flags = new byte[image.Tracks.Count];

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                     SectorTagType.CdTrackFlags)[0];

                            trackNo++;
                        }

                        flags.Should().BeEquivalentTo(_trackFlags[i], $"Track flags: {_testFiles[i]}");
                    });
                }
            }
        }

        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.DiscJuggler();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    Md5Context ctx;

                    foreach(bool @long in new[]
                    {
                        false, true
                    })
                    {
                        ctx = new Md5Context();

                        foreach(Track currentTrack in image.Tracks)
                        {
                            ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                            ulong doneSectors = 0;

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if(sectors - doneSectors >= SECTORS_TO_READ)
                                {
                                    sector =
                                        @long ? image.ReadSectorsLong(doneSectors, SECTORS_TO_READ,
                                                                      currentTrack.TrackSequence)
                                            : image.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                currentTrack.TrackSequence);

                                    doneSectors += SECTORS_TO_READ;
                                }
                                else
                                {
                                    sector =
                                        @long ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                      currentTrack.TrackSequence)
                                            : image.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                currentTrack.TrackSequence);

                                    doneSectors += sectors - doneSectors;
                                }

                                ctx.Update(sector);
                            }
                        }

                        Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(),
                                        $"{(@long ? "Long hash" : "Hash")}: {_testFiles[i]}");
                    }

                    if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        continue;

                    ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = image.ReadSectorsTag(doneSectors, SECTORS_TO_READ, currentTrack.TrackSequence,
                                                              SectorTagType.CdSectorSubchannel);

                                doneSectors += SECTORS_TO_READ;
                            }
                            else
                            {
                                sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                              currentTrack.TrackSequence,
                                                              SectorTagType.CdSectorSubchannel);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(_subchannelMd5S[i], ctx.End(), $"Subchannel hash: {_testFiles[i]}");
                }
            });
        }
    }
}