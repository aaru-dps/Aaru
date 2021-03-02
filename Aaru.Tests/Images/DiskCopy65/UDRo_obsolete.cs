// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDRo_obsolete.cs
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

namespace Aaru.Tests.Images.DiskCopy65
{
    [TestFixture]
    public class UDRo_obsolete : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "DC6_UDRo_DOS_1440.img.lz", "DC6_UDRo_DOS_720.img.lz", "DC6_UDRo_DOS_DMF.img.lz",
            "DC6_UDRo_HFS_1440.img.lz", "DC6_UDRo_HFS_800.img.lz", "DC6_UDRo_HFS_DMF.img.lz", "DC6_UDRo_PD_1440.img.lz",
            "DC6_UDRo_PD_800.img.lz", "DC6_UDRo_PD_DMF.img.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // DC6_UDRo_DOS_1440.img.lz
            2880,

            // DC6_UDRo_DOS_720.img.lz
            1440,

            // DC6_UDRo_DOS_DMF.img.lz
            3360,

            // DC6_UDRo_HFS_1440.img.lz
            2880,

            // DC6_UDRo_HFS_800.img.lz
            1600,

            // DC6_UDRo_HFS_DMF.img.lz
            3360,

            // DC6_UDRo_PD_1440.img.lz
            2880,

            // DC6_UDRo_PD_800.img.lz
            1600,

            // DC6_UDRo_PD_DMF.img.lz
            3360
        };

        public override uint[] _sectorSize => new uint[]
        {
            // DC6_UDRo_DOS_1440.img.lz
            512,

            // DC6_UDRo_DOS_720.img.lz
            512,

            // DC6_UDRo_DOS_DMF.img.lz
            512,

            // DC6_UDRo_HFS_1440.img.lz
            512,

            // DC6_UDRo_HFS_800.img.lz
            512,

            // DC6_UDRo_HFS_DMF.img.lz
            512,

            // DC6_UDRo_PD_1440.img.lz
            512,

            // DC6_UDRo_PD_800.img.lz
            512,

            // DC6_UDRo_PD_DMF.img.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // DC6_UDRo_DOS_1440.img.lz
            MediaType.DOS_35_HD,

            // DC6_UDRo_DOS_720.img.lz
            MediaType.DOS_35_DS_DD_9,

            // DC6_UDRo_DOS_DMF.img.lz
            MediaType.DMF,

            // DC6_UDRo_HFS_1440.img.lz
            MediaType.DOS_35_HD,

            // DC6_UDRo_HFS_800.img.lz
            MediaType.AppleSonyDS,

            // DC6_UDRo_HFS_DMF.img.lz
            MediaType.DMF,

            // DC6_UDRo_PD_1440.img.lz
            MediaType.DOS_35_HD,

            // DC6_UDRo_PD_800.img.lz
            MediaType.AppleSonyDS,

            // DC6_UDRo_PD_DMF.img.lz
            MediaType.DMF
        };

        public override string[] _md5S => new[]
        {
            // DC6_UDRo_DOS_1440.img.lz
            "ff419213080574056ebd9adf7bab3d32",

            // DC6_UDRo_DOS_720.img.lz
            "c2be571406cf6353269faa59a4a8c0a4",

            // DC6_UDRo_DOS_DMF.img.lz
            "92ea7a359957012a682ba126cfdef0ce",

            // DC6_UDRo_HFS_1440.img.lz
            "3160038ca028ccf52ad7863790072145",

            // DC6_UDRo_HFS_800.img.lz
            "5e255c4bc0f6a26ecd27845b37e65aaa",

            // DC6_UDRo_HFS_DMF.img.lz
            "652dc979c177f2d8e846587158b38478",

            // DC6_UDRo_PD_1440.img.lz
            "7975e8cf7579a6848d6fb4e546d1f682",

            // DC6_UDRo_PD_800.img.lz
            "a72da7aedadbe194c22a3d71c62e4766",

            // DC6_UDRo_PD_DMF.img.lz
            "7fbf0251a93cb36d98e68b7d19624de5"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskCopy 6.5", "UDIF", "UDRo_OBS");
        public override IMediaImage _plugin => new Udif();
    }
}