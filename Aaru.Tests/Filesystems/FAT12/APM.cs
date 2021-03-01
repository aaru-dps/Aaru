// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT12.cs
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

namespace Aaru.Tests.Filesystems.FAT12
{
    [TestFixture]
    public class APM : FilesystemTest
    {
        public APM() : base("FAT12") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT12 (APM)");
        public override IFilesystem _plugin     => new FAT();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "macosx_10.11.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            16384
        };

        public override uint[] _sectorSize => new uint[]
        {
            512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true
        };

        public override long[] _clusters => new long[]
        {
            4076
        };

        public override uint[] _clusterSize => new uint[]
        {
            2048
        };
        public override string[] _oemId => new[]
        {
            "BSD  4.4"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new[]
        {
            "VOLUMELABEL"
        };

        public override string[] _volumeSerial => new[]
        {
            "32181F09"
        };
    }
}