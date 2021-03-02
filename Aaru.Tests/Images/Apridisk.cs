// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apridisk.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Apridisk : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "apr00001.dsk.lz", "apr00002.dsk.lz", "apr00006.dsk.lz", "apr00203.dsk.lz"
        };
        public override ulong[] _sectors => new ulong[]
        {
            // apr00001.dsk.lz
            1440,

            // apr00002.dsk.lz
            1440,

            // apr00006.dsk.lz
            1440,

            // apr00203.dsk.lz
            1440
        };
        public override uint[] _sectorSize => new uint[]
        {
            // apr00001.dsk.lz
            512,

            // apr00002.dsk.lz
            512,

            // apr00006.dsk.lz
            512,

            // apr00203.dsk.lz
            512
        };
        public override MediaType[] _mediaTypes => new[]
        {
            // apr00001.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // apr00002.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // apr00006.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // apr00203.dsk.lz
            MediaType.DOS_35_DS_DD_9
        };
        public override string[] _md5S => new[]
        {
            // apr00001.dsk.lz
            "6c264287a3260a6d89e36dfcb1c98dce",

            // apr00002.dsk.lz
            "dd8e04939baeb0fcdb11ddade60c9a93",

            // apr00006.dsk.lz
            "89132d303ef6b0ff69f4cfd38e2a22a6",

            // apr00203.dsk.lz
            "cd34832ca3aa7f55e0dd8ba126372f97"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Apridisk");
        public override IMediaImage _plugin => new DiscImages.Apridisk();
    }
}