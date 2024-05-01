// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Toast.cs
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
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
public class Toast : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "Roxio Toast");
    public override IMediaImage Plugin     => new ZZZRawImage();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 826,
            Md5       = "d6bbb43811561cb9fa415b1fa05dcfdb",
            LongMd5   = "577e9f2b27a8181669b0e96eb55eb688",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 825,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 946,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_dos_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 826,
            Md5       = "6f79117d7fc8649635896f2d0e2e9546",
            LongMd5   = "6dfb9b49b268392c0127664ca1e5a915",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 825,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 946,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_dos.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "e7ce7b651b029ca8a4249ca1d8fad69c",
            LongMd5   = "7fa666dd62d11a537da5c48d0caf780b",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 244,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ebook_eng.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "fc097ef49cc9b578cf31d6430dc13df2",
            LongMd5   = "8f4a95e750ed53e5a345e89d5cd839a6",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 244,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ebook_fra.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "a8867952b5e0e5293871959b4b91ef48",
            LongMd5   = "1eb4cf2ed8f438646766b8a8a36954f7",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 244,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_joliet_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 831,
            Md5       = "e62b21444f0c895a396a63d136a35558",
            LongMd5   = "264213ffb43772397c562158d10b404a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 830,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 951,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Disk utils"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_joliet.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "ac825459dbbbf08c77c8b0fa7efde389",
            LongMd5   = "04262d65e3f09e417550d06dfc0bf9f3",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 249,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Disk utils"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_mac_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 826,
            Md5       = "5ab7aab16d297a8cf5014e4af95acfcc",
            LongMd5   = "1fb4ac43c918f5a33df5920e35e05d4a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 825,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 946,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Disk utils"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_mac.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "3060b6c4a6096dd80c47c1d3b850370e",
            LongMd5   = "83d8b04d7bc6c77862aa9c48c75cdb4c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 244,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Disk utils"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ver_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 826,
            Md5       = "fd8a0d1d4faa28928afc3fc0bd5ae206",
            LongMd5   = "86d168eb16f47329f3e13f69d4a9300f",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 825,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 946,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 826,
            Md5       = "6284b4c7800a20b98618e29566e6e59f",
            LongMd5   = "103fde59a213126f6fe7319c15fea293",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 825,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 946,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ver_dos.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "5c604e9b6c2caa9d23748a1b188f9614",
            LongMd5   = "24d5ffc906f4dd603818829f2a28c00c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 244,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 831,
            Md5       = "61c3ba8655a9076ac72c999ea5663787",
            LongMd5   = "964b12844c4b0159fc88631093496b91",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 830,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 951,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Disk utils"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ver_joliet.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "02d9e7c51156243c514ffe5140be4c6b",
            LongMd5   = "19693d6f99c6a1d59090bc47d0ca4466",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 249,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "Disk utils"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "toast_3.5.7_iso9660_xa_ver.toast.lz",
            MediaType = MediaType.CD,
            Sectors   = 262,
            Md5       = "51d7be9014160e4052ce833b9511e901",
            LongMd5   = "34e2b00526e812272c0f980a7663284c",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 261,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 244,
                            ClusterSize = 2048,
                            Type        = "iso9660",
                            VolumeName  = "DISK_UTILS"
                        }
                    }
                }
            }
        }
    };
}