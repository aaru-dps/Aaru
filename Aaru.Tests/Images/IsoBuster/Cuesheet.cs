// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IsoBusterCuesheet.cs
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

namespace Aaru.Tests.Images.IsoBuster
{
    [TestFixture]
    public class Cuesheet
    {
        readonly string[] _testFiles =
        {
            "gigarec.cue", "jaguarcd.cue", "pcengine.cue", "pcfx.cue", "report_cdr.cue", "report_cdrom.cue",
            "report_cdrw.cue", "test_audiocd_cdtext.cue", "test_enhancedcd.cue", "test_incd_udf200_finalized.cue",
            "test_multi_karaoke_sampler.cue", "test_multiple_indexes.cue", "test_multisession.cue",
            "test_multisession_dvd+r.cue", "test_multisession_dvd-r.cue", "test_videocd.cue"
        };

        readonly ulong[] _sectors =
        {
            // gigarec.cue
            469652,

            // jaguarcd.cue
            243587,

            // pcengine.cue
            160956,

            // pcfx.cue
            246680,

            // report_cdr.cue
            254265,

            // report_cdrom.cue
            254265,

            // report_cdrw.cue
            308224,

            // test_audiocd_cdtext.cue
            277696,

            // test_enhancedcd.cue
            59206,

            // test_incd_udf200_finalized.cue
            350134,

            // test_multi_karaoke_sampler.cue
            329158,

            // test_multiple_indexes.cue
            65536,

            // test_multisession.cue
            51168,

            // test_multisession_dvd+r.cue
            230624,

            // test_multisession_dvd-r.cue
            257264,

            // test_videocd.cue
            48794
        };

        readonly MediaType[] _mediaTypes =
        {
            // gigarec.cue
            // This is a mistake by IsoBuster
            MediaType.CDROM,

            // jaguarcd.cue
            MediaType.CDDA,

            // pcengine.cue
            MediaType.CD,

            // pcfx.cue
            MediaType.CD,

            // report_cdr.cue
            // This is a mistake by IsoBuster
            MediaType.CDROM,

            // report_cdrom.cue
            MediaType.CDROM,

            // report_cdrw.cue
            MediaType.CDRW,

            // test_audiocd_cdtext.cue
            // This is a mistake by IsoBuster
            MediaType.CDDA,

            // test_enhancedcd.cue
            // This is a mistake by IsoBuster
            MediaType.CDPLUS,

            // test_incd_udf200_finalized.cue
            // This is a mistake by IsoBuster
            MediaType.CDROMXA,

            // test_multi_karaoke_sampler.cue
            // This is a mistake by IsoBuster
            MediaType.CDROMXA,

            // test_multiple_indexes.cue
            // This is a mistake by IsoBuster
            MediaType.CDDA,

            // test_multisession.cue
            // This is a mistake by IsoBuster
            MediaType.CDROMXA,

            // test_multisession_dvd+r.cue
            // This is a mistake by IsoBuster
            MediaType.DVDPRDL,

            // test_multisession_dvd-r.cue
            MediaType.DVDR,

            // test_videocd.cue
            // This is a mistake by IsoBuster
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // gigarec.cue
            "b7659466b925296a36390c58c480e4bb",

            // jaguarcd.cue
            "e20824bc6258d8434096c84548f1c4cf",

            // pcengine.cue
            "989122b6c1f0fc135ee6d481bc347295",

            // pcfx.cue
            "0034c2e54afd76387797c7221c4a054b",

            // report_cdr.cue
            "aacfe792d28a17f641c7218ccd35f5ff",

            // report_cdrom.cue
            "bf4bbec517101d0d6f45d2e4d50cb875",

            // report_cdrw.cue
            "1e55aa420ca8f8ea77d5b597c9cfc19b",

            // test_audiocd_cdtext.cue
            "b236def899758bd04b8a3105b47126db",

            // test_enhancedcd.cue
            "04b7bcd252635eaa8e6b21c1597d44ba",

            // test_incd_udf200_finalized.cue
            "7b3e4a952c369cd4837cee40f1a567f2",

            // test_multi_karaoke_sampler.cue
            "546f85b167c61c2e80dec709f4a4bfb5",

            // test_multiple_indexes.cue
            "4bc4eb89184a69d902ecc1f2745ecf32",

            // test_multisession.cue
            "671f5b747692780a979b3c4b59b39597",

            // test_multisession_dvd+r.cue
            "020993315e49ab0d36bc7248819162ea",

            // test_multisession_dvd-r.cue
            "dff8f2107a4ea9633a88ce38ff609b8e",

            // test_videocd.cue
            "22d646f182b79efcf8915fd01f484391"
        };

