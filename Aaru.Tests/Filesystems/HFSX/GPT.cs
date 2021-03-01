// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSX.cs
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

namespace Aaru.Tests.Filesystems.HFSX
{
    [TestFixture]
    public class GPT : FilesystemTest
    {
        public GPT() : base("HFSX") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFSX (GPT)");
        public override IFilesystem _plugin => new AppleHFSPlus();
        public override bool _partitions => true;

        public override string[] _testFiles => new[]
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            819200, 1228800
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
            102390, 153590
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096
        };
        public override string[] _type => null;
        public override string[] _oemId => new[]
        {
            "10.0", "HFSJ"
        };

        public override string[] _volumeName => new string[]
        {
            null, null
        };

        public override string[] _volumeSerial => new[]
        {
            "328343989312AE9F", "FB98504073464C5C"
        };
    }
}