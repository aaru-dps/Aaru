// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser4.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Reiser4 : FilesystemTest
    {
        public Reiser4() : base("Reiser 4 filesystem") {}

        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Filesystems", "Reiser filesystem v4");
        public override IFilesystem Plugin     => new Aaru.Filesystems.Reiser4();
        public override bool        Partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "linux.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Clusters     = 32511,
                ClusterSize  = 4096,
                VolumeName   = "Volume label",
                VolumeSerial = "b0c1924e-6f10-8c42-b6c5-66a457896460"
            }
        };
    }
}