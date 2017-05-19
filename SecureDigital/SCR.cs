// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SCR.cs
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
    public class SCR
    {
        public byte Structure;
        public byte Spec;
        public bool DataStatusAfterErase;
        public byte Security;
        public byte BusWidth;
        public bool Spec3;
        public byte ExtendedSecurity;
        public bool Spec4;
        public byte SpecX;
        public byte CommandSupport;
        public byte[] ManufacturerReserved;
    }

    public partial class Decoders
    {
        public static SCR DecodeSCR(uint[] response)
        {
            if(response == null)
                return null;

            if(response.Length != 2)
                return null;

            byte[] data = new byte[8];
            byte[] tmp = new byte[4];

            tmp = BitConverter.GetBytes(response[0]);
            Array.Copy(tmp, 0, data, 0, 4);
            tmp = BitConverter.GetBytes(response[1]);
            Array.Copy(tmp, 0, data, 4, 4);

            return DecodeSCR(data);
        }

        public static SCR DecodeSCR(byte[] response)
        {
            if(response == null)
                return null;

            if(response.Length != 8)
                return null;

            SCR scr = new SCR();
            scr.Structure = (byte)((response[0] & 0xF0) >> 4);
            scr.Spec = (byte)(response[0] & 0x0F);
            scr.DataStatusAfterErase = (response[1] & 0x80) == 0x80;
            scr.Security = (byte)((response[1] & 0x70) >> 4);
            scr.BusWidth = (byte)(response[1] & 0x0F);
            scr.Spec3 = (response[2] & 0x80) == 0x80;
            scr.ExtendedSecurity = (byte)((response[2] & 0x78) >> 3);
            scr.Spec4 = (response[2] & 0x04) == 0x04;
            scr.SpecX = (byte)(((response[2] & 0x03) << 2) + ((response[3] & 0xC0) >> 6));
            scr.CommandSupport = (byte)(response[3] & 0x0F);
            scr.ManufacturerReserved = new byte[4];
            Array.Copy(response, 4, scr.ManufacturerReserved, 0, 4);

            return scr;
        }

        public static string PrettifySCR(SCR scr)
        {
            if(scr == null)
                return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SecureDigital Device Configuration Register:");

            if(scr.Structure != 0)
                sb.AppendFormat("\tUnknown register version {0}", scr.Structure).AppendLine();

            if(scr.Spec == 0 && scr.Spec3 == false && scr.Spec4 == false && scr.SpecX == 0)
                sb.AppendLine("\tDevice follows SecureDigital Physical Layer Specification version 1.0x");
            else if(scr.Spec == 1 && scr.Spec3 == false && scr.Spec4 == false && scr.SpecX == 0)
                sb.AppendLine("\tDevice follows SecureDigital Physical Layer Specification version 1.10");
            else if(scr.Spec == 2 && scr.Spec3 == false && scr.Spec4 == false && scr.SpecX == 0)
                sb.AppendLine("\tDevice follows SecureDigital Physical Layer Specification version 2.00");
            else if(scr.Spec == 2 && scr.Spec3 == true && scr.Spec4 == false && scr.SpecX == 0)
                sb.AppendLine("\tDevice follows SecureDigital Physical Layer Specification version 3.0x");
            else if(scr.Spec == 2 && scr.Spec3 == true && scr.Spec4 == true && scr.SpecX == 0)
                sb.AppendLine("\tDevice follows SecureDigital Physical Layer Specification version 4.xx");
            else if(scr.Spec == 2 && scr.Spec3 == true && scr.SpecX == 1)
                sb.AppendLine("\tDevice follows SecureDigital Physical Layer Specification version 5.xx");
            else
                sb.AppendFormat("\tDevice follows SecureDigital Physical Layer Specification with unknown version {0}.{1}.{2}.{3}",
                                scr.Spec, scr.Spec3, scr.Spec4, scr.SpecX).AppendLine();
            switch(scr.Security)
            {
                case 0:
                    sb.AppendLine("\tDevice does not support CPRM");
                    break;
                case 1:
                    sb.AppendLine("\tDevice does not use CPRM");
                    break;
                case 2:
                    sb.AppendLine("\tDevice uses CPRM according to specification version 1.01");
                    break;
                case 3:
                    sb.AppendLine("\tDevice uses CPRM according to specification version 2.00");
                    break;
                case 4:
                    sb.AppendLine("\tDevice uses CPRM according to specification version 3.xx");
                    break;
                default:
                    sb.AppendFormat("\tDevice uses unknown CPRM specification with code {0}", scr.Security).AppendLine();
                    break;
            }

            if((scr.BusWidth & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports 1-bit data bus");
            if((scr.BusWidth & 0x04) == 0x04)
                sb.AppendLine("\tDevice supports 4-bit data bus");

            if(scr.ExtendedSecurity != 0)
                sb.AppendLine("\tDevice supports extended security");

            if((scr.CommandSupport & 0x08) == 0x08)
                sb.AppendLine("\tDevice supports extension register multi-block commands");
            if((scr.CommandSupport & 0x04) == 0x04)
                sb.AppendLine("\tDevice supports extension register single-block commands");
            if((scr.CommandSupport & 0x02) == 0x02)
                sb.AppendLine("\tDevice supports set block count command");
            if((scr.CommandSupport & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports speed class control command");

            return sb.ToString();
        }

        public static string PrettifySCR(uint[] response)
        {
            return PrettifySCR(DecodeSCR(response));
        }

        public static string PrettifySCR(byte[] response)
        {
            return PrettifySCR(DecodeSCR(response));
        }
    }
}
