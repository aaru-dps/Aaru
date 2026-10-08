// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : StuffItX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the StuffIt X decompressors, mirroring the Aaru.Compression.Native test suite.
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

using System.IO;
using Aaru.Compression.StuffItX;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Compression;

[TestFixture]
public class StuffItXTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    /// <summary>Capacity of the intermediate buffer, as in the native test suite</summary>
    const int MIDDLE_SIZE = Helpers.ALICE29_SIZE * 2;

    [Test]
    public void Brimstone()
    {
        byte[] source = Helpers.ReadFixture("stuffitx_brimstone.bin");

        // First byte is the exponent of the sub-allocator size, second one the model order
        int subAllocSize = 1 << source[0];
        int maxOrder     = source[1];

        using Stream stage1 = new BrimstoneStream(new MemoryStream(source, 2, source.Length - 2),
                                                  MIDDLE_SIZE,
                                                  maxOrder,
                                                  subAllocSize);
        byte[]       middle = Helpers.ReadAll(stage1);

        middle.Should().NotBeEmpty("first stage must produce data");

        using Stream english = new EnglishStream(new MemoryStream(middle), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(english, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Cyanide()
    {
        byte[] source = Helpers.ReadFixture("stuffitx_cyanide.bin");

        using Stream stage1 = new CyanideStream(new MemoryStream(source), MIDDLE_SIZE);
        byte[]       middle = Helpers.ReadAll(stage1);

        middle.Should().NotBeEmpty("first stage must produce data");

        using Stream english = new EnglishStream(new MemoryStream(middle), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(english, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Darkhorse()
    {
        byte[] source = Helpers.ReadFixture("stuffitx_darkhorse.bin");

        // First byte is the window size in bits
        int windowBits = source[0];

        using Stream stage1 = new DarkhorseStream(new MemoryStream(source, 1, source.Length - 1),
                                                  MIDDLE_SIZE,
                                                  windowBits);
        byte[]       middle = Helpers.ReadAll(stage1);

        middle.Should().NotBeEmpty("first stage must produce data");

        using Stream english = new EnglishStream(new MemoryStream(middle), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(english, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Deflate()
    {
        byte[] source = Helpers.ReadFixture("stuffitx_deflate.bin");

        using Stream stage1 = new DeflateStream(new MemoryStream(source, 1, source.Length - 1), MIDDLE_SIZE);
        byte[]       middle = Helpers.ReadAll(stage1);

        middle.Should().NotBeEmpty("first stage must produce data");

        using Stream english = new EnglishStream(new MemoryStream(middle), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(english, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Blend()
    {
        byte[] source = Helpers.ReadFixture("stuffitx_blend.bin");

        using Stream stage1 = new BlendStream(new MemoryStream(source), MIDDLE_SIZE);
        byte[]       middle = Helpers.ReadAll(stage1);

        middle.Should().NotBeEmpty("first stage must produce data");

        using Stream english = new EnglishStream(new MemoryStream(middle), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(english, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }
}