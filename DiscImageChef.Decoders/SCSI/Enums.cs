// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
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
        /// Security Manager Device
        /// </summary>
        SCSISecurityManagerDevice = 0x13,
        /// <summary>
        /// Host managed zoned block device
        /// </summary>
        SCSIZonedBlockDEvice = 0x14,
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
        /// Device complies with ANSI X3.408:2005.
        /// </summary>
        SCSIANSI2005Version = 0x05,
        /// <summary>
        /// Device complies with SPC-4
        /// </summary>
        SCSIANSI2008Version = 0x06
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
}

