// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains PCMCIA enumerations.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Decoders.PCMCIA;

/// <summary>Tuple codes.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TupleCodes : byte
{
    /// <summary>Checksum control</summary>
    CISTPL_CHECKSUM = 0x10,
    /// <summary>End-of-chain</summary>
    CISTPL_END = 0xFF,
    /// <summary>Indirect access PC Card memory</summary>
    CISTPL_INDIRECT = 0x03,
    /// <summary>Link-target-control</summary>
    CISTPL_LINKTARGET = 0x13,
    /// <summary>Longlink to attribute memory</summary>
    CISTPL_LONGLINK_A = 0x11,
    /// <summary>Longlink to common memory</summary>
    CISTPL_LONGLINK_C = 0x12,
    /// <summary>Longlink to next chain on a Cardbus PC Card</summary>
    CISTPL_LONGLINK_CB = 0x02,
    /// <summary>Longlink to function specific chains</summary>
    CISTPL_LONGLINK_MFC = 0x06,
    /// <summary>No-link to common memory</summary>
    CISTPL_NO_LINK = 0x14,
    /// <summary>Null tuple</summary>
    CISTPL_NULL = 0x00,
    /// <summary>Alternate language string</summary>
    CISTPL_ALTSTR = 0x16,
    /// <summary>Common memory device information</summary>
    CISTPL_DEVICE = 0x01,
    /// <summary>Attribute memory device information</summary>
    CISTPL_DEVICE_A = 0x17,
    /// <summary>Other operating conditions information for attribute memory</summary>
    CISTPL_DEVICE_OA = 0x1D,
    /// <summary>Other operating conditions information for common memory</summary>
    CISTPL_DEVICE_OC = 0x1C,
    /// <summary>Device geometry information for common memory</summary>
    CISTPL_DEVICEGEO = 0x1E,
    /// <summary>Device geometry information for attribute memory</summary>
    CISTPL_DEVICEGEO_A = 0x1F,
    /// <summary>Extended common memory device information</summary>
    CISTPL_EXTDEVIC = 0x09,
    /// <summary>Function extensions</summary>
    CISTPL_FUNCE = 0x22,
    /// <summary>Function class identification</summary>
    CISTPL_FUNCID = 0x21,
    /// <summary>JEDEC programming information for attribute memory</summary>
    CISTPL_JEDEC_A = 0x19,
    /// <summary>JEDEC programming information for common memory</summary>
    CISTPL_JEDEC_C = 0x18,
    /// <summary>Manufacturer identification string</summary>
    CISTPL_MANFID = 0x20,
    /// <summary>Level 1 version/product information</summary>
    CISTPL_VERS_1 = 0x15,
    /// <summary>BAR for a CardBus PC Card</summary>
    CISTPL_BAR = 0x07,
    /// <summary>Configuration-table-entry</summary>
    CISTPL_CFTABLE_ENTRY = 0x1B,
    /// <summary>Configuration-table-entry for a CardBus PC Card</summary>
    CISTPL_CFTABLE_ENTRY_CB = 0x05,
    /// <summary>Configuration tuple for a 16-bit PC Card</summary>
    CISTPL_CONFIG = 0x1A,
    /// <summary>Configuration tuple for a CardBus PC Card</summary>
    CISTPL_CONFIG_CB = 0x04,
    /// <summary>Function state save/restore definition</summary>
    CISTPL_PWR_MGMNT = 0x08,
    /// <summary>Battery replacement date</summary>
    CISTPL_BATTERY = 0x45,
    /// <summary>Card initialization date</summary>
    CISTPL_DATE = 0x44,
    /// <summary>Level 2 version/product information</summary>
    CISTPL_VERS_2 = 0x40,
    /// <summary>Byte ordering for disk-like partitions</summary>
    CISTPL_BYTEORDER = 0x43,
    /// <summary>Data recording format for common memory</summary>
    CISTPL_FORMAT = 0x41,
    /// <summary>Data recording format for attribute memory</summary>
    CISTPL_FORMAT_A = 0x47,
    /// <summary>Partition geometry</summary>
    CISTPL_GEOMETRY = 0x42,
    /// <summary>Software interleaving</summary>
    CISTPL_SWIL = 0x23,
    /// <summary>Partition organization</summary>
    CISTPL_ORG = 0x46,
    /// <summary>Special purpose</summary>
    CISTPL_SPCL = 0x90
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DeviceTypeCodes : byte
{
    /// <summary>No device, used to designate a hole</summary>
    DTYPE_NULL = 0,
    /// <summary>Masked ROM</summary>
    DTYPE_ROM = 1,
    /// <summary>One-type-programmable ROM</summary>
    DTYPE_OTPROM = 2,
    /// <summary>UV-Erasable Programmable ROM</summary>
    DTYPE_EPROM = 3,
    /// <summary>Electronically-Erasable Programmable ROM</summary>
    DTYPE_EEPROM = 4,
    /// <summary>Flash memory</summary>
    DTYPE_FLASH = 5,
    /// <summary>Static RAM</summary>
    DTYPE_SRAM = 6,
    /// <summary>Dynamic RAM</summary>
    DTYPE_DRAM = 7,
    /// <summary>Function-specific memory address range</summary>
    DTYPE_FUNCSPEC = 13,
    /// <summary>Extended type follows</summary>
    DTYPE_EXTEND = 14
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DeviceSpeedCodes : byte
{
    /// <summary>No device</summary>
    DSPEED_NULL = 0,
    /// <summary>250 ns</summary>
    DSPEED_250NS = 1,
    /// <summary>200 ns</summary>
    DSPEED_200NS = 2,
    /// <summary>150 ns</summary>
    DSPEED_150NS = 3,
    /// <summary>100 ns</summary>
    DSPEED_100NS = 4,
    /// <summary>Extended speed follows</summary>
    DSPEED_EXT = 7
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum FunctionCodes : byte
{
    MultiFunction  = 0x00, Memory     = 0x01, Serial          = 0x02,
    Parallel       = 0x03, FixedDisk  = 0x04, Video           = 0x05,
    Network        = 0x06, AIMS       = 0x07, SCSI            = 0x08,
    Security       = 0x09, Instrument = 0x0A, HighSpeedSerial = 0x0B,
    VendorSpecific = 0xFE
}