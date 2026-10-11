// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmComparison.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Compares two Data Position Measurements.
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
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Devices.Dpm;

namespace Aaru.Core.Image;

/// <summary>Result of comparing two DPMs</summary>
public enum DpmComparisonVerdict : byte
{
    /// <summary>Both DPMs agree within their measurement noise</summary>
    Match,
    /// <summary>At least one region differs beyond the measurement noise</summary>
    Differ,
    /// <summary>The DPMs do not overlap, are of different media, or have too few measured bins in common</summary>
    NotComparable
}

/// <summary>A run of bins where the density of two DPMs differs beyond the measurement noise</summary>
public struct DpmDifference
{
    /// <summary>First sector of the region</summary>
    public ulong  FirstLba;
    /// <summary>Last sector of the region</summary>
    public ulong  LastLba;
    /// <summary>Mean relative density deviation, second against first, of the bins over the threshold</summary>
    public double MeanDeviation;
    /// <summary>Relative density deviation of largest magnitude in the region</summary>
    public double MaximumDeviation;
}

/// <summary>A bin of the grid both DPMs are resampled onto</summary>
public struct DpmComparisonBin
{
    /// <summary>Sector where the bin starts</summary>
    public ulong  FirstLba;
    /// <summary>Sector where the bin ends, the start of the next one</summary>
    public ulong  EndLba;
    /// <summary>Segment, between layer boundaries of either DPM, the bin is in</summary>
    public int    Segment;
    /// <summary>Degrees per sector in the first DPM</summary>
    public double Density1;
    /// <summary>Degrees per sector in the second DPM</summary>
    public double Density2;
    /// <summary>Relative deviation of the second DPM against the first, once the calibration scale is removed</summary>
    public double Deviation;
    /// <summary>Whether both DPMs measured the bin, otherwise it is not compared</summary>
    public bool   Valid;
}

/// <summary>Comparison of two DPMs</summary>
public struct DpmComparisonResult
{
    /// <summary>Verdict</summary>
    public DpmComparisonVerdict Verdict;
    /// <summary>Whether the timing units are known and differ</summary>
    public bool                 TimingUnitDiffers;
    /// <summary>Whether the track paths differ</summary>
    public bool                 TrackPathDiffers;
    /// <summary>Whether the layer boundaries differ</summary>
    public bool                 LayersDiffer;
    /// <summary>Whether the requested speeds are known and differ</summary>
    public bool                 SpeedDiffers;
    /// <summary>Whether the nominal spacings differ</summary>
    public bool                 SpacingDiffers;
    /// <summary>Sectors between grid points</summary>
    public uint                 Spacing;
    /// <summary>First sector compared</summary>
    public ulong                FirstLba;
    /// <summary>Last sector compared</summary>
    public ulong                LastLba;
    /// <summary>Bins compared</summary>
    public int                  Bins;
    /// <summary>Bins not compared, because either DPM did not measure them</summary>
    public int                  ExcludedBins;
    /// <summary>Single bins over the threshold, considered misreads and not reported as differences</summary>
    public int                  IsolatedOutliers;
    /// <summary>
    ///     Ratio between the densities of the second and first DPMs, for every segment between layer boundaries. It is
    ///     the calibration error of the drives, not a property of the disc, and is removed before comparing. NaN for
    ///     segments without measured bins.
    /// </summary>
    public double[]             Scales;
    /// <summary>Robust standard deviation of the bin to bin density noise of the first DPM, relative</summary>
    public double               Noise1;
    /// <summary>Robust standard deviation of the bin to bin density noise of the second DPM, relative</summary>
    public double               Noise2;
    /// <summary>Relative deviation over which a bin differs</summary>
    public double               Threshold;
    /// <summary>Median of the absolute relative deviations</summary>
    public double               MedianDeviation;
    /// <summary>95th percentile of the absolute relative deviations</summary>
    public double               Percentile95Deviation;
    /// <summary>Largest absolute relative deviation</summary>
    public double               MaximumDeviation;
    /// <summary>Regions that differ</summary>
    public DpmDifference[]      Differences;
    /// <summary>The grid both DPMs were resampled onto</summary>
    public DpmComparisonBin[]   Grid;
}

