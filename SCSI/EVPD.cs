// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : EVPD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI EVPDs.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;

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
            if(page == null)
                return null;

            if(page[1] != 0)
                return null;

            if(page.Length != page[3] + 4)
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
            if(page == null)
                return null;

            if(page[1] == 0 || page[1] > 0x7F)
                return null;

            if(page.Length != page[3] + 4)
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
            if(page == null)
                return null;

            if(page[1] != 0x80)
                return null;

            if(page.Length != page[3] + 4)
                return null;

            byte[] ascii = new byte[page.Length - 4];

            Array.Copy(page, 4, ascii, 0, page.Length - 4);

            return StringHandlers.CToString(ascii);
        }

        /// <summary>
        /// Decodes VPD page 0x82: ASCII implemented operating definition
        /// </summary>
        /// <returns>ASCII implemented operating definition.</returns>
        /// <param name="page">Page 0x82.</param>
        public static string DecodePage82(byte[] page)
        {
            if(page == null)
                return null;

            if(page[1] != 0x82)
                return null;

            if(page.Length != page[3] + 4)
                return null;

            byte[] ascii = new byte[page.Length - 4];

            Array.Copy(page, 4, ascii, 0, page.Length - 4);

            return StringHandlers.CToString(ascii);
        }
    }
}

