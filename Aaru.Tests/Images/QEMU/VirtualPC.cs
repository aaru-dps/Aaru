// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VirtualPC.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.QEMU
{
    [TestFixture]
    public class VirtualPC : BlockMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "QEMU", "VirtualPC");
        public override IMediaImage _plugin => new Vhd();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "qemu_dynamic_250mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 512064,
                SectorSize = 512,
                MD5        = "0435d6781d14d34a32c6ac40f5e70d35",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 512064
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "qemu_fixed_10mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 20536,
                SectorSize = 512,
                MD5        = "adfad4fb019f157e868baa39e7753db7",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 20536
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "virtualpc.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 251940,
                SectorSize = 512,
                MD5        = "6246bff640cb3a56d2611e7f8616384d",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 63,
                        Length = 251841
                    }
                }
            }
        };
    }
}