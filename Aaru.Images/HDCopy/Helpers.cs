// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Helpers;

public sealed partial class HdCopy
{
    static bool TryReadHeader(Stream stream, ref FileHeader fhdr, ref long dataStartOffset)
    {
        var numTracks = 82;

        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 16 + 2 * 82)
            return false;

        FileHeader fheader;

        /* assume it's a regular HD-Copy file without the disk name */
        dataStartOffset         = 2 + 2 * numTracks;
        fheader.lastCylinder    = (byte)stream.ReadByte();
        fheader.sectorsPerTrack = (byte)stream.ReadByte();

        if(fheader.lastCylinder    == 0xff &&
           fheader.sectorsPerTrack == 0x18)
        {
            /* This is an "extended" HD-Copy file with filename information and 84 tracks */
            stream.Seek(0x0e, SeekOrigin.Begin);
            fheader.lastCylinder    = (byte)stream.ReadByte();
            fheader.sectorsPerTrack = (byte)stream.ReadByte();
            numTracks               = 84;
            dataStartOffset         = 16 + 2 * numTracks;
        }

        fheader.trackMap = new byte[2 * numTracks];
        stream.EnsureRead(fheader.trackMap, 0, 2 * numTracks);

        /* Some sanity checks on the values we just read.
         * We know the image is from a DOS floppy disk, so assume
         * some sane cylinder and sectors-per-track count.
         */
        if(fheader.sectorsPerTrack is < 8 or > 40)
            return false;

        if(fheader.lastCylinder is < 37 or >= 84)
            return false;

        // Validate the trackmap. First two tracks need to be present
        if(fheader.trackMap[0] != 1 ||
           fheader.trackMap[1] != 1)
            return false;

        // all other tracks must be either present (=1) or absent (=0)
        for(var i = 0; i < 2 * numTracks; i++)
            if(fheader.trackMap[i] > 1)
                return false;

        /* return success */
        fhdr = fheader;

        return true;
    }

    ErrorNumber ReadTrackIntoCache(Stream stream, int trackNum)
    {
        var trackData = new byte[_imageInfo.SectorSize * _imageInfo.SectorsPerTrack];
        var blkHeader = new byte[3];

        // check that track is present
        if(_trackOffset[trackNum] == -1)
        {
            AaruConsole.ErrorWriteLine("Tried reading a track that is not present in image");

            return ErrorNumber.SectorNotFound;
        }

        stream.Seek(_trackOffset[trackNum], SeekOrigin.Begin);

        // read the compressed track data
        stream.EnsureRead(blkHeader, 0, 3);
        var  compressedLength = (short)(BitConverter.ToInt16(blkHeader, 0) - 1);
        byte escapeByte       = blkHeader[2];

        var cBuffer = new byte[compressedLength];
        stream.EnsureRead(cBuffer, 0, compressedLength);

        // decompress the data
        var sIndex = 0; // source buffer position
        var dIndex = 0; // destination buffer position

        while(sIndex < compressedLength)
            if(cBuffer[sIndex] == escapeByte)
            {
                sIndex++; // skip over escape byte
                byte fillByte  = cBuffer[sIndex++];
                byte fillCount = cBuffer[sIndex++];

                // fill destination buffer
                for(var i = 0; i < fillCount; i++)
                    trackData[dIndex++] = fillByte;
            }
            else
                trackData[dIndex++] = cBuffer[sIndex++];

        // check that the number of bytes decompressed matches a whole track
        if(dIndex != _imageInfo.SectorSize * _imageInfo.SectorsPerTrack)
        {
            AaruConsole.ErrorWriteLine("Track decompression yielded incomplete data");

            return ErrorNumber.InOutError;
        }

        // store track in cache
        _trackCache[trackNum] = trackData;

        return ErrorNumber.NoError;
    }
}