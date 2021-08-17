// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Subchannel.cs
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
using Aaru.Checksums;

namespace Aaru.Decoders.CD
{
    public static class Subchannel
    {
        static readonly string[] _isrcTable =
        {
            // 0x00
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "", "", "", "", "", "",

            // 0x10
            "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",

            // 0x20
            "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "", "", "", "", "",

            // 0x30
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""
        };

        public static void BinaryToBcdQ(byte[] q)
        {
            if((q[0] & 0xF) == 1 ||
               (q[0] & 0xF) == 5)
            {
                q[1] = (byte)(((q[1] / 10) << 4) + (q[1] % 10));
                q[2] = (byte)(((q[2] / 10) << 4) + (q[2] % 10));
                q[3] = (byte)(((q[3] / 10) << 4) + (q[3] % 10));
                q[4] = (byte)(((q[4] / 10) << 4) + (q[4] % 10));
                q[5] = (byte)(((q[5] / 10) << 4) + (q[5] % 10));
                q[6] = (byte)(((q[6] / 10) << 4) + (q[6] % 10));
                q[7] = (byte)(((q[7] / 10) << 4) + (q[7] % 10));
                q[8] = (byte)(((q[8] / 10) << 4) + (q[8] % 10));
            }

            q[9] = (byte)(((q[9] / 10) << 4) + (q[9] % 10));
        }

        public static void BcdToBinaryQ(byte[] q)
        {
            if((q[0] & 0xF) == 1 ||
               (q[0] & 0xF) == 5)
            {
                q[1] = (byte)((q[1] / 16 * 10) + (q[1] & 0x0F));
                q[2] = (byte)((q[2] / 16 * 10) + (q[2] & 0x0F));
                q[3] = (byte)((q[3] / 16 * 10) + (q[3] & 0x0F));
                q[4] = (byte)((q[4] / 16 * 10) + (q[4] & 0x0F));
                q[5] = (byte)((q[5] / 16 * 10) + (q[5] & 0x0F));
                q[6] = (byte)((q[6] / 16 * 10) + (q[6] & 0x0F));
                q[7] = (byte)((q[7] / 16 * 10) + (q[7] & 0x0F));
                q[8] = (byte)((q[8] / 16 * 10) + (q[8] & 0x0F));
            }

            q[9] = (byte)((q[9] / 16 * 10) + (q[9] & 0x0F));
        }

        public static byte[] ConvertQToRaw(byte[] subchannel)
        {
            int    pos    = 0;
            byte[] subBuf = new byte[subchannel.Length * 6];

            for(int i = 0; i < subchannel.Length; i += 16)
            {
                // P
                if((subchannel[i + 15] & 0x80) <= 0)
                {
                    pos += 12;
                }
                else
                {
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                    subBuf[pos++] = 0xFF;
                }

                // Q
                subBuf[pos++] = subchannel[i + 0];
                subBuf[pos++] = subchannel[i + 1];
                subBuf[pos++] = subchannel[i + 2];
                subBuf[pos++] = subchannel[i + 3];
                subBuf[pos++] = subchannel[i + 4];
                subBuf[pos++] = subchannel[i + 5];
                subBuf[pos++] = subchannel[i + 6];
                subBuf[pos++] = subchannel[i + 7];
                subBuf[pos++] = subchannel[i + 8];
                subBuf[pos++] = subchannel[i + 9];
                subBuf[pos++] = subchannel[i + 10];
                subBuf[pos++] = subchannel[i + 11];

                // R to W
                pos += 72;
            }

            return Interleave(subBuf);
        }

