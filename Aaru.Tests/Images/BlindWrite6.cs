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

using System;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class BlindWrite6
    {
        readonly string[] _testFiles =
        {
            "dvdrom.B6T", "jaguarcd.B6T", "pcengine.B6T", "pcfx.B6T", "report_cdr.B6T", "report_cdrom.B6T",
            "report_cdrw_2x.B6T", "test_karaoke_multi_sampler.B6T"
        };

        readonly ulong[] _sectors =
        {
            // dvdrom.B6T
            0,

            // jaguarcd.B6T
            243587,

            // pcengine.B6T
            160956,

            // pcfx.B6T
            246680,

            // report_cdr.B6T
            254265,

            // report_cdrom.B6T
            254265,

            // report_cdrw_2x.B6T
            308224,

            // test_karaoke_multi_sampler.B6T
            329158
        };

        readonly MediaType[] _mediaTypes =
        {
            // dvdrom.B6T
            MediaType.DVDROM,

            // jaguarcd.B6T
            MediaType.CDDA,

            // pcengine.B6T
            MediaType.CD,

            // pcfx.B6T
            MediaType.CD,

            // report_cdr.B6T
            MediaType.CDR,

            // report_cdrom.B6T
            MediaType.CDROM,

            // report_cdrw_2x.B6T
            MediaType.CDRW,

            // test_karaoke_multi_sampler.B6T
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // dvdrom.B6T
            "UNKNOWN",

            // jaguarcd.B6T
            "3dd5bd0f7d95a40d411761d69255567a",

            // pcengine.B6T
            "4f5165069b3c5f11afe5f59711bd945d",

            // pcfx.B6T
            "c1bc8de499756453d1387542bb32bb4d",

            // report_cdr.B6T
            "63c99a087570b8936bb55156f5502f38",

            // report_cdrom.B6T
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw_2x.B6T
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // test_karaoke_multi_sampler.B6T
            "a34e29e42b60023a6ae59f37d2bd4bea"
        };

        readonly string[] _longMd5S =
        {
            // dvdrom.B6T
            "UNKNOWN",

            // jaguarcd.B6T
            "3dd5bd0f7d95a40d411761d69255567a",

            // pcengine.B6T
            "fd30db9486f67654179c90c8a5052edb",

            // pcfx.B6T
            "455ec326506d2c5b974c4617c1010796",

            // report_cdr.B6T
            "368c06d4b42ed581f3ad7f6ad57f70f6",

            // report_cdrom.B6T
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw_2x.B6T
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // test_karaoke_multi_sampler.B6T
            "e981f7dfdb522ba937fe75474e23a446"
        };

        readonly string[] _subchannelMd5S =
        {
            // dvdrom.B6T
            null,

            // jaguarcd.B6T
            null,

            // pcengine.B6T
            null,

            // pcfx.B6T
            null,

            // report_cdr.B6T
            null,

            // report_cdrom.B6T
            null,

            // report_cdrw_2x.B6T
            null,

            // test_karaoke_multi_sampler.B6T
            null
        };

        readonly int[] _tracks =
        {
            // dvdrom.B6T
            1,

            // jaguarcd.B6T
            11,

            // pcengine.B6T
            16,

            // pcfx.B6T
            8,

            // report_cdr.B6T
            1,

            // report_cdrom.B6T
            1,

            // report_cdrw_2x.B6T
            1,

            // test_karaoke_multi_sampler.B6T
            16
        };

        readonly int[][] _trackSessions =
        {
            // dvdrom.B6T
            new[]
            {
                1
            },

            // jaguarcd.B6T
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // pcengine.B6T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.B6T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

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

            // report_cdrw_2x.B6T
            new[]
            {
                1
            },

            // test_karaoke_multi_sampler.B6T
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // dvdrom.B6T
            new ulong[]
            {
                0
            },

            // jaguarcd.B6T
            new ulong[]
            {
                0, 27490, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.B6T
            new ulong[]
            {
                0, 3590, 38464, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // pcfx.B6T
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220645, 225646, 235498
            },

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

            // report_cdrw_2x.B6T
            new ulong[]
            {
                0
            },

            // test_karaoke_multi_sampler.B6T
            new ulong[]
            {
                0, 1737, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // dvdrom.B6T
            new ulong[]
            {
                0
            },

            // jaguarcd.B6T
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.B6T
            new ulong[]
            {
                3589, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },

            // pcfx.B6T
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },

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

            // report_cdrw_2x.B6T
            new ulong[]
            {
                308223
            },

            // test_karaoke_multi_sampler.B6T
            new ulong[]
            {
                1736, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // dvdrom.B6T
            new ulong[]
            {
                0
            },

            // jaguarcd.B6T
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.B6T
            new ulong[]
            {
                150, 0, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.B6T
            new ulong[]
            {
                150, 0, 0, 0, 0, 150, 0, 0
            },

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

            // report_cdrw_2x.B6T
            new ulong[]
            {
                150
            },

            // test_karaoke_multi_sampler.B6T
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        readonly byte[][] _trackFlags =
        {
            // dvdrom.B6T
            null,

            // jaguarcd.B6T
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.B6T
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.B6T
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

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

            // report_cdrw_2x.B6T
            new byte[]
            {
                4
            },

            // test_karaoke_multi_sampler.B6T
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 6");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new DiscImages.BlindWrite5();
                bool opened = image.Open(filter);

                Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
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

        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.BlindWrite5();
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

                                if(sectors - doneSectors >= SECTORS_TO_READ)
                                {
                                    sector =
                                        @long ? image.ReadSectorsLong(doneSectors, SECTORS_TO_READ,
                                                                      currentTrack.TrackSequence)
                                            : image.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                currentTrack.TrackSequence);

                                    doneSectors += SECTORS_TO_READ;
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

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = image.ReadSectorsTag(doneSectors, SECTORS_TO_READ, currentTrack.TrackSequence,
                                                              SectorTagType.CdSectorSubchannel);

                                doneSectors += SECTORS_TO_READ;
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