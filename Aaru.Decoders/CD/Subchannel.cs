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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Checksums;

namespace Aaru.Decoders.CD;

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
        if((q[0] & 0xF) == 1 || (q[0] & 0xF) == 5)
        {
            q[1] = (byte)((q[1] / 10 << 4) + q[1] % 10);
            q[2] = (byte)((q[2] / 10 << 4) + q[2] % 10);
            q[3] = (byte)((q[3] / 10 << 4) + q[3] % 10);
            q[4] = (byte)((q[4] / 10 << 4) + q[4] % 10);
            q[5] = (byte)((q[5] / 10 << 4) + q[5] % 10);
            q[6] = (byte)((q[6] / 10 << 4) + q[6] % 10);
            q[7] = (byte)((q[7] / 10 << 4) + q[7] % 10);
            q[8] = (byte)((q[8] / 10 << 4) + q[8] % 10);
        }

        q[9] = (byte)((q[9] / 10 << 4) + q[9] % 10);
    }

    public static void BcdToBinaryQ(byte[] q)
    {
        if((q[0] & 0xF) == 1 || (q[0] & 0xF) == 5)
        {
            q[1] = (byte)(q[1] / 16 * 10 + (q[1] & 0x0F));
            q[2] = (byte)(q[2] / 16 * 10 + (q[2] & 0x0F));
            q[3] = (byte)(q[3] / 16 * 10 + (q[3] & 0x0F));
            q[4] = (byte)(q[4] / 16 * 10 + (q[4] & 0x0F));
            q[5] = (byte)(q[5] / 16 * 10 + (q[5] & 0x0F));
            q[6] = (byte)(q[6] / 16 * 10 + (q[6] & 0x0F));
            q[7] = (byte)(q[7] / 16 * 10 + (q[7] & 0x0F));
            q[8] = (byte)(q[8] / 16 * 10 + (q[8] & 0x0F));
        }

        q[9] = (byte)(q[9] / 16 * 10 + (q[9] & 0x0F));
    }

    public static byte[] ConvertQToRaw(byte[] subchannel)
    {
        var pos    = 0;
        var subBuf = new byte[subchannel.Length * 6];

        for(var i = 0; i < subchannel.Length; i += 16)
        {
            // P
            if((subchannel[i + 15] & 0x80) <= 0)
                pos += 12;
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
        var subBuf = new byte[subchannel.Length];

        var outPos = 0;

        for(var inPos = 0; inPos < subchannel.Length; inPos += 96)
        {
            for(var i = 0; i < 12; i++)
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
        var subBuf = new byte[subchannel.Length];
        var inPos  = 0;

        for(var outPos = 0; outPos < subchannel.Length; outPos += 96)
        {
            for(var i = 0; i < 12; i++)
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
        CRC16CcittContext.Data(subBuf, 10, out byte[] crc);

        bool   crcOk  = crc[0] == subBuf[10] && crc[1] == subBuf[11];
        long   minute = (lba + 150)        / 4500;
        long   second = (lba + 150) % 4500 / 75;
        long   frame  = (lba + 150)        % 4500 % 75;
        string area;
        int    control = (subBuf[0] & 0xF0) / 16;
        int    adr     = subBuf[0] & 0x0F;

        string controlInfo = ((control & 0xC) / 4) switch
                             {
                                 0 => (control & 0x01) == 1
                                          ? Localization.Subchannel_PrettifyQ_stereo_audio_with_pre_emphasis
                                          : Localization.Subchannel_PrettifyQ_stereo_audio_without_pre_emphasis,
                                 1 => (control & 0x01) == 1
                                          ? Localization.Subchannel_PrettifyQ_incremental_data
                                          : Localization.Subchannel_PrettifyQ_uninterrupted_data,
                                 2 => (control & 0x01) == 1
                                          ? Localization.Subchannel_PrettifyQ_quadraphonic_audio_with_pre_emphasis
                                          : Localization.Subchannel_PrettifyQ_quadraphonic_audio_without_pre_emphasis,
                                 _ => string.Format(Localization.Subchannel_PrettifyQ_reserved_control_value__0_,
                                                    control & 0x01)
                             };

        string copy = (control & 0x02) > 0
                          ? Localization.Subchannel_PrettifyQ_copy_permitted
                          : Localization.Subchannel_PrettifyQ_copy_prohibited;

        if(bcd) BcdToBinaryQ(subBuf);

        int  qPos = subBuf[3] * 60 * 75 + subBuf[4] * 75 + subBuf[5] - 150;
        byte pmin = subBuf[7];
        byte psec = subBuf[8];

        int  qStart  = subBuf[7] * 60 * 75 + subBuf[8] * 75 + subBuf[9] - 150;
        int  nextPos = subBuf[3] * 60 * 75 + subBuf[4] * 75 + subBuf[5] - 150;
        byte zero    = subBuf[6];
        int  maxOut  = subBuf[7] * 60 * 75 + subBuf[8] * 75 + subBuf[9] - 150;
        bool final   = subBuf[3] == 0xFF && subBuf[4] == 0xFF && subBuf[5] == 0xFF;

        BinaryToBcdQ(subBuf);

        if(lba < 0)
        {
            area = Localization.Subchannel_PrettifyQ_Lead_In;

            switch(adr)
            {
                case 1 when subBuf[2] < 0xA0:
                    return string.Format(Localization
                                            .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_position_9_10_11_LBA_12_track_13_starts_at_14_15_16_LBA_17_Q_CRC_18_19_20_R_W_21,
                                         minute,
                                         second,
                                         frame,
                                         lba,
                                         area,
                                         corruptedPause
                                             ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                             : pause
                                                 ? Localization.Subchannel_PrettifyQ_pause
                                                 : Localization.Subchannel_PrettifyQ_not_pause,
                                         controlInfo,
                                         copy,
                                         adr,
                                         subBuf[3],
                                         subBuf[4],
                                         subBuf[5],
                                         qPos,
                                         subBuf[2],
                                         subBuf[7],
                                         subBuf[8],
                                         subBuf[9],
                                         qStart,
                                         subBuf[10],
                                         subBuf[11],
                                         crcOk
                                             ? Localization.Subchannel_PrettifyQ_OK
                                             : Localization.Subchannel_PrettifyQ_BAD,
                                         rwEmpty
                                             ? Localization.Subchannel_PrettifyQ_empty
                                             : Localization.Subchannel_PrettifyQ_not_empty);
                case 1 when subBuf[2] == 0xA0:
                {
                    string format = subBuf[8] switch
                                    {
                                        0x00 => Localization.Subchannel_PrettifyQ_CD_DA_CD_ROM,
                                        0x10 => Localization.Subchannel_PrettifyQ_CD_i,
                                        0x20 => Localization.Subchannel_PrettifyQ_CD_ROM_XA,
                                        _    => string.Format(Localization.Subchannel_PrettifyQ_unknown_0, subBuf[0])
                                    };

                    return string.Format(Localization
                                            .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_position_9_10_11_LBA_12_track_13_is_first_program_area_track_in_14_format_Q_CRC_15_16_17_R_W_18,
                                         minute,
                                         second,
                                         frame,
                                         lba,
                                         area,
                                         corruptedPause
                                             ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                             : pause
                                                 ? Localization.Subchannel_PrettifyQ_pause
                                                 : Localization.Subchannel_PrettifyQ_not_pause,
                                         controlInfo,
                                         copy,
                                         adr,
                                         subBuf[3],
                                         subBuf[4],
                                         subBuf[5],
                                         qPos,
                                         subBuf[2],
                                         format,
                                         subBuf[10],
                                         subBuf[11],
                                         crcOk
                                             ? Localization.Subchannel_PrettifyQ_OK
                                             : Localization.Subchannel_PrettifyQ_BAD,
                                         rwEmpty
                                             ? Localization.Subchannel_PrettifyQ_empty
                                             : Localization.Subchannel_PrettifyQ_not_empty);
                }
                case 1 when subBuf[2] == 0xA1:
                    return string.Format(Localization
                                            .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_position_9_10_11_LBA_12_track_13_is_last_program_area_track_Q_CRC_14_15_16_R_W_17,
                                         minute,
                                         second,
                                         frame,
                                         lba,
                                         area,
                                         corruptedPause
                                             ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                             : pause
                                                 ? Localization.Subchannel_PrettifyQ_pause
                                                 : Localization.Subchannel_PrettifyQ_not_pause,
                                         controlInfo,
                                         copy,
                                         adr,
                                         subBuf[3],
                                         subBuf[4],
                                         subBuf[5],
                                         qPos,
                                         subBuf[2],
                                         subBuf[10],
                                         subBuf[11],
                                         crcOk
                                             ? Localization.Subchannel_PrettifyQ_OK
                                             : Localization.Subchannel_PrettifyQ_BAD,
                                         rwEmpty
                                             ? Localization.Subchannel_PrettifyQ_empty
                                             : Localization.Subchannel_PrettifyQ_not_empty);
                case 1:
                    return subBuf[2] == 0xA2
                               ? string.Format(Localization
                                                  .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_position_9_10_11_LBA_12_track_13_starts_at_14_15_16_LBA_17_Q_CRC_18_19_20_R_W_21,
                                               minute,
                                               second,
                                               frame,
                                               lba,
                                               area,
                                               corruptedPause
                                                   ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                                   : pause
                                                       ? Localization.Subchannel_PrettifyQ_pause
                                                       : Localization.Subchannel_PrettifyQ_not_pause,
                                               controlInfo,
                                               copy,
                                               adr,
                                               subBuf[3],
                                               subBuf[4],
                                               subBuf[5],
                                               qPos,
                                               subBuf[2],
                                               subBuf[7],
                                               subBuf[8],
                                               subBuf[9],
                                               qStart,
                                               subBuf[10],
                                               subBuf[11],
                                               crcOk
                                                   ? Localization.Subchannel_PrettifyQ_OK
                                                   : Localization.Subchannel_PrettifyQ_BAD,
                                               rwEmpty
                                                   ? Localization.Subchannel_PrettifyQ_empty
                                                   : Localization.Subchannel_PrettifyQ_not_empty)
                               : string.Format(Localization
                                                  .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_8_9_10_11_12_13_14_15_16_17_CRC_18_19_20_R_W_21,
                                               minute,
                                               second,
                                               frame,
                                               lba,
                                               area,
                                               corruptedPause
                                                   ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                                   : pause
                                                       ? Localization.Subchannel_PrettifyQ_pause
                                                       : Localization.Subchannel_PrettifyQ_not_pause,
                                               controlInfo,
                                               copy,
                                               subBuf[0],
                                               subBuf[1],
                                               subBuf[2],
                                               subBuf[3],
                                               subBuf[4],
                                               subBuf[5],
                                               subBuf[6],
                                               subBuf[7],
                                               subBuf[8],
                                               subBuf[9],
                                               subBuf[10],
                                               subBuf[11],
                                               crcOk
                                                   ? Localization.Subchannel_PrettifyQ_OK
                                                   : Localization.Subchannel_PrettifyQ_BAD,
                                               rwEmpty
                                                   ? Localization.Subchannel_PrettifyQ_empty
                                                   : Localization.Subchannel_PrettifyQ_not_empty);
                case 2:
                    return string.Format(Localization
                                            .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_MCN_9_frame_10_CRC_11_12_13_R_W_14,
                                         minute,
                                         second,
                                         frame,
                                         lba,
                                         area,
                                         corruptedPause
                                             ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                             : pause
                                                 ? Localization.Subchannel_PrettifyQ_pause
                                                 : Localization.Subchannel_PrettifyQ_not_pause,
                                         controlInfo,
                                         copy,
                                         adr,
                                         DecodeMcn(subBuf),
                                         subBuf[9],
                                         subBuf[10],
                                         subBuf[11],
                                         crcOk
                                             ? Localization.Subchannel_PrettifyQ_OK
                                             : Localization.Subchannel_PrettifyQ_BAD,
                                         rwEmpty
                                             ? Localization.Subchannel_PrettifyQ_empty
                                             : Localization.Subchannel_PrettifyQ_not_empty);
            }

            if(adr != 5)
            {
                return string.Format(Localization
                                        .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_8_9_10_11_12_13_14_15_16_17_CRC_18_19_20_R_W_21,
                                     minute,
                                     second,
                                     frame,
                                     lba,
                                     area,
                                     corruptedPause
                                         ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                         : pause
                                             ? Localization.Subchannel_PrettifyQ_pause
                                             : Localization.Subchannel_PrettifyQ_not_pause,
                                     controlInfo,
                                     copy,
                                     subBuf[0],
                                     subBuf[1],
                                     subBuf[2],
                                     subBuf[3],
                                     subBuf[4],
                                     subBuf[5],
                                     subBuf[6],
                                     subBuf[7],
                                     subBuf[8],
                                     subBuf[9],
                                     subBuf[10],
                                     subBuf[11],
                                     crcOk
                                         ? Localization.Subchannel_PrettifyQ_OK
                                         : Localization.Subchannel_PrettifyQ_BAD,
                                     rwEmpty
                                         ? Localization.Subchannel_PrettifyQ_empty
                                         : Localization.Subchannel_PrettifyQ_not_empty);
            }

            switch(subBuf[2])
            {
                case <= 0x40:
                    return string.Format(Localization
                                            .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_skip_interval_start_time_9_10_11_skip_interval_stop_time_12_13_14_CRC_15_16_17_R_W_18,
                                         minute,
                                         second,
                                         frame,
                                         lba,
                                         area,
                                         corruptedPause
                                             ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                             : pause
                                                 ? Localization.Subchannel_PrettifyQ_pause
                                                 : Localization.Subchannel_PrettifyQ_not_pause,
                                         controlInfo,
                                         copy,
                                         adr,
                                         subBuf[7],
                                         subBuf[8],
                                         subBuf[9],
                                         subBuf[3],
                                         subBuf[4],
                                         subBuf[5],
                                         subBuf[10],
                                         subBuf[11],
                                         crcOk
                                             ? Localization.Subchannel_PrettifyQ_OK
                                             : Localization.Subchannel_PrettifyQ_BAD,
                                         rwEmpty
                                             ? Localization.Subchannel_PrettifyQ_empty
                                             : Localization.Subchannel_PrettifyQ_not_empty);
                case 0xB0:
                    return final
                               ? string.Format(Localization
                                                  .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_next_program_area_can_start_at_9_10_11_LBA_12_last_session_13_mode_5_pointers_CRC_14_15_16_R_W_17,
                                               minute,
                                               second,
                                               frame,
                                               lba,
                                               area,
                                               corruptedPause
                                                   ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                                   : pause
                                                       ? Localization.Subchannel_PrettifyQ_pause
                                                       : Localization.Subchannel_PrettifyQ_not_pause,
                                               controlInfo,
                                               copy,
                                               adr,
                                               subBuf[3],
                                               subBuf[4],
                                               subBuf[5],
                                               nextPos,
                                               zero,
                                               subBuf[10],
                                               subBuf[11],
                                               crcOk
                                                   ? Localization.Subchannel_PrettifyQ_OK
                                                   : Localization.Subchannel_PrettifyQ_BAD,
                                               rwEmpty
                                                   ? Localization.Subchannel_PrettifyQ_empty
                                                   : Localization.Subchannel_PrettifyQ_not_empty)
                               : string.Format(Localization
                                                  .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_next_program_area_can_start_at_9_10_11_LBA_12_maximum_Lead_out_at_13_14_15_LBA_16_17_mode_5_pointers_CRC_18_19_20_R_W_21,
                                               minute,
                                               second,
                                               frame,
                                               lba,
                                               area,
                                               corruptedPause
                                                   ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                                   : pause
                                                       ? Localization.Subchannel_PrettifyQ_pause
                                                       : Localization.Subchannel_PrettifyQ_not_pause,
                                               controlInfo,
                                               copy,
                                               adr,
                                               subBuf[3],
                                               subBuf[4],
                                               subBuf[5],
                                               nextPos,
                                               subBuf[7],
                                               subBuf[8],
                                               subBuf[9],
                                               maxOut,
                                               zero,
                                               subBuf[10],
                                               subBuf[11],
                                               crcOk
                                                   ? Localization.Subchannel_PrettifyQ_OK
                                                   : Localization.Subchannel_PrettifyQ_BAD,
                                               rwEmpty
                                                   ? Localization.Subchannel_PrettifyQ_empty
                                                   : Localization.Subchannel_PrettifyQ_not_empty);
                case 0xB1:
                    return string.Format(Localization
                                            .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_9_skip_interval_pointers_10_skip_track_assignments_CRC_11_12_13_R_W_14,
                                         minute,
                                         second,
                                         frame,
                                         lba,
                                         area,
                                         corruptedPause
                                             ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                             : pause
                                                 ? Localization.Subchannel_PrettifyQ_pause
                                                 : Localization.Subchannel_PrettifyQ_not_pause,
                                         controlInfo,
                                         copy,
                                         adr,
                                         pmin,
                                         psec,
                                         subBuf[10],
                                         subBuf[11],
                                         crcOk
                                             ? Localization.Subchannel_PrettifyQ_OK
                                             : Localization.Subchannel_PrettifyQ_BAD,
                                         rwEmpty
                                             ? Localization.Subchannel_PrettifyQ_empty
                                             : Localization.Subchannel_PrettifyQ_not_empty);
            }

            if(subBuf[2] != 0xB2 && subBuf[2] != 0xB3 && subBuf[2] != 0xB4)
            {
                return subBuf[2] == 0xC0
                           ? string.Format(Localization
                                              .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_ATIP_values_9_10_11_first_disc_Lead_in_starts_at_12_13_14_LBA_15_CRC_16_17_18_R_W_19,
                                           minute,
                                           second,
                                           frame,
                                           lba,
                                           area,
                                           corruptedPause
                                               ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                               : pause
                                                   ? Localization.Subchannel_PrettifyQ_pause
                                                   : Localization.Subchannel_PrettifyQ_not_pause,
                                           controlInfo,
                                           copy,
                                           adr,
                                           subBuf[3],
                                           subBuf[4],
                                           subBuf[5],
                                           subBuf[7],
                                           subBuf[8],
                                           subBuf[9],
                                           qStart,
                                           subBuf[10],
                                           subBuf[11],
                                           crcOk
                                               ? Localization.Subchannel_PrettifyQ_OK
                                               : Localization.Subchannel_PrettifyQ_BAD,
                                           rwEmpty
                                               ? Localization.Subchannel_PrettifyQ_empty
                                               : Localization.Subchannel_PrettifyQ_not_empty)
                           : string.Format(Localization
                                              .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_8_9_10_11_12_13_14_15_16_17_CRC_18_19_20_R_W_21,
                                           minute,
                                           second,
                                           frame,
                                           lba,
                                           area,
                                           corruptedPause
                                               ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                               : pause
                                                   ? Localization.Subchannel_PrettifyQ_pause
                                                   : Localization.Subchannel_PrettifyQ_not_pause,
                                           controlInfo,
                                           copy,
                                           subBuf[0],
                                           subBuf[1],
                                           subBuf[2],
                                           subBuf[3],
                                           subBuf[4],
                                           subBuf[5],
                                           subBuf[6],
                                           subBuf[7],
                                           subBuf[8],
                                           subBuf[9],
                                           subBuf[10],
                                           subBuf[11],
                                           crcOk
                                               ? Localization.Subchannel_PrettifyQ_OK
                                               : Localization.Subchannel_PrettifyQ_BAD,
                                           rwEmpty
                                               ? Localization.Subchannel_PrettifyQ_empty
                                               : Localization.Subchannel_PrettifyQ_not_empty);
            }

            var skipTracks = $"{subBuf[3]:X2}";

            if(subBuf[4] > 0) skipTracks += $", {subBuf[4]:X2}";

            if(subBuf[5] > 0) skipTracks += $", {subBuf[4]:X2}";

            if(subBuf[7] > 0) skipTracks += $", {subBuf[4]:X2}";

            if(subBuf[8] > 0) skipTracks += $", {subBuf[4]:X2}";

            if(subBuf[9] > 0) skipTracks += $", {subBuf[4]:X2}";

            return string.Format(Localization
                                    .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_tracks_9_to_be_skipped_CRC_10_11_12_R_W_13,
                                 minute,
                                 second,
                                 frame,
                                 lba,
                                 area,
                                 corruptedPause
                                     ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                     : pause
                                         ? Localization.Subchannel_PrettifyQ_pause
                                         : Localization.Subchannel_PrettifyQ_not_pause,
                                 controlInfo,
                                 copy,
                                 adr,
                                 skipTracks,
                                 subBuf[10],
                                 subBuf[11],
                                 crcOk ? Localization.Subchannel_PrettifyQ_OK : Localization.Subchannel_PrettifyQ_BAD,
                                 rwEmpty
                                     ? Localization.Subchannel_PrettifyQ_empty
                                     : Localization.Subchannel_PrettifyQ_not_empty);
        }

        area = subBuf[1] == 0xAA
                   ? Localization.Subchannel_PrettifyQ_Lead_out
                   : Localization.Subchannel_PrettifyQ_Program;

        return adr switch
               {
                   1 => string.Format(Localization
                                         .Subchannel_PrettifyQ_0_D2_1_2_LBA_3_4_area_5_6_7_Q_mode_8_position_track_9_index_10_relative_position_11_12_13_LBA_14_absolute_position_15_16_17_LBA_18_Q_CRC_19_20_21_R_W_22,
                                      minute,
                                      second,
                                      frame,
                                      lba,
                                      area,
                                      corruptedPause
                                          ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                          : pause
                                              ? Localization.Subchannel_PrettifyQ_pause
                                              : Localization.Subchannel_PrettifyQ_not_pause,
                                      controlInfo,
                                      copy,
                                      adr,
                                      subBuf[1],
                                      subBuf[2],
                                      subBuf[3],
                                      subBuf[4],
                                      subBuf[5],
                                      qPos + 150,
                                      subBuf[7],
                                      subBuf[8],
                                      subBuf[9],
                                      qStart,
                                      subBuf[10],
                                      subBuf[11],
                                      crcOk
                                          ? Localization.Subchannel_PrettifyQ_OK
                                          : Localization.Subchannel_PrettifyQ_BAD,
                                      rwEmpty
                                          ? Localization.Subchannel_PrettifyQ_empty
                                          : Localization.Subchannel_PrettifyQ_not_empty),
                   2 => string.Format(Localization
                                         .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_MCN_9_frame_10_CRC_11_12_13_R_W_14,
                                      minute,
                                      second,
                                      frame,
                                      lba,
                                      area,
                                      corruptedPause
                                          ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                          : pause
                                              ? Localization.Subchannel_PrettifyQ_pause
                                              : Localization.Subchannel_PrettifyQ_not_pause,
                                      controlInfo,
                                      copy,
                                      adr,
                                      DecodeMcn(subBuf),
                                      subBuf[9],
                                      subBuf[10],
                                      subBuf[11],
                                      crcOk
                                          ? Localization.Subchannel_PrettifyQ_OK
                                          : Localization.Subchannel_PrettifyQ_BAD,
                                      rwEmpty
                                          ? Localization.Subchannel_PrettifyQ_empty
                                          : Localization.Subchannel_PrettifyQ_not_empty),
                   3 => string.Format(Localization
                                         .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_mode_8_ISRC_9_frame_10_CRC_11_12_13_R_W_14,
                                      minute,
                                      second,
                                      frame,
                                      lba,
                                      area,
                                      corruptedPause
                                          ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                          : pause
                                              ? Localization.Subchannel_PrettifyQ_pause
                                              : Localization.Subchannel_PrettifyQ_not_pause,
                                      controlInfo,
                                      copy,
                                      adr,
                                      DecodeIsrc(subBuf),
                                      subBuf[9],
                                      subBuf[10],
                                      subBuf[11],
                                      crcOk
                                          ? Localization.Subchannel_PrettifyQ_OK
                                          : Localization.Subchannel_PrettifyQ_BAD,
                                      rwEmpty
                                          ? Localization.Subchannel_PrettifyQ_empty
                                          : Localization.Subchannel_PrettifyQ_not_empty),
                   _ => string.Format(Localization
                                         .Subchannel_PrettifyQ_0_1_2_LBA_3_4_area_5_6_7_Q_8_9_10_11_12_13_14_15_16_17_CRC_18_19_20_R_W_21,
                                      minute,
                                      second,
                                      frame,
                                      lba,
                                      area,
                                      corruptedPause
                                          ? Localization.Subchannel_PrettifyQ_corrupted_pause
                                          : pause
                                              ? Localization.Subchannel_PrettifyQ_pause
                                              : Localization.Subchannel_PrettifyQ_not_pause,
                                      controlInfo,
                                      copy,
                                      subBuf[0],
                                      subBuf[1],
                                      subBuf[2],
                                      subBuf[3],
                                      subBuf[4],
                                      subBuf[5],
                                      subBuf[6],
                                      subBuf[7],
                                      subBuf[8],
                                      subBuf[9],
                                      subBuf[10],
                                      subBuf[11],
                                      crcOk
                                          ? Localization.Subchannel_PrettifyQ_OK
                                          : Localization.Subchannel_PrettifyQ_BAD,
                                      rwEmpty
                                          ? Localization.Subchannel_PrettifyQ_empty
                                          : Localization.Subchannel_PrettifyQ_not_empty)
               };
    }

    public static string DecodeIsrc(byte[] q) =>
        $"{_isrcTable[q[1] / 4]}{_isrcTable[(q[1] & 3) * 16 + q[2] / 16]}{_isrcTable[(q[2] & 0xF) * 4 + q[3] / 64]}{_isrcTable[q[3] & 0x3F]}{_isrcTable[q[4] / 4]}{q[5]:X2}{q[6]:X2}{q[7]:X2}{q[8] / 16:X1}";

    public static string DecodeMcn(byte[] q) => $"{q[1]:X2}{q[2]:X2}{q[3]:X2}{q[4]:X2}{q[5]:X2}{q[6]:X2}{q[7] >> 4:X}";

    public static byte GetIsrcCode(char c) => c switch
                                              {
                                                  '0' => 0x00,
                                                  '1' => 0x01,
                                                  '2' => 0x02,
                                                  '3' => 0x03,
                                                  '4' => 0x04,
                                                  '5' => 0x05,
                                                  '6' => 0x06,
                                                  '7' => 0x07,
                                                  '8' => 0x08,
                                                  '9' => 0x09,
                                                  'A' => 0x11,
                                                  'B' => 0x12,
                                                  'C' => 0x13,
                                                  'D' => 0x14,
                                                  'E' => 0x15,
                                                  'F' => 0x16,
                                                  'G' => 0x17,
                                                  'H' => 0x18,
                                                  'I' => 0x19,
                                                  'J' => 0x1A,
                                                  'K' => 0x1B,
                                                  'L' => 0x1C,
                                                  'M' => 0x1D,
                                                  'N' => 0x1E,
                                                  'O' => 0x1F,
                                                  'P' => 0x20,
                                                  'Q' => 0x21,
                                                  'R' => 0x22,
                                                  'S' => 0x23,
                                                  'T' => 0x24,
                                                  'U' => 0x25,
                                                  'V' => 0x26,
                                                  'W' => 0x27,
                                                  'X' => 0x28,
                                                  'Y' => 0x29,
                                                  'Z' => 0x2A,
                                                  _   => 0x00
                                              };

    public static byte[] Generate(int sector, uint trackSequence, int pregap, int trackStart, byte flags, byte index)
    {
        bool isPregap = sector < 0 || sector <= trackStart + pregap;

        if(index == 0) index = (byte)(isPregap ? 0 : 1);

        var sub = new byte[96];

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
        var q = new byte[12];

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
        int sec   = relative / 75 - min * 60;
        int frame = relative      - min * 60 * 75 - sec * 75;

        int amin   = sector / 60 / 75;
        int asec   = sector / 75 - amin * 60;
        int aframe = sector      - amin * 60 * 75 - asec * 75;

        q[3] = (byte)min;
        q[4] = (byte)sec;
        q[5] = (byte)frame;

        q[7] = (byte)amin;
        q[8] = (byte)asec;
        q[9] = (byte)aframe;

        q[1] = (byte)((q[1] / 10 << 4) + q[1] % 10);
        q[2] = (byte)((q[2] / 10 << 4) + q[2] % 10);
        q[3] = (byte)((q[3] / 10 << 4) + q[3] % 10);
        q[4] = (byte)((q[4] / 10 << 4) + q[4] % 10);
        q[5] = (byte)((q[5] / 10 << 4) + q[5] % 10);
        q[6] = (byte)((q[6] / 10 << 4) + q[6] % 10);
        q[7] = (byte)((q[7] / 10 << 4) + q[7] % 10);
        q[8] = (byte)((q[8] / 10 << 4) + q[8] % 10);

        q[9] = (byte)((q[9] / 10 << 4) + q[9] % 10);

        CRC16CcittContext.Data(q, 10, out byte[] qCrc);
        q[10] = qCrc[0];
        q[11] = qCrc[1];

        Array.Copy(q, 0, sub, 12, 12);

        return Interleave(sub);
    }
}