// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : V1.cs
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
using Aaru.CommonTypes.Structs;
using NUnit.Framework;

namespace Aaru.Tests.Images.AaruFormat.Tape
{
    [TestFixture]
    public class V1 : TapeMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "AaruFormat", "V1");
        public override IMediaImage _plugin => new DiscImages.AaruFormat();

        public override TapeImageTestExpected[] Tests => new[]
        {
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Boot Tape).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 1604,
                SectorSize = 10240,
                MD5        = "a6334d975523b3422fea522b0cc118a9",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 1603,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 1603,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 15485,
                SectorSize = 512,
                MD5        = "17ef78d9e5c53b976f530d4ca44223fd",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 15484,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 15484,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 15,
                SectorSize = 32256,
                MD5        = "76c0ae10f4ec70ef8681b212f02a71c8",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 14,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 14,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Operating System).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 3298,
                SectorSize = 32256,
                MD5        = "e331c9d0ae7c25c81c6580bc9965e2d0",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 3297,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 3297,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Optional Packages).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 3152,
                SectorSize = 32256,
                MD5        = "018c37c40f8df91ab9b098d643c9ae6c",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 3151,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 3151,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 818,
                SectorSize = 32256,
                MD5        = "eb3ce36b2c3afeeec59e5b8ed802a393",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 817,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 817,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 7,
                SectorSize = 32256,
                MD5        = "b057656698a224187afb2bdbb8caf7f3",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 6,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 6,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "Nonstop-UX System V Release 4 B32 (Required Packages).aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 684,
                SectorSize = 32256,
                MD5        = "8e48e388e7094f3170065718ab618b53",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 683,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 683,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "OpenWindows.3.0.exabyte.aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 73525,
                SectorSize = 1024,
                MD5        = "8861f8c06a2e93ca5a81d729ad3e1de1",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 73524,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 0,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 1,
                        FirstBlock = 1,
                        LastBlock  = 164,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 2,
                        FirstBlock = 165,
                        LastBlock  = 2412,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 3,
                        FirstBlock = 2413,
                        LastBlock  = 5612,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 4,
                        FirstBlock = 5613,
                        LastBlock  = 73524,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "OpenWindows.3.0.Q150.aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 290,
                SectorSize = 262144,
                MD5        = "bfc402b23af0cf1ad22d9fb2ea29b58f",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 289,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 0,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 1,
                        FirstBlock = 1,
                        LastBlock  = 1,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 2,
                        FirstBlock = 2,
                        LastBlock  = 10,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 3,
                        FirstBlock = 11,
                        LastBlock  = 23,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 4,
                        FirstBlock = 24,
                        LastBlock  = 289,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "OS.MP.4.1C.exabyte.aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 37587,
                SectorSize = 8192,
                MD5        = "e4a3e2fe26c72ca025ac0c017ec73ee9",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 37586,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 1,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 1,
                        FirstBlock = 2,
                        LastBlock  = 3,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 2,
                        FirstBlock = 4,
                        LastBlock  = 6860,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 3,
                        FirstBlock = 6861,
                        LastBlock  = 13773,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 4,
                        FirstBlock = 13774,
                        LastBlock  = 20263,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 5,
                        FirstBlock = 20264,
                        LastBlock  = 20299,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 6,
                        FirstBlock = 20300,
                        LastBlock  = 22603,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 7,
                        FirstBlock = 22604,
                        LastBlock  = 23472,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 8,
                        FirstBlock = 23473,
                        LastBlock  = 24946,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 9,
                        FirstBlock = 24947,
                        LastBlock  = 26436,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 10,
                        FirstBlock = 26437,
                        LastBlock  = 27720,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 11,
                        FirstBlock = 27721,
                        LastBlock  = 31922,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 12,
                        FirstBlock = 31923,
                        LastBlock  = 32283,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 13,
                        FirstBlock = 32284,
                        LastBlock  = 32675,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 14,
                        FirstBlock = 32676,
                        LastBlock  = 33549,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 15,
                        FirstBlock = 33550,
                        LastBlock  = 33686,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 16,
                        FirstBlock = 33687,
                        LastBlock  = 33909,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 17,
                        FirstBlock = 33910,
                        LastBlock  = 33949,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 18,
                        FirstBlock = 33950,
                        LastBlock  = 34180,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 19,
                        FirstBlock = 34181,
                        LastBlock  = 34573,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 20,
                        FirstBlock = 34574,
                        LastBlock  = 35072,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 21,
                        FirstBlock = 35073,
                        LastBlock  = 35163,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 22,
                        FirstBlock = 35164,
                        LastBlock  = 35908,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 23,
                        FirstBlock = 35909,
                        LastBlock  = 35984,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 24,
                        FirstBlock = 35985,
                        LastBlock  = 36098,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 25,
                        FirstBlock = 36099,
                        LastBlock  = 36270,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 26,
                        FirstBlock = 36271,
                        LastBlock  = 36276,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 27,
                        FirstBlock = 36277,
                        LastBlock  = 36647,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 28,
                        FirstBlock = 36648,
                        LastBlock  = 37111,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 29,
                        FirstBlock = 37112,
                        LastBlock  = 37583,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 30,
                        FirstBlock = 37584,
                        LastBlock  = 37584,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 31,
                        FirstBlock = 37585,
                        LastBlock  = 37585,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 32,
                        FirstBlock = 37586,
                        LastBlock  = 37586,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "X.3.0.exabyte.aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 25046,
                SectorSize = 1024,
                MD5        = "e625c03d7493dc22fe49f91f731446e8",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 25045,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 0,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 1,
                        FirstBlock = 1,
                        LastBlock  = 61,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 2,
                        FirstBlock = 62,
                        LastBlock  = 149,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 3,
                        FirstBlock = 150,
                        LastBlock  = 2781,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 4,
                        FirstBlock = 2782,
                        LastBlock  = 11885,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 5,
                        FirstBlock = 11886,
                        LastBlock  = 25045,
                        Partition  = 0
                    }
                }
            },
            new TapeImageTestExpected
            {
                TestFile   = "X.3.Q150.aif",
                MediaType  = MediaType.UnknownTape,
                Sectors    = 102,
                SectorSize = 262144,
                MD5        = "198464b1daf8e674debf8eda0fcbf016",
                Partitions = new[]
                {
                    new TapePartition
                    {
                        FirstBlock = 0,
                        LastBlock  = 101,
                        Number     = 0
                    }
                },
                Files = new[]
                {
                    new TapeFile
                    {
                        File       = 0,
                        FirstBlock = 0,
                        LastBlock  = 0,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 1,
                        FirstBlock = 1,
                        LastBlock  = 1,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 2,
                        FirstBlock = 2,
                        LastBlock  = 2,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 3,
                        FirstBlock = 3,
                        LastBlock  = 13,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 4,
                        FirstBlock = 14,
                        LastBlock  = 49,
                        Partition  = 0
                    },
                    new TapeFile
                    {
                        File       = 5,
                        FirstBlock = 50,
                        LastBlock  = 101,
                        Partition  = 0
                    }
                }
            }
        };
    }
}