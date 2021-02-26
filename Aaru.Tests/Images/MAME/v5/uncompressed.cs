// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : uncompressed.cs
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
using System.Threading.Tasks;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.DiscImages;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images.MAME.v5
{
    [TestFixture]
    public class Uncompressed
    {
        readonly string[] _testFiles =
        {
            "gigarec.chd", "hdd.chd", "jaguarcd.chd", "pcengine.chd", "pcfx.chd", "report_audiocd.chd",
            "report_cdr.chd", "report_cdrom.chd", "report_cdrw.chd", "test_audiocd_cdtext.chd", "test_enhancedcd.chd",
            "test_incd_udf200_finalized.chd", "test_multi_karaoke_sampler.chd", "test_multiple_indexes.chd",
            "test_multisession.chd", "test_multisession_dvd+r.chd", "test_multisession_dvd-r.chd", "test_videocd.chd"
        };

        readonly ulong[] _sectors =
        {
            // gigarec.chd
            469652,

            // hdd.chd
            251904,

            // jaguarcd.chd
            243587,

            // pcengine.chd
            160506,

            // pcfx.chd
            246380,

            // report_audiocd.chd
            247073,

            // report_cdr.chd
            254265,

            // report_cdrom.chd
            254265,

            // report_cdrw.chd
            308224,

            // test_audiocd_cdtext.chd
            277696,

            // test_enhancedcd.chd
            59206,

            // test_incd_udf200_finalized.chd
            350134,

            // test_multi_karaoke_sampler.chd
            329158,

            // test_multiple_indexes.chd
            65536,

            // test_multisession.chd
            51168,

            // test_multisession_dvd+r.chd
            230624,

            // test_multisession_dvd-r.chd
            257264,

            // test_videocd.chd
            48794
        };

        readonly uint[] _sectorSize =
        {
            // gigarec.chd
            2048,

            // hdd.chd
            512,

            // jaguarcd.chd
            2352,

            // pcengine.chd
            2352,

            // pcfx.chd
            2352,

            // report_audiocd.chd
            2352,

            // report_cdr.chd
            2048,

            // report_cdrom.chd
            2048,

            // report_cdrw.chd
            2048,

            // test_audiocd_cdtext.chd
            2352,

            // test_enhancedcd.chd
            2352,

            // test_incd_udf200_finalized.chd
            2336,

            // test_multi_karaoke_sampler.chd
            2352,

            // test_multiple_indexes.chd
            2352,

            // test_multisession.chd
            2336,

            // test_multisession_dvd+r.chd
            2048,

            // test_multisession_dvd-r.chd
            2048,

            // test_videocd.chd
            2336
        };

        readonly MediaType[] _mediaTypes =
        {
            // gigarec.chd
            MediaType.CDROM,

            // hdd.chd
            MediaType.GENERIC_HDD,

            // jaguarcd.chd
            MediaType.CDROM,

            // pcengine.chd
            MediaType.CDROM,

            // pcfx.chd
            MediaType.CDROM,

            // report_audiocd.chd
            MediaType.CDROM,

            // report_cdr.chd
            MediaType.CDROM,

            // report_cdrom.chd
            MediaType.CDROM,

            // report_cdrw.chd
            MediaType.CDROM,

            // test_audiocd_cdtext.chd
            MediaType.CDROM,

            // test_enhancedcd.chd
            MediaType.CDROM,

            // test_incd_udf200_finalized.chd
            MediaType.CDROM,

            // test_multi_karaoke_sampler.chd
            MediaType.CDROM,

            // test_multiple_indexes.chd
            MediaType.CDROM,

            // test_multisession.chd
            MediaType.CDROM,

            // test_multisession_dvd+r.chd
            MediaType.CDROM,

            // test_multisession_dvd-r.chd
            MediaType.CDROM,

            // test_videocd.chd
            MediaType.CDROM
        };

        readonly string[] _md5S =
        {
            // gigarec.chd
            "UNKNOWN",

            // hdd.chd
            "43476343f53a177dd57b68dd769917aa",

            // jaguarcd.chd
            "UNKNOWN",

            // pcengine.chd
            "UNKNOWN",

            // pcfx.chd
            "UNKNOWN",

            // report_audiocd.chd
            "UNKNOWN",

            // report_cdr.chd
            "UNKNOWN",

            // report_cdrom.chd
            "UNKNOWN",

            // report_cdrw.chd
            "UNKNOWN",

            // test_audiocd_cdtext.chd
            "UNKNOWN",

            // test_enhancedcd.chd
            "UNKNOWN",

            // test_incd_udf200_finalized.chd
            "UNKNOWN",

            // test_multi_karaoke_sampler.chd
            "UNKNOWN",

            // test_multiple_indexes.chd
            "UNKNOWN",

            // test_multisession.chd
            "UNKNOWN",

            // test_multisession_dvd+r.chd
            "UNKNOWN",

            // test_multisession_dvd-r.chd
            "UNKNOWN",

            // test_videocd.chd
            "UNKNOWN"
        };

        readonly string[] _longMd5S =
        {
            // gigarec.chd
            "UNKNOWN",

            // hdd.chd
            null,

            // jaguarcd.chd
            "UNKNOWN",

            // pcengine.chd
            "UNKNOWN",

            // pcfx.chd
            "UNKNOWN",

            // report_audiocd.chd
            "UNKNOWN",

            // report_cdr.chd
            "UNKNOWN",

            // report_cdrom.chd
            "UNKNOWN",

            // report_cdrw.chd
            "UNKNOWN",

            // test_audiocd_cdtext.chd
            "UNKNOWN",

            // test_enhancedcd.chd
            "UNKNOWN",

            // test_incd_udf200_finalized.chd
            "UNKNOWN",

            // test_multi_karaoke_sampler.chd
            "UNKNOWN",

            // test_multiple_indexes.chd
            "UNKNOWN",

            // test_multisession.chd
            "UNKNOWN",

            // test_multisession_dvd+r.chd
            "UNKNOWN",

            // test_multisession_dvd-r.chd
            "UNKNOWN",

            // test_videocd.chd
            "UNKNOWN"
        };

        readonly string[] _subchannelMd5S =
        {
            // gigarec.chd
            "UNKNOWN",

            // hdd.chd
            null,

            // jaguarcd.chd
            "UNKNOWN",

            // pcengine.chd
            "UNKNOWN",

            // pcfx.chd
            "UNKNOWN",

            // report_audiocd.chd
            "UNKNOWN",

            // report_cdr.chd
            "UNKNOWN",

            // report_cdrom.chd
            "UNKNOWN",

            // report_cdrw.chd
            "UNKNOWN",

            // test_audiocd_cdtext.chd
            "UNKNOWN",

            // test_enhancedcd.chd
            "UNKNOWN",

            // test_incd_udf200_finalized.chd
            "UNKNOWN",

            // test_multi_karaoke_sampler.chd
            "UNKNOWN",

            // test_multiple_indexes.chd
            "UNKNOWN",

            // test_multisession.chd
            "UNKNOWN",

            // test_multisession_dvd+r.chd
            "UNKNOWN",

            // test_multisession_dvd-r.chd
            "UNKNOWN",

            // test_videocd.chd
            "UNKNOWN"
        };

        readonly int[] _tracks =
        {
            // gigarec.chd
            1,

            // hdd.chd
            -1,

            // jaguarcd.chd
            11,

            // pcengine.chd
            16,

            // pcfx.chd
            8,

            // report_audiocd.chd
            14,

            // report_cdr.chd
            1,

            // report_cdrom.chd
            1,

            // report_cdrw.chd
            1,

            // test_audiocd_cdtext.chd
            11,

            // test_enhancedcd.chd
            3,

            // test_incd_udf200_finalized.chd
            1,

            // test_multi_karaoke_sampler.chd
            16,

            // test_multiple_indexes.chd
            5,

            // test_multisession.chd
            4,

            // test_multisession_dvd+r.chd
            2,

            // test_multisession_dvd-r.chd
            2,

            // test_videocd.chd
            2
        };

        readonly int[][] _trackSessions =
        {
            // gigarec.chd
            new[]
            {
                1
            },

            // hdd.chd
            null,

            // jaguarcd.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcengine.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_audiocd.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.chd
            new[]
            {
                1
            },

            // report_cdrom.chd
            new[]
            {
                1
            },

            // report_cdrw.chd
            new[]
            {
                1
            },

            // test_audiocd_cdtext.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_enhancedcd.chd
            new[]
            {
                1, 1, 1
            },

            // test_incd_udf200_finalized.chd
            new[]
            {
                1
            },

            // test_multi_karaoke_sampler.chd
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.chd
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.chd
            new[]
            {
                1, 1, 1, 1
            },

            // test_multisession_dvd+r.chd
            new[]
            {
                1, 1
            },

            // test_multisession_dvd-r.chd
            new[]
            {
                1, 1
            },

            // test_videocd.chd
            new[]
            {
                1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // gigarec.chd
            new ulong[]
            {
                0
            },

            // hdd.chd
            null,

            // jaguarcd.chd
            new ulong[]
            {
                0, 27640, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.chd
            new ulong[]
            {
                0, 3440, 38314, 46917, 53201, 61519, 68263, 75097, 82830, 86181, 90967, 98974, 106393, 111938, 119970,
                125779
            },

            // pcfx.chd
            new ulong[]
            {
                0, 4245, 4759, 5791, 41909, 220495, 225346, 235198
            },

            // report_audiocd.chd
            new ulong[]
            {
                0, 16549, 30051, 47950, 63314, 78925, 94732, 117125, 136166, 154072, 170751, 186539, 201799, 224449
            },

            // report_cdr.chd
            new ulong[]
            {
                0
            },

            // report_cdrom.chd
            new ulong[]
            {
                0
            },

            // report_cdrw.chd
            new ulong[]
            {
                0
            },

            // test_audiocd_cdtext.chd
            new ulong[]
            {
                0, 29902, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_enhancedcd.chd
            new ulong[]
            {
                0, 14405, 40353
            },

            // test_incd_udf200_finalized.chd
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.chd
            new ulong[]
            {
                0, 1887, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multiple_indexes.chd
            new ulong[]
            {
                0, 4804, 13875, 41185, 54989
            },

            // test_multisession.chd
            new ulong[]
            {
                0, 19533, 32860, 45378
            },

            // test_multisession_dvd+r.chd
            new ulong[]
            {
                0, 24064
            },

            // test_multisession_dvd-r.chd
            new ulong[]
            {
                0, 235248
            },

            // test_videocd.chd
            new ulong[]
            {
                0, 1252
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // gigarec.chd
            new ulong[]
            {
                469651
            },

            // hdd.chd
            null,

            // jaguarcd.chd
            new ulong[]
            {
                27639, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.chd
            new ulong[]
            {
                3439, 38313, 46916, 53200, 61518, 68262, 75096, 82829, 86180, 90966, 98973, 106392, 111937, 119969,
                125778, 160505
            },

            // pcfx.chd
            new ulong[]
            {
                4244, 4758, 5790, 41908, 220494, 225345, 235197, 246379
            },

            // report_audiocd.chd
            new ulong[]
            {
                16548, 30050, 47949, 63313, 78924, 94731, 117124, 136165, 154071, 170750, 186538, 201798, 224448, 247072
            },

            // report_cdr.chd
            new ulong[]
            {
                254264
            },

            // report_cdrom.chd
            new ulong[]
            {
                254264
            },

            // report_cdrw.chd
            new ulong[]
            {
                308223
            },

            // test_audiocd_cdtext.chd
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_enhancedcd.chd
            new ulong[]
            {
                14404, 40352, 59205
            },

            // test_incd_udf200_finalized.chd
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.chd
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multiple_indexes.chd
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.chd
            new ulong[]
            {
                19532, 32859, 45377, 51167
            },

            // test_multisession_dvd+r.chd
            new ulong[]
            {
                24063, 230623
            },

            // test_multisession_dvd-r.chd
            new ulong[]
            {
                235247, 257263
            },

            // test_videocd.chd
            new ulong[]
            {
                1251, 48793
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // gigarec.chd
            new ulong[]
            {
                150
            },

            // hdd.chd
            null,

            // jaguarcd.chd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.chd
            new ulong[]
            {
                150, 150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // pcfx.chd
            new ulong[]
            {
                150, 150, 0, 0, 0, 150, 0, 0
            },

            // report_audiocd.chd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.chd
            new ulong[]
            {
                150
            },

            // report_cdrom.chd
            new ulong[]
            {
                150
            },

            // report_cdrw.chd
            new ulong[]
            {
                150
            },

            // test_audiocd_cdtext.chd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_enhancedcd.chd
            new ulong[]
            {
                150, 150, 0
            },

            // test_incd_udf200_finalized.chd
            new ulong[]
            {
                150
            },

            // test_multi_karaoke_sampler.chd
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.chd
            new ulong[]
            {
                150, 0, 0, 0, 0
            },

            // test_multisession.chd
            new ulong[]
            {
                150, 0, 0, 0
            },

            // test_multisession_dvd+r.chd
            new ulong[]
            {
                150, 0
            },

            // test_multisession_dvd-r.chd
            new ulong[]
            {
                150, 0
            },

            // test_videocd.chd
            new ulong[]
            {
                150, 0
            }
        };

        readonly byte[][] _trackFlags =
        {
            // gigarec.chd
            new byte[]
            {
                0
            },

            // hdd.chd
            null,

            // jaguarcd.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_audiocd.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.chd
            new byte[]
            {
                0
            },

            // report_cdrom.chd
            new byte[]
            {
                0
            },

            // report_cdrw.chd
            new byte[]
            {
                0
            },

            // test_audiocd_cdtext.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_enhancedcd.chd
            new byte[]
            {
                0, 0, 0
            },

            // test_incd_udf200_finalized.chd
            new byte[]
            {
                0
            },

            // test_multi_karaoke_sampler.chd
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.chd
            new byte[]
            {
                0, 0, 0, 0, 0
            },

            // test_multisession.chd
            new byte[]
            {
                0, 0, 0, 0
            },

            // test_multisession_dvd+r.chd
            new byte[]
            {
                0, 0
            },

            // test_multisession_dvd-r.chd
            new byte[]
            {
                0, 0
            },

            // test_videocd.chd
            new byte[]
            {
                0, 0
            }
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "MAME", "v5", "uncompressed");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            using(new AssertionScope())
            {
                Assert.Multiple(() =>
                {
                    for(int i = 0; i < _testFiles.Length; i++)
                    {
                        var filter = new ZZZNoFilter();
                        filter.Open(_testFiles[i]);

                        var  image  = new Chd();
                        bool opened = image.Open(filter);

                        Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                        using(new AssertionScope())
                        {
                            Assert.Multiple(() =>
                            {
                                Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");

                                if(image.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                                    return;

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
                });
            }
        }

        // How many sectors to read at once
        const uint sectorsToRead = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                Parallel.For(0, _testFiles.Length, (i, state) =>
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new Chd();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    Md5Context ctx;

                    if(image.Info.XmlMediaType == XmlMediaType.OpticalDisc)
                    {
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
                                                : image.ReadSectors(doneSectors, sectorsToRead,
                                                                    currentTrack.TrackSequence);

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
                            return;

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
                                    sector = image.ReadSectorsTag(doneSectors, sectorsToRead,
                                                                  currentTrack.TrackSequence,
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
                    else
                    {
                        ctx = new Md5Context();
                        ulong doneSectors = 0;

                        while(doneSectors < image.Info.Sectors)
                        {
                            byte[] sector;

                            if(image.Info.Sectors - doneSectors >= sectorsToRead)
                            {
                                sector      =  image.ReadSectors(doneSectors, sectorsToRead);
                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                                doneSectors += image.Info.Sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }

                        Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                    }
                });
            });
        }
    }
}