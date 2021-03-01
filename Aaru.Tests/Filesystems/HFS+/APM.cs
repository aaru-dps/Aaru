// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace Aaru.Tests.Filesystems.HFSPlus
{
    [TestFixture]
    public class APM : FilesystemTest
    {
        public APM() : base("HFS+") {}

        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (APM)");
        public override IFilesystem Plugin => new AppleHFSPlus();
        public override bool Partitions => true;

        // Missing Darwin 1.4.1
        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 409600,
                SectorSize   = 512,
                Clusters     = 51190,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "FA94762D086A18A9"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11_journal.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 614400,
                SectorSize   = 512,
                Clusters     = 76790,
                ClusterSize  = 4096,
                SystemId     = "HFSJ",
                VolumeSerial = "33D4A309C8E7BD10"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.3.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 819200,
                SectorSize  = 512,
                Clusters    = 102392,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.3.1_wrapped.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 614400,
                SectorSize  = 512,
                Clusters    = 76774,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.4.1_wrapped.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 614400,
                SectorSize  = 512,
                Clusters    = 76774,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_6.0.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 819200,
                SectorSize  = 512,
                Clusters    = 102392,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_6.0.2_wrapped.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 614400,
                SectorSize  = 512,
                Clusters    = 76774,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile     = "darwin_8.0.1_journal.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1228800,
                SectorSize   = 512,
                Clusters     = 153592,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "4D5140EB8F14A385"
            },
            new FileSystemTest
            {
                TestFile     = "darwin_8.0.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 819200,
                SectorSize   = 512,
                Clusters     = 102392,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "0D592249833E2DC4"
            },
            new FileSystemTest
            {
                TestFile     = "darwin_8.0.1_wrapped.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 614400,
                SectorSize   = 512,
                Clusters     = 76774,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "AA616146576BD9BC"
            },
            new FileSystemTest
            {
                TestFile    = "macos_8.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 4194304,
                SectorSize  = 512,
                Clusters    = 524152,
                ClusterSize = 4096,
                SystemId    = "8.10"
            },
            new FileSystemTest
            {
                TestFile    = "macos_9.0.4.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 4194304,
                SectorSize  = 512,
                Clusters    = 524088,
                ClusterSize = 4096,
                SystemId    = "8.10"
            },
            new FileSystemTest
            {
                TestFile    = "macos_9.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 4194304,
                SectorSize  = 512,
                Clusters    = 524088,
                ClusterSize = 4096,
                SystemId    = "8.10"
            },
            new FileSystemTest
            {
                TestFile    = "macos_9.2.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 4194304,
                SectorSize  = 512,
                Clusters    = 524088,
                ClusterSize = 4096,
                SystemId    = "8.10"
            },
            new FileSystemTest
            {
                TestFile    = "macos_9.2.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 4194304,
                SectorSize  = 512,
                Clusters    = 524088,
                ClusterSize = 4096,
                SystemId    = "8.10"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 4194304,
                SectorSize   = 512,
                Clusters     = 524008,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "EFA132FFFAC1ADA6"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.3_journal.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 2097152,
                SectorSize   = 512,
                Clusters     = 261884,
                ClusterSize  = 4096,
                SystemId     = "HFSJ",
                VolumeSerial = "009D570FFCF8F20B"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.3.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 4194304,
                SectorSize   = 512,
                Clusters     = 491240,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "17F6F33AB313EE32"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.4_journal.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 2097152,
                SectorSize   = 512,
                Clusters     = 261884,
                ClusterSize  = 4096,
                SystemId     = "HFSJ",
                VolumeSerial = "AD5690C093F66FCF"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.4.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 4194304,
                SectorSize   = 512,
                Clusters     = 491240,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "A7D63854DF76DDE6"
            }
        };
    }
}