/// <summary>Compares two Data Position Measurements</summary>
/// <remarks>
///     DPMs are analogue measurements and are never identical. Both are resampled onto a common grid, inside every
///     layer, and compared by density, in degrees per sector, as the cumulative angle accumulates any calibration error.
///     The ratio between both densities, the calibration error of the drives, is removed. A bin differs when it deviates
///     more than <see cref="NOISE_SIGMAS" /> times the combined noise of both DPMs, or <see cref="MIN_DEVIATION" />,
///     whichever is larger. Copy protection density changes span many bins, so only runs of at least
///     <see cref="MIN_RUN" /> differing bins are reported, while single bins are misreads.
/// </remarks>
public static class DpmComparison
{
    /// <summary>Neighbours each side for the local median of the noise estimate</summary>
    const int    NOISE_HALF_WIN = 4;
    /// <summary>Robust sigmas of the combined noise over which a bin differs</summary>
    const double NOISE_SIGMAS   = 5.0;
    /// <summary>Minimum relative deviation for a bin to differ</summary>
    const double MIN_DEVIATION  = 0.005;
    /// <summary>Minimum differing bins in a region</summary>
    const int    MIN_RUN        = 3;
    /// <summary>Bins under the threshold allowed between differing bins of the same region</summary>
    const int    MAX_GAP        = 1;
    /// <summary>Minimum compared bins for the comparison to be meaningful</summary>
    const int    MIN_BINS       = 2 * NOISE_HALF_WIN + 1;

