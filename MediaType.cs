// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MediaType.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common media types.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

// ReSharper disable InconsistentNaming
// TODO: Rename contents

using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

namespace Aaru.CommonTypes
{
    public enum MediaEncoding
    {
        Unknown, FM, MFM,
        M2FM, AppleGCR, CommodoreGCR
    }

    /// <summary>Contains an enumeration of all known types of media.</summary>
    public enum MediaType : uint
    {
        #region Generics, types 0 to 9
        /// <summary>Unknown disk type</summary>
        Unknown = 0,
        /// <summary>Unknown magneto-optical</summary>
        UnknownMO = 1,
        /// <summary>Generic hard disk</summary>
        GENERIC_HDD = 2,
        /// <summary>Microdrive type hard disk</summary>
        Microdrive = 3,
        /// <summary>Zoned hard disk</summary>
        Zone_HDD = 4,
        /// <summary>USB flash drives</summary>
        FlashDrive = 5,
        /// <summary>USB flash drives</summary>
        UnknownTape = 4,
        #endregion Generics, types 0 to 9

        #region Somewhat standard Compact Disc formats, types 10 to 39
        /// <summary>Any unknown or standard violating CD</summary>
        CD = 10,
        /// <summary>CD Digital Audio (Red Book)</summary>
        CDDA = 11,
        /// <summary>CD+G (Red Book)</summary>
        CDG = 12,
        /// <summary>CD+EG (Red Book)</summary>
        CDEG = 13,
        /// <summary>CD-i (Green Book)</summary>
        CDI = 14,
        /// <summary>CD-ROM (Yellow Book)</summary>
        CDROM = 15,
        /// <summary>CD-ROM XA (Yellow Book)</summary>
        CDROMXA = 16,
        /// <summary>CD+ (Blue Book)</summary>
        CDPLUS = 17,
        /// <summary>CD-MO (Orange Book)</summary>
        CDMO = 18,
        /// <summary>CD-Recordable (Orange Book)</summary>
        CDR = 19,
        /// <summary>CD-ReWritable (Orange Book)</summary>
        CDRW = 20,
        /// <summary>Mount-Rainier CD-RW</summary>
        CDMRW = 21,
        /// <summary>Video CD (White Book)</summary>
        VCD = 22,
        /// <summary>Super Video CD (White Book)</summary>
        SVCD = 23,
        /// <summary>Photo CD (Beige Book)</summary>
        PCD = 24,
        /// <summary>Super Audio CD (Scarlet Book)</summary>
        SACD = 25,
        /// <summary>Double-Density CD-ROM (Purple Book)</summary>
        DDCD = 26,
        /// <summary>DD CD-R (Purple Book)</summary>
        DDCDR = 27,
        /// <summary>DD CD-RW (Purple Book)</summary>
        DDCDRW = 28,
        /// <summary>DTS audio CD (non-standard)</summary>
        DTSCD = 29,
        /// <summary>CD-MIDI (Red Book)</summary>
        CDMIDI = 30,
        /// <summary>CD-Video (ISO/IEC 61104)</summary>
        CDV = 31,
        /// <summary>120mm, Phase-Change, 1298496 sectors, 512 bytes/sector, PD650, ECMA-240, ISO 15485</summary>
        PD650 = 32,
        /// <summary>120mm, Write-Once, 1281856 sectors, 512 bytes/sector, PD650, ECMA-240, ISO 15485</summary>
        PD650_WORM = 33,
        /// <summary>
        ///     CD-i Ready, contains a track before the first TOC track, in mode 2, and all TOC tracks are Audio. Subchannel
        ///     marks track as audio pause.
        /// </summary>
        CDIREADY = 34, FMTOWNS = 35,
        #endregion Somewhat standard Compact Disc formats, types 10 to 39

        #region Standard DVD formats, types 40 to 50
        /// <summary>DVD-ROM (applies to DVD Video and DVD Audio)</summary>
        DVDROM = 40,
        /// <summary>DVD-R</summary>
        DVDR = 41,
        /// <summary>DVD-RW</summary>
        DVDRW = 42,
        /// <summary>DVD+R</summary>
        DVDPR = 43,
        /// <summary>DVD+RW</summary>
        DVDPRW = 44,
        /// <summary>DVD+RW DL</summary>
        DVDPRWDL = 45,
        /// <summary>DVD-R DL</summary>
        DVDRDL = 46,
        /// <summary>DVD+R DL</summary>
        DVDPRDL = 47,
        /// <summary>DVD-RAM</summary>
        DVDRAM = 48,
        /// <summary>DVD-RW DL</summary>
        DVDRWDL = 49,
        /// <summary>DVD-Download</summary>
        DVDDownload = 50,
        #endregion Standard DVD formats, types 40 to 50

        #region Standard HD-DVD formats, types 51 to 59
        /// <summary>HD DVD-ROM (applies to HD DVD Video)</summary>
        HDDVDROM = 51,
        /// <summary>HD DVD-RAM</summary>
        HDDVDRAM = 52,
        /// <summary>HD DVD-R</summary>
        HDDVDR = 53,
        /// <summary>HD DVD-RW</summary>
        HDDVDRW = 54,
        /// <summary>HD DVD-R DL</summary>
        HDDVDRDL = 55,
        /// <summary>HD DVD-RW DL</summary>
        HDDVDRWDL = 56,
        #endregion Standard HD-DVD formats, types 51 to 59

        #region Standard Blu-ray formats, types 60 to 69
        /// <summary>BD-ROM (and BD Video)</summary>
        BDROM = 60,
        /// <summary>BD-R</summary>
        BDR = 61,
        /// <summary>BD-RE</summary>
        BDRE = 62,
        /// <summary>BD-R XL</summary>
        BDRXL = 63,
        /// <summary>BD-RE XL</summary>
        BDREXL = 64,
        #endregion Standard Blu-ray formats, types 60 to 69

