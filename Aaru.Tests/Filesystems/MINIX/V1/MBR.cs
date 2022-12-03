// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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

namespace Aaru.Tests.Filesystems.MINIX.V1;

[TestFixture]
public class MBR : FilesystemTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "MINIX v1 filesystem (MBR)");
    public override IFilesystem Plugin => new MinixFS();
    public override bool Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "minix_3.1.2a.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 102400,
            SectorSize  = 512,
            Clusters    = 50399,
            ClusterSize = 1024,
            Type        = "Minix 3 v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_4.19_minix1_flashdrive.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 131072,
            SectorSize  = 512,
            Clusters    = 64512,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.29.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.34.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.37.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.38.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.2.17.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.2.20.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.4.18.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65535,
            ClusterSize = 1024,
            Type        = "Minix v1"
        }
    };
}