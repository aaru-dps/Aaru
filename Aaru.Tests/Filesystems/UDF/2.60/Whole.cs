// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UDF._260
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Universal Disc Format", "2.60");
        public override IFilesystem _plugin     => new Aaru.Filesystems.UDF();
        public override bool        _partitions => false;

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
            1228800
        };

        public override uint[] _sectorSize => new uint[]
        {
            512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false
        };

        public override long[] _clusters => new long[]
        {
            1228800
        };

        public override uint[] _clusterSize => new uint[]
        {
            512
        };

        public override string[] _oemId => new[]
        {
            "*Apple Mac OS X UDF FS"
        };

        public override string[] _type => new[]

        {
            "UDF v2.60"
        };

        public override string[] _volumeName => new[]
        {
            "Volume label"
        };

        public override string[] _volumeSerial => new[]
        {
            "78CE3237 (Mac OS X newfs_udf) UDF Volume Set"
        };
    }
}