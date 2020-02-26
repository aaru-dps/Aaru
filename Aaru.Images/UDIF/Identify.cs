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
//     Identifies Apple Universal Disk Image Format.
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
    public partial class Udif
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 512) return false;

            stream.Seek(-Marshal.SizeOf<UdifFooter>(), SeekOrigin.End);
            byte[] footerB = new byte[Marshal.SizeOf<UdifFooter>()];

            stream.Read(footerB, 0, Marshal.SizeOf<UdifFooter>());
            footer = Marshal.ByteArrayToStructureBigEndian<UdifFooter>(footerB);

            if(footer.signature == UDIF_SIGNATURE) return true;

            // Old UDIF as created by DiskCopy 6.5 using "OBSOLETE" format. (DiskCopy 5 rumored format?)
            stream.Seek(0, SeekOrigin.Begin);
            byte[] headerB = new byte[Marshal.SizeOf<UdifFooter>()];

            stream.Read(headerB, 0, Marshal.SizeOf<UdifFooter>());
            footer = Marshal.ByteArrayToStructureBigEndian<UdifFooter>(headerB);

            return footer.signature == UDIF_SIGNATURE;
        }
    }
}