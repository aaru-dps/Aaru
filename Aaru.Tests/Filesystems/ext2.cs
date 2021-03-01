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

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "linux_ext2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Clusters     = 130048,
                ClusterSize  = 1024,
                Type         = "ext2",
                VolumeName   = "VolumeLabel",
                VolumeSerial = "8e3992cf-7d98-e44a-b753-0591a35913eb"
            },
            new FileSystemTest
            {
                TestFile     = "linux_ext3.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Clusters     = 130048,
                ClusterSize  = 1024,
                Type         = "ext3",
                VolumeName   = "VolumeLabel",
                VolumeSerial = "1b411516-5415-4b42-95e6-1a247056a960"
            },
            new FileSystemTest
            {
                TestFile     = "linux_ext4.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Clusters     = 130048,
                ClusterSize  = 1024,
                Type         = "ext4",
                VolumeName   = "VolumeLabel",
                VolumeSerial = "b2f8f305-770f-ad47-abe4-f0484aa319e9"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_7.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8388608,
                SectorSize   = 512,
                Clusters     = 1046567,
                ClusterSize  = 4096,
                Type         = "ext2",
                VolumeName   = "Volume label",
                VolumeSerial = "e72aee05-627b-11e7-a573-0800272a08ec"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_7.1_r0.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 2097152,
                SectorSize   = 512,
                Clusters     = 260135,
                ClusterSize  = 4096,
                Type         = "ext2",
                VolumeName   = "Volume label",
                VolumeSerial = "072756f2-627c-11e7-a573-0800272a08ec"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_ext2_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 510976,
                ClusterSize  = 1024,
                Type         = "ext2",
                VolumeName   = "DicSetter",
                VolumeSerial = "f5b2500f-99fb-764b-a6c4-c4db0b98a653"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_ext3_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 510976,
                ClusterSize  = 1024,
                Type         = "ext3",
                VolumeName   = "DicSetter",
                VolumeSerial = "a3914b55-260f-7245-8c72-7ccdf45436cb"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_ext4_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 510976,
                ClusterSize  = 1024,
                Type         = "ext4",
                VolumeName   = "DicSetter",
                VolumeSerial = "10413797-43d1-6545-8fbc-6ebc9d328be9"
            }
        };
    }
}