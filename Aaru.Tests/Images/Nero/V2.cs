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
    public class V2 : OpticalMediaImageTest
    {
        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Nero Burning ROM", "V2");
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
                Sectors       = 232337,
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
                        Session = 2,
                        Start   = 27640,
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
                MD5           = "7119f623e909737e59732b935f103908",
                LongMD5       = "f1c1dbe1cd9df11fe2c1f0a97130c25f",
                SubchannelMD5 = "9e9a6b51bc2e5ec67400cb33ad0ca33f",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 169535,
                        Pregap  = 150,
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
                        Start   = 16399,
                        End     = 30050,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 29901,
                        End     = 47949,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 47800,
                        End     = 63313,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 63164,
                        End     = 78924,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 78775,
                        End     = 94731,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 94582,
                        End     = 117124,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 116975,
                        End     = 136165,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 136016,
                        End     = 154071,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 154072,
                        End     = 170750,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 170751,
                        End     = 186538,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 186539,
                        End     = 201798,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 201799,
                        End     = 224448,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 224449,
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
                TestFile      = "report_dvd+r-dl.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 3455936,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 3455935,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_dvd+rw.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 2295104,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 2295103,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_dvdram_v1.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 1218960,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 1218959,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_dvdram_v2.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 2236704,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 2236703,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_dvdrom.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 2146368,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 2146367,
                        Pregap  = 0,
                        Flags   = 0
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
                        Start   = 243738,
                        End     = 269899,
                        Pregap  = 0,
                        Flags   = 2
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 269900,
                        End     = 277845,
                        Pregap  = 0,
                        Flags   = 2
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_all_tracks_are_track1.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 25689,
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
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 36789,
                        End     = 37088,
                        Pregap  = 150,
                        Flags   = 0
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
                        Start   = 243738,
                        End     = 269899,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 269900,
                        End     = 270199,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_data_track_as_audio.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 51135,
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
                        End     = 62534,
                        Pregap  = 150,
                        Flags   = 2
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_data_track_as_audio_fixed_sub.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 51135,
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
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 36789,
                        End     = 62534,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_incd_udf200_finalized.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 350134,
                MD5           = "f95d6f978ddb4f98bbffda403f627fe1",
                LongMD5       = "efe2b3fe51022ef8e0a62587294d1d9c",
                SubchannelMD5 = "f8c96f120cac18c52178b99ef4c4e2a9",
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
                LongMD5       = "1b13a8f8aeb23f0b8bbc68518217e771",
                SubchannelMD5 = "25bae9e30657e2f64a45e5f690e3ae9e",
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
                MediaType     = MediaType.CDDA,
                Sectors       = 65536,
                MD5           = "1b13a8f8aeb23f0b8bbc68518217e771",
                LongMD5       = "199b85a01c27f55f463fc7d606adfafa",
                SubchannelMD5 = "48656afdbc40b6df06486a04a4d62401",
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
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "933f1699ba88a70aff5062f9626ef529",
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
                        End     = 26109,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 3,
                        Start   = 32710,
                        End     = 38627,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 4,
                        Start   = 45228,
                        End     = 51317,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_track1_overlaps_session2.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 25539,
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
                Sectors       = 51135,
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
                        End     = 25688,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 36789,
                        End     = 62534,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_track2_inside_track1.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 51135,
                MD5           = "6fa06c10561343438736a8d3d9a965ea",
                LongMD5       = "4a045788e69965efe0c87950d013e720",
                SubchannelMD5 = "UNKNOWN",
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
                        End     = 25688,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 36789,
                        End     = 62534,
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
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 949,
                        Pregap  = 150,
                        Flags   = 4
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 950,
                        End     = 49095,
                        Pregap  = 302,
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
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 29902,
                        End     = 65483,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 65334,
                        End     = 79025,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 78876,
                        End     = 95829,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 95680,
                        End     = 127046,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 126897,
                        End     = 156008,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 155859,
                        End     = 192884,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 192735,
                        End     = 224125,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 223976,
                        End     = 244937,
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
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 29902,
                        End     = 65483,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 65334,
                        End     = 79025,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 78876,
                        End     = 95829,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 95680,
                        End     = 127046,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 126897,
                        End     = 156008,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 155859,
                        End     = 192884,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 192735,
                        End     = 224125,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 223976,
                        End     = 244937,
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
                TestFile      = "make_data_dvd_iso9660-1999.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 82704,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82703,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_dvd_joliet.nrg",
                MediaType     = MediaType.DVDROM,
                Sectors       = 83072,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83071,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_iso9660-1999_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 82695,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82694,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_iso9660-1999_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 82695,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82694,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 83068,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83067,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 83068,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83067,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_102_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85364,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85363,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_102_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85364,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85363,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_150_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85364,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85363,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_150_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85364,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85363,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_150_sparing_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_150_sparing_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_150_virtual_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85368,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85367,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_150_virtual_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85368,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85367,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_200_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85366,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85365,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_200_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85366,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85365,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_200_sparing_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_200_sparing_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_200_virtual_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85370,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85369,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_200_virtual_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85370,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85369,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_201_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85366,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85365,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_201_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85366,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85365,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_201_sparing_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_201_sparing_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_201_virtual_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85370,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85369,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode1_joliet_udf_201_virtual_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85370,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85369,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_iso9660-1999_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 82697,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82696,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_iso9660-1999_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 82697,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82696,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 83082,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83081,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 83082,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 83081,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_102_physical_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85378,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85377,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_102_physical_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85378,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85377,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_150_physical_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85378,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85377,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_150_physical_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85378,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85377,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_150_sparing_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_150_sparing_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_150_virtual_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85382,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85381,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_150_virtual_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85382,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85381,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_200_physical_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85380,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85379,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_200_physical_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85380,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85379,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_200_sparing_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_200_sparing_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_200_virtual_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85384,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85383,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_200_virtual_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85384,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85383,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_201_physical_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85380,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85379,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_201_physical_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85380,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85379,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_201_sparing_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_201_sparing_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 86529,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 86528,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_201_virtual_dao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85384,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85383,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_mode2_joliet_udf_201_virtual_tao.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 85384,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85383,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_102_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84616,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84615,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_102_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84616,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84615,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_150_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84616,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84615,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_150_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84616,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84615,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_150_sparing_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85793,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85792,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_150_sparing_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85793,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85792,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_150_virtual_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84620,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84619,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_150_virtual_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84620,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84619,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_200_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84618,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84617,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_200_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84618,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84617,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_200_sparing_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85793,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85792,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_200_sparing_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85793,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85792,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_200_virtual_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84622,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84621,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_200_virtual_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84622,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84621,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_201_physical_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84618,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84617,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_201_physical_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84618,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84617,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_201_sparing_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85793,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85792,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_201_sparing_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 85793,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 85792,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_201_virtual_dao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84622,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84621,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_data_udf_201_virtual_tao.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 84622,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 84621,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_enhancedcd_dao.nrg",
                MediaType     = MediaType.CDPLUS,
                Sectors       = 326011,
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
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 29902,
                        End     = 65483,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 65334,
                        End     = 79025,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 78876,
                        End     = 95829,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 95680,
                        End     = 127046,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 126897,
                        End     = 156008,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 155859,
                        End     = 192884,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 192735,
                        End     = 224125,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 223976,
                        End     = 244937,
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
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 281259,
                        End     = 328223,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_enhancedcd_tao.nrg",
                MediaType     = MediaType.CDPLUS,
                Sectors       = 324361,
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
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 29902,
                        End     = 65483,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 65334,
                        End     = 79025,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 78876,
                        End     = 95829,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 95680,
                        End     = 127046,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 126897,
                        End     = 156008,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 155859,
                        End     = 192884,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 192735,
                        End     = 224125,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 223976,
                        End     = 244937,
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
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 281259,
                        End     = 328223,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_hdburn_full.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 727605,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 727604,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "make_hdburn.nrg",
                MediaType     = MediaType.CDROM,
                Sectors       = 31084,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 31083,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "make_mixed_mode_dao.nrg",
                MediaType = MediaType.CDROMXA,
                Sectors   = 362041,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82694,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 82695,
                        End     = 112896,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 112747,
                        End     = 148328,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 148179,
                        End     = 161870,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 161721,
                        End     = 178674,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 178525,
                        End     = 209891,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 209742,
                        End     = 238853,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 238704,
                        End     = 275729,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 275580,
                        End     = 306970,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 296263,
                        End     = 317224,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 317075,
                        End     = 343536,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 343387,
                        End     = 351632,
                        Pregap  = 150,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "make_mixed_mode_tao.nrg",
                MediaType = MediaType.CDROMXA,
                Sectors   = 360391,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 82694,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 82695,
                        End     = 112896,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 112747,
                        End     = 148328,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 148179,
                        End     = 161870,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 161721,
                        End     = 178674,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 178525,
                        End     = 209891,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 209742,
                        End     = 238853,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 238704,
                        End     = 275729,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 275580,
                        End     = 306970,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 296263,
                        End     = 317224,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 317075,
                        End     = 343536,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 343387,
                        End     = 351632,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            }
        };
    }
}