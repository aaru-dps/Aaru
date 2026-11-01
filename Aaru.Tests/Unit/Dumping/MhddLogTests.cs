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
using System.Reflection;
using System.Runtime.CompilerServices;
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

    // The constructor needs a device, so only what writing the records uses is set
    MhddLog LogWithoutHeader()
    {
        var log = (MhddLog)RuntimeHelpers.GetUninitializedObject(typeof(MhddLog));

        typeof(MhddLog).GetField("_logFile", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(log, _logPath);

        typeof(MhddLog).GetField("_mhddFs", BindingFlags.Instance | BindingFlags.NonPublic)!
                       .SetValue(log, new MemoryStream());

        return log;
    }

    ulong[] WrittenRecords()
    {
        byte[] log = File.ReadAllBytes(_logPath);

        return Enumerable.Range(0, log.Length / 8).Select(i => BitConverter.ToUInt64(log, i * 8)).ToArray();
    }

    [TestCase(10,   false, 10000ul)]
    [TestCase(10,   true,  65535000ul)]
    [TestCase(600,  true,  600000ul)]
    [TestCase(600,  false, 600000ul)]
    public void RetriesFollowTheConventionForFailedReads(double duration, bool failed, ulong expected)
    {
        MhddLog log = LogWithoutHeader();

        log.WriteRetry(7, duration, failed);
        log.Close();

        WrittenRecords().Should().Equal(7, expected);
    }

    [Test]
    public void FlushWritesTheFileAndTheLogKeepsGrowing()
    {
        MhddLog log = LogWithoutHeader();

        log.Write(1, 2);
        log.Flush();

        WrittenRecords().Should().Equal(1, 2000);

        log.WriteRetry(3, 4, false);
        log.Close();

        WrittenRecords().Should().Equal(1, 2000, 3, 4000);
    }

    [TestCase("/dumps/disc/My.Disc.mhddlog.bin", "/dumps/disc/My.Disc.mhddlog.png")]
    [TestCase("disc/image.mhddlog.bin", "disc/image.mhddlog.png")]
    [TestCase("image.mhddlog.bin", "image.mhddlog.png")]
    public void GraphGoesNextToTheLog(string logFile, string expected)
    {
        MhddLog.GraphPath(logFile).Should().Be(expected);
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