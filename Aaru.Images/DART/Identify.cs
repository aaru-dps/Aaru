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
//     Identifies Apple Disk Archival/Retrieval Tool format.
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

using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Dart
{
#region IMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < 84) return false;

        stream.Seek(0, SeekOrigin.Begin);
        var headerB = new byte[Marshal.SizeOf<Header>()];

        stream.EnsureRead(headerB, 0, Marshal.SizeOf<Header>());
        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(headerB);

        if(header.srcCmp > COMPRESS_NONE) return false;

        int expectedMaxSize = 84 + header.srcSize * 2 * 524;

        switch(header.srcType)
        {
            case DISK_MAC:
                if(header.srcSize != SIZE_MAC_SS && header.srcSize != SIZE_MAC) return false;

                break;
            case DISK_LISA:
                if(header.srcSize != SIZE_LISA) return false;

                break;
            case DISK_APPLE2:
                if(header.srcSize != SIZE_APPLE2) return false;

                break;
            case DISK_MAC_HD:
                if(header.srcSize != SIZE_MAC_HD) return false;

                expectedMaxSize += 64;

                break;
            case DISK_DOS:
                if(header.srcSize != SIZE_DOS) return false;

                break;
            case DISK_DOS_HD:
                if(header.srcSize != SIZE_DOS_HD) return false;

                expectedMaxSize += 64;

                break;
            default:
                return false;
        }

        return stream.Length <= expectedMaxSize;
    }

#endregion
}