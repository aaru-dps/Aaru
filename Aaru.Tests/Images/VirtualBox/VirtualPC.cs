// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VirtualPC.cs
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
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.VirtualBox;

[TestFixture]
public class VirtualPc : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "VirtualBox", "VirtualPC");
    public override IMediaImage Plugin => new Vhd();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "virtualbox_linux_dynamic_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 512000,
            SectorSize = 512,
            Md5        = "ab3248888d6f10ef30a084fac6a1e2fd"
        },
        new BlockImageTestExpected
        {
            TestFile   = "virtualbox_linux_fixed_10mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20480,
            SectorSize = 512,
            Md5        = "f1c9645dbc14efddc7d8a322685f26eb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "virtualbox_macos_dynamic_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 512000,
            SectorSize = 512,
            Md5        = "ab3248888d6f10ef30a084fac6a1e2fd"
        },
        new BlockImageTestExpected
        {
            TestFile   = "virtualbox_macos_fixed_10mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20480,
            SectorSize = 512,
            Md5        = "f1c9645dbc14efddc7d8a322685f26eb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "virtualbox_windows_dynamic_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 512000,
            SectorSize = 512,
            Md5        = "ab3248888d6f10ef30a084fac6a1e2fd"
        },
        new BlockImageTestExpected
        {
            TestFile   = "virtualbox_windows_fixed_10mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20480,
            SectorSize = 512,
            Md5        = "f1c9645dbc14efddc7d8a322685f26eb"
        }
    };
}