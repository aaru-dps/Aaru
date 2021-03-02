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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images.Nero
{
    [TestFixture]
    public class V1 : OpticalMediaImageTest
    {
        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Nero Burning ROM", "V1");
        public override IMediaImage _plugin => new DiscImages.Nero();

        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile      = "cdiready_the_apprentice.nrg",
                MediaType     = MediaType.CDDA,
                Sectors       = 279300,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 69150,
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
                MD5           = "79ade978aad90667f272a693012c11ca",
                LongMD5       = "8086a3654d6dede562621d24ae18729e",
                SubchannelMD5 = "83ec1010fc44694d69dc48bacec5481a",
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
                        Session = 1,
                        Start   = 27640,
                        End     = 28236,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 28237,
                        End     = 78891,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 78892,
                        End     = 100053,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 100054,
                        End     = 133202,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 133203,
                        End     = 160907,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 160908,
                        End     = 181465,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 181466,
                        End     = 202023,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 202024,
                        End     = 222581,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 222582,
                        End     = 243139,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 243140,
                        End     = 243586,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "pcengine.nrg",
                MediaType     = MediaType.CD,
                Sectors       = 160956,
                MD5           = "7119f623e909737e59732b935f103908",
                LongMD5       = "f1c1dbe1cd9df11fe2c1f0a97130c25f",
                SubchannelMD5 = "9e9a6b51bc2e5ec67400cb33ad0ca33f",
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
                        Start   = 3590,
                        End     = 38463,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 38614,
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
                        Start   = 126229,
                        End     = 160955,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "pcfx.nrg",
                MediaType     = MediaType.CD,
                Sectors       = 246680,
                MD5           = "5a1ed6d71094e8e7ae53b6604a6fcc0a",
                LongMD5       = "dac5dc0961fa435da3c7d433477cda1a",
                SubchannelMD5 = "e3a0d78b6c32f5795b1b513bd13a6bda",
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
                        Start   = 4395,
                        End     = 4908,
                        Pregap  = 150,
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
                        Start   = 220795,
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
                TestFile      = "report_audiocd.nrg",
                MediaType     = MediaType.CDDA,
                Sectors       = 247073,
                MD5           = "c09f408a4416634d8ac1c1ffd0ed75a5",
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
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrom.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 254265,
                MD5           = "bf4bbec517101d0d6f45d2e4d50cb875",
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
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrw.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 308224,
                MD5           = "3af5f943ddb9427d9c63a4ce3b704db9",
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
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_enhancedcd.nrg",
                MediaType     = MediaType.CDPLUS,
                Sectors       = 303316,
                MD5           = "dfd6c0bd02c19145b2a64d8a15912302",
                LongMD5       = "0038395e272242a29e84a1fb34a3a15e",
                SubchannelMD5 = "e6f7319532f46c3fa4fd3569c65546e1",
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
                        Start   = 234180,
                        End     = 303315,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_audiocd_cdtext.nrg",
                MediaType     = MediaType.CDDA,
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
                        Start   = 36939,
                        End     = 62384,
                        Pregap  = 150,
                        Flags   = 2
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_incd_udf200_finalized.nrg",
                MediaType     = MediaType.CDROMXA,
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
                        Flags   = 7
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_multi_karaoke_sampler.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 329158,
                MD5           = "1731384a29149b7e6f4c0d0d07f178ca",
                LongMD5       = "efe2b3fe51022ef8e0a62587294d1d9c",
                SubchannelMD5 = "f8c96f120cac18c52178b99ef4c4e2a9",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 1736,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 1887,
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
                MediaType     = MediaType.CD,
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
                TestFile      = "test_multisession.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 51168,
                MD5           = "f793fecc486a83cbe05b51c2d98059b9",
                LongMD5       = "199b85a01c27f55f463fc7d606adfafa",
                SubchannelMD5 = "48656afdbc40b6df06486a04a4d62401",
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
                        Start   = 19533,
                        End     = 25959,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 3,
                        Start   = 32860,
                        End     = 38477,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 4,
                        Start   = 45378,
                        End     = 51167,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_track2_inside_session2_leadin.nrg",
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
                        Pregap  = 150,
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
                TestFile      = "test_track2_inside_track1.nrg",
                MediaType     = MediaType.CDROMXA,
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
                        End     = 13199,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 13350,
                        End     = 25688,
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
                TestFile      = "test_videocd.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 48794,
                MD5           = "ec7c86e6cfe5f965faa2488ae940e15a",
                LongMD5       = "4a045788e69965efe0c87950d013e720",
                SubchannelMD5 = "935a91f5850352818d92b71f1c87c393",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 1101,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 1252,
                        End     = 48793,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_audiocd_dao.nrg",
                MediaType     = MediaType.CDDA,
                Sectors       = 279196,
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
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 27454,
                        End     = 63035,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 62934,
                        End     = 76625,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 4428,
                        End     = 21381,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 22432,
                        End     = 53798,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 54833,
                        End     = 83944,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 9459,
                        End     = 46484,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 45087,
                        End     = 76477,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 4360,
                        End     = 25321,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 244938,
                        End     = 271399,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 271250,
                        End     = 279495,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_audiocd_tao.nrg",
                MediaType     = MediaType.CDDA,
                Sectors       = 277696,
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
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 27454,
                        End     = 63035,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 62934,
                        End     = 76625,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 4428,
                        End     = 21381,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 22432,
                        End     = 53798,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 54833,
                        End     = 83944,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 9459,
                        End     = 46484,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 45087,
                        End     = 76477,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 4360,
                        End     = 25321,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 244938,
                        End     = 271399,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 271250,
                        End     = 279495,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 83078,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83077,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_level2_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 83084,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83083,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_level2_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 83084,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83083,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 83078,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83077,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_udf_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85733,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85732,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_udf_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85733,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85732,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 83092,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83091,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_level2_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 83092,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83091,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_level2_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 83092,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83091,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 83092,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83091,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_udf_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85747,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85746,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_udf_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85747,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85746,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_mixed_mode_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 325928,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 46581,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 45382,
                        End     = 75583,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 4586,
                        End     = 40167,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 36450,
                        End     = 50141,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 49960,
                        End     = 66913,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 67964,
                        End     = 99330,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 27229,
                        End     = 56340,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 58575,
                        End     = 95600,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 23403,
                        End     = 53593,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 264817,
                        End     = 285778,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 285629,
                        End     = 312090,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 311941,
                        End     = 320186,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_mixed_mode_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 324278,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 46581,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 45382,
                        End     = 75583,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 4586,
                        End     = 40167,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 36450,
                        End     = 50141,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 49960,
                        End     = 66913,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 67964,
                        End     = 99330,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 27229,
                        End     = 56340,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 58575,
                        End     = 95600,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 23403,
                        End     = 53593,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 264817,
                        End     = 285778,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 285629,
                        End     = 312090,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 311941,
                        End     = 320186,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_udf_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84985,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84984,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_udf_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84985,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84984,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            }
        };
    }
}