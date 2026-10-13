// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reads.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Timed reads, unreadable sectors, spin-up and cache defeat for Data Position Measurement.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dpm;

public sealed partial class DpmMeasurement
{
    const double SETTLE_MAX_SEC   = 3.0;
    const double SETTLE_TOLERANCE = 0.02;
    /// <summary>Less than 1.5 ms is impossible from the media, it is a cache hit</summary>
    const long PAIR_CACHE_HIT_NS = 1500000;
    const int  PAIR_REPS_MAX     = 3;
    /// <summary>32 MiB, well past typical drive buffers</summary>
    const uint CACHE_DEFEAT_SECTORS = 16384;
    const ushort CACHE_DEFEAT_CHUNK = 64;
    /// <summary>Forces a large radial seek, not just a different buffer segment nearby</summary>
    const uint CACHE_DEFEAT_OFFSET = 200000;
    /// <summary>Sectors added to the measured read-ahead, in case the drive reads a little further elsewhere</summary>
    const uint READ_AHEAD_MARGIN = 4;
    /// <summary>Largest read-ahead looked for; past it reads that can't bypass the cache are not timed</summary>
    const uint READ_AHEAD_MAX = 4096;

    /// <summary>Unreadable timing units. A failed read is remembered and never retried.</summary>
    readonly HashSet<uint> _bad = [];
    /// <summary>Timing units the dump read that failed once here, the next failure makes them unreadable</summary>
    readonly HashSet<uint> _failedOnce = [];
    /// <summary>Whether the sectors the dump could not read are known, so any other sector was read by it</summary>
    bool _dumpKnown;
    /// <summary>Whether the last read failed</summary>
    bool _lastReadFailed;
    /// <summary>Failed reads so far</summary>
    int _readErrors;
    /// <summary>Failed reads when the speed was last restored</summary>
    int _errorsRestored;
    /// <summary>Last sector read successfully</summary>
    uint _lastGood;
    /// <summary>
    ///     Distance after a read past which a read that can't bypass the cache comes from the medium, see
    ///     <see cref="ReadAheadFor" />
    /// </summary>
    uint _readAhead;
    /// <summary>Whether <see cref="_readAhead" /> was measured</summary>
    bool _readAheadKnown;

    /// <summary>Whether the timing unit of a sector is known to be unreadable</summary>
    bool IsBad(uint lba) => _bad.Contains(lba / _unit);

    /// <summary>Remembers a sector as unreadable</summary>
    void MarkBad(uint lba)
    {
        if(!_bad.Add(lba / _unit)) return;

        AaruLogging.Debug(MODULE_NAME,
                          "  sector {0} unreadable - skipping {1} from now on",
                          lba,
                          _unit > 1 ? "its block" : "it");
    }

    /// <summary>
    ///     Marks sectors already known to be unreadable, so they are never read. The list is taken as complete: any
    ///     other sector was read by the dump, so a read of it that fails is retried once before giving it up.
    /// </summary>
    /// <param name="sectors">Unreadable sectors, e.g. from a dump</param>
    public void SeedUnreadable(IEnumerable<ulong> sectors)
    {
        foreach(ulong sector in sectors)
            if(sector <= uint.MaxValue)
                _bad.Add((uint)sector / _unit);

        _dumpKnown = true;
    }

    /// <summary>Single sector read, timed, that remembers failures</summary>
    /// <param name="lba">Sector address</param>
    /// <param name="elapsed">Nanoseconds the command took, to tell cache hits</param>
    /// <param name="sincePrevious">
    ///     Nanoseconds since the previous command finished, which is how far the disc turned, whatever the host did in
    ///     between
    /// </param>
    bool ReadBlock(uint lba, out long elapsed, out long sincePrevious)
    {
        elapsed       = 0;
        sincePrevious = 0;

        if(IsBad(lba)) return false;

        if(!_drive.TimedRead(lba, out elapsed, out sincePrevious))
        {
            _readErrors++;

            // The dump read it, so this may be the drive having a bad moment, unless it follows other failures, as in an
            // area that went bad since the dump
            bool retry = _dumpKnown && !_lastReadFailed && _unreadStreak == 0 && _failedOnce.Add(lba / _unit);
            _lastReadFailed = true;

            if(retry)
            {
                AaruLogging.Debug(MODULE_NAME, "  sector {0} failed to read, but the dump read it - will retry", lba);

                return false;
            }

            MarkBad(lba);

            return false;
        }

        _lastGood       = lba;
        _lastReadFailed = false;

        return true;
    }

