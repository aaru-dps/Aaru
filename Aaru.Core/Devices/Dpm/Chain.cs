// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Chain.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Chained angle measurement for Data Position Measurement.
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
using Aaru.Logging;

namespace Aaru.Core.Devices.Dpm;

/// <remarks>
///     Reading block B and then, back to back, block B' takes (k + frac(dtheta)) * T, with k a whole number, where
///     dtheta is the rotation from B to B': the command overhead only decides how many whole rotations the drive waits,
///     never the fraction. Re-reading B itself is the dtheta = 0 case, exactly k*T. So one timed read per control point
///     gives the fractional rotation from the previous point, and the whole rotations come from the previous bins'
///     density. Each point costs two reads: the chained read, and a re-read of the same block, which tracks the
///     rotation period continuously.
/// </remarks>
public sealed partial class DpmMeasurement
{
    /// <summary>Rotations off the prediction to be suspect</summary>
    const double CHAIN_TOL = 0.35;
    /// <summary>Rotations two attempts must agree within</summary>
    const double CHAIN_AGREE    = 0.03;
    const int    CHAIN_ATTEMPTS = 3;
    /// <summary>Re-read periods in the running median</summary>
    const int CHAIN_T_HIST = 7;
    /// <summary>Recent bins in the density prediction</summary>
    const int CHAIN_PRED_N = 5;
    /// <summary>Failed reads before a bin is given up as unreadable</summary>
    const int CHAIN_MAX_BAD = 4;
    /// <summary>Rate marker: bin has no readable endpoints</summary>
    const double BIN_UNREADABLE = -3.0;
    /// <summary>Rate marker: bin crosses a layer break or is inside a single timing unit</summary>
    const double BIN_BREAK = -2.0;
    /// <summary>Rate marker: bin measurement failed</summary>
    const double BIN_FAILED = -1.0;

    readonly double[] _history = new double[CHAIN_T_HIST];
    /// <summary>Degrees per sector from the latest calibration</summary>
    double _apsCal;
    /// <summary>Block just read twice, or <see cref="uint.MaxValue" /></summary>
    uint _at;
    int  _cacheHits;
    int  _calibrationCount;
    int  _historyCount, _historyPosition;
    /// <summary>Re-calibrations forced by a rotation period change</summary>
    int _speedRecalibrations;
    /// <summary>Current rotation period, ms</summary>
    double _t;
    /// <summary>Consecutive re-reads off the rotation period by more than <see cref="T_JUMP" /></summary>
    int _tSuspect;
    /// <summary>Consecutive bins given up as unreadable</summary>
    int _unreadStreak;

    void ChainSetT(double t, double aps)
    {
        _t               = t;
        _apsCal          = aps;
        _historyCount    = _historyPosition = 0;
        _tSuspect        = 0;

        // Calibration moved the head
        _at = uint.MaxValue;
    }

