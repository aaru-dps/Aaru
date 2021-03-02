// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : TeleDisk.cs
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
    public class TeleDisk : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "md2dd8.td0.lz", "md2dd_fdformat_f400.td0.lz", "md2dd_fdformat_f410.td0.lz", "md2dd_fdformat_f720.td0.lz",
            "md2dd_fdformat_f800.td0.lz", "md2dd_fdformat_f820.td0.lz", "md2dd_freedos_800s.td0.lz",
            "md2dd_maxiform_1640s.td0.lz", "md2dd_maxiform_840s.td0.lz", "md2dd_qcopy_1476s.td0.lz",
            "md2dd_qcopy_1600s.td0.lz", "md2dd_qcopy_1640s.td0.lz", "md2dd.td0.lz", "md2hd_fdformat_f144.td0.lz",
            "md2hd_fdformat_f148.td0.lz", "md2hd_maxiform_2788s.td0.lz", "md2hd.td0.lz", "md2hd_xdf.td0.lz",
            "mf2dd_fdformat_800.td0.lz", "mf2dd_fdformat_820.td0.lz", "mf2dd_fdformat_f800.td0.lz",
            "mf2dd_fdformat_f820.td0.lz", "mf2dd_freedos_1600s.td0.lz", "mf2dd_freedos.td0.lz",
            "mf2dd_maxiform_1600s.td0.lz", "mf2dd_qcopy_1494s.td0.lz", "mf2dd_qcopy_1600s.td0.lz",
            "mf2dd_qcopy_1660s.td0.lz", "mf2dd.td0.lz", /*
"mf2hd_2mgui.td0.lz",*/"mf2hd_2m_max.td0.lz", "mf2hd_2m.td0.lz", "mf2hd_alt_adv.td0.lz", "mf2hd_alt_dos_adv.td0.lz",
            "mf2hd_alt_dos.td0.lz", "mf2hd_alt.td0.lz", "mf2hd_dmf.td0.lz", "mf2hd_fdformat_168.td0.lz",
            "mf2hd_fdformat_172.td0.lz", "mf2hd_fdformat_f168.td0.lz", "mf2hd_fdformat_f16.td0.lz",
            "mf2hd_fdformat_f172.td0.lz", "mf2hd_freedos_3360s.td0.lz", "mf2hd_freedos_3486s.td0.lz",
            "mf2hd_freedos.td0.lz", "mf2hd_maxiform_3200s.td0.lz", "mf2hd_qcopy_2460s.td0.lz",
            "mf2hd_qcopy_2720s.td0.lz", "mf2hd_qcopy_2788s.td0.lz", "mf2hd_qcopy_2880s.td0.lz",
            "mf2hd_qcopy_2952s.td0.lz", "mf2hd_qcopy_3200s.td0.lz", "mf2hd_qcopy_3320s.td0.lz",
            "mf2hd_qcopy_3360s.td0.lz", "mf2hd_qcopy_3486s.td0.lz", "mf2hd.td0.lz", "mf2hd_xdf_adv.td0.lz",
            "mf2hd_xdf.td0.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // md2dd8.td0.lz
            640,

            // md2dd_fdformat_f400.td0.lz
            800,

            // md2dd_fdformat_f410.td0.lz
            820,

            // md2dd_fdformat_f720.td0.lz
            1440,

            // md2dd_fdformat_f800.td0.lz
            1600,

            // md2dd_fdformat_f820.td0.lz
            1640,

            // md2dd_freedos_800s.td0.lz
            800,

            // md2dd_maxiform_1640s.td0.lz
            1640,

            // md2dd_maxiform_840s.td0.lz
            840,

            // md2dd_qcopy_1476s.td0.lz
            1476,

            // md2dd_qcopy_1600s.td0.lz
            1600,

            // md2dd_qcopy_1640s.td0.lz
            1640,

            // md2dd.td0.lz
            720,

            // md2hd_fdformat_f144.td0.lz
            2880,

            // md2hd_fdformat_f148.td0.lz
            2952,

            // md2hd_maxiform_2788s.td0.lz
            2788,

            // md2hd.td0.lz
            2400,

            // md2hd_xdf.td0.lz
            640,

            // mf2dd_fdformat_800.td0.lz
            1600,

            // mf2dd_fdformat_820.td0.lz
            1640,

            // mf2dd_fdformat_f800.td0.lz
            1600,

            // mf2dd_fdformat_f820.td0.lz
            1640,

            // mf2dd_freedos_1600s.td0.lz
            1600,

            // mf2dd_freedos.td0.lz
            1640,

            // mf2dd_maxiform_1600s.td0.lz
            1600,

            // mf2dd_qcopy_1494s.td0.lz
            1494,

            // mf2dd_qcopy_1600s.td0.lz
            1600,

            // mf2dd_qcopy_1660s.td0.lz
            1660,

            // mf2dd.td0.lz
            1440, /*
// mf2hd_2mgui.td0.lz
0,*/
            // mf2hd_2m_max.td0.lz
            1148,

            // mf2hd_2m.td0.lz
            1804,

            // mf2hd_alt_adv.td0.lz
            2880,

            // mf2hd_alt_dos_adv.td0.lz
            2880,

            // mf2hd_alt_dos.td0.lz
            2880,

            // mf2hd_alt.td0.lz
            2880,

            // mf2hd_dmf.td0.lz
            3360,

            // mf2hd_fdformat_168.td0.lz
            332,

            // mf2hd_fdformat_172.td0.lz
            3444,

            // mf2hd_fdformat_f168.td0.lz
            3360,

            // mf2hd_fdformat_f16.td0.lz
            3200,

            // mf2hd_fdformat_f172.td0.lz
            3444,

            // mf2hd_freedos_3360s.td0.lz
            3360,

            // mf2hd_freedos_3486s.td0.lz
            3486,

            // mf2hd_freedos.td0.lz
            3486,

            // mf2hd_maxiform_3200s.td0.lz
            3200,

            // mf2hd_qcopy_2460s.td0.lz
            2460,

            // mf2hd_qcopy_2720s.td0.lz
            2720,

            // mf2hd_qcopy_2788s.td0.lz
            2788,

            // mf2hd_qcopy_2880s.td0.lz
            2880,

            // mf2hd_qcopy_2952s.td0.lz
            2952,

            // mf2hd_qcopy_3200s.td0.lz
            3200,

            // mf2hd_qcopy_3320s.td0.lz
            3320,

            // mf2hd_qcopy_3360s.td0.lz
            3360,

            // mf2hd_qcopy_3486s.td0.lz
            3486,

            // mf2hd.td0.lz
            2880,

            // mf2hd_xdf_adv.td0.lz
            640,

            // mf2hd_xdf.td0.lz
            640
        };

        public override uint[] _sectorSize => new uint[]
        {
            // md2dd8.td0.lz
            512,

            // md2dd_fdformat_f400.td0.lz
            512,

            // md2dd_fdformat_f410.td0.lz
            512,

            // md2dd_fdformat_f720.td0.lz
            512,

            // md2dd_fdformat_f800.td0.lz
            512,

            // md2dd_fdformat_f820.td0.lz
            512,

            // md2dd_freedos_800s.td0.lz
            512,

            // md2dd_maxiform_1640s.td0.lz
            512,

            // md2dd_maxiform_840s.td0.lz
            512,

            // md2dd_qcopy_1476s.td0.lz
            512,

            // md2dd_qcopy_1600s.td0.lz
            512,

            // md2dd_qcopy_1640s.td0.lz
            512,

            // md2dd.td0.lz
            512,

            // md2hd_fdformat_f144.td0.lz
            512,

            // md2hd_fdformat_f148.td0.lz
            512,

            // md2hd_maxiform_2788s.td0.lz
            512,

            // md2hd.td0.lz
            512,

            // md2hd_xdf.td0.lz
            128,

            // mf2dd_fdformat_800.td0.lz
            512,

            // mf2dd_fdformat_820.td0.lz
            512,

            // mf2dd_fdformat_f800.td0.lz
            512,

            // mf2dd_fdformat_f820.td0.lz
            512,

            // mf2dd_freedos_1600s.td0.lz
            512,

            // mf2dd_freedos.td0.lz
            512,

            // mf2dd_maxiform_1600s.td0.lz
            512,

            // mf2dd_qcopy_1494s.td0.lz
            512,

            // mf2dd_qcopy_1600s.td0.lz
            512,

            // mf2dd_qcopy_1660s.td0.lz
            512,

            // mf2dd.td0.lz
            512,

            // mf2hd_2mgui.td0.lz
            //0,
            // mf2hd_2m_max.td0.lz
            512,

            // mf2hd_2m.td0.lz
            512,

            // mf2hd_alt_adv.td0.lz
            512,

            // mf2hd_alt_dos_adv.td0.lz
            512,

            // mf2hd_alt_dos.td0.lz
            512,

            // mf2hd_alt.td0.lz
            512,

            // mf2hd_dmf.td0.lz
            512,

            // mf2hd_fdformat_168.td0.lz
            512,

            // mf2hd_fdformat_172.td0.lz
            512,

            // mf2hd_fdformat_f168.td0.lz
            512,

            // mf2hd_fdformat_f16.td0.lz
            512,

            // mf2hd_fdformat_f172.td0.lz
            512,

            // mf2hd_freedos_3360s.td0.lz
            512,

            // mf2hd_freedos_3486s.td0.lz
            512,

            // mf2hd_freedos.td0.lz
            512,

            // mf2hd_maxiform_3200s.td0.lz
            512,

            // mf2hd_qcopy_2460s.td0.lz
            512,

            // mf2hd_qcopy_2720s.td0.lz
            512,

            // mf2hd_qcopy_2788s.td0.lz
            512,

            // mf2hd_qcopy_2880s.td0.lz
            512,

            // mf2hd_qcopy_2952s.td0.lz
            512,

            // mf2hd_qcopy_3200s.td0.lz
            512,

            // mf2hd_qcopy_3320s.td0.lz
            512,

            // mf2hd_qcopy_3360s.td0.lz
            512,

            // mf2hd_qcopy_3486s.td0.lz
            512,

            // mf2hd.td0.lz
            512,

            // mf2hd_xdf_adv.td0.lz
            512,

            // mf2hd_xdf.td0.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // md2dd8.td0.lz
            MediaType.DOS_525_DS_DD_8,

            // md2dd_fdformat_f400.td0.lz
            MediaType.Unknown,

            // md2dd_fdformat_f410.td0.lz
            MediaType.Unknown,

            // md2dd_fdformat_f720.td0.lz
            MediaType.ECMA_78_2,

            // md2dd_fdformat_f800.td0.lz
            MediaType.Unknown,

            // md2dd_fdformat_f820.td0.lz
            MediaType.FDFORMAT_525_DD,

            // md2dd_freedos_800s.td0.lz
            MediaType.Unknown,

            // md2dd_maxiform_1640s.td0.lz
            MediaType.FDFORMAT_525_DD,

            // md2dd_maxiform_840s.td0.lz
            MediaType.Unknown,

            // md2dd_qcopy_1476s.td0.lz
            MediaType.Unknown,

            // md2dd_qcopy_1600s.td0.lz
            MediaType.Unknown,

            // md2dd_qcopy_1640s.td0.lz
            MediaType.FDFORMAT_525_DD,

            // md2dd.td0.lz
            MediaType.DOS_525_DS_DD_9,

            // md2hd_fdformat_f144.td0.lz
            MediaType.Unknown,

            // md2hd_fdformat_f148.td0.lz
            MediaType.Unknown,

            // md2hd_maxiform_2788s.td0.lz
            MediaType.Unknown,

            // md2hd.td0.lz
            MediaType.DOS_525_HD,

            // md2hd_xdf.td0.lz
            MediaType.Unknown,

            // mf2dd_fdformat_800.td0.lz
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_820.td0.lz
            MediaType.Unknown,

            // mf2dd_fdformat_f800.td0.lz
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_f820.td0.lz
            MediaType.Unknown,

            // mf2dd_freedos_1600s.td0.lz
            MediaType.CBM_35_DD,

            // mf2dd_freedos.td0.lz
            MediaType.Unknown,

            // mf2dd_maxiform_1600s.td0.lz
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1494s.td0.lz
            MediaType.Unknown,

            // mf2dd_qcopy_1600s.td0.lz
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1660s.td0.lz
            MediaType.Unknown,

            // mf2dd.td0.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2hd_2mgui.td0.lz
            //MediaType.CD,
            // mf2hd_2m_max.td0.lz
            MediaType.Unknown,

            // mf2hd_2m.td0.lz
            MediaType.Unknown,

            // mf2hd_alt_adv.td0.lz
            MediaType.Unknown,

            // mf2hd_alt_dos_adv.td0.lz
            MediaType.DOS_35_HD,

            // mf2hd_alt_dos.td0.lz
            MediaType.DOS_35_HD,

            // mf2hd_alt.td0.lz
            MediaType.Unknown,

            // mf2hd_dmf.td0.lz
            MediaType.DMF,

            // mf2hd_fdformat_168.td0.lz
            MediaType.Unknown,

            // mf2hd_fdformat_172.td0.lz
            MediaType.Unknown,

            // mf2hd_fdformat_f168.td0.lz
            MediaType.DMF,

            // mf2hd_fdformat_f16.td0.lz
            MediaType.Unknown,

            // mf2hd_fdformat_f172.td0.lz
            MediaType.DMF_82,

            // mf2hd_freedos_3360s.td0.lz
            MediaType.DMF,

            // mf2hd_freedos_3486s.td0.lz
            MediaType.Unknown,

            // mf2hd_freedos.td0.lz
            MediaType.Unknown,

            // mf2hd_maxiform_3200s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2460s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2720s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2788s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2880s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2952s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3200s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3320s.td0.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3360s.td0.lz
            MediaType.DMF,

            // mf2hd_qcopy_3486s.td0.lz
            MediaType.Unknown,

            // mf2hd.td0.lz
            MediaType.Unknown,

            // mf2hd_xdf_adv.td0.lz
            MediaType.XDF_35,

            // mf2hd_xdf.td0.lz
            MediaType.XDF_35
        };

        public override string[] _md5S => new[]
        {
            // md2dd8.td0.lz
            "beef1cdb004dc69391d6b3d508988b95",

            // md2dd_fdformat_f400.td0.lz
            "0aef12c906b744101b932d799ca88a78",

            // md2dd_fdformat_f410.td0.lz
            "348d12add1ed226cd712a4a6a10d1a34",

            // md2dd_fdformat_f720.td0.lz
            "1c36b819cfe355c11360bc120c9216fe",

            // md2dd_fdformat_f800.td0.lz
            "25114403c11e337480e2afc4e6e32108",

            // md2dd_fdformat_f820.td0.lz
            "3d7760ddaa55cd258057773d15106b78",

            // md2dd_freedos_800s.td0.lz
            "29054ef703394ee3b35e849468a412ba",

            // md2dd_maxiform_1640s.td0.lz
            "c91e852828c2aeee2fc94a6adbeed0ae",

            // md2dd_maxiform_840s.td0.lz
            "efb6cfe53a6770f0ae388cb2c7f46264",

            // md2dd_qcopy_1476s.td0.lz
            "6116f7c1397cadd55ba8d79c2aadc9dd",

            // md2dd_qcopy_1600s.td0.lz
            "93100f8d86e5d0d0e6340f59c52a5e0d",

            // md2dd_qcopy_1640s.td0.lz
            "cf7b7d43aa70863bedcc4a8432a5af67",

            // md2dd.td0.lz
            "6213897b7dbf263f12abf76901d43862",

            // md2hd_fdformat_f144.td0.lz
            "073a172879a71339ef4b00ebb47b67fc",

            // md2hd_fdformat_f148.td0.lz
            "d9890897130d0fc1eee3dbf4d9b0440f",

            // md2hd_maxiform_2788s.td0.lz
            "09ca721aa883d5bbaa422c7943b0782c",

            // md2hd.td0.lz
            "02259cd5fbcc20f8484aa6bece7a37c6",

            // md2hd_xdf.td0.lz
            "b903ea7e0c9d7e4c6251df4825212db4",

            // mf2dd_fdformat_800.td0.lz
            "c533488a21098a62c85f1649abda2803",

            // mf2dd_fdformat_820.td0.lz
            "db9cfb6eea18820b7a7e0b5b45594471",

            // mf2dd_fdformat_f800.td0.lz
            "26532a62985b51a2c3b877a57f6d257b",

            // mf2dd_fdformat_f820.td0.lz
            "a7771acff766557cc23b8c6943b588f9",

            // mf2dd_freedos_1600s.td0.lz
            "d07f7ffaee89742c6477aaaf94eb5715",

            // mf2dd_freedos.td0.lz
            "456390a9c6ab05cb458a03c47296de08",

            // mf2dd_maxiform_1600s.td0.lz
            "56af87802a9852e6e01e08d544740816",

            // mf2dd_qcopy_1494s.td0.lz
            "fd7fb1ba11cdfe11db54af0322abf59d",

            // mf2dd_qcopy_1600s.td0.lz
            "d9db52d992a76bf3bbc626ff844215a5",

            // mf2dd_qcopy_1660s.td0.lz
            "5949d0be57ce8bffcda7c4be4d1348ee",

            // mf2dd.td0.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2hd_2mgui.td0.lz
            //"UNKNOWN",
            // mf2hd_2m_max.td0.lz
            "4b88a3e43b57778422e8b1e851a9c902",

            // mf2hd_2m.td0.lz
            "d032d928c43b66419b7404b016ec07ff",

            // mf2hd_alt_adv.td0.lz
            "1d32a686b7675c7a4f88c15522738432",

            // mf2hd_alt_dos_adv.td0.lz
            "8aea37782c507baf6b294467249b4608",

            // mf2hd_alt_dos.td0.lz
            "8aea37782c507baf6b294467249b4608",

            // mf2hd_alt.td0.lz
            "1d32a686b7675c7a4f88c15522738432",

            // mf2hd_dmf.td0.lz
            "28764d4f69c3865e2af71a41ca3f432f",

            // mf2hd_fdformat_168.td0.lz
            "62b900808c3e9f91f8361fd1716155a1",

            // mf2hd_fdformat_172.td0.lz
            "9dea1e119a73a21a38d134f36b2e5564",

            // mf2hd_fdformat_f168.td0.lz
            "7e3bf04f3660dd1052a335dc99441e44",

            // mf2hd_fdformat_f16.td0.lz
            "8eb8cb310feaf03c69fffd4f6e729847",

            // mf2hd_fdformat_f172.td0.lz
            "a58fd062f024b95714f1223a8bc2232f",

            // mf2hd_freedos_3360s.td0.lz
            "2bfd2e0a81bad704f8fc7758358cfcca",

            // mf2hd_freedos_3486s.td0.lz
            "a79ec33c623697b4562dacaed31523b8",

            // mf2hd_freedos.td0.lz
            "dbd52e9e684f97d9e2292811242bb24e",

            // mf2hd_maxiform_3200s.td0.lz
            "3c4becd695ed25866d39966a9a93c2d9",

            // mf2hd_qcopy_2460s.td0.lz
            "72282e11f7d91bf9c090b550fabfe80d",

            // mf2hd_qcopy_2720s.td0.lz
            "457c1126dc7f36bbbabe9e17e90372e3",

            // mf2hd_qcopy_2788s.td0.lz
            "852181d5913c6f290872c66bbe992314",

            // mf2hd_qcopy_2880s.td0.lz
            "2980cc32504c945598dc50f1db576994",

            // mf2hd_qcopy_2952s.td0.lz
            "c1c58d74fffb3656dd7f60f74ae8a629",

            // mf2hd_qcopy_3200s.td0.lz
            "e45d41a61fbe48f328c995fcc10a5548",

            // mf2hd_qcopy_3320s.td0.lz
            "c25f2a57c71db1cd4fea2263598f544a",

            // mf2hd_qcopy_3360s.td0.lz
            "15f71b92bd72aba5d80bf70eca4d5b1e",

            // mf2hd_qcopy_3486s.td0.lz
            "d88c8d818e238c9e52b8588b5fd52efe",

            // mf2hd.td0.lz
            "b4a602f67903c46eef62addb0780aa56",

            // mf2hd_xdf_adv.td0.lz
            "728f9361203dc39961b1413aa050f70d",

            // mf2hd_xdf.td0.lz
            "728f9361203dc39961b1413aa050f70d"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "TeleDisk");
        public override IMediaImage _plugin => new DiscImages.TeleDisk();
    }
}