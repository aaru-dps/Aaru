// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DART.cs
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

namespace Aaru.Tests.Images;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

[TestFixture]
public class Dart : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "DART");
    public override IMediaImage Plugin    => new DiscImages.Dart();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_hfs_fast.dart.lz",
            MediaType  = MediaType.AppleSonySS,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "eae3a95671d077deb702b3549a769f56"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_mfs_fast.dart.lz",
            MediaType  = MediaType.AppleSonySS,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "c5d92544c3e78b7f0a9b4baaa9a64eec"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_hfs_fast.dart.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "a99744348a70b62b57bce2dec9132ced"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_mfs_fast.dart.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "93e71b9ecdb39d3ec9245b4f451856d4"
        }
        #region Unsupported LZH compression
        /*
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_hfs_best.dart.lz",
            MediaType  = MediaType.AppleSonySS,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "eae3a95671d077deb702b3549a769f56"
        },
       new BlockImageTestExpected
       {
           TestFile   = "mf1dd_mfs_best.dart.lz",
           MediaType  = MediaType.AppleSonySS,
           Sectors    = 800,
           SectorSize = 512,
           MD5        = "c5d92544c3e78b7f0a9b4baaa9a64eec"
       },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_hfs_best.dart.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "a99744348a70b62b57bce2dec9132ced"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_mfs_best.dart.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "93e71b9ecdb39d3ec9245b4f451856d4"
        },
        */
        #endregion Unsupported LZH compression
    };
}