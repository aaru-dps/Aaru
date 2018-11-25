// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PCMCIA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from PCMCIA devices.
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
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report
{
    /// <summary>
    ///     Implements creating a report for a PCMCIA device
    /// </summary>
    static class Pcmcia
    {
        /// <summary>
        ///     Fills a device report with parameters specific to a PCMCIA device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        internal static void Report(Device dev, ref DeviceReportV2 report)
        {
            report.PCMCIA = new CommonTypes.Metadata.Pcmcia {CIS = dev.Cis};
            Tuple[] tuples = CIS.GetTuples(dev.Cis);
            if(tuples == null) return;

            foreach(Tuple tuple in tuples)
                switch(tuple.Code)
                {
                    case TupleCodes.CISTPL_MANFID:
                        ManufacturerIdentificationTuple manfid = CIS.DecodeManufacturerIdentificationTuple(tuple);

                        if(manfid != null)
                        {
                            report.PCMCIA.ManufacturerCode = manfid.ManufacturerID;
                            report.PCMCIA.CardCode         = manfid.CardID;
                        }

                        break;
                    case TupleCodes.CISTPL_VERS_1:
                        Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                        if(vers != null)
                        {
                            report.PCMCIA.Manufacturer          = vers.Manufacturer;
                            report.PCMCIA.ProductName           = vers.Product;
                            report.PCMCIA.Compliance            = $"{vers.MajorVersion}.{vers.MinorVersion}";
                            report.PCMCIA.AdditionalInformation = vers.AdditionalInformation;
                        }

                        break;
                }
        }
    }
}