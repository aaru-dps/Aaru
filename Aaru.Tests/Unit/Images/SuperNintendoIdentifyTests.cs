// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SuperNintendoIdentifyTests.cs
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

using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Filters;
using Aaru.Images;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Images;

[TestFixture]
[Category("Unit")]
public class SuperNintendoIdentifyTests
{
    const int  KIB            = 1024;
    const long LOROM_HEADER   = 0x7FB0;
    const long HIROM_HEADER   = 0xFFB0;
    const long EXHIROM_HEADER = 0x40FFB0;

    /// <summary>Offsets inside the 48 bytes internal header, which starts 16 bytes before the title.</summary>
    const int TITLE_OFFSET    = 0x10;
    const int MODE_OFFSET     = 0x25;
    const int ROM_SIZE_OFFSET = 0x27;
    const int REGION_OFFSET   = 0x29;
    const int CHECKSUM_OFFSET = 0x2C;

    /// <summary>Builds a ROM with non-zero filler and an internal header at the given location.</summary>
    static byte[] BuildRom(int size, long header, byte mode, byte romSize, string title, bool fillChecksum)
    {
        var rom = new byte[size];

        for(var i = 0; i < size; i += 2) rom[i] = (byte)(i >> 8);

        byte[] titleBytes = Encoding.ASCII.GetBytes(title.PadRight(21));
        titleBytes.CopyTo(rom, header + TITLE_OFFSET);

        rom[header + MODE_OFFSET]     = mode;
        rom[header + ROM_SIZE_OFFSET] = romSize;
        rom[header + REGION_OFFSET]   = 1;

        if(!fillChecksum)
        {
            rom[header + CHECKSUM_OFFSET]     = 0;
            rom[header + CHECKSUM_OFFSET + 1] = 0;
            rom[header + CHECKSUM_OFFSET + 2] = 0;
            rom[header + CHECKSUM_OFFSET + 3] = 0;

            return rom;
        }

        // The checksum is computed as if complement and checksum were 0xFFFF and 0x0000
        rom[header + CHECKSUM_OFFSET]     = 0xFF;
        rom[header + CHECKSUM_OFFSET + 1] = 0xFF;
        rom[header + CHECKSUM_OFFSET + 2] = 0;
        rom[header + CHECKSUM_OFFSET + 3] = 0;

        ushort checksum = rom.Aggregate<byte, ushort>(0, static (sum, b) => (ushort)(sum + b));

        var complement = (ushort)(checksum ^ 0xFFFF);

        rom[header + CHECKSUM_OFFSET]     = (byte)complement;
        rom[header + CHECKSUM_OFFSET + 1] = (byte)(complement >> 8);
        rom[header + CHECKSUM_OFFSET + 2] = (byte)checksum;
        rom[header + CHECKSUM_OFFSET + 3] = (byte)(checksum >> 8);

        return rom;
    }

    static bool Identify(byte[] data)
    {
        var filter = new ZZZNoFilter();
        filter.Open(data).Should().Be(ErrorNumber.NoError);

        return new SuperNintendo().Identify(filter);
    }

    [Test]
    public void IdentifiesLoRom() =>
        Identify(BuildRom(512 * KIB, LOROM_HEADER, 0x20, 9, "AARU TEST ROM", true)).Should().BeTrue();

    [Test]
    public void IdentifiesHiRom() =>
        Identify(BuildRom(1024 * KIB, HIROM_HEADER, 0x31, 10, "AARU TEST ROM", true)).Should().BeTrue();

    [Test]
    public void IdentifiesExHiRom() => Identify(BuildRom(6144 * KIB, EXHIROM_HEADER, 0x35, 13, "AARU TEST ROM", true))
                                      .Should()
                                      .BeTrue();

    [Test]
    public void IdentifiesHomebrewWithoutChecksum() =>
        Identify(BuildRom(256 * KIB, LOROM_HEADER, 0x20, 8, "HOMEBREW", false)).Should().BeTrue();

    [Test]
    public void OpensAndDecodesHeader()
    {
        var filter = new ZZZNoFilter();

        filter.Open(BuildRom(1024 * KIB, HIROM_HEADER, 0x31, 10, "AARU TEST ROM", true))
              .Should()
              .Be(ErrorNumber.NoError);

        var image = new SuperNintendo();
        image.Open(filter).Should().Be(ErrorNumber.NoError);
        image.Info.MediaTitle.Should().Be("AARU TEST ROM");
    }

    [Test]
    public void RejectsZeroedData()
    {
        // ISO 9660 images start with 32KiB of zeros, where a LoROM header would be
        var iso = new byte[512 * KIB];
        "CD001"u8.ToArray().CopyTo(iso, 0x8001);

        Identify(iso).Should().BeFalse();
    }

    [Test]
    public void RejectsUnprintableTitleWithoutChecksum() =>
        Identify(BuildRom(256 * KIB, LOROM_HEADER, 0x20, 8, "\u0001\u0002\u0003", false)).Should().BeFalse();

    [Test]
    public void RejectsMemoryMapNotMatchingLocation() =>
        Identify(BuildRom(256 * KIB, LOROM_HEADER, 0x21, 8, "AARU TEST ROM", true)).Should().BeFalse();

    [Test]
    public void RejectsInvalidMapModeHighBits() =>
        Identify(BuildRom(256 * KIB, LOROM_HEADER, 0xE0, 8, "AARU TEST ROM", true)).Should().BeFalse();

    [Test]
    public void RejectsImpossibleRomSize() =>
        Identify(BuildRom(256 * KIB, LOROM_HEADER, 0x20, 0, "AARU TEST ROM", true)).Should().BeFalse();
}