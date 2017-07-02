// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT12_MBR.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.IO;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using NUnit.Framework;
using DiscImageChef.DiscImages;
using DiscImageChef.PartPlugins;
using DiscImageChef.CommonTypes;
using System.Collections.Generic;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class FAT12_MBR
    {
        readonly string[] testfiles = {
            "compaqmsdos331.vdi.lz", "drdos_3.40.vdi.lz", "drdos_3.41.vdi.lz", "drdos_5.00.vdi.lz",
            "drdos_6.00.vdi.lz", "drdos_7.02.vdi.lz", "drdos_7.03.vdi.lz", "drdos_8.00.vdi.lz",
            "msdos331.vdi.lz", "msdos401.vdi.lz", "msdos500.vdi.lz", "msdos600.vdi.lz",
            "msdos620rc1.vdi.lz", "msdos620.vdi.lz", "msdos621.vdi.lz", "msdos622.vdi.lz",
            "msdos710.vdi.lz", "novelldos_7.00.vdi.lz", "opendos_7.01.vdi.lz", "pcdos2000.vdi.lz",
            "pcdos200.vdi.lz", "pcdos210.vdi.lz", "pcdos300.vdi.lz", "pcdos310.vdi.lz",
            "pcdos330.vdi.lz", "pcdos400.vdi.lz", "pcdos500.vdi.lz", "pcdos502.vdi.lz",
            "pcdos610.vdi.lz", "pcdos630.vdi.lz", "toshibamsdos330.vdi.lz", "toshibamsdos401.vdi.lz",
            "msos2_1.21.vdi.lz", "msos2_1.30.1.vdi.lz", "multiuserdos_7.22r4.vdi.lz", "os2_1.20.vdi.lz",
            "os2_1.30.vdi.lz", "os2_6.307.vdi.lz", "os2_6.514.vdi.lz", "os2_6.617.vdi.lz",
            "os2_8.162.vdi.lz", "os2_9.023.vdi.lz", "ecs.vdi.lz",
        };

        readonly ulong[] sectors = {
            8192, 30720, 28672, 28672,
            28672, 28672, 28672, 28672,
            8192, 8192, 8192, 8192,
            8192, 8192, 8192, 8192,
            16384, 28672, 28672, 32768,
            32768, 32768, 32768, 32768,
            32768, 32768, 32768, 32768,
            32768, 32768, 8192, 8192,
            16384, 16384, 16384, 16384,
            16384, 16384, 16384, 16384,
            16384, 16384, 16384,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512,
        };

        readonly long[] clusters = {
            1000, 3654, 3520, 3520,
            3520, 3520, 3520, 3520,
            1000, 1000, 1000, 1000,
            1000, 1000, 1000, 1000,
            2008, 3520, 3520, 4024,
            4031, 4031, 4024, 4024,
            4024, 4024, 4024, 4024,
            4024, 4024, 1000, 1000,
            2008, 2008, 2008, 2008,
            2008, 2008, 2008, 2008,
            2008, 2008, 1890,
        };

        readonly int[] clustersize = {
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096,
        };

        readonly string[] volumename = {
            null,null,null,null,
            null,null,null,"VOLUMELABEL",
            null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL",null,null,"VOLUMELABEL",
            null,null,null,null,
            null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL",null,"VOLUMELABEL",
            "NO NAME    ","NO NAME    ",null,"NO NAME    ",
            "NO NAME    ","NO NAME    ","NO NAME    ","NO NAME    ",
            "NO NAME    ","NO NAME    ","NO NAME    ",
        };

        readonly string[] volumeserial = {
            null,null,null,null,
            null,null,null,"1BFB1273",
            null,"407D1907","345D18FB","332518F4",
            "395718E9","076718EF","1371181B","23281816",
            "2F781809",null,null,"294F100F",
            null,null,null,null,
            null,"0F340FE4","1A5E0FF9","1D2F0FFE",
            "076C1004","2C481009",null,"3C2319E8",
            "66CC3C15","66A54C15",null,"5C578015",
            "5B845015","5C4BF015","E6B5F414","E6B15414",
            "E6A41414","E6A39414","E6B0B814",
        };

        readonly string[] oemid = {
            "IBM  3.3", "IBM  3.2", "IBM  3.2", "IBM  3.3",
            "IBM  3.3", "IBM  3.3", "DRDOS  7", "IBM  5.0",
            "IBM  3.3", "MSDOS4.0", "MSDOS5.0", "MSDOS5.0",
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",
            "MSWIN4.1", "IBM  3.3", "IBM  3.3", "IBM  7.0",
            "IBM  2.0", "IBM  2.0", "IBM  3.0", "IBM  3.1",
            "IBM  3.3", "IBM  4.0", "IBM  5.0", "IBM  5.0",
            "IBM  6.0", "IBM  6.0", "T V3.30 ", "T V4.00 ",
            "IBM 10.2", "IBM 10.2", "IBM  3.2", "IBM 10.2",
            "IBM 10.2", "IBM 20.0", "IBM 20.0", "IBM 20.0",
            "IBM 20.0", "IBM 20.0", "IBM 4.50",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "fat12_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new FAT();
                Assert.AreEqual(true, fs.Identify(image, partitions[0].PartitionStartSector, partitions[0].PartitionStartSector + partitions[0].PartitionSectors - 1), testfiles[i]);
                fs.GetInformation(image, partitions[0].PartitionStartSector, partitions[0].PartitionStartSector + partitions[0].PartitionSectors - 1, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT12", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
