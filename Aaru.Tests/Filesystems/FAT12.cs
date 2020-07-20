// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT12.cs
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems.FAT;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Fat12
    {
        readonly string[] testfiles =
        {
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
            "msdos_3.30A_dsdd.img.lz", "msdos_3.30A_dsdd8.img.lz", "msdos_3.30A_dshd.img.lz",
            "msdos_3.30A_mf2dd.img.lz", "msdos_3.30A_mf2hd.img.lz", "msdos_3.30A_ssdd.img.lz",
            "msdos_3.30A_ssdd8.img.lz",

            // MS-DOS 3.31
            "msdos_3.31_dsdd.img.lz", "msdos_3.31_dsdd8.img.lz", "msdos_3.31_dshd.img.lz", "msdos_3.31_mf2dd.img.lz",
            "msdos_3.31_mf2hd.img.lz", "msdos_3.31_ssdd.img.lz", "msdos_3.31_ssdd8.img.lz",

            // MS-DOS 4.01
            "msdos_4.01_dsdd.img.lz", "msdos_4.01_dsdd8.img.lz", "msdos_4.01_dshd.img.lz", "msdos_4.01_mf2dd.img.lz",
            "msdos_4.01_mf2hd.img.lz", "msdos_4.01_ssdd.img.lz", "msdos_4.01_ssdd8.img.lz",

            // MS-DOS 5.00
            "msdos_5.00_dsdd.img.lz", "msdos_5.00_dsdd8.img.lz", "msdos_5.00_dshd.img.lz", "msdos_5.00_mf2dd.img.lz",
            "msdos_5.00_mf2ed.img.lz", "msdos_5.00_mf2hd.img.lz", "msdos_5.00_ssdd.img.lz", "msdos_5.00_ssdd8.img.lz",

            // MS-DOS 6.00
            "msdos_6.00_dsdd.img.lz", "msdos_6.00_dsdd8.img.lz", "msdos_6.00_dshd.img.lz", "msdos_6.00_mf2dd.img.lz",
            "msdos_6.00_mf2ed.img.lz", "msdos_6.00_mf2hd.img.lz", "msdos_6.00_ssdd.img.lz", "msdos_6.00_ssdd8.img.lz",

            // MS-DOS 6.20
            "msdos_6.20_dsdd.img.lz", "msdos_6.20_dsdd8.img.lz", "msdos_6.20_dshd.img.lz", "msdos_6.20_mf2dd.img.lz",
            "msdos_6.20_mf2ed.img.lz", "msdos_6.20_mf2hd.img.lz", "msdos_6.20_ssdd.img.lz", "msdos_6.20_ssdd8.img.lz",

            // MS-DOS 6.20 RC1
            "msdos_6.20rc1_dsdd.img.lz", "msdos_6.20rc1_dsdd8.img.lz", "msdos_6.20rc1_dshd.img.lz",
            "msdos_6.20rc1_mf2dd.img.lz", "msdos_6.20rc1_mf2ed.img.lz", "msdos_6.20rc1_mf2hd.img.lz",
            "msdos_6.20rc1_ssdd.img.lz", "msdos_6.20rc1_ssdd8.img.lz",

            // MS-DOS 6.21
            "msdos_6.21_dsdd.img.lz", "msdos_6.21_dsdd8.img.lz", "msdos_6.21_dshd.img.lz", "msdos_6.21_mf2dd.img.lz",
            "msdos_6.21_mf2ed.img.lz", "msdos_6.21_mf2hd.img.lz", "msdos_6.21_ssdd.img.lz", "msdos_6.21_ssdd8.img.lz",

            // MS-DOS 6.22
            "msdos_6.22_dsdd.img.lz", "msdos_6.22_dsdd8.img.lz", "msdos_6.22_dshd.img.lz", "msdos_6.22_mf2dd.img.lz",
            "msdos_6.22_mf2ed.img.lz", "msdos_6.22_mf2hd.img.lz", "msdos_6.22_ssdd.img.lz", "msdos_6.22_ssdd8.img.lz",

            // MS-DOS 7.10
            "msdos_7.10_dsdd.img.lz", "msdos_7.10_dsdd8.img.lz", "msdos_7.10_dshd.img.lz", "msdos_7.10_mf2dd.img.lz",
            "msdos_7.10_mf2ed.img.lz", "msdos_7.10_mf2hd.img.lz", "msdos_7.10_ssdd.img.lz", "msdos_7.10_ssdd8.img.lz",

            // MS-DOS 3.20 for Amstrad
            "msdos_amstrad_3.20_dsdd.img.lz", "msdos_amstrad_3.20_dsdd8.img.lz", "msdos_amstrad_3.20_dshd.img.lz",
            "msdos_amstrad_3.20_mf2dd.img.lz", "msdos_amstrad_3.20_ssdd.img.lz", "msdos_amstrad_3.20_ssdd8.img.lz",

            // MS-DOS 2.11 for AT&T
            "msdos_att_2.11_dsdd.img.lz",

            // MS-DOS 3.30 for DeLL
            "msdos_dell_3.30_dsdd.img.lz", "msdos_dell_3.30_dsdd8.img.lz", "msdos_dell_3.30_dshd.img.lz",
            "msdos_dell_3.30_mf2dd.img.lz", "msdos_dell_3.30_mf2hd.img.lz", "msdos_dell_3.30_ssdd.img.lz",
            "msdos_dell_3.30_ssdd8.img.lz",

            // MS-DOS 3.10 for Epson
            "msdos_epson_3.10_dsdd.img.lz", "msdos_epson_3.10_dsdd8.img.lz", "msdos_epson_3.10_dshd.img.lz",

            // MS-DOS 3.20 for Epson
            "msdos_epson_3.20_dsdd.img.lz", "msdos_epson_3.20_dsdd8.img.lz", "msdos_epson_3.20_dshd.img.lz",
            "msdos_epson_3.20_mf2dd.img.lz", "msdos_epson_3.20_ssdd.img.lz", "msdos_epson_3.20_ssdd8.img.lz",

            // MS-DOS 3.20 for HP
            "msdos_hp_3.20_dsdd.img.lz", "msdos_hp_3.20_dsdd8.img.lz", "msdos_hp_3.20_dshd.img.lz",
            "msdos_hp_3.20_mf2dd.img.lz", "msdos_hp_3.20_mf2hd.img.lz", "msdos_hp_3.20_ssdd.img.lz",
            "msdos_hp_3.20_ssdd8.img.lz",

            // MS-DOS 3.21 for Hyosung
            "msdos_hyonsung_3.21_dsdd.img.lz", "msdos_hyonsung_3.21_dsdd8.img.lz", "msdos_hyonsung_3.21_dshd.img.lz",
            "msdos_hyonsung_3.21_mf2dd.img.lz", "msdos_hyonsung_3.21_mf2hd.img.lz", "msdos_hyonsung_3.21_ssdd.img.lz",
            "msdos_hyonsung_3.21_ssdd8.img.lz",

            // MS-DOS 3.21 for Kaypro
            "msdos_kaypro_3.21_dsdd.img.lz", "msdos_kaypro_3.21_dsdd8.img.lz", "msdos_kaypro_3.21_dshd.img.lz",
            "msdos_kaypro_3.21_mf2dd.img.lz", "msdos_kaypro_3.21_mf2hd.img.lz", "msdos_kaypro_3.21_ssdd.img.lz",
            "msdos_kaypro_3.21_ssdd8.img.lz",

            // MS-DOS 3.10 for Olivetti
            "msdos_olivetti_3.10_dsdd.img.lz", "msdos_olivetti_3.10_dshd.img.lz", "msdos_olivetti_3.10_ssdd.img.lz",

            // MS-DOS 3.30 for Toshiba
            "msdos_toshiba_3.30_dsdd.img.lz", "msdos_toshiba_3.30_dsdd8.img.lz", "msdos_toshiba_3.30_dshd.img.lz",
            "msdos_toshiba_3.30_mf2dd.img.lz", "msdos_toshiba_3.30_mf2hd.img.lz", "msdos_toshiba_3.30_ssdd.img.lz",
            "msdos_toshiba_3.30_ssdd8.img.lz",

            // MS-DOS 4.01 for Toshiba
            "msdos_toshiba_4.01_dsdd.img.lz", "msdos_toshiba_4.01_dsdd8.img.lz", "msdos_toshiba_4.01_dshd.img.lz",
            "msdos_toshiba_4.01_mf2dd.img.lz", "msdos_toshiba_4.01_mf2hd.img.lz", "msdos_toshiba_4.01_ssdd.img.lz",
            "msdos_toshiba_4.01_ssdd8.img.lz",

            // Novell DOS 7.00
            "novelldos_7.00_dsdd.img.lz", "novelldos_7.00_dsdd8.img.lz", "novelldos_7.00_dshd.img.lz",
            "novelldos_7.00_mf2dd.img.lz", "novelldos_7.00_mf2ed.img.lz", "novelldos_7.00_mf2hd.img.lz",
            "novelldos_7.00_ssdd.img.lz", "novelldos_7.00_ssdd8.img.lz",

            // OpenDOS 7.01
            "opendos_7.01_dsdd.img.lz", "opendos_7.01_dsdd8.img.lz", "opendos_7.01_dshd.img.lz",
            "opendos_7.01_mf2dd.img.lz", "opendos_7.01_mf2ed.img.lz", "opendos_7.01_mf2hd.img.lz",
            "opendos_7.01_ssdd.img.lz", "opendos_7.01_ssdd8.img.lz",

            // PC-DOS 2.00
            "pcdos_2.00_dsdd.img.lz",

            // PC-DOS 2.10
            "pcdos_2.10_dsdd.img.lz",

            // PC-DOS 2000
            "pcdos_2000_dsdd.img.lz", "pcdos_2000_dsdd8.img.lz", "pcdos_2000_dshd.img.lz", "pcdos_2000_mf2dd.img.lz",
            "pcdos_2000_mf2ed.img.lz", "pcdos_2000_mf2hd.img.lz", "pcdos_2000_ssdd.img.lz", "pcdos_2000_ssdd8.img.lz",

            // PC-DOS 3.00
            "pcdos_3.00_dshd.img.lz",

            // PC-DOS 3.10
            "pcdos_3.10_dshd.img.lz",

            // PC-DOS 3.30
            "pcdos_3.30_dshd.img.lz", "pcdos_3.30_mf2hd.img.lz",

            // PC-DOS 4.00
            "pcdos_4.00_dshd.img.lz", "pcdos_4.00_mf2hd.img.lz",

            // PC-DOS 5.00
            "pcdos_5.00_dsdd.img.lz", "pcdos_5.00_dsdd8.img.lz", "pcdos_5.00_dshd.img.lz", "pcdos_5.00_mf2dd.img.lz",
            "pcdos_5.00_mf2ed.img.lz", "pcdos_5.00_mf2hd.img.lz", "pcdos_5.00_ssdd.img.lz", "pcdos_5.00_ssdd8.img.lz",

            // PC-DOS 5.02
            "pcdos_5.02_dsdd.img.lz", "pcdos_5.02_dsdd8.img.lz", "pcdos_5.02_dshd.img.lz", "pcdos_5.02_mf2dd.img.lz",
            "pcdos_5.02_mf2ed.img.lz", "pcdos_5.02_mf2hd.img.lz", "pcdos_5.02_ssdd.img.lz", "pcdos_5.02_ssdd8.img.lz",

            // PC-DOS 6.10
            "pcdos_6.10_dsdd.img.lz", "pcdos_6.10_dsdd8.img.lz", "pcdos_6.10_dshd.img.lz", "pcdos_6.10_mf2dd.img.lz",
            "pcdos_6.10_mf2ed.img.lz", "pcdos_6.10_mf2hd.img.lz", "pcdos_6.10_ssdd.img.lz", "pcdos_6.10_ssdd8.img.lz",

            // PC-DOS 6.30
            "pcdos_6.30_dsdd.img.lz", "pcdos_6.30_dsdd8.img.lz", "pcdos_6.30_dshd.img.lz", "pcdos_6.30_mf2dd.img.lz",
            "pcdos_6.30_mf2ed.img.lz", "pcdos_6.30_mf2hd.img.lz", "pcdos_6.30_ssdd.img.lz", "pcdos_6.30_ssdd8.img.lz",

            // mkfs.vfat
            "mkfs.vfat_dshd.img.lz", "mkfs.vfat_mf2dd.img.lz", "mkfs.vfat_mf2ed.img.lz", "mkfs.vfat_mf2hd.img.lz",

            // mkfs.vfat for Atari
            "mkfs.vfat_atari_dshd.img.lz", "mkfs.vfat_atari_mf2dd.img.lz", "mkfs.vfat_atari_mf2ed.img.lz",
            "mkfs.vfat_atari_mf2hd.img.lz",

            // Microsoft OS/2 1.00 for Tandy
            "msos2_1.00_tandy_dsdd.img.lz", "msos2_1.00_tandy_dshd.img.lz", "msos2_1.00_tandy_mf2dd.img.lz",
            "msos2_1.00_tandy_mf2hd.img.lz",

            // Microsoft OS/2 1.10 for AST
            "msos2_1.10_ast_dsdd.img.lz", "msos2_1.10_ast_dshd.img.lz", "msos2_1.10_ast_mf2dd.img.lz",
            "msos2_1.10_ast_mf2hd.img.lz",

            // Microsoft OS/2 1.10 for Nokia
            "msos2_1.10_nokia_dsdd.img.lz", "msos2_1.10_nokia_dshd.img.lz", "msos2_1.10_nokia_mf2dd.img.lz",
            "msos2_1.10_nokia_mf2hd.img.lz",

            // Microsoft OS/2 1.21
            "msos2_1.21_dsdd.img.lz", "msos2_1.21_dshd.img.lz", "msos2_1.21_mf2dd.img.lz", "msos2_1.21_mf2hd.img.lz",

            // Microsoft OS/2 1.30.1
            "msos2_1.30.1_dsdd.img.lz", "msos2_1.30.1_dshd.img.lz", "msos2_1.30.1_mf2dd.img.lz",
            "msos2_1.30.1_mf2ed.img.lz", "msos2_1.30.1_mf2hd.img.lz",

            // OS/2 1.20
            "os2_1.20_dsdd.img.lz", "os2_1.20_dshd.img.lz", "os2_1.20_mf2dd.img.lz", "os2_1.20_mf2hd.img.lz",

            // OS/2 1.30
            "os2_1.30_dsdd.img.lz", "os2_1.30_dshd.img.lz", "os2_1.30_mf2dd.img.lz", "os2_1.30_mf2hd.img.lz",

            // OS/2 2.00
            "os2_6.307_dsdd.img.lz", "os2_6.307_dshd.img.lz", "os2_6.307_mf2dd.img.lz", "os2_6.307_mf2ed.img.lz",
            "os2_6.307_mf2hd.img.lz",

            // OS/2 2.10
            "os2_6.514_dsdd.img.lz", "os2_6.514_dshd.img.lz", "os2_6.514_mf2dd.img.lz", "os2_6.514_mf2ed.img.lz",
            "os2_6.514_mf2hd.img.lz",

            // OS/2 2.11
            "os2_6.617_dsdd.img.lz", "os2_6.617_dshd.img.lz", "os2_6.617_mf2dd.img.lz", "os2_6.617_mf2ed.img.lz",
            "os2_6.617_mf2hd.img.lz",

            // OS/2 Warp 3
            "os2_8.162_dshd.img.lz", "os2_8.162_mf2dd.img.lz", "os2_8.162_mf2ed.img.lz", "os2_8.162_mf2hd.img.lz",

            // OS/2 Warp 4
            "os2_9.023_dshd.img.lz", "os2_9.023_mf2dd.img.lz", "os2_9.023_mf2ed.img.lz", "os2_9.023_mf2hd.img.lz",

            // eComStation
            "ecs_dshd.img.lz", "ecs_mf2dd.img.lz", "ecs_mf2ed.img.lz", "ecs_mf2hd.img.lz",
            "ecs20_mf2hd_fstester.img.lz",

            // Windows 95
            "win95_dsdd8.img.lz", "win95_dsdd.img.lz", "win95_dshd.img.lz", "win95_mf2dd.img.lz", "win95_mf2ed.img.lz",
            "win95_mf2hd.img.lz", "win95_ssdd8.img.lz", "win95_ssdd.img.lz",

            // Windows 95 OSR 2
            "win95osr2_dsdd8.img.lz", "win95osr2_dsdd.img.lz", "win95osr2_dshd.img.lz", "win95osr2_mf2dd.img.lz",
            "win95osr2_mf2ed.img.lz", "win95osr2_mf2hd.img.lz", "win95osr2_ssdd8.img.lz", "win95osr2_ssdd.img.lz",

            // Windows 95 OSR 2.1
            "win95osr2.1_dsdd8.img.lz", "win95osr2.1_dsdd.img.lz", "win95osr2.1_dshd.img.lz",
            "win95osr2.1_mf2dd.img.lz", "win95osr2.1_mf2ed.img.lz", "win95osr2.1_mf2hd.img.lz",
            "win95osr2.1_ssdd8.img.lz", "win95osr2.1_ssdd.img.lz",

            // Windows 95 OSR 2.5
            "win95osr2.5_dsdd8.img.lz", "win95osr2.5_dsdd.img.lz", "win95osr2.5_dshd.img.lz",
            "win95osr2.5_mf2dd.img.lz", "win95osr2.5_mf2ed.img.lz", "win95osr2.5_mf2hd.img.lz",
            "win95osr2.5_ssdd8.img.lz", "win95osr2.5_ssdd.img.lz",

            // Windows 98
            "win98_dsdd8.img.lz", "win98_dsdd.img.lz", "win98_dshd.img.lz", "win98_mf2dd.img.lz", "win98_mf2ed.img.lz",
            "win98_mf2hd.img.lz", "win98_ssdd8.img.lz", "win98_ssdd.img.lz",

            // Windows 98 Second Edition
            "win98se_dsdd8.img.lz", "win98se_dsdd.img.lz", "win98se_dshd.img.lz", "win98se_mf2dd.img.lz",
            "win98se_mf2ed.img.lz", "win98se_mf2hd.img.lz", "win98se_ssdd8.img.lz", "win98se_ssdd.img.lz",

            // Windows Me
            "winme_dsdd.img.lz", "winme_dshd.img.lz", "winme_mf2dd.img.lz", "winme_mf2ed.img.lz", "winme_mf2hd.img.lz",

            // Windows NT 3.10
            "winnt_3.10_dshd.img.lz", "winnt_3.10_mf2dd.img.lz", "winnt_3.10_mf2ed.img.lz", "winnt_3.10_mf2hd.img.lz",

            // Windows NT 3.50
            "winnt_3.50_dshd.img.lz", "winnt_3.50_mf2dd.img.lz", "winnt_3.50_mf2ed.img.lz", "winnt_3.50_mf2hd.img.lz",

            // Windows NT 3.51
            "winnt_3.51_dshd.img.lz", "winnt_3.51_mf2dd.img.lz", "winnt_3.51_mf2ed.img.lz", "winnt_3.51_mf2hd.img.lz",

            // Windows NT 4.00
            "winnt_4_dsdd.img.lz", "winnt_4_dshd.img.lz", "winnt_4_mf2dd.img.lz", "winnt_4_mf2ed.img.lz",
            "winnt_4_mf2hd.img.lz", "winnt_4_ssdd.img.lz",

            // Windows 2000
            "win2000_dsdd.img.lz", "win2000_dshd.img.lz", "win2000_mf2dd.img.lz", "win2000_mf2ed.img.lz",
            "win2000_mf2hd.img.lz",

            // Windows Vista
            "winvista_dsdd.img.lz", "winvista_dshd.img.lz", "winvista_mf2dd.img.lz", "winvista_mf2ed.img.lz",
            "winvista_mf2hd.img.lz",

            // BeOS R4.5
            "beos_r4.5_mf2hd.img.lz",

            // Hatari
            "hatari_mf1dd.st.lz", "hatari_mf1dd_10.st.lz", "hatari_mf1dd_11.st.lz", "hatari_mf2dd.st.lz",
            "hatari_mf2dd_10.st.lz", "hatari_mf2dd_11.st.lz", "hatari_mf2ed.st.lz", "hatari_mf2hd.st.lz",

            // Atari TOS 1.04
            "tos_1.04_mf1dd.st.lz", "tos_1.04_mf2dd.st.lz",

            // NetBSD 1.6
            "netbsd_1.6_mf2dd.img.lz", "netbsd_1.6_mf2hd.img.lz",

            // NeXTStep 3.3
            "nextstep_3.3_mf2dd.img.lz", "nextstep_3.3_mf2hd.img.lz",

            // OpenStep for Mach 4.0
            "openstep_4.0_mf2dd.img.lz", "openstep_4.0_mf2hd.img.lz",

            // OpenStep for Mach 4.2
            "openstep_4.2_mf2dd.img.lz", "openstep_4.2_mf2hd.img.lz",

            // Solaris 2.4
            "solaris_2.4_mf2dd.img.lz", "solaris_2.4_mf2hd.img.lz",

            // COHERENT UNIX 4.2.10
            "coherentunix_4.2.10_dsdd.img.lz", "coherentunix_4.2.10_dshd.img.lz", "coherentunix_4.2.10_mf2dd.img.lz",
            "coherentunix_4.2.10_mf2hd.img.lz",

            // SCO OpenServer 5.0.7Hw
            "scoopenserver_5.0.7hw_dshd.img.lz", "scoopenserver_5.0.7hw_mf2dd.img.lz",
            "scoopenserver_5.0.7hw_mf2hd.img.lz",

            // Epson MS-DOS 5.00 for PC-98
            "msdos_epson_pc98_5.00_md2dd.img.lz", "msdos_epson_pc98_5.00_md2hd.img.lz",

            // NEC MS-DOS 3.30 for PC-98
            "msdos_pc98_3.30_md2dd.img.lz", "msdos_pc98_3.30_md2hd.img.lz",

            // NEC MS-DOS 5.00 for PC-98
            "msdos_pc98_5.00_md2dd.img.lz", "msdos_pc98_5.00_md2hd.img.lz",

            // NEC MS-DOS 6.20 for PC-98
            "msdos_pc98_6.20_md2dd.img.lz", "msdos_pc98_6.20_md2hd.img.lz",

            // GEOS 1.2
            "geos12_md2hd.img.lz",

            // GEOS 2.0
            "geos20_mf2hd.img.lz",

            // GEOS 3.1
            "geos31_mf2hd.img.lz",

            // GEOS 3.2
            "geos32_mf2hd.img.lz",

            // GEOS 4.1
            "geos41_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
        {
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
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // DR-DOS 7.03
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // DR-DOS 8.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.30A
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.31
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 4.01
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 5.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 6.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 6.20
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 6.20 RC1
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 6.21
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 6.22
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 7.10
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.20 for Amstrad
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 2.11 for AT&T
            MediaType.DOS_525_DS_DD_9,

            // MS-DOS 3.30 for DeLL
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.10 for Epson
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD,

            // MS-DOS 3.20 for Epson
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.20 for HP
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.21 for Hyosung
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.21 for Kaypro
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 3.10 for Olivetti
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_525_SS_DD_9,

            // MS-DOS 3.30 for Toshiba
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // MS-DOS 4.01 for Toshiba
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // Novell DOS 7.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // OpenDOS 7.01
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // PC-DOS 2.00
            MediaType.DOS_525_DS_DD_9,

            // PC-DOS 2.10
            MediaType.DOS_525_DS_DD_9,

            // PC-DOS 2000
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // PC-DOS 3.00
            MediaType.DOS_525_HD,

            // PC-DOS 3.10
            MediaType.DOS_525_HD,

            // PC-DOS 3.30
            MediaType.DOS_525_HD, MediaType.DOS_35_HD,

            // PC-DOS 4.00
            MediaType.DOS_525_HD, MediaType.DOS_35_HD,

            // PC-DOS 5.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // PC-DOS 5.02
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // PC-DOS 6.10
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // PC-DOS 6.30
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9, MediaType.DOS_525_SS_DD_8,

            // mkfs.vfat
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // mkfs.vfat for Atari
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // Microsoft OS/2 1.00 for Tandy
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // Microsoft OS/2 1.10 for AST
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // Microsoft OS/2 1.10 for Nokia
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // Microsoft OS/2 1.21
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // Microsoft OS/2 1.30.1
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // OS/2 1.20
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // OS/2 1.30
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // OS/2 2.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // OS/2 2.10
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // OS/2 2.11
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // OS/2 Warp 3
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // OS/2 Warp 4
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // eComStation
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,
            MediaType.DOS_35_HD,

            // Windows 95
            MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,

            // Windows 95 OSR 2
            MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,

            // Windows 95 OSR 2.1
            MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,

            // Windows 95 OSR 2.5
            MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,

            // Windows 98
            MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,

            // Windows 98 Second Edition
            MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,

            // Windows Me
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // Windows NT 3.10
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // Windows NT 3.50
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // Windows NT 3.51
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // Windows NT 4.00
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD, MediaType.DOS_525_SS_DD_9,

            // Windows 2000
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // Windows Vista
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
            MediaType.DOS_35_HD,

            // BeOS R4.5
            MediaType.DOS_35_HD,

            // Hatari
            MediaType.DOS_35_SS_DD_9, MediaType.ATARI_35_SS_DD, MediaType.ATARI_35_SS_DD_11, MediaType.DOS_35_DS_DD_9,
            MediaType.ATARI_35_DS_DD, MediaType.ATARI_35_DS_DD_11, MediaType.DOS_35_ED, MediaType.DOS_35_HD,

            // Atari TOS 1.04
            MediaType.DOS_35_SS_DD_9, MediaType.DOS_35_DS_DD_9,

            // NetBSD 1.6
            MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // NeXTStep 3.3
            MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // OpenStep for Mach 4.0
            MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // OpenStep for Mach 4.2
            MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // Solaris 2.4
            MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // COHERENT UNIX 4.2.10
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // SCO OpenServer 5.0.7Hw
            MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,

            // Epson MS-DOS 5.00 for PC-98
            MediaType.DOS_35_DS_DD_9, MediaType.NEC_525_HD,

            // NEC MS-DOS 3.30 for PC-98
            MediaType.DOS_35_DS_DD_9, MediaType.NEC_525_HD,

            // NEC MS-DOS 5.00 for PC-98
            MediaType.DOS_35_DS_DD_9, MediaType.NEC_525_HD,

            // NEC MS-DOS 6.20 for PC-98
            MediaType.DOS_35_DS_DD_9, MediaType.NEC_525_HD,

            // GEOS 1.2
            MediaType.DOS_525_HD,

            // GEOS 2.0
            MediaType.DOS_35_HD,

            // GEOS 3.1
            MediaType.DOS_35_HD,

            // GEOS 3.2
            MediaType.DOS_35_HD,

            // GEOS 4.1
            MediaType.DOS_35_HD
        };

        readonly ulong[] sectors =
        {
            // Concurrent DOS 6.00
            2400, 1440, 2880,

            // DR-DOS 3.40
            720, 640, 2400, 1440, 2880, 360, 320,

            // DR-DOS 3.41
            720, 640, 2400, 1440, 2880, 360, 320,

            // DR-DOS 5.00
            720, 640, 2400, 1440, 2880, 360, 320,

            // DR-DOS 6.00
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // DR-DOS 7.02
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // DR-DOS 7.03
            720, 640, 2400, 1440, 5760, 2880,

            // DR-DOS 8.00
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 3.30A
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 3.31
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 4.01
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 5.00
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 6.00
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 6.20
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 6.20 RC1
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 6.21
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 6.22
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 7.10
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // MS-DOS 3.20 for Amstrad
            720, 640, 2400, 1440, 360, 320,

            // MS-DOS 2.11 for AT&T
            720,

            // MS-DOS 3.30 for DeLL
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 3.10 for Epson
            720, 640, 2400,

            // MS-DOS 3.20 for Epson
            720, 640, 2400, 1440, 360, 320,

            // MS-DOS 3.20 for HP
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 3.21 for Hyosung
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 3.21 for Kaypro
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 3.10 for Olivetti
            720, 2400, 360,

            // MS-DOS 3.30 for Toshiba
            720, 640, 2400, 1440, 2880, 360, 320,

            // MS-DOS 4.01 for Toshiba
            720, 640, 2400, 1440, 2880, 360, 320,

            // Novell DOS 7.00
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // OpenDOS 7.01
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // PC-DOS 2.00
            720,

            // PC-DOS 2.10
            720,

            // PC-DOS 2000
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // PC-DOS 3.00
            2400,

            // PC-DOS 3.10
            2400,

            // PC-DOS 3.30
            2400, 2880,

            // PC-DOS 4.00
            2400, 2880,

            // PC-DOS 5.00
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // PC-DOS 5.02
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // PC-DOS 6.10
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // PC-DOS 6.30
            720, 640, 2400, 1440, 5760, 2880, 360, 320,

            // mkfs.vfat
            2400, 1440, 5760, 2880,

            // mkfs.vfat for Atari
            2400, 1440, 5760, 2880,

            // Microsoft OS/2 1.00 for Tandy
            720, 2400, 1440, 2880,

            // Microsoft OS/2 1.10 for AST
            720, 2400, 1440, 2880,

            // Microsoft OS/2 1.10 for Nokia
            720, 2400, 1440, 2880,

            // Microsoft OS/2 1.21
            720, 2400, 1440, 2880,

            // Microsoft OS/2 1.30.1
            720, 2400, 1440, 5760, 2880,

            // OS/2 1.20
            720, 2400, 1440, 2880,

            // OS/2 1.30
            720, 2400, 1440, 2880,

            // OS/2 2.00
            720, 2400, 1440, 5760, 2880,

            // OS/2 2.10
            720, 2400, 1440, 5760, 2880,

            // OS/2 2.11
            720, 2400, 1440, 5760, 2880,

            // OS/2 Warp 3
            2400, 1440, 5760, 2880,

            // OS/2 Warp 4
            2400, 1440, 5760, 2880,

            // eComStation
            2400, 1440, 5760, 2880, 2880,

            // Windows 95
            640, 720, 2400, 1440, 5760, 2880, 320, 360,

            // Windows 95 OSR 2
            640, 720, 2400, 1440, 5760, 2880, 320, 360,

            // Windows 95 OSR 2.1
            640, 720, 2400, 1440, 5760, 2880, 320, 360,

            // Windows 95 OSR 2.5
            640, 720, 2400, 1440, 5760, 2880, 320, 360,

            // Windows 98
            640, 720, 2400, 1440, 5760, 2880, 320, 360,

            // Windows 98 Second Edition
            640, 720, 2400, 1440, 5760, 2880, 320, 360,

            // Windows Me
            720, 2400, 1440, 5760, 2880,

            // Windows NT 3.10
            2400, 1440, 5760, 2880,

            // Windows NT 3.50
            2400, 1440, 5760, 2880,

            // Windows NT 3.51
            2400, 1440, 5760, 2880,

            // Windows NT 4.00
            720, 2400, 1440, 5760, 2880, 360,

            // Windows 2000
            720, 2400, 1440, 5760, 2880,

            // Windows Vista
            720, 2400, 1440, 5760, 2880,

            // BeOS R4.5
            2880,

            // Hatari
            720, 800, 880, 1440, 1600, 1760, 5760, 2880,

            // Atari TOS 1.04
            720, 1440,

            // NetBSD 1.6
            1440, 2880,

            // NeXTStep 3.3
            1440, 2880,

            // OpenStep for Mach 4.0
            1440, 2880,

            // OpenStep for Mach 4.2
            1440, 2880,

            // Solaris 2.4
            1440, 2880,

            // COHERENT UNIX 4.2.10
            720, 2400, 1440, 2880,

            // SCO OpenServer 5.0.7Hw
            2400, 1440, 2880,

            // Epson MS-DOS 5.00 for PC-98
            1440, 1232,

            // NEC MS-DOS 3.30 for PC-98
            1440, 1232,

            // NEC MS-DOS 5.00 for PC-98
            1440, 1232,

            // NEC MS-DOS 6.20 for PC-98
            1440, 1232,

            // GEOS 1.2
            2400,

            // GEOS 2.0
            2880,

            // GEOS 3.1
            2880,

            // GEOS 3.2
            2880,

            // GEOS 4.1
            2880
        };

        readonly uint[] sectorsize =
        {
            // Concurrent DOS 6.00
            512, 512, 512,

            // DR-DOS 3.40
            512, 512, 512, 512, 512, 512, 512,

            // DR-DOS 3.41
            512, 512, 512, 512, 512, 512, 512,

            // DR-DOS 5.00
            512, 512, 512, 512, 512, 512, 512,

            // DR-DOS 6.00
            512, 512, 512, 512, 512, 512, 512, 512,

            // DR-DOS 7.02
            512, 512, 512, 512, 512, 512, 512, 512,

            // DR-DOS 7.03
            512, 512, 512, 512, 512, 512,

            // DR-DOS 8.00
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.30A
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.31
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 4.01
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 5.00
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 6.00
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 6.20
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 6.20 RC1
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 6.21
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 6.22
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 7.10
            512, 512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.20 for Amstrad
            512, 512, 512, 512, 512, 512,

            // MS-DOS 2.11 for AT&T
            512,

            // MS-DOS 3.30 for DeLL
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.10 for Epson
            512, 512, 512,

            // MS-DOS 3.20 for Epson
            512, 512, 512, 512, 512, 512,

            // MS-DOS 3.20 for HP
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.21 for Hyosung
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.21 for Kaypro
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 3.10 for Olivetti
            512, 512, 512,

            // MS-DOS 3.30 for Toshiba
            512, 512, 512, 512, 512, 512, 512,

            // MS-DOS 4.01 for Toshiba
            512, 512, 512, 512, 512, 512, 512,

            // Novell DOS 7.00
            512, 512, 512, 512, 512, 512, 512, 512,

            // OpenDOS 7.01
            512, 512, 512, 512, 512, 512, 512, 512,

            // PC-DOS 2.00
            512,

            // PC-DOS 2.10
            512,

            // PC-DOS 2000
            512, 512, 512, 512, 512, 512, 512, 512,

            // PC-DOS 3.00
            512,

            // PC-DOS 3.10
            512,

            // PC-DOS 3.30
            512, 512,

            // PC-DOS 4.00
            512, 512,

            // PC-DOS 5.00
            512, 512, 512, 512, 512, 512, 512, 512,

            // PC-DOS 5.02
            512, 512, 512, 512, 512, 512, 512, 512,

            // PC-DOS 6.10
            512, 512, 512, 512, 512, 512, 512, 512,

            // PC-DOS 6.30
            512, 512, 512, 512, 512, 512, 512, 512,

            // mkfs.vfat
            512, 512, 512, 512,

            // mkfs.vfat for Atari
            512, 512, 512, 512,

            // Microsoft OS/2 1.00 for Tandy
            512, 512, 512, 512,

            // Microsoft OS/2 1.10 for AST
            512, 512, 512, 512,

            // Microsoft OS/2 1.10 for Nokia
            512, 512, 512, 512,

            // Microsoft OS/2 1.21
            512, 512, 512, 512,

            // Microsoft OS/2 1.30.1
            512, 512, 512, 512, 512,

            // OS/2 1.20
            512, 512, 512, 512,

            // OS/2 1.30
            512, 512, 512, 512,

            // OS/2 2.00
            512, 512, 512, 512, 512,

            // OS/2 2.10
            512, 512, 512, 512, 512,

            // OS/2 2.11
            512, 512, 512, 512, 512,

            // OS/2 Warp 3
            512, 512, 512, 512,

            // OS/2 Warp 4
            512, 512, 512, 512,

            // eComStation
            512, 512, 512, 512, 512,

            // Windows 95
            512, 512, 512, 512, 512, 512, 512, 512,

            // Windows 95 OSR 2
            512, 512, 512, 512, 512, 512, 512, 512,

            // Windows 95 OSR 2.1
            512, 512, 512, 512, 512, 512, 512, 512,

            // Windows 95 OSR 2.5
            512, 512, 512, 512, 512, 512, 512, 512,

            // Windows 98
            512, 512, 512, 512, 512, 512, 512, 512,

            // Windows 98 Second Edition
            512, 512, 512, 512, 512, 512, 512, 512,

            // Windows Me
            512, 512, 512, 512, 512,

            // Windows NT 3.10
            512, 512, 512, 512,

            // Windows NT 3.50
            512, 512, 512, 512,

            // Windows NT 3.51
            512, 512, 512, 512,

            // Windows NT 4.00
            512, 512, 512, 512, 512, 512,

            // Windows 2000
            512, 512, 512, 512, 512,

            // Windows Vista
            512, 512, 512, 512, 512,

            // BeOS R4.5
            512,

            // Hatari
            512, 512, 512, 512, 512, 512, 512, 512,

            // Atari TOS 1.04
            512, 512,

            // NetBSD 1.6
            512, 512,

            // NeXTStep 3.3
            512, 512,

            // OpenStep for Mach 4.0
            512, 512,

            // OpenStep for Mach 4.2
            512, 512,

            // Solaris 2.4
            512, 512,

            // COHERENT UNIX 4.2.10
            512, 512, 512, 512,

            // SCO OpenServer 5.0.7Hw
            512, 512, 512,

            // Epson MS-DOS 5.00 for PC-98
            512, 1024,

            // NEC MS-DOS 3.30 for PC-98
            512, 1024,

            // NEC MS-DOS 5.00 for PC-98
            512, 1024,

            // NEC MS-DOS 6.20 for PC-98
            512, 1024,

            // GEOS 1.2
            512,

            // GEOS 2.0
            512,

            // GEOS 3.1
            512,

            // GEOS 3.2
            512,

            // GEOS 4.1
            512
        };

        readonly long[] clusters =
        {
            // Concurrent DOS 6.00
            2400, 720, 2880,

            // DR-DOS 3.40
            360, 320, 2400, 720, 2880, 360, 320,

            // DR-DOS 3.41
            360, 320, 2400, 720, 2880, 360, 320,

            // DR-DOS 5.00
            360, 320, 2400, 720, 2880, 360, 320,

            // DR-DOS 6.00
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // DR-DOS 7.02
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // DR-DOS 7.03
            360, 320, 2400, 720, 2880, 2880,

            // DR-DOS 8.00
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 3.30A
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 3.31
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 4.01
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 5.00
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 6.00
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 6.20
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 6.20 RC1
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 6.21
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 6.22
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 7.10
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // MS-DOS 3.20 for Amstrad
            360, 320, 2400, 720, 360, 320,

            // MS-DOS 2.11 for AT&T
            360,

            // MS-DOS 3.30 for DeLL
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 3.10 for Epson
            360, 320, 2400,

            // MS-DOS 3.20 for Epson
            360, 320, 2400, 720, 360, 320,

            // MS-DOS 3.20 for HP
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 3.21 for Hyosung
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 3.21 for Kaypro
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 3.10 for Olivetti
            360, 2400, 360,

            // MS-DOS 3.30 for Toshiba
            360, 320, 2400, 720, 2880, 360, 320,

            // MS-DOS 4.01 for Toshiba
            360, 320, 2400, 720, 2880, 360, 320,

            // Novell DOS 7.00
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // OpenDOS 7.01
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // PC-DOS 2.00
            360,

            // PC-DOS 2.10
            360,

            // PC-DOS 2000
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // PC-DOS 3.00
            2400,

            // PC-DOS 3.10
            2400,

            // PC-DOS 3.30
            2400, 2880,

            // PC-DOS 4.00
            2400, 2880,

            // PC-DOS 5.00
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // PC-DOS 5.02
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // PC-DOS 6.10
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // PC-DOS 6.30
            360, 320, 2400, 720, 2880, 2880, 360, 320,

            // mkfs.vfat
            2400, 720, 2880, 2880,

            // mkfs.vfat for Atari
            1200, 720, 2880, 1440,

            // Microsoft OS/2 1.00 for Tandy
            360, 2400, 720, 2880,

            // Microsoft OS/2 1.10 for AST
            360, 2400, 720, 2880,

            // Microsoft OS/2 1.10 for Nokia
            360, 2400, 720, 2880,

            // Microsoft OS/2 1.21
            360, 2400, 720, 2880,

            // Microsoft OS/2 1.30.1
            360, 2400, 720, 2880, 2880,

            // OS/2 1.20
            360, 2400, 720, 2880,

            // OS/2 1.30
            360, 2400, 720, 2880,

            // OS/2 2.00
            360, 2400, 720, 2880, 2880,

            // OS/2 2.10
            360, 2400, 720, 2880, 2880,

            // OS/2 2.11
            360, 2400, 720, 2880, 2880,

            // OS/2 Warp 3
            2400, 720, 2880, 2880,

            // OS/2 Warp 4
            2400, 720, 2880, 2880,

            // eComStation
            2400, 720, 2880, 2880, 2880,

            // Windows 95
            320, 360, 2400, 720, 2880, 2880, 320, 360,

            // Windows 95 OSR 2
            320, 360, 2400, 720, 2880, 2880, 320, 360,

            // Windows 95 OSR 2.1
            320, 360, 2400, 720, 2880, 2880, 320, 360,

            // Windows 95 OSR 2.5
            320, 360, 2400, 720, 2880, 2880, 320, 360,

            // Windows 98
            320, 360, 2400, 720, 2880, 2880, 320, 360,

            // Windows 98 Second Edition
            320, 360, 2400, 720, 2880, 2880, 320, 360,

            // Windows Me
            360, 2400, 720, 2880, 2880,

            // Windows NT 3.10
            2400, 720, 2880, 2880,

            // Windows NT 3.50
            2400, 720, 2880, 2880,

            // Windows NT 3.51
            2400, 720, 2880, 2880,

            // Windows NT 4.00
            360, 2400, 720, 2880, 2880, 360,

            // Windows 2000
            360, 2400, 720, 2880, 2880,

            // Windows Vista
            360, 2400, 720, 2880, 2880,

            // BeOS R4.5
            2880,

            // Hatari
            360, 400, 440, 720, 800, 880, 2880, 1440,

            // Atari TOS 1.04
            360, 720,

            // NetBSD 1.6
            720, 2880,

            // NeXTStep 3.3
            720, 2880,

            // OpenStep for Mach 4.0
            720, 2880,

            // OpenStep for Mach 4.2
            720, 2880,

            // Solaris 2.4
            720, 2880,

            // COHERENT UNIX 4.2.10
            360, 2400, 720, 2880,

            // SCO OpenServer 5.0.7Hw
            2400, 1440, 2880,

            // Epson MS-DOS 5.00 for PC-98
            640, 1232,

            // NEC MS-DOS 3.30 for PC-98
            640, 1232,

            // NEC MS-DOS 5.00 for PC-98
            640, 1232,

            // NEC MS-DOS 6.20 for PC-98
            640, 1232,

            // GEOS 1.2
            2400,

            // GEOS 2.0
            2880,

            // GEOS 3.1
            2880,

            // GEOS 3.2
            2880,

            // GEOS 4.1
            2880
        };

        readonly int[] clustersize =
        {
            // Concurrent DOS 6.00
            512, 1024, 512,

            // DR-DOS 3.40
            1024, 1024, 512, 1024, 512, 512, 512,

            // DR-DOS 3.41
            1024, 1024, 512, 1024, 512, 512, 512,

            // DR-DOS 5.00
            1024, 1024, 512, 1024, 512, 512, 512,

            // DR-DOS 6.00
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // DR-DOS 7.02
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // DR-DOS 7.03
            1024, 1024, 512, 1024, 1024, 512,

            // DR-DOS 8.00
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 3.30A
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 3.31
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 4.01
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 5.00
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 6.00
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 6.20
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 6.20 RC1
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 6.21
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 6.22
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 7.10
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // MS-DOS 3.20 for Amstrad
            1024, 1024, 512, 1024, 512, 512,

            // MS-DOS 2.11 for AT&T
            1024,

            // MS-DOS 3.30 for DeLL
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 3.10 for Epson
            1024, 1024, 512,

            // MS-DOS 3.20 for Epson
            1024, 1024, 512, 1024, 512, 512,

            // MS-DOS 3.20 for HP
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 3.21 for Hyosung
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 3.21 for Kaypro
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 3.10 for Olivetti
            1024, 512, 512,

            // MS-DOS 3.30 for Toshiba
            1024, 1024, 512, 1024, 512, 512, 512,

            // MS-DOS 4.01 for Toshiba
            1024, 1024, 512, 1024, 512, 512, 512,

            // Novell DOS 7.00
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // OpenDOS 7.01
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // PC-DOS 2.00
            1024,

            // PC-DOS 2.10
            1024,

            // PC-DOS 2000
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // PC-DOS 3.00
            512,

            // PC-DOS 3.10
            512,

            // PC-DOS 3.30
            512, 512,

            // PC-DOS 4.00
            512, 512,

            // PC-DOS 5.00
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // PC-DOS 5.02
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // PC-DOS 6.10
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // PC-DOS 6.30
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // mkfs.vfat
            512, 1024, 1024, 512,

            // mkfs.vfat for Atari
            1024, 1024, 1024, 1024,

            // Microsoft OS/2 1.00 for Tandy
            1024, 512, 1024, 512,

            // Microsoft OS/2 1.10 for AST
            1024, 512, 1024, 512,

            // Microsoft OS/2 1.10 for Nokia
            1024, 512, 1024, 512,

            // Microsoft OS/2 1.21
            1024, 512, 1024, 512,

            // Microsoft OS/2 1.30.1
            1024, 512, 1024, 1024, 512,

            // OS/2 1.20
            1024, 512, 1024, 512,

            // OS/2 1.30
            1024, 512, 1024, 512,

            // OS/2 2.00
            1024, 512, 1024, 1024, 512,

            // OS/2 2.10
            1024, 512, 1024, 1024, 512,

            // OS/2 2.11
            1024, 512, 1024, 1024, 512,

            // OS/2 Warp 3
            512, 1024, 1024, 512,

            // OS/2 Warp 4
            512, 1024, 1024, 512,

            // eComStation
            512, 1024, 1024, 512, 512,

            // Windows 95
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // Windows 95 OSR 2
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // Windows 95 OSR 2.1
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // Windows 95 OSR 2.5
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // Windows 98
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // Windows 98 Second Edition
            1024, 1024, 512, 1024, 1024, 512, 512, 512,

            // Windows Me
            1024, 512, 1024, 1024, 512,

            // Windows NT 3.10
            512, 1024, 1024, 512,

            // Windows NT 3.50
            512, 1024, 1024, 512,

            // Windows NT 3.51
            512, 1024, 1024, 512,

            // Windows NT 4.00
            1024, 512, 1024, 512, 512, 512,

            // Windows 2000
            1024, 512, 1024, 1024, 512,

            // Windows Vista
            1024, 512, 1024, 1024, 512,

            // BeOS R4.5
            512,

            // Hatari
            1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024,

            // Atari TOS 1.04
            1024, 1024,

            // NetBSD 1.6
            1024, 512,

            // NeXTStep 3.3
            1024, 512,

            // OpenStep for Mach 4.0
            1024, 512,

            // OpenStep for Mach 4.2
            1024, 512,

            // Solaris 2.4
            1024, 512,

            // COHERENT UNIX 4.2.10
            1024, 512, 1024, 512,

            // SCO OpenServer 5.0.7Hw
            512, 512, 512,

            // Epson MS-DOS 5.00 for PC-98
            1024, 1024,

            // NEC MS-DOS 3.30 for PC-98
            1024, 1024,

            // NEC MS-DOS 5.00 for PC-98
            1024, 1024,

            // NEC MS-DOS 6.20 for PC-98
            1024, 1024,

            // GEOS 1.2
            512,

            // GEOS 2.0
            512,

            // GEOS 3.1
            512,

            // GEOS 3.2
            512,

            // GEOS 4.1
            512
        };

        readonly string[] volumename =
        {
            // Concurrent DOS 6.00
            null, null, null,

            // DR-DOS 3.40
            null, null, null, null, null, null, null,

            // DR-DOS 3.41
            null, null, null, null, null, null, null,

            // DR-DOS 5.00
            null, null, null, null, null, null, null,

            // DR-DOS 6.00
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL",

            // DR-DOS 7.02
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL",

            // DR-DOS 7.03
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // DR-DOS 8.00
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL",

            // MS-DOS 3.30A
            null, null, null, null, null, null, null,

            // MS-DOS 3.31
            null, null, null, null, null, null, null,

            // MS-DOS 4.01
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 5.00
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 6.00
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 6.20
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 6.20 RC1
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 6.21
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 6.22
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 7.10
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // MS-DOS 3.20 for Amstrad
            null, null, null, null, null, null,

            // MS-DOS 2.11 for AT&T
            null,

            // MS-DOS 3.30 for DeLL
            null, null, null, null, null, null, null,

            // MS-DOS 3.10 for Epson
            null, null, null,

            // MS-DOS 3.20 for Epson
            null, null, null, null, null, null,

            // MS-DOS 3.20 for HP
            null, null, null, null, null, null, null,

            // MS-DOS 3.21 for Hyosung
            null, null, null, null, null, null, null,

            // MS-DOS 3.21 for Kaypro
            null, null, null, null, null, null, null,

            // MS-DOS 3.10 for Olivetti
            null, null, null,

            // MS-DOS 3.30 for Toshiba
            null, null, null, null, null, null, null,

            // MS-DOS 4.01 for Toshiba
            "VOLUMELABEL", "NO NAME    ", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "NO NAME    ",

            // Novell DOS 7.00
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL",

            // OpenDOS 7.01
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL",

            // PC-DOS 2.00
            null,

            // PC-DOS 2.10
            null,

            // PC-DOS 2000
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // PC-DOS 3.00
            null,

            // PC-DOS 3.10
            null,

            // PC-DOS 3.30
            null, null,

            // PC-DOS 4.00
            "VOLUMELABEL", "VOLUMELABEL",

            // PC-DOS 5.00
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // PC-DOS 5.02
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // PC-DOS 6.10
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // PC-DOS 6.30
            "VOLUMELABEL", null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null,

            // mkfs.vfat
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // mkfs.vfat for Atari
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Microsoft OS/2 1.00 for Tandy
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Microsoft OS/2 1.10 for AST
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Microsoft OS/2 1.10 for Nokia
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Microsoft OS/2 1.21
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Microsoft OS/2 1.30.1
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 1.20
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 1.30
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 2.00
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 2.10
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 2.11
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 Warp 3
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // OS/2 Warp 4
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // eComStation
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows 95
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, "VOLUMELABEL",

            // Windows 95 OSR 2
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, "VOLUMELABEL",

            // Windows 95 OSR 2.1
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, "VOLUMELABEL",

            // Windows 95 OSR 2.5
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, "VOLUMELABEL",

            // Windows 98
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, "VOLUMELABEL",

            // Windows 98 Second Edition
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, "VOLUMELABEL",

            // Windows Me
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows NT 3.10
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows NT 3.50
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows NT 3.51
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows NT 4.00
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows 2000
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // Windows Vista
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // BeOS R4.5
            "VOLUMELABEL",

            // Hatari
            "volumelabel", "volumelabel", "volumelabel", "volumelabel", "volumelabel", "volumelabel", "volumelabel",
            "volumelabel",

            // Atari TOS 1.04
            "VOLUMELABEL", "VOLUMELABEL",

            // NetBSD 1.6
            "VOLUMELABEL", "VOLUMELABEL",

            // NeXTStep 3.3
            "VOLUMELABEL", "VOLUME LABE",

            // OpenStep for Mach 4.0
            "VOLUMELABEL", "VOLUMELABEL",

            // OpenStep for Mach 4.2
            "VOLUMELABEL", "VOLUMELABEL",

            // Solaris 2.4
            null, null,

            // COHERENT UNIX 4.2.10
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",

            // SCO OpenServer 5.0.7Hw
            null, null, null,

            // Epson MS-DOS 5.00 for PC-98
            "NO NAME    ", "NO NAME    ",

            // NEC MS-DOS 3.30 for PC-98
            null, null,

            // NEC MS-DOS 5.00 for PC-98
            "NO NAME    ", "NO NAME    ",

            // NEC MS-DOS 6.20 for PC-98
            "NO NAME    ", "NO NAME    ",

            // GEOS 1.2
            "GEOS12",

            // GEOS 2.0
            "GEOS20",

            // GEOS 3.1
            "GEOS32",

            // GEOS 3.2
            "NDO2000",

            // GEOS 4.1
            "GEOS41"
        };

        readonly string[] volumeserial =
        {
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
            "1BF63C69", "1BF70E75", "1BF7185F", "1BF80C4F", "1BF90F1D", "1BF82777", "1BF72430", "1BF72F1E",

            // DR-DOS 7.03
            "0C1A2013", "0CE22B5B", "0CEA1D3E", "0CEE102F", "0CEE3760", "0CEF2739",

            // DR-DOS 8.00
            "1BFD1977", "1BFD2D3F", "1BFD3531", "1BFC3231", "1BFA1D58", "1BFC117D", "1BFE0971", "1BFE1423",

            // MS-DOS 3.30A
            null, null, null, null, null, null, null,

            // MS-DOS 3.31
            null, null, null, null, null, null, null,

            // MS-DOS 4.01
            "122C190A", null, "2480190A", "2D471909", "0F5A1908", "2F3D190A", null,

            // MS-DOS 5.00
            "0B6018F8", null, "1E3518F8", "285A18FB", "231D18FE", "415118FC", "316118F8", null,

            // MS-DOS 6.00
            "067B18F6", null, "193418F6", "1F3A18F5", "165318F3", "172418F4", "234918F6", null,

            // MS-DOS 6.20
            "265418ED", null, "0B7018EE", "127418F0", "137F18F2", "364C18F0", "185C18EE", null,

            // MS-DOS 6.20 RC1
            "064B18EB", null, "192518EB", "244C18EA", "3C3118E7", "344118E9", "267E18EB", null,

            // MS-DOS 6.21
            "2A41181B", null, "0641181C", "3B26181C", "082518E2", "237118E1", "123F181C", null,

            // MS-DOS 6.22
            "317C1818", null, "0D3A1819", "3C251817", "387A1815", "185E1817", "18231819", null,

            // MS-DOS 7.10
            "1156180A", null, "2951180A", "3057180B", "2B4A1811", "344B180C", "352D180A", null,

            // MS-DOS 3.20 for Amstrad
            null, null, null, null, null, null,

            // MS-DOS 2.11 for AT&T
            null,

            // MS-DOS 3.30 for DeLL
            null, null, null, null, null, null, null,

            // MS-DOS 3.10 for Epson
            null, null, null,

            // MS-DOS 3.20 for Epson
            null, null, null, null, null, null,

            // MS-DOS 3.20 for HP
            null, null, null, null, null, null, null,

            // MS-DOS 3.21 for Hyosung
            null, null, null, null, null, null, null,

            // MS-DOS 3.21 for Kaypro
            null, null, null, null, null, null, null,

            // MS-DOS 3.10 for Olivetti
            null, null, null,

            // MS-DOS 3.30 for Toshiba
            null, null, null, null, null, null, null,

            // MS-DOS 4.01 for Toshiba
            "0B2519E7", "163419E7", "1E3119E7", "133919E9", "177419EA", "317E19E7", "3B7319E7",

            // Novell DOS 7.00
            "1BE7254C", "1BE73024", "1BE7397C", "1BE63635", "1BE51661", "1BE61143", "1BE80A5D", "1BE8144C",

            // OpenDOS 7.01
            "1BE93E2B", "1BEA234D", "1BEA325D", "1BEB294F", "1BEC2C2E", "1BEC0C5D", "1BEA3E60", "1BEB0E26",

            // PC-DOS 2.00
            null,

            // PC-DOS 2.10
            null,

            // PC-DOS 2000
            "2634100E", null, "3565100E", "3B6B1012", "3B2D1013", "1D491013", "4136100E", null,

            // PC-DOS 3.00
            null,

            // PC-DOS 3.10
            null,

            // PC-DOS 3.30
            null, null,

            // PC-DOS 4.00
            "3C240FE3", "2E3E0FE1",

            // PC-DOS 5.00
            "33260FF9", null, "11550FFA", "234F0FFB", "2F600FFC", "0D550FFC", "1D630FFA", null,

            // PC-DOS 5.02
            "06231000", null, "1A3E1000", "1F3B0FFF", "3D750FFD", "3F4F0FFE", "26471000", null,

            // PC-DOS 6.10
            "25551004", null, "3E5F1004", "142D1006", "17541007", "355A1006", "0D5E1005", null,

            // PC-DOS 6.30
            "2B22100C", null, "3B47100C", "0C55100C", "1B80100A", "0B59100B", "0A3A100D", null,

            // mkfs.vfat
            "20C279B1", "20FD9501", "2132D70A", "2118F1AA",

            // mkfs.vfat for Atari
            "83E030", "C53F06", "A154CD", "D54DEE",

            // Microsoft OS/2 1.00 for Tandy
            "9C170C15", "9BFB0C15", "9C13FC15", "9BF99C15",

            // Microsoft OS/2 1.10 for AST
            "66A42C15", "67696C15", "66DEBC15", "66DC4C15",

            // Microsoft OS/2 1.10 for Nokia
            "676B4C15", "67768C15", "9C12DC15", "66A74C15",

            // Microsoft OS/2 1.21
            "9C074C15", "66BCFC15", "66C1AC15", "66C7FC15",

            // Microsoft OS/2 1.30.1
            "66C47C15", "66CBEC15", "9C167C15", "9C147C15", "9C0FEC15",

            // OS/2 1.20
            "5BF5E015", "5BE61015", "5C26F015", "5C376015",

            // OS/2 1.30
            "5C418015", "5BE20015", "5C7F1015", "5B83C015",

            // OS/2 2.00
            "5C3BD015", "5B807015", "5BE69015", "5C187015", "5C390015",

            // OS/2 2.10
            "1BFCB414", "E6C6C414", "E6CCF414", "E6AF6414", "1C005414",

            // OS/2 2.11
            "E6AEB414", "1C00D414", "1C03B414", "E6C90414", "E6B6E414",

            // OS/2 Warp 3
            "E6AF7414", "E6D63414", "E6A65414", "E6AE6414",

            // OS/2 Warp 4
            "E6CD9414", "1BFAD414", "E6DFF414", "E6D4C414",

            // eComStation
            "E6CA5814", "E6CBC814", "E6B81814", "1C013814", "9BF37814",

            // Windows 95
            null, "3B360D0D", "24240D0D", "3C260D11", "30050D10", "275A0D11", null, "3B100D0F",

            // Windows 95 OSR 2
            null, "1C5B0D19", "11510D19", "0F1F0D15", "40200D17", "3D610D14", null, "280B0D19",

            // Windows 95 OSR 2.1
            null, "1F3B0D1C", "14470D1C", "1C510DE4", "2E250DE2", "10640DE4", null, "2B3E0D1C",

            // Windows 95 OSR 2.5
            null, "18190DFB", "0A240DFB", "1E320DE7", "33230DE8", "125B0DE7", null, "21410DFB",

            // Windows 98
            null, "40090E0F", "28140E0F", "0E620E0A", "14390E0D", "0E081246", null, "30600E10",

            // Windows 98 Second Edition
            null, "1B550EEC", "1B100EEB", "08410EE6", "0E0F0EE8", "325D0EE4", null, "13380EEC",

            // Windows Me
            "2F200F02", "103A0F01", "2F1C0EFC", "21570EFF", "07040EFB",

            // Windows NT 3.10
            "60EA50BC", "6C857D51", "4009440C", "30761EDC",

            // Windows NT 3.50
            "0C478404", "7CBEB35B", "7C1E8DCB", "ECB276AF",

            // Windows NT 3.51
            "482D8681", "8889C95E", "54DE6C39", "F47D2516",

            // Windows NT 4.00
            "D8CAAC1F", "E0BB6D70", "C08C3C60", "9C44B411", "4C7DD099", "4CD82982",

            // Windows 2000
            "4019989C", "78F30AF8", "E4217DDE", "80B3B996", "28043527",

            // Windows Vista
            "3C9F0BD2", "3A8E465C", "B2EFB822", "3C30C632", "16DAB07A",

            // BeOS R4.5
            "00000000",

            // Hatari
            "A82270", "D08917", "37AD91", "1ED910", "299DFE", "94AE59", "3A1757", "C08249",

            // Atari TOS 1.04
            "2356F0", "51C7A3",

            // NetBSD 1.6
            "EEB51A0C", "CCFD1A06",

            // NeXTStep 3.3
            null, null,

            // OpenStep for Mach 4.0
            null, null,

            // OpenStep for Mach 4.2
            null, null,

            // Solaris 2.4
            null, null,

            // COHERENT UNIX 4.2.10
            null, null, null, null,

            // SCO OpenServer 5.0.7Hw
            null, null, null,

            // Epson MS-DOS 5.00 for PC-98
            "27021316", "11021317",

            // NEC MS-DOS 3.30 for PC-98
            null, null,

            // NEC MS-DOS 5.00 for PC-98
            "1002120E", "41021209",

            // NEC MS-DOS 6.20 for PC-98
            "3D021418", "16021409",

            // GEOS 1.2
            "0000049C",

            // GEOS 2.0
            "8DC94C67",

            // GEOS 3.1
            "8E0D4C67",

            // GEOS 3.2
            "8EDB4C67",

            // GEOS 4.1
            "8D684C67"
        };

        readonly string[] oemid =
        {
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
            "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7",

            // DR-DOS 7.03
            "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7",

            // DR-DOS 8.00
            "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7", "DRDOS  7",

            // MS-DOS 3.30A
            "MSDOS3.3", null, "MSDOS3.3", "MSDOS3.3", "MSDOS3.3", "MSDOS3.3", null,

            // MS-DOS 3.31
            "IBM  3.3", null, "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", null,

            // MS-DOS 4.01
            "MSDOS4.0", null, "MSDOS4.0", "MSDOS4.0", "MSDOS4.0", "MSDOS4.0", null,

            // MS-DOS 5.00
            "MSDOS5.0", null, "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", null,

            // MS-DOS 6.00
            "MSDOS5.0", null, "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", null,

            // MS-DOS 6.20
            "MSDOS5.0", null, "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", null,

            // MS-DOS 6.20 RC1
            "MSDOS5.0", null, "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", null,

            // MS-DOS 6.21
            "MSDOS5.0", null, "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", null,

            // MS-DOS 6.22
            "MSDOS5.0", null, "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", null,

            // MS-DOS 7.10
            "MSWIN4.1", null, "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", null,

            // MS-DOS 3.20 for Amstrad
            "MSDOS3.2", null, "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", null,

            // MS-DOS 2.11 for AT&T
            "PSA 1.04",

            // MS-DOS 3.30 for DeLL
            "IBM  3.3", null, "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", null,

            // MS-DOS 3.10 for Epson
            "EPS 3.10", "EPS 3.10", "EPS 3.10",

            // MS-DOS 3.20 for Epson
            "IBM  3.2", "IBM  3.2", "IBM  3.2", "IBM  3.2", "IBM  3.2", "IBM  3.2",

            // MS-DOS 3.20 for HP
            "MSDOS3.2", null, "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", null,

            // MS-DOS 3.21 for Hyosung
            "MSDOS3.2", null, "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", null,

            // MS-DOS 3.21 for Kaypro
            "MSDOS3.2", null, "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", "MSDOS3.2", null,

            // MS-DOS 3.10 for Olivetti
            "IBM  3.1", "IBM  3.1", "IBM  3.1",

            // MS-DOS 3.30 for Toshiba
            "IBM  3.3", null, "IBM  3.3", "IBM  3.3", "IBM  3.3", "IBM  3.3", null,

            // MS-DOS 4.01 for Toshiba
            "T V4.00 ", "T V4.00 ", "T V4.00 ", "T V4.00 ", "T V4.00 ", "T V4.00 ", "T V4.00 ",

            // Novell DOS 7.00
            "NWDOS7.0", "NWDOS7.0", "NWDOS7.0", "NWDOS7.0", "NWDOS7.0", "NWDOS7.0", "NWDOS7.0", "NWDOS7.0",

            // OpenDOS 7.01
            "OPENDOS7", "OPENDOS7", "OPENDOS7", "OPENDOS7", "OPENDOS7", "OPENDOS7", "OPENDOS7", "OPENDOS7",

            // PC-DOS 2.00
            "IBM  2.0",

            // PC-DOS 2.10
            "IBM  2.0",

            // PC-DOS 2000
            "IBM  7.0", null, "IBM  7.0", "IBM  7.0", "IBM  7.0", "IBM  7.0", "IBM  7.0", null,

            // PC-DOS 3.00
            "IBM  3.0",

            // PC-DOS 3.10
            "IBM  3.1",

            // PC-DOS 3.30
            "IBM  3.3", "IBM  3.3",

            // PC-DOS 4.00
            "IBM  4.0", "IBM  4.0",

            // PC-DOS 5.00
            "IBM  5.0", null, "IBM  5.0", "IBM  5.0", "IBM  5.0", "IBM  5.0", "IBM  5.0", null,

            // PC-DOS 5.02
            "IBM  5.0", null, "IBM  5.0", "IBM  5.0", "IBM  5.0", "IBM  5.0", "IBM  5.0", null,

            // PC-DOS 6.10
            "IBM  6.0", null, "IBM  6.0", "IBM  6.0", "IBM  6.0", "IBM  6.0", "IBM  6.0", null,

            // PC-DOS 6.30
            "IBM  6.0", null, "IBM  6.0", "IBM  6.0", "IBM  6.0", "IBM  6.0", "IBM  6.0", null,

            // mkfs.vfat
            "mkfs.fat", "mkfs.fat", "mkfs.fat", "mkfs.fat",

            // mkfs.vfat for Atari
            "mkdosf", "mkdosf", "mkdosf", "mkdosf",

            // Microsoft OS/2 1.00 for Tandy
            "TAN 10.0", "TAN 10.0", "TAN 10.0", "TAN 10.0",

            // Microsoft OS/2 1.10 for AST
            "IBM 10.1", "IBM 10.1", "IBM 10.1", "IBM 10.1",

            // Microsoft OS/2 1.10 for Nokia
            "IBM 10.1", "IBM 10.1", "IBM 10.1", "IBM 10.1",

            // Microsoft OS/2 1.21
            "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2",

            // Microsoft OS/2 1.30.1
            "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2",

            // OS/2 1.20
            "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2",

            // OS/2 1.30
            "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2",

            // OS/2 2.00
            "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0",

            // OS/2 2.10
            "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0",

            // OS/2 2.11
            "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0",

            // OS/2 Warp 3
            "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0",

            // OS/2 Warp 4
            "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0",

            // eComStation
            "IBM 4.50", "IBM 4.50", "IBM 4.50", "IBM 4.50", "IBM 4.50",

            // Windows 95
            null, "MSWIN4.0", "MSWIN4.0", "MSWIN4.0", "MSWIN4.0", "MSWIN4.0", null, "MSWIN4.0",

            // Windows 95 OSR 2
            null, "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", null, "MSWIN4.1",

            // Windows 95 OSR 2.1
            null, "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", null, "MSWIN4.1",

            // Windows 95 OSR 2.5
            null, "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", null, "MSWIN4.1",

            // Windows 98
            null, "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", null, "MSWIN4.1",

            // Windows 98 Second Edition
            null, "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", null, "MSWIN4.1",

            // Windows Me
            "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1",

            // Windows NT 3.10
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",

            // Windows NT 3.50
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",

            // Windows NT 3.51
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",

            // Windows NT 4.00
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",

            // Windows 2000
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",

            // Windows Vista
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0",

            // BeOS R4.5
            "BeOS    ",

            // Hatari
            "NNNNNN", "NNNNNN", "NNNNNN", "NNNNNN", "NNNNNN", "NNNNNN", "NNNNNN", "NNNNNN",

            // Atari TOS 1.04
            "NNNNNN", "NNNNNN",

            // NetBSD 1.6
            "BSD  4.4", "BSD  4.4",

            // NeXTStep 3.3
            "NEXT    ", "NEXT    ",

            // OpenStep for Mach 4.0
            "NEXT    ", "NEXT    ",

            // OpenStep for Mach 4.2
            "NEXT    ", "NEXT    ",

            // Solaris 2.4
            "MSDOS3.3", "MSDOS3.3",

            // COHERENT UNIX 4.2.10
            "COHERENT", "COHERENT", "COHERENT", "COHERENT",

            // SCO OpenServer 5.0.7Hw
            "SCO BOOT", "SCO BOOT", "SCO BOOT",

            // Epson MS-DOS 5.00 for PC-98
            "EPSON5.0", "EPSON5.0",

            // NEC MS-DOS 3.30 for PC-98
            "NEC 2.00", "NEC 2.00",

            // NEC MS-DOS 5.00 for PC-98
            "NEC  5.0", "NEC  5.0",

            // NEC MS-DOS 6.20 for PC-98
            "NEC  5.0", "NEC  5.0",

            // GEOS 1.2
            "GEOWORKS",

            // GEOS 2.0
            "GEOWORKS",

            // GEOS 3.1
            "GEOWORKS",

            // GEOS 3.2
            "GEOWORKS",

            // GEOS 4.1
            "GEOWORKS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new FAT();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT12", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat12Apm
    {
        readonly string[] testfiles =
        {
            "macosx_10.11.aif"
        };

        readonly ulong[] sectors =
        {
            16384
        };

        readonly uint[] sectorsize =
        {
            512
        };

        readonly long[] clusters =
        {
            4076
        };

        readonly int[] clustersize =
        {
            2048
        };

        readonly string[] volumename =
        {
            "VOLUMELABEL"
        };

        readonly string[] volumeserial =
        {
            "32181F09"
        };

        readonly string[] oemid =
        {
            "BSD  4.4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12 (APM)", testfiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "DOS_FAT_12")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT12", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat12Gpt
    {
        readonly string[] testfiles =
        {
            "macosx_10.11.aif"
        };

        readonly ulong[] sectors =
        {
            16384
        };

        readonly uint[] sectorsize =
        {
            512
        };

        readonly long[] clusters =
        {
            4076
        };

        readonly int[] clustersize =
        {
            2048
        };

        readonly string[] volumename =
        {
            "VOLUMELABEL"
        };

        readonly string[] volumeserial =
        {
            "66901F1B"
        };

        readonly string[] oemid =
        {
            "BSD  4.4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12 (GPT)", testfiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Microsoft Basic data")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT12", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat12Mbr
    {
        readonly string[] testfiles =
        {
            "compaqmsdos331.aif", "drdos_3.40.aif", "drdos_3.41.aif", "drdos_5.00.aif", "drdos_6.00.aif",
            "drdos_7.02.aif", "drdos_7.03.aif", "drdos_8.00.aif", "msdos331.aif", "msdos401.aif", "msdos500.aif",
            "msdos600.aif", "msdos620rc1.aif", "msdos620.aif", "msdos621.aif", "msdos622.aif", "msdos710.aif",
            "novelldos_7.00.aif", "opendos_7.01.aif", "pcdos2000.aif", "pcdos200.aif", "pcdos210.aif", "pcdos300.aif",
            "pcdos310.aif", "pcdos330.aif", "pcdos400.aif", "pcdos500.aif", "pcdos502.aif", "pcdos610.aif",
            "pcdos630.aif", "toshibamsdos330.aif", "toshibamsdos401.aif", "msos2_1.21.aif", "msos2_1.30.1.aif",
            "multiuserdos_7.22r4.aif", "os2_1.20.aif", "os2_1.30.aif", "os2_6.307.aif", "os2_6.514.aif",
            "os2_6.617.aif", "os2_8.162.aif", "os2_9.023.aif", "ecs.aif", "macosx_10.11.aif", "win10.aif",
            "win2000.aif", "win95.aif", "win95osr2.1.aif", "win95osr2.5.aif", "win95osr2.aif", "win98.aif",
            "win98se.aif", "winme.aif", "winnt_3.10.aif", "winnt_3.50.aif", "winnt_3.51.aif", "winnt_4.00.aif",
            "winvista.aif", "beos_r4.5.aif", "linux.aif", "freebsd_6.1.aif", "freebsd_7.0.aif", "freebsd_8.2.aif"
        };

        readonly ulong[] sectors =
        {
            8192, 30720, 28672, 28672, 28672, 28672, 28672, 28672, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
            16384, 28672, 28672, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 8192,
            8192, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384,
            16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384,
            16384, 16384
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            1000, 3654, 3520, 3520, 3520, 3520, 3520, 3520, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 2008, 3520,
            3520, 4024, 4031, 4031, 4024, 4024, 4024, 4024, 4024, 4024, 4024, 4024, 1000, 1000, 2008, 2008, 2008, 2008,
            2008, 2008, 2008, 2008, 2008, 2008, 1890, 4079, 3552, 4088, 2008, 2008, 2008, 2008, 2044, 2044, 2044, 4016,
            2044, 2044, 4016, 3072, 2040, 3584, 2044, 2044, 2044
        };

        readonly int[] clustersize =
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 2048, 2048, 2048, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 2048,
            4096, 4096, 2048, 2048, 4096, 2048, 4096, 4096, 4096
        };

        readonly string[] volumename =
        {
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, null, null,
            null, null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VolumeLabel", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL"
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null, null, null, "1BFB1273", null, "407D1907", "345D18FB", "332518F4", "395718E9",
            "076718EF", "1371181B", "23281816", "2F781809", null, null, "294F100F", null, null, null, null, null,
            "0F340FE4", "1A5E0FF9", "1D2F0FFE", "076C1004", "2C481009", null, "3C2319E8", "66CC3C15", "66A54C15", null,
            "5C578015", "5B845015", "5C4BF015", "E6B5F414", "E6B15414", "E6A41414", "E6A39414", "E6B0B814", "26A21EF4",
            "74F4921D", "C4B64D11", "29200D0C", "234F0DE4", "074C0DFC", "33640D18", "0E121460", "094C0EED", "38310F02",
            "50489A1B", "2CE52101", "94313E7E", "BC184FE6", "BAD08A1E", "00000000", "8D418102", "8FC80E0A", "34FA0E0B",
            "02140E0B"
        };

        readonly string[] oemid =
        {
            "IBM  3.3", "IBM  3.2", "IBM  3.2", "IBM  3.3", "IBM  3.3", "IBM  3.3", "DRDOS  7", "IBM  5.0", "IBM  3.3",
            "MSDOS4.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "IBM  3.3",
            "IBM  3.3", "IBM  7.0", "IBM  2.0", "IBM  2.0", "IBM  3.0", "IBM  3.1", "IBM  3.3", "IBM  4.0", "IBM  5.0",
            "IBM  5.0", "IBM  6.0", "IBM  6.0", "T V3.30 ", "T V4.00 ", "IBM 10.2", "IBM 10.2", "IBM  3.2", "IBM 10.2",
            "IBM 10.2", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 4.50", "BSD  4.4", "MSDOS5.0",
            "MSDOS5.0", "MSWIN4.0", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSDOS5.0",
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "BeOS    ", "mkfs.fat", "BSD  4.4", "BSD  4.4", "BSD4.4  "
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12 (MBR)", testfiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                Assert.AreEqual(true, fs.Identify(image, partitions[0]), testfiles[i]);
                fs.GetInformation(image, partitions[0], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT12", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat12Human
    {
        readonly string[] testfiles =
        {
            "diska.aif", "diskb.aif"
        };

        readonly MediaType[] mediatypes =
        {
            MediaType.SHARP_525, MediaType.SHARP_525
        };

        readonly ulong[] sectors =
        {
            1232, 1232
        };

        readonly uint[] sectorsize =
        {
            1024, 1024
        };

        readonly long[] clusters =
        {
            1232, 1232
        };

        readonly int[] clustersize =
        {
            1024, 1024
        };

        readonly string[] volumename =
        {
            null, null
        };

        readonly string[] volumeserial =
        {
            null, null
        };

        readonly string[] oemid =
        {
            "Hudson soft 2.00", "Hudson soft 2.00"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12 (Human68K)", testfiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new FAT();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT12", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}