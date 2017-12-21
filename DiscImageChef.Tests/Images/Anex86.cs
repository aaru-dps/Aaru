// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Anex86.cs
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Images
{
    [TestFixture]
    public class Anex86
    {
        readonly string[] testfiles =
        {
            "anex86_10mb.hdi.lz", "anex86_15mb.hdi.lz", "anex86_20mb.hdi.lz", "anex86_30mb.hdi.lz",
            "anex86_40mb.hdi.lz", "anex86_5mb.hdi.lz", "blank_md2hd.fdi.lz", "msdos33d_md2hd.fdi.lz",
            "msdos50_epson_md2hd.fdi.lz", "msdos50_md2hd.fdi.lz", "msdos62_md2hd.fdi.lz"
        };

        readonly ulong[] sectors = {40920, 61380, 81840, 121770, 162360, 20196, 1232, 1232, 1232, 1232, 1232};

        readonly uint[] sectorsize = {256, 256, 256, 256, 256, 256, 1024, 1024, 1024, 1024, 1024};

        readonly MediaType[] mediatypes =
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.NEC_525_HD, MediaType.NEC_525_HD,
            MediaType.NEC_525_HD, MediaType.NEC_525_HD, MediaType.NEC_525_HD
        };

        readonly string[] md5s =
        {
            "1c5387e38e58165c517c059e5d48905d", "a84366658c1c3bd09af4d0d42fbf716e", "919c9eecf1b65b10870f617cb976668a",
            "02d35af02581afb2e56792dcaba2c1af", "b8c3f858f1a9d300d3e74f36eea04354", "c348bbbaf99fcb8c8e66de157aef62f4",
            "c3587f7020743067cf948c9d5c5edb27", "a23874a4474334b035a24c6924140744", "bc1ef3236e75cb09575037b884ee9dce",
            "243036c4617b666a6c886cc23d7274e0", "09bb2ff964a0c5c223a1900f085e3955"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "images", "anex86", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new DiscImages.Anex86();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.MediaType, testfiles[i]);

                // How many sectors to read at once
                const uint sectorsToRead = 256;
                ulong doneSectors = 0;

                Md5Context ctx = new Md5Context();
                ctx.Init();

                while(doneSectors < image.ImageInfo.Sectors)
                {
                    byte[] sector;

                    if(image.ImageInfo.Sectors - doneSectors >= sectorsToRead)
                    {
                        sector = image.ReadSectors(doneSectors, sectorsToRead);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector = image.ReadSectors(doneSectors, (uint)(image.ImageInfo.Sectors - doneSectors));
                        doneSectors += image.ImageInfo.Sectors - doneSectors;
                    }

                    ctx.Update(sector);
                }

                Assert.AreEqual(md5s[i], ctx.End(), testfiles[i]);
            }
        }
    }
}