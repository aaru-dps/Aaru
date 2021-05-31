// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
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
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filesystems;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Filesystems.FATX
{
    [TestFixture]
    public class Xbox : ReadOnlyFilesystemTest
    {
        public Xbox() : base("FATX filesystem") {}

        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Xbox FAT16", "le");
        public override IFilesystem Plugin => new XboxFatPlugin();
        public override bool Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "fatx.img.lz",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 62720,
                SectorSize   = 512,
                Clusters     = 1960,
                ClusterSize  = 16384,
                VolumeName   = "Volume láb€l",
                VolumeSerial = "4639B7D0",
                Info = new FileSystemInfo
                {
                    Blocks         = 1960,
                    FilenameLength = 42,
                    Files          = 0,
                    FreeBlocks     = 0,
                    FreeFiles      = 0,
                    Type           = "Xbox FAT",
                    Id =
                    {
                        IsInt    = true,
                        Serial32 = 0x58544146
                    },
                    PluginId = Plugin.Id
                },
                Contents = new Dictionary<string, FileData>
                {
                    {
                        "49470015", new FileData
                        {
                            Info = new FileEntryInfo
                            {
                                AccessTimeUtc    = new DateTime(2007, 03, 06, 15, 08, 44, DateTimeKind.Utc),
                                Attributes       = FileAttributes.Directory,
                                Blocks           = 1,
                                BlockSize        = 16384,
                                CreationTimeUtc  = new DateTime(2007, 03, 06, 15, 08, 44, DateTimeKind.Utc),
                                Inode            = 2,
                                LastWriteTimeUtc = new DateTime(2007, 03, 06, 15, 08, 44, DateTimeKind.Utc),
                                Length           = 16384,
                                Links            = 1
                            },
                            Children = new Dictionary<string, FileData>
                            {
                                {
                                    "TitleImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 14, 12, 50, 08, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 14, 12, 50, 08, DateTimeKind.Utc),
                                            Inode            = 3,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 14, 12, 50, 08, DateTimeKind.Utc),
                                            Length           = 10240,
                                            Links            = 1
                                        },
                                        MD5 = "ffcc6c6dfbf2c40aca17abdfa4d405e4"
                                    }
                                },
                                {
                                    "SaveImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 14, 12, 50, 20, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 14, 12, 50, 20, DateTimeKind.Utc),
                                            Inode            = 4,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 14, 12, 50, 20, DateTimeKind.Utc),
                                            Length           = 4096,
                                            Links            = 1
                                        },
                                        MD5 = "5aecd07bd3487167a43fc039ab5eb757"
                                    }
                                },
                                {
                                    "7AC2FE88C908", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 14, 12, 55, 42, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 14, 12, 55, 42, DateTimeKind.Utc),
                                            Inode            = 5,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 14, 12, 55, 42, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 51, 42, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 51, 42, DateTimeKind.Utc),
                                                        Inode = 6,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 51, 42, DateTimeKind.Utc),
                                                        Length = 42,
                                                        Links  = 1
                                                    },
                                                    MD5 = "4a4a402fa9850b14ef4e7c86df816827"
                                                }
                                            },
                                            {
                                                "savedata.dat", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 51, 42, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 4,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 55, 42, DateTimeKind.Utc),
                                                        Inode = 7,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 51, 42, DateTimeKind.Utc),
                                                        Length = 62244,
                                                        Links  = 1
                                                    },
                                                    MD5 = "8956992829f84afab844a9f923e06d38"
                                                }
                                            },
                                            {
                                                "saveimage.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 47, 56, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 47, 56, DateTimeKind.Utc),
                                                        Inode = 11,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 14, 12, 47, 56, DateTimeKind.Utc),
                                                        Length = 4096,
                                                        Links  = 1
                                                    },
                                                    MD5 = "925cc01dd19ed499ef443ee5c1a322fd"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "TitleMeta.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 14, 12, 50, 08, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 14, 12, 50, 08, DateTimeKind.Utc),
                                            Inode            = 12,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 14, 12, 50, 08, DateTimeKind.Utc),
                                            Length           = 34,
                                            Links            = 1
                                        },
                                        MD5 = "7d11933fe277f23c648d97bc79a69080"
                                    }
                                }
                            }
                        }
                    },
                    {
                        "4d5300d1", new FileData
                        {
                            Info = new FileEntryInfo
                            {
                                AccessTimeUtc    = new DateTime(2007, 03, 06, 15, 08, 56, DateTimeKind.Utc),
                                Attributes       = FileAttributes.Directory,
                                Blocks           = 1,
                                BlockSize        = 16384,
                                CreationTimeUtc  = new DateTime(2007, 03, 06, 15, 08, 56, DateTimeKind.Utc),
                                Inode            = 13,
                                LastWriteTimeUtc = new DateTime(2007, 03, 06, 15, 08, 56, DateTimeKind.Utc),
                                Length           = 16384,
                                Links            = 1
                            },
                            Children = new Dictionary<string, FileData>
                            {
                                {
                                    "TitleImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 22, 11, 16, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 22, 11, 16, DateTimeKind.Utc),
                                            Inode            = 14,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 22, 11, 16, DateTimeKind.Utc),
                                            Length           = 10240,
                                            Links            = 1
                                        },
                                        MD5 = "44ae700918695fc85511205c94027803"
                                    }
                                },
                                {
                                    "17BC17F1B373", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 22, 47, 08, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 22, 47, 08, DateTimeKind.Utc),
                                            Inode            = 15,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 22, 47, 08, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Inode = 16,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Length = 30,
                                                        Links  = 1
                                                    },
                                                    MD5 = "1a258a8b22be42c222680fa8bbbb2b2e"
                                                }
                                            },
                                            {
                                                "Profile.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Inode = 17,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Length = 4096,
                                                        Links  = 1
                                                    },
                                                    MD5 = "9bd68a94b5812944f8d28f11411eb9f9"
                                                }
                                            },
                                            {
                                                "saveimage.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Inode = 18,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 05, 22, 47, 06, DateTimeKind.Utc),
                                                        Length = 4096,
                                                        Links  = 1
                                                    },
                                                    MD5 = "4cbdae0b459a9813c3dbbfe852482ad8"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "TitleMeta.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 22, 11, 16, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 22, 11, 16, DateTimeKind.Utc),
                                            Inode            = 19,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 22, 11, 16, DateTimeKind.Utc),
                                            Length           = 74,
                                            Links            = 1
                                        },
                                        MD5 = "5eea3e004dcc043d17bfd5631f3093fc"
                                    }
                                }
                            }
                        }
                    },
                    {
                        "4d53006e", new FileData
                        {
                            Info = new FileEntryInfo
                            {
                                AccessTimeUtc    = new DateTime(2007, 03, 06, 15, 09, 04, DateTimeKind.Utc),
                                Attributes       = FileAttributes.Directory,
                                Blocks           = 1,
                                BlockSize        = 16384,
                                CreationTimeUtc  = new DateTime(2007, 03, 06, 15, 09, 04, DateTimeKind.Utc),
                                Inode            = 20,
                                LastWriteTimeUtc = new DateTime(2007, 03, 06, 15, 09, 04, DateTimeKind.Utc),
                                Length           = 16384,
                                Links            = 1
                            },
                            Children = new Dictionary<string, FileData>
                            {
                                {
                                    "TitleImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 14, 14, 29, 50, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 14, 14, 29, 50, DateTimeKind.Utc),
                                            Inode            = 21,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 14, 14, 29, 50, DateTimeKind.Utc),
                                            Length           = 10240,
                                            Links            = 1
                                        },
                                        MD5 = "e397fd25dd1008c940e05493c8ca81f2"
                                    }
                                },
                                {
                                    "18AAB83D24EF", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 21, 13, 23, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 21, 13, 23, 54, DateTimeKind.Utc),
                                            Inode            = 22,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 21, 13, 23, 54, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 23, 54, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 23, 54, DateTimeKind.Utc),
                                                        Inode = 23,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 23, 54, DateTimeKind.Utc),
                                                        Length = 32,
                                                        Links  = 1
                                                    },
                                                    MD5 = "81f95b7ad4f750f8ecadc4803345fde0"
                                                }
                                            },
                                            {
                                                "saveimage.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 14, 14, 20, 42, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 14, 14, 20, 42, DateTimeKind.Utc),
                                                        Inode = 24,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 14, 14, 20, 42, DateTimeKind.Utc),
                                                        Length = 4096,
                                                        Links  = 1
                                                    },
                                                    MD5 = "2f567b3c40414cc9c3e1c6096ff8eca4"
                                                }
                                            },
                                            {
                                                "RTCIndex.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Inode = 25,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Length = 1452,
                                                        Links  = 1
                                                    },
                                                    MD5 = "03f6a94bc7a6fb708539e44e3f5c26ab"
                                                }
                                            },
                                            {
                                                "Stats.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Inode = 26,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Length = 882,
                                                        Links  = 1
                                                    },
                                                    MD5 = "ddac3531ce8d788472d84687838baa53"
                                                }
                                            },
                                            {
                                                "DrivatarProfile.xml", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 36, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 36, DateTimeKind.Utc),
                                                        Inode = 27,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 36, DateTimeKind.Utc),
                                                        Length = 205,
                                                        Links  = 1
                                                    },
                                                    MD5 = "73dd74fa7da47daec1c3eb0600f6e61e"
                                                }
                                            },
                                            {
                                                "Tsukuba.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Inode = 28,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2013, 05, 21, 13, 29, 34, DateTimeKind.Utc),
                                                        Length = 7950,
                                                        Links  = 1
                                                    },
                                                    MD5 = "e71b737c706197d3bad9054fdb10fb1e"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "TitleMeta.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2013, 05, 14, 14, 29, 50, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2013, 05, 14, 14, 29, 50, DateTimeKind.Utc),
                                            Inode            = 29,
                                            LastWriteTimeUtc = new DateTime(2013, 05, 14, 14, 29, 50, DateTimeKind.Utc),
                                            Length           = 58,
                                            Links            = 1
                                        },
                                        MD5 = "2d91d0fe160393c819c5e4fb12181a33"
                                    }
                                }
                            }
                        }
                    },
                    {
                        "4d530004", new FileData
                        {
                            Info = new FileEntryInfo
                            {
                                AccessTimeUtc    = new DateTime(2007, 03, 06, 15, 09, 20, DateTimeKind.Utc),
                                Attributes       = FileAttributes.Directory,
                                Blocks           = 1,
                                BlockSize        = 16384,
                                CreationTimeUtc  = new DateTime(2007, 03, 06, 15, 09, 20, DateTimeKind.Utc),
                                Inode            = 30,
                                LastWriteTimeUtc = new DateTime(2007, 03, 06, 15, 09, 20, DateTimeKind.Utc),
                                Length           = 16384,
                                Links            = 1
                            },
                            Children = new Dictionary<string, FileData>
                            {
                                {
                                    "TitleImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Inode            = 31,
                                            LastWriteTimeUtc = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Length           = 10240,
                                            Links            = 1
                                        },
                                        MD5 = "050e0693073b089b00fb14a7ee5a0019"
                                    }
                                },
                                {
                                    "SaveImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Inode            = 32,
                                            LastWriteTimeUtc = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Length           = 4096,
                                            Links            = 1
                                        },
                                        MD5 = "742e10609eba1d19f546890feac808ca"
                                    }
                                },
                                {
                                    "122A17771B9F", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                            Inode            = 33,
                                            LastWriteTimeUtc = new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Inode = 34,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Length = 28,
                                                        Links  = 1
                                                    },
                                                    MD5 = "dd8d7db493cbebb333b6f0054df24acf"
                                                }
                                            },
                                            {
                                                "savegame.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 224,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Inode = 35,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Length = 3670016,
                                                        Links  = 1
                                                    },
                                                    MD5 = "047f55cc6793fa0ea3dd6552694a8754"
                                                }
                                            },
                                            {
                                                "blam.sav", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Inode = 259,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 46, 52, DateTimeKind.Utc),
                                                        Length = 512,
                                                        Links  = 1
                                                    },
                                                    MD5 = "a365b6fa9247b017ff16bd6eb403ebd7"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "TitleMeta.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Inode            = 260,
                                            LastWriteTimeUtc = new DateTime(2007, 05, 13, 17, 59, 54, DateTimeKind.Utc),
                                            Length           = 34,
                                            Links            = 1
                                        },
                                        MD5 = "e58efb392e8ffeba52fd68ada9dc250f"
                                    }
                                },
                                {
                                    "16C31B26E558", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2008, 05, 29, 03, 49, 46, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2008, 05, 29, 03, 49, 46, DateTimeKind.Utc),
                                            Inode            = 276,
                                            LastWriteTimeUtc = new DateTime(2008, 05, 29, 03, 49, 46, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 49, 46, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 49, 46, DateTimeKind.Utc),
                                                        Inode = 277,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 49, 46, DateTimeKind.Utc),
                                                        Length = 30,
                                                        Links  = 1
                                                    },
                                                    MD5 = "2efc569cb129cf520ee610e7cf21bad0"
                                                }
                                            },
                                            {
                                                "savegame.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 48, 32, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 224,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 31, 00, 45, 28, DateTimeKind.Utc),
                                                        Inode = 279,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 48, 32, DateTimeKind.Utc),
                                                        Length = 3670016,
                                                        Links  = 1
                                                    },
                                                    MD5 = "6096fa89e6981accdafe6a3840ebe3f3"
                                                }
                                            },
                                            {
                                                "blam.sav", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 48, 32, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 30, 00, 13, 22, DateTimeKind.Utc),
                                                        Inode = 278,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 48, 32, DateTimeKind.Utc),
                                                        Length = 512,
                                                        Links  = 1
                                                    },
                                                    MD5 = "c84c2066c91aca25c3f22f53fe9abe20"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "122A17771B9E", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                            Inode            = 503,
                                            LastWriteTimeUtc = new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Inode = 504,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Length = 28,
                                                        Links  = 1
                                                    },
                                                    MD5 = "da18a1d021e4816e2eb6e6a84f3c0b5a"
                                                }
                                            },
                                            {
                                                "savegame.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 224,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Inode = 505,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Length = 3670016,
                                                        Links  = 1
                                                    },
                                                    MD5 = "7ab424489b08ce63c84e005dce9dbd74"
                                                }
                                            },
                                            {
                                                "blam.sav", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Inode = 729,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 47, 40, DateTimeKind.Utc),
                                                        Length = 512,
                                                        Links  = 1
                                                    },
                                                    MD5 = "af9ecd24da5e5015ce2bcdba61b8c4e6"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "00630061006C", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 05, 13, 18, 19, 20, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 05, 13, 18, 19, 20, DateTimeKind.Utc),
                                            Inode            = 730,
                                            LastWriteTimeUtc = new DateTime(2007, 05, 13, 18, 19, 20, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 20, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 20, DateTimeKind.Utc),
                                                        Inode = 731,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 20, DateTimeKind.Utc),
                                                        Length = 22,
                                                        Links  = 1
                                                    },
                                                    MD5 = "44737eea03ad5b1a911b413c76b6543b"
                                                }
                                            },
                                            {
                                                "savegame.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 04, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 224,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 46, 22, DateTimeKind.Utc),
                                                        Inode = 733,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 04, DateTimeKind.Utc),
                                                        Length = 3670016,
                                                        Links  = 1
                                                    },
                                                    MD5 = "ebe90f15990f89814c9c655edcc459c9"
                                                }
                                            },
                                            {
                                                "blam.sav", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 04, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 37, 20, DateTimeKind.Utc),
                                                        Inode = 732,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 19, 04, DateTimeKind.Utc),
                                                        Length = 512,
                                                        Links  = 1
                                                    },
                                                    MD5 = "14bb70e9cf9e0c4f2a85b334ba9553c1"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "006513971B21", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 05, 13, 18, 00, 46, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 05, 13, 18, 00, 46, DateTimeKind.Utc),
                                            Inode            = 957,
                                            LastWriteTimeUtc = new DateTime(2007, 05, 13, 18, 00, 46, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 46, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 46, DateTimeKind.Utc),
                                                        Inode = 958,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 46, DateTimeKind.Utc),
                                                        Length = 26,
                                                        Links  = 1
                                                    },
                                                    MD5 = "7bc579ebf77ae6e80dcca3d8dc5cc049"
                                                }
                                            },
                                            {
                                                "savegame.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 16, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 224,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 14, 08, 23, 52, DateTimeKind.Utc),
                                                        Inode = 960,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 16, DateTimeKind.Utc),
                                                        Length = 3670016,
                                                        Links  = 1
                                                    },
                                                    MD5 = "9a2b13138e63f397bc1a73b78d8cdd61"
                                                }
                                            },
                                            {
                                                "blam.sav", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 16, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 13, 19, 01, 04, DateTimeKind.Utc),
                                                        Inode = 959,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 13, 18, 00, 16, DateTimeKind.Utc),
                                                        Length = 512,
                                                        Links  = 1
                                                    },
                                                    MD5 = "4cffa85fff011a98247156308e04c49b"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "0061170917BB", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                            Inode            = 1184,
                                            LastWriteTimeUtc = new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Inode = 1185,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Length = 26,
                                                        Links  = 1
                                                    },
                                                    MD5 = "daa39e5a21a8607eab381114baf9c52c"
                                                }
                                            },
                                            {
                                                "savegame.bin", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 224,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2008, 05, 29, 03, 35, 16, DateTimeKind.Utc),
                                                        Inode = 1186,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Length = 3670016,
                                                        Links  = 1
                                                    },
                                                    MD5 = "577de82d90718679bf3947ee9db1faba"
                                                }
                                            },
                                            {
                                                "blam.sav", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 05, 02, 21, 46, 16, DateTimeKind.Utc),
                                                        Inode = 1410,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 05, 02, 16, 29, 54, DateTimeKind.Utc),
                                                        Length = 512,
                                                        Links  = 1
                                                    },
                                                    MD5 = "a25928297f366497f8bdd9020ad91323"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "4947007c", new FileData
                        {
                            Info = new FileEntryInfo
                            {
                                AccessTimeUtc    = new DateTime(2007, 03, 06, 15, 10, 12, DateTimeKind.Utc),
                                Attributes       = FileAttributes.Directory,
                                Blocks           = 1,
                                BlockSize        = 16384,
                                CreationTimeUtc  = new DateTime(2007, 03, 06, 15, 10, 12, DateTimeKind.Utc),
                                Inode            = 261,
                                LastWriteTimeUtc = new DateTime(2007, 03, 06, 15, 10, 12, DateTimeKind.Utc),
                                Length           = 16384,
                                Links            = 1
                            },
                            Children = new Dictionary<string, FileData>
                            {
                                {
                                    "TitleImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Inode            = 262,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Length           = 10240,
                                            Links            = 1
                                        },
                                        MD5 = "3d74ce17d7414a761c456986cb81c017"
                                    }
                                },
                                {
                                    "SaveImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Inode            = 263,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Length           = 4096,
                                            Links            = 1
                                        },
                                        MD5 = "140ca4f78fb933a9533e45d480875646"
                                    }
                                },
                                {
                                    "6D93D2718B8F", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 04, 01, 52, 38, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 04, 01, 52, 38, DateTimeKind.Utc),
                                            Inode            = 264,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 04, 01, 52, 38, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 04, 01, 14, 04, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 04, 01, 14, 04, DateTimeKind.Utc),
                                                        Inode = 265,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 04, 01, 14, 04, DateTimeKind.Utc),
                                                        Length = 50,
                                                        Links  = 1
                                                    },
                                                    MD5 = "bc5ba4c145dc5b2de4c86bc09004bf0f"
                                                }
                                            },
                                            {
                                                "Matrix PON Game 1", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 04, 01, 14, 04, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 04, 01, 52, 38, DateTimeKind.Utc),
                                                        Inode = 266,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 04, 01, 14, 04, DateTimeKind.Utc),
                                                        Length = 9604,
                                                        Links  = 1
                                                    },
                                                    MD5 = "29bc0897607b055baaf77b1170811a2d"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "TitleMeta.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Inode            = 267,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 04, 00, 07, 06, DateTimeKind.Utc),
                                            Length           = 56,
                                            Links            = 1
                                        },
                                        MD5 = "de72490bded233aac0864007c368f0e3"
                                    }
                                }
                            }
                        }
                    },
                    {
                        "4541003e", new FileData
                        {
                            Info = new FileEntryInfo
                            {
                                AccessTimeUtc    = new DateTime(2007, 03, 06, 15, 10, 20, DateTimeKind.Utc),
                                Attributes       = FileAttributes.Directory,
                                Blocks           = 1,
                                BlockSize        = 16384,
                                CreationTimeUtc  = new DateTime(2007, 03, 06, 15, 10, 20, DateTimeKind.Utc),
                                Inode            = 268,
                                LastWriteTimeUtc = new DateTime(2007, 03, 06, 15, 10, 20, DateTimeKind.Utc),
                                Length           = 16384,
                                Links            = 1
                            },
                            Children = new Dictionary<string, FileData>
                            {
                                {
                                    "TitleImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Inode            = 269,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Length           = 10240,
                                            Links            = 1
                                        },
                                        MD5 = "8c58d74ac47302a4b554d5b15691e148"
                                    }
                                },
                                {
                                    "SaveImage.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Inode            = 270,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Length           = 4096,
                                            Links            = 1
                                        },
                                        MD5 = "73f07585217f400faeba8752394452b8"
                                    }
                                },
                                {
                                    "2ECC5651910B", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.Directory,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                            Inode            = 271,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                            Length           = 16384,
                                            Links            = 1
                                        },
                                        Children = new Dictionary<string, FileData>
                                        {
                                            {
                                                "SaveMeta.xbx", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 1,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                                        Inode = 272,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                                        Length = 36,
                                                        Links  = 1
                                                    },
                                                    MD5 = "566bb332aae52bb93e48351a1fe680a3"
                                                }
                                            },
                                            {
                                                "rotk", new FileData
                                                {
                                                    Info = new FileEntryInfo
                                                    {
                                                        AccessTimeUtc =
                                                            new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                                        Attributes = FileAttributes.None,
                                                        Blocks     = 2,
                                                        BlockSize  = 16384,
                                                        CreationTimeUtc =
                                                            new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                                        Inode = 273,
                                                        LastWriteTimeUtc =
                                                            new DateTime(2007, 03, 05, 23, 08, 04, DateTimeKind.Utc),
                                                        Length = 24436,
                                                        Links  = 1
                                                    },
                                                    MD5 = "cd3b513946f6aeef06b13dac68c47794"
                                                }
                                            }
                                        }
                                    }
                                },
                                {
                                    "TitleMeta.xbx", new FileData
                                    {
                                        Info = new FileEntryInfo
                                        {
                                            AccessTimeUtc    = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Attributes       = FileAttributes.None,
                                            Blocks           = 1,
                                            BlockSize        = 16384,
                                            CreationTimeUtc  = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Inode            = 275,
                                            LastWriteTimeUtc = new DateTime(2007, 03, 05, 22, 56, 54, DateTimeKind.Utc),
                                            Length           = 410,
                                            Links            = 1
                                        },
                                        MD5 = "5fd0375de8c1657f40f44e8c161e674d"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}