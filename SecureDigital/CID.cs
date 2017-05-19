// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CID.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;

namespace DiscImageChef.Decoders.SecureDigital
{
    public class CID
    {
        public byte Manufacturer;
        public string ApplicationID;
        public string ProductName;
        public byte ProductRevision;
        public uint ProductSerialNumber;
        public ushort ManufacturingDate;
        public byte CRC;
    }

    public partial class Decoders
    {
        public static CID DecodeCID(uint[] response)
        {
            if(response == null)
                return null;

            if(response.Length != 4)
                return null;

            byte[] data = new byte[16];
            byte[] tmp = new byte[4];

            tmp = BitConverter.GetBytes(response[0]);
            Array.Copy(tmp, 0, data, 0, 4);
            tmp = BitConverter.GetBytes(response[1]);
            Array.Copy(tmp, 0, data, 4, 4);
            tmp = BitConverter.GetBytes(response[2]);
            Array.Copy(tmp, 0, data, 8, 4);
            tmp = BitConverter.GetBytes(response[3]);
            Array.Copy(tmp, 0, data, 12, 4);

            return DecodeCID(data);
        }

        public static CID DecodeCID(byte[] response)
        {
            if(response == null)
                return null;

            if(response.Length != 16)
                return null;

            byte[] tmp;

            CID cid = new CID();
            cid.Manufacturer = response[0];
            tmp = new byte[2];
            Array.Copy(response, 1, tmp, 0, 2);
            cid.ApplicationID = StringHandlers.CToString(tmp);
            tmp = new byte[5];
            Array.Copy(response, 3, tmp, 0, 5);
            cid.ProductName = StringHandlers.CToString(tmp);
            cid.ProductRevision = response[8];
            cid.ProductSerialNumber = BitConverter.ToUInt32(response, 9);
            cid.ManufacturingDate = (ushort)(((response[13] & 0x0F) << 4) + response[14]);
            cid.CRC = (byte)((response[15] & 0xFE) >> 1);

            return cid;
        }

        public static string PrettifyCID(CID cid)
        {
            if(cid == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SecureDigital Device Identification Register:");
            sb.AppendFormat("\tManufacturer: {0}", VendorString.Prettify(cid.Manufacturer)).AppendLine();
            sb.AppendFormat("\tApplication ID: {0}", cid.ApplicationID).AppendLine();
            sb.AppendFormat("\tProduct name: {0}", cid.ProductName).AppendLine();
            sb.AppendFormat("\tProduct revision: {0:X2}.{1:X2}", (cid.ProductRevision & 0xF0) >> 4, cid.ProductRevision & 0x0F).AppendLine();
            sb.AppendFormat("\tProduct serial number: {0}", cid.ProductSerialNumber).AppendLine();
            sb.AppendFormat("\tDevice manufactured month {0} of {1}", (cid.ManufacturingDate & 0xF00) >> 8, (cid.ManufacturingDate & 0xFF) + 2000).AppendLine();
            sb.AppendFormat("\tCID CRC: 0x{0:X2}", cid.CRC).AppendLine();

            return sb.ToString();
        }

        public static string PrettifyCID(uint[] response)
        {
            return PrettifyCID(DecodeCID(response));
        }

        public static string PrettifyCID(byte[] response)
        {
            return PrettifyCID(DecodeCID(response));
        }
    }
}
