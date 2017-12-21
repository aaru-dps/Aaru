// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Logging
{
    class MhddLog
    {
        FileStream mhddFs;

        internal MhddLog(string outputFile, Device dev, ulong blocks, ulong blockSize, ulong blocksToRead)
        {
            if(dev == null || string.IsNullOrEmpty(outputFile)) return;

            mhddFs = new FileStream(outputFile, FileMode.Create);

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

            string device = $"DEVICE: {dev.Manufacturer} {dev.Model}";
            string fw = $"F/W: {dev.Revision}";
            string sn = $"S/N: {dev.Serial}";
            string sectors = string.Format(new CultureInfo("en-US"), "SECTORS: {0:n0}", blocks);
            string sectorsize = string.Format(new CultureInfo("en-US"), "SECTOR SIZE: {0:n0} bytes",
                                              blockSize);
            string scanblocksize = string.Format(new CultureInfo("en-US"),
                                                 "SCAN BLOCK SIZE: {0:n0} sectors", blocksToRead);
            const string MHDD_VER = "VER:2 ";

            byte[] deviceBytes = Encoding.ASCII.GetBytes(device);
            byte[] modeBytes = Encoding.ASCII.GetBytes(mode);
            byte[] fwBytes = Encoding.ASCII.GetBytes(fw);
            byte[] snBytes = Encoding.ASCII.GetBytes(sn);
            byte[] sectorsBytes = Encoding.ASCII.GetBytes(sectors);
            byte[] sectorsizeBytes = Encoding.ASCII.GetBytes(sectorsize);
            byte[] scanblocksizeBytes = Encoding.ASCII.GetBytes(scanblocksize);
            byte[] verBytes = Encoding.ASCII.GetBytes(MHDD_VER);

            uint pointer = (uint)(deviceBytes.Length + modeBytes.Length + fwBytes.Length + snBytes.Length +
                                  sectorsBytes.Length + sectorsizeBytes.Length + scanblocksizeBytes.Length +
                                  verBytes.Length + 2 * 9 + // New lines
                                  4); // Pointer

            byte[] newLine = new byte[2];
            newLine[0] = 0x0D;
            newLine[1] = 0x0A;

            mhddFs.Write(BitConverter.GetBytes(pointer), 0, 4);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(verBytes, 0, verBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(modeBytes, 0, modeBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(deviceBytes, 0, deviceBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(fwBytes, 0, fwBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(snBytes, 0, snBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(sectorsBytes, 0, sectorsBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(sectorsizeBytes, 0, sectorsizeBytes.Length);
            mhddFs.Write(newLine, 0, 2);
            mhddFs.Write(scanblocksizeBytes, 0, scanblocksizeBytes.Length);
            mhddFs.Write(newLine, 0, 2);
        }

        internal void Write(ulong sector, double duration)
        {
            if(mhddFs == null) return;

            byte[] sectorBytes = BitConverter.GetBytes(sector);
            byte[] durationBytes = BitConverter.GetBytes((ulong)(duration * 1000));

            mhddFs.Write(sectorBytes, 0, 8);
            mhddFs.Write(durationBytes, 0, 8);
        }

        internal void Close()
        {
            if(mhddFs != null) mhddFs.Close();
        }
    }
}