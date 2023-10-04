// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ata48.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains 48-bit LBA ATA commands.
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

using System;
using Aaru.Console;
using Aaru.Decoders.ATA;

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Gets native max address using 48-bit addressing</summary>
    /// <param name="lba">Maximum addressable block</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool GetNativeMaxAddressExt(out ulong  lba, out AtaErrorRegistersLba48 statusRegisters, uint timeout,
                                       out double duration)
    {
        lba = 0;
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba48
        {
            Command = (byte)AtaCommands.NativeMaxAddress,
            Feature = 0x0000
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        if((statusRegisters.Status & 0x23) == 0)
        {
            lba = (ulong)((statusRegisters.LbaHighCurrent << 16) +
                          (statusRegisters.LbaMidCurrent  << 8)  +
                          statusRegisters.LbaLowCurrent);

            lba <<= 24;

            lba += (ulong)((statusRegisters.LbaHighPrevious << 16) +
                           (statusRegisters.LbaMidPrevious  << 8)  +
                           statusRegisters.LbaLowPrevious);
        }

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.GET_NATIVE_MAX_ADDRESS_EXT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads sectors using 48-bit addressing and DMA transfer</summary>
    /// <param name="buffer">Buffer that contains the read data</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="lba">LBA of read start</param>
    /// <param name="count">How many blocks to read, or 0 to indicate 65536 blocks</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadDma(out byte[] buffer,  out AtaErrorRegistersLba48 statusRegisters, ulong lba, ushort count,
                        uint       timeout, out double                 duration)
    {
        buffer = count == 0 ? new byte[512 * 65536] : new byte[512 * count];

        var registers = new AtaRegistersLba48
        {
            Command         = (byte)AtaCommands.ReadDmaExt,
            SectorCount     = count,
            LbaHighPrevious = (byte)((lba & 0xFF0000000000) / 0x10000000000),
            LbaMidPrevious  = (byte)((lba & 0xFF00000000)   / 0x100000000),
            LbaLowPrevious  = (byte)((lba & 0xFF000000)     / 0x1000000),
            LbaHighCurrent  = (byte)((lba & 0xFF0000)       / 0x10000),
            LbaMidCurrent   = (byte)((lba & 0xFF00)         / 0x100),
            LbaLowCurrent   = (byte)(lba & 0xFF)
        };

        registers.DeviceHead += 0x40;

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                   ref buffer, timeout,             true,            out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.READ_DMA_EXT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads a drive log using PIO transfer</summary>
    /// <param name="buffer">Buffer that contains the read data</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="logAddress">Log address</param>
    /// <param name="pageNumber">Log page number</param>
    /// <param name="count">How log blocks to read</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadLog(out byte[] buffer,     out AtaErrorRegistersLba48 statusRegisters, byte logAddress,
                        ushort     pageNumber, ushort                     count, uint timeout, out double duration)
    {
        buffer = new byte[512 * count];

        var registers = new AtaRegistersLba48
        {
            Command        = (byte)AtaCommands.ReadLogExt,
            SectorCount    = count,
            LbaMidCurrent  = (byte)(pageNumber & 0xFF),
            LbaMidPrevious = (byte)((pageNumber & 0xFF00) / 0x100),
            LbaLowCurrent  = logAddress
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                   ref buffer, timeout,             true,              out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.READ_LOG_EXT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads a drive log using DMA transfer</summary>
    /// <param name="buffer">Buffer that contains the read data</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="logAddress">Log address</param>
    /// <param name="pageNumber">Log page number</param>
    /// <param name="count">How log blocks to read</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadLogDma(out byte[] buffer,     out AtaErrorRegistersLba48 statusRegisters, byte logAddress,
                           ushort     pageNumber, ushort                     count, uint timeout, out double duration)
    {
        buffer = new byte[512 * count];

        var registers = new AtaRegistersLba48
        {
            Command        = (byte)AtaCommands.ReadLogDmaExt,
            SectorCount    = count,
            LbaMidCurrent  = (byte)(pageNumber & 0xFF),
            LbaMidPrevious = (byte)((pageNumber & 0xFF00) / 0x100),
            LbaLowCurrent  = logAddress
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                   ref buffer, timeout,             true,            out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.READ_LOG_DMA_EXT_took_0_ms, duration);

        return sense;
    }

    /// <summary>
    ///     Reads sectors using 48-bit addressing and PIO transfer, sending an interrupt only after all the sectors have
    ///     been transferred
    /// </summary>
    /// <param name="buffer">Buffer that contains the read data</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="lba">LBA of read start</param>
    /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadMultiple(out byte[] buffer,  out AtaErrorRegistersLba48 statusRegisters, ulong lba, ushort count,
                             uint       timeout, out double                 duration)
    {
        buffer = count == 0 ? new byte[512 * 65536] : new byte[512 * count];

        var registers = new AtaRegistersLba48
        {
            Command         = (byte)AtaCommands.ReadMultipleExt,
            SectorCount     = count,
            LbaHighPrevious = (byte)((lba & 0xFF0000000000) / 0x10000000000),
            LbaMidPrevious  = (byte)((lba & 0xFF00000000)   / 0x100000000),
            LbaLowPrevious  = (byte)((lba & 0xFF000000)     / 0x1000000),
            LbaHighCurrent  = (byte)((lba & 0xFF0000)       / 0x10000),
            LbaMidCurrent   = (byte)((lba & 0xFF00)         / 0x100),
            LbaLowCurrent   = (byte)(lba & 0xFF)
        };

        registers.DeviceHead += 0x40;

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                   ref buffer, timeout,             true,              out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.READ_MULTIPLE_EXT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads native max address using 48-bit addressing</summary>
    /// <param name="lba">Maximum addressable block</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadNativeMaxAddress(out ulong  lba, out AtaErrorRegistersLba48 statusRegisters, uint timeout,
                                     out double duration)
    {
        lba = 0;
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba48
        {
            Command = (byte)AtaCommands.ReadNativeMaxAddressExt
        };

        registers.DeviceHead += 0x40;

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        if((statusRegisters.Status & 0x23) == 0)
        {
            lba = (ulong)((statusRegisters.LbaHighCurrent << 16) +
                          (statusRegisters.LbaMidCurrent  << 8)  +
                          statusRegisters.LbaLowCurrent);

            lba <<= 24;

            lba += (ulong)((statusRegisters.LbaHighPrevious << 16) +
                           (statusRegisters.LbaMidPrevious  << 8)  +
                           statusRegisters.LbaLowPrevious);
        }

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.READ_NATIVE_MAX_ADDRESS_EXT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads sectors using 48-bit addressing and PIO transfer</summary>
    /// <param name="buffer">Buffer that contains the read data</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="lba">LBA of read start</param>
    /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool Read(out byte[] buffer,  out AtaErrorRegistersLba48 statusRegisters, ulong lba, ushort count,
                     uint       timeout, out double                 duration)
    {
        buffer = count == 0 ? new byte[512 * 65536] : new byte[512 * count];

        var registers = new AtaRegistersLba48
        {
            Command         = (byte)AtaCommands.ReadExt,
            SectorCount     = count,
            LbaHighPrevious = (byte)((lba & 0xFF0000000000) / 0x10000000000),
            LbaMidPrevious  = (byte)((lba & 0xFF00000000)   / 0x100000000),
            LbaLowPrevious  = (byte)((lba & 0xFF000000)     / 0x1000000),
            LbaHighCurrent  = (byte)((lba & 0xFF0000)       / 0x10000),
            LbaMidCurrent   = (byte)((lba & 0xFF00)         / 0x100),
            LbaLowCurrent   = (byte)(lba & 0xFF)
        };

        registers.DeviceHead += 0x40;

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                   ref buffer, timeout,             true,              out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.READ_SECTORS_EXT_took_0_ms, duration);

        return sense;
    }
}