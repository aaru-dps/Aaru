// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AmigaDosLongNameTests.cs
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
public class AmigaDosLongNameTests
{
    // Merged name and comment starts 184 bytes before the end of the block on DOS\6 and DOS\7
    const int NAC_FROM_END = 184;

    static byte[] BuildLongNameBlock(int blockSize, string name, string comment, uint commentBlock)
    {
        var block = new byte[blockSize];
        int nac   = blockSize - NAC_FROM_END;

        block[nac] = (byte)name.Length;
        Encoding.ASCII.GetBytes(name).CopyTo(block, nac + 1);

        int commentOffset = nac + 1 + name.Length;
        block[commentOffset] = (byte)(comment?.Length ?? 0);

        if(comment is not null) Encoding.ASCII.GetBytes(comment).CopyTo(block, commentOffset + 1);

        block[blockSize - 72]     = (byte)(commentBlock >> 24);
        block[blockSize - 72 + 1] = (byte)(commentBlock >> 16);
        block[blockSize - 72 + 2] = (byte)(commentBlock >> 8);
        block[blockSize - 72 + 3] = (byte)commentBlock;

        return block;
    }

    static string Ascii(byte[] data) => Encoding.ASCII.GetString(data);

    [TestCase(512)]
    [TestCase(1024)]
    public void NameAndCommentAreMerged(int blockSize)
    {
        byte[] block = BuildLongNameBlock(blockSize, "barney", "fred", 0);

        Ascii(AmigaDOSPlugin.GetEntryNameBytes(block, true)).Should().Be("barney");
        Ascii(AmigaDOSPlugin.GetInlineCommentBytes(block, true)).Should().Be("fred");
        AmigaDOSPlugin.GetCommentBlockPointer(block, true).Should().Be(0);
    }

    [TestCase(512)]
    [TestCase(1024)]
    public void NameWithoutComment(int blockSize)
    {
        byte[] block = BuildLongNameBlock(blockSize, "wilma", null, 0);

        Ascii(AmigaDOSPlugin.GetEntryNameBytes(block, true)).Should().Be("wilma");
        AmigaDOSPlugin.GetInlineCommentBytes(block, true).Should().BeEmpty();
    }

    [Test]
    public void MaximumLengthNameWithCommentBlock()
    {
        string name  = new('n', 107);
        byte[] block = BuildLongNameBlock(512, name, null, 1234);

        Ascii(AmigaDOSPlugin.GetEntryNameBytes(block, true)).Should().Be(name);
        AmigaDOSPlugin.GetInlineCommentBytes(block, true).Should().BeEmpty();
        AmigaDOSPlugin.GetCommentBlockPointer(block, true).Should().Be(1234);
    }

    [Test]
    public void OverlongNameIsCapped()
    {
        byte[] block = BuildLongNameBlock(512, new string('n', 107), null, 0);
        block[512 - NAC_FROM_END] = 200;

        AmigaDOSPlugin.GetEntryNameBytes(block, true).Should().HaveCount(107);
    }

    [Test]
    public void CommentMustFitInMergedField()
    {
        // 100 character name leaves no room for a 20 character comment in the 112 byte field
        byte[] block = BuildLongNameBlock(512, new string('n', 100), null, 0);
        block[512 - NAC_FROM_END + 101] = 20;

        AmigaDOSPlugin.GetInlineCommentBytes(block, true).Should().BeEmpty();
    }

    [Test]
    public void ClassicLayoutIsUnchanged()
    {
        var block = new byte[512];
        block[512 - 184] = 4;
        Encoding.ASCII.GetBytes("fred").CopyTo(block, 512 - 184 + 1);
        block[512 - 80] = 6;
        Encoding.ASCII.GetBytes("barney").CopyTo(block, 512 - 80 + 1);

        Ascii(AmigaDOSPlugin.GetEntryNameBytes(block, false)).Should().Be("barney");
        Ascii(AmigaDOSPlugin.GetInlineCommentBytes(block, false)).Should().Be("fred");
        AmigaDOSPlugin.GetCommentBlockPointer(block, false).Should().Be(0);
    }

    [TestCase(512,  false, 420)]
    [TestCase(512,  true,  452)]
    [TestCase(1024, true,  964)]
    public void DateOffset(int blockSize, bool longNames, int expected) =>
        AmigaDOSPlugin.GetEntryDateOffset(blockSize, longNames).Should().Be(expected);
}