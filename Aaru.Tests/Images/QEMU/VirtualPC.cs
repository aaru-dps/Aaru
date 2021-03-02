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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.QEMU
{
    [TestFixture]
    public class VirtualPC : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "qemu_dynamic_250mb.vhd.lz", "qemu_fixed_10mb.vhd.lz", "virtualpc.vhd.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // qemu_dynamic_250mb.vhd.lz"
            512064,

            // qemu_fixed_10mb.vhd.lz"
            20536,

            // virtualpc.vhd.lz
            251940
        };

        public override uint[] _sectorSize => new uint[]
        {
            // qemu_dynamic_250mb.vhd.lz"
            512,

            // qemu_fixed_10mb.vhd.lz"
            512,

            // virtualpc.vhd.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // qemu_dynamic_250mb.vhd.lz"
            MediaType.Unknown,

            // qemu_fixed_10mb.vhd.lz"
            MediaType.Unknown,

            // virtualpc.vhd.lz
            MediaType.Unknown
        };

        public override string[] _md5S => new[]
        {
            // qemu_dynamic_250mb.vhd.lz"
            "0435d6781d14d34a32c6ac40f5e70d35",

            // qemu_fixed_10mb.vhd.lz"
            "adfad4fb019f157e868baa39e7753db7",

            // virtualpc.vhd.lz
            "6246bff640cb3a56d2611e7f8616384d"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "QEMU", "VirtualPC");
        public override IMediaImage _plugin => new Vhd();
    }
}