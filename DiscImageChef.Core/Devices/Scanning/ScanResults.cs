// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;

namespace DiscImageChef.Core.Devices.Scanning
{
    public struct ScanResults
    {
        public double TotalTime;
        public double ProcessingTime;
        public double AvgSpeed;
        public double MaxSpeed;
        public double MinSpeed;
        public ulong A;
        public ulong B;
        public ulong C;
        public ulong D;
        public ulong E;
        public ulong F;
        public List<ulong> UnreadableSectors;
        public double SeekMax;
        public double SeekMin;
        public double SeekTotal;
        public int SeekTimes;
        public ulong Blocks;
        public ulong Errored;
    }
}