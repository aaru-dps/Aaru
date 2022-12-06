// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Interop;
using Aaru.Decoders.ATA;
using Aaru.Devices.FreeBSD;
using Aaru.Devices.Windows;
using Microsoft.Win32.SafeHandles;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

// ReSharper disable UnusedMember.Global

namespace Aaru.Devices
{
    internal static class Command
    {
        /// <summary>Sends a SCSI command</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense">
        ///     <c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer" /> contains SCSI
        ///     sense
        /// </param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendScsiCommand(object fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer,
                                            uint timeout, ScsiDirection direction, out double duration, out bool sense)
        {
            PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendScsiCommand(ptId, fd, cdb, ref buffer, out senseBuffer, timeout, direction, out duration,
                                   out sense);
        }

        /// <summary>Sends a SCSI command</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="ptId">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense">
        ///     <c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer" /> contains SCSI
        ///     sense
        /// </param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendScsiCommand(PlatformID ptId, object fd, byte[] cdb, ref byte[] buffer,
                                            out byte[] senseBuffer, uint timeout, ScsiDirection direction,
                                            out double duration, out bool sense)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                {
                    ScsiIoctlDirection dir;

                    switch(direction)
                    {
                        case ScsiDirection.In:
                            dir = ScsiIoctlDirection.In;

                            break;
                        case ScsiDirection.Out:
                            dir = ScsiIoctlDirection.Out;

                            break;
                        default:
                            dir = ScsiIoctlDirection.Unspecified;

                            break;
                    }

                    return Windows.Command.SendScsiCommand((SafeFileHandle)fd, cdb, ref buffer, out senseBuffer,
                                                           timeout, dir, out duration, out sense);
                }
                case PlatformID.Linux:
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
                case PlatformID.FreeBSD:
                {
                    CcbFlags flags = 0;

                    switch(direction)
                    {
                        case ScsiDirection.In:
                            flags = CcbFlags.CamDirIn;

                            break;
                        case ScsiDirection.Out:
                            flags = CcbFlags.CamDirOut;

                            break;
                        case ScsiDirection.Bidirectional:
                            flags = CcbFlags.CamDirBoth;

                            break;
                        case ScsiDirection.None:
                            flags = CcbFlags.CamDirNone;

                            break;
                    }

                    return IntPtr.Size == 8
                               ? FreeBSD.Command.SendScsiCommand64((IntPtr)fd, cdb, ref buffer, out senseBuffer,
                                                                   timeout, flags, out duration, out sense)
                               : FreeBSD.Command.SendScsiCommand((IntPtr)fd, cdb, ref buffer, out senseBuffer, timeout,
                                                                 flags, out duration, out sense);
                }
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        /// <summary>Sends an ATA command in CHS format</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA returned non-OK status</param>
        /// <param name="registers">Registers to send to the device</param>
        /// <param name="errorRegisters">Registers returned by the device</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">What register contains the transfer length</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer length is in block, otherwise it is in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendAtaCommand(object fd, AtaRegistersChs registers,
                                           out AtaErrorRegistersChs errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptId, fd, registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                  timeout, transferBlocks, out duration, out sense);
        }

        /// <summary>Sends an ATA command in CHS format</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="ptId">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA returned non-OK status</param>
        /// <param name="registers">Registers to send to the device</param>
        /// <param name="errorRegisters">Registers returned by the device</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">What register contains the transfer length</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer length is in block, otherwise it is in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendAtaCommand(PlatformID ptId, object fd, AtaRegistersChs registers,
                                           out AtaErrorRegistersChs errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                {
                    if((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1 &&
                        (Environment.OSVersion.ServicePack == "Service Pack 1" ||
                         Environment.OSVersion.ServicePack == "")) ||
                       (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0))
                        throw new InvalidOperationException("Windows XP or earlier is not supported.");

                    // Windows NT 4 or earlier, requires special ATA pass thru SCSI. But Aaru cannot run there (or can it?)
                    if(Environment.OSVersion.Version.Major <= 4)
                        throw new InvalidOperationException("Windows NT 4.0 or earlier is not supported.");

                    return Windows.Command.SendAtaCommand((SafeFileHandle)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                case PlatformID.Linux:
                {
                    return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                                                        transferRegister, ref buffer, timeout, transferBlocks,
                                                        out duration, out sense);
                }
                case PlatformID.FreeBSD:
                {
                    return FreeBSD.Command.SendAtaCommand((IntPtr)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        /// <summary>Sends an ATA command in CHS format</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA returned non-OK status</param>
        /// <param name="registers">Registers to send to the device</param>
        /// <param name="errorRegisters">Registers returned by the device</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">What register contains the transfer length</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer length is in block, otherwise it is in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendAtaCommand(object fd, AtaRegistersLba28 registers,
                                           out AtaErrorRegistersLba28 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptId, fd, registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                  timeout, transferBlocks, out duration, out sense);
        }

        /// <summary>Sends an ATA command in 28-bit LBA format</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="ptId">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA returned non-OK status</param>
        /// <param name="registers">Registers to send to the device</param>
        /// <param name="errorRegisters">Registers returned by the device</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">What register contains the transfer length</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer length is in block, otherwise it is in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendAtaCommand(PlatformID ptId, object fd, AtaRegistersLba28 registers,
                                           out AtaErrorRegistersLba28 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                {
                    if((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1 &&
                        (Environment.OSVersion.ServicePack == "Service Pack 1" ||
                         Environment.OSVersion.ServicePack == "")) ||
                       (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0))
                        throw new InvalidOperationException("Windows XP or earlier is not supported.");

                    // Windows NT 4 or earlier, requires special ATA pass thru SCSI. But Aaru cannot run there (or can it?)
                    if(Environment.OSVersion.Version.Major <= 4)
                        throw new InvalidOperationException("Windows NT 4.0 or earlier is not supported.");

                    return Windows.Command.SendAtaCommand((SafeFileHandle)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                case PlatformID.Linux:
                {
                    return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                                                        transferRegister, ref buffer, timeout, transferBlocks,
                                                        out duration, out sense);
                }
                case PlatformID.FreeBSD:
                {
                    return FreeBSD.Command.SendAtaCommand((IntPtr)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                }
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        /// <summary>Sends an ATA command in 48-bit LBA format</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA returned non-OK status</param>
        /// <param name="registers">Registers to send to the device</param>
        /// <param name="errorRegisters">Registers returned by the device</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">What register contains the transfer length</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer length is in block, otherwise it is in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendAtaCommand(object fd, AtaRegistersLba48 registers,
                                           out AtaErrorRegistersLba48 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendAtaCommand(ptId, fd, registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                  timeout, transferBlocks, out duration, out sense);
        }

        /// <summary>Sends an ATA command in 48-bit format</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="ptId">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA returned non-OK status</param>
        /// <param name="registers">Registers to send to the device</param>
        /// <param name="errorRegisters">Registers returned by the device</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">What register contains the transfer length</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer length is in block, otherwise it is in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendAtaCommand(PlatformID ptId, object fd, AtaRegistersLba48 registers,
                                           out AtaErrorRegistersLba48 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                    // No check for Windows version. A 48-bit ATA disk simply does not work on earlier systems
                    return Windows.Command.SendAtaCommand((SafeFileHandle)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                case PlatformID.Linux:
                    return Linux.Command.SendAtaCommand((int)fd, registers, out errorRegisters, protocol,
                                                        transferRegister, ref buffer, timeout, transferBlocks,
                                                        out duration, out sense);
                case PlatformID.FreeBSD:
                    return FreeBSD.Command.SendAtaCommand((IntPtr)fd, registers, out errorRegisters, protocol,
                                                          ref buffer, timeout, out duration, out sense);
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        /// <summary>Sends a MMC/SD command</summary>
        /// <returns>The result of the command.</returns>
        /// <param name="fd">File handle</param>
        /// <param name="command">MMC/SD opcode</param>
        /// <param name="buffer">Buffer for MMC/SD command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if MMC/SD returned non-OK status</param>
        /// <param name="write"><c>True</c> if data is sent from host to card</param>
        /// <param name="isApplication"><c>True</c> if command should be preceded with CMD55</param>
        /// <param name="flags">Flags indicating kind and place of response</param>
        /// <param name="blocks">How many blocks to transfer</param>
        /// <param name="argument">Command argument</param>
        /// <param name="response">Response registers</param>
        /// <param name="blockSize">Size of block in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendMmcCommand(object fd, MmcCommands command, bool write, bool isApplication,
                                           MmcFlags flags, uint argument, uint blockSize, uint blocks,
                                           ref byte[] buffer, out uint[] response, out double duration, out bool sense,
                                           uint timeout = 0)
        {
            PlatformID ptId = DetectOS.GetRealPlatformID();

            return SendMmcCommand(ptId, (int)fd, command, write, isApplication, flags, argument, blockSize, blocks,
                                  ref buffer, out response, out duration, out sense, timeout);
        }

        /// <summary>Sends a MMC/SD command</summary>
        /// <returns>The result of the command.</returns>
        /// <param name="ptId">Platform ID for executing the command</param>
        /// <param name="fd">File handle</param>
        /// <param name="command">MMC/SD opcode</param>
        /// <param name="buffer">Buffer for MMC/SD command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if MMC/SD returned non-OK status</param>
        /// <param name="write"><c>True</c> if data is sent from host to card</param>
        /// <param name="isApplication"><c>True</c> if command should be preceded with CMD55</param>
        /// <param name="flags">Flags indicating kind and place of response</param>
        /// <param name="blocks">How many blocks to transfer</param>
        /// <param name="argument">Command argument</param>
        /// <param name="response">Response registers</param>
        /// <param name="blockSize">Size of block in bytes</param>
        /// <exception cref="InvalidOperationException">If the specified platform is not supported</exception>
        internal static int SendMmcCommand(PlatformID ptId, object fd, MmcCommands command, bool write,
                                           bool isApplication, MmcFlags flags, uint argument, uint blockSize,
                                           uint blocks, ref byte[] buffer, out uint[] response, out double duration,
                                           out bool sense, uint timeout = 0)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                    return Windows.Command.SendMmcCommand((SafeFileHandle)fd, command, write, isApplication, flags,
                                                          argument, blockSize, blocks, ref buffer, out response,
                                                          out duration, out sense, timeout);
                case PlatformID.Linux:
                    return Linux.Command.SendMmcCommand((int)fd, command, write, isApplication, flags, argument,
                                                        blockSize, blocks, ref buffer, out response, out duration,
                                                        out sense, timeout);
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        internal static int SendMultipleMmcCommands(PlatformID ptId, object fd, Device.MmcSingleCommand[] commands,
                                                    out double duration, out bool sense, uint timeout = 0)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                    return Windows.Command.SendMultipleMmcCommands((SafeFileHandle)fd, commands, out duration,
                                                                   out sense, timeout);
                case PlatformID.Linux:
                    return Linux.Command.SendMultipleMmcCommands((int)fd, commands, out duration, out sense, timeout);
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        internal static int ReOpen(PlatformID ptId, string devicePath, object fd, out object newFd)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT: return Windows.Command.ReOpen(devicePath, (SafeFileHandle)fd, out newFd);
                case PlatformID.Linux:   return Linux.Command.ReOpen(devicePath, (int)fd, out newFd);
                default:                 throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }

        internal static int BufferedOsRead(PlatformID ptId, object fd, out byte[] buffer, long offset, uint length,
                                           out double duration)
        {
            switch(ptId)
            {
                case PlatformID.Win32NT:
                    return Windows.Command.BufferedOsRead((SafeFileHandle)fd, out buffer, offset, length, out duration);
                case PlatformID.Linux:
                    return Linux.Command.BufferedOsRead((int)fd, out buffer, offset, length, out duration);
                default: throw new InvalidOperationException($"Platform {ptId} not yet supported.");
            }
        }
    }
}