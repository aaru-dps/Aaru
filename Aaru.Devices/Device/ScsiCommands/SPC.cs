// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SPC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI Primary Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SCSI commands defined in SPC standards.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.Console;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

namespace Aaru.Devices;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class Device
{
    /// <summary>Sends the SPC INQUIRY command to the device using default device timeout.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer) =>
        ScsiInquiry(out buffer, out senseBuffer, Timeout);

    /// <summary>Sends the SPC INQUIRY command to the device using default device timeout.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, out double duration) =>
        ScsiInquiry(out buffer, out senseBuffer, Timeout, out duration);

    /// <summary>Sends the SPC INQUIRY command to the device.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, uint timeout) =>
        ScsiInquiry(out buffer, out senseBuffer, timeout, out _);

    /// <summary>Sends the SPC INQUIRY command to the device.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        buffer      = new byte[36];
        senseBuffer = new byte[64];

        byte[] cdb =
        {
            (byte)ScsiCommands.Inquiry, 0, 0, 0, 36, 0
        };

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        byte pagesLength = (byte)(buffer[4] + 5);

        cdb = new byte[]
        {
            (byte)ScsiCommands.Inquiry, 0, 0, 0, pagesLength, 0
        };

        buffer      = new byte[pagesLength];
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.INQUIRY_took_0_ms, duration);

        return sense;
    }

    /// <summary>
    ///     Sends the SPC INQUIRY command to the device with an Extended Vital Product Data page using default device
    ///     timeout.
    /// </summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="page">The Extended Vital Product Data</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page) =>
        ScsiInquiry(out buffer, out senseBuffer, page, Timeout);

    /// <summary>
    ///     Sends the SPC INQUIRY command to the device with an Extended Vital Product Data page using default device
    ///     timeout.
    /// </summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="page">The Extended Vital Product Data</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, out double duration) =>
        ScsiInquiry(out buffer, out senseBuffer, page, Timeout, out duration);

    /// <summary>Sends the SPC INQUIRY command to the device with an Extended Vital Product Data page.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="page">The Extended Vital Product Data</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, uint timeout) =>
        ScsiInquiry(out buffer, out senseBuffer, page, timeout, out _);

    /// <summary>Sends the SPC INQUIRY command to the device with an Extended Vital Product Data page.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="page">The Extended Vital Product Data</param>
    public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, uint timeout, out double duration)
    {
        buffer      = new byte[36];
        senseBuffer = new byte[64];

        byte[] cdb =
        {
            (byte)ScsiCommands.Inquiry, 1, page, 0, 36, 0
        };

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        // This is because INQ was returned instead of EVPD
        if(buffer[1] != page)
            return true;

        byte pagesLength = (byte)(buffer[3] + 4);

        cdb = new byte[]
        {
            (byte)ScsiCommands.Inquiry, 1, page, 0, pagesLength, 0
        };

        buffer      = new byte[pagesLength];
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.INQUIRY_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC TEST UNIT READY command to the device</summary>
    /// <returns><c>true</c>, if unit is NOT ready, <c>false</c> otherwise.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ScsiTestUnitReady(out byte[] senseBuffer, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];

        byte[] cdb =
        {
            (byte)ScsiCommands.TestUnitReady, 0, 0, 0, 0, 0
        };

        byte[] buffer = Array.Empty<byte>();

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.TEST_UNIT_READY_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC MODE SENSE(6) command to the device as introduced in SCSI-1</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI MODE SENSE(6) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ModeSense(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ModeSense6(out buffer, out senseBuffer, false, ScsiModeSensePageControl.Current, 0, 0, timeout, out duration);

    /// <summary>Sends the SPC MODE SENSE(6) command to the device as introduced in SCSI-2</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI MODE SENSE(6) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="dbd">If set to <c>true</c> device MUST not return any block descriptor.</param>
    /// <param name="pageControl">Page control.</param>
    /// <param name="pageCode">Page code.</param>
    public bool ModeSense6(out byte[] buffer, out byte[] senseBuffer, bool dbd, ScsiModeSensePageControl pageControl,
                           byte pageCode, uint timeout, out double duration) =>
        ModeSense6(out buffer, out senseBuffer, dbd, pageControl, pageCode, 0, timeout, out duration);

    /// <summary>Sends the SPC MODE SENSE(6) command to the device as introduced in SCSI-3 SPC-3</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI MODE SENSE(6) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="dbd">If set to <c>true</c> device MUST not return any block descriptor.</param>
    /// <param name="pageControl">Page control.</param>
    /// <param name="pageCode">Page code.</param>
    /// <param name="subPageCode">Sub-page code.</param>
    public bool ModeSense6(out byte[] buffer, out byte[] senseBuffer, bool dbd, ScsiModeSensePageControl pageControl,
                           byte pageCode, byte subPageCode, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[6];
        buffer = new byte[255];

        cdb[0] = (byte)ScsiCommands.ModeSense;

        if(dbd)
            cdb[1] = 0x08;

        cdb[2] |= (byte)pageControl;
        cdb[2] |= (byte)(pageCode & 0x3F);
        cdb[3] =  subPageCode;
        cdb[4] =  (byte)buffer.Length;
        cdb[5] =  0;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        byte modeLength = (byte)(buffer[0] + 1);
        buffer      = new byte[modeLength];
        cdb[4]      = (byte)buffer.Length;
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.MODE_SENSE_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC MODE SENSE(10) command to the device as introduced in SCSI-2</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI MODE SENSE(10) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="dbd">If set to <c>true</c> device MUST not return any block descriptor.</param>
    /// <param name="pageControl">Page control.</param>
    /// <param name="pageCode">Page code.</param>
    public bool ModeSense10(out byte[] buffer, out byte[] senseBuffer, bool dbd, ScsiModeSensePageControl pageControl,
                            byte pageCode, uint timeout, out double duration) =>
        ModeSense10(out buffer, out senseBuffer, false, dbd, pageControl, pageCode, 0, timeout, out duration);

    /// <summary>Sends the SPC MODE SENSE(10) command to the device as introduced in SCSI-3 SPC-2</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI MODE SENSE(10) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="dbd">If set to <c>true</c> device MUST not return any block descriptor.</param>
    /// <param name="pageControl">Page control.</param>
    /// <param name="pageCode">Page code.</param>
    /// <param name="llbaa">If set means 64-bit LBAs are accepted by the caller.</param>
    public bool ModeSense10(out byte[] buffer, out byte[] senseBuffer, bool llbaa, bool dbd,
                            ScsiModeSensePageControl pageControl, byte pageCode, uint timeout, out double duration) =>
        ModeSense10(out buffer, out senseBuffer, llbaa, dbd, pageControl, pageCode, 0, timeout, out duration);

    /// <summary>Sends the SPC MODE SENSE(10) command to the device as introduced in SCSI-3 SPC-3</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI MODE SENSE(10) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="dbd">If set to <c>true</c> device MUST not return any block descriptor.</param>
    /// <param name="pageControl">Page control.</param>
    /// <param name="pageCode">Page code.</param>
    /// <param name="subPageCode">Sub-page code.</param>
    /// <param name="llbaa">If set means 64-bit LBAs are accepted by the caller.</param>
    public bool ModeSense10(out byte[] buffer, out byte[] senseBuffer, bool llbaa, bool dbd,
                            ScsiModeSensePageControl pageControl, byte pageCode, byte subPageCode, uint timeout,
                            out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];
        buffer = new byte[4096];

        cdb[0] = (byte)ScsiCommands.ModeSense10;

        if(llbaa)
            cdb[1] |= 0x10;

        if(dbd)
            cdb[1] |= 0x08;

        cdb[2] |= (byte)pageControl;
        cdb[2] |= (byte)(pageCode & 0x3F);
        cdb[3] =  subPageCode;
        cdb[7] =  (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8] =  (byte)(buffer.Length & 0xFF);
        cdb[9] =  0;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        ushort modeLength = (ushort)((buffer[0] << 8) + buffer[1] + 2);
        buffer      = new byte[modeLength];
        cdb[7]      = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8]      = (byte)(buffer.Length & 0xFF);
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.MODE_SENSE_10_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC PREVENT ALLOW MEDIUM REMOVAL command to prevent medium removal</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool SpcPreventMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration) =>
        SpcPreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Prevent, timeout, out duration);

    /// <summary>Sends the SPC PREVENT ALLOW MEDIUM REMOVAL command to allow medium removal</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool SpcAllowMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration) =>
        SpcPreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Allow, timeout, out duration);

    /// <summary>Sends the SPC PREVENT ALLOW MEDIUM REMOVAL command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="prevent"><c>true</c> to prevent medium removal, <c>false</c> to allow it.</param>
    public bool SpcPreventAllowMediumRemoval(out byte[] senseBuffer, bool prevent, uint timeout, out double duration) =>
        prevent ? SpcPreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Prevent, timeout, out duration)
            : SpcPreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Allow, timeout, out duration);

    /// <summary>Sends the SPC PREVENT ALLOW MEDIUM REMOVAL command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="preventMode">Prevention mode.</param>
    public bool SpcPreventAllowMediumRemoval(out byte[] senseBuffer, ScsiPreventAllowMode preventMode, uint timeout,
                                             out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.PreventAllowMediumRemoval;
        cdb[4] = (byte)((byte)preventMode & 0x03);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.PREVENT_ALLOW_MEDIUM_REMOVAL_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC READ CAPACITY command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ CAPACITY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadCapacity(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadCapacity(out buffer, out senseBuffer, false, 0, false, timeout, out duration);

    /// <summary>Sends the SPC READ CAPACITY command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ CAPACITY response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="relAddr">Indicates that <paramref name="address" /> is relative to current medium position</param>
    /// <param name="address">Address where information is requested from, only valid if <paramref name="pmi" /> is set</param>
    /// <param name="pmi">If set, it is requesting partial media capacity</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadCapacity(out byte[] buffer, out byte[] senseBuffer, bool relAddr, uint address, bool pmi,
                             uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];
        buffer = new byte[8];

        cdb[0] = (byte)ScsiCommands.ReadCapacity;

        if(pmi)
        {
            cdb[8] = 0x01;

            if(relAddr)
                cdb[1] = 0x01;

            cdb[2] = (byte)((address & 0xFF000000) >> 24);
            cdb[3] = (byte)((address & 0xFF0000)   >> 16);
            cdb[4] = (byte)((address & 0xFF00)     >> 8);
            cdb[5] = (byte)(address & 0xFF);
        }

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_CAPACITY_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC READ CAPACITY(16) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ CAPACITY(16) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadCapacity16(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadCapacity16(out buffer, out senseBuffer, 0, false, timeout, out duration);

    /// <summary>Sends the SPC READ CAPACITY(16) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ CAPACITY(16) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="address">Address where information is requested from, only valid if <paramref name="pmi" /> is set</param>
    /// <param name="pmi">If set, it is requesting partial media capacity</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadCapacity16(out byte[] buffer, out byte[] senseBuffer, ulong address, bool pmi, uint timeout,
                               out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[16];
        buffer = new byte[32];

        cdb[0] = (byte)ScsiCommands.ServiceActionIn;
        cdb[1] = (byte)ScsiServiceActions.ReadCapacity16;

        if(pmi)
        {
            cdb[14] = 0x01;
            byte[] temp = BitConverter.GetBytes(address);
            cdb[2] = temp[7];
            cdb[3] = temp[6];
            cdb[4] = temp[5];
            cdb[5] = temp[4];
            cdb[6] = temp[3];
            cdb[7] = temp[2];
            cdb[8] = temp[1];
            cdb[9] = temp[0];
        }

        cdb[10] = (byte)((buffer.Length & 0xFF000000) >> 24);
        cdb[11] = (byte)((buffer.Length & 0xFF0000)   >> 16);
        cdb[12] = (byte)((buffer.Length & 0xFF00)     >> 8);
        cdb[13] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_CAPACITY_16_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC READ MEDIA SERIAL NUMBER command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ MEDIA SERIAL NUMBER response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadMediaSerialNumber(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];
        buffer = new byte[4];

        cdb[0] = (byte)ScsiCommands.ReadSerialNumber;
        cdb[1] = 0x01;
        cdb[6] = (byte)((buffer.Length & 0xFF000000) >> 24);
        cdb[7] = (byte)((buffer.Length & 0xFF0000)   >> 16);
        cdb[8] = (byte)((buffer.Length & 0xFF00)     >> 8);
        cdb[9] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        uint strctLength = (uint)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3] + 4);
        buffer      = new byte[strctLength];
        cdb[6]      = (byte)((buffer.Length & 0xFF000000) >> 24);
        cdb[7]      = (byte)((buffer.Length & 0xFF0000)   >> 16);
        cdb[8]      = (byte)((buffer.Length & 0xFF00)     >> 8);
        cdb[9]      = (byte)(buffer.Length & 0xFF);
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_MEDIA_SERIAL_NUMBER_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads an attribute from the medium auxiliary memory</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
    /// <param name="partition">Partition number.</param>
    /// <param name="firstAttribute">First attribute identifier.</param>
    /// <param name="cache">If set to <c>true</c> device can return cached data.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action, byte partition,
                              ushort firstAttribute, bool cache, uint timeout, out double duration) =>
        ReadAttribute(out buffer, out senseBuffer, action, 0, 0, 0, partition, firstAttribute, cache, timeout,
                      out duration);

    /// <summary>Reads an attribute from the medium auxiliary memory</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
    /// <param name="firstAttribute">First attribute identifier.</param>
    /// <param name="cache">If set to <c>true</c> device can return cached data.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action,
                              ushort firstAttribute, bool cache, uint timeout, out double duration) =>
        ReadAttribute(out buffer, out senseBuffer, action, 0, 0, 0, 0, firstAttribute, cache, timeout, out duration);

    /// <summary>Reads an attribute from the medium auxiliary memory</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
    /// <param name="partition">Partition number.</param>
    /// <param name="firstAttribute">First attribute identifier.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action, byte partition,
                              ushort firstAttribute, uint timeout, out double duration) =>
        ReadAttribute(out buffer, out senseBuffer, action, 0, 0, 0, partition, firstAttribute, false, timeout,
                      out duration);

    /// <summary>Reads an attribute from the medium auxiliary memory</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
    /// <param name="firstAttribute">First attribute identifier.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action,
                              ushort firstAttribute, uint timeout, out double duration) =>
        ReadAttribute(out buffer, out senseBuffer, action, 0, 0, 0, 0, firstAttribute, false, timeout, out duration);

    /// <summary>Reads an attribute from the medium auxiliary memory</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
    /// <param name="volume">Volume number.</param>
    /// <param name="partition">Partition number.</param>
    /// <param name="firstAttribute">First attribute identifier.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action, byte volume,
                              byte partition, ushort firstAttribute, uint timeout, out double duration) =>
        ReadAttribute(out buffer, out senseBuffer, action, 0, 0, volume, partition, firstAttribute, false, timeout,
                      out duration);

    /// <summary>Reads an attribute from the medium auxiliary memory</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
    /// <param name="volume">Volume number.</param>
    /// <param name="partition">Partition number.</param>
    /// <param name="firstAttribute">First attribute identifier.</param>
    /// <param name="cache">If set to <c>true</c> device can return cached data.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action, byte volume,
                              byte partition, ushort firstAttribute, bool cache, uint timeout, out double duration) =>
        ReadAttribute(out buffer, out senseBuffer, action, 0, 0, volume, partition, firstAttribute, cache, timeout,
                      out duration);

    /// <summary>Sends the SPC MODE SELECT(6) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer with the data to be sent to the device</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="savePages">Set to save pages between resets.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="pageFormat">Set if page is formatted.</param>
    public bool ModeSelect(byte[] buffer, out byte[] senseBuffer, bool pageFormat, bool savePages, uint timeout,
                           out double duration)
    {
        senseBuffer = new byte[64];

        // Prevent overflows
        if(buffer.Length > 255)
        {
            if(PlatformId != PlatformID.Win32NT      &&
               PlatformId != PlatformID.Win32S       &&
               PlatformId != PlatformID.Win32Windows &&
               PlatformId != PlatformID.WinCE        &&
               PlatformId != PlatformID.WindowsPhone &&
               PlatformId != PlatformID.Xbox)
                LastError = 75;
            else
                LastError = 111;

            Error    = true;
            duration = 0;

            return true;
        }

        byte[] cdb = new byte[6];

        cdb[0] = (byte)ScsiCommands.ModeSelect;

        if(pageFormat)
            cdb[1] += 0x10;

        if(savePages)
            cdb[1] += 0x01;

        cdb[4] = (byte)buffer.Length;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.MODE_SELECT_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the SPC MODE SELECT(10) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer with the data to be sent to the device</param>
    /// <param name="savePages">Set to save pages between resets.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="pageFormat">Set if page is formatted.</param>
    public bool ModeSelect10(byte[] buffer, out byte[] senseBuffer, bool pageFormat, bool savePages, uint timeout,
                             out double duration)
    {
        senseBuffer = new byte[64];

        // Prevent overflows
        if(buffer.Length > 65535)
        {
            if(PlatformId != PlatformID.Win32NT      &&
               PlatformId != PlatformID.Win32S       &&
               PlatformId != PlatformID.Win32Windows &&
               PlatformId != PlatformID.WinCE        &&
               PlatformId != PlatformID.WindowsPhone &&
               PlatformId != PlatformID.Xbox)
                LastError = 75;
            else
                LastError = 111;

            Error    = true;
            duration = 0;

            return true;
        }

        byte[] cdb = new byte[10];

        cdb[0] = (byte)ScsiCommands.ModeSelect10;

        if(pageFormat)
            cdb[1] += 0x10;

        if(savePages)
            cdb[1] += 0x01;

        cdb[7] = (byte)((buffer.Length & 0xFF00) << 8);
        cdb[8] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.MODE_SELECT_10_took_0_ms, duration);

        return sense;
    }

    /// <summary>Requests the device fixed sense</summary>
    /// <param name="buffer">Sense buffer</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed.</returns>
    public bool RequestSense(out byte[] buffer, uint timeout, out double duration) =>
        RequestSense(false, out buffer, timeout, out duration);

    /// <summary>Requests the device sense</summary>
    /// <param name="descriptor">Request a descriptor sense</param>
    /// <param name="buffer">Sense buffer</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed.</returns>
    public bool RequestSense(bool descriptor, out byte[] buffer, uint timeout, out double duration)
    {
        byte[] cdb = new byte[6];
        buffer = new byte[252];

        cdb[0] = (byte)ScsiCommands.RequestSense;

        if(descriptor)
            cdb[1] = 0x01;

        cdb[2] = 0;
        cdb[3] = 0;
        cdb[4] = (byte)buffer.Length;
        cdb[5] = 0;

        LastError = SendScsiCommand(cdb, ref buffer, out _, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.REQUEST_SENSE_took_0_ms, duration);

        return sense;
    }
}