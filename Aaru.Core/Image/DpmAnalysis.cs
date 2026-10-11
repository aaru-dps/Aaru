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
}

/// <summary>Analyses a Data Position Measurement for display</summary>
public static class DpmAnalysis
{
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