// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CloneCD.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CloneCD : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "CloneCD");
    public override IMediaImage Plugin     => new CloneCd();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "cdiready_theapprentice.ccd",
            MediaType     = MediaType.CDDA,
            Sectors       = 279300,
            Md5           = "a9412931e69111ba162d5d8b4822ac3f",
            LongMd5       = "a9412931e69111ba162d5d8b4822ac3f",
            SubchannelMd5 = "96f314754e66d95133308d5bb8573536",
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
            TestFile      = "jaguarcd.ccd",
            MediaType     = MediaType.CDDA,
            Sectors       = 243587,
            Md5           = "530a6d7a9ce9b60f8c727d2db0f6039e",
            LongMd5       = "530a6d7a9ce9b60f8c727d2db0f6039e",
            SubchannelMd5 = "47d4397c640734f5f85fe0c843e480f8",
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
            TestFile      = "pcengine.ccd",
            MediaType     = MediaType.CD,
            Sectors       = 160956,
            Md5           = "127b0a92b00ea9a67df1ed8c80daadc7",
            LongMd5       = "6ead3bdedb374f7b9bdf24773d30e491",
            SubchannelMd5 = "315ee5ebb36969b4ce0fb0162f7a9932",
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
                    End     = 38613,
                    Pregap  = 0,
                    Flags   = 4,
                    Number  = 2,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 28672,
                            ClusterSize = 2048,
                            Type        = "pcengine"
                        }
                    }
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
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcfx.ccd",
            MediaType     = MediaType.CD,
            Sectors       = 246680,
            Md5           = "9d538bd1ee1db068685ed59d29185941",
            LongMd5       = "76f4bd63c13db3e44fbf7acda20f49e2",
            SubchannelMd5 = "d9804e5f919ffb1531832049df8f0165",
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
                            Type        = "pcfx",
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
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_audiocd.ccd",
            MediaType     = MediaType.CDDA,
            Sectors       = 247073,
            Md5           = "c09f408a4416634d8ac1c1ffd0ed75a5",
            LongMd5       = "c09f408a4416634d8ac1c1ffd0ed75a5",
            SubchannelMd5 = "b744ddaf1d4ebd3bd0b96a160f55637d",
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
            TestFile      = "report_cdrom.ccd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "c5ae648d586e55afd1108294c9b86ca6",
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
                            Type        = "hfs",
                            VolumeName  = "Winpower"
                        },
                        new FileSystemTest
                        {
                            Clusters    = 254265,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Winpower"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrw_2x.ccd",
            MediaType     = MediaType.CDROM,
            Sectors       = 308224,
            Md5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMd5 = "c73559a91abd57f732c7ea609fef547a",
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
                            Type        = "iso9660",
                            VolumeName  = "ARCH_201901"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_enhancedcd.ccd",
            MediaType     = MediaType.CDPLUS,
            Sectors       = 303316,
            Md5           = "97e5bf1caf3998e818d40cd845c6ecc9",
            LongMd5       = "07b4d88c8f38cc0168a2f5725b31c52e",
            SubchannelMd5 = "a71264ddd9d364a4b1cd0ee4d4a7e1ad",
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
                            Type        = "iso9660",
                            VolumeName  = "Melanie C"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_audiocd_cdtext.ccd",
            MediaType     = MediaType.CDDA,
            Sectors       = 277696,
            Md5           = "7c8fc7bb768cff15d702ac8cd10108d7",
            LongMd5       = "7c8fc7bb768cff15d702ac8cd10108d7",
            SubchannelMd5 = "2a2918ad19f5bf1b6e52b57e40fe47eb",
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
            TestFile      = "test_enhancedcd.ccd",
            MediaType     = MediaType.CDPLUS,
            Sectors       = 59206,
            Md5           = "0ddda63b1cb61f8f961eabfa90737171",
            LongMd5       = "666ec8a1213cf4f6adc4675d9dd5955a",
            SubchannelMd5 = "d6184c3ac1966e61c528ae875627e65c",
            Tracks = new[]
            {
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
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 40203,
                    End     = 59205,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 3,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 18853,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "New"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_incd_udf200_finalized.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 350134,
            Md5           = "f95d6f978ddb4f98bbffda403f627fe1",
            LongMd5       = "6751e0ae7821f92221672b1cd5a1ff36",
            SubchannelMd5 = "569c87cdc115f2d02b2268fc2b4d8b11",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 350133,
                    Pregap  = 150,
                    Flags   = 7,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 600,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "INCD"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 402107,
                            ClusterSize  = 2048,
                            Type         = "udf",
                            VolumeName   = "InCD",
                            VolumeSerial = "40888C15CA13D401InCD"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_karaoke_multi_sampler.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 329158,
            Md5           = "9a19aa0df066732a8ec34025e8160248",
            LongMd5       = "e981f7dfdb522ba937fe75474e23a446",
            SubchannelMd5 = "c48c09b8c7c4af99de1cf97faaef32fc",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1886,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 1587,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = ""
                        }
                    }
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
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multiple_indexes.ccd",
            MediaType     = MediaType.CDDA,
            Sectors       = 65536,
            Md5           = "1b13a8f8aeb23f0b8bbc68518217e771",
            LongMd5       = "1b13a8f8aeb23f0b8bbc68518217e771",
            SubchannelMd5 = "d374e82dfcbc4515c09a9a6e5955bf1d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4803,
                    Pregap  = 150,
                    Flags   = 2
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
                    Flags   = 8
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
            TestFile      = "test_multisession.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 51168,
            Md5           = "236f95016ad395ba691517d35a05b767",
            LongMd5       = "8c48c8951229fd083c1aafcb3e062f2b",
            SubchannelMd5 = "a49b0b2dcebcc4a106524cb7f0f3c331",
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
                            Type        = "iso9660",
                            VolumeName  = "Session 1"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 9340,
                            ClusterSize  = 2048,
                            Type         = "udf",
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
                            Type        = "iso9660",
                            VolumeName  = "Session 2"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 7381,
                            ClusterSize  = 2048,
                            Type         = "udf",
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
                            Type        = "iso9660",
                            VolumeName  = "Session 3"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 6451,
                            ClusterSize  = 2048,
                            Type         = "udf",
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
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_videocd.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 48794,
            Md5           = "b640eed2eba209ebba4e6cd3171883a4",
            LongMd5       = "a1194d29dfb4e207eabf6208f908a213",
            SubchannelMd5 = "712725733e44be46e55f16569659fd07",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1251,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 1102,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "VIDEOCD"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1252,
                    End     = 48793,
                    Pregap  = 0,
                    Flags   = 4
                }
            }
        },

    #region These test images violate the specifications and are not expected to work yet

        /*
        new OpticalImageTestExpected
        {
            TestFile      = "test_castrated_leadout.ccd",
            MediaType     = MediaType.CDDA,
            Sectors       = 1050,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
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
                    End     = 1049,
                    Pregap  = 0,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_disc_starts_at_track2.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "6fa06c10561343438736a8d3d9a965ea",
            LongMD5       = "c82d20702d31bc15bdc91f7e107862ae",
            SubchannelMD5 = "976f4684da623c64acee464e9dca046e",
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
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_data_track_as_audio_fixed_sub.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "d9d46cae2a3a46316c8e1411e84d40ef",
            LongMD5       = "b3550e61649ba5276fed8d74f8e512ee",
            SubchannelMD5 = "a53aba8a0fdb038ef67e68ba009aa5b1",
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
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_data_track_as_audio.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "d9d46cae2a3a46316c8e1411e84d40ef",
            LongMD5       = "b3550e61649ba5276fed8d74f8e512ee",
            SubchannelMD5 = "a53aba8a0fdb038ef67e68ba009aa5b1",
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
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track0_in_session2.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 36939,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "3b3172070738044417ae5284195acbfd",
            SubchannelMD5 = "7eedb60edb3dc77eac41fd8f2214dfb8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 36938,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 36939,
                    End     = 36938,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track111_in_session2_fixed_sub.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 36939,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "396f86cdd8bfb012b68eabd5a94f604b",
            SubchannelMD5 = "c81a161af0fcd01dfd340290178a32fd",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 36938,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 36939,
                    End     = 36938,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track1-2-9_fixed_sub.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "649047a018bc6c1ba667397049eae888",
            SubchannelMD5 = "5fcfa6ec0511a0b5c73a817a456d5412",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 13349,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13350,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track1-2-9.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "3b3172070738044417ae5284195acbfd",
            SubchannelMD5 = "5fcfa6ec0511a0b5c73a817a456d5412",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 13349,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13350,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track1_overlaps_session2.ccd",
            MediaType     = MediaType.CDROM,
            Sectors       = 25539,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 113870,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_leadout.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 25539,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 62385,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_session2_leadin.ccd",
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
                    End     = 25499,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 25500,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_track1.ccd",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "6fa06c10561343438736a8d3d9a965ea",
            LongMD5       = "450fe640a58c0bc1fc9cd6e779884d2c",
            SubchannelMD5 = "1bf8af738f8dddb7142b308c245d05f5",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 13349,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13350,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        */

    #endregion
    };
}