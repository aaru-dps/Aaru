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
    public enum PeripheralQualifiers : byte
    {
        /// <summary>
        /// Peripheral qualifier: Device is connected and supported
        /// </summary>
        Supported = 0x00,
        /// <summary>
        /// Peripheral qualifier: Device is supported but not connected
        /// </summary>
        Unconnected = 0x01,
        /// <summary>
        /// Peripheral qualifier: Reserved value
        /// </summary>
        Reserved = 0x02,
        /// <summary>
        /// Peripheral qualifier: Device is connected but unsupported
        /// </summary>
        Unsupported = 0x03,
        /// <summary>
        /// Peripheral qualifier: Vendor values: 0x04, 0x05, 0x06 and 0x07
        /// </summary>
        VendorMask = 0x04
    }

    public enum PeripheralDeviceTypes : byte
    {
        /// <summary>
        /// Direct-access device
        /// </summary>
        DirectAccess = 0x00,
        /// <summary>
        /// Sequential-access device
        /// </summary>
        SequentialAccess = 0x01,
        /// <summary>
        /// Printer device
        /// </summary>
        PrinterDevice = 0x02,
        /// <summary>
        /// Processor device
        /// </summary>
        ProcessorDevice = 0x03,
        /// <summary>
        /// Write-once device
        /// </summary>
        WriteOnceDevice = 0x04,
        /// <summary>
        /// CD-ROM/DVD/etc device
        /// </summary>
        MultiMediaDevice = 0x05,
        /// <summary>
        /// Scanner device
        /// </summary>
        ScannerDevice = 0x06,
        /// <summary>
        /// Optical memory device
        /// </summary>
        OpticalDevice = 0x07,
        /// <summary>
        /// Medium change device
        /// </summary>
        MediumChangerDevice = 0x08,
        /// <summary>
        /// Communications device
        /// </summary>
        CommsDevice = 0x09,
        /// <summary>
        /// Graphics arts pre-press device (defined in ASC IT8)
        /// </summary>
        PrePressDevice1 = 0x0A,
        /// <summary>
        /// Graphics arts pre-press device (defined in ASC IT8)
        /// </summary>
        PrePressDevice2 = 0x0B,
        /// <summary>
        /// Array controller device
        /// </summary>
        ArrayControllerDevice = 0x0C,
        /// <summary>
        /// Enclosure services device
        /// </summary>
        EnclosureServiceDevice = 0x0D,
        /// <summary>
        /// Simplified direct-access device
        /// </summary>
        SimplifiedDevice = 0x0E,
        /// <summary>
        /// Optical card reader/writer device
        /// </summary>
        OCRWDevice = 0x0F,
        /// <summary>
        /// Bridging Expanders
        /// </summary>
        BridgingExpander = 0x10,
        /// <summary>
        /// Object-based Storage Device
        /// </summary>
        ObjectDevice = 0x11,
        /// <summary>
        /// Automation/Drive Interface
        /// </summary>
        ADCDevice = 0x12,
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
        WellKnownDevice = 0x1E,
        /// <summary>
        /// Unknown or no device type
        /// </summary>
        UnknownDevice = 0x1F
    }

    public enum ANSIVersions : byte
    {
        /// <summary>
        /// Device does not claim conformance to any ANSI version
        /// </summary>
        ANSINoVersion = 0x00,
        /// <summary>
        /// Device complies with ANSI X3.131:1986
        /// </summary>
        ANSI1986Version = 0x01,
        /// <summary>
        /// Device complies with ANSI X3.131:1994
        /// </summary>
        ANSI1994Version = 0x02,
        /// <summary>
        /// Device complies with ANSI X3.301:1997
        /// </summary>
        ANSI1997Version = 0x03,
        /// <summary>
        /// Device complies with ANSI X3.351:2001
        /// </summary>
        ANSI2001Version = 0x04,
        /// <summary>
        /// Device complies with ANSI X3.408:2005.
        /// </summary>
        ANSI2005Version = 0x05,
        /// <summary>
        /// Device complies with SPC-4
        /// </summary>
        ANSI2008Version = 0x06
    }

    public enum ECMAVersions : byte
    {
        /// <summary>
        /// Device does not claim conformance to any ECMA version
        /// </summary>
        ECMANoVersion = 0x00,
        /// <summary>
        /// Device complies with a ECMA-111 standard
        /// </summary>
        ECMA111 = 0x01
    }

    public enum ISOVersions : byte
    {
        /// <summary>
        /// Device does not claim conformance to any ISO/IEC version
        /// </summary>
        ISONoVersion = 0x00,
        /// <summary>
        /// Device complies with ISO/IEC 9316:1995
        /// </summary>
        ISO1995Version = 0x02
    }

    public enum SPIClocking : byte
    {
        /// <summary>
        /// Supports only ST
        /// </summary>
        ST = 0x00,
        /// <summary>
        /// Supports only DT
        /// </summary>
        DT = 0x01,
        /// <summary>
        /// Reserved value
        /// </summary>
        Reserved = 0x02,
        /// <summary>
        /// Supports ST and DT
        /// </summary>
        STandDT = 0x03,
    }

    public enum TGPSValues : byte
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

