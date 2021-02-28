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
    public class CDRWin10
    {
        readonly string[] _testFiles =
        {
            "report_audiocd.xmd", "report_cdrom.xmd", "report_cdrw_2x.xmd", "report_cdr.xmd", "report_enhancedcd.xmd",
            "test_karaoke_multi_sampler.xmd"
        };

        readonly ulong[] _sectors =
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

        readonly MediaType[] _mediaTypes =
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

        readonly string[] _md5S =
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

        readonly string[] _longMd5S =
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

        readonly string[] _subchannelMd5S =
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

        readonly int[] _tracks =
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

        readonly int[][] _trackSessions =
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

        readonly ulong[][] _trackStarts =
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

        readonly ulong[][] _trackEnds =
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

        readonly ulong[][] _trackPregaps =
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

        readonly byte[][] _trackFlags =
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

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CDRWin 10");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

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