// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDRO.cs
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

namespace Aaru.Tests.Images.DiskCopy65;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

[TestFixture]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class UDRO : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskCopy 6.5", "UDIF", "UDRO");
    public override IMediaImage Plugin => new Udif();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_DOS_1440.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 2884,
            SectorSize = 512,
            Md5        = "4306922864c6cf40a419fd5876b5879d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 4,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_DOS_720.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 1444,
            SectorSize = 512,
            Md5        = "a885825f28929a5626e71201b37ed96e"
            /* TODO: NullReferenceException when getting cluster of last depth folder
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 4,
                    Length = 1440
                }
            }
            */
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_DOS_DMF.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 3364,
            SectorSize = 512,
            Md5        = "180a0db19ecfe9d55d068c6460f028be"
            /* TODO: NullReferenceException when getting cluster of last depth folder
            Partitions = new[]
            {
            new BlockPartitionVolumes
            {
            Start  = 4,
            Length = 3360
            }
            }
            */
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_HFS_1440.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 2884,
            SectorSize = 512,
            Md5        = "e307949819edeecd5e855b661a3bfba3"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_HFS_800.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 1604,
            SectorSize = 512,
            Md5        = "c5a5ad78997ddc30f1dc768112f52609"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_HFS_DMF.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 3364,
            SectorSize = 512,
            Md5        = "5fd35f80791be6eaa44195875aa0465a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_PD_1440.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 2884,
            SectorSize = 512,
            Md5        = "1c336199896d1f9bff9b2d5c49b48b63"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_PD_800.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 1604,
            SectorSize = 512,
            Md5        = "d654f84668c671e801f4aa107e0aee92"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DC6_UDRO_PD_DMF.dmg",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 3364,
            SectorSize = 512,
            Md5        = "e7b1de07a1f402e4663c3dee4fd3d6fe"
        }
    };
}