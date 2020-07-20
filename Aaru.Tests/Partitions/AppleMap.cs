// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleMap.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class AppleMap
    {
        readonly string[] testfiles =
        {
            "d2_driver.aif", "hdt_1.8_encrypted1.aif", "hdt_1.8_encrypted2.aif", "hdt_1.8_password.aif", "hdt_1.8.aif",
            "linux.aif", "macos_1.1.aif", "macos_2.0.aif", "macos_4.2.aif", "macos_4.3.aif", "macos_6.0.2.aif",
            "macos_6.0.3.aif", "macos_6.0.4.aif", "macos_6.0.5.aif", "macos_6.0.7.aif", "macos_6.0.8.aif",
            "macos_6.0.aif", "macos_7.0.aif", "macos_7.1.1.aif", "macos_7.5.aif", "parted.aif",
            "silverlining_2.2.1.aif", "speedtools_3.6.aif", "vcpformatter_2.1.1.aif"
        };

        readonly Partition[][] wanted =
        {
            // D2
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 1024,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 2,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 42496,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 83,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 55808,
                    Name        = "Empty",
                    Type        = "Apple_Free",
                    Offset      = 75264,
                    Length      = 109,
                    Sequence    = 2,
                    Start       = 147
                },
                new Partition
                {
                    Description = null,
                    Size        = 26083328,
                    Name        = "Volume label",
                    Type        = "Apple_HFS",
                    Offset      = 131072,
                    Length      = 50944,
                    Sequence    = 3,
                    Start       = 256
                }
            },

            // HDT 1.8 Encryption Level 1
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 7168,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 14,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 524288,
                    Name        = "FWB Disk Driver",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 1024,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 25657344,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 557056,
                    Length      = 50112,
                    Sequence    = 2,
                    Start       = 1088
                }
            },

            // HDT 1.8 Encryption Level 2
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 7168,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 14,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 524288,
                    Name        = "FWB Disk Driver",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 1024,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 25657344,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 557056,
                    Length      = 50112,
                    Sequence    = 2,
                    Start       = 1088
                }
            },

            // HDT 1.8 with password
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 7168,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 14,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 524288,
                    Name        = "FWB Disk Driver",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 1024,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 25657344,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 557056,
                    Length      = 50112,
                    Sequence    = 2,
                    Start       = 1088
                }
            },

            // HDT 1.8
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 7168,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 14,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 524288,
                    Name        = "FWB Disk Driver",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 1024,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 25657344,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 557056,
                    Length      = 50112,
                    Sequence    = 2,
                    Start       = 1088
                }
            },

            // Linux
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 512,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 32768,
                    Length      = 1,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 819200,
                    Name        = "bootstrap",
                    Type        = "Apple_Bootstrap",
                    Offset      = 33280,
                    Length      = 1600,
                    Sequence    = 1,
                    Start       = 65
                },
                new Partition
                {
                    Description = null,
                    Size        = 512,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 852480,
                    Length      = 1,
                    Sequence    = 2,
                    Start       = 1665
                },
                new Partition
                {
                    Description = null,
                    Size        = 52428800,
                    Name        = "Linux",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 852992,
                    Length      = 102400,
                    Sequence    = 3,
                    Start       = 1666
                },
                new Partition
                {
                    Description = null,
                    Size        = 20971520,
                    Name        = "ProDOS",
                    Type        = "Apple_PRODOS",
                    Offset      = 53281792,
                    Length      = 40960,
                    Sequence    = 4,
                    Start       = 104066
                },
                new Partition
                {
                    Description = null,
                    Size        = 52428800,
                    Name        = "Macintosh",
                    Type        = "Apple_HFS",
                    Offset      = 74253312,
                    Length      = 102400,
                    Sequence    = 5,
                    Start       = 145026
                },
                new Partition
                {
                    Description = null,
                    Size        = 7535616,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 126682112,
                    Length      = 14718,
                    Sequence    = 6,
                    Start       = 247426
                }
            },

            // Mac OS 1.1
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 2048,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 4
                },
                new Partition
                {
                    Description = null,
                    Size        = 21403648,
                    Name        = "Macintosh",
                    Type        = "Apple_HFS",
                    Offset      = 8192,
                    Length      = 41804,
                    Sequence    = 1,
                    Start       = 16
                }
            },

            // Mac OS 2.0
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 2048,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 4
                },
                new Partition
                {
                    Description = null,
                    Size        = 19950080,
                    Name        = "Macintosh",
                    Type        = "Apple_HFS",
                    Offset      = 8192,
                    Length      = 38965,
                    Sequence    = 1,
                    Start       = 16
                }
            },

            // Mac OS 4.2
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5632,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 2048,
                    Length      = 11,
                    Sequence    = 0,
                    Start       = 4
                },
                new Partition
                {
                    Description = null,
                    Size        = 19950080,
                    Name        = "Macintosh",
                    Type        = "Apple_HFS",
                    Offset      = 8192,
                    Length      = 38965,
                    Sequence    = 1,
                    Start       = 16
                }
            },

            // Mac OS 4.3
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5632,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 2048,
                    Length      = 11,
                    Sequence    = 0,
                    Start       = 4
                },
                new Partition
                {
                    Description = null,
                    Size        = 19950080,
                    Name        = "Macintosh",
                    Type        = "Apple_HFS",
                    Offset      = 8192,
                    Length      = 38965,
                    Sequence    = 1,
                    Start       = 16
                }
            },

            // Mac OS 6.0.2
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 3203072,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 6256,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 524288,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 3252224,
                    Length      = 1024,
                    Sequence    = 3,
                    Start       = 6352
                },
                new Partition
                {
                    Description = null,
                    Size        = 1048576,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 3776512,
                    Length      = 2048,
                    Sequence    = 4,
                    Start       = 7376
                },
                new Partition
                {
                    Description = null,
                    Size        = 2191360,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 4825088,
                    Length      = 4280,
                    Sequence    = 5,
                    Start       = 9424
                },
                new Partition
                {
                    Description = null,
                    Size        = 1217024,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 7016448,
                    Length      = 2377,
                    Sequence    = 6,
                    Start       = 13704
                },
                new Partition
                {
                    Description = null,
                    Size        = 1572864,
                    Name        = "Eschatology 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 8233472,
                    Length      = 3072,
                    Sequence    = 7,
                    Start       = 16081
                },
                new Partition
                {
                    Description = null,
                    Size        = 1310720,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 9806336,
                    Length      = 2560,
                    Sequence    = 8,
                    Start       = 19153
                },
                new Partition
                {
                    Description = null,
                    Size        = 2550272,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 11117056,
                    Length      = 4981,
                    Sequence    = 9,
                    Start       = 21713
                },
                new Partition
                {
                    Description = null,
                    Size        = 2048000,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 13667328,
                    Length      = 4000,
                    Sequence    = 10,
                    Start       = 26694
                },
                new Partition
                {
                    Description = null,
                    Size        = 1296384,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 15715328,
                    Length      = 2532,
                    Sequence    = 11,
                    Start       = 30694
                },
                new Partition
                {
                    Description = null,
                    Size        = 1364992,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 17011712,
                    Length      = 2666,
                    Sequence    = 12,
                    Start       = 33226
                },
                new Partition
                {
                    Description = null,
                    Size        = 3986432,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 18376704,
                    Length      = 7786,
                    Sequence    = 13,
                    Start       = 35892
                },
                new Partition
                {
                    Description = null,
                    Size        = 5714944,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 22363136,
                    Length      = 11162,
                    Sequence    = 14,
                    Start       = 43678
                }
            },

            // Mac OS 6.0.3
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 5948928,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 11619,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 1029632,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 5998080,
                    Length      = 2011,
                    Sequence    = 3,
                    Start       = 11715
                },
                new Partition
                {
                    Description = null,
                    Size        = 2455552,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 7027712,
                    Length      = 4796,
                    Sequence    = 4,
                    Start       = 13726
                },
                new Partition
                {
                    Description = null,
                    Size        = 3932160,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 9483264,
                    Length      = 7680,
                    Sequence    = 5,
                    Start       = 18522
                },
                new Partition
                {
                    Description = null,
                    Size        = 4194304,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 13415424,
                    Length      = 8192,
                    Sequence    = 6,
                    Start       = 26202
                },
                new Partition
                {
                    Description = null,
                    Size        = 587776,
                    Name        = "Eschatology 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 17609728,
                    Length      = 1148,
                    Sequence    = 7,
                    Start       = 34394
                },
                new Partition
                {
                    Description = null,
                    Size        = 6537216,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 18197504,
                    Length      = 12768,
                    Sequence    = 8,
                    Start       = 35542
                },
                new Partition
                {
                    Description = null,
                    Size        = 1766400,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 24734720,
                    Length      = 3450,
                    Sequence    = 9,
                    Start       = 48310
                },
                new Partition
                {
                    Description = null,
                    Size        = 18432,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 26501120,
                    Length      = 36,
                    Sequence    = 10,
                    Start       = 51760
                },
                new Partition
                {
                    Description = null,
                    Size        = 1558528,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 26519552,
                    Length      = 3044,
                    Sequence    = 11,
                    Start       = 51796
                }
            },

            // Mac OS 6.0.4
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 3932160,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 7680,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 3197440,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 3981312,
                    Length      = 6245,
                    Sequence    = 3,
                    Start       = 7776
                },
                new Partition
                {
                    Description = null,
                    Size        = 3197440,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 7178752,
                    Length      = 6245,
                    Sequence    = 4,
                    Start       = 14021
                },
                new Partition
                {
                    Description = null,
                    Size        = 2626560,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 10376192,
                    Length      = 5130,
                    Sequence    = 5,
                    Start       = 20266
                },
                new Partition
                {
                    Description = null,
                    Size        = 1370112,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 13002752,
                    Length      = 2676,
                    Sequence    = 6,
                    Start       = 25396
                },
                new Partition
                {
                    Description = null,
                    Size        = 2944512,
                    Name        = "Eschatology 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 14372864,
                    Length      = 5751,
                    Sequence    = 7,
                    Start       = 28072
                },
                new Partition
                {
                    Description = null,
                    Size        = 2776576,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 17317376,
                    Length      = 5423,
                    Sequence    = 8,
                    Start       = 33823
                },
                new Partition
                {
                    Description = null,
                    Size        = 2892800,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 20093952,
                    Length      = 5650,
                    Sequence    = 9,
                    Start       = 39246
                },
                new Partition
                {
                    Description = null,
                    Size        = 3433472,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 22986752,
                    Length      = 6706,
                    Sequence    = 10,
                    Start       = 44896
                },
                new Partition
                {
                    Description = null,
                    Size        = 1657856,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 26420224,
                    Length      = 3238,
                    Sequence    = 11,
                    Start       = 51602
                }
            },

            // Mac OS 6.0.5
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 4096,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 342528,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 2146304,
                    Length      = 669,
                    Sequence    = 3,
                    Start       = 4192
                },
                new Partition
                {
                    Description = null,
                    Size        = 1417216,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 2488832,
                    Length      = 2768,
                    Sequence    = 4,
                    Start       = 4861
                },
                new Partition
                {
                    Description = null,
                    Size        = 1830912,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 3906048,
                    Length      = 3576,
                    Sequence    = 5,
                    Start       = 7629
                },
                new Partition
                {
                    Description = null,
                    Size        = 1448960,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 5736960,
                    Length      = 2830,
                    Sequence    = 6,
                    Start       = 11205
                },
                new Partition
                {
                    Description = null,
                    Size        = 2687488,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 7185920,
                    Length      = 5249,
                    Sequence    = 7,
                    Start       = 14035
                },
                new Partition
                {
                    Description = null,
                    Size        = 2565632,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 9873408,
                    Length      = 5011,
                    Sequence    = 8,
                    Start       = 19284
                },
                new Partition
                {
                    Description = null,
                    Size        = 1954816,
                    Name        = "Unreserved 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 12439040,
                    Length      = 3818,
                    Sequence    = 9,
                    Start       = 24295
                },
                new Partition
                {
                    Description = null,
                    Size        = 3543040,
                    Name        = "Unreserved 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 14393856,
                    Length      = 6920,
                    Sequence    = 10,
                    Start       = 28113
                },
                new Partition
                {
                    Description = null,
                    Size        = 2565632,
                    Name        = "Unreserved 3",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 17936896,
                    Length      = 5011,
                    Sequence    = 11,
                    Start       = 35033
                },
                new Partition
                {
                    Description = null,
                    Size        = 2932224,
                    Name        = "Unreserved 4",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 20502528,
                    Length      = 5727,
                    Sequence    = 12,
                    Start       = 40044
                },
                new Partition
                {
                    Description = null,
                    Size        = 1221632,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 23434752,
                    Length      = 2386,
                    Sequence    = 13,
                    Start       = 45771
                },
                new Partition
                {
                    Description = null,
                    Size        = 3421696,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 24656384,
                    Length      = 6683,
                    Sequence    = 14,
                    Start       = 48157
                }
            },

            // Mac OS 6.0.7
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 14013952,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 27371,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 1492992,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 14063104,
                    Length      = 2916,
                    Sequence    = 3,
                    Start       = 27467
                },
                new Partition
                {
                    Description = null,
                    Size        = 919040,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 15556096,
                    Length      = 1795,
                    Sequence    = 4,
                    Start       = 30383
                },
                new Partition
                {
                    Description = null,
                    Size        = 1302016,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 16475136,
                    Length      = 2543,
                    Sequence    = 5,
                    Start       = 32178
                },
                new Partition
                {
                    Description = null,
                    Size        = 1796608,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 17777152,
                    Length      = 3509,
                    Sequence    = 6,
                    Start       = 34721
                },
                new Partition
                {
                    Description = null,
                    Size        = 1943552,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 19573760,
                    Length      = 3796,
                    Sequence    = 7,
                    Start       = 38230
                },
                new Partition
                {
                    Description = null,
                    Size        = 2186752,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 21517312,
                    Length      = 4271,
                    Sequence    = 8,
                    Start       = 42026
                },
                new Partition
                {
                    Description = null,
                    Size        = 524288,
                    Name        = "Unreserved 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 23704064,
                    Length      = 1024,
                    Sequence    = 9,
                    Start       = 46297
                },
                new Partition
                {
                    Description = null,
                    Size        = 655360,
                    Name        = "Unreserved 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 24228352,
                    Length      = 1280,
                    Sequence    = 10,
                    Start       = 47321
                },
                new Partition
                {
                    Description = null,
                    Size        = 798208,
                    Name        = "Unreserved 3",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 24883712,
                    Length      = 1559,
                    Sequence    = 11,
                    Start       = 48601
                },
                new Partition
                {
                    Description = null,
                    Size        = 143360,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 25681920,
                    Length      = 280,
                    Sequence    = 12,
                    Start       = 50160
                },
                new Partition
                {
                    Description = null,
                    Size        = 2252800,
                    Name        = "Unreserved 4",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 25825280,
                    Length      = 4400,
                    Sequence    = 13,
                    Start       = 50440
                }
            },

            // Mac OS 6.0.8
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 4575744,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 8937,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 1143808,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 4624896,
                    Length      = 2234,
                    Sequence    = 3,
                    Start       = 9033
                },
                new Partition
                {
                    Description = null,
                    Size        = 3020800,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 5768704,
                    Length      = 5900,
                    Sequence    = 4,
                    Start       = 11267
                },
                new Partition
                {
                    Description = null,
                    Size        = 1615872,
                    Name        = "Unreserved 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 8789504,
                    Length      = 3156,
                    Sequence    = 5,
                    Start       = 17167
                },
                new Partition
                {
                    Description = null,
                    Size        = 1384960,
                    Name        = "Unreserved 3",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 10405376,
                    Length      = 2705,
                    Sequence    = 6,
                    Start       = 20323
                },
                new Partition
                {
                    Description = null,
                    Size        = 952832,
                    Name        = "Unreserved 4",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 11790336,
                    Length      = 1861,
                    Sequence    = 7,
                    Start       = 23028
                },
                new Partition
                {
                    Description = null,
                    Size        = 1246208,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 12743168,
                    Length      = 2434,
                    Sequence    = 8,
                    Start       = 24889
                },
                new Partition
                {
                    Description = null,
                    Size        = 1495040,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 13989376,
                    Length      = 2920,
                    Sequence    = 9,
                    Start       = 27323
                },
                new Partition
                {
                    Description = null,
                    Size        = 1615872,
                    Name        = "Unreserved 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 15484416,
                    Length      = 3156,
                    Sequence    = 10,
                    Start       = 30243
                },
                new Partition
                {
                    Description = null,
                    Size        = 2885120,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 17100288,
                    Length      = 5635,
                    Sequence    = 11,
                    Start       = 33399
                },
                new Partition
                {
                    Description = null,
                    Size        = 2308096,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 19985408,
                    Length      = 4508,
                    Sequence    = 12,
                    Start       = 39034
                },
                new Partition
                {
                    Description = null,
                    Size        = 3693056,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 22293504,
                    Length      = 7213,
                    Sequence    = 13,
                    Start       = 43542
                },
                new Partition
                {
                    Description = null,
                    Size        = 2091520,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 25986560,
                    Length      = 4085,
                    Sequence    = 14,
                    Start       = 50755
                }
            },

            // Mac OS 6.0
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 4096,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 2146304,
                    Length      = 4096,
                    Sequence    = 3,
                    Start       = 4192
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 4243456,
                    Length      = 4096,
                    Sequence    = 4,
                    Start       = 8288
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 6340608,
                    Length      = 4096,
                    Sequence    = 5,
                    Start       = 12384
                },
                new Partition
                {
                    Description = null,
                    Size        = 1048576,
                    Name        = "Swap",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 8437760,
                    Length      = 2048,
                    Sequence    = 6,
                    Start       = 16480
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "Eschatology 2",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 9486336,
                    Length      = 4096,
                    Sequence    = 7,
                    Start       = 18528
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 11583488,
                    Length      = 4096,
                    Sequence    = 8,
                    Start       = 22624
                },
                new Partition
                {
                    Description = null,
                    Size        = 2310144,
                    Name        = "Usr file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 13680640,
                    Length      = 4512,
                    Sequence    = 9,
                    Start       = 26720
                },
                new Partition
                {
                    Description = null,
                    Size        = 5416960,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 15990784,
                    Length      = 10580,
                    Sequence    = 10,
                    Start       = 31232
                },
                new Partition
                {
                    Description = null,
                    Size        = 4096,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 21407744,
                    Length      = 8,
                    Sequence    = 11,
                    Start       = 41812
                }
            },

            // Mac OS 7.0
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 5120,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 10,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 3073024,
                    Name        = "Scratch",
                    Type        = "Apple_Scratch",
                    Offset      = 49152,
                    Length      = 6002,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 2726400,
                    Name        = "Root file system",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 3122176,
                    Length      = 5325,
                    Sequence    = 3,
                    Start       = 6098
                },
                new Partition
                {
                    Description = null,
                    Size        = 3180544,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 5848576,
                    Length      = 6212,
                    Sequence    = 4,
                    Start       = 11423
                },
                new Partition
                {
                    Description = null,
                    Size        = 4203520,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 9029120,
                    Length      = 8210,
                    Sequence    = 5,
                    Start       = 17635
                },
                new Partition
                {
                    Description = null,
                    Size        = 2613248,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 13232640,
                    Length      = 5104,
                    Sequence    = 6,
                    Start       = 25845
                },
                new Partition
                {
                    Description = null,
                    Size        = 5262336,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 15845888,
                    Length      = 10278,
                    Sequence    = 7,
                    Start       = 30949
                },
                new Partition
                {
                    Description = null,
                    Size        = 1707520,
                    Name        = "Eschatology 1",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 21108224,
                    Length      = 3335,
                    Sequence    = 8,
                    Start       = 41227
                },
                new Partition
                {
                    Description = null,
                    Size        = 5262336,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 22815744,
                    Length      = 10278,
                    Sequence    = 9,
                    Start       = 44562
                }
            },

            // Mac OS 7.1.1
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 8704,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 17,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 1486848,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 49152,
                    Length      = 2904,
                    Sequence    = 2,
                    Start       = 96
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "ProDOS",
                    Type        = "Apple_PRODOS",
                    Offset      = 1536000,
                    Length      = 4096,
                    Sequence    = 3,
                    Start       = 3000
                },
                new Partition
                {
                    Description = null,
                    Size        = 1564160,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 3633152,
                    Length      = 3055,
                    Sequence    = 4,
                    Start       = 7096
                },
                new Partition
                {
                    Description = null,
                    Size        = 2097152,
                    Name        = "ProDOS",
                    Type        = "Apple_PRODOS",
                    Offset      = 5197312,
                    Length      = 4096,
                    Sequence    = 5,
                    Start       = 10151
                },
                new Partition
                {
                    Description = null,
                    Size        = 5148160,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 7294464,
                    Length      = 10055,
                    Sequence    = 6,
                    Start       = 14247
                },
                new Partition
                {
                    Description = null,
                    Size        = 4406784,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 12442624,
                    Length      = 8607,
                    Sequence    = 7,
                    Start       = 24302
                },
                new Partition
                {
                    Description = null,
                    Size        = 2485760,
                    Name        = "Random A/UX fs",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 16849408,
                    Length      = 4855,
                    Sequence    = 8,
                    Start       = 32909
                },
                new Partition
                {
                    Description = null,
                    Size        = 4746240,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 19335168,
                    Length      = 9270,
                    Sequence    = 9,
                    Start       = 37764
                },
                new Partition
                {
                    Description = null,
                    Size        = 3996672,
                    Name        = "A/UX Root",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 24081408,
                    Length      = 7806,
                    Sequence    = 10,
                    Start       = 47034
                }
            },

            // Mac OS 7.5
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 9216,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 18,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 28028928,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 49152,
                    Length      = 54744,
                    Sequence    = 2,
                    Start       = 96
                }
            },

            // GNU Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 2064384,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 32768,
                    Length      = 4032,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 47185920,
                    Name        = "untitled",
                    Type        = "Apple_HFS",
                    Offset      = 2097152,
                    Length      = 92160,
                    Sequence    = 1,
                    Start       = 4096
                },
                new Partition
                {
                    Description = null,
                    Size        = 84934656,
                    Name        = "untitled",
                    Type        = "Apple_UNIX_SVR2",
                    Offset      = 49283072,
                    Length      = 165888,
                    Sequence    = 2,
                    Start       = 96256
                }
            },

            // Silverlining 2.2.1
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 3072,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 6,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 65536,
                    Name        = "Macintosh_SL",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 128,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 25088,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 98304,
                    Length      = 49,
                    Sequence    = 2,
                    Start       = 192
                },
                new Partition
                {
                    Description = null,
                    Size        = 65536,
                    Name        = "Macintosh_SL",
                    Type        = "Apple_Driver_ATA",
                    Offset      = 98304,
                    Length      = 128,
                    Sequence    = 3,
                    Start       = 192
                },
                new Partition
                {
                    Description = null,
                    Size        = 25804800,
                    Name        = "Untitled  #1",
                    Type        = "Apple_HFS",
                    Offset      = 163840,
                    Length      = 50400,
                    Sequence    = 4,
                    Start       = 320
                },
                new Partition
                {
                    Description = null,
                    Size        = 237568,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 25968640,
                    Length      = 464,
                    Sequence    = 5,
                    Start       = 50720
                }
            },

            // Hard Disk Speed Tools 3.6
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 13824,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 32768,
                    Length      = 27,
                    Sequence    = 0,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 51200,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver43",
                    Offset      = 32768,
                    Length      = 100,
                    Sequence    = 1,
                    Start       = 64
                },
                new Partition
                {
                    Description = null,
                    Size        = 25165824,
                    Name        = "untitled",
                    Type        = "Apple_HFS",
                    Offset      = 83968,
                    Length      = 49152,
                    Sequence    = 2,
                    Start       = 164
                },
                new Partition
                {
                    Description = null,
                    Size        = 963584,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 25249792,
                    Length      = 1882,
                    Sequence    = 3,
                    Start       = 49316
                }
            },

            // VCP Formatter 2.1.1
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 12288,
                    Name        = null,
                    Type        = "Apple_Driver",
                    Offset      = 57344,
                    Length      = 24,
                    Sequence    = 0,
                    Start       = 112
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Macintosh",
                    Type        = "Apple_Driver",
                    Offset      = 57344,
                    Length      = 32,
                    Sequence    = 1,
                    Start       = 112
                },
                new Partition
                {
                    Description = null,
                    Size        = 16384,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 73728,
                    Length      = 32,
                    Sequence    = 2,
                    Start       = 144
                },
                new Partition
                {
                    Description = null,
                    Size        = 27986944,
                    Name        = "MacOS",
                    Type        = "Apple_HFS",
                    Offset      = 90112,
                    Length      = 54662,
                    Sequence    = 3,
                    Start       = 176
                },

                // TODO: ADFS tries to read past this partition...
                new Partition
                {
                    Description = null,
                    Size        = 1024,
                    Name        = "Extra",
                    Type        = "Apple_Free",
                    Offset      = 28077056,
                    Length      = 2,
                    Sequence    = 4,
                    Start       = 54838
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "Apple Partition Map",
                                               testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);

                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Size, partitions[j].Size, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Name, partitions[j].Name, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Type, partitions[j].Type, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Offset, partitions[j].Offset, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length, partitions[j].Length, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Sequence, partitions[j].Sequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start, partitions[j].Start, testfiles[i]);
                }
            }
        }
    }
}