    bool ChainCalibrate(uint lba)
    {
        // Calibrate restores the speed if needed
        _calibrationCount++;

        if(!Calibrate(lba, out double t, out double a)) return false;

        if(Math.Abs(t - _t) / _t > T_DRIFT_WARN)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "  rotation period changed {0:F3} -> {1:F3} ms ({2:+0.00;-0.00}%)",
                              _t,
                              t,
                              100.0 * (t - _t) / _t);
        }

        ChainSetT(t, a);

        return true;
    }

    /// <summary>Folds a re-read time, k whole rotations, into the running rotation period median</summary>
    void ChainTrackT(double e0)
    {
        long k = (long)Math.Round(e0 / _t, MidpointRounding.AwayFromZero);

        if(k < 1) return;

        double tp = e0 / k;

        if(Math.Abs(tp - _t) / _t > T_JUMP)
        {
            _tSuspect++;

            return;
        }

        _tSuspect                  = 0;
        _history[_historyPosition] = tp;
        _historyPosition           = (_historyPosition + 1) % CHAIN_T_HIST;

        if(_historyCount < CHAIN_T_HIST) _historyCount++;

        var tmp = new double[CHAIN_T_HIST];
        Array.Copy(_history, tmp, _historyCount);
        _t = Median(tmp, _historyCount);
    }

    /// <summary>
    ///     Reads the block of <paramref name="lba" />, then re-reads it: the head reference for the next chained read,
    ///     plus a rotation period sample.
    /// </summary>
    /// <returns>0 on success, -1 on failure, -3 if the speed was restored and the caller must re-anchor</returns>
    int ChainReadTwice(uint lba, bool wantFirst, out double eFirst)
    {
        eFirst = 0;
        double e = TimedReadMs(Block(lba));

        // First good read after unreadable sectors: the drive has slowed, and timing at that speed with the old T could
        // pass the prediction check and still be wrong. Restore the speed and take the read again.
        if(e >= 0 && RestoreSpeedIfNeeded())
        {
            _at           = uint.MaxValue;
            _historyCount = _historyPosition = 0;
            _tSuspect     = 0;

            // Head moved, the caller must re-anchor
            if(wantFirst) return -3;

            e = TimedReadMs(Block(lba));
        }

        double e0 = e >= 0 ? TimedReadMs(Block(lba)) : e;

        if(e is -2.0 || e0 is -2.0)
        {
            _cacheHits++;
            DefeatCache(lba);
        }

        if(e < 0 || e0 < 0)
        {
            _at = uint.MaxValue;

            return -1;
        }

        ChainTrackT(e0);
        _at    = Block(lba);
        eFirst = e;

        return 0;
    }

    /// <summary>Rotation, in turns, from control point <paramref name="lo" /> to <paramref name="hi" /></summary>
    /// <returns>
    ///     0 and <paramref name="dTheta" /> when two attempts agree, or the first lands within <see cref="CHAIN_TOL" />
    ///     of the prediction, -2 if there is nothing readable to measure across, -1 otherwise
    /// </returns>
    int ChainMeasure(uint lo, uint hi, double apsPrediction, out double dTheta, out uint span)
    {
        dTheta = 0;
        span   = 0;

        // Inside an unreadable region one failed probe is enough to give a bin up: unreadable areas run for many bins,
        // and every failed read can cost the drive seconds.
        int    maxBad    = _unreadStreak > 0 ? 1 : CHAIN_MAX_BAD;
        uint   u         = _unit;
        double previous  = 0;
        uint   prevSpan  = 0;
        bool   havePrev  = false;
        int    a         = 0, guard = 0, badReads = 0;

        while(a < CHAIN_ATTEMPTS && guard++ < CHAIN_ATTEMPTS + 64)
        {
            // Endpoints: the first readable block from lo forward and the last from hi backward, so an unreadable
            // boundary just shortens the span measured. The search steps 1, 2, 4, ... units, so a long unreadable run
            // costs a few failed reads, not one per sector.
            uint s0 = Block(lo), s1 = Block(hi), st;

            for(st = u; s0 < s1 && IsBad(s0); st *= 2) s0 += st;

            for(st = u; s1 > s0 && IsBad(s1); st *= 2) s1 = s1 > st ? s1 - st : 0;

            // Nothing readable to measure across
            if(s1 <= s0 || badReads >= maxBad) return -2;

            double prediction = (s1 - s0) * apsPrediction / 360.0;

            if(_tSuspect >= 2)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "  rotation period off at sector {0} - drive changed speed? Re-calibrating.",
                                  lo);

                _speedRecalibrations++;

                if(!ChainCalibrate(s0)) return -1;
            }

            if(_at != s0 && ChainReadTwice(s0, false, out _) != 0)
            {
                if(IsBad(s0))
                    badReads++;
                else
                    a++; // A transient failure costs an attempt

                continue;
            }

            int r1 = ChainReadTwice(s1, true, out double e);

            // Speed restored: re-anchor, no attempt lost
            if(r1 == -3) continue;

            if(r1 != 0)
            {
                if(IsBad(s1))
                    badReads++;
                else
                    a++;

                continue;
            }

            double fraction = e / _t % 1.0;
            double got      = fraction + Math.Round(prediction - fraction, MidpointRounding.AwayFromZero);

            if(a == 0 && Math.Abs(got - prediction) <= CHAIN_TOL)
            {
                dTheta = got;
                span   = s1 - s0;

                return 0;
            }

            if(havePrev && prevSpan == s1 - s0 && Math.Abs(got - previous) <= CHAIN_AGREE)
            {
                dTheta = (got + previous) / 2.0;
                span   = s1 - s0;

                return 0;
            }

            previous = got;
            prevSpan = s1 - s0;
            havePrev = true;

            // Measure this bin afresh
            _at = uint.MaxValue;
            a++;
        }

        return -1;
    }

    /// <summary>
    ///     Mean degrees per sector over the bin [lo, hi): chained measurement, then a re-calibration and one more try as
    ///     the drive may have changed speed, then a local sawtooth sweep as the last resort.
    /// </summary>
    /// <returns>Degrees per sector, <see cref="BIN_UNREADABLE" />, or <see cref="BIN_FAILED" /> if all fail</returns>
    double MeasureBinOnce(uint lo, uint hi, double apsPrediction)
    {
        int r = ChainMeasure(lo, hi, apsPrediction, out double dTheta, out uint span);

        if(r == 0) return 360.0 * dTheta / span;

        if(r == -2)
        {
            AaruLogging.Debug(MODULE_NAME, "  bin {0}-{1}: no readable blocks to measure across", lo, hi);

            return BIN_UNREADABLE;
        }

        AaruLogging.Debug(MODULE_NAME, "  bin {0}-{1}: chained reads disagree, re-calibrating", lo, hi);

        if(ChainCalibrate(lo))
        {
            r = ChainMeasure(lo, hi, apsPrediction, out dTheta, out span);

            if(r == 0) return 360.0 * dTheta / span;

            if(r == -2) return BIN_UNREADABLE;
        }

        span = Block(hi) - Block(lo);

        AaruLogging.Debug(MODULE_NAME, "  bin {0}-{1}: falling back to a local sweep", lo, hi);

        double s = MeasurePoint(Block(lo) + span / 2, ref _t, ref _cacheHits, out _);
        _at           = uint.MaxValue;
        _historyCount = _historyPosition = 0;

        return s > 0 ? 360.0 * s / _t : BIN_FAILED;
    }

    double MeasureBin(uint lo, uint hi, double apsPrediction)
    {
        double r = MeasureBinOnce(lo, hi, apsPrediction);
        _unreadStreak = r == BIN_UNREADABLE ? _unreadStreak + 1 : 0;

        return r;
    }
}