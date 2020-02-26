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
//     Identifies DiscImageChef format disk images.
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
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Helpers;

namespace DiscImageChef.DiscImages
{
    public partial class DiscImageChef
    {
        public bool Identify(IFilter imageFilter)
        {
            imageStream = imageFilter.GetDataForkStream();
            imageStream.Seek(0, SeekOrigin.Begin);

            if(imageStream.Length < Marshal.SizeOf<DicHeader>()) return false;

            structureBytes = new byte[Marshal.SizeOf<DicHeader>()];
            imageStream.Read(structureBytes, 0, structureBytes.Length);
            header = Marshal.ByteArrayToStructureLittleEndian<DicHeader>(structureBytes);

            return header.identifier == DIC_MAGIC && header.imageMajorVersion <= DICF_VERSION;
        }
    }
}