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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Filesystems.UFS;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class APM : FilesystemTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (APM)");
    public override IFilesystem Plugin => new FFSPlugin();
    public override bool Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "ffs43/macosx_10.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130156,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/macosx_10.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130156,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/macosx_10.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130156,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/macosx_10.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 121448,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/macosx_10.4.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 122328,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_1.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_1.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_1.3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_1.4.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_7.0.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ffs43/darwin_8.0.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_1.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_1.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_1.3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_1.4.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_7.0.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/darwin_8.0.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/macosx_10.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130156,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/macosx_10.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130156,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/macosx_10.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130156,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/macosx_10.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 121448,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "ufs1/macosx_10.4.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 122328,
            ClusterSize = 1024,
            Type        = "UFS"
        }
    };
}