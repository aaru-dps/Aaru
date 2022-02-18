// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Adler32.cs
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

namespace Aaru.Tests.Checksums
{
    [TestFixture]
    public class Adler32
    {
        static readonly byte[] _expectedEmpty =
        {
            0x00, 0xf0, 0x00, 0x01
        };
        static readonly byte[] _expectedRandom =
        {
            0x37, 0x28, 0xd1, 0x86
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
            Adler32Context.Data(data, out byte[] result);
            result.Should().BeEquivalentTo(_expectedEmpty);
        }

        [Test]
        public void EmptyFile()
        {
            byte[] result = Adler32Context.File(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "empty"));
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
            IChecksum ctx = new Adler32Context();
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
            Adler32Context.Data(data, out byte[] result);
            result.Should().BeEquivalentTo(_expectedRandom);
        }

        [Test]
        public void RandomFile()
        {
            byte[] result = Adler32Context.File(Path.Combine(Consts.TEST_FILES_ROOT, "Checksum test files", "random"));
            result.Should().BeEquivalentTo(_expectedRandom);
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
            IChecksum ctx = new Adler32Context();
            ctx.Update(data);
            byte[] result = ctx.Final();
            result.Should().BeEquivalentTo(_expectedRandom);
        }
    }
}