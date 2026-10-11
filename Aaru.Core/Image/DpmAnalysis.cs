// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmAnalysis.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Analyses a Data Position Measurement for display.
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

/// <summary>A DPM bin, the range of sectors between two consecutive entries</summary>
public struct DpmBin
{
    /// <summary>Sector where the bin starts</summary>
    public ulong          FirstLba;
    /// <summary>Sector where the bin ends, the start of the next one</summary>
    public ulong          EndLba;
    /// <summary>Layer the bin starts in</summary>
    public int            Layer;
    /// <summary>Status of the bin</summary>
    public DpmEntryStatus Status;
    /// <summary>Degrees per sector</summary>
    public double         Density;
    /// <summary>Sectors per turn, the inverse of the density</summary>
    public double         SectorsPerTurn;
    /// <summary>Turns from the first entry to the start of the bin</summary>
    public double         Turn;
    /// <summary>Whether the bin was measured, rather than interpolated or copied</summary>
    public bool           Measured;
    /// <summary>Degrees per sector of the spiral fitted to the layer</summary>
    public double         Fit;
    /// <summary>Relative deviation of the density from the fitted spiral, NaN if the bin was not measured</summary>
    public double         Deviation;
}

/// <summary>Summary of a layer of a DPM</summary>
public struct DpmLayerSummary
{
    /// <summary>Layer number, starting at 0</summary>
    public int    Layer;
    /// <summary>First sector of the layer in the DPM</summary>
    public ulong  FirstLba;
    /// <summary>Last sector of the layer in the DPM</summary>
    public ulong  LastLba;
    /// <summary>Bins in the layer</summary>
    public int    Bins;
    /// <summary>Measured bins in the layer</summary>
    public int    MeasuredBins;
    /// <summary>Turns spanned by the layer</summary>
    public double Turns;
    /// <summary>Lowest density of a measured bin, in degrees per sector</summary>
    public double MinimumDensity;
    /// <summary>Highest density of a measured bin, in degrees per sector</summary>
    public double MaximumDensity;
    /// <summary>Mean density of the measured bins, in degrees per sector</summary>
    public double MeanDensity;
    /// <summary>Median density of the measured bins, in degrees per sector</summary>
    public double MedianDensity;
    /// <summary>Sectors per turn of the first measured bin</summary>
    public double FirstSectorsPerTurn;
    /// <summary>Sectors per turn of the last measured bin</summary>
    public double LastSectorsPerTurn;
    /// <summary>Whether a spiral could be fitted to the layer</summary>
    public bool   Fitted;
    /// <summary>Intercept of the fitted spiral, the inverse of the squared density at sector 0</summary>
    public double FitA;
    /// <summary>Slope of the fitted spiral, the change of the inverse of the squared density per sector</summary>
    public double FitB;
    /// <summary>Robust standard deviation of the relative deviation of the measured bins from the spiral</summary>
    public double DeviationSigma;
    /// <summary>Relative deviation of largest magnitude of a measured bin from the fitted spiral</summary>
    public double MaximumDeviation;
    /// <summary>Relative deviation over which a bin is part of a band</summary>
    public double BandThreshold;
}

/// <summary>A rotation calibration of a DPM</summary>
public struct DpmCalibrationPoint
{
    /// <summary>Sector where the calibration was done</summary>
    public ulong  Lba;
    /// <summary>Layer of the sector</summary>
    public int    Layer;
    /// <summary>Revolutions per minute</summary>
    public double Rpm;
    /// <summary>Rotation period in milliseconds</summary>
    public double RotationPeriod;
    /// <summary>Sectors per turn</summary>
    public double SectorsPerTurn;
}

/// <summary>A run of bins whose density deviates from the fitted spiral beyond the measurement noise</summary>
public struct DpmBand
{
    /// <summary>First sector of the band</summary>
    public ulong  FirstLba;
    /// <summary>Last sector of the band</summary>
    public ulong  LastLba;
    /// <summary>Layer of the band</summary>
    public int    Layer;
    /// <summary>Bins over the threshold in the band</summary>
    public int    Bins;
    /// <summary>Mean relative deviation of the bins over the threshold</summary>
    public double MeanDeviation;
    /// <summary>Relative deviation of largest magnitude in the band</summary>
    public double MaximumDeviation;
}

