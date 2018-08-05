// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SecureDigital and MMC device information.
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

using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class SecureDigital : BaseEntity
    {
        public byte[] CID         { get; set; }
        public byte[] CSD         { get; set; }
        public byte[] OCR         { get; set; }
        public byte[] SCR         { get; set; }
        public byte[] ExtendedCSD { get; set; }

        public static SecureDigital MapSd(mmcsdType oldSd)
        {
            if(oldSd == null) return null;

            return new SecureDigital
            {
                CID         = oldSd.CID,
                CSD         = oldSd.CSD,
                ExtendedCSD = oldSd.ExtendedCSD,
                OCR         = oldSd.OCR,
                SCR         = oldSd.SCR
            };
        }
    }
}