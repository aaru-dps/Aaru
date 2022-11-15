﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NTFS.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.NTFS;

[TestFixture]
public class GPT : FilesystemTest
{
    public GPT() : base("NTFS") {}

    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "New Technology File System (GPT)");
    public override IFilesystem Plugin     => new Aaru.Filesystems.NTFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "haiku_hrev51259.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32511,
            ClusterSize  = 4096,
            VolumeSerial = "2A1DF87732D3285C"
        }
    };
}