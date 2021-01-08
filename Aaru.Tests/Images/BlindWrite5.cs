// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite5.cs
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
using Aaru.Filters;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class BlindWrite5
    {
        readonly string[] _testFiles =
        {
            "audiocd_cdtext.B5T", "cdg.B5T", "cdplus.B5T", "cdr.B5T", "cdrom80mm.B5T", "cdrom.B5T", "cdrw.B5T",

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            //"dvdrom.B5T", 
            "cdi.B5T", "gdrom.B5T", "jaguarcd.B5T", "mixed.B5T", "multitrack.B5T", "pcengine.B5T", "pcfx.B5T",
            "videocd.B5T"
        };

        readonly ulong[] _sectors =
        {
            // "audiocd_cdtext.B5T"
            222187,

            // "cdg.B5T"
            309546,

            // "cdplus.B5T"
            303316,

            // "cdr.B5T"
            97765,

            // "cdrom80mm.B5T"
            19042,

            // "cdrom.B5T"
            502,

            // "cdrw.B5T"
            355500,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            //2287072,
            // "cdi.B5T"
            309834,

            // "gdrom.B5T"
            6400,

            // "jaguarcd.B5T"
            243587,

            // "mixed.B5T"
            283397,

            // "multitrack.B5T"
            328360,

            // "pcengine.B5T"
            160956,

            // "pcfx.B5T"
            246680,

            // "videocd.B5T"
            205072
        };

        readonly MediaType[] _mediaTypes =
        {
            // "audiocd_cdtext.B5T"
            MediaType.CDDA,

            // "cdg.B5T"
            MediaType.CDDA,

            // "cdplus.B5T"
            MediaType.CDPLUS,

            // "cdr.B5T"
            MediaType.CDR,

            // "cdrom80mm.B5T"
            MediaType.CDROM,

            // "cdrom.B5T"
            MediaType.CDROM,

            // "cdrw.B5T"
            MediaType.CDRW,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            //MediaType.DVDROM,
            // "cdi.B5T"
            MediaType.CDROMXA,

            // "gdrom.B5T"
            MediaType.CDROMXA,

            // "jaguarcd.B5T"
            MediaType.CDDA,

            // "mixed.B5T"
            MediaType.CDROMXA,

            // "multitrack.B5T"
            MediaType.CDROM,

            // "pcengine.B5T"
            MediaType.CD,

            // "pcfx.B5T"
            MediaType.CD,

            // "videocd.B5T"
            MediaType.CDROMXA
        };

        readonly string[] _md5S =
        {
            // "audiocd_cdtext.B5T"
            "1a4f916dff70030e26fe0454729d0e79",

            // "cdg.B5T"
            "d61ace888212ea274071e4c454dfaf5c",

            // "cdplus.B5T"
            "22c1dc74889d87ffb80e0a4d03cac230",

            // "cdr.B5T"
            "d5c765d46834abc33a1f303a379ec840",

            // "cdrom80mm.B5T"
            "20a0307dc58aa2ab409e903b3ba85518",

            // "cdrom.B5T"
            "a637b6849f983623efd86563af30e6d9",

            // "cdrw.B5T"
            "bd2bc3f0a72a4c41b1afc7267bd70430",

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            //"b9b0b4318e6264c405c3f96128901815",
            // "cdi.B5T"
            "b6aea697cf5580f3f798ffc4d86f48b8",

            // "gdrom.B5T"
            "b8795d40ccbd9d480cfe79961b9fb3cc",

            // "jaguarcd.B5T"
            "3dd5bd0f7d95a40d411761d69255567a",

            // "mixed.B5T"
            "556b0159070ee11a926f3932650c8f2c",

            // "multitrack.B5T"
            "41458c6ff3e35aa635cc2f2fdb5582ae",

            // "pcengine.B5T"
            "4f5165069b3c5f11afe5f59711bd945d",

            // "pcfx.B5T"
            "c1bc8de499756453d1387542bb32bb4d",

            // "videocd.B5T"
            "47284e4065fbb26c94cf13870cb31c5d"
        };

        readonly string[] _longMd5S =
        {
            // "audiocd_cdtext.B5T"
            "1a4f916dff70030e26fe0454729d0e79",

            // "cdg.B5T"
            "d61ace888212ea274071e4c454dfaf5c",

            // "cdplus.B5T"
            "b59128c19a617782f5a1f22263046ad7",

            // "cdr.B5T"
            "18569ebb43ef9eb45f5f26bcbff3ebd7",

            // "cdrom80mm.B5T"
            "a940232a64a51e2848fdd7ea22cbb5f1",

            // "cdrom.B5T"
            "e242fd3e7e353af1661b453dfe7f0562",

            // "cdrw.B5T"
            "40a83558b159ea0a7dae0f87f9fd60d8",

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            //"b9b0b4318e6264c405c3f96128901815",
            // "cdi.B5T"
            "a071c4ff0e6bf75dec2f9293af52fc64",

            // "gdrom.B5T"
            "b2297e21f26a509701d48507626e8990",

            // "jaguarcd.B5T"
            "3dd5bd0f7d95a40d411761d69255567a",

            // "mixed.B5T"
            "ca7e0d49553f026098bfc5cfd5cdc7d0",

            // "multitrack.B5T"
            "6d8d9c35156b26cad81e1c598cc326ac",

            // "pcengine.B5T"
            "fd30db9486f67654179c90c8a5052edb",

            // "pcfx.B6T"    
            "455ec326506d2c5b974c4617c1010796",

            // "videocd.B6T"
            "93dccc154dabfbe98790b462f1b8dec3"
        };

        readonly string[] _subchannelMd5S =
        {
            // "audiocd_cdtext.B5T"
            null,

            // "cdg.B5T"
            null,

            // "cdplus.B5T"
            null,

            // "cdr.B5T"
            null,

            // "cdrom80mm.B5T"
            null,

            // "cdrom.B5T"
            null,

            // "cdrw.B5T"
            null,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            //null,
            // "cdi.B5T"
            null,

            // "gdrom.B5T"
            null,

            // "jaguarcd.B5T"
            null,

            // "mixed.B5T"
            null,

            // "multitrack.B5T"
            null,

            // "pcengine.B6T"
            null,

            // "pcfx.B6T"    
            null,

            // "videocd.B6T"
            null
        };

        readonly int[] _tracks =
        {
            // "audiocd_cdtext.B5T"
            13,

            // "cdg.B5T"
            16,

            // "cdplus.B5T"
            14,

            // "cdr.B5T"
            2,

            // "cdrom80mm.B5T"
            1,

            // "cdrom.B5T"
            1,

            // "cdrw.B5T"
            1,

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            //1, 
            // "cdi.B5T"
            1,

            // "gdrom.B5T"
            2,

            // "jaguarcd.B5T"
            11,

            // "mixed.B5T"
            11,

            // "multitrack.B5T"
            3,

            // "pcengine.B5T"
            16,

            // "pcfx.B5T"
            8,

            // "videocd.B5T"
            18
        };

        readonly int[][] _trackSessions =
        {
            // "audiocd_cdtext.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "cdg.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "cdplus.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2
            },

            // "cdr.B5T"
            new[]
            {
                1, 2
            },

            // "cdrom80mm.B5T"
            new[]
            {
                1
            },

            // "cdrom.B5T"
            new[]
            {
                1
            },

            // "cdrw.B5T"
            new[]
            {
                1
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            /*
            new[]
            {
                1
            },
            */
            // "cdi.B5T"
            new[]
            {
                1
            },

            // "gdrom.B5T"
            new[]
            {
                1, 1
            },

            // "jaguarcd.B5T"
            new[]
            {
                1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },

            // "mixed.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "multitrack.B5T"
            new[]
            {
                1, 1, 1
            },

            // "pcengine.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            },

            // "pcfx.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1
            },

            // "videocd.B5T"
            new[]
            {
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        readonly ulong[][] _trackStarts =
        {
            // "audiocd_cdtext.B5T"
            new ulong[]
            {
                0, 14710, 31607, 48487, 70120, 86710, 103485, 122540, 139935, 153990, 172670, 190525, 205465
            },

            // "cdg.B5T"
            new ulong[]
            {
                0, 17377, 38525, 54936, 72860, 90755, 114546, 136451, 154773, 172150, 193298, 209709, 227633, 245528,
                269319, 291224
            },

            // "cdplus.B5T"
            new ulong[]
            {
                0, 15661, 33959, 51330, 71973, 87582, 103305, 117691, 136167, 153418, 166932, 187113, 201441, 234030
            },

            // "cdr.B5T"
            new ulong[]
            {
                0, 42737
            },

            // "cdrom80mm.B5T"
            new ulong[]
            {
                0
            },

            // "cdrom.B5T"
            new ulong[]
            {
                0
            },

            // "cdrw.B5T"
            new ulong[]
            {
                0
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            /*
            new ulong[]
            {
                0
            },
            */
            // "cdi.B5T"
            new ulong[]
            {
                0
            },

            // "gdrom.B5T"
            new ulong[]
            {
                0, 450
            },

            // "jaguarcd.B5T"
            new ulong[]
            {
                0, 27490, 28237, 78892, 100054, 133203, 160908, 181466, 202024, 222582, 243140
            },

            // "mixed.B5T"
            new ulong[]
            {
                0, 16586, 40043, 51156, 88837, 116403, 149909, 188938, 214294, 243625, 259729
            },

            // "multitrack.B5T"
            new ulong[]
            {
                0, 48658, 207528
            },

            // "pcengine.B5T"
            new ulong[]
            {
                0, 3590, 38464, 47217, 53501, 61819, 68563, 75397, 83130, 86481, 91267, 99274, 106693, 112238, 120270,
                126229
            },

            // "pcfx.B5T"
            new ulong[]
            {
                0, 4395, 4909, 5941, 42059, 220645, 225646, 235498
            },

            // "videocd.B5T"
            new ulong[]
            {
                0, 2100, 12985, 20451, 39498, 47368, 56600, 67387, 71546, 77030, 80535, 95180, 110808, 115449, 118723,
                123266, 128328, 131637
            }
        };

        readonly ulong[][] _trackEnds =
        {
            // "audiocd_cdtext.B5T"
            new ulong[]
            {
                14709, 31606, 48486, 70119, 86709, 103484, 122539, 139934, 153989, 172669, 190524, 205464, 222186
            },

            // "cdg.B5T"
            new ulong[]
            {
                17376, 38524, 54935, 72859, 90754, 114545, 136450, 154772, 172149, 193297, 209708, 227632, 245527,
                269318, 291223, 309545
            },

            // "cdplus.B5T"
            new ulong[]
            {
                15660, 33958, 51329, 71972, 87581, 103304, 117690, 136166, 153417, 166931, 187112, 201440, 222779,
                303315
            },

            // "cdr.B5T"
            new ulong[]
            {
                31336, 97764
            },

            // "cdrom80mm.B5T"
            new ulong[]
            {
                19041
            },

            // "cdrom.B5T"
            new ulong[]
            {
                501
            },

            // "cdrw.B5T"
            new ulong[]
            {
                355499
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            /*
            new ulong[]
            {
            2287071
            },
            */
            // "cdi.B5T"
            new ulong[]
            {
                309833
            },

            // "gdrom.B5T"
            new ulong[]
            {
                449, 6399
            },

            // "jaguarcd.B5T"
            new ulong[]
            {
                16239, 28236, 78891, 100053, 133202, 160907, 181465, 202023, 222581, 243139, 243586
            },

            // "mixed.B5T"
            new ulong[]
            {
                16585, 40042, 51155, 88836, 116402, 149908, 188937, 214293, 243624, 259728, 283396
            },

            // "multitrack.B5T"
            new ulong[]
            {
                48657, 207527, 328359
            },

            // "pcengine.B5T"
            new ulong[]
            {
                3589, 38463, 47216, 53500, 61818, 68562, 75396, 83129, 86480, 91266, 99273, 106692, 112237, 120269,
                126228, 160955
            },

            // "pcfx.B5T"
            new ulong[]
            {
                4394, 4908, 5940, 42058, 220644, 225645, 235497, 246679
            },

            // "videocd.B5T"
            new ulong[]
            {
                2099, 12984, 20450, 39497, 47367, 56599, 67386, 71545, 77029, 80534, 95179, 110807, 115448, 118722,
                123265, 128327, 131636, 205071
            }
        };

        readonly ulong[][] _trackPregaps =
        {
            // "audiocd_cdtext.B5T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdg.B5T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdplus.B5T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 150
            },

            // "cdr.B5T"
            new ulong[]
            {
                150, 150
            },

            // "cdrom80mm.B5T"
            new ulong[]
            {
                150
            },

            // "cdrom.B5T"
            new ulong[]
            {
                150
            },

            // "cdrw.B5T"
            new ulong[]
            {
                150
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            /*
            new ulong[]
            {
            0
            },
            */
            // "cdi.B5T"
            new ulong[]
            {
                150
            },

            // "gdrom.B5T"
            new ulong[]
            {
                150, 0
            },

            // "jaguarcd.B5T"
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "mixed.B5T"
            new ulong[]
            {
                150, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "multitrack.B5T"
            new ulong[]
            {
                150, 0, 0
            },

            // "pcengine.B5T"
            new ulong[]
            {
                150, 0, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "pcfx.B5T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 150, 0, 0
            },

            // "videocd.B5T"
            new ulong[]
            {
                150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            }
        };

        readonly byte[][] _trackFlags =
        {
            // "audiocd_cdtext.B5T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdg.B5T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "cdplus.B5T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // "cdr.B5T"
            new byte[]
            {
                4, 4
            },

            // "cdrom80mm.B5T"
            new byte[]
            {
                4
            },

            // "cdrom.B5T"
            new byte[]
            {
                4
            },

            // "cdrw.B5T"
            new byte[]
            {
                4
            },

            // TODO: https://github.com/aaru-dps/Aaru/issues/430
            // "dvdrom.B5T"
            /*
            null,
            */
            // "cdi.B5T"
            new byte[]
            {
                4
            },

            // "gdrom.B5T"
            new byte[]
            {
                4, 0
            },

            // "jaguarcd.B5T"
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "mixed.B5T"
            new byte[]
            {
                4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            },

            // "multitrack.B5T"
            new byte[]
            {
                4, 4, 4
            },

            // "pcengine.B5T"
            new byte[]
            {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4
            },

            // "pcfx.B5T"
            new byte[]
            {
                0, 4, 4, 4, 4, 0, 0, 0
            },

            // "videocd.B5T"
            new byte[]
            {
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4
            }
        };

        [Test]
        public void Test()
        {
            // How many sectors to read at once
            const uint sectorsToRead = 256;

            Environment.CurrentDirectory = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "BlindWrite 5");

            IFilter[] filters = new IFilter[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                filters[i] = new ZZZNoFilter();
                filters[i].Open(_testFiles[i]);
            }

            IOpticalMediaImage[] images = new IOpticalMediaImage[_testFiles.Length];

            for(int i = 0; i < _testFiles.Length; i++)
            {
                images[i] = new DiscImages.BlindWrite5();
                Assert.AreEqual(true, images[i].Open(filters[i]), $"Open: {_testFiles[i]}");
            }

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_sectors[i], images[i].Info.Sectors, $"Sectors: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_mediaTypes[i], images[i].Info.MediaType, $"Media type: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                Assert.AreEqual(_tracks[i], images[i].Tracks.Count, $"Tracks: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackSessions[i].Should().
                                  BeEquivalentTo(images[i].Tracks.Select(t => t.TrackSession),
                                                 $"Track session: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackStarts[i].Should().BeEquivalentTo(images[i].Tracks.Select(t => t.TrackStartSector),
                                                        $"Track start: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackEnds[i].Should().
                              BeEquivalentTo(images[i].Tracks.Select(t => t.TrackEndSector),
                                             $"Track end: {_testFiles[i]}");

            for(int i = 0; i < _testFiles.Length; i++)
                _trackPregaps[i].Should().
                                 BeEquivalentTo(images[i].Tracks.Select(t => t.TrackPregap),
                                                $"Track pregap: {_testFiles[i]}");

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