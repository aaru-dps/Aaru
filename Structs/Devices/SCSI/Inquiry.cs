// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Inquiry.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common structures for SCSI devices.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines a high level interpretation of the SCSI INQUIRY response.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Aaru.Console;

namespace Aaru.CommonTypes.Structs.Devices.SCSI;

/// <summary>
///     Information from the following standards: T9/375-D revision 10l T10/995-D revision 10 T10/1236-D revision 20
///     T10/1416-D revision 23 T10/1731-D revision 16 T10/502 revision 05 RFC 7144 ECMA-111
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public struct Inquiry
{
    /// <summary>Peripheral qualifier Byte 0, bits 7 to 5</summary>
    public byte PeripheralQualifier;
    /// <summary>Peripheral device type Byte 0, bits 4 to 0</summary>
    public byte PeripheralDeviceType;
    /// <summary>Removable device Byte 1, bit 7</summary>
    public bool RMB;
    /// <summary>SCSI-1 vendor-specific qualification codes Byte 1, bits 6 to 0</summary>
    public byte DeviceTypeModifier;
    /// <summary>ISO/IEC SCSI Standard Version Byte 2, bits 7 to 6, mask = 0xC0, >> 6</summary>
    public byte ISOVersion;
    /// <summary>ECMA SCSI Standard Version Byte 2, bits 5 to 3, mask = 0x38, >> 3</summary>
    public byte ECMAVersion;
    /// <summary>ANSI SCSI Standard Version Byte 2, bits 2 to 0, mask = 0x07</summary>
    public byte ANSIVersion;
    /// <summary>Asynchronous Event Reporting Capability supported Byte 3, bit 7</summary>
    public bool AERC;
    /// <summary>Device supports TERMINATE TASK command Byte 3, bit 6</summary>
    public bool TrmTsk;
    /// <summary>Supports setting Normal ACA Byte 3, bit 5</summary>
    public bool NormACA;
    /// <summary>Supports LUN hierarchical addressing Byte 3, bit 4</summary>
    public bool HiSup;
    /// <summary>Responde data format Byte 3, bit 3 to 0</summary>
    public byte ResponseDataFormat;
    /// <summary>Lenght of total INQUIRY response minus 4 Byte 4</summary>
    public byte AdditionalLength;
    /// <summary>Device contains an embedded storage array controller Byte 5, bit 7</summary>
    public bool SCCS;
    /// <summary>Device contains an Access Control Coordinator Byte 5, bit 6</summary>
    public bool ACC;
    /// <summary>Supports asymetrical logical unit access Byte 5, bits 5 to 4</summary>
    public byte TPGS;
    /// <summary>Supports third-party copy commands Byte 5, bit 3</summary>
    public bool ThreePC;
    /// <summary>Reserved Byte 5, bits 2 to 1</summary>
    public byte Reserved2;
    /// <summary>Supports protection information Byte 5, bit 0</summary>
    public bool Protect;
    /// <summary>Supports basic queueing Byte 6, bit 7</summary>
    public bool BQue;
    /// <summary>Device contains an embedded enclosure services component Byte 6, bit 6</summary>
    public bool EncServ;
    /// <summary>Vendor-specific Byte 6, bit 5</summary>
    public bool VS1;
    /// <summary>Multi-port device Byte 6, bit 4</summary>
    public bool MultiP;
    /// <summary>Device contains or is attached to a medium changer Byte 6, bit 3</summary>
    public bool MChngr;
    /// <summary>Device supports request and acknowledge handshakes Byte 6, bit 2</summary>
    public bool ACKREQQ;
    /// <summary>Supports 32-bit wide SCSI addresses Byte 6, bit 1</summary>
    public bool Addr32;
    /// <summary>Supports 16-bit wide SCSI addresses Byte 6, bit 0</summary>
    public bool Addr16;
    /// <summary>Device supports relative addressing Byte 7, bit 7</summary>
    public bool RelAddr;
    /// <summary>Supports 32-bit wide data transfers Byte 7, bit 6</summary>
    public bool WBus32;
    /// <summary>Supports 16-bit wide data transfers Byte 7, bit 5</summary>
    public bool WBus16;
    /// <summary>Supports synchronous data transfer Byte 7, bit 4</summary>
    public bool Sync;
    /// <summary>Supports linked commands Byte 7, bit 3</summary>
    public bool Linked;
    /// <summary>Supports CONTINUE TASK and TARGET TRANSFER DISABLE commands Byte 7, bit 2</summary>
    public bool TranDis;
    /// <summary>Supports TCQ queue Byte 7, bit 1</summary>
    public bool CmdQue;
    /// <summary>Indicates that the devices responds to RESET with soft reset Byte 7, bit 0</summary>
    public bool SftRe;
    /// <summary>Vendor identification Bytes 8 to 15</summary>
    public byte[] VendorIdentification;
    /// <summary>Product identification Bytes 16 to 31</summary>
    public byte[] ProductIdentification;
    /// <summary>Product revision level Bytes 32 to 35</summary>
    public byte[] ProductRevisionLevel;
    /// <summary>Vendor-specific data Bytes 36 to 55</summary>
    public byte[] VendorSpecific;
    /// <summary>Byte 56, bits 7 to 4</summary>
    public byte Reserved3;
    /// <summary>Supported SPI clocking Byte 56, bits 3 to 2</summary>
    public byte Clocking;
    /// <summary>Device supports Quick Arbitration and Selection Byte 56, bit 1</summary>
    public bool QAS;
    /// <summary>Supports information unit transfers Byte 56, bit 0</summary>
    public bool IUS;
    /// <summary>Reserved Byte 57</summary>
    public byte Reserved4;
    /// <summary>Array of version descriptors Bytes 58 to 73</summary>
    public ushort[] VersionDescriptors;
    /// <summary>Reserved Bytes 74 to 95</summary>
    public byte[] Reserved5;
    /// <summary>Reserved Bytes 96 to end</summary>
    public byte[] VendorSpecific2;

    // Per DLT4000/DLT4500/DLT4700 Cartridge Tape Subsystem Product Manual

    #region Quantum vendor unique inquiry data structure
    /// <summary>Means that the INQUIRY response contains 56 bytes or more, so this data has been filled</summary>
    public bool QuantumPresent;
    /// <summary>The product family. Byte 36, bits 7 to 5</summary>
    public byte Qt_ProductFamily;
    /// <summary>The released firmware. Byte 36, bits 4 to 0</summary>
    public byte Qt_ReleasedFirmware;
    /// <summary>The firmware major version. Byte 37</summary>
    public byte Qt_FirmwareMajorVersion;
    /// <summary>The firmware minor version. Byte 38</summary>
    public byte Qt_FirmwareMinorVersion;
    /// <summary>The EEPROM format major version. Byte 39</summary>
    public byte Qt_EEPROMFormatMajorVersion;
    /// <summary>The EEPROM format minor version. Byte 40</summary>
    public byte Qt_EEPROMFormatMinorVersion;
    /// <summary>The firmware personality. Byte 41</summary>
    public byte Qt_FirmwarePersonality;
    /// <summary>The firmware sub personality. Byte 42</summary>
    public byte Qt_FirmwareSubPersonality;
    /// <summary>The tape directory format version. Byte 43</summary>
    public byte Qt_TapeDirectoryFormatVersion;
    /// <summary>The controller hardware version. Byte 44</summary>
    public byte Qt_ControllerHardwareVersion;
    /// <summary>The drive EEPROM version. Byte 45</summary>
    public byte Qt_DriveEEPROMVersion;
    /// <summary>The drive hardware version. Byte 46</summary>
    public byte Qt_DriveHardwareVersion;
    /// <summary>The media loader firmware version. Byte 47</summary>
    public byte Qt_MediaLoaderFirmwareVersion;
    /// <summary>The media loader hardware version. Byte 48</summary>
    public byte Qt_MediaLoaderHardwareVersion;
    /// <summary>The media loader mechanical version. Byte 49</summary>
    public byte Qt_MediaLoaderMechanicalVersion;
    /// <summary>Is a media loader present? Byte 50</summary>
    public bool Qt_MediaLoaderPresent;
    /// <summary>Is a library present? Byte 51</summary>
    public bool Qt_LibraryPresent;
    /// <summary>The module revision. Bytes 52 to 55</summary>
    public byte[] Qt_ModuleRevision;
    #endregion Quantum vendor unique inquiry data structure

    #region IBM vendor unique inquiry data structure
    /// <summary>Means that the INQUIRY response contains 56 bytes or more, so this data has been filled</summary>
    public bool IBMPresent;
    /// <summary>Drive is not capable of automation Byte 36 bit 0</summary>
    public bool IBM_AutDis;
    /// <summary>If not zero, limit in MB/s = Max * (this / 256) Byte 37</summary>
    public byte IBM_PerformanceLimit;
    /// <summary>Byte 41</summary>
    public byte IBM_OEMSpecific;
    #endregion IBM vendor unique inquiry data structure

    #region HP vendor unique inquiry data structure
    /// <summary>Means that the INQUIRY response contains 49 bytes or more, so this data has been filled</summary>
    public bool HPPresent;
    /// <summary>WORM version Byte 40 bits 7 to 1</summary>
    public byte HP_WORMVersion;
    /// <summary>WORM supported Byte 40 bit 0</summary>
    public bool HP_WORM;
    /// <summary>Bytes 43 to 48</summary>
    public byte[] HP_OBDR;
    #endregion HP vendor unique inquiry data structure

    #region Seagate vendor unique inquiry data structure
    /// <summary>Means that bytes 36 to 43 are filled</summary>
    public bool SeagatePresent;
    /// <summary>Drive Serial Number Bytes 36 to 43</summary>
    public byte[] Seagate_DriveSerialNumber;
    /// <summary>Means that bytes 96 to 143 are filled</summary>
    public bool Seagate2Present;
    /// <summary>Contains Seagate copyright notice Bytes 96 to 143</summary>
    public byte[] Seagate_Copyright;
    /// <summary>Means that bytes 144 to 147 are filled</summary>
    public bool Seagate3Present;
    /// <summary>Reserved Seagate field Bytes 144 to 147</summary>
    public byte[] Seagate_ServoPROMPartNo;
    #endregion Seagate vendor unique inquiry data structure

    #region Kreon vendor unique inquiry data structure
    /// <summary>Means that firmware is Kreon</summary>
    public bool KreonPresent;
    /// <summary>Kreon identifier Bytes 36 to 40</summary>
    public byte[] KreonIdentifier;
    /// <summary>Kreon just a 0x20 Bytes 41</summary>
    public byte KreonSpace;
    /// <summary>Kreon version string Bytes 42 to 46</summary>
    public byte[] KreonVersion;
    #endregion Kreon vendor unique inquiry data structure

    #region Sony Hi-MD data
    /// <summary>Set if Hi-MD signature is present</summary>
    public bool IsHiMD;
    /// <summary>Hi-MD signature, bytes 36 to 44</summary>
    public byte[] HiMDSignature;
    /// <summary>Unknown data, bytes 44 to 55</summary>
    public byte[] HiMDSpecific;
    #endregion Sony Hi-MD data

    static readonly byte[] HiMDSignatureContents =
    {
        0x48, 0x69, 0x2D, 0x4D, 0x44, 0x20, 0x20, 0x20
    };

    /// <summary>Decodes a SCSI INQUIRY response</summary>
    /// <param name="SCSIInquiryResponse">INQUIRY raw response data</param>
    /// <returns>Decoded SCSI INQUIRY</returns>
    #region Public methods
    public static Inquiry? Decode(byte[] SCSIInquiryResponse)
    {
        if(SCSIInquiryResponse == null)
            return null;

        if(SCSIInquiryResponse.Length < 36 &&
           SCSIInquiryResponse.Length != 5)
        {
            AaruConsole.DebugWriteLine("SCSI INQUIRY decoder",
                                       "INQUIRY response is {0} bytes, less than minimum of 36 bytes, decoded data can be incorrect, not decoding.",
                                       SCSIInquiryResponse.Length);

            return null;
        }

        if(SCSIInquiryResponse.Length < SCSIInquiryResponse[4] + 4 &&
           SCSIInquiryResponse.Length != SCSIInquiryResponse[4])
        {
            AaruConsole.DebugWriteLine("SCSI INQUIRY decoder",
                                       "INQUIRY response length ({0} bytes) is different than specified in length field ({1} bytes), decoded data can be incorrect, not decoding.",
                                       SCSIInquiryResponse.Length, SCSIInquiryResponse[4] + 4);

            return null;
        }

        var decoded = new Inquiry();

        if(SCSIInquiryResponse.Length >= 1)
        {
            decoded.PeripheralQualifier  = (byte)((SCSIInquiryResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (byte)(SCSIInquiryResponse[0] & 0x1F);
        }

        if(SCSIInquiryResponse.Length >= 2)
        {
            decoded.RMB                = Convert.ToBoolean(SCSIInquiryResponse[1] & 0x80);
            decoded.DeviceTypeModifier = (byte)(SCSIInquiryResponse[1] & 0x7F);
        }

        if(SCSIInquiryResponse.Length >= 3)
        {
            decoded.ISOVersion  = (byte)((SCSIInquiryResponse[2] & 0xC0) >> 6);
            decoded.ECMAVersion = (byte)((SCSIInquiryResponse[2] & 0x38) >> 3);
            decoded.ANSIVersion = (byte)(SCSIInquiryResponse[2] & 0x07);
        }

        if(SCSIInquiryResponse.Length >= 4)
        {
            decoded.AERC               = Convert.ToBoolean(SCSIInquiryResponse[3] & 0x80);
            decoded.TrmTsk             = Convert.ToBoolean(SCSIInquiryResponse[3] & 0x40);
            decoded.NormACA            = Convert.ToBoolean(SCSIInquiryResponse[3] & 0x20);
            decoded.HiSup              = Convert.ToBoolean(SCSIInquiryResponse[3] & 0x10);
            decoded.ResponseDataFormat = (byte)(SCSIInquiryResponse[3] & 0x07);
        }

        if(SCSIInquiryResponse.Length >= 5)
            decoded.AdditionalLength = SCSIInquiryResponse[4];

        if(SCSIInquiryResponse.Length >= 6)
        {
            decoded.SCCS      = Convert.ToBoolean(SCSIInquiryResponse[5] & 0x80);
            decoded.ACC       = Convert.ToBoolean(SCSIInquiryResponse[5] & 0x40);
            decoded.TPGS      = (byte)((SCSIInquiryResponse[5] & 0x30) >> 4);
            decoded.ThreePC   = Convert.ToBoolean(SCSIInquiryResponse[5] & 0x08);
            decoded.Reserved2 = (byte)((SCSIInquiryResponse[5] & 0x06) >> 1);
            decoded.Protect   = Convert.ToBoolean(SCSIInquiryResponse[5] & 0x01);
        }

        if(SCSIInquiryResponse.Length >= 7)
        {
            decoded.BQue    = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x80);
            decoded.EncServ = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x40);
            decoded.VS1     = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x20);
            decoded.MultiP  = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x10);
            decoded.MChngr  = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x08);
            decoded.ACKREQQ = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x04);
            decoded.Addr32  = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x02);
            decoded.Addr16  = Convert.ToBoolean(SCSIInquiryResponse[6] & 0x01);
        }

        if(SCSIInquiryResponse.Length >= 8)
        {
            decoded.RelAddr = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x80);
            decoded.WBus32  = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x40);
            decoded.WBus16  = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x20);
            decoded.Sync    = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x10);
            decoded.Linked  = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x08);
            decoded.TranDis = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x04);
            decoded.CmdQue  = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x02);
            decoded.SftRe   = Convert.ToBoolean(SCSIInquiryResponse[7] & 0x01);
        }

        if(SCSIInquiryResponse.Length >= 16)
        {
            decoded.VendorIdentification = new byte[8];
            Array.Copy(SCSIInquiryResponse, 8, decoded.VendorIdentification, 0, 8);
        }

        if(SCSIInquiryResponse.Length >= 32)
        {
            decoded.ProductIdentification = new byte[16];
            Array.Copy(SCSIInquiryResponse, 16, decoded.ProductIdentification, 0, 16);
        }

        if(SCSIInquiryResponse.Length >= 36)
        {
            decoded.ProductRevisionLevel = new byte[4];
            Array.Copy(SCSIInquiryResponse, 32, decoded.ProductRevisionLevel, 0, 4);
        }

        if(SCSIInquiryResponse.Length >= 44)
        {
            // Seagate 1
            decoded.SeagatePresent            = true;
            decoded.Seagate_DriveSerialNumber = new byte[8];
            Array.Copy(SCSIInquiryResponse, 36, decoded.Seagate_DriveSerialNumber, 0, 8);

            // Hi-MD
            decoded.HiMDSignature = new byte[8];
            Array.Copy(SCSIInquiryResponse, 36, decoded.HiMDSignature, 0, 8);
            decoded.IsHiMD = HiMDSignatureContents.SequenceEqual(decoded.HiMDSignature);
        }

        if(SCSIInquiryResponse.Length >= 46)
        {
            // Kreon
            decoded.KreonIdentifier = new byte[5];
            Array.Copy(SCSIInquiryResponse, 36, decoded.KreonIdentifier, 0, 5);
            decoded.KreonSpace   = SCSIInquiryResponse[41];
            decoded.KreonVersion = new byte[5];
            Array.Copy(SCSIInquiryResponse, 42, decoded.KreonVersion, 0, 5);

            if(decoded.KreonSpace == 0x20 &&
               decoded.KreonIdentifier.SequenceEqual(new byte[]
               {
                   0x4B, 0x52, 0x45, 0x4F, 0x4E
               }))
                decoded.KreonPresent = true;
        }

        if(SCSIInquiryResponse.Length >= 49)
        {
            // HP
            decoded.HPPresent      =  true;
            decoded.HP_WORM        |= (SCSIInquiryResponse[40] & 0x01) == 0x01;
            decoded.HP_WORMVersion =  (byte)((SCSIInquiryResponse[40] & 0x7F) >> 1);
            decoded.HP_OBDR        =  new byte[6];
            Array.Copy(SCSIInquiryResponse, 43, decoded.HP_OBDR, 0, 6);
        }

        if(SCSIInquiryResponse.Length >= 56)
        {
            if(decoded.IsHiMD)
            {
                decoded.HiMDSpecific = new byte[12];
                Array.Copy(SCSIInquiryResponse, 44, decoded.HiMDSpecific, 0, 12);
            }
            else
            {
                decoded.VendorSpecific = new byte[20];
                Array.Copy(SCSIInquiryResponse, 36, decoded.VendorSpecific, 0, 20);
            }

            // Quantum
            decoded.QuantumPresent                  = true;
            decoded.Qt_ProductFamily                = (byte)((SCSIInquiryResponse[36] & 0xF0) >> 4);
            decoded.Qt_ReleasedFirmware             = (byte)(SCSIInquiryResponse[36] & 0x0F);
            decoded.Qt_FirmwareMajorVersion         = SCSIInquiryResponse[37];
            decoded.Qt_FirmwareMinorVersion         = SCSIInquiryResponse[38];
            decoded.Qt_EEPROMFormatMajorVersion     = SCSIInquiryResponse[39];
            decoded.Qt_EEPROMFormatMinorVersion     = SCSIInquiryResponse[40];
            decoded.Qt_FirmwarePersonality          = SCSIInquiryResponse[41];
            decoded.Qt_FirmwareSubPersonality       = SCSIInquiryResponse[42];
            decoded.Qt_TapeDirectoryFormatVersion   = SCSIInquiryResponse[43];
            decoded.Qt_ControllerHardwareVersion    = SCSIInquiryResponse[44];
            decoded.Qt_DriveEEPROMVersion           = SCSIInquiryResponse[45];
            decoded.Qt_DriveHardwareVersion         = SCSIInquiryResponse[46];
            decoded.Qt_MediaLoaderFirmwareVersion   = SCSIInquiryResponse[47];
            decoded.Qt_MediaLoaderHardwareVersion   = SCSIInquiryResponse[48];
            decoded.Qt_MediaLoaderMechanicalVersion = SCSIInquiryResponse[49];
            decoded.Qt_MediaLoaderPresent           = SCSIInquiryResponse[50] > 0;
            decoded.Qt_LibraryPresent               = SCSIInquiryResponse[51] > 0;
            decoded.Qt_ModuleRevision               = new byte[4];
            Array.Copy(SCSIInquiryResponse, 52, decoded.Qt_ModuleRevision, 0, 4);

            // IBM
            decoded.IBMPresent           =  true;
            decoded.IBM_AutDis           |= (SCSIInquiryResponse[36] & 0x01) == 0x01;
            decoded.IBM_PerformanceLimit =  SCSIInquiryResponse[37];
            decoded.IBM_OEMSpecific      =  SCSIInquiryResponse[41];
        }

        if(SCSIInquiryResponse.Length >= 57)
        {
            decoded.Reserved3 = (byte)((SCSIInquiryResponse[56] & 0xF0) >> 4);
            decoded.Clocking  = (byte)((SCSIInquiryResponse[56] & 0x0C) >> 2);
            decoded.QAS       = Convert.ToBoolean(SCSIInquiryResponse[56] & 0x02);
            decoded.IUS       = Convert.ToBoolean(SCSIInquiryResponse[56] & 0x01);
        }

        if(SCSIInquiryResponse.Length >= 58)
            decoded.Reserved4 = SCSIInquiryResponse[57];

        if(SCSIInquiryResponse.Length >= 60)
        {
            int descriptorsNo;

            if(SCSIInquiryResponse.Length >= 74)
                descriptorsNo = 8;
            else
                descriptorsNo = (SCSIInquiryResponse.Length - 58) / 2;

            decoded.VersionDescriptors = new ushort[descriptorsNo];

            for(int i = 0; i < descriptorsNo; i++)
                decoded.VersionDescriptors[i] = BitConverter.ToUInt16(SCSIInquiryResponse, 58 + (i * 2));
        }

        if(SCSIInquiryResponse.Length >= 75 &&
           SCSIInquiryResponse.Length < 96)
        {
            decoded.Reserved5 = new byte[SCSIInquiryResponse.Length - 74];
            Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, SCSIInquiryResponse.Length - 74);
        }

        if(SCSIInquiryResponse.Length >= 96)
        {
            decoded.Reserved5 = new byte[22];
            Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, 22);
        }

        if(SCSIInquiryResponse.Length > 96)
        {
            decoded.VendorSpecific2 = new byte[SCSIInquiryResponse.Length - 96];
            Array.Copy(SCSIInquiryResponse, 96, decoded.VendorSpecific2, 0, SCSIInquiryResponse.Length - 96);
        }

        if(SCSIInquiryResponse.Length >= 144)
        {
            // Seagate 2
            decoded.Seagate2Present   = true;
            decoded.Seagate_Copyright = new byte[48];
            Array.Copy(SCSIInquiryResponse, 96, decoded.Seagate_Copyright, 0, 48);
        }

        if(SCSIInquiryResponse.Length < 148)
            return decoded;

        // Seagate 2
        decoded.Seagate3Present         = true;
        decoded.Seagate_ServoPROMPartNo = new byte[4];
        Array.Copy(SCSIInquiryResponse, 144, decoded.Seagate_ServoPROMPartNo, 0, 4);

        return decoded;
    }

    /// <summary>Encodes a SCSI INQUIRY response</summary>
    /// <param name="inq">Decoded SCSI INQUIRY</param>
    /// <returns>Raw SCSI INQUIRY response</returns>
    public static byte[] Encode(Inquiry? inq)
    {
        if(inq is null)
            return null;

        Inquiry decoded = inq.Value;

        byte[] buffer = new byte[512];
        byte   length = 0;

        buffer[0] =  (byte)(decoded.PeripheralQualifier << 5);
        buffer[0] += decoded.PeripheralDeviceType;

        if(decoded.RMB)
            buffer[1] = 0x80;

        buffer[1] += decoded.DeviceTypeModifier;

        buffer[2] =  (byte)(decoded.ISOVersion  << 6);
        buffer[2] += (byte)(decoded.ECMAVersion << 3);
        buffer[2] += decoded.ANSIVersion;

        if(decoded.AERC)
            buffer[3] = 0x80;

        if(decoded.TrmTsk)
            buffer[3] += 0x40;

        if(decoded.NormACA)
            buffer[3] += 0x20;

        if(decoded.HiSup)
            buffer[3] += 0x10;

        buffer[3] += decoded.ResponseDataFormat;

        if(decoded.AdditionalLength > 0)
        {
            length    = 5;
            buffer[4] = decoded.AdditionalLength;
        }

        if(decoded.SCCS          ||
           decoded.ACC           ||
           decoded.TPGS > 0      ||
           decoded.ThreePC       ||
           decoded.Reserved2 > 0 ||
           decoded.Protect)
        {
            length = 6;

            if(decoded.SCCS)
                buffer[5] = 0x80;

            if(decoded.ACC)
                buffer[5] += 0x40;

            buffer[5] += (byte)(decoded.TPGS << 4);

            if(decoded.ThreePC)
                buffer[5] += 0x08;

            buffer[5] += (byte)(decoded.Reserved2 << 1);

            if(decoded.Protect)
                buffer[5] += 0x01;
        }

        if(decoded.BQue    ||
           decoded.EncServ ||
           decoded.VS1     ||
           decoded.MultiP  ||
           decoded.MChngr  ||
           decoded.ACKREQQ ||
           decoded.Addr32  ||
           decoded.Addr16)
        {
            length = 7;

            if(decoded.BQue)
                buffer[6] = 0x80;

            if(decoded.EncServ)
                buffer[6] += 0x40;

            if(decoded.VS1)
                buffer[6] += 0x20;

            if(decoded.MultiP)
                buffer[6] += 0x10;

            if(decoded.MChngr)
                buffer[6] += 0x08;

            if(decoded.ACKREQQ)
                buffer[6] += 0x04;

            if(decoded.Addr32)
                buffer[6] += 0x02;

            if(decoded.Addr16)
                buffer[6] += 0x01;
        }

        if(decoded.RelAddr ||
           decoded.WBus32  ||
           decoded.WBus16  ||
           decoded.Sync    ||
           decoded.Linked  ||
           decoded.TranDis ||
           decoded.CmdQue  ||
           decoded.SftRe)

        {
            length = 8;

            if(decoded.RelAddr)
                buffer[7] = 0x80;

            if(decoded.WBus32)
                buffer[7] += 0x40;

            if(decoded.WBus16)
                buffer[7] += 0x20;

            if(decoded.Sync)
                buffer[7] += 0x10;

            if(decoded.Linked)
                buffer[7] += 0x08;

            if(decoded.TranDis)
                buffer[7] += 0x04;

            if(decoded.CmdQue)
                buffer[7] += 0x02;

            if(decoded.SftRe)
                buffer[7] += 0x01;
        }

        if(decoded.VendorIdentification != null)
        {
            length = 16;

            Array.Copy(decoded.VendorIdentification, 0, buffer, 8,
                       decoded.VendorIdentification.Length >= 8 ? 8 : decoded.VendorIdentification.Length);
        }

        if(decoded.ProductIdentification != null)
        {
            length = 32;

            Array.Copy(decoded.ProductIdentification, 0, buffer, 16,
                       decoded.ProductIdentification.Length >= 16 ? 16 : decoded.ProductIdentification.Length);
        }

        if(decoded.ProductRevisionLevel != null)
        {
            length = 36;

            Array.Copy(decoded.ProductRevisionLevel, 0, buffer, 32,
                       decoded.ProductRevisionLevel.Length >= 4 ? 4 : decoded.ProductRevisionLevel.Length);
        }

        if(decoded.Seagate_DriveSerialNumber != null)
        {
            length = 44;
            Array.Copy(decoded.Seagate_DriveSerialNumber, 0, buffer, 36, 8);
        }

        if(decoded.KreonIdentifier != null &&
           decoded.KreonVersion    != null)
        {
            length = 46;
            Array.Copy(decoded.KreonIdentifier, 0, buffer, 36, 5);
            buffer[41] = decoded.KreonSpace;
            Array.Copy(decoded.KreonVersion, 0, buffer, 42, 5);
        }

        if(decoded.HP_WORM            ||
           decoded.HP_WORMVersion > 0 ||
           decoded.HP_OBDR        != null)
        {
            length = 49;

            if(decoded.HP_WORM)
                buffer[40] = 0x01;

            buffer[40] += (byte)(decoded.HP_WORMVersion << 1);
            Array.Copy(decoded.HP_OBDR, 0, buffer, 43, 6);
        }

        if(decoded.IsHiMD)
        {
            length = 56;
            Array.Copy(HiMDSignatureContents, 0, buffer, 36, 8);

            if(decoded.HiMDSpecific != null)
                Array.Copy(decoded.HiMDSpecific, 0, buffer, 44, 12);
        }

        if(decoded.VendorSpecific != null &&
           !decoded.IsHiMD)
        {
            length = 56;
            Array.Copy(decoded.VendorSpecific, 0, buffer, 36, 20);
        }

        if(decoded.Reserved3 > 0 ||
           decoded.Clocking  > 0 ||
           decoded.QAS           ||
           decoded.IUS)
        {
            length     =  57;
            buffer[56] =  (byte)(decoded.Reserved3 << 4);
            buffer[56] += (byte)(decoded.Clocking  << 2);

            if(decoded.QAS)
                buffer[56] += 0x02;

            if(decoded.IUS)
                buffer[56] += 0x01;
        }

        if(decoded.Reserved4 != 0)
        {
            length     = 58;
            buffer[57] = decoded.Reserved4;
        }

        if(decoded.VersionDescriptors != null)
        {
            length = (byte)(58 + (decoded.VersionDescriptors.Length * 2));

            for(int i = 0; i < decoded.VersionDescriptors.Length; i++)
                Array.Copy(BitConverter.GetBytes(decoded.VersionDescriptors[i]), 0, buffer, 56 + (i * 2), 2);
        }

        if(decoded.Reserved5 != null)
        {
            length = (byte)(74 + decoded.Reserved5.Length);
            Array.Copy(decoded.Reserved5, 0, buffer, 74, decoded.Reserved5.Length);
        }

        if(decoded.VendorSpecific2 != null)
        {
            length = (byte)(96 + decoded.VendorSpecific2.Length);
            Array.Copy(decoded.VendorSpecific2, 0, buffer, 96, decoded.VendorSpecific2.Length);
        }

        if(decoded.Seagate_Copyright != null)
        {
            length = 144;
            Array.Copy(decoded.Seagate_Copyright, 0, buffer, 96, 48);
        }

        if(decoded.Seagate_ServoPROMPartNo != null)
        {
            length = 148;
            Array.Copy(decoded.Seagate_ServoPROMPartNo, 0, buffer, 144, 4);
        }

        buffer[4] = length;
        byte[] dest = new byte[length];
        Array.Copy(buffer, 0, dest, 0, length);

        return dest;
    }
    #endregion Public methods
}