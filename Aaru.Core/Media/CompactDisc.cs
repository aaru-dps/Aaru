// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
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

namespace Aaru.Core.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Aaru.Helpers;

/// <summary>Operations over CD based media</summary>
public static class CompactDisc
{
    /// <summary>Writes subchannel data to an image</summary>
    /// <param name="supportedSubchannel">Subchannel read by drive</param>
    /// <param name="desiredSubchannel">Subchannel user wants written to image</param>
    /// <param name="sub">Subchannel data</param>
    /// <param name="sectorAddress">Starting sector</param>
    /// <param name="length">How many sectors were read</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="isrcs">List of ISRCs</param>
    /// <param name="currentTrack">Current track number</param>
    /// <param name="mcn">Disc's MCN</param>
    /// <param name="tracks">List of tracks</param>
    /// <param name="subchannelExtents">List of subchannel extents</param>
    /// <param name="fixSubchannelPosition">If we want to fix subchannel position</param>
    /// <param name="outputPlugin">Output image</param>
    /// <param name="fixSubchannel">If we want to fix subchannel contents</param>
    /// <param name="fixSubchannelCrc">If we want to fix Q subchannel CRC if the contents look sane</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Status update callback</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest known pregap per track</param>
    /// <param name="dumping">Set if we are dumping, otherwise converting</param>
    /// <returns><c>true</c> if indexes have changed, <c>false</c> otherwise</returns>
    public static bool WriteSubchannelToImage(MmcSubchannel supportedSubchannel, MmcSubchannel desiredSubchannel,
                                              byte[] sub, ulong sectorAddress, uint length, SubchannelLog subLog,
                                              Dictionary<byte, string> isrcs, byte currentTrack, ref string mcn,
                                              Track[] tracks, HashSet<int> subchannelExtents,
                                              bool fixSubchannelPosition, IWritableOpticalImage outputPlugin,
                                              bool fixSubchannel, bool fixSubchannelCrc, DumpLog dumpLog,
                                              UpdateStatusHandler updateStatus,
                                              Dictionary<byte, int> smallestPregapLbaPerTrack, bool dumping,
                                              out List<ulong> newPregapSectors)
    {
        // We need to work in PW raw subchannels
        if(supportedSubchannel == MmcSubchannel.Q16)
            sub = Subchannel.ConvertQToRaw(sub);

        // If not desired to fix, or to save, the subchannel, just save as is (or none)
        if(!fixSubchannelPosition &&
           desiredSubchannel != MmcSubchannel.None)
            outputPlugin.WriteSectorsTag(sub, sectorAddress, length, SectorTagType.CdSectorSubchannel);

        subLog?.WriteEntry(sub, supportedSubchannel == MmcSubchannel.Raw, (long)sectorAddress, length, false, false);

        byte[] deSub = Subchannel.Deinterleave(sub);

        bool indexesChanged = CheckIndexesFromSubchannel(deSub, isrcs, currentTrack, ref mcn, tracks, dumpLog,
                                                         updateStatus, smallestPregapLbaPerTrack, dumping,
                                                         out newPregapSectors, sectorAddress);

        if(!fixSubchannelPosition ||
           desiredSubchannel == MmcSubchannel.None)
            return indexesChanged;

        int prePos = int.MinValue;

        // Check subchannel
        for(var subPos = 0; subPos < deSub.Length; subPos += 96)
        {
            // Expected LBA
            long lba = (long)sectorAddress + subPos / 96;

            // We fixed the subchannel
            var @fixed = false;

            var q = new byte[12];
            Array.Copy(deSub, subPos + 12, q, 0, 12);

            // Check Q CRC
            CRC16CCITTContext.Data(q, 10, out byte[] crc);
            bool crcOk = crc[0] == q[10] && crc[1] == q[11];

            // Start considering P to be OK
            var pOk     = true;
            var pWeight = 0;

            // Check P and weight
            for(int p = subPos; p < subPos + 12; p++)
            {
                if(deSub[p] != 0 &&
                   deSub[p] != 255)
                    pOk = false;

                for(var w = 0; w < 8; w++)
                    if(((deSub[p] >> w) & 1) > 0)
                        pWeight++;
            }

            // This seems to be a somewhat common pattern
            bool rOk = deSub.Skip(subPos + 24).Take(12).All(r => r == 0x00) ||
                       deSub.Skip(subPos + 24).Take(12).All(r => r == 0xFF);

            bool sOk = deSub.Skip(subPos + 36).Take(12).All(s => s == 0x00) ||
                       deSub.Skip(subPos + 36).Take(12).All(s => s == 0xFF);

            bool tOk = deSub.Skip(subPos + 48).Take(12).All(t => t == 0x00) ||
                       deSub.Skip(subPos + 48).Take(12).All(t => t == 0xFF);

            bool uOk = deSub.Skip(subPos + 60).Take(12).All(u => u == 0x00) ||
                       deSub.Skip(subPos + 60).Take(12).All(u => u == 0xFF);

            bool vOk = deSub.Skip(subPos + 72).Take(12).All(v => v == 0x00) ||
                       deSub.Skip(subPos + 72).Take(12).All(v => v == 0xFF);

            bool wOk = deSub.Skip(subPos + 84).Take(12).All(w => w == 0x00) ||
                       deSub.Skip(subPos + 84).Take(12).All(w => w == 0xFF);

            bool rwOk         = rOk && sOk && tOk && uOk && vOk && wOk;
            var  rwPacket     = false;
            var  cdtextPacket = false;

            // Check RW contents
            if(!rwOk)
            {
                var sectorSub = new byte[96];
                Array.Copy(sub, subPos, sectorSub, 0, 96);

                DetectRwPackets(sectorSub, out _, out rwPacket, out cdtextPacket);

                // TODO: CD+G reed solomon
                if(rwPacket && !cdtextPacket)
                    rwOk = true;

                if(cdtextPacket)
                    rwOk = CheckCdTextPackets(sectorSub);
            }

            // Fix P
            if(!pOk && fixSubchannel)
            {
                if(pWeight >= 48)
                    for(int p = subPos; p < subPos + 12; p++)
                        deSub[p] = 255;
                else
                    for(int p = subPos; p < subPos + 12; p++)
                        deSub[p] = 0;

                pOk    = true;
                @fixed = true;

                subLog?.WritePFix(lba);
            }

            // RW is not a known pattern or packet, fix it
            if(!rwOk         &&
               !rwPacket     &&
               !cdtextPacket &&
               fixSubchannel)
            {
                for(int rw = subPos + 24; rw < subPos + 96; rw++)
                    deSub[rw] = 0;

                rwOk   = true;
                @fixed = true;

                subLog.WriteRwFix(lba);
            }

            int  aPos;

            // Fix Q
            if(!crcOk        &&
               fixSubchannel &&
               subPos > 0    &&
               subPos < deSub.Length - 96)
            {
                isrcs.TryGetValue(currentTrack, out string knownGoodIsrc);

                crcOk = FixQSubchannel(deSub, q, subPos, mcn, knownGoodIsrc, fixSubchannelCrc, out bool fixedAdr,
                                       out bool controlFix, out bool fixedZero, out bool fixedTno, out bool fixedIndex,
                                       out bool fixedRelPos, out bool fixedAbsPos, out bool fixedCrc, out bool fixedMcn,
                                       out bool fixedIsrc);

                if(crcOk)
                {
                    Array.Copy(q, 0, deSub, subPos + 12, 12);
                    @fixed = true;

                    if(fixedAdr)
                        subLog?.WriteQAdrFix(lba);

                    if(controlFix)
                        subLog?.WriteQCtrlFix(lba);

                    if(fixedZero)
                        subLog?.WriteQZeroFix(lba);

                    if(fixedTno)
                        subLog?.WriteQTnoFix(lba);

                    if(fixedIndex)
                        subLog?.WriteQIndexFix(lba);

                    if(fixedRelPos)
                        subLog?.WriteQRelPosFix(lba);

                    if(fixedAbsPos)
                        subLog?.WriteQAbsPosFix(lba);

                    if(fixedCrc)
                        subLog?.WriteQCrcFix(lba);

                    if(fixedMcn)
                        subLog?.WriteQMcnFix(lba);

                    if(fixedIsrc)
                        subLog?.WriteQIsrcFix(lba);
                }
            }

            if(!pOk   ||
               !crcOk ||
               !rwOk)
                continue;

            var aframe = (byte)(q[9] / 16 * 10 + (q[9] & 0x0F));

            if((q[0] & 0x3) == 1)
            {
                var  amin = (byte)(q[7] / 16 * 10 + (q[7] & 0x0F));
                var asec = (byte)(q[8] / 16 * 10 + (q[8] & 0x0F));
                aPos = amin * 60 * 75 + asec * 75 + aframe - 150;
            }
            else
            {
                ulong expectedSectorAddress = sectorAddress + (ulong)(subPos / 96) + 150;
                var  smin                  = (byte)(expectedSectorAddress / 60 / 75);
                expectedSectorAddress -= (ulong)(smin                 * 60 * 75);
                var ssec = (byte)(expectedSectorAddress / 75);

                aPos = smin * 60 * 75 + ssec * 75 + aframe - 150;

                // Next second
                if(aPos < prePos)
                    aPos += 75;
            }

            // TODO: Negative sectors
            if(aPos < 0)
                continue;

            prePos = aPos;

            var posSub = new byte[96];
            Array.Copy(deSub, subPos, posSub, 0, 96);
            posSub = Subchannel.Interleave(posSub);
            outputPlugin.WriteSectorTag(posSub, (ulong)aPos, SectorTagType.CdSectorSubchannel);

            subchannelExtents.Remove(aPos);

            if(@fixed)
                subLog?.WriteEntry(posSub, supportedSubchannel == MmcSubchannel.Raw, lba, 1, false, true);
        }

        return indexesChanged;
    }

