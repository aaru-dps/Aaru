// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IbgLogTests.cs
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
using System.Reflection;
using System.Threading;
using Aaru.Core.Logging;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Dumping;

[TestFixture]
public class IbgLogTests
{
    const string EXISTING_LOG = "IBGD\r\n"                        +
                                "[START_CONFIGURATION]\r\n"       +
                                "VERIFY_SPEED_START=2.00\r\n"     +
                                "VERIFY_SPEED_END=4.00\r\n"       +
                                "VERIFY_SPEED_AVERAGE=3.00\r\n"   +
                                "VERIFY_SPEED_MAX=40.00\r\n"      +
                                "VERIFY_TIME_TAKEN=10\r\n"        +
                                "[END_CONFIGURATION]\r\n"         +
                                "[START_VERIFY_GRAPH_VALUES]\r\n" +
                                "2.00,0,100,0\r\n"                +
                                "4.00,50,100,0\r\n"               +
                                "[END_VERIFY_GRAPH_VALUES]\r\n";

    string _logPath;

    [SetUp]
    public void SetUp() => _logPath = Path.GetTempFileName();

    [TearDown]
    public void TearDown() => File.Delete(_logPath);

    static string[] Rows(IbgLog log) =>
        typeof(IbgLog).GetField("_ibgSb", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(log)!.ToString()!
                      .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

    static T Field<T>(IbgLog log, string name) =>
        (T)typeof(IbgLog).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(log)!;

    [Test]
    public void ResumingKeepsTheRowsAndAddsNewOnes()
    {
        File.WriteAllText(_logPath, EXISTING_LOG);

        IbgLog log = new(_logPath, 0x0008, true);

        log.Write(100, 1500);
        Thread.Sleep(120);
        log.Write(101, 1500);

        string[] rows = Rows(log);

        rows.Should().HaveCount(3);
        rows[0].Should().Be("2.00,0,100,0");
        rows[1].Should().Be("4.00,50,100,0");
        rows[2].Should().StartWith("10.00,100,");
    }

    [Test]
    public void ResumingKeepsTheStartAndMaximumSpeeds()
    {
        File.WriteAllText(_logPath, EXISTING_LOG);

        IbgLog log = new(_logPath, 0x0008, true);

        log.Write(100, 1500);
        Thread.Sleep(120);
        log.Write(101, 1500);

        Field<double>(log, "_ibgStartSpeed").Should().Be(2);
        Field<double>(log, "_ibgMaxSpeed").Should().Be(40);
    }

    [Test]
    public void NotResumingIgnoresTheExistingLog()
    {
        File.WriteAllText(_logPath, EXISTING_LOG);

        Rows(new IbgLog(_logPath, 0x0008)).Should().BeEmpty();
    }

    [TestCase("not a log")]
    [TestCase("")]
    public void ResumingAnInvalidLogStartsFresh(string contents)
    {
        File.WriteAllText(_logPath, contents);

        IbgLog log = new(_logPath, 0x0008, true);

        Rows(log).Should().BeEmpty();
        Field<bool>(log, "_ibgStartSet").Should().BeFalse();
    }

    [Test]
    public void ResumingAMissingLogStartsFresh()
    {
        File.Delete(_logPath);

        Rows(new IbgLog(_logPath, 0x0008, true)).Should().BeEmpty();
    }

    [Test]
    public void SnapshotsThatDidNotFillARowAreKept()
    {
        IbgLog log = new(_logPath, 0x0008);

        log.Write(5, 1500);
        Rows(log).Should().BeEmpty();

        log.WritePendingRow();

        string[] rows = Rows(log);

        rows.Should().ContainSingle();
        rows[0].Should().StartWith("10.00,5,");
        Field<double>(log, "_ibgMaxSpeed").Should().Be(10);
    }

    [Test]
    public void RestartingARowDropsTheStopButKeepsItsSnapshots()
    {
        IbgLog log = new(_logPath, 0x0008);

        log.Write(5, 1500);
        Thread.Sleep(150);
        log.RestartRow();
        log.Write(6, 1500);

        Rows(log).Should().BeEmpty();

        log.WritePendingRow();

        string[] columns = Rows(log)[0].Split(',');

        columns[0].Should().Be("10.00");
        columns[1].Should().Be("5");
        int.Parse(columns[2]).Should().BeLessThan(100);
    }

    [Test]
    public void NoPendingSnapshotsWriteNoRow()
    {
        IbgLog log = new(_logPath, 0x0008);

        log.WritePendingRow();
        log.Write(5, 1500);
        log.WritePendingRow();
        log.WritePendingRow();

        Rows(log).Should().ContainSingle();
    }

    [Test]
    public void MergedAverageIsWeightedByTime()
    {
        (double seconds, double endSpeed, double averageSpeed) = IbgLog.MergeTotals(10, 100, 300, 30, 500, 700, true);

        seconds.Should().Be(40);
        endSpeed.Should().Be(500);
        averageSpeed.Should().BeApproximately(600, 0.001);
    }

    [Test]
    public void RunWithoutMeasuringKeepsThePreviousTotals()
    {
        (double seconds, double endSpeed, double averageSpeed) = IbgLog.MergeTotals(10, 100, 300, 9, 0, 0, false);

        seconds.Should().Be(10);
        endSpeed.Should().Be(100);
        averageSpeed.Should().Be(300);
    }

    [Test]
    public void MaximumSpeedIsAveragedOverTheSamplesOfARow()
    {
        // CD-ROM profile, where 1x is 150 KiB/s. Nothing is written to the file until the log is closed.
        IbgLog log = new("unused.ibg", 0x0008);

        for(ulong s = 0; s < 6; s++)
        {
            log.Write(s, 1500);
            Thread.Sleep(30);
        }

        FieldInfo maxSpeedField =
            typeof(IbgLog).GetField("_ibgMaxSpeed", BindingFlags.Instance | BindingFlags.NonPublic);

        ((double)maxSpeedField!.GetValue(log)!).Should().BeApproximately(10, 0.001);
    }

    [Test]
    public void GraphStartsAtTheFirstMeasuredSpeed()
    {
        IbgLog log = new("unused.ibg", 0x0008);

        // Everything before the first speed is measured, like a pregap read, must not reach the graph
        Thread.Sleep(500);
        log.Write(0, 0);
        log.Write(1, 0);

        log.Write(5, 1500);
        Thread.Sleep(120);
        log.Write(6, 1500);

        FieldInfo rowsField = typeof(IbgLog).GetField("_ibgSb", BindingFlags.Instance | BindingFlags.NonPublic);

        string[] rows = rowsField!.GetValue(log)!.ToString()!.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        rows.Should().ContainSingle();

        string[] columns = rows[0].Trim().Split(',');

        columns[0].Should().Be("10.00");
        columns[1].Should().Be("5");
        int.Parse(columns[2]).Should().BeLessThan(500);
    }
}