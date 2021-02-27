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

using System;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.DiscImages;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Toast
    {
        readonly string[] _testFiles =
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

        readonly ulong[] _sectors =
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

        readonly uint[] _sectorSize =
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

        readonly MediaType[] _mediaTypes =
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

        readonly string[] _md5S =
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

        readonly string[] _longMd5S =
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

        readonly string[] _subchannelMd5S =
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

        readonly int[] _tracks =
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

        readonly int[][] _trackSessions =
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

        readonly ulong[][] _trackStarts =
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

        readonly ulong[][] _trackEnds =
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

        readonly ulong[][] _trackPregaps =
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

        readonly byte[][] _trackFlags =
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

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Roxio Toast");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new ZZZRawImage();
                bool opened = image.Open(filter);

                Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                        Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                        Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");

                        Assert.AreEqual(_tracks[i], image.Tracks.Count, $"Tracks: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackSession).Should().
                              BeEquivalentTo(_trackSessions[i], $"Track session: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackStartSector).Should().
                              BeEquivalentTo(_trackStarts[i], $"Track start: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackEndSector).Should().
                              BeEquivalentTo(_trackEnds[i], $"Track end: {_testFiles[i]}");

                        image.Tracks.Select(t => t.TrackPregap).Should().
                              BeEquivalentTo(_trackPregaps[i], $"Track pregap: {_testFiles[i]}");

                        int trackNo = 0;

                        byte[] flags = new byte[image.Tracks.Count];

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                     SectorTagType.CdTrackFlags)[0];

                            trackNo++;
                        }

                        flags.Should().BeEquivalentTo(_trackFlags[i], $"Track flags: {_testFiles[i]}");
                    });
                }
            }
        }

        [Test]
        public void Hashes()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Roxio Toast");

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new ZZZRawImage();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    Md5Context ctx;

                    foreach(bool @long in new[]
                    {
                        false, true
                    })
                    {
                        ctx = new Md5Context();

                        foreach(Track currentTrack in image.Tracks)
                        {
                            ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                            ulong doneSectors = 0;

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if(sectors - doneSectors >= sectorsToRead)
                                {
                                    sector =
                                        @long ? image.ReadSectorsLong(doneSectors, sectorsToRead,
                                                                      currentTrack.TrackSequence)
                                            : image.ReadSectors(doneSectors, sectorsToRead, currentTrack.TrackSequence);

                                    doneSectors += sectorsToRead;
                                }
                                else
                                {
                                    sector =
                                        @long ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                      currentTrack.TrackSequence)
                                            : image.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                currentTrack.TrackSequence);

                                    doneSectors += sectors - doneSectors;
                                }

                                ctx.Update(sector);
                            }
                        }

                        Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(),
                                        $"{(@long ? "Long hash" : "Hash")}: {_testFiles[i]}");
                    }

                    if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        continue;

                    ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= sectorsToRead)
                            {
                                sector = image.ReadSectorsTag(doneSectors, sectorsToRead, currentTrack.TrackSequence,
                                                              SectorTagType.CdSectorSubchannel);

                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                              currentTrack.TrackSequence,
                                                              SectorTagType.CdSectorSubchannel);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(_subchannelMd5S[i], ctx.End(), $"Subchannel hash: {_testFiles[i]}");
                }
            });
        }
    }
}