// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite6.cs
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
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class BlindWrite6
    {
        readonly string[] _testFiles =
        {
            "audiocd_cdtext.B6T", "cdg.B6T", "cdplus.B6T", "cdr.B6T", "cdrom80mm.B6T", "cdrom.B6T", "cdrw.B6T",

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            //"dvdrom.B6T", 
            "fakecdtext6.B6T", "gdrom.B6T", "jaguarcd.B6T", "mixed.B6T", "multitrack.B6T", "pcengine.B6T", "pcfx.B6T",
            "videocd.B6T", "cdi.B6T"
        };

        readonly ulong[] _sectors =
        {
            // "audiocd_cdtext.B6T"
            222187,

            // "cdg.B6T"
            309546,

            // "cdplus.B6T"
            303316,

            // "cdr.B6T"
            97765,

            // "cdrom80mm.B6T"
            19042,

            // "cdrom.B6T"
            502,

            // "cdrw.B6T"
            1219,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            //2287072,
            // "fakecdtext6.B6T"
            1385,

            // "gdrom.B6T"
            6400,

            // "jaguarcd.B6T"
            243587,

            // "mixed.B6T"
            283397,

            // "multitrack.B6T"
            328360,

            // "pcengine.B6T"
            160956,

            // "pcfx.B6T"    
            246680,

            // "videocd.B6T"
            205072,

            // "cdi.B6T"
            309834
        };

        readonly MediaType[] _mediaTypes =
        {
            // "audiocd_cdtext.B6T"
            MediaType.CDDA,

            // "cdg.B6T"
            MediaType.CDDA,

            // "cdplus.B6T"
            MediaType.CDPLUS,

            // "cdr.B6T"
            MediaType.CDR,

            // "cdrom80mm.B6T"
            MediaType.CDROM,

            // "cdrom.B6T"
            MediaType.CDROM,

            // "cdrw.B6T"
            MediaType.CDRW,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            //MediaType.DVDROM, 
            // "fakecdtext6.B6T"
            MediaType.CDR,

            // "gdrom.B6T"
            MediaType.CDROMXA,

            // "jaguarcd.B6T"
            MediaType.CDDA,

            // "mixed.B6T"
            MediaType.CDROMXA,

            // "multitrack.B6T"
            MediaType.CDROM,

            // "pcengine.B6T"
            MediaType.CD,

            // "pcfx.B6T"    
            MediaType.CD,

            // "videocd.B6T"
            MediaType.CDROMXA,

            // "cdi.B6T"
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // "audiocd_cdtext.B6T"
            "1a4f916dff70030e26fe0454729d0e79",

            // "cdg.B6T"
            "d61ace888212ea274071e4c454dfaf5c",

            // "cdplus.B6T"
            "22c1dc74889d87ffb80e0a4d03cac230",

            // "cdr.B6T"
            "9bcde8ddcb945557c58c08bfa08f857c",

            // "cdrom80mm.B6T"
            "20a0307dc58aa2ab409e903b3ba85518",

            // "cdrom.B6T"
            "a637b6849f983623efd86563af30e6d9",

            // "cdrw.B6T"
            "a280948374cacd96d11417be74b504e1",

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            //"b9b0b4318e6264c405c3f96128901815",
            // "fakecdtext6.B6T"
            "d68b727b1ea31011ad36f06c1e79d0b1",

            // "gdrom.B6T"
            "b8795d40ccbd9d480cfe79961b9fb3cc",

            // "jaguarcd.B6T"
            "3dd5bd0f7d95a40d411761d69255567a",

            // "mixed.B6T"
            "16444db03f93623cdd4939b2905e5f62",

            // "multitrack.B6T"
            "41458c6ff3e35aa635cc2f2fdb5582ae",

            // "pcengine.B6T"
            "4f5165069b3c5f11afe5f59711bd945d",

            // "pcfx.B6T"    
            "c1bc8de499756453d1387542bb32bb4d",

            // "videocd.B6T"
            "47284e4065fbb26c94cf13870cb31c5d",

            // "cdi.B6T"
            "d47ec66b7aba66e38c2edbbe6fc552f0"
        };

        readonly string[] _longMd5S =
        {
            // "audiocd_cdtext.B6T"
            "1a4f916dff70030e26fe0454729d0e79",

            // "cdg.B6T"
            "d61ace888212ea274071e4c454dfaf5c",

            // "cdplus.B6T"
            "b59128c19a617782f5a1f22263046ad7",

            // "cdr.B6T"
            "193dff356e269d79e72dfe9a6bc2e1eb",

            // "cdrom80mm.B6T"
            "a940232a64a51e2848fdd7ea22cbb5f1",

            // "cdrom.B6T"
            "e242fd3e7e353af1661b453dfe7f0562",

            // "cdrw.B6T"
            "5657eb302e16577ddade84c87219f5e6",

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            //"b9b0b4318e6264c405c3f96128901815",
            // "fakecdtext6.B6T"
            "d68b727b1ea31011ad36f06c1e79d0b1",

            // "gdrom.B6T"
            "b2297e21f26a509701d48507626e8990",

            // "jaguarcd.B6T"
            "3dd5bd0f7d95a40d411761d69255567a",

            // "mixed.B6T"
            "59a0a3a6d5bc7fa39f626de09addd98e",

            // "multitrack.B6T"
            "2917ddb231651a6174dad9c8fb684467",

            // "pcengine.B6T"
            "fd30db9486f67654179c90c8a5052edb",

            // "pcfx.B6T"    
            "455ec326506d2c5b974c4617c1010796",

            // "videocd.B6T"
            "93dccc154dabfbe98790b462f1b8dec3",

            // "cdi.B6T"
            "e08c1cf7a1f3ba02c56e81f7d4889964"
        };

        readonly string[] _subchannelMd5S =
        {
            // "audiocd_cdtext.B6T"
            null,

            // "cdg.B6T"
            null,

            // "cdplus.B6T"
            null,

            // "cdr.B6T"
            null,

            // "cdrom80mm.B6T"
            null,

            // "cdrom.B6T"
            null,

            // "cdrw.B6T"
            null,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            //null,
            // "fakecdtext6.B6T"
            null,

            // "gdrom.B6T"
            null,

            // "jaguarcd.B6T"
            null,

            // "mixed.B6T"
            null,

            // "multitrack.B6T"
            null,

            // "pcengine.B6T"
            null,

            // "pcfx.B6T"    
            null,

            // "videocd.B6T"
            null,

            // "cdi.B6T"
            null
        };

        readonly int[] _tracks =
        {
            // "audiocd_cdtext.B6T"
            13,

            // "cdg.B6T"
            16,

            // "cdplus.B6T"
            14,

            // "cdr.B6T"
            2,

            // "cdrom80mm.B6T"
            1,

            // "cdrom.B6T"
            1,

            // "cdrw.B6T"
            1,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            //1,
            // "fakecdtext6.B6T"
            3,

            // "gdrom.B6T"
            2,

            // "jaguarcd.B6T"
            11,

            // "mixed.B6T"
            11,

            // "multitrack.B6T"
            3,

            // "pcengine.B6T"
            16,

            // "pcfx.B6T"    
            8,

            // "videocd.B6T"
            18,

            // "cdi.B6T"
            1
        };

        readonly int[][] _trackSessions =
        {
            // "audiocd_cdtext.B6T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "cdg.B6T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "cdplus.B6T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // "cdr.B6T"
            new[]
            {
                1, 2
            },

            // "cdrom80mm.B6T"
            new[]
            {
                1
            },

            // "cdrom.B6T"
            new[]
            {
                1
            },

            // "cdrw.B6T"
            new[]
            {
                1
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            /*
                  new[]
                  {
                      1
                  },
                  */
            // "fakecdtext6.B6T"
            new[]
            {
                1, 1, 1
            },

            // "gdrom.B6T"
            new[]
            {
                1, 1
            },

            // "jaguarcd.B6T"
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // "mixed.B6T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "multitrack.B6T"
            new[]
            {
                1, 1, 1
            },

            // "pcengine.B6T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "pcfx.B6T"    
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // "videocd.B6T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "cdi.B6T"
            new[]
            {
                1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // "audiocd_cdtext.B6T"
            new ulong[]
            {
                0, 14710, 31607, 48487, 70120, 86710, 103485, 122540, 139935, 153990, 172670, 190525, 205465
            },

            // "cdg.B6T"
            new ulong[]
            {
                0, 17377, 38525, 54936, 72860, 90755, 114546, 136451, 154773, 172150, 193298, 209709, 227633, 245528,
                269319, 291224
            },

            // "cdplus.B6T"
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234030
            },

            // "cdr.B6T"
            new ulong[]
            {
                0, 42737
            },

            // "cdrom80mm.B6T"
            new ulong[]
            {
                0
            },

            // "cdrom.B6T"
            new ulong[]
            {
                0
            },

            // "cdrw.B6T"
            new ulong[]
            {
                0
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            /*
                  new ulong[]
                  {
                      0
                  },
                  */
            // "fakecdtext6.B6T"
            new ulong[]
            {
                0, 450, 900
            },

            // "gdrom.B6T"
            new ulong[]
            {
                0, 450
            },

            // "jaguarcd.B6T"
            new ulong[]
            {
                0, 27490, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // "mixed.B6T"
            new ulong[]
            {
                0, 16586, 40043, 51156, 88837, 116403, 149909, 188938, 214294, 243625, 259729
            },

            // "multitrack.B6T"
            new ulong[]
            {
                0, 48658, 207528
            },

            // "pcengine.B6T"
            new ulong[]
            {
                0, 3590, 38464, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // "pcfx.B6T"    
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220645, 225646, 235498
            },

            // "videocd.B6T"
            new ulong[]
            {
                0, 2100, 12985, 20451, 39498, 47368, 56600, 67387, 71546, 77030, 80535, 95180, 110808, 115449, 118723,
                123266, 128328, 131637
            },

            // "cdi.B6T"
            new ulong[]
            {
                0
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // "audiocd_cdtext.B6T"
            new ulong[]
            {
                14709, 31606, 48486, 70119, 86709, 103484, 122539, 139934, 153989, 172669, 190524, 205464, 222186
            },

            // "cdg.B6T"
            new ulong[]
            {
                17376, 38524, 54935, 72859, 90754, 114545, 136450, 154772, 172149, 193297, 209708, 227632, 245527,
                269318, 291223, 309545
            },

            // "cdplus.B6T"
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // "cdr.B6T"
            new ulong[]
            {
                31336, 97764
            },

            // "cdrom80mm.B6T"
            new ulong[]
            {
                19041
            },

            // "cdrom.B6T"
            new ulong[]
            {
                501
            },

            // "cdrw.B6T"
            new ulong[]
            {
                1218
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            /*
                 new ulong[]
                 {
                     2287071
                 },
                 */
            // "fakecdtext6.B6T"
            new ulong[]
            {
                449, 899, 1384
            },

            // "gdrom.B6T"
            new ulong[]
            {
                449, 6399
            },

            // "jaguarcd.B6T"
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // "mixed.B6T"
            new ulong[]
            {
                16585, 40042, 51155, 88836, 116402, 149908, 188937, 214293, 243624, 259728, 283396
            },

            // "multitrack.B6T"
            new ulong[]
            {
                48657, 207527, 328359
            },

            // "pcengine.B6T"
            new ulong[]
            {
                3589, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },

            // "pcfx.B6T"    
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },

            // "videocd.B6T"
            new ulong[]
            {
                2099, 12984, 20450, 39497, 47367, 56599, 67386, 71545, 77029, 80534, 95179, 110807, 115448, 118722,
                123265, 128327, 131636, 205071
            },

            // "cdi.B6T"
            new ulong[]
            {
                309833
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // "audiocd_cdtext.B6T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdg.B6T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdplus.B6T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // "cdr.B6T"
            new ulong[]
            {
                150, 150
            },

            // "cdrom80mm.B6T"
            new ulong[]
            {
                150
            },

            // "cdrom.B6T"
            new ulong[]
            {
                150
            },

            // "cdrw.B6T"
            new ulong[]
            {
                150
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            /*
                 new ulong[]
                 {
                     0
                 },
                 */
            // "fakecdtext6.B6T"
            new ulong[]
            {
                150, 0, 0
            },

            // "gdrom.B6T"
            new ulong[]
            {
                150, 0
            },

            // "jaguarcd.B6T"
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "mixed.B6T"
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "multitrack.B6T"
            new ulong[]
            {
                150, 0, 0
            },

            // "pcengine.B6T"
            new ulong[]
            {
                150, 0, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "pcfx.B6T"    
            new ulong[]
            {
                150, 0, 0, 0, 0, 150, 0, 0
            },

            // "videocd.B6T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdi.B6T"
            new ulong[]
            {
                150
            }
        };

        readonly byte[][] _trackFlags =
        {
            // "audiocd_cdtext.B6T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdg.B6T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdplus.B6T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // "cdr.B6T"
            new byte[]
            {
                4, 4
            },

            // "cdrom80mm.B6T"
            new byte[]
            {
                4
            },

            // "cdrom.B6T"
            new byte[]
            {
                4
            },

            // "cdrw.B6T"
            new byte[]
            {
                4
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B6T"
            // null,
            // "fakecdtext6.B6T"
            new byte[]
            {
                2, 2, 2
            },

            // "gdrom.B6T"
            new byte[]
            {
                4, 0
            },

            // "jaguarcd.B6T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "mixed.B6T"
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "multitrack.B6T"
            new byte[]
            {
                4, 4, 4
            },

            // "pcengine.B6T"
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // "pcfx.B6T"    
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // "videocd.B6T"
            new byte[]
            {
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4
            },

            // "cdi.B6T"
            new byte[]
            {
                4
            }
        };

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 6");

            for(int i = 0; i < _testFiles.Length; i++)
            {
                IFilter filter = new ZZZNoFilter();
                filter.Open(_testFiles[i]);
                IOpticalMediaImage image = new DiscImages.BlindWrite5();
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