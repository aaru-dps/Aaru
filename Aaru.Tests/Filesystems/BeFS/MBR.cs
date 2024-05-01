// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BeFS.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.BeFS;

[TestFixture]
public class MBR() : FilesystemTest("befs")
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Be File System (MBR)");
    public override IFilesystem Plugin     => new Aaru.Filesystems.BeFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests =>
    [
        new FileSystemTest
        {
            TestFile    = "beos_r3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 131040,
            ClusterSize = 1024,
            VolumeName  = "Big Volume Label Goes Big Brrrr"
        },
        new FileSystemTest
        {
            TestFile    = "beos_r4.5.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130536,
            ClusterSize = 1024,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "haiku_hrev51259.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65024,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "haiku_hrev51259_8k.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 16256,
            ClusterSize = 8192,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "syllable_0.6.7.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 2097152,
            SectorSize  = 512,
            Clusters    = 524272,
            ClusterSize = 2048,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "beos_r5.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 130536,
            ClusterSize = 1024,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "beos_r5_2k.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65268,
            ClusterSize = 2048,
            VolumeName  = "Volume label 2K"
        },
        new FileSystemTest
        {
            TestFile    = "beos_r5_4k.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32634,
            ClusterSize = 4096,
            VolumeName  = "Volume label 4K"
        }
    ];
}