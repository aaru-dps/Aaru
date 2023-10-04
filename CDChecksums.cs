// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDChecksums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements CD checksums.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.If not, see<http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ECC algorithm from ECM(c) 2002-2011 Neill Corlett
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Checksums;

/// <summary>Implements ReedSolomon and CRC32 algorithms as used by CD-ROM</summary>
public static class CdChecksums
{
    const  string MODULE_NAME = "CD checksums";
    static byte[] _eccFTable;
    static byte[] _eccBTable;
    static uint[] _edcTable;

    /// <summary>Checks the EDC and ECC of a CD sector</summary>
    /// <param name="buffer">CD sector</param>
    /// <returns>
    ///     <c>true</c> if all checks were correct, <c>false</c> if any of them weren't, and <c>null</c> if none of them
    ///     are present.
    /// </returns>
    public static bool? CheckCdSector(byte[] buffer) => CheckCdSector(buffer, out _, out _, out _);

    /// <summary>Checks the EDC and ECC of a CD sector</summary>
    /// <param name="buffer">CD sector</param>
    /// <param name="correctEccP">
    ///     <c>true</c> if ECC P is correct, <c>false</c> if it isn't, and <c>null</c> if there is no ECC
    ///     P in sector.
    /// </param>
    /// <param name="correctEccQ">
    ///     <c>true</c> if ECC Q is correct, <c>false</c> if it isn't, and <c>null</c> if there is no ECC
    ///     Q in sector.
    /// </param>
    /// <param name="correctEdc">
    ///     <c>true</c> if EDC is correct, <c>false</c> if it isn't, and <c>null</c> if there is no EDC in
    ///     sector.
    /// </param>
    /// <returns>
    ///     <c>true</c> if all checks were correct, <c>false</c> if any of them weren't, and <c>null</c> if none of them
    ///     are present.
    /// </returns>
    public static bool? CheckCdSector(byte[] buffer, out bool? correctEccP, out bool? correctEccQ, out bool? correctEdc)
    {
        correctEccP = null;
        correctEccQ = null;
        correctEdc  = null;

        switch(buffer.Length)
        {
            case 2448:
            {
                var subchannel = new byte[96];
                var channel    = new byte[2352];

                Array.Copy(buffer, 0,    channel,    0, 2352);
                Array.Copy(buffer, 2352, subchannel, 0, 96);

                bool? channelStatus = CheckCdSectorChannel(channel, out correctEccP, out correctEccQ, out correctEdc);

                bool? subchannelStatus = CheckCdSectorSubChannel(subchannel);
                bool? status           = null;

                if(channelStatus    == false ||
                   subchannelStatus == false)
                    status = false;

                status = channelStatus switch
                         {
                             null when subchannelStatus == true => true,
                             true when subchannelStatus == null => true,
                             _                                  => status
                         };

                return status;
            }

            case 2352:
                return CheckCdSectorChannel(buffer, out correctEccP, out correctEccQ, out correctEdc);
            default:
                return null;
        }
    }

    static void EccInit()
    {
        _eccFTable = new byte[256];
        _eccBTable = new byte[256];
        _edcTable  = new uint[256];

        for(uint i = 0; i < 256; i++)
        {
            uint edc = i;
            var  j   = (uint)(i << 1 ^ ((i & 0x80) == 0x80 ? 0x11D : 0));
            _eccFTable[i]     = (byte)j;
            _eccBTable[i ^ j] = (byte)i;

            for(j = 0; j < 8; j++)
                edc = edc >> 1 ^ ((edc & 1) > 0 ? 0xD8018001 : 0);

            _edcTable[i] = edc;
        }
    }

