// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XDF.cs
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

namespace Aaru.Tests.Images.pce
{
    [TestFixture]
    public class XDF : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "mf2hd_xdf_teledisk.xdf.lz", "mf2hd_xdf.xdf.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // mf2hd_xdf_teledisk.xdf.lz
            3680,

            // mf2hd_xdf.xdf.lz
            3680
        };

        public override uint[] _sectorSize => new uint[]
        {
            // mf2hd_xdf_teledisk.xdf.lz
            512,

            // mf2hd_xdf.xdf.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // mf2hd_xdf_teledisk.xdf.lz
            MediaType.XDF_35,

            // mf2hd_xdf.xdf.lz
            MediaType.XDF_35
        };

        public override string[] _md5S => new[]
        {
            // mf2hd_xdf_teledisk.xdf.lz
            "90e8f5022bff8fa90c5148ec35f5d64c",

            // mf2hd_xdf.xdf.lz
            "825ca9cdcb2f35ff8bbbda9cb0a27c4d"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "pce", "XDF");
        public override IMediaImage _plugin => new ZZZRawImage();
    }
}