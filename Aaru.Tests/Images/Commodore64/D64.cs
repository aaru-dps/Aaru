// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : D64.cs
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

namespace Aaru.Tests.Images.Commodore64;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

[TestFixture]
public class D64 : BlockMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "Commodore D64");
    public override IMediaImage Plugin => new ZZZRawImage();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "0 Grad Nord (1985)(Ariolasoft)(de)(Side A).d64.lz",
            MediaType  = MediaType.CBM_1540,
            Sectors    = 683,
            SectorSize = 256,
            Md5        = "9205f7ccdb19cfbdb0b46d10e40b2a90"
        }
    };
}