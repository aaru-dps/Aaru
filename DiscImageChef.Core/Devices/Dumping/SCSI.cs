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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using DiscImageChef.DiscImages;
using DiscImageChef.Metadata;
using MediaType = DiscImageChef.CommonTypes.MediaType;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implements dumping SCSI and ATAPI devices
    /// </summary>
    public class Scsi
    {
        // TODO: Get cartridge serial number from Certance vendor EVPD
        /// <summary>
        ///     Dumps a SCSI Block Commands device or a Reduced Block Commands devices
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output data files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump long or scrambled sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="dumpLeadIn">Try to read and dump as much Lead-in as possible</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <exception cref="ArgumentException">If you asked to dump long sectors from a SCSI Streaming device</exception>
        public static void Dump(Device     dev, string devicePath, IWritableImage outputPlugin, ushort retryPasses,
                                bool       force, bool dumpRaw, bool              persistent, bool     stopOnError,
                                ref Resume resume,
                                ref
                                    DumpLog dumpLog, bool dumpLeadIn, Encoding encoding,
                                string      outputPrefix,
                                string
                                    outputPath, Dictionary<string, string> formatOptions)
        {
            MediaType dskType = MediaType.Unknown;
            int       resets  = 0;

            if(dev.IsRemovable)
            {
                deviceGotReset:
                bool sense = dev.ScsiTestUnitReady(out byte[] senseBuf, dev.Timeout, out _);
                if(sense)
                {
                    FixedSense? decSense = Sense.DecodeFixed(senseBuf);
                    if(decSense.HasValue)
                    {
                        dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
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
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                decSense = Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Please insert media in drive");
                                return;
                            }
                        }
                        else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 10;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                decSense = Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                                return;
                            }
                        }
                        /*else if (decSense.Value.ASC == 0x29 && decSense.Value.ASCQ == 0x00)
                        {
                            if (!deviceReset)
                            {
                                deviceReset = true;
                                DicConsole.ErrorWriteLine("Device did reset, retrying...");
                                goto retryTestReady;
                            }

                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                            return;
                        }*/
                        // These should be trapped by the OS but seems in some cases they're not
                        else if(decSense.Value.ASC == 0x28)
                        {
                            int leftRetries = 10;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                decSense = Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                                return;
                            }
                        }
                        else
                        {
                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                            return;
                        }
                    }
                    else
                    {
                        DicConsole.ErrorWriteLine("Unknown testing unit was ready.");
                        return;
                    }
                }
            }

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.SequentialAccess:
                    if(dumpRaw) throw new ArgumentException("Tapes cannot be dumped raw.");

                    Ssc.Dump(dev, outputPrefix, devicePath, ref resume, ref dumpLog);
                    return;
                case PeripheralDeviceTypes.MultiMediaDevice:
                    Mmc.Dump(dev, devicePath, outputPlugin, retryPasses, force, dumpRaw, persistent, stopOnError,
                             ref dskType, ref resume, ref dumpLog, dumpLeadIn, encoding, outputPrefix, outputPath,
                             formatOptions);
                    return;
                default:
                    Sbc.Dump(dev, devicePath, outputPlugin, retryPasses, force, dumpRaw, persistent, stopOnError, null,
                             ref dskType, false, ref resume, ref dumpLog, encoding, outputPrefix, outputPath,
                             formatOptions);
                    break;
            }
        }
    }
}