// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiEvpd.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI EVPDs from reports.
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
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    public static class ScsiEvpd
    {
        public static void Report(pageType[] pages, string vendor, ref Dictionary<string, string> evpdPages)
        {
            foreach(pageType evpd in pages)
            {
                string decoded;
                if(evpd.page >= 0x01 && evpd.page <= 0x7F) decoded = EVPD.DecodeASCIIPage(evpd.value);
                else if(evpd.page == 0x81) decoded = EVPD.PrettifyPage_81(evpd.value);
                else if(evpd.page == 0x82) decoded = EVPD.DecodePage82(evpd.value);
                else if(evpd.page == 0x83) decoded = EVPD.PrettifyPage_83(evpd.value);
                else if(evpd.page == 0x84) decoded = EVPD.PrettifyPage_84(evpd.value);
                else if(evpd.page == 0x85) decoded = EVPD.PrettifyPage_85(evpd.value);
                else if(evpd.page == 0x86) decoded = EVPD.PrettifyPage_86(evpd.value);
                else if(evpd.page == 0x89) decoded = EVPD.PrettifyPage_89(evpd.value);
                else if(evpd.page == 0xB0) decoded = EVPD.PrettifyPage_B0(evpd.value);
                else if(evpd.page == 0xB2)
                    decoded = string.Format("TapeAlert Supported Flags Bitmap: 0x{0:X16}<br/>",
                                            EVPD.DecodePageB2(evpd.value));
                else if(evpd.page == 0xB4) decoded = EVPD.DecodePageB4(evpd.value);
                else if(evpd.page == 0xC0 && vendor.Trim() == "quantum")
                    decoded = EVPD.PrettifyPage_C0_Quantum(evpd.value);
                else if(evpd.page == 0xC0 && vendor.Trim() == "seagate")
                    decoded = EVPD.PrettifyPage_C0_Seagate(evpd.value);
                else if(evpd.page == 0xC0 && vendor.Trim() == "ibm")
                    decoded = EVPD.PrettifyPage_C0_IBM(evpd.value);
                else if(evpd.page == 0xC1 && vendor.Trim() == "ibm")
                    decoded = EVPD.PrettifyPage_C1_IBM(evpd.value);
                else if((evpd.page == 0xC0 || evpd.page == 0xC1) && vendor.Trim() == "certance")
                    decoded = EVPD.PrettifyPage_C0_C1_Certance(evpd.value);
                else if((evpd.page == 0xC2 || evpd.page == 0xC3 || evpd.page == 0xC4 || evpd.page == 0xC5 ||
                         evpd.page == 0xC6) &&
                        vendor.Trim() == "certance")
                    decoded = EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(evpd.value);
                else if((evpd.page == 0xC0 || evpd.page == 0xC1 || evpd.page == 0xC2 || evpd.page == 0xC3 ||
                         evpd.page == 0xC4 || evpd.page == 0xC5) &&
                        vendor.Trim() == "hp") decoded = EVPD.PrettifyPage_C0_to_C5_HP(evpd.value);
                else if(evpd.page == 0xDF && vendor.Trim() == "certance")
                    decoded = EVPD.PrettifyPage_DF_Certance(evpd.value);
                else decoded = "Undecoded";

                if(!string.IsNullOrEmpty(decoded)) decoded = decoded.Replace("\n", "<br/>");

                evpdPages.Add(string.Format("EVPD page {0:X2}h", evpd.page), decoded);
            }
        }
    }
}