// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HDCopy.cs
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

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class HDCopy : BlockMediaImageTest
    {
        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "HD-COPY");
        public override IMediaImage _plugin => new HdCopy();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0000.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0001.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0009.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0010.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0024.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0025.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0030.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0045.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0046.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0047.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0048.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0049.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0050.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0051.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0052.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0053.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0054.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0055.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0056.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0057.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0058.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0059.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0060.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0069.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0075.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0076.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0078.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0080.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0082.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0084.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0107.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0108.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0111.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0112.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0113.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0114.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0115.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0116.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0117.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0122.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0123.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0124.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0125.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0126.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0163.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0164.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0168.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0169.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0170.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0171.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0174.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0175.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0176.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0177.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0180.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0181.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0182.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0183.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0262.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0263.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0264.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0265.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0266.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0267.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0268.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0269.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0270.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0271.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0272.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0273.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0282.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0283.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0284.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0285.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0301.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0302.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0303.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0304.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0305.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0311.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0314.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0316.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0317.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0318.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0319.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0320.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "TFULL.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "TFULLPAS.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "TNORMAL.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 0,
                MD5        = "UNKNOWN"
            }
        };
    }
}