    /// <summary>Check subchannel for indexes</summary>
    /// <param name="deSub">De-interleaved subchannel</param>
    /// <param name="isrcs">List of ISRCs</param>
    /// <param name="currentTrackNumber">Current track number</param>
    /// <param name="mcn">Disc's MCN</param>
    /// <param name="tracks">List of tracks</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Status update callback</param>
    /// <param name="smallestPregapLbaPerTrack">List of smallest known pregap per track</param>
    /// <param name="dumping">Set if we are dumping, otherwise converting</param>
    /// <returns><c>true</c> if indexes have changed, <c>false</c> otherwise</returns>
    static bool CheckIndexesFromSubchannel(byte[] deSub, Dictionary<byte, string> isrcs, byte currentTrackNumber,
                                           ref string mcn, Track[] tracks, DumpLog dumpLog,
                                           UpdateStatusHandler updateStatus,
                                           Dictionary<byte, int> smallestPregapLbaPerTrack, bool dumping,
                                           out List<ulong> newPregapSectors, ulong sectorAddress)
    {
        var status = false;
        newPregapSectors = new List<ulong>();

        // Check subchannel
        for(var subPos = 0; subPos < deSub.Length; subPos += 96)
        {
            var q = new byte[12];
            Array.Copy(deSub, subPos + 12, q, 0, 12);

            CRC16CCITTContext.Data(q, 10, out byte[] crc);
            bool crcOk = crc[0] == q[10] && crc[1] == q[11];

            switch(q[0] & 0x3)
            {
                // ISRC
                case 3:
                {
                    string isrc = Subchannel.DecodeIsrc(q);

                    if(isrc is null or "000000000000")
                        continue;

                    if(!crcOk)
                        continue;

                    if(!isrcs.ContainsKey(currentTrackNumber))
                    {
                        dumpLog?.WriteLine($"Found new ISRC {isrc} for track {currentTrackNumber}.");
                        updateStatus?.Invoke($"Found new ISRC {isrc} for track {currentTrackNumber}.");

                        isrcs[currentTrackNumber] = isrc;
                    }
                    else if(isrcs[currentTrackNumber] != isrc)
                    {
                        Track currentTrack =
                            tracks.FirstOrDefault(t => sectorAddress + (ulong)subPos / 96 >= t.StartSector);

                        if(currentTrack?.Sequence == currentTrackNumber)
                        {
                            dumpLog?.
                                WriteLine($"ISRC for track {currentTrackNumber} changed from {isrcs[currentTrackNumber]} to {isrc}.");

                            updateStatus?.
                                Invoke($"ISRC for track {currentTrackNumber} changed from {isrcs[currentTrackNumber]} to {isrc}.");

                            isrcs[currentTrackNumber] = isrc;
                        }
                    }

                    break;
                }

                // MCN
                case 2:
                {
                    string newMcn = Subchannel.DecodeMcn(q);

                    if(newMcn is null or "0000000000000")
                        continue;

                    if(!crcOk)
                        continue;

                    if(mcn is null)
                    {
                        dumpLog?.WriteLine($"Found new MCN {newMcn}.");
                        updateStatus?.Invoke($"Found new MCN {newMcn}.");
                    }
                    else if(mcn != newMcn)
                    {
                        dumpLog?.WriteLine($"MCN changed from {mcn} to {newMcn}.");
                        updateStatus?.Invoke($"MCN changed from {mcn} to {newMcn}.");
                    }

                    mcn = newMcn;

                    break;
                }

                // Positioning
                case 1 when !crcOk: continue;
                case 1:
                {
                    var trackNo = (byte)(q[1] / 16 * 10 + (q[1] & 0x0F));

                    for(var i = 0; i < tracks.Length; i++)
                    {
                        if(tracks[i].Sequence != trackNo)
                            continue;

                        // Pregap
                        if(q[2]    == 0 &&
                           trackNo > 1)
                        {
                            var pmin   = (byte)(q[3] / 16 * 10 + (q[3] & 0x0F));
                            var psec   = (byte)(q[4] / 16 * 10 + (q[4] & 0x0F));
                            var pframe = (byte)(q[5] / 16 * 10 + (q[5] & 0x0F));
                            int qPos   = pmin * 60 * 75 + psec * 75 + pframe;

                            // When we are dumping we calculate the pregap in reverse from index 1 back.
                            // When we are not, we go from index 0.
                            if(!smallestPregapLbaPerTrack.ContainsKey(trackNo))
                                smallestPregapLbaPerTrack[trackNo] = dumping ? 1 : 0;

                            uint firstTrackNumberInSameSession = tracks.
                                                                 Where(t => t.Session == tracks[i].Session).
                                                                 Min(t => t.Sequence);

                            if(tracks[i].Sequence == firstTrackNumberInSameSession)
                                continue;

                            if(qPos < smallestPregapLbaPerTrack[trackNo])
                            {
                                int dif = smallestPregapLbaPerTrack[trackNo] - qPos;
                                tracks[i].Pregap                   += (ulong)dif;
                                tracks[i].StartSector              -= (ulong)dif;
                                smallestPregapLbaPerTrack[trackNo] =  qPos;

                                if(i                       > 0 &&
                                   tracks[i - 1].EndSector >= tracks[i].StartSector)
                                    tracks[i - 1].EndSector = tracks[i].StartSector - 1;

                                dumpLog?.WriteLine($"Pregap for track {trackNo} set to {tracks[i].Pregap} sectors.");

                                updateStatus?.Invoke($"Pregap for track {trackNo} set to {tracks[i].Pregap} sectors.");

                                for(var p = 0; p < dif; p++)
                                    newPregapSectors.Add(tracks[i].StartSector + (ulong)p);

                                status = true;
                            }

                            if(tracks[i].Pregap >= (ulong)qPos)
                                continue;

                            ulong oldPregap = tracks[i].Pregap;

                            tracks[i].Pregap      =  (ulong)qPos;
                            tracks[i].StartSector -= tracks[i].Pregap - oldPregap;

                            if(i                       > 0 &&
                               tracks[i - 1].EndSector >= tracks[i].StartSector)
                                tracks[i - 1].EndSector = tracks[i].StartSector - 1;

                            dumpLog?.WriteLine($"Pregap for track {trackNo} set to {tracks[i].Pregap} sectors.");

                            updateStatus?.Invoke($"Pregap for track {trackNo} set to {tracks[i].Pregap} sectors.");

                            for(var p = 0; p < (int)(tracks[i].Pregap - oldPregap); p++)
                                newPregapSectors.Add(tracks[i].StartSector + (ulong)p);

                            status = true;

                            continue;
                        }

                        if(q[2] == 0)
                            continue;

                        var amin   = (byte)(q[7] / 16 * 10 + (q[7] & 0x0F));
                        var asec   = (byte)(q[8] / 16 * 10 + (q[8] & 0x0F));
                        var aframe = (byte)(q[9] / 16 * 10 + (q[9] & 0x0F));
                        int aPos   = amin * 60 * 75 + asec * 75 + aframe - 150;

                        // Do not set INDEX 1 to a value higher than what the TOC already said.
                        if(q[2] == 1 &&
                           aPos > (int)tracks[i].StartSector)
                            continue;

                        if(tracks[i].Indexes.ContainsKey(q[2]) &&
                           aPos >= tracks[i].Indexes[q[2]])
                            continue;

                        dumpLog?.WriteLine($"Setting index {q[2]} for track {trackNo} to LBA {aPos}.");
                        updateStatus?.Invoke($"Setting index {q[2]} for track {trackNo} to LBA {aPos}.");

                        tracks[i].Indexes[q[2]] = aPos;

                        status = true;
                    }

                    break;
                }
            }
        }

        return status;
    }

