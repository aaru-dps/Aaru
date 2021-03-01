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
    // Mising Darwin 6.0.2 wrapped
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("HFS+") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (MBR)");

        public override IFilesystem _plugin     => new AppleHFSPlus();
        public override bool        _partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 303104,
                SectorSize   = 512,
                Clusters     = 37878,
                ClusterSize  = 4096,
                SystemId     = "10.0",
                VolumeSerial = "C84F550907D13F50"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11_journal.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 352256,
                SectorSize   = 512,
                Clusters     = 44021,
                ClusterSize  = 4096,
                SystemId     = "HFSJ",
                VolumeSerial = "016599F88029F73D"
            },
            new FileSystemTest
            {
                TestFile    = "linux.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 262144,
                SectorSize  = 512,
                Clusters    = 32512,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "linux_journal.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 262144,
                SectorSize  = 512,
                Clusters    = 32512,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.3.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 819200,
                SectorSize  = 512,
                Clusters    = 102178,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.3.1_wrapped.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 614400,
                SectorSize  = 512,
                Clusters    = 76708,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.4.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 819200,
                SectorSize  = 512,
                Clusters    = 102178,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_1.4.1_wrapped.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 614400,
                SectorSize  = 512,
                Clusters    = 76708,
                ClusterSize = 4096,
                SystemId    = "10.0"
            },
            new FileSystemTest
            {
                TestFile    = "darwin_6.0.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 819200,
                SectorSize  = 512,
                Clusters    = 102178,
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
                VolumeSerial = "F92964F9B3F64ABB"
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
                VolumeSerial = "A8FAC484A0A2B177"
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
                VolumeSerial = "D5D5BF1346AD2B8D"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_hfs+_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 127744,
                ClusterSize  = 4096,
                SystemId     = "H+Lx",
                VolumeSerial = "B9BAC6856878A404"
            }
        };
    }
}