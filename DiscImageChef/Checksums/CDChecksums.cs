/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : CDChecksums.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Checksums.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Checks a CD checksum
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
ECC algorithm from ECM (C) 2002-2011 Neill Corlett
****************************************************************************/
//$Id$
using System;

namespace DiscImageChef.Checksums
{
    public static class CDChecksums
    {
        static byte[] ECC_F_Table;
        static byte[] ECC_B_Table;
        const UInt32 CDCRC32Poly = 0xD8018001;
        const UInt32 CDCRC32Seed = 0x00000000;

        public static bool? CheckCDSector(byte[] buffer)
        {
            switch (buffer.Length)
            {
                case 2448:
                    {
                        byte[] subchannel = new byte[96];
                        byte[] channel = new byte[2352];

                        Array.Copy(buffer, 0, channel, 0, 2352);
                        Array.Copy(buffer, 2352, subchannel, 0, 96);

                        bool? channelStatus = CheckCDSectorChannel(channel);
                        bool? subchannelStatus = CheckCDSectorSubChannel(subchannel);

                        if (channelStatus == null || subchannelStatus == null)
                            return null;
                        if (channelStatus == false || subchannelStatus == false)
                            return false;
                        if (channelStatus == true && subchannelStatus == true)
                            return true;

                        return null;
                    }
                case 2352:
                    return CheckCDSectorChannel(buffer);
                default:
                    return null;
            }
        }

        static void ECCInit()
        {
            ECC_F_Table = new byte[256];
            ECC_B_Table = new byte[256];

            for (UInt32 i = 0; i < 256; i++)
            {
                UInt32 j = (uint)((i << 1) ^ ((i & 0x80) == 0x80 ? 0x11D : 0));
                ECC_F_Table[i] = (byte)j;
                ECC_B_Table[i ^ j] = (byte)i;
            }
        }

        static bool CheckECC(
            byte[] address,
            byte[] data,
            UInt32 major_count,
            UInt32 minor_count,
            UInt32 major_mult,
            UInt32 minor_inc,
            byte[] ecc
        )
        {
            UInt32 size = major_count * minor_count;
            UInt32 major;
            for (major = 0; major < major_count; major++)
            {
                UInt32 index = (major >> 1) * major_mult + (major & 1);
                byte ecc_a = 0;
                byte ecc_b = 0;
                UInt32 minor;
                for (minor = 0; minor < minor_count; minor++)
                {
                    byte temp;
                    if (index < 4)
                    {
                        temp = address[index];
                    }
                    else
                    {
                        temp = data[index - 4];
                    }
                    index += minor_inc;
                    if (index >= size)
                    {
                        index -= size;
                    }
                    ecc_a ^= temp;
                    ecc_b ^= temp;
                    ecc_a = ECC_F_Table[ecc_a];
                }
                ecc_a = ECC_B_Table[ECC_F_Table[ecc_a] ^ ecc_b];
                if (
                    ecc[major] != (ecc_a) ||
                    ecc[major + major_count] != (ecc_a ^ ecc_b))
                {
                    return false;
                }
            }
            return true;
        }

        static bool? CheckCDSectorChannel(byte[] channel)
        {
            ECCInit();

            if (
                channel[0x000] == 0x00 && // sync (12 bytes)
                channel[0x001] == 0xFF &&
                channel[0x002] == 0xFF &&
                channel[0x003] == 0xFF &&
                channel[0x004] == 0xFF &&
                channel[0x005] == 0xFF &&
                channel[0x006] == 0xFF &&
                channel[0x007] == 0xFF &&
                channel[0x008] == 0xFF &&
                channel[0x009] == 0xFF &&
                channel[0x00A] == 0xFF &&
                channel[0x00B] == 0x00)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDChecksums): Data sector, address {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);

