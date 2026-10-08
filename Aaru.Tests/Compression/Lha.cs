// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Lha.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the LHA family decompressors, mirroring the Aaru.Compression.Native test suite.
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

using System;
using System.IO;
using Aaru.Compression;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Compression;

[TestFixture]
public class LhaTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    [Test]
    public void Lh6()
    {
        byte[] payload = LoadPayload("lha_lh6.lzh", out uint uncompressedSize);

        uncompressedSize.Should().Be(Helpers.ALICE29_SIZE, "header must declare the expected size");

        using Stream stream = new LhaStream(new MemoryStream(payload), Helpers.ALICE29_SIZE, 6);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Lh1()
    {
        byte[] payload = LoadPayload("lharc_lh1.lzh", out uint uncompressedSize);

        uncompressedSize.Should().Be(Helpers.ALICE29_SIZE, "header must declare the expected size");

        using Stream stream = new LhaStream(new MemoryStream(payload), Helpers.ALICE29_SIZE, 1);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void LarcLz5()
    {
        byte[] payload = LoadPayload("larc_lz5.lzs", out uint uncompressedSize);

        uncompressedSize.Should().Be(Helpers.ALICE29_SIZE, "header must declare the expected size");

        using Stream stream = new LarcStream(new MemoryStream(payload), Helpers.ALICE29_SIZE, 5);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void PmarcPm2()
    {
        byte[] payload = LoadPayload("pmarc_pm2.pma", out uint uncompressedSize);

        uncompressedSize.Should().Be(152192, "header must declare the expected size");

        using Stream stream = new PmarcStream(new MemoryStream(payload), 152192, 2);

        Helpers.AssertStream(stream, 152192, 0x1BBF031E);
    }

    /// <summary>
    ///     Extracts the compressed payload from a single-member LHA, LArc or PMarc archive with a level 0, 1 or 2
    ///     header, as the native test suite does.
    /// </summary>
    static byte[] LoadPayload(string fixture, out uint uncompressedSize)
    {
        byte[] data = Helpers.ReadFixture(fixture);

        data.Length.Should().BeGreaterThanOrEqualTo(22, "archive must contain a header");

        uint compressedSize = BitConverter.ToUInt32(data, 7);
        uncompressedSize = BitConverter.ToUInt32(data, 11);
        byte level = data[20];

        int offset = level switch
                     {
                         0 or 1 => data[0] + 2,
                         2      => BitConverter.ToUInt16(data, 0),
                         _      => -1
                     };

        offset.Should().BeGreaterThan(0, "header level must be 0, 1 or 2");
        ((long)offset + compressedSize).Should().BeLessThanOrEqualTo(data.Length, "payload must fit in the file");

        return data[offset..(offset + (int)compressedSize)];
    }
}