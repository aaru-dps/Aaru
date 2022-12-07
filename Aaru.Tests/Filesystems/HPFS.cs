// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class Hpfs : FilesystemTest
{
    public Hpfs() : base("hpfs") {}

    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems",
                                                      "High Performance File System");
    public override IFilesystem Plugin     => new HPFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "ecs.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 261072,
            ClusterSize  = 512,
            SystemId     = "IBM 4.50",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2BBBD814"
        },
        new FileSystemTest
        {
            TestFile     = "msos2_1.21.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1023056,
            ClusterSize  = 512,
            SystemId     = "OS2 10.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "AC0DDC15"
        },
        new FileSystemTest
        {
            TestFile     = "msos2_1.30.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1023056,
            ClusterSize  = 512,
            SystemId     = "OS2 10.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "ABEB2C15"
        },
        new FileSystemTest
        {
            TestFile     = "os2_1.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 10.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "6D3D2815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_1.30.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 10.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "6D195815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.307.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "6D24F815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.514.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "6D27F415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.617.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 20.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "6D396415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_8.162.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "6D118415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_9.023.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "OS2 20.0",
            VolumeName   = "VOLUME LABE",
            VolumeSerial = "ACA08415"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_3.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "E851CB14"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_3.50.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262112,
            ClusterSize  = 512,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "A4EDC29C"
        },
        new FileSystemTest
        {
            TestFile     = "ecs20_fstester.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1022112,
            ClusterSize  = 512,
            SystemId     = "IBM 4.50",
            VolumeName   = "VOLUME LABE",
            VolumeSerial = "AC096014"
        }
    };
}