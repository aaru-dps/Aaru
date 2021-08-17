// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CisCopy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CisCopy disk images.
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
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    /* This is a very simple format created by a German application called CisCopy, aka CCOPY.EXE, with extension .DCF.
     * First byte indicates the floppy type, limited to standard formats.
     * Indeed if the floppy is not DOS formatted, user must choose from the list of supported formats manually.
     * Next 80 bytes (for 5.25" DD disks) or 160 bytes (for 5.25" HD and 3.5" disks) indicate if a track has been copied
     * or not.
     * It offers three copy methods:
     * a) All, copies all tracks
     * b) FAT, copies all tracks which contain sectors marked as sued by FAT
     * c) "Belelung" similarly to FAT. On some disk tests FAT cuts data, while belelung does not.
     * Finally, next byte indicates compression:
     * 0) No compression
     * 1) Normal compression, algorithm unknown
     * 2) High compression, algorithm unknown
     * Then the data for whole tracks follow.
     */
    /// <inheritdoc />
    /// <summary>
    /// Implements reading and writing CisCopy disk images
    /// </summary>
    public sealed partial class CisCopy : IWritableImage
    {
        byte[]     _decodedDisk;
        ImageInfo  _imageInfo;
        long       _writingOffset;
        FileStream _writingStream;

        public CisCopy() => _imageInfo = new ImageInfo
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