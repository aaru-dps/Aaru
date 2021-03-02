// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Rdxx.cs
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

namespace Aaru.Tests.Images.DiskImagesFramework.NDIF
{
    [TestFixture]
    public class Rdxx : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "DOS_1440.img", "DOS_720.img", "DOS_DMF.img", "DOS_SP_5Mb.img", "HFS_1440.img", "HFS_800.img",
            "HFS_DMF.img", "HFSP_SP_5Mb.img", "HFS_SP_5Mb.img", "ProDOS_1440.img", "ProDOS_800.img", "ProDOS_DMF.img",
            "UFS_1440.img", "UFS_720.img", "UFS_800.img", "UFS_DMF.img"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // DOS_1440.img
            2880,

            // DOS_720.img
            1440,

            // DOS_DMF.img
            3360,

            // DOS_SP_5Mb.img
            10240,

            // HFS_1440.img
            2880,

            // HFS_800.img
            1600,

            // HFS_DMF.img
            3360,

            // HFSP_SP_5Mb.img
            10144,

            // HFS_SP_5Mb.img
            10240,

            // ProDOS_1440.img
            2880,

            // ProDOS_800.img
            1600,

            // ProDOS_DMF.img
            3360,

            // UFS_1440.img
            2880,

            // UFS_720.img
            1440,

            // UFS_800.img
            1600,

            // UFS_DMF.img
            3360
        };

        public override uint[] _sectorSize => new uint[]
        {
            // DOS_1440.img
            512,

            // DOS_720.img
            512,

            // DOS_DMF.img
            512,

            // DOS_SP_5Mb.img
            512,

            // HFS_1440.img
            512,

            // HFS_800.img
            512,

            // HFS_DMF.img
            512,

            // HFSP_SP_5Mb.img
            512,

            // HFS_SP_5Mb.img
            512,

            // ProDOS_1440.img
            512,

            // ProDOS_800.img
            512,

            // ProDOS_DMF.img
            512,

            // UFS_1440.img
            512,

            // UFS_720.img
            512,

            // UFS_800.img
            512,

            // UFS_DMF.img
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // DOS_1440.img
            MediaType.DOS_35_HD,

            // DOS_720.img
            MediaType.DOS_35_DS_DD_9,

            // DOS_DMF.img
            MediaType.DMF,

            // DOS_SP_5Mb.img
            MediaType.GENERIC_HDD,

            // HFS_1440.img
            MediaType.DOS_35_HD,

            // HFS_800.img
            MediaType.AppleSonyDS,

            // HFS_DMF.img
            MediaType.DMF,

            // HFSP_SP_5Mb.img
            MediaType.GENERIC_HDD,

            // HFS_SP_5Mb.img
            MediaType.GENERIC_HDD,

            // ProDOS_1440.img
            MediaType.DOS_35_HD,

            // ProDOS_800.img
            MediaType.AppleSonyDS,

            // ProDOS_DMF.img
            MediaType.DMF,

            // UFS_1440.img
            MediaType.DOS_35_HD,

            // UFS_720.img
            MediaType.DOS_35_DS_DD_9,

            // UFS_800.img
            MediaType.AppleSonyDS,

            // UFS_DMF.img
            MediaType.DMF
        };

        public override string[] _md5S => new[]
        {
            // DOS_1440.img
            "ff419213080574056ebd9adf7bab3d32",

            // DOS_720.img
            "c2be571406cf6353269faa59a4a8c0a4",

            // DOS_DMF.img
            "92ea7a359957012a682ba126cfdef0ce",

            // DOS_SP_5Mb.img
            "df3b4331a4a5652393ff55f001998439",

            // HFS_1440.img
            "3160038ca028ccf52ad7863790072145",

            // HFS_800.img
            "5e255c4bc0f6a26ecd27845b37e65aaa",

            // HFS_DMF.img
            "652dc979c177f2d8e846587158b38478",

            // HFSP_SP_5Mb.img
            "5841dbaceb4937df2518742c2d5cb8d5",

            // HFS_SP_5Mb.img
            "506c3deb99e78579b4d77e76224d3b4e",

            // ProDOS_1440.img
            "7975e8cf7579a6848d6fb4e546d1f682",

            // ProDOS_800.img
            "a72da7aedadbe194c22a3d71c62e4766",

            // ProDOS_DMF.img
            "7fbf0251a93cb36d98e68b7d19624de5",

            // UFS_1440.img
            "b37823c7a90d1917f719ba5927b23da8",

            // UFS_720.img
            "4942032f7bf1d115237ea1764424828b",

            // UFS_800.img
            "85574aebeef03eb355bf8541955d06ea",

            // UFS_DMF.img
            "cdfebf3f8b8f250dc6905a90dd1bc90f"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskImagesFramework", "NDIF", "Rdxx");
        public override IMediaImage _plugin => new Ndif();
    }
}