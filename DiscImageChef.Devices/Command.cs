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
//     it under the terms of the GNU Lesser General internal License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General internal License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Interop;
using Microsoft.Win32.SafeHandles;

namespace DiscImageChef.Devices
{
    static class Command
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
        internal static int SendScsiCommand(object fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer,
                                          uint timeout, ScsiDirection direction, out double duration, out bool sense)
        {
            Interop.PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendScsiCommand(ptId, fd, cdb, ref buffer, out senseBuffer, timeout, direction, out duration,
                                   out sense);
        }

        /// <summary>
        /// Sends a SCSI command
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="ptId">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        internal static int SendScsiCommand(Interop.PlatformID ptId, object fd, byte[] cdb, ref byte[] buffer,
                                          out byte[] senseBuffer, uint timeout, ScsiDirection direction,
                                          out double duration, out bool sense)
        {
            switch(ptId)
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

                    return Windows.Command.SendScsiCommand((SafeFileHandle)fd, cdb, ref buffer, out senseBuffer,
                                                           timeout, dir, out duration, out sense);
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

                    return Linux.Command.SendScsiCommand((int)fd, cdb, ref buffer, out senseBuffer, timeout, dir,
                                                         out duration, out sense);
                }
                case Interop.PlatformID.FreeBSD:
                {
                    FreeBSD.CcbFlags flags = 0;

                    switch(direction)
                    {
                        case ScsiDirection.In:
                            flags = FreeBSD.CcbFlags.CamDirIn;
                            break;
                        case ScsiDirection.Out:
                            flags = FreeBSD.CcbFlags.CamDirOut;
                            break;
                        case ScsiDirection.Bidirectional:
                            flags = FreeBSD.CcbFlags.CamDirBoth;
                            break;
                        case ScsiDirection.None:
                            flags = FreeBSD.CcbFlags.CamDirNone;
                            break;
                    }

                    return IntPtr.Size == 8
                               ? FreeBSD.Command.SendScsiCommand64((IntPtr)fd, cdb, ref buffer, out senseBuffer,
                                                                   timeout, flags, out duration, out sense)
                               : FreeBSD.Command.SendScsiCommand((IntPtr)fd, cdb, ref buffer, out senseBuffer, timeout,
                                                                 flags, out duration, out sense);
                }
                default: throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", ptId));
            }
        }

        internal static int SendAtaCommand(object fd, AtaRegistersCHS registers, out AtaErrorRegistersCHS errorRegisters,
                                         AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                         uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            Interop.PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptId, fd, registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                  timeout, transferBlocks, out duration, out sense);
        }

        internal static int SendAtaCommand(Interop.PlatformID ptId, object fd, AtaRegistersCHS registers,
                                         out AtaErrorRegistersCHS errorRegisters, AtaProtocol protocol,
                                         AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                         bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptId)
            {
                case Interop.PlatformID.Win32NT:
                {
                    if(Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1 &&
                       (Environment.OSVersion.ServicePack == "Service Pack 1" ||
                        Environment.OSVersion.ServicePack == "") || Environment.OSVersion.Version.Major == 5 &&
                       Environment.OSVersion.Version.Minor == 0)
                        return Windows.Command.SendIdeCommand((SafeFileHandle)fd, registers, out errorRegisters,
                                                              protocol, ref buffer, timeout, out duration, out sense);
                    // Windows NT 4 or earlier, requires special ATA pass thru SCSI. But DiscImageChef cannot run there (or can it?)
                    if(Environment.OSVersion.Version.Major <= 4)
                        throw new InvalidOperationException("Windows NT 4.0 or earlier is not supported.");

                    return Windows.Command.SendAtaCommand((SafeFileHandle)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                case Interop.PlatformID.Linux:
                {
                    return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                                                        transferRegister, ref buffer, timeout, transferBlocks,
                                                        out duration, out sense);
                }
                case Interop.PlatformID.FreeBSD:
                {
                    return FreeBSD.Command.SendAtaCommand((IntPtr)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                default: throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", ptId));
            }
        }

        internal static int SendAtaCommand(object fd, AtaRegistersLBA28 registers,
                                         out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
                                         AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                         bool transferBlocks, out double duration, out bool sense)
        {
            Interop.PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptId, fd, registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                  timeout, transferBlocks, out duration, out sense);
        }

        internal static int SendAtaCommand(Interop.PlatformID ptId, object fd, AtaRegistersLBA28 registers,
                                         out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
                                         AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                         bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptId)
            {
                case Interop.PlatformID.Win32NT:
                {
                    if(Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1 &&
                       (Environment.OSVersion.ServicePack == "Service Pack 1" ||
                        Environment.OSVersion.ServicePack == "") || Environment.OSVersion.Version.Major == 5 &&
                       Environment.OSVersion.Version.Minor == 0)
                        return Windows.Command.SendIdeCommand((SafeFileHandle)fd, registers, out errorRegisters,
                                                              protocol, ref buffer, timeout, out duration, out sense);
                    // Windows NT 4 or earlier, requires special ATA pass thru SCSI. But DiscImageChef cannot run there (or can it?)
                    if(Environment.OSVersion.Version.Major <= 4)
                        throw new InvalidOperationException("Windows NT 4.0 or earlier is not supported.");

                    return Windows.Command.SendAtaCommand((SafeFileHandle)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                case Interop.PlatformID.Linux:
                {
                    return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                                                        transferRegister, ref buffer, timeout, transferBlocks,
                                                        out duration, out sense);
                }
                case Interop.PlatformID.FreeBSD:
                {
                    return FreeBSD.Command.SendAtaCommand((IntPtr)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                default: throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", ptId));
            }
        }

        internal static int SendAtaCommand(object fd, AtaRegistersLBA48 registers,
                                         out AtaErrorRegistersLBA48 errorRegisters, AtaProtocol protocol,
                                         AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                         bool transferBlocks, out double duration, out bool sense)
        {
            Interop.PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptId, fd, registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                  timeout, transferBlocks, out duration, out sense);
        }

        internal static int SendAtaCommand(Interop.PlatformID ptId, object fd, AtaRegistersLBA48 registers,
                                         out AtaErrorRegistersLBA48 errorRegisters, AtaProtocol protocol,
                                         AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                         bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptId)
            {
                case Interop.PlatformID.Win32NT:
                {
                    // No check for Windows version. A 48-bit ATA disk simply does not work on earlier systems
                    return Windows.Command.SendAtaCommand((SafeFileHandle)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                case Interop.PlatformID.Linux:
                {
                    return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                                                        transferRegister, ref buffer, timeout, transferBlocks,
                                                        out duration, out sense);
                }
                case Interop.PlatformID.FreeBSD:
                {
                    return FreeBSD.Command.SendAtaCommand((IntPtr)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                default: throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", ptId));
            }
        }

        internal static int SendMmcCommand(object fd, MmcCommands command, bool write, bool isApplication, MmcFlags flags,
                                         uint argument, uint blockSize, uint blocks, ref byte[] buffer,
                                         out uint[] response, out double duration, out bool sense, uint timeout = 0)
        {
            Interop.PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendMmcCommand(ptId, (int)fd, command, write, isApplication, flags, argument, blockSize, blocks,
                                  ref buffer, out response, out duration, out sense, timeout);
        }

        internal static int SendMmcCommand(Interop.PlatformID ptId, object fd, MmcCommands command, bool write,
                                         bool isApplication, MmcFlags flags, uint argument, uint blockSize, uint blocks,
                                         ref byte[] buffer, out uint[] response, out double duration, out bool sense,
                                         uint timeout = 0)
        {
            switch(ptId)
            {
                case Interop.PlatformID.Win32NT:
                {
                    return Windows.Command.SendMmcCommand((SafeFileHandle)fd, command, write, isApplication, flags,
                                                          argument, blockSize, blocks, ref buffer, out response,
                                                          out duration, out sense, timeout);
                }
                case Interop.PlatformID.Linux:
                {
                    return Linux.Command.SendMmcCommand((int)fd, command, write, isApplication, flags, argument,
                                                        blockSize, blocks, ref buffer, out response, out duration,
                                                        out sense, timeout);
                }
                default: throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", ptId));
            }
        }
    }
}