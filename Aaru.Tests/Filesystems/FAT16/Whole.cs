// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT16.cs
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
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT16
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base("FAT16") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16");

        public override IFilesystem _plugin     => new FAT();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            // MS-DOS 3.30A
            "msdos_3.30A_mf2ed.img.lz",

            // MS-DOS 3.31
            "msdos_3.31_mf2ed.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // MS-DOS 3.30A
            MediaType.DOS_35_ED,

            // MS-DOS 3.31
            MediaType.DOS_35_ED
        };

        public override ulong[] _sectors => new ulong[]
        {
            // MS-DOS 3.30A
            5760,

            // MS-DOS 3.31
            5760
        };

        public override uint[] _sectorSize => new uint[]
        {
            // MS-DOS 3.30A
            512,

            // MS-DOS 3.31
            512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true, true
        };

        public override long[] _clusters => new long[]
        {
            // MS-DOS 3.30A
            5760,

            // MS-DOS 3.31
            5760
        };

        public override uint[] _clusterSize => new uint[]
        {
            // MS-DOS 3.30A
            512,

            // MS-DOS 3.31
            512
        };
        public override string[] _oemId => new[]
        {
            // MS-DOS 3.30A
            "MSDOS3.3",

            // MS-DOS 3.31
            "IBM  3.3"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new string[]
        {
            // MS-DOS 3.30A
            null,

            // MS-DOS 3.31
            null
        };

        public override string[] _volumeSerial => new string[]
        {
            // MS-DOS 3.30A
            null,

            // MS-DOS 3.31
            null
        };
    }
}