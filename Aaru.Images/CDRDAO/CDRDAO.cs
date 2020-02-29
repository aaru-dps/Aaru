// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDRDAO.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages cdrdao cuesheets (toc/bin).
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

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    // TODO: Doesn't support compositing from several files
    // TODO: Doesn't support silences that are not in files
    public partial class Cdrdao : IWritableOpticalImage
    {
        IFilter      cdrdaoFilter;
        StreamWriter descriptorStream;
        CdrdaoDisc   discimage;
        ImageInfo    imageInfo;
        Stream       imageStream;
        /// <summary>Dictionary, index is track #, value is TrackFile</summary>
        Dictionary<uint, ulong> offsetmap;
        bool                         separateTracksWriting;
        StreamReader                 tocStream;
        Dictionary<byte, byte>       trackFlags;
        Dictionary<byte, string>     trackIsrcs;
        string                       writingBaseName;
        Dictionary<uint, FileStream> writingStreams;
        List<Track>                  writingTracks;

        public Cdrdao() => imageInfo = new ImageInfo
        {
            ReadableSectorTags = new List<SectorTagType>(), ReadableMediaTags = new List<MediaTagType>(),
            HasPartitions      = true, HasSessions                            = true, Version = null,
            ApplicationVersion = null,
            MediaTitle         = null, Creator = null, MediaManufacturer = null,
            MediaModel         = null,
            MediaPartNumber    = null, MediaSequence = 0, LastMediaSequence = 0,
            DriveManufacturer  = null,
            DriveModel         = null, DriveSerialNumber = null, DriveFirmwareRevision = null
        };
    }
}