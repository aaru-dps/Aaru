// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Calibration.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Rotation calibration for Data Position Measurement.
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
using Aaru.CommonTypes.Structs;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dpm;

public sealed partial class DpmMeasurement
{
    const int CAL_MAX_D = 128;
    /// <summary>Stop the sweep after this many drops...</summary>
    const int CAL_WANT_WRAPS = 5;
    /// <summary>...but not before this many steps</summary>
    const int CAL_MIN_D = 24;
    /// <summary>At least 2 wrap intervals for the spacing check</summary>
    const int CAL_MIN_WRAPS = 3;
    const int CAL_ATTEMPTS = 12;
    /// <summary>Two sweeps must agree within 1.5%</summary>
    const double CAL_AGREE = 0.015;
    /// <summary>Timed vs counted sectors per rotation</summary>
    const double CAL_SPACING_TOL = 0.04;
    /// <summary>|step - s - m*T| must be within 6% of T</summary>
    const double CAL_MULT_TOL = 0.06;
    /// <summary>Inlier band around the median step</summary>
    const double POINT_STEP_TOL = 0.25;
    const double T_DRIFT_WARN = 0.01;
    /// <summary>Local T this far off means the speed changed</summary>
    const double T_JUMP = 0.03;

    /// <summary>
    ///     One calibration sweep at <paramref name="lba" />: walks d upward, in timing units, tracing the sawtooth of
    ///     paired reads, then derives the step slope s (ms per unit) and rotation period T robustly.
    /// </summary>
    /// <remarks>
    ///     Every step of the sweep is s + m*T for an integer m: m = 0 for a step that stays on the slope, m = -1 for a
    ///     genuine wrap, and a +1/-1 pair for a sample that missed its window and waited an extra rotation.
    ///     <list type="bullet">
    ///         <item>
    ///             Slope and wrap steps are told apart by sign, which holds for any s in (0, T). On DVD, where more than
    ///             half the steps wrap, the slope can't be taken as the majority step.
    ///         </item>
    ///         <item>
    ///             s and T start as the two clusters' medians and are refined by a least-squares fit of step = s + m*T
    ///             over every step that is a clean integer multiple; anything else is rejected as noise.
    ///         </item>
    ///         <item>
    ///             Genuine wraps are the steps where the running rotation count reaches a new low, so a missed-window
    ///             +1/-1 pair cancels out, and their spacing is units per rotation found by counting alone. It must match
    ///             T/s, or the sweep is rejected.
    ///         </item>
    ///     </list>
    /// </remarks>
    bool CalibrateOnce(uint lba, out double tMs, out double aps)
    {
        tMs = 0;
        aps = 0;

        var  e         = new double[CAL_MAX_D];
        var  diffs     = new double[CAL_MAX_D];
        var  positive  = new double[CAL_MAX_D];
        var  negative  = new double[CAL_MAX_D];
        var  wrapPos   = new int[CAL_MAX_D];
        uint u         = _unit;
        uint baseLba   = PlaceSweep(lba, CAL_MAX_D * u, false);
        int  cacheHits = 0;
        int  d         = 0, drops = 0, moves = 0;

        while(d < CAL_MAX_D)
        {
            e[d] = PairElapsedMs(baseLba, (uint)d * u, ref cacheHits);

            if(e[d] < 0)
            {
                // Enough of a sweep already
                if(d >= CAL_MIN_D) break;

                // Unreadable block in the way: restart the sweep a little past it, or before the original start if that
                // runs out of the layer.
                uint at = IsBad(baseLba) ? baseLba : baseLba + (uint)d * u;
                LayerBounds(baseLba, out uint lo, out uint hi);
                uint past = at - at % u + 16 * u;
                uint span = CAL_MAX_D * u;

                if(++moves > 6)
                {
                    AaruLogging.Debug(MODULE_NAME, "  calibration: too many unreadable blocks near sector {0}", lba);

                    return false;
                }

                if((ulong)past + span <= (ulong)hi + 1)
                    baseLba = past;
                else if(baseLba >= lo + span + 16 * u)
                    baseLba = baseLba - span - 16 * u;
                else
                {
                    AaruLogging.Debug(MODULE_NAME, "  calibration read failure at d={0}", d);

                    return false;
                }

                d = drops = 0;

                continue;
            }

            if(d > 0)
            {
                diffs[d - 1] = e[d] - e[d - 1];

                if(diffs[d - 1] < 0) drops++;
            }

            d++;

            if(drops >= CAL_WANT_WRAPS && d >= CAL_MIN_D) break;
        }

        int nd = d - 1;
        int np = 0, nn = 0;

        for(int i = 0; i < nd; i++)
        {
            if(diffs[i] > 0)
                positive[np++] = diffs[i];
            else
                negative[nn++] = diffs[i];
        }

        if(np < 2 || nn < CAL_MIN_WRAPS)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "  sweep rejected: {0} slope and {1} wrap steps within {2} steps",
                              np,
                              nn,
                              nd);

