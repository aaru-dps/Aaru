// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Media.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Medium parameters and spiral geometry for Data Position Measurement.
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

public sealed partial class DpmMeasurement
{
    /// <summary>Speed in kB/s of 1x CD</summary>
    const ushort CD_1X  = 176;
    /// <summary>Speed in kB/s of 1x DVD</summary>
    const ushort DVD_1X = 1385;
    /// <summary>Speed in kB/s of 1x BD</summary>
    const ushort BD_1X  = 4496;

    /// <summary>Whether the medium is not a CD, so it is timed in blocks</summary>
    bool          _blocked;
    /// <summary>Nominal channel bit length, m</summary>
    double        _channelBit;
    /// <summary>Requested SET CD SPEED rate</summary>
    ushort        _kbps;
    /// <summary>Medium kind</summary>
    DpmMediumKind _kind;
    /// <summary>Last sector of the medium</summary>
    uint          _lastLba;
    /// <summary>Last sector of every layer but the last</summary>
    uint[]        _layerEnds = [];
    /// <summary>Whether the layers use opposite track path</summary>
    bool          _otp;
    /// <summary>Timing steps per control point in local sweeps</summary>
    int           _pointSteps;
    /// <summary>Sectors between rotation period re-checks</summary>
    uint          _recalEvery;
    /// <summary>Paired samples per step, the median is used</summary>
    int           _reps;
    /// <summary>Sectors per timing step: 1 on CD, an ECC block on DVD, a cluster on BD</summary>
    uint          _unit;
    /// <summary>Control point spacing, sectors</summary>
    uint          _window;

    /// <summary>Sets the measurement parameters for the medium</summary>
    /// <remarks>
    ///     CD sectors are timed one at a time. A DVD drive can only deliver a sector by reading and correcting its
    ///     whole 16 sectors ECC block, so command completion is quantized to block boundaries, and each step advances
    ///     the disc 0.25-0.6 of a rotation, so unlike on CD most steps wrap. Blu-ray is read in 32 sectors clusters.
    /// </remarks>
    void SetMedium(DpmMedium medium)
    {
        _kind       = medium.Kind;
        _lastLba    = medium.LastLba;
        _layerEnds  = medium.LayerEnds ?? [];
        _otp        = medium.OppositeTrackPath;
        _channelBit = medium.ChannelBit;

        switch(medium.Kind)
        {
            case DpmMediumKind.Dvd:
                _blocked    = true;
                _unit       = 16;
                _window     = 256;
                _recalEvery = 160000;

                // Clean enough at DVD speeds, the step median and the outlier pass catch the rest
                _reps       = 1;
                _pointSteps = 8;

                break;
            case DpmMediumKind.Bd:
                _blocked    = true;
                _unit       = 32;
                _window     = 512;
                _recalEvery = 320000;
                _reps       = 1;
                _pointSteps = 8;

                break;
            default:
                _blocked    = false;
                _unit       = 1;
                _window     = 50;
                _recalEvery = 100000;
                _reps       = 3;
                _pointSteps = 12;

                break;
        }
    }

    /// <summary>Converts a speed multiple to the SET CD SPEED rate</summary>
    ushort SpeedToKbps(ushort speed)
    {
        if(speed is 0 or 0xFFFF) return 0xFFFF;

        int kbps = speed *
                   _kind switch
                   {
                       DpmMediumKind.Dvd => DVD_1X,
                       DpmMediumKind.Bd  => BD_1X,
                       _                 => CD_1X
                   };

        return (ushort)Math.Min(kbps, 0xFFFE);
    }

    /// <summary>Gets the layer a sector is in</summary>
    int LayerOf(uint lba)
    {
        int layer = 0;

        while(layer < _layerEnds.Length && lba > _layerEnds[layer]) layer++;

        return layer;
    }

    /// <summary>Gets the first and last sector of the layer a sector is in</summary>
    void LayerBounds(uint lba, out uint lo, out uint hi)
    {
        int layer = LayerOf(lba);

        lo = layer == 0 ? 0 : _layerEnds[layer - 1] + 1;
        hi = layer < _layerEnds.Length ? _layerEnds[layer] : _lastLba;
    }

    /// <summary>
    ///     Position along the spiral from the start of the data area, in sectors: the address a single layer disc would
    ///     have at this radius. On an opposite track path disc every odd layer spirals back inward, starting at the radius
    ///     where the layer before it ended; on a parallel one every layer starts over at the inside.
    /// </summary>
    double RadialIndex(uint lba)
    {
        int layer = LayerOf(lba);

        if(layer == 0) return lba;

        uint   layerStart = _layerEnds[layer - 1] + 1;
        double x          = (double)lba           - layerStart;

        if(_otp && layer % 2 == 1)
        {
            // Spirals back from the radius where the previous layer, which went outwards, ended
            uint previousStart = layer == 1 ? 0 : _layerEnds[layer - 2] + 1;
            x = (double)_layerEnds[layer - 1] - previousStart - x;
        }

        return x < 0 ? 0 : x;
    }

