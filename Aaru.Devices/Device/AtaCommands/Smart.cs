// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Smart.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains S.M.A.R.T. commands.
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
    /// <summary>Disables S.M.A.R.T.</summary>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartDisable(out AtaErrorRegistersLba28 statusRegisters, uint timeout, out double duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.Disable,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_DISABLE_OPERATIONS_took_0_ms, duration);

        return sense;
    }

    /// <summary>Enables auto-saving of S.M.A.R.T. attributes</summary>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartEnableAttributeAutosave(out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                             out double                 duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba28
        {
            Command     = (byte)AtaCommands.Smart,
            Feature     = (byte)AtaSmartSubCommands.EnableDisableAttributeAutosave,
            LbaHigh     = 0xC2,
            LbaMid      = 0x4F,
            SectorCount = 0xF1
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_ENABLE_ATTRIBUTE_AUTOSAVE_took_0_ms, duration);

        return sense;
    }

    /// <summary>Disables auto-saving of S.M.A.R.T. attributes</summary>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartDisableAttributeAutosave(out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                              out double                 duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.EnableDisableAttributeAutosave,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_DISABLE_ATTRIBUTE_AUTOSAVE_took_0_ms, duration);

        return sense;
    }

    /// <summary>Enables S.M.A.R.T.</summary>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartEnable(out AtaErrorRegistersLba28 statusRegisters, uint timeout, out double duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.Enable,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_ENABLE_OPERATIONS_took_0_ms, duration);

        return sense;
    }

    /// <summary>Requests drive to execute offline immediate S.M.A.R.T. test</summary>
    /// <param name="subcommand">Subcommand</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartExecuteOffLineImmediate(out AtaErrorRegistersLba28 statusRegisters, byte subcommand, uint timeout,
                                             out double                 duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.ExecuteOfflineImmediate,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F,
            LbaLow  = subcommand
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_EXECUTE_OFF_LINE_IMMEDIATE_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads S.M.A.R.T. data</summary>
    /// <param name="buffer">Buffer containing data</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartReadData(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                              out double duration)
    {
        buffer = new byte[512];

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.ReadData,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,             out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_READ_DATA_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads S.M.A.R.T. log</summary>
    /// <param name="buffer">Buffer containing log</param>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="logAddress">Log address</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartReadLog(out byte[] buffer,  out AtaErrorRegistersLba28 statusRegisters, byte logAddress,
                             uint       timeout, out double                 duration)
    {
        buffer = new byte[512];

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.ReadLog,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F,
            LbaLow  = logAddress
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,             out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_READ_LOG_took_0_ms, duration);

        return sense;
    }

    /// <summary>Retrieves S.M.A.R.T. status</summary>
    /// <param name="statusRegisters">Returned status registers</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SmartReturnStatus(out AtaErrorRegistersLba28 statusRegisters, uint timeout, out double duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersLba28
        {
            Command = (byte)AtaCommands.Smart,
            Feature = (byte)AtaSmartSubCommands.ReturnStatus,
            LbaHigh = 0xC2,
            LbaMid  = 0x4F
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.SMART_RETURN_STATUS_took_0_ms, duration);

        return sense;
    }
}