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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/
// ReSharper disable CheckNamespace

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UDF._201
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Filesystems", "Universal Disc Format", "2.01");
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
                Type         = "UDF v2.01",
                VolumeName   = "Volume label",
                VolumeSerial = "595c5d0bee60c3bbLinuxUDF"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 819200,
                SectorSize   = 512,
                Clusters     = 819200,
                ClusterSize  = 512,
                SystemId     = "*Apple Mac OS X UDF FS",
                Type         = "UDF v2.01",
                VolumeName   = "Volume label",
                VolumeSerial = "48847EB3 (Mac OS X newfs_udf) UDF Volume Set"
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
                Type         = "UDF v2.01",
                VolumeName   = "anonymous",
                VolumeSerial = "5bc935e11b089104"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_udf_2.01_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 1024000,
                ClusterSize  = 512,
                SystemId     = "*Linux UDFFS",
                Type         = "UDF v2.01",
                VolumeName   = "DicSetter",
                VolumeSerial = "5cc8816fcb3a3b38LinuxUDF"
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
                Type         = "UDF v2.01",
                VolumeName   = "anonymous",
                VolumeSerial = "60a80c385dda0f96"
            }
        };
    }
}