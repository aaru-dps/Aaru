// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HAMMER.cs
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

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Hammer : FilesystemTest
    {
        public Hammer() : base("HAMMER") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "HAMMER (MBR)");
        public override IFilesystem _plugin     => new HAMMER();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "dflybsd_3.6.1.vdi.lz", "dflybsd_4.0.5.vdi.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            104857600, 104857600
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false
        };

        public override long[] _clusters => new long[]
        {
            6310, 6310
        };

        public override uint[] _clusterSize => new uint[]
        {
            8388608, 8388608
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Volume label", "Volume label"
        };

        public override string[] _volumeSerial => new[]
        {
            "f8e1a8bb-626d-11e7-94b5-0900274691e4", "ff4dc664-6276-11e7-983f-090027c41b46"
        };
    }
}