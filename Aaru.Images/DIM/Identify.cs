// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies DIM disk images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages
{
    public partial class Dim
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < DATA_OFFSET) return false;

            comment = new byte[60];
            hdrId   = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            return headerId.SequenceEqual(hdrId);
        }
    }
}