// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CopyQM.cs
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
    public class CopyQm : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "DSKA0000.CQM.lz", "DSKA0001.CQM.lz", "DSKA0002.CQM.lz", "DSKA0003.CQM.lz", "DSKA0004.CQM.lz",
            "DSKA0006.CQM.lz", "DSKA0009.CQM.lz", "DSKA0010.CQM.lz", "DSKA0011.CQM.lz", "DSKA0012.CQM.lz",
            "DSKA0013.CQM.lz", "DSKA0017.CQM.lz", "DSKA0018.CQM.lz", "DSKA0019.CQM.lz", "DSKA0020.CQM.lz",
            "DSKA0021.CQM.lz", "DSKA0023.CQM.lz", "DSKA0024.CQM.lz", "DSKA0025.CQM.lz", "DSKA0026.CQM.lz",
            "DSKA0027.CQM.lz", "DSKA0028.CQM.lz", "DSKA0029.CQM.lz", "DSKA0030.CQM.lz", "DSKA0031.CQM.lz",
            "DSKA0032.CQM.lz", "DSKA0033.CQM.lz", "DSKA0034.CQM.lz", "DSKA0035.CQM.lz", "DSKA0036.CQM.lz",
            "DSKA0037.CQM.lz", "DSKA0038.CQM.lz", "DSKA0039.CQM.lz", "DSKA0040.CQM.lz", "DSKA0041.CQM.lz",
            "DSKA0042.CQM.lz", "DSKA0043.CQM.lz", "DSKA0044.CQM.lz", "DSKA0045.CQM.lz", "DSKA0046.CQM.lz",
            "DSKA0047.CQM.lz", "DSKA0048.CQM.lz", "DSKA0049.CQM.lz", "DSKA0050.CQM.lz", "DSKA0051.CQM.lz",
            "DSKA0052.CQM.lz", "DSKA0053.CQM.lz", "DSKA0054.CQM.lz", "DSKA0055.CQM.lz", "DSKA0056.CQM.lz",
            "DSKA0057.CQM.lz", "DSKA0058.CQM.lz", "DSKA0059.CQM.lz", "DSKA0060.CQM.lz", "DSKA0069.CQM.lz",
            "DSKA0070.CQM.lz", "DSKA0073.CQM.lz", "DSKA0074.CQM.lz", "DSKA0075.CQM.lz", "DSKA0076.CQM.lz",
            "DSKA0077.CQM.lz", "DSKA0078.CQM.lz", "DSKA0080.CQM.lz", "DSKA0081.CQM.lz", "DSKA0082.CQM.lz",
            "DSKA0083.CQM.lz", "DSKA0084.CQM.lz", "DSKA0085.CQM.lz", "DSKA0105.CQM.lz", "DSKA0106.CQM.lz",
            "DSKA0107.CQM.lz", "DSKA0108.CQM.lz", "DSKA0109.CQM.lz", "DSKA0110.CQM.lz", "DSKA0111.CQM.lz",
            "DSKA0112.CQM.lz", "DSKA0113.CQM.lz", "DSKA0114.CQM.lz", "DSKA0115.CQM.lz", "DSKA0116.CQM.lz",
            "DSKA0117.CQM.lz", "DSKA0120.CQM.lz", "DSKA0121.CQM.lz", "DSKA0122.CQM.lz", "DSKA0123.CQM.lz",
            "DSKA0124.CQM.lz", "DSKA0125.CQM.lz", "DSKA0126.CQM.lz", "DSKA0147.CQM.lz", "DSKA0148.CQM.lz",
            "DSKA0149.CQM.lz", "DSKA0150.CQM.lz", "DSKA0151.CQM.lz", "DSKA0153.CQM.lz", "DSKA0158.CQM.lz",
            "DSKA0159.CQM.lz", "DSKA0162.CQM.lz", "DSKA0163.CQM.lz", "DSKA0164.CQM.lz", "DSKA0166.CQM.lz",
            "DSKA0167.CQM.lz", "DSKA0168.CQM.lz", "DSKA0169.CQM.lz", "DSKA0173.CQM.lz", "DSKA0174.CQM.lz",
            "DSKA0175.CQM.lz", "DSKA0180.CQM.lz", "DSKA0181.CQM.lz", "DSKA0182.CQM.lz", "DSKA0183.CQM.lz",
            "DSKA0184.CQM.lz", "DSKA0185.CQM.lz", "DSKA0186.CQM.lz", "DSKA0197.CQM.lz", "DSKA0198.CQM.lz",
            "DSKA0199.CQM.lz", "DSKA0200.CQM.lz", "DSKA0201.CQM.lz", "DSKA0202.CQM.lz", "DSKA0203.CQM.lz",
            "DSKA0204.CQM.lz", "DSKA0205.CQM.lz", "DSKA0206.CQM.lz", "DSKA0207.CQM.lz", "DSKA0209.CQM.lz",
            "DSKA0210.CQM.lz", "DSKA0211.CQM.lz", "DSKA0212.CQM.lz", "DSKA0213.CQM.lz", "DSKA0214.CQM.lz",
            "DSKA0215.CQM.lz", "DSKA0216.CQM.lz", "DSKA0221.CQM.lz", "DSKA0222.CQM.lz", "DSKA0225.CQM.lz",
            "DSKA0228.CQM.lz", "DSKA0232.CQM.lz", "DSKA0234.CQM.lz", "DSKA0240.CQM.lz", "DSKA0241.CQM.lz",
            "DSKA0242.CQM.lz", "DSKA0243.CQM.lz", "DSKA0244.CQM.lz", "DSKA0245.CQM.lz", "DSKA0246.CQM.lz",
            "DSKA0247.CQM.lz", "DSKA0248.CQM.lz", "DSKA0250.CQM.lz", "DSKA0251.CQM.lz", "DSKA0252.CQM.lz",
            "DSKA0253.CQM.lz", "DSKA0254.CQM.lz", "DSKA0258.CQM.lz", "DSKA0262.CQM.lz", "DSKA0263.CQM.lz",
            "DSKA0264.CQM.lz", "DSKA0265.CQM.lz", "DSKA0266.CQM.lz", "DSKA0267.CQM.lz", "DSKA0268.CQM.lz",
            "DSKA0269.CQM.lz", "DSKA0270.CQM.lz", "DSKA0271.CQM.lz", "DSKA0272.CQM.lz", "DSKA0273.CQM.lz",
            "DSKA0280.CQM.lz", "DSKA0281.CQM.lz", "DSKA0282.CQM.lz", "DSKA0283.CQM.lz", "DSKA0284.CQM.lz",
            "DSKA0285.CQM.lz", "DSKA0287.CQM.lz", "DSKA0288.CQM.lz", "DSKA0289.CQM.lz", "DSKA0290.CQM.lz",
            "DSKA0291.CQM.lz", "DSKA0299.CQM.lz", "DSKA0300.CQM.lz", "DSKA0301.CQM.lz", "DSKA0302.CQM.lz",
            "DSKA0303.CQM.lz", "DSKA0304.CQM.lz", "DSKA0305.CQM.lz", "DSKA0307.CQM.lz", "DSKA0308.CQM.lz",
            "DSKA0311.CQM.lz", "DSKA0314.CQM.lz", "DSKA0316.CQM.lz", "DSKA0317.CQM.lz", "DSKA0318.CQM.lz",
            "DSKA0319.CQM.lz", "DSKA0320.CQM.lz", "DSKA0322.CQM.lz", "mf2dd.cqm.lz", "mf2dd_fdformat_800.cqm.lz",
            "mf2dd_freedos.cqm.lz", "mf2hd_blind.cqm.lz", "mf2hd.cqm.lz", "mf2hd_fdformat_168.cqm.lz",
            "mf2hd_freedos.cqm.lz"
        };
        public override ulong[] _sectors => new ulong[]
        {
            // DSKA0000.CQM.lz
            2880,

            // DSKA0001.CQM.lz
            1600,

            // DSKA0002.CQM.lz
            1280,

            // DSKA0003.CQM.lz
            800,

            // DSKA0004.CQM.lz
            1280,

            // DSKA0006.CQM.lz
            360,

            // DSKA0009.CQM.lz
            2880,

            // DSKA0010.CQM.lz
            1440,

            // DSKA0011.CQM.lz
            1280,

            // DSKA0012.CQM.lz
            1600,

            // DSKA0013.CQM.lz
            1600,

            // DSKA0017.CQM.lz
            3040,

            // DSKA0018.CQM.lz
            1440,

            // DSKA0019.CQM.lz
            640,

            // DSKA0020.CQM.lz
            1440,

            // DSKA0021.CQM.lz
            3040,

            // DSKA0023.CQM.lz
            1280,

            // DSKA0024.CQM.lz
            2880,

            // DSKA0025.CQM.lz
            1440,

            // DSKA0026.CQM.lz
            800,

            // DSKA0027.CQM.lz
            640,

            // DSKA0028.CQM.lz
            1440,

            // DSKA0029.CQM.lz
            576,

            // DSKA0030.CQM.lz
            1440,

            // DSKA0031.CQM.lz
            640,

            // DSKA0032.CQM.lz
            640,

            // DSKA0033.CQM.lz
            1280,

            // DSKA0034.CQM.lz
            1280,

            // DSKA0035.CQM.lz
            320,

            // DSKA0036.CQM.lz
            320,

            // DSKA0037.CQM.lz
            360,

            // DSKA0038.CQM.lz
            360,

            // DSKA0039.CQM.lz
            640,

            // DSKA0040.CQM.lz
            640,

            // DSKA0041.CQM.lz
            640,

            // DSKA0042.CQM.lz
            640,

            // DSKA0043.CQM.lz
            720,

            // DSKA0044.CQM.lz
            720,

            // DSKA0045.CQM.lz
            2400,

            // DSKA0046.CQM.lz
            2460,

            // DSKA0047.CQM.lz
            1280,

            // DSKA0048.CQM.lz
            1440,

            // DSKA0049.CQM.lz
            1476,

            // DSKA0050.CQM.lz
            1600,

            // DSKA0051.CQM.lz
            1640,

            // DSKA0052.CQM.lz
            2880,

            // DSKA0053.CQM.lz
            2952,

            // DSKA0054.CQM.lz
            3200,

            // DSKA0055.CQM.lz
            3280,

            // DSKA0056.CQM.lz
            3360,

            // DSKA0057.CQM.lz
            3444,

            // DSKA0058.CQM.lz
            3486,

            // DSKA0059.CQM.lz
            3528,

            // DSKA0060.CQM.lz
            3570,

            // DSKA0069.CQM.lz
            1440,

            // DSKA0070.CQM.lz
            1640,

            // DSKA0073.CQM.lz
            320,

            // DSKA0074.CQM.lz
            360,

            // DSKA0075.CQM.lz
            640,

            // DSKA0076.CQM.lz
            720,

            // DSKA0077.CQM.lz
            800,

            // DSKA0078.CQM.lz
            2400,

            // DSKA0080.CQM.lz
            1440,

            // DSKA0081.CQM.lz
            1600,

            // DSKA0082.CQM.lz
            2880,

            // DSKA0083.CQM.lz
            2988,

            // DSKA0084.CQM.lz
            3360,

            // DSKA0085.CQM.lz
            3486,

            // DSKA0105.CQM.lz
            400,

            // DSKA0106.CQM.lz
            410,

            // DSKA0107.CQM.lz
            800,

            // DSKA0108.CQM.lz
            820,

            // DSKA0109.CQM.lz
            1600,

            // DSKA0110.CQM.lz
            1640,

            // DSKA0111.CQM.lz
            2880,

            // DSKA0112.CQM.lz
            2952,

            // DSKA0113.CQM.lz
            1600,

            // DSKA0114.CQM.lz
            1640,

            // DSKA0115.CQM.lz
            2952,

            // DSKA0116.CQM.lz
            3200,

            // DSKA0117.CQM.lz
            3280,

            // DSKA0120.CQM.lz
            320,

            // DSKA0121.CQM.lz
            360,

            // DSKA0122.CQM.lz
            640,

            // DSKA0123.CQM.lz
            720,

            // DSKA0124.CQM.lz
            2400,

            // DSKA0125.CQM.lz
            1440,

            // DSKA0126.CQM.lz
            2880,

            // DSKA0147.CQM.lz
            320,

            // DSKA0148.CQM.lz
            640,

            // DSKA0149.CQM.lz
            200,

            // DSKA0150.CQM.lz
            400,

            // DSKA0151.CQM.lz
            360,

            // DSKA0153.CQM.lz
            360,

            // DSKA0158.CQM.lz
            1280,

            // DSKA0159.CQM.lz
            640,

            // DSKA0162.CQM.lz
            320,

            // DSKA0163.CQM.lz
            720,

            // DSKA0164.CQM.lz
            820,

            // DSKA0166.CQM.lz
            1440,

            // DSKA0167.CQM.lz
            800,

            // DSKA0168.CQM.lz
            2400,

            // DSKA0169.CQM.lz
            2880,

            // DSKA0173.CQM.lz
            720,

            // DSKA0174.CQM.lz
            1440,

            // DSKA0175.CQM.lz
            1600,

            // DSKA0180.CQM.lz
            3200,

            // DSKA0181.CQM.lz
            3360,

            // DSKA0182.CQM.lz
            3402,

            // DSKA0183.CQM.lz
            3444,

            // DSKA0184.CQM.lz
            1280,

            // DSKA0185.CQM.lz
            800,

            // DSKA0186.CQM.lz
            160,

            // DSKA0197.CQM.lz
            600,

            // DSKA0198.CQM.lz
            1200,

            // DSKA0199.CQM.lz
            2400,

            // DSKA0200.CQM.lz
            1280,

            // DSKA0201.CQM.lz
            800,

            // DSKA0202.CQM.lz
            1280,

            // DSKA0203.CQM.lz
            1280,

            // DSKA0204.CQM.lz
            360,

            // DSKA0205.CQM.lz
            1476,

            // DSKA0206.CQM.lz
            1440,

            // DSKA0207.CQM.lz
            3040,

            // DSKA0209.CQM.lz
            1600,

            // DSKA0210.CQM.lz
            1600,

            // DSKA0211.CQM.lz
            1440,

            // DSKA0212.CQM.lz
            1440,

            // DSKA0213.CQM.lz
            800,

            // DSKA0214.CQM.lz
            1280,

            // DSKA0215.CQM.lz
            1280,

            // DSKA0216.CQM.lz
            2880,

            // DSKA0221.CQM.lz
            5120,

            // DSKA0222.CQM.lz
            1600,

            // DSKA0225.CQM.lz
            990,

            // DSKA0228.CQM.lz
            1050,

            // DSKA0232.CQM.lz
            621,

            // DSKA0234.CQM.lz
            2400,

            // DSKA0240.CQM.lz
            720,

            // DSKA0241.CQM.lz
            714,

            // DSKA0242.CQM.lz
            1232,

            // DSKA0243.CQM.lz
            1280,

            // DSKA0244.CQM.lz
            1280,

            // DSKA0245.CQM.lz
            1600,

            // DSKA0246.CQM.lz
            1600,

            // DSKA0247.CQM.lz
            1280,

            // DSKA0248.CQM.lz
            1280,

            // DSKA0250.CQM.lz
            640,

            // DSKA0251.CQM.lz
            2560,

            // DSKA0252.CQM.lz
            1280,

            // DSKA0253.CQM.lz
            640,

            // DSKA0254.CQM.lz
            360,

            // DSKA0258.CQM.lz
            1232,

            // DSKA0262.CQM.lz
            1440,

            // DSKA0263.CQM.lz
            1600,

            // DSKA0264.CQM.lz
            1640,

            // DSKA0265.CQM.lz
            1660,

            // DSKA0266.CQM.lz
            2880,

            // DSKA0267.CQM.lz
            3040,

            // DSKA0268.CQM.lz
            3200,

            // DSKA0269.CQM.lz
            3280,

            // DSKA0270.CQM.lz
            3320,

            // DSKA0271.CQM.lz
            3360,

            // DSKA0272.CQM.lz
            3444,

            // DSKA0273.CQM.lz
            3486,

            // DSKA0280.CQM.lz
            360,

            // DSKA0281.CQM.lz
            400,

            // DSKA0282.CQM.lz
            640,

            // DSKA0283.CQM.lz
            720,

            // DSKA0284.CQM.lz
            800,

            // DSKA0285.CQM.lz
            840,

            // DSKA0287.CQM.lz
            1440,

            // DSKA0288.CQM.lz
            1494,

            // DSKA0289.CQM.lz
            1600,

            // DSKA0290.CQM.lz
            1640,

            // DSKA0291.CQM.lz
            1660,

            // DSKA0299.CQM.lz
            320,

            // DSKA0300.CQM.lz
            360,

            // DSKA0301.CQM.lz
            640,

            // DSKA0302.CQM.lz
            720,

            // DSKA0303.CQM.lz
            2400,

            // DSKA0304.CQM.lz
            1440,

            // DSKA0305.CQM.lz
            2880,

            // DSKA0307.CQM.lz
            840,

            // DSKA0308.CQM.lz
            1600,

            // DSKA0311.CQM.lz
            3444,

            // DSKA0314.CQM.lz
            1440,

            // DSKA0316.CQM.lz
            2880,

            // DSKA0317.CQM.lz
            3360,

            // DSKA0318.CQM.lz
            3444,

            // DSKA0319.CQM.lz
            3360,

            // DSKA0320.CQM.lz
            3360,

            // DSKA0322.CQM.lz
            1386,

            // mf2dd.cqm.lz
            1440,

            // mf2dd_fdformat_800.cqm.lz
            1600,

            // mf2dd_freedos.cqm.lz
            1600,

            // mf2hd_blind.cqm.lz
            2880,

            // mf2hd.cqm.lz
            2880,

            // mf2hd_fdformat_168.cqm.lz
            3360,

            // mf2hd_freedos.cqm.lz
            3360
        };
        public override uint[] _sectorSize => new uint[]
        {
            // DSKA0000.CQM.lz
            512,

            // DSKA0001.CQM.lz
            512,

            // DSKA0002.CQM.lz
            1024,

            // DSKA0003.CQM.lz
            1024,

            // DSKA0004.CQM.lz
            1024,

            // DSKA0006.CQM.lz
            512,

            // DSKA0009.CQM.lz
            512,

            // DSKA0010.CQM.lz
            512,

            // DSKA0011.CQM.lz
            1024,

            // DSKA0012.CQM.lz
            512,

            // DSKA0013.CQM.lz
            512,

            // DSKA0017.CQM.lz
            512,

            // DSKA0018.CQM.lz
            512,

            // DSKA0019.CQM.lz
            1024,

            // DSKA0020.CQM.lz
            512,

            // DSKA0021.CQM.lz
            512,

            // DSKA0023.CQM.lz
            1024,

            // DSKA0024.CQM.lz
            512,

            // DSKA0025.CQM.lz
            512,

            // DSKA0026.CQM.lz
            1024,

            // DSKA0027.CQM.lz
            1024,

            // DSKA0028.CQM.lz
            512,

            // DSKA0029.CQM.lz
            1024,

            // DSKA0030.CQM.lz
            512,

            // DSKA0031.CQM.lz
            256,

            // DSKA0032.CQM.lz
            256,

            // DSKA0033.CQM.lz
            256,

            // DSKA0034.CQM.lz
            256,

            // DSKA0035.CQM.lz
            512,

            // DSKA0036.CQM.lz
            512,

            // DSKA0037.CQM.lz
            512,

            // DSKA0038.CQM.lz
            512,

            // DSKA0039.CQM.lz
            512,

            // DSKA0040.CQM.lz
            512,

            // DSKA0041.CQM.lz
            512,

            // DSKA0042.CQM.lz
            512,

            // DSKA0043.CQM.lz
            512,

            // DSKA0044.CQM.lz
            512,

            // DSKA0045.CQM.lz
            512,

            // DSKA0046.CQM.lz
            512,

            // DSKA0047.CQM.lz
            512,

            // DSKA0048.CQM.lz
            512,

            // DSKA0049.CQM.lz
            512,

            // DSKA0050.CQM.lz
            512,

            // DSKA0051.CQM.lz
            512,

            // DSKA0052.CQM.lz
            512,

            // DSKA0053.CQM.lz
            512,

            // DSKA0054.CQM.lz
            512,

            // DSKA0055.CQM.lz
            512,

            // DSKA0056.CQM.lz
            512,

            // DSKA0057.CQM.lz
            512,

            // DSKA0058.CQM.lz
            512,

            // DSKA0059.CQM.lz
            512,

            // DSKA0060.CQM.lz
            512,

            // DSKA0069.CQM.lz
            512,

            // DSKA0070.CQM.lz
            512,

            // DSKA0073.CQM.lz
            512,

            // DSKA0074.CQM.lz
            512,

            // DSKA0075.CQM.lz
            512,

            // DSKA0076.CQM.lz
            512,

            // DSKA0077.CQM.lz
            512,

            // DSKA0078.CQM.lz
            512,

            // DSKA0080.CQM.lz
            512,

            // DSKA0081.CQM.lz
            512,

            // DSKA0082.CQM.lz
            512,

            // DSKA0083.CQM.lz
            512,

            // DSKA0084.CQM.lz
            512,

            // DSKA0085.CQM.lz
            512,

            // DSKA0105.CQM.lz
            512,

            // DSKA0106.CQM.lz
            512,

            // DSKA0107.CQM.lz
            512,

            // DSKA0108.CQM.lz
            512,

            // DSKA0109.CQM.lz
            512,

            // DSKA0110.CQM.lz
            512,

            // DSKA0111.CQM.lz
            512,

            // DSKA0112.CQM.lz
            512,

            // DSKA0113.CQM.lz
            512,

            // DSKA0114.CQM.lz
            512,

            // DSKA0115.CQM.lz
            512,

            // DSKA0116.CQM.lz
            512,

            // DSKA0117.CQM.lz
            512,

            // DSKA0120.CQM.lz
            512,

            // DSKA0121.CQM.lz
            512,

            // DSKA0122.CQM.lz
            512,

            // DSKA0123.CQM.lz
            512,

            // DSKA0124.CQM.lz
            512,

            // DSKA0125.CQM.lz
            512,

            // DSKA0126.CQM.lz
            512,

            // DSKA0147.CQM.lz
            512,

            // DSKA0148.CQM.lz
            512,

            // DSKA0149.CQM.lz
            1024,

            // DSKA0150.CQM.lz
            1024,

            // DSKA0151.CQM.lz
            512,

            // DSKA0153.CQM.lz
            512,

            // DSKA0158.CQM.lz
            256,

            // DSKA0159.CQM.lz
            256,

            // DSKA0162.CQM.lz
            512,

            // DSKA0163.CQM.lz
            512,

            // DSKA0164.CQM.lz
            512,

            // DSKA0166.CQM.lz
            512,

            // DSKA0167.CQM.lz
            1024,

            // DSKA0168.CQM.lz
            512,

            // DSKA0169.CQM.lz
            512,

            // DSKA0173.CQM.lz
            512,

            // DSKA0174.CQM.lz
            512,

            // DSKA0175.CQM.lz
            512,

            // DSKA0180.CQM.lz
            512,

            // DSKA0181.CQM.lz
            512,

            // DSKA0182.CQM.lz
            512,

            // DSKA0183.CQM.lz
            512,

            // DSKA0184.CQM.lz
            1024,

            // DSKA0185.CQM.lz
            2048,

            // DSKA0186.CQM.lz
            8192,

            // DSKA0197.CQM.lz
            256,

            // DSKA0198.CQM.lz
            256,

            // DSKA0199.CQM.lz
            256,

            // DSKA0200.CQM.lz
            1024,

            // DSKA0201.CQM.lz
            1024,

            // DSKA0202.CQM.lz
            1024,

            // DSKA0203.CQM.lz
            1024,

            // DSKA0204.CQM.lz
            512,

            // DSKA0205.CQM.lz
            512,

            // DSKA0206.CQM.lz
            512,

            // DSKA0207.CQM.lz
            512,

            // DSKA0209.CQM.lz
            512,

            // DSKA0210.CQM.lz
            512,

            // DSKA0211.CQM.lz
            512,

            // DSKA0212.CQM.lz
            512,

            // DSKA0213.CQM.lz
            1024,

            // DSKA0214.CQM.lz
            1024,

            // DSKA0215.CQM.lz
            1024,

            // DSKA0216.CQM.lz
            512,

            // DSKA0221.CQM.lz
            256,

            // DSKA0222.CQM.lz
            512,

            // DSKA0225.CQM.lz
            256,

            // DSKA0228.CQM.lz
            256,

            // DSKA0232.CQM.lz
            512,

            // DSKA0234.CQM.lz
            256,

            // DSKA0240.CQM.lz
            256,

            // DSKA0241.CQM.lz
            256,

            // DSKA0242.CQM.lz
            1024,

            // DSKA0243.CQM.lz
            256,

            // DSKA0244.CQM.lz
            256,

            // DSKA0245.CQM.lz
            512,

            // DSKA0246.CQM.lz
            512,

            // DSKA0247.CQM.lz
            256,

            // DSKA0248.CQM.lz
            256,

            // DSKA0250.CQM.lz
            1024,

            // DSKA0251.CQM.lz
            256,

            // DSKA0252.CQM.lz
            256,

            // DSKA0253.CQM.lz
            1024,

            // DSKA0254.CQM.lz
            512,

            // DSKA0258.CQM.lz
            1024,

            // DSKA0262.CQM.lz
            512,

            // DSKA0263.CQM.lz
            512,

            // DSKA0264.CQM.lz
            512,

            // DSKA0265.CQM.lz
            512,

            // DSKA0266.CQM.lz
            512,

            // DSKA0267.CQM.lz
            512,

            // DSKA0268.CQM.lz
            512,

            // DSKA0269.CQM.lz
            512,

            // DSKA0270.CQM.lz
            512,

            // DSKA0271.CQM.lz
            512,

            // DSKA0272.CQM.lz
            512,

            // DSKA0273.CQM.lz
            512,

            // DSKA0280.CQM.lz
            512,

            // DSKA0281.CQM.lz
            512,

            // DSKA0282.CQM.lz
            512,

            // DSKA0283.CQM.lz
            512,

            // DSKA0284.CQM.lz
            512,

            // DSKA0285.CQM.lz
            512,

            // DSKA0287.CQM.lz
            512,

            // DSKA0288.CQM.lz
            512,

            // DSKA0289.CQM.lz
            512,

            // DSKA0290.CQM.lz
            512,

            // DSKA0291.CQM.lz
            512,

            // DSKA0299.CQM.lz
            512,

            // DSKA0300.CQM.lz
            512,

            // DSKA0301.CQM.lz
            512,

            // DSKA0302.CQM.lz
            512,

            // DSKA0303.CQM.lz
            512,

            // DSKA0304.CQM.lz
            512,

            // DSKA0305.CQM.lz
            512,

            // DSKA0307.CQM.lz
            512,

            // DSKA0308.CQM.lz
            512,

            // DSKA0311.CQM.lz
            512,

            // DSKA0314.CQM.lz
            512,

            // DSKA0316.CQM.lz
            512,

            // DSKA0317.CQM.lz
            512,

            // DSKA0318.CQM.lz
            512,

            // DSKA0319.CQM.lz
            512,

            // DSKA0320.CQM.lz
            512,

            // DSKA0322.CQM.lz
            512,

            // mf2dd.cqm.lz
            512,

            // mf2dd_fdformat_800.cqm.lz
            512,

            // mf2dd_freedos.cqm.lz
            512,

            // mf2hd_blind.cqm.lz
            512,

            // mf2hd.cqm.lz
            512,

            // mf2hd_fdformat_168.cqm.lz
            512,

            // mf2hd_freedos.cqm.lz
            512
        };
        public override MediaType[] _mediaTypes => new[]
        {
            // DSKA0000.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0001.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0002.CQM.lz
            MediaType.Unknown,

            // DSKA0003.CQM.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0004.CQM.lz
            MediaType.Unknown,

            // DSKA0006.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0009.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0010.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0011.CQM.lz
            MediaType.Unknown,

            // DSKA0012.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0013.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0017.CQM.lz
            MediaType.XDF_525,

            // DSKA0018.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0019.CQM.lz
            MediaType.Unknown,

            // DSKA0020.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0021.CQM.lz
            MediaType.XDF_525,

            // DSKA0023.CQM.lz
            MediaType.Unknown,

            // DSKA0024.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0025.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0026.CQM.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0027.CQM.lz
            MediaType.Unknown,

            // DSKA0028.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0029.CQM.lz
            MediaType.Unknown,

            // DSKA0030.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0031.CQM.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0032.CQM.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0033.CQM.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0034.CQM.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0035.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0036.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0037.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0038.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0039.CQM.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0040.CQM.lz
            MediaType.DOS_35_SS_DD_8,

            // DSKA0041.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0042.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0043.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0044.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0045.CQM.lz
            MediaType.NEC_35_HD_15,

            // DSKA0046.CQM.lz
            MediaType.Unknown,

            // DSKA0047.CQM.lz
            MediaType.DOS_35_DS_DD_8,

            // DSKA0048.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0049.CQM.lz
            MediaType.Unknown,

            // DSKA0050.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0051.CQM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0052.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0053.CQM.lz
            MediaType.Unknown,

            // DSKA0054.CQM.lz
            MediaType.Unknown,

            // DSKA0055.CQM.lz
            MediaType.Unknown,

            // DSKA0056.CQM.lz
            MediaType.DMF,

            // DSKA0057.CQM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0058.CQM.lz
            MediaType.Unknown,

            // DSKA0059.CQM.lz
            MediaType.Unknown,

            // DSKA0060.CQM.lz
            MediaType.Unknown,

            // DSKA0069.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0070.CQM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0073.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0074.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0075.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0076.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0077.CQM.lz
            MediaType.Unknown,

            // DSKA0078.CQM.lz
            MediaType.DOS_525_HD,

            // DSKA0080.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0081.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0082.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0083.CQM.lz
            MediaType.Unknown,

            // DSKA0084.CQM.lz
            MediaType.DMF,

            // DSKA0085.CQM.lz
            MediaType.Unknown,

            // DSKA0105.CQM.lz
            MediaType.Unknown,

            // DSKA0106.CQM.lz
            MediaType.Unknown,

            // DSKA0107.CQM.lz
            MediaType.Unknown,

            // DSKA0108.CQM.lz
            MediaType.Unknown,

            // DSKA0109.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0110.CQM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0111.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0112.CQM.lz
            MediaType.Unknown,

            // DSKA0113.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0114.CQM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0115.CQM.lz
            MediaType.Unknown,

            // DSKA0116.CQM.lz
            MediaType.Unknown,

            // DSKA0117.CQM.lz
            MediaType.Unknown,

            // DSKA0120.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0121.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0122.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0123.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0124.CQM.lz
            MediaType.DOS_525_HD,

            // DSKA0125.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0126.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0147.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0148.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0149.CQM.lz
            MediaType.Unknown,

            // DSKA0150.CQM.lz
            MediaType.Unknown,

            // DSKA0151.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0153.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0158.CQM.lz
            MediaType.Unknown,

            // DSKA0159.CQM.lz
            MediaType.ACORN_525_SS_DD_40,

            // DSKA0162.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0163.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0164.CQM.lz
            MediaType.Unknown,

            // DSKA0166.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0167.CQM.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0168.CQM.lz
            MediaType.DOS_525_HD,

            // DSKA0169.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0173.CQM.lz
            MediaType.DOS_35_SS_DD_9,

            // DSKA0174.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0175.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0180.CQM.lz
            MediaType.Unknown,

            // DSKA0181.CQM.lz
            MediaType.DMF,

            // DSKA0182.CQM.lz
            MediaType.Unknown,

            // DSKA0183.CQM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0184.CQM.lz
            MediaType.Unknown,

            // DSKA0185.CQM.lz
            MediaType.Unknown,

            // DSKA0186.CQM.lz
            MediaType.Unknown,

            // DSKA0197.CQM.lz
            MediaType.Unknown,

            // DSKA0198.CQM.lz
            MediaType.Unknown,

            // DSKA0199.CQM.lz
            MediaType.Unknown,

            // DSKA0200.CQM.lz
            MediaType.Unknown,

            // DSKA0201.CQM.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0202.CQM.lz
            MediaType.Unknown,

            // DSKA0203.CQM.lz
            MediaType.Unknown,

            // DSKA0204.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0205.CQM.lz
            MediaType.Unknown,

            // DSKA0206.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0207.CQM.lz
            MediaType.XDF_525,

            // DSKA0209.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0210.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0211.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0212.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0213.CQM.lz
            MediaType.ACORN_35_DS_DD,

            // DSKA0214.CQM.lz
            MediaType.Unknown,

            // DSKA0215.CQM.lz
            MediaType.Unknown,

            // DSKA0216.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0221.CQM.lz
            MediaType.Unknown,

            // DSKA0222.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0225.CQM.lz
            MediaType.Unknown,

            // DSKA0228.CQM.lz
            MediaType.Unknown,

            // DSKA0232.CQM.lz
            MediaType.Unknown,

            // DSKA0234.CQM.lz
            MediaType.Unknown,

            // DSKA0240.CQM.lz
            MediaType.ATARI_525_DD,

            // DSKA0241.CQM.lz
            MediaType.Unknown,

            // DSKA0242.CQM.lz
            MediaType.NEC_35_HD_8,

            // DSKA0243.CQM.lz
            MediaType.Unknown,

            // DSKA0244.CQM.lz
            MediaType.Unknown,

            // DSKA0245.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0246.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0247.CQM.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0248.CQM.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0250.CQM.lz
            MediaType.Unknown,

            // DSKA0251.CQM.lz
            MediaType.ACORN_525_DS_DD,

            // DSKA0252.CQM.lz
            MediaType.ACORN_525_SS_DD_80,

            // DSKA0253.CQM.lz
            MediaType.Unknown,

            // DSKA0254.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0258.CQM.lz
            MediaType.SHARP_525,

            // DSKA0262.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0263.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0264.CQM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0265.CQM.lz
            MediaType.Unknown,

            // DSKA0266.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0267.CQM.lz
            MediaType.XDF_525,

            // DSKA0268.CQM.lz
            MediaType.Unknown,

            // DSKA0269.CQM.lz
            MediaType.Unknown,

            // DSKA0270.CQM.lz
            MediaType.Unknown,

            // DSKA0271.CQM.lz
            MediaType.DMF,

            // DSKA0272.CQM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0273.CQM.lz
            MediaType.Unknown,

            // DSKA0280.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0281.CQM.lz
            MediaType.Unknown,

            // DSKA0282.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0283.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0284.CQM.lz
            MediaType.Unknown,

            // DSKA0285.CQM.lz
            MediaType.Unknown,

            // DSKA0287.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0288.CQM.lz
            MediaType.Unknown,

            // DSKA0289.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0290.CQM.lz
            MediaType.FDFORMAT_35_DD,

            // DSKA0291.CQM.lz
            MediaType.Unknown,

            // DSKA0299.CQM.lz
            MediaType.DOS_525_SS_DD_8,

            // DSKA0300.CQM.lz
            MediaType.DOS_525_SS_DD_9,

            // DSKA0301.CQM.lz
            MediaType.DOS_525_DS_DD_8,

            // DSKA0302.CQM.lz
            MediaType.DOS_525_DS_DD_9,

            // DSKA0303.CQM.lz
            MediaType.DOS_525_HD,

            // DSKA0304.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0305.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0307.CQM.lz
            MediaType.Unknown,

            // DSKA0308.CQM.lz
            MediaType.CBM_35_DD,

            // DSKA0311.CQM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0314.CQM.lz
            MediaType.DOS_35_DS_DD_9,

            // DSKA0316.CQM.lz
            MediaType.DOS_35_HD,

            // DSKA0317.CQM.lz
            MediaType.DMF,

            // DSKA0318.CQM.lz
            MediaType.FDFORMAT_35_HD,

            // DSKA0319.CQM.lz
            MediaType.DMF,

            // DSKA0320.CQM.lz
            MediaType.DMF,

            // DSKA0322.CQM.lz
            MediaType.Unknown,

            // mf2dd.cqm.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2dd_fdformat_800.cqm.lz
            MediaType.CBM_35_DD,

            // mf2dd_freedos.cqm.lz
            MediaType.CBM_35_DD,

            // mf2hd_blind.cqm.lz
            MediaType.DOS_35_HD,

            // mf2hd.cqm.lz
            MediaType.DOS_35_HD,

            // mf2hd_fdformat_168.cqm.lz
            MediaType.DMF,

            // mf2hd_freedos.cqm.lz
            MediaType.DMF
        };
        public override string[] _md5S => new[]
        {
            // DSKA0000.CQM.lz
            "e8bbbd22db87181974e12ba0227ea011",

            // DSKA0001.CQM.lz
            "9f5635f3df4d880a500910b0ad1ab535",

            // DSKA0002.CQM.lz
            "9176f59e9205846b6212e084f46ed95c",

            // DSKA0003.CQM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0004.CQM.lz
            "1045bfd216ae1ae480dd0ef626f5ff39",

            // DSKA0006.CQM.lz
            "46fce47baf08c6f093f2c355a603543d",

            // DSKA0009.CQM.lz
            "95ea232f59e44db374b994cfe7f1c07f",

            // DSKA0010.CQM.lz
            "9e2b01f4397db2a6c76e2bc267df37b3",

            // DSKA0011.CQM.lz
            "dbbf55398d930e14c2b0a035dd1277b9",

            // DSKA0012.CQM.lz
            "656002e6e620cb3b73c27f4c21d32edb",

            // DSKA0013.CQM.lz
            "1244cc2c101c66e6bb4ad5183b356b19",

            // DSKA0017.CQM.lz
            "8cad624afc06ab756f9800eba22ee886",

            // DSKA0018.CQM.lz
            "84cce7b4d8c8e21040163cd2d03a730c",

            // DSKA0019.CQM.lz
            "76a1ef9485ffd5da1e9836725e375ada",

            // DSKA0020.CQM.lz
            "d236783dfd1dc29f350c51949b1e9e68",

            // DSKA0021.CQM.lz
            "6915f208cdda762eea2fe64ad754e72f",

            // DSKA0023.CQM.lz
            "b52f26c3c5b9b2cfc93a287a7fca3548",

            // DSKA0024.CQM.lz
            "2302991363cb3681cffdc4388915b51e",

            // DSKA0025.CQM.lz
            "4e4cafed1cc22ea72201169427e5e1b6",

            // DSKA0026.CQM.lz
            "a579b349a5a24218d59a44e36bdb1333",

            // DSKA0027.CQM.lz
            "3135430552171a832339a8a93d44cc90",

            // DSKA0028.CQM.lz
            "1a4c7487382c98b7bc74623ddfb488e6",

            // DSKA0029.CQM.lz
            "a8a9caa886a338b66181cfa21db6b620",

            // DSKA0030.CQM.lz
            "af83d011608042d35021e39aa5e10b2f",

            // DSKA0031.CQM.lz
            "e640835966327f3f662e1db8e0575510",

            // DSKA0032.CQM.lz
            "ff3534234d1d2dd88bf6e83be23d9227",

            // DSKA0033.CQM.lz
            "dfaff34a6556b515642f1e54f839b02e",

            // DSKA0034.CQM.lz
            "ca8f5c7f9ed161b03ccb166eb9d62146",

            // DSKA0035.CQM.lz
            "6642c1a32d2c58e93481d664974fc202",

            // DSKA0036.CQM.lz
            "846f01b8b60cb3c775bd66419e977926",

            // DSKA0037.CQM.lz
            "5101f89850dc28efbcfb7622086a9ddf",

            // DSKA0038.CQM.lz
            "8e570be2ed1f00ddea82e50a2d9c446a",

            // DSKA0039.CQM.lz
            "abba2a1ddd60a649047a9c44d94bbeae",

            // DSKA0040.CQM.lz
            "e3bc48bec81be5b35be73d41fdffd2ab",

            // DSKA0041.CQM.lz
            "43b5068af9d016d1432eb2e12d2b802a",

            // DSKA0042.CQM.lz
            "5bf2ad4dc300592604b6e32f8b8e2656",

            // DSKA0043.CQM.lz
            "cb9a832ca6a4097b8ccc30d2108e1f7d",

            // DSKA0044.CQM.lz
            "56d181a6bb8713e6b2854fe8887faab6",

            // DSKA0045.CQM.lz
            "41aef7cff26aefda1add8d49c5b962c2",

            // DSKA0046.CQM.lz
            "2437c5f089f1cba3866b36360b016f16",

            // DSKA0047.CQM.lz
            "bdaa8f17373b265830fdf3a06b794367",

            // DSKA0048.CQM.lz
            "629932c285478d0540ff7936aa008351",

            // DSKA0049.CQM.lz
            "7a2abef5d4701e2e49abb05af8d4da50",

            // DSKA0050.CQM.lz
            "e3507522c914264f44fb2c92c3170c09",

            // DSKA0051.CQM.lz
            "824fe65dbb1a42b6b94f05405ef984f2",

            // DSKA0052.CQM.lz
            "1a8c2e78e7132cf9ba5d6c2b75876be0",

            // DSKA0053.CQM.lz
            "936b20bb0966fe693b4d5e2353e24846",

            // DSKA0054.CQM.lz
            "803b01a0b440c2837d37c21308f30cd5",

            // DSKA0055.CQM.lz
            "aa0d31f914760cc4cde75479779ebed6",

            // DSKA0056.CQM.lz
            "31269ed6464302ae26d22b7c87bceb23",

            // DSKA0057.CQM.lz
            "5e413433c54f48978d281c6e66d1106e",

            // DSKA0058.CQM.lz
            "4fc28b0128543b2eb70f6432c4c8a980",

            // DSKA0059.CQM.lz
            "24a7459d080cea3a60d131b8fd7dc5d1",

            // DSKA0060.CQM.lz
            "2031b1e16ee2defc0d15f732f633df33",

            // DSKA0069.CQM.lz
            "5fc19ca552b6db957061e9a1750394d2",

            // DSKA0070.CQM.lz
            "d1e978b679c63a218c3f77a7ca2c7206",

            // DSKA0073.CQM.lz
            "a33b46f042b78fe3d0b3c5dbb3908a93",

            // DSKA0074.CQM.lz
            "565d3c001cbb532154aa5d3c65b2439c",

            // DSKA0075.CQM.lz
            "e60442c3ebd72c99bdd7545fdba59613",

            // DSKA0076.CQM.lz
            "058a33a129539285c9b64010496af52f",

            // DSKA0077.CQM.lz
            "0726ecbc38965d30a6222c3e74cd1aa3",

            // DSKA0078.CQM.lz
            "c9a193837db7d8a5eb025eb41e8a76d7",

            // DSKA0080.CQM.lz
            "c38d69ac88520f14fcc6d6ced22b065d",

            // DSKA0081.CQM.lz
            "91d51964e1e64ef3f6f622fa19aa833c",

            // DSKA0082.CQM.lz
            "db36d9651c952ff679ec33223c8db2d3",

            // DSKA0083.CQM.lz
            "5f1d98806309aee7f81de72e51e6d386",

            // DSKA0084.CQM.lz
            "1207a1cc7ff73d4f74c8984b4e7db33f",

            // DSKA0085.CQM.lz
            "c97a3081fd25474b6b7945b8572d5ab8",

            // DSKA0105.CQM.lz
            "d40a99cb549fcfb26fcf9ef01b5dfca7",

            // DSKA0106.CQM.lz
            "7b41dd9ca7eb32828960eb1417a6092a",

            // DSKA0107.CQM.lz
            "126dfd25363c076727dfaab03955c931",

            // DSKA0108.CQM.lz
            "e6492aac144f5f6f593b84c64680cf64",

            // DSKA0109.CQM.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0110.CQM.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0111.CQM.lz
            "f01541de322c8d6d7321084d7a245e7b",

            // DSKA0112.CQM.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0113.CQM.lz
            "7973e569ed93beb1ece2e84a5ef3a8d1",

            // DSKA0114.CQM.lz
            "a793047503af08e83361427b3e2806e0",

            // DSKA0115.CQM.lz
            "ba6ec1652ff41bcc687aaf9c4e32dc18",

            // DSKA0116.CQM.lz
            "6631b66fdfd89319323771c41334c7ba",

            // DSKA0117.CQM.lz
            "56471a253f4d6803b634e2bbff6c0931",

            // DSKA0120.CQM.lz
            "7d36aee5a3071ff75b979f3acb649c40",

            // DSKA0121.CQM.lz
            "0ccb62039363ab544c69eca229a17fae",

            // DSKA0122.CQM.lz
            "7851d31fad9302ff45d3ded4fba25387",

            // DSKA0123.CQM.lz
            "915b08c82591e8488320e001b7303b6d",

            // DSKA0124.CQM.lz
            "5e5ea6fe9adf842221fdc60e56630405",

            // DSKA0125.CQM.lz
            "a22e254f7e3526ec30dc4915a19fcb52",

            // DSKA0126.CQM.lz
            "ddc6c1200c60e9f7796280f50c2e5283",

            // DSKA0147.CQM.lz
            "6efa72a33021d5051546c3e0dd4c3c09",

            // DSKA0148.CQM.lz
            "8b5acfd14818ff9556d3d81361ce4862",

            // DSKA0149.CQM.lz
            "cf42d08469548a31caf2649a1d08a85f",

            // DSKA0150.CQM.lz
            "62745e10683cf2ec1dac177535459891",

            // DSKA0151.CQM.lz
            "298c377de52947c472a85d281b6d3d4d",

            // DSKA0153.CQM.lz
            "298c377de52947c472a85d281b6d3d4d",

            // DSKA0158.CQM.lz
            "8b5acfd14818ff9556d3d81361ce4862",

            // DSKA0159.CQM.lz
            "6efa72a33021d5051546c3e0dd4c3c09",

            // DSKA0162.CQM.lz
            "e63014a4299f52f22e6e2c9609f51979",

            // DSKA0163.CQM.lz
            "be05d1ff10ef8b2220546c4db962ac9e",

            // DSKA0164.CQM.lz
            "32823b9009c99b6711e89336ad03ec7f",

            // DSKA0166.CQM.lz
            "1c8b03a8550ed3e70e1c78316aa445aa",

            // DSKA0167.CQM.lz
            "185bc63e4304a2d2554615362b2d25c5",

            // DSKA0168.CQM.lz
            "0bdf9130c07bb5d558a4705249f949d0",

            // DSKA0169.CQM.lz
            "2dafeddaa99e7dc0db5ef69e128f9c8e",

            // DSKA0173.CQM.lz
            "028769dc0abefab1740cc309432588b6",

            // DSKA0174.CQM.lz
            "152023525154b45ab26687190bac94db",

            // DSKA0175.CQM.lz
            "db38ecd93f28dd065927fed21917eed5",

            // DSKA0180.CQM.lz
            "f206c0caa4e0eda37233ab6e89ab5493",

            // DSKA0181.CQM.lz
            "554492a7b41f4cd9068a3a2b70eb0e5f",

            // DSKA0182.CQM.lz
            "865ad9072cb6c7458f7d86d7e9368622",

            // DSKA0183.CQM.lz
            "2461e458438f0033bc5811fd6958ad02",

            // DSKA0184.CQM.lz
            "606d5fbf174708c7ecfbfdd2a50fec9c",

            // DSKA0185.CQM.lz
            "6173d4c7b6a1addb14a4cbe088ede9d7",

            // DSKA0186.CQM.lz
            "5f47876d515d9495789f5e27ed313959",

            // DSKA0197.CQM.lz
            "65531301132413a81f3994eaf0b16f50",

            // DSKA0198.CQM.lz
            "a13fbf4d230f421d1bc4d21b714dc36b",

            // DSKA0199.CQM.lz
            "de0170cd10ddd839a63370355b2ba4ed",

            // DSKA0200.CQM.lz
            "1045bfd216ae1ae480dd0ef626f5ff39",

            // DSKA0201.CQM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0202.CQM.lz
            "1045bfd216ae1ae480dd0ef626f5ff39",

            // DSKA0203.CQM.lz
            "8a16a3008739516fc3ba4c878868d056",

            // DSKA0204.CQM.lz
            "46fce47baf08c6f093f2c355a603543d",

            // DSKA0205.CQM.lz
            "ee73a5d5c8dfac236baf7b99811696f9",

            // DSKA0206.CQM.lz
            "8245ddd644583bd78ac0638133c89824",

            // DSKA0207.CQM.lz
            "33c51a3d6f13cfedb5f08bf4c3cba7b9",

            // DSKA0209.CQM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0210.CQM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0211.CQM.lz
            "647f14749f59be471aac04a71a079a64",

            // DSKA0212.CQM.lz
            "517cdd5e42a4673f733d1aedfb46770f",

            // DSKA0213.CQM.lz
            "6ad92e9522e4ba902c01beecb5943bb1",

            // DSKA0214.CQM.lz
            "8e077143864bb20e36f25a4685860a1e",

            // DSKA0215.CQM.lz
            "9724c94417cef88b2ad2f3c1db9d8730",

            // DSKA0216.CQM.lz
            "40199611e6e75bbc37ad6c52a5b77eae",

            // DSKA0221.CQM.lz
            "f92b2e52259531d50bfb403dc1274ab1",

            // DSKA0222.CQM.lz
            "85574aebeef03eb355bf8541955d06ea",

            // DSKA0225.CQM.lz
            "dbcd4aa7c1c670a667c89b309bd9de42",

            // DSKA0228.CQM.lz
            "d88f521c048df99b8ef5f01a8a001455",

            // DSKA0232.CQM.lz
            "b76bd117ce24d933cdefe09b1de2164a",

            // DSKA0234.CQM.lz
            "a50f82253aa4d8dea4fb193d64a66778",

            // DSKA0240.CQM.lz
            "d1ab955f0961ab94e6cf69f78134a84b",

            // DSKA0241.CQM.lz
            "8b62738f15bcc916a668eaa67eec86e7",

            // DSKA0242.CQM.lz
            "87a432496cb23b5c2299545500df3553",

            // DSKA0243.CQM.lz
            "9866ab8e58fa4be25010184aec4ad3aa",

            // DSKA0244.CQM.lz
            "9dab329ae098b29889ab08278de38f95",

            // DSKA0245.CQM.lz
            "0d71b4952dadbfb1061acc1f4640c787",

            // DSKA0246.CQM.lz
            "af7ac6b5b9d2d57dad22dbb64ef7de38",

            // DSKA0247.CQM.lz
            "f8f81f945aaad6fbfe7e2db1905302c1",

            // DSKA0248.CQM.lz
            "f6f81c75b5ba45d91c1886c6dda9caee",

            // DSKA0250.CQM.lz
            "0b9cb8107cbb94c5e36aea438a04dc98",

            // DSKA0251.CQM.lz
            "d075e50705f4ddca7ba4dbc981ec1176",

            // DSKA0252.CQM.lz
            "9f86480c86bae33a5b444e4a7ed55048",

            // DSKA0253.CQM.lz
            "231891ccd0cc599cfe25419c669fc5f8",

            // DSKA0254.CQM.lz
            "5dc0d482a773043d8683a84c8220df95",

            // DSKA0258.CQM.lz
            "855943f9caecdcce9b06f0098d773c6b",

            // DSKA0262.CQM.lz
            "5ac0a9fc7337f761098f816359b0f6f7",

            // DSKA0263.CQM.lz
            "1ea6ec8e663218b1372048f6e25795b5",

            // DSKA0264.CQM.lz
            "77a1167b1b9043496e32b8578cde0ff0",

            // DSKA0265.CQM.lz
            "4b07d760d65f3f0f8ffa5f2b81cee907",

            // DSKA0266.CQM.lz
            "32c044c5c2b0bd13806149a759c14935",

            // DSKA0267.CQM.lz
            "8752095abc13dba3f3467669da333891",

            // DSKA0268.CQM.lz
            "aece7cd34bbba3e75307fa70404d9d30",

            // DSKA0269.CQM.lz
            "5289afb16a6e4a33213e3bcca56c6230",

            // DSKA0270.CQM.lz
            "1aef0a0ba233476db6567878c3c2b266",

            // DSKA0271.CQM.lz
            "b96596711f4d2ee85dfda0fe3b9f26c3",

            // DSKA0272.CQM.lz
            "a4f461af7fda5e93a7ab63fcbb7e7683",

            // DSKA0273.CQM.lz
            "8f7f7099d4475f6631fcf0a79b031d61",

            // DSKA0280.CQM.lz
            "4feeaf4b4ee5dad85db727fbbda4b6d1",

            // DSKA0281.CQM.lz
            "3c77ca681df78e4cd7baa162aa9b0859",

            // DSKA0282.CQM.lz
            "51da1f86c49657ffdb367bb2ddeb7990",

            // DSKA0283.CQM.lz
            "b81a4987f89936630b8ebc62e4bbce6e",

            // DSKA0284.CQM.lz
            "f76f92dd326c99c5efad5ee58daf72e1",

            // DSKA0285.CQM.lz
            "b6f2c10e42908e334025bc4ffd81e771",

            // DSKA0287.CQM.lz
            "f2f409ea2a62a7866fd2777cc4fc9739",

            // DSKA0288.CQM.lz
            "3e441d69cec5c3169274e1379de4af4b",

            // DSKA0289.CQM.lz
            "30a93f30dd4485c6fc037fe0775d3fc7",

            // DSKA0290.CQM.lz
            "e0caf02cce5597c98313bcc480366ec7",

            // DSKA0291.CQM.lz
            "4af4904d2b3c815da7bef7049209f5eb",

            // DSKA0299.CQM.lz
            "39bf5a98bcb2185d855ac06378febcfa",

            // DSKA0300.CQM.lz
            "dc20055b6e6fd6f8e1114d4be2effeed",

            // DSKA0301.CQM.lz
            "56af9256cf71d5aac5fd5d363674bc49",

            // DSKA0302.CQM.lz
            "bbba1e2d1418e05c3a4e7b4d585d160b",

            // DSKA0303.CQM.lz
            "bca3a045e81617f7f5ebb5a8818eac47",

            // DSKA0304.CQM.lz
            "a296663cb8e75e94603221352f29cfff",

            // DSKA0305.CQM.lz
            "ecda36ebf0e1100233cb0ec722c18583",

            // DSKA0307.CQM.lz
            "cef2f4fe9b1a32d5c0544f814e634264",

            // DSKA0308.CQM.lz
            "bbe58e26b8f8f822cd3edfd37a4e4924",

            // DSKA0311.CQM.lz
            "b9b6ebdf711364c979de7cf70c3a438a",

            // DSKA0314.CQM.lz
            "d37424f367f545acbb397f2bed766843",

            // DSKA0316.CQM.lz
            "9963dd6f19ce6bd56eabeccdfbbd821a",

            // DSKA0317.CQM.lz
            "acf6604559ae8217f7869823e2429024",

            // DSKA0318.CQM.lz
            "23bf2139cdfdc4c16db058fd31ea6481",

            // DSKA0319.CQM.lz
            "fa26adda0415f02057b113ad29c80c8d",

            // DSKA0320.CQM.lz
            "4f2a8d036fefd6c6c88d99eda3aa12b7",

            // DSKA0322.CQM.lz
            "e794a3ffa4069ea999fdf7146710fa9e",

            // mf2dd.cqm.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2dd_fdformat_800.cqm.lz
            "c533488a21098a62c85f1649abda2803",

            // mf2dd_freedos.cqm.lz
            "1ff7649b679ba22ff20d39ff717dbec8",

            // mf2hd_blind.cqm.lz
            "b4a602f67903c46eef62addb0780aa56",

            // mf2hd.cqm.lz
            "b4a602f67903c46eef62addb0780aa56",

            // mf2hd_fdformat_168.cqm.lz
            "03c2af6a8ebf4bd6f530335de34ae5dd",

            // mf2hd_freedos.cqm.lz
            "1a9f2eeb3cbeeb057b9a9a5c6e9b0cc6"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CopyQM");
        public override IMediaImage _plugin => new DiscImages.CopyQm();
    }
}