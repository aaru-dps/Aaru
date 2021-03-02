// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDRWin10.cs
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
    public class CDRWin10 : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "report_audiocd.xmd", "report_cdrom.xmd", "report_cdrw_2x.xmd", "report_cdr.xmd", "report_enhancedcd.xmd",
            "test_karaoke_multi_sampler.xmd"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // report_audiocd.xmd
            0,

            // report_cdrom.xmd
            0,

            // report_cdrw_2x.xmd
            0,

            // report_cdr.xmd
            0,

            // report_enhancedcd.xmd
            0,

            // test_karaoke_multi_sampler.xmd
            0
        };
        public override uint[] _sectorSize => null;

        public override MediaType[] _mediaTypes => new[]
        {
            // report_audiocd.xmd
            MediaType.CDDA,

            // report_cdrom.xmd
            MediaType.CDROM,

            // report_cdrw_2x.xmd
            MediaType.CDRW,

            // report_cdr.xmd
            MediaType.CDR,

            // report_enhancedcd.xmd
            MediaType.CDPLUS,

            // test_karaoke_multi_sampler.xmd
            MediaType.CD
        };

        public override string[] _md5S => new[]
        {
            // report_audiocd.xmd
            "UNKNOWN",

            // report_cdrom.xmd
            "UNKNOWN",

            // report_cdrw_2x.xmd
            "UNKNOWN",

            // report_cdr.xmd
            "UNKNOWN",

            // report_enhancedcd.xmd
            "UNKNOWN",

            // test_karaoke_multi_sampler.xmd
            "UNKNOWN"
        };

        public override string[] _longMd5S => new[]
        {
            // report_audiocd.xmd
            "UNKNOWN",

            // report_cdrom.xmd
            "UNKNOWN",

            // report_cdrw_2x.xmd
            "UNKNOWN",

            // report_cdr.xmd
            "UNKNOWN",

            // report_enhancedcd.xmd
            "UNKNOWN",

            // test_karaoke_multi_sampler.xmd
            "UNKNOWN"
        };

        public override string[] _subchannelMd5S => new[]
        {
            // report_audiocd.xmd
            "UNKNOWN",

            // report_cdrom.xmd
            "UNKNOWN",

            // report_cdrw_2x.xmd
            "UNKNOWN",

            // report_cdr.xmd
            "UNKNOWN",

            // report_enhancedcd.xmd
            "UNKNOWN",

            // test_karaoke_multi_sampler.xmd
            "UNKNOWN"
        };

        public override int[] _tracks => new[]
        {
            // report_audiocd.xmd
            1,

            // report_cdrom.xmd
            1,

            // report_cdrw_2x.xmd
            1,

            // report_cdr.xmd
            1,

            // report_enhancedcd.xmd
            1,

            // test_karaoke_multi_sampler.xmd
            1
        };

        public override int[][] _trackSessions => new[]
        {
            // report_audiocd.xmd
            new[]
            {
                1
            },

            // report_cdrom.xmd
            new[]
            {
                1
            },

            // report_cdrw_2x.xmd
            new[]
            {
                1
            },

            // report_cdr.xmd
            new[]
            {
                1
            },

            // report_enhancedcd.xmd
            new[]
            {
                1
            },

            // test_karaoke_multi_sampler.xmd
            new[]
            {
                1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // report_audiocd.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom.xmd
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.xmd
            new ulong[]
            {
                0
            },

            // report_cdr.xmd
            new ulong[]
            {
                0
            },

            // report_enhancedcd.xmd
            new ulong[]
            {
                0
            },

            // test_karaoke_multi_sampler.xmd
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // report_audiocd.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom.xmd
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.xmd
            new ulong[]
            {
                0
            },

            // report_cdr.xmd
            new ulong[]
            {
                0
            },

            // report_enhancedcd.xmd
            new ulong[]
            {
                0
            },

            // test_karaoke_multi_sampler.xmd
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // report_audiocd.xmd
            new ulong[]
            {
                0
            },

            // report_cdrom.xmd
            new ulong[]
            {
                0
            },

            // report_cdrw_2x.xmd
            new ulong[]
            {
                0
            },

            // report_cdr.xmd
            new ulong[]
            {
                0
            },

            // report_enhancedcd.xmd
            new ulong[]
            {
                0
            },

            // test_karaoke_multi_sampler.xmd
            new ulong[]
            {
                0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // report_audiocd.xmd
            new byte[]
            {
                0
            },

            // report_cdrom.xmd
            new byte[]
            {
                0
            },

            // report_cdrw_2x.xmd
            new byte[]
            {
                0
            },

            // report_cdr.xmd
            new byte[]
            {
                0
            },

            // report_enhancedcd.xmd
            new byte[]
            {
                0
            },

            // test_karaoke_multi_sampler.xmd
            new byte[]
            {
                0
            }
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CDRWin 10");
        public override IMediaImage _plugin => new DiscImages.Alcohol120();
    }
}