using System;
using System.IO;
using Aaru.Checksums;

namespace Aaru.Core.Logging
{
    public class SubchannelLog
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
        readonly StreamWriter _logSw;
        bool?                 _bcd;

        /// <summary>Initializes the dump log</summary>
        /// <param name="outputFile">Output log file</param>
        public SubchannelLog(string outputFile)
        {
            if(string.IsNullOrEmpty(outputFile))
                return;

            _logSw = new StreamWriter(outputFile, true);

            _logSw.WriteLine("Start subchannel logging at {0}", DateTime.Now);
            _logSw.WriteLine("######################################################");
            _logSw.Flush();
        }

        /// <summary>Finishes and closes the dump log</summary>
        public void Close()
        {
            _logSw.WriteLine("######################################################");
            _logSw.WriteLine("End logging at {0}", DateTime.Now);
            _logSw.Close();
        }

        public void WriteEntry(byte[] subchannel, bool raw, long startingLba, uint blocks)
        {
            int subSize = raw ? 96 : 16;

            if(subchannel.Length / subSize != blocks)
            {
                _logSw.WriteLine("Data length is invalid!");
                _logSw.Flush();

                return;
            }

            int[] p = new int[subchannel.Length / 8];
            int[] q = new int[subchannel.Length / 8];
            int[] r = new int[subchannel.Length / 8];
            int[] s = new int[subchannel.Length / 8];
            int[] t = new int[subchannel.Length / 8];
            int[] u = new int[subchannel.Length / 8];
            int[] v = new int[subchannel.Length / 8];
            int[] w = new int[subchannel.Length / 8];

            if(raw)
            {
                for(int i = 0; i < subchannel.Length; i += 8)
                {
                    p[i / 8] =  subchannel[i] & 0x80;
                    p[i / 8] += (subchannel[i + 1] & 0x80) >> 1;
                    p[i / 8] += (subchannel[i + 2] & 0x80) >> 2;
                    p[i / 8] += (subchannel[i + 3] & 0x80) >> 3;
                    p[i / 8] += (subchannel[i + 4] & 0x80) >> 4;
                    p[i / 8] += (subchannel[i + 5] & 0x80) >> 5;
                    p[i / 8] += (subchannel[i + 6] & 0x80) >> 6;
                    p[i / 8] += (subchannel[i + 7] & 0x80) >> 7;

                    q[i / 8] =  (subchannel[i] & 0x40) << 1;
                    q[i / 8] += subchannel[i + 1] & 0x40;
                    q[i / 8] += (subchannel[i + 2] & 0x40) >> 1;
                    q[i / 8] += (subchannel[i + 3] & 0x40) >> 2;
                    q[i / 8] += (subchannel[i + 4] & 0x40) >> 3;
                    q[i / 8] += (subchannel[i + 5] & 0x40) >> 4;
                    q[i / 8] += (subchannel[i + 6] & 0x40) >> 5;
                    q[i / 8] += (subchannel[i + 7] & 0x40) >> 6;

                    r[i / 8] =  (subchannel[i]     & 0x20) << 2;
                    r[i / 8] += (subchannel[i + 1] & 0x20) << 1;
                    r[i / 8] += subchannel[i + 2] & 0x20;
                    r[i / 8] += (subchannel[i + 3] & 0x20) >> 1;
                    r[i / 8] += (subchannel[i + 4] & 0x20) >> 2;
                    r[i / 8] += (subchannel[i + 5] & 0x20) >> 3;
                    r[i / 8] += (subchannel[i + 6] & 0x20) >> 4;
                    r[i / 8] += (subchannel[i + 7] & 0x20) >> 5;

                    s[i / 8] =  (subchannel[i]     & 0x10) << 3;
                    s[i / 8] += (subchannel[i + 1] & 0x10) << 2;
                    s[i / 8] += (subchannel[i + 2] & 0x10) << 1;
                    s[i / 8] += subchannel[i + 3] & 0x10;
                    s[i / 8] += (subchannel[i + 4] & 0x10) >> 1;
                    s[i / 8] += (subchannel[i + 5] & 0x10) >> 2;
                    s[i / 8] += (subchannel[i + 6] & 0x10) >> 3;
                    s[i / 8] += (subchannel[i + 7] & 0x10) >> 4;

                    t[i / 8] =  (subchannel[i]     & 0x08) << 4;
                    t[i / 8] += (subchannel[i + 1] & 0x08) << 3;
                    t[i / 8] += (subchannel[i + 2] & 0x08) << 2;
                    t[i / 8] += (subchannel[i + 3] & 0x08) << 1;
                    t[i / 8] += subchannel[i + 4] & 0x08;
                    t[i / 8] += (subchannel[i + 5] & 0x08) >> 1;
                    t[i / 8] += (subchannel[i + 6] & 0x08) >> 2;
                    t[i / 8] += (subchannel[i + 7] & 0x08) >> 3;

                    u[i / 8] =  (subchannel[i]     & 0x04) << 5;
                    u[i / 8] += (subchannel[i + 1] & 0x04) << 4;
                    u[i / 8] += (subchannel[i + 2] & 0x04) << 3;
                    u[i / 8] += (subchannel[i + 3] & 0x04) << 2;
                    u[i / 8] += (subchannel[i + 4] & 0x04) << 1;
                    u[i / 8] += subchannel[i + 5] & 0x04;
                    u[i / 8] += (subchannel[i + 6] & 0x04) >> 1;
                    u[i / 8] += (subchannel[i + 7] & 0x04) >> 2;

                    v[i / 8] =  (subchannel[i]     & 0x02) << 6;
                    v[i / 8] += (subchannel[i + 1] & 0x02) << 5;
                    v[i / 8] += (subchannel[i + 2] & 0x02) << 4;
                    v[i / 8] += (subchannel[i + 3] & 0x02) << 3;
                    v[i / 8] += (subchannel[i + 4] & 0x02) << 2;
                    v[i / 8] += (subchannel[i + 5] & 0x02) << 1;
                    v[i / 8] += subchannel[i + 6] & 0x02;
                    v[i / 8] += (subchannel[i + 7] & 0x02) >> 1;

                    w[i / 8] =  (subchannel[i]     & 0x01) << 7;
                    w[i / 8] += (subchannel[i + 1] & 0x01) << 6;
                    w[i / 8] += (subchannel[i + 2] & 0x01) << 5;
                    w[i / 8] += (subchannel[i + 3] & 0x01) << 4;
                    w[i / 8] += (subchannel[i + 4] & 0x01) << 3;
                    w[i / 8] += (subchannel[i + 5] & 0x01) << 2;
                    w[i / 8] += (subchannel[i + 6] & 0x01) << 1;
                    w[i / 8] += subchannel[i + 7] & 0x01;
                }
            }
            else
            {
                r = null;
                s = null;
                t = null;
                u = null;
                v = null;
                w = null;
                int pPos = 0;
                int qPos = 0;

                for(int i = 0; i < subchannel.Length; i += 16)
                {
                    q[qPos++] = subchannel[i + 0];
                    q[qPos++] = subchannel[i + 1];
                    q[qPos++] = subchannel[i + 2];
                    q[qPos++] = subchannel[i + 3];
                    q[qPos++] = subchannel[i + 4];
                    q[qPos++] = subchannel[i + 5];
                    q[qPos++] = subchannel[i + 6];
                    q[qPos++] = subchannel[i + 7];
                    q[qPos++] = subchannel[i + 8];
                    q[qPos++] = subchannel[i + 9];
                    q[qPos++] = subchannel[i + 10];
                    q[qPos++] = subchannel[i + 11];

                    if((subchannel[i + 15] & 0x80) <= 0)
                        continue;

                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                    p[pPos++] = 0xFF;
                }
            }

            if(_bcd is null)
            {
                _bcd = (q[9] & 0x10) > 0;

                _logSw.WriteLine(_bcd switch
                {
                    true  => "Subchannel is BCD",
                    false => "Subchannel is not BCD"
                });
            }

            for(uint block = 0; block < blocks; block++)
            {
                bool rwEmpty = true;

                if(r != null)
                {
                    for(uint i = 12 * block; i < (12 * block) + 12; i++)
                    {
                        if(r[i] == 0 &&
                           s[i] == 0 &&
                           t[i] == 0 &&
                           u[i] == 0 &&
                           v[i] == 0 &&
                           w[i] == 0)
                            continue;

                        rwEmpty = false;

                        break;
                    }
                }

                bool corruptedPause = false;
                bool pause          = false;

                for(int i = 0; i < 12; i++)
                {
                    if(p[i] == 0 ||
                       p[i] == 0xFF)
                        continue;

                    corruptedPause = true;

                    break;
                }

                if(!corruptedPause)
                    pause = p[0] == 1;

                byte[] subBuf = new byte[12];
                subBuf[0]  = (byte)q[0  + (block * 12)];
                subBuf[1]  = (byte)q[1  + (block * 12)];
                subBuf[2]  = (byte)q[2  + (block * 12)];
                subBuf[3]  = (byte)q[3  + (block * 12)];
                subBuf[4]  = (byte)q[4  + (block * 12)];
                subBuf[5]  = (byte)q[5  + (block * 12)];
                subBuf[6]  = (byte)q[6  + (block * 12)];
                subBuf[7]  = (byte)q[7  + (block * 12)];
                subBuf[8]  = (byte)q[8  + (block * 12)];
                subBuf[9]  = (byte)q[9  + (block * 12)];
                subBuf[10] = (byte)q[10 + (block * 12)];
                subBuf[11] = (byte)q[11 + (block * 12)];

                _logSw.WriteLine(PrettifyQ(subBuf, _bcd == true, startingLba + block, corruptedPause, pause, rwEmpty));
            }

            _logSw.Flush();
        }

        static void BinaryToBcdQ(byte[] q)
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

        static void BcdToBinaryQ(byte[] q)
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

        static string PrettifyQ(byte[] subBuf, bool bcd, long lba, bool corruptedPause, bool pause, bool rwEmpty)
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