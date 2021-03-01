// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LisaFS.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems.LisaFS;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class LisaFs : FilesystemTest
    {
        public LisaFs() : base("LisaFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                           "Apple Lisa filesystem");
        public override IFilesystem _plugin     => new LisaFS();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "166files.dc42.lz", "222files.dc42.lz", "blank2.0.dc42.lz", "blank-disk.dc42.lz",
            "file-with-a-password.dc42.lz", "tfwdndrc-has-been-erased.dc42.lz", "tfwdndrc-has-been-restored.dc42.lz",
            "three-empty-folders.dc42.lz", "three-folders-with-differently-named-docs.dc42.lz",
            "three-folders-with-differently-named-docs-root-alphabetical.dc42.lz",
            "three-folders-with-differently-named-docs-root-chronological.dc42.lz",
            "three-folders-with-identically-named-docs.dc42.lz", "lisafs1.dc42.lz", "lisafs2.dc42.lz",
            "lisafs3.dc42.lz", "lisafs3_with_desktop.dc42.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleFileWare, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS
        };

        public override ulong[] _sectors => new ulong[]
        {
            800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1702, 800, 800, 800
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false
        };

        public override long[] _clusters => new long[]
        {
            800, 800, 792, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1684, 792, 800, 800
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "166Files", "222Files", "AOS  4:59 pm 10/02/87", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0",
            "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 4:15 pm 5/06/1983", "Office System 1 2.0",
            "Office System 1 3.0", "AOS 3.0"
        };

        public override string[] _volumeSerial => new[]
        {
            "A23703A202010663", "A23703A201010663", "A32D261301010663", "A22CB48D01010663", "A22CC3A702010663",
            "A22CB48D14010663", "A22CB48D14010663", "A22CB48D01010663", "A22CB48D01010663", "A22CB48D01010663",
            "A22CB48D01010663", "A22CB48D01010663", "9924151E190001E1", "9497F10016010D10", "9CF9CF89070100A8",
            "A4FE1A191F011652"
        };
    }
}