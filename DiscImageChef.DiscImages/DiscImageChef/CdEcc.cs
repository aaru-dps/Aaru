// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ECC algorithm from ECM(c) 2002-2011 Neill Corlett
// ****************************************************************************/

using System;

namespace DiscImageChef.DiscImages
{
    public partial class DiscImageChef
    {
        byte[] eccBTable;
        byte[] eccFTable;
        uint[] edcTable;

        void EccInit()
        {
            eccFTable = new byte[256];
            eccBTable = new byte[256];
            edcTable  = new uint[256];

            for(uint i = 0; i < 256; i++)
            {
                uint edc = i;
                uint j   = (uint)((i << 1) ^ ((i & 0x80) == 0x80 ? 0x11D : 0));
                eccFTable[i]     = (byte)j;
                eccBTable[i ^ j] = (byte)i;
                for(j = 0; j < 8; j++) edc = (edc >> 1) ^ ((edc & 1) > 0 ? 0xD8018001 : 0);
                edcTable[i] = edc;
            }
        }

        bool SuffixIsCorrect(byte[] channel)
        {
            if(channel[0x814] != 0x00 || // reserved (8 bytes)
               channel[0x815] != 0x00 || channel[0x816] != 0x00 || channel[0x817] != 0x00 || channel[0x818] != 0x00 ||
               channel[0x819] != 0x00 || channel[0x81A] != 0x00 || channel[0x81B] != 0x00) return false;

            byte[] address = new byte[4];
            byte[] data    = new byte[2060];
            byte[] data2   = new byte[2232];
            byte[] eccP    = new byte[172];
            byte[] eccQ    = new byte[104];

            Array.Copy(channel, 0x0C,  address, 0, 4);
            Array.Copy(channel, 0x10,  data,    0, 2060);
            Array.Copy(channel, 0x10,  data2,   0, 2232);
            Array.Copy(channel, 0x81C, eccP,    0, 172);
            Array.Copy(channel, 0x8C8, eccQ,    0, 104);

            bool correctEccP = CheckEcc(ref address, ref data, 86, 24, 2, 86, ref eccP);
            if(!correctEccP) return false;

            bool correctEccQ = CheckEcc(ref address, ref data2, 52, 43, 86, 88, ref eccQ);
            if(!correctEccQ) return false;

            uint storedEdc              = BitConverter.ToUInt32(channel, 0x810);
            uint edc                    = 0;
            int  size                   = 0x810;
            int  pos                    = 0;
            for(; size > 0; size--) edc = (edc >> 8) ^ edcTable[(edc ^ channel[pos++]) & 0xFF];
            uint calculatedEdc          = edc;

            return calculatedEdc == storedEdc;
        }

        bool CheckEcc(ref byte[] address,  ref byte[] data, uint majorCount, uint minorCount, uint majorMult,
                      uint       minorInc, ref byte[] ecc)
        {
            uint size = majorCount * minorCount;
            uint major;
            for(major = 0; major < majorCount; major++)
            {
                uint idx  = (major >> 1) * majorMult + (major & 1);
                byte eccA = 0;
                byte eccB = 0;
                uint minor;
                for(minor = 0; minor < minorCount; minor++)
                {
                    byte temp = idx < 4 ? address[idx] : data[idx - 4];
                    idx += minorInc;
                    if(idx >= size) idx -= size;
                    eccA ^= temp;
                    eccB ^= temp;
                    eccA =  eccFTable[eccA];
                }

                eccA = eccBTable[eccFTable[eccA] ^ eccB];
                if(ecc[major] != eccA || ecc[major + majorCount] != (eccA ^ eccB)) return false;
            }

            return true;
        }
    }
}