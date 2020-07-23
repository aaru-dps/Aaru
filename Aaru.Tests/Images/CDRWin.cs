// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CDRWin.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.DiscImages;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class CDRWin
    {
        readonly string[] _testFiles =
        {
            "audiocd_cdtext.cue", "cdi.cue", "cdrom80mm.cue", "cdrom.cue", "cdrw.cue", "fakecdtext.cue", "gdrom.cue",
            "mixed.cue", "multitrack.cue", "pcengine.cue", "pcfx.cue", "videocd.cue"
        };

        readonly ulong[] _sectors =
        {
            222187, 309834, 19042, 502, 1219, 1385, 6250, 283247, 328360, 160356, 246305, 205072
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.CDDA, MediaType.CDROMXA, MediaType.CDROM, MediaType.CDROM, MediaType.CDROM, MediaType.CDDA,
            MediaType.CDROMXA, MediaType.CDROMXA, MediaType.CDROM, MediaType.CD, MediaType.CD, MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            "1a4f916dff70030e26fe0454729d0e79", "bcbfe40d07149ce495ac7648b42dfafd", "20a0307dc58aa2ab409e903b3ba85518",
            "a637b6849f983623efd86563af30e6d9", "a280948374cacd96d11417be74b504e1", "d68b727b1ea31011ad36f06c1e79d0b1",
            "919202f8dc03fefd2d8a3cb92f5a1a0a", "c68e679f86b62b02b9cb66b5e217d15b", "41458c6ff3e35aa635cc2f2fdb5582ae",
            "8eb436b476c9df343acb89ac1ba7e1b4", "73e2855fff156f95fb8f0ae7c58d1b9d", "47284e4065fbb26c94cf13870cb31c5d"
        };

        readonly string[] _longMd5S =
        {
            "1a4f916dff70030e26fe0454729d0e79", "a00ef23e29318cf3deca0d497df2ee85", "a940232a64a51e2848fdd7ea22cbb5f1",
            "e242fd3e7e353af1661b453dfe7f0562", "5657eb302e16577ddade84c87219f5e6", "d68b727b1ea31011ad36f06c1e79d0b1",
            "9e624b9d02cb876640015e7f7027766c", "e431e3438d45af3156c0c2348d29217a", "38c1bb71be9f5cd17dc8151b4edb9c32",
            "bdcd5cabf4f48333f9dbb08967dce7a8", "f421fc4af3ac528911b6d824825ff9b5", "93dccc154dabfbe98790b462f1b8dec3"
        };

        readonly string[] _subchannelMd5S =
        {
            null, null, null, null, null, null, null, null, null, null, null, null
        };

        readonly int[] _tracks =
        {
            13, 1, 1, 1, 1, 3, 2, 11, 3, 16, 8, 18
        };

        readonly int[][] _trackSessions =
        {
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            new[]
            {
                1
            },
            new[]
            {
                1
            },
            new[]
            {
                1
            },
            new[]
            {
                1
            },
            new[]
            {
                1, 1, 1
            },
            new[]
            {
                1, 1
            },
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            new[]
            {
                1, 1, 1
            },
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            new ulong[]
            {
                0, 14560, 31457, 48337, 69970, 86560, 103335, 122390, 139785, 153840, 172520, 190375, 205315
            },
            new ulong[]
            {
                0
            },
            new ulong[]
            {
                0
            },
            new ulong[]
            {
                0
            },
            new ulong[]
            {
                0
            },
            new ulong[]
            {
                0, 300, 750
            },
            new ulong[]
            {
                0, 300
            },
            new ulong[]
            {
                0, 16586, 39743, 50856, 88537, 116103, 149609, 188638, 213994, 243325, 259429
            },
            new ulong[]
            {
                0, 48658, 207528
            },
            new ulong[]
            {
                0, 3365, 38239, 46692, 52976, 61294, 68038, 74872, 82605, 85956, 90742, 98749, 106168, 111713, 119745,
                125629
            },
            new ulong[]
            {
                0, 4170, 4684, 5716, 41834, 220420, 225121, 234973
            },
            new ulong[]
            {
                0, 2100, 12985, 20451, 39498, 47368, 56600, 67387, 71546, 77030, 80535, 95180, 110808, 115449, 118723,
                123266, 128328, 131637
            }
        };

        readonly ulong[][] _trackEnds =
        {
            new ulong[]
            {
                14559, 31456, 48336, 69969, 86559, 103334, 122389, 139784, 153839, 172519, 190374, 205314, 222186
            },
            new ulong[]
            {
                309833
            },
            new ulong[]
            {
                19041
            },
            new ulong[]
            {
                501
            },
            new ulong[]
            {
                1218
            },
            new ulong[]
            {
                299, 749, 1384
            },
            new ulong[]
            {
                299, 6249
            },
            new ulong[]
            {
                16585, 39742, 50855, 88536, 116102, 149608, 188637, 213993, 243324, 259428, 283246
            },
            new ulong[]
            {
                48657, 207527, 328359
            },
            new ulong[]
            {
                3364, 38238, 46691, 52975, 61293, 68037, 74871, 82604, 85955, 90741, 98748, 106167, 111712, 119744,
                125628, 160355
            },
            new ulong[]
            {
                4169, 4683, 5715, 41833, 220419, 225120, 234972, 246304
            },
            new ulong[]
            {
                2099, 12984, 20450, 39497, 47367, 56599, 67386, 71545, 77029, 80534, 95179, 110807, 115448, 118722,
                123265, 128327, 131636, 205071
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },
            new ulong[]
            {
                150
            },
            new ulong[]
            {
                150
            },
            new ulong[]
            {
                150
            },
            new ulong[]
            {
                150
            },
            new ulong[]
            {
                150, 150, 150
            },
            new ulong[]
            {
                150, 150
            },
            new ulong[]
            {
                150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150
            },
            new ulong[]
            {
                150, 0, 0
            },
            new ulong[]
            {
                150, 225, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 225
            },
            new ulong[]
            {
                150, 225, 0, 0, 0, 150, 150, 150
            },
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        readonly byte[][] _trackFlags =
        {
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                4
            },
            new byte[]
            {
                4
            },
            new byte[]
            {
                4
            },
            new byte[]
            {
                4
            },
            new byte[]
            {
                2, 2, 2
            },
            new byte[]
            {
                4, 0
            },
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                4, 4, 4
            },
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },
            new byte[]
            {
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4
            }
        };

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CDRWin");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                IFilter filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);
                IOpticalMediaImage image = new CdrWin();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_tracks[i], image.Tracks.Count, _testFiles[i]);

                // How many sectors to read at once
                const uint sectorsToRead = 256;

                int trackNo = 0;

                foreach(Track currentTrack in image.Tracks)
                {
                    Assert.AreEqual(_trackSessions[i][trackNo], currentTrack.TrackSession, _testFiles[i]);
                    Assert.AreEqual(_trackStarts[i][trackNo], currentTrack.TrackStartSector, _testFiles[i]);
                    Assert.AreEqual(_trackEnds[i][trackNo], currentTrack.TrackEndSector, _testFiles[i]);
                    Assert.AreEqual(_trackPregaps[i][trackNo], currentTrack.TrackPregap, _testFiles[i]);

                    if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                        Assert.AreEqual(_trackFlags[i][trackNo],
                                        image.ReadSectorTag(currentTrack.TrackSequence, SectorTagType.CdTrackFlags)[0],
                                        _testFiles[i]);

                    trackNo++;
                }

                foreach(bool @long in new[]
                {
                    false, true
                })
                {
                    var ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors     = (currentTrack.TrackEndSector - currentTrack.TrackStartSector) + 1;
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
                                sector = @long ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                       currentTrack.TrackSequence)
                                             : image.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                 currentTrack.TrackSequence);

                                doneSectors += sectors - doneSectors;
                            }

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(), _testFiles[i]);
                }

                if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                {
                    var ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors     = (currentTrack.TrackEndSector - currentTrack.TrackStartSector) + 1;
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

                    Assert.AreEqual(_subchannelMd5S[i], ctx.End(), _testFiles[i]);
                }
            }
        }
    }
}