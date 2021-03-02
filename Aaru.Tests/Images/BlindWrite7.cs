// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite6.cs
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
    public class BlindWrite7 : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "report_cdr.B6T", "report_cdrom.B6T", "report_cdrw.B6T"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // report_cdr.B6T
            254265,

            // report_cdrom.B6T
            254265,

            // report_cdrw.B6T
            308224
        };
        public override uint[] _sectorSize => new uint[]
        {
            2048, 2048, 2048
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // report_cdr.B6T
            MediaType.CDR,

            // report_cdrom.B6T
            MediaType.CDROM,

            // report_cdrw.B6T
            MediaType.CDRW
        };

        public override string[] _md5S => new[]
        {
            // report_cdr.B6T
            "86b8a763ef6522fccf97f743d7bf4fa3",

            // report_cdrom.B6T
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.B6T
            "1e55aa420ca8f8ea77d5b597c9cfc19b"
        };

        public override string[] _longMd5S => new[]
        {
            // report_cdr.B6T
            "a292359cce05849dec1d06ae471ecf9e",

            // report_cdrom.B6T
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.B6T
            "3af5f943ddb9427d9c63a4ce3b704db9"
        };

        public override string[] _subchannelMd5S => new string[]
        {
            // report_cdr.B6T
            null,

            // report_cdrom.B6T
            null,

            // report_cdrw.B6T
            null
        };

        public override int[] _tracks => new[]
        {
            // report_cdr.B6T
            1,

            // report_cdrom.B6T
            1,

            // report_cdrw.B6T
            1
        };

        public override int[][] _trackSessions => new[]
        {
            // report_cdr.B6T
            new[]
            {
                1
            },

            // report_cdrom.B6T
            new[]
            {
                1
            },

            // report_cdrw.B6T
            new[]
            {
                1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // report_cdr.B6T
            new ulong[]
            {
                0
            },

            // report_cdrom.B6T
            new ulong[]
            {
                0
            },

            // report_cdrw.B6T
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // report_cdr.B6T
            new ulong[]
            {
                254264
            },

            // report_cdrom.B6T
            new ulong[]
            {
                254264
            },

            // report_cdrw.B6T
            new ulong[]
            {
                308223
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // report_cdr.B6T
            new ulong[]
            {
                150
            },

            // report_cdrom.B6T
            new ulong[]
            {
                150
            },

            // report_cdrw.B6T
            new ulong[]
            {
                150
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // report_cdr.B6T
            new byte[]
            {
                4
            },

            // report_cdrom.B6T
            new byte[]
            {
                4
            },

            // report_cdrw.B6T
            new byte[]
            {
                4
            }
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 7");
        public override IMediaImage _plugin => new DiscImages.BlindWrite5();
    }
}