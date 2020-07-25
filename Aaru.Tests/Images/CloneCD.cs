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
// Copyright © 2011-2020 Natalia Portillo
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
    public class CloneCD
    {
        readonly string[] _testFiles =
        {
            "audiocd_cdtext.ccd", "cdg.ccd", "cdplus.ccd", "cdr.ccd", "cdrom80mm.ccd", "cdrom.ccd", "cdrw.ccd",
            "cdi.ccd", "fakecdtext.ccd", "gdrom.ccd", "jaguarcd.ccd", "mixed.ccd", "multitrack.ccd", "pcengine.ccd",
            "pcfx.ccd", "videocd.ccd"
        };

        readonly ulong[] _sectors =
        {
            222187, 309546, 291916, 86365, 19042, 502, 1219, 309834, 1385, 6400, 232187, 283397, 328360, 160956, 246680,
            205072
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.CDDA, MediaType.CDDA, MediaType.CDROMXA, MediaType.CDROM, MediaType.CDROM, MediaType.CDROM,
            MediaType.CDROM, MediaType.CDROMXA, MediaType.CDDA, MediaType.CDROMXA, MediaType.CDDA, MediaType.CDROMXA,
            MediaType.CDROM, MediaType.CD, MediaType.CD, MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            "1a4f916dff70030e26fe0454729d0e79", "d61ace888212ea274071e4c454dfaf5c", "056639b0a0d5c9aa8874ee49a75a31c4",
            "574039238706d45482c4f4c4e792394d", "20a0307dc58aa2ab409e903b3ba85518", "a637b6849f983623efd86563af30e6d9",
            "a280948374cacd96d11417be74b504e1", "891e8e6ec7b6da12ce5a16df2831d5ba", "d68b727b1ea31011ad36f06c1e79d0b1",
            "b8795d40ccbd9d480cfe79961b9fb3cc", "3147ff203341692813de8e5775f45d84", "975ea5de5ad82b2a339e1e8b8f7cd048",
            "41458c6ff3e35aa635cc2f2fdb5582ae", "127b0a92b00ea9a67df1ed8c80daadc7", "9d538bd1ee1db068685ed59d29185941",
            "47284e4065fbb26c94cf13870cb31c5d"
        };

        readonly string[] _longMd5S =
        {
            "1a4f916dff70030e26fe0454729d0e79", "d61ace888212ea274071e4c454dfaf5c", "aa01f7aa762633732d1afc611e9261d2",
            "269652107bdf6afe66c1bd26e34d010e", "a940232a64a51e2848fdd7ea22cbb5f1", "e242fd3e7e353af1661b453dfe7f0562",
            "5657eb302e16577ddade84c87219f5e6", "e396c2ee9dfee53fb69a168af0c19028", "d68b727b1ea31011ad36f06c1e79d0b1",
            "b2297e21f26a509701d48507626e8990", "3147ff203341692813de8e5775f45d84", "043959be4abd2520e7c763918a79bd21",
            "e9767e5fbd6ea745a6d8bcdaed65da19", "6ead3bdedb374f7b9bdf24773d30e491", "76f4bd63c13db3e44fbf7acda20f49e2",
            "93dccc154dabfbe98790b462f1b8dec3"
        };

        readonly string[] _subchannelMd5S =
        {
            "1e9be6967360c1c883e5a67e7be63356", "8eae4188011be97b789f2ee9af9dc487", "6fe2ecad1b18e30b4979a54120421d69",
            "c581d52ac015669a74a9d66e4a46521d", "d4920193b33661e305c17078b6517848", "6c08365c619d9020e72f1dc6d42b6959",
            "9f6910ce13a752a0fae0b5ab17f4fdcd", "45efce2da54bb797f8ea2b742861436e", "92e8315eb05526b712ebcf0f3da96b44",
            "170a101ba07096891640e9405270ed29", "0534d96336d9fb46f2c48c9c27f07999", "d57760b6feedfbf88f4b080ad2567760",
            "d8347b4ef06cc352e1ebbd7b75e84c2b", "315ee5ebb36969b4ce0fb0162f7a9932", "d9804e5f919ffb1531832049df8f0165",
            "24a8b5486521bd8d0223a4f31a2272ba"
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
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },
            new[]
            {
                1, 2
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
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
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
                309833
            },
            new ulong[]
            {
                449, 899, 1384
            },
            new ulong[]
            {
                449, 6399
            },
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },
            new ulong[]
            {
                16735, 40042, 51155, 88836, 116402, 149908, 188937, 214293, 243624, 259728, 283396
            },
            new ulong[]
            {
                48657, 207527, 328359
            },
            new ulong[]
            {
                3589, 38613, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220794, 225645, 235497, 246679
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
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0
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
                0, 0, 0
            },
            new ulong[]
            {
                0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0
            },
            new ulong[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
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
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                0, 0
            },
            new byte[]
            {
                0
            },
            new byte[]
            {
                0
            },
            new byte[]
            {
                0
            },
            new byte[]
            {
                0
            },
            new byte[]
            {
                0, 0, 0
            },
            new byte[]
            {
                0, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                0, 0, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0
            },
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CloneCD");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                IFilter filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);
                IOpticalMediaImage image = new CloneCd();
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