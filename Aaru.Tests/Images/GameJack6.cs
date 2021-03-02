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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class GameJack6 : OpticalMediaImageTest
    {
        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "HD-COPY");
        public override IMediaImage _plugin => new DiscImages.Alcohol120();

        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrom_cooked_nodpm.xmd",
                MediaType     = MediaType.CDROM,
                Sectors       = 0,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrom_cooked.xmd",
                MediaType     = MediaType.CDROM,
                Sectors       = 0,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrom_nodpm.xmd",
                MediaType     = MediaType.CDROM,
                Sectors       = 0,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrom.xmd",
                MediaType     = MediaType.CDROM,
                Sectors       = 0,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdrw.xmd",
                MediaType     = MediaType.CDRW,
                Sectors       = 0,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile      = "report_cdr.xmd",
                MediaType     = MediaType.CDR,
                Sectors       = 0,
                MD5           = "UNKNOWN",
                LongMD5       = "UNKNOWN",
                SubchannelMD5 = "UNKNOWN",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 0,
                        Pregap  = 0,
                        Flags   = 0
                    }
                }
            },
            new OpticalImageTestExpected
            {
                TestFile  = "report_dvdram_v1.xmd",
                MediaType = MediaType.DVDRAM,
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
                TestFile  = "report_dvdram_v2.xmd",
                MediaType = MediaType.DVDRAM,
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
                TestFile  = "report_dvd+r-dl.xmd",
                MediaType = MediaType.DVDPRDL,
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
                TestFile  = "report_dvdrom.xmd",
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
                TestFile  = "report_dvd+rw.xmd",
                MediaType = MediaType.DVDPRW,
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
                TestFile  = "report_dvd-rw.xmd",
                MediaType = MediaType.DVDRW,
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
                TestFile  = "report_dvd+r.xmd",
                MediaType = MediaType.DVDPR,
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
                TestFile  = "report_dvd-r.xmd",
                MediaType = MediaType.DVDR,
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
            }
        };
    }
}