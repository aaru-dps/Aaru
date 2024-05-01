// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI Stream Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SCSI commands defined in SSC standards.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.Console;

// ReSharper disable UnusedMember.Global

namespace Aaru.Devices;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public partial class Device
{
    /// <summary>Prepares the medium for reading</summary>
    /// <returns><c>true</c>, if load was successful, <c>false</c> otherwise.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Load(out byte[] senseBuffer, uint timeout, out double duration) =>
        LoadUnload(out senseBuffer, false, true, false, false, false, timeout, out duration);

    /// <summary>Prepares the medium for ejection</summary>
    /// <returns><c>true</c>, if unload was successful, <c>false</c> otherwise.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Unload(out byte[] senseBuffer, uint timeout, out double duration) =>
        LoadUnload(out senseBuffer, false, false, false, false, false, timeout, out duration);

    /// <summary>Prepares the medium for reading or ejection</summary>
    /// <returns><c>true</c>, if load/unload was successful, <c>false</c> otherwise.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="load">If set to <c>true</c> load the medium for reading.</param>
    /// <param name="retense">If set to <c>true</c> retense the tape.</param>
    /// <param name="endOfTape">If set to <c>true</c> move the medium to the EOT mark.</param>
    /// <param name="hold">
    ///     If set to <c>true</c> and <paramref name="load" /> is also set to <c>true</c>, moves the medium to
    ///     the drive but does not prepare it for reading.
    /// </param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool LoadUnload(out byte[] senseBuffer, bool immediate, bool load, bool retense, bool endOfTape, bool hold,
                           uint       timeout,     out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.LoadUnload;

        if(immediate) cdb[1] = 0x01;

        if(load) cdb[4] += 0x01;

        if(retense) cdb[4] += 0x02;

        if(endOfTape) cdb[4] += 0x04;

        if(hold) cdb[4] += 0x08;

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.LOAD_UNLOAD_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Positions the medium to the specified block in the current partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate(out byte[] senseBuffer, uint lba, uint timeout, out double duration) =>
        Locate(out senseBuffer, false, false, false, 0, lba, timeout, out duration);

    /// <summary>Positions the medium to the specified block in the specified partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="partition">Partition to position to.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate(out byte[] senseBuffer, byte partition, uint lba, uint timeout, out double duration) =>
        Locate(out senseBuffer, false, false, false, partition, lba, timeout, out duration);

    /// <summary>Positions the medium to the specified block in the current partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate(out byte[] senseBuffer, bool immediate, uint lba, uint timeout, out double duration) =>
        Locate(out senseBuffer, immediate, false, false, 0, lba, timeout, out duration);

