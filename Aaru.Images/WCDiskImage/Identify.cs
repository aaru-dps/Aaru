// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies d2f/WC DISK IMAGE disk images.
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
// Copyright © 2018-2019 Michael Drüing
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class WCDiskImage
{
    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 32)
            return false;

        byte[] header = new byte[32];
        stream.Read(header, 0, 32);

        FileHeader fheader = Marshal.ByteArrayToStructureLittleEndian<FileHeader>(header);

        /* check the signature */
        if(Encoding.ASCII.GetString(fheader.signature).TrimEnd('\x00') != FILE_SIGNATURE)
            return false;

        /* Some sanity checks on the values we just read. */
        if(fheader.version > 1)
            return false;

        if(fheader.heads < 1 ||
           fheader.heads > 2)
            return false;

        if(fheader.sectorsPerTrack < 8 ||
           fheader.sectorsPerTrack > 18)
            return false;

        if(fheader.cylinders < 1 ||
           fheader.cylinders > 80)
            return false;

        if(fheader.extraTracks[0] > 1 ||
           fheader.extraTracks[1] > 1 ||
           fheader.extraTracks[2] > 1 ||
           fheader.extraTracks[3] > 1)
            return false;

        // TODO: validate all sectors
        // For now, having a valid header will suffice.
        return ((byte)fheader.extraFlags & ~0x03) == 0;
    }
}