// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mac.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the Compact Pro, DiskDoubler and StuffIt decompressors, mirroring the
//     Aaru.Compression.Native test suite.
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
using Aaru.Compression.DiskDoubler;
using Aaru.Compression.StuffIt;
using NUnit.Framework;

namespace Aaru.Tests.Compression;

[TestFixture]
public class MacTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    [Test]
    public void CompactProRle()
    {
        byte[] source = Helpers.ReadFixture("cpt_rle.bin");

        using Stream stream = new Aaru.Compression.CompactPro.RleStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void CompactProLzhRle()
    {
        byte[] source = Helpers.ReadFixture("cpt_lzh_rle.bin");

        using Stream stream = new Aaru.Compression.CompactPro.LzhStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void DiskDoublerAdn1()
    {
        byte[] source = Helpers.ReadFixture("dd_ad1.bin");

        using Stream stream = new AdnStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void DiskDoublerAdn2()
    {
        byte[] source = Helpers.ReadFixture("dd_ad2.bin");

        using Stream stream = new AdnStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void DiskDoublerDdn1()
    {
        byte[] source = Helpers.ReadFixture("dd_dd1.bin");

        using Stream stream = new DdnStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void DiskDoublerDdn2()
    {
        byte[] source = Helpers.ReadFixture("dd_dd2.bin");

        using Stream stream = new DdnStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void DiskDoublerDdn3()
    {
        byte[] source = Helpers.ReadFixture("dd_dd3.bin");

        using Stream stream = new DdnStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void StuffItCompress()
    {
        byte[] source = Helpers.ReadFixture("stuffit_compress.bin");

        using Stream stream = new CompressStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void StuffItMethod13()
    {
        byte[] source = Helpers.ReadFixture("stuffit_method13.bin");

        using Stream stream = new Method13Stream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void StuffItMethod13V5()
    {
        byte[] source = Helpers.ReadFixture("stuffit_method13_v5.bin");

        using Stream stream = new Method13Stream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void StuffItArsenic()
    {
        byte[] source = Helpers.ReadFixture("stuffit_arsenic.bin");

        using Stream stream = new ArsenicStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void StuffItShrinkWrap()
    {
        byte[] source      = Helpers.ReadFixture("stuffit_shrinkwrap.bin");
        byte[] destination = new byte[65536];
        int    size        = ShrinkWrap.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, 65536, 0x93A5E340);
    }
}