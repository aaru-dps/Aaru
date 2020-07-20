// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CHD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages MAME Compressed Hunks of Data disk images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;

namespace Aaru.DiscImages
{
    // TODO: Implement PCMCIA support
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public partial class Chd : IOpticalMediaImage, IVerifiableImage
    {
        /// <summary>"MComprHD"</summary>
        readonly byte[] chdTag =
        {
            0x4D, 0x43, 0x6F, 0x6D, 0x70, 0x72, 0x48, 0x44
        };
        SectorBuilder             _sectorBuilder;
        uint                      bytesPerHunk;
        byte[]                    cis;
        byte[]                    expectedChecksum;
        uint                      hdrCompression;
        uint                      hdrCompression1;
        uint                      hdrCompression2;
        uint                      hdrCompression3;
        Dictionary<ulong, byte[]> hunkCache;
        byte[]                    hunkMap;
        ulong[]                   hunkTable;
        uint[]                    hunkTableSmall;
        byte[]                    identify;
        ImageInfo                 imageInfo;
        Stream                    imageStream;
        bool                      isCdrom;
        bool                      isGdrom;
        bool                      isHdd;
        uint                      mapVersion;
        int                       maxBlockCache;
        int                       maxSectorCache;
        Dictionary<ulong, uint>   offsetmap;
        List<Partition>           partitions;
        Dictionary<ulong, byte[]> sectorCache;
        uint                      sectorsPerHunk;
        bool                      swapAudio;
        uint                      totalHunks;
        Dictionary<uint, Track>   tracks;

        public Chd() => imageInfo = new ImageInfo
        {
            ReadableSectorTags    = new List<SectorTagType>(),
            ReadableMediaTags     = new List<MediaTagType>(),
            HasPartitions         = false,
            HasSessions           = false,
            Application           = "MAME",
            Creator               = null,
            Comments              = null,
            MediaManufacturer     = null,
            MediaModel            = null,
            MediaSerialNumber     = null,
            MediaBarcode          = null,
            MediaPartNumber       = null,
            MediaSequence         = 0,
            LastMediaSequence     = 0,
            DriveManufacturer     = null,
            DriveModel            = null,
            DriveSerialNumber     = null,
            DriveFirmwareRevision = null
        };
    }
}