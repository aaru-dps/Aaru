// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dump.cs
// Author(s)      : Rebecca Wallander <sakcheen+github@gmail.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     SCSI read commands related to Content Scrambling System.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2020-2022 Rebecca Wallander
// ****************************************************************************/

namespace Aaru.Decryption.DVD;

using System;
using System.Linq;
using Aaru.Console;
using Aaru.Decoders.DVD;
using Aaru.Devices;

public sealed class Dump
{
    const    byte   KEY_SIZE       = 5;
    const    byte   CHALLENGE_SIZE = 2 * KEY_SIZE;
    readonly Device _dev;

    public Dump(Device dev)
    {
        _dev   = dev;
        BusKey = Array.Empty<byte>();
        Agid   = 0;
    }

    public byte   Agid   { get; private set; }
    public byte[] BusKey { get; private set; }

    /// <summary>Returns the Authentication Success Flag of the logical unit.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the Authentication Success Flag will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadAsf(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, uint timeout,
                        out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[8];

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.Asf ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device", "REPORT ASF (AGID: {1}, Sense: {2}, Last Error: {3}) took {0} ms.",
                                   duration, Agid, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Returns the Regional Playback Control State of the logical unit.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadRpc(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, uint timeout,
                        out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[8];

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.RpcState ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device", "REPORT ASF (AGID: {1}, Sense: {2}, Last Error: {3}) took {0} ms.",
                                   duration, Agid, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Invalidates an Authentication Grant ID.</summary>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool InvalidateAgid(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, uint timeout,
                               out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = Array.Empty<byte>();

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.InvalidateAgid ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device",
                                   "INVALIDATE AGID (AGID: {1}, Sense: {2}, Last Error: {3}) took {0} ms.", duration,
                                   Agid, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Returns a valid Authentication Grant ID for CSS/CPPM.</summary>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool ReportAgidCssCppm(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, uint timeout,
                                  out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[8];

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.AgidForCssCppm ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device",
                                   "REPORT AGID CSS/CPPM (AGID: {1}, Sense: {2}, Last Error: {3}) took {0} ms.",
                                   duration, Agid, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Returns KEY1 from the logical unit.</summary>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool ReportKey1(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, uint timeout,
                           out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[12];

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.Key1 ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device", "REPORT KEY1 (AGID: {1}, Sense: {2}, Last Error: {3}) took {0} ms.",
                                   duration, Agid, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Returns the challenge from the logical unit.</summary>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool ReportChallenge(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, uint timeout,
                                out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[16];

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.ChallengeKey ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device",
                                   "REPORT CHALLENGE (AGID: {1}, Sense: {2}, Last Error: {3}) took {0} ms.", duration,
                                   Agid, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Send a challenge to the logical unit.</summary>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="challengeKey">The challenge; can be any 10 bytes.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool SendChallenge(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, byte[] challengeKey,
                              uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[16];

        cdb[0]     = (byte)ScsiCommands.SendKey;
        cdb[7]     = (byte)keyClass;
        cdb[8]     = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]     = (byte)(buffer.Length & 0xFF);
        cdb[10]    = (byte)((byte)CssSendKeyFormat.ChallengeKey ^ ((Agid & 0x03) << 6));
        buffer[0]  = (byte)(((buffer.Length - 2) & 0xFF00) >> 8);
        buffer[1]  = (byte)((buffer.Length - 2) & 0xFF);
        buffer[4]  = challengeKey[9];
        buffer[5]  = challengeKey[8];
        buffer[6]  = challengeKey[7];
        buffer[7]  = challengeKey[6];
        buffer[8]  = challengeKey[5];
        buffer[9]  = challengeKey[4];
        buffer[10] = challengeKey[3];
        buffer[11] = challengeKey[2];
        buffer[12] = challengeKey[1];
        buffer[13] = challengeKey[0];

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                             out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device",
                                   "SEND CHALLENGE (AGID: {1}, Challenge {2}, Sense: {3}, Last Error: {4}) took {0} ms.",
                                   duration, Agid, challengeKey, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Send KEY2 to the logical unit.</summary>
    /// <param name="buffer">Buffer where the Regional Playback Control State will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="key2">The KEY2 message.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool SendKey2(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, byte[] key2, uint timeout,
                         out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[12];

        cdb[0]    = (byte)ScsiCommands.SendKey;
        cdb[7]    = (byte)keyClass;
        cdb[8]    = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]    = (byte)(buffer.Length & 0xFF);
        cdb[10]   = (byte)((byte)CssSendKeyFormat.Key2 ^ ((Agid & 0x03) << 6));
        buffer[0] = (byte)(((buffer.Length - 2) & 0xFF00) >> 8);
        buffer[1] = (byte)((buffer.Length - 2) & 0xFF);
        buffer[4] = key2[4];
        buffer[5] = key2[3];
        buffer[6] = key2[2];
        buffer[7] = key2[1];
        buffer[8] = key2[0];

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                             out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device",
                                   "SEND CHALLENGE (AGID: {1}, KEY2 {2}, Sense: {3}, Last Error: {4}) took {0} ms.",
                                   duration, Agid, key2, sense, _dev.LastError);

        return sense;
    }

    /// <summary>Returns the encrypted disc key of the MMC logical unit</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the bus key will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadDiscKey(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[2052];

        cdb[0]  = (byte)ScsiCommands.ReadDiscStructure;
        cdb[1]  = (byte)MmcDiscStructureMediaType.Dvd & 0x0F;
        cdb[6]  = 0;
        cdb[7]  = (byte)MmcDiscStructureFormat.DiscKey;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((Agid & 0x03) << 6);

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        return sense;
    }

    /// <summary>Returns the bus key of the MMC logical unit</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the bus key will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="protectionType">The type of protection the logical unit reports</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    public bool ReadBusKey(out byte[] buffer, out byte[] senseBuffer, CopyrightType protectionType, uint timeout,
                           out double duration)
    {
        duration    = 0;
        buffer      = Array.Empty<byte>();
        senseBuffer = new byte[64];

        var  sense     = false;
        var  challenge = new byte[CHALLENGE_SIZE];
        var  key1      = new byte[KEY_SIZE];
        byte variant   = 0;

        for(byte i = 0; i < 4; i++)
        {
            // Invalidate AGID to reset any previous drive communications
            Agid = i;

            sense = InvalidateAgid(out buffer, out senseBuffer, DvdCssKeyClass.DvdCssCppmOrCprm, timeout, out duration);

            switch(protectionType)
            {
                // Get AGID
                case CopyrightType.CSS:
                    sense = ReportAgidCssCppm(out buffer, out senseBuffer, DvdCssKeyClass.DvdCssCppmOrCprm, timeout,
                                              out duration);

                    break;
                case CopyrightType.CPRM: throw new NotImplementedException();
            }

            if(sense)
                continue;

            Agid = (byte)(buffer[7] >> 6);

            break;
        }

        if(sense)
            return true;

        for(byte i = 0; i < CHALLENGE_SIZE; i++)
            challenge[i] = i;

        sense = SendChallenge(out buffer, out senseBuffer, DvdCssKeyClass.DvdCssCppmOrCprm, challenge, timeout,
                              out duration);

        if(sense)
            return true;

        sense = ReportKey1(out buffer, out senseBuffer, DvdCssKeyClass.DvdCssCppmOrCprm, timeout, out duration);

        if(sense)
            return true;

        for(byte i = 0; i < KEY_SIZE; i++)
            key1[i] = buffer[8 - i];

        for(byte i = 0; i < 32; i++)
        {
            CSS.EncryptKey(DvdCssKeyType.Key1, i, challenge, out byte[] keyCheck);

            if(key1.SequenceEqual(keyCheck))
            {
                variant = i;

                break;
            }

            if(i < 31)
                continue;

            senseBuffer = Array.Empty<byte>();

            return true;
        }

        sense = ReportChallenge(out buffer, out senseBuffer, DvdCssKeyClass.DvdCssCppmOrCprm, timeout, out duration);

        if(sense)
            return true;

        for(byte i = 0; i < CHALLENGE_SIZE; i++)
            challenge[i] = buffer[13 - i];

        CSS.EncryptKey(DvdCssKeyType.Key2, variant, challenge, out byte[] key2);

        sense = SendKey2(out buffer, out senseBuffer, DvdCssKeyClass.DvdCssCppmOrCprm, key2, timeout, out duration);

        if(sense)
            return true;

        key1.CopyTo(challenge, 0);
        key2.CopyTo(challenge, key1.Length);
        CSS.EncryptKey(DvdCssKeyType.BusKey, variant, challenge, out buffer);

        BusKey = buffer;

        return false;
    }

