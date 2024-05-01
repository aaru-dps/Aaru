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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.AFFS;

[TestFixture]
public class RDB() : FilesystemTest("affs")
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "Amiga Fast File System (RDB)");

    public override IFilesystem Plugin     => new AmigaDOSPlugin();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "amigaos_3.9.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 510032,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56D0F5C"
        },
        new FileSystemTest
        {
            TestFile     = "amigaos_3.9_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 510032,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56D049C"
        },
        new FileSystemTest
        {
            TestFile     = "aros.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 407232,
            ClusterSize  = 512,
            VolumeName   = "Volume label",
            VolumeSerial = "A58307A9"
        },
        new FileSystemTest
        {
            TestFile     = "aros_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 407232,
            ClusterSize  = 512,
            VolumeName   = "Volume label",
            VolumeSerial = "A58304BE"
        },
        new FileSystemTest
        {
            TestFile     = "amigaos_4.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 511040,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56CC7EE"
        },
        new FileSystemTest
        {
            TestFile     = "amigaos_4.0_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 511040,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56CDDC4"
        },
        new FileSystemTest
        {
            TestFile     = "amigaos_4.0_cache.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 511040,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56CC133"
        },
        new FileSystemTest
        {
            TestFile     = "morphos_3.13.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 261936,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1D93009A"
        },
        new FileSystemTest
        {
            TestFile     = "morphos_3.13_cache.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 261936,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1D9107DA"
        },
        new FileSystemTest
        {
            TestFile     = "morphos_3.13_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 261936,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1D92FD23"
        }
    };
}