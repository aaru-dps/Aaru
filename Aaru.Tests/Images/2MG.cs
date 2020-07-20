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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Apple2Mg
    {
        readonly string[] _testfiles =
        {
            "blank140.2mg.lz", "dos32.2mg.lz", "dos33-do.2mg.lz", "dos33-nib.2mg.lz", "dos33-po.2mg.lz",
            "prodos1440.2mg.lz"
        };

        readonly ulong[] _sectors =
        {
            560, 455, 560, 560, 560, 2880
        };

        readonly uint[] _sectorsize =
        {
            256, 256, 256, 256, 256, 512
        };

        readonly MediaType[] _mediatypes =
        {
            MediaType.Apple33SS, MediaType.Apple32SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS,
            MediaType.DOS_35_HD
        };

        readonly string[] _md5S =
        {
            "7db5d585270ab858043d50e60068d45f", "906c1bdbf76bf089ea47aae98151df5d", "91d020725d081500caa1fd8aad959397",
            "91d020725d081500caa1fd8aad959397", "91d020725d081500caa1fd8aad959397", "eb9b60c78b30d2b6541ed0781944b6da"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "2mg", _testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new DiscImages.Apple2Mg();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                Assert.AreEqual(_mediatypes[i], image.Info.MediaType, _testfiles[i]);

                // How many sectors to read at once
                const uint sectorsToRead = 256;
                ulong      doneSectors   = 0;

                var ctx = new Md5Context();

                while(doneSectors < image.Info.Sectors)
                {
                    byte[] sector;

                    if(image.Info.Sectors - doneSectors >= sectorsToRead)
                    {
                        sector      =  image.ReadSectors(doneSectors, sectorsToRead);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                        doneSectors += image.Info.Sectors - doneSectors;
                    }

                    ctx.Update(sector);
                }

                Assert.AreEqual(_md5S[i], ctx.End(), _testfiles[i]);
            }
        }
    }
}