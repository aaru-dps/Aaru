// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleMap.cs
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
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Partitions
{
    [TestFixture]
    public class AppleMap
    {
        readonly string[] testfiles = {
            "d2_driver.vdi.lz","hdt_1.8_encrypted1.vdi.lz","hdt_1.8_encrypted2.vdi.lz","hdt_1.8_password.vdi.lz",
            "hdt_1.8.vdi.lz","linux.vdi.lz","macos_1.1.vdi.lz","macos_2.0.vdi.lz",
            "macos_4.2.vdi.lz","macos_4.3.vdi.lz","macos_6.0.2.vdi.lz","macos_6.0.3.vdi.lz",
            "macos_6.0.4.vdi.lz","macos_6.0.5.vdi.lz","macos_6.0.7.vdi.lz","macos_6.0.8.vdi.lz",
            "macos_6.0.vdi.lz","macos_7.0.vdi.lz","macos_7.1.1.vdi.lz","macos_7.5.vdi.lz",
            "parted.vdi.lz","silverlining_2.2.1.vdi.lz","speedtools_3.6.vdi.lz","vcpformatter_2.1.1.vdi.lz",
        };

        readonly Partition[][] wanted = {
            // D2
            new []{ 
                new Partition{ PartitionDescription = null, PartitionLength = 1024, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 2,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 42496, PartitionName = "Macintosh", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 83,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 55808, PartitionName = "Empty", PartitionType = "Apple_Free", PartitionStart = 75264, PartitionSectors = 109,
                    PartitionSequence = 2, PartitionStartSector = 147 },
                new Partition{ PartitionDescription = null, PartitionLength = 26083328, PartitionName = "Volume label", PartitionType = "Apple_HFS", PartitionStart = 131072, PartitionSectors = 50944,
                    PartitionSequence = 3, PartitionStartSector = 256 },
            },
            // HDT 1.8 Encryption Level 1
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 7168, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 14,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 524288, PartitionName = "FWB Disk Driver", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 1024,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 25657344, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 557056, PartitionSectors = 50112,
                    PartitionSequence = 2, PartitionStartSector = 1088 },
            },
            // HDT 1.8 Encryption Level 2
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 7168, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 14,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 524288, PartitionName = "FWB Disk Driver", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 1024,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 25657344, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 557056, PartitionSectors = 50112,
                    PartitionSequence = 2, PartitionStartSector = 1088 },
            },
            // HDT 1.8 with password
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 7168, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 14,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 524288, PartitionName = "FWB Disk Driver", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 1024,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 25657344, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 557056, PartitionSectors = 50112,
                    PartitionSequence = 2, PartitionStartSector = 1088 },
            },
            // HDT 1.8
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 7168, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 14,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 524288, PartitionName = "FWB Disk Driver", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 1024,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 25657344, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 557056, PartitionSectors = 50112,
                    PartitionSequence = 2, PartitionStartSector = 1088 },
            },
            // Linux
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 512, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 32768, PartitionSectors = 1,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 819200, PartitionName = "bootstrap", PartitionType = "Apple_Bootstrap", PartitionStart = 33280, PartitionSectors = 1600,
                    PartitionSequence = 1, PartitionStartSector = 65 },
                new Partition{ PartitionDescription = null, PartitionLength = 512, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 852480, PartitionSectors = 1,
                    PartitionSequence = 2, PartitionStartSector = 1665 },
                new Partition{ PartitionDescription = null, PartitionLength = 52428800, PartitionName = "Linux", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 852992, PartitionSectors = 102400,
                    PartitionSequence = 3, PartitionStartSector = 1666 },
                new Partition{ PartitionDescription = null, PartitionLength = 20971520, PartitionName = "ProDOS", PartitionType = "Apple_PRODOS", PartitionStart = 53281792, PartitionSectors = 40960,
                    PartitionSequence = 4, PartitionStartSector = 104066 },
                new Partition{ PartitionDescription = null, PartitionLength = 52428800, PartitionName = "Macintosh", PartitionType = "Apple_HFS", PartitionStart = 74253312, PartitionSectors = 102400,
                    PartitionSequence = 5, PartitionStartSector = 145026 },
                new Partition{ PartitionDescription = null, PartitionLength = 7535616, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 126682112, PartitionSectors = 14718,
                    PartitionSequence = 6, PartitionStartSector = 247426 },
            },
            // Mac OS 1.1
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 2048, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 4 },
                new Partition{ PartitionDescription = null, PartitionLength = 21403648, PartitionName = "Macintosh", PartitionType = "Apple_HFS", PartitionStart = 8192, PartitionSectors = 41804,
                    PartitionSequence = 1, PartitionStartSector = 16 },
            },
            // Mac OS 2.0
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 2048, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 4 },
                new Partition{ PartitionDescription = null, PartitionLength = 19950080, PartitionName = "Macintosh", PartitionType = "Apple_HFS", PartitionStart = 8192, PartitionSectors = 38965,
                    PartitionSequence = 1, PartitionStartSector = 16 },
            },
            // Mac OS 4.2
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5632, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 2048, PartitionSectors = 11,
                    PartitionSequence = 0, PartitionStartSector = 4 },
                new Partition{ PartitionDescription = null, PartitionLength = 19950080, PartitionName = "Macintosh", PartitionType = "Apple_HFS", PartitionStart = 8192, PartitionSectors = 38965,
                    PartitionSequence = 1, PartitionStartSector = 16 },
            },
            // Mac OS 4.3
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5632, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 2048, PartitionSectors = 11,
                    PartitionSequence = 0, PartitionStartSector = 4 },
                new Partition{ PartitionDescription = null, PartitionLength = 19950080, PartitionName = "Macintosh", PartitionType = "Apple_HFS", PartitionStart = 8192, PartitionSectors = 38965,
                    PartitionSequence = 1, PartitionStartSector = 16 },
            },
            // Mac OS 6.0.2
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 3203072, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 6256,
                    PartitionSequence = 2, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 524288, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 3252224, PartitionSectors = 1024,
                    PartitionSequence = 3, PartitionStartSector = 6352 },
                new Partition{ PartitionDescription = null, PartitionLength = 1048576, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 3776512, PartitionSectors = 2048,
                    PartitionSequence = 4, PartitionStartSector = 7376 },
                new Partition{ PartitionDescription = null, PartitionLength = 2191360, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 4825088, PartitionSectors = 4280,
                    PartitionSequence = 5, PartitionStartSector = 9424 },
                new Partition{ PartitionDescription = null, PartitionLength = 1217024, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 7016448, PartitionSectors = 2377,
                    PartitionSequence = 6, PartitionStartSector = 13704 },
                new Partition{ PartitionDescription = null, PartitionLength = 1572864, PartitionName = "Eschatology 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 8233472, PartitionSectors = 3072,
                    PartitionSequence = 7, PartitionStartSector = 16081 },
                new Partition{ PartitionDescription = null, PartitionLength = 1310720, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 9806336, PartitionSectors = 2560,
                    PartitionSequence = 8, PartitionStartSector = 19153 },
                new Partition{ PartitionDescription = null, PartitionLength = 2550272, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 11117056, PartitionSectors = 4981,
                    PartitionSequence = 9, PartitionStartSector = 21713 },
                new Partition{ PartitionDescription = null, PartitionLength = 2048000, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 13667328, PartitionSectors = 4000,
                    PartitionSequence = 10, PartitionStartSector = 26694 },
                new Partition{ PartitionDescription = null, PartitionLength = 1296384, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 15715328, PartitionSectors = 2532,
                    PartitionSequence = 11, PartitionStartSector = 30694 },
                new Partition{ PartitionDescription = null, PartitionLength = 1364992, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 17011712, PartitionSectors = 2666,
                    PartitionSequence = 12, PartitionStartSector = 33226 },
                new Partition{ PartitionDescription = null, PartitionLength = 3986432, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 18376704, PartitionSectors = 7786,
                    PartitionSequence = 13, PartitionStartSector = 35892 },
                new Partition{ PartitionDescription = null, PartitionLength = 5714944, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 22363136, PartitionSectors = 11162,
                    PartitionSequence = 14, PartitionStartSector = 43678 },
            },
            // Mac OS 6.0.3
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 5948928, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 11619,
                    PartitionSequence = 2, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 1029632, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 5998080, PartitionSectors = 2011,
                    PartitionSequence = 3, PartitionStartSector = 11715 },
                new Partition{ PartitionDescription = null, PartitionLength = 2455552, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 7027712, PartitionSectors = 4796,
                    PartitionSequence = 4, PartitionStartSector = 13726 },
                new Partition{ PartitionDescription = null, PartitionLength = 3932160, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 9483264, PartitionSectors = 7680,
                    PartitionSequence = 5, PartitionStartSector = 18522 },
                new Partition{ PartitionDescription = null, PartitionLength = 4194304, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 13415424, PartitionSectors = 8192,
                    PartitionSequence = 6, PartitionStartSector = 26202 },
                new Partition{ PartitionDescription = null, PartitionLength = 587776, PartitionName = "Eschatology 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 17609728, PartitionSectors = 1148,
                    PartitionSequence = 7, PartitionStartSector = 34394 },
                new Partition{ PartitionDescription = null, PartitionLength = 6537216, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 18197504, PartitionSectors = 12768,
                    PartitionSequence = 8, PartitionStartSector = 35542 },
                new Partition{ PartitionDescription = null, PartitionLength = 1766400, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 24734720, PartitionSectors = 3450,
                    PartitionSequence = 9, PartitionStartSector = 48310 },
                new Partition{ PartitionDescription = null, PartitionLength = 1558528, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 26519552, PartitionSectors = 3044,
                    PartitionSequence = 10, PartitionStartSector = 51796 },
                new Partition{ PartitionDescription = null, PartitionLength = 18432, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 26501120, PartitionSectors = 36,
                    PartitionSequence = 11, PartitionStartSector = 51760 },
            },
            // Mac OS 6.0.4
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 3932160, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 7680,
                    PartitionSequence = 2, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 3197440, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 3981312, PartitionSectors = 6245,
                    PartitionSequence = 3, PartitionStartSector = 7776 },
                new Partition{ PartitionDescription = null, PartitionLength = 3197440, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 7178752, PartitionSectors = 6245,
                    PartitionSequence = 4, PartitionStartSector = 14021 },
                new Partition{ PartitionDescription = null, PartitionLength = 2626560, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 10376192, PartitionSectors = 5130,
                    PartitionSequence = 5, PartitionStartSector = 20266 },
                new Partition{ PartitionDescription = null, PartitionLength = 1370112, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 13002752, PartitionSectors = 2676,
                    PartitionSequence = 6, PartitionStartSector = 25396 },
                new Partition{ PartitionDescription = null, PartitionLength = 2944512, PartitionName = "Eschatology 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 14372864, PartitionSectors = 5751,
                    PartitionSequence = 7, PartitionStartSector = 28072 },
                new Partition{ PartitionDescription = null, PartitionLength = 2776576, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 17317376, PartitionSectors = 5423,
                    PartitionSequence = 8, PartitionStartSector = 33823 },
                new Partition{ PartitionDescription = null, PartitionLength = 2892800, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 20093952, PartitionSectors = 5650,
                    PartitionSequence = 9, PartitionStartSector = 39246 },
                new Partition{ PartitionDescription = null, PartitionLength = 3433472, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 22986752, PartitionSectors = 6706,
                    PartitionSequence = 10, PartitionStartSector = 44896 },
                new Partition{ PartitionDescription = null, PartitionLength = 1657856, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 26420224, PartitionSectors = 3238,
                    PartitionSequence = 11, PartitionStartSector = 51602 },
            },
            // Mac OS 6.0.5
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 4096,
                    PartitionSequence = 2, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 342528, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 2146304, PartitionSectors = 669,
                    PartitionSequence = 3, PartitionStartSector = 4192 },
                new Partition{ PartitionDescription = null, PartitionLength = 1417216, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 2488832, PartitionSectors = 2768,
                    PartitionSequence = 4, PartitionStartSector = 4861 },
                new Partition{ PartitionDescription = null, PartitionLength = 1830912, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 3906048, PartitionSectors = 3576,
                    PartitionSequence = 5, PartitionStartSector = 7629 },
                new Partition{ PartitionDescription = null, PartitionLength = 1448960, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 5736960, PartitionSectors = 2830,
                    PartitionSequence = 6, PartitionStartSector = 11205 },
                new Partition{ PartitionDescription = null, PartitionLength = 2687488, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 7185920, PartitionSectors = 5249,
                    PartitionSequence = 7, PartitionStartSector = 14035 },
                new Partition{ PartitionDescription = null, PartitionLength = 2565632, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 9873408, PartitionSectors = 5011,
                    PartitionSequence = 8, PartitionStartSector = 19284 },
                new Partition{ PartitionDescription = null, PartitionLength = 1954816, PartitionName = "Unreserved 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 12439040, PartitionSectors = 3818,
                    PartitionSequence = 9, PartitionStartSector = 24295 },
                new Partition{ PartitionDescription = null, PartitionLength = 3543040, PartitionName = "Unreserved 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 14393856, PartitionSectors = 6920,
                    PartitionSequence = 10, PartitionStartSector = 28113 },
                new Partition{ PartitionDescription = null, PartitionLength = 2565632, PartitionName = "Unreserved 3", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 17936896, PartitionSectors = 5011,
                    PartitionSequence = 11, PartitionStartSector = 35033 },
                new Partition{ PartitionDescription = null, PartitionLength = 2932224, PartitionName = "Unreserved 4", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 20502528, PartitionSectors = 5727,
                    PartitionSequence = 12, PartitionStartSector = 40044 },
                new Partition{ PartitionDescription = null, PartitionLength = 1221632, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 23434752, PartitionSectors = 2386,
                    PartitionSequence = 13, PartitionStartSector = 45771 },
                new Partition{ PartitionDescription = null, PartitionLength = 3421696, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 24656384, PartitionSectors = 6683,
                    PartitionSequence = 14, PartitionStartSector = 48157 },
            },
            // Mac OS 6.0.7
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 14013952, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 27371,
                    PartitionSequence = 1, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 2, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 1492992, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 14063104, PartitionSectors = 2916,
                    PartitionSequence = 3, PartitionStartSector = 27467 },
                new Partition{ PartitionDescription = null, PartitionLength = 919040, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 15556096, PartitionSectors = 1795,
                    PartitionSequence = 4, PartitionStartSector = 30383 },
                new Partition{ PartitionDescription = null, PartitionLength = 1302016, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 16475136, PartitionSectors = 2543,
                    PartitionSequence = 5, PartitionStartSector = 32178 },
                new Partition{ PartitionDescription = null, PartitionLength = 1796608, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 17777152, PartitionSectors = 3509,
                    PartitionSequence = 6, PartitionStartSector = 34721 },
                new Partition{ PartitionDescription = null, PartitionLength = 1943552, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 19573760, PartitionSectors = 3796,
                    PartitionSequence = 7, PartitionStartSector = 38230 },
                new Partition{ PartitionDescription = null, PartitionLength = 2186752, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 21517312, PartitionSectors = 4271,
                    PartitionSequence = 8, PartitionStartSector = 42026 },
                new Partition{ PartitionDescription = null, PartitionLength = 524288, PartitionName = "Unreserved 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 23704064, PartitionSectors = 1024,
                    PartitionSequence = 9, PartitionStartSector = 46297 },
                new Partition{ PartitionDescription = null, PartitionLength = 655360, PartitionName = "Unreserved 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 24228352, PartitionSectors = 1280,
                    PartitionSequence = 10, PartitionStartSector = 47321 },
                new Partition{ PartitionDescription = null, PartitionLength = 798208, PartitionName = "Unreserved 3", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 24883712, PartitionSectors = 1559,
                    PartitionSequence = 11, PartitionStartSector = 48601 },
                new Partition{ PartitionDescription = null, PartitionLength = 2252800, PartitionName = "Unreserved 4", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 25825280, PartitionSectors = 4400,
                    PartitionSequence = 12, PartitionStartSector = 50440 },
                new Partition{ PartitionDescription = null, PartitionLength = 143360, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 25681920, PartitionSectors = 280,
                    PartitionSequence = 13, PartitionStartSector = 50160 },
            },
            // Mac OS 6.0.8
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 4575744, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 8937,
                    PartitionSequence = 2, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 1143808, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 4624896, PartitionSectors = 2234,
                    PartitionSequence = 3, PartitionStartSector = 9033 },
                new Partition{ PartitionDescription = null, PartitionLength = 3020800, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 5768704, PartitionSectors = 5900,
                    PartitionSequence = 4, PartitionStartSector = 11267 },
                new Partition{ PartitionDescription = null, PartitionLength = 2091520, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 25986560, PartitionSectors = 4085,
                    PartitionSequence = 5, PartitionStartSector = 50755 },
                new Partition{ PartitionDescription = null, PartitionLength = 3693056, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 22293504, PartitionSectors = 7213,
                    PartitionSequence = 6, PartitionStartSector = 43542 },
                new Partition{ PartitionDescription = null, PartitionLength = 2308096, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 19985408, PartitionSectors = 4508,
                    PartitionSequence = 7, PartitionStartSector = 39034 },
                new Partition{ PartitionDescription = null, PartitionLength = 2885120, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 17100288, PartitionSectors = 5635,
                    PartitionSequence = 8, PartitionStartSector = 33399 },
                new Partition{ PartitionDescription = null, PartitionLength = 1615872, PartitionName = "Unreserved 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 8789504, PartitionSectors = 3156,
                    PartitionSequence = 9, PartitionStartSector = 17167 },
                new Partition{ PartitionDescription = null, PartitionLength = 1615872, PartitionName = "Unreserved 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 15484416, PartitionSectors = 3156,
                    PartitionSequence = 10, PartitionStartSector = 30243 },
                new Partition{ PartitionDescription = null, PartitionLength = 1384960, PartitionName = "Unreserved 3", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 10405376, PartitionSectors = 2705,
                    PartitionSequence = 11, PartitionStartSector = 20323 },
                new Partition{ PartitionDescription = null, PartitionLength = 952832, PartitionName = "Unreserved 4", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 11790336, PartitionSectors = 1861,
                    PartitionSequence = 12, PartitionStartSector = 23028 },
                new Partition{ PartitionDescription = null, PartitionLength = 1495040, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 13989376, PartitionSectors = 2920,
                    PartitionSequence = 13, PartitionStartSector = 27323 },
                new Partition{ PartitionDescription = null, PartitionLength = 1246208, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 12743168, PartitionSectors = 2434,
                    PartitionSequence = 14, PartitionStartSector = 24889 },
            },
            // Mac OS 6.0
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 4096,
                    PartitionSequence = 2, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 2146304, PartitionSectors = 4096,
                    PartitionSequence = 3, PartitionStartSector = 4192 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 4243456, PartitionSectors = 4096,
                    PartitionSequence = 4, PartitionStartSector = 8288 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 6340608, PartitionSectors = 4096,
                    PartitionSequence = 5, PartitionStartSector = 12384 },
                new Partition{ PartitionDescription = null, PartitionLength = 1048576, PartitionName = "Swap", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 8437760, PartitionSectors = 2048,
                    PartitionSequence = 6, PartitionStartSector = 16480 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "Eschatology 2", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 9486336, PartitionSectors = 4096,
                    PartitionSequence = 7, PartitionStartSector = 18528 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 11583488, PartitionSectors = 4096,
                    PartitionSequence = 8, PartitionStartSector = 22624 },
                new Partition{ PartitionDescription = null, PartitionLength = 2310144, PartitionName = "Usr file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 13680640, PartitionSectors = 4512,
                    PartitionSequence = 9, PartitionStartSector = 26720 },
                new Partition{ PartitionDescription = null, PartitionLength = 5416960, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 15990784, PartitionSectors = 10580,
                    PartitionSequence = 10, PartitionStartSector = 31232 },
                new Partition{ PartitionDescription = null, PartitionLength = 4096, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 21407744, PartitionSectors = 8,
                    PartitionSequence = 11, PartitionStartSector = 41812 },
            },
            // Mac OS 7.0
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 5120, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 10,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 5262336, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 15845888, PartitionSectors = 10278,
                    PartitionSequence = 2, PartitionStartSector = 30949 },
                new Partition{ PartitionDescription = null, PartitionLength = 3073024, PartitionName = "Scratch", PartitionType = "Apple_Scratch", PartitionStart = 49152, PartitionSectors = 6002,
                    PartitionSequence = 3, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 1707520, PartitionName = "Eschatology 1", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 21108224, PartitionSectors = 3335,
                    PartitionSequence = 4, PartitionStartSector = 41227 },
                new Partition{ PartitionDescription = null, PartitionLength = 5262336, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 22815744, PartitionSectors = 10278,
                    PartitionSequence = 5, PartitionStartSector = 44562 },
                new Partition{ PartitionDescription = null, PartitionLength = 2726400, PartitionName = "Root file system", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 3122176, PartitionSectors = 5325,
                    PartitionSequence = 6, PartitionStartSector = 6098 },
                new Partition{ PartitionDescription = null, PartitionLength = 3180544, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 5848576, PartitionSectors = 6212,
                    PartitionSequence = 7, PartitionStartSector = 11423 },
                new Partition{ PartitionDescription = null, PartitionLength = 4203520, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 9029120, PartitionSectors = 8210,
                    PartitionSequence = 8, PartitionStartSector = 17635 },
                new Partition{ PartitionDescription = null, PartitionLength = 2613248, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 13232640, PartitionSectors = 5104,
                    PartitionSequence = 9, PartitionStartSector = 25845 },
            },
            // Mac OS 7.1.1
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 8704, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 17,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 5148160, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 7294464, PartitionSectors = 10055,
                    PartitionSequence = 2, PartitionStartSector = 14247 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "ProDOS", PartitionType = "Apple_PRODOS", PartitionStart = 5197312, PartitionSectors = 4096,
                    PartitionSequence = 3, PartitionStartSector = 10151 },
                new Partition{ PartitionDescription = null, PartitionLength = 3996672, PartitionName = "A/UX Root", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 24081408, PartitionSectors = 7806,
                    PartitionSequence = 4, PartitionStartSector = 47034 },
                new Partition{ PartitionDescription = null, PartitionLength = 1486848, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 49152, PartitionSectors = 2904,
                    PartitionSequence = 5, PartitionStartSector = 96 },
                new Partition{ PartitionDescription = null, PartitionLength = 4406784, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 12442624, PartitionSectors = 8607,
                    PartitionSequence = 6, PartitionStartSector = 24302 },
                new Partition{ PartitionDescription = null, PartitionLength = 2485760, PartitionName = "Random A/UX fs", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 16849408, PartitionSectors = 4855,
                    PartitionSequence = 7, PartitionStartSector = 32909 },
                new Partition{ PartitionDescription = null, PartitionLength = 4746240, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 19335168, PartitionSectors = 9270,
                    PartitionSequence = 8, PartitionStartSector = 37764 },
                new Partition{ PartitionDescription = null, PartitionLength = 2097152, PartitionName = "ProDOS", PartitionType = "Apple_PRODOS", PartitionStart = 1536000, PartitionSectors = 4096,
                    PartitionSequence = 9, PartitionStartSector = 3000 },
                new Partition{ PartitionDescription = null, PartitionLength = 1564160, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 3633152, PartitionSectors = 3055,
                    PartitionSequence = 10, PartitionStartSector = 7096 },
            },
            // Mac OS 7.5
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 9216, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 18,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 28028928, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 49152, PartitionSectors = 54744,
                    PartitionSequence = 2, PartitionStartSector = 96 },
            },
            // GNU Parted
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 47185920, PartitionName = "untitled", PartitionType = "Apple_HFS", PartitionStart = 2097152, PartitionSectors = 92160,
                    PartitionSequence = 0, PartitionStartSector = 4096 },
                new Partition{ PartitionDescription = null, PartitionLength = 84934656, PartitionName = "untitled", PartitionType = "Apple_UNIX_SVR2", PartitionStart = 49283072, PartitionSectors = 165888,
                    PartitionSequence = 1, PartitionStartSector = 96256 },
                new Partition{ PartitionDescription = null, PartitionLength = 2064384, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 32768, PartitionSectors = 4032,
                    PartitionSequence = 2, PartitionStartSector = 64 },
            },
            // Silverlining 2.2.1
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 3072, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 6,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 25088, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 98304, PartitionSectors = 49,
                    PartitionSequence = 1, PartitionStartSector = 192 },
                new Partition{ PartitionDescription = null, PartitionLength = 65536, PartitionName = "Macintosh_SL", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 128,
                    PartitionSequence = 2, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 65536, PartitionName = "Macintosh_SL", PartitionType = "Apple_Driver_ATA", PartitionStart = 98304, PartitionSectors = 128,
                    PartitionSequence = 3, PartitionStartSector = 192 },
                new Partition{ PartitionDescription = null, PartitionLength = 25804800, PartitionName = "Untitled  #1", PartitionType = "Apple_HFS", PartitionStart = 163840, PartitionSectors = 50400,
                    PartitionSequence = 4, PartitionStartSector = 320 },
                new Partition{ PartitionDescription = null, PartitionLength = 237568, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 25968640, PartitionSectors = 464,
                    PartitionSequence = 5, PartitionStartSector = 50720 },
            },
            // Hard Disk Speed Tools 3.6
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 13824, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 32768, PartitionSectors = 27,
                    PartitionSequence = 0, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 51200, PartitionName = "Macintosh", PartitionType = "Apple_Driver43", PartitionStart = 32768, PartitionSectors = 100,
                    PartitionSequence = 1, PartitionStartSector = 64 },
                new Partition{ PartitionDescription = null, PartitionLength = 25165824, PartitionName = "untitled", PartitionType = "Apple_HFS", PartitionStart = 83968, PartitionSectors = 49152,
                    PartitionSequence = 2, PartitionStartSector = 164 },
                new Partition{ PartitionDescription = null, PartitionLength = 963584, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 25249792, PartitionSectors = 1882,
                    PartitionSequence = 3, PartitionStartSector = 49316 },
            },
            // VCP Formatter 2.1.1
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 12288, PartitionName = null, PartitionType = "Apple_Driver", PartitionStart = 57344, PartitionSectors = 24,
                    PartitionSequence = 0, PartitionStartSector = 112 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Macintosh", PartitionType = "Apple_Driver", PartitionStart = 57344, PartitionSectors = 32,
                    PartitionSequence = 1, PartitionStartSector = 112 },
                new Partition{ PartitionDescription = null, PartitionLength = 16384, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 73728, PartitionSectors = 32,
                    PartitionSequence = 2, PartitionStartSector = 144 },
                new Partition{ PartitionDescription = null, PartitionLength = 27986944, PartitionName = "MacOS", PartitionType = "Apple_HFS", PartitionStart = 90112, PartitionSectors = 54662,
                    PartitionSequence = 3, PartitionStartSector = 176 },
                // TODO: ADFS tries to read past this partition...
                new Partition{ PartitionDescription = null, PartitionLength = 1024, PartitionName = "Extra", PartitionType = "Apple_Free", PartitionStart = 28077056, PartitionSectors = 2,
                    PartitionSequence = 4, PartitionStartSector = 54838 },
            },
        };  

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "partitions", "apm", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.AppleMap();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);
                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionLength, partitions[j].PartitionLength, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionName, partitions[j].PartitionName, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionType, partitions[j].PartitionType, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionStart, partitions[j].PartitionStart, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSectors, partitions[j].PartitionSectors, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSequence, partitions[j].PartitionSequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionStartSector, partitions[j].PartitionStartSector, testfiles[i]);
                }
            }
        }
    }
}
