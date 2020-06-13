// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Subchannel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles CompactDisc subchannel data.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Devices;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping
{
    partial class Dump
    {
        public static bool SupportsRwSubchannel(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus)
        {
            dumpLog?.WriteLine("Checking if drive supports full raw subchannel reading...");
            updateStatus?.Invoke("Checking if drive supports full raw subchannel reading...");

            return !dev.ReadCd(out _, out _, 0, 2352 + 96, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw,
                               dev.Timeout, out _);
        }

        public static bool SupportsPqSubchannel(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus)
        {
            dumpLog?.WriteLine("Checking if drive supports PQ subchannel reading...");
            updateStatus?.Invoke("Checking if drive supports PQ subchannel reading...");

            return !dev.ReadCd(out _, out _, 0, 2352 + 16, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16,
                               dev.Timeout, out _);
        }

        // Return true if indexes have changed
        bool WriteSubchannelToImage(MmcSubchannel supportedSubchannel, MmcSubchannel desiredSubchannel, byte[] sub,
                                    ulong sectorAddress, uint length, SubchannelLog subLog,
                                    Dictionary<byte, string> isrcs, byte currentTrack, ref string mcn, Track[] tracks,
                                    ExtentsInt subchannelExtents)
        {
            if(supportedSubchannel == MmcSubchannel.Q16)
                sub = Subchannel.ConvertQToRaw(sub);

            if(!_fixSubchannelPosition &&
               desiredSubchannel != MmcSubchannel.None)
                _outputPlugin.WriteSectorsTag(sub, sectorAddress, length, SectorTagType.CdSectorSubchannel);

            subLog?.WriteEntry(sub, supportedSubchannel == MmcSubchannel.Raw, (long)sectorAddress, length);

            byte[] deSub = Subchannel.Deinterleave(sub);

            bool indexesChanged = CheckIndexesFromSubchannel(deSub, isrcs, currentTrack, ref mcn, tracks);

            if(!_fixSubchannelPosition ||
               desiredSubchannel == MmcSubchannel.None)
                return indexesChanged;

            int prePos = int.MinValue;

            // Check subchannel
            for(int subPos = 0; subPos < deSub.Length; subPos += 96)
            {
                byte[] q = new byte[12];
                Array.Copy(deSub, subPos + 12, q, 0, 12);

                CRC16CCITTContext.Data(q, 10, out byte[] crc);
                bool crcOk = crc[0] == q[10] && crc[1] == q[11];

                bool pOk     = true;
                int  pWeight = 0;

                bool rwOk = true;

                for(int p = subPos; p < subPos + 12; p++)
                {
                    if(deSub[p] != 0 &&
                       deSub[p] != 255)
                        pOk = false;

                    for(int w = 0; w < 8; w++)
                    {
                        if(((deSub[p] >> w) & 1) > 0)
                            pWeight++;
                    }
                }

                for(int rw = subPos + 24; rw < subPos + 96; rw++)
                    if(deSub[rw] != 0)
                        rwOk = false;

                if(!rwOk)
                {
                    byte[] sectorSub = new byte[96];
                    Array.Copy(sub, subPos, sectorSub, 0, 96);

                    DetectRwPackets(sectorSub, out _, out bool rwPacket, out bool cdtextPacket);

                    // TODO: CD+G reed solomon
                    if(rwPacket && !cdtextPacket)
                        rwOk = true;

                    if(cdtextPacket)
                        rwOk = CheckCdTextPackets(sectorSub);
                }

                // TODO: Fix
                if(!pOk   ||
                   !crcOk ||
                   !rwOk)
                    continue;

                int aPos = int.MinValue;

                byte aframe = (byte)(((q[9] / 16) * 10) + (q[9] & 0x0F));

                if((q[0] & 0x3) == 1)
                {
                    byte amin = (byte)(((q[7] / 16) * 10) + (q[7] & 0x0F));
                    byte asec = (byte)(((q[8] / 16) * 10) + (q[8] & 0x0F));
                    aPos = ((amin * 60 * 75) + (asec * 75) + aframe) - 150;
                }
                else
                {
                    ulong expectedSectorAddress = sectorAddress + (ulong)(subPos / 96) + 150;
                    byte  smin                  = (byte)(expectedSectorAddress / 60 / 75);
                    expectedSectorAddress -= (ulong)(smin * 60 * 75);
                    byte ssec = (byte)(expectedSectorAddress / 75);
                    expectedSectorAddress -= (ulong)(smin * 75);
                    byte sframe = (byte)(expectedSectorAddress - ((ulong)ssec * 75));

                    aPos = ((smin * 60 * 75) + (ssec * 75) + aframe) - 150;

                    // Next second
                    if(aPos < prePos)
                        aPos += 75;
                }

                // TODO: Negative sectors
                if(aPos < 0)
                    continue;

                prePos = aPos;

                byte[] posSub = new byte[96];
                Array.Copy(deSub, subPos, posSub, 0, 96);
                posSub = Subchannel.Interleave(posSub);
                _outputPlugin.WriteSectorTag(posSub, (ulong)aPos, SectorTagType.CdSectorSubchannel);

                subchannelExtents.Remove(aPos);
            }

            return indexesChanged;
        }

        bool CheckIndexesFromSubchannel(byte[] deSub, Dictionary<byte, string> isrcs, byte currentTrack, ref string mcn,
                                        Track[] tracks)
        {
            // Check subchannel
            for(int subPos = 0; subPos < deSub.Length; subPos += 96)
            {
                byte[] q = new byte[12];
                Array.Copy(deSub, subPos + 12, q, 0, 12);

                CRC16CCITTContext.Data(q, 10, out byte[] crc);
                bool crcOk = crc[0] == q[10] && crc[1] == q[11];

                // ISRC
                if((q[0] & 0x3) == 3)
                {
                    string isrc = Subchannel.DecodeIsrc(q);

                    if(isrc == null ||
                       isrc == "000000000000")
                        continue;

                    if(!crcOk)
                        continue;

                    if(!isrcs.ContainsKey(currentTrack))
                    {
                        _dumpLog?.WriteLine($"Found new ISRC {isrc} for track {currentTrack}.");
                        UpdateStatus?.Invoke($"Found new ISRC {isrc} for track {currentTrack}.");
                    }
                    else if(isrcs[currentTrack] != isrc)
                    {
                        _dumpLog?.
                            WriteLine($"ISRC for track {currentTrack} changed from {isrcs[currentTrack]} to {isrc}.");

                        UpdateStatus?.
                            Invoke($"ISRC for track {currentTrack} changed from {isrcs[currentTrack]} to {isrc}.");
                    }

                    isrcs[currentTrack] = isrc;
                }
                else if((q[0] & 0x3) == 2)
                {
                    string newMcn = Subchannel.DecodeMcn(q);

                    if(newMcn == null ||
                       newMcn == "0000000000000")
                        continue;

                    if(!crcOk)
                        continue;

                    if(mcn is null)
                    {
                        _dumpLog?.WriteLine($"Found new MCN {newMcn}.");
                        UpdateStatus?.Invoke($"Found new MCN {newMcn}.");
                    }
                    else if(mcn != newMcn)
                    {
                        _dumpLog?.WriteLine($"MCN changed from {mcn} to {newMcn}.");
                        UpdateStatus?.Invoke($"MCN changed from {mcn} to {newMcn}.");
                    }

                    mcn = newMcn;
                }
                else if((q[0] & 0x3) == 1)
                {
                    // TODO: Indexes

                    // Pregap
                    if(q[2] != 0)
                        continue;

                    if(!crcOk)
                        continue;

                    byte trackNo = (byte)(((q[1] / 16) * 10) + (q[1] & 0x0F));

                    for(int i = 0; i < tracks.Length; i++)
                    {
                        if(tracks[i].TrackSequence != trackNo ||
                           trackNo                 == 1)
                        {
                            continue;
                        }

                        byte pmin   = (byte)(((q[3] / 16) * 10) + (q[3] & 0x0F));
                        byte psec   = (byte)(((q[4] / 16) * 10) + (q[4] & 0x0F));
                        byte pframe = (byte)(((q[5] / 16) * 10) + (q[5] & 0x0F));
                        int  qPos   = (pmin * 60 * 75) + (psec * 75) + pframe;

                        if(tracks[i].TrackPregap >= (ulong)(qPos + 1))
                            continue;

                        tracks[i].TrackPregap      =  (ulong)(qPos + 1);
                        tracks[i].TrackStartSector -= tracks[i].TrackPregap;

                        if(i > 0)
                            tracks[i - 1].TrackEndSector = tracks[i].TrackStartSector - 1;

                        _dumpLog?.WriteLine($"Pregap for track {trackNo} set to {tracks[i].TrackPregap} sectors.");
                        UpdateStatus?.Invoke($"Pregap for track {trackNo} set to {tracks[i].TrackPregap} sectors.");

                        return true;
                    }
                }
            }

            return false;
        }

        void DetectRwPackets(byte[] subchannel, out bool zero, out bool rwPacket, out bool cdtextPacket)
        {
            zero         = false;
            rwPacket     = false;
            cdtextPacket = false;

            byte[] cdTextPack1  = new byte[18];
            byte[] cdTextPack2  = new byte[18];
            byte[] cdTextPack3  = new byte[18];
            byte[] cdTextPack4  = new byte[18];
            byte[] cdSubRwPack1 = new byte[24];
            byte[] cdSubRwPack2 = new byte[24];
            byte[] cdSubRwPack3 = new byte[24];
            byte[] cdSubRwPack4 = new byte[24];

            int i = 0;

            for(int j = 0; j < 18; j++)
            {
                cdTextPack1[j] = (byte)(cdTextPack1[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack1[j] = (byte)(cdTextPack1[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x3F));
            }

            for(int j = 0; j < 18; j++)
            {
                cdTextPack2[j] = (byte)(cdTextPack2[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack2[j] = (byte)(cdTextPack2[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x3F));
            }

            for(int j = 0; j < 18; j++)
            {
                cdTextPack3[j] = (byte)(cdTextPack3[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack3[j] = (byte)(cdTextPack3[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x3F));
            }

            for(int j = 0; j < 18; j++)
            {
                cdTextPack4[j] = (byte)(cdTextPack4[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack4[j] = (byte)(cdTextPack4[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x3F));
            }

            i = 0;

            for(int j = 0; j < 24; j++)
                cdSubRwPack1[j] = (byte)(subchannel[i++] & 0x3F);

            for(int j = 0; j < 24; j++)
                cdSubRwPack2[j] = (byte)(subchannel[i++] & 0x3F);

            for(int j = 0; j < 24; j++)
                cdSubRwPack3[j] = (byte)(subchannel[i++] & 0x3F);

            for(int j = 0; j < 24; j++)
                cdSubRwPack4[j] = (byte)(subchannel[i++] & 0x3F);

            switch(cdSubRwPack1[0])
            {
                case 0x00:
                    zero = true;

                    break;
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x18:
                case 0x38:
                    rwPacket = true;

                    break;
                case 0x14:
                    cdtextPacket = true;

                    break;
            }

            switch(cdSubRwPack2[0])
            {
                case 0x00:
                    zero = true;

                    break;
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x18:
                case 0x38:
                    rwPacket = true;

                    break;
                case 0x14:
                    cdtextPacket = true;

                    break;
            }

            switch(cdSubRwPack3[0])
            {
                case 0x00:
                    zero = true;

                    break;
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x18:
                case 0x38:
                    rwPacket = true;

                    break;
                case 0x14:
                    cdtextPacket = true;

                    break;
            }

            switch(cdSubRwPack4[0])
            {
                case 0x00:
                    zero = true;

                    break;
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x18:
                case 0x38:
                    rwPacket = true;

                    break;
                case 0x14:
                    cdtextPacket = true;

                    break;
            }

            if((cdTextPack1[0] & 0x80) == 0x80)
                cdtextPacket = true;

            if((cdTextPack2[0] & 0x80) == 0x80)
                cdtextPacket = true;

            if((cdTextPack3[0] & 0x80) == 0x80)
                cdtextPacket = true;

            if((cdTextPack4[0] & 0x80) == 0x80)
                cdtextPacket = true;
        }

        bool CheckCdTextPackets(byte[] subchannel)
        {
            byte[] cdTextPack1 = new byte[18];
            byte[] cdTextPack2 = new byte[18];
            byte[] cdTextPack3 = new byte[18];
            byte[] cdTextPack4 = new byte[18];

            int i = 0;

            for(int j = 0; j < 18; j++)
            {
                cdTextPack1[j] = (byte)(cdTextPack1[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack1[j] = (byte)(cdTextPack1[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack1[j] = (byte)(cdTextPack1[j] | (subchannel[i++] & 0x3F));
            }

            for(int j = 0; j < 18; j++)
            {
                cdTextPack2[j] = (byte)(cdTextPack2[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack2[j] = (byte)(cdTextPack2[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack2[j] = (byte)(cdTextPack2[j] | (subchannel[i++] & 0x3F));
            }

            for(int j = 0; j < 18; j++)
            {
                cdTextPack3[j] = (byte)(cdTextPack3[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack3[j] = (byte)(cdTextPack3[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack3[j] = (byte)(cdTextPack3[j] | (subchannel[i++] & 0x3F));
            }

            for(int j = 0; j < 18; j++)
            {
                cdTextPack4[j] = (byte)(cdTextPack4[j] | ((subchannel[i++] & 0x3F) << 2));

                cdTextPack4[j] = (byte)(cdTextPack4[j++] | ((subchannel[i] & 0xC0) >> 4));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j] | ((subchannel[i++] & 0x0F) << 4));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j++] | ((subchannel[i] & 0x3C) >> 2));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j] | ((subchannel[i++] & 0x03) << 6));

                if(j < 18)
                    cdTextPack4[j] = (byte)(cdTextPack4[j] | (subchannel[i++] & 0x3F));
            }

            bool status = true;

            if((cdTextPack1[0] & 0x80) == 0x80)
            {
                ushort cdTextPack1Crc    = BigEndianBitConverter.ToUInt16(cdTextPack1, 16);
                byte[] cdTextPack1ForCrc = new byte[16];
                Array.Copy(cdTextPack1, 0, cdTextPack1ForCrc, 0, 16);
                ushort calculatedCdtp1Crc = CRC16CCITTContext.Calculate(cdTextPack1ForCrc);

                if(cdTextPack1Crc != calculatedCdtp1Crc &&
                   cdTextPack1Crc != 0)
                    status = false;
            }

            if((cdTextPack2[0] & 0x80) == 0x80)
            {
                ushort cdTextPack2Crc    = BigEndianBitConverter.ToUInt16(cdTextPack2, 16);
                byte[] cdTextPack2ForCrc = new byte[16];
                Array.Copy(cdTextPack2, 0, cdTextPack2ForCrc, 0, 16);
                ushort calculatedCdtp2Crc = CRC16CCITTContext.Calculate(cdTextPack2ForCrc);

                if(cdTextPack2Crc != calculatedCdtp2Crc &&
                   cdTextPack2Crc != 0)
                    status = false;
            }

            if((cdTextPack3[0] & 0x80) == 0x80)
            {
                ushort cdTextPack3Crc    = BigEndianBitConverter.ToUInt16(cdTextPack3, 16);
                byte[] cdTextPack3ForCrc = new byte[16];
                Array.Copy(cdTextPack3, 0, cdTextPack3ForCrc, 0, 16);
                ushort calculatedCdtp3Crc = CRC16CCITTContext.Calculate(cdTextPack3ForCrc);

                if(cdTextPack3Crc != calculatedCdtp3Crc &&
                   cdTextPack3Crc != 0)
                    status = false;
            }

            if((cdTextPack4[0] & 0x80) != 0x80)
                return status;

            ushort cdTextPack4Crc    = BigEndianBitConverter.ToUInt16(cdTextPack4, 16);
            byte[] cdTextPack4ForCrc = new byte[16];
            Array.Copy(cdTextPack4, 0, cdTextPack4ForCrc, 0, 16);
            ushort calculatedCdtp4Crc = CRC16CCITTContext.Calculate(cdTextPack4ForCrc);

            if(cdTextPack4Crc == calculatedCdtp4Crc ||
               cdTextPack4Crc == 0)
                return status;

            return false;
        }
    }
}