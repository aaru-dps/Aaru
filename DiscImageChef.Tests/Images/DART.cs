// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DART.cs
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
    public class DART
    {
        readonly string[] testfiles =
        {
            "mf1dd_hfs_best.dart.lz", "mf1dd_hfs_fast.dart.lz", "mf1dd_mfs_best.dart.lz", "mf1dd_mfs_fast.dart.lz",
            "mf2dd_hfs_best.dart.lz", "mf2dd_hfs_fast.dart.lz", "mf2dd_mfs_best.dart.lz", "mf2dd_mfs_fast.dart.lz"
        };

        readonly ulong[] sectors = {800, 800, 800, 800, 1600, 1600, 1600, 1600};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512, 512};

        readonly MediaType[] mediatypes =
        {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS
        };

        readonly string[] md5s =
        {
            "eae3a95671d077deb702b3549a769f56", "eae3a95671d077deb702b3549a769f56", "c5d92544c3e78b7f0a9b4baaa9a64eec",
            "c5d92544c3e78b7f0a9b4baaa9a64eec", "a99744348a70b62b57bce2dec9132ced", "a99744348a70b62b57bce2dec9132ced",
            "93e71b9ecdb39d3ec9245b4f451856d4", "93e71b9ecdb39d3ec9245b4f451856d4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "images", "dart", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new DiscImages.D88();
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