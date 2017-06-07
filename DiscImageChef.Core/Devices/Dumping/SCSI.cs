// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    public class SCSI
    {
        // TODO: Get cartridge serial number from Certance vendor EVPD
        public static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force, bool dumpRaw, bool persistent, bool stopOnError)
        {
            byte[] senseBuf = null;
            bool sense = false;
            double duration;
            MediaType dskType = MediaType.Unknown;

            if(dev.IsRemovable)
            {
                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                if(sense)
                {
                    Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                    if(decSense.HasValue)
                    {
                        if(decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                System.Threading.Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out duration);
                                if(!sense)
                                    break;

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
                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
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
                        else
                        {
                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
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

            if(dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
            {
                if(dumpRaw)
                    throw new ArgumentException("Tapes cannot be dumped raw.");

                SSC.Dump(dev, outputPrefix, devicePath, ref sidecar);
                return;
            }

            FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml",
                                          FileMode.Create);
            System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));

            if(dev.SCSIType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
            {
                MMC.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar, ref dskType);

                DicConsole.WriteLine("Writing metadata sidecar");

                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();

                return;
            }

            SBC.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar, ref dskType, false);

            DicConsole.WriteLine("Writing metadata sidecar");

            xmlSer.Serialize(xmlFs, sidecar);
            xmlFs.Close();
        }
    }
}
