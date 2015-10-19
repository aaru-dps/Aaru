// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : EVPD.cs
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
using DiscImageChef;

namespace DiscImageChef.Decoders.SCSI
{
    public static class EVPD
    {
        /// <summary>
        /// Decodes VPD page 0x00: Supported VPD pages
        /// </summary>
        /// <returns>A byte array containing all supported VPD pages.</returns>
        /// <param name="page">Page 0x00.</param>
        public static byte[] DecodePage00(byte[] page)
        {
            if (page == null)
                return null;
            
            if (page[1] != 0)
                return null;

            if (page.Length != page[3] + 4)
                return null;

            byte[] decoded = new byte[page.Length - 4];

            Array.Copy(page, 4, decoded, 0, page.Length - 4);

            return decoded;
        }

        /// <summary>
        /// Decides VPD pages 0x01 to 0x7F: ASCII Information
        /// </summary>
        /// <returns>An ASCII string with the contents of the page.</returns>
        /// <param name="page">Page 0x01-0x7F.</param>
        public static string DecodeASCIIPage(byte[] page)
        {
            if (page == null)
                return null;

            if (page[1] == 0 || page[1] > 0x7F)
                return null;

            if (page.Length != page[3] + 4)
                return null;

            byte[] ascii = new byte[page[4]];

            Array.Copy(page, 5, ascii, 0, page[4]);

            return StringHandlers.CToString(ascii);
        }

        /// <summary>
        /// Decodes VPD page 0x80: Unit Serial Number
        /// </summary>
        /// <returns>The unit serial number.</returns>
        /// <param name="page">Page 0x80.</param>
        public static string DecodePage80(byte[] page)
        {
            if (page == null)
                return null;

            if (page[1] != 0x80)
                return null;

            if (page.Length != page[3] + 4)
                return null;

            byte[] ascii = new byte[page.Length - 4];

            Array.Copy(page, 4, ascii, 0, page.Length - 4);

            return StringHandlers.CToString(ascii);
        }
    }
}