        #region Rare or uncommon optical standards, types 70 to 79
        /// <summary>Enhanced Versatile Disc</summary>
        EVD = 70,
        /// <summary>Forward Versatile Disc</summary>
        FVD = 71,
        /// <summary>Holographic Versatile Disc</summary>
        HVD = 72,
        /// <summary>China Blue High Definition</summary>
        CBHD = 73,
        /// <summary>High Definition Versatile Multilayer Disc</summary>
        HDVMD = 74,
        /// <summary>Versatile Compact Disc High Density</summary>
        VCDHD = 75,
        /// <summary>Stacked Volumetric Optical Disc</summary>
        SVOD = 76,
        /// <summary>Five Dimensional disc</summary>
        FDDVD = 77,
        /// <summary>China Video Disc</summary>
        CVD = 78,
        #endregion Rare or uncommon optical standards, types 70 to 79

        #region LaserDisc based, types 80 to 89
        /// <summary>Pioneer LaserDisc</summary>
        LD = 80,
        /// <summary>Pioneer LaserDisc data</summary>
        LDROM = 81, LDROM2 = 82, LVROM = 83, MegaLD = 84,
        #endregion LaserDisc based, types 80 to 89

        #region MiniDisc based, types 90 to 99
        /// <summary>Sony Hi-MD</summary>
        HiMD = 90,
        /// <summary>Sony MiniDisc</summary>
        MD = 91,
        /// <summary>Sony MD-Data</summary>
        MDData = 92,
        /// <summary>Sony MD-Data2</summary>
        MDData2 = 93,
        /// <summary>Sony MiniDisc, 60 minutes, formatted with Hi-MD format</summary>
        MD60 = 94,
        /// <summary>Sony MiniDisc, 74 minutes, formatted with Hi-MD format</summary>
        MD74 = 95,
        /// <summary>Sony MiniDisc, 80 minutes, formatted with Hi-MD format</summary>
        MD80 = 96,
        #endregion MiniDisc based, types 90 to 99

        #region Plasmon UDO, types 100 to 109
        /// <summary>5.25", Phase-Change, 1834348 sectors, 8192 bytes/sector, Ultra Density Optical, ECMA-350, ISO 17345</summary>
        UDO = 100,
        /// <summary>5.25", Phase-Change, 3669724 sectors, 8192 bytes/sector, Ultra Density Optical 2, ECMA-380, ISO 11976</summary>
        UDO2 = 101,
        /// <summary>5.25", Write-Once, 3668759 sectors, 8192 bytes/sector, Ultra Density Optical 2, ECMA-380, ISO 11976</summary>
        UDO2_WORM = 102,
        #endregion Plasmon UDO, types 100 to 109

        #region Sony game media, types 110 to 129
        PlayStationMemoryCard = 110, PlayStationMemoryCard2 = 111,
        /// <summary>Sony PlayStation game CD</summary>
        PS1CD = 112,
        /// <summary>Sony PlayStation 2 game CD</summary>
        PS2CD = 113,
        /// <summary>Sony PlayStation 2 game DVD</summary>
        PS2DVD = 114,
        /// <summary>Sony PlayStation 3 game DVD</summary>
        PS3DVD = 115,
        /// <summary>Sony PlayStation 3 game Blu-ray</summary>
        PS3BD = 116,
        /// <summary>Sony PlayStation 4 game Blu-ray</summary>
        PS4BD = 117,
        /// <summary>Sony PlayStation Portable Universal Media Disc (ECMA-365)</summary>
        UMD = 118, PlayStationVitaGameCard = 119,
        #endregion Sony game media, types 110 to 129

        #region Microsoft game media, types 130 to 149
        /// <summary>Microsoft X-box Game Disc</summary>
        XGD = 130,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD2 = 131,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD3 = 132,
        /// <summary>Microsoft X-box One Game Disc</summary>
        XGD4 = 133,
        #endregion Microsoft game media, types 130 to 149

        #region Sega game media, types 150 to 169
        /// <summary>Sega MegaCD</summary>
        MEGACD = 150,
        /// <summary>Sega Saturn disc</summary>
        SATURNCD = 151,
        /// <summary>Sega/Yamaha Gigabyte Disc</summary>
        GDROM = 152,
        /// <summary>Sega/Yamaha recordable Gigabyte Disc</summary>
        GDR = 153, SegaCard = 154, MilCD = 155,
        #endregion Sega game media, types 150 to 169

        #region Other game media, types 170 to 179
        /// <summary>PC-Engine / TurboGrafx cartridge</summary>
        HuCard = 170,
        /// <summary>PC-Engine / TurboGrafx CD</summary>
        SuperCDROM2 = 171,
        /// <summary>Atari Jaguar CD</summary>
        JaguarCD = 172,
        /// <summary>3DO CD</summary>
        ThreeDO = 173,
        /// <summary>NEC PC-FX</summary>
        PCFX = 174,
        /// <summary>NEO-GEO CD</summary>
        NeoGeoCD = 175,
        /// <summary>Commodore CDTV</summary>
        CDTV = 176,
        /// <summary>Amiga CD32</summary>
        CD32 = 177,
        /// <summary>Nuon (DVD based videogame console)</summary>
        Nuon = 178,
        /// <summary>Bandai Playdia</summary>
        Playdia = 179,
        #endregion Other game media, types 170 to 179

        #region Apple standard floppy format, types 180 to 189
        /// <summary>5.25", SS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR</summary>
        Apple32SS = 180,
        /// <summary>5.25", DS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR</summary>
        Apple32DS = 181,
        /// <summary>5.25", SS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR</summary>
        Apple33SS = 182,
        /// <summary>5.25", DS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR</summary>
        Apple33DS = 183,
        /// <summary>3.5", SS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR</summary>
        AppleSonySS = 184,
        /// <summary>3.5", DS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR</summary>
        AppleSonyDS = 185,
        /// <summary>5.25", DS, ?D, ?? tracks, ?? spt, 512 bytes/sector, GCR, opposite side heads, aka Twiggy</summary>
        AppleFileWare = 186,
        #endregion Apple standard floppy format

