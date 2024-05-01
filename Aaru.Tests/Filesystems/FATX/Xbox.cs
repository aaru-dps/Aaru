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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

// ReSharper disable StringLiteralTypo

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FATX;

[TestFixture]
public class Xbox() : ReadOnlyFilesystemTest("fatx")
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Xbox FAT16", "le");
    public override IFilesystem Plugin     => new XboxFatPlugin();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests =>
    [
        new FileSystemTest
        {
            TestFile     = "fatx.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 62720,
            SectorSize   = 512,
            Clusters     = 1960,
            ClusterSize  = 16384,
            VolumeName   = "Volume láb€l",
            VolumeSerial = "4639B7D0"
        }
    ];
}