// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus.cs
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

// ReSharper disable CheckNamespace

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.HFSPlus;

[TestFixture]
public class GPT : FilesystemTest
{
    public GPT() : base("HFS+") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple HFS+ (GPT)");
    public override IFilesystem Plugin     => new AppleHFSPlus();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 51190,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "D8C68470046E67BE"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 614400,
            SectorSize   = 512,
            Clusters     = 76790,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "FD3CB598F3C6294A"
        }
    };
}