// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Archivers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the DOS archiver decompressors, mirroring the Aaru.Compression.Native test suite.
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
using Aaru.Compression;
using NUnit.Framework;
using Aaru.Compression.Arc;
using Aaru.Compression.Pak;

namespace Aaru.Tests.Compression;

[TestFixture]
public class ArchiverTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    [Test]
    public void Lh5()
    {
        byte[] source = Helpers.ReadFixture("alice29.lh5");

        using Stream stream = new Lh5Stream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArcPack()
    {
        byte[] source = Helpers.ReadFixture("arcpack.bin");

        using Stream stream = new PackStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArcSqueeze()
    {
        byte[] source = Helpers.ReadFixture("arcsqueeze.bin");

        using Stream stream = new SqueezeStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArcCrunchNoRepack()
    {
        byte[] source = Helpers.ReadFixture("arccrunchnr.bin");

        using Stream stream = new CrunchStream(new MemoryStream(source), Helpers.ALICE29_SIZE, true, false);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArcCrunchDynamic()
    {
        byte[] source = Helpers.ReadFixture("arccrunch_dynamic.bin");

        using Stream stream = new LzwStream(new MemoryStream(source), Helpers.ALICE29_SIZE, false);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArcSquash()
    {
        byte[] source = Helpers.ReadFixture("arcsquash.bin");

        using Stream stream = new LzwStream(new MemoryStream(source), Helpers.ALICE29_SIZE, true);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void PakCrush()
    {
        byte[] source = Helpers.ReadFixture("pak_crush.bin");

        using Stream stream = new CrushStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void PakDistill()
    {
        byte[] source = Helpers.ReadFixture("pak_distill.bin");

        using Stream stream = new DistillStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void HaAsc()
    {
        byte[] source = Helpers.ReadFixture("ha_asc.bin");

        using Stream stream = new HaStream(new MemoryStream(source), Helpers.ALICE29_SIZE, HaStream.HaMethod.ASC);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void HaHsc()
    {
        byte[] source = Helpers.ReadFixture("ha_hsc.bin");

        using Stream stream = new HaStream(new MemoryStream(source), Helpers.ALICE29_SIZE, HaStream.HaMethod.HSC);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void AceV1Lz77()
    {
        byte[] source = Helpers.ReadFixture("ace_v1_lz77.bin");

        using Stream stream = new AceStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 1, 20);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void AceV2Blocked()
    {
        byte[] source = Helpers.ReadFixture("ace_v2_blocked.bin");

        using Stream stream = new AceStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 2, 20);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArjMethod1()
    {
        byte[] source = Helpers.ReadFixture("arj_m1.bin");

        using Stream stream = new ArjStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 1);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArjMethod2()
    {
        byte[] source = Helpers.ReadFixture("arj_m2.bin");

        using Stream stream = new ArjStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 2);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArjMethod3()
    {
        byte[] source = Helpers.ReadFixture("arj_m3.bin");

        using Stream stream = new ArjStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 3);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArjMethod4Fastest()
    {
        byte[] source = Helpers.ReadFixture("arj_m4.bin");

        using Stream stream = new ArjStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 4);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArjzDefault()
    {
        byte[] source = Helpers.ReadFixture("arjz_default.bin");

        using Stream stream = new ArjzStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 1);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void ArjzV55Deflatez()
    {
        byte[] source = Helpers.ReadFixture("arjz_v55_new.bin");

        using Stream stream = new ArjzStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 5);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Rar15()
    {
        byte[] source = Helpers.ReadFixture("rar15_m5.bin");

        using Stream stream = new RarStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 15);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Rar20()
    {
        byte[] source = Helpers.ReadFixture("rar20_default.bin");

        using Stream stream = new RarStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 20);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Rar30()
    {
        byte[] source = Helpers.ReadFixture("rar30_default.bin");

        using Stream stream = new RarStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 29);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Rar50()
    {
        byte[] source = Helpers.ReadFixture("rar50_default.bin");

        using Stream stream = new RarStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 50, 262144);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Rar50Method3()
    {
        byte[] source = Helpers.ReadFixture("rar50_m3.bin");

        using Stream stream = new RarStream(new MemoryStream(source), 246814, 50, 1048576);

        Helpers.AssertStream(stream, 246814, 0x3AE33007);
    }

    [Test]
    public void Rar50Method4()
    {
        byte[] source = Helpers.ReadFixture("rar50_m4.bin");

        using Stream stream = new RarStream(new MemoryStream(source), 246814, 50, 1048576);

        Helpers.AssertStream(stream, 246814, 0x3AE33007);
    }

    [Test]
    public void Rar50Method5()
    {
        byte[] source = Helpers.ReadFixture("rar50_m5.bin");

        using Stream stream = new RarStream(new MemoryStream(source), 246814, 50, 1048576);

        Helpers.AssertStream(stream, 246814, 0x3AE33007);
    }
}