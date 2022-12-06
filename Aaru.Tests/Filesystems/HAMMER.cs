// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HAMMER.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Hammer : FilesystemTest
    {
        public Hammer() : base("HAMMER") {}

        public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "HAMMER (MBR)");
        public override IFilesystem Plugin     => new HAMMER();
        public override bool        Partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "dflybsd_3.6.1.vdi.lz",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 104857600,
                SectorSize   = 512,
                Clusters     = 6310,
                ClusterSize  = 8388608,
                VolumeName   = "Volume label",
                VolumeSerial = "f8e1a8bb-626d-11e7-94b5-0900274691e4"
            },
            new FileSystemTest
            {
                TestFile     = "dflybsd_4.0.5.vdi.lz",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 104857600,
                SectorSize   = 512,
                Clusters     = 6310,
                ClusterSize  = 8388608,
                VolumeName   = "Volume label",
                VolumeSerial = "ff4dc664-6276-11e7-983f-090027c41b46"
            }
        };
    }
}