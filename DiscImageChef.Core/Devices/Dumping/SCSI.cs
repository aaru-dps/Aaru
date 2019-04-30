// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Threading;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping SCSI and ATAPI devices
    /// </summary>
    public partial class Dump
    {
        // TODO: Get cartridge serial number from Certance vendor EVPD
        /// <summary>
        ///     Dumps a SCSI Block Commands device or a Reduced Block Commands devices
        /// </summary>
        public void Scsi()
        {
            MediaType dskType = MediaType.Unknown;
            int       resets  = 0;

            if(dev.IsRemovable)
            {
                InitProgress?.Invoke();
                deviceGotReset:
                bool sense = dev.ScsiTestUnitReady(out byte[] senseBuf, dev.Timeout, out _);
                if(sense)
                {
                    FixedSense? decSense = Sense.DecodeFixed(senseBuf);
                    if(decSense.HasValue)
                    {
                        ErrorMessage
                          ?.Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");
                        dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                          decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);

                        // Just retry, for 5 times
                        if(decSense.Value.ASC == 0x29)
                        {
                            resets++;
                            if(resets < 5) goto deviceGotReset;
                        }

                        if(decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;
                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke("Waiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                decSense = Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                {
                                    ErrorMessage
                                      ?.Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");
                                    dumpLog.WriteLine("Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage?.Invoke("Please insert media in drive");
                                return;
                            }
                        }
                        else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 50;
                            while(leftRetries > 0)
                            {
                                PulseProgress?.Invoke("Waiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                decSense = Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                {
                                    ErrorMessage
                                      ?.Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage
                                  ?.Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");
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
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                decSense = Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                {
                                    ErrorMessage
                                      ?.Invoke($"Device not ready. Sense {decSense.Value.SenseKey} ASC {decSense.Value.ASC:X2}h ASCQ {decSense.Value.ASCQ:X2}h");
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                }

                                leftRetries--;
                            }

                            if(sense)
                            {
                                StoppingErrorMessage
                                  ?.Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");
                                return;
                            }
                        }
                        else
                        {
                            StoppingErrorMessage
                              ?.Invoke($"Error testing unit was ready:\n{Sense.PrettifySense(senseBuf)}");
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

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.SequentialAccess:
                    if(dumpRaw)
                    {
                        StoppingErrorMessage?.Invoke("Tapes cannot be dumped raw.");
                        return;
                    }

                    if(outputPlugin is IWritableTapeImage) Ssc();
                    else
                        StoppingErrorMessage
                          ?.Invoke("The specified plugin does not support storing streaming tape images.");
                    return;
                case PeripheralDeviceTypes.MultiMediaDevice:
                    if(outputPlugin is IWritableOpticalImage) Mmc(ref dskType);
                    else
                        StoppingErrorMessage
                          ?.Invoke("The specified plugin does not support storing optical disc images.");
                    return;
                default:
                    Sbc(null, ref dskType, false);
                    break;
            }
        }
    }
}