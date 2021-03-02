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

namespace Aaru.Tests.Images.ShrinkWrap
{
    [TestFixture]
    public class DiskCopy42 : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "DC6_RW_HFS_1440.image.lz", "DC6_RW_HFS_800.image.lz", "DOS1440.image.lz", "DOS720.image.lz",
            "PD1440.image.lz", "PD800.image.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // DC6_RW_HFS_1440.image.lz
            2880,

            // DC6_RW_HFS_800.image.lz
            1600,

            // DOS1440.image.lz
            2880,

            // DOS720.image.lz
            1440,

            // PD1440.image.lz
            2880,

            // PD800.image.lz
            1600
        };

        public override uint[] _sectorSize => new uint[]
        {
            // DC6_RW_HFS_1440.image.lz
            512,

            // DC6_RW_HFS_800.image.lz
            512,

            // DOS1440.image.lz
            512,

            // DOS720.image.lz
            512,

            // PD1440.image.lz
            512,

            // PD800.image.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // DC6_RW_HFS_1440.image.lz
            MediaType.DOS_35_HD,

            // DC6_RW_HFS_800.image.lz
            MediaType.AppleSonyDS,

            // DOS1440.image.lz
            MediaType.DOS_35_HD,

            // DOS720.image.lz
            MediaType.DOS_35_DS_DD_9,

            // PD1440.image.lz
            MediaType.DOS_35_HD,

            // PD800.image.lz
            MediaType.AppleSonyDS
        };

        public override string[] _md5S => new[]
        {
            // DC6_RW_HFS_1440.image.lz
            "3160038ca028ccf52ad7863790072145",

            // DC6_RW_HFS_800.image.lz
            "5e255c4bc0f6a26ecd27845b37e65aaa",

            // DOS1440.image.lz
            "ff419213080574056ebd9adf7bab3d32",

            // DOS720.image.lz
            "c2be571406cf6353269faa59a4a8c0a4",

            // PD1440.image.lz
            "7975e8cf7579a6848d6fb4e546d1f682",

            // PD800.image.lz
            "a72da7aedadbe194c22a3d71c62e4766"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "ShrinkWrap 3", "DiskCopy 4.2");
        public override IMediaImage _plugin => new DiscImages.DiskCopy42();
    }
}