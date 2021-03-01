// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ext2.cs
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
    public class Ext2 : FilesystemTest
    {
        public Ext2() : base(null) {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "ext2");
        public override IFilesystem _plugin     => new ext2FS();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "linux_ext2.aif", "linux_ext3.aif", "linux_ext4.aif", "netbsd_7.1.aif", "netbsd_7.1_r0.aif",
            "linux_4.19_ext2_flashdrive.aif", "linux_4.19_ext3_flashdrive.aif", "linux_4.19_ext4_flashdrive.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            262144, 262144, 262144, 8388608, 2097152, 1024000, 1024000, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            130048, 130048, 130048, 1046567, 260135, 510976, 510976, 510976
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024, 4096, 4096, 1024, 1024, 1024
        };
        public override string[] _oemId => null;

        public override string[] _type => new[]
        {
            "ext2", "ext3", "ext4", "ext2", "ext2", "ext2", "ext3", "ext4"
        };

        public override string[] _volumeName => new[]
        {
            "VolumeLabel", "VolumeLabel", "VolumeLabel", "Volume label", "Volume label", "DicSetter", "DicSetter",
            "DicSetter"
        };

        public override string[] _volumeSerial => new[]
        {
            "8e3992cf-7d98-e44a-b753-0591a35913eb", "1b411516-5415-4b42-95e6-1a247056a960",
            "b2f8f305-770f-ad47-abe4-f0484aa319e9", "e72aee05-627b-11e7-a573-0800272a08ec",
            "072756f2-627c-11e7-a573-0800272a08ec", "f5b2500f-99fb-764b-a6c4-c4db0b98a653",
            "a3914b55-260f-7245-8c72-7ccdf45436cb", "10413797-43d1-6545-8fbc-6ebc9d328be9"
        };
    }
}