    /// <summary>
    ///     Start of a sweep of <paramref name="span" /> sectors covering <paramref name="lba" />, centred on it if
    ///     <paramref name="center" />, else starting there, kept inside its layer, as a sweep across a layer break would
    ///     time a layer jump and not the spiral, and aligned to a timing unit.
    /// </summary>
    uint PlaceSweep(uint lba, uint span, bool center)
    {
        LayerBounds(lba, out uint lo, out uint hi);

        long s = lba - (center ? span / 2 : 0L);

        if(s + span > hi + 1L) s = hi + 1L - span;

        if(s < lo) s = lo;

        s -= s % _unit;

        if(s < lo) s += _unit;

        return (uint)s;
    }

    /// <summary>Rounds a sector down to its timing unit</summary>
    uint Block(uint lba) => lba - lba % _unit;

    /// <summary>
    ///     Loose physical bounds on degrees per sector at <paramref name="lba" />. A sector of length L at radius r spans
    ///     360 * L / (2 pi r) degrees, and r^2 = r0^2 + x * L * p / pi for x sectors along the spiral.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             CD (Red Book): data starts at radius 24-25.5 mm, scanning velocity 0.95-1.45 m/s, track pitch
    ///             1.3-1.75 um.
    ///         </item>
    ///         <item>
    ///             DVD (ECMA-267): data starts at radius 23.5-24.5 mm, sector = 38688 channel bits of the disc's own
    ///             nominal length +-5%, track pitch 0.70-0.78 um.
    ///         </item>
    ///         <item>
    ///             BD: data starts at radius 23.5-24.5 mm, a 2048 bytes sector is 1/32 of a 498 frames cluster of 1932
    ///             channel bits, of 55.9 to 80 nm, track pitch 0.30-0.35 um.
    ///         </item>
    ///     </list>
    ///     These catch gross failures, e.g. an integer rotation error; sweep repeatability is the fine grained check.
    /// </remarks>
    void ApsBounds(uint lba, out double lo, out double hi)
    {
        double       r0Lo, r0Hi, lLo, lHi, pLo, pHi;
        const double rMax = 58.5e-3;

        switch(_kind)
        {
            case DpmMediumKind.Dvd:
                r0Lo = 23.5e-3;
                r0Hi = 24.5e-3;
                lLo  = 38688.0 * _channelBit * 0.95;
                lHi  = 38688.0 * _channelBit * 1.05;
                pLo  = 0.70e-6;
                pHi  = 0.78e-6;

                break;
            case DpmMediumKind.Bd:
                r0Lo = 23.5e-3;
                r0Hi = 24.5e-3;
                lLo  = 498.0 * 1932.0 / 32.0 * 55.9e-9 * 0.95;
                lHi  = 498.0 * 1932.0 / 32.0 * 80.0e-9 * 1.05;
                pLo  = 0.30e-6;
                pHi  = 0.35e-6;

                break;
            default:
                r0Lo = 24.0e-3;
                r0Hi = 25.5e-3;
                lLo  = 0.95 / 75.0;
                lHi  = 1.45 / 75.0;
                pLo  = 1.3e-6;
                pHi  = 1.75e-6;

                break;
        }

        double x     = RadialIndex(lba);
        double rFar  = Math.Sqrt(r0Hi * r0Hi + x * lHi * pHi / Math.PI);
        double rNear = Math.Sqrt(r0Lo * r0Lo + x * lLo * pLo / Math.PI);

        if(rFar  > rMax) rFar  = rMax;
        if(rNear > rMax) rNear = rMax;

        lo = 360.0 * lLo / (2.0 * Math.PI * rFar);
        hi = 360.0 * lHi / (2.0 * Math.PI * rNear);
    }

    /// <summary>Logs the medium</summary>
    void LogMedium(DpmMedium medium)
    {
        AaruLogging.Debug(MODULE_NAME,
                          "Medium: {0} (profile 0x{1:X4}), {2} layers{3}, last LBA {4}",
                          medium.Kind,
                          medium.Profile < 0 ? 0 : medium.Profile,
                          _layerEnds.Length + 1,
                          _layerEnds.Length > 0 ? _otp ? ", opposite track path" : ", parallel track path" : "",
                          _lastLba);

        for(int i = 0; i < _layerEnds.Length; i++)
            AaruLogging.Debug(MODULE_NAME, "Layer {0} ends at LBA {1}", i, _layerEnds[i]);
    }
}