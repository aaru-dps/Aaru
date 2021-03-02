// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
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

namespace Aaru.Tests.Images.pce
{
    [TestFixture]
    public class DiskCopy42 : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "mf1dd_gcr.dc42.lz", "mf2dd.dc42.lz", "mf2dd_gcr.dc42.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // mf1dd_gcr.dc42.lz
            800,

            // mf2dd.dc42.lz
            1440,

            // mf2dd_gcr.dc42.lz
            1600
        };

        public override uint[] _sectorSize => new uint[]
        {
            // mf1dd_gcr.dc42.lz
            512,

            // mf2dd.dc42.lz
            512,

            // mf2dd_gcr.dc42.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // mf1dd_gcr.dc42.lz
            MediaType.AppleSonySS,

            // mf2dd.dc42.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_gcr.dc42.lz
            MediaType.AppleSonyDS
        };

        public override string[] _md5S => new[]
        {
            // mf1dd_gcr.dc42.lz
            "c5d92544c3e78b7f0a9b4baaa9a64eec",

            // mf2dd.dc42.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2dd_gcr.dc42.lz
            "93e71b9ecdb39d3ec9245b4f451856d4"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "pce", "DiskCopy 4.2");
        public override IMediaImage _plugin => new DiscImages.DiskCopy42();
    }
}