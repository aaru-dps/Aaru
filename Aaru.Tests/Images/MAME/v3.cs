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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Images.MAME;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

[TestFixture]
public class V3 : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "MAME", "v3");
    public override IMediaImage Plugin    => new Chd();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "gigarec.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 469652,
            SectorSize    = 2048,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 469651,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "hdd.chd",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 251904,
            SectorSize = 512,
            Md5        = "43476343f53a177dd57b68dd769917aa"
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcengine.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 160506,
            SectorSize    = 2352,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 3439,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 3440,
                    End     = 38313,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 38316,
                    End     = 46918,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 46920,
                    End     = 53203,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 53204,
                    End     = 61521,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 61524,
                    End     = 68267,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 68268,
                    End     = 75101,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 75104,
                    End     = 82836,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 82840,
                    End     = 86190,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 86192,
                    End     = 90977,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 90980,
                    End     = 98986,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 98988,
                    End     = 106406,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 106408,
                    End     = 111952,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 111956,
                    End     = 119987,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 119988,
                    End     = 125796,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 125800,
                    End     = 160526,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcfx.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 246380,
            SectorSize    = 2352,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4244,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4248,
                    End     = 4761,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4764,
                    End     = 5795,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 5796,
                    End     = 41913,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 41916,
                    End     = 220501,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 220504,
                    End     = 225354,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 225356,
                    End     = 235207,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 235208,
                    End     = 246389,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_audiocd.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 247073,
            SectorSize    = 2352,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 16548,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 16552,
                    End     = 30053,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 30056,
                    End     = 47954,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 47956,
                    End     = 63319,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 63320,
                    End     = 78930,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78932,
                    End     = 94738,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 94740,
                    End     = 117132,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 117136,
                    End     = 136176,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136180,
                    End     = 154085,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 154088,
                    End     = 170766,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 170768,
                    End     = 186555,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 186556,
                    End     = 201815,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 201816,
                    End     = 224465,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 224468,
                    End     = 247091,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdr.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            SectorSize    = 2048,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrom.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            SectorSize    = 2048,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrw.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 308224,
            SectorSize    = 2048,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 308223,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_enhancedcd.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 28953,
            SectorSize    = 2352,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 14404,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 14408,
                    End     = 28955,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multi_karaoke_sample.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 329008,
            SectorSize    = 2352,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1736,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1740,
                    End     = 32601,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 32604,
                    End     = 52526,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 52528,
                    End     = 70159,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 70160,
                    End     = 99953,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 99956,
                    End     = 119618,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 119620,
                    End     = 136857,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136860,
                    End     = 155650,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155652,
                    End     = 175687,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 175688,
                    End     = 206322,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 206324,
                    End     = 226312,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 226316,
                    End     = 244220,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 244224,
                    End     = 273833,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 273836,
                    End     = 293622,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 293624,
                    End     = 310582,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 310584,
                    End     = 329030,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multisession.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 8133,
            SectorSize    = 2048,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 8132,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_videocd.chd",
            MediaType     = MediaType.CDROM,
            Sectors       = 48794,
            SectorSize    = 2336,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1251,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1252,
                    End     = 48793,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        }
    };
}