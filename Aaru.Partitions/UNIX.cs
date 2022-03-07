// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UNIX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides old UNIX hardwired partitions for appropiate device dumps.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Partitions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;

// These partitions are hardwired in kernel sources for some UNIX versions predating System V.
// They depend on exact device, indeed the kernel chooses what to use depending on the disk driver, so that's what we do.
// Currently only DEC devices used in Ultrix are added, probably it's missing a lot of entries.
/// <inheritdoc />
/// <summary>Implements decoding of historic UNIX static partitions</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UNIX : IPartition
{
    readonly Partition[] RA60 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9600,
            Start       = 0,
            Size        = 4915200,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 20000,
            Start       = 9600,
            Size        = 10240000,
            Offset      = 4915200,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 29600,
            Size        = 102400,
            Offset      = 15155200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 6000,
            Start       = 29800,
            Size        = 3072000,
            Offset      = 15257600,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 363376,
            Start       = 35800,
            Size        = 186048512,
            Offset      = 18329600,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 181688,
            Start       = 35800,
            Size        = 93024256,
            Offset      = 18329600,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 181688,
            Start       = 217488,
            Size        = 93024256,
            Offset      = 111353856,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 1000,
            Start       = 399176,
            Size        = 512000,
            Offset      = 204378112,
            Sequence    = 8
        }
    };

    readonly Partition[] RA80 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9600,
            Start       = 0,
            Size        = 4915200,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 20000,
            Start       = 9600,
            Size        = 10240000,
            Offset      = 4915200,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 29600,
            Size        = 102400,
            Offset      = 15155200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 6000,
            Start       = 29800,
            Size        = 3072000,
            Offset      = 15257600,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 200412,
            Start       = 35800,
            Size        = 102610944,
            Offset      = 18329600,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 1000,
            Start       = 236212,
            Size        = 512000,
            Offset      = 120940544,
            Sequence    = 8
        }
    };

    readonly Partition[] RA81 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9600,
            Start       = 0,
            Size        = 4915200,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 20000,
            Start       = 9600,
            Size        = 10240000,
            Offset      = 4915200,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 29600,
            Size        = 102400,
            Offset      = 15155200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 6000,
            Start       = 29800,
            Size        = 3072000,
            Offset      = 15257600,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 854272,
            Start       = 35800,
            Size        = 437387264,
            Offset      = 18329600,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 181688,
            Start       = 35800,
            Size        = 93024256,
            Offset      = 18329600,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 181688,
            Start       = 217488,
            Size        = 93024256,
            Offset      = 111353856,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 490896,
            Start       = 399176,
            Size        = 251338752,
            Offset      = 204378112,
            Sequence    = 6
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 1000,
            Start       = 890072,
            Size        = 512000,
            Offset      = 455716864,
            Sequence    = 8
        }
    };

    readonly Partition[] RC25 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9000,
            Start       = 0,
            Size        = 4608000,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 9000,
            Size        = 102400,
            Offset      = 4608000,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 4000,
            Start       = 9200,
            Size        = 2048000,
            Offset      = 4710400,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 37600,
            Start       = 13200,
            Size        = 19251200,
            Offset      = 6758400,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 12000,
            Start       = 13200,
            Size        = 6144000,
            Offset      = 6758400,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 25600,
            Start       = 25200,
            Size        = 13107200,
            Offset      = 6758400,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 102,
            Start       = 890072,
            Size        = 50800,
            Offset      = 26009600,
            Sequence    = 8
        }
    };

    readonly Partition[] RD31 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9700,
            Start       = 0,
            Size        = 4966400,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 9700,
            Size        = 51200,
            Offset      = 4966400,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3000,
            Start       = 9800,
            Size        = 1536000,
            Offset      = 5017600,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 28728,
            Start       = 12800,
            Size        = 14708736,
            Offset      = 6553600,
            Sequence    = 6
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 32,
            Start       = 41528,
            Size        = 16384,
            Offset      = 21262336,
            Sequence    = 8
        }
    };

    readonly Partition[] RD32 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9700,
            Start       = 0,
            Size        = 4966400,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 17300,
            Start       = 9700,
            Size        = 102400,
            Offset      = 4966400,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 27000,
            Size        = 51200,
            Offset      = 13824000,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3000,
            Start       = 27100,
            Size        = 1536000,
            Offset      = 13875200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 53072,
            Start       = 30100,
            Size        = 27172864,
            Offset      = 15411200,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 32,
            Start       = 83172,
            Size        = 16384,
            Offset      = 42584064,
            Sequence    = 8
        }
    };

    readonly Partition[] RD51 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 7460,
            Start       = 0,
            Size        = 4608000,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 40,
            Start       = 0,
            Size        = 20480,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 2200,
            Start       = 0,
            Size        = 1126400,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 11868,
            Start       = 9700,
            Size        = 6076416,
            Offset      = 6758400,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 32,
            Start       = 21568,
            Size        = 16384,
            Offset      = 11042816,
            Sequence    = 8
        }
    };

    readonly Partition[] RD52 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9700,
            Start       = 0,
            Size        = 4966400,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 17300,
            Start       = 9700,
            Size        = 8857600,
            Offset      = 4966400,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 27000,
            Size        = 51200,
            Offset      = 13824000,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3000,
            Start       = 27100,
            Size        = 1536000,
            Offset      = 13875200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 30348,
            Start       = 30100,
            Size        = 15538176,
            Offset      = 15411200,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 32,
            Start       = 60448,
            Size        = 16384,
            Offset      = 30949376,
            Sequence    = 8
        }
    };

    readonly Partition[] RD53 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9700,
            Start       = 0,
            Size        = 4966400,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 17300,
            Start       = 9700,
            Size        = 8857600,
            Offset      = 4966400,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 27000,
            Size        = 51200,
            Offset      = 13824000,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3000,
            Start       = 27100,
            Size        = 1536000,
            Offset      = 13875200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 108540,
            Start       = 30100,
            Size        = 55572480,
            Offset      = 15411200,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 32,
            Start       = 138640,
            Size        = 16384,
            Offset      = 70983680,
            Sequence    = 8
        }
    };

    readonly Partition[] RD54 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9700,
            Start       = 0,
            Size        = 4966400,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 17300,
            Start       = 9700,
            Size        = 8857600,
            Offset      = 4966400,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 27000,
            Size        = 51200,
            Offset      = 13824000,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3000,
            Start       = 27100,
            Size        = 1536000,
            Offset      = 13875200,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 281068,
            Start       = 30100,
            Size        = 143906816,
            Offset      = 15411200,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "maintenance area",
            Type        = "maintenance",
            Length      = 32,
            Start       = 311168,
            Size        = 16384,
            Offset      = 159318016,
            Sequence    = 8
        }
    };

    readonly Partition[] RK06 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 7920,
            Start       = 0,
            Size        = 4055040,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 7920,
            Size        = 51200,
            Offset      = 4055040,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 2936,
            Start       = 8020,
            Size        = 1503232,
            Offset      = 4106240,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 16126,
            Start       = 10956,
            Size        = 8256512,
            Offset      = 5609472,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "bad sector file",
            Type        = "bad",
            Length      = 44,
            Start       = 27082,
            Size        = 22528,
            Offset      = 13865984,
            Sequence    = 8
        }
    };

    readonly Partition[] RK07 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 7920,
            Start       = 0,
            Size        = 4055040,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 7920,
            Size        = 51200,
            Offset      = 4055040,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 2936,
            Start       = 8020,
            Size        = 1503232,
            Offset      = 4106240,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 42790,
            Start       = 10956,
            Size        = 21908480,
            Offset      = 5609472,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "bad sector file",
            Type        = "bad",
            Length      = 44,
            Start       = 53746,
            Size        = 22528,
            Offset      = 27517952,
            Sequence    = 8
        }
    };

    readonly Partition[] RM02 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9120,
            Start       = 0,
            Size        = 4669440,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 9120,
            Size        = 102400,
            Offset      = 4669440,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 5400,
            Start       = 9320,
            Size        = 2764800,
            Offset      = 2764800,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 5600,
            Start       = 29120,
            Size        = 2867200,
            Offset      = 14909440,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 96896,
            Start       = 34720,
            Size        = 49610752,
            Offset      = 17776640,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 32160,
            Start       = 34720,
            Size        = 16465920,
            Offset      = 17776640,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 32160,
            Start       = 66880,
            Size        = 16465920,
            Offset      = 34242560,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 32576,
            Start       = 99040,
            Size        = 16678912,
            Offset      = 50708480,
            Sequence    = 6
        },
        new()
        {
            Description = null,
            Name        = "bad sector file",
            Type        = "bad",
            Length      = 64,
            Start       = 131616,
            Size        = 32768,
            Offset      = 67387392,
            Sequence    = 8
        }
    };

    readonly Partition[] RM05 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 10336,
            Start       = 0,
            Size        = 5292032,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 21280,
            Start       = 10336,
            Size        = 10895360,
            Offset      = 5292032,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 31616,
            Size        = 102400,
            Offset      = 16187392,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 6388,
            Start       = 31816,
            Size        = 3270656,
            Offset      = 16289792,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 462016,
            Start       = 38304,
            Size        = 236552192,
            Offset      = 19611648,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 153824,
            Start       = 38304,
            Size        = 78757888,
            Offset      = 19611648,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 153824,
            Start       = 192128,
            Size        = 78757888,
            Offset      = 98369536,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 154368,
            Start       = 192128,
            Size        = 79036416,
            Offset      = 98369536,
            Sequence    = 6
        },
        new()
        {
            Description = null,
            Name        = "bad sector file",
            Type        = "bad",
            Length      = 64,
            Start       = 421312,
            Size        = 32768,
            Offset      = 215711744,
            Sequence    = 8
        }
    };

    readonly Partition[] RP02 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 8400,
            Start       = 0,
            Size        = 4300800,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 8400,
            Size        = 51200,
            Offset      = 4300800,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3100,
            Start       = 8500,
            Size        = 1587200,
            Offset      = 4352000,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 28400,
            Start       = 11600,
            Size        = 14540800,
            Offset      = 5939200,
            Sequence    = 2
        }
    };

    readonly Partition[] RP03 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 8400,
            Start       = 0,
            Size        = 4300800,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 100,
            Start       = 8400,
            Size        = 51200,
            Offset      = 4300800,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 3100,
            Start       = 8500,
            Size        = 1587200,
            Offset      = 4352000,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 68400,
            Start       = 11600,
            Size        = 35020800,
            Offset      = 5939200,
            Sequence    = 2
        }
    };

    readonly Partition[] RP04 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9614,
            Start       = 0,
            Size        = 4922368,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 20064,
            Start       = 9614,
            Size        = 10272768,
            Offset      = 4922368,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 29678,
            Size        = 102400,
            Offset      = 15195136,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 6070,
            Start       = 29878,
            Size        = 1587200,
            Offset      = 15297536,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 135806,
            Start       = 35948,
            Size        = 69532672,
            Offset      = 18405376,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "bad sector file",
            Type        = "bad",
            Length      = 44,
            Start       = 171754,
            Size        = 22528,
            Offset      = 87938048,
            Sequence    = 8
        }
    };

    readonly Partition[] RP06 =
    {
        new()
        {
            Description = null,
            Name        = "/",
            Type        = "data",
            Length      = 9614,
            Start       = 0,
            Size        = 4922368,
            Offset      = 0,
            Sequence    = 0
        },
        new()
        {
            Description = null,
            Name        = "/usr",
            Type        = "data",
            Length      = 20064,
            Start       = 9614,
            Size        = 10272768,
            Offset      = 4922368,
            Sequence    = 1
        },
        new()
        {
            Description = null,
            Name        = "error log",
            Type        = "errorlog",
            Length      = 200,
            Start       = 29678,
            Size        = 102400,
            Offset      = 15195136,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "swap",
            Type        = "swap",
            Length      = 6070,
            Start       = 29878,
            Size        = 1587200,
            Offset      = 15297536,
            Sequence    = 2
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 135806,
            Start       = 35948,
            Size        = 69532672,
            Offset      = 18405376,
            Sequence    = 3
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 304678,
            Start       = 35948,
            Size        = 155995136,
            Offset      = 18405376,
            Sequence    = 4
        },
        new()
        {
            Description = null,
            Name        = "user",
            Type        = "data",
            Length      = 166828,
            Start       = 171798,
            Size        = 85415936,
            Offset      = 87960576,
            Sequence    = 5
        },
        new()
        {
            Description = null,
            Name        = "bad sector file",
            Type        = "bad",
            Length      = 44,
            Start       = 340626,
            Size        = 22528,
            Offset      = 174400512,
            Sequence    = 8
        }
    };

    /// <inheritdoc />
    public string Name => "UNIX hardwired";
    /// <inheritdoc />
    public Guid Id => new("9ED7E30B-53BF-4619-87A0-5D2002155617");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<Partition>();
        Partition[] parts;

        if(sectorOffset != 0)
            return false;

        switch(imagePlugin.Info.MediaType)
        {
            case MediaType.RA60:
                parts = RA60;

                break;
            case MediaType.RA80:
                parts = RA80;

                break;
            case MediaType.RA81:
                parts = RA81;

                break;
            case MediaType.RC25:
                parts = RC25;

                break;
            case MediaType.RD31:
                parts = RD31;

                break;
            case MediaType.RD32:
                parts = RD32;

                break;
            case MediaType.RD51:
                parts = RD51;

                break;
            case MediaType.RD52:
                parts = RD52;

                break;
            case MediaType.RD53:
                parts = RD53;

                break;
            case MediaType.RD54:
                parts = RD54;

                break;
            case MediaType.RK06:
                parts = RK06;

                break;
            case MediaType.RK07:
                parts = RK07;

                break;
            case MediaType.RM02:
            case MediaType.RM03:
                parts = RM02;

                break;
            case MediaType.RM05:
                parts = RM05;

                break;
            case MediaType.RP02:
                parts = RP02;

                break;
            case MediaType.RP03:
                parts = RP03;

                break;
            case MediaType.RP04:
            case MediaType.RP05:
                parts = RP04;

                break;
            case MediaType.RP06:
                parts = RP06;

                break;
            default: return false;
        }

        for(var i = 0; i < parts.Length; i++)
            parts[i].Scheme = "";

        partitions = parts.ToList();

        return partitions.Count > 0;
    }
}