// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiscStructureCapabilities.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI DISC STRUCTURE structures.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class DiscStructureCapabilities
{
    public static Capability[] Decode(byte[] response)
    {
        ushort len = (ushort)((response[0] << 8) + response[1]);

        if(len + 2 != response.Length)
            return null;

        List<Capability> caps = new();

        uint offset = 4;

        while(offset < response.Length)
        {
            var cap = new Capability
            {
                FormatCode = response[offset],
                SDS        = (response[offset + 1] & 0x80) == 0x80,
                RDS        = (response[offset + 1] & 0x40) == 0x40
            };

            caps.Add(cap);
            offset += 4;
        }

        return caps.ToArray();
    }

    public struct Capability
    {
        /// <summary>READ/SEND DISC STRUCTURE format code</summary>
        public byte FormatCode;
        /// <summary>Supported in SEND DISC STRUCTURE</summary>
        public bool SDS;
        /// <summary>Supported in READ DISC STRUCTURE</summary>
        public bool RDS;
    }
}