    static bool CheckEcc(byte[] address, byte[] data, uint majorCount, uint minorCount, uint majorMult, uint minorInc,
                         byte[] ecc)
    {
        uint size = majorCount * minorCount;
        uint major;

        for(major = 0; major < majorCount; major++)
        {
            uint index = (major >> 1) * majorMult + (major & 1);
            byte eccA  = 0;
            byte eccB  = 0;
            uint minor;

            for(minor = 0; minor < minorCount; minor++)
            {
                byte temp = index < 4 ? address[index] : data[index - 4];
                index += minorInc;

                if(index >= size)
                    index -= size;

                eccA ^= temp;
                eccB ^= temp;
                eccA =  _eccFTable[eccA];
            }

            eccA = _eccBTable[_eccFTable[eccA] ^ eccB];

            if(ecc[major]              != eccA ||
               ecc[major + majorCount] != (eccA ^ eccB))
                return false;
        }

        return true;
    }

    static bool? CheckCdSectorChannel(byte[]    channel, out bool? correctEccP, out bool? correctEccQ,
                                      out bool? correctEdc)
    {
        EccInit();

        correctEccP = null;
        correctEccQ = null;
        correctEdc  = null;

        if(channel[0x000] != 0x00 ||
           channel[0x001] != 0xFF ||
           channel[0x002] != 0xFF ||
           channel[0x003] != 0xFF ||
           channel[0x004] != 0xFF ||
           channel[0x005] != 0xFF ||
           channel[0x006] != 0xFF ||
           channel[0x007] != 0xFF ||
           channel[0x008] != 0xFF ||
           channel[0x009] != 0xFF ||
           channel[0x00A] != 0xFF ||
           channel[0x00B] != 0x00)
            return null;

        //AaruConsole.DebugWriteLine(MODULE_NAME, "Data sector, address {0:X2}:{1:X2}:{2:X2}", channel[0x00C],
        //                          channel[0x00D], channel[0x00E]);

        switch(channel[0x00F] & 0x03)
        {
            // mode (1 byte)
            case 0x00:
            {
                //AaruConsole.DebugWriteLine(MODULE_NAME, "Mode 0 sector at address {0:X2}:{1:X2}:{2:X2}",
                //                          channel[0x00C], channel[0x00D], channel[0x00E]);
                for(var i = 0x010; i < 0x930; i++)
                {
                    if(channel[i] == 0x00)
                        continue;

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "Mode 0 sector with error at address: {0:X2}:{1:X2}:{2:X2}",
                                               channel[0x00C], channel[0x00D], channel[0x00E]);

                    return false;
                }

                return true;
            }

            // mode (1 byte)
            //AaruConsole.DebugWriteLine(MODULE_NAME, "Mode 1 sector at address {0:X2}:{1:X2}:{2:X2}",
            //                          channel[0x00C], channel[0x00D], channel[0x00E]);
            case 0x01 when channel[0x814] != 0x00 || // reserved (8 bytes)
                           channel[0x815] != 0x00 || channel[0x816] != 0x00 || channel[0x817] != 0x00 ||
                           channel[0x818] != 0x00 || channel[0x819] != 0x00 || channel[0x81A] != 0x00 ||
                           channel[0x81B] != 0x00:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "Mode 1 sector with data in reserved bytes at address: {0:X2}:{1:X2}:{2:X2}",
                                           channel[0x00C], channel[0x00D], channel[0x00E]);

