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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/
// ReSharper disable CheckNamespace

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UDF._102
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Filesystems", "Universal Disc Format", "1.02");
        public override IFilesystem Plugin     => new Aaru.Filesystems.UDF();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "linux.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 1024000,
                ClusterSize  = 512,
                SystemId     = "*Linux UDFFS",
                Type         = "UDF v1.02",
                VolumeName   = "Volume label",
                VolumeSerial = "595c5cfa38ce8b66LinuxUDF"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 204800,
                SectorSize   = 512,
                Clusters     = 204800,
                ClusterSize  = 512,
                SystemId     = "*Apple Mac OS X UDF FS",
                Type         = "UDF v1.02",
                VolumeName   = "Volume label",
                VolumeSerial = "6D02A231 (Mac OS X newfs_udf) UDF Volume Set"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_udf_1.02_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 1024000,
                ClusterSize  = 512,
                SystemId     = "*Linux UDFFS",
                Type         = "UDF v2.01",
                VolumeName   = "DicSetter",
                VolumeSerial = "5cc7882441a86e93LinuxUDF"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_6.1.5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Clusters     = 262144,
                ClusterSize  = 512,
                SystemId     = "*NetBSD userland UDF",
                Type         = "UDF v1.50",
                VolumeName   = "anonymous",
                VolumeSerial = "47b062a138bcfb83"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_7.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Clusters     = 262144,
                ClusterSize  = 512,
                SystemId     = "*NetBSD userland UDF",
                Type         = "UDF v1.50",
                VolumeName   = "anonymous",
                VolumeSerial = "382885b272e72ab8"
            }
        };
    }
}