        #region IBM/Microsoft PC floppy formats, types 190 to 209
        /// <summary>5.25", SS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_525_SS_DD_8 = 190,
        /// <summary>5.25", SS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_525_SS_DD_9 = 191,
        /// <summary>5.25", DS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_525_DS_DD_8 = 192,
        /// <summary>5.25", DS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_525_DS_DD_9 = 193,
        /// <summary>5.25", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        DOS_525_HD = 194,
        /// <summary>3.5", SS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_35_SS_DD_8 = 195,
        /// <summary>3.5", SS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_35_SS_DD_9 = 196,
        /// <summary>3.5", DS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_35_DS_DD_8 = 197,
        /// <summary>3.5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_35_DS_DD_9 = 198,
        /// <summary>3.5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM</summary>
        DOS_35_HD = 199,
        /// <summary>3.5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM</summary>
        DOS_35_ED = 200,
        /// <summary>3.5", DS, HD, 80 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF = 201,
        /// <summary>3.5", DS, HD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF_82 = 202,
        /// <summary>
        ///     5.25", DS, HD, 80 tracks, ? spt, ??? + ??? + ??? bytes/sector, MFM track 0 = ??15 sectors, 512 bytes/sector,
        ///     falsified to DOS as 19 spt, 512 bps
        /// </summary>
        XDF_525 = 203,
        /// <summary>
        ///     3.5", DS, HD, 80 tracks, 4 spt, 8192 + 2048 + 1024 + 512 bytes/sector, MFM track 0 = 19 sectors, 512
        ///     bytes/sector, falsified to DOS as 23 spt, 512 bps
        /// </summary>
        XDF_35 = 204,
        #endregion IBM/Microsoft PC standard floppy formats, types 190 to 209

        #region IBM standard floppy formats, types 210 to 219
        /// <summary>8", SS, SD, 32 tracks, 8 spt, 319 bytes/sector, FM</summary>
        IBM23FD = 210,
        /// <summary>8", SS, SD, 73 tracks, 26 spt, 128 bytes/sector, FM</summary>
        IBM33FD_128 = 211,
        /// <summary>8", SS, SD, 74 tracks, 15 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM33FD_256 = 212,
        /// <summary>8", SS, SD, 74 tracks, 8 spt, 512 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM33FD_512 = 213,
        /// <summary>8", DS, SD, 74 tracks, 26 spt, 128 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM43FD_128 = 214,
        /// <summary>8", DS, SD, 74 tracks, 26 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM43FD_256 = 215,
        /// <summary>
        ///     8", DS, DD, 74 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0
        ///     side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        IBM53FD_256 = 216,
        /// <summary>
        ///     8", DS, DD, 74 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0
        ///     side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        IBM53FD_512 = 217,
        /// <summary>
        ///     8", DS, DD, 74 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0
        ///     side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        IBM53FD_1024 = 218,
        #endregion IBM standard floppy formats, types 210 to 219

        #region DEC standard floppy formats, types 220 to 229
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        RX01 = 220,
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM</summary>
        RX02 = 221,
        /// <summary>8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM</summary>
        RX03 = 222,
        /// <summary>5.25", SS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        RX50 = 223,
        #endregion DEC standard floppy formats, types 220 to 229

        #region Acorn standard floppy formats, types 230 to 239
        /// <summary>5,25", SS, SD, 40 tracks, 10 spt, 256 bytes/sector, FM</summary>
        ACORN_525_SS_SD_40 = 230,
        /// <summary>5,25", SS, SD, 80 tracks, 10 spt, 256 bytes/sector, FM</summary>
        ACORN_525_SS_SD_80 = 231,
        /// <summary>5,25", SS, DD, 40 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_SS_DD_40 = 232,
        /// <summary>5,25", SS, DD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_SS_DD_80 = 233,
        /// <summary>5,25", DS, DD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_DS_DD = 234,
        /// <summary>3,5", DS, DD, 80 tracks, 5 spt, 1024 bytes/sector, MFM</summary>
        ACORN_35_DS_DD = 235,
        /// <summary>3,5", DS, HD, 80 tracks, 10 spt, 1024 bytes/sector, MFM</summary>
        ACORN_35_DS_HD = 236,
        #endregion Acorn standard floppy formats, types 230 to 239

        #region Atari standard floppy formats, types 240 to 249
        /// <summary>5,25", SS, SD, 40 tracks, 18 spt, 128 bytes/sector, FM</summary>
        ATARI_525_SD = 240,
        /// <summary>5,25", SS, ED, 40 tracks, 26 spt, 128 bytes/sector, MFM</summary>
        ATARI_525_ED = 241,
        /// <summary>5,25", SS, DD, 40 tracks, 18 spt, 256 bytes/sector, MFM</summary>
        ATARI_525_DD = 242,
        /// <summary>3,5", SS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_SS_DD = 243,
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_DS_DD = 244,
        /// <summary>3,5", SS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_SS_DD_11 = 245,
        /// <summary>3,5", DS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM</summary>
        ATARI_35_DS_DD_11 = 246,
        #endregion Atari standard floppy formats, types 240 to 249

        #region Commodore standard floppy formats, types 250 to 259
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM (1581)</summary>
        CBM_35_DD = 250,
        /// <summary>3,5", DS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_DD = 251,
        /// <summary>3,5", DS, HD, 80 tracks, 22 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_HD = 252,
        /// <summary>5,25", SS, DD, 35 tracks, GCR</summary>
        CBM_1540 = 253,
        /// <summary>5,25", SS, DD, 40 tracks, GCR</summary>
        CBM_1540_Ext = 254,
        /// <summary>5,25", DS, DD, 35 tracks, GCR</summary>
        CBM_1571 = 255,
        #endregion Commodore standard floppy formats, types 250 to 259

