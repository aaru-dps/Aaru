// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
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

namespace Aaru.Tests.Filesystems.UFS
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "UNIX filesystem (MBR)");
        public override IFilesystem _plugin     => new FFSPlugin();
        public override bool        _partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "ufs1/linux.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 262144,
                SectorSize  = 512,
                Clusters    = 65024,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs2/linux.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 262144,
                SectorSize  = 512,
                Clusters    = 65024,
                ClusterSize = 2048,
                Type        = "UFS2",
                VolumeName  = "VolumeLabel"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/darwin_1.3.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511024,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/darwin_1.4.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511024,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/darwin_6.0.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511024,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/darwin_8.0.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511488,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/dflybsd_1.2.0.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511950,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/dflybsd_3.6.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 255470,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/dflybsd_4.0.5.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 255470,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/netbsd_1.6.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511992,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ffs43/netbsd_7.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 409600,
                SectorSize  = 512,
                Clusters    = 204768,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/darwin_1.3.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 204800,
                SectorSize  = 512,
                Clusters    = 102280,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/darwin_1.4.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 204800,
                SectorSize  = 512,
                Clusters    = 102280,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/darwin_6.0.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 204800,
                SectorSize  = 512,
                Clusters    = 102280,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/darwin_8.0.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 204800,
                SectorSize  = 512,
                Clusters    = 102368,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/dflybsd_1.2.0.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 1048500,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/dflybsd_3.6.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 523758,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/dflybsd_4.0.5.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 523758,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/freebsd_6.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 262138,
                ClusterSize = 4096,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/freebsd_7.0.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 8388608,
                SectorSize  = 512,
                Clusters    = 1048231,
                ClusterSize = 4096,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/freebsd_8.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 8388608,
                SectorSize  = 512,
                Clusters    = 2096462,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/netbsd_1.6.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 524284,
                ClusterSize = 2048,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/netbsd_7.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511968,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/solaris_7.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 1038240,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs1/solaris_9.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 1046808,
                ClusterSize = 1024,
                Type        = "UFS"
            },
            new FileSystemTest
            {
                TestFile    = "ufs2/freebsd_6.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 16777216,
                SectorSize  = 512,
                Clusters    = 2096472,
                ClusterSize = 4096,
                Type        = "UFS2",
                VolumeName  = "VolumeLabel"
            },
            new FileSystemTest
            {
                TestFile    = "ufs2/freebsd_7.0.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 16777216,
                SectorSize  = 512,
                Clusters    = 2096472,
                ClusterSize = 4096,
                Type        = "UFS2",
                VolumeName  = "VolumeLabel"
            },
            new FileSystemTest
            {
                TestFile    = "ufs2/freebsd_8.2.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 16777216,
                SectorSize  = 512,
                Clusters    = 4192945,
                ClusterSize = 2048,
                Type        = "UFS2",
                VolumeName  = "VolumeLabel"
            },
            new FileSystemTest
            {
                TestFile    = "ufs2/netbsd_7.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 524272,
                ClusterSize = 2048,
                Type        = "UFS2",
                VolumeName  = ""
            }
        };
    }
}