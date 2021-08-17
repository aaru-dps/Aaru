// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskDupe.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages floppy disk images created with DiskDupe
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
// Copyright © 2021 Michael Drüing
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

/* Some information on the file format from Michal Necasek (www.os2museum.com):
 *
 * The DDI diskette image format was used by the DiskDupe DOS utility,
 * developed by Micro System Designs, Inc. of Cupertino, California in the
 * early 1990. DiskDupe was used to drive floppy autoloaders/duplicators
 * and disk image functionality was a fringe feature.
 *
 * All information about this format was obtained by analyzing available image
 * files and may not be entirely accurate.
 *
 * The DDI (DiskDupe Image) format is very simplistic, only supporting
 * standard PC floppy formats. It was not intended as a generic floppy disk
 * archival format.
 *
 * There is no compression and no checksums, but provisions are made to leave
 * out unused tracks. There is a track map at the beginning of the image.
 * The image header is the same size as a single track; the track map uses
 * this fact and gives the starting offset of each track in units of one
 * track's worth of data (e.g. 7.5KB for 1.2M images).
 *
 * There is a unique signature ('IM' followed by several zeroes), which means
 * that DDI files are easy to recognize.
 */

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

// ReSharper disable NotAccessedField.Local

namespace Aaru.DiscImages
{
    /// <inheritdoc />
    /// <summary>Implements reading DiskDupe disk images</summary>
    public sealed partial class DiskDupe : IMediaImage
    {
        /// <summary>The DDI file header after the image has been opened</summary>
        FileHeader _fileHeader;

        /// <summary>The track map for the image after it has been opened</summary>
        TrackInfo[] _trackMap;

        /// <summary>The track offsets in the image after the file has been opened</summary>
        long[] _trackOffsets;

        /// <summary>The ImageFilter we're reading from, after the file has been opened</summary>
        IFilter _ddiImageFilter;
        ImageInfo _imageInfo;

        public DiskDupe() => _imageInfo = new ImageInfo
        {
            ReadableSectorTags    = new List<SectorTagType>(),
            ReadableMediaTags     = new List<MediaTagType>(),
            HasPartitions         = false,
            HasSessions           = false,
            Version               = null,
            Application           = null,
            ApplicationVersion    = null,
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