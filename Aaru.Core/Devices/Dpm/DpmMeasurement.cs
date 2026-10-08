// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmMeasurement.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Measures Data Position Measurement (DPM), the physical angle of sectors along
//     the spiral of an optical disc, per US7463565B2.
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

using System.Collections.Generic;
using System.Diagnostics;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Structs;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dpm;

/// <summary>Measures Data Position Measurement (DPM), the physical angle of sectors along the spiral of an optical disc</summary>
/// <remarks>
///     <list type="number">
///         <item>Locks the drive to a fixed speed, and lets the spindle settle.</item>
///         <item>
///             Calibrates the rotation period T and degrees per sector from a sawtooth of back to back paired reads.
///             Read S, then time the read of S+d for increasing d: the time grows by T*aps/360 per sector and drops by
///             T at each rotation wrap. A calibration is only accepted when two sweeps agree, the counted wrap spacing
///             matches the timed sectors per rotation, and the result fits the medium geometry.
///         </item>
///         <item>
///             Measures the cumulative angle at control points by chained reads: reading one point's block right after
///             the previous point's takes (k + frac(dtheta)) rotations, so one timed read gives the fractional rotation
///             between points, and the previous bins' density gives the whole rotations.
///         </item>
///     </list>
///     DVDs are timed per 16 sectors ECC block and Blu-rays per 32 sectors cluster, as the drive can't
///     deliver a sector without reading its whole block.
/// </remarks>
public sealed partial class DpmMeasurement
{
    const string MODULE_NAME = "DPM";

    readonly List<DpmCalibration> _calibrations = [];
    readonly IDpmDrive            _drive;
    bool                          _aborted;
    bool                          _prepared;
    /// <summary>Requested speed multiple, 0xFFFF for the drive maximum</summary>
    ushort _speed;

    /// <summary>Creates a DPM measurement on a drive</summary>
    /// <param name="drive">Drive</param>
    public DpmMeasurement(IDpmDrive drive) => _drive = drive;

    /// <summary>Last sector of the medium, valid after <see cref="Prepare" /></summary>
    public uint LastLba => _lastLba;

    /// <summary>Kind of the medium, valid after <see cref="Prepare" /></summary>
    public DpmMediumKind Kind => _kind;

    /// <summary>Event raised when the progress bar is not longer needed</summary>
    public event EndProgressHandler EndProgress;
    /// <summary>Event raised when a progress bar is needed</summary>
    public event InitProgressHandler InitProgress;
    /// <summary>Event raised to update the values of a determinate progress bar</summary>
    public event UpdateProgressHandler UpdateProgress;
    /// <summary>Event raised to update the status of an undeterminate progress bar</summary>
    public event UpdateStatusHandler UpdateStatus;
    /// <summary>Event raised when there is an error</summary>
    public event ErrorMessageHandler ErrorMessage;

    /// <summary>Stops the measurement as soon as possible</summary>
    public void Abort() => _aborted = true;

    /// <summary>Identifies the medium and sets the measurement parameters</summary>
    /// <param name="spacing">Sectors between DPM entries, 0 for the medium default</param>
    /// <returns><c>true</c> if the medium can be measured</returns>
    public bool Prepare(uint spacing)
    {
        if(!Stopwatch.IsHighResolution)
        {
            ErrorMessage?.Invoke(Localization.Core.DPM_needs_a_high_resolution_timer);

            return false;
        }

        if(!_drive.DetectMedium(out DpmMedium medium))
        {
            ErrorMessage?.Invoke(Localization.Core.DPM_cannot_be_measured_on_this_medium);

            return false;
        }

        SetMedium(medium);
        LogMedium(medium);

        if(spacing > 0)
        {
            // Rounded to whole timing units
            _window = (spacing + _unit / 2) / _unit * _unit;

            if(_window < _unit) _window = _unit;

            AaruLogging.Debug(MODULE_NAME, "Entry spacing: {0} sectors", _window);
        }

        _prepared = true;

        return true;
    }

    /// <summary>Measures the DPM of a range of sectors</summary>
    /// <param name="startLba">First sector</param>
    /// <param name="endLba">Last sector, clamped to the last sector of the medium</param>
    /// <param name="speed">Speed as a multiple of the medium base speed, 0 for the drive maximum</param>
    /// <returns>The DPM, or <c>null</c> if it could not be measured</returns>
    public DataPositionMeasurement? Measure(uint startLba, uint endLba, ushort speed)
    {
        if(!_prepared) return null;

        _calibrations.Clear();
        _cacheHits           = 0;
        _calibrationCount    = 0;
        _speedRecalibrations = 0;
        _unreadStreak        = 0;
        _aborted             = false;

        if(endLba > _lastLba)
        {
            AaruLogging.Debug(MODULE_NAME, "end_lba clamped to the last sector, {0}", _lastLba);
            endLba = _lastLba;
        }

        // The whole measurement depends on a fixed, known speed: the calibrated rotation period is only valid as long as
        // the drive doesn't change speed. So a failed speed lock is fatal.
        _speed = speed == 0 ? (ushort)0xFFFF : speed;
        _kbps  = SpeedToKbps(_speed);

        if(!_drive.SetSpeed(_kbps))
        {
            ErrorMessage?.Invoke(Localization.Core.DPM_cannot_lock_the_drive_speed);

            return null;
        }

        AaruLogging.Debug(MODULE_NAME,
                          _drive.DisableReadCache()
                              ? "Disabled read cache via MODE SELECT caching page."
                              : "Drive doesn't support disabling its read cache via MODE SELECT; relying on cache-thrashing fallback.");

        try
        {
            UpdateStatus?.Invoke(Localization.Core.Calibrating_disc_rotation);
            SpinUpSettle(startLba);

            if(!Calibrate(startLba, out double t, out double aps))
            {
                ErrorMessage?.Invoke(string.Format(Localization.Core.DPM_calibration_failed_at_sector_0, startLba));

                return null;
            }

            UpdateStatus?.Invoke(string.Format(Localization.Core.Disc_turns_every_0_ms_1_RPM, t, 60000.0 / t));

            DataPositionMeasurement? dpm = MeasureDpm(startLba, endLba, t, aps);

            if(dpm is null && !_aborted) ErrorMessage?.Invoke(Localization.Core.DPM_measurement_failed);

            return dpm;
        }
        finally
        {
            _drive.RestoreReadCache();
            _drive.SetSpeed(0xFFFF);
        }
    }
}