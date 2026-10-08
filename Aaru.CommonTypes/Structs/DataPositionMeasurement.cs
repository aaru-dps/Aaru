// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DataPositionMeasurement.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the Data Position Measurement (DPM) structures and helpers.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;

namespace Aaru.CommonTypes.Structs;

/// <summary>Status of a DPM bin, the range of sectors that ends at a DPM entry</summary>
public enum DpmEntryStatus : byte
{
    /// <summary>Unknown, e.g. imported from a format that does not record it, or the first entry</summary>
    Unknown      = 0,
    /// <summary>Measured on the first attempt</summary>
    Measured     = 1,
    /// <summary>Measured again because the first measurement was an outlier</summary>
    Remeasured   = 2,
    /// <summary>Measurement failed, interpolated from its neighbours</summary>
    Interpolated = 3,
    /// <summary>Sectors could not be read, interpolated from its neighbours</summary>
    Unreadable   = 4,
    /// <summary>Bin crosses a layer boundary or is too short to measure, density copied from the bin before</summary>
    LayerBreak   = 5
}

/// <summary>A DPM control point</summary>
public struct DpmEntry
{
    /// <summary>Sector address</summary>
    public ulong          Lba;
    /// <summary>
    ///     Cumulative angle, from the first entry (whose angle is 0) to this sector, in units of
    ///     <see cref="Dpm.UNITS_PER_TURN" /> per turn
    /// </summary>
    public ulong          Angle;
    /// <summary>Status of the bin that ends at this entry. Ignored for the first entry.</summary>
    public DpmEntryStatus Status;
}

/// <summary>A rotation calibration done while measuring DPM</summary>
public struct DpmCalibration
{
    /// <summary>Sector address where the calibration was done</summary>
    public ulong Lba;
    /// <summary>Rotation period, in nanoseconds</summary>
    public ulong RotationPeriod;
    /// <summary>Sectors per rotation at <see cref="Lba" />, multiplied by 1000</summary>
    public ulong SectorsPerTurn;
}

/// <summary>Data Position Measurement, the physical angle of sectors along the spiral of an optical disc</summary>
public struct DataPositionMeasurement
{
    /// <summary>Nominal spacing, in sectors, between entries</summary>
    public uint             NominalSpacing;
    /// <summary>Sectors per timing unit: 1 on CD, 16 on DVD, 32 on BD. 0 if unknown.</summary>
    public ushort           TimingUnit;
    /// <summary>Requested drive speed, as a multiple of the medium base speed. 0xFFFF is drive maximum, 0 unknown.</summary>
    public ushort           Speed;
    /// <summary>Whether the layers use opposite track path</summary>
    public bool             OppositeTrackPath;
    /// <summary>Last sector of every layer but the last, increasing. Empty for single layer media or if unknown.</summary>
    public ulong[]          LayerEnds;
    /// <summary>Control points, with strictly increasing sector addresses</summary>
    public DpmEntry[]       Entries;
    /// <summary>Rotation calibrations done while measuring, may be empty</summary>
    public DpmCalibration[] Calibrations;
}

/// <summary>Helpers to work with <see cref="DataPositionMeasurement" /></summary>
public static class Dpm
{
    /// <summary>Angle units per turn, Alcohol 120% units (256 per turn) multiplied by 10000</summary>
    public const ulong UNITS_PER_TURN         = 2560000;
    /// <summary>Angle units per Alcohol 120% unit</summary>
    public const ulong UNITS_PER_ALCOHOL_UNIT = 10000;

