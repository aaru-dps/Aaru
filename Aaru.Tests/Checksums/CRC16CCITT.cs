// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CRC16CCITT.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.Checksums;
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Checksums;

[TestFixture]
public class Crc16Ccitt
{
    static readonly byte[] _expectedEmpty = { 0xFF, 0xFF };
    static readonly byte[] _expectedRandom =
    {
        // ReSharper disable once UseUtf8StringLiteral
        0x36, 0x40
    };
    static readonly byte[] _expectedRandom15   = { 0x16, 0x6e };
    static readonly byte[] _expectedRandom31   = { 0xd0, 0x16 };
    static readonly byte[] _expectedRandom63   = { 0x73, 0xc4 };
    static readonly byte[] _expectedRandom2352 = { 0x19, 0x46 };

    [Test]
    public void EmptyData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        CRC16CCITTContext.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyFile()
    {
        byte[] result = CRC16CCITTContext.File(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"));

        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyInstance()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        var ctx = new CRC16CCITTContext();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void PartialInstance15()
    {
        var data = new byte[15];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 15);
        fs.Close();
        fs.Dispose();
        var ctx = new CRC16CCITTContext();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom15);
    }

    [Test]
    public void PartialInstance2352()
    {
        var data = new byte[2352];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 2352);
        fs.Close();
        fs.Dispose();
        var ctx = new CRC16CCITTContext();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom2352);
    }

    [Test]
    public void PartialInstance31()
    {
        var data = new byte[31];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 31);
        fs.Close();
        fs.Dispose();
        var ctx = new CRC16CCITTContext();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom31);
    }

    [Test]
    public void PartialInstance63()
    {
        var data = new byte[63];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 63);
        fs.Close();
        fs.Dispose();
        var ctx = new CRC16CCITTContext();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom63);
    }

    [Test]
    public void RandomData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        CRC16CCITTContext.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomFile()
    {
        byte[] result = CRC16CCITTContext.File(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"));

        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomInstance()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        var ctx = new CRC16CCITTContext();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom);
    }
}