        #region NEC/SHARP standard floppy formats, types 260 to 269
        /// <summary>8", DS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        NEC_8_SD = 260,
        /// <summary>8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, MFM</summary>
        NEC_8_DD = 261,
        /// <summary>5.25", SS, SD, 80 tracks, 16 spt, 256 bytes/sector, FM</summary>
        NEC_525_SS = 262,
        /// <summary>5.25", DS, SD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        NEC_525_DS = 263,
        /// <summary>5,25", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_525_HD = 264,
        /// <summary>3,5", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM, aka mode 3</summary>
        NEC_35_HD_8 = 265,
        /// <summary>3,5", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        NEC_35_HD_15 = 266,
        /// <summary>3,5", DS, TD, 240 tracks, 38 spt, 512 bytes/sector, MFM</summary>
        NEC_35_TD = 267,
        /// <summary>5,25", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        SHARP_525 = NEC_525_HD,
        /// <summary>3,5", DS, HD, 80 tracks, 9 spt, 1024 bytes/sector, MFM</summary>
        SHARP_525_9 = 268,
        /// <summary>3,5", DS, HD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        SHARP_35 = NEC_35_HD_8,
        /// <summary>3,5", DS, HD, 80 tracks, 9 spt, 1024 bytes/sector, MFM</summary>
        SHARP_35_9 = 269,
        #endregion NEC/SHARP standard floppy formats, types 260 to 269

        #region ECMA floppy standards, types 270 to 289
        /// <summary>
        ///     5,25", DS, DD, 80 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track
        ///     0 side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        ECMA_99_8 = 270,
        /// <summary>
        ///     5,25", DS, DD, 77 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track
        ///     0 side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        ECMA_99_15 = 271,
        /// <summary>
        ///     5,25", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track
        ///     0 side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        ECMA_99_26 = 272,
        /// <summary>3,5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        ECMA_100 = DOS_35_DS_DD_9,
        /// <summary>3,5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM</summary>
        ECMA_125 = DOS_35_HD,
        /// <summary>3,5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM</summary>
        ECMA_147 = DOS_35_ED,
        /// <summary>8", SS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        ECMA_54 = 273,
        /// <summary>8", DS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        ECMA_59 = 274,
        /// <summary>5,25", SS, DD, 35 tracks, 9 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector</summary>
        ECMA_66 = 275,
        /// <summary>
        ///     8", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0
        ///     side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        ECMA_69_8 = 276,
        /// <summary>
        ///     8", DS, DD, 77 tracks, 15 spt, 512 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0
        ///     side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        ECMA_69_15 = 277,
        /// <summary>
        ///     8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0
        ///     side 1 = 26 sectors, 256 bytes/sector
        /// </summary>
        ECMA_69_26 = 278,
        /// <summary>
        ///     5,25", DS, DD, 40 tracks, 16 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector, track 0
        ///     side 1 = 16 sectors, 256 bytes/sector
        /// </summary>
        ECMA_70 = 279,
        /// <summary>
        ///     5,25", DS, DD, 80 tracks, 16 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector, track 0
        ///     side 1 = 16 sectors, 256 bytes/sector
        /// </summary>
        ECMA_78 = 280,
        /// <summary>5,25", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, FM</summary>
        ECMA_78_2 = 281,
        #endregion ECMA floppy standards, types 270 to 289

        #region Non-standard PC formats (FDFORMAT, 2M, etc), types 290 to 308
        /// <summary>5,25", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_DD = 290,
        /// <summary>5,25", DS, HD, 82 tracks, 17 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_HD = 291,
        /// <summary>3,5", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_DD = 292,
        /// <summary>3,5", DS, HD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_HD = 293,
        #endregion Non-standard PC formats (FDFORMAT, 2M, etc), types 290 to 308

        #region Apricot ACT standard floppy formats, type 309
        /// <summary>3.5", DS, DD, 70 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        Apricot_35 = 309,
        #endregion Apricot ACT standard floppy formats, type 309

        #region OnStream ADR, types 310 to 319
        ADR2120 = 310, ADR260 = 311, ADR30 = 312,
        ADR50   = 313,
        #endregion OnStream ADR, types 310 to 319

        #region Advanced Intelligent Tape, types 320 to 339
        AIT1      = 320, AIT1Turbo = 321, AIT2   = 322,
        AIT2Turbo = 323, AIT3      = 324, AIT3Ex = 325,
        AIT3Turbo = 326, AIT4      = 327, AIT5   = 328,
        AITETurbo = 329, SAIT1     = 330, SAIT2  = 331,
        #endregion Advanced Intelligent Tape, types 320 to 339

        #region Iomega, types 340 to 359
        /// <summary>Obsolete type for 8"x11" Bernoulli Box disk</summary>
        [Obsolete]
        Bernoulli = 340,
        /// <summary>Obsolete type for 5⅓" Bernoulli Box II disks</summary>
        [Obsolete]
        Bernoulli2 = 341, Ditto = 342, DittoMax  = 343, Jaz    = 344,
        Jaz2                    = 345, PocketZip = 346, REV120 = 347,
        REV35                   = 348, REV70     = 349, ZIP100 = 350,
        ZIP250                  = 351, ZIP750    = 352,
        /// <summary>5⅓" Bernoulli Box II disk with 35Mb capacity</summary>
        Bernoulli35 = 353,
        /// <summary>5⅓" Bernoulli Box II disk with 44Mb capacity</summary>
        Bernoulli44 = 354,
        /// <summary>5⅓" Bernoulli Box II disk with 65Mb capacity</summary>
        Bernoulli65 = 355,
        /// <summary>5⅓" Bernoulli Box II disk with 90Mb capacity</summary>
        Bernoulli90 = 356,
        /// <summary>5⅓" Bernoulli Box II disk with 105Mb capacity</summary>
        Bernoulli105 = 357,
        /// <summary>5⅓" Bernoulli Box II disk with 150Mb capacity</summary>
        Bernoulli150 = 358,
        /// <summary>5⅓" Bernoulli Box II disk with 230Mb capacity</summary>
        Bernoulli230 = 359,
        #endregion Iomega, types 340 to 359

