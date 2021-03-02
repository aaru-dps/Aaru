// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDCO.cs
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

namespace Aaru.Tests.Images.DiskImagesFramework.UDIF
{
    [TestFixture]
    public class UDCO : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "DOS_1440.dmg.lz", "DOS_720.dmg.lz", "DOS_DMF.dmg.lz", "DOS_SP_5Mb.dmg.lz", "HFS_1440.dmg.lz",
            "HFS_800.dmg.lz", "HFS_DMF.dmg.lz", "HFSP_SP_5Mb.dmg.lz", "HFS_SP_5Mb.dmg.lz", "ProDOS_1440.dmg.lz",
            "ProDOS_800.dmg.lz", "ProDOS_DMF.dmg.lz", "UFS_1440.dmg.lz", "UFS_720.dmg.lz", "UFS_800.dmg.lz",
            "UFS_DMF.dmg.lz", "UFS_SP_5Mb.dmg.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // DOS_1440.dmg.lz
            2880,

            // DOS_720.dmg.lz
            1440,

            // DOS_DMF.dmg.lz
            3360,

            // DOS_SP_5Mb.dmg.lz
            10240,

            // HFS_1440.dmg.lz
            2880,

            // HFS_800.dmg.lz
            1600,

            // HFS_DMF.dmg.lz
            3360,

            // HFSP_SP_5Mb.dmg.lz
            10240,

            // HFS_SP_5Mb.dmg.lz
            10240,

            // ProDOS_1440.dmg.lz
            2880,

            // ProDOS_800.dmg.lz
            1600,

            // ProDOS_DMF.dmg.lz
            3360,

            // UFS_1440.dmg.lz
            2880,

            // UFS_720.dmg.lz
            1440,

            // UFS_800.dmg.lz
            1600,

            // UFS_DMF.dmg.lz
            3360,

            // UFS_SP_5Mb.dmg.lz
            10304
        };

        public override uint[] _sectorSize => new uint[]
        {
            // DOS_1440.dmg.lz
            512,

            // DOS_720.dmg.lz
            512,

            // DOS_DMF.dmg.lz
            512,

            // DOS_SP_5Mb.dmg.lz
            512,

            // HFS_1440.dmg.lz
            512,

            // HFS_800.dmg.lz
            512,

            // HFS_DMF.dmg.lz
            512,

            // HFSP_SP_5Mb.dmg.lz
            512,

            // HFS_SP_5Mb.dmg.lz
            512,

            // ProDOS_1440.dmg.lz
            512,

            // ProDOS_800.dmg.lz
            512,

            // ProDOS_DMF.dmg.lz
            512,

            // UFS_1440.dmg.lz
            512,

            // UFS_720.dmg.lz
            512,

            // UFS_800.dmg.lz
            512,

            // UFS_DMF.dmg.lz
            512,

            // UFS_SP_5Mb.dmg.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // DOS_1440.dmg.lz
            MediaType.GENERIC_HDD,

            // DOS_720.dmg.lz
            MediaType.GENERIC_HDD,

            // DOS_DMF.dmg.lz
            MediaType.GENERIC_HDD,

            // DOS_SP_5Mb.dmg.lz
            MediaType.GENERIC_HDD,

            // HFS_1440.dmg.lz
            MediaType.GENERIC_HDD,

            // HFS_800.dmg.lz
            MediaType.GENERIC_HDD,

            // HFS_DMF.dmg.lz
            MediaType.GENERIC_HDD,

            // HFSP_SP_5Mb.dmg.lz
            MediaType.GENERIC_HDD,

            // HFS_SP_5Mb.dmg.lz
            MediaType.GENERIC_HDD,

            // ProDOS_1440.dmg.lz
            MediaType.GENERIC_HDD,

            // ProDOS_800.dmg.lz
            MediaType.GENERIC_HDD,

            // ProDOS_DMF.dmg.lz
            MediaType.GENERIC_HDD,

            // UFS_1440.dmg.lz
            MediaType.GENERIC_HDD,

            // UFS_720.dmg.lz
            MediaType.GENERIC_HDD,

            // UFS_800.dmg.lz
            MediaType.GENERIC_HDD,

            // UFS_DMF.dmg.lz
            MediaType.GENERIC_HDD,

            // UFS_SP_5Mb.dmg.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // DOS_1440.dmg.lz
            "ff419213080574056ebd9adf7bab3d32",

            // DOS_720.dmg.lz
            "c2be571406cf6353269faa59a4a8c0a4",

            // DOS_DMF.dmg.lz
            "92ea7a359957012a682ba126cfdef0ce",

            // DOS_SP_5Mb.dmg.lz
            "df3b4331a4a5652393ff55f001998439",

            // HFS_1440.dmg.lz
            "3160038ca028ccf52ad7863790072145",

            // HFS_800.dmg.lz
            "5e255c4bc0f6a26ecd27845b37e65aaa",

            // HFS_DMF.dmg.lz
            "652dc979c177f2d8e846587158b38478",

            // HFSP_SP_5Mb.dmg.lz
            "894fa8596f64e161fe7d7f81e74a8839",

            // HFS_SP_5Mb.dmg.lz
            "506c3deb99e78579b4d77e76224d3b4e",

            // ProDOS_1440.dmg.lz
            "7975e8cf7579a6848d6fb4e546d1f682",

            // ProDOS_800.dmg.lz
            "a72da7aedadbe194c22a3d71c62e4766",

            // ProDOS_DMF.dmg.lz
            "7fbf0251a93cb36d98e68b7d19624de5",

            // UFS_1440.dmg.lz
            "b37823c7a90d1917f719ba5927b23da8",

            // UFS_720.dmg.lz
            "4942032f7bf1d115237ea1764424828b",

            // UFS_800.dmg.lz
            "85574aebeef03eb355bf8541955d06ea",

            // UFS_DMF.dmg.lz
            "cdfebf3f8b8f250dc6905a90dd1bc90f",

            // UFS_SP_5Mb.dmg.lz
            "b7d4ad55c7702658081b6578b588a57f"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskImagesFramework", "UDIF", "UDCO");
        public override IMediaImage _plugin => new Udif();
    }
}