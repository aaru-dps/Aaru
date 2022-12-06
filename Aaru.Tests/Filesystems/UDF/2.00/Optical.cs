// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
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
// ReSharper disable CheckNamespace

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UDF._200
{
    [TestFixture]
    public class Optical : FilesystemTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Filesystems", "Universal Disc Format", "2.00");
        public override IFilesystem Plugin     => new Aaru.Filesystems.UDF();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "ecs20.aif",
                MediaType    = MediaType.DVDPR,
                Sectors      = 2295104,
                SectorSize   = 2048,
                Clusters     = 2295104,
                ClusterSize  = 2048,
                SystemId     = "*ExpressUDF",
                Type         = "UDF v2.01",
                VolumeName   = "UDF5A5DEF48",
                VolumeSerial = "Volume Set ID not specified"
            }
        };
    }
}