// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Types.cs
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

namespace DiscImageChef.Decoders.SCSI
{
    public enum MediumTypes : byte
    {
        Default = 0x00,
        #region Medium Types defined in ECMA-111 for Direct-Access devices

        /// <summary>
        /// ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side
        /// </summary>
        ECMA54 = 0x09,
        /// <summary>
        /// ECMA-59 &amp; ANSI X3.121-1984: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides
        /// </summary>
        ECMA59 = 0x0A,
        /// <summary>
        /// ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides
        /// </summary>
        ECMA69 = 0x0B,
        /// <summary>
        /// ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side
        /// </summary>
        ECMA66 = 0x0E,
        /// <summary>
        /// ECMA-70 &amp; ANSI X3.125-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm
        /// </summary>
        ECMA70 = 0x12,
        /// <summary>
        /// ECMA-78 &amp; ANSI X3.126-1986: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm
        /// </summary>
        ECMA78 = 0x16,
        /// <summary>
        /// ECMA-99 &amp; ISO 8630-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm
        /// </summary>
        ECMA99 = 0x1A,
        /// <summary>
        /// ECMA-100 &amp; ANSI X3.137: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm
        /// </summary>
        ECMA100 = 0x1E,
        #endregion Medium Types defined in ECMA-111 for Direct-Access devices

        #region Medium Types defined in SCSI-2 for Direct-Access devices

        /// <summary>
        /// Unspecified single sided flexible disk
        /// </summary>
        Unspecified_SS = 0x01,
        /// <summary>
        /// Unspecified double sided flexible disk
        /// </summary>
        Unspecified_DS = 0x02,
        /// <summary>
        /// ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 1 side
        /// </summary>
        X3_73 = 0x05,
        /// <summary>
        /// ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 2 sides
        /// </summary>
        X3_73_DS = 0x06,
        /// <summary>
        /// ANSI X3.80-1980: 130 mm, 3979 ftprad, 1,9 Tracks per mm, 1 side
        /// </summary>
        X3_82 = 0x0D,
        /// <summary>
        /// 6,3 mm tape with 12 tracks at 394 ftpmm
        /// </summary>
        Tape12 = 0x40,
        /// <summary>
        /// 6,3 mm tape with 24 tracks at 394 ftpmm
        /// </summary>
        Tape24 = 0x44,
        #endregion Medium Types defined in SCSI-2 for Direct-Access devices

        #region Medium Types defined in SCSI-3 SBC-1 for Optical devices

        /// <summary>
        /// Read-only medium
        /// </summary>
        ReadOnly = 0x01,
        /// <summary>
        /// Write-once Read-many medium
        /// </summary>
        WORM = 0x02,
        /// <summary>
        /// Erasable medium
        /// </summary>
        Erasable = 0x03,
        /// <summary>
        /// Combination of read-only and write-once medium
        /// </summary>
        RO_WORM = 0x04,
        /// <summary>
        /// Combination of read-only and erasable medium
        /// </summary>
        RO_RW = 0x05,
        /// <summary>
        /// Combination of write-once and erasable medium
        /// </summary>
        WORM_RW = 0x06,
        #endregion Medium Types defined in SCSI-3 SBC-1 for Optical devices

        #region Medium Types defined in SCSI-2 for MultiMedia devices

        /// <summary>
        /// 120 mm CD-ROM
        /// </summary>
        CDROM = 0x01,
        /// <summary>
        /// 120 mm Compact Disc Digital Audio
        /// </summary>
        CDDA = 0x02,
        /// <summary>
        /// 120 mm Compact Disc with data and audio
        /// </summary>
        MixedCD = 0x03,
        /// <summary>
        /// 80 mm CD-ROM
        /// </summary>
        CDROM_80 = 0x05,
        /// <summary>
        /// 80 mm Compact Disc Digital Audio
        /// </summary>
        CDDA_80 = 0x06,
        /// <summary>
        /// 80 mm Compact Disc with data and audio
        /// </summary>
        MixedCD_80 = 0x07,