    /// <summary>Detect RW packets</summary>
    /// <param name="subchannel">Subchannel data</param>
    /// <param name="zero">Set if it contains a ZERO packet</param>
    /// <param name="rwPacket">Set if it contains a GRAPHICS, EXTENDED GRAPHICS or MIDI packet</param>
    /// <param name="cdtextPacket">Set if it contains a TEXT packet</param>
    static void DetectRwPackets(byte[] subchannel, out bool zero, out bool rwPacket, out bool cdtextPacket)
    {
        zero         = false;
        rwPacket     = false;
        cdtextPacket = false;

        var cdTextPack1  = new byte[18];
        var cdTextPack2  = new byte[18];
        var cdTextPack3  = new byte[18];
        var cdTextPack4  = new byte[18];
        var cdSubRwPack1 = new byte[24];
        var cdSubRwPack2 = new byte[24];
        var cdSubRwPack3 = new byte[24];
        var cdSubRwPack4 = new byte[24];

        var i = 0;

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 24; j++)
            cdSubRwPack1[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack2[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack3[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
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

    /// <summary>Checks if subchannel contains a TEXT packet</summary>
    /// <param name="subchannel">Subchannel data</param>
    /// <returns><c>true</c> if subchannel contains a TEXT packet, <c>false</c> otherwise</returns>
    static bool CheckCdTextPackets(byte[] subchannel)
    {
        var cdTextPack1 = new byte[18];
        var cdTextPack2 = new byte[18];
        var cdTextPack3 = new byte[18];
        var cdTextPack4 = new byte[18];

        var i = 0;

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 18; j++)
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

        for(var j = 0; j < 18; j++)
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

        var status = true;

        if((cdTextPack1[0] & 0x80) == 0x80)
        {
            var cdTextPack1Crc    = BigEndianBitConverter.ToUInt16(cdTextPack1, 16);
            var cdTextPack1ForCrc = new byte[16];
            Array.Copy(cdTextPack1, 0, cdTextPack1ForCrc, 0, 16);
            ushort calculatedCdtp1Crc = CRC16CCITTContext.Calculate(cdTextPack1ForCrc);

            if(cdTextPack1Crc != calculatedCdtp1Crc &&
               cdTextPack1Crc != 0)
                status = false;
        }

        if((cdTextPack2[0] & 0x80) == 0x80)
        {
            var cdTextPack2Crc    = BigEndianBitConverter.ToUInt16(cdTextPack2, 16);
            var cdTextPack2ForCrc = new byte[16];
            Array.Copy(cdTextPack2, 0, cdTextPack2ForCrc, 0, 16);
            ushort calculatedCdtp2Crc = CRC16CCITTContext.Calculate(cdTextPack2ForCrc);

            if(cdTextPack2Crc != calculatedCdtp2Crc &&
               cdTextPack2Crc != 0)
                status = false;
        }

        if((cdTextPack3[0] & 0x80) == 0x80)
        {
            var cdTextPack3Crc    = BigEndianBitConverter.ToUInt16(cdTextPack3, 16);
            var cdTextPack3ForCrc = new byte[16];
            Array.Copy(cdTextPack3, 0, cdTextPack3ForCrc, 0, 16);
            ushort calculatedCdtp3Crc = CRC16CCITTContext.Calculate(cdTextPack3ForCrc);

            if(cdTextPack3Crc != calculatedCdtp3Crc &&
               cdTextPack3Crc != 0)
                status = false;
        }

        if((cdTextPack4[0] & 0x80) != 0x80)
            return status;

        var cdTextPack4Crc    = BigEndianBitConverter.ToUInt16(cdTextPack4, 16);
        var cdTextPack4ForCrc = new byte[16];
        Array.Copy(cdTextPack4, 0, cdTextPack4ForCrc, 0, 16);
        ushort calculatedCdtp4Crc = CRC16CCITTContext.Calculate(cdTextPack4ForCrc);

        if(cdTextPack4Crc == calculatedCdtp4Crc ||
           cdTextPack4Crc == 0)
            return status;

        return false;
    }

    /// <summary>Fixes Q subchannel</summary>
    /// <param name="deSub">Deinterleaved subchannel data</param>
    /// <param name="q">Q subchannel</param>
    /// <param name="subPos">Position in <see>deSub</see></param>
    /// <param name="mcn">Disc's MCN</param>
    /// <param name="isrc">Track ISRC</param>
    /// <param name="fixCrc">Set to <c>true</c> if we should fix the CRC, <c>false</c> otherwise</param>
    /// <param name="fixedAdr">Set to <c>true</c> if we fixed the ADR, <c>false</c> otherwise</param>
    /// <param name="controlFix">Set to <c>true</c> if we fixed the CONTROL, <c>false</c> otherwise</param>
    /// <param name="fixedZero">Set to <c>true</c> if we fixed the ZERO, <c>false</c> otherwise</param>
    /// <param name="fixedTno">Set to <c>true</c> if we fixed the TNO, <c>false</c> otherwise</param>
    /// <param name="fixedIndex">Set to <c>true</c> if we fixed the INDEX, <c>false</c> otherwise</param>
    /// <param name="fixedRelPos">Set to <c>true</c> if we fixed the PMIN, PSEC and/or PFRAME, <c>false</c> otherwise</param>
    /// <param name="fixedAbsPos">Set to <c>true</c> if we fixed the AMIN, ASEC and/or AFRAME, <c>false</c> otherwise</param>
    /// <param name="fixedCrc">Set to <c>true</c> if we fixed the CRC, <c>false</c> otherwise</param>
    /// <param name="fixedMcn">Set to <c>true</c> if we fixed the MCN, <c>false</c> otherwise</param>
    /// <param name="fixedIsrc">Set to <c>true</c> if we fixed the ISRC, <c>false</c> otherwise</param>
    /// <returns><c>true</c> if it was fixed correctly, <c>false</c> otherwise</returns>
    static bool FixQSubchannel(byte[] deSub, byte[] q, int subPos, string mcn, string isrc, bool fixCrc,
                               out bool fixedAdr, out bool controlFix, out bool fixedZero, out bool fixedTno,
                               out bool fixedIndex, out bool fixedRelPos, out bool fixedAbsPos, out bool fixedCrc,
                               out bool fixedMcn, out bool fixedIsrc)
    {
        byte aframe;
        byte rframe;
        controlFix  = false;
        fixedZero   = false;
        fixedTno    = false;
        fixedIndex  = false;
        fixedRelPos = false;
        fixedAbsPos = false;
        fixedCrc    = false;
        fixedMcn    = false;
        fixedIsrc   = false;

        var preQ  = new byte[12];
        var nextQ = new byte[12];
        Array.Copy(deSub, subPos + 12 - 96, preQ, 0, 12);
        Array.Copy(deSub, subPos      + 12 + 96, nextQ, 0, 12);
        bool status;

        CRC16CCITTContext.Data(preQ, 10, out byte[] preCrc);
        bool preCrcOk = preCrc[0] == preQ[10] && preCrc[1] == preQ[11];

        CRC16CCITTContext.Data(nextQ, 10, out byte[] nextCrc);
        bool nextCrcOk = nextCrc[0] == nextQ[10] && nextCrc[1] == nextQ[11];

        fixedAdr = false;

        // Extraneous bits in ADR
        if((q[0] & 0xC) != 0)
        {
            q[0]     &= 0xF3;
            fixedAdr =  true;
        }

        CRC16CCITTContext.Data(q, 10, out byte[] qCrc);
        status = qCrc[0] == q[10] && qCrc[1] == q[11];

        if(fixedAdr && status)
            return true;

        int oldAdr = q[0] & 0x3;

        // Try Q-Mode 1
        q[0] = (byte)((q[0] & 0xF0) + 1);
        CRC16CCITTContext.Data(q, 10, out qCrc);
        status = qCrc[0] == q[10] && qCrc[1] == q[11];

        if(status)
        {
            fixedAdr = true;

            return true;
        }

        // Try Q-Mode 2
        q[0] = (byte)((q[0] & 0xF0) + 2);
        CRC16CCITTContext.Data(q, 10, out qCrc);
        status = qCrc[0] == q[10] && qCrc[1] == q[11];

        if(status)
        {
            fixedAdr = true;

            return true;
        }

        // Try Q-Mode 3
        q[0] = (byte)((q[0] & 0xF0) + 3);
        CRC16CCITTContext.Data(q, 10, out qCrc);
        status = qCrc[0] == q[10] && qCrc[1] == q[11];

        if(status)
        {
            fixedAdr = true;

            return true;
        }

        q[0] = (byte)((q[0] & 0xF0) + oldAdr);

        oldAdr = q[0];

        // Try using previous control
        if(preCrcOk && (q[0] & 0xF0) != (preQ[0] & 0xF0))
        {
            q[0] = (byte)((q[0] & 0x03) + (preQ[0] & 0xF0));

            CRC16CCITTContext.Data(q, 10, out qCrc);
            status = qCrc[0] == q[10] && qCrc[1] == q[11];

            if(status)
            {
                controlFix = true;

                return true;
            }

            q[0] = (byte)oldAdr;
        }

        // Try using next control
        if(nextCrcOk && (q[0] & 0xF0) != (nextQ[0] & 0xF0))
        {
            q[0] = (byte)((q[0] & 0x03) + (nextQ[0] & 0xF0));

            CRC16CCITTContext.Data(q, 10, out qCrc);
            status = qCrc[0] == q[10] && qCrc[1] == q[11];

            if(status)
            {
                controlFix = true;

                return true;
            }

            q[0] = (byte)oldAdr;
        }

        if(preCrcOk                               &&
           nextCrcOk                              &&
           (nextQ[0] & 0xF0) == (preQ[0]  & 0xF0) &&
           (q[0]     & 0xF0) != (nextQ[0] & 0xF0))
        {
            q[0] = (byte)((q[0] & 0x03) + (nextQ[0] & 0xF0));

            controlFix = true;
        }

        switch(q[0] & 0x3)
        {
            // Positioning
            case 1:
            {
                // ZERO not zero
                if(q[6] != 0)
                {
                    q[6]      = 0;
                    fixedZero = true;

                    CRC16CCITTContext.Data(q, 10, out qCrc);
                    status = qCrc[0] == q[10] && qCrc[1] == q[11];

                    if(status)
                        return true;
                }

                if(preCrcOk && nextCrcOk)
                    if(preQ[1] == nextQ[1] &&
                       preQ[1] != q[1])
                    {
                        q[1]     = preQ[1];
                        fixedTno = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }

                if(preCrcOk && nextCrcOk)
                    if(preQ[2] == nextQ[2] &&
                       preQ[2] != q[2])
                    {
                        q[2]       = preQ[2];
                        fixedIndex = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }

                var  amin = (byte)(q[7] / 16 * 10 + (q[7] & 0x0F));
                var asec = (byte)(q[8] / 16 * 10 + (q[8] & 0x0F));
                aframe = (byte)(q[9] / 16 * 10                      + (q[9] & 0x0F));
                int aPos = amin      * 60 * 75 + asec * 75 + aframe - 150;

                var pmin   = (byte)(q[3] / 16 * 10 + (q[3] & 0x0F));
                var psec   = (byte)(q[4] / 16 * 10 + (q[4] & 0x0F));
                var pframe = (byte)(q[5] / 16 * 10 + (q[5] & 0x0F));
                int pPos   = pmin * 60 * 75 + psec * 75 + pframe;

                // TODO: pregap
                // Not pregap
                byte rmin;

                byte rsec;

                int rPos;

                int dPos;

                if(q[2] > 0)
                {
                    // Previous was not pregap either
                    if(preQ[2] > 0 && preCrcOk)
                    {
                        rmin   = (byte)(preQ[3] / 16 * 10 + (preQ[3] & 0x0F));
                        rsec   = (byte)(preQ[4] / 16 * 10 + (preQ[4] & 0x0F));
                        rframe = (byte)(preQ[5] / 16 * 10 + (preQ[5] & 0x0F));
                        rPos   = rmin * 60 * 75 + rsec * 75 + rframe;

                        dPos = pPos - rPos;

                        if(dPos != 1)
                        {
                            q[3] = preQ[3];
                            q[4] = preQ[4];
                            q[5] = preQ[5];

                            // BCD add 1, so 0x39 becomes 0x40
                            if((q[5] & 0xF) == 9)
                                q[5] += 7;
                            else
                                q[5]++;

                            // 74 frames, so from 0x00 to 0x74, BCD
                            if(q[5] >= 0x74)
                            {
                                // 0 frames
                                q[5] = 0;

                                // Add 1 second
                                if((q[4] & 0xF) == 9)
                                    q[4] += 7;
                                else
                                    q[4]++;

                                // 60 seconds, so from 0x00 to 0x59, BCD
                                if(q[4] >= 0x59)
                                {
                                    // 0 seconds
                                    q[4] = 0;

                                    // Add 1 minute
                                    q[3]++;
                                }
                            }

                            fixedRelPos = true;

                            CRC16CCITTContext.Data(q, 10, out qCrc);
                            status = qCrc[0] == q[10] && qCrc[1] == q[11];

                            if(status)
                                return true;
                        }
                    }

                    // Next is not pregap and we didn't fix relative position with previous
                    if(nextQ[2] > 0 &&
                       nextCrcOk    &&
                       !fixedRelPos)
                    {
                        rmin   = (byte)(nextQ[3] / 16 * 10 + (nextQ[3] & 0x0F));
                        rsec   = (byte)(nextQ[4] / 16 * 10 + (nextQ[4] & 0x0F));
                        rframe = (byte)(nextQ[5] / 16 * 10 + (nextQ[5] & 0x0F));
                        rPos   = rmin * 60 * 75 + rsec * 75 + rframe;

                        dPos = rPos - pPos;

                        if(dPos != 1)
                        {
                            q[3] = nextQ[3];
                            q[4] = nextQ[4];
                            q[5] = nextQ[5];

                            // If frames is 0
                            if(q[5] == 0)
                            {
                                // If seconds is 0
                                if(q[4] == 0)
                                {
                                    // BCD decrease minutes
                                    if((q[3] & 0xF) == 0)
                                        q[3] = (byte)((q[3] & 0xF0) - 0x10);
                                    else
                                        q[3]--;

                                    q[4] = 0x59;
                                    q[5] = 0x73;
                                }
                                else
                                {
                                    // BCD decrease seconds
                                    if((q[4] & 0xF) == 0)
                                        q[4] = (byte)((q[4] & 0xF0) - 0x10);
                                    else
                                        q[4]--;

                                    q[5] = 0x73;
                                }
                            }

                            // BCD decrease frames
                            else if((q[5] & 0xF) == 0)
                                q[5] = (byte)((q[5] & 0xF0) - 0x10);
                            else
                                q[5]--;

                            fixedRelPos = true;

                            CRC16CCITTContext.Data(q, 10, out qCrc);
                            status = qCrc[0] == q[10] && qCrc[1] == q[11];

                            if(status)
                                return true;
                        }
                    }
                }

                // Previous Q's CRC is correct
                if(preCrcOk)
                {
                    rmin   = (byte)(preQ[7] / 16 * 10 + (preQ[7] & 0x0F));
                    rsec   = (byte)(preQ[8] / 16 * 10 + (preQ[8] & 0x0F));
                    rframe = (byte)(preQ[9] / 16 * 10 + (preQ[9] & 0x0F));
                    rPos   = rmin * 60 * 75 + rsec * 75 + rframe - 150;

                    dPos = aPos - rPos;

                    if(dPos != 1)
                    {
                        q[7] = preQ[7];
                        q[8] = preQ[8];
                        q[9] = preQ[9];

                        // BCD add 1, so 0x39 becomes 0x40
                        if((q[9] & 0xF) == 9)
                            q[9] += 7;
                        else
                            q[9]++;

                        // 74 frames, so from 0x00 to 0x74, BCD
                        if(q[9] >= 0x74)
                        {
                            // 0 frames
                            q[9] = 0;

                            // Add 1 second
                            if((q[8] & 0xF) == 9)
                                q[8] += 7;
                            else
                                q[8]++;

                            // 60 seconds, so from 0x00 to 0x59, BCD
                            if(q[8] >= 0x59)
                            {
                                // 0 seconds
                                q[8] = 0;

                                // Add 1 minute
                                q[7]++;
                            }
                        }

                        fixedAbsPos = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }
                }

                // Next is not pregap and we didn't fix relative position with previous
                if(nextQ[2] > 0 &&
                   nextCrcOk    &&
                   !fixedAbsPos)
                {
                    rmin   = (byte)(nextQ[7] / 16 * 10 + (nextQ[7] & 0x0F));
                    rsec   = (byte)(nextQ[8] / 16 * 10 + (nextQ[8] & 0x0F));
                    rframe = (byte)(nextQ[9] / 16 * 10 + (nextQ[9] & 0x0F));
                    rPos   = rmin * 60 * 75 + rsec * 75 + rframe - 150;

                    dPos = rPos - pPos;

                    if(dPos != 1)
                    {
                        q[7] = nextQ[7];
                        q[8] = nextQ[8];
                        q[9] = nextQ[9];

                        // If frames is 0
                        if(q[9] == 0)
                        {
                            // If seconds is 0
                            if(q[8] == 0)
                            {
                                // BCD decrease minutes
                                if((q[7] & 0xF) == 0)
                                    q[7] = (byte)((q[7] & 0xF0) - 0x10);
                                else
                                    q[7]--;

                                q[8] = 0x59;
                                q[9] = 0x73;
                            }
                            else
                            {
                                // BCD decrease seconds
                                if((q[8] & 0xF) == 0)
                                    q[8] = (byte)((q[8] & 0xF0) - 0x10);
                                else
                                    q[8]--;

                                q[9] = 0x73;
                            }
                        }

                        // BCD decrease frames
                        else if((q[9] & 0xF) == 0)
                            q[9] = (byte)((q[9] & 0xF0) - 0x10);
                        else
                            q[9]--;

                        fixedAbsPos = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }
                }

                CRC16CCITTContext.Data(q, 10, out qCrc);
                status = qCrc[0] == q[10] && qCrc[1] == q[11];

                // Game Over
                if(!fixCrc || status)
                    return false;

                // Previous Q's CRC is correct
                if(preCrcOk)
                {
                    rmin   = (byte)(preQ[7] / 16 * 10 + (preQ[7] & 0x0F));
                    rsec   = (byte)(preQ[8] / 16 * 10 + (preQ[8] & 0x0F));
                    rframe = (byte)(preQ[9] / 16 * 10 + (preQ[9] & 0x0F));
                    rPos   = rmin * 60 * 75 + rsec * 75 + rframe - 150;

                    dPos = aPos - rPos;

                    bool absOk = dPos == 1;

                    rmin   = (byte)(preQ[3] / 16 * 10 + (preQ[3] & 0x0F));
                    rsec   = (byte)(preQ[4] / 16 * 10 + (preQ[4] & 0x0F));
                    rframe = (byte)(preQ[5] / 16 * 10 + (preQ[5] & 0x0F));
                    rPos   = rmin * 60 * 75 + rsec * 75 + rframe;

                    dPos = pPos - rPos;

                    bool relOk = dPos == 1;

                    if(q[0] != preQ[0] ||
                       q[1] != preQ[1] ||
                       q[2] != preQ[2] ||
                       q[6] != 0       ||
                       !absOk          ||
                       !relOk)
                        return false;

                    CRC16CCITTContext.Data(q, 10, out qCrc);
                    q[10] = qCrc[0];
                    q[11] = qCrc[1];

                    fixedCrc = true;

                    return true;
                }

                // Next Q's CRC is correct
                if(nextCrcOk)
                {
                    rmin   = (byte)(nextQ[7] / 16 * 10 + (nextQ[7] & 0x0F));
                    rsec   = (byte)(nextQ[8] / 16 * 10 + (nextQ[8] & 0x0F));
                    rframe = (byte)(nextQ[9] / 16 * 10 + (nextQ[9] & 0x0F));
                    rPos   = rmin * 60 * 75 + rsec * 75 + rframe - 150;

                    dPos = rPos - aPos;

                    bool absOk = dPos == 1;

                    rmin   = (byte)(nextQ[3] / 16 * 10 + (nextQ[3] & 0x0F));
                    rsec   = (byte)(nextQ[4] / 16 * 10 + (nextQ[4] & 0x0F));
                    rframe = (byte)(nextQ[5] / 16 * 10 + (nextQ[5] & 0x0F));
                    rPos   = rmin * 60 * 75 + rsec * 75 + rframe;

                    dPos = rPos - pPos;

                    bool relOk = dPos == 1;

                    if(q[0] != nextQ[0] ||
                       q[1] != nextQ[1] ||
                       q[2] != nextQ[2] ||
                       q[6] != 0        ||
                       !absOk           ||
                       !relOk)
                        return false;

                    CRC16CCITTContext.Data(q, 10, out qCrc);
                    q[10] = qCrc[0];
                    q[11] = qCrc[1];

                    fixedCrc = true;

                    return true;
                }

                // Ok if previous and next are both BAD I won't rewrite the CRC at all
                break;
            }

            // MCN
            case 2:
            {
                // Previous Q's CRC is correct
                if(preCrcOk)
                {
                    rframe = (byte)(preQ[9] / 16 * 10 + (preQ[9] & 0x0F));
                    aframe = (byte)(q[9]    / 16 * 10 + (q[9]    & 0x0F));

                    if(aframe - rframe != 1)
                    {
                        q[9] = preQ[9];

                        if((q[9] & 0xF) == 9)
                            q[9] += 7;
                        else
                            q[9]++;

                        if(q[9] >= 0x74)
                            q[9] = 0;

                        fixedAbsPos = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }
                }

                // Next Q's CRC is correct
                else if(nextCrcOk)
                {
                    rframe = (byte)(nextQ[9] / 16 * 10 + (nextQ[9] & 0x0F));
                    aframe = (byte)(q[9]     / 16 * 10 + (q[9]     & 0x0F));

                    if(aframe - rframe != 1)
                    {
                        q[9] = nextQ[9];

                        if(q[9] == 0)
                            q[9] = 0x73;
                        else if((q[9] & 0xF) == 0)
                            q[9] = (byte)((q[9] & 0xF0) - 0x10);
                        else
                            q[9]--;

                        fixedAbsPos = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }
                }

                // We know the MCN
                if(mcn != null)
                {
                    q[1] = (byte)(((mcn[0]  - 0x30) & 0x0F) * 16 + ((mcn[1]  - 0x30) & 0x0F));
                    q[2] = (byte)(((mcn[2]  - 0x30) & 0x0F) * 16 + ((mcn[3]  - 0x30) & 0x0F));
                    q[3] = (byte)(((mcn[4]  - 0x30) & 0x0F) * 16 + ((mcn[5]  - 0x30) & 0x0F));
                    q[4] = (byte)(((mcn[6]  - 0x30) & 0x0F) * 16 + ((mcn[7]  - 0x30) & 0x0F));
                    q[5] = (byte)(((mcn[8]  - 0x30) & 0x0F) * 16 + ((mcn[9]  - 0x30) & 0x0F));
                    q[6] = (byte)(((mcn[10] - 0x30) & 0x0F) * 16 + ((mcn[11] - 0x30) & 0x0F));
                    q[7] = (byte)(((mcn[12]                                  - 0x30) & 0x0F) * 8);
                    q[8] = 0;

                    fixedMcn = true;

                    CRC16CCITTContext.Data(q, 10, out qCrc);
                    status = qCrc[0] == q[10] && qCrc[1] == q[11];

                    if(status)
                        return true;
                }

                if(!fixCrc    ||
                   !nextCrcOk ||
                   !preCrcOk)
                    return false;

                CRC16CCITTContext.Data(q, 10, out qCrc);
                q[10] = qCrc[0];
                q[11] = qCrc[1];

                fixedCrc = true;

                return true;
            }

            // ISRC
            case 3:
            {
                // Previous Q's CRC is correct
                if(preCrcOk)
                {
                    rframe = (byte)(preQ[9] / 16 * 10 + (preQ[9] & 0x0F));
                    aframe = (byte)(q[9]    / 16 * 10 + (q[9]    & 0x0F));

                    if(aframe - rframe != 1)
                    {
                        q[9] = preQ[9];

                        if((q[9] & 0xF) == 9)
                            q[9] += 7;
                        else
                            q[9]++;

                        if(q[9] >= 0x74)
                            q[9] = 0;

                        fixedAbsPos = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }
                }

                // Next Q's CRC is correct
                else if(nextCrcOk)
                {
                    rframe = (byte)(nextQ[9] / 16 * 10 + (nextQ[9] & 0x0F));
                    aframe = (byte)(q[9]     / 16 * 10 + (q[9]     & 0x0F));

                    if(aframe - rframe != 1)
                    {
                        q[9] = nextQ[9];

                        if(q[9] == 0)
                            q[9] = 0x73;
                        else if((q[9] & 0xF) == 0)
                            q[9] = (byte)((q[9] & 0xF0) - 0x10);
                        else
                            q[9]--;

                        fixedAbsPos = true;

                        CRC16CCITTContext.Data(q, 10, out qCrc);
                        status = qCrc[0] == q[10] && qCrc[1] == q[11];

                        if(status)
                            return true;
                    }
                }

                // We know the ISRC
                if(isrc != null)
                {
                    byte i1 = Subchannel.GetIsrcCode(isrc[0]);
                    byte i2 = Subchannel.GetIsrcCode(isrc[1]);
                    byte i3 = Subchannel.GetIsrcCode(isrc[2]);
                    byte i4 = Subchannel.GetIsrcCode(isrc[3]);
                    byte i5 = Subchannel.GetIsrcCode(isrc[4]);

                    q[1] = (byte)((i1 << 2) + ((i2 & 0x30) >> 4));
                    q[2] = (byte)(((i2             & 0xF)  << 4) + (i3 >> 2));
                    q[3] = (byte)(((i3             & 0x3)  << 6) + i4);
                    q[4] = (byte)(i5 << 2);
                    q[5] = (byte)(((isrc[5] - 0x30) & 0x0F) * 16 + ((isrc[6]  - 0x30) & 0x0F));
                    q[6] = (byte)(((isrc[7] - 0x30) & 0x0F) * 16 + ((isrc[8]  - 0x30) & 0x0F));
                    q[7] = (byte)(((isrc[9] - 0x30) & 0x0F) * 16 + ((isrc[10] - 0x30) & 0x0F));
                    q[8] = (byte)(((isrc[11]                                  - 0x30) & 0x0F) * 16);

                    fixedIsrc = true;

                    CRC16CCITTContext.Data(q, 10, out qCrc);
                    status = qCrc[0] == q[10] && qCrc[1] == q[11];

                    if(status)
                        return true;
                }

                if(!fixCrc    ||
                   !nextCrcOk ||
                   !preCrcOk)
                    return false;

                CRC16CCITTContext.Data(q, 10, out qCrc);
                q[10] = qCrc[0];
                q[11] = qCrc[1];

                fixedCrc = true;

                return true;
            }
        }

        return false;
    }

    /// <summary>Generates a correct subchannel all the missing ones</summary>
    /// <param name="subchannelExtents">List of missing subchannels</param>
    /// <param name="tracks">List of tracks</param>
    /// <param name="trackFlags">Flags of tracks</param>
    /// <param name="blocks">Disc size</param>
    /// <param name="subLog">Subchannel log</param>
    /// <param name="dumpLog">Dump log</param>
    /// <param name="initProgress">Progress initialization callback</param>
    /// <param name="updateProgress">Progress update callback</param>
    /// <param name="endProgress">Progress finalization callback</param>
    /// <param name="outputPlugin">Output image</param>
    public static void GenerateSubchannels(HashSet<int> subchannelExtents, Track[] tracks,
                                           Dictionary<byte, byte> trackFlags, ulong blocks, SubchannelLog subLog,
                                           DumpLog dumpLog, InitProgressHandler initProgress,
                                           UpdateProgressHandler updateProgress, EndProgressHandler endProgress,
                                           IWritableImage outputPlugin)
    {
        initProgress?.Invoke();

        foreach(int sector in subchannelExtents)
        {
            Track track = tracks.LastOrDefault(t => (int)t.StartSector <= sector);
            byte  trkFlags;
            byte  flags;
            ulong trackStart;
            ulong pregap;

            if(track == null)
                continue;

            // Hidden track
            if(track.Sequence == 0)
            {
                track      = tracks.FirstOrDefault(t => (int)t.Sequence == 1);
                trackStart = 0;
                pregap     = track?.StartSector ?? 0;
            }
            else
            {
                trackStart = track.StartSector;
                pregap     = track.Pregap;
            }

            if(!trackFlags.TryGetValue((byte)(track?.Sequence ?? 0), out trkFlags) &&
               track?.Type != TrackType.Audio)
                flags = (byte)CdFlags.DataTrack;
            else
                flags = trkFlags;

            byte index;

            if(track?.Indexes?.Count > 0)
                index = (byte)track.Indexes.LastOrDefault(i => i.Value >= sector).Key;
            else
                index = 0;

            updateProgress?.Invoke($"Generating subchannel for sector {sector}...", sector, (long)blocks);
            dumpLog?.WriteLine($"Generating subchannel for sector {sector}.");

            byte[] sub = Subchannel.Generate(sector, track?.Sequence ?? 0, (int)pregap, (int)trackStart, flags, index);

            outputPlugin.WriteSectorsTag(sub, (ulong)sector, 1, SectorTagType.CdSectorSubchannel);

            subLog?.WriteEntry(sub, true, sector, 1, true, false);
        }

        endProgress?.Invoke();
    }
}