// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATAPI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from ATAPI devices.
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

using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report
{
    /// <summary>
    ///     Implements creating a report for an ATAPI device
    /// </summary>
    static class Atapi
    {
        /// <summary>
        ///     Fills a SCSI device report with parameters specific to an ATAPI device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        /// <param name="debug">If debug is enabled</param>
        internal static void Report(Device dev, ref DeviceReportV2 report, bool debug)
        {
            if(report == null) return;

            const uint TIMEOUT = 5;

            DicConsole.WriteLine("Querying ATAPI IDENTIFY...");

            dev.AtapiIdentify(out byte[] buffer, out _, TIMEOUT, out _);

            if(!Identify.Decode(buffer).HasValue) return;

            Identify.IdentifyDevice? atapiIdNullable = Identify.Decode(buffer);
            if(atapiIdNullable != null) report.ATAPI = new CommonTypes.Metadata.Ata {IdentifyDevice = atapiIdNullable};

            if(debug) report.ATAPI.Identify = buffer;
        }
    }
}