            return false;
        }

        double s = Median(positive, np);
        double t = s - Median(negative, nn);

        int nc = 0;

        for(int iter = 0; iter < 3; iter++)
        {
            double n  = 0, sm = 0, smm = 0, sd = 0, smd = 0;
            long   mLo = 0, mHi = 0;
            nc = 0;

            for(int i = 0; i < nd; i++)
            {
                long m = (long)Math.Round((diffs[i] - s) / t, MidpointRounding.AwayFromZero);

                if(Math.Abs(diffs[i] - s - m * t) >= CAL_MULT_TOL * t) continue;

                n   += 1;
                sm  += m;
                smm += (double)m * m;
                sd  += diffs[i];
                smd += m * diffs[i];

                if(m < mLo) mLo = m;
                if(m > mHi) mHi = m;

                nc++;
            }

            double det = n * smm - sm * sm;

            if(mLo == mHi || det <= 0)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "  sweep rejected: steps don't separate into slope and whole rotations");

                return false;
            }

            s = (sd * smm - sm * smd) / det;
            t = (n  * smd - sm * sd)  / det;

            if(!(s <= 0) && !(t <= s)) continue;

            AaruLogging.Debug(MODULE_NAME, "  sweep rejected: implausible fit (slope {0:F4} ms, T {1:F4} ms)", s, t);

            return false;
        }

        int inconsistent = nd - nc;

        if(inconsistent * 8 > nd)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "  sweep rejected: {0} of {1} steps are noise, not slope plus whole rotations",
                              inconsistent,
                              nd);

            return false;
        }

        // Genuine wrap positions: new lows of the running rotation count
        int kCum = 0, kMin = 0, nWrap = 0;

        for(int i = 0; i < nd; i++)
        {
            long m = (long)Math.Round((diffs[i] - s) / t, MidpointRounding.AwayFromZero);

            if(m == 0 || Math.Abs(diffs[i] - s - m * t) >= CAL_MULT_TOL * t) continue;

            kCum += (int)m;

            if(kCum >= kMin) continue;

            kMin              = kCum;
            wrapPos[nWrap++] = i + 1;
        }

        if(nWrap < CAL_MIN_WRAPS)
        {
            AaruLogging.Debug(MODULE_NAME, "  sweep rejected: only {0} genuine wraps", nWrap);

            return false;
        }

        // Integer wrap positions put the counted spacing within 1/(n-1) units of the true one
        double uprCount = (double)(wrapPos[nWrap - 1] - wrapPos[0]) / (nWrap - 1);
        double uprTime  = t / s;

        if(Math.Abs(uprCount - uprTime) > 1.0 / (nWrap - 1) + CAL_SPACING_TOL * uprTime)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "  sweep rejected: timing says {0:F2} sectors/rotation but wraps are {1:F2} sectors apart",
                              uprTime  * u,
                              uprCount * u);

            return false;
        }

        tMs = t;
        aps = 360.0 * s / (t * u);

        AaruLogging.Debug(MODULE_NAME,
                          "  sweep at {0}: {1} points, {2} wraps ({3} noise steps), {4} cache hits deflected; slope={5:F4} ms/sector, T={6:F3} ms, {7:F2} sectors/rotation (counted {8:F2})",
                          baseLba,
                          d,
                          nWrap,
                          inconsistent,
                          cacheHits,
                          s / u,
                          t,
                          uprTime  * u,
                          uprCount * u);

        return true;
    }

    /// <summary>
    ///     Validated calibration at <paramref name="lba" />: repeats sweeps until two of them pass the internal checks,
    ///     fall within the medium's geometry bounds, and agree with each other within <see cref="CAL_AGREE" /> in both T
    ///     and degrees per sector. A single sweep is never trusted on its own, a sweep taken while the spindle is still
    ///     adjusting can be internally consistent and still wrong.
    /// </summary>
    bool Calibrate(uint lba, out double tMs, out double aps)
    {
        tMs = 0;
        aps = 0;

        var gT = new double[CAL_ATTEMPTS];
        var gA = new double[CAL_ATTEMPTS];
        int ng = 0;

        ApsBounds(lba, out double lo, out double hi);
        AaruLogging.Debug(MODULE_NAME, "Sawtooth calibration at sector {0}...", lba);

        for(int attempt = 0; attempt < CAL_ATTEMPTS && !_aborted; attempt++)
        {
            RestoreSpeedIfNeeded();
            int badBefore = _bad.Count;

            if(!CalibrateOnce(lba, out double t, out double a)) continue;

            // Hit a new unreadable sector, the drive may have slowed partway, so don't trust it
            if(_bad.Count != badBefore) continue;

            if(a < lo || a > hi)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "  sweep rejected: {0:F4} deg/sector is outside {1} geometry bounds [{2:F2}, {3:F2}] at sector {4}",
                                  a,
                                  _kind,
                                  lo,
                                  hi,
                                  lba);

                continue;
            }

            for(int j = 0; j < ng; j++)
            {
                if(!(Math.Abs(t - gT[j]) / gT[j] < CAL_AGREE) || !(Math.Abs(a - gA[j]) / gA[j] < CAL_AGREE)) continue;

                tMs = (t + gT[j]) / 2.0;
                aps = (a + gA[j]) / 2.0;

                AaruLogging.Debug(MODULE_NAME,
                                  "  -> rotation period T={0:F3} ms ({1:F1} RPM), {2:F4} deg/sector ({3:F2} sectors/rotation) at sector {4}",
                                  tMs,
                                  60000.0 / tMs,
                                  aps,
                                  360.0 / aps,
                                  lba);

                _calibrations.Add(new DpmCalibration
                {
                    Lba            = lba,
                    RotationPeriod = (ulong)Math.Round(tMs * 1e6),
                    SectorsPerTurn = (ulong)Math.Round(360.0 / aps * 1000.0)
                });

                return true;
            }

            gT[ng] = t;
            gA[ng] = a;
            ng++;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Calibration failed at sector {0}: no two of {1} sweeps passed the checks and agreed within {2:F1}%",
                          lba,
                          CAL_ATTEMPTS,
                          CAL_AGREE * 100.0);

        return false;
    }

    /// <summary>
    ///     Slope, in ms per sector, at one control point with T already known: a short sawtooth sweep centred on the
    ///     point, each step reduced modulo T into [0, T), which maps slope and wrap steps alike onto the slope. A sample
    ///     that missed its window by whole rotations lands back on the slope too; anything else lands at a random residue,
    ///     which the median of all reduced steps ignores.
    /// </summary>
    /// <param name="lba">Control point</param>
    /// <param name="tMs">Rotation period</param>
    /// <param name="tLocal">This sweep's own rotation period estimate, or 0 if the sweep had no wrap</param>
    /// <param name="cacheHits">Cache hit counter</param>
    /// <returns>Slope, or -1 if it could not be measured</returns>
    double SlopeAtPoint(uint lba, double tMs, out double tLocal, ref int cacheHits)
    {
        var  steps    = new double[64];
        var  sorted   = new double[64];
        var  positive = new double[64];
        var  negative = new double[64];
        int  np       = 0, nn = 0;
        uint u        = _unit;
        int  nSteps   = _pointSteps;
        uint baseLba  = PlaceSweep(lba, (uint)nSteps * u, true);
        double previous = -1;
        int    n        = 0;

        for(int d = 0; d <= nSteps; d++)
        {
            double e = PairElapsedMs(baseLba, (uint)d * u, ref cacheHits);

            if(e < 0)
            {
                // Don't difference across a missing sample
                previous = -1;

                continue;
            }

            if(previous >= 0)
            {
                if(e - previous > 0)
                    positive[np++] = e - previous;
                else
                    negative[nn++] = e - previous;

                double step = (e - previous) % tMs;

                if(step < 0) step += tMs;

                steps[n++] = step;
            }

            previous = e;
        }

        tLocal = 0;

        if(np > 0 && nn > 0)
        {
            double tl = Median(positive, np) - Median(negative, nn);

            if(tl > 0.5 * tMs && tl < 2.0 * tMs) tLocal = tl;
        }

        if(n < nSteps / 2) return -1.0;

        Array.Copy(steps, sorted, n);
        double median = Median(sorted, n);

        if(median <= 0) return -1.0;

        // A clear majority must agree with the median; average those
        double sum = 0;
        int    ni  = 0;

        for(int i = 0; i < n; i++)
        {
            if(!(Math.Abs(steps[i] - median) <= POINT_STEP_TOL * median)) continue;

            sum += steps[i];
            ni++;
        }

        if(ni < nSteps / 2) return -1.0;

        return sum / ni / u;
    }

    /// <summary>
    ///     Measures one control point, checking that the drive is still spinning at <paramref name="tCurrent" />. Drives
    ///     change speed on their own, and a jump between two periodic re-calibrations would be smeared over every point in
    ///     between. If the sweep's own rotation period disagrees by more than <see cref="T_JUMP" />, re-calibrates here,
    ///     updates <paramref name="tCurrent" /> and measures again.
    /// </summary>
    double MeasurePoint(uint lba, ref double tCurrent, ref int cacheHits, out bool jumped)
    {
        jumped = false;
        double s = SlopeAtPoint(lba, tCurrent, out double tLocal, ref cacheHits);

        if(tLocal <= 0 || Math.Abs(tLocal - tCurrent) / tCurrent <= T_JUMP) return s;

        // A single junk sample can fake a mismatch; a real speed change shows up again on a second sweep
        double tFirst = tLocal;
        double s2     = SlopeAtPoint(lba, tCurrent, out tLocal, ref cacheHits);

        if(tLocal                                 <= 0      ||
           Math.Abs(tLocal - tCurrent) / tCurrent <= T_JUMP ||
           Math.Abs(tLocal - tFirst)   / tFirst   > T_JUMP)
            return s2 > 0 ? s2 : s;

        AaruLogging.Debug(MODULE_NAME,
                          "  point {0}: rotation period looks like {1:F3} ms, not {2:F3} ms - drive changed speed? Re-calibrating.",
                          lba,
                          tLocal,
                          tCurrent);

        if(!Calibrate(lba, out double t, out _)) return -1.0;

        AaruLogging.Debug(MODULE_NAME,
                          "  rotation period changed {0:F3} -> {1:F3} ms ({2:+0.00;-0.00}%)",
                          tCurrent,
                          t,
                          100.0 * (t - tCurrent) / tCurrent);

        tCurrent = t;
        jumped   = true;

        return SlopeAtPoint(lba, tCurrent, out _, ref cacheHits);
    }
}