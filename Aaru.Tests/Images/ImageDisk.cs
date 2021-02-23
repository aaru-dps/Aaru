// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageDisk.cs
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
    public class ImageDisk
    {
        readonly string[] _testFiles =
        {
            "CPM1_ALL.IMD.lz", "DSKA0000.IMD.lz", "DSKA0001.IMD.lz", "DSKA0002.IMD.lz", "DSKA0003.IMD.lz",
            "DSKA0004.IMD.lz", "DSKA0006.IMD.lz", "DSKA0009.IMD.lz", "DSKA0010.IMD.lz", "DSKA0011.IMD.lz",
            "DSKA0012.IMD.lz", "DSKA0013.IMD.lz", "DSKA0017.IMD.lz", "DSKA0018.IMD.lz", "DSKA0019.IMD.lz",
            "DSKA0020.IMD.lz", "DSKA0021.IMD.lz", "DSKA0022.IMD.lz", "DSKA0023.IMD.lz", "DSKA0024.IMD.lz",
            "DSKA0025.IMD.lz", "DSKA0026.IMD.lz", "DSKA0027.IMD.lz", "DSKA0028.IMD.lz", "DSKA0029.IMD.lz",
            "DSKA0030.IMD.lz", "DSKA0031.IMD.lz", "DSKA0032.IMD.lz", "DSKA0033.IMD.lz", "DSKA0034.IMD.lz",
            "DSKA0035.IMD.lz", "DSKA0036.IMD.lz", "DSKA0037.IMD.lz", "DSKA0038.IMD.lz", "DSKA0039.IMD.lz",
            "DSKA0040.IMD.lz", "DSKA0041.IMD.lz", "DSKA0042.IMD.lz", "DSKA0043.IMD.lz", "DSKA0044.IMD.lz",
            "DSKA0045.IMD.lz", "DSKA0046.IMD.lz", "DSKA0047.IMD.lz", "DSKA0048.IMD.lz", "DSKA0049.IMD.lz",
            "DSKA0050.IMD.lz", "DSKA0051.IMD.lz", "DSKA0052.IMD.lz", "DSKA0053.IMD.lz", "DSKA0054.IMD.lz",
            "DSKA0055.IMD.lz", "DSKA0057.IMD.lz", "DSKA0058.IMD.lz", "DSKA0059.IMD.lz", "DSKA0060.IMD.lz",
            "DSKA0061.IMD.lz", "DSKA0063.IMD.lz", "DSKA0064.IMD.lz", "DSKA0065.IMD.lz", "DSKA0066.IMD.lz",
            "DSKA0067.IMD.lz", "DSKA0068.IMD.lz", "DSKA0069.IMD.lz", "DSKA0070.IMD.lz", "DSKA0073.IMD.lz",
            "DSKA0074.IMD.lz", "DSKA0075.IMD.lz", "DSKA0076.IMD.lz", "DSKA0077.IMD.lz", "DSKA0078.IMD.lz",
            "DSKA0080.IMD.lz", "DSKA0081.IMD.lz", "DSKA0082.IMD.lz", "DSKA0083.IMD.lz", "DSKA0084.IMD.lz",
            "DSKA0085.IMD.lz", "DSKA0086.IMD.lz", "DSKA0089.IMD.lz", "DSKA0090.IMD.lz", "DSKA0091.IMD.lz",
            "DSKA0092.IMD.lz", "DSKA0093.IMD.lz", "DSKA0094.IMD.lz", "DSKA0097.IMD.lz", "DSKA0098.IMD.lz",
            "DSKA0099.IMD.lz", "DSKA0101.IMD.lz", "DSKA0103.IMD.lz", "DSKA0105.IMD.lz", "DSKA0106.IMD.lz",
            "DSKA0107.IMD.lz", "DSKA0108.IMD.lz", "DSKA0109.IMD.lz", "DSKA0110.IMD.lz", "DSKA0111.IMD.lz",
            "DSKA0112.IMD.lz", "DSKA0113.IMD.lz", "DSKA0114.IMD.lz", "DSKA0115.IMD.lz", "DSKA0116.IMD.lz",
            "DSKA0117.IMD.lz", "DSKA0120.IMD.lz", "DSKA0121.IMD.lz", "DSKA0122.IMD.lz", "DSKA0123.IMD.lz",
            "DSKA0124.IMD.lz", "DSKA0125.IMD.lz", "DSKA0126.IMD.lz", "DSKA0147.IMD.lz", "DSKA0148.IMD.lz",
            "DSKA0149.IMD.lz", "DSKA0150.IMD.lz", "DSKA0151.IMD.lz", "DSKA0153.IMD.lz", "DSKA0154.IMD.lz",
            "DSKA0155.IMD.lz", "DSKA0157.IMD.lz", "DSKA0158.IMD.lz", "DSKA0159.IMD.lz", "DSKA0160.IMD.lz",
            "DSKA0162.IMD.lz", "DSKA0163.IMD.lz", "DSKA0164.IMD.lz", "DSKA0166.IMD.lz", "DSKA0167.IMD.lz",
            "DSKA0168.IMD.lz", "DSKA0169.IMD.lz", "DSKA0170.IMD.lz", "DSKA0171.IMD.lz", "DSKA0173.IMD.lz",
            "DSKA0174.IMD.lz", "DSKA0175.IMD.lz", "DSKA0176.IMD.lz", "DSKA0177.IMD.lz", "DSKA0180.IMD.lz",
            "DSKA0181.IMD.lz", "DSKA0182.IMD.lz", "DSKA0183.IMD.lz", "DSKA0184.IMD.lz", "DSKA0185.IMD.lz",
            "DSKA0186.IMD.lz", "DSKA0191.IMD.lz", "DSKA0192.IMD.lz", "DSKA0194.IMD.lz", "DSKA0196.IMD.lz",
            "DSKA0197.IMD.lz", "DSKA0198.IMD.lz", "DSKA0199.IMD.lz", "DSKA0200.IMD.lz", "DSKA0201.IMD.lz",
            "DSKA0202.IMD.lz", "DSKA0203.IMD.lz", "DSKA0204.IMD.lz", "DSKA0205.IMD.lz", "DSKA0206.IMD.lz",
            "DSKA0207.IMD.lz", "DSKA0208.IMD.lz", "DSKA0209.IMD.lz", "DSKA0210.IMD.lz", "DSKA0211.IMD.lz",
            "DSKA0212.IMD.lz", "DSKA0213.IMD.lz", "DSKA0214.IMD.lz", "DSKA0215.IMD.lz", "DSKA0216.IMD.lz",
            "DSKA0218.IMD.lz", "DSKA0219.IMD.lz", "DSKA0220.IMD.lz", "DSKA0221.IMD.lz", "DSKA0222.IMD.lz",
            "DSKA0223.IMD.lz", "DSKA0224.IMD.lz", "DSKA0225.IMD.lz", "DSKA0226.IMD.lz", "DSKA0227.IMD.lz",
            "DSKA0228.IMD.lz", "DSKA0232.IMD.lz", "DSKA0233.IMD.lz", "DSKA0234.IMD.lz", "DSKA0235.IMD.lz",
            "DSKA0236.IMD.lz", "DSKA0238.IMD.lz", "DSKA0240.IMD.lz", "DSKA0241.IMD.lz", "DSKA0242.IMD.lz",
            "DSKA0243.IMD.lz", "DSKA0244.IMD.lz", "DSKA0245.IMD.lz", "DSKA0246.IMD.lz", "DSKA0247.IMD.lz",
            "DSKA0248.IMD.lz", "DSKA0250.IMD.lz", "DSKA0251.IMD.lz", "DSKA0252.IMD.lz", "DSKA0253.IMD.lz",
            "DSKA0254.IMD.lz", "DSKA0255.IMD.lz", "DSKA0258.IMD.lz", "DSKA0262.IMD.lz", "DSKA0263.IMD.lz",
            "DSKA0264.IMD.lz", "DSKA0265.IMD.lz", "DSKA0266.IMD.lz", "DSKA0267.IMD.lz", "DSKA0268.IMD.lz",
            "DSKA0269.IMD.lz", "DSKA0270.IMD.lz", "DSKA0271.IMD.lz", "DSKA0272.IMD.lz", "DSKA0273.IMD.lz",
            "DSKA0280.IMD.lz", "DSKA0281.IMD.lz", "DSKA0282.IMD.lz", "DSKA0283.IMD.lz", "DSKA0284.IMD.lz",
            "DSKA0285.IMD.lz", "DSKA0287.IMD.lz", "DSKA0288.IMD.lz", "DSKA0289.IMD.lz", "DSKA0290.IMD.lz",
            "DSKA0291.IMD.lz", "DSKA0299.IMD.lz", "DSKA0300.IMD.lz", "DSKA0301.IMD.lz", "DSKA0302.IMD.lz",
            "DSKA0303.IMD.lz", "DSKA0304.IMD.lz", "DSKA0305.IMD.lz", "DSKA0307.IMD.lz", "DSKA0308.IMD.lz",
            "DSKA0311.IMD.lz", "DSKA0314.IMD.lz", "DSKA0316.IMD.lz", "DSKA0317.IMD.lz", "DSKA0318.IMD.lz",
            "DSKA0319.IMD.lz", "DSKA0320.IMD.lz", "DSKA0322.IMD.lz", "md1dd_rx01.imd.lz", "md1qd_rx50.imd.lz",
            "md2hd_nec.imd.lz", "mf2dd_2mgui.imd.lz", "mf2dd_2m.imd.lz", "mf2dd_fdformat_800.imd.lz",
            "mf2dd_fdformat_820.imd.lz", "mf2dd_freedos.imd.lz", "mf2dd.imd.lz", "mf2hd_2mgui.imd.lz",
            "mf2hd_2m.imd.lz", "mf2hd_fdformat_168.imd.lz", "mf2hd_fdformat_172.imd.lz", "mf2hd_freedos.imd.lz",
            "mf2hd.imd.lz", "mf2hd_xdf.imd.lz"
        };

        readonly ulong[] _sectors =
        {
            // CPM1_ALL.IMD.lz
            1280,

            // DSKA0000.IMD.lz
            2880,

            // DSKA0001.IMD.lz
            1600,

            // DSKA0002.IMD.lz
            1600,

            // DSKA0003.IMD.lz
            800,

            // DSKA0004.IMD.lz
            1600,

            // DSKA0006.IMD.lz
            360,

            // DSKA0009.IMD.lz
            2880,

            // DSKA0010.IMD.lz
            1440,

            // DSKA0011.IMD.lz
            1280,

            // DSKA0012.IMD.lz
            1600,

            // DSKA0013.IMD.lz
            1600,

            // DSKA0017.IMD.lz
            3200,

            // DSKA0018.IMD.lz
            1600,

            // DSKA0019.IMD.lz
            90,

            // DSKA0020.IMD.lz
            1600,

            // DSKA0021.IMD.lz
            3200,

            // DSKA0022.IMD.lz
            2560,

            // DSKA0023.IMD.lz
            1600,

            // DSKA0024.IMD.lz
            2880,

            // DSKA0025.IMD.lz
            1440,

            // DSKA0026.IMD.lz
            800,

            // DSKA0027.IMD.lz
            960,

            // DSKA0028.IMD.lz
            1440,

            // DSKA0029.IMD.lz
            960,

            // DSKA0030.IMD.lz
            1440,

            // DSKA0031.IMD.lz
            640,

            // DSKA0032.IMD.lz
            640,

            // DSKA0033.IMD.lz
            1280,

            // DSKA0034.IMD.lz
            1280,

            // DSKA0035.IMD.lz
            320,

            // DSKA0036.IMD.lz
            320,

            // DSKA0037.IMD.lz
            360,

            // DSKA0038.IMD.lz
            360,

            // DSKA0039.IMD.lz
            640,

            // DSKA0040.IMD.lz
            640,

            // DSKA0041.IMD.lz
            640,

            // DSKA0042.IMD.lz
            640,

            // DSKA0043.IMD.lz
            720,

            // DSKA0044.IMD.lz
            720,

            // DSKA0045.IMD.lz
            2400,

            // DSKA0046.IMD.lz
            2460,

            // DSKA0047.IMD.lz
            1280,

            // DSKA0048.IMD.lz
            1440,

            // DSKA0049.IMD.lz
            1476,

            // DSKA0050.IMD.lz
            1600,

            // DSKA0051.IMD.lz
            1640,

            // DSKA0052.IMD.lz
            2880,

            // DSKA0053.IMD.lz
            2952,

            // DSKA0054.IMD.lz
            3200,

            // DSKA0055.IMD.lz
            3280,

            // DSKA0057.IMD.lz
            3444,

            // DSKA0058.IMD.lz
            3486,

            // DSKA0059.IMD.lz
            3528,

            // DSKA0060.IMD.lz
            3570,

            // DSKA0061.IMD.lz
            5100,

            // DSKA0063.IMD.lz
            6604,

            // DSKA0064.IMD.lz
            9180,

            // DSKA0065.IMD.lz
            10710,

            // DSKA0066.IMD.lz
            10710,

            // DSKA0067.IMD.lz
            13770,

            // DSKA0068.IMD.lz
            1440,

            // DSKA0069.IMD.lz
            1440,

            // DSKA0070.IMD.lz
            1640,

            // DSKA0073.IMD.lz
            320,

            // DSKA0074.IMD.lz
            360,

            // DSKA0075.IMD.lz
            640,

            // DSKA0076.IMD.lz
            720,

            // DSKA0077.IMD.lz
            800,

            // DSKA0078.IMD.lz
            2400,

            // DSKA0080.IMD.lz
            1440,

            // DSKA0081.IMD.lz
            1600,

            // DSKA0082.IMD.lz
            2880,

            // DSKA0083.IMD.lz
            2988,

            // DSKA0084.IMD.lz
            3360,

            // DSKA0085.IMD.lz
            3486,

            // DSKA0086.IMD.lz
            3360,

            // DSKA0089.IMD.lz
            664,

            // DSKA0090.IMD.lz
            670,

            // DSKA0091.IMD.lz
            824,

            // DSKA0092.IMD.lz
            824,

            // DSKA0093.IMD.lz
            1483,

            // DSKA0094.IMD.lz
            995,

            // DSKA0097.IMD.lz
            1812,

            // DSKA0098.IMD.lz
            1160,

            // DSKA0099.IMD.lz
            164,

            // DSKA0101.IMD.lz
            164,

            // DSKA0103.IMD.lz
            164,

            // DSKA0105.IMD.lz
            400,

            // DSKA0106.IMD.lz
            410,

            // DSKA0107.IMD.lz
            800,

            // DSKA0108.IMD.lz
            820,

            // DSKA0109.IMD.lz
            1600,

            // DSKA0110.IMD.lz
            1640,

            // DSKA0111.IMD.lz
            2880,

            // DSKA0112.IMD.lz
            2952,

            // DSKA0113.IMD.lz
            1600,

            // DSKA0114.IMD.lz
            1640,

            // DSKA0115.IMD.lz
            2952,

            // DSKA0116.IMD.lz
            3200,

            // DSKA0117.IMD.lz
            3240,

            // DSKA0120.IMD.lz
            320,

            // DSKA0121.IMD.lz
            360,

            // DSKA0122.IMD.lz
            640,

            // DSKA0123.IMD.lz
            720,

            // DSKA0124.IMD.lz
            2400,

            // DSKA0125.IMD.lz
            1440,

            // DSKA0126.IMD.lz
            2880,

            // DSKA0147.IMD.lz
            320,

            // DSKA0148.IMD.lz
            640,

            // DSKA0149.IMD.lz
            200,

            // DSKA0150.IMD.lz
            400,

            // DSKA0151.IMD.lz
            400,

            // DSKA0153.IMD.lz
            360,

            // DSKA0154.IMD.lz
            800,

            // DSKA0155.IMD.lz
            848,

            // DSKA0157.IMD.lz
            1440,

            // DSKA0158.IMD.lz
            1280,

            // DSKA0159.IMD.lz
            640,

            // DSKA0160.IMD.lz
            1280,

            // DSKA0162.IMD.lz
            320,

            // DSKA0163.IMD.lz
            720,

            // DSKA0164.IMD.lz
            820,

            // DSKA0166.IMD.lz
            1440,

            // DSKA0167.IMD.lz
            960,

            // DSKA0168.IMD.lz
            2400,

            // DSKA0169.IMD.lz
            2880,

            // DSKA0170.IMD.lz
            2952,

            // DSKA0171.IMD.lz
            2988,

            // DSKA0173.IMD.lz
            720,

            // DSKA0174.IMD.lz
            1440,

            // DSKA0175.IMD.lz
            1600,

            // DSKA0176.IMD.lz
            1640,

            // DSKA0177.IMD.lz
            1660,

            // DSKA0180.IMD.lz
            3200,

            // DSKA0181.IMD.lz
            3360,

            // DSKA0182.IMD.lz
            3402,

            // DSKA0183.IMD.lz
            3444,

            // DSKA0184.IMD.lz
            1760,

            // DSKA0185.IMD.lz
            1120,

            // DSKA0186.IMD.lz
            320,

            // DSKA0191.IMD.lz
            626,

            // DSKA0192.IMD.lz
            670,

            // DSKA0194.IMD.lz
            356,

            // DSKA0196.IMD.lz
            960,

            // DSKA0197.IMD.lz
            640,

            // DSKA0198.IMD.lz
            1280,

            // DSKA0199.IMD.lz
            2560,

            // DSKA0200.IMD.lz
            1600,

            // DSKA0201.IMD.lz
            800,

            // DSKA0202.IMD.lz
            1600,

            // DSKA0203.IMD.lz
            1280,

            // DSKA0204.IMD.lz
            360,

            // DSKA0205.IMD.lz
            1476,

            // DSKA0206.IMD.lz
            1600,

            // DSKA0207.IMD.lz
            3200,

            // DSKA0208.IMD.lz
            480,

            // DSKA0209.IMD.lz
            1600,

            // DSKA0210.IMD.lz
            1600,

            // DSKA0211.IMD.lz
            1440,

            // DSKA0212.IMD.lz
            1440,

            // DSKA0213.IMD.lz
            800,

            // DSKA0214.IMD.lz
            1600,

            // DSKA0215.IMD.lz
            1600,

            // DSKA0216.IMD.lz
            2880,

            // DSKA0218.IMD.lz
            5100,

            // DSKA0219.IMD.lz
            9180,

            // DSKA0220.IMD.lz
            13770,

            // DSKA0221.IMD.lz
            5120,

            // DSKA0222.IMD.lz
            1600,

            // DSKA0223.IMD.lz
            1600,

            // DSKA0224.IMD.lz
            1152,

            // DSKA0225.IMD.lz
            1056,

            // DSKA0226.IMD.lz
            2560,

            // DSKA0227.IMD.lz
            5120,

            // DSKA0228.IMD.lz
            1120,

            // DSKA0232.IMD.lz
            621,

            // DSKA0233.IMD.lz
            720,

            // DSKA0234.IMD.lz
            2560,

            // DSKA0235.IMD.lz
            1600,

            // DSKA0236.IMD.lz
            800,

            // DSKA0238.IMD.lz
            1600,

            // DSKA0240.IMD.lz
            720,

            // DSKA0241.IMD.lz
            714,

            // DSKA0242.IMD.lz
            1232,

            // DSKA0243.IMD.lz
            1280,

            // DSKA0244.IMD.lz
            1280,

            // DSKA0245.IMD.lz
            1600,

            // DSKA0246.IMD.lz
            1600,

            // DSKA0247.IMD.lz
            1280,

            // DSKA0248.IMD.lz
            1280,

            // DSKA0250.IMD.lz
            800,

            // DSKA0251.IMD.lz
            2560,

            // DSKA0252.IMD.lz
            1280,

            // DSKA0253.IMD.lz
            800,

            // DSKA0254.IMD.lz
            360,

            // DSKA0255.IMD.lz
            2544,

            // DSKA0258.IMD.lz
            1232,

            // DSKA0262.IMD.lz
            1440,

            // DSKA0263.IMD.lz
            1600,

            // DSKA0264.IMD.lz
            1640,

            // DSKA0265.IMD.lz
            1660,

            // DSKA0266.IMD.lz
            2880,

            // DSKA0267.IMD.lz
            3040,

            // DSKA0268.IMD.lz
            3200,

            // DSKA0269.IMD.lz
            3280,

            // DSKA0270.IMD.lz
            3320,

            // DSKA0271.IMD.lz
            3360,

            // DSKA0272.IMD.lz
            3444,

            // DSKA0273.IMD.lz
            3486,

            // DSKA0280.IMD.lz
            360,

            // DSKA0281.IMD.lz
            400,

            // DSKA0282.IMD.lz
            640,

            // DSKA0283.IMD.lz
            720,

            // DSKA0284.IMD.lz
            800,

            // DSKA0285.IMD.lz
            840,

            // DSKA0287.IMD.lz
            1440,

            // DSKA0288.IMD.lz
            1494,

            // DSKA0289.IMD.lz
            1600,

            // DSKA0290.IMD.lz
            1640,

            // DSKA0291.IMD.lz
            1660,

            // DSKA0299.IMD.lz
            320,

            // DSKA0300.IMD.lz
            360,

            // DSKA0301.IMD.lz
            640,

            // DSKA0302.IMD.lz
            720,

            // DSKA0303.IMD.lz
            2400,

            // DSKA0304.IMD.lz
            1440,

            // DSKA0305.IMD.lz
            2880,

            // DSKA0307.IMD.lz
            840,

            // DSKA0308.IMD.lz
            1600,

            // DSKA0311.IMD.lz
            3444,

            // DSKA0314.IMD.lz
            1440,

            // DSKA0316.IMD.lz
            2880,

            // DSKA0317.IMD.lz
            3360,

            // DSKA0318.IMD.lz
            3444,

            // DSKA0319.IMD.lz
            3360,

            // DSKA0320.IMD.lz
            3360,

            // DSKA0322.IMD.lz
            1386,

            // md1dd_rx01.imd.lz
            2002,

            // md1qd_rx50.imd.lz
            800,

            // md2hd_nec.imd.lz
            1232,

            // mf2dd_2mgui.imd.lz
            164,

            // mf2dd_2m.imd.lz
            987,

            // mf2dd_fdformat_800.imd.lz
            1600,

            // mf2dd_fdformat_820.imd.lz
            1640,

            // mf2dd_freedos.imd.lz
            1600,

            // mf2dd.imd.lz
            1440,

            // mf2hd_2mgui.imd.lz
            164,

            // mf2hd_2m.imd.lz
            1812,

            // mf2hd_fdformat_168.imd.lz
            3360,

            // mf2hd_fdformat_172.imd.lz
            3444,

            // mf2hd_freedos.imd.lz
            3486,

            // mf2hd.imd.lz
            2880,

            // mf2hd_xdf.imd.lz
            670
        };

        readonly uint[] _sectorSize =
        {
            // CPM1_ALL.IMD.lz
            512,

            // DSKA0000.IMD.lz
            512,

            // DSKA0001.IMD.lz
            512,

            // DSKA0002.IMD.lz
            1024,

            // DSKA0003.IMD.lz
            1024,

            // DSKA0004.IMD.lz
            1024,

            // DSKA0006.IMD.lz
            512,

            // DSKA0009.IMD.lz
            512,

            // DSKA0010.IMD.lz
            512,

            // DSKA0011.IMD.lz
            1024,

            // DSKA0012.IMD.lz
            512,

            // DSKA0013.IMD.lz
            512,

            // DSKA0017.IMD.lz
            512,

            // DSKA0018.IMD.lz
            512,

            // DSKA0019.IMD.lz
            512,

            // DSKA0020.IMD.lz
            512,

            // DSKA0021.IMD.lz
            512,

            // DSKA0022.IMD.lz
            256,

            // DSKA0023.IMD.lz
            1024,

            // DSKA0024.IMD.lz
            512,

            // DSKA0025.IMD.lz
            512,

            // DSKA0026.IMD.lz
            1024,

            // DSKA0027.IMD.lz
            1024,

            // DSKA0028.IMD.lz
            512,

            // DSKA0029.IMD.lz
            1024,

            // DSKA0030.IMD.lz
            512,

            // DSKA0031.IMD.lz
            256,

            // DSKA0032.IMD.lz
            256,

            // DSKA0033.IMD.lz
            256,

            // DSKA0034.IMD.lz
            256,

            // DSKA0035.IMD.lz
            512,

            // DSKA0036.IMD.lz
            512,

            // DSKA0037.IMD.lz
            512,

            // DSKA0038.IMD.lz
            512,

            // DSKA0039.IMD.lz
            512,

            // DSKA0040.IMD.lz
            512,

            // DSKA0041.IMD.lz
            512,

            // DSKA0042.IMD.lz
            512,

            // DSKA0043.IMD.lz
            512,

            // DSKA0044.IMD.lz
            512,

            // DSKA0045.IMD.lz
            512,

            // DSKA0046.IMD.lz
            512,

            // DSKA0047.IMD.lz
            512,

            // DSKA0048.IMD.lz
            512,

            // DSKA0049.IMD.lz
            512,

            // DSKA0050.IMD.lz
            512,

            // DSKA0051.IMD.lz
            512,

            // DSKA0052.IMD.lz
            512,

            // DSKA0053.IMD.lz
            512,

            // DSKA0054.IMD.lz
            512,

            // DSKA0055.IMD.lz
            512,

            // DSKA0057.IMD.lz
            512,

            // DSKA0058.IMD.lz
            512,

            // DSKA0059.IMD.lz
            512,

            // DSKA0060.IMD.lz
            512,

            // DSKA0061.IMD.lz
            512,

            // DSKA0063.IMD.lz
            512,

            // DSKA0064.IMD.lz
            512,

            // DSKA0065.IMD.lz
            512,

            // DSKA0066.IMD.lz
            512,

            // DSKA0067.IMD.lz
            512,

            // DSKA0068.IMD.lz
            512,

            // DSKA0069.IMD.lz
            512,

            // DSKA0070.IMD.lz
            512,

            // DSKA0073.IMD.lz
            512,

            // DSKA0074.IMD.lz
            512,

            // DSKA0075.IMD.lz
            512,

            // DSKA0076.IMD.lz
            512,

            // DSKA0077.IMD.lz
            512,

            // DSKA0078.IMD.lz
            512,

            // DSKA0080.IMD.lz
            512,

            // DSKA0081.IMD.lz
            512,

            // DSKA0082.IMD.lz
            512,

            // DSKA0083.IMD.lz
            512,

            // DSKA0084.IMD.lz
            512,

            // DSKA0085.IMD.lz
            512,

            // DSKA0086.IMD.lz
            512,

            // DSKA0089.IMD.lz
            512,

            // DSKA0090.IMD.lz
            2048,

            // DSKA0091.IMD.lz
            1024,

            // DSKA0092.IMD.lz
            2048,

            // DSKA0093.IMD.lz
            1024,

            // DSKA0094.IMD.lz
            2048,

            // DSKA0097.IMD.lz
            1024,

            // DSKA0098.IMD.lz
            2048,

            // DSKA0099.IMD.lz
            16384,

            // DSKA0101.IMD.lz
            16384,

            // DSKA0103.IMD.lz
            16384,

            // DSKA0105.IMD.lz
            512,

            // DSKA0106.IMD.lz
            512,

            // DSKA0107.IMD.lz
            512,

            // DSKA0108.IMD.lz
            512,

            // DSKA0109.IMD.lz
            512,

            // DSKA0110.IMD.lz
            512,

            // DSKA0111.IMD.lz
            512,

            // DSKA0112.IMD.lz
            512,

            // DSKA0113.IMD.lz
            512,

            // DSKA0114.IMD.lz
            512,

            // DSKA0115.IMD.lz
            512,

            // DSKA0116.IMD.lz
            512,

            // DSKA0117.IMD.lz
            512,

            // DSKA0120.IMD.lz
            512,

            // DSKA0121.IMD.lz
            512,

            // DSKA0122.IMD.lz
            512,

            // DSKA0123.IMD.lz
            512,

            // DSKA0124.IMD.lz
            512,

            // DSKA0125.IMD.lz
            512,

            // DSKA0126.IMD.lz
            512,

            // DSKA0147.IMD.lz
            512,

            // DSKA0148.IMD.lz
            512,

            // DSKA0149.IMD.lz
            1024,

            // DSKA0150.IMD.lz
            1024,

            // DSKA0151.IMD.lz
            512,

            // DSKA0153.IMD.lz
            512,

            // DSKA0154.IMD.lz
            512,

            // DSKA0155.IMD.lz
            512,

            // DSKA0157.IMD.lz
            512,

            // DSKA0158.IMD.lz
            256,

            // DSKA0159.IMD.lz
            256,

            // DSKA0160.IMD.lz
            256,

            // DSKA0162.IMD.lz
            512,

            // DSKA0163.IMD.lz
            512,

            // DSKA0164.IMD.lz
            512,

            // DSKA0166.IMD.lz
            512,

            // DSKA0167.IMD.lz
            1024,

            // DSKA0168.IMD.lz
            512,

            // DSKA0169.IMD.lz
            512,

            // DSKA0170.IMD.lz
            512,

            // DSKA0171.IMD.lz
            512,

            // DSKA0173.IMD.lz
            512,

            // DSKA0174.IMD.lz
            512,

            // DSKA0175.IMD.lz
            512,

            // DSKA0176.IMD.lz
            512,

            // DSKA0177.IMD.lz
            512,

            // DSKA0180.IMD.lz
            512,

            // DSKA0181.IMD.lz
            512,

            // DSKA0182.IMD.lz
            512,

            // DSKA0183.IMD.lz
            512,

            // DSKA0184.IMD.lz
            1024,

            // DSKA0185.IMD.lz
            2048,

            // DSKA0186.IMD.lz
            4096,

            // DSKA0191.IMD.lz
            1024,

            // DSKA0192.IMD.lz
            2048,

            // DSKA0194.IMD.lz
            4096,

            // DSKA0196.IMD.lz
            1024,

            // DSKA0197.IMD.lz
            256,

            // DSKA0198.IMD.lz
            256,

            // DSKA0199.IMD.lz
            256,

            // DSKA0200.IMD.lz
            1024,

            // DSKA0201.IMD.lz
            1024,

            // DSKA0202.IMD.lz
            1024,

            // DSKA0203.IMD.lz
            1024,

            // DSKA0204.IMD.lz
            512,

            // DSKA0205.IMD.lz
            512,

            // DSKA0206.IMD.lz
            512,

            // DSKA0207.IMD.lz
            512,

            // DSKA0208.IMD.lz
            1024,

            // DSKA0209.IMD.lz
            512,

            // DSKA0210.IMD.lz
            512,

            // DSKA0211.IMD.lz
            512,

            // DSKA0212.IMD.lz
            512,

            // DSKA0213.IMD.lz
            1024,

            // DSKA0214.IMD.lz
            1024,

            // DSKA0215.IMD.lz
            1024,

            // DSKA0216.IMD.lz
            512,

            // DSKA0218.IMD.lz
            512,

            // DSKA0219.IMD.lz
            512,

            // DSKA0220.IMD.lz
            512,

            // DSKA0221.IMD.lz
            256,

            // DSKA0222.IMD.lz
            512,

            // DSKA0223.IMD.lz
            256,

            // DSKA0224.IMD.lz
            256,

            // DSKA0225.IMD.lz
            256,

            // DSKA0226.IMD.lz
            256,

            // DSKA0227.IMD.lz
            256,

            // DSKA0228.IMD.lz
            256,

            // DSKA0232.IMD.lz
            512,

            // DSKA0233.IMD.lz
            128,

            // DSKA0234.IMD.lz
            256,

            // DSKA0235.IMD.lz
            256,

            // DSKA0236.IMD.lz
            256,

            // DSKA0238.IMD.lz
            512,

            // DSKA0240.IMD.lz
            256,

            // DSKA0241.IMD.lz
            256,

            // DSKA0242.IMD.lz
            1024,

            // DSKA0243.IMD.lz
            256,

            // DSKA0244.IMD.lz
            256,

            // DSKA0245.IMD.lz
            512,

            // DSKA0246.IMD.lz
            512,

            // DSKA0247.IMD.lz
            256,

            // DSKA0248.IMD.lz
            256,

            // DSKA0250.IMD.lz
            1024,

            // DSKA0251.IMD.lz
            256,

            // DSKA0252.IMD.lz
            256,

            // DSKA0253.IMD.lz
            1024,

            // DSKA0254.IMD.lz
            512,

            // DSKA0255.IMD.lz
            256,

            // DSKA0258.IMD.lz
            1024,

            // DSKA0262.IMD.lz
            512,

            // DSKA0263.IMD.lz
            512,

            // DSKA0264.IMD.lz
            512,

            // DSKA0265.IMD.lz
            512,

            // DSKA0266.IMD.lz
            512,

            // DSKA0267.IMD.lz
            512,

            // DSKA0268.IMD.lz
            512,

            // DSKA0269.IMD.lz
            512,

            // DSKA0270.IMD.lz
            512,

            // DSKA0271.IMD.lz
            512,

            // DSKA0272.IMD.lz
            512,

            // DSKA0273.IMD.lz
            512,

            // DSKA0280.IMD.lz
            512,

            // DSKA0281.IMD.lz
            512,

            // DSKA0282.IMD.lz
            512,

            // DSKA0283.IMD.lz
            512,

            // DSKA0284.IMD.lz
            512,

            // DSKA0285.IMD.lz
            512,

            // DSKA0287.IMD.lz
            512,

            // DSKA0288.IMD.lz
            512,

            // DSKA0289.IMD.lz
            512,

            // DSKA0290.IMD.lz
            512,

            // DSKA0291.IMD.lz
            512,

            // DSKA0299.IMD.lz
            512,

            // DSKA0300.IMD.lz
            512,

            // DSKA0301.IMD.lz
            512,

            // DSKA0302.IMD.lz
            512,

            // DSKA0303.IMD.lz
            512,

            // DSKA0304.IMD.lz
            512,

            // DSKA0305.IMD.lz
            512,

            // DSKA0307.IMD.lz
            512,

            // DSKA0308.IMD.lz
            512,

            // DSKA0311.IMD.lz
            512,

            // DSKA0314.IMD.lz
            512,

            // DSKA0316.IMD.lz
            512,

            // DSKA0317.IMD.lz
            512,

            // DSKA0318.IMD.lz
            512,

            // DSKA0319.IMD.lz
            512,

            // DSKA0320.IMD.lz
            512,

            // DSKA0322.IMD.lz
            512,

            // md1dd_rx01.imd.lz
            128,

            // md1qd_rx50.imd.lz
            512,

            // md2hd_nec.imd.lz
            1024,

            // mf2dd_2mgui.imd.lz
            16384,

            // mf2dd_2m.imd.lz
            1024,

            // mf2dd_fdformat_800.imd.lz
            512,

            // mf2dd_fdformat_820.imd.lz
            512,

            // mf2dd_freedos.imd.lz
            512,

            // mf2dd.imd.lz
            512,

            // mf2hd_2mgui.imd.lz
            16384,

            // mf2hd_2m.imd.lz
            1024,

            // mf2hd_fdformat_168.imd.lz
            512,

            // mf2hd_fdformat_172.imd.lz
            512,

            // mf2hd_freedos.imd.lz
            512,

            // mf2hd.imd.lz
            512,

            // mf2hd_xdf.imd.lz
            2048
        };

        readonly MediaType[] _mediaTypes =
        {
            // CPM1_ALL.IMD.lz
            MediaType.DOS_35_DS_DD_8,

            // DSKA0000.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0001.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0002.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0003.IMD.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0004.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0006.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0009.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0010.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0011.IMD.lz
            MediaType.Unknown,

            // DSKA0012.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0013.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0017.IMD.lz
            MediaType.Unknown,

            // DSKA0018.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0019.IMD.lz
            MediaType.Unknown,

            // DSKA0020.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0021.IMD.lz
            MediaType.Unknown,

            // DSKA0022.IMD.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0023.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0024.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0025.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0026.IMD.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0027.IMD.lz
            MediaType.Unknown,

            // DSKA0028.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0029.IMD.lz
            MediaType.Unknown,

            // DSKA0030.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0031.IMD.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0032.IMD.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0033.IMD.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0034.IMD.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0035.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0036.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0037.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0038.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0039.IMD.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0040.IMD.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0041.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0042.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0043.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0044.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0045.IMD.lz
            MediaType.NEC_35_HD_15,

            // DSKA0046.IMD.lz
            MediaType.Unknown,

            // DSKA0047.IMD.lz
            MediaType.DOS_35_DS_DD_8,

            // DSKA0048.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0049.IMD.lz
            MediaType.Unknown,

            // DSKA0050.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0051.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0052.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0053.IMD.lz
            MediaType.Unknown,

            // DSKA0054.IMD.lz
            MediaType.Unknown,

            // DSKA0055.IMD.lz
            MediaType.Unknown,

            // DSKA0057.IMD.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0058.IMD.lz
            MediaType.Unknown,

            // DSKA0059.IMD.lz
            MediaType.Unknown,

            // DSKA0060.IMD.lz
            MediaType.Unknown,

            // DSKA0061.IMD.lz
            MediaType.Unknown,

            // DSKA0063.IMD.lz
            MediaType.Unknown,

            // DSKA0064.IMD.lz
            MediaType.Unknown,

            // DSKA0065.IMD.lz
            MediaType.Unknown,

            // DSKA0066.IMD.lz
            MediaType.Unknown,

            // DSKA0067.IMD.lz
            MediaType.Unknown,

            // DSKA0068.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0069.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0070.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0073.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0074.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0075.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0076.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0077.IMD.lz
            MediaType.Unknown,

            // DSKA0078.IMD.lz
            MediaType.NEC_35_HD_15,

            // DSKA0080.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0081.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0082.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0083.IMD.lz
            MediaType.Unknown,

            // DSKA0084.IMD.lz
            MediaType.DMF,

            // DSKA0085.IMD.lz
            MediaType.Unknown,

            // DSKA0086.IMD.lz
            MediaType.DMF,

            // DSKA0089.IMD.lz
            MediaType.Unknown,

            // DSKA0090.IMD.lz
            MediaType.Unknown,

            // DSKA0091.IMD.lz
            MediaType.Unknown,

            // DSKA0092.IMD.lz
            MediaType.Unknown,

            // DSKA0093.IMD.lz
            MediaType.Unknown,

            // DSKA0094.IMD.lz
            MediaType.Unknown,

            // DSKA0097.IMD.lz
            MediaType.Unknown,

            // DSKA0098.IMD.lz
            MediaType.Unknown,

            // DSKA0099.IMD.lz
            MediaType.Unknown,

            // DSKA0101.IMD.lz
            MediaType.Unknown,

            // DSKA0103.IMD.lz
            MediaType.Unknown,

            // DSKA0105.IMD.lz
            MediaType.Unknown,

            // DSKA0106.IMD.lz
            MediaType.Unknown,

            // DSKA0107.IMD.lz
            MediaType.Unknown,

            // DSKA0108.IMD.lz
            MediaType.Unknown,

            // DSKA0109.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0110.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0111.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0112.IMD.lz
            MediaType.Unknown,

            // DSKA0113.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0114.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0115.IMD.lz
            MediaType.Unknown,

            // DSKA0116.IMD.lz
            MediaType.Unknown,

            // DSKA0117.IMD.lz
            MediaType.Unknown,

            // DSKA0120.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0121.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0122.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0123.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0124.IMD.lz
            MediaType.NEC_35_HD_15,

            // DSKA0125.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0126.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0147.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0148.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0149.IMD.lz
            MediaType.Unknown,

            // DSKA0150.IMD.lz
            MediaType.Unknown,

            // DSKA0151.IMD.lz
            MediaType.Unknown,

            // DSKA0153.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0154.IMD.lz
            MediaType.RX50,

            // DSKA0155.IMD.lz
            MediaType.Unknown,

            // DSKA0157.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0158.IMD.lz
            MediaType.Unknown,

            // DSKA0159.IMD.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0160.IMD.lz
            MediaType.Unknown,

            // DSKA0162.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0163.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0164.IMD.lz
            MediaType.Unknown,

            // DSKA0166.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0167.IMD.lz
            MediaType.Unknown,

            // DSKA0168.IMD.lz
            MediaType.NEC_35_HD_15,

            // DSKA0169.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0170.IMD.lz
            MediaType.Unknown,

            // DSKA0171.IMD.lz
            MediaType.Unknown,

            // DSKA0173.IMD.lz
            MediaType.DOS_35_SS_DD_9,

            // DSKA0174.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0175.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0176.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0177.IMD.lz
            MediaType.Unknown,

            // DSKA0180.IMD.lz
            MediaType.Unknown,

            // DSKA0181.IMD.lz
            MediaType.DMF,

            // DSKA0182.IMD.lz
            MediaType.Unknown,

            // DSKA0183.IMD.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0184.IMD.lz
            MediaType.Unknown,

            // DSKA0185.IMD.lz
            MediaType.Unknown,

            // DSKA0186.IMD.lz
            MediaType.Unknown,

            // DSKA0191.IMD.lz
            MediaType.Unknown,

            // DSKA0192.IMD.lz
            MediaType.Unknown,

            // DSKA0194.IMD.lz
            MediaType.Unknown,

            // DSKA0196.IMD.lz
            MediaType.Unknown,

            // DSKA0197.IMD.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0198.IMD.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0199.IMD.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0200.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0201.IMD.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0202.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0203.IMD.lz
            MediaType.Unknown,

            // DSKA0204.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0205.IMD.lz
            MediaType.Unknown,

            // DSKA0206.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0207.IMD.lz
            MediaType.Unknown,

            // DSKA0208.IMD.lz
            MediaType.Unknown,

            // DSKA0209.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0210.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0211.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0212.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0213.IMD.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0214.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0215.IMD.lz
            MediaType.ACORN_35_DS_HD,

            // DSKA0216.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0218.IMD.lz
            MediaType.Unknown,

            // DSKA0219.IMD.lz
            MediaType.Unknown,

            // DSKA0220.IMD.lz
            MediaType.Unknown,

            // DSKA0221.IMD.lz
            MediaType.Unknown,

            // DSKA0222.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0223.IMD.lz
            MediaType.Unknown,

            // DSKA0224.IMD.lz
            MediaType.Unknown,

            // DSKA0225.IMD.lz
            MediaType.Unknown,

            // DSKA0226.IMD.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0227.IMD.lz
            MediaType.Unknown,

            // DSKA0228.IMD.lz
            MediaType.Unknown,

            // DSKA0232.IMD.lz
            MediaType.Unknown,

            // DSKA0233.IMD.lz
            MediaType.ATARI_525_SD,

            // DSKA0234.IMD.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0235.IMD.lz
            MediaType.Unknown,

            // DSKA0236.IMD.lz
            MediaType.ACORN_525_SS_SD_80,

            // DSKA0238.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0240.IMD.lz
            MediaType.ATARI_525_DD,

            // DSKA0241.IMD.lz
            MediaType.Unknown,

            // DSKA0242.IMD.lz
            MediaType.NEC_35_HD_8,

            // DSKA0243.IMD.lz
            MediaType.Unknown,

            // DSKA0244.IMD.lz
            MediaType.Unknown,

            // DSKA0245.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0246.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0247.IMD.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0248.IMD.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0250.IMD.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0251.IMD.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0252.IMD.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0253.IMD.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0254.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0255.IMD.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0258.IMD.lz
            MediaType.NEC_35_HD_8,

            // DSKA0262.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0263.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0264.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0265.IMD.lz
            MediaType.Unknown,

            // DSKA0266.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0267.IMD.lz
            MediaType.XDF_525,

            // DSKA0268.IMD.lz
            MediaType.Unknown,

            // DSKA0269.IMD.lz
            MediaType.Unknown,

            // DSKA0270.IMD.lz
            MediaType.Unknown,

            // DSKA0271.IMD.lz
            MediaType.DMF,

            // DSKA0272.IMD.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0273.IMD.lz
            MediaType.Unknown,

            // DSKA0280.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0281.IMD.lz
            MediaType.Unknown,

            // DSKA0282.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0283.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0284.IMD.lz
            MediaType.Unknown,

            // DSKA0285.IMD.lz
            MediaType.Unknown,

            // DSKA0287.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0288.IMD.lz
            MediaType.Unknown,

            // DSKA0289.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0290.IMD.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0291.IMD.lz
            MediaType.Unknown,

            // DSKA0299.IMD.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0300.IMD.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0301.IMD.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0302.IMD.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0303.IMD.lz
            MediaType.NEC_35_HD_15,

            // DSKA0304.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0305.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0307.IMD.lz
            MediaType.Unknown,

            // DSKA0308.IMD.lz
            MediaType.CBM_35_DD,

            // DSKA0311.IMD.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0314.IMD.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0316.IMD.lz
            MediaType.DOS_35_HD,

            // DSKA0317.IMD.lz
            MediaType.DMF,

            // DSKA0318.IMD.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0319.IMD.lz
            MediaType.DMF,

            // DSKA0320.IMD.lz
            MediaType.DMF,

            // DSKA0322.IMD.lz
            MediaType.Unknown,

            // md1dd_rx01.imd.lz
            MediaType.RX01,

            // md1qd_rx50.imd.lz
            MediaType.RX50,

            // md2hd_nec.imd.lz
            MediaType.NEC_35_HD_8,

            // mf2dd_2mgui.imd.lz
            MediaType.Unknown,

            // mf2dd_2m.imd.lz
            MediaType.Unknown,

            // mf2dd_fdformat_800.imd.lz
            MediaType.CBM_35_DD,

            // mf2dd_fdformat_820.imd.lz
            MediaType.FDFORMAT_35_DD,

            // mf2dd_freedos.imd.lz
            MediaType.CBM_35_DD,

            // mf2dd.imd.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2hd_2mgui.imd.lz
            MediaType.Unknown,

            // mf2hd_2m.imd.lz
            MediaType.Unknown,

            // mf2hd_fdformat_168.imd.lz
            MediaType.DMF,

            // mf2hd_fdformat_172.imd.lz
            MediaType.FDFORMAT_35_HD,

            // mf2hd_freedos.imd.lz
            MediaType.Unknown,

            // mf2hd.imd.lz
            MediaType.DOS_35_HD,

            // mf2hd_xdf.imd.lz
            MediaType.Unknown
        };

        readonly string[] _md5S =
        {
            // CPM1_ALL.IMD.lz
            "b5ab1915fc3d7fceecfcd7fda82f6b0d",

            // DSKA0000.IMD.lz
            "e8bbbd22db87181974e12ba0227ea011",

            // DSKA0001.IMD.lz
            "9f5635f3df4d880a500910b0ad1ab535",

            // DSKA0002.IMD.lz
            "3bad4b4db8f5e2f991637fccf7a25740",

            // DSKA0003.IMD.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0004.IMD.lz
            "a481bd5a8281dad089edbef390c136ed",

            // DSKA0006.IMD.lz
            "46fce47baf08c6f093f2c355a603543d",

            // DSKA0009.IMD.lz
            "95ea232f59e44db374b994cfe7f1c07f",

            // DSKA0010.IMD.lz
            "9e2b01f4397db2a6c76e2bc267df37b3",

            // DSKA0011.IMD.lz
            "dbbf55398d930e14c2b0a035dd1277b9",

            // DSKA0012.IMD.lz
            "656002e6e620cb3b73c27f4c21d32edb",

            // DSKA0013.IMD.lz
            "1244cc2c101c66e6bb4ad5183b356b19",

            // DSKA0017.IMD.lz
            "a817a56036f591a5cff11857b7d466be",

            // DSKA0018.IMD.lz
            "439b2b76e154f3ce7e86bf1377282d5f",

            // DSKA0019.IMD.lz
            "3c21d11e2b4ca108de3ec8ffface814d",

            // DSKA0020.IMD.lz
            "c2e64e8a388b4401719f06d6a868dd1b",

            // DSKA0021.IMD.lz
            "6fc7f2233f094af7ae0d454668976858",

            // DSKA0022.IMD.lz
            "ad6c3e6910457a53572695401efda4ab",

            // DSKA0023.IMD.lz
            "5e41fe3201ab32f25873faf8d3f79a02",

            // DSKA0024.IMD.lz
            "2302991363cb3681cffdc4388915b51e",

            // DSKA0025.IMD.lz
            "4e4cafed1cc22ea72201169427e5e1b6",

            // DSKA0026.IMD.lz
            "a579b349a5a24218d59a44e36bdb1333",

            // DSKA0027.IMD.lz
            "669b2155d5e4d7849d662729717a68d8",

            // DSKA0028.IMD.lz
            "1a4c7487382c98b7bc74623ddfb488e6",

            // DSKA0029.IMD.lz
            "23f5700ea3bfe076c88dd399a8026a1e",

            // DSKA0030.IMD.lz
            "af83d011608042d35021e39aa5e10b2f",

            // DSKA0031.IMD.lz
            "e640835966327f3f662e1db8e0575510",

            // DSKA0032.IMD.lz
            "ff3534234d1d2dd88bf6e83be23d9227",

            // DSKA0033.IMD.lz
            "dfaff34a6556b515642f1e54f839b02e",

            // DSKA0034.IMD.lz
            "ca8f5c7f9ed161b03ccb166eb9d62146",

            // DSKA0035.IMD.lz
            "6642c1a32d2c58e93481d664974fc202",

            // DSKA0036.IMD.lz
            "6642c1a32d2c58e93481d664974fc202",

            // DSKA0037.IMD.lz
            "5101f89850dc28efbcfb7622086a9ddf",

            // DSKA0038.IMD.lz
            "8e570be2ed1f00ddea82e50a2d9c446a",

            // DSKA0039.IMD.lz
            "abba2a1ddd60a649047a9c44d94bbeae",

            // DSKA0040.IMD.lz
            "e3bc48bec81be5b35be73d41fdffd2ab",

            // DSKA0041.IMD.lz
            "43b5068af9d016d1432eb2e12d2b802a",

            // DSKA0042.IMD.lz
            "5bf2ad4dc300592604b6e32f8b8e2656",

            // DSKA0043.IMD.lz
            "cb9a832ca6a4097b8ccc30d2108e1f7d",

            // DSKA0044.IMD.lz
            "56d181a6bb8713e6b2854fe8887faab6",

            // DSKA0045.IMD.lz
            "41aef7cff26aefda1add8d49c5b962c2",

            // DSKA0046.IMD.lz
            "2437c5f089f1cba3866b36360b016f16",

            // DSKA0047.IMD.lz
            "bdaa8f17373b265830fdf3a06b794367",

            // DSKA0048.IMD.lz
            "629932c285478d0540ff7936aa008351",

            // DSKA0049.IMD.lz
            "7a2abef5d4701e2e49abb05af8d4da50",

            // DSKA0050.IMD.lz
            "e3507522c914264f44fb2c92c3170c09",

            // DSKA0051.IMD.lz
            "824fe65dbb1a42b6b94f05405ef984f2",

            // DSKA0052.IMD.lz
            "1a8c2e78e7132cf9ba5d6c2b75876be0",

            // DSKA0053.IMD.lz
            "936b20bb0966fe693b4d5e2353e24846",

            // DSKA0054.IMD.lz
            "803b01a0b440c2837d37c21308f30cd5",

            // DSKA0055.IMD.lz
            "aa0d31f914760cc4cde75479779ebed6",

            // DSKA0057.IMD.lz
            "5e413433c54f48978d281c6e66d1106e",

            // DSKA0058.IMD.lz
            "4fc28b0128543b2eb70f6432c4c8a980",

            // DSKA0059.IMD.lz
            "24a7459d080cea3a60d131b8fd7dc5d1",

            // DSKA0060.IMD.lz
            "2031b1e16ee2defc0d15f732f633df33",

            // DSKA0061.IMD.lz
            "79e5f1fbd63b87c087d85904d45964e6",

            // DSKA0063.IMD.lz
            "1b2495a8f2274852b6fae80ae6fbff2f",

            // DSKA0064.IMD.lz
            "3a70851950ad06c20e3063ad6f128eef",

            // DSKA0065.IMD.lz
            "98a91bbdbe8454cf64e20d0ec5c35017",

            // DSKA0066.IMD.lz
            "666706f299a1362cb30f34a3a7f555be",

            // DSKA0067.IMD.lz
            "2fa1eedb57fac492d6f6b71e2c0a079c",

            // DSKA0068.IMD.lz
            "3152c8e3544bbfaceff14b7522faf5af",

            // DSKA0069.IMD.lz
            "5fc19ca552b6db957061e9a1750394d2",

            // DSKA0070.IMD.lz
            "d1e978b679c63a218c3f77a7ca2c7206",

            // DSKA0073.IMD.lz
            "a33b46f042b78fe3d0b3c5dbb3908a93",

            // DSKA0074.IMD.lz
            "565d3c001cbb532154aa5d3c65b2439c",

            // DSKA0075.IMD.lz
            "e60442c3ebd72c99bdd7545fdba59613",

            // DSKA0076.IMD.lz
            "058a33a129539285c9b64010496af52f",

            // DSKA0077.IMD.lz
            "0726ecbc38965d30a6222c3e74cd1aa3",

            // DSKA0078.IMD.lz
            "c9a193837db7d8a5eb025eb41e8a76d7",

            // DSKA0080.IMD.lz
            "c38d69ac88520f14fcc6d6ced22b065d",

            // DSKA0081.IMD.lz
            "91d51964e1e64ef3f6f622fa19aa833c",

            // DSKA0082.IMD.lz
            "db36d9651c952ff679ec33223c8db2d3",

            // DSKA0083.IMD.lz
            "5f1d98806309aee7f81de72e51e6d386",

            // DSKA0084.IMD.lz
            "1207a1cc7ff73d4f74c8984b4e7db33f",

            // DSKA0085.IMD.lz
            "c97a3081fd25474b6b7945b8572d5ab8",

            // DSKA0086.IMD.lz
            "31269ed6464302ae26d22b7c87bceb23",

            // DSKA0089.IMD.lz
            "8b31e5865611dbe01cc25b5ba2fbdf25",

            // DSKA0090.IMD.lz
            "be278c00c3ec906756e7c8d544d8833d",

            // DSKA0091.IMD.lz
            "8e7fb60151e0002e8bae2fb2abe13a69",

            // DSKA0092.IMD.lz
            "45e0b2a2925a95bbdcb43a914d70f91b",

            // DSKA0093.IMD.lz
            "082d7eda62eead1e20fd5a060997ff0f",

            // DSKA0094.IMD.lz
            "9b75a2fb671d1e7fa27434038b375e5e",

            // DSKA0097.IMD.lz
            "97c4f895d64ba196f19a3179e68ef693",

            // DSKA0098.IMD.lz
            "c838233a380973de386e66ee0e0cbcc2",

            // DSKA0099.IMD.lz
            "dea88f91ca0f6d90626b4029286cb01f",

            // DSKA0101.IMD.lz
            "db82b15389e2ffa9a20f7251cc5cce5b",

            // DSKA0103.IMD.lz
            "638b56d7061a8156ee87166c78f06111",

            // DSKA0105.IMD.lz
            "d40a99cb549fcfb26fcf9ef01b5dfca7",

            // DSKA0106.IMD.lz
            "7b41dd9ca7eb32828960eb1417a6092a",

            // DSKA0107.IMD.lz
            "126dfd25363c076727dfaab03955c931",

            // DSKA0108.IMD.lz
            "e6492aac144f5f6f593b84c64680cf64",

            // DSKA0109.IMD.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0110.IMD.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0111.IMD.lz
            "f01541de322c8d6d7321084d7a245e7b",

            // DSKA0112.IMD.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0113.IMD.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0114.IMD.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0115.IMD.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0116.IMD.lz
            "6631b66fdfd89319323771c41334c7ba",

            // DSKA0117.IMD.lz
            "4b5e2c9599bb7861b3b52bec00d81278",

            // DSKA0120.IMD.lz
            "7d36aee5a3071ff75b979f3acb649c40",

            // DSKA0121.IMD.lz
            "0ccb62039363ab544c69eca229a17fae",

            // DSKA0122.IMD.lz
            "7851d31fad9302ff45d3ded4fba25387",

            // DSKA0123.IMD.lz
            "915b08c82591e8488320e001b7303b6d",

            // DSKA0124.IMD.lz
            "5e5ea6fe9adf842221fdc60e56630405",

            // DSKA0125.IMD.lz
            "a22e254f7e3526ec30dc4915a19fcb52",

            // DSKA0126.IMD.lz
            "ddc6c1200c60e9f7796280f50c2e5283",

            // DSKA0147.IMD.lz
            "6efa72a33021d5051546c3e0dd4c3c09",

            // DSKA0148.IMD.lz
            "8b5acfd14818ff9556d3d81361ce4862",

            // DSKA0149.IMD.lz
            "cf42d08469548a31caf2649a1d08a85f",

            // DSKA0150.IMD.lz
            "62745e10683cf2ec1dac177535459891",

            // DSKA0151.IMD.lz
            "cf42d08469548a31caf2649a1d08a85f",

            // DSKA0153.IMD.lz
            "298c377de52947c472a85d281b6d3d4d",

            // DSKA0154.IMD.lz
            "387373301cf6c15d61eec9bab18d9b6a",

            // DSKA0155.IMD.lz
            "83b66a88d92cbf2715343016e4108211",

            // DSKA0157.IMD.lz
            "20e047061b6ca4059288deed8c9dd247",

            // DSKA0158.IMD.lz
            "8b5acfd14818ff9556d3d81361ce4862",

            // DSKA0159.IMD.lz
            "6efa72a33021d5051546c3e0dd4c3c09",

            // DSKA0160.IMD.lz
            "8b5acfd14818ff9556d3d81361ce4862",

            // DSKA0162.IMD.lz
            "e63014a4299f52f22e6e2c9609f51979",

            // DSKA0163.IMD.lz
            "be05d1ff10ef8b2220546c4db962ac9e",

            // DSKA0164.IMD.lz
            "32823b9009c99b6711e89336ad03ec7f",

            // DSKA0166.IMD.lz
            "1c8b03a8550ed3e70e1c78316aa445aa",

            // DSKA0167.IMD.lz
            "efbc62e2ecddc15241aa0779e078d478",

            // DSKA0168.IMD.lz
            "0bdf9130c07bb5d558a4705249f949d0",

            // DSKA0169.IMD.lz
            "2dafeddaa99e7dc0db5ef69e128f9c8e",

            // DSKA0170.IMD.lz
            "589ae671a19e78ffcba5032092c4c0d5",

            // DSKA0171.IMD.lz
            "cf0c71b65b56cb6b617d29525bd719dd",

            // DSKA0173.IMD.lz
            "028769dc0abefab1740cc309432588b6",

            // DSKA0174.IMD.lz
            "152023525154b45ab26687190bac94db",

            // DSKA0175.IMD.lz
            "db38ecd93f28dd065927fed21917eed5",

            // DSKA0176.IMD.lz
            "716262401bc69f2f440a9c156c21c9e9",

            // DSKA0177.IMD.lz
            "83213865ca6a40c289b22324a32a2608",

            // DSKA0180.IMD.lz
            "f206c0caa4e0eda37233ab6e89ab5493",

            // DSKA0181.IMD.lz
            "554492a7b41f4cd9068a3a2b70eb0e5f",

            // DSKA0182.IMD.lz
            "865ad9072cb6c7458f7d86d7e9368622",

            // DSKA0183.IMD.lz
            "2461e458438f0033bc5811fd6958ad02",

            // DSKA0184.IMD.lz
            "be75996696aa70ee9338297137556d83",

            // DSKA0185.IMD.lz
            "5a0f2bad567464288ec7ce935672870a",

            // DSKA0186.IMD.lz
            "69f9f0b5c1fc00a8f398151df9d93ab5",

            // DSKA0191.IMD.lz
            "fb144f79239f6f5f113b417700c2d278",

            // DSKA0192.IMD.lz
            "6a936d2ecb771e37b856bdad16822c32",

            // DSKA0194.IMD.lz
            "e283af9d280efaf059c816b6a2c9206b",

            // DSKA0196.IMD.lz
            "e4625838148a4b7c6580c697cd47362c",

            // DSKA0197.IMD.lz
            "74f71ef3978fefce64689e8be18359ba",

            // DSKA0198.IMD.lz
            "5c4e555b29a264f2a81f8a2b58bfc442",

            // DSKA0199.IMD.lz
            "64ae73ac812bbf473a5d443de4d5dfbf",

            // DSKA0200.IMD.lz
            "a481bd5a8281dad089edbef390c136ed",

            // DSKA0201.IMD.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0202.IMD.lz
            "a481bd5a8281dad089edbef390c136ed",

            // DSKA0203.IMD.lz
            "8a16a3008739516fc3ba4c878868d056",

            // DSKA0204.IMD.lz
            "46fce47baf08c6f093f2c355a603543d",

            // DSKA0205.IMD.lz
            "ee73a5d5c8dfac236baf7b99811696f9",

            // DSKA0206.IMD.lz
            "b3bdbc62fb96e3893dac3bccbde59ab0",

            // DSKA0207.IMD.lz
            "02942b9dc9d3b1bc9335b73c99e6da2e",

            // DSKA0208.IMD.lz
            "dfc9e8c7bd3d50f404d6f0b6ada20b0c",

            // DSKA0209.IMD.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0210.IMD.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0211.IMD.lz
            "647f14749f59be471aac04a71a079a64",

            // DSKA0212.IMD.lz
            "517cdd5e42a4673f733d1aedfb46770f",

            // DSKA0213.IMD.lz
            "6ad92e9522e4ba902c01beecb5943bb1",

            // DSKA0214.IMD.lz
            "9a1a7d8f53fcfad7603fe585c6c7214c",

            // DSKA0215.IMD.lz
            "2a7a9b48551fd4d8b166bcfcbe1ca132",

            // DSKA0216.IMD.lz
            "40199611e6e75bbc37ad6c52a5b77eae",

            // DSKA0218.IMD.lz
            "8fa0ffd7481a94b9e7c4006599329250",

            // DSKA0219.IMD.lz
            "3fa51592c5a65b7e4915a8e22d523ced",

            // DSKA0220.IMD.lz
            "2153339750c119627bab75bd0bf7a193",

            // DSKA0221.IMD.lz
            "f92b2e52259531d50bfb403dc1274ab1",

            // DSKA0222.IMD.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0223.IMD.lz
            "a5dc382d75ec46434b313e289c281d8c",

            // DSKA0224.IMD.lz
            "8335b175c352352e19f9008ad67d1375",

            // DSKA0225.IMD.lz
            "447efa963c19474508c503d037a3b429",

            // DSKA0226.IMD.lz
            "b7669fa76ecf5634313675b001bb7fa2",

            // DSKA0227.IMD.lz
            "676f1bc7764899912ab6ad8257c63a16",

            // DSKA0228.IMD.lz
            "d72e86324d4d518996f6671751614800",

            // DSKA0232.IMD.lz
            "b76bd117ce24d933cdefe09b1de2164a",

            // DSKA0233.IMD.lz
            "a769b7642a222d97a56c46f53833fafa",

            // DSKA0234.IMD.lz
            "dfa733d034bb1f83d694dfa217910081",

            // DSKA0235.IMD.lz
            "8260ee01a245aec2de162ee0d85f4b7f",

            // DSKA0236.IMD.lz
            "261c7a5a4298e9f050928dd770097c77",

            // DSKA0238.IMD.lz
            "a47068ff73dfbea58c25daa5b9132a9e",

            // DSKA0240.IMD.lz
            "d1ab955f0961ab94e6cf69f78134a84b",

            // DSKA0241.IMD.lz
            "8b62738f15bcc916a668eaa67eec86e7",

            // DSKA0242.IMD.lz
            "87a432496cb23b5c2299545500df3553",

            // DSKA0243.IMD.lz
            "9866ab8e58fa4be25010184aec4ad3aa",

            // DSKA0244.IMD.lz
            "9dab329ae098b29889ab08278de38f95",

            // DSKA0245.IMD.lz
            "0d71b4952dadbfb1061acc1f4640c787",

            // DSKA0246.IMD.lz
            "af7ac6b5b9d2d57dad22dbb64ef7de38",

            // DSKA0247.IMD.lz
            "f8f81f945aaad6fbfe7e2db1905302c1",

            // DSKA0248.IMD.lz
            "f6f81c75b5ba45d91c1886c6dda9caee",

            // DSKA0250.IMD.lz
            "d4809467b321991a9c772ad87fc8aa19",

            // DSKA0251.IMD.lz
            "d075e50705f4ddca7ba4dbc981ec1176",

            // DSKA0252.IMD.lz
            "9f86480c86bae33a5b444e4a7ed55048",

            // DSKA0253.IMD.lz
            "629971775d902d1cc2658fc76f57e072",

            // DSKA0254.IMD.lz
            "5dc0d482a773043d8683a84c8220df95",

            // DSKA0255.IMD.lz
            "1718d8acd18fce3c5c1a7a074ed8ac29",

            // DSKA0258.IMD.lz
            "855943f9caecdcce9b06f0098d773c6b",

            // DSKA0262.IMD.lz
            "5ac0a9fc7337f761098f816359b0f6f7",

            // DSKA0263.IMD.lz
            "1ea6ec8e663218b1372048f6e25795b5",

            // DSKA0264.IMD.lz
            "77a1167b1b9043496e32b8578cde0ff0",

            // DSKA0265.IMD.lz
            "4b07d760d65f3f0f8ffa5f2b81cee907",

            // DSKA0266.IMD.lz
            "32c044c5c2b0bd13806149a759c14935",

            // DSKA0267.IMD.lz
            "8752095abc13dba3f3467669da333891",

            // DSKA0268.IMD.lz
            "aece7cd34bbba3e75307fa70404d9d30",

            // DSKA0269.IMD.lz
            "5289afb16a6e4a33213e3bcca56c6230",

            // DSKA0270.IMD.lz
            "1aef0a0ba233476db6567878c3c2b266",

            // DSKA0271.IMD.lz
            "b96596711f4d2ee85dfda0fe3b9f26c3",

            // DSKA0272.IMD.lz
            "a4f461af7fda5e93a7ab63fcbb7e7683",

            // DSKA0273.IMD.lz
            "8f7f7099d4475f6631fcf0a79b031d61",

            // DSKA0280.IMD.lz
            "4feeaf4b4ee5dad85db727fbbda4b6d1",

            // DSKA0281.IMD.lz
            "3c77ca681df78e4cd7baa162aa9b0859",

            // DSKA0282.IMD.lz
            "51da1f86c49657ffdb367bb2ddeb7990",

            // DSKA0283.IMD.lz
            "b81a4987f89936630b8ebc62e4bbce6e",

            // DSKA0284.IMD.lz
            "f76f92dd326c99c5efad5ee58daf72e1",

            // DSKA0285.IMD.lz
            "b6f2c10e42908e334025bc4ffd81e771",

            // DSKA0287.IMD.lz
            "f2f409ea2a62a7866fd2777cc4fc9739",

            // DSKA0288.IMD.lz
            "3e441d69cec5c3169274e1379de4af4b",

            // DSKA0289.IMD.lz
            "30a93f30dd4485c6fc037fe0775d3fc7",

            // DSKA0290.IMD.lz
            "e0caf02cce5597c98313bcc480366ec7",

            // DSKA0291.IMD.lz
            "4af4904d2b3c815da7bef7049209f5eb",

            // DSKA0299.IMD.lz
            "39bf5a98bcb2185d855ac06378febcfa",

            // DSKA0300.IMD.lz
            "dc20055b6e6fd6f8e1114d4be2effeed",

            // DSKA0301.IMD.lz
            "56af9256cf71d5aac5fd5d363674bc49",

            // DSKA0302.IMD.lz
            "bbba1e2d1418e05c3a4e7b4d585d160b",

            // DSKA0303.IMD.lz
            "bca3a045e81617f7f5ebb5a8818eac47",

            // DSKA0304.IMD.lz
            "a296663cb8e75e94603221352f29cfff",

            // DSKA0305.IMD.lz
            "ecda36ebf0e1100233cb0ec722c18583",

            // DSKA0307.IMD.lz
            "cef2f4fe9b1a32d5c0544f814e634264",

            // DSKA0308.IMD.lz
            "bbe58e26b8f8f822cd3edfd37a4e4924",

            // DSKA0311.IMD.lz
            "b9b6ebdf711364c979de7cf70c3a438a",

            // DSKA0314.IMD.lz
            "d37424f367f545acbb397f2bed766843",

            // DSKA0316.IMD.lz
            "9963dd6f19ce6bd56eabeccdfbbd821a",

            // DSKA0317.IMD.lz
            "acf6604559ae8217f7869823e2429024",

            // DSKA0318.IMD.lz
            "23bf2139cdfdc4c16db058fd31ea6481",

            // DSKA0319.IMD.lz
            "fa26adda0415f02057b113ad29c80c8d",

            // DSKA0320.IMD.lz
            "4f2a8d036fefd6c6c88d99eda3aa12b7",

            // DSKA0322.IMD.lz
            "e794a3ffa4069ea999fdf7146710fa9e",

            // md1dd_rx01.imd.lz
            "5b4e36d92b180c3845387391cb5a1c64",

            // md1qd_rx50.imd.lz
            "ccd4431139755c58f340681f63510642",

            // md2hd_nec.imd.lz
            "fd54916f713d01b670c1a5df5e74a97f",

            // mf2dd_2mgui.imd.lz
            "623b224f63d65ae3b6c3ddadadf3b836",

            // mf2dd_2m.imd.lz
            "08b530d8c25d785b20c93a1a7a6468a0",

            // mf2dd_fdformat_800.imd.lz
            "c533488a21098a62c85f1649abda2803",

            // mf2dd_fdformat_820.imd.lz
            "db9cfb6eea18820b7a7e0b5b45594471",

            // mf2dd_freedos.imd.lz
            "1ff7649b679ba22ff20d39ff717dbec8",

            // mf2dd.imd.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2hd_2mgui.imd.lz
            "adafed1fac3d1a181380bdb590249385",

            // mf2hd_2m.imd.lz
            "c741c78eecd673f8fc49e77459871940",

            // mf2hd_fdformat_168.imd.lz
            "03c2af6a8ebf4bd6f530335de34ae5dd",

            // mf2hd_fdformat_172.imd.lz
            "9dea1e119a73a21a38d134f36b2e5564",

            // mf2hd_freedos.imd.lz
            "dbd52e9e684f97d9e2292811242bb24e",

            // mf2hd.imd.lz
            "b4a602f67903c46eef62addb0780aa56",

            // mf2hd_xdf.imd.lz
            "71194f8dba31d29780bd0a6ecee5ab2b"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "ImageDisk");

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

                    var  image  = new Imd();
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

                    var   image       = new Imd();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

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