        #endregion Medium Types defined in SCSI-2 for MultiMedia devices

        #region Medium Types defined in SFF-8020i

        /// <summary>
        /// Unknown medium type
        /// </summary>
        Unknown_CD = 0x00,
        /// <summary>
        /// 120 mm Hybrid disc (Photo CD)
        /// </summary>
        HybridCD = 0x04,
        /// <summary>
        /// Unknown size CD-R
        /// </summary>
        Unknown_CDR = 0x10,
        /// <summary>
        /// 120 mm CD-R with data only
        /// </summary>
        CDR = 0x11,
        /// <summary>
        /// 120 mm CD-R with audio only
        /// </summary>
        CDR_DA = 0x12,
        /// <summary>
        /// 120 mm CD-R with data and audio
        /// </summary>
        CDR_Mixed = 0x13,
        /// <summary>
        /// 120 mm Hybrid CD-R (Photo CD)
        /// </summary>
        HybridCDR = 0x14,
        /// <summary>
        /// 80 mm CD-R with data only
        /// </summary>
        CDR_80 = 0x15,
        /// <summary>
        /// 80 mm CD-R with audio only
        /// </summary>
        CDR_DA_80 = 0x16,
        /// <summary>
        /// 80 mm CD-R with data and audio
        /// </summary>
        CDR_Mixed_80 = 0x17,
        /// <summary>
        /// 80 mm Hybrid CD-R (Photo CD)
        /// </summary>
        HybridCDR_80 = 0x18,
        /// <summary>
        /// Unknown size CD-RW
        /// </summary>
        Unknown_CDRW = 0x20,
        /// <summary>
        /// 120 mm CD-RW with data only
        /// </summary>
        CDRW = 0x21,
        /// <summary>
        /// 120 mm CD-RW with audio only
        /// </summary>
        CDRW_DA = 0x22,
        /// <summary>
        /// 120 mm CD-RW with data and audio
        /// </summary>
        CDRW_Mixed = 0x23,
        /// <summary>
        /// 120 mm Hybrid CD-RW (Photo CD)
        /// </summary>
        HybridCDRW = 0x24,
        /// <summary>
        /// 80 mm CD-RW with data only
        /// </summary>
        CDRW_80 = 0x25,
        /// <summary>
        /// 80 mm CD-RW with audio only
        /// </summary>
        CDRW_DA_80 = 0x26,
        /// <summary>
        /// 80 mm CD-RW with data and audio
        /// </summary>
        CDRW_Mixed_80 = 0x27,
        /// <summary>
        /// 80 mm Hybrid CD-RW (Photo CD)
        /// </summary>
        HybridCDRW_80 = 0x28,
        /// <summary>
        /// Unknown size HD disc
        /// </summary>
        Unknown_HD = 0x30,
        /// <summary>
        /// 120 mm HD disc
        /// </summary>
        HD = 0x31,
        /// <summary>
        /// 80 mm HD disc
        /// </summary>
        HD_80 = 0x35,
        /// <summary>
        /// No disc inserted, tray closed or caddy inserted
        /// </summary>
        NoDisc = 0x70,
        /// <summary>
        /// Tray open or no caddy inserted
        /// </summary>
        TrayOpen = 0x71,
        /// <summary>
        /// Tray closed or caddy inserted but medium error
        /// </summary>
        MediumError = 0x72,
        #endregion Medium Types defined in SFF-8020i

        #region Medium Types defined in USB Mass Storage Class - UFI Command Specification

        /// <summary>
        /// 3.5-inch, 135 tpi, 12362 bits/radian, double-sided MFM (aka 1.25Mb)
        /// </summary>
        Type3Floppy = 0x93,
        /// <summary>
        /// 3.5-inch, 135 tpi, 15916 bits/radian, double-sided MFM (aka 1.44Mb)
        /// </summary>
        HDFloppy = 0x94,
        #endregion Medium Types defined in USB Mass Storage Class - UFI Command Specification

