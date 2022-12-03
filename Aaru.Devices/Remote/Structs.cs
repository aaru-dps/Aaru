// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Remote.
//
// --[ Description ] ----------------------------------------------------------
//
//     Structures for the Aaru Remote protocol.
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

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable IdentifierTypo

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.Decoders.ATA;

namespace Aaru.Devices.Remote;

/// <summary>Header for any Aaru remote packet</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketHeader
{
    /// <summary>Unique Aaru packet identifier (primary)</summary>
    public uint remote_id;
    /// <summary>Unique Aaru packet identifier (secondary)</summary>
    public uint packet_id;
    /// <summary>Packet length</summary>
    public uint len;
    /// <summary>Packet version</summary>
    public byte version;
    /// <summary>Unique Aaru packet type identifier</summary>
    public AaruPacketType packetType;
    /// <summary>Spare for expansion (or alignment)</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public readonly byte[] spare;
}

/// <summary>Hello packet, identifies a remote initiator with a responder</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketHello
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Application name</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string application;
    /// <summary>Application version</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string version;
    /// <summary>Maximum supported protocol version</summary>
    public byte maxProtocol;
    /// <summary>Spare for expansion (or alignment)</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public readonly byte[] spare;
    /// <summary>Operating system name</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string sysname;
    /// <summary>Operating system version / release</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string release;
    /// <summary>Operating system machine / architecture</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string machine;
}

/// <summary>Request a list of device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCommandListDevices
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns the requested list of devices</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public readonly struct AaruPacketResponseListDevices
{
    /// <summary>Packet header</summary>
    public readonly AaruPacketHeader hdr;
    /// <summary>How many device descriptors follows this structure in the packet</summary>
    public readonly ushort devices;
}

/// <summary>Sends a request or returns a response that requires no intervention or further processing</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketNop
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Reason code</summary>
    public AaruNopReason reasonCode;
    /// <summary>Spare for expansion (or alignment)</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public readonly byte[] spare;
    /// <summary>Reason name</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string reason;
    /// <summary>Operating system error number</summary>
    public int errno;
}

/// <summary>Requests to open a device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCommandOpenDevice
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Device path</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public string device_path;
}

/// <summary>
///     Requests remote to send a command to a SCSI device. This header is followed by the CDB and after it comes the
///     buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdScsi
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the CDB that follows this structure</summary>
    public uint cdb_len;
    /// <summary>Length in bytes of the buffer that follows the CDB</summary>
    public uint buf_len;
    /// <summary>Direction of SCSI data transfer</summary>
    public int direction;
    /// <summary>Timeout waiting for device to respond to command</summary>
    public uint timeout;
}

/// <summary>
///     Returns the response from a command sent to a SCSI device. This structure is followed by the buffer containing
///     the REQUEST SENSE response and this is followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResScsi
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Size the REQUEST SENSE buffer that follows this structure</summary>
    public uint sense_len;
    /// <summary>Length in bytes of the data buffer that follows the sense buffer</summary>
    public uint buf_len;
    /// <summary>Time in milliseconds it took for the device to execute the command</summary>
    public uint duration;
    /// <summary>Set to anything different of zero if there was a SENSE returned</summary>
    public uint sense;
    /// <summary>Set to the remote operating system error number</summary>
    public uint error_no;
}

/// <summary>
///     Requests remote to send a command to an ATA device using the CHS command set. This header is followed by the
///     data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdAtaChs
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Registers to set in the ATA device</summary>
    public AtaRegistersChs registers;
    /// <summary>ATA protocol code</summary>
    public byte protocol;
    /// <summary>ATA transfer register indicator</summary>
    public byte transferRegister;
    /// <summary>Set to <c>true</c> to transfer blocks, <c>false</c> to transfer bytes</summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool transferBlocks;
    /// <summary>Spare for expansion (or alignment)</summary>
    public byte spare;
    /// <summary>Timeout waiting for device to respond to command</summary>
    public uint timeout;
}

/// <summary>
///     Returns the response from a command sent to an ATA device using the CHS command set. This structure is
///     followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResAtaChs
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Registers as set back by the ATA device</summary>
    public AtaErrorRegistersChs registers;
    /// <summary>Time in milliseconds it took for the device to execute the command</summary>
    public uint duration;
    /// <summary>Set to anything different of zero if the device set an error condition</summary>
    public uint sense;
    /// <summary>Set to the remote operating system error number</summary>
    public uint error_no;
}

