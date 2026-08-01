// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ext2.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class Ext4() : FilesystemTest("ext4")
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "ext4");
    public override IFilesystem Plugin     => new ext2FS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests =>
    [
        new()
        {
            TestFile     = "linux_4.19_ext4_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 510976,
            ClusterSize  = 1024,
            Type         = "ext4",
            VolumeName   = "DicSetter",
            VolumeSerial = "10413797-43d1-6545-8fbc-6ebc9d328be9"
        }
    ];
}