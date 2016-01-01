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

namespace DiscImageChef.Metadata
{
    public static class DiskType
    {
        public static void DiskTypeToString(CommonTypes.DiskType dskType, out string DiscType, out string DiscSubType)
        {
            switch (dskType)
            {
                case CommonTypes.DiskType.BDR:
                    DiscType = "BD";
                    DiscSubType = "BD-R";
                    break;
                case CommonTypes.DiskType.BDRE:
                    DiscType = "BD";
                    DiscSubType = "BD-RE";
                    break;
                case CommonTypes.DiskType.BDREXL:
                    DiscType = "BD";
                    DiscSubType = "BD-RE XL";
                    break;
                case CommonTypes.DiskType.BDROM:
                    DiscType = "BD";
                    DiscSubType = "BD-ROM";
                    break;
                case CommonTypes.DiskType.BDRXL:
                    DiscType = "BD";
                    DiscSubType = "BD-R XL";
                    break;
                case CommonTypes.DiskType.CBHD:
                    DiscType = "BD";
                    DiscSubType = "CBHD";
                    break;
                case CommonTypes.DiskType.CD:
                    DiscType = "CD";
                    DiscSubType = "CD";
                    break;
                case CommonTypes.DiskType.CDDA:
                    DiscType = "CD";
                    DiscSubType = "CD Digital Audio";
                    break;
                case CommonTypes.DiskType.CDEG:
                    DiscType = "CD";
                    DiscSubType = "CD+EG";
                    break;
                case CommonTypes.DiskType.CDG:
                    DiscType = "CD";
                    DiscSubType = "CD+G";
                    break;
                case CommonTypes.DiskType.CDI:
                    DiscType = "CD";
                    DiscSubType = "CD-i";
                    break;
                case CommonTypes.DiskType.CDMIDI:
                    DiscType = "CD";
                    DiscSubType = "CD+MIDI";
                    break;
                case CommonTypes.DiskType.CDMO:
                    DiscType = "CD";
                    DiscSubType = "CD-MO";
                    break;
                case CommonTypes.DiskType.CDMRW:
                    DiscType = "CD";
                    DiscSubType = "CD-MRW";
                    break;
                case CommonTypes.DiskType.CDPLUS:
                    DiscType = "CD";
                    DiscSubType = "CD+";
                    break;
                case CommonTypes.DiskType.CDR:
                    DiscType = "CD";
                    DiscSubType = "CD-R";
                    break;
                case CommonTypes.DiskType.CDROM:
                    DiscType = "CD";
                    DiscSubType = "CD-ROM";
                    break;
                case CommonTypes.DiskType.CDROMXA:
                    DiscType = "CD";
                    DiscSubType = "CD-ROM XA";
                    break;
                case CommonTypes.DiskType.CDRW:
                    DiscType = "CD";
                    DiscSubType = "CD-RW";
                    break;
                case CommonTypes.DiskType.CDV:
                    DiscType = "CD";
                    DiscSubType = "CD-Video";
                    break;
                case CommonTypes.DiskType.DDCD:
                    DiscType = "DDCD";
                    DiscSubType = "DDCD";
                    break;
                case CommonTypes.DiskType.DDCDR:
                    DiscType = "DDCD";
                    DiscSubType = "DDCD-R";
                    break;
                case CommonTypes.DiskType.DDCDRW:
                    DiscType = "DDCD";
                    DiscSubType = "DDCD-RW";
                    break;
                case CommonTypes.DiskType.DTSCD:
                    DiscType = "CD";
                    DiscSubType = "DTS CD";
                    break;
                case CommonTypes.DiskType.DVDDownload:
                    DiscType = "DVD";
                    DiscSubType = "DVD-Download";
                    break;
                case CommonTypes.DiskType.DVDPR:
                    DiscType = "DVD";
                    DiscSubType = "DVD+R";
                    break;
                case CommonTypes.DiskType.DVDPRDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD+R DL";
                    break;
                case CommonTypes.DiskType.DVDPRW:
                    DiscType = "DVD";
                    DiscSubType = "DVD+RW";
                    break;
                case CommonTypes.DiskType.DVDPRWDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD+RW DL";
                    break;
                case CommonTypes.DiskType.DVDR:
                    DiscType = "DVD";
                    DiscSubType = "DVD-R";
                    break;
                case CommonTypes.DiskType.DVDRAM:
                    DiscType = "DVD";
                    DiscSubType = "DVD-RAM";
                    break;
                case CommonTypes.DiskType.DVDRDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD-R DL";
                    break;
                case CommonTypes.DiskType.DVDROM:
                    DiscType = "DVD";
                    DiscSubType = "DVD-ROM";
                    break;
                case CommonTypes.DiskType.DVDRW:
                    DiscType = "DVD";
                    DiscSubType = "DVD-RW";
                    break;
                case CommonTypes.DiskType.DVDRWDL:
                    DiscType = "DVD";
                    DiscSubType = "DVD-RW";
                    break;
                case CommonTypes.DiskType.EVD:
                    DiscType = "EVD";
                    DiscSubType = "EVD";
                    break;
                case CommonTypes.DiskType.FDDVD:
                    DiscType = "FDDVD";
                    DiscSubType = "FDDVD";
                    break;
                case CommonTypes.DiskType.FVD:
                    DiscType = "FVD";
                    DiscSubType = "FVD";
                    break;
                case CommonTypes.DiskType.GDR:
                    DiscType = "GD";
                    DiscSubType = "GD-R";
                    break;
                case CommonTypes.DiskType.GDROM:
                    DiscType = "GD";
                    DiscSubType = "GD-ROM";
                    break;
                case CommonTypes.DiskType.GOD:
                    DiscType = "DVD";
                    DiscSubType = "GameCube Game Disc";
                    break;
                case CommonTypes.DiskType.WOD:
                    DiscType = "DVD";
                    DiscSubType = "Wii Optical Disc";
                    break;
                case CommonTypes.DiskType.WUOD:
                    DiscType = "BD";
                    DiscSubType = "Wii U Optical Disc";
                    break;
                case CommonTypes.DiskType.HDDVDR:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-R";
                    break;
                case CommonTypes.DiskType.HDDVDRAM:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-RAM";
                    break;
                case CommonTypes.DiskType.HDDVDRDL:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-R DL";
                    break;
                case CommonTypes.DiskType.HDDVDROM:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-ROM";
                    break;
                case CommonTypes.DiskType.HDDVDRW:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-RW";
                    break;
                case CommonTypes.DiskType.HDDVDRWDL:
                    DiscType = "HD DVD";
                    DiscSubType = "HD DVD-RW DL";
                    break;
                case CommonTypes.DiskType.HDVMD:
                    DiscType = "HD VMD";
                    DiscSubType = "HD VMD";
                    break;
                case CommonTypes.DiskType.HiMD:
                    DiscType = "MiniDisc";
                    DiscSubType = "HiMD";
                    break;
                case CommonTypes.DiskType.HVD:
                    DiscType = "HVD";
                    DiscSubType = "HVD";
                    break;
                case CommonTypes.DiskType.LD:
                    DiscType = "LaserDisc";
                    DiscSubType = "LaserDisc";
                    break;
                case CommonTypes.DiskType.LDROM:
                    DiscType = "LaserDisc";
                    DiscSubType = "LD-ROM";
                    break;
                case CommonTypes.DiskType.MD:
                    DiscType = "MiniDisc";
                    DiscSubType = "MiniDisc";
                    break;
                case CommonTypes.DiskType.MEGACD:
                    DiscType = "CD";
                    DiscSubType = "Sega Mega CD";
                    break;
                case CommonTypes.DiskType.PCD:
                    DiscType = "CD";
                    DiscSubType = "Photo CD";
                    break;
                case CommonTypes.DiskType.PS1CD:
                    DiscType = "CD";
                    DiscSubType = "PlayStation Game Disc";
                    break;
                case CommonTypes.DiskType.PS2CD:
                    DiscType = "CD";
                    DiscSubType = "PlayStation 2 Game Disc";
                    break;
                case CommonTypes.DiskType.PS2DVD:
                    DiscType = "DVD";
                    DiscSubType = "PlayStation 2 Game Disc";
                    break;
                case CommonTypes.DiskType.PS3BD:
                    DiscType = "BD";
                    DiscSubType = "PlayStation 3 Game Disc";
                    break;
                case CommonTypes.DiskType.PS3DVD:
                    DiscType = "DVD";
                    DiscSubType = "PlayStation 3 Game Disc";
                    break;
                case CommonTypes.DiskType.PS4BD:
                    DiscType = "BD";
                    DiscSubType = "PlayStation 4 Game Disc";
                    break;
                case CommonTypes.DiskType.SACD:
                    DiscType = "SACD";
                    DiscSubType = "Super Audio CD";
                    break;
                case CommonTypes.DiskType.SATURNCD:
                    DiscType = "CD";
                    DiscSubType = "Sega Saturn CD";
                    break;
                case CommonTypes.DiskType.SVCD:
                    DiscType = "CD";
                    DiscSubType = "Super Video CD";
                    break;
                case CommonTypes.DiskType.SVOD:
                    DiscType = "SVOD";
                    DiscSubType = "SVOD";
                    break;
                case CommonTypes.DiskType.UDO:
                    DiscType = "UDO";
                    DiscSubType = "UDO";
                    break;
                case CommonTypes.DiskType.UMD:
                    DiscType = "UMD";
                    DiscSubType = "Universal Media Disc";
                    break;
                case CommonTypes.DiskType.VCD:
                    DiscType = "CD";
                    DiscSubType = "Video CD";
                    break;
                case CommonTypes.DiskType.XGD:
                    DiscType = "DVD";
                    DiscSubType = "Xbox Game Disc (XGD)";
                    break;
                case CommonTypes.DiskType.XGD2:
                    DiscType = "DVD";
                    DiscSubType = "Xbox 360 Game Disc (XGD2)";
                    break;
                case CommonTypes.DiskType.XGD3:
                    DiscType = "DVD";
                    DiscSubType = "Xbox 360 Game Disc (XGD3)";
                    break;
                case CommonTypes.DiskType.XGD4:
                    DiscType = "BD";
                    DiscSubType = "Xbox One Game Disc (XGD4)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.Apple32SS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.2";
                    break;
                case DiscImageChef.CommonTypes.DiskType.Apple32DS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.2 (double-sided)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.Apple33SS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.3";
                    break;
                case DiscImageChef.CommonTypes.DiskType.Apple33DS:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple DOS 3.3 (double-sided)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.AppleSonySS:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Apple 400K";
                    break;
                case DiscImageChef.CommonTypes.DiskType.AppleSonyDS:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Apple 800K";
                    break;
                case DiscImageChef.CommonTypes.DiskType.AppleFileWare:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Apple FileWare";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_525_SS_DD_8:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 8 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_525_SS_DD_9:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 9 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_525_DS_DD_8:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 8 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_525_DS_DD_9:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 9 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_525_HD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_35_SS_DD_8:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 8 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_35_SS_DD_9:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, single-sided, 9 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_35_DS_DD_8:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 8 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_35_DS_DD_9:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM double-density, double-sided, 9 sectors";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_35_HD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DOS_35_ED:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM extra-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DMF:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Microsoft DMF";
                    break;
                case DiscImageChef.CommonTypes.DiskType.DMF_82:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Microsoft DMF (82-track)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.XDF_35:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "IBM XDF";
                    break;
                case DiscImageChef.CommonTypes.DiskType.XDF_525:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "IBM XDF";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM23FD:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 23FD";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM33FD_128:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 33FD (128 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM33FD_256:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 33FD (256 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM33FD_512:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 33FD (512 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM43FD_128:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 43FD (128 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM43FD_256:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 43FD (256 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM53FD_256:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 53FD (256 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM53FD_512:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 53FD (512 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.IBM53FD_1024:
                    DiscType = "8\" floppy";
                    DiscSubType = "IBM 53FD (1024 bytes/sector)";
                    break;
                case DiscImageChef.CommonTypes.DiskType.RX01:
                    DiscType = "8\" floppy";
                    DiscSubType = "DEC RX-01";
                    break;
                case DiscImageChef.CommonTypes.DiskType.RX02:
                    DiscType = "8\" floppy";
                    DiscSubType = "DEC RX-02";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_SD_40:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "BBC Micro 100K";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_SD_80:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "BBC Micro 200K";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_DD_40:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Acorn S";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_DD_80:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Acorn M";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_DS_DD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Acorn L";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ACORN_35_DS_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Acorn Archimedes";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ATARI_525_SD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Atari single-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ATARI_525_ED:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Atari enhanced-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ATARI_525_DD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Atari double-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.CBM_1540:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Commodore 1540/1541";
                    break;
                case DiscImageChef.CommonTypes.DiskType.CBM_35_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Commodore 1581";
                    break;
                case DiscImageChef.CommonTypes.DiskType.CBM_AMIGA_35_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Amiga double-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.CBM_AMIGA_35_HD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Amiga high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.NEC_8_SD:
                    DiscType = "8\" floppy";
                    DiscSubType = "NEC single-sided";
                    break;
                case DiscImageChef.CommonTypes.DiskType.NEC_8_DD:
                    DiscType = "8\" floppy";
                    DiscSubType = "NEC double-sided";
                    break;
                case DiscImageChef.CommonTypes.DiskType.NEC_525_HD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "NEC high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.NEC_35_HD_8:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "NEC high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.NEC_35_HD_15:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "NEC floppy mode 3";
                    break;
                case DiscImageChef.CommonTypes.DiskType.SHARP_525:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "Sharp";
                    break;
                case DiscImageChef.CommonTypes.DiskType.SHARP_35:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "Sharp";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_54:
                    DiscType = "8\" floppy";
                    DiscSubType = "ECMA-54";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_59:
                    DiscType = "8\" floppy";
                    DiscSubType = "ECMA-59";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_69_8:
                case DiscImageChef.CommonTypes.DiskType.ECMA_69_15:
                case DiscImageChef.CommonTypes.DiskType.ECMA_69_26:
                    DiscType = "8\" floppy";
                    DiscSubType = "ECMA-69";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_66:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-66";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_70:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-70";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_78:
                case DiscImageChef.CommonTypes.DiskType.ECMA_78_2:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-78";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_99_8:
                case DiscImageChef.CommonTypes.DiskType.ECMA_99_15:
                case DiscImageChef.CommonTypes.DiskType.ECMA_99_26:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "ECMA-99";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_100:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "ECMA-99";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_125:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "ECMA-125";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_147:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "ECMA-147";
                    break;
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_525_DD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "FDFORMAT double-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_525_HD:
                    DiscType = "5.25\" floppy";
                    DiscSubType = "FDFORMAT high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_35_DD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "FDFORMAT double-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_35_HD:
                    DiscType = "3.5\" floppy";
                    DiscSubType = "FDFORMAT high-density";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_183_512:
                case DiscImageChef.CommonTypes.DiskType.ECMA_183_1024:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-183";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_184_512:
                case DiscImageChef.CommonTypes.DiskType.ECMA_184_1024:
                    DiscType = "5.25\" magneto-optical";
                    DiscSubType = "ECMA-184";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_154:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-154";
                    break;
                case DiscImageChef.CommonTypes.DiskType.ECMA_201:
                    DiscType = "3.5\" magneto-optical";
                    DiscSubType = "ECMA-201";
                    break;
                default:
                    DiscType = "Unknown";
                    DiscSubType = "Unknown";
                    break;
            }
        }
    }
}

