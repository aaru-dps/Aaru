// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol120DpmTests.cs
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
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Filters;
using Aaru.Images;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Images;

[TestFixture]
[Category("Unit")]
public class Alcohol120DpmTests
{
    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

    [SetUp]
    public void SetUp() => _directory = Directory.CreateTempSubdirectory("aaru-dpm-").FullName;

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, true);

    const ulong SECTORS = 2000;
    string      _directory;

    /// <summary>Writes a single track CD image with the given DPM and returns the path to its descriptor</summary>
    string WriteImage(DataPositionMeasurement? dpm)
    {
        string path  = Path.Combine(_directory, "test.mds");
        var    image = new Alcohol120();

        image.Create(path, MediaType.CDROM, new Dictionary<string, string>(), SECTORS, 0, 0, 2352)
             .Should()
             .BeTrue(image.ErrorMessage);

        image.SetTracks([
                  new Track
                  {
                      Sequence          = 1,
                      Session           = 1,
                      StartSector       = 0,
                      EndSector         = SECTORS - 1,
                      Type              = TrackType.CdMode1,
                      BytesPerSector    = 2048,
                      RawBytesPerSector = 2352,
                      SubchannelType    = TrackSubchannelType.None,
                      Indexes           = new Dictionary<ushort, int>
                      {
                          [1] = 0
                      }
                  }
              ])
             .Should()
             .BeTrue(image.ErrorMessage);

        var data   = new byte[2352 * SECTORS];
        var status = new SectorStatus[SECTORS];
        Array.Fill(status, SectorStatus.Dumped);

        image.WriteSectorsLong(data, 0, false, (uint)SECTORS, status).Should().BeTrue(image.ErrorMessage);

        if(dpm.HasValue) image.SetDpm(dpm.Value).Should().BeTrue(image.ErrorMessage);

        image.Close().Should().BeTrue(image.ErrorMessage);

        return path;
    }

    static Alcohol120 OpenImage(string path)
    {
        var filter = new ZZZNoFilter();
        filter.Open(path).Should().Be(ErrorNumber.NoError);

        var image = new Alcohol120();
        image.Open(filter).Should().Be(ErrorNumber.NoError);

        return image;
    }

    /// <summary>Builds an Alcohol 120% style DPM, with 256 per turn angles on a 50 sectors grid</summary>
    static DataPositionMeasurement BuildGrid()
    {
        var  entries = new DpmEntry[SECTORS / 50 + 1];
        uint angle   = 0;

        entries[0] = new DpmEntry();

        for(int i = 1; i < entries.Length; i++)
        {
            // About 1317 units per entry, like a CD near its centre, with SecuROM-like density bands
            angle += (uint)(1317 + (i / 7 % 2 == 0 ? 30 : 0) - i % 3);

            entries[i] = new DpmEntry
            {
                Lba   = (ulong)i * 50,
                Angle = angle * Dpm.UNITS_PER_ALCOHOL_UNIT
            };
        }

        return new DataPositionMeasurement
        {
            NominalSpacing = 50,
            LayerEnds      = [],
            Entries        = entries,
            Calibrations   = []
        };
    }

    [Test]
    public void ImageWithoutDpmHasNoDpm()
    {
        Alcohol120 image = OpenImage(WriteImage(null));

        image.ReadDpm(out _).Should().Be(ErrorNumber.NoData);
    }

    [Test]
    public void GridDpmRoundTripsExactly()
    {
        DataPositionMeasurement written = BuildGrid();
        Alcohol120              image   = OpenImage(WriteImage(written));

        image.ReadDpm(out DataPositionMeasurement read).Should().Be(ErrorNumber.NoError);

        read.NominalSpacing.Should().Be(50);
        read.Entries.Should().HaveCount(written.Entries.Length);

        for(int i = 0; i < read.Entries.Length; i++)
        {
            read.Entries[i].Lba.Should().Be(written.Entries[i].Lba);
            read.Entries[i].Angle.Should().Be(written.Entries[i].Angle);
            read.Entries[i].Status.Should().Be(DpmEntryStatus.Unknown);
        }
    }

    [Test]
    public void MeasuredDpmIsResampledToTheGrid()
    {
        // Uneven control points, like a measurement around a layer break with a short last bin, with angles that are
        // not whole Alcohol units
        ulong[] lbas = [0, 50, 99, 100, 150, 200, 1990];

        var entries = new DpmEntry[lbas.Length];

        for(int i = 0; i < lbas.Length; i++)
        {
            entries[i] = new DpmEntry
            {
                Lba    = lbas[i],
                Angle  = lbas[i] * 263_417,
                Status = DpmEntryStatus.Measured
            };
        }

        var written = new DataPositionMeasurement
        {
            NominalSpacing = 50,
            TimingUnit     = 1,
            LayerEnds      = [99],
            Entries        = entries,
            Calibrations =
            [
                new DpmCalibration
                {
                    Lba            = 0,
                    RotationPeriod = 7_500_000,
                    SectorsPerTurn = 9_718_000
                }
            ]
        };

        Alcohol120 image = OpenImage(WriteImage(written));

        image.ReadDpm(out DataPositionMeasurement read).Should().Be(ErrorNumber.NoError);

        // 1990 / 50 grid points after the start
        read.Entries.Should().HaveCount(39 + 1);

        foreach(DpmEntry entry in read.Entries)
        {
            ulong expected = entry.Lba * 263_417;

            ((long)entry.Angle - (long)expected).Should()
                                                .BeInRange(-(long)Dpm.UNITS_PER_ALCOHOL_UNIT / 2,
                                                           (long)Dpm.UNITS_PER_ALCOHOL_UNIT / 2);
        }

        read.LayerEnds.Should().BeEmpty();
        read.Calibrations.Should().BeEmpty();
    }

    [Test]
    public void InvalidDpmIsRefused()
    {
        string path  = Path.Combine(_directory, "invalid.mds");
        var    image = new Alcohol120();

        image.Create(path, MediaType.CDROM, new Dictionary<string, string>(), SECTORS, 0, 0, 2352).Should().BeTrue();

        image.SetDpm(new DataPositionMeasurement
              {
                  NominalSpacing = 50,
                  Entries        = [new DpmEntry()]
              })
             .Should()
             .BeFalse();
    }
}