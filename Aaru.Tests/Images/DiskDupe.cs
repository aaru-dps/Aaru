// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskDupe.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
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
// Copyright © 2021 Michael Drüing
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class DiskDupe : BlockMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskDupe");
        public override IMediaImage _plugin => new DiscImages.DiskDupe();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "1.DDI.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "0d5735269cb9c3d0e63ec9ccfb38e4e2"
            },
            new BlockImageTestExpected
            {
                TestFile   = "2.DDI.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "fa639b4bd96d2fb7be33a1725e9c7c4f"
            },
            new BlockImageTestExpected
            {
                TestFile   = "3.DDI.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "f63e676310b2f1a9e44e9a471c7cf1f2"
            },
        };
    }
}
