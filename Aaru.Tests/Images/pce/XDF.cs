// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XDF.cs
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.pce;

[TestFixture, SuppressMessage("ReSharper", "InconsistentNaming")]
public class XDF : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "pce", "XDF");
    public override IMediaImage Plugin     => new ZZZRawImage();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_teledisk.xdf.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "90e8f5022bff8fa90c5148ec35f5d64c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.xdf.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "825ca9cdcb2f35ff8bbbda9cb0a27c4d"
        }
    };
}