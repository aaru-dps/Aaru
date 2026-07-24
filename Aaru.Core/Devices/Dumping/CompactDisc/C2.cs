// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : C2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles CompactDisc C2 error pointers for secure audio reading.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    // Full sector size (2352) + C2 error block (294) + raw subchannel (96)
    const uint C2_DATA_SIZE  = 2352;
    const uint C2_POINTERS   = 294;
    const uint C2_SUB_SIZE   = 96;
    const uint C2_BLOCK_SIZE = C2_DATA_SIZE + C2_POINTERS + C2_SUB_SIZE; // 2742

    /// <summary>
    ///     Probes, once at dump start, whether the drive returns C2 error pointers together with subchannel and, if so,
    ///     in which byte order (the MMC spec mandates [data][C2][sub] but many drives emit [data][sub][C2]). The
    ///     subchannel region is located by decoding its Q channel and validating its CRC-16; the remaining region is
    ///     the C2 block. Results are written to the dump log only. On any ambiguity or failure C2 is left disabled and
    ///     the normal dump path is used unchanged.
    /// </summary>
    /// <param name="firstLba">First readable LBA of the disc, used to derive a test sector.</param>
    /// <param name="readcd">Whether the drive supports the READ CD command.</param>
    /// <param name="supportedSubchannel">Subchannel mode the drive supports.</param>
    /// <param name="hasAudio">Whether the media contains any audio track; C2 is only enabled when it does.</param>
    void DetectC2Layout(uint firstLba, bool readcd, MmcSubchannel supportedSubchannel, bool hasAudio)
    {
        _c2Supported = false;
        _c2BlockSize = 0;
        _c2Offset    = 0;
        _c2SubOffset = 0;

        // C2 error pointers only matter for audio. On a disc with no audio tracks C2 is never enabled, so the media is
        // never read with C2 and the convergence path is never taken. No message is logged, to avoid noise on data discs.
        if(!hasAudio) return;

        // OmniDrive returns C2 natively in a fixed layout (data + C2 + subchannel), so no probing is needed.
        if(_omnidrive)
        {
            _c2Supported    = true;
            _c2BlockSize    = C2_BLOCK_SIZE;
            _c2Offset       = (int)C2_DATA_SIZE;                 // 2352
            _c2SubOffset    = (int)(C2_DATA_SIZE + C2_POINTERS); // 2646
            _c2SuspectAudio = [];

            UpdateStatus?.Invoke(Localization.Core.C2_secure_audio_enabled_OmniDrive);

            AaruLogging.WriteLine(string.Format(Localization.Core.C2_secure_audio_enabled_OmniDrive_details_0,
                                                C2_BLOCK_SIZE));

            return;
        }

        // C2 secure audio reading requires READ CD and raw (96 byte) subchannel to validate the layout via Q CRC.
        if(!readcd)
        {
            AaruLogging.WriteLine(Localization.Core.C2_secure_audio_not_probed_requires_READ_CD);

            return;
        }

        if(supportedSubchannel != MmcSubchannel.Raw)
        {
            UpdateStatus?.Invoke(Localization.Core.C2_secure_audio_disabled_requires_raw_subchannel);

            AaruLogging.WriteLine(string.Format(Localization.Core.C2_secure_audio_disabled_no_raw_subchannel_0,
                                                C2_SUB_SIZE));

            return;
        }

        // Use a test sector well inside the first track, like the BCD subchannel probe does.
        uint testLba = (firstLba / 75 + 1) * 75 + 35;

        bool sense = _dev.ReadCd(out byte[] cmdBuf,
                                 out _,
                                 testLba,
                                 C2_BLOCK_SIZE,
                                 1,
                                 MmcSectorTypes.AllTypes,
                                 false,
                                 false,
                                 true,
                                 MmcHeaderCodes.AllHeaders,
                                 true,
                                 true,
                                 MmcErrorField.C2Pointers,
                                 supportedSubchannel,
                                 _dev.Timeout,
                                 out _);

        if(sense || _dev.Error || cmdBuf is null || cmdBuf.Length < C2_BLOCK_SIZE)
        {
            UpdateStatus?.Invoke(Localization.Core.C2_secure_audio_disabled_no_C2_and_subchannel);
            AaruLogging.WriteLine(Localization.Core.C2_secure_audio_disabled_READ_CD_C2_failed);

            return;
        }

        // Two candidate layouts. The subchannel region is the one whose Q channel CRC validates.
        // Layout A (MMC spec order): [data 2352][C2 294][sub 96]  -> C2 at 2352, sub at 2646
        // Layout B (common drives)  : [data 2352][sub 96][C2 294] -> sub at 2352, C2 at 2646
        const int specC2Offset  = (int)C2_DATA_SIZE;                 // 2352
        const int specSubOffset = (int)(C2_DATA_SIZE + C2_POINTERS); // 2646
        const int altSubOffset  = (int)C2_DATA_SIZE;                 // 2352
        const int altC2Offset   = (int)(C2_DATA_SIZE + C2_SUB_SIZE); // 2448

        bool specValid = ValidateSubchannelQ(cmdBuf, specSubOffset);
        bool altValid  = ValidateSubchannelQ(cmdBuf, altSubOffset);

        int    c2Offset;
        int    subOffset;
        string layout;

        switch(specValid)
        {
            // Prefer spec order if it validates; only choose alt if spec did not and alt did.
            case true:
                c2Offset  = specC2Offset;
                subOffset = specSubOffset;
                layout    = Localization.Core.C2_layout_data_C2_subchannel;

                break;
            case false when altValid:
                c2Offset  = altC2Offset;
                subOffset = altSubOffset;
                layout    = Localization.Core.C2_layout_data_subchannel_C2;

                break;
            default:
                UpdateStatus?.Invoke(Localization.Core.C2_secure_audio_disabled_unknown_byte_order);

                AaruLogging.WriteLine(Localization.Core.C2_secure_audio_disabled_no_valid_Q_CRC);

                return;
        }

        _c2Supported    = true;
        _c2BlockSize    = C2_BLOCK_SIZE;
        _c2Offset       = c2Offset;
        _c2SubOffset    = subOffset;
        _c2SuspectAudio = [];

        // Corroborating evidence: on a clean pressed sector the C2 region should be (near) all zero.
        var dirtyC2Bytes = 0;

        for(var b = 0; b < C2_POINTERS; b++)
        {
            if(cmdBuf[c2Offset + b] != 0) dirtyC2Bytes++;
        }

        UpdateStatus?.Invoke(string.Format(Localization.Core.C2_secure_audio_enabled_layout_0, layout));

        AaruLogging.WriteLine(string.Format(Localization.Core.C2_secure_audio_enabled_details,
                                            C2_BLOCK_SIZE,
                                            layout,
                                            testLba,
                                            subOffset,
                                            c2Offset,
                                            dirtyC2Bytes,
                                            C2_POINTERS));
    }

    /// <summary>
    ///     Repacks an audio block that was read with C2 error pointers (2742 bytes/sector) back into the normal block
    ///     layout (data + subchannel) the rest of the read loop expects, and records which sectors the drive concealed
    ///     (i.e. returned at least one interpolated/guessed byte) into the C2 suspect set. That set is kept separate
    ///     from hard read errors (<see cref="Resume.BadBlocks" />): a suspect sector already has data, it just is not
    ///     trustworthy byte-for-byte. Detection only; the audio is not rewritten here. Gated on a successful C2 probe.
    /// </summary>
    /// <param name="cmdBuf">Buffer read in C2 layout; replaced in place with the normal block layout.</param>
    /// <param name="blocksToRead">Number of sectors in the buffer.</param>
    /// <param name="blockSize">Normal block size (data + subchannel) expected downstream.</param>
    /// <param name="subSize">Subchannel size in bytes.</param>
    /// <param name="firstSectorToRead">LBA of the first sector in the buffer.</param>
    /// <param name="audioExtents">
    ///     Audio extents; only sectors inside them are flagged, since C2 is meaningless for data sectors. Pass
    ///     <c>null</c> when the whole buffer is known to be audio.
    /// </param>
    void RepackAudioC2(ref byte[]   cmdBuf, uint blocksToRead, uint blockSize, uint subSize, uint firstSectorToRead,
                       ExtentsULong audioExtents = null)
    {
        if(!_c2Supported || cmdBuf is null || cmdBuf.Length < blocksToRead * _c2BlockSize) return;

        _c2SuspectAudio         ??= [];
        _resume.ConcealedBlocks ??= [];

        var copySub = (int)Math.Min(subSize, C2_SUB_SIZE);

        // Sectors read to fix a negative offset wrap around near uint.MaxValue; their LBAs are not meaningful, so the
        // data is still repacked but C2 flags are not attributed to a (wrong) sector number.
        bool offsetWrapped = firstSectorToRead >= 0xFFFF0000;

        // Compact in place: the C2 layout (2742/sector) collapses to the normal layout (blockSize/sector) within the
        // same buffer, so no per-read allocation happens. dst is always <= src, and each sector's source bytes are
        // still untouched when it is processed, so the overlapping Array.Copy (memmove semantics) is safe. The unused
        // tail is ignored downstream, which addresses cmdBuf purely by blockSize stride.
        for(var b = 0; b < blocksToRead; b++)
        {
            int src = b * (int)_c2BlockSize;
            int dst = b * (int)blockSize;

            // Check C2 before the data move overwrites nothing of the C2 region (C2 sits after the data we move).
            if(!offsetWrapped && SectorHasC2Error(cmdBuf, b))
            {
                ulong sector = firstSectorToRead + (ulong)b;

                // C2 error pointers only carry meaning for audio; a data sector is validated by its EDC/ECC instead.
                if((audioExtents is null || audioExtents.Contains(sector)) && _c2SuspectAudio.Add(sector))
                {
                    _resume.ConcealedBlocks.Add(sector);
                    _errorLog?.WriteLine(sector, Localization.Core.Reason_audio_concealed_C2);
                }
            }

            // Move data first, then subchannel: the data destination ends before this sector's subchannel source, so
            // the subchannel bytes are still intact when copied.
            Array.Copy(cmdBuf, src,                cmdBuf, dst,                     (int)C2_DATA_SIZE);
            Array.Copy(cmdBuf, src + _c2SubOffset, cmdBuf, dst + (int)C2_DATA_SIZE, copySub);
        }
    }

    /// <summary>
    ///     Re-reads each audio sector the drive concealed (the C2 suspect set) until it converges on trustworthy data:
    ///     a re-read is only trusted when its whole offset window comes back C2-clean, and a sector is accepted once two
    ///     clean reads agree byte-for-byte. Converged sectors are rewritten into the image and dropped from the suspect
    ///     set. Read-offset alignment mirrors the standard retry path. Gated on a successful C2 probe.
    /// </summary>
    void ConvergeAudioC2(int offsetBytes, int sectorsForOffset, MmcSubchannel supportedSubchannel, ExtentsULong extents,
                         IWritableOpticalImage outputOptical, bool readcd)
    {
        if(!_c2Repair || !_c2Supported || !readcd && !_omnidrive || _c2SuspectAudio is not { Count: > 0 } || _aborted)
            return;

        ulong[] suspects    = _c2SuspectAudio.OrderBy(static s => s).ToArray();
        int     maxAttempts = Math.Max((int)_retryPasses, 8);
        var     converged   = 0;
        var     improved    = 0;
        var     failed      = 0;
        long    totalSteps  = (long)suspects.Length * maxAttempts;
        long    steps       = 0;

        InitProgress?.Invoke();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Converging_0_concealed_audio_sectors, suspects.Length));

        foreach(ulong badSector in suspects)
        {
            if(_aborted) break;

            byte[] merged         = null;                   // full sector, improved as clean bytes arrive
            var    cleanValue     = new byte[C2_DATA_SIZE]; // last C2-clean value seen for each byte
            var    haveClean      = new bool[C2_DATA_SIZE]; // a clean value has been observed for this byte
            var    confirmed      = new bool[C2_DATA_SIZE]; // two agreeing clean reads seen for this byte
            var    confirmedCount = 0;
            var    attemptsRun    = 0;

            for(var attempt = 0; attempt < maxAttempts && !_aborted; attempt++)
            {
                UpdateProgress?.Invoke(string.Format(Localization.Core.Converging_concealed_audio_sector_0_attempt_1,
                                                     badSector,
                                                     attempt + 1),
                                       steps + attemptsRun,
                                       totalSteps);

                attemptsRun++;

                if(!TryReadAudioC2Aligned(badSector,
                                          offsetBytes,
                                          sectorsForOffset,
                                          supportedSubchannel,
                                          out byte[] aligned,
                                          out bool[] cleanMask))
                    continue;

                // Baseline: keep a full plausible sector so bytes never seen clean are not left as zeros.
                merged ??= (byte[])aligned.Clone();

                for(var j = 0; j < (int)C2_DATA_SIZE; j++)
                {
                    // A byte is accepted only when two C2-clean reads agree on it, merging good bytes across reads.
                    if(confirmed[j] || !cleanMask[j]) continue;

                    if(!haveClean[j])
                    {
                        haveClean[j]  = true;
                        cleanValue[j] = aligned[j];
                        merged[j]     = aligned[j];
                    }
                    else if(cleanValue[j] == aligned[j])
                    {
                        confirmed[j] = true;
                        merged[j]    = aligned[j];
                        confirmedCount++;
                    }
                    else
                    {
                        // Two clean reads disagree on this byte: C2 is unreliable here. Adopt the newest and keep going.
                        cleanValue[j] = aligned[j];
                        merged[j]     = aligned[j];
                    }
                }

                if(confirmedCount == (int)C2_DATA_SIZE) break;
            }

            // Account for this sector's whole retry budget so the bar advances even when it converges early.
            steps += maxAttempts;

            // A sector is only clean when every byte was confirmed by two agreeing C2-clean reads.
            if(merged is not null && confirmedCount == (int)C2_DATA_SIZE)
            {
                outputOptical.WriteSectorLong(merged, badSector, false, SectorStatus.Dumped);
                extents.Add(badSector);
                converged++;
                _c2SuspectAudio.Remove(badSector);
                _resume.ConcealedBlocks.Remove(badSector);
                _mediaGraph?.PaintSectorGood(badSector);
                _errorLog?.WriteLine(badSector, Localization.Core.Reason_audio_converged_C2);

                continue;
            }

            // Could not be made clean. Keep the best data we obtained (better than the concealed original) but mark
            // the sector as bad so it is reported and tracked as not correctly dumped at the end of the dump.
            if(merged is not null)
            {
                outputOptical.WriteSectorLong(merged, badSector, false, SectorStatus.Dumped);
                improved++;

                _errorLog?.WriteLine(badSector,
                                     string.Format(Localization.Core.Reason_audio_merged_0_1,
                                                   confirmedCount,
                                                   (int)C2_DATA_SIZE));
            }
            else
            {
                failed++;
                _errorLog?.WriteLine(badSector, Localization.Core.Reason_audio_not_rereadable_C2);
            }

            if(!_resume.BadBlocks.Contains(badSector)) _resume.BadBlocks.Add(badSector);

            extents.Remove(badSector);
            _mediaGraph?.PaintSectorBad(badSector);
        }

        _resume.BadBlocks.Sort();

        EndProgress?.Invoke();

        UpdateStatus?.Invoke(string.Format(Localization.Core.C2_convergence_result_0_1_2, converged, improved, failed));

        AaruLogging.WriteLine(string.Format(Localization.Core.C2_audio_convergence_complete_0_1_2,
                                            converged,
                                            improved,
                                            failed));
    }

    /// <summary>
    ///     Reads a single audio sector requesting C2 error pointers, applying the same read-offset window handling as the
    ///     retry path, and returns the offset-aligned 2352-byte audio together with a per-byte mask of which of those
    ///     aligned bytes the drive reported as C2-clean. The C2 mask is shifted through the same offset as the data.
    /// </summary>
    /// <returns><c>true</c> if the read succeeded and produced aligned data.</returns>
    bool TryReadAudioC2Aligned(ulong         badSector,           int        offsetBytes, int        sectorsForOffset,
                               MmcSubchannel supportedSubchannel, out byte[] aligned,     out bool[] cleanMask)
    {
        aligned   = null;
        cleanMask = null;

        byte sectorsToReRead   = 1;
        var  badSectorToReRead = (uint)badSector;
        var  offsetFix         = 0;

        // OmniDrive always needs the offset window (like the retry path); the generic path only when fixing offset.
        if((_fixOffset || _omnidrive) && offsetBytes != 0)
        {
            if(offsetBytes < 0)
            {
                if(badSectorToReRead == 0)
                    badSectorToReRead = uint.MaxValue - (uint)(sectorsForOffset - 1);
                else
                    badSectorToReRead -= (uint)sectorsForOffset;
            }

            sectorsToReRead += (byte)sectorsForOffset;

            // Same slice offset FixOffsetData applies to the data, so the C2 mask lines up with the aligned audio.
            offsetFix = offsetBytes < 0 ? (int)C2_DATA_SIZE * sectorsForOffset + offsetBytes : offsetBytes;
        }

        if(!TryReadWindow(badSectorToReRead,
                          sectorsToReRead,
                          supportedSubchannel,
                          out byte[] buf,
                          out int stride,
                          out bool haveC2))
            return false;

        int windowBytes = (int)C2_DATA_SIZE * sectorsToReRead;

        if(offsetFix < 0 || offsetFix + (int)C2_DATA_SIZE > windowBytes || buf.Length < sectorsToReRead * stride)
            return false;

        aligned   = new byte[C2_DATA_SIZE];
        cleanMask = new bool[C2_DATA_SIZE];

        for(var j = 0; j < (int)C2_DATA_SIZE; j++)
        {
            int windowByte = offsetFix + j;                  // byte index within the concatenated window audio
            int s          = windowByte / (int)C2_DATA_SIZE; // which read sector it falls in
            int k          = windowByte % (int)C2_DATA_SIZE; // byte within that sector

            aligned[j] = buf[s * stride + k];

            // Without C2 the byte cannot be confirmed clean, so it stays concealed (mask false).
            if(!haveC2) continue;

            // C2 sits right after the audio in every convergence read (data + C2, or data + C2 + sub on OmniDrive), so
            // its offset is C2_DATA_SIZE. One C2 bit per audio byte, MSB-first (the order Aaru's ECC fixer tries first).
            // Wrong-order guesses are caught by the two-agreeing-reads rule: a mislabelled concealed byte won't agree.
            byte c2Byte = buf[s * stride + (int)C2_DATA_SIZE + (k >> 3)];

            cleanMask[j] = (c2Byte & 0x80 >> (k & 7)) == 0;
        }

        return true;
    }

    /// <summary>
    ///     Reads an audio window with C2 error pointers, falling back through less capable reads so a readable sector
    ///     always yields data. Order: cache-busting C2 read, plain C2 read, then a plain audio read (no C2) to at least
    ///     recover the samples. Reports the per-sector stride and whether C2 information is present.
    /// </summary>
    bool TryReadWindow(uint     lba, byte count, MmcSubchannel supportedSubchannel, out byte[] buf, out int stride,
                       out bool haveC2)
    {
        buf    = null;
        stride = 0;
        haveC2 = false;

        if(_omnidrive)
        {
            // Prefer a cache-busting (FUA) C2 read, then without FUA, so a drive that rejects FUA still gives C2.
            foreach(bool fua in new[]
                    {
                        true, false
                    })
            {
                bool s = _dev.OmniDriveReadCdWithC2(out byte[] b, out _, lba, count, _dev.Timeout, out _, fua);

                if(s || _dev.Error || b is null || b.Length < count * _c2BlockSize) continue;

                buf    = b;
                stride = (int)_c2BlockSize;
                haveC2 = true;

                return true;
            }

            // Last resort: a plain read (data + subchannel, no C2) to at least recover the audio samples.
            bool ps = _dev.OmniDriveReadCd(out byte[] pb, out _, lba, count, _dev.Timeout, out _);

            if(ps || _dev.Error || pb is null || pb.Length < count * 2448) return false;

            buf    = pb;
            stride = 2448;

            return true;
        }

        // Convergence only needs audio + C2, not subchannel; requesting subchannel alongside C2 makes some drives
        // reject the command. Without subchannel the layout is simply data + C2, so C2 sits at offset C2_DATA_SIZE.
        // Read one sector at a time and assemble the window: some drives abort a multi-sector READ CD with C2 even
        // though single-sector C2 reads succeed.
        var c2Stride    = (int)(C2_DATA_SIZE + C2_POINTERS);
        var c2Assembled = new byte[c2Stride * count];
        var c2Ok        = true;

        for(var s = 0; s < count; s++)
        {
            bool oneSense = _dev.ReadCd(out byte[] one,
                                        out _,
                                        lba + (uint)s,
                                        (uint)c2Stride,
                                        1,
                                        MmcSectorTypes.Cdda,
                                        false,
                                        false,
                                        false,
                                        MmcHeaderCodes.None,
                                        true,
                                        false,
                                        MmcErrorField.C2Pointers,
                                        MmcSubchannel.None,
                                        _dev.Timeout,
                                        out _);

            if(oneSense || _dev.Error || one is null || one.Length < c2Stride)
            {
                c2Ok = false;

                break;
            }

            Array.Copy(one, 0, c2Assembled, s * c2Stride, c2Stride);
        }

        if(c2Ok)
        {
            buf    = c2Assembled;
            stride = c2Stride;
            haveC2 = true;

            return true;
        }

        // Fall back to a plain audio read (no C2, no subchannel) to at least recover the samples.
        bool pSense = _dev.ReadCd(out byte[] pgb,
                                  out _,
                                  lba,
                                  C2_DATA_SIZE,
                                  count,
                                  MmcSectorTypes.Cdda,
                                  false,
                                  false,
                                  false,
                                  MmcHeaderCodes.None,
                                  true,
                                  false,
                                  MmcErrorField.None,
                                  MmcSubchannel.None,
                                  _dev.Timeout,
                                  out _);

        if(pSense || _dev.Error || pgb is null || pgb.Length < count * (int)C2_DATA_SIZE) return false;

        buf    = pgb;
        stride = (int)C2_DATA_SIZE;

        return true;
    }

    /// <summary>
    ///     Returns whether the C2 error block of the sector at index <paramref name="index" /> inside a multi-sector C2
    ///     read buffer has any bit set, i.e. the drive concealed at least one byte of that audio sector.
    /// </summary>
    bool SectorHasC2Error(byte[] block, int index)
    {
        int baseOffset = index * (int)_c2BlockSize + _c2Offset;

        if(block.Length < baseOffset + (int)C2_POINTERS) return false;

        for(var b = 0; b < C2_POINTERS; b++)
        {
            if(block[baseOffset + b] != 0) return true;
        }

        return false;
    }

    /// <summary>
    ///     Deinterleaves the raw 96-byte subchannel found at <paramref name="offset" /> inside <paramref name="block" />
    ///     and validates the CRC-16 of its Q channel.
    /// </summary>
    /// <returns><c>true</c> if the Q channel CRC is valid.</returns>
    static bool ValidateSubchannelQ(byte[] block, int offset)
    {
        if(block.Length < offset + (int)C2_SUB_SIZE) return false;

        var raw = new byte[C2_SUB_SIZE];
        Array.Copy(block, offset, raw, 0, (int)C2_SUB_SIZE);

        byte[] deinterleaved = Subchannel.Deinterleave(raw);

        // Q channel is the second 12-byte group of the deinterleaved buffer.
        var q = new byte[12];
        Array.Copy(deinterleaved, 12, q, 0, 12);

        CRC16CcittContext.Data(q, 10, out byte[] crc);

        return crc[0] == q[10] && crc[1] == q[11];
    }
}