        #region Audio or video media, types 360 to 369
        CompactCassette = 360, Data8 = 361, MiniDV = 362,
        /// <summary>D/CAS-25: Digital data on Compact Cassette form factor, special magnetic media, 9-track</summary>
        Dcas25 = 363,
        /// <summary>D/CAS-85: Digital data on Compact Cassette form factor, special magnetic media, 17-track</summary>
        Dcas85 = 364,
        /// <summary>D/CAS-103: Digital data on Compact Cassette form factor, special magnetic media, 21-track</summary>
        Dcas103 = 365,
        #endregion Audio media, types 360 to 369

        #region CompactFlash Association, types 370 to 379
        CFast = 370, CompactFlash = 371, CompactFlashType2 = 372,
        #endregion CompactFlash Association, types 370 to 379

        #region Digital Audio Tape / Digital Data Storage, types 380 to 389
        DigitalAudioTape = 380, DAT160 = 381, DAT320 = 382,
        DAT72            = 383, DDS1   = 384, DDS2   = 385,
        DDS3             = 386, DDS4   = 387,
        #endregion Digital Audio Tape / Digital Data Storage, types 380 to 389

        #region DEC, types 390 to 399
        CompactTapeI = 390, CompactTapeII = 391, DECtapeII = 392,
        DLTtapeIII   = 393, DLTtapeIIIxt  = 394, DLTtapeIV = 395,
        DLTtapeS4    = 396, SDLT1         = 397, SDLT2     = 398,
        VStapeI      = 399,
        #endregion DEC, types 390 to 399

        #region Exatape, types 400 to 419
        Exatape15m  = 400, Exatape22m  = 401, Exatape22mAME = 402,
        Exatape28m  = 403, Exatape40m  = 404, Exatape45m    = 405,
        Exatape54m  = 406, Exatape75m  = 407, Exatape76m    = 408,
        Exatape80m  = 409, Exatape106m = 410, Exatape160mXL = 411,
        Exatape112m = 412, Exatape125m = 413, Exatape150m   = 414,
        Exatape170m = 415, Exatape225m = 416,
        #endregion Exatape, types 400 to 419

        #region PCMCIA / ExpressCard, types 420 to 429
        ExpressCard34 = 420, ExpressCard54 = 421, PCCardTypeI  = 422,
        PCCardTypeII  = 423, PCCardTypeIII = 424, PCCardTypeIV = 425,
        #endregion PCMCIA / ExpressCard, types 420 to 429

        #region SyQuest, types 430 to 449
        /// <summary>SyQuest 135Mb cartridge for use in EZ135 and EZFlyer drives</summary>
        EZ135 = 430,
        /// <summary>SyQuest EZFlyer 230Mb cartridge for use in EZFlyer drive</summary>
        EZ230 = 431,
        /// <summary>SyQuest 4.7Gb for use in Quest drive</summary>
        Quest = 432,
        /// <summary>SyQuest SparQ 1Gb cartridge</summary>
        SparQ = 433,
        /// <summary>SyQuest 5Mb cartridge for SQ306RD drive</summary>
        SQ100 = 434,
        /// <summary>SyQuest 10Mb cartridge for SQ312RD drive</summary>
        SQ200 = 435,
        /// <summary>SyQuest 15Mb cartridge for SQ319RD drive</summary>
        SQ300 = 436,
        /// <summary>SyQuest 105Mb cartridge for SQ3105 and SQ3270 drives</summary>
        SQ310 = 437,
        /// <summary>SyQuest 270Mb cartridge for SQ3270 drive</summary>
        SQ327 = 438,
        /// <summary>SyQuest 44Mb cartridge for SQ555, SQ5110 and SQ5200C/SQ200 drives</summary>
        SQ400 = 439,
        /// <summary>SyQuest 88Mb cartridge for SQ5110 and SQ5200C/SQ200 drives</summary>
        SQ800 = 440,
        /// <summary>SyQuest 1.5Gb cartridge for SyJet drive</summary>
        [Obsolete]
        SQ1500 = 441,
        /// <summary>SyQuest 200Mb cartridge for use in SQ5200C drive</summary>
        SQ2000 = 442,
        /// <summary>SyQuest 1.5Gb cartridge for SyJet drive</summary>
        SyJet = 443,
        #endregion SyQuest, types 430 to 449

        #region Nintendo, types 450 to 469
        FamicomGamePak = 450, GameBoyAdvanceGamePak = 451, GameBoyGamePak = 452,
        /// <summary>Nintendo GameCube Optical Disc</summary>
        GOD = 453, N64DD    = 454, N64GamePak       = 455, NESGamePak         = 456,
        Nintendo3DSGameCard = 457, NintendoDiskCard = 458, NintendoDSGameCard = 459,
        NintendoDSiGameCard = 460, SNESGamePak      = 461, SNESGamePakUS      = 462,
        /// <summary>Nintendo Wii Optical Disc</summary>
        WOD = 463,
        /// <summary>Nintendo Wii U Optical Disc</summary>
        WUOD = 464, SwitchGameCard = 465,
        #endregion Nintendo, types 450 to 469

        #region IBM Tapes, types 470 to 479
        IBM3470  = 470, IBM3480 = 471, IBM3490 = 472,
        IBM3490E = 473, IBM3592 = 474,
        #endregion IBM Tapes, types 470 to 479

        #region LTO Ultrium, types 480 to 509
        LTO      = 480, LTO2     = 481, LTO3     = 482,
        LTO3WORM = 483, LTO4     = 484, LTO4WORM = 485,
        LTO5     = 486, LTO5WORM = 487, LTO6     = 488,
        LTO6WORM = 489, LTO7     = 490, LTO7WORM = 491,
        #endregion LTO Ultrium, types 480 to 509

        #region MemoryStick, types 510 to 519
        MemoryStick    = 510, MemoryStickDuo    = 511, MemoryStickMicro = 512,
        MemoryStickPro = 513, MemoryStickProDuo = 514,
        #endregion MemoryStick, types 510 to 519

