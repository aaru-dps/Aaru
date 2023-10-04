// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CRC64.cs
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Checksums;

[TestFixture]
public class Crc64
{
    static readonly byte[] _expectedEmpty =
    {
        0x60, 0x6b, 0x70, 0xa2, 0x3e, 0xba, 0xf6, 0xc2
    };
    static readonly byte[] _expectedRandom =
    {
        0xbf, 0x09, 0x99, 0x2c, 0xc5, 0xed, 0xe3, 0x8e
    };
    static readonly byte[] _expectedRandom15 =
    {
        0x79, 0x7F, 0x37, 0x66, 0xFD, 0x93, 0x97, 0x5B
    };
    static readonly byte[] _expectedRandom31 =
    {
        0xCD, 0x92, 0x01, 0x90, 0x5A, 0x79, 0x37, 0xFD
    };
    static readonly byte[] _expectedRandom63 =
    {
        0x29, 0xF3, 0x31, 0xFC, 0x90, 0x70, 0x2B, 0xF4
    };
    static readonly byte[] _expectedRandom2352 =
    {
        0x12, 0x64, 0x35, 0xDB, 0x43, 0x47, 0x76, 0x23
    };

    [Test]
    public void EmptyData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.EnsureRead(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Crc64Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyFile()
    {
        byte[] result = Crc64Context.File(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"));
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
        IChecksum ctx = new Crc64Context();
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
        IChecksum ctx = new Crc64Context();
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
        IChecksum ctx = new Crc64Context();
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
        IChecksum ctx = new Crc64Context();
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
        IChecksum ctx = new Crc64Context();
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
        Crc64Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomFile()
    {
        byte[] result = Crc64Context.File(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"));
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
        IChecksum ctx = new Crc64Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom);
    }
}