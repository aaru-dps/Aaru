// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
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

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class XFS() : FilesystemTest("xfs")
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "XFS");
    public override IFilesystem Plugin     => new Aaru.Filesystems.XFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests =>
    [
        new FileSystemTest
        {
            TestFile     = "linux.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1048576,
            SectorSize   = 512,
            Clusters     = 130816,
            ClusterSize  = 4096,
            VolumeName   = "Volume label",
            VolumeSerial = "230075b7-9834-b44e-a257-982a058311d8"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_xfs_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 127744,
            ClusterSize  = 4096,
            VolumeName   = "DicSetter",
            VolumeSerial = "ed6b4d35-aa66-ce4a-9d8f-c56dbc6d7c8c"
        }
    ];
}