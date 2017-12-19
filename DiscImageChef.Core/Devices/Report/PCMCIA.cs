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

using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report
{
    public static class PCMCIA
    {
        public static void Report(Device dev, ref DeviceReport report, bool debug, ref bool removable)
        {
            report.PCMCIA = new pcmciaType();
            report.PCMCIA.CIS = dev.CIS;
            Tuple[] tuples = CIS.GetTuples(dev.CIS);
            if(tuples != null)
            {
                foreach(Tuple tuple in tuples)
                {
                    if(tuple.Code == TupleCodes.CISTPL_MANFID)
                    {
                        ManufacturerIdentificationTuple manfid = CIS.DecodeManufacturerIdentificationTuple(tuple);

                        if(manfid != null)
                        {
                            report.PCMCIA.ManufacturerCode = manfid.ManufacturerID;
                            report.PCMCIA.CardCode = manfid.CardID;
                            report.PCMCIA.ManufacturerCodeSpecified = true;
                            report.PCMCIA.CardCodeSpecified = true;
                        }
                    }
                    else if(tuple.Code == TupleCodes.CISTPL_VERS_1)
                    {
                        Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                        if(vers != null)
                        {
                            report.PCMCIA.Manufacturer = vers.Manufacturer;
                            report.PCMCIA.ProductName = vers.Product;
                            report.PCMCIA.Compliance = string.Format("{0}.{1}", vers.MajorVersion, vers.MinorVersion);
                            report.PCMCIA.AdditionalInformation = vers.AdditionalInformation;
                        }
                    }
                }
            }
        }
    }
}