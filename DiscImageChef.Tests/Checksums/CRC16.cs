// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CRC16.cs
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

using System;
using NUnit.Framework;
using DiscImageChef.Checksums;
using System.IO;

namespace DiscImageChef.Tests.Checksums
{
    [TestFixture]
    public class CRC16
    {
        static readonly byte[] ExpectedEmpty = { 0x00, 0x00 };
        static readonly byte[] ExpectedRandom = { 0x2d, 0x6d };

        [Test]
        public void CRC16EmptyFile()
        {
            byte[] result = CRC16Context.File(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"));
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void CRC16EmptyData()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"), FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            CRC16Context.Data(data, out byte[] result);
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void CRC16EmptyInstance()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"), FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            CRC16Context ctx = new CRC16Context();
            ctx.Init();
            ctx.Update(data);
            byte[] result = ctx.Final();
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void CRC16RandomFile()
        {
            byte[] result = CRC16Context.File(Path.Combine(Consts.TestFilesRoot, "checksums", "random"));
            Assert.AreEqual(ExpectedRandom, result);
        }

        [Test]
        public void CRC16RandomData()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "random"), FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            CRC16Context.Data(data, out byte[] result);
            Assert.AreEqual(ExpectedRandom, result);
        }

        [Test]
        public void CRC16RandomInstance()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "random"), FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            CRC16Context ctx = new CRC16Context();
            ctx.Init();
            ctx.Update(data);
            byte[] result = ctx.Final();
            Assert.AreEqual(ExpectedRandom, result);
        }
    }
}
