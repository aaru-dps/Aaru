// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BZip2.cs
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

namespace Aaru.Tests.Filters;

using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using NUnit.Framework;

[TestFixture]
public class BZip2
{
    static readonly byte[] _expectedFile =
    {
        0xf8, 0xb6, 0xbc, 0x62, 0x33, 0xcf, 0x1d, 0x28, 0x02, 0xef, 0x80, 0xf1, 0xe4, 0xfc, 0x1b, 0xdf
    };
    static readonly byte[] _expectedContents =
    {
        0x18, 0x90, 0x5a, 0xf9, 0x83, 0xd8, 0x2b, 0xdd, 0x1a, 0xcc, 0x69, 0x75, 0x4f, 0x0f, 0x81, 0x5e
    };
    readonly string _location;

    public BZip2() => _location = Path.Combine(Consts.TestFilesRoot, "Filters", "bzip2.bz2");

    [Test]
    public void CheckContents()
    {
        IFilter filter = new Aaru.Filters.BZip2();
        filter.Open(_location);
        Stream str  = filter.GetDataForkStream();
        var    data = new byte[1048576];
        str.EnsureRead(data, 0, 1048576);
        str.Close();
        str.Dispose();
        filter.Close();
        Md5Context.Data(data, out byte[] result);
        Assert.AreEqual(_expectedContents, result);
    }

    [Test]
    public void CheckCorrectFile()
    {
        byte[] result = Md5Context.File(_location);
        Assert.AreEqual(_expectedFile, result);
    }

    [Test]
    public void CheckFilterId()
    {
        IFilter filter = new Aaru.Filters.BZip2();
        Assert.AreEqual(true, filter.Identify(_location));
    }

    [Test]
    public void Test()
    {
        IFilter filter = new Aaru.Filters.BZip2();
        Assert.AreEqual(ErrorNumber.NoError, filter.Open(_location));
        Assert.AreEqual(1048576, filter.DataForkLength);
        Assert.AreNotEqual(null, filter.GetDataForkStream());
        Assert.AreEqual(0, filter.ResourceForkLength);
        Assert.AreEqual(null, filter.GetResourceForkStream());
        Assert.AreEqual(false, filter.HasResourceFork);
        filter.Close();
    }
}