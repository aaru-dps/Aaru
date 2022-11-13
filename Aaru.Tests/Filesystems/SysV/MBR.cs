﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SysV.cs
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

namespace Aaru.Tests.Filesystems.SysV
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("SVR4 fs") {}

        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Filesystems", "System V filesystem (MBR)");
        public override IFilesystem Plugin     => new SysVfs();
        public override bool        Partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "att_unix_svr4v2.1.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 511056,
                ClusterSize = 1024,
                VolumeName  = "/usr3"
            },
            new FileSystemTest
            {
                TestFile    = "att_unix_svr4v2.1_2k.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Clusters    = 255528,
                ClusterSize = 2048,
                VolumeName  = "/usr3"
            },
            new FileSystemTest
            {
                TestFile    = "scoopenserver_5.0.7hw.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 2097152,
                SectorSize  = 512,
                Clusters    = 1020096,
                ClusterSize = 1024,
                VolumeName  = "Volume label"
            }
        };
    }
}