/// <summary>Analysis of a DPM</summary>
public struct DpmAnalysisResult
{
    /// <summary>Sectors between entries</summary>
    public uint                  NominalSpacing;
    /// <summary>Timing unit, 1 for CD, 16 for DVD, 32 for BD, 0 unknown</summary>
    public ushort                TimingUnit;
    /// <summary>Speed the DPM was measured at, multiple of the base speed, 0xFFFF maximum, 0 unknown</summary>
    public ushort                Speed;
    /// <summary>Whether the layers have opposite track paths</summary>
    public bool                  OppositeTrackPath;
    /// <summary>Entries in the DPM</summary>
    public int                   Entries;
    /// <summary>First sector of the DPM</summary>
    public ulong                 FirstLba;
    /// <summary>Last sector of the DPM</summary>
    public ulong                 LastLba;
    /// <summary>Turns from the first to the last entry</summary>
    public double                TotalTurns;
    /// <summary>Last sector of every layer but the last</summary>
    public ulong[]               LayerEnds;
    /// <summary>Bins, indexed by the entry they start at</summary>
    public DpmBin[]              Bins;
    /// <summary>Bins with every status, indexed by <see cref="DpmEntryStatus" /></summary>
    public int[]                 StatusCounts;
    /// <summary>Summary of every layer</summary>
    public DpmLayerSummary[]     Layers;
    /// <summary>Rotation calibrations</summary>
    public DpmCalibrationPoint[] Calibrations;
    /// <summary>Regions that deviate from the fitted spiral, usually density bands of copy protections</summary>
    public DpmBand[]             Bands;
}

/// <summary>Analyses a Data Position Measurement for display</summary>
public static class DpmAnalysis
{
    /// <summary>Robust sigmas of deviation over which a bin is left out of the second fit</summary>
    const double FIT_SIGMAS   = 3.0;
    /// <summary>Minimum measured bins to fit a spiral to a layer</summary>
    const int    MIN_FIT_BINS = 3;
    /// <summary>Robust sigmas of deviation over which a bin is part of a band</summary>
    const double BAND_SIGMAS  = 5.0;
    /// <summary>Minimum relative deviation for a bin to be part of a band</summary>
    const double MIN_BAND     = 0.005;
    /// <summary>Minimum bins over the threshold in a band</summary>
    const int    MIN_RUN      = 3;
    /// <summary>Bins under the threshold allowed between bins of the same band</summary>
    const int    MAX_GAP      = 1;

    /// <summary>Analyses a DPM</summary>
    /// <param name="dpm">DPM</param>
    /// <returns>The analysis, or <c>null</c> if the DPM is not valid</returns>
    public static DpmAnalysisResult? Analyse(DataPositionMeasurement dpm)
    {
        if(!Dpm.Validate(dpm)) return null;

        DpmEntry[] entries = dpm.Entries;

        var result = new DpmAnalysisResult
        {
            NominalSpacing    = dpm.NominalSpacing,
            TimingUnit        = dpm.TimingUnit,
            Speed             = dpm.Speed,
            OppositeTrackPath = dpm.OppositeTrackPath,
            Entries           = entries.Length,
            FirstLba          = entries[0].Lba,
            LastLba           = entries[^1].Lba,
            TotalTurns        = (double)entries[^1].Angle / Dpm.UNITS_PER_TURN,
            LayerEnds         = dpm.LayerEnds ?? [],
            Bins              = new DpmBin[entries.Length - 1],
            StatusCounts      = new int[Enum.GetValues<DpmEntryStatus>().Length]
        };

        for(int i = 0; i < result.Bins.Length; i++)
        {
            DpmEntryStatus status = entries[i + 1].Status;

            double density = (entries[i + 1].Angle - entries[i].Angle) *
                             360.0              /
                             Dpm.UNITS_PER_TURN /
                             (entries[i + 1].Lba - entries[i].Lba);

            result.Bins[i] = new DpmBin
            {
                FirstLba       = entries[i].Lba,
                EndLba         = entries[i + 1].Lba,
                Layer          = Dpm.LayerOf(dpm, entries[i].Lba),
                Status         = status,
                Density        = density,
                SectorsPerTurn = density > 0 ? 360.0 / density : 0,
                Turn           = (double)entries[i].Angle / Dpm.UNITS_PER_TURN,
                Measured = density > 0 &&
                           status is not (DpmEntryStatus.Interpolated
                                       or DpmEntryStatus.Unreadable
                                       or DpmEntryStatus.LayerBreak)
            };

            if((int)status < result.StatusCounts.Length) result.StatusCounts[(int)status]++;
        }

        result.Layers = Layers(result.Bins);

        for(int l = 0, first = 0; l < result.Layers.Length; first += result.Layers[l].Bins, l++)
            Fit(result.Bins, first, ref result.Layers[l]);

        result.Bands = FindBands(result.Bins, result.Layers);

        result.Calibrations = (dpm.Calibrations ?? []).Where(static c => c.RotationPeriod > 0)
                                                      .Select(c => new DpmCalibrationPoint
                                                       {
                                                           Lba            = c.Lba,
                                                           Layer          = Dpm.LayerOf(dpm, c.Lba),
                                                           Rpm            = 60e9             / c.RotationPeriod,
                                                           RotationPeriod = c.RotationPeriod / 1e6,
                                                           SectorsPerTurn = c.SectorsPerTurn / 1000.0
                                                       })
                                                      .ToArray();

        return result;
    }

