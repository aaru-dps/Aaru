// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MacBinary3.cs
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
    public class MacBinary3
    {
        const string ExpectedFile = "3a7363a6109fb52a264b0b45dfa16694";
        const string ExpectedContents = "c2be571406cf6353269faa59a4a8c0a4";
        const string ExpectedResource = "a972d27c44193a7587b21416c0953cc3";
        readonly string location;

        public MacBinary3()
        {
            location = Path.Combine(Consts.TestFilesRoot, "filters", "macbinary", "macbinary3.bin");
        }

        [Test]
        public void CheckCorrectFile()
        {
            MD5Context ctx = new MD5Context();
            ctx.Init();
            string result = ctx.File(location, out byte[] tmp);
            Assert.AreEqual(ExpectedFile, result);
        }

        [Test]
        public void CheckFilterId()
        {
            Filter filter = new DiscImageChef.Filters.MacBinary();
            Assert.AreEqual(true, filter.Identify(location));
        }

        [Test]
        public void Test()
        {
            Filter filter = new DiscImageChef.Filters.MacBinary();
            filter.Open(location);
            Assert.AreEqual(true, filter.IsOpened());
            Assert.AreEqual(737280, filter.GetDataForkLength());
            Assert.AreNotEqual(null, filter.GetDataForkStream());
            Assert.AreEqual(286, filter.GetResourceForkLength());
            Assert.AreNotEqual(null, filter.GetResourceForkStream());
            Assert.AreEqual(true, filter.HasResourceFork());
            filter.Close();
        }

        [Test]
        public void CheckContents()
        {
            Filter filter = new DiscImageChef.Filters.MacBinary();
            filter.Open(location);
            Stream str = filter.GetDataForkStream();
            byte[] data = new byte[737280];
            str.Read(data, 0, 737280);
            str.Close();
            str.Dispose();
            filter.Close();
            MD5Context ctx = new MD5Context();
            ctx.Init();
            string result = ctx.Data(data, out byte[] tmp);
            Assert.AreEqual(ExpectedContents, result);
        }

        [Test]
        public void CheckResource()
        {
            Filter filter = new DiscImageChef.Filters.MacBinary();
            filter.Open(location);
            Stream str = filter.GetResourceForkStream();
            byte[] data = new byte[286];
            str.Read(data, 0, 286);
            str.Close();
            str.Dispose();
            filter.Close();
            MD5Context ctx = new MD5Context();
            ctx.Init();
            string result = ctx.Data(data, out byte[] tmp);
            Assert.AreEqual(ExpectedResource, result);
        }
    }
}
