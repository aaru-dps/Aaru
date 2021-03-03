// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite5.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class BlindWrite5 : OpticalMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 5");
        public override IMediaImage _plugin => new DiscImages.BlindWrite5();

        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile  = "dvdrom.B5T",
                MediaType = MediaType.DVDROM,
                Sectors   = 0,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "gigarec.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 469652,
                MD5       = "e2e967adc0e5c530964ac4eebe8cac47",
                LongMD5   = "1dc7801008110af6b8015aad64d91739",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 469651,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "jaguarcd.B5T",
                MediaType = MediaType.CDDA,
                Sectors   = 243587,
                MD5       = "3dd5bd0f7d95a40d411761d69255567a",
                LongMD5   = "3dd5bd0f7d95a40d411761d69255567a",
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
                TestFile  = "pcengine.B5T",
                MediaType = MediaType.CD,
                Sectors   = 160956,
                MD5       = "4f5165069b3c5f11afe5f59711bd945d",
                LongMD5   = "fd30db9486f67654179c90c8a5052edb",
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
                        End     = 38463,
                        Pregap  = 0,
                        Flags   = 4
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
                TestFile  = "pcfx.B5T",
                MediaType = MediaType.CD,
                Sectors   = 246680,
                MD5       = "c1bc8de499756453d1387542bb32bb4d",
                LongMD5   = "455ec326506d2c5b974c4617c1010796",
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
                        Flags   = 4
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
                TestFile  = "report_audiocd.B5T",
                MediaType = MediaType.CDDA,
                Sectors   = 247073,
                MD5       = "c09f408a4416634d8ac1c1ffd0ed75a5",
                LongMD5   = "c09f408a4416634d8ac1c1ffd0ed75a5",
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
                TestFile  = "report_cdr.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 254265,
                MD5       = "65e79ef740833188a0f5be19da14c09d",
                LongMD5   = "47b32c32a6427ad1e6b4b1bd047df716",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 254264,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "report_cdrom.B5T",
                MediaType = MediaType.CDROM,
                Sectors   = 254265,
                MD5       = "bf4bbec517101d0d6f45d2e4d50cb875",
                LongMD5   = "3d3f9cf7d1ba2249b1e7960071e5af46",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 254264,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "report_cdrw_2x.B5T",
                MediaType = MediaType.CDRW,
                Sectors   = 308224,
                MD5       = "1e55aa420ca8f8ea77d5b597c9cfc19b",
                LongMD5   = "3af5f943ddb9427d9c63a4ce3b704db9",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 308223,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_all_tracks_are_track1.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 0,
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
                        Start   = 25539,
                        End     = 51077,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_audiocd_cdtext.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 277696,
                MD5       = "7c8fc7bb768cff15d702ac8cd10108d7",
                LongMD5   = "7c8fc7bb768cff15d702ac8cd10108d7",
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
                TestFile  = "test_castrated_leadout.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 0,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
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
                TestFile  = "test_data_track_as_audio.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 62385,
                MD5       = "ce3d63e831b4e6191b05ec9ce452ad91",
                LongMD5   = "4bd5511229857ca167b45e607dea12dc",
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
                TestFile  = "test_data_track_as_audio_fixed_sub.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 62385,
                MD5       = "ce3d63e831b4e6191b05ec9ce452ad91",
                LongMD5   = "4bd5511229857ca167b45e607dea12dc",
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
                TestFile  = "test_disc_starts_at_track2.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 62385,
                MD5       = "25fb1b49726aaac09196ea56490beeb1",
                LongMD5   = "8fd0dbe9085363cc20709f0ca76a373d",
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
                TestFile  = "test_enhancedcd.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 59206,
                MD5       = "3736dbfcb7bf5648e3ac067379087001",
                LongMD5   = "c2dfd5a32678c3ff049c143c98ad36a5",
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
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_incd_udf200_finalized.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 350134,
                MD5       = "901e4fe17ea6591b1fd53ba822428ef4",
                LongMD5   = "7b489457540c40037aabcf3f21e0201e",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 350133,
                        Pregap  = 150,
                        Flags   = 7
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_multiple_indexes.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 65536,
                MD5       = "1b13a8f8aeb23f0b8bbc68518217e771",
                LongMD5   = "1b13a8f8aeb23f0b8bbc68518217e771",
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
                TestFile  = "test_multisession.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 51168,
                MD5       = "e2e19cf38891e67a0829d01842b4052e",
                LongMD5   = "3e646a04eb29a8e0ad892b6ac00ba962",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 8132,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 19383,
                        End     = 25959,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 3,
                        Start   = 32710,
                        End     = 38477,
                        Pregap  = 150,
                        Flags   = 4
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
                TestFile  = "test_track1_overlaps_session2.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 0,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_track2_inside_session2_leadin.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 62385,
                MD5       = "4e797aa5dedaac71a0e67ebd9ac9d555",
                LongMD5   = "311d641c93a3fe1dfae7deb3a2be28c7",
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
                        Start   = 36789,
                        End     = 62384,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_track2_inside_track1.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 0,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
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
            },
            new OpticalImageTestExpected
            {
                TestFile  = "test_videocd.B5T",
                MediaType = MediaType.CDR,
                Sectors   = 48794,
                MD5       = "203a40d27b9bee018705c2df8d15e96d",
                LongMD5   = "a686cade367db0a12fef1d9862f39e1d",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 1251,
                        Pregap  = 150,
                        Flags   = 4
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
            }
        };
    }
}