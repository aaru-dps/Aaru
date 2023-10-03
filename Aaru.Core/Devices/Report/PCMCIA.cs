// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Metadata;
using Aaru.Decoders.PCMCIA;

namespace Aaru.Core.Devices.Report;

/// <summary>Implements creating a report for a PCMCIA device</summary>
public sealed partial class DeviceReport
{
    /// <summary>Fills a device report with parameters specific to a PCMCIA device</summary>
    public Pcmcia PcmciaReport()
    {
        var pcmciaReport = new Pcmcia
        {
            CIS = _dev.Cis
        };

        Tuple[] tuples = CIS.GetTuples(_dev.Cis);

        if(tuples == null)
            return pcmciaReport;

        foreach(Tuple tuple in tuples)
        {
            switch(tuple.Code)
            {
                case TupleCodes.CISTPL_MANFID:
                    ManufacturerIdentificationTuple manfid = CIS.DecodeManufacturerIdentificationTuple(tuple);

                    if(manfid != null)
                    {
                        pcmciaReport.ManufacturerCode = manfid.ManufacturerID;
                        pcmciaReport.CardCode         = manfid.CardID;
                    }

                    break;
                case TupleCodes.CISTPL_VERS_1:
                    Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                    if(vers != null)
                    {
                        pcmciaReport.Manufacturer          = vers.Manufacturer;
                        pcmciaReport.ProductName           = vers.Product;
                        pcmciaReport.Compliance            = $"{vers.MajorVersion}.{vers.MinorVersion}";
                        pcmciaReport.AdditionalInformation = vers.AdditionalInformation;
                    }

                    break;
            }
        }

        return pcmciaReport;
    }
}