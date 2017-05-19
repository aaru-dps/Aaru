// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool ReadCSD(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[16];
            bool sense = false;

            lastError = SendMmcCommand(MmcCommands.SendCSD, false, false, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       0, 16, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SEND_CSD took {0} ms.", duration);

            return sense;
        }

        public bool ReadCID(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[16];
            bool sense = false;

            lastError = SendMmcCommand(MmcCommands.SendCID, false, false, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       0, 16, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SEND_CID took {0} ms.", duration);

            return sense;
        }

        public bool ReadOCR(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[4];
            bool sense = false;

            lastError = SendMmcCommand(MmcCommands.SendOpCond, false, true, MmcFlags.ResponseSPI_R3 | MmcFlags.Response_R3 | MmcFlags.CommandBCR,
                                       0, 4, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SEND_OP_COND took {0} ms.", duration);

            return sense;
        }

        public bool ReadExtendedCSD(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[512];
            bool sense = false;

            lastError = SendMmcCommand(MmcCommands.SendExtCSD, false, false, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       0, 512, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SEND_EXT_CSD took {0} ms.", duration);

            return sense;
        }

        public bool SetBlockLength(uint length, out uint[] response, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            bool sense = false;

            lastError = SendMmcCommand(MmcCommands.SetBlocklen, false, false, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandAC,
                                       length, 0, 0, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SET_BLOCKLEN took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer, out uint[] response, uint lba, uint blockSize, uint transferLength, bool byteAddressed, uint timeout, out double duration)
        {
            buffer = new byte[transferLength * blockSize];
            bool sense = false;
            uint address;
            if(byteAddressed)
                address = lba * blockSize;
            else
                address = lba;

            MmcCommands command;
            if(transferLength > 1)
                command = MmcCommands.ReadMultipleBlock;
            else
                command = MmcCommands.ReadSingleBlock;

            lastError = SendMmcCommand(command, false, false, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       address, blockSize, transferLength, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            if(transferLength > 1)
                DicConsole.DebugWriteLine("MMC Device", "READ_MULTIPLE_BLOCK took {0} ms.", duration);
            else
                DicConsole.DebugWriteLine("MMC Device", "READ_SINGLE_BLOCK took {0} ms.", duration);

            return sense;
        }

        public bool ReadStatus(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[4];
            bool sense = false;

            lastError = SendMmcCommand(MmcCommands.SendStatus, false, true, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       0, 4, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SEND_STATUS took {0} ms.", duration);

            return sense;
        }
    }
}
