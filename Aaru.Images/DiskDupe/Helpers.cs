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
// Copyright © 2021-2022 Michael Drüing
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class DiskDupe
    {
        bool TryReadHeader(Stream stream, ref FileHeader fhdr, ref TrackInfo[] tmap, ref long[] toffsets)
        {
            int         numTracks;
            int         trackLen; // the length of a single track, in bytes
            TrackInfo[] trackMap;
            byte[]      buffer = new byte[6];
            FileHeader  fHeader;
            long[]      trackOffsets;

            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 256)
                return false;

            // read and check signature
            fHeader.signature = new byte[10];
            stream.Read(fHeader.signature, 0, 10);

            if(!fHeader.signature.SequenceEqual(_headerMagic))
                return false;

            // read and check disk type byte
            fHeader.diskType = (byte)stream.ReadByte();

            if(fHeader.diskType < 1 ||
               fHeader.diskType > 4)
                return false;

            // seek to start of the trackmap
            stream.Seek(TRACKMAP_OFFSET, SeekOrigin.Begin);
            numTracks    = _diskTypes[fHeader.diskType].cyl * _diskTypes[fHeader.diskType].hd;
            trackLen     = 512                              * _diskTypes[fHeader.diskType].spt;
            trackMap     = new TrackInfo[numTracks];
            trackOffsets = new long[numTracks];

            AaruConsole.DebugWriteLine("DiskDupe plugin", "Identified image with C/H/S = {0}/{1}/{2}",
                                       _diskTypes[fHeader.diskType].cyl, _diskTypes[fHeader.diskType].hd,
                                       _diskTypes[fHeader.diskType].spt);

            // read the trackmap and store the track offsets
            for(int i = 0; i < numTracks; i++)
            {
                stream.Read(buffer, 0, 6);
                trackMap[i]     = Marshal.ByteArrayToStructureBigEndian<TrackInfo>(buffer);
                trackOffsets[i] = trackLen * trackMap[i].trackNumber;
            }

            fhdr     = fHeader;
            tmap     = trackMap;
            toffsets = trackOffsets;

            return true;
        }
    }
}