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
using System.Text;
using System.Threading;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;
using Schemas;
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
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump long or scrambled sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="separateSubchannel">Write subchannel separate from main channel</param>
        /// <param name="dumpLeadIn">Try to read and dump as much Lead-in as possible</param>
        /// <exception cref="ArgumentException">If you asked to dump long sectors from a SCSI Streaming device</exception>
        public static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force,
                                bool dumpRaw, bool persistent, bool stopOnError, bool separateSubchannel,
                                ref Resume resume, ref DumpLog dumpLog, bool dumpLeadIn, Encoding encoding)
        {
            MediaType dskType = MediaType.Unknown;
            int resets = 0;

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

            CICMMetadataType sidecar = new CICMMetadataType();

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.SequentialAccess:
                    if(dumpRaw) throw new ArgumentException("Tapes cannot be dumped raw.");

                    Ssc.Dump(dev, outputPrefix, devicePath, ref sidecar, ref resume, ref dumpLog);
                    return;
                case PeripheralDeviceTypes.MultiMediaDevice:
                    Mmc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError,
                             ref sidecar, ref dskType, separateSubchannel, ref resume, ref dumpLog, dumpLeadIn,
                             encoding);
                    return;
                default:
                    Sbc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError,
                             ref sidecar, ref dskType, false, ref resume, ref dumpLog, encoding);
                    break;
            }
        }
    }
}