    /// <summary>Compares two DPMs</summary>
    /// <param name="first">First DPM</param>
    /// <param name="second">Second DPM</param>
    /// <returns>The comparison</returns>
    public static DpmComparisonResult Compare(DataPositionMeasurement first, DataPositionMeasurement second)
    {
        var result = new DpmComparisonResult
        {
            TimingUnitDiffers =
                first.TimingUnit != 0 && second.TimingUnit != 0 && first.TimingUnit != second.TimingUnit,
            TrackPathDiffers = first.OppositeTrackPath != second.OppositeTrackPath,
            LayersDiffer     = !(first.LayerEnds ?? []).SequenceEqual(second.LayerEnds ?? []),
            SpeedDiffers     = first.Speed != 0 && second.Speed != 0 && first.Speed != second.Speed,
            SpacingDiffers   = first.NominalSpacing != second.NominalSpacing,
            Spacing          = Math.Max(first.NominalSpacing, second.NominalSpacing),
            Verdict          = DpmComparisonVerdict.NotComparable,
            Scales           = [],
            Differences      = [],
            Grid             = []
        };

        if(result.TimingUnitDiffers || !Dpm.Validate(first) || !Dpm.Validate(second)) return result;

        ulong start = Math.Max(first.Entries[0].Lba, second.Entries[0].Lba);
        ulong end   = Math.Min(first.Entries[^1].Lba, second.Entries[^1].Lba);

        if(start >= end) return result;

        // Grid points follow the coarser DPM, so its bins are not split
        DpmEntry[] coarse = first.NominalSpacing >= second.NominalSpacing ? first.Entries : second.Entries;

        List<DpmComparisonBin>         grid     = [];
        List<(ulong start, ulong end)> segments = Segments(first, second, start, end);

        for(int s = 0; s < segments.Count; s++)
        {
            ulong segmentStart = segments[s].start;
            ulong segmentEnd   = segments[s].end;

            // Start on a point of the coarser DPM when there is one in the segment
            int aligned = Array.FindIndex(coarse, e => e.Lba >= segmentStart && e.Lba < segmentEnd);

            if(aligned >= 0) segmentStart = coarse[aligned].Lba;

            if(segmentEnd <= segmentStart) continue;

            ulong count = Math.Max(1, (segmentEnd - segmentStart) / result.Spacing);

            for(ulong i = 0; i < count; i++)
            {
                ulong binStart = segmentStart + i * result.Spacing;
                ulong binEnd   = i == count - 1 ? segmentEnd : binStart + result.Spacing;

                var bin = new DpmComparisonBin
                {
                    FirstLba = binStart,
                    EndLba   = binEnd,
                    Segment  = s,
                    Density1 = DensityBetween(first,  binStart, binEnd),
                    Density2 = DensityBetween(second, binStart, binEnd)
                };

                bin.Valid = bin.Density1 > 0                             &&
                            bin.Density2 > 0                             &&
                            IsMeasured(first.Entries,  binStart, binEnd) &&
                            IsMeasured(second.Entries, binStart, binEnd);

                grid.Add(bin);
            }
        }

        DpmComparisonBin[] bins = grid.ToArray();
        result.Grid     = bins;
        result.FirstLba = bins.Length > 0 ? bins[0].FirstLba : start;
        result.LastLba  = bins.Length > 0 ? bins[^1].EndLba : end;
        result.Scales   = new double[segments.Count];

        // Calibration scale, for every segment
        for(int s = 0; s < segments.Count; s++)
        {
            double[] ratios = bins.Where(b => b.Valid && b.Segment == s).Select(b => b.Density2 / b.Density1).ToArray();

            result.Scales[s] = ratios.Length > 0 ? DpmMeasurement.Median(ratios, ratios.Length) : double.NaN;
        }

        for(int i = 0; i < bins.Length; i++)
        {
            if(!bins[i].Valid) continue;

            bins[i].Deviation = bins[i].Density2 / (result.Scales[bins[i].Segment] * bins[i].Density1) - 1.0;
        }

        result.Bins         = bins.Count(b => b.Valid);
        result.ExcludedBins = bins.Length - result.Bins;

        if(result.Bins < MIN_BINS) return result;

        result.Noise1 = Noise(bins, static b => b.Density1);
        result.Noise2 = Noise(bins, static b => b.Density2);

        result.Threshold =
            Math.Max(NOISE_SIGMAS * Math.Sqrt(result.Noise1 * result.Noise1 + result.Noise2 * result.Noise2),
                     MIN_DEVIATION);

        double[] deviations = bins.Where(b => b.Valid).Select(b => Math.Abs(b.Deviation)).ToArray();
        Array.Sort(deviations);
        result.MedianDeviation       = DpmMeasurement.Median(deviations, deviations.Length);
        result.Percentile95Deviation = deviations[(int)Math.Ceiling(0.95 * deviations.Length) - 1];
        result.MaximumDeviation      = deviations[^1];

        result.Differences = FindDifferences(bins, result.Threshold, out result.IsolatedOutliers);
        result.Verdict     = result.Differences.Length > 0 ? DpmComparisonVerdict.Differ : DpmComparisonVerdict.Match;

        return result;
    }

    /// <summary>
    ///     Splits the compared range at the layer boundaries of either DPM. A segment ends on the last sector of a layer
    ///     and the next one starts on the first sector of the next layer, so no bin crosses a layer boundary.
    /// </summary>
    static List<(ulong start, ulong end)> Segments(DataPositionMeasurement first, DataPositionMeasurement second,
                                                   ulong                   start, ulong                   end)
    {
        ulong[] layerEnds = (first.LayerEnds ?? []).Concat(second.LayerEnds ?? [])
                                                   .Where(l => l >= start && l < end)
                                                   .Distinct()
                                                   .Order()
                                                   .ToArray();

        List<(ulong start, ulong end)> segments = [];
        ulong                          current  = start;

        foreach(ulong layerEnd in layerEnds)
        {
            if(layerEnd > current) segments.Add((current, layerEnd));

            current = layerEnd + 1;
        }

        if(end > current) segments.Add((current, end));

        return segments;
    }

