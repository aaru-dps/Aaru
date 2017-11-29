// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaType.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common media types.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.CommonTypes
{
    // Media (disk, cartridge, tape, cassette, etc) types
    public enum MediaType
    {
        /// <summary>Unknown disk type</summary>
        Unknown,

        #region Somewhat standard Compact Disc formats
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
        #endregion Somewhat standard Compact Disc formats

        #region Standard DVD formats
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
        #endregion Standard DVD formats

        #region Standard HD-DVD formats
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
        #endregion Standard HD-DVD formats

        #region Standard Blu-ray formats
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
        #endregion Standard Blu-ray formats

        #region Rare or uncommon optical standards
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
        /// <summary>Stacked Volumetric Optical Disc</summary>
        SVOD,
        /// <summary>Five Dimensional disc</summary>
        FDDVD,
        #endregion Rare or uncommon optical standards

        #region LaserDisc based
        /// <summary>Pioneer LaserDisc</summary>
        LD,
        /// <summary>Pioneer LaserDisc data</summary>
        LDROM,
        LDROM2,
        LVROM,
        MegaLD,
        #endregion LaserDisc based

        #region MiniDisc based
        /// <summary>Sony Hi-MD</summary>
        HiMD,
        /// <summary>Sony MiniDisc</summary>
        MD,
        MDData,
        MDData2,
        #endregion MiniDisc based

        #region Plasmon UDO
        /// <summary>5.25", Phase-Change, 1834348 sectors, 8192 bytes/sector, Ultra Density Optical, ECMA-350, ISO 17345</summary>
        UDO,
        /// <summary>5.25", Phase-Change, 3669724 sectors, 8192 bytes/sector, Ultra Density Optical 2, ECMA-380, ISO 11976</summary>
        UDO2,
        /// <summary>5.25", Write-Once, 3668759 sectors, 8192 bytes/sector, Ultra Density Optical 2, ECMA-380, ISO 11976</summary>
        UDO2_WORM,
        #endregion Plasmon UDO

        #region Sony game media
        PlayStationMemoryCard,
        PlayStationMemoryCard2,
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
        #endregion Sony game media

        #region Microsoft game media
        /// <summary>Microsoft X-box Game Disc</summary>
        XGD,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD2,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD3,
        /// <summary>Microsoft X-box One Game Disc</summary>
        XGD4,
        #endregion Microsoft game media

        #region Sega game media
        /// <summary>Sega MegaCD</summary>
        MEGACD,
        /// <summary>Sega Saturn disc</summary>
        SATURNCD,
        /// <summary>Sega/Yamaha Gigabyte Disc</summary>
        GDROM,
        /// <summary>Sega/Yamaha recordable Gigabyte Disc</summary>
        GDR,
        SegaCard,
        #endregion Sega game media

        #region Other game media
        /// <summary>PC-Engine / TurboGrafx cartridge</summary>
        HuCard,
        /// <summary>PC-Engine / TurboGrafx CD</summary>
        SuperCDROM2,
        /// <summary>Atari Jaguar CD</summary>
        JaguarCD,
        /// <summary>3DO CD</summary>
        ThreeDO,
        #endregion Other game media

        #region Apple standard floppy format
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
        #endregion Apple standard floppy format

        #region IBM/Microsoft PC standard floppy formats
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
        #endregion IBM/Microsoft PC standard floppy formats

        #region Microsoft non standard floppy formats
        /// <summary>3.5", DS, HD, 80 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF,
        /// <summary>3.5", DS, HD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF_82,
        #endregion Microsoft non standard floppy formats

        #region IBM non standard floppy formats
        XDF_525,
        /// <summary>3.5", DS, HD, 80 tracks, 4 spt, 8192 + 2048 + 1024 + 512 bytes/sector, MFMm track 0 = 19 sectors, 512 bytes/sector, falsified to DOS as 23 spt, 512 bps</summary>
        XDF_35,
        #endregion IBM non standard floppy formats

        #region IBM standard floppy formats
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
        #endregion IBM standard floppy formats

        #region DEC standard floppy formats
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        RX01,
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM</summary>
        RX02,
        /// <summary>8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM</summary>
        RX03,
        /// <summary>5.25", SS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        RX50,
        #endregion DEC standard floppy formats

        #region Acorn standard floppy formats
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
        /// <summary>3,5", DS, DD, 80 tracks, 5 spt, 1024 bytes/sector, MFM</summary>
        ACORN_35_DS_DD,
        /// <summary>3,5", DS, HD, 80 tracks, 10 spt, 1024 bytes/sector, MFM</summary>
        ACORN_35_DS_HD,
        #endregion Acorn standard floppy formats

        #region Atari standard floppy formats
        /// <summary>5,25", SS, SD, 40 tracks, 18 spt, 128 bytes/sector, FM</summary>
        ATARI_525_SD,
        /// <summary>5,25", SS, ED, 40 tracks, 26 spt, 128 bytes/sector, MFM</summary>
        ATARI_525_ED,
        /// <summary>5,25", SS, DD, 40 tracks, 18 spt, 256 bytes/sector, MFM</summary>
        ATARI_525_DD,
        /// <summary>3,5", SS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_SS_DD,
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_DS_DD,
        /// <summary>3,5", SS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_SS_DD_11,
        /// <summary>3,5", DS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_DS_DD_11,
        #endregion Atari standard floppy formats

        #region Commodore standard floppy formats
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM (1581)</summary>
        CBM_35_DD,
        /// <summary>3,5", DS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_DD,
        /// <summary>3,5", DS, HD, 80 tracks, 22 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_HD,
        /// <summary>5,25", SS, DD, 35 tracks, GCR</summary>
        CBM_1540,
        /// <summary>5,25", SS, DD, 40 tracks, GCR</summary>
        CBM_1540_Ext,
        /// <summary>5,25", DS, DD, 35 tracks, GCR</summary>
        CBM_1571,
        #endregion Commodore standard floppy formats

        #region NEC standard floppy formats
        /// <summary>8", DS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        NEC_8_SD,
        /// <summary>8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, MFM</summary>
        NEC_8_DD,
        /// <summary>5.25", SS, SD, 80 tracks, 16 spt, 256 bytes/sector, FM</summary>
        NEC_525_SS,
        /// <summary>5.25", DS, SD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        NEC_525_DS,
        /// <summary>5,25", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_525_HD,
        /// <summary>3,5", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM, aka mode 3</summary>
        NEC_35_HD_8,
        /// <summary>3,5", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        NEC_35_HD_15,
        /// <summary>3,5", DS, TD, 240 tracks, 38 spt, 512 bytes/sector, MFM</summary>
        NEC_35_TD,
        #endregion NEC standard floppy formats

        #region SHARP standard floppy formats
        /// <summary>5,25", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        SHARP_525,
        /// <summary>3,5", DS, HD, 80 tracks, 9 spt, 1024 bytes/sector, MFM</summary>
        SHARP_525_9,
        /// <summary>3,5", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        SHARP_35,
        /// <summary>3,5", DS, HD, 80 tracks, 9 spt, 1024 bytes/sector, MFM</summary>
        SHARP_35_9,
        #endregion SHARP standard floppy formats

        #region ECMA floppy standards
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
        #endregion ECMA floppy standards

        #region FDFORMAT, non-standard floppy formats
        /// <summary>5,25", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_DD,
        /// <summary>5,25", DS, HD, 82 tracks, 17 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_HD,
        /// <summary>3,5", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_DD,
        /// <summary>3,5", DS, HD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_HD,
        #endregion FDFORMAT, non-standard floppy formats

        #region Apricot ACT standard floppy formats
        /// <summary>3.5", DS, DD, 70 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        Apricot_35,
        #endregion Apricot ACT standard floppy formats

        #region OnStream ADR
        ADR2120,
        ADR260,
        ADR30,
        ADR50,
        #endregion OnStream ADR

        #region Advanced Intelligent Tape
        AIT1,
        AIT1Turbo,
        AIT2,
        AIT2Turbo,
        AIT3,
        AIT3Ex,
        AIT3Turbo,
        AIT4,
        AIT5,
        AITETurbo,
        SAIT1,
        SAIT2,
        #endregion Advanced Intelligent Tape

        #region Iomega
        Bernoulli,
        Bernoulli2,
        Ditto,
        DittoMax,
        Jaz,
        Jaz2,
        PocketZip,
        REV120,
        REV35,
        REV70,
        ZIP100,
        ZIP250,
        ZIP750,
        #endregion Iomega

        #region Audio or video media
        CompactCassette,
        Data8,
        MiniDV,
        #endregion Audio media

        #region CompactFlash Association
        CFast,
        CompactFlash,
        CompactFlashType2,
        #endregion CompactFlash Association

        #region Digital Audio Tape / Digital Data Storage
        DigitalAudioTape,
        DAT160,
        DAT320,
        DAT72,
        DDS1,
        DDS2,
        DDS3,
        DDS4,
        #endregion Digital Audio Tape / Digital Data Storage

        #region DEC
        CompactTapeI,
        CompactTapeII,
        DECtapeII,
        DLTtapeIII,
        DLTtapeIIIxt,
        DLTtapeIV,
        DLTtapeS4,
        SDLT1,
        SDLT2,
        VStapeI,
        #endregion DEC

        #region Exatape
        Exatape15m,
        Exatape22m,
        Exatape22mAME,
        Exatape28m,
        Exatape40m,
        Exatape45m,
        Exatape54m,
        Exatape75m,
        Exatape76m,
        Exatape80m,
        Exatape106m,
        Exatape160mXL,
        Exatape112m,
        Exatape125m,
        Exatape150m,
        Exatape170m,
        Exatape225m,
        #endregion Exatape

        #region PCMCIA / ExpressCard
        ExpressCard34,
        ExpressCard54,
        PCCardTypeI,
        PCCardTypeII,
        PCCardTypeIII,
        PCCardTypeIV,
        #endregion PCMCIA / ExpressCard

        #region SyQuest
        EZ135,
        EZ230,
        Quest,
        SparQ,
        SQ100,
        SQ200,
        SQ300,
        SQ310,
        SQ327,
        SQ400,
        SQ800,
        SQ1500,
        SQ2000,
        SyJet,
        #endregion SyQuest

        #region Nintendo
        FamicomGamePak,
        GameBoyAdvanceGamePak,
        GameBoyGamePak,
        /// <summary>Nintendo GameCube Optical Disc</summary>
        GOD,
        N64DD,
        N64GamePak,
        NESGamePak,
        Nintendo3DSGameCard,
        NintendoDiskCard,
        NintendoDSGameCard,
        NintendoDSiGameCard,
        SNESGamePak,
        SNESGamePakUS,
        /// <summary>Nintendo Wii Optical Disc</summary>
        WOD,
        /// <summary>Nintendo Wii U Optical Disc</summary>
        WUOD,
        #endregion Nintendo

        #region IBM Tapes
        IBM3470,
        IBM3480,
        IBM3490,
        IBM3490E,
        IBM3592,
        #endregion IBM Tapes

        #region LTO Ultrium
        LTO,
        LTO2,
        LTO3,
        LTO3WORM,
        LTO4,
        LTO4WORM,
        LTO5,
        LTO5WORM,
        LTO6,
        LTO6WORM,
        LTO7,
        LTO7WORM,
        #endregion LTO Ultrium

        #region MemoryStick
        MemoryStick,
        MemoryStickDuo,
        MemoryStickMicro,
        MemoryStickPro,
        MemoryStickProDuo,
        #endregion MemoryStick

        #region SecureDigital
        microSD,
        miniSD,
        SecureDigital,
        #endregion SecureDigital

        #region MultiMediaCard
        MMC,
        MMCmicro,
        RSMMC,
        MMCplus,
        MMCmobile,
        #endregion MultiMediaCard

        #region SLR
        MLR1,
        MLR1SL,
        MLR3,
        SLR1,
        SLR2,
        SLR3,
        SLR32,
        SLR32SL,
        SLR4,
        SLR5,
        SLR5SL,
        SLR6,
        SLRtape7,
        SLRtape7SL,
        SLRtape24,
        SLRtape24SL,
        SLRtape40,
        SLRtape50,
        SLRtape60,
        SLRtape75,
        SLRtape100,
        SLRtape140,
        #endregion SLR

        #region QIC
        QIC11,
        QIC120,
        QIC1350,
        QIC150,
        QIC24,
        QIC3010,
        QIC3020,
        QIC3080,
        QIC3095,
        QIC320,
        QIC40,
        QIC525,
        QIC80,
        #endregion QIC

        #region StorageTek tapes
        STK4480,
        STK4490,
        STK9490,
        T9840A,
        T9840B,
        T9840C,
        T9840D,
        T9940A,
        T9940B,
        T10000A,
        T10000B,
        T10000C,
        T10000D,
        #endregion StorageTek tapes

        #region Travan
        Travan,
        Travan1Ex,
        Travan3,
        Travan3Ex,
        Travan4,
        Travan5,
        Travan7,
        #endregion Travan

        #region VXA
        VXA1,
        VXA2,
        VXA3,
        #endregion VXA

        #region Magneto-optical
        /// <summary>5,25", M.O., ??? sectors, 1024 bytes/sector, ECMA-153, ISO 11560</summary>
        ECMA_153,
        /// <summary>5,25", M.O., ??? sectors, 512 bytes/sector, ECMA-153, ISO 11560</summary>
        ECMA_153_512,
        /// <summary>3,5", M.O., 249850 sectors, 512 bytes/sector, ECMA-154, ISO 10090</summary>
        ECMA_154,
        /// <summary>5,25", M.O., 904995 sectors, 512 bytes/sector, ECMA-183, ISO 13481</summary>
        ECMA_183_512,
        /// <summary>5,25", M.O., 498526 sectors, 1024 bytes/sector, ECMA-183, ISO 13481</summary>
        ECMA_183,
        /// <summary>5,25", M.O., 1128772 or 1163337 sectors, 512 bytes/sector, ECMA-183, ISO 13549</summary>
        ECMA_184_512,
        /// <summary>5,25", M.O., 603466 or 637041 sectors, 1024 bytes/sector, ECMA-183, ISO 13549</summary>
        ECMA_184,
        /// <summary>300mm, M.O., ??? sectors, 1024 bytes/sector, ECMA-189, ISO 13614</summary>
        ECMA_189,
        /// <summary>300mm, M.O., ??? sectors, 1024 bytes/sector, ECMA-190, ISO 13403</summary>
        ECMA_190,
        /// <summary>5,25", M.O., 936921 or 948770 sectors, 1024 bytes/sector, ECMA-195, ISO 13842</summary>
        ECMA_195,
        /// <summary>5,25", M.O., 1644581 or 1647371 sectors, 512 bytes/sector, ECMA-195, ISO 13842</summary>
        ECMA_195_512,
        /// <summary>3,5", M.O., 446325 sectors, 512 bytes/sector, ECMA-201, ISO 13963</summary>
        ECMA_201,
        /// <summary>3,5", M.O., 429975 sectors, 512 bytes/sector, embossed, ISO 13963</summary>
        ECMA_201_ROM,
        /// <summary>3,5", M.O., 371371 sectors, 1024 bytes/sector, ECMA-223</summary>
        ECMA_223,
        /// <summary>3,5", M.O., 694929 sectors, 512 bytes/sector, ECMA-223</summary>
        ECMA_223_512,
        /// <summary>5,25", M.O., 1244621 sectors, 1024 bytes/sector, ECMA-238, ISO 15486</summary>
        ECMA_238,
        /// <summary>3,5", M.O., 318988, 320332 or 321100 sectors, 2048 bytes/sector, ECMA-239, ISO 15498</summary>
        ECMA_239,
        /// <summary>356mm, M.O., 14476734 sectors, 1024 bytes/sector, ECMA-260, ISO 15898</summary>
        ECMA_260,
        /// <summary>356mm, M.O., 24445990 sectors, 1024 bytes/sector, ECMA-260, ISO 15898</summary>
        ECMA_260_Double,
        /// <summary>5,25", M.O., 1128134 sectors, 2048 bytes/sector, ECMA-280, ISO 18093</summary>
        ECMA_280,
        /// <summary>300mm, M.O., 7355716 sectors, 2048 bytes/sector, ECMA-317, ISO 20162</summary>
        ECMA_317,
        /// <summary>5,25", M.O., 1095840 sectors, 4096 bytes/sector, ECMA-322, ISO 22092</summary>
        ECMA_322,
        /// <summary>5,25", M.O., 2043664 sectors, 2048 bytes/sector, ECMA-322, ISO 22092</summary>
        ECMA_322_2k,
        /// <summary>3,5", M.O., 605846 sectors, 2048 bytes/sector, Cherry Book, GigaMo, ECMA-351, ISO 17346</summary>
        GigaMo,
        /// <summary>3,5", M.O., 1063146 sectors, 2048 bytes/sector, Cherry Book 2, GigaMo 2, ECMA-353, ISO 22533</summary>
        GigaMo2,
        UnknownMO,
        #endregion Magneto-optical

        #region Other floppy standards
        CompactFloppy,
        DemiDiskette,
        /// <summary>3.5", 652 tracks, 2 sides, 512 bytes/sector, Floptical, ECMA-207, ISO 14169</summary>
        Floptical,
        HiFD,
        LS120,
        LS240,
        FD32MB,
        QuickDisk,
        UHD144,
        VideoFloppy,
        Wafer,
        ZXMicrodrive,
        #endregion Other floppy standards

        #region Miscellaneous
        BeeCard,
        Borsu,
        DataStore,
        DIR,
        DST,
        DTF,
        DTF2,
        Flextra3020,
        Flextra3225,
        HiTC1,
        HiTC2,
        LT1,
        MiniCard,
        Orb,
        Orb5,
        SmartMedia,
        xD,
        XQD,
        DataPlay,
        /// <summary>120mm, Phase-Change, 1298496 sectors, 512 bytes/sector, PD650, ECMA-240, ISO 15485</summary>
        PD650,
        /// <summary>120mm, Write-Once, 1281856 sectors, 512 bytes/sector, PD650, ECMA-240, ISO 15485</summary>
        PD650_WORM,
        #endregion Miscellaneous

        #region Apple Hard Disks
        AppleProfile,
        AppleWidget,
        AppleHD20,
        #endregion Apple Hard Disks

        #region DEC hard disks
        /// <summary>2382 cylinders, 4 tracks/cylinder, 42 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 204890112 bytes</summary>
        RA60,
        /// <summary>546 cylinders, 14 tracks/cylinder, 31 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 121325568 bytes</summary>
        RA80,
        /// <summary>1248 cylinders, 14 tracks/cylinder, 51 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 456228864 bytes</summary>
        RA81,
        /// <summary>302 cylinders, 4 tracks/cylinder, 42 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 25976832 bytes</summary>
        RC25,
        /// <summary>615 cylinders, 4 tracks/cylinder, 17 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 21411840 bytes</summary>
        RD31,
        /// <summary>820 cylinders, 6 tracks/cylinder, 17 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 42823680 bytes</summary>
        RD32,
        /// <summary>306 cylinders, 4 tracks/cylinder, 17 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 10653696 bytes</summary>
        RD51,
        /// <summary>480 cylinders, 7 tracks/cylinder, 18 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 30965760 bytes</summary>
        RD52,
        /// <summary>1024 cylinders, 7 tracks/cylinder, 18 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 75497472 bytes</summary>
        RD53,
        /// <summary>1225 cylinders, 8 tracks/cylinder, 18 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 159936000 bytes</summary>
        RD54,
        /// <summary>411 cylinders, 3 tracks/cylinder, 22 sectors/track, 256 words/sector, 16 bits/word, 512 bytes/sector, 13888512 bytes</summary>
        RK06,
        /// <summary>411 cylinders, 3 tracks/cylinder, 20 sectors/track, 256 words/sector, 18 bits/word, 576 bytes/sector, 14204160 bytes</summary>
        RK06_18,
        /// <summary>815 cylinders, 3 tracks/cylinder, 22 sectors/track, 256 words/sector, 16 bits/word, 512 bytes/sector, 27540480 bytes</summary>
        RK07,
        /// <summary>815 cylinders, 3 tracks/cylinder, 20 sectors/track, 256 words/sector, 18 bits/word, 576 bytes/sector, 28166400 bytes</summary>
        RK07_18,
        /// <summary>823 cylinders, 5 tracks/cylinder, 32 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 67420160 bytes</summary>
        RM02,
        /// <summary>823 cylinders, 5 tracks/cylinder, 32 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 67420160 bytes</summary>
        RM03,
        /// <summary>823 cylinders, 19 tracks/cylinder, 32 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 256196608 bytes</summary>
        RM05,
        /// <summary>203 cylinders, 10 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 22865920 bytes</summary>
        RP02,
        /// <summary>203 cylinders, 10 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector, 23385600 bytes</summary>
        RP02_18,
        /// <summary>400 cylinders, 10 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 45056000 bytes</summary>
        RP03,
        /// <summary>400 cylinders, 10 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector, 46080000 bytes</summary>
        RP03_18,
        /// <summary>411 cylinders, 19 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 87960576 bytes</summary>
        RP04,
        /// <summary>411 cylinders, 19 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector, 89959680 bytes</summary>
        RP04_18,
        /// <summary>411 cylinders, 19 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 87960576 bytes</summary>
        RP05,
        /// <summary>411 cylinders, 19 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector, 89959680 bytes</summary>
        RP05_18,
        /// <summary>815 cylinders, 19 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 174423040 bytes</summary>
        RP06,
        /// <summary>815 cylinders, 19 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector, 178387200 bytes</summary>
        RP06_18,
        #endregion

        #region Generic hard disks
        Microdrive,
        PriamDataTower,
        RDX,
        /// <summary>Imation 320Gb RDX</summary>
        RDX320,
        GENERIC_HDD,
        Zone_HDD,
        // USB flash drives
        FlashDrive
        #endregion Generic hard disks
    };
}
