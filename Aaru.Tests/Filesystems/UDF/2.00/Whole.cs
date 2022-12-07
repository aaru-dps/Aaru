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

namespace Aaru.Tests.Filesystems.UDF._200;

[TestFixture]
public class Whole : FilesystemTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "Universal Disc Format", "2.00");
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
            Type         = "udf",
            VolumeName   = "Volume label",
            VolumeSerial = "595c5d07f4fc8e8dLinuxUDF"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 614400,
            SectorSize   = 512,
            Clusters     = 614400,
            ClusterSize  = 512,
            SystemId     = "*Apple Mac OS X UDF FS",
            Type         = "udf",
            VolumeName   = "Volume label",
            VolumeSerial = "5D91CB4F (Mac OS X newfs_udf) UDF Volume Set"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_udf_2.00_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 1024000,
            ClusterSize  = 512,
            SystemId     = "*Linux UDFFS",
            Type         = "udf",
            VolumeName   = "DicSetter",
            VolumeSerial = "5cc7f4183e0d5f7aLinuxUDF"
        }
    };
}