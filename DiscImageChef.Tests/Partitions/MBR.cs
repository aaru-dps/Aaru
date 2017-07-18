// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MBR.cs
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
    public class MBR
    {
        readonly string[] testfiles = {
            "concurrentdos_6.0.vdi.lz","darwin_1.4.1.vdi.lz","darwin_6.0.2.vdi.lz","darwin_8.0.1.vdi.lz",
            "drdos_3.40.vdi.lz","drdos_3.41.vdi.lz","drdos_5.00.vdi.lz","drdos_6.00.vdi.lz",
            "drdos_7.02.vdi.lz","drdos_7.03.vdi.lz","drdos_8.0.vdi.lz","linux.vdi.lz",
            "macosx_10.3.vdi.lz","macosx_10.4.vdi.lz","msdos_3.30a.vdi.lz","msdos_5.00.vdi.lz",
            "msdos_6.00.vdi.lz","msdos_6.20.vdi.lz","msdos_6.21.vdi.lz","msdos_6.22.vdi.lz",
            "multiuserdos_7.22r04.vdi.lz","novelldos_7.00.vdi.lz","opendos_7.01.vdi.lz","parted.vdi.lz",
            "pcdos_2000.vdi.lz","pcdos_2.00.vdi.lz","pcdos_2.10.vdi.lz","pcdos_3.00.vdi.lz",
            "pcdos_3.10.vdi.lz","pcdos_3.30.vdi.lz","pcdos_4.00.vdi.lz","pcdos_5.00.vdi.lz",
            "pcdos_6.10.vdi.lz","win95.vdi.lz","win96osr25.vdi.lz","winnt_3.10.vdi.lz",
        };

        readonly Partition[][] wanted = {
            // Concurrent DOS 6.0
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 100800,
                    PartitionSequence = 0, PartitionStartSector = 1008 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 99792,
                    PartitionSequence = 1, PartitionStartSector = 102816 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 100800,
                    PartitionSequence = 2, PartitionStartSector = 202608 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 303408,
                    PartitionSequence = 2, PartitionStartSector = 352800 },
            },
            // Darwin 1.4.1
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x07", PartitionSectors = 409248,
                    PartitionSequence = 0, PartitionStartSector = 409248 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xA8", PartitionSectors = 204624,
                    PartitionSequence = 1, PartitionStartSector = 818496 },
            },
            // Darwin 6.0.2
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xA8", PartitionSectors = 204561,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAB", PartitionSectors = 81648,
                    PartitionSequence = 1, PartitionStartSector = 204624 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245952,
                    PartitionSequence = 2, PartitionStartSector = 286272 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 488880,
                    PartitionSequence = 2, PartitionStartSector = 532224 },
            },
            // Darwin 8.0.1
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 150000,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 176000,
                    PartitionSequence = 1, PartitionStartSector = 150063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xA8", PartitionSectors = 350000,
                    PartitionSequence = 2, PartitionStartSector = 326063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x0C", PartitionSectors = 347937,
                    PartitionSequence = 2, PartitionStartSector = 676063 },
            },
            // DR-DOS 3.40
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 100800,
                    PartitionSequence = 0, PartitionStartSector = 1008 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 402129,
                    PartitionSequence = 1, PartitionStartSector = 101871 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 152145,
                    PartitionSequence = 2, PartitionStartSector = 504063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 365841,
                    PartitionSequence = 2, PartitionStartSector = 656271 },
            },
            // DR-DOS 3.41
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 126945,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 124929,
                    PartitionSequence = 1, PartitionStartSector = 127071 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 101745,
                    PartitionSequence = 2, PartitionStartSector = 252063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 668241,
                    PartitionSequence = 2, PartitionStartSector = 353871 },
            },
            // DR-DOS 5.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 128016,
                    PartitionSequence = 0, PartitionStartSector = 124992 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 99729,
                    PartitionSequence = 1, PartitionStartSector = 253071 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 100737,
                    PartitionSequence = 2, PartitionStartSector = 352863 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 313425,
                    PartitionSequence = 2, PartitionStartSector = 453663 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 254961,
                    PartitionSequence = 2, PartitionStartSector = 767151 },
            },
            // DR-DOS 6.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 101745,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 18081,
                    PartitionSequence = 1, PartitionStartSector = 102879 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 130977,
                    PartitionSequence = 2, PartitionStartSector = 121023 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 202545,
                    PartitionSequence = 2, PartitionStartSector = 252063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 567441,
                    PartitionSequence = 2, PartitionStartSector = 454671 },
            },
            // DR-DOS 7.02
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 102753,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 307377,
                    PartitionSequence = 1, PartitionStartSector = 102879 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 384993,
                    PartitionSequence = 2, PartitionStartSector = 410319 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 17073,
                    PartitionSequence = 2, PartitionStartSector = 795375 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 209601,
                    PartitionSequence = 2, PartitionStartSector = 812511 },
            },
            // DR-DOS 7.03
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 202545,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 141057,
                    PartitionSequence = 1, PartitionStartSector = 202671 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 152145,
                    PartitionSequence = 2, PartitionStartSector = 352863 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 364833,
                    PartitionSequence = 2, PartitionStartSector = 505071 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 152145,
                    PartitionSequence = 2, PartitionStartSector = 869967 },
            },
            // DR-DOS 8.0
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 138033,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 303345,
                    PartitionSequence = 2, PartitionStartSector = 352863 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 249921,
                    PartitionSequence = 2, PartitionStartSector = 656271 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 115857,
                    PartitionSequence = 2, PartitionStartSector = 906255 },
            },
            // Linux
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 20480,
                    PartitionSequence = 0, PartitionStartSector = 2048 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x24", PartitionSectors = 40960,
                    PartitionSequence = 1, PartitionStartSector = 22528 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xA7", PartitionSectors = 61440,
                    PartitionSequence = 2, PartitionStartSector = 65536 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x42", PartitionSectors = 81920,
                    PartitionSequence = 2, PartitionStartSector = 129024 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x83", PartitionSectors = 49152,
                    PartitionSequence = 2, PartitionStartSector = 212992 },
            },
            // Mac OS X 10.3
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xA8", PartitionSectors = 204800,
                    PartitionSequence = 0, PartitionStartSector = 8 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 102400,
                    PartitionSequence = 1, PartitionStartSector = 204816 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x0B", PartitionSectors = 102400,
                    PartitionSequence = 2, PartitionStartSector = 307224 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 204800,
                    PartitionSequence = 0, PartitionStartSector = 409632 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 204800,
                    PartitionSequence = 1, PartitionStartSector = 614440 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 204752,
                    PartitionSequence = 2, PartitionStartSector = 819248 },
            },
            // Mac OS X 10.4
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 102400,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 204800,
                    PartitionSequence = 1, PartitionStartSector = 102501 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x0B", PartitionSectors = 204800,
                    PartitionSequence = 2, PartitionStartSector = 307314 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xA8", PartitionSectors = 204800,
                    PartitionSequence = 0, PartitionStartSector = 512127 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 102400,
                    PartitionSequence = 1, PartitionStartSector = 716940 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0xAF", PartitionSectors = 204624,
                    PartitionSequence = 2, PartitionStartSector = 819378 },
            },
            // MS-DOS 3.30A
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 1, PartitionStartSector = 65583 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 131103 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 196623 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 262143 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 327663 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 393183 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 458703 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 524223 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 589743 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 655263 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 720783 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 786303 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 851823 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 917343 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 39249,
                    PartitionSequence = 2, PartitionStartSector = 982863 },
            },
            // MS-DOS 5.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 102753,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 31185,
                    PartitionSequence = 1, PartitionStartSector = 102879 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 41265,
                    PartitionSequence = 2, PartitionStartSector = 134127 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 51345,
                    PartitionSequence = 2, PartitionStartSector = 175455 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 61425,
                    PartitionSequence = 2, PartitionStartSector = 226863 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 72513,
                    PartitionSequence = 2, PartitionStartSector = 288351 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 82593,
                    PartitionSequence = 2, PartitionStartSector = 360927 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 92673,
                    PartitionSequence = 2, PartitionStartSector = 443583 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 102753,
                    PartitionSequence = 2, PartitionStartSector = 536319 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 112833,
                    PartitionSequence = 2, PartitionStartSector = 639135 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 122913,
                    PartitionSequence = 2, PartitionStartSector = 752031 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 134001,
                    PartitionSequence = 2, PartitionStartSector = 875007 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 13041,
                    PartitionSequence = 2, PartitionStartSector = 1009071 },
            },
            // MS-DOS 6.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 51345,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 72513,
                    PartitionSequence = 1, PartitionStartSector = 51471 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 92673,
                    PartitionSequence = 2, PartitionStartSector = 124047 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 112833,
                    PartitionSequence = 2, PartitionStartSector = 216783 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 134001,
                    PartitionSequence = 2, PartitionStartSector = 329679 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 154161,
                    PartitionSequence = 2, PartitionStartSector = 463743 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 178353,
                    PartitionSequence = 2, PartitionStartSector = 617967 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 184401,
                    PartitionSequence = 2, PartitionStartSector = 796383 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 41265,
                    PartitionSequence = 2, PartitionStartSector = 980847 },
            },
            // MS-DOS 6.20
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 225729,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 2, PartitionStartSector = 431487 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 267057,
                    PartitionSequence = 2, PartitionStartSector = 677439 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 61425,
                    PartitionSequence = 2, PartitionStartSector = 944559 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 16065,
                    PartitionSequence = 2, PartitionStartSector = 1006047 },
            },
            // MS-DOS 6.21
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 225729,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 2, PartitionStartSector = 431487 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 267057,
                    PartitionSequence = 2, PartitionStartSector = 677439 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 51345,
                    PartitionSequence = 2, PartitionStartSector = 944559 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 6993,
                    PartitionSequence = 2, PartitionStartSector = 995967 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 19089,
                    PartitionSequence = 2, PartitionStartSector = 1003023 },
            },
            // MS-DOS 6.22
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 1, PartitionStartSector = 246015 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 307377,
                    PartitionSequence = 2, PartitionStartSector = 451647 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 225729,
                    PartitionSequence = 2, PartitionStartSector = 759087 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 37233,
                    PartitionSequence = 2, PartitionStartSector = 984879 },
            },
            // Multiuser DOS 7.22 release 04
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 152145,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 99729,
                    PartitionSequence = 1, PartitionStartSector = 152271 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 202545,
                    PartitionSequence = 2, PartitionStartSector = 252063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 1953,
                    PartitionSequence = 2, PartitionStartSector = 454671 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 565425,
                    PartitionSequence = 2, PartitionStartSector = 456687 },
            },
            // Novell DOS 7.0
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 252945,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 4977,
                    PartitionSequence = 1, PartitionStartSector = 253071 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 202545,
                    PartitionSequence = 2, PartitionStartSector = 352863 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 348705,
                    PartitionSequence = 2, PartitionStartSector = 555471 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 117873,
                    PartitionSequence = 2, PartitionStartSector = 904239 },
            },
            // OpenDOS 7.01
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 307377,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 4977,
                    PartitionSequence = 1, PartitionStartSector = 307503 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 40257,
                    PartitionSequence = 2, PartitionStartSector = 312543 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 202545,
                    PartitionSequence = 2, PartitionStartSector = 352863 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 466641,
                    PartitionSequence = 2, PartitionStartSector = 555471 },
            },
            // Parted
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x83", PartitionSectors = 67584,
                    PartitionSequence = 0, PartitionStartSector = 4096 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x07", PartitionSectors = 59392,
                    PartitionSequence = 1, PartitionStartSector = 73728 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 129024,
                    PartitionSequence = 2, PartitionStartSector = 133120 },
            },
            // PC-DOS 2000
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 225729,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 2, PartitionStartSector = 431487 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 287217,
                    PartitionSequence = 2, PartitionStartSector = 677439 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 57393,
                    PartitionSequence = 2, PartitionStartSector = 964719 },
            },
            // PC-DOS 2.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 1022111,
                    PartitionSequence = 0, PartitionStartSector = 1 },
            },
            // PC-DOS 2.10
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 1022111,
                    PartitionSequence = 0, PartitionStartSector = 1 },
            },
            // PC-DOS 3.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 66465,
                    PartitionSequence = 0, PartitionStartSector = 63 },
            },
            // PC-DOS 3.10
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 66465,
                    PartitionSequence = 0, PartitionStartSector = 63 },
            },
            // PC-DOS 3.30
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 1, PartitionStartSector = 65583 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 131103 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 196623 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 262143 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 327663 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 393183 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 458703 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 524223 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 589743 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 655263 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 720783 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 786303 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 851823 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 65457,
                    PartitionSequence = 2, PartitionStartSector = 917343 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x04", PartitionSectors = 39249,
                    PartitionSequence = 2, PartitionStartSector = 982863 },
            },
            // PC-DOS 4.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 25137,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 2, PartitionStartSector = 230895 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 307377,
                    PartitionSequence = 2, PartitionStartSector = 476847 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 237825,
                    PartitionSequence = 2, PartitionStartSector = 784287 },
            },
            // PC-DOS 5.00
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 25137,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 2, PartitionStartSector = 230895 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 287217,
                    PartitionSequence = 2, PartitionStartSector = 476847 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 257985,
                    PartitionSequence = 2, PartitionStartSector = 764127 },
            },
            // PC-DOS 6.10
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 25137,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 225729,
                    PartitionSequence = 2, PartitionStartSector = 230895 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 2, PartitionStartSector = 456687 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 319473,
                    PartitionSequence = 2, PartitionStartSector = 702639 },
            },
            // Windows 95
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 205569,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 1, PartitionStartSector = 205695 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 267057,
                    PartitionSequence = 2, PartitionStartSector = 451647 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 287217,
                    PartitionSequence = 2, PartitionStartSector = 718767 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 17073,
                    PartitionSequence = 2, PartitionStartSector = 1006047 },
            },
            // Windows 95 OSR 2.5
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 307377,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 245889,
                    PartitionSequence = 1, PartitionStartSector = 307503 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 328545,
                    PartitionSequence = 2, PartitionStartSector = 553455 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 102753,
                    PartitionSequence = 2, PartitionStartSector = 882063 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 21105,
                    PartitionSequence = 2, PartitionStartSector = 984879 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 17073,
                    PartitionSequence = 2, PartitionStartSector = 1006047 },
            },
            // Windows NT 3.10
            new []{
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x07", PartitionSectors = 204561,
                    PartitionSequence = 0, PartitionStartSector = 63 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x07", PartitionSectors = 60480,
                    PartitionSequence = 1, PartitionStartSector = 962640 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x07", PartitionSectors = 307377,
                    PartitionSequence = 2, PartitionStartSector = 204687 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 224721,
                    PartitionSequence = 2, PartitionStartSector = 512127 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x06", PartitionSectors = 214641,
                    PartitionSequence = 2, PartitionStartSector = 736911 },
                new Partition{ PartitionDescription = null, PartitionName = null, PartitionType = "0x01", PartitionSectors = 10017,
                    PartitionSequence = 2, PartitionStartSector = 951615 },
            },
        };  

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "partitions", "mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);
                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSectors * 512, partitions[j].PartitionLength, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionName, partitions[j].PartitionName, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionType, partitions[j].PartitionType, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionStartSector * 512, partitions[j].PartitionStart, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSectors, partitions[j].PartitionSectors, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSequence, partitions[j].PartitionSequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionStartSector, partitions[j].PartitionStartSector, testfiles[i]);
                }
            }
        }
    }
}
