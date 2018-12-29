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
//     Identifies Anex86 disk images.
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
    public partial class SaveDskF
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 41) return false;

            byte[] hdr = new byte[40];
            stream.Read(hdr, 0, 40);

            header = new SaveDskFHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(40);
            Marshal.Copy(hdr, 0, hdrPtr, 40);
            header = (SaveDskFHeader)Marshal.PtrToStructure(hdrPtr, typeof(SaveDskFHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return (header.magic == SDF_MAGIC || header.magic == SDF_MAGIC_COMPRESSED ||
                    header.magic == SDF_MAGIC_OLD) && header.fatCopies <= 2            && header.padding == 0 &&
                   header.commentOffset                                < stream.Length &&
                   header.dataOffset                                   < stream.Length;
        }
    }
}