    /// <summary>Positions the medium to the specified block in the specified partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="partition">Partition to position to.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate(out byte[] senseBuffer, bool immediate, byte partition, uint lba, uint timeout,
                       out double duration) => Locate(out senseBuffer,
                                                      immediate,
                                                      false,
                                                      false,
                                                      partition,
                                                      lba,
                                                      timeout,
                                                      out duration);

    /// <summary>Positions the medium to the specified object identifier</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="blockType">If set to <c>true</c> object identifier is vendor specified.</param>
    /// <param name="changePartition">If set to <c>true</c> change partition.</param>
    /// <param name="partition">Partition to position to.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate(out byte[] senseBuffer, bool immediate, bool blockType, bool changePartition, byte partition,
                       uint       objectId,    uint timeout,   out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[10];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.Locate;

        if(immediate) cdb[1] += 0x01;

        if(changePartition) cdb[1] += 0x02;

        if(blockType) cdb[1] += 0x04;

        cdb[3] = (byte)((objectId & 0xFF000000) >> 24);
        cdb[4] = (byte)((objectId & 0xFF0000)   >> 16);
        cdb[5] = (byte)((objectId & 0xFF00)     >> 8);
        cdb[6] = (byte)(objectId & 0xFF);
        cdb[8] = partition;

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.LOCATE_10_took_0_ms, duration);

        return sense;
    }

    /// <summary>Positions the medium to the specified block in the current partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate16(out byte[] senseBuffer, ulong lba, uint timeout, out double duration) =>
        Locate16(out senseBuffer, false, false, SscLogicalIdTypes.ObjectId, false, 0, lba, timeout, out duration);

    /// <summary>Positions the medium to the specified block in the specified partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="partition">Partition to position to.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate16(out byte[] senseBuffer, byte partition, ulong lba, uint timeout, out double duration) =>
        Locate16(out senseBuffer,
                 false,
                 false,
                 SscLogicalIdTypes.ObjectId,
                 false,
                 partition,
                 lba,
                 timeout,
                 out duration);

    /// <summary>Positions the medium to the specified block in the current partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate16(out byte[] senseBuffer, bool immediate, ulong lba, uint timeout, out double duration) =>
        Locate16(out senseBuffer, immediate, false, SscLogicalIdTypes.ObjectId, false, 0, lba, timeout, out duration);

    /// <summary>Positions the medium to the specified block in the specified partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="partition">Partition to position to.</param>
    /// <param name="lba">Logical block address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate16(out byte[] senseBuffer, bool immediate, byte partition, ulong lba, uint timeout,
                         out double duration) => Locate16(out senseBuffer,
                                                          immediate,
                                                          false,
                                                          SscLogicalIdTypes.ObjectId,
                                                          false,
                                                          partition,
                                                          lba,
                                                          timeout,
                                                          out duration);

    /// <summary>Positions the medium to the specified object identifier</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="changePartition">If set to <c>true</c> change partition.</param>
    /// <param name="destType">Destination type.</param>
    /// <param name="bam">If set to <c>true</c> objectId is explicit.</param>
    /// <param name="partition">Partition to position to.</param>
    /// <param name="identifier">Destination identifier.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Locate16(out byte[] senseBuffer, bool immediate, bool  changePartition, SscLogicalIdTypes destType,
                         bool       bam,         byte partition, ulong identifier, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb     = new byte[16];
        byte[] buffer  = Array.Empty<byte>();
        byte[] idBytes = BitConverter.GetBytes(identifier);

        cdb[0] = (byte)ScsiCommands.Locate16;
        cdb[1] = (byte)((byte)destType << 3);

        if(immediate) cdb[1] += 0x01;

        if(changePartition) cdb[1] += 0x02;

        if(bam) cdb[2] = 0x01;

        cdb[3] = partition;

        cdb[4]  = idBytes[7];
        cdb[5]  = idBytes[6];
        cdb[6]  = idBytes[5];
        cdb[7]  = idBytes[4];
        cdb[8]  = idBytes[3];
        cdb[9]  = idBytes[2];
        cdb[10] = idBytes[1];
        cdb[11] = idBytes[0];

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.LOCATE_16_took_0_ms, duration);

        return sense;
    }

    /*/// <summary>
    /// Reads the specified number of blocks from the medium
    /// </summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read6(out byte[] buffer, out byte[] senseBuffer, uint blocks, uint blockSize, uint timeout, out double duration)
    {
        return Read6(out buffer, out senseBuffer, false, true, blocks, blockSize, timeout, out duration);
    }*/

    /// <summary>Reads the specified number of bytes or of blocks from the medium</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="transferLen">How many bytes to read.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read6(out byte[] buffer,  out byte[] senseBuffer, bool sili, uint transferLen, uint blockSize,
                      uint       timeout, out double duration) => Read6(out buffer,
                                                                        out senseBuffer,
                                                                        sili,
                                                                        false,
                                                                        transferLen,
                                                                        blockSize,
                                                                        timeout,
                                                                        out duration);

    /// <summary>Reads the specified number of bytes or of blocks from the medium</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">
    ///     If set to <c>true</c> suppress the incorrect-length indication. Cannot be set while
    ///     <paramref name="fixedLen" /> is set also.
    /// </param>
    /// <param name="fixedLen">
    ///     If set to <c>true</c> <paramref name="transferLen" /> indicates how many blocks to read of a
    ///     fixed size.
    /// </param>
    /// <param name="transferLen">Transfer length in blocks or bytes depending of <paramref name="fixedLen" /> status.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read6(out byte[] buffer,    out byte[] senseBuffer, bool       sili, bool fixedLen, uint transferLen,
                      uint       blockSize, uint       timeout,     out double duration)
    {
        buffer = fixedLen ? new byte[blockSize * transferLen] : new byte[transferLen];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.Read6;

        if(fixedLen) cdb[1] += 0x01;

        if(sili) cdb[1] += 0x02;

        cdb[2] = (byte)((transferLen & 0xFF0000) >> 16);
        cdb[3] = (byte)((transferLen & 0xFF00)   >> 8);
        cdb[4] = (byte)(transferLen & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.READ_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads a number of fixed-length blocks starting at specified object</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read16(out byte[] buffer,    out byte[] senseBuffer, bool       sili, ulong objectId, uint blocks,
                       uint       blockSize, uint       timeout,     out double duration) =>
        Read16(out buffer, out senseBuffer, sili, false, 0, objectId, blocks, blockSize, timeout, out duration);

    /// <summary>Reads a number of fixed-length blocks starting at specified block from the specified partition</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="partition">Partition to read object from.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read16(out byte[] buffer, out byte[] senseBuffer, bool sili,    byte       partition, ulong objectId,
                       uint       blocks, uint       blockSize,   uint timeout, out double duration) =>
        Read16(out buffer, out senseBuffer, sili, false, partition, objectId, blocks, blockSize, timeout, out duration);

    /// <summary>Reads a number of fixed-length blocks starting at specified object</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read16(out byte[] buffer,  out byte[] senseBuffer, ulong objectId, uint blocks, uint blockSize,
                       uint       timeout, out double duration) => Read16(out buffer,
                                                                          out senseBuffer,
                                                                          false,
                                                                          true,
                                                                          0,
                                                                          objectId,
                                                                          blocks,
                                                                          blockSize,
                                                                          timeout,
                                                                          out duration);

    /// <summary>Reads a number of fixed-length blocks starting at specified block from the specified partition</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="partition">Partition to read object from.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read16(out byte[] buffer,    out byte[] senseBuffer, byte       partition, ulong objectId, uint blocks,
                       uint       blockSize, uint       timeout,     out double duration) =>
        Read16(out buffer, out senseBuffer, false, true, partition, objectId, blocks, blockSize, timeout, out duration);

    /// <summary>Reads a number of bytes or objects starting at specified object from the specified partition</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">
    ///     If set to <c>true</c> suppress the incorrect-length indication. Cannot be set while
    ///     <paramref name="fixedLen" /> is set also.
    /// </param>
    /// <param name="fixedLen">
    ///     If set to <c>true</c> <paramref name="transferLen" /> indicates how many blocks to read of a
    ///     fixed size.
    /// </param>
    /// <param name="partition">Partition to read object from.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="transferLen">Transfer length in blocks or bytes depending of <paramref name="fixedLen" /> status.</param>
    /// <param name="objectSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Read16(out byte[] buffer,   out byte[] senseBuffer, bool sili,       bool fixedLen, byte partition,
                       ulong      objectId, uint       transferLen, uint objectSize, uint timeout,  out double duration)
    {
        buffer = fixedLen ? new byte[objectSize * transferLen] : new byte[transferLen];
        var cdb = new byte[6];
        senseBuffer = new byte[64];
        byte[] idBytes = BitConverter.GetBytes(objectId);

        cdb[0] = (byte)ScsiCommands.Read16;

        if(fixedLen) cdb[1] += 0x01;

        if(sili) cdb[1] += 0x02;

        cdb[3]  = partition;
        cdb[4]  = idBytes[7];
        cdb[5]  = idBytes[6];
        cdb[6]  = idBytes[5];
        cdb[7]  = idBytes[4];
        cdb[8]  = idBytes[3];
        cdb[9]  = idBytes[2];
        cdb[10] = idBytes[1];
        cdb[11] = idBytes[0];
        cdb[12] = (byte)((transferLen & 0xFF0000) >> 16);
        cdb[13] = (byte)((transferLen & 0xFF00)   >> 8);
        cdb[14] = (byte)(transferLen & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.READ_16_took_0_ms, duration);

        return sense;
    }

    /// <summary>Requests the drive the maximum and minimum block size</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadBlockLimits(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        buffer = new byte[6];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.ReadBlockLimits;

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.READ_BLOCK_LIMITS_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reports current reading/writing elements position on the medium</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadPosition(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadPosition(out buffer, out senseBuffer, SscPositionForms.Short, timeout, out duration);

    /// <summary>Reports current reading/writing elements position on the medium using 32 bytes response</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadPositionLong(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadPosition(out buffer, out senseBuffer, SscPositionForms.Long, timeout, out duration);

    /// <summary>Reports current reading/writing elements position on the medium</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="vendorType">Requests the position to be given in vendor-specified meaning.</param>
    /// <param name="longForm">Requests the response to be 32 bytes format.</param>
    /// <param name="totalPosition">Requests current logical position.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadPosition(out byte[] buffer,        out byte[] senseBuffer, bool       vendorType, bool longForm,
                             bool       totalPosition, uint       timeout,     out double duration)
    {
        byte responseForm = 0;

        if(vendorType) responseForm += 0x01;

        if(longForm) responseForm += 0x02;

        if(totalPosition) responseForm += 0x04;

        return ReadPosition(out buffer, out senseBuffer, (SscPositionForms)responseForm, timeout, out duration);
    }

    /// <summary>Reports current reading/writing elements position on the medium</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="responseForm">Response form.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadPosition(out byte[] buffer, out byte[] senseBuffer, SscPositionForms responseForm, uint timeout,
                             out double duration)
    {
        switch(responseForm)
        {
            case SscPositionForms.Long:
            case SscPositionForms.OldLong:
            case SscPositionForms.OldLongTclpVendor:
            case SscPositionForms.OldLongVendor:
            case SscPositionForms.Extended:
                buffer = new byte[32];

                break;
            case SscPositionForms.OldTclp:
            case SscPositionForms.OldTclpVendor:
            case SscPositionForms.Short:
            case SscPositionForms.VendorShort:
                buffer = new byte[20];

                break;
            default:
                buffer = new byte[32]; // Invalid

                break;
        }

        var cdb = new byte[10];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.ReadPosition;
        cdb[1] = (byte)((byte)responseForm & 0x1F);

        if(responseForm == SscPositionForms.Extended)
        {
            cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(buffer.Length & 0xFF);
        }

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.READ_POSITION_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads the specified number of blocks from the medium, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse6(out byte[] buffer, out byte[] senseBuffer, uint blocks, uint blockSize, uint timeout,
                             out double duration) => ReadReverse6(out buffer,
                                                                  out senseBuffer,
                                                                  false,
                                                                  false,
                                                                  true,
                                                                  blocks,
                                                                  blockSize,
                                                                  timeout,
                                                                  out duration);

    /// <summary>Reads the specified number of bytes or of blocks from the medium, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="transferLen">How many bytes to read.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse6(out byte[] buffer,  out byte[] senseBuffer, bool sili, uint transferLen, uint blockSize,
                             uint       timeout, out double duration) =>
        ReadReverse6(out buffer, out senseBuffer, false, sili, false, transferLen, blockSize, timeout, out duration);

    /// <summary>Reads the specified number of bytes or of blocks from the medium, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="byteOrder">If set to <c>true</c> drive should un-reverse the blocks and bytes</param>
    /// <param name="sili">
    ///     If set to <c>true</c> suppress the incorrect-length indication. Cannot be set while
    ///     <paramref name="fixedLen" /> is set also.
    /// </param>
    /// <param name="fixedLen">
    ///     If set to <c>true</c> <paramref name="transferLen" /> indicates how many blocks to read of a
    ///     fixed size.
    /// </param>
    /// <param name="transferLen">Transfer length in blocks or bytes depending of <paramref name="fixedLen" /> status.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse6(out byte[] buffer,      out byte[] senseBuffer, bool byteOrder, bool sili, bool fixedLen,
                             uint       transferLen, uint       blockSize,   uint timeout,   out double duration)
    {
        buffer = fixedLen ? new byte[blockSize * transferLen] : new byte[transferLen];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.ReadReverse;

        if(fixedLen) cdb[1] += 0x01;

        if(sili) cdb[1] += 0x02;

        if(byteOrder) cdb[1] += 0x04;

        cdb[2] = (byte)((transferLen & 0xFF0000) >> 16);
        cdb[3] = (byte)((transferLen & 0xFF00)   >> 8);
        cdb[4] = (byte)(transferLen & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.READ_REVERSE_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads a number of fixed-length blocks starting at specified object, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse16(out byte[] buffer,    out byte[] senseBuffer, bool sili, ulong objectId, uint blocks,
                              uint       blockSize, uint       timeout,     out double duration) =>
        ReadReverse16(out buffer,
                      out senseBuffer,
                      false,
                      sili,
                      false,
                      0,
                      objectId,
                      blocks,
                      blockSize,
                      timeout,
                      out duration);

    /// <summary>Reads a number of fixed-length blocks starting at specified block from the specified partition, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="partition">Partition to read object from.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse16(out byte[] buffer, out byte[] senseBuffer, bool sili,    byte partition, ulong objectId,
                              uint       blocks, uint       blockSize,   uint timeout, out double duration) =>
        ReadReverse16(out buffer,
                      out senseBuffer,
                      false,
                      sili,
                      false,
                      partition,
                      objectId,
                      blocks,
                      blockSize,
                      timeout,
                      out duration);

    /// <summary>Reads a number of fixed-length blocks starting at specified object, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse16(out byte[] buffer,  out byte[] senseBuffer, ulong objectId, uint blocks, uint blockSize,
                              uint       timeout, out double duration) =>
        ReadReverse16(out buffer,
                      out senseBuffer,
                      false,
                      false,
                      true,
                      0,
                      objectId,
                      blocks,
                      blockSize,
                      timeout,
                      out duration);

    /// <summary>Reads a number of fixed-length blocks starting at specified block from the specified partition, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="partition">Partition to read object from.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse16(out byte[] buffer,    out byte[] senseBuffer, byte partition, ulong objectId, uint blocks,
                              uint       blockSize, uint       timeout,     out double duration) =>
        ReadReverse16(out buffer,
                      out senseBuffer,
                      false,
                      false,
                      true,
                      partition,
                      objectId,
                      blocks,
                      blockSize,
                      timeout,
                      out duration);

    /// <summary>Reads a number of bytes or objects starting at specified object from the specified partition, backwards</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="byteOrder">If set to <c>true</c> drive should un-reverse the blocks and bytes</param>
    /// <param name="sili">
    ///     If set to <c>true</c> suppress the incorrect-length indication. Cannot be set while
    ///     <paramref name="fixedLen" /> is set also.
    /// </param>
    /// <param name="fixedLen">
    ///     If set to <c>true</c> <paramref name="transferLen" /> indicates how many blocks to read of a
    ///     fixed size.
    /// </param>
    /// <param name="partition">Partition to read object from.</param>
    /// <param name="objectId">Object identifier.</param>
    /// <param name="transferLen">Transfer length in blocks or bytes depending of <paramref name="fixedLen" /> status.</param>
    /// <param name="objectSize">Object size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadReverse16(out byte[] buffer,    out byte[] senseBuffer, bool byteOrder, bool sili, bool fixedLen,
                              byte       partition, ulong objectId, uint transferLen, uint objectSize, uint timeout,
                              out double duration)
    {
        buffer = fixedLen ? new byte[objectSize * transferLen] : new byte[transferLen];
        var cdb = new byte[6];
        senseBuffer = new byte[64];
        byte[] idBytes = BitConverter.GetBytes(objectId);

        cdb[0] = (byte)ScsiCommands.Read16;

        if(fixedLen) cdb[1] += 0x01;

        if(sili) cdb[1] += 0x02;

        if(byteOrder) cdb[1] += 0x04;

        cdb[3]  = partition;
        cdb[4]  = idBytes[7];
        cdb[5]  = idBytes[6];
        cdb[6]  = idBytes[5];
        cdb[7]  = idBytes[4];
        cdb[8]  = idBytes[3];
        cdb[9]  = idBytes[2];
        cdb[10] = idBytes[1];
        cdb[11] = idBytes[0];
        cdb[12] = (byte)((transferLen & 0xFF0000) >> 16);
        cdb[13] = (byte)((transferLen & 0xFF00)   >> 8);
        cdb[14] = (byte)(transferLen & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.READ_REVERSE_16_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads the specified number of blocks from the device's buffer</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="blocks">How many blocks to read.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool RecoverBufferedData(out byte[] buffer,  out byte[] senseBuffer, uint blocks, uint blockSize,
                                    uint       timeout, out double duration) =>
        RecoverBufferedData(out buffer, out senseBuffer, false, true, blocks, blockSize, timeout, out duration);

    /// <summary>Reads the specified number of bytes or of blocks from the device's buffer</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">If set to <c>true</c> suppress the incorrect-length indication.</param>
    /// <param name="transferLen">How many bytes to read.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool RecoverBufferedData(out byte[] buffer,    out byte[] senseBuffer, bool       sili, uint transferLen,
                                    uint       blockSize, uint       timeout,     out double duration) =>
        RecoverBufferedData(out buffer, out senseBuffer, sili, false, transferLen, blockSize, timeout, out duration);

    /// <summary>Reads the specified number of bytes or of blocks from the device's buffer</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sili">
    ///     If set to <c>true</c> suppress the incorrect-length indication. Cannot be set while
    ///     <paramref name="fixedLen" /> is set also.
    /// </param>
    /// <param name="fixedLen">
    ///     If set to <c>true</c> <paramref name="transferLen" /> indicates how many blocks to read of a
    ///     fixed size.
    /// </param>
    /// <param name="transferLen">Transfer length in blocks or bytes depending of <paramref name="fixedLen" /> status.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool RecoverBufferedData(out byte[] buffer,      out byte[] senseBuffer, bool sili,    bool       fixedLen,
                                    uint       transferLen, uint       blockSize,   uint timeout, out double duration)
    {
        buffer = fixedLen ? new byte[blockSize * transferLen] : new byte[transferLen];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.RecoverBufferedData;

        if(fixedLen) cdb[1] += 0x01;

        if(sili) cdb[1] += 0x02;

        cdb[2] = (byte)((transferLen & 0xFF0000) >> 16);
        cdb[3] = (byte)((transferLen & 0xFF00)   >> 8);
        cdb[4] = (byte)(transferLen & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.RECOVER_BUFFERED_DATA_took_0_ms, duration);

        return sense;
    }

    /// <summary>Requests the device to return descriptors for supported densities or medium types</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReportDensitySupport(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReportDensitySupport(out buffer, out senseBuffer, false, false, timeout, out duration);

    /// <summary>Requests the device to return descriptors for supported densities or medium types</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="currentMedia">If set to <c>true</c> descriptors should apply to currently inserted media.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReportDensitySupport(out byte[] buffer, out byte[] senseBuffer, bool currentMedia, uint timeout,
                                     out double duration) =>
        ReportDensitySupport(out buffer, out senseBuffer, false, currentMedia, timeout, out duration);

    /// <summary>Requests the device to return descriptors for supported densities or medium types</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="mediumType">If set to <c>true</c> descriptors should be about medium types.</param>
    /// <param name="currentMedia">If set to <c>true</c> descriptors should apply to currently inserted media.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReportDensitySupport(out byte[] buffer,  out byte[] senseBuffer, bool mediumType, bool currentMedia,
                                     uint       timeout, out double duration)
    {
        buffer = new byte[256];
        var cdb = new byte[10];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.ReportDensitySupport;

        if(currentMedia) cdb[1] += 0x01;

        if(mediumType) cdb[1] += 0x02;

        cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense) return true;

        var availableLength = (ushort)((buffer[0] << 8) + buffer[1] + 2);
        buffer      = new byte[availableLength];
        cdb[7]      = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8]      = (byte)(buffer.Length & 0xFF);
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.REPORT_DENSITY_SUPPORT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Positions the reading/writing element to the beginning of current partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Rewind(out byte[] senseBuffer, uint timeout, out double duration) =>
        Rewind(out senseBuffer, false, timeout, out duration);

    /// <summary>Positions the reading/writing element to the beginning of current partition</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c> return from the command immediately.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool Rewind(out byte[] senseBuffer, bool immediate, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.Rewind;

        if(immediate) cdb[1] += 0x01;

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.REWIND_took_0_ms, duration);

        return sense;
    }

    /// <summary>Selects the specified track</summary>
    /// <returns><c>true</c>, if select was tracked, <c>false</c> otherwise.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="track">Track.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool TrackSelect(out byte[] senseBuffer, byte track, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.TrackSelect;
        cdb[5] = track;

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.TRACK_SELECT_took_0_ms, duration);

        return sense;
    }

    /// <summary>Writes a space mark in the media</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="code">Space type code.</param>
    /// <param name="count">How many marks to write</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    /// <returns><c>true</c>, if select was tracked, <c>false</c> otherwise.</returns>
    public bool Space(out byte[] senseBuffer, SscSpaceCodes code, int count, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();
        byte[] countB = BitConverter.GetBytes(count);

        cdb[0] = (byte)ScsiCommands.Space;
        cdb[1] = (byte)((byte)code & 0x0F);
        cdb[2] = countB[2];
        cdb[3] = countB[1];
        cdb[4] = countB[0];

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.SPACE_took_0_ms, duration);

        return sense;
    }
}