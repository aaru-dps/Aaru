// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite5DpmTests.cs
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
using System.Buffers.Binary;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using Aaru.Images;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Images;

/// <summary>
///     BlindWrite stores DPM in its descriptor and in an external BWA file, laid out as libmirage reads them. No
///     image with DPM is available, so the layouts are built here.
/// </summary>
[TestFixture]
[Category("Unit")]
public class BlindWrite5DpmTests
{
    [SetUp]
    public void SetUp() => _directory = Directory.CreateTempSubdirectory("aaru-bw-dpm-").FullName;

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, true);

    string _directory;

    /// <summary>Builds a DPM block: fixed fields, then start, resolution, count and the angles</summary>
    static byte[] Block(uint[] header, uint start, uint resolution, uint[] angles)
    {
        var data = new byte[(header.Length + 3 + angles.Length) * 4];
        int pos  = 0;

        foreach(uint value in header)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(pos), value);
            pos += 4;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(pos),     start);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(pos + 4), resolution);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(pos + 8), (uint)angles.Length);
        pos += 12;

        foreach(uint angle in angles)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(pos), angle);
            pos += 4;
        }

        return data;
    }

    /// <summary>Internal block: 1, 1, 0, 0, the length twice, 0, 1</summary>
    static byte[] InternalBlock(uint[] angles) =>
        Block([1, 1, 0, 0, (uint)(12 + angles.Length * 4), (uint)(12 + angles.Length * 4), 0, 1], 0, 50, angles);

    /// <summary>BWA file: 1, 8, 1, start, resolution, count, a value that is not an angle, then the angles</summary>
    static byte[] Bwa(uint[] angles) => Block([1, 8, 1], 0, 50, [14609492, ..angles]);

    static BlindWrite5 Load(string descriptor, byte[] internalBlock)
    {
        File.WriteAllBytes(descriptor, [0]);

        var filter = new ZZZNoFilter();
        filter.Open(descriptor).Should().Be(ErrorNumber.NoError);

        var image = new BlindWrite5
        {
            _dpm = internalBlock
        };

        image.LoadDpm(filter);

        return image;
    }

    [Test]
    public void DecodesInternalBlock()
    {
        BlindWrite5 image = Load(Path.Combine(_directory, "disc.b6t"), InternalBlock([1317, 2634, 3952]));

        image.ReadDpm(out DataPositionMeasurement dpm).Should().Be(ErrorNumber.NoError);

        dpm.NominalSpacing.Should().Be(50);
        dpm.Entries.Should().HaveCount(4);
        dpm.Entries[0].Lba.Should().Be(0);
        dpm.Entries[0].Angle.Should().Be(0);
        dpm.Entries[3].Lba.Should().Be(150);
        dpm.Entries[3].Angle.Should().Be(3952 * Dpm.UNITS_PER_ALCOHOL_UNIT);
    }

    [Test]
    public void PrefersTheBwaFile()
    {
        string descriptor = Path.Combine(_directory, "disc.b6t");

        // Different case than the descriptor, as found on case sensitive file systems
        File.WriteAllBytes(Path.Combine(_directory, "disc.BWA"), Bwa([1300, 2600, 3900, 5200, 6500]));

        BlindWrite5 image = Load(descriptor, InternalBlock([1317, 2634]));

        image.ReadDpm(out DataPositionMeasurement dpm).Should().Be(ErrorNumber.NoError);

        dpm.Entries.Should().HaveCount(6);
        dpm.Entries[5].Angle.Should().Be(6500 * Dpm.UNITS_PER_ALCOHOL_UNIT);
    }

    [Test]
    public void FallsBackToTheInternalBlockWithAnInvalidBwa()
    {
        string descriptor = Path.Combine(_directory, "disc.b5t");
        File.WriteAllBytes(Path.Combine(_directory, "disc.bwa"), [1, 0, 0, 0]);

        BlindWrite5 image = Load(descriptor, InternalBlock([1317, 2634]));

        image.ReadDpm(out DataPositionMeasurement dpm).Should().Be(ErrorNumber.NoError);
        dpm.Entries.Should().HaveCount(3);
    }

    [Test]
    public void NoDpm()
    {
        BlindWrite5 image = Load(Path.Combine(_directory, "disc.b6t"), []);

        image.ReadDpm(out _).Should().Be(ErrorNumber.NoData);
    }

    [Test]
    public void BwaSkipsTheValueThatIsNotAnAngle()
    {
        DataPositionMeasurement? dpm = BlindWrite4.DecodeBwa(Bwa([1362, 2723, 4082]));

        dpm.Should().NotBeNull();
        dpm.Value.Entries.Should().HaveCount(4);
        dpm.Value.Entries[1].Lba.Should().Be(50);
        dpm.Value.Entries[1].Angle.Should().Be(1362 * Dpm.UNITS_PER_ALCOHOL_UNIT);

        // Without that value, as libmirage expects them, every value is an angle
        dpm = BlindWrite4.DecodeBwa(Block([1, 8, 1], 0, 50, [1362, 2723, 4082]));

        dpm.Should().NotBeNull();
        dpm.Value.Entries.Should().HaveCount(4);
    }

    [Test]
    public void BlindWrite4ReadsItsBwaFile()
    {
        File.WriteAllBytes(Path.Combine(_directory, "disc.BWA"), Bwa([1362, 2723, 4082]));
        DataPositionMeasurement? dpm = BlindWrite4.DecodeBwa(File.ReadAllBytes(Path.Combine(_directory, "disc.BWA")));

        dpm.Should().NotBeNull();
        dpm.Value.Entries[^1].Lba.Should().Be(150);
    }

    [Test]
    public void RejectsTruncatedOrDecreasingAngles()
    {
        byte[] truncated = InternalBlock([1317, 2634, 3952]);
        BlindWrite5.DecodeDpm(truncated[..^4], 32).Should().BeNull();

        BlindWrite5.DecodeDpm(InternalBlock([2634, 1317]), 32).Should().BeNull();
    }
}