// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SubchannelLog.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.Decoders.CD;

namespace Aaru.Core.Logging;

/// <summary>Logs subchannel data</summary>
public class SubchannelLog
{
    const    int          SUB_SIZE = 96;
    readonly bool         _bcd;
    readonly StreamWriter _logSw;

    /// <summary>Initializes the subchannel log</summary>
    /// <param name="outputFile">Output log file</param>
    /// <param name="bcd">Drive returns subchannel in BCD format</param>
    public SubchannelLog(string outputFile, bool bcd)
    {
        if(string.IsNullOrEmpty(outputFile))
            return;

        _bcd = bcd;

        _logSw = new StreamWriter(outputFile, true);

        _logSw.WriteLine(Localization.Core.Start_subchannel_logging_at_0, DateTime.Now);
        _logSw.WriteLine(Localization.Core.Log_section_separator);
        _logSw.Flush();
    }

    /// <summary>Finishes and closes the subchannel log</summary>
    public void Close()
    {
        _logSw.WriteLine(Localization.Core.Log_section_separator);
        _logSw.WriteLine(Localization.Core.End_logging_at_0, DateTime.Now);
        _logSw.Close();
    }

    /// <summary>Logs an entry to the subchannel log</summary>
    /// <param name="subchannel">Subchannel data</param>
    /// <param name="raw">Set to <c>true</c> if the subchannel data is raw</param>
    /// <param name="startingLba">First LBA read from drive to retrieve the data</param>
    /// <param name="blocks">Number of blocks read</param>
    /// <param name="generated">Set to <c>true</c> if the subchannel has been generated, <c>false</c> if read from media</param>
    /// <param name="fixed">Set to <c>true</c> if the subchannel has been fixed, <c>false</c> if as is</param>
    public void WriteEntry(byte[] subchannel, bool raw, long startingLba, uint blocks, bool generated, bool @fixed)
    {
        if(subchannel.Length / SUB_SIZE != blocks)
        {
            _logSw.WriteLine(Localization.Core.Data_length_is_invalid);
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
                    if((r[i] == 0    && s[i] == 0    && t[i] == 0    && u[i] == 0    && v[i] == 0    && w[i] == 0) ||
                       (r[i] == 0xFF && s[i] == 0xFF && t[i] == 0xFF && u[i] == 0xFF && v[i] == 0xFF && w[i] == 0xFF))
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

            string prettyQ = Subchannel.PrettifyQ(subBuf, generated || _bcd, startingLba + block, corruptedPause, pause,
                                                  rwEmpty);

            if(generated)
                prettyQ += Localization.Core._GENERATED;
            else if(@fixed)
                prettyQ += Localization.Core._FIXED;

            _logSw.WriteLine(prettyQ);
        }

        _logSw.Flush();
    }

    /// <summary>Logs message indicating the P subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WritePFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_P_subchannel_using_weight_average);

    /// <summary>Logs message indicating the R-W subchannels have been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteRwFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_R_W_subchannels_writing_empty_data);

    /// <summary>Logs message indicating the ADR field of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQAdrFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_ADR);

    /// <summary>Logs message indicating the CONTROL field of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQCtrlFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_CONTROL);

    /// <summary>Logs message indicating the ZERO field of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQZeroFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_ZERO);

    /// <summary>Logs message indicating the TNO field of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQTnoFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_TNO);

    /// <summary>Logs message indicating the INDEX field of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQIndexFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_INDEX);

    /// <summary>Logs message indicating the relative position of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQRelPosFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_RELATIVE_POSITION);

    /// <summary>Logs message indicating the absolute position of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQAbsPosFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_ABSOLUTE_POSITION);

    /// <summary>Logs message indicating the CRC of the Q subchannel has been fixed</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQCrcFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_correct_CRC);

    /// <summary>Logs message indicating the the Q subchannel has been fixed with a known good MCN</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQMcnFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_known_good_MCN);

    /// <summary>Logs message indicating the the Q subchannel has been fixed with a known good ISRC</summary>
    /// <param name="lba">LBA fix belongs to</param>
    public void WriteQIsrcFix(long lba) =>
        WriteMessageWithPosition(lba, Localization.Core.fixed_Q_subchannel_with_known_good_ISRC);

    /// <summary>Logs a message with a specified position</summary>
    /// <param name="lba">LBA position</param>
    /// <param name="message">Message to log</param>
    public void WriteMessageWithPosition(long lba, string message)
    {
        long   minute = (lba + 150)        / 4500;
        long   second = (lba + 150) % 4500 / 75;
        long   frame  = (lba + 150)        % 4500 % 75;
        string area   = lba < 0 ? Localization.Core.Lead_In : Localization.Core.Program;

        _logSw.WriteLine(Localization.Core._0_1_2_LBA_3_4_area_5, minute, second, frame, lba, area, message);
        _logSw.Flush();
    }
}