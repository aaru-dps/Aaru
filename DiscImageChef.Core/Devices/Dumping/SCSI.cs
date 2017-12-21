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
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Core.Logging;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    public class Scsi
    {
        // TODO: Get cartridge serial number from Certance vendor EVPD
        public static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force,
                                bool dumpRaw, bool persistent, bool stopOnError, bool separateSubchannel,
                                ref Metadata.Resume resume, ref DumpLog dumpLog, bool dumpLeadIn, Encoding encoding)
        {
            byte[] senseBuf = null;
            bool sense = false;
            MediaType dskType = MediaType.Unknown;
            int resets = 0;

            if(dev.IsRemovable)
            {
                deviceGotReset:
                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out double duration);
                if(sense)
                {
                    Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
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
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if(!sense) break;

                                decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
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
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if(!sense) break;

                                decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Decoders.SCSI.Sense.PrettifySense(senseBuf));
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
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if(!sense) break;

                                decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                                if(decSense.HasValue)
                                    dumpLog.WriteLine("Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                      decSense.Value.SenseKey, decSense.Value.ASC, decSense.Value.ASCQ);
                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Decoders.SCSI.Sense.PrettifySense(senseBuf));
                                return;
                            }
                        }
                        else
                        {
                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                      Decoders.SCSI.Sense.PrettifySense(senseBuf));
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

            switch(dev.ScsiType) {
                case Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess:
                    if(dumpRaw) throw new ArgumentException("Tapes cannot be dumped raw.");

                    Ssc.Dump(dev, outputPrefix, devicePath, ref sidecar, ref resume, ref dumpLog);
                    return;
                case Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice:
                    Mmc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError,
                             ref sidecar, ref dskType, separateSubchannel, ref resume, ref dumpLog, dumpLeadIn, encoding);
                    return;
                default:
                    Sbc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar,
                             ref dskType, false, ref resume, ref dumpLog, encoding);
                    break;
            }
        }
    }
}