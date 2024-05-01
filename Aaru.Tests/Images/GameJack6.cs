// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GameJack6.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
public class GameJack6 : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "GameJack 6");
    public override IMediaImage Plugin     => new Aaru.Images.Alcohol120();

    public override OpticalImageTestExpected[] Tests =>
    [
        new OpticalImageTestExpected
        {
            TestFile      = "report_cdrom_cooked_nodpm.xmd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "1994c303674718c74b35f9a4ea1d3515",
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
            TestFile      = "report_cdrom_cooked.xmd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "1994c303674718c74b35f9a4ea1d3515",
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
            TestFile      = "report_cdrom_nodpm.xmd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "66518892168f9bd5003e14979573861c",
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
            TestFile      = "report_cdrom.xmd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "8f0313d7a5f85e23be0d254f3c091004",
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
            TestFile      = "report_cdrw.xmd",
            MediaType     = MediaType.CDRW,
            Sectors       = 308224,
            Md5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMd5 = "124df553ac9337d1b36c611aa1a3e16f",
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
            TestFile      = "report_cdr.xmd",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "248dd7375479f40267b6d4f9fd889d5b",
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
            TestFile  = "report_dvdram_v1.xmd",
            MediaType = MediaType.DVDRAM,
            Sectors   = 471091,
            Md5       = "b6fe37716c05c1d52ef19c28946f3b76",
            LongMd5   = "b6fe37716c05c1d52ef19c28946f3b76",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 471090,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 471090,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "12_2_RELEASE_AMD64_CD"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvdram_v2.xmd",
            MediaType = MediaType.DVDRAM,
            Sectors   = 471091,
            Md5       = "b6fe37716c05c1d52ef19c28946f3b76",
            LongMd5   = "b6fe37716c05c1d52ef19c28946f3b76",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 471090,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 471090,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "12_2_RELEASE_AMD64_CD"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvdrom.xmd",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146358,
            Md5       = "ae08c024d6942e62884abe137f66a80f",
            LongMd5   = "ae08c024d6942e62884abe137f66a80f",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146357,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd+rw.xmd",
            MediaType = MediaType.DVDPRW,
            Sectors   = 2146358,
            Md5       = "cea717c199230bef889ce268a473d2e6",
            LongMd5   = "cea717c199230bef889ce268a473d2e6",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146357,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd-rw.xmd",
            MediaType = MediaType.DVDRW,
            Sectors   = 2146369,
            Md5       = "a60ea0383c5b39e14e09f47e749a3f46",
            LongMd5   = "a60ea0383c5b39e14e09f47e749a3f46",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146368,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd+r.xmd",
            MediaType = MediaType.DVDPR,
            Sectors   = 2146358,
            Md5       = "cea717c199230bef889ce268a473d2e6",
            LongMd5   = "cea717c199230bef889ce268a473d2e6",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146357,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
                        }
                    ]
                }
            ]
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd-r.xmd",
            MediaType = MediaType.DVDR,
            Sectors   = 2146358,
            Md5       = "6ba700d9b40b7ef1a9e4f78e317f124d",
            LongMd5   = "6ba700d9b40b7ef1a9e4f78e317f124d",
            Tracks =
            [
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146357,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems =
                    [
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 2146357,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "SU1100.001"
                        }
                    ]
                }
            ]
        }
    ];
}