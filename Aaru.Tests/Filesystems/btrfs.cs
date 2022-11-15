// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : btrfs.cs
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

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class Btrfs : FilesystemTest
{
    public Btrfs() : base("B-tree file system") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "btrfs");
    public override IFilesystem Plugin     => new BTRFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "linux.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32512,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "a4fc5201-85cc-6840-8a68-998cab9ae897"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_btrfs_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 127744,
            ClusterSize  = 4096,
            VolumeName   = "btrfs",
            VolumeSerial = "5af44541-0605-f541-af6d-c229576707ab"
        }
    };
}