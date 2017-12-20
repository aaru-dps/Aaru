// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PCExchange.cs
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
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filters
{
    [TestFixture]
    public class PCExchange
    {
        const string ExpectedFile = "348825a08fa84766d20b91ed917012b9";
        const string ExpectedContents = "c2be571406cf6353269faa59a4a8c0a4";
        const string ExpectedResource = "5cb168d60ce8b2b1b3133c2faaf47165";
        readonly string location;

        public PCExchange()
        {
            location = Path.Combine(Consts.TestFilesRoot, "filters", "pcexchange", "DC6_RW_DOS_720.img");
        }

        [Test]
        public void CheckCorrectFile()
        {
            Md5Context ctx = new Md5Context();
            ctx.Init();
            string result = ctx.File(Path.Combine(Consts.TestFilesRoot, "filters", "pcexchange", "FINDER.DAT"),
                                     out byte[] tmp);
            Assert.AreEqual(ExpectedFile, result);
        }

        [Test]
        public void CheckFilterId()
        {
            Filter filter = new DiscImageChef.Filters.PCExchange();
            Assert.AreEqual(true, filter.Identify(location));
        }

        [Test]
        public void Test()
        {
            Filter filter = new DiscImageChef.Filters.PCExchange();
            filter.Open(location);
            Assert.AreEqual(true, filter.IsOpened());
            Assert.AreEqual(737280, filter.GetDataForkLength());
            Assert.AreNotEqual(null, filter.GetDataForkStream());
            Assert.AreEqual(546, filter.GetResourceForkLength());
            Assert.AreNotEqual(null, filter.GetResourceForkStream());
            Assert.AreEqual(true, filter.HasResourceFork());
            filter.Close();
        }

        [Test]
        public void CheckContents()
        {
            Filter filter = new DiscImageChef.Filters.PCExchange();
            filter.Open(location);
            Stream str = filter.GetDataForkStream();
            byte[] data = new byte[737280];
            str.Read(data, 0, 737280);
            str.Close();
            str.Dispose();
            filter.Close();
            Md5Context ctx = new Md5Context();
            ctx.Init();
            string result = ctx.Data(data, out byte[] tmp);
            Assert.AreEqual(ExpectedContents, result);
        }

        [Test]
        public void CheckResource()
        {
            Filter filter = new DiscImageChef.Filters.PCExchange();
            filter.Open(location);
            Stream str = filter.GetResourceForkStream();
            byte[] data = new byte[546];
            str.Read(data, 0, 546);
            str.Close();
            str.Dispose();
            filter.Close();
            Md5Context ctx = new Md5Context();
            ctx.Init();
            string result = ctx.Data(data, out byte[] tmp);
            Assert.AreEqual(ExpectedResource, result);
        }
    }
}