// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : partclone.cs
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
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Partclone : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "ext2.partclone.lz", "fat16.partclone.lz", "fat32.partclone.lz", "hfsplus.partclone.lz", "ntfs.partclone.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // ext2.partclone.lz
            127882,

            // fat16.partclone.lz
            1012032,

            // fat32.partclone.lz
            1023057,

            // hfsplus.partclone.lz
            127882,

            // ntfs.partclone.lz
            1023056
        };

        public override uint[] _sectorSize => new uint[]
        {
            // ext2.partclone.lz
            4096,

            // fat16.partclone.lz
            512,

            // fat32.partclone.lz
            512,

            // hfsplus.partclone.lz
            4096,

            // ntfs.partclone.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // ext2.partclone.lz
            MediaType.GENERIC_HDD,

            // fat16.partclone.lz
            MediaType.GENERIC_HDD,

            // fat32.partclone.lz
            MediaType.GENERIC_HDD,

            // hfsplus.partclone.lz
            MediaType.GENERIC_HDD,

            // ntfs.partclone.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // ext2.partclone.lz
            "ff239c91166b6b13fa826dd258b40666",

            // fat16.partclone.lz
            "f98b1a51ca2e7bf047d84969a2392a3d",

            // fat32.partclone.lz
            "1b0b5eb965a401f16fa8a07e303cd1c0",

            // hfsplus.partclone.lz
            "880a6777d05c496901e930684abbecff",

            // ntfs.partclone.lz
            "61cc3faa286364e7ad5bab18120c1151"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "partclone");
        public override IMediaImage _plugin => new PartClone();
    }
}