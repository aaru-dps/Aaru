// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CPCDSK.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CPCDSK : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "CPCDSK");
    public override IMediaImage Plugin     => new Cpcdsk();

    public override BlockImageTestExpected[] Tests =>
    [
        new BlockImageTestExpected
        {
            TestFile   = "3D Construction Kit (1991)(Domark)(Disk 1 of 2)[a].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "ee601c0d2beade20bb5c04b3f5800ff6",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "3D Construction Kit (1991)(Domark)(Disk 1 of 2).dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "ee601c0d2beade20bb5c04b3f5800ff6",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "3D Construction Kit (1991)(Domark)(Disk 2 of 2)[a].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "dcb039b3b2ff2d6bdef8bf6c13ef3f83",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "3D Construction Kit (1991)(Domark)(Disk 2 of 2).dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "dcb039b3b2ff2d6bdef8bf6c13ef3f83",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        /* TODO: Does not open
        new BlockImageTestExpected
        {
            TestFile   = "3D Construction Kit (1991)(Domark).dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "e1b14e9b744b08a1b2b56fa25f034682"
        },
        */ new BlockImageTestExpected
        {
            TestFile   = "3D Construction Kit (1991)(Domark)[Objects Disk].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 387,
            SectorSize = 512,
            Md5        = "82007217a3aa6bb91468b71a6dc4bfe5",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 387
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "BCPL Compiler v1.0 (1986)(Arnor)[CPM Version].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "acd60bb0119e0b5aa1790bef344211ac",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "BCPL Compiler v1.0 (1986)(Arnor).dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "0330956c2fe38f278d7cba6f7bd8aa2d",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[a][CPM Version].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 359,
            SectorSize = 512,
            Md5        = "a568e44f556661f9e4b7db01c126c676"
        },
        new BlockImageTestExpected
        {
            TestFile   = "CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[CPM Version].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "5dc0d482a773043d8683a84c8220df95",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "CPM Plus v1.0 (1985)(Amstrad)(Disk 2 of 4)[CPM Version].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "64edd62fabb381ef49bf3a8f43435824",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "CPM Plus v1.0 (1985)(Amstrad)(Disk 3 of 4)[CPM Version].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "b381bbc72ab664d658ddd5898c7ff266",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "CPM Plus v1.0 (1985)(Amstrad)(Disk 4 of 4)[CPM Version].dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "ad43345ac469844465da6d73369cc6b1",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "z88dk_cpc.dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "6e4ab38fcc5dc2d8173173dcbf8ca2e1",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            ]
        },
        new BlockImageTestExpected
        {
            TestFile   = "z88dk_pcw40.dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "37cfac07eb636ca8181878a06101a955"
        },
        new BlockImageTestExpected
        {
            TestFile   = "z88dk_pcw80.dsk.lz",
            MediaType  = MediaType.CompactFloppy,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "17b79ecfd045d1d5d3526b182b32064a",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1440
                }
            ]
        }
    ];
}