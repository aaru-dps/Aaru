// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cuesheet.cs
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

namespace Aaru.Tests.Images.UltraISO
{
    [TestFixture]
    public class Cuesheet : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "cdiready_the_apprentice.cue", "report_audiocd.cue", "report_cdrom.cue", "report_cdrw.cue",
            "report_dvdram_v2.cue", "report_dvd-r+dl.cue", "report_dvdrom.cue", "report_enhancedcd.cue",
            "test_multi_karaoke_sampler.cue"
        };
        public override uint[] _sectorSize => null;

        public override ulong[] _sectors => new ulong[]
        {
            // cdiready_the_apprentice.cue
            210150,

            // report_audiocd.cue
            247073,

            // report_cdrom.cue
            254265,

            // report_cdrw.cue
            308224,

            // report_dvdram_v2.cue
            471090,

            // report_dvd-r+dl.cue
            3455920,

            // report_dvdrom.cue
            2146357,

            // report_enhancedcd.cue
            303616,

            // test_multi_karaoke_sampler.cue
            329158
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // cdiready_the_apprentice.cue
            MediaType.CDDA,

            // report_audiocd.cue
            MediaType.CDDA,

            // report_cdrom.cue
            MediaType.CDROM,

            // report_cdrw.cue
            MediaType.CDROM,

            // report_dvdram_v2.cue
            MediaType.CDROM,

            // report_dvd-r+dl.cue
            MediaType.CDROM,

            // report_dvdrom.cue
            MediaType.CDROM,

            // report_enhancedcd.cue
            MediaType.CDPLUS,

            // test_multi_karaoke_sampler.cue
            MediaType.CDROMXA
        };

        public override string[] _md5S => new[]
        {
            // cdiready_the_apprentice.cue
            "d3b069721052a1093151c6f7504ca593",

            // report_audiocd.cue
            "c041297aca68c206d95d20aa9435e01b",

            // report_cdrom.cue
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.cue
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_dvdram_v2.cue
            "35cb08dd5fedfb8e9ad2918292e51791",

            // report_dvd-r+dl.cue
            "ea4cfa28a4e449d7b59251b98394c7f4",

            // report_dvdrom.cue
            "5e1841b7cd6ac0a95b8ae6f110fd89f2",

            // report_enhancedcd.cue
            "026acd68cecc7b2d49a3f9a42312a18f",

            // test_multi_karaoke_sampler.cue
            "9a19aa0df066732a8ec34025e8160248"
        };

        public override string[] _longMd5S => new[]
        {
            // cdiready_the_apprentice.cue
            "d3b069721052a1093151c6f7504ca593",

            // report_audiocd.cue
            "c041297aca68c206d95d20aa9435e01b",

            // report_cdrom.cue
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.cue
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_dvdram_v2.cue
            "c7ee3dc509bb40948c383686b6f66da9",

            // report_dvd-r+dl.cue
            "282de41e0118781f8a9216b0a4a31088",

            // report_dvdrom.cue
            "8325ba263cfa419f9566de93e55248d5",

            // report_enhancedcd.cue
            "31a4c8805b6e8fa7edf93d41b1785661",

            // test_multi_karaoke_sampler.cue
            "e981f7dfdb522ba937fe75474e23a446"
        };

        public override string[] _subchannelMd5S => new string[]
        {
            // cdiready_the_apprentice.cue
            null,

            // report_audiocd.cue
            null,

            // report_cdrom.cue
            null,

            // report_cdrw.cue
            null,

            // report_dvdram_v2.cue
            null,

            // report_dvd-r+dl.cue
            null,

            // report_dvdrom.cue
            null,

            // report_enhancedcd.cue
            null,

            // test_multi_karaoke_sampler.cue
            null
        };

        public override int[] _tracks => new[]
        {
            // cdiready_the_apprentice.cue
            22,

            // report_audiocd.cue
            14,

            // report_cdrom.cue
            1,

            // report_cdrw.cue
            1,

            // report_dvdram_v2.cue
            1,

            // report_dvd-r+dl.cue
            1,

            // report_dvdrom.cue
            1,

            // report_enhancedcd.cue
            14,

            // test_multi_karaoke_sampler.cue
            16
        };

        public override int[][] _trackSessions => new[]
        {
            // cdiready_the_apprentice.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.cue
            new[]
            {
                1
            },

            // report_cdrw.cue
            new[]
            {
                1
            },

            // report_dvdram_v2.cue
            new[]
            {
                1
            },

            // report_dvd-r+dl.cue
            new[]
            {
                1
            },

            // report_dvdrom.cue
            new[]
            {
                1
            },

            // report_enhancedcd.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // cdiready_the_apprentice.cue
            new ulong[]
            {
                69150, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // report_audiocd.cue
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449U
            },

            // report_cdrom.cue
            new ulong[]
            {
                0
            },

            // report_cdrw.cue
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.cue
            new ulong[]
            {
                0
            },

            // report_dvd-r+dl.cue
            new ulong[]
            {
                0
            },

            // report_dvdrom.cue
            new ulong[]
            {
                0
            },

            // report_enhancedcd.cue
            new ulong[]
            {
                0, 15511, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234030
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // cdiready_the_apprentice.cue
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279299
            },

            // report_audiocd.cue
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdrom.cue
            new ulong[]
            {
                254264
            },

            // report_cdrw.cue
            new ulong[]
            {
                308223
            },

            // report_dvdram_v2.cue
            new ulong[]
            {
                471089
            },

            // report_dvd-r+dl.cue
            new ulong[]
            {
                3455919
            },

            // report_dvdrom.cue
            new ulong[]
            {
                2146356
            },

            // report_enhancedcd.cue
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 234179,
                303315
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // cdiready_the_apprentice.cue
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.cue
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.cue
            new ulong[]
            {
                150
            },

            // report_cdrw.cue
            new ulong[]
            {
                150
            },

            // report_dvdram_v2.cue
            new ulong[]
            {
                150
            },

            // report_dvd-r+dl.cue
            new ulong[]
            {
                150
            },

            // report_dvdrom.cue
            new ulong[]
            {
                150
            },

            // report_enhancedcd.cue
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // cdiready_the_apprentice.cue
            new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00
            },

            // report_audiocd.cue
            new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            },

            // report_cdrom.cue
            new byte[]
            {
                4
            },

            // report_cdrw.cue
            new byte[]
            {
                4
            },

            // report_dvdram_v2.cue
            new byte[]
            {
                4
            },

            // report_dvd-r+dl.cue
            new byte[]
            {
                4
            },

            // report_dvdrom.cue
            new byte[]
            {
                4
            },

            // report_enhancedcd.cue
            new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04
            },

            // test_multi_karaoke_sampler.cue
            new byte[]
            {
                0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            }
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "UltraISO", "Cuesheet");
        public override IMediaImage _plugin => new CdrWin();
    }
}