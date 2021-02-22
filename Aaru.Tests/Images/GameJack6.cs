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
    public class GameJack6
    {
        readonly string[] _testFiles =
        {
            "report_cdrom_cooked_nodpm.xmd", "report_cdrom_cooked.xmd", "report_cdrom_nodpm.xmd", "report_cdrom.xmd",
            "report_cdrw.xmd", "report_cdr.xmd", "report_dvdram_v1.xmd", "report_dvdram_v2.xmd", "report_dvd+r-dl.xmd",
            "report_dvdrom.xmd", "report_dvd+rw.xmd", "report_dvd-rw.xmd", "report_dvd+r.xmd", "report_dvd-r.xmd"
        };

        readonly ulong[] _sectors =
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

        readonly MediaType[] _mediaTypes =
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

        readonly string[] _md5S =
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

        readonly string[] _longMd5S =
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

        readonly string[] _subchannelMd5S =
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

        readonly int[] _tracks =
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

        readonly int[][] _trackSessions =
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

        readonly ulong[][] _trackStarts =
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

        readonly ulong[][] _trackEnds =
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

        readonly ulong[][] _trackPregaps =
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

        readonly byte[][] _trackFlags =
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

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "GameJack 6");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new DiscImages.Alcohol120();
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

        [Test]
        public void Hashes()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "GameJack 6");

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.Alcohol120();
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