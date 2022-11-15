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
                throw new NotImplementedException("Resuming CHS devices is currently not supported.");

            if(resume.Tape != isTape)
                throw new InvalidOperationException($"Resume file specifies a {(resume.Tape ? "tape" : "not tape")
                } device but you're requesting to dump a {(isTape ? "tape" : "not tape")} device, not continuing...");

            if(resume.Removable != removable &&
               !force)
                throw new InvalidOperationException($"Resume file specifies a {
                    (resume.Removable ? "removable" : "non removable")} device but you're requesting to dump a {
                        (removable ? "removable" : "non removable")} device, not continuing...");

            if(!isTape                        &&
               resume.LastBlock != blocks - 1 &&
               !force)
                throw new InvalidOperationException($"Resume file specifies a device with {resume.LastBlock + 1
                } blocks but you're requesting to dump one with {blocks} blocks, not continuing...");

            foreach(DumpHardwareType oldTry in resume.Tries)
            {
                if(!removable &&
                   !force)
                {
                    if(oldTry.Manufacturer != manufacturer)
                        throw new InvalidOperationException($"Resume file specifies a device manufactured by {
                            oldTry.Manufacturer} but you're requesting to dump one by {manufacturer
                            }, not continuing...");

                    if(oldTry.Model != model)
                        throw new InvalidOperationException($"Resume file specifies a device model {oldTry.Model
                        } but you're requesting to dump model {model}, not continuing...");

                    if(oldTry.Serial != serial)
                        throw new InvalidOperationException($"Resume file specifies a device with serial {oldTry.Serial
                        } but you're requesting to dump one with serial {serial}, not continuing...");

                    if(oldTry.Firmware != firmware)
                        throw new InvalidOperationException($"Resume file specifies a device with firmware version {
                            oldTry.Firmware} but you're requesting to dump one with firmware version {firmware
                            }, not continuing...");
                }

                if(oldTry.Software == null)
                    throw new InvalidOperationException("Found corrupt resume file, cannot continue...");

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