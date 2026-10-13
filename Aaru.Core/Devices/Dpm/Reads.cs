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
    ///     Re-reading the same sector is not something a look-ahead cache serves, so no cache defeat is needed, and it
    ///     would only add seek noise.
    /// </remarks>
    void SpinUpSettle(uint lba)
    {
        long previous    = 0;
        int  stableCount = 0;
        long deadline    = _drive.Now + (long)(SETTLE_MAX_SEC * 1e9);

        AaruLogging.Debug(MODULE_NAME, "Waiting for spindle to spin up and stabilize...");

        // An unreadable sector here would be retried by the drive for many seconds on every pass, move along
        while(!_drive.TimedRead(lba, out _, out _) && _drive.Now < deadline) lba += 16;

        while(_drive.Now < deadline)
        {
            // Since the previous read finished, a whole number of turns whatever the host took in between
            if(!_drive.TimedRead(lba, out _, out long delta))
            {
                lba         += 16;
                previous    =  0;
                stableCount =  0;

                continue;
            }

            if(previous != 0)
            {
                double change = Math.Abs((double)delta - previous) / previous;

                if(change < SETTLE_TOLERANCE)
                {
                    stableCount++;

                    if(stableCount >= 3) break;
                }
                else
                    stableCount = 0;
            }

            previous = delta;
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

        if(!ReadBlock(s, out _, out _)) return 0;

        return ReadBlock(s + d, out elapsed, out long sincePrevious) ? sincePrevious : 0;
    }

    /// <summary>Median of the paired samples, detecting and retrying cache hits</summary>
    /// <returns>Milliseconds, or -1 if no sample could be taken</returns>
    double PairElapsedMs(uint s, uint d, ref int cacheHits)
    {
        var values   = new double[PAIR_REPS_MAX];
        int n        = 0;
        int attempts = 0;

        while(n < _reps && attempts < _reps * 3)
        {
            attempts++;
            long turned = PairSample(s, d, out long elapsed);

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

    /// <summary>Median of the first <paramref name="n" /> values, sorting them in place</summary>
    internal static double Median(double[] values, int n)
    {
        Array.Sort(values, 0, n);

        return (n & 1) == 1 ? values[n / 2] : (values[n / 2 - 1] + values[n / 2]) / 2.0;
    }
}