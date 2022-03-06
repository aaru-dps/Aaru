// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SHA512.cs
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

using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes.Interfaces;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Checksums;

[TestFixture]
public class Sha512
{
    static readonly byte[] _expectedEmpty =
    {
        0xd6, 0x29, 0x26, 0x85, 0xb3, 0x80, 0xe3, 0x38, 0xe0, 0x25, 0xb3, 0x41, 0x5a, 0x90, 0xfe, 0x8f, 0x9d, 0x39,
        0xa4, 0x6e, 0x7b, 0xdb, 0xa8, 0xcb, 0x78, 0xc5, 0x0a, 0x33, 0x8c, 0xef, 0xca, 0x74, 0x1f, 0x69, 0xe4, 0xe4,
        0x64, 0x11, 0xc3, 0x2d, 0xe1, 0xaf, 0xde, 0xdf, 0xb2, 0x68, 0xe5, 0x79, 0xa5, 0x1f, 0x81, 0xff, 0x85, 0xe5,
        0x6f, 0x55, 0xb0, 0xee, 0x7c, 0x33, 0xfe, 0x8c, 0x25, 0xc9
    };
    static readonly byte[] _expectedRandom =
    {
        0x6a, 0x0a, 0x18, 0xc2, 0xad, 0xf8, 0x83, 0xac, 0x58, 0xe6, 0x21, 0x96, 0xdb, 0x8d, 0x3d, 0x0e, 0xb9, 0x87,
        0xd1, 0x49, 0x24, 0x97, 0xdb, 0x15, 0xb9, 0xfc, 0xcc, 0xb0, 0x36, 0xdf, 0x64, 0xae, 0xdb, 0x3e, 0x82, 0xa0,
        0x4d, 0xdc, 0xd1, 0x37, 0x48, 0x92, 0x95, 0x51, 0xf9, 0xdd, 0xab, 0x82, 0xf4, 0x8a, 0x85, 0x3f, 0x9a, 0x01,
        0xb5, 0xf2, 0x8c, 0xbb, 0x4a, 0xa5, 0x1b, 0x40, 0x7c, 0xb6
    };

    [Test]
    public void EmptyData()
    {
        byte[] data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Sha512Context.Data(data, out byte[] result);
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyFile()
    {
        byte[] result = Sha512Context.File(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"));
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void EmptyInstance()
    {
        byte[] data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"), FileMode.Open,
                                FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        IChecksum ctx = new Sha512Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        result.Should().BeEquivalentTo(_expectedEmpty);
    }

    [Test]
    public void RandomData()
    {
        byte[] data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"),
                                FileMode.Open, FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        Sha512Context.Data(data, out byte[] result);
        Assert.AreEqual(_expectedRandom, result);
    }

    [Test]
    public void RandomFile()
    {
        byte[] result = Sha512Context.File(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"));
        Assert.AreEqual(_expectedRandom, result);
    }

    [Test]
    public void RandomInstance()
    {
        byte[] data = new byte[1048576];

        var fs = new FileStream(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"),
                                FileMode.Open, FileAccess.Read);

        fs.Read(data, 0, 1048576);
        fs.Close();
        fs.Dispose();
        IChecksum ctx = new Sha512Context();
        ctx.Update(data);
        byte[] result = ctx.Final();
        Assert.AreEqual(_expectedRandom, result);
    }
}