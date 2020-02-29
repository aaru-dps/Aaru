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
//     Identifies HD-Copy disk images.
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
// Copyright © 2017 Michael Drüing
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class HdCopy
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 2 + (2 * 82))
                return false;

            byte[] header = new byte[2 + (2 * 82)];
            stream.Read(header, 0, 2 + (2 * 82));

            HdcpFileHeader fheader = Marshal.ByteArrayToStructureLittleEndian<HdcpFileHeader>(header);

            /* Some sanity checks on the values we just read.
             * We know the image is from a DOS floppy disk, so assume
             * some sane cylinder and sectors-per-track count.
             */
            if(fheader.sectorsPerTrack < 8 ||
               fheader.sectorsPerTrack > 40)
                return false;

            if(fheader.lastCylinder < 37 ||
               fheader.lastCylinder >= 82)
                return false;

            // Validate the trackmap. First two tracks need to be present
            if(fheader.trackMap[0] != 1 ||
               fheader.trackMap[1] != 1)
                return false;

            // all other tracks must be either present (=1) or absent (=0)
            for(int i = 0; i < 2 * 82; i++)
                if(fheader.trackMap[i] > 1)
                    return false;

            // TODO: validate the tracks
            // For now, having a valid header should be sufficient.
            return true;
        }
    }
}