        readonly string[] _longMd5S =
        {
            // gigarec.cue
            "51bf2c54fee363520906709cc42a710a",

            // jaguarcd.cue
            "e20824bc6258d8434096c84548f1c4cf",

            // pcengine.cue
            "2f58bc40012040bd3c9e4ae56fbbfad3",

            // pcfx.cue
            "77a9dcd8f5a69d939e076e45602923e0",

            // report_cdr.cue
            "73e38276225ec2d26c0ace10d42513e1",

            // report_cdrom.cue
            "3d3f9cf7d1ba2249b1e7960071e5af46",

            // report_cdrw.cue
            "3af5f943ddb9427d9c63a4ce3b704db9",

            // test_audiocd_cdtext.cue
            "b236def899758bd04b8a3105b47126db",

            // test_enhancedcd.cue
            "b480c86b959c246294a2cc4ad3180cbf",

            // test_incd_udf200_finalized.cue
            "d6555969dd70fb2772cd5b979c6fa284",

            // test_multi_karaoke_sampler.cue
            "82e40f2e2e36a1ec2eeb89ea154aa7f3",

            // test_multiple_indexes.cue
            "4bc4eb89184a69d902ecc1f2745ecf32",

            // test_multisession.cue
            "4171f86df9f3b8c277958324a48c54d8",

            // test_multisession_dvd+r.cue
            "020993315e49ab0d36bc7248819162ea",

            // test_multisession_dvd-r.cue
            "dff8f2107a4ea9633a88ce38ff609b8e",

            // test_videocd.cue
            "72243676a71ff7a3161dce368d3ddc71"
        };

        readonly string[] _subchannelMd5S =
        {
            // gigarec.cue
            null,

            // jaguarcd.cue
            null,

            // pcengine.cue
            null,

            // pcfx.cue
            null,

            // report_cdr.cue
            null,

            // report_cdrom.cue
            null,

            // report_cdrw.cue
            null,

            // test_audiocd_cdtext.cue
            null,

            // test_enhancedcd.cue
            null,

            // test_incd_udf200_finalized.cue
            null,

            // test_multi_karaoke_sampler.cue
            null,

            // test_multiple_indexes.cue
            null,

            // test_multisession.cue
            null,

            // test_multisession_dvd+r.cue
            null,

            // test_multisession_dvd-r.cue
            null,

            // test_videocd.cue
            null
        };

        readonly int[] _tracks =
        {
            // gigarec.cue
            1,

            // jaguarcd.cue
            11,

            // pcengine.cue
            16,

            // pcfx.cue
            8,

            // report_cdr.cue
            1,

            // report_cdrom.cue
            1,

            // report_cdrw.cue
            1,

            // test_audiocd_cdtext.cue
            11,

            // test_enhancedcd.cue
            3,

            // test_incd_udf200_finalized.cue
            1,

            // test_multi_karaoke_sampler.cue
            16,

            // test_multiple_indexes.cue
            5,

            // test_multisession.cue
            4,

            // test_multisession_dvd+r.cue
            2,

            // test_multisession_dvd-r.cue
            2,

            // test_videocd.cue
            2
        };

