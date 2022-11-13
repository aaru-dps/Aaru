// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cuesheet.cs
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

namespace Aaru.Tests.Images.MagicISO;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

[TestFixture]
public class Cuesheet : OpticalMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "MagicISO", "Cuesheet");
    public override IMediaImage Plugin => new CdrWin();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile  = "cdiready_the_apprentice.cue",
            MediaType = MediaType.CDDA,
            Sectors   = 279299,
            Md5       = "ab350df419f96d967f51d0161ebeba63",
            LongMd5   = "ab350df419f96d967f51d0161ebeba63",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 69000,
                    End     = 88799,
                    Pregap  = 150,
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
                    End     = 279298,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_audiocd.cue",
            MediaType = MediaType.CDDA,
            Sectors   = 247072,
            Md5       = "277e98295297f618cc63687e98288d7e",
            LongMd5   = "277e98295297f618cc63687e98288d7e",
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
                    Start   = 16399,
                    End     = 30050,
                    Pregap  = 150,
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
                    End     = 247071,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_cdrom.cue",
            MediaType = MediaType.CDROM,
            Sectors   = 254264,
            Md5       = "2de6dd5eaa71c1a97625bab68382da60",
            LongMd5   = "222edd2c920b63aefe2087ed6278abe6",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254263,
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
            TestFile  = "report_cdrw.cue",
            MediaType = MediaType.CDROM,
            Sectors   = 308223,
            Md5       = "f1510c82ea4ff535415833242adddac6",
            LongMd5   = "22bd168e59e075229821448b60d1820b",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 308222,
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
            TestFile  = "report_dvdram_v1.cue",
            MediaType = MediaType.DVDROM,
            Sectors   = 1218959,
            Md5       = "fa58d5645e091cc3f0c87998c819955e",
            LongMd5   = "fa58d5645e091cc3f0c87998c819955e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1218958,
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
            TestFile  = "report_dvdram_v2.cue",
            MediaType = MediaType.DVDROM,
            Sectors   = 2236703,
            Md5       = "c02a2cf88af17ed8333af2e7d4560e40",
            LongMd5   = "c02a2cf88af17ed8333af2e7d4560e40",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2236702,
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
            TestFile  = "report_dvd+r-dl.cue",
            MediaType = MediaType.DVDROM,
            Sectors   = 3455935,
            Md5       = "e71164a331f656e30025c39956e35372",
            LongMd5   = "e71164a331f656e30025c39956e35372",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 3455934
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "report_dvd-rom.cue",
            MediaType = MediaType.DVDROM,
            Sectors   = 2146367,
            Md5       = "fa1beeef4c4f1493f306a13f4fd113b2",
            LongMd5   = "fa1beeef4c4f1493f306a13f4fd113b2",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2146366,
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
            TestFile  = "report_dvd+rw.cue",
            MediaType = MediaType.DVDROM,
            Sectors   = 2295103,
            Md5       = "767692b526a7ea7020e7144d6d7cadab",
            LongMd5   = "767692b526a7ea7020e7144d6d7cadab",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2295102,
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
        /* This image is invalid an impossible to process properly due to a bug in MagicISO
        new OpticalImageTestExpected
        {
            TestFile  = "report_enhancedcd.cue",
            MediaType = MediaType.CDPLUS,
            Sectors   = 303315,
            MD5       = "0ac3eaefdd2c138e86229d195d63cba2",
            LongMD5   = "2524762a816af8e8c188b971dfd27374",
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
                    Start   = 15511,
                    End     = 33958,
                    Pregap  = 150,
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
                    End     = 234179,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 234030,
                    End     = 303314,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        */ new OpticalImageTestExpected
        {
            TestFile  = "test_multi_karaoke_sampler.cue",
            MediaType = MediaType.CDROMXA,
            Sectors   = 329157,
            Md5       = "cc6354d06b009b0446012842c7f94be7",
            LongMd5   = "bb3ebf139ebb76fff1b229a379d289e4",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1886,
                    Pregap  = 150,
                    Flags   = 4
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
                    End     = 329156,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        }
    };
}