        #region SecureDigital, types 520 to 529
        microSD = 520, miniSD = 521, SecureDigital = 522,
        #endregion SecureDigital, types 520 to 529

        #region MultiMediaCard, types 530 to 539
        MMC     = 530, MMCmicro  = 531, RSMMC = 532,
        MMCplus = 533, MMCmobile = 534,
        #endregion MultiMediaCard, types 530 to 539

        #region SLR, types 540 to 569
        MLR1        = 540, MLR1SL     = 541, MLR3       = 542,
        SLR1        = 543, SLR2       = 544, SLR3       = 545,
        SLR32       = 546, SLR32SL    = 547, SLR4       = 548,
        SLR5        = 549, SLR5SL     = 550, SLR6       = 551,
        SLRtape7    = 552, SLRtape7SL = 553, SLRtape24  = 554,
        SLRtape24SL = 555, SLRtape40  = 556, SLRtape50  = 557,
        SLRtape60   = 558, SLRtape75  = 559, SLRtape100 = 560,
        SLRtape140  = 561,
        #endregion SLR, types 540 to 569

        #region QIC, types 570 to 589
        QIC11   = 570, QIC120  = 571, QIC1350 = 572,
        QIC150  = 573, QIC24   = 574, QIC3010 = 575,
        QIC3020 = 576, QIC3080 = 577, QIC3095 = 578,
        QIC320  = 579, QIC40   = 580, QIC525  = 581,
        QIC80   = 582,
        #endregion QIC, types 570 to 589

        #region StorageTek tapes, types 590 to 609
        STK4480 = 590, STK4490 = 591, STK9490 = 592,
        T9840A  = 593, T9840B  = 594, T9840C  = 595,
        T9840D  = 596, T9940A  = 597, T9940B  = 598,
        T10000A = 599, T10000B = 600, T10000C = 601,
        T10000D = 602,
        #endregion StorageTek tapes, types 590 to 609

        #region Travan, types 610 to 619
        Travan    = 610, Travan1Ex = 611, Travan3 = 612,
        Travan3Ex = 613, Travan4   = 614, Travan5 = 615,
        Travan7   = 616,
        #endregion Travan, types 610 to 619

        #region VXA, types 620 to 629
        VXA1 = 620, VXA2 = 621, VXA3 = 622,
        #endregion VXA, types 620 to 629

        #region Magneto-optical, types 630 to 659
        /// <summary>5,25", M.O., WORM, 650Mb, 318750 sectors, 1024 bytes/sector, ECMA-153, ISO 11560</summary>
        ECMA_153 = 630,
        /// <summary>5,25", M.O., WORM, 600Mb, 581250 sectors, 512 bytes/sector, ECMA-153, ISO 11560</summary>
        ECMA_153_512 = 631,
        /// <summary>3,5", M.O., RW, 128Mb, 248826 sectors, 512 bytes/sector, ECMA-154, ISO 10090</summary>
        ECMA_154 = 632,
        /// <summary>5,25", M.O., RW/WORM, 1Gb, 904995 sectors, 512 bytes/sector, ECMA-183, ISO 13481</summary>
        ECMA_183_512 = 633,
        /// <summary>5,25", M.O., RW/WORM, 1Gb, 498526 sectors, 1024 bytes/sector, ECMA-183, ISO 13481</summary>
        ECMA_183 = 634,
        /// <summary>5,25", M.O., RW/WORM, 1.2Gb, 1165600 sectors, 512 bytes/sector, ECMA-184, ISO 13549</summary>
        ECMA_184_512 = 635,
        /// <summary>5,25", M.O., RW/WORM, 1.3Gb, 639200 sectors, 1024 bytes/sector, ECMA-184, ISO 13549</summary>
        ECMA_184 = 636,
        /// <summary>300mm, M.O., WORM, ??? sectors, 1024 bytes/sector, ECMA-189, ISO 13614</summary>
        ECMA_189 = 637,
        /// <summary>300mm, M.O., WORM, ??? sectors, 1024 bytes/sector, ECMA-190, ISO 13403</summary>
        ECMA_190 = 638,
        /// <summary>5,25", M.O., RW/WORM, 936921 or 948770 sectors, 1024 bytes/sector, ECMA-195, ISO 13842</summary>
        ECMA_195 = 639,
        /// <summary>5,25", M.O., RW/WORM, 1644581 or 1647371 sectors, 512 bytes/sector, ECMA-195, ISO 13842</summary>
        ECMA_195_512 = 640,
        /// <summary>3,5", M.O., 446325 sectors, 512 bytes/sector, ECMA-201, ISO 13963</summary>
        ECMA_201 = 641,
        /// <summary>3,5", M.O., 429975 sectors, 512 bytes/sector, embossed, ISO 13963</summary>
        ECMA_201_ROM = 642,
        /// <summary>3,5", M.O., 371371 sectors, 1024 bytes/sector, ECMA-223</summary>
        ECMA_223 = 643,
        /// <summary>3,5", M.O., 694929 sectors, 512 bytes/sector, ECMA-223</summary>
        ECMA_223_512 = 644,
        /// <summary>5,25", M.O., 1244621 sectors, 1024 bytes/sector, ECMA-238, ISO 15486</summary>
        ECMA_238 = 645,
        /// <summary>3,5", M.O., 310352, 320332 or 321100 sectors, 2048 bytes/sector, ECMA-239, ISO 15498</summary>
        ECMA_239 = 646,
        /// <summary>356mm, M.O., 14476734 sectors, 1024 bytes/sector, ECMA-260, ISO 15898</summary>
        ECMA_260 = 647,
        /// <summary>356mm, M.O., 24445990 sectors, 1024 bytes/sector, ECMA-260, ISO 15898</summary>
        ECMA_260_Double = 648,
        /// <summary>5,25", M.O., 1128134 sectors, 2048 bytes/sector, ECMA-280, ISO 18093</summary>
        ECMA_280 = 649,
        /// <summary>300mm, M.O., 7355716 sectors, 2048 bytes/sector, ECMA-317, ISO 20162</summary>
        ECMA_317 = 650,
        /// <summary>5,25", M.O., 1095840 sectors, 4096 bytes/sector, ECMA-322, ISO 22092, 9.1Gb/cart</summary>
        ECMA_322 = 651,
        /// <summary>5,25", M.O., 2043664 sectors, 2048 bytes/sector, ECMA-322, ISO 22092, 8.6Gb/cart</summary>
        ECMA_322_2k = 652,
        /// <summary>3,5", M.O., 605846 sectors, 2048 bytes/sector, Cherry Book, GigaMo, ECMA-351, ISO 17346</summary>
        GigaMo = 653,
        /// <summary>3,5", M.O., 1063146 sectors, 2048 bytes/sector, Cherry Book 2, GigaMo 2, ECMA-353, ISO 22533</summary>
        GigaMo2 = 654,
        /// <summary>5,25", M.O., 1263472 sectors, 2048 bytes/sector, ISO 15286, 5.2Gb/cart</summary>
        ISO_15286 = 655,
        /// <summary>5,25", M.O., 2319786 sectors, 1024 bytes/sector, ISO 15286, 4.8Gb/cart</summary>
        ISO_15286_1024 = 656,
        /// <summary>5,25", M.O., ??????? sectors, 512 bytes/sector, ISO 15286, 4.1Gb/cart</summary>
        ISO_15286_512 = 657,
        /// <summary>5,25", M.O., 314569 sectors, 1024 bytes/sector, ISO 10089, 650Mb/cart</summary>
        ISO_10089 = 658,
        /// <summary>5,25", M.O., ?????? sectors, 512 bytes/sector, ISO 10089, 594Mb/cart</summary>
        ISO_10089_512 = 659,
        #endregion Magneto-optical, types 630 to 659

