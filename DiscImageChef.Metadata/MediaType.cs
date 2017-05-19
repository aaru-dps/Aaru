// /***************************************************************************
// The Disc Image Chef
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

namespace DiscImageChef.Metadata
{
    public static class MediaType
    {
        public static void MediaTypeToString(CommonTypes.MediaType dskType, out string DiscType, out string DiscSubType)
        {
            switch(dskType)
            {
                case CommonTypes.MediaType.BDR:
                    DiscType = "BD";
                    DiscSubType = "BD-R";
                    break;
                case CommonTypes.MediaType.BDRE:
                    DiscType = "BD";
                    DiscSubType = "BD-RE";
                    break;
                case CommonTypes.MediaType.BDREXL:
                    DiscType = "BD";
                    DiscSubType = "BD-RE XL";
                    break;
                case CommonTypes.MediaType.BDROM:
                    DiscType = "BD";
                    DiscSubType = "BD-ROM";
                    break;
                case CommonTypes.MediaType.BDRXL:
                    DiscType = "BD";
                    DiscSubType = "BD-R XL";
                    break;
                case CommonTypes.MediaType.CBHD:
                    DiscType = "BD";
                    DiscSubType = "CBHD";
                    break;
                case CommonTypes.MediaType.CD:
                    DiscType = "CD";
                    DiscSubType = "CD";
                    break;
                case CommonTypes.MediaType.CDDA:
                    DiscType = "CD";
                    DiscSubType = "CD Digital Audio";
                    break;
                case CommonTypes.MediaType.CDEG:
                    DiscType = "CD";
                    DiscSubType = "CD+EG";
                    break;
                case CommonTypes.MediaType.CDG:
                    DiscType = "CD";
                    DiscSubType = "CD+G";
                    break;
                case CommonTypes.MediaType.CDI:
                    DiscType = "CD";
                    DiscSubType = "CD-i";
                    break;
                case CommonTypes.MediaType.CDMIDI:
                    DiscType = "CD";
                    DiscSubType = "CD+MIDI";
                    break;
                case CommonTypes.MediaType.CDMO:
                    DiscType = "CD";
                    DiscSubType = "CD-MO";
                    break;
                case CommonTypes.MediaType.CDMRW:
                    DiscType = "CD";
                    DiscSubType = "CD-MRW";
                    break;
                case CommonTypes.MediaType.CDPLUS:
                    DiscType = "CD";
                    DiscSubType = "CD+";
                    break;
                case CommonTypes.MediaType.CDR:
                    DiscType = "CD";
                    DiscSubType = "CD-R";
                    break;
                case CommonTypes.MediaType.CDROM:
                    DiscType = "CD";
                    DiscSubType = "CD-ROM";
                    break;
                case CommonTypes.MediaType.CDROMXA:
                    DiscType = "CD";
                    DiscSubType = "CD-ROM XA";
                    break;
                case CommonTypes.MediaType.CDRW:
                    DiscType = "CD";
                    DiscSubType = "CD-RW";
                    break;
                case CommonTypes.MediaType.CDV:
                    DiscType = "CD";
                    DiscSubType = "CD-Video";
                    break;
                case CommonTypes.MediaType.DDCD:
                    DiscType = "DDCD";
                    DiscSubType = "DDCD";
                    break;
                case CommonTypes.MediaType.DDCDR:
                    DiscType = "DDCD";
                    DiscSubType = "DDCD-R";
                    break;
                case CommonTypes.MediaType.DDCDRW:
                    DiscType = "DDCD";
                    DiscSubType = "DDCD-RW";
                    break;
                case CommonTypes.MediaType.DTSCD:
                    DiscType = "CD";
                    DiscSubType = "DTS CD";
                    break;
                case CommonTypes.MediaType.DVDDownload:
                    DiscType = "DVD";
                    DiscSubType = "DVD-Download";
                    break;
                case CommonTypes.MediaType.DVDPR:
                    DiscType = "DVD";
                    DiscSubType = "DVD+R";
                    break;
                case CommonTypes.MediaType.DVDPRDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD+R DL";
                    break;
                case CommonTypes.MediaType.DVDPRW:
                    DiscType = "DVD";
                    DiscSubType = "DVD+RW";
                    break;
                case CommonTypes.MediaType.DVDPRWDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD+RW DL";
                    break;
                case CommonTypes.MediaType.DVDR:
                    DiscType = "DVD";
                    DiscSubType = "DVD-R";
                    break;
                case CommonTypes.MediaType.DVDRAM:
                    DiscType = "DVD";
                    DiscSubType = "DVD-RAM";
                    break;
                case CommonTypes.MediaType.DVDRDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD-R DL";
                    break;
                case CommonTypes.MediaType.DVDROM:
                    DiscType = "DVD";
                    DiscSubType = "DVD-ROM";
                    break;
                case CommonTypes.MediaType.DVDRW:
                    DiscType = "DVD";
                    DiscSubType = "DVD-RW";
                    break;
                case CommonTypes.MediaType.DVDRWDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD-RW";
                    break;
                case CommonTypes.MediaType.EVD:
                    DiscType = "EVD";
                    DiscSubType = "EVD";
                    break;
                case CommonTypes.MediaType.FDDVD:
                    DiscType = "FDDVD";
                    DiscSubType = "FDDVD";
                    break;
                case CommonTypes.MediaType.FVD:
                    DiscType = "FVD";
                    DiscSubType = "FVD";
                    break;
                case CommonTypes.MediaType.GDR:
                    DiscType = "GD";
                    DiscSubType = "GD-R";
                    break;
                case CommonTypes.MediaType.GDROM:
                    DiscType = "GD";
                    DiscSubType = "GD-ROM";
                    break;
                case CommonTypes.MediaType.GOD:
                    DiscType = "DVD";
                    DiscSubType = "GameCube Game Disc";
                    break;
                case CommonTypes.MediaType.WOD:
                    DiscType = "DVD";
                    DiscSubType = "Wii Optical Disc";
                    break;
                case CommonTypes.MediaType.WUOD:
                    DiscType = "BD";
                    DiscSubType = "Wii U Optical Disc";
                    break;
                case CommonTypes.MediaType.HDDVDR:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-R";
                    break;
                case CommonTypes.MediaType.HDDVDRAM:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-RAM";
                    break;
                case CommonTypes.MediaType.HDDVDRDL:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-R DL";
                    break;
                case CommonTypes.MediaType.HDDVDROM:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-ROM";
                    break;
                case CommonTypes.MediaType.HDDVDRW:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-RW";
                    break;
                case CommonTypes.MediaType.HDDVDRWDL:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-RW DL";
                    break;
                case CommonTypes.MediaType.HDVMD:
                    DiscType = "HD VMD";
                    DiscSubType = "HD VMD";
                    break;
                case CommonTypes.MediaType.HiMD:
                    DiscType = "MiniDisc";
                    DiscSubType = "HiMD";
                    break;
                case CommonTypes.MediaType.HVD:
                    DiscType = "HVD";
                    DiscSubType = "HVD";
                    break;
                case CommonTypes.MediaType.LD:
                    DiscType = "LaserDisc";
                    DiscSubType = "LaserDisc";
                    break;
                case CommonTypes.MediaType.LDROM:
                    DiscType = "LaserDisc";
                    DiscSubType = "LD-ROM";
                    break;
                case CommonTypes.MediaType.MD:
                    DiscType = "MiniDisc";
                    DiscSubType = "MiniDisc";
                    break;
                case CommonTypes.MediaType.MEGACD:
                    DiscType = "CD";
                    DiscSubType = "Sega Mega CD";
                    break;
                case CommonTypes.MediaType.PCD:
                    DiscType = "CD";
                    DiscSubType = "Photo CD";
                    break;
                case CommonTypes.MediaType.PS1CD:
                    DiscType = "CD";
                    DiscSubType = "PlayStation Game Disc";
                    break;
                case CommonTypes.MediaType.PS2CD:
                    DiscType = "CD";
                    DiscSubType = "PlayStation 2 Game Disc";
                    break;
                case CommonTypes.MediaType.PS2DVD:
                    DiscType = "DVD";
                    DiscSubType = "PlayStation 2 Game Disc";
                    break;
                case CommonTypes.MediaType.PS3BD:
                    DiscType = "BD";
                    DiscSubType = "PlayStation 3 Game Disc";
                    break;
                case CommonTypes.MediaType.PS3DVD:
                    DiscType = "DVD";
                    DiscSubType = "PlayStation 3 Game Disc";
                    break;
                case CommonTypes.MediaType.PS4BD:
                    DiscType = "BD";
                    DiscSubType = "PlayStation 4 Game Disc";
                    break;
                case CommonTypes.MediaType.SACD:
                    DiscType = "SACD";
                    DiscSubType = "Super Audio CD";
                    break;
                case CommonTypes.MediaType.SATURNCD:
                    DiscType = "CD";
                    DiscSubType = "Sega Saturn CD";
                    break;
                case CommonTypes.MediaType.SVCD:
                    DiscType = "CD";
                    DiscSubType = "Super Video CD";
                    break;
                case CommonTypes.MediaType.SVOD:
                    DiscType = "SVOD";
                    DiscSubType = "SVOD";
                    break;
                case CommonTypes.MediaType.UDO:
                    DiscType = "UDO";
                    DiscSubType = "UDO";
                    break;
                case CommonTypes.MediaType.UMD:
                    DiscType = "UMD";
                    DiscSubType = "Universal Media Disc";
                    break;
                case CommonTypes.MediaType.VCD:
                    DiscType = "CD";
                    DiscSubType = "Video CD";
                    break;
                case CommonTypes.MediaType.XGD:
                    DiscType = "DVD";
                    DiscSubType = "Xbox Game Disc (XGD)";
                    break;
                case CommonTypes.MediaType.XGD2:
                    DiscType = "DVD";
                    DiscSubType = "Xbox 360 Game Disc (XGD2)";
                    break;
                case CommonTypes.MediaType.XGD3:
                    DiscType = "DVD";
                    DiscSubType = "Xbox 360 Game Disc (XGD3)";
                    break;
                case CommonTypes.MediaType.XGD4:
                    DiscType = "BD";
                    DiscSubType = "Xbox One Game Disc (XGD4)";
                    break;
                case CommonTypes.MediaType.Apple32SS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.2";
                    break;
                case CommonTypes.MediaType.Apple32DS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.2 (double-sided)";
                    break;
                case CommonTypes.MediaType.Apple33SS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.3";
                    break;
                case CommonTypes.MediaType.Apple33DS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.3 (double-sided)";
                    break;
                case CommonTypes.MediaType.AppleSonySS:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Apple 400K";
                    break;
                case CommonTypes.MediaType.AppleSonyDS:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Apple 800K";
                    break;
                case CommonTypes.MediaType.AppleFileWare:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple FileWare";
                    break;
                case CommonTypes.MediaType.DOS_525_SS_DD_8:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 8 sectors";
                    break;
                case CommonTypes.MediaType.DOS_525_SS_DD_9:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 9 sectors";
                    break;
                case CommonTypes.MediaType.DOS_525_DS_DD_8:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 8 sectors";
                    break;
                case CommonTypes.MediaType.DOS_525_DS_DD_9:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 9 sectors";
                    break;
                case CommonTypes.MediaType.DOS_525_HD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM high-density";
                    break;
                case CommonTypes.MediaType.DOS_35_SS_DD_8:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 8 sectors";
                    break;
                case CommonTypes.MediaType.DOS_35_SS_DD_9:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 9 sectors";
                    break;
                case CommonTypes.MediaType.DOS_35_DS_DD_8:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 8 sectors";
                    break;
                case CommonTypes.MediaType.DOS_35_DS_DD_9:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 9 sectors";
                    break;
                case CommonTypes.MediaType.DOS_35_HD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM high-density";
                    break;
                case CommonTypes.MediaType.DOS_35_ED:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM extra-density";
                    break;
                case CommonTypes.MediaType.DMF:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Microsoft DMF";
                    break;
                case CommonTypes.MediaType.DMF_82:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Microsoft DMF (82-track)";
                    break;
                case CommonTypes.MediaType.XDF_35:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM XDF";
                    break;
                case CommonTypes.MediaType.XDF_525:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM XDF";
                    break;
                case CommonTypes.MediaType.IBM23FD:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 23FD";
                    break;
                case CommonTypes.MediaType.IBM33FD_128:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 33FD (128 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM33FD_256:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 33FD (256 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM33FD_512:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 33FD (512 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM43FD_128:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 43FD (128 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM43FD_256:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 43FD (256 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM53FD_256:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 53FD (256 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM53FD_512:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 53FD (512 bytes/sector)";
                    break;
                case CommonTypes.MediaType.IBM53FD_1024:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 53FD (1024 bytes/sector)";
                    break;
                case CommonTypes.MediaType.RX01:
                    DiscType = "8\" floppy";
                    DiscSubType = "DEC RX-01";
                    break;
                case CommonTypes.MediaType.RX02:
                    DiscType = "8\" floppy";
                    DiscSubType = "DEC RX-02";
                    break;
                case CommonTypes.MediaType.ACORN_525_SS_SD_40:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "BBC Micro 100K";
                    break;
                case CommonTypes.MediaType.ACORN_525_SS_SD_80:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "BBC Micro 200K";
                    break;
                case CommonTypes.MediaType.ACORN_525_SS_DD_40:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Acorn S";
                    break;
                case CommonTypes.MediaType.ACORN_525_SS_DD_80:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Acorn M";
                    break;
                case CommonTypes.MediaType.ACORN_525_DS_DD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Acorn L";
                    break;
                case CommonTypes.MediaType.ACORN_35_DS_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Acorn Archimedes";
                    break;
                case CommonTypes.MediaType.ATARI_525_SD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Atari single-density";
                    break;
                case CommonTypes.MediaType.ATARI_525_ED:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Atari enhanced-density";
                    break;
                case CommonTypes.MediaType.ATARI_525_DD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Atari double-density";
                    break;
                case CommonTypes.MediaType.CBM_1540:
                case CommonTypes.MediaType.CBM_1540_Ext:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Commodore 1540/1541";
                    break;
                case CommonTypes.MediaType.CBM_1571:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Commodore 1571";
                    break;
                case CommonTypes.MediaType.CBM_35_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Commodore 1581";
                    break;
                case CommonTypes.MediaType.CBM_AMIGA_35_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Amiga double-density";
                    break;
                case CommonTypes.MediaType.CBM_AMIGA_35_HD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Amiga high-density";
                    break;
                case CommonTypes.MediaType.NEC_8_SD:
                    DiscType = "8\" floppy";
                    DiscSubType = "NEC single-sided";
                    break;
                case CommonTypes.MediaType.NEC_8_DD:
                    DiscType = "8\" floppy";
                    DiscSubType = "NEC double-sided";
                    break;
                case CommonTypes.MediaType.NEC_525_HD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "NEC high-density";
                    break;
                case CommonTypes.MediaType.NEC_35_HD_8:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "NEC high-density";
                    break;
                case CommonTypes.MediaType.NEC_35_HD_15:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "NEC floppy mode 3";
                    break;
                case CommonTypes.MediaType.SHARP_525:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Sharp";
                    break;
                case CommonTypes.MediaType.SHARP_35:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Sharp";
                    break;
                case CommonTypes.MediaType.ECMA_54:
                    DiscType = "8\" floppy";
                    DiscSubType = "ECMA-54";
                    break;
                case CommonTypes.MediaType.ECMA_59:
                    DiscType = "8\" floppy";
                    DiscSubType = "ECMA-59";
                    break;
                case CommonTypes.MediaType.ECMA_69_8:
                case CommonTypes.MediaType.ECMA_69_15:
                case CommonTypes.MediaType.ECMA_69_26:
                    DiscType = "8\" floppy";
                    DiscSubType = "ECMA-69";
                    break;
                case CommonTypes.MediaType.ECMA_66:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-66";
                    break;
                case CommonTypes.MediaType.ECMA_70:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-70";
                    break;
                case CommonTypes.MediaType.ECMA_78:
                case CommonTypes.MediaType.ECMA_78_2:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-78";
                    break;
                case CommonTypes.MediaType.ECMA_99_8:
                case CommonTypes.MediaType.ECMA_99_15:
                case CommonTypes.MediaType.ECMA_99_26:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-99";
                    break;
                case CommonTypes.MediaType.ECMA_100:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "ECMA-99";
                    break;
                case CommonTypes.MediaType.ECMA_125:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "ECMA-125";
                    break;
                case CommonTypes.MediaType.ECMA_147:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "ECMA-147";
                    break;
                case CommonTypes.MediaType.FDFORMAT_525_DD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "FDFORMAT double-density";
                    break;
                case CommonTypes.MediaType.FDFORMAT_525_HD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "FDFORMAT high-density";
                    break;
                case CommonTypes.MediaType.FDFORMAT_35_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "FDFORMAT double-density";
                    break;
                case CommonTypes.MediaType.FDFORMAT_35_HD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "FDFORMAT high-density";
                    break;
                case CommonTypes.MediaType.ECMA_260:
                case CommonTypes.MediaType.ECMA_260_Double:
                    DiscType = "356mm magneto-optical";
                    DiscSubType = "ECMA-260 / ISO 15898";
                    break;
                case CommonTypes.MediaType.ECMA_183_512:
                case CommonTypes.MediaType.ECMA_183:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-183";
                    break;
                case CommonTypes.MediaType.ECMA_184_512:
                case CommonTypes.MediaType.ECMA_184:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-184";
                    break;
                case CommonTypes.MediaType.ECMA_154:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-154";
                    break;
                case CommonTypes.MediaType.ECMA_201:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-201";
                    break;
                case CommonTypes.MediaType.FlashDrive:
                    DiscType = "USB flash drive";
                    DiscSubType = "USB flash drive";
                    break;
                case CommonTypes.MediaType.SuperCDROM2:
                    DiscType = "CD";
                    DiscSubType = "Super CD-ROM²";
                    break;
                case CommonTypes.MediaType.JaguarCD:
                    DiscType = "CD";
                    DiscSubType = "Atari Jaguar CD";
                    break;
                case CommonTypes.MediaType.ThreeDO:
                    DiscType = "CD";
                    DiscSubType = "3DO";
                    break;
                case CommonTypes.MediaType.ZIP100:
                    DiscType = "Iomega ZIP";
                    DiscSubType = "Iomega ZIP100";
                    break;
                case CommonTypes.MediaType.ZIP250:
                    DiscType = "Iomega ZIP";
                    DiscSubType = "Iomega ZIP250";
                    break;
                case CommonTypes.MediaType.ZIP750:
                    DiscType = "Iomega ZIP";
                    DiscSubType = "Iomega ZIP750";
                    break;
                case CommonTypes.MediaType.AppleProfile:
                    DiscType = "HDD";
                    DiscSubType = "Apple Profile";
                    break;
                case CommonTypes.MediaType.AppleWidget:
                    DiscType = "HDD";
                    DiscSubType = "Apple Widget";
                    break;
                case CommonTypes.MediaType.AppleHD20:
                    DiscType = "HDD";
                    DiscSubType = "Apple HD20";
                    break;
                case CommonTypes.MediaType.PriamDataTower:
                    DiscType = "HDD";
                    DiscSubType = "Priam DataTower";
                    break;
                case CommonTypes.MediaType.DDS1:
                    DiscType = "DDS";
                    DiscSubType = "DDS";
                    break;
                case CommonTypes.MediaType.DDS2:
                    DiscType = "DDS";
                    DiscSubType = "DDS-2";
                    break;
                case CommonTypes.MediaType.DDS3:
                    DiscType = "DDS";
                    DiscSubType = "DDS-3";
                    break;
                case CommonTypes.MediaType.DDS4:
                    DiscType = "DDS";
                    DiscSubType = "DDS-4";
                    break;
                case CommonTypes.MediaType.PocketZip:
                    DiscType = "Iomega PocketZip";
                    DiscSubType = "Iomega PocketZip";
                    break;
                default:
                    DiscType = "Unknown";
                    DiscSubType = "Unknown";
                    break;
            }
        }
    }
}

