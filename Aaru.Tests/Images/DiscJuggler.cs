// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiscJuggler.cs
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

namespace Aaru.Tests.Images;

[TestFixture]
public class DiscJuggler : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "DiscJuggler");
    public override IMediaImage Plugin     => new Aaru.Images.DiscJuggler();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "jaguarcd.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 243587,
            Md5           = "01de32dcb21ac62be269ddd3bc99ab3e",
            LongMd5       = "01de32dcb21ac62be269ddd3bc99ab3e",
            SubchannelMd5 = "214794f37c41d1e2727eba68efdd369d",
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
                    End     = 28087,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 28088,
                    End     = 78741,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 78742,
                    End     = 99904,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 99905,
                    End     = 133053,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 133054,
                    End     = 160758,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 160759,
                    End     = 181316,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 181317,
                    End     = 201874,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 201875,
                    End     = 222432,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 222433,
                    End     = 242990,
                    Pregap  = 149,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 242991,
                    End     = 243586,
                    Pregap  = 149,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "make_audiocd.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 277696,
            Md5           = "0c355a31a7a488ec387c4508c498d6c0",
            LongMd5       = "0c355a31a7a488ec387c4508c498d6c0",
            SubchannelMd5 = "ac79fa4b07a042bcdd033a3e0319a0ae",
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
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65184,
                    End     = 78575,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78576,
                    End     = 95229,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95230,
                    End     = 126296,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126297,
                    End     = 155108,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155109,
                    End     = 191834,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 191835,
                    End     = 222925,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222926,
                    End     = 243587,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243588,
                    End     = 269749,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269750,
                    End     = 277695,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode1_joliet.cdi",
            MediaType = MediaType.CDROM,
            Sectors   = 83063,
            Md5       = "9f1251feaed14a62326ab399b73342e3",
            LongMd5   = "8ae1725d36537af9395ece058992e2b3",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83062,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_data_mode2_joliet.cdi",
            MediaType = MediaType.CDROMXA,
            Sectors   = 83077,
            Md5       = "68d39977149d3062b41dba6c1ff475cf",
            LongMd5   = "382b5d7957ee7e19b0e9dd7db866e4c4",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83076,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_dvd.cdi",
            MediaType = MediaType.DVDROM,
            Sectors   = 84896,
            Md5       = "5240b794f12174da73915e8c1f38b6a4",
            LongMd5   = "5240b794f12174da73915e8c1f38b6a4",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 84895,
                    Pregap  = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_enhancedcd.cdi",
            MediaType = MediaType.CD,
            Sectors   = 335666,
            Md5       = "ce5de948ef5d1fccd1c1664451b1ba10",
            LongMd5   = "fe4165656cc6023b999d9fbf05501b25",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 29901,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 29902,
                    End     = 65183,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 65184,
                    End     = 78575,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 78576,
                    End     = 95229,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 95230,
                    End     = 126296,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 126297,
                    End     = 155108,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 155109,
                    End     = 191834,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 191835,
                    End     = 222925,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 222926,
                    End     = 243587,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 243588,
                    End     = 269749,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 269750,
                    End     = 277695,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 288946,
                    End     = 335665,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_mixed_mode.cdi",
            MediaType = MediaType.CDROMXA,
            Sectors   = 360909,
            Md5       = "a5eba1d1bfeae8d6eea6c8abfdf79be4",
            LongMd5   = "b981912374fe50f91c91085e31028886",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 83062,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 83063,
                    End     = 113114,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 113115,
                    End     = 148396,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 148397,
                    End     = 161788,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 161789,
                    End     = 178442,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 178443,
                    End     = 209509,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 209510,
                    End     = 238321,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 238322,
                    End     = 275047,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 275048,
                    End     = 306138,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 306139,
                    End     = 326800,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 326801,
                    End     = 352962,
                    Pregap  = 0,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 352963,
                    End     = 360908,
                    Pregap  = 0,
                    Flags   = 0
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_multisession_dvd.cdi",
            MediaType = MediaType.DVDROM,
            Sectors   = 86560,
            Md5       = "95388d443073217e7cc4cf6b0391ec7f",
            LongMd5   = "95388d443073217e7cc4cf6b0391ec7f",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 86559
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_pangram_mode1_joliet.cdi",
            MediaType = MediaType.CDROM,
            Sectors   = 642,
            Md5       = "36477c851cd6184034c86cc61cdd0e60",
            LongMd5   = "1b11183918ed5a2295f89272e2fa5810",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 641,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "make_pangram_mode2_joliet.cdi",
            MediaType = MediaType.CDROMXA,
            Sectors   = 656,
            Md5       = "ea2e0354dccd3dfdca6242154f024b59",
            LongMd5   = "18c580f0621cabdd9fc81be6275c41f0",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 655,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "pcengine.cdi",
            MediaType     = MediaType.CD,
            Sectors       = 160956,
            Md5           = "4fc2665cec77b289a68d35edbdd50730",
            LongMd5       = "afabe77571db99e29cf4e6854b6f523e",
            SubchannelMd5 = "c2026374c196855ee95f19de967f3ba1",
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
                    Flags   = 0,
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
                    Flags   = 4
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
            TestFile  = "pcfx.cdi",
            MediaType = MediaType.CD,
            Sectors   = 246680,
            Md5       = "2e872a5cfa43959183677398ede15c08",
            LongMd5   = "a8939e0fd28ee0bd876101b218af3572",
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
                    End     = 4758,
                    Pregap  = 150,
                    Flags   = 0,
                    Number  = 2,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 364,
                            ClusterSize = 2048,
                            Type        = "pcfx",
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
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 5791,
                    End     = 41908,
                    Pregap  = 150,
                    Flags   = 0
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
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 225646,
                    End     = 235497,
                    Pregap  = 0,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 235498,
                    End     = 246679,
                    Pregap  = 0,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "report_audiocd.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 247073,
            Md5           = "9f8abbb08746f86d7c3d0b1b22f1fcc2",
            LongMd5       = "9f8abbb08746f86d7c3d0b1b22f1fcc2",
            SubchannelMd5 = "1045e621183bf4c8a9d641f8a5767fec",
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
                    End     = 30050,
                    Pregap  = 150,
                    Flags   = 0
                },
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 30051,
                    End     = 47799,
                    Pregap  = 0,
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
            TestFile      = "report_cdr.cdi",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "dc02efe9c59c0d0e72bb7d26d7f01bce",
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
            TestFile      = "report_cdrom.cdi",
            MediaType     = MediaType.CDROM,
            Sectors       = 254265,
            Md5           = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5       = "3d3f9cf7d1ba2249b1e7960071e5af46",
            SubchannelMd5 = "e0d121ac5c0e4e30a823ac445942feae",
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
            TestFile      = "report_cdrw_2x.cdi",
            MediaType     = MediaType.CDROM,
            Sectors       = 308224,
            Md5           = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5       = "3af5f943ddb9427d9c63a4ce3b704db9",
            SubchannelMd5 = "82e9a16a532717d7b733258d674073c3",
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
            TestFile  = "report_dvdram_v1.cdi",
            MediaType = MediaType.DVDROM,
            Sectors   = 1218961,
            Md5       = "b04c88635c5d493c250c289964018a7a",
            LongMd5   = "b04c88635c5d493c250c289964018a7a",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 1218960,
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
            TestFile  = "report_dvdram_v2.cdi",
            MediaType = MediaType.DVDROM,
            Sectors   = 2236705,
            Md5       = "c0823b070513d02c9f272986f23e74e8",
            LongMd5   = "c0823b070513d02c9f272986f23e74e8",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 2236704,
                    Pregap  = 0,
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
            TestFile  = "report_dvd-r.cdi",
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
            TestFile  = "report_dvd+r-dl.cdi",
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
            TestFile  = "report_dvdrom.cdi",
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
            TestFile  = "report_dvd+rw.cdi",
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
            TestFile  = "report_dvd-rw.cdi",
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
            TestFile      = "report_enhancedcd.cdi",
            MediaType     = MediaType.CDPLUS,
            Sectors       = 303316,
            Md5           = "7a76c22590401d57b84396c9fc2925e4",
            LongMd5       = "63a318522c436ccd6b3ac3959bdb417f",
            SubchannelMd5 = "2581844f8192c719da44ba57e8e5512b",
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
            TestFile  = "test_audiocd_cdtext.cdi",
            MediaType = MediaType.CDDA,
            Sectors   = 277696,
            Md5       = "52d7a2793b7600dc94d007f5e7dfd942",
            LongMd5   = "52d7a2793b7600dc94d007f5e7dfd942",
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
            TestFile  = "test_enhancedcd.cdi",
            MediaType = MediaType.CDPLUS,
            Sectors   = 59206,
            Md5       = "31054e6b8f4d51fe502ac340490bcd46",
            LongMd5   = "2fc4b8966350322ed3fd553b9e628164",
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
            TestFile  = "test_incd_udf200_finalized.cdi",
            MediaType = MediaType.CDROMXA,
            Sectors   = 350134,
            Md5       = "d976a8d0131bf48926542160bb41fc13",
            LongMd5   = "cd55978d00f1bc127a0e652259ba2418",
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
                            Clusters     = 399371,
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
            TestFile      = "test_karaoke_multi_sampler.cdi",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 329158,
            Md5           = "a34e29e42b60023a6ae59f37d2bd4bea",
            LongMd5       = "e981f7dfdb522ba937fe75474e23a446",
            SubchannelMd5 = "62164333675c194acbe14eed5213e4f4",
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
            TestFile  = "test_multiple_indexes.cdi",
            MediaType = MediaType.CDDA,
            Sectors   = 65536,
            Md5       = "9315c6fc3cf5371ae3795df2b624bd5e",
            LongMd5   = "9315c6fc3cf5371ae3795df2b624bd5e",
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
                    Flags   = 2
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
            TestFile  = "test_multisession.cdi",
            MediaType = MediaType.CDROMXA,
            Sectors   = 51168,
            Md5       = "46e43ed4712e5ae61b653b4d19f27080",
            LongMd5   = "cac33e71b4693b2902f086a0a433129d",
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
                            Clusters     = 9276,
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
                            Clusters     = 7330,
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
                            Clusters     = 6408,
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
            TestFile  = "test_multisession_dvd+r.cdi",
            MediaType = MediaType.DVDROM,
            Sectors   = 230624,
            Md5       = "020993315e49ab0d36bc7248819162ea",
            LongMd5   = "020993315e49ab0d36bc7248819162ea",
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
                            Type        = "iso9660",
                            VolumeName  = "Session 1"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "test_multisession_dvd-r.cdi",
            MediaType = MediaType.DVDROM,
            Sectors   = 257264,
            Md5       = "dff8f2107a4ea9633a88ce38ff609b8e",
            LongMd5   = "dff8f2107a4ea9633a88ce38ff609b8e",
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
                            Type        = "iso9660",
                            VolumeName  = "Session 1"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile  = "test_videocd.cdi",
            MediaType = MediaType.CDROMXA,
            Sectors   = 48794,
            Md5       = "e5b596e73f46f646a51e1315b59e7cb9",
            LongMd5   = "acd1a8de676ebe6feeb9d6964ccd63ea",
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
                            Type        = "iso9660",
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
            TestFile      = "test_data_track_as_audio.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 50985,
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
                    Flags   = 1
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
            TestFile      = "test_data_track_as_audio_fixed_sub.cdi",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 50985,
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
                    Start   = 36789,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 2
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_disc_starts_at_track2.cdi",
            MediaType     = MediaType.CDROMXA,
            Sectors       = 50985,
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
                    Start   = 36789,
                    End     = 62384,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track111_in_session2.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 0,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1,
                    End     = 1,
                    Pregap  = 1,
                    Flags   = 1
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track111_in_session2_fixed_sub.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 0,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1,
                    End     = 1,
                    Pregap  = 1,
                    Flags   = 1
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile      = "test_track2_inside_track1.cdi",
            MediaType     = MediaType.CDDA,
            Sectors       = 0,
            MD5           = "UNKNOWN",
            LongMD5       = "UNKNOWN",
            SubchannelMD5 = "UNKNOWN",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 1,
                    End     = 1,
                    Pregap  = 1,
                    Flags   = 1
                }
            }
        },
        */

#endregion
    };
}