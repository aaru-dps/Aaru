// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFS.cs
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

namespace Aaru.Tests.Filesystems.HFS
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("HFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS (MBR)");
        public override IFilesystem _plugin => new AppleHFS();
        public override bool _partitions => true;

        public override string[] _testFiles => new[]
        {
            "linux.aif", "darwin_1.3.1.aif", "darwin_1.4.1.aif", "darwin_6.0.2.aif", "darwin_8.0.1.aif",
            "linux_4.19_hfs_flashdrive.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            262144, 409600, 409600, 409600, 409600, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            65018, 51145, 51145, 58452, 58502, 63870
        };

        public override uint[] _clusterSize => new uint[]
        {
            2048, 4096, 4096, 3584, 3584, 8192
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "DicSetter"
        };

        public override string[] _volumeSerial => new[]
        {
            null, null, null, null, "81FE805D61458753", null
        };
    }
}