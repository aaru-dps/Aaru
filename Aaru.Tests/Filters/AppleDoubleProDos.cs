// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleDoubleProDos.cs
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filters;

[TestFixture]
public class AppleDoubleProDos
{
    const    string EXPECTED_FILE     = "c2be571406cf6353269faa59a4a8c0a4";
    const    string EXPECTED_SIDECAR  = "7b0c25bf8cb70f6fb1a15eca31585250";
    const    string EXPECTED_CONTENTS = "c2be571406cf6353269faa59a4a8c0a4";
    const    string EXPECTED_RESOURCE = "c689c58945169065483d94e39583d416";
    readonly string _location;
    readonly string _sidecar;

    public AppleDoubleProDos()
    {
        _location = Path.Combine(Consts.TEST_FILES_ROOT, "Filters", "AppleDouble", "prodos", "DOS_720.dmg");
        _sidecar  = Path.Combine(Consts.TEST_FILES_ROOT, "Filters", "AppleDouble", "prodos", "R.DOS_720.dmg");
    }

    [Test]
    public void CheckContents()
    {
        IFilter filter = new AppleDouble();
        filter.Open(_location);
        Stream str  = filter.GetDataForkStream();
        byte[] data = new byte[737280];
        str.Read(data, 0, 737280);
        str.Close();
        str.Dispose();
        filter.Close();
        string result = Md5Context.Data(data, out _);
        Assert.AreEqual(EXPECTED_CONTENTS, result);
    }

    [Test]
    public void CheckCorrectFile()
    {
        string result = Md5Context.File(_location, out _);
        Assert.AreEqual(EXPECTED_FILE, result);

        result = Md5Context.File(_sidecar, out _);
        Assert.AreEqual(EXPECTED_SIDECAR, result);
    }

    [Test]
    public void CheckFilterId()
    {
        IFilter filter = new AppleDouble();
        Assert.AreEqual(true, filter.Identify(_location));
    }

    [Test]
    public void CheckResource()
    {
        IFilter filter = new AppleDouble();
        filter.Open(_location);
        Stream str  = filter.GetResourceForkStream();
        byte[] data = new byte[286];
        str.Read(data, 0, 286);
        str.Close();
        str.Dispose();
        filter.Close();
        string result = Md5Context.Data(data, out _);
        Assert.AreEqual(EXPECTED_RESOURCE, result);
    }

    [Test]
    public void Test()
    {
        IFilter filter = new AppleDouble();
        Assert.AreEqual(ErrorNumber.NoError, filter.Open(_location));
        Assert.AreEqual(737280, filter.DataForkLength);
        Assert.AreNotEqual(null, filter.GetDataForkStream());
        Assert.AreEqual(286, filter.ResourceForkLength);
        Assert.AreNotEqual(null, filter.GetResourceForkStream());
        Assert.AreEqual(true, filter.HasResourceFork);
        filter.Close();
    }
}