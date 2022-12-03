// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Sends the MMC GET CONFIGURATION command for all Features</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI GET CONFIGURATION response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool GetConfiguration(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        GetConfiguration(out buffer, out senseBuffer, 0x0000, MmcGetConfigurationRt.All, timeout, out duration);

    /// <summary>Sends the MMC GET CONFIGURATION command for all Features starting with specified one</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI GET CONFIGURATION response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="startingFeatureNumber">Feature number where the feature list should start from</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool GetConfiguration(out byte[] buffer, out byte[] senseBuffer, ushort startingFeatureNumber, uint timeout,
                                 out double duration) =>
        GetConfiguration(out buffer, out senseBuffer, startingFeatureNumber, MmcGetConfigurationRt.All, timeout,
                         out duration);

    /// <summary>Sends the MMC GET CONFIGURATION command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI GET CONFIGURATION response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="startingFeatureNumber">Starting Feature number.</param>
    /// <param name="rt">Return type, <see cref="MmcGetConfigurationRt" />.</param>
    public bool GetConfiguration(out byte[] buffer, out byte[] senseBuffer, ushort startingFeatureNumber,
                                 MmcGetConfigurationRt rt, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];
        buffer = new byte[8];

        cdb[0] = (byte)ScsiCommands.GetConfiguration;
        cdb[1] = (byte)((byte)rt & 0x03);
        cdb[2] = (byte)((startingFeatureNumber & 0xFF00) >> 8);
        cdb[3] = (byte)(startingFeatureNumber & 0xFF);
        cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8] = (byte)(buffer.Length & 0xFF);
        cdb[9] = 0;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        ushort confLength = (ushort)((buffer[2] << 8) + buffer[3] + 4);
        buffer      = new byte[confLength];
        cdb[7]      = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8]      = (byte)(buffer.Length & 0xFF);
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       GET_CONFIGURATION_Starting_Feature_Number_1_Return_Type_2_Sense_3_Last_Error_4_took_0_ms,
                                   duration, startingFeatureNumber, rt, sense, LastError);

        return sense;
    }

    /// <summary>Sends the MMC READ DISC STRUCTURE command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ DISC STRUCTURE response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="mediaType">Medium type for requested disc structure</param>
    /// <param name="address">Medium address for requested disc structure</param>
    /// <param name="layerNumber">Medium layer for requested disc structure</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="format">Which disc structure are we requesting</param>
    /// <param name="agid">AGID used in medium copy protection</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadDiscStructure(out byte[] buffer, out byte[] senseBuffer, MmcDiscStructureMediaType mediaType,
                                  uint address, byte layerNumber, MmcDiscStructureFormat format, byte agid,
                                  uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];
        buffer = new byte[8];

        cdb[0]  = (byte)ScsiCommands.ReadDiscStructure;
        cdb[1]  = (byte)((byte)mediaType & 0x0F);
        cdb[2]  = (byte)((address & 0xFF000000) >> 24);
        cdb[3]  = (byte)((address & 0xFF0000)   >> 16);
        cdb[4]  = (byte)((address & 0xFF00)     >> 8);
        cdb[5]  = (byte)(address & 0xFF);
        cdb[6]  = layerNumber;
        cdb[7]  = (byte)format;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((agid & 0x03) << 6);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        if(sense)
            return true;

        ushort strctLength = (ushort)((buffer[0] << 8) + buffer[1] + 2);

        // WORKAROUND: Some drives return incorrect length information. As these structures are fixed length just apply known length.
        if(mediaType == MmcDiscStructureMediaType.Bd)
            buffer = format switch
            {
                MmcDiscStructureFormat.DiscInformation        => new byte[4100],
                MmcDiscStructureFormat.BdBurstCuttingArea     => new byte[68],
                MmcDiscStructureFormat.BdDds                  => new byte[strctLength < 100 ? 100 : strctLength],
                MmcDiscStructureFormat.CartridgeStatus        => new byte[8],
                MmcDiscStructureFormat.BdSpareAreaInformation => new byte[16],
                _                                             => new byte[strctLength]
            };
        else
            buffer = new byte[strctLength];

        cdb[8]      = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]      = (byte)(buffer.Length & 0xFF);
        senseBuffer = new byte[64];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       READ_DISC_STRUCTURE_Media_Type_1_Address_2_Layer_Number_3_Format_4_AGID_5_Sense_6_Last_Error_7_took_0_ms,
                                   duration, mediaType, address, layerNumber, format, agid, sense, LastError);

        return sense;
    }

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get formatted TOC from disc, in MM:SS:FF format</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="track">Start TOC from this track</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadToc(out byte[] buffer, out byte[] senseBuffer, byte track, uint timeout, out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, true, 0, track, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get formatted TOC from disc</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="msf">If <c>true</c>, request data in MM:SS:FF units, otherwise, in blocks</param>
    /// <param name="track">Start TOC from this track</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadToc(out byte[] buffer, out byte[] senseBuffer, bool msf, byte track, uint timeout,
                        out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, msf, 0, track, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get multi-session information, in MM:SS:FF format</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadSessionInfo(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, true, 1, 0, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get multi-session information</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="msf">If <c>true</c>, request data in MM:SS:FF units, otherwise, in blocks</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadSessionInfo(out byte[] buffer, out byte[] senseBuffer, bool msf, uint timeout,
                                out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, msf, 1, 0, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get raw TOC subchannels</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sessionNumber">Session which TOC to get</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadRawToc(out byte[] buffer, out byte[] senseBuffer, byte sessionNumber, uint timeout,
                           out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, true, 2, sessionNumber, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get PMA</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadPma(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, true, 3, 0, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get ATIP</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadAtip(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, true, 4, 0, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command to get Lead-In CD-TEXT</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadCdText(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadTocPmaAtip(out buffer, out senseBuffer, true, 5, 0, timeout, out duration);

    /// <summary>Sends the MMC READ TOC/PMA/ATIP command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ TOC/PMA/ATIP response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="msf">If <c>true</c>, request data in MM:SS:FF units, otherwise, in blocks</param>
    /// <param name="format">What structure is requested</param>
    /// <param name="trackSessionNumber">Track/Session number</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadTocPmaAtip(out byte[] buffer, out byte[] senseBuffer, bool msf, byte format,
                               byte trackSessionNumber, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];

        byte[] tmpBuffer = (format & 0x0F) == 5 ? new byte[32768] : new byte[1536];

        cdb[0] = (byte)ScsiCommands.ReadTocPmaAtip;

        if(msf)
            cdb[1] = 0x02;

        cdb[2] = (byte)(format & 0x0F);
        cdb[6] = trackSessionNumber;
        cdb[7] = (byte)((tmpBuffer.Length & 0xFF00) >> 8);
        cdb[8] = (byte)(tmpBuffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref tmpBuffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        uint strctLength = (uint)((tmpBuffer[0] << 8) + tmpBuffer[1] + 2);
        buffer = new byte[strctLength];

        if(buffer.Length <= tmpBuffer.Length)
        {
            Array.Copy(tmpBuffer, 0, buffer, 0, buffer.Length);

            AaruConsole.DebugWriteLine("SCSI Device",
                                       Localization.
                                           READ_TOC_PMA_ATIP_took_MSF_1_Format_2_Track_Session_Number_3_Sense_4_LastError_5_0_ms,
                                       duration, msf, format, trackSessionNumber, sense, LastError);

            return sense;
        }

        double tmpDuration = duration;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out sense);

        Error = LastError != 0;

        duration += tmpDuration;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       READ_TOC_PMA_ATIP_took_MSF_1_Format_2_Track_Session_Number_3_Sense_4_LastError_5_0_ms,
                                   duration, msf, format, trackSessionNumber, sense, LastError);

        return sense;
    }

    /// <summary>Sends the MMC READ DISC INFORMATION command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ DISC INFORMATION response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadDiscInformation(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        ReadDiscInformation(out buffer, out senseBuffer, MmcDiscInformationDataTypes.DiscInformation, timeout,
                            out duration);

    /// <summary>Sends the MMC READ DISC INFORMATION command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ DISC INFORMATION response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="dataType">Which disc information to read</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadDiscInformation(out byte[] buffer, out byte[] senseBuffer, MmcDiscInformationDataTypes dataType,
                                    uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb       = new byte[10];
        byte[] tmpBuffer = new byte[804];

        cdb[0] = (byte)ScsiCommands.ReadDiscInformation;
        cdb[1] = (byte)dataType;
        cdb[7] = (byte)((tmpBuffer.Length & 0xFF00) >> 8);
        cdb[8] = (byte)(tmpBuffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref tmpBuffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        uint strctLength = (uint)((tmpBuffer[0] << 8) + tmpBuffer[1] + 2);

        if(strctLength > tmpBuffer.Length)
            strctLength = (uint)tmpBuffer.Length;

        buffer = new byte[strctLength];
        Array.Copy(tmpBuffer, 0, buffer, 0, buffer.Length);

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.READ_DISC_INFORMATION_Data_Type_1_Sense_2_Last_Error_3_took_0_ms,
                                   duration, dataType, sense, LastError);

        return sense;
    }

    /// <summary>Sends the MMC READ CD command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the MMC READ CD response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="blockSize">Block size.</param>
    /// <param name="expectedSectorType">Expected sector type.</param>
    /// <param name="dap">If set to <c>true</c> CD-DA should be modified by mute and interpolation</param>
    /// <param name="relAddr">If set to <c>true</c> address is relative to current position.</param>
    /// <param name="sync">If set to <c>true</c> we request the sync bytes for data sectors.</param>
    /// <param name="headerCodes">Header codes.</param>
    /// <param name="userData">If set to <c>true</c> we request the user data.</param>
    /// <param name="edcEcc">If set to <c>true</c> we request the EDC/ECC fields for data sectors.</param>
    /// <param name="c2Error">C2 error options.</param>
    /// <param name="subchannel">Subchannel selection.</param>
    public bool ReadCd(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint transferLength,
                       MmcSectorTypes expectedSectorType, bool dap, bool relAddr, bool sync, MmcHeaderCodes headerCodes,
                       bool userData, bool edcEcc, MmcErrorField c2Error, MmcSubchannel subchannel, uint timeout,
                       out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];

        cdb[0] = (byte)ScsiCommands.ReadCd;
        cdb[1] = (byte)((byte)expectedSectorType << 2);

        if(dap)
            cdb[1] += 0x02;

        if(relAddr)
            cdb[1] += 0x01;

        cdb[2] =  (byte)((lba & 0xFF000000) >> 24);
        cdb[3] =  (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] =  (byte)((lba & 0xFF00)     >> 8);
        cdb[5] =  (byte)(lba & 0xFF);
        cdb[6] =  (byte)((transferLength & 0xFF0000) >> 16);
        cdb[7] =  (byte)((transferLength & 0xFF00)   >> 8);
        cdb[8] =  (byte)(transferLength & 0xFF);
        cdb[9] =  (byte)((byte)c2Error     << 1);
        cdb[9] += (byte)((byte)headerCodes << 5);

        if(sync)
            cdb[9] += 0x80;

        if(userData)
            cdb[9] += 0x10;

        if(edcEcc)
            cdb[9] += 0x08;

        cdb[10] = (byte)subchannel;

        buffer = new byte[blockSize * transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       READ_CD_LBA_1_Block_Size_2_Transfer_Length_3_Expected_Sector_Type_4_DAP_5_Relative_Address_6_Sync_7_Headers_8_User_Data_9_ECC_EDC_10_C2_11_Subchannel_12_Sense_13_Last_Error_14_took_0_ms,
                                   duration, lba, blockSize, transferLength, expectedSectorType, dap, relAddr, sync,
                                   headerCodes, userData, edcEcc, c2Error, subchannel, sense, LastError);

        return sense;
    }

    /// <summary>Sends the MMC READ CD MSF command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the MMC READ CD MSF response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="startMsf">Start MM:SS:FF of read encoded as 0x00MMSSFF.</param>
    /// <param name="endMsf">End MM:SS:FF of read encoded as 0x00MMSSFF.</param>
    /// <param name="blockSize">Block size.</param>
    /// <param name="expectedSectorType">Expected sector type.</param>
    /// <param name="dap">If set to <c>true</c> CD-DA should be modified by mute and interpolation</param>
    /// <param name="sync">If set to <c>true</c> we request the sync bytes for data sectors.</param>
    /// <param name="headerCodes">Header codes.</param>
    /// <param name="userData">If set to <c>true</c> we request the user data.</param>
    /// <param name="edcEcc">If set to <c>true</c> we request the EDC/ECC fields for data sectors.</param>
    /// <param name="c2Error">C2 error options.</param>
    /// <param name="subchannel">Subchannel selection.</param>
    public bool ReadCdMsf(out byte[] buffer, out byte[] senseBuffer, uint startMsf, uint endMsf, uint blockSize,
                          MmcSectorTypes expectedSectorType, bool dap, bool sync, MmcHeaderCodes headerCodes,
                          bool userData, bool edcEcc, MmcErrorField c2Error, MmcSubchannel subchannel, uint timeout,
                          out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];

        cdb[0] = (byte)ScsiCommands.ReadCdMsf;
        cdb[1] = (byte)((byte)expectedSectorType << 2);

        if(dap)
            cdb[1] += 0x02;

        cdb[3] =  (byte)((startMsf & 0xFF0000) >> 16);
        cdb[4] =  (byte)((startMsf & 0xFF00)   >> 8);
        cdb[5] =  (byte)(startMsf & 0xFF);
        cdb[6] =  (byte)((endMsf & 0xFF0000) >> 16);
        cdb[7] =  (byte)((endMsf & 0xFF00)   >> 8);
        cdb[8] =  (byte)(endMsf & 0xFF);
        cdb[9] =  (byte)((byte)c2Error     << 1);
        cdb[9] += (byte)((byte)headerCodes << 5);

        if(sync)
            cdb[9] += 0x80;

        if(userData)
            cdb[9] += 0x10;

        if(edcEcc)
            cdb[9] += 0x08;

        cdb[10] = (byte)subchannel;

        uint transferLength = (uint)(((cdb[6] - cdb[3]) * 60 * 75) + ((cdb[7] - cdb[4]) * 75) + (cdb[8] - cdb[5]));

        buffer = new byte[blockSize * transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       READ_CD_MSF_Start_MSF_1_End_MSF_2_Block_Size_3_Expected_Sector_Type_4_DAP_5_Sync_6_Headers_7_User_Data_8_ECC_EDC_9_C2_10_Subchannel_11_Sense_12_LastError_13_took_0_ms,
                                   duration, startMsf, endMsf, blockSize, expectedSectorType, dap, sync, headerCodes,
                                   userData, edcEcc, c2Error, subchannel, sense, LastError);

        return sense;
    }

    /// <summary>Prevents ejection of the media inserted in the drive</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool PreventMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration) =>
        PreventAllowMediumRemoval(out senseBuffer, false, true, timeout, out duration);

    /// <summary>Allows ejection of the media inserted in the drive</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool AllowMediumRemoval(out byte[] senseBuffer, uint timeout, out double duration) =>
        PreventAllowMediumRemoval(out senseBuffer, false, false, timeout, out duration);

    /// <summary>Prevents or allows ejection of the media inserted in the drive</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="persistent">Persistent.</param>
    /// <param name="prevent">Prevent.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool PreventAllowMediumRemoval(out byte[] senseBuffer, bool persistent, bool prevent, uint timeout,
                                          out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.PreventAllowMediumRemoval;

        if(prevent)
            cdb[4] += 0x01;

        if(persistent)
            cdb[4] += 0x02;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       PREVENT_ALLOW_MEDIUM_REMOVAL_Persistent_1_Prevent_2_Sense_3_LastError_4_took_0_ms,
                                   duration, persistent, prevent, sense, LastError);

        return sense;
    }

    /// <summary>Loads the media tray</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool LoadTray(out byte[] senseBuffer, uint timeout, out double duration) =>
        StartStopUnit(out senseBuffer, false, 0, 0, false, true, true, timeout, out duration);

    /// <summary>Ejects the media or its tray</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool EjectTray(out byte[] senseBuffer, uint timeout, out double duration) =>
        StartStopUnit(out senseBuffer, false, 0, 0, false, true, false, timeout, out duration);

    /// <summary>Starts the drive's reading mechanism</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool StartUnit(out byte[] senseBuffer, uint timeout, out double duration) =>
        StartStopUnit(out senseBuffer, false, 0, 0, false, false, true, timeout, out duration);

    /// <summary>Stops the drive's reading mechanism</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool StopUnit(out byte[] senseBuffer, uint timeout, out double duration) =>
        StartStopUnit(out senseBuffer, false, 0, 0, false, false, false, timeout, out duration);

    /// <summary>Starts or stops the drive's reading mechanism or tray</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">Return from execution immediately</param>
    /// <param name="formatLayer">Choose density layer for hybrid discs</param>
    /// <param name="powerConditions">Power condition</param>
    /// <param name="changeFormatLayer">Change format layer</param>
    /// <param name="loadEject">Loads or ejects the media</param>
    /// <param name="start">Starts the mechanism</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool StartStopUnit(out byte[] senseBuffer, bool immediate, byte formatLayer, byte powerConditions,
                              bool changeFormatLayer, bool loadEject, bool start, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.StartStopUnit;

        if(immediate)
            cdb[1] += 0x01;

        if(changeFormatLayer)
        {
            cdb[3] =  (byte)(formatLayer & 0x03);
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

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       START_STOP_UNIT_Immediate_1_FormatLayer_2_Power_Conditions_3_Change_Format_Layer_4_Load_Eject_5_Start_6_Sense_7_Last_Error_8_took_0_ms,
                                   duration, immediate, formatLayer, powerConditions, changeFormatLayer, loadEject,
                                   start, sense, LastError);

        return sense;
    }

    /// <summary>Reads the MCN from a disc</summary>
    /// <param name="mcn">Decoded MCN.</param>
    /// <param name="buffer">Buffer containing raw drive response.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    [SuppressMessage("ReSharper", "ShiftExpressionZeroLeftOperand")]
    public bool ReadMcn(out string mcn, out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];
        mcn = null;

        cdb[0] = (byte)ScsiCommands.ReadSubChannel;
        cdb[1] = 0;
        cdb[2] = 0x40;
        cdb[3] = 0x02;
        cdb[7] = (23 & 0xFF00) >> 8;
        cdb[8] = 23 & 0xFF;

        buffer = new byte[23];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.READ_READ_SUB_CHANNEL_MCN_Sense_1_Last_Error_2_took_0_ms,
                                   duration, sense, LastError);

        if(!sense &&
           (buffer[8] & 0x80) == 0x80)
            mcn = Encoding.ASCII.GetString(buffer, 9, 13);

        return sense;
    }

    /// <summary>Reads the ISRC from a track</summary>
    /// <param name="trackNumber">Track number.</param>
    /// <param name="isrc">Decoded ISRC.</param>
    /// <param name="buffer">Buffer containing raw drive response.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    [SuppressMessage("ReSharper", "ShiftExpressionZeroLeftOperand")]
    public bool ReadIsrc(byte trackNumber, out string isrc, out byte[] buffer, out byte[] senseBuffer, uint timeout,
                         out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];
        isrc = null;

        cdb[0] = (byte)ScsiCommands.ReadSubChannel;
        cdb[1] = 0;
        cdb[2] = 0x40;
        cdb[3] = 0x03;
        cdb[6] = trackNumber;
        cdb[7] = (23 & 0xFF00) >> 8;
        cdb[8] = 23 & 0xFF;

        buffer = new byte[23];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       READ_READ_SUB_CHANNEL_ISRC_Track_Number_1_Sense_2_Last_Error_3_took_0_ms,
                                   duration, trackNumber, sense, LastError);

        if(!sense &&
           (buffer[8] & 0x80) == 0x80)
            isrc = Encoding.ASCII.GetString(buffer, 9, 12);

        return sense;
    }

    /// <summary>Sets the reading speed</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="rotationalControl">Rotational control.</param>
    /// <param name="readSpeed">Read speed.</param>
    /// <param name="writeSpeed">Write speed.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool SetCdSpeed(out byte[] senseBuffer, RotationalControl rotationalControl, ushort readSpeed,
                           ushort writeSpeed, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[12];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.SetCdRomSpeed;
        cdb[1] = (byte)((byte)rotationalControl & 0x03);
        cdb[2] = (byte)((readSpeed & 0xFF00) >> 8);
        cdb[3] = (byte)(readSpeed & 0xFF);
        cdb[4] = (byte)((writeSpeed & 0xFF00) >> 8);
        cdb[5] = (byte)(writeSpeed & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.
                                       SET_CD_SPEED_Rotational_Control_1_Read_Speed_2_Write_Speed_3_Sense_4_Last_Error_5_took_0_ms,
                                   duration, rotationalControl, readSpeed, writeSpeed, sense, LastError);

        return sense;
    }

    /// <summary>Sends the MMC READ TRACK INFORMATION command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ DISC INFORMATION response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="address">Track/session/sector address</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="open">Report information of non-closed tracks</param>
    /// <param name="type">Type of information to retrieve</param>
    public bool ReadTrackInformation(out byte[] buffer, out byte[] senseBuffer, bool open, TrackInformationType type,
                                     uint address, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[10];
        buffer = new byte[48];

        cdb[0] = (byte)ScsiCommands.ReadTrackInformation;
        cdb[1] = (byte)((byte)type & 0x3);
        cdb[2] = (byte)((address & 0xFF000000) >> 24);
        cdb[3] = (byte)((address & 0xFF0000)   >> 16);
        cdb[4] = (byte)((address & 0xFF00)     >> 8);
        cdb[5] = (byte)(address & 0xFF);
        cdb[7] = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[8] = (byte)(buffer.Length & 0xFF);

        if(open)
            cdb[1] += 4;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device",
                                   Localization.READ_TRACK_INFORMATION_Data_Type_1_Sense_2_Last_Error_3_took_0_ms,
                                   duration, type, sense, LastError);

        return sense;
    }
}