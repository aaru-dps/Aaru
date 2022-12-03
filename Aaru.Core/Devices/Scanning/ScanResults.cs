// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ScanResults.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structure to store scan results.
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

using System.Collections.Generic;

namespace Aaru.Core.Devices.Scanning;

/// <summary>Contains the results of a media scan</summary>
public struct ScanResults
{
    /// <summary>Total time spent scanning</summary>
    public double TotalTime;
    /// <summary>Total time spent by the device processing commands</summary>
    public double ProcessingTime;
    /// <summary>Average scan speed</summary>
    public double AvgSpeed;
    /// <summary>Maximum scan speed burst</summary>
    public double MaxSpeed;
    /// <summary>Minimum scan speed</summary>
    public double MinSpeed;
    /// <summary>Sectors that took less than 3 milliseconds to be processed</summary>
    public ulong A;
    /// <summary>Sectors that took less than 10 milliseconds but more than 3 milliseconds to be processed</summary>
    public ulong B;
    /// <summary>Sectors that took less than 50 milliseconds but more than 10 milliseconds to be processed</summary>
    public ulong C;
    /// <summary>Sectors that took less than 150 milliseconds but more than 50 milliseconds to be processed</summary>
    public ulong D;
    /// <summary>Sectors that took less than 500 milliseconds but more than 150 milliseconds to be processed</summary>
    public ulong E;
    /// <summary>Sectors that took more than 500 milliseconds to be processed</summary>
    public ulong F;
    /// <summary>List of sectors that could not be read</summary>
    public List<ulong> UnreadableSectors;
    /// <summary>Slowest seek</summary>
    public double SeekMax;
    /// <summary>Fastest seek</summary>
    public double SeekMin;
    /// <summary>Total time spent seeking</summary>
    public double SeekTotal;
    /// <summary>How many seeks have been done</summary>
    public int SeekTimes;
    /// <summary>How many blocks were scanned</summary>
    public ulong Blocks;
    /// <summary>How many blocks could not be read</summary>
    public ulong Errored;
}