// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockLimits.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI SSC block limits structures.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.SCSI.SSC;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class BlockLimits
{
    public static BlockLimitsData? Decode(byte[] response)
    {
        if(response?.Length != 6)
            return null;

        return new BlockLimitsData
        {
            granularity = (byte)(response[0] & 0x1F),
            maxBlockLen = (uint)((response[1]   << 16) + (response[2] << 8) + response[3]),
            minBlockLen = (ushort)((response[4] << 8)  + response[5])
        };
    }

    public static string Prettify(BlockLimitsData? decoded)
    {
        if(decoded == null)
            return null;

        var sb = new StringBuilder();

        if(decoded.Value.maxBlockLen == decoded.Value.minBlockLen)
            sb.AppendFormat("Device's block size is fixed at {0} bytes", decoded.Value.minBlockLen).AppendLine();
        else
        {
            if(decoded.Value.maxBlockLen > 0)
                sb.AppendFormat("Device's maximum block size is {0} bytes", decoded.Value.maxBlockLen).AppendLine();
            else
                sb.AppendLine("Device does not specify a maximum block size");

            sb.AppendFormat("Device's minimum block size is {0} bytes", decoded.Value.minBlockLen).AppendLine();

            if(decoded.Value.granularity > 0)
                sb.AppendFormat("Device's needs a block size granularity of 2^{0} ({1}) bytes",
                                decoded.Value.granularity, Math.Pow(2, decoded.Value.granularity)).AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] response) => Prettify(Decode(response));

    public struct BlockLimitsData
    {
        /// <summary>All blocks size must be multiple of 2^<cref name="granularity" /></summary>
        public byte granularity;
        /// <summary>Maximum block length in bytes</summary>
        public uint maxBlockLen;
        /// <summary>Minimum block length in bytes</summary>
        public ushort minBlockLen;
    }
}