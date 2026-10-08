// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Measure.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Data Position Measurement main pass, outlier handling and interpolation.
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
using System.Diagnostics;
using Aaru.CommonTypes.Structs;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dpm;

public sealed partial class DpmMeasurement
{
    /// <summary>Neighbours each side for the local median</summary>
    const int    OUTLIER_HALF_WIN = 4;
    const double OUTLIER_SIGMA    = 6.0;
    const double OUTLIER_MIN_REL  = 0.03;
    const int    OUTLIER_RETRIES  = 3;

    /// <summary>
    ///     Flags control points that deviate from the median of their <see cref="OUTLIER_HALF_WIN" /> neighbours on each
    ///     side, in the same layer only, by more than max(<see cref="OUTLIER_SIGMA" /> robust sigmas,
    ///     <see cref="OUTLIER_MIN_REL" />). Real DPM features are bands spanning many control points, which the local
    ///     median follows; a misread is one to three isolated points. Failed points are flagged too.
    /// </summary>
    int FlagOutliers(uint[] lba, double[] aps, int n, bool[] flag)
    {
        var rel   = new double[n];
        var tmp   = new double[n];
        var win   = new double[2 * OUTLIER_HALF_WIN];
        int nr    = 0, count = 0;

        for(int i = 0; i < n; i++)
        {
            rel[i] = double.NaN;

            if(aps[i] < 0) continue;

            int nw = 0;
            int lo = i > OUTLIER_HALF_WIN ? i - OUTLIER_HALF_WIN : 0;

            for(int j = lo; j <= i + OUTLIER_HALF_WIN && j < n; j++)
                if(j != i && aps[j] >= 0 && LayerOf(lba[j]) == LayerOf(lba[i]))
                    win[nw++] = aps[j];

            if(nw < OUTLIER_HALF_WIN) continue;

            rel[i]    = aps[i] / Median(win, nw) - 1.0;
            tmp[nr++] = Math.Abs(rel[i]);
        }

        double sigma     = nr > 0 ? 1.4826 * Median(tmp, nr) : 0.0;
        double threshold = Math.Max(OUTLIER_SIGMA * sigma, OUTLIER_MIN_REL);

        for(int i = 0; i < n; i++)
        {
            flag[i] = aps[i] < 0 || !double.IsNaN(rel[i]) && Math.Abs(rel[i]) > threshold;

            if(flag[i]) count++;
        }

        return count;
    }

    /// <summary>
    ///     Median degrees per sector of up to <see cref="CHAIN_PRED_N" /> valid bins before bin <paramref name="j" /> in
    ///     the same layer, or -1 if there are none
    /// </summary>
    double RecentRate(uint[] pt, double[] rate, int j)
    {
        var v  = new double[CHAIN_PRED_N];
        int nv = 0;

        for(int k = j - 1; k >= 0 && nv < CHAIN_PRED_N; k--)
        {
            if(LayerOf(pt[k]) != LayerOf(pt[j])) break;

            if(rate[k] > 0) v[nv++] = rate[k];
        }

        return nv > 0 ? Median(v, nv) : -1.0;
    }

    /// <summary>Median degrees per sector of the valid same layer bins around bin <paramref name="j" /></summary>
    double NeighbourRate(uint[] pt, double[] rate, int nb, int j)
    {
        var v  = new double[2 * OUTLIER_HALF_WIN];
        int nv = 0;
        int lo = j > OUTLIER_HALF_WIN ? j - OUTLIER_HALF_WIN : 0;

        for(int k = lo; k <= j + OUTLIER_HALF_WIN && k < nb; k++)
            if(k != j && rate[k] > 0 && LayerOf(pt[k]) == LayerOf(pt[j]))
                v[nv++] = rate[k];

        return nv > 0 ? Median(v, nv) : -1.0;
    }

    /// <summary>Control points every <see cref="_window" /> sectors, plus both sides of every layer break and the end</summary>
    uint[] ControlPoints(uint startLba, uint endLba)
    {
        List<uint> pt = [];

        for(ulong lba = startLba; lba <= endLba; lba += _window)
        {
            bool skip = false;

            // No bin straddles a layer break: the last sector of a layer and the first of the next are control points
            foreach(uint layerEnd in _layerEnds)
            {
                if(layerEnd < startLba || layerEnd >= endLba) continue;

                if(lba <= layerEnd || pt[^1] > layerEnd) continue;

                if(pt[^1] != layerEnd) pt.Add(layerEnd);

                pt.Add(layerEnd + 1);

                if(lba == layerEnd + 1UL) skip = true;
            }

            if(!skip) pt.Add((uint)lba);
        }

        if(pt[^1] != endLba) pt.Add(endLba);

        return pt.ToArray();
    }

