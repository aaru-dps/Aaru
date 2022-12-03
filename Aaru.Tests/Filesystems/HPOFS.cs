// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
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

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class Hpofs : FilesystemTest
{
    public Hpofs() : base("HPOFS") {}

    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems",
                                                      "High Performance Optical File System");
    public override IFilesystem Plugin     => new HPOFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "rid1.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "AC226814"
        },
        new FileSystemTest
        {
            TestFile     = "rid10.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "AC160814"
        },
        new FileSystemTest
        {
            TestFile     = "rid66percent.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "AC306C14"
        },
        new FileSystemTest
        {
            TestFile     = "rid266.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "ABEF2C14"
        }
    };
}