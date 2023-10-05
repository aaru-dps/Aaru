// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MCPT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains Media Card Pass-Thru commands.
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
using System.Diagnostics.CodeAnalysis;
using Aaru.Console;
using Aaru.Decoders.ATA;

// ReSharper disable UnusedMember.Global

namespace Aaru.Devices;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
[SuppressMessage("ReSharper", "OutParameterValueIsAlwaysDiscarded.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public partial class Device
{
    /// <summary>Enables media card pass through</summary>
    /// <param name="statusRegisters">Status registers.</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="duration">Time it took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool EnableMediaCardPassThrough(out AtaErrorRegistersChs statusRegisters, uint timeout,
                                           out double               duration) =>
        CheckMediaCardType(1, out statusRegisters, timeout, out duration);

    /// <summary>Disables media card pass through</summary>
    /// <param name="statusRegisters">Status registers.</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="duration">Time it took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool DisableMediaCardPassThrough(out AtaErrorRegistersChs statusRegisters, uint timeout,
                                            out double               duration) =>
        CheckMediaCardType(0, out statusRegisters, timeout, out duration);

    /// <summary>Checks media card pass through</summary>
    /// <param name="feature">Feature</param>
    /// <param name="statusRegisters">Status registers.</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="duration">Time it took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool CheckMediaCardType(byte       feature, out AtaErrorRegistersChs statusRegisters, uint timeout,
                                   out double duration)
    {
        byte[] buffer = Array.Empty<byte>();

        var registers = new AtaRegistersChs
        {
            Command = (byte)AtaCommands.CheckMediaCardType,
            Feature = feature
        };

        LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                   ref buffer, timeout,             false,               out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(ATA_MODULE_NAME, Localization.CHECK_MEDIA_CARD_TYPE_took_0_ms, duration);

        return sense;
    }
}