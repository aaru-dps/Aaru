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

namespace Aaru.Tests.Filesystems.UDF._201
{
    [TestFixture]
    public class Optical : FilesystemTest
    {
        public Optical() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Universal Disc Format", "2.01");
        public override IFilesystem _plugin     => new Aaru.Filesystems.UDF();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "ecs20.aif", "ecs20_cdrw.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.DVDPR, MediaType.CDRW
        };

        public override ulong[] _sectors => new ulong[]
        {
            2295104, 295264
        };

        public override uint[] _sectorSize => new uint[]
        {
            2048, 2048
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false
        };

        public override long[] _clusters => new long[]
        {
            2295104, 295264
        };

        public override uint[] _clusterSize => new uint[]
        {
            2048, 2048
        };

        public override string[] _oemId => new[]
        {
            "*ExpressUDF", "*ExpressUDF"
        };
        public override string[] _type => new[]
        {
            "UDF v2.01", "UDF v2.01"
        };

        public override string[] _volumeName => new[]
        {
            "VolLabel", "UDF5A5DFF10"
        };

        public override string[] _volumeSerial => new[]
        {
            "VolumeSetId", "Volume Set ID not specified"
        };
    }
}