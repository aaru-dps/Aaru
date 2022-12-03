// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for DiskDupe DDI disk images.
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
// Copyright © 2021-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class DiskDupe
{
    bool TryReadHeader(Stream stream, ref FileHeader fhdr, ref TrackInfo[] tmap, ref long[] toffsets)
    {
        byte[]     buffer = new byte[6];
        FileHeader fHeader;

        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 256)
            return false;

        // read and check signature
        fHeader.signature = new byte[10];
        stream.EnsureRead(fHeader.signature, 0, 10);

        if(!fHeader.signature.SequenceEqual(_headerMagic))
            return false;

        // read and check disk type byte
        fHeader.diskType = (byte)stream.ReadByte();

        if(fHeader.diskType is < 1 or > 4)
            return false;

        // seek to start of the trackmap
        stream.Seek(TRACKMAP_OFFSET, SeekOrigin.Begin);
        int         numTracks    = _diskTypes[fHeader.diskType].cyl * _diskTypes[fHeader.diskType].hd;
        int         trackLen     = 512 * _diskTypes[fHeader.diskType].spt; // the length of a single track, in bytes
        TrackInfo[] trackMap     = new TrackInfo[numTracks];
        long[]      trackOffsets = new long[numTracks];

        AaruConsole.DebugWriteLine("DiskDupe plugin", Localization.Identified_image_with_CHS_equals_0_1_2,
                                   _diskTypes[fHeader.diskType].cyl, _diskTypes[fHeader.diskType].hd,
                                   _diskTypes[fHeader.diskType].spt);

        // read the trackmap and store the track offsets
        for(int i = 0; i < numTracks; i++)
        {
            stream.EnsureRead(buffer, 0, 6);
            trackMap[i]     = Marshal.ByteArrayToStructureBigEndian<TrackInfo>(buffer);
            trackOffsets[i] = trackLen * trackMap[i].trackNumber;
        }

        fhdr     = fHeader;
        tmap     = trackMap;
        toffsets = trackOffsets;

        return true;
    }
}