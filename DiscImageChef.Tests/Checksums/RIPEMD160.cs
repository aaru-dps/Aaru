// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RIPEMD160.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.Checksums;
using NUnit.Framework;

namespace DiscImageChef.Tests.Checksums
{
    [TestFixture]
    public class RIPEMD160
    {
        static readonly byte[] ExpectedEmpty =
        {
            0x59, 0xf4, 0x4e, 0x7d, 0xaf, 0xba, 0xe0, 0xfa, 0x30, 0x15, 0xc1, 0x96, 0x41, 0xc5, 0xa5, 0xaf, 0x2d, 0x93,
            0x99, 0x8d
        };
        static readonly byte[] ExpectedRandom =
        {
            0x2a, 0x7b, 0x6c, 0x0e, 0xc1, 0x80, 0xce, 0x6a, 0xe2, 0xfd, 0x77, 0xb4, 0xe0, 0xb6, 0x80, 0xc5, 0xd9, 0x9f,
            0xf6, 0x7b
        };

        [Test]
        public void RIPEMD160EmptyFile()
        {
            Ripemd160Context ctx = new Ripemd160Context();
            ctx.Init();
            byte[] result = ctx.File(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"));
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void RIPEMD160EmptyData()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            Ripemd160Context ctx = new Ripemd160Context();
            ctx.Init();
            ctx.Data(data, out byte[] result);
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void RIPEMD160EmptyInstance()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            Ripemd160Context ctx = new Ripemd160Context();
            ctx.Init();
            ctx.Update(data);
            byte[] result = ctx.Final();
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void RIPEMD160RandomFile()
        {
            Ripemd160Context ctx = new Ripemd160Context();
            ctx.Init();
            byte[] result = ctx.File(Path.Combine(Consts.TestFilesRoot, "checksums", "random"));
            Assert.AreEqual(ExpectedRandom, result);
        }

        [Test]
        public void RIPEMD160RandomData()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "random"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            Ripemd160Context ctx = new Ripemd160Context();
            ctx.Init();
            ctx.Data(data, out byte[] result);
            Assert.AreEqual(ExpectedRandom, result);
        }

        [Test]
        public void RIPEMD160RandomInstance()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "random"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            Ripemd160Context ctx = new Ripemd160Context();
            ctx.Init();
            ctx.Update(data);
            byte[] result = ctx.Final();
            Assert.AreEqual(ExpectedRandom, result);
        }
    }
}