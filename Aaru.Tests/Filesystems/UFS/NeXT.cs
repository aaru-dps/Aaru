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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UFS;

[TestFixture]
public class NeXT : FilesystemTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (NeXT)");
    public override IFilesystem Plugin => new FFSPlugin();
    public override bool Partitions => true;

    public override FileSystemTest[] Tests =>
    [
        new FileSystemTest
        {
            TestFile    = "nextstep_3.3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130880,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "macosx_1.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "macosx_1.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "macosx_1.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        },
        new FileSystemTest
        {
            TestFile    = "macosx_1.2v3.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130912,
            ClusterSize = 1024,
            Type        = "ufs"
        }
    ];
}