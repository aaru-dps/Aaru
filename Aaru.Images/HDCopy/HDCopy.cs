// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HDCopy.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages floppy disk images created with HD-Copy
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

/* Some information on the file format from Michal Necasek (www.os2museum.com):
 *
 * The HD-Copy diskette image format was used by the eponymous DOS utility,
 * written by Oliver Fromme around 1995. The HD-Copy format is relatively
 * straightforward, supporting images with 512-byte sector size and uniform
 * sectors per track count. A basic form of run-length compression is also
 * supported, and empty/unused tracks aren't stored in the image. Images
 * with up to 82 cylinders are supported.
 *
 * No provision appears to be made for single-sided images. The disk image
 * is stored as a sequence of compressed tracks (where a track refers to only
 * one side of the disk), and individual tracks may be left out.
 *
 * The HD-Copy RLE compression works as follows. The image is divided into a
 * number of independent blocks, one per track. Each compressed block starts
 * with a header which contains the size of compressed data (16-bit little
 * endian) and the escape byte. Whenever the escape byte is encountered in the
 * byte stream, it is followed by a data byte and a count byte.
 *
 * Note that HD-Copy uses RLE compression for sequences of as few as three
 * bytes, even though that provides no benefit.
 *
 * It would be tempting to perform in-place decompression to save memory.
 * Unfortunately the simplistic RLE algorithm means the encoded data may be
 * larger than the decoded version, with unknown worst case behavior. Hence
 * the compressed data for a sector may not fit into a buffer the size of the
 * uncompressed sector.
 *
 * There is no signature, hence heuristics must be used to identify a HD-Copy
 * diskette image. Fortunately, the HD-Copy header is highly recognizable.
 */

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

// ReSharper disable NotAccessedField.Local

namespace Aaru.DiscImages
{
    /// <summary>
    /// Implements reading HD-Copy disk images
    /// </summary>
    public sealed partial class HdCopy : IMediaImage
    {
        /// <summary>Every track that has been read is cached here</summary>
        readonly Dictionary<int, byte[]> _trackCache = new Dictionary<int, byte[]>();

        /// <summary>The offset in the file where each track starts, or -1 if the track is not present</summary>
        readonly Dictionary<int, long> _trackOffset = new Dictionary<int, long>();
        /// <summary>The HDCP file header after the image has been opened</summary>
        FileHeader _fileHeader;

        /// <summary>The ImageFilter we're reading from, after the file has been opened</summary>
        IFilter _hdcpImageFilter;
        ImageInfo _imageInfo;

        public HdCopy() => _imageInfo = new ImageInfo
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