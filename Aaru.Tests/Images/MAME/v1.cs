// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : v1.cs
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

namespace Aaru.Tests.Images.MAME
{
    [TestFixture]
    public class V1 : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "hdd.chd"
        };
        public override ulong[] _sectors => new ulong[]
        {
            // hdd.chd
            251904
        };
        public override uint[] _sectorSize => new uint[]
        {
            // hdd.chd
            512
        };
        public override MediaType[] _mediaTypes => new[]
        {
            // hdd.chd
            MediaType.GENERIC_HDD
        };
        public override string[] _md5S => new[]
        {
            // hdd.chd
            "43476343f53a177dd57b68dd769917aa"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "MAME", "v1");
        public override IMediaImage _plugin => new Chd();
    }
}