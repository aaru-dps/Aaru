// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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

namespace Aaru.Tests.Filesystems.MINIX.V2
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v2 filesystem");
        public override IFilesystem _plugin     => new MinixFS();
        public override bool        _partitions => false;
        public override string[] _testFiles => new[]
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD
        };

        public override ulong[] _sectors => new ulong[]
        {
            720, 2400, 1440, 2880
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            360, 1200, 720, 1440
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024, 1024
        };
        public override string[] _oemId => null;

        public override string[] _type => new[]
        {
            "Minix 3 v2", "Minix 3 v2", "Minix 3 v2", "Minix 3 v2"
        };
        public override string[] _volumeName => new string[]
        {
            null, null, null, null
        };
        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null
        };
    }
}