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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images.Nero;

[TestFixture]
public class V2 : OpticalMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "Nero Burning ROM", "V2");

    public override IMediaImage Plugin => new Aaru.Images.Nero();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "cdiready_the_apprentice.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 279300,
            Md5           = "7557c72d4cf6df8bc1896388b863727a",
            LongMd5       = "7557c72d4cf6df8bc1896388b863727a",
            SubchannelMd5 = "08cda0c6092a6d831712f56e676c021a",
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
            Md5           = "49dbfa68a7b3873d376fabec174be493",
            LongMd5       = "49dbfa68a7b3873d376fabec174be493",
            SubchannelMd5 = "aaa144eb86936ebd352193c836e62d48",
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
                    Pregap  = 0,
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
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "securdisc.nrg",
            MediaType     = MediaType.CDROM,
            Sectors       = 169536,
            Md5           = "95fa1df73ec2dbe008cb691495af6344",
            LongMd5       = "6649f47b6829715c1d1ca74e17ac7c0b",
            SubchannelMd5 = "8527822753d8123e9a01507a9acc8956",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 169535,
                    Pregap  = 150,
                    Flags   = 4
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
                    End     = 16398,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 16399,
                    End     = 29900,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 29901,
                    End     = 47799,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 47800,
                    End     = 63163,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 63164,
                    End     = 78774,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78775,
                    End     = 94581,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 94582,
                    End     = 116974,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 116975,
                    End     = 136015,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 136016,
                    End     = 153921,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 153922,
                    End     = 170600,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 170601,
                    End     = 186388,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 186389,
                    End     = 201648,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 201649,
                    End     = 224298,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 224299,
                    End     = 247072,
                    Pregap  = 150,
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
                            Type        = "iso9660",
                            VolumeName  = "ARCH_201901"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd+r-dl.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 3455936,
            Md5       = "692148a01b4204160b088141fb52bd70",
            LongMd5   = "692148a01b4204160b088141fb52bd70",
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
                            Type         = "udf",
                            VolumeName   = "Test DVD",
                            VolumeSerial = "483E25D50034BBB0"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd+rw.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 2295104,
            Md5       = "759e9c19389aee07f88a994132b6f8d9",
            LongMd5   = "759e9c19389aee07f88a994132b6f8d9",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2295103,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvdram_v1.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 1218960,
            Md5       = "c22b7796791cd4299d74863ed04496c6",
            LongMd5   = "c22b7796791cd4299d74863ed04496c6",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1218959,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 471090,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "12_2_RELEASE_AMD64_CD"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvdram_v2.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 2236704,
            Md5       = "00b1d7c5e9855959a4d2f6b796aeaf4c",
            LongMd5   = "00b1d7c5e9855959a4d2f6b796aeaf4c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2236703,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 471090,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "12_2_RELEASE_AMD64_CD"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvdrom.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146368,
            Md5       = "106f141400355476b499213f36a363f9",
            LongMd5   = "106f141400355476b499213f36a363f9",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146367,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
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
            Md5           = "7174351b366e423082846c7e396905ff",
            LongMd5       = "0988146c02c49fe563894d0e24435bbc",
            SubchannelMd5 = "758e4933c5703b9d90db0766dcb47b79",
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
            TestFile      = "test_audiocd_cdtext.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 277696,
            Md5           = "7c8fc7bb768cff15d702ac8cd10108d7",
            LongMd5       = "7c8fc7bb768cff15d702ac8cd10108d7",
            SubchannelMd5 = "c0bc1ac22c7e0e53407836c8f2331a94",
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
            Md5           = "4a76893cf5e5bee7016692b8f26504e3",
            LongMd5       = "f633fb0d3e63ded81118df8d955517a3",
            SubchannelMd5 = "d6257be337751e6f10effacaa82d8350",
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
                            Type        = "iso9660",
                            VolumeName  = "INCD"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 418519,
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
            TestFile      = "test_multi_karaoke_sampler.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 329158,
            Md5           = "a34e29e42b60023a6ae59f37d2bd4bea",
            LongMd5       = "e981f7dfdb522ba937fe75474e23a446",
            SubchannelMd5 = "d6ba23bc1118deb2db4a609e72437385",
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
                            Type        = "iso9660",
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
            SubchannelMd5 = "1a2583cb21730c2ed4f1c53fadffa60a",
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
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 54989,
                    End     = 65535,
                    Pregap  = 0,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multisession.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 51168,
            Md5           = "d5b4f6cd608800aa02a79eb4ddc714dc",
            LongMd5       = "5cd43bed94fc3e98f5ad805841c3d0a3",
            SubchannelMd5 = "67db42c525f3c850481e94465acd2423",
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
                            Clusters     = 9721,
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
                            Clusters     = 7682,
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
                            Clusters     = 6715,
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
                    Flags   = 4,
                    Number  = 4,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 45796,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Session 4"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 6920,
                            ClusterSize  = 2048,
                            Type         = "udf",
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
            Md5           = "5b5e93e5477cd7e8e444d25e8ff42a2a",
            LongMd5       = "806eee4238d63e8330710bc141e85bc8",
            SubchannelMd5 = "2f111b57f8932c43a6cf4ad2fd5eb5e2",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 949,
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
                    Start   = 950,
                    End     = 48793,
                    Pregap  = 302,
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
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 29902,
                    End     = 65333,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65334,
                    End     = 78875,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78876,
                    End     = 95679,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95680,
                    End     = 126896,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126897,
                    End     = 155858,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155859,
                    End     = 192734,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 192735,
                    End     = 223975,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 223976,
                    End     = 244787,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 244788,
                    End     = 271099,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 271100,
                    End     = 279195,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_audiocd_tao.nrg",
            MediaType = MediaType.CDDA,
            Sectors   = 279196,
            Md5       = "5c30e6a6fa2e85751a2e1592fbf3245d",
            LongMd5   = "5c30e6a6fa2e85751a2e1592fbf3245d",
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
            TestFile  = "make_data_dvd_iso9660-1999.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 82704,
            Md5       = "dac40e24aeccfe416a044bf9502d2b7e",
            LongMd5   = "dac40e24aeccfe416a044bf9502d2b7e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82703,
                    Pregap  = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_dvd_joliet.nrg",
            MediaType = MediaType.DVDROM,
            Sectors   = 83072,
            Md5       = "a412c13e81a4044407a81ad794095306",
            LongMd5   = "a412c13e81a4044407a81ad794095306",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83071,
                    Pregap  = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_iso9660-1999_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 82695,
            Md5       = "b14ace0656db97360e21bc9d7d3d5109",
            LongMd5   = "5793b471f2ef0087af63facba9485bee",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82694,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_iso9660-1999_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 82695,
            Md5       = "b14ace0656db97360e21bc9d7d3d5109",
            LongMd5   = "5793b471f2ef0087af63facba9485bee",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82694,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 83068,
            Md5       = "05dcbde7856dae96bb1fcff7d02fdb96",
            LongMd5   = "bf5a216352b7a025fb98d76b38afbe3d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83067,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 83068,
            Md5       = "05dcbde7856dae96bb1fcff7d02fdb96",
            LongMd5   = "bf5a216352b7a025fb98d76b38afbe3d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83067,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_102_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85364,
            Md5       = "331c02751e4c2fd505fffa163b1bc361",
            LongMd5   = "12f448affe38c96311c9de4633f787e1",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85363,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_102_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85364,
            Md5       = "52df6748a9436452e6a024d6d43cc5fb",
            LongMd5   = "ed6f139b0e763690d84b1f4aba2a6b78",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85363,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_150_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85364,
            Md5       = "45703870e27a99cdc5ee486f9b919209",
            LongMd5   = "d9ed9c8bafd5f218d3b7f6aee6be2d44",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85363,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_150_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85364,
            Md5       = "56392a983981f9e222ea18807934a3d4",
            LongMd5   = "9cbd519fe328fcb206c1786fbbfeeb87",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85363,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_150_sparing_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 86529,
            Md5       = "ee86f608a9276e4bc267b8c66907ada4",
            LongMd5   = "f92ebc7cd69e2e060db761eba6582d67",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_150_sparing_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 86529,
            Md5       = "6fbd06e26bbeb49b19434b8630b4711d",
            LongMd5   = "68692050b99c94dc61c38c2b315ac8f5",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_150_virtual_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85368,
            Md5       = "408ff544e060baa6b67cce490aba1f77",
            LongMd5   = "c48e0ec4b399399507e1da52e569db33",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85367,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_150_virtual_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85368,
            Md5       = "79fc10eb1b87a95cc46581a6680fed02",
            LongMd5   = "1ba7ebd3904756d07f28c8cca5df3176",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85367,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_200_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85366,
            Md5       = "cfd56ea81d9927f3bd84303e2e46f3d0",
            LongMd5   = "5906fae5ed3eae8b34d7b1c3768254f3",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85365,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_200_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85366,
            Md5       = "45a74e0240eafd4a25f3719fcb63c423",
            LongMd5   = "5d2ebb99b0aaaacd08294f910b95ea25",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85365,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_200_sparing_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 86529,
            Md5       = "a36f10b5881798f73a60dcabfdbda2e5",
            LongMd5   = "cbef2b2fa29347456f2d2c6aadf0a65a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_200_sparing_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 86529,
            Md5       = "20484d666fdb7be5cb4783853f1d5e11",
            LongMd5   = "3fd901634fca7ec9979b69ae8b242d5c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_200_virtual_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85370,
            Md5       = "477788d0e383b9dbd9bf179c6eae1950",
            LongMd5   = "b688819e815a7dea55ab894666d98a36",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85369,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_200_virtual_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85370,
            Md5       = "884633f3720b20c36c2f56032456ff42",
            LongMd5   = "520c0da95e556285f264055309d0643d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85369,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_201_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85366,
            Md5       = "8703cbb59eb1dfb6f7b3748f9e410698",
            LongMd5   = "750009655962df0f01d3261ed71e2b06",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85365,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_201_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85366,
            Md5       = "5a8ee84276bdf1b1d0b07b64639892a4",
            LongMd5   = "2b89eb05149632731bc07aef7bd85518",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85365,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_201_sparing_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 86529,
            Md5       = "b88ca672a6f86f84072a626b62ba9f14",
            LongMd5   = "131c8da49d40e850f4e956cc8770445d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_201_sparing_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 86529,
            Md5       = "7b7558a98a0ade5e3d5046eb24983c2b",
            LongMd5   = "2d625d5192a506cf2baf91f8b04ff722",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_201_virtual_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85370,
            Md5       = "2b96f44ee072f04cb7477abc23c0ac78",
            LongMd5   = "bf7ad9935df332783848f815458e1687",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85369,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet_udf_201_virtual_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85370,
            Md5       = "70ea1fca4e5929514d15be2b16156961",
            LongMd5   = "929cff7615d84c900c637067e6bf44e6",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85369,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_iso9660-1999_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 82697,
            Md5       = "9712faa85483cf520e0efae0bbd53164",
            LongMd5   = "c7a5031c12fcac644f20384c8cafe3a8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82696,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_iso9660-1999_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 82697,
            Md5       = "9712faa85483cf520e0efae0bbd53164",
            LongMd5   = "c7a5031c12fcac644f20384c8cafe3a8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82696,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83082,
            Md5       = "2a391c84479c34267439103ca6abf7bf",
            LongMd5   = "dd5fd9f1e45acff0c2c9b85f6abb3ab8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83081,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83082,
            Md5       = "2a391c84479c34267439103ca6abf7bf",
            LongMd5   = "dd5fd9f1e45acff0c2c9b85f6abb3ab8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83081,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_102_physical_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85378,
            Md5       = "0861eb66287123d470b7945debe8fb12",
            LongMd5   = "139d794e62632e4845ab24daff3685f0",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85377,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_102_physical_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85378,
            Md5       = "b2b0772bbc6b950bbdbaaac90831c9f2",
            LongMd5   = "9cd6841be3c35cd3ea012fbb4f5b313d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85377,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_150_physical_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85378,
            Md5       = "8329b3663619eca9d424eb1fbc7036b7",
            LongMd5   = "451bc347c53260b50c48b101e64e9023",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85377,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_150_physical_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85378,
            Md5       = "055310ad3609d44011081863d617d33f",
            LongMd5   = "a9d37132e6b519bb774f603129c1b7dd",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85377,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_150_sparing_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 86529,
            Md5       = "cc9ece6ddcbb456ff5cf197d0f21b785",
            LongMd5   = "d0042821851343b7149231e13f60677d",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_150_sparing_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 86529,
            Md5       = "ae08aedc15e623ed2a035b9813e5360d",
            LongMd5   = "260416feefd735fbfa9d428197fc31ce",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_150_virtual_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85382,
            Md5       = "23ba6f2deb635e408e7938345aaecd5e",
            LongMd5   = "3c3cc0829c0f5b3dc90552cd06f6caca",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85381,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_150_virtual_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85382,
            Md5       = "1b7b0d16a910b65173fd777c974e94b6",
            LongMd5   = "cb9495de8c27af64b84925b32351ceb2",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85381,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_200_physical_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85380,
            Md5       = "7f550c2b0b587275a63ccf13a732bb55",
            LongMd5   = "629a18aa832bc0336d7f5eeb69d179c8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85379,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_200_physical_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85380,
            Md5       = "119fe576b2a81e0070b48748545ed691",
            LongMd5   = "aae875760f7a1e6e41b1b93b71de3b20",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85379,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_200_sparing_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 86529,
            Md5       = "2b4b5af6ac9d988a2dc38dc5b873d574",
            LongMd5   = "5c18f49d07d53fd799e74f24d4d53484",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_200_sparing_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 86529,
            Md5       = "0613ae6e1c5b87ae563b3f7f572a8b18",
            LongMd5   = "a6169364cb292c245e435eb63cb47057",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_200_virtual_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85384,
            Md5       = "d45ffe1153db6f5c71596cbd905fc488",
            LongMd5   = "a902f86d13d56f2e6f227a52634089d9",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85383,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_200_virtual_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85384,
            Md5       = "779eecd0085b18b1d9918b41423dd339",
            LongMd5   = "d0513c7eee273befeff5ce89f9238560",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85383,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_201_physical_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85380,
            Md5       = "4c81ffc4fa384850de42910b35a5aca3",
            LongMd5   = "b3a211be5fdae118e301a64de7fcb179",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85379,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_201_physical_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85380,
            Md5       = "55785037c6349fa1f7fdd0b908181818",
            LongMd5   = "eab4b4988478a929abb00456c79bdd93",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85379,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_201_sparing_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 86529,
            Md5       = "92ce7748584f7e6ed60c4bd507b4dc0f",
            LongMd5   = "685810f81bfdf3726b29c6bfb3fe6240",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_201_sparing_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 86529,
            Md5       = "2a546c0c5eabb2bdcf23f30b463bb275",
            LongMd5   = "50de9b1be03914910e6c60f7bfa0e077",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86528,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_201_virtual_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85384,
            Md5       = "8122aa37899aea7ca6edd5da216ea172",
            LongMd5   = "20a452a3389e06af32a79137b005dc66",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85383,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet_udf_201_virtual_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 85384,
            Md5       = "92b7c6ba9530389f8189f5ca271dfb25",
            LongMd5   = "f7c3a968839604a0317132c76387617c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85383,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_102_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84616,
            Md5       = "d15dc18c94c1800c578dc50130395a3e",
            LongMd5   = "e133b2452e7f8a3fb0993e6626b81dbf",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84615,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_102_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84616,
            Md5       = "897d2c63f0e181854191c859d9aa8bd2",
            LongMd5   = "00b91fff0f255a541f9e6ce1484f7853",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84615,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_150_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84616,
            Md5       = "d3dd0903e74a4a714114751fe3071ceb",
            LongMd5   = "9764d6dcd88081043d12592ec529739e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84615,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_150_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84616,
            Md5       = "076f320539c8171246e4d24a5cf3d533",
            LongMd5   = "70a988b0c6c9ecd3ae3b12a18dd4dc0e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84615,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_150_sparing_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85793,
            Md5       = "fac65be4def378788d467966e2b795d8",
            LongMd5   = "0c221dec63b409a3785d7183b4149176",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85792,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_150_sparing_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85793,
            Md5       = "366195d1e5140c0a690d2b256891db60",
            LongMd5   = "934e4d7fde8b1ad40c8135e43159e74b",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85792,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_150_virtual_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84620,
            Md5       = "a40b83ad5ba0de4a1e19426c0bd05934",
            LongMd5   = "4b7698c901b19739c5911db97eb1ca55",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84619,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_150_virtual_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84620,
            Md5       = "cc0ebb19ee2fa513c7fcdc6c4916536d",
            LongMd5   = "7cee40146211ed2bbd50dbe5082bd290",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84619,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_200_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84618,
            Md5       = "86d67a9f66ff43f89f5a58b785598b08",
            LongMd5   = "1bb6cc5f1db38a4bb4598cd8776d5aaf",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84617,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_200_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84618,
            Md5       = "ca9cee749466053ab344a27a4b5c2e11",
            LongMd5   = "3880fae96c3fb170fb4f337985adc5c5",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84617,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_200_sparing_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85793,
            Md5       = "803526863c0ec3f64c2a89cdb7ebce77",
            LongMd5   = "5f16344d375502a1082c635f5e27ea4e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85792,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_200_sparing_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85793,
            Md5       = "00570af53cea4275c7191ba52e65f1bf",
            LongMd5   = "2efedcd2a6bfb1658015221ecfba0752",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85792,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_200_virtual_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84622,
            Md5       = "b0b56d1663c508e7d0ceb33f3bb5cc78",
            LongMd5   = "20a0714daaff2d41798d97f72cd95b3a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84621,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_200_virtual_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84622,
            Md5       = "de4c155829449b40632bb9165dba0839",
            LongMd5   = "f12ca7d65ddad197d3075926ad7c3aaf",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84621,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_201_physical_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84618,
            Md5       = "bd83c27796677aa75f067c474952226e",
            LongMd5   = "220904f85af94dff3b899e36c30c6511",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84617,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_201_physical_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84618,
            Md5       = "fa86ee2ecaf7a0b9e06b1e0ea3e66a16",
            LongMd5   = "e6e3a29fc6642716c1c302c63586f7cb",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84617,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_201_sparing_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85793,
            Md5       = "7f6b97cd47875e397de275b56a3cfd86",
            LongMd5   = "1b50de41d291308c1aa678da06a38978",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85792,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_201_sparing_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 85793,
            Md5       = "c3bed9a87c42cafe1f7a1239eaa0fac6",
            LongMd5   = "0c5c3ab226f73c3c36636abdb6ac289e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 85792,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_201_virtual_dao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84622,
            Md5       = "54ba4fe1ac87998dd0cb7dddfea97664",
            LongMd5   = "a946851e149714a084ea3d1794964fc5",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84621,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_udf_201_virtual_tao.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 84622,
            Md5       = "372d1a672f8ec1310c8665024b35b483",
            LongMd5   = "e4e2304b1cccffdac1e781bae228dee4",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84621,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_enhancedcd_dao.nrg",
            MediaType = MediaType.CDPLUS,
            Sectors   = 337261,
            Md5       = "9f1272614a307e3fac0b3e6ba90098e8",
            LongMd5   = "b5d6a75d73752f78a978e51e7f4c4adf",
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
                    End     = 65333,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65334,
                    End     = 78875,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78876,
                    End     = 95679,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95680,
                    End     = 126896,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126897,
                    End     = 155858,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155859,
                    End     = 192734,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 192735,
                    End     = 223975,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 223976,
                    End     = 244787,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 244788,
                    End     = 271099,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 271100,
                    End     = 279195,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 290446,
                    End     = 337260,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_enhancedcd_tao.nrg",
            MediaType = MediaType.CDPLUS,
            Sectors   = 337261,
            Md5       = "7bbe7fd534a37882924c718604c9a6e9",
            LongMd5   = "c3053f05e4f371fcceeaf001bd1b235c",
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
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 290296,
                    End     = 337260,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_hdburn_full.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 727605,
            Md5       = "f47418bf60ea47be64e97c17192e2d5f",
            LongMd5   = "e7daf8bc5100fd211028cf0f6491d343",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 727604,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_hdburn.nrg",
            MediaType = MediaType.CDROM,
            Sectors   = 31084,
            Md5       = "c76c3537f1b3f3c4feecca0e35b4b859",
            LongMd5   = "a58449cfb0de9708f2a19d515d9d37f8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 31083,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_mixed_mode_dao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 362041,
            Md5       = "e50fb58ee954ae5bcec18c09896095a5",
            LongMd5   = "1d3da4b1804a0e9aa07d8f7c2f51672b",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82694,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 82695,
                    End     = 112746,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 112747,
                    End     = 148178,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 148179,
                    End     = 161720,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 161721,
                    End     = 178524,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 178525,
                    End     = 209741,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 209742,
                    End     = 238703,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 238704,
                    End     = 275579,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 275580,
                    End     = 306820,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 306821,
                    End     = 327632,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 327633,
                    End     = 353944,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 353945,
                    End     = 362040,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_mixed_mode_tao.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 362041,
            Md5       = "c1b14eec8c9bc1177926c8ef5f382cc0",
            LongMd5   = "8f62724f4f6bfe8898daca1b39b25eb8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 82694,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 82695,
                    End     = 112746,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 112747,
                    End     = 148178,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 148179,
                    End     = 161720,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 161721,
                    End     = 178524,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 178525,
                    End     = 209741,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 209742,
                    End     = 238703,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 238704,
                    End     = 275579,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 275580,
                    End     = 306820,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 306821,
                    End     = 327632,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 327633,
                    End     = 353944,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 353945,
                    End     = 362040,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },

