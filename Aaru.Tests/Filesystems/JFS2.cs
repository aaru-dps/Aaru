// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : JFS2.cs
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
public class Jfs2 : FilesystemTest
{
    public Jfs2() : base("jfs") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "JFS2");
    public override IFilesystem Plugin     => new JFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "linux.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 257632,
            ClusterSize  = 4096,
            VolumeName   = "Volume labe",
            VolumeSerial = "8033b783-0cd1-1645-8ecc-f8f113ad6a47"
        },
        new FileSystemTest
        {
            TestFile     = "linux_caseinsensitive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 257632,
            ClusterSize  = 4096,
            VolumeName   = "Volume labe",
            VolumeSerial = "d6cd91e9-3899-7e40-8468-baab688ee2e2"
        },
        new FileSystemTest
        {
            TestFile     = "ecs20_fstester.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1017512,
            ClusterSize  = 4096,
            VolumeName   = "Volume labe",
            VolumeSerial = "f4077ce9-0000-0000-0000-000000007c10"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_jfs_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1017416,
            ClusterSize  = 4096,
            VolumeName   = "DicSetter",
            VolumeSerial = "91746c77-eb51-7441-85e2-902c925969f8"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_jfs_os2_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1017416,
            ClusterSize  = 4096,
            VolumeName   = "DicSetter",
            VolumeSerial = "08fc8e22-0201-894e-89c9-31ec3f546203"
        }
    };
}