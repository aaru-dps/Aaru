// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DriveCrash.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Detects and recovers from a firmware crash of MediaTek based drives.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Threading;
using Aaru.Core.Logging;
using Aaru.Decoders.SCSI;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>
    ///     Checks if a failed read crashed the drive, and revives it by cycling the tray. At random, an errored or
    ///     difficult sector makes the firmware of MediaTek based drives answer every command with invalid field in CDB,
    ///     and only an eject, with the same CDB used at the end of a dump, still works.
    /// </summary>
    /// <param name="sector">Sector that was being read</param>
    /// <param name="meter">Speed meter of the read in progress</param>
    /// <param name="ibgLog">ImgBurn log of the read in progress</param>
    /// <returns>
    ///     <c>true</c> if the read must be repeated or the dump has been aborted, <c>false</c> if it has to be handled
    ///     as a normal read error
    /// </returns>
    bool RecoverFromDriveCrash(ulong sector, SpeedMeter meter = null, IbgLog ibgLog = null)
    {
        if(_aborted) return true;

        // Only a crashed drive can say that TEST UNIT READY has an invalid field, a read may be genuinely rejected
        if(!DriveHasCrashed(out ReadOnlySpan<byte> senseBuf)) return false;

        bool dumpTimerRunning = _dumpStopwatch.IsRunning;
        _dumpStopwatch.Stop();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Drive_firmware_crashed_reading_sector_0_cycling_tray,
                                           sector));

        _errorLog?.WriteLine(sector, _dev.Error, _dev.LastError, senseBuf.ToArray());

        if(_dev.EjectTray(out _, _dev.Timeout, out _) || _dev.Error)
        {
            AbortForDriveCrash();

            return true;
        }

        _dev.LoadTray(out _, _dev.Timeout, out _);

        var askedToCloseTray = false;

        while(true)
        {
            if(_aborted) return true;

            if(DriveHasCrashed(out senseBuf))
            {
                AbortForDriveCrash();

                return true;
            }

            // The drive is ready
            if(senseBuf.IsEmpty) break;

            if(Sense.Decode(senseBuf)?.ASC == 0x3A && !askedToCloseTray)
            {
                UpdateStatus?.Invoke(Localization.Core.Close_drive_tray_with_same_disc_to_continue);
                PulseProgress?.Invoke(Localization.Core.Close_drive_tray_with_same_disc_to_continue);
                askedToCloseTray = true;
            }

            Thread.Sleep(2000);
        }

        if(_speedMultiplier >= 0) SetCdSpeedClamped(0xFFFF);

        if(dumpTimerRunning) _dumpStopwatch.Start();

        // The wait and the spin up are not part of the speed of the media
        meter?.Restart();
        ibgLog?.RestartRow();

        if(_lastCrashSector == sector)
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Sector_0_crashed_drive_again_treating_as_read_error,
                                               sector));

            return false;
        }

        _lastCrashSector = sector;
        UpdateStatus?.Invoke(Localization.Core.Drive_recovered_reading_sector_again);

        return true;
    }

    /// <summary>Checks if the drive answers TEST UNIT READY with invalid field in CDB</summary>
    /// <param name="senseBuf">Sense buffer of the command, empty if the drive is ready</param>
    /// <returns><c>true</c> if the drive has crashed</returns>
    bool DriveHasCrashed(out ReadOnlySpan<byte> senseBuf)
    {
        bool sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

        if(!sense)
        {
            if(!_dev.Error) senseBuf = ReadOnlySpan<byte>.Empty;

            return false;
        }

        DecodedSense? decodedSense = Sense.Decode(senseBuf);

        return decodedSense is { SenseKey: SenseKeys.IllegalRequest, ASC: 0x24, ASCQ: 0x00 };
    }

    /// <summary>Aborts the dump because the drive could not be revived</summary>
    void AbortForDriveCrash()
    {
        StoppingErrorMessage?.Invoke(Localization.Core.Drive_firmware_crashed_cycle_tray_and_resume_dump);
        _abortReason = AbortReason.FatalError;
        _aborted     = true;
    }
}