#region These test images violate the specifications and are not expected to work yet

        /*
        new OpticalImageTestExpected
        {
            TestFile  = "test_all_tracks_are_track1.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 36939,
            MD5       = "UNKNOWN",
            LongMD5   = "UNKNOWN",

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
                    End     = 36938,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_castrated_leadout.nrg",
            MediaType     = MediaType.CDDA,
            Sectors       = 270050,
            MD5           = "UNKNOWN",
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
                    End     = 270049,
                    Pregap  = 0,
                    Flags   = 2
                }
            }
        },
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
            TestFile      = "test_data_track_as_audio_fixed_sub.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 62385,
            MD5           = "UNKNOWN",
            LongMD5       = "6751e0ae7821f92221672b1cd5a1ff36",
            SubchannelMD5 = "65f938f7f9ac34fabd3ab94c14eb76b5",
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
            TestFile      = "test_track1_overlaps_session2.nrg",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 4294992835,
            MD5           = "UNKNOWN",
            LongMD5       = "608a73cd10bccdadde68523aead1ee72",
            SubchannelMD5 = "d8eed571f137c92f22bb858d78fc1e41",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 113870,
                    End     = 4294992834,
                    Pregap  = 114020,
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
            LongMD5       = "c82d20702d31bc15bdc91f7e107862ae",
            SubchannelMD5 = "935a91f5850352818d92b71f1c87c393",
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
        new OpticalImageTestExpected
        {
            TestFile  = "test_track2_inside_track1.nrg",
            MediaType = MediaType.CDROMXA,
            Sectors   = 62385,
            MD5       = "6fa06c10561343438736a8d3d9a965ea",
            LongMD5   = "4a045788e69965efe0c87950d013e720",

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