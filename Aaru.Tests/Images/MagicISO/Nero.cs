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
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images.MagicISO
{
    [TestFixture]
    public class Nero
    {
        readonly string[] _testFiles =
        {
            "cdiready_the_apprentice.nrg", "report_audiocd.nrg", "report_cdrom.nrg", "report_cdrw.nrg",
            "report_dvdram_v1.nrg", "report_dvdram_v2.nrg", "report_dvd+r-dl.nrg", "report_dvd-rom.nrg",
            "report_dvd+rw.nrg", "report_enhancedcd.nrg", "test_multi_karaoke_sampler.nrg", "report_dvd-r.nrg",
            "report_dvd-rw.nrg"
        };

        readonly ulong[] _sectors =
        {
            // cdiready_the_apprentice.nrg
            261150,

            // report_audiocd.nrg
            247223,

            // report_cdrom.nrg
            254265,

            // report_cdrw.nrg
            308224,

            // report_dvdram_v1.nrg
            1218960,

            // report_dvdram_v2.nrg
            2236704,

            // report_dvd+r-dl.nrg
            3455936,

            // report_dvd-rom.nrg
            2146368,

            // report_dvd+rw.nrg
            2295104,

            // report_enhancedcd.nrg
            314864,

            // test_multi_karaoke_sampler.nrg
            329307,

            // report_dvd-r.nrg
            2146368,

            // report_dvd-rw.nrg
            2146368
        };

        readonly MediaType[] _mediaTypes =
        {
            // cdiready_the_apprentice.nrg
            MediaType.CDDA,

            // report_audiocd.nrg
            MediaType.CDDA,

            // report_cdrom.nrg
            MediaType.CDROM,

            // report_cdrw.nrg
            MediaType.CDROM,

            // report_dvdram_v1.nrg
            MediaType.CDROM,

            // report_dvdram_v2.nrg
            MediaType.CDROM,

            // report_dvd+r-dl.nrg
            MediaType.CDROM,

            // report_dvd-rom.nrg
            MediaType.CDROM,

            // report_dvd+rw.nrg
            MediaType.CDROM,

            // report_enhancedcd.nrg
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.nrg
            MediaType.CDROMXA,

            // This is a fail from MagicISO
            // report_dvd-r.nrg
            MediaType.CDROM,

            // report_dvd-rw.nrg
            MediaType.CDROM
        };

        readonly string[] _md5S =
        {
            // cdiready_the_apprentice.nrg
            "ab350df419f96d967f51d0161ebeba63",

            // report_audiocd.nrg
            "277e98295297f618cc63687e98288d7e",

            // report_cdrom.nrg
            "2de6dd5eaa71c1a97625bab68382da60",

            // report_cdrw.nrg
            "f1510c82ea4ff535415833242adddac6",

            // report_dvdram_v1.nrg
            "192aea84e64cb396cc0f637a611788bf",

            // report_dvdram_v2.nrg
            "fa5cb9657d9ed429a41913027d7b27eb",

            // report_dvd+r-dl.nrg
            "cf5ba4a055c6bdb4c9287c52b01c4ffb",

            // report_dvd-rom.nrg
            "8ed49c810da17e7957962df4b07ca9a6",

            // report_dvd+rw.nrg
            "d7a519529ca4a4ad04a6e14858f92a33",

            // report_enhancedcd.nrg
            "0ac3eaefdd2c138e86229d195d63cba2",

            // test_multi_karaoke_sampler.nrg
            "cc6354d06b009b0446012842c7f94be7",

            // report_dvd-r.nrg
            "UNKNOWN",

            // report_dvd-rw.nrg
            "UNKNOWN"
        };

        readonly string[] _longMd5S =
        {
            // cdiready_the_apprentice.nrg
            "ab350df419f96d967f51d0161ebeba63",

            // report_audiocd.nrg
            "277e98295297f618cc63687e98288d7e",

            // report_cdrom.nrg
            "222edd2c920b63aefe2087ed6278abe6",

            // report_cdrw.nrg
            "22bd168e59e075229821448b60d1820b",

            // report_dvdram_v1.nrg
            "192aea84e64cb396cc0f637a611788bf",

            // report_dvdram_v2.nrg
            "fa5cb9657d9ed429a41913027d7b27eb",

            // report_dvd+r-dl.nrg
            "cf5ba4a055c6bdb4c9287c52b01c4ffb",

            // report_dvd-rom.nrg
            "8ed49c810da17e7957962df4b07ca9a6",

            // report_dvd+rw.nrg
            "d7a519529ca4a4ad04a6e14858f92a33",

            // report_enhancedcd.nrg
            "2524762a816af8e8c188b971dfd27374",

            // test_multi_karaoke_sampler.nrg
            "bb3ebf139ebb76fff1b229a379d289e4",

            // report_dvd-r.nrg
            "UNKNOWN",

            // report_dvd-rw.nrg
            "UNKNOWN"
        };

        readonly string[] _subchannelMd5S =
        {
            // cdiready_the_apprentice.nrg
            null,

            // report_audiocd.nrg
            null,

            // report_cdrom.nrg
            null,

            // report_cdrw.nrg
            null,

            // report_dvdram_v1.nrg
            null,

            // report_dvdram_v2.nrg
            null,

            // report_dvd+r-dl.nrg
            null,

            // report_dvd-rom.nrg
            null,

            // report_dvd+rw.nrg
            null,

            // report_enhancedcd.nrg
            null,

            // test_multi_karaoke_sampler.nrg
            null,

            // report_dvd-r.nrg
            null,

            // report_dvd-rw.nrg
            null
        };

        readonly int[] _tracks =
        {
            // cdiready_the_apprentice.nrg
            22,

            // report_audiocd.nrg
            14,

            // report_cdrom.nrg
            1,

            // report_cdrw.nrg
            1,

            // report_dvdram_v1.nrg
            1,

            // report_dvdram_v2.nrg
            1,

            // report_dvd+r-dl.nrg
            1,

            // report_dvd-rom.nrg
            1,

            // report_dvd+rw.nrg
            1,

            // report_enhancedcd.nrg
            14,

            // test_multi_karaoke_sampler.nrg
            16,

            // report_dvd-r.nrg
            1,

            // report_dvd-rw.nrg
            1
        };

        readonly int[][] _trackSessions =
        {
            // cdiready_the_apprentice.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdrom.nrg
            new[]
            {
                1
            },

            // report_cdrw.nrg
            new[]
            {
                1
            },

            // report_dvdram_v1.nrg
            new[]
            {
                1
            },

            // report_dvdram_v2.nrg
            new[]
            {
                1
            },

            // report_dvd+r-dl.nrg
            new[]
            {
                1
            },

            // report_dvd-rom.nrg
            new[]
            {
                1
            },

            // report_dvd+rw.nrg
            new[]
            {
                1
            },

            // report_enhancedcd.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // test_multi_karaoke_sampler.nrg
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_dvd-r.nrg
            new[]
            {
                1
            },

            // report_dvd-rw.nrg
            new[]
            {
                1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                69000, 88800, 107625, 112200, 133650, 138225, 159825, 164775, 185400, 190125, 208875, 213000, 232200,
                236700, 241875, 256125, 256875, 265650, 267375, 270000, 271650, 274275
            },

            // report_audiocd.nrg
            new ulong[]
            {
                0, 16399, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdrom.nrg
            new ulong[]
            {
                0
            },

            // report_cdrw.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v1.nrg
            new ulong[]
            {
                0
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                0
            },

            // report_dvd+r-dl.nrg
            new ulong[]
            {
                0
            },

            // report_dvd-rom.nrg
            new ulong[]
            {
                0
            },

            // report_dvd+rw.nrg
            new ulong[]
            {
                0
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                0, 15511, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234030
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                0, 1737, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // report_dvd-r.nrg
            new ulong[]
            {
                0
            },

            // report_dvd-rw.nrg
            new ulong[]
            {
                0
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                88799, 107624, 112199, 133649, 138224, 159824, 164774, 185399, 190124, 208874, 212999, 232199, 236699,
                241874, 256124, 256874, 265649, 267374, 269999, 271649, 274274, 279298
            },

            // report_audiocd.nrg
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247071
            },

            // report_cdrom.nrg
            new ulong[]
            {
                254263
            },

            // report_cdrw.nrg
            new ulong[]
            {
                308222
            },

            // report_dvdram_v1.nrg
            new ulong[]
            {
                1218958
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                2236702
            },

            // report_dvd+r-dl.nrg
            new ulong[]
            {
                3455934
            },

            // report_dvd-rom.nrg
            new ulong[]
            {
                2146366
            },

            // report_dvd+rw.nrg
            new ulong[]
            {
                2295102
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 234179,
                303314
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329156
            },

            // report_dvd-r.nrg
            new ulong[]
            {
                2146367
            },

            // report_dvd-rw.nrg
            new ulong[]
            {
                2146367
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // cdiready_the_apprentice.nrg
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.nrg
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.nrg
            new ulong[]
            {
                150
            },

            // report_cdrw.nrg
            new ulong[]
            {
                150
            },

            // report_dvdram_v1.nrg
            new ulong[]
            {
                150
            },

            // report_dvdram_v2.nrg
            new ulong[]
            {
                150
            },

            // report_dvd+r-dl.nrg
            new ulong[]
            {
                150
            },

            // report_dvd-rom.nrg
            new ulong[]
            {
                150
            },

            // report_dvd+rw.nrg
            new ulong[]
            {
                150
            },

            // report_enhancedcd.nrg
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_multi_karaoke_sampler.nrg
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_dvd-r.nrg
            new ulong[]
            {
                0
            },

            // report_dvd-rw.nrg
            new ulong[]
            {
                0
            }
        };

        readonly byte[][] _trackFlags =
        {
            // cdiready_the_apprentice.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdrom.nrg
            new byte[]
            {
                4
            },

            // report_cdrw.nrg
            new byte[]
            {
                4
            },

            // report_dvdram_v1.nrg
            new byte[]
            {
                4
            },

            // report_dvdram_v2.nrg
            new byte[]
            {
                4
            },

            // report_dvd+r-dl.nrg
            new byte[]
            {
                4
            },

            // report_dvd-rom.nrg
            new byte[]
            {
                4
            },

            // report_dvd+rw.nrg
            new byte[]
            {
                4
            },

            // report_enhancedcd.nrg
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // test_multi_karaoke_sampler.nrg
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_dvd-r.nrg
            null,

            // report_dvd-rw.nrg
            null
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "MagicISO", "Nero");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new DiscImages.Nero();
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

                    var  image  = new DiscImages.Nero();
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