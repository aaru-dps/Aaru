// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : F2FS.cs
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
public class F2Fs : FilesystemTest
{
    public F2Fs() : base("F2FS filesystem") {}

    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "F2FS");
    public override IFilesystem Plugin     => new F2FS();
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
            VolumeSerial = "81bd3a4e-de0c-484c-becc-aaa479b2070a"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_f2fs_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 2097152,
            SectorSize   = 512,
            Clusters     = 261888,
            ClusterSize  = 4096,
            VolumeName   = "DicSetter",
            VolumeSerial = "422bd2a8-68ab-6f45-9a04-9c264d07dd6e"
        }
    };
}