    /// <summary>Degrees per sector between two sectors</summary>
    static double DensityBetween(DataPositionMeasurement dpm, ulong start, ulong end)
    {
        ulong? first = Dpm.AngleAt(dpm, start);
        ulong? last  = Dpm.AngleAt(dpm, end);

        if(first is null || last is null || last <= first) return 0;

        return (last.Value - first.Value) * 360.0 / Dpm.UNITS_PER_TURN / (end - start);
    }

    /// <summary>Whether every bin of a DPM overlapping a range was measured</summary>
    static bool IsMeasured(DpmEntry[] entries, ulong start, ulong end)
    {
        // First entry after the start, its bin is the first one overlapping the range
        int low = 0, high = entries.Length;

        while(low < high)
        {
            int middle = low + (high - low) / 2;

            if(entries[middle].Lba <= start)
                low = middle + 1;
            else
                high = middle;
        }

        for(int i = Math.Max(low, 1); i < entries.Length && entries[i - 1].Lba < end; i++)
        {
            if(entries[i].Status is DpmEntryStatus.Interpolated
                                 or DpmEntryStatus.Unreadable
                                 or DpmEntryStatus.LayerBreak)
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Robust standard deviation of the relative deviation of every valid bin from the median of its
    ///     <see cref="NOISE_HALF_WIN" /> valid neighbours on each side, in the same segment
    /// </summary>
    static double Noise(DpmComparisonBin[] bins, Func<DpmComparisonBin, double> density)
    {
        var window = new double[2 * NOISE_HALF_WIN];
        var rel    = new double[bins.Length];
        int nr     = 0;

        for(int i = 0; i < bins.Length; i++)
        {
            if(!bins[i].Valid) continue;

            int nw = 0;
            int lo = Math.Max(0, i - NOISE_HALF_WIN);

            for(int j = lo; j <= i + NOISE_HALF_WIN && j < bins.Length; j++)
                if(j != i && bins[j].Valid && bins[j].Segment == bins[i].Segment)
                    window[nw++] = density(bins[j]);

            if(nw < NOISE_HALF_WIN) continue;

            rel[nr++] = Math.Abs(density(bins[i]) / DpmMeasurement.Median(window, nw) - 1.0);
        }

        return nr > 0 ? 1.4826 * DpmMeasurement.Median(rel, nr) : 0.0;
    }

    /// <summary>Groups the bins over the threshold into regions</summary>
    /// <param name="bins">Grid</param>
    /// <param name="threshold">Relative deviation over which a bin differs</param>
    /// <param name="isolated">Bins over the threshold in runs too short to be a region</param>
    static DpmDifference[] FindDifferences(DpmComparisonBin[] bins, double threshold, out int isolated)
    {
        List<DpmDifference> differences = [];
        List<int>           run         = [];
        int                 gap         = 0;
        int                 segment     = -1;
        int                 single      = 0;

        void Close()
        {
            if(run.Count >= MIN_RUN)
            {
                double maximum = run.Select(r => bins[r].Deviation).MaxBy(static d => Math.Abs(d));

                differences.Add(new DpmDifference
                {
                    FirstLba         = bins[run[0]].FirstLba,
                    LastLba          = bins[run[^1]].EndLba - 1,
                    MeanDeviation    = run.Average(r => bins[r].Deviation),
                    MaximumDeviation = maximum
                });
            }
            else
                single += run.Count;

            run.Clear();
            gap = 0;
        }

        for(int i = 0; i < bins.Length; i++)
        {
            // Unmeasured bins neither extend nor break a region
            if(!bins[i].Valid) continue;

            if(bins[i].Segment != segment)
            {
                Close();
                segment = bins[i].Segment;
            }

            if(Math.Abs(bins[i].Deviation) > threshold)
            {
                run.Add(i);
                gap = 0;

                continue;
            }

            if(run.Count == 0) continue;

            if(++gap > MAX_GAP) Close();
        }

        Close();
        isolated = single;

        return differences.ToArray();
    }
}