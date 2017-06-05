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
                    DiscType = "Blu-ray";
                    DiscSubType = "BD-R";
                    break;
                case CommonTypes.MediaType.BDRE:
                    DiscType = "Blu-ray";
                    DiscSubType = "BD-RE";
                    break;
                case CommonTypes.MediaType.BDREXL:
                    DiscType = "Blu-ray";
                    DiscSubType = "BD-RE XL";
                    break;
                case CommonTypes.MediaType.BDROM:
                    DiscType = "Blu-ray";
                    DiscSubType = "BD-ROM";
                    break;
                case CommonTypes.MediaType.BDRXL:
                    DiscType = "Blu-ray";
                    DiscSubType = "BD-R XL";
                    break;
                case CommonTypes.MediaType.CBHD:
                    DiscType = "Blu-ray";
                    DiscSubType = "CBHD";
                    break;
                case CommonTypes.MediaType.CD:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD";
                    break;
                case CommonTypes.MediaType.CDDA:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD Digital Audio";
                    break;
                case CommonTypes.MediaType.CDEG:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD+EG";
                    break;
                case CommonTypes.MediaType.CDG:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD+G";
                    break;
                case CommonTypes.MediaType.CDI:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-i";
                    break;
                case CommonTypes.MediaType.CDMIDI:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD+MIDI";
                    break;
                case CommonTypes.MediaType.CDMO:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-MO";
                    break;
                case CommonTypes.MediaType.CDMRW:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-MRW";
                    break;
                case CommonTypes.MediaType.CDPLUS:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD+";
                    break;
                case CommonTypes.MediaType.CDR:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-R";
                    break;
                case CommonTypes.MediaType.CDROM:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-ROM";
                    break;
                case CommonTypes.MediaType.CDROMXA:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-ROM XA";
                    break;
                case CommonTypes.MediaType.CDRW:
                    DiscType = "Compact Disc";
                    DiscSubType = "CD-RW";
                    break;
                case CommonTypes.MediaType.CDV:
                    DiscType = "Compact Disc";
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
                    DiscType = "Compact Disc";
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
                    DiscType = "Blu-ray";
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
                    DiscType = "Compact Disc";
                    DiscSubType = "Sega Mega CD";
                    break;
                case CommonTypes.MediaType.PCD:
                    DiscType = "Compact Disc";
                    DiscSubType = "Photo CD";
                    break;
                case CommonTypes.MediaType.PS1CD:
                    DiscType = "Compact Disc";
                    DiscSubType = "PlayStation Game Disc";
                    break;
                case CommonTypes.MediaType.PS2CD:
                    DiscType = "Compact Disc";
                    DiscSubType = "PlayStation 2 Game Disc";
                    break;
                case CommonTypes.MediaType.PS2DVD:
                    DiscType = "DVD";
                    DiscSubType = "PlayStation 2 Game Disc";
                    break;
                case CommonTypes.MediaType.PS3BD:
                    DiscType = "Blu-ray";
                    DiscSubType = "PlayStation 3 Game Disc";
                    break;
                case CommonTypes.MediaType.PS3DVD:
                    DiscType = "DVD";
                    DiscSubType = "PlayStation 3 Game Disc";
                    break;
                case CommonTypes.MediaType.PS4BD:
                    DiscType = "Blu-ray";
                    DiscSubType = "PlayStation 4 Game Disc";
                    break;
                case CommonTypes.MediaType.SACD:
                    DiscType = "SACD";
                    DiscSubType = "Super Audio CD";
                    break;
                case CommonTypes.MediaType.SATURNCD:
                    DiscType = "Compact Disc";
                    DiscSubType = "Sega Saturn CD";
                    break;
                case CommonTypes.MediaType.SVCD:
                    DiscType = "Compact Disc";
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
                    DiscType = "Compact Disc";
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
                    DiscType = "Blu-ray";
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
                case CommonTypes.MediaType.ECMA_201_ROM:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-201";
                    break;
                case CommonTypes.MediaType.FlashDrive:
                    DiscType = "USB flash drive";
                    DiscSubType = "USB flash drive";
                    break;
                case CommonTypes.MediaType.SuperCDROM2:
                    DiscType = "Compact Disc";
                    DiscSubType = "Super CD-ROM²";
                    break;
                case CommonTypes.MediaType.JaguarCD:
                    DiscType = "Compact Disc";
                    DiscSubType = "Atari Jaguar CD";
                    break;
                case CommonTypes.MediaType.ThreeDO:
                    DiscType = "Compact Disc";
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
                    DiscType = "Hard Disk Drive";
                    DiscSubType = "Apple Profile";
                    break;
                case CommonTypes.MediaType.AppleWidget:
                    DiscType = "Hard Disk Drive";
                    DiscSubType = "Apple Widget";
                    break;
                case CommonTypes.MediaType.AppleHD20:
                    DiscType = "Hard Disk Drive";
                    DiscSubType = "Apple HD20";
                    break;
                case CommonTypes.MediaType.PriamDataTower:
                    DiscType = "Hard Disk Drive";
                    DiscSubType = "Priam DataTower";
                    break;
                case CommonTypes.MediaType.DDS1:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DDS";
                    break;
                case CommonTypes.MediaType.DDS2:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DDS-2";
                    break;
                case CommonTypes.MediaType.DDS3:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DDS-3";
                    break;
                case CommonTypes.MediaType.DDS4:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DDS-4";
                    break;
                case CommonTypes.MediaType.PocketZip:
                    DiscType = "Iomega PocketZip";
                    DiscSubType = "Iomega PocketZip";
                    break;
                case CommonTypes.MediaType.CompactFloppy:
                    DiscType = "3\" floppy";
                    DiscSubType = "Compact Floppy";
                    break;
                case CommonTypes.MediaType.GENERIC_HDD:
                    DiscType = "Hard Disk Drive";
                    DiscSubType = "Unknown";
                    break;
                case CommonTypes.MediaType.MDData:
                    DiscType = "MiniDisc";
                    DiscSubType = "MD-DATA";
                    break;
                case CommonTypes.MediaType.MDData2:
                    DiscType = "MiniDisc";
                    DiscSubType = "MD-DATA2";
                    break;
                case CommonTypes.MediaType.UDO2:
                    DiscType = "UDO";
                    DiscSubType = "UDO2";
                    break;
                case CommonTypes.MediaType.UDO2_WORM:
                    DiscType = "UDO";
                    DiscSubType = "UDO2 (WORM)";
                    break;
                case CommonTypes.MediaType.ADR30:
                    DiscType = "Advanced Digital Recording";
                    DiscSubType = "ADR 30";
                    break;
                case CommonTypes.MediaType.ADR50:
                    DiscType = "Advanced Digital Recording";
                    DiscSubType = "ADR 50";
                    break;
                case CommonTypes.MediaType.ADR260:
                    DiscType = "Advanced Digital Recording";
                    DiscSubType = "ADR 2.60";
                    break;
                case CommonTypes.MediaType.ADR2120:
                    DiscType = "Advanced Digital Recording";
                    DiscSubType = "ADR 2.120";
                    break;
                case CommonTypes.MediaType.AIT1:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-1";
                    break;
                case CommonTypes.MediaType.AIT1Turbo:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-1 Turbo";
                    break;
                case CommonTypes.MediaType.AIT2:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-2";
                    break;
                case CommonTypes.MediaType.AIT2Turbo:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-2 Turbo";
                    break;
                case CommonTypes.MediaType.AIT3:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-3";
                    break;
                case CommonTypes.MediaType.AIT3Ex:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-3Ex";
                    break;
                case CommonTypes.MediaType.AIT3Turbo:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-3 Turbo";
                    break;
                case CommonTypes.MediaType.AIT4:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-4";
                    break;
                case CommonTypes.MediaType.AIT5:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-5";
                    break;
                case CommonTypes.MediaType.AITETurbo:
                    DiscType = "Advanced Intelligent Tape";
                    DiscSubType = "AIT-E Turbo";
                    break;
                case CommonTypes.MediaType.SAIT1:
                    DiscType = "Super Advanced Intelligent Tape";
                    DiscSubType = "SAIT-1";
                    break;
                case CommonTypes.MediaType.SAIT2:
                    DiscType = "Super Advanced Intelligent Tape";
                    DiscSubType = "SAIT-2";
                    break;
                case CommonTypes.MediaType.Bernoulli:
                    DiscType = "Iomega Bernoulli";
                    DiscSubType = "Iomega Bernoulli";
                    break;
                case CommonTypes.MediaType.Bernoulli2:
                    DiscType = "Iomega Bernoulli";
                    DiscSubType = "Iomega Bernoulli 2";
                    break;
                case CommonTypes.MediaType.Ditto:
                    DiscType = "Iomega Ditto";
                    DiscSubType = "Iomega Ditto";
                    break;
                case CommonTypes.MediaType.DittoMax:
                    DiscType = "Iomega Ditto";
                    DiscSubType = "Iomega Ditto Max";
                    break;
                case CommonTypes.MediaType.Jaz:
                    DiscType = "Iomega Jaz";
                    DiscSubType = "Iomega Jaz 1GB";
                    break;
                case CommonTypes.MediaType.Jaz2:
                    DiscType = "Iomega Jaz";
                    DiscSubType = "Iomega Jaz 2GB";
                    break;
                case CommonTypes.MediaType.REV35:
                    DiscType = "Iomega REV";
                    DiscSubType = "Iomega REV-35";
                    break;
                case CommonTypes.MediaType.REV70:
                    DiscType = "Iomega REV";
                    DiscSubType = "Iomega REV-70";
                    break;
                case CommonTypes.MediaType.REV120:
                    DiscType = "Iomega REV";
                    DiscSubType = "Iomega REV-120";
                    break;
                case CommonTypes.MediaType.CompactFlash:
                    DiscType = "Compact Flash";
                    DiscSubType = "Compact Flash";
                    break;
                case CommonTypes.MediaType.CompactFlashType2:
                    DiscType = "Compact Flash";
                    DiscSubType = "Compact Flash Type 2";
                    break;
                case CommonTypes.MediaType.CFast:
                    DiscType = "Compact Flash";
                    DiscSubType = "CFast";
                    break;
                case CommonTypes.MediaType.DigitalAudioTape:
                    DiscType = "Digital Audio Tape";
                    DiscSubType = "Digital Audio Tape";
                    break;
                case CommonTypes.MediaType.DAT72:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DAT-72";
                    break;
                case CommonTypes.MediaType.DAT160:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DAT-160";
                    break;
                case CommonTypes.MediaType.DAT320:
                    DiscType = "Digital Data Storage";
                    DiscSubType = "DAT-320";
                    break;
                case CommonTypes.MediaType.DECtapeII:
                    DiscType = "DECtape";
                    DiscSubType = "DECtape II";
                    break;
                case CommonTypes.MediaType.CompactTapeI:
                    DiscType = "CompacTape";
                    DiscSubType = "CompacTape";
                    break;
                case CommonTypes.MediaType.CompactTapeII:
                    DiscType = "CompacTape";
                    DiscSubType = "CompacTape II";
                    break;
                case CommonTypes.MediaType.DLTtapeIII:
                    DiscType = "Digital Linear Tape";
                    DiscSubType = "DLTtape III";
                    break;
                case CommonTypes.MediaType.DLTtapeIIIxt:
                    DiscType = "Digital Linear Tape";
                    DiscSubType = "DLTtape IIIXT";
                    break;
                case CommonTypes.MediaType.DLTtapeIV:
                    DiscType = "Digital Linear Tape";
                    DiscSubType = "DLTtape IV";
                    break;
                case CommonTypes.MediaType.DLTtapeS4:
                    DiscType = "Digital Linear Tape";
                    DiscSubType = "DLTtape S4";
                    break;
                case CommonTypes.MediaType.SDLT1:
                    DiscType = "Super Digital Linear Tape";
                    DiscSubType = "SDLTtape I";
                    break;
                case CommonTypes.MediaType.SDLT2:
                    DiscType = "Super Digital Linear Tape";
                    DiscSubType = "SDLTtape II";
                    break;
                case CommonTypes.MediaType.VStapeI:
                    DiscType = "Digital Linear Tape";
                    DiscSubType = "DLTtape VS1";
                    break;
                case CommonTypes.MediaType.Exatape15m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (15m)";
                    break;
                case CommonTypes.MediaType.Exatape22m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (22m)";
                    break;
                case CommonTypes.MediaType.Exatape22mAME:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (22m AME)";
                    break;
                case CommonTypes.MediaType.Exatape28m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (28m)";
                    break;
                case CommonTypes.MediaType.Exatape40m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (40m)";
                    break;
                case CommonTypes.MediaType.Exatape45m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (45m)";
                    break;
                case CommonTypes.MediaType.Exatape54m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (54m)";
                    break;
                case CommonTypes.MediaType.Exatape75m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (75m)";
                    break;
                case CommonTypes.MediaType.Exatape76m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (76m)";
                    break;
                case CommonTypes.MediaType.Exatape80m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (80m)";
                    break;
                case CommonTypes.MediaType.Exatape106m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (106m)";
                    break;
                case CommonTypes.MediaType.Exatape112m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (112m)";
                    break;
                case CommonTypes.MediaType.Exatape125m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (125m)";
                    break;
                case CommonTypes.MediaType.Exatape150m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (150m)";
                    break;
                case CommonTypes.MediaType.Exatape160mXL:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape XL (160m)";
                    break;
                case CommonTypes.MediaType.Exatape170m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (170m)";
                    break;
                case CommonTypes.MediaType.Exatape225m:
                    DiscType = "Exatape";
                    DiscSubType = "Exatape (225m)";
                    break;
                case CommonTypes.MediaType.EZ135:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "EZ135";
                    break;
                case CommonTypes.MediaType.EZ230:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "EZ230";
                    break;
                case CommonTypes.MediaType.Quest:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "Quest";
                    break;
                case CommonTypes.MediaType.SparQ:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "SparQ";
                    break;
                case CommonTypes.MediaType.SQ100:
                    DiscType = "3.9\" SyQuest cartridge";
                    DiscSubType = "SQ100";
                    break;
                case CommonTypes.MediaType.SQ200:
                    DiscType = "3.9\" SyQuest cartridge";
                    DiscSubType = "SQ200";
                    break;
                case CommonTypes.MediaType.SQ300:
                    DiscType = "3.9\" SyQuest cartridge";
                    DiscSubType = "SQ300";
                    break;
                case CommonTypes.MediaType.SQ310:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "SQ310";
                    break;
                case CommonTypes.MediaType.SQ327:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "SQ327";
                    break;
                case CommonTypes.MediaType.SQ400:
                    DiscType = "5.25\" SyQuest cartridge";
                    DiscSubType = "SQ400";
                    break;
                case CommonTypes.MediaType.SQ800:
                    DiscType = "5.25\" SyQuest cartridge";
                    DiscSubType = "SQ800";
                    break;
                case CommonTypes.MediaType.SQ1500:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "SQ1500";
                    break;
                case CommonTypes.MediaType.SQ2000:
                    DiscType = "5.25\" SyQuest cartridge";
                    DiscSubType = "SQ2000";
                    break;
                case CommonTypes.MediaType.SyJet:
                    DiscType = "3.5\" SyQuest cartridge";
                    DiscSubType = "SyJet";
                    break;
                case CommonTypes.MediaType.LTO:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO";
                    break;
                case CommonTypes.MediaType.LTO2:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-2";
                    break;
                case CommonTypes.MediaType.LTO3:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-3";
                    break;
                case CommonTypes.MediaType.LTO3WORM:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-3 (WORM)";
                    break;
                case CommonTypes.MediaType.LTO4:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-4";
                    break;
                case CommonTypes.MediaType.LTO4WORM:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-4 (WORM)";
                    break;
                case CommonTypes.MediaType.LTO5:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-5";
                    break;
                case CommonTypes.MediaType.LTO5WORM:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-5 (WORM)";
                    break;
                case CommonTypes.MediaType.LTO6:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-6";
                    break;
                case CommonTypes.MediaType.LTO6WORM:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-6 (WORM)";
                    break;
                case CommonTypes.MediaType.LTO7:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-7";
                    break;
                case CommonTypes.MediaType.LTO7WORM:
                    DiscType = "Linear Tape-Open";
                    DiscSubType = "LTO-7 (WORM)";
                    break;
                case CommonTypes.MediaType.MemoryStick:
                    DiscType = "Memory Stick";
                    DiscSubType = "Memory Stick";
                    break;
                case CommonTypes.MediaType.MemoryStickDuo:
                    DiscType = "Memory Stick";
                    DiscSubType = "Memory Stick Duo";
                    break;
                case CommonTypes.MediaType.MemoryStickMicro:
                    DiscType = "Memory Stick";
                    DiscSubType = "Memory Stick Micro";
                    break;
                case CommonTypes.MediaType.MemoryStickPro:
                    DiscType = "Memory Stick";
                    DiscSubType = "Memory Stick Pro";
                    break;
                case CommonTypes.MediaType.MemoryStickProDuo:
                    DiscType = "Memory Stick";
                    DiscSubType = "Memory Stick PRO Duo";
                    break;
                case CommonTypes.MediaType.SecureDigital:
                    DiscType = "Secure Digital";
                    DiscSubType = "Secure Digital";
                    break;
                case CommonTypes.MediaType.miniSD:
                    DiscType = "Secure Digital";
                    DiscSubType = "miniSD";
                    break;
                case CommonTypes.MediaType.microSD:
                    DiscType = "Secure Digital";
                    DiscSubType = "microSD";
                    break;
                case CommonTypes.MediaType.MMC:
                    DiscType = "MultiMediaCard";
                    DiscSubType = "MultiMediaCard";
                    break;
                case CommonTypes.MediaType.MMCmicro:
                    DiscType = "MultiMediaCard";
                    DiscSubType = "MMCmicro";
                    break;
                case CommonTypes.MediaType.RSMMC:
                    DiscType = "MultiMediaCard";
                    DiscSubType = "Reduced-Size MultiMediaCard";
                    break;
                case CommonTypes.MediaType.MMCplus:
                    DiscType = "MultiMediaCard";
                    DiscSubType = "MMCplus";
                    break;
                case CommonTypes.MediaType.MMCmobile:
                    DiscType = "MultiMediaCard";
                    DiscSubType = "MMCmobile";
                    break;
                case CommonTypes.MediaType.MLR1:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "MLR1";
                    break;
                case CommonTypes.MediaType.MLR1SL:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "MLR1 SL";
                    break;
                case CommonTypes.MediaType.MLR3:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "MLR3";
                    break;
                case CommonTypes.MediaType.SLR1:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR1";
                    break;
                case CommonTypes.MediaType.SLR2:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR2";
                    break;
                case CommonTypes.MediaType.SLR3:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR3";
                    break;
                case CommonTypes.MediaType.SLR32:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR32";
                    break;
                case CommonTypes.MediaType.SLR32SL:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR32 SL";
                    break;
                case CommonTypes.MediaType.SLR4:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR4";
                    break;
                case CommonTypes.MediaType.SLR5:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR5";
                    break;
                case CommonTypes.MediaType.SLR5SL:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR5 SL";
                    break;
                case CommonTypes.MediaType.SLR6:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLR6";
                    break;
                case CommonTypes.MediaType.SLRtape7:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape7";
                    break;
                case CommonTypes.MediaType.SLRtape7SL:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape7 SL";
                    break;
                case CommonTypes.MediaType.SLRtape24:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape24";
                    break;
                case CommonTypes.MediaType.SLRtape24SL:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape24 SL";
                    break;
                case CommonTypes.MediaType.SLRtape40:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape40";
                    break;
                case CommonTypes.MediaType.SLRtape50:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape50";
                    break;
                case CommonTypes.MediaType.SLRtape60:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape60";
                    break;
                case CommonTypes.MediaType.SLRtape75:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape75";
                    break;
                case CommonTypes.MediaType.SLRtape100:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape100";
                    break;
                case CommonTypes.MediaType.SLRtape140:
                    DiscType = "Scalable Linear Recording";
                    DiscSubType = "SLRtape140";
                    break;
                case CommonTypes.MediaType.QIC11:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-11";
                    break;
                case CommonTypes.MediaType.QIC24:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-24";
                    break;
                case CommonTypes.MediaType.QIC40:
                    DiscType = "Quarter-inch mini cartridge";
                    DiscSubType = "QIC-40";
                    break;
                case CommonTypes.MediaType.QIC80:
                    DiscType = "Quarter-inch mini cartridge";
                    DiscSubType = "QIC-80";
                    break;
                case CommonTypes.MediaType.QIC120:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-120";
                    break;
                case CommonTypes.MediaType.QIC150:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-150";
                    break;
                case CommonTypes.MediaType.QIC320:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-320";
                    break;
                case CommonTypes.MediaType.QIC525:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-525";
                    break;
                case CommonTypes.MediaType.QIC1350:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-1350";
                    break;
                case CommonTypes.MediaType.QIC3010:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-3010";
                    break;
                case CommonTypes.MediaType.QIC3020:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-3020";
                    break;
                case CommonTypes.MediaType.QIC3080:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-3080";
                    break;
                case CommonTypes.MediaType.QIC3095:
                    DiscType = "Quarter-inch cartridge";
                    DiscSubType = "QIC-3095";
                    break;
                case CommonTypes.MediaType.Travan:
                    DiscType = "Travan";
                    DiscSubType = "TR-1";
                    break;
                case CommonTypes.MediaType.Travan1Ex:
                    DiscType = "Travan";
                    DiscSubType = "TR-1 Ex";
                    break;
                case CommonTypes.MediaType.Travan3:
                    DiscType = "Travan";
                    DiscSubType = "TR-3";
                    break;
                case CommonTypes.MediaType.Travan3Ex:
                    DiscType = "Travan";
                    DiscSubType = "TR-3 Ex";
                    break;
                case CommonTypes.MediaType.Travan4:
                    DiscType = "Travan";
                    DiscSubType = "TR-4";
                    break;
                case CommonTypes.MediaType.Travan5:
                    DiscType = "Travan";
                    DiscSubType = "TR-5";
                    break;
                case CommonTypes.MediaType.Travan7:
                    DiscType = "Travan";
                    DiscSubType = "TR-7";
                    break;
                case CommonTypes.MediaType.VXA1:
                    DiscType = "VXA";
                    DiscSubType = "VXA-1";
                    break;
                case CommonTypes.MediaType.VXA2:
                    DiscType = "VXA";
                    DiscSubType = "VXA-2";
                    break;
                case CommonTypes.MediaType.VXA3:
                    DiscType = "VXA";
                    DiscSubType = "VXA-3";
                    break;
                case CommonTypes.MediaType.ECMA_153:
                case CommonTypes.MediaType.ECMA_153_512:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-153";
                    break;
                case CommonTypes.MediaType.ECMA_189:
                    DiscType = "300mm magneto optical";
                    DiscSubType = "ECMA-189";
                    break;
                case CommonTypes.MediaType.ECMA_190:
                    DiscType = "300mm magneto optical";
                    DiscSubType = "ECMA-190";
                    break;
                case CommonTypes.MediaType.ECMA_195:
                case CommonTypes.MediaType.ECMA_195_512:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-195";
                    break;
                case CommonTypes.MediaType.ECMA_223:
                case CommonTypes.MediaType.ECMA_223_512:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-223";
                    break;
                case CommonTypes.MediaType.ECMA_238:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-238";
                    break;
                case CommonTypes.MediaType.ECMA_239:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-239";
                    break;
                case CommonTypes.MediaType.ECMA_280:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-280";
                    break;
                case CommonTypes.MediaType.ECMA_317:
                    DiscType = "300mm magneto optical";
                    DiscSubType = "ECMA-317";
                    break;
                case CommonTypes.MediaType.ECMA_322:
                case CommonTypes.MediaType.ECMA_322_2k:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-322";
                    break;
                case CommonTypes.MediaType.GigaMo:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "GIGAMO";
                    break;
                case CommonTypes.MediaType.GigaMo2:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "2.3GB GIGAMO";
                    break;
                case CommonTypes.MediaType.UnknownMO:
                    DiscType = "Magneto-optical";
                    DiscSubType = "Unknown";
                    break;
                case CommonTypes.MediaType.Floptical:
                    DiscType = "Floptical";
                    DiscSubType = "Floptical";
                    break;
                case CommonTypes.MediaType.HiFD:
                    DiscType = "HiFD";
                    DiscSubType = "HiFD";
                    break;
                case CommonTypes.MediaType.LS120:
                    DiscType = "SuperDisk";
                    DiscSubType = "LS-120";
                    break;
                case CommonTypes.MediaType.LS240:
                    DiscType = "SuperDisk";
                    DiscSubType = "LS-240";
                    break;
                case CommonTypes.MediaType.UHD144:
                    DiscType = "UHD144";
                    DiscSubType = "UHD144";
                    break;
                default:
                    DiscType = "Unknown";
                    DiscSubType = "Unknown";
                    break;
            }
        }
    }
}

