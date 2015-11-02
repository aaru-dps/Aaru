// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiscStructureCapabilities.cs
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

namespace DiscImageChef.Decoders.SCSI
{
    public static class DiscStructureCapabilities
    {
        public struct Capability
        {
            /// <summary>
            /// READ/SEND DISC STRUCTURE format code
            /// </summary>
            public byte FormatCode;
            /// <summary>
            /// Supported in SEND DISC STRUCTURE
            /// </summary>
            public bool SDS;
            /// <summary>
            /// Supported in READ DISC STRUCTURE
            /// </summary>
            public bool RDS;
        }

        public static Capability[] Decode(byte[] response)
        {
            ushort len = (ushort)((response[0] << 8) + response[1]);

            if (len + 2 != response.Length)
                return null;

            List<Capability> caps = new List<Capability>();

            uint offset = 4;

            while (offset < response.Length)
            {
                Capability cap = new Capability();
                cap.FormatCode = response[offset];
                cap.SDS = (response[offset + 1] & 0x80) == 0x80;
                cap.RDS = (response[offset + 1] & 0x40) == 0x40;
                caps.Add(cap);
                offset += 4;
            }

            return caps.ToArray();
        }
    }
}

