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
//     Identifies CisCopy disk images.
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

public sealed partial class CisCopy
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        var  type = (DiskType)stream.ReadByte();
        byte tracks;

        switch(type)
        {
            case DiskType.MD1DD8:
            case DiskType.MD1DD:
            case DiskType.MD2DD8:
            case DiskType.MD2DD:
                tracks = 80;

                break;
            case DiskType.MF2DD:
            case DiskType.MD2HD:
            case DiskType.MF2HD:
                tracks = 160;

                break;
            default:
                return false;
        }

        var trackBytes = new byte[tracks];
        stream.EnsureRead(trackBytes, 0, tracks);

        for(var i = 0; i < tracks; i++)
        {
            if(trackBytes[i] != (byte)TrackType.Copied  &&
               trackBytes[i] != (byte)TrackType.Omitted &&
               trackBytes[i] != (byte)TrackType.OmittedAlternate)
                return false;
        }

        var cmpr = (Compression)stream.ReadByte();

        if(cmpr != Compression.None && cmpr != Compression.Normal && cmpr != Compression.High) return false;

        switch(type)
        {
            case DiskType.MD1DD8:
                if(stream.Length > 40 * 1 * 8 * 512 + 82) return false;

                break;
            case DiskType.MD1DD:
                if(stream.Length > 40 * 1 * 9 * 512 + 82) return false;

                break;
            case DiskType.MD2DD8:
                if(stream.Length > 40 * 2 * 8 * 512 + 82) return false;

                break;
            case DiskType.MD2DD:
                if(stream.Length > 40 * 2 * 9 * 512 + 82) return false;

                break;
            case DiskType.MF2DD:
                if(stream.Length > 80 * 2 * 9 * 512 + 162) return false;

                break;
            case DiskType.MD2HD:
                if(stream.Length > 80 * 2 * 15 * 512 + 162) return false;

                break;
            case DiskType.MF2HD:
                if(stream.Length > 80 * 2 * 18 * 512 + 162) return false;

                break;
        }

        return true;
    }

#endregion
}