                return false;
            case 0x01:
            {
                var address = new byte[4];
                var data    = new byte[2060];
                var data2   = new byte[2232];
                var eccP    = new byte[172];
                var eccQ    = new byte[104];

                Array.Copy(channel, 0x0C,  address, 0, 4);
                Array.Copy(channel, 0x10,  data,    0, 2060);
                Array.Copy(channel, 0x10,  data2,   0, 2232);
                Array.Copy(channel, 0x81C, eccP,    0, 172);
                Array.Copy(channel, 0x8C8, eccQ,    0, 104);

                bool failedEccP = !CheckEcc(address, data,  86, 24, 2,  86, eccP);
                bool failedEccQ = !CheckEcc(address, data2, 52, 43, 86, 88, eccQ);

                correctEccP = !failedEccP;
                correctEccQ = !failedEccQ;

                if(failedEccP)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC P check",
                                               channel[0x00C], channel[0x00D], channel[0x00E]);
                }

                if(failedEccQ)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC Q check",
                                               channel[0x00C], channel[0x00D], channel[0x00E]);
                }

                var  storedEdc     = BitConverter.ToUInt32(channel, 0x810);
                uint calculatedEdc = ComputeEdc(0, channel, 0x810);

                correctEdc = calculatedEdc == storedEdc;

                if(calculatedEdc == storedEdc)
                    return !failedEccP && !failedEccQ;

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}",
                                           channel[0x00C], channel[0x00D], channel[0x00E], calculatedEdc, storedEdc);

                return false;
            }

            // mode (1 byte)
            case 0x02:
            {
                //AaruConsole.DebugWriteLine(MODULE_NAME, "Mode 2 sector at address {0:X2}:{1:X2}:{2:X2}",
                //                          channel[0x00C], channel[0x00D], channel[0x00E]);
                var mode2Sector = new byte[channel.Length - 0x10];
                Array.Copy(channel, 0x10, mode2Sector, 0, mode2Sector.Length);

                if((channel[0x012] & 0x20) == 0x20) // mode 2 form 2
                {
                    if(channel[0x010] != channel[0x014] ||
                       channel[0x011] != channel[0x015] ||
                       channel[0x012] != channel[0x016] ||
                       channel[0x013] != channel[0x017])
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   "Subheader copies differ in mode 2 form 2 sector at address: {0:X2}:{1:X2}:{2:X2}",
                                                   channel[0x00C], channel[0x00D], channel[0x00E]);
                    }

                    var storedEdc = BitConverter.ToUInt32(mode2Sector, 0x91C);

                    // No CRC stored!
                    if(storedEdc == 0x00000000)
                        return true;

                    uint calculatedEdc = ComputeEdc(0, mode2Sector, 0x91C);

                    correctEdc = calculatedEdc == storedEdc;

                    if(calculatedEdc == storedEdc)
                        return true;

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "Mode 2 form 2 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}",
                                               channel[0x00C], channel[0x00D], channel[0x00E], calculatedEdc,
                                               storedEdc);

                    return false;
                }
                else
                {
                    if(channel[0x010] != channel[0x014] ||
                       channel[0x011] != channel[0x015] ||
                       channel[0x012] != channel[0x016] ||
                       channel[0x013] != channel[0x017])
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   "Subheader copies differ in mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}",
                                                   channel[0x00C], channel[0x00D], channel[0x00E]);
                    }

                    var address = new byte[4];
                    var eccP    = new byte[172];
                    var eccQ    = new byte[104];

                    Array.Copy(mode2Sector, 0x80C, eccP, 0, 172);
                    Array.Copy(mode2Sector, 0x8B8, eccQ, 0, 104);

                    bool failedEccP = !CheckEcc(address, mode2Sector, 86, 24, 2,  86, eccP);
                    bool failedEccQ = !CheckEcc(address, mode2Sector, 52, 43, 86, 88, eccQ);

                    correctEccP = !failedEccP;
                    correctEccQ = !failedEccQ;

                    if(failedEccP)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   "Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC P check",
                                                   channel[0x00C], channel[0x00D], channel[0x00E]);
                    }

                    if(failedEccQ)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   "Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC Q check",
                                                   channel[0x00C], channel[0x00D], channel[0x00E]);
                    }

                    var  storedEdc     = BitConverter.ToUInt32(mode2Sector, 0x808);
                    uint calculatedEdc = ComputeEdc(0, mode2Sector, 0x808);

                    correctEdc = calculatedEdc == storedEdc;

                    if(calculatedEdc == storedEdc)
                        return !failedEccP && !failedEccQ;

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "Mode 2 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}",
                                               channel[0x00C], channel[0x00D], channel[0x00E], calculatedEdc,
                                               storedEdc);

                    return false;
                }
            }
            default:
                AaruConsole.DebugWriteLine(MODULE_NAME, "Unknown mode {0} sector at address: {1:X2}:{2:X2}:{3:X2}",
                                           channel[0x00F], channel[0x00C], channel[0x00D], channel[0x00E]);

                return null;
        }
    }

    static uint ComputeEdc(uint edc, IReadOnlyList<byte> src, int size)
    {
        var pos = 0;

        for(; size > 0; size--)
            edc = edc >> 8 ^ _edcTable[(edc ^ src[pos++]) & 0xFF];

        return edc;
    }

    static bool? CheckCdSectorSubChannel(IReadOnlyList<byte> subchannel)
    {
        bool? status       = true;
        var   qSubChannel  = new byte[12];
        var   cdTextPack1  = new byte[18];
        var   cdTextPack2  = new byte[18];
        var   cdTextPack3  = new byte[18];
        var   cdTextPack4  = new byte[18];
        var   cdSubRwPack1 = new byte[24];
        var   cdSubRwPack2 = new byte[24];
        var   cdSubRwPack3 = new byte[24];
        var   cdSubRwPack4 = new byte[24];

        var i = 0;

        for(var j = 0; j < 12; j++)
            qSubChannel[j] = 0;

        for(var j = 0; j < 18; j++)
        {
            cdTextPack1[j] = 0;
            cdTextPack2[j] = 0;
            cdTextPack3[j] = 0;
            cdTextPack4[j] = 0;
        }

        for(var j = 0; j < 24; j++)
        {
            cdSubRwPack1[j] = 0;
            cdSubRwPack2[j] = 0;
            cdSubRwPack3[j] = 0;
            cdSubRwPack4[j] = 0;
        }

        for(var j = 0; j < 12; j++)
        {
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) << 1);
            qSubChannel[j] = (byte)(qSubChannel[j] | subchannel[i++] & 0x40);
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) >> 1);
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) >> 2);
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) >> 3);
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) >> 4);
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) >> 5);
            qSubChannel[j] = (byte)(qSubChannel[j] | (subchannel[i++] & 0x40) >> 6);
        }

        i = 0;

        for(var j = 0; j < 18; j++)
        {
            cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17)
                cdTextPack1[j] = (byte)(cdTextPack1[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17)
                cdTextPack1[j] = (byte)(cdTextPack1[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack1[j] = (byte)(cdTextPack1[j] | subchannel[i++] & 0x3F);
        }

        for(var j = 0; j < 18; j++)
        {
            cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17)
                cdTextPack2[j] = (byte)(cdTextPack2[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17)
                cdTextPack2[j] = (byte)(cdTextPack2[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack2[j] = (byte)(cdTextPack2[j] | subchannel[i++] & 0x3F);
        }

        for(var j = 0; j < 18; j++)
        {
            cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17)
                cdTextPack3[j] = (byte)(cdTextPack3[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17)
                cdTextPack3[j] = (byte)(cdTextPack3[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack3[j] = (byte)(cdTextPack3[j] | subchannel[i++] & 0x3F);
        }

        for(var j = 0; j < 18; j++)
        {
            cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17)
                cdTextPack4[j] = (byte)(cdTextPack4[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17)
                cdTextPack4[j] = (byte)(cdTextPack4[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack4[j] = (byte)(cdTextPack4[j] | subchannel[i++] & 0x3F);
        }

        i = 0;

        for(var j = 0; j < 24; j++)
            cdSubRwPack1[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack2[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack3[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack4[j] = (byte)(subchannel[i++] & 0x3F);

        switch(cdSubRwPack1[0])
        {
            case 0x00:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_Zero_Pack_in_subchannel);

                break;
            case 0x08:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_Line_Graphics_Pack_in_subchannel);

                break;
            case 0x09:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_CD_G_Pack_in_subchannel);

                break;
            case 0x0A:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_CD_EG_Pack_in_subchannel);

                break;
            case 0x14:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_CD_TEXT_Pack_in_subchannel);

                break;
            case 0x18:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_CD_MIDI_Pack_in_subchannel);

                break;
            case 0x38:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Detected_User_Pack_in_subchannel);

                break;
            default:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Detected_unknown_Pack_type_in_subchannel_mode_0_item_1,
                                           Convert.ToString(cdSubRwPack1[0] & 0x38, 2),
                                           Convert.ToString(cdSubRwPack1[0] & 0x07, 2));

                break;
        }

        var qSubChannelCrc    = BigEndianBitConverter.ToUInt16(qSubChannel, 10);
        var qSubChannelForCrc = new byte[10];
        Array.Copy(qSubChannel, 0, qSubChannelForCrc, 0, 10);
        ushort calculatedQcrc = CRC16CCITTContext.Calculate(qSubChannelForCrc);

        if(qSubChannelCrc != calculatedQcrc)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Q_subchannel_CRC_0_expected_1, calculatedQcrc,
                                       qSubChannelCrc);

            status = false;
        }

        if((cdTextPack1[0] & 0x80) == 0x80)
        {
            var cdTextPack1Crc    = BigEndianBitConverter.ToUInt16(cdTextPack1, 16);
            var cdTextPack1ForCrc = new byte[16];
            Array.Copy(cdTextPack1, 0, cdTextPack1ForCrc, 0, 16);
            ushort calculatedCdtp1Crc = CRC16CCITTContext.Calculate(cdTextPack1ForCrc);

            if(cdTextPack1Crc != calculatedCdtp1Crc &&
               cdTextPack1Crc != 0)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.CD_Text_Pack_one_CRC_0_expected_1,
                                           cdTextPack1Crc, calculatedCdtp1Crc);

                status = false;
            }
        }

        if((cdTextPack2[0] & 0x80) == 0x80)
        {
            var cdTextPack2Crc    = BigEndianBitConverter.ToUInt16(cdTextPack2, 16);
            var cdTextPack2ForCrc = new byte[16];
            Array.Copy(cdTextPack2, 0, cdTextPack2ForCrc, 0, 16);
            ushort calculatedCdtp2Crc = CRC16CCITTContext.Calculate(cdTextPack2ForCrc);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Cyclic_CDTP2_0_Calc_CDTP2_1, cdTextPack2Crc,
                                       calculatedCdtp2Crc);

            if(cdTextPack2Crc != calculatedCdtp2Crc &&
               cdTextPack2Crc != 0)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.CD_Text_Pack_two_CRC_0_expected_1,
                                           cdTextPack2Crc, calculatedCdtp2Crc);

                status = false;
            }
        }

        if((cdTextPack3[0] & 0x80) == 0x80)
        {
            var cdTextPack3Crc    = BigEndianBitConverter.ToUInt16(cdTextPack3, 16);
            var cdTextPack3ForCrc = new byte[16];
            Array.Copy(cdTextPack3, 0, cdTextPack3ForCrc, 0, 16);
            ushort calculatedCdtp3Crc = CRC16CCITTContext.Calculate(cdTextPack3ForCrc);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Cyclic_CDTP3_0_Calc_CDTP3_1, cdTextPack3Crc,
                                       calculatedCdtp3Crc);

            if(cdTextPack3Crc != calculatedCdtp3Crc &&
               cdTextPack3Crc != 0)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.CD_Text_Pack_three_CRC_0_expected_1,
                                           cdTextPack3Crc, calculatedCdtp3Crc);

                status = false;
            }
        }

        if((cdTextPack4[0] & 0x80) != 0x80)
            return status;

        var cdTextPack4Crc    = BigEndianBitConverter.ToUInt16(cdTextPack4, 16);
        var cdTextPack4ForCrc = new byte[16];
        Array.Copy(cdTextPack4, 0, cdTextPack4ForCrc, 0, 16);
        ushort calculatedCdtp4Crc = CRC16CCITTContext.Calculate(cdTextPack4ForCrc);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Cyclic_CDTP4_0_Calc_CDTP4_1, cdTextPack4Crc,
                                   calculatedCdtp4Crc);

        if(cdTextPack4Crc == calculatedCdtp4Crc ||
           cdTextPack4Crc == 0)
            return status;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.CD_Text_Pack_four_CRC_0_expected_1, cdTextPack4Crc,
                                   calculatedCdtp4Crc);

        return false;
    }
}