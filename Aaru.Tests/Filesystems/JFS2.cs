// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : JFS2.cs
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
    public class Jfs2 : FilesystemTest
    {
        public Jfs2() : base("JFS filesystem") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "JFS2");
        public override IFilesystem _plugin     => new JFS();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "linux.aif", "linux_caseinsensitive.aif", "ecs20_fstester.aif", "linux_4.19_jfs_flashdrive.aif",
            "linux_4.19_jfs_os2_flashdrive.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            262144, 262144, 1024000, 1024000, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true, true, true, true, true
        };

        public override long[] _clusters => new long[]
        {
            257632, 257632, 1017512, 1017416, 1017416
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096, 4096
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Volume labe", "Volume labe", "Volume labe", "DicSetter", "DicSetter"
        };

        public override string[] _volumeSerial => new[]
        {
            "8033b783-0cd1-1645-8ecc-f8f113ad6a47", "d6cd91e9-3899-7e40-8468-baab688ee2e2",
            "f4077ce9-0000-0000-0000-000000007c10", "91746c77-eb51-7441-85e2-902c925969f8",
            "08fc8e22-0201-894e-89c9-31ec3f546203"
        };
    }
}