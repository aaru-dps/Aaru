// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite6.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class BlindWrite6 : OpticalMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Media image formats", "BlindWrite 6");
        public override IMediaImage _plugin => new DiscImages.BlindWrite5();

        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile  = "dvdrom.B6T",
                MediaType = MediaType.DVDROM,
                Sectors   = 2287072,
                MD5       = "7272cae103a922910a09fdb6a6841dff",
                LongMD5   = "7272cae103a922910a09fdb6a6841dff",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 2287071,
                        Pregap  = 0,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Clusters    = 2287072,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "GuiaRaw"
                            }
                        }
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "jaguarcd.B6T",
                MediaType = MediaType.CDDA,
                Sectors   = 243587,
                MD5       = "3dd5bd0f7d95a40d411761d69255567a",
                LongMD5   = "3dd5bd0f7d95a40d411761d69255567a",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 16239,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 27490,
                        End     = 28236,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 28237,
                        End     = 78891,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 78892,
                        End     = 100053,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 100054,
                        End     = 133202,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 133203,
                        End     = 160907,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 160908,
                        End     = 181465,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 181466,
                        End     = 202023,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 202024,
                        End     = 222581,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 222582,
                        End     = 243139,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 243140,
                        End     = 243586,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "pcengine.B6T",
                MediaType = MediaType.CD,
                Sectors   = 160956,
                MD5       = "4f5165069b3c5f11afe5f59711bd945d",
                LongMD5   = "fd30db9486f67654179c90c8a5052edb",
                Tracks = new[]
                {
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
                        End     = 38463,
                        Pregap  = 0,
                        Flags   = 4,
                        Number  = 2,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Clusters    = 28672,
                                ClusterSize = 2048,
                                Type        = "PC Engine filesystem"
                            }
                        }
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 38464,
                        End     = 47216,
                        Pregap  = 150,
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
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "pcfx.B6T",
                MediaType = MediaType.CD,
                Sectors   = 246680,
                MD5       = "c1bc8de499756453d1387542bb32bb4d",
                LongMD5   = "455ec326506d2c5b974c4617c1010796",
                Tracks = new[]
                {
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
                        Flags   = 4,
                        Number  = 2,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 514,
                                ClusterSize = 2048,
                                Type        = "PC-FX",
                                VolumeName  = "同級生２"
                            }
                        }
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
                        End     = 220644,
                        Pregap  = 0,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 220645,
                        End     = 225645,
                        Pregap  = 150,
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
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdr.B6T",
                MediaType     = MediaType.CDR,
                Sectors       = 254265,
                MD5           = "63c99a087570b8936bb55156f5502f38",
                LongMD5       = "368c06d4b42ed581f3ad7f6ad57f70f6",
                SubchannelMD5 = "9c231e680e601cd10bb61fb519f00c84",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 254264,
                        Pregap  = 150,
                        Flags   = 4,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Clusters    = 63562,
                                ClusterSize = 8192,
                                Type        = "HFS",
                                VolumeName  = "Winpower"
                            },
                            new FileSystemTest
                            {
                                Clusters    = 254265,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "Winpower"
                            }
                        }
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrom.B6T",
                MediaType     = MediaType.CDROM,
                Sectors       = 254265,
                MD5           = "bf4bbec517101d0d6f45d2e4d50cb875",
                LongMD5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
                SubchannelMD5 = "46b6244ed63434cb0f91e0610c63fec8",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 254264,
                        Pregap  = 150,
                        Flags   = 4,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Clusters    = 63562,
                                ClusterSize = 8192,
                                Type        = "HFS",
                                VolumeName  = "Winpower"
                            },
                            new FileSystemTest
                            {
                                Clusters    = 254265,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "Winpower"
                            }
                        }
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrw_2x.B6T",
                MediaType     = MediaType.CDRW,
                Sectors       = 308224,
                MD5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
                LongMD5       = "3af5f943ddb9427d9c63a4ce3b704db9",
                SubchannelMD5 = "19f74cd5f05894203465374111be2aa7",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 308223,
                        Pregap  = 150,
                        Flags   = 4,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 308224,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "ARCH_201901"
                            }
                        }
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_karaoke_multi_sampler.B6T",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 329158,
                MD5           = "a34e29e42b60023a6ae59f37d2bd4bea",
                LongMD5       = "e981f7dfdb522ba937fe75474e23a446",
                SubchannelMD5 = "485a233924c003a1ab2ea9228f582344",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 1736,
                        Pregap  = 150,
                        Flags   = 4,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Clusters    = 1587,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = ""
                            }
                        }
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 1737,
                        End     = 32748,
                        Pregap  = 150,
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
                }
            }
        };
    }
}