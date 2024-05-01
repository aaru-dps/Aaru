// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : nosub.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images.cdrdao.raw;

[TestFixture]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class rw_raw : OpticalMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "cdrdao", "raw", "rw_raw");

    public override IMediaImage Plugin => new Cdrdao();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "gigarec.toc",
            MediaType     = MediaType.CDROM,
            Sectors       = 469652,
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Clusters    = 469652,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "New Volume"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_audiocd.toc",
            MediaType     = MediaType.CDDA,
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
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdr.toc",
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
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
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrom.toc",
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
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
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrw.toc",
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 308224,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "ARCH_201901"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_enhancedcd.toc",
            MediaType     = MediaType.CDDA,
            Sectors       = 28953,
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
        new OpticalImageTestExpected
        {
            TestFile      = "test_multi_karaoke_sample.toc",
            MediaType     = MediaType.CDROM,
            Sectors       = 329008,
            SectorSize    = 2048,
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Clusters    = 1587,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = ""
                        }
                    ]
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
                    Pregap  = 150,
                    Flags   = 0
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_multisession.toc",
            MediaType     = MediaType.CDROM,
            Sectors       = 8133,
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Clusters    = 7876,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Session 1"
                        },
                        new FileSystemTest
                        {
                            Clusters     = 8133,
                            ClusterSize  = 2048,
                            Type         = "udf",
                            VolumeName   = "Session 1",
                            VolumeSerial = "50958B61AF6A749E"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_videocd.toc",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 48794,
            Md5           = "e30dbde1dbb5a384e9005f148541abbe",
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
                    Flags   = 4,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Clusters    = 1102,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "VIDEOCD"
                        }
                    ]
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
        },

#region These test images violate the specifications and are not expected to work yet

        /*
        new OpticalImageTestExpected
        {
            TestFile      = "test_castrated_leadout.toc",
            MediaType     = MediaType.CDDA,
            Sectors       = 7843003432689721,
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
                    Pregap  = 0,
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
                    End     = 7843003432689720,
                    Pregap  = 0,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_data_track_as_audio_fixed_sub.toc",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 25539,
            MD5           = "f9efc75192a7c0f3252e696c617f8ddd",
            LongMD5       = "3b3172070738044417ae5284195acbfd",
            SubchannelMD5 = "caf393bb809eda1a6d3a1727fbac7cb7",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 25538,
                    Pregap  = 0,
                    Flags   = 4
                }
            }
        },
        */

#endregion
    };
}