// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ProDOS.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images.AppleDOS;

[TestFixture]
public class ProDOS : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "Apple ProDOS Order");

    public override IMediaImage Plugin => new AppleDos();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "dos33.po.lz",
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
            TestFile   = "hfs1440.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "2c0b397aa3fe23a52cf7908340739f78"
        },
        new BlockImageTestExpected
        {
            TestFile   = "hfs.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "ddd04ef378552c789f85382b4f49da06"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal800.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "5158e2fe9d8e7ae1f7db73156478e4f4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos1440.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "55ff5838139c0e8fa3f904397dc22fa5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos5mb.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "137463bc1f758fb8f2c354b02603817b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos800.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "193c5cc22f07e5aeb96eb187cb59c2d9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodosmod.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "26d9c57e262f61c4eb6c150eefafe4c0"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos.po.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "11ef56c80c94347d2e3f921d5c36c8de"
        }
    };
}