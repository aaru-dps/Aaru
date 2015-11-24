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
            byte[] cdb = new byte[12];
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
            byte[] tmpBuffer = new byte[804];
            bool sense;

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
            Array.Copy(tmpBuffer, 0, buffer, 0, buffer.Length);

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
    }
}

