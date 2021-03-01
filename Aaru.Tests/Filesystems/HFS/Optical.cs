// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFS.cs
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

namespace Aaru.Tests.Filesystems.HFS
{
    [TestFixture]
    public class Optical : FilesystemTest
    {
        public Optical() : base("HFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS (CD-ROM)");
        public override IFilesystem _plugin => new AppleHFS();
        public override bool _partitions => true;

        public override string[] _testFiles => new[]
        {
            "toast_3.5.7_hfs_from_volume.aif", "toast_3.5.7_iso9660_hfs.aif", "toast_4.1.3_hfs_from_volume.aif",
            "toast_4.1.3_iso9660_hfs.aif", "toast_3.5.7_hfs_from_files.aif", "toast_4.1.3_hfs_from_files.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD
        };

        public override ulong[] _sectors => new ulong[]
        {
            942, 1880, 943, 1882, 1509, 1529
        };

        public override uint[] _sectorSize => new uint[]
        {
            2048, 2048, 2048, 2048, 2048, 2048
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            3724, 931, 931, 931, 249, 249
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 2048, 2048, 2048, 12288, 12288
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Disk utils", "Disk utils", "Disk utils", "Disk utils", "Disk utils", "Disk utils"
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null, null
        };
    }
}