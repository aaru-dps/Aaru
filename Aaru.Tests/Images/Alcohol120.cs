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

namespace Aaru.Tests.Images;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

[TestFixture]
public class Alcohol120 : OpticalMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Alcohol 120%");
    public override IMediaImage _plugin => new DiscImages.Alcohol120();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "cdiready_the_apprentice.mds",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 279300,
            MD5           = "556d7d32e3c01c2087cc56b25fe5f66d",
            LongMD5       = "556d7d32e3c01c2087cc56b25fe5f66d",
            SubchannelMD5 = "6ffdfdeacee7cd3caf6316f6b5f3a635",
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
            TestFile      = "gigarec.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 469652,
            MD5           = "dc8aaff9bd1a8a6f642e15bce29cd03e",
            LongMD5       = "1ba5f0fb9f3572197a8d039fd341c0aa",
            SubchannelMD5 = "95ef603d7dc9e285929cbf3c79ba9db2",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 469651,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 469652,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "New Volume"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "jaguarcd.mds",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 243587,
            MD5           = "1dee46e2fa0de388d1f225ab8fa6d0b4",
            LongMD5       = "1dee46e2fa0de388d1f225ab8fa6d0b4",
            SubchannelMD5 = "b765ee54404c081b6aa8e67181d04e17",
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
            TestFile      = "pcengine.mds",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 160956,
            MD5           = "248ff28ea147ecdf0724fdfb0e59174a",
            LongMD5       = "eb48e46f5bd085dd6f9936d89afe6e9b",
            SubchannelMD5 = "42eea856ab1bbb04d16b1efed7c54d3f",
            Tracks = new[]
            {
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
                    End     = 38463,
                    Pregap  = 150,
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
                    End     = 126078,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126079,
                    End     = 160955,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcfx.mds",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 246680,
            MD5           = "64d6baf711d2e0f24499d284ac2bc580",
            LongMD5       = "f0af56f9d093b214e1b7c9148a869eb3",
            SubchannelMD5 = "e596bcd432f69758678cda1e04207de5",
            Tracks = new[]
            {
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
                    End     = 4908,
                    Pregap  = 150,
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
            TestFile      = "report_audiocd.mds",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 247073,
            MD5           = "ff35cfa013871b322ef54612e719c185",
            LongMD5       = "ff35cfa013871b322ef54612e719c185",
            SubchannelMD5 = "9da6ad8f6f0cadd92509c10809da7296",
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

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdr.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 254265,
            MD5           = "016e9431ca3161d427b29dbc1312a232",
            LongMD5       = "6b4e35ec371770751f26163629253015",
            SubchannelMD5 = "6ea1db8638c111b7fd45b35a138d24fe",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 150,
                    Flags   = 4 /*
                        Number  =1,
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
                        }*/
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrom.mds",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            MD5           = "016e9431ca3161d427b29dbc1312a232",
            LongMD5       = "6b4e35ec371770751f26163629253015",
            SubchannelMD5 = "1994c303674718c74b35f9a4ea1d3515",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 150,
                    Flags   = 4 /*,
                        Number  =1,
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
                        }*/
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrw_12x.mds",
            MediaType     = MediaType.CDRW,
            Sectors       = 308224,
            MD5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMD5       = "a1890f71563eb9907e4a08fef6afd6bf",
            SubchannelMD5 = "337aefffca57a2d0222dabd8989f0b3f",
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
            TestFile      = "report_cdrw_2x.mds",
            MediaType     = MediaType.CDRW,
            Sectors       = 308224,
            MD5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMD5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMD5 = "6fe81a972e750c68e08f6935e4d91e34",
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
            TestFile      = "report_cdrw_4x.mds",
            MediaType     = MediaType.CDRW,
            Sectors       = 254265,
            MD5           = "fe67ffb95da123e060a1c4d278df3c5a",
            LongMD5       = "9c13c4f7dcb76feae684ba9a368094c5",
            SubchannelMD5 = "e4095cb91fa40382dcadc22433b281c3",
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
            TestFile  = "report_dvd+r-dl.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 3455936,
            MD5       = "692148a01b4204160b088141fb52bd70",
            LongMD5   = "692148a01b4204160b088141fb52bd70",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 3455935,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters     = 3455936,
                            ClusterSize  = 2048,
                            Type         = "UDF v1.02",
                            VolumeName   = "Test DVD",
                            VolumeSerial = "483E25D50034BBB0"
                        }
                    }
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd+r.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146368,
            MD5       = "32746029d25e430cd50c464232536d1a",
            LongMD5   = "32746029d25e430cd50c464232536d1a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146367,
                    Pregap  = 0 /*,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 2146357,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "SU1100.001"
                            }
                        }*/
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd-r.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146368,
            MD5       = "c20217c0356fcd074c33b5f4b1355914",
            LongMD5   = "c20217c0356fcd074c33b5f4b1355914",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146367,
                    Pregap  = 0 /*,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 2146357,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "SU1100.001"
                            }
                        }*/
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvdrom.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146368,
            MD5       = "0a49394278360f737a22e48ef125d7cd",
            LongMD5   = "0a49394278360f737a22e48ef125d7cd",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146367,
                    Pregap  = 0 /*,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 2146357,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "SU1100.001"
                            }
                        }*/
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd+rw.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 2295104,
            MD5       = "4d0cac3a6f56c581870de38682408f95",
            LongMD5   = "4d0cac3a6f56c581870de38682408f95",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2295103,
                    Pregap  = 0 /*,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 2146357,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "SU1100.001"
                            }
                        }*/
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd-rw.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146368,
            MD5       = "4844a94a97027b0fea664a1fba3ecbb2",
            LongMD5   = "4844a94a97027b0fea664a1fba3ecbb2",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146367,
                    Pregap  = 0 /*,
                        Number  = 1,
                        FileSystems = new[]
                        {
                            new FileSystemTest
                            {
                                Bootable    = true,
                                Clusters    = 2146357,
                                ClusterSize = 2048,
                                Type        = "ISO9660",
                                VolumeName  = "SU1100.001"
                            }
                        }*/
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_enhancedcd.mds",
            MediaType     = MediaType.CDPLUS,
            Sectors       = 303316,
            MD5           = "7246ab63afe862677302929fb3514676",
            LongMD5       = "797e7cb29028763ab827212d8630cb50",
            SubchannelMD5 = "ceee6cf49071da484dd995c50a0b09fb",
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
            TestFile      = "test_audiocd_cdtext.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 277696,
            MD5           = "7c8fc7bb768cff15d702ac8cd10108d7",
            LongMD5       = "7c8fc7bb768cff15d702ac8cd10108d7",
            SubchannelMD5 = "ca781a7afc4eb77c51f7c551ed45c03c",
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
            TestFile      = "test_enhancedcd.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 59206,
            MD5           = "947139fcc9924337f11040945ee8f1f7",
            LongMD5       = "5d755e3ea7c66f81a381b9c59168107a",
            SubchannelMD5 = "84cb28d835c25e51fdcb6c2291707786",
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
                            Type        = "ISO9660",
                            VolumeName  = "New"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_incd_udf200_finalized.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 350134,
            MD5           = "f95d6f978ddb4f98bbffda403f627fe1",
            LongMD5       = "6751e0ae7821f92221672b1cd5a1ff36",
            SubchannelMD5 = "65f938f7f9ac34fabd3ab94c14eb76b5",
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
                            Type        = "ISO9660",
                            VolumeName  = "INCD"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 350134,
                            ClusterSize  = 2048,
                            Type         = "UDF v2.00",
                            VolumeName   = "InCD",
                            VolumeSerial = "40888C15CA13D401InCD"
                        }
                    }
                }
            }
        },

        // TODO: Needs redump, corrupted image
        new OpticalImageTestExpected
        {
            TestFile      = "test_multi_karaoke_sampler.mds",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 329158,
            MD5           = "064afaa489a2f402f42aaf9b546a3fef",
            LongMD5       = "4d02563f72bdfbbf5a41bacf7a0fe916",
            SubchannelMD5 = "e5e51af5f0a689f956ffc52df2949e71",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1736,
                    Pregap  = 150,
                    Flags   = 4 /*,
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
                        }*/
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
            TestFile      = "test_multiple_indexes.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 65536,
            MD5           = "1b13a8f8aeb23f0b8bbc68518217e771",
            LongMD5       = "1b13a8f8aeb23f0b8bbc68518217e771",
            SubchannelMD5 = "25bae9e30657e2f64a45e5f690e3ae9e",
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
            TestFile  = "test_multisession_dvd+r.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 230624,
            MD5       = "020993315e49ab0d36bc7248819162ea",
            LongMD5   = "020993315e49ab0d36bc7248819162ea",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 230623,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 22016,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Session 1"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "test_multisession_dvd-r.mds",
            MediaType = MediaType.DVDROM,
            Sectors   = 257264,
            MD5       = "dff8f2107a4ea9633a88ce38ff609b8e",
            LongMD5   = "dff8f2107a4ea9633a88ce38ff609b8e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 257263,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 206560,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Session 1"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multisession.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 51168,
            MD5           = "236f95016ad395ba691517d35a05b767",
            LongMD5       = "8c48c8951229fd083c1aafcb3e062f2b",
            SubchannelMD5 = "5731d17924f9fa8934c1e1ac076c6259",
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
                            Clusters     = 8133,
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
                            Clusters     = 6427,
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
                            Clusters     = 5618,
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
                            Clusters     = 5790,
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
            TestFile      = "test_videocd.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 48794,
            MD5           = "ab3cf9dfcc3e79c57e11e4675655d5e2",
            LongMD5       = "cadb31c693c0996f50ba47e262d84518",
            SubchannelMD5 = "4da6d2891fc0f916c1d6cd6eebe4586a",
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
        #region These test images violate the specifications and are not expected to work yet
        /*
        new OpticalImageTestExpected
        {
            TestFile      = "test_all_tracks_are_track_1.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 51078,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
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
                    Start   = 25539,
                    End     = 51077,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_castrated_leadout.mds",
            MediaType     = MediaType.CDR,
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
            TestFile      = "test_data_track_as_audio_fixed_sub.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 62385,
            MD5           = "d9d46cae2a3a46316c8e1411e84d40ef",
            LongMD5       = "b3550e61649ba5276fed8d74f8e512ee",
            SubchannelMD5 = "77778d0e72a499b6c22f75df11a8d97f",
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
            TestFile      = "test_data_track_as_audio.mds",
            MediaType     = MediaType.CDR,
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
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track0_in_session2.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "3b3172070738044417ae5284195acbfd",
            SubchannelMD5 = "7eedb60edb3dc77eac41fd8f2214dfb8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track111_in_session2_fixed_sub.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "396f86cdd8bfb012b68eabd5a94f604b",
            SubchannelMD5 = "c81a161af0fcd01dfd340290178a32fd",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track111_in_session2.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "76175679c852073137299c5ca7b113e4",
            SubchannelMD5 = "8a3b37786d5276529c8cdbbf57e2d528",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track1_2_9_fixed_sub.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "6ff84bf8ecf2624fbaba37df08462294",
            SubchannelMD5 = "b6051aa115a91c08de0ffc47ca64275e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 13349,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13350,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track1_2_9.mds",
            MediaType     = MediaType.CDR,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "3b3172070738044417ae5284195acbfd",
            SubchannelMD5 = "ee1a81152b386347dc656697d8f50ab9",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 13349,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13350,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_leadout.mds",
            MediaType     = MediaType.CDR,
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
                    End     = 62234,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 62385,
                    End     = 25538,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_session2_leadin.mds",
            MediaType     = MediaType.CDR,
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
                    Session = 1,
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_track1.mds",
            MediaType     = MediaType.CDR,
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
                    End     = 13349,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13350,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 36939,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },*/
        #endregion
    };
}