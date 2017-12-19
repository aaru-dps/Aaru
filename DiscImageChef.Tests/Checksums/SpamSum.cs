// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SpamSum.cs
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
    public class SpamSum
    {
        static readonly string ExpectedEmpty = "3::";
        static readonly string ExpectedRandom = "24576:3dvzuAsHTQ16pc7O1Q/gS9qze+Swwn9s6IX:8/TQQpaVqze+JN6IX";

        [Test]
        public void SpamSumEmptyData()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            SpamSumContext ctx = new SpamSumContext();
            string result = ctx.Data(data, out byte[] tmp);
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void SpamSumEmptyInstance()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "empty"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            SpamSumContext ctx = new SpamSumContext();
            ctx.Init();
            ctx.Update(data);
            string result = ctx.End();
            Assert.AreEqual(ExpectedEmpty, result);
        }

        [Test]
        public void SpamSumRandomData()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "random"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            SpamSumContext ctx = new SpamSumContext();
            string result = ctx.Data(data, out byte[] tmp);
            Assert.AreEqual(ExpectedRandom, result);
        }

        [Test]
        public void SpamSumRandomInstance()
        {
            byte[] data = new byte[1048576];
            FileStream fs = new FileStream(Path.Combine(Consts.TestFilesRoot, "checksums", "random"), FileMode.Open,
                                           FileAccess.Read);
            fs.Read(data, 0, 1048576);
            fs.Close();
            fs.Dispose();
            SpamSumContext ctx = new SpamSumContext();
            ctx.Init();
            ctx.Update(data);
            string result = ctx.End();
            Assert.AreEqual(ExpectedRandom, result);
        }
    }
}