        #region Other floppy standards, types 660 to 689
        CompactFloppy = 660, DemiDiskette = 661,
        /// <summary>3.5", 652 tracks, 2 sides, 512 bytes/sector, Floptical, ECMA-207, ISO 14169</summary>
        Floptical = 662, HiFD = 663, QuickDisk = 664, UHD144       = 665,
        VideoFloppy           = 666, Wafer     = 667, ZXMicrodrive = 668,
        #endregion Other floppy standards, types 660 to 669

        #region Miscellaneous, types 670 to 689
        BeeCard    = 670, Borsu       = 671, DataStore   = 672,
        DIR        = 673, DST         = 674, DTF         = 675,
        DTF2       = 676, Flextra3020 = 677, Flextra3225 = 678,
        HiTC1      = 679, HiTC2       = 680, LT1         = 681,
        MiniCard   = 872, Orb         = 683, Orb5        = 684,
        SmartMedia = 685, xD          = 686, XQD         = 687,
        DataPlay   = 688,
        #endregion Miscellaneous, types 670 to 689

        #region Apple specific media, types 690 to 699
        AppleProfile   = 690, AppleWidget = 691, AppleHD20 = 692,
        PriamDataTower = 693, Pippin      = 694,
        #endregion Apple specific media, types 690 to 699

        #region DEC hard disks, types 700 to 729
        /// <summary>
        ///     2382 cylinders, 4 tracks/cylinder, 42 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     204890112 bytes
        /// </summary>
        RA60 = 700,
        /// <summary>
        ///     546 cylinders, 14 tracks/cylinder, 31 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     121325568 bytes
        /// </summary>
        RA80 = 701,
        /// <summary>
        ///     1248 cylinders, 14 tracks/cylinder, 51 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     456228864 bytes
        /// </summary>
        RA81 = 702,
        /// <summary>
        ///     302 cylinders, 4 tracks/cylinder, 42 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 25976832
        ///     bytes
        /// </summary>
        RC25 = 703,
        /// <summary>
        ///     615 cylinders, 4 tracks/cylinder, 17 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 21411840
        ///     bytes
        /// </summary>
        RD31 = 704,
        /// <summary>
        ///     820 cylinders, 6 tracks/cylinder, 17 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 42823680
        ///     bytes
        /// </summary>
        RD32 = 705,
        /// <summary>
        ///     306 cylinders, 4 tracks/cylinder, 17 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 10653696
        ///     bytes
        /// </summary>
        RD51 = 706,
        /// <summary>
        ///     480 cylinders, 7 tracks/cylinder, 18 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 30965760
        ///     bytes
        /// </summary>
        RD52 = 707,
        /// <summary>
        ///     1024 cylinders, 7 tracks/cylinder, 18 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     75497472 bytes
        /// </summary>
        RD53 = 708,
        /// <summary>
        ///     1225 cylinders, 8 tracks/cylinder, 18 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     159936000 bytes
        /// </summary>
        RD54 = 709,
        /// <summary>
        ///     411 cylinders, 3 tracks/cylinder, 22 sectors/track, 256 words/sector, 16 bits/word, 512 bytes/sector, 13888512
        ///     bytes
        /// </summary>
        RK06 = 710,
        /// <summary>
        ///     411 cylinders, 3 tracks/cylinder, 20 sectors/track, 256 words/sector, 18 bits/word, 576 bytes/sector, 14204160
        ///     bytes
        /// </summary>
        RK06_18 = 711,
        /// <summary>
        ///     815 cylinders, 3 tracks/cylinder, 22 sectors/track, 256 words/sector, 16 bits/word, 512 bytes/sector, 27540480
        ///     bytes
        /// </summary>
        RK07 = 712,
        /// <summary>
        ///     815 cylinders, 3 tracks/cylinder, 20 sectors/track, 256 words/sector, 18 bits/word, 576 bytes/sector, 28166400
        ///     bytes
        /// </summary>
        RK07_18 = 713,
        /// <summary>
        ///     823 cylinders, 5 tracks/cylinder, 32 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 67420160
        ///     bytes
        /// </summary>
        RM02 = 714,
        /// <summary>
        ///     823 cylinders, 5 tracks/cylinder, 32 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector, 67420160
        ///     bytes
        /// </summary>
        RM03 = 715,
        /// <summary>
        ///     823 cylinders, 19 tracks/cylinder, 32 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     256196608 bytes
        /// </summary>
        RM05 = 716,
        /// <summary>
        ///     203 cylinders, 10 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     22865920 bytes
        /// </summary>
        RP02 = 717,
        /// <summary>
        ///     203 cylinders, 10 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector,
        ///     23385600 bytes
        /// </summary>
        RP02_18 = 718,
        /// <summary>
        ///     400 cylinders, 10 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     45056000 bytes
        /// </summary>
        RP03 = 719,
        /// <summary>
        ///     400 cylinders, 10 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector,
        ///     46080000 bytes
        /// </summary>
        RP03_18 = 720,
        /// <summary>
        ///     411 cylinders, 19 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     87960576 bytes
        /// </summary>
        RP04 = 721,
        /// <summary>
        ///     411 cylinders, 19 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector,
        ///     89959680 bytes
        /// </summary>
        RP04_18 = 722,
        /// <summary>
        ///     411 cylinders, 19 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     87960576 bytes
        /// </summary>
        RP05 = 723,
        /// <summary>
        ///     411 cylinders, 19 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector,
        ///     89959680 bytes
        /// </summary>
        RP05_18 = 724,
        /// <summary>
        ///     815 cylinders, 19 tracks/cylinder, 22 sectors/track, 128 words/sector, 32 bits/word, 512 bytes/sector,
        ///     174423040 bytes
        /// </summary>
        RP06 = 725,
        /// <summary>
        ///     815 cylinders, 19 tracks/cylinder, 20 sectors/track, 128 words/sector, 36 bits/word, 576 bytes/sector,
        ///     178387200 bytes
        /// </summary>
        RP06_18 = 726,
        #endregion DEC hard disks, types 700 to 729

