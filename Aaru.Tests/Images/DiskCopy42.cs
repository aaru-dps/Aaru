// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class DiskCopy42 : BlockMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskCopy 4.2");
        public override IMediaImage _plugin => new DiscImages.DiskCopy42();
        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "hfs.dsk.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "2762f41d0379b476042fc62891baac84"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf1dd_hfs.img.lz",
                MediaType  = MediaType.AppleSonySS,
                Sectors    = 800,
                SectorSize = 512,
                MD5        = "eae3a95671d077deb702b3549a769f56"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf1dd_mfs.img.lz",
                MediaType  = MediaType.AppleSonySS,
                Sectors    = 800,
                SectorSize = 512,
                MD5        = "c5d92544c3e78b7f0a9b4baaa9a64eec"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_hfs.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "a99744348a70b62b57bce2dec9132ced"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_mfs.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "93e71b9ecdb39d3ec9245b4f451856d4"
            },
            new BlockImageTestExpected
            {
                TestFile   = "modified.dsk.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "b748f6df3e60e7169d42ec6fcc857ea4"
            },
            new BlockImageTestExpected
            {
                TestFile   = "pascal800.dsk.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "dbd0ec8a3126236910709faf923adcf2"
            },
            new BlockImageTestExpected
            {
                TestFile   = "prodos1440.dsk.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "fcf747bd356b48d442ff74adb8f3516b"
            },
            new BlockImageTestExpected
            {
                TestFile   = "prodos800.dsk.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "fcf747bd356b48d442ff74adb8f3516b"
            }
        };
    }
}