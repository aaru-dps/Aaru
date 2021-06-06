// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Nero.cs
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

namespace Aaru.Tests.Images.UltraISO
{
    [TestFixture]
    public class Nero : OpticalMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "UltraISO", "Nero");
        public override IMediaImage _plugin => new DiscImages.Nero();

        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile      = "cdiready_the_apprentice.nrg",
                MediaType     = MediaType.CDDA,
                Sectors       = 210150,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 19649,
                        Pregap  = 150,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 88800,
                        End     = 38474,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 107625,
                        End     = 43049,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 112200,
                        End     = 64499,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 133650,
                        End     = 69074,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 138225,
                        End     = 90674,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 159825,
                        End     = 95624,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 164775,
                        End     = 116249,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 185400,
                        End     = 120974,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 121125,
                        End     = 139874,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 139875,
                        End     = 143999,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 144000,
                        End     = 163199,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 163200,
                        End     = 167699,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 167700,
                        End     = 172874,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 172875,
                        End     = 187124,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 190125,
                        End     = 121724,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 208875,
                        End     = 148499,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 213000,
                        End     = 145574,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 232200,
                        End     = 165674,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 236700,
                        End     = 169199,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 241875,
                        End     = 175349,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 256125,
                        End     = 191999,
                        Pregap  = 18446744073709482466,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_audiocd.nrg",
                MediaType     = MediaType.CDDA,
                Sectors       = 247073,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
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
                        Start   = 154222,
                        End     = 170900,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 170901,
                        End     = 186688,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 186689,
                        End     = 201948,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 201949,
                        End     = 224598,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 224599,
                        End     = 247222,
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
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
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
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
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
                TestFile  = "report_dvdram_v2.nrg",
                MediaType = MediaType.DVDROM,
                Sectors   = 471090,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 18446744073709551466,
                        End     = 471089,
                        Pregap  = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "report_dvd-r+dl.nrg",
                MediaType = MediaType.DVDROM,
                Sectors   = 3455920,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 18446744073709551466,
                        End     = 3455919,
                        Pregap  = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "report_dvdrom.nrg",
                MediaType = MediaType.DVDROM,
                Sectors   = 2146357,
                MD5       = "UNKNOWN",
                LongMD5   = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 18446744073709551466,
                        End     = 2146356,
                        Pregap  = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_enhancedcd.nrg",
                MediaType     = MediaType.CDPLUS,
                Sectors       = 292066,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
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
                        Start   = 153568,
                        End     = 167081,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 167082,
                        End     = 187262,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 187263,
                        End     = 201590,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 201591,
                        End     = 222929,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 2,
                        Start   = 256021,
                        End     = 325456,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "test_multi_karaoke_sampler.nrg",
                MediaType     = MediaType.CDROMXA,
                Sectors       = 329008,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 1736,
                        Pregap  = 150,
                        Flags   = 0
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
                        Start   = 175732,
                        End     = 206366,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 206367,
                        End     = 226355,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 226356,
                        End     = 244260,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 244261,
                        End     = 273870,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 273871,
                        End     = 293657,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 293658,
                        End     = 310616,
                        Pregap  = 0,
                        Flags   = 0
                    },
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 175826,
                        End     = 194272,
                        Pregap  = 0,
                        Flags   = 4
                    }
                }
            }
        };
    }
}