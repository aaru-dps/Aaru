using System;
using System.IO;
using Aaru.Decoders.CD;

namespace Aaru.Core.Logging
{
    public class SubchannelLog
    {
        const    int          _subSize = 96;
        readonly bool         _bcd;
        readonly StreamWriter _logSw;

        /// <summary>Initializes the dump log</summary>
        /// <param name="outputFile">Output log file</param>
        public SubchannelLog(string outputFile, bool bcd)
        {
            if(string.IsNullOrEmpty(outputFile))
                return;

            _bcd = bcd;

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
            if(subchannel.Length / _subSize != blocks)
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

            for(uint block = 0; block < blocks; block++)
            {
                bool rwEmpty = true;

                if(raw)
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

                _logSw.WriteLine(Subchannel.PrettifyQ(subBuf, _bcd, startingLba + block, corruptedPause, pause,
                                                      rwEmpty));
            }

            _logSw.Flush();
        }
    }
}