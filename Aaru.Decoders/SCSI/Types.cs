// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Types.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains various SCSI type values.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum MediumTypes : byte
{
    Default = 0x00,

#region Medium Types defined in ECMA-111 for Direct-Access devices

    /// <summary>ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side</summary>
    ECMA54 = 0x09,
    /// <summary>
    ///     ECMA-59 &amp; ANSI X3.121-1984: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad
    ///     on Both Sides
    /// </summary>
    ECMA59 = 0x0A,
    /// <summary>ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides</summary>
    ECMA69 = 0x0B,
    /// <summary>ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side</summary>
    ECMA66 = 0x0E,
    /// <summary>
    ///     ECMA-70 &amp; ANSI X3.125-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both
    ///     Sides; 1,9 Tracks per mm
    /// </summary>
    ECMA70 = 0x12,
    /// <summary>
    ///     ECMA-78 &amp; ANSI X3.126-1986: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both
    ///     Sides; 3,8 Tracks per mm
    /// </summary>
    ECMA78 = 0x16,
    /// <summary>
    ///     ECMA-99 &amp; ISO 8630-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides;
    ///     3,8 Tracks per mm
    /// </summary>
    ECMA99 = 0x1A,
    /// <summary>
    ///     ECMA-100 &amp; ANSI X3.137: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides;
    ///     5,3 Tracks per mm
    /// </summary>
    ECMA100 = 0x1E,

#endregion Medium Types defined in ECMA-111 for Direct-Access devices

#region Medium Types defined in SCSI-2 for Direct-Access devices

    /// <summary>Unspecified single sided flexible disk</summary>
    Unspecified_SS = 0x01,
    /// <summary>Unspecified double sided flexible disk</summary>
    Unspecified_DS = 0x02,
    /// <summary>ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 1 side</summary>
    X3_73 = 0x05,
    /// <summary>ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 2 sides</summary>
    X3_73_DS = 0x06,
    /// <summary>ANSI X3.80-1980: 130 mm, 3979 ftprad, 1,9 Tracks per mm, 1 side</summary>
    X3_82 = 0x0D,
    /// <summary>6,3 mm tape with 12 tracks at 394 ftpmm</summary>
    Tape12 = 0x40,
    /// <summary>6,3 mm tape with 24 tracks at 394 ftpmm</summary>
    Tape24 = 0x44,

#endregion Medium Types defined in SCSI-2 for Direct-Access devices

#region Medium Types defined in SCSI-3 SBC-1 for Optical devices

    /// <summary>Read-only medium</summary>
    ReadOnly = 0x01,
    /// <summary>Write-once Read-many medium</summary>
    WORM = 0x02,
    /// <summary>Erasable medium</summary>
    Erasable = 0x03,
    /// <summary>Combination of read-only and write-once medium</summary>
    RO_WORM = 0x04,
    /// <summary>Combination of read-only and erasable medium</summary>
    RO_RW = 0x05,
    /// <summary>Combination of write-once and erasable medium</summary>
    WORM_RW = 0x06,

#endregion Medium Types defined in SCSI-3 SBC-1 for Optical devices

#region Medium Types defined in SCSI-2 for MultiMedia devices

    /// <summary>120 mm CD-ROM</summary>
    CDROM = 0x01,
    /// <summary>120 mm Compact Disc Digital Audio</summary>
    CDDA = 0x02,
    /// <summary>120 mm Compact Disc with data and audio</summary>
    MixedCD = 0x03,
    /// <summary>80 mm CD-ROM</summary>
    CDROM_80 = 0x05,
    /// <summary>80 mm Compact Disc Digital Audio</summary>
    CDDA_80 = 0x06,
    /// <summary>80 mm Compact Disc with data and audio</summary>
    MixedCD_80 = 0x07,

#endregion Medium Types defined in SCSI-2 for MultiMedia devices

#region Medium Types defined in SFF-8020i

    /// <summary>Unknown medium type</summary>
    Unknown_CD = 0x00,
    /// <summary>120 mm Hybrid disc (Photo CD)</summary>
    HybridCD = 0x04,
    /// <summary>Unknown size CD-R</summary>
    Unknown_CDR = 0x10,
    /// <summary>120 mm CD-R with data only</summary>
    CDR = 0x11,
    /// <summary>120 mm CD-R with audio only</summary>
    CDR_DA = 0x12,
    /// <summary>120 mm CD-R with data and audio</summary>
    CDR_Mixed = 0x13,
    /// <summary>120 mm Hybrid CD-R (Photo CD)</summary>
    HybridCDR = 0x14,
    /// <summary>80 mm CD-R with data only</summary>
    CDR_80 = 0x15,
    /// <summary>80 mm CD-R with audio only</summary>
    CDR_DA_80 = 0x16,
    /// <summary>80 mm CD-R with data and audio</summary>
    CDR_Mixed_80 = 0x17,
    /// <summary>80 mm Hybrid CD-R (Photo CD)</summary>
    HybridCDR_80 = 0x18,
    /// <summary>Unknown size CD-RW</summary>
    Unknown_CDRW = 0x20,
    /// <summary>120 mm CD-RW with data only</summary>
    CDRW = 0x21,
    /// <summary>120 mm CD-RW with audio only</summary>
    CDRW_DA = 0x22,
    /// <summary>120 mm CD-RW with data and audio</summary>
    CDRW_Mixed = 0x23,
    /// <summary>120 mm Hybrid CD-RW (Photo CD)</summary>
    HybridCDRW = 0x24,
    /// <summary>80 mm CD-RW with data only</summary>
    CDRW_80 = 0x25,
    /// <summary>80 mm CD-RW with audio only</summary>
    CDRW_DA_80 = 0x26,
    /// <summary>80 mm CD-RW with data and audio</summary>
    CDRW_Mixed_80 = 0x27,
    /// <summary>80 mm Hybrid CD-RW (Photo CD)</summary>
    HybridCDRW_80 = 0x28,
    /// <summary>Unknown size HD disc</summary>
    Unknown_HD = 0x30,
    /// <summary>120 mm HD disc</summary>
    HD = 0x31,
    /// <summary>80 mm HD disc</summary>
    HD_80 = 0x35,
    /// <summary>No disc inserted, tray closed or caddy inserted</summary>
    NoDisc = 0x70,
    /// <summary>Tray open or no caddy inserted</summary>
    TrayOpen = 0x71,
    /// <summary>Tray closed or caddy inserted but medium error</summary>
    MediumError = 0x72,

#endregion Medium Types defined in SFF-8020i

#region Medium Types defined in USB Mass Storage Class - UFI Command Specification

    /// <summary>3.5-inch, 135 tpi, 12362 bits/radian, double-sided MFM (aka 1.25Mb)</summary>
    Type3Floppy = 0x93,
    /// <summary>3.5-inch, 135 tpi, 15916 bits/radian, double-sided MFM (aka 1.44Mb)</summary>
    HDFloppy = 0x94,

#endregion Medium Types defined in USB Mass Storage Class - UFI Command Specification

#region Medium Types defined in INF-8070

    /// <summary>Unknown type block device</summary>
    UnknownBlockDevice = 0x40,
    /// <summary>Read-only block device</summary>
    ReadOnlyBlockDevice = 0x41,
    /// <summary>Read/Write block device</summary>
    ReadWriteBlockDevice = 0x42,

#endregion Medium Types defined in INF-8070

#region Medium Types found in vendor documents

    /// <summary>LTO WORM as reported by HP drives</summary>
    LTOWORM = 0x01,
    /// <summary>LTO cleaning cartridge as reported by Certance/Seagate drives</summary>
    LTOCleaning = 0x01,
    /// <summary>Direct-overwrite magneto-optical</summary>
    DOW = 0x07,
    /// <summary>LTO Ultrium</summary>
    LTO = 0x18,
    /// <summary>LTO Ultrium-2</summary>
    LTO2 = 0x28,
    /// <summary>DC-2900SL</summary>
    DC2900SL = 0x31,
    /// <summary>MLR-1</summary>
    MLR1 = 0x33,
    /// <summary>DDS-3</summary>
    DDS3 = 0x33,
    /// <summary>DC-9200</summary>
    DC9200 = 0x34,
    /// <summary>DDS-4</summary>
    DDS4 = 0x34,
    /// <summary>DAT-72</summary>
    DAT72 = 0x35,
    /// <summary>LTO Ultrium-3</summary>
    LTO3 = 0x38,
    /// <summary>LTO Ultrium-3 WORM</summary>
    LTO3WORM = 0x3C,
    /// <summary>DDS cleaning cartridge</summary>
    DDSCleaning = 0x3F,
    /// <summary>DC-9250</summary>
    DC9250 = 0x40,
    /// <summary>SLR-32</summary>
    SLR32 = 0x43,
    /// <summary>MLR-1SL</summary>
    MLR1SL = 0x44,
    /// <summary>SLRtape-50</summary>
    SLRtape50 = 0x47,
    /// <summary>LTO Ultrium-4</summary>
    LTO4 = 0x48,
    /// <summary>LTO Ultrium-4 WORM</summary>
    LTO4WORM = 0x4C,
    /// <summary>SLRtape-50SL</summary>
    SLRtape50SL = 0x50,
    /// <summary>SLR-32SL</summary>
    SLR32SL = 0x54,
    /// <summary>SLR-5</summary>
    SLR5 = 0x55,
    /// <summary>SLR-5SL</summary>
    SLR5SL = 0x56,
    /// <summary>LTO Ultrium-5</summary>
    LTO5 = 0x58,
    /// <summary>LTO Ultrium-5 WORM</summary>
    LTO5WORM = 0x5C,
    /// <summary>SLRtape-7</summary>
    SLRtape7 = 0x63,
    /// <summary>SLRtape-7SL</summary>
    SLRtape7SL = 0x64,
    /// <summary>SLRtape-24</summary>
    SLRtape24 = 0x65,
    /// <summary>SLRtape-24SL</summary>
    SLRtape24SL = 0x66,
    /// <summary>LTO Ultrium-6</summary>
    LTO6 = 0x68,
    /// <summary>LTO Ultrium-6 WORM</summary>
    LTO6WORM = 0x6C,
    /// <summary>SLRtape-140</summary>
    SLRtape140 = 0x70,
    /// <summary>SLRtape-40</summary>
    SLRtape40 = 0x73,
    /// <summary>SLRtape-60</summary>
    SLRtape60 = 0x74,
    /// <summary>SLRtape-75</summary>
    SLRtape75 = 0x74,
    /// <summary>SLRtape-100</summary>
    SLRtape100 = 0x75,
    /// <summary>SLR-40 or SLR-60 or SLR-100</summary>
    SLR40_60_100 = 0x76,
    /// <summary>LTO Ultrium-7</summary>
    LTO7 = 0x78,
    /// <summary>LTO Ultrium-7 WORM</summary>
    LTO7WORM = 0x7C,
    /// <summary>HP LTO emulating a CD</summary>
    LTOCD = 0x80,
    /// <summary>Exatape 15m</summary>
    Exatape15m = 0x81,
    /// <summary>IBM MagStar</summary>
    MagStar = 0x81,
    /// <summary>VXA</summary>
    VXA = 0x81,
    /// <summary>CompactTape I</summary>
    CT1 = 0x82,
    /// <summary>Exatape 28m</summary>
    Exatape28m = 0x82,
    /// <summary>CompactTape II</summary>
    CT2 = 0x82,
    /// <summary>VXA-2</summary>
    VXA2 = 0x82,
    /// <summary>VXA-3</summary>
    VXA3 = 0x82,
    /// <summary>Exatape 54m</summary>
    Exatape54m = 0x83,
    /// <summary>DLTtape III</summary>
    DLTtapeIII = 0x83,
    /// <summary>Exatape 80m</summary>
    Exatape80m = 0x84,
    /// <summary>DLTtape IIIxt</summary>
    DLTtapeIIIxt = 0x84,
    /// <summary>Exatape 106m</summary>
    Exatape106m = 0x85,
    /// <summary>DLTtape IV</summary>
    DLTtapeIV = 0x85,
    /// <summary>Travan 5</summary>
    Travan5 = 0x85,
    /// <summary>Exatape 106m XL</summary>
    Exatape106mXL = 0x86,
    /// <summary>SuperDLT I</summary>
    SDLT1 = 0x86,
    /// <summary>SuperDLT II</summary>
    SDLT2 = 0x87,
    /// <summary>VStape I</summary>
    VStapeI = 0x90,
    /// <summary>DLTtape S4</summary>
    DLTtapeS4 = 0x91,
    /// <summary>Travan 7</summary>
    Travan7 = 0x95,
    /// <summary>Exatape 22m</summary>
    Exatape22m = 0xC1,
    /// <summary>Exatape 40m</summary>
    Exatape40m = 0xC2,
    /// <summary>Exatape 76m</summary>
    Exatape76m = 0xC3,
    /// <summary>Exatape 112m</summary>
    Exatape112m = 0xC4,
    /// <summary>Exatape 22m AME</summary>
    Exatape22mAME = 0xD1,
    /// <summary>Exatape 170m</summary>
    Exatape170m = 0xD2,
    /// <summary>Exatape 125m</summary>
    Exatape125m = 0xD3,
    /// <summary>Exatape 45m</summary>
    Exatape45m = 0xD4,
    /// <summary>Exatape 225m</summary>
    Exatape225m = 0xD5,
    /// <summary>Exatape 150m</summary>
    Exatape150m = 0xD6,
    /// <summary>Exatape 75m</summary>
    Exatape75m = 0xD7,

#endregion Medium Types found in vendor documents

#region Medium Types found testing a Hi-MD drive

    /// <summary>Hi-MD</summary>
    HiMD = 0x44,

#endregion Medium Types found testing a Hi-MD drive
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum DensityType : byte
{
    Default = 0x00,

#region Density Types defined in ECMA-111 for Direct-Access devices

    /// <summary>7958 flux transitions per radian</summary>
    Flux7958 = 0x01,
    /// <summary>13262 flux transitions per radian</summary>
    Flux13262 = 0x02,
    /// <summary>15916 flux transitions per radian</summary>
    Flux15916 = 0x03,

#endregion Density Types defined in ECMA-111 for Direct-Access devices

#region Density Types defined in ECMA-111 for Sequential-Access devices

    /// <summary>ECMA-62 &amp; ANSI X3.22-1983: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZI, 32 cpmm</summary>
    ECMA62 = 0x01,
    /// <summary>ECMA-62 &amp; ANSI X3.39-1986: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm</summary>
    ECMA62_Phase = 0x02,
    /// <summary>ECMA-62 &amp; ANSI X3.54-1986: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZI, 245 cpmm GCR</summary>
    ECMA62_GCR = 0x03,
    /// <summary>ECMA-79 &amp; ANSI X3.116-1986: 6,30 mm Magnetic Tape Cartridge using MFM Recording at 252 ftpmm</summary>
    ECMA79 = 0x07,
    /// <summary>
    ///     Draft ECMA &amp; ANSI X3B5/87-099: 12,7 mm Magnetic Tape Cartridge using IFM Recording on 18 Tracks at 1944
    ///     ftpmm, GCR (IBM 3480, 3490, 3490E)
    /// </summary>
    IBM3480 = 0x09,
    /// <summary>ECMA-46 &amp; ANSI X3.56-1986: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm</summary>
    ECMA46 = 0x0B,
    /// <summary>ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZI Recording, 394 ftpmm</summary>
    ECMA98 = 0x0E,

#endregion Density Types defined in ECMA-111 for Sequential-Access devices

#region Density Types defined in SCSI-2 for Sequential-Access devices

    /// <summary>ANXI X3.136-1986: 6,35 mm 4 or 9-Track Magnetic Tape Cartridge, 315 bpmm, GCR (QIC-24)</summary>
    X3_136 = 0x05,
    /// <summary>ANXI X3.157-1987: 12,7 mm 9-Track Magnetic Tape, 126 bpmm, Phase Encoding</summary>
    X3_157 = 0x06,
    /// <summary>ANXI X3.158-1987: 3,81 mm 4-Track Magnetic Tape Cassette, 315 bpmm, GCR</summary>
    X3_158 = 0x08,
    /// <summary>ANXI X3B5/86-199: 12,7 mm 22-Track Magnetic Tape Cartridge, 262 bpmm, MFM</summary>
    X3B5_86 = 0x0A,
    /// <summary>HI-TC1: 12,7 mm 24-Track Magnetic Tape Cartridge, 500 bpmm, GCR</summary>
    HiTC1 = 0x0C,
    /// <summary>HI-TC2: 12,7 mm 24-Track Magnetic Tape Cartridge, 999 bpmm, GCR</summary>
    HiTC2 = 0x0D,
    /// <summary>QIC-120: 6,3 mm 15-Track Magnetic Tape Cartridge, 394 bpmm, GCR</summary>
    QIC120 = 0x0F,
    /// <summary>QIC-150: 6,3 mm 18-Track Magnetic Tape Cartridge, 394 bpmm, GCR</summary>
    QIC150 = 0x10,
    /// <summary>QIC-320: 6,3 mm 26-Track Magnetic Tape Cartridge, 630 bpmm, GCR</summary>
    QIC320 = 0x11,
    /// <summary>QIC-1350: 6,3 mm 30-Track Magnetic Tape Cartridge, 2034 bpmm, RLL</summary>
    QIC1350 = 0x12,
    /// <summary>ANXI X3B5/88-185A: 3,81 mm Magnetic Tape Cassette, 2400 bpmm, DDS</summary>
    X3B5_88 = 0x13,
    /// <summary>ANXI X3.202-1991: 8 mm Magnetic Tape Cassette, 1703 bpmm, RLL</summary>
    X3_202 = 0x14,
    /// <summary>ECMA TC17: 8 mm Magnetic Tape Cassette, 1789 bpmm, RLL</summary>
    ECMA_TC17 = 0x15,
    /// <summary>ANXI X3.193-1990: 12,7 mm 48-Track Magnetic Tape Cartridge, 394 bpmm, MFM</summary>
    X3_193 = 0x16,
    /// <summary>ANXI X3B5/97-174: 12,7 mm 48-Track Magnetic Tape Cartridge, 1673 bpmm, MFM</summary>
    X3B5_91 = 0x17,

#endregion Density Types defined in SCSI-2 for Sequential-Access devices

#region Density Types defined in SCSI-2 for MultiMedia devices

    /// <summary>User data only</summary>
    User = 0x01,
    /// <summary>User data plus auxiliary data field</summary>
    UserAuxiliary = 0x02,
    /// <summary>4-byt tag field, user data plus auxiliary data</summary>
    UserAuxiliaryTag = 0x03,
    /// <summary>Audio information only</summary>
    Audio = 0x04,

#endregion Density Types defined in SCSI-2 for MultiMedia devices

#region Density Types defined in SCSI-2 for Optical devices

    /// <summary>ISO/IEC 10090: 86 mm Read/Write single-sided optical disc with 12500 tracks</summary>
    ISO10090 = 0x01,
    /// <summary>89 mm Read/Write double-sided optical disc with 12500 tracks</summary>
    D581 = 0x02,
    /// <summary>ANSI X3.212: 130 mm Read/Write double-sided optical disc with 18750 tracks</summary>
    X3_212 = 0x03,
    /// <summary>ANSI X3.191: 130 mm Write-Once double-sided optical disc with 30000 tracks</summary>
    X3_191 = 0x04,
    /// <summary>ANSI X3.214: 130 mm Write-Once double-sided optical disc with 20000 tracks</summary>
    X3_214 = 0x05,
    /// <summary>ANSI X3.211: 130 mm Write-Once double-sided optical disc with 18750 tracks</summary>
    X3_211 = 0x06,
    /// <summary>200 mm optical disc</summary>
    D407 = 0x07,
    /// <summary>ISO/IEC 13614: 300 mm double-sided optical disc</summary>
    ISO13614 = 0x08,
    /// <summary>ANSI X3.200: 356 mm double-sided optical disc with 56350 tracks</summary>
    X3_200 = 0x09,

#endregion Density Types defined in SCSI-2 for Optical devices

#region Density Types found in vendor documents

    /// <summary>QIC-11</summary>
    QIC11 = 0x04,
    /// <summary>CompactTape I</summary>
    CT1 = 0x0A,
    /// <summary>Exabyte 8200 format</summary>
    Ex8200 = 0x14,
    /// <summary>Exabyte 8500 format</summary>
    Ex8500 = 0x15,
    /// <summary>CompactTape II</summary>
    CT2 = 0x16,
    /// <summary>DLTtape III 42500 bpi</summary>
    DLT3_42k = 0x17,
    /// <summary>DLTtape III 56 track</summary>
    DLT3_56t = 0x18,
    /// <summary>DLTtape III 62500 bpi</summary>
    DLT3_62k = 0x19,
    /// <summary>DLTtape IV</summary>
    DLT4 = 0x1A,
    /// <summary>DLTtape IV 85937 bpi</summary>
    DLT4_85k = 0x1B,
    /// <summary>DDS-2</summary>
    DDS2 = 0x24,
    /// <summary>DDS-3</summary>
    DDS3 = 0x25,
    /// <summary>DDS-4</summary>
    DDS4 = 0x26,
    /// <summary>Exabyte Mammoth</summary>
    Mammoth = 0x27,
    /// <summary>IBM 3490 &amp; 3490E</summary>
    IBM3490E = 0x28,
    /// <summary>Exabyte Mammoth-2</summary>
    Mammoth2 = 0x28,
    /// <summary>IBM 3590</summary>
    IBM3590 = 0x29,
    /// <summary>IBM 3590E</summary>
    IBM3590E = 0x2A,
    /// <summary>AIT-1</summary>
    AIT1 = 0x30,
    /// <summary>AIT-2</summary>
    AIT2 = 0x31,
    /// <summary>AIT-3</summary>
    AIT3 = 0x32,
    /// <summary>DLTtape IV 123090 bpi</summary>
    DLT4_123k = 0x40,
    /// <summary>Ultrium-1</summary>
    LTO1 = 0x40,
    /// <summary>Super AIT-1</summary>
    SAIT1 = 0x40,
    /// <summary>DLTtape IV 85937 bpi</summary>
    DLT4_98k = 0x41,
    /// <summary>Ultrium-2 as reported by the Certance drive</summary>
    LTO2Old = 0x41,
    /// <summary>Ultrium-2</summary>
    LTO2 = 0x42,
    /// <summary>T9840</summary>
    T9840 = 0x42,
    /// <summary>T9940</summary>
    T9940 = 0x43,
    /// <summary>Ultrium-</summary>
    LTO3 = 0x44,
    /// <summary>T9840C</summary>
    T9840C = 0x45,
    /// <summary>Travan 5</summary>
    Travan5 = 0x46,
    /// <summary>Ultrium-4</summary>
    LTO4 = 0x46,
    /// <summary>T9840D</summary>
    T9840D = 0x46,
    /// <summary>DAT-72</summary>
    DAT72 = 0x47,
    /// <summary>Super DLTtape I 133000 bpi</summary>
    SDLT1_133k = 0x48,
    /// <summary>Super DLTtape I</summary>
    SDLT1 = 0x49,
    /// <summary>T10000A</summary>
    T10000A = 0x4A,
    /// <summary>Super DLTtape II</summary>
    SDLT2 = 0x4A,
    /// <summary>DLTtape S4</summary>
    DLTS4 = 0x4B,
    /// <summary>T10000B</summary>
    T10000B = 0x4B,
    /// <summary>T10000C</summary>
    T10000C = 0x4C,
    /// <summary>T10000D</summary>
    T10000D = 0x4D,
    /// <summary>VStape I</summary>
    VStape1 = 0x40,
    /// <summary>Ultrium-5</summary>
    LTO5 = 0x58,
    /// <summary>Ultrium-6</summary>
    LTO6 = 0x5A,
    /// <summary>Ultrium-7</summary>
    LTO7 = 0x5C,
    /// <summary>DLTtape III 62500 bpi secondary code</summary>
    DLT3_62kAlt = 0x80,
    /// <summary>VXA-1</summary>
    VXA1 = 0x80,
    /// <summary>DLTtape III compressed</summary>
    DLT3c = 0x81,
    /// <summary>VXA-2</summary>
    VXA2 = 0x81,
    /// <summary>DLTtape IV secondary code</summary>
    DLT4Alt = 0x82,
    /// <summary>VXA-3</summary>
    VXA3 = 0x82,
    /// <summary>DLTtape IV compressed</summary>
    DLT4c = 0x83,
    /// <summary>DLTtape IV 85937 bpi secondary code</summary>
    DLT4_85kAlt = 0x84,
    /// <summary>DLTtape IV 85937 bpi compressed</summary>
    DLT4c_85k = 0x85,
    /// <summary>DLTtape IV 123090 bpi secondary code</summary>
    DLT4_123kAlt = 0x86,
    /// <summary>DLTtape IV 123090 bpi compressed</summary>
    DLT4c_123k = 0x87,
    /// <summary>DLTtape IV 98250 bpi secondary code</summary>
    DLT4_98kAlt = 0x88,
    /// <summary>DLTtape IV 98250 bpi compressed</summary>
    DLT4c_98k = 0x89,
    /// <summary>Exabyte compressed 8200 format</summary>
    Ex8500c = 0x8C,
    /// <summary>Exabyte compressed 8500 format</summary>
    Ex8200c = 0x90,
    /// <summary>Super DLTtape I secondary code</summary>
    SDLT1Alt = 0x90,
    /// <summary>Super DLTtape I compressed</summary>
    SDLT1c = 0x91,
    /// <summary>Super DLTtape I 133000 bpi secondary code</summary>
    SDLT1_133kAlt = 0x92,
    /// <summary>Super DLTtape I 133000 bpi compressed</summary>
    SDLT1c_133k = 0x93,
    /// <summary>VStape I secondary code</summary>
    VStape1Alt = 0x98,
    /// <summary>VStape I compressed</summary>
    VStape1c = 0x99,

#endregion Density Types found in vendor documents
}