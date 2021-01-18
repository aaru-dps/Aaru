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
    public class V2
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.nrg", "jaguarcd.nrg", "securdisc.nrg", "report_audiocd.nrg", "report_cdrom.nrg",
            "report_cdrw.nrg", "report_dvd+r-dl.nrg", "report_dvd+rw.nrg", "report_dvdram_v1.nrg",
            "report_dvdram_v2.nrg", "report_dvdrom.nrg", "report_enhancedcd.nrg", "test_audiocd_cdtext.nrg",
            "test_all_tracks_are_track1.nrg", "test_castrated_leadout.nrg", "test_data_track_as_audio.nrg",
            "test_data_track_as_audio_fixed_sub.nrg", "test_incd_udf200_finalized.nrg",
            "test_multi_karaoke_sampler.nrg", "test_multiple_indexes.nrg", "test_multisession.nrg",
            "test_track1_overlaps_session2.nrg", "test_track2_inside_session2_leadin.nrg",
            "test_track2_inside_track1.nrg", "test_videocd.nrg", "make_audiocd_dao.nrg", "make_audiocd_tao.nrg",
            "make_data_dvd_iso9660-1999.nrg", "make_data_dvd_joliet.nrg", "make_data_mode1_iso9660-1999_dao.nrg",
            "make_data_mode1_iso9660-1999_tao.nrg", "make_data_mode1_joliet_dao.nrg", "make_data_mode1_joliet_tao.nrg",
            "make_data_mode1_joliet_udf_102_physical_dao.nrg", "make_data_mode1_joliet_udf_102_physical_tao.nrg",
            "make_data_mode1_joliet_udf_150_physical_dao.nrg", "make_data_mode1_joliet_udf_150_physical_tao.nrg",
            "make_data_mode1_joliet_udf_150_sparing_dao.nrg", "make_data_mode1_joliet_udf_150_sparing_tao.nrg",
            "make_data_mode1_joliet_udf_150_virtual_dao.nrg", "make_data_mode1_joliet_udf_150_virtual_tao.nrg",
            "make_data_mode1_joliet_udf_200_physical_dao.nrg", "make_data_mode1_joliet_udf_200_physical_tao.nrg",
            "make_data_mode1_joliet_udf_200_sparing_dao.nrg", "make_data_mode1_joliet_udf_200_sparing_tao.nrg",
            "make_data_mode1_joliet_udf_200_virtual_dao.nrg", "make_data_mode1_joliet_udf_200_virtual_tao.nrg",
            "make_data_mode1_joliet_udf_201_physical_dao.nrg", "make_data_mode1_joliet_udf_201_physical_tao.nrg",
            "make_data_mode1_joliet_udf_201_sparing_dao.nrg", "make_data_mode1_joliet_udf_201_sparing_tao.nrg",
            "make_data_mode1_joliet_udf_201_virtual_dao.nrg", "make_data_mode1_joliet_udf_201_virtual_tao.nrg",
            "make_data_mode2_iso9660-1999_dao.nrg", "make_data_mode2_iso9660-1999_tao.nrg",
            "make_data_mode2_joliet_dao.nrg", "make_data_mode2_joliet_tao.nrg",
            "make_data_mode2_joliet_udf_102_physical_dao.nrg", "make_data_mode2_joliet_udf_102_physical_tao.nrg",
            "make_data_mode2_joliet_udf_150_physical_dao.nrg", "make_data_mode2_joliet_udf_150_physical_tao.nrg",
            "make_data_mode2_joliet_udf_150_sparing_dao.nrg", "make_data_mode2_joliet_udf_150_sparing_tao.nrg",
            "make_data_mode2_joliet_udf_150_virtual_dao.nrg", "make_data_mode2_joliet_udf_150_virtual_tao.nrg",
            "make_data_mode2_joliet_udf_200_physical_dao.nrg", "make_data_mode2_joliet_udf_200_physical_tao.nrg",
            "make_data_mode2_joliet_udf_200_sparing_dao.nrg", "make_data_mode2_joliet_udf_200_sparing_tao.nrg",
            "make_data_mode2_joliet_udf_200_virtual_dao.nrg", "make_data_mode2_joliet_udf_200_virtual_tao.nrg",
            "make_data_mode2_joliet_udf_201_physical_dao.nrg", "make_data_mode2_joliet_udf_201_physical_tao.nrg",
            "make_data_mode2_joliet_udf_201_sparing_dao.nrg", "make_data_mode2_joliet_udf_201_sparing_tao.nrg",
            "make_data_mode2_joliet_udf_201_virtual_dao.nrg", "make_data_mode2_joliet_udf_201_virtual_tao.nrg",
            "make_data_udf_102_physical_dao.nrg", "make_data_udf_102_physical_tao.nrg",
            "make_data_udf_150_physical_dao.nrg", "make_data_udf_150_physical_tao.nrg",
            "make_data_udf_150_sparing_dao.nrg", "make_data_udf_150_sparing_tao.nrg",
            "make_data_udf_150_virtual_dao.nrg", "make_data_udf_150_virtual_tao.nrg",
            "make_data_udf_200_physical_dao.nrg", "make_data_udf_200_physical_tao.nrg",
            "make_data_udf_200_sparing_dao.nrg", "make_data_udf_200_sparing_tao.nrg",
            "make_data_udf_200_virtual_dao.nrg", "make_data_udf_200_virtual_tao.nrg",
            "make_data_udf_201_physical_dao.nrg", "make_data_udf_201_physical_tao.nrg",
            "make_data_udf_201_sparing_dao.nrg", "make_data_udf_201_sparing_tao.nrg",
            "make_data_udf_201_virtual_dao.nrg", "make_data_udf_201_virtual_tao.nrg", "make_enhancedcd_dao.nrg",
            "make_enhancedcd_tao.nrg", "make_hdburn_full.nrg", "make_hdburn.nrg", "make_mixed_mode_dao.nrg",
            "make_mixed_mode_tao.nrg"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.nrg
            279300,

            // jaguarcd.nrg
            232337,

            // securdisc.nrg
            169536,

            // report_audiocd.nrg
            247073,

            // report_cdrom.nrg
            254265,

            // report_cdrw.nrg
            308224,

            // report_dvd+r-dl.nrg
            3455936,

            // report_dvd+rw.nrg
            2295104,

            // report_dvdram_v1.nrg
            1218960,

            // report_dvdram_v2.nrg
            2236704,

            // report_dvdrom.nrg
            2146368,

            // report_enhancedcd.nrg
            303316,

            // test_audiocd_cdtext.nrg
            277696,

            // test_all_tracks_are_track1
            25689,

            // test_castrated_leadout
            270050,

            // test_data_track_as_audio.nrg
            51135,

            // test_data_track_as_audio_fixed_sub.nrg 
            51135,

            // test_incd_udf200_finalized.nrg
            350134,

            // test_multi_karaoke_sampler.nrg
            329158,

            // test_multiple_indexes.nrg
            65536,

            // test_multisession.nrg
            51168,

            // test_track1_overlaps_session2.nrg
            25539,

            // test_track2_inside_session2_leadin.nrg
            51135,

            // test_track2_inside_track1.nrg
            51135,

            // test_videocd.nrg
            48794,

            // make_audiocd_dao.nrg
            279196,

            // make_audiocd_tao.nrg
            277696,

            // make_data_dvd_iso9660-1999.nrg
            82704,

            // make_data_dvd_joliet.nrg
            83072,

            // make_data_mode1_iso9660-1999_dao.nrg
            82695,

            // make_data_mode1_iso9660-1999_tao.nrg
            82695,

            // make_data_mode1_joliet_dao.nrg
            83068,

            // make_data_mode1_joliet_tao.nrg
            83068,

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            85364,

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            85364,

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            85364,

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            85364,

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            86529,

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            86529,

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            85368,

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            85368,

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            85366,

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            85366,

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            86529,

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            86529,

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            85370,

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            85370,

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            85366,

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            85366,

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            86529,

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            86529,

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            85370,

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            85370,

            // make_data_mode2_iso9660-1999_dao.nrg
            82697,

            // make_data_mode2_iso9660-1999_tao.nrg
            82697,

            // make_data_mode2_joliet_dao.nrg
            83082,

            // make_data_mode2_joliet_tao.nrg
            83082,

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            85378,

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            85378,

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            85378,

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            85378,

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            86529,

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            86529,

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            85382,

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            85382,

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            85380,

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            85380,

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            86529,

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            86529,

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            85384,

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            85384,

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            85380,

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            85380,

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            86529,

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            86529,

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            85384,

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            85384,

            // make_data_udf_102_physical_dao.nrg
            84616,

            // make_data_udf_102_physical_tao.nrg
            84616,

            // make_data_udf_150_physical_dao.nrg
            84616,

            // make_data_udf_150_physical_tao.nrg
            84616,

            // make_data_udf_150_sparing_dao.nrg
            85793,

            // make_data_udf_150_sparing_tao.nrg
            85793,

            // make_data_udf_150_virtual_dao.nrg
            84620,

            // make_data_udf_150_virtual_tao.nrg
            84620,

            // make_data_udf_200_physical_dao.nrg
            84618,

            // make_data_udf_200_physical_tao.nrg
            84618,

            // make_data_udf_200_sparing_dao.nrg
            85793,

            // make_data_udf_200_sparing_tao.nrg
            85793,

            // make_data_udf_200_virtual_dao.nrg
            84622,

            // make_data_udf_200_virtual_tao.nrg
            84622,

            // make_data_udf_201_physical_dao.nrg
            84618,

            // make_data_udf_201_physical_tao.nrg
            84618,

            // make_data_udf_201_sparing_dao.nrg
            85793,

            // make_data_udf_201_sparing_tao.nrg
            85793,

            // make_data_udf_201_virtual_dao.nrg
            84622,

            // make_data_udf_201_virtual_tao.nrg
            84622,

            // make_enhancedcd_dao.nrg
            326011,

            // make_enhancedcd_tao.nrg
            324361,

            // make_hdburn_full.nrg
            727605,

            // make_hdburn.nrg
            31084,

            // make_mixed_mode_dao.nrg
            362041,

            // make_mixed_mode_tao.nrg
            360391
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.nrg
            MediaType.CDDA,

            // jaguarcd.nrg
            MediaType.CDDA,

            // securdisc.nrg
            MediaType.CDROM,

            // report_audiocd.nrg
            MediaType.CDDA,

            // report_cdrom.nrg
            MediaType.CDROM,

            // report_cdrw.nrg
            MediaType.CDROM,

            // report_dvd+r-dl.nrg
            MediaType.DVDROM,

            // report_dvd+rw.nrg
            MediaType.DVDROM,

            // report_dvdram_v1.nrg
            MediaType.DVDROM,

            // report_dvdram_v2.nrg
            MediaType.DVDROM,

            // report_dvdrom.nrg
            MediaType.DVDROM,

            // report_enhancedcd.nrg
            MediaType.CDPLUS,

            // test_audiocd_cdtext.nrg
            MediaType.CDDA,

            // test_all_tracks_are_track1.nrg
            MediaType.CDROMXA,

            // test_castrated_leadout.nrg
            MediaType.CDDA,

            // test_data_track_as_audio.nrg
            MediaType.CDROMXA,

            // test_data_track_as_audio_fixed_sub.nrg 
            MediaType.CDROMXA,

            // test_incd_udf200_finalized.nrg
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.nrg
            MediaType.CDROMXA,

            // test_multiple_indexes.nrg
            MediaType.CDDA,

            // test_multisession.nrg
            MediaType.CDROMXA,

            // test_track1_overlaps_session2.nrg
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

            // make_data_dvd_iso9660-1999.nrg
            MediaType.DVDROM,

            // make_data_dvd_joliet.nrg
            MediaType.DVDROM,

            // make_data_mode1_iso9660-1999_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_iso9660-1999_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            MediaType.CDROM,

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            MediaType.CDROM,

            // make_data_mode2_iso9660-1999_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_iso9660-1999_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            MediaType.CDROMXA,

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            MediaType.CDROMXA,

            // make_data_udf_102_physical_dao.nrg
            MediaType.CDROM,

            // make_data_udf_102_physical_tao.nrg
            MediaType.CDROM,

            // make_data_udf_150_physical_dao.nrg
            MediaType.CDROM,

            // make_data_udf_150_physical_tao.nrg
            MediaType.CDROM,

            // make_data_udf_150_sparing_dao.nrg
            MediaType.CDROM,

            // make_data_udf_150_sparing_tao.nrg
            MediaType.CDROM,

            // make_data_udf_150_virtual_dao.nrg
            MediaType.CDROM,

            // make_data_udf_150_virtual_tao.nrg
            MediaType.CDROM,

            // make_data_udf_200_physical_dao.nrg
            MediaType.CDROM,

            // make_data_udf_200_physical_tao.nrg
            MediaType.CDROM,

            // make_data_udf_200_sparing_dao.nrg
            MediaType.CDROM,

            // make_data_udf_200_sparing_tao.nrg
            MediaType.CDROM,

            // make_data_udf_200_virtual_dao.nrg
            MediaType.CDROM,

            // make_data_udf_200_virtual_tao.nrg
            MediaType.CDROM,

            // make_data_udf_201_physical_dao.nrg
            MediaType.CDROM,

            // make_data_udf_201_physical_tao.nrg
            MediaType.CDROM,

            // make_data_udf_201_sparing_dao.nrg
            MediaType.CDROM,

            // make_data_udf_201_sparing_tao.nrg
            MediaType.CDROM,

            // make_data_udf_201_virtual_dao.nrg
            MediaType.CDROM,

            // make_data_udf_201_virtual_tao.nrg
            MediaType.CDROM,

            // make_enhancedcd_dao.nrg
            MediaType.CDPLUS,

            // make_enhancedcd_tao.nrg
            MediaType.CDPLUS,

            // make_hdburn_full.nrg
            MediaType.CDROM,

            // make_hdburn.nrg
            MediaType.CDROM,

            // make_mixed_mode_dao.nrg
            MediaType.CDROMXA,

            // make_mixed_mode_tao.nrg
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // jaguarcd.nrg
            "79ade978aad90667f272a693012c11ca",

            // securdisc.nrg
            "7119f623e909737e59732b935f103908",

            // report_audiocd.nrg
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdrom.nrg
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.nrg
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_dvd+r-dl.nrg
            "UNKNOWN",

            // report_dvd+rw.nrg
            "UNKNOWN",

            // report_dvdram_v1.nrg
            "UNKNOWN",

            // report_dvdram_v2.nrg
            "UNKNOWN",

            // report_dvdrom.nrg
            "UNKNOWN",

            // report_enhancedcd.nrg
            "dfd6c0bd02c19145b2a64d8a15912302",

            // test_audiocd_cdtext.nrg
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_all_tracks_are_track1
            "UNKNOWN",

            // test_castrated_leadout
            "UNKNOWN",

            // test_data_track_as_audio.nrg
            "d9d46cae2a3a46316c8e1411e84d40ef",

            // test_data_track_as_audio_fixed_sub.nrg 
            "UNKNOWN",

            // test_incd_udf200_finalized.nrg
            "f95d6f978ddb4f98bbffda403f627fe1",

            // test_multi_karaoke_sampler.nrg
            "1731384a29149b7e6f4c0d0d07f178ca",

            // test_multiple_indexes.nrg
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.nrg
            "f793fecc486a83cbe05b51c2d98059b9",

            // test_track1_overlaps_session2.nrg
            "UNKNOWN",

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

            // make_data_dvd_iso9660-1999.nrg
            "UNKNOWN",

            // make_data_dvd_joliet.nrg
            "UNKNOWN",

            // make_data_mode1_iso9660-1999_dao.nrg
            "UNKNOWN",

            // make_data_mode1_iso9660-1999_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_iso9660-1999_dao.nrg
            "UNKNOWN",

            // make_data_mode2_iso9660-1999_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_enhancedcd_dao.nrg
            "UNKNOWN",

            // make_enhancedcd_tao.nrg
            "UNKNOWN",

            // make_hdburn_full.nrg
            "UNKNOWN",

            // make_hdburn.nrg
            "UNKNOWN",

            // make_mixed_mode_dao.nrg
            "UNKNOWN",

            // make_mixed_mode_tao.nrg
            "UNKNOWN"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // jaguarcd.nrg
            "8086a3654d6dede562621d24ae18729e",

            // securdisc.nrg
            "f1c1dbe1cd9df11fe2c1f0a97130c25f",

            // report_audiocd.nrg
            "ff35cfa013871b322ef54612e719c185",

            // report_cdrom.nrg
            "6b4e35ec371770751f26163629253015",

            // report_cdrw.nrg
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_dvd+r-dl.nrg
            "UNKNOWN",

            // report_dvd+rw.nrg
            "UNKNOWN",

            // report_dvdram_v1.nrg
            "UNKNOWN",

            // report_dvdram_v2.nrg
            "UNKNOWN",

            // report_dvdrom.nrg
            "UNKNOWN",

            // report_enhancedcd.nrg
            "0038395e272242a29e84a1fb34a3a15e",

            // test_all_tracks_are_track1
            "UNKNOWN",

            // test_castrated_leadout
            "UNKNOWN",

            // test_audiocd_cdtext.nrg
            "7c8fc7bb768cff15d702ac8cd10108d7",

            // test_data_track_as_audio.nrg
            "b3550e61649ba5276fed8d74f8e512ee",

            // test_data_track_as_audio_fixed_sub.nrg 

            // test_incd_udf200_finalized.nrg
            "6751e0ae7821f92221672b1cd5a1ff36",

            // test_multi_karaoke_sampler.nrg
            "efe2b3fe51022ef8e0a62587294d1d9c",

            // test_multiple_indexes.nrg
            "1b13a8f8aeb23f0b8bbc68518217e771",

            // test_multisession.nrg
            "199b85a01c27f55f463fc7d606adfafa",

            // test_track1_overlaps_session2.nrg
            "UNKNOWN",

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

            // make_data_dvd_iso9660-1999.nrg
            "UNKNOWN",

            // make_data_dvd_joliet.nrg
            "UNKNOWN",

            // make_data_mode1_iso9660-1999_dao.nrg
            "UNKNOWN",

            // make_data_mode1_iso9660-1999_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_iso9660-1999_dao.nrg
            "UNKNOWN",

            // make_data_mode2_iso9660-1999_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_enhancedcd_dao.nrg
            "UNKNOWN",

            // make_enhancedcd_tao.nrg
            "UNKNOWN",

            // make_hdburn_full.nrg
            "UNKNOWN",

            // make_hdburn.nrg
            "UNKNOWN",

            // make_mixed_mode_dao.nrg
            "UNKNOWN",

            // make_mixed_mode_tao.nrg
            "UNKNOWN"
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.nrg
            "UNKNOWN",

            // jaguarcd.nrg
            "83ec1010fc44694d69dc48bacec5481a",

            // securdisc.nrg
            "9e9a6b51bc2e5ec67400cb33ad0ca33f",

            // report_audiocd.nrg
            "9da6ad8f6f0cadd92509c10809da7296",

            // report_cdrom.nrg
            "1994c303674718c74b35f9a4ea1d3515",

            // report_cdrw.nrg
            "6fe81a972e750c68e08f6935e4d91e34",

            // report_dvd+r-dl.nrg
            "UNKNOWN",

            // report_dvd+rw.nrg
            "UNKNOWN",

            // report_dvdram_v1.nrg
            "UNKNOWN",

            // report_dvdram_v2.nrg
            "UNKNOWN",

            // report_dvdrom.nrg
            "UNKNOWN",

            // report_enhancedcd.nrg
            "e6f7319532f46c3fa4fd3569c65546e1",

            // test_all_tracks_are_track1
            "UNKNOWN",

            // test_castrated_leadout
            "UNKNOWN",

            // test_audiocd_cdtext.nrg
            "ca781a7afc4eb77c51f7c551ed45c03c",

            // test_data_track_as_audio.nrg
            "5479a1115bb6481db69fd6262e8c6076",

            // test_data_track_as_audio_fixed_sub.nrg 

            // test_incd_udf200_finalized.nrg
            "65f938f7f9ac34fabd3ab94c14eb76b5",

            // test_multi_karaoke_sampler.nrg
            "f8c96f120cac18c52178b99ef4c4e2a9",

            // test_multiple_indexes.nrg
            "25bae9e30657e2f64a45e5f690e3ae9e",

            // test_multisession.nrg
            "48656afdbc40b6df06486a04a4d62401",

            // test_track1_overlaps_session2.nrg

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

            // make_data_dvd_iso9660-1999.nrg
            "UNKNOWN",

            // make_data_dvd_joliet.nrg
            "UNKNOWN",

            // make_data_mode1_iso9660-1999_dao.nrg
            "UNKNOWN",

            // make_data_mode1_iso9660-1999_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_iso9660-1999_dao.nrg
            "UNKNOWN",

            // make_data_mode2_iso9660-1999_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_102_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_102_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_150_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_150_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_200_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_200_virtual_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_physical_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_physical_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_sparing_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_sparing_tao.nrg
            "UNKNOWN",

            // make_data_udf_201_virtual_dao.nrg
            "UNKNOWN",

            // make_data_udf_201_virtual_tao.nrg
            "UNKNOWN",

            // make_enhancedcd_dao.nrg
            "UNKNOWN",

            // make_enhancedcd_tao.nrg
            "UNKNOWN",

            // make_hdburn_full.nrg
            "UNKNOWN",

            // make_hdburn.nrg
            "UNKNOWN",

            // make_mixed_mode_dao.nrg
            "UNKNOWN",

            // make_mixed_mode_tao.nrg
            "UNKNOWN"
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.nrg
            22,

            // jaguarcd.nrg
            11,

            // securdisc.nrg
            1,

            // report_audiocd.nrg
            14,

            // report_cdrom.nrg
            1,

            // report_cdrw.nrg
            1,

            // report_dvd+r-dl.nrg
            1,

            // report_dvd+rw.nrg
            1,

            // report_dvdram_v1.nrg
            1,

            // report_dvdram_v2.nrg
            1,

            // report_dvdrom.nrg
            1,

            // report_enhancedcd.nrg
            14,

            // test_audiocd_cdtext.nrg
            11,

            // test_all_tracks_are_track1
            2,

            // test_castrated_leadout
            11,

            // test_data_track_as_audio.nrg
            2,

            // test_data_track_as_audio_fixed_sub.nrg 
            2,

            // test_incd_udf200_finalized.nrg
            1,

            // test_multi_karaoke_sampler.nrg
            16,

            // test_multiple_indexes.nrg
            5,

            // test_multisession.nrg
            4,

            // test_track1_overlaps_session2.nrg
            1,

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

            // make_data_dvd_iso9660-1999.nrg
            1,

            // make_data_dvd_joliet.nrg
            1,

            // make_data_mode1_iso9660-1999_dao.nrg
            1,

            // make_data_mode1_iso9660-1999_tao.nrg
            1,

            // make_data_mode1_joliet_dao.nrg
            1,

            // make_data_mode1_joliet_tao.nrg
            1,

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            1,

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            1,

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            1,

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            1,

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            1,

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            1,

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            1,

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            1,

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            1,

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            1,

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            1,

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            1,

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            1,

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            1,

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            1,

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            1,

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            1,

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            1,

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            1,

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            1,

            // make_data_mode2_iso9660-1999_dao.nrg
            1,

            // make_data_mode2_iso9660-1999_tao.nrg
            1,

            // make_data_mode2_joliet_dao.nrg
            1,

            // make_data_mode2_joliet_tao.nrg
            1,

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            1,

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            1,

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            1,

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            1,

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            1,

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            1,

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            1,

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            1,

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            1,

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            1,

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            1,

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            1,

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            1,

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            1,

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            1,

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            1,

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            1,

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            1,

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            1,

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            1,

            // make_data_udf_102_physical_dao.nrg
            1,

            // make_data_udf_102_physical_tao.nrg
            1,

            // make_data_udf_150_physical_dao.nrg
            1,

            // make_data_udf_150_physical_tao.nrg
            1,

            // make_data_udf_150_sparing_dao.nrg
            1,

            // make_data_udf_150_sparing_tao.nrg
            1,

            // make_data_udf_150_virtual_dao.nrg
            1,

            // make_data_udf_150_virtual_tao.nrg
            1,

            // make_data_udf_200_physical_dao.nrg
            1,

            // make_data_udf_200_physical_tao.nrg
            1,

            // make_data_udf_200_sparing_dao.nrg
            1,

            // make_data_udf_200_sparing_tao.nrg
            1,

            // make_data_udf_200_virtual_dao.nrg
            1,

            // make_data_udf_200_virtual_tao.nrg
            1,

            // make_data_udf_201_physical_dao.nrg
            1,

            // make_data_udf_201_physical_tao.nrg
            1,

            // make_data_udf_201_sparing_dao.nrg
            1,

            // make_data_udf_201_sparing_tao.nrg
            1,

            // make_data_udf_201_virtual_dao.nrg
            1,

            // make_data_udf_201_virtual_tao.nrg
            1,

            // make_enhancedcd_dao.nrg
            12,

            // make_enhancedcd_tao.nrg
            12,

            // make_hdburn_full.nrg
            1,

            // make_hdburn.nrg
            1,

            // make_mixed_mode_dao.nrg
            12,

            // make_mixed_mode_tao.nrg
            12
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
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // securdisc.nrg
            new[]
            {
                1
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

            // report_dvd+r-dl.nrg
            new[]
            {
                1
            },

            // report_dvd+rw.nrg
            new[]
            {
                1
            },

            // report_dvdram_v1.nrg
            new[]
            {
                1
            },

            // report_dvdram_v2.nrg
            new[]
            {
                1
            },

            // report_dvdrom.nrg
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

            // test_all_tracks_are_track1
            new[]
            {
                1, 2
            },

            // test_castrated_leadout
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_data_track_as_audio.nrg
            new[]
            {
                1, 2
            },

            // test_data_track_as_audio_fixed_sub.nrg 
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

            // test_track1_overlaps_session2.nrg
            new[]
            {
                1
            },

            // test_track2_inside_session2_leadin.nrg
            new[]
            {
                1, 1, 2
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

            // make_data_dvd_iso9660-1999.nrg
            new[]
            {
                1
            },

            // make_data_dvd_joliet.nrg
            new[]
            {
                1
            },

            // make_data_mode1_iso9660-1999_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_iso9660-1999_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_iso9660-1999_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_iso9660-1999_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_102_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_102_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_150_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_150_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_150_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_150_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_150_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_150_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_200_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_200_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_200_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_200_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_200_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_200_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_201_physical_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_201_physical_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_201_sparing_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_201_sparing_tao.nrg
            new[]
            {
                1
            },

            // make_data_udf_201_virtual_dao.nrg
            new[]
            {
                1
            },

            // make_data_udf_201_virtual_tao.nrg
            new[]
            {
                1
            },

            // make_enhancedcd_dao.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // make_enhancedcd_tao.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // make_hdburn_full.nrg
            new[]
            {
                1
            },

            // make_hdburn.nrg
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
            }
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

            // securdisc.nrg
            new ulong[]
            {
                0
            },

            // report_audiocd.nrg
            new ulong[]
            {
                0, 16399, 29901, 47800, 63164, 78775, 94582, 116975, 136016, 154072, 170751, 186539, 201799, 224449
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

            // report_dvd+r-dl.nrg
            new ulong[]
            {
                0
            },

            // report_dvd+rw.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                0
            },

            // report_dvdrom.nrg
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
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243738, 269900
            },

            // test_all_tracks_are_track1
            new ulong[]
            {
                0, 36789
            },

            // test_castrated_leadout
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243738, 269900
            },

            // test_data_track_as_audio.nrg
            new ulong[]
            {
                0, 36789
            },

            // test_data_track_as_audio_fixed_sub.nrg 
            new ulong[]
            {
                0, 36789
            },

            // test_incd_udf200_finalized.nrg
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multiple_indexes.nrg
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession.nrg
            new ulong[]
            {
                0, 19383, 32710, 45228
            },

            // test_track1_overlaps_session2.nrg
            new ulong[]
            {
                113870
            },

            // test_track2_inside_session2_leadin.nrg
            new ulong[]
            {
                0, 25350, 36789
            },

            // test_track2_inside_track1.nrg
            new ulong[]
            {
                0, 13200, 36789
            },

            // test_videocd.nrg
            new ulong[]
            {
                0, 950
            },

            // make_audiocd_dao.nrg
            new ulong[]
            {
                0, 29902, 65334, 78876, 95680, 126897, 155859, 192735, 223976, 244938, 271250
            },

            // make_audiocd_tao.nrg
            new ulong[]
            {
                0, 29902, 65334, 78876, 95680, 126897, 155859, 192735, 223976, 244938, 271250
            },

            // make_data_dvd_iso9660-1999.nrg
            new ulong[]
            {
                0
            },

            // make_data_dvd_joliet.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_iso9660-1999_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_iso9660-1999_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_iso9660-1999_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_iso9660-1999_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_102_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_102_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_150_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_150_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_150_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_150_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_150_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_150_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_200_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_200_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_200_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_200_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_200_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_200_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_201_physical_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_201_physical_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_201_sparing_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_201_sparing_tao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_201_virtual_dao.nrg
            new ulong[]
            {
                0
            },

            // make_data_udf_201_virtual_tao.nrg
            new ulong[]
            {
                0
            },

            // make_enhancedcd_dao.nrg
            new ulong[]
            {
                0, 29902, 65334, 78876, 95680, 126897, 155859, 192735, 223976, 244938, 271250, 281259
            },

            // make_enhancedcd_tao.nrg
            new ulong[]
            {
                0, 29902, 65334, 78876, 95680, 126897, 155859, 192735, 223976, 244938, 271250, 281259
            },

            // make_hdburn_full.nrg
            new ulong[]
            {
                0
            },

            // make_hdburn.nrg
            new ulong[]
            {
                0
            },

            // make_mixed_mode_dao.nrg
            new ulong[]
            {
                0, 82695, 112747, 148179, 161721, 178525, 209742, 238704, 275580, 296263, 317075, 343387
            },

            // make_mixed_mode_tao.nrg
            new ulong[]
            {
                0, 82695, 112747, 148179, 161721, 178525, 209742, 238704, 275580, 296263, 317075, 343387
            }
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

            // securdisc.nrg
            new ulong[]
            {
                169535
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

            // report_dvd+r-dl.nrg
            new ulong[]
            {
                3455935
            },

            // report_dvd+rw.nrg
            new ulong[]
            {
                2295103
            },

            // report_dvdram_v1.nrg
            new ulong[]
            {
                1218959
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                2236703
            },

            // report_dvdrom.nrg
            new ulong[]
            {
                2146367
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // test_audiocd_cdtext.nrg
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269899, 277845
            },

            // test_all_tracks_are_track1
            new ulong[]
            {
                25538, 37088
            },

            // test_castrated_leadout
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269899, 270199
            },

            // test_data_track_as_audio.nrg
            new ulong[]
            {
                25538, 62534
            },

            // test_data_track_as_audio_fixed_sub.nrg 
            new ulong[]
            {
                25538, 62534
            },

            // test_incd_udf200_finalized.nrg
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multiple_indexes.nrg
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.nrg
            new ulong[]
            {
                8132, 26109, 38627, 51317
            },

            // test_track1_overlaps_session2.nrg
            new ulong[]
            {
                //0
                4294992834
            },

            // test_track2_inside_session2_leadin.nrg
            new ulong[]
            {
                25349, 25688, 62534
            },

            // test_track2_inside_track1.nrg
            new ulong[]
            {
                13199, 25688, 62534
            },

            // test_videocd.nrg
            new ulong[]
            {
                949, 49095
            },

            // make_audiocd_dao.nrg
            new ulong[]
            {
                29901, 65483, 79025, 95829, 127046, 156008, 192884, 224125, 244937, 271399, 279495
            },

            // make_audiocd_tao.nrg
            new ulong[]
            {
                29901, 65483, 79025, 95829, 127046, 156008, 192884, 224125, 244937, 271399, 279495
            },

            // make_data_dvd_iso9660-1999.nrg
            new ulong[]
            {
                82703
            },

            // make_data_dvd_joliet.nrg
            new ulong[]
            {
                83071
            },

            // make_data_mode1_iso9660-1999_dao.nrg
            new ulong[]
            {
                82694
            },

            // make_data_mode1_iso9660-1999_tao.nrg
            new ulong[]
            {
                82694
            },

            // make_data_mode1_joliet_dao.nrg
            new ulong[]
            {
                83067
            },

            // make_data_mode1_joliet_tao.nrg
            new ulong[]
            {
                83067
            },

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            new ulong[]
            {
                85363
            },

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            new ulong[]
            {
                85363
            },

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            new ulong[]
            {
                85363
            },

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            new ulong[]
            {
                85363
            },

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            new ulong[]
            {
                85367
            },

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            new ulong[]
            {
                85367
            },

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            new ulong[]
            {
                85365
            },

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            new ulong[]
            {
                85365
            },

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            new ulong[]
            {
                85369
            },

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            new ulong[]
            {
                85369
            },

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            new ulong[]
            {
                85365
            },

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            new ulong[]
            {
                85365
            },

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            new ulong[]
            {
                85369
            },

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            new ulong[]
            {
                85369
            },

            // make_data_mode2_iso9660-1999_dao.nrg
            new ulong[]
            {
                82696
            },

            // make_data_mode2_iso9660-1999_tao.nrg
            new ulong[]
            {
                82696
            },

            // make_data_mode2_joliet_dao.nrg
            new ulong[]
            {
                83081
            },

            // make_data_mode2_joliet_tao.nrg
            new ulong[]
            {
                83081
            },

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            new ulong[]
            {
                85377
            },

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            new ulong[]
            {
                85377
            },

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            new ulong[]
            {
                85377
            },

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            new ulong[]
            {
                85377
            },

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            new ulong[]
            {
                85381
            },

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            new ulong[]
            {
                85381
            },

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            new ulong[]
            {
                85379
            },

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            new ulong[]
            {
                85379
            },

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            new ulong[]
            {
                85383
            },

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            new ulong[]
            {
                85383
            },

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            new ulong[]
            {
                85379
            },

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            new ulong[]
            {
                85379
            },

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            new ulong[]
            {
                86528
            },

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            new ulong[]
            {
                85383
            },

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            new ulong[]
            {
                85383
            },

            // make_data_udf_102_physical_dao.nrg
            new ulong[]
            {
                84615
            },

            // make_data_udf_102_physical_tao.nrg
            new ulong[]
            {
                84615
            },

            // make_data_udf_150_physical_dao.nrg
            new ulong[]
            {
                84615
            },

            // make_data_udf_150_physical_tao.nrg
            new ulong[]
            {
                84615
            },

            // make_data_udf_150_sparing_dao.nrg
            new ulong[]
            {
                85792
            },

            // make_data_udf_150_sparing_tao.nrg
            new ulong[]
            {
                85792
            },

            // make_data_udf_150_virtual_dao.nrg
            new ulong[]
            {
                84619
            },

            // make_data_udf_150_virtual_tao.nrg
            new ulong[]
            {
                84619
            },

            // make_data_udf_200_physical_dao.nrg
            new ulong[]
            {
                84617
            },

            // make_data_udf_200_physical_tao.nrg
            new ulong[]
            {
                84617
            },

            // make_data_udf_200_sparing_dao.nrg
            new ulong[]
            {
                85792
            },

            // make_data_udf_200_sparing_tao.nrg
            new ulong[]
            {
                85792
            },

            // make_data_udf_200_virtual_dao.nrg
            new ulong[]
            {
                84621
            },

            // make_data_udf_200_virtual_tao.nrg
            new ulong[]
            {
                84621
            },

            // make_data_udf_201_physical_dao.nrg
            new ulong[]
            {
                84617
            },

            // make_data_udf_201_physical_tao.nrg
            new ulong[]
            {
                84617
            },

            // make_data_udf_201_sparing_dao.nrg
            new ulong[]
            {
                85792
            },

            // make_data_udf_201_sparing_tao.nrg
            new ulong[]
            {
                85792
            },

            // make_data_udf_201_virtual_dao.nrg
            new ulong[]
            {
                84621
            },

            // make_data_udf_201_virtual_tao.nrg
            new ulong[]
            {
                84621
            },

            // make_enhancedcd_dao.nrg
            new ulong[]
            {
                29901, 65483, 79025, 95829, 127046, 156008, 192884, 224125, 244937, 271399, 279495, 328223
            },

            // make_enhancedcd_tao.nrg
            new ulong[]
            {
                29901, 65483, 79025, 95829, 127046, 156008, 192884, 224125, 244937, 271399, 279495, 328223
            },

            // make_hdburn_full.nrg
            new ulong[]
            {
                727604
            },

            // make_hdburn.nrg
            new ulong[]
            {
                31083
            },

            // make_mixed_mode_dao.nrg
            new ulong[]
            {
                82694, 112896, 148328, 161870, 178674, 209891, 238853, 275729, 306970, 317224, 343536, 351632
            },

            // make_mixed_mode_tao.nrg
            new ulong[]
            {
                82694, 112896, 148328, 161870, 178674, 209891, 238853, 275729, 306970, 317224, 343536, 351632
            }
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
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // securdisc.nrg
            new ulong[]
            {
                150
            },

            // report_audiocd.nrg
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
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

            // report_dvd+r-dl.nrg
            new ulong[]
            {
                0
            },

            // report_dvd+rw.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                0
            },

            // report_dvdrom.nrg
            new ulong[]
            {
                0
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

            // test_all_tracks_are_track1
            new ulong[]
            {
                150, 150
            },

            // test_castrated_leadout
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_data_track_as_audio.nrg
            new ulong[]
            {
                150, 150
            },

            // test_data_track_as_audio_fixed_sub.nrg 
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

            // test_track1_overlaps_session2.nrg
            new ulong[]
            {
                114020
            },

            // test_track2_inside_session2_leadin.nrg
            new ulong[]
            {
                150, 150, 150
            },

            // test_track2_inside_track1.nrg
            new ulong[]
            {
                150, 150, 150
            },

            // test_videocd.nrg
            new ulong[]
            {
                150, 302
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

            // make_data_dvd_iso9660-1999.nrg
            new ulong[]
            {
                0
            },

            // make_data_dvd_joliet.nrg
            new ulong[]
            {
                0
            },

            // make_data_mode1_iso9660-1999_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_iso9660-1999_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_iso9660-1999_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_iso9660-1999_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_102_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_102_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_150_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_150_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_150_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_150_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_150_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_150_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_200_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_200_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_200_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_200_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_200_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_200_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_201_physical_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_201_physical_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_201_sparing_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_201_sparing_tao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_201_virtual_dao.nrg
            new ulong[]
            {
                150
            },

            // make_data_udf_201_virtual_tao.nrg
            new ulong[]
            {
                150
            },

            // make_enhancedcd_dao.nrg
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },

            // make_enhancedcd_tao.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // make_hdburn_full.nrg
            new ulong[]
            {
                150
            },

            // make_hdburn.nrg
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
            }
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

            // securdisc.nrg
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
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

            // report_dvd+r-dl.nrg
            new byte[]
            {
                0
            },

            // report_dvd+rw.nrg
            new byte[]
            {
                0
            },

            // report_dvdram_v1.nrg
            new byte[]
            {
                0
            },

            // report_dvdram_v2.nrg
            new byte[]
            {
                0
            },

            // report_dvdrom.nrg
            new byte[]
            {
                0
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

            // test_all_tracks_are_track1
            new byte[]
            {
                0
            },

            // test_castrated_leadout
            new byte[]
            {
                0
            },

            // test_data_track_as_audio.nrg
            new byte[]
            {
                4, 2
            },

            // test_data_track_as_audio_fixed_sub.nrg 
            new byte[]
            {
                0
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

            // test_track1_overlaps_session2.nrg
            new byte[]
            {
                0
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
                0
            },

            // make_audiocd_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_dvd_iso9660-1999.nrg
            new byte[]
            {
                0
            },

            // make_data_dvd_joliet.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_iso9660-1999_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_iso9660-1999_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_102_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_102_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_150_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_200_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode1_joliet_udf_201_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_iso9660-1999_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_iso9660-1999_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_102_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_102_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_150_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_200_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_mode2_joliet_udf_201_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_102_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_102_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_150_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_150_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_150_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_150_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_150_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_150_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_200_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_200_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_200_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_200_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_200_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_200_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_201_physical_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_201_physical_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_201_sparing_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_201_sparing_tao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_201_virtual_dao.nrg
            new byte[]
            {
                0
            },

            // make_data_udf_201_virtual_tao.nrg
            new byte[]
            {
                0
            },

            // make_enhancedcd_dao.nrg
            new byte[]
            {
                0
            },

            // make_enhancedcd_tao.nrg
            new byte[]
            {
                0
            },

            // make_hdburn_full.nrg
            new byte[]
            {
                0
            },

            // make_hdburn.nrg
            new byte[]
            {
                0
            },

            // make_mixed_mode_dao.nrg
            new byte[]
            {
                0
            },

            // make_mixed_mode_tao.nrg
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

            Environment.CurrentDirectory =
                Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Nero Burning ROM", "V2");

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