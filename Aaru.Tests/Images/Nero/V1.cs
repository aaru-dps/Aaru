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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images.Nero;

[TestFixture]
public class V1 : OpticalMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "Nero Burning ROM", "V1");
    public override IMediaImage Plugin => new DiscImages.Nero();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile  = "cdiready_the_apprentice.nrg",
            MediaType = MediaType.CDDA,
            Sectors   = 279300,
            Md5       = "7557c72d4cf6df8bc1896388b863727a",
            LongMd5   = "7557c72d4cf6df8bc1896388b863727a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 88799,
                    Pregap  = 69300,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 88800,
                    End     = 107624,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 107625,
                    End     = 112199,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 112200,
                    End     = 133649,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 133650,
                    End     = 138224,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 138225,
                    End     = 159824,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 159825,
                    End     = 164774,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 164775,
                    End     = 185399,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 185400,
                    End     = 190124,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 190125,
                    End     = 208874,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 208875,
                    End     = 212999,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 213000,
                    End     = 232199,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 232200,
                    End     = 236699,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 236700,
                    End     = 241874,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 241875,
                    End     = 256124,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 256125,
                    End     = 256874,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 256875,
                    End     = 265649,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 265650,
                    End     = 267374,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 267375,
                    End     = 269999,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 270000,
                    End     = 271649,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 271650,
                    End     = 274274,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 274275,
                    End     = 279299,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "jaguarcd.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 243587,
            Md5           = "79ade978aad90667f272a693012c11ca",
            LongMd5       = "79ade978aad90667f272a693012c11ca",
            SubchannelMd5 = "83ec1010fc44694d69dc48bacec5481a",
            Tracks = new[]
            {
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
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcengine.nrg",
            MediaType     = MediaType.CD,
            Sectors       = 160956,
            Md5           = "8218b4aeea658111957fa3815a139e74",
            LongMd5       = "58b875ac8cb3b6b1f426bc734c3400e4",
            SubchannelMd5 = "9e9a6b51bc2e5ec67400cb33ad0ca33f",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 3364,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 3365,
                    End     = 38463,
                    Pregap  = 225,
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
                    End     = 126003,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126004,
                    End     = 160955,
                    Pregap  = 225,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcfx.nrg",
            MediaType     = MediaType.CD,
            Sectors       = 246680,
            Md5           = "24ff2f3451489a71ee502475137cccc3",
            LongMd5       = "891ebf5e6bd2eda7445f02958cc4fbd5",
            SubchannelMd5 = "e3a0d78b6c32f5795b1b513bd13a6bda",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4169,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4170,
                    End     = 4758,
                    Pregap  = 225,
                    Flags   = 4,
                    Number  = 2,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 364,
                            ClusterSize = 2048,
                            Type        = "PC-FX",
                            VolumeName  = "同級生２"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4759,
                    End     = 5790,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 5791,
                    End     = 41908,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 41909,
                    End     = 220644,
                    Pregap  = 150,
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
            TestFile      = "report_audiocd.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 247073,
            Md5           = "c09f408a4416634d8ac1c1ffd0ed75a5",
            LongMd5       = "c09f408a4416634d8ac1c1ffd0ed75a5",
            SubchannelMd5 = "9da6ad8f6f0cadd92509c10809da7296",
            Tracks = new[]
            {
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
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrom.nrg",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "1994c303674718c74b35f9a4ea1d3515",
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
            TestFile      = "report_cdrw.nrg",
            MediaType     = MediaType.CDROM,
            Sectors       = 308224,
            Md5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMd5 = "6fe81a972e750c68e08f6935e4d91e34",
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
            TestFile      = "report_enhancedcd.nrg",
            MediaType     = MediaType.CDPLUS,
            Sectors       = 303316,
            Md5           = "97e5bf1caf3998e818d40cd845c6ecc9",
            LongMd5       = "07b4d88c8f38cc0168a2f5725b31c52e",
            SubchannelMd5 = "e6f7319532f46c3fa4fd3569c65546e1",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 15660,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 15661,
                    End     = 33958,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 33959,
                    End     = 51329,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 51330,
                    End     = 71972,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 71973,
                    End     = 87581,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 87582,
                    End     = 103304,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 103305,
                    End     = 117690,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 117691,
                    End     = 136166,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136167,
                    End     = 153417,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 153418,
                    End     = 166931,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 166932,
                    End     = 187112,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 187113,
                    End     = 201440,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 201441,
                    End     = 222779,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 234030,
                    End     = 303315,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 14,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 69136,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Melanie C"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_audiocd_cdtext.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 277696,
            Md5           = "7c8fc7bb768cff15d702ac8cd10108d7",
            LongMd5       = "7c8fc7bb768cff15d702ac8cd10108d7",
            SubchannelMd5 = "ca781a7afc4eb77c51f7c551ed45c03c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 29901,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 29902,
                    End     = 65183,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65184,
                    End     = 78575,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78576,
                    End     = 95229,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95230,
                    End     = 126296,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126297,
                    End     = 155108,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155109,
                    End     = 191834,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 191835,
                    End     = 222925,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222926,
                    End     = 243587,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243588,
                    End     = 269749,
                    Pregap  = 0,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269750,
                    End     = 277695,
                    Pregap  = 0,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_incd_udf200_finalized.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 350134,
            Md5           = "684122981d4d762daf7b9e559584bccf",
            LongMd5       = "f3c26446201534c3635f4d2633310e45",
            SubchannelMd5 = "65f938f7f9ac34fabd3ab94c14eb76b5",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 350133,
                    Pregap  = 150,
                    Flags   = 6,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 600,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "INCD"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 402107,
                            ClusterSize  = 2048,
                            Type         = "UDF v2.00",
                            VolumeName   = "InCD",
                            VolumeSerial = "40888C15CA13D401InCD"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multi_karaoke_sampler.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 329158,
            Md5           = "a34e29e42b60023a6ae59f37d2bd4bea",
            LongMd5       = "e981f7dfdb522ba937fe75474e23a446",
            SubchannelMd5 = "f8c96f120cac18c52178b99ef4c4e2a9",
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
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multiple_indexes.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 65536,
            Md5           = "1b13a8f8aeb23f0b8bbc68518217e771",
            LongMd5       = "1b13a8f8aeb23f0b8bbc68518217e771",
            SubchannelMd5 = "25bae9e30657e2f64a45e5f690e3ae9e",
            Tracks = new[]
            {
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
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 54989,
                    End     = 65535,
                    Pregap  = 0,
                    Flags   = 1
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multisession.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 51168,
            Md5           = "5c35db53f7d4d9acce660de76eb81654",
            LongMd5       = "e8737ac5b670175abfa6dc927098abab",
            SubchannelMd5 = "48656afdbc40b6df06486a04a4d62401",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 8132,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 7876,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Session 1"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 9340,
                            ClusterSize  = 2048,
                            Type         = "UDF v1.02",
                            VolumeName   = "Session 1",
                            VolumeSerial = "50958B61AF6A749E"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 19383,
                    End     = 25959,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 2,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 6170,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Session 2"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 7381,
                            ClusterSize  = 2048,
                            Type         = "UDF v1.02",
                            VolumeName   = "Session 2",
                            VolumeSerial = "50958BBBAF6A7444"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 3,
                    Start   = 32710,
                    End     = 38477,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 3,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 5360,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Session 3"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 6451,
                            ClusterSize  = 2048,
                            Type         = "UDF v2.00",
                            VolumeName   = "Session 3",
                            VolumeSerial = "50958C19AF6A73E6"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 4,
                    Start   = 45228,
                    End     = 51167,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 4,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 45796,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Session 4"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 6649,
                            ClusterSize  = 2048,
                            Type         = "UDF v2.60",
                            VolumeName   = "Session 4",
                            VolumeSerial = "50958C82AF6A737D"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_videocd.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 48794,
            Md5           = "5412af85d30455e1466644ea97d1adae",
            LongMd5       = "610f972fa2e1c5988e4bd0f912b0f12f",
            SubchannelMd5 = "935a91f5850352818d92b71f1c87c393",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1101,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 1102,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "VIDEOCD"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1102,
                    End     = 48793,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_audiocd_dao.nrg",
            MediaType = MediaType.CDDA,
            Sectors   = 279196,
            Md5       = "cce718c0d4d60eb9a0571cd0ae7e2ff2",
            LongMd5   = "cce718c0d4d60eb9a0571cd0ae7e2ff2",
            Tracks = new[]
            {
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
                    End     = 65333,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65334,
                    End     = 78875,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78876,
                    End     = 95679,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95680,
                    End     = 126896,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126897,
                    End     = 155858,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155859,
                    End     = 192734,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 192735,
                    End     = 223975,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 223976,
                    End     = 244787,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 244788,
                    End     = 271099,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 271100,
                    End     = 279195,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_audiocd_tao.nrg",
            MediaType = MediaType.CDDA,
            Sectors   = 277696,
            Md5       = "0c355a31a7a488ec387c4508c498d6c0",
            LongMd5   = "0c355a31a7a488ec387c4508c498d6c0",
            Tracks = new[]
            {
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
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65184,
                    End     = 78575,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78576,
                    End     = 95229,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95230,
                    End     = 126296,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126297,
                    End     = 155108,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155109,
                    End     = 191834,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 191835,
                    End     = 222925,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222926,
                    End     = 243587,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243588,
                    End     = 269749,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269750,
                    End     = 277695,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 83078,
            Md5       = "6cdbcf18acc4c5edd7cc8d6e744dfda7",
            LongMd5   = "25fee97101ad661bb719ee008a1404c0",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83077,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_level2_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 83084,
            Md5       = "25f3dca4291f9c79bfa5592a3e050e8f",
            LongMd5   = "3acc918a3633f16c4242a39b76af3b35",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83083,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_level2_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 83084,
            Md5       = "25f3dca4291f9c79bfa5592a3e050e8f",
            LongMd5   = "3acc918a3633f16c4242a39b76af3b35",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83083,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 83078,
            Md5       = "6cdbcf18acc4c5edd7cc8d6e744dfda7",
            LongMd5   = "25fee97101ad661bb719ee008a1404c0",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83077,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_udf_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85733,
            Md5       = "d4088d90592000fbe3f8da5d6822aab1",
            LongMd5   = "761122bad9da6699773436a9f6ce753b",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85732,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_udf_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85733,
            Md5       = "1ec9d3cb33dd32b82d338ebf5c4da09c",
            LongMd5   = "f85fd68d3d159dbe417ccd39b221827a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85732,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83092,
            Md5       = "d9627277c18e16ab83da11e0c86afb8f",
            LongMd5   = "1ccd0e946b422fea751bddfde2ef245c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83091,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_level2_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83092,
            Md5       = "50e24226e31ad48de312135a5d3410bb",
            LongMd5   = "99f99437dadf65e1acac27fa68495525",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83091,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_level2_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83092,
            Md5       = "50e24226e31ad48de312135a5d3410bb",
            LongMd5   = "99f99437dadf65e1acac27fa68495525",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83091,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83092,
            Md5       = "d9627277c18e16ab83da11e0c86afb8f",
            LongMd5   = "1ccd0e946b422fea751bddfde2ef245c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83091,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_udf_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85747,
            Md5       = "9eef8934d6354be6fe6d03630d19de9e",
            LongMd5   = "c7ac66550c45dae54bc456070f408ff7",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85746,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_udf_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85747,
            Md5       = "046d55938e6d075f40e738d0f3f1161a",
            LongMd5   = "86c65c7e9fcf61eb6fe28d3bfc749da8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85746,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_mixed_mode_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 325928,
            Md5       = "666cf279a98e99a28af0347cac190118",
            LongMd5   = "d7dc3fe279da7643882818751b9e0ac0",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 46581,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 46582,
                    End     = 76633,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 76634,
                    End     = 112065,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 112066,
                    End     = 125607,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 125608,
                    End     = 142411,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 142412,
                    End     = 173628,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 173629,
                    End     = 202590,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 202591,
                    End     = 239466,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 239467,
                    End     = 270707,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 270708,
                    End     = 291519,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 291520,
                    End     = 317831,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 317832,
                    End     = 325927,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_mixed_mode_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 324278,
            Md5       = "7a82a04d2e6b283337e42b93a52f5083",
            LongMd5   = "c17ccaaf93dc07444cd9f03dc27a3b9f",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 46581,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 46582,
                    End     = 76483,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 76484,
                    End     = 111765,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 111766,
                    End     = 125157,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 125158,
                    End     = 141811,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 141812,
                    End     = 172878,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 172879,
                    End     = 201690,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 201691,
                    End     = 238416,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 238417,
                    End     = 269507,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269508,
                    End     = 290169,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 290170,
                    End     = 316331,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 316332,
                    End     = 324277,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_udf_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84985,
            Md5       = "34ef81d7871dcb2911cd4c682c8413fe",
            LongMd5   = "22f62fe5f6b6fe696e582ab879d44508",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84984,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_udf_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84985,
            Md5       = "549c2a6729fbecf222b85a0fc71a8ce5",
            LongMd5   = "a1236fa2aa68dc2fea157930ec0d0b62",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84984,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        }
        #region These test images violate the specifications and are not expected to work yet
        /*
        new OpticalImageTestExpected
        {
            TestFile      = "test_data_track_as_audio.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "d9d46cae2a3a46316c8e1411e84d40ef",
            LongMD5       = "b3550e61649ba5276fed8d74f8e512ee",
            SubchannelMD5 = "5479a1115bb6481db69fd6262e8c6076",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 36789,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_session2_leadin.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "6fa06c10561343438736a8d3d9a965ea",
            LongMD5       = "608a73cd10bccdadde68523aead1ee72",
            SubchannelMD5 = "933f1699ba88a70aff5062f9626ef529",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 25349,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 25350,
                    End     = 36788,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 36789,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_track1.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "6fa06c10561343438736a8d3d9a965ea",
            LongMD5       = "c82d20702d31bc15bdc91f7e107862ae",
            SubchannelMD5 = "d8eed571f137c92f22bb858d78fc1e41",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 13199,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13200,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 36789,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        */
        #endregion
    };
}