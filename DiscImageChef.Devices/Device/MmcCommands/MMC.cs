// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SecureDigital and MultiMediaCard commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains MultiMediaCard commands.
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

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool ReadCsd(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[16];

            LastError = SendMmcCommand(MmcCommands.SendCsd, false,
                                       false,
                                       MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 | MmcFlags.CommandAc, 0,
                                       16,                                                                1,
                                       ref buffer,                                                        out response,
                                       out duration,                                                      out bool sense, timeout);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SEND_CSD took {0} ms.", duration);

            return sense;
        }

        public bool ReadCid(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[16];

            LastError = SendMmcCommand(MmcCommands.SendCid, false,
                                       false,
                                       MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 | MmcFlags.CommandAc, 0,
                                       16,                                                                1,
                                       ref buffer,                                                        out response,
                                       out duration,                                                      out bool sense, timeout);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SEND_CID took {0} ms.", duration);

            return sense;
        }

        public bool ReadOcr(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[4];

            LastError = SendMmcCommand(MmcCommands.SendOpCond, false,
                                       true,
                                       MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 | MmcFlags.CommandBcr, 0,
                                       4,                                                                  1,
                                       ref buffer,                                                         out response,
                                       out duration,                                                       out bool sense, timeout);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SEND_OP_COND took {0} ms.", duration);

            return sense;
        }

        public bool ReadExtendedCsd(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[512];

            LastError = SendMmcCommand(MmcCommands.SendExtCsd, false,
                                       false,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc, 0,
                                       512,                                                                 1,
                                       ref buffer,
                                       out response, out duration, out bool sense, timeout);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SEND_EXT_CSD took {0} ms.", duration);

            return sense;
        }

        public bool SetBlockLength(uint length, out uint[] response, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            LastError = SendMmcCommand(MmcCommands.SetBlocklen, false,
                                       false,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAc, length,
                                       0,                                                                 0,
                                       ref buffer,                                                        out response,
                                       out duration,                                                      out bool sense, timeout);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("MMC Device", "SET_BLOCKLEN took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer, out uint[] response, uint lba, uint blockSize,
                         uint       transferLength,
                         bool       byteAddressed, uint timeout, out double duration)
        {
            buffer = new byte[transferLength * blockSize];
            uint address;
            if(byteAddressed) address = lba * blockSize;
            else address              = lba;

            MmcCommands command = transferLength > 1 ? MmcCommands.ReadMultipleBlock : MmcCommands.ReadSingleBlock;

            LastError = SendMmcCommand(command, false,
                                       false,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc, address,
                                       blockSize,
                                       transferLength, ref buffer, out response, out duration,
                                       out bool sense, timeout);
            Error = LastError != 0;

            if(transferLength > 1)
            {
                byte[] foo = new byte[0];
                SendMmcCommand(MmcCommands.StopTransmission, false,
                               false,
                               MmcFlags.ResponseR1B | MmcFlags.ResponseSpiR1B | MmcFlags.CommandAc, 0,
                               0,                                                                   0, ref foo,
                               out _,
                               out double stopDuration, out bool _, timeout);
                duration += stopDuration;
                DicConsole.DebugWriteLine("MMC Device", "READ_MULTIPLE_BLOCK took {0} ms.", duration);
            }
            else DicConsole.DebugWriteLine("MMC Device", "READ_SINGLE_BLOCK took {0} ms.", duration);

            return sense;
        }

        public bool ReadStatus(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[4];

            LastError = SendMmcCommand(MmcCommands.SendStatus, false,
                                       true,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAc, 0,
                                       4,                                                                 1,
                                       ref buffer,                                                        out response,
                                       out duration,                                                      out bool sense, timeout);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SEND_STATUS took {0} ms.", duration);

            return sense;
        }
    }
}