/// <summary>
///     Requests remote to send a command to an ATA device using the 28-bit command set. This header is followed by
///     the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdAtaLba28
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Registers to set in the ATA device</summary>
    public AtaRegistersLba28 registers;
    /// <summary>ATA protocol code</summary>
    public byte protocol;
    /// <summary>ATA transfer register indicator</summary>
    public byte transferRegister;
    /// <summary>Set to <c>true</c> to transfer blocks, <c>false</c> to transfer bytes</summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool transferBlocks;
    /// <summary>Spare for expansion (or alignment)</summary>
    public byte spare;
    /// <summary>Timeout waiting for device to respond to command</summary>
    public uint timeout;
}

/// <summary>
///     Returns the response from a command sent to an ATA device using the 28-bit LBA command set. This structure is
///     followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResAtaLba28
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Registers as set back by the ATA device</summary>
    public AtaErrorRegistersLba28 registers;
    /// <summary>Time in milliseconds it took for the device to execute the command</summary>
    public uint duration;
    /// <summary>Set to anything different of zero if the device set an error condition</summary>
    public uint sense;
    /// <summary>Set to the remote operating system error number</summary>
    public uint error_no;
}

/// <summary>
///     Requests remote to send a command to an ATA device using the 48-bit command set. This header is followed by
///     the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdAtaLba48
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Registers to set in the ATA device</summary>
    public AtaRegistersLba48 registers;
    /// <summary>ATA protocol code</summary>
    public byte protocol;
    /// <summary>ATA transfer register indicator</summary>
    public byte transferRegister;
    /// <summary>Set to <c>true</c> to transfer blocks, <c>false</c> to transfer bytes</summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool transferBlocks;
    /// <summary>Spare for expansion (or alignment)</summary>
    public byte spare;
    /// <summary>Timeout waiting for device to respond to command</summary>
    public uint timeout;
}

/// <summary>
///     Returns the response from a command sent to an ATA device using the 48-bit LBA command set. This structure is
///     followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResAtaLba48
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Registers as set back by the ATA device</summary>
    public AtaErrorRegistersLba48 registers;
    /// <summary>Time in milliseconds it took for the device to execute the command</summary>
    public uint duration;
    /// <summary>Set to anything different of zero if the device set an error condition</summary>
    public uint sense;
    /// <summary>Set to the remote operating system error number</summary>
    public uint error_no;
}

/// <summary>SecureDigital or MultiMediaCard command description</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruCmdSdhci
{
    /// <summary>Command</summary>
    public MmcCommands command;
    /// <summary>Set to <c>true</c> if the command writes to the device, <c>false</c> otherwise</summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool write;
    /// <summary>Set to <c>true</c> if it is an application command, <c>false</c> otherwise</summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool application;
    /// <summary>Flags</summary>
    public MmcFlags flags;
    /// <summary>Argument</summary>
    public uint argument;
    /// <summary>Block size</summary>
    public uint block_size;
    /// <summary>Number of blocks to transfer</summary>
    public uint blocks;
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;
    /// <summary>Timeout waiting for device to respond to command</summary>
    public uint timeout;
}

/// <summary>
///     Requests remote to send a command to a SecureDigital or MultiMediaCard device attached using a SDHCI
///     controller. This structure is followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdSdhci
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>SecureDigital or MultiMediaCard command description</summary>
    public AaruCmdSdhci command;
}

/// <summary>
///     Returns the response from a command sent to a SecureDigital or MultiMediaCard device attached to a SDHCI
///     controller. This structure is followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruResSdhci
{
    /// <summary>Length in bytes of the data buffer</summary>
    public uint buf_len;

    /// <summary>Response registers</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] response;

    /// <summary>Time in milliseconds it took for the device to execute the command</summary>
    public uint duration;
    /// <summary>Set to anything different of zero if the device set an error condition</summary>
    public uint sense;
    /// <summary>Set to the remote operating system error number</summary>
    public uint error_no;
}

/// <summary>
///     Returns the response from a command sent to a SecureDigital or MultiMediaCard device attached to a SDHCI
///     controller. This structure is followed by the data buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResSdhci
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Response</summary>
    public AaruResSdhci res;
}

