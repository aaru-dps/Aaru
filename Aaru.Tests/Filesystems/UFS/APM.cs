// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
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

namespace Aaru.Tests.Filesystems.UFS
{
    [TestFixture]
    public class APM : FilesystemTest
    {
        public APM() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "UNIX filesystem (APM)");
        public override IFilesystem _plugin     => new FFSPlugin();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "ffs43/darwin_1.3.1.aif", "ffs43/darwin_1.4.1.aif", "ffs43/darwin_6.0.2.aif", "ffs43/darwin_8.0.1.aif",
            "ufs1/darwin_1.3.1.aif", "ufs1/darwin_1.4.1.aif", "ufs1/darwin_6.0.2.aif", "ufs1/darwin_8.0.1.aif",
            "ufs1/macosx_10.2.aif", "ufs1/macosx_10.3.aif", "ufs1/macosx_10.4.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            1024000, 1024000, 1024000, 1024000, 204800, 204800, 204800, 204800, 2097152, 2097152, 2097152
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            511488, 511488, 511488, 511488, 102368, 102368, 102368, 102368, 1047660, 1038952, 1038952
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024
        };
        public override string[] _oemId => null;

        public override string[] _type => new[]
        {
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS"
        };

        public override string[] _volumeName => new string[]
        {
            null, null, null, null, null, null, null, null, null, null, null
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null, null, null, null, null, null, null
        };
    }
}