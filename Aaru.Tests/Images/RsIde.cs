// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RsIde.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class RsIde : BlockMediaImageTest
    {
        public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "RS-IDE");
        public override IMediaImage _plugin    => new DiscImages.RsIde();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "divide.hdf.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 20480,
                SectorSize = 512,
                MD5        = "ee7b8fe07784f2ebacc18da1fc248f5a",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 63,
                        Length = 20417
                    }
                }
            }
        };
    }
}