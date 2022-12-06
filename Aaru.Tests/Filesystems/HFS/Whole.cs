// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFS.cs
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

namespace Aaru.Tests.Filesystems.HFS
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base("HFS") {}

        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple HFS");

        public override IFilesystem Plugin     => new AppleHFS();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "macos_1.1_mf2dd.img.lz",
                MediaType   = MediaType.AppleSonyDS,
                Sectors     = 1600,
                SectorSize  = 512,
                Clusters    = 1594,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "macos_2.0_mf2dd.img.lz",
                MediaType   = MediaType.AppleSonyDS,
                Sectors     = 1600,
                SectorSize  = 512,
                Clusters    = 1594,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "macos_6.0.7_mf2dd.img.lz",
                MediaType   = MediaType.AppleSonyDS,
                Sectors     = 1600,
                SectorSize  = 512,
                Clusters    = 1594,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "nextstep_3.3_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "openstep_4.0_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "openstep_4.2_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "rhapsody_dr1_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "Volume label"
            },
            new FileSystemTest
            {
                TestFile    = "ecs20_mf2hd_fstester.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "VOLUME LABEL"
            },
            new FileSystemTest
            {
                TestFile    = "linux_2.2.17_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "VolumeLabel"
            },
            new FileSystemTest
            {
                TestFile    = "linux_2.2.20_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "VolumeLabel"
            },
            new FileSystemTest
            {
                TestFile    = "linux_2.4.18_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2874,
                ClusterSize = 512,
                VolumeName  = "VolumeLabel"
            }
        };
    }
}