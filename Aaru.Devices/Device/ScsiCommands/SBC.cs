// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SBC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI Block Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SCSI commands defined in SBC standards.
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

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Sends the SBC READ (6) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    public bool Read6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint timeout,
                      out double duration) =>
        Read6(out buffer, out senseBuffer, lba, blockSize, 1, timeout, out duration);

    /// <summary>Sends the SBC READ (6) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    public bool Read6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, byte transferLength,
                      uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[6];

        cdb[0] = (byte)ScsiCommands.Read6;
        cdb[1] = (byte)((lba & 0x1F0000) >> 16);
        cdb[2] = (byte)((lba & 0xFF00)   >> 8);
        cdb[3] = (byte)(lba & 0xFF);
        cdb[4] = transferLength;

        buffer = transferLength == 0 ? new byte[256 * blockSize] : new byte[transferLength * blockSize];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SBC READ (10) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="rdprotect">Instructs the drive how to check for protection information on the medium.</param>
    /// <param name="dpo">
    ///     If set to <c>true</c> requested blocks shall be assigned the lowest retention priority on cache
    ///     fetch/retain.
    /// </param>
    /// <param name="fua">If set to <c>true</c> requested blocks MUST bu read from medium and not the cache.</param>
    /// <param name="fuaNv">
    ///     If set to <c>true</c> requested blocks will be returned from non-volatile cache. If they're not
    ///     present they shall be stored there.
    /// </param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="groupNumber">Group number where attributes associated with this command should be collected.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="relAddr">If set to <c>true</c> address is relative to current position.</param>
    public bool Read10(out byte[] buffer, out byte[] senseBuffer, byte rdprotect, bool dpo, bool fua, bool fuaNv,
                       bool relAddr, uint lba, uint blockSize, byte groupNumber, ushort transferLength, uint timeout,
                       out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];

        cdb[0] = (byte)ScsiCommands.Read10;
        cdb[1] = (byte)((rdprotect & 0x07) << 5);

        if(dpo)
            cdb[1] += 0x10;

        if(fua)
            cdb[1] += 0x08;

        if(fuaNv)
            cdb[1] += 0x02;

        if(relAddr)
            cdb[1] += 0x01;

        cdb[2] = (byte)((lba & 0xFF000000) >> 24);
        cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] = (byte)((lba & 0xFF00)     >> 8);
        cdb[5] = (byte)(lba         & 0xFF);
        cdb[6] = (byte)(groupNumber & 0x1F);
        cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
        cdb[8] = (byte)(transferLength & 0xFF);

        buffer = new byte[transferLength * blockSize];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_10_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SBC READ (12) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="rdprotect">Instructs the drive how to check for protection information on the medium.</param>
    /// <param name="dpo">
    ///     If set to <c>true</c> requested blocks shall be assigned the lowest retention priority on cache
    ///     fetch/retain.
    /// </param>
    /// <param name="fua">If set to <c>true</c> requested blocks MUST bu read from medium and not the cache.</param>
    /// <param name="fuaNv">
    ///     If set to <c>true</c> requested blocks will be returned from non-volatile cache. If they're not
    ///     present they shall be stored there.
    /// </param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="groupNumber">Group number where attributes associated with this command should be collected.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="streaming">If set to <c>true</c> the stream playback operation should be used (MMC only).</param>
    /// <param name="relAddr">If set to <c>true</c> address is relative to current position.</param>
    public bool Read12(out byte[] buffer, out byte[] senseBuffer, byte rdprotect, bool dpo, bool fua, bool fuaNv,
                       bool relAddr, uint lba, uint blockSize, byte groupNumber, uint transferLength, bool streaming,
                       uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];

        cdb[0] = (byte)ScsiCommands.Read12;
        cdb[1] = (byte)((rdprotect & 0x07) << 5);

        if(dpo)
            cdb[1] += 0x10;

        if(fua)
            cdb[1] += 0x08;

        if(fuaNv)
            cdb[1] += 0x02;

        if(relAddr)
            cdb[1] += 0x01;

        cdb[2]  = (byte)((lba & 0xFF000000) >> 24);
        cdb[3]  = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4]  = (byte)((lba & 0xFF00)     >> 8);
        cdb[5]  = (byte)(lba & 0xFF);
        cdb[6]  = (byte)((transferLength & 0xFF000000) >> 24);
        cdb[7]  = (byte)((transferLength & 0xFF0000)   >> 16);
        cdb[8]  = (byte)((transferLength & 0xFF00)     >> 8);
        cdb[9]  = (byte)(transferLength & 0xFF);
        cdb[10] = (byte)(groupNumber    & 0x1F);

        if(streaming)
            cdb[10] += 0x80;

        buffer = new byte[transferLength * blockSize];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_12_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SBC READ (16) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="rdprotect">Instructs the drive how to check for protection information on the medium.</param>
    /// <param name="dpo">
    ///     If set to <c>true</c> requested blocks shall be assigned the lowest retention priority on cache
    ///     fetch/retain.
    /// </param>
    /// <param name="fua">If set to <c>true</c> requested blocks MUST bu read from medium and not the cache.</param>
    /// <param name="fuaNv">
    ///     If set to <c>true</c> requested blocks will be returned from non-volatile cache. If they're not
    ///     present they shall be stored there.
    /// </param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="groupNumber">Group number where attributes associated with this command should be collected.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="streaming">If set to <c>true</c> the stream playback operation should be used (MMC only).</param>
    public bool Read16(out byte[] buffer, out byte[] senseBuffer, byte rdprotect, bool dpo, bool fua, bool fuaNv,
                       ulong lba, uint blockSize, byte groupNumber, uint transferLength, bool streaming, uint timeout,
                       out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb      = new byte[16];
        byte[] lbaBytes = BitConverter.GetBytes(lba);

        cdb[0] = (byte)ScsiCommands.Read16;
        cdb[1] = (byte)((rdprotect & 0x07) << 5);

        if(dpo)
            cdb[1] += 0x10;

        if(fua)
            cdb[1] += 0x08;

        if(fuaNv)
            cdb[1] += 0x02;

        cdb[2]  = lbaBytes[7];
        cdb[3]  = lbaBytes[6];
        cdb[4]  = lbaBytes[5];
        cdb[5]  = lbaBytes[4];
        cdb[6]  = lbaBytes[3];
        cdb[7]  = lbaBytes[2];
        cdb[8]  = lbaBytes[1];
        cdb[9]  = lbaBytes[0];
        cdb[10] = (byte)((transferLength & 0xFF000000) >> 24);
        cdb[11] = (byte)((transferLength & 0xFF0000)   >> 16);
        cdb[12] = (byte)((transferLength & 0xFF00)     >> 8);
        cdb[13] = (byte)(transferLength & 0xFF);
        cdb[14] = (byte)(groupNumber    & 0x1F);

        if(streaming)
            cdb[14] += 0x80;

        buffer = new byte[transferLength * blockSize];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_16_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SBC READ LONG (10) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ LONG response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="relAddr"></param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="correct">If set to <c>true</c> ask the drive to try to correct errors in the sector.</param>
    /// <param name="lba">LBA to read.</param>
    /// <param name="transferBytes">
    ///     How many bytes to read. If the number is not exactly the drive's size, the command will
    ///     fail and incidate a delta of the size in SENSE.
    /// </param>
    public bool ReadLong10(out byte[] buffer, out byte[] senseBuffer, bool correct, bool relAddr, uint lba,
                           ushort transferBytes, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];

        cdb[0] = (byte)ScsiCommands.ReadLong;

        if(correct)
            cdb[1] += 0x02;

        if(relAddr)
            cdb[1] += 0x01;

        cdb[2] = (byte)((lba & 0xFF000000) >> 24);
        cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] = (byte)((lba & 0xFF00)     >> 8);
        cdb[5] = (byte)(lba & 0xFF);
        cdb[7] = (byte)((transferBytes & 0xFF00) >> 8);
        cdb[8] = (byte)(transferBytes & 0xFF);

        buffer = new byte[transferBytes];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_LONG_10_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SBC READ LONG (16) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ LONG response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="correct">If set to <c>true</c> ask the drive to try to correct errors in the sector.</param>
    /// <param name="lba">LBA to read.</param>
    /// <param name="transferBytes">
    ///     How many bytes to read. If the number is not exactly the drive's size, the command will
    ///     fail and incidate a delta of the size in SENSE.
    /// </param>
    public bool ReadLong16(out byte[] buffer, out byte[] senseBuffer, bool correct, ulong lba, uint transferBytes,
                           uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb      = new byte[16];
        byte[] lbaBytes = BitConverter.GetBytes(lba);

        cdb[0]  = (byte)ScsiCommands.ServiceActionIn;
        cdb[1]  = (byte)ScsiServiceActions.ReadLong16;
        cdb[2]  = lbaBytes[7];
        cdb[3]  = lbaBytes[6];
        cdb[4]  = lbaBytes[5];
        cdb[5]  = lbaBytes[4];
        cdb[6]  = lbaBytes[3];
        cdb[7]  = lbaBytes[2];
        cdb[8]  = lbaBytes[1];
        cdb[9]  = lbaBytes[0];
        cdb[12] = (byte)((transferBytes & 0xFF00) >> 8);
        cdb[13] = (byte)(transferBytes & 0xFF);

        if(correct)
            cdb[14] += 0x01;

        buffer = new byte[transferBytes];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_LONG_16_took_0_ms, duration);

        return sense;
    }

    /// <summary>Moves the device reading element to the specified block address</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">LBA.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Seek6(out byte[] senseBuffer, uint lba, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.Seek6;
        cdb[1] = (byte)((lba & 0x1F0000) >> 16);
        cdb[2] = (byte)((lba & 0xFF00)   >> 8);
        cdb[3] = (byte)(lba & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.SEEK_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Moves the device reading element to the specified block address</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">LBA.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Seek10(out byte[] senseBuffer, uint lba, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[10];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.Seek10;
        cdb[2] = (byte)((lba & 0xFF000000) >> 24);
        cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] = (byte)((lba & 0xFF00)     >> 8);
        cdb[5] = (byte)(lba & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.SEEK_10_took_0_ms, duration);

        return sense;
    }
}