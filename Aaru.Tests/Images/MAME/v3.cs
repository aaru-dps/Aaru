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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images.MAME;

[TestFixture]
public class V3 : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "MAME", "v3");
    public override IMediaImage Plugin     => new Chd();

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
            TestFile   = "pcengine.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 160506,
            SectorSize = 2352,
            Md5        = "0dac1b20a9dc65c4ed1b11f6160ed983",
            LongMd5    = "f1c1dbe1cd9df11fe2c1f0a97130c25f",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 3439,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 3440,
                    End     = 38313,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 38314,
                    End     = 46916,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 46917,
                    End     = 53200,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 53201,
                    End     = 61518,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 61519,
                    End     = 68262,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 68263,
                    End     = 75096,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 75097,
                    End     = 82829,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 82830,
                    End     = 86180,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 86181,
                    End     = 90966,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 90967,
                    End     = 98973,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 98974,
                    End     = 106392,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 106393,
                    End     = 111937,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 111938,
                    End     = 119969,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 119970,
                    End     = 125778,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 125779,
                    End     = 160505,
                    Pregap  = 0,
                    Flags   = 4
                }
            ]
        },
        new()
        {
            TestFile   = "pcfx.chd",
            MediaType  = MediaType.CDROM,
            Sectors    = 246380,
            SectorSize = 2352,
            Md5        = "bc514cb4f3c7e2ee6857b2a3d470278b",
            LongMd5    = "dac5dc0961fa435da3c7d433477cda1a",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4244,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4245,
                    End     = 4758,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4759,
                    End     = 5790,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 5791,
                    End     = 41908,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 41909,
                    End     = 220494,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 220495,
                    End     = 225345,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 225346,
                    End     = 235197,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 235198,
                    End     = 246379,
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
            TestFile      = "test_enhancedcd.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 28953,
            SectorSize    = 2352,
            Md5           = "08629b564dc094927b657b8b9d2106d6",
            LongMd5       = "08629b564dc094927b657b8b9d2106d6",
            SubchannelMd5 = "4747b7451c06564582940008522c40a4",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 14404,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 14405,
                    End     = 28952,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile      = "test_multi_karaoke_sampler.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 329008,
            SectorSize    = 2352,
            Md5           = "f09312ba25a479fb81912a2965babd22",
            LongMd5       = "f48603d11883593f45ec4a3824681e4e",
            SubchannelMd5 = "ed0ca0f9bcdd0ebae2671549bf13311e",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1736,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1737,
                    End     = 32598,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 32599,
                    End     = 52521,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 52522,
                    End     = 70153,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 70154,
                    End     = 99947,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 99948,
                    End     = 119610,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 119611,
                    End     = 136848,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136849,
                    End     = 155639,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155640,
                    End     = 175675,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 175676,
                    End     = 206310,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 206311,
                    End     = 226299,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 226300,
                    End     = 244204,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 244205,
                    End     = 273814,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 273815,
                    End     = 293601,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 293602,
                    End     = 310560,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 310561,
                    End     = 329007,
                    Pregap  = 0,
                    Flags   = 0
                }
            ]
        },
        new()
        {
            TestFile      = "test_multisession.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 8133,
            SectorSize    = 2048,
            Md5           = "5a62e796e3d3224d4d7c4a94fa067947",
            LongMd5       = "84ce0389b12772b371bf93b9ef9cd0d6",
            SubchannelMd5 = "90f2548c86ea8fc9cde74ac89c01fad7",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 8132,
                    Pregap  = 150,
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