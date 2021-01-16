// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cuesheet.cs
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
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.DiscImages;
using Aaru.Filters;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Images.MagicISO
{
    [TestFixture]
    public class Cuesheet
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.cue", "report_audiocd.cue", "report_cdrom.cue", "report_cdrw.cue",
            "report_dvdram_v1.cue", "report_dvdram_v2.cue", "report_dvd+r-dl.cue", "report_dvd-rom.cue",
            "report_dvd+rw.cue", "report_enhancedcd.cue", "test_multi_karaoke_sampler.cue"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.cue
            210299,

            // report_audiocd.cue
            247222,

            // report_cdrom.cue
            254264,

            // report_cdrw.cue
            308223,

            // report_dvdram_v1.cue
            1218959,

            // report_dvdram_v2.cue
            2236703,

            // report_dvd+r-dl.cue
            3455935,

            // report_dvd-rom.cue
            2146367,

            // report_dvd+rw.cue
            2295103,

            // report_enhancedcd.cue
            303615,

            // test_multi_karaoke_sampler.cue
            329307
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.cue
            MediaType.CDDA,

            // report_audiocd.cue
            MediaType.CDDA,

            // report_cdrom.cue
            MediaType.CDROM,

            // report_cdrw.cue
            MediaType.CDROM,

            // report_dvdram_v1.cue
            MediaType.CDROM,

            // report_dvdram_v2.cue
            MediaType.CDROM,

            // report_dvd+r-dl.cue
            MediaType.CDROM,

            // report_dvd-rom.cue
            MediaType.CDROM,

            // report_dvd+rw.cue
            MediaType.CDROM,

            // report_enhancedcd.cue
            MediaType.CDPLUS,

            // test_multi_karaoke_sampler.cue
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.cue
            "ab350df419f96d967f51d0161ebeba63",

            // report_audiocd.cue
            "277e98295297f618cc63687e98288d7e",

            // report_cdrom.cue
            "2de6dd5eaa71c1a97625bab68382da60",

            // report_cdrw.cue
            "f1510c82ea4ff535415833242adddac6",

            // report_dvdram_v1.cue
            "192aea84e64cb396cc0f637a611788bf",

            // report_dvdram_v2.cue
            "fa5cb9657d9ed429a41913027d7b27eb",

            // report_dvd+r-dl.cue
            "cf5ba4a055c6bdb4c9287c52b01c4ffb",

            // report_dvd-rom.cue
            "8ed49c810da17e7957962df4b07ca9a6",

            // report_dvd+rw.cue
            "d7a519529ca4a4ad04a6e14858f92a33",

            // report_enhancedcd.cue
            "0ac3eaefdd2c138e86229d195d63cba2",

            // test_multi_karaoke_sampler.cue
            "cc6354d06b009b0446012842c7f94be7"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.cue
            "ab350df419f96d967f51d0161ebeba63",

            // report_audiocd.cue
            "277e98295297f618cc63687e98288d7e",

            // report_cdrom.cue
            "222edd2c920b63aefe2087ed6278abe6",

            // report_cdrw.cue
            "22bd168e59e075229821448b60d1820b",

            // report_dvdram_v1.cue
            "192aea84e64cb396cc0f637a611788bf",

            // report_dvdram_v2.cue
            "fa5cb9657d9ed429a41913027d7b27eb",

            // report_dvd+r-dl.cue
            "cf5ba4a055c6bdb4c9287c52b01c4ffb",

            // report_dvd-rom.cue
            "8ed49c810da17e7957962df4b07ca9a6",

            // report_dvd+rw.cue
            "d7a519529ca4a4ad04a6e14858f92a33",

            // report_enhancedcd.cue
            "2524762a816af8e8c188b971dfd27374",

            // test_multi_karaoke_sampler.cue
            "bb3ebf139ebb76fff1b229a379d289e4"
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.cue
            null,

            // report_audiocd.cue
            null,

            // report_cdrom.cue
            null,

            // report_cdrw.cue
            null,

            // report_dvdram_v1.cue
            null,

            // report_dvdram_v2.cue
            null,

            // report_dvd+r-dl.cue
            null,

            // report_dvd-rom.cue
            null,

            // report_dvd+rw.cue
            null,

            // report_enhancedcd.cue
            null,

            // test_multi_karaoke_sampler.cue
            null
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.cue
            22,

            // report_audiocd.cue
            14,

            // report_cdrom.cue
            1,

            // report_cdrw.cue
            1,

            // report_dvdram_v1.cue
            1,

            // report_dvdram_v2.cue
            1,

            // report_dvd+r-dl.cue
            1,

            // report_dvd-rom.cue
            1,

            // report_dvd+rw.cue
            1,

            // report_enhancedcd.cue
            14,

            // test_multi_karaoke_sampler.cue
            16
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.cue
            new[]
            {
                1
            },

            // report_cdrw.cue
            new[]
            {
                1
            },

            // report_dvdram_v1.cue
            new[]
            {
                1
            },

            // report_dvdram_v2.cue
            new[]
            {
                1
            },

            // report_dvd+r-dl.cue
            new[]
            {
                1
            },

            // report_dvd-rom.cue
            new[]
            {
                1
            },

            // report_dvd+rw.cue
            new[]
            {
                1
            },

            // report_enhancedcd.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.cue
            new[]
            {
                69000UL, 88800UL, 107625UL, 112200UL, 133650UL, 138225UL, 159825UL, 164775UL, 185400UL, 190125UL,
                208875UL, 213000UL, 232200UL, 236700UL, 241875UL, 256125UL, 256875UL, 265650UL, 267375UL, 270000UL,
                271650UL, 274275UL
            },

            // report_audiocd.cue
            new[]
            {
                0UL, 16399UL, 30051UL, 47950UL, 63314UL, 78925UL, 94732UL, 117125UL, 136166UL, 154072UL, 170751UL,
                186539UL, 201799UL, 224449UL
            },

            // report_cdrom.cue
            new ulong[]
            {
                0
            },

            // report_cdrw.cue
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.cue
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.cue
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.cue
            new ulong[]
            {
                0
            },

            // report_dvd-rom.cue
            new ulong[]
            {
                0
            },

            // report_dvd+rw.cue
            new ulong[]
            {
                0
            },

            // report_enhancedcd.cue
            new[]
            {
                0UL, 15511UL, 33959UL, 51330UL, 71973UL, 87582UL, 103305UL, 117691UL, 136167UL, 153418UL, 166932UL,
                187113UL, 201441UL, 234030UL
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                0UL, 1737UL, 32749UL, 52672UL, 70304UL, 100098UL, 119761UL, 136999UL, 155790UL, 175826UL, 206461UL,
                226450UL, 244355UL, 273965UL, 293752UL, 310711UL
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.cue
            new[]
            {
                88799UL, 107624UL, 112199UL, 133649UL, 138224UL, 159824UL, 164774UL, 185399UL, 190124UL, 208874UL,
                212999UL, 232199UL, 236699UL, 241874UL, 256124UL, 256874UL, 265649UL, 267374UL, 269999UL, 271649UL,
                274274UL, 279298UL
            },

            // report_audiocd.cue
            new[]
            {
                16548UL, 30050UL, 47949UL, 63313UL, 78924UL, 94731UL, 117124UL, 136165UL, 154071UL, 170750UL, 186538UL,
                201798UL, 224448UL, 247071UL
            },

            // report_cdrom.cue
            new ulong[]
            {
                254263
            },

            // report_cdrw.cue
            new ulong[]
            {
                308222
            },

            // report_dvdram_v1.cue
            new[]
            {
                1218958UL
            },

            // report_dvdram_v2.cue
            new[]
            {
                2236702UL
            },

            // report_dvd+r-dl.cue
            new[]
            {
                3455934UL
            },

            // report_dvd-rom.cue
            new[]
            {
                2146366UL
            },

            // report_dvd+rw.cue
            new ulong[]
            {
                2295102
            },

            // report_enhancedcd.cue
            new[]
            {
                15660UL, 33958UL, 51329UL, 71972UL, 87581UL, 103304UL, 117690UL, 136166UL, 153417UL, 166931UL, 187112UL,
                201440UL, 234179UL, 303314UL
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                1886UL, 32748UL, 52671UL, 70303UL, 100097UL, 119760UL, 136998UL, 155789UL, 175825UL, 206460UL, 226449UL,
                244354UL, 273964UL, 293751UL, 310710UL, 329156UL
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.cue
            new[]
            {
                150UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL,
                0UL, 0UL
            },

            // report_audiocd.cue
            new[]
            {
                150UL, 150UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL
            },

            // report_cdrom.cue
            new ulong[]
            {
                150
            },

            // report_cdrw.cue
            new ulong[]
            {
                150
            },

            // report_dvdram_v1.cue
            new ulong[]
            {
                150
            },

            // report_dvdram_v2.cue
            new ulong[]
            {
                150
            },

            // report_dvd+r-dl.cue
            new ulong[]
            {
                150
            },

            // report_dvd-rom.cue
            new ulong[]
            {
                150
            },

            // report_dvd+rw.cue
            new ulong[]
            {
                150
            },

            // report_enhancedcd.cue
            new[]
            {
                150UL, 150UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 150UL
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                150UL, 150UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL
            }
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.cue
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.cue
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.cue
            new byte[]
            {
                4
            },

            // report_cdrw.cue
            new byte[]
            {
                4
            },

            // report_dvdram_v1.cue
            new byte[]
            {
                4
            },

            // report_dvdram_v2.cue
            new byte[]
            {
                4
            },

            // report_dvd+r-dl.cue
            new byte[]
            {
                4
            },

            // report_dvd-rom.cue
            new byte[]
            {
                4
            },

            // report_dvd+rw.cue
            new byte[]
            {
                4
            },

            // report_enhancedcd.cue
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // test_multi_karaoke_sampler.cue
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        [Test]
        public void Test()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory =
                Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "MagicISO", "Cuesheet");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new CdrWin();
                Assert.AreEqual(true, images[i].Open(filters[i]), $"Open: {_testFiles[i]}");
            }

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_sectors[i], images[i].Info.Sectors, $"Sectors: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_mediaTypes[i], images[i].Info.MediaType, $"Media type: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_tracks[i], images[i].Tracks.Count, $"Tracks: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackSession).Should().
                          BeEquivalentTo(_trackSessions[i], $"Track session: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackStartSector).Should().
                          BeEquivalentTo(_trackStarts[i], $"Track start: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackEndSector).Should().
                          BeEquivalentTo(_trackEnds[i], $"Track end: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                images[i].Tracks.Select(t => t.TrackPregap).Should().
                          BeEquivalentTo(_trackPregaps[i], $"Track pregap: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                int trackNo = 0;

                foreach(Track currentTrack in images[i].Tracks)
                {
                    if(images[i].Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                        Assert.AreEqual(_trackFlags[i][trackNo],
                                        images[i].ReadSectorTag(currentTrack.TrackSequence, SectorTagType.CdTrackFlags)
                                            [0], $"Track flags: {_testFiles[i]}, track {currentTrack.TrackSequence}");

                    trackNo++;
                }
            }

            foreach(bool @long in new[]
            {
                false, true
            })
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var ctx = new Md5Context();

                    foreach(Track currentTrack in images[i].Tracks)
                    {
                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= sectorsToRead)
                            {
                                sector = @long ? images[i].
                                                 ReadSectorsLong(doneSectors, sectorsToRead, currentTrack.TrackSequence)
                                             : images[i].
                                                 ReadSectors(doneSectors, sectorsToRead, currentTrack.TrackSequence);

                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = @long ? images[i].ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                           currentTrack.TrackSequence)
                                             : images[i].ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                     currentTrack.TrackSequence);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(),
                                    $"{(@long ? "Long hash" : "Hash")}: {_testFiles[i]}");
                }

            for(int i = 0; i < _testFiles.Length; i++)
                if(images[i].Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                {
                    var ctx = new Md5Context();

                    foreach(Track currentTrack in images[i].Tracks)
                    {
                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= sectorsToRead)
                            {
                                sector = images[i].ReadSectorsTag(doneSectors, sectorsToRead,
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = images[i].ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(_subchannelMd5S[i], ctx.End(), $"Subchannel hash: {_testFiles[i]}");
                }
        }
    }
}