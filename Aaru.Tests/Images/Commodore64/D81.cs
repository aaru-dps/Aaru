﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DOS.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.Commodore64
{
    [TestFixture]
    public class D81 : BlockMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Media image formats", "Commodore D81");
        public override IMediaImage _plugin => new ZZZRawImage();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "Strategiegames #01 (19xx)(-).d81.lz",
                MediaType  = MediaType.CBM_35_DD,
                Sectors    = 3200,
                SectorSize = 256,
                MD5        = "e84d86b63e798747c42b27b58ab88665"
            }
        };
    }
}