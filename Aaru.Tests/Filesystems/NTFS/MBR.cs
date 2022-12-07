// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NTFS.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.NTFS;

[TestFixture]
public class MBR : FilesystemTest
{
    public MBR() : base("ntfs") {}

    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "New Technology File System (MBR)");

    public override IFilesystem Plugin     => new Aaru.Filesystems.NTFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "win10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 524288,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65263,
            ClusterSize  = 4096,
            VolumeSerial = "C46C1B3C6C1B28A6"
        },
        new FileSystemTest
        {
            TestFile     = "win2000.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 2097152,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1046511,
            ClusterSize  = 1024,
            VolumeSerial = "8070C8EC70C8E9CC"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_3.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1023057,
            ClusterSize  = 512,
            VolumeSerial = "10CC6AC6CC6AA5A6"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_3.50.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 524288,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524256,
            ClusterSize  = 512,
            VolumeSerial = "7A14F50014F4BFE5"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_3.51.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 524288,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524256,
            ClusterSize  = 512,
            VolumeSerial = "24884447884419A6"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            VolumeSerial = "DE047EC1047E9C69"
        },
        new FileSystemTest
        {
            TestFile     = "winvista.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 524288,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 64767,
            ClusterSize  = 4096,
            VolumeSerial = "E20AF54B0AF51D6B"
        },
        new FileSystemTest
        {
            TestFile     = "linux.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32511,
            ClusterSize  = 4096,
            VolumeSerial = "065BB96B7C1BCFDA"
        },
        new FileSystemTest
        {
            TestFile     = "haiku_hrev51259.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32511,
            ClusterSize  = 4096,
            VolumeSerial = "323BED1E2A2FF4D5"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_ntfs3g_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 127743,
            ClusterSize  = 4096,
            VolumeSerial = "1FC3802B52F9611C"
        }
    };
}