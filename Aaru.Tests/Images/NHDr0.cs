// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NHDr0.cs
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class NHDr0 : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "T-98 Next");
    public override IMediaImage Plugin     => new Nhdr0();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "t98n_128.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 261120,
            SectorSize = 512,
            Md5        = "af7c3cfa315b6661300017f865bf26d6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "t98n_20.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 40800,
            SectorSize = 512,
            Md5        = "bcb390d0b4d12feac29dbadc1a623c99"
        },
        new BlockImageTestExpected
        {
            TestFile   = "t98n_256.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 522240,
            SectorSize = 512,
            Md5        = "e50e78b3742f5f89dd1a5573ba3141c4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "t98n_41.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 83640,
            SectorSize = 512,
            Md5        = "007acca6fb53f90728d78f7c40c2b094"
        },
        new BlockImageTestExpected
        {
            TestFile   = "t98n_512.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 1044480,
            SectorSize = 512,
            Md5        = "42d1cb6fc2a9df39ecd53002edd978d6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "t98n_65.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 132600,
            SectorSize = 512,
            Md5        = "b53f5b406234663de6c2bdffac88322d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "t98n_80.nhd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 163200,
            SectorSize = 512,
            Md5        = "fe9ecc6f0b5beb9635a1595155941925"
        }
    };
}