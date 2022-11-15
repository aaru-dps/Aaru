// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UFS;

[TestFixture, SuppressMessage("ReSharper", "InconsistentNaming")]
public class NeXT_Floppy : FilesystemTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (NeXT)");
    public override IFilesystem Plugin => new FFSPlugin();
    public override bool Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "nextstep_3.3_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 624,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "nextstep_3.3_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1344,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.0_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 624,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.0_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1344,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.2_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 624,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "openstep_4.2_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1344,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr1_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 624,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr1_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1344,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr2_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 624,
            ClusterSize = 1024,
            Type        = "UFS"
        },
        new FileSystemTest
        {
            TestFile    = "rhapsody_dr2_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1344,
            ClusterSize = 1024,
            Type        = "UFS"
        }
    };
}