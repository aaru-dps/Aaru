// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT32.cs
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

namespace Aaru.Tests.Filesystems.FAT32
{
    [TestFixture]
    public class APM : ReadOnlyFilesystemTest
    {
        public APM() : base("FAT32") {}

        public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT32 (APM)");
        public override IFilesystem Plugin     => new FAT();
        public override bool        Partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "darwin_6.0.2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 262080,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "7C930CFA"
            },
            new FileSystemTest
            {
                TestFile     = "darwin_7.0.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 262080,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "44681B0D"
            },
            new FileSystemTest
            {
                TestFile     = "darwin_8.0.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 262080,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "72D719E8"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.3.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 260304,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "7CD11609"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.4.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 262064,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "4495131D"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 4194304,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 524278,
                ClusterSize  = 4096,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "35BD1F0A"
            }
        };
    }
}