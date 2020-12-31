// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SectorBuilder.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;

namespace Aaru.Decoders.CD
{
    public class SectorBuilder
    {
        readonly byte[] _eccBTable;
        readonly byte[] _eccFTable;
        readonly uint[] _edcTable;

        public SectorBuilder()
        {
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
        }

        static (byte minute, byte second, byte frame) LbaToMsf(long pos) =>
            ((byte)((pos + 150) / 75 / 60), (byte)(((pos + 150) / 75) % 60), (byte)((pos + 150) % 75));

        public void ReconstructPrefix(ref byte[] sector, // must point to a full 2352-byte sector
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

        uint ComputeEdc(uint edc, byte[] src, int size, int srcOffset = 0)
        {
            int pos = srcOffset;

            for(; size > 0; size--)
                edc = (edc >> 8) ^ _edcTable[(edc ^ src[pos++]) & 0xFF];

            return edc;
        }

        public void ReconstructEcc(ref byte[] sector, // must point to a full 2352-byte sector
                                   TrackType type)
        {
            byte[] computedEdc;

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

            byte[] zeroaddress = new byte[4];

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
                    EccWriteSector(zeroaddress, sector, ref sector, 0, 0x10, 0x81C);

                    break;
                default: return;
            }

            //
            // Done
            //
        }

        void EccWriteSector(byte[] address, byte[] data, ref byte[] ecc, int addressOffset, int dataOffset,
                            int eccOffset)
        {
            WriteEcc(address, data, 86, 24, 2, 86, ref ecc, addressOffset, dataOffset, eccOffset);         // P
            WriteEcc(address, data, 52, 43, 86, 88, ref ecc, addressOffset, dataOffset, eccOffset + 0xAC); // Q
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
                    byte temp = idx < 4 ? address[idx + addressOffset] : data[(idx + dataOffset) - 4];
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
    }
}