    /// <summary>Checks that a DPM is consistent</summary>
    /// <param name="dpm">DPM</param>
    /// <returns><c>true</c> if the DPM is consistent, <c>false</c> otherwise</returns>
    public static bool Validate(DataPositionMeasurement dpm)
    {
        if(dpm.Entries is not { Length: >= 2 } || dpm.NominalSpacing == 0 || dpm.Entries[0].Angle != 0) return false;

        for(int i = 1; i < dpm.Entries.Length; i++)
        {
            if(dpm.Entries[i].Lba <= dpm.Entries[i - 1].Lba) return false;

            if(dpm.Entries[i].Angle < dpm.Entries[i - 1].Angle) return false;
        }

        if(dpm.LayerEnds is null) return true;

        for(int i = 1; i < dpm.LayerEnds.Length; i++)
            if(dpm.LayerEnds[i] <= dpm.LayerEnds[i - 1])
                return false;

        return true;
    }

    /// <summary>Gets the layer a sector is in</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="lba">Sector address</param>
    /// <returns>Layer number, starting at 0</returns>
    public static int LayerOf(DataPositionMeasurement dpm, ulong lba)
    {
        if(dpm.LayerEnds is null) return 0;

        int layer = 0;

        while(layer < dpm.LayerEnds.Length && lba > dpm.LayerEnds[layer]) layer++;

        return layer;
    }

    /// <summary>Gets the angle at a sector, interpolating linearly inside the bin that contains it</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="lba">Sector address</param>
    /// <returns>Angle, in units of <see cref="UNITS_PER_TURN" /> per turn, or <c>null</c> if outside the DPM</returns>
    public static ulong? AngleAt(DataPositionMeasurement dpm, ulong lba)
    {
        int bin = BinOf(dpm, lba);

        if(bin < 0) return null;

        DpmEntry first = dpm.Entries[bin];

        if(lba == first.Lba) return first.Angle;

        DpmEntry last = dpm.Entries[bin + 1];

        UInt128 delta = (UInt128)(last.Angle - first.Angle) * (lba - first.Lba);
        ulong   span  = last.Lba - first.Lba;

        return first.Angle + (ulong)((delta + span / 2) / span);
    }

    /// <summary>Gets the density, in degrees per sector, of the bin that contains a sector</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="lba">Sector address</param>
    /// <returns>Degrees per sector, or <c>null</c> if outside the DPM</returns>
    public static double? DensityAt(DataPositionMeasurement dpm, ulong lba)
    {
        int bin = BinOf(dpm, lba);

        if(bin < 0) return null;

        DpmEntry first = dpm.Entries[bin];
        DpmEntry last  = dpm.Entries[bin + 1];

        return (last.Angle - first.Angle) * 360.0 / UNITS_PER_TURN / (last.Lba - first.Lba);
    }

    /// <summary>Resamples a DPM onto a uniform grid, starting at the first entry</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="spacing">Sectors between grid points</param>
    /// <returns>
    ///     Angles at the first entry plus every multiple of <paramref name="spacing" /> up to the last entry, not
    ///     including the first entry itself
    /// </returns>
    public static ulong[] ToGrid(DataPositionMeasurement dpm, uint spacing)
    {
        if(dpm.Entries is not { Length: >= 2 } || spacing == 0) return [];

        ulong start = dpm.Entries[0].Lba;
        ulong count = (dpm.Entries[^1].Lba - start) / spacing;
        var   grid  = new ulong[count];

        for(ulong i = 0; i < count; i++) grid[i] = AngleAt(dpm, start + (i + 1) * spacing) ?? 0;

        return grid;
    }

    /// <summary>Gets the bin that contains a sector</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="lba">Sector address</param>
    /// <returns>Index of the entry that starts the bin, or -1 if outside the DPM</returns>
    static int BinOf(DataPositionMeasurement dpm, ulong lba)
    {
        DpmEntry[] entries = dpm.Entries;

        if(entries is not { Length: >= 2 } || lba < entries[0].Lba || lba > entries[^1].Lba) return -1;

        int low  = 0;
        int high = entries.Length - 1;

        // Find the last entry whose address is not above the sector, keeping the last entry inside the last bin
        while(high - low > 1)
        {
            int middle = low + (high - low) / 2;

            if(entries[middle].Lba <= lba)
                low = middle;
            else
                high = middle;
        }

        return low;
    }
}