        #region Medium Types defined in INF-8070

        /// <summary>
        /// Unknown type block device
        /// </summary>
        UnknownBlockDevice = 0x40,
        /// <summary>
        /// Read-only block device
        /// </summary>
        ReadOnlyBlockDevice = 0x41,
        /// <summary>
        /// Read/Write block device
        /// </summary>
        ReadWriteBlockDevice = 0x42

        #endregion Medium Types defined in INF-8070
    }

    public enum DensityType : byte
    {
        Default = 0x00,
        #region Density Types defined in ECMA-111 for Direct-Access devices

        /// <summary>
        /// 7958 flux transitions per radian
        /// </summary>
        Flux7958 = 0x01,
        /// <summary>
        /// 13262 flux transitions per radian
        /// </summary>
        Flux13262 = 0x02,
        /// <summary>
        /// 15916 flux transitions per radian
        /// </summary>
        Flux15916 = 0x03,
        #endregion Density Types defined in ECMA-111 for Direct-Access devices

        #region Density Types defined in ECMA-111 for Sequential-Access devices

        /// <summary>
        /// ECMA-62 &amp; ANSI X3.22-1983: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZI, 32 cpmm
        /// </summary>
        ECMA62 = 0x01,
        /// <summary>
        /// ECMA-62 &amp; ANSI X3.39-1986: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm
        /// </summary>
        ECMA62_Phase = 0x02,
        /// <summary>
        /// ECMA-62 &amp; ANSI X3.54-1986: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZI, 245 cpmm GCR
        /// </summary>
        ECMA62_GCR = 0x03,
        /// <summary>
        /// ECMA-79 &amp; ANSI X3.116-1986: 6,30 mm Magnetic Tape Cartridge using MFM Recording at 252 ftpmm
        /// </summary>
        ECMA79 = 0x07,
        /// <summary>
        /// Draft ECMA &amp; ANSI X3B5/87-099: 12,7 mm Magnetic Tape Cartridge using IFM Recording on 18 Tracks at 1944 ftpmm, GCR
        /// </summary>
        ECMADraft = 0x09,
        /// <summary>
        /// ECMA-46 &amp; ANSI X3.56-1986: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm
        /// </summary>
        ECMA46 = 0x0B,
        /// <summary>
        /// ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZI Recording, 394 ftpmm
        /// </summary>
        ECMA98 = 0x0E,
        #endregion Density Types defined in ECMA-111 for Sequential-Access devices

        #region Density Types defined in SCSI-2 for Sequential-Access devices

