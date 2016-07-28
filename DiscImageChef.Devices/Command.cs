// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     High level commands used to directly access devices.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Interop;
using Microsoft.Win32.SafeHandles;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices
{
    public static class Command
    {
        /// <summary>
        /// Sends a SCSI command
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        public static int SendScsiCommand(object fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ScsiDirection direction, out double duration, out bool sense)
        {
            Interop.PlatformID ptID = DetectOS.GetRealPlatformID();

            return SendScsiCommand(ptID, (SafeFileHandle)fd, cdb, ref buffer, out senseBuffer, timeout, direction, out duration, out sense);
        }

        /// <summary>
        /// Sends a SCSI command
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="ptID">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        public static int SendScsiCommand(Interop.PlatformID ptID, object fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ScsiDirection direction, out double duration, out bool sense)
        {
            switch(ptID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        Windows.ScsiIoctlDirection dir;

                        switch(direction)
                        {
                            case ScsiDirection.In:
                                dir = Windows.ScsiIoctlDirection.In;
                                break;
                            case ScsiDirection.Out:
                                dir = Windows.ScsiIoctlDirection.Out;
                                break;
                            default:
                                dir = Windows.ScsiIoctlDirection.Unspecified;
                                break;
                        }

                        return Windows.Command.SendScsiCommand((SafeFileHandle)fd, cdb, ref buffer, out senseBuffer, timeout, dir, out duration, out sense);
                    }
                case Interop.PlatformID.Linux:
                    {
                        Linux.ScsiIoctlDirection dir;

                        switch(direction)
                        {
                            case ScsiDirection.In:
                                dir = Linux.ScsiIoctlDirection.In;
                                break;
                            case ScsiDirection.Out:
                                dir = Linux.ScsiIoctlDirection.Out;
                                break;
                            case ScsiDirection.Bidirectional:
                                dir = Linux.ScsiIoctlDirection.Unspecified;
                                break;
                            case ScsiDirection.None:
                                dir = Linux.ScsiIoctlDirection.None;
                                break;
                            default:
                                dir = Linux.ScsiIoctlDirection.Unknown;
                                break;
                        }

                        return Linux.Command.SendScsiCommand((int)fd, cdb, ref buffer, out senseBuffer, timeout, dir, out duration, out sense);
                    }
                default:
                    throw new InvalidOperationException(String.Format("Platform {0} not yet supported.", ptID));
            }
        }

        public static int SendAtaCommand(object fd, AtaRegistersCHS registers,
            out AtaErrorRegistersCHS errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            Interop.PlatformID ptID = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptID, fd, registers, out errorRegisters, protocol,
                transferRegister, ref buffer, timeout, transferBlocks, out duration, out sense);
        }

        public static int SendAtaCommand(Interop.PlatformID ptID, object fd, AtaRegistersCHS registers,
            out AtaErrorRegistersCHS errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        throw new NotImplementedException();
                    }
                case Interop.PlatformID.Linux:
                    {
                        return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                            transferRegister, ref buffer, timeout, transferBlocks, out duration, out sense);
                    }
                default:
                    throw new InvalidOperationException(String.Format("Platform {0} not yet supported.", ptID));
            }
        }

        public static int SendAtaCommand(object fd, AtaRegistersLBA28 registers,
            out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            Interop.PlatformID ptID = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptID, fd, registers, out errorRegisters, protocol,
                transferRegister, ref buffer, timeout, transferBlocks, out duration, out sense);
        }

        public static int SendAtaCommand(Interop.PlatformID ptID, object fd, AtaRegistersLBA28 registers,
            out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        throw new NotImplementedException();
                    }
                case Interop.PlatformID.Linux:
                    {
                        return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                            transferRegister, ref buffer, timeout, transferBlocks, out duration, out sense);
                    }
                default:
                    throw new InvalidOperationException(String.Format("Platform {0} not yet supported.", ptID));
            }
        }

        public static int SendAtaCommand(object fd, AtaRegistersLBA48 registers,
            out AtaErrorRegistersLBA48 errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            Interop.PlatformID ptID = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptID, fd, registers, out errorRegisters, protocol,
                transferRegister, ref buffer, timeout, transferBlocks, out duration, out sense);
        }

        public static int SendAtaCommand(Interop.PlatformID ptID, object fd, AtaRegistersLBA48 registers,
            out AtaErrorRegistersLBA48 errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        throw new NotImplementedException();
                    }
                case Interop.PlatformID.Linux:
                    {
                        return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                            transferRegister, ref buffer, timeout, transferBlocks, out duration, out sense);
                    }
                default:
                    throw new InvalidOperationException(String.Format("Platform {0} not yet supported.", ptID));
            }
        }
    }
}

