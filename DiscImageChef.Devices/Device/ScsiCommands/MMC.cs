// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI MultiMedia Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SCSI commands defined in MMC standards.
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

using System;
using DiscImageChef.Console;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Sends the MMC GET CONFIGURATION command for all Features
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
        /// Sends the MMC GET CONFIGURATION command for all Features starting with specified one
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
        /// Sends the MMC GET CONFIGURATION command
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

            if(sense)
                return true;

#pragma warning disable IDE0004 // Cast is necessary or an invalid bitshift happens
            ushort confLength = (ushort)(((int)buffer[2] << 8) + buffer[3] + 4);
#pragma warning restore IDE0004 // Cast is necessary or an invalid bitshift happens
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
        /// Sends the MMC READ DISC STRUCTURE command
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

            if(sense)
                return true;

#pragma warning disable IDE0004 // Cast is necessary or an invalid bitshift happens
            ushort strctLength = (ushort)(((int)buffer[0] << 8) + buffer[1] + 2);
#pragma warning restore IDE0004 // Cast is necessary or an invalid bitshift happens
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get formatted TOC from disc, in MM:SS:FF format
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get formatted TOC from disc
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get multi-session information, in MM:SS:FF format
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get multi-session information
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get raw TOC subchannels
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get PMA
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get ATIP
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
        /// Sends the MMC READ TOC/PMA/ATIP command to get Lead-In CD-TEXT
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
        /// Sends the MMC READ TOC/PMA/ATIP command
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
            if(MSF)
                cdb[1] = 0x02;
            cdb[2] = (byte)(format & 0x0F);
            cdb[6] = trackSessionNumber;
            cdb[7] = (byte)((tmpBuffer.Length & 0xFF00) >> 8);
            cdb[8] = (byte)(tmpBuffer.Length & 0xFF);

            lastError = SendScsiCommand(cdb, ref tmpBuffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

#pragma warning disable IDE0004 // Cast is necessary or an invalid bitshift happens
            uint strctLength = (uint)(((int)tmpBuffer[0] << 8) + tmpBuffer[1] + 2);
#pragma warning restore IDE0004 // Cast is necessary or an invalid bitshift happens
            buffer = new byte[strctLength];

            if(buffer.Length <= tmpBuffer.Length)
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
        /// Sends the MMC READ DISC INFORMATION command
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
        /// Sends the MMC READ DISC INFORMATION command
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

#pragma warning disable IDE0004 // Cast is necessary or an invalid bitshift happens
            uint strctLength = (uint)(((int)tmpBuffer[0] << 8) + tmpBuffer[1] + 2);
#pragma warning restore IDE0004 // Cast is necessary or an invalid bitshift happens
            buffer = new byte[strctLength];
            Array.Copy(tmpBuffer, 0, buffer, 0, buffer.Length);

            DicConsole.DebugWriteLine("SCSI Device", "READ DISC INFORMATION took {0} ms.", duration);

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

        public bool PreventMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return PreventAllowMediumRemoval(out senseBuffer, false, true, timeout, out duration);
        }

        public bool AllowMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return PreventAllowMediumRemoval(out senseBuffer, false, false, timeout, out duration);
        }

        public bool PreventAllowMediumRemoval(out byte[] senseBuffer, bool persistent, bool prevent, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[6];
            byte[] buffer = new byte[0];
            bool sense;

            cdb[0] = (byte)ScsiCommands.PreventAllowMediumRemoval;
            if(prevent)
                cdb[4] += 0x01;
            if(persistent)
                cdb[4] += 0x02;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "PREVENT ALLOW MEDIUM REMOVAL took {0} ms.", duration);

            return sense;
        }

        public bool LoadTray(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return StartStopUnit(out senseBuffer, false, 0, 0, false, true, true, timeout, out duration);
        }

        public bool EjectTray(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return StartStopUnit(out senseBuffer, false, 0, 0, false, true, false, timeout, out duration);
        }

        public bool StartUnit(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return StartStopUnit(out senseBuffer, false, 0, 0, false, false, true, timeout, out duration);
        }

        public bool StopUnit(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return StartStopUnit(out senseBuffer, false, 0, 0, false, false, false, timeout, out duration);
        }

        public bool StartStopUnit(out byte[] senseBuffer, bool immediate, byte formatLayer, byte powerConditions, bool changeFormatLayer, bool loadEject, bool start, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[6];
            byte[] buffer = new byte[0];
            bool sense;

            cdb[0] = (byte)ScsiCommands.StartStopUnit;
            if(immediate)
                cdb[1] += 0x01;
            if(changeFormatLayer)
            {
                cdb[3] = (byte)(formatLayer & 0x03);
                cdb[4] += 0x04;
            }
            else
            {
                if(loadEject)
                    cdb[4] += 0x02;
                if(start)
                    cdb[4] += 0x01;
            }
            cdb[4] += (byte)((powerConditions & 0x0F) << 4);

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "START STOP UNIT took {0} ms.", duration);

            return sense;
        }

    }
}

