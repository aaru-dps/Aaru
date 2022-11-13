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

namespace Aaru.Tests.Filesystems.HFS;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class Optical : FilesystemTest
{
    public Optical() : base("HFS") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple HFS (CD-ROM)");
    public override IFilesystem Plugin     => new AppleHFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "toast_3.5.7_hfs_from_volume.aif",
            MediaType   = MediaType.CD,
            Sectors     = 942,
            SectorSize  = 2048,
            Clusters    = 3724,
            ClusterSize = 512,
            VolumeName  = "Disk utils"
        },
        new FileSystemTest
        {
            TestFile    = "toast_3.5.7_iso9660_hfs.aif",
            MediaType   = MediaType.CD,
            Sectors     = 1880,
            SectorSize  = 2048,
            Clusters    = 931,
            ClusterSize = 2048,
            VolumeName  = "Disk utils"
        },
        new FileSystemTest
        {
            TestFile    = "toast_4.1.3_hfs_from_volume.aif",
            MediaType   = MediaType.CD,
            Sectors     = 943,
            SectorSize  = 2048,
            Clusters    = 931,
            ClusterSize = 2048,
            VolumeName  = "Disk utils"
        },
        new FileSystemTest
        {
            TestFile    = "toast_4.1.3_iso9660_hfs.aif",
            MediaType   = MediaType.CD,
            Sectors     = 1882,
            SectorSize  = 2048,
            Clusters    = 931,
            ClusterSize = 2048,
            VolumeName  = "Disk utils"
        },
        new FileSystemTest
        {
            TestFile    = "toast_3.5.7_hfs_from_files.aif",
            MediaType   = MediaType.CD,
            Sectors     = 1509,
            SectorSize  = 2048,
            Clusters    = 249,
            ClusterSize = 12288,
            VolumeName  = "Disk utils"
        },
        new FileSystemTest
        {
            TestFile    = "toast_4.1.3_hfs_from_files.aif",
            MediaType   = MediaType.CD,
            Sectors     = 1529,
            SectorSize  = 2048,
            Clusters    = 249,
            ClusterSize = 12288,
            VolumeName  = "Disk utils"
        }
    };
}