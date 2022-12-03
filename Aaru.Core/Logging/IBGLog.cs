// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Aaru.Devices;

namespace Aaru.Core.Logging;

/// <summary>Implements a log in the format used by IMGBurn</summary>
sealed class IbgLog
{
    readonly CultureInfo   _ibgCulture;
    readonly double        _ibgDivider;
    readonly string        _ibgMediaType;
    readonly StringBuilder _ibgSb;
    readonly string        _logFile;
    DateTime               _ibgDatePoint;
    ulong                  _ibgIntSector;
    double                 _ibgIntSpeed;
    double                 _ibgMaxSpeed;
    int                    _ibgSampleRate;
    int                    _ibgSnaps;
    bool                   _ibgStartSet;
    double                 _ibgStartSpeed;

    /// <summary>Initializes the IMGBurn log</summary>
    /// <param name="outputFile">Log file</param>
    /// <param name="currentProfile">Profile as defined by SCSI MultiMedia Commands specification</param>
    internal IbgLog(string outputFile, ushort currentProfile)
    {
        if(string.IsNullOrEmpty(outputFile))
            return;

        _logFile      = outputFile;
        _ibgSb        = new StringBuilder();
        _ibgDatePoint = DateTime.Now;
        _ibgCulture   = new CultureInfo("en-US");
        _ibgStartSet  = false;
        _ibgMaxSpeed  = 0;
        _ibgIntSpeed  = 0;
        _ibgSnaps     = 0;
        _ibgIntSector = 0;

        switch(currentProfile)
        {
            case 0x0001:
                _ibgMediaType = "HDD";
                _ibgDivider   = 1353;

                break;
            case 0x0002:
                _ibgMediaType = "PD-650";
                _ibgDivider   = 150;

                break;
            case 0x0005:
                _ibgMediaType = "CD-MO";
                _ibgDivider   = 150;

                break;
            case 0x0008:
                _ibgMediaType = "CD-ROM";
                _ibgDivider   = 150;

                break;
            case 0x0009:
                _ibgMediaType = "CD-R";
                _ibgDivider   = 150;

                break;
            case 0x000A:
                _ibgMediaType = "CD-RW";
                _ibgDivider   = 150;

                break;
            case 0x0010:
                _ibgMediaType = "DVD-ROM";
                _ibgDivider   = 1353;

                break;
            case 0x0011:
                _ibgMediaType = "DVD-R";
                _ibgDivider   = 1353;

                break;
            case 0x0012:
                _ibgMediaType = "DVD-RAM";
                _ibgDivider   = 1353;

                break;
            case 0x0013:
            case 0x0014:
                _ibgMediaType = "DVD-RW";
                _ibgDivider   = 1353;

                break;
            case 0x0015:
            case 0x0016:
                _ibgMediaType = "DVD-R DL";
                _ibgDivider   = 1353;

                break;
            case 0x0017:
                _ibgMediaType = "DVD-RW DL";
                _ibgDivider   = 1353;

                break;
            case 0x0018:
                _ibgMediaType = "DVD-Download";
                _ibgDivider   = 1353;

                break;
            case 0x001A:
                _ibgMediaType = "DVD+RW";
                _ibgDivider   = 1353;

                break;
            case 0x001B:
                _ibgMediaType = "DVD+R";
                _ibgDivider   = 1353;

                break;
            case 0x0020:
                _ibgMediaType = "DDCD-ROM";
                _ibgDivider   = 150;

                break;
            case 0x0021:
                _ibgMediaType = "DDCD-R";
                _ibgDivider   = 150;

                break;
            case 0x0022:
                _ibgMediaType = "DDCD-RW";
                _ibgDivider   = 150;

                break;
            case 0x002A:
                _ibgMediaType = "DVD+RW DL";
                _ibgDivider   = 1353;

                break;
            case 0x002B:
                _ibgMediaType = "DVD+R DL";
                _ibgDivider   = 1353;

                break;
            case 0x0040:
                _ibgMediaType = "BD-ROM";
                _ibgDivider   = 4500;

                break;
            case 0x0041:
            case 0x0042:
                _ibgMediaType = "BD-R";
                _ibgDivider   = 4500;

                break;
            case 0x0043:
                _ibgMediaType = "BD-RE";
                _ibgDivider   = 4500;

                break;
            case 0x0050:
                _ibgMediaType = "HD DVD-ROM";
                _ibgDivider   = 4500;

                break;
            case 0x0051:
                _ibgMediaType = "HD DVD-R";
                _ibgDivider   = 4500;

                break;
            case 0x0052:
                _ibgMediaType = "HD DVD-RAM";
                _ibgDivider   = 4500;

                break;
            case 0x0053:
                _ibgMediaType = "HD DVD-RW";
                _ibgDivider   = 4500;

                break;
            case 0x0058:
                _ibgMediaType = "HD DVD-R DL";
                _ibgDivider   = 4500;

                break;
            case 0x005A:
                _ibgMediaType = "HD DVD-RW DL";
                _ibgDivider   = 4500;

                break;
            default:
                _ibgMediaType = "Unknown";
                _ibgDivider   = 1353;

                break;
        }
    }

