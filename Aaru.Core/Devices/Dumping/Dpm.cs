// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dpm.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Measures Data Position Measurement as the last phase of optical disc dumps.
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

using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Devices.Dpm;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>DPM measurement in progress, so it can be aborted</summary>
    DpmMeasurement _dpmMeasurement;

    /// <summary>
    ///     Measures the Data Position Measurement of the disc and stores it in the image. It runs after every read, trim
    ///     and retry phase, so the sectors that could not be read are known and never read again.
    /// </summary>
    /// <remarks>
    ///     Measuring does its own plain reads at its own speed, so it does not matter how the data was read: raw, through
    ///     an OmniDrive firmware, or any other reader. A failure is reported but does not fail the dump.
    /// </remarks>
    /// <param name="output">Output image</param>
    void MeasureDpm(IWritableOpticalImage output)
    {
        if(!_dpm || _aborted || output is null) return;

        if(_dev is Aaru.Devices.Remote.Device)
        {
            UpdateStatus?.Invoke(Localization.Core.DPM_not_measured_on_remote_devices);

            return;
        }

        UpdateStatus?.Invoke(Localization.Core.Measuring_Data_Position_Measurement);

        _dpmMeasurement = new DpmMeasurement(new DeviceDpmDrive(_dev));

        _dpmMeasurement.UpdateStatus   += text => UpdateStatus?.Invoke(text);
        _dpmMeasurement.ErrorMessage   += text => ErrorMessage?.Invoke(text);
        _dpmMeasurement.InitProgress   += () => InitProgress?.Invoke();
        _dpmMeasurement.UpdateProgress += (text, current, maximum) => UpdateProgress?.Invoke(text, current, maximum);
        _dpmMeasurement.EndProgress    += () => EndProgress?.Invoke();

        try
        {
            if(!_dpmMeasurement.Prepare(0)) return;

            if(_dpmMeasurement.Kind == DpmMediumKind.Bd)
                UpdateStatus?.Invoke(Localization.Core.DPM_on_Bluray_is_experimental);

            _dpmMeasurement.SeedUnreadable(_resume?.BadBlocks ?? []);

            DataPositionMeasurement? dpm = _dpmMeasurement.Measure(0, _dpmMeasurement.LastLba, 0);

            if(dpm is null) return;

            if(!output.SetDpm(dpm.Value))
            {
                ErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_writing_DPM_to_image, output.ErrorMessage));

                return;
            }

            DpmSidecar.Write(dpm.Value, _outputPrefix);

            UpdateStatus?.Invoke(string.Format(Localization.Core.Measured_DPM_with_0_entries, dpm.Value.Entries.Length));
        }
        finally
        {
            _dpmMeasurement = null;

            // The measurement left the drive at its maximum speed
            AaruLogging.Debug(MODULE_NAME, "DPM phase finished");
        }
    }
}