                if (channel[0x00F] == 0x00) // mode (1 byte)
                {
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (CDChecksums): Mode 0 sector at address {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);
                    for (int i = 0x010; i < 0x930; i++)
                    {
                        if (channel[i] != 0x00)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDChecksums): Mode 0 sector with error at address: {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);
                            return false;
                        }
                    }
                    return true;
                }
                else if (channel[0x00F] == 0x01) // mode (1 byte)
                {
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (CDChecksums): Mode 1 sector at address {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);

                    if (channel[0x814] != 0x00 || // reserved (8 bytes)
                        channel[0x815] != 0x00 ||
                        channel[0x816] != 0x00 ||
                        channel[0x817] != 0x00 ||
                        channel[0x818] != 0x00 ||
                        channel[0x819] != 0x00 ||
                        channel[0x81A] != 0x00 ||
                        channel[0x81B] != 0x00)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDChecksums): Mode 1 sector with data in reserved bytes at address: {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);
                        return false;
                    }

                    byte[] address = new byte[4];
                    byte[] data = new byte[2060];
                    byte[] data2 = new byte[2232];
                    byte[] ecc_p = new byte[172];
                    byte[] ecc_q = new byte[104];

                    Array.Copy(channel, 0x0C, address, 0, 4);
                    Array.Copy(channel, 0x0C, data, 0, 2060);
                    Array.Copy(channel, 0x0C, data2, 0, 2232);
                    Array.Copy(channel, 0x81C, ecc_p, 0, 172);
                    Array.Copy(channel, 0x8C8, ecc_q, 0, 104);

                    bool FailedECC_P = CheckECC(address, data, 86, 24, 2, 86, ecc_p);
                    bool FailedECC_Q = CheckECC(address, data2, 52, 43, 86, 88, ecc_q);

                    if (FailedECC_P)
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (CDChecksums): Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC P check", channel[0x00C], channel[0x00D], channel[0x00E]);
                    if (FailedECC_Q)
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (CDChecksums): Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC Q check", channel[0x00C], channel[0x00D], channel[0x00E]);

                    if (FailedECC_P || FailedECC_Q)
                        return false;

                    byte[] SectorForCheck = new byte[0x810];
                    UInt32 StoredEDC = BitConverter.ToUInt32(channel, 0x810);
                    byte[] CalculatedEDCBytes;
                    Array.Copy(channel, 0, SectorForCheck, 0, 0x810);
                    CRC32Context.Data(SectorForCheck, 0x810, out CalculatedEDCBytes, CDCRC32Poly, CDCRC32Seed);
                    UInt32 CalculatedEDC = BitConverter.ToUInt32(CalculatedEDCBytes, 0);

                    if (CalculatedEDC != StoredEDC)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDChecksums): Mode 1 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}", channel[0x00C], channel[0x00D], channel[0x00E], CalculatedEDC, StoredEDC);
                        return false;
                    }

                    return true;
                }
                else if (channel[0x00F] == 0x02) // mode (1 byte)
                {
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (CDChecksums): Mode 2 sector at address {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);

                    if ((channel[0x012] & 0x20) == 0x20) // mode 2 form 2
                    {
                        if (channel[0x010] != channel[0x014] || channel[0x011] != channel[0x015] || channel[0x012] != channel[0x016] || channel[0x013] != channel[0x017])
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDChecksums): Subheader copies differ in mode 2 form 2 sector at address: {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);
                        }

                        byte[] SectorForCheck = new byte[0x91C];
                        UInt32 StoredEDC = BitConverter.ToUInt32(channel, 0x92C);
                        byte[] CalculatedEDCBytes;
                        Array.Copy(channel, 0x10, SectorForCheck, 0, 0x91C);
                        CRC32Context.Data(SectorForCheck, 0x91C, out CalculatedEDCBytes, CDCRC32Poly, CDCRC32Seed);
                        UInt32 CalculatedEDC = BitConverter.ToUInt32(CalculatedEDCBytes, 0);

                        if (CalculatedEDC != StoredEDC && StoredEDC != 0x00000000)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDChecksums): Mode 2 form 2 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}", channel[0x00C], channel[0x00D], channel[0x00E], CalculatedEDC, StoredEDC);
                            return false;
                        }
                    }
                    else
                    {
                        if (channel[0x010] != channel[0x014] || channel[0x011] != channel[0x015] || channel[0x012] != channel[0x016] || channel[0x013] != channel[0x017])
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDChecksums): Subheader copies differ in mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}", channel[0x00C], channel[0x00D], channel[0x00E]);
                        }

                        byte[] address = new byte[4];
                        byte[] data = new byte[2060];
                        byte[] data2 = new byte[2232];
                        byte[] ecc_p = new byte[172];
                        byte[] ecc_q = new byte[104];

                        address[0] = 0;
                        address[1] = 0;
                        address[2] = 0;
                        address[3] = 0;
                        Array.Copy(channel, 0x0C, data, 0, 2060);
                        Array.Copy(channel, 0x0C, data2, 0, 2232);
                        Array.Copy(channel, 0x80C, ecc_p, 0, 172);
                        Array.Copy(channel, 0x8B8, ecc_q, 0, 104);

                        bool FailedECC_P = CheckECC(address, data, 86, 24, 2, 86, ecc_p);
                        bool FailedECC_Q = CheckECC(address, data2, 52, 43, 86, 88, ecc_q);

                        if (FailedECC_P)
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDChecksums): Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC P check", channel[0x00C], channel[0x00D], channel[0x00E]);
                        if (FailedECC_Q)
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDChecksums): Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, fails ECC Q check", channel[0x00F], channel[0x00C], channel[0x00D], channel[0x00E]);

                        if (FailedECC_P || FailedECC_Q)
                            return false;

                        byte[] SectorForCheck = new byte[0x808];
                        UInt32 StoredEDC = BitConverter.ToUInt32(channel, 0x818);
                        byte[] CalculatedEDCBytes;
                        Array.Copy(channel, 0x10, SectorForCheck, 0, 0x808);
                        CRC32Context.Data(SectorForCheck, 0x808, out CalculatedEDCBytes, CDCRC32Poly, CDCRC32Seed);
                        UInt32 CalculatedEDC = BitConverter.ToUInt32(CalculatedEDCBytes, 0);

                        if (CalculatedEDC != StoredEDC)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDChecksums): Mode 2 form 1 sector at address: {0:X2}:{1:X2}:{2:X2}, got CRC 0x{3:X8} expected 0x{4:X8}", channel[0x00C], channel[0x00D], channel[0x00E], CalculatedEDC, StoredEDC);
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (CDChecksums): Unknown mode {0} sector at address: {1:X2}:{2:X2}:{3:X2}", channel[0x00F], channel[0x00C], channel[0x00D], channel[0x00E]);
                    return null;
                }
            }
            else
                return null;
        }

        static bool? CheckCDSectorSubChannel(byte[] subchannel)
        {
            bool? status = true;
            byte[] QSubChannel = new byte[12];
            byte[] CDTextPack1 = new byte[18];
            byte[] CDTextPack2 = new byte[18];
            byte[] CDTextPack3 = new byte[18];
            byte[] CDTextPack4 = new byte[18];

            int i = 0;
            for (int j = 0; j < 12; j++)
                QSubChannel[j] = 0;
            for (int j = 0; j < 18; j++)
            {
                CDTextPack1[j] = 0;
                CDTextPack2[j] = 0;
                CDTextPack3[j] = 0;
                CDTextPack4[j] = 0;
            }

            for (int j = 0; j < 12; j++)
            {
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) << 1));
                QSubChannel[j] = (byte)(QSubChannel[j] | (subchannel[i++] & 0x40));
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) >> 1));
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) >> 2));
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) >> 3));
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) >> 4));
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) >> 5));
                QSubChannel[j] = (byte)(QSubChannel[j] | ((subchannel[i++] & 0x40) >> 6));
            }

            i = 0;
            for (int j = 0; j < 18; j++)
            {
                if (j < 18)
                    CDTextPack1[j] = (byte)(CDTextPack1[j] | ((subchannel[i++] & 0x3F) << 2));
                if (j < 18)
                    CDTextPack1[j] = (byte)(CDTextPack1[j++] | ((subchannel[i] & 0xC0) >> 4));
                if (j < 18)
                    CDTextPack1[j] = (byte)(CDTextPack1[j] | ((subchannel[i++] & 0x0F) << 4));
                if (j < 18)
                    CDTextPack1[j] = (byte)(CDTextPack1[j++] | ((subchannel[i] & 0x3C) >> 2));
                if (j < 18)
                    CDTextPack1[j] = (byte)(CDTextPack1[j] | ((subchannel[i++] & 0x03) << 6));
                if (j < 18)
                    CDTextPack1[j] = (byte)(CDTextPack1[j] | (subchannel[i++] & 0x3F));
            }
            for (int j = 0; j < 18; j++)
            {
                if (j < 18)
                    CDTextPack2[j] = (byte)(CDTextPack2[j] | ((subchannel[i++] & 0x3F) << 2));
                if (j < 18)
                    CDTextPack2[j] = (byte)(CDTextPack2[j++] | ((subchannel[i] & 0xC0) >> 4));
                if (j < 18)
                    CDTextPack2[j] = (byte)(CDTextPack2[j] | ((subchannel[i++] & 0x0F) << 4));
                if (j < 18)
                    CDTextPack2[j] = (byte)(CDTextPack2[j++] | ((subchannel[i] & 0x3C) >> 2));
                if (j < 18)
                    CDTextPack2[j] = (byte)(CDTextPack2[j] | ((subchannel[i++] & 0x03) << 6));
                if (j < 18)
                    CDTextPack2[j] = (byte)(CDTextPack2[j] | (subchannel[i++] & 0x3F));
            }
            for (int j = 0; j < 18; j++)
            {
                if (j < 18)
                    CDTextPack3[j] = (byte)(CDTextPack3[j] | ((subchannel[i++] & 0x3F) << 2));
                if (j < 18)
                    CDTextPack3[j] = (byte)(CDTextPack3[j++] | ((subchannel[i] & 0xC0) >> 4));
                if (j < 18)
                    CDTextPack3[j] = (byte)(CDTextPack3[j] | ((subchannel[i++] & 0x0F) << 4));
                if (j < 18)
                    CDTextPack3[j] = (byte)(CDTextPack3[j++] | ((subchannel[i] & 0x3C) >> 2));
                if (j < 18)
                    CDTextPack3[j] = (byte)(CDTextPack3[j] | ((subchannel[i++] & 0x03) << 6));
                if (j < 18)
                    CDTextPack3[j] = (byte)(CDTextPack3[j] | (subchannel[i++] & 0x3F));
            }
            for (int j = 0; j < 18; j++)
            {
                if (j < 18)
                    CDTextPack4[j] = (byte)(CDTextPack4[j] | ((subchannel[i++] & 0x3F) << 2));
                if (j < 18)
                    CDTextPack4[j] = (byte)(CDTextPack4[j++] | ((subchannel[i] & 0xC0) >> 4));
                if (j < 18)
                    CDTextPack4[j] = (byte)(CDTextPack4[j] | ((subchannel[i++] & 0x0F) << 4));
                if (j < 18)
                    CDTextPack4[j] = (byte)(CDTextPack4[j++] | ((subchannel[i] & 0x3C) >> 2));
                if (j < 18)
                    CDTextPack4[j] = (byte)(CDTextPack4[j] | ((subchannel[i++] & 0x03) << 6));
                if (j < 18)
                    CDTextPack4[j] = (byte)(CDTextPack4[j] | (subchannel[i++] & 0x3F));
            }

            UInt16 QSubChannelCRC = BitConverter.ToUInt16(QSubChannel, 10);
            byte[] QSubChannelForCRC = new byte[10];
            Array.Copy(QSubChannel, 0, QSubChannelForCRC, 0, 10);
            byte[] CalculatedQCRCBytes = new byte[2];
            CRC16Context.Data(QSubChannelForCRC, out CalculatedQCRCBytes);
            UInt16 CalculatedQCRC = BitConverter.ToUInt16(CalculatedQCRCBytes, 0);

            if (QSubChannelCRC != CalculatedQCRC)
            {
                if (MainClass.isDebug)
//                    Console.WriteLine("DEBUG (CDChecksums): Q subchannel CRC 0x{0:X4}, expected 0x{1:X4}", CalculatedQCRC, QSubChannelCRC);
                status = false;
            }

            UInt16 CDTextPack1CRC = BitConverter.ToUInt16(CDTextPack1, 16);
            byte[] CDTextPack1ForCRC = new byte[16];
            Array.Copy(CDTextPack1, 0, CDTextPack1ForCRC, 0, 16);
            byte[] CalculatedCDTP1CRCBytes = new byte[2];
            CRC16Context.Data(CDTextPack1ForCRC, out CalculatedCDTP1CRCBytes);
            UInt16 CalculatedCDTP1CRC = BitConverter.ToUInt16(CalculatedCDTP1CRCBytes, 0);

            if (CDTextPack1CRC != CalculatedCDTP1CRC && CDTextPack1CRC != 0)
            {
                if (MainClass.isDebug)
//                    Console.WriteLine("DEBUG (CDChecksums): CD-Text Pack 1 CRC 0x{0:X4}, expected 0x{1:X4}", CDTextPack1CRC, CalculatedCDTP1CRC);
                status = false;
            }

            UInt16 CDTextPack2CRC = BitConverter.ToUInt16(CDTextPack2, 16);
            byte[] CDTextPack2ForCRC = new byte[16];
            Array.Copy(CDTextPack2, 0, CDTextPack2ForCRC, 0, 16);
            byte[] CalculatedCDTP2CRCBytes = new byte[2];
            CRC16Context.Data(CDTextPack2ForCRC, out CalculatedCDTP2CRCBytes);
            UInt16 CalculatedCDTP2CRC = BitConverter.ToUInt16(CalculatedCDTP2CRCBytes, 0);

            if (CDTextPack2CRC != CalculatedCDTP2CRC && CDTextPack2CRC != 0)
            {
                if (MainClass.isDebug)
//                    Console.WriteLine("DEBUG (CDChecksums): CD-Text Pack 2 CRC 0x{0:X4}, expected 0x{1:X4}", CDTextPack2CRC, CalculatedCDTP2CRC);
                status = false;
            }

            UInt16 CDTextPack3CRC = BitConverter.ToUInt16(CDTextPack3, 16);
            byte[] CDTextPack3ForCRC = new byte[16];
            Array.Copy(CDTextPack3, 0, CDTextPack3ForCRC, 0, 16);
            byte[] CalculatedCDTP3CRCBytes = new byte[2];
            CRC16Context.Data(CDTextPack3ForCRC, out CalculatedCDTP3CRCBytes);
            UInt16 CalculatedCDTP3CRC = BitConverter.ToUInt16(CalculatedCDTP3CRCBytes, 0);

            if (CDTextPack3CRC != CalculatedCDTP3CRC && CDTextPack3CRC != 0)
            {
                if (MainClass.isDebug)
//                    Console.WriteLine("DEBUG (CDChecksums): CD-Text Pack 3 CRC 0x{0:X4}, expected 0x{1:X4}", CDTextPack3CRC, CalculatedCDTP3CRC);
                status = false;
            }

            UInt16 CDTextPack4CRC = BitConverter.ToUInt16(CDTextPack4, 16);
            byte[] CDTextPack4ForCRC = new byte[16];
            Array.Copy(CDTextPack4, 0, CDTextPack4ForCRC, 0, 16);
            byte[] CalculatedCDTP4CRCBytes = new byte[2];
            CRC16Context.Data(CDTextPack4ForCRC, out CalculatedCDTP4CRCBytes);
            UInt16 CalculatedCDTP4CRC = BitConverter.ToUInt16(CalculatedCDTP4CRCBytes, 0);

            if (CDTextPack4CRC != CalculatedCDTP4CRC && CDTextPack4CRC != 0)
            {
                if (MainClass.isDebug)
//                    Console.WriteLine("DEBUG (CDChecksums): CD-Text Pack 4 CRC 0x{0:X4}, expected 0x{1:X4}", CDTextPack4CRC, CalculatedCDTP4CRC);
                status = false;
            }

//            return status;
            // TODO: Correct CRC poly and seed
            // TODO: Detect CD-Text vs CD+G packets
            return null;
        }
    }
}

