// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ClauniaSubchannelTransform.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the CD ECC algorithm.
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
// ECC algorithm from ECM(c) 2002-2011 Neill Corlett
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;

namespace Aaru.DiscImages;

public sealed partial class AaruFormat
{
    byte[] _eccBTable;
    byte[] _eccFTable;
    uint[] _edcTable;
    bool   _initedEdc;

    void EccInit()
    {
        if(_initedEdc)
            return;

        _eccFTable = new byte[256];
        _eccBTable = new byte[256];
        _edcTable  = new uint[256];

        for(uint i = 0; i < 256; i++)
        {
            uint edc = i;
            uint j   = (uint)((i << 1) ^ ((i & 0x80) == 0x80 ? 0x11D : 0));
            _eccFTable[i]     = (byte)j;
            _eccBTable[i ^ j] = (byte)i;

            for(j = 0; j < 8; j++)
                edc = (edc >> 1) ^ ((edc & 1) > 0 ? 0xD8018001 : 0);

            _edcTable[i] = edc;
        }

        _initedEdc = true;
    }

    bool SuffixIsCorrect(byte[] sector)
    {
        if(!_initedEdc)
            EccInit();

        if(sector[0x814] != 0x00 || // reserved (8 bytes)
           sector[0x815] != 0x00 ||
           sector[0x816] != 0x00 ||
           sector[0x817] != 0x00 ||
           sector[0x818] != 0x00 ||
           sector[0x819] != 0x00 ||
           sector[0x81A] != 0x00 ||
           sector[0x81B] != 0x00)
            return false;

        bool correctEccP = CheckEcc(sector, sector, 86, 24, 2, 86, sector, 0xC, 0x10, 0x81C);

        if(!correctEccP)
            return false;

        bool correctEccQ = CheckEcc(sector, sector, 52, 43, 86, 88, sector, 0xC, 0x10, 0x81C + 0xAC);

        if(!correctEccQ)
            return false;

        uint storedEdc = BitConverter.ToUInt32(sector, 0x810);
        uint edc       = 0;
        int  size      = 0x810;
        int  pos       = 0;

        for(; size > 0; size--)
            edc = (edc >> 8) ^ _edcTable[(edc ^ sector[pos++]) & 0xFF];

        uint calculatedEdc = edc;

        return calculatedEdc == storedEdc;
    }

    bool SuffixIsCorrectMode2(byte[] sector)
    {
        if(!_initedEdc)
            EccInit();

        byte[] zeroAddress = new byte[4];

        bool correctEccP = CheckEcc(zeroAddress, sector, 86, 24, 2, 86, sector, 0, 0x10, 0x81C);

        if(!correctEccP)
            return false;

        bool correctEccQ = CheckEcc(zeroAddress, sector, 52, 43, 86, 88, sector, 0, 0x10, 0x81C + 0xAC);

        if(!correctEccQ)
            return false;

        uint storedEdc = BitConverter.ToUInt32(sector, 0x818);
        uint edc       = 0;
        int  size      = 0x808;
        int  pos       = 0x10;

        for(; size > 0; size--)
            edc = (edc >> 8) ^ _edcTable[(edc ^ sector[pos++]) & 0xFF];

        uint calculatedEdc = edc;

        return calculatedEdc == storedEdc;
    }

    bool CheckEcc(byte[] address, byte[] data, uint majorCount, uint minorCount, uint majorMult, uint minorInc,
                  byte[] ecc, int addressOffset, int dataOffset, int eccOffset)
    {
        uint size = majorCount * minorCount;
        uint major;

        for(major = 0; major < majorCount; major++)
        {
            uint idx  = ((major >> 1) * majorMult) + (major & 1);
            byte eccA = 0;
            byte eccB = 0;
            uint minor;

            for(minor = 0; minor < minorCount; minor++)
            {
                byte temp = idx < 4 ? address[idx + addressOffset] : data[idx + dataOffset - 4];
                idx += minorInc;

                if(idx >= size)
                    idx -= size;

                eccA ^= temp;
                eccB ^= temp;
                eccA =  _eccFTable[eccA];
            }

            eccA = _eccBTable[_eccFTable[eccA] ^ eccB];

            if(ecc[major + eccOffset]              != eccA ||
               ecc[major + majorCount + eccOffset] != (eccA ^ eccB))
                return false;
        }

        return true;
    }

    void WriteEcc(byte[] address, byte[] data, uint majorCount, uint minorCount, uint majorMult, uint minorInc,
                  ref byte[] ecc, int addressOffset, int dataOffset, int eccOffset)
    {
        uint size = majorCount * minorCount;
        uint major;

        for(major = 0; major < majorCount; major++)
        {
            uint idx  = ((major >> 1) * majorMult) + (major & 1);
            byte eccA = 0;
            byte eccB = 0;
            uint minor;

            for(minor = 0; minor < minorCount; minor++)
            {
                byte temp = idx < 4 ? address[idx + addressOffset] : data[idx + dataOffset - 4];
                idx += minorInc;

                if(idx >= size)
                    idx -= size;

                eccA ^= temp;
                eccB ^= temp;
                eccA =  _eccFTable[eccA];
            }

            eccA                                = _eccBTable[_eccFTable[eccA] ^ eccB];
            ecc[major + eccOffset]              = eccA;
            ecc[major + majorCount + eccOffset] = (byte)(eccA ^ eccB);
        }
    }

