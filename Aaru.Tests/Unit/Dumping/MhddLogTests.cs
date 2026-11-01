// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MhddLogTests.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using Aaru.Core.Logging;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Dumping;

[TestFixture]
public class MhddLogTests
{
    string _logPath;

    [SetUp]
    public void SetUp() => _logPath = Path.GetTempFileName();

    [TearDown]
    public void TearDown() => File.Delete(_logPath);

    static byte[] Log(uint pointer, int headerLength, int recordsLength)
    {
        byte[] log = new byte[headerLength + recordsLength];
        BitConverter.GetBytes(pointer).CopyTo(log, 0);

        for(var i = 0; i < recordsLength; i++) log[headerLength + i] = (byte)(i + 1);

        return log;
    }

    [Test]
    public void ReadsTheRecordsAfterTheHeader()
    {
        File.WriteAllBytes(_logPath, Log(20, 20, 32));

        MhddLog.ReadExistingRecords(_logPath).Should().Equal(Enumerable.Range(1, 32).Select(static i => (byte)i));
    }

    [Test]
    public void DropsAnIncompleteRecord()
    {
        File.WriteAllBytes(_logPath, Log(20, 20, 20));

        MhddLog.ReadExistingRecords(_logPath).Should().Equal(Enumerable.Range(1, 16).Select(static i => (byte)i));
    }

    [Test]
    public void LogWithOnlyAHeaderHasNoRecords()
    {
        File.WriteAllBytes(_logPath, Log(20, 20, 0));

        MhddLog.ReadExistingRecords(_logPath).Should().BeEmpty();
    }

    [TestCase(1u)]
    [TestCase(1000u)]
    public void InvalidPointerHasNoRecords(uint pointer)
    {
        File.WriteAllBytes(_logPath, Log(pointer, 20, 32));

        MhddLog.ReadExistingRecords(_logPath).Should().BeEmpty();
    }

    [Test]
    public void ShortFileHasNoRecords()
    {
        File.WriteAllBytes(_logPath, [1, 2]);

        MhddLog.ReadExistingRecords(_logPath).Should().BeEmpty();
    }

    [Test]
    public void MissingFileHasNoRecords()
    {
        File.Delete(_logPath);

        MhddLog.ReadExistingRecords(_logPath).Should().BeEmpty();
    }
}