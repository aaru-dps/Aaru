// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Anex86.cs
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

namespace Aaru.Tests.Images;

[TestFixture]
public class Anex86 : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "Anex86");
    public override IMediaImage Plugin     => new DiscImages.Anex86();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "anex86_10mb.hdi.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 40920,
            SectorSize = 256,
            Md5        = "1c5387e38e58165c517c059e5d48905d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "anex86_15mb.hdi.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 61380,
            SectorSize = 256,
            Md5        = "a84366658c1c3bd09af4d0d42fbf716e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "anex86_20mb.hdi.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 81840,
            SectorSize = 256,
            Md5        = "919c9eecf1b65b10870f617cb976668a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "anex86_30mb.hdi.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 121770,
            SectorSize = 256,
            Md5        = "02d35af02581afb2e56792dcaba2c1af"
        },
        new BlockImageTestExpected
        {
            TestFile   = "anex86_40mb.hdi.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 162360,
            SectorSize = 256,
            Md5        = "b8c3f858f1a9d300d3e74f36eea04354"
        },
        new BlockImageTestExpected
        {
            TestFile   = "anex86_5mb.hdi.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 20196,
            SectorSize = 256,
            Md5        = "c348bbbaf99fcb8c8e66de157aef62f4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "blank_md2hd.fdi.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "c3587f7020743067cf948c9d5c5edb27"
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos33d_md2hd.fdi.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "a23874a4474334b035a24c6924140744"
            /* TODO: NullReferenceException
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1232
                }
            }
            */
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos50_epson_md2hd.fdi.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "bc1ef3236e75cb09575037b884ee9dce",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1232
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos50_md2hd.fdi.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "243036c4617b666a6c886cc23d7274e0",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1232
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos62_md2hd.fdi.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "09bb2ff964a0c5c223a1900f085e3955",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1232
                }
            }
        }
    };
}