        #region Imation, types 730 to 739
        LS120 = 730, LS240 = 731, FD32MB = 732,
        RDX   = 733,
        /// <summary>Imation 320Gb RDX</summary>
        RDX320 = 734,
        #endregion Imation, types 730 to 739

        #region VideoNow, types 740 to 749
        VideoNow = 740, VideoNowColor = 741, VideoNowXp = 742,
        #endregion

        #region Iomega, types 750 to 759
        /// <summary>8"x11" Bernoulli Box disk with 10Mb capacity</summary>
        Bernoulli10 = 750,
        /// <summary>8"x11" Bernoulli Box disk with 20Mb capacity</summary>
        Bernoulli20 = 751,
        /// <summary>5⅓" Bernoulli Box II disk with 20Mb capacity</summary>
        BernoulliBox2_20 = 752,
        #endregion Iomega, types 750 to 759

        #region Kodak, types 760 to 769
        KodakVerbatim3 = 760, KodakVerbatim6 = 761, KodakVerbatim12 = 762,
        #endregion Kodak, types 760 to 769

        #region Sony and Panasonic Blu-ray derived, types 770 to 799
        /// <summary>Professional Disc for video, single layer, rewritable, 23Gb</summary>
        ProfessionalDisc = 770,
        /// <summary>Professional Disc for video, dual layer, rewritable, 50Gb</summary>
        ProfessionalDiscDual = 771,
        /// <summary>Professional Disc for video, triple layer, rewritable, 100Gb</summary>
        ProfessionalDiscTriple = 772,
        /// <summary>Professional Disc for video, quad layer, write once, 128Gb</summary>
        ProfessionalDiscQuad = 773,
        /// <summary>Professional Disc for DATA, single layer, rewritable, 23Gb</summary>
        PDD = 774,
        /// <summary>Professional Disc for DATA, single layer, write once, 23Gb</summary>
        PDD_WORM = 775,
        /// <summary>Archival Disc, 1st gen., 300Gb</summary>
        ArchivalDisc = 776,
        /// <summary>Archival Disc, 2nd gen., 500Gb</summary>
        ArchivalDisc2 = 777,
        /// <summary>Archival Disc, 3rd gen., 1Tb</summary>
        ArchivalDisc3 = 778,
        /// <summary>Optical Disc archive, 1st gen., write once, 300Gb</summary>
        ODC300R = 779,
        /// <summary>Optical Disc archive, 1st gen., rewritable, 300Gb</summary>
        ODC300RE = 780,
        /// <summary>Optical Disc archive, 2nd gen., write once, 600Gb</summary>
        ODC600R = 781,
        /// <summary>Optical Disc archive, 2nd gen., rewritable, 600Gb</summary>
        ODC600RE = 782,
        /// <summary>Optical Disc archive, 3rd gen., rewritable, 1200Gb</summary>
        ODC1200RE = 783,
        /// <summary>Optical Disc archive, 3rd gen., write once, 1500Gb</summary>
        ODC1500R = 784,
        /// <summary>Optical Disc archive, 4th gen., write once, 3300Gb</summary>
        ODC3300R = 785,
        /// <summary>Optical Disc archive, 5th gen., write once, 5500Gb</summary>
        ODC5500R = 786,
        #endregion Sony and Panasonic Blu-ray derived, types 770 to 799

        #region Magneto-optical, types 800 to 819
        /// <summary>5,25", M.O., 4383356 sectors, 1024 bytes/sector, ECMA-322, ISO 22092, 9.1Gb/cart</summary>
        ECMA_322_1k = 800,
        /// <summary>5,25", M.O., ??????? sectors, 512 bytes/sector, ECMA-322, ISO 22092, 9.1Gb/cart</summary>
        ECMA_322_512 = 801,
        /// <summary>5,25", M.O., 1273011 sectors, 1024 bytes/sector, ISO 14517, 2.6Gb/cart</summary>
        ISO_14517 = 802,
        /// <summary>5,25", M.O., 2244958 sectors, 512 bytes/sector, ISO 14517, 2.3Gb/cart</summary>
        ISO_14517_512 = 803,
        #endregion Magneto-optical, types 800 to 819
    }
}