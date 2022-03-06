// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LisaFS.cs
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
using Aaru.Filesystems.LisaFS;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class LisaFs : ReadOnlyFilesystemTest
{
    public LisaFs() : base("LisaFS") {}

    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                      "Apple Lisa filesystem");
    public override IFilesystem Plugin     => new LisaFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "166files.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "166Files",
            VolumeSerial = "A23703A202010663"
        },
        new FileSystemTest
        {
            TestFile     = "222files.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "222Files",
            VolumeSerial = "A23703A201010663"
        },
        new FileSystemTest
        {
            TestFile     = "blank2.0.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 792,
            ClusterSize  = 512,
            VolumeName   = "AOS  4:59 pm 10/02/87",
            VolumeSerial = "A32D261301010663"
        },
        new FileSystemTest
        {
            TestFile     = "blank-disk.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D01010663"
        },
        new FileSystemTest
        {
            TestFile     = "file-with-a-password.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CC3A702010663"
        },
        new FileSystemTest
        {
            TestFile     = "tfwdndrc-has-been-erased.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D14010663"
        },
        new FileSystemTest
        {
            TestFile     = "tfwdndrc-has-been-restored.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D14010663"
        },
        new FileSystemTest
        {
            TestFile     = "three-empty-folders.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D01010663"
        },
        new FileSystemTest
        {
            TestFile     = "three-folders-with-differently-named-docs.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D01010663"
        },
        new FileSystemTest
        {
            TestFile     = "three-folders-with-differently-named-docs-root-alphabetical.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D01010663"
        },
        new FileSystemTest
        {
            TestFile     = "three-folders-with-differently-named-docs-root-chronological.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D01010663"
        },
        new FileSystemTest
        {
            TestFile     = "three-folders-with-identically-named-docs.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A22CB48D01010663"
        },
        new FileSystemTest
        {
            TestFile     = "lisafs1.dc42.lz",
            MediaType    = MediaType.AppleFileWare,
            Sectors      = 1702,
            SectorSize   = 512,
            Clusters     = 1684,
            ClusterSize  = 512,
            VolumeName   = "AOS 4:15 pm 5/06/1983",
            VolumeSerial = "9924151E190001E1"
        },
        new FileSystemTest
        {
            TestFile     = "lisafs2.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 792,
            ClusterSize  = 512,
            VolumeName   = "Office System 1 2.0",
            VolumeSerial = "9497F10016010D10"
        },
        new FileSystemTest
        {
            TestFile     = "lisafs3.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "Office System 1 3.0",
            VolumeSerial = "9CF9CF89070100A8"
        },
        new FileSystemTest
        {
            TestFile     = "lisafs3_with_desktop.dc42.lz",
            MediaType    = MediaType.AppleSonySS,
            Sectors      = 800,
            SectorSize   = 512,
            Clusters     = 800,
            ClusterSize  = 512,
            VolumeName   = "AOS 3.0",
            VolumeSerial = "A4FE1A191F011652"
        }
    };
}