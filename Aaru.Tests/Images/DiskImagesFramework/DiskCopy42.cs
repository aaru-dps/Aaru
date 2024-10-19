// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Images.DiskImagesFramework;

[TestFixture]
public class DiskCopy42 : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "DiskImagesFramework", "DiskCopy 4.2");

    public override IMediaImage Plugin => new Aaru.Images.DiskCopy42();

    public override BlockImageTestExpected[] Tests =>
    [
        new BlockImageTestExpected
        {
            TestFile   = "DOS_1440.img.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "ff419213080574056ebd9adf7bab3d32",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "DOS_720.img.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "c2be571406cf6353269faa59a4a8c0a4",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1440
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "HFS_1440.img.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "3160038ca028ccf52ad7863790072145"
        },
        new BlockImageTestExpected
        {
            TestFile   = "HFS_800.img.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "5e255c4bc0f6a26ecd27845b37e65aaa"
        },
        new BlockImageTestExpected
        {
            TestFile   = "ProDOS_1440.img.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "7975e8cf7579a6848d6fb4e546d1f682"
        },
        new BlockImageTestExpected
        {
            TestFile   = "ProDOS_800.img.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "a72da7aedadbe194c22a3d71c62e4766"
        },
        new BlockImageTestExpected
        {
            TestFile   = "UFS_1440.img.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "b37823c7a90d1917f719ba5927b23da8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "UFS_720.img.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "4942032f7bf1d115237ea1764424828b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "UFS_800.img.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        }
    ];
}