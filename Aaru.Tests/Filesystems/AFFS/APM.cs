// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AFFS.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.AFFS;

[TestFixture]
public class APM : FilesystemTest
{
    public APM() : base("Amiga FFS") {}

    public override string DataFolder =>
        Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Fast File System (APM)");
    public override IFilesystem Plugin     => new AmigaDOSPlugin();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "morphos_3.13.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 262018,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1D930192"
        },
        new FileSystemTest
        {
            TestFile     = "morphos_3.13_cache.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 262018,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1D9105B0"
        },
        new FileSystemTest
        {
            TestFile     = "morphos_3.13_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 262018,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1D93031D"
        }
    };
}