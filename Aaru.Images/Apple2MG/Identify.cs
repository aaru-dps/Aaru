// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies XGS emulator disk images.
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

using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Apple2Mg
{
    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 65)
            return false;

        byte[] header = new byte[64];
        stream.Read(header, 0, 64);

        Header hdr = Marshal.SpanToStructureLittleEndian<Header>(header);

        if(hdr.Magic != MAGIC)
            return false;

        if(hdr.DataOffset > stream.Length)
            return false;

        // There seems to be incorrect endian in some images on the wild
        if(hdr.DataSize == 0x00800C00)
            hdr.DataSize = 0x000C8000;

        if(hdr.DataOffset + hdr.DataSize > stream.Length)
            return false;

        if(hdr.CommentOffset > stream.Length)
            return false;

        if(hdr.CommentOffset + hdr.CommentSize > stream.Length)
            return false;

        if(hdr.CreatorSpecificOffset > stream.Length)
            return false;

        return hdr.CreatorSpecificOffset + hdr.CreatorSpecificSize <= stream.Length;
    }
}