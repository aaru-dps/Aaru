// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CloneCD.cs
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

namespace Aaru.Tests.Images.UltraISO
{
    [TestFixture]
    public class CloneCD
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.ccd", "report_audiocd.ccd", "report_cdrom.ccd", "report_cdrw.ccd",
            "report_enhancedcd.ccd", "test_multi_karaoke_sampler.ccd"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.ccd
            210150,

            // report_audiocd.ccd
            247073,

            // report_cdrom.ccd
            254265,

            // report_cdrw.ccd
            308224,

            // report_enhancedcd.ccd
            291916,

            // test_multi_karaoke_sampler.ccd
            329158
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.ccd
            MediaType.CDDA,

            // report_audiocd.ccd
            MediaType.CDDA,

            // report_cdrom.ccd
            MediaType.CDROM,

            // report_cdrw.ccd
            MediaType.CDROM,

            // report_enhancedcd.ccd
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.ccd
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.ccd
            "f6bd226d3f249fa821460aeb1393cf3b",

            // report_audiocd.ccd
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdrom.ccd
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.ccd
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // report_enhancedcd.ccd
            "588d8ff1fef693bbe5719ac6c2f96bc1",

            // test_multi_karaoke_sampler.ccd
            "8d8493eb8eba6c67be7a8f47d4fde971"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.ccd
            "f6bd226d3f249fa821460aeb1393cf3b",

            // report_audiocd.ccd
            "c09f408a4416634d8ac1c1ffd0ed75a5",

            // report_cdrom.ccd
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.ccd
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // report_enhancedcd.ccd
            "d72e737f49482d1330e8fe03b9f40b79",

            // test_multi_karaoke_sampler.ccd
            "5a9eb4f35ecc39de5c011a2bac8549b5"
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.ccd
            "864c1fc074773d109fe556f93b70be24",

            // report_audiocd.ccd
            "e6b61ad780c72d162c3ceb784de1fbd2",

            // report_cdrom.ccd
            "292b671b4b296a20511516557dbbd2b1",

            // report_cdrw.ccd
            "4b054ac37c290a91a47997c84c9978d6",

            // report_enhancedcd.ccd
            "266d259c5ac40b253f28ccfc452d0046",

            // test_multi_karaoke_sampler.ccd
            "159b910e0ec1a88e004b9bcebdbde747"
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.ccd
            22,

            // report_audiocd.ccd
            14,

            // report_cdrom.ccd
            1,

            // report_cdrw.ccd
            1,

            // report_enhancedcd.ccd
            14,

            // test_multi_karaoke_sampler.ccd
            16
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.ccd
            new[]
            {
                1
            },

            // report_cdrw.ccd
            new[]
            {
                1
            },

            // report_enhancedcd.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_multi_karaoke_sampler.ccd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.ccd
            new ulong[]
            {
                69150, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // report_audiocd.ccd
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdrom.ccd
            new ulong[]
            {
                0
            },

            // report_cdrw.ccd
            new ulong[]
            {
                0
            },

            // report_enhancedcd.ccd
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234180
            },

            // test_multi_karaoke_sampler.ccd
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.ccd
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279299
            },

            // report_audiocd.ccd
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdrom.ccd
            new ulong[]
            {
                254264
            },

            // report_cdrw.ccd
            new ulong[]
            {
                308223
            },

            // report_enhancedcd.ccd
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // test_multi_karaoke_sampler.ccd
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.ccd
            new ulong[]
            {
                150
            },

            // report_cdrw.ccd
            new ulong[]
            {
                150
            },

            // report_enhancedcd.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_multi_karaoke_sampler.ccd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.ccd
            new byte[]
            {
                4
            },

            // report_audiocd.ccd
            new byte[]
            {
                4
            },

            // report_cdrom.ccd
            new byte[]
            {
                4
            },

            // report_cdrw.ccd
            new byte[]
            {
                4
            },

            // report_enhancedcd.ccd
            new byte[]
            {
                4
            },

            // test_multi_karaoke_sampler.ccd
            new byte[]
            {
                4
            }
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "UltraISO", "CloneCD");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new CloneCd();
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

                    var  image  = new CloneCd();
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