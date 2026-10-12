// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : uncompressed.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images.MAME.v5;

[TestFixture]
public class Uncompressed : OpticalMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "MAME", "v5", "uncompressed");

    public override IMediaImage Plugin => new Chd();

    public override OpticalImageTestExpected[] Tests =>
    [
        new()
        {
            TestFile      = "gigarec.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 469652,
            SectorSize    = 2048,
            Md5           = "cafcd998298d8d1d447ab2e567638c9e",
            LongMd5       = "092b4207604685c6b274068b6eca52c0",
            SubchannelMd5 = "607c53d8ea5589bbc8a3e4ff5857cb5d",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 469651,
                    Pregap  = 150,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "jaguarcd.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 243587,
            SectorSize = 2352,
            Md5        = "8f4c4e5720b63b7369a0c40ab04e1542",
            LongMd5    = "8f4c4e5720b63b7369a0c40ab04e1542",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 27639,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 27640,
                    End     = 28236,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 28237,
                    End     = 78891,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78892,
                    End     = 100053,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 100054,
                    End     = 133202,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 133203,
                    End     = 160907,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 160908,
                    End     = 181465,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 181466,
                    End     = 202023,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 202024,
                    End     = 222581,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222582,
                    End     = 243139,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243140,
                    End     = 243586,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile   = "pcengine.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 160956,
            SectorSize = 2352,
            Md5        = "04a8a46f4db34fdbbd71ccd55010491d",
            LongMd5    = "48a23baf25b7e9a380769c743bbf2594",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 3589,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 3590,
                    End     = 38613,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 38614,
                    End     = 47216,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 47217,
                    End     = 53500,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 53501,
                    End     = 61818,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 61819,
                    End     = 68562,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 68563,
                    End     = 75396,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 75397,
                    End     = 83129,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 83130,
                    End     = 86480,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 86481,
                    End     = 91266,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 91267,
                    End     = 99273,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 99274,
                    End     = 106692,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 106693,
                    End     = 112237,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 112238,
                    End     = 120269,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 120270,
                    End     = 126228,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126229,
                    End     = 160955,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "pcfx.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 246680,
            SectorSize = 2352,
            Md5        = "8f8ff0fe5fc4480ba4cdee69ba185778",
            LongMd5    = "787b5020c842fe67d034d48377d11ac1",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4394,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4395,
                    End     = 4908,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4909,
                    End     = 5940,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 5941,
                    End     = 42058,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 42059,
                    End     = 220794,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 220795,
                    End     = 225645,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 225646,
                    End     = 235497,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 235498,
                    End     = 246679,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile      = "report_audiocd.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 247073,
            SectorSize    = 2352,
            Md5           = "1787cdeab550752d159a8ecca62fe386",
            LongMd5       = "1787cdeab550752d159a8ecca62fe386",
            SubchannelMd5 = "4fa3d7dd5e1710878ac318986a677fb3",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 16548,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 16549,
                    End     = 30050,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 30051,
                    End     = 47949,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 47950,
                    End     = 63313,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 63314,
                    End     = 78924,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78925,
                    End     = 94731,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 94732,
                    End     = 117124,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 117125,
                    End     = 136165,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136166,
                    End     = 154071,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 154072,
                    End     = 170750,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 170751,
                    End     = 186538,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 186539,
                    End     = 201798,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 201799,
                    End     = 224448,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 224449,
                    End     = 247072,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile      = "report_cdr.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            SectorSize    = 2048,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "d81b6abcfa5f84ba0647f38f86cc8fc8",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 150,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile      = "report_cdrom.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            SectorSize    = 2048,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "935c5d899c1cf0e46fa571df99400253",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 150,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile      = "report_cdrw.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 308224,
            SectorSize    = 2048,
            Md5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMd5 = "2e9b8107254e46b7be689105f680ee84",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 308223,
                    Pregap  = 150,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "test_audiocd_cdtext.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 277696,
            SectorSize = 2352,
            Md5        = "7c8fc7bb768cff15d702ac8cd10108d7",
            LongMd5    = "7c8fc7bb768cff15d702ac8cd10108d7",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 29901,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 29902,
                    End     = 65183,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65184,
                    End     = 78575,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78576,
                    End     = 95229,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95230,
                    End     = 126296,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126297,
                    End     = 155108,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155109,
                    End     = 191834,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 191835,
                    End     = 222925,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222926,
                    End     = 243587,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243588,
                    End     = 269749,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269750,
                    End     = 277695,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile   = "test_enhancedcd.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 59206,
            SectorSize = 2352,
            Md5        = "c2265c2875888923f5716c95e47fc2a7",
            LongMd5    = "bd3458a3e9a5d969e308f1e9f0f9689e",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 14254,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 14255,
                    End     = 40352,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 40353,
                    End     = 59205,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "test_incd_udf200_finalized.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 350134,
            SectorSize = 2336,
            Md5        = "7b3e4a952c369cd4837cee40f1a567f2",
            LongMd5    = "d6555969dd70fb2772cd5b979c6fa284",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 350133,
                    Pregap  = 150,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "test_multi_karaoke_sampler.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 329158,
            SectorSize = 2352,
            Md5        = "9a19aa0df066732a8ec34025e8160248",
            LongMd5    = "e981f7dfdb522ba937fe75474e23a446",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1886,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1887,
                    End     = 32748,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 32749,
                    End     = 52671,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 52672,
                    End     = 70303,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 70304,
                    End     = 100097,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 100098,
                    End     = 119760,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 119761,
                    End     = 136998,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136999,
                    End     = 155789,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155790,
                    End     = 175825,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 175826,
                    End     = 206460,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 206461,
                    End     = 226449,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 226450,
                    End     = 244354,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 244355,
                    End     = 273964,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 273965,
                    End     = 293751,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 293752,
                    End     = 310710,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 310711,
                    End     = 329157,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile   = "test_multiple_indexes.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 65536,
            SectorSize = 2352,
            Md5        = "1b13a8f8aeb23f0b8bbc68518217e771",
            LongMd5    = "1b13a8f8aeb23f0b8bbc68518217e771",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4803,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4804,
                    End     = 13874,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13875,
                    End     = 41184,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 41185,
                    End     = 54988,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 54989,
                    End     = 65535,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile   = "test_multisession.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 51168,
            SectorSize = 2336,
            Md5        = "3441f164295e1b884e0bb1bb028c8c68",
            LongMd5    = "3678385ea919d1956cf701be1f6e69c9",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 19532,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 19533,
                    End     = 32859,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 32860,
                    End     = 45377,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 45378,
                    End     = 51167,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "test_multisession_dvd+r.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 230624,
            SectorSize = 2048,
            Md5        = "020993315e49ab0d36bc7248819162ea",
            LongMd5    = "c3e359363d07f4f536f245d162b5a2f2",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 24063,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 24064,
                    End     = 230623,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "test_multisession_dvd-r.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 257264,
            SectorSize = 2048,
            Md5        = "dff8f2107a4ea9633a88ce38ff609b8e",
            LongMd5    = "5405fe9a2fbf65a117511b32241ca420",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 235247,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 235248,
                    End     = 257263,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile      = "test_videocd.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 48794,
            SectorSize    = 2336,
            Md5           = "9500676e0400f6e734f50df65469b69c",
            LongMd5       = "23e2214500204f2c42ca2989f928fea2",
            SubchannelMd5 = "f738847620ace3f333dbc305321820a1",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1251,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1252,
                    End     = 48793,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        }
    ];
}