    /// <summary>
    ///     After read errors the drive drops its spindle speed and keeps it down until told otherwise, and a re-read
    ///     can't be trusted to notice. So after any new unreadable sector, ask for the speed again and let it spin up
    ///     before timing anything.
    /// </summary>
    /// <returns><c>true</c> if the speed was restored</returns>
    bool RestoreSpeedIfNeeded()
    {
        if(_readErrors == _errorsRestored) return false;

        AaruLogging.Debug(MODULE_NAME,
                          "  after unreadable sectors: re-sending SET CD SPEED (drives slow down after read errors)");

        _drive.SetSpeed(_kbps);

        // Settle on a sector known to read, not one inside the bad area
        SpinUpSettle(_lastGood);
        _errorsRestored = _readErrors;

        return true;
    }

    /// <summary>
    ///     Lets the spindle spin up and stabilize before any timed measurement: repeatedly re-reads the sector and waits
    ///     until consecutive timings stop changing beyond <see cref="SETTLE_TOLERANCE" />, or
    ///     <see cref="SETTLE_MAX_SEC" /> elapses.
    /// </summary>
    /// <remarks>
    ///     Re-reading the same sector is not something a look-ahead cache serves when the read bypasses the cache, so no
    ///     cache defeat is needed, and it would only add seek noise. Where it can't, the drive serves the re-read from its
    ///     cache, so the sector alternates with one past what the drive read ahead, and each delay is compared with the
    ///     one two reads before.
    /// </remarks>
    void SpinUpSettle(uint lba)
    {
        long[] previous    = [0, 0];
        int    stableCount = 0;
        long   deadline    = _drive.Now + (long)(SETTLE_MAX_SEC * 1e9);
        int    read        = 0;

        AaruLogging.Debug(MODULE_NAME, "Waiting for spindle to spin up and stabilize...");

        // An unreadable sector here would be retried by the drive for many seconds on every pass, move along
        while(!_drive.TimedRead(lba, out _, out _) && _drive.Now < deadline) lba += 16;

        uint ahead = ReadAheadFor(lba);
        uint other = ahead is > 0 and < uint.MaxValue && (ulong)lba + ahead <= _lastLba ? lba + ahead : lba;

        while(_drive.Now < deadline)
        {
            int parity = other == lba ? 0 : read & 1;
            read++;

            // Since the previous read finished, a whole number of turns whatever the host took in between, or the same
            // fraction of one as two reads before when alternating
            if(!_drive.TimedRead(parity == 0 ? lba : other, out _, out long delta))
            {
                lba         += 16;
                other       =  other == lba - 16 ? lba : other + 16;
                previous    =  [0, 0];
                stableCount =  0;

                continue;
            }

            if(previous[parity] != 0)
            {
                double change = Math.Abs((double)delta - previous[parity]) / previous[parity];

                if(change < SETTLE_TOLERANCE)
                {
                    stableCount++;

                    if(stableCount >= 3) break;
                }
                else
                    stableCount = 0;
            }

            previous[parity] = delta;
        }

        AaruLogging.Debug(MODULE_NAME, "Spindle settled.");
    }

    /// <summary>Reads a region big enough to fill the drive cache, evicting the sectors being timed</summary>
    void DefeatCacheRegion(uint decoyStart)
    {
        uint remaining = CACHE_DEFEAT_SECTORS;
        uint lba       = decoyStart;

        while(remaining > 0)
        {
            ushort chunk = remaining > CACHE_DEFEAT_CHUNK ? CACHE_DEFEAT_CHUNK : (ushort)remaining;

            // Errors ignored, it is a best effort flush
            _drive.ReadUntimed(lba, chunk);
            lba       += chunk;
            remaining -= chunk;
        }
    }

