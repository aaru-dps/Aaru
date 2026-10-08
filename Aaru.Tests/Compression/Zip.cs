// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Zip.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the ZIP decompressors, mirroring the Aaru.Compression.Native test suite.
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
using Aaru.Compression.Zip;

namespace Aaru.Tests.Compression;

[TestFixture]
public class ZipTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    [Test]
    public void Shrink()
    {
        byte[] source = Helpers.ReadFixture("zip_shrink.bin");

        using Stream stream = new ShrinkStream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Implode()
    {
        byte[] source = Helpers.ReadFixture("zip_implode.bin");

        using Stream stream = new ImplodeStream(new MemoryStream(source), Helpers.ALICE29_SIZE, true, true);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Deflate64()
    {
        byte[] source = Helpers.ReadFixture("zip_deflate64.bin");

        using Stream stream = new Deflate64Stream(new MemoryStream(source), Helpers.ALICE29_SIZE);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }

    [Test]
    public void Ppmd()
    {
        byte[] source = Helpers.ReadFixture("zip_ppmd.bin");

        using Stream stream = new PpmdStream(new MemoryStream(source), Helpers.ALICE29_SIZE, 8, 4194304, false);

        Helpers.AssertStream(stream, Helpers.ALICE29_SIZE, Helpers.ALICE29_CRC);
    }
}