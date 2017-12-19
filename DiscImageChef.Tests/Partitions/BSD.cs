// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Partitions
{
    [TestFixture]
    public class BSD
    {
        readonly string[] testfiles = {
            "parted.vdi.lz","netbsd_1.6.vdi.lz","netbsd_6.1.5.vdi.lz","netbsd_7.1.vdi.lz"
        };

        readonly Partition[][] wanted = {
            // Parted
            new []{
                new Partition{ Description = null, Size = 38797312, Name = null, Type = "FAT", Offset = 1048576, Length = 75776,
                    Sequence = 0, Start = 2048 },
                new Partition{ Description = null, Size = 19922944, Name = null, Type = "FAT", Offset = 40894464, Length = 38912,
                    Sequence = 1, Start = 79872 },
                new Partition{ Description = null, Size = 48234496, Name = null, Type = "FAT", Offset = 61865984, Length = 94208,
                    Sequence = 2, Start = 120832 },
            },
            // NetBSD 1.6
            new []{
                new Partition{ Description = null, Size = 10453504, Name = null, Type = "FAT", Offset = 516096, Length = 20417,
                    Sequence = 0, Start = 1008 },
                new Partition{ Description = null, Size = 209715200, Name = null, Type = "FAT", Offset = 11354112, Length = 409600,
                    Sequence = 1, Start = 22176 },
                new Partition{ Description = null, Size = 805306368, Name = null, Type = "FAT", Offset = 221405184, Length = 1572864,
                    Sequence = 2, Start = 432432 },
                new Partition{ Description = null, Size = 747656192, Name = null, Type = "4.2BSD Fast File System", Offset = 1027031040, Length = 1460266,
                    Sequence = 3, Start = 2005920 },
                new Partition{ Description = null, Size = 268435456, Name = null, Type = "4.4LFS", Offset = 1774854144, Length = 524288,
                    Sequence = 4, Start = 3466512 },
                new Partition{ Description = null, Size = 103743488, Name = null, Type = "4.2BSD Fast File System", Offset = 2043740160, Length = 202624,
                    Sequence = 5, Start = 3991680 },
            },
            // NetBSD 6.1.5
            new []{
                new Partition{ Description = null, Size = 10485760, Name = null, Type = "FAT", Offset = 516096, Length = 20480,
                    Sequence = 0, Start = 1008 },
                new Partition{ Description = null, Size = 104857600, Name = null, Type = "FAT", Offset = 11354112, Length = 204800,
                    Sequence = 1, Start = 22176 },
                new Partition{ Description = null, Size = 209715200, Name = null, Type = "FAT", Offset = 116637696, Length = 409600,
                    Sequence = 2, Start = 227808 },
                new Partition{ Description = null, Size = 40771584, Name = null, Type = "4.2BSD Fast File System", Offset = 326688768, Length = 79632,
                    Sequence = 3, Start = 638064 },
                new Partition{ Description = null, Size = 419430400, Name = null, Type = "4.2BSD Fast File System", Offset = 367460352, Length = 819200,
                    Sequence = 4, Start = 717696 },
                new Partition{ Description = null, Size = 471859200, Name = null, Type = "4.2BSD Fast File System", Offset = 787562496, Length = 921600,
                    Sequence = 5, Start = 1538208 },
                // Type conflicts between DragonFly and NetBSD, really is Apple UFS
                new Partition{ Description = null, Size = 78643200, Name = null, Type = "Hammer", Offset = 1259790336, Length = 153600,
                    Sequence = 6, Start = 2460528 },
                new Partition{ Description = null, Size = 99614720, Name = null, Type = "UNIX 7th Edition", Offset = 1338753024, Length = 194560,
                    Sequence = 7, Start = 2614752 },
                new Partition{ Description = null, Size = 244318208, Name = null, Type = "4.4LFS", Offset = 1438875648, Length = 477184,
                    Sequence = 8, Start = 2810304 },
                // Type conflicts, really is Linux ext2
                new Partition{ Description = null, Size = 463978496, Name = null, Type = "Digital LSM Public Region", Offset = 1683505152, Length = 906208,
                    Sequence = 9, Start = 3288096 },
            },
            // NetBSD 7.1
            new []{
                new Partition{ Description = null, Size = 10321920, Name = null, Type = "FAT", Offset = 516096, Length = 20160,
                    Sequence = 0, Start = 1008 },
                new Partition{ Description = null, Size = 104767488, Name = null, Type = "FAT", Offset = 11354112, Length = 204624,
                    Sequence = 1, Start = 22176 },
                new Partition{ Description = null, Size = 209534976, Name = null, Type = "FAT", Offset = 116637696, Length = 409248,
                    Sequence = 2, Start = 227808 },
                new Partition{ Description = null, Size = 40255488, Name = null, Type = "4.2BSD Fast File System", Offset = 326688768, Length = 78624,
                    Sequence = 3, Start = 638064 },
                new Partition{ Description = null, Size = 419069952, Name = null, Type = "4.2BSD Fast File System", Offset = 367460352, Length = 818496,
                    Sequence = 4, Start = 717696 },
                new Partition{ Description = null, Size = 471711744, Name = null, Type = "4.2BSD Fast File System", Offset = 787562496, Length = 921312,
                    Sequence = 5, Start = 1538208 },
                // Type conflicts between DragonFly and NetBSD, really is Apple UFS
                new Partition{ Description = null, Size = 78446592, Name = null, Type = "Hammer", Offset = 1259790336, Length = 153216,
                    Sequence = 6, Start = 2460528 },
                new Partition{ Description = null, Size = 99606528, Name = null, Type = "UNIX 7th Edition", Offset = 1338753024, Length = 194544,
                    Sequence = 7, Start = 2614752 },
                new Partition{ Description = null, Size = 243597312, Name = null, Type = "4.4LFS", Offset = 1438875648, Length = 475776,
                    Sequence = 8, Start = 2810304 },
                // Type conflicts, really is Linux ext2
                new Partition{ Description = null, Size = 463970304, Name = null, Type = "Digital LSM Public Region", Offset = 1683505152, Length = 906192,
                    Sequence = 9, Start = 3288096 },
            },
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "partitions", "bsd", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);
                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Size, partitions[j].Size, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Name, partitions[j].Name, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Type, partitions[j].Type, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Offset, partitions[j].Offset, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length, partitions[j].Length, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Sequence, partitions[j].Sequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start, partitions[j].Start, testfiles[i]);
                }
            }
        }
    }
}