    /// <summary>
    ///     Fills the drive cache from two distinct regions far from <paramref name="avoidLba" />, in case the drive keeps
    ///     more than one cached window.
    /// </summary>
    /// <remarks>
    ///     Both regions must be on the medium: past its end the reads fail at once and fill nothing, so on a medium
    ///     shorter than the offset every flush did nothing and every timed read kept coming from the cache.
    /// </remarks>
    void DefeatCache(uint avoidLba)
    {
        const uint span = CACHE_DEFEAT_SECTORS * 4;

        uint decoy;

        if(avoidLba > CACHE_DEFEAT_OFFSET)
            decoy = avoidLba - CACHE_DEFEAT_OFFSET;
        else if((ulong)avoidLba + CACHE_DEFEAT_OFFSET + span <= _lastLba)
            decoy = avoidLba + CACHE_DEFEAT_OFFSET;
        else
        {
            // Short medium: start or end, whichever is farther from the sector to avoid
            uint top = _lastLba > span ? _lastLba - span : 0;
            decoy = avoidLba >= top / 2 ? 0 : top;
        }

        DefeatCacheRegion(decoy);
        DefeatCacheRegion(decoy + CACHE_DEFEAT_SECTORS * 3);
    }

    /// <summary>One paired sample: positions on S, then times the read of S+d</summary>
    /// <param name="s">First sector</param>
    /// <param name="d">Distance to the timed sector</param>
    /// <param name="elapsed">Nanoseconds the timed command took, to tell cache hits</param>
    /// <returns>Nanoseconds from the end of the read of S to the end of the read of S+d, or 0 on error</returns>
    long PairSample(uint s, uint d, out long elapsed)
    {
        elapsed = 0;

        EvictIfReadAhead(s);

        if(!ReadBlock(s, out _, out _)) return 0;

        return ReadBlock(s + d, out elapsed, out long sincePrevious) ? sincePrevious : 0;
    }

    /// <summary>Median of the paired samples, detecting and retrying cache hits</summary>
    /// <param name="s">First sector</param>
    /// <param name="d">Distance to the timed sector</param>
    /// <param name="ahead">
    ///     Sectors added to <paramref name="d" /> so the timed read is past what the drive read ahead, see
    ///     <see cref="SweepReadAhead" />. Being the same for a whole sweep, it shifts the sawtooth without changing its
    ///     slope or period.
    /// </param>
    /// <param name="cacheHits">Cache hit counter</param>
    /// <returns>Milliseconds, or -1 if no sample could be taken</returns>
    double PairElapsedMs(uint s, uint d, uint ahead, ref int cacheHits)
    {
        var values   = new double[PAIR_REPS_MAX];
        int n        = 0;
        int attempts = 0;

        while(n < _reps && attempts < _reps * 3)
        {
            attempts++;
            long turned = PairSample(s, ahead + d, out long elapsed);

            if(turned == 0) continue;

            if(elapsed < PAIR_CACHE_HIT_NS)
            {
                // Served from cache, thrash it and retry
                cacheHits++;
                DefeatCache(s);

                continue;
            }

            values[n++] = turned / 1e6;
        }

        return n == 0 ? -1.0 : Median(values, n);
    }

    /// <summary>One timed read</summary>
    /// <returns>
    ///     Milliseconds since the previous command finished, which is how far the disc turned, -1 on error, -2 if served
    ///     from cache
    /// </returns>
    double TimedReadMs(uint lba)
    {
        if(!ReadBlock(lba, out long elapsed, out long sincePrevious)) return -1.0;

        return elapsed < PAIR_CACHE_HIT_NS ? -2.0 : sincePrevious / 1e6;
    }

