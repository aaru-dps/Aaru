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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.HFS;

[TestFixture]
public class MBR : FilesystemTest
{
    public MBR() : base("HFS") {}

    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS (MBR)");
    public override IFilesystem Plugin     => new AppleHFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65528,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "5426B36FBE19CF1E"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65514,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "E7E1830009BA60A8"
        },
        new FileSystemTest
        {
            TestFile    = "linux.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65018,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32750,
            ClusterSize = 4096,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.4.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32750,
            ClusterSize = 4096,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65499,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65515,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "8BF73DE208CD7E7B"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65515,
            ClusterSize  = 2048,
            VolumeName   = "Volume label",
            VolumeSerial = "D149994212CC652E"
        },
        new FileSystemTest
        {
            TestFile    = "linux_4.19_hfs_flashdrive.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 1024000,
            SectorSize  = 512,
            Clusters    = 63870,
            ClusterSize = 8192,
            VolumeName  = "DicSetter"
        }
    };
}