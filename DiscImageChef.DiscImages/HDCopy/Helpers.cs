// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for HD-Copy disk images.
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

using System;
using System.IO;

namespace DiscImageChef.DiscImages
{
    public partial class HdCopy
    {
        void ReadTrackIntoCache(Stream stream, int tracknum)
        {
            byte[] trackData = new byte[imageInfo.SectorSize * imageInfo.SectorsPerTrack];
            byte[] blkHeader = new byte[3];

            // check that track is present
            if(trackOffset[tracknum] == -1)
                throw new InvalidDataException("Tried reading a track that is not present in image");

            stream.Seek(trackOffset[tracknum], SeekOrigin.Begin);

            // read the compressed track data
            stream.Read(blkHeader, 0, 3);
            short compressedLength = (short)(BitConverter.ToInt16(blkHeader, 0) - 1);
            byte  escapeByte       = blkHeader[2];

            byte[] cBuffer = new byte[compressedLength];
            stream.Read(cBuffer, 0, compressedLength);

            // decompress the data
            int sIndex = 0; // source buffer position
            int dIndex = 0; // destination buffer position
            while(sIndex < compressedLength)
                if(cBuffer[sIndex] == escapeByte)
                {
                    sIndex++; // skip over escape byte
                    byte fillByte  = cBuffer[sIndex++];
                    byte fillCount = cBuffer[sIndex++];
                    // fill destination buffer
                    for(int i = 0; i < fillCount; i++) trackData[dIndex++] = fillByte;
                }
                else
                    trackData[dIndex++] = cBuffer[sIndex++];

            // check that the number of bytes decompressed matches a whole track
            if(dIndex != imageInfo.SectorSize * imageInfo.SectorsPerTrack)
                throw new InvalidDataException("Track decompression yielded incomplete data");

            // store track in cache
            trackCache[tracknum] = trackData;
        }
    }
}