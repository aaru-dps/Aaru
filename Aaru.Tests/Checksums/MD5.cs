// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MD5.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Checksums;

using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes.Interfaces;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class Md5
{
    static readonly byte[] _expectedEmpty =
    {
        0xb6, 0xd8, 0x1b, 0x36, 0x0a, 0x56, 0x72, 0xd8, 0x0c, 0x27, 0x43, 0x0f, 0x39, 0x15, 0x3e, 0x2c
    };
    static readonly byte[] _expectedRandom =
    {
        0xd7, 0x8f, 0x0e, 0xec, 0x41, 0x7b, 0xe3, 0x86, 0x21, 0x9b, 0x21, 0xb7, 0x00, 0x04, 0x4b, 0x95
    };

    [Test]
    public void EmptyData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Md5Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyFile()
    {
        byte[] result = Md5Context.File(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"));
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyInstance()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        IChecksum ctx = new Md5Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void RandomData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Md5Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomFile()
    {
        byte[] result = Md5Context.File(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"));
        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomInstance()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        IChecksum ctx = new Md5Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom);
    }
}