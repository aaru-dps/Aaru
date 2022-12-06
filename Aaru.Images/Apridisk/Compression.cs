// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Compression.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains compression algorithm for Apridisk disk images.
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

using System;
using System.IO;

namespace Aaru.DiscImages
{
    public sealed partial class Apridisk
    {
        static uint Decompress(byte[] compressed, out byte[] decompressed)
        {
            int readp  = 0;
            int cLen   = compressed.Length;
            var buffer = new MemoryStream();

            uint uLen = 0;

            while(cLen >= 3)
            {
                ushort blklen = BitConverter.ToUInt16(compressed, readp);
                readp += 2;

                for(int i = 0; i < blklen; i++)
                    buffer.WriteByte(compressed[readp]);

                uLen += blklen;
                readp++;
                cLen -= 3;
            }

            decompressed = buffer.ToArray();

            return uLen;
        }
    }
}