        public static byte[] Interleave(byte[] subchannel)
        {
            byte[] subBuf = new byte[subchannel.Length];

            int outPos = 0;

            for(int inPos = 0; inPos < subchannel.Length; inPos += 96)
            {
                for(int i = 0; i < 12; i++)
                {
                    // P
                    subBuf[outPos + 0] += (byte)(subchannel[inPos + i + 0] & 0x80);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 0] & 0x40) << 1);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 0] & 0x20) << 2);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 0] & 0x10) << 3);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 0] & 0x08) << 4);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 0] & 0x04) << 5);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 0] & 0x02) << 6);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 0] & 0x01) << 7);

                    // Q
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 12] & 0x80) >> 1);
                    subBuf[outPos + 1] += (byte)(subchannel[inPos + i + 12] & 0x40);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 12] & 0x20) << 1);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 12] & 0x10) << 2);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 12] & 0x08) << 3);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 12] & 0x04) << 4);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 12] & 0x02) << 5);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 12] & 0x01) << 6);

                    // R
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 24] & 0x80) >> 2);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 24] & 0x40) >> 1);
                    subBuf[outPos + 2] += (byte)(subchannel[inPos + i + 24] & 0x20);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 24] & 0x10) << 1);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 24] & 0x08) << 2);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 24] & 0x04) << 3);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 24] & 0x02) << 4);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 24] & 0x01) << 5);

                    // S
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 36] & 0x80) >> 3);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 36] & 0x40) >> 2);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 36] & 0x20) >> 1);
                    subBuf[outPos + 3] += (byte)(subchannel[inPos + i + 36] & 0x10);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 36] & 0x08) << 1);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 36] & 0x04) << 2);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 36] & 0x02) << 3);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 36] & 0x01) << 4);

                    // T
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 48] & 0x80) >> 4);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 48] & 0x40) >> 3);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 48] & 0x20) >> 2);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 48] & 0x10) >> 1);
                    subBuf[outPos + 4] += (byte)(subchannel[inPos + i + 48] & 0x08);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 48] & 0x04) << 1);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 48] & 0x02) << 2);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 48] & 0x01) << 3);

                    // U
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 60] & 0x80) >> 5);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 60] & 0x40) >> 4);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 60] & 0x20) >> 3);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 60] & 0x10) >> 2);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 60] & 0x08) >> 1);
                    subBuf[outPos + 5] += (byte)(subchannel[inPos + i + 60] & 0x04);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 60] & 0x02) << 1);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 60] & 0x01) << 2);

                    // V
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 72] & 0x80) >> 6);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 72] & 0x40) >> 5);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 72] & 0x20) >> 4);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 72] & 0x10) >> 3);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 72] & 0x08) >> 2);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 72] & 0x04) >> 1);
                    subBuf[outPos + 6] += (byte)(subchannel[inPos + i + 72] & 0x02);
                    subBuf[outPos + 7] += (byte)((subchannel[inPos + i + 72] & 0x01) << 1);

                    // W
                    subBuf[outPos + 0] += (byte)((subchannel[inPos + i + 84] & 0x80) >> 7);
                    subBuf[outPos + 1] += (byte)((subchannel[inPos + i + 84] & 0x40) >> 6);
                    subBuf[outPos + 2] += (byte)((subchannel[inPos + i + 84] & 0x20) >> 5);
                    subBuf[outPos + 3] += (byte)((subchannel[inPos + i + 84] & 0x10) >> 4);
                    subBuf[outPos + 4] += (byte)((subchannel[inPos + i + 84] & 0x08) >> 3);
                    subBuf[outPos + 5] += (byte)((subchannel[inPos + i + 84] & 0x04) >> 2);
                    subBuf[outPos + 6] += (byte)((subchannel[inPos + i + 84] & 0x02) >> 1);
                    subBuf[outPos + 7] += (byte)(subchannel[inPos + i + 84] & 0x01);
                    outPos             += 8;
                }
            }

            return subBuf;
        }

        public static byte[] Deinterleave(byte[] subchannel)
        {
            byte[] subBuf = new byte[subchannel.Length];
            int    inPos  = 0;

            for(int outPos = 0; outPos < subchannel.Length; outPos += 96)
            {
                for(int i = 0; i < 12; i++)
                {
                    // P
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 0] & 0x80) >> 0);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 1] & 0x80) >> 1);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 2] & 0x80) >> 2);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 3] & 0x80) >> 3);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 4] & 0x80) >> 4);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 5] & 0x80) >> 5);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 6] & 0x80) >> 6);
                    subBuf[outPos + i + 0] += (byte)((subchannel[inPos + 7] & 0x80) >> 7);

                    // Q
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 0] & 0x40) << 1);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 1] & 0x40) >> 0);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 2] & 0x40) >> 1);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 3] & 0x40) >> 2);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 4] & 0x40) >> 3);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 5] & 0x40) >> 4);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 6] & 0x40) >> 5);
                    subBuf[outPos + i + 12] += (byte)((subchannel[inPos + 7] & 0x40) >> 6);

                    // R
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 0] & 0x20) << 2);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 1] & 0x20) << 1);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 2] & 0x20) >> 0);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 3] & 0x20) >> 1);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 4] & 0x20) >> 2);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 5] & 0x20) >> 3);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 6] & 0x20) >> 4);
                    subBuf[outPos + i + 24] += (byte)((subchannel[inPos + 7] & 0x20) >> 5);

                    // S
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 0] & 0x10) << 3);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 1] & 0x10) << 2);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 2] & 0x10) << 1);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 3] & 0x10) >> 0);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 4] & 0x10) >> 1);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 5] & 0x10) >> 2);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 6] & 0x10) >> 3);
                    subBuf[outPos + i + 36] += (byte)((subchannel[inPos + 7] & 0x10) >> 4);

                    // T
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 0] & 0x8) << 4);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 1] & 0x8) << 3);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 2] & 0x8) << 2);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 3] & 0x8) << 1);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 4] & 0x8) >> 0);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 5] & 0x8) >> 1);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 6] & 0x8) >> 2);
                    subBuf[outPos + i + 48] += (byte)((subchannel[inPos + 7] & 0x8) >> 3);

                    // U
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 0] & 0x4) << 5);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 1] & 0x4) << 4);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 2] & 0x4) << 3);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 3] & 0x4) << 2);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 4] & 0x4) << 1);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 5] & 0x4) >> 0);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 6] & 0x4) >> 1);
                    subBuf[outPos + i + 60] += (byte)((subchannel[inPos + 7] & 0x4) >> 2);

                    // V
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 0] & 0x2) << 6);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 1] & 0x2) << 5);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 2] & 0x2) << 4);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 3] & 0x2) << 3);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 4] & 0x2) << 2);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 5] & 0x2) << 1);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 6] & 0x2) >> 0);
                    subBuf[outPos + i + 72] += (byte)((subchannel[inPos + 7] & 0x2) >> 1);

                    // W
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 0] & 0x1) << 7);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 1] & 0x1) << 6);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 2] & 0x1) << 5);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 3] & 0x1) << 4);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 4] & 0x1) << 3);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 5] & 0x1) << 2);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 6] & 0x1) << 1);
                    subBuf[outPos + i + 84] += (byte)((subchannel[inPos + 7] & 0x1) >> 0);

                    inPos += 8;
                }
            }

            return subBuf;
        }

        public static string PrettifyQ(byte[] subBuf, bool bcd, long lba, bool corruptedPause, bool pause, bool rwEmpty)
        {
            CRC16CCITTContext.Data(subBuf, 10, out byte[] crc);

            bool   crcOk  = crc[0] == subBuf[10] && crc[1] == subBuf[11];
            long   minute = (lba + 150)        / 4500;
            long   second = (lba + 150) % 4500 / 75;
            long   frame  = (lba + 150)        % 4500 % 75;
            string area;
            int    control = (subBuf[0] & 0xF0) / 16;
            int    adr     = subBuf[0] & 0x0F;

            string controlInfo = ((control & 0xC) / 4) switch
            {
                0 => $"stereo audio {((control       & 0x01) == 1 ? "with" : "without")} pre-emphasis",
                1 => $"{((control                    & 0x01) == 1 ? "incremental" : "uninterrupted")} data",
                2 => $"quadraphonic audio {((control & 0x01) == 1 ? "with" : "without")} pre-emphasis",
                _ => $"reserved control value {control & 0x01}"
            };

            string copy = (control & 0x02) > 0 ? "copy permitted" : "copy prohibited";

            if(bcd)
                BcdToBinaryQ(subBuf);

            int  qPos = (subBuf[3] * 60 * 75) + (subBuf[4] * 75) + subBuf[5] - 150;
            byte pmin = subBuf[7];
            byte psec = subBuf[8];

            int  qStart  = (subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9] - 150;
            int  nextPos = (subBuf[3] * 60 * 75) + (subBuf[4] * 75) + subBuf[5] - 150;
            byte zero    = subBuf[6];
            int  maxOut  = (subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9] - 150;
            bool final   = subBuf[3] == 0xFF && subBuf[4] == 0xFF && subBuf[5] == 0xFF;

            BinaryToBcdQ(subBuf);

            if(lba < 0)
            {
                area = "Lead-In";

                switch(adr)
                {
                    case 1 when subBuf[2] < 0xA0:
                        return
                            $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} position: {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {qPos}), track {subBuf[2]:X} starts at {subBuf[7]:X2}:{subBuf[8]:X2}:{subBuf[9]:X2} (LBA {qStart}), Q CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                    case 1 when subBuf[2] == 0xA0:
                    {
                        string format = subBuf[8] switch
                        {
                            0x00 => "CD-DA / CD-ROM",
                            0x10 => "CD-i",
                            0x20 => "CD-ROM XA",
                            _    => $"unknown {subBuf[0]:X2}"
                        };

                        return
                            $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} position: {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {qPos}), track {subBuf[2]:X} is first program area track in {format} format, Q CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                    }
                    case 1 when subBuf[2] == 0xA1:
                        return
                            $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} position: {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {qPos}), track {subBuf[2]:X} is last program area track, Q CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                    case 1:
                        return subBuf[2] == 0xA2
                                   ? $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} position: {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {qPos}), track {subBuf[2]:X} starts at {subBuf[7]:X2}{subBuf[8]:X2}{subBuf[9]:X2} (LBA {qStart}), Q CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}"
                                   : $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q: {subBuf[0]:X2} {subBuf[1]:X2} {subBuf[2]:X2} {subBuf[3]:X2} {subBuf[4]:X2} {subBuf[5]:X2} {subBuf[6]:X2} {subBuf[7]:X2} {subBuf[8]:X2} {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                    case 2:
                        return
                            $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} MCN: {DecodeMcn(subBuf)} frame {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                }

                if(adr != 5)
                    return
                        $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q: {subBuf[0]:X2} {subBuf[1]:X2} {subBuf[2]:X2} {subBuf[3]:X2} {subBuf[4]:X2} {subBuf[5]:X2} {subBuf[6]:X2} {subBuf[7]:X2} {subBuf[8]:X2} {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";

                if(subBuf[2] <= 0x40)
                {
                    return
                        $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} skip interval start time {subBuf[7]:X2}{subBuf[8]:X2}{subBuf[9]:X2}, skip interval stop time {subBuf[3]:X2}{subBuf[4]:X2}{subBuf[5]:X2}, CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                }

                if(subBuf[2] == 0xB0)
                {
                    return final
                               ? $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} next program area can start at {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {nextPos}), last-session, {zero} mode 5 pointers, CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}"
                               : $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} next program area can start at {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {nextPos}), maximum Lead-out at {subBuf[7]:X2}:{subBuf[8]:X2}:{subBuf[9]:X2} (LBA {maxOut}), {zero} mode 5 pointers, CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                }

                if(subBuf[2] == 0xB1)
                {
                    return
                        $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr}, {pmin} skip interval pointers, {psec} skip track assignments, CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
                }

                if(subBuf[2] != 0xB2 &&
                   subBuf[2] != 0xB3 &&
                   subBuf[2] != 0xB4)
                    return subBuf[2] == 0xC0
                               ? $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr}, ATIP values {subBuf[3]:X2}, {subBuf[4]:X2}, {subBuf[5]:X2}, first disc Lead-in starts at {subBuf[7]:X2}{subBuf[8]:X2}{subBuf[9]:X2} (LBA {qStart}), CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}"
                               : $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q: {subBuf[0]:X2} {subBuf[1]:X2} {subBuf[2]:X2} {subBuf[3]:X2} {subBuf[4]:X2} {subBuf[5]:X2} {subBuf[6]:X2} {subBuf[7]:X2} {subBuf[8]:X2} {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";

                string skipTracks = $"{subBuf[3]:X2}";

                if(subBuf[4] > 0)
                    skipTracks += $", {subBuf[4]:X2}";

                if(subBuf[5] > 0)
                    skipTracks += $", {subBuf[4]:X2}";

                if(subBuf[7] > 0)
                    skipTracks += $", {subBuf[4]:X2}";

                if(subBuf[8] > 0)
                    skipTracks += $", {subBuf[4]:X2}";

                if(subBuf[9] > 0)
                    skipTracks += $", {subBuf[4]:X2}";

                return
                    $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr}, tracks {skipTracks} to be skipped, CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
            }

            area = subBuf[1] == 0xAA ? "Lead-out" : "Program";

            return adr switch
            {
                1 =>
                    $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} position: track {subBuf[1]:X} index {subBuf[2]:X} relative position {subBuf[3]:X2}:{subBuf[4]:X2}:{subBuf[5]:X2} (LBA {qPos + 150}), absolute position {subBuf[7]:X2}:{subBuf[8]:X2}:{subBuf[9]:X2} (LBA {qStart}), Q CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}",
                2 =>
                    $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} MCN: {DecodeMcn(subBuf)} frame {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}",
                3 =>
                    $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} ISRC: {DecodeIsrc(subBuf)} frame {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}",
                _ =>
                    $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q: {subBuf[0]:X2} {subBuf[1]:X2} {subBuf[2]:X2} {subBuf[3]:X2} {subBuf[4]:X2} {subBuf[5]:X2} {subBuf[6]:X2} {subBuf[7]:X2} {subBuf[8]:X2} {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}"
            };
        }

        public static string DecodeIsrc(byte[] q) =>
            $"{_isrcTable[q[1] / 4]}{_isrcTable[((q[1] & 3) * 16) + (q[2] / 16)]}{_isrcTable[((q[2] & 0xF) * 4) + (q[3] / 64)]}{_isrcTable[q[3] & 0x3F]}{_isrcTable[q[4] / 4]}{q[5]:X2}{q[6]:X2}{q[7]:X2}{q[8] / 16:X1}";

        public static string DecodeMcn(byte[] q) =>
            $"{q[1]:X2}{q[2]:X2}{q[3]:X2}{q[4]:X2}{q[5]:X2}{q[6]:X2}{q[7] >> 4:X}";

        public static byte GetIsrcCode(char c)
        {
            switch(c)
            {
                case '0': return 0x00;
                case '1': return 0x01;
                case '2': return 0x02;
                case '3': return 0x03;
                case '4': return 0x04;
                case '5': return 0x05;
                case '6': return 0x06;
                case '7': return 0x07;
                case '8': return 0x08;
                case '9': return 0x09;
                case 'A': return 0x11;
                case 'B': return 0x12;
                case 'C': return 0x13;
                case 'D': return 0x14;
                case 'E': return 0x15;
                case 'F': return 0x16;
                case 'G': return 0x17;
                case 'H': return 0x18;
                case 'I': return 0x19;
                case 'J': return 0x1A;
                case 'K': return 0x1B;
                case 'L': return 0x1C;
                case 'M': return 0x1D;
                case 'N': return 0x1E;
                case 'O': return 0x1F;
                case 'P': return 0x20;
                case 'Q': return 0x21;
                case 'R': return 0x22;
                case 'S': return 0x23;
                case 'T': return 0x24;
                case 'U': return 0x25;
                case 'V': return 0x26;
                case 'W': return 0x27;
                case 'X': return 0x28;
                case 'Y': return 0x29;
                case 'Z': return 0x2A;
                default:  return 0x00;
            }
        }

        public static byte[] Generate(int sector, uint trackSequence, int pregap, int trackStart, byte flags,
                                      byte index)
        {
            bool isPregap = sector < 0 || sector <= trackStart + pregap;

            if(index == 0)
                index = (byte)(isPregap ? 0 : 1);

            byte[] sub = new byte[96];

            // P
            if(isPregap)
            {
                sub[0]  = 0xFF;
                sub[1]  = 0xFF;
                sub[2]  = 0xFF;
                sub[3]  = 0xFF;
                sub[4]  = 0xFF;
                sub[5]  = 0xFF;
                sub[6]  = 0xFF;
                sub[7]  = 0xFF;
                sub[8]  = 0xFF;
                sub[9]  = 0xFF;
                sub[10] = 0xFF;
                sub[11] = 0xFF;
            }

            // Q
            byte[] q = new byte[12];

            q[0] = (byte)((flags << 4) + 1);
            q[1] = (byte)trackSequence;
            q[2] = index;

            int relative;

            if(isPregap)
                relative = pregap + trackStart - sector;
            else
                relative = sector - trackStart;

            sector += 150;

            int min   = relative / 60 / 75;
            int sec   = (relative / 75) - (min * 60);
            int frame = relative        - (min * 60 * 75) - (sec * 75);

            int amin   = sector / 60 / 75;
            int asec   = (sector / 75) - (amin * 60);
            int aframe = sector        - (amin * 60 * 75) - (asec * 75);

            q[3] = (byte)min;
            q[4] = (byte)sec;
            q[5] = (byte)frame;

            q[7] = (byte)amin;
            q[8] = (byte)asec;
            q[9] = (byte)aframe;

            q[1] = (byte)(((q[1] / 10) << 4) + (q[1] % 10));
            q[2] = (byte)(((q[2] / 10) << 4) + (q[2] % 10));
            q[3] = (byte)(((q[3] / 10) << 4) + (q[3] % 10));
            q[4] = (byte)(((q[4] / 10) << 4) + (q[4] % 10));
            q[5] = (byte)(((q[5] / 10) << 4) + (q[5] % 10));
            q[6] = (byte)(((q[6] / 10) << 4) + (q[6] % 10));
            q[7] = (byte)(((q[7] / 10) << 4) + (q[7] % 10));
            q[8] = (byte)(((q[8] / 10) << 4) + (q[8] % 10));

            q[9] = (byte)(((q[9] / 10) << 4) + (q[9] % 10));

            CRC16CCITTContext.Data(q, 10, out byte[] qCrc);
            q[10] = qCrc[0];
            q[11] = qCrc[1];

            Array.Copy(q, 0, sub, 12, 12);

            return Interleave(sub);
        }
    }
}