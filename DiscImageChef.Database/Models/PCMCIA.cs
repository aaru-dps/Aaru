// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PCMCIA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for PCMCIA device information.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class PCMCIA : BaseEntity
    {
        public byte[]            CIS                   { get; set; }
        public string            Compliance            { get; set; }
        public ushort            ManufacturerCode      { get; set; }
        public ushort            CardCode              { get; set; }
        public string            Manufacturer          { get; set; }
        public string            ProductName           { get; set; }
        public List<StringClass> AdditionalInformation { get; set; }

        public static PCMCIA MapPcmcia(pcmciaType oldPcmcia)
        {
            if(oldPcmcia == null) return null;

            PCMCIA newPcmcia = new PCMCIA
            {
                CIS          = oldPcmcia.CIS,
                Compliance   = oldPcmcia.Compliance,
                Manufacturer = oldPcmcia.Manufacturer,
                ProductName  = oldPcmcia.ProductName
            };
            if(oldPcmcia.ManufacturerCodeSpecified) newPcmcia.ManufacturerCode = oldPcmcia.ManufacturerCode;
            if(oldPcmcia.CardCodeSpecified) newPcmcia.CardCode                 = oldPcmcia.CardCode;
            if(oldPcmcia.AdditionalInformation == null) return newPcmcia;

            if(oldPcmcia.AdditionalInformation == null) return newPcmcia;

            newPcmcia.AdditionalInformation =
                new List<StringClass>(oldPcmcia.AdditionalInformation.Select(t => new StringClass {Value = t}));

            return newPcmcia;
        }
    }
}