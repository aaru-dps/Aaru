// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from SCSI devices.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implements dumping SCSI and ATAPI devices</summary>
public partial class Dump
{
    // TODO: Get cartridge serial number from Certance vendor EVPD
    /// <summary>Dumps a SCSI Block Commands device or a Reduced Block Commands devices</summary>
    void Scsi()
    {
        var resets = 0;

        if(_dev.IsRemovable)
        {
            InitProgress?.Invoke();
        deviceGotReset:
            bool sense = _dev.ScsiTestUnitReady(out byte[] senseBuf, _dev.Timeout, out _);

            if(sense)
            {
                DecodedSense? decSense = Sense.Decode(senseBuf);

                if(decSense.HasValue)
                {
                    ErrorMessage?.Invoke(string.Format(Localization.Core.Device_not_ready_Sense,
                                                       decSense.Value.SenseKey,
                                                       decSense.Value.ASC,
                                                       decSense.Value.ASCQ));

                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense,
                                       decSense.Value.SenseKey,
                                       decSense.Value.ASC,
                                       decSense.Value.ASCQ);

                    // Just retry, for 5 times
                    if(decSense.Value.ASC == 0x29)
                    {
                        resets++;

                        if(resets < 5) goto deviceGotReset;
                    }

                    switch(decSense.Value.ASC)
                    {
                        case 0x3A:
                        {
                            var leftRetries = 5;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke(Localization.Core.Waiting_for_drive_to_become_ready);
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense) break;

                                decSense = Sense.Decode(senseBuf);

                                if(decSense.HasValue)
                                {
                                    ErrorMessage?.Invoke(string.Format(Localization.Core.Device_not_ready_Sense,
                                                                       decSense.Value.SenseKey,
                                                                       decSense.Value.ASC,
                                                                       decSense.Value.ASCQ));

                                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense,
                                                       decSense.Value.SenseKey,
                                                       decSense.Value.ASC,
                                                       decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.Invoke(Localization.Core.Please_insert_media_in_drive);

                                return;
                            }

                            break;
                        }
                        case 0x04 when decSense.Value.ASCQ == 0x01:
                        {
                            var leftRetries = 50;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke(Localization.Core.Waiting_for_drive_to_become_ready);
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense) break;

                                decSense = Sense.Decode(senseBuf);

                                if(decSense.HasValue)
                                {
                                    ErrorMessage?.Invoke(string.Format(Localization.Core.Device_not_ready_Sense,
                                                                       decSense.Value.SenseKey,
                                                                       decSense.Value.ASC,
                                                                       decSense.Value.ASCQ));

                                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense,
                                                       decSense.Value.SenseKey,
                                                       decSense.Value.ASC,
                                                       decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.Invoke(string.Format(Localization.Core
                                                                              .Error_testing_unit_was_ready_0,
                                                                           Sense.PrettifySense(senseBuf)));

                                return;
                            }

                            break;
                        }
                        /*else if (decSense.Value.ASC == 0x29 && decSense.Value.ASCQ == 0x00)
                    {
                        if (!deviceReset)
                        {
                            deviceReset = true;
                            ErrorMessage?.Invoke("Device did reset, retrying...");
                            goto retryTestReady;
                        }

                        StoppingErrorMessage?.Invoke(string.Format("Error testing unit was ready:\n{0}",
                                                     Decoders.SCSI.Sense.PrettifySense(senseBuf)));
                        return;
                    }*/
                        // These should be trapped by the OS but seems in some cases they're not
                        case 0x28:
                        {
                            var leftRetries = 10;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke(Localization.Core.Waiting_for_drive_to_become_ready);
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense) break;

                                decSense = Sense.Decode(senseBuf);

                                if(decSense.HasValue)
                                {
                                    ErrorMessage?.Invoke(string.Format(Localization.Core.Device_not_ready_Sense,
                                                                       decSense.Value.SenseKey,
                                                                       decSense.Value.ASC,
                                                                       decSense.Value.ASCQ));

                                    _dumpLog.WriteLine(Localization.Core.Device_not_ready_Sense,
                                                       decSense.Value.SenseKey,
                                                       decSense.Value.ASC,
                                                       decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.Invoke(string.Format(Localization.Core
                                                                              .Error_testing_unit_was_ready_0,
                                                                           Sense.PrettifySense(senseBuf)));

                                return;
                            }

                            break;
                        }
                        default:
                            StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_testing_unit_was_ready_0,
                                                                       Sense.PrettifySense(senseBuf)));

                            return;
                    }
                }
                else
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.Unknown_sense_testing_unit_was_ready);

                    return;
                }
            }

            EndProgress?.Invoke();
        }

        switch(_dev.ScsiType)
        {
            case PeripheralDeviceTypes.SequentialAccess:
                if(_dumpRaw)
                {
                    StoppingErrorMessage?.Invoke(Localization.Core.Tapes_cannot_be_dumped_raw);

                    return;
                }

                if(_outputPlugin is IWritableTapeImage)
                    Ssc();
                else
                {
                    StoppingErrorMessage?.Invoke(Localization.Core
                                                             .The_specified_image_format_cannot_represent_streaming_tapes);
                }

                return;
            case PeripheralDeviceTypes.MultiMediaDevice:
                if(_outputPlugin is IWritableOpticalImage)
                    Mmc();
                else
                {
                    StoppingErrorMessage?.Invoke(Localization.Core
                                                             .The_specified_image_format_cannot_represent_optical_discs);
                }

                return;
            case PeripheralDeviceTypes.BridgingExpander
                when _dev.Model.StartsWith("MDM", StringComparison.InvariantCulture) ||
                     _dev.Model.StartsWith("MDH", StringComparison.InvariantCulture):
                MiniDisc();

                break;
            default:
                Sbc(null, MediaType.Unknown, false);

                break;
        }
    }
}