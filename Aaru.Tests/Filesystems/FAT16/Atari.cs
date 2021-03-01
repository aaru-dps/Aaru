// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT16.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT16
{
    [TestFixture]
    public class Atari : FilesystemTest
    {
        public Atari() : base("FAT16") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (Atari)");
        public override IFilesystem _plugin     => new FAT();
        public override bool        _partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "tos_1.04.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 81920,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 10239,
                ClusterSize  = 4096,
                VolumeSerial = "BA9831"
            },
            new FileSystemTest
            {
                TestFile     = "tos_1.04_small.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 8191,
                ClusterSize  = 1024,
                VolumeSerial = "2019E1"
            }
        };
    }
}