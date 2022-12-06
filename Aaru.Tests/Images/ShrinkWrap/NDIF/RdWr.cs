// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RdWr.cs
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

namespace Aaru.Tests.Images.ShrinkWrap.NDIF
{
    [TestFixture]
    public class RdWr : BlockMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "ShrinkWrap 3",
                                                          "NDIF", "No compression", "No encryption");
        public override IMediaImage _plugin => new Ndif();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "CDROM.img",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 91108,
                SectorSize = 512,
                MD5        = "69e3234920e472b24365060241934ca6"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DC6_RW_HFS_1440.img",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "3160038ca028ccf52ad7863790072145"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DC6_RW_HFS_800.img",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "5e255c4bc0f6a26ecd27845b37e65aaa"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DC6_RW_HFS_DMF.img",
                MediaType  = MediaType.DMF,
                Sectors    = 3360,
                SectorSize = 512,
                MD5        = "652dc979c177f2d8e846587158b38478"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DOS1440.img",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "ff419213080574056ebd9adf7bab3d32",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 2880
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "DOS720.img",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "c2be571406cf6353269faa59a4a8c0a4",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 1440
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "DOSDMF.img",
                MediaType  = MediaType.DMF,
                Sectors    = 3360,
                SectorSize = 512,
                MD5        = "92ea7a359957012a682ba126cfdef0ce",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 3360
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "PD1440.img",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "7975e8cf7579a6848d6fb4e546d1f682"
            },
            new BlockImageTestExpected
            {
                TestFile   = "PD800.img",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "a72da7aedadbe194c22a3d71c62e4766"
            },
            new BlockImageTestExpected
            {
                TestFile   = "PDDMF.img",
                MediaType  = MediaType.DMF,
                Sectors    = 3360,
                SectorSize = 512,
                MD5        = "7fbf0251a93cb36d98e68b7d19624de5"
            }
        };
    }
}