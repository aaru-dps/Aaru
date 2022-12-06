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
// Copyright © 2011-2023 Natalia Portillo
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
    /// <summary>Implements reading MAME CHD disk images</summary>
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public sealed partial class Chd : IOpticalMediaImage, IVerifiableImage
    {
        /// <summary>"MComprHD"</summary>
        readonly byte[] _chdTag =
        {
            0x4D, 0x43, 0x6F, 0x6D, 0x70, 0x72, 0x48, 0x44
        };
        SectorBuilder             _sectorBuilder;
        uint                      _bytesPerHunk;
        byte[]                    _cis;
        byte[]                    _expectedChecksum;
        uint                      _hdrCompression;
        uint                      _hdrCompression1;
        uint                      _hdrCompression2;
        uint                      _hdrCompression3;
        Dictionary<ulong, byte[]> _hunkCache;
        byte[]                    _hunkMap;
        ulong[]                   _hunkTable;
        uint[]                    _hunkTableSmall;
        byte[]                    _identify;
        ImageInfo                 _imageInfo;
        Stream                    _imageStream;
        bool                      _isCdrom;
        bool                      _isGdrom;
        bool                      _isHdd;
        uint                      _mapVersion;
        int                       _maxBlockCache;
        int                       _maxSectorCache;
        Dictionary<ulong, uint>   _offsetmap;
        List<Partition>           _partitions;
        Dictionary<ulong, byte[]> _sectorCache;
        uint                      _sectorsPerHunk;
        bool                      _swapAudio;
        uint                      _totalHunks;
        Dictionary<uint, Track>   _tracks;

        public Chd() => _imageInfo = new ImageInfo
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