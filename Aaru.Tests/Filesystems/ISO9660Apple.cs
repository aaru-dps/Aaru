// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660Apple.cs
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
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filesystems;
using FluentAssertions;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Filesystems;

/// <summary>Checks the Apple extensions to ISO 9660 support of the ISO9660 plugin against images with known contents.</summary>
/// <remarks>
///     <para>
///         genisoimage_apple.iso and genisoimage_apple_xa.iso were created with genisoimage -apple, with Rock Ridge
///         and CD-ROM XA respectively, from macOS AppleDouble files. /text.txt is TEXT/ttxt with the system file bit,
///         /app is APPL/TEST with the bundle and alias bits, both with resource forks, and /plain.bin has none.
///     </para>
///     <para>
///         genisoimage_apple_patched.iso is genisoimage_apple.iso with the Apple extensions protocol identifier in the
///         system identifier, an old "BA" extension with identifier 3 (bundle bit set) for /text.txt, a ProDOS "AA"
///         extension (type $04, auxiliary type $1234) for /plain.bin, and no Apple extension for /app, whose
///         associated file must still be its resource fork as the volume declares the Apple extensions.
///     </para>
///     <para>
///         genisoimage_prodos_names.iso was created with genisoimage from ProDOS names with their periods turned into
///         underscores, with a protocol identifier telling the ProDOS filenames were transformed.
///     </para>
/// </remarks>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class Iso9660Apple
{
    const string APPLE        = "genisoimage_apple.iso";
    const string APPLE_XA     = "genisoimage_apple_xa.iso";
    const string PATCHED      = "genisoimage_apple_patched.iso";
    const string PRODOS_NAMES = "genisoimage_prodos_names.iso";

    const string TEXT_FINDER_INFO =
        "5445585474747874100000000000000000000000000000000000000000000000";

    [Test]
    public void ResourceForkKeepsFileAttributes()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(APPLE);

        try
        {
            // The data fork record, after its associated file, has the attributes of the file
            FileEntryInfo text = Iso9660Rrip.Stat(fs, "/text.txt");
            text.Mode.Should().Be(Convert.ToUInt32("644", 8));
            text.UID.Should().Be(1000);
            text.Attributes.Should().Be(FileAttributes.System);

            Iso9660Rrip.ListXAttr(fs, "/text.txt")
                       .Should()
                       .BeEquivalentTo("com.apple.ResourceFork", "com.apple.FinderInfo", "hfs.type", "hfs.creator");

            Convert.ToHexString(Iso9660Rrip.GetXattr(fs, "/text.txt", "com.apple.FinderInfo"))
                   .Should()
                   .BeEquivalentTo(TEXT_FINDER_INFO);

            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/text.txt", "hfs.type")).Should().Be("TEXT");
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/text.txt", "hfs.creator")).Should().Be("ttxt");

            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/text.txt", "com.apple.ResourceFork"))
                    .Should()
                    .Be("RESOURCE FORK OF TEXT");

            Encoding.ASCII.GetString(Iso9660Rrip.ReadFile(fs, "/text.txt", 0, 10, 10)).Should().Be("text data\n");

            Iso9660Rrip.Stat(fs, "/app").Attributes.Should().Be(FileAttributes.Alias | FileAttributes.Bundle);
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/app", "hfs.type")).Should().Be("APPL");
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/app", "hfs.creator")).Should().Be("TEST");
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void ExtensionsAfterCdromXa()
    {
        // Without Rock Ridge, the Apple extensions follow the 14 bytes of CD-ROM XA data
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(APPLE_XA, "normal");

        try
        {
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/TEXT.TXT", "hfs.type")).Should().Be("TEXT");
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/TEXT.TXT", "hfs.creator")).Should().Be("ttxt");

            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/TEXT.TXT", "com.apple.ResourceFork"))
                    .Should()
                    .Be("RESOURCE FORK OF TEXT");

            Iso9660Rrip.Stat(fs, "/TEXT.TXT").Attributes.Should().HaveFlag(FileAttributes.System);
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void ProDosExtension()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(PATCHED);

        try
        {
            Iso9660Rrip.ListXAttr(fs, "/plain.bin").Should().BeEquivalentTo("prodos.type", "prodos.aux_type");
            Iso9660Rrip.GetXattr(fs, "/plain.bin", "prodos.type").Should().Equal(0x04);
            Iso9660Rrip.GetXattr(fs, "/plain.bin", "prodos.aux_type").Should().Equal(0x34, 0x12);
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void OldExtensionWithBundleBit()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(PATCHED);

        try
        {
            // Rock Ridge entries after the padding byte of the "BA" extension are still found
            Iso9660Rrip.ReadDir(fs, "/").Should().Contain("text.txt");

            Iso9660Rrip.Stat(fs, "/text.txt").Attributes.Should().Be(FileAttributes.Bundle);
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/text.txt", "hfs.type")).Should().Be("TEXT");
            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/text.txt", "hfs.creator")).Should().Be("ttxt");
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void ProtocolMakesAssociatedFilesResourceForks()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(PATCHED);

        try
        {
            List<string> xattrs = Iso9660Rrip.ListXAttr(fs, "/app");
            xattrs.Should().Contain("com.apple.ResourceFork");
            xattrs.Should().NotContain("org.iso.9660.AssociatedFile");

            Encoding.ASCII.GetString(Iso9660Rrip.GetXattr(fs, "/app", "com.apple.ResourceFork"))
                    .Should()
                    .Be("RESOURCE FORK OF APP!");
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void ProDosFilenamesRestored()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(PRODOS_NAMES, "normal");

        try
        {
            Iso9660Rrip.ReadDir(fs, "/").Should().BeEquivalentTo("BASIC.SY", "DESK.ACC", "PRODOS");
            Iso9660Rrip.ReadDir(fs, "/DESK.ACC").Should().BeEquivalentTo("CONTROL.");

            Encoding.ASCII.GetString(Iso9660Rrip.ReadFile(fs, "/DESK.ACC/CONTROL.", 0, 4, 4)).Should().Be("acc\n");
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void ProDosFilenamesOnlyInPlainNamespace()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(PRODOS_NAMES, "vms");

        try
        {
            Iso9660Rrip.ReadDir(fs, "/").Should().Contain("DESK_ACC");
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }
}