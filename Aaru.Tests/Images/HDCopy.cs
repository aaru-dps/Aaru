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
    public class HDCopy
    {
        readonly string[] _testFiles =
        {
            "DSKA0000.IMG.lz", "DSKA0001.IMG.lz", "DSKA0009.IMG.lz", "DSKA0010.IMG.lz", "DSKA0024.IMG.lz",
            "DSKA0025.IMG.lz", "DSKA0030.IMG.lz", "DSKA0045.IMG.lz", "DSKA0046.IMG.lz", "DSKA0047.IMG.lz",
            "DSKA0048.IMG.lz", "DSKA0049.IMG.lz", "DSKA0050.IMG.lz", "DSKA0051.IMG.lz", "DSKA0052.IMG.lz",
            "DSKA0053.IMG.lz", "DSKA0054.IMG.lz", "DSKA0055.IMG.lz", "DSKA0056.IMG.lz", "DSKA0057.IMG.lz",
            "DSKA0058.IMG.lz", "DSKA0059.IMG.lz", "DSKA0060.IMG.lz", "DSKA0069.IMG.lz", "DSKA0075.IMG.lz",
            "DSKA0076.IMG.lz", "DSKA0078.IMG.lz", "DSKA0080.IMG.lz", "DSKA0082.IMG.lz", "DSKA0084.IMG.lz",
            "DSKA0107.IMG.lz", "DSKA0108.IMG.lz", "DSKA0111.IMG.lz", "DSKA0112.IMG.lz", "DSKA0113.IMG.lz",
            "DSKA0114.IMG.lz", "DSKA0115.IMG.lz", "DSKA0116.IMG.lz", "DSKA0117.IMG.lz", "DSKA0122.IMG.lz",
            "DSKA0123.IMG.lz", "DSKA0124.IMG.lz", "DSKA0125.IMG.lz", "DSKA0126.IMG.lz", "DSKA0163.IMG.lz",
            "DSKA0164.IMG.lz", "DSKA0168.IMG.lz", "DSKA0169.IMG.lz", "DSKA0170.IMG.lz", "DSKA0171.IMG.lz",
            "DSKA0174.IMG.lz", "DSKA0175.IMG.lz", "DSKA0176.IMG.lz", "DSKA0177.IMG.lz", "DSKA0180.IMG.lz",
            "DSKA0181.IMG.lz", "DSKA0182.IMG.lz", "DSKA0183.IMG.lz", "DSKA0262.IMG.lz", "DSKA0263.IMG.lz",
            "DSKA0264.IMG.lz", "DSKA0265.IMG.lz", "DSKA0266.IMG.lz", "DSKA0267.IMG.lz", "DSKA0268.IMG.lz",
            "DSKA0269.IMG.lz", "DSKA0270.IMG.lz", "DSKA0271.IMG.lz", "DSKA0272.IMG.lz", "DSKA0273.IMG.lz",
            "DSKA0282.IMG.lz", "DSKA0283.IMG.lz", "DSKA0284.IMG.lz", "DSKA0285.IMG.lz", "DSKA0301.IMG.lz",
            "DSKA0302.IMG.lz", "DSKA0303.IMG.lz", "DSKA0304.IMG.lz", "DSKA0305.IMG.lz", "DSKA0311.IMG.lz",
            "DSKA0314.IMG.lz", "DSKA0316.IMG.lz", "DSKA0317.IMG.lz", "DSKA0318.IMG.lz", "DSKA0319.IMG.lz",
            "DSKA0320.IMG.lz", "TFULL.IMG.lz", "TFULLPAS.IMG.lz", "TNORMAL.IMG.lz"
        };

        readonly ulong[] _sectors =
        {
            // DSKA0000.IMG.lz
            0,

            // DSKA0001.IMG.lz
            0,

            // DSKA0009.IMG.lz
            0,

            // DSKA0010.IMG.lz
            0,

            // DSKA0024.IMG.lz
            0,

            // DSKA0025.IMG.lz
            0,

            // DSKA0030.IMG.lz
            0,

            // DSKA0045.IMG.lz
            0,

            // DSKA0046.IMG.lz
            0,

            // DSKA0047.IMG.lz
            0,

            // DSKA0048.IMG.lz
            0,

            // DSKA0049.IMG.lz
            0,

            // DSKA0050.IMG.lz
            0,

            // DSKA0051.IMG.lz
            0,

            // DSKA0052.IMG.lz
            0,

            // DSKA0053.IMG.lz
            0,

            // DSKA0054.IMG.lz
            0,

            // DSKA0055.IMG.lz
            0,

            // DSKA0056.IMG.lz
            0,

            // DSKA0057.IMG.lz
            0,

            // DSKA0058.IMG.lz
            0,

            // DSKA0059.IMG.lz
            0,

            // DSKA0060.IMG.lz
            0,

            // DSKA0069.IMG.lz
            0,

            // DSKA0075.IMG.lz
            0,

            // DSKA0076.IMG.lz
            0,

            // DSKA0078.IMG.lz
            0,

            // DSKA0080.IMG.lz
            0,

            // DSKA0082.IMG.lz
            0,

            // DSKA0084.IMG.lz
            0,

            // DSKA0107.IMG.lz
            0,

            // DSKA0108.IMG.lz
            0,

            // DSKA0111.IMG.lz
            0,

            // DSKA0112.IMG.lz
            0,

            // DSKA0113.IMG.lz
            0,

            // DSKA0114.IMG.lz
            0,

            // DSKA0115.IMG.lz
            0,

            // DSKA0116.IMG.lz
            0,

            // DSKA0117.IMG.lz
            0,

            // DSKA0122.IMG.lz
            0,

            // DSKA0123.IMG.lz
            0,

            // DSKA0124.IMG.lz
            0,

            // DSKA0125.IMG.lz
            0,

            // DSKA0126.IMG.lz
            0,

            // DSKA0163.IMG.lz
            0,

            // DSKA0164.IMG.lz
            0,

            // DSKA0168.IMG.lz
            0,

            // DSKA0169.IMG.lz
            0,

            // DSKA0170.IMG.lz
            0,

            // DSKA0171.IMG.lz
            0,

            // DSKA0174.IMG.lz
            0,

            // DSKA0175.IMG.lz
            0,

            // DSKA0176.IMG.lz
            0,

            // DSKA0177.IMG.lz
            0,

            // DSKA0180.IMG.lz
            0,

            // DSKA0181.IMG.lz
            0,

            // DSKA0182.IMG.lz
            0,

            // DSKA0183.IMG.lz
            0,

            // DSKA0262.IMG.lz
            0,

            // DSKA0263.IMG.lz
            0,

            // DSKA0264.IMG.lz
            0,

            // DSKA0265.IMG.lz
            0,

            // DSKA0266.IMG.lz
            0,

            // DSKA0267.IMG.lz
            0,

            // DSKA0268.IMG.lz
            0,

            // DSKA0269.IMG.lz
            0,

            // DSKA0270.IMG.lz
            0,

            // DSKA0271.IMG.lz
            0,

            // DSKA0272.IMG.lz
            0,

            // DSKA0273.IMG.lz
            0,

            // DSKA0282.IMG.lz
            0,

            // DSKA0283.IMG.lz
            0,

            // DSKA0284.IMG.lz
            0,

            // DSKA0285.IMG.lz
            0,

            // DSKA0301.IMG.lz
            0,

            // DSKA0302.IMG.lz
            0,

            // DSKA0303.IMG.lz
            0,

            // DSKA0304.IMG.lz
            0,

            // DSKA0305.IMG.lz
            0,

            // DSKA0311.IMG.lz
            0,

            // DSKA0314.IMG.lz
            0,

            // DSKA0316.IMG.lz
            0,

            // DSKA0317.IMG.lz
            0,

            // DSKA0318.IMG.lz
            0,

            // DSKA0319.IMG.lz
            0,

            // DSKA0320.IMG.lz
            0,

            // TFULL.IMG.lz
            0,

            // TFULLPAS.IMG.lz
            0,

            // TNORMAL.IMG.lz
            0
        };

        readonly uint[] _sectorSize =
        {
            // DSKA0000.IMG.lz
            0,

            // DSKA0001.IMG.lz
            0,

            // DSKA0009.IMG.lz
            0,

            // DSKA0010.IMG.lz
            0,

            // DSKA0024.IMG.lz
            0,

            // DSKA0025.IMG.lz
            0,

            // DSKA0030.IMG.lz
            0,

            // DSKA0045.IMG.lz
            0,

            // DSKA0046.IMG.lz
            0,

            // DSKA0047.IMG.lz
            0,

            // DSKA0048.IMG.lz
            0,

            // DSKA0049.IMG.lz
            0,

            // DSKA0050.IMG.lz
            0,

            // DSKA0051.IMG.lz
            0,

            // DSKA0052.IMG.lz
            0,

            // DSKA0053.IMG.lz
            0,

            // DSKA0054.IMG.lz
            0,

            // DSKA0055.IMG.lz
            0,

            // DSKA0056.IMG.lz
            0,

            // DSKA0057.IMG.lz
            0,

            // DSKA0058.IMG.lz
            0,

            // DSKA0059.IMG.lz
            0,

            // DSKA0060.IMG.lz
            0,

            // DSKA0069.IMG.lz
            0,

            // DSKA0075.IMG.lz
            0,

            // DSKA0076.IMG.lz
            0,

            // DSKA0078.IMG.lz
            0,

            // DSKA0080.IMG.lz
            0,

            // DSKA0082.IMG.lz
            0,

            // DSKA0084.IMG.lz
            0,

            // DSKA0107.IMG.lz
            0,

            // DSKA0108.IMG.lz
            0,

            // DSKA0111.IMG.lz
            0,

            // DSKA0112.IMG.lz
            0,

            // DSKA0113.IMG.lz
            0,

            // DSKA0114.IMG.lz
            0,

            // DSKA0115.IMG.lz
            0,

            // DSKA0116.IMG.lz
            0,

            // DSKA0117.IMG.lz
            0,

            // DSKA0122.IMG.lz
            0,

            // DSKA0123.IMG.lz
            0,

            // DSKA0124.IMG.lz
            0,

            // DSKA0125.IMG.lz
            0,

            // DSKA0126.IMG.lz
            0,

            // DSKA0163.IMG.lz
            0,

            // DSKA0164.IMG.lz
            0,

            // DSKA0168.IMG.lz
            0,

            // DSKA0169.IMG.lz
            0,

            // DSKA0170.IMG.lz
            0,

            // DSKA0171.IMG.lz
            0,

            // DSKA0174.IMG.lz
            0,

            // DSKA0175.IMG.lz
            0,

            // DSKA0176.IMG.lz
            0,

            // DSKA0177.IMG.lz
            0,

            // DSKA0180.IMG.lz
            0,

            // DSKA0181.IMG.lz
            0,

            // DSKA0182.IMG.lz
            0,

            // DSKA0183.IMG.lz
            0,

            // DSKA0262.IMG.lz
            0,

            // DSKA0263.IMG.lz
            0,

            // DSKA0264.IMG.lz
            0,

            // DSKA0265.IMG.lz
            0,

            // DSKA0266.IMG.lz
            0,

            // DSKA0267.IMG.lz
            0,

            // DSKA0268.IMG.lz
            0,

            // DSKA0269.IMG.lz
            0,

            // DSKA0270.IMG.lz
            0,

            // DSKA0271.IMG.lz
            0,

            // DSKA0272.IMG.lz
            0,

            // DSKA0273.IMG.lz
            0,

            // DSKA0282.IMG.lz
            0,

            // DSKA0283.IMG.lz
            0,

            // DSKA0284.IMG.lz
            0,

            // DSKA0285.IMG.lz
            0,

            // DSKA0301.IMG.lz
            0,

            // DSKA0302.IMG.lz
            0,

            // DSKA0303.IMG.lz
            0,

            // DSKA0304.IMG.lz
            0,

            // DSKA0305.IMG.lz
            0,

            // DSKA0311.IMG.lz
            0,

            // DSKA0314.IMG.lz
            0,

            // DSKA0316.IMG.lz
            0,

            // DSKA0317.IMG.lz
            0,

            // DSKA0318.IMG.lz
            0,

            // DSKA0319.IMG.lz
            0,

            // DSKA0320.IMG.lz
            0,

            // TFULL.IMG.lz
            0,

            // TFULLPAS.IMG.lz
            0,

            // TNORMAL.IMG.lz
            0
        };

        readonly MediaType[] _mediaTypes =
        {
            // DSKA0000.IMG.lz
            MediaType.CD,

            // DSKA0001.IMG.lz
            MediaType.CD,

            // DSKA0009.IMG.lz
            MediaType.CD,

            // DSKA0010.IMG.lz
            MediaType.CD,

            // DSKA0024.IMG.lz
            MediaType.CD,

            // DSKA0025.IMG.lz
            MediaType.CD,

            // DSKA0030.IMG.lz
            MediaType.CD,

            // DSKA0045.IMG.lz
            MediaType.CD,

            // DSKA0046.IMG.lz
            MediaType.CD,

            // DSKA0047.IMG.lz
            MediaType.CD,

            // DSKA0048.IMG.lz
            MediaType.CD,

            // DSKA0049.IMG.lz
            MediaType.CD,

            // DSKA0050.IMG.lz
            MediaType.CD,

            // DSKA0051.IMG.lz
            MediaType.CD,

            // DSKA0052.IMG.lz
            MediaType.CD,

            // DSKA0053.IMG.lz
            MediaType.CD,

            // DSKA0054.IMG.lz
            MediaType.CD,

            // DSKA0055.IMG.lz
            MediaType.CD,

            // DSKA0056.IMG.lz
            MediaType.CD,

            // DSKA0057.IMG.lz
            MediaType.CD,

            // DSKA0058.IMG.lz
            MediaType.CD,

            // DSKA0059.IMG.lz
            MediaType.CD,

            // DSKA0060.IMG.lz
            MediaType.CD,

            // DSKA0069.IMG.lz
            MediaType.CD,

            // DSKA0075.IMG.lz
            MediaType.CD,

            // DSKA0076.IMG.lz
            MediaType.CD,

            // DSKA0078.IMG.lz
            MediaType.CD,

            // DSKA0080.IMG.lz
            MediaType.CD,

            // DSKA0082.IMG.lz
            MediaType.CD,

            // DSKA0084.IMG.lz
            MediaType.CD,

            // DSKA0107.IMG.lz
            MediaType.CD,

            // DSKA0108.IMG.lz
            MediaType.CD,

            // DSKA0111.IMG.lz
            MediaType.CD,

            // DSKA0112.IMG.lz
            MediaType.CD,

            // DSKA0113.IMG.lz
            MediaType.CD,

            // DSKA0114.IMG.lz
            MediaType.CD,

            // DSKA0115.IMG.lz
            MediaType.CD,

            // DSKA0116.IMG.lz
            MediaType.CD,

            // DSKA0117.IMG.lz
            MediaType.CD,

            // DSKA0122.IMG.lz
            MediaType.CD,

            // DSKA0123.IMG.lz
            MediaType.CD,

            // DSKA0124.IMG.lz
            MediaType.CD,

            // DSKA0125.IMG.lz
            MediaType.CD,

            // DSKA0126.IMG.lz
            MediaType.CD,

            // DSKA0163.IMG.lz
            MediaType.CD,

            // DSKA0164.IMG.lz
            MediaType.CD,

            // DSKA0168.IMG.lz
            MediaType.CD,

            // DSKA0169.IMG.lz
            MediaType.CD,

            // DSKA0170.IMG.lz
            MediaType.CD,

            // DSKA0171.IMG.lz
            MediaType.CD,

            // DSKA0174.IMG.lz
            MediaType.CD,

            // DSKA0175.IMG.lz
            MediaType.CD,

            // DSKA0176.IMG.lz
            MediaType.CD,

            // DSKA0177.IMG.lz
            MediaType.CD,

            // DSKA0180.IMG.lz
            MediaType.CD,

            // DSKA0181.IMG.lz
            MediaType.CD,

            // DSKA0182.IMG.lz
            MediaType.CD,

            // DSKA0183.IMG.lz
            MediaType.CD,

            // DSKA0262.IMG.lz
            MediaType.CD,

            // DSKA0263.IMG.lz
            MediaType.CD,

            // DSKA0264.IMG.lz
            MediaType.CD,

            // DSKA0265.IMG.lz
            MediaType.CD,

            // DSKA0266.IMG.lz
            MediaType.CD,

            // DSKA0267.IMG.lz
            MediaType.CD,

            // DSKA0268.IMG.lz
            MediaType.CD,

            // DSKA0269.IMG.lz
            MediaType.CD,

            // DSKA0270.IMG.lz
            MediaType.CD,

            // DSKA0271.IMG.lz
            MediaType.CD,

            // DSKA0272.IMG.lz
            MediaType.CD,

            // DSKA0273.IMG.lz
            MediaType.CD,

            // DSKA0282.IMG.lz
            MediaType.CD,

            // DSKA0283.IMG.lz
            MediaType.CD,

            // DSKA0284.IMG.lz
            MediaType.CD,

            // DSKA0285.IMG.lz
            MediaType.CD,

            // DSKA0301.IMG.lz
            MediaType.CD,

            // DSKA0302.IMG.lz
            MediaType.CD,

            // DSKA0303.IMG.lz
            MediaType.CD,

            // DSKA0304.IMG.lz
            MediaType.CD,

            // DSKA0305.IMG.lz
            MediaType.CD,

            // DSKA0311.IMG.lz
            MediaType.CD,

            // DSKA0314.IMG.lz
            MediaType.CD,

            // DSKA0316.IMG.lz
            MediaType.CD,

            // DSKA0317.IMG.lz
            MediaType.CD,

            // DSKA0318.IMG.lz
            MediaType.CD,

            // DSKA0319.IMG.lz
            MediaType.CD,

            // DSKA0320.IMG.lz
            MediaType.CD,

            // TFULL.IMG.lz
            MediaType.CD,

            // TFULLPAS.IMG.lz
            MediaType.CD,

            // TNORMAL.IMG.lz
            MediaType.CD
        };

        readonly string[] _md5S =
        {
            // DSKA0000.IMG.lz
            "UNKNOWN",

            // DSKA0001.IMG.lz
            "UNKNOWN",

            // DSKA0009.IMG.lz
            "UNKNOWN",

            // DSKA0010.IMG.lz
            "UNKNOWN",

            // DSKA0024.IMG.lz
            "UNKNOWN",

            // DSKA0025.IMG.lz
            "UNKNOWN",

            // DSKA0030.IMG.lz
            "UNKNOWN",

            // DSKA0045.IMG.lz
            "UNKNOWN",

            // DSKA0046.IMG.lz
            "UNKNOWN",

            // DSKA0047.IMG.lz
            "UNKNOWN",

            // DSKA0048.IMG.lz
            "UNKNOWN",

            // DSKA0049.IMG.lz
            "UNKNOWN",

            // DSKA0050.IMG.lz
            "UNKNOWN",

            // DSKA0051.IMG.lz
            "UNKNOWN",

            // DSKA0052.IMG.lz
            "UNKNOWN",

            // DSKA0053.IMG.lz
            "UNKNOWN",

            // DSKA0054.IMG.lz
            "UNKNOWN",

            // DSKA0055.IMG.lz
            "UNKNOWN",

            // DSKA0056.IMG.lz
            "UNKNOWN",

            // DSKA0057.IMG.lz
            "UNKNOWN",

            // DSKA0058.IMG.lz
            "UNKNOWN",

            // DSKA0059.IMG.lz
            "UNKNOWN",

            // DSKA0060.IMG.lz
            "UNKNOWN",

            // DSKA0069.IMG.lz
            "UNKNOWN",

            // DSKA0075.IMG.lz
            "UNKNOWN",

            // DSKA0076.IMG.lz
            "UNKNOWN",

            // DSKA0078.IMG.lz
            "UNKNOWN",

            // DSKA0080.IMG.lz
            "UNKNOWN",

            // DSKA0082.IMG.lz
            "UNKNOWN",

            // DSKA0084.IMG.lz
            "UNKNOWN",

            // DSKA0107.IMG.lz
            "UNKNOWN",

            // DSKA0108.IMG.lz
            "UNKNOWN",

            // DSKA0111.IMG.lz
            "UNKNOWN",

            // DSKA0112.IMG.lz
            "UNKNOWN",

            // DSKA0113.IMG.lz
            "UNKNOWN",

            // DSKA0114.IMG.lz
            "UNKNOWN",

            // DSKA0115.IMG.lz
            "UNKNOWN",

            // DSKA0116.IMG.lz
            "UNKNOWN",

            // DSKA0117.IMG.lz
            "UNKNOWN",

            // DSKA0122.IMG.lz
            "UNKNOWN",

            // DSKA0123.IMG.lz
            "UNKNOWN",

            // DSKA0124.IMG.lz
            "UNKNOWN",

            // DSKA0125.IMG.lz
            "UNKNOWN",

            // DSKA0126.IMG.lz
            "UNKNOWN",

            // DSKA0163.IMG.lz
            "UNKNOWN",

            // DSKA0164.IMG.lz
            "UNKNOWN",

            // DSKA0168.IMG.lz
            "UNKNOWN",

            // DSKA0169.IMG.lz
            "UNKNOWN",

            // DSKA0170.IMG.lz
            "UNKNOWN",

            // DSKA0171.IMG.lz
            "UNKNOWN",

            // DSKA0174.IMG.lz
            "UNKNOWN",

            // DSKA0175.IMG.lz
            "UNKNOWN",

            // DSKA0176.IMG.lz
            "UNKNOWN",

            // DSKA0177.IMG.lz
            "UNKNOWN",

            // DSKA0180.IMG.lz
            "UNKNOWN",

            // DSKA0181.IMG.lz
            "UNKNOWN",

            // DSKA0182.IMG.lz
            "UNKNOWN",

            // DSKA0183.IMG.lz
            "UNKNOWN",

            // DSKA0262.IMG.lz
            "UNKNOWN",

            // DSKA0263.IMG.lz
            "UNKNOWN",

            // DSKA0264.IMG.lz
            "UNKNOWN",

            // DSKA0265.IMG.lz
            "UNKNOWN",

            // DSKA0266.IMG.lz
            "UNKNOWN",

            // DSKA0267.IMG.lz
            "UNKNOWN",

            // DSKA0268.IMG.lz
            "UNKNOWN",

            // DSKA0269.IMG.lz
            "UNKNOWN",

            // DSKA0270.IMG.lz
            "UNKNOWN",

            // DSKA0271.IMG.lz
            "UNKNOWN",

            // DSKA0272.IMG.lz
            "UNKNOWN",

            // DSKA0273.IMG.lz
            "UNKNOWN",

            // DSKA0282.IMG.lz
            "UNKNOWN",

            // DSKA0283.IMG.lz
            "UNKNOWN",

            // DSKA0284.IMG.lz
            "UNKNOWN",

            // DSKA0285.IMG.lz
            "UNKNOWN",

            // DSKA0301.IMG.lz
            "UNKNOWN",

            // DSKA0302.IMG.lz
            "UNKNOWN",

            // DSKA0303.IMG.lz
            "UNKNOWN",

            // DSKA0304.IMG.lz
            "UNKNOWN",

            // DSKA0305.IMG.lz
            "UNKNOWN",

            // DSKA0311.IMG.lz
            "UNKNOWN",

            // DSKA0314.IMG.lz
            "UNKNOWN",

            // DSKA0316.IMG.lz
            "UNKNOWN",

            // DSKA0317.IMG.lz
            "UNKNOWN",

            // DSKA0318.IMG.lz
            "UNKNOWN",

            // DSKA0319.IMG.lz
            "UNKNOWN",

            // DSKA0320.IMG.lz
            "UNKNOWN",

            // TFULL.IMG.lz
            "UNKNOWN",

            // TFULLPAS.IMG.lz
            "UNKNOWN",

            // TNORMAL.IMG.lz
            "UNKNOWN"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "HD-COPY");

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

                    var  image  = new HdCopy();
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

                    var   image       = new HdCopy();
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