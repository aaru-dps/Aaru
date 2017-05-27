// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaScan.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-scan' verb.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;
using DiscImageChef.Core.Devices.Scanning;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands
{
    public static class MediaScan
    {
        public static void doMediaScan(MediaScanOptions options)
        {
            DicConsole.DebugWriteLine("Media-Scan command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Media-Scan command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Media-Scan command", "--device={0}", options.DevicePath);
            DicConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", options.MHDDLogPath);
            DicConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}", options.IBGLogPath);

            if(!System.IO.File.Exists(options.DevicePath))
            {
                DicConsole.ErrorWriteLine("Specified device does not exist.");
                return;
            }

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    ATA.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.NVMe:
                    NVMe.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    SCSI.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }

            Core.Statistics.AddCommand("media-scan");
        }
    }
}

