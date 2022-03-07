// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ADFS.cs
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
public class Adfs : FilesystemTest
{
    public Adfs() : base("Acorn Advanced Disc Filing System") {}

    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                      "Acorn Advanced Disc Filing System");

    public override IFilesystem Plugin     => new AcornADFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "adfs_d.adf.lz",
            MediaType    = MediaType.ACORN_35_DS_DD,
            Sectors      = 800,
            SectorSize   = 1024,
            Clusters     = 800,
            ClusterSize  = 1024,
            VolumeName   = "ADFSD",
            VolumeSerial = "3E48"
        },
        new FileSystemTest
        {
            TestFile     = "adfs_e.adf.lz",
            MediaType    = MediaType.ACORN_35_DS_DD,
            Sectors      = 800,
            SectorSize   = 1024,
            Clusters     = 800,
            ClusterSize  = 1024,
            VolumeName   = "ADFSE     ",
            VolumeSerial = "E13A"
        },
        new FileSystemTest
        {
            TestFile    = "adfs_f.adf.lz",
            MediaType   = MediaType.ACORN_35_DS_HD,
            Sectors     = 1600,
            SectorSize  = 1024,
            Clusters    = 1600,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile     = "adfs_e+.adf.lz",
            MediaType    = MediaType.ACORN_35_DS_DD,
            Sectors      = 800,
            SectorSize   = 1024,
            Clusters     = 800,
            ClusterSize  = 1024,
            VolumeName   = "ADFSE+    ",
            VolumeSerial = "1142"
        },
        new FileSystemTest
        {
            TestFile    = "adfs_f+.adf.lz",
            MediaType   = MediaType.ACORN_35_DS_HD,
            Sectors     = 1600,
            SectorSize  = 1024,
            Clusters    = 1600,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile     = "adfs_s.adf.lz",
            MediaType    = MediaType.ACORN_525_SS_DD_40,
            Sectors      = 640,
            SectorSize   = 256,
            Clusters     = 640,
            ClusterSize  = 256,
            VolumeName   = "$",
            VolumeSerial = "F20D"
        },
        new FileSystemTest
        {
            TestFile     = "adfs_m.adf.lz",
            MediaType    = MediaType.ACORN_525_SS_DD_80,
            Sectors      = 1280,
            SectorSize   = 256,
            Clusters     = 1280,
            ClusterSize  = 256,
            VolumeName   = "$",
            VolumeSerial = "D6CA"
        },
        new FileSystemTest
        {
            TestFile     = "adfs_l.adf.lz",
            MediaType    = MediaType.ACORN_525_DS_DD,
            Sectors      = 2560,
            SectorSize   = 256,
            Clusters     = 2560,
            ClusterSize  = 256,
            VolumeName   = "$",
            VolumeSerial = "0CA6"
        },
        new FileSystemTest
        {
            TestFile     = "hdd_old.hdf.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 78336,
            SectorSize   = 256,
            Clusters     = 78336,
            ClusterSize  = 256,
            VolumeName   = "VolLablOld",
            VolumeSerial = "080E"
        },
        new FileSystemTest
        {
            TestFile    = "hdd_new.hdf.lz",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 78336,
            SectorSize  = 256,
            Clusters    = 78336,
            ClusterSize = 256
        }
    };
}