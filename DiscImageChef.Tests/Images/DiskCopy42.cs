// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
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
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using NUnit.Framework;
using DiscImageChef.Checksums;

namespace DiscImageChef.Tests.Images
{
    [TestFixture]
    public class DiskCopy42
    {
        readonly string[] testfiles = {
            // Made with DiskCopy 4.2
            "dc42/mf1dd_hfs.img.lz", "dc42/mf1dd_mfs.img.lz", "dc42/mf2dd_hfs.img.lz", "dc42/mf2dd_mfs.img.lz",
            // Made with ShrinkWrap 3
            "shrinkwrap/DiskCopy 4/DC6_RW_HFS_1440.image.lz", "shrinkwrap/DiskCopy 4/DC6_RW_HFS_800.image.lz", "shrinkwrap/DiskCopy 4/DOS1440.image.lz", "shrinkwrap/DiskCopy 4/DOS720.image.lz",
            "shrinkwrap/DiskCopy 4/PD1440.image.lz", "shrinkwrap/DiskCopy 4/PD800.image.lz",
            // Made with DiskImages.framework
            "macosx/DC42/DOS_1440.img.lz", "macosx/DC42/DOS_720.img.lz", "macosx/DC42/HFS_1440.img.lz", "macosx/DC42/HFS_800.img.lz",
            "macosx/DC42/ProDOS_1440.img.lz", "macosx/DC42/ProDOS_800.img.lz", "macosx/DC42/UFS_1440.img.lz", "macosx/DC42/UFS_720.img.lz",
            "macosx/DC42/UFS_800.img.lz",
        };

        readonly ulong[] sectors = {
            800, 800, 1600, 1600,
            2880, 1600, 2880, 1440,
            2880, 1600,
            2880, 1440, 2880, 1600,
            2880, 1600, 2880, 1440,
            1600,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512,
        };

        readonly MediaType[] mediatypes = {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonyDS, MediaType.AppleSonyDS,
            MediaType.DOS_35_HD, MediaType.AppleSonyDS, MediaType.DOS_35_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.AppleSonyDS,
            MediaType.DOS_35_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD, MediaType.AppleSonyDS,
            MediaType.DOS_35_HD, MediaType.AppleSonyDS, MediaType.DOS_35_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.AppleSonyDS,
        };

        readonly string[] md5s = {
            "eae3a95671d077deb702b3549a769f56", "c5d92544c3e78b7f0a9b4baaa9a64eec", "a99744348a70b62b57bce2dec9132ced", "93e71b9ecdb39d3ec9245b4f451856d4",
            "3160038ca028ccf52ad7863790072145", "5e255c4bc0f6a26ecd27845b37e65aaa", "ff419213080574056ebd9adf7bab3d32", "c2be571406cf6353269faa59a4a8c0a4",
            "7975e8cf7579a6848d6fb4e546d1f682", "a72da7aedadbe194c22a3d71c62e4766",
            "ff419213080574056ebd9adf7bab3d32", "c2be571406cf6353269faa59a4a8c0a4", "3160038ca028ccf52ad7863790072145", "5e255c4bc0f6a26ecd27845b37e65aaa",
            "7975e8cf7579a6848d6fb4e546d1f682", "a72da7aedadbe194c22a3d71c62e4766", "b37823c7a90d1917f719ba5927b23da8", "4942032f7bf1d115237ea1764424828b",
            "85574aebeef03eb355bf8541955d06ea",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "images", "", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new DiscImageChef.ImagePlugins.DiskCopy42();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.mediaType, testfiles[i]);

                // How many sectors to read at once
                const uint sectorsToRead = 256;
                ulong doneSectors = 0;

                MD5Context ctx = new MD5Context();
                ctx.Init();

                while(doneSectors < image.ImageInfo.sectors)
                {
                    byte[] sector;

                    if((image.ImageInfo.sectors - doneSectors) >= sectorsToRead)
                    {
                        sector = image.ReadSectors(doneSectors, sectorsToRead);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector = image.ReadSectors(doneSectors, (uint)(image.ImageInfo.sectors - doneSectors));
                        doneSectors += (image.ImageInfo.sectors - doneSectors);
                    }

                    ctx.Update(sector);
                }

                Assert.AreEqual(md5s[i], ctx.End(), testfiles[i]);
            }
        }
    }
}
