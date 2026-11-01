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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics;
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
    readonly Stopwatch     _ibgStopwatch;
    readonly string        _logFile;
    ulong                  _ibgIntSector;
    double                 _ibgIntSpeed;
    double                 _ibgMaxSpeed;
    int                    _ibgSampleRate;
    int                    _ibgSnaps;
    bool                   _ibgStartSet;
    double                 _ibgStartSpeed;
    double                 _resumedAverageSpeed;
    double                 _resumedEndSpeed;
    bool                   _resumedLog;
    double                 _resumedSeconds;

    /// <summary>Initializes the IMGBurn log</summary>
    /// <param name="outputFile">Log file</param>
    /// <param name="currentProfile">Profile as defined by SCSI MultiMedia Commands specification</param>
    /// <param name="resume">Keep what a previous run wrote to the log file</param>
    internal IbgLog(string outputFile, ushort currentProfile, bool resume = false)
    {
        if(string.IsNullOrEmpty(outputFile)) return;

        _logFile      = outputFile;
        _ibgSb        = new StringBuilder();
        _ibgStopwatch = new Stopwatch();
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

        if(resume) LoadExisting();
    }

    /// <summary>Loads the rows and totals a previous run wrote to the log file</summary>
    void LoadExisting()
    {
        if(!File.Exists(_logFile)) return;

        string[] lines = File.ReadAllLines(_logFile);
        int      start = Array.IndexOf(lines, "[START_VERIFY_GRAPH_VALUES]");
        int      end   = Array.IndexOf(lines, "[END_VERIFY_GRAPH_VALUES]");

        if(start < 0 || end < start) return;

        for(int i = start + 1; i < end; i++) _ibgSb.AppendLine(lines[i]);

        _ibgStartSpeed       = ReadTotal(lines, "VERIFY_SPEED_START");
        _ibgStartSet         = _ibgStartSpeed > 0;
        _ibgMaxSpeed         = ReadTotal(lines, "VERIFY_SPEED_MAX");
        _resumedEndSpeed     = ReadTotal(lines, "VERIFY_SPEED_END");
        _resumedAverageSpeed = ReadTotal(lines, "VERIFY_SPEED_AVERAGE");
        _resumedSeconds      = ReadTotal(lines, "VERIFY_TIME_TAKEN");
        _resumedLog          = true;
    }

    /// <summary>Reads a total from the configuration section of a log file</summary>
    /// <param name="lines">Lines of the log file</param>
    /// <param name="name">Name of the total</param>
    /// <returns>The total, or 0 if not found</returns>
    double ReadTotal(string[] lines, string name)
    {
        string line = Array.Find(lines, l => l.StartsWith(name + "=", StringComparison.Ordinal));

        return double.TryParse(line?[(name.Length + 1)..], NumberStyles.Float, _ibgCulture, out double total)
                   ? total
                   : 0;
    }

    /// <summary>Merges the totals of a previous run with the ones of this run</summary>
    /// <param name="previousSeconds">Seconds taken by the previous run</param>
    /// <param name="previousEndSpeed">Speed at the end of the previous run</param>
    /// <param name="previousAverageSpeed">Average speed of the previous run</param>
    /// <param name="seconds">Seconds taken by this run</param>
    /// <param name="endSpeed">Speed at the end of this run</param>
    /// <param name="averageSpeed">Average speed of this run</param>
    /// <param name="measured">Whether this run measured any speed</param>
    /// <returns>Seconds, end speed and average speed of all the runs</returns>
    internal static (double Seconds, double EndSpeed, double AverageSpeed)
        MergeTotals(double previousSeconds, double previousEndSpeed, double previousAverageSpeed, double seconds,
                    double endSpeed,        double averageSpeed,     bool   measured)
    {
        if(!measured) return (previousSeconds, previousEndSpeed, previousAverageSpeed);

        double totalSeconds = previousSeconds + seconds;

        if(totalSeconds <= 0) return (seconds, endSpeed, averageSpeed);

        return (totalSeconds, endSpeed,
                (previousAverageSpeed * previousSeconds + averageSpeed * seconds) / totalSeconds);
    }

    /// <summary>Adds a new speed snapshot to the log</summary>
    /// <param name="sector">Sector for the snapshot</param>
    /// <param name="currentSpeed">Current speed at the snapshot</param>
    internal void Write(ulong sector, double currentSpeed)
    {
        if(_logFile == null) return;

        // The graph starts at the first measured speed, what was done before that is not part of it
        if(!_ibgStopwatch.IsRunning)
        {
            if(currentSpeed <= 0) return;

            _ibgIntSector = sector;
            _ibgStopwatch.Start();
        }

        _ibgIntSpeed   += currentSpeed;
        _ibgSampleRate =  (int)Math.Floor(_ibgStopwatch.Elapsed.TotalMilliseconds);
        _ibgSnaps++;

        if(_ibgSampleRate < 100) return;

        WriteRow();
        _ibgStopwatch.Restart();
        _ibgIntSector = sector;
    }

    /// <summary>Writes the row with the snapshots added since the previous one</summary>
    void WriteRow()
    {
        if(_ibgIntSpeed > 0 && !_ibgStartSet)
        {
            _ibgStartSpeed = _ibgIntSpeed / _ibgSnaps / _ibgDivider;
            _ibgStartSet   = true;
        }

        _ibgSb.Append(_ibgCulture,
                      $"{_ibgIntSpeed / _ibgSnaps / _ibgDivider:0.00},{_ibgIntSector},{_ibgSampleRate:0},0")
              .AppendLine();

        double rowSpeed = _ibgIntSpeed / _ibgSnaps / _ibgDivider;

        if(rowSpeed > _ibgMaxSpeed) _ibgMaxSpeed = rowSpeed;

        _ibgIntSpeed   = 0;
        _ibgSampleRate = 0;
        _ibgSnaps      = 0;
    }

    /// <summary>Writes the snapshots that did not fill a whole row yet</summary>
    internal void WritePendingRow()
    {
        if(_ibgSnaps > 0) WriteRow();
    }

    /// <summary>Closes the IMGBurn log</summary>
    /// <param name="dev">Device</param>
    /// <param name="blocks">Media blocks</param>
    /// <param name="blockSize">Bytes per block</param>
    /// <param name="totalSeconds">Total seconds spent dumping</param>
    /// <param name="currentSpeed">Speed at the end</param>
    /// <param name="averageSpeed">Average speed</param>
    /// <param name="devicePath">Device path</param>
    internal void Close(Device dev,          ulong  blocks, ulong blockSize, double totalSeconds, double currentSpeed,
                        double averageSpeed, string devicePath)
    {
        if(_logFile == null) return;

        WritePendingRow();

        if(_resumedLog)
        {
            (totalSeconds, currentSpeed, averageSpeed) = MergeTotals(_resumedSeconds,
                                                                     _resumedEndSpeed     * _ibgDivider,
                                                                     _resumedAverageSpeed * _ibgDivider,
                                                                     totalSeconds,
                                                                     currentSpeed,
                                                                     averageSpeed,
                                                                     _ibgStopwatch.IsRunning);
        }

        _ibgStopwatch.Stop();

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
        ibgHeader.Append(_ibgCulture, $"DATE={DateTime.Now}").AppendLine();
        ibgHeader.AppendLine();
        ibgHeader.Append($"SAMPLE_RATE={100}").AppendLine();

        ibgHeader.AppendLine();

        ibgHeader.Append($"DEVICE=[0:0:0] {dev.Manufacturer} {dev.Model} ({devicePath}) ({ibgBusType})").AppendLine();

        ibgHeader.AppendLine("DEVICE_ADDRESS=0:0:0");
        ibgHeader.Append($"DEVICE_MAKEMODEL={dev.Manufacturer} {dev.Model}").AppendLine();
        ibgHeader.Append($"DEVICE_FIRMWAREVERSION={dev.FirmwareRevision}").AppendLine();
        ibgHeader.Append($"DEVICE_DRIVELETTER={devicePath}").AppendLine();
        ibgHeader.Append($"DEVICE_BUSTYPE={ibgBusType}").AppendLine();
        ibgHeader.AppendLine();

        ibgHeader.Append($"MEDIA_TYPE={_ibgMediaType}").AppendLine();
        ibgHeader.AppendLine("MEDIA_BOOKTYPE=Unknown");
        ibgHeader.AppendLine("MEDIA_ID=N/A");
        ibgHeader.AppendLine("MEDIA_TRACKPATH=PTP");
        ibgHeader.AppendLine("MEDIA_SPEEDS=N/A");
        ibgHeader.Append($"MEDIA_CAPACITY={blocks}").AppendLine();
        ibgHeader.AppendLine("MEDIA_LAYER_BREAK=0");
        ibgHeader.AppendLine();
        ibgHeader.AppendLine("DATA_IMAGEFILE=/dev/null");
        ibgHeader.Append($"DATA_SECTORS={blocks}").AppendLine();
        ibgHeader.Append($"DATA_TYPE=MODE1/{blockSize}").AppendLine();
        ibgHeader.AppendLine("DATA_VOLUMEIDENTIFIER=");
        ibgHeader.AppendLine();
        ibgHeader.Append(_ibgCulture, $"VERIFY_SPEED_START={_ibgStartSpeed:0.00}").AppendLine();
        ibgHeader.Append(_ibgCulture, $"VERIFY_SPEED_END={currentSpeed / _ibgDivider:0.00}").AppendLine();

        ibgHeader.Append(_ibgCulture, $"VERIFY_SPEED_AVERAGE={averageSpeed / _ibgDivider:0.00}").AppendLine();

        ibgHeader.Append(_ibgCulture, $"VERIFY_SPEED_MAX={_ibgMaxSpeed:0.00}").AppendLine();
        ibgHeader.Append(_ibgCulture, $"VERIFY_TIME_TAKEN={Math.Floor(totalSeconds):0}").AppendLine();
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