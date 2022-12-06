// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MHDDLog.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Glue logic to create a binary media scan log in MHDD's format.
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
using System.Globalization;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Devices;

namespace Aaru.Core.Logging
{
    /// <summary>Implements a log in the format used by MHDD</summary>
    internal sealed class MhddLog
    {
        const    string       MHDD_VER = "VER:2 ";
        readonly string       _logFile;
        readonly MemoryStream _mhddFs;

        /// <summary>Initializes the MHDD log</summary>
        /// <param name="outputFile">Log file</param>
        /// <param name="dev">Device</param>
        /// <param name="blocks">Blocks in media</param>
        /// <param name="blockSize">Bytes per block</param>
        /// <param name="blocksToRead">How many blocks read at once</param>
        /// <param name="private">Disable saving paths or serial numbers in log</param>
        internal MhddLog(string outputFile, Device dev, ulong blocks, ulong blockSize, ulong blocksToRead,
                         bool @private)
        {
            if(dev == null ||
               string.IsNullOrEmpty(outputFile))
                return;

            _mhddFs  = new MemoryStream();
            _logFile = outputFile;

            string mode;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                case DeviceType.ATAPI:
                    mode = "MODE: IDE";

                    break;
                case DeviceType.SCSI:
                    mode = "MODE: SCSI";

                    break;
                case DeviceType.MMC:
                    mode = "MODE: MMC";

                    break;
                case DeviceType.NVMe:
                    mode = "MODE: NVMe";

                    break;
                case DeviceType.SecureDigital:
                    mode = "MODE: SD";

                    break;
                default:
                    mode = "MODE: IDE";

                    break;
            }

            string device     = $"DEVICE: {dev.Manufacturer} {dev.Model}";
            string fw         = $"F/W: {dev.FirmwareRevision}";
            string sn         = $"S/N: {(@private ? "" : dev.Serial)}";
            string sectors    = string.Format(new CultureInfo("en-US"), "SECTORS: {0:n0}", blocks);
            string sectorSize = string.Format(new CultureInfo("en-US"), "SECTOR SIZE: {0:n0} bytes", blockSize);

            string scanBlockSize =
                string.Format(new CultureInfo("en-US"), "SCAN BLOCK SIZE: {0:n0} sectors", blocksToRead);

            byte[] deviceBytes        = Encoding.ASCII.GetBytes(device);
            byte[] modeBytes          = Encoding.ASCII.GetBytes(mode);
            byte[] fwBytes            = Encoding.ASCII.GetBytes(fw);
            byte[] snBytes            = Encoding.ASCII.GetBytes(sn);
            byte[] sectorsBytes       = Encoding.ASCII.GetBytes(sectors);
            byte[] sectorSizeBytes    = Encoding.ASCII.GetBytes(sectorSize);
            byte[] scanBlockSizeBytes = Encoding.ASCII.GetBytes(scanBlockSize);
            byte[] verBytes           = Encoding.ASCII.GetBytes(MHDD_VER);

            uint pointer = (uint)(deviceBytes.Length  + modeBytes.Length       + fwBytes.Length + snBytes.Length +
                                  sectorsBytes.Length + sectorSizeBytes.Length + scanBlockSizeBytes.Length +
                                  verBytes.Length     + (2 * 9)                + // New lines
                                  4);                                            // Pointer

            byte[] newLine = new byte[2];
            newLine[0] = 0x0D;
            newLine[1] = 0x0A;

            _mhddFs.Write(BitConverter.GetBytes(pointer), 0, 4);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(verBytes, 0, verBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(modeBytes, 0, modeBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(deviceBytes, 0, deviceBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(fwBytes, 0, fwBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(snBytes, 0, snBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(sectorsBytes, 0, sectorsBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(sectorSizeBytes, 0, sectorSizeBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
            _mhddFs.Write(scanBlockSizeBytes, 0, scanBlockSizeBytes.Length);
            _mhddFs.Write(newLine, 0, 2);
        }

        /// <summary>Logs a new read</summary>
        /// <param name="sector">Starting sector</param>
        /// <param name="duration">Duration in milliseconds</param>
        internal void Write(ulong sector, double duration)
        {
            if(_logFile == null)
                return;

            byte[] sectorBytes   = BitConverter.GetBytes(sector);
            byte[] durationBytes = BitConverter.GetBytes((ulong)(duration * 1000));

            _mhddFs.Write(sectorBytes, 0, 8);
            _mhddFs.Write(durationBytes, 0, 8);
        }

        /// <summary>Closes and writes to file the MHDD log</summary>
        internal void Close()
        {
            if(_logFile == null)
                return;

            var fs = new FileStream(_logFile, FileMode.Create);
            _mhddFs.WriteTo(fs);
            _mhddFs.Close();
            fs.Close();
        }
    }
}