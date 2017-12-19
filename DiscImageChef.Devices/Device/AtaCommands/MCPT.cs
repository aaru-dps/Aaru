// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool EnableMediaCardPassThrough(out AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            return CheckMediaCardType(1, out statusRegisters, timeout, out duration);
        }

        public bool DisableMediaCardPassThrough(out AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            return CheckMediaCardType(0, out statusRegisters, timeout, out duration);
        }

        public bool CheckMediaCardType(byte feature, out AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersCHS registers = new AtaRegistersCHS();
            bool sense;

            registers.command = (byte)AtaCommands.CheckMediaCardType;
            registers.feature = feature;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "CHECK MEDIA CARD TYPE took {0} ms.", duration);

            return sense;
        }
    }
}

