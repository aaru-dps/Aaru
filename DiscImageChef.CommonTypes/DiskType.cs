// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiskType.cs
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

namespace DiscImageChef.CommonTypes
{
    // Disk types
    public enum DiskType
    {
        /// <summary>Unknown disk type</summary>
        Unknown,

        // Somewhat standard Compact Disc formats
        /// <summary>CD Digital Audio (Red Book)</summary>
        CDDA,
        /// <summary>CD+G (Red Book)</summary>
        CDG,
        /// <summary>CD+EG (Red Book)</summary>
        CDEG,
        /// <summary>CD-i (Green Book)</summary>
        CDI,
        /// <summary>CD-ROM (Yellow Book)</summary>
        CDROM,
        /// <summary>CD-ROM XA (Yellow Book)</summary>
        CDROMXA,
        /// <summary>CD+ (Blue Book)</summary>
        CDPLUS,
        /// <summary>CD-MO (Orange Book)</summary>
        CDMO,
        /// <summary>CD-Recordable (Orange Book)</summary>
        CDR,
        /// <summary>CD-ReWritable (Orange Book)</summary>
        CDRW,
        /// <summary>Mount-Rainier CD-RW</summary>
        CDMRW,
        /// <summary>Video CD (White Book)</summary>
        VCD,
        /// <summary>Super Video CD (White Book)</summary>
        SVCD,
        /// <summary>Photo CD (Beige Book)</summary>
        PCD,
        /// <summary>Super Audio CD (Scarlet Book)</summary>
        SACD,
        /// <summary>Double-Density CD-ROM (Purple Book)</summary>
        DDCD,
        /// <summary>DD CD-R (Purple Book)</summary>
        DDCDR,
        /// <summary>DD CD-RW (Purple Book)</summary>
        DDCDRW,
        /// <summary>DTS audio CD (non-standard)</summary>
        DTSCD,
        /// <summary>CD-MIDI (Red Book)</summary>
        CDMIDI,
        /// <summary>CD-Video (ISO/IEC 61104)</summary>
        CDV,
        /// <summary>Any unknown or standard violating CD</summary>
        CD,

        // Standard DVD formats
        /// <summary>DVD-ROM (applies to DVD Video and DVD Audio)</summary>
        DVDROM,
        /// <summary>DVD-R</summary>
        DVDR,
        /// <summary>DVD-RW</summary>
        DVDRW,
        /// <summary>DVD+R</summary>
        DVDPR,
        /// <summary>DVD+RW</summary>
        DVDPRW,
        /// <summary>DVD+RW DL</summary>
        DVDPRWDL,
        /// <summary>DVD-R DL</summary>
        DVDRDL,
        /// <summary>DVD+R DL</summary>
        DVDPRDL,
        /// <summary>DVD-RAM</summary>
        DVDRAM,
        /// <summary>DVD-RW DL</summary>
        DVDRWDL,
        /// <summary>DVD-Download</summary>
        DVDDownload,

        // Standard HD-DVD formats
        /// <summary>HD DVD-ROM (applies to HD DVD Video)</summary>
        HDDVDROM,
        /// <summary>HD DVD-RAM</summary>
        HDDVDRAM,
        /// <summary>HD DVD-R</summary>
        HDDVDR,
        /// <summary>HD DVD-RW</summary>
        HDDVDRW,
        /// <summary>HD DVD-R DL</summary>
        HDDVDRDL,
        /// <summary>HD DVD-RW DL</summary>
        HDDVDRWDL,

        // Standard Blu-ray formats
        /// <summary>BD-ROM (and BD Video)</summary>
        BDROM,
        /// <summary>BD-R</summary>
        BDR,
        /// <summary>BD-RE</summary>
        BDRE,
        /// <summary>BD-R XL</summary>
        BDRXL,
        /// <summary>BD-RE XL</summary>
        BDREXL,

        // Rare or uncommon standards
        /// <summary>Enhanced Versatile Disc</summary>
        EVD,
        /// <summary>Forward Versatile Disc</summary>
        FVD,
        /// <summary>Holographic Versatile Disc</summary>
        HVD,
        /// <summary>China Blue High Definition</summary>
        CBHD,
        /// <summary>High Definition Versatile Multilayer Disc</summary>
        HDVMD,
        /// <summary>Versatile Compact Disc High Density</summary>
        VCDHD,
        /// <summary>Pioneer LaserDisc</summary>
        LD,
        /// <summary>Pioneer LaserDisc data</summary>
        LDROM,
        /// <summary>Sony MiniDisc</summary>
        MD,
        /// <summary>Sony Hi-MD</summary>
        HiMD,
        /// <summary>Ultra Density Optical</summary>
        UDO,
        /// <summary>Stacked Volumetric Optical Disc</summary>
        SVOD,
        /// <summary>Five Dimensional disc</summary>
        FDDVD,

