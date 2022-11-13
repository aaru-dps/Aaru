// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CisCopy.cs
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

namespace Aaru.Tests.Images.CisCopy
{
    [TestFixture]
    public class Uncompressed : BlockMediaImageTest
    {
        // TODO: Support compression
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "CisCopy");
        public override IMediaImage _plugin => new DiscImages.CisCopy();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "md1dd8_all.dcf.lz",
                MediaType  = MediaType.DOS_525_SS_DD_8,
                Sectors    = 320,
                SectorSize = 512,
                MD5        = "95c0b76419c1c74db6dbe1d790f97dde",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 320
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md1dd8_belelung.dcf.lz",
                MediaType  = MediaType.DOS_525_SS_DD_8,
                Sectors    = 320,
                SectorSize = 512,
                MD5        = "95c0b76419c1c74db6dbe1d790f97dde",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 320
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md1dd8_fat.dcf.lz",
                MediaType  = MediaType.DOS_525_SS_DD_8,
                Sectors    = 320,
                SectorSize = 512,
                MD5        = "6f6507e416b7320d583dc347b8e57844",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 320
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md1dd_all.dcf.lz",
                MediaType  = MediaType.DOS_525_SS_DD_9,
                Sectors    = 360,
                SectorSize = 512,
                MD5        = "48b93e8619c4c13f4a3724b550e4b371",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 360
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md1dd_belelung.dcf.lz",
                MediaType  = MediaType.DOS_525_SS_DD_9,
                Sectors    = 360,
                SectorSize = 512,
                MD5        = "48b93e8619c4c13f4a3724b550e4b371",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 360
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md1dd_fat.dcf.lz",
                MediaType  = MediaType.DOS_525_SS_DD_9,
                Sectors    = 360,
                SectorSize = 512,
                MD5        = "1d060d2e2543e1c2e8569f5451660060",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 360
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2dd8_all.dcf.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "0c93155bbc5e412f5014e037d08c2745",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 640
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2dd8_belelung.dcf.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "0c93155bbc5e412f5014e037d08c2745",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 640
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2dd8_fat.dcf.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "0c93155bbc5e412f5014e037d08c2745",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 640
                    }
                }
            },

            // TODO: False positive CP/M filesystem
            new BlockImageTestExpected
            {
                TestFile   = "md2dd_all.dcf.lz",
                MediaType  = MediaType.DOS_525_DS_DD_9,
                Sectors    = 720,
                SectorSize = 512,
                MD5        = "d2a33090ec03bfb536e7356deacf4bbc"
            },

            // TODO: False positive CP/M filesystem
            new BlockImageTestExpected
            {
                TestFile   = "md2dd_belelung.dcf.lz",
                MediaType  = MediaType.DOS_525_DS_DD_9,
                Sectors    = 720,
                SectorSize = 512,
                MD5        = "d2a33090ec03bfb536e7356deacf4bbc"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2dd_fat.dcf.lz",
                MediaType  = MediaType.DOS_525_DS_DD_9,
                Sectors    = 720,
                SectorSize = 512,
                MD5        = "d2a33090ec03bfb536e7356deacf4bbc"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2hd_all.dcf.lz",
                MediaType  = MediaType.DOS_525_HD,
                Sectors    = 2400,
                SectorSize = 512,
                MD5        = "181f3bc62f0b90f74af9d8027ebf7512",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 2400
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2hd_belelung.dcf.lz",
                MediaType  = MediaType.DOS_525_HD,
                Sectors    = 2400,
                SectorSize = 512,
                MD5        = "181f3bc62f0b90f74af9d8027ebf7512",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 2400
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2hd_fat.dcf.lz",
                MediaType  = MediaType.DOS_525_HD,
                Sectors    = 2400,
                SectorSize = 512,
                MD5        = "181f3bc62f0b90f74af9d8027ebf7512",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 2400
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_all.dcf.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "783559ee5e774515d5e7d2feab9c333e",
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
                TestFile   = "mf2dd_belelung.dcf.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "783559ee5e774515d5e7d2feab9c333e",
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
                TestFile   = "mf2dd_fat.dcf.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "783559ee5e774515d5e7d2feab9c333e",
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
                TestFile   = "mf2hd_all.dcf.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "91f3fde8d56a536cdda4c6758e5dbc93",
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
                TestFile   = "mf2hd_belelung.dcf.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "91f3fde8d56a536cdda4c6758e5dbc93",
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
                TestFile   = "mf2hd_fat.dcf.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "91f3fde8d56a536cdda4c6758e5dbc93",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 2880
                    }
                }
            }
        };
    }
}