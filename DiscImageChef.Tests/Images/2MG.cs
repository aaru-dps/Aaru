// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : 2MG.cs
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
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Images
{
    [TestFixture]
    public class Apple2Mg
    {
        readonly string[] testfiles =
        {
            "blank140.2mg.lz", "dos32.2mg.lz", "dos33-do.2mg.lz", "dos33-nib.2mg.lz", "dos33-po.2mg.lz",
            "prodos1440.2mg.lz"
        };

        readonly ulong[] sectors = {560, 415, 560, 560, 560, 2880};

        readonly uint[] sectorsize = {256, 256, 256, 256, 256, 512};

        readonly MediaType[] mediatypes =
        {
            MediaType.Apple33SS, MediaType.Apple32SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS,
            MediaType.DOS_35_HD
        };

        readonly string[] md5S =
        {
            "7db5d585270ab858043d50e60068d45f", "c62ee4dac8835f9acc999e7531d3b5f8", "86bc7879bb477917aab3976057a6a7b2",
            "32b8f8163238d4f3ea2a3db95ad1a812", "a5acba4cbaddd9c53ea0e8210f4aba55", "eb9b60c78b30d2b6541ed0781944b6da"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "images", "2mg", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new DiscImages.Apple2Mg();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);

                // How many sectors to read at once
                const uint SECTORS_TO_READ = 256;
                ulong      doneSectors     = 0;

                Md5Context ctx = new Md5Context();

                while(doneSectors < image.Info.Sectors)
                {
                    byte[] sector;

                    if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        sector      =  image.ReadSectors(doneSectors, SECTORS_TO_READ);
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                        doneSectors += image.Info.Sectors - doneSectors;
                    }

                    ctx.Update(sector);
                }

                Assert.AreEqual(md5S[i], ctx.End(), testfiles[i]);
            }
        }
    }
}