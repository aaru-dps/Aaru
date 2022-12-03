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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class Ext2 : FilesystemTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "ext2");
    public override IFilesystem Plugin     => new ext2FS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "netbsd_6.1.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131040,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "a4a95973-ab77-11eb-811f-08002792bfed"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_6.1.5_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131040,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "19b1dda1-ab75-11eb-a6ba-08002792bfed"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_7.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 130540,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "7f49b17e-ab5d-11eb-bc21-0800272a08ec"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_7.1_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131040,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "e2601450-ab5c-11eb-a154-0800272a08ec"
        },
        new FileSystemTest
        {
            TestFile     = "openbsd_4.7.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131040,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "0b2bb462-17ac-c444-8ff0-ab10537ae902"
        },
        new FileSystemTest
        {
            TestFile     = "openbsd_4.7_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131040,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "32e4de93-37f9-6747-9650-ae7b1f1bb901"
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
            TestFile    = "linux_2.0.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131008,
            ClusterSize = 1024,
            Type        = "ext2"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.29.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "a81d28a3-83b2-eb11-9ae7-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.29_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1a4fbe63-84b2-eb11-9bda-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.34.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "00d2a5e7-fab2-eb11-8eae-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.34_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "3e1dfd3a-f9b2-eb11-8384-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "ccac0734-e6b3-eb11-885b-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "Volume label",
            VolumeSerial = "a85e3131-e6b3-eb11-886c-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.38.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "e48311a9-0fb3-eb11-83ed-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.38_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "86e00f25-0fb3-eb11-8f77-525400123456"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "9bdfe82d-778f-c64a-816c-a5ab33eb4f52"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "6248975a-8692-bc42-bf66-22790157be29"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "e7263487-cec2-3444-8e58-218181083a89"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "d227254f-b684-2244-9ad1-23b95e331c95"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "7f1a86dd-18a7-7d4e-aef0-728b1f674001"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18_r0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext2",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "c9e61e78-98c3-844e-98d5-d54b880d07cb"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18_ext3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext3",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "40bb6664-4e0e-ca4b-a328-7c744ddd8bf4"
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