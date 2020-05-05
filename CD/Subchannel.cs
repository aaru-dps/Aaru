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
            q[1] = (byte)(((q[1] / 10) << 4) + (q[1] % 10));
            q[2] = (byte)(((q[2] / 10) << 4) + (q[2] % 10));
            q[3] = (byte)(((q[3] / 10) << 4) + (q[3] % 10));
            q[4] = (byte)(((q[4] / 10) << 4) + (q[4] % 10));
            q[5] = (byte)(((q[5] / 10) << 4) + (q[5] % 10));
            q[6] = (byte)(((q[6] / 10) << 4) + (q[6] % 10));
            q[7] = (byte)(((q[7] / 10) << 4) + (q[7] % 10));
            q[8] = (byte)(((q[8] / 10) << 4) + (q[8] % 10));
            q[9] = (byte)(((q[9] / 10) << 4) + (q[9] % 10));
        }

        public static void BcdToBinaryQ(byte[] q)
        {
            q[1] = (byte)(((q[1] / 16) * 10) + (q[1] & 0x0F));
            q[2] = (byte)(((q[2] / 16) * 10) + (q[2] & 0x0F));
            q[3] = (byte)(((q[3] / 16) * 10) + (q[3] & 0x0F));
            q[4] = (byte)(((q[4] / 16) * 10) + (q[4] & 0x0F));
            q[5] = (byte)(((q[5] / 16) * 10) + (q[5] & 0x0F));
            q[6] = (byte)(((q[6] / 16) * 10) + (q[6] & 0x0F));
            q[7] = (byte)(((q[7] / 16) * 10) + (q[7] & 0x0F));
            q[8] = (byte)(((q[8] / 16) * 10) + (q[8] & 0x0F));
            q[9] = (byte)(((q[9] / 16) * 10) + (q[9] & 0x0F));
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

            int pos = 0;

            while(pos < subBuf.Length)
            {
                // P
                subBuf[pos + 0] += (byte)(subchannel[pos + 0] & 0x80);
                subBuf[pos + 1] += (byte)((subchannel[pos + 0] & 0x40) << 1);
                subBuf[pos + 2] += (byte)((subchannel[pos + 0] & 0x20) << 2);
                subBuf[pos + 3] += (byte)((subchannel[pos + 0] & 0x10) << 3);
                subBuf[pos + 4] += (byte)((subchannel[pos + 0] & 0x08) << 4);
                subBuf[pos + 5] += (byte)((subchannel[pos + 0] & 0x04) << 5);
                subBuf[pos + 6] += (byte)((subchannel[pos + 0] & 0x02) << 6);
                subBuf[pos + 7] += (byte)((subchannel[pos + 0] & 0x01) << 7);

                // Q
                subBuf[pos + 0] += (byte)((subchannel[pos + 1] & 0x80) >> 1);
                subBuf[pos + 1] += (byte)(subchannel[pos + 1] & 0x40);
                subBuf[pos + 2] += (byte)((subchannel[pos + 1] & 0x20) << 1);
                subBuf[pos + 3] += (byte)((subchannel[pos + 1] & 0x10) << 2);
                subBuf[pos + 4] += (byte)((subchannel[pos + 1] & 0x08) << 3);
                subBuf[pos + 5] += (byte)((subchannel[pos + 1] & 0x04) << 4);
                subBuf[pos + 6] += (byte)((subchannel[pos + 1] & 0x02) << 5);
                subBuf[pos + 7] += (byte)((subchannel[pos + 1] & 0x01) << 6);

                // R
                subBuf[pos + 0] += (byte)((subchannel[pos + 2] & 0x80) >> 2);
                subBuf[pos + 1] += (byte)((subchannel[pos + 2] & 0x40) >> 1);
                subBuf[pos + 2] += (byte)(subchannel[pos + 2] & 0x20);
                subBuf[pos + 3] += (byte)((subchannel[pos + 2] & 0x10) << 1);
                subBuf[pos + 4] += (byte)((subchannel[pos + 2] & 0x08) << 2);
                subBuf[pos + 5] += (byte)((subchannel[pos + 2] & 0x04) << 3);
                subBuf[pos + 6] += (byte)((subchannel[pos + 2] & 0x02) << 4);
                subBuf[pos + 7] += (byte)((subchannel[pos + 2] & 0x01) << 5);

                // S
                subBuf[pos + 0] += (byte)((subchannel[pos + 3] & 0x80) >> 3);
                subBuf[pos + 1] += (byte)((subchannel[pos + 3] & 0x40) >> 2);
                subBuf[pos + 2] += (byte)((subchannel[pos + 3] & 0x20) >> 1);
                subBuf[pos + 3] += (byte)(subchannel[pos + 3] & 0x10);
                subBuf[pos + 4] += (byte)((subchannel[pos + 3] & 0x08) << 1);
                subBuf[pos + 5] += (byte)((subchannel[pos + 3] & 0x04) << 2);
                subBuf[pos + 6] += (byte)((subchannel[pos + 3] & 0x02) << 3);
                subBuf[pos + 7] += (byte)((subchannel[pos + 3] & 0x01) << 4);

                // T
                subBuf[pos + 0] += (byte)((subchannel[pos + 4] & 0x80) >> 4);
                subBuf[pos + 1] += (byte)((subchannel[pos + 4] & 0x40) >> 3);
                subBuf[pos + 2] += (byte)((subchannel[pos + 4] & 0x20) >> 2);
                subBuf[pos + 3] += (byte)((subchannel[pos + 4] & 0x10) >> 1);
                subBuf[pos + 4] += (byte)(subchannel[pos + 4] & 0x08);
                subBuf[pos + 5] += (byte)((subchannel[pos + 4] & 0x04) << 1);
                subBuf[pos + 6] += (byte)((subchannel[pos + 4] & 0x02) << 2);
                subBuf[pos + 7] += (byte)((subchannel[pos + 4] & 0x01) << 3);

                // U
                subBuf[pos + 0] += (byte)((subchannel[pos + 5] & 0x80) >> 5);
                subBuf[pos + 1] += (byte)((subchannel[pos + 5] & 0x40) >> 4);
                subBuf[pos + 2] += (byte)((subchannel[pos + 5] & 0x20) >> 3);
                subBuf[pos + 3] += (byte)((subchannel[pos + 5] & 0x10) >> 2);
                subBuf[pos + 4] += (byte)((subchannel[pos + 5] & 0x08) >> 1);
                subBuf[pos + 5] += (byte)(subchannel[pos + 5] & 0x04);
                subBuf[pos + 6] += (byte)((subchannel[pos + 5] & 0x02) << 1);
                subBuf[pos + 7] += (byte)((subchannel[pos + 5] & 0x01) << 2);

                // V
                subBuf[pos + 0] += (byte)((subchannel[pos + 6] & 0x80) >> 6);
                subBuf[pos + 1] += (byte)((subchannel[pos + 6] & 0x40) >> 5);
                subBuf[pos + 2] += (byte)((subchannel[pos + 6] & 0x20) >> 4);
                subBuf[pos + 3] += (byte)((subchannel[pos + 6] & 0x10) >> 3);
                subBuf[pos + 4] += (byte)((subchannel[pos + 6] & 0x08) >> 2);
                subBuf[pos + 5] += (byte)((subchannel[pos + 6] & 0x04) >> 1);
                subBuf[pos + 6] += (byte)(subchannel[pos + 6] & 0x02);
                subBuf[pos + 7] += (byte)((subchannel[pos + 6] & 0x01) << 1);

                // W
                subBuf[pos + 0] += (byte)((subchannel[pos + 7] & 0x80) >> 7);
                subBuf[pos + 1] += (byte)((subchannel[pos + 7] & 0x40) >> 6);
                subBuf[pos + 2] += (byte)((subchannel[pos + 7] & 0x20) >> 5);
                subBuf[pos + 3] += (byte)((subchannel[pos + 7] & 0x10) >> 4);
                subBuf[pos + 4] += (byte)((subchannel[pos + 7] & 0x08) >> 3);
                subBuf[pos + 5] += (byte)((subchannel[pos + 7] & 0x04) >> 2);
                subBuf[pos + 6] += (byte)((subchannel[pos + 7] & 0x02) >> 1);
                subBuf[pos + 7] += (byte)(subchannel[pos + 7] & 0x01);

                pos += 8;
            }

            return subBuf;
        }

        public static byte[] Deinterleave(byte[] subchannel)
        {
            byte[] subBuf = new byte[subchannel.Length];
            int    pos    = 0;

            while(pos < subchannel.Length)
            {
                // P
                subBuf[pos + 0] += (byte)((subchannel[pos + 0] & 0x80) >> 0);
                subBuf[pos + 0] += (byte)((subchannel[pos + 1] & 0x80) >> 1);
                subBuf[pos + 0] += (byte)((subchannel[pos + 2] & 0x80) >> 2);
                subBuf[pos + 0] += (byte)((subchannel[pos + 3] & 0x80) >> 3);
                subBuf[pos + 0] += (byte)((subchannel[pos + 4] & 0x80) >> 4);
                subBuf[pos + 0] += (byte)((subchannel[pos + 5] & 0x80) >> 5);
                subBuf[pos + 0] += (byte)((subchannel[pos + 6] & 0x80) >> 6);
                subBuf[pos + 0] += (byte)((subchannel[pos + 7] & 0x80) >> 7);

                // Q
                subBuf[pos + 1] += (byte)((subchannel[pos + 0] & 0x40) >> 1);
                subBuf[pos + 1] += (byte)((subchannel[pos + 1] & 0x40) >> 0);
                subBuf[pos + 1] += (byte)((subchannel[pos + 2] & 0x40) >> 1);
                subBuf[pos + 1] += (byte)((subchannel[pos + 3] & 0x40) >> 2);
                subBuf[pos + 1] += (byte)((subchannel[pos + 4] & 0x40) >> 3);
                subBuf[pos + 1] += (byte)((subchannel[pos + 5] & 0x40) >> 4);
                subBuf[pos + 1] += (byte)((subchannel[pos + 6] & 0x40) >> 5);
                subBuf[pos + 1] += (byte)((subchannel[pos + 7] & 0x40) >> 6);

                // R
                subBuf[pos + 2] += (byte)((subchannel[pos + 0] & 0x20) >> 2);
                subBuf[pos + 2] += (byte)((subchannel[pos + 1] & 0x20) >> 1);
                subBuf[pos + 2] += (byte)((subchannel[pos + 2] & 0x20) >> 0);
                subBuf[pos + 2] += (byte)((subchannel[pos + 3] & 0x20) >> 1);
                subBuf[pos + 2] += (byte)((subchannel[pos + 4] & 0x20) >> 2);
                subBuf[pos + 2] += (byte)((subchannel[pos + 5] & 0x20) >> 3);
                subBuf[pos + 2] += (byte)((subchannel[pos + 6] & 0x20) >> 4);
                subBuf[pos + 2] += (byte)((subchannel[pos + 7] & 0x20) >> 5);

                // S
                subBuf[pos + 3] += (byte)((subchannel[pos + 0] & 0x10) >> 3);
                subBuf[pos + 3] += (byte)((subchannel[pos + 1] & 0x10) >> 2);
                subBuf[pos + 3] += (byte)((subchannel[pos + 2] & 0x10) >> 1);
                subBuf[pos + 3] += (byte)((subchannel[pos + 3] & 0x10) >> 0);
                subBuf[pos + 3] += (byte)((subchannel[pos + 4] & 0x10) >> 1);
                subBuf[pos + 3] += (byte)((subchannel[pos + 5] & 0x10) >> 2);
                subBuf[pos + 3] += (byte)((subchannel[pos + 6] & 0x10) >> 3);
                subBuf[pos + 3] += (byte)((subchannel[pos + 7] & 0x10) >> 4);

                // T
                subBuf[pos + 4] += (byte)((subchannel[pos + 0] & 0x8) >> 4);
                subBuf[pos + 4] += (byte)((subchannel[pos + 1] & 0x8) >> 3);
                subBuf[pos + 4] += (byte)((subchannel[pos + 2] & 0x8) >> 2);
                subBuf[pos + 4] += (byte)((subchannel[pos + 3] & 0x8) >> 1);
                subBuf[pos + 4] += (byte)((subchannel[pos + 4] & 0x8) >> 0);
                subBuf[pos + 4] += (byte)((subchannel[pos + 5] & 0x8) >> 1);
                subBuf[pos + 4] += (byte)((subchannel[pos + 6] & 0x8) >> 2);
                subBuf[pos + 4] += (byte)((subchannel[pos + 7] & 0x8) >> 3);

                // U
                subBuf[pos + 5] += (byte)((subchannel[pos + 0] & 0x4) >> 5);
                subBuf[pos + 5] += (byte)((subchannel[pos + 1] & 0x4) >> 4);
                subBuf[pos + 5] += (byte)((subchannel[pos + 2] & 0x4) >> 3);
                subBuf[pos + 5] += (byte)((subchannel[pos + 3] & 0x4) >> 2);
                subBuf[pos + 5] += (byte)((subchannel[pos + 4] & 0x4) >> 1);
                subBuf[pos + 5] += (byte)((subchannel[pos + 5] & 0x4) >> 0);
                subBuf[pos + 5] += (byte)((subchannel[pos + 6] & 0x4) >> 1);
                subBuf[pos + 5] += (byte)((subchannel[pos + 7] & 0x4) >> 2);

                // V
                subBuf[pos + 6] += (byte)((subchannel[pos + 0] & 0x2) >> 6);
                subBuf[pos + 6] += (byte)((subchannel[pos + 1] & 0x2) >> 5);
                subBuf[pos + 6] += (byte)((subchannel[pos + 2] & 0x2) >> 4);
                subBuf[pos + 6] += (byte)((subchannel[pos + 3] & 0x2) >> 3);
                subBuf[pos + 6] += (byte)((subchannel[pos + 4] & 0x2) >> 2);
                subBuf[pos + 6] += (byte)((subchannel[pos + 5] & 0x2) >> 1);
                subBuf[pos + 6] += (byte)((subchannel[pos + 6] & 0x2) >> 0);
                subBuf[pos + 6] += (byte)((subchannel[pos + 7] & 0x2) >> 1);

                // W
                subBuf[pos + 7] += (byte)((subchannel[pos + 0] & 0x1) >> 7);
                subBuf[pos + 7] += (byte)((subchannel[pos + 1] & 0x1) >> 6);
                subBuf[pos + 7] += (byte)((subchannel[pos + 2] & 0x1) >> 5);
                subBuf[pos + 7] += (byte)((subchannel[pos + 3] & 0x1) >> 4);
                subBuf[pos + 7] += (byte)((subchannel[pos + 4] & 0x1) >> 3);
                subBuf[pos + 7] += (byte)((subchannel[pos + 5] & 0x1) >> 2);
                subBuf[pos + 7] += (byte)((subchannel[pos + 6] & 0x1) >> 1);
                subBuf[pos + 7] += (byte)((subchannel[pos + 7] & 0x1) >> 0);

                pos += 8;
            }

            return subBuf;
        }

        public static string PrettifyQ(byte[] subBuf, bool bcd, long lba, bool corruptedPause, bool pause, bool rwEmpty)
        {
            CRC16CCITTContext.Data(subBuf, 10, out byte[] crc);

            bool   crcOk  = crc[0] == subBuf[10] && crc[1] == subBuf[11];
            long   minute = (lba + 150)          / 4500;
            long   second = ((lba + 150) % 4500) / 75;
            long   frame  = (lba + 150) % 4500   % 75;
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

            int  qPos = ((subBuf[3] * 60 * 75) + (subBuf[4] * 75) + subBuf[5]) - 150;
            byte pmin = subBuf[7];
            byte psec = subBuf[8];

            int  qStart  = ((subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9]) - 150;
            int  nextPos = ((subBuf[3] * 60 * 75) + (subBuf[4] * 75) + subBuf[5]) - 150;
            byte zero    = subBuf[6];
            int  maxOut  = ((subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9]) - 150;
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
                            $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} MCN: {subBuf[1]:X2}{subBuf[2]:X2}{subBuf[3]:X2}{subBuf[4]:X2}{subBuf[5]:X2}{subBuf[6]:X2}{subBuf[7] / 8:X} frame {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}";
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
                $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} MCN: {subBuf[1]:X2}{subBuf[2]:X2}{subBuf[3]:X2}{subBuf[4]:X2}{subBuf[5]:X2}{subBuf[6]:X2}{subBuf[7] / 8:X} frame {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}",
                3 =>
                $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q mode {adr} ISRC: {_isrcTable[subBuf[1] / 4]}{_isrcTable[((subBuf[1] & 3) * 16) + (subBuf[2] / 16)]}{_isrcTable[((subBuf[2] & 0xF) * 4) + (subBuf[3] / 64)]}{_isrcTable[subBuf[3] & 0x3F]}{_isrcTable[subBuf[4] / 4]}{subBuf[5]:X2}{subBuf[6]:X2}{subBuf[7]:X2}{subBuf[8] / 16:X2} frame {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}",
                _ =>
                $"{minute:D2}:{second:D2}:{frame:D2} - LBA {lba,6}: {area} area, {(corruptedPause ? "corrupted pause" : pause ? "pause" : "not pause")}, {controlInfo}, {copy}, Q: {subBuf[0]:X2} {subBuf[1]:X2} {subBuf[2]:X2} {subBuf[3]:X2} {subBuf[4]:X2} {subBuf[5]:X2} {subBuf[6]:X2} {subBuf[7]:X2} {subBuf[8]:X2} {subBuf[9]:X2} CRC 0x{subBuf[10]:X2}{subBuf[11]:X2} ({(crcOk ? "OK" : "BAD")}), R-W {(rwEmpty ? "empty" : "not empty")}"
            };
        }
    }
}