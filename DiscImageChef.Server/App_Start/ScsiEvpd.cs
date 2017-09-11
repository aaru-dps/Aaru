// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiEvpd.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
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
                if(evpd.page >= 0x01 && evpd.page <= 0x7F)
                    decoded = Decoders.SCSI.EVPD.DecodeASCIIPage(evpd.value);
                else if(evpd.page == 0x81)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_81(evpd.value);
                else if(evpd.page == 0x82)
                    decoded = Decoders.SCSI.EVPD.DecodePage82(evpd.value);
                else if(evpd.page == 0x83)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_83(evpd.value);
                else if(evpd.page == 0x84)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_84(evpd.value);
                else if(evpd.page == 0x85)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_85(evpd.value);
                else if(evpd.page == 0x86)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_86(evpd.value);
                else if(evpd.page == 0x89)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_89(evpd.value);
                else if(evpd.page == 0xB0)
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_B0(evpd.value);
                else if(evpd.page == 0xB2)
                    decoded = string.Format("TapeAlert Supported Flags Bitmap: 0x{0:X16}<br/>", Decoders.SCSI.EVPD.DecodePageB2(evpd.value));
                else if(evpd.page == 0xB4)
                    decoded = Decoders.SCSI.EVPD.DecodePageB4(evpd.value);
                else if(evpd.page == 0xC0 && vendor.Trim() == "quantum")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C0_Quantum(evpd.value);
                else if(evpd.page == 0xC0 && vendor.Trim() == "seagate")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C0_Seagate(evpd.value);
                else if(evpd.page == 0xC0 && vendor.Trim() == "ibm")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C0_IBM(evpd.value);
                else if(evpd.page == 0xC1 && vendor.Trim() == "ibm")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C1_IBM(evpd.value);
                else if((evpd.page == 0xC0 || evpd.page == 0xC1) && vendor.Trim() == "certance")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C0_C1_Certance(evpd.value);
                else if((evpd.page == 0xC2 || evpd.page == 0xC3 || evpd.page == 0xC4 || evpd.page == 0xC5 || evpd.page == 0xC6) &&
                        vendor.Trim() == "certance")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(evpd.value);
                else if((evpd.page == 0xC0 || evpd.page == 0xC1 || evpd.page == 0xC2 || evpd.page == 0xC3 || evpd.page == 0xC4 || evpd.page == 0xC5) &&
                        vendor.Trim() == "hp")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_C0_to_C5_HP(evpd.value);
                else if(evpd.page == 0xDF && vendor.Trim() == "certance")
                    decoded = Decoders.SCSI.EVPD.PrettifyPage_DF_Certance(evpd.value);
                else
                    decoded = "Undecoded";

                if(!string.IsNullOrEmpty(decoded))
                    decoded = decoded.Replace("\n", "<br/>");
                
                evpdPages.Add(string.Format("EVPD page {0:X2}h", evpd.page), decoded);
            }
        }
    }
}
