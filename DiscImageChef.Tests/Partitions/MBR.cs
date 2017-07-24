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
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 100800,
                    Sequence = 0, Start = 1008 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 99792,
                    Sequence = 1, Start = 102816 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 100800,
                    Sequence = 2, Start = 202608 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 303408,
                    Sequence = 3, Start = 352800 },
            },
            // Darwin 1.4.1
            new []{
                new Partition{ Description = null, Name = null, Type = "0x07", Length = 409248,
                    Sequence = 0, Start = 409248 },
                new Partition{ Description = null, Name = null, Type = "0xA8", Length = 204624,
                    Sequence = 1, Start = 818496 },
            },
            // Darwin 6.0.2
            new []{
                new Partition{ Description = null, Name = null, Type = "0xA8", Length = 204561,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0xAB", Length = 81648,
                    Sequence = 1, Start = 204624 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245952,
                    Sequence = 2, Start = 286272 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 488880,
                    Sequence = 3, Start = 532224 },
            },
            // Darwin 8.0.1
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 150000,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 176000,
                    Sequence = 1, Start = 150063 },
                new Partition{ Description = null, Name = null, Type = "0xA8", Length = 350000,
                    Sequence = 2, Start = 326063 },
                new Partition{ Description = null, Name = null, Type = "0x0C", Length = 347937,
                    Sequence = 3, Start = 676063 },
            },
            // DR-DOS 3.40
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 100800,
                    Sequence = 0, Start = 1008 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 402129,
                    Sequence = 1, Start = 101871 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 152145,
                    Sequence = 2, Start = 504063 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 365841,
                    Sequence = 3, Start = 656271 },
            },
            // DR-DOS 3.41
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 126945,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 124929,
                    Sequence = 1, Start = 127071 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 101745,
                    Sequence = 2, Start = 252063 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 668241,
                    Sequence = 3, Start = 353871 },
            },
            // DR-DOS 5.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 128016,
                    Sequence = 0, Start = 124992 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 99729,
                    Sequence = 1, Start = 253071 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 100737,
                    Sequence = 2, Start = 352863 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 313425,
                    Sequence = 3, Start = 453663 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 254961,
                    Sequence = 4, Start = 767151 },
            },
            // DR-DOS 6.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 101745,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 18081,
                    Sequence = 1, Start = 102879 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 130977,
                    Sequence = 2, Start = 121023 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 202545,
                    Sequence = 3, Start = 252063 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 567441,
                    Sequence = 4, Start = 454671 },
            },
            // DR-DOS 7.02
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 102753,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 307377,
                    Sequence = 1, Start = 102879 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 384993,
                    Sequence = 2, Start = 410319 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 17073,
                    Sequence = 3, Start = 795375 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 209601,
                    Sequence = 4, Start = 812511 },
            },
            // DR-DOS 7.03
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 202545,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 141057,
                    Sequence = 1, Start = 202671 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 152145,
                    Sequence = 2, Start = 352863 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 364833,
                    Sequence = 3, Start = 505071 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 152145,
                    Sequence = 4, Start = 869967 },
            },
            // DR-DOS 8.0
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 138033,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 303345,
                    Sequence = 2, Start = 352863 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 249921,
                    Sequence = 3, Start = 656271 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 115857,
                    Sequence = 4, Start = 906255 },
            },
            // Linux
            new []{
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 20480,
                    Sequence = 0, Start = 2048 },
                new Partition{ Description = null, Name = null, Type = "0x24", Length = 40960,
                    Sequence = 1, Start = 22528 },
                new Partition{ Description = null, Name = null, Type = "0xA7", Length = 61440,
                    Sequence = 2, Start = 65536 },
                new Partition{ Description = null, Name = null, Type = "0x42", Length = 81920,
                    Sequence = 3, Start = 129024 },
                new Partition{ Description = null, Name = null, Type = "0x83", Length = 49152,
                    Sequence = 4, Start = 212992 },
            },
            // Mac OS X 10.3
            new []{
                new Partition{ Description = null, Name = null, Type = "0xA8", Length = 204800,
                    Sequence = 0, Start = 8 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 102400,
                    Sequence = 1, Start = 204816 },
                new Partition{ Description = null, Name = null, Type = "0x0B", Length = 102400,
                    Sequence = 2, Start = 307224 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 204800,
                    Sequence = 3, Start = 409632 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 204800,
                    Sequence = 4, Start = 614440 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 204752,
                    Sequence = 5, Start = 819248 },
            },
            // Mac OS X 10.4
            new []{
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 102400,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 204800,
                    Sequence = 1, Start = 102501 },
                new Partition{ Description = null, Name = null, Type = "0x0B", Length = 204800,
                    Sequence = 2, Start = 307314 },
                new Partition{ Description = null, Name = null, Type = "0xA8", Length = 204800,
                    Sequence = 3, Start = 512127 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 102400,
                    Sequence = 4, Start = 716940 },
                new Partition{ Description = null, Name = null, Type = "0xAF", Length = 204622,
                    Sequence = 5, Start = 819378 },
            },
            // MS-DOS 3.30A
            new []{
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 1, Start = 65583 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 2, Start = 131103 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 3, Start = 196623 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 4, Start = 262143 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 5, Start = 327663 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 6, Start = 393183 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 7, Start = 458703 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 8, Start = 524223 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 9, Start = 589743 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 10, Start = 655263 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 11, Start = 720783 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 12, Start = 786303 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 13, Start = 851823 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 14, Start = 917343 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 39249,
                    Sequence = 15, Start = 982863 },
            },
            // MS-DOS 5.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 102753,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 31185,
                    Sequence = 1, Start = 102879 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 41265,
                    Sequence = 2, Start = 134127 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 51345,
                    Sequence = 3, Start = 175455 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 61425,
                    Sequence = 4, Start = 226863 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 72513,
                    Sequence = 5, Start = 288351 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 82593,
                    Sequence = 6, Start = 360927 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 92673,
                    Sequence = 7, Start = 443583 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 102753,
                    Sequence = 8, Start = 536319 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 112833,
                    Sequence = 9, Start = 639135 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 122913,
                    Sequence = 10, Start = 752031 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 134001,
                    Sequence = 11, Start = 875007 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 13041,
                    Sequence = 12, Start = 1009071 },
            },
            // MS-DOS 6.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 51345,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 72513,
                    Sequence = 1, Start = 51471 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 92673,
                    Sequence = 2, Start = 124047 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 112833,
                    Sequence = 3, Start = 216783 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 134001,
                    Sequence = 4, Start = 329679 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 154161,
                    Sequence = 5, Start = 463743 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 178353,
                    Sequence = 6, Start = 617967 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 184401,
                    Sequence = 7, Start = 796383 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 41265,
                    Sequence = 8, Start = 980847 },
            },
            // MS-DOS 6.20
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 225729,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 2, Start = 431487 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 267057,
                    Sequence = 3, Start = 677439 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 61425,
                    Sequence = 4, Start = 944559 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 16065,
                    Sequence = 5, Start = 1006047 },
            },
            // MS-DOS 6.21
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 225729,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 2, Start = 431487 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 267057,
                    Sequence = 3, Start = 677439 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 51345,
                    Sequence = 4, Start = 944559 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 6993,
                    Sequence = 5, Start = 995967 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 19089,
                    Sequence = 6, Start = 1003023 },
            },
            // MS-DOS 6.22
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 1, Start = 246015 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 307377,
                    Sequence = 2, Start = 451647 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 225729,
                    Sequence = 3, Start = 759087 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 37233,
                    Sequence = 4, Start = 984879 },
            },
            // Multiuser DOS 7.22 release 04
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 152145,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 99729,
                    Sequence = 1, Start = 152271 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 202545,
                    Sequence = 2, Start = 252063 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 1953,
                    Sequence = 3, Start = 454671 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 565425,
                    Sequence = 4, Start = 456687 },
            },
            // Novell DOS 7.0
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 252945,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 4977,
                    Sequence = 1, Start = 253071 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 202545,
                    Sequence = 2, Start = 352863 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 348705,
                    Sequence = 3, Start = 555471 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 117873,
                    Sequence = 4, Start = 904239 },
            },
            // OpenDOS 7.01
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 307377,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 4977,
                    Sequence = 1, Start = 307503 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 40257,
                    Sequence = 2, Start = 312543 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 202545,
                    Sequence = 3, Start = 352863 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 466641,
                    Sequence = 4, Start = 555471 },
            },
            // Parted
            new []{
                new Partition{ Description = null, Name = null, Type = "0x83", Length = 67584,
                    Sequence = 0, Start = 4096 },
                new Partition{ Description = null, Name = null, Type = "0x07", Length = 59392,
                    Sequence = 1, Start = 73728 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 129024,
                    Sequence = 2, Start = 133120 },
            },
            // PC-DOS 2000
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 225729,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 2, Start = 431487 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 287217,
                    Sequence = 3, Start = 677439 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 57393,
                    Sequence = 4, Start = 964719 },
            },
            // PC-DOS 2.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 1022111,
                    Sequence = 0, Start = 1 },
            },
            // PC-DOS 2.10
            new []{
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 1022111,
                    Sequence = 0, Start = 1 },
            },
            // PC-DOS 3.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 66465,
                    Sequence = 0, Start = 63 },
            },
            // PC-DOS 3.10
            new []{
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 66465,
                    Sequence = 0, Start = 63 },
            },
            // PC-DOS 3.30
            new []{
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 1, Start = 65583 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 2, Start = 131103 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 3, Start = 196623 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 4, Start = 262143 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 5, Start = 327663 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 6, Start = 393183 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 7, Start = 458703 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 8, Start = 524223 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 9, Start = 589743 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 10, Start = 655263 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 11, Start = 720783 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 12, Start = 786303 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 13, Start = 851823 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 65457,
                    Sequence = 14, Start = 917343 },
                new Partition{ Description = null, Name = null, Type = "0x04", Length = 39249,
                    Sequence = 15, Start = 982863 },
            },
            // PC-DOS 4.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 25137,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 2, Start = 230895 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 307377,
                    Sequence = 3, Start = 476847 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 237825,
                    Sequence = 4, Start = 784287 },
            },
            // PC-DOS 5.00
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 25137,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 2, Start = 230895 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 287217,
                    Sequence = 3, Start = 476847 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 257985,
                    Sequence = 4, Start = 764127 },
            },
            // PC-DOS 6.10
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 25137,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 225729,
                    Sequence = 2, Start = 230895 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 3, Start = 456687 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 319473,
                    Sequence = 4, Start = 702639 },
            },
            // Windows 95
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 205569,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 1, Start = 205695 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 267057,
                    Sequence = 2, Start = 451647 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 287217,
                    Sequence = 3, Start = 718767 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 17073,
                    Sequence = 4, Start = 1006047 },
            },
            // Windows 95 OSR 2.5
            new []{
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 307377,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 245889,
                    Sequence = 1, Start = 307503 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 328545,
                    Sequence = 2, Start = 553455 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 102753,
                    Sequence = 3, Start = 882063 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 21105,
                    Sequence = 4, Start = 984879 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 17073,
                    Sequence = 5, Start = 1006047 },
            },
            // Windows NT 3.10
            new []{
                new Partition{ Description = null, Name = null, Type = "0x07", Length = 204561,
                    Sequence = 0, Start = 63 },
                new Partition{ Description = null, Name = null, Type = "0x07", Length = 307377,
                    Sequence = 1, Start = 204687 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 224721,
                    Sequence = 2, Start = 512127 },
                new Partition{ Description = null, Name = null, Type = "0x06", Length = 214641,
                    Sequence = 3, Start = 736911 },
                new Partition{ Description = null, Name = null, Type = "0x01", Length = 10017,
                    Sequence = 4, Start = 951615 },
                new Partition{ Description = null, Name = null, Type = "0x07", Length = 60480,
                    Sequence = 5, Start = 962640 },
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
                    Assert.AreEqual(wanted[i][j].Length * 512, partitions[j].Size, testfiles[i]);
//                    Assert.AreEqual(wanted[i][j].Name, partitions[j].Name, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Type, partitions[j].Type, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start * 512, partitions[j].Offset, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length, partitions[j].Length, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Sequence, partitions[j].Sequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start, partitions[j].Start, testfiles[i]);
                }
            }
        }
    }
}
