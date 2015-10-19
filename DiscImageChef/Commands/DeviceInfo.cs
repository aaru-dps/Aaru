// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceInfo.cs
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
using DiscImageChef.Devices;
using System.IO;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class DeviceInfo
    {
        public static void doDeviceInfo(DeviceInfoSubOptions options)
        {
            DicConsole.DebugWriteLine("Device-Info command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Device-Info command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Info command", "--device={0}", options.DevicePath);

            if (options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && Char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + Char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if (dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            byte[] senseBuf;
            byte[] inqBuf;

            bool sense = dev.ScsiInquiry(out inqBuf, out senseBuf);

            if(sense)
            {
                DicConsole.ErrorWriteLine("SCSI error. Sense decoding not yet implemented.");

                #if DEBUG
                FileStream senseFs = File.Open("sense.bin", FileMode.OpenOrCreate);
                senseFs.Write(senseBuf, 0, senseBuf.Length);
                #endif
            }
            else
                DicConsole.WriteLine("SCSI OK");

            DicConsole.WriteLine(Decoders.SCSI.Inquiry.Prettify(inqBuf));

            Structs.AtaErrorRegistersCHS errorRegisters;

            byte[] ataBuf;
            sense = dev.AtaIdentify(out ataBuf, out errorRegisters);

            if (sense)
            {
                
                if ((errorRegisters.status & 0x01) == 0x01
                   && (errorRegisters.error & 0x04) == 0x04
                   && errorRegisters.cylinderHigh == 0xEB
                   && errorRegisters.cylinderLow == 0x14)
                {
                    DicConsole.WriteLine("ATA error, but ATAPI signature detected.");
                    sense = dev.AtapiIdentify(out ataBuf, out errorRegisters);

                    if (sense)
                    {
                        DicConsole.WriteLine("ATAPI error");

                        DicConsole.DebugWriteLine("Device-Info command", "STATUS = 0x{0:X2}", errorRegisters.status);
                        DicConsole.DebugWriteLine("Device-Info command", "ERROR = 0x{0:X2}", errorRegisters.error);
                        DicConsole.DebugWriteLine("Device-Info command", "NSECTOR = 0x{0:X2}", errorRegisters.sectorCount);
                        DicConsole.DebugWriteLine("Device-Info command", "SECTOR = 0x{0:X2}", errorRegisters.sector);
                        DicConsole.DebugWriteLine("Device-Info command", "CYLHIGH = 0x{0:X2}", errorRegisters.cylinderHigh);
                        DicConsole.DebugWriteLine("Device-Info command", "CYLLOW = 0x{0:X2}", errorRegisters.cylinderLow);
                        DicConsole.DebugWriteLine("Device-Info command", "DEVICE = 0x{0:X2}", errorRegisters.deviceHead);
                        DicConsole.DebugWriteLine("Device-Info command", "COMMAND = 0x{0:X2}", errorRegisters.command);
                        DicConsole.DebugWriteLine("Device-Info command", "Error code = {0}", dev.LastError);
                    }
                    else
                    {
                        DicConsole.WriteLine("ATAPI OK");
                        DicConsole.WriteLine(Decoders.ATA.Identify.Prettify(ataBuf));
                    }
                }
                else
                {
                    DicConsole.WriteLine("ATA error");

                    DicConsole.DebugWriteLine("Device-Info command", "STATUS = 0x{0:X2}", errorRegisters.status);
                    DicConsole.DebugWriteLine("Device-Info command", "ERROR = 0x{0:X2}", errorRegisters.error);
                    DicConsole.DebugWriteLine("Device-Info command", "NSECTOR = 0x{0:X2}", errorRegisters.sectorCount);
                    DicConsole.DebugWriteLine("Device-Info command", "SECTOR = 0x{0:X2}", errorRegisters.sector);
                    DicConsole.DebugWriteLine("Device-Info command", "CYLHIGH = 0x{0:X2}", errorRegisters.cylinderHigh);
                    DicConsole.DebugWriteLine("Device-Info command", "CYLLOW = 0x{0:X2}", errorRegisters.cylinderLow);
                    DicConsole.DebugWriteLine("Device-Info command", "DEVICE = 0x{0:X2}", errorRegisters.deviceHead);
                    DicConsole.DebugWriteLine("Device-Info command", "COMMAND = 0x{0:X2}", errorRegisters.command);
                    DicConsole.DebugWriteLine("Device-Info command", "Error code = {0}", dev.LastError);
                }
            }
            else
            {
                DicConsole.WriteLine("ATA OK");
                DicConsole.WriteLine(Decoders.ATA.Identify.Prettify(ataBuf));
           }
        }
    }
}

