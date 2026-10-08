// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AaruFormatDpmTests.cs
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
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Filters;
using Aaru.Images;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Images;

[TestFixture]
[Category("Unit")]
public class AaruFormatDpmTests
{
    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

    [SetUp]
    public void SetUp() => _directory = Directory.CreateTempSubdirectory("aaru-dpm-").FullName;

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, true);

    const ulong SECTORS = 2000;
    string      _directory;

    static Track DataTrack() => new()
    {
        Sequence          = 1,
        Session           = 1,
        StartSector       = 0,
        EndSector         = SECTORS - 1,
        Type              = TrackType.CdMode1,
        BytesPerSector    = 2048,
        RawBytesPerSector = 2352,
        SubchannelType    = TrackSubchannelType.None,
        Indexes = new Dictionary<ushort, int>
        {
            [1] = 0
        }
    };

    /// <summary>Creates a single track CD image, writes its sectors and the DPM, and closes it</summary>
    static void WriteImage(IWritableOpticalImage image, string path, DataPositionMeasurement? dpm)
    {
        image.Create(path, MediaType.CDROM, new Dictionary<string, string>(), SECTORS, 0, 0, 2352)
             .Should()
             .BeTrue(image.ErrorMessage);

        image.SetTracks([DataTrack()]).Should().BeTrue(image.ErrorMessage);

        var data   = new byte[2352 * SECTORS];
        var status = new SectorStatus[SECTORS];
        Array.Fill(status, SectorStatus.Dumped);

        image.WriteSectorsLong(data, 0, false, (uint)SECTORS, status).Should().BeTrue(image.ErrorMessage);

        if(dpm.HasValue) image.SetDpm(dpm.Value).Should().BeTrue(image.ErrorMessage);

        image.Close().Should().BeTrue(image.ErrorMessage);
    }

    static T OpenImage<T>(string path) where T : IOpticalMediaImage, new()
    {
        var filter = new ZZZNoFilter();
        filter.Open(path).Should().Be(ErrorNumber.NoError);

        var image = new T();
        image.Open(filter).Should().Be(ErrorNumber.NoError);

        return image;
    }

    /// <summary>Builds a measured DPM, with uneven control points around a layer break and a short last bin</summary>
    static DataPositionMeasurement BuildMeasured(ulong unitsPerSector)
    {
        ulong[] lbas    = [0, 50, 100, 150, 999, 1000, 1050, 1990];
        var     entries = new DpmEntry[lbas.Length];

        for(int i = 0; i < lbas.Length; i++)
        {
            entries[i] = new DpmEntry
            {
                Lba    = lbas[i],
                Angle  = lbas[i] * unitsPerSector,
                Status = (DpmEntryStatus)(i % 6)
            };
        }

        return new DataPositionMeasurement
        {
            NominalSpacing    = 50,
            TimingUnit        = 16,
            Speed             = 0xFFFF,
            OppositeTrackPath = true,
            LayerEnds         = [999],
            Entries           = entries,
            Calibrations =
            [
                new DpmCalibration
                {
                    Lba            = 0,
                    RotationPeriod = 7_720_000,
                    SectorsPerTurn = 9_718_000
                },
                new DpmCalibration
                {
                    Lba            = 1000,
                    RotationPeriod = 11_490_000,
                    SectorsPerTurn = 21_402_000
                }
            ]
        };
    }

    static void ShouldBeEqual(DataPositionMeasurement read, DataPositionMeasurement written)
    {
        read.NominalSpacing.Should().Be(written.NominalSpacing);
        read.TimingUnit.Should().Be(written.TimingUnit);
        read.Speed.Should().Be(written.Speed);
        read.OppositeTrackPath.Should().Be(written.OppositeTrackPath);
        read.LayerEnds.Should().Equal(written.LayerEnds);
        read.Entries.Should().Equal(written.Entries);
        read.Calibrations.Should().Equal(written.Calibrations);
    }

    [Test]
    public void ImageWithoutDpmHasNoDpm()
    {
        string path = Path.Combine(_directory, "nodpm.aif");
        WriteImage(new AaruFormat(), path, null);

        OpenImage<AaruFormat>(path).ReadDpm(out _).Should().Be(ErrorNumber.NoData);
    }

    [Test]
    public void DpmRoundTrips()
    {
        string                  path    = Path.Combine(_directory, "dpm.aif");
        DataPositionMeasurement written = BuildMeasured(263_417);

        WriteImage(new AaruFormat(), path, written);

        OpenImage<AaruFormat>(path).ReadDpm(out DataPositionMeasurement read).Should().Be(ErrorNumber.NoError);
        ShouldBeEqual(read, written);
    }

    [Test]
    public void ResumeKeepsReplacesAndClearsDpm()
    {
        string                  path   = Path.Combine(_directory, "resume.aif");
        DataPositionMeasurement first  = BuildMeasured(263_417);
        DataPositionMeasurement second = BuildMeasured(512_301);

        WriteImage(new AaruFormat(), path, first);

        // Resuming without touching the DPM keeps it, and it can be read while resumed
        var image = new AaruFormat();
        image.Create(path, MediaType.CDROM, new Dictionary<string, string>(), SECTORS, 0, 0, 2352).Should().BeTrue();
        image.ReadDpm(out DataPositionMeasurement resumed).Should().Be(ErrorNumber.NoError);
        ShouldBeEqual(resumed, first);
        image.Close().Should().BeTrue(image.ErrorMessage);

        OpenImage<AaruFormat>(path).ReadDpm(out DataPositionMeasurement read).Should().Be(ErrorNumber.NoError);
        ShouldBeEqual(read, first);

        // Setting it again replaces it
        image = new AaruFormat();
        image.Create(path, MediaType.CDROM, new Dictionary<string, string>(), SECTORS, 0, 0, 2352).Should().BeTrue();
        image.SetDpm(second).Should().BeTrue(image.ErrorMessage);
        image.Close().Should().BeTrue(image.ErrorMessage);

        OpenImage<AaruFormat>(path).ReadDpm(out read).Should().Be(ErrorNumber.NoError);
        ShouldBeEqual(read, second);

        // Clearing it removes it
        image = new AaruFormat();
        image.Create(path, MediaType.CDROM, new Dictionary<string, string>(), SECTORS, 0, 0, 2352).Should().BeTrue();
        image.ClearDpm().Should().BeTrue(image.ErrorMessage);
        image.Close().Should().BeTrue(image.ErrorMessage);

        OpenImage<AaruFormat>(path).ReadDpm(out _).Should().Be(ErrorNumber.NoData);
    }

    [Test]
    public void AlcoholToAaruFormatToAlcoholIsLossless()
    {
        // An Alcohol 120% table: 256 per turn angles on a 50 sectors grid
        var  entries = new DpmEntry[SECTORS / 50 + 1];
        uint angle   = 0;
        entries[0] = new DpmEntry();

        for(int i = 1; i < entries.Length; i++)
        {
            angle += (uint)(1317 + (i / 7 % 2 == 0 ? 30 : 0) - i % 3);

            entries[i] = new DpmEntry
            {
                Lba   = (ulong)i * 50,
                Angle = angle    * Dpm.UNITS_PER_ALCOHOL_UNIT
            };
        }

        var original = new DataPositionMeasurement
        {
            NominalSpacing = 50,
            LayerEnds      = [],
            Entries        = entries,
            Calibrations   = []
        };

        string firstMds = Path.Combine(_directory, "first.mds");
        string aaruf    = Path.Combine(_directory, "converted.aif");
        string lastMds  = Path.Combine(_directory, "last.mds");

        WriteImage(new Alcohol120(), firstMds, original);
        OpenImage<Alcohol120>(firstMds).ReadDpm(out DataPositionMeasurement fromMds).Should().Be(ErrorNumber.NoError);

        WriteImage(new AaruFormat(), aaruf, fromMds);
        OpenImage<AaruFormat>(aaruf).ReadDpm(out DataPositionMeasurement fromAaruf).Should().Be(ErrorNumber.NoError);
        ShouldBeEqual(fromAaruf, fromMds);

        WriteImage(new Alcohol120(), lastMds, fromAaruf);
        OpenImage<Alcohol120>(lastMds).ReadDpm(out DataPositionMeasurement back).Should().Be(ErrorNumber.NoError);
        ShouldBeEqual(back, original);
    }

    [Test]
    public void AaruFormatToAlcoholResamplesToTheGrid()
    {
        DataPositionMeasurement measured = BuildMeasured(263_417);

        string aaruf = Path.Combine(_directory, "measured.aif");
        string mds   = Path.Combine(_directory, "measured.mds");

        WriteImage(new AaruFormat(), aaruf, measured);
        OpenImage<AaruFormat>(aaruf).ReadDpm(out DataPositionMeasurement fromAaruf).Should().Be(ErrorNumber.NoError);

        WriteImage(new Alcohol120(), mds, fromAaruf);
        OpenImage<Alcohol120>(mds).ReadDpm(out DataPositionMeasurement fromMds).Should().Be(ErrorNumber.NoError);

        fromMds.NominalSpacing.Should().Be(50);
        fromMds.Entries.Should().HaveCount(39 + 1);

        foreach(DpmEntry entry in fromMds.Entries)
        {
            ulong expected = Dpm.AngleAt(measured, entry.Lba)!.Value;

            ((long)entry.Angle - (long)expected).Should()
                                                .BeInRange(-(long)Dpm.UNITS_PER_ALCOHOL_UNIT / 2,
                                                           (long)Dpm.UNITS_PER_ALCOHOL_UNIT  / 2);
        }

        // And back to AaruFormat, keeping the grid angles
        string back = Path.Combine(_directory, "back.aif");
        WriteImage(new AaruFormat(), back, fromMds);
        OpenImage<AaruFormat>(back).ReadDpm(out DataPositionMeasurement roundTrip).Should().Be(ErrorNumber.NoError);
        roundTrip.Entries.Should().Equal(fromMds.Entries);
    }
}