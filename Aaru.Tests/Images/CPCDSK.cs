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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class CPCDSK : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "3D Construction Kit (1991)(Domark)(Disk 1 of 2)[a].dsk.lz",
            "3D Construction Kit (1991)(Domark)(Disk 1 of 2).dsk.lz",
            "3D Construction Kit (1991)(Domark)(Disk 2 of 2)[a].dsk.lz",
            "3D Construction Kit (1991)(Domark)(Disk 2 of 2).dsk.lz", "3D Construction Kit (1991)(Domark).dsk.lz",
            "3D Construction Kit (1991)(Domark)[Objects Disk].dsk.lz",
            "BCPL Compiler v1.0 (1986)(Arnor)[CPM Version].dsk.lz", "BCPL Compiler v1.0 (1986)(Arnor).dsk.lz",
            "CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[a][CPM Version].dsk.lz",
            "CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[CPM Version].dsk.lz",
            "CPM Plus v1.0 (1985)(Amstrad)(Disk 2 of 4)[CPM Version].dsk.lz",
            "CPM Plus v1.0 (1985)(Amstrad)(Disk 3 of 4)[CPM Version].dsk.lz",
            "CPM Plus v1.0 (1985)(Amstrad)(Disk 4 of 4)[CPM Version].dsk.lz"
        };
        public override ulong[] _sectors => new ulong[]
        {
            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2)[a].dsk.lz
            360,

            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2).dsk.lz
            360,

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2)[a].dsk.lz
            360,

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2).dsk.lz
            360,

            // 3D Construction Kit (1991)(Domark).dsk.lz
            360,

            // 3D Construction Kit (1991)(Domark)[Objects Disk].dsk.lz
            387,

            // BCPL Compiler v1.0 (1986)(Arnor)[CPM Version].dsk.lz
            360,

            // BCPL Compiler v1.0 (1986)(Arnor).dsk.lz
            360,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[a][CPM Version].dsk.lz
            359,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[CPM Version].dsk.lz
            360,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 2 of 4)[CPM Version].dsk.lz
            360,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 3 of 4)[CPM Version].dsk.lz
            360,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 4 of 4)[CPM Version].dsk.lz
            360
        };
        public override uint[] _sectorSize => new uint[]
        {
            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2)[a].dsk.lz
            512,

            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2).dsk.lz
            512,

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2)[a].dsk.lz
            512,

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2).dsk.lz
            512,

            // 3D Construction Kit (1991)(Domark).dsk.lz
            512,

            // 3D Construction Kit (1991)(Domark)[Objects Disk].dsk.lz
            512,

            // BCPL Compiler v1.0 (1986)(Arnor)[CPM Version].dsk.lz
            512,

            // BCPL Compiler v1.0 (1986)(Arnor).dsk.lz
            512,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[a][CPM Version].dsk.lz
            512,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[CPM Version].dsk.lz
            512,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 2 of 4)[CPM Version].dsk.lz
            512,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 3 of 4)[CPM Version].dsk.lz
            512,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 4 of 4)[CPM Version].dsk.lz
            512
        };
        public override MediaType[] _mediaTypes => new[]
        {
            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2)[a].dsk.lz
            MediaType.CompactFloppy,

            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2).dsk.lz
            MediaType.CompactFloppy,

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2)[a].dsk.lz
            MediaType.CompactFloppy,

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2).dsk.lz
            MediaType.CompactFloppy,

            // 3D Construction Kit (1991)(Domark).dsk.lz
            MediaType.CompactFloppy,

            // 3D Construction Kit (1991)(Domark)[Objects Disk].dsk.lz
            MediaType.CompactFloppy,

            // BCPL Compiler v1.0 (1986)(Arnor)[CPM Version].dsk.lz
            MediaType.CompactFloppy,

            // BCPL Compiler v1.0 (1986)(Arnor).dsk.lz
            MediaType.CompactFloppy,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[a][CPM Version].dsk.lz
            MediaType.CompactFloppy,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[CPM Version].dsk.lz
            MediaType.CompactFloppy,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 2 of 4)[CPM Version].dsk.lz
            MediaType.CompactFloppy,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 3 of 4)[CPM Version].dsk.lz
            MediaType.CompactFloppy,

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 4 of 4)[CPM Version].dsk.lz
            MediaType.CompactFloppy
        };
        public override string[] _md5S => new[]
        {
            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2)[a].dsk.lz
            "ee601c0d2beade20bb5c04b3f5800ff6",

            // 3D Construction Kit (1991)(Domark)(Disk 1 of 2).dsk.lz
            "ee601c0d2beade20bb5c04b3f5800ff6",

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2)[a].dsk.lz
            "dcb039b3b2ff2d6bdef8bf6c13ef3f83",

            // 3D Construction Kit (1991)(Domark)(Disk 2 of 2).dsk.lz
            "dcb039b3b2ff2d6bdef8bf6c13ef3f83",

            // 3D Construction Kit (1991)(Domark).dsk.lz
            "e1b14e9b744b08a1b2b56fa25f034682",

            // 3D Construction Kit (1991)(Domark)[Objects Disk].dsk.lz
            "82007217a3aa6bb91468b71a6dc4bfe5",

            // BCPL Compiler v1.0 (1986)(Arnor)[CPM Version].dsk.lz
            "acd60bb0119e0b5aa1790bef344211ac",

            // BCPL Compiler v1.0 (1986)(Arnor).dsk.lz
            "0330956c2fe38f278d7cba6f7bd8aa2d",

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[a][CPM Version].dsk.lz
            "a568e44f556661f9e4b7db01c126c676",

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 1 of 4)[CPM Version].dsk.lz
            "5dc0d482a773043d8683a84c8220df95",

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 2 of 4)[CPM Version].dsk.lz
            "64edd62fabb381ef49bf3a8f43435824",

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 3 of 4)[CPM Version].dsk.lz
            "b381bbc72ab664d658ddd5898c7ff266",

            // CPM Plus v1.0 (1985)(Amstrad)(Disk 4 of 4)[CPM Version].dsk.lz
            "ad43345ac469844465da6d73369cc6b1"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CPCDSK");
        public override IMediaImage _plugin => new Cpcdsk();
    }
}