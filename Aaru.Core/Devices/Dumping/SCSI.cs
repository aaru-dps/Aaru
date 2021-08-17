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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;

namespace Aaru.Core.Devices.Dumping
{
    /// <summary>Implements dumping SCSI and ATAPI devices</summary>
    public partial class Dump
    {
        // TODO: Get cartridge serial number from Certance vendor EVPD
        /// <summary>Dumps a SCSI Block Commands device or a Reduced Block Commands devices</summary>
        void Scsi()
        {
            int resets = 0;

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
                        ErrorMessage?.
                            Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");

                        _dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                           decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);

                        // Just retry, for 5 times
                        if(decSense.Value.ASC == 0x29)
                        {
                            resets++;

                            if(resets < 5)
                                goto deviceGotReset;
                        }

                        if(decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke("Waiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense)
                                    break;

                                decSense = Sense.Decode(senseBuf);

                                if(decSense.HasValue)
                                {
                                    ErrorMessage?.
                                        Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");

                                    _dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                                       decSense.Value.SenseKey, decSense.Value.ASC,
                                                       decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.Invoke("Please insert media in drive");

                                return;
                            }
                        }
                        else if(decSense.Value.ASC  == 0x04 &&
                                decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 50;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke("Waiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense)
                                    break;

                                decSense = Sense.Decode(senseBuf);

                                if(decSense.HasValue)
                                {
                                    ErrorMessage?.
                                        Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");

                                    _dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                       decSense.Value.SenseKey, decSense.Value.ASC,
                                                       decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.
                                    Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");

                                return;
                            }
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
                        else if(decSense.Value.ASC == 0x28)
                        {
                            int leftRetries = 10;

                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke("Waiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = _dev.ScsiTestUnitReady(out senseBuf, _dev.Timeout, out _);

                                if(!sense)
                                    break;

                                decSense = Sense.Decode(senseBuf);

                                if(decSense.HasValue)
                                {
                                    ErrorMessage?.
                                        Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");

                                    _dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                       decSense.Value.SenseKey, decSense.Value.ASC,
                                                       decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.
                                    Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");

                                return;
                            }
                        }
                        else
                        {
                            StoppingErrorMessage?.
                                Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");

                            return;
                        }
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke("Unknown testing unit was ready.");

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
                        StoppingErrorMessage?.Invoke("Tapes cannot be dumped raw.");

                        return;
                    }

                    if(_outputPlugin is IWritableTapeImage)
                        Ssc();
                    else
                        StoppingErrorMessage?.
                            Invoke("The specified plugin does not support storing streaming tape images.");

                    return;
                case PeripheralDeviceTypes.MultiMediaDevice:
                    if(_outputPlugin is IWritableOpticalImage)
                        Mmc();
                    else
                        StoppingErrorMessage?.
                            Invoke("The specified plugin does not support storing optical disc images.");

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
}