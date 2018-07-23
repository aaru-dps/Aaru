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
//     Identifies Parallels disk images.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.DiscImages
{
    public partial class Parallels
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (ParallelsHeader)Marshal.PtrToStructure(headerPtr, typeof(ParallelsHeader));
            Marshal.FreeHGlobal(headerPtr);

            return parallelsMagic.SequenceEqual(pHdr.magic) || parallelsExtMagic.SequenceEqual(pHdr.magic);
        }
    }
}