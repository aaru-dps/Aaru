// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DOS.cs
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

namespace Aaru.Tests.Images.AppleDOS
{
    [TestFixture]
    public class DOS : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "dos33.do.lz", "hfs.do.lz", "pascal800.do.lz", "pascal.do.lz", "prodos800.do.lz", "prodos.do.lz",
            "prodosmod.do.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // dos33.do.lz
            560,

            // hfs.do.lz
            560,

            // pascal800.do.lz
            560,

            // pascal.do.lz
            560,

            // prodos800.do.lz
            560,

            // prodos.do.lz
            560,

            // prodosmod.do.lz
            560
        };

        public override uint[] _sectorSize => new uint[]
        {
            // dos33.do.lz
            256,

            // hfs.do.lz
            256,

            // pascal800.do.lz
            256,

            // pascal.do.lz
            256,

            // prodos800.do.lz
            256,

            // prodos.do.lz
            256,

            // prodosmod.do.lz
            256
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // dos33.do.lz
            MediaType.Apple33SS,

            // hfs.do.lz
            MediaType.Apple33SS,

            // pascal800.do.lz
            MediaType.Apple33SS,

            // pascal.do.lz
            MediaType.Apple33SS,

            // prodos800.do.lz
            MediaType.Apple33SS,

            // prodos.do.lz
            MediaType.Apple33SS,

            // prodosmod.do.lz
            MediaType.Apple33SS
        };

        public override string[] _md5S => new[]
        {
            // dos33.do.lz
            "0ffcbd4180306192726926b43755db2f",

            // hfs.do.lz
            "ddd04ef378552c789f85382b4f49da06",

            // pascal800.do.lz
            "5158e2fe9d8e7ae1f7db73156478e4f4",

            // pascal.do.lz
            "4c4926103a32ac15f7e430ec3ced4be5",

            // prodos800.do.lz
            "193c5cc22f07e5aeb96eb187cb59c2d9",

            // prodos.do.lz
            "23f42e529c9fde2a8033f1bc6a7bca93",

            // prodosmod.do.lz
            "a7ec980472c320da5ea6f2f0aec0f502"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Apple DOS Order");
        public override IMediaImage _plugin => new AppleDos();
    }
}