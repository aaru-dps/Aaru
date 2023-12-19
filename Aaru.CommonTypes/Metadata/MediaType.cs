// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MediaType.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Converts a common media type to the XML equivalent.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

#pragma warning disable 612
namespace Aaru.CommonTypes.Metadata;

/// <summary>Handles media type for metadata</summary>
public static class MediaType
{
    /// <summary>Converts a media type of a pair of type and subtype strings to use in metadata</summary>
    /// <param name="dskType">Media type</param>
    /// <returns>Media type and subtype for metadata</returns>
    public static (string type, string subType) MediaTypeToString(CommonTypes.MediaType dskType)
    {
        string discType;
        string discSubType;

        switch(dskType)
        {
            case CommonTypes.MediaType.BDR:
                discType    = "Blu-ray";
                discSubType = "BD-R";

                break;
            case CommonTypes.MediaType.BDRE:
                discType    = "Blu-ray";
                discSubType = "BD-RE";

                break;
            case CommonTypes.MediaType.BDREXL:
                discType    = "Blu-ray";
                discSubType = "BD-RE XL";

                break;
            case CommonTypes.MediaType.BDROM:
                discType    = "Blu-ray";
                discSubType = "BD-ROM";

                break;
            case CommonTypes.MediaType.BDRXL:
                discType    = "Blu-ray";
                discSubType = "BD-R XL";

                break;
            case CommonTypes.MediaType.UHDBD:
                discType    = "Blu-ray";
                discSubType = "Ultra HD Blu-ray";

                break;
            case CommonTypes.MediaType.CBHD:
                discType    = "Blu-ray";
                discSubType = "CBHD";

                break;
            case CommonTypes.MediaType.CD:
                discType    = "Compact Disc";
                discSubType = "CD";

                break;
            case CommonTypes.MediaType.CDDA:
                discType    = "Compact Disc";
                discSubType = "CD Digital Audio";

                break;
            case CommonTypes.MediaType.CDEG:
                discType    = "Compact Disc";
                discSubType = "CD+EG";

                break;
            case CommonTypes.MediaType.CDG:
                discType    = "Compact Disc";
                discSubType = "CD+G";

                break;
            case CommonTypes.MediaType.CDI:
                discType    = "Compact Disc";
                discSubType = "CD-i";

                break;
            case CommonTypes.MediaType.CDIREADY:
                discType    = "Compact Disc";
                discSubType = "CD-i Ready";

                break;
            case CommonTypes.MediaType.CDMIDI:
                discType    = "Compact Disc";
                discSubType = "CD+MIDI";

                break;
            case CommonTypes.MediaType.CDMO:
                discType    = "Compact Disc";
                discSubType = "CD-MO";

                break;
            case CommonTypes.MediaType.CDMRW:
                discType    = "Compact Disc";
                discSubType = "CD-MRW";

                break;
            case CommonTypes.MediaType.CDPLUS:
                discType    = "Compact Disc";
                discSubType = "CD+";

                break;
            case CommonTypes.MediaType.CDR:
                discType    = "Compact Disc";
                discSubType = "CD-R";

                break;
            case CommonTypes.MediaType.CDROM:
                discType    = "Compact Disc";
                discSubType = "CD-ROM";

                break;
            case CommonTypes.MediaType.CDROMXA:
                discType    = "Compact Disc";
                discSubType = "CD-ROM XA";

                break;
            case CommonTypes.MediaType.CDRW:
                discType    = "Compact Disc";
                discSubType = "CD-RW";

                break;
            case CommonTypes.MediaType.CDV:
                discType    = "Compact Disc";
                discSubType = "CD-Video";

                break;
            case CommonTypes.MediaType.DDCD:
                discType    = "DDCD";
                discSubType = "DDCD";

                break;
            case CommonTypes.MediaType.DDCDR:
                discType    = "DDCD";
                discSubType = "DDCD-R";

                break;
            case CommonTypes.MediaType.DDCDRW:
                discType    = "DDCD";
                discSubType = "DDCD-RW";

                break;
            case CommonTypes.MediaType.DTSCD:
                discType    = "Compact Disc";
                discSubType = "DTS CD";

                break;
            case CommonTypes.MediaType.DVDDownload:
                discType    = "DVD";
                discSubType = "DVD-Download";

                break;
            case CommonTypes.MediaType.DVDPR:
                discType    = "DVD";
                discSubType = "DVD+R";

                break;
            case CommonTypes.MediaType.DVDPRDL:
                discType    = "DVD";
                discSubType = "DVD+R DL";

                break;
            case CommonTypes.MediaType.DVDPRW:
                discType    = "DVD";
                discSubType = "DVD+RW";

                break;
            case CommonTypes.MediaType.DVDPRWDL:
                discType    = "DVD";
                discSubType = "DVD+RW DL";

                break;
            case CommonTypes.MediaType.DVDR:
                discType    = "DVD";
                discSubType = "DVD-R";

                break;
            case CommonTypes.MediaType.DVDRAM:
                discType    = "DVD";
                discSubType = "DVD-RAM";

                break;
            case CommonTypes.MediaType.DVDRDL:
                discType    = "DVD";
                discSubType = "DVD-R DL";

                break;
            case CommonTypes.MediaType.DVDROM:
                discType    = "DVD";
                discSubType = "DVD-ROM";

                break;
            case CommonTypes.MediaType.DVDRW:
                discType    = "DVD";
                discSubType = "DVD-RW";

                break;
            case CommonTypes.MediaType.DVDRWDL:
                discType    = "DVD";
                discSubType = "DVD-RW DL";

                break;
            case CommonTypes.MediaType.EVD:
                discType    = "EVD";
                discSubType = "EVD";

                break;
            case CommonTypes.MediaType.FDDVD:
                discType    = "FDDVD";
                discSubType = "FDDVD";

                break;
            case CommonTypes.MediaType.FVD:
                discType    = "FVD";
                discSubType = "FVD";

                break;
            case CommonTypes.MediaType.GDR:
                discType    = "GD";
                discSubType = "GD-R";

                break;
            case CommonTypes.MediaType.GDROM:
                discType    = "GD";
                discSubType = "GD-ROM";

                break;
            case CommonTypes.MediaType.GOD:
                discType    = "DVD";
                discSubType = "GameCube Game Disc";

                break;
            case CommonTypes.MediaType.WOD:
                discType    = "DVD";
                discSubType = "Wii Optical Disc";

                break;
            case CommonTypes.MediaType.WUOD:
                discType    = "Blu-ray";
                discSubType = "Wii U Optical Disc";

                break;
            case CommonTypes.MediaType.HDDVDR:
                discType    = "HD DVD";
                discSubType = "HD DVD-R";

                break;
            case CommonTypes.MediaType.HDDVDRAM:
                discType    = "HD DVD";
                discSubType = "HD DVD-RAM";

                break;
            case CommonTypes.MediaType.HDDVDRDL:
                discType    = "HD DVD";
                discSubType = "HD DVD-R DL";

                break;
            case CommonTypes.MediaType.HDDVDROM:
                discType    = "HD DVD";
                discSubType = "HD DVD-ROM";

                break;
            case CommonTypes.MediaType.HDDVDRW:
                discType    = "HD DVD";
                discSubType = "HD DVD-RW";

                break;
            case CommonTypes.MediaType.HDDVDRWDL:
                discType    = "HD DVD";
                discSubType = "HD DVD-RW DL";

                break;
            case CommonTypes.MediaType.HDVMD:
                discType    = "HD VMD";
                discSubType = "HD VMD";

                break;
            case CommonTypes.MediaType.HiMD:
                discType    = "MiniDisc";
                discSubType = "Hi-MD";

                break;
            case CommonTypes.MediaType.HVD:
                discType    = "HVD";
                discSubType = "HVD";

                break;
            case CommonTypes.MediaType.LD:
                discType    = "LaserDisc";
                discSubType = "LaserDisc";

                break;
            case CommonTypes.MediaType.CRVdisc:
                discType    = "CRVdisc";
                discSubType = "CRVdisc";

                break;
            case CommonTypes.MediaType.LDROM:
                discType    = "LaserDisc";
                discSubType = "LD-ROM";

                break;
            case CommonTypes.MediaType.LVROM:
                discType    = "LaserDisc";
                discSubType = "LV-ROM";

                break;
            case CommonTypes.MediaType.MegaLD:
                discType    = "LaserDisc";
                discSubType = "MegaLD";

                break;
            case CommonTypes.MediaType.MD:
                discType    = "MiniDisc";
                discSubType = "MiniDisc";

                break;
            case CommonTypes.MediaType.MD60:
                discType    = "MiniDisc";
                discSubType = "MiniDisc (60 minute)";

                break;
            case CommonTypes.MediaType.MD74:
                discType    = "MiniDisc";
                discSubType = "MiniDisc (74 minute)";

                break;
            case CommonTypes.MediaType.MD80:
                discType    = "MiniDisc";
                discSubType = "MiniDisc (80 minute)";

                break;
            case CommonTypes.MediaType.MEGACD:
                discType    = "Compact Disc";
                discSubType = "Sega Mega CD";

                break;
            case CommonTypes.MediaType.PCD:
                discType    = "Compact Disc";
                discSubType = "Photo CD";

                break;
            case CommonTypes.MediaType.PlayStationMemoryCard:
                discType    = "PlayStation Memory Card";
                discSubType = "PlayStation Memory Card";

                break;
            case CommonTypes.MediaType.PlayStationMemoryCard2:
                discType    = "PlayStation Memory Card";
                discSubType = "PlayStation 2 Memory Card";

                break;
            case CommonTypes.MediaType.PS1CD:
                discType    = "Compact Disc";
                discSubType = "PlayStation Game Disc";

                break;
            case CommonTypes.MediaType.PS2CD:
                discType    = "Compact Disc";
                discSubType = "PlayStation 2 Game Disc";

                break;
            case CommonTypes.MediaType.PS2DVD:
                discType    = "DVD";
                discSubType = "PlayStation 2 Game Disc";

                break;
            case CommonTypes.MediaType.PS3BD:
                discType    = "Blu-ray";
                discSubType = "PlayStation 3 Game Disc";

                break;
            case CommonTypes.MediaType.PS3DVD:
                discType    = "DVD";
                discSubType = "PlayStation 3 Game Disc";

                break;
            case CommonTypes.MediaType.PS4BD:
                discType    = "Blu-ray";
                discSubType = "PlayStation 4 Game Disc";

                break;
            case CommonTypes.MediaType.PS5BD:
                discType    = "Blu-ray";
                discSubType = "PlayStation 5 Game Disc";

                break;
            case CommonTypes.MediaType.SACD:
                discType    = "SACD";
                discSubType = "Super Audio CD";

                break;
            case CommonTypes.MediaType.SegaCard:
                discType    = "Sega Card";
                discSubType = "Sega Card";

                break;
            case CommonTypes.MediaType.SATURNCD:
                discType    = "Compact Disc";
                discSubType = "Sega Saturn CD";

                break;
            case CommonTypes.MediaType.SVCD:
                discType    = "Compact Disc";
                discSubType = "Super Video CD";

                break;
            case CommonTypes.MediaType.CVD:
                discType    = "Compact Disc";
                discSubType = "China Video Disc";

                break;
            case CommonTypes.MediaType.SVOD:
                discType    = "SVOD";
                discSubType = "SVOD";

                break;
            case CommonTypes.MediaType.UDO:
                discType    = "UDO";
                discSubType = "UDO";

                break;
            case CommonTypes.MediaType.UMD:
                discType    = "UMD";
                discSubType = "Universal Media Disc";

                break;
            case CommonTypes.MediaType.VCD:
                discType    = "Compact Disc";
                discSubType = "Video CD";

                break;
            case CommonTypes.MediaType.Nuon:
                discType    = "DVD";
                discSubType = "Nuon";

                break;
            case CommonTypes.MediaType.XGD:
                discType    = "DVD";
                discSubType = "Xbox Game Disc (XGD)";

                break;
            case CommonTypes.MediaType.XGD2:
                discType    = "DVD";
                discSubType = "Xbox 360 Game Disc (XGD2)";

                break;
            case CommonTypes.MediaType.XGD3:
                discType    = "DVD";
                discSubType = "Xbox 360 Game Disc (XGD3)";

                break;
            case CommonTypes.MediaType.XGD4:
                discType    = "Blu-ray";
                discSubType = "Xbox One Game Disc (XGD4)";

                break;
            case CommonTypes.MediaType.FMTOWNS:
                discType    = "Compact Disc";
                discSubType = "FM-Towns";

                break;
            case CommonTypes.MediaType.Apple32SS:
                discType    = "5.25\" floppy";
                discSubType = "Apple DOS 3.2";

                break;
            case CommonTypes.MediaType.Apple32DS:
                discType    = "5.25\" floppy";
                discSubType = "Apple DOS 3.2 (double-sided)";

                break;
            case CommonTypes.MediaType.Apple33SS:
                discType    = "5.25\" floppy";
                discSubType = "Apple DOS 3.3";

                break;
            case CommonTypes.MediaType.Apple33DS:
                discType    = "5.25\" floppy";
                discSubType = "Apple DOS 3.3 (double-sided)";

                break;
            case CommonTypes.MediaType.AppleSonySS:
                discType    = "3.5\" floppy";
                discSubType = "Apple 400K";

                break;
            case CommonTypes.MediaType.AppleSonyDS:
                discType    = "3.5\" floppy";
                discSubType = "Apple 800K";

                break;
            case CommonTypes.MediaType.AppleFileWare:
                discType    = "5.25\" floppy";
                discSubType = "Apple FileWare";

                break;
            case CommonTypes.MediaType.RX50:
                discType    = "5.25\" floppy";
                discSubType = "DEC RX50";

                break;
            case CommonTypes.MediaType.DOS_525_SS_DD_8:
                discType    = "5.25\" floppy";
                discSubType = "IBM double-density, single-sided, 8 sectors";

                break;
            case CommonTypes.MediaType.DOS_525_SS_DD_9:
                discType    = "5.25\" floppy";
                discSubType = "IBM double-density, single-sided, 9 sectors";

                break;
            case CommonTypes.MediaType.DOS_525_DS_DD_8:
                discType    = "5.25\" floppy";
                discSubType = "IBM double-density, double-sided, 8 sectors";

                break;
            case CommonTypes.MediaType.DOS_525_DS_DD_9:
                discType    = "5.25\" floppy";
                discSubType = "IBM double-density, double-sided, 9 sectors";

                break;
            case CommonTypes.MediaType.DOS_525_HD:
                discType    = "5.25\" floppy";
                discSubType = "IBM high-density";

                break;
            case CommonTypes.MediaType.DOS_35_SS_DD_8:
                discType    = "3.5\" floppy";
                discSubType = "IBM double-density, single-sided, 8 sectors";

                break;
            case CommonTypes.MediaType.DOS_35_SS_DD_9:
                discType    = "3.5\" floppy";
                discSubType = "IBM double-density, single-sided, 9 sectors";

                break;
            case CommonTypes.MediaType.DOS_35_DS_DD_8:
                discType    = "3.5\" floppy";
                discSubType = "IBM double-density, double-sided, 8 sectors";

                break;
            case CommonTypes.MediaType.DOS_35_DS_DD_9:
                discType    = "3.5\" floppy";
                discSubType = "IBM double-density, double-sided, 9 sectors";

                break;
            case CommonTypes.MediaType.DOS_35_HD:
                discType    = "3.5\" floppy";
                discSubType = "IBM high-density";

                break;
            case CommonTypes.MediaType.DOS_35_ED:
                discType    = "3.5\" floppy";
                discSubType = "IBM extra-density";

                break;
            case CommonTypes.MediaType.Apricot_35:
                discType    = "3.5\" floppy";
                discSubType = "Apricot double-density, single-sided, 70 tracks";

                break;
            case CommonTypes.MediaType.DMF:
                discType    = "3.5\" floppy";
                discSubType = "Microsoft DMF";

                break;
            case CommonTypes.MediaType.DMF_82:
                discType    = "3.5\" floppy";
                discSubType = "Microsoft DMF (82-track)";

                break;
            case CommonTypes.MediaType.XDF_35:
                discType    = "3.5\" floppy";
                discSubType = "IBM XDF";

                break;
            case CommonTypes.MediaType.XDF_525:
                discType    = "5.25\" floppy";
                discSubType = "IBM XDF";

                break;
            case CommonTypes.MediaType.IBM23FD:
                discType    = "8\" floppy";
                discSubType = "IBM 23FD";

                break;
            case CommonTypes.MediaType.IBM33FD_128:
                discType    = "8\" floppy";
                discSubType = "IBM 33FD (128 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM33FD_256:
                discType    = "8\" floppy";
                discSubType = "IBM 33FD (256 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM33FD_512:
                discType    = "8\" floppy";
                discSubType = "IBM 33FD (512 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM43FD_128:
                discType    = "8\" floppy";
                discSubType = "IBM 43FD (128 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM43FD_256:
                discType    = "8\" floppy";
                discSubType = "IBM 43FD (256 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM53FD_256:
                discType    = "8\" floppy";
                discSubType = "IBM 53FD (256 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM53FD_512:
                discType    = "8\" floppy";
                discSubType = "IBM 53FD (512 bytes/sector)";

                break;
            case CommonTypes.MediaType.IBM53FD_1024:
                discType    = "8\" floppy";
                discSubType = "IBM 53FD (1024 bytes/sector)";

                break;
            case CommonTypes.MediaType.RX01:
                discType    = "8\" floppy";
                discSubType = "DEC RX-01";

                break;
            case CommonTypes.MediaType.RX02:
                discType    = "8\" floppy";
                discSubType = "DEC RX-02";

                break;
            case CommonTypes.MediaType.RX03:
                discType    = "8\" floppy";
                discSubType = "DEC RX-03";

                break;
            case CommonTypes.MediaType.ACORN_525_SS_SD_40:
                discType    = "5.25\" floppy";
                discSubType = "BBC Micro 100K";

                break;
            case CommonTypes.MediaType.ACORN_525_SS_SD_80:
                discType    = "5.25\" floppy";
                discSubType = "BBC Micro 200K";

                break;
            case CommonTypes.MediaType.ACORN_525_SS_DD_40:
                discType    = "5.25\" floppy";
                discSubType = "Acorn S";

                break;
            case CommonTypes.MediaType.ACORN_525_SS_DD_80:
                discType    = "5.25\" floppy";
                discSubType = "Acorn M";

                break;
            case CommonTypes.MediaType.ACORN_525_DS_DD:
                discType    = "5.25\" floppy";
                discSubType = "Acorn L";

                break;
            case CommonTypes.MediaType.ACORN_35_DS_DD:
                discType    = "3.5\" floppy";
                discSubType = "Acorn Archimedes";

                break;
            case CommonTypes.MediaType.ACORN_35_DS_HD:
                discType    = "3.5\" floppy";
                discSubType = "Acorn Archimedes high-density";

                break;
            case CommonTypes.MediaType.ATARI_525_SD:
                discType    = "5.25\" floppy";
                discSubType = "Atari single-density";

                break;
            case CommonTypes.MediaType.ATARI_525_ED:
                discType    = "5.25\" floppy";
                discSubType = "Atari enhanced-density";

                break;
            case CommonTypes.MediaType.ATARI_525_DD:
                discType    = "5.25\" floppy";
                discSubType = "Atari double-density";

                break;
            case CommonTypes.MediaType.ATARI_35_SS_DD:
                discType    = "3.5\" floppy";
                discSubType = "Atari ST double-density, single-sided, 10 sectors";

                break;
            case CommonTypes.MediaType.ATARI_35_DS_DD:
                discType    = "3.5\" floppy";
                discSubType = "Atari ST double-density, double-sided, 10 sectors";

                break;
            case CommonTypes.MediaType.ATARI_35_SS_DD_11:
                discType    = "3.5\" floppy";
                discSubType = "Atari ST double-density, single-sided, 11 sectors";

                break;
            case CommonTypes.MediaType.ATARI_35_DS_DD_11:
                discType    = "3.5\" floppy";
                discSubType = "Atari ST double-density, double-sided, 11 sectors";

                break;
            case CommonTypes.MediaType.CBM_1540:
            case CommonTypes.MediaType.CBM_1540_Ext:
                discType    = "5.25\" floppy";
                discSubType = "Commodore 1540/1541";

                break;
            case CommonTypes.MediaType.CBM_1571:
                discType    = "5.25\" floppy";
                discSubType = "Commodore 1571";

                break;
            case CommonTypes.MediaType.CBM_35_DD:
                discType    = "3.5\" floppy";
                discSubType = "Commodore 1581";

                break;
            case CommonTypes.MediaType.CBM_AMIGA_35_DD:
                discType    = "3.5\" floppy";
                discSubType = "Amiga double-density";

                break;
            case CommonTypes.MediaType.CBM_AMIGA_35_HD:
                discType    = "3.5\" floppy";
                discSubType = "Amiga high-density";

                break;
            case CommonTypes.MediaType.NEC_8_SD:
                discType    = "8\" floppy";
                discSubType = "NEC single-sided";

                break;
            case CommonTypes.MediaType.NEC_8_DD:
                discType    = "8\" floppy";
                discSubType = "NEC double-sided";

                break;
            case CommonTypes.MediaType.NEC_525_SS:
                discType    = "5.25\" floppy";
                discSubType = "NEC single-sided";

                break;
            case CommonTypes.MediaType.NEC_525_HD:
                discType    = "5.25\" floppy";
                discSubType = "NEC high-density";

                break;
            case CommonTypes.MediaType.NEC_35_HD_8:
                discType    = "3.5\" floppy";
                discSubType = "NEC high-density";

                break;
            case CommonTypes.MediaType.NEC_35_HD_15:
                discType    = "3.5\" floppy";
                discSubType = "NEC high-density";

                break;
            case CommonTypes.MediaType.NEC_35_TD:
                discType    = "3.5\" floppy";
                discSubType = "NEC triple-density";

                break;
            case CommonTypes.MediaType.SHARP_525_9:
                discType    = "5.25\" floppy";
                discSubType = "Sharp (9 sectors per track)";

                break;
            case CommonTypes.MediaType.SHARP_35_9:
                discType    = "3.5\" floppy";
                discSubType = "Sharp (9 sectors per track)";

                break;
            case CommonTypes.MediaType.ECMA_54:
                discType    = "8\" floppy";
                discSubType = "ECMA-54";

                break;
            case CommonTypes.MediaType.ECMA_59:
                discType    = "8\" floppy";
                discSubType = "ECMA-59";

                break;
            case CommonTypes.MediaType.ECMA_69_8:
            case CommonTypes.MediaType.ECMA_69_15:
            case CommonTypes.MediaType.ECMA_69_26:
                discType    = "8\" floppy";
                discSubType = "ECMA-69";

                break;
            case CommonTypes.MediaType.ECMA_66:
                discType    = "5.25\" floppy";
                discSubType = "ECMA-66";

                break;
            case CommonTypes.MediaType.ECMA_70:
                discType    = "5.25\" floppy";
                discSubType = "ECMA-70";

                break;
            case CommonTypes.MediaType.ECMA_78:
            case CommonTypes.MediaType.ECMA_78_2:
                discType    = "5.25\" floppy";
                discSubType = "ECMA-78";

                break;
            case CommonTypes.MediaType.ECMA_99_8:
            case CommonTypes.MediaType.ECMA_99_15:
            case CommonTypes.MediaType.ECMA_99_26:
                discType    = "5.25\" floppy";
                discSubType = "ECMA-99";

                break;
            case CommonTypes.MediaType.FDFORMAT_525_DD:
                discType    = "5.25\" floppy";
                discSubType = "FDFORMAT double-density";

                break;
            case CommonTypes.MediaType.FDFORMAT_525_HD:
                discType    = "5.25\" floppy";
                discSubType = "FDFORMAT high-density";

                break;
            case CommonTypes.MediaType.FDFORMAT_35_DD:
                discType    = "3.5\" floppy";
                discSubType = "FDFORMAT double-density";

                break;
            case CommonTypes.MediaType.FDFORMAT_35_HD:
                discType    = "3.5\" floppy";
                discSubType = "FDFORMAT high-density";

                break;
            case CommonTypes.MediaType.ECMA_260:
            case CommonTypes.MediaType.ECMA_260_Double:
                discType    = "356mm magneto-optical";
                discSubType = "ECMA-260 / ISO 15898";

                break;
            case CommonTypes.MediaType.ECMA_183_512:
            case CommonTypes.MediaType.ECMA_183:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-183";

                break;
            case CommonTypes.MediaType.ISO_10089:
            case CommonTypes.MediaType.ISO_10089_512:
                discType    = "5.25\" magneto-optical";
                discSubType = "ISO 10089";

                break;
            case CommonTypes.MediaType.ECMA_184_512:
            case CommonTypes.MediaType.ECMA_184:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-184";

                break;
            case CommonTypes.MediaType.ECMA_154:
                discType    = "3.5\" magneto-optical";
                discSubType = "ECMA-154";

                break;
            case CommonTypes.MediaType.ECMA_201:
            case CommonTypes.MediaType.ECMA_201_ROM:
                discType    = "3.5\" magneto-optical";
                discSubType = "ECMA-201";

                break;
            case CommonTypes.MediaType.ISO_15041_512:
                discType    = "3.5\" magneto-optical";
                discSubType = "ISO 15041";

                break;
            case CommonTypes.MediaType.FlashDrive:
                discType    = "USB flash drive";
                discSubType = "USB flash drive";

                break;
            case CommonTypes.MediaType.SuperCDROM2:
                discType    = "Compact Disc";
                discSubType = "Super CD-ROM²";

                break;
            case CommonTypes.MediaType.LDROM2:
                discType    = "LaserDisc";
                discSubType = "LD-ROM²";

                break;
            case CommonTypes.MediaType.JaguarCD:
                discType    = "Compact Disc";
                discSubType = "Atari Jaguar CD";

                break;
            case CommonTypes.MediaType.MilCD:
                discType    = "Compact Disc";
                discSubType = "Sega MilCD";

                break;
            case CommonTypes.MediaType.ThreeDO:
                discType    = "Compact Disc";
                discSubType = "3DO";

                break;
            case CommonTypes.MediaType.PCFX:
                discType    = "Compact Disc";
                discSubType = "PC-FX";

                break;
            case CommonTypes.MediaType.NeoGeoCD:
                discType    = "Compact Disc";
                discSubType = "NEO-GEO CD";

                break;
            case CommonTypes.MediaType.CDTV:
                discType    = "Compact Disc";
                discSubType = "Commodore CDTV";

                break;
            case CommonTypes.MediaType.CD32:
                discType    = "Compact Disc";
                discSubType = "Amiga CD32";

                break;
            case CommonTypes.MediaType.Playdia:
                discType    = "Compact Disc";
                discSubType = "Bandai Playdia";

                break;
            case CommonTypes.MediaType.Pippin:
                discType    = "Compact Disc";
                discSubType = "Apple Pippin";

                break;
            case CommonTypes.MediaType.ZIP100:
                discType    = "Iomega ZIP";
                discSubType = "Iomega ZIP100";

                break;
            case CommonTypes.MediaType.ZIP250:
                discType    = "Iomega ZIP";
                discSubType = "Iomega ZIP250";

                break;
            case CommonTypes.MediaType.ZIP750:
                discType    = "Iomega ZIP";
                discSubType = "Iomega ZIP750";

                break;
            case CommonTypes.MediaType.AppleProfile:
                discType    = "Hard Disk Drive";
                discSubType = "Apple Profile";

                break;
            case CommonTypes.MediaType.AppleWidget:
                discType    = "Hard Disk Drive";
                discSubType = "Apple Widget";

                break;
            case CommonTypes.MediaType.AppleHD20:
                discType    = "Hard Disk Drive";
                discSubType = "Apple HD20";

                break;
            case CommonTypes.MediaType.PriamDataTower:
                discType    = "Hard Disk Drive";
                discSubType = "Priam DataTower";

                break;
            case CommonTypes.MediaType.DDS1:
                discType    = "Digital Data Storage";
                discSubType = "DDS";

                break;
            case CommonTypes.MediaType.DDS2:
                discType    = "Digital Data Storage";
                discSubType = "DDS-2";

                break;
            case CommonTypes.MediaType.DDS3:
                discType    = "Digital Data Storage";
                discSubType = "DDS-3";

                break;
            case CommonTypes.MediaType.DDS4:
                discType    = "Digital Data Storage";
                discSubType = "DDS-4";

                break;
            case CommonTypes.MediaType.PocketZip:
                discType    = "Iomega PocketZip";
                discSubType = "Iomega PocketZip";

                break;
            case CommonTypes.MediaType.CompactFloppy:
                discType    = "3\" floppy";
                discSubType = "Compact Floppy";

                break;
            case CommonTypes.MediaType.GENERIC_HDD:
                discType    = "Hard Disk Drive";
                discSubType = "Unknown";

                break;
            case CommonTypes.MediaType.MDData:
                discType    = "MiniDisc";
                discSubType = "MD-DATA";

                break;
            case CommonTypes.MediaType.MDData2:
                discType    = "MiniDisc";
                discSubType = "MD-DATA2";

                break;
            case CommonTypes.MediaType.UDO2:
                discType    = "UDO";
                discSubType = "UDO2";

                break;
            case CommonTypes.MediaType.UDO2_WORM:
                discType    = "UDO";
                discSubType = "UDO2 (WORM)";

                break;
            case CommonTypes.MediaType.ADR30:
                discType    = "Advanced Digital Recording";
                discSubType = "ADR 30";

                break;
            case CommonTypes.MediaType.ADR50:
                discType    = "Advanced Digital Recording";
                discSubType = "ADR 50";

                break;
            case CommonTypes.MediaType.ADR260:
                discType    = "Advanced Digital Recording";
                discSubType = "ADR 2.60";

                break;
            case CommonTypes.MediaType.ADR2120:
                discType    = "Advanced Digital Recording";
                discSubType = "ADR 2.120";

                break;
            case CommonTypes.MediaType.AIT1:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-1";

                break;
            case CommonTypes.MediaType.AIT1Turbo:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-1 Turbo";

                break;
            case CommonTypes.MediaType.AIT2:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-2";

                break;
            case CommonTypes.MediaType.AIT2Turbo:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-2 Turbo";

                break;
            case CommonTypes.MediaType.AIT3:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-3";

                break;
            case CommonTypes.MediaType.AIT3Ex:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-3Ex";

                break;
            case CommonTypes.MediaType.AIT3Turbo:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-3 Turbo";

                break;
            case CommonTypes.MediaType.AIT4:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-4";

                break;
            case CommonTypes.MediaType.AIT5:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-5";

                break;
            case CommonTypes.MediaType.AITETurbo:
                discType    = "Advanced Intelligent Tape";
                discSubType = "AIT-E Turbo";

                break;
            case CommonTypes.MediaType.SAIT1:
                discType    = "Super Advanced Intelligent Tape";
                discSubType = "SAIT-1";

                break;
            case CommonTypes.MediaType.SAIT2:
                discType    = "Super Advanced Intelligent Tape";
                discSubType = "SAIT-2";

                break;
            case CommonTypes.MediaType.Bernoulli:
            case CommonTypes.MediaType.Bernoulli10:
                discType    = "Iomega Bernoulli Box";
                discSubType = "Iomega Bernoulli Box 10Mb";

                break;
            case CommonTypes.MediaType.Bernoulli20:
                discType    = "Iomega Bernoulli Box";
                discSubType = "Iomega Bernoulli Box 20Mb";

                break;
            case CommonTypes.MediaType.BernoulliBox2_20:
            case CommonTypes.MediaType.Bernoulli2:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 20Mb";

                break;
            case CommonTypes.MediaType.Bernoulli35:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 35Mb";

                break;
            case CommonTypes.MediaType.Bernoulli44:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 44Mb";

                break;
            case CommonTypes.MediaType.Bernoulli65:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 65Mb";

                break;
            case CommonTypes.MediaType.Bernoulli90:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 90Mb";

                break;
            case CommonTypes.MediaType.Bernoulli105:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 105Mb";

                break;
            case CommonTypes.MediaType.Bernoulli150:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 150Mb";

                break;
            case CommonTypes.MediaType.Bernoulli230:
                discType    = "Iomega Bernoulli Box II";
                discSubType = "Iomega Bernoulli Box II 230Mb";

                break;
            case CommonTypes.MediaType.Ditto:
                discType    = "Iomega Ditto";
                discSubType = "Iomega Ditto";

                break;
            case CommonTypes.MediaType.DittoMax:
                discType    = "Iomega Ditto";
                discSubType = "Iomega Ditto Max";

                break;
            case CommonTypes.MediaType.Jaz:
                discType    = "Iomega Jaz";
                discSubType = "Iomega Jaz 1GB";

                break;
            case CommonTypes.MediaType.Jaz2:
                discType    = "Iomega Jaz";
                discSubType = "Iomega Jaz 2GB";

                break;
            case CommonTypes.MediaType.REV35:
                discType    = "Iomega REV";
                discSubType = "Iomega REV-35";

                break;
            case CommonTypes.MediaType.REV70:
                discType    = "Iomega REV";
                discSubType = "Iomega REV-70";

                break;
            case CommonTypes.MediaType.REV120:
                discType    = "Iomega REV";
                discSubType = "Iomega REV-120";

                break;
            case CommonTypes.MediaType.CompactFlash:
                discType    = "Compact Flash";
                discSubType = "Compact Flash";

                break;
            case CommonTypes.MediaType.CompactFlashType2:
                discType    = "Compact Flash";
                discSubType = "Compact Flash Type 2";

                break;
            case CommonTypes.MediaType.CFast:
                discType    = "Compact Flash";
                discSubType = "CFast";

                break;
            case CommonTypes.MediaType.DigitalAudioTape:
                discType    = "Digital Audio Tape";
                discSubType = "Digital Audio Tape";

                break;
            case CommonTypes.MediaType.DAT72:
                discType    = "Digital Data Storage";
                discSubType = "DAT-72";

                break;
            case CommonTypes.MediaType.DAT160:
                discType    = "Digital Data Storage";
                discSubType = "DAT-160";

                break;
            case CommonTypes.MediaType.DAT320:
                discType    = "Digital Data Storage";
                discSubType = "DAT-320";

                break;
            case CommonTypes.MediaType.DECtapeII:
                discType    = "DECtape";
                discSubType = "DECtape II";

                break;
            case CommonTypes.MediaType.CompactTapeI:
                discType    = "CompacTape";
                discSubType = "CompacTape";

                break;
            case CommonTypes.MediaType.CompactTapeII:
                discType    = "CompacTape";
                discSubType = "CompacTape II";

                break;
            case CommonTypes.MediaType.DLTtapeIII:
                discType    = "Digital Linear Tape";
                discSubType = "DLTtape III";

                break;
            case CommonTypes.MediaType.DLTtapeIIIxt:
                discType    = "Digital Linear Tape";
                discSubType = "DLTtape IIIXT";

                break;
            case CommonTypes.MediaType.DLTtapeIV:
                discType    = "Digital Linear Tape";
                discSubType = "DLTtape IV";

                break;
            case CommonTypes.MediaType.DLTtapeS4:
                discType    = "Digital Linear Tape";
                discSubType = "DLTtape S4";

                break;
            case CommonTypes.MediaType.SDLT1:
                discType    = "Super Digital Linear Tape";
                discSubType = "SDLTtape I";

                break;
            case CommonTypes.MediaType.SDLT2:
                discType    = "Super Digital Linear Tape";
                discSubType = "SDLTtape II";

                break;
            case CommonTypes.MediaType.VStapeI:
                discType    = "Digital Linear Tape";
                discSubType = "DLTtape VS1";

                break;
            case CommonTypes.MediaType.Data8:
                discType    = "Data8";
                discSubType = "Data8";

                break;
            case CommonTypes.MediaType.MiniDV:
                discType    = "DV tape";
                discSubType = "MiniDV";

                break;
            case CommonTypes.MediaType.Exatape15m:
                discType    = "Exatape";
                discSubType = "Exatape (15m)";

                break;
            case CommonTypes.MediaType.Exatape22m:
                discType    = "Exatape";
                discSubType = "Exatape (22m)";

                break;
            case CommonTypes.MediaType.Exatape22mAME:
                discType    = "Exatape";
                discSubType = "Exatape (22m AME)";

                break;
            case CommonTypes.MediaType.Exatape28m:
                discType    = "Exatape";
                discSubType = "Exatape (28m)";

                break;
            case CommonTypes.MediaType.Exatape40m:
                discType    = "Exatape";
                discSubType = "Exatape (40m)";

                break;
            case CommonTypes.MediaType.Exatape45m:
                discType    = "Exatape";
                discSubType = "Exatape (45m)";

                break;
            case CommonTypes.MediaType.Exatape54m:
                discType    = "Exatape";
                discSubType = "Exatape (54m)";

                break;
            case CommonTypes.MediaType.Exatape75m:
                discType    = "Exatape";
                discSubType = "Exatape (75m)";

                break;
            case CommonTypes.MediaType.Exatape76m:
                discType    = "Exatape";
                discSubType = "Exatape (76m)";

                break;
            case CommonTypes.MediaType.Exatape80m:
                discType    = "Exatape";
                discSubType = "Exatape (80m)";

                break;
            case CommonTypes.MediaType.Exatape106m:
                discType    = "Exatape";
                discSubType = "Exatape (106m)";

                break;
            case CommonTypes.MediaType.Exatape112m:
                discType    = "Exatape";
                discSubType = "Exatape (112m)";

                break;
            case CommonTypes.MediaType.Exatape125m:
                discType    = "Exatape";
                discSubType = "Exatape (125m)";

                break;
            case CommonTypes.MediaType.Exatape150m:
                discType    = "Exatape";
                discSubType = "Exatape (150m)";

                break;
            case CommonTypes.MediaType.Exatape160mXL:
                discType    = "Exatape";
                discSubType = "Exatape XL (160m)";

                break;
            case CommonTypes.MediaType.Exatape170m:
                discType    = "Exatape";
                discSubType = "Exatape (170m)";

                break;
            case CommonTypes.MediaType.Exatape225m:
                discType    = "Exatape";
                discSubType = "Exatape (225m)";

                break;
            case CommonTypes.MediaType.EZ135:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "EZ135";

                break;
            case CommonTypes.MediaType.EZ230:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "EZ230";

                break;
            case CommonTypes.MediaType.Quest:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "Quest";

                break;
            case CommonTypes.MediaType.SparQ:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "SparQ";

                break;
            case CommonTypes.MediaType.SQ100:
                discType    = "3.9\" SyQuest cartridge";
                discSubType = "SQ100";

                break;
            case CommonTypes.MediaType.SQ200:
                discType    = "3.9\" SyQuest cartridge";
                discSubType = "SQ200";

                break;
            case CommonTypes.MediaType.SQ300:
                discType    = "3.9\" SyQuest cartridge";
                discSubType = "SQ300";

                break;
            case CommonTypes.MediaType.SQ310:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "SQ310";

                break;
            case CommonTypes.MediaType.SQ327:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "SQ327";

                break;
            case CommonTypes.MediaType.SQ400:
                discType    = "5.25\" SyQuest cartridge";
                discSubType = "SQ400";

                break;
            case CommonTypes.MediaType.SQ800:
                discType    = "5.25\" SyQuest cartridge";
                discSubType = "SQ800";

                break;
            case CommonTypes.MediaType.SQ1500:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "SQ1500";

                break;
            case CommonTypes.MediaType.SQ2000:
                discType    = "5.25\" SyQuest cartridge";
                discSubType = "SQ2000";

                break;
            case CommonTypes.MediaType.SyJet:
                discType    = "3.5\" SyQuest cartridge";
                discSubType = "SyJet";

                break;
            case CommonTypes.MediaType.LTO:
                discType    = "Linear Tape-Open";
                discSubType = "LTO";

                break;
            case CommonTypes.MediaType.LTO2:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-2";

                break;
            case CommonTypes.MediaType.LTO3:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-3";

                break;
            case CommonTypes.MediaType.LTO3WORM:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-3 (WORM)";

                break;
            case CommonTypes.MediaType.LTO4:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-4";

                break;
            case CommonTypes.MediaType.LTO4WORM:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-4 (WORM)";

                break;
            case CommonTypes.MediaType.LTO5:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-5";

                break;
            case CommonTypes.MediaType.LTO5WORM:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-5 (WORM)";

                break;
            case CommonTypes.MediaType.LTO6:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-6";

                break;
            case CommonTypes.MediaType.LTO6WORM:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-6 (WORM)";

                break;
            case CommonTypes.MediaType.LTO7:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-7";

                break;
            case CommonTypes.MediaType.LTO7WORM:
                discType    = "Linear Tape-Open";
                discSubType = "LTO-7 (WORM)";

                break;
            case CommonTypes.MediaType.MemoryStick:
                discType    = "Memory Stick";
                discSubType = "Memory Stick";

                break;
            case CommonTypes.MediaType.MemoryStickDuo:
                discType    = "Memory Stick";
                discSubType = "Memory Stick Duo";

                break;
            case CommonTypes.MediaType.MemoryStickMicro:
                discType    = "Memory Stick";
                discSubType = "Memory Stick Micro";

                break;
            case CommonTypes.MediaType.MemoryStickPro:
                discType    = "Memory Stick";
                discSubType = "Memory Stick Pro";

                break;
            case CommonTypes.MediaType.MemoryStickProDuo:
                discType    = "Memory Stick";
                discSubType = "Memory Stick PRO Duo";

                break;
            case CommonTypes.MediaType.SecureDigital:
                discType    = "Secure Digital";
                discSubType = "Secure Digital";

                break;
            case CommonTypes.MediaType.miniSD:
                discType    = "Secure Digital";
                discSubType = "miniSD";

                break;
            case CommonTypes.MediaType.microSD:
                discType    = "Secure Digital";
                discSubType = "microSD";

                break;
            case CommonTypes.MediaType.MMC:
                discType    = "MultiMediaCard";
                discSubType = "MultiMediaCard";

                break;
            case CommonTypes.MediaType.MMCmicro:
                discType    = "MultiMediaCard";
                discSubType = "MMCmicro";

                break;
            case CommonTypes.MediaType.RSMMC:
                discType    = "MultiMediaCard";
                discSubType = "Reduced-Size MultiMediaCard";

                break;
            case CommonTypes.MediaType.MMCplus:
                discType    = "MultiMediaCard";
                discSubType = "MMCplus";

                break;
            case CommonTypes.MediaType.MMCmobile:
                discType    = "MultiMediaCard";
                discSubType = "MMCmobile";

                break;
            case CommonTypes.MediaType.MLR1:
                discType    = "Scalable Linear Recording";
                discSubType = "MLR1";

                break;
            case CommonTypes.MediaType.MLR1SL:
                discType    = "Scalable Linear Recording";
                discSubType = "MLR1 SL";

                break;
            case CommonTypes.MediaType.MLR3:
                discType    = "Scalable Linear Recording";
                discSubType = "MLR3";

                break;
            case CommonTypes.MediaType.SLR1:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR1";

                break;
            case CommonTypes.MediaType.SLR2:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR2";

                break;
            case CommonTypes.MediaType.SLR3:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR3";

                break;
            case CommonTypes.MediaType.SLR32:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR32";

                break;
            case CommonTypes.MediaType.SLR32SL:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR32 SL";

                break;
            case CommonTypes.MediaType.SLR4:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR4";

                break;
            case CommonTypes.MediaType.SLR5:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR5";

                break;
            case CommonTypes.MediaType.SLR5SL:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR5 SL";

                break;
            case CommonTypes.MediaType.SLR6:
                discType    = "Scalable Linear Recording";
                discSubType = "SLR6";

                break;
            case CommonTypes.MediaType.SLRtape7:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape7";

                break;
            case CommonTypes.MediaType.SLRtape7SL:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape7 SL";

                break;
            case CommonTypes.MediaType.SLRtape24:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape24";

                break;
            case CommonTypes.MediaType.SLRtape24SL:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape24 SL";

                break;
            case CommonTypes.MediaType.SLRtape40:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape40";

                break;
            case CommonTypes.MediaType.SLRtape50:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape50";

                break;
            case CommonTypes.MediaType.SLRtape60:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape60";

                break;
            case CommonTypes.MediaType.SLRtape75:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape75";

                break;
            case CommonTypes.MediaType.SLRtape100:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape100";

                break;
            case CommonTypes.MediaType.SLRtape140:
                discType    = "Scalable Linear Recording";
                discSubType = "SLRtape140";

                break;
            case CommonTypes.MediaType.QIC11:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-11";

                break;
            case CommonTypes.MediaType.QIC24:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-24";

                break;
            case CommonTypes.MediaType.QIC40:
                discType    = "Quarter-inch mini cartridge";
                discSubType = "QIC-40";

                break;
            case CommonTypes.MediaType.QIC80:
                discType    = "Quarter-inch mini cartridge";
                discSubType = "QIC-80";

                break;
            case CommonTypes.MediaType.QIC120:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-120";

                break;
            case CommonTypes.MediaType.QIC150:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-150";

                break;
            case CommonTypes.MediaType.QIC320:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-320";

                break;
            case CommonTypes.MediaType.QIC525:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-525";

                break;
            case CommonTypes.MediaType.QIC1350:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-1350";

                break;
            case CommonTypes.MediaType.QIC3010:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-3010";

                break;
            case CommonTypes.MediaType.QIC3020:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-3020";

                break;
            case CommonTypes.MediaType.QIC3080:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-3080";

                break;
            case CommonTypes.MediaType.QIC3095:
                discType    = "Quarter-inch cartridge";
                discSubType = "QIC-3095";

                break;
            case CommonTypes.MediaType.Travan:
                discType    = "Travan";
                discSubType = "TR-1";

                break;
            case CommonTypes.MediaType.Travan1Ex:
                discType    = "Travan";
                discSubType = "TR-1 Ex";

                break;
            case CommonTypes.MediaType.Travan3:
                discType    = "Travan";
                discSubType = "TR-3";

                break;
            case CommonTypes.MediaType.Travan3Ex:
                discType    = "Travan";
                discSubType = "TR-3 Ex";

                break;
            case CommonTypes.MediaType.Travan4:
                discType    = "Travan";
                discSubType = "TR-4";

                break;
            case CommonTypes.MediaType.Travan5:
                discType    = "Travan";
                discSubType = "TR-5";

                break;
            case CommonTypes.MediaType.Travan7:
                discType    = "Travan";
                discSubType = "TR-7";

                break;
            case CommonTypes.MediaType.VXA1:
                discType    = "VXA";
                discSubType = "VXA-1";

                break;
            case CommonTypes.MediaType.VXA2:
                discType    = "VXA";
                discSubType = "VXA-2";

                break;
            case CommonTypes.MediaType.VXA3:
                discType    = "VXA";
                discSubType = "VXA-3";

                break;
            case CommonTypes.MediaType.ECMA_153:
            case CommonTypes.MediaType.ECMA_153_512:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-153";

                break;
            case CommonTypes.MediaType.ECMA_189:
                discType    = "300mm magneto optical";
                discSubType = "ECMA-189";

                break;
            case CommonTypes.MediaType.ECMA_190:
                discType    = "300mm magneto optical";
                discSubType = "ECMA-190";

                break;
            case CommonTypes.MediaType.ECMA_195:
            case CommonTypes.MediaType.ECMA_195_512:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-195";

                break;
            case CommonTypes.MediaType.ECMA_223:
            case CommonTypes.MediaType.ECMA_223_512:
                discType    = "3.5\" magneto-optical";
                discSubType = "ECMA-223";

                break;
            case CommonTypes.MediaType.ECMA_238:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-238";

                break;
            case CommonTypes.MediaType.ECMA_239:
                discType    = "3.5\" magneto-optical";
                discSubType = "ECMA-239";

                break;
            case CommonTypes.MediaType.ECMA_280:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-280";

                break;
            case CommonTypes.MediaType.ECMA_317:
                discType    = "300mm magneto optical";
                discSubType = "ECMA-317";

                break;
            case CommonTypes.MediaType.ECMA_322:
            case CommonTypes.MediaType.ECMA_322_512:
            case CommonTypes.MediaType.ECMA_322_1k:
            case CommonTypes.MediaType.ECMA_322_2k:
                discType    = "5.25\" magneto-optical";
                discSubType = "ECMA-322 / ISO 22092";

                break;
            case CommonTypes.MediaType.ISO_15286:
            case CommonTypes.MediaType.ISO_15286_1024:
            case CommonTypes.MediaType.ISO_15286_512:
                discType    = "5.25\" magneto-optical";
                discSubType = "ISO-15286";

                break;
            case CommonTypes.MediaType.ISO_14517:
            case CommonTypes.MediaType.ISO_14517_512:
                discType    = "5.25\" magneto-optical";
                discSubType = "ISO-14517";

                break;
            case CommonTypes.MediaType.GigaMo:
                discType    = "3.5\" magneto-optical";
                discSubType = "GIGAMO";

                break;
            case CommonTypes.MediaType.GigaMo2:
                discType    = "3.5\" magneto-optical";
                discSubType = "2.3GB GIGAMO";

                break;
            case CommonTypes.MediaType.UnknownMO:
                discType    = "Magneto-optical";
                discSubType = "Unknown";

                break;
            case CommonTypes.MediaType.Floptical:
                discType    = "Floptical";
                discSubType = "Floptical";

                break;
            case CommonTypes.MediaType.HiFD:
                discType    = "HiFD";
                discSubType = "HiFD";

                break;
            case CommonTypes.MediaType.LS120:
                discType    = "SuperDisk";
                discSubType = "LS-120";

                break;
            case CommonTypes.MediaType.LS240:
                discType    = "SuperDisk";
                discSubType = "LS-240";

                break;
            case CommonTypes.MediaType.FD32MB:
                discType    = "3.5\" floppy";
                discSubType = "FD32MB";

                break;
            case CommonTypes.MediaType.UHD144:
                discType    = "UHD144";
                discSubType = "UHD144";

                break;
            case CommonTypes.MediaType.VCDHD:
                discType    = "VCDHD";
                discSubType = "VCDHD";

                break;
            case CommonTypes.MediaType.HuCard:
                discType    = "HuCard";
                discSubType = "HuCard";

                break;
            case CommonTypes.MediaType.CompactCassette:
                discType    = "Compact Cassette";
                discSubType = "Compact Cassette";

                break;
            case CommonTypes.MediaType.Dcas25:
                discType    = "Compact Cassette";
                discSubType = "D/CAS-25";

                break;
            case CommonTypes.MediaType.Dcas85:
                discType    = "Compact Cassette";
                discSubType = "D/CAS-85";

                break;
            case CommonTypes.MediaType.Dcas103:
                discType    = "Compact Cassette";
                discSubType = "D/CAS-103";

                break;
            case CommonTypes.MediaType.PCCardTypeI:
                discType    = "PCMCIA Card";
                discSubType = "PC-Card Type I";

                break;
            case CommonTypes.MediaType.PCCardTypeII:
                discType    = "PCMCIA Card";
                discSubType = "PC-Card Type II";

                break;
            case CommonTypes.MediaType.PCCardTypeIII:
                discType    = "PCMCIA Card";
                discSubType = "PC-Card Type III";

                break;
            case CommonTypes.MediaType.PCCardTypeIV:
                discType    = "PCMCIA Card";
                discSubType = "PC-Card Type IV";

                break;
            case CommonTypes.MediaType.ExpressCard34:
                discType    = "Express Card";
                discSubType = "Express Card (34mm)";

                break;
            case CommonTypes.MediaType.ExpressCard54:
                discType    = "Express Card";
                discSubType = "Express Card (54mm)";

                break;
            case CommonTypes.MediaType.FamicomGamePak:
                discType    = "Nintendo Famicom Game Pak";
                discSubType = "Nintendo Famicom Game Pak";

                break;
            case CommonTypes.MediaType.GameBoyAdvanceGamePak:
                discType    = "Nintendo Game Boy Advance Game Pak";
                discSubType = "Nintendo Game Boy Advance Game Pak";

                break;
            case CommonTypes.MediaType.GameBoyGamePak:
                discType    = "Nintendo Game Boy Game Pak";
                discSubType = "Nintendo Game Boy Game Pak";

                break;
            case CommonTypes.MediaType.N64DD:
                discType    = "Nintendo 64 Disk";
                discSubType = "Nintendo 64 Disk";

                break;
            case CommonTypes.MediaType.N64GamePak:
                discType    = "Nintendo 64 Game Pak";
                discSubType = "Nintendo 64 Game Pak";

                break;
            case CommonTypes.MediaType.NESGamePak:
                discType    = "Nintendo Entertainment System Game Pak";
                discSubType = "Nintendo Entertainment System Game Pak";

                break;
            case CommonTypes.MediaType.Nintendo3DSGameCard:
                discType    = "Nintendo 3DS Game Card";
                discSubType = "Nintendo 3DS Game Card";

                break;
            case CommonTypes.MediaType.NintendoDiskCard:
                discType    = "Nintendo Disk Card";
                discSubType = "Nintendo Disk Card";

                break;
            case CommonTypes.MediaType.NintendoDSGameCard:
                discType    = "Nintendo DS Game Card";
                discSubType = "Nintendo DS Game Card";

                break;
            case CommonTypes.MediaType.NintendoDSiGameCard:
                discType    = "Nintendo DSi Game Card";
                discSubType = "Nintendo DSi Game Card";

                break;
            case CommonTypes.MediaType.SNESGamePak:
                discType    = "Super Nintendo Game Pak";
                discSubType = "Super Nintendo Game Pak";

                break;
            case CommonTypes.MediaType.SNESGamePakUS:
                discType    = "Super Nintendo Game Pak (US)";
                discSubType = "Super Nintendo Game Pak (US)";

                break;
            case CommonTypes.MediaType.SwitchGameCard:
                discType    = "Nintendo Switch Game Card";
                discSubType = "Nintendo Switch Game Card";

                break;
            case CommonTypes.MediaType.IBM3470:
                discType    = "IBM 3470";
                discSubType = "IBM 3470";

                break;
            case CommonTypes.MediaType.IBM3480:
                discType    = "IBM 3480";
                discSubType = "IBM 3480";

                break;
            case CommonTypes.MediaType.IBM3490:
                discType    = "IBM 3490";
                discSubType = "IBM 3490";

                break;
            case CommonTypes.MediaType.IBM3490E:
                discType    = "IBM 3490E";
                discSubType = "IBM 3490E";

                break;
            case CommonTypes.MediaType.IBM3592:
                discType    = "IBM 3592";
                discSubType = "IBM 3592";

                break;
            case CommonTypes.MediaType.STK4480:
                discType    = "STK 4480";
                discSubType = "STK 4480";

                break;
            case CommonTypes.MediaType.STK4490:
                discType    = "STK 4490";
                discSubType = "STK 4490";

                break;
            case CommonTypes.MediaType.STK9490:
                discType    = "STK 9490";
                discSubType = "STK 9490";

                break;
            case CommonTypes.MediaType.T9840A:
                discType    = "STK T-9840";
                discSubType = "STK T-9840A";

                break;
            case CommonTypes.MediaType.T9840B:
                discType    = "STK T-9840";
                discSubType = "STK T-9840B";

                break;
            case CommonTypes.MediaType.T9840C:
                discType    = "STK T-9840";
                discSubType = "STK T-9840C";

                break;
            case CommonTypes.MediaType.T9840D:
                discType    = "STK T-9840";
                discSubType = "STK T-9840D";

                break;
            case CommonTypes.MediaType.T9940A:
                discType    = "STK T-9940";
                discSubType = "STK T-9940A";

                break;
            case CommonTypes.MediaType.T9940B:
                discType    = "STK T-9840";
                discSubType = "STK T-9840B";

                break;
            case CommonTypes.MediaType.T10000A:
                discType    = "STK T-10000";
                discSubType = "STK T-10000A";

                break;
            case CommonTypes.MediaType.T10000B:
                discType    = "STK T-10000";
                discSubType = "STK T-10000B";

                break;
            case CommonTypes.MediaType.T10000C:
                discType    = "STK T-10000";
                discSubType = "STK T-10000C";

                break;
            case CommonTypes.MediaType.T10000D:
                discType    = "STK T-10000";
                discSubType = "STK T-10000D";

                break;
            case CommonTypes.MediaType.DemiDiskette:
                discType    = "DemiDiskette";
                discSubType = "DemiDiskette";

                break;
            case CommonTypes.MediaType.QuickDisk:
                discType    = "QuickDisk";
                discSubType = "QuickDisk";

                break;
            case CommonTypes.MediaType.VideoFloppy:
                discType    = "VideoFloppy";
                discSubType = "VideoFloppy";

                break;
            case CommonTypes.MediaType.Wafer:
                discType    = "Wafer";
                discSubType = "Wafer";

                break;
            case CommonTypes.MediaType.ZXMicrodrive:
                discType    = "ZX Microdrive";
                discSubType = "ZX Microdrive";

                break;
            case CommonTypes.MediaType.BeeCard:
                discType    = "BeeCard";
                discSubType = "BeeCard";

                break;
            case CommonTypes.MediaType.Borsu:
                discType    = "Borsu";
                discSubType = "Borsu";

                break;
            case CommonTypes.MediaType.DataStore:
                discType    = "DataStore";
                discSubType = "DataStore";

                break;
            case CommonTypes.MediaType.DIR:
                discType    = "DIR";
                discSubType = "DIR";

                break;
            case CommonTypes.MediaType.DST:
                discType    = "DST";
                discSubType = "DST";

                break;
            case CommonTypes.MediaType.DTF:
                discType    = "DTF";
                discSubType = "DTF";

                break;
            case CommonTypes.MediaType.DTF2:
                discType    = "DTF2";
                discSubType = "DTF2";

                break;
            case CommonTypes.MediaType.Flextra3020:
                discType    = "Flextra";
                discSubType = "Flextra 3020";

                break;
            case CommonTypes.MediaType.Flextra3225:
                discType    = "Flextra";
                discSubType = "Flextra 3225";

                break;
            case CommonTypes.MediaType.HiTC1:
                discType    = "HiTC";
                discSubType = "HiTC1";

                break;
            case CommonTypes.MediaType.HiTC2:
                discType    = "HiTC";
                discSubType = "HiTC2";

                break;
            case CommonTypes.MediaType.LT1:
                discType    = "LT1";
                discSubType = "LT1";

                break;
            case CommonTypes.MediaType.MiniCard:
                discType    = "MiniCard";
                discSubType = "MiniCard";

                break;
            case CommonTypes.MediaType.Orb:
                discType    = "Orb";
                discSubType = "Orb";

                break;
            case CommonTypes.MediaType.Orb5:
                discType    = "Orb";
                discSubType = "Orb5";

                break;
            case CommonTypes.MediaType.SmartMedia:
                discType    = "SmartMedia";
                discSubType = "SmartMedia";

                break;
            case CommonTypes.MediaType.xD:
                discType    = "xD";
                discSubType = "xD";

                break;
            case CommonTypes.MediaType.XQD:
                discType    = "XQD";
                discSubType = "XQD";

                break;
            case CommonTypes.MediaType.DataPlay:
                discType    = "DataPlay";
                discSubType = "DataPlay";

                break;
            case CommonTypes.MediaType.PD650:
                discType    = "PD650";
                discSubType = "PD650";

                break;
            case CommonTypes.MediaType.PD650_WORM:
                discType    = "PD650";
                discSubType = "PD650 (WORM)";

                break;
            case CommonTypes.MediaType.RA60:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RA-60";

                break;
            case CommonTypes.MediaType.RA80:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RA-80";

                break;
            case CommonTypes.MediaType.RA81:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RA-81";

                break;
            case CommonTypes.MediaType.RC25:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RC-25";

                break;
            case CommonTypes.MediaType.RD31:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RD-31";

                break;
            case CommonTypes.MediaType.RD32:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RD-32";

                break;
            case CommonTypes.MediaType.RD51:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RD-51";

                break;
            case CommonTypes.MediaType.RD52:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RD-52";

                break;
            case CommonTypes.MediaType.RD53:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RD-53";

                break;
            case CommonTypes.MediaType.RD54:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RD-54";

                break;
            case CommonTypes.MediaType.RK06:
            case CommonTypes.MediaType.RK06_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RK-06";

                break;
            case CommonTypes.MediaType.RK07:
            case CommonTypes.MediaType.RK07_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RK-07";

                break;
            case CommonTypes.MediaType.RM02:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RM-02";

                break;
            case CommonTypes.MediaType.RM03:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RM-03";

                break;
            case CommonTypes.MediaType.RM05:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RM-05";

                break;
            case CommonTypes.MediaType.RP02:
            case CommonTypes.MediaType.RP02_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RP-02";

                break;
            case CommonTypes.MediaType.RP03:
            case CommonTypes.MediaType.RP03_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RP-03";

                break;
            case CommonTypes.MediaType.RP04:
            case CommonTypes.MediaType.RP04_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RP-04";

                break;
            case CommonTypes.MediaType.RP05:
            case CommonTypes.MediaType.RP05_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RP-05";

                break;
            case CommonTypes.MediaType.RP06:
            case CommonTypes.MediaType.RP06_18:
                discType    = "Hard Disk Drive";
                discSubType = "DEC RP-06";

                break;
            case CommonTypes.MediaType.RDX:
                discType    = "RDX";
                discSubType = "RDX";

                break;
            case CommonTypes.MediaType.RDX320:
                discType    = "RDX";
                discSubType = "RDX 320";

                break;
            case CommonTypes.MediaType.Zone_HDD:
                discType    = "Zoned Hard Disk Drive";
                discSubType = "Unknown";

                break;
            case CommonTypes.MediaType.Microdrive:
                discType    = "Hard Disk Drive";
                discSubType = "Microdrive";

                break;
            case CommonTypes.MediaType.VideoNow:
                discType    = "VideoNow";
                discSubType = "VideoNow";

                break;
            case CommonTypes.MediaType.VideoNowColor:
                discType    = "VideoNow";
                discSubType = "VideoNow Color";

                break;
            case CommonTypes.MediaType.VideoNowXp:
                discType    = "VideoNow";
                discSubType = "VideoNow XP";

                break;
            case CommonTypes.MediaType.KodakVerbatim3:
                discType    = "Kodak Verbatim";
                discSubType = "Kodak Verbatim (3 Mb)";

                break;
            case CommonTypes.MediaType.KodakVerbatim6:
                discType    = "Kodak Verbatim";
                discSubType = "Kodak Verbatim (6 Mb)";

                break;
            case CommonTypes.MediaType.KodakVerbatim12:
                discType    = "Kodak Verbatim";
                discSubType = "Kodak Verbatim (12 Mb)";

                break;
            case CommonTypes.MediaType.ProfessionalDisc:
                discType    = "Sony Professional Disc";
                discSubType = "Sony Professional Disc (single layer)";

                break;
            case CommonTypes.MediaType.ProfessionalDiscDual:
                discType    = "Sony Professional Disc";
                discSubType = "Sony Professional Disc (double layer)";

                break;
            case CommonTypes.MediaType.ProfessionalDiscTriple:
                discType    = "Sony Professional Disc";
                discSubType = "Sony Professional Disc (triple layer)";

                break;
            case CommonTypes.MediaType.ProfessionalDiscQuad:
                discType    = "Sony Professional Disc";
                discSubType = "Sony Professional Disc (quad layer)";

                break;
            case CommonTypes.MediaType.PDD:
                discType    = "Sony Professional Disc for DATA";
                discSubType = "Sony Professional Disc for DATA";

                break;
            case CommonTypes.MediaType.PDD_WORM:
                discType    = "Sony Professional Disc for DATA";
                discSubType = "Sony Professional Disc for DATA (write-once)";

                break;
            case CommonTypes.MediaType.ArchivalDisc:
                discType    = "Archival Disc";
                discSubType = "Archival Disc";

                break;
            case CommonTypes.MediaType.ArchivalDisc2:
                discType    = "Archival Disc";
                discSubType = "Archival Disc (2nd generation)";

                break;
            case CommonTypes.MediaType.ArchivalDisc3:
                discType    = "Archival Disc";
                discSubType = "Archival Disc (3rd generation)";

                break;
            case CommonTypes.MediaType.ODC300R:
                discType    = "Optical Disc Archive";
                discSubType = "ODC300R";

                break;
            case CommonTypes.MediaType.ODC300RE:
                discType    = "Optical Disc Archive";
                discSubType = "ODC300RE";

                break;
            case CommonTypes.MediaType.ODC600R:
                discType    = "Optical Disc Archive";
                discSubType = "ODC600R";

                break;
            case CommonTypes.MediaType.ODC600RE:
                discType    = "Optical Disc Archive";
                discSubType = "ODC600RE";

                break;
            case CommonTypes.MediaType.ODC1200RE:
                discType    = "Optical Disc Archive";
                discSubType = "ODC1200RE";

                break;
            case CommonTypes.MediaType.ODC1500R:
                discType    = "Optical Disc Archive";
                discSubType = "ODC1500R";

                break;
            case CommonTypes.MediaType.ODC3300R:
                discType    = "Optical Disc Archive";
                discSubType = "ODC3300R";

                break;
            case CommonTypes.MediaType.ODC5500R:
                discType    = "Optical Disc Archive";
                discSubType = "ODC5500R";

                break;
            case CommonTypes.MediaType.MetaFloppy_Mod_I:
                discType    = "5.25\" floppy";
                discSubType = "Micropolis MetaFloppy Mod I";

                break;
            case CommonTypes.MediaType.MetaFloppy_Mod_II:
                discType    = "5.25\" floppy";
                discSubType = "Micropolis MetaFloppy Mod II";

                break;
            case CommonTypes.MediaType.HF12:
                discType    = "HyperFlex";
                discSubType = "HyperFlex (12Mb)";

                break;
            case CommonTypes.MediaType.HF24:
                discType    = "HyperFlex";
                discSubType = "HyperFlex (24Mb)";

                break;
            default:
                discType    = "Unknown";
                discSubType = "Unknown";

                break;
        }

        return (discType, discSubType);
    }
}