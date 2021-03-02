// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Toast.cs
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
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Toast : OpticalMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "toast_3.5.7_iso9660_xa_apple.toast.lz", "toast_3.5.7_iso9660_xa_dos_apple.toast.lz",
            "toast_3.5.7_iso9660_xa_dos.toast.lz", "toast_3.5.7_iso9660_xa_ebook_eng.toast.lz",
            "toast_3.5.7_iso9660_xa_ebook_fra.toast.lz", "toast_3.5.7_iso9660_xa_joliet_apple.toast.lz",
            "toast_3.5.7_iso9660_xa_joliet.toast.lz", "toast_3.5.7_iso9660_xa_mac_apple.toast.lz",
            "toast_3.5.7_iso9660_xa_mac.toast.lz", "toast_3.5.7_iso9660_xa.toast.lz",
            "toast_3.5.7_iso9660_xa_ver_apple.toast.lz", "toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz",
            "toast_3.5.7_iso9660_xa_ver_dos.toast.lz", "toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz",
            "toast_3.5.7_iso9660_xa_ver_joliet.toast.lz", "toast_3.5.7_iso9660_xa_ver.toast.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            0,

            // toast_3.5.7_iso9660_xa.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            0
        };

        public override uint[] _sectorSize => new uint[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            0,

            // toast_3.5.7_iso9660_xa.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            0,

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            0
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            MediaType.CD,

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            MediaType.CD
        };

        public override string[] _md5S => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            "UNKNOWN"
        };

        public override string[] _longMd5S => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            "UNKNOWN"
        };

        public override string[] _subchannelMd5S => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            "UNKNOWN",

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            "UNKNOWN"
        };

        public override int[] _tracks => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            1,

            // toast_3.5.7_iso9660_xa.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            1,

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            1
        };

        public override int[][] _trackSessions => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            new[]
            {
                1
            },

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            new[]
            {
                1
            }
        };

        public override ulong[][] _trackStarts => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackEnds => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            new ulong[]
            {
                0
            }
        };

        public override ulong[][] _trackPregaps => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            new ulong[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            new ulong[]
            {
                0
            }
        };

        public override byte[][] _trackFlags => new[]
        {
            // toast_3.5.7_iso9660_xa_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_dos.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_eng.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ebook_fra.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_joliet.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_mac.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_dos.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver_joliet.toast.lz
            new byte[]
            {
                0
            },

            // toast_3.5.7_iso9660_xa_ver.toast.lz
            new byte[]
            {
                0
            }
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Roxio Toast");
        public override IMediaImage _plugin => new ZZZRawImage();
    }
}