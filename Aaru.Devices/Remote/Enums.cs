// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Remote.
//
// --[ Description ] ----------------------------------------------------------
//
//     Enumerations for the Aaru Remote protocol.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Devices.Remote
{
    public enum AaruPacketType : sbyte
    {
        Nop                       = -1, Hello                   = 1, CommandListDevices        = 2,
        ResponseListDevices       = 3, CommandOpen              = 4, CommandScsi               = 5,
        ResponseScsi              = 6, CommandAtaChs            = 7, ResponseAtaChs            = 8,
        CommandAtaLba28           = 9, ResponseAtaLba28         = 10, CommandAtaLba48          = 11,
        ResponseAtaLba48          = 12, CommandSdhci            = 13, ResponseSdhci            = 14,
        CommandGetType            = 15, ResponseGetType         = 16, CommandGetSdhciRegisters = 17,
        ResponseGetSdhciRegisters = 18, CommandGetUsbData       = 19, ResponseGetUsbData       = 20,
        CommandGetFireWireData    = 21, ResponseGetFireWireData = 22, CommandGetPcmciaData     = 23,
        ResponseGetPcmciaData     = 24, CommandCloseDevice      = 25, CommandAmIRoot           = 26,
        ResponseAmIRoot           = 27, MultiCommandSdhci       = 28, ResponseMultiSdhci       = 29,
        CommandReOpenDevice       = 30, CommandOsRead           = 31, ResponseOsRead           = 32
    }

    public enum AaruNopReason : byte
    {
        OutOfOrder       = 0, NotImplemented = 1, NotRecognized = 2,
        ErrorListDevices = 3, OpenOk         = 4, OpenError     = 5,
        ReOpenOk         = 6, CloseError     = 7
    }
}