    /// <summary>Fits a spiral to the measured bins of a layer and gets the deviation of every bin from it</summary>
    /// <remarks>
    ///     At constant linear density and track pitch the radius at sector s is sqrt(r0² + s·L·p/π), and the density
    ///     is inversely proportional to the radius, so the inverse of the squared density is linear in the sector. The
    ///     line is fitted by least squares and fitted again without the bins over <see cref="FIT_SIGMAS" /> robust
    ///     sigmas, so density bands do not bend it. It is the same on layers spiralling inward, with a negative slope.
    ///     The model is smooth over the whole layer, so the bin to bin changes of a band are never flattened.
    /// </remarks>
    static void Fit(DpmBin[] bins, int first, ref DpmLayerSummary layer)
    {
        int end = first + layer.Bins;

        for(int i = first; i < end; i++)
        {
            bins[i].Fit       = double.NaN;
            bins[i].Deviation = double.NaN;
        }

        bool[] use = new bool[layer.Bins];

        for(int i = 0; i < use.Length; i++) use[i] = bins[first + i].Measured;

        for(int pass = 0; pass < 2; pass++)
        {
            if(!Line(bins, first, use, out double a, out double b)) return;

            layer.Fitted = true;
            layer.FitA   = a;
            layer.FitB   = b;

            for(int i = first; i < end; i++)
            {
                double inverse = a + b * Middle(bins[i]);

                bins[i].Fit       = inverse > 0 ? 1.0 / Math.Sqrt(inverse) : double.NaN;
                bins[i].Deviation = bins[i].Measured ? bins[i].Density / bins[i].Fit - 1.0 : double.NaN;
            }

            double[] deviations = bins[first..end].Where(static d => !double.IsNaN(d.Deviation))
                                                  .Select(static d => Math.Abs(d.Deviation))
                                                  .ToArray();

            layer.DeviationSigma = deviations.Length > 0
                                       ? 1.4826 * DpmMeasurement.Median(deviations, deviations.Length)
                                       : 0;

            layer.MaximumDeviation = bins[first..end].Where(static d => !double.IsNaN(d.Deviation))
                                                     .Select(static d => d.Deviation)
                                                     .DefaultIfEmpty()
                                                     .MaxBy(Math.Abs);

            layer.BandThreshold = Math.Max(BAND_SIGMAS * layer.DeviationSigma, MIN_BAND);

            if(pass == 1 || layer.DeviationSigma <= 0) break;

            for(int i = 0; i < use.Length; i++)
            {
                use[i] = bins[first + i].Measured &&
                         Math.Abs(bins[first + i].Deviation) <= FIT_SIGMAS * layer.DeviationSigma;
            }
        }
    }

