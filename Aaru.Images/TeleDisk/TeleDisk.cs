// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : TeleDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sydex TeleDisk disk images.
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
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    // Created following notes from Dave Dunfield
    // http://www.classiccmp.org/dunfield/img54306/td0notes.txt
    /// <summary>Implements reading of Sydex TeleDisk disk images</summary>
    public sealed partial class TeleDisk : IMediaImage, IVerifiableImage, IVerifiableSectorsImage
    {
        readonly List<ulong> _sectorsWhereCrcHasFailed;
        bool                 _aDiskCrcHasFailed;
        byte[]               _commentBlock;
        CommentBlockHeader   _commentHeader;
        Header               _header;
        ImageInfo            _imageInfo;
        Stream               _inStream;
        byte[]               _leadOut;

        // Cylinder by head, sector data matrix
        byte[][][][] _sectorsData;

        // LBA, data
        uint _totalDiskSize;

        public TeleDisk()
        {
            _imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "Sydex TeleDisk",
                Comments              = null,
                Creator               = null,
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

            _aDiskCrcHasFailed        = false;
            _sectorsWhereCrcHasFailed = new List<ulong>();
        }
    }
}