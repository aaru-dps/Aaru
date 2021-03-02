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
        public override string[] _testFiles => new[]
        {
            "report_cdrom_cooked_nodpm.xmd", "report_cdrom_cooked.xmd", "report_cdrom_nodpm.xmd", "report_cdrom.xmd",
            "report_cdrw.xmd", "report_cdr.xmd", "report_dvdram_v1.xmd", "report_dvdram_v2.xmd", "report_dvd+r-dl.xmd",
            "report_dvdrom.xmd", "report_dvd+rw.xmd", "report_dvd-rw.xmd", "report_dvd+r.xmd", "report_dvd-r.xmd"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // report_cdrom_cooked_nodpm.xmd
            0,

            // report_cdrom_cooked.xmd
            0,

            // report_cdrom_nodpm.xmd
            0,

            // report_cdrom.xmd
            0,

            // report_cdrw.xmd
            0,

            // report_cdr.xmd
            0,

            // report_dvdram_v1.xmd
            0,

            // report_dvdram_v2.xmd
            0,

            // report_dvd+r-dl.xmd
            0,

            // report_dvdrom.xmd
            0,

            // report_dvd+rw.xmd
            0,

            // report_dvd-rw.xmd
            0,

            // report_dvd+r.xmd
            0,

            // report_dvd-r.xmd
            0
        };
        public override uint[] _sectorSize => null;

        public override MediaType[] _mediaTypes => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            MediaType.CDROM,

            // report_cdrom_cooked.xmd
            MediaType.CDROM,

            // report_cdrom_nodpm.xmd
            MediaType.CDROM,

            // report_cdrom.xmd
            MediaType.CDROM,

            // report_cdrw.xmd
            MediaType.CDRW,

            // report_cdr.xmd
            MediaType.CDR,

            // report_dvdram_v1.xmd
            MediaType.DVDRAM,

            // report_dvdram_v2.xmd
            MediaType.DVDRAM,

            // report_dvd+r-dl.xmd
            MediaType.DVDPRDL,

            // report_dvdrom.xmd
            MediaType.DVDROM,

            // report_dvd+rw.xmd
            MediaType.DVDPRW,

            // report_dvd-rw.xmd
            MediaType.DVDRW,

            // report_dvd+r.xmd
            MediaType.DVDPR,

            // report_dvd-r.xmd
            MediaType.DVDR
        };

        public override string[] _md5S => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            "UNKNOWN",

            // report_cdrom_cooked.xmd
            "UNKNOWN",

            // report_cdrom_nodpm.xmd
            "UNKNOWN",

            // report_cdrom.xmd
            "UNKNOWN",

            // report_cdrw.xmd
            "UNKNOWN",

            // report_cdr.xmd
            "UNKNOWN",

            // report_dvdram_v1.xmd
            "UNKNOWN",

            // report_dvdram_v2.xmd
            "UNKNOWN",

            // report_dvd+r-dl.xmd
            "UNKNOWN",

            // report_dvdrom.xmd
            "UNKNOWN",

            // report_dvd+rw.xmd
            "UNKNOWN",

            // report_dvd-rw.xmd
            "UNKNOWN",

            // report_dvd+r.xmd
            "UNKNOWN",

            // report_dvd-r.xmd
            "UNKNOWN"
        };

        public override string[] _longMd5S => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            "UNKNOWN",

            // report_cdrom_cooked.xmd
            "UNKNOWN",

            // report_cdrom_nodpm.xmd
            "UNKNOWN",

            // report_cdrom.xmd
            "UNKNOWN",

            // report_cdrw.xmd
            "UNKNOWN",

            // report_cdr.xmd
            "UNKNOWN",

            // report_dvdram_v1.xmd
            "UNKNOWN",

            // report_dvdram_v2.xmd
            "UNKNOWN",

            // report_dvd+r-dl.xmd
            "UNKNOWN",

            // report_dvdrom.xmd
            "UNKNOWN",

            // report_dvd+rw.xmd
            "UNKNOWN",

            // report_dvd-rw.xmd
            "UNKNOWN",

            // report_dvd+r.xmd
            "UNKNOWN",

            // report_dvd-r.xmd
            "UNKNOWN"
        };

        public override string[] _subchannelMd5S => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            "UNKNOWN",

            // report_cdrom_cooked.xmd
            "UNKNOWN",

            // report_cdrom_nodpm.xmd
            "UNKNOWN",

            // report_cdrom.xmd
            "UNKNOWN",

            // report_cdrw.xmd
            "UNKNOWN",

            // report_cdr.xmd
            "UNKNOWN",

            // report_dvdram_v1.xmd
            null,

            // report_dvdram_v2.xmd
            null,

            // report_dvd+r-dl.xmd
            null,

            // report_dvdrom.xmd
            null,

            // report_dvd+rw.xmd
            null,

            // report_dvd-rw.xmd
            null,

            // report_dvd+r.xmd
            null,

            // report_dvd-r.xmd
            null
        };

        public override int[] _tracks => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            1,

            // report_cdrom_cooked.xmd
            1,

            // report_cdrom_nodpm.xmd
            1,

            // report_cdrom.xmd
            1,

            // report_cdrw.xmd
            1,

            // report_cdr.xmd
            1,

            // report_dvdram_v1.xmd
            1,

            // report_dvdram_v2.xmd
            1,

            // report_dvd+r-dl.xmd
            1,

            // report_dvdrom.xmd
            1,

            // report_dvd+rw.xmd
            1,

            // report_dvd-rw.xmd
            1,

            // report_dvd+r.xmd
            1,

            // report_dvd-r.xmd
            1
        };

        public override int[][] _trackSessions => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            new[]
            {
                1
            },

            // report_cdrom_cooked.xmd
            new[]
            {
                1
            },

            // report_cdrom_nodpm.xmd
            new[]
            {
                1
            },

            // report_cdrom.xmd
            new[]
            {
                1
            },

            // report_cdrw.xmd
            new[]
            {
                1
            },

            // report_cdr.xmd
            new[]
            {
                1
            },

            // report_dvdram_v1.xmd
            new[]
            {
                1
            },

            // report_dvdram_v2.xmd
            new[]
            {
                1
            },

            // report_dvd+r-dl.xmd
            new[]
            {
                1
            },

            // report_dvdrom.xmd
            new[]
            {
                1
            },

            // report_dvd+rw.xmd
            new[]
            {
                1
            },

            // report_dvd-rw.xmd
            new[]
            {
                1
            },

            // report_dvd+r.xmd
            new[]
            {
                1
            },

            // report_dvd-r.xmd
            new[]
            {
                1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom_cooked.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom_nodpm.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom.xmd
            new ulong[]
            {
                0
            },

            // report_cdrw.xmd
            new ulong[]
            {
                0
            },

            // report_cdr.xmd
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.xmd
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.xmd
            new ulong[]
            {
                0
            },

            // report_dvdrom.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+rw.xmd
            new ulong[]
            {
                0
            },

            // report_dvd-rw.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+r.xmd
            new ulong[]
            {
                0
            },

            // report_dvd-r.xmd
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom_cooked.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom_nodpm.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom.xmd
            new ulong[]
            {
                0
            },

            // report_cdrw.xmd
            new ulong[]
            {
                0
            },

            // report_cdr.xmd
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.xmd
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.xmd
            new ulong[]
            {
                0
            },

            // report_dvdrom.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+rw.xmd
            new ulong[]
            {
                0
            },

            // report_dvd-rw.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+r.xmd
            new ulong[]
            {
                0
            },

            // report_dvd-r.xmd
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom_cooked.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom_nodpm.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom.xmd
            new ulong[]
            {
                0
            },

            // report_cdrw.xmd
            new ulong[]
            {
                0
            },

            // report_cdr.xmd
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.xmd
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.xmd
            new ulong[]
            {
                0
            },

            // report_dvdrom.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+rw.xmd
            new ulong[]
            {
                0
            },

            // report_dvd-rw.xmd
            new ulong[]
            {
                0
            },

            // report_dvd+r.xmd
            new ulong[]
            {
                0
            },

            // report_dvd-r.xmd
            new ulong[]
            {
                0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // report_cdrom_cooked_nodpm.xmd
            new byte[]
            {
                0
            },

            // report_cdrom_cooked.xmd
            new byte[]
            {
                0
            },

            // report_cdrom_nodpm.xmd
            new byte[]
            {
                0
            },

            // report_cdrom.xmd
            new byte[]
            {
                0
            },

            // report_cdrw.xmd
            new byte[]
            {
                0
            },

            // report_cdr.xmd
            new byte[]
            {
                0
            },

            // report_dvdram_v1.xmd
            null,

            // report_dvdram_v2.xmd
            null,

            // report_dvd+r-dl.xmd
            null,

            // report_dvdrom.xmd
            null,

            // report_dvd+rw.xmd
            null,

            // report_dvd-rw.xmd
            null,

            // report_dvd+r.xmd
            null,

            // report_dvd-r.xmd
            null
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "HD-COPY");
        public override IMediaImage _plugin => new DiscImages.Alcohol120();
    }
}