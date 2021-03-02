// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : v3.cs
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

namespace Aaru.Tests.Images.MAME
{
    [TestFixture]
    public class V3 : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "gigarec.chd", "hdd.chd", "pcengine.chd", "pcfx.chd", "report_audiocd.chd", "report_cdr.chd",
            "report_cdrom.chd", "report_cdrw.chd", "test_enhancedcd.chd", "test_multi_karaoke_sample.chd",
            "test_multisession.chd", "test_videocd.chd"
        };
        public override ulong[] _sectors => new ulong[]
        {
            // gigarec.chd
            469652,

            // hdd.chd
            251904,

            // pcengine.chd
            160506,

            // pcfx.chd
            246380,

            // report_audiocd.chd
            247073,

            // report_cdr.chd
            254265,

            // report_cdrom.chd
            254265,

            // report_cdrw.chd
            308224,

            // test_enhancedcd.chd
            28953,

            // test_multi_karaoke_sample.chd
            329008,

            // test_multisession.chd
            8133,

            // test_videocd.chd
            48794
        };
        public override uint[] _sectorSize => new uint[]
        {
            // gigarec.chd
            2048,

            // hdd.chd
            512,

            // pcengine.chd
            2352,

            // pcfx.chd
            2352,

            // report_audiocd.chd
            2352,

            // report_cdr.chd
            2048,

            // report_cdrom.chd
            2048,

            // report_cdrw.chd
            2048,

            // test_enhancedcd.chd
            2352,

            // test_multi_karaoke_sample.chd
            2352,

            // test_multisession.chd
            2048,

            // test_videocd.chd
            2336
        };
        public override MediaType[] _mediaTypes => new[]
        {
            // gigarec.chd
            MediaType.CDROM,

            // hdd.chd
            MediaType.GENERIC_HDD,

            // pcengine.chd
            MediaType.CDROM,

            // pcfx.chd
            MediaType.CDROM,

            // report_audiocd.chd
            MediaType.CDROM,

            // report_cdr.chd
            MediaType.CDROM,

            // report_cdrom.chd
            MediaType.CDROM,

            // report_cdrw.chd
            MediaType.CDROM,

            // test_enhancedcd.chd
            MediaType.CDROM,

            // test_multi_karaoke_sample.chd
            MediaType.CDROM,

            // test_multisession.chd
            MediaType.CDROM,

            // test_videocd.chd
            MediaType.CDROM
        };

        public override string[] _md5S => new[]
        {
            // gigarec.chd
            "UNKNOWN",

            // hdd.chd
            "43476343f53a177dd57b68dd769917aa",

            // pcengine.chd
            "UNKNOWN",

            // pcfx.chd
            "UNKNOWN",

            // report_audiocd.chd
            "UNKNOWN",

            // report_cdr.chd
            "UNKNOWN",

            // report_cdrom.chd
            "UNKNOWN",

            // report_cdrw.chd
            "UNKNOWN",

            // test_enhancedcd.chd
            "UNKNOWN",

            // test_multi_karaoke_sample.chd
            "UNKNOWN",

            // test_multisession.chd
            "UNKNOWN",

            // test_videocd.chd
            "UNKNOWN"
        };

        public override string[] _longMd5S => new[]
        {
            // gigarec.chd
            "UNKNOWN",

            // hdd.chd
            null,

            // pcengine.chd
            "UNKNOWN",

            // pcfx.chd
            "UNKNOWN",

            // report_audiocd.chd
            "UNKNOWN",

            // report_cdr.chd
            "UNKNOWN",

            // report_cdrom.chd
            "UNKNOWN",

            // report_cdrw.chd
            "UNKNOWN",

            // test_enhancedcd.chd
            "UNKNOWN",

            // test_multi_karaoke_sample.chd
            "UNKNOWN",

            // test_multisession.chd
            "UNKNOWN",

            // test_videocd.chd
            "UNKNOWN"
        };

        public override string[] _subchannelMd5S => new[]
        {
            // gigarec.chd
            "UNKNOWN",

            // hdd.chd
            null,

            // pcengine.chd
            "UNKNOWN",

            // pcfx.chd
            "UNKNOWN",

            // report_audiocd.chd
            "UNKNOWN",

            // report_cdr.chd
            "UNKNOWN",

            // report_cdrom.chd
            "UNKNOWN",

            // report_cdrw.chd
            "UNKNOWN",

            // test_enhancedcd.chd
            "UNKNOWN",

            // test_multi_karaoke_sample.chd
            "UNKNOWN",

            // test_multisession.chd
            "UNKNOWN",

            // test_videocd.chd
            "UNKNOWN"
        };

        public override int[] _tracks => new[]
        {
            // gigarec.chd
            1,

            // hdd.chd
            -1,

            // pcengine.chd
            16,

            // pcfx.chd
            8,

            // report_audiocd.chd
            14,

            // report_cdr.chd
            1,

            // report_cdrom.chd
            1,

            // report_cdrw.chd
            1,

            // test_enhancedcd.chd
            2,

            // test_multi_karaoke_sample.chd
            16,

            // test_multisession.chd
            1,

            // test_videocd.chd
            2
        };

        public override int[][] _trackSessions => new[]
        {
            // gigarec.chd
            new[]
            {
                1
            },

            // hdd.chd
            null,

            // pcengine.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.chd
            new[]
            {
                1
            },

            // report_cdrom.chd
            new[]
            {
                1
            },

            // report_cdrw.chd
            new[]
            {
                1
            },

            // test_enhancedcd.chd
            new[]
            {
                1, 1
            },

            // test_multi_karaoke_sample.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multisession.chd
            new[]
            {
                1
            },

            // test_videocd.chd
            new[]
            {
                1, 1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // gigarec.chd
            new ulong[]
            {
                0
            },

            // hdd.chd
            null,

            // pcengine.chd
            new ulong[]
            {
                0, 3440, 38316, 46920, 53204, 61524, 68268, 75104, 82840, 86192, 90980, 98988, 106408, 111956, 119988,
                125800
            },

            // pcfx.chd
            new ulong[]
            {
                0, 4248, 4764, 5796, 41916, 220504, 225356, 235208
            },

            // report_audiocd.chd
            new ulong[]
            {
                0, 16552, 30056, 47956, 63320, 78932, 94740, 117136, 136180, 154088, 170768, 186556, 201816, 224468
            },

            // report_cdr.chd
            new ulong[]
            {
                0
            },

            // report_cdrom.chd
            new ulong[]
            {
                0
            },

            // report_cdrw.chd
            new ulong[]
            {
                0
            },

            // test_enhancedcd.chd
            new ulong[]
            {
                0, 14408
            },

            // test_multi_karaoke_sample.chd
            new ulong[]
            {
                0, 1740, 32604, 52528, 70160, 99956, 119620, 136860, 155652, 175688, 206324, 226316, 244224, 273836,
                293624, 310584
            },

            // test_multisession.chd
            new ulong[]
            {
                0
            },

            // test_videocd.chd
            new ulong[]
            {
                0, 1252
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // gigarec.chd
            new ulong[]
            {
                469651
            },

            // hdd.chd
            null,

            // pcengine.chd
            new ulong[]
            {
                3439, 38313, 46918, 53203, 61521, 68267, 75101, 82836, 86190, 90977, 98986, 106406, 111952, 119987,
                125796, 160526
            },

            // pcfx.chd
            new ulong[]
            {
                4244, 4761, 5795, 41913, 220501, 225354, 235207, 246389
            },

            // report_audiocd.chd
            new ulong[]
            {
                16548, 30053, 47954, 63319, 78930, 94738, 117132, 136176, 154085, 170766, 186555, 201815, 224465, 247091
            },

            // report_cdr.chd
            new ulong[]
            {
                254264
            },

            // report_cdrom.chd
            new ulong[]
            {
                254264
            },

            // report_cdrw.chd
            new ulong[]
            {
                308223
            },

            // test_enhancedcd.chd
            new ulong[]
            {
                14404, 28955
            },

            // test_multi_karaoke_sample.chd
            new ulong[]
            {
                1736, 32601, 52526, 70159, 99953, 119618, 136857, 155650, 175687, 206322, 226312, 244220, 273833,
                293622, 310582, 329030
            },

            // test_multisession.chd
            new ulong[]
            {
                8132
            },

            // test_videocd.chd
            new ulong[]
            {
                1251, 48793
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // gigarec.chd
            new ulong[]
            {
                0
            },

            // hdd.chd
            null,

            // pcengine.chd
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.chd
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.chd
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.chd
            new ulong[]
            {
                0
            },

            // report_cdrom.chd
            new ulong[]
            {
                0
            },

            // report_cdrw.chd
            new ulong[]
            {
                0
            },

            // test_enhancedcd.chd
            new ulong[]
            {
                0, 0
            },

            // test_multi_karaoke_sample.chd
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multisession.chd
            new ulong[]
            {
                0
            },

            // test_videocd.chd
            new ulong[]
            {
                0, 0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // gigarec.chd
            new byte[]
            {
                0
            },

            // hdd.chd
            new byte[]
            {
                0
            },

            // pcengine.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.chd
            new byte[]
            {
                0
            },

            // report_cdrom.chd
            new byte[]
            {
                0
            },

            // report_cdrw.chd
            new byte[]
            {
                0
            },

            // test_enhancedcd.chd
            new byte[]
            {
                0, 0
            },

            // test_multi_karaoke_sample.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multisession.chd
            new byte[]
            {
                0
            },

            // test_videocd.chd
            new byte[]
            {
                0, 0
            }
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "MAME", "v3");
        public override IMediaImage _plugin => new Chd();
    }
}