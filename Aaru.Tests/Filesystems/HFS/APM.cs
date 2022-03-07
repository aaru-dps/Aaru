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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Filesystems.HFS;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class APM : FilesystemTest
{
    public APM() : base("HFS") {}

    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS (APM)");
    public override IFilesystem Plugin     => new AppleHFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "amigaos_3.9.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 1024128,
            SectorSize  = 512,
            Clusters    = 64003,
            ClusterSize = 8192,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32758,
            ClusterSize = 4096,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32758,
            ClusterSize = 4096,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.4.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32758,
            ClusterSize = 4096,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65514,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65514,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "AE72FE7C300796B3"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65514,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "5D4A28AA69D62082"
        },
        new FileSystemTest
        {
            TestFile    = "macos_1.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 41820,
            SectorSize  = 512,
            Clusters    = 41788,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_2.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 41820,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_4.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 41820,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_4.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 41820,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 41820,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 54840,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 54840,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.4.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 54840,
            SectorSize  = 512,
            Clusters    = 7673,
            ClusterSize = 512,
            VolumeName  = "Test disk"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.5.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 54840,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.7.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 81648,
            SectorSize  = 512,
            Clusters    = 39991,
            ClusterSize = 1024,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_6.0.8.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 54840,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_7.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 54840,
            SectorSize  = 512,
            Clusters    = 38950,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_7.1.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65504,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_7.5.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Bootable    = true,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65350,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_7.5.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65352,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_7.6.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65352,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_8.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Bootable    = true,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65352,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_8.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Bootable    = true,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65352,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.0.4.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.2.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.2.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32537,
            ClusterSize  = 4096,
            VolumeName   = "Volume label",
            VolumeSerial = "BD1F12F12468C949"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32537,
            ClusterSize  = 4096,
            VolumeName   = "Volume label",
            VolumeSerial = "6AC7046792447A85"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65072,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "8D00F9766E58A900"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65070,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "4D9FC602A1273D8D"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65070,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "B28973FD0129BD10"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 409600,
            SectorSize  = 512,
            Clusters    = 58506,
            ClusterSize = 3584,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "d2_driver.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 51200,
            SectorSize  = 512,
            Clusters    = 50926,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "hdt_1.8.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 51200,
            SectorSize  = 512,
            Clusters    = 50094,
            ClusterSize = 512,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "parted.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 46071,
            ClusterSize = 1024,
            VolumeName  = "Untitled"
        },
        new FileSystemTest
        {
            TestFile    = "silverlining_2.2.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 51200,
            SectorSize  = 512,
            Clusters    = 50382,
            ClusterSize = 512,
            VolumeName  = "Untitled  #1"
        },
        new FileSystemTest
        {
            TestFile    = "speedtools_3.6.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 51200,
            SectorSize  = 512,
            Clusters    = 49135,
            ClusterSize = 512,
            VolumeName  = "24 MB Disk"
        },
        new FileSystemTest
        {
            TestFile    = "nextstep_3.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65224,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "aux_3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65504,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "morphos_3.13.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65499,
            ClusterSize = 2048,
            VolumeName  = "VolumeLabel"
        }
    };
}