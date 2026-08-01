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
public class Ext3() : FilesystemTest("ext3")
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "ext3");
    public override IFilesystem Plugin     => new ext2FS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests =>
    [
        new()
        {
            TestFile     = "linux_2.4.18_ext3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 131008,
            ClusterSize  = 1024,
            Type         = "ext3",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "40bb6664-4e0e-ca4b-a328-7c744ddd8bf4"
        },
        new()
        {
            TestFile     = "linux_4.19_ext3_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 510976,
            ClusterSize  = 1024,
            Type         = "ext3",
            VolumeName   = "DicSetter",
            VolumeSerial = "a3914b55-260f-7245-8c72-7ccdf45436cb"
        }
    ];
}