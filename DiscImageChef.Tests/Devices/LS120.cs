// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LS120.cs
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
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Devices
{
    [TestFixture]
    public class Ls120
    {
        readonly string[] testfiles = {"ls120.bin.lz", "mf2dd.bin.lz", "mf2hd.bin.lz"};

        readonly MediaType[] mediatypes = {MediaType.LS120, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {246528, 1440, 2880};

        readonly uint[] sectorsize = {512, 512, 512};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "devices", "ls120", testfiles[i]);
                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
            }
        }
    }
}