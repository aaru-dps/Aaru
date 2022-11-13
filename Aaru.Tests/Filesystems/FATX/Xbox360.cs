// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
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

namespace Aaru.Tests.Filesystems.FATX;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class Xbox360 : ReadOnlyFilesystemTest
{
    public Xbox360() : base("FATX filesystem") {}

    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Xbox FAT16", "be");

    public override IFilesystem Plugin     => new XboxFatPlugin();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "microsoft256mb.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 491520,
            SectorSize   = 512,
            Clusters     = 14848,
            ClusterSize  = 16384,
            VolumeName   = "",
            VolumeSerial = "66C2E9D0"
        }
    };
}