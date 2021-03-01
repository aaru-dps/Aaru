// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
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

namespace Aaru.Tests.Filesystems.UFS
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "UNIX filesystem (MBR)");
        public override IFilesystem _plugin     => new FFSPlugin();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "ufs1/linux.aif", "ufs2/linux.aif", "ffs43/darwin_1.3.1.aif", "ffs43/darwin_1.4.1.aif",
            "ffs43/darwin_6.0.2.aif", "ffs43/darwin_8.0.1.aif", "ffs43/dflybsd_1.2.0.aif", "ffs43/dflybsd_3.6.1.aif",
            "ffs43/dflybsd_4.0.5.aif", "ffs43/netbsd_1.6.aif", "ffs43/netbsd_7.1.aif", "ufs1/darwin_1.3.1.aif",
            "ufs1/darwin_1.4.1.aif", "ufs1/darwin_6.0.2.aif", "ufs1/darwin_8.0.1.aif", "ufs1/dflybsd_1.2.0.aif",
            "ufs1/dflybsd_3.6.1.aif", "ufs1/dflybsd_4.0.5.aif", "ufs1/freebsd_6.1.aif", "ufs1/freebsd_7.0.aif",
            "ufs1/freebsd_8.2.aif", "ufs1/netbsd_1.6.aif", "ufs1/netbsd_7.1.aif", "ufs1/solaris_7.aif",
            "ufs1/solaris_9.aif", "ufs2/freebsd_6.1.aif", "ufs2/freebsd_7.0.aif", "ufs2/freebsd_8.2.aif",
            "ufs2/netbsd_7.1.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            262144, 262144, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 409600, 204800,
            204800, 204800, 204800, 2097152, 2097152, 2097152, 2097152, 8388608, 8388608, 2097152, 1024000, 2097152,
            2097152, 16777216, 16777216, 16777216, 2097152
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false, false, false, false, false, false, false, false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            65024, 65024, 511024, 511024, 511024, 511488, 511950, 255470, 255470, 511992, 204768, 102280, 102280,
            102280, 102368, 1048500, 523758, 523758, 262138, 1048231, 2096462, 524284, 511968, 1038240, 1046808,
            2096472, 2096472, 4192945, 524272
        };

        public override uint[] _clusterSize => new uint[]
        {
            2048, 2048, 1024, 1024, 1024, 1024, 1024, 2048, 2048, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 2048, 2048,
            4096, 4096, 2048, 2048, 1024, 1024, 1024, 4096, 4096, 2048, 2048
        };
        public override string[] _oemId => null;

        public override string[] _type => new[]
        {
            "UFS", "UFS2", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS2", "UFS2", "UFS2", "UFS2"
        };

        public override string[] _volumeName => new[]
        {
            null, "VolumeLabel", null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, "VolumeLabel", "VolumeLabel", "VolumeLabel", ""
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null
        };
    }
}