    /// <summary>
    ///     Measures the angle at control points every <see cref="_window" /> sectors by chained reads, re-calibrating the
    ///     rotation period at the start, at each layer start, every <see cref="_recalEvery" /> sectors, and whenever the
    ///     re-reads say it has moved. Bins whose density disagrees with their neighbourhood are re-measured, and
    ///     interpolated over if they never settle.
    /// </summary>
    DataPositionMeasurement? MeasureDpm(uint startLba, uint endLba, double tStart, double apsStart)
    {
        if(endLba <= startLba)
        {
            AaruLogging.Debug(MODULE_NAME, "start_lba must be below end_lba");

            return null;
        }

        uint[] pt     = ControlPoints(startLba, endLba);
        int    nb     = pt.Length - 1;
        var    rate   = new double[nb];
        var    status = new DpmEntryStatus[nb];
        var    flag   = new bool[nb];

        AaruLogging.Debug(MODULE_NAME, "Measuring {0} bins of {1} sectors by chained reads...", nb, _window);

        ChainSetT(tStart, apsStart);
        uint      nextRecal = startLba + _recalEvery;
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Unreadable regions can span dozens of bins, and every failed read costs the drive seconds. So after two
        // unreadable bins in a row, skip ahead in doubling jumps, presuming the skipped bins unreadable, and once a bin
        // reads again, walk back over the skipped ones until the region's real end.
        uint skipLeft = 0, skipN = 0;
        int  skipFrom = 0;

        InitProgress?.Invoke();

        for(int j = 0; j < nb; j++)
        {
            if(_aborted)
            {
                EndProgress?.Invoke();

                return null;
            }

            uint lo = pt[j], hi = pt[j + 1];

            UpdateProgress?.Invoke(string.Format(Localization.Core.Measuring_DPM_at_sector_0, lo), lo, endLba);

            if(skipLeft > 0 && LayerOf(lo) == LayerOf(pt[skipFrom]))
            {
                rate[j]   = BIN_UNREADABLE;
                status[j] = DpmEntryStatus.Unreadable;
                skipLeft--;
                skipN++;

                continue;
            }

            skipLeft = 0;

            // No bin across a layer break, nor one inside a single block: those take their neighbour's density afterwards
            if(LayerOf(lo) != LayerOf(hi) || Block(hi) == Block(lo))
            {
                rate[j]   = BIN_BREAK;
                status[j] = DpmEntryStatus.LayerBreak;

                continue;
            }

            if(j > 0 && LayerOf(lo) != LayerOf(pt[j - 1]))
            {
                AaruLogging.Debug(MODULE_NAME, "Layer {0} starts at sector {1}.", LayerOf(lo), lo);
                ChainCalibrate(lo);
            }
            else if(lo >= nextRecal) ChainCalibrate(lo);

            while(nextRecal <= lo) nextRecal += _recalEvery;

            double prediction    = RecentRate(pt, rate, j);
            int    recalsBefore  = _speedRecalibrations;
            rate[j] = MeasureBin(lo, hi, prediction > 0 ? prediction : _apsCal);

            // A speed change is only confirmed by the second odd re-read, so the bin before was measured while it was
            // already under way.
            if(_speedRecalibrations != recalsBefore && j > 0 && rate[j - 1] >= 0 && LayerOf(pt[j - 1]) == LayerOf(lo))
            {
                double old = rate[j - 1];
                rate[j - 1]   = MeasureBin(pt[j - 1], lo, rate[j] >= 0 ? rate[j] : old);
                status[j - 1] = DpmEntryStatus.Remeasured;

                AaruLogging.Debug(MODULE_NAME,
                                  "  bin {0}-{1}: re-measured after the speed change: {2:F4} -> {3:F4} deg/sector",
                                  pt[j - 1],
                                  lo,
                                  old,
                                  rate[j - 1]);

                _at = uint.MaxValue;
            }

            if(rate[j] == BIN_UNREADABLE && _unreadStreak >= 2)
            {
                if(skipN == 0) skipFrom = j + 1;

                skipLeft = 1u << (_unreadStreak - 1 < 12 ? _unreadStreak - 1 : 12);
                AaruLogging.Debug(MODULE_NAME, "  unreadable region: skipping {0} bins ahead", skipLeft);
            }

            // Restores the speed first
            if(rate[j] >= 0 && _bad.Count != _badRestored) ChainCalibrate(lo);

            if(rate[j] >= 0 && skipN > 0)
            {
                // Walk back over the skipped bins, predicting from the bin just after each, until one is really
                // unreadable.
                AaruLogging.Debug(MODULE_NAME,
                                  "  readable again at sector {0}: checking the {1} skipped bins backwards",
                                  lo,
                                  skipN);

                for(int k = j - 1; k >= skipFrom; k--)
                {
                    double r = MeasureBin(pt[k], pt[k + 1], rate[k + 1]);
                    rate[k]   = r;
                    status[k] = StatusOf(r, DpmEntryStatus.Measured);

                    if(r == BIN_UNREADABLE) break;

                    AaruLogging.Debug(MODULE_NAME,
                                      "  bin {0}-{1}: {2:F4} deg/sector ({3:F2} sectors/rotation)",
                                      pt[k],
                                      pt[k + 1],
                                      r,
                                      360.0 / r);
                }

                skipN         = 0;
                _unreadStreak = 0;
                _at           = uint.MaxValue;

                // The walk back hit the edge
                if(_bad.Count != _badRestored) ChainCalibrate(lo);
            }

            if(rate[j] >= 0) skipN = 0;

            status[j] = StatusOf(rate[j], DpmEntryStatus.Measured);

            if(rate[j] == BIN_UNREADABLE)
            {
                // Already reported
            }
            else if(rate[j] < 0)
                AaruLogging.Debug(MODULE_NAME, "  bin {0}-{1}: measurement failed", lo, hi);
            else
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "  bin {0}-{1}: {2:F4} deg/sector ({3:F2} sectors/rotation)",
                                  lo,
                                  hi,
                                  rate[j],
                                  360.0 / rate[j]);
            }
        }

        EndProgress?.Invoke();
        AaruLogging.Debug(MODULE_NAME, "Main pass: {0:F1} s", stopwatch.Elapsed.TotalSeconds);

        // Re-measure failed and outlying bins, predicting from neighbours
        for(int pass = 1; pass <= OUTLIER_RETRIES && !_aborted; pass++)
        {
            FlagOutliers(pt, rate, nb, flag);
            int todo = 0;

            for(int j = 0; j < nb; j++)
                if(flag[j] && rate[j] != BIN_BREAK && rate[j] != BIN_UNREADABLE)
                    todo++;

            if(todo == 0) break;

            UpdateStatus?.Invoke(string.Format(Localization.Core.Re_measuring_0_DPM_bins_pass_1_of_2,
                                               todo,
                                               pass,
                                               OUTLIER_RETRIES));

            for(int j = 0; j < nb && !_aborted; j++)
            {
                if(!flag[j] || rate[j] == BIN_BREAK || rate[j] == BIN_UNREADABLE) continue;

                // At the edge of an unreadable region sectors are marginal, they read only sometimes, with erratic
                // timing, and every failure there costs seconds. Interpolate instead.
                if(j > 0 && rate[j - 1] == BIN_UNREADABLE || j + 1 < nb && rate[j + 1] == BIN_UNREADABLE)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "  bin {0}-{1}: at the edge of an unreadable region, interpolating",
                                      pt[j],
                                      pt[j + 1]);

                    rate[j]   = BIN_UNREADABLE;
                    status[j] = DpmEntryStatus.Unreadable;

                    continue;
                }

                double prediction = NeighbourRate(pt, rate, nb, j);
                double old        = rate[j];
                _at       = uint.MaxValue;
                rate[j]   = MeasureBin(pt[j], pt[j + 1], prediction > 0 ? prediction : _apsCal);
                status[j] = StatusOf(rate[j], DpmEntryStatus.Remeasured);

                AaruLogging.Debug(MODULE_NAME,
                                  "  bin {0}-{1}: {2:F4} -> {3:F4} deg/sector",
                                  pt[j],
                                  pt[j + 1],
                                  old,
                                  rate[j]);
            }
        }

        if(_aborted) return null;

        FlagOutliers(pt, rate, nb, flag);
        int nBad = 0, nUnread = 0;

        for(int j = 0; j < nb; j++)
            if(rate[j] == BIN_UNREADABLE)
                nUnread++;

        for(int j = 0; j < nb; j++)
        {
            if(!flag[j] || rate[j] == BIN_BREAK || rate[j] == BIN_UNREADABLE) continue;

            AaruLogging.Debug(MODULE_NAME,
                              "  bin {0}-{1}: still failed/outlying ({2:F4}), interpolating",
                              pt[j],
                              pt[j + 1],
                              rate[j]);

            rate[j]   = BIN_FAILED;
            status[j] = DpmEntryStatus.Interpolated;
            nBad++;
        }

        if(nBad          > 0) AaruLogging.Debug(MODULE_NAME, "({0} bins interpolated)",                     nBad);
        if(nUnread       > 0) AaruLogging.Debug(MODULE_NAME, "({0} unreadable bins interpolated)",          nUnread);
        if(_cacheHits    > 0) AaruLogging.Debug(MODULE_NAME, "({0} cache hits detected and deflected)",    _cacheHits);
        AaruLogging.Debug(MODULE_NAME, "({0} calibrations in total)", _calibrationCount + 1);

        if(_bad.Count > 0)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "({0} unreadable {1} skipped)",
                              _bad.Count,
                              _unit > 1 ? "blocks" : "sectors");
        }

        // Failed bins: linear interpolation between valid same layer neighbours; break or one block bins: their
        // preceding neighbour.
        for(int j = 0; j < nb; j++)
        {
            if(rate[j] >= 0) continue;

            int layer = LayerOf(pt[j]);
            int lo    = j - 1;
            int hi    = j + 1;

            while(lo >= 0 && (rate[lo] < 0 || LayerOf(pt[lo]) != layer)) lo--;

            while(hi < nb && (rate[hi] < 0 || LayerOf(pt[hi]) != layer)) hi++;

            if(rate[j] == BIN_BREAK && j > 0 && rate[j - 1] >= 0)
                rate[j] = rate[j - 1];
            else if(lo >= 0 && hi < nb)
            {
                double f = (double)(pt[j] - pt[lo]) / (pt[hi] - pt[lo]);
                rate[j] = rate[lo] + (rate[hi] - rate[lo]) * f;
            }
            else if(lo >= 0)
                rate[j] = rate[lo];
            else if(hi < nb)
                rate[j] = rate[hi];
            else
            {
                AaruLogging.Debug(MODULE_NAME, "all bins in a layer failed");

                return null;
            }

            if(status[j] == DpmEntryStatus.Measured) status[j] = DpmEntryStatus.Interpolated;
        }

        return BuildDpm(pt, rate, status);
    }

    /// <summary>Status of a bin from its measured rate</summary>
    static DpmEntryStatus StatusOf(double rate, DpmEntryStatus measured) => rate switch
                                                                            {
                                                                                >= 0           => measured,
                                                                                BIN_UNREADABLE => DpmEntryStatus.Unreadable,
                                                                                BIN_BREAK      => DpmEntryStatus.LayerBreak,
                                                                                _              => DpmEntryStatus.Interpolated
                                                                            };

    /// <summary>Builds the DPM from the control points and the density of each bin</summary>
    DataPositionMeasurement BuildDpm(uint[] pt, double[] rate, DpmEntryStatus[] status)
    {
        var    entries    = new DpmEntry[pt.Length];
        double cumulative = 0;

        entries[0] = new DpmEntry
        {
            Lba = pt[0]
        };

        // The angle is accumulated in full precision and rounded once per entry, so rounding never accumulates
        for(int j = 0; j < rate.Length; j++)
        {
            cumulative += rate[j] * (pt[j + 1] - pt[j]);

            entries[j + 1] = new DpmEntry
            {
                Lba    = pt[j + 1],
                Angle  = (ulong)Math.Round(cumulative * CommonTypes.Structs.Dpm.UNITS_PER_TURN / 360.0),
                Status = status[j]
            };
        }

        var layerEnds = new ulong[_layerEnds.Length];

        for(int i = 0; i < _layerEnds.Length; i++) layerEnds[i] = _layerEnds[i];

        return new DataPositionMeasurement
        {
            NominalSpacing    = _window,
            TimingUnit        = (ushort)_unit,
            Speed             = _speed,
            OppositeTrackPath = _otp,
            LayerEnds         = layerEnds,
            Entries           = entries,
            Calibrations      = _calibrations.ToArray()
        };
    }
}