        readonly int[][] _trackSessions =
        {
            // gigarec.cue
            new[]
            {
                1
            },

            // jaguarcd.cue
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // pcengine.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // pcfx.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // report_cdr.cue
            new[]
            {
                1
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

            // test_audiocd_cdtext.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_enhancedcd.cue
            new[]
            {
                1, 1, 2
            },

            // test_incd_udf200_finalized.cue
            new[]
            {
                1
            },

            // test_multi_karaoke_sampler.cue
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // test_multiple_indexes.cue
            new[]
            {
                1, 1, 1, 1, 1
            },

            // test_multisession.cue
            new[]
            {
                1, 2, 3, 4
            },

            // test_multisession_dvd+r.cue
            new[]
            {
                1, 2
            },

            // test_multisession_dvd-r.cue
            new[]
            {
                1, 2
            },

            // test_videocd.cue
            new[]
            {
                1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // gigarec.cue
            new ulong[]
            {
                0
            },

            // jaguarcd.cue
            new ulong[]
            {
                0, 27490, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // pcengine.cue
            new ulong[]
            {
                0, 3440, 38614, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // pcfx.cue
            new ulong[]
            {
                0, 4245, 4909, 5941, 42059, 220795, 225646, 235498
            },

            // report_cdr.cue
            new ulong[]
            {
                0
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

            // test_audiocd_cdtext.cue
            new ulong[]
            {
                0, 29752, 65184, 78576, 95230, 126297, 155109, 191835, 222926, 243588, 269750
            },

            // test_enhancedcd.cue
            new ulong[]
            {
                0, 14255, 40203
            },

            // test_incd_udf200_finalized.cue
            new ulong[]
            {
                0
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                0, 1737, 32749, 52672, 70304, 100098, 119761, 136999, 155790, 175826, 206461, 226450, 244355, 273965,
                293752, 310711
            },

            // test_multiple_indexes.cue
            new ulong[]
            {
                0, 4654, 13875, 41185, 54989
            },

            // test_multisession.cue
            new ulong[]
            {
                0, 19383, 32710, 45228
            },

            // test_multisession_dvd+r.cue
            new ulong[]
            {
                0, 23914
            },

            // test_multisession_dvd-r.cue
            new ulong[]
            {
                0, 235098
            },

            // test_videocd.cue
            new ulong[]
            {
                0, 1100
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // gigarec.cue
            new ulong[]
            {
                469651
            },

            // jaguarcd.cue
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // pcengine.cue
            new ulong[]
            {
                3589, 38613, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },

            // pcfx.cue
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220794, 225645, 235497, 246679
            },

            // report_cdr.cue
            new ulong[]
            {
                254264
            },

            // report_cdrom.cue
            new ulong[]
            {
                254264
            },

            // report_cdrw.cue
            new ulong[]
            {
                308223
            },

            // test_audiocd_cdtext.cue
            new ulong[]
            {
                29901, 65183, 78575, 95229, 126296, 155108, 191834, 222925, 243587, 269749, 277695
            },

            // test_enhancedcd.cue
            new ulong[]
            {
                14254, 28952, 59205
            },

            // test_incd_udf200_finalized.cue
            new ulong[]
            {
                350133
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                1886, 32748, 52671, 70303, 100097, 119760, 136998, 155789, 175825, 206460, 226449, 244354, 273964,
                293751, 310710, 329157
            },

            // test_multiple_indexes.cue
            new ulong[]
            {
                4803, 13874, 41184, 54988, 65535
            },

            // test_multisession.cue
            new ulong[]
            {
                8132, 25959, 38477, 51167
            },

            // test_multisession_dvd+r.cue
            new ulong[]
            {
                24063, 230623
            },

            // test_multisession_dvd-r.cue
            new ulong[]
            {
                235247, 257263
            },

            // test_videocd.cue
            new ulong[]
            {
                1099, 48793
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // gigarec.cue
            new ulong[]
            {
                150
            },

            // jaguarcd.cue
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.cue
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcfx.cue
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0
            },

            // report_cdr.cue
            new ulong[]
            {
                150
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

            // test_audiocd_cdtext.cue
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_enhancedcd.cue
            new ulong[]
            {
                150, 150, 150
            },

            // test_incd_udf200_finalized.cue
            new ulong[]
            {
                150
            },

            // test_multi_karaoke_sampler.cue
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // test_multiple_indexes.cue
            new ulong[]
            {
                150, 0, 0, 0, 150
            },

            // test_multisession.cue
            new ulong[]
            {
                150, 150, 150, 150
            },

            // test_multisession_dvd+r.cue
            new ulong[]
            {
                0, 0
            },

            // test_multisession_dvd-r.cue
            new ulong[]
            {
                0, 0
            },

            // test_videocd.cue
            new ulong[]
            {
                150, 152
            }
        };

        readonly byte[][] _trackFlags =
        {
            // gigarec.cue
            new byte[]
            {
                4
            },

            // jaguarcd.cue
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // pcengine.cue
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // pcfx.cue
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // report_cdr.cue
            new byte[]
            {
                4
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

            // test_audiocd_cdtext.cue
            // This is an error from IsoBuster, should be 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_enhancedcd.cue
            new byte[]
            {
                0, 0, 4
            },

            // test_incd_udf200_finalized.cue
            // This is an error from IsoBuster, should be 7
            new byte[]
            {
                4
            },

            // test_multi_karaoke_sampler.cue
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // test_multiple_indexes.cue
            // This is an error from IsoBuster, should be 2, 0, 0, 8, 1
            new byte[]
            {
                0, 0, 0, 0, 0
            },

            // test_multisession.cue
            new byte[]
            {
                4, 4, 4, 4
            },

            // test_multisession_dvd+r.cue
            null,

            // test_multisession_dvd-r.cue
            null,

            // test_videocd.cue
            new byte[]
            {
                4, 4
            }
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "IsoBuster", "Cuesheet");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            for(int i = 0; i < _testFiles.Length; i++)
            {
                var filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);

                var  image  = new CdrWin();
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

                    var  image  = new CdrWin();
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