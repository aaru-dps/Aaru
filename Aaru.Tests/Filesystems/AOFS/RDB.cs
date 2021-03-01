// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AOFS.cs
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
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.AOFS
{
    [TestFixture]
    public class RDB : FilesystemTest
    {
        public RDB() : base("Amiga OFS") {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Old File System (RDB)");
        public override IFilesystem _plugin     => new AmigaDOSPlugin();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "amigaos_3.9.aif", "amigaos_3.9_intl.aif", "aros.aif", "aros_intl.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            1024128, 1024128, 409600, 409600
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            510032, 510032, 407232, 407232
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 512, 512
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Volume label", "Volume label", "Volume label", "Volume label"
        };

        public override string[] _volumeSerial => new[]
        {
            "A56D13BB", "A56D0415", "A582F3A0", "A5830B06"
        };
    }
}