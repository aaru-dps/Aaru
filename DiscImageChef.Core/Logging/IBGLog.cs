// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IBGLog.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Glue logic to create a binary media scan log in ImgBurn's format.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Logging
{
    /// <summary>
    ///     Implements a log in the format used by IMGBurn
    /// </summary>
    class IbgLog
    {
        readonly CultureInfo   ibgCulture;
        DateTime               ibgDatePoint;
        readonly double        ibgDivider;
        ulong                  ibgIntSector;
        double                 ibgIntSpeed;
        double                 ibgMaxSpeed;
        readonly string        ibgMediaType;
        int                    ibgSampleRate;
        readonly StringBuilder ibgSb;
        int                    ibgSnaps;
        bool                   ibgStartSet;
        double                 ibgStartSpeed;
        readonly string        logFile;

        /// <summary>
        ///     Initializes the IMGBurn log
        /// </summary>
        /// <param name="outputFile">Log file</param>
        /// <param name="currentProfile">Profile as defined by SCSI MultiMedia Commands specification</param>
        internal IbgLog(string outputFile, ushort currentProfile)
        {
            if(string.IsNullOrEmpty(outputFile)) return;

            logFile      = outputFile;
            ibgSb        = new StringBuilder();
            ibgDatePoint = DateTime.Now;
            ibgCulture   = new CultureInfo("en-US");
            ibgStartSet  = false;
            ibgMaxSpeed  = 0;
            ibgIntSpeed  = 0;
            ibgSnaps     = 0;
            ibgIntSector = 0;

            switch(currentProfile)
            {
                case 0x0001:
                    ibgMediaType = "HDD";
                    ibgDivider   = 1353;
                    break;
                case 0x0002:
                    ibgMediaType = "PD-650";
                    ibgDivider   = 150;
                    break;
                case 0x0005:
                    ibgMediaType = "CD-MO";
                    ibgDivider   = 150;
                    break;
                case 0x0008:
                    ibgMediaType = "CD-ROM";
                    ibgDivider   = 150;
                    break;
                case 0x0009:
                    ibgMediaType = "CD-R";
                    ibgDivider   = 150;
                    break;
                case 0x000A:
                    ibgMediaType = "CD-RW";
                    ibgDivider   = 150;
                    break;
                case 0x0010:
                    ibgMediaType = "DVD-ROM";
                    ibgDivider   = 1353;
                    break;
                case 0x0011:
                    ibgMediaType = "DVD-R";
                    ibgDivider   = 1353;
                    break;
                case 0x0012:
                    ibgMediaType = "DVD-RAM";
                    ibgDivider   = 1353;
                    break;
                case 0x0013:
                case 0x0014:
                    ibgMediaType = "DVD-RW";
                    ibgDivider   = 1353;
                    break;
                case 0x0015:
                case 0x0016:
                    ibgMediaType = "DVD-R DL";
                    ibgDivider   = 1353;
                    break;
                case 0x0017:
                    ibgMediaType = "DVD-RW DL";
                    ibgDivider   = 1353;
                    break;
                case 0x0018:
                    ibgMediaType = "DVD-Download";
                    ibgDivider   = 1353;
                    break;
                case 0x001A:
                    ibgMediaType = "DVD+RW";
                    ibgDivider   = 1353;
                    break;
                case 0x001B:
                    ibgMediaType = "DVD+R";
                    ibgDivider   = 1353;
                    break;
                case 0x0020:
                    ibgMediaType = "DDCD-ROM";
                    ibgDivider   = 150;
                    break;
                case 0x0021:
                    ibgMediaType = "DDCD-R";
                    ibgDivider   = 150;
                    break;
                case 0x0022:
                    ibgMediaType = "DDCD-RW";
                    ibgDivider   = 150;
                    break;
                case 0x002A:
                    ibgMediaType = "DVD+RW DL";
                    ibgDivider   = 1353;
                    break;
                case 0x002B:
                    ibgMediaType = "DVD+R DL";
                    ibgDivider   = 1353;
                    break;
                case 0x0040:
                    ibgMediaType = "BD-ROM";
                    ibgDivider   = 4500;
                    break;
                case 0x0041:
                case 0x0042:
                    ibgMediaType = "BD-R";
                    ibgDivider   = 4500;
                    break;
                case 0x0043:
                    ibgMediaType = "BD-RE";
                    ibgDivider   = 4500;
                    break;
                case 0x0050:
                    ibgMediaType = "HD DVD-ROM";
                    ibgDivider   = 4500;
                    break;
                case 0x0051:
                    ibgMediaType = "HD DVD-R";
                    ibgDivider   = 4500;
                    break;
                case 0x0052:
                    ibgMediaType = "HD DVD-RAM";
                    ibgDivider   = 4500;
                    break;
                case 0x0053:
                    ibgMediaType = "HD DVD-RW";
                    ibgDivider   = 4500;
                    break;
                case 0x0058:
                    ibgMediaType = "HD DVD-R DL";
                    ibgDivider   = 4500;
                    break;
                case 0x005A:
                    ibgMediaType = "HD DVD-RW DL";
                    ibgDivider   = 4500;
                    break;
                default:
                    ibgMediaType = "Unknown";
                    ibgDivider   = 1353;
                    break;
            }
        }

        /// <summary>
        ///     Adds a new speed snapshot to the log
        /// </summary>
        /// <param name="sector">Sector for the snapshot</param>
        /// <param name="currentSpeed">Current speed at the snapshot</param>
        internal void Write(ulong sector, double currentSpeed)
        {
            if(logFile == null) return;

            ibgIntSpeed   += currentSpeed;
            ibgSampleRate += (int)Math.Floor((DateTime.Now - ibgDatePoint).TotalMilliseconds);
            ibgSnaps++;

            if(ibgSampleRate < 100) return;

            if(ibgIntSpeed > 0 && !ibgStartSet)
            {
                ibgStartSpeed = ibgIntSpeed / ibgSnaps / ibgDivider;
                ibgStartSet   = true;
            }

            ibgSb.AppendFormat("{0:0.00},{1},{2:0},0", ibgIntSpeed / ibgSnaps / ibgDivider, ibgIntSector, ibgSampleRate)
                 .AppendLine();
            if(ibgIntSpeed / ibgSnaps / ibgDivider > ibgMaxSpeed) ibgMaxSpeed = ibgIntSpeed / ibgDivider;

            ibgDatePoint  = DateTime.Now;
            ibgIntSpeed   = 0;
            ibgSampleRate = 0;
            ibgSnaps      = 0;
            ibgIntSector  = sector;
        }

        /// <summary>
        ///     Closes the IMGBurn log
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="blocks">Media blocks</param>
        /// <param name="blockSize">Bytes per block</param>
        /// <param name="totalSeconds">Total seconds spent dumping</param>
        /// <param name="currentSpeed">Speed at the end</param>
        /// <param name="averageSpeed">Average speed</param>
        /// <param name="devicePath">Device path</param>
        internal void Close(Device dev, ulong blocks, ulong blockSize, double totalSeconds,
                            double currentSpeed,
                            double averageSpeed, string devicePath)
        {
            if(logFile == null) return;

            FileStream    ibgFs     = new FileStream(logFile, FileMode.Create);
            StringBuilder ibgHeader = new StringBuilder();
            string        ibgBusType;

            if(dev.IsUsb) ibgBusType           = "USB";
            else if(dev.IsFireWire) ibgBusType = "FireWire";
            else ibgBusType                    = dev.Type.ToString();

            ibgHeader.AppendLine("IBGD");
            ibgHeader.AppendLine();
            ibgHeader.AppendLine("[START_CONFIGURATION]");
            ibgHeader.AppendLine("IBGD_VERSION=2");
            ibgHeader.AppendLine();
            ibgHeader.AppendFormat("DATE={0}", DateTime.Now).AppendLine();
            ibgHeader.AppendLine();
            ibgHeader.AppendFormat("SAMPLE_RATE={0}", 100).AppendLine();

            ibgHeader.AppendLine();
            ibgHeader.AppendFormat("DEVICE=[0:0:0] {0} {1} ({2}) ({3})", dev.Manufacturer, dev.Model, devicePath,
                                   ibgBusType).AppendLine();
            ibgHeader.AppendLine("DEVICE_ADDRESS=0:0:0");
            ibgHeader.AppendFormat("DEVICE_MAKEMODEL={0} {1}", dev.Manufacturer, dev.Model).AppendLine();
            ibgHeader.AppendFormat("DEVICE_FIRMWAREVERSION={0}", dev.Revision).AppendLine();
            ibgHeader.AppendFormat("DEVICE_DRIVELETTER={0}", devicePath).AppendLine();
            ibgHeader.AppendFormat("DEVICE_BUSTYPE={0}", ibgBusType).AppendLine();
            ibgHeader.AppendLine();

            ibgHeader.AppendFormat("MEDIA_TYPE={0}", ibgMediaType).AppendLine();
            ibgHeader.AppendLine("MEDIA_BOOKTYPE=Unknown");
            ibgHeader.AppendLine("MEDIA_ID=N/A");
            ibgHeader.AppendLine("MEDIA_TRACKPATH=PTP");
            ibgHeader.AppendLine("MEDIA_SPEEDS=N/A");
            ibgHeader.AppendFormat("MEDIA_CAPACITY={0}", blocks).AppendLine();
            ibgHeader.AppendLine("MEDIA_LAYER_BREAK=0");
            ibgHeader.AppendLine();
            ibgHeader.AppendLine("DATA_IMAGEFILE=/dev/null");
            ibgHeader.AppendFormat("DATA_SECTORS={0}", blocks).AppendLine();
            ibgHeader.AppendFormat("DATA_TYPE=MODE1/{0}", blockSize).AppendLine();
            ibgHeader.AppendLine("DATA_VOLUMEIDENTIFIER=");
            ibgHeader.AppendLine();
            ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_START={0:0.00}", ibgStartSpeed).AppendLine();
            ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_END={0:0.00}", currentSpeed / ibgDivider).AppendLine();
            ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_AVERAGE={0:0.00}", averageSpeed / ibgDivider).AppendLine();
            ibgHeader.AppendFormat(ibgCulture, "VERIFY_SPEED_MAX={0:0.00}", ibgMaxSpeed).AppendLine();
            ibgHeader.AppendFormat(ibgCulture, "VERIFY_TIME_TAKEN={0:0}", Math.Floor(totalSeconds)).AppendLine();
            ibgHeader.AppendLine("[END_CONFIGURATION]");
            ibgHeader.AppendLine();
            ibgHeader.AppendLine("HRPC=True");
            ibgHeader.AppendLine();
            ibgHeader.AppendLine("[START_VERIFY_GRAPH_VALUES]");
            ibgHeader.Append(ibgSb);
            ibgHeader.AppendLine("[END_VERIFY_GRAPH_VALUES]");
            ibgHeader.AppendLine();
            ibgHeader.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

            StreamWriter sr = new StreamWriter(ibgFs);
            sr.Write(ibgHeader);
            sr.Close();
            ibgFs.Close();
        }
    }
}