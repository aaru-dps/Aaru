// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser3.cs
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

namespace Aaru.Tests.Filesystems;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class Reiser3 : FilesystemTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Reiser filesystem v3");
    public override IFilesystem Plugin => new Reiser();
    public override bool Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "linux_2.2.20_r3.5.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32752,
            ClusterSize = 4096,
            Type        = "Reiser 3.5 filesystem"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.4.18_r3.5.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32752,
            ClusterSize = 4096,
            Type        = "Reiser 3.5 filesystem"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18_r3.6.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32752,
            ClusterSize  = 4096,
            Type         = "Reiser 3.6 filesystem",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "43c72111-6512-e747-b626-63704e65352a"
        },
        new FileSystemTest
        {
            TestFile    = "linux_4.19_reiser_3.5_flashdrive.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 1024000,
            SectorSize  = 512,
            Clusters    = 127744,
            ClusterSize = 4096,
            Type        = "Reiser 3.5 filesystem"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_reiser_3.6_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 127744,
            ClusterSize  = 4096,
            Type         = "Reiser 3.6 filesystem",
            VolumeName   = "DicSetter",
            VolumeSerial = "8902ac3c-3e0c-4c4c-84ec-03405c1710f1"
        }
    };
}