        // Propietary game discs
        /// <summary>Sony PlayStation game CD</summary>
        PS1CD,
        /// <summary>Sony PlayStation 2 game CD</summary>
        PS2CD,
        /// <summary>Sony PlayStation 2 game DVD</summary>
        PS2DVD,
        /// <summary>Sony PlayStation 3 game DVD</summary>
        PS3DVD,
        /// <summary>Sony PlayStation 3 game Blu-ray</summary>
        PS3BD,
        /// <summary>Sony PlayStation 4 game Blu-ray</summary>
        PS4BD,
        /// <summary>Sony PlayStation Portable Universal Media Disc (ECMA-365)</summary>
        UMD,
        /// <summary>Nintendo GameCube Optical Disc</summary>
        GOD,
        /// <summary>Nintendo Wii Optical Disc</summary>
        WOD,
        /// <summary>Nintendo Wii U Optical Disc</summary>
        WUOD,
        /// <summary>Microsoft X-box Game Disc</summary>
        XGD,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD2,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD3,
        /// <summary>Microsoft X-box One Game Disc</summary>
        XGD4,
        /// <summary>Sega MegaCD</summary>
        MEGACD,
        /// <summary>Sega Saturn disc</summary>
        SATURNCD,
        /// <summary>Sega/Yamaha Gigabyte Disc</summary>
        GDROM,
        /// <summary>Sega/Yamaha recordable Gigabyte Disc}}</summary>
        GDR,

        // Apple standard floppy format
        /// <summary>5.25", SS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR</summary>
        Apple32SS,
        /// <summary>5.25", DS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR</summary>
        Apple32DS,
        /// <summary>5.25", SS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR</summary>
        Apple33SS,
        /// <summary>5.25", DS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR</summary>
        Apple33DS,
        /// <summary>3.5", SS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR</summary>
        AppleSonySS,
        /// <summary>3.5", DS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR</summary>
        AppleSonyDS,
        /// <summary>5.25", DS, ?D, ?? tracks, ?? spt, 512 bytes/sector, GCR, opposite side heads, aka Twiggy</summary>
        AppleFileWare,

        // IBM/Microsoft PC standard floppy formats
        /// <summary>5.25", SS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_525_SS_DD_8,
        /// <summary>5.25", SS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_525_SS_DD_9,
        /// <summary>5.25", DS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_525_DS_DD_8,
        /// <summary>5.25", DS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_525_DS_DD_9,
        /// <summary>5.25", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        DOS_525_HD,
        /// <summary>3.5", SS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_35_SS_DD_8,
        /// <summary>3.5", SS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_35_SS_DD_9,
        /// <summary>3.5", DS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_35_DS_DD_8,
        /// <summary>3.5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_35_DS_DD_9,
        /// <summary>3.5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM</summary>
        DOS_35_HD,
        /// <summary>3.5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM</summary>
        DOS_35_ED,

        // Microsoft non standard floppy formats
        /// <summary>3.5", DS, DD, 80 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF,
        /// <summary>3.5", DS, DD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF_82,

        // IBM non standard floppy formats
        XDF_525,
        XDF_35,

        // IBM standard floppy formats
        /// <summary>8", SS, SD, 32 tracks, 8 spt, 319 bytes/sector, FM</summary>
        IBM23FD,
        /// <summary>8", SS, SD, 73 tracks, 26 spt, 128 bytes/sector, FM</summary>
        IBM33FD_128,
        /// <summary>8", SS, SD, 74 tracks, 15 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM33FD_256,
        /// <summary>8", SS, SD, 74 tracks, 8 spt, 512 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM33FD_512,
        /// <summary>8", DS, SD, 74 tracks, 26 spt, 128 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM43FD_128,
        /// <summary>8", DS, SD, 74 tracks, 26 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM43FD_256,
        /// <summary>8", DS, DD, 74 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        IBM53FD_256,
        /// <summary>8", DS, DD, 74 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        IBM53FD_512,
        /// <summary>8", DS, DD, 74 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        IBM53FD_1024,