    void EccWriteSector(byte[] address, byte[] data, ref byte[] ecc, int addressOffset, int dataOffset, int eccOffset)
    {
        WriteEcc(address, data, 86, 24, 2, 86, ref ecc, addressOffset, dataOffset, eccOffset);         // P
        WriteEcc(address, data, 52, 43, 86, 88, ref ecc, addressOffset, dataOffset, eccOffset + 0xAC); // Q
    }

    static (byte minute, byte second, byte frame) LbaToMsf(long pos) =>
        ((byte)((pos + 150) / 75 / 60), (byte)((pos + 150) / 75 % 60), (byte)((pos + 150) % 75));

    static void ReconstructPrefix(ref byte[] sector, // must point to a full 2352-byte sector
                                  TrackType type, long lba)
    {
        //
        // Sync
        //
        sector[0x000] = 0x00;
        sector[0x001] = 0xFF;
        sector[0x002] = 0xFF;
        sector[0x003] = 0xFF;
        sector[0x004] = 0xFF;
        sector[0x005] = 0xFF;
        sector[0x006] = 0xFF;
        sector[0x007] = 0xFF;
        sector[0x008] = 0xFF;
        sector[0x009] = 0xFF;
        sector[0x00A] = 0xFF;
        sector[0x00B] = 0x00;

        (byte minute, byte second, byte frame) msf = LbaToMsf(lba);

        sector[0x00C] = (byte)(((msf.minute / 10) << 4) + (msf.minute % 10));
        sector[0x00D] = (byte)(((msf.second / 10) << 4) + (msf.second % 10));
        sector[0x00E] = (byte)(((msf.frame  / 10) << 4) + (msf.frame  % 10));

        switch(type)
        {
            case TrackType.CdMode1:
                //
                // Mode
                //
                sector[0x00F] = 0x01;

                break;
            case TrackType.CdMode2Form1:
            case TrackType.CdMode2Form2:
            case TrackType.CdMode2Formless:
                //
                // Mode
                //
                sector[0x00F] = 0x02;

                //
                // Flags
                //
                sector[0x010] = sector[0x014];
                sector[0x011] = sector[0x015];
                sector[0x012] = sector[0x016];
                sector[0x013] = sector[0x017];

                break;
            default: return;
        }
    }

    void ReconstructEcc(ref byte[] sector, // must point to a full 2352-byte sector
                        TrackType type)
    {
        byte[] computedEdc;

        if(!_initedEdc)
            EccInit();

        switch(type)
        {
            //
            // Compute EDC
            //
            case TrackType.CdMode1:
                computedEdc   = BitConverter.GetBytes(ComputeEdc(0, sector, 0x810));
                sector[0x810] = computedEdc[0];
                sector[0x811] = computedEdc[1];
                sector[0x812] = computedEdc[2];
                sector[0x813] = computedEdc[3];

                break;
            case TrackType.CdMode2Form1:
                computedEdc   = BitConverter.GetBytes(ComputeEdc(0, sector, 0x808, 0x10));
                sector[0x818] = computedEdc[0];
                sector[0x819] = computedEdc[1];
                sector[0x81A] = computedEdc[2];
                sector[0x81B] = computedEdc[3];

                break;
            case TrackType.CdMode2Form2:
                computedEdc   = BitConverter.GetBytes(ComputeEdc(0, sector, 0x91C, 0x10));
                sector[0x92C] = computedEdc[0];
                sector[0x92D] = computedEdc[1];
                sector[0x92E] = computedEdc[2];
                sector[0x92F] = computedEdc[3];

                break;
            default: return;
        }

        byte[] zeroAddress = new byte[4];

        switch(type)
        {
            //
            // Compute ECC
            //
            case TrackType.CdMode1:
                //
                // Reserved
                //
                sector[0x814] = 0x00;
                sector[0x815] = 0x00;
                sector[0x816] = 0x00;
                sector[0x817] = 0x00;
                sector[0x818] = 0x00;
                sector[0x819] = 0x00;
                sector[0x81A] = 0x00;
                sector[0x81B] = 0x00;
                EccWriteSector(sector, sector, ref sector, 0xC, 0x10, 0x81C);

                break;
            case TrackType.CdMode2Form1:
                EccWriteSector(zeroAddress, sector, ref sector, 0, 0x10, 0x81C);

                break;
            default: return;
        }

        //
        // Done
        //
    }

    uint ComputeEdc(uint edc, byte[] src, int size, int srcOffset = 0)
    {
        if(!_initedEdc)
            EccInit();

        int pos = srcOffset;

        for(; size > 0; size--)
            edc = (edc >> 8) ^ _edcTable[(edc ^ src[pos++]) & 0xFF];

        return edc;
    }
}