// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser3.cs
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

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Reiser3 : FilesystemTest
    {
        public Reiser3() : base(null) {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                           "Reiser filesystem v3");
        public override IFilesystem _plugin     => new Reiser();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "linux_r3.5.aif", "linux_r3.6.aif", "linux_4.19_reiser_3.5_flashdrive.aif",
            "linux_4.19_reiser_3.6_flashdrive.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            262144, 262144, 1024000, 1024000
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
            32512, 32512, 127744, 127744
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096
        };
        public override string[] _oemId => null;
        public override string[] _type => new[]
        {
            "Reiser 3.5 filesystem", "Reiser 3.6 filesystem", "Reiser 3.5 filesystem", "Reiser 3.6 filesystem"
        };
        public override string[] _volumeName => new[]
        {
            null, "Volume label", null, "DicSetter"
        };
        public override string[] _volumeSerial => new[]
        {
            null, "844155c0-c854-d34e-8133-26ffac2e7b5d", null, "8902ac3c-3e0c-4c4c-84ec-03405c1710f1"
        };
    }
}