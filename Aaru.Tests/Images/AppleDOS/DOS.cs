// /***************************************************************************
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
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images.AppleDOS;

[TestFixture]
public class DOS : BlockMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "Apple DOS Order");
    public override IMediaImage Plugin => new AppleDos();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "dos33.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "0ffcbd4180306192726926b43755db2f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "hfs.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "ddd04ef378552c789f85382b4f49da06"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal800.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "5158e2fe9d8e7ae1f7db73156478e4f4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos800.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "193c5cc22f07e5aeb96eb187cb59c2d9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "23f42e529c9fde2a8033f1bc6a7bca93"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodosmod.do.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "a7ec980472c320da5ea6f2f0aec0f502"
        }
    };
}