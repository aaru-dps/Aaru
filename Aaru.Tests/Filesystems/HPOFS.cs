// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
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
    public class Hpofs : FilesystemTest
    {
        public Hpofs() : base("HPOFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                           "High Performance Optical File System");
        public override IFilesystem _plugin     => new HPOFS();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "rid1.img.lz", "rid10.img.lz", "rid66percent.img.lz", "rid266.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD
        };

        public override ulong[] _sectors => new ulong[]
        {
            2880, 2880, 2880, 2880
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
            2880, 2880, 2880, 2880
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 512, 512, 512
        };
        public override string[] _oemId => new[]
        {
            "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new[]
        {
            "VOLUME LABEL", "VOLUME LABEL", "VOLUME LABEL", "VOLUME LABEL"
        };

        public override string[] _volumeSerial => new[]
        {
            "AC226814", "AC160814", "AC306C14", "ABEF2C14"
        };
    }
}