/// <summary>Requests the Aaru device type for the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdGetDeviceType
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns the Aaru device type for the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResGetDeviceType
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Aaru's device type</summary>
    public DeviceType device_type;
}

/// <summary>Requests the registers of a SecureDigital or MultiMediaCard attached to an SDHCI controller</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdGetSdhciRegisters
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns the registers of a SecureDigital or MultiMediaCard attached to an SDHCI controller</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResGetSdhciRegisters
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>
    ///     <c>true</c> if the device is attached to an SDHCI controller and the rest of the fields on this packet are
    ///     valid, <c>false</c> otherwise
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool isSdhci;
    /// <summary>CSD registers</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] csd;
    /// <summary>CID registers</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] cid;
    /// <summary>OCR registers</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] ocr;
    /// <summary>SCR registers</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] scr;
    /// <summary>Length of the CSD registers</summary>
    public uint csd_len;
    /// <summary>Length of the CID registers</summary>
    public uint cid_len;
    /// <summary>Length of the OCR registers</summary>
    public uint ocr_len;
    /// <summary>Length of the SCR registers</summary>
    public uint scr_len;
}

/// <summary>Requests information about the USB connection of the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdGetUsbData
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns information about the USB connection of the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResGetUsbData
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>
    ///     <c>true</c> if the device is attached using USB and the rest of the fields on this packet are valid,
    ///     <c>false</c> otherwise
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool isUsb;
    /// <summary>Length of the descriptors</summary>
    public ushort descLen;
    /// <summary>Raw USB descriptors</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
    public byte[] descriptors;
    /// <summary>USB vendor ID</summary>
    public ushort idVendor;
    /// <summary>USB product ID</summary>
    public ushort idProduct;
    /// <summary>USB manufacturer string</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string manufacturer;
    /// <summary>USB product string</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string product;
    /// <summary>USB serial number string</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string serial;
}

/// <summary>Requests information about the FireWire connection of the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdGetFireWireData
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns information about the FireWire connection of the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResGetFireWireData
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>
    ///     <c>true</c> if the device is attached using FireWire and the rest of the fields on this packet are valid,
    ///     <c>false</c> otherwise
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool isFireWire;
    /// <summary>FireWire model ID</summary>
    public uint idModel;
    /// <summary>FireWire vendor ID</summary>
    public uint idVendor;
    /// <summary>FireWire's device GUID</summary>
    public ulong guid;
    /// <summary>FireWire vendor string</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string vendor;
    /// <summary>FireWire model string</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string model;
}

/// <summary>Requests information about the PCMCIA or CardBus connection of the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdGetPcmciaData
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns information about the PCMCIA or CardBus connection of the opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResGetPcmciaData
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>
    ///     <c>true</c> if the device is a PCMCIA or CardBus device and the rest of the fields on this packet are valid,
    ///     <c>false</c> otherwise
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool isPcmcia;
    /// <summary>CIS buffer length</summary>
    public ushort cis_len;
    /// <summary>CIS buffer</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
    public byte[] cis;
}

/// <summary>Requests to close the currently opened device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdClose
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Requests to know if the remote is running with administrative (aka root) privileges</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdAmIRoot
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Returns if the remote is running with administrative (aka root) privileges</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResAmIRoot
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Set to any value different of 0 to indicate the remote is running with administrative (aka root) privileges</summary>
    public uint am_i_root;
}

/// <summary>Initiates a multiple command block with the SDHCI controller the SecureDigital or MultiMediaCard is attached</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketMultiCmdSdhci
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>How many commands to queue</summary>
    public ulong cmd_count;
}

/// <summary>Closes and then re-opens the same device</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdReOpen
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
}

/// <summary>Reads data using operating system buffers</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketCmdOsRead
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Device offset where to read</summary>
    public ulong offset;
    /// <summary>Number of bytes to read</summary>
    public uint length;
}

/// <summary>Returns data read using operating system buffers. This structure is followed by the data buffer.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AaruPacketResOsRead
{
    /// <summary>Packet header</summary>
    public AaruPacketHeader hdr;
    /// <summary>Set to the remote operating system error number</summary>
    public int errno;
    /// <summary>Time in milliseconds it took for the device to execute the command</summary>
    public uint duration;
}