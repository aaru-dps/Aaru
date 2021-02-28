// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RayDIM.cs
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
using Aaru.DiscImages;
using Aaru.Filters;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class RayDIM
    {
        readonly string[] _testFiles =
        {
            "5f1dd8.dim.lz", "5f1dd8_pass.dim.lz", "5f1dd.dim.lz", "5f1dd_pass.dim.lz", "5f2dd8.dim.lz",
            "5f2dd8_pass.dim.lz", "5f2dd.dim.lz", "5f2dd_pass.dim.lz", "5f2hd.dim.lz", "5f2hd_pass.dim.lz",
            "DSKA0000.DIM.lz", "DSKA0001.DIM.lz", "DSKA0009.DIM.lz", "DSKA0010.DIM.lz", "DSKA0012.DIM.lz",
            "DSKA0013.DIM.lz", "DSKA0017.DIM.lz", "DSKA0018.DIM.lz", "DSKA0020.DIM.lz", "DSKA0021.DIM.lz",
            "DSKA0024.DIM.lz", "DSKA0025.DIM.lz", "DSKA0028.DIM.lz", "DSKA0030.DIM.lz", "DSKA0035.DIM.lz",
            "DSKA0036.DIM.lz", "DSKA0037.DIM.lz", "DSKA0038.DIM.lz", "DSKA0039.DIM.lz", "DSKA0040.DIM.lz",
            "DSKA0041.DIM.lz", "DSKA0042.DIM.lz", "DSKA0043.DIM.lz", "DSKA0044.DIM.lz", "DSKA0045.DIM.lz",
            "DSKA0046.DIM.lz", "DSKA0047.DIM.lz", "DSKA0048.DIM.lz", "DSKA0049.DIM.lz", "DSKA0050.DIM.lz",
            "DSKA0051.DIM.lz", "DSKA0052.DIM.lz", "DSKA0053.DIM.lz", "DSKA0054.DIM.lz", "DSKA0055.DIM.lz",
            "DSKA0056.DIM.lz", "DSKA0057.DIM.lz", "DSKA0058.DIM.lz", "DSKA0059.DIM.lz", "DSKA0060.DIM.lz",
            "DSKA0061.DIM.lz", "DSKA0068.DIM.lz", "DSKA0069.DIM.lz", "DSKA0073.DIM.lz", "DSKA0074.DIM.lz",
            "DSKA0075.DIM.lz", "DSKA0076.DIM.lz", "DSKA0077.DIM.lz", "DSKA0078.DIM.lz", "DSKA0080.DIM.lz",
            "DSKA0081.DIM.lz", "DSKA0082.DIM.lz", "DSKA0083.DIM.lz", "DSKA0084.DIM.lz", "DSKA0085.DIM.lz",
            "DSKA0105.DIM.lz", "DSKA0106.DIM.lz", "DSKA0107.DIM.lz", "DSKA0108.DIM.lz", "DSKA0109.DIM.lz",
            "DSKA0110.DIM.lz", "DSKA0111.DIM.lz", "DSKA0112.DIM.lz", "DSKA0113.DIM.lz", "DSKA0114.DIM.lz",
            "DSKA0115.DIM.lz", "DSKA0116.DIM.lz", "DSKA0117.DIM.lz", "DSKA0120.DIM.lz", "DSKA0121.DIM.lz",
            "DSKA0122.DIM.lz", "DSKA0123.DIM.lz", "DSKA0124.DIM.lz", "DSKA0125.DIM.lz", "DSKA0126.DIM.lz",
            "DSKA0147.DIM.lz", "DSKA0148.DIM.lz", "DSKA0151.DIM.lz", "DSKA0153.DIM.lz", "DSKA0154.DIM.lz",
            "DSKA0157.DIM.lz", "DSKA0162.DIM.lz", "DSKA0163.DIM.lz", "DSKA0164.DIM.lz", "DSKA0166.DIM.lz",
            "DSKA0168.DIM.lz", "DSKA0169.DIM.lz", "DSKA0170.DIM.lz", "DSKA0173.DIM.lz", "DSKA0174.DIM.lz",
            "DSKA0175.DIM.lz", "DSKA0176.DIM.lz", "DSKA0177.DIM.lz", "DSKA0180.DIM.lz", "DSKA0181.DIM.lz",
            "DSKA0205.DIM.lz", "DSKA0206.DIM.lz", "DSKA0207.DIM.lz", "DSKA0209.DIM.lz", "DSKA0210.DIM.lz",
            "DSKA0211.DIM.lz", "DSKA0212.DIM.lz", "DSKA0216.DIM.lz", "DSKA0218.DIM.lz", "DSKA0219.DIM.lz",
            "DSKA0220.DIM.lz", "DSKA0222.DIM.lz", "DSKA0232.DIM.lz", "DSKA0245.DIM.lz", "DSKA0246.DIM.lz",
            "DSKA0262.DIM.lz", "DSKA0263.DIM.lz", "DSKA0264.DIM.lz", "DSKA0265.DIM.lz", "DSKA0266.DIM.lz",
            "DSKA0267.DIM.lz", "DSKA0268.DIM.lz", "DSKA0269.DIM.lz", "DSKA0270.DIM.lz", "DSKA0271.DIM.lz",
            "DSKA0272.DIM.lz", "DSKA0273.DIM.lz", "DSKA0280.DIM.lz", "DSKA0281.DIM.lz", "DSKA0282.DIM.lz",
            "DSKA0283.DIM.lz", "DSKA0284.DIM.lz", "DSKA0285.DIM.lz", "DSKA0287.DIM.lz", "DSKA0288.DIM.lz",
            "DSKA0289.DIM.lz", "DSKA0290.DIM.lz", "DSKA0299.DIM.lz", "DSKA0300.DIM.lz", "DSKA0301.DIM.lz",
            "DSKA0302.DIM.lz", "DSKA0303.DIM.lz", "DSKA0304.DIM.lz", "DSKA0305.DIM.lz", "DSKA0307.DIM.lz",
            "DSKA0308.DIM.lz", "DSKA0311.DIM.lz", "DSKA0314.DIM.lz", "DSKA0316.DIM.lz", "DSKA0317.DIM.lz",
            "DSKA0318.DIM.lz", "DSKA0319.DIM.lz", "DSKA0320.DIM.lz", "DSKA0322.DIM.lz", "md1dd8.dim.lz", "md1dd.dim.lz",
            "md1dd_fdformat_f200.dim.lz", "md1dd_fdformat_f205.dim.lz", "md2dd8.dim.lz", "md2dd.dim.lz",
            "md2dd_fdformat_f400.dim.lz", "md2dd_fdformat_f410.dim.lz", "md2dd_fdformat_f720.dim.lz",
            "md2dd_fdformat_f800.dim.lz", "md2dd_fdformat_f820.dim.lz", "md2dd_freedos_800s.dim.lz",
            "md2dd_maxiform_1640s.dim.lz", "md2dd_maxiform_840s.dim.lz", "md2dd_qcopy_1476s.dim.lz",
            "md2dd_qcopy_1600s.dim.lz", "md2dd_qcopy_1640s.dim.lz", "md2hd.dim.lz", "md2hd_fdformat_f144.dim.lz",
            "md2hd_fdformat_f148.dim.lz", "md2hd_maxiform_2788s.dim.lz", "mf2dd_alt.dim.lz", "mf2dd_alt_pass.dim.lz",
            "mf2dd.dim.lz", "mf2dd_fdformat_f800.dim.lz", "mf2dd_fdformat_f820.dim.lz", "mf2dd_freedos_1600s.dim.lz",
            "mf2dd_maxiform_1600s.dim.lz", "mf2dd_qcopy_1494s.dim.lz", "mf2dd_qcopy_1600s.dim.lz",
            "mf2dd_qcopy_1660s.dim.lz", "mf2ed.dim.lz", "mf2ed_pass.dim.lz", "mf2hd_alt.dim.lz",
            "mf2hd_alt_pass.dim.lz", "mf2hd.dim.lz", "mf2hd_dmf.dim.lz", "mf2hd_fdformat_f168.dim.lz",
            "mf2hd_fdformat_f16.dim.lz", "mf2hd_fdformat_f172.dim.lz", "mf2hd_freedos_3360s.dim.lz",
            "mf2hd_maxiform_3200s.dim.lz", "mf2hd_pass.dim.lz", "mf2hd_qcopy_2460s.dim.lz", "mf2hd_qcopy_2720s.dim.lz",
            "mf2hd_qcopy_2788s.dim.lz", "mf2hd_qcopy_2880s.dim.lz", "mf2hd_qcopy_2952s.dim.lz",
            "mf2hd_qcopy_2988s.dim.lz", "mf2hd_qcopy_3200s.dim.lz", "mf2hd_qcopy_3320s.dim.lz",
            "mf2hd_qcopy_3360s.dim.lz", "mf2hd_qcopy_3486s.dim.lz", "mf2hd_xdf_alt.dim.lz", "mf2hd_xdf_alt_pass.dim.lz"
        };
        readonly ulong[] _sectors =
        {
            // 5f1dd8.dim.lz
            336,

            // 5f1dd8_pass.dim.lz
            336,

            // 5f1dd.dim.lz
            378,

            // 5f1dd_pass.dim.lz
            378,

            // 5f2dd8.dim.lz
            672,

            // 5f2dd8_pass.dim.lz
            672,

            // 5f2dd.dim.lz
            756,

            // 5f2dd_pass.dim.lz
            756,

            // 5f2hd.dim.lz
            2460,

            // 5f2hd_pass.dim.lz
            2460,

            // DSKA0000.DIM.lz
            2880,

            // DSKA0001.DIM.lz
            1600,

            // DSKA0009.DIM.lz
            2880,

            // DSKA0010.DIM.lz
            1440,

            // DSKA0012.DIM.lz
            1600,

            // DSKA0013.DIM.lz
            1600,

            // DSKA0017.DIM.lz
            3040,

            // DSKA0018.DIM.lz
            1440,

            // DSKA0020.DIM.lz
            1440,

            // DSKA0021.DIM.lz
            3040,

            // DSKA0024.DIM.lz
            2880,

            // DSKA0025.DIM.lz
            1440,

            // DSKA0028.DIM.lz
            1440,

            // DSKA0030.DIM.lz
            1440,

            // DSKA0035.DIM.lz
            320,

            // DSKA0036.DIM.lz
            320,

            // DSKA0037.DIM.lz
            360,

            // DSKA0038.DIM.lz
            360,

            // DSKA0039.DIM.lz
            640,

            // DSKA0040.DIM.lz
            640,

            // DSKA0041.DIM.lz
            640,

            // DSKA0042.DIM.lz
            640,

            // DSKA0043.DIM.lz
            720,

            // DSKA0044.DIM.lz
            720,

            // DSKA0045.DIM.lz
            2400,

            // DSKA0046.DIM.lz
            2460,

            // DSKA0047.DIM.lz
            1280,

            // DSKA0048.DIM.lz
            1440,

            // DSKA0049.DIM.lz
            1476,

            // DSKA0050.DIM.lz
            1600,

            // DSKA0051.DIM.lz
            1640,

            // DSKA0052.DIM.lz
            2880,

            // DSKA0053.DIM.lz
            2952,

            // DSKA0054.DIM.lz
            3200,

            // DSKA0055.DIM.lz
            3280,

            // DSKA0056.DIM.lz
            3360,

            // DSKA0057.DIM.lz
            3444,

            // DSKA0058.DIM.lz
            3528,

            // DSKA0059.DIM.lz
            3528,

            // DSKA0060.DIM.lz
            3612,

            // DSKA0061.DIM.lz
            5120,

            // DSKA0068.DIM.lz
            720,

            // DSKA0069.DIM.lz
            1440,

            // DSKA0073.DIM.lz
            320,

            // DSKA0074.DIM.lz
            360,

            // DSKA0075.DIM.lz
            640,

            // DSKA0076.DIM.lz
            720,

            // DSKA0077.DIM.lz
            800,

            // DSKA0078.DIM.lz
            2400,

            // DSKA0080.DIM.lz
            1440,

            // DSKA0081.DIM.lz
            1600,

            // DSKA0082.DIM.lz
            2880,

            // DSKA0083.DIM.lz
            3024,

            // DSKA0084.DIM.lz
            3360,

            // DSKA0085.DIM.lz
            3528,

            // DSKA0105.DIM.lz
            400,

            // DSKA0106.DIM.lz
            420,

            // DSKA0107.DIM.lz
            800,

            // DSKA0108.DIM.lz
            840,

            // DSKA0109.DIM.lz
            1600,

            // DSKA0110.DIM.lz
            1640,

            // DSKA0111.DIM.lz
            2880,

            // DSKA0112.DIM.lz
            2952,

            // DSKA0113.DIM.lz
            1600,

            // DSKA0114.DIM.lz
            1640,

            // DSKA0115.DIM.lz
            2952,

            // DSKA0116.DIM.lz
            3200,

            // DSKA0117.DIM.lz
            3280,

            // DSKA0120.DIM.lz
            320,

            // DSKA0121.DIM.lz
            360,

            // DSKA0122.DIM.lz
            640,

            // DSKA0123.DIM.lz
            720,

            // DSKA0124.DIM.lz
            2400,

            // DSKA0125.DIM.lz
            1440,

            // DSKA0126.DIM.lz
            2880,

            // DSKA0147.DIM.lz
            320,

            // DSKA0148.DIM.lz
            640,

            // DSKA0151.DIM.lz
            360,

            // DSKA0153.DIM.lz
            720,

            // DSKA0154.DIM.lz
            800,

            // DSKA0157.DIM.lz
            720,

            // DSKA0162.DIM.lz
            320,

            // DSKA0163.DIM.lz
            720,

            // DSKA0164.DIM.lz
            840,

            // DSKA0166.DIM.lz
            1440,

            // DSKA0168.DIM.lz
            2400,

            // DSKA0169.DIM.lz
            2880,

            // DSKA0170.DIM.lz
            2880,

            // DSKA0173.DIM.lz
            720,

            // DSKA0174.DIM.lz
            1440,

            // DSKA0175.DIM.lz
            1600,

            // DSKA0176.DIM.lz
            1600,

            // DSKA0177.DIM.lz
            1600,

            // DSKA0180.DIM.lz
            3200,

            // DSKA0181.DIM.lz
            3360,

            // DSKA0205.DIM.lz
            1512,

            // DSKA0206.DIM.lz
            1440,

            // DSKA0207.DIM.lz
            3040,

            // DSKA0209.DIM.lz
            1600,

            // DSKA0210.DIM.lz
            1600,

            // DSKA0211.DIM.lz
            1440,

            // DSKA0212.DIM.lz
            1440,

            // DSKA0216.DIM.lz
            2880,

            // DSKA0218.DIM.lz
            5080,

            // DSKA0219.DIM.lz
            9144,

            // DSKA0220.DIM.lz
            13716,

            // DSKA0222.DIM.lz
            1600,

            // DSKA0232.DIM.lz
            630,

            // DSKA0245.DIM.lz
            1600,

            // DSKA0246.DIM.lz
            1600,

            // DSKA0262.DIM.lz
            1440,

            // DSKA0263.DIM.lz
            1600,

            // DSKA0264.DIM.lz
            1640,

            // DSKA0265.DIM.lz
            1680,

            // DSKA0266.DIM.lz
            2880,

            // DSKA0267.DIM.lz
            3040,

            // DSKA0268.DIM.lz
            3200,

            // DSKA0269.DIM.lz
            3280,

            // DSKA0270.DIM.lz
            3360,

            // DSKA0271.DIM.lz
            3360,

            // DSKA0272.DIM.lz
            3444,

            // DSKA0273.DIM.lz
            3528,

            // DSKA0280.DIM.lz
            360,

            // DSKA0281.DIM.lz
            400,

            // DSKA0282.DIM.lz
            640,

            // DSKA0283.DIM.lz
            720,

            // DSKA0284.DIM.lz
            800,

            // DSKA0285.DIM.lz
            840,

            // DSKA0287.DIM.lz
            1440,

            // DSKA0288.DIM.lz
            1512,

            // DSKA0289.DIM.lz
            1600,

            // DSKA0290.DIM.lz
            1640,

            // DSKA0299.DIM.lz
            320,

            // DSKA0300.DIM.lz
            360,

            // DSKA0301.DIM.lz
            640,

            // DSKA0302.DIM.lz
            720,

            // DSKA0303.DIM.lz
            2400,

            // DSKA0304.DIM.lz
            1440,

            // DSKA0305.DIM.lz
            2880,

            // DSKA0307.DIM.lz
            840,

            // DSKA0308.DIM.lz
            1600,

            // DSKA0311.DIM.lz
            3444,

            // DSKA0314.DIM.lz
            1440,

            // DSKA0316.DIM.lz
            2880,

            // DSKA0317.DIM.lz
            3360,

            // DSKA0318.DIM.lz
            3444,

            // DSKA0319.DIM.lz
            3360,

            // DSKA0320.DIM.lz
            3360,

            // DSKA0322.DIM.lz
            1404,

            // md1dd8.dim.lz
            320,

            // md1dd.dim.lz
            360,

            // md1dd_fdformat_f200.dim.lz
            400,

            // md1dd_fdformat_f205.dim.lz
            420,

            // md2dd8.dim.lz
            640,

            // md2dd.dim.lz
            720,

            // md2dd_fdformat_f400.dim.lz
            800,

            // md2dd_fdformat_f410.dim.lz
            840,

            // md2dd_fdformat_f720.dim.lz
            1440,

            // md2dd_fdformat_f800.dim.lz
            1600,

            // md2dd_fdformat_f820.dim.lz
            1640,

            // md2dd_freedos_800s.dim.lz
            800,

            // md2dd_maxiform_1640s.dim.lz
            1640,

            // md2dd_maxiform_840s.dim.lz
            840,

            // md2dd_qcopy_1476s.dim.lz
            1476,

            // md2dd_qcopy_1600s.dim.lz
            1600,

            // md2dd_qcopy_1640s.dim.lz
            1640,

            // md2hd.dim.lz
            2400,

            // md2hd_fdformat_f144.dim.lz
            2880,

            // md2hd_fdformat_f148.dim.lz
            2952,

            // md2hd_maxiform_2788s.dim.lz
            2788,

            // mf2dd_alt.dim.lz
            1476,

            // mf2dd_alt_pass.dim.lz
            1476,

            // mf2dd.dim.lz
            1440,

            // mf2dd_fdformat_f800.dim.lz
            1600,

            // mf2dd_fdformat_f820.dim.lz
            1640,

            // mf2dd_freedos_1600s.dim.lz
            1600,

            // mf2dd_maxiform_1600s.dim.lz
            1600,

            // mf2dd_qcopy_1494s.dim.lz
            1512,

            // mf2dd_qcopy_1600s.dim.lz
            1600,

            // mf2dd_qcopy_1660s.dim.lz
            1680,

            // mf2ed.dim.lz
            5904,

            // mf2ed_pass.dim.lz
            5904,

            // mf2hd_alt.dim.lz
            2952,

            // mf2hd_alt_pass.dim.lz
            2952,

            // mf2hd.dim.lz
            2880,

            // mf2hd_dmf.dim.lz
            3360,

            // mf2hd_fdformat_f168.dim.lz
            3360,

            // mf2hd_fdformat_f16.dim.lz
            3200,

            // mf2hd_fdformat_f172.dim.lz
            3444,

            // mf2hd_freedos_3360s.dim.lz
            3360,

            // mf2hd_maxiform_3200s.dim.lz
            3200,

            // mf2hd_pass.dim.lz
            2880,

            // mf2hd_qcopy_2460s.dim.lz
            2460,

            // mf2hd_qcopy_2720s.dim.lz
            2720,

            // mf2hd_qcopy_2788s.dim.lz
            2788,

            // mf2hd_qcopy_2880s.dim.lz
            2880,

            // mf2hd_qcopy_2952s.dim.lz
            2952,

            // mf2hd_qcopy_2988s.dim.lz
            3024,

            // mf2hd_qcopy_3200s.dim.lz
            3200,

            // mf2hd_qcopy_3320s.dim.lz
            3360,

            // mf2hd_qcopy_3360s.dim.lz
            3360,

            // mf2hd_qcopy_3486s.dim.lz
            3528,

            // mf2hd_xdf_alt.dim.lz
            3772,

            // mf2hd_xdf_alt_pass.dim.lz
            3772
        };

        readonly uint[] _sectorSize =
        {
            // 5f1dd8.dim.lz
            512,

            // 5f1dd8_pass.dim.lz
            512,

            // 5f1dd.dim.lz
            512,

            // 5f1dd_pass.dim.lz
            512,

            // 5f2dd8.dim.lz
            512,

            // 5f2dd8_pass.dim.lz
            512,

            // 5f2dd.dim.lz
            512,

            // 5f2dd_pass.dim.lz
            512,

            // 5f2hd.dim.lz
            512,

            // 5f2hd_pass.dim.lz
            512,

            // DSKA0000.DIM.lz
            512,

            // DSKA0001.DIM.lz
            512,

            // DSKA0009.DIM.lz
            512,

            // DSKA0010.DIM.lz
            512,

            // DSKA0012.DIM.lz
            512,

            // DSKA0013.DIM.lz
            512,

            // DSKA0017.DIM.lz
            512,

            // DSKA0018.DIM.lz
            512,

            // DSKA0020.DIM.lz
            512,

            // DSKA0021.DIM.lz
            512,

            // DSKA0024.DIM.lz
            512,

            // DSKA0025.DIM.lz
            512,

            // DSKA0028.DIM.lz
            512,

            // DSKA0030.DIM.lz
            512,

            // DSKA0035.DIM.lz
            512,

            // DSKA0036.DIM.lz
            512,

            // DSKA0037.DIM.lz
            512,

            // DSKA0038.DIM.lz
            512,

            // DSKA0039.DIM.lz
            512,

            // DSKA0040.DIM.lz
            512,

            // DSKA0041.DIM.lz
            512,

            // DSKA0042.DIM.lz
            512,

            // DSKA0043.DIM.lz
            512,

            // DSKA0044.DIM.lz
            512,

            // DSKA0045.DIM.lz
            512,

            // DSKA0046.DIM.lz
            512,

            // DSKA0047.DIM.lz
            512,

            // DSKA0048.DIM.lz
            512,

            // DSKA0049.DIM.lz
            512,

            // DSKA0050.DIM.lz
            512,

            // DSKA0051.DIM.lz
            512,

            // DSKA0052.DIM.lz
            512,

            // DSKA0053.DIM.lz
            512,

            // DSKA0054.DIM.lz
            512,

            // DSKA0055.DIM.lz
            512,

            // DSKA0056.DIM.lz
            512,

            // DSKA0057.DIM.lz
            512,

            // DSKA0058.DIM.lz
            512,

            // DSKA0059.DIM.lz
            512,

            // DSKA0060.DIM.lz
            512,

            // DSKA0061.DIM.lz
            512,

            // DSKA0068.DIM.lz
            512,

            // DSKA0069.DIM.lz
            512,

            // DSKA0073.DIM.lz
            512,

            // DSKA0074.DIM.lz
            512,

            // DSKA0075.DIM.lz
            512,

            // DSKA0076.DIM.lz
            512,

            // DSKA0077.DIM.lz
            512,

            // DSKA0078.DIM.lz
            512,

            // DSKA0080.DIM.lz
            512,

            // DSKA0081.DIM.lz
            512,

            // DSKA0082.DIM.lz
            512,

            // DSKA0083.DIM.lz
            512,

            // DSKA0084.DIM.lz
            512,

            // DSKA0085.DIM.lz
            512,

            // DSKA0105.DIM.lz
            512,

            // DSKA0106.DIM.lz
            512,

            // DSKA0107.DIM.lz
            512,

            // DSKA0108.DIM.lz
            512,

            // DSKA0109.DIM.lz
            512,

            // DSKA0110.DIM.lz
            512,

            // DSKA0111.DIM.lz
            512,

            // DSKA0112.DIM.lz
            512,

            // DSKA0113.DIM.lz
            512,

            // DSKA0114.DIM.lz
            512,

            // DSKA0115.DIM.lz
            512,

            // DSKA0116.DIM.lz
            512,

            // DSKA0117.DIM.lz
            512,

            // DSKA0120.DIM.lz
            512,

            // DSKA0121.DIM.lz
            512,

            // DSKA0122.DIM.lz
            512,

            // DSKA0123.DIM.lz
            512,

            // DSKA0124.DIM.lz
            512,

            // DSKA0125.DIM.lz
            512,

            // DSKA0126.DIM.lz
            512,

            // DSKA0147.DIM.lz
            512,

            // DSKA0148.DIM.lz
            512,

            // DSKA0151.DIM.lz
            512,

            // DSKA0153.DIM.lz
            512,

            // DSKA0154.DIM.lz
            512,

            // DSKA0157.DIM.lz
            512,

            // DSKA0162.DIM.lz
            512,

            // DSKA0163.DIM.lz
            512,

            // DSKA0164.DIM.lz
            512,

            // DSKA0166.DIM.lz
            512,

            // DSKA0168.DIM.lz
            512,

            // DSKA0169.DIM.lz
            512,

            // DSKA0170.DIM.lz
            512,

            // DSKA0173.DIM.lz
            512,

            // DSKA0174.DIM.lz
            512,

            // DSKA0175.DIM.lz
            512,

            // DSKA0176.DIM.lz
            512,

            // DSKA0177.DIM.lz
            512,

            // DSKA0180.DIM.lz
            512,

            // DSKA0181.DIM.lz
            512,

            // DSKA0205.DIM.lz
            512,

            // DSKA0206.DIM.lz
            512,

            // DSKA0207.DIM.lz
            512,

            // DSKA0209.DIM.lz
            512,

            // DSKA0210.DIM.lz
            512,

            // DSKA0211.DIM.lz
            512,

            // DSKA0212.DIM.lz
            512,

            // DSKA0216.DIM.lz
            512,

            // DSKA0218.DIM.lz
            512,

            // DSKA0219.DIM.lz
            512,

            // DSKA0220.DIM.lz
            512,

            // DSKA0222.DIM.lz
            512,

            // DSKA0232.DIM.lz
            512,

            // DSKA0245.DIM.lz
            512,

            // DSKA0246.DIM.lz
            512,

            // DSKA0262.DIM.lz
            512,

            // DSKA0263.DIM.lz
            512,

            // DSKA0264.DIM.lz
            512,

            // DSKA0265.DIM.lz
            512,

            // DSKA0266.DIM.lz
            512,

            // DSKA0267.DIM.lz
            512,

            // DSKA0268.DIM.lz
            512,

            // DSKA0269.DIM.lz
            512,

            // DSKA0270.DIM.lz
            512,

            // DSKA0271.DIM.lz
            512,

            // DSKA0272.DIM.lz
            512,

            // DSKA0273.DIM.lz
            512,

            // DSKA0280.DIM.lz
            512,

            // DSKA0281.DIM.lz
            512,

            // DSKA0282.DIM.lz
            512,

            // DSKA0283.DIM.lz
            512,

            // DSKA0284.DIM.lz
            512,

            // DSKA0285.DIM.lz
            512,

            // DSKA0287.DIM.lz
            512,

            // DSKA0288.DIM.lz
            512,

            // DSKA0289.DIM.lz
            512,

            // DSKA0290.DIM.lz
            512,

            // DSKA0299.DIM.lz
            512,

            // DSKA0300.DIM.lz
            512,

            // DSKA0301.DIM.lz
            512,

            // DSKA0302.DIM.lz
            512,

            // DSKA0303.DIM.lz
            512,

            // DSKA0304.DIM.lz
            512,

            // DSKA0305.DIM.lz
            512,

            // DSKA0307.DIM.lz
            512,

            // DSKA0308.DIM.lz
            512,

            // DSKA0311.DIM.lz
            512,

            // DSKA0314.DIM.lz
            512,

            // DSKA0316.DIM.lz
            512,

            // DSKA0317.DIM.lz
            512,

            // DSKA0318.DIM.lz
            512,

            // DSKA0319.DIM.lz
            512,

            // DSKA0320.DIM.lz
            512,

            // DSKA0322.DIM.lz
            512,

            // md1dd8.dim.lz
            512,

            // md1dd.dim.lz
            512,

            // md1dd_fdformat_f200.dim.lz
            512,

            // md1dd_fdformat_f205.dim.lz
            512,

            // md2dd8.dim.lz
            512,

            // md2dd.dim.lz
            512,

            // md2dd_fdformat_f400.dim.lz
            512,

            // md2dd_fdformat_f410.dim.lz
            512,

            // md2dd_fdformat_f720.dim.lz
            512,

            // md2dd_fdformat_f800.dim.lz
            512,

            // md2dd_fdformat_f820.dim.lz
            512,

            // md2dd_freedos_800s.dim.lz
            512,

            // md2dd_maxiform_1640s.dim.lz
            512,

            // md2dd_maxiform_840s.dim.lz
            512,

            // md2dd_qcopy_1476s.dim.lz
            512,

            // md2dd_qcopy_1600s.dim.lz
            512,

            // md2dd_qcopy_1640s.dim.lz
            512,

            // md2hd.dim.lz
            512,

            // md2hd_fdformat_f144.dim.lz
            512,

            // md2hd_fdformat_f148.dim.lz
            512,

            // md2hd_maxiform_2788s.dim.lz
            512,

            // mf2dd_alt.dim.lz
            512,

            // mf2dd_alt_pass.dim.lz
            512,

            // mf2dd.dim.lz
            512,

            // mf2dd_fdformat_f800.dim.lz
            512,

            // mf2dd_fdformat_f820.dim.lz
            512,

            // mf2dd_freedos_1600s.dim.lz
            512,

            // mf2dd_maxiform_1600s.dim.lz
            512,

            // mf2dd_qcopy_1494s.dim.lz
            512,

            // mf2dd_qcopy_1600s.dim.lz
            512,

            // mf2dd_qcopy_1660s.dim.lz
            512,

            // mf2ed.dim.lz
            512,

            // mf2ed_pass.dim.lz
            512,

            // mf2hd_alt.dim.lz
            512,

            // mf2hd_alt_pass.dim.lz
            512,

            // mf2hd.dim.lz
            512,

            // mf2hd_dmf.dim.lz
            512,

            // mf2hd_fdformat_f168.dim.lz
            512,

            // mf2hd_fdformat_f16.dim.lz
            512,

            // mf2hd_fdformat_f172.dim.lz
            512,

            // mf2hd_freedos_3360s.dim.lz
            512,

            // mf2hd_maxiform_3200s.dim.lz
            512,

            // mf2hd_pass.dim.lz
            512,

            // mf2hd_qcopy_2460s.dim.lz
            512,

            // mf2hd_qcopy_2720s.dim.lz
            512,

            // mf2hd_qcopy_2788s.dim.lz
            512,

            // mf2hd_qcopy_2880s.dim.lz
            512,

            // mf2hd_qcopy_2952s.dim.lz
            512,

            // mf2hd_qcopy_2988s.dim.lz
            512,

            // mf2hd_qcopy_3200s.dim.lz
            512,

            // mf2hd_qcopy_3320s.dim.lz
            512,

            // mf2hd_qcopy_3360s.dim.lz
            512,

            // mf2hd_qcopy_3486s.dim.lz
            512,

            // mf2hd_xdf_alt.dim.lz
            512,

            // mf2hd_xdf_alt_pass.dim.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // 5f1dd8.dim.lz
            MediaType.Unknown,

            // 5f1dd8_pass.dim.lz
            MediaType.Unknown,

            // 5f1dd.dim.lz
            MediaType.Unknown,

            // 5f1dd_pass.dim.lz
            MediaType.Unknown,

            // 5f2dd8.dim.lz
            MediaType.Unknown,

            // 5f2dd8_pass.dim.lz
            MediaType.Unknown,

            // 5f2dd.dim.lz
            MediaType.Unknown,

            // 5f2dd_pass.dim.lz
            MediaType.Unknown,

            // 5f2hd.dim.lz
            MediaType.Unknown,

            // 5f2hd_pass.dim.lz
            MediaType.Unknown,

            // DSKA0000.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0001.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0009.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0010.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0012.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0013.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0017.DIM.lz
            MediaType.XDF_525,

            // DSKA0018.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0020.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0021.DIM.lz
            MediaType.XDF_525,

            // DSKA0024.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0025.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0028.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0030.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0035.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0036.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0037.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0038.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0039.DIM.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0040.DIM.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0041.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0042.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0043.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0044.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0045.DIM.lz
            MediaType.DOS_525_HD,

            // DSKA0046.DIM.lz
            MediaType.Unknown,

            // DSKA0047.DIM.lz
            MediaType.DOS_35_DS_DD_8,

            // DSKA0048.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0049.DIM.lz
            MediaType.Unknown,

            // DSKA0050.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0051.DIM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0052.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0053.DIM.lz
            MediaType.Unknown,

            // DSKA0054.DIM.lz
            MediaType.Unknown,

            // DSKA0055.DIM.lz
            MediaType.Unknown,

            // DSKA0056.DIM.lz
            MediaType.DMF,

            // DSKA0057.DIM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0058.DIM.lz
            MediaType.Unknown,

            // DSKA0059.DIM.lz
            MediaType.Unknown,

            // DSKA0060.DIM.lz
            MediaType.Unknown,

            // DSKA0061.DIM.lz
            MediaType.Unknown,

            // DSKA0068.DIM.lz
            MediaType.DOS_35_SS_DD_9,

            // DSKA0069.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0073.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0074.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0075.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0076.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0077.DIM.lz
            MediaType.Unknown,

            // DSKA0078.DIM.lz
            MediaType.DOS_525_HD,

            // DSKA0080.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0081.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0082.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0083.DIM.lz
            MediaType.Unknown,

            // DSKA0084.DIM.lz
            MediaType.DMF,

            // DSKA0085.DIM.lz
            MediaType.Unknown,

            // DSKA0105.DIM.lz
            MediaType.Unknown,

            // DSKA0106.DIM.lz
            MediaType.Unknown,

            // DSKA0107.DIM.lz
            MediaType.Unknown,

            // DSKA0108.DIM.lz
            MediaType.Unknown,

            // DSKA0109.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0110.DIM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0111.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0112.DIM.lz
            MediaType.Unknown,

            // DSKA0113.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0114.DIM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0115.DIM.lz
            MediaType.Unknown,

            // DSKA0116.DIM.lz
            MediaType.Unknown,

            // DSKA0117.DIM.lz
            MediaType.Unknown,

            // DSKA0120.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0121.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0122.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0123.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0124.DIM.lz
            MediaType.DOS_525_HD,

            // DSKA0125.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0126.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0147.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0148.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0151.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0153.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0154.DIM.lz
            MediaType.Unknown,

            // DSKA0157.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0162.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0163.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0164.DIM.lz
            MediaType.Unknown,

            // DSKA0166.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0168.DIM.lz
            MediaType.DOS_525_HD,

            // DSKA0169.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0170.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0173.DIM.lz
            MediaType.DOS_35_SS_DD_9,

            // DSKA0174.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0175.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0176.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0177.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0180.DIM.lz
            MediaType.Unknown,

            // DSKA0181.DIM.lz
            MediaType.DMF,

            // DSKA0205.DIM.lz
            MediaType.Unknown,

            // DSKA0206.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0207.DIM.lz
            MediaType.XDF_525,

            // DSKA0209.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0210.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0211.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0212.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0216.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0218.DIM.lz
            MediaType.Unknown,

            // DSKA0219.DIM.lz
            MediaType.Unknown,

            // DSKA0220.DIM.lz
            MediaType.Unknown,

            // DSKA0222.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0232.DIM.lz
            MediaType.Unknown,

            // DSKA0245.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0246.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0262.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0263.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0264.DIM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0265.DIM.lz
            MediaType.Unknown,

            // DSKA0266.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0267.DIM.lz
            MediaType.XDF_525,

            // DSKA0268.DIM.lz
            MediaType.Unknown,

            // DSKA0269.DIM.lz
            MediaType.Unknown,

            // DSKA0270.DIM.lz
            MediaType.Unknown,

            // DSKA0271.DIM.lz
            MediaType.DMF,

            // DSKA0272.DIM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0273.DIM.lz
            MediaType.Unknown,

            // DSKA0280.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0281.DIM.lz
            MediaType.Unknown,

            // DSKA0282.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0283.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0284.DIM.lz
            MediaType.Unknown,

            // DSKA0285.DIM.lz
            MediaType.Unknown,

            // DSKA0287.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0288.DIM.lz
            MediaType.Unknown,

            // DSKA0289.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0290.DIM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0299.DIM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0300.DIM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0301.DIM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0302.DIM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0303.DIM.lz
            MediaType.DOS_525_HD,

            // DSKA0304.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0305.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0307.DIM.lz
            MediaType.Unknown,

            // DSKA0308.DIM.lz
            MediaType.CBM_35_DD,

            // DSKA0311.DIM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0314.DIM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0316.DIM.lz
            MediaType.DOS_35_HD,

            // DSKA0317.DIM.lz
            MediaType.DMF,

            // DSKA0318.DIM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0319.DIM.lz
            MediaType.DMF,

            // DSKA0320.DIM.lz
            MediaType.DMF,

            // DSKA0322.DIM.lz
            MediaType.Unknown,

            // md1dd8.dim.lz
            MediaType.DOS_525_SS_DD_8,

            // md1dd.dim.lz
            MediaType.DOS_525_SS_DD_9,

            // md1dd_fdformat_f200.dim.lz
            MediaType.Unknown,

            // md1dd_fdformat_f205.dim.lz
            MediaType.Unknown,

            // md2dd8.dim.lz
            MediaType.DOS_525_DS_DD_8,

            // md2dd.dim.lz
            MediaType.DOS_525_DS_DD_9,

            // md2dd_fdformat_f400.dim.lz
            MediaType.Unknown,

            // md2dd_fdformat_f410.dim.lz
            MediaType.Unknown,

            // md2dd_fdformat_f720.dim.lz
            MediaType.DOS_35_DS_DD_9,

            // md2dd_fdformat_f800.dim.lz
            MediaType.CBM_35_DD,

            // md2dd_fdformat_f820.dim.lz
            MediaType.FDFORMAT_35_DD,

            // md2dd_freedos_800s.dim.lz
            MediaType.Unknown,

            // md2dd_maxiform_1640s.dim.lz
            MediaType.FDFORMAT_35_DD,

            // md2dd_maxiform_840s.dim.lz
            MediaType.Unknown,

            // md2dd_qcopy_1476s.dim.lz
            MediaType.Unknown,

            // md2dd_qcopy_1600s.dim.lz
            MediaType.CBM_35_DD,

            // md2dd_qcopy_1640s.dim.lz
            MediaType.FDFORMAT_35_DD,

            // md2hd.dim.lz
            MediaType.DOS_525_HD,

            // md2hd_fdformat_f144.dim.lz
            MediaType.DOS_35_HD,

            // md2hd_fdformat_f148.dim.lz
            MediaType.Unknown,

            // md2hd_maxiform_2788s.dim.lz
            MediaType.FDFORMAT_525_HD,

            // mf2dd_alt.dim.lz
            MediaType.Unknown,

            // mf2dd_alt_pass.dim.lz
            MediaType.Unknown,

            // mf2dd.dim.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_fdformat_f800.dim.lz
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_f820.dim.lz
            MediaType.FDFORMAT_35_DD,

            // mf2dd_freedos_1600s.dim.lz
            MediaType.CBM_35_DD,

            // mf2dd_maxiform_1600s.dim.lz
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1494s.dim.lz
            MediaType.Unknown,

            // mf2dd_qcopy_1600s.dim.lz
            MediaType.CBM_35_DD,

            // mf2dd_qcopy_1660s.dim.lz
            MediaType.Unknown,

            // mf2ed.dim.lz
            MediaType.Unknown,

            // mf2ed_pass.dim.lz
            MediaType.Unknown,

            // mf2hd_alt.dim.lz
            MediaType.Unknown,

            // mf2hd_alt_pass.dim.lz
            MediaType.Unknown,

            // mf2hd.dim.lz
            MediaType.DOS_35_HD,

            // mf2hd_dmf.dim.lz
            MediaType.DMF,

            // mf2hd_fdformat_f168.dim.lz
            MediaType.DMF,

            // mf2hd_fdformat_f16.dim.lz
            MediaType.Unknown,

            // mf2hd_fdformat_f172.dim.lz
            MediaType.FDFORMAT_35_HD,

            // mf2hd_freedos_3360s.dim.lz
            MediaType.DMF,

            // mf2hd_maxiform_3200s.dim.lz
            MediaType.Unknown,

            // mf2hd_pass.dim.lz
            MediaType.DOS_35_HD,

            // mf2hd_qcopy_2460s.dim.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2720s.dim.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2788s.dim.lz
            MediaType.FDFORMAT_525_HD,

            // mf2hd_qcopy_2880s.dim.lz
            MediaType.DOS_35_HD,

            // mf2hd_qcopy_2952s.dim.lz
            MediaType.Unknown,

            // mf2hd_qcopy_2988s.dim.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3200s.dim.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3320s.dim.lz
            MediaType.Unknown,

            // mf2hd_qcopy_3360s.dim.lz
            MediaType.DMF,

            // mf2hd_qcopy_3486s.dim.lz
            MediaType.Unknown,

            // mf2hd_xdf_alt.dim.lz
            MediaType.Unknown,

            // mf2hd_xdf_alt_pass.dim.lz
            MediaType.Unknown
        };
        readonly string[] _md5S =
        {
            // 5f1dd8.dim.lz
            "c109e802e65365245dedd1737ec65c92",

            // 5f1dd8_pass.dim.lz
            "d6eb723ac53eb469f64d8df69efef3dd",

            // 5f1dd.dim.lz
            "a327c34060570e1a917eb1d88716a11a",

            // 5f1dd_pass.dim.lz
            "b9807f1c25bf472633e7e80fa947a4d1",

            // 5f2dd8.dim.lz
            "8b9e6662ef25a08d167f7ec4436efac8",

            // 5f2dd8_pass.dim.lz
            "532694cde41f1553587b65c528bc185b",

            // 5f2dd.dim.lz
            "a0b2aa16acaab9f521dff74ba93485ae",

            // 5f2dd_pass.dim.lz
            "934e3a0f07410d0f4750f2beb3ce48f1",

            // 5f2hd.dim.lz
            "78819708381987b3120fc777a5f08f2d",

            // 5f2hd_pass.dim.lz
            "37dbeabaf72384870284ccd102b85eb7",

            // DSKA0000.DIM.lz
            "e8bbbd22db87181974e12ba0227ea011",

            // DSKA0001.DIM.lz
            "9f5635f3df4d880a500910b0ad1ab535",

            // DSKA0009.DIM.lz
            "95ea232f59e44db374b994cfe7f1c07f",

            // DSKA0010.DIM.lz
            "9e2b01f4397db2a6c76e2bc267df37b3",

            // DSKA0012.DIM.lz
            "656002e6e620cb3b73c27f4c21d32edb",

            // DSKA0013.DIM.lz
            "1244cc2c101c66e6bb4ad5183b356b19",

            // DSKA0017.DIM.lz
            "8cad624afc06ab756f9800eba22ee886",

            // DSKA0018.DIM.lz
            "84cce7b4d8c8e21040163cd2d03a730c",

            // DSKA0020.DIM.lz
            "d236783dfd1dc29f350c51949b1e9e68",

            // DSKA0021.DIM.lz
            "6915f208cdda762eea2fe64ad754e72f",

            // DSKA0024.DIM.lz
            "2302991363cb3681cffdc4388915b51e",

            // DSKA0025.DIM.lz
            "4e4cafed1cc22ea72201169427e5e1b6",

            // DSKA0028.DIM.lz
            "1a4c7487382c98b7bc74623ddfb488e6",

            // DSKA0030.DIM.lz
            "af83d011608042d35021e39aa5e10b2f",

            // DSKA0035.DIM.lz
            "6642c1a32d2c58e93481d664974fc202",

            // DSKA0036.DIM.lz
            "846f01b8b60cb3c775bd66419e977926",

            // DSKA0037.DIM.lz
            "5101f89850dc28efbcfb7622086a9ddf",

            // DSKA0038.DIM.lz
            "8e570be2ed1f00ddea82e50a2d9c446a",

            // DSKA0039.DIM.lz
            "abba2a1ddd60a649047a9c44d94bbeae",

            // DSKA0040.DIM.lz
            "e3bc48bec81be5b35be73d41fdffd2ab",

            // DSKA0041.DIM.lz
            "43b5068af9d016d1432eb2e12d2b802a",

            // DSKA0042.DIM.lz
            "5bf2ad4dc300592604b6e32f8b8e2656",

            // DSKA0043.DIM.lz
            "cb9a832ca6a4097b8ccc30d2108e1f7d",

            // DSKA0044.DIM.lz
            "56d181a6bb8713e6b2854fe8887faab6",

            // DSKA0045.DIM.lz
            "41aef7cff26aefda1add8d49c5b962c2",

            // DSKA0046.DIM.lz
            "2437c5f089f1cba3866b36360b016f16",

            // DSKA0047.DIM.lz
            "bdaa8f17373b265830fdf3a06b794367",

            // DSKA0048.DIM.lz
            "629932c285478d0540ff7936aa008351",

            // DSKA0049.DIM.lz
            "7a2abef5d4701e2e49abb05af8d4da50",

            // DSKA0050.DIM.lz
            "e3507522c914264f44fb2c92c3170c09",

            // DSKA0051.DIM.lz
            "824fe65dbb1a42b6b94f05405ef984f2",

            // DSKA0052.DIM.lz
            "1a8c2e78e7132cf9ba5d6c2b75876be0",

            // DSKA0053.DIM.lz
            "936b20bb0966fe693b4d5e2353e24846",

            // DSKA0054.DIM.lz
            "803b01a0b440c2837d37c21308f30cd5",

            // DSKA0055.DIM.lz
            "aa0d31f914760cc4cde75479779ebed6",

            // DSKA0056.DIM.lz
            "31269ed6464302ae26d22b7c87bceb23",

            // DSKA0057.DIM.lz
            "5e413433c54f48978d281c6e66d1106e",

            // DSKA0058.DIM.lz
            "a7688d6be942272ce866736e6007bc46",

            // DSKA0059.DIM.lz
            "24a7459d080cea3a60d131b8fd7dc5d1",

            // DSKA0060.DIM.lz
            "ef0c3da4749da2f79d7d623d9b6f3d4d",

            // DSKA0061.DIM.lz
            "5231d2e8a99ba5f8dfd16ca1a05f40cd",

            // DSKA0068.DIM.lz
            "8f91482c56161ecbf5d86f42b03b9636",

            // DSKA0069.DIM.lz
            "5fc19ca552b6db957061e9a1750394d2",

            // DSKA0073.DIM.lz
            "a33b46f042b78fe3d0b3c5dbb3908a93",

            // DSKA0074.DIM.lz
            "565d3c001cbb532154aa5d3c65b2439c",

            // DSKA0075.DIM.lz
            "e60442c3ebd72c99bdd7545fdba59613",

            // DSKA0076.DIM.lz
            "058a33a129539285c9b64010496af52f",

            // DSKA0077.DIM.lz
            "0726ecbc38965d30a6222c3e74cd1aa3",

            // DSKA0078.DIM.lz
            "c9a193837db7d8a5eb025eb41e8a76d7",

            // DSKA0080.DIM.lz
            "c38d69ac88520f14fcc6d6ced22b065d",

            // DSKA0081.DIM.lz
            "91d51964e1e64ef3f6f622fa19aa833c",

            // DSKA0082.DIM.lz
            "db36d9651c952ff679ec33223c8db2d3",

            // DSKA0083.DIM.lz
            "952f33314fb930c2d02ef4604585c0e6",

            // DSKA0084.DIM.lz
            "1207a1cc7ff73d4f74c8984b4e7db33f",

            // DSKA0085.DIM.lz
            "53dfcaceed8203ee629fc7fe520e1217",

            // DSKA0105.DIM.lz
            "d40a99cb549fcfb26fcf9ef01b5dfca7",

            // DSKA0106.DIM.lz
            "6433f8fbf8dda1e307b15a4203c1a4e6",

            // DSKA0107.DIM.lz
            "126dfd25363c076727dfaab03955c931",

            // DSKA0108.DIM.lz
            "386763ae9afde1a0a19eb4a54ba462aa",

            // DSKA0109.DIM.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0110.DIM.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0111.DIM.lz
            "f01541de322c8d6d7321084d7a245e7b",

            // DSKA0112.DIM.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0113.DIM.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0114.DIM.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0115.DIM.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0116.DIM.lz
            "6631b66fdfd89319323771c41334c7ba",

            // DSKA0117.DIM.lz
            "56471a253f4d6803b634e2bbff6c0931",

            // DSKA0120.DIM.lz
            "7d36aee5a3071ff75b979f3acb649c40",

            // DSKA0121.DIM.lz
            "0ccb62039363ab544c69eca229a17fae",

            // DSKA0122.DIM.lz
            "7851d31fad9302ff45d3ded4fba25387",

            // DSKA0123.DIM.lz
            "915b08c82591e8488320e001b7303b6d",

            // DSKA0124.DIM.lz
            "5e5ea6fe9adf842221fdc60e56630405",

            // DSKA0125.DIM.lz
            "a22e254f7e3526ec30dc4915a19fcb52",

            // DSKA0126.DIM.lz
            "ddc6c1200c60e9f7796280f50c2e5283",

            // DSKA0147.DIM.lz
            "6efa72a33021d5051546c3e0dd4c3c09",

            // DSKA0148.DIM.lz
            "8b5acfd14818ff9556d3d81361ce4862",

            // DSKA0151.DIM.lz
            "298c377de52947c472a85d281b6d3d4d",

            // DSKA0153.DIM.lz
            "32975e1a2d10a360331de84682371277",

            // DSKA0154.DIM.lz
            "a5dc382d75ec46434b313e289c281d8c",

            // DSKA0157.DIM.lz
            "3a7f25fa38019109e89051993076063a",

            // DSKA0162.DIM.lz
            "e63014a4299f52f22e6e2c9609f51979",

            // DSKA0163.DIM.lz
            "be05d1ff10ef8b2220546c4db962ac9e",

            // DSKA0164.DIM.lz
            "e01d813dd6c3a49428520df40d63cadd",

            // DSKA0166.DIM.lz
            "1c8b03a8550ed3e70e1c78316aa445aa",

            // DSKA0168.DIM.lz
            "0bdf9130c07bb5d558a4705249f949d0",

            // DSKA0169.DIM.lz
            "2dafeddaa99e7dc0db5ef69e128f9c8e",

            // DSKA0170.DIM.lz
            "0c043ceba489ef80c1b7f58534af12f5",

            // DSKA0173.DIM.lz
            "028769dc0abefab1740cc309432588b6",

            // DSKA0174.DIM.lz
            "152023525154b45ab26687190bac94db",

            // DSKA0175.DIM.lz
            "db38ecd93f28dd065927fed21917eed5",

            // DSKA0176.DIM.lz
            "ca53f9cc4dcd04d06f5c4c3df09195ab",

            // DSKA0177.DIM.lz
            "fde94075cb3fd1c52af32062b0251af0",

            // DSKA0180.DIM.lz
            "f206c0caa4e0eda37233ab6e89ab5493",

            // DSKA0181.DIM.lz
            "4375fe3d7e50a5044b4850d8542363fb",

            // DSKA0205.DIM.lz
            "d3106f2c989a0afcf97b63b051be8312",

            // DSKA0206.DIM.lz
            "8245ddd644583bd78ac0638133c89824",

            // DSKA0207.DIM.lz
            "33c51a3d6f13cfedb5f08bf4c3cba7b9",

            // DSKA0209.DIM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0210.DIM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0211.DIM.lz
            "647f14749f59be471aac04a71a079a64",

            // DSKA0212.DIM.lz
            "517cdd5e42a4673f733d1aedfb46770f",

            // DSKA0216.DIM.lz
            "40199611e6e75bbc37ad6c52a5b77eae",

            // DSKA0218.DIM.lz
            "fabacd63bd25f4c3db71523c21242bfb",

            // DSKA0219.DIM.lz
            "0d1a1dfa4482422ff11fea76f8cef3a9",

            // DSKA0220.DIM.lz
            "a6a67106457a20b46d05f2d9b27244f1",

            // DSKA0222.DIM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0232.DIM.lz
            "53a50481d90228f527b72f058de257da",

            // DSKA0245.DIM.lz
            "0d71b4952dadbfb1061acc1f4640c787",

            // DSKA0246.DIM.lz
            "af7ac6b5b9d2d57dad22dbb64ef7de38",

            // DSKA0262.DIM.lz
            "5ac0a9fc7337f761098f816359b0f6f7",

            // DSKA0263.DIM.lz
            "1ea6ec8e663218b1372048f6e25795b5",

            // DSKA0264.DIM.lz
            "77a1167b1b9043496e32b8578cde0ff0",

            // DSKA0265.DIM.lz
            "2b2c891ef5edee8518a1ae2ed3ab71a0",

            // DSKA0266.DIM.lz
            "32c044c5c2b0bd13806149a759c14935",

            // DSKA0267.DIM.lz
            "8752095abc13dba3f3467669da333891",

            // DSKA0268.DIM.lz
            "aece7cd34bbba3e75307fa70404d9d30",

            // DSKA0269.DIM.lz
            "5289afb16a6e4a33213e3bcca56c6230",

            // DSKA0270.DIM.lz
            "092308e5df684702dd0ec393b6d3563a",

            // DSKA0271.DIM.lz
            "b96596711f4d2ee85dfda0fe3b9f26c3",

            // DSKA0272.DIM.lz
            "a4f461af7fda5e93a7ab63fcbb7e7683",

            // DSKA0273.DIM.lz
            "963f3aa8d4468d4373054f842d0e2245",

            // DSKA0280.DIM.lz
            "4feeaf4b4ee5dad85db727fbbda4b6d1",

            // DSKA0281.DIM.lz
            "3c77ca681df78e4cd7baa162aa9b0859",

            // DSKA0282.DIM.lz
            "51da1f86c49657ffdb367bb2ddeb7990",

            // DSKA0283.DIM.lz
            "b81a4987f89936630b8ebc62e4bbce6e",

            // DSKA0284.DIM.lz
            "f76f92dd326c99c5efad5ee58daf72e1",

            // DSKA0285.DIM.lz
            "b6f2c10e42908e334025bc4ffd81e771",

            // DSKA0287.DIM.lz
            "f2f409ea2a62a7866fd2777cc4fc9739",

            // DSKA0288.DIM.lz
            "be89d2aab865a1217a3dda86e99bed97",

            // DSKA0289.DIM.lz
            "30a93f30dd4485c6fc037fe0775d3fc7",

            // DSKA0290.DIM.lz
            "e0caf02cce5597c98313bcc480366ec7",

            // DSKA0299.DIM.lz
            "39bf5a98bcb2185d855ac06378febcfa",

            // DSKA0300.DIM.lz
            "dc20055b6e6fd6f8e1114d4be2effeed",

            // DSKA0301.DIM.lz
            "56af9256cf71d5aac5fd5d363674bc49",

            // DSKA0302.DIM.lz
            "bbba1e2d1418e05c3a4e7b4d585d160b",

            // DSKA0303.DIM.lz
            "bca3a045e81617f7f5ebb5a8818eac47",

            // DSKA0304.DIM.lz
            "a296663cb8e75e94603221352f29cfff",

            // DSKA0305.DIM.lz
            "ecda36ebf0e1100233cb0ec722c18583",

            // DSKA0307.DIM.lz
            "cef2f4fe9b1a32d5c0544f814e634264",

            // DSKA0308.DIM.lz
            "bbe58e26b8f8f822cd3edfd37a4e4924",

            // DSKA0311.DIM.lz
            "b9b6ebdf711364c979de7cf70c3a438a",

            // DSKA0314.DIM.lz
            "d37424f367f545acbb397f2bed766843",

            // DSKA0316.DIM.lz
            "9963dd6f19ce6bd56eabeccdfbbd821a",

            // DSKA0317.DIM.lz
            "acf6604559ae8217f7869823e2429024",

            // DSKA0318.DIM.lz
            "23bf2139cdfdc4c16db058fd31ea6481",

            // DSKA0319.DIM.lz
            "fa26adda0415f02057b113ad29c80c8d",

            // DSKA0320.DIM.lz
            "4f2a8d036fefd6c6c88d99eda3aa12b7",

            // DSKA0322.DIM.lz
            "1f6a23974b29d525706a2b0228325656",

            // md1dd8.dim.lz
            "d81f5cb64fd0b99f138eab34110bbc3c",

            // md1dd.dim.lz
            "a89006a75d13bee9202d1d6e52721ccb",

            // md1dd_fdformat_f200.dim.lz
            "e1ad4a022778d7a0b24a93d8e68a59dc",

            // md1dd_fdformat_f205.dim.lz
            "56a95fcf1d6f5c3108a17207b53ec07c",

            // md2dd8.dim.lz
            "beef1cdb004dc69391d6b3d508988b95",

            // md2dd.dim.lz
            "6213897b7dbf263f12abf76901d43862",

            // md2dd_fdformat_f400.dim.lz
            "0aef12c906b744101b932d799ca88a78",

            // md2dd_fdformat_f410.dim.lz
            "e7367df9998de0030a97b5131d1bed20",

            // md2dd_fdformat_f720.dim.lz
            "1c36b819cfe355c11360bc120c9216fe",

            // md2dd_fdformat_f800.dim.lz
            "25114403c11e337480e2afc4e6e32108",

            // md2dd_fdformat_f820.dim.lz
            "3d7760ddaa55cd258057773d15106b78",

            // md2dd_freedos_800s.dim.lz
            "29054ef703394ee3b35e849468a412ba",

            // md2dd_maxiform_1640s.dim.lz
            "c91e852828c2aeee2fc94a6adbeed0ae",

            // md2dd_maxiform_840s.dim.lz
            "efb6cfe53a6770f0ae388cb2c7f46264",

            // md2dd_qcopy_1476s.dim.lz
            "6116f7c1397cadd55ba8d79c2aadc9dd",

            // md2dd_qcopy_1600s.dim.lz
            "93100f8d86e5d0d0e6340f59c52a5e0d",

            // md2dd_qcopy_1640s.dim.lz
            "cf7b7d43aa70863bedcc4a8432a5af67",

            // md2hd.dim.lz
            "02259cd5fbcc20f8484aa6bece7a37c6",

            // md2hd_fdformat_f144.dim.lz
            "073a172879a71339ef4b00ebb47b67fc",

            // md2hd_fdformat_f148.dim.lz
            "d9890897130d0fc1eee3dbf4d9b0440f",

            // md2hd_maxiform_2788s.dim.lz
            "09ca721aa883d5bbaa422c7943b0782c",

            // mf2dd_alt.dim.lz
            "259ff90e41e60682d948dd7d6af89735",

            // mf2dd_alt_pass.dim.lz
            "b40f8273fa7492bfe71c3d743269b97c",

            // mf2dd.dim.lz
            "9827ba1b3e9cac41263caabd862e78f9",

            // mf2dd_fdformat_f800.dim.lz
            "67d299c6e83f3f0fbcb8faa9ffa422c1",

            // mf2dd_fdformat_f820.dim.lz
            "81d3bfec7b201f6a4503eb24c4394d4a",

            // mf2dd_freedos_1600s.dim.lz
            "d07f7ffaee89742c6477aaaf94eb5715",

            // mf2dd_maxiform_1600s.dim.lz
            "56af87802a9852e6e01e08d544740816",

            // mf2dd_qcopy_1494s.dim.lz
            "34b7b99ef6fba2235eedbd8ae406d7d3",

            // mf2dd_qcopy_1600s.dim.lz
            "d9db52d992a76bf3bbc626ff844215a5",

            // mf2dd_qcopy_1660s.dim.lz
            "3b74e367926181152c3499de8dd9b914",

            // mf2ed.dim.lz
            "82825116ffe6d68b4d920ad4875bd709",

            // mf2ed_pass.dim.lz
            "e7cdd1123b08eac4e9571825b1f6172f",

            // mf2hd_alt.dim.lz
            "3b16537076c5517306dc672f8f1e376e",

            // mf2hd_alt_pass.dim.lz
            "022893d7766205894fca41bcde3c9f6c",

            // mf2hd.dim.lz
            "1d32a686b7675c7a4f88c15522738432",

            // mf2hd_dmf.dim.lz
            "084d4d75f5e780cb9ec66a2fa784c371",

            // mf2hd_fdformat_f168.dim.lz
            "1e06f21a1c11ea3347212da115bca08f",

            // mf2hd_fdformat_f16.dim.lz
            "8eb8cb310feaf03c69fffd4f6e729847",

            // mf2hd_fdformat_f172.dim.lz
            "3fc3a03d049416d81f81cc3b9ea8e5de",

            // mf2hd_freedos_3360s.dim.lz
            "2bfd2e0a81bad704f8fc7758358cfcca",

            // mf2hd_maxiform_3200s.dim.lz
            "3c4becd695ed25866d39966a9a93c2d9",

            // mf2hd_pass.dim.lz
            "1d32a686b7675c7a4f88c15522738432",

            // mf2hd_qcopy_2460s.dim.lz
            "72282e11f7d91bf9c090b550fabfe80d",

            // mf2hd_qcopy_2720s.dim.lz
            "457c1126dc7f36bbbabe9e17e90372e3",

            // mf2hd_qcopy_2788s.dim.lz
            "852181d5913c6f290872c66bbe992314",

            // mf2hd_qcopy_2880s.dim.lz
            "2980cc32504c945598dc50f1db576994",

            // mf2hd_qcopy_2952s.dim.lz
            "c1c58d74fffb3656dd7f60f74ae8a629",

            // mf2hd_qcopy_2988s.dim.lz
            "67391c3750f17a806503be3f9d514b1f",

            // mf2hd_qcopy_3200s.dim.lz
            "e45d41a61fbe48f328c995fcc10a5548",

            // mf2hd_qcopy_3320s.dim.lz
            "c7764476489072dd053d5ec878171423",

            // mf2hd_qcopy_3360s.dim.lz
            "15f71b92bd72aba5d80bf70eca4d5b1e",

            // mf2hd_qcopy_3486s.dim.lz
            "f725bc714c3204e835e23c726ce77b89",

            // mf2hd_xdf_alt.dim.lz
            "02d7c237c6ac1fbcd2fbbfb45c5fb767",

            // mf2hd_xdf_alt_pass.dim.lz
            "99f83e846c5106dd4992646726e91636"
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Disk IMage Archiver");

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

                    var  image  = new RayDim();
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
        const uint SECTORS_TO_READ = 256;

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

                    var   image       = new RayDim();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
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