// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DriDiskCopy.cs
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
    public class DriDiskCopy
    {
        readonly string[] _testFiles =
        {
            "DSKA0000.IMG.lz", "DSKA0001.IMG.lz", "DSKA0009.IMG.lz", "DSKA0010.IMG.lz", "DSKA0024.IMG.lz",
            "DSKA0025.IMG.lz", "DSKA0030.IMG.lz", "DSKA0035.IMG.lz", "DSKA0036.IMG.lz", "DSKA0037.IMG.lz",
            "DSKA0038.IMG.lz", "DSKA0039.IMG.lz", "DSKA0040.IMG.lz", "DSKA0041.IMG.lz", "DSKA0042.IMG.lz",
            "DSKA0043.IMG.lz", "DSKA0044.IMG.lz", "DSKA0045.IMG.lz", "DSKA0046.IMG.lz", "DSKA0047.IMG.lz",
            "DSKA0048.IMG.lz", "DSKA0049.IMG.lz", "DSKA0050.IMG.lz", "DSKA0051.IMG.lz", "DSKA0052.IMG.lz",
            "DSKA0053.IMG.lz", "DSKA0054.IMG.lz", "DSKA0055.IMG.lz", "DSKA0056.IMG.lz", "DSKA0057.IMG.lz",
            "DSKA0058.IMG.lz", "DSKA0059.IMG.lz", "DSKA0060.IMG.lz", "DSKA0069.IMG.lz", "DSKA0073.IMG.lz",
            "DSKA0074.IMG.lz", "DSKA0075.IMG.lz", "DSKA0076.IMG.lz", "DSKA0077.IMG.lz", "DSKA0078.IMG.lz",
            "DSKA0080.IMG.lz", "DSKA0081.IMG.lz", "DSKA0082.IMG.lz", "DSKA0083.IMG.lz", "DSKA0084.IMG.lz",
            "DSKA0085.IMG.lz", "DSKA0089.IMG.lz", "DSKA0090.IMG.lz", "DSKA0091.IMG.lz", "DSKA0092.IMG.lz",
            "DSKA0093.IMG.lz", "DSKA0094.IMG.lz", "DSKA0097.IMG.lz", "DSKA0098.IMG.lz", "DSKA0099.IMG.lz",
            "DSKA0101.IMG.lz", "DSKA0103.IMG.lz", "DSKA0105.IMG.lz", "DSKA0106.IMG.lz", "DSKA0107.IMG.lz",
            "DSKA0108.IMG.lz", "DSKA0109.IMG.lz", "DSKA0110.IMG.lz", "DSKA0111.IMG.lz", "DSKA0112.IMG.lz",
            "DSKA0113.IMG.lz", "DSKA0114.IMG.lz", "DSKA0115.IMG.lz", "DSKA0116.IMG.lz", "DSKA0117.IMG.lz",
            "DSKA0120.IMG.lz", "DSKA0121.IMG.lz", "DSKA0122.IMG.lz", "DSKA0123.IMG.lz", "DSKA0124.IMG.lz",
            "DSKA0125.IMG.lz", "DSKA0126.IMG.lz", "DSKA0163.IMG.lz", "DSKA0164.IMG.lz", "DSKA0166.IMG.lz",
            "DSKA0168.IMG.lz", "DSKA0169.IMG.lz", "DSKA0173.IMG.lz", "DSKA0174.IMG.lz", "DSKA0175.IMG.lz",
            "DSKA0180.IMG.lz", "DSKA0181.IMG.lz", "DSKA0182.IMG.lz", "DSKA0183.IMG.lz", "DSKA0262.IMG.lz",
            "DSKA0263.IMG.lz", "DSKA0264.IMG.lz", "DSKA0265.IMG.lz", "DSKA0266.IMG.lz", "DSKA0267.IMG.lz",
            "DSKA0268.IMG.lz", "DSKA0269.IMG.lz", "DSKA0270.IMG.lz", "DSKA0271.IMG.lz", "DSKA0272.IMG.lz",
            "DSKA0273.IMG.lz", "DSKA0280.IMG.lz", "DSKA0281.IMG.lz", "DSKA0282.IMG.lz", "DSKA0283.IMG.lz",
            "DSKA0284.IMG.lz", "DSKA0285.IMG.lz", "DSKA0287.IMG.lz", "DSKA0288.IMG.lz", "DSKA0289.IMG.lz",
            "DSKA0290.IMG.lz", "DSKA0291.IMG.lz", "DSKA0299.IMG.lz", "DSKA0300.IMG.lz", "DSKA0301.IMG.lz",
            "DSKA0302.IMG.lz", "DSKA0303.IMG.lz", "DSKA0304.IMG.lz", "DSKA0305.IMG.lz", "DSKA0308.IMG.lz",
            "DSKA0311.IMG.lz", "DSKA0314.IMG.lz", "DSKA0316.IMG.lz", "DSKA0317.IMG.lz", "DSKA0318.IMG.lz",
            "DSKA0319.IMG.lz", "DSKA0320.IMG.lz", "DSKA0322.IMG.lz", "md1dd8.img.lz", "md1dd.img.lz",
            "md2dd_2m_fast.img.lz", "md2dd_2m_max.img.lz", "md2dd8.img.lz", "md2dd_freedos_800s.img.lz", "md2dd.img.lz",
            "md2dd_maxiform_1640s.img.lz", "md2dd_maxiform_840s.img.lz", "md2dd_qcopy_1476s.img.lz",
            "md2dd_qcopy_1600s.img.lz", "md2dd_qcopy_1640s.img.lz", "md2hd_2m_fast.img.lz", "md2hd_2m_max.img.lz",
            "md2hd.img.lz", "md2hd_maxiform_2788s.img.lz", "md2hd_nec.img.lz", "md2hd_xdf.img.lz", "mf2dd_2m.dsk.lz",
            "mf2dd_2m_fast.img.lz", "mf2dd_2mgui.dsk.lz", "mf2dd_2m_max.dsk.lz", "mf2dd_2m_max.img.lz", "mf2dd.dsk.lz",
            "mf2dd_fdformat_800.dsk.lz", "mf2dd_fdformat_820.dsk.lz", "mf2dd_freedos_1600s.img.lz",
            "mf2dd_freedos.dsk.lz", "mf2dd.img.lz", "mf2dd_maxiform_1600s.img.lz", "mf2dd_qcopy_1494s.img.lz",
            "mf2dd_qcopy_1600s.img.lz", "mf2dd_qcopy_1660s.img.lz", "mf2ed.img.lz", "mf2hd_2m.dsk.lz",
            "mf2hd_2m_fast.img.lz", "mf2hd_2mgui.dsk.lz", "mf2hd_2m_max.dsk.lz", "mf2hd_2m_max.img.lz",
            "mf2hd_dmf.img.lz", "mf2hd.dsk.lz", "mf2hd_fdformat_168.dsk.lz", "mf2hd_fdformat_172.dsk.lz",
            "mf2hd_freedos_3360s.img.lz", "mf2hd_freedos_3486s.img.lz", "mf2hd.img.lz", "mf2hd_maxiform_3200s.img.lz",
            "mf2hd_nec.img.lz", "mf2hd_qcopy_2460s.img.lz", "mf2hd_qcopy_2720s.img.lz", "mf2hd_qcopy_2788s.img.lz",
            "mf2hd_qcopy_2880s.img.lz", "mf2hd_qcopy_2952s.img.lz", "mf2hd_qcopy_2988s.img.lz",
            "mf2hd_qcopy_3200s.img.lz", "mf2hd_qcopy_3320s.img.lz", "mf2hd_qcopy_3360s.img.lz",
            "mf2hd_qcopy_3486s.img.lz", "mf2hd_xdf.dsk.lz", "mf2hd_xdf.img.lz"
        };

        readonly ulong[] _sectors =
        {
            // DSKA0000.IMG.lz
            2880,

            // DSKA0001.IMG.lz
            1600,

            // DSKA0009.IMG.lz
            2880,

            // DSKA0010.IMG.lz
            1440,

            // DSKA0024.IMG.lz
            2880,

            // DSKA0025.IMG.lz
            640,

            // DSKA0030.IMG.lz
            1440,

            // DSKA0035.IMG.lz
            320,

            // DSKA0036.IMG.lz
            320,

            // DSKA0037.IMG.lz
            360,

            // DSKA0038.IMG.lz
            360,

            // DSKA0039.IMG.lz
            640,

            // DSKA0040.IMG.lz
            640,

            // DSKA0041.IMG.lz
            640,

            // DSKA0042.IMG.lz
            640,

            // DSKA0043.IMG.lz
            720,

            // DSKA0044.IMG.lz
            720,

            // DSKA0045.IMG.lz
            2400,

            // DSKA0046.IMG.lz
            2460,

            // DSKA0047.IMG.lz
            1280,

            // DSKA0048.IMG.lz
            1440,

            // DSKA0049.IMG.lz
            1476,

            // DSKA0050.IMG.lz
            1600,

            // DSKA0051.IMG.lz
            1640,

            // DSKA0052.IMG.lz
            2880,

            // DSKA0053.IMG.lz
            2952,

            // DSKA0054.IMG.lz
            3200,

            // DSKA0055.IMG.lz
            3280,

            // DSKA0056.IMG.lz
            3360,

            // DSKA0057.IMG.lz
            3444,

            // DSKA0058.IMG.lz
            3486,

            // DSKA0059.IMG.lz
            3528,

            // DSKA0060.IMG.lz
            3570,

            // DSKA0069.IMG.lz
            1440,

            // DSKA0073.IMG.lz
            320,

            // DSKA0074.IMG.lz
            360,

            // DSKA0075.IMG.lz
            640,

            // DSKA0076.IMG.lz
            720,

            // DSKA0077.IMG.lz
            800,

            // DSKA0078.IMG.lz
            2400,

            // DSKA0080.IMG.lz
            1440,

            // DSKA0081.IMG.lz
            1600,

            // DSKA0082.IMG.lz
            2880,

            // DSKA0083.IMG.lz
            2988,

            // DSKA0084.IMG.lz
            3360,

            // DSKA0085.IMG.lz
            3486,

            // DSKA0089.IMG.lz
            3040,

            // DSKA0090.IMG.lz
            3680,

            // DSKA0091.IMG.lz
            1640,

            // DSKA0092.IMG.lz
            1804,

            // DSKA0093.IMG.lz
            2952,

            // DSKA0094.IMG.lz
            3116,

            // DSKA0097.IMG.lz
            3608,

            // DSKA0098.IMG.lz
            3772,

            // DSKA0099.IMG.lz
            1952,

            // DSKA0101.IMG.lz
            3280,

            // DSKA0103.IMG.lz
            3944,

            // DSKA0105.IMG.lz
            400,

            // DSKA0106.IMG.lz
            410,

            // DSKA0107.IMG.lz
            800,

            // DSKA0108.IMG.lz
            820,

            // DSKA0109.IMG.lz
            1600,

            // DSKA0110.IMG.lz
            1640,

            // DSKA0111.IMG.lz
            2880,

            // DSKA0112.IMG.lz
            2952,

            // DSKA0113.IMG.lz
            1600,

            // DSKA0114.IMG.lz
            1640,

            // DSKA0115.IMG.lz
            2952,

            // DSKA0116.IMG.lz
            3200,

            // DSKA0117.IMG.lz
            3280,

            // DSKA0120.IMG.lz
            320,

            // DSKA0121.IMG.lz
            360,

            // DSKA0122.IMG.lz
            640,

            // DSKA0123.IMG.lz
            720,

            // DSKA0124.IMG.lz
            2400,

            // DSKA0125.IMG.lz
            1440,

            // DSKA0126.IMG.lz
            2880,

            // DSKA0163.IMG.lz
            720,

            // DSKA0164.IMG.lz
            820,

            // DSKA0166.IMG.lz
            1440,

            // DSKA0168.IMG.lz
            2400,

            // DSKA0169.IMG.lz
            2880,

            // DSKA0173.IMG.lz
            720,

            // DSKA0174.IMG.lz
            1440,

            // DSKA0175.IMG.lz
            1600,

            // DSKA0180.IMG.lz
            3200,

            // DSKA0181.IMG.lz
            3360,

            // DSKA0182.IMG.lz
            3444,

            // DSKA0183.IMG.lz
            3486,

            // DSKA0262.IMG.lz
            1440,

            // DSKA0263.IMG.lz
            1600,

            // DSKA0264.IMG.lz
            1640,

            // DSKA0265.IMG.lz
            1660,

            // DSKA0266.IMG.lz
            2880,

            // DSKA0267.IMG.lz
            3040,

            // DSKA0268.IMG.lz
            3200,

            // DSKA0269.IMG.lz
            3280,

            // DSKA0270.IMG.lz
            3320,

            // DSKA0271.IMG.lz
            3360,

            // DSKA0272.IMG.lz
            3444,

            // DSKA0273.IMG.lz
            3486,

            // DSKA0280.IMG.lz
            360,

            // DSKA0281.IMG.lz
            400,

            // DSKA0282.IMG.lz
            640,

            // DSKA0283.IMG.lz
            720,

            // DSKA0284.IMG.lz
            800,

            // DSKA0285.IMG.lz
            840,

            // DSKA0287.IMG.lz
            1440,

            // DSKA0288.IMG.lz
            1494,

            // DSKA0289.IMG.lz
            1600,

            // DSKA0290.IMG.lz
            1640,

            // DSKA0291.IMG.lz
            1660,

            // DSKA0299.IMG.lz
            320,

            // DSKA0300.IMG.lz
            360,

            // DSKA0301.IMG.lz
            640,

            // DSKA0302.IMG.lz
            720,

            // DSKA0303.IMG.lz
            2400,

            // DSKA0304.IMG.lz
            1440,

            // DSKA0305.IMG.lz
            2880,

            // DSKA0308.IMG.lz
            1600,

            // DSKA0311.IMG.lz
            3444,

            // DSKA0314.IMG.lz
            1440,

            // DSKA0316.IMG.lz
            2880,

            // DSKA0317.IMG.lz
            3360,

            // DSKA0318.IMG.lz
            3444,

            // DSKA0319.IMG.lz
            3360,

            // DSKA0320.IMG.lz
            3360,

            // DSKA0322.IMG.lz
            1386,

            // md1dd8.img.lz
            320,

            // md1dd.img.lz
            360,

            // md2dd_2m_fast.img.lz
            1640,

            // md2dd_2m_max.img.lz
            1804,

            // md2dd8.img.lz
            640,

            // md2dd_freedos_800s.img.lz
            800,

            // md2dd.img.lz
            720,

            // md2dd_maxiform_1640s.img.lz
            1640,

            // md2dd_maxiform_840s.img.lz
            840,

            // md2dd_qcopy_1476s.img.lz
            1476,

            // md2dd_qcopy_1600s.img.lz
            1600,

            // md2dd_qcopy_1640s.img.lz
            1640,

            // md2hd_2m_fast.img.lz
            2952,

            // md2hd_2m_max.img.lz
            3116,

            // md2hd.img.lz
            2400,

            // md2hd_maxiform_2788s.img.lz
            2788,

            // md2hd_nec.img.lz
            2464,

            // md2hd_xdf.img.lz
            3040,

            // mf2dd_2m.dsk.lz
            1968,

            // mf2dd_2m_fast.img.lz
            1968,

            // mf2dd_2mgui.dsk.lz
            9408,

            // mf2dd_2m_max.dsk.lz
            2132,

            // mf2dd_2m_max.img.lz
            2132,

            // mf2dd.dsk.lz
            1440,

            // mf2dd_fdformat_800.dsk.lz
            1600,

            // mf2dd_fdformat_820.dsk.lz
            1640,

            // mf2dd_freedos_1600s.img.lz
            1600,

            // mf2dd_freedos.dsk.lz
            1600,

            // mf2dd.img.lz
            1440,

            // mf2dd_maxiform_1600s.img.lz
            1600,

            // mf2dd_qcopy_1494s.img.lz
            1494,

            // mf2dd_qcopy_1600s.img.lz
            1600,

            // mf2dd_qcopy_1660s.img.lz
            1660,

            // mf2ed.img.lz
            5760,

            // mf2hd_2m.dsk.lz
            3608,

            // mf2hd_2m_fast.img.lz
            3608,

            // mf2hd_2mgui.dsk.lz
            15776,

            // mf2hd_2m_max.dsk.lz
            3772,

            // mf2hd_2m_max.img.lz
            3772,

            // mf2hd_dmf.img.lz
            3360,

            // mf2hd.dsk.lz
            2880,

            // mf2hd_fdformat_168.dsk.lz
            3360,

            // mf2hd_fdformat_172.dsk.lz
            3444,

            // mf2hd_freedos_3360s.img.lz
            3360,

            // mf2hd_freedos_3486s.img.lz
            3486,

            // mf2hd.img.lz
            2880,

            // mf2hd_maxiform_3200s.img.lz
            3200,

            // mf2hd_nec.img.lz
            2464,

            // mf2hd_qcopy_2460s.img.lz
            2460,

            // mf2hd_qcopy_2720s.img.lz
            2720,

            // mf2hd_qcopy_2788s.img.lz
            2788,

            // mf2hd_qcopy_2880s.img.lz
            2880,

            // mf2hd_qcopy_2952s.img.lz
            2952,

            // mf2hd_qcopy_2988s.img.lz
            2988,

            // mf2hd_qcopy_3200s.img.lz
            3200,

            // mf2hd_qcopy_3320s.img.lz
            3320,

            // mf2hd_qcopy_3360s.img.lz
            3360,

            // mf2hd_qcopy_3486s.img.lz
            3486,

            // mf2hd_xdf.dsk.lz
            3680,

            // mf2hd_xdf.img.lz
            3680
        };

        readonly uint[] _sectorSize =
        {
            // DSKA0000.IMG.lz
            512,

            // DSKA0001.IMG.lz
            512,

            // DSKA0009.IMG.lz
            512,

            // DSKA0010.IMG.lz
            512,

            // DSKA0024.IMG.lz
            512,

            // DSKA0025.IMG.lz
            512,

            // DSKA0030.IMG.lz
            512,

            // DSKA0035.IMG.lz
            512,

            // DSKA0036.IMG.lz
            512,

            // DSKA0037.IMG.lz
            512,

            // DSKA0038.IMG.lz
            512,

            // DSKA0039.IMG.lz
            512,

            // DSKA0040.IMG.lz
            512,

            // DSKA0041.IMG.lz
            512,

            // DSKA0042.IMG.lz
            512,

            // DSKA0043.IMG.lz
            512,

            // DSKA0044.IMG.lz
            512,

            // DSKA0045.IMG.lz
            512,

            // DSKA0046.IMG.lz
            512,

            // DSKA0047.IMG.lz
            512,

            // DSKA0048.IMG.lz
            512,

            // DSKA0049.IMG.lz
            512,

            // DSKA0050.IMG.lz
            512,

            // DSKA0051.IMG.lz
            512,

            // DSKA0052.IMG.lz
            512,

            // DSKA0053.IMG.lz
            512,

            // DSKA0054.IMG.lz
            512,

            // DSKA0055.IMG.lz
            512,

            // DSKA0056.IMG.lz
            512,

            // DSKA0057.IMG.lz
            512,

            // DSKA0058.IMG.lz
            512,

            // DSKA0059.IMG.lz
            512,

            // DSKA0060.IMG.lz
            512,

            // DSKA0069.IMG.lz
            512,

            // DSKA0073.IMG.lz
            512,

            // DSKA0074.IMG.lz
            512,

            // DSKA0075.IMG.lz
            512,

            // DSKA0076.IMG.lz
            512,

            // DSKA0077.IMG.lz
            512,

            // DSKA0078.IMG.lz
            512,

            // DSKA0080.IMG.lz
            512,

            // DSKA0081.IMG.lz
            512,

            // DSKA0082.IMG.lz
            512,

            // DSKA0083.IMG.lz
            512,

            // DSKA0084.IMG.lz
            512,

            // DSKA0085.IMG.lz
            512,

            // DSKA0089.IMG.lz
            512,

            // DSKA0090.IMG.lz
            512,

            // DSKA0091.IMG.lz
            512,

            // DSKA0092.IMG.lz
            512,

            // DSKA0093.IMG.lz
            512,

            // DSKA0094.IMG.lz
            512,

            // DSKA0097.IMG.lz
            512,

            // DSKA0098.IMG.lz
            512,

            // DSKA0099.IMG.lz
            512,

            // DSKA0101.IMG.lz
            512,

            // DSKA0103.IMG.lz
            512,

            // DSKA0105.IMG.lz
            512,

            // DSKA0106.IMG.lz
            512,

            // DSKA0107.IMG.lz
            512,

            // DSKA0108.IMG.lz
            512,

            // DSKA0109.IMG.lz
            512,

            // DSKA0110.IMG.lz
            512,

            // DSKA0111.IMG.lz
            512,

            // DSKA0112.IMG.lz
            512,

            // DSKA0113.IMG.lz
            512,

            // DSKA0114.IMG.lz
            512,

            // DSKA0115.IMG.lz
            512,

            // DSKA0116.IMG.lz
            512,

            // DSKA0117.IMG.lz
            512,

            // DSKA0120.IMG.lz
            512,

            // DSKA0121.IMG.lz
            512,

            // DSKA0122.IMG.lz
            512,

            // DSKA0123.IMG.lz
            512,

            // DSKA0124.IMG.lz
            512,

            // DSKA0125.IMG.lz
            512,

            // DSKA0126.IMG.lz
            512,

            // DSKA0163.IMG.lz
            512,

            // DSKA0164.IMG.lz
            512,

            // DSKA0166.IMG.lz
            512,

            // DSKA0168.IMG.lz
            512,

            // DSKA0169.IMG.lz
            512,

            // DSKA0173.IMG.lz
            512,

            // DSKA0174.IMG.lz
            512,

            // DSKA0175.IMG.lz
            512,

            // DSKA0180.IMG.lz
            512,

            // DSKA0181.IMG.lz
            512,

            // DSKA0182.IMG.lz
            512,

            // DSKA0183.IMG.lz
            512,

            // DSKA0262.IMG.lz
            512,

            // DSKA0263.IMG.lz
            512,

            // DSKA0264.IMG.lz
            512,

            // DSKA0265.IMG.lz
            512,

            // DSKA0266.IMG.lz
            512,

            // DSKA0267.IMG.lz
            512,

            // DSKA0268.IMG.lz
            512,

            // DSKA0269.IMG.lz
            512,

            // DSKA0270.IMG.lz
            512,

            // DSKA0271.IMG.lz
            512,

            // DSKA0272.IMG.lz
            512,

            // DSKA0273.IMG.lz
            512,

            // DSKA0280.IMG.lz
            512,

            // DSKA0281.IMG.lz
            512,

            // DSKA0282.IMG.lz
            512,

            // DSKA0283.IMG.lz
            512,

            // DSKA0284.IMG.lz
            512,

            // DSKA0285.IMG.lz
            512,

            // DSKA0287.IMG.lz
            512,

            // DSKA0288.IMG.lz
            512,

            // DSKA0289.IMG.lz
            512,

            // DSKA0290.IMG.lz
            512,

            // DSKA0291.IMG.lz
            512,

            // DSKA0299.IMG.lz
            512,

            // DSKA0300.IMG.lz
            512,

            // DSKA0301.IMG.lz
            512,

            // DSKA0302.IMG.lz
            512,

            // DSKA0303.IMG.lz
            512,

            // DSKA0304.IMG.lz
            512,

            // DSKA0305.IMG.lz
            512,

            // DSKA0308.IMG.lz
            512,

            // DSKA0311.IMG.lz
            512,

            // DSKA0314.IMG.lz
            512,

            // DSKA0316.IMG.lz
            512,

            // DSKA0317.IMG.lz
            512,

            // DSKA0318.IMG.lz
            512,

            // DSKA0319.IMG.lz
            512,

            // DSKA0320.IMG.lz
            512,

            // DSKA0322.IMG.lz
            512,

            // md1dd8.img.lz
            512,

            // md1dd.img.lz
            512,

            // md2dd_2m_fast.img.lz
            512,

            // md2dd_2m_max.img.lz
            512,

            // md2dd8.img.lz
            512,

            // md2dd_freedos_800s.img.lz
            512,

            // md2dd.img.lz
            512,

            // md2dd_maxiform_1640s.img.lz
            512,

            // md2dd_maxiform_840s.img.lz
            512,

            // md2dd_qcopy_1476s.img.lz
            512,

            // md2dd_qcopy_1600s.img.lz
            512,

            // md2dd_qcopy_1640s.img.lz
            512,

            // md2hd_2m_fast.img.lz
            512,

            // md2hd_2m_max.img.lz
            512,

            // md2hd.img.lz
            512,

            // md2hd_maxiform_2788s.img.lz
            512,

            // md2hd_nec.img.lz
            1024,

            // md2hd_xdf.img.lz
            512,

            // mf2dd_2m.dsk.lz
            512,

            // mf2dd_2m_fast.img.lz
            512,

            // mf2dd_2mgui.dsk.lz
            128,

            // mf2dd_2m_max.dsk.lz
            512,

            // mf2dd_2m_max.img.lz
            512,

            // mf2dd.dsk.lz
            512,

            // mf2dd_fdformat_800.dsk.lz
            512,

            // mf2dd_fdformat_820.dsk.lz
            512,

            // mf2dd_freedos_1600s.img.lz
            512,

            // mf2dd_freedos.dsk.lz
            512,

            // mf2dd.img.lz
            512,

            // mf2dd_maxiform_1600s.img.lz
            512,

            // mf2dd_qcopy_1494s.img.lz
            512,

            // mf2dd_qcopy_1600s.img.lz
            512,

            // mf2dd_qcopy_1660s.img.lz
            512,

            // mf2ed.img.lz
            512,

            // mf2hd_2m.dsk.lz
            512,

            // mf2hd_2m_fast.img.lz
            512,

            // mf2hd_2mgui.dsk.lz
            128,

            // mf2hd_2m_max.dsk.lz
            512,

            // mf2hd_2m_max.img.lz
            512,

            // mf2hd_dmf.img.lz
            512,

            // mf2hd.dsk.lz
            512,

            // mf2hd_fdformat_168.dsk.lz
            512,

            // mf2hd_fdformat_172.dsk.lz
            512,

            // mf2hd_freedos_3360s.img.lz
            512,

            // mf2hd_freedos_3486s.img.lz
            512,

            // mf2hd.img.lz
            512,

            // mf2hd_maxiform_3200s.img.lz
            512,

            // mf2hd_nec.img.lz
            1024,

            // mf2hd_qcopy_2460s.img.lz
            512,

            // mf2hd_qcopy_2720s.img.lz
            512,

            // mf2hd_qcopy_2788s.img.lz
            512,

            // mf2hd_qcopy_2880s.img.lz
            512,

            // mf2hd_qcopy_2952s.img.lz
            512,

            // mf2hd_qcopy_2988s.img.lz
            512,

            // mf2hd_qcopy_3200s.img.lz
            512,

            // mf2hd_qcopy_3320s.img.lz
            512,

            // mf2hd_qcopy_3360s.img.lz
            512,

            // mf2hd_qcopy_3486s.img.lz
            512,

            // mf2hd_xdf.dsk.lz
            512,

            // mf2hd_xdf.img.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // DSKA0000.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0001.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0009.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0010.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0024.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0025.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0030.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0035.IMG.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0036.IMG.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0037.IMG.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0038.IMG.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0039.IMG.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0040.IMG.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0041.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0042.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0043.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0044.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0045.IMG.lz
            MediaType.DOS_525_HD,

            // DSKA0046.IMG.lz
            MediaType.Unknown,

            // DSKA0047.IMG.lz
            MediaType.DOS_35_DS_DD_8,

            // DSKA0048.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0049.IMG.lz
            MediaType.Unknown,

            // DSKA0050.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0051.IMG.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0052.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0053.IMG.lz
            MediaType.Unknown,

            // DSKA0054.IMG.lz
            MediaType.Unknown,

            // DSKA0055.IMG.lz
            MediaType.Unknown,

            // DSKA0056.IMG.lz
            MediaType.DMF,

            // DSKA0057.IMG.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0058.IMG.lz
            MediaType.Unknown,

            // DSKA0059.IMG.lz
            MediaType.Unknown,

            // DSKA0060.IMG.lz
            MediaType.Unknown,

            // DSKA0069.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0073.IMG.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0074.IMG.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0075.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0076.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0077.IMG.lz
            MediaType.Unknown,

            // DSKA0078.IMG.lz
            MediaType.DOS_525_HD,

            // DSKA0080.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0081.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0082.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0083.IMG.lz
            MediaType.Unknown,

            // DSKA0084.IMG.lz
            MediaType.DMF,

            // DSKA0085.IMG.lz
            MediaType.Unknown,

            // DSKA0089.IMG.lz
            MediaType.XDF_525,

            // DSKA0090.IMG.lz
            MediaType.XDF_35,

            // DSKA0091.IMG.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0092.IMG.lz
            MediaType.Unknown,

            // DSKA0093.IMG.lz
            MediaType.Unknown,

            // DSKA0094.IMG.lz
            MediaType.Unknown,

            // DSKA0097.IMG.lz
            MediaType.Unknown,

            // DSKA0098.IMG.lz
            MediaType.Unknown,

            // DSKA0099.IMG.lz
            MediaType.Unknown,

            // DSKA0101.IMG.lz
            MediaType.Unknown,

            // DSKA0103.IMG.lz
            MediaType.Unknown,

            // DSKA0105.IMG.lz
            MediaType.Unknown,

            // DSKA0106.IMG.lz
            MediaType.Unknown,

            // DSKA0107.IMG.lz
            MediaType.Unknown,

            // DSKA0108.IMG.lz
            MediaType.Unknown,

            // DSKA0109.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0110.IMG.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0111.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0112.IMG.lz
            MediaType.Unknown,

            // DSKA0113.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0114.IMG.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0115.IMG.lz
            MediaType.Unknown,

            // DSKA0116.IMG.lz
            MediaType.Unknown,

            // DSKA0117.IMG.lz
            MediaType.Unknown,

            // DSKA0120.IMG.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0121.IMG.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0122.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0123.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0124.IMG.lz
            MediaType.DOS_525_HD,

            // DSKA0125.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0126.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0163.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0164.IMG.lz
            MediaType.Unknown,

            // DSKA0166.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0168.IMG.lz
            MediaType.DOS_525_HD,

            // DSKA0169.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0173.IMG.lz
            MediaType.DOS_35_SS_DD_9,

            // DSKA0174.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0175.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0180.IMG.lz
            MediaType.Unknown,

            // DSKA0181.IMG.lz
            MediaType.DMF,

            // DSKA0182.IMG.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0183.IMG.lz
            MediaType.Unknown,

            // DSKA0262.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0263.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0264.IMG.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0265.IMG.lz
            MediaType.Unknown,

            // DSKA0266.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0267.IMG.lz
            MediaType.XDF_525,

            // DSKA0268.IMG.lz
            MediaType.Unknown,

            // DSKA0269.IMG.lz
            MediaType.Unknown,

            // DSKA0270.IMG.lz
            MediaType.Unknown,

            // DSKA0271.IMG.lz
            MediaType.DMF,

            // DSKA0272.IMG.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0273.IMG.lz
            MediaType.Unknown,

            // DSKA0280.IMG.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0281.IMG.lz
            MediaType.Unknown,

            // DSKA0282.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0283.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0284.IMG.lz
            MediaType.Unknown,

            // DSKA0285.IMG.lz
            MediaType.Unknown,

            // DSKA0287.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0288.IMG.lz
            MediaType.Unknown,

            // DSKA0289.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0290.IMG.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0291.IMG.lz
            MediaType.Unknown,

            // DSKA0299.IMG.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0300.IMG.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0301.IMG.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0302.IMG.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0303.IMG.lz
            MediaType.DOS_525_HD,

            // DSKA0304.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0305.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0308.IMG.lz
            MediaType.CBM_35_DD,

            // DSKA0311.IMG.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0314.IMG.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0316.IMG.lz
            MediaType.DOS_35_HD,

            // DSKA0317.IMG.lz
            MediaType.DMF,

            // DSKA0318.IMG.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0319.IMG.lz
            MediaType.DMF,

            // DSKA0320.IMG.lz
            MediaType.DMF,

            // DSKA0322.IMG.lz
            MediaType.Unknown,

            // md1dd8.img.lz
            MediaType.DOS_525_SS_DD_8,

            // md1dd.img.lz
            MediaType.DOS_525_SS_DD_9,

            // md2dd_2m_fast.img.lz
            MediaType.FDFORMAT_35_DD,

            // md2dd_2m_max.img.lz
            MediaType.Unknown,

            // md2dd8.img.lz
            MediaType.DOS_525_DS_DD_8,

            // md2dd_freedos_800s.img.lz
            MediaType.Unknown,

            // md2dd.img.lz
            MediaType.DOS_525_DS_DD_9,

            // md2dd_maxiform_1640s.img.lz
            MediaType.FDFORMAT_35_DD,

            // md2dd_maxiform_840s.img.lz
            MediaType.Unknown,

            // md2dd_qcopy_1476s.img.lz
            MediaType.Unknown,

            // md2dd_qcopy_1600s.img.lz
            MediaType.CBM_35_DD,

            // md2dd_qcopy_1640s.img.lz
            MediaType.FDFORMAT_35_DD,

            // md2hd_2m_fast.img.lz
            MediaType.Unknown,

            // md2hd_2m_max.img.lz
            MediaType.Unknown,

            // md2hd.img.lz
            MediaType.DOS_525_HD,

            // md2hd_maxiform_2788s.img.lz
            MediaType.FDFORMAT_525_HD,

            // md2hd_nec.img.lz
            MediaType.SHARP_525,

            // md2hd_xdf.img.lz
            MediaType.XDF_525,

            // mf2dd_2m.dsk.lz
            MediaType.Unknown,

            // mf2dd_2m_fast.img.lz
            MediaType.Unknown,

            // mf2dd_2mgui.dsk.lz
            MediaType.Unknown,

            // mf2dd_2m_max.dsk.lz
            MediaType.Unknown,

            // mf2dd_2m_max.img.lz
            MediaType.Unknown,

            // mf2dd.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_fdformat_800.dsk.lz
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_820.dsk.lz
            MediaType.FDFORMAT_35_DD,

            // mf2dd_freedos_1600s.img.lz
            MediaType.CBM_35_DD,

            // mf2dd_freedos.dsk.lz
            MediaType.CBM_35_DD,

            // mf2dd.img.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_maxiform_1600s.img.lz
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1494s.img.lz
            MediaType.Unknown,

            // mf2dd_qcopy_1600s.img.lz
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1660s.img.lz
            MediaType.Unknown,

            // mf2ed.img.lz
            MediaType.ECMA_147,

            // mf2hd_2m.dsk.lz
            MediaType.Unknown,

            // mf2hd_2m_fast.img.lz
            MediaType.Unknown,

            // mf2hd_2mgui.dsk.lz
            MediaType.Unknown,

            // mf2hd_2m_max.dsk.lz
            MediaType.Unknown,

            // mf2hd_2m_max.img.lz
            MediaType.Unknown,

            // mf2hd_dmf.img.lz
            MediaType.DMF,

            // mf2hd.dsk.lz
            MediaType.DOS_35_HD,

            // mf2hd_fdformat_168.dsk.lz
            MediaType.DMF,

            // mf2hd_fdformat_172.dsk.lz
            MediaType.FDFORMAT_35_HD,

            // mf2hd_freedos_3360s.img.lz
            MediaType.DMF,

            // mf2hd_freedos_3486s.img.lz
            MediaType.Unknown,

            // mf2hd.img.lz
            MediaType.DOS_35_HD,

            // mf2hd_maxiform_3200s.img.lz
            MediaType.Unknown,

            // mf2hd_nec.img.lz
            MediaType.SHARP_525,

            // mf2hd_qcopy_2460s.img.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2720s.img.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2788s.img.lz
            MediaType.FDFORMAT_525_HD,

            // mf2hd_qcopy_2880s.img.lz
            MediaType.DOS_35_HD,

            // mf2hd_qcopy_2952s.img.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2988s.img.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3200s.img.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3320s.img.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3360s.img.lz
            MediaType.DMF,

            // mf2hd_qcopy_3486s.img.lz
            MediaType.Unknown,

            // mf2hd_xdf.dsk.lz
            MediaType.XDF_35,

            // mf2hd_xdf.img.lz
            MediaType.XDF_35
        };

        readonly string[] _md5S =
        {
            // DSKA0000.IMG.lz
            "e8bbbd22db87181974e12ba0227ea011",

            // DSKA0001.IMG.lz
            "9f5635f3df4d880a500910b0ad1ab535",

            // DSKA0009.IMG.lz
            "95ea232f59e44db374b994cfe7f1c07f",

            // DSKA0010.IMG.lz
            "9e2b01f4397db2a6c76e2bc267df37b3",

            // DSKA0024.IMG.lz
            "2302991363cb3681cffdc4388915b51e",

            // DSKA0025.IMG.lz
            "f7dd138edcab7bd328d7396d48aac395",

            // DSKA0030.IMG.lz
            "af83d011608042d35021e39aa5e10b2f",

            // DSKA0035.IMG.lz
            "6642c1a32d2c58e93481d664974fc202",

            // DSKA0036.IMG.lz
            "846f01b8b60cb3c775bd66419e977926",

            // DSKA0037.IMG.lz
            "5101f89850dc28efbcfb7622086a9ddf",

            // DSKA0038.IMG.lz
            "8e570be2ed1f00ddea82e50a2d9c446a",

            // DSKA0039.IMG.lz
            "abba2a1ddd60a649047a9c44d94bbeae",

            // DSKA0040.IMG.lz
            "e3bc48bec81be5b35be73d41fdffd2ab",

            // DSKA0041.IMG.lz
            "43b5068af9d016d1432eb2e12d2b802a",

            // DSKA0042.IMG.lz
            "5bf2ad4dc300592604b6e32f8b8e2656",

            // DSKA0043.IMG.lz
            "cb9a832ca6a4097b8ccc30d2108e1f7d",

            // DSKA0044.IMG.lz
            "56d181a6bb8713e6b2854fe8887faab6",

            // DSKA0045.IMG.lz
            "41aef7cff26aefda1add8d49c5b962c2",

            // DSKA0046.IMG.lz
            "2437c5f089f1cba3866b36360b016f16",

            // DSKA0047.IMG.lz
            "bdaa8f17373b265830fdf3a06b794367",

            // DSKA0048.IMG.lz
            "629932c285478d0540ff7936aa008351",

            // DSKA0049.IMG.lz
            "7a2abef5d4701e2e49abb05af8d4da50",

            // DSKA0050.IMG.lz
            "e3507522c914264f44fb2c92c3170c09",

            // DSKA0051.IMG.lz
            "824fe65dbb1a42b6b94f05405ef984f2",

            // DSKA0052.IMG.lz
            "1a8c2e78e7132cf9ba5d6c2b75876be0",

            // DSKA0053.IMG.lz
            "936b20bb0966fe693b4d5e2353e24846",

            // DSKA0054.IMG.lz
            "803b01a0b440c2837d37c21308f30cd5",

            // DSKA0055.IMG.lz
            "aa0d31f914760cc4cde75479779ebed6",

            // DSKA0056.IMG.lz
            "31269ed6464302ae26d22b7c87bceb23",

            // DSKA0057.IMG.lz
            "5e413433c54f48978d281c6e66d1106e",

            // DSKA0058.IMG.lz
            "4fc28b0128543b2eb70f6432c4c8a980",

            // DSKA0059.IMG.lz
            "24a7459d080cea3a60d131b8fd7dc5d1",

            // DSKA0060.IMG.lz
            "2031b1e16ee2defc0d15f732f633df33",

            // DSKA0069.IMG.lz
            "5fc19ca552b6db957061e9a1750394d2",

            // DSKA0073.IMG.lz
            "a33b46f042b78fe3d0b3c5dbb3908a93",

            // DSKA0074.IMG.lz
            "565d3c001cbb532154aa5d3c65b2439c",

            // DSKA0075.IMG.lz
            "e60442c3ebd72c99bdd7545fdba59613",

            // DSKA0076.IMG.lz
            "058a33a129539285c9b64010496af52f",

            // DSKA0077.IMG.lz
            "0726ecbc38965d30a6222c3e74cd1aa3",

            // DSKA0078.IMG.lz
            "c9a193837db7d8a5eb025eb41e8a76d7",

            // DSKA0080.IMG.lz
            "c38d69ac88520f14fcc6d6ced22b065d",

            // DSKA0081.IMG.lz
            "91d51964e1e64ef3f6f622fa19aa833c",

            // DSKA0082.IMG.lz
            "db36d9651c952ff679ec33223c8db2d3",

            // DSKA0083.IMG.lz
            "5f1d98806309aee7f81de72e51e6d386",

            // DSKA0084.IMG.lz
            "1207a1cc7ff73d4f74c8984b4e7db33f",

            // DSKA0085.IMG.lz
            "c97a3081fd25474b6b7945b8572d5ab8",

            // DSKA0089.IMG.lz
            "0abf995856080e5292e63c63f7c97a45",

            // DSKA0090.IMG.lz
            "8be2aaf6ecea213aee9fc82c8a85061e",

            // DSKA0091.IMG.lz
            "0a432572a28d3b53a0cf2b5c211fe777",

            // DSKA0092.IMG.lz
            "cd84fa2d62ac7c36783224c3ba0be664",

            // DSKA0093.IMG.lz
            "63d29a9d867d924421c10793a0f22965",

            // DSKA0094.IMG.lz
            "21778906886c0314f0f33c4b0040ba16",

            // DSKA0097.IMG.lz
            "6b8e89b1d5117ba19c3e52544ffe041e",

            // DSKA0098.IMG.lz
            "543fc539902eb66b5c312d7908ecf97a",

            // DSKA0099.IMG.lz
            "b30709f798bfb8469d02a82c882f780c",

            // DSKA0101.IMG.lz
            "0f3e923010b50b550591a89ea2dee62b",

            // DSKA0103.IMG.lz
            "d5b927503abcd1978496bc679bb9c2f7",

            // DSKA0105.IMG.lz
            "d40a99cb549fcfb26fcf9ef01b5dfca7",

            // DSKA0106.IMG.lz
            "7b41dd9ca7eb32828960eb1417a6092a",

            // DSKA0107.IMG.lz
            "126dfd25363c076727dfaab03955c931",

            // DSKA0108.IMG.lz
            "e6492aac144f5f6f593b84c64680cf64",

            // DSKA0109.IMG.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0110.IMG.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0111.IMG.lz
            "f01541de322c8d6d7321084d7a245e7b",

            // DSKA0112.IMG.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0113.IMG.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0114.IMG.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0115.IMG.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0116.IMG.lz
            "6631b66fdfd89319323771c41334c7ba",

            // DSKA0117.IMG.lz
            "56471a253f4d6803b634e2bbff6c0931",

            // DSKA0120.IMG.lz
            "7d36aee5a3071ff75b979f3acb649c40",

            // DSKA0121.IMG.lz
            "0ccb62039363ab544c69eca229a17fae",

            // DSKA0122.IMG.lz
            "7851d31fad9302ff45d3ded4fba25387",

            // DSKA0123.IMG.lz
            "915b08c82591e8488320e001b7303b6d",

            // DSKA0124.IMG.lz
            "5e5ea6fe9adf842221fdc60e56630405",

            // DSKA0125.IMG.lz
            "a22e254f7e3526ec30dc4915a19fcb52",

            // DSKA0126.IMG.lz
            "ddc6c1200c60e9f7796280f50c2e5283",

            // DSKA0163.IMG.lz
            "be05d1ff10ef8b2220546c4db962ac9e",

            // DSKA0164.IMG.lz
            "32823b9009c99b6711e89336ad03ec7f",

            // DSKA0166.IMG.lz
            "1c8b03a8550ed3e70e1c78316aa445aa",

            // DSKA0168.IMG.lz
            "0bdf9130c07bb5d558a4705249f949d0",

            // DSKA0169.IMG.lz
            "2dafeddaa99e7dc0db5ef69e128f9c8e",

            // DSKA0173.IMG.lz
            "028769dc0abefab1740cc309432588b6",

            // DSKA0174.IMG.lz
            "152023525154b45ab26687190bac94db",

            // DSKA0175.IMG.lz
            "db38ecd93f28dd065927fed21917eed5",

            // DSKA0180.IMG.lz
            "f206c0caa4e0eda37233ab6e89ab5493",

            // DSKA0181.IMG.lz
            "554492a7b41f4cd9068a3a2b70eb0e5f",

            // DSKA0182.IMG.lz
            "36dd03967a2a3369538cad29b8b74b71",

            // DSKA0183.IMG.lz
            "4f5c02448e75bbc086e051c728414513",

            // DSKA0262.IMG.lz
            "5ac0a9fc7337f761098f816359b0f6f7",

            // DSKA0263.IMG.lz
            "1ea6ec8e663218b1372048f6e25795b5",

            // DSKA0264.IMG.lz
            "77a1167b1b9043496e32b8578cde0ff0",

            // DSKA0265.IMG.lz
            "4b07d760d65f3f0f8ffa5f2b81cee907",

            // DSKA0266.IMG.lz
            "32c044c5c2b0bd13806149a759c14935",

            // DSKA0267.IMG.lz
            "8752095abc13dba3f3467669da333891",

            // DSKA0268.IMG.lz
            "aece7cd34bbba3e75307fa70404d9d30",

            // DSKA0269.IMG.lz
            "5289afb16a6e4a33213e3bcca56c6230",

            // DSKA0270.IMG.lz
            "1aef0a0ba233476db6567878c3c2b266",

            // DSKA0271.IMG.lz
            "b96596711f4d2ee85dfda0fe3b9f26c3",

            // DSKA0272.IMG.lz
            "a4f461af7fda5e93a7ab63fcbb7e7683",

            // DSKA0273.IMG.lz
            "8f7f7099d4475f6631fcf0a79b031d61",

            // DSKA0280.IMG.lz
            "4feeaf4b4ee5dad85db727fbbda4b6d1",

            // DSKA0281.IMG.lz
            "3c77ca681df78e4cd7baa162aa9b0859",

            // DSKA0282.IMG.lz
            "51da1f86c49657ffdb367bb2ddeb7990",

            // DSKA0283.IMG.lz
            "b81a4987f89936630b8ebc62e4bbce6e",

            // DSKA0284.IMG.lz
            "f76f92dd326c99c5efad5ee58daf72e1",

            // DSKA0285.IMG.lz
            "b6f2c10e42908e334025bc4ffd81e771",

            // DSKA0287.IMG.lz
            "f2f409ea2a62a7866fd2777cc4fc9739",

            // DSKA0288.IMG.lz
            "3e441d69cec5c3169274e1379de4af4b",

            // DSKA0289.IMG.lz
            "30a93f30dd4485c6fc037fe0775d3fc7",

            // DSKA0290.IMG.lz
            "e0caf02cce5597c98313bcc480366ec7",

            // DSKA0291.IMG.lz
            "4af4904d2b3c815da7bef7049209f5eb",

            // DSKA0299.IMG.lz
            "39bf5a98bcb2185d855ac06378febcfa",

            // DSKA0300.IMG.lz
            "dc20055b6e6fd6f8e1114d4be2effeed",

            // DSKA0301.IMG.lz
            "56af9256cf71d5aac5fd5d363674bc49",

            // DSKA0302.IMG.lz
            "bbba1e2d1418e05c3a4e7b4d585d160b",

            // DSKA0303.IMG.lz
            "bca3a045e81617f7f5ebb5a8818eac47",

            // DSKA0304.IMG.lz
            "a296663cb8e75e94603221352f29cfff",

            // DSKA0305.IMG.lz
            "ecda36ebf0e1100233cb0ec722c18583",

            // DSKA0308.IMG.lz
            "bbe58e26b8f8f822cd3edfd37a4e4924",

            // DSKA0311.IMG.lz
            "b9b6ebdf711364c979de7cf70c3a438a",

            // DSKA0314.IMG.lz
            "d37424f367f545acbb397f2bed766843",

            // DSKA0316.IMG.lz
            "9963dd6f19ce6bd56eabeccdfbbd821a",

            // DSKA0317.IMG.lz
            "acf6604559ae8217f7869823e2429024",

            // DSKA0318.IMG.lz
            "23bf2139cdfdc4c16db058fd31ea6481",

            // DSKA0319.IMG.lz
            "fa26adda0415f02057b113ad29c80c8d",

            // DSKA0320.IMG.lz
            "4f2a8d036fefd6c6c88d99eda3aa12b7",

            // DSKA0322.IMG.lz
            "e794a3ffa4069ea999fdf7146710fa9e",

            // md1dd8.img.lz
            "d81f5cb64fd0b99f138eab34110bbc3c",

            // md1dd.img.lz
            "a89006a75d13bee9202d1d6e52721ccb",

            // md2dd_2m_fast.img.lz
            "319fa8bef964c2a63e34bdb48e77cc4e",

            // md2dd_2m_max.img.lz
            "306a61469b4c3c83f3e5f9ae409d83cd",

            // md2dd8.img.lz
            "beef1cdb004dc69391d6b3d508988b95",

            // md2dd_freedos_800s.img.lz
            "29054ef703394ee3b35e849468a412ba",

            // md2dd.img.lz
            "6213897b7dbf263f12abf76901d43862",

            // md2dd_maxiform_1640s.img.lz
            "c91e852828c2aeee2fc94a6adbeed0ae",

            // md2dd_maxiform_840s.img.lz
            "efb6cfe53a6770f0ae388cb2c7f46264",

            // md2dd_qcopy_1476s.img.lz
            "6116f7c1397cadd55ba8d79c2aadc9dd",

            // md2dd_qcopy_1600s.img.lz
            "93100f8d86e5d0d0e6340f59c52a5e0d",

            // md2dd_qcopy_1640s.img.lz
            "cf7b7d43aa70863bedcc4a8432a5af67",

            // md2hd_2m_fast.img.lz
            "215198cf2a336e718208fc207bb62c6d",

            // md2hd_2m_max.img.lz
            "2c96964b5d91444302e21721c25ea120",

            // md2hd.img.lz
            "02259cd5fbcc20f8484aa6bece7a37c6",

            // md2hd_maxiform_2788s.img.lz
            "09ca721aa883d5bbaa422c7943b0782c",

            // md2hd_nec.img.lz
            "84812b791fd2113b4aa00894f6894339",

            // md2hd_xdf.img.lz
            "d78dc81491edeec99aa202d02f3daf00",

            // mf2dd_2m.dsk.lz
            "9a8670fbaf6307b8d5f32aa10e1be435",

            // mf2dd_2m_fast.img.lz
            "05d29642cdcddafa0dcaff91682f8fe0",

            // mf2dd_2mgui.dsk.lz
            "beb782f6bc970e32ceef79cd112e2e48",

            // mf2dd_2m_max.dsk.lz
            "a99603cd3219aab1299e66b2999f0e57",

            // mf2dd_2m_max.img.lz
            "3da419125f45e1fe3b46f6fad3acc1c2",

            // mf2dd.dsk.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2dd_fdformat_800.dsk.lz
            "c533488a21098a62c85f1649abda2803",

            // mf2dd_fdformat_820.dsk.lz
            "81d3bfec7b201f6a4503eb24c4394d4a",

            // mf2dd_freedos_1600s.img.lz
            "d07f7ffaee89742c6477aaaf94eb5715",

            // mf2dd_freedos.dsk.lz
            "1ff7649b679ba22ff20d39ff717dbec8",

            // mf2dd.img.lz
            "9827ba1b3e9cac41263caabd862e78f9",

            // mf2dd_maxiform_1600s.img.lz
            "56af87802a9852e6e01e08d544740816",

            // mf2dd_qcopy_1494s.img.lz
            "fd7fb1ba11cdfe11db54af0322abf59d",

            // mf2dd_qcopy_1600s.img.lz
            "d9db52d992a76bf3bbc626ff844215a5",

            // mf2dd_qcopy_1660s.img.lz
            "5949d0be57ce8bffcda7c4be4d1348ee",

            // mf2ed.img.lz
            "4aeafaf2a088d6a7406856dce8118567",

            // mf2hd_2m.dsk.lz
            "2f6964d410b275c8e9f60fe2f24b361a",

            // mf2hd_2m_fast.img.lz
            "967726aede85c68f66887672078f8856",

            // mf2hd_2mgui.dsk.lz
            "0037b5497d5cb0c7721085f61e223b6a",

            // mf2hd_2m_max.dsk.lz
            "3fa4f87d7058ba940b88e0d80f0d7ded",

            // mf2hd_2m_max.img.lz
            "5a6d961ed5f089364f2816692bcbe685",

            // mf2hd_dmf.img.lz
            "b042310181410227d0072fef1e98a989",

            // mf2hd.dsk.lz
            "b4a602f67903c46eef62addb0780aa56",

            // mf2hd_fdformat_168.dsk.lz
            "1e06f21a1c11ea3347212da115bca08f",

            // mf2hd_fdformat_172.dsk.lz
            "3fc3a03d049416d81f81cc3b9ea8e5de",

            // mf2hd_freedos_3360s.img.lz
            "2bfd2e0a81bad704f8fc7758358cfcca",

            // mf2hd_freedos_3486s.img.lz
            "a79ec33c623697b4562dacaed31523b8",

            // mf2hd.img.lz
            "00e61c06bf29f0c04a7eabe2dbd7efb6",

            // mf2hd_maxiform_3200s.img.lz
            "3c4becd695ed25866d39966a9a93c2d9",

            // mf2hd_nec.img.lz
            "626ec389d4f8968170401b3775181a2b",

            // mf2hd_qcopy_2460s.img.lz
            "72282e11f7d91bf9c090b550fabfe80d",

            // mf2hd_qcopy_2720s.img.lz
            "457c1126dc7f36bbbabe9e17e90372e3",

            // mf2hd_qcopy_2788s.img.lz
            "852181d5913c6f290872c66bbe992314",

            // mf2hd_qcopy_2880s.img.lz
            "2980cc32504c945598dc50f1db576994",

            // mf2hd_qcopy_2952s.img.lz
            "c1c58d74fffb3656dd7f60f74ae8a629",

            // mf2hd_qcopy_2988s.img.lz
            "097bb2fd34cee5ebde7b5641975ffd60",

            // mf2hd_qcopy_3200s.img.lz
            "e45d41a61fbe48f328c995fcc10a5548",

            // mf2hd_qcopy_3320s.img.lz
            "c25f2a57c71db1cd4fea2263598f544a",

            // mf2hd_qcopy_3360s.img.lz
            "15f71b92bd72aba5d80bf70eca4d5b1e",

            // mf2hd_qcopy_3486s.img.lz
            "d88c8d818e238c9e52b8588b5fd52efe",

            // mf2hd_xdf.dsk.lz
            "3d5fcdaf627257ae9f50a06bdba26965",

            // mf2hd_xdf.img.lz
            "4cb9398cf02ed9e08d0972c1ccba804b"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DRI DISKCOPY");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    var  image  = new DiscImages.DriDiskCopy();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

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
        const uint _sectorsToRead = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    var   image       = new DiscImages.DriDiskCopy();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
                    var ctx = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= _sectorsToRead)
                        {
                            sector      =  image.ReadSectors(doneSectors, _sectorsToRead);
                            doneSectors += _sectorsToRead;
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