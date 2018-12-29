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
//     Identifies KryoFlux STREAM images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.DiscImages
{
    public partial class KryoFlux
    {
        public bool Identify(IFilter imageFilter)
        {
            OobBlock header = new OobBlock();
            Stream   stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < Marshal.SizeOf(header)) return false;

            byte[] hdr = new byte[Marshal.SizeOf(header)];
            stream.Read(hdr, 0, Marshal.SizeOf(header));

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.Copy(hdr, 0, hdrPtr, Marshal.SizeOf(header));
            header = (OobBlock)Marshal.PtrToStructure(hdrPtr, typeof(OobBlock));
            Marshal.FreeHGlobal(hdrPtr);

            OobBlock footer = new OobBlock();
            stream.Seek(-Marshal.SizeOf(footer), SeekOrigin.End);

            hdr = new byte[Marshal.SizeOf(footer)];
            stream.Read(hdr, 0, Marshal.SizeOf(footer));

            hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(footer));
            Marshal.Copy(hdr, 0, hdrPtr, Marshal.SizeOf(footer));
            footer = (OobBlock)Marshal.PtrToStructure(hdrPtr, typeof(OobBlock));
            Marshal.FreeHGlobal(hdrPtr);

            return header.blockId == BlockIds.Oob && header.blockType == OobTypes.KFInfo &&
                   footer.blockId == BlockIds.Oob && footer.blockType == OobTypes.EOF    && footer.length == 0x0D0D;
        }
    }
}