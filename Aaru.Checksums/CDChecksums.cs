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
// Copyright © 2011-2026 Natalia Portillo
// ECC algorithm from ECM(c) 2002-2011 Neill Corlett
// ****************************************************************************/

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Checksums;

/// <summary>Result of a <see cref="CdChecksums.FixSector(byte[])" /> operation.</summary>
public enum SectorFixResult
{
    /// <summary>Input is invalid or the sector type has no repairable ECC.</summary>
    NotApplicable,
    /// <summary>Sector had no errors — no correction was needed.</summary>
    Correct,
    /// <summary>Sector had correctable errors — correction was applied.</summary>
    Fixed,
    /// <summary>Sector had uncorrectable errors.</summary>
    CouldNotFix
}

/// <summary>Implements ReedSolomon and CRC32 algorithms as used by CD-ROM</summary>
public static class CdChecksums
{
    const  string MODULE_NAME = "CD checksums";
    static byte[] _eccFTable;
    static byte[] _eccBTable;
    static uint[] _edcTable;
    static byte[] _gfExp;       // α^k for k=0..254, length 256 (_gfExp[255] wraps to _gfExp[0])
    static byte[] _gfLog;       // log_α(v) for v=1..255; _gfLog[0] unused
    static byte[] _eccPWeights; // H_P column weights: α^(25−pos) for pos=0..25
    static byte[] _eccQWeights; // H_Q column weights: α^(44−pos) for pos=0..44

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
                bool? channelStatus = CheckCdSectorChannel(buffer.AsSpan(0, 2352),
                                                           out correctEccP,
                                                           out correctEccQ,
                                                           out correctEdc);

                bool? subchannelStatus = CheckCdSectorSubChannel(buffer.AsSpan(2352, 96));
                bool? status           = null;

                if(channelStatus == false || subchannelStatus == false) status = false;

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
        if(_eccFTable != null && _eccBTable != null && _edcTable != null && _gfExp != null) return;

        _eccFTable = new byte[256];
        _eccBTable = new byte[256];
        _edcTable  = new uint[256];

        for(uint i = 0; i < 256; i++)
        {
            uint edc = i;
            uint j   = i << 1 ^ ((i & 0x80) == 0x80 ? 0x11D : 0U);
            _eccFTable[i]     = (byte)j;
            _eccBTable[i ^ j] = (byte)i;

            for(j = 0; j < 8; j++) edc = edc >> 1 ^ ((edc & 1) > 0 ? 0xD8018001 : 0);

            _edcTable[i] = edc;
        }

        // GF(2^8) log/exp tables using primitive element α=2 and polynomial x^8+x^4+x^3+x^2+1
        // as specified in ECMA-130 Annex A section A.3.
        _gfExp    = new byte[256];
        _gfLog    = new byte[256];
        _gfExp[0] = 1;

        for(var i = 1; i < 255; i++) _gfExp[i] = _eccFTable[_gfExp[i - 1]];

        _gfExp[255] = _gfExp[0];

        for(var i = 0; i < 255; i++) _gfLog[_gfExp[i]] = (byte)i;

        // H_P column weights: weights[pos] = α^(25−pos) for pos=0..25 (ECMA-130 Annex A, H_P matrix).
        _eccPWeights = new byte[26];

        for(var pos = 0; pos < 26; pos++) _eccPWeights[pos] = _gfExp[(255 + 25 - pos) % 255];

        // H_Q column weights: weights[pos] = α^(44−pos) for pos=0..44 (ECMA-130 Annex A, H_Q matrix).
        _eccQWeights = new byte[45];

