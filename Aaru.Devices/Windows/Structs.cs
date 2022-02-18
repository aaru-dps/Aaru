// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures necessary for directly interfacing devices under
//     Windows.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Devices.Windows
{
    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct ScsiPassThroughDirect
    {
        public ushort Length;
        public byte   ScsiStatus;
        public byte   PathId;
        public byte   TargetId;
        public byte   Lun;
        public byte   CdbLength;
        public byte   SenseInfoLength;
        [MarshalAs(UnmanagedType.U1)]
        public ScsiIoctlDirection DataIn;
        public uint   DataTransferLength;
        public uint   TimeOutValue;
        public IntPtr DataBuffer;
        public uint   SenseInfoOffset;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Cdb;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ScsiPassThroughDirectAndSenseBuffer
    {
        public ScsiPassThroughDirect sptd;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] SenseBuf;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct AtaPassThroughDirect
    {
        /// <summary>Length in bytes of this structure</summary>
        public ushort Length;

        /// <summary>Indicates transfer direction and kind of operation</summary>
        [MarshalAs(UnmanagedType.U2)]
        public AtaFlags AtaFlags;

        /// <summary>Indicates IDE port or bus, set by driver</summary>
        public byte PathId;

        /// <summary>Indicates target device on bus, set by driver</summary>
        public byte TargetId;

        /// <summary>Indicates logical unit number of device, set by driver</summary>
        public byte Lun;

        /// <summary>Reserved</summary>
        public byte ReservedAsUchar;

        /// <summary>Data transfer length in bytes</summary>
        public uint DataTransferLength;

        /// <summary>Timeout value in seconds</summary>
        public uint TimeOutValue;

        /// <summary>Reserved</summary>
        public uint ReservedAsUlong;

        /// <summary>Pointer to data buffer</summary>
        public IntPtr DataBuffer;

        /// <summary>Previous ATA registers, for LBA48</summary>
        public AtaTaskFile PreviousTaskFile;

        /// <summary>ATA registers</summary>
        public AtaTaskFile CurrentTaskFile;
    }

    [StructLayout(LayoutKind.Explicit), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct AtaTaskFile
    {
        // Fields for commands sent
        [FieldOffset(0)]
        public byte Features;
        [FieldOffset(6)]
        public byte Command;

        // Fields on command return
        [FieldOffset(0)]
        public byte Error;
        [FieldOffset(6)]
        public byte Status;

        // Common fields
        [FieldOffset(1)]
        public byte SectorCount;
        [FieldOffset(2)]
        public byte SectorNumber;
        [FieldOffset(3)]
        public byte CylinderLow;
        [FieldOffset(4)]
        public byte CylinderHigh;
        [FieldOffset(5)]
        public byte DeviceHead;
        [FieldOffset(7)]
        public byte Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoragePropertyQuery
    {
        [MarshalAs(UnmanagedType.U4)]
        public StoragePropertyId PropertyId;
        [MarshalAs(UnmanagedType.U4)]
        public StorageQueryType QueryType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] AdditionalParameters;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct StorageDescriptorHeader
    {
        public uint Version;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StorageDeviceDescriptor
    {
        public uint Version;
        public uint Size;
        public byte DeviceType;
        public byte DeviceTypeModifier;
        [MarshalAs(UnmanagedType.U1)]
        public bool RemovableMedia;
        [MarshalAs(UnmanagedType.U1)]
        public bool CommandQueueing;
        public int            VendorIdOffset;
        public int            ProductIdOffset;
        public int            ProductRevisionOffset;
        public int            SerialNumberOffset;
        public StorageBusType BusType;
        public uint           RawPropertiesLength;
        public byte[]         RawDeviceProperties;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IdePassThroughDirect
    {
        /// <summary>ATA registers</summary>
        public AtaTaskFile CurrentTaskFile;

        /// <summary>Size of data buffer</summary>
        public uint DataBufferSize;

        /// <summary>Data buffer</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] DataBuffer;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct StorageDeviceNumber
    {
        public int deviceType;
        public int deviceNumber;
        public int partitionNumber;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct DeviceInfoData
    {
        public int    cbSize;
        public Guid   classGuid;
        public uint   devInst;
        public IntPtr reserved;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct DeviceInterfaceData
    {
        public   int    cbSize;
        public   Guid   interfaceClassGuid;
        public   uint   flags;
        readonly IntPtr reserved;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct UsbSetupPacket
    {
        public byte  bmRequest;
        public byte  bRequest;
        public short wValue;
        public short wIndex;
        public short wLength;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct UsbDescriptorRequest
    {
        public int ConnectionIndex;

        public UsbSetupPacket SetupPacket;

        //public byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct SffdiskQueryDeviceProtocolData
    {
        public ushort size;
        public ushort reserved;
        public Guid   protocolGuid;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct SffdiskDeviceCommandData
    {
        public ushort      size;
        public ushort      reserved;
        public SffdiskDcmd command;
        public ushort      protocolArgumentSize;
        public uint        deviceDataBufferSize;
        public uint        information;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SdCmdDescriptor
    {
        public byte                commandCode;
        public SdCommandClass      cmdClass;
        public SdTransferDirection transferDirection;
        public SdTransferType      transferType;
        public SdResponseType      responseType;
    }
}