        /// <summary>
        /// ANXI X3.136-1986: 6,3 mm 4 or 9-Track Magnetic Tape Cartridge, 315 bpmm, GCR
        /// </summary>
        X3_136 = 0x05,
        /// <summary>
        /// ANXI X3.157-1987: 12,7 mm 9-Track Magnetic Tape, 126 bpmm, Phase Encoding
        /// </summary>
        X3_157 = 0x06,
        /// <summary>
        /// ANXI X3.158-1987: 3,81 mm 4-Track Magnetic Tape Cassette, 315 bpmm, GCR
        /// </summary>
        X3_158 = 0x08,
        /// <summary>
        /// ANXI X3B5/86-199: 12,7 mm 22-Track Magnetic Tape Cartridge, 262 bpmm, MFM
        /// </summary>
        X3B5_86 = 0x0A,
        /// <summary>
        /// HI-TC1: 12,7 mm 24-Track Magnetic Tape Cartridge, 500 bpmm, GCR
        /// </summary>
        HiTC1 = 0x0C,
        /// <summary>
        /// HI-TC2: 12,7 mm 24-Track Magnetic Tape Cartridge, 999 bpmm, GCR
        /// </summary>
        HiTC2 = 0x0D,
        /// <summary>
        /// QIC-120: 6,3 mm 15-Track Magnetic Tape Cartridge, 394 bpmm, GCR
        /// </summary>
        QIC120 = 0x0F,
        /// <summary>
        /// QIC-150: 6,3 mm 18-Track Magnetic Tape Cartridge, 394 bpmm, GCR
        /// </summary>
        QIC150 = 0x10,
        /// <summary>
        /// QIC-320: 6,3 mm 26-Track Magnetic Tape Cartridge, 630 bpmm, GCR
        /// </summary>
        QIC320 = 0x11,
        /// <summary>
        /// QIC-1350: 6,3 mm 30-Track Magnetic Tape Cartridge, 2034 bpmm, RLL
        /// </summary>
        QIC1350 = 0x12,
        /// <summary>
        /// ANXI X3B5/88-185A: 3,81 mm Magnetic Tape Cassette, 2400 bpmm, DDS
        /// </summary>
        X3B5_88 = 0x13,
        /// <summary>
        /// ANXI X3.202-1991: 8 mm Magnetic Tape Cassette, 1703 bpmm, RLL
        /// </summary>
        X3_202 = 0x14,
        /// <summary>
        /// ECMA TC17: 8 mm Magnetic Tape Cassette, 1789 bpmm, RLL
        /// </summary>
        ECMA_TC17 = 0x15,
        /// <summary>
        /// ANXI X3.193-1990: 12,7 mm 48-Track Magnetic Tape Cartridge, 394 bpmm, MFM
        /// </summary>
        X3_193 = 0x16,
        /// <summary>
        /// ANXI X3B5/97-174: 12,7 mm 48-Track Magnetic Tape Cartridge, 1673 bpmm, MFM
        /// </summary>
        X3B5_91 = 0x17,
        #endregion Density Types defined in SCSI-2 for Sequential-Access devices

        #region Density Types defined in SCSI-2 for MultiMedia devices

        /// <summary>
        /// User data only
        /// </summary>
        User = 0x01,
        /// <summary>
        /// User data plus auxiliary data field
        /// </summary>
        UserAuxiliary = 0x02,
        /// <summary>
        /// 4-byt tag field, user data plus auxiliary data
        /// </summary>
        UserAuxiliaryTag = 0x03,
        /// <summary>
        /// Audio information only
        /// </summary>
        Audio = 0x04,
        #endregion Density Types defined in SCSI-2 for MultiMedia devices

        #region Density Types defined in SCSI-2 for Optical devices

        /// <summary>
        /// ISO/IEC 10090: 86 mm Read/Write single-sided optical disc with 12500 tracks
        /// </summary>
        ISO10090 = 0x01,
        /// <summary>
        /// 89 mm Read/Write double-sided optical disc with 12500 tracks
        /// </summary>
        D581 = 0x02,
        /// <summary>
        /// ANSI X3.212: 130 mm Read/Write double-sided optical disc with 18750 tracks
        /// </summary>
        X3_212 = 0x03,
        /// <summary>
        /// ANSI X3.191: 130 mm Write-Once double-sided optical disc with 30000 tracks
        /// </summary>
        X3_191 = 0x04,
        /// <summary>
        /// ANSI X3.214: 130 mm Write-Once double-sided optical disc with 20000 tracks
        /// </summary>
        X3_214 = 0x05,
        /// <summary>
        /// ANSI X3.211: 130 mm Write-Once double-sided optical disc with 18750 tracks
        /// </summary>
        X3_211 = 0x06,
        /// <summary>
        /// 200 mm optical disc
        /// </summary>
        D407 = 0x07,
        /// <summary>
        /// ISO/IEC 13614: 300 mm double-sided optical disc
        /// </summary>
        ISO13614 = 0x08,
        /// <summary>
        /// ANSI X3.200: 356 mm double-sided optical disc with 56350 tracks
        /// </summary>
        X3_200 = 0x09

        #endregion Density Types defined in SCSI-2 for Optical devices
    }
}

