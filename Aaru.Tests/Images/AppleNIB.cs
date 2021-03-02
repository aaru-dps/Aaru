// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleNIB.cs
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
    public class AppleNib : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "dos32.nib.lz", "dos33.nib.lz", "pascal.nib.lz", "prodos.nib.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // dos32.nib.lz
            455,

            // dos33.nib.lz
            560,

            // pascal.nib.lz
            560,

            // prodos.nib.lz
            560
        };

        public override uint[] _sectorSize => new uint[]
        {
            // dos32.nib.lz
            256,

            // dos33.nib.lz
            256,

            // pascal.nib.lz
            256,

            // prodos.nib.lz
            256
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // dos32.nib.lz
            MediaType.Apple32SS,

            // dos33.nib.lz
            MediaType.Apple33SS,

            // pascal.nib.lz
            MediaType.Apple33SS,

            // prodos.nib.lz
            MediaType.Apple33SS
        };

        public override string[] _md5S => new[]
        {
            // dos32.nib.lz
            "76f8fe4c5bc1976f99641ad7cdf53109",

            // dos33.nib.lz
            "0ffcbd4180306192726926b43755db2f",

            // pascal.nib.lz
            "4c4926103a32ac15f7e430ec3ced4be5",

            // prodos.nib.lz
            "11ef56c80c94347d2e3f921d5c36c8de"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Nibbles");
        public override IMediaImage _plugin => new DiscImages.AppleNib();
    }
}