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

namespace DiscImageChef.Commands
{
    public static class DeviceInfo
    {
        public static void doDeviceInfo(DeviceInfoSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--device={0}", options.DevicePath);
            }

            if (options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && Char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + Char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if (dev.Error)
            {
                Console.WriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            byte[] senseBuf;
            byte[] inqBuf;

            bool sense = dev.ScsiInquiry(out inqBuf, out senseBuf);

            if(sense)
            {
                Console.WriteLine("SCSI error. Sense decoding not yet implemented.");

                #if DEBUG
                FileStream senseFs = File.Open("sense.bin", FileMode.OpenOrCreate);
                senseFs.Write(senseBuf, 0, senseBuf.Length);
                #endif
            }
            else
                Console.WriteLine("SCSI OK");

            Console.WriteLine("{0}", Decoders.SCSI.PrettifySCSIInquiry(inqBuf));
        }
    }
}

