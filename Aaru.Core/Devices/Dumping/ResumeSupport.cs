// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ResumeSupport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains logic to support dump resuming.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Metadata;
using Schemas;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements resume support</summary>
static class ResumeSupport
{
    /// <summary>Process resume</summary>
    /// <param name="isLba">If drive is LBA</param>
    /// <param name="removable">If media is removable from device</param>
    /// <param name="blocks">Media blocks</param>
    /// <param name="manufacturer">Device manufacturer</param>
    /// <param name="model">Device model</param>
    /// <param name="serial">Device serial</param>
    /// <param name="platform">Platform where the dump is made</param>
    /// <param name="resume">Previous resume, or null</param>
    /// <param name="currentTry">Current dumping hardware</param>
    /// <param name="extents">Dumped extents</param>
    /// <param name="firmware">Firmware revision</param>
    /// <param name="isTape">Set to <c>true</c> if device is a streaming tape, <c>false</c> otherwise</param>
    /// <param name="private">Disable saving paths or serial numbers in images and logs</param>
    /// <param name="force">Force dump enabled</param>
    /// <exception cref="System.NotImplementedException">If device uses CHS addressing</exception>
    /// <exception cref="System.InvalidOperationException">
    ///     If the provided resume does not correspond with the current in
    ///     progress dump
    /// </exception>
    internal static void Process(bool isLba, bool removable, ulong blocks, string manufacturer, string model,
                                 string serial, PlatformID platform, ref Resume resume, ref DumpHardwareType currentTry,
                                 ref ExtentsULong extents, string firmware, bool @private, bool force,
                                 bool isTape = false)
    {
        if(@private)
            serial = null;

        if(resume != null)
        {
            if(!isLba)
                throw new NotImplementedException(Localization.Core.Resuming_CHS_devices_is_currently_not_supported);

            if(resume.Tape != isTape)
            {
                if(resume.Tape)
                    throw new InvalidOperationException(Localization.Core.Resume_specifies_tape_but_device_is_not_tape);

                throw new InvalidOperationException(Localization.Core.Resume_specifies_not_tape_but_device_is_tape);
            }

            if(resume.Removable != removable &&
               !force)
            {
                if(resume.Removable)
                    throw new InvalidOperationException(Localization.Core.
                                                                     Resume_specifies_removable_but_device_is_non_removable);

                throw new InvalidOperationException(Localization.Core.
                                                                 Resume_specifies_non_removable_but_device_is_removable);
            }

            if(!isTape                        &&
               resume.LastBlock != blocks - 1 &&
               !force)
                throw new
                    InvalidOperationException(string.Format(Localization.Core.Resume_file_different_number_of_blocks_not_continuing,
                                                            resume.LastBlock + 1, blocks));

            foreach(DumpHardwareType oldTry in resume.Tries)
            {
                if(!removable &&
                   !force)
                {
                    if(oldTry.Manufacturer != manufacturer)
                        throw new
                            InvalidOperationException(string.
                                                          Format(Localization.Core.Resume_file_different_manufacturer_not_continuing,
                                                                 oldTry.Manufacturer, manufacturer));

                    if(oldTry.Model != model)
                        throw new
                            InvalidOperationException(string.
                                                          Format(Localization.Core.Resume_file_different_model_not_continuing,
                                                                 oldTry.Model, model));

                    if(oldTry.Serial != serial)
                        throw new
                            InvalidOperationException(string.
                                                          Format(Localization.Core.Resume_file_different_serial_number_not_continuing,
                                                                 oldTry.Serial, serial));

                    if(oldTry.Firmware != firmware)
                        throw new
                            InvalidOperationException(string.
                                                          Format(Localization.Core.Resume_file_different_firmware_revision_not_continuing,
                                                                 oldTry.Firmware, firmware));
                }

                if(oldTry.Software == null)
                    throw new InvalidOperationException(Localization.Core.Found_corrupt_resume_file_cannot_continue);

                if(oldTry.Software.Name            != "Aaru"              ||
                   oldTry.Software.OperatingSystem != platform.ToString() ||
                   oldTry.Software.Version         != Version.GetVersion())
                    continue;

                if(removable && (oldTry.Manufacturer != manufacturer || oldTry.Model    != model ||
                                 oldTry.Serial       != serial       || oldTry.Firmware != firmware))
                    continue;

                currentTry = oldTry;
                extents    = ExtentsConverter.FromMetadata(currentTry.Extents);

                break;
            }

            if(currentTry != null)
                return;

            currentTry = new DumpHardwareType
            {
                Software     = CommonTypes.Metadata.Version.GetSoftwareType(),
                Manufacturer = manufacturer,
                Model        = model,
                Serial       = serial,
                Firmware     = firmware
            };

            resume.Tries.Add(currentTry);
            extents = new ExtentsULong();
        }
        else
        {
            resume = new Resume
            {
                Tries        = new List<DumpHardwareType>(),
                CreationDate = DateTime.UtcNow,
                BadBlocks    = new List<ulong>(),
                LastBlock    = isTape ? 0 : blocks - 1,
                Tape         = isTape
            };

            currentTry = new DumpHardwareType
            {
                Software     = CommonTypes.Metadata.Version.GetSoftwareType(),
                Manufacturer = manufacturer,
                Model        = model,
                Serial       = serial,
                Firmware     = firmware
            };

            resume.Tries.Add(currentTry);
            extents          = new ExtentsULong();
            resume.Removable = removable;
        }
    }
}