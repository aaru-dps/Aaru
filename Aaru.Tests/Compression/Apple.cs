// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apple.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the Apple decompressors, mirroring the Aaru.Compression.Native test suite.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using Aaru.Compression.Apple;
using NUnit.Framework;

namespace Aaru.Tests.Compression;

[TestFixture]
public class AppleTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    [Test]
    public void Adc()
    {
        byte[] source      = Helpers.ReadFixture("adc.bin");
        byte[] destination = new byte[327680];
        int    size        = ADC.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, 262144, 0x5A5A7388);
    }

    [Test]
    public void AppleRle()
    {
        byte[] source      = Helpers.ReadFixture("apple_rle.bin");
        byte[] destination = new byte[32768];
        int    size        = Rle.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, 20960, 0x3525EF06);
    }

    [TestCase("kencode_boot.bin", 10240, 0x18DDE60CU)]
    [TestCase("kencode_zeros.bin", 10240, 0x271DDE9AU)]
    [TestCase("kencode_hfs.bin", 10240, 0x72B8E8F1U)]
    [TestCase("kencode_last.bin", 6144, 0xA0B2EAEFU)]
    [TestCase("kencode_large.bin", 10240, 0xA584FF1AU)]
    public void KenCodeDecode(string fixture, int expectedSize, uint expectedCrc)
    {
        byte[] source      = Helpers.ReadFixture(fixture);
        byte[] destination = new byte[expectedSize];
        int    size        = KenCode.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, expectedSize, expectedCrc);
    }
}