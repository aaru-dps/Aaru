// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SHA256.cs
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
public class Sha256
{
    static readonly byte[] _expectedEmpty =
    {
        0x30, 0xe1, 0x49, 0x55, 0xeb, 0xf1, 0x35, 0x22, 0x66, 0xdc, 0x2f, 0xf8, 0x06, 0x7e, 0x68, 0x10, 0x46, 0x07,
        0xe7, 0x50, 0xab, 0xb9, 0xd3, 0xb3, 0x65, 0x82, 0xb8, 0xaf, 0x90, 0x9f, 0xcb, 0x58
    };
    static readonly byte[] _expectedRandom =
    {
        0x4d, 0x1a, 0x6b, 0x8a, 0x54, 0x67, 0x00, 0xc4, 0x8e, 0xda, 0x70, 0xd3, 0x39, 0x1c, 0x8f, 0x15, 0x8a, 0x8d,
        0x12, 0xb2, 0x38, 0x92, 0x89, 0x29, 0x50, 0x47, 0x8c, 0x41, 0x8e, 0x25, 0xcc, 0x39
    };

    [Test]
    public void EmptyData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Sha256Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyFile()
    {
        byte[] result = Sha256Context.File(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"));
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyInstance()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        IChecksum ctx = new Sha256Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void RandomData()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Sha256Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomFile()
    {
        byte[] result = Sha256Context.File(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"));
        result.Should().BeEquivalentTo(_expectedRandom);
    }

    [Test]
    public void RandomInstance()
    {
        var data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "Checksum test files", "random"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        IChecksum ctx = new Sha256Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedRandom);
    }
}