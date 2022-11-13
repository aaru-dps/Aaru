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

namespace Aaru.Tests.Filesystems.UDF._260;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class Whole : FilesystemTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "Universal Disc Format", "2.60");
    public override IFilesystem Plugin     => new UDF();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1228800,
            SectorSize   = 512,
            Clusters     = 1228800,
            ClusterSize  = 512,
            SystemId     = "*Apple Mac OS X UDF FS",
            Type         = "UDF v2.60",
            VolumeName   = "Volume label",
            VolumeSerial = "78CE3237 (Mac OS X newfs_udf) UDF Volume Set"
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
            Type         = "UDF v2.60",
            VolumeName   = "anonymous",
            VolumeSerial = "10f24d1248067621"
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
            Type         = "UDF v2.60",
            VolumeName   = "anonymous",
            VolumeSerial = "05f537510deab1e7"
        }
    };
}