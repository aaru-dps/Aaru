// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : V1.cs
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

namespace Aaru.Tests.Images.AaruFormat;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

[TestFixture]
public class V1 : OpticalMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "AaruFormat", "V1");
    public override IMediaImage Plugin => new AaruFormat();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "cdiready_the_apprentice.aif",
            MediaType     = MediaType.CDIREADY,
            Sectors       = 279300,
            SectorSize    = 2352,
            Md5           = "ad6b898e5f93faf33967fe53fea7037e",
            LongMd5       = "8c897ff39ce1ae7b091bfd00fbc3c1bb",
            SubchannelMd5 = "579e2b502d86bc1eb7d6aded2b752c36",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 69149,
                    Pregap  = 0,
                    Flags   = 4,
                    Number  = 0,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 279300,
                            ClusterSize = 2048,
                            Type        = "CD-i",
                            VolumeName  = "The Apprentice"
                        }
                    }
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 69150,
                    End     = 88649,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 88650,
                    End     = 107474,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 107475,
                    End     = 112049,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 112050,
                    End     = 133499,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 133500,
                    End     = 138074,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 138075,
                    End     = 159674,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 159675,
                    End     = 164624,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 164625,
                    End     = 185249,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 185250,
                    End     = 189974,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 189975,
                    End     = 208724,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 208725,
                    End     = 212849,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 212850,
                    End     = 232049,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 232050,
                    End     = 236549,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 236550,
                    End     = 241724,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 241725,
                    End     = 255974,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 255975,
                    End     = 256724,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 256725,
                    End     = 265499,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 265500,
                    End     = 267224,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 267225,
                    End     = 269849,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269850,
                    End     = 271499,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 271500,
                    End     = 274124,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 274125,
                    End     = 279299,
                    Pregap  = 150,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_audiocd.aif",
            MediaType     = MediaType.CDDA,
            Sectors       = 247073,
            SectorSize    = 2352,
            Md5           = "c9036cb72bcb67d469ca82eb7a66cb2a",
            LongMd5       = "c9036cb72bcb67d469ca82eb7a66cb2a",
            SubchannelMd5 = "6d2ae02b362918f531ad414c736d349a",
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
            TestFile      = "report_cdr.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 254265,
            SectorSize    = 2048,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "34b8e75c3038deceaea7d382f22740cb",
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
            TestFile      = "report_cdrom.aif",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            SectorSize    = 2048,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "5d7f79a75e21f56e62d6fc894ee71ee6",
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
            TestFile      = "report_cdrw_2x.aif",
            MediaType     = MediaType.CDRW,
            Sectors       = 308224,
            SectorSize    = 2048,
            Md5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMd5 = "80a59aaf861f925a530e1b0d7857fe25",
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
            TestFile   = "report_dvd+r.aif",
            MediaType  = MediaType.DVDPR,
            Sectors    = 2146368,
            SectorSize = 2048,
            Md5        = "106f141400355476b499213f36a363f9",
            LongMd5    = "106f141400355476b499213f36a363f9",
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
                            Type        = "ISO9660",
                            VolumeName  = "SU1100.001"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd-r.aif",
            MediaType  = MediaType.DVDR,
            Sectors    = 2146368,
            SectorSize = 2048,
            Md5        = "106f141400355476b499213f36a363f9",
            LongMd5    = "106f141400355476b499213f36a363f9",
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
                            Type        = "ISO9660",
                            VolumeName  = "SU1100.001"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd-ram_v1.aif",
            MediaType  = MediaType.DVDRAM,
            Sectors    = 1218960,
            SectorSize = 2048,
            Md5        = "c22b7796791cd4299d74863ed04496c6",
            LongMd5    = "c22b7796791cd4299d74863ed04496c6",
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
                            Type        = "ISO9660",
                            VolumeName  = "12_2_RELEASE_AMD64_CD"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd-ram_v2.aif",
            MediaType  = MediaType.DVDRAM,
            Sectors    = 2236704,
            SectorSize = 2048,
            Md5        = "00b1d7c5e9855959a4d2f6b796aeaf4c",
            LongMd5    = "00b1d7c5e9855959a4d2f6b796aeaf4c",
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
                            Type        = "ISO9660",
                            VolumeName  = "12_2_RELEASE_AMD64_CD"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd+r_dl.aif",
            MediaType  = MediaType.DVDROM,
            Sectors    = 16384000,
            SectorSize = 2048,
            Md5        = "63d0fd3f25ab503a1818b15ca5eb86b5",
            LongMd5    = "63d0fd3f25ab503a1818b15ca5eb86b5",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 16383999,
                    Pregap  = 0,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters     = 16384000,
                            ClusterSize  = 2048,
                            Type         = "UDF v1.02",
                            VolumeName   = "Test DVD",
                            VolumeSerial = "483E25D50034BBB0"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd-rom.aif",
            MediaType  = MediaType.DVDROM,
            Sectors    = 2146368,
            SectorSize = 2048,
            Md5        = "106f141400355476b499213f36a363f9",
            LongMd5    = "106f141400355476b499213f36a363f9",
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
                            Type        = "ISO9660",
                            VolumeName  = "SU1100.001"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd+rw.aif",
            MediaType  = MediaType.DVDPRW,
            Sectors    = 2295104,
            SectorSize = 2048,
            Md5        = "3c03ab1def372553f1b04afa0fdbc527",
            LongMd5    = "3c03ab1def372553f1b04afa0fdbc527",
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
                            Type        = "ISO9660",
                            VolumeName  = "SU1100.001"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_dvd-rw.aif",
            MediaType  = MediaType.DVDRWDL,
            Sectors    = 2146368,
            SectorSize = 2048,
            Md5        = "106f141400355476b499213f36a363f9",
            LongMd5    = "106f141400355476b499213f36a363f9",
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
                            Type        = "ISO9660",
                            VolumeName  = "SU1100.001"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_enhancedcd.aif",
            MediaType     = MediaType.CD,
            Sectors       = 303316,
            SectorSize    = 2352,
            Md5           = "b6cb0d4b3a7763dc2ba6b5256a23bcbe",
            LongMd5       = "74cf2db1d7a0c1d790728e6250f866b7",
            SubchannelMd5 = "27d433659fd0142310b81175fbd610a7",
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
            TestFile      = "test_audiocd_cdtext.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 277696,
            SectorSize    = 2352,
            Md5           = "78466ec1a08d7804a6cb38f2ed89b10f",
            LongMd5       = "78466ec1a08d7804a6cb38f2ed89b10f",
            SubchannelMd5 = "ac39ed98b7033da6aa936b4314574a2a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 29751,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 29752,
                    End     = 65033,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65034,
                    End     = 78425,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78426,
                    End     = 95079,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95080,
                    End     = 126146,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126147,
                    End     = 154958,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 154959,
                    End     = 191684,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 191685,
                    End     = 222775,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222776,
                    End     = 243437,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243438,
                    End     = 269599,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269600,
                    End     = 277695,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_audiocd_multiple_indexes.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 65536,
            SectorSize    = 2352,
            Md5           = "d5d22e15dcf3f081d562b351611a8991",
            LongMd5       = "d5d22e15dcf3f081d562b351611a8991",
            SubchannelMd5 = "3546cc3e1b2b3898de5a03083af9d6ee",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 4652,
                    Pregap  = 150,
                    Flags   = 2
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 4653,
                    End     = 13804,
                    Pregap  = 151,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 13805,
                    End     = 36684,
                    Pregap  = 70,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 36685,
                    End     = 54988,
                    Pregap  = 4500,
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
            TestFile      = "test_cdr_incd_finalized.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 350134,
            SectorSize    = 2048,
            Md5           = "edc146b00d622f92c6a9bb4648cbea82",
            LongMd5       = "6b36340c27d5583e73539175eb87c683",
            SubchannelMd5 = "663da762a5bef780d09217fca9d23e08",
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
        new OpticalImageTestExpected
        {
            TestFile      = "test_enhancedcd.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 59206,
            SectorSize    = 2352,
            Md5           = "3e9862ad534415cb3b1ef216f9446d4e",
            LongMd5       = "502cdc0391687bc2e2a89ea7a5906ebb",
            SubchannelMd5 = "1aa52cf9468044489d791ab69a023a31",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 14255,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 14256,
                    End     = 28952,
                    Pregap  = 149,
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
            TestFile      = "test_multi_karaoke_sampler.aif",
            MediaType     = MediaType.CD,
            Sectors       = 329158,
            SectorSize    = 2352,
            Md5           = "fef9ff409aa2643ac0c0649e84346f5f",
            LongMd5       = "ef18dc4f63ad59c6294ab09da7704366",
            SubchannelMd5 = "aa71734f6385319656e2f1a64af5328b",
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
            TestFile      = "test_multisession.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 51168,
            SectorSize    = 2048,
            Md5           = "e2e19cf38891e67a0829d01842b4052e",
            LongMd5       = "b31f2d228dd564c88ad851b12b43c01d",
            SubchannelMd5 = "989c696ee5bb336b4ad30474da573925",
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
            TestFile      = "test_videocd.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 48794,
            SectorSize    = 2328,
            Md5           = "a5531d15eefe70ff21718b3b5da08255",
            LongMd5       = "11a0d9994ee761655ef4d61c6cda99e9",
            SubchannelMd5 = "f49e383ccee2f3cb97aeb82fcb4fdb18",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1107,
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
                    Start   = 1108,
                    End     = 48793,
                    Pregap  = 144,
                    Flags   = 4
                }
            }
        }
    };
}