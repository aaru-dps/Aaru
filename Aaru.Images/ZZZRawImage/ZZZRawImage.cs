// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ZZZRawImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages raw image, that is, user data sector by sector copy.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    /// <summary>
    /// Implements reading and writing raw (sector by sector) images
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed partial class ZZZRawImage : IWritableOpticalImage
    {
        string                           _basePath;
        bool                             _differentTrackZeroSize;
        string                           _extension;
        bool                             _hasSubchannel;
        ImageInfo                        _imageInfo;
        Dictionary<MediaTagType, byte[]> _mediaTags;
        bool                             _mode2;
        bool                             _rawCompactDisc;
        IFilter                          _rawImageFilter;
        FileStream                       _writingStream;

        /// <summary>
        /// Implements reading and writing raw (sector by sector) images
        /// </summary>
        public ZZZRawImage() => _imageInfo = new ImageInfo
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