        // DEC standard floppy formats
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        RX01,
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM</summary>
        RX02,

        // Acorn standard floppy formats
        /// <summary>5,25", SS, SD, 40 tracks, 10 spt, 256 bytes/sector, FM</summary>
        ACORN_525_SS_SD_40,
        /// <summary>5,25", SS, SD, 80 tracks, 10 spt, 256 bytes/sector, FM</summary>
        ACORN_525_SS_SD_80,
        /// <summary>5,25", SS, DD, 40 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_SS_DD_40,
        /// <summary>5,25", SS, DD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_SS_DD_80,
        /// <summary>5,25", DS, DD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_DS_DD,
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        ACORN_35_DS_DD,

        // Atari standard floppy formats
        /// <summary>5,25", SS, SD, 40 tracks, 18 spt, 128 bytes/sector, FM</summary>
        ATARI_525_SD,
        /// <summary>5,25", SS, ED, 40 tracks, 26 spt, 128 bytes/sector, MFM</summary>
        ATARI_525_ED,
        /// <summary>5,25", SS, DD, 40 tracks, 18 spt, 256 bytes/sector, MFM</summary>
        ATARI_525_DD,

        // Commodore standard floppy formats
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM (1581)</summary>
        CBM_35_DD,
        /// <summary>3,5", DS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_DD,
        /// <summary>3,5", DS, HD, 80 tracks, 22 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_HD,
        /// <summary>5,25", SS, DD, 35 tracks, GCR</summary>
        CBM_1540,

        // NEC standard floppy formats
        /// <summary>8", SS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        NEC_8_SD,
        /// <summary>8", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_8_DD,
        /// <summary>5,25", DS, HD, 80 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_525_HD,
        /// <summary>3,5", DS, HD, 80 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_35_HD_8,
        /// <summary>3,5", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        NEC_35_HD_15,

        // SHARP standard floppy formats
        /// <summary>5,25", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM</summary>
        SHARP_525,
        /// <summary>3,5", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM</summary>
        SHARP_35,

        // ECMA standards
        /// <summary>5,25", DS, DD, 80 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_99_8,
        /// <summary>5,25", DS, DD, 77 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_99_15,
        /// <summary>5,25", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_99_26,
        /// <summary>3,5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        ECMA_100,
        /// <summary>3,5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM</summary>
        ECMA_125,
        /// <summary>3,5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM</summary>
        ECMA_147,
        /// <summary>8", SS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        ECMA_54,
        /// <summary>8", DS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        ECMA_59,
        /// <summary>5,25", SS, DD, 35 tracks, 9 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector</summary>
        ECMA_66,
        /// <summary>8", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_69_8,
        /// <summary>8", DS, DD, 77 tracks, 15 spt, 512 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_69_15,
        /// <summary>8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_69_26,
        /// <summary>5,25", DS, DD, 40 tracks, 16 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector, track 0 side 1 = 16 sectors, 256 bytes/sector</summary>
        ECMA_70,
        /// <summary>5,25", DS, DD, 80 tracks, 16 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector, track 0 side 1 = 16 sectors, 256 bytes/sector</summary>
        ECMA_78,
        /// <summary>5,25", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, FM</summary>
        ECMA_78_2,
        /// <summary>3,5", M.O., 250000 sectors, 512 bytes/sector</summary>
        ECMA_154,
        /// <summary>5,25", M.O., 940470 sectors, 512 bytes/sector</summary>
        ECMA_183_512,
        /// <summary>5,25", M.O., 520902 sectors, 1024 bytes/sector</summary>
        ECMA_183_1024,
        /// <summary>5,25", M.O., 1165600 sectors, 512 bytes/sector</summary>
        ECMA_184_512,
        /// <summary>5,25", M.O., 639200 sectors, 1024 bytes/sector</summary>
        ECMA_184_1024,
        /// <summary>3,5", M.O., 448500 sectors, 512 bytes/sector</summary>
        ECMA_201,

        // FDFORMAT, non-standard floppy formats
        /// <summary>5,25", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_DD,
        /// <summary>5,25", DS, HD, 82 tracks, 17 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_HD,
        /// <summary>3,5", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_DD,
        /// <summary>3,5", DS, HD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_HD,

        // Generic hard disks
        GENERIC_HDD
    };
}