    /// <summary>
    ///     Sectors to add to the distance of a timed read of <paramref name="lba" /> so it comes from the medium: 0 when
    ///     the read bypasses the cache, otherwise how far the drive reads ahead, measured the first time it is needed.
    /// </summary>
    /// <remarks>
    ///     Audio can only be read with READ CD, which has no Force Unit Access, and the drive serves the sectors it read
    ///     ahead from its cache: a timed read of one of them took no time at all, every sample was taken as a cache hit
    ///     and flushed for, and audio could not be measured. Re-reading a sector is served from the cache too.
    /// </remarks>
    /// <returns>Sectors, or <see cref="uint.MaxValue" /> if the drive reads ahead too far to time these reads</returns>
    uint ReadAheadFor(uint lba)
    {
        if(_drive.BypassesCache(lba)) return 0;

        if(!_readAheadKnown) MeasureReadAhead(lba);

        return _readAhead;
    }

    /// <summary>Read-ahead for a sweep of <paramref name="span" /> sectors from <paramref name="baseLba" /></summary>
    uint SweepReadAhead(uint baseLba, uint span) =>
        Math.Max(ReadAheadFor(baseLba), ReadAheadFor(span > 0 ? baseLba + span - 1 : baseLba));

    /// <summary>
    ///     Finds the shortest distance after a read at which a read of <paramref name="lba" />'s kind comes from the
    ///     medium twice in a row, and keeps it with a margin.
    /// </summary>
    void MeasureReadAhead(uint lba)
    {
        _readAheadKnown = true;
        _readAhead      = uint.MaxValue;

        for(uint d = 1, step = 1; d <= READ_AHEAD_MAX && !_aborted; d += step)
        {
            // Finely at first, where drives' read-ahead usually ends, then coarser
            if(d >= 64) step = d / 8;

            if((ulong)lba + d > _lastLba) break;

            if(!ReadsFromMedium(lba, d) || !ReadsFromMedium(lba, d)) continue;

            _readAhead = d + READ_AHEAD_MARGIN;

            AaruLogging.Debug(MODULE_NAME,
                              "Reads that can't bypass the drive cache come from the medium {0} sectors after the last read, timing them {1} sectors after",
                              d,
                              _readAhead);

            break;
        }

        if(_readAhead == uint.MaxValue)
            AaruLogging.Debug(MODULE_NAME,
                              "The drive reads ahead more than {0} sectors where reads can't bypass its cache, they can't be timed",
                              READ_AHEAD_MAX);

        // The head moved
        _at = uint.MaxValue;
    }

    /// <summary>
    ///     Before reading <paramref name="lba" /> to put the head there, makes sure it isn't in what the drive read
    ///     ahead of the last read, where it can't bypass the cache: it would come back from it without the head moving.
    ///     Reads one sector just before that window, as drives keep only the last one, which is far cheaper than
    ///     flushing the whole cache.
    /// </summary>
    void EvictIfReadAhead(uint lba)
    {
        uint ahead = ReadAheadFor(lba);

        if(ahead is 0 or uint.MaxValue || lba < _lastGood || lba - _lastGood > ahead) return;

        // Before the window when there is room, else past it: either way its own read-ahead can't reach lba
        uint evict = lba > ahead + 1 ? lba - ahead - 1 : lba + 2 * ahead;

        if(evict <= _lastLba) ReadBlock(evict, out _, out _);
    }

    /// <summary>Whether, right after reading <paramref name="s" />, the read of s + d comes from the medium</summary>
    bool ReadsFromMedium(uint s, uint d) => _drive.TimedRead(s, out _, out _) &&
                                            _drive.TimedRead(s + d, out long elapsed, out _) &&
                                            elapsed >= PAIR_CACHE_HIT_NS;

    /// <summary>Median of the first <paramref name="n" /> values, sorting them in place</summary>
    internal static double Median(double[] values, int n)
    {
        Array.Sort(values, 0, n);

        return (n & 1) == 1 ? values[n / 2] : (values[n / 2 - 1] + values[n / 2]) / 2.0;
    }
}