// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol120.cs
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Alcohol120
    {
        readonly string[] _testFiles =
        {
            "audiocd_cdtext.mds", "cdg.mds", "cdplus.mds", "cdr.mds", "cdrom80mm.mds", "cdrom.mds", "cdrw.mds",
            "dvdrom.mds", "fakecdtext.mds", "gdrom.mds", "jaguarcd.mds", "mixed.mds", "multitrack.mds", "pcengine.mds",
            "pcfx.mds", "videocd.mds"
        };

        readonly ulong[] _sectors =
        {
            222187, 309546, 303316, 97765, 19042, 502, 1219, 2287072, 1385, 6400, 243587, 283397, 328360, 160956,
            246680, 205072
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.CDDA, MediaType.CDDA, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROM,
            MediaType.CDRW, MediaType.DVDROM, MediaType.CDR, MediaType.CDROMXA, MediaType.CDDA, MediaType.CDROMXA,
            MediaType.CDROM, MediaType.CD, MediaType.CD, MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            "1a4f916dff70030e26fe0454729d0e79", "d61ace888212ea274071e4c454dfaf5c", "056639b0a0d5c9aa8874ee49a75a31c4",
            "7bd3f0b9ebc90c48f5dfce2bf67cc7a3", "20a0307dc58aa2ab409e903b3ba85518", "a637b6849f983623efd86563af30e6d9",
            "a280948374cacd96d11417be74b504e1", "b9b0b4318e6264c405c3f96128901815", "d68b727b1ea31011ad36f06c1e79d0b1",
            "919202f8dc03fefd2d8a3cb92f5a1a0a", "8086a3654d6dede562621d24ae18729e", "c68e679f86b62b02b9cb66b5e217d15b",
            "41458c6ff3e35aa635cc2f2fdb5582ae", "0dac1b20a9dc65c4ed1b11f6160ed983", "bc514cb4f3c7e2ee6857b2a3d470278b",
            "c28398b4b49c45dce49aa04c71893baa"
        };

        readonly string[] _longMd5S =
        {
            "1a4f916dff70030e26fe0454729d0e79", "d61ace888212ea274071e4c454dfaf5c", "aa01f7aa762633732d1afc611e9261d2",
            "784fd68573817893a3a0a6e05684ea52", "a940232a64a51e2848fdd7ea22cbb5f1", "e242fd3e7e353af1661b453dfe7f0562",
            "5657eb302e16577ddade84c87219f5e6", "b9b0b4318e6264c405c3f96128901815", "d68b727b1ea31011ad36f06c1e79d0b1",
            "9e624b9d02cb876640015e7f7027766c", "8086a3654d6dede562621d24ae18729e", "e431e3438d45af3156c0c2348d29217a",
            "2d349b97860ab08666416fdc424097b1", "f1c1dbe1cd9df11fe2c1f0a97130c25f", "dac5dc0961fa435da3c7d433477cda1a",
            "ab9f7cd1c27eec3292ab438df99381ce"
        };

        readonly string[] _subchannelMd5S =
        {
            "52a66b04f28fd3161bb4d41b80c621dc", "6d77b37c8ca0946f56a6452930910390", "c1fd2a298224d7d3b0fd096fbc97c758",
            "6f3d69f6ea2fa81e84c325a36d37ecb8", "fab80226b764a4bde04a1b182f5f68f5", "6c08365c619d9020e72f1dc6d42b6959",
            "9f6910ce13a752a0fae0b5ab17f4fdcd", "db0b4318e6264c405c3f96128901815", "2af826aec596b10393eaabb994471236",
            "a8599b043483431b4b9380b9f71ec228", "83ec1010fc44694d69dc48bacec5481a", "614fdc36219120b833fdfa62eee5b0f7",
            "a9158c3b13aee6fdde14031be61d9a5c", "9e9a6b51bc2e5ec67400cb33ad0ca33f", "e3a0d78b6c32f5795b1b513bd13a6bda",
            "85b051cae460fe505644e734dde032a0"
        };

        readonly int[] _tracks =
        {
            13, 16, 14, 2, 1, 1, 1, 1, 3, 2, 11, 11, 3, 16, 8, 18
        };

        readonly int[][] _trackSessions =
        {
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 // TODO: 2 goes here
            },
            new[]
            {
                1, 1 // TODO: 2 goes here
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
                // TODO
                // 2 goes here
                // 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
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
                0, 14710, 31607, 48487, 70120, 86710, 103485, 122540, 139935, 153990, 172670, 190525, 205465
            },
            new ulong[]
            {
                0, 17377, 38525, 54936, 72860, 90755, 114546, 136451, 154773, 172150, 193298, 209709, 227633, 245528,
                269319, 291224
            },
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234180
            },
            new ulong[]
            {
                0, 42887
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
                0, 450, 900
            },
            new ulong[]
            {
                0, 450
            },
            new ulong[]
            {
                0, 27640, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },
            new ulong[]
            {
                0, 16736, 40043, 51156, 88837, 116403, 149909, 188938, 214294, 243625, 259729
            },
            new ulong[]
            {
                0, 48658, 207528
            },
            new ulong[]
            {
                0, 3590, 38614, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220795, 225646, 235498
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
                14709, 31606, 48486, 70119, 86709, 103484, 122539, 139934, 153989, 172669, 190524, 205464, 222186
            },
            new ulong[]
            {
                17376, 38524, 54935, 72859, 90754, 114545, 136450, 154772, 172149, 193297, 209708, 227632, 245527,
                269318, 291223, 309545
            },
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },
            new ulong[]
            {
                31486, 97764
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
                2287071
            },
            new ulong[]
            {
                449, 899, 1384
            },
            new ulong[]
            {
                299, 6399
            },
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },
            new ulong[]
            {
                16585, 40042, 51155, 88836, 116402, 149908, 188937, 214293, 243624, 259728, 283396
            },
            new ulong[]
            {
                48657, 207527, 328359
            },
            new ulong[]
            {
                3439, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126078, 160955
            },
            new ulong[]
            {
                4244, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },
            new ulong[]
            {
                1949, 12984, 20450, 39497, 47367, 56599, 67386, 71545, 77029, 80534, 95179, 110807, 115448, 118722,
                123265, 128327, 131636, 205071
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },
            new ulong[]
            {
                150, 150
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
                0
            },
            new ulong[]
            {
                150, 0, 0
            },
            new ulong[]
            {
                150, 150
            },
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                150, 0, 0
            },
            new ulong[]
            {
                150, 150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },
            new ulong[]
            {
                150, 150, 0, 0, 0, 150, 0, 0
            },
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
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
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },
            new byte[]
            {
                4, 4
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
            null, new byte[]
            {
                2, 2, 2
            },
            new byte[]
            {
                4, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
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
            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Alcohol 120%");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                IFilter filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);
                IOpticalMediaImage image = new DiscImages.Alcohol120();
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