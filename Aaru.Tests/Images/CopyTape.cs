// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CopyTape.cs
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

using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class CopyTape
    {
        readonly string[] _testFiles =
        {
            "Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz",
            "Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz", "OpenWindows.3.0.exabyte.cptp.lz",
            "OpenWindows.3.0.Q150.cptp.lz", "OS.MP.4.1C.exabyte.cptp.lz", "X.3.0.exabyte.cptp.lz", "X.3.Q150.cptp.lz"
        };
        readonly ulong[] _sectors =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz
            1604,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz
            15485,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz
            15,

            // Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz
            3298,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz
            3152,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz
            818,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz
            7,

            // Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz
            684,

            // OpenWindows.3.0.exabyte.cptp.lz
            73525,

            // OpenWindows.3.0.Q150.cptp.lz
            290,

            // OS.MP.4.1C.exabyte.cptp.lz
            37587,

            // X.3.0.exabyte.cptp.lz
            25046,

            // X.3.Q150.cptp.lz
            102
        };
        readonly uint[] _sectorSize =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz
            10240,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz
            512,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz
            32256,

            // Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz
            32256,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz
            32256,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz
            32256,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz
            32256,

            // Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz
            32256,

            // OpenWindows.3.0.exabyte.cptp.lz
            1024,

            // OpenWindows.3.0.Q150.cptp.lz
            262144,

            // OS.MP.4.1C.exabyte.cptp.lz
            8192,

            // X.3.0.exabyte.cptp.lz
            1024,

            // X.3.Q150.cptp.lz
            262144
        };
        readonly MediaType[] _mediaTypes =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz
            MediaType.UnknownTape,

            // Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz
            MediaType.UnknownTape,

            // OpenWindows.3.0.exabyte.cptp.lz
            MediaType.UnknownTape,

            // OpenWindows.3.0.Q150.cptp.lz
            MediaType.UnknownTape,

            // OS.MP.4.1C.exabyte.cptp.lz
            MediaType.UnknownTape,

            // X.3.0.exabyte.cptp.lz
            MediaType.UnknownTape,

            // X.3.Q150.cptp.lz
            MediaType.UnknownTape
        };
        readonly string[] _md5S =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz
            "a6334d975523b3422fea522b0cc118a9",

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz
            "17ef78d9e5c53b976f530d4ca44223fd",

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz
            "76c0ae10f4ec70ef8681b212f02a71c8",

            // Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz
            "e331c9d0ae7c25c81c6580bc9965e2d0",

            // Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz
            "018c37c40f8df91ab9b098d643c9ae6c",

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz
            "eb3ce36b2c3afeeec59e5b8ed802a393",

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz
            "b057656698a224187afb2bdbb8caf7f3",

            // Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz
            "8e48e388e7094f3170065718ab618b53",

            // OpenWindows.3.0.exabyte.cptp.lz
            "8861f8c06a2e93ca5a81d729ad3e1de1",

            // OpenWindows.3.0.Q150.cptp.lz
            "bfc402b23af0cf1ad22d9fb2ea29b58f",

            // OS.MP.4.1C.exabyte.cptp.lz
            "e4a3e2fe26c72ca025ac0c017ec73ee9",

            // X.3.0.exabyte.cptp.lz
            "e625c03d7493dc22fe49f91f731446e8",

            // X.3.Q150.cptp.lz
            "198464b1daf8e674debf8eda0fcbf016"
        };

        readonly TapeFile[][] _tapeFiles =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz
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

            // Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz
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

            // OpenWindows.3.0.exabyte.cptp.lz
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

            // OpenWindows.3.0.Q150.cptp.lz
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

            // OS.MP.4.1C.exabyte.cptp.lz
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

            // X.3.0.exabyte.cptp.lz
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

            // X.3.Q150.cptp.lz
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
        readonly TapePartition[][] _tapePartitions =
        {
            // Nonstop-UX System V Release 4 B32 (Boot Tape).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 1603,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Integrity SX25 VME V5.0+).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 15484,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Online Software Upgrade).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 14,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Operating System).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 3297,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Optional Packages).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 3151,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (OSF-Motif 1.2.4).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 817,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Reliable Ethernet).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 6,
                    Number     = 0
                }
            },

            // Nonstop-UX System V Release 4 B32 (Required Packages).cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 683,
                    Number     = 0
                }
            },

            // OpenWindows.3.0.exabyte.cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 73524,
                    Number     = 0
                }
            },

            // OpenWindows.3.0.Q150.cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 289,
                    Number     = 0
                }
            },

            // OS.MP.4.1C.exabyte.cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 37586,
                    Number     = 0
                }
            },

            // X.3.0.exabyte.cptp.lz
            new[]
            {
                new TapePartition
                {
                    FirstBlock = 0,
                    LastBlock  = 25045,
                    Number     = 0
                }
            },

            // X.3.Q150.cptp.lz
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

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "copytape");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.CopyTape();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                            Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                            Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");
                        });
                    }
                }
            });
        }

        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    var   image       = new DiscImages.CopyTape();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    var ctx = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            sector      =  image.ReadSectors(doneSectors, SECTORS_TO_READ);
                            doneSectors += SECTORS_TO_READ;
                        }
                        else
                        {
                            sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                            doneSectors += image.Info.Sectors - doneSectors;
                        }

                        ctx.Update(sector);
                    }

                    Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                }
            });
        }

        [Test]
        public void Tape()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    ITapeImage image  = new DiscImages.CopyTape();
                    bool       opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    Assert.AreEqual(true, image.IsTape, $"Is tape?: {_testFiles[i]}");

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            image.Files.Should().BeEquivalentTo(_tapeFiles[i], $"Tape files: {_testFiles[i]}");

                            image.TapePartitions.Should().
                                  BeEquivalentTo(_tapePartitions[i], $"Tape files: {_testFiles[i]}");
                        });
                    }
                }
            });
        }
    }
}