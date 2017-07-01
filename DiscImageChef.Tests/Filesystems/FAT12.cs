// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT12.cs
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
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class FAT12
    {
        readonly string[] testfiles = {
            // Concurrent DOS 6.00
            "concurrentdos_6.00_dshd.img.lz", "concurrentdos_6.00_mf2dd.img.lz", "concurrentdos_6.00_mf2hd.img.lz",
            // DR-DOS 3.40
            "drdos_3.40_dsdd.img.lz", "drdos_3.40_dsdd8.img.lz", "drdos_3.40_dshd.img.lz", "drdos_3.40_mf2dd.img.lz",
            "drdos_3.40_mf2hd.img.lz", "drdos_3.40_ssdd.img.lz", "drdos_3.40_ssdd8.img.lz",
            // DR-DOS 3.41
            "drdos_3.41_dsdd.img.lz", "drdos_3.41_dsdd8.img.lz", "drdos_3.41_dshd.img.lz", "drdos_3.41_mf2dd.img.lz",
            "drdos_3.41_mf2hd.img.lz", "drdos_3.41_ssdd.img.lz", "drdos_3.41_ssdd8.img.lz",
            // DR-DOS 5.00
            "drdos_5.00_dsdd.img.lz", "drdos_5.00_dsdd8.img.lz", "drdos_5.00_dshd.img.lz", "drdos_5.00_mf2dd.img.lz",
            "drdos_5.00_mf2hd.img.lz", "drdos_5.00_ssdd.img.lz", "drdos_5.00_ssdd8.img.lz",
            // DR-DOS 6.00
            "drdos_6.00_dsdd.img.lz", "drdos_6.00_dsdd8.img.lz", "drdos_6.00_dshd.img.lz", "drdos_6.00_mf2dd.img.lz",
            "drdos_6.00_mf2ed.img.lz", "drdos_6.00_mf2hd.img.lz", "drdos_6.00_ssdd.img.lz", "drdos_6.00_ssdd8.img.lz",
            // DR-DOS 7.02
            "drdos_7.02_dsdd.img.lz", "drdos_7.02_dsdd8.img.lz", "drdos_7.02_dshd.img.lz", "drdos_7.02_mf2dd.img.lz",
            "drdos_7.02_mf2ed.img.lz", "drdos_7.02_mf2hd.img.lz", "drdos_7.02_ssdd.img.lz", "drdos_7.02_ssdd8.img.lz",
            // DR-DOS 7.03
            "drdos_7.03_dsdd.img.lz", "drdos_7.03_dsdd8.img.lz", "drdos_7.03_dshd.img.lz", "drdos_7.03_mf2dd.img.lz",
            "drdos_7.03_mf2ed.img.lz", "drdos_7.03_mf2hd.img.lz",
            // DR-DOS 8.00
            "drdos_8.00_dsdd.img.lz", "drdos_8.00_dsdd8.img.lz", "drdos_8.00_dshd.img.lz", "drdos_8.00_mf2dd.img.lz",
            "drdos_8.00_mf2ed.img.lz", "drdos_8.00_mf2hd.img.lz", "drdos_8.00_ssdd.img.lz", "drdos_8.00_ssdd8.img.lz",
            // MS-DOS 3.30A
            "msdos_3.30A_dsdd.img.lz","msdos_3.30A_dsdd8.img.lz","msdos_3.30A_dshd.img.lz","msdos_3.30A_mf2dd.img.lz",
            "msdos_3.30A_mf2ed.img.lz","msdos_3.30A_mf2hd.img.lz","msdos_3.30A_ssdd.img.lz","msdos_3.30A_ssdd8.img.lz",
            // MS-DOS 3.31
            "msdos_3.31_dsdd.img.lz","msdos_3.31_dsdd8.img.lz","msdos_3.31_dshd.img.lz","msdos_3.31_mf2dd.img.lz",
            "msdos_3.31_mf2ed.img.lz","msdos_3.31_mf2hd.img.lz","msdos_3.31_ssdd.img.lz","msdos_3.31_ssdd8.img.lz",
            // MS-DOS 4.01
            "msdos_4.01_dsdd.img.lz","msdos_4.01_dsdd8.img.lz","msdos_4.01_dshd.img.lz","msdos_4.01_mf2dd.img.lz",
            "msdos_4.01_mf2hd.img.lz","msdos_4.01_ssdd.img.lz","msdos_4.01_ssdd8.img.lz",
            // MS-DOS 5.00
            "msdos_5.00_dsdd.img.lz","msdos_5.00_dsdd8.img.lz","msdos_5.00_dshd.img.lz","msdos_5.00_mf2dd.img.lz",
            "msdos_5.00_mf2ed.img.lz","msdos_5.00_mf2hd.img.lz","msdos_5.00_ssdd.img.lz","msdos_5.00_ssdd8.img.lz",
            // MS-DOS 6.00
            "msdos_6.00_dsdd.img.lz","msdos_6.00_dsdd8.img.lz","msdos_6.00_dshd.img.lz","msdos_6.00_mf2dd.img.lz",
            "msdos_6.00_mf2ed.img.lz","msdos_6.00_mf2hd.img.lz","msdos_6.00_ssdd.img.lz","msdos_6.00_ssdd8.img.lz",
            // MS-DOS 6.20
            "msdos_6.20_dsdd.img.lz","msdos_6.20_dsdd8.img.lz","msdos_6.20_dshd.img.lz","msdos_6.20_mf2dd.img.lz",
            "msdos_6.20_mf2ed.img.lz","msdos_6.20_mf2hd.img.lz","msdos_6.20_ssdd.img.lz","msdos_6.20_ssdd8.img.lz",
            // MS-DOS 6.20 RC1
            "msdos_6.20rc1_dsdd.img.lz","msdos_6.20rc1_dsdd8.img.lz","msdos_6.20rc1_dshd.img.lz","msdos_6.20rc1_mf2dd.img.lz",
            "msdos_6.20rc1_mf2ed.img.lz","msdos_6.20rc1_mf2hd.img.lz","msdos_6.20rc1_ssdd.img.lz","msdos_6.20rc1_ssdd8.img.lz",
            // MS-DOS 6.21
            "msdos_6.21_dsdd.img.lz","msdos_6.21_dsdd8.img.lz","msdos_6.21_dshd.img.lz","msdos_6.21_mf2dd.img.lz",
            "msdos_6.21_mf2ed.img.lz","msdos_6.21_mf2hd.img.lz","msdos_6.21_ssdd.img.lz","msdos_6.21_ssdd8.img.lz",
            // MS-DOS 6.22
            "msdos_6.22_dsdd.img.lz","msdos_6.22_dsdd8.img.lz","msdos_6.22_dshd.img.lz","msdos_6.22_mf2dd.img.lz",
            "msdos_6.22_mf2ed.img.lz","msdos_6.22_mf2hd.img.lz","msdos_6.22_ssdd.img.lz","msdos_6.22_ssdd8.img.lz",
            // MS-DOS 7.10
            "msdos_7.10_dsdd.img.lz","msdos_7.10_dsdd8.img.lz","msdos_7.10_dshd.img.lz","msdos_7.10_mf2dd.img.lz",
            "msdos_7.10_mf2ed.img.lz","msdos_7.10_mf2hd.img.lz","msdos_7.10_ssdd.img.lz","msdos_7.10_ssdd8.img.lz",
            // MS-DOS 3.20 for Amstrad
            "msdos_amstrad_3.20_dsdd.img.lz","msdos_amstrad_3.20_dsdd8.img.lz","msdos_amstrad_3.20_dshd.img.lz",
            "msdos_amstrad_3.20_mf2dd.img.lz","msdos_amstrad_3.20_ssdd.img.lz","msdos_amstrad_3.20_ssdd8.img.lz",
            // MS-DOS 2.11 for AT&T
            "msdos_att_2.11_dsdd.img.lz",
            // MS-DOS 3.30 for DeLL
            "msdos_dell_3.30_dsdd.img.lz","msdos_dell_3.30_dsdd8.img.lz","msdos_dell_3.30_dshd.img.lz",
            "msdos_dell_3.30_mf2dd.img.lz","msdos_dell_3.30_mf2hd.img.lz","msdos_dell_3.30_ssdd.img.lz",
            "msdos_dell_3.30_ssdd8.img.lz",
            // MS-DOS 3.10 for Epson
            "msdos_epson_3.10_dsdd.img.lz","msdos_epson_3.10_dsdd8.img.lz","msdos_epson_3.10_dshd.img.lz",
            // MS-DOS 3.20 for Epson
            "msdos_epson_3.20_dsdd.img.lz","msdos_epson_3.20_dsdd8.img.lz","msdos_epson_3.20_dshd.img.lz",
            "msdos_epson_3.20_mf2dd.img.lz","msdos_epson_3.20_ssdd.img.lz","msdos_epson_3.20_ssdd8.img.lz",
            // MS-DOS 3.20 for HP
            "msdos_hp_3.20_dsdd.img.lz","msdos_hp_3.20_dsdd8.img.lz","msdos_hp_3.20_dshd.img.lz",
            "msdos_hp_3.20_mf2dd.img.lz","msdos_hp_3.20_mf2hd.img.lz","msdos_hp_3.20_ssdd.img.lz",
            "msdos_hp_3.20_ssdd8.img.lz",
            // MS-DOS 3.21 for Hyosung
            "msdos_hyonsung_3.21_dsdd.img.lz","msdos_hyonsung_3.21_dsdd8.img.lz","msdos_hyonsung_3.21_dshd.img.lz",
            "msdos_hyonsung_3.21_mf2dd.img.lz","msdos_hyonsung_3.21_mf2hd.img.lz","msdos_hyonsung_3.21_ssdd.img.lz",
            "msdos_hyonsung_3.21_ssdd8.img.lz",
            // MS-DOS 3.21 for Kaypro
            "msdos_kaypro_3.21_dsdd.img.lz","msdos_kaypro_3.21_dsdd8.img.lz","msdos_kaypro_3.21_dshd.img.lz",
            "msdos_kaypro_3.21_mf2dd.img.lz","msdos_kaypro_3.21_mf2hd.img.lz","msdos_kaypro_3.21_ssdd.img.lz",
            //"msdos_kaypro_3.21_ssdd8.img.lz",
            // MS-DOS 3.10 for Olivetti
            "msdos_olivetti_3.10_dsdd.img.lz","msdos_olivetti_3.10_dshd.img.lz","msdos_olivetti_3.10_ssdd.img.lz",
            // MS-DOS 3.30 for Toshiba
            "msdos_toshiba_3.30_dsdd.img.lz","msdos_toshiba_3.30_dsdd8.img.lz","msdos_toshiba_3.30_dshd.img.lz",
            "msdos_toshiba_3.30_mf2dd.img.lz","msdos_toshiba_3.30_mf2hd.img.lz","msdos_toshiba_3.30_ssdd.img.lz",
            "msdos_toshiba_3.30_ssdd8.img.lz",
            // MS-DOS 4.01 for Toshiba
            "msdos_toshiba_4.01_dsdd.img.lz","msdos_toshiba_4.01_dsdd8.img.lz","msdos_toshiba_4.01_dshd.img.lz",
            "msdos_toshiba_4.01_mf2dd.img.lz","msdos_toshiba_4.01_mf2hd.img.lz","msdos_toshiba_4.01_ssdd.img.lz",
            "msdos_toshiba_4.01_ssdd8.img.lz",
            // Novell DOS 7.00
            "novelldos_7.00_dsdd.img.lz","novelldos_7.00_dsdd8.img.lz","novelldos_7.00_dshd.img.lz","novelldos_7.00_mf2dd.img.lz",
            "novelldos_7.00_mf2ed.img.lz","novelldos_7.00_mf2hd.img.lz","novelldos_7.00_ssdd.img.lz","novelldos_7.00_ssdd8.img.lz",
            // OpenDOS 7.01
            "opendos_7.01_dsdd.img.lz","opendos_7.01_dsdd8.img.lz","opendos_7.01_dshd.img.lz","opendos_7.01_mf2dd.img.lz",
            "opendos_7.01_mf2ed.img.lz","opendos_7.01_mf2hd.img.lz","opendos_7.01_ssdd.img.lz","opendos_7.01_ssdd8.img.lz",
            // PC-DOS 2.00
            "pcdos_2.00_dsdd.img.lz",
            // PC-DOS 2.10
            "pcdos_2.10_dsdd.img.lz",
            // PC-DOS 2000
            "pcdos_2000_dsdd.img.lz","pcdos_2000_dsdd8.img.lz","pcdos_2000_dshd.img.lz","pcdos_2000_mf2dd.img.lz",
            "pcdos_2000_mf2ed.img.lz","pcdos_2000_mf2hd.img.lz","pcdos_2000_ssdd.img.lz","pcdos_2000_ssdd8.img.lz",
            // PC-DOS 3.00
            "pcdos_3.00_dshd.img.lz",
            // PC-DOS 3.10
            "pcdos_3.10_dshd.img.lz",
            // PC-DOS 3.30
            "pcdos_3.30_dshd.img.lz","pcdos_3.30_mf2hd.img.lz",
            // PC-DOS 4.00
            "pcdos_4.00_dshd.img.lz","pcdos_4.00_mf2hd.img.lz",
            // PC-DOS 5.00
            "pcdos_5.00_dsdd.img.lz","pcdos_5.00_dsdd8.img.lz","pcdos_5.00_dshd.img.lz","pcdos_5.00_mf2dd.img.lz",
            "pcdos_5.00_mf2ed.img.lz","pcdos_5.00_mf2hd.img.lz","pcdos_5.00_ssdd.img.lz","pcdos_5.00_ssdd8.img.lz",
            // PC-DOS 5.02
            "pcdos_5.02_dsdd.img.lz","pcdos_5.02_dsdd8.img.lz","pcdos_5.02_dshd.img.lz","pcdos_5.02_mf2dd.img.lz",
            "pcdos_5.02_mf2ed.img.lz","pcdos_5.02_mf2hd.img.lz","pcdos_5.02_ssdd.img.lz","pcdos_5.02_ssdd8.img.lz",
            // PC-DOS 6.10
            "pcdos_6.10_dsdd.img.lz","pcdos_6.10_dsdd8.img.lz","pcdos_6.10_dshd.img.lz","pcdos_6.10_mf2dd.img.lz",
            "pcdos_6.10_mf2ed.img.lz","pcdos_6.10_mf2hd.img.lz","pcdos_6.10_ssdd.img.lz","pcdos_6.10_ssdd8.img.lz",
            // PC-DOS 6.30
            "pcdos_6.30_dsdd.img.lz","pcdos_6.30_dsdd8.img.lz","pcdos_6.30_dshd.img.lz","pcdos_6.30_mf2dd.img.lz",
            "pcdos_6.30_mf2ed.img.lz","pcdos_6.30_mf2hd.img.lz","pcdos_6.30_ssdd.img.lz","pcdos_6.30_ssdd8.img.lz",
        };

        readonly MediaType[] mediatypes = {
            // Concurrent DOS 6.00
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,
            // DR-DOS 3.40
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,
            // DR-DOS 3.41
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,
            // DR-DOS 5.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,
            // DR-DOS 6.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,
            // DR-DOS 7.02
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // DR-DOS 7.03
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,
            // DR-DOS 8.00
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.30A
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.31
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 4.01
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 5.00
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 6.00
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 6.20
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 6.20 RC1
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 6.21
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 6.22
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 7.10
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.20 for Amstrad
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 2.11 for AT&T
            MediaType.DOS_525_DS_DD_9,
            // MS-DOS 3.30 for DeLL
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.10 for Epson
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,
            // MS-DOS 3.20 for Epson
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.20 for HP
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.21 for Hyosung
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.21 for Kaypro
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 3.10 for Olivetti
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_HD,MediaType.DOS_525_SS_DD_9,
            // MS-DOS 3.30 for Toshiba
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // MS-DOS 4.01 for Toshiba
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // Novell DOS 7.00
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // OpenDOS 7.01
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // PC-DOS 2.00
            MediaType.DOS_525_DS_DD_9,
            // PC-DOS 2.10
            MediaType.DOS_525_DS_DD_9,
            // PC-DOS 2000
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // PC-DOS 3.00
            MediaType.DOS_525_HD,
            // PC-DOS 3.10
            MediaType.DOS_525_HD,
            // PC-DOS 3.30
            MediaType.DOS_525_HD,MediaType.DOS_35_HD,
            // PC-DOS 4.00
            MediaType.DOS_525_HD,MediaType.DOS_35_HD,
            // PC-DOS 5.00
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // PC-DOS 5.02
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // PC-DOS 6.10
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
            // PC-DOS 6.30
            MediaType.DOS_525_DS_DD_9,MediaType.DOS_525_DS_DD_8,MediaType.DOS_525_HD,MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED,MediaType.DOS_35_HD,MediaType.DOS_525_SS_DD_9,MediaType.DOS_525_SS_DD_8,
        };

        readonly ulong[] sectors = {
            // Concurrent DOS 6.00
            2400, 1440, 2880,
            // DR-DOS 3.40
            720, 640, 2400, 1440, 2880, 360, 320,
            // DR-DOS 3.41
            720,640,2400,1440,2880,360,320,
            // DR-DOS 5.00
            720,640,2400,1440,2880,360,320,
            // DR-DOS 6.00
            720,640,2400,1440,5760,2880,360,320,
            // DR-DOS 7.02
            720,640,2400,1440,5760,2880,360,320,
            // DR-DOS 7.03
            720,640,2400,1440,5760,2880,
            // DR-DOS 8.00
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 3.30A
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 3.31
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 4.01
            720,640,2400,1440,2880,360,320,
            // MS-DOS 5.00
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 6.00
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 6.20
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 6.20 RC1
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 6.21
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 6.22
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 7.10
            720,640,2400,1440,5760,2880,360,320,
            // MS-DOS 3.20 for Amstrad
            720,640,2400,1440,360,320,
            // MS-DOS 2.11 for AT&T
            720,
            // MS-DOS 3.30 for DeLL
            720,640,2400,1440,2880,360,320,
            // MS-DOS 3.10 for Epson
            720,640,2400,
            // MS-DOS 3.20 for Epson
            720,640,2400,1440,360,320,
            // MS-DOS 3.20 for HP
            720,640,2400,1440,2880,360,320,
            // MS-DOS 3.21 for Hyosung
            720,640,2400,1440,2880,360,320,
            // MS-DOS 3.21 for Kaypro
            720,640,2400,1440,2880,360,320,
            // MS-DOS 3.10 for Olivetti
            720,2400,360,
            // MS-DOS 3.30 for Toshiba
            720,640,2400,1440,2880,360,320,
            // MS-DOS 4.01 for Toshiba
            720,640,2400,1440,2880,360,320,
            // Novell DOS 7.00
            720,640,2400,1440,5760,2880,360,320,
            // OpenDOS 7.01
            720,640,2400,1440,5760,2880,360,320,
            // PC-DOS 2.00
            720,
            // PC-DOS 2.10
            720,
            // PC-DOS 2000
            720,640,2400,1440,5760,2880,360,320,
            // PC-DOS 3.00
            2400,
            // PC-DOS 3.10
            2400,
            // PC-DOS 3.30
            2400,2880,
            // PC-DOS 4.00
            2400,2880,
            // PC-DOS 5.00
            720,640,2400,1440,5760,2880,360,320,
            // PC-DOS 5.02
            720,640,2400,1440,5760,2880,360,320,
            // PC-DOS 6.10
            720,640,2400,1440,5760,2880,360,320,
            // PC-DOS 6.30
            720,640,2400,1440,5760,2880,360,320,
        };

        readonly uint[] sectorsize = {
            // Concurrent DOS 6.00
            512, 512, 512,
            // DR-DOS 3.40
            512, 512, 512, 512, 512, 512, 512,
            // DR-DOS 3.41
            512,512,512,512,512,512,512,
            // DR-DOS 5.00
            512,512,512,512,512,512,512,
            // DR-DOS 6.00
            512,512,512,512,512,512,512,512,
            // DR-DOS 7.02
            512,512,512,512,512,512,512,512,
            // DR-DOS 7.03
            512,512,512,512,512,512,
            // DR-DOS 8.00
            512,512,512,512,512,512,512,512,
            // MS-DOS 3.30A
            512,512,512,512,512,512,512,512,
            // MS-DOS 3.31
            512,512,512,512,512,512,512,512,
            // MS-DOS 4.01
            512,512,512,512,512,512,512,
            // MS-DOS 5.00
            512,512,512,512,512,512,512,512,
            // MS-DOS 6.00
            512,512,512,512,512,512,512,512,
            // MS-DOS 6.20
            512,512,512,512,512,512,512,512,
            // MS-DOS 6.20 RC1
            512,512,512,512,512,512,512,512,
            // MS-DOS 6.21
            512,512,512,512,512,512,512,512,
            // MS-DOS 6.22
            512,512,512,512,512,512,512,512,
            // MS-DOS 7.10
            512,512,512,512,512,512,512,512,
            // MS-DOS 3.20 for Amstrad
            512,512,512,512,512,512,
            // MS-DOS 2.11 for AT&T
            512,
            // MS-DOS 3.30 for DeLL
            512,512,512,512,512,512,512,
            // MS-DOS 3.10 for Epson
            512,512,512,
            // MS-DOS 3.20 for Epson
            512,512,512,512,512,512,
            // MS-DOS 3.20 for HP
            512,512,512,512,512,512,512,
            // MS-DOS 3.21 for Hyosung
            512,512,512,512,512,512,512,
            // MS-DOS 3.21 for Kaypro
            512,512,512,512,512,512,512,
            // MS-DOS 3.10 for Olivetti
            512,512,512,
            // MS-DOS 3.30 for Toshiba
            512,512,512,512,512,512,512,
            // MS-DOS 4.01 for Toshiba
            512,512,512,512,512,512,512,
            // Novell DOS 7.00
            512,512,512,512,512,512,512,512,
            // OpenDOS 7.01
            512,512,512,512,512,512,512,512,
            // PC-DOS 2.00
            512,
            // PC-DOS 2.10
            512,
            // PC-DOS 2000
            512,512,512,512,512,512,512,512,
            // PC-DOS 3.00
            512,
            // PC-DOS 3.10
            512,
            // PC-DOS 3.30
            512,512,
            // PC-DOS 4.00
            512,512,
            // PC-DOS 5.00
            512,512,512,512,512,512,512,512,
            // PC-DOS 5.02
            512,512,512,512,512,512,512,512,
            // PC-DOS 6.10
            512,512,512,512,512,512,512,512,
            // PC-DOS 6.30
            512,512,512,512,512,512,512,512,
        };

        readonly long[] clusters = {
            // Concurrent DOS 6.00
            2400, 720, 2880,
            // DR-DOS 3.40
            360,320,2400,720,2880,360,320,
            // DR-DOS 3.41
            360,320,2400,720,2880,360,320,
            // DR-DOS 5.00
            360,320,2400,720,2880,360,320,
            // DR-DOS 6.00
            360,320,2400,720,2880,2880,360,320,
            // DR-DOS 7.02
            360,320,2400,720,2880,2880,360,320,
            // DR-DOS 7.03
            360,320,2400,720,2880,2880,
            // DR-DOS 8.00
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 3.30A
            360,320,2400,720,5760,2880,360,320,
            // MS-DOS 3.31
            360,320,2400,720,5760,2880,360,320,
            // MS-DOS 4.01
            360,320,2400,720,2880,360,320,
            // MS-DOS 5.00
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 6.00
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 6.20
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 6.20 RC1
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 6.21
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 6.22
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 7.10
            360,320,2400,720,2880,2880,360,320,
            // MS-DOS 3.20 for Amstrad
            360,320,2400,720,360,320,
            // MS-DOS 2.11 for AT&T
            360,
            // MS-DOS 3.30 for DeLL
            360,320,2400,720,2880,360,320,
            // MS-DOS 3.10 for Epson
            360,320,2400,
            // MS-DOS 3.20 for Epson
            360,320,2400,720,360,320,
            // MS-DOS 3.20 for HP
            360,320,2400,720,2880,360,320,
            // MS-DOS 3.21 for Hyosung
            360,320,2400,720,2880,360,320,
            // MS-DOS 3.21 for Kaypro
            360,320,2400,720,2880,360,320,
            // MS-DOS 3.10 for Olivetti
            360,2400,360,
            // MS-DOS 3.30 for Toshiba
            360,320,2400,720,2880,360,320,
            // MS-DOS 4.01 for Toshiba
            360,320,2400,720,2880,360,320,
            // Novell DOS 7.00
            360,320,2400,720,2880,2880,360,320,
            // OpenDOS 7.01
            360,320,2400,720,2880,2880,360,320,
            // PC-DOS 2.00
            360,
            // PC-DOS 2.10
            360,
            // PC-DOS 2000
            360,320,2400,720,2880,2880,360,320,
            // PC-DOS 3.00
            2400,
            // PC-DOS 3.10
            2400,
            // PC-DOS 3.30
            2400,2880,
            // PC-DOS 4.00
            2400,2880,
            // PC-DOS 5.00
            360,320,2400,720,2880,2880,360,320,
            // PC-DOS 5.02
            360,320,2400,720,2880,2880,360,320,
            // PC-DOS 6.10
            360,320,2400,720,2880,2880,360,320,
            // PC-DOS 6.30
            360,320,2400,720,2880,2880,360,320,
        };

        readonly int[] clustersize = {
            // Concurrent DOS 6.00
            512, 1024, 512,
            // DR-DOS 3.40
            1024,1024,512,1024,512,512,512,
            // DR-DOS 3.41
            1024,1024,512,1024,512,512,512,
            // DR-DOS 5.00
            1024,1024,512,1024,512,512,512,
            // DR-DOS 6.00
            1024,1024,512,1024,1024,512,512,512,
            // DR-DOS 7.02
            1024,1024,512,1024,1024,512,512,512,
            // DR-DOS 7.03
            1024,1024,512,1024,1024,512,
            // DR-DOS 8.00
            1024,1024,512,1024,1024,512,512,512,
            // MS-DOS 3.30A
            1024,/*1024,*/512,1024,512,512,512,512,
            // MS-DOS 3.31
            1024,/*1024,*/512,1024,512,512,512,512,
            // MS-DOS 4.01
            1024,/*1024,*/512,1024,512,512,512,
            // MS-DOS 5.00
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 6.00
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 6.20
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 6.20 RC1
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 6.21
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 6.22
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 7.10
            1024,/*1024,*/512,1024,1024,512,512,512,
            // MS-DOS 3.20 for Amstrad
            1024,/*1024,*/512,1024,512,512,
            // MS-DOS 2.11 for AT&T
            1024,
            // MS-DOS 3.30 for DeLL
            1024,/*1024,*/512,1024,512,512,512,
            // MS-DOS 3.10 for Epson
            1024,1024,512,
            // MS-DOS 3.20 for Epson
            1024,1024,512,1024,512,512,
            // MS-DOS 3.20 for HP
            1024,/*1024,*/512,1024,512,512,512,
            // MS-DOS 3.21 for Hyosung
            1024,/*1024,*/512,1024,512,512,512,
            // MS-DOS 3.21 for Kaypro
            1024,/*1024,*/512,1024,512,512,512,
            // MS-DOS 3.10 for Olivetti
            1024,512,512,
            // MS-DOS 3.30 for Toshiba
            1024,/*1024,*/512,1024,512,512,512,
            // MS-DOS 4.01 for Toshiba
            1024,1024,512,1024,512,512,512,
            // Novell DOS 7.00
            1024,1024,512,1024,1024,512,512,512,
            // OpenDOS 7.01
            1024,1024,512,1024,1024,512,512,512,
            // PC-DOS 2.00
            1024,
            // PC-DOS 2.10
            1024,
            // PC-DOS 2000
            1024,/*1024,*/512,1024,1024,512,512,512,
            // PC-DOS 3.00
            512,
            // PC-DOS 3.10
            512,
            // PC-DOS 3.30
            512,512,
            // PC-DOS 4.00
            512,512,
            // PC-DOS 5.00
            1024,/*1024,*/512,1024,1024,512,512,512,
            // PC-DOS 5.02
            1024,/*1024,*/512,1024,1024,512,512,512,
            // PC-DOS 6.10
            1024,/*1024,*/512,1024,1024,512,512,512,
            // PC-DOS 6.30
            1024,/*1024,*/512,1024,1024,512,512,512,
        };

        readonly string[] volumename = {
            // Concurrent DOS 6.00
            null, null, null,
            // DR-DOS 3.40
            null, null, null, null, null, null, null,
            // DR-DOS 3.41
            null, null, null, null, null, null, null,
            // DR-DOS 5.00
            null, null, null, null, null, null, null,
            // DR-DOS 6.00
            null, null, null, null, null, null, null, null,
            // DR-DOS 7.02
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            // DR-DOS 7.03
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            // DR-DOS 8.00
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            // MS-DOS 3.30A
            null, null, null, null, null, null, null,null,
            // MS-DOS 3.31
            null, null, null, null, null, null, null,null,
            // MS-DOS 4.01
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 5.00
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 6.00
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 6.20
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 6.20 RC1
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 6.21
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 6.22
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 7.10
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // MS-DOS 3.20 for Amstrad
            null,null,null,null,null,null,
            // MS-DOS 2.11 for AT&T
            null,
            // MS-DOS 3.30 for DeLL
            null,null,null,null,null,null,null,
            // MS-DOS 3.10 for Epson
            null,null,null,
            // MS-DOS 3.20 for Epson
            null,null,null,null,null,null,
            // MS-DOS 3.20 for HP
            null,null,null,null,null,null,null,
            // MS-DOS 3.21 for Hyosung
            null,null,null,null,null,null,null,
            // MS-DOS 3.21 for Kaypro
            null,null,null,null,null,null,null,
            // MS-DOS 3.10 for Olivetti
            null,null,null,
            // MS-DOS 3.30 for Toshiba
            null,null,null,null,null,null,null,
            // MS-DOS 4.01 for Toshiba
            "VOLUMELABEL","NO NAME    ","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","NO NAME    ",
            // Novell DOS 7.00
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            // OpenDOS 7.01
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            // PC-DOS 2.00
            null,
            // PC-DOS 2.10
            null,
            // PC-DOS 2000
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // PC-DOS 3.00
            null,
            // PC-DOS 3.10
            null,
            // PC-DOS 3.30
            null,null,
            // PC-DOS 4.00
            "VOLUMELABEL","VOLUMELABEL",
            // PC-DOS 5.00
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // PC-DOS 5.02
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // PC-DOS 6.10
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
            // PC-DOS 6.30
            "VOLUMELABEL",null,"VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",null,
        };

        readonly string[] volumeserial = {
            // Concurrent DOS 6.00
            null, null, null,
            // DR-DOS 3.40
            null, null, null, null, null, null, null,
            // DR-DOS 3.41
            null, null, null, null, null, null, null,
            // DR-DOS 5.00
            null, null, null, null, null, null, null,
            // DR-DOS 6.00
            null, null, null, null, null, null, null, null,
            // DR-DOS 7.02
            "1BF63C69","1BF70E75","1BF7185F","1BF80C4F","1BF90F1D","1BF82777","1BF72430","1BF72F1E",
            // DR-DOS 7.03
            "0C1A2013","0CE22B5B","0CEA1D3E","0CEE102F","0CEE3760","0CEF2739",
            // DR-DOS 8.00
            "1BFD1977","1BFD2D3F","1BFD3531","1BFC3231","1BFA1D58","1BFC117D","1BFE0971","1BFE1423",
            // MS-DOS 3.30A
            null, null, null, null, null, null, null,null,
            // MS-DOS 3.31
            null, null, null, null, null, null, null,null,
            // MS-DOS 4.01
            "122C190A",null,"2480190A","2D471909","0F5A1908","2F3D190A",null,
            // MS-DOS 5.00
            "0B6018F8",null,"1E3518F8","285A18FB","231D18FE","415118FC","316118F8",null,
            // MS-DOS 6.00
            "067B18F6",null,"193418F6","1F3A18F5","165318F3","172418F4","234918F6",null,
            // MS-DOS 6.20
            "265418ED",null,"0B7018EE","127418F0","137F18F2","364C18F0","185C18EE",null,
            // MS-DOS 6.20 RC1
            "064B18EB",null,"192518EB","244C18EA","3C3118E7","344118E9","267E18EB",null,
            // MS-DOS 6.21
            "2A41181B",null,"0641181C","3B26181C","082518E2","237118E1","123F181C",null,
            // MS-DOS 6.22
            "317C1818",null,"0D3A1819","3C251817","387A1815","185E1817","18231819",null,
            // MS-DOS 7.10
            "1156180A",null,"2951180A","3057180B","2B4A1811","344B180C","352D180A",null,
            // MS-DOS 3.20 for Amstrad
            null,null,null,null,null,null,
            // MS-DOS 2.11 for AT&T
            null,
            // MS-DOS 3.30 for DeLL
            null,null,null,null,null,null,null,
            // MS-DOS 3.10 for Epson
            null,null,null,
            // MS-DOS 3.20 for Epson
            null,null,null,null,null,null,
            // MS-DOS 3.20 for HP
            null,null,null,null,null,null,null,
            // MS-DOS 3.21 for Hyosung
            null,null,null,null,null,null,null,
            // MS-DOS 3.21 for Kaypro
            null,null,null,null,null,null,null,
            // MS-DOS 3.10 for Olivetti
            null,null,null,
            // MS-DOS 3.30 for Toshiba
            null,null,null,null,null,null,null,
            // MS-DOS 4.01 for Toshiba
            "0B2519E7","163419E7","1E3119E7","133919E9","177419EA","317E19E7","3B7319E7",
            // Novell DOS 7.00
            "1BE7254C","1BE73024","1BE7397C","1BE63635","1BE51661","1BE61143","1BE80A5D","1BE8144C",
            // OpenDOS 7.01
            "1BE93E2B","1BEA234D","1BEA325D","1BEB294F","1BEC2C2E","1BEC0C5D","1BEA3E60","1BEB0E26",
            // PC-DOS 2.00
            null,
            // PC-DOS 2.10
            null,
            // PC-DOS 2000
            "2634100E",null,"3565100E","3B6B1012","3B2D1013","1D491013","4136100E",null,
            // PC-DOS 3.00
            null,
            // PC-DOS 3.10
            null,
            // PC-DOS 3.30
            null,null,
            // PC-DOS 4.00
            "3C240FE3","2E3E0FE1",
            // PC-DOS 5.00
            "33260FF9",null,"11550FFA","234F0FFB","2F600FFC","0D550FFC","1D630FFA",null,
            // PC-DOS 5.02
            "06231000",null,"1A3E1000","1F3B0FFF","3D750FFD","3F4F0FFE","26471000",null,
            // PC-DOS 6.10
            "25551004",null,"3E5F1004","142D1006","17541007","355A1006","0D5E1005",null,
            // PC-DOS 6.30
            "2B22100C",null,"3B47100C","0C55100C","1B80100A","0B59100B","0A3A100D",null,
        };

        readonly string[] oemid = {
            // Concurrent DOS 6.00
            "DIGITAL ", "DIGITAL ", "DIGITAL ",
            // DR-DOS 3.40
            "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ",
            // DR-DOS 3.41
            "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ", "DIGITAL ",
            // DR-DOS 5.00
            "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3",
            // DR-DOS 6.00
            "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3",
            // DR-DOS 7.02
            "DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7",
            // DR-DOS 7.03
            "DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7",
            // DR-DOS 8.00
            "DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7","DRDOS  7",
            // MS-DOS 3.30A
            "MSDOS3.3",null,"MSDOS3.3","MSDOS3.3","MSDOS3.3","MSDOS3.3","MSDOS3.3",null,
            // MS-DOS 3.31
            "IBM  3.3",null,"IBM  3.3","IBM  3.3","IBM  3.3","IBM  3.3","IBM  3.3",null,
            // MS-DOS 4.01
            "MSDOS4.0",null,"MSDOS4.0","MSDOS4.0","MSDOS4.0","MSDOS4.0",null,
            // MS-DOS 5.00
            "MSDOS5.0",null,"MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0",null,
            // MS-DOS 6.00
            "MSDOS5.0",null,"MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0",null,
            // MS-DOS 6.20
            "MSDOS5.0",null,"MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0",null,
            // MS-DOS 6.20 RC1
            "MSDOS5.0",null,"MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0",null,
            // MS-DOS 6.21
            "MSDOS5.0",null,"MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0",null,
            // MS-DOS 6.22
            "MSDOS5.0",null,"MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0","MSDOS5.0",null,
            // MS-DOS 7.10
            "MSWIN4.1",null,"MSWIN4.1","MSWIN4.1","MSWIN4.1","MSWIN4.1","MSWIN4.1",null,
            // MS-DOS 3.20 for Amstrad
            "MSDOS3.2",null,"MSDOS3.2","MSDOS3.2","MSDOS3.2",null,
            // MS-DOS 2.11 for AT&T
            "PSA 1.04",
            // MS-DOS 3.30 for DeLL
            "IBM  3.3",null,"IBM  3.3","IBM  3.3","IBM  3.3","IBM  3.3",null,
            // MS-DOS 3.10 for Epson
            "EPS 3.10","EPS 3.10","EPS 3.10",
            // MS-DOS 3.20 for Epson
            "IBM  3.2","IBM  3.2","IBM  3.2","IBM  3.2","IBM  3.2","IBM  3.2",
            // MS-DOS 3.20 for HP
            "MSDOS3.2",null,"MSDOS3.2","MSDOS3.2","MSDOS3.2","MSDOS3.2",null,
            // MS-DOS 3.21 for Hyosung
            "MSDOS3.2",null,"MSDOS3.2","MSDOS3.2","MSDOS3.2","MSDOS3.2",null,
            // MS-DOS 3.21 for Kaypro
            "MSDOS3.2",null,"MSDOS3.2","MSDOS3.2","MSDOS3.2","MSDOS3.2",null,
            // MS-DOS 3.10 for Olivetti
            "IBM  3.1","IBM  3.1","IBM  3.1",
            // MS-DOS 3.30 for Toshiba
            "IBM  3.3",null,"IBM  3.3","IBM  3.3","IBM  3.3","IBM  3.3",null,
            // MS-DOS 4.01 for Toshiba
            "T V4.00 ","T V4.00 ","T V4.00 ","T V4.00 ","T V4.00 ","T V4.00 ","T V4.00 ",
            // Novell DOS 7.00
            "NWDOS7.0","NWDOS7.0","NWDOS7.0","NWDOS7.0","NWDOS7.0","NWDOS7.0","NWDOS7.0","NWDOS7.0",
            // OpenDOS 7.01
            "OPENDOS7","OPENDOS7","OPENDOS7","OPENDOS7","OPENDOS7","OPENDOS7","OPENDOS7","OPENDOS7",
            // PC-DOS 2.00
            "IBM  2.0",
            // PC-DOS 2.10
            "IBM  2.0",
            // PC-DOS 2000
            "IBM  7.0",null,"IBM  7.0","IBM  7.0","IBM  7.0","IBM  7.0","IBM  7.0",null,
            // PC-DOS 3.00
            "IBM  3.0",
            // PC-DOS 3.10
            "IBM  3.1",
            // PC-DOS 3.30
            "IBM  3.3","IBM  3.3",
            // PC-DOS 4.00
            "IBM  4.0","IBM  4.0",
            // PC-DOS 5.00
            "IBM  5.0",null,"IBM  5.0","IBM  5.0","IBM  5.0","IBM  5.0","IBM  5.0",null,
            // PC-DOS 5.02
            "IBM  5.0",null,"IBM  5.0","IBM  5.0","IBM  5.0","IBM  5.0","IBM  5.0",null,
            // PC-DOS 6.10
            "IBM  6.0",null,"IBM  6.0","IBM  6.0","IBM  6.0","IBM  6.0","IBM  6.0",null,
            // PC-DOS 6.30
            "IBM  6.0",null,"IBM  6.0","IBM  6.0","IBM  6.0","IBM  6.0","IBM  6.0",null,
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "fat12", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.mediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                Filesystem fs = new FAT();
                Assert.AreEqual(true, fs.Identify(image, 0, image.ImageInfo.sectors - 1), testfiles[i]);
                fs.GetInformation(image, 0, image.ImageInfo.sectors - 1, out string information);
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