    /// <summary>Reads a title key for a sector on the disc.</summary>
    /// <param name="buffer">Buffer where the bus key will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="keyClass">Key class.</param>
    /// <param name="address">The sector address to get the key for.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool ReadTitleKey(out byte[] buffer, out byte[] senseBuffer, DvdCssKeyClass keyClass, ulong address,
                             uint timeout, out double duration)
    {
        // We need to be in a bus key state to read title keys. Only CSS has title keys.
        ReadBusKey(out buffer, out senseBuffer, CopyrightType.CSS, timeout, out duration);

        BusKey = buffer;

        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[12];

        cdb[0]  = (byte)ScsiCommands.ReportKey;
        cdb[2]  = (byte)((address & 0xFF000000) >> 24);
        cdb[3]  = (byte)((address & 0xFF0000)   >> 16);
        cdb[4]  = (byte)((address & 0xFF00)     >> 8);
        cdb[5]  = (byte)(address & 0xFF);
        cdb[7]  = (byte)keyClass;
        cdb[8]  = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[9]  = (byte)(buffer.Length & 0xFF);
        cdb[10] = (byte)((byte)CssReportKeyFormat.TitleKey ^ ((Agid & 0x03) << 6));

        _dev.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        AaruConsole.DebugWriteLine("SCSI Device", "GET TITLE KEY (AGID: {1}, LBA: {2}, Sense: {3}) took {0} ms.",
                                   duration, Agid, address, sense);

        return sense;
    }
}