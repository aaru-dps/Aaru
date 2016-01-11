// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiCommands.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Direct device access
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains SCSI commands
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.Console;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Sends the SCSI INQUIRY command to the device using default device timeout.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer)
        {
            return ScsiInquiry(out buffer, out senseBuffer, Timeout);
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device using default device timeout.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, out double duration)
        {
            return ScsiInquiry(out buffer, out senseBuffer, Timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, uint timeout)
        {
            double duration;
            return ScsiInquiry(out buffer, out senseBuffer, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[36];
            senseBuffer = new byte[32];
            byte[] cdb = { (byte)ScsiCommands.Inquiry, 0, 0, 0, 36, 0 };
            bool sense;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            byte pagesLength = (byte)(buffer[4] + 5);

            cdb = new byte[] { (byte)ScsiCommands.Inquiry, 0, 0, 0, pagesLength, 0 };
            buffer = new byte[pagesLength];
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "INQUIRY took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device with an Extended Vital Product Data page using default device timeout.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="page">The Extended Vital Product Data</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page)
        {
            return ScsiInquiry(out buffer, out senseBuffer, page, Timeout);
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device with an Extended Vital Product Data page using default device timeout.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="page">The Extended Vital Product Data</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, out double duration)
        {
            return ScsiInquiry(out buffer, out senseBuffer, page, Timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device with an Extended Vital Product Data page.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="page">The Extended Vital Product Data</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, uint timeout)
        {
            double duration;
            return ScsiInquiry(out buffer, out senseBuffer, page, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI INQUIRY command to the device with an Extended Vital Product Data page.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI INQUIRY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="page">The Extended Vital Product Data</param>
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, uint timeout, out double duration)
        {
            buffer = new byte[36];
            senseBuffer = new byte[32];
            byte[] cdb = { (byte)ScsiCommands.Inquiry, 1, page, 0, 36, 0 };
            bool sense;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            // This is because INQ was returned instead of EVPD
            if (buffer[1] != page)
                return true;

            byte pagesLength = (byte)(buffer[3] + 4);

            cdb = new byte[] { (byte)ScsiCommands.Inquiry, 1, page, 0, pagesLength, 0 };
            buffer = new byte[pagesLength];
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "INQUIRY took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI TEST UNIT READY command to the device
        /// </summary>
        /// <returns><c>true</c>, if unit is NOT ready, <c>false</c> otherwise.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ScsiTestUnitReady(out byte[] senseBuffer, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = { (byte)ScsiCommands.TestUnitReady, 0, 0, 0, 0, 0 };
            bool sense;
            byte[] buffer = new byte[0];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "TEST UNIT READY took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI MODE SENSE(6) command to the device as introduced in SCSI-1
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI MODE SENSE(6) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ModeSense(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ModeSense6(out buffer, out senseBuffer, false, ScsiModeSensePageControl.Current, 0, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI MODE SENSE(6) command to the device as introduced in SCSI-2
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI MODE SENSE(6) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="DBD">If set to <c>true</c> device MUST not return any block descriptor.</param>
        /// <param name="pageControl">Page control.</param>
        /// <param name="pageCode">Page code.</param>
        public bool ModeSense6(out byte[] buffer, out byte[] senseBuffer, bool DBD, ScsiModeSensePageControl pageControl, byte pageCode, uint timeout, out double duration)
        {
            return ModeSense6(out buffer, out senseBuffer, DBD, pageControl, pageCode, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI MODE SENSE(6) command to the device as introduced in SCSI-3 SPC-3
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI MODE SENSE(6) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="DBD">If set to <c>true</c> device MUST not return any block descriptor.</param>
        /// <param name="pageControl">Page control.</param>
        /// <param name="pageCode">Page code.</param>
        /// <param name="subPageCode">Sub-page code.</param>
        public bool ModeSense6(out byte[] buffer, out byte[] senseBuffer, bool DBD, ScsiModeSensePageControl pageControl, byte pageCode, byte subPageCode, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[6];
            buffer = new byte[4];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ModeSense;
            if (DBD)
                cdb[1] = 0x08;
            cdb[2] |= (byte)pageControl;
            cdb[2] |= (byte)(pageCode & 0x3F);
            cdb[3] = subPageCode;
            cdb[4] = (byte)buffer.Length;
            cdb[5] = 0;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            byte modeLength = (byte)(buffer[0] + 1);
            buffer = new byte[modeLength];
            cdb[4] = (byte)buffer.Length;
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "MODE SENSE(6) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI MODE SENSE(10) command to the device as introduced in SCSI-2
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI MODE SENSE(10) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="DBD">If set to <c>true</c> device MUST not return any block descriptor.</param>
        /// <param name="pageControl">Page control.</param>
        /// <param name="pageCode">Page code.</param>
        public bool ModeSense10(out byte[] buffer, out byte[] senseBuffer, bool DBD, ScsiModeSensePageControl pageControl, byte pageCode, uint timeout, out double duration)
        {
            return ModeSense10(out buffer, out senseBuffer, false, DBD, pageControl, pageCode, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI MODE SENSE(10) command to the device as introduced in SCSI-3 SPC-2
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI MODE SENSE(10) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="DBD">If set to <c>true</c> device MUST not return any block descriptor.</param>
        /// <param name="pageControl">Page control.</param>
        /// <param name="pageCode">Page code.</param>
        /// <param name="LLBAA">If set means 64-bit LBAs are accepted by the caller.</param>
        public bool ModeSense10(out byte[] buffer, out byte[] senseBuffer, bool LLBAA, bool DBD, ScsiModeSensePageControl pageControl, byte pageCode, uint timeout, out double duration)
        {
            return ModeSense10(out buffer, out senseBuffer, LLBAA, DBD, pageControl, pageCode, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI MODE SENSE(10) command to the device as introduced in SCSI-3 SPC-3
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI MODE SENSE(10) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="DBD">If set to <c>true</c> device MUST not return any block descriptor.</param>
        /// <param name="pageControl">Page control.</param>
        /// <param name="pageCode">Page code.</param>
        /// <param name="subPageCode">Sub-page code.</param>
        /// <param name="LLBAA">If set means 64-bit LBAs are accepted by the caller.</param>
        public bool ModeSense10(out byte[] buffer, out byte[] senseBuffer, bool LLBAA, bool DBD, ScsiModeSensePageControl pageControl, byte pageCode, byte subPageCode, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            buffer = new byte[4096];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ModeSense10;
            if (LLBAA)
                cdb[1] |= 0x10;
            if (DBD)
                cdb[1] |= 0x08;
            cdb[2] |= (byte)pageControl;
            cdb[2] |= (byte)(pageCode & 0x3F);
            cdb[3] = subPageCode;
            cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(buffer.Length & 0xFF);
            cdb[9] = 0;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            ushort modeLength = (ushort)(((int)buffer[0] << 8) + buffer[1] + 2);
            buffer = new byte[modeLength];
            cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(buffer.Length & 0xFF);
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "MODE SENSE(10) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI PREVENT ALLOW MEDIUM REMOVAL command to prevent medium removal
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool PreventMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return PreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Prevent, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI PREVENT ALLOW MEDIUM REMOVAL command to allow medium removal
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool AllowMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return PreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Allow, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI PREVENT ALLOW MEDIUM REMOVAL command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="prevent"><c>true</c> to prevent medium removal, <c>false</c> to allow it.</param>
        public bool PreventAllowMediumRemoval(out byte[] senseBuffer, bool prevent, uint timeout, out double duration)
        {
            if (prevent)
                return PreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Prevent, timeout, out duration);
            else
                return PreventAllowMediumRemoval(out senseBuffer, ScsiPreventAllowMode.Allow, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI PREVENT ALLOW MEDIUM REMOVAL command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="preventMode">Prevention mode.</param>
        public bool PreventAllowMediumRemoval(out byte[] senseBuffer, ScsiPreventAllowMode preventMode, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[6];
            bool sense;
            byte[] buffer = new byte[0];

            cdb[0] = (byte)ScsiCommands.PreventAllowMediumRemoval;
            cdb[4] = (byte)((byte)preventMode & 0x03);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PREVENT ALLOW MEDIUM REMOVAL took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI GET CONFIGURATION command for all Features
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI GET CONFIGURATION response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool GetConfiguration(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return GetConfiguration(out buffer, out senseBuffer, 0x0000, MmcGetConfigurationRt.All, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI GET CONFIGURATION command for all Features starting with specified one
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI GET CONFIGURATION response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="startingFeatureNumber">Feature number where the feature list should start from</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool GetConfiguration(out byte[] buffer, out byte[] senseBuffer, ushort startingFeatureNumber, uint timeout, out double duration)
        {
            return GetConfiguration(out buffer, out senseBuffer, startingFeatureNumber, MmcGetConfigurationRt.All, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI GET CONFIGURATION command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI GET CONFIGURATION response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="startingFeatureNumber">Starting Feature number.</param>
        /// <param name="RT">Return type, <see cref="MmcGetConfigurationRt"/>.</param>
        public bool GetConfiguration(out byte[] buffer, out byte[] senseBuffer, ushort startingFeatureNumber, MmcGetConfigurationRt RT, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            buffer = new byte[8];
            bool sense;

            cdb[0] = (byte)ScsiCommands.GetConfiguration;
            cdb[1] = (byte)((byte)RT & 0x03);
            cdb[2] = (byte)((startingFeatureNumber & 0xFF00) >> 8);
            cdb[3] = (byte)(startingFeatureNumber & 0xFF);
            cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(buffer.Length & 0xFF);
            cdb[9] = 0;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            ushort confLength = (ushort)(((int)buffer[2] << 8) + buffer[3] + 4);
            buffer = new byte[confLength];
            cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(buffer.Length & 0xFF);
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "GET CONFIGURATION took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ DISC STRUCTURE command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ DISC STRUCTURE response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="mediaType">Medium type for requested disc structure</param>
        /// <param name="address">Medium address for requested disc structure</param>
        /// <param name="layerNumber">Medium layer for requested disc structure</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="format">Which disc structure are we requesting</param>
        /// <param name="AGID">AGID used in medium copy protection</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadDiscStructure(out byte[] buffer, out byte[] senseBuffer, MmcDiscStructureMediaType mediaType, uint address, byte layerNumber, MmcDiscStructureFormat format, byte AGID, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            buffer = new byte[8];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadDiscStructure;
            cdb[1] = (byte)((byte)mediaType & 0x0F);
            cdb[2] = (byte)((address & 0xFF000000) >> 24);
            cdb[3] = (byte)((address & 0xFF0000) >> 16);
            cdb[4] = (byte)((address & 0xFF00) >> 8);
            cdb[5] = (byte)(address & 0xFF);
            cdb[6] = layerNumber;
            cdb[7] = (byte)format;
            cdb[8] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[9] = (byte)(buffer.Length & 0xFF);
            cdb[10] = (byte)((AGID & 0x03) << 6);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            ushort strctLength = (ushort)(((int)buffer[0] << 8) + buffer[1] + 2);
            buffer = new byte[strctLength];
            cdb[8] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[9] = (byte)(buffer.Length & 0xFF);
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ DISC STRUCTURE took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ CAPACITY command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ CAPACITY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadCapacity(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadCapacity(out buffer, out senseBuffer, false, 0, false, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ CAPACITY command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ CAPACITY response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="RelAddr">Indicates that <paramref name="address"/> is relative to current medium position</param>
        /// <param name="address">Address where information is requested from, only valid if <paramref name="PMI"/> is set</param>
        /// <param name="PMI">If set, it is requesting partial media capacity</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadCapacity(out byte[] buffer, out byte[] senseBuffer, bool RelAddr, uint address, bool PMI, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            buffer = new byte[8];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadCapacity;

            if (PMI)
            {
                cdb[8] = 0x01;
                if (RelAddr)
                    cdb[1] = 0x01;

                cdb[2] = (byte)((address & 0xFF000000) >> 24);
                cdb[3] = (byte)((address & 0xFF0000) >> 16);
                cdb[4] = (byte)((address & 0xFF00) >> 8);
                cdb[5] = (byte)(address & 0xFF);
            }

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CAPACITY took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ CAPACITY(16) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ CAPACITY(16) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadCapacity16(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadCapacity16(out buffer, out senseBuffer, 0, false, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ CAPACITY(16) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ CAPACITY(16) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="address">Address where information is requested from, only valid if <paramref name="PMI"/> is set</param>
        /// <param name="PMI">If set, it is requesting partial media capacity</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadCapacity16(out byte[] buffer, out byte[] senseBuffer, ulong address, bool PMI, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[16];
            buffer = new byte[32];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ServiceActionIn;
            cdb[1] = (byte)ScsiServiceActions.ReadCapacity16;

            if (PMI)
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
            cdb[11] = (byte)((buffer.Length & 0xFF0000) >> 16);
            cdb[12] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[13] = (byte)(buffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CAPACITY(16) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ MEDIA SERIAL NUMBER command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ MEDIA SERIAL NUMBER response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadMediaSerialNumber(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            buffer = new byte[4];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadSerialNumber;
            cdb[1] = 0x01;
            cdb[6] = (byte)((buffer.Length & 0xFF000000) >> 24);
            cdb[7] = (byte)((buffer.Length & 0xFF0000) >> 16);
            cdb[8] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[9] = (byte)(buffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            if (sense)
                return true;

            uint strctLength = (uint)(((int)buffer[0] << 24) + ((int)buffer[1] << 16) + ((int)buffer[2] << 8) + buffer[3] + 4);
            cdb[6] = (byte)((buffer.Length & 0xFF000000) >> 24);
            cdb[7] = (byte)((buffer.Length & 0xFF0000) >> 16);
            cdb[8] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[9] = (byte)(buffer.Length & 0xFF);
            buffer = new byte[strctLength];
            senseBuffer = new byte[32];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ MEDIA SERIAL NUMBER took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get formatted TOC from disc, in MM:SS:FF format
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="track">Start TOC from this track</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadToc(out byte[] buffer, out byte[] senseBuffer, byte track, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, true, 0, track, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get formatted TOC from disc
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="MSF">If <c>true</c>, request data in MM:SS:FF units, otherwise, in blocks</param>
        /// <param name="track">Start TOC from this track</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadToc(out byte[] buffer, out byte[] senseBuffer, bool MSF, byte track, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, MSF, 0, track, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get multi-session information, in MM:SS:FF format
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadSessionInfo(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, true, 1, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get multi-session information
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="MSF">If <c>true</c>, request data in MM:SS:FF units, otherwise, in blocks</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadSessionInfo(out byte[] buffer, out byte[] senseBuffer, bool MSF, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, MSF, 1, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get raw TOC subchannels
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="sessionNumber">Session which TOC to get</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadRawToc(out byte[] buffer, out byte[] senseBuffer, byte sessionNumber, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, true, 2, sessionNumber, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get PMA
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadPma(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, true, 3, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get ATIP
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadAtip(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, true, 4, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command to get Lead-In CD-TEXT
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadCdText(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadTocPmaAtip(out buffer, out senseBuffer, true, 5, 0, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ TOC/PMA/ATIP command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="MSF">If <c>true</c>, request data in MM:SS:FF units, otherwise, in blocks</param>
        /// <param name="format">What structure is requested</param>
        /// <param name="trackSessionNumber">Track/Session number</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadTocPmaAtip(out byte[] buffer, out byte[] senseBuffer, bool MSF, byte format, byte trackSessionNumber, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            byte[] tmpBuffer;
            bool sense;

            if(format == 5)
                tmpBuffer = new byte[32768];
            else
                tmpBuffer = new byte[1024];

            cdb[0] = (byte)ScsiCommands.ReadTocPmaAtip;
            if (MSF)
                cdb[1] = 0x02;
            cdb[2] = (byte)(format & 0x0F);
            cdb[6] = trackSessionNumber;
            cdb[7] = (byte)((tmpBuffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(tmpBuffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref tmpBuffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            uint strctLength = (uint)(((int)tmpBuffer[0] << 8) + tmpBuffer[1] + 2);
            buffer = new byte[strctLength];

            if (buffer.Length <= tmpBuffer.Length)
            {
                Array.Copy(tmpBuffer, 0, buffer, 0, buffer.Length);
                DicConsole.DebugWriteLine("SCSI Device", "READ TOC/PMA/ATIP took {0} ms.", duration);
                return sense;
            }

            double tmpDuration = duration;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            duration += tmpDuration;
            DicConsole.DebugWriteLine("SCSI Device", "READ TOC/PMA/ATIP took {0} ms.", duration);
            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ DISC INFORMATION command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ DISC INFORMATION response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadDiscInformation(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            return ReadDiscInformation(out buffer, out senseBuffer, MmcDiscInformationDataTypes.DiscInformation, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ DISC INFORMATION command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ DISC INFORMATION response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="dataType">Which disc information to read</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        public bool ReadDiscInformation(out byte[] buffer, out byte[] senseBuffer, MmcDiscInformationDataTypes dataType, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            byte[] tmpBuffer = new byte[804];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadDiscInformation;
            cdb[1] = (byte)dataType;
            cdb[7] = (byte)((tmpBuffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(tmpBuffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref tmpBuffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            uint strctLength = (uint)(((int)tmpBuffer[0] << 8) + tmpBuffer[1] + 2);
            buffer = new byte[strctLength];
            Array.Copy(tmpBuffer, 0, buffer, 0, buffer.Length);

            DicConsole.DebugWriteLine("SCSI Device", "READ DISC INFORMATION took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ (6) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Starting block.</param>
        /// <param name="blockSize">Block size in bytes.</param>
        public bool Read6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint timeout, out double duration)
        {
            return Read6(out buffer, out senseBuffer, lba, blockSize, 1, timeout, out duration);
        }

        /// <summary>
        /// Sends the SCSI READ (6) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Starting block.</param>
        /// <param name="blockSize">Block size in bytes.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool Read6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, byte transferLength, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[6];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Read6;
            cdb[1] = (byte)((lba & 0x1F0000) >> 16);
            cdb[2] = (byte)((lba & 0xFF00) >> 8);
            cdb[3] = (byte)(lba & 0xFF);
            cdb[4] = transferLength;

            if(transferLength == 0)
                buffer = new byte[256 * blockSize];
            else
                buffer = new byte[transferLength * blockSize];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ (6) took {0} ms.", duration);
        
            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ (10) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="rdprotect">Instructs the drive how to check for protection information on the medium.</param>
        /// <param name="dpo">If set to <c>true</c> requested blocks shall be assigned the lowest retention priority on cache fetch/retain.</param>
        /// <param name="fua">If set to <c>true</c> requested blocks MUST bu read from medium and not the cache.</param>
        /// <param name="fuaNv">If set to <c>true</c> requested blocks will be returned from non-volatile cache. If they're not present they shall be stored there.</param>
        /// <param name="lba">Starting block.</param>
        /// <param name="blockSize">Block size in bytes.</param>
        /// <param name="groupNumber">Group number where attributes associated with this command should be collected.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="relAddr">If set to <c>true</c> address is relative to current position.</param>
        public bool Read10(out byte[] buffer, out byte[] senseBuffer, byte rdprotect, bool dpo, bool fua, bool fuaNv, bool relAddr, uint lba, uint blockSize, byte groupNumber, ushort transferLength, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            bool sense;

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
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[6] = (byte)(groupNumber & 0x1F);
            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[transferLength * blockSize];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ (10) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ (12) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="rdprotect">Instructs the drive how to check for protection information on the medium.</param>
        /// <param name="dpo">If set to <c>true</c> requested blocks shall be assigned the lowest retention priority on cache fetch/retain.</param>
        /// <param name="fua">If set to <c>true</c> requested blocks MUST bu read from medium and not the cache.</param>
        /// <param name="fuaNv">If set to <c>true</c> requested blocks will be returned from non-volatile cache. If they're not present they shall be stored there.</param>
        /// <param name="lba">Starting block.</param>
        /// <param name="blockSize">Block size in bytes.</param>
        /// <param name="groupNumber">Group number where attributes associated with this command should be collected.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="streaming">If set to <c>true</c> the stream playback operation should be used (MMC only).</param>
        /// <param name="relAddr">If set to <c>true</c> address is relative to current position.</param>
        public bool Read12(out byte[] buffer, out byte[] senseBuffer, byte rdprotect, bool dpo, bool fua, bool fuaNv, bool relAddr, uint lba, uint blockSize, byte groupNumber, uint transferLength, bool streaming, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

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
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[6] = (byte)((transferLength & 0xFF000000) >> 24);
            cdb[7] = (byte)((transferLength & 0xFF0000) >> 16);
            cdb[8] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[9] = (byte)(transferLength & 0xFF);
            cdb[10] = (byte)(groupNumber & 0x1F);
            if(streaming)
                cdb[10] += 0x80;

            buffer = new byte[transferLength * blockSize];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ (12) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ (16) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="rdprotect">Instructs the drive how to check for protection information on the medium.</param>
        /// <param name="dpo">If set to <c>true</c> requested blocks shall be assigned the lowest retention priority on cache fetch/retain.</param>
        /// <param name="fua">If set to <c>true</c> requested blocks MUST bu read from medium and not the cache.</param>
        /// <param name="fuaNv">If set to <c>true</c> requested blocks will be returned from non-volatile cache. If they're not present they shall be stored there.</param>
        /// <param name="lba">Starting block.</param>
        /// <param name="blockSize">Block size in bytes.</param>
        /// <param name="groupNumber">Group number where attributes associated with this command should be collected.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="streaming">If set to <c>true</c> the stream playback operation should be used (MMC only).</param>
        public bool Read16(out byte[] buffer, out byte[] senseBuffer, byte rdprotect, bool dpo, bool fua, bool fuaNv, ulong lba, uint blockSize, byte groupNumber, uint transferLength, bool streaming, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[16];
            bool sense;
            byte[] lbaBytes = BitConverter.GetBytes(lba);

            cdb[0] = (byte)ScsiCommands.Read16;
            cdb[1] = (byte)((rdprotect & 0x07) << 5);
            if(dpo)
                cdb[1] += 0x10;
            if(fua)
                cdb[1] += 0x08;
            if(fuaNv)
                cdb[1] += 0x02;
            cdb[2] = lbaBytes[7];
            cdb[3] = lbaBytes[6];
            cdb[4] = lbaBytes[5];
            cdb[5] = lbaBytes[4];
            cdb[6] = lbaBytes[3];
            cdb[7] = lbaBytes[2];
            cdb[8] = lbaBytes[1];
            cdb[9] = lbaBytes[0];
            cdb[10] = (byte)((transferLength & 0xFF000000) >> 24);
            cdb[11] = (byte)((transferLength & 0xFF0000) >> 16);
            cdb[12] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[13] = (byte)(transferLength & 0xFF);
            cdb[14] = (byte)(groupNumber & 0x1F);
            if(streaming)
                cdb[14] += 0x80;

            buffer = new byte[transferLength * blockSize];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ (16) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ LONG (10) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ LONG response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name = "relAddr"></param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="correct">If set to <c>true</c> ask the drive to try to correct errors in the sector.</param>
        /// <param name="lba">LBA to read.</param>
        /// <param name="transferBytes">How many bytes to read. If the number is not exactly the drive's size, the command will fail and incidate a delta of the size in SENSE.</param>
        public bool ReadLong10(out byte[] buffer, out byte[] senseBuffer, bool correct, bool relAddr, uint lba, ushort transferBytes, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadLong;
            if(correct)
                cdb[1] += 0x02;
            if(relAddr)
                cdb[1] += 0x01;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[7] = (byte)((transferBytes & 0xFF00) >> 8);
            cdb[8] = (byte)(transferBytes & 0xFF);

            buffer = new byte[transferBytes];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ LONG (10) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the SCSI READ LONG (16) command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the SCSI READ LONG response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="correct">If set to <c>true</c> ask the drive to try to correct errors in the sector.</param>
        /// <param name="lba">LBA to read.</param>
        /// <param name="transferBytes">How many bytes to read. If the number is not exactly the drive's size, the command will fail and incidate a delta of the size in SENSE.</param>
        public bool ReadLong16(out byte[] buffer, out byte[] senseBuffer, bool correct, ulong lba, uint transferBytes, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[16];
            bool sense;
            byte[] lbaBytes = BitConverter.GetBytes(lba);

            cdb[0] = (byte)ScsiCommands.ServiceActionIn;
            cdb[1] = (byte)ScsiServiceActions.ReadLong16;
            cdb[2] = lbaBytes[7];
            cdb[3] = lbaBytes[6];
            cdb[4] = lbaBytes[5];
            cdb[5] = lbaBytes[4];
            cdb[6] = lbaBytes[3];
            cdb[7] = lbaBytes[2];
            cdb[8] = lbaBytes[1];
            cdb[9] = lbaBytes[0];
            cdb[12] = (byte)((transferBytes & 0xFF00) >> 8);
            cdb[13] = (byte)(transferBytes & 0xFF);
            if(correct)
                cdb[14] += 0x01;

            buffer = new byte[transferBytes];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ LONG (16) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the MMC READ CD command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the MMC READ CD response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="blockSize">Block size.</param>
        /// <param name="expectedSectorType">Expected sector type.</param>
        /// <param name="DAP">If set to <c>true</c> CD-DA should be modified by mute and interpolation</param>
        /// <param name="relAddr">If set to <c>true</c> address is relative to current position.</param>
        /// <param name="sync">If set to <c>true</c> we request the sync bytes for data sectors.</param>
        /// <param name="headerCodes">Header codes.</param>
        /// <param name="userData">If set to <c>true</c> we request the user data.</param>
        /// <param name="edcEcc">If set to <c>true</c> we request the EDC/ECC fields for data sectors.</param>
        /// <param name="C2Error">C2 error options.</param>
        /// <param name="subchannel">Subchannel selection.</param>
        public bool ReadCd(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint transferLength, MmcSectorTypes expectedSectorType,
            bool DAP, bool relAddr, bool sync, MmcHeaderCodes headerCodes, bool userData, bool edcEcc, MmcErrorField C2Error, MmcSubchannel subchannel, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadCd;
            cdb[1] = (byte)((byte)expectedSectorType << 2);
            if(DAP)
                cdb[1] += 0x02;
            if(relAddr)
                cdb[1] += 0x01;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[6] = (byte)((transferLength & 0xFF0000) >> 16);
            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);
            cdb[9] = (byte)((byte)C2Error << 1);
            cdb[9] += (byte)((byte)headerCodes << 5);
            if(sync)
                cdb[9] += 0x80;
            if(userData)
                cdb[9] += 0x10;
            if(edcEcc)
                cdb[9] += 0x08;
            cdb[10] = (byte)subchannel;

            buffer = new byte[blockSize * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the MMC READ CD MSF command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the MMC READ CD MSF response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="startMsf">Start MM:SS:FF of read encoded as 0x00MMSSFF.</param>
        /// <param name="endMsf">End MM:SS:FF of read encoded as 0x00MMSSFF.</param>
        /// <param name="blockSize">Block size.</param>
        /// <param name="expectedSectorType">Expected sector type.</param>
        /// <param name="DAP">If set to <c>true</c> CD-DA should be modified by mute and interpolation</param>
        /// <param name="sync">If set to <c>true</c> we request the sync bytes for data sectors.</param>
        /// <param name="headerCodes">Header codes.</param>
        /// <param name="userData">If set to <c>true</c> we request the user data.</param>
        /// <param name="edcEcc">If set to <c>true</c> we request the EDC/ECC fields for data sectors.</param>
        /// <param name="C2Error">C2 error options.</param>
        /// <param name="subchannel">Subchannel selection.</param>
        public bool ReadCdMsf(out byte[] buffer, out byte[] senseBuffer, uint startMsf, uint endMsf, uint blockSize, MmcSectorTypes expectedSectorType,
            bool DAP, bool sync, MmcHeaderCodes headerCodes, bool userData, bool edcEcc, MmcErrorField C2Error, MmcSubchannel subchannel, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadCdMsf;
            cdb[1] = (byte)((byte)expectedSectorType << 2);
            if(DAP)
                cdb[1] += 0x02;
            cdb[3] = (byte)((startMsf & 0xFF0000) >> 16);
            cdb[4] = (byte)((startMsf & 0xFF00) >> 8);
            cdb[5] = (byte)(startMsf & 0xFF);
            cdb[6] = (byte)((endMsf & 0xFF0000) >> 16);
            cdb[7] = (byte)((endMsf & 0xFF00) >> 8);
            cdb[8] = (byte)(endMsf & 0xFF);
            cdb[9] = (byte)((byte)C2Error << 1);
            cdb[9] += (byte)((byte)headerCodes << 5);
            if(sync)
                cdb[9] += 0x80;
            if(userData)
                cdb[9] += 0x10;
            if(edcEcc)
                cdb[9] += 0x08;
            cdb[10] = (byte)subchannel;

            uint transferLength = (uint)((cdb[6] - cdb[3]) * 60 * 75 + (cdb[7] - cdb[4]) * 75 + (cdb[8] - cdb[5]));

            buffer = new byte[blockSize * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD MSF took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the Pioneer READ CD-DA command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the Pioneer READ CD-DA response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="blockSize">Block size.</param>
        /// <param name="subchannel">Subchannel selection.</param>
        public bool ReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint transferLength, PioneerSubchannel subchannel, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadCdDa;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[7] = (byte)((transferLength & 0xFF0000) >> 16);
            cdb[8] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[9] = (byte)(transferLength & 0xFF);
            cdb[10] = (byte)subchannel;

            buffer = new byte[blockSize * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD-DA took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the Pioneer READ CD-DA MSF command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the Pioneer READ CD-DA MSF response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="startMsf">Start MM:SS:FF of read encoded as 0x00MMSSFF.</param>
        /// <param name="endMsf">End MM:SS:FF of read encoded as 0x00MMSSFF.</param>
        /// <param name="blockSize">Block size.</param>
        /// <param name="subchannel">Subchannel selection.</param>
        public bool ReadCdDaMsf(out byte[] buffer, out byte[] senseBuffer, uint startMsf, uint endMsf, uint blockSize, PioneerSubchannel subchannel, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadCdMsf;
            cdb[3] = (byte)((startMsf & 0xFF0000) >> 16);
            cdb[4] = (byte)((startMsf & 0xFF00) >> 8);
            cdb[5] = (byte)(startMsf & 0xFF);
            cdb[7] = (byte)((endMsf & 0xFF0000) >> 16);
            cdb[8] = (byte)((endMsf & 0xFF00) >> 8);
            cdb[9] = (byte)(endMsf & 0xFF);
            cdb[10] = (byte)subchannel;

            uint transferLength = (uint)((cdb[6] - cdb[3]) * 60 * 75 + (cdb[7] - cdb[4]) * 75 + (cdb[8] - cdb[5]));

            buffer = new byte[blockSize * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD-DA MSF took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the Plextor READ CD-DA command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the Plextor READ CD-DA response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="blockSize">Block size.</param>
        /// <param name="subchannel">Subchannel selection.</param>
        public bool ReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint transferLength, PlextorSubchannel subchannel, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadCdDa;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[6] = (byte)((transferLength & 0xFF000000) >> 24);
            cdb[7] = (byte)((transferLength & 0xFF0000) >> 16);
            cdb[8] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[9] = (byte)(transferLength & 0xFF);
            cdb[10] = (byte)subchannel;

            buffer = new byte[blockSize * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD-DA took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Sends the NEC READ CD-DA command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the NEC READ CD-DA response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool ReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            bool sense;

            cdb[0] = (byte)ScsiCommands.NEC_ReadCdDa;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[2352 * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD-DA took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Reads a "raw" sector from DVD on Plextor drives. Does it reading drive's cache.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the Plextor READ DVD (RAW) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool PlextorReadRawDvd(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            buffer = new byte[2064 * transferLength];
            bool sense;

            cdb[0] = (byte)ScsiCommands.ReadBuffer;
            cdb[1] = 0x02;
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[3] = (byte)((buffer.Length & 0xFF0000) >> 16);
            cdb[4] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[5] = (byte)(buffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "Plextor READ DVD (RAW) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Reads a "raw" sector from DVD on HL-DT-ST drives.
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the HL-DT-ST READ DVD (RAW) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool HlDtStReadRawDvd(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            buffer = new byte[2064 * transferLength];
            bool sense;

            cdb[0] = (byte)ScsiCommands.HlDtSt_Vendor;
            cdb[1] = 0x48;
            cdb[2] = 0x49;
            cdb[3] = 0x54;
            cdb[4] = 0x01;
            cdb[6] = (byte)((lba & 0xFF000000) >> 24);
            cdb[7] = (byte)((lba & 0xFF0000) >> 16);
            cdb[8] = (byte)((lba & 0xFF00) >> 8);
            cdb[9] = (byte)(lba & 0xFF);
            cdb[10] = (byte)((buffer.Length & 0xFF00) >> 8);
            cdb[11] = (byte)(buffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "HL-DT-ST READ DVD (RAW) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Moves the device reading element to the specified block address
        /// </summary>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="lba">LBA.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool Seek6(out byte[] senseBuffer, uint lba, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[6];
            byte[] buffer = new byte[0];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Seek6;
            cdb[1] = (byte)((lba & 0x1F0000) >> 16);
            cdb[2] = (byte)((lba & 0xFF00) >> 8);
            cdb[3] = (byte)(lba & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "SEEK (6) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Moves the device reading element to the specified block address
        /// </summary>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="lba">LBA.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool Seek10(out byte[] senseBuffer, uint lba, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            byte[] buffer = new byte[0];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Seek10;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "SEEK (10) took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Reads the statistics EEPROM from Plextor CD recorders
        /// </summary>
        /// <returns><c>true</c>, if EEPROM is correctly read, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorReadEepromCDR(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[256];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_ReadEeprom;
            cdb[8] = 1;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR READ EEPROM took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Reads the statistics EEPROM from Plextor PX-708 and PX-712 recorders
        /// </summary>
        /// <returns><c>true</c>, if EEPROM is correctly read, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorReadEeprom(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[512];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_ReadEeprom;
            cdb[8] = 2;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR READ EEPROM took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Reads a block from the statistics EEPROM from Plextor DVD recorders
        /// </summary>
        /// <returns><c>true</c>, if EEPROM is correctly read, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="block">EEPROM block to read</param>
        /// <param name="blockSize">How many bytes are in the EEPROM block</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorReadEepromBlock(out byte[] buffer, out byte[] senseBuffer, byte block, ushort blockSize, uint timeout, out double duration)
        {
            buffer = new byte[blockSize];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_ReadEeprom;
            cdb[1] = 1;
            cdb[7] = block;
            cdb[8] = (byte)((blockSize & 0xFF00) >> 8);
            cdb[9] = (byte)(blockSize & 0xFF);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR READ EEPROM took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets speeds set by Plextor PoweRec
        /// </summary>
        /// <returns><c>true</c>, if speeds were got correctly, <c>false</c> otherwise.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="selected">Selected write speed.</param>
        /// <param name="max">Max speed for currently inserted media.</param>
        /// <param name="last">Last actual speed.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSpeeds(out byte[] senseBuffer, out ushort selected, out ushort max, out ushort last, uint timeout, out double duration)
        {
            byte[] buf = new byte[10];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            selected = 0;
            max = 0;
            last = 0;

            cdb[0] = (byte)ScsiCommands.Plextor_PoweRec;
            cdb[9] = (byte)buf.Length;

            lastError = SendScsiCommand(cdb, ref buf, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR POWEREC GET SPEEDS took {0} ms.", duration);

            if (!sense && !error)
            {
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                selected = BigEndianBitConverter.ToUInt16(buf, 4);
                max = BigEndianBitConverter.ToUInt16(buf, 6);
                last = BigEndianBitConverter.ToUInt16(buf, 8);
            }

            return sense;
        }

        /// <summary>
        /// Gets the Plextor PoweRec status
        /// </summary>
        /// <returns><c>true</c>, if PoweRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="enabled">PoweRec is enabled.</param>
        /// <param name="speed">PoweRec recommended speed.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetPoweRec(out byte[] senseBuffer, out bool enabled, out ushort speed, uint timeout, out double duration)
        {
            byte[] buf = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            enabled = false;
            speed = 0;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend2;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[9] = (byte)buf.Length;

            lastError = SendScsiCommand(cdb, ref buf, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR POWEREC GET SPEEDS took {0} ms.", duration);

            if (!sense && !error)
            {
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                enabled = buf[2] != 0;
                speed = BigEndianBitConverter.ToUInt16(buf, 4);
            }

            return sense;
        }

        /// <summary>
        /// Gets the Plextor SilentMode status
        /// </summary>
        /// <returns><c>true</c>, if SilentMode is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSilentMode(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.Silent;
            cdb[3] = 4;
            cdb[10] = (byte)buffer.Length;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SILENT MODE took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor GigaRec status
        /// </summary>
        /// <returns><c>true</c>, if GigaRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetGigaRec(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.GigaRec;
            cdb[10] = (byte)buffer.Length;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET GIGAREC took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor VariRec status
        /// </summary>
        /// <returns><c>true</c>, if VariRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetVariRec(out byte[] buffer, out byte[] senseBuffer, bool dvd, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.VariRec;
            cdb[10] = (byte)buffer.Length;

            if(dvd)
                cdb[3] = 0x12;
            else
                cdb[3] = 0x02;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET VARIREC took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor SecuRec status
        /// </summary>
        /// <returns><c>true</c>, if SecuRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSecuRec(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[2] = (byte)PlextorSubCommands.SecuRec;
            cdb[10] = (byte)buffer.Length;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SECUREC took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor SpeedRead status
        /// </summary>
        /// <returns><c>true</c>, if SpeedRead is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSpeedRead(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.SpeedRead;
            cdb[10] = (byte)buffer.Length;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SPEEDREAD took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor CD-R and multi-session hiding status
        /// </summary>
        /// <returns><c>true</c>, if CD-R and multi-session hiding is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetHiding(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.SessionHide;
            cdb[9] = (byte)buffer.Length;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SINGLE-SESSION / HIDE CD-R took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor DVD+ book bitsetting status
        /// </summary>
        /// <returns><c>true</c>, if DVD+ book bitsetting is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetBitsetting(out byte[] buffer, out byte[] senseBuffer, bool dualLayer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.BitSet;
            cdb[9] = (byte)buffer.Length;

            if(dualLayer)
                cdb[3] = (byte)PlextorSubCommands.BitSetRDL;
            else
                cdb[3] = (byte)PlextorSubCommands.BitSetR;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET BOOK BITSETTING took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        /// Gets the Plextor DVD+ test writing status
        /// </summary>
        /// <returns><c>true</c>, if DVD+ test writing is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetTestWriteDvdPlus(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[8];
            senseBuffer = new byte[32];
            byte[] cdb = new byte[12];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Plextor_Extend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.TestWriteDvdPlus;
            cdb[10] = (byte)buffer.Length;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET TEST WRITE DVD+ took {0} ms.", duration);

            return sense;
        }
    }
}

