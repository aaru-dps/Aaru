/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : SCSI.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes SCSI structures.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders
{
    /// <summary>
    /// Information from the following standards:
    /// T9/375-D revision 10l
    /// T10/995-D revision 10
    /// T10/1236-D revision 20
    /// T10/1416-D revision 23
    /// </summary>
    public static class SCSI
    {
        #region Enumerations

        enum SCSIPeripheralQualifiers : byte
        {
            /// <summary>
            /// Peripheral qualifier: Device is connected and supported
            /// </summary>
            SCSIPQSupported = 0x00,
            /// <summary>
            /// Peripheral qualifier: Device is supported but not connected
            /// </summary>
            SCSIPQUnconnected = 0x01,
            /// <summary>
            /// Peripheral qualifier: Reserved value
            /// </summary>
            SCSIPQReserved = 0x02,
            /// <summary>
            /// Peripheral qualifier: Device is connected but unsupported
            /// </summary>
            SCSIPQUnsupported = 0x03,
            /// <summary>
            /// Peripheral qualifier: Vendor values: 0x04, 0x05, 0x06 and 0x07
            /// </summary>
            SCSIPQVendorMask = 0x04
        }

        public enum SCSIPeripheralDeviceTypes : byte
        {
            /// <summary>
            /// Direct-access device
            /// </summary>
            SCSIPDTDirectAccess = 0x00,
            /// <summary>
            /// Sequential-access device
            /// </summary>
            SCSIPDTSequentialAccess = 0x01,
            /// <summary>
            /// Printer device
            /// </summary>
            SCSIPDTPrinterDevice = 0x02,
            /// <summary>
            /// Processor device
            /// </summary>
            SCSIPDTProcessorDevice = 0x03,
            /// <summary>
            /// Write-once device
            /// </summary>
            SCSIPDTWriteOnceDevice = 0x04,
            /// <summary>
            /// CD-ROM/DVD/etc device
            /// </summary>
            SCSIPDTMultiMediaDevice = 0x05,
            /// <summary>
            /// Scanner device
            /// </summary>
            SCSIPDTScannerDevice = 0x06,
            /// <summary>
            /// Optical memory device
            /// </summary>
            SCSIPDTOpticalDevice = 0x07,
            /// <summary>
            /// Medium change device
            /// </summary>
            SCSIPDTMediumChangerDevice = 0x08,
            /// <summary>
            /// Communications device
            /// </summary>
            SCSIPDTCommsDevice = 0x09,
            /// <summary>
            /// Graphics arts pre-press device (defined in ASC IT8)
            /// </summary>
            SCSIPDTPrePressDevice1 = 0x0A,
            /// <summary>
            /// Graphics arts pre-press device (defined in ASC IT8)
            /// </summary>
            SCSIPDTPrePressDevice2 = 0x0B,
            /// <summary>
            /// Array controller device
            /// </summary>
            SCSIPDTArrayControllerDevice = 0x0C,
            /// <summary>
            /// Enclosure services device
            /// </summary>
            SCSIPDTEnclosureServiceDevice = 0x0D,
            /// <summary>
            /// Simplified direct-access device
            /// </summary>
            SCSIPDTSimplifiedDevice = 0x0E,
            /// <summary>
            /// Optical card reader/writer device
            /// </summary>
            SCSIPDTOCRWDevice = 0x0F,
            /// <summary>
            /// Bridging Expanders
            /// </summary>
            SCSIPDTBridgingExpander = 0x10,
            /// <summary>
            /// Object-based Storage Device
            /// </summary>
            SCSIPDTObjectDevice = 0x11,
            /// <summary>
            /// Automation/Drive Interface
            /// </summary>
            SCSIPDTADCDevice = 0x12,
            /// <summary>
            /// Well known logical unit
            /// </summary>
            SCSIPDTWellKnownDevice = 0x1E,
            /// <summary>
            /// Unknown or no device type
            /// </summary>
            SCSIPDTUnknownDevice = 0x1F
        }

        enum SCSIANSIVersions : byte
        {
            /// <summary>
            /// Device does not claim conformance to any ANSI version
            /// </summary>
            SCSIANSINoVersion = 0x00,
            /// <summary>
            /// Device complies with ANSI X3.131:1986
            /// </summary>
            SCSIANSI1986Version = 0x01,
            /// <summary>
            /// Device complies with ANSI X3.131:1994
            /// </summary>
            SCSIANSI1994Version = 0x02,
            /// <summary>
            /// Device complies with ANSI X3.301:1997
            /// </summary>
            SCSIANSI1997Version = 0x03,
            /// <summary>
            /// Device complies with ANSI X3.351:2001
            /// </summary>
            SCSIANSI2001Version = 0x04,
            /// <summary>
            /// Device complies with SPC3, T10, 2005.
            /// </summary>
            SCSIANSI2005Version = 0x05
        }

        enum SCSIECMAVersions : byte
        {
            /// <summary>
            /// Device does not claim conformance to any ECMA version
            /// </summary>
            SCSIECMANoVersion = 0x00,
            /// <summary>
            /// Device complies with an obsolete ECMA standard
            /// </summary>
            SCSIECMAObsolete = 0x01
        }

        enum SCSIISOVersions : byte
        {
            /// <summary>
            /// Device does not claim conformance to any ISO/IEC version
            /// </summary>
            SCSIISONoVersion = 0x00,
            /// <summary>
            /// Device complies with ISO/IEC 9316:1995
            /// </summary>
            SCSIISO1995Version = 0x02
        }

        enum SCSISPIClocking : byte
        {
            /// <summary>
            /// Supports only ST
            /// </summary>
            SCSIClockingST = 0x00,
            /// <summary>
            /// Supports only DT
            /// </summary>
            SCSIClockingDT = 0x01,
            /// <summary>
            /// Reserved value
            /// </summary>
            SCSIClockingReserved = 0x02,
            /// <summary>
            /// Supports ST and DT
            /// </summary>
            SCSIClockingSTandDT = 0x03,
        }

        enum SCSIVersionDescriptorStandardMask : ushort
        {
            NoStandard = 0x0000,
            SAM = 0x0020,
            SAM2 = 0x0040,
            SAM3 = 0x0060,
            SAM4 = 0x0080,
            SPC = 0x0120,
            MMC = 0x0140,
            SCC = 0x0160,
            SBC = 0x0180,
            SMC = 0x01A0,
            SES = 0x01C0,
            SCC2 = 0x01E0,
            SSC = 0x0200,
            RBC = 0x0220,
            MMC2 = 0x0240,
            SPC2 = 0x0260,
            OCRW = 0x0280,
            MMC3 = 0x02A0,
            RMC = 0x02C0,
            SMC2 = 0x02E0,
            SPC3 = 0x0300,
            SBC2 = 0x0320,
            OSD = 0x0340,
            SSC2 = 0x0360,
            BCC = 0x0380,
            MMC4 = 0x03A0,
            ADC = 0x03C0,
            SES2 = 0x03E0,
            SSC3 = 0x0400,
            MMC5 = 0x0420,
            OSD2 = 0x0440,
            SPC4 = 0x0460,
            SMC3 = 0x0480,
            ADC2 = 0x04A0,
            SSA_TL2 = 0x0820,
            SSA_TL1 = 0x0840,
            SSA_S3P = 0x0860,
            SSA_S2P = 0x0880,
            SIP = 0x08A0,
            FCP = 0x08C0,
            SBP2 = 0x08E0,
            FCP2 = 0x0900,
            SST = 0x0920,
            SRP = 0x0940,
            iSCSI = 0x0960,
            SBP3 = 0x0980,
            ADP = 0x09C0,
            ADT = 0x09E0,
            FCP3 = 0x0A00,
            ADT2 = 0x0A20,
            SPI = 0x0AA0,
            Fast20 = 0xAC0,
            SPI2 = 0x0AE0,
            SPI3 = 0x0B00,
            EPI = 0x0B20,
            SPI4 = 0x0B40,
            SPI5 = 0x0B60,
            SAS = 0x0BE0,
            SAS11 = 0x0C00,
            FC_PH = 0x0D20,
            FC_AL = 0x0D40,
            FC_AL2 = 0x0D60,
            FC_PH3 = 0x0D80,
            FC_FS = 0x0DA0,
            FC_PI = 0x0DC0,
            FC_PI2 = 0x0DE0,
            FC_FS2 = 0x0E00,
            FC_LS = 0x0E20,
            FC_SP = 0x0E40,
            FC_DA = 0x12E0,
            FC_Tape = 0x1300,
            FC_FLA = 0x1320,
            FC_PLDA = 0x1340,
            SSA_PH2 = 0x1360,
            SSA_PH3 = 0x1380,
            IEEE1394 = 0x14A0,
            IEEE1394a = 0x14C0,
            IEEE1394b = 0x14E0,
            ATA_ATAPI6 = 0x15E0,
            ATA_ATAPI7 = 0x1600,
            ATA_ATAPI8 = 0x1620,
            USB = 0x1720,
            SAT = 0x1EA0
        }

        enum SCSIVersionDescriptorSAMMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0994 revision 18
            /// </summary>
            T10_0994_r18 = 0x001B,
            /// <summary>
            /// ANSI X3.270:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorSAM2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1157-D revision 23
            /// </summary>
            T10_1157_r23 = 0x0014,
            /// <summary>
            /// T10/1157-D revision 24
            /// </summary>
            T10_1157_r24 = 0x0015,
            /// <summary>
            /// ANSI INCITS 366-2003
            /// </summary>
            ANSI_2003 = 0x001C
        }

        enum SCSIVersionDescriptorSAM3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1561-D revision 7
            /// </summary>
            T10_1561_r07 = 0x0002,
            /// <summary>
            /// T10/1561-D revision 13
            /// </summary>
            T10_1561_r13 = 0x0015,
            /// <summary>
            /// T10/1157-D revision 14
            /// </summary>
            T10_1561_r14 = 0x0016,
            /// <summary>
            /// ANSI INCITS 402-2005
            /// </summary>
            ANSI_2005 = 0x0017
        }

        enum SCSIVersionDescriptorSPCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0995 revision 11a
            /// </summary>
            T10_0995_r11a = 0x001B,
            /// <summary>
            /// ANSI X3.301:1997
            /// </summary>
            ANSI_1997 = 0x001C
        }

        enum SCSIVersionDescriptorMMCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1048 revision 10a
            /// </summary>
            T10_1048_r10a = 0x001B,
            /// <summary>
            /// ANSI X3.304:1997
            /// </summary>
            ANSI_1997 = 0x001C
        }

        enum SCSIVersionDescriptorSCCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1047 revision 06c
            /// </summary>
            T10_1048_r06c = 0x001B,
            /// <summary>
            /// ANSI X3.276:1997
            /// </summary>
            ANSI_1997 = 0x001C
        }

        enum SCSIVersionDescriptorSBCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0996 revision 08c
            /// </summary>
            T10_0996_r08c = 0x001B,
            /// <summary>
            /// ANSI NCITS.306:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorSMCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0999 revision 10a
            /// </summary>
            T10_0999_r10a = 0x001B,
            /// <summary>
            /// ANSI NCITS.314:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorSESMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1212 revision 08b
            /// </summary>
            T10_1212_r08b = 0x001B,
            /// <summary>
            /// ANSI NCITS.305:1998
            /// </summary>
            ANSI_1998 = 0x001C,
            /// <summary>
            /// T10/1212 revision 08b with ANSI NCITS.305/AM1:2000
            /// </summary>
            T10_1212_r08b_2000 = 0x001D,
            /// <summary>
            /// ANSI NCITS.305:1998 with ANSI NCITS.305/AM1:2000
            /// </summary>
            ANSI_1998_2000 = 0x001E
        }

        enum SCSIVersionDescriptorSCC2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1125 revision 04
            /// </summary>
            T10_1125_r04 = 0x001B,
            /// <summary>
            /// ANSI NCITS.318:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorSSCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0997 revision 17
            /// </summary>
            T10_0997_r17 = 0x0001,
            /// <summary>
            /// T10/0997 revision 22
            /// </summary>
            T10_0997_r22 = 0x0007,
            /// <summary>
            /// ANSI NCITS.335:2000
            /// </summary>
            ANSI_2000 = 0x001C
        }

        enum SCSIVersionDescriptorRBCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1240 revision 10a
            /// </summary>
            T10_1240_r10a = 0x0018,
            /// <summary>
            /// ANSI NCITS.330:2000
            /// </summary>
            ANSI_2000 = 0x001C
        }

        enum SCSIVersionDescriptorMMC2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1228 revision 11
            /// </summary>
            T10_1228_r11 = 0x0015,
            /// <summary>
            /// T10/1228 revision 11a
            /// </summary>
            T10_1228_r11a = 0x001B,
            /// <summary>
            /// ANSI NCITS.333:2000
            /// </summary>
            ANSI_2000 = 0x001C
        }

        enum SCSIVersionDescriptorSPC2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1236 revision 12
            /// </summary>
            T10_1236_r12 = 0x0007,
            /// <summary>
            /// T10/1236 revision 18
            /// </summary>
            T10_1236_r18 = 0x0009,
            /// <summary>
            /// T10/1236 revision 19
            /// </summary>
            T10_1236_r19 = 0x0015,
            /// <summary>
            /// T10/1236 revision 20
            /// </summary>
            T10_1236_r20 = 0x0016,
            /// <summary>
            /// ANSI INCITS 351-2001
            /// </summary>
            ANSI_2001 = 0x0017,
        }

        enum SCSIVersionDescriptorOCRWMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ISO/IEC 14776-381
            /// </summary>
            ISO14776_381 = 0x001E
        }

        enum SCSIVersionDescriptorMMC3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1363-D revision 9
            /// </summary>
            T10_1363_r09 = 0x0015,
            /// <summary>
            /// T10/1363-D revision 10g
            /// </summary>
            T10_1363_r10g = 0x0016,
            /// <summary>
            /// ANSI INCITS 360-2002
            /// </summary>
            ANSI_2001 = 0x0018
        }

        enum SCSIVersionDescriptorSMC2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1383-D revision 5
            /// </summary>
            T10_1383_r05 = 0x0015,
            /// <summary>
            /// T10/1383-D revision 6
            /// </summary>
            T10_1383_r06 = 0x001C,
            /// <summary>
            /// T10/1363-D revision 7
            /// </summary>
            T10_1383_r07 = 0x001D,
            /// <summary>
            /// ANSI INCITS 382-2004
            /// </summary>
            ANSI_2004 = 0x001E
        }

        enum SCSIVersionDescriptorSPC3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1416-D revision 7
            /// </summary>
            T10_1416_r07 = 0x0001,
            /// <summary>
            /// T10/1416-D revision 21
            /// </summary>
            T10_1416_r21 = 0x0007,
            /// <summary>
            /// T10/1416-D revision 22
            /// </summary>
            T10_1416_r22 = 0x000F
        }

        enum SCSIVersionDescriptorSBC2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1383 revision 5
            /// </summary>
            T10_1383_r05 = 0x0002,
            /// <summary>
            /// T10/1383 revision 6
            /// </summary>
            T10_1383_r06 = 0x0004,
            /// <summary>
            /// T10/1383 revision 7
            /// </summary>
            T10_1383_r07 = 0x001B,
            /// <summary>
            /// ANSI INCITS 405-2005
            /// </summary>
            ANSI_2005 = 0x001D
        }

        enum SCSIVersionDescriptorOSDMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1355 revision 0
            /// </summary>
            T10_1355_r0 = 0x0001,
            /// <summary>
            /// T10/1355 revision 7a
            /// </summary>
            T10_1355_r7a = 0x0002,
            /// <summary>
            /// T10/1355 revision 8
            /// </summary>
            T10_1355_r8 = 0x0003,
            /// <summary>
            /// T10/1355 revision 9
            /// </summary>
            T10_1355_r9 = 0x0004,
            /// <summary>
            /// T10/1355 revision 10
            /// </summary>
            T10_1355_r10 = 0x0015,
            /// <summary>
            /// ANSI INCITS 400-2004
            /// </summary>
            ANSI_2004 = 0x0016
        }

        enum SCSIVersionDescriptorSSC2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1434-D revision 7
            /// </summary>
            T10_1434_r07 = 0x0014,
            /// <summary>
            /// T10/1434-D revision 9
            /// </summary>
            T10_1434_r09 = 0x0015,
            /// <summary>
            /// ANSI INCITS 380-2003
            /// </summary>
            ANSI_2003 = 0x001D
        }

        enum SCSIVersionDescriptorMMC4Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1545-D revision 5
            /// </summary>
            T10_1545_r5 = 0x0010,
            /// <summary>
            /// T10/1545-D revision 3
            /// </summary>
            T10_1545_r3 = 0x001D,
            /// <summary>
            /// T10/1545-D revision 3d
            /// </summary>
            T10_1545_r3d = 0x001E,
            /// <summary>
            /// ANSI INCITS 401-2005
            /// </summary>
            ANSI_2005 = 0x001F
        }

        enum SCSIVersionDescriptorADCMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1558-D revision 6
            /// </summary>
            T10_1558_r6 = 0x0015,
            /// <summary>
            /// T10/1558-D revision 7
            /// </summary>
            T10_1558_r7 = 0x0016,
            /// <summary>
            /// ANSI INCITS 403-2005
            /// </summary>
            ANSI_2005 = 0x0017
        }

        enum SCSIVersionDescriptorSSA_TL2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10.1/1147 revision 05b
            /// </summary>
            T10_1147_r05b = 0x001B,
            /// <summary>
            /// ANSI NCITS.308:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorSSA_TL1Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10.1/0989 revision 10b
            /// </summary>
            T10_0989_r10b = 0x001B,
            /// <summary>
            /// ANSI X3.295:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorSSA_S3PMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10.1/1051 revision 05b
            /// </summary>
            T10_1051_r05b = 0x001B,
            /// <summary>
            /// ANSI NCITS.309:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorSSA_S2PMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10.1/1121 revision 07b
            /// </summary>
            T10_1121_r07b = 0x001B,
            /// <summary>
            /// ANSI X3.294:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorSIPMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0856 revision 10
            /// </summary>
            T10_0856_r10 = 0x001B,
            /// <summary>
            /// ANSI X3.292:1997
            /// </summary>
            ANSI_1997 = 0x001C
        }

        enum SCSIVersionDescriptorFCPMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0993 revision 12
            /// </summary>
            T10_0993_r12 = 0x001B,
            /// <summary>
            /// ANSI X3.269:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorSBP2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1155 revision 04
            /// </summary>
            T10_1155_r04 = 0x001B,
            /// <summary>
            /// ANSI NCITS.325:1999
            /// </summary>
            ANSI_1999 = 0x001C
        }

        enum SCSIVersionDescriptorFCP2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1144-D revision 4
            /// </summary>
            T10_1144_r4 = 0x0001,
            /// <summary>
            /// T10/1144-D revision 7
            /// </summary>
            T10_1144_r7 = 0x0015,
            /// <summary>
            /// T10/1144-D revision 7a
            /// </summary>
            T10_1144_r7a = 0x0016,
            /// <summary>
            /// ANSI INCITS 350-2003
            /// </summary>
            ANSI_2003 = 0x0017,
            /// <summary>
            /// T10/1144-D revision 8
            /// </summary>
            T10_1144_r8 = 0x0018
        }

        enum SCSIVersionDescriptorSSTMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1380-D revision 8b
            /// </summary>
            T10_1380_r8b = 0x0015
        }

        enum SCSIVersionDescriptorSRPMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1415-D revision 10
            /// </summary>
            T10_1415_r10 = 0x0014,
            /// <summary>
            /// T10/1415-D revision 16a
            /// </summary>
            T10_1415_r16a = 0x0015,
            /// <summary>
            /// ANSI INCITS 365-2002
            /// </summary>
            ANSI_2003 = 0x001C
        }

        enum SCSIVersionDescriptorSBP3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1467-D revision 1f
            /// </summary>
            T10_1467_r1f = 0x0002,
            /// <summary>
            /// T10/1467-D revision 3
            /// </summary>
            T10_1467_r3 = 0x0014,
            /// <summary>
            /// T10/1467-D revision 4
            /// </summary>
            T10_1467_r4 = 0x001A,
            /// <summary>
            /// T10/1467-D revision 5
            /// </summary>
            T10_1467_r5 = 0x001B,
            /// <summary>
            /// ANSI INCITS 375-2004
            /// </summary>
            ANSI_2004 = 0x001C
        }

        enum SCSIVersionDescriptorADTMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1557-D revision 11
            /// </summary>
            T10_1557_r11 = 0x0019,
            /// <summary>
            /// T10/1557-D revision 14
            /// </summary>
            T10_1557_r14 = 0x001A,
            /// <summary>
            /// ANSI INCITS 406-2005
            /// </summary>
            ANSI_2005 = 0x001D
        }

        enum SCSIVersionDescriptorFCP3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1560-D revision 4
            /// </summary>
            T10_1560_r4 = 0x000F
        }

        enum SCSIVersionDescriptorSPIMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/0855 revision 15a
            /// </summary>
            T10_0855_r15a = 0x0019,
            /// <summary>
            /// ANSI X3.253:1995
            /// </summary>
            ANSI_1995 = 0x001A,
            /// <summary>
            /// T10/0855 revision 15a with SPI Amnd revision 3a
            /// </summary>
            T10_0855_r15a_Amnd_3a = 0x001B,
            /// <summary>
            /// ANSI X3.253:1995 with SPI Amnd ANSI X3.253/AM1:1998
            /// </summary>
            ANSI_1995_1998 = 0x001C
        }

        enum SCSIVersionDescriptorFast20Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1071 revision 06
            /// </summary>
            T10_1071_r06 = 0x001B,
            /// <summary>
            /// ANSI X3.277:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorSPI2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1142 revision 20b
            /// </summary>
            T10_1142_r20b = 0x001B,
            /// <summary>
            /// ANSI X3.302:1999
            /// </summary>
            ANSI_1999 = 0x001C
        }

        enum SCSIVersionDescriptorSPI3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1302-D revision 10
            /// </summary>
            T10_1302D_r10 = 0x0018,
            /// <summary>
            /// T10/1302-D revision 13a
            /// </summary>
            T10_1302D_r13a = 0x0019,
            /// <summary>
            /// T10/1302-D revision 14
            /// </summary>
            T10_1302D_r14 = 0x001A,
            /// <summary>
            /// ANSI NCITS.336:2000
            /// </summary>
            ANSI_2000 = 0x001C
        }

        enum SCSIVersionDescriptorEPIMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1134 revision 16
            /// </summary>
            T10_1134_r16 = 0x001B,
            /// <summary>
            /// ANSI NCITS TR-23:1999
            /// </summary>
            ANSI_1999 = 0x001C
        }

        enum SCSIVersionDescriptorSPI4Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1365-D revision 7
            /// </summary>
            T10_1365_r7 = 0x0014,
            /// <summary>
            /// T10/1365-D revision 9
            /// </summary>
            T10_1365_r9 = 0x0015,
            /// <summary>
            /// ANSI INCITS 362-2002
            /// </summary>
            ANSI_2002 = 0x0016,
            /// <summary>
            /// T10/1365-D revision 10
            /// </summary>
            T10_1365_r10 = 0x0019
        }

        enum SCSIVersionDescriptorSPI5Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1525-D revision 3
            /// </summary>
            T10_1525_r3 = 0x0019,
            /// <summary>
            /// T10/1525-D revision 5
            /// </summary>
            T10_1525_r5 = 0x001A,
            /// <summary>
            /// T10/1525-D revision 6
            /// </summary>
            T10_1525_r6 = 0x001B,
            /// <summary>
            /// ANSI INCITS 367-2003
            /// </summary>
            ANSI_2003 = 0x001C
        }

        enum SCSIVersionDescriptorSASMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1562-D revision 01
            /// </summary>
            T10_1562_r01 = 0x0001,
            /// <summary>
            /// T10/1562-D revision 03
            /// </summary>
            T10_1562_r03 = 0x0015,
            /// <summary>
            /// T10/1562-D revision 04
            /// </summary>
            T10_1562_r04 = 0x001A,
            /// <summary>
            /// T10/1562-D revision 04
            /// </summary>
            T10_1562_r04bis = 0x001B,
            /// <summary>
            /// T10/1562-D revision 05
            /// </summary>
            T10_1562_r05 = 0x001C,
            /// <summary>
            /// ANSI INCITS 376-2003
            /// </summary>
            ANSI_2003 = 0x001D
        }

        enum SCSIVersionDescriptorSAS11Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10/1601-D revision 9
            /// </summary>
            T10_1601_r9 = 0x0007,
            /// <summary>
            /// T10/1601-D revision 10
            /// </summary>
            T10_1601_r10 = 0x000F
        }

        enum SCSIVersionDescriptorFCPHMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ANSI X3.230:1994
            /// </summary>
            ANSI_1994 = 0x001B,
            /// <summary>
            /// ANSI X3.230:1994 with Amnd 1 ANSI X3.230/AM1:1996
            /// </summary>
            ANSI_1994_1996 = 0x001C
        }

        enum SCSIVersionDescriptorFCALMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ANSI X3.272:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorFCAL2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1133 revision 7.0
            /// </summary>
            T11_1133_r70 = 0x0001,
            /// <summary>
            /// ANSI NCITS.332:1999
            /// </summary>
            ANSI_1999 = 0x001C,
            /// <summary>
            /// ANSI INCITS 332-1999 with Amnd 1 AM1-2002
            /// </summary>
            ANSI_1999_2002 = 0x001D
        }

        enum SCSIVersionDescriptorFCPH3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ANSI X3.303-1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorFCFSMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1331 revision 1.2
            /// </summary>
            T11_1331_r12 = 0x0017,
            /// <summary>
            /// T11/1331 revision 1.7
            /// </summary>
            T11_1331_r17 = 0x0018,
            /// <summary>
            /// ANSI INCITS 373-2003
            /// </summary>
            ANSI_2003 = 0x001C
        }

        enum SCSIVersionDescriptorFCPIMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ANSI INCITS 352-2002
            /// </summary>
            ANSI_2002 = 0x001C
        }

        enum SCSIVersionDescriptorFCPI2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1506-D revision 5.0
            /// </summary>
            T11_1506_r50 = 0x0002
        }

        enum SCSIVersionDescriptorFCSPMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1570-D revision 1.6
            /// </summary>
            T11_1570_r16 = 0x0002
        }

        enum SCSIVersionDescriptorFCDAMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1513-DT revision 3.1
            /// </summary>
            T11_1513_r31 = 0x0002
        }

        enum SCSIVersionDescriptorFCTapeMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1315 revision 1.16
            /// </summary>
            T11_1315_r116 = 0x0001,
            /// <summary>
            /// T11/1315 revision 1.17
            /// </summary>
            T11_1315_r117 = 0x001B,
            /// <summary>
            /// ANSI NCITS TR-24:1999
            /// </summary>
            ANSI_1999 = 0x001C
        }

        enum SCSIVersionDescriptorFCFLAMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1235 revision 7
            /// </summary>
            T11_1235_r7 = 0x001B,
            /// <summary>
            /// ANSI NCITS TR-20:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorFCPLDAMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T11/1162 revision 2.1
            /// </summary>
            T11_1162_r21 = 0x001B,
            /// <summary>
            /// ANSI NCITS TR-19:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorSSAPH2Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10.1/1145 revision 09c
            /// </summary>
            T10_1145_r09c = 0x001B,
            /// <summary>
            /// ANSI X3.293:1996
            /// </summary>
            ANSI_1996 = 0x001C
        }

        enum SCSIVersionDescriptorSSAPH3Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T10.1/1146 revision 05b
            /// </summary>
            T10_1146_r05b = 0x001B,
            /// <summary>
            /// ANSI NCITS.307:1998
            /// </summary>
            ANSI_1998 = 0x001C
        }

        enum SCSIVersionDescriptorIEEE1394Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ANSI IEEE 1394:1995
            /// </summary>
            ANSI_1995 = 0x001D
        }

        enum SCSIVersionDescriptorATA6Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// ANSI INCITS 361-2002
            /// </summary>
            ANSI_2002 = 0x001D
        }

        enum SCSIVersionDescriptorATA7Mask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// T13/1532-D revision 3
            /// </summary>
            T13_1532_r3 = 0x0002,
            /// <summary>
            /// ANSI INCITS 397-2005
            /// </summary>
            ANSI_2005 = 0x001C
        }

        enum SCSIVersionDescriptorATA8Mask : ushort
        {
            /// <summary>
            /// ATA8-AAM Architecture Model (no version claimed)
            /// </summary>
            ATA8_AAM = 0x0000,
            /// <summary>
            /// ATA8-PT Parallel Transport (no version claimed)
            /// </summary>
            ATA8_PT = 0x0001,
            /// <summary>
            /// ATA8-AST Serial Transport (no version claimed)
            /// </summary>
            ATA8_AST = 0x0002,
            /// <summary>
            /// ATA8-ACS ATA/ATAPI Command Set (no version claimed)
            /// </summary>
            ATA8_ACS = 0x0003
        }

        enum SCSIVersionDescriptorUSBMask : ushort
        {
            /// <summary>
            /// No revision of the standard is claimed
            /// </summary>
            NoVersion = 0x0000,
            /// <summary>
            /// Universal Serial Bus Specification, Revision 1.1
            /// </summary>
            USB11 = 0x0008,
            /// <summary>
            /// Universal Serial Bus Specification, Revision 2.0
            /// </summary>
            USB20 = 0x0009,
            /// <summary>
            /// USB Mass Storage Class Bulk-Only Transport, Revision 1.0
            /// </summary>
            USB_MSC_BULK = 0x0010
        }

        enum SCSITGPSValues : byte
        {
            /// <summary>
            /// Assymetrical access not supported
            /// </summary>
            NotSupported = 0x00,
            /// <summary>
            /// Only implicit assymetrical access is supported
            /// </summary>
            OnlyImplicit = 0x01,
            /// <summary>
            /// Only explicit assymetrical access is supported
            /// </summary>
            OnlyExplicit = 0x02,
            /// <summary>
            /// Both implicit and explicit assymetrical access are supported
            /// </summary>
            Both = 0x03
        }

        #endregion Enumerations

        #region Private methods

        static string PrettifySCSIVendorString(string SCSIVendorString)
        {
            switch (SCSIVendorString)
            {
                case "3M":
                    return "3M Company";
                case "ACL":
                    return "Automated Cartridge Librarys, Inc.";
                case "ADAPTEC":
                    return "Adaptec";
                case "ADSI":
                    return "Adaptive Data Systems, Inc. (a Western Digital subsidiary)";
                case "ADTX":
                    return "ADTX Co., Ltd.";
                case "AERONICS":
                    return "Aeronics, Inc.";
                case "AGFA":
                    return "AGFA";
                case "AMCODYNE":
                    return "Amcodyne";
                case "ANAMATIC":
                    return "Anamartic Limited (England)";
                case "ANCOT":
                    return "ANCOT Corp.";
                case "ANRITSU":
                    return "Anritsu Corporation";
                case "APPLE":
                    return "Apple Computer, Inc.";
                case "ARCHIVE":
                    return "Archive";
                case "ASACA":
                    return "ASACA Corp.";
                case "ASPEN":
                    return "Aspen Peripherals";
                case "AST":
                    return "AST Research";
                case "ASTK":
                    return "Alcatel STK A/S";
                case "AT&T":
                    return "AT&T";
                case "ATARI":
                    return "Atari Corporation";
                case "ATG CYG":
                    return "ATG Cygnet Inc.";
                case "ATTO":
                    return "ATTO Technology Inc.";
                case "ATX":
                    return "Alphatronix";
                case "AVR":
                    return "Advanced Vision Research";
                case "BALLARD":
                    return "Ballard Synergy Corp.";
                case "BERGSWD":
                    return "Berg Software Design";
                case "BEZIER":
                    return "Bezier Systems, Inc.";
                case "BULL":
                    return "Bull Peripherals Corp.";
                case "BUSLOGIC":
                    return "BusLogic Inc.";
                case "BiT":
                    return "BiT Microsystems";
                case "BoxHill":
                    return "Box Hill Systems Corporation";
                case "CALIPER":
                    return "Caliper (California Peripheral Corp.)";
                case "CAST":
                    return "Advanced Storage Tech";
                case "CDC":
                    return "Control Data or MPI";
                case "CDP":
                    return "Columbia Data Products";
                case "CHEROKEE":
                    return "Cherokee Data Systems";
                case "CHINON":
                    return "Chinon";
                case "CIE&YED":
                    return "YE Data, C.Itoh Electric Corp.";
                case "CIPHER":
                    return "Cipher Data Products";
                case "CIRRUSL":
                    return "Cirrus Logic Inc.";
                case "CMD":
                    return "CMD Technology Inc.";
                case "CNGR SFW":
                    return "Congruent Software, Inc.";
                case "COGITO":
                    return "Cogito";
                case "COMPAQ":
                    return "Compaq Computer Corporation";
                case "COMPORT":
                    return "Comport Corp.";
                case "COMPSIG":
                    return "Computer Signal Corporation";
                case "CONNER":
                    return "Conner Peripherals";
                case "CORE":
                    return "Core International, Inc.";
                case "CPU TECH":
                    return "CPU Technology, Inc.";
                case "CREO":
                    return "Creo Products Inc.";
                case "CROSFLD":
                    return "Crosfield Electronics";
                case "CSM, INC":
                    return "Computer SM, Inc.";
                case "CalComp":
                    return "CalComp, A Lockheed Company";
                case "Ciprico":
                    return "Ciprico, Inc.";
                case "DATABOOK":
                    return "Databook, Inc.";
                case "DATACOPY":
                    return "Datacopy Corp.";
                case "DATAPT":
                    return "Datapoint Corp.";
                case "DEC":
                    return "Digital Equipment";
                case "DELPHI":
                    return "Delphi Data Div. of Sparks Industries, Inc.";
                case "DENON":
                    return "Denon/Nippon Columbia";
                case "DenOptix":
                    return "DenOptix, Inc.";
                case "DEST":
                    return "DEST Corp.";
                case "DGC":
                    return "Data General Corp.";
                case "DIGIDATA":
                    return "Digi-Data Corporation";
                case "DILOG":
                    return "Distributed Logic Corp.";
                case "DISC":
                    return "Document Imaging Systems Corp.";
                case "DPT":
                    return "Distributed Processing Technology";
                case "DSI":
                    return "Data Spectrum, Inc.";
                case "DSM":
                    return "Deterner Steuerungs- und Maschinenbau GmbH & Co.";
                case "DTC QUME":
                    return "Data Technology Qume";
                case "DXIMAGIN":
                    return "DX Imaging";
                case "Digital":
                    return "Digital Equipment Corporation";
                case "ECMA":
                    return "European Computer Manufacturers Association";
                case "Elms":
                    return "Elms Systems Corporation";
                case "EMC":
                    return "EMC Corp.";
                case "EMULEX":
                    return "Emulex";
                case "EPSON":
                    return "Epson";
                case "Eris/RSI":
                    return "RSI Systems, Inc.";
                case "EXABYTE":
                    return "Exabyte Corp.";
                case "FILENET":
                    return "FileNet Corp.";
                case "FRAMDRV":
                    return "FRAMEDRIVE Corp.";
                case "FUJI":
                    return "Fuji Electric Co., Ltd. (Japan)";
                case "FUJITSU":
                    return "Fujitsu";
                case "FUNAI":
                    return "Funai Electric Co., Ltd.";
                case "FUTURED":
                    return "Future Domain Corp.";
                case "GIGATAPE":
                    return "GIGATAPE GmbH";
                case "GIGATRND":
                    return "GigaTrend Incorporated";
                case "GOULD":
                    return "Gould";
                case "Gen_Dyn":
                    return "General Dynamics";
                case "Goidelic":
                    return "Goidelic Precision, Inc.";
                case "HITACHI":
                    return "Hitachi America Ltd or Nissei Sangyo America Ltd";
                case "HONEYWEL":
                    return "Honeywell Inc.";
                case "HP":
                    return "Hewlett Packard";
                case "i-cubed":
                    return "i-cubed ltd.";
                case "IBM":
                    return "International Business Machines";
                case "ICL":
                    return "ICL";
                case "IDE":
                    return "International Data Engineering, Inc.";
                case "IGR":
                    return "Intergraph Corp.";
                case "IMPLTD":
                    return "Integrated Micro Products Ltd.";
                case "IMPRIMIS":
                    return "Imprimis Technology Inc.";
                case "INSITE":
                    return "Insite Peripherals";
                case "INTEL":
                    return "INTEL Corporation";
                case "IOC":
                    return "I/O Concepts, Inc.";
                case "IOMEGA":
                    return "Iomega";
                case "ISi":
                    return "Information Storage inc.";
                case "ISO":
                    return "International Standards Organization";
                case "ITC":
                    return "International Tapetronics Corporation";
                case "JPC Inc.":
                    return "JPC Inc.";
                case "JVC":
                    return "JVC Information Products Co.";
                case "KENNEDY":
                    return "Kennedy Company";
                case "KENWOOD":
                    return "KENWOOD Corporation";
                case "KODAK":
                    return "Eastman Kodak";
                case "KONAN":
                    return "Konan";
                case "KONICA":
                    return "Konica Japan";
                case "LAPINE":
                    return "Lapine Technology";
                case "LASERDRV":
                    return "LaserDrive Limited";
                case "LASERGR":
                    return "Lasergraphics, Inc.";
                case "LION":
                    return "Lion Optics Corporation";
                case "LMS":
                    return "Laser Magnetic Storage International Company";
                case "MATSHITA":
                    return "Matsushita";
                case "MAXSTRAT":
                    return "Maximum Strategy, Inc.";
                case "MAXTOR":
                    return "Maxtor Corp.";
                case "MDI":
                    return "Micro Design International, Inc.";
                case "MEADE":
                    return "Meade Instruments Corporation";
                case "MELA":
                    return "Mitsubishi Electronics America";
                case "MELCO":
                    return "Mitsubishi Electric (Japan)";
                case "MEMREL":
                    return "Memrel Corporation";
                case "MEMTECH":
                    return "MemTech Technology";
                case "MERIDATA":
                    return "Oy Meridata Finland Ltd.";
                case "METRUM":
                    return "Metrum, Inc.";
                case "MICROBTX":
                    return "Microbotics Inc.";
                case "MICROP":
                    return "Micropolis";
                case "MICROTEK":
                    return "Microtek Storage Corp";
                case "MINSCRIB":
                    return "Miniscribe";
                case "MITSUMI":
                    return "Mitsumi Electric Co., Ltd.";
                case "MOTOROLA":
                    return "Motorola";
                case "MST":
                    return "Morning Star Technologies, Inc.";
                case "MTNGATE":
                    return "MountainGate Data Systems";
                case "MaxOptix":
                    return "Maxoptix Corp.";
                case "Minitech":
                    return "Minitech (UK) Limited";
                case "Minolta":
                    return "Minolta Corporation";
                case "NAI":
                    return "North Atlantic Industries";
                case "NAKAMICH":
                    return "Nakamichi Corporation";
                case "NCL":
                    return "NCL America";
                case "NCR":
                    return "NCR Corporation";
                case "NEC":
                    return "NEC";
                case "NISCA":
                    return "NISCA Inc.";
                case "NKK":
                    return "NKK Corp.";
                case "NRC":
                    return "Nakamichi Corporation";
                case "NSM":
                    return "NSM Jukebox GmbH";
                case "NT":
                    return "Northern Telecom";
                case "NatInst":
                    return "National Instruments";
                case "NatSemi":
                    return "National Semiconductor Corp.";
                case "OAI":
                    return "Optical Access International";
                case "OCE":
                    return "Oce Graphics";
                case "OKI":
                    return "OKI Electric Industry Co.,Ltd (Japan)";
                case "OMI":
                    return "Optical Media International";
                case "OMNIS":
                    return "OMNIS Company (FRANCE)";
                case "OPTIMEM":
                    return "Cipher/Optimem";
                case "OPTOTECH":
                    return "Optotech";
                case "ORCA":
                    return "Orca Technology";
                case "OSI":
                    return "Optical Storage International";
                case "OTL":
                    return "OTL Engineering";
                case "PASCOsci":
                    return "Pasco Scientific";
                case "PERTEC":
                    return "Pertec Peripherals Corporation";
                case "PFTI":
                    return "Performance Technology Inc.";
                case "PFU":
                    return "PFU Limited";
                case "PIONEER":
                    return "Pioneer Electronic Corp.";
                case "PLASMON":
                    return "Plasmon Data";
                case "PRAIRIE":
                    return "PrairieTek";
                case "PREPRESS":
                    return "PrePRESS Solutions";
                case "PRESOFT":
                    return "PreSoft Architects";
                case "PRESTON":
                    return "Preston Scientific";
                case "PRIAM":
                    return "Priam";
                case "PRIMAGFX":
                    return "Primagraphics Ltd";
                case "PTI":
                    return "Peripheral Technology Inc.";
                case "QIC":
                    return "Quarter-Inch Cartridge Drive Standards, Inc.";
                case "QUALSTAR":
                    return "Qualstar";
                case "QUANTUM":
                    return "Quantum Corp.";
                case "QUANTEL":
                    return "Quantel Ltd.";
                case "R-BYTE":
                    return "R-Byte, Inc.";
                case "RACALREC":
                    return "Racal Recorders";
                case "RADSTONE":
                    return "Radstone Technology";
                case "RGI":
                    return "Raster Graphics, Inc.";
                case "RICOH":
                    return "Ricoh";
                case "RODIME":
                    return "Rodime";
                case "RTI":
                    return "Reference Technology";
                case "SAMSUNG":
                    return "Samsung Electronics Co., Ltd.";
                case "SANKYO":
                    return "Sankyo Seiki";
                case "SANYO":
                    return "SANYO Electric Co., Ltd.";
                case "SCREEN":
                    return "Dainippon Screen Mfg. Co., Ltd.";
                case "SEAGATE":
                    return "Seagate";
                case "SEQUOIA":
                    return "Sequoia Advanced Technologies, Inc.";
                case "SIEMENS":
                    return "Siemens";
                case "SII":
                    return "Seiko Instruments Inc.";
                case "SMS":
                    return "Scientific Micro Systems/OMTI";
                case "SNYSIDE":
                    return "Sunnyside Computing Inc.";
                case "SONIC":
                    return "Sonic Solutions";
                case "SONY":
                    return "Sony Corporation Japan";
                case "SPECIAL":
                    return "Special Computing Co.";
                case "SPECTRA":
                    return "Spectra Logic, a Division of Western Automation Labs, Inc.";
                case "SPERRY":
                    return "Sperry (now Unisys Corp.)";
                case "STK":
                    return "Storage Technology Corporation";
                case "StrmLgc":
                    return "StreamLogic Corp.";
                case "SUMITOMO":
                    return "Sumitomo Electric Industries, Ltd.";
                case "SUN":
                    return "Sun Microsystems, Inc.";
                case "SYMBIOS":
                    return "Symbios Logic Inc.";
                case "SYSGEN":
                    return "Sysgen";
                case "Shinko":
                    return "Shinko Electric Co., Ltd.";
                case "SyQuest":
                    return "SyQuest Technology, Inc.";
                case "T-MITTON":
                    return "Transmitton England";
                case "TALARIS":
                    return "Talaris Systems, Inc.";
                case "TALLGRAS":
                    return "Tallgrass Technologies";
                case "TANDBERG":
                    return "Tandberg Data A/S";
                case "TANDON":
                    return "Tandon";
                case "TEAC":
                    return "TEAC Japan";
                case "TECOLOTE":
                    return "Tecolote Designs";
                case "TEGRA":
                    return "Tegra Varityper";
                case "TENTIME":
                    return "Laura Technologies, Inc.";
                case "TI-DSG":
                    return "Texas Instruments";
                case "TOSHIBA":
                    return "Toshiba Japan";
                case "Tek":
                    return "Tektronix";
                case "ULTRA":
                    return "UltraStor Corporation";
                case "UNISYS":
                    return "Unisys";
                case "USCORE":
                    return "Underscore, Inc.";
                case "USDC":
                    return "US Design Corp.";
                case "VERBATIM":
                    return "Verbatim Corporation";
                case "VEXCEL":
                    return "VEXCEL IMAGING GmbH";
                case "VICOMSL1":
                    return "Vicom Systems, Inc.";
                case "VRC":
                    return "Vermont Research Corp.";
                case "WANGTEK":
                    return "Wangtek";
                case "WDIGTL":
                    return "Western Digital";
                case "WEARNES":
                    return "Wearnes Technology Corporation";
                case "WangDAT":
                    return "WangDAT";
                case "X3":
                    return "Accredited Standards Committee X3, Information Technology";
                case "XEBEC":
                    return "Xebec Corporation";
                case "Acuid":
                    return "Acuid Corporation Ltd.";
                case "AcuLab":
                    return "AcuLab, Inc. (Tulsa, OK)";
                case "ADIC":
                    return "Advanced Digital Information Corporation";
                case "ADVA":
                    return "ADVA Optical Networking AG";
                case "Ancor":
                    return "Ancor Communications, Inc.";
                case "ANDATACO":
                    return "Andataco (now nStor)";
                case "ARK":
                    return "ARK Research Corporation";
                case "ARTECON":
                    return "Artecon Inc. (Obs. - now Dot Hill)";
                case "ASC":
                    return "Advanced Storage Concepts, Inc.";
                case "BHTi":
                    return "Breece Hill Technologies";
                case "BITMICRO":
                    return "BiT Microsystems, Inc.";
                case "BNCHMARK":
                    return "Benchmark Tape Systems Corporation";
                case "BREA":
                    return "BREA Technologies, Inc.";
                case "BROCADE":
                    return "Brocade Communications Systems, Incorporated";
                case "CenData":
                    return "Central Data Corporation";
                case "Cereva":
                    return "Cereva Networks Inc.";
                case "CISCO":
                    return "Cisco Systems, Inc.";
                case "CNSi":
                    return "Chaparral Network Storage, Inc.";
                case "COMPTEX":
                    return "Comptex Pty Limited";
                case "CPL":
                    return "Cross Products Ltd";
                case "CROSSRDS":
                    return "Crossroads Systems, Inc.";
                case "Data Com":
                    return "Data Com Information Systems Pty. Ltd.";
                case "DataCore":
                    return "DataCore Software Corporation";
                case "DDN":
                    return "DataDirect Networks, Inc.";
                case "DEI":
                    return "Digital Engineering, Inc.";
                case "DELL":
                    return "Dell Computer Corporation";
                case "DigiIntl":
                    return "Digi International";
                case "DotHill":
                    return "Dot Hill Systems Corp.";
                case "ECCS":
                    return "ECCS, Inc.";
                case "EMASS":
                    return "EMASS, Inc.";
                case "EMTEC":
                    return "EMTEC Magnetics";
                case "EuroLogc":
                    return "Eurologic Systems Limited";
                case "FFEILTD":
                    return "FujiFilm Electonic Imaging Ltd";
                case "FUJIFILM":
                    return "Fuji Photo Film, Co., Ltd.";
                case "G&D":
                    return "Giesecke & Devrient GmbH";
                case "GENSIG":
                    return "General Signal Networks";
                case "Global":
                    return "Global Memory Test Consortium";
                case "GoldStar":
                    return "LG Electronics Inc.";
                case "HAGIWARA":
                    return "Hagiwara Sys-Com Co., Ltd.";
                case "ICP":
                    return "ICP vortex Computersysteme GmbH";
                case "IMATION":
                    return "Imation";
                case "Indigita":
                    return "Indigita Corporation";
                case "INITIO":
                    return "Initio Corporation";
                case "IVIVITY":
                    return "iVivity, Inc.";
                case "Kyocera":
                    return "Kyocera Corporation";
                case "LG":
                    return "LG Electronics Inc.";
                case "LGE":
                    return "LG Electronics Inc.";
                case "LSI":
                    return "LSI Logic Corp.";
                case "LSILOGIC":
                    return "LSI Logic Storage Systems, Inc.";
                case "LTO-CVE":
                    return "Linear Tape - Open, Compliance Verification Entity";
                case "MAXELL":
                    return "Hitachi Maxell, Ltd.";
                case "McDATA":
                    return "McDATA Corporation";
                case "MEII":
                    return "Mountain Engineering II, Inc.";
                case "MOSAID":
                    return "Mosaid Technologies Inc.";
                case "MPM":
                    return "Mitsubishi Paper Mills, Ltd.";
                case "NCITS":
                    return "National Committee for Information Technology Standards";
                case "NEXSAN":
                    return "Nexsan Technologies, Ltd.";
                case "NISHAN":
                    return "Nishan Systems Inc.";
                case "NSD":
                    return "Nippon Systems Development Co.,Ltd.";
                case "nStor":
                    return "nStor Technologies, Inc.";
                case "NUCONNEX":
                    return "NuConnex";
                case "NUSPEED":
                    return "NuSpeed, Inc.";
                case "ORANGE":
                    return "Orange Micro, Inc.";
                case "PATHLGHT":
                    return "Pathlight Technology, Inc.";
                case "PICO":
                    return "Packard Instrument Company";
                case "PROCOM":
                    return "Procom Technology";
                case "RHAPSODY":
                    return "Rhapsody Networks, Inc.";
                case "RHS":
                    return "Racal-Heim Systems GmbH";
                case "SAN":
                    return "Storage Area Networks, Ltd.";
                case "SCInc.":
                    return "Storage Concepts, Inc.";
                case "SDI":
                    return "Storage Dimensions, Inc.";
                case "SDS":
                    return "Solid Data Systems";
                case "SPD":
                    return "Storage Products Distribution, Inc.";
                case "Sterling":
                    return "Sterling Diagnostic Imaging, Inc.";
                case "STOR":
                    return "StorageNetworks, Inc.";
                case "STORAPP":
                    return "StorageApps, Inc.";
                case "STORM":
                    return "Storm Technology, Inc.";
                case "TDK":
                    return "TDK Corporation";
                case "TMS":
                    return "Texas Memory Systems, Inc.";
                case "TRIPACE":
                    return "Tripace";
                case "VDS":
                    return "Victor Data Systems Co., Ltd.";
                case "VIXEL":
                    return "Vixel Corporation";
                case "WSC0001":
                    return "Wisecom, Inc.";
                case "Mendocin":
                    return "Mendocino Software";
                case "0B4C":
                    return "MOOSIK Ltd.";
                case "2AI":
                    return "2AI (Automatisme et Avenir Informatique)";
                case "3PARdata":
                    return "3PARdata, Inc.";
                case "A-Max":
                    return "A-Max Technology Co., Ltd";
                case "Acer":
                    return "Acer, Inc.";
                case "AIPTEK":
                    return "AIPTEK International Inc.";
                case "AMCC":
                    return "Applied Micro Circuits Corporation";
                case "Amphenol":
                    return "Amphenol";
                case "andiamo":
                    return "Andiamo Systems, Inc.";
                case "ANTONIO":
                    return "Antonio Precise Products Manufactory Ltd.";
                case "ARIO":
                    return "Ario Data Networks, Inc.";
                case "ARISTOS":
                    return "Aristos Logic Corp.";
                case "ATA":
                    return "SCSI / ATA Translator Software (Organization Not Specified)";
                case "ATL":
                    return "Quantum|ATL Products";
                case "AVC":
                    return "AVC Technology Ltd";
                case "Barco":
                    return "Barco";
                case "BAROMTEC":
                    return "Barom Technologies Co., Ltd.";
                case "BDT":
                    return "Buero- und Datentechnik GmbH & Co.KG";
                case "BENQ":
                    return "BENQ Corporation.";
                case "BIR":
                    return "Bio-Imaging Research, Inc.";
                case "BlueArc":
                    return "BlueArc Corporation";
                case "Broadcom":
                    return "Broadcom Corporation";
                case "CAMBEX":
                    return "Cambex Corporation";
                case "CAMEOSYS":
                    return "Cameo Systems Inc.";
                case "CANDERA":
                    return "Candera Inc.";
                case "CAPTION":
                    return "CAPTION BANK";
                case "CATALYST":
                    return "Catalyst Enterprises";
                case "CERTANCE":
                    return "Certance";
                case "CLOVERLF":
                    return "Cloverleaf Communications, Inc";
                case "CMTechno":
                    return "CMTech";
                case "CNT":
                    return "Computer Network Technology";
                case "COBY":
                    return "Coby Electronics Corporation, USA";
                case "COMPELNT":
                    return "Compellent Technologies, Inc.";
                case "COPANSYS":
                    return "COPAN SYSTEMS INC";
                case "COWON":
                    return "COWON SYSTEMS, Inc.";
                case "CSCOVRTS":
                    return "Cisco - Veritas";
                case "CYBERNET":
                    return "Cybernetics";
                case "Cygnal":
                    return "Dekimo";
                case "DALSEMI":
                    return "Dallas Semiconductor";
                case "DANGER":
                    return "Danger Inc.";
                case "DAT-MG":
                    return "DAT Manufacturers Group";
                case "DNS":
                    return "Data and Network Security";
                case "DP":
                    return "Dell, Inc.";
                case "DSC":
                    return "DigitalStream Corporation";
                case "elipsan":
                    return "Elipsan UK Ltd.";
                case "ENERGY-B":
                    return "Energybeam Corporation";
                case "ENGENIO":
                    return "Engenio Information Technologies, Inc.";
                case "EQLOGIC":
                    return "EqualLogic";
                case "evolve":
                    return "Evolution Technologies, Inc";
                case "EXATEL":
                    return "Exatelecom Co., Ltd.";
                case "EXAVIO":
                    return "Exavio, Inc.";
                case "FALCON":
                    return "FalconStor, Inc.";
                case "Fibxn":
                    return "Fiberxon, Inc.";
                case "FID":
                    return "First International Digital, Inc.";
                case "FREECION":
                    return "Nable Communications, Inc.";
                case "Gadzoox":
                    return "Gadzoox Networks, Inc.";
                case "GDI":
                    return "Generic Distribution International";
                case "Generic":
                    return "Generic Technology Co., Ltd.";
                case "HAPP3":
                    return "Inventec Multimedia and Telecom co., ltd";
                case "Heydays":
                    return "Mazo Technology Co., Ltd.";
                case "HI-TECH":
                    return "HI-TECH Software Pty. Ltd.";
                case "HPQ":
                    return "Hewlett Packard";
                case "HYUNWON":
                    return "HYUNWON inc";
                case "IET":
                    return "ISCSI ENTERPRISE TARGET";
                case "IFT":
                    return "Infortrend Technology, Inc.";
                case "INCIPNT":
                    return "Incipient Technologies Inc.";
                case "INCITS":
                    return "InterNational Committee for Information Technology";
                case "INRANGE":
                    return "INRANGE Technologies Corporation";
                case "integrix":
                    return "Integrix, Inc.";
                case "iqstor":
                    return "iQstor Networks, Inc.";
                case "IVMMLTD":
                    return "InnoVISION Multimedia Ltd.";
                case "JETWAY":
                    return "Jetway Information Co., Ltd";
                case "KASHYA":
                    return "Kashya, Inc.";
                case "KSCOM":
                    return "KSCOM Co. Ltd.,";
                case "KUDELSKI":
                    return "Nagravision SA - Kudelski Group";
                case "LEFTHAND":
                    return "LeftHand Networks";
                case "Lexar":
                    return "Lexar Media, Inc.";
                case "LUXPRO":
                    return "Luxpro Corporation";
                case "Malakite":
                    return "Malachite Technologies (New VID is: Sandial)";
                case "MaXXan":
                    return "MaXXan Systems, Inc.";
                case "MAYCOM":
                    return "maycom Co., Ltd.";
                case "MBEAT":
                    return "K-WON C&C Co.,Ltd";
                case "MCC":
                    return "Measurement Computing Corporation";
                case "MHTL":
                    return "Matsunichi Hi-Tech Limited";
                case "MICROLIT":
                    return "Microlite Corporation";
                case "MKM":
                    return "Mitsubishi Kagaku Media Co., LTD.";
                case "MP-400":
                    return "Daiwa Manufacturing Limited";
                case "MPEYE":
                    return "Touchstone Technology Co., Ltd";
                case "MPMan":
                    return "MPMan.com, Inc.";
                case "MSFT":
                    return "Microsoft Corporation";
                case "MSI":
                    return "Micro-Star International Corp.";
                case "MTI":
                    return "MTI Technology Corporation";
                case "MXI":
                    return "Memory Experts International";
                case "nac":
                    return "nac Image Technology Inc.";
                case "NAGRA":
                    return "Nagravision SA - Kudelski Group";
                case "Neartek":
                    return "Neartek, Inc.";
                case "NETAPP":
                    return "Network Appliance";
                case "Netcom":
                    return "Netcom Storage";
                case "NHR":
                    return "NH Research, Inc.";
                case "NVIDIA":
                    return "NVIDIA Corporation";
                case "Olidata":
                    return "Olidata S.p.A.";
                case "OMNIFI":
                    return "Rockford Corporation - Omnifi Media";
                case "Packard":
                    return "Parkard Bell";
                case "PARALAN":
                    return "Paralan Corporation";
                case "PerStor":
                    return "Perstor";
                case "PHILIPS":
                    return "Philips Electronics";
                case "Pillar":
                    return "Pillar Data Systems";
                case "PIVOT3":
                    return "Pivot3, Inc.";
                case "PROSTOR":
                    return "ProStor Systems, Inc.";
                case "PTICO":
                    return "Pacific Technology International";
                case "QLogic":
                    return "QLogic Corporation";
                case "Realm":
                    return "Realm Systems";
                case "Revivio":
                    return "Revivio, Inc.";
                case "SANRAD":
                    return "SANRAD Inc.";
                case "SC.Net":
                    return "StorageConnections.Net";
                case "SCIENTEK":
                    return "SCIENTEK CORP";
                case "SEAC":
                    return "SeaChange International, Inc.";
                case "SEAGRAND":
                    return "SEAGRAND In Japan";
                case "SigmaTel":
                    return "SigmaTel, Inc.";
                case "SLI":
                    return "Sierra Logic, Inc.";
                case "SoniqCas":
                    return "SoniqCast";
                case "STONEFLY":
                    return "StoneFly Networks, Inc.";
                case "STORCOMP":
                    return "Storage Computer Corporation";
                case "SUNCORP":
                    return "SunCorporation";
                case "suntx":
                    return "Suntx System Co., Ltd";
                case "SYMANTEC":
                    return "Symantec Corporation";
                case "T11":
                    return "INCITS Technical Committee T11";
                case "TANDEM":
                    return "Tandem (now HP)";
                case "TGEGROUP":
                    return "TGE Group Co.,LTD.";
                case "Tite":
                    return "Tite Technology Limited";
                case "TOLISGRP":
                    return "The TOLIS Group";
                case "TROIKA":
                    return "Troika Networks, Inc.";
                case "TRULY":
                    return "TRULY Electronics MFG. LTD.";
                case "UDIGITAL":
                    return "United Digital Limited";
                case "VERITAS":
                    return "VERITAS Software Corporation";
                case "VicomSys":
                    return "Vicom Systems, Inc.";
                case "VIDEXINC":
                    return "Videx, Inc.";
                case "VITESSE":
                    return "Vitesse Semiconductor Corporation";
                case "VMAX":
                    return "VMAX Technologies Corp.";
                case "Vobis":
                    return "Vobis Microcomputer AG";
                case "Waitec":
                    return "Waitec NV";
                case "Wasabi":
                    return "Wasabi Systems";
                case "WAVECOM":
                    return "Wavecom";
                case "WD":
                    return "Western Digital Technologies Inc.";
                case "WDC":
                    return "Western Digital Technologies inc.";
                case "Xerox":
                    return "Xerox Corporation";
                case "XIOtech":
                    return "XIOtech Corporation";
                case "XIRANET":
                    return "Xiranet Communications GmbH";
                case "XYRATEX":
                    return "Xyratex";
                case "YINHE":
                    return "NUDT Computer Co.";
                case "YIXUN":
                    return "Yixun Electronic Co.,Ltd.";
                case "YOTTA":
                    return "YottaYotta, Inc.";
                case "Zarva":
                    return "Zarva Digital Technology Co., Ltd.";
                case "ZETTA":
                    return "Zetta Systems, Inc.";
                default:
                    return SCSIVendorString;
            }
        }

        #endregion Private methods

        #region Public methods

        public static SCSIInquiry? DecodeSCSIInquiry(byte[] SCSIInquiryResponse)
        {
            if (SCSIInquiryResponse == null)
                return null;

            if (SCSIInquiryResponse.Length < 36)
            {
                //if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (SCSI INQUIRY Decoder): INQUIRY response is less than minimum of 36 bytes, decoded data can be incorrect, proceeding anyway.");
                //else
                    return null;
            }

            if (SCSIInquiryResponse.Length != SCSIInquiryResponse[4] + 5)
            {
                //if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (SCSI INQUIRY Decoder): INQUIRY response length ({0} bytes) is different than specified in length field ({1} bytes), decoded data can be incorrect, proceeding anyway.", SCSIInquiryResponse.Length, SCSIInquiryResponse[4] + 4);
                //else
                    return null;
            }

            SCSIInquiry decoded = new SCSIInquiry();

            if (SCSIInquiryResponse.Length >= 1)
            {
                decoded.PeripheralQualifier = (byte)((SCSIInquiryResponse[0] & 0xE0) >> 5);
                decoded.PeripheralDeviceType = (byte)(SCSIInquiryResponse[0] & 0x1F);
            }
            if (SCSIInquiryResponse.Length >= 2)
            {
                decoded.RMB = Convert.ToBoolean((SCSIInquiryResponse[1] & 0x80));
                decoded.DeviceTypeModifier = (byte)(SCSIInquiryResponse[1] & 0x7F);
            }
            if (SCSIInquiryResponse.Length >= 3)
            {
                decoded.ISOVersion = (byte)((SCSIInquiryResponse[2] & 0xC0) >> 6);
                decoded.ECMAVersion = (byte)((SCSIInquiryResponse[2] & 0x38) >> 3);
                decoded.ANSIVersion = (byte)(SCSIInquiryResponse[2] & 0x07);
            }
            if (SCSIInquiryResponse.Length >= 4)
            {
                decoded.AERC = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x80));
                decoded.TrmTsk = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x40));
                decoded.NormACA = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x20));
                decoded.HiSup = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x10));
                decoded.ResponseDataFormat = (byte)(SCSIInquiryResponse[3] & 0x07);
            }
            if (SCSIInquiryResponse.Length >= 5)
                decoded.AdditionalLength = SCSIInquiryResponse[4];
            if (SCSIInquiryResponse.Length >= 6)
            {
                decoded.SCCS = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x80));
                decoded.ACC = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x40));
                decoded.TPGS = (byte)((SCSIInquiryResponse[5] & 0x30) >> 4);
                decoded.ThreePC = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x08));
                decoded.Reserved2 = (byte)((SCSIInquiryResponse[5] & 0x06) >> 1);
                decoded.Protect = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 7)
            {
                decoded.BQue = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x80));
                decoded.EncServ = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x40));
                decoded.VS1 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x20));
                decoded.MultiP = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x10));
                decoded.MChngr = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x08));
                decoded.ACKREQQ = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x04));
                decoded.Addr32 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x02));
                decoded.Addr16 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 8)
            {
                decoded.RelAddr = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x80));
                decoded.WBus32 = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x40));
                decoded.WBus16 = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x20));
                decoded.Sync = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x10));
                decoded.Linked = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x08));
                decoded.TranDis = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x04));
                decoded.CmdQue = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x02));
                decoded.SftRe = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 16)
            {
                decoded.VendorIdentification = new byte[8];
                Array.Copy(SCSIInquiryResponse, 8, decoded.VendorIdentification, 0, 8);
            }
            if (SCSIInquiryResponse.Length >= 32)
            {
                decoded.ProductIdentification = new byte[16];
                Array.Copy(SCSIInquiryResponse, 16, decoded.ProductIdentification, 0, 16);
            }
            if (SCSIInquiryResponse.Length >= 36)
            {
                decoded.ProductRevisionLevel = new byte[4];
                Array.Copy(SCSIInquiryResponse, 32, decoded.ProductRevisionLevel, 0, 4);
            }
            if (SCSIInquiryResponse.Length >= 56)
            {
                decoded.VendorSpecific = new byte[20];
                Array.Copy(SCSIInquiryResponse, 36, decoded.VendorSpecific, 0, 20);
            }
            if (SCSIInquiryResponse.Length >= 57)
            {
                decoded.Reserved3 = (byte)((SCSIInquiryResponse[56] & 0xF0) >> 4);
                decoded.Clocking = (byte)((SCSIInquiryResponse[56] & 0x0C) >> 2);
                decoded.QAS = Convert.ToBoolean((SCSIInquiryResponse[56] & 0x02));
                decoded.IUS = Convert.ToBoolean((SCSIInquiryResponse[56] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 58)
                decoded.Reserved4 = SCSIInquiryResponse[57];
            if (SCSIInquiryResponse.Length >= 74)
            {
                decoded.VersionDescriptors = new ushort[8];
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                for (int i = 0; i < 8; i++)
                {
                    decoded.VersionDescriptors[i] = BigEndianBitConverter.ToUInt16(SCSIInquiryResponse, 58 + (i * 2));
                }
            }
            if (SCSIInquiryResponse.Length >= 75 && SCSIInquiryResponse.Length < 96)
            {
                decoded.Reserved5 = new byte[SCSIInquiryResponse.Length - 74];
                Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, SCSIInquiryResponse.Length - 74);
            }
            if (SCSIInquiryResponse.Length >= 96)
            {
                decoded.Reserved5 = new byte[22];
                Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, 22);
            }
            if (SCSIInquiryResponse.Length > 96)
            {
                decoded.VendorSpecific2 = new byte[SCSIInquiryResponse.Length - 96];
                Array.Copy(SCSIInquiryResponse, 96, decoded.Reserved5, 0, SCSIInquiryResponse.Length - 96);
            }

            return decoded;
        }

        public static string PrettifySCSIInquiry(SCSIInquiry? SCSIInquiryResponse)
        {
            if (SCSIInquiryResponse == null)
                return null;

            SCSIInquiry response = SCSIInquiryResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Device vendor: {0}", PrettifySCSIVendorString(StringHandlers.SpacePaddedToString(response.VendorIdentification))).AppendLine();
            sb.AppendFormat("Device name: {0}", StringHandlers.SpacePaddedToString(response.ProductIdentification)).AppendLine();
            sb.AppendFormat("Device release level: {0}", StringHandlers.SpacePaddedToString(response.ProductRevisionLevel)).AppendLine();
            switch ((SCSIPeripheralQualifiers)response.PeripheralQualifier)
            {
                case SCSIPeripheralQualifiers.SCSIPQSupported:
                    sb.AppendLine("Device is connected and supported.");
                    break;
                case SCSIPeripheralQualifiers.SCSIPQUnconnected:
                    sb.AppendLine("Device is supported but not connected.");
                    break;
                case SCSIPeripheralQualifiers.SCSIPQReserved:
                    sb.AppendLine("Reserved value set in Peripheral Qualifier field.");
                    break;
                case SCSIPeripheralQualifiers.SCSIPQUnsupported:
                    sb.AppendLine("Device is connected but unsupported.");
                    break;
                default:
                    sb.AppendFormat("Vendor value {0} set in Peripheral Qualifier field.", response.PeripheralQualifier).AppendLine();
                    break;
            }

            switch ((SCSIPeripheralDeviceTypes)response.PeripheralDeviceType)
            {
                case SCSIPeripheralDeviceTypes.SCSIPDTDirectAccess: //0x00,
                    sb.AppendLine("Direct-access device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTSequentialAccess: //0x01,
                    sb.AppendLine("Sequential-access device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTPrinterDevice: //0x02,
                    sb.AppendLine("Printer device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTProcessorDevice: //0x03,
                    sb.AppendLine("Processor device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTWriteOnceDevice: //0x04,
                    sb.AppendLine("Write-once device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTMultiMediaDevice: //0x05,
                    sb.AppendLine("CD-ROM/DVD/etc device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTScannerDevice: //0x06,
                    sb.AppendLine("Scanner device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTOpticalDevice: //0x07,
                    sb.AppendLine("Optical memory device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTMediumChangerDevice: //0x08,
                    sb.AppendLine("Medium change device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTCommsDevice: //0x09,
                    sb.AppendLine("Communications device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTPrePressDevice1: //0x0A,
                    sb.AppendLine("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTPrePressDevice2: //0x0B,
                    sb.AppendLine("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTArrayControllerDevice: //0x0C,
                    sb.AppendLine("Array controller device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTEnclosureServiceDevice: //0x0D,
                    sb.AppendLine("Enclosure services device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTSimplifiedDevice: //0x0E,
                    sb.AppendLine("Simplified direct-access device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTOCRWDevice: //0x0F,
                    sb.AppendLine("Optical card reader/writer device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTBridgingExpander: //0x10,
                    sb.AppendLine("Bridging Expanders");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTObjectDevice: //0x11,
                    sb.AppendLine("Object-based Storage Device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTADCDevice: //0x12,
                    sb.AppendLine("Automation/Drive Interface");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTWellKnownDevice: //0x1E,
                    sb.AppendLine("Well known logical unit");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTUnknownDevice: //0x1F
                    sb.AppendLine("Unknown or no device type");
                    break;
                default:
                    sb.AppendFormat("Unknown device type field value 0x{0:X2}", response.PeripheralDeviceType).AppendLine();
                    break;
            }

            switch ((SCSIANSIVersions)response.ANSIVersion)
            {
                case SCSIANSIVersions.SCSIANSINoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ANSI standard");
                    break;
                case SCSIANSIVersions.SCSIANSI1986Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.131:1986");
                    break;
                case SCSIANSIVersions.SCSIANSI1994Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.131:1994");
                    break;
                case SCSIANSIVersions.SCSIANSI1997Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.301:1997");
                    break;
                case SCSIANSIVersions.SCSIANSI2001Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.351:2001");
                    break;
                case SCSIANSIVersions.SCSIANSI2005Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.???:2005");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ANSI standard value 0x{0:X2})", response.ANSIVersion).AppendLine();
                    break;
            }

            switch ((SCSIECMAVersions)response.ECMAVersion)
            {
                case SCSIECMAVersions.SCSIECMANoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ECMA standard");
                    break;
                case SCSIECMAVersions.SCSIECMAObsolete:
                    sb.AppendLine("Device claims to comply with an obsolete SCSI ECMA standard");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ECMA standard value 0x{0:X2})", response.ECMAVersion).AppendLine();
                    break;
            }

            switch ((SCSIISOVersions)response.ISOVersion)
            {
                case SCSIISOVersions.SCSIISONoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ISO/IEC standard");
                    break;
                case SCSIISOVersions.SCSIISO1995Version:
                    sb.AppendLine("Device claims to comply with ISO/IEC 9316:1995");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ISO/IEC standard value 0x{0:X2})", response.ISOVersion).AppendLine();
                    break;
            }

            if (response.RMB)
                sb.AppendLine("Device is removable");
            if (response.AERC)
                sb.AppendLine("Device supports Asynchronous Event Reporting Capability");
            if (response.TrmTsk)
                sb.AppendLine("Device supports TERMINATE TASK command");
            if (response.NormACA)
                sb.AppendLine("Device supports setting Normal ACA");
            if (response.HiSup)
                sb.AppendLine("Device supports LUN hierarchical addressing");
            if (response.SCCS)
                sb.AppendLine("Device contains an embedded storage array controller");
            if (response.ACC)
                sb.AppendLine("Device contains an Access Control Coordinator");
            if (response.ThreePC)
                sb.AppendLine("Device supports third-party copy commands");
            if (response.Protect)
                sb.AppendLine("Device supports protection information");
            if (response.BQue)
                sb.AppendLine("Device supports basic queueing");
            if (response.EncServ)
                sb.AppendLine("Device contains an embedded enclosure services component");
            if (response.MultiP)
                sb.AppendLine("Multi-port device");
            if (response.MChngr)
                sb.AppendLine("Device contains or is attached to a medium changer");
            if (response.ACKREQQ)
                sb.AppendLine("Device supports request and acknowledge handshakes");
            if (response.Addr32)
                sb.AppendLine("Device supports 32-bit wide SCSI addresses");
            if (response.Addr16)
                sb.AppendLine("Device supports 16-bit wide SCSI addresses");
            if (response.RelAddr)
                sb.AppendLine("Device supports relative addressing");
            if (response.WBus32)
                sb.AppendLine("Device supports 32-bit wide data transfers");
            if (response.WBus16)
                sb.AppendLine("Device supports 16-bit wide data transfers");
            if (response.Sync)
                sb.AppendLine("Device supports synchronous data transfer");
            if (response.Linked)
                sb.AppendLine("Device supports linked commands");
            if (response.TranDis)
                sb.AppendLine("Device supports CONTINUE TASK and TARGET TRANSFER DISABLE commands");
            if (response.QAS)
                sb.AppendLine("Device supports Quick Arbitration and Selection");
            if (response.CmdQue)
                sb.AppendLine("Device supports TCQ queue");
            if (response.IUS)
                sb.AppendLine("Device supports information unit transfers");
            if (response.SftRe)
                sb.AppendLine("Device implements RESET as a soft reset");
            //if (MainClass.isDebug)
            {
                if (response.VS1)
                    sb.AppendLine("Vendor specific bit 5 on byte 6 of INQUIRY response is set");
            }

            switch ((SCSITGPSValues)response.TPGS)
            {
                case SCSITGPSValues.NotSupported:
                    sb.AppendLine("Device does not support assymetrical access");
                    break;
                case SCSITGPSValues.OnlyImplicit:
                    sb.AppendLine("Device only supports implicit assymetrical access");
                    break;
                case SCSITGPSValues.OnlyExplicit:
                    sb.AppendLine("Device only supports explicit assymetrical access");
                    break;
                case SCSITGPSValues.Both:
                    sb.AppendLine("Device supports implicit and explicit assymetrical access");
                    break;
                default:
                    sb.AppendFormat("Unknown value in TPGS field 0x{0:X2}", response.TPGS).AppendLine();
                    break;
            }

            switch ((SCSISPIClocking)response.Clocking)
            {
                case SCSISPIClocking.SCSIClockingST:
                    sb.AppendLine("Device supports only ST clocking");
                    break;
                case SCSISPIClocking.SCSIClockingDT:
                    sb.AppendLine("Device supports only DT clocking");
                    break;
                case SCSISPIClocking.SCSIClockingReserved:
                    sb.AppendLine("Reserved value 0x02 found in SPI clocking field");
                    break;
                case SCSISPIClocking.SCSIClockingSTandDT:
                    sb.AppendLine("Device supports ST and DT clocking");
                    break;
                default:
                    sb.AppendFormat("Unknown value in SPI clocking field 0x{0:X2}", response.Clocking).AppendLine();
                    break;
            }

            foreach (UInt16 VersionDescriptor in response.VersionDescriptors)
            {
                switch (VersionDescriptor & 0xFFE0)
                {
                    case (int)SCSIVersionDescriptorStandardMask.NoStandard: //0x0000
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                //sb.AppendLine("Device claims no standard");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of no standard", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAM: //0x0020
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSAMMask.NoVersion:
                                sb.AppendLine("Device complies with standard SAM, no version");
                                break;
                            case (int)SCSIVersionDescriptorSAMMask.T10_0994_r18:
                                sb.AppendLine("Device complies with T10/0994 revision 18 (SAM)");
                                break;
                            case (int)SCSIVersionDescriptorSAMMask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.270:1996 (SAM)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAM", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAM2: //0x0040
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSAM2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SAM-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSAM2Mask.T10_1157_r23:
                                sb.AppendLine("Device complies with T10/1157-D revision 23 (SAM-2)");
                                break;
                            case (int)SCSIVersionDescriptorSAM2Mask.T10_1157_r24:
                                sb.AppendLine("Device complies with T10/1157-D revision 24 (SAM-2)");
                                break;
                            case (int)SCSIVersionDescriptorSAM2Mask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 366-2003 (SAM-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAM-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAM3: //0x0060
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSAM3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SAM-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorSAM3Mask.T10_1561_r07:
                                sb.AppendLine("Device complies with T10/1561-D revision 7 (SAM-3)");
                                break;
                            case (int)SCSIVersionDescriptorSAM3Mask.T10_1561_r13:
                                sb.AppendLine("Device complies with T10/1561-D revision 13 (SAM-3)");
                                break;
                            case (int)SCSIVersionDescriptorSAM3Mask.T10_1561_r14:
                                sb.AppendLine("Device complies with T10/1157-D revision 14 (SAM-3)");
                                break;
                            case (int)SCSIVersionDescriptorSAM3Mask.ANSI_2005:
                                sb.AppendLine("Device complies with ANSI INCITS 402-2005 (SAM-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAM-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAM4: //0x0080
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard SAM-4, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAM-4", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPC: //0x0120
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPCMask.NoVersion:
                                sb.AppendLine("Device complies with standard SPC, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPCMask.T10_0995_r11a:
                                sb.AppendLine("Device complies with T10/0995 revision 11a (SPC)");
                                break;
                            case (int)SCSIVersionDescriptorSPCMask.ANSI_1997:
                                sb.AppendLine("Device complies with ANSI X3.301:1997 (SPC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.MMC: //0x0140
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorMMCMask.NoVersion:
                                sb.AppendLine("Device complies with standard MMC, no version");
                                break;
                            case (int)SCSIVersionDescriptorMMCMask.T10_1048_r10a:
                                sb.AppendLine("Device complies with T10/1048 revision 10a (MMC)");
                                break;
                            case (int)SCSIVersionDescriptorMMCMask.ANSI_1997:
                                sb.AppendLine("Device complies with ANSI X3.304:1997 (MMC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard MMC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SCC: //0x0160
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSCCMask.NoVersion:
                                sb.AppendLine("Device complies with standard SCC, no version");
                                break;
                            case (int)SCSIVersionDescriptorSCCMask.T10_1048_r06c:
                                sb.AppendLine("Device complies with T10/1047 revision 06c (SCC)");
                                break;
                            case (int)SCSIVersionDescriptorSCCMask.ANSI_1997:
                                sb.AppendLine("Device complies with ANSI X3.276:1997 (SCC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SCC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SBC: //0x0180
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSBCMask.NoVersion:
                                sb.AppendLine("Device complies with standard SBC, no version");
                                break;
                            case (int)SCSIVersionDescriptorSBCMask.T10_0996_r08c:
                                sb.AppendLine("Device complies with T10/0996 revision 08c (SBC)");
                                break;
                            case (int)SCSIVersionDescriptorSBCMask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.306:1998 (SBC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SBC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SMC: //0x01A0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSMCMask.NoVersion:
                                sb.AppendLine("Device complies with standard SMC, no version");
                                break;
                            case (int)SCSIVersionDescriptorSMCMask.T10_0999_r10a:
                                sb.AppendLine("Device complies with T10/0999 revision 10a (SMC)");
                                break;
                            case (int)SCSIVersionDescriptorSMCMask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.314:1998 (SMC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SMC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SES: //0x01C0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSESMask.NoVersion:
                                sb.AppendLine("Device complies with standard SES, no version");
                                break;
                            case (int)SCSIVersionDescriptorSESMask.T10_1212_r08b:
                                sb.AppendLine("Device complies with T10/1212 revision 08b (SES)");
                                break;
                            case (int)SCSIVersionDescriptorSESMask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.305:1998 (SES)");
                                break;
                            case (int)SCSIVersionDescriptorSESMask.T10_1212_r08b_2000:
                                sb.AppendLine("Device complies with T10/1212 revision 08b with ANSI NCITS.305/AM1:2000 (SES)");
                                break;
                            case (int)SCSIVersionDescriptorSESMask.ANSI_1998_2000:
                                sb.AppendLine("Device complies with ANSI NCITS.305:1998 with ANSI NCITS.305/AM1:2000 (SES)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SES", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SCC2: //0x01E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSCC2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SCC-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSCC2Mask.T10_1125_r04:
                                sb.AppendLine("Device complies with T10/1125 revision 04 (SCC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSCC2Mask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.318:1998 (SCC-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SCC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSC: //0x0200
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSCMask.NoVersion:
                                sb.AppendLine("Device complies with standard SSC, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSCMask.T10_0997_r17:
                                sb.AppendLine("Device complies with T10/0997 revision 17 (SSC)");
                                break;
                            case (int)SCSIVersionDescriptorSSCMask.T10_0997_r22:
                                sb.AppendLine("Device complies with T10/0997 revision 22 (SSC)");
                                break;
                            case (int)SCSIVersionDescriptorSSCMask.ANSI_2000:
                                sb.AppendLine("Device complies with ANSI NCITS.335:2000 (SSC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.RBC: //0x0220
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorRBCMask.NoVersion:
                                sb.AppendLine("Device complies with standard RBC, no version");
                                break;
                            case (int)SCSIVersionDescriptorRBCMask.T10_1240_r10a:
                                sb.AppendLine("Device complies with T10/1240 revision 10a (RBC)");
                                break;
                            case (int)SCSIVersionDescriptorRBCMask.ANSI_2000:
                                sb.AppendLine("Device complies with ANSI NCITS.330:2000 (RBC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard RBC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.MMC2: //0x0240
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorMMC2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard MMC-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorMMC2Mask.T10_1228_r11:
                                sb.AppendLine("Device complies with T10/1228 revision 11 (MMC-2)");
                                break;
                            case (int)SCSIVersionDescriptorMMC2Mask.T10_1228_r11a:
                                sb.AppendLine("Device complies with T10/1228 revision 11a (MMC-2)");
                                break;
                            case (int)SCSIVersionDescriptorMMC2Mask.ANSI_2000:
                                sb.AppendLine("Device complies with ANSI NCITS.333:2000 (MMC-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard MMC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPC2: //0x0260
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPC2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SPC-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPC2Mask.T10_1236_r12:
                                sb.AppendLine("Device complies with T10/1236 revision 12 (SPC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSPC2Mask.T10_1236_r18:
                                sb.AppendLine("Device complies with T10/1236 revision 18 (SPC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSPC2Mask.T10_1236_r19:
                                sb.AppendLine("Device complies with T10/1236 revision 19 (SPC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSPC2Mask.T10_1236_r20:
                                sb.AppendLine("Device complies with T10/1236 revision 20 (SPC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSPC2Mask.ANSI_2001:
                                sb.AppendLine("Device complies with ANSI INCITS 351-2001 (SPC-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.OCRW: //0x0280
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorOCRWMask.NoVersion:
                                sb.AppendLine("Device complies with standard OCRW, no version");
                                break;
                            case (int)SCSIVersionDescriptorOCRWMask.ISO14776_381:
                                sb.AppendLine("Device complies with ISO/IEC 14776-381 (OCRW)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard OCRW", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.MMC3: //0x02A0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorMMC3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard MMC-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorMMC3Mask.T10_1363_r09:
                                sb.AppendLine("Device complies with T10/1363-D revision 9 (MMC-3)");
                                break;
                            case (int)SCSIVersionDescriptorMMC3Mask.T10_1363_r10g:
                                sb.AppendLine("Device complies with T10/1363-D revision 10g (MMC-3)");
                                break;
                            case (int)SCSIVersionDescriptorMMC3Mask.ANSI_2001:
                                sb.AppendLine("Device complies with ANSI INCITS 360-2002 (MMC-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard MMC-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.RMC: //0x02C0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard ");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SMC2: //0x02E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSMC2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SMC-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSMC2Mask.T10_1383_r05:
                                sb.AppendLine("Device complies with T10/1383-D revision 5 (SMC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSMC2Mask.T10_1383_r06:
                                sb.AppendLine("Device complies with T10/1383-D revision 6 (SMC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSMC2Mask.T10_1383_r07:
                                sb.AppendLine("Device complies with T10/1383-D revision 7 (SMC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSMC2Mask.ANSI_2004:
                                sb.AppendLine("Device complies with ANSI_2004 (SMC-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SMC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPC3: //0x0300
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPC3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SPC-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPC3Mask.T10_1416_r07:
                                sb.AppendLine("Device complies with T10/1416-D revision 7 (SPC-3)");
                                break;
                            case (int)SCSIVersionDescriptorSPC3Mask.T10_1416_r21:
                                sb.AppendLine("Device complies with T10/1416-D revision 21 (SPC-3)");
                                break;
                            case (int)SCSIVersionDescriptorSPC3Mask.T10_1416_r22:
                                sb.AppendLine("Device complies with T10/1416-D revision 22 (SPC-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPC-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SBC2: //0x0320
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSBC2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SBC-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSBC2Mask.T10_1383_r05:
                                sb.AppendLine("Device complies with T10/1383 revision 5 (SBC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSBC2Mask.T10_1383_r06:
                                sb.AppendLine("Device complies with T10/1383 revision 6 (SBC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSBC2Mask.T10_1383_r07:
                                sb.AppendLine("Device complies with T10/1383 revision 7 (SBC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSBC2Mask.ANSI_2005:
                                sb.AppendLine("Device complies with ANSI INCITS 405-2005 (SBC-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SBC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.OSD: //0x0340
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorOSDMask.NoVersion:
                                sb.AppendLine("Device complies with standard OSD, no version");
                                break;
                            case (int)SCSIVersionDescriptorOSDMask.T10_1355_r0:
                                sb.AppendLine("Device complies with T10/1355 revision 0 (OSD)");
                                break;
                            case (int)SCSIVersionDescriptorOSDMask.T10_1355_r7a:
                                sb.AppendLine("Device complies with T10/1355 revision 7a (OSD)");
                                break;
                            case (int)SCSIVersionDescriptorOSDMask.T10_1355_r8:
                                sb.AppendLine("Device complies with T10/1355 revision 8 (OSD)");
                                break;
                            case (int)SCSIVersionDescriptorOSDMask.T10_1355_r9:
                                sb.AppendLine("Device complies with T10/1355 revision 9 (OSD)");
                                break;
                            case (int)SCSIVersionDescriptorOSDMask.T10_1355_r10:
                                sb.AppendLine("Device complies with T10/1355 revision 10 (OSD)");
                                break;
                            case (int)SCSIVersionDescriptorOSDMask.ANSI_2004:
                                sb.AppendLine("Device complies with ANSI INCITS 400-2004 (OSD)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard OSD", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSC2: //0x0360
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSC2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SSC-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSC2Mask.T10_1434_r07:
                                sb.AppendLine("Device complies with T10/1434-D revision 7 (SSC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSSC2Mask.T10_1434_r09:
                                sb.AppendLine("Device complies with T10/1434-D revision 9 (SSC-2)");
                                break;
                            case (int)SCSIVersionDescriptorSSC2Mask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 380-2003 (SSC-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.BCC: //0x0380
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard BCC, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard BCC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.MMC4: //0x03A0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorMMC4Mask.NoVersion:
                                sb.AppendLine("Device complies with standard MMC-4, no version");
                                break;
                            case (int)SCSIVersionDescriptorMMC4Mask.T10_1545_r5:
                                sb.AppendLine("Device complies with T10/1545-D revision 5 (MMC-4)");
                                break;
                            case (int)SCSIVersionDescriptorMMC4Mask.T10_1545_r3:
                                sb.AppendLine("Device complies with T10/1545-D revision 3 (MMC-4)");
                                break;
                            case (int)SCSIVersionDescriptorMMC4Mask.T10_1545_r3d:
                                sb.AppendLine("Device complies with T10/1545-D revision 3d (MMC-4)");
                                break;
                            case (int)SCSIVersionDescriptorMMC4Mask.ANSI_2005:
                                sb.AppendLine("Device complies with ANSI INCITS 401-2005 (MMC-4)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard MMC-4", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ADC: //0x03C0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorADCMask.NoVersion:
                                sb.AppendLine("Device complies with standard ADC, no version");
                                break;
                            case (int)SCSIVersionDescriptorADCMask.T10_1558_r6:
                                sb.AppendLine("Device complies with T10/1558-D revision 6 (ADC)");
                                break;
                            case (int)SCSIVersionDescriptorADCMask.T10_1558_r7:
                                sb.AppendLine("Device complies with T10/1558-D revision 7 (ADC)");
                                break;
                            case (int)SCSIVersionDescriptorADCMask.ANSI_2005:
                                sb.AppendLine("Device complies with ANSI INCITS 403-2005 (ADC)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ADC", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SES2: //0x03E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard SES-2, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SES-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSC3: //0x0400
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard SSC-3, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSC-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.MMC5: //0x0420
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard MMC-5, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard MMC-5", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.OSD2: //0x0440
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard OSD-2, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard OSD-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPC4: //0x0460
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard SPC-4, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPC-4", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SMC3: //0x0480
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard SMC-3, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SMC-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ADC2: //0x04A0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard ADC-2, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ADC-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSA_TL2: //0x0820
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSA_TL2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SSA-TL2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSA_TL2Mask.T10_1147_r05b:
                                sb.AppendLine("Device complies with T10.1/1147 revision 05b (SSA-TL2)");
                                break;
                            case (int)SCSIVersionDescriptorSSA_TL2Mask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.308:1998 (SSA-TL2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSA-TL2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSA_TL1: //0x0840
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSA_TL1Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SSA-TL1, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSA_TL1Mask.T10_0989_r10b:
                                sb.AppendLine("Device complies with T10.1/0989 revision 10b (SSA-TL1)");
                                break;
                            case (int)SCSIVersionDescriptorSSA_TL1Mask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.295:1996 (SSA-TL1)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSA-TL1", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSA_S3P: //0x0860
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSA_S3PMask.NoVersion:
                                sb.AppendLine("Device complies with standard SSA-S3P, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSA_S3PMask.T10_1051_r05b:
                                sb.AppendLine("Device complies with T10.1/1051 revision 05b (SSA-S3P)");
                                break;
                            case (int)SCSIVersionDescriptorSSA_S3PMask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.309:1998 (SSA-S3P)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSA-S3P", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSA_S2P: //0x0880
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSA_S2PMask.NoVersion:
                                sb.AppendLine("Device complies with standard SSA-S2P, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSA_S2PMask.T10_1121_r07b:
                                sb.AppendLine("Device complies with T10.1/1121 revision 07b (SSA-S2P)");
                                break;
                            case (int)SCSIVersionDescriptorSSA_S2PMask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.294:1996 (SSA-S2P)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSA-S2P", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SIP: //0x08A0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSIPMask.NoVersion:
                                sb.AppendLine("Device complies with standard SIP, no version");
                                break;
                            case (int)SCSIVersionDescriptorSIPMask.T10_0856_r10:
                                sb.AppendLine("Device complies with T10/0856 revision 10 (SIP)");
                                break;
                            case (int)SCSIVersionDescriptorSIPMask.ANSI_1997:
                                sb.AppendLine("Device complies with ANSI X3.292:1997 (SIP)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SIP", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FCP: //0x08C0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCPMask.NoVersion:
                                sb.AppendLine("Device complies with standard FCP, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCPMask.T10_0993_r12:
                                sb.AppendLine("Device complies with T10/0993 revision 12 (FCP)");
                                break;
                            case (int)SCSIVersionDescriptorFCPMask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.269:1996 (FCP)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FCP", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SBP2: //0x08E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSBP2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SBP-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSBP2Mask.T10_1155_r04:
                                sb.AppendLine("Device complies with T10/1155 revision 04 (SBP-2)");
                                break;
                            case (int)SCSIVersionDescriptorSBP2Mask.ANSI_1999:
                                sb.AppendLine("Device complies with ANSI NCITS.325:1999 (SBP-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SBP-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FCP2: //0x0900
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCP2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard FCP-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCP2Mask.T10_1144_r4:
                                sb.AppendLine("Device complies with T10/1144-D revision 4 (FCP-2)");
                                break;
                            case (int)SCSIVersionDescriptorFCP2Mask.T10_1144_r7:
                                sb.AppendLine("Device complies with T10/1144-D revision 7 (FCP-2)");
                                break;
                            case (int)SCSIVersionDescriptorFCP2Mask.T10_1144_r7a:
                                sb.AppendLine("Device complies with T10/1144-D revision 7a (FCP-2)");
                                break;
                            case (int)SCSIVersionDescriptorFCP2Mask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 350-2003 (FCP-2)");
                                break;
                            case (int)SCSIVersionDescriptorFCP2Mask.T10_1144_r8:
                                sb.AppendLine("Device complies with T10/1144-D revision 8 (FCP-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FCP-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SST: //0x0920
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSTMask.NoVersion:
                                sb.AppendLine("Device complies with standard SST, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSTMask.T10_1380_r8b:
                                sb.AppendLine("Device complies with T10/1380-D revision 8b (SST)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SST", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SRP: //0x0940
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSRPMask.NoVersion:
                                sb.AppendLine("Device complies with standard SRP, no version");
                                break;
                            case (int)SCSIVersionDescriptorSRPMask.T10_1415_r10:
                                sb.AppendLine("Device complies with T10/1415-D revision 10 (SRP)");
                                break;
                            case (int)SCSIVersionDescriptorSRPMask.T10_1415_r16a:
                                sb.AppendLine("Device complies with T10/1415-D revision 16a (SRP)");
                                break;
                            case (int)SCSIVersionDescriptorSRPMask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 365-2002 (SRP)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SRP", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.iSCSI: //0x0960
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard iSCSI, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard iSCSI", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SBP3: //0x0980
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSBP3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SBP-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorSBP3Mask.T10_1467_r1f:
                                sb.AppendLine("Device complies with T10/1467-D revision 1f (SBP-3)");
                                break;
                            case (int)SCSIVersionDescriptorSBP3Mask.T10_1467_r3:
                                sb.AppendLine("Device complies with T10/1467-D revision 3 (SBP-3)");
                                break;
                            case (int)SCSIVersionDescriptorSBP3Mask.T10_1467_r4:
                                sb.AppendLine("Device complies with T10/1467-D revision 4 (SBP-3)");
                                break;
                            case (int)SCSIVersionDescriptorSBP3Mask.T10_1467_r5:
                                sb.AppendLine("Device complies with T10/1467-D revision 5 (SBP-3)");
                                break;
                            case (int)SCSIVersionDescriptorSBP3Mask.ANSI_2004:
                                sb.AppendLine("Device complies with ANSI INCITS 375-2004 (SBP-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SBP-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ADP: //0x09C0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard ADP, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ADP", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ADT: //0x09E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorADTMask.NoVersion:
                                sb.AppendLine("Device complies with standard ADT, no version");
                                break;
                            case (int)SCSIVersionDescriptorADTMask.T10_1557_r11:
                                sb.AppendLine("Device complies with T10/1557-D revision 11 (ADT)");
                                break;
                            case (int)SCSIVersionDescriptorADTMask.T10_1557_r14:
                                sb.AppendLine("Device complies with T10/1557-D revision 14 (ADT)");
                                break;
                            case (int)SCSIVersionDescriptorADTMask.ANSI_2005:
                                sb.AppendLine("Device complies with ANSI INCITS 406-2005 (ADT)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ADT", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FCP3: //0x0A00
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCP3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard FCP-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCP3Mask.T10_1560_r4:
                                sb.AppendLine("Device complies with T10/1560-D revision 4 (FCP-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FCP-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ADT2: //0x0A20
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard ADT-2, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ADT-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPI: //0x0AA0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPIMask.NoVersion:
                                sb.AppendLine("Device complies with standard SPI, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPIMask.T10_0855_r15a:
                                sb.AppendLine("Device complies with T10/0855 revision 15a (SPI)");
                                break;
                            case (int)SCSIVersionDescriptorSPIMask.ANSI_1995:
                                sb.AppendLine("Device complies with ANSI X3.253:1995 (SPI)");
                                break;
                            case (int)SCSIVersionDescriptorSPIMask.T10_0855_r15a_Amnd_3a:
                                sb.AppendLine("Device complies with T10/0855 revision 15a with SPI Amnd revision 3a (SPI)");
                                break;
                            case (int)SCSIVersionDescriptorSPIMask.ANSI_1995_1998:
                                sb.AppendLine("Device complies with ANSI X3.253:1995 with SPI Amnd ANSI X3.253/AM1:1998 (SPI)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPI", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.Fast20: //0xAC0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFast20Mask.NoVersion:
                                sb.AppendLine("Device complies with standard Fast-20, no version");
                                break;
                            case (int)SCSIVersionDescriptorFast20Mask.T10_1071_r06:
                                sb.AppendLine("Device complies with T10/1071 revision 06 (Fast-20)");
                                break;
                            case (int)SCSIVersionDescriptorFast20Mask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.277:1996 (Fast-20)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard Fast-20", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPI2: //0x0AE0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPI2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SPI-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPI2Mask.T10_1142_r20b:
                                sb.AppendLine("Device complies with T10/1142 revision 20b (SPI-2)");
                                break;
                            case (int)SCSIVersionDescriptorSPI2Mask.ANSI_1999:
                                sb.AppendLine("Device complies with ANSI X3.302:1999 (SPI-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPI-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPI3: //0x0B00
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPI3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SPI-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPI3Mask.T10_1302D_r10:
                                sb.AppendLine("Device complies with T10/1302-D revision 10 (SPI-3)");
                                break;
                            case (int)SCSIVersionDescriptorSPI3Mask.T10_1302D_r13a:
                                sb.AppendLine("Device complies with T10/1302-D revision 13a (SPI-3)");
                                break;
                            case (int)SCSIVersionDescriptorSPI3Mask.T10_1302D_r14:
                                sb.AppendLine("Device complies with T10/1302-D revision 14 (SPI-3)");
                                break;
                            case (int)SCSIVersionDescriptorSPI3Mask.ANSI_2000:
                                sb.AppendLine("Device complies with ANSI NCITS.336:2000 (SPI-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPI-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.EPI: //0x0B20
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorEPIMask.NoVersion:
                                sb.AppendLine("Device complies with standard EPI, no version");
                                break;
                            case (int)SCSIVersionDescriptorEPIMask.T10_1134_r16:
                                sb.AppendLine("Device complies with T10/1134 revision 16 (EPI)");
                                break;
                            case (int)SCSIVersionDescriptorEPIMask.ANSI_1999:
                                sb.AppendLine("Device complies with ANSI NCITS TR-23:1999 (EPI)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard EPI", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPI4: //0x0B40
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPI4Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SPI-4, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPI4Mask.T10_1365_r7:
                                sb.AppendLine("Device complies with T10/1365-D revision 7 (SPI-4)");
                                break;
                            case (int)SCSIVersionDescriptorSPI4Mask.T10_1365_r9:
                                sb.AppendLine("Device complies with T10/1365-D revision 9 (SPI-4)");
                                break;
                            case (int)SCSIVersionDescriptorSPI4Mask.ANSI_2002:
                                sb.AppendLine("Device complies with ANSI INCITS 362-2002 (SPI-4)");
                                break;
                            case (int)SCSIVersionDescriptorSPI4Mask.T10_1365_r10:
                                sb.AppendLine("Device complies with T10/1365-D revision 10 (SPI-4)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPI-4", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SPI5: //0x0B40
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSPI5Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SPI-5, no version");
                                break;
                            case (int)SCSIVersionDescriptorSPI5Mask.T10_1525_r3:
                                sb.AppendLine("Device complies with T10/1525-D revision 3 (SPI-5)");
                                break;
                            case (int)SCSIVersionDescriptorSPI5Mask.T10_1525_r5:
                                sb.AppendLine("Device complies with T10/1525-D revision 5 (SPI-5)");
                                break;
                            case (int)SCSIVersionDescriptorSPI5Mask.T10_1525_r6:
                                sb.AppendLine("Device complies with T10/1525-D revision 6 (SPI-5)");
                                break;
                            case (int)SCSIVersionDescriptorSPI5Mask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 367-2003 (SPI-5)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SPI-5", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAS: //0x0BE0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSASMask.NoVersion:
                                sb.AppendLine("Device complies with standard (SAS)");
                                break;
                            case (int)SCSIVersionDescriptorSASMask.T10_1562_r01:
                                sb.AppendLine("Device complies with T10/1562-D revision 01 (SAS)");
                                break;
                            case (int)SCSIVersionDescriptorSASMask.T10_1562_r03:
                                sb.AppendLine("Device complies with T10/1562-D revision 03 (SAS)");
                                break;
                            case (int)SCSIVersionDescriptorSASMask.T10_1562_r04:
                            case (int)SCSIVersionDescriptorSASMask.T10_1562_r04bis:
                                sb.AppendLine("Device complies with T10/1562-D revision 04 (SAS)");
                                break;
                            case (int)SCSIVersionDescriptorSASMask.T10_1562_r05:
                                sb.AppendLine("Device complies with T10/1562-D revision 05 (SAS)");
                                break;
                            case (int)SCSIVersionDescriptorSASMask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 376-2003 (SAS)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAS", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAS11: //0x0C00
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSAS11Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SAS 1.1, no version");
                                break;
                            case (int)SCSIVersionDescriptorSAS11Mask.T10_1601_r9:
                                sb.AppendLine("Device complies with T10/1601-D revision 9 (SAS 1.1)");
                                break;
                            case (int)SCSIVersionDescriptorSAS11Mask.T10_1601_r10:
                                sb.AppendLine("Device complies with T10/1601-D revision 10 (SAS 1.1)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAS 1.1", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_PH: //0x0D20
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCPHMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-PH, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCPHMask.ANSI_1994:
                                sb.AppendLine("Device complies with ANSI X3.230:1994 (FC-PH)");
                                break;
                            case (int)SCSIVersionDescriptorFCPHMask.ANSI_1994_1996:
                                sb.AppendLine("Device complies with ANSI X3.230:1994 with Amnd 1 ANSI X3.230/AM1:1996 (FC-PH)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-PH", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_AL: //0x0D40
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCALMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-AL, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCALMask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.272:1996 (FC-AL)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-AL", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_AL2: //0x0D60
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCAL2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-AL-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCAL2Mask.T11_1133_r70:
                                sb.AppendLine("Device complies with T11/1133 revision 7.0 (FC-AL-2)");
                                break;
                            case (int)SCSIVersionDescriptorFCAL2Mask.ANSI_1999:
                                sb.AppendLine("Device complies with ANSI NCITS.332:1999 (FC-AL-2)");
                                break;
                            case (int)SCSIVersionDescriptorFCAL2Mask.ANSI_1999_2002:
                                sb.AppendLine("Device complies with ANSI INCITS 332-1999 with Amnd 1 AM1-2002 (FC-AL-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-AL-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_PH3: //0x0D80
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCPH3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-PH-3, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCPH3Mask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI X3.303-1998 (FC-PH-3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-PH-3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_FS: //0x0DA0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCFSMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-FS, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCFSMask.T11_1331_r12:
                                sb.AppendLine("Device complies with T11/1331 revision 1.2 (FC-FS)");
                                break;
                            case (int)SCSIVersionDescriptorFCFSMask.T11_1331_r17:
                                sb.AppendLine("Device complies with T11/1331 revision 1.7 (FC-FS)");
                                break;
                            case (int)SCSIVersionDescriptorFCFSMask.ANSI_2003:
                                sb.AppendLine("Device complies with ANSI INCITS 373-2003 (FC-FS)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-FS", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_PI: //0x0DC0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCPIMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-PI, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCPIMask.ANSI_2002:
                                sb.AppendLine("Device complies with ANSI INCITS 352-2002 (FC-PI)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-PI", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_PI2: //0x0DE0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCPI2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-PI-2, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCPI2Mask.T11_1506_r50:
                                sb.AppendLine("Device complies with T11/1506-D revision 5.0 (FC-PI-2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-PI-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_FS2: //0x0E00
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard FC-FS-2, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-FS-2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_LS: //0x0E20
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard FC-LS, no version");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-LS", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_SP: //0x0E40
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCSPMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-SP, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCSPMask.T11_1570_r16:
                                sb.AppendLine("Device complies with T11/1570-D revision 1.6 (FC-SP)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-SP", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_DA: //0x12E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCDAMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-DA, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCDAMask.T11_1513_r31:
                                sb.AppendLine("Device complies with T11/1513-DT revision 3.1 (FC-DA)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-DA", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_Tape: //0x1300
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCTapeMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-Tape, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCTapeMask.T11_1315_r116:
                                sb.AppendLine("Device complies with T11/1315 revision 1.16 (FC-Tape)");
                                break;
                            case (int)SCSIVersionDescriptorFCTapeMask.T11_1315_r117:
                                sb.AppendLine("Device complies with T11/1315 revision 1.17 (FC-Tape)");
                                break;
                            case (int)SCSIVersionDescriptorFCTapeMask.ANSI_1999:
                                sb.AppendLine("Device complies with ANSI NCITS TR-24:1999 (FC-Tape)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-Tape", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_FLA: //0x1320
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCFLAMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-FLA, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCFLAMask.T11_1235_r7:
                                sb.AppendLine("Device complies with T11/1235 revision 7 (FC-FLA)");
                                break;
                            case (int)SCSIVersionDescriptorFCFLAMask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS TR-20:1998 (FC-FLA)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-FLA", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.FC_PLDA: //0x1340
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorFCPLDAMask.NoVersion:
                                sb.AppendLine("Device complies with standard FC-PLDA, no version");
                                break;
                            case (int)SCSIVersionDescriptorFCPLDAMask.T11_1162_r21:
                                sb.AppendLine("Device complies with T11/1162 revision 2.1 (FC-PLDA)");
                                break;
                            case (int)SCSIVersionDescriptorFCPLDAMask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS TR-19:1998 (FC-PLDA)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard FC-PLDA", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSA_PH2: //0x1360
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSAPH2Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SSA-PH2, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSAPH2Mask.T10_1145_r09c:
                                sb.AppendLine("Device complies with T10.1/1145 revision 09c (SSA-PH2)");
                                break;
                            case (int)SCSIVersionDescriptorSSAPH2Mask.ANSI_1996:
                                sb.AppendLine("Device complies with ANSI X3.293:1996 (SSA-PH2)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSA-PH2", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SSA_PH3: //0x1380
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorSSAPH3Mask.NoVersion:
                                sb.AppendLine("Device complies with standard SSA-PH3, no version");
                                break;
                            case (int)SCSIVersionDescriptorSSAPH3Mask.T10_1146_r05b:
                                sb.AppendLine("Device complies with T10.1/1146 revision 05b (SSA-PH3)");
                                break;
                            case (int)SCSIVersionDescriptorSSAPH3Mask.ANSI_1998:
                                sb.AppendLine("Device complies with ANSI NCITS.307:1998 (SSA-PH3)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SSA-PH3", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.IEEE1394: //0x14A0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorIEEE1394Mask.NoVersion:
                                sb.AppendLine("Device complies with standard IEEE-1394, no version");
                                break;
                            case (int)SCSIVersionDescriptorIEEE1394Mask.ANSI_1995:
                                sb.AppendLine("Device complies with ANSI IEEE 1394:1995");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard IEEE-1394", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.IEEE1394a: //0x14C0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard IEEE-1394a");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard IEEE-1394a", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.IEEE1394b: //0x14E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard IEEE-1394b");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard IEEE-1394b", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ATA_ATAPI6: //0x15E0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorATA6Mask.NoVersion:
                                sb.AppendLine("Device complies with standard ATA/ATAPI-6, no version");
                                break;
                            case (int)SCSIVersionDescriptorATA6Mask.ANSI_2002:
                                sb.AppendLine("Device complies with ANSI INCITS 361-2002 (ATA/ATAPI-6)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ATA/ATAPI-6)", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ATA_ATAPI7: //0x1600
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorATA7Mask.NoVersion:
                                sb.AppendLine("Device complies with standard ATA/ATAPI-7, no version");
                                break;
                            case (int)SCSIVersionDescriptorATA7Mask.T13_1532_r3:
                                sb.AppendLine("Device complies with T13/1532-D revision 3 (ATA/ATAPI-7)");
                                break;
                            case (int)SCSIVersionDescriptorATA7Mask.ANSI_2005:
                                sb.AppendLine("Device complies with ANSI INCITS 397-2005 (ATA/ATAPI-7)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ATA/ATAPI-7", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.ATA_ATAPI8: //0x1620
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorATA8Mask.ATA8_AAM:
                                sb.AppendLine("Device complies with standard ATA8-AAM Architecture Model (no version claimed)");
                                break;
                            case (int)SCSIVersionDescriptorATA8Mask.ATA8_PT:
                                sb.AppendLine("Device complies with standard ATA8-PT Parallel Transport (no version claimed)");
                                break;
                            case (int)SCSIVersionDescriptorATA8Mask.ATA8_AST:
                                sb.AppendLine("Device complies with standard ATA8-AST Serial Transport (no version claimed)");
                                break;
                            case (int)SCSIVersionDescriptorATA8Mask.ATA8_ACS:
                                sb.AppendLine("Device complies with standard ATA8-ACS ATA/ATAPI Command Set (no version claimed)");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ATA/ATAPI-8", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.USB: //0x1720
                        switch (VersionDescriptor & 0x001F)
                        {
                            case (int)SCSIVersionDescriptorUSBMask.NoVersion:
                                sb.AppendLine("Device complies with Universal Serial Bus specification, no revision claimed");
                                break;
                            case (int)SCSIVersionDescriptorUSBMask.USB11:
                                sb.AppendLine("Device complies with Universal Serial Bus Specification, Revision 1.1");
                                break;
                            case (int)SCSIVersionDescriptorUSBMask.USB20:
                                sb.AppendLine("Device complies with Universal Serial Bus Specification, Revision 2.0");
                                break;
                            case (int)SCSIVersionDescriptorUSBMask.USB_MSC_BULK:
                                sb.AppendLine("Device complies with Universal Serial Bus Mass Storage Class Bulk-Only Transport, Revision 1.0");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard ", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    case (int)SCSIVersionDescriptorStandardMask.SAT: //0x1EA0
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendLine("Device complies with standard SAT");
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of standard SAT", VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                    default:
                        switch (VersionDescriptor & 0x001F)
                        {
                            case 0x00:
                                sb.AppendFormat("Device complies with unknown standard 0x:{0:X4}", VersionDescriptor & 0x001F).AppendLine();
                                break;
                            default:
                                sb.AppendFormat("Device claims unknown version 0x{0:X2} of unknown standard 0x:{1:X4}", VersionDescriptor & 0x001F, VersionDescriptor & 0x001F).AppendLine();
                                break;
                        }
                        break;
                }
            }

            //if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (SCSIInquiry Decoder): Vendor's device type modifier = 0x{0:X2}", response.DeviceTypeModifier).AppendLine();
                sb.AppendFormat("DEBUG (SCSIInquiry Decoder): Reserved byte 5, bits 2 to 1 = 0x{0:X2}", response.Reserved2).AppendLine();
                sb.AppendFormat("DEBUG (SCSIInquiry Decoder): Reserved byte 56, bits 7 to 4 = 0x{0:X2}", response.Reserved3).AppendLine();
                sb.AppendFormat("DEBUG (SCSIInquiry Decoder): Reserved byte 57 = 0x{0:X2}", response.Reserved4).AppendLine();

                if (response.Reserved5 != null)
                {
                    sb.AppendLine("DEBUG (SCSIInquiry Decoder): Reserved bytes 74 to 95");
                    sb.AppendLine("============================================================");
                    sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.Reserved5, 60));
                    sb.AppendLine("============================================================");
                }

                if (response.VendorSpecific != null)
                {
                    sb.AppendLine("DEBUG (SCSIInquiry Decoder): Vendor-specific bytes 36 to 55");
                    sb.AppendLine("============================================================");
                    sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific, 60));
                    sb.AppendLine("============================================================");
                }

                if (response.VendorSpecific2 != null)
                {
                    sb.AppendFormat("DEBUG (SCSIInquiry Decoder): Vendor-specific bytes 96 to {0}", response.AdditionalLength+4).AppendLine();
                    sb.AppendLine("============================================================");
                    sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific2, 60));
                    sb.AppendLine("============================================================");
                }
            }

            return sb.ToString();
        }

        public static string PrettifySCSIInquiry(byte[] SCSIInquiryResponse)
        {
            SCSIInquiry? decoded = DecodeSCSIInquiry(SCSIInquiryResponse);
            return PrettifySCSIInquiry(decoded);
        }

        #endregion Public methods

        #region Public structures

        // SCSI INQUIRY command response
        public struct SCSIInquiry
        {
            /// <summary>
            /// Peripheral qualifier
            /// Byte 0, bits 7 to 5
            /// </summary>
            public byte PeripheralQualifier;
            /// <summary>
            /// Peripheral device type
            /// Byte 0, bits 4 to 0
            /// </summary>
            public byte PeripheralDeviceType;
            /// <summary>
            /// Removable device
            /// Byte 1, bit 7
            /// </summary>
            public bool RMB;
            /// <summary>
            /// SCSI-1 vendor-specific qualification codes
            /// Byte 1, bits 6 to 0
            /// </summary>
            public byte DeviceTypeModifier;
            /// <summary>
            /// ISO/IEC SCSI Standard Version
            /// Byte 2, bits 7 to 6, mask = 0xC0, >> 6
            /// </summary>
            public byte ISOVersion;
            /// <summary>
            /// ECMA SCSI Standard Version
            /// Byte 2, bits 5 to 3, mask = 0x38, >> 3
            /// </summary>
            public byte ECMAVersion;
            /// <summary>
            /// ANSI SCSI Standard Version
            /// Byte 2, bits 2 to 0, mask = 0x07
            /// </summary>
            public byte ANSIVersion;
            /// <summary>
            /// Asynchronous Event Reporting Capability supported
            /// Byte 3, bit 7
            /// </summary>
            public bool AERC;
            /// <summary>
            /// Device supports TERMINATE TASK command
            /// Byte 3, bit 6
            /// </summary>
            public bool TrmTsk;
            /// <summary>
            /// Supports setting Normal ACA
            /// Byte 3, bit 5
            /// </summary>
            public bool NormACA;
            /// <summary>
            /// Supports LUN hierarchical addressing
            /// Byte 3, bit 4
            /// </summary>
            public bool HiSup;
            /// <summary>
            /// Responde data format
            /// Byte 3, bit 3 to 0
            /// </summary>
            public byte ResponseDataFormat;
            /// <summary>
            /// Lenght of total INQUIRY response minus 4
            /// Byte 4
            /// </summary>
            public byte AdditionalLength;
            /// <summary>
            /// Device contains an embedded storage array controller
            /// Byte 5, bit 7
            /// </summary>
            public bool SCCS;
            /// <summary>
            /// Device contains an Access Control Coordinator
            /// Byte 5, bit 6
            /// </summary>
            public bool ACC;
            /// <summary>
            /// Supports asymetrical logical unit access
            /// Byte 5, bits 5 to 4
            /// </summary>
            public byte TPGS;
            /// <summary>
            /// Supports third-party copy commands
            /// Byte 5, bit 3
            /// </summary>
            public bool ThreePC;
            /// <summary>
            /// Reserved
            /// Byte 5, bits 2 to 1
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Supports protection information
            /// Byte 5, bit 0
            /// </summary>
            public bool Protect;
            /// <summary>
            /// Supports basic queueing
            /// Byte 6, bit 7
            /// </summary>
            public bool BQue;
            /// <summary>
            /// Device contains an embedded enclosure services component
            /// Byte 6, bit 6
            /// </summary>
            public bool EncServ;
            /// <summary>
            /// Vendor-specific
            /// Byte 6, bit 5
            /// </summary>
            public bool VS1;
            /// <summary>
            /// Multi-port device
            /// Byte 6, bit 4
            /// </summary>
            public bool MultiP;
            /// <summary>
            /// Device contains or is attached to a medium changer
            /// Byte 6, bit 3
            /// </summary>
            public bool MChngr;
            /// <summary>
            /// Device supports request and acknowledge handshakes
            /// Byte 6, bit 2
            /// </summary>
            public bool ACKREQQ;
            /// <summary>
            /// Supports 32-bit wide SCSI addresses
            /// Byte 6, bit 1
            /// </summary>
            public bool Addr32;
            /// <summary>
            /// Supports 16-bit wide SCSI addresses
            /// Byte 6, bit 0
            /// </summary>
            public bool Addr16;
            /// <summary>
            /// Device supports relative addressing
            /// Byte 7, bit 7
            /// </summary>
            public bool RelAddr;
            /// <summary>
            /// Supports 32-bit wide data transfers
            /// Byte 7, bit 6
            /// </summary>
            public bool WBus32;
            /// <summary>
            /// Supports 16-bit wide data transfers
            /// Byte 7, bit 5
            /// </summary>
            public bool WBus16;
            /// <summary>
            /// Supports synchronous data transfer
            /// Byte 7, bit 4
            /// </summary>
            public bool Sync;
            /// <summary>
            /// Supports linked commands
            /// Byte 7, bit 3
            /// </summary>
            public bool Linked;
            /// <summary>
            /// Supports CONTINUE TASK and TARGET TRANSFER DISABLE commands
            /// Byte 7, bit 2
            /// </summary>
            public bool TranDis;
            /// <summary>
            /// Supports TCQ queue
            /// Byte 7, bit 1
            /// </summary>
            public bool CmdQue;
            /// <summary>
            /// Indicates that the devices responds to RESET with soft reset
            /// Byte 7, bit 0
            /// </summary>
            public bool SftRe;
            /// <summary>
            /// Vendor identification
            /// Bytes 8 to 15
            /// </summary>
            public byte[] VendorIdentification;
            /// <summary>
            /// Product identification
            /// Bytes 16 to 31
            /// </summary>
            public byte[] ProductIdentification;
            /// <summary>
            /// Product revision level
            /// Bytes 32 to 35
            /// </summary>
            public byte[] ProductRevisionLevel;
            /// <summary>
            /// Vendor-specific data
            /// Bytes 36 to 55
            /// </summary>
            public byte[] VendorSpecific;
            /// <summary>
            /// Byte 56, bits 7 to 4
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Supported SPI clocking
            /// Byte 56, bits 3 to 2
            /// </summary>
            public byte Clocking;
            /// <summary>
            /// Device supports Quick Arbitration and Selection
            /// Byte 56, bit 1
            /// </summary>
            public bool QAS;
            /// <summary>
            /// Supports information unit transfers
            /// Byte 56, bit 0
            /// </summary>
            public bool IUS;
            /// <summary>
            /// Reserved
            /// Byte 57
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Array of version descriptors
            /// Bytes 58 to 73
            /// </summary>
            public UInt16[] VersionDescriptors;
            /// <summary>
            /// Reserved
            /// Bytes 74 to 95
            /// </summary>
            public byte[] Reserved5;
            /// <summary>
            /// Reserved
            /// Bytes 96 to end
            /// </summary>
            public byte[] VendorSpecific2;
        }

        #endregion Public structures
    }
}

