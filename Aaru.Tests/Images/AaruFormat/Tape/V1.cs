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
// Copyright © 2011-2021 Natalia Portillo
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
        public override string[] _testFiles => new[]
        {
            "Nonstop-UX System V Release 4 B32 (Boot Tape).aif",
            "Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif",
            "Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif",
            "Nonstop-UX System V Release 4 B32 (Operating System).aif",
            "Nonstop-UX System V Release 4 B32 (Optional Packages).aif",
            "Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif",
            "Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif",
            "Nonstop-UX System V Release 4 B32 (Required Packages).aif", "OpenWindows.3.0.exabyte.aif",
            "OpenWindows.3.0.Q150.aif", "OS.MP.4.1C.exabyte.aif", "X.3.0.exabyte.aif", "X.3.Q150.aif"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            1604,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            15485,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            15,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            3298,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            3152,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            818,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            7,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            684,

            // OpenWindows.3.0.exabyte.aif
            73525,

            // OpenWindows.3.0.Q150.aif
            290,

            // OS.MP.4.1C.exabyte.aif
            37587,

            // X.3.0.exabyte.aif
            25046,

            // X.3.Q150.aif
            102
        };

        public override uint[] _sectorSize => new uint[]
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            10240,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            512,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            28637,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            32256,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            32256,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            32256,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            26185,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            32256,

            // OpenWindows.3.0.exabyte.aif
            1024,

            // OpenWindows.3.0.Q150.aif
            262144,

            // OS.MP.4.1C.exabyte.aif
            8192,

            // X.3.0.exabyte.aif
            1024,

            // X.3.Q150.aif
            258048
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            MediaType.UnknownTape,

            // OpenWindows.3.0.exabyte.aif
            MediaType.UnknownTape,

            // OpenWindows.3.0.Q150.aif
            MediaType.UnknownTape,

            // OS.MP.4.1C.exabyte.aif
            MediaType.UnknownTape,

            // X.3.0.exabyte.aif
            MediaType.UnknownTape,

            // X.3.Q150.aif
            MediaType.UnknownTape
        };

        public override string[] _md5S => new[]
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            "a6334d975523b3422fea522b0cc118a9",

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            "17ef78d9e5c53b976f530d4ca44223fd",

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            "6b6e80c4b3a48b2bc46571389eeaf78b",

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            "91b6115a718b9854b69478fee8e8644e",

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            "018c37c40f8df91ab9b098d643c9ae6c",

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            "181c9b00c236d14c7dfa4fa009c4559d",

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            "7dc46bb181077d215a5c93cc990da365",

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            "80e1d90052bf8c2df641398d0a30e630",

            // OpenWindows.3.0.exabyte.aif
            "8861f8c06a2e93ca5a81d729ad3e1de1",

            // OpenWindows.3.0.Q150.aif
            "2b944c7a353a63a48fdcf5517306fba6",

            // OS.MP.4.1C.exabyte.aif
            "a923a4fffb3456386bafd00c1d939224",

            // X.3.0.exabyte.aif
            "e625c03d7493dc22fe49f91f731446e8",

            // X.3.Q150.aif
            "198464b1daf8e674debf8eda0fcbf016"
        };

        readonly bool[] _isTape =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            true,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            true,

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            true,

            // OpenWindows.3.0.exabyte.aif
            true,

            // OpenWindows.3.0.Q150.aif
            true,

            // OS.MP.4.1C.exabyte.aif
            true,

            // X.3.0.exabyte.aif
            true,

            // X.3.Q150.aif
            true
        };

        public override TapeFile[][] _tapeFiles => new[]
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 1603,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 15484,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 14,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 3297,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 3151,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 817,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 6,
                    Partition  = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            new[]
            {
                new TapeFile
                {
                    File       = 0,
                    FirstBlock = 0,
                    LastBlock  = 683,
                    Partition  = 0
                }
            },

            // OpenWindows.3.0.exabyte.aif
            new[]
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
            },

            // OpenWindows.3.0.Q150.aif
            new[]
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
            },

            // OS.MP.4.1C.exabyte.aif
            new[]
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
            },

            // X.3.0.exabyte.aif
            new[]
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
            },

            // X.3.Q150.aif
            new[]
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
        };

        public override TapePartition[][] _tapePartitions => new[]
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 1603,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 15484,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 14,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Operating System).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 3297,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Optional Packages).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 3151,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 817,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 6,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Required Packages).aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 683,
                    Number     = 0
                }
            },

            // OpenWindows.3.0.exabyte.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 73524,
                    Number     = 0
                }
            },

            // OpenWindows.3.0.Q150.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 289,
                    Number     = 0
                }
            },

            // OS.MP.4.1C.exabyte.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 37586,
                    Number     = 0
                }
            },

            // X.3.0.exabyte.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 25045,
                    Number     = 0
                }
            },

            // X.3.Q150.aif
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 101,
                    Number     = 0
                }
            }
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "AaruFormat", "V1");
        public override IMediaImage _plugin => new DiscImages.AaruFormat();
    }
}