// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : F2FS.cs
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
    public class F2Fs : FilesystemTest
    {
        public F2Fs() : base("F2FS filesystem") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "F2FS");
        public override IFilesystem _plugin     => new F2FS();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "linux.aif", "linux_4.19_f2fs_flashdrive.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            262144, 2097152
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false
        };

        public override long[] _clusters => new long[]
        {
            32512, 261888
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "VolumeLabel", "DicSetter"
        };

        public override string[] _volumeSerial => new[]
        {
            "81bd3a4e-de0c-484c-becc-aaa479b2070a", "422bd2a8-68ab-6f45-9a04-9c264d07dd6e"
        };
    }
}