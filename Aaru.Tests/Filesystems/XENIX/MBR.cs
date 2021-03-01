// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XENIX.cs
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

namespace Aaru.Tests.Filesystems.XENIX
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("XENIX fs") {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "XENIX filesystem (MBR)");
        public override IFilesystem _plugin     => new SysVfs();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "xenix_2.3.2d.aif", "xenix_2.3.4h.aif", "scoopenserver_5.0.7hw.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            40960, 40960, 2097152
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false
        };

        public override long[] _clusters => new long[]
        {
            0, 0, 0, 19624, 19624, 19624
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024
        };
        public override string[] _oemId => null;

        public override string[] _type => null;

        public override string[] _volumeName => new[]
        {
            "", "", ""
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null
        };
    }
}