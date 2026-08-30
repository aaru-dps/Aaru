// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Iso9660ShiftJisTests.cs
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

using System.Text;
using Aaru.Filesystems;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Filesystems;

[TestFixture]
[Category("Unit")]
public class Iso9660ShiftJisTests
{
    // 日本語 in Shift-JIS
    static readonly byte[] _kanji = [0x93, 0xFA, 0x96, 0x7B, 0x8C, 0xEA, 0x20, 0x20];

    // Half-width katakana ｱｲｳ
    static readonly byte[] _katakana = [0xB1, 0xB2, 0xB3, 0x20];

    [Test]
    public void AsciiIsNotShiftJis() =>
        ISO9660.IsShiftJis(Encoding.ASCII.GetBytes("AARU_CDROM      ")).Should().BeFalse();

    [Test]
    public void EmptyIsNotShiftJis()
    {
        ISO9660.IsShiftJis([0x20, 0x20, 0x20, 0x20]).Should().BeFalse();
        ISO9660.IsShiftJis([]).Should().BeFalse();
        ISO9660.IsShiftJis(null).Should().BeFalse();
    }

    [Test]
    public void KanjiIsShiftJis() => ISO9660.IsShiftJis(_kanji).Should().BeTrue();

    [Test]
    public void MixedAsciiAndKanjiIsShiftJis() =>
        ISO9660.IsShiftJis([(byte)'C', (byte)'D', 0x20, 0x93, 0xFA, 0x96, 0x7B, 0x20]).Should().BeTrue();

    [Test]
    public void KatakanaRunIsShiftJis() => ISO9660.IsShiftJis(_katakana).Should().BeTrue();

    [Test]
    public void SingleKatakanaByteIsNotShiftJis() =>
        ISO9660.IsShiftJis([(byte)'C', (byte)'D', 0xB1, 0x20]).Should().BeFalse();

    [Test]
    public void UnpairedLeadByteIsNotShiftJis() =>
        ISO9660.IsShiftJis([(byte)'C', (byte)'D', 0x93, 0x20, 0x20]).Should().BeFalse();

    [Test]
    public void TruncatedLeadByteIsNotShiftJis() => ISO9660.IsShiftJis([0x93, 0xFA, 0x96]).Should().BeFalse();

    [Test]
    public void InvalidByteIsNotShiftJis()
    {
        ISO9660.IsShiftJis([(byte)'C', 0xFF, 0x20]).Should().BeFalse();
        ISO9660.IsShiftJis([(byte)'C', 0x80, 0x41, 0x20]).Should().BeFalse();
        ISO9660.IsShiftJis([(byte)'C', 0xA0, 0x41, 0x20]).Should().BeFalse();
    }

    [Test]
    public void Latin1IsNotShiftJis() =>
        ISO9660.IsShiftJis(Encoding.GetEncoding("iso8859-1").GetBytes("VOLUMÉN ")).Should().BeFalse();

    [Test]
    public void DecodesShiftJisIdentifier()
    {
        var shiftJis = false;

        ISO9660.DecodeIdentifier(_kanji, Encoding.ASCII, ref shiftJis).Should().Be("日本語");

        shiftJis.Should().BeTrue();
    }

    [Test]
    public void DecodesAsciiIdentifierWithFallback()
    {
        var shiftJis = false;

        ISO9660.DecodeIdentifier(Encoding.ASCII.GetBytes("AARU_CDROM      "), Encoding.ASCII, ref shiftJis)
               .Should()
               .Be("AARU_CDROM");

        shiftJis.Should().BeFalse();
    }
}
