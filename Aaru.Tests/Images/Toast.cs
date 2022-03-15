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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Images;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

[TestFixture]
public class Toast : OpticalMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Roxio Toast");
    public override IMediaImage Plugin => new ZZZRawImage();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "toast_3.5.7_iso9660_xa_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_dos_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_dos.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ebook_eng.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ebook_fra.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_joliet_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_joliet.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_mac_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_mac.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ver_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ver_dos_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ver_dos.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ver_joliet_apple.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ver_joliet.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
            TestFile      = "toast_3.5.7_iso9660_xa_ver.toast.lz",
            MediaType     = MediaType.CD,
            Sectors       = 0,
            Md5           = "UNKNOWN",
            LongMd5       = "UNKNOWN",
            SubchannelMd5 = "UNKNOWN",
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
        }
    };
}