    /// <summary>Least squares line of the inverse of the squared density on the middle sector of used bins</summary>
    static bool Line(DpmBin[] bins, int first, bool[] use, out double a, out double b)
    {
        a = b = 0;

        int    n  = 0;
        double sx = 0, sy = 0, sxx = 0, sxy = 0;

        // Sectors relative to the first bin of the layer, for precision
        double origin = bins[first].FirstLba;

        for(int i = 0; i < use.Length; i++)
        {
            if(!use[i]) continue;

            double x = Middle(bins[first + i]) - origin;
            double y = 1.0 / (bins[first + i].Density * bins[first + i].Density);

            n++;
            sx  += x;
            sy  += y;
            sxx += x * x;
            sxy += x * y;
        }

        if(n < MIN_FIT_BINS) return false;

        double denominator = n * sxx - sx * sx;

        if(denominator <= 0) return false;

        b = (n * sxy - sx * sy) / denominator;
        a = (sy - b * sx) / n - b * origin;

        return true;
    }

    /// <summary>Groups the measured bins that deviate from the spiral of their layer into bands</summary>
    /// <remarks>
    ///     The sign of the deviation is not considered, as some protections alternate denser and sparser bins. Bins not
    ///     measured neither extend nor break a band.
    /// </remarks>
    static DpmBand[] FindBands(DpmBin[] bins, DpmLayerSummary[] layers)
    {
        List<DpmBand> bands = [];
        List<int>     run   = [];
        int           gap   = 0;

        void Close()
        {
            if(run.Count >= MIN_RUN)
            {
                bands.Add(new DpmBand
                {
                    FirstLba         = bins[run[0]].FirstLba,
                    LastLba          = bins[run[^1]].EndLba - 1,
                    Layer            = bins[run[0]].Layer,
                    Bins             = run.Count,
                    MeanDeviation    = run.Average(r => bins[r].Deviation),
                    MaximumDeviation = run.Select(r => bins[r].Deviation).MaxBy(Math.Abs)
                });
            }

            run.Clear();
            gap = 0;
        }

        for(int first = 0, l = 0; l < layers.Length; first += layers[l].Bins, l++)
        {
            Close();

            if(!layers[l].Fitted) continue;

            double threshold = layers[l].BandThreshold;

            for(int i = first; i < first + layers[l].Bins; i++)
            {
                if(double.IsNaN(bins[i].Deviation)) continue;

                if(Math.Abs(bins[i].Deviation) > threshold)
                {
                    run.Add(i);
                    gap = 0;

                    continue;
                }

                if(run.Count == 0) continue;

                gap++;

                if(gap > MAX_GAP) Close();
            }
        }

        Close();

        return bands.ToArray();
    }

    /// <summary>Middle sector of a bin</summary>
    static double Middle(DpmBin bin)
    {
        return (bin.FirstLba + bin.EndLba) / 2.0;
    }

    /// <summary>Summarizes every layer of the bins</summary>
    static DpmLayerSummary[] Layers(DpmBin[] bins)
    {
        List<DpmLayerSummary> layers = [];
        int                   start  = 0;

        while(start < bins.Length)
        {
            int end = start;

            while(end < bins.Length && bins[end].Layer == bins[start].Layer) end++;

            var layer = new DpmLayerSummary
            {
                Layer    = bins[start].Layer,
                FirstLba = bins[start].FirstLba,
                LastLba  = bins[end - 1].EndLba,
                Bins     = end                                                   - start,
                Turns    = (end < bins.Length ? bins[end].Turn : LastTurn(bins)) - bins[start].Turn
            };

            double[] densities = bins[start..end].Where(static b => b.Measured).Select(static b => b.Density).ToArray();

            layer.MeasuredBins = densities.Length;

            if(densities.Length > 0)
            {
                DpmBin first = bins[start..end].First(static b => b.Measured);
                DpmBin last  = bins[start..end].Last(static b => b.Measured);

                layer.MinimumDensity      = densities.Min();
                layer.MaximumDensity      = densities.Max();
                layer.MeanDensity         = densities.Average();
                layer.FirstSectorsPerTurn = first.SectorsPerTurn;
                layer.LastSectorsPerTurn  = last.SectorsPerTurn;
                layer.MedianDensity       = DpmMeasurement.Median(densities, densities.Length);
            }

            layers.Add(layer);
            start = end;
        }

        return layers.ToArray();
    }

    /// <summary>Turns from the first entry to the end of the last bin</summary>
    static double LastTurn(DpmBin[] bins)
    {
        DpmBin last = bins[^1];

        return last.Turn + (last.EndLba - last.FirstLba) * last.Density / 360.0;
    }
}