        for(var pos = 0; pos < 45; pos++) _eccQWeights[pos] = _gfExp[(255 + 44 - pos) % 255];
    }

    static bool CheckEcc(ReadOnlySpan<byte> address,   ReadOnlySpan<byte> data,     uint majorCount, uint minorCount,
                         uint               majorMult, uint               minorInc, ReadOnlySpan<byte> ecc)
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
                byte temp = index < 4 ? address[(int)index] : data[(int)(index - 4)];
                index += minorInc;

                if(index >= size) index -= size;

                eccA ^= temp;
                eccB ^= temp;
                eccA =  _eccFTable[eccA];
            }

            eccA = _eccBTable[_eccFTable[eccA] ^ eccB];

            if(ecc[(int)major] != eccA || ecc[(int)(major + majorCount)] != (eccA ^ eccB)) return false;
        }

        return true;
    }

    static bool? CheckCdSectorChannel(ReadOnlySpan<byte> channel, out bool? correctEccP, out bool? correctEccQ,
                                      out bool?          correctEdc)
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

        //AaruLogging.DebugWriteLine(MODULE_NAME, "Data sector, address {0:X2}:{1:X2}:{2:X2}", channel[0x00C],
        //                          channel[0x00D], channel[0x00E]);

        switch(channel[0x00F] & 0x03)
        {
            // mode (1 byte)
            case 0x00:
            {
                //AaruLogging.DebugWriteLine(MODULE_NAME, "Mode 0 sector at address {0:X2}:{1:X2}:{2:X2}",
                //                          channel[0x00C], channel[0x00D], channel[0x00E]);
                for(var i = 0x010; i < 0x930; i++)
                {
                    if(channel[i] == 0x00) continue;

                    AaruLogging.Debug(MODULE_NAME,
                                      "Mode 0 sector with error at address: {0:X2}:{1:X2}:{2:X2}",
                                      channel[0x00C],
                                      channel[0x00D],
                                      channel[0x00E]);

                    return false;
                }

                return true;
            }

            // mode (1 byte)
            //AaruLogging.DebugWriteLine(MODULE_NAME, "Mode 1 sector at address {0:X2}:{1:X2}:{2:X2}",
            //                          channel[0x00C], channel[0x00D], channel[0x00E]);
            case 0x01 when channel[0x814] != 0x00 || // reserved (8 bytes)
                           channel[0x815] != 0x00 ||
                           channel[0x816] != 0x00 ||
                           channel[0x817] != 0x00 ||
                           channel[0x818] != 0x00 ||
                           channel[0x819] != 0x00 ||
                           channel[0x81A] != 0x00 ||
                           channel[0x81B] != 0x00:
                AaruLogging.Debug(MODULE_NAME,
                                  "Mode 1 sector with data in reserved bytes at address: {0:X2}:{1:X2}:{2:X2}",
                                  channel[0x00C],
                                  channel[0x00D],
                                  channel[0x00E]);

                return false;
            case 0x01:
            {
                ReadOnlySpan<byte> address = channel.Slice(0x0C,  4);
                ReadOnlySpan<byte> data    = channel.Slice(0x10,  2060);
                ReadOnlySpan<byte> data2   = channel.Slice(0x10,  2232);
                ReadOnlySpan<byte> eccP    = channel.Slice(0x81C, 172);
                ReadOnlySpan<byte> eccQ    = channel.Slice(0x8C8, 104);

                bool failedEccP = !CheckEcc(address, data,  86, 24, 2,  86, eccP);
                bool failedEccQ = !CheckEcc(address, data2, 52, 43, 86, 88, eccQ);

                correctEccP = !failedEccP;
                correctEccQ = !failedEccQ;

                if(failedEccP)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC P check",
                                      channel[0x00C],
                                      channel[0x00D],
                                      channel[0x00E]);
                }

                if(failedEccQ)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC Q check",
                                      channel[0x00C],
                                      channel[0x00D],
                                      channel[0x00E]);
                }

                var  storedEdc     = BinaryPrimitives.ReadUInt32LittleEndian(channel.Slice(0x810, 4));
                uint calculatedEdc = ComputeEdc(0, channel, 0x810);

                correctEdc = calculatedEdc == storedEdc;

                if(calculatedEdc == storedEdc) return !failedEccP && !failedEccQ;

                AaruLogging.Debug(MODULE_NAME,
                                  "Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}",
                                  channel[0x00C],
                                  channel[0x00D],
                                  channel[0x00E],
                                  calculatedEdc,
                                  storedEdc);

                return false;
            }

            // mode (1 byte)
            case 0x02:
            {
                //AaruLogging.DebugWriteLine(MODULE_NAME, "Mode 2 sector at address {0:X2}:{1:X2}:{2:X2}",
                //                          channel[0x00C], channel[0x00D], channel[0x00E]);
                ReadOnlySpan<byte> mode2Sector = channel[0x10..];

                if((channel[0x012] & 0x20) == 0x20) // mode 2 form 2
                {
                    if(channel[0x010] != channel[0x014] ||
                       channel[0x011] != channel[0x015] ||
                       channel[0x012] != channel[0x016] ||
                       channel[0x013] != channel[0x017])
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "Subheader copies differ in mode 2 form 2 sector at address: {0:X2}:{1:X2}:{2:X2}",
                                          channel[0x00C],
                                          channel[0x00D],
                                          channel[0x00E]);
                    }

                    var storedEdc = BinaryPrimitives.ReadUInt32LittleEndian(mode2Sector.Slice(0x91C, 4));

                    // No CRC stored!
                    if(storedEdc == 0x00000000) return true;

                    uint calculatedEdc = ComputeEdc(0, mode2Sector, 0x91C);

                    correctEdc = calculatedEdc == storedEdc;

                    if(calculatedEdc == storedEdc) return true;

                    AaruLogging.Debug(MODULE_NAME,
                                      "Mode 2 form 2 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}",
                                      channel[0x00C],
                                      channel[0x00D],
                                      channel[0x00E],
                                      calculatedEdc,
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
                        AaruLogging.Debug(MODULE_NAME,
                                          "Subheader copies differ in mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}",
                                          channel[0x00C],
                                          channel[0x00D],
                                          channel[0x00E]);
                    }

                    ReadOnlySpan<byte> address = stackalloc byte[4];
                    ReadOnlySpan<byte> eccP    = mode2Sector.Slice(0x80C, 172);
                    ReadOnlySpan<byte> eccQ    = mode2Sector.Slice(0x8B8, 104);

                    bool failedEccP = !CheckEcc(address, mode2Sector, 86, 24, 2,  86, eccP);
                    bool failedEccQ = !CheckEcc(address, mode2Sector, 52, 43, 86, 88, eccQ);

                    correctEccP = !failedEccP;
                    correctEccQ = !failedEccQ;

                    if(failedEccP)
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC P check",
                                          channel[0x00C],
                                          channel[0x00D],
                                          channel[0x00E]);
                    }

                    if(failedEccQ)
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC Q check",
                                          channel[0x00C],
                                          channel[0x00D],
                                          channel[0x00E]);
                    }

                    var  storedEdc     = BinaryPrimitives.ReadUInt32LittleEndian(mode2Sector.Slice(0x808, 4));
                    uint calculatedEdc = ComputeEdc(0, mode2Sector, 0x808);

                    correctEdc = calculatedEdc == storedEdc;

                    if(calculatedEdc == storedEdc) return !failedEccP && !failedEccQ;

                    AaruLogging.Debug(MODULE_NAME,
                                      "Mode 2 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}",
                                      channel[0x00C],
                                      channel[0x00D],
                                      channel[0x00E],
                                      calculatedEdc,
                                      storedEdc);

                    return false;
                }
            }
            default:
                AaruLogging.Debug(MODULE_NAME,
                                  "Unknown mode {0} sector at address: {1:X2}:{2:X2}:{3:X2}",
                                  channel[0x00F],
                                  channel[0x00C],
                                  channel[0x00D],
                                  channel[0x00E]);

                return null;
        }
    }

    /// <summary>
    ///     Attempts to repair a raw 2352-byte data sector using the stored ECC-P and ECC-Q values according to ECMA-119.
    /// </summary>
    /// <param name="buffer">Raw 2352-byte sector.</param>
    /// <returns>
    ///     <see cref="SectorFixResult.Correct" /> if the sector was already correct,
    ///     <see cref="SectorFixResult.Fixed" /> if it had errors that were successfully corrected,
    ///     <see cref="SectorFixResult.CouldNotFix" /> if it had uncorrectable errors, and
    ///     <see cref="SectorFixResult.NotApplicable" /> if the input is invalid or the sector type has no repairable ECC.
    /// </returns>
    public static SectorFixResult FixSector(byte[] buffer) => FixSector(buffer, (bool[])null);

    /// <summary>
    ///     Attempts to repair a raw 2352-byte data sector using stored ECC-P and ECC-Q values and MMC C2 pointers.
    /// </summary>
    /// <param name="buffer">Raw 2352-byte sector.</param>
    /// <param name="c2Pointers">294-byte C2 pointer map, or 296-byte C2 pointer map plus block error bytes.</param>
    /// <returns>
    ///     <see cref="SectorFixResult.Correct" /> if the sector was already correct,
    ///     <see cref="SectorFixResult.Fixed" /> if it had errors that were successfully corrected,
    ///     <see cref="SectorFixResult.CouldNotFix" /> if it had uncorrectable errors, and
    ///     <see cref="SectorFixResult.NotApplicable" /> if the input is invalid or the sector type has no repairable ECC.
    /// </returns>
    public static SectorFixResult FixSector(byte[] buffer, byte[] c2Pointers)
    {
        if(buffer is not { Length: 2352 }) return SectorFixResult.NotApplicable;

        if(c2Pointers == null) return FixSector(buffer, (bool[])null);

        if(c2Pointers.Length != 294 && c2Pointers.Length != 296) return SectorFixResult.NotApplicable;

        var original = new byte[buffer.Length];
        Array.Copy(buffer, original, buffer.Length);

        for(var bitOrder = 0; bitOrder < 2; bitOrder++)
        {
            Array.Copy(original, buffer, buffer.Length);

            bool[]          erasureMap = CreateErasureMapFromC2Pointers(c2Pointers, bitOrder == 0);
            SectorFixResult result     = FixSector(buffer, erasureMap);

            if(result == SectorFixResult.CouldNotFix) continue;

            return result == SectorFixResult.Correct && SectorChanged(original, buffer)
                       ? SectorFixResult.Fixed
                       : result;
        }

        Array.Copy(original, buffer, buffer.Length);

        return SectorFixResult.CouldNotFix;
    }

    static bool SectorChanged(ReadOnlySpan<byte> original, ReadOnlySpan<byte> current) =>
        !MemoryExtensions.SequenceEqual(original, current);

    static SectorFixResult FixSector(byte[] buffer, bool[] erasureMap)
    {
        if(buffer is not { Length: 2352 }) return SectorFixResult.NotApplicable;

        EccInit();

        if(buffer[0x000] != 0x00 ||
           buffer[0x001] != 0xFF ||
           buffer[0x002] != 0xFF ||
           buffer[0x003] != 0xFF ||
           buffer[0x004] != 0xFF ||
           buffer[0x005] != 0xFF ||
           buffer[0x006] != 0xFF ||
           buffer[0x007] != 0xFF ||
           buffer[0x008] != 0xFF ||
           buffer[0x009] != 0xFF ||
           buffer[0x00A] != 0xFF ||
           buffer[0x00B] != 0x00)
            return SectorFixResult.NotApplicable;

        SectorFixResult result = (buffer[0x00F] & 0x03) switch
                                 {
                                     0x01 => FixMode1Sector(buffer, erasureMap),
                                     0x02 when (buffer[0x012] & 0x20) == 0x20 => SectorFixResult.NotApplicable,
                                     0x02 => FixMode2Form1Sector(buffer, erasureMap),
                                     _ => SectorFixResult.NotApplicable
                                 };

        return result;
    }

    static bool[] CreateErasureMapFromC2Pointers(byte[] c2Pointers, bool msbFirst)
    {
        var erasureMap = new bool[2352];

        for(var i = 0; i < 294; i++)
        {
            for(var bit = 0; bit < 8; bit++)
            {
                int mask = msbFirst ? 0x80 >> bit : 1 << bit;

                if((c2Pointers[i] & mask) != 0) erasureMap[i * 8 + bit] = true;
            }
        }

        return erasureMap;
    }

    static SectorFixResult FixMode1Sector(byte[] sector, bool[] erasureMap)
    {
        bool? status = CheckCdSectorChannel(sector, out bool? correctEccP, out bool? correctEccQ, out bool? _);

        if(status == true) return SectorFixResult.Correct;

        for(var i = 0x814; i < 0x81C; i++) sector[i] = 0;

        if(correctEccP == true && correctEccQ == true)
        {
            UpdateEdc(sector, 0, 0x810, 0x810);

            return CheckCdSectorChannel(sector, out _, out _, out _) == true
                       ? SectorFixResult.Fixed
                       : SectorFixResult.CouldNotFix;
        }

        int[] pMap = CreateOffsetMap(2064, 0x0C, 0x10, false);
        int[] qMap = CreateOffsetMap(2236, 0x0C, 0x10, false);

        return FixSectorWithEcc(sector, pMap, qMap, 0x81C, 0x8C8, 0, 0x810, 0x810, erasureMap);
    }

    static SectorFixResult FixMode2Form1Sector(byte[] sector, bool[] erasureMap)
    {
        bool? status = CheckCdSectorChannel(sector, out bool? correctEccP, out bool? correctEccQ, out bool? _);

        if(status == true) return SectorFixResult.Correct;

        if(correctEccP == true && correctEccQ == true)
        {
            UpdateEdc(sector, 0x10, 0x808, 0x818);

            return CheckCdSectorChannel(sector, out _, out _, out _) == true
                       ? SectorFixResult.Fixed
                       : SectorFixResult.CouldNotFix;
        }

        int[] pMap = CreateOffsetMap(2064, 0, 0x10, true);
        int[] qMap = CreateOffsetMap(2236, 0, 0x10, true);

        return FixSectorWithEcc(sector, pMap, qMap, 0x81C, 0x8C8, 0x10, 0x808, 0x818, erasureMap);
    }

    static SectorFixResult FixSectorWithEcc(byte[] sector, int[] pMap, int[] qMap, int eccPOffset, int eccQOffset,
                                            int    edcSourceOffset, int edcSize, int edcOffset, bool[] erasureMap)
    {
        int[][] pRows = BuildRowOffsets(pMap, 86, 24, 2,  86);
        int[][] qRows = BuildRowOffsets(qMap, 52, 43, 86, 88);

        // Reverse maps: sector byte offset → (row index, position in row) for each code.
        // Used by TryFixEccValidated to require cross-code locator agreement before applying a correction.
        var pByteToRow = new int[0x930];
        var qByteToRow = new int[0x930];

        for(var i = 0; i < 0x930; i++)
        {
            pByteToRow[i] = -1;
            qByteToRow[i] = -1;
        }

        for(var maj = 0; maj < 86; maj++)
        {
            for(var min = 0; min < 24; min++)
                if(pRows[maj][min] >= 0)
                    pByteToRow[pRows[maj][min]] = maj;
        }

        for(var maj = 0; maj < 52; maj++)
        {
            for(var min = 0; min < 43; min++)
                if(qRows[maj][min] >= 0)
                    qByteToRow[qRows[maj][min]] = maj;
        }

        // Also map parity byte offsets into the reverse maps.
        for(var maj = 0; maj < 86; maj++)
        {
            int pb1 = eccPOffset + maj, pb2 = eccPOffset + 86 + maj;

            if(pb1 < 0x930) pByteToRow[pb1] = maj;

            if(pb2 >= 0x930) continue;

            pByteToRow[pb2] = maj;
        }

        for(var maj = 0; maj < 52; maj++)
        {
            int qb1 = eccQOffset + maj, qb2 = eccQOffset + 52 + maj;

            if(qb1 < 0x930) qByteToRow[qb1] = maj;

            if(qb2 >= 0x930) continue;

            qByteToRow[qb2] = maj;
        }

        if(erasureMap != null)
        {
            SectorFixResult erasureResult =
                FixSectorWithErasures(sector, pRows, qRows, eccPOffset, eccQOffset, erasureMap);

            if(erasureResult != SectorFixResult.CouldNotFix) return erasureResult;
        }

        var previous = new byte[sector.Length];

        for(var pass = 0; pass < 64; pass++)
        {
            uint[] pSyndromes = ComputeRowSyndromes(sector, pRows, eccPOffset, 86, 24, _eccPWeights);
            uint[] qSyndromes = ComputeRowSyndromes(sector, qRows, eccQOffset, 52, 43, _eccQWeights);
            int    failedRows = CountFailedRows(pSyndromes) + CountFailedRows(qSyndromes);
            Array.Copy(sector, previous, sector.Length);

            // Fix P-rows (skip if Q's 1-error locator points elsewhere), then undo newly-broken Q-rows.
            bool corrected = TryFixEccValidated(sector,
                                                pRows,
                                                eccPOffset,
                                                86,
                                                24,
                                                _eccPWeights,
                                                qRows,
                                                eccQOffset,
                                                52,
                                                43,
                                                _eccQWeights,
                                                qByteToRow);

            RevertNewlyFailingRows(sector, qRows, eccQOffset, 52, 43, _eccQWeights, qSyndromes);

            // Recompute P syndromes after P-pass + Q-reverts — these reflect which P-rows are now correct.
            uint[] pSyndromesAfterP = ComputeRowSyndromes(sector, pRows, eccPOffset, 86, 24, _eccPWeights);

            // Fix Q-rows (skip if P's 1-error locator points elsewhere), then undo newly-broken P-rows.
            corrected = TryFixEccValidated(sector,
                                           qRows,
                                           eccQOffset,
                                           52,
                                           43,
                                           _eccQWeights,
                                           pRows,
                                           eccPOffset,
                                           86,
                                           24,
                                           _eccPWeights,
                                           pByteToRow) ||
                        corrected;

            RevertNewlyFailingRows(sector, pRows, eccPOffset, 86, 24, _eccPWeights, pSyndromesAfterP);

            if(corrected)
            {
                uint[] correctedPSyndromes = ComputeRowSyndromes(sector, pRows, eccPOffset, 86, 24, _eccPWeights);
                uint[] correctedQSyndromes = ComputeRowSyndromes(sector, qRows, eccQOffset, 52, 43, _eccQWeights);
                int correctedFailedRows = CountFailedRows(correctedPSyndromes) + CountFailedRows(correctedQSyndromes);

                if(correctedFailedRows >= failedRows)
                {
                    Array.Copy(previous, sector, sector.Length);
                    corrected = false;
                }
            }

            bool? status =
                CheckCdSectorChannel(sector, out bool? correctEccP, out bool? correctEccQ, out bool? correctEdc);

            if(correctEccP == true && correctEccQ == true)
            {
                if(correctEdc != true) UpdateEdc(sector, edcSourceOffset, edcSize, edcOffset);

                return CheckCdSectorChannel(sector, out _, out _, out _) == true
                           ? SectorFixResult.Fixed
                           : SectorFixResult.CouldNotFix;
            }

            if(!corrected || status == null) break;
        }

        // Single-error correction stalled — try 2-error correction with cross-code validation.
        var previous2 = new byte[sector.Length];

        for(var brutePass = 0; brutePass < 64; brutePass++)
        {
            bool bruteFixed = TryFix2ErrorRows(sector,
                                               pRows,
                                               86,
                                               24,
                                               qRows,
                                               52,
                                               43,
                                               eccPOffset,
                                               eccQOffset,
                                               _eccPWeights,
                                               _eccQWeights);

            bruteFixed |= TryFix2ErrorRows(sector,
                                           qRows,
                                           52,
                                           43,
                                           pRows,
                                           86,
                                           24,
                                           eccQOffset,
                                           eccPOffset,
                                           _eccQWeights,
                                           _eccPWeights);

            if(!bruteFixed) break;

            // Re-run single-error correction after each 2-error pass, with the same
            // convergence guard as the outer loop so that wrong corrections (fake locators
            // from still-multi-error rows) are reverted and do not accumulate.
            for(var synPass = 0; synPass < 64; synPass++)
            {
                uint[] pSyn2Before   = ComputeRowSyndromes(sector, pRows, eccPOffset, 86, 24, _eccPWeights);
                uint[] qSyn2Before   = ComputeRowSyndromes(sector, qRows, eccQOffset, 52, 43, _eccQWeights);
                int    failed2Before = CountFailedRows(pSyn2Before) + CountFailedRows(qSyn2Before);
                Array.Copy(sector, previous2, sector.Length);

                bool corrected2 = TryFixEcc(sector, pRows, eccPOffset, 86, 24, _eccPWeights);
                RevertNewlyFailingRows(sector, qRows, eccQOffset, 52, 43, _eccQWeights, qSyn2Before);
                uint[] pSyn2AfterP = ComputeRowSyndromes(sector, pRows, eccPOffset, 86, 24, _eccPWeights);
                corrected2 = TryFixEcc(sector, qRows, eccQOffset, 52, 43, _eccQWeights) || corrected2;
                RevertNewlyFailingRows(sector, pRows, eccPOffset, 86, 24, _eccPWeights, pSyn2AfterP);

                if(corrected2)
                {
                    uint[] pSyn2After   = ComputeRowSyndromes(sector, pRows, eccPOffset, 86, 24, _eccPWeights);
                    uint[] qSyn2After   = ComputeRowSyndromes(sector, qRows, eccQOffset, 52, 43, _eccQWeights);
                    int    failed2After = CountFailedRows(pSyn2After) + CountFailedRows(qSyn2After);

                    if(failed2After >= failed2Before)
                    {
                        Array.Copy(previous2, sector, sector.Length);

                        break;
                    }
                }

                CheckCdSectorChannel(sector, out bool? correctEccP2, out bool? correctEccQ2, out bool? correctEdc2);

                if(correctEccP2 == true && correctEccQ2 == true)
                {
                    if(correctEdc2 != true) UpdateEdc(sector, edcSourceOffset, edcSize, edcOffset);

                    return CheckCdSectorChannel(sector, out _, out _, out _) == true
                               ? SectorFixResult.Fixed
                               : SectorFixResult.CouldNotFix;
                }

                if(!corrected2) break;
            }
        }

        return SectorFixResult.CouldNotFix;
    }

    /// <summary>
    ///     2-error correction on stuck code rows using cross-code syndrome validation.
    ///     For each row with a non-zero ECMA syndrome (S₀,S₁), enumerates all C(n,2) position pairs (p1,p2)
    ///     and solves for (e1,e2) in O(1) via the ECMA-130 Annex A H matrix formula:
    ///     e1 = (S₁ ⊕ w2·S₀)/(w1 ⊕ w2),  e2 = S₀ ⊕ e1  where w_i = fixWeights[p_i].
    ///     All pairs are tried; the pair that minimises the cross-code failure count is accepted,
    ///     provided it does not increase that count (Q must not get worse when fixing a P-row and
    ///     vice-versa).  Accepting the globally best pair prevents false-positive corrections that
    ///     arise when a correct fix reduces 2-error cross-code rows to 1-error (still failing, so
    ///     a strict-decrease test would wrongly reject it).
    /// </summary>
    static bool TryFix2ErrorRows(byte[]  sector,         int[][] fixRows,         int fixMajorCount, int fixMinorCount,
                                 int[][] checkRows,      int     checkMajorCount, int checkMinorCount, int fixEccOffset,
                                 int     checkEccOffset, byte[]  fixWeights,      byte[] checkWeights)
    {
        int fixPositionCount = fixMinorCount + 2;
        var anyFixed         = false;

        for(var major = 0; major < fixMajorCount; major++)
        {
            (byte s0, byte s1) = ComputeEcmaRowSyndrome(sector,
                                                        fixRows[major],
                                                        fixEccOffset,
                                                        major,
                                                        fixMajorCount,
                                                        fixMinorCount,
                                                        fixWeights);

            if(s0 == 0 && s1 == 0) continue;

            int checkFailedBefore =
                CountFailedRows(ComputeRowSyndromes(sector,
                                                    checkRows,
                                                    checkEccOffset,
                                                    checkMajorCount,
                                                    checkMinorCount,
                                                    checkWeights));

            // Find the pair (p1, p2) that minimises cross-code failures after correction.
            int  bestCheckFailed = checkFailedBefore + 1; // sentinel: must be ≤ before to accept
            int  bestP1          = -1;
            int  bestP2          = -1;
            byte bestE1          = 0;
            byte bestE2          = 0;

            for(var p1 = 0; p1 < fixPositionCount; p1++)
            {
                for(int p2 = p1 + 1; p2 < fixPositionCount; p2++)
                {
                    byte w1    = fixWeights[p1];
                    byte w2    = fixWeights[p2];
                    var  denom = (byte)(w1 ^ w2);

                    if(denom == 0) continue;

                    byte e1 = GfDiv((byte)(s1 ^ GfMul(w2, s0)), denom);
                    var  e2 = (byte)(s0 ^ e1);

                    if(e1 == 0 && e2 == 0) continue;

                    ApplyEccRowError(sector, fixRows[major], fixEccOffset, major, fixMajorCount, fixMinorCount, p1, e1);
                    ApplyEccRowError(sector, fixRows[major], fixEccOffset, major, fixMajorCount, fixMinorCount, p2, e2);

                    int checkFailedAfter =
                        CountFailedRows(ComputeRowSyndromes(sector,
                                                            checkRows,
                                                            checkEccOffset,
                                                            checkMajorCount,
                                                            checkMinorCount,
                                                            checkWeights));

                    ApplyEccRowError(sector, fixRows[major], fixEccOffset, major, fixMajorCount, fixMinorCount, p1, e1);
                    ApplyEccRowError(sector, fixRows[major], fixEccOffset, major, fixMajorCount, fixMinorCount, p2, e2);

                    if(checkFailedAfter >= bestCheckFailed) continue;

                    bestCheckFailed = checkFailedAfter;
                    bestP1          = p1;
                    bestP2          = p2;
                    bestE1          = e1;
                    bestE2          = e2;
                }
            }

            // Accept only if the best candidate strictly decreases cross-code failures.
            if(bestP1 < 0 || bestCheckFailed >= checkFailedBefore) continue;

            ApplyEccRowError(sector, fixRows[major], fixEccOffset, major, fixMajorCount, fixMinorCount, bestP1, bestE1);

            ApplyEccRowError(sector, fixRows[major], fixEccOffset, major, fixMajorCount, fixMinorCount, bestP2, bestE2);

            anyFixed = true;
        }

        return anyFixed;
    }

    static int[][] BuildRowOffsets(int[] offsetMap, int majorCount, int minorCount, int majorMult, int minorInc)
    {
        var rows = new int[majorCount][];
        int size = majorCount * minorCount;

        for(var major = 0; major < majorCount; major++)
        {
            int index = (major >> 1) * majorMult + (major & 1);
            rows[major] = new int[minorCount];

            for(var minor = 0; minor < minorCount; minor++)
            {
                rows[major][minor] =  offsetMap[index];
                index              += minorInc;

                if(index >= size) index -= size;
            }
        }

        return rows;
    }

    static SectorFixResult FixSectorWithErasures(byte[] sector,     int[][] pRows, int[][] qRows, int eccPOffset,
                                                 int    eccQOffset, bool[]  erasureMap)
    {
        for(var pass = 0; pass < 64; pass++)
        {
            bool corrected = TryFixEccErasures(sector, pRows, eccPOffset, 86, 24, erasureMap, _eccPWeights);

            corrected = TryFixEccErasures(sector, qRows, eccQOffset, 52, 43, erasureMap, _eccQWeights) || corrected;

            bool? status =
                CheckCdSectorChannel(sector, out bool? correctEccP, out bool? correctEccQ, out bool? correctEdc);

            if(correctEccP == true && correctEccQ == true)
                return correctEdc == true ? SectorFixResult.Fixed : SectorFixResult.CouldNotFix;

            if(!corrected || status == null) break;
        }

        return SectorFixResult.CouldNotFix;
    }

    static bool TryFixEccErasures(byte[] sector,     int[][] rows, int eccOffset, int majorCount, int minorCount,
                                  bool[] erasureMap, byte[]  weights)
    {
        var corrected = false;

        for(var major = 0; major < majorCount; major++)
        {
            List<int> positions = GetKnownErasurePositions(rows[major],
                                                           eccOffset,
                                                           major,
                                                           majorCount,
                                                           minorCount,
                                                           erasureMap);

            if(positions.Count is 0 or > 2) continue;

            (byte s0, byte s1) =
                ComputeEcmaRowSyndrome(sector, rows[major], eccOffset, major, majorCount, minorCount, weights);

            if(s0 == 0 && s1 == 0)
            {
                ClearKnownErasurePositions(rows[major],
                                           eccOffset,
                                           major,
                                           majorCount,
                                           minorCount,
                                           positions,
                                           erasureMap);

                continue;
            }

            if(positions.Count == 1)
            {
                if(!TrySolveOneErasureSyndrome(s0, s1, positions[0], weights, out byte error)) continue;

                ApplyEccRowError(sector, rows[major], eccOffset, major, majorCount, minorCount, positions[0], error);

                ClearKnownErasurePosition(rows[major],
                                          eccOffset,
                                          major,
                                          majorCount,
                                          minorCount,
                                          positions[0],
                                          erasureMap);

                corrected = true;

                continue;
            }

            if(!TrySolveTwoErasureSyndrome(s0,
                                           s1,
                                           positions[0],
                                           positions[1],
                                           weights,
                                           out byte error1,
                                           out byte error2))
                continue;

            ApplyEccRowError(sector, rows[major], eccOffset, major, majorCount, minorCount, positions[0], error1);
            ApplyEccRowError(sector, rows[major], eccOffset, major, majorCount, minorCount, positions[1], error2);
            ClearKnownErasurePositions(rows[major], eccOffset, major, majorCount, minorCount, positions, erasureMap);

            corrected = true;
        }

        return corrected;
    }

    // GF(2^8) multiply using log/exp tables; returns 0 if either operand is 0.
    static byte GfMul(byte a, byte b) => a == 0 || b == 0 ? (byte)0 : _gfExp[(_gfLog[a] + _gfLog[b]) % 255];

    // GF(2^8) divide a/b; returns 0 if a==0. b must be non-zero.
    static byte GfDiv(byte a, byte b) => a == 0 ? (byte)0 : _gfExp[(255 + _gfLog[a] - _gfLog[b]) % 255];

    /// <summary>
    ///     Computes the two ECMA-130 Annex A syndromes (S₀, S₁) for one ECC row.
    ///     S₀ = XOR of all codeword bytes (H row 1 = all-ones).
    ///     S₁ = Σ weights[pos]·byte[pos] (H row 2 = α-weighted sum).
    ///     Both are zero for a correct codeword.  A single error e at position pos satisfies
    ///     S₀=e and S₁=weights[pos]·e, giving pos=(n−1)−log_α(S₁/S₀).
    /// </summary>
    static (byte s0, byte s1) ComputeEcmaRowSyndrome(byte[] sector, int[] row, int eccOffset, int major, int majorCount,
                                                     int    minorCount, byte[] weights)
    {
        byte s0 = 0, s1 = 0;

        for(var pos = 0; pos < minorCount; pos++)
        {
            int offset = row[pos];

            if(offset < 0) continue;

            byte v = sector[offset];
            s0 ^= v;
            s1 ^= GfMul(weights[pos], v);
        }

        byte pa = sector[eccOffset + major];
        s0 ^= pa;
        s1 ^= GfMul(weights[minorCount], pa); // α^1 weight for first parity byte

        byte pb = sector[eccOffset + majorCount + major];
        s0 ^= pb;
        s1 ^= pb; // α^0 = 1 weight for second parity byte

        return (s0, s1);
    }

    static uint ComputeRowSyndrome(byte[] sector, int[] row, int eccOffset, int major, int majorCount, int minorCount,
                                   byte[] weights)
    {
        (byte s0, byte s1) = ComputeEcmaRowSyndrome(sector, row, eccOffset, major, majorCount, minorCount, weights);

        return (uint)(s0 << 8 | s1);
    }

    static List<int> GetKnownErasurePositions(int[]  row, int eccOffset, int major, int majorCount, int minorCount,
                                              bool[] erasureMap)
    {
        List<int> positions = [];

        for(var position = 0; position < minorCount + 2; position++)
        {
            int offset = GetEccRowPositionOffset(row, eccOffset, major, majorCount, minorCount, position);

            if(offset >= 0 && erasureMap[offset]) positions.Add(position);
        }

        return positions;
    }

    static void ClearKnownErasurePositions(int[]     row, int eccOffset, int major, int majorCount, int minorCount,
                                           List<int> positions, bool[] erasureMap)
    {
        foreach(int position in positions)
            ClearKnownErasurePosition(row, eccOffset, major, majorCount, minorCount, position, erasureMap);
    }

    static void ClearKnownErasurePosition(int[] row,      int    eccOffset, int major, int majorCount, int minorCount,
                                          int   position, bool[] erasureMap)
    {
        int offset = GetEccRowPositionOffset(row, eccOffset, major, majorCount, minorCount, position);

        if(offset >= 0) erasureMap[offset] = false;
    }

    static uint[] ComputeRowSyndromes(byte[] sector, int[][] rows, int eccOffset, int majorCount, int minorCount,
                                      byte[] weights)
    {
        var syndromes = new uint[majorCount];

        for(var major = 0; major < majorCount; major++)
            syndromes[major] =
                ComputeRowSyndrome(sector, rows[major], eccOffset, major, majorCount, minorCount, weights);

        return syndromes;
    }

    static int CountFailedRows(IEnumerable<uint> syndromes) => syndromes.Count(static syndrome => syndrome != 0);

    static int GetEccRowPositionOffset(int[] rowOffsets, int eccOffset, int major, int majorCount, int minorCount,
                                       int   position)
    {
        if(position < minorCount) return rowOffsets[position];

        return position == minorCount ? eccOffset + major : eccOffset + majorCount + major;
    }

    /// <summary>
    ///     Solve for the single erasure error value from ECMA syndromes.
    ///     From H·V=0 with one error e at position pos: S₀=e, S₁=weights[pos]·e.
    ///     Verify consistency and return error = S₀.
    /// </summary>
    static bool TrySolveOneErasureSyndrome(byte s0, byte s1, int position, byte[] weights, out byte error)
    {
        error = 0;

        if(s0 == 0) return false;

        // Verify syndrome is consistent with a single error at this position.
        if(GfMul(weights[position], s0) != s1) return false;

        error = s0;

        return true;
    }

    /// <summary>
    ///     Solve for two erasure error values from ECMA syndromes using the direct GF formula.
    ///     From H·V=0 with errors (e1,e2) at known positions (p1,p2):
    ///     e1 = (S₁ ⊕ w2·S₀) / (w1 ⊕ w2),  e2 = S₀ ⊕ e1  where w_i = weights[p_i].
    /// </summary>
    static bool TrySolveTwoErasureSyndrome(byte     s0,     byte     s1, int position1, int position2, byte[] weights,
                                           out byte error1, out byte error2)
    {
        error1 = 0;
        error2 = 0;

        byte w1    = weights[position1];
        byte w2    = weights[position2];
        var  denom = (byte)(w1 ^ w2);

        if(denom == 0) return false;

        error1 = GfDiv((byte)(s1 ^ GfMul(w2, s0)), denom);
        error2 = (byte)(s0 ^ error1);

        return true;
    }

    static void ApplyEccRowError(byte[] sector,     int[] rowOffsets, int  eccOffset, int major, int majorCount,
                                 int    minorCount, int   position,   byte error)
    {
        int offset = GetEccRowPositionOffset(rowOffsets, eccOffset, major, majorCount, minorCount, position);

        if(offset >= 0) sector[offset] ^= error;
    }

    /// <summary>
    ///     For each row in <paramref name="rows" /> that had a zero syndrome in <paramref name="syndromesBefore" />
    ///     (was correct) but now has a non-zero syndrome (broke), revertes the single error that was introduced.
    ///     This catches wrong single-error corrections from <see cref="TryFixEcc" /> that applied a fake locator
    ///     (derived from a multi-error row) at a byte that was previously clean, making the cross-code row fail.
    ///     The syndrome of a newly-failing row that was clean before is guaranteed to be a 1-error syndrome because
    ///     exactly one byte changed in that row (the fake correction), so the locator unambiguously identifies and
    ///     reverts it.
    /// </summary>
    static void RevertNewlyFailingRows(byte[] sector,  int[][] rows, int eccOffset, int majorCount, int minorCount,
                                       byte[] weights, uint[]  syndromesBefore)
    {
        int n = minorCount + 2;

        for(var major = 0; major < majorCount; major++)
        {
            // Only care about rows that were passing (syndrome == 0) before the last fix pass.
            if(syndromesBefore[major] != 0) continue;

            (byte s0, byte s1) =
                ComputeEcmaRowSyndrome(sector, rows[major], eccOffset, major, majorCount, minorCount, weights);

            if(s0 == 0) continue; // still passing — no wrong correction here

            // Row went from correct to failing: a wrong correction introduced exactly 1 error.
            // Use the ECMA single-error locator to find and revert it.
            byte locator = GfDiv(s1, s0);

            if(locator == 0) continue;

            int pos = n - 1 - _gfLog[locator];

            if(pos < 0 || pos >= n) continue;

            ApplyEccRowError(sector, rows[major], eccOffset, major, majorCount, minorCount, pos, s0);
        }
    }

    /// <summary>
    ///     Single-error correction with cross-code locator agreement validation.
    ///     For each candidate correction: the fix-code locator gives position <c>b</c> and value <c>e</c>.
    ///     The correction is accepted only if the check-code row containing <c>b</c> also has its
    ///     1-error locator pointing to <c>b</c> (i.e., both codes independently identify the same byte as
    ///     the single error).  This eliminates corrections at previously-correct bytes that happen to give
    ///     a valid-looking fix-code locator from a multi-error row.
    ///     When the check-code row has 2+ errors the locator does not point to <c>b</c>, so the correction
    ///     is skipped; subsequent passes (after the check-code errors decrease) will accept it.
    /// </summary>
    static bool TryFixEccValidated(byte[] sector,          int[][] rows, int eccOffset, int majorCount, int minorCount,
                                   byte[] weights,         int[][] checkRows, int checkEccOffset, int checkMajorCount,
                                   int    checkMinorCount, byte[]  checkWeights, int[] byteToCheckRow)
    {
        var corrected = false;
        int n         = minorCount      + 2;
        int nCheck    = checkMinorCount + 2;

        for(var major = 0; major < majorCount; major++)
        {
            (byte s0, byte s1) =
                ComputeEcmaRowSyndrome(sector, rows[major], eccOffset, major, majorCount, minorCount, weights);

            if(s0 == 0) continue;

            byte locator = GfDiv(s1, s0);

            if(locator == 0) continue;

            int pos = n - 1 - _gfLog[locator];

            if(pos < 0 || pos >= n) continue;

            // Get the byte offset this correction targets.
            int byteOffset = GetEccRowPositionOffset(rows[major], eccOffset, major, majorCount, minorCount, pos);

            if(byteOffset < 0 || byteOffset >= byteToCheckRow.Length)
                goto apply; // parity byte or unmapped: apply directly

            int checkRowIdx = byteToCheckRow[byteOffset];

            if(checkRowIdx < 0) goto apply; // byte not covered by check code: apply directly

            // Require the check-code row's 1-error locator to point to the same byte.
            (byte cs0, byte cs1) = ComputeEcmaRowSyndrome(sector,
                                                          checkRows[checkRowIdx],
                                                          checkEccOffset,
                                                          checkRowIdx,
                                                          checkMajorCount,
                                                          checkMinorCount,
                                                          checkWeights);

            if(cs0 == 0) goto skip; // check-code row is correct — fix-code "correction" would break it

            byte checkLocator = GfDiv(cs1, cs0);

            if(checkLocator == 0) goto skip;

            int checkLocPos = nCheck - 1 - _gfLog[checkLocator];

            // Only skip if the check-code has a VALID single-error locator that points to a DIFFERENT byte.
            // This catches the case: Q-row has exactly 1 error somewhere else, P-correction would introduce
            // a second error at the candidate byte.  If the locator is out-of-range (multi-error Q-row with
            // no valid single-error locator), we allow the P-correction — we just can't confirm it.
            if(checkLocPos >= 0 && checkLocPos < nCheck)
            {
                int checkByteOffset = GetEccRowPositionOffset(checkRows[checkRowIdx],
                                                              checkEccOffset,
                                                              checkRowIdx,
                                                              checkMajorCount,
                                                              checkMinorCount,
                                                              checkLocPos);

                if(checkByteOffset >= 0 && checkByteOffset != byteOffset) goto skip;
            }

        apply:
            ApplyEccRowError(sector, rows[major], eccOffset, major, majorCount, minorCount, pos, s0);
            corrected = true;

            continue;

        skip: ;
        }

        return corrected;
    }

    /// <summary>
    ///     Single-error correction using the ECMA-130 Annex A parity check matrices.
    ///     For each row with syndrome (S₀,S₁): error value = S₀,
    ///     error position = (n−1) − log_α(S₁/S₀) where n = minorCount+2.
    /// </summary>
    static bool TryFixEcc(byte[] sector, int[][] rows, int eccOffset, int majorCount, int minorCount, byte[] weights)
    {
        var corrected = false;
        int n         = minorCount + 2;

        for(var major = 0; major < majorCount; major++)
        {
            (byte s0, byte s1) =
                ComputeEcmaRowSyndrome(sector, rows[major], eccOffset, major, majorCount, minorCount, weights);

            if(s0 == 0) continue;

            byte locator = GfDiv(s1, s0);

            if(locator == 0) continue; // S₁=0 but S₀≠0: even-count error pattern, unlocatable

            int pos = n - 1 - _gfLog[locator];

            if(pos < 0 || pos >= n) continue;

            ApplyEccRowError(sector, rows[major], eccOffset, major, majorCount, minorCount, pos, s0);
            corrected = true;
        }

        return corrected;
    }

    static int[] CreateOffsetMap(int size, int addressOffset, int dataOffset, bool zeroAddress)
    {
        var map = new int[size];

        for(var i = 0; i < 4; i++) map[i] = zeroAddress ? -1 : addressOffset + i;

        for(var i = 4; i < size; i++) map[i] = dataOffset + i - 4;

        return map;
    }

    static void UpdateEdc(byte[] sector, int sourceOffset, int size, int destinationOffset)
    {
        uint   edc       = ComputeEdc(0, sector.AsSpan(sourceOffset, size), size);
        byte[] storedEdc = BitConverter.GetBytes(edc);
        Array.Copy(storedEdc, 0, sector, destinationOffset, storedEdc.Length);
    }

    static uint ComputeEdc(uint edc, ReadOnlySpan<byte> src, int size)
    {
        var pos = 0;

        for(; size > 0; size--) edc = edc >> 8 ^ _edcTable[(edc ^ src[pos++]) & 0xFF];

        return edc;
    }

    static bool? CheckCdSectorSubChannel(ReadOnlySpan<byte> subchannel)
    {
        bool? status       = true;
        var   qSubChannel  = new byte[12];
        var   cdTextPack1  = new byte[18];
        var   cdTextPack2  = new byte[18];
        var   cdTextPack3  = new byte[18];
        var   cdTextPack4  = new byte[18];
        var   cdSubRwPack1 = new byte[24];

        var i = 0;

        for(var j = 0; j < 12; j++) qSubChannel[j] = 0;

        for(var j = 0; j < 18; j++)
        {
            cdTextPack1[j] = 0;
            cdTextPack2[j] = 0;
            cdTextPack3[j] = 0;
            cdTextPack4[j] = 0;
        }

        for(var j = 0; j < 24; j++) cdSubRwPack1[j] = 0;

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

            if(j < 17) cdTextPack1[j] = (byte)(cdTextPack1[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17) cdTextPack1[j] = (byte)(cdTextPack1[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack1[j] = (byte)(cdTextPack1[j] | subchannel[i++] & 0x3F);
        }

        for(var j = 0; j < 18; j++)
        {
            cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17) cdTextPack2[j] = (byte)(cdTextPack2[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17) cdTextPack2[j] = (byte)(cdTextPack2[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack2[j] = (byte)(cdTextPack2[j] | subchannel[i++] & 0x3F);
        }

        for(var j = 0; j < 18; j++)
        {
            cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17) cdTextPack3[j] = (byte)(cdTextPack3[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17) cdTextPack3[j] = (byte)(cdTextPack3[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack3[j] = (byte)(cdTextPack3[j] | subchannel[i++] & 0x3F);
        }

        for(var j = 0; j < 18; j++)
        {
            cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x3F) << 2);

            if(j < 17) cdTextPack4[j] = (byte)(cdTextPack4[j++] | (subchannel[i] & 0xC0) >> 4);

            cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x0F) << 4);

            if(j < 17) cdTextPack4[j] = (byte)(cdTextPack4[j++] | (subchannel[i] & 0x3C) >> 2);

            cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x03) << 6);

            cdTextPack4[j] = (byte)(cdTextPack4[j] | subchannel[i++] & 0x3F);
        }

        i = 0;

        for(var j = 0; j < 24; j++) cdSubRwPack1[j] = (byte)(subchannel[i++] & 0x3F);

        switch(cdSubRwPack1[0])
        {
            case 0x00:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_Zero_Pack_in_subchannel);

                break;
            case 0x08:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_Line_Graphics_Pack_in_subchannel);

                break;
            case 0x09:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_CD_G_Pack_in_subchannel);

                break;
            case 0x0A:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_CD_EG_Pack_in_subchannel);

                break;
            case 0x14:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_CD_TEXT_Pack_in_subchannel);

                break;
            case 0x18:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_CD_MIDI_Pack_in_subchannel);

                break;
            case 0x38:
                AaruLogging.Debug(MODULE_NAME, Localization.Detected_User_Pack_in_subchannel);

                break;
            default:
                AaruLogging.Debug(MODULE_NAME,
                                  Localization.Detected_unknown_Pack_type_in_subchannel_mode_0_item_1,
                                  Convert.ToString(cdSubRwPack1[0] & 0x38, 2),
                                  Convert.ToString(cdSubRwPack1[0] & 0x07, 2));

                break;
        }

        var qSubChannelCrc    = BigEndianBitConverter.ToUInt16(qSubChannel, 10);
        var qSubChannelForCrc = new byte[10];
        Array.Copy(qSubChannel, 0, qSubChannelForCrc, 0, 10);
        ushort calculatedQcrc = CRC16CcittContext.Calculate(qSubChannelForCrc);

        if(qSubChannelCrc != calculatedQcrc)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.Q_subchannel_CRC_0_expected_1, calculatedQcrc, qSubChannelCrc);

            status = false;
        }

        if((cdTextPack1[0] & 0x80) == 0x80)
        {
            var cdTextPack1Crc    = BigEndianBitConverter.ToUInt16(cdTextPack1, 16);
            var cdTextPack1ForCrc = new byte[16];
            Array.Copy(cdTextPack1, 0, cdTextPack1ForCrc, 0, 16);
            ushort calculatedCdtp1Crc = CRC16CcittContext.Calculate(cdTextPack1ForCrc);

            if(cdTextPack1Crc != calculatedCdtp1Crc && cdTextPack1Crc != 0)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  Localization.CD_Text_Pack_one_CRC_0_expected_1,
                                  cdTextPack1Crc,
                                  calculatedCdtp1Crc);

                status = false;
            }
        }

        if((cdTextPack2[0] & 0x80) == 0x80)
        {
            var cdTextPack2Crc    = BigEndianBitConverter.ToUInt16(cdTextPack2, 16);
            var cdTextPack2ForCrc = new byte[16];
            Array.Copy(cdTextPack2, 0, cdTextPack2ForCrc, 0, 16);
            ushort calculatedCdtp2Crc = CRC16CcittContext.Calculate(cdTextPack2ForCrc);

            AaruLogging.Debug(MODULE_NAME,
                              Localization.Cyclic_CDTP2_0_Calc_CDTP2_1,
                              cdTextPack2Crc,
                              calculatedCdtp2Crc);

            if(cdTextPack2Crc != calculatedCdtp2Crc && cdTextPack2Crc != 0)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  Localization.CD_Text_Pack_two_CRC_0_expected_1,
                                  cdTextPack2Crc,
                                  calculatedCdtp2Crc);

                status = false;
            }
        }

        if((cdTextPack3[0] & 0x80) == 0x80)
        {
            var cdTextPack3Crc    = BigEndianBitConverter.ToUInt16(cdTextPack3, 16);
            var cdTextPack3ForCrc = new byte[16];
            Array.Copy(cdTextPack3, 0, cdTextPack3ForCrc, 0, 16);
            ushort calculatedCdtp3Crc = CRC16CcittContext.Calculate(cdTextPack3ForCrc);

            AaruLogging.Debug(MODULE_NAME,
                              Localization.Cyclic_CDTP3_0_Calc_CDTP3_1,
                              cdTextPack3Crc,
                              calculatedCdtp3Crc);

            if(cdTextPack3Crc != calculatedCdtp3Crc && cdTextPack3Crc != 0)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  Localization.CD_Text_Pack_three_CRC_0_expected_1,
                                  cdTextPack3Crc,
                                  calculatedCdtp3Crc);

                status = false;
            }
        }

        if((cdTextPack4[0] & 0x80) != 0x80) return status;

        var cdTextPack4Crc    = BigEndianBitConverter.ToUInt16(cdTextPack4, 16);
        var cdTextPack4ForCrc = new byte[16];
        Array.Copy(cdTextPack4, 0, cdTextPack4ForCrc, 0, 16);
        ushort calculatedCdtp4Crc = CRC16CcittContext.Calculate(cdTextPack4ForCrc);

        AaruLogging.Debug(MODULE_NAME, Localization.Cyclic_CDTP4_0_Calc_CDTP4_1, cdTextPack4Crc, calculatedCdtp4Crc);

        if(cdTextPack4Crc == calculatedCdtp4Crc || cdTextPack4Crc == 0) return status;

        AaruLogging.Debug(MODULE_NAME,
                          Localization.CD_Text_Pack_four_CRC_0_expected_1,
                          cdTextPack4Crc,
                          calculatedCdtp4Crc);

        return false;
    }
}