    /// <summary>Adds a new speed snapshot to the log</summary>
    /// <param name="sector">Sector for the snapshot</param>
    /// <param name="currentSpeed">Current speed at the snapshot</param>
    internal void Write(ulong sector, double currentSpeed)
    {
        if(_logFile == null)
            return;

        _ibgIntSpeed   += currentSpeed;
        _ibgSampleRate += (int)Math.Floor((DateTime.Now - _ibgDatePoint).TotalMilliseconds);
        _ibgSnaps++;

        if(_ibgSampleRate < 100)
            return;

        if(_ibgIntSpeed > 0 &&
           !_ibgStartSet)
        {
            _ibgStartSpeed = _ibgIntSpeed / _ibgSnaps / _ibgDivider;
            _ibgStartSet   = true;
        }

        _ibgSb.AppendFormat("{0:0.00},{1},{2:0},0", _ibgIntSpeed / _ibgSnaps / _ibgDivider, _ibgIntSector,
                            _ibgSampleRate).AppendLine();

        if(_ibgIntSpeed / _ibgSnaps / _ibgDivider > _ibgMaxSpeed)
            _ibgMaxSpeed = _ibgIntSpeed / _ibgDivider;

        _ibgDatePoint  = DateTime.Now;
        _ibgIntSpeed   = 0;
        _ibgSampleRate = 0;
        _ibgSnaps      = 0;
        _ibgIntSector  = sector;
    }

    /// <summary>Closes the IMGBurn log</summary>
    /// <param name="dev">Device</param>
    /// <param name="blocks">Media blocks</param>
    /// <param name="blockSize">Bytes per block</param>
    /// <param name="totalSeconds">Total seconds spent dumping</param>
    /// <param name="currentSpeed">Speed at the end</param>
    /// <param name="averageSpeed">Average speed</param>
    /// <param name="devicePath">Device path</param>
    internal void Close(Device dev, ulong blocks, ulong blockSize, double totalSeconds, double currentSpeed,
                        double averageSpeed, string devicePath)
    {
        if(_logFile == null)
            return;

        var    ibgFs     = new FileStream(_logFile, FileMode.Create);
        var    ibgHeader = new StringBuilder();
        string ibgBusType;

        if(dev.IsUsb)
            ibgBusType = "USB";
        else if(dev.IsFireWire)
            ibgBusType = "FireWire";
        else
            ibgBusType = dev.Type.ToString();

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
        ibgHeader.AppendFormat("DEVICE_FIRMWAREVERSION={0}", dev.FirmwareRevision).AppendLine();
        ibgHeader.AppendFormat("DEVICE_DRIVELETTER={0}", devicePath).AppendLine();
        ibgHeader.AppendFormat("DEVICE_BUSTYPE={0}", ibgBusType).AppendLine();
        ibgHeader.AppendLine();

        ibgHeader.AppendFormat("MEDIA_TYPE={0}", _ibgMediaType).AppendLine();
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
        ibgHeader.AppendFormat(_ibgCulture, "VERIFY_SPEED_START={0:0.00}", _ibgStartSpeed).AppendLine();
        ibgHeader.AppendFormat(_ibgCulture, "VERIFY_SPEED_END={0:0.00}", currentSpeed / _ibgDivider).AppendLine();

        ibgHeader.AppendFormat(_ibgCulture, "VERIFY_SPEED_AVERAGE={0:0.00}", averageSpeed / _ibgDivider).AppendLine();

        ibgHeader.AppendFormat(_ibgCulture, "VERIFY_SPEED_MAX={0:0.00}", _ibgMaxSpeed).AppendLine();
        ibgHeader.AppendFormat(_ibgCulture, "VERIFY_TIME_TAKEN={0:0}", Math.Floor(totalSeconds)).AppendLine();
        ibgHeader.AppendLine("[END_CONFIGURATION]");
        ibgHeader.AppendLine();
        ibgHeader.AppendLine("HRPC=True");
        ibgHeader.AppendLine();
        ibgHeader.AppendLine("[START_VERIFY_GRAPH_VALUES]");
        ibgHeader.Append(_ibgSb);
        ibgHeader.AppendLine("[END_VERIFY_GRAPH_VALUES]");
        ibgHeader.AppendLine();
        ibgHeader.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

        var sr = new StreamWriter(ibgFs);
        sr.Write(ibgHeader);
        sr.Close();
        ibgFs.Close();
    }
}