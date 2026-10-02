// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660Acorn.cs
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using FluentAssertions;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Filesystems;

/// <summary>Checks the Acorn ARCHIMEDES extension support of the ISO9660 plugin against an image with known contents.</summary>
/// <remarks>
///     genisoimage_archimedes.iso was created with genisoimage, then 32 byte ARCHIMEDES system use areas were added by
///     hand to its directory records, as the cdrtools ARCHIMEDES patch records them: /TEXTFILE has filetype &amp;FFF,
///     /LOCKED filetype &amp;FFD and the locked attribute, /DATA load address &amp;8000 and execution address &amp;8023
///     without a filetype, and /_APP is the application directory !APP, with filetype &amp;FED, holding /_APP/_RUN, a
///     file without the application flag.
/// </remarks>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class Iso9660Acorn
{
    const string ARCHIMEDES = "genisoimage_archimedes.iso";

    [Test]
    public void ApplicationNames()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(ARCHIMEDES, "normal");

        try
        {
            Iso9660Rrip.ReadDir(fs, "/").Should().BeEquivalentTo("DATA", "LOCKED", "TEXTFILE", "!APP");

            // Only names whose record has the application flag get their exclamation mark back
            Iso9660Rrip.ReadDir(fs, "/!APP").Should().BeEquivalentTo("_RUN");
            Encoding.ASCII.GetString(Iso9660Rrip.ReadFile(fs, "/!APP/_RUN", 0, 4, 4)).Should().Be("run\n");
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void Filetypes()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(ARCHIMEDES, "normal");

        try
        {
            Iso9660Rrip.ListXAttr(fs, "/TEXTFILE").Should().BeEquivalentTo("riscos.type", "riscos.attr");
            Iso9660Rrip.GetXattr(fs, "/TEXTFILE", "riscos.type").Should().Equal(0xFF, 0x0F);
            Iso9660Rrip.GetXattr(fs, "/TEXTFILE", "riscos.attr").Should().Equal(0x33, 0x00, 0x00, 0x00);

            Iso9660Rrip.GetXattr(fs, "/!APP", "riscos.type").Should().Equal(0xED, 0x0F);
            Iso9660Rrip.GetXattr(fs, "/!APP", "riscos.attr").Should().Equal(0x33, 0x01, 0x00, 0x00);
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void LoadAndExecutionAddresses()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(ARCHIMEDES, "normal");

        try
        {
            Iso9660Rrip.ListXAttr(fs, "/DATA")
                       .Should()
                       .BeEquivalentTo("riscos.loadaddr", "riscos.execaddr", "riscos.attr");

            Iso9660Rrip.GetXattr(fs, "/DATA", "riscos.loadaddr").Should().Equal(0x00, 0x80, 0x00, 0x00);
            Iso9660Rrip.GetXattr(fs, "/DATA", "riscos.execaddr").Should().Equal(0x23, 0x80, 0x00, 0x00);
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }

    [Test]
    public void LockedIsReadOnly()
    {
        (ISO9660 fs, IMediaImage image) = Iso9660Rrip.Mount(ARCHIMEDES, "normal");

        try
        {
            Iso9660Rrip.Stat(fs, "/LOCKED").Attributes.Should().Be(FileAttributes.ReadOnly);
            Iso9660Rrip.Stat(fs, "/TEXTFILE").Attributes.Should().Be(FileAttributes.None);
        }
        finally
        {
            Iso9660Rrip.Unmount(fs, image);
        }
    }
}