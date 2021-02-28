// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SaveDskF.cs
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

using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.Filters;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class SaveDskF
    {
        readonly string[] _testFiles =
        {
            "5dd8_c.dsk", "5dd8_ck.dsk", "5dd8_na.dsk", "5dd8_nak.dsk", "5dd8_n.dsk", "5dd8_nk.dsk", "5dd_c.dsk",
            "5dd_ck.dsk", "5dd_na.dsk", "5dd_nak.dsk", "5dd_n.dsk", "5dd_nk.dsk", "5hd_c.dsk", "5hd_ck.dsk",
            "5hd_na.dsk", "5hd_nak.dsk", "5hd_n.dsk", "5hd_nk.dsk", "5sd8_c.dsk", "5sd8_ck.dsk", "5sd8_na.dsk",
            "5sd8_nak.dsk", "5sd8_n.dsk", "5sd8_nk.dsk", "5sd_c.dsk", "5sd_ck.dsk", "5sd_na.dsk", "5sd_nak.dsk",
            "5sd_n.dsk", "5sd_nk.dsk", "md1dd8.dsk", "md1dd.dsk", "md1dd_fdformat_f200.dsk", "md1dd_fdformat_f205.dsk",
            "md2dd_2m_fast.dsk", "md2dd_2m_max.dsk", "md2dd8.dsk", "md2dd.dsk", "md2dd_fdformat_f400.dsk",
            "md2dd_fdformat_f410.dsk", "md2dd_fdformat_f720.dsk", "md2dd_fdformat_f800.dsk", "md2dd_fdformat_f820.dsk",
            "md2dd_freedos_800s.dsk", "md2dd_maxiform_1640s.dsk", "md2dd_maxiform_840s.dsk", "md2dd_qcopy_1476s.dsk",
            "md2dd_qcopy_1600s.dsk", "md2dd_qcopy_1640s.dsk", "md2hd_2m_fast.dsk", "md2hd_2m_max.dsk", "md2hd.dsk",
            "md2hd_fdformat_f144.dsk", "md2hd_fdformat_f148.dsk", "md2hd_maxiform_2788s.dsk", "md2hd_xdf.dsk",
            "mf2dd_2m.dsk", "mf2dd_2m_fast.dsk", "mf2dd_2mgui.dsk", "mf2dd_2m_max.dsk", "mf2dd_c.dsk", "mf2dd_ck.dsk",
            "mf2dd.dsk", "mf2dd_fdformat_800.dsk", "mf2dd_fdformat_820.dsk", "mf2dd_fdformat_f800.dsk",
            "mf2dd_fdformat_f820.dsk", "mf2dd_freedos_1600s.dsk", "mf2dd_maxiform_1600s.dsk", "mf2dd_na.dsk",
            "mf2dd_nak.dsk", "mf2dd_n.dsk", "mf2dd_nk.dsk", "mf2dd_qcopy_1494s.dsk", "mf2dd_qcopy_1600s.dsk",
            "mf2dd_qcopy_1660s.dsk", "mf2ed_c.dsk", "mf2ed_ck.dsk", "mf2ed.dsk", "mf2ed_na.dsk", "mf2ed_nak.dsk",
            "mf2ed_n.dsk", "mf2ed_nk.dsk", "mf2hd_2m.dsk", "mf2hd_2m_fast.dsk", "mf2hd_2mgui.dsk", "mf2hd_2m_max.dsk",
            "mf2hd_c.dsk", "mf2hd_ck.dsk", "mf2hd_dmf.dsk", "mf2hd.dsk", "mf2hd_fdformat_168.dsk",
            "mf2hd_fdformat_172.dsk", "mf2hd_fdformat_f168.dsk", "mf2hd_fdformat_f16.dsk", "mf2hd_fdformat_f172.dsk",
            "mf2hd_freedos_3360s.dsk", "mf2hd_freedos_3486s.dsk", "mf2hd_maxiform_3200s.dsk", "mf2hd_na.dsk",
            "mf2hd_nak.dsk", "mf2hd_n.dsk", "mf2hd_nk.dsk", "mf2hd_qcopy_2460s.dsk", "mf2hd_qcopy_2720s.dsk",
            "mf2hd_qcopy_2788s.dsk", "mf2hd_qcopy_2880s.dsk", "mf2hd_qcopy_2952s.dsk", "mf2hd_qcopy_2988s.dsk",
            "mf2hd_qcopy_3200s.dsk", "mf2hd_qcopy_3320s.dsk", "mf2hd_qcopy_3360s.dsk", "mf2hd_qcopy_3486s.dsk",
            "mf2hd_xdf_c.dsk", "mf2hd_xdf_ck.dsk", "mf2hd_xdf.dsk", "mf2hd_xdf_na.dsk", "mf2hd_xdf_nak.dsk",
            "mf2hd_xdf_n.dsk", "mf2hd_xdf_nk.dsk"
        };

        readonly ulong[] _sectors =
        {
            // 5dd8_c.dsk
            640,

            // 5dd8_ck.dsk
            640,

            // 5dd8_na.dsk
            640,

            // 5dd8_nak.dsk
            640,

            // 5dd8_n.dsk
            640,

            // 5dd8_nk.dsk
            640,

            // 5dd_c.dsk
            720,

            // 5dd_ck.dsk
            720,

            // 5dd_na.dsk
            720,

            // 5dd_nak.dsk
            720,

            // 5dd_n.dsk
            720,

            // 5dd_nk.dsk
            720,

            // 5hd_c.dsk
            2400,

            // 5hd_ck.dsk
            2400,

            // 5hd_na.dsk
            2400,

            // 5hd_nak.dsk
            2400,

            // 5hd_n.dsk
            2400,

            // 5hd_nk.dsk
            2400,

            // 5sd8_c.dsk
            320,

            // 5sd8_ck.dsk
            320,

            // 5sd8_na.dsk
            320,

            // 5sd8_nak.dsk
            320,

            // 5sd8_n.dsk
            320,

            // 5sd8_nk.dsk
            320,

            // 5sd_c.dsk
            360,

            // 5sd_ck.dsk
            360,

            // 5sd_na.dsk
            360,

            // 5sd_nak.dsk
            360,

            // 5sd_n.dsk
            360,

            // 5sd_nk.dsk
            360,

            // md1dd8.dsk
            320,

            // md1dd.dsk
            360,

            // md1dd_fdformat_f200.dsk
            400,

            // md1dd_fdformat_f205.dsk
            410,

            // md2dd_2m_fast.dsk
            1640,

            // md2dd_2m_max.dsk
            1804,

            // md2dd8.dsk
            640,

            // md2dd.dsk
            720,

            // md2dd_fdformat_f400.dsk
            800,

            // md2dd_fdformat_f410.dsk
            820,

            // md2dd_fdformat_f720.dsk
            1440,

            // md2dd_fdformat_f800.dsk
            1600,

            // md2dd_fdformat_f820.dsk
            1640,

            // md2dd_freedos_800s.dsk
            800,

            // md2dd_maxiform_1640s.dsk
            1640,

            // md2dd_maxiform_840s.dsk
            840,

            // md2dd_qcopy_1476s.dsk
            1476,

            // md2dd_qcopy_1600s.dsk
            1600,

            // md2dd_qcopy_1640s.dsk
            1640,

            // md2hd_2m_fast.dsk
            2952,

            // md2hd_2m_max.dsk
            3116,

            // md2hd.dsk
            2400,

            // md2hd_fdformat_f144.dsk
            2880,

            // md2hd_fdformat_f148.dsk
            2952,

            // md2hd_maxiform_2788s.dsk
            2788,

            // md2hd_xdf.dsk
            3040,

            // mf2dd_2m.dsk
            1968,

            // mf2dd_2m_fast.dsk
            1968,

            // mf2dd_2mgui.dsk
            9408,

            // mf2dd_2m_max.dsk
            2132,

            // mf2dd_c.dsk
            1440,

            // mf2dd_ck.dsk
            1440,

            // mf2dd.dsk
            1440,

            // mf2dd_fdformat_800.dsk
            1600,

            // mf2dd_fdformat_820.dsk
            1640,

            // mf2dd_fdformat_f800.dsk
            1600,

            // mf2dd_fdformat_f820.dsk
            1640,

            // mf2dd_freedos_1600s.dsk
            1600,

            // mf2dd_maxiform_1600s.dsk
            1600,

            // mf2dd_na.dsk
            1440,

            // mf2dd_nak.dsk
            1440,

            // mf2dd_n.dsk
            1440,

            // mf2dd_nk.dsk
            1440,

            // mf2dd_qcopy_1494s.dsk
            1494,

            // mf2dd_qcopy_1600s.dsk
            1600,

            // mf2dd_qcopy_1660s.dsk
            1660,

            // mf2ed_c.dsk
            5760,

            // mf2ed_ck.dsk
            5760,

            // mf2ed.dsk
            5760,

            // mf2ed_na.dsk
            5760,

            // mf2ed_nak.dsk
            5760,

            // mf2ed_n.dsk
            5760,

            // mf2ed_nk.dsk
            5760,

            // mf2hd_2m.dsk
            3608,

            // mf2hd_2m_fast.dsk
            3608,

            // mf2hd_2mgui.dsk
            15776,

            // mf2hd_2m_max.dsk
            3772,

            // mf2hd_c.dsk
            2880,

            // mf2hd_ck.dsk
            2880,

            // mf2hd_dmf.dsk
            3360,

            // mf2hd.dsk
            2880,

            // mf2hd_fdformat_168.dsk
            3360,

            // mf2hd_fdformat_172.dsk
            3444,

            // mf2hd_fdformat_f168.dsk
            3360,

            // mf2hd_fdformat_f16.dsk
            3200,

            // mf2hd_fdformat_f172.dsk
            3444,

            // mf2hd_freedos_3360s.dsk
            3360,

            // mf2hd_freedos_3486s.dsk
            3486,

            // mf2hd_maxiform_3200s.dsk
            3200,

            // mf2hd_na.dsk
            2880,

            // mf2hd_nak.dsk
            2880,

            // mf2hd_n.dsk
            2880,

            // mf2hd_nk.dsk
            2880,

            // mf2hd_qcopy_2460s.dsk
            2460,

            // mf2hd_qcopy_2720s.dsk
            2720,

            // mf2hd_qcopy_2788s.dsk
            2788,

            // mf2hd_qcopy_2880s.dsk
            2880,

            // mf2hd_qcopy_2952s.dsk
            2952,

            // mf2hd_qcopy_2988s.dsk
            2988,

            // mf2hd_qcopy_3200s.dsk
            3200,

            // mf2hd_qcopy_3320s.dsk
            3320,

            // mf2hd_qcopy_3360s.dsk
            3360,

            // mf2hd_qcopy_3486s.dsk
            3486,

            // mf2hd_xdf_c.dsk
            3680,

            // mf2hd_xdf_ck.dsk
            3680,

            // mf2hd_xdf.dsk
            3680,

            // mf2hd_xdf_na.dsk
            3680,

            // mf2hd_xdf_nak.dsk
            3680,

            // mf2hd_xdf_n.dsk
            3680,

            // mf2hd_xdf_nk.dsk
            3680
        };

        readonly uint[] _sectorSize =
        {
            // 5dd8_c.dsk
            512,

            // 5dd8_ck.dsk
            512,

            // 5dd8_na.dsk
            512,

            // 5dd8_nak.dsk
            512,

            // 5dd8_n.dsk
            512,

            // 5dd8_nk.dsk
            512,

            // 5dd_c.dsk
            512,

            // 5dd_ck.dsk
            512,

            // 5dd_na.dsk
            512,

            // 5dd_nak.dsk
            512,

            // 5dd_n.dsk
            512,

            // 5dd_nk.dsk
            512,

            // 5hd_c.dsk
            512,

            // 5hd_ck.dsk
            512,

            // 5hd_na.dsk
            512,

            // 5hd_nak.dsk
            512,

            // 5hd_n.dsk
            512,

            // 5hd_nk.dsk
            512,

            // 5sd8_c.dsk
            512,

            // 5sd8_ck.dsk
            512,

            // 5sd8_na.dsk
            512,

            // 5sd8_nak.dsk
            512,

            // 5sd8_n.dsk
            512,

            // 5sd8_nk.dsk
            512,

            // 5sd_c.dsk
            512,

            // 5sd_ck.dsk
            512,

            // 5sd_na.dsk
            512,

            // 5sd_nak.dsk
            512,

            // 5sd_n.dsk
            512,

            // 5sd_nk.dsk
            512,

            // md1dd8.dsk
            512,

            // md1dd.dsk
            512,

            // md1dd_fdformat_f200.dsk
            512,

            // md1dd_fdformat_f205.dsk
            512,

            // md2dd_2m_fast.dsk
            512,

            // md2dd_2m_max.dsk
            512,

            // md2dd8.dsk
            512,

            // md2dd.dsk
            512,

            // md2dd_fdformat_f400.dsk
            512,

            // md2dd_fdformat_f410.dsk
            512,

            // md2dd_fdformat_f720.dsk
            512,

            // md2dd_fdformat_f800.dsk
            512,

            // md2dd_fdformat_f820.dsk
            512,

            // md2dd_freedos_800s.dsk
            512,

            // md2dd_maxiform_1640s.dsk
            512,

            // md2dd_maxiform_840s.dsk
            512,

            // md2dd_qcopy_1476s.dsk
            512,

            // md2dd_qcopy_1600s.dsk
            512,

            // md2dd_qcopy_1640s.dsk
            512,

            // md2hd_2m_fast.dsk
            512,

            // md2hd_2m_max.dsk
            512,

            // md2hd.dsk
            512,

            // md2hd_fdformat_f144.dsk
            512,

            // md2hd_fdformat_f148.dsk
            512,

            // md2hd_maxiform_2788s.dsk
            512,

            // md2hd_xdf.dsk
            512,

            // mf2dd_2m.dsk
            512,

            // mf2dd_2m_fast.dsk
            512,

            // mf2dd_2mgui.dsk
            128,

            // mf2dd_2m_max.dsk
            512,

            // mf2dd_c.dsk
            512,

            // mf2dd_ck.dsk
            512,

            // mf2dd.dsk
            512,

            // mf2dd_fdformat_800.dsk
            512,

            // mf2dd_fdformat_820.dsk
            512,

            // mf2dd_fdformat_f800.dsk
            512,

            // mf2dd_fdformat_f820.dsk
            512,

            // mf2dd_freedos_1600s.dsk
            512,

            // mf2dd_maxiform_1600s.dsk
            512,

            // mf2dd_na.dsk
            512,

            // mf2dd_nak.dsk
            512,

            // mf2dd_n.dsk
            512,

            // mf2dd_nk.dsk
            512,

            // mf2dd_qcopy_1494s.dsk
            512,

            // mf2dd_qcopy_1600s.dsk
            512,

            // mf2dd_qcopy_1660s.dsk
            512,

            // mf2ed_c.dsk
            512,

            // mf2ed_ck.dsk
            512,

            // mf2ed.dsk
            512,

            // mf2ed_na.dsk
            512,

            // mf2ed_nak.dsk
            512,

            // mf2ed_n.dsk
            512,

            // mf2ed_nk.dsk
            512,

            // mf2hd_2m.dsk
            512,

            // mf2hd_2m_fast.dsk
            512,

            // mf2hd_2mgui.dsk
            128,

            // mf2hd_2m_max.dsk
            512,

            // mf2hd_c.dsk
            512,

            // mf2hd_ck.dsk
            512,

            // mf2hd_dmf.dsk
            512,

            // mf2hd.dsk
            512,

            // mf2hd_fdformat_168.dsk
            512,

            // mf2hd_fdformat_172.dsk
            512,

            // mf2hd_fdformat_f168.dsk
            512,

            // mf2hd_fdformat_f16.dsk
            512,

            // mf2hd_fdformat_f172.dsk
            512,

            // mf2hd_freedos_3360s.dsk
            512,

            // mf2hd_freedos_3486s.dsk
            512,

            // mf2hd_maxiform_3200s.dsk
            512,

            // mf2hd_na.dsk
            512,

            // mf2hd_nak.dsk
            512,

            // mf2hd_n.dsk
            512,

            // mf2hd_nk.dsk
            512,

            // mf2hd_qcopy_2460s.dsk
            512,

            // mf2hd_qcopy_2720s.dsk
            512,

            // mf2hd_qcopy_2788s.dsk
            512,

            // mf2hd_qcopy_2880s.dsk
            512,

            // mf2hd_qcopy_2952s.dsk
            512,

            // mf2hd_qcopy_2988s.dsk
            512,

            // mf2hd_qcopy_3200s.dsk
            512,

            // mf2hd_qcopy_3320s.dsk
            512,

            // mf2hd_qcopy_3360s.dsk
            512,

            // mf2hd_qcopy_3486s.dsk
            512,

            // mf2hd_xdf_c.dsk
            512,

            // mf2hd_xdf_ck.dsk
            512,

            // mf2hd_xdf.dsk
            512,

            // mf2hd_xdf_na.dsk
            512,

            // mf2hd_xdf_nak.dsk
            512,

            // mf2hd_xdf_n.dsk
            512,

            // mf2hd_xdf_nk.dsk
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // 5dd8_c.dsk
            MediaType.DOS_525_DS_DD_8,

            // 5dd8_ck.dsk
            MediaType.DOS_525_DS_DD_8,

            // 5dd8_na.dsk
            MediaType.DOS_525_DS_DD_8,

            // 5dd8_nak.dsk
            MediaType.DOS_525_DS_DD_8,

            // 5dd8_n.dsk
            MediaType.DOS_525_DS_DD_8,

            // 5dd8_nk.dsk
            MediaType.DOS_525_DS_DD_8,

            // 5dd_c.dsk
            MediaType.DOS_525_DS_DD_9,

            // 5dd_ck.dsk
            MediaType.DOS_525_DS_DD_9,

            // 5dd_na.dsk
            MediaType.DOS_525_DS_DD_9,

            // 5dd_nak.dsk
            MediaType.DOS_525_DS_DD_9,

            // 5dd_n.dsk
            MediaType.DOS_525_DS_DD_9,

            // 5dd_nk.dsk
            MediaType.DOS_525_DS_DD_9,

            // 5hd_c.dsk
            MediaType.DOS_525_HD,

            // 5hd_ck.dsk
            MediaType.DOS_525_HD,

            // 5hd_na.dsk
            MediaType.DOS_525_HD,

            // 5hd_nak.dsk
            MediaType.DOS_525_HD,

            // 5hd_n.dsk
            MediaType.DOS_525_HD,

            // 5hd_nk.dsk
            MediaType.DOS_525_HD,

            // 5sd8_c.dsk
            MediaType.DOS_525_SS_DD_8,

            // 5sd8_ck.dsk
            MediaType.DOS_525_SS_DD_8,

            // 5sd8_na.dsk
            MediaType.DOS_525_SS_DD_8,

            // 5sd8_nak.dsk
            MediaType.DOS_525_SS_DD_8,

            // 5sd8_n.dsk
            MediaType.DOS_525_SS_DD_8,

            // 5sd8_nk.dsk
            MediaType.DOS_525_SS_DD_8,

            // 5sd_c.dsk
            MediaType.DOS_525_SS_DD_9,

            // 5sd_ck.dsk
            MediaType.DOS_525_SS_DD_9,

            // 5sd_na.dsk
            MediaType.DOS_525_SS_DD_9,

            // 5sd_nak.dsk
            MediaType.DOS_525_SS_DD_9,

            // 5sd_n.dsk
            MediaType.DOS_525_SS_DD_9,

            // 5sd_nk.dsk
            MediaType.DOS_525_SS_DD_9,

            // md1dd8.dsk
            MediaType.DOS_525_SS_DD_8,

            // md1dd.dsk
            MediaType.DOS_525_SS_DD_9,

            // md1dd_fdformat_f200.dsk
            MediaType.Unknown,

            // md1dd_fdformat_f205.dsk
            MediaType.Unknown,

            // md2dd_2m_fast.dsk
            MediaType.FDFORMAT_35_DD,

            // md2dd_2m_max.dsk
            MediaType.Unknown,

            // md2dd8.dsk
            MediaType.DOS_525_DS_DD_8,

            // md2dd.dsk
            MediaType.DOS_525_DS_DD_9,

            // md2dd_fdformat_f400.dsk
            MediaType.Unknown,

            // md2dd_fdformat_f410.dsk
            MediaType.Unknown,

            // md2dd_fdformat_f720.dsk
            MediaType.DOS_35_DS_DD_9,

            // md2dd_fdformat_f800.dsk
            MediaType.CBM_35_DD,

            // md2dd_fdformat_f820.dsk
            MediaType.FDFORMAT_35_DD,

            // md2dd_freedos_800s.dsk
            MediaType.Unknown,

            // md2dd_maxiform_1640s.dsk
            MediaType.FDFORMAT_35_DD,

            // md2dd_maxiform_840s.dsk
            MediaType.Unknown,

            // md2dd_qcopy_1476s.dsk
            MediaType.Unknown,

            // md2dd_qcopy_1600s.dsk
            MediaType.CBM_35_DD,

            // md2dd_qcopy_1640s.dsk
            MediaType.FDFORMAT_35_DD,

            // md2hd_2m_fast.dsk
            MediaType.Unknown,

            // md2hd_2m_max.dsk
            MediaType.Unknown,

            // md2hd.dsk
            MediaType.DOS_525_HD,

            // md2hd_fdformat_f144.dsk
            MediaType.DOS_35_HD,

            // md2hd_fdformat_f148.dsk
            MediaType.Unknown,

            // md2hd_maxiform_2788s.dsk
            MediaType.FDFORMAT_525_HD,

            // md2hd_xdf.dsk
            MediaType.XDF_525,

            // mf2dd_2m.dsk
            MediaType.Unknown,

            // mf2dd_2m_fast.dsk
            MediaType.Unknown,

            // mf2dd_2mgui.dsk
            MediaType.Unknown,

            // mf2dd_2m_max.dsk
            MediaType.Unknown,

            // mf2dd_c.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_ck.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_fdformat_800.dsk
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_820.dsk
            MediaType.FDFORMAT_35_DD,

            // mf2dd_fdformat_f800.dsk
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_f820.dsk
            MediaType.FDFORMAT_35_DD,

            // mf2dd_freedos_1600s.dsk
            MediaType.CBM_35_DD,

            // mf2dd_maxiform_1600s.dsk
            MediaType.CBM_35_DD,

            // mf2dd_na.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_nak.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_n.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_nk.dsk
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_qcopy_1494s.dsk
            MediaType.Unknown,

            // mf2dd_qcopy_1600s.dsk
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1660s.dsk
            MediaType.Unknown,

            // mf2ed_c.dsk
            MediaType.ECMA_147,

            // mf2ed_ck.dsk
            MediaType.ECMA_147,

            // mf2ed.dsk
            MediaType.ECMA_147,

            // mf2ed_na.dsk
            MediaType.ECMA_147,

            // mf2ed_nak.dsk
            MediaType.ECMA_147,

            // mf2ed_n.dsk
            MediaType.ECMA_147,

            // mf2ed_nk.dsk
            MediaType.ECMA_147,

            // mf2hd_2m.dsk
            MediaType.Unknown,

            // mf2hd_2m_fast.dsk
            MediaType.Unknown,

            // mf2hd_2mgui.dsk
            MediaType.Unknown,

            // mf2hd_2m_max.dsk
            MediaType.Unknown,

            // mf2hd_c.dsk
            MediaType.DOS_35_HD,

            // mf2hd_ck.dsk
            MediaType.DOS_35_HD,

            // mf2hd_dmf.dsk
            MediaType.DMF,

            // mf2hd.dsk
            MediaType.DOS_35_HD,

            // mf2hd_fdformat_168.dsk
            MediaType.DMF,

            // mf2hd_fdformat_172.dsk
            MediaType.FDFORMAT_35_HD,

            // mf2hd_fdformat_f168.dsk
            MediaType.DMF,

            // mf2hd_fdformat_f16.dsk
            MediaType.Unknown,

            // mf2hd_fdformat_f172.dsk
            MediaType.FDFORMAT_35_HD,

            // mf2hd_freedos_3360s.dsk
            MediaType.DMF,

            // mf2hd_freedos_3486s.dsk
            MediaType.Unknown,

            // mf2hd_maxiform_3200s.dsk
            MediaType.Unknown,

            // mf2hd_na.dsk
            MediaType.DOS_35_HD,

            // mf2hd_nak.dsk
            MediaType.DOS_35_HD,

            // mf2hd_n.dsk
            MediaType.DOS_35_HD,

            // mf2hd_nk.dsk
            MediaType.DOS_35_HD,

            // mf2hd_qcopy_2460s.dsk
            MediaType.Unknown,

            // mf2hd_qcopy_2720s.dsk
            MediaType.Unknown,

            // mf2hd_qcopy_2788s.dsk
            MediaType.FDFORMAT_525_HD,

            // mf2hd_qcopy_2880s.dsk
            MediaType.DOS_35_HD,

            // mf2hd_qcopy_2952s.dsk
            MediaType.Unknown,

            // mf2hd_qcopy_2988s.dsk
            MediaType.Unknown,

            // mf2hd_qcopy_3200s.dsk
            MediaType.Unknown,

            // mf2hd_qcopy_3320s.dsk
            MediaType.Unknown,

            // mf2hd_qcopy_3360s.dsk
            MediaType.DMF,

            // mf2hd_qcopy_3486s.dsk
            MediaType.Unknown,

            // mf2hd_xdf_c.dsk
            MediaType.XDF_35,

            // mf2hd_xdf_ck.dsk
            MediaType.XDF_35,

            // mf2hd_xdf.dsk
            MediaType.XDF_35,

            // mf2hd_xdf_na.dsk
            MediaType.XDF_35,

            // mf2hd_xdf_nak.dsk
            MediaType.XDF_35,

            // mf2hd_xdf_n.dsk
            MediaType.XDF_35,

            // mf2hd_xdf_nk.dsk
            MediaType.XDF_35
        };

        readonly string[] _md5S =
        {
            // 5dd8_c.dsk
            "5a1e0a75d31d88c1ce7429fd333c268f",

            // 5dd8_ck.dsk
            "5a1e0a75d31d88c1ce7429fd333c268f",

            // 5dd8_na.dsk
            "4989762c82f173f9b52e0bdb8cf5becb",

            // 5dd8_nak.dsk
            "4989762c82f173f9b52e0bdb8cf5becb",

            // 5dd8_n.dsk
            "5a1e0a75d31d88c1ce7429fd333c268f",

            // 5dd8_nk.dsk
            "5a1e0a75d31d88c1ce7429fd333c268f",

            // 5dd_c.dsk
            "c1a67b27bc76b64d0845965501b24120",

            // 5dd_ck.dsk
            "c1a67b27bc76b64d0845965501b24120",

            // 5dd_na.dsk
            "8a4d35dd0d97e6bca8b000170a43a56f",

            // 5dd_nak.dsk
            "8a4d35dd0d97e6bca8b000170a43a56f",

            // 5dd_n.dsk
            "c1a67b27bc76b64d0845965501b24120",

            // 5dd_nk.dsk
            "c1a67b27bc76b64d0845965501b24120",

            // 5hd_c.dsk
            "1c28b4c3cdc1dbf19c24a5eca3891a87",

            // 5hd_ck.dsk
            "1c28b4c3cdc1dbf19c24a5eca3891a87",

            // 5hd_na.dsk
            "2ce745ac23712d3eb03d7a11ba933b12",

            // 5hd_nak.dsk
            "2ce745ac23712d3eb03d7a11ba933b12",

            // 5hd_n.dsk
            "1c28b4c3cdc1dbf19c24a5eca3891a87",

            // 5hd_nk.dsk
            "1c28b4c3cdc1dbf19c24a5eca3891a87",

            // 5sd8_c.dsk
            "65ce0cd08d90c882df12637c9c72c1ba",

            // 5sd8_ck.dsk
            "65ce0cd08d90c882df12637c9c72c1ba",

            // 5sd8_na.dsk
            "6f5d09c13a7b481bad9ea78042e61e00",

            // 5sd8_nak.dsk
            "6f5d09c13a7b481bad9ea78042e61e00",

            // 5sd8_n.dsk
            "65ce0cd08d90c882df12637c9c72c1ba",

            // 5sd8_nk.dsk
            "65ce0cd08d90c882df12637c9c72c1ba",

            // 5sd_c.dsk
            "412fdc582506c0d7e76735d403b30759",

            // 5sd_ck.dsk
            "412fdc582506c0d7e76735d403b30759",

            // 5sd_na.dsk
            "fd81fceb26bda5b02053c5c729a6f67f",

            // 5sd_nak.dsk
            "fd81fceb26bda5b02053c5c729a6f67f",

            // 5sd_n.dsk
            "412fdc582506c0d7e76735d403b30759",

            // 5sd_nk.dsk
            "412fdc582506c0d7e76735d403b30759",

            // md1dd8.dsk
            "d81f5cb64fd0b99f138eab34110bbc3c",

            // md1dd.dsk
            "a89006a75d13bee9202d1d6e52721ccb",

            // md1dd_fdformat_f200.dsk
            "e1ad4a022778d7a0b24a93d8e68a59dc",

            // md1dd_fdformat_f205.dsk
            "353f3c2125ab6f74e3a271b60ad34840",

            // md2dd_2m_fast.dsk
            "319fa8bef964c2a63e34bdb48e77cc4e",

            // md2dd_2m_max.dsk
            "306a61469b4c3c83f3e5f9ae409d83cd",

            // md2dd8.dsk
            "beef1cdb004dc69391d6b3d508988b95",

            // md2dd.dsk
            "6213897b7dbf263f12abf76901d43862",

            // md2dd_fdformat_f400.dsk
            "0aef12c906b744101b932d799ca88a78",

            // md2dd_fdformat_f410.dsk
            "348d12add1ed226cd712a4a6a10d1a34",

            // md2dd_fdformat_f720.dsk
            "1c36b819cfe355c11360bc120c9216fe",

            // md2dd_fdformat_f800.dsk
            "25114403c11e337480e2afc4e6e32108",

            // md2dd_fdformat_f820.dsk
            "3d7760ddaa55cd258057773d15106b78",

            // md2dd_freedos_800s.dsk
            "29054ef703394ee3b35e849468a412ba",

            // md2dd_maxiform_1640s.dsk
            "c91e852828c2aeee2fc94a6adbeed0ae",

            // md2dd_maxiform_840s.dsk
            "efb6cfe53a6770f0ae388cb2c7f46264",

            // md2dd_qcopy_1476s.dsk
            "6116f7c1397cadd55ba8d79c2aadc9dd",

            // md2dd_qcopy_1600s.dsk
            "93100f8d86e5d0d0e6340f59c52a5e0d",

            // md2dd_qcopy_1640s.dsk
            "cf7b7d43aa70863bedcc4a8432a5af67",

            // md2hd_2m_fast.dsk
            "215198cf2a336e718208fc207bb62c6d",

            // md2hd_2m_max.dsk
            "2c96964b5d91444302e21721c25ea120",

            // md2hd.dsk
            "02259cd5fbcc20f8484aa6bece7a37c6",

            // md2hd_fdformat_f144.dsk
            "073a172879a71339ef4b00ebb47b67fc",

            // md2hd_fdformat_f148.dsk
            "d9890897130d0fc1eee3dbf4d9b0440f",

            // md2hd_maxiform_2788s.dsk
            "09ca721aa883d5bbaa422c7943b0782c",

            // md2hd_xdf.dsk
            "d78dc81491edeec99aa202d02f3daf00",

            // mf2dd_2m.dsk
            "9a8670fbaf6307b8d5f32aa10e1be435",

            // mf2dd_2m_fast.dsk
            "05d29642cdcddafa0dcaff91682f8fe0",

            // mf2dd_2mgui.dsk
            "beb782f6bc970e32ceef79cd112e2e48",

            // mf2dd_2m_max.dsk
            "a99603cd3219aab1299e66b2999f0e57",

            // mf2dd_c.dsk
            "2aefc1e97f29bf9982e0fd7091dfb9f5",

            // mf2dd_ck.dsk
            "2aefc1e97f29bf9982e0fd7091dfb9f5",

            // mf2dd.dsk
            "9827ba1b3e9cac41263caabd862e78f9",

            // mf2dd_fdformat_800.dsk
            "2e69bbd591ab736e471834ae03dde9a6",

            // mf2dd_fdformat_820.dsk
            "81d3bfec7b201f6a4503eb24c4394d4a",

            // mf2dd_fdformat_f800.dsk
            "26532a62985b51a2c3b877a57f6d257b",

            // mf2dd_fdformat_f820.dsk
            "a7771acff766557cc23b8c6943b588f9",

            // mf2dd_freedos_1600s.dsk
            "d07f7ffaee89742c6477aaaf94eb5715",

            // mf2dd_maxiform_1600s.dsk
            "56af87802a9852e6e01e08d544740816",

            // mf2dd_na.dsk
            "e574be0d057f2ef775dfb685561d27cf",

            // mf2dd_nak.dsk
            "e574be0d057f2ef775dfb685561d27cf",

            // mf2dd_n.dsk
            "2aefc1e97f29bf9982e0fd7091dfb9f5",

            // mf2dd_nk.dsk
            "2aefc1e97f29bf9982e0fd7091dfb9f5",

            // mf2dd_qcopy_1494s.dsk
            "fd7fb1ba11cdfe11db54af0322abf59d",

            // mf2dd_qcopy_1600s.dsk
            "d9db52d992a76bf3bbc626ff844215a5",

            // mf2dd_qcopy_1660s.dsk
            "5949d0be57ce8bffcda7c4be4d1348ee",

            // mf2ed_c.dsk
            "e4746aa9629a2325c520db1c8a641ac6",

            // mf2ed_ck.dsk
            "e4746aa9629a2325c520db1c8a641ac6",

            // mf2ed.dsk
            "4aeafaf2a088d6a7406856dce8118567",

            // mf2ed_na.dsk
            "42e73287b23ac985c9825466cae26859",

            // mf2ed_nak.dsk
            "42e73287b23ac985c9825466cae26859",

            // mf2ed_n.dsk
            "e4746aa9629a2325c520db1c8a641ac6",

            // mf2ed_nk.dsk
            "e4746aa9629a2325c520db1c8a641ac6",

            // mf2hd_2m.dsk
            "2f6964d410b275c8e9f60fe2f24b361a",

            // mf2hd_2m_fast.dsk
            "967726aede85c68f66887672078f8856",

            // mf2hd_2mgui.dsk
            "786e45bbfcb369913968aa31365f00bb",

            // mf2hd_2m_max.dsk
            "3fa4f87d7058ba940b88e0d80f0d7ded",

            // mf2hd_c.dsk
            "003e9130d83a23018f488f9fa89cae5e",

            // mf2hd_ck.dsk
            "003e9130d83a23018f488f9fa89cae5e",

            // mf2hd_dmf.dsk
            "b042310181410227d0072fef1e98a989",

            // mf2hd.dsk
            "00e61c06bf29f0c04a7eabe2dbd7efb6",

            // mf2hd_fdformat_168.dsk
            "1e06f21a1c11ea3347212da115bca08f",

            // mf2hd_fdformat_172.dsk
            "3fc3a03d049416d81f81cc3b9ea8e5de",

            // mf2hd_fdformat_f168.dsk
            "7e3bf04f3660dd1052a335dc99441e44",

            // mf2hd_fdformat_f16.dsk
            "8eb8cb310feaf03c69fffd4f6e729847",

            // mf2hd_fdformat_f172.dsk
            "a58fd062f024b95714f1223a8bc2232f",

            // mf2hd_freedos_3360s.dsk
            "2bfd2e0a81bad704f8fc7758358cfcca",

            // mf2hd_freedos_3486s.dsk
            "a79ec33c623697b4562dacaed31523b8",

            // mf2hd_maxiform_3200s.dsk
            "3c4becd695ed25866d39966a9a93c2d9",

            // mf2hd_na.dsk
            "009cc68e28b2b13814d3afbec9d9e59f",

            // mf2hd_nak.dsk
            "009cc68e28b2b13814d3afbec9d9e59f",

            // mf2hd_n.dsk
            "003e9130d83a23018f488f9fa89cae5e",

            // mf2hd_nk.dsk
            "003e9130d83a23018f488f9fa89cae5e",

            // mf2hd_qcopy_2460s.dsk
            "72282e11f7d91bf9c090b550fabfe80d",

            // mf2hd_qcopy_2720s.dsk
            "457c1126dc7f36bbbabe9e17e90372e3",

            // mf2hd_qcopy_2788s.dsk
            "852181d5913c6f290872c66bbe992314",

            // mf2hd_qcopy_2880s.dsk
            "2980cc32504c945598dc50f1db576994",

            // mf2hd_qcopy_2952s.dsk
            "c1c58d74fffb3656dd7f60f74ae8a629",

            // mf2hd_qcopy_2988s.dsk
            "097bb2fd34cee5ebde7b5641975ffd60",

            // mf2hd_qcopy_3200s.dsk
            "e45d41a61fbe48f328c995fcc10a5548",

            // mf2hd_qcopy_3320s.dsk
            "c25f2a57c71db1cd4fea2263598f544a",

            // mf2hd_qcopy_3360s.dsk
            "15f71b92bd72aba5d80bf70eca4d5b1e",

            // mf2hd_qcopy_3486s.dsk
            "d88c8d818e238c9e52b8588b5fd52efe",

            // mf2hd_xdf_c.dsk
            "2770e5b1b7935ca6e9695a32008b936a",

            // mf2hd_xdf_ck.dsk
            "2770e5b1b7935ca6e9695a32008b936a",

            // mf2hd_xdf.dsk
            "3d5fcdaf627257ae9f50a06bdba26965",

            // mf2hd_xdf_na.dsk
            "34b4bdab5fcc17076cceb7c1a39ea430",

            // mf2hd_xdf_nak.dsk
            "34b4bdab5fcc17076cceb7c1a39ea430",

            // mf2hd_xdf_n.dsk
            "2770e5b1b7935ca6e9695a32008b936a",

            // mf2hd_xdf_nk.dsk
            "2770e5b1b7935ca6e9695a32008b936a"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "SaveDskF");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.SaveDskF();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                            Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                            Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");
                        });
                    }
                }
            });
        }

        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var   image       = new DiscImages.SaveDskF();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    var ctx = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            sector      =  image.ReadSectors(doneSectors, SECTORS_TO_READ);
                            doneSectors += SECTORS_TO_READ;
                        }
                        else
                        {
                            sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                            doneSectors += image.Info.Sectors - doneSectors;
                        